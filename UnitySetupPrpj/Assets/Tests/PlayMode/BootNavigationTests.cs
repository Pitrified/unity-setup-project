// PlayMode test: full Boot → Menu → Game → Menu loop, asserts no leaked GameObjects.
//
// These tests load real scenes via Boot.unity and exercise the full bootstrap
// and navigation stack. Time.captureDeltaTime is set to 0.016 so Unity advances
// time at a fixed rate each frame, making async scene loads deterministic.
//
// Run via Window > General > Test Runner > PlayMode.
namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;

    /// <summary>
    /// PlayMode smoke tests verifying the full application navigation loop
    /// completes without errors and without leaking GameObjects.
    /// Scenes are loaded for real; a fixed simulated frame rate keeps tests deterministic.
    /// </summary>
    public sealed class BootNavigationTests
    {
        /// <summary>Maximum frames to wait before a WaitForState assertion fails.</summary>
        private const int WaitFrames = 900; // 900 frames × 16 ms = ~14.4 s simulated

        // ------------------------------------------------------------------ setup / teardown

        [SetUp]
        public void SetUp()
        {
            // Advance exactly 16 ms of game time per frame so tests are deterministic.
            Time.captureDeltaTime = 0.016f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        // ------------------------------------------------------------------ tests

        /// <summary>
        /// Full Boot → Menu → Game → Menu navigation loop.
        /// Asserts that no root GameObjects are added to the Persistent scene
        /// after the round trip, confirming that systems are not duplicated across
        /// scene transitions.
        /// </summary>
        [UnityTest]
        public IEnumerator BootMenuGameMenuLoop_NoLeakedGameObjects()
        {
            // ---- Arrange: boot from Boot scene ---- //

            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);

            Assert.IsNotNull(GameManager.Instance, "GameManager.Instance must exist after boot");
            Assert.AreEqual(GameState.Menu, GameManager.Instance.CurrentState,
                "State must be Menu after boot sequence completes");

            int persistentRootAtMenu = GetPersistentRootCount();

            // ---- Act: Menu → Game ---- //

            var startTask = GameManager.Instance.StartNewGameAsync();
            while (!startTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Playing);
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState,
                "State must be Playing after StartNewGameAsync");

            // ---- Act: Game → Menu ---- //

            var returnTask = GameManager.Instance.ReturnToMenuAsync();
            while (!returnTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Menu);

            // ---- Assert ---- //

            Assert.AreEqual(GameState.Menu, GameManager.Instance.CurrentState,
                "State must return to Menu after ReturnToMenuAsync");

            Assert.AreEqual(
                persistentRootAtMenu,
                GetPersistentRootCount(),
                "Persistent scene root object count must not change after Boot->Menu->Game->Menu loop");

            GameManager[] allGMs = Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include);
            Assert.AreEqual(1, allGMs.Length,
                "Exactly one GameManager instance must exist after the loop");
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>
        /// Yields frames until <see cref="GameManager.Instance"/> reports <paramref name="target"/>,
        /// or fails the test after <see cref="WaitFrames"/> frames.
        /// </summary>
        private static IEnumerator WaitForState(GameState target)
        {
            for (int frame = 0; frame < WaitFrames; frame++)
            {
                if (GameManager.Instance != null
                    && GameManager.Instance.CurrentState == target)
                    yield break;

                yield return null;
            }

            string actual = GameManager.Instance?.CurrentState.ToString()
                ?? "null (no GameManager instance)";
            Assert.Fail(
                $"Timed out waiting for GameState.{target} after {WaitFrames} frames. "
                + $"Actual state: {actual}");
        }

        /// <summary>
        /// Returns the number of root GameObjects in the Persistent scene.
        /// Returns 0 if the scene is not currently loaded.
        /// </summary>
        private static int GetPersistentRootCount()
        {
            Scene scene = SceneManager.GetSceneByName(SceneNames.Persistent);
            if (!scene.IsValid() || !scene.isLoaded)
                return 0;

            return scene.GetRootGameObjects().Length;
        }
    }
}
