using System;

namespace SpaceExplorer
{
    /// <summary>
    /// Tiny global event hub for gameplay facts that spawned prefabs need to report
    /// without holding references to scene objects (e.g. an asteroid awarding points).
    /// Listeners must unsubscribe in OnDisable.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised with a positive (reward) or negative (penalty) amount of points.</summary>
        public static event Action<int> PointsAwarded;

        /// <summary>Raises <see cref="PointsAwarded"/>: a positive amount rewards, a negative one penalises.</summary>
        public static void AwardPoints(int amount) => PointsAwarded?.Invoke(amount);
    }
}
