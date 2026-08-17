using System;
using System.Collections;
using Base_Classes;
using UnityEngine;
using UnityEngine.Networking;

namespace UI_MVP.ChooseName
{
    public class ChooseNameUIModel: ModelBase
    {
        private const string RandomNameApi = "https://randomuser.me/api/?inc=login";

        //Fallback
        private static readonly string[] RandomAdjectives =
            { "Swift", "Brave", "Silent", "Crimson", "Lucky", "Shadow", "Iron", "Golden", "Wild", "Frost" };
        private static readonly string[] RandomNouns =
            { "Wolf", "Falcon", "Ranger", "Viper", "Knight", "Ghost", "Comet", "Tiger", "Raven", "Blade" };

        public void SaveConfirmedName(string name)
        {
            
        }

        public bool TryApplyConfirmedName(string confirmedName)
        {
            
        }

        public string GetCurrentNetworkDisplayName()
        {
            
        }

        public bool IsNameAlreadyTaken(string name)
        {
            
        }

        public IEnumerator FetchRandomNameRoutine(Action<string> onComplete)
        {
            using var req = UnityWebRequest.Get(RandomNameApi);
            yield return req.SendWebRequest();

            string name = null;
            if (req.result == UnityWebRequest.Result.Success)
                name = ParseUsername(req.downloadHandler.text);
            else
                Debug.LogWarning($"[NameEntryUIModel] Random name request failed ({req.result}); using local fallback.");

            if (string.IsNullOrEmpty(name))
                name = LocalRandomName();

            if (name.Length > 32)
                name = name.Substring(0, 32);

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
                Debug.LogWarning($"[NameEntryUIModel] Failed to parse random name: {e.Message}");
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