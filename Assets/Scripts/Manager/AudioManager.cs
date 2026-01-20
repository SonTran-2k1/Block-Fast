using UnityEngine;
using Core.Singleton;
using DG.Tweening;

namespace Core.Managers
{
    [System.Serializable]
    public class AudioClipConfig
    {
        public string clipName;
        public AudioClip clip;
        public float volume = 1f;
    }

    /// <summary>
    /// Centralized audio management system with music and SFX support.
    /// Uses DOTween for smooth volume transitions.
    /// </summary>
    public class AudioManager : SingletonBase<AudioManager>
    {
        [SerializeField] private AudioClipConfig[] musicClips;
        [SerializeField] private AudioClipConfig[] sfxClips;
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private bool musicEnabled = true;
        private bool sfxEnabled = true;
        private string currentMusicClip;

        // Events for audio state changes
        public delegate void AudioEventHandler(string clipName);
        public event AudioEventHandler OnMusicChanged;
        public event AudioEventHandler OnSFXPlayed;

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SFXVolume => sfxVolume;
        public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
        public string CurrentMusicClip => currentMusicClip;

        protected override void OnSingletonInitialized()
        {
            SetupAudioSources();
            Debug.Log("[AudioManager] Initialized with music and SFX sources");
        }

        /// <summary>
        /// Setup audio sources for music and SFX
        /// </summary>
        private void SetupAudioSources()
        {
            // Create or get music source
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = musicVolume * masterVolume;
            }

            // Create or get SFX source
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }

        /// <summary>
        /// Play music with fade in/out effect
        /// </summary>
        public void PlayMusic(string clipName, float fadeDuration = 1f)
        {
            var config = System.Array.Find(musicClips, c => c.clipName == clipName);
            if (config == null || config.clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music clip '{clipName}' not found");
                return;
            }

            // If same clip is already playing, skip
            if (musicSource.clip == config.clip && musicSource.isPlaying)
                return;

            currentMusicClip = clipName;

            // Fade out current music
            musicSource.DOFade(0, fadeDuration * 0.5f).OnComplete(() =>
            {
                musicSource.clip = config.clip;
                musicSource.volume = 0;
                if (musicEnabled)
                    musicSource.Play();

                // Fade in new music
                musicSource.DOFade(config.volume * musicVolume * masterVolume, fadeDuration * 0.5f);
            });

            OnMusicChanged?.Invoke(clipName);
        }

        /// <summary>
        /// Stop current music with fade out
        /// </summary>
        public void StopMusic(float fadeDuration = 1f)
        {
            if (musicSource == null || !musicSource.isPlaying)
                return;

            musicSource.DOFade(0, fadeDuration).OnComplete(() =>
            {
                musicSource.Stop();
                musicSource.clip = null;
                currentMusicClip = null;
            });
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(string clipName)
        {
            if (!sfxEnabled)
                return;

            var config = System.Array.Find(sfxClips, c => c.clipName == clipName);
            if (config == null || config.clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX clip '{clipName}' not found");
                return;
            }

            sfxSource.PlayOneShot(config.clip, config.volume * sfxVolume * masterVolume);
            OnSFXPlayed?.Invoke(clipName);
        }

        /// <summary>
        /// Set master volume (affects all audio)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Toggle music on/off
        /// </summary>
        public void ToggleMusic(bool enabled)
        {
            musicEnabled = enabled;
            if (musicSource == null)
                return;

            if (musicEnabled && !musicSource.isPlaying)
                musicSource.Play();
            else if (!musicEnabled)
                musicSource.Pause();
        }

        /// <summary>
        /// Toggle SFX on/off
        /// </summary>
        public void ToggleSFX(bool enabled)
        {
            sfxEnabled = enabled;
        }

        /// <summary>
        /// Update all volume levels
        /// </summary>
        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }

        /// <summary>
        /// Get status of audio system for debugging
        /// </summary>
        public string GetAudioStatus()
        {
            return $"Master: {masterVolume:F2} | Music: {musicVolume:F2} ({(musicEnabled ? "ON" : "OFF")}) | " +
                   $"SFX: {sfxVolume:F2} ({(sfxEnabled ? "ON" : "OFF")}) | " +
                   $"Current Music: {(currentMusicClip ?? "None")}";
        }
    }
}
