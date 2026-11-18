namespace Game.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using NUnit.Framework;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.TestTools;
    using Game.Core;

    /// <summary>
    /// How managed allocation is measured here, checked against a known allocation: the Profiler's
    /// "GC Allocated In Frame" sees it; <c>GC.GetAllocatedBytesForCurrentThread</c> reads 0 in Unity's
    /// runtime. The game's own number belongs to a player, not the Editor (whose play mode allocates
    /// about 150 kB a frame by itself): the allocation scan, <c>-allocationScan</c>, in a development
    /// player (docs/systems/logging.md).
    /// </summary>
    public sealed class AllocationTests
    {
        private const int WaitFrames = 900;
        private const int SettleFrames = 60;
        private const int MeasureFrames = 120;

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

        /// <summary>Allocates a known amount every frame: the control that proves the measurement counts.</summary>
        private sealed class KnownAllocator : MonoBehaviour
        {
            public static int Size = 1024;
            public static object Sink;

            private void Update() => Sink = new byte[Size];
        }

        [UnityTest]
        public IEnumerator Measurement_SeesAKnownAllocation()
        {
            var go = new GameObject("KnownAllocator");
            go.AddComponent<KnownAllocator>();
            var measured = new long[2];
            yield return Measure(measured);
            UnityEngine.Object.Destroy(go);
            Debug.Log($"AllocationTests control: thread counter {measured[0] / MeasureFrames} B/frame, profiler counter {measured[1] / MeasureFrames} B/frame");
            Assert.That(measured[1] / MeasureFrames, Is.GreaterThanOrEqualTo(KnownAllocator.Size), "the profiler counter sees 1 KB a frame");
        }

        /// <summary>Calls <see cref="Game.Gameplay.Water.WaveField.HeightAt"/> a thousand times a frame.</summary>
        private sealed class HeightQueries : MonoBehaviour
        {
            public Game.Gameplay.Water.WaveField Field;
            public float Sum;

            private void Update()
            {
                for (int i = 0; i < 1000; i++)
                {
                    Sum += Field.HeightAt(i * 0.1f, i * 0.2f, Time.time);
                }
            }
        }

        /// <summary>Does nothing: the per-frame overhead to compare against.</summary>
        private sealed class Idle : MonoBehaviour
        {
        }

        [UnityTest]
        public IEnumerator WaveFieldHeightAt_DoesNotAllocate()
        {
            var measured = new long[2];
            var idle = new GameObject("Idle");
            idle.AddComponent<Idle>();
            yield return Measure(measured);
            long overhead = measured[1] / MeasureFrames;
            UnityEngine.Object.Destroy(idle);

            var go = new GameObject("HeightQueries");
            go.AddComponent<HeightQueries>().Field = new Game.Gameplay.Water.WaveField(0f, new[]
            {
                new Game.Gameplay.Water.GerstnerWave { directionDeg = 0f, wavelength = 10f, steepness = 0.2f },
                new Game.Gameplay.Water.GerstnerWave { directionDeg = 45f, wavelength = 6f, steepness = 0.1f },
            });
            yield return Measure(measured);
            long withQueries = measured[1] / MeasureFrames;
            UnityEngine.Object.Destroy(go);

            Debug.Log($"AllocationTests: idle {overhead} B/frame, 1000 HeightAt a frame {withQueries} B/frame");
            Assert.That(withQueries - overhead, Is.LessThan(64), "a thousand height queries a frame allocate nothing");
        }

        /// <summary>
        /// Bytes allocated over <see cref="MeasureFrames"/> frames: [0] from the thread's counter,
        /// [1] from the Profiler's "GC Allocated In Frame", summed.
        /// </summary>
        private static IEnumerator Measure(long[] result)
        {
            yield return null;
            using var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", MeasureFrames + 4);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < MeasureFrames; i++)
            {
                yield return null;
            }
            result[0] = GC.GetAllocatedBytesForCurrentThread() - before;
            long sum = 0;
            for (int i = 0; i < recorder.Count; i++)
            {
                sum += recorder.GetSample(i).Value;
            }
            result[1] = sum;
        }

        private static IEnumerable<Type> ComponentTypes()
        {
            var types = new SortedDictionary<string, Type>();
            foreach (Behaviour b in UnityEngine.Object.FindObjectsByType<Behaviour>(FindObjectsInactive.Exclude))
            {
                if (b.enabled && !(b is Camera))
                {
                    types[b.GetType().FullName] = b.GetType();
                }
            }
            return types.Values;
        }

        private static List<Behaviour> Disable(Type type)
        {
            var list = new List<Behaviour>();
            foreach (Behaviour b in UnityEngine.Object.FindObjectsByType<Behaviour>(FindObjectsInactive.Exclude))
            {
                if (b.GetType() == type && b.enabled)
                {
                    b.enabled = false;
                    list.Add(b);
                }
            }
            return list;
        }
    }
}
