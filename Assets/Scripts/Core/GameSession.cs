using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Carries the result of the last run from the Gameplay scene to the End Game scene
    /// and persists the best score between sessions.
    /// </summary>
    public static class GameSession
    {
        const string BestScoreKey = "SpaceExplorer.BestScore";

        public static int LastScore { get; private set; }
        public static bool IsNewRecord { get; private set; }
        public static int BestScore => PlayerPrefs.GetInt(BestScoreKey, 0);

        /// <summary>Stores the final score of a run and updates the best score if beaten.</summary>
        public static void Submit(int score)
        {
            LastScore = score;
            IsNewRecord = score > BestScore;
            if (!IsNewRecord) return;

            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
        }
    }
}
