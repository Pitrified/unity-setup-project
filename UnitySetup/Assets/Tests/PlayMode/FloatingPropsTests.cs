namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;
    using Game.Gameplay.Water;

    /// <summary>
    /// The Game scene's floating props: after a few seconds of play every body lighter than water
    /// is still at the surface the water draws there (within a body's size), none sank, none flew off.
    /// </summary>
    public sealed class FloatingPropsTests
    {
        private const int WaitFrames = 900;
        private const float PlaySeconds = 6f;
        private const float SurfaceTolerance = 1.2f;

        [SetUp]
        public void SetUp()
        {
            Time.captureDeltaTime = 1f / 30f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() => PlayModeTestReset.DestroyPersistentGameManager();

        [UnityTest]
        public IEnumerator FloatingProps_StayAtTheSurface()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Boot, LoadSceneMode.Single);
            for (int i = 0; i < WaitFrames && (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Menu); i++)
            {
                yield return null;
            }
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            for (int i = 0; i < WaitFrames && GameManager.Instance.CurrentState != GameState.Playing; i++)
            {
                yield return null;
            }

            int frames = Mathf.RoundToInt(PlaySeconds * 30f);
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            Buoyancy[] floaters = Object.FindObjectsByType<Buoyancy>(FindObjectsInactive.Exclude);
            Assert.That(floaters.Length, Is.GreaterThan(0), "The Game scene has floating props");
            int failures = 0;
            foreach (Buoyancy b in floaters)
            {
                Vector3 p = b.transform.position;
                float surface = b.Water.HeightAt(p);
                float off = p.y - surface;
                if (Mathf.Abs(off) > SurfaceTolerance)
                {
                    failures++;
                    Debug.LogWarning($"FloatingProps: {b.name} at ({p.x:F1}, {p.y:F2}, {p.z:F1}) scale {b.transform.lossyScale.x:F2}, surface {surface:F2}, off {off:F2}, wet {b.WetFraction:F2}, velocity {b.GetComponent<Rigidbody>().linearVelocity.magnitude:F2}");
                }
            }
            Assert.AreEqual(0, failures, $"{failures} of {floaters.Length} floaters left the surface (see warnings)");
        }
    }
}
