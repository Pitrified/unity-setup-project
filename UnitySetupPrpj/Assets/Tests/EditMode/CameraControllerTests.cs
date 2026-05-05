// EditMode tests for Game.Gameplay.CameraController
// Run via Window > General > Test Runner > EditMode
namespace Game.Tests.EditMode
{
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    [TestFixture]
    public sealed class CameraControllerTests
    {
        private GameObject _cameraGo;
        private CameraController _camera;
        private SO_CameraTuning _tuning;
        private GameObject _targetGo;

        // SO_CameraTuning defaults: LocalOffset=(0,3,-6), PositionLerp=5, RotationLerp=5, LookAtTarget=true

        [SetUp]
        public void SetUp()
        {
            _cameraGo = new GameObject("MainCamera");
            _camera = _cameraGo.AddComponent<CameraController>();

            _tuning = ScriptableObject.CreateInstance<SO_CameraTuning>();
            var tuningField = typeof(CameraController).GetField(
                "_tuning",
                BindingFlags.NonPublic | BindingFlags.Instance);
            tuningField.SetValue(_camera, _tuning);

            _targetGo = new GameObject("Ship");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_cameraGo);
            Object.DestroyImmediate(_targetGo);
            Object.DestroyImmediate(_tuning);
        }

        // ------------------------------------------------------------------
        // SnapToTarget

        [Test]
        public void SnapToTarget_WithTargetAtOrigin_PlacesCameraAtLocalOffset()
        {
            // Target at world origin with identity rotation.
            // LocalOffset (0, 3, -6) → world position (0, 3, -6).
            _targetGo.transform.position = Vector3.zero;
            _targetGo.transform.rotation = Quaternion.identity;

            _camera.SetTarget(_targetGo.transform);
            _camera.SnapToTarget();

            Vector3 pos = _cameraGo.transform.position;
            Assert.AreEqual(0f, pos.x, 0.001f);
            Assert.AreEqual(3f, pos.y, 0.001f);
            Assert.AreEqual(-6f, pos.z, 0.001f);
        }

        [Test]
        public void SnapToTarget_WithTargetOffset_PlacesCameraAtWorldOffset()
        {
            // Target at (10, 0, 10) with identity rotation.
            // World position = (10, 0, 10) + (0, 3, -6) = (10, 3, 4).
            _targetGo.transform.position = new Vector3(10f, 0f, 10f);
            _targetGo.transform.rotation = Quaternion.identity;

            _camera.SetTarget(_targetGo.transform);
            _camera.SnapToTarget();

            Vector3 pos = _cameraGo.transform.position;
            Assert.AreEqual(10f, pos.x, 0.001f);
            Assert.AreEqual(3f, pos.y, 0.001f);
            Assert.AreEqual(4f, pos.z, 0.001f);
        }

        [Test]
        public void SnapToTarget_WithNullTarget_DoesNotMoveCamera()
        {
            _cameraGo.transform.position = new Vector3(5f, 5f, 5f);

            // No target set - camera should stay at (5, 5, 5).
            _camera.SnapToTarget();

            Vector3 pos = _cameraGo.transform.position;
            Assert.AreEqual(5f, pos.x, 0.001f);
            Assert.AreEqual(5f, pos.y, 0.001f);
            Assert.AreEqual(5f, pos.z, 0.001f);
        }

        // ------------------------------------------------------------------
        // Tick - null target

        [Test]
        public void Tick_WithNullTarget_DoesNotMoveCamera()
        {
            _cameraGo.transform.position = new Vector3(3f, 3f, 3f);

            _camera.Tick(1f);

            Vector3 pos = _cameraGo.transform.position;
            Assert.AreEqual(3f, pos.x, 0.001f);
            Assert.AreEqual(3f, pos.y, 0.001f);
            Assert.AreEqual(3f, pos.z, 0.001f);
        }

        // ------------------------------------------------------------------
        // Tick - position smoothing

        [Test]
        public void Tick_WithLargeDt_PositionConvergesToDesiredOffset()
        {
            // After a very large dt, the exp-based lerp factor approaches 1.0 and
            // the camera position should be essentially at the desired world offset.
            _targetGo.transform.position = new Vector3(10f, 0f, 10f);
            _targetGo.transform.rotation = Quaternion.identity;
            // Desired world pos = (10, 3, 4)

            _camera.SetTarget(_targetGo.transform);
            _camera.Tick(100f); // t = 1 - exp(-5*100) ≈ 1.0

            Vector3 pos = _cameraGo.transform.position;
            Assert.AreEqual(10f, pos.x, 0.01f);
            Assert.AreEqual(3f, pos.y, 0.01f);
            Assert.AreEqual(4f, pos.z, 0.01f);
        }

        [Test]
        public void Tick_WithSmallDt_PositionApproachesButDoesNotReachTarget()
        {
            // With a small dt, the camera should move toward the target but not reach it.
            _targetGo.transform.position = new Vector3(0f, 0f, 10f);
            _targetGo.transform.rotation = Quaternion.identity;
            // Desired world pos = (0, 3, 4). Camera starts at (0, 0, 0).

            _camera.SetTarget(_targetGo.transform);
            _camera.Tick(0.016f); // one frame at ~60fps

            Vector3 pos = _cameraGo.transform.position;
            float desiredZ = 4f;
            // Camera should have moved toward z=4 but not reached it.
            Assert.Greater(pos.z, 0f, "Camera should have moved toward target");
            Assert.Less(pos.z, desiredZ, "Camera should not have reached target in one frame");
        }

        // ------------------------------------------------------------------
        // SetTarget

        [Test]
        public void SetTarget_ToNull_CameraHoldsPositionOnTick()
        {
            _targetGo.transform.position = new Vector3(0f, 0f, 10f);
            _camera.SetTarget(_targetGo.transform);
            _camera.Tick(100f); // snap to near offset

            Vector3 posBeforeNull = _cameraGo.transform.position;

            _camera.SetTarget(null);
            _camera.Tick(1f);

            Vector3 posAfterNull = _cameraGo.transform.position;
            Assert.AreEqual(posBeforeNull.x, posAfterNull.x, 0.001f);
            Assert.AreEqual(posBeforeNull.y, posAfterNull.y, 0.001f);
            Assert.AreEqual(posBeforeNull.z, posAfterNull.z, 0.001f);
        }
    }
}
