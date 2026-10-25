namespace SpaceExplorer
{
    /// <summary>Anything that can be hit by projectiles or by colliding with an enemy.</summary>
    public interface IDamageable
    {
        Faction Faction { get; }

        /// <summary>
        /// Applies <paramref name="amount"/> damage and reports whether it landed: an invulnerable
        /// ship, a boss that is still flying in and a dead target all return false, so callers can
        /// skip their side effects (penalties, impact sounds) instead of reacting to a hit that
        /// never happened.
        /// </summary>
        bool TakeDamage(int amount);
    }
}
