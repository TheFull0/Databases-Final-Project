using System.Threading.Tasks;
using Base_Classes;
using BootStraps;
using Events;
using Managers;
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
            EventBus.Subscribe<GameStartedEvent>(HideQueueWindow);
            EventBus.Subscribe<MainMenuQueueStatusUpdatedEvent>(OnQueueStatusUpdated);
            EventBus.Subscribe<ChooseNameConfirmedEvent>(OnChooseNameConfirmed);
            EventBus.Subscribe<MatchEndedEvent>(OnChooseNameConfirmed);

            _isSubscribed = true;
            InitializeView();
        }

        private void HideQueueWindow(GameStartedEvent obj)
        {
            _model.SetQueueVisible(false);
            _view.Render(_model);
        }

        public void UnSubscribeToEvents()
        {
            if (!_isSubscribed) return;

            _view.OnQueueUpClicked -= HandleQueueUpClicked;
            _view.OnCancelQueueClicked -= HandleCancelQueueClicked;
            _view.OnCreditsClicked -= HandleCreditsClicked;
            _view.OnQuitClicked -= HandleQuitClicked;

            EventBus.Unsubscribe<MainMenuQueueStateChangedEvent>(OnQueueStateChanged);
            EventBus.Unsubscribe<GameStartedEvent>(HideQueueWindow);
            EventBus.Unsubscribe<MainMenuQueueStatusUpdatedEvent>(OnQueueStatusUpdated);
            EventBus.Unsubscribe<ChooseNameConfirmedEvent>(OnChooseNameConfirmed);
            EventBus.Unsubscribe<MatchEndedEvent>(OnChooseNameConfirmed);

            _isSubscribed = false;
        }

        private void InitializeView()
        {
            // Main menu starts with the queue panel hidden.
            _model.SetQueueVisible(false);
            _ = RefreshPlayerIdentityFromCaches();
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

        private void OnQueueStatusUpdated(MainMenuQueueStatusUpdatedEvent e)
        {
            _model.SetQueueStatus(e.StatusText);
            _view.Render(_model);
        }

        private async void OnChooseNameConfirmed<T>(T _)
        {
            await RefreshPlayerIdentityFromCaches();
            _view.Render(_model);
        }

        private async Task RefreshPlayerIdentityFromCaches()
        {
            var cachedName = QueueManager.Instance ? QueueManager.Instance.PlayerName : string.Empty;
            _model.SetPlayerName(cachedName);
            
            await ServerFunctions.RefreshMyProfile();

            if (ServerFunctions.MyElo >= 0)
            {
                _model.SetPlayerStatsText($"{ServerFunctions.CachedID}\n{cachedName} - {ServerFunctions.MyElo}");
            }
            else
            {
                _model.SetPlayerStatsText("Couldn't Fetch Stats!");
            }
        }
    }
}
