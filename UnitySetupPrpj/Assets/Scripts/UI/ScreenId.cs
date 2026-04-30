namespace Game.UI
{
    /// <summary>Identifies a named UI screen managed by <see cref="UISystem"/>.</summary>
    public enum ScreenId
    {
        /// <summary>Main menu screen (owned by the Menu scene; not handled by UISystem directly).</summary>
        Menu,

        /// <summary>Pause overlay (owned by the Persistent scene).</summary>
        Pause,
    }
}
