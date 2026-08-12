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
    
    public struct GameStartedEvent
    {
        public PlayerData PlayerData;
    }

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

    public struct TimerFinishedEvent {}
}