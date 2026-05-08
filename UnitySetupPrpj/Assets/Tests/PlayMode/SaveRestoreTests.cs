// PlayMode test: save → reload scene → Continue restores ship position.
//
// Flow:
//   1. Boot → Menu → StartNewGameAsync → Playing          (save deleted by StartNewGame)
//   2. Teleport ship to a known far-away position
//   3. ReturnToMenuAsync                                   (PersistCurrentState runs first,
//                                                          before Game scene unloads)
//   4. ContinueAsync → Game scene reloaded fresh
//   5. GameSceneWiring.Start() → RegisterShip() reads save → Teleport(savedPos, savedYaw)
//   6. Assert new ship instance is at the saved position
//
// The test writes a real save file to Application.persistentDataPath.
// Each run starts with StartNewGameAsync which deletes any prior save,
// so runs are isolated from each other.
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
    /// PlayMode test verifying that the save-and-restore cycle correctly
    /// persists and rehydrates the ship's world position and yaw.
    /// </summary>
    public sealed class SaveRestoreTests
    {
        private const int   WaitFrames  = 900;
        private const float PosEpsilon  = 0.1f;
        private const float YawEpsilon  = 0.1f;

        /// <summary>
        /// Far-from-origin position used as the canonical "saved" location.
        /// The integer values round-trip through JSON without floating-point error.
        /// </summary>
        private static readonly Vector3 SavedPos = new Vector3(100f, 0f, 100f);
        private const float SavedYaw = 90f;

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

        // ------------------------------------------------------------------ test

        /// <summary>
        /// Teleports the ship to a known position, returns to Menu (which triggers a save),
        /// then continues (which reloads the Game scene and restores the position from save).
        /// Asserts the freshly loaded ship is within <see cref="PosEpsilon"/> metres of the
        /// saved location and within <see cref="YawEpsilon"/> degrees of the saved yaw.
        /// </summary>
        [UnityTest]
        public IEnumerator SaveReloadContinue_RestoresShipPosition()
        {
            // ---- Arrange: boot to Playing (StartNewGameAsync deletes any prior save) ---- //

            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            yield return WaitForState(GameState.Menu);

            var startTask = GameManager.Instance.StartNewGameAsync();
            while (!startTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Playing);

            // One frame so that GameSceneWiring.Start() runs and RegisterShip() wires
            // _activeShip into GameManager before we move it.
            yield return null;

            ShipController[] ships =
                Object.FindObjectsByType<ShipController>(FindObjectsInactive.Exclude);
            Assert.AreEqual(1, ships.Length, "Exactly one ShipController must exist in the Game scene");

            // ---- Act: teleport to known position then return to Menu (triggers save) ---- //

            ships[0].Teleport(SavedPos, SavedYaw);

            var returnTask = GameManager.Instance.ReturnToMenuAsync();
            while (!returnTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Menu);
            Assert.IsTrue(GameManager.Instance.SaveSystem.HasSave,
                "A save file must exist after ReturnToMenuAsync");

            // ---- Act: Continue → Game scene reloaded → RegisterShip restores position ---- //

            var continueTask = GameManager.Instance.ContinueAsync();
            while (!continueTask.IsCompleted)
                yield return null;

            yield return WaitForState(GameState.Playing);

            // One frame so that GameSceneWiring.Start() runs and RegisterShip() teleports
            // the freshly spawned ship to the saved coordinates.
            yield return null;

            // ---- Assert ---- //

            ShipController[] restoredShips =
                Object.FindObjectsByType<ShipController>(FindObjectsInactive.Exclude);
            Assert.AreEqual(1, restoredShips.Length,
                "Exactly one ShipController must exist after scene reload");

            ShipController restored = restoredShips[0];

            float posDelta = Vector3.Distance(restored.transform.position, SavedPos);
            float yawDelta = Mathf.Abs(Mathf.DeltaAngle(restored.transform.eulerAngles.y, SavedYaw));

            Assert.That(posDelta, Is.LessThan(PosEpsilon),
                $"Ship position must be restored to {SavedPos} (actual={restored.transform.position}, delta={posDelta:F4} m)");
            Assert.That(yawDelta, Is.LessThan(YawEpsilon),
                $"Ship yaw must be restored to {SavedYaw} deg (actual={restored.transform.eulerAngles.y:F4}, delta={yawDelta:F4} deg)");
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
