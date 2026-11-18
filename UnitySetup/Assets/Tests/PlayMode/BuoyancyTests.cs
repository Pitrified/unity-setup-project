namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Game.Gameplay.Water;

    /// <summary>
    /// <see cref="Buoyancy"/> on calm water built in code: a body lighter than water settles at
    /// the draft its density gives, a denser one sinks, a tipped one rights itself. A 1 m cube is
    /// eight slices of 0.5 m around its corners' centres, and floats with draft = density x 1 m.
    /// </summary>
    public sealed class BuoyancyTests
    {
        private const float SettleSeconds = 12f;
        private const float DraftTolerance = 0.08f;

        private GameObject _waterObject;
        private SO_WaterSettings _settings;
        private WaterSurface _water;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WaterSettings>();
            _settings.SetWaves(0f, new GerstnerWave[0]);
            _waterObject = new GameObject("TestWater");
            _water = _waterObject.AddComponent<WaterSurface>();
            _water.UseSettings(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_waterObject);
            Object.Destroy(_settings);
        }

        [UnityTest]
        public IEnumerator LightCube_SettlesAtItsDraft()
        {
            GameObject cube = MakeCube(relativeDensity: 0.5f, startHeight: 2f);
            yield return Simulate(SettleSeconds);

            // Draft 0.5 m on a 1 m cube: the centre sits at the surface.
            Assert.AreEqual(0f, cube.transform.position.y, DraftTolerance, "centre at the waterline");
            Assert.That(Mathf.Abs(cube.GetComponent<Rigidbody>().linearVelocity.y), Is.LessThan(0.1f), "no longer bobbing");
            Object.Destroy(cube);
        }

        [UnityTest]
        public IEnumerator HeavierLightCube_FloatsDeeper()
        {
            GameObject cube = MakeCube(relativeDensity: 0.8f, startHeight: 2f);
            yield return Simulate(SettleSeconds);

            // Draft 0.8 m: the base is 0.8 m down, the centre 0.3 m down.
            Assert.AreEqual(-0.3f, cube.transform.position.y, DraftTolerance);
            Object.Destroy(cube);
        }

        [UnityTest]
        public IEnumerator TippedCube_DoesNotSink()
        {
            GameObject cube = MakeCube(relativeDensity: 0.5f, startHeight: 0.5f);
            cube.transform.rotation = Quaternion.Euler(170f, 0f, 25f);
            yield return Simulate(SettleSeconds);

            // Upside down or not, it floats: lift follows where each slice is, not which way it faces.
            Assert.AreEqual(0f, cube.transform.position.y, DraftTolerance * 2f, "still at the waterline");
            Object.Destroy(cube);
        }

        [UnityTest]
        public IEnumerator DenseCube_Sinks()
        {
            GameObject cube = MakeCube(relativeDensity: 2f, startHeight: 1f);
            yield return Simulate(3f);

            Assert.That(cube.transform.position.y, Is.LessThan(-2f), "a body denser than water goes down");
            Object.Destroy(cube);
        }

        private GameObject MakeCube(float relativeDensity, float startHeight)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(0f, startHeight, 0f);
            cube.AddComponent<Rigidbody>();
            var buoyancy = cube.AddComponent<Buoyancy>();
            buoyancy.Configure(relativeDensity, 1f, new[]
            {
                new Vector3(-0.25f, -0.25f, -0.25f), new Vector3(0.25f, -0.25f, -0.25f),
                new Vector3(-0.25f, -0.25f, 0.25f), new Vector3(0.25f, -0.25f, 0.25f),
                new Vector3(-0.25f, 0.25f, -0.25f), new Vector3(0.25f, 0.25f, -0.25f),
                new Vector3(-0.25f, 0.25f, 0.25f), new Vector3(0.25f, 0.25f, 0.25f),
            }, 0.5f);
            buoyancy.Initialise(_water);
            return cube;
        }

        private static IEnumerator Simulate(float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / Time.fixedDeltaTime);
            for (int i = 0; i < steps; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
