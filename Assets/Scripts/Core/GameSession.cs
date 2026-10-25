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

        /// <summary>Score of the run that just ended.</summary>
        public static int LastScore { get; private set; }

        /// <summary>True when the run that just ended beat the stored best score.</summary>
        public static bool IsNewRecord { get; private set; }

        /// <summary>Highest score ever recorded on this machine.</summary>
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
