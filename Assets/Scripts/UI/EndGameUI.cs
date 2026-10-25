using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>
    /// End Game screen: shows the final and best score, a medal for the score reached,
    /// a "New Record" badge when the best score was beaten, and Replay / Home / Quit buttons.
    /// It also plays the run's verdict — the falling game-over jingle, or the fanfare when the
    /// best score was beaten.
    /// </summary>
    public class EndGameUI : MonoBehaviour
    {
        [SerializeField] Text scoreText;
        [SerializeField] Text bestScoreText;
        [SerializeField] GameObject newRecordBadge;

        [Header("Medal")]
        [SerializeField] Image medalImage;
        [Tooltip("Bronze, silver, gold — same order as the thresholds.")]
        [SerializeField] Sprite[] medalSprites;
        [Tooltip("Minimum score for each medal, ascending.")]
        [SerializeField] int[] medalThresholds = { 200, 600, 1200 };

        [Header("Buttons")]
        [SerializeField] Button replayButton;
        [SerializeField] Button homeButton;
        [SerializeField] Button quitButton;

        void Awake()
        {
            // A scene generated before the Replay button existed simply leaves the field empty.
            if (replayButton) replayButton.onClick.AddListener(() => Click(() => SceneLoader.Load(SceneNames.Gameplay)));
            homeButton.onClick.AddListener(() => Click(() => SceneLoader.Load(SceneNames.MainMenu)));
            quitButton.onClick.AddListener(() => Click(AppQuit.Quit));

            // Moving between buttons ticks; pressing one answers with the click.
            UiSound.Attach(replayButton, homeButton, quitButton);
        }

        void Start()
        {
            int score = GameSession.LastScore;
            scoreText.text = $"SCORE  {score}";
            bestScoreText.text = $"BEST  {GameSession.BestScore}";
            newRecordBadge.SetActive(GameSession.IsNewRecord);

            // The screen's verdict, in sound: rising fanfare or falling game over.
            AudioDirector.Play(GameSession.IsNewRecord ? SoundEffect.NewRecord : SoundEffect.GameOver);

            int medal = MedalIndexFor(score);
            medalImage.gameObject.SetActive(medal >= 0);
            if (medal >= 0) medalImage.sprite = medalSprites[medal];
        }

        /// <summary>Runs a menu action behind a tick, so every button answers in the same voice.</summary>
        static void Click(UnityAction action)
        {
            AudioDirector.Play(SoundEffect.UiClick);
            action();
        }

        /// <summary>Index of the best medal earned, or -1 when no threshold is reached.</summary>
        int MedalIndexFor(int score)
        {
            int earned = -1;
            for (int i = 0; i < medalThresholds.Length && i < medalSprites.Length; i++)
                if (score >= medalThresholds[i]) earned = i;
            return earned;
        }
    }
}
