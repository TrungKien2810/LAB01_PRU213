using System;
using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>Owns the score of the current run. Collects points raised through <see cref="GameEvents"/>.</summary>
    public class ScoreKeeper : MonoBehaviour
    {
        public event Action<int> ScoreChanged;

        public int Score { get; private set; }

        void OnEnable() => GameEvents.PointsAwarded += Add;
        void OnDisable() => GameEvents.PointsAwarded -= Add;
        void Start() => ScoreChanged?.Invoke(Score);

        void Add(int amount)
        {
            Score = Mathf.Max(0, Score + amount); // penalties never push the score below zero
            ScoreChanged?.Invoke(Score);
        }
    }
}
