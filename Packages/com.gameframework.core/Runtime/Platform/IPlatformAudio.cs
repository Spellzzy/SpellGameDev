using System;

namespace GameFramework.Platform
{
    /// <summary>
    /// 平台音频接口。
    /// 抽象 Unity AudioSource / WX.InnerAudioContext 等不同平台的音频播放实现。
    /// AudioManager 通过此接口驱动底层播放，上层 API 保持不变。
    /// </summary>
    public interface IPlatformAudio
    {
        /// <summary>
        /// 初始化音频系统
        /// </summary>
        void Initialize();

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="clipPath">音频资源路径</param>
        /// <param name="loop">是否循环</param>
        /// <param name="volume">音量 (0~1)</param>
        void PlayBGM(string clipPath, bool loop = true, float volume = 1f);

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        void StopBGM();

        /// <summary>
        /// 暂停/恢复背景音乐
        /// </summary>
        void PauseBGM(bool pause);

        /// <summary>
        /// 设置背景音乐音量
        /// </summary>
        void SetBGMVolume(float volume);

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipPath">音频资源路径</param>
        /// <param name="volume">音量 (0~1)</param>
        void PlaySFX(string clipPath, float volume = 1f);

        /// <summary>
        /// 设置音效音量（影响后续播放的音效）
        /// </summary>
        void SetSFXVolume(float volume);

        /// <summary>
        /// 设置是否静音
        /// </summary>
        void SetMute(bool mute);

        /// <summary>
        /// 清空音频缓存
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 释放资源
        /// </summary>
        void Dispose();
    }
}
