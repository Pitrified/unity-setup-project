namespace Game.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Game.Systems;
    using Game.UI;

    /// <summary>
    /// Async scene-transition service. Lives in the Persistent scene; owned by GameManager.
    /// All transitions block input and display the loading overlay for at least
    /// <see cref="MinDisplaySeconds"/> to avoid a flicker on fast hardware.
    /// </summary>
    public sealed class SceneLoader : MonoBehaviour, ISceneLoader
    {
        /// <summary>Minimum wall-clock seconds the loading overlay is visible per transition.</summary>
        internal const float MinDisplaySeconds = 0.5f;

        // ----------------------------------------------------------------- injected deps

        private IInputManager _input;
        private IUISystem _ui;

        // ----------------------------------------------------------------- async state

        // _isLoading and _inflight are set/cleared on the Unity main thread only.
        private bool _isLoading;
        private Task _inflight;

        // ----------------------------------------------------------------- ISceneLoader

        /// <inheritdoc/>
        public bool IsLoading => _isLoading;

        /// <inheritdoc/>
        public event Action<float> Progress;

        // ----------------------------------------------------------------- initializer

        /// <summary>
        /// Wires the dependencies needed for scene transitions.
        /// Called by GameManager during bootstrap.
        /// </summary>
        public void Initialize(IInputManager input, IUISystem ui)
        {
            _input = input;
            _ui = ui;
            Log.Info(LogCat.Scene, "SceneLoader initialized.");
        }

        // ----------------------------------------------------------------- public API

        /// <inheritdoc/>
        public Task LoadMenuAsync(CancellationToken ct = default)
        {
            // Idempotent: Menu is already the active content scene.
            if (!_isLoading
                && SceneManager.GetSceneByName(SceneNames.Menu).isLoaded
                && !SceneManager.GetSceneByName(SceneNames.Game).isLoaded)
            {
                return Task.CompletedTask;
            }

            if (_isLoading)
                return _inflight;

            _inflight = RunTransitionAsync(LoadMenuInternalAsync, ct);
            return _inflight;
        }

        /// <inheritdoc/>
        public Task LoadGameAsync(CancellationToken ct = default)
        {
            // Idempotent: Game is already loaded.
            if (!_isLoading && SceneManager.GetSceneByName(SceneNames.Game).isLoaded)
                return Task.CompletedTask;

            if (_isLoading)
                return _inflight;

            _inflight = RunTransitionAsync(LoadGameInternalAsync, ct);
            return _inflight;
        }

        /// <inheritdoc/>
        public Task UnloadGameAsync(CancellationToken ct = default)
        {
            // No-op if Game is not loaded.
            if (!_isLoading && !SceneManager.GetSceneByName(SceneNames.Game).isLoaded)
                return Task.CompletedTask;

            if (_isLoading)
                return _inflight;

            _inflight = RunTransitionAsync(UnloadGameInternalAsync, ct);
            return _inflight;
        }

        /// <inheritdoc/>
        public Task ReloadGameAsync(CancellationToken ct = default)
        {
            if (_isLoading)
                return _inflight;

            _inflight = RunTransitionAsync(ReloadGameInternalAsync, ct);
            return _inflight;
        }

        // ----------------------------------------------------------------- transition wrapper

        private async Task RunTransitionAsync(Func<CancellationToken, Task> body, CancellationToken ct)
        {
            _isLoading = true;
            _input?.SetEnabled(false);
            _ui?.ShowLoading();
            RaiseProgress(0f);

            float startTime = Time.realtimeSinceStartup;

            try
            {
                await body(ct);
            }
            catch (OperationCanceledException)
            {
                Log.Warn(LogCat.Scene, "Scene transition cancelled; cleaning up partial state.");
                await SafeUnloadSceneAsync(SceneNames.Game);
            }
            catch (Exception ex)
            {
                Log.Error(LogCat.Scene, "Scene transition failed: {0}; falling back to Menu.", ex.Message);
                await SafeLoadMenuFallbackAsync();
            }
            finally
            {
                // Always enforce minimum display time so the overlay does not flicker.
                float elapsed = Time.realtimeSinceStartup - startTime;
                float remaining = MinDisplaySeconds - elapsed;
                if (remaining > 0f)
                    await Task.Delay(TimeSpan.FromSeconds(remaining));

                RaiseProgress(1f);
                _ui?.HideLoading();
                _input?.SetEnabled(true);
                _isLoading = false;
                _inflight = null;
            }
        }

        // ----------------------------------------------------------------- individual transitions

        private async Task LoadMenuInternalAsync(CancellationToken ct)
        {
            if (SceneManager.GetSceneByName(SceneNames.Game).isLoaded)
            {
                RaiseProgress(0.1f);
                await UnloadSceneAsync(SceneNames.Game, ct);
                RaiseProgress(0.4f);
            }

            if (!SceneManager.GetSceneByName(SceneNames.Menu).isLoaded)
            {
                RaiseProgress(0.5f);
                await LoadSceneAdditiveAsync(SceneNames.Menu, ct);
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNames.Menu));
            RaiseProgress(0.9f);
            Log.Info(LogCat.Scene, "Menu scene ready.");
        }

        private async Task LoadGameInternalAsync(CancellationToken ct)
        {
            if (SceneManager.GetSceneByName(SceneNames.Menu).isLoaded)
            {
                RaiseProgress(0.1f);
                await UnloadSceneAsync(SceneNames.Menu, ct);
                RaiseProgress(0.4f);
            }

            RaiseProgress(0.5f);
            await LoadSceneAdditiveAsync(SceneNames.Game, ct);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNames.Game));
            RaiseProgress(0.9f);
            Log.Info(LogCat.Scene, "Game scene ready.");
        }

        private async Task UnloadGameInternalAsync(CancellationToken ct)
        {
            RaiseProgress(0.5f);
            await UnloadSceneAsync(SceneNames.Game, ct);
            RaiseProgress(0.9f);
            Log.Info(LogCat.Scene, "Game scene unloaded.");
        }

        private async Task ReloadGameInternalAsync(CancellationToken ct)
        {
            if (SceneManager.GetSceneByName(SceneNames.Game).isLoaded)
            {
                RaiseProgress(0.1f);
                await UnloadSceneAsync(SceneNames.Game, ct);
                RaiseProgress(0.5f);
            }

            await LoadSceneAdditiveAsync(SceneNames.Game, ct);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNames.Game));
            RaiseProgress(0.9f);
            Log.Info(LogCat.Scene, "Game scene reloaded.");
        }

        // ----------------------------------------------------------------- scene operation helpers

        private static async Task LoadSceneAdditiveAsync(string sceneName, CancellationToken ct)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                throw new InvalidOperationException(
                    $"SceneManager returned null for '{sceneName}'. Ensure it is registered in Build Settings.");
            }

            op.allowSceneActivation = true;
            while (!op.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private static async Task UnloadSceneAsync(string sceneName, CancellationToken ct)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
                return; // Scene was not loaded; nothing to do.

            while (!op.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        // ----------------------------------------------------------------- error-path helpers

        private async Task SafeUnloadSceneAsync(string sceneName)
        {
            try
            {
                await UnloadSceneAsync(sceneName, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Warn(LogCat.Scene, "Safe-unload of '{0}' failed: {1}", sceneName, ex.Message);
            }
        }

        private async Task SafeLoadMenuFallbackAsync()
        {
            try
            {
                await SafeUnloadSceneAsync(SceneNames.Game);

                if (!SceneManager.GetSceneByName(SceneNames.Menu).isLoaded)
                    await LoadSceneAdditiveAsync(SceneNames.Menu, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Error(LogCat.Scene, "Fallback to Menu failed: {0}", ex.Message);
            }
        }

        // ----------------------------------------------------------------- internal test helpers

        /// <summary>
        /// Test-only entry point. Puts the loader into an in-progress state so unit tests can
        /// exercise the concurrent-call guard without needing a real scene load.
        /// </summary>
        internal void ForceSetInflight(Task task)
        {
            _isLoading = true;
            _inflight = task;
        }

        // ----------------------------------------------------------------- private utilities

        private void RaiseProgress(float value)
        {
            Progress?.Invoke(value);
        }
    }
}
