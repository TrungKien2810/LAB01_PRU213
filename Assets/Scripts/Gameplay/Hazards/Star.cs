using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>A collectible star that floats down in a gentle wave. Touching it awards points.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Star : MonoBehaviour
    {
        [SerializeField] int points = 50;
        [SerializeField] Vector2 fallSpeedRange = new(1f, 2f);
        [SerializeField] float waveAmplitude = 1.2f;
        [SerializeField] float waveFrequency = 1.5f;

        Rigidbody2D body;
        float fallSpeed;
        float phase;

        void Start()
        {
            body = GetComponent<Rigidbody2D>();
            fallSpeed = Random.Range(fallSpeedRange.x, fallSpeedRange.y);
            phase = Random.Range(0f, Mathf.PI * 2f);
        }

        void FixedUpdate()
        {
            float sway = Mathf.Cos(Time.time * waveFrequency + phase) * waveAmplitude;
            body.linearVelocity = new Vector2(sway, -fallSpeed);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerHealth _)) return;

            AudioDirector.Play(SoundEffect.StarPickup);
            GameEvents.AwardPoints(points);
            Destroy(gameObject);
        }
    }
}
