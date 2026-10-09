using UnityEngine;

namespace SpaceExplorer
{
    /// <summary>
    /// Instantiates a prefab just above the top of the screen at a random x position and a
    /// random interval. Used for both asteroids and stars; can be paused (e.g. during a boss).
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [SerializeField] GameObject prefab;
        [SerializeField] Vector2 intervalRange = new(0.8f, 1.6f);
        [SerializeField, Min(0f)] float horizontalPadding = 0.8f;
        [SerializeField, Min(0f)] float spawnHeightAboveScreen = 1f;

        public bool IsPaused { get; set; }

        float timer;

        void Start() => ScheduleNext();

        void Update()
        {
            if (IsPaused) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;

            Rect area = ScreenBounds.World;
            float x = Random.Range(area.xMin + horizontalPadding, area.xMax - horizontalPadding);
            Instantiate(prefab, new Vector3(x, area.yMax + spawnHeightAboveScreen, 0f), Quaternion.identity, transform);
            ScheduleNext();
        }

        void ScheduleNext() => timer = Random.Range(intervalRange.x, intervalRange.y);
    }
}
