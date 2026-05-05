// EditMode tests for Game.Gameplay.ShipController
// Run via Window > General > Test Runner > EditMode
namespace Game.Tests.EditMode
{
    using System.Reflection;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay;

    [TestFixture]
    public sealed class ShipControllerTests
    {
        private GameObject _go;
        private ShipController _controller;
        private SO_ShipTuning _tuning;

        // SO_ShipTuning defaults: MaxSpeed=8, Acceleration=4, ReverseFactor=0.3,
        //                         YawRateDeg=60, SteeringResponseLerp=6

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Ship");
            _controller = _go.AddComponent<ShipController>();

            _tuning = ScriptableObject.CreateInstance<SO_ShipTuning>();
            var tuningField = typeof(ShipController).GetField(
                "_tuning",
                BindingFlags.NonPublic | BindingFlags.Instance);
            tuningField.SetValue(_controller, _tuning);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_tuning);
        }

        // ------------------------------------------------------------------
        // SetInput - NaN / Infinity rejection

        [Test]
        public void SetInput_NaNThrottle_ClampsToZero_NoMovement()
        {
            _controller.SetInput(float.NaN, 0f);
            _controller.Tick(1f);

            ShipState state = _controller.GetState();
            Assert.IsFalse(float.IsNaN(state.Position.z), "Position.z must not be NaN");
            Assert.AreEqual(0f, state.Position.z, 0.001f, "NaN throttle must produce zero movement");
        }

        [Test]
        public void SetInput_NaNSteering_ClampsToZero_NoYawChange()
        {
            _controller.SetInput(0f, float.NaN);
            _controller.Tick(1f);

            ShipState state = _controller.GetState();
            Assert.IsFalse(float.IsNaN(state.YawDeg), "YawDeg must not be NaN");
            Assert.AreEqual(0f, state.YawDeg, 0.001f, "NaN steering must produce zero yaw change");
        }

        [Test]
        public void SetInput_PositiveInfinityThrottle_ClampsToZero_NoMovement()
        {
            _controller.SetInput(float.PositiveInfinity, 0f);
            _controller.Tick(1f);

            ShipState state = _controller.GetState();
            Assert.IsFalse(float.IsInfinity(state.Position.z), "Position.z must not be Infinity");
            Assert.AreEqual(0f, state.Position.z, 0.001f, "Infinity throttle must produce zero movement");
        }

        [Test]
        public void SetInput_NegativeInfinitySteering_ClampsToZero_NoYawChange()
        {
            _controller.SetInput(0f, float.NegativeInfinity);
            _controller.Tick(1f);

            ShipState state = _controller.GetState();
            Assert.IsFalse(float.IsInfinity(state.YawDeg), "YawDeg must not be Infinity");
            Assert.AreEqual(0f, state.YawDeg, 0.001f, "Infinity steering must produce zero yaw change");
        }

        // ------------------------------------------------------------------
        // SetInput - range clamping
        //
        // Using a large dt (100 s) so speed reaches steady state, making the
        // clamped vs unclamped positions clearly distinguishable.
        //
        // throttle=1 (or clamped from 2):
        //   targetSpeed = 8 m/s; MoveTowards(0, 8, 4*100)=8; pos.z = 8*100 = 800 m
        //
        // throttle=-1 (or clamped from -2):
        //   targetSpeed = 8*0.3*(-1) = -2.4 m/s; pos.z = -2.4*100 = -240 m
        //
        // throttle=-2 unclamped would give: targetSpeed = -4.8 m/s; pos.z = -480 m
        // throttle=2  unclamped would give: targetSpeed = 16 m/s;  pos.z = 1600 m

        [Test]
        public void SetInput_ThrottleAboveOne_ClampsToOne()
        {
            _controller.SetInput(2f, 0f);
            _controller.Tick(100f);

            ShipState state = _controller.GetState();
            Assert.AreEqual(800f, state.Position.z, 0.001f,
                "Throttle 2 must be clamped to 1, giving the same position as throttle 1");
        }

        [Test]
        public void SetInput_ThrottleBelowNegativeOne_ClampsToNegativeOne()
        {
            _controller.SetInput(-2f, 0f);
            _controller.Tick(100f);

            ShipState state = _controller.GetState();
            Assert.AreEqual(-240f, state.Position.z, 0.001f,
                "Throttle -2 must be clamped to -1, giving the same position as throttle -1");
        }

        [Test]
        public void SetInput_SteeringAboveOne_ClampsToOne()
        {
            // Use a small dt so yaw delta is predictable before Lerp saturates.
            // smoothedSteering = Lerp(0, 1, 6*0.016) ~ 0.096  (same for both clamp cases)
            // The key check: calling SetInput with 2 must not produce more yaw than SetInput with 1.

            _controller.SetInput(0f, 2f);  // clamped to 1
            _controller.Tick(0.016f);
            float yawClamped = _controller.GetState().YawDeg;

            // Reset
            _controller.Teleport(Vector3.zero, 0f);

            _controller.SetInput(0f, 1f);
            _controller.Tick(0.016f);
            float yawAtOne = _controller.GetState().YawDeg;

            Assert.AreEqual(yawAtOne, yawClamped, 0.001f,
                "Steering 2 must be clamped to 1, producing the same yaw as steering 1");
        }

        // ------------------------------------------------------------------
        // Teleport

        [Test]
        public void Teleport_SetsPositionAndYaw()
        {
            _controller.Teleport(new Vector3(10f, 99f, 20f), 90f);

            ShipState state = _controller.GetState();
            Assert.AreEqual(10f, state.Position.x, 0.001f);
            Assert.AreEqual(0f, state.Position.y, 0.001f, "Y must be locked to sea surface (0)");
            Assert.AreEqual(20f, state.Position.z, 0.001f);
            Assert.AreEqual(90f, state.YawDeg, 0.001f);
        }

        [Test]
        public void Teleport_ResetsAccumulatedSpeed()
        {
            // Build up speed then teleport; after one zero-dt tick the ship must not have moved.
            _controller.SetInput(1f, 0f);
            _controller.Tick(10f); // ship is now moving fast

            _controller.Teleport(new Vector3(5f, 0f, 5f), 0f);
            _controller.Tick(0f); // zero-dt: should not move

            ShipState state = _controller.GetState();
            Assert.AreEqual(5f, state.Position.x, 0.001f);
            Assert.AreEqual(5f, state.Position.z, 0.001f);
        }

        // ------------------------------------------------------------------
        // GetState

        [Test]
        public void GetState_DefaultPosition_IsOrigin()
        {
            ShipState state = _controller.GetState();
            Assert.AreEqual(Vector3.zero, state.Position);
            Assert.AreEqual(0f, state.YawDeg, 0.001f);
        }
    }
}
