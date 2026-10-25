using System;
using System.Collections;
using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Decides when a boss fight happens. Each time the score crosses the next threshold it
    /// stops the asteroid stream, shows a warning, then spawns the boss. When the boss is
    /// defeated the asteroids return and the next threshold is raised.
    /// </summary>
    public class BossDirector : MonoBehaviour
    {
        [SerializeField] Boss bossPrefab;
        [SerializeField] ScoreKeeper scoreKeeper;
        [SerializeField] Spawner asteroidSpawner;
        [SerializeField] int firstBossScore = 400;
        [SerializeField] int scoreBetweenBosses = 1000;
        [SerializeField, Min(0f)] float warningSeconds = 2.5f;

        /// <summary>Raised when the warning starts (true) and ends (false).</summary>
        public event Action<bool> WarningToggled;
        /// <summary>Raised with the new boss so the HUD can bind to its health.</summary>
        public event Action<Boss> BossSpawned;

        /// <summary>Raised when the fight is over and the asteroid stream resumes.</summary>
        public event Action BossDefeated;

        int nextBossScore;
        bool fightInProgress;

        void Awake() => nextBossScore = firstBossScore;

        void OnEnable() => scoreKeeper.ScoreChanged += HandleScoreChanged;
        void OnDisable() => scoreKeeper.ScoreChanged -= HandleScoreChanged;

        void HandleScoreChanged(int score)
        {
            if (!fightInProgress && score >= nextBossScore) StartCoroutine(BossFightRoutine());
        }

        IEnumerator BossFightRoutine()
        {
            fightInProgress = true;
            asteroidSpawner.IsPaused = true;

            AudioDirector.Play(SoundEffect.BossWarning);
            WarningToggled?.Invoke(true);
            yield return new WaitForSeconds(warningSeconds);
            WarningToggled?.Invoke(false);

            Vector3 entry = new(0f, ScreenBounds.World.yMax + 2f, 0f);
            Boss boss = Instantiate(bossPrefab, entry, Quaternion.identity);
            bool defeated = false;
            boss.Defeated += () => defeated = true;
            BossSpawned?.Invoke(boss);

            yield return new WaitUntil(() => defeated);

            BossDefeated?.Invoke();
            asteroidSpawner.IsPaused = false;
            nextBossScore = scoreKeeper.Score + scoreBetweenBosses;
            fightInProgress = false;
        }
    }
}
