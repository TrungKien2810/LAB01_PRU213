namespace SpaceExplorer
{
    /// <summary>
    /// Every sound the game can play. The values double as indices into the clip table of
    /// <see cref="AudioDirector"/>, so the order must match the order of the builders in
    /// <see cref="SoundBank.CreateAll"/>.
    /// </summary>
    public enum SoundEffect
    {
        /// <summary>Fired every time the ship shoots a laser.</summary>
        LaserShot,

        /// <summary>Asteroid hit but not destroyed yet.</summary>
        AsteroidCrack,

        /// <summary>Small blast: an asteroid destroyed or the player's ship exploding.</summary>
        Explosion,

        /// <summary>The ship lost a life but is still alive.</summary>
        PlayerHit,

        /// <summary>The ship ran out of lives.</summary>
        PlayerDeath,

        /// <summary>Collected a star.</summary>
        StarPickup,

        /// <summary>Siren that announces an incoming boss.</summary>
        BossWarning,

        /// <summary>The boss fires a volley of energy orbs.</summary>
        BossShot,

        /// <summary>Energy orb hit the boss' hull.</summary>
        BossHit,

        /// <summary>The boss blows up.</summary>
        BossExplosion,

        /// <summary>Any menu button pressed.</summary>
        UiClick,

        /// <summary>New best score reached on the End Game screen.</summary>
        NewRecord,

        /// <summary>Pointer or keyboard selection moved onto another menu button.</summary>
        UiHover,

        /// <summary>An energy round burst against the ship's shield.</summary>
        ShieldImpact,

        /// <summary>Rock or hull slammed into the ship.</summary>
        HullImpact,

        /// <summary>Jingle for a run that ended without beating the best score.</summary>
        GameOver,
    }
}
