using System.Collections;
using System.Collections.Generic;
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
        
        private PlayerData _playerData;
        private Question _currentQuestion;
        private int _currentQuestionIndex = -1;
        private bool _questionsLoaded;

        private async void Awake()  
        {
            var questions = await ServerFunctions.GetRandomQuestions(gameData.QuestionCount);
            
            if (questions == null)
            {
                Debug.LogError("[GameManager] Failed to retrieve questions from the server.");
                return;
            }

            CopyQuestionsToList(questions);

            SubscribeToEvents();
            
            StartGame();
        }

        private void StartGame()
        {
            EventBus.Raise(new GameStartedEvent { PlayerData = _playerData });
            LoadNextQuestion();
        }

        private void CopyQuestionsToList(IEnumerable<Question> questions)
        {
            foreach (var question in questions)
            {
                _questions.Add(question);
            }
            _questionsLoaded = true;
        }

        public void InitializeGame(PlayerData enemyPlayerData)
        {
            _playerData = enemyPlayerData;
            _currentQuestionIndex = -1;
        }

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
            stopWatch.Start(gameData.TimePerQuestion);
            
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

        private void EndGame()
        {
            // Handle end of game logic here
        }


        private void OnDestroy()
        {
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