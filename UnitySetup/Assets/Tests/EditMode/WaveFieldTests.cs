namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Gameplay.Water;

    /// <summary>
    /// <see cref="WaveField"/>, the CPU side of the water's Gerstner waves: hand-computed values,
    /// the height lookup agreeing with the forward displacement, travel at the deep-water speed.
    /// That a query allocates nothing is measured per frame in the PlayMode AllocationTests.
    /// </summary>
    public sealed class WaveFieldTests
    {
        private const float Tolerance = 1e-4f;
        private const float HeightTolerance = 0.01f;

        private static GerstnerWave Wave(float directionDeg, float wavelength, float steepness) =>
            new GerstnerWave { directionDeg = directionDeg, wavelength = wavelength, steepness = steepness };

        [Test]
        public void Displacement_OneWaveAtOrigin_MatchesGerstnerFormula()
        {
            var field = new WaveField(0f, new[] { Wave(0f, 10f, 0.5f) });
            float k = 2f * Mathf.PI / 10f;
            float a = 0.5f / k;

            Vector3 d = field.Displacement(0f, 0f, 0f);

            // f = 0: the crest's horizontal push is a along the travel direction, no lift yet.
            Assert.AreEqual(a, d.x, Tolerance);
            Assert.AreEqual(0f, d.y, Tolerance);
            Assert.AreEqual(0f, d.z, Tolerance);

            // A quarter wavelength on, f = pi/2: all lift.
            Vector3 q = field.Displacement(2.5f, 0f, 0f);
            Assert.AreEqual(0f, q.x, Tolerance);
            Assert.AreEqual(a, q.y, Tolerance);
        }

        [Test]
        public void HeightAt_AgreesWithForwardDisplacement()
        {
            var field = new WaveField(-0.35f, new[]
            {
                Wave(20f, 12f, 0.10f), Wave(60f, 7.5f, 0.09f), Wave(-15f, 4.5f, 0.08f), Wave(100f, 3.4f, 0.06f),
            });
            for (int i = 0; i < 50; i++)
            {
                float x = -20f + i * 0.83f;
                float z = 7f - i * 0.61f;
                float t = i * 0.37f;
                Vector3 d = field.Displacement(x, z, t);
                // The surface point that started at (x, z) is now at (x + dx, z + dz), at height sea + dy.
                float h = field.HeightAt(x + d.x, z + d.z, t);
                Assert.AreEqual(-0.35f + d.y, h, HeightTolerance, $"sample {i}");
            }
        }

        [Test]
        public void Wave_TravelsAtDeepWaterSpeed()
        {
            const float wavelength = 8f;
            var field = new WaveField(0f, new[] { Wave(0f, wavelength, 0.3f) });
            float speed = Mathf.Sqrt(9.81f * wavelength / (2f * Mathf.PI));
            const float dt = 1.3f;

            float before = field.Displacement(1f, 0f, 0f).y;
            float after = field.Displacement(1f + speed * dt, 0f, dt).y;

            Assert.AreEqual(before, after, Tolerance);
        }

        [Test]
        public void NormalAtRest_FlatWater_PointsUp()
        {
            var field = new WaveField(0f, new GerstnerWave[0]);
            Vector3 n = field.NormalAtRest(3f, -2f, 5f);
            Assert.That((n - Vector3.up).magnitude, Is.LessThan(Tolerance));
        }
    }
}
