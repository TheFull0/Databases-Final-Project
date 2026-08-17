using Game_Logic;
using Structs;

namespace Events
{
    public struct ShowLoadingScreenEvent { }

    public struct HideLoadingScreenEvent { }
    
    public struct QuestionAnsweredIncorrectlyEvent
    {
        public int AnswerIndex;
    }
    
    public struct GameStartedEvent { }

    public struct QuestionAnsweredCorrectlyEvent {}

    public struct QuestionAnsweredEvent
    {
        public int AnswerIndex;
    }
    
    public struct NewQuestionLoadedEvent
    {
        public Question NewQuestion { get; }
        
        public NewQuestionLoadedEvent(Question newQuestion)
        {
            NewQuestion = newQuestion;
        }
    }

    public struct QuestionUnansweredEvent
    {
        public int AnswerIndex;
    }

    public struct MatchEndedEvent
    {
        public string PlayerName;

        public MatchEndedEvent(string playerName)
        {
            PlayerName = playerName;
        }
    }

    public struct TimerFinishedEvent {}

    public struct MainMenuQueueUpClickedEvent { }

    public struct MainMenuQueueCanceledEvent { }
    
    public struct WinScreenMainMenuClickedEvent { }

    public struct MainMenuCreditsClickedEvent { }

    public struct MainMenuQuitClickedEvent { }

    public struct MainMenuQueueStateChangedEvent
    {
        public bool IsInQueue;
    }

    public struct MainMenuPlayerNameUpdatedEvent
    {
        public string PlayerName;
    }

    public struct MainMenuQueueStatusUpdatedEvent
    {
        public string StatusText;
    }
    
    // Raised once the player has successfully confirmed their display name.
    public struct ChooseNameConfirmedEvent
    {
        public string ConfirmedName;
 
        public ChooseNameConfirmedEvent(string confirmedName)
        {
            ConfirmedName = confirmedName;
        }
    }
}