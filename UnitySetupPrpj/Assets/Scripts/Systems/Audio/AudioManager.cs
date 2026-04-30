namespace Game.Systems
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Audio;

    /// <summary>
    /// Central audio router. Lives in the Persistent scene; owned by GameManager.
    /// Manages one-at-a-time music with crossfade and a fixed SFX source pool.
    /// Volumes are persisted via PlayerPrefs.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour, IAudioManager
    {
        // ------------------------------------------------------------------ PlayerPrefs keys

        private const string PrefKeyMaster = "audio.master";
        private const string PrefKeyMusic  = "audio.music";
        private const string PrefKeySfx    = "audio.sfx";

        // ------------------------------------------------------------------ AudioMixer exposed-parameter names
        // Must match the parameter names exposed in the AudioMixer asset.

        private const string MixerParamMaster = "MasterVol";
        private const string MixerParamMusic  = "MusicVol";
        private const string MixerParamSfx    = "SfxVol";

        // ------------------------------------------------------------------ constants

        /// <summary>Minimum linear volume before dB mapping (avoids log10(0)).</summary>
        internal const float MinVolume    = 0.0001f;

        private const float DefaultVolume = 1f;
        private const int   SfxPoolSize   = 8;

        // ------------------------------------------------------------------ serialized

        [SerializeField] private AudioMixer      _mixer;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;

        // ------------------------------------------------------------------ private state

        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;

        // True when _musicSourceA is the current/incoming active track.
        private bool      _musicAIsActive;
        private Coroutine _musicFadeRoutine;

        private AudioSource[] _sfxPool;
        private int           _sfxPoolIndex;

        private float _masterVolume;
        private float _musicVolume;
        private float _sfxVolume;

        // False when _mixer is null; triggers direct AudioSource volume fallback.
        private bool _hasMixer;

        // ------------------------------------------------------------------ lifecycle

        private void Awake()
        {
            _hasMixer = _mixer != null;
            if (!_hasMixer)
                Log.Error(LogCat.Audio, "AudioMixer not assigned; using direct AudioSource volume fallback.");

            _masterVolume = PlayerPrefs.GetFloat(PrefKeyMaster, DefaultVolume);
            _musicVolume  = PlayerPrefs.GetFloat(PrefKeyMusic,  DefaultVolume);
            _sfxVolume    = PlayerPrefs.GetFloat(PrefKeySfx,    DefaultVolume);

            _musicSourceA   = CreateChildSource("MusicA", _musicGroup, initialVolume: 0f);
            _musicSourceB   = CreateChildSource("MusicB", _musicGroup, initialVolume: 0f);
            _musicAIsActive = true;

            _sfxPool = new AudioSource[SfxPoolSize];
            for (int i = 0; i < SfxPoolSize; i++)
                _sfxPool[i] = CreateChildSource("Sfx_" + i, _sfxGroup, initialVolume: DefaultVolume);

            ApplyAllVolumes();
            Log.Info(LogCat.Audio, "AudioManager initialized.");
        }

        // ------------------------------------------------------------------ IAudioManager

        /// <inheritdoc/>
        public void PlayMusic(AudioClip clip, float fadeSeconds = 1f)
        {
            if (clip == null)
            {
                Log.Warn(LogCat.Audio, "PlayMusic called with null clip.");
                return;
            }

            if (_musicFadeRoutine != null)
                StopCoroutine(_musicFadeRoutine);

            AudioSource outgoing = _musicAIsActive ? _musicSourceA : _musicSourceB;
            AudioSource incoming = _musicAIsActive ? _musicSourceB : _musicSourceA;
            _musicAIsActive = !_musicAIsActive;

            incoming.clip   = clip;
            incoming.volume = 0f;
            incoming.loop   = true;
            incoming.Play();

            _musicFadeRoutine = StartCoroutine(CrossfadeRoutine(outgoing, incoming, fadeSeconds));
        }

        /// <inheritdoc/>
        public void StopMusic(float fadeSeconds = 1f)
        {
            if (_musicFadeRoutine != null)
                StopCoroutine(_musicFadeRoutine);

            // Clear the passive source in case a crossfade was interrupted mid-flight.
            AudioSource passive = _musicAIsActive ? _musicSourceB : _musicSourceA;
            passive.volume = 0f;
            passive.Stop();
            passive.clip = null;

            AudioSource active = _musicAIsActive ? _musicSourceA : _musicSourceB;
            if (!active.isPlaying)
                return;

            _musicFadeRoutine = StartCoroutine(FadeOutRoutine(active, fadeSeconds));
        }

        /// <inheritdoc/>
        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
            {
                Log.Warn(LogCat.Audio, "PlaySfx called with null clip.");
                return;
            }

            AudioSource src = AcquireSfxSource();
            src.clip = clip;
            // With mixer: volumeScale is the only per-clip modulator; mixer handles channel/master levels.
            // Without mixer: fold all volumes into the source volume as a best-effort fallback.
            src.volume = _hasMixer
                ? Mathf.Clamp01(volumeScale)
                : Mathf.Clamp01(volumeScale * _masterVolume * _sfxVolume);
            src.Play();
        }

        /// <inheritdoc/>
        public void SetMasterVolume(float v)
        {
            _masterVolume = ClampVolume(v);
            PlayerPrefs.SetFloat(PrefKeyMaster, _masterVolume);
            ApplyAllVolumes();
        }

        /// <inheritdoc/>
        public void SetMusicVolume(float v)
        {
            _musicVolume = ClampVolume(v);
            PlayerPrefs.SetFloat(PrefKeyMusic, _musicVolume);
            ApplyMusicVolume();
        }

        /// <inheritdoc/>
        public void SetSfxVolume(float v)
        {
            _sfxVolume = ClampVolume(v);
            PlayerPrefs.SetFloat(PrefKeySfx, _sfxVolume);
            ApplySfxVolume();
        }

        // ------------------------------------------------------------------ volume helpers

        private void ApplyAllVolumes()
        {
            ApplyMasterVolume();
            ApplyMusicVolume();
            ApplySfxVolume();
        }

        private void ApplyMasterVolume()
        {
            if (_hasMixer)
                SetMixerDb(MixerParamMaster, _masterVolume);
        }

        private void ApplyMusicVolume()
        {
            if (_hasMixer)
                SetMixerDb(MixerParamMusic, _musicVolume);
        }

        private void ApplySfxVolume()
        {
            if (_hasMixer)
                SetMixerDb(MixerParamSfx, _sfxVolume);
        }

        private void SetMixerDb(string paramName, float linearVolume)
        {
            if (!_mixer.SetFloat(paramName, VolumeToDb(linearVolume)))
                Log.Warn(LogCat.Audio, "AudioMixer parameter '{0}' not found.", paramName);
        }

        // ------------------------------------------------------------------ SFX pool

        private AudioSource AcquireSfxSource()
        {
            for (int i = 0; i < SfxPoolSize; i++)
            {
                int idx = (_sfxPoolIndex + i) % SfxPoolSize;
                if (!_sfxPool[idx].isPlaying)
                {
                    _sfxPoolIndex = (idx + 1) % SfxPoolSize;
                    return _sfxPool[idx];
                }
            }

            // All sources busy: steal the oldest in the ring.
            AudioSource stolen = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % SfxPoolSize;
            return stolen;
        }

        // ------------------------------------------------------------------ factory

        private AudioSource CreateChildSource(string goName, AudioMixerGroup group, float initialVolume)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.volume      = initialVolume;
            if (group != null)
                src.outputAudioMixerGroup = group;
            return src;
        }

        // ------------------------------------------------------------------ coroutines

        // The target fade-in level differs by mode: with a mixer the source level
        // acts only as a fade envelope (0-1) and the mixer handles dB attenuation;
        // without a mixer the source level must encode the real effective volume.
        private float MusicFadeTarget => _hasMixer ? 1f : Mathf.Clamp01(_masterVolume * _musicVolume);

        private IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming, float duration)
        {
            float elapsed     = 0f;
            float startOut    = outgoing.volume;
            float targetIn    = MusicFadeTarget;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(startOut, 0f, t);
                incoming.volume = Mathf.Lerp(0f, targetIn, t);
                yield return null;
            }

            outgoing.volume = 0f;
            outgoing.Stop();
            outgoing.clip     = null;
            incoming.volume   = targetIn;
            _musicFadeRoutine = null;
        }

        private IEnumerator FadeOutRoutine(AudioSource source, float duration)
        {
            float elapsed    = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            source.volume     = 0f;
            source.Stop();
            source.clip       = null;
            _musicFadeRoutine = null;
        }

        // ------------------------------------------------------------------ pure math (internal for tests)

        /// <summary>Converts a linear volume (0-1) to decibels for the AudioMixer.</summary>
        internal static float VolumeToDb(float linearVolume)
            => Mathf.Log10(Mathf.Max(linearVolume, MinVolume)) * 20f;

        /// <summary>Clamps a volume value to the [0, 1] range.</summary>
        internal static float ClampVolume(float v)
            => Mathf.Clamp01(v);
    }
}
