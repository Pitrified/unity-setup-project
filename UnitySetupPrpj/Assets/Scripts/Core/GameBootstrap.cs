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
    /// Single entry point for the application. Lives only in <c>Boot.unity</c> and runs once.
    /// Applies runtime defaults, loads the Persistent scene additively, wires all systems
    /// into <see cref="GameManager"/>, hands off control, and unloads the Boot scene.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        // ------------------------------------------------------------------ constants

        /// <summary>Name of the quality level applied at startup.</summary>
        private const string MobileQualityName = "Mobile";

        // ------------------------------------------------------------------ fatal-error state

        private bool _fatalError;
        private string _fatalMessage;

        // ------------------------------------------------------------------ Unity lifecycle

        // async void is the one exception to the no-async-void rule: Unity event handlers.
        private async void Start()
        {
            ApplyRuntimeDefaults();
            await BootAsync(destroyCancellationToken);
        }

        // ------------------------------------------------------------------ runtime defaults

        private static void ApplyRuntimeDefaults()
        {
            Application.targetFrameRate = 60;

            // Allow landscape left and right; disable portrait.
            Screen.autorotateToLandscapeLeft      = true;
            Screen.autorotateToLandscapeRight     = true;
            Screen.autorotateToPortrait           = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.AutoRotation;

            int mobileIndex = FindQualityIndex(MobileQualityName);
            if (mobileIndex >= 0)
            {
                QualitySettings.SetQualityLevel(mobileIndex, applyExpensiveChanges: true);
            }
            else
            {
                Log.Warn(LogCat.Boot, "Quality level '{0}' not found; keeping current level.", MobileQualityName);
            }

            Log.Info(LogCat.Boot, "Runtime defaults applied: fps=60, orientation=LandscapeLeft|Right, quality={0}.", MobileQualityName);
        }

        private static int FindQualityIndex(string levelName)
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == levelName)
                    return i;
            }
            return -1;
        }

        // ------------------------------------------------------------------ boot sequence

        private async Task BootAsync(CancellationToken ct)
        {
            // ---- load Persistent scene additively ---- //

            AsyncOperation loadOp;
            try
            {
                loadOp = SceneManager.LoadSceneAsync(SceneNames.Persistent, LoadSceneMode.Additive);
            }
            catch (Exception ex)
            {
                SetFatal($"Could not begin loading '{SceneNames.Persistent}' scene: {ex.Message}");
                return;
            }

            if (loadOp == null)
            {
                SetFatal($"SceneManager returned null for '{SceneNames.Persistent}'. Verify Build Settings.");
                return;
            }

            while (!loadOp.isDone)
            {
                if (ct.IsCancellationRequested)
                    return;

                await Task.Yield();
            }

            if (ct.IsCancellationRequested)
                return;

            // ---- find GameManager ---- //

            var gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager == null)
            {
                SetFatal($"GameManager not found in '{SceneNames.Persistent}' scene.");
                return;
            }

            // ---- find all required systems ---- //

            var sceneLoader  = FindAnyObjectByType<SceneLoader>();
            var saveSystem   = FindAnyObjectByType<SaveSystem>();
            var inputManager = FindAnyObjectByType<InputManager>();
            var audioManager = FindAnyObjectByType<AudioManager>();
            var uiSystem     = FindAnyObjectByType<UISystem>();

            if (sceneLoader  == null) { SetFatal($"SceneLoader missing from '{SceneNames.Persistent}'."); return; }
            if (saveSystem   == null) { SetFatal($"SaveSystem missing from '{SceneNames.Persistent}'."); return; }
            if (inputManager == null) { SetFatal($"InputManager missing from '{SceneNames.Persistent}'."); return; }
            if (audioManager == null) { SetFatal($"AudioManager missing from '{SceneNames.Persistent}'."); return; }
            if (uiSystem     == null) { SetFatal($"UISystem missing from '{SceneNames.Persistent}'."); return; }

            // ---- wire and hand off ---- //

            gameManager.Initialize(sceneLoader, saveSystem, inputManager, audioManager, uiSystem);
            gameManager.OnBootComplete();

            Log.Info(LogCat.Boot, "Handoff complete. Requesting Boot scene unload.");

            // Fire-and-forget: this GameObject is destroyed when Boot unloads.
            SceneManager.UnloadSceneAsync(SceneNames.Boot);
        }

        // ------------------------------------------------------------------ fatal-error handling

        private void SetFatal(string message)
        {
            _fatalError   = true;
            _fatalMessage = message;
            Log.Error(LogCat.Boot, "[GameBootstrap] Fatal: {0}", message);
        }

        // OnGUI is intentionally used here - the one permitted use in this project.
        // See docs/systems/game-bootstrap.md "Failure handling".
        private void OnGUI()
        {
            if (!_fatalError)
                return;

            GUI.color = Color.red;
            GUI.Label(
                new Rect(20f, 20f, Screen.width - 40f, 80f),
                $"[FATAL] GameBootstrap: {_fatalMessage}");
        }
    }
}
