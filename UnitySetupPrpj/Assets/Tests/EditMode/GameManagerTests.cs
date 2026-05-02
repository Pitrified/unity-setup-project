// EditMode tests for Game.Core.GameManager state machine.
//
// The async public methods (StartNewGameAsync, ContinueAsync, ReturnToMenuAsync)
// call through to ISceneLoader, which is stubbed here with a fake that returns
// Task.CompletedTask. Because an already-completed Task's awaiter reports
// IsCompleted = true, continuations run synchronously on the caller's thread,
// so the full async flow completes before the test assertion.
//
// Update(), OnApplicationPause, and OnApplicationQuit are Unity messages that
// do not fire in EditMode; their logic is covered indirectly through the public API.
//
// Run via Window > General > Test Runner > EditMode.
namespace Game.Tests.EditMode
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Core;
    using Game.Systems;
    using Game.UI;

    // ======================================================================
    // Test doubles (private to this file)
    // ======================================================================

    internal sealed class FakeSceneLoader : ISceneLoader
    {
        public bool IsLoading => false;

        // Events with empty add/remove: tests never subscribe to Progress here.
        public event Action<float> Progress
        {
            add { }
            remove { }
        }

        public Task LoadMenuAsync(CancellationToken ct = default)  => Task.CompletedTask;
        public Task LoadGameAsync(CancellationToken ct = default)  => Task.CompletedTask;
        public Task UnloadGameAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ReloadGameAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    internal sealed class FakeSaveSystem : ISaveSystem
    {
        public bool HasSave { get; set; }
        public SaveData Load() => SaveData.CreateDefault();
        public void Save(SaveData data) { HasSave = true; }
        public void DeleteSave() { HasSave = false; }
    }

    internal sealed class FakeInputManager : IInputManager
    {
        public bool PausePressed { get; set; }
        public MovementInput GetMovementInput() => default;
        public bool ConsumePausePressed() { bool v = PausePressed; PausePressed = false; return v; }
        public void SetEnabled(bool enabled) { }
    }

    internal sealed class FakeAudioManager : IAudioManager
    {
        public void PlayMusic(AudioClip clip, float fadeSeconds = 1f) { }
        public void StopMusic(float fadeSeconds = 1f) { }
        public void PlaySfx(AudioClip clip, float volumeScale = 1f) { }
        public void SetMusicVolume(float v) { }
        public void SetSfxVolume(float v) { }
        public void SetMasterVolume(float v) { }
    }

    internal sealed class FakeUISystem : IUISystem
    {
        public bool IsAnyOverlayOpen => false;
        public void ShowLoading() { }
        public void HideLoading() { }
        public void ShowScreen(ScreenId id) { }
        public void HideScreen(ScreenId id) { }
        public event Action OnResumeRequested;
        public event Action OnReturnToMenuRequested;
        public void FireResumeRequested() => OnResumeRequested?.Invoke();
        public void FireReturnToMenuRequested() => OnReturnToMenuRequested?.Invoke();
    }

    // ======================================================================
    // Tests
    // ======================================================================

    [TestFixture]
    public sealed class GameManagerTests
    {
        private GameObject      _go;
        private GameManager     _gm;
        private FakeSceneLoader _fakeLoader;
        private FakeSaveSystem  _fakeSave;
        private FakeInputManager _fakeInput;
        private FakeAudioManager _fakeAudio;
        private FakeUISystem    _fakeUI;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("GameManager");
            _gm = _go.AddComponent<GameManager>();

            // In EditMode (Application.isPlaying == false) Unity does not dispatch
            // Awake automatically when AddComponent is called. Invoke it explicitly
            // so the singleton Instance is set before any test assertion.
            typeof(GameManager)
                .GetMethod("Awake",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(_gm, null);

            _fakeLoader = new FakeSceneLoader();
            _fakeSave   = new FakeSaveSystem();
            _fakeInput  = new FakeInputManager();
            _fakeAudio  = new FakeAudioManager();
            _fakeUI     = new FakeUISystem();

            _gm.Initialize(_fakeLoader, _fakeSave, _fakeInput, _fakeAudio, _fakeUI);
        }

        [TearDown]
        public void TearDown()
        {
            // In EditMode, DestroyImmediate does not dispatch OnDestroy via the
            // Unity message pump (symmetric with Awake not firing on AddComponent).
            // Invoke it explicitly so Instance is genuinely null before the next test.
            if (_go != null && _gm != null)
            {
                typeof(GameManager)
                    .GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_gm, null);
            }
            UnityEngine.Object.DestroyImmediate(_go);
        }

        // Helper: drives through OnBootComplete to reach Menu.
        // With FakeSceneLoader returning Task.CompletedTask, the entire
        // async boot sequence completes synchronously.
        private void BootToMenu()
        {
            _gm.OnBootComplete();
            // After sync completion: Boot -> Loading -> Menu
        }

        // Helper: advances from Menu to Playing.
        private async Task BootToPlayingAsync()
        {
            BootToMenu();
            await _gm.StartNewGameAsync();
        }

        // Helper: advances from Menu through Playing into Paused.
        private async Task BootToPausedAsync()
        {
            await BootToPlayingAsync();
            _gm.PauseGame();
        }

        // ------------------------------------------------------------------ initial state

        [Test]
        public void InitialState_Is_Boot()
        {
            Assert.AreEqual(GameState.Boot, _gm.CurrentState);
        }

        [Test]
        public void PreviousState_Initially_MatchesCurrentState()
        {
            Assert.AreEqual(_gm.CurrentState, _gm.PreviousState);
        }

        // ------------------------------------------------------------------ OnBootComplete

        [Test]
        public void OnBootComplete_Transitions_To_Menu()
        {
            BootToMenu();

            Assert.AreEqual(GameState.Menu, _gm.CurrentState);
        }

        [Test]
        public void OnBootComplete_Sets_PreviousState_To_Loading()
        {
            // The last leg of boot is Loading -> Menu, so previous = Loading.
            BootToMenu();

            Assert.AreEqual(GameState.Loading, _gm.PreviousState);
        }

        // ------------------------------------------------------------------ StateChanged event

        [Test]
        public void StateChanged_Fires_On_Each_Transition()
        {
            int fireCount = 0;
            _gm.StateChanged += (_, __) => fireCount++;

            BootToMenu(); // Boot->Loading and Loading->Menu = 2 transitions

            Assert.AreEqual(2, fireCount);
        }

        [Test]
        public void StateChanged_Delivers_Correct_Previous_And_Next()
        {
            GameState capturedPrev = default;
            GameState capturedNext = default;
            _gm.StateChanged += (prev, next) => { capturedPrev = prev; capturedNext = next; };

            _gm.OnBootComplete(); // last fired transition is Loading -> Menu

            Assert.AreEqual(GameState.Loading, capturedPrev);
            Assert.AreEqual(GameState.Menu,    capturedNext);
        }

        // ------------------------------------------------------------------ invalid transitions

        [Test]
        public void InvalidTransition_DoesNotChangeState()
        {
            // Boot -> Paused is not allowed.
            GameState before = _gm.CurrentState;
            // PauseGame from Boot is a no-op.
            _gm.PauseGame();

            Assert.AreEqual(before, _gm.CurrentState);
        }

        [Test]
        public void InvalidTransition_DoesNotFireStateChanged()
        {
            bool fired = false;
            _gm.StateChanged += (_, __) => fired = true;

            _gm.PauseGame(); // invalid from Boot

            Assert.IsFalse(fired);
        }

        [Test]
        public void ResumeGame_From_NonPaused_DoesNotChangeState()
        {
            BootToMenu();
            GameState before = _gm.CurrentState; // Menu

            _gm.ResumeGame(); // invalid from Menu

            Assert.AreEqual(before, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ PauseGame / ResumeGame

        [Test]
        public async Task PauseGame_From_Playing_Transitions_To_Paused()
        {
            await BootToPlayingAsync();

            _gm.PauseGame();

            Assert.AreEqual(GameState.Paused, _gm.CurrentState);
        }

        [Test]
        public async Task PauseGame_Sets_PreviousState_To_Playing()
        {
            await BootToPlayingAsync();

            _gm.PauseGame();

            Assert.AreEqual(GameState.Playing, _gm.PreviousState);
        }

        [Test]
        public async Task ResumeGame_From_Paused_Transitions_To_Playing()
        {
            await BootToPausedAsync();

            _gm.ResumeGame();

            Assert.AreEqual(GameState.Playing, _gm.CurrentState);
        }

        [Test]
        public async Task ResumeGame_Sets_PreviousState_To_Paused()
        {
            await BootToPausedAsync();

            _gm.ResumeGame();

            Assert.AreEqual(GameState.Paused, _gm.PreviousState);
        }

        // ------------------------------------------------------------------ StartNewGameAsync

        [Test]
        public async Task StartNewGameAsync_From_Menu_Transitions_To_Playing()
        {
            BootToMenu();

            await _gm.StartNewGameAsync();

            Assert.AreEqual(GameState.Playing, _gm.CurrentState);
        }

        [Test]
        public async Task StartNewGameAsync_DeletesSave()
        {
            BootToMenu();
            _fakeSave.HasSave = true;

            await _gm.StartNewGameAsync();

            Assert.IsFalse(_fakeSave.HasSave);
        }

        [Test]
        public async Task StartNewGameAsync_FromInvalidState_DoesNotTransition()
        {
            // Still in Boot; cannot start a new game.
            GameState before = _gm.CurrentState;

            await _gm.StartNewGameAsync();

            Assert.AreEqual(before, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ ContinueAsync

        [Test]
        public async Task ContinueAsync_WithSave_From_Menu_Transitions_To_Playing()
        {
            BootToMenu();
            _fakeSave.HasSave = true;

            await _gm.ContinueAsync();

            Assert.AreEqual(GameState.Playing, _gm.CurrentState);
        }

        [Test]
        public async Task ContinueAsync_WithNoSave_DoesNotTransition()
        {
            BootToMenu();
            _fakeSave.HasSave = false;

            GameState before = _gm.CurrentState;
            await _gm.ContinueAsync();

            Assert.AreEqual(before, _gm.CurrentState);
        }

        [Test]
        public async Task ContinueAsync_FromInvalidState_DoesNotTransition()
        {
            // Still in Boot with a save; should still not transition.
            _fakeSave.HasSave = true;
            GameState before = _gm.CurrentState;

            await _gm.ContinueAsync();

            Assert.AreEqual(before, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ ReturnToMenuAsync

        [Test]
        public async Task ReturnToMenuAsync_From_Playing_Transitions_To_Menu()
        {
            await BootToPlayingAsync();

            await _gm.ReturnToMenuAsync();

            Assert.AreEqual(GameState.Menu, _gm.CurrentState);
        }

        [Test]
        public async Task ReturnToMenuAsync_From_Paused_Transitions_To_Menu()
        {
            await BootToPausedAsync();

            await _gm.ReturnToMenuAsync();

            Assert.AreEqual(GameState.Menu, _gm.CurrentState);
        }

        [Test]
        public async Task ReturnToMenuAsync_FromInvalidState_DoesNotTransition()
        {
            BootToMenu();
            GameState before = _gm.CurrentState; // Menu

            await _gm.ReturnToMenuAsync(); // invalid from Menu

            Assert.AreEqual(before, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ UISystem event wiring

        [Test]
        public async Task UISystem_OnResumeRequested_ResumesGame()
        {
            await BootToPausedAsync();

            _fakeUI.FireResumeRequested();

            Assert.AreEqual(GameState.Playing, _gm.CurrentState);
        }

        [Test]
        public async Task UISystem_OnReturnToMenuRequested_ReturnsToMenu()
        {
            await BootToPlayingAsync();

            _fakeUI.FireReturnToMenuRequested();

            Assert.AreEqual(GameState.Menu, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ IsValidTransition coverage (all allowed pairs via public API)

        [Test]
        public void IsValidTransition_AllAllowedPairs_DoNotFireWarningsAndSucceed()
        {
            // Boot -> Loading -> Menu (via OnBootComplete)
            BootToMenu();
            Assert.AreEqual(GameState.Menu, _gm.CurrentState);

            // Menu -> Loading -> Playing (via StartNewGameAsync)
            _gm.StartNewGameAsync().GetAwaiter().GetResult();
            Assert.AreEqual(GameState.Playing, _gm.CurrentState);

            // Playing -> Paused (via PauseGame)
            _gm.PauseGame();
            Assert.AreEqual(GameState.Paused, _gm.CurrentState);

            // Paused -> Playing (via ResumeGame)
            _gm.ResumeGame();
            Assert.AreEqual(GameState.Playing, _gm.CurrentState);

            // Playing -> Loading -> Menu (via ReturnToMenuAsync)
            _gm.ReturnToMenuAsync().GetAwaiter().GetResult();
            Assert.AreEqual(GameState.Menu, _gm.CurrentState);
        }

        // ------------------------------------------------------------------ singleton

        [Test]
        public void Instance_IsSet_AfterAwake()
        {
            Assert.IsNotNull(GameManager.Instance);
            Assert.AreSame(_gm, GameManager.Instance);
        }

        [Test]
        public void Instance_IsCleared_AfterDestroy()
        {
            // DestroyImmediate does not dispatch OnDestroy in EditMode; invoke manually.
            typeof(GameManager)
                .GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(_gm, null);

            UnityEngine.Object.DestroyImmediate(_go);
            _go = null; // prevent double-invoke in TearDown

            Assert.IsNull(GameManager.Instance);
        }

    }
}
