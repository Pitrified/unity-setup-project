// EditMode tests for AudioManager pure-math helpers.
// Run via Window > General > Test Runner > EditMode.
namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    [TestFixture]
    public sealed class AudioManagerTests
    {
        // ------------------------------------------------------------------
        // VolumeToDb

        [Test]
        public void VolumeToDb_One_ReturnsZeroDB()
        {
            float result = AudioManager.VolumeToDb(1f);

            Assert.That(result, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void VolumeToDb_Zero_ReturnsSilenceFloor()
        {
            // v=0 is clamped to MinVolume (0.0001), so result = log10(0.0001)*20 = -80 dB
            float expected = Mathf.Log10(AudioManager.MinVolume) * 20f;

            float result = AudioManager.VolumeToDb(0f);

            Assert.That(result, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void VolumeToDb_Negative_ReturnsSameFloorAsZero()
        {
            float atZero    = AudioManager.VolumeToDb(0f);
            float atNegative = AudioManager.VolumeToDb(-1f);

            Assert.That(atNegative, Is.EqualTo(atZero).Within(0.0001f));
        }

        [Test]
        public void VolumeToDb_Half_IsApproximatelyMinusSixDB()
        {
            // 20*log10(0.5) ≈ -6.02 dB
            float result = AudioManager.VolumeToDb(0.5f);

            Assert.That(result, Is.EqualTo(-6.0206f).Within(0.001f));
        }

        [Test]
        public void VolumeToDb_IsMonotonicallyIncreasing()
        {
            float low  = AudioManager.VolumeToDb(0.1f);
            float mid  = AudioManager.VolumeToDb(0.5f);
            float high = AudioManager.VolumeToDb(1.0f);

            Assert.Less(low, mid);
            Assert.Less(mid, high);
        }

        // ------------------------------------------------------------------
        // ClampVolume

        [Test]
        public void ClampVolume_NegativeInput_ReturnsZero()
        {
            float result = AudioManager.ClampVolume(-0.5f);

            Assert.AreEqual(0f, result);
        }

        [Test]
        public void ClampVolume_GreaterThanOne_ReturnsOne()
        {
            float result = AudioManager.ClampVolume(2f);

            Assert.AreEqual(1f, result);
        }

        [Test]
        public void ClampVolume_ZeroToOne_ReturnsUnchanged()
        {
            Assert.AreEqual(0f,   AudioManager.ClampVolume(0f));
            Assert.AreEqual(0.5f, AudioManager.ClampVolume(0.5f));
            Assert.AreEqual(1f,   AudioManager.ClampVolume(1f));
        }

        [Test]
        public void ClampVolume_ExactBoundaries_AreInclusive()
        {
            Assert.AreEqual(0f, AudioManager.ClampVolume(0f));
            Assert.AreEqual(1f, AudioManager.ClampVolume(1f));
        }
    }
}
