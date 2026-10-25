namespace SpaceExplorer
{
    /// <summary>
    /// Single source of truth for scene names. Must match the scenes registered in Build Settings.
    /// </summary>
    public static class SceneNames
    {
        /// <summary>Title screen with the Play, Instructions and Quit buttons.</summary>
        public const string MainMenu = "MainMenu";

        /// <summary>The run itself: ship, asteroids, stars and the boss fight.</summary>
        public const string Gameplay = "Gameplay";

        /// <summary>Result screen shown after the ship is destroyed.</summary>
        public const string EndGame = "EndGame";
    }
}
