namespace Structs
{
    // The two-player result of a *finished* match, fetched from the server
    // once both players have submitted. This is distinct from MatchResult,
    // which is this player's own per-question breakdown built locally
    // during play - that one never needs the server at all.
    public struct MatchOutcome
    {
        public bool IsWin;
        public bool IsTie;
        public int MyCorrectAnswers;
        public int OpponentCorrectAnswers;
        public float OpponentTotalAnswerTime; // seconds
    }
}