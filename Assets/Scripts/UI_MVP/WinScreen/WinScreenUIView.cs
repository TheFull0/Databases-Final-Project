using System;
using Base_Classes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.WinScreen
{
    public class WinScreenUIView : ViewBase
    {
        public event Action OnMainMenuClicked;

        private Label _playerNameLabel;
        private Button _mainMenuButton;
        private bool _callbacksRegistered;

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        public void Render(WinScreenUIModel model)
        {
            if (!EnsureInitialized()) return;
            if (_playerNameLabel == null) return;

            _playerNameLabel.text = string.IsNullOrWhiteSpace(model.WinnerText)
                ? "Player"
                : model.WinnerText;
        }

        protected override void OnInitializeUI()
        {
            _playerNameLabel = Root.Q<Label>(UI_Win_Screen.PlayerNameTXT);
            _mainMenuButton = Root.Q<Button>(UI_Win_Screen.ToTitleScreenBTN);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_callbacksRegistered) return;

            if (_mainMenuButton == null)
            {
                Debug.LogError("[WinScreenUIView] Could not find Button named 'ToTitleScreenBTN' in Win_Screen.");
                return;
            }

            _mainMenuButton.clicked += HandleMainMenuClicked;
            _callbacksRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if (!_callbacksRegistered) return;

            if (_mainMenuButton != null)
            {
                _mainMenuButton.clicked -= HandleMainMenuClicked;
            }

            _callbacksRegistered = false;
        }

        private void HandleMainMenuClicked()
        {
            OnMainMenuClicked?.Invoke();
        }
    }
}
