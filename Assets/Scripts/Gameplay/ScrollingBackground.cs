using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Endless vertical scrolling backdrop. The sprite is scaled to cover the camera width and
    /// drawn as two stacked tiles; the pair is shifted down and wrapped every tile height.
    /// Requires the sprite to be imported with Mesh Type = Full Rect.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScrollingBackground : MonoBehaviour
    {
        [SerializeField] float scrollSpeed = 0.6f;

        float tileHeight;
        float offset;

        void Start()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            Rect view = ScreenBounds.World;

            float scale = Mathf.Max(view.width / spriteSize.x, view.height / spriteSize.y);
            transform.localScale = new Vector3(scale, scale, 1f);
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = new Vector2(spriteSize.x, spriteSize.y * 2f);
            tileHeight = spriteSize.y * scale;
        }

        void Update()
        {
            offset = (offset + scrollSpeed * Time.deltaTime) % tileHeight;
            // Two tiles centred at (tileHeight/2 - offset) always cover [-tileHeight/2, tileHeight/2],
            // which is at least the full camera view.
            Vector3 p = transform.position;
            transform.position = new Vector3(p.x, Camera.main.transform.position.y + tileHeight * 0.5f - offset, p.z);
        }
    }
}
