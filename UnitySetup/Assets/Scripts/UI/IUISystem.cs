namespace Game.UI
{
    using System;

    /// <summary>Contract for the persistent UI root that shows and hides screen overlays.</summary>
    public interface IUISystem
    {
        /// <summary>Returns true when any overlay (loading or pause) is currently visible.</summary>
        bool IsAnyOverlayOpen { get; }

        /// <summary>Shows the full-screen loading overlay.</summary>
        void ShowLoading();

        /// <summary>Hides the full-screen loading overlay.</summary>
        void HideLoading();

        /// <summary>Shows the named screen overlay.</summary>
        void ShowScreen(ScreenId id);

        /// <summary>Hides the named screen overlay.</summary>
        void HideScreen(ScreenId id);

        /// <summary>Fired when the player taps the Resume button on the pause overlay.</summary>
        event Action OnResumeRequested;

        /// <summary>Fired when the player taps the Return to Menu button on the pause overlay.</summary>
        event Action OnReturnToMenuRequested;
    }
}
