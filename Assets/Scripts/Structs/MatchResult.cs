namespace Structs
{
    public struct MatchResult
    {
        public int CorrectAnswers;
        public float AvgAnswerTime;
        
        public MatchResult(int correctAnswers, float avgAnswerTime)
        {
            CorrectAnswers = correctAnswers;
            AvgAnswerTime = avgAnswerTime;
        }
    }
}