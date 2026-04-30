namespace Game.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contract for the async scene-loading service. Owned exclusively by GameManager.
    /// All public methods are safe to call from the main thread only.
    /// </summary>
    public interface ISceneLoader
    {
        /// <summary>True while any scene transition is in progress.</summary>
        bool IsLoading { get; }

        /// <summary>
        /// Fires with normalized progress in [0, 1] at key points during a transition.
        /// Always fires with 1f when the transition completes (including on error).
        /// </summary>
        event Action<float> Progress;

        /// <summary>
        /// Transitions to the Menu scene.
        /// No-op if Menu is already the active content scene.
        /// Returns the in-flight task if a transition is already underway.
        /// </summary>
        Task LoadMenuAsync(CancellationToken ct = default);

        /// <summary>
        /// Transitions to the Game scene additively.
        /// No-op if Game is already loaded.
        /// Returns the in-flight task if a transition is already underway.
        /// </summary>
        Task LoadGameAsync(CancellationToken ct = default);

        /// <summary>
        /// Unloads the Game scene.
        /// No-op if Game is not currently loaded.
        /// Returns the in-flight task if a transition is already underway.
        /// </summary>
        Task UnloadGameAsync(CancellationToken ct = default);

        /// <summary>
        /// Unloads then reloads the Game scene, forcing a clean state.
        /// Returns the in-flight task if a transition is already underway.
        /// </summary>
        Task ReloadGameAsync(CancellationToken ct = default);
    }
}
