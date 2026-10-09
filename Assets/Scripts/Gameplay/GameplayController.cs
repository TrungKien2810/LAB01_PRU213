using System.Collections;
using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Scene-level flow for Gameplay: when the player dies, stores the result in
    /// <see cref="GameSession"/> and moves on to the End Game scene after a short pause.
    /// </summary>
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] PlayerHealth player;
        [SerializeField] ScoreKeeper scoreKeeper;
        [Tooltip("Seconds to let the explosion play before leaving the scene.")]
        [SerializeField, Min(0f)] float delayBeforeEnd = 1.5f;

        void OnEnable() => player.Died += HandlePlayerDied;
        void OnDisable() => player.Died -= HandlePlayerDied;

        void HandlePlayerDied() => StartCoroutine(EndRunRoutine());

        IEnumerator EndRunRoutine()
        {
            GameSession.Submit(scoreKeeper.Score);
            yield return new WaitForSeconds(delayBeforeEnd);
            SceneLoader.Load(SceneNames.EndGame);
        }
    }
}
