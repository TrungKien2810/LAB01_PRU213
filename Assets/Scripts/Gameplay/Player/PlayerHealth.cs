using System;
using System.Collections;
using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// The ship's lives. Each hit costs one life and grants a short blinking invulnerability.
    /// When no lives remain the ship explodes and <see cref="Died"/> is raised.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] int maxLives = 3;
        [SerializeField, Min(0f)] float invulnerableSeconds = 1.5f;
        [SerializeField] SpriteRenderer[] blinkRenderers;
        [SerializeField] GameObject explosionPrefab;

        /// <summary>Raised with the number of lives left every time it changes.</summary>
        public event Action<int> LivesChanged;

        /// <summary>Raised once, when the last life is lost.</summary>
        public event Action Died;

        /// <summary>The ship fights for the player side.</summary>
        public Faction Faction => Faction.Player;

        /// <summary>Lives left in the current run.</summary>
        public int Lives { get; private set; }

        /// <summary>Lives the ship starts a run with.</summary>
        public int MaxLives => maxLives;

        bool isInvulnerable;

        void Awake() => Lives = maxLives;

        void Start() => LivesChanged?.Invoke(Lives);

        /// <summary>Costs one life unless the ship is invulnerable or already destroyed, and
        /// returns whether the hit landed. See <see cref="IDamageable"/>.</summary>
        public bool TakeDamage(int amount)
        {
            if (isInvulnerable || Lives <= 0) return false; // the shields soak it; nothing to report

            Lives = Mathf.Max(0, Lives - amount);
            LivesChanged?.Invoke(Lives);

            if (Lives == 0) Die();
            else
            {
                AudioDirector.Play(SoundEffect.PlayerHit);
                StartCoroutine(InvulnerabilityRoutine());
            }
            return true;
        }

        void Die()
        {
            AudioDirector.Play(SoundEffect.PlayerDeath);
            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
            Died?.Invoke();
        }

        IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            const float blinkInterval = 0.1f;
            for (float t = 0f; t < invulnerableSeconds; t += blinkInterval)
            {
                SetVisible(!blinkRenderers[0].enabled);
                yield return new WaitForSeconds(blinkInterval);
            }
            SetVisible(true);
            isInvulnerable = false;
        }

        void SetVisible(bool visible)
        {
            foreach (SpriteRenderer r in blinkRenderers) r.enabled = visible;
        }
    }
}
