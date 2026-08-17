using Base_Classes;
using Events;
using UI_MVP.ChooseName.UI_MVP.ChooseName;
using UnityEngine;
 
namespace UI_MVP.ChooseName
{
    // Coordinates Choose Name view events, model updates, and app-level side effects.
    public class ChooseNameUIPresenter : IPresenter
    {
        private readonly ChooseNameUIModel _model;
        private readonly ChooseNameUIView _view;
 
        private bool _isSubscribed;
 
        public ChooseNameUIPresenter(
            ChooseNameUIModel model,
            ChooseNameUIView view)
        {
            _model = model;
            _view = view;
        }
 
        public void SubscribeToEvents()
        {
            if (_isSubscribed) return;
 
            _view.OnRandomClicked += HandleRandomClicked;
            _view.OnSubmitClicked += HandleSubmitClicked;
 
            _isSubscribed = true;
            InitializeView();
        }
 
        public void UnSubscribeToEvents()
        {
            if (!_isSubscribed) return;
 
            _view.OnRandomClicked -= HandleRandomClicked;
            _view.OnSubmitClicked -= HandleSubmitClicked;
 
            _isSubscribed = false;
        }
 
        private void InitializeView()
        {
            _view.Render(_model);
        }
 
        private void HandleRandomClicked()
        {
            _view.SetBusy(true);
            _view.StartCoroutine(_model.FetchRandomNameRoutine(OnRandomNameFetched));
        }
 
        private void OnRandomNameFetched(string name)
        {
            _view.SetBusy(false);
            _view.SetNameFieldText(name);
        }
 
        private void HandleSubmitClicked(string enteredName)
        {
            if (_model.TryApplyConfirmedName(enteredName))
            {
                EventBus.Raise(new ChooseNameConfirmedEvent(_model.ConfirmedName));
            }
            else
            {
                Debug.LogWarning("[ChooseNameUIPresenter] Name was empty or already taken; submission rejected.");
            }
        }
    }
}