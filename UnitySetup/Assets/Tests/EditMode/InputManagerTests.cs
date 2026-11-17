// EditMode tests for InputManager normalization logic.
// Run via Window > General > Test Runner > EditMode.
namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    [TestFixture]
    public sealed class InputManagerTests
    {
        // ------------------------------------------------------------------
        // ApplyDeadzone - below threshold

        [Test]
        public void ApplyDeadzone_BelowThreshold_ReturnsZero()
        {
            Vector2 result = InputManager.ApplyDeadzone(new Vector2(0.1f, 0.0f), 0.15f);

            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void ApplyDeadzone_ZeroVector_ReturnsZero()
        {
            Vector2 result = InputManager.ApplyDeadzone(Vector2.zero, 0.15f);

            Assert.AreEqual(Vector2.zero, result);
        }

        // ------------------------------------------------------------------
        // ApplyDeadzone - at/above threshold

        [Test]
        public void ApplyDeadzone_ExactlyAtThreshold_ReturnsZero()
        {
            // magnitude == deadzone: remapped = (0.15 - 0.15) / 0.85 = 0 -> zero vector
            Vector2 result = InputManager.ApplyDeadzone(new Vector2(0.15f, 0.0f), 0.15f);

            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void ApplyDeadzone_FullMagnitude_ReturnsMagnitudeOne()
        {
            Vector2 result = InputManager.ApplyDeadzone(new Vector2(1.0f, 0.0f), 0.15f);

            Assert.That(result.magnitude, Is.EqualTo(1.0f).Within(0.0001f));
        }

        [Test]
        public void ApplyDeadzone_MidRange_RemapsMagnitudeCorrectly()
        {
            // magnitude = 0.575; expected remapped = (0.575 - 0.15) / (1.0 - 0.15) = 0.5
            Vector2 result = InputManager.ApplyDeadzone(new Vector2(0.575f, 0.0f), 0.15f);

            Assert.That(result.magnitude, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void ApplyDeadzone_OverMagnitudeOne_ClampedToMagnitudeOne()
        {
            // Oversized input must be clamped; result should not exceed magnitude 1.
            Vector2 result = InputManager.ApplyDeadzone(new Vector2(2.0f, 0.0f), 0.15f);

            Assert.That(result.magnitude, Is.LessThanOrEqualTo(1.0f));
        }

        // ------------------------------------------------------------------
        // ApplyDeadzone - direction preservation

        [Test]
        public void ApplyDeadzone_PreservesDirection()
        {
            var raw = new Vector2(0.6f, 0.6f);
            Vector2 expectedDir = raw.normalized;

            Vector2 result = InputManager.ApplyDeadzone(raw, 0.15f);

            Assert.That(result.normalized.x, Is.EqualTo(expectedDir.x).Within(0.001f));
            Assert.That(result.normalized.y, Is.EqualTo(expectedDir.y).Within(0.001f));
        }

        // ------------------------------------------------------------------
        // DeadzoneRadius constant

        [Test]
        public void DeadzoneRadius_IsExpectedValue()
        {
            Assert.That(InputManager.DeadzoneRadius, Is.EqualTo(0.15f));
        }
    }
}
