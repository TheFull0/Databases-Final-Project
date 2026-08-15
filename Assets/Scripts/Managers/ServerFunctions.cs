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
        private const string ServerUrl = "https://https://localhost:7208//api";

        // Set once JoinAndWaitForOpponent completes; every later call needs these.
        public static int PlayerId { get; private set; }
        public static int MatchId { get; private set; }

        // ---------------------------------------------------------------
        // Data contracts
        // ---------------------------------------------------------------

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
        }

        [Serializable]
        public class PlayerResultDto
        {
            public int playerId;
            public string name;
            public int correctCount;
            public int totalTimeMs;
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

        [Serializable] private class JoinRequestBody { public string name; }

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

        // Awaits until an opponent is actually paired. Show a "waiting for
        // opponent" screen while this runs - it can take anywhere from
        // instant (someone was already waiting) to indefinite (no one else
        // has opened the game yet).
        public static async Task JoinAndWaitForOpponent(string playerName, CancellationToken token)
        {
            var joinBody = JsonUtility.ToJson(new JoinRequestBody { name = playerName });
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
                PlayerId = result.playerId;

                if (result.matched)
                {
                    MatchId = result.matchId;
                    return;
                }
            }

            // No one was waiting - poll until someone joins.
            while (!token.IsCancellationRequested)
            {
                using var req = UnityWebRequest.Get($"{ServerUrl}/match/status/{PlayerId}");
                await req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var status = JsonUtility.FromJson<JoinResultDto>(req.downloadHandler.text);
                    if (status.matched)
                    {
                        MatchId = status.matchId;
                        return;
                    }
                }

                await Task.Delay(1500, token);
            }
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
        // also finished and the server has decided the winner - show a
        // "waiting for opponent to finish" screen while this runs.
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

                        return new MatchOutcome
                        {
                            IsTie = result.isTie,
                            IsWin = !result.isTie && result.winnerId == PlayerId,
                            MyCorrectAnswers = me.correctCount,
                            OpponentCorrectAnswers = opponent.correctCount,
                            OpponentTotalAnswerTime = opponent.totalTimeMs / 1000f
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