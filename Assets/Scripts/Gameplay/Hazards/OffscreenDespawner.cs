using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Destroys objects once they leave the bottom or sides of the screen.
    /// Objects enter from above, so the top edge is deliberately not checked.
    /// </summary>
    public class OffscreenDespawner : MonoBehaviour
    {
        [SerializeField, Min(0f)] float margin = 2f;

        void Update()
        {
            Rect area = ScreenBounds.World;
            Vector2 p = transform.position;
            if (p.y < area.yMin - margin || p.x < area.xMin - margin || p.x > area.xMax + margin)
                Destroy(gameObject);
        }
    }
}
