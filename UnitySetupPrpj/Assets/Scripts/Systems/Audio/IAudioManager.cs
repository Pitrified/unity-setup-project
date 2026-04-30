namespace Game.Systems
{
    using UnityEngine;

    /// <summary>Contract for the central audio router.</summary>
    public interface IAudioManager
    {
        /// <summary>
        /// Starts playing <paramref name="clip"/> as the music track.
        /// Crossfades from the current track over <paramref name="fadeSeconds"/>.
        /// </summary>
        void PlayMusic(AudioClip clip, float fadeSeconds = 1f);

        /// <summary>Fades out and stops the current music track over <paramref name="fadeSeconds"/>.</summary>
        void StopMusic(float fadeSeconds = 1f);

        /// <summary>
        /// Plays <paramref name="clip"/> as a one-shot SFX using the internal source pool.
        /// <paramref name="volumeScale"/> is a 0-1 relative scale applied per clip.
        /// </summary>
        void PlaySfx(AudioClip clip, float volumeScale = 1f);

        /// <summary>Sets the music channel volume (0-1). Persisted via PlayerPrefs.</summary>
        void SetMusicVolume(float v);

        /// <summary>Sets the SFX channel volume (0-1). Persisted via PlayerPrefs.</summary>
        void SetSfxVolume(float v);

        /// <summary>Sets the master volume (0-1). Persisted via PlayerPrefs.</summary>
        void SetMasterVolume(float v);
    }
}
