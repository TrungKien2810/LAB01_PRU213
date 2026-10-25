using System;
using System.Collections;
using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Boss warship: glides in from the top, sways left and right, and fires fans of
    /// energy orbs at the player. Flashes when hit and explodes in a chain when destroyed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(SpriteAnimator))]
    public class Boss : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1)] int maxHealth = 40;
        [SerializeField] int defeatPoints = 500;

        [Header("Movement")]
        [Tooltip("Height of the hover line, measured down from the top of the screen.")]
        [SerializeField] float hoverDepth = 2.2f;
        [SerializeField] float entrySpeed = 2f;
        [SerializeField] float swayAmplitude = 4.5f;
        [SerializeField] float swayFrequency = 0.6f;

        [Header("Attack")]
        [SerializeField] Projectile bulletPrefab;
        [SerializeField] Transform muzzle;
        [SerializeField, Min(0.1f)] float volleyInterval = 1.4f;
        [SerializeField, Min(1)] int bulletsPerVolley = 5;
        [SerializeField, Range(0f, 180f)] float spreadAngle = 60f;

        [Header("Feedback")]
        [SerializeField] Sprite hitSprite;
        [SerializeField] float hitFlashSeconds = 0.08f;
        [SerializeField] GameObject explosionPrefab;

        /// <summary>Normalised remaining health (1 = full).</summary>
        public event Action<float> HealthChanged;

        /// <summary>Raised once, after the death explosions have finished.</summary>
        public event Action Defeated;

        /// <summary>The boss fights for the enemy side.</summary>
        public Faction Faction => Faction.Enemy;

        Rigidbody2D body;
        SpriteRenderer spriteRenderer;
        SpriteAnimator idleAnimation;
        int health;
        bool hasArrived;
        bool isDead;
        float swayTime;
        float nextVolleyTime;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            idleAnimation = GetComponent<SpriteAnimator>();
            health = maxHealth;
        }

        void FixedUpdate()
        {
            if (isDead) return;

            float hoverY = ScreenBounds.World.yMax - hoverDepth;
            if (!hasArrived)
            {
                Vector2 next = Vector2.MoveTowards(body.position, new Vector2(0f, hoverY), entrySpeed * Time.fixedDeltaTime);
                body.MovePosition(next);
                hasArrived = Mathf.Approximately(next.y, hoverY);
                nextVolleyTime = Time.time + volleyInterval;
                return;
            }

            swayTime += Time.fixedDeltaTime;
            body.MovePosition(new Vector2(Mathf.Sin(swayTime * swayFrequency * Mathf.PI * 2f) * swayAmplitude, hoverY));
        }

        void Update()
        {
            if (isDead || !hasArrived || Time.time < nextVolleyTime) return;

            nextVolleyTime = Time.time + volleyInterval;
            AudioDirector.Play(SoundEffect.BossShot);
            FireVolley();
        }

        void FireVolley()
        {
            float step = bulletsPerVolley > 1 ? spreadAngle / (bulletsPerVolley - 1) : 0f;
            float start = bulletsPerVolley > 1 ? -spreadAngle * 0.5f : 0f;
            for (int i = 0; i < bulletsPerVolley; i++)
            {
                // 180° turns the projectile's "up" to face down toward the player.
                Quaternion rotation = Quaternion.Euler(0f, 0f, 180f + start + step * i);
                Instantiate(bulletPrefab, muzzle.position, rotation);
            }
        }

        /// <summary>Applies damage unless the boss is already dead or still flying in, and returns
        /// whether the hit landed. See <see cref="IDamageable"/>.</summary>
        public bool TakeDamage(int amount)
        {
            if (isDead || !hasArrived) return false; // invulnerable while flying in

            health = Mathf.Max(0, health - amount);
            HealthChanged?.Invoke((float)health / maxHealth);

            if (health == 0) StartCoroutine(DeathRoutine());
            else
            {
                AudioDirector.Play(SoundEffect.BossHit);
                StartCoroutine(HitFlashRoutine());
            }
            return true;
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (isDead || !other.TryGetComponent(out PlayerHealth player)) return;

            // Contact repeats every physics step; the audio director rate-limits the grind to one
            // hit at a time. The damage is refused while the ship is invulnerable, the sound is not.
            AudioDirector.Play(SoundEffect.HullImpact);
            player.TakeDamage(1);
        }

        IEnumerator HitFlashRoutine()
        {
            idleAnimation.enabled = false;
            spriteRenderer.sprite = hitSprite;
            yield return new WaitForSeconds(hitFlashSeconds);
            if (!isDead) idleAnimation.enabled = true;
        }

        IEnumerator DeathRoutine()
        {
            isDead = true;
            AudioDirector.Play(SoundEffect.BossExplosion);
            idleAnimation.enabled = false;
            spriteRenderer.sprite = hitSprite;
            GetComponent<Collider2D>().enabled = false;

            Bounds b = spriteRenderer.bounds;
            for (int i = 0; i < 6; i++)
            {
                Vector3 p = new(UnityEngine.Random.Range(b.min.x, b.max.x), UnityEngine.Random.Range(b.min.y, b.max.y), 0f);
                Instantiate(explosionPrefab, p, Quaternion.identity);
                AudioDirector.Play(SoundEffect.Explosion);
                yield return new WaitForSeconds(0.12f);
            }

            GameEvents.AwardPoints(defeatPoints);
            Defeated?.Invoke();
            Destroy(gameObject);
        }
    }
}
