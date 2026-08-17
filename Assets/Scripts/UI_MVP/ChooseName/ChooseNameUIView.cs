using System;
using Base_Classes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_MVP.ChooseName
{
    namespace UI_MVP.ChooseName
{
    // Binds Choose_Name.uxml controls and exposes user intent as events.
    public class ChooseNameUIView : ViewBase
    {
        public event Action<string> OnSubmitClicked;
        public event Action OnRandomClicked;
 
        private TextField _nameField;
        private Button _randomButton;
        private Button _submitButton;
 
        private bool _callbacksRegistered;
 
        private void OnDisable()
        {
            UnregisterCallbacks();
        }
 
        public void Render(ChooseNameUIModel model)
        {
            if (!EnsureInitialized()) return;
 
            if (_nameField != null && string.IsNullOrEmpty(_nameField.value))
                _nameField.value = model.GetCurrentNetworkDisplayName();
        }
 
        public void SetNameFieldText(string text)
        {
            if (_nameField != null)
                _nameField.value = text;
        }
 
        public void SetBusy(bool isBusy)
        {
            if (_nameField != null) _nameField.SetEnabled(!isBusy);
            if (_randomButton != null) _randomButton.SetEnabled(!isBusy);
            if (_submitButton != null) _submitButton.SetEnabled(!isBusy);
        }
 
        protected override void OnInitializeUI()
        {
            // One-time cache of UI Toolkit references used by render/update paths.
            _nameField = Root.Q<TextField>(UI_Choose_Name_Map.Names[UI_Choose_Name.ChoseNameTF]);
            _randomButton = Root.Q<Button>(UI_Choose_Name_Map.Names[UI_Choose_Name.RandomBTN]);
            _submitButton = Root.Q<Button>(UI_Choose_Name_Map.Names[UI_Choose_Name.SubmitBTN]);
 
            RegisterCallbacks();
        }
 
        private void RegisterCallbacks()
        {
            if (_callbacksRegistered) return;
 
            RegisterButton(_randomButton, HandleRandomClicked, "RandomBTN");
            RegisterButton(_submitButton, HandleSubmitClicked, "SubmitBTN");
 
            _callbacksRegistered = true;
        }
 
        private void UnregisterCallbacks()
        {
            if (!_callbacksRegistered) return;
 
            if (_randomButton != null) _randomButton.clicked -= HandleRandomClicked;
            if (_submitButton != null) _submitButton.clicked -= HandleSubmitClicked;
 
            _callbacksRegistered = false;
        }
 
        private static void RegisterButton(Button button, Action callback, string buttonName)
        {
            if (button == null)
            {
                Debug.LogError($"[ChooseNameUIView] Could not find Button named '{buttonName}' in Choose_Name.");
                return;
            }
 
            button.clicked += callback;
        }
 
        private void HandleRandomClicked()
        {
            OnRandomClicked?.Invoke();
        }
 
        private void HandleSubmitClicked()
        {
            OnSubmitClicked?.Invoke(_nameField != null ? _nameField.value : string.Empty);
        }
    }
}
}