// ============================================================================
// 微信小游戏音频实现（预留）
// 导入微信 Unity SDK 后，添加 WEIXINMINIGAME 条件编译符号即可激活。
// 微信小游戏中长音频（BGM）建议使用 InnerAudioContext 以获得更好的性能。
// 短音效仍可使用 Unity AudioSource（通过 DefaultAudio），
// 也可统一使用此实现通过 InnerAudioContext 播放。
// ============================================================================

#if WEIXINMINIGAME

using System.Collections.Generic;
using WeChatWASM;
using UnityEngine;

namespace GameFramework.Platform
{
    /// <summary>
    /// 微信小游戏音频实现。
    /// BGM 使用 WX.CreateInnerAudioContext 播放，性能更优。
    /// SFX 使用独立的 InnerAudioContext 池播放。
    /// </summary>
    public class WXAudio : IPlatformAudio
    {
        private const int MAX_SFX_CONTEXTS = 8;

        private WXInnerAudioContext _bgmContext;
        private readonly List<WXInnerAudioContext> _sfxContexts = new List<WXInnerAudioContext>();
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMuted;

        /// <summary>
        /// 初始化音频系统
        /// </summary>
        public void Initialize()
        {
            _bgmContext = WX.CreateInnerAudioContext(new InnerAudioContextParam());
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayBGM(string clipPath, bool loop = true, float volume = 1f)
        {
            if (_bgmContext == null) return;

            _bgmContext.src = clipPath;
            _bgmContext.loop = loop;
            _bgmContext.volume = _isMuted ? 0f : volume * _bgmVolume;
            _bgmContext.Play();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBGM()
        {
            _bgmContext?.Stop();
        }

        /// <summary>
        /// 暂停/恢复背景音乐
        /// </summary>
        public void PauseBGM(bool pause)
        {
            if (_bgmContext == null) return;

            if (pause)
                _bgmContext.Pause();
            else
                _bgmContext.Play();
        }

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmContext != null && !_isMuted)
            {
                _bgmContext.volume = _bgmVolume;
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(string clipPath, float volume = 1f)
        {
            var ctx = GetAvailableSfxContext();
            ctx.src = clipPath;
            ctx.loop = false;
            ctx.volume = _isMuted ? 0f : volume * _sfxVolume;
            ctx.Play();
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
            if (_bgmContext != null)
            {
                _bgmContext.volume = mute ? 0f : _bgmVolume;
            }
        }

        /// <summary>
        /// 清空音频缓存（微信端由系统管理，此处为空实现）
        /// </summary>
        public void ClearCache()
        {
            // 微信 InnerAudioContext 无需手动管理缓存
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _bgmContext?.Destroy();
            _bgmContext = null;

            foreach (var ctx in _sfxContexts)
            {
                ctx?.Destroy();
            }
            _sfxContexts.Clear();
        }

        private WXInnerAudioContext GetAvailableSfxContext()
        {
            // 复用或创建新的 context
            if (_sfxContexts.Count < MAX_SFX_CONTEXTS)
            {
                var ctx = WX.CreateInnerAudioContext(new InnerAudioContextParam());
                _sfxContexts.Add(ctx);
                return ctx;
            }

            // 超过上限，复用最早的
            return _sfxContexts[0];
        }
    }
}

#endif
