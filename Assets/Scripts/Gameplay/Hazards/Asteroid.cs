using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// A drifting, spinning rock. Shooting it awards points; ramming it costs the player
    /// a life and deducts points. Each instance randomises its look, size, speed and spin.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class Asteroid : MonoBehaviour, IDamageable
    {
        [SerializeField] Sprite[] variants;
        [SerializeField, Min(1)] int hitPoints = 2;
        [SerializeField] int destroyPoints = 10;
        [SerializeField] int collisionPenalty = 50;
        [SerializeField] Vector2 speedRange = new(1.5f, 3.5f);
        [Tooltip("Maximum sideways drift as a fraction of the forward speed.")]
        [SerializeField, Range(0f, 1f)] float maxDrift = 0.45f;
        [SerializeField] Vector2 spinRange = new(-90f, 90f);
        [SerializeField] Vector2 scaleRange = new(0.7f, 1.2f);
        [SerializeField] GameObject explosionPrefab;

        /// <summary>A rock is a hazard, so it fights for the enemy side.</summary>
        public Faction Faction => Faction.Enemy;

        void Start()
        {
            if (variants.Length > 0) GetComponent<SpriteRenderer>().sprite = variants[Random.Range(0, variants.Length)];
            transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);

            var body = GetComponent<Rigidbody2D>();
            Vector2 direction = new Vector2(Random.Range(-maxDrift, maxDrift), -1f).normalized;
            body.linearVelocity = direction * Random.Range(speedRange.x, speedRange.y);
            body.angularVelocity = Random.Range(spinRange.x, spinRange.y);
        }

        /// <summary>Chips a hit point off the rock; the last one destroys it and awards
        /// <see cref="destroyPoints"/>. Always reports the hit as landed.</summary>
        public bool TakeDamage(int amount)
        {
            hitPoints -= amount;
            if (hitPoints > 0)
            {
                AudioDirector.Play(SoundEffect.AsteroidCrack);
                return true;
            }

            GameEvents.AwardPoints(destroyPoints);
            Explode();
            return true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerHealth player)) return;

            // Rock on hull is felt as weight; the blast that follows belongs to the rock itself.
            AudioDirector.Play(SoundEffect.HullImpact);
            // The penalty follows the damage: a hit the shields absorbed costs no points.
            if (player.TakeDamage(1)) GameEvents.AwardPoints(-collisionPenalty);
            Explode();
        }

        void Explode()
        {
            AudioDirector.Play(SoundEffect.Explosion);
            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
