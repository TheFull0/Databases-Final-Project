using System;
using System.Collections;
using Base_Classes;
using Events;
using UnityEngine;
using UnityEngine.Networking;

namespace UI_MVP.ChooseName
{
    public class ChooseNameUIModel : ModelBase
    {
        private const string RandomNameApi = "https://randomuser.me/api/?inc=login";
        private const string PlayerNamePrefKey = "PlayerName";
        private const int MaxNameLength = 32;

        //Fallback
        private static readonly string[] RandomAdjectives =
            { "Swift", "Brave", "Silent", "Crimson", "Lucky", "Shadow", "Iron", "Golden", "Wild", "Frost" };
        private static readonly string[] RandomNouns =
            { "Wolf", "Falcon", "Ranger", "Viper", "Knight", "Ghost", "Comet", "Tiger", "Raven", "Blade" };

        public string ConfirmedName { get; private set; } = string.Empty;

        public void SaveConfirmedName(string name)
        {
            ConfirmedName = name ?? string.Empty;

            PlayerPrefs.SetString(PlayerNamePrefKey, ConfirmedName);
            PlayerPrefs.Save();

            EventBus.Raise(new ChooseNameConfirmedEvent(ConfirmedName));
        }

        public bool TryApplyConfirmedName(string confirmedName)
        {
            if (string.IsNullOrWhiteSpace(confirmedName))
                return false;

            var trimmedName = confirmedName.Trim();
            if (trimmedName.Length > MaxNameLength)
                trimmedName = trimmedName.Substring(0, MaxNameLength);

            if (IsNameAlreadyTaken(trimmedName))
                return false;

            SaveConfirmedName(trimmedName);
            return true;
        }

        public string GetCurrentNetworkDisplayName()
        {
            if (!string.IsNullOrEmpty(ConfirmedName))
                return ConfirmedName;

            return PlayerPrefs.GetString(PlayerNamePrefKey, string.Empty);
        }

        public bool IsNameAlreadyTaken(string name)
        {
            // TODO: wire this up to the real matchmaking/network player registry
            // once one exists. Until then, no name is considered taken.
            return false;
        }

        public IEnumerator FetchRandomNameRoutine(Action<string> onComplete)
        {
            using var req = UnityWebRequest.Get(RandomNameApi);
            yield return req.SendWebRequest();

            string name = null;
            if (req.result == UnityWebRequest.Result.Success)
                name = ParseUsername(req.downloadHandler.text);
            else
                Debug.LogWarning($"[ChooseNameUIModel] Random name request failed ({req.result}); using local fallback.");

            if (string.IsNullOrEmpty(name))
                name = LocalRandomName();

            if (name.Length > MaxNameLength)
                name = name.Substring(0, MaxNameLength);

            if (IsNameAlreadyTaken(name))
                name = LocalRandomName();

            onComplete?.Invoke(name);
        }

        private static string ParseUsername(string json)
        {
            try
            {
                var parsed = JsonUtility.FromJson<RandomUserResponse>(json);
                if (parsed?.results != null && parsed.results.Length > 0 && parsed.results[0].login != null)
                    return parsed.results[0].login.username;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ChooseNameUIModel] Failed to parse random name: {e.Message}");
            }
            return null;
        }

        private static string LocalRandomName() =>
            $"{RandomAdjectives[UnityEngine.Random.Range(0, RandomAdjectives.Length)]}" +
            $"{RandomNouns[UnityEngine.Random.Range(0, RandomNouns.Length)]}" +
            $"{UnityEngine.Random.Range(1, 1000)}";

        [Serializable] private class RandomUserResponse { public RandomUser[] results; }
        [Serializable] private class RandomUser { public Login login; }
        [Serializable] private class Login { public string username; }
    }
}