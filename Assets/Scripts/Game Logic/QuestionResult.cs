namespace Game_Logic
{
    public class QuestionResult
    {
        public bool WasAnsweredCorrectly { get; }
        public float TimeTaken { get; }
        
        public QuestionResult(bool wasAnsweredCorrectly,  float timeTaken)
        {
            WasAnsweredCorrectly = wasAnsweredCorrectly;
            TimeTaken = timeTaken;
        }
    }
}