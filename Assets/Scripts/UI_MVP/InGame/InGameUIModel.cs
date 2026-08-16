using Base_Classes;
using Structs;

namespace UI.InGame
{
    public enum InGameAnswerFeedbackState
    {
        None,
        Correct,
        Incorrect,
        Unanswered
    }

    public class InGameUIModel : ModelBase
    {
        public string QuestionText { get; private set; } = string.Empty;
        public string[] AnswerOptions { get; } = new string[4] { string.Empty, string.Empty, string.Empty, string.Empty };
        public int SelectedAnswerIndex { get; private set; } = -1;
        public int CorrectAnswerIndex { get; private set; } = -1;
        public bool IsInputLocked { get; private set; }
        public InGameAnswerFeedbackState FeedbackState { get; private set; } = InGameAnswerFeedbackState.None;

        public bool CanConfirm => !IsInputLocked && SelectedAnswerIndex >= 0 && SelectedAnswerIndex < AnswerOptions.Length;

        public void SetQuestion(Question question)
        {
            QuestionText = question.QuestionText ?? string.Empty;

            for (var i = 0; i < AnswerOptions.Length; i++)
            {
                if (question.Options != null && i < question.Options.Length)
                {
                    AnswerOptions[i] = question.Options[i] ?? string.Empty;
                    continue;
                }

                AnswerOptions[i] = string.Empty;
            }

            SelectedAnswerIndex = -1;
            CorrectAnswerIndex = -1;
            FeedbackState = InGameAnswerFeedbackState.None;
            IsInputLocked = false;
        }

        public void SelectAnswer(int answerIndex)
        {
            if (IsInputLocked) return;
            if (answerIndex < 0 || answerIndex >= AnswerOptions.Length) return;

            SelectedAnswerIndex = answerIndex;
        }

        public void MarkSubmitted()
        {
            if (!CanConfirm) return;

            IsInputLocked = true;
            FeedbackState = InGameAnswerFeedbackState.None;
            CorrectAnswerIndex = -1;
        }

        public void MarkAnsweredCorrectly()
        {
            IsInputLocked = true;
            FeedbackState = InGameAnswerFeedbackState.Correct;
            CorrectAnswerIndex = SelectedAnswerIndex;
        }

        public void MarkAnsweredIncorrectly(int correctAnswerIndex)
        {
            IsInputLocked = true;
            FeedbackState = InGameAnswerFeedbackState.Incorrect;
            CorrectAnswerIndex = correctAnswerIndex;
        }

        public void MarkUnanswered(int correctAnswerIndex)
        {
            IsInputLocked = true;
            FeedbackState = InGameAnswerFeedbackState.Unanswered;
            CorrectAnswerIndex = correctAnswerIndex;
        }
    }
}
