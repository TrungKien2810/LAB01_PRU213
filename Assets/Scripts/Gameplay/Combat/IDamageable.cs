namespace SpaceExplorer
{
    /// <summary>Anything that can be hit by projectiles or by colliding with an enemy.</summary>
    public interface IDamageable
    {
        Faction Faction { get; }
        void TakeDamage(int amount);
    }
}
