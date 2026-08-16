namespace Structs
{
    public struct Question
    {
        public string QuestionText;
        public string[] Options;
        public int CorrectAnswerIndex;

        public Question(string questionText, string[] options, int correctAnswerIndex)
        {
            QuestionText = questionText;
            Options = options;
            CorrectAnswerIndex = correctAnswerIndex;
        }

        public override string ToString()
        {
            return $"Question: {QuestionText}, Options: [{string.Join(", ", Options)}], CorrectAnswerIndex: {CorrectAnswerIndex}";
        }
    }
}