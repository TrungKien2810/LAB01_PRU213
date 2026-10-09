using UnityEngine;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>
    /// In-game HUD. Purely a view: it listens to gameplay components and never changes game state.
    /// Shows score, remaining lives, the boss warning banner and the boss health bar.
    /// </summary>
    public class HudUI : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] ScoreKeeper scoreKeeper;
        [SerializeField] PlayerHealth player;
        [SerializeField] BossDirector bossDirector;

        [Header("Views")]
        [SerializeField] Text scoreText;
        [SerializeField] Text livesText;
        [SerializeField] GameObject warningBanner;
        [SerializeField] BossHealthBarUI bossHealthBar;

        void Awake()
        {
            warningBanner.SetActive(false);
            bossHealthBar.Hide();
        }

        void OnEnable()
        {
            scoreKeeper.ScoreChanged += ShowScore;
            player.LivesChanged += ShowLives;
            bossDirector.WarningToggled += warningBanner.SetActive;
            bossDirector.BossSpawned += bossHealthBar.Show;
            bossDirector.BossDefeated += bossHealthBar.Hide;
        }

        void OnDisable()
        {
            scoreKeeper.ScoreChanged -= ShowScore;
            player.LivesChanged -= ShowLives;
            bossDirector.WarningToggled -= warningBanner.SetActive;
            bossDirector.BossSpawned -= bossHealthBar.Show;
            bossDirector.BossDefeated -= bossHealthBar.Hide;
        }

        void ShowScore(int score) => scoreText.text = $"SCORE  {score}";

        void ShowLives(int lives) => livesText.text = $"LIVES  {lives}";
    }
}
