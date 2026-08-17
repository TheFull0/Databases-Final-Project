using System.Collections.Generic;
using Events;
using Managers;
using UnityEngine;

namespace BootStraps
{
    public class QueueManager : MonoBehaviour
    {
        public static QueueManager Instance { get; private set; }
        [SerializeField] private GameManager gameManagerPrefab;
        private GameManager _gameManagerInstance;
        private bool _initialized;

        private string _playerName;
        public string PlayerName => _playerName;
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogError("[QueueManager] Duplicate instance detected. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }
            
            Instance = this;
            EventBus.Subscribe<MainMenuQueueUpClickedEvent>(HandleMainMenuQueueUpClicked);
            EventBus.Subscribe<MainMenuQueueCanceledEvent>(HandleMainMenuQueueCanceled);
            EventBus.Subscribe<ChooseNameConfirmedEvent>(OnNameChosen);
            EventBus.Subscribe<MatchEndedEvent>(HandleGameFinished);
        }

        private void HandleGameFinished(MatchEndedEvent e)
        {
            if (!_gameManagerInstance || !_initialized) return;
            Destroy(_gameManagerInstance.gameObject);
            _gameManagerInstance = null;
            _initialized = false;
        }

        private void OnNameChosen(ChooseNameConfirmedEvent e)
        {
            _playerName = e.ConfirmedName;
        }

        private void HandleMainMenuQueueCanceled(MainMenuQueueCanceledEvent obj)
        {
            if (!_gameManagerInstance || !_initialized) return;
            if (!_gameManagerInstance.IsAbortable) return;

            _gameManagerInstance.CancelMatchmaking();
            Destroy(_gameManagerInstance.gameObject);
            _gameManagerInstance = null;
            _initialized = false;
        }

        private void HandleMainMenuQueueUpClicked(MainMenuQueueUpClickedEvent evt)
        {
            if (_initialized) return;

            _gameManagerInstance = Instantiate(gameManagerPrefab);
            DontDestroyOnLoad(_gameManagerInstance.gameObject);
            _initialized = true;
            _gameManagerInstance.Run(_playerName);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            
            EventBus.Unsubscribe<MainMenuQueueUpClickedEvent>(HandleMainMenuQueueUpClicked);
            EventBus.Unsubscribe<MainMenuQueueCanceledEvent>(HandleMainMenuQueueCanceled);
            EventBus.Unsubscribe<ChooseNameConfirmedEvent>(OnNameChosen);
            EventBus.Unsubscribe<MatchEndedEvent>(HandleGameFinished);
        }
    }
}