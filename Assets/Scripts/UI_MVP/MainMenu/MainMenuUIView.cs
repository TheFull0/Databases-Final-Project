using System;
using Base_Classes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.MainMenu
{
    // Binds Main_Menu.uxml controls and exposes user intent as events.
    public class MainMenuUIView : ViewBase
    {
        public event Action OnQueueUpClicked;
        public event Action OnCreditsClicked;
        public event Action OnQuitClicked;
        public event Action OnCancelQueueClicked;

        private VisualElement _buttonContainer;
        private VisualElement _queueScreen;

        private Label _playerNameLabel;
        private Label _playerStatsLabel;
        private Label _queueStatusLabel;

        private Button _queueUpButton;
        private Button _creditsButton;
        private Button _quitButton;
        private Button _cancelQueueButton;

        private bool _callbacksRegistered;

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        public void Render(MainMenuUIModel model)
        {
            if (!EnsureInitialized()) return;

            SetQueueVisible(model.IsQueueVisible);
            SetPlayerName(model.PlayerName);
            SetPlayerStatsText(model.PlayerStatsText);
            SetQueueStatus(model.QueueStatusText);
        }

        protected override void OnInitializeUI()
        {
            // One-time cache of UI Toolkit references used by render/update paths.
            _buttonContainer = Root.Q<VisualElement>(UI_Main_Menu.ButtonContainer);
            _queueScreen = Root.Q<VisualElement>(UI_Main_Menu.QueueScreen);

            _playerNameLabel = Root.Q<Label>(UI_Main_Menu.PlayerNameTXT);
            _playerStatsLabel = Root.Q<Label>(UI_Main_Menu.PlayerStatsTXT);
            _queueStatusLabel = Root.Q<Label>(UI_Main_Menu.OtherPlayerName);

            _queueUpButton = Root.Q<Button>(UI_Main_Menu.QueueUpBTN);
            _creditsButton = Root.Q<Button>(UI_Main_Menu.CreditsBTN);
            _quitButton = Root.Q<Button>(UI_Main_Menu.QuitBTN);
            _cancelQueueButton = Root.Q<Button>(UI_Main_Menu.CancelQueueButton);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_callbacksRegistered) return;

            RegisterButton(_queueUpButton, HandleQueueUpClicked, "QueueUpBTN");
            RegisterButton(_creditsButton, HandleCreditsClicked, "CreditsBTN");
            RegisterButton(_quitButton, HandleQuitClicked, "QuitBTN");
            RegisterButton(_cancelQueueButton, HandleCancelQueueClicked, "CancelQueueButton");

            _callbacksRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if (!_callbacksRegistered) return;

            if (_queueUpButton != null) _queueUpButton.clicked -= HandleQueueUpClicked;
            if (_creditsButton != null) _creditsButton.clicked -= HandleCreditsClicked;
            if (_quitButton != null) _quitButton.clicked -= HandleQuitClicked;
            if (_cancelQueueButton != null) _cancelQueueButton.clicked -= HandleCancelQueueClicked;

            _callbacksRegistered = false;
        }

        private static void RegisterButton(Button button, Action callback, string buttonName)
        {
            if (button == null)
            {
                Debug.LogError($"[MainMenuUIView] Could not find Button named '{buttonName}' in Main_Menu.");
                return;
            }

            button.clicked += callback;
        }

        private void SetQueueVisible(bool isVisible)
        {
            if (_queueScreen != null)
                _queueScreen.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetPlayerName(string playerName)
        {
            if (_playerNameLabel != null)
                _playerNameLabel.text = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName;
        }

        private void SetQueueStatus(string statusText)
        {
            if (_queueStatusLabel != null)
                _queueStatusLabel.text = statusText;
        }
        
        private void SetPlayerStatsText(string playerStatsText)
        {
            if (_playerStatsLabel != null)
                _playerStatsLabel.text = string.IsNullOrWhiteSpace(playerStatsText) ? "Loading stats..." : playerStatsText;
        }

        private void HandleQueueUpClicked()
        {
            OnQueueUpClicked?.Invoke();
        }

        private void HandleCreditsClicked()
        {
            OnCreditsClicked?.Invoke();
        }

        private void HandleQuitClicked()
        {
            OnQuitClicked?.Invoke();
        }

        private void HandleCancelQueueClicked()
        {
            OnCancelQueueClicked?.Invoke();
        }
    }
}
