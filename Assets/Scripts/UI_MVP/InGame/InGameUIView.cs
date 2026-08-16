using System;
using Base_Classes;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.InGame
{
    public class InGameUIView : ViewBase
    {
        public event Action<int> OnAnswerSelected;
        public event Action OnConfirmClicked;

        private Label _questionTextLabel;
        private Button _answer1Button;
        private Button _answer2Button;
        private Button _answer3Button;
        private Button _answer4Button;
        private Button _confirmButton;

        private bool _callbacksRegistered;

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        public void Render(InGameUIModel model)
        {
            if (!EnsureInitialized()) return;

            SetQuestionText(model.QuestionText);
            SetAnswerTexts(model.AnswerOptions);
            VisualizeSelectionAndFeedback(model);
            SetConfirmButtonState(model);
        }

        protected override void OnInitializeUI()
        {
            _questionTextLabel = Root.Q<Label>(UI_In_Game.QuestionTXT);

            _answer1Button = Root.Q<Button>(UI_In_Game.Answer1BTN);
            _answer2Button = Root.Q<Button>(UI_In_Game.Answer2BTN);
            _answer3Button = Root.Q<Button>(UI_In_Game.Answer3BTN);
            _answer4Button = Root.Q<Button>(UI_In_Game.Answer4BT);
            _confirmButton = Root.Q<Button>(UI_In_Game.ConfirmBTN);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_callbacksRegistered) return;

            RegisterButton(_answer1Button, HandleAnswer1Clicked, "Answer1BTN");
            RegisterButton(_answer2Button, HandleAnswer2Clicked, "Answer2BTN");
            RegisterButton(_answer3Button, HandleAnswer3Clicked, "Answer3BTN");
            RegisterButton(_answer4Button, HandleAnswer4Clicked, "Answer4BT");
            RegisterButton(_confirmButton, HandleConfirmClicked, "ConfirmBTN");

            _callbacksRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if (!_callbacksRegistered) return;

            if (_answer1Button != null) _answer1Button.clicked -= HandleAnswer1Clicked;
            if (_answer2Button != null) _answer2Button.clicked -= HandleAnswer2Clicked;
            if (_answer3Button != null) _answer3Button.clicked -= HandleAnswer3Clicked;
            if (_answer4Button != null) _answer4Button.clicked -= HandleAnswer4Clicked;
            if (_confirmButton != null) _confirmButton.clicked -= HandleConfirmClicked;

            _callbacksRegistered = false;
        }

        private static void RegisterButton(Button button, Action callback, string buttonName)
        {
            if (button == null)
            {
                Debug.LogError($"[InGameUIView] Could not find Button named '{buttonName}' in In_Game.");
                return;
            }

            button.clicked += callback;
        }

        private void SetQuestionText(string questionText)
        {
            if (_questionTextLabel == null) return;
            _questionTextLabel.text = questionText ?? string.Empty;
        }

        private void SetAnswerTexts(string[] answerOptions)
        {
            SetButtonText(_answer1Button, answerOptions, 0);
            SetButtonText(_answer2Button, answerOptions, 1);
            SetButtonText(_answer3Button, answerOptions, 2);
            SetButtonText(_answer4Button, answerOptions, 3);
        }

        private static void SetButtonText(Button button, string[] options, int index)
        {
            if (button == null) return;

            var text = string.Empty;
            if (options != null && index >= 0 && index < options.Length)
            {
                text = options[index] ?? string.Empty;
            }

            button.text = text;
        }

        private void VisualizeSelectionAndFeedback(InGameUIModel model)
        {
            ResetButtonColors();

            HighlightButton(model.SelectedAnswerIndex, new Color(0.35f, 0.55f, 0.95f, 0.75f));

            switch (model.FeedbackState)
            {
                case InGameAnswerFeedbackState.Correct:
                    HighlightButton(model.SelectedAnswerIndex, new Color(0.20f, 0.75f, 0.20f, 0.85f));
                    break;
                case InGameAnswerFeedbackState.Incorrect:
                    HighlightButton(model.SelectedAnswerIndex, new Color(0.90f, 0.25f, 0.25f, 0.85f));
                    HighlightButton(model.CorrectAnswerIndex, new Color(0.20f, 0.75f, 0.20f, 0.85f));
                    break;
                case InGameAnswerFeedbackState.Unanswered:
                    HighlightButton(model.CorrectAnswerIndex, new Color(0.20f, 0.75f, 0.20f, 0.85f));
                    break;
            }

            SetInputEnabled(!model.IsInputLocked);
        }

        private void SetInputEnabled(bool isEnabled)
        {
            if (_answer1Button != null) _answer1Button.SetEnabled(isEnabled);
            if (_answer2Button != null) _answer2Button.SetEnabled(isEnabled);
            if (_answer3Button != null) _answer3Button.SetEnabled(isEnabled);
            if (_answer4Button != null) _answer4Button.SetEnabled(isEnabled);
        }

        private void SetConfirmButtonState(InGameUIModel model)
        {
            if (_confirmButton == null) return;

            switch (model.FeedbackState)
            {
                case InGameAnswerFeedbackState.Correct:
                    _confirmButton.text = "Correct!";
                    break;
                case InGameAnswerFeedbackState.Incorrect:
                    _confirmButton.text = "Incorrect";
                    break;
                case InGameAnswerFeedbackState.Unanswered:
                    _confirmButton.text = "Time's up!";
                    break;
                default:
                    _confirmButton.text = "Submit";
                    break;
            }

            _confirmButton.SetEnabled(model.CanConfirm);
        }

        private void ResetButtonColors()
        {
            ClearButtonColor(_answer1Button);
            ClearButtonColor(_answer2Button);
            ClearButtonColor(_answer3Button);
            ClearButtonColor(_answer4Button);
        }

        private void HighlightButton(int answerIndex, Color color)
        {
            var button = GetAnswerButton(answerIndex);
            if (button == null) return;

            button.style.backgroundColor = color;
        }

        private void ClearButtonColor(Button button)
        {
            if (button == null) return;
            button.style.backgroundColor = StyleKeyword.Null;
        }

        private Button GetAnswerButton(int answerIndex)
        {
            return answerIndex switch
            {
                0 => _answer1Button,
                1 => _answer2Button,
                2 => _answer3Button,
                3 => _answer4Button,
                _ => null
            };
        }

        private void HandleAnswer1Clicked() => OnAnswerSelected?.Invoke(0);
        private void HandleAnswer2Clicked() => OnAnswerSelected?.Invoke(1);
        private void HandleAnswer3Clicked() => OnAnswerSelected?.Invoke(2);
        private void HandleAnswer4Clicked() => OnAnswerSelected?.Invoke(3);
        private void HandleConfirmClicked() => OnConfirmClicked?.Invoke();
    }
}
