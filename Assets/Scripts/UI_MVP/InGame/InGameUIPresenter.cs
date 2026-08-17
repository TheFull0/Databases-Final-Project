using Base_Classes;
using Events;
using UnityEngine;

namespace UI.InGame
{
    public class InGameUIPresenter : IPresenter
    {
        private readonly InGameUIModel _model;
        private readonly InGameUIView _view;
        private bool _isSubscribed;

        public InGameUIPresenter(InGameUIModel model, InGameUIView view)
        {
            _model = model;
            _view = view;
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed) return;

            _view.OnAnswerSelected += HandleAnswerSelected;
            _view.OnConfirmClicked += HandleConfirmClicked;

            EventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Subscribe<NewQuestionLoadedEvent>(OnNewQuestionLoaded);
            EventBus.Subscribe<QuestionAnsweredCorrectlyEvent>(OnQuestionAnsweredCorrectly);
            EventBus.Subscribe<QuestionAnsweredIncorrectlyEvent>(OnQuestionAnsweredIncorrectly);
            EventBus.Subscribe<QuestionUnansweredEvent>(OnQuestionUnanswered);

            _isSubscribed = true;
            _view.Render(_model);
        }

        public void UnSubscribeToEvents()
        {
            if (!_isSubscribed) return;

            _view.OnAnswerSelected -= HandleAnswerSelected;
            _view.OnConfirmClicked -= HandleConfirmClicked;

            EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
            EventBus.Unsubscribe<NewQuestionLoadedEvent>(OnNewQuestionLoaded);
            EventBus.Unsubscribe<QuestionAnsweredCorrectlyEvent>(OnQuestionAnsweredCorrectly);
            EventBus.Unsubscribe<QuestionAnsweredIncorrectlyEvent>(OnQuestionAnsweredIncorrectly);
            EventBus.Unsubscribe<QuestionUnansweredEvent>(OnQuestionUnanswered);

            _isSubscribed = false;
        }

        private void OnGameStarted(GameStartedEvent _)
        {
            if (!UIManager.Instance)
            {
                Debug.LogError("[InGameUIPresenter] UIManager instance is unavailable.");
                return;
            }

            UIManager.Instance.SetCurrentScreen(UIScreenId.InGame);
        }

        private void OnNewQuestionLoaded(NewQuestionLoadedEvent e)
        {
            _model.SetQuestion(e.NewQuestion);
            _view.Render(_model);
        }

        private void HandleAnswerSelected(int answerIndex)
        {
            _model.SelectAnswer(answerIndex);
            _view.Render(_model);
        }

        private void HandleConfirmClicked()
        {
            if (!_model.CanConfirm) return;

            _model.MarkSubmitted();
            _view.Render(_model);
            EventBus.Raise(new QuestionAnsweredEvent { AnswerIndex = _model.SelectedAnswerIndex });
        }

        private void OnQuestionAnsweredCorrectly(QuestionAnsweredCorrectlyEvent _)
        {
            _model.MarkAnsweredCorrectly();
            _view.Render(_model);
        }

        private void OnQuestionAnsweredIncorrectly(QuestionAnsweredIncorrectlyEvent e)
        {
            _model.MarkAnsweredIncorrectly(e.AnswerIndex);
            _view.Render(_model);
        }

        private void OnQuestionUnanswered(QuestionUnansweredEvent e)
        {
            _model.MarkUnanswered(e.AnswerIndex);
            _view.Render(_model);
        }
    }
}
