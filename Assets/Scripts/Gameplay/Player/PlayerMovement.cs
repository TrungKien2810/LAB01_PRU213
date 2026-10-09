using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>Moves the ship in all directions and keeps it inside the visible screen.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] float speed = 7f;
        [Tooltip("Distance kept between the ship's centre and the screen edges.")]
        [SerializeField] Vector2 edgePadding = new(0.6f, 0.6f);

        Rigidbody2D body;
        PlayerInputReader input;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
        }

        void FixedUpdate()
        {
            Vector2 target = body.position + input.Move * (speed * Time.fixedDeltaTime);
            Rect area = ScreenBounds.World;
            target.x = Mathf.Clamp(target.x, area.xMin + edgePadding.x, area.xMax - edgePadding.x);
            target.y = Mathf.Clamp(target.y, area.yMin + edgePadding.y, area.yMax - edgePadding.y);
            body.MovePosition(target);
        }
    }
}
