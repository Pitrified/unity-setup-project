namespace Game.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Contract for the central application state machine and lifecycle coordinator.
    /// Owned by the Persistent scene; survives all scene loads via DontDestroyOnLoad.
    /// </summary>
    public interface IGameManager
    {
        /// <summary>The currently active lifecycle state.</summary>
        GameState CurrentState { get; }

        /// <summary>The state immediately before <see cref="CurrentState"/>.</summary>
        GameState PreviousState { get; }

        /// <summary>
        /// Fired whenever the state transitions successfully.
        /// Parameters are (previous, next).
        /// </summary>
        event Action<GameState, GameState> StateChanged;

        /// <summary>
        /// Signals that the bootstrap phase is complete.
        /// Triggers the initial Menu scene load and transitions Boot to Menu.
        /// </summary>
        void OnBootComplete();

        /// <summary>Deletes the current save and loads the Game scene as a new session.</summary>
        Task StartNewGameAsync(CancellationToken ct = default);

        /// <summary>
        /// Resumes a previously saved game session.
        /// Logs a warning and returns without transitioning if no save exists.
        /// </summary>
        Task ContinueAsync(CancellationToken ct = default);

        /// <summary>
        /// Saves current state (if applicable) and returns to the Menu scene.
        /// Valid from <see cref="GameState.Playing"/> or <see cref="GameState.Paused"/> only.
        /// </summary>
        Task ReturnToMenuAsync(CancellationToken ct = default);

        /// <summary>
        /// Suspends gameplay. Transitions <see cref="GameState.Playing"/> to
        /// <see cref="GameState.Paused"/>. No-op from any other state.
        /// </summary>
        void PauseGame();

        /// <summary>
        /// Resumes gameplay. Transitions <see cref="GameState.Paused"/> to
        /// <see cref="GameState.Playing"/>. No-op from any other state.
        /// </summary>
        void ResumeGame();

        /// <summary>Persists state if needed and exits the application.</summary>
        void QuitGame();
    }
}
