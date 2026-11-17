// EditMode tests for Game.Systems.Log
// Run via Window > General > Test Runner > EditMode
namespace Game.Tests.EditMode
{
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Game.Systems;

    [TestFixture]
    public sealed class LogTests
    {
        [SetUp]
        public void SetUp()
        {
            Log.ResetForTesting();
        }

        // ------------------------------------------------------------------
        // Warn / Error populate the ring buffer

        [Test]
        public void Warn_WritesEntry_ToRingBuffer()
        {
            Log.Warn(LogCat.Save, "test warning");

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(1, count);
            Assert.AreEqual("test warning", buf[0].Text);
            Assert.AreEqual(LogCat.Save, buf[0].Category);
        }

        [Test]
        public void Error_WritesEntry_ToRingBuffer()
        {
            LogAssert.Expect(LogType.Error, "[ERR][Boot] test error");
            Log.Error(LogCat.Boot, "test error");

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(1, count);
            Assert.AreEqual("test error", buf[0].Text);
            Assert.AreEqual(LogCat.Boot, buf[0].Category);
        }

        // ------------------------------------------------------------------
        // Info / Verbose also populate buffer in editor

        [Test]
        public void Info_WritesEntry_InEditorBuild()
        {
            Log.Info(LogCat.Input, "info msg");

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(1, count);
            Assert.AreEqual("info msg", buf[0].Text);
        }

        [Test]
        public void Verbose_WritesEntry_InEditorBuild()
        {
            Log.Verbose(LogCat.Gameplay, "verbose msg");

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(1, count);
            Assert.AreEqual("verbose msg", buf[0].Text);
        }

        // ------------------------------------------------------------------
        // Format args are expanded before storage

        [Test]
        public void Warn_WithFormatArgs_StoresFormattedText()
        {
            Log.Warn(LogCat.Audio, "volume={0}", 0.75f);

            var buf = new LogEntry[20];
            Log.GetRecentEntries(buf);

            StringAssert.Contains("0.75", buf[0].Text);
        }

        // ------------------------------------------------------------------
        // Multiple entries are returned oldest-first

        [Test]
        public void GetRecentEntries_ReturnsOldestFirst()
        {
            Log.Info(LogCat.Boot, "first");
            Log.Warn(LogCat.Scene, "second");
            LogAssert.Expect(LogType.Error, "[ERR][UI] third");
            Log.Error(LogCat.UI, "third");

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(3, count);
            Assert.AreEqual("first",  buf[0].Text);
            Assert.AreEqual("second", buf[1].Text);
            Assert.AreEqual("third",  buf[2].Text);
        }

        // ------------------------------------------------------------------
        // Ring buffer wraps correctly at capacity

        [Test]
        public void RingBuffer_WrapsAt20_RetainsNewest20()
        {
            for (int i = 0; i < 25; i++)
                Log.Info(LogCat.Build, "msg {0}", i);

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(20, count);
            // Oldest retained entry is msg 5 (25 total - 20 capacity = 5 dropped)
            StringAssert.Contains("5", buf[0].Text);
            // Newest is msg 24
            StringAssert.Contains("24", buf[19].Text);
        }

        // ------------------------------------------------------------------
        // dest array smaller than ring: returns only dest.Length entries (newest)

        [Test]
        public void GetRecentEntries_ClampsToDestLength()
        {
            for (int i = 0; i < 10; i++)
                Log.Warn(LogCat.Save, "m{0}", i);

            var buf = new LogEntry[3];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(3, count);
            // Should be the oldest 3 of the 10 written (oldest-first order)
            StringAssert.Contains("0", buf[0].Text);
            StringAssert.Contains("1", buf[1].Text);
            StringAssert.Contains("2", buf[2].Text);
        }

        // ------------------------------------------------------------------
        // Reset clears the buffer

        [Test]
        public void ResetForTesting_ClearsBuffer()
        {
            LogAssert.Expect(LogType.Error, "[ERR][Boot] before reset");
            Log.Error(LogCat.Boot, "before reset");
            Log.ResetForTesting();

            var buf = new LogEntry[20];
            int count = Log.GetRecentEntries(buf);

            Assert.AreEqual(0, count);
        }
    }
}
