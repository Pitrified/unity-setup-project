namespace Game.Core
{
    /// <summary>Discrete application lifecycle states owned by <see cref="GameManager"/>.</summary>
    public enum GameState
    {
        /// <summary>Pre-initialization; set before the first scene has finished loading.</summary>
        Boot,

        /// <summary>A scene transition is in progress.</summary>
        Loading,

        /// <summary>The main menu is active.</summary>
        Menu,

        /// <summary>Active gameplay.</summary>
        Playing,

        /// <summary>Gameplay is suspended by the pause overlay.</summary>
        Paused,
    }
}
