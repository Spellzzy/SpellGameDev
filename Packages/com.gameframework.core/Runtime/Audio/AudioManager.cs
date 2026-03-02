using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;

namespace GameFramework.Audio
{
    /// <summary>
    /// 音频管理器。
    /// 管理BGM和SFX的播放、音量控制、静音。
    /// 支持BGM渐变切换、SFX对象池复用。
    /// </summary>
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private const string TAG = "AudioManager";
        private const string AUDIO_ROOT = "Audio/";

        [Header("音量设置")]
        [Range(0f, 1f)] private float _masterVolume = 1f;
        [Range(0f, 1f)] private float _bgmVolume = 1f;
        [Range(0f, 1f)] private float _sfxVolume = 1f;

        private bool _isMuted = false;

        // BGM播放器（两个用于交叉渐变）
        private AudioSource _bgmSourceA;
        private AudioSource _bgmSourceB;
        private AudioSource _activeBgmSource;

        // SFX对象池
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private const int MAX_SFX_SOURCES = 16;

        // 音频缓存
        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

        #region Properties

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                UpdateVolumes();
                EventSystem.Instance.Publish(GameEvents.AUDIO_VOLUME_CHANGED, this);
            }
        }

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = Mathf.Clamp01(value);
                UpdateVolumes();
                EventSystem.Instance.Publish(GameEvents.AUDIO_VOLUME_CHANGED, this);
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                EventSystem.Instance.Publish(GameEvents.AUDIO_VOLUME_CHANGED, this);
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                UpdateVolumes();
            }
        }

        #endregion

        protected override void OnInit()
        {
            // 创建BGM双通道
            _bgmSourceA = CreateAudioSource("BGM_A");
            _bgmSourceA.loop = true;
            _bgmSourceB = CreateAudioSource("BGM_B");
            _bgmSourceB.loop = true;
            _activeBgmSource = _bgmSourceA;

            GameLogger.LogInfo(TAG, "AudioManager initialized.");
        }

        #region BGM

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipName">音频资源名称（Resources/Audio/下）</param>
        /// <param name="fadeTime">渐变时间（0=立即切换）</param>
        public void PlayBGM(string clipName, float fadeTime = 1f)
        {
            var clip = GetClip(clipName);
            if (clip == null) return;

            if (_activeBgmSource.clip == clip && _activeBgmSource.isPlaying)
            {
                return; // 相同BGM，不重复播放
            }

            if (fadeTime > 0f && _activeBgmSource.isPlaying)
            {
                StartCoroutine(CrossFadeBGM(clip, fadeTime));
            }
            else
            {
                _activeBgmSource.clip = clip;
                _activeBgmSource.volume = GetBgmRealVolume();
                _activeBgmSource.Play();
            }

            GameLogger.LogInfo(TAG, $"BGM: {clipName}");
            EventSystem.Instance.Publish(GameEvents.AUDIO_BGM_CHANGED, this);
        }

        /// <summary>
        /// 停止BGM
        /// </summary>
        public void StopBGM(float fadeTime = 1f)
        {
            if (fadeTime > 0f)
            {
                StartCoroutine(FadeOut(_activeBgmSource, fadeTime));
            }
            else
            {
                _activeBgmSource.Stop();
            }
        }

        /// <summary>
        /// 暂停/恢复BGM
        /// </summary>
        public void PauseBGM(bool pause)
        {
            if (pause)
                _activeBgmSource.Pause();
            else
                _activeBgmSource.UnPause();
        }

        private System.Collections.IEnumerator CrossFadeBGM(AudioClip newClip, float duration)
        {
            var oldSource = _activeBgmSource;
            var newSource = (_activeBgmSource == _bgmSourceA) ? _bgmSourceB : _bgmSourceA;

            newSource.clip = newClip;
            newSource.volume = 0f;
            newSource.Play();

            float timer = 0f;
            float targetVolume = GetBgmRealVolume();

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                oldSource.volume = Mathf.Lerp(targetVolume, 0f, t);
                newSource.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            oldSource.Stop();
            oldSource.volume = 0f;
            newSource.volume = targetVolume;
            _activeBgmSource = newSource;
        }

        private System.Collections.IEnumerator FadeOut(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
        }

        #endregion

        #region SFX

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipName">音频资源名称</param>
        /// <param name="volumeScale">音量缩放（基于SfxVolume）</param>
        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            var clip = GetClip(clipName);
            if (clip == null) return;

            var source = GetAvailableSfxSource();
            source.clip = clip;
            source.volume = GetSfxRealVolume() * volumeScale;
            source.Play();
        }

        /// <summary>
        /// 在世界空间位置播放3D音效
        /// </summary>
        public void PlaySFXAtPosition(string clipName, Vector3 position, float volumeScale = 1f)
        {
            var clip = GetClip(clipName);
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, GetSfxRealVolume() * volumeScale);
        }

        private AudioSource GetAvailableSfxSource()
        {
            // 找空闲的
            foreach (var source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 没有空闲的，创建新的（不超过上限）
            if (_sfxSources.Count < MAX_SFX_SOURCES)
            {
                var source = CreateAudioSource($"SFX_{_sfxSources.Count}");
                _sfxSources.Add(source);
                return source;
            }

            // 超过上限，复用最早的
            return _sfxSources[0];
        }

        #endregion

        #region Helpers

        private AudioClip GetClip(string clipName)
        {
            if (_clipCache.TryGetValue(clipName, out var cached))
            {
                return cached;
            }

            var clip = Resources.Load<AudioClip>(AUDIO_ROOT + clipName);
            if (clip == null)
            {
                GameLogger.LogError(TAG, $"AudioClip not found: '{AUDIO_ROOT + clipName}'.");
                return null;
            }

            _clipCache[clipName] = clip;
            return clip;
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            return go.AddComponent<AudioSource>();
        }

        private float GetBgmRealVolume() => _isMuted ? 0f : _masterVolume * _bgmVolume;
        private float GetSfxRealVolume() => _isMuted ? 0f : _masterVolume * _sfxVolume;

        private void UpdateVolumes()
        {
            if (_bgmSourceA.isPlaying)
                _bgmSourceA.volume = (_activeBgmSource == _bgmSourceA) ? GetBgmRealVolume() : 0f;
            if (_bgmSourceB.isPlaying)
                _bgmSourceB.volume = (_activeBgmSource == _bgmSourceB) ? GetBgmRealVolume() : 0f;
        }

        /// <summary>
        /// 清空音频缓存
        /// </summary>
        public void ClearCache()
        {
            _clipCache.Clear();
        }

        #endregion
    }
}
