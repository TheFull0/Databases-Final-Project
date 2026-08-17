using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Structs;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Managers
{
    public static class ServerFunctions
    {
        private const string ServerUrl = "https://localhost:7208/api";
        private const string PersistentPlayerIdPrefKey = "PersistentLocalPlayerId";

        // Set once JoinAndWaitForOpponent completes - the UI reads these to
        // show the matchmaking / result screens.
        public static int PlayerId { get; private set; }
        public static int MatchId { get; private set; }
        public static int MyElo { get; private set; }
        public static string OpponentName { get; private set; } = "";
        public static int OpponentElo { get; private set; }

        // ---------------------------------------------------------------
        // Data contracts
        // ---------------------------------------------------------------

        [Serializable]
        private class JoinRequestBody
        {
            public string persistentPlayerId;
            public string name;
        }
        
        [Serializable]
        private class PlayerProfileDto
        {
            public string persistentPlayerId;
            public string name;
            public int elo;
        }

        [Serializable]
        public class QuestionDto
        {
            public int id;
            public string text;
            public string ans1, ans2, ans3, ans4;
        }

        [Serializable]
        private class QuestionListWrapper
        {
            public List<QuestionDto> items;
        }

        [Serializable]
        public class JoinResultDto
        {
            public bool matched;
            public int playerId;
            public int matchId; // 0 when not yet matched
            public string persistentPlayerId;
            public int myElo;
            public string opponentName;
            public int opponentElo;
        }

        [Serializable]
        public class PlayerResultDto
        {
            public int playerId;
            public string name;
            public int correctCount;
            public int totalTimeMs;
            public int eloChange;
            public int newElo;
        }

        [Serializable]
        public class MatchResultDto
        {
            public int matchId;
            public bool completed;
            public int winnerId; // 0 when it's a tie - check isTie first
            public bool isTie;
            public List<PlayerResultDto> players;
        }
        
        [Serializable]
        private class SubmitScoreRequestBody
        {
            public int playerId;
            public int correctCount;
            public int totalTimeMs;
        }

        // ---------------------------------------------------------------
        // Matchmaking
        // ---------------------------------------------------------------

        // Awaits until an opponent is actually paired (matched by closest
        // Elo). Show a "waiting for opponent" screen while this runs - it
        // can take anywhere from instant (someone was already waiting) to
        // indefinite (no one else has opened the game yet), so pass a token
        // your UI can cancel if the player backs out.
        // Reads/writes the persistent player id from PlayerPrefs itself -
        // callers just pass the name the player typed this session.
        public static async Task JoinAndWaitForOpponent(string playerName, CancellationToken token)
        {
            var cachedId = PlayerPrefs.GetString(PersistentPlayerIdPrefKey, "");

            var joinBody = JsonUtility.ToJson(new JoinRequestBody
            {
                persistentPlayerId = string.IsNullOrEmpty(cachedId) ? null : cachedId,
                name = playerName
            });

            using (var req = new UnityWebRequest($"{ServerUrl}/match/join", "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(joinBody));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[ServerFunctions] Join failed: {req.error}");
                    return;
                }

                var result = JsonUtility.FromJson<JoinResultDto>(req.downloadHandler.text);
                ApplyJoinResult(result);

                if (result.matched) return;
            }

            // No one was waiting - poll until someone with a comparable Elo joins.
            while (!token.IsCancellationRequested)
            {
                using var req = UnityWebRequest.Get($"{ServerUrl}/match/status/{PlayerId}");
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var status = JsonUtility.FromJson<JoinResultDto>(req.downloadHandler.text);
                    if (status.matched)
                    {
                        ApplyJoinResult(status);
                        return;
                    }
                }

                await Task.Delay(1500, token);
            }
        }

        private static void ApplyJoinResult(JoinResultDto result)
        {
            PlayerId = result.playerId;
            MyElo = result.myElo;

            // Save (or re-save) the persistent id every time - covers both
            // "first launch, server just minted one" and "server confirmed
            // the cached one is still valid".
            if (!string.IsNullOrEmpty(result.persistentPlayerId))
            {
                PlayerPrefs.SetString(PersistentPlayerIdPrefKey, result.persistentPlayerId);
                PlayerPrefs.Save();
            }

            if (result.matched)
            {
                MatchId = result.matchId;
                OpponentName = result.opponentName;
                OpponentElo = result.opponentElo;
            }
        }
        
        // Call this from the menu, before any matchmaking - reads the
        // cached persistent id and populates MyElo without joining a match.
        // If this id has never played, MyElo is set to the same starting
        // value the server would give it (no network call needed).
        public static async Task RefreshMyProfile()
        {
            var cachedId = PlayerPrefs.GetString(PersistentPlayerIdPrefKey, "");
            if (string.IsNullOrEmpty(cachedId))
            {
                MyElo = 1000;
                return;
            }

            using var req = UnityWebRequest.Get($"{ServerUrl}/players/{cachedId}");
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                // 404 means "valid id, never played yet" - not a real error.
                if (req.responseCode != 404)
                    Debug.LogError($"[ServerFunctions] RefreshMyProfile failed: {req.error}");
                MyElo = 1000;
                return;
            }

            var profile = JsonUtility.FromJson<PlayerProfileDto>(req.downloadHandler.text);
            MyElo = profile.elo;
        }

        // ---------------------------------------------------------------
        // Questions
        // ---------------------------------------------------------------

        public static async Task<IEnumerable<Question>> GetRandomQuestions(int count)
        {
            using var req = UnityWebRequest.Get($"{ServerUrl}/questions");
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ServerFunctions] GetRandomQuestions failed: {req.error}");
                return null;
            }
            Debug.Log($"[ServerFunctions] GetRandomQuestions response - Success");

            var wrapped = "{\"items\":" + req.downloadHandler.text + "}";
            var all = JsonUtility.FromJson<QuestionListWrapper>(wrapped).items;

            return all.OrderBy(_ => Random.value).Take(count).Select(ToGameQuestion);
        }

        // The server always puts the correct answer in ans1. This shuffles
        // the four options for display and tracks where the correct one
        // ended up, matching Question's CorrectAnswerIndex.
        private static Question ToGameQuestion(QuestionDto dto)
        {
            var options = new[] { dto.ans1, dto.ans2, dto.ans3, dto.ans4 };
            var order = Enumerable.Range(0, 4).OrderBy(_ => Random.value).ToArray();

            var shuffledOptions = order.Select(originalIndex => options[originalIndex]).ToArray();
            var correctIndex = Array.IndexOf(order, 0); // where ans1 ended up

            return new Question(dto.text, shuffledOptions, correctIndex);
        }

        // ---------------------------------------------------------------
        // Scoring
        // ---------------------------------------------------------------

        // Call once, right as EndGame() runs, with this player's own tally
        // built from GameManager's local _questionResults list.
        public static async Task SubmitMatchResult(int correctAnswers, float totalAnswerTimeSeconds)
        {
            var body = JsonUtility.ToJson(new SubmitScoreRequestBody
            {
                playerId = PlayerId,
                correctCount = correctAnswers,
                totalTimeMs = Mathf.RoundToInt(totalAnswerTimeSeconds * 1000f)
            });

            using var req = new UnityWebRequest($"{ServerUrl}/match/submit-score", "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[ServerFunctions] SubmitMatchResult failed: {req.error}");
        }

        // Call right after SubmitMatchResult. Awaits until the opponent has
        // also finished and the server has settled both players' Elo -
        // show a "waiting for opponent to finish" screen while this runs.
        public static async Task<MatchOutcome?> WaitForMatchOutcome(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var req = UnityWebRequest.Get($"{ServerUrl}/match/result/{PlayerId}");
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var result = JsonUtility.FromJson<MatchResultDto>(req.downloadHandler.text);
                    if (result.completed)
                    {
                        var me = result.players.First(p => p.playerId == PlayerId);
                        var opponent = result.players.First(p => p.playerId != PlayerId);

                        MyElo = me.newElo;

                        return new MatchOutcome
                        {
                            IsTie = result.isTie,
                            IsWin = !result.isTie && result.winnerId == PlayerId,
                            MyCorrectAnswers = me.correctCount,
                            OpponentCorrectAnswers = opponent.correctCount,
                            OpponentTotalAnswerTime = opponent.totalTimeMs / 1000f,
                            MyEloChange = me.eloChange,
                            MyNewElo = me.newElo
                        };
                    }
                }
                else
                {
                    Debug.LogError($"[ServerFunctions] WaitForMatchOutcome failed: {req.error}");
                    return null;
                }

                await Task.Delay(1500, token);
            }
            return null;
        }
    }
}
