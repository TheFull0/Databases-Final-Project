using System;

namespace Game_Logic
{
    public class PlayerRatingCalculator
    {
        public static int CalculateNewRating(int currentRating, int opponentRating, bool isWin)
        {
            // Simple Elo rating calculation
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (opponentRating - currentRating) / 400.0));
            int kFactor = 32; // K-factor can be adjusted based on the game's needs

            int newRating = currentRating + (int)(kFactor * ((isWin ? 1 : 0) - expectedScore));
            return newRating;
        }
    }
}