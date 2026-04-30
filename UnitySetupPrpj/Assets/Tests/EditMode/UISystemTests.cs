// EditMode tests for Game.UI.UISystem overlay state tracking.
// UIDocument.rootVisualElement is null in EditMode (no play loop / rendering pipeline),
// so these tests verify only the boolean state machine: IsAnyOverlayOpen, ShowLoading,
// HideLoading, ShowScreen, and HideScreen. Visual element calls are guarded with null
// checks inside UISystem and produce no exceptions when documents are unassigned.
//
// Run via Window > General > Test Runner > EditMode.
namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.UI;

    [TestFixture]
    public sealed class UISystemTests
    {
        private GameObject _go;
        private UISystem   _ui;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("UISystem");
            _ui = _go.AddComponent<UISystem>();
            // Note: Start() is not invoked in EditMode, so _loadingRoot/_pauseRoot are null.
            // SetVisible/ApplySafeArea guard against null; boolean state is still tracked.
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // --------------------------------------------------------------------- initial state

        [Test]
        public void IsAnyOverlayOpen_IsFalse_Initially()
        {
            Assert.IsFalse(_ui.IsAnyOverlayOpen);
        }

        // --------------------------------------------------------------------- ShowLoading / HideLoading

        [Test]
        public void ShowLoading_SetsIsAnyOverlayOpen_True()
        {
            _ui.ShowLoading();

            Assert.IsTrue(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void HideLoading_AfterShow_SetsIsAnyOverlayOpen_False()
        {
            _ui.ShowLoading();
            _ui.HideLoading();

            Assert.IsFalse(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void HideLoading_WithoutPriorShow_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _ui.HideLoading());
        }

        // --------------------------------------------------------------------- ShowScreen(Pause) / HideScreen(Pause)

        [Test]
        public void ShowScreen_Pause_SetsIsAnyOverlayOpen_True()
        {
            _ui.ShowScreen(ScreenId.Pause);

            Assert.IsTrue(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void HideScreen_Pause_AfterShow_SetsIsAnyOverlayOpen_False()
        {
            _ui.ShowScreen(ScreenId.Pause);
            _ui.HideScreen(ScreenId.Pause);

            Assert.IsFalse(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void HideScreen_Pause_WithoutPriorShow_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _ui.HideScreen(ScreenId.Pause));
        }

        // --------------------------------------------------------------------- combined state

        [Test]
        public void IsAnyOverlayOpen_RemainsTrue_WhenOnlyLoadingHiddenButPauseStillShown()
        {
            _ui.ShowLoading();
            _ui.ShowScreen(ScreenId.Pause);

            _ui.HideLoading();

            Assert.IsTrue(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void IsAnyOverlayOpen_IsFalse_OnlyAfterBothHidden()
        {
            _ui.ShowLoading();
            _ui.ShowScreen(ScreenId.Pause);

            _ui.HideLoading();
            _ui.HideScreen(ScreenId.Pause);

            Assert.IsFalse(_ui.IsAnyOverlayOpen);
        }

        [Test]
        public void IsAnyOverlayOpen_RemainsTrue_WhenOnlyPauseHiddenButLoadingStillShown()
        {
            _ui.ShowLoading();
            _ui.ShowScreen(ScreenId.Pause);

            _ui.HideScreen(ScreenId.Pause);

            Assert.IsTrue(_ui.IsAnyOverlayOpen);
        }

        // --------------------------------------------------------------------- events

        [Test]
        public void OnResumeRequested_CanSubscribeAndUnsubscribe_WithoutException()
        {
            void Handler() { }

            Assert.DoesNotThrow(() =>
            {
                _ui.OnResumeRequested += Handler;
                _ui.OnResumeRequested -= Handler;
            });
        }

        [Test]
        public void OnReturnToMenuRequested_CanSubscribeAndUnsubscribe_WithoutException()
        {
            void Handler() { }

            Assert.DoesNotThrow(() =>
            {
                _ui.OnReturnToMenuRequested += Handler;
                _ui.OnReturnToMenuRequested -= Handler;
            });
        }
    }
}
