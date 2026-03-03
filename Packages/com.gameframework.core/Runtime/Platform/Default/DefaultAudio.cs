using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Platform
{
    /// <summary>
    /// 默认音频实现。
    /// 基于 Unity AudioSource，适用于 Editor / Standalone / 移动端原生平台。
    /// AudioManager 通过此实现驱动底层播放。
    /// </summary>
    public class DefaultAudio : IPlatformAudio
    {
        private const string AUDIO_ROOT = "Audio/";
        private const int MAX_SFX_SOURCES = 16;

        private GameObject _audioRoot;
        private AudioSource _bgmSourceA;
        private AudioSource _bgmSourceB;
        private AudioSource _activeBgmSource;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private readonly Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();

        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMuted;

        /// <summary>
        /// 初始化音频系统，创建 BGM 双通道
        /// </summary>
        public void Initialize()
        {
            _audioRoot = new GameObject("[DefaultAudio]");
            Object.DontDestroyOnLoad(_audioRoot);

            _bgmSourceA = CreateSource("BGM_A");
            _bgmSourceA.loop = true;
            _bgmSourceB = CreateSource("BGM_B");
            _bgmSourceB.loop = true;
            _activeBgmSource = _bgmSourceA;
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(string clipPath, bool loop = true, float volume = 1f)
        {
            var clip = GetClip(clipPath);
            if (clip == null) return;

            if (_activeBgmSource.clip == clip && _activeBgmSource.isPlaying)
            {
                return;
            }

            _activeBgmSource.clip = clip;
            _activeBgmSource.loop = loop;
            _activeBgmSource.volume = _isMuted ? 0f : volume * _bgmVolume;
            _activeBgmSource.Play();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            _activeBgmSource.Stop();
        }

        /// <summary>
        /// 暂停/恢复背景音乐
        /// </summary>
        public void PauseBGM(bool pause)
        {
            if (pause)
                _activeBgmSource.Pause();
            else
                _activeBgmSource.UnPause();
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_activeBgmSource != null && !_isMuted)
            {
                _activeBgmSource.volume = _bgmVolume;
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string clipPath, float volume = 1f)
        {
            var clip = GetClip(clipPath);
            if (clip == null) return;

            var source = GetAvailableSfxSource();
            source.clip = clip;
            source.volume = _isMuted ? 0f : volume * _sfxVolume;
            source.Play();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// 设置是否静音
        /// </summary>
        public void SetMute(bool mute)
        {
            _isMuted = mute;
            float bgmVol = mute ? 0f : _bgmVolume;
            if (_bgmSourceA != null && _bgmSourceA.isPlaying)
                _bgmSourceA.volume = (_activeBgmSource == _bgmSourceA) ? bgmVol : 0f;
            if (_bgmSourceB != null && _bgmSourceB.isPlaying)
                _bgmSourceB.volume = (_activeBgmSource == _bgmSourceB) ? bgmVol : 0f;
        }

        /// <summary>
        /// 清空音频缓存
        /// </summary>
        public void ClearCache()
        {
            _clipCache.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _clipCache.Clear();
            _sfxSources.Clear();
            if (_audioRoot != null)
            {
                Object.Destroy(_audioRoot);
                _audioRoot = null;
            }
        }

        private AudioClip GetClip(string clipPath)
        {
            if (_clipCache.TryGetValue(clipPath, out var cached))
            {
                return cached;
            }

            var clip = Resources.Load<AudioClip>(AUDIO_ROOT + clipPath);
            if (clip != null)
            {
                _clipCache[clipPath] = clip;
            }
            return clip;
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_audioRoot.transform);
            return go.AddComponent<AudioSource>();
        }

        private AudioSource GetAvailableSfxSource()
        {
            foreach (var source in _sfxSources)
            {
                if (!source.isPlaying) return source;
            }

            if (_sfxSources.Count < MAX_SFX_SOURCES)
            {
                var source = CreateSource($"SFX_{_sfxSources.Count}");
                _sfxSources.Add(source);
                return source;
            }

            return _sfxSources[0];
        }
    }
}
