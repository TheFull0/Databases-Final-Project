using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Events;
using Game_Logic;
using ScriptableObjects.Classes;
using Structs;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameDataSO gameData;
        [SerializeField] private StopWatch stopWatch;
        
        private List<Question> _questions = new List<Question>();
        private List<QuestionResult> _questionResults = new List<QuestionResult>();
        
        private Question _currentQuestion;
        private int _currentQuestionIndex = -1;
        private bool _questionsLoaded;
        private CancellationTokenSource _matchmakingCancellationTokenSource;
        
        public bool IsAbortable { get; private set; }

        private async void Awake()
        {
            _matchmakingCancellationTokenSource = new CancellationTokenSource();
            var matchmakingToken = _matchmakingCancellationTokenSource.Token;
            IsAbortable = true;

            try
            {
                SubscribeToEvents();
            
                //TODO Make a name-entry UI and raise some event or add it straight to gamemanager and read the value when submit clicked or something
                const string playerName = "";
             
                //TODO raise some "waiting for opponent" UI event here or show it straight from uimanager
                await ServerFunctions.JoinAndWaitForOpponent(playerName, matchmakingToken);

                if (matchmakingToken.IsCancellationRequested)
                {
                    return;
                }

                IsAbortable = false;
             
                var questions = await ServerFunctions.GetRandomQuestions(gameData.QuestionCount);
            
                if (questions == null)
                {
                    Debug.LogError("[GameManager] Failed to retrieve questions from the server.");
                    return;
                }

                CopyQuestionsToList(questions);
                StartGame();
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{GetType().Name}] Matchmaking queue was canceled.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void StartGame()
        {
            IsAbortable = false;
            LoadNextQuestion();
        }

        public void CancelMatchmaking()
        {
            if (!IsAbortable) return;

            IsAbortable = false;
            _matchmakingCancellationTokenSource?.Cancel();
        }

        private void CopyQuestionsToList(IEnumerable<Question> questions)
        {
            foreach (var question in questions)
            {
                _questions.Add(question);
                Debug.Log($"[{GetType().Name}] Question: {question}");
            }
            _questionsLoaded = true;
        }

        // there is no need for this, there's no opponent identity available before the match ends so this hook doesn't have anything to be called with.
        /*public void InitializeGame(PlayerData enemyPlayerData)
        {
            _playerData = enemyPlayerData;
            _currentQuestionIndex = -1;
        }*/

        private void LoadNextQuestion()
        {
            if (!_questionsLoaded || _questions.Count == 0)
            {
                Debug.LogWarning("[GameManager] No questions loaded or available.");
                return;
            }

            _currentQuestionIndex++;
            if (_currentQuestionIndex >= _questions.Count)
            {
                Debug.Log("[GameManager] All questions have been answered.");
                EndGame();
                return;
            }

            _currentQuestion = _questions[_currentQuestionIndex];
            EventBus.Raise(new NewQuestionLoadedEvent(_currentQuestion));
            stopWatch.StartClock(gameData.TimePerQuestion);
            
        }

        private void OnQuestionAnswered(QuestionAnsweredEvent e)
        {
            StartCoroutine(OnQuestionAnsweredRoutine(e));
        }

        private IEnumerator OnQuestionAnsweredRoutine(QuestionAnsweredEvent e)
        {
            var timeTaken = stopWatch.StopAndPeek(); //gets elapsed time and stops the count
            var wasAnsweredCorrectly = _currentQuestion.CorrectAnswerIndex == e.AnswerIndex;
            
            _questionResults.Add(new QuestionResult(wasAnsweredCorrectly, timeTaken));
            
            
            if (wasAnsweredCorrectly)
            {
                EventBus.Raise(new QuestionAnsweredCorrectlyEvent {});
            }
            else
            {
                EventBus.Raise(new QuestionAnsweredIncorrectlyEvent { AnswerIndex = _currentQuestion.CorrectAnswerIndex });
            }
            
            
            yield return new WaitForSeconds(gameData.WaitBetweenQuestions); // Wait for x seconds before loading the next question
            
            LoadNextQuestion();
        }
        
        private void OnTimerFinished(TimerFinishedEvent e)
        {
            EventBus.Raise(new QuestionUnansweredEvent { AnswerIndex = _currentQuestion.CorrectAnswerIndex });
        }

        private async void EndGame()
        {
            try
            {
                var correct = _questionResults.Count(r => r.WasAnsweredCorrectly);
                var totalTime = _questionResults.Sum(r => r.TimeTaken);

                // raise a "waiting for opponent to finish" UI event here
                await ServerFunctions.SubmitMatchResult(correct, totalTime);
                
                //TODO Make a "Waiting for opponent to finish" UI and raise some event
                var outcome = await ServerFunctions.WaitForMatchOutcome(CancellationToken.None);

                if (outcome.HasValue)
                {
                    // Success! Raise a "show winner screen" event here
                    // you have a tie or a winner if there's a tie the winners id will be 0. but isTie will be true
                }
                else
                {
                    //The polling loop encountered a network error and returned null
                    //Lost connection to the server while waiting for opponent.
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{GetType().Name}] Match polling was canceled because the object was destroyed.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GetType().Name}] An unexpected error occurred: {e.Message}");
            }
        }


        private void OnDestroy()
        {
            IsAbortable = false;
            _matchmakingCancellationTokenSource?.Cancel();
            _matchmakingCancellationTokenSource?.Dispose();
            _matchmakingCancellationTokenSource = null;
            UnsubscribeAll();
        }
        
        private void SubscribeToEvents()
        {
            EventBus.Subscribe<QuestionAnsweredEvent>(OnQuestionAnswered);
            EventBus.Subscribe<TimerFinishedEvent>(OnTimerFinished);
        }

        

        private void UnsubscribeAll()
        {
            EventBus.Unsubscribe<QuestionAnsweredEvent>(OnQuestionAnswered);
            EventBus.Unsubscribe<TimerFinishedEvent>(OnTimerFinished);
        }
        
    }
}