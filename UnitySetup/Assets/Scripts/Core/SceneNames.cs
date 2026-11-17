namespace Game.Core
{
    /// <summary>
    /// Canonical scene name constants. Always use these instead of raw string literals.
    /// Must match the scene file names registered in Build Settings.
    /// </summary>
    public static class SceneNames
    {
        /// <summary>The Bootstrap scene; first to load. Unloaded immediately after Persistent is ready.</summary>
        public const string Boot = "Boot";

        /// <summary>The always-loaded infrastructure scene that owns all shared systems.</summary>
        public const string Persistent = "Persistent";

        /// <summary>The main menu content scene.</summary>
        public const string Menu = "Menu";

        /// <summary>The gameplay content scene.</summary>
        public const string Game = "Game";
    }
}
