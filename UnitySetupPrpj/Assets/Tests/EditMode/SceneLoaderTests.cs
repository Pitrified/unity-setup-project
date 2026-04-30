// EditMode tests for SceneLoader state-machine and SceneNames constants.
//
// SceneManager.LoadSceneAsync / UnloadSceneAsync are not available in EditMode without
// full play-loop infrastructure, so these tests cover only the logic that does NOT
// require actual scene operations:
//   - SceneNames constant values
//   - IsLoading initial state
//   - Concurrent-call guard (returns in-flight Task instead of starting a new one)
//   - Progress event subscription
//
// Run via Window > General > Test Runner > EditMode.
namespace Game.Tests.EditMode
{
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Core;

    [TestFixture]
    public sealed class SceneLoaderTests
    {
        private GameObject _go;
        private SceneLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("SceneLoader");
            _loader = _go.AddComponent<SceneLoader>();
            // Note: Initialize() is not called here; _input and _ui are null.
            // The methods under test do not reach the body that uses those refs
            // because the concurrent-guard path returns before Initialize is needed.
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // --------------------------------------------------------------------- SceneNames

        [Test]
        public void SceneNames_Boot_IsExpectedValue()
        {
            Assert.AreEqual("Boot", SceneNames.Boot);
        }

        [Test]
        public void SceneNames_Persistent_IsExpectedValue()
        {
            Assert.AreEqual("Persistent", SceneNames.Persistent);
        }

        [Test]
        public void SceneNames_Menu_IsExpectedValue()
        {
            Assert.AreEqual("Menu", SceneNames.Menu);
        }

        [Test]
        public void SceneNames_Game_IsExpectedValue()
        {
            Assert.AreEqual("Game", SceneNames.Game);
        }

        // --------------------------------------------------------------------- initial state

        [Test]
        public void IsLoading_IsFalse_Initially()
        {
            Assert.IsFalse(_loader.IsLoading);
        }

        [Test]
        public void Progress_CanSubscribeAndUnsubscribe_WithoutError()
        {
            void Handler(float _) { }
            Assert.DoesNotThrow(() =>
            {
                _loader.Progress += Handler;
                _loader.Progress -= Handler;
            });
        }

        // --------------------------------------------------------------------- concurrent-call guard

        // The ForceSetInflight helper (internal, exposed via InternalsVisibleTo) puts the
        // loader into the "in-progress" state. Every public load method must return the
        // same in-flight task rather than starting a second transition.

        [Test]
        public void LoadMenuAsync_WhileLoading_ReturnsSameInflightTask()
        {
            var tcs = new TaskCompletionSource<bool>();
            _loader.ForceSetInflight(tcs.Task);

            Task first  = _loader.LoadMenuAsync();
            Task second = _loader.LoadMenuAsync();

            Assert.AreSame(first, second);
            tcs.SetResult(true);
        }

        [Test]
        public void LoadGameAsync_WhileLoading_ReturnsSameInflightTask()
        {
            var tcs = new TaskCompletionSource<bool>();
            _loader.ForceSetInflight(tcs.Task);

            Task first  = _loader.LoadGameAsync();
            Task second = _loader.LoadGameAsync();

            Assert.AreSame(first, second);
            tcs.SetResult(true);
        }

        [Test]
        public void UnloadGameAsync_WhileLoading_ReturnsSameInflightTask()
        {
            var tcs = new TaskCompletionSource<bool>();
            _loader.ForceSetInflight(tcs.Task);

            Task first  = _loader.UnloadGameAsync();
            Task second = _loader.UnloadGameAsync();

            Assert.AreSame(first, second);
            tcs.SetResult(true);
        }

        [Test]
        public void ReloadGameAsync_WhileLoading_ReturnsSameInflightTask()
        {
            var tcs = new TaskCompletionSource<bool>();
            _loader.ForceSetInflight(tcs.Task);

            Task first  = _loader.ReloadGameAsync();
            Task second = _loader.ReloadGameAsync();

            Assert.AreSame(first, second);
            tcs.SetResult(true);
        }

        // --------------------------------------------------------------------- IsLoading state

        [Test]
        public void IsLoading_IsTrue_AfterForceSetInflight()
        {
            var tcs = new TaskCompletionSource<bool>();
            _loader.ForceSetInflight(tcs.Task);

            Assert.IsTrue(_loader.IsLoading);
            tcs.SetResult(true);
        }

        [Test]
        public void MinDisplaySeconds_IsHalfSecond()
        {
            Assert.AreEqual(0.5f, SceneLoader.MinDisplaySeconds, delta: 0.0001f);
        }
    }
}
