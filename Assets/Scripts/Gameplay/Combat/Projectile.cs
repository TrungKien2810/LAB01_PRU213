using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// A bullet that flies along its local up axis and damages the first
    /// <see cref="IDamageable"/> of the opposing faction it touches.
    /// Used both for the player's laser and the boss's energy orbs.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] Faction faction = Faction.Player;
        [SerializeField, Min(0f)] float speed = 14f;
        [SerializeField, Min(1)] int damage = 1;
        [SerializeField, Min(0.1f)] float lifetime = 3f;

        void Start()
        {
            GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;
            Destroy(gameObject, lifetime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out IDamageable target) || target.Faction == faction) return;

            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
