namespace Game.Systems
{
    using System;
    using System.Globalization;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// JSON persistence for the player session.
    /// Writes atomically via a temp file and backs up corrupt data before resetting.
    /// Lives in the Persistent scene; owned by GameManager.
    /// </summary>
    public sealed class SaveSystem : MonoBehaviour, ISaveSystem
    {
        private const string SaveFileName = "save.json";
        private const string TempFileName = "save.json.tmp";
        private const string BackupFileName = "save.json.bak";

        private string _saveDir;

        private void Awake()
        {
            _saveDir = Application.persistentDataPath;
        }

        /// <summary>
        /// Overrides the save directory. For EditMode tests only; never call this from production code.
        /// </summary>
        internal void SetSaveDirectory(string dir)
        {
            _saveDir = dir;
        }

        private string SavePath => Path.Combine(_saveDir, SaveFileName);
        private string TempPath => Path.Combine(_saveDir, TempFileName);
        private string BackupPath => Path.Combine(_saveDir, BackupFileName);

        /// <inheritdoc/>
        public bool HasSave => File.Exists(SavePath);

        /// <inheritdoc/>
        public T Load<T>(int version) where T : new()
        {
            if (!File.Exists(SavePath))
            {
                Log.Info(LogCat.Save, "No save file found, returning defaults.");
                return new T();
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                SaveEnvelope<T> envelope = JsonUtility.FromJson<SaveEnvelope<T>>(json);

                if (envelope == null)
                    throw new InvalidDataException("save file is empty");

                if (envelope.version != version)
                {
                    Log.Warn(LogCat.Save, "Save version mismatch (got {0}, expected {1}), resetting.",
                        envelope.version, version);
                    BackupAndReset();
                    return new T();
                }

                return envelope.payload == null ? new T() : envelope.payload;
            }
            catch (Exception ex)
            {
                Log.Warn(LogCat.Save, "Save file corrupt ({0}), resetting.", ex.Message);
                BackupAndReset();
                return new T();
            }
        }

        /// <inheritdoc/>
        public void Save<T>(T payload, int version)
        {
            var envelope = new SaveEnvelope<T>
            {
                version      = version,
                lastSavedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                payload      = payload,
            };

            try
            {
                string json = JsonUtility.ToJson(envelope);
                File.WriteAllText(TempPath, json);

                // File.Replace requires the destination to already exist.
                // On the very first save the destination is absent, so use Move instead.
                if (File.Exists(SavePath))
                    File.Replace(TempPath, SavePath, null);
                else
                    File.Move(TempPath, SavePath);

                Log.Info(LogCat.Save, "Saved to {0}", SavePath);
            }
            catch (Exception ex)
            {
                Log.Error(LogCat.Save, "Save failed: {0}", ex.Message);
            }
        }

        /// <inheritdoc/>
        public void DeleteSave()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return;

                File.Delete(SavePath);
                Log.Info(LogCat.Save, "Save file deleted.");
            }
            catch (Exception ex)
            {
                Log.Error(LogCat.Save, "Delete save failed: {0}", ex.Message);
            }
        }

        private void BackupAndReset()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return;

                // Overwrite any stale backup from a previous reset.
                if (File.Exists(BackupPath))
                    File.Delete(BackupPath);

                File.Move(SavePath, BackupPath);
            }
            catch (Exception ex)
            {
                Log.Warn(LogCat.Save, "Could not back up corrupt save: {0}", ex.Message);
            }
        }
    }
}
