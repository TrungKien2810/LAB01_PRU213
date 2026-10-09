using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>World-space rectangle visible through the main orthographic camera.</summary>
    public static class ScreenBounds
    {
        public static Rect World
        {
            get
            {
                Camera cam = Camera.main;
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;
                Vector2 center = cam.transform.position;
                return new Rect(center.x - halfWidth, center.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
            }
        }
    }
}
