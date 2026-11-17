// PlayMode test: pause → resume preserves avatar transform.
//
// The avatar spawns at zero speed and receives no input during the test.
// PauseGame() disables InputManager (returns default MovementInput = 0,0);
// with zero throttle and zero steering AvatarController.Tick() produces no
// position or rotation change, so transform must be bitwise identical before
// and after the pause window.
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
    using Game.Gameplay;

    /// <summary>
    /// PlayMode tests verifying that the pause/resume cycle does not alter
    /// the avatar's world transform.
    /// </summary>
    public sealed class PauseResumeTests
    {
        private const int WaitFrames   = 900;
        private const int PauseFrames  = 10;
        private const float Epsilon    = 0.001f;

        // ------------------------------------------------------------------ setup / teardown

        [SetUp]
        public void SetUp()
        {
            Time.captureDeltaTime = 0.016f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        /// <summary>Destroys the DontDestroyOnLoad GameManager so the next test boots fresh.</summary>
        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        // ------------------------------------------------------------------ tests

        /// <summary>
        /// Boots into the Game scene, pauses, advances <see cref="PauseFrames"/> frames,
        /// resumes, then asserts the avatar's position and yaw are unchanged.
        /// Verifies that disabling input during pause is sufficient to keep the
        /// kinematic avatar stationary.
        /// </summary>
        [UnityTest]
        public IEnumerator PauseResume_AvatarTransformPreserved()
        {
            // ---- Arrange: boot to Playing ---- //

            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);

            var startTask = GameManager.Instance.StartNewGameAsync();
            while (!startTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Playing);

            // One extra frame so that Start() callbacks on Game scene objects have run
            // (GameSceneWiring.Start wires InputManager into the avatar).
            yield return null;

            AvatarController[] avatars =
                Object.FindObjectsByType<AvatarController>(FindObjectsInactive.Exclude);
            Assert.AreEqual(1, avatars.Length, "Exactly one AvatarController must exist in the Game scene");

            AvatarController avatar = avatars[0];
            Vector3 positionBefore = avatar.transform.position;
            float   yawBefore      = avatar.transform.eulerAngles.y;

            // ---- Act: pause, hold, resume ---- //

            GameManager.Instance.PauseGame();
            Assert.AreEqual(GameState.Paused, GameManager.Instance.CurrentState,
                "State must be Paused immediately after PauseGame()");

            for (int i = 0; i < PauseFrames; i++)
                yield return null;

            GameManager.Instance.ResumeGame();
            Assert.AreEqual(GameState.Playing, GameManager.Instance.CurrentState,
                "State must be Playing immediately after ResumeGame()");

            // ---- Assert: transform unchanged ---- //

            float positionDelta = Vector3.Distance(avatar.transform.position, positionBefore);
            float yawDelta      = Mathf.Abs(Mathf.DeltaAngle(avatar.transform.eulerAngles.y, yawBefore));

            Assert.That(positionDelta, Is.LessThan(Epsilon),
                $"Avatar position must be unchanged after pause/resume (delta={positionDelta:F6} m)");
            Assert.That(yawDelta, Is.LessThan(Epsilon),
                $"Avatar yaw must be unchanged after pause/resume (delta={yawDelta:F6} deg)");
        }

        // ------------------------------------------------------------------ helpers

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
    }
}
