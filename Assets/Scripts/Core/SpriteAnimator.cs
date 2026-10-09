using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Plays a flip-book of sprites on a SpriteRenderer. Lightweight alternative to an
    /// Animator Controller for simple looping (thruster, stars) or one-shot (explosion) effects.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField, Min(0.1f)] float framesPerSecond = 10f;
        [SerializeField] bool loop = true;
        [Tooltip("Destroy this GameObject after the last frame of a non-looping animation.")]
        [SerializeField] bool destroyOnComplete;
        [Tooltip("Start at a random frame so many instances do not animate in lockstep.")]
        [SerializeField] bool randomStartFrame;

        SpriteRenderer spriteRenderer;
        float elapsed;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (randomStartFrame && frames.Length > 0)
                elapsed = Random.Range(0, frames.Length) / framesPerSecond;
            ShowFrame();
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            if (!loop && CurrentFrame >= frames.Length)
            {
                if (destroyOnComplete) Destroy(gameObject);
                else enabled = false;
                return;
            }
            ShowFrame();
        }

        int CurrentFrame => Mathf.FloorToInt(elapsed * framesPerSecond);

        void ShowFrame()
        {
            if (frames == null || frames.Length == 0) return;
            int frame = loop ? CurrentFrame % frames.Length : Mathf.Min(CurrentFrame, frames.Length - 1);
            spriteRenderer.sprite = frames[frame];
        }
    }
}
