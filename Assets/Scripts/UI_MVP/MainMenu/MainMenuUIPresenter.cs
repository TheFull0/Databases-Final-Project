using Base_Classes;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu
{
    // Coordinates Main Menu view events, model updates, and app-level side effects.
    public class MainMenuUIPresenter : IPresenter
    {
        private readonly MainMenuUIModel _model;
        private readonly MainMenuUIView _view;

        private bool _isSubscribed;

        public MainMenuUIPresenter(
            MainMenuUIModel model,
            MainMenuUIView view)
        {
            _model = model;
            _view = view;
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed) return;

            _view.OnQueueUpClicked += HandleQueueUpClicked;
            _view.OnCancelQueueClicked += HandleCancelQueueClicked;
            _view.OnCreditsClicked += HandleCreditsClicked;
            _view.OnQuitClicked += HandleQuitClicked;

            EventBus.Subscribe<MainMenuQueueStateChangedEvent>(OnQueueStateChanged);
            EventBus.Subscribe<MainMenuPlayerNameUpdatedEvent>(OnPlayerNameUpdated);
            EventBus.Subscribe<MainMenuQueueStatusUpdatedEvent>(OnQueueStatusUpdated);

            _isSubscribed = true;
            InitializeView();
        }

        public void UnSubscribeToEvents()
        {
            if (!_isSubscribed) return;

            _view.OnQueueUpClicked -= HandleQueueUpClicked;
            _view.OnCancelQueueClicked -= HandleCancelQueueClicked;
            _view.OnCreditsClicked -= HandleCreditsClicked;
            _view.OnQuitClicked -= HandleQuitClicked;

            EventBus.Unsubscribe<MainMenuQueueStateChangedEvent>(OnQueueStateChanged);
            EventBus.Unsubscribe<MainMenuPlayerNameUpdatedEvent>(OnPlayerNameUpdated);
            EventBus.Unsubscribe<MainMenuQueueStatusUpdatedEvent>(OnQueueStatusUpdated);

            _isSubscribed = false;
        }

        private void InitializeView()
        {
            // Main menu starts with the queue panel hidden.
            _model.SetQueueVisible(false);
            _view.Render(_model);
        }

        private void HandleQueueUpClicked()
        {
            _model.SetQueueVisible(true);
            _view.Render(_model);

            EventBus.Raise(new MainMenuQueueUpClickedEvent());
        }

        private void HandleCancelQueueClicked()
        {
            _model.SetQueueVisible(false);
            _view.Render(_model);

            EventBus.Raise(new MainMenuQueueCanceledEvent());
        }

        private void HandleCreditsClicked()
        {
            EventBus.Raise(new MainMenuCreditsClickedEvent());
        }

        private void HandleQuitClicked()
        {
            EventBus.Raise(new MainMenuQuitClickedEvent());

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnQueueStateChanged(MainMenuQueueStateChangedEvent e)
        {
            _model.SetQueueVisible(e.IsInQueue);
            _view.Render(_model);
        }

        private void OnPlayerNameUpdated(MainMenuPlayerNameUpdatedEvent e)
        {
            _model.SetPlayerName(e.PlayerName);
            _view.Render(_model);
        }

        private void OnQueueStatusUpdated(MainMenuQueueStatusUpdatedEvent e)
        {
            _model.SetQueueStatus(e.StatusText);
            _view.Render(_model);
        }
    }
}
