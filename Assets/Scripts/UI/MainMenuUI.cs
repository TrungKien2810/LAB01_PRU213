using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SpaceExplorer
{
    /// <summary>Main menu: Play starts the game, Instructions opens the how-to-play panel, Quit exits.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] Button playButton;
        [SerializeField] Button instructionsButton;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject instructionsPanel;
        [SerializeField] Button closeInstructionsButton;

        void Awake()
        {
            // Moving between buttons ticks; pressing one answers with the click.
            UiSound.Attach(playButton, instructionsButton, closeInstructionsButton, quitButton);

            playButton.onClick.AddListener(() => Click(() => SceneLoader.Load(SceneNames.Gameplay)));
            instructionsButton.onClick.AddListener(() => Click(() => instructionsPanel.SetActive(true)));
            closeInstructionsButton.onClick.AddListener(() => Click(() => instructionsPanel.SetActive(false)));
            quitButton.onClick.AddListener(() => Click(AppQuit.Quit));
            instructionsPanel.SetActive(false);
        }

        /// <summary>Runs a menu action behind a tick, so every button answers in the same voice.</summary>
        static void Click(UnityAction action)
        {
            AudioDirector.Play(SoundEffect.UiClick);
            action();
        }
    }
}
