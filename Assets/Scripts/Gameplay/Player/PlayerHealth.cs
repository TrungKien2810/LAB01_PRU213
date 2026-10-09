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

        public event Action<int> LivesChanged;
        public event Action Died;

        public Faction Faction => Faction.Player;
        public int Lives { get; private set; }
        public int MaxLives => maxLives;

        bool isInvulnerable;

        void Awake() => Lives = maxLives;

        void Start() => LivesChanged?.Invoke(Lives);

        public void TakeDamage(int amount)
        {
            if (isInvulnerable || Lives <= 0) return;

            Lives = Mathf.Max(0, Lives - amount);
            LivesChanged?.Invoke(Lives);

            if (Lives == 0) Die();
            else
            {
                AudioDirector.Play(SoundEffect.PlayerHit);
                StartCoroutine(InvulnerabilityRoutine());
            }
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
