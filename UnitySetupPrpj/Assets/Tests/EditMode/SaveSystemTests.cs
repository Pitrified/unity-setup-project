// EditMode tests for Game.Systems.SaveSystem
// Run via Window > General > Test Runner > EditMode
namespace Game.Tests.EditMode
{
    using System.IO;
    using NUnit.Framework;
    using UnityEngine;
    using Game.Systems;

    [TestFixture]
    public sealed class SaveSystemTests
    {
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
            _sys.Save(SaveData.CreateDefault());

            Assert.IsTrue(_sys.HasSave);
        }

        // ------------------------------------------------------------------
        // Load - happy path

        [Test]
        public void Load_ReturnsCurrentVersion_WhenNoFileExists()
        {
            SaveData result = _sys.Load();

            Assert.AreEqual(SaveData.CurrentVersion, result.version);
        }

        [Test]
        public void Load_ReturnsZeroShipData_WhenNoFileExists()
        {
            SaveData result = _sys.Load();

            Assert.AreEqual(0f, result.ship.x);
            Assert.AreEqual(0f, result.ship.y);
            Assert.AreEqual(0f, result.ship.z);
            Assert.AreEqual(0f, result.ship.yawDeg);
        }

        [Test]
        public void Load_AfterSave_RoundTrips_ShipPosition()
        {
            var data = SaveData.CreateDefault();
            data.ship = new ShipSaveData { x = 1.5f, y = 0f, z = -3.2f, yawDeg = 90f };

            _sys.Save(data);
            SaveData loaded = _sys.Load();

            Assert.AreEqual(1.5f,  loaded.ship.x,      1e-4f);
            Assert.AreEqual(0f,    loaded.ship.y,      1e-4f);
            Assert.AreEqual(-3.2f, loaded.ship.z,      1e-4f);
            Assert.AreEqual(90f,   loaded.ship.yawDeg, 1e-4f);
        }

        // ------------------------------------------------------------------
        // Save - metadata stamping

        [Test]
        public void Save_StampsCurrentVersion()
        {
            _sys.Save(SaveData.CreateDefault());
            SaveData loaded = _sys.Load();

            Assert.AreEqual(SaveData.CurrentVersion, loaded.version);
        }

        [Test]
        public void Save_StampsLastSavedUtc_NonEmpty()
        {
            _sys.Save(SaveData.CreateDefault());
            SaveData loaded = _sys.Load();

            Assert.IsFalse(string.IsNullOrEmpty(loaded.lastSavedUtc));
        }

        [Test]
        public void Save_LeavesNoTempFile()
        {
            _sys.Save(SaveData.CreateDefault());

            Assert.IsFalse(File.Exists(Path.Combine(_tempDir, "save.json.tmp")));
        }

        // ------------------------------------------------------------------
        // Load - corrupt file

        [Test]
        public void Load_ReturnsDefault_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            SaveData result = _sys.Load();

            Assert.AreEqual(SaveData.CurrentVersion, result.version);
        }

        [Test]
        public void Load_CreatesBackup_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            _sys.Load();

            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "save.json.bak")));
        }

        [Test]
        public void Load_RemovesSaveFile_OnCorruptFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), "CORRUPT_DATA");

            _sys.Load();

            Assert.IsFalse(File.Exists(Path.Combine(_tempDir, "save.json")));
        }

        // ------------------------------------------------------------------
        // Load - version mismatch

        [Test]
        public void Load_ReturnsDefault_OnVersionMismatch()
        {
            // Write a save with an unknown future version.
            var stale = new SaveData { version = 99, ship = default, lastSavedUtc = string.Empty };
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), JsonUtility.ToJson(stale));

            SaveData result = _sys.Load();

            Assert.AreEqual(SaveData.CurrentVersion, result.version);
        }

        [Test]
        public void Load_CreatesBackup_OnVersionMismatch()
        {
            var stale = new SaveData { version = 99, ship = default, lastSavedUtc = string.Empty };
            File.WriteAllText(Path.Combine(_tempDir, "save.json"), JsonUtility.ToJson(stale));

            _sys.Load();

            Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "save.json.bak")));
        }

        // ------------------------------------------------------------------
        // DeleteSave

        [Test]
        public void DeleteSave_RemovesFile()
        {
            _sys.Save(SaveData.CreateDefault());
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
            var first = SaveData.CreateDefault();
            first.ship = new ShipSaveData { x = 10f, yawDeg = 45f };
            _sys.Save(first);

            var second = SaveData.CreateDefault();
            second.ship = new ShipSaveData { x = 99f, yawDeg = 180f };
            _sys.Save(second);

            SaveData loaded = _sys.Load();

            Assert.AreEqual(99f,  loaded.ship.x,      1e-4f);
            Assert.AreEqual(180f, loaded.ship.yawDeg, 1e-4f);
        }
    }
}
