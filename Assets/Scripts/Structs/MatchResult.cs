using System.Collections.Generic;
using Game_Logic;

namespace Structs
{
    public struct MatchResult
    {
        public List<QuestionResult> QuestionResults;
        public int CorrectAnswers;
        public float AvgAnswerTime;
        
        public MatchResult(List<QuestionResult> questionResults, float avgAnswerTime,  int correctAnswers)
        {
            AvgAnswerTime = avgAnswerTime;
            QuestionResults = questionResults;
            CorrectAnswers = 0;
        }
    }
}