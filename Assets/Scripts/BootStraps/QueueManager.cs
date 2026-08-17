using System.Collections.Generic;
using Events;
using Managers;
using UnityEngine;

namespace BootStraps
{
    public class QueueManager : MonoBehaviour
    {
        [SerializeField] private QueueManager Instance;
        [SerializeField] private GameManager gameManagerPrefab;
        private GameManager _gameManagerInstance;
        private bool _initialized;

        private string _playerName;
        
        private void Awake()
        {
            EventBus.Subscribe<MainMenuQueueUpClickedEvent>(HandleMainMenuQueueUpClicked);
            EventBus.Subscribe<MainMenuQueueCanceledEvent>(HandleMainMenuQueueCanceled);
            EventBus.Subscribe<ChooseNameConfirmedEvent>(OnNameChosen);
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
            EventBus.Unsubscribe<MainMenuQueueUpClickedEvent>(HandleMainMenuQueueUpClicked);
            EventBus.Unsubscribe<MainMenuQueueCanceledEvent>(HandleMainMenuQueueCanceled);
            EventBus.Unsubscribe<ChooseNameConfirmedEvent>(OnNameChosen);
        }
    }
}