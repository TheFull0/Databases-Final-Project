using Base_Classes;
using Events;
using UnityEngine;

namespace UI.WinScreen
{
    public class WinScreenUIPresenter : IPresenter
    {
        private readonly WinScreenUIModel _model;
        private readonly WinScreenUIView _view;
        private bool _isSubscribed;

        public WinScreenUIPresenter(WinScreenUIModel model, WinScreenUIView view)
        {
            _model = model;
            _view = view;
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed) return;

            _view.OnMainMenuClicked += HandleMainMenuClicked;
            EventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);

            _isSubscribed = true;
            _view.Render(_model);
        }

        public void UnSubscribeToEvents()
        {
            if (!_isSubscribed) return;

            _view.OnMainMenuClicked -= HandleMainMenuClicked;
            EventBus.Unsubscribe<MatchEndedEvent>(OnMatchEnded);

            _isSubscribed = false;
        }

        private void OnMatchEnded(MatchEndedEvent evt)
        {
            _model.SetWinnerText(evt.PlayerName);
            _view.Render(_model);

            if (!UIManager.Instance)
            {
                Debug.LogError("[WinScreenUIPresenter] UIManager instance is unavailable.");
                return;
            }

            UIManager.Instance.SetCurrentScreen(UIScreenId.WinScreen);
        }

        private void HandleMainMenuClicked()
        {
            EventBus.Raise(new WinScreenMainMenuClickedEvent());
        }
    }
}
