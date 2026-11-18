namespace Game.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.TestTools;
    using Game.Gameplay.Water;

    /// <summary>
    /// The ripple simulation on the GPU, read back: an impulse lifts the water where it lands,
    /// a ring has travelled outward a few steps later, and the field dies down. And the sample
    /// cannonball launcher's ball lands in the water and sinks.
    /// </summary>
    public sealed class WaterRippleTests
    {
        private const int Resolution = 64;

        // Far from the Game scene's island, which an earlier test may have left loaded.
        private static readonly Vector3 Origin = new Vector3(1000f, 0f, 1000f);

        private GameObject _host;
        private SO_WaterSettings _settings;
        private Material _simulation;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SO_WaterSettings>();
            _settings.SetWaves(0f, new GerstnerWave[0]);
            _host = new GameObject("TestWater");
            _host.AddComponent<WaterSurface>().UseSettings(_settings);
            _simulation = new Material(Shader.Find("unity-setup/WaterRippleSim"));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_host);
            Object.Destroy(_settings);
            Object.Destroy(_simulation);
        }

        [UnityTest]
        public IEnumerator Impulse_SpreadsAsARing_AndDiesDown()
        {
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), 32f, Resolution, 0f); // stepped by hand below

            ripples.AddImpulse(Origin, 1.5f, 0.5f);
            ripples.Step(1);
            yield return null;
            yield return null;
            float[] first = Read(ripples.Field);
            int centre = Resolution / 2 * Resolution + Resolution / 2;
            Assert.That(first[centre], Is.GreaterThan(0.1f), "the impulse lifts the water where it lands");

            ripples.Step(10);
            yield return null;
            yield return null;
            float[] later = Read(ripples.Field);
            float ring = RingEnergy(later, 6, 14);
            Assert.That(ring, Is.GreaterThan(RingEnergy(first, 6, 14) + 0.01f), "a ring has travelled outward");

            float before = Energy(later);
            ripples.Step(150);
            yield return null;
            yield return null;
            Assert.That(Energy(Read(ripples.Field)), Is.LessThan(before * 0.2f), "the ripples die down");
        }

        [UnityTest]
        public IEnumerator Cannonball_LandsInTheWater_AndSinks()
        {
            var ripples = _host.AddComponent<WaterRipples>();
            ripples.Configure(_simulation, new Vector2(Origin.x, Origin.z), 32f, Resolution, 30f);
            var launcherObject = new GameObject("Launcher");
            launcherObject.transform.position = Origin + new Vector3(0f, 2f, -8f);
            var launcher = launcherObject.AddComponent<CannonballLauncher>();
            launcher.Configure(_host.GetComponent<WaterSurface>(), ripples, Origin, 999f);

            launcher.FireNow();
            Rigidbody ball = launcher.Balls[0];
            for (int i = 0; i < Mathf.RoundToInt(4f / Time.fixedDeltaTime); i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(ball.position.y, Is.LessThan(-1f), "an iron ball sinks");
            Assert.That(new Vector2(ball.position.x - Origin.x, ball.position.z - Origin.z).magnitude, Is.LessThan(8f),
                "it landed near the target (spread 5 m, and it drifts as it sinks)");
            Object.Destroy(launcherObject);
            foreach (Rigidbody b in launcher.Balls)
            {
                Object.Destroy(b.gameObject);
            }
        }

        private static float[] Read(RenderTexture field)
        {
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(field, 0, TextureFormat.RGFloat);
            request.WaitForCompletion();
            NativeArray<Vector2> data = request.GetData<Vector2>();
            var heights = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                heights[i] = data[i].x;
            }
            return heights;
        }

        private static float Energy(float[] h)
        {
            float sum = 0f;
            foreach (float v in h)
            {
                sum += v * v;
            }
            return sum;
        }

        private static float RingEnergy(float[] h, int inner, int outer)
        {
            float sum = 0f;
            int c = Resolution / 2;
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    if (r >= inner && r < outer)
                    {
                        sum += h[y * Resolution + x] * h[y * Resolution + x];
                    }
                }
            }
            return sum;
        }
    }
}
