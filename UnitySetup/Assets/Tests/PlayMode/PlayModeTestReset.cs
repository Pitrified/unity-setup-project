// Shared teardown for PlayMode tests that boot the game.
namespace Game.Tests.PlayMode
{
    using System.Collections;
    using UnityEngine;
    using Game.Core;

    /// <summary>
    /// Resets the state a booted game leaves behind between tests.
    /// <see cref="GameManager"/> is <c>DontDestroyOnLoad</c>, so it survives the next test's
    /// <c>LoadSceneMode.Single</c>: that test's own GameManager would then destroy itself as a
    /// duplicate, and a wait on the old instance's state would pass before the new
    /// Persistent scene has loaded.
    /// </summary>
    internal static class PlayModeTestReset
    {
        /// <summary>Destroys the surviving GameManager and waits one frame for it to go.</summary>
        public static IEnumerator DestroyPersistentGameManager()
        {
            if (GameManager.Instance != null)
                Object.Destroy(GameManager.Instance.gameObject);

            yield return null;
        }
    }
}
