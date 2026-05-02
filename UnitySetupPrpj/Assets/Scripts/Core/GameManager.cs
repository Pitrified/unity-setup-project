namespace Game.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Game.Systems;
    using Game.UI;

    /// <summary>
    /// Central application state machine and lifecycle coordinator.
    /// Lives in the Persistent scene; survives all scene loads via
    /// <see cref="Object.DontDestroyOnLoad"/>.
    /// All public methods must be called from the Unity main thread.
    /// </summary>
    public sealed class GameManager : MonoBehaviour, IGameManager
    {
        // ------------------------------------------------------------------ singleton

        /// <summary>
        /// Read-only singleton reference. Assigned in <see cref="Awake"/>; cleared in
        /// <see cref="OnDestroy"/>. Never write from outside this class.
        /// </summary>
        public static GameManager Instance { get; private set; }

        // ------------------------------------------------------------------ IGameManager state

        /// <inheritdoc/>
        public GameState CurrentState { get; private set; } = GameState.Boot;

        /// <inheritdoc/>
        public GameState PreviousState { get; private set; } = GameState.Boot;

        /// <inheritdoc/>
        public event Action<GameState, GameState> StateChanged;

        // ------------------------------------------------------------------ system references (set by Bootstrap)

        /// <summary>The scene-loading service. Assigned by <see cref="Initialize"/>.</summary>
        public ISceneLoader SceneLoader { get; private set; }

        /// <summary>The persistence service. Assigned by <see cref="Initialize"/>.</summary>
        public ISaveSystem SaveSystem { get; private set; }

        /// <summary>The input abstraction layer. Assigned by <see cref="Initialize"/>.</summary>
        public IInputManager InputManager { get; private set; }

        /// <summary>The audio router. Assigned by <see cref="Initialize"/>.</summary>
        public IAudioManager AudioManager { get; private set; }

        /// <summary>The persistent UI root. Assigned by <see cref="Initialize"/>.</summary>
        public IUISystem UISystem { get; private set; }

        // ------------------------------------------------------------------ initializer

        /// <summary>
        /// Wires all system dependencies. Must be called by GameBootstrap before any other
        /// method on this class. Throws <see cref="ArgumentNullException"/> for null arguments.
        /// </summary>
        public void Initialize(
            ISceneLoader sceneLoader,
            ISaveSystem saveSystem,
            IInputManager inputManager,
            IAudioManager audioManager,
            IUISystem uiSystem)
        {
            SceneLoader  = sceneLoader  ?? throw new ArgumentNullException(nameof(sceneLoader));
            SaveSystem   = saveSystem   ?? throw new ArgumentNullException(nameof(saveSystem));
            InputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            AudioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
            UISystem     = uiSystem     ?? throw new ArgumentNullException(nameof(uiSystem));

            UISystem.OnResumeRequested       += ResumeGame;
            UISystem.OnReturnToMenuRequested += HandleReturnToMenuRequested;

            // Expose current state to the debug overlay via the Log utility.
            Log.GetGameState = () => CurrentState.ToString();

            Log.Info(LogCat.Boot, "GameManager initialized.");
        }

        // ------------------------------------------------------------------ IGameManager public API

        /// <inheritdoc/>
        public void OnBootComplete()
        {
            // Fire-and-forget; async continues synchronously when the fake / real
            // SceneLoader returns a completed Task.
            _ = RunBootAsync(destroyCancellationToken);
        }

        /// <inheritdoc/>
        public async Task StartNewGameAsync(CancellationToken ct = default)
        {
            if (CurrentState != GameState.Menu)
            {
                Log.Warn(LogCat.Boot, "StartNewGameAsync called from invalid state '{0}'. Ignoring.", CurrentState);
                return;
            }

            TransitionTo(GameState.Loading);
            SaveSystem.DeleteSave();

            try
            {
                await SceneLoader.LoadGameAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(LogCat.Boot, "StartNewGameAsync: scene load failed - {0}. Falling back to Menu.", ex.Message);
                await FallBackToMenuAsync(ct);
                return;
            }

            if (!ct.IsCancellationRequested)
                TransitionTo(GameState.Playing);
        }

        /// <inheritdoc/>
        public async Task ContinueAsync(CancellationToken ct = default)
        {
            if (!SaveSystem.HasSave)
            {
                Log.Warn(LogCat.Boot, "ContinueAsync: no save exists. Ignoring.");
                return;
            }

            if (CurrentState != GameState.Menu)
            {
                Log.Warn(LogCat.Boot, "ContinueAsync called from invalid state '{0}'. Ignoring.", CurrentState);
                return;
            }

            TransitionTo(GameState.Loading);

            try
            {
                await SceneLoader.LoadGameAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(LogCat.Boot, "ContinueAsync: scene load failed - {0}. Falling back to Menu.", ex.Message);
                await FallBackToMenuAsync(ct);
                return;
            }

            if (!ct.IsCancellationRequested)
                TransitionTo(GameState.Playing);
        }

        /// <inheritdoc/>
        public async Task ReturnToMenuAsync(CancellationToken ct = default)
        {
            if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
            {
                Log.Warn(LogCat.Boot, "ReturnToMenuAsync called from invalid state '{0}'. Ignoring.", CurrentState);
                return;
            }

            PersistCurrentState();
            TransitionTo(GameState.Loading);

            try
            {
                await SceneLoader.LoadMenuAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(LogCat.Boot, "ReturnToMenuAsync: scene load failed - {0}. Remaining in Loading.", ex.Message);
                return;
            }

            if (!ct.IsCancellationRequested)
                TransitionTo(GameState.Menu);
        }

        /// <inheritdoc/>
        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                Log.Warn(LogCat.Boot, "PauseGame called from invalid state '{0}'. Ignoring.", CurrentState);
                return;
            }

            InputManager?.SetEnabled(false);
            UISystem?.ShowScreen(ScreenId.Pause);
            TransitionTo(GameState.Paused);
        }

        /// <inheritdoc/>
        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                Log.Warn(LogCat.Boot, "ResumeGame called from invalid state '{0}'. Ignoring.", CurrentState);
                return;
            }

            UISystem?.HideScreen(ScreenId.Pause);
            InputManager?.SetEnabled(true);
            TransitionTo(GameState.Playing);
        }

        /// <inheritdoc/>
        public void QuitGame()
        {
            PersistCurrentState();
            Log.Info(LogCat.Boot, "Application quit requested.");
            Application.Quit();
        }

        // ------------------------------------------------------------------ Unity messages

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // DontDestroyOnLoad is only meaningful (and valid) during actual play
            // sessions. In EditMode unit tests Application.isPlaying is false,
            // and calling it outside play mode can trigger an internal Unity error
            // that fires OnDestroy, immediately clearing Instance again.
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (UISystem != null)
            {
                UISystem.OnResumeRequested       -= ResumeGame;
                UISystem.OnReturnToMenuRequested -= HandleReturnToMenuRequested;
            }

            Log.GetGameState = null;
        }

        // No allocation in Update: ConsumePausePressed returns a bool (struct).
        private void Update()
        {
            if (CurrentState == GameState.Playing
                && InputManager != null
                && InputManager.ConsumePausePressed())
            {
                PauseGame();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CurrentState == GameState.Playing)
            {
                Log.Info(LogCat.Boot, "OS pause received while playing; auto-saving and pausing.");
                PersistCurrentState();
                PauseGame();
            }
        }

        private void OnApplicationQuit()
        {
            if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
                PersistCurrentState();
        }

        // ------------------------------------------------------------------ private helpers

        private async Task RunBootAsync(CancellationToken ct)
        {
            TransitionTo(GameState.Loading);

            try
            {
                await SceneLoader.LoadMenuAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(LogCat.Boot, "Boot: failed to load Menu scene - {0}.", ex.Message);
                return;
            }

            if (!ct.IsCancellationRequested)
                TransitionTo(GameState.Menu);
        }

        private async Task FallBackToMenuAsync(CancellationToken ct)
        {
            try
            {
                await SceneLoader.LoadMenuAsync(ct);
                if (!ct.IsCancellationRequested)
                    TransitionTo(GameState.Menu);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Error(LogCat.Boot, "Fallback to Menu also failed: {0}.", ex.Message);
            }
        }

        /// <summary>
        /// Re-persists the current save data so the timestamp is updated.
        /// TODO(phase6): extend to collect live ship-position data from the
        ///               ship controller before writing.
        /// </summary>
        private void PersistCurrentState()
        {
            if (SaveSystem == null)
                return;

            try
            {
                SaveData data = SaveSystem.Load();
                SaveSystem.Save(data);
            }
            catch (Exception ex)
            {
                // Per spec: log the error but never block quit or flow on save failure.
                Log.Error(LogCat.Boot, "PersistCurrentState failed: {0}.", ex.Message);
            }
        }

        private void TransitionTo(GameState next)
        {
            if (!IsValidTransition(CurrentState, next))
            {
                Log.Warn(LogCat.Boot, "Rejected invalid state transition: {0} -> {1}.", CurrentState, next);
                return;
            }

            GameState previous = CurrentState;
            PreviousState = previous;
            CurrentState  = next;

            Log.Info(LogCat.Boot, "GameState: {0} -> {1}", previous, next);
            StateChanged?.Invoke(previous, next);
        }

        // Encodes the allowed transition table from docs/systems/game-manager.md.
        private static bool IsValidTransition(GameState from, GameState to)
        {
            return (from, to) switch
            {
                (GameState.Boot,    GameState.Loading) => true,
                (GameState.Loading, GameState.Menu)    => true,
                (GameState.Menu,    GameState.Loading) => true,
                (GameState.Loading, GameState.Playing) => true,
                (GameState.Playing, GameState.Paused)  => true,
                (GameState.Paused,  GameState.Playing) => true,
                (GameState.Paused,  GameState.Loading) => true,
                (GameState.Playing, GameState.Loading) => true,
                _                                      => false,
            };
        }

        private void HandleReturnToMenuRequested()
        {
            _ = ReturnToMenuAsync(destroyCancellationToken);
        }
    }
}
