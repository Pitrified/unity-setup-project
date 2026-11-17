// EditMode tests for Game.Systems.SaveSystem
// Run via Window > General > Test Runner > EditMode
namespace Game.Tests.EditMode
{
    using System.IO;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    /// <summary>A payload the save system knows nothing about, standing in for a game's.</summary>
    [System.Serializable]
    internal struct TestPayload
    {
        public int score;
        public Vector3 position;
        public string name;
    }

    [TestFixture]
    public sealed class SaveSystemTests
    {
        private const int Version = 3;

        private string _tempDir;
        private GameObject _go;
        private SaveSystem _sys;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(
                Path.GetTempPath(),
                "SaveSystemTests_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            _go = new GameObject("SaveSystem");
            _sys = _go.AddComponent<SaveSystem>();
            _sys.SetSaveDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // ------------------------------------------------------------------
        // HasSave

        [Test]
        public void HasSave_ReturnsFalse_WhenNoFileExists()
        {
            Assert.IsFalse(_sys.HasSave);
        }

        [Test]
        public void HasSave_ReturnsTrue_AfterSave()
        {
            _sys.Save(new TestPayload(), Version);

            Assert.IsTrue(_sys.HasSave);
        }

        // ------------------------------------------------------------------
        // Load - happy path

        [Test]
        public void Load_ReturnsNewPayload_WhenNoFileExists()
        {
            TestPayload result = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(0, result.score);
            Assert.AreEqual(0f, result.position.x);
            Assert.IsNull(result.name);
        }

        [Test]
        public void Load_AfterSave_RoundTrips_Payload()
        {
            var data = new TestPayload { score = 7, position = new Vector3(1.5f, 0f, -3.2f), name = "a" };

            _sys.Save(data, Version);
            TestPayload loaded = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(7, loaded.score);
            Assert.AreEqual(1.5f,  loaded.position.x, 1e-4f);
            Assert.AreEqual(-3.2f, loaded.position.z, 1e-4f);
            Assert.AreEqual("a", loaded.name);
        }

        // ------------------------------------------------------------------
        // Save - envelope stamping

        [Test]
        public void Save_StampsCallerVersion()
        {
            _sys.Save(new TestPayload(), Version);

            Assert.AreEqual(Version, ReadEnvelope().version);
        }

        [Test]
        public void Save_StampsLastSavedUtc_NonEmpty()
        {
            _sys.Save(new TestPayload(), Version);

            Assert.IsFalse(string.IsNullOrEmpty(ReadEnvelope().lastSavedUtc));
        }

        [Test]
        public void Save_LeavesNoTempFile()
        {
            _sys.Save(new TestPayload(), Version);

            Assert.IsFalse(File.Exists(Path.Combine(_tempDir, "save.json.tmp")));
        }

        // ------------------------------------------------------------------
        // Load - corrupt file

        [Test]
        public void Load_ReturnsNewPayload_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            TestPayload result = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(0, result.score);
        }

        [Test]
        public void Load_CreatesBackup_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            _sys.Load<TestPayload>(Version);

            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "save.json.bak")));
        }

        [Test]
        public void Load_RemovesSaveFile_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            _sys.Load<TestPayload>(Version);

            Assert.IsFalse(File.Exists(Path.Combine(_tempDir, "save.json")));
        }

        // ------------------------------------------------------------------
        // Load - version mismatch

        [Test]
        public void Load_ReturnsNewPayload_OnVersionMismatch()
        {
            _sys.Save(new TestPayload { score = 5 }, Version + 1);

            TestPayload result = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(0, result.score);
        }

        [Test]
        public void Load_CreatesBackup_OnVersionMismatch()
        {
            _sys.Save(new TestPayload { score = 5 }, Version + 1);

            _sys.Load<TestPayload>(Version);

            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "save.json.bak")));
        }

        [Test]
        public void Load_ResetsFileWithoutEnvelope()
        {
            // A file from before the envelope: version at the top level, no payload.
            File.WriteAllText(Path.Combine(_tempDir, "save.json"),
                "{\"version\":1,\"ship\":{\"x\":1.0,\"y\":0.0,\"z\":2.0,\"yawDeg\":3.0},\"lastSavedUtc\":\"\"}");

            TestPayload result = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(0, result.score);
            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "save.json.bak")));
        }

        // ------------------------------------------------------------------
        // DeleteSave

        [Test]
        public void DeleteSave_RemovesFile()
        {
            _sys.Save(new TestPayload(), Version);
            Assert.IsTrue(_sys.HasSave);

            _sys.DeleteSave();

            Assert.IsFalse(_sys.HasSave);
        }

        [Test]
        public void DeleteSave_DoesNotThrow_WhenNoFileExists()
        {
            Assert.DoesNotThrow(() => _sys.DeleteSave());
        }

        // ------------------------------------------------------------------
        // Second save overwrites first

        [Test]
        public void Save_Twice_SecondDataWins()
        {
            _sys.Save(new TestPayload { score = 10 }, Version);
            _sys.Save(new TestPayload { score = 99 }, Version);

            TestPayload loaded = _sys.Load<TestPayload>(Version);

            Assert.AreEqual(99, loaded.score);
        }

        // ------------------------------------------------------------------
        // helpers

        private SaveEnvelope<TestPayload> ReadEnvelope()
        {
            string json = File.ReadAllText(Path.Combine(_tempDir, "save.json"));
            return JsonUtility.FromJson<SaveEnvelope<TestPayload>>(json);
        }
    }
}
