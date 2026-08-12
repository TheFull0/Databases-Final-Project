namespace Structs
{
    public struct Question
    {
        public string QuestionText;
        public string[] Options;
        public int CorrectOptionIndex;

        public Question(string questionText, string[] options, int correctOptionIndex)
        {
            QuestionText = questionText;
            Options = options;
            CorrectOptionIndex = correctOptionIndex;
        }
    }
}