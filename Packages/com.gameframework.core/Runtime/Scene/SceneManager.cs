using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFramework.Core;
using GameFramework.Event;
using GameFramework.Log;

namespace GameFramework.Scene
{
    /// <summary>
    /// 场景管理器，封装Unity SceneManager。
    /// 支持异步加载、加载进度回调、加载完成事件。
    /// </summary>
    public class SceneManager : MonoSingleton<SceneManager>
    {
        private const string TAG = "SceneManager";

        /// <summary>
        /// 当前正在加载的场景名称
        /// </summary>
        public string LoadingSceneName { get; private set; }

        /// <summary>
        /// 是否正在加载场景
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// 当前活跃场景名称
        /// </summary>
        public string CurrentSceneName => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        /// <summary>
        /// 加载进度回调（0~1）
        /// </summary>
        public event Action<float> OnLoadProgress;

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="mode">加载模式</param>
        /// <param name="onComplete">加载完成回调</param>
        public void LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onComplete = null)
        {
            if (IsLoading)
            {
                GameLogger.LogWarning(TAG, $"Already loading scene '{LoadingSceneName}', ignoring request for '{sceneName}'.");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName, mode, onComplete));
        }

        /// <summary>
        /// 异步卸载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="onComplete">卸载完成回调</param>
        public void UnloadSceneAsync(string sceneName, Action onComplete = null)
        {
            StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode, Action onComplete)
        {
            IsLoading = true;
            LoadingSceneName = sceneName;

            GameLogger.LogInfo(TAG, $"Loading scene '{sceneName}'...");
            EventSystem.Instance.Publish(GameEvents.SCENE_LOAD_START, this,
                new SceneEventArgs(sceneName, 0f));

            var asyncOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            if (asyncOp == null)
            {
                GameLogger.LogError(TAG, $"Failed to load scene '{sceneName}'. Is it added to Build Settings?");
                IsLoading = false;
                LoadingSceneName = null;
                yield break;
            }

            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                OnLoadProgress?.Invoke(progress);
                EventSystem.Instance.Publish(GameEvents.SCENE_LOAD_PROGRESS, this,
                    new SceneEventArgs(sceneName, progress));
                yield return null;
            }

            // 加载完成，激活场景
            asyncOp.allowSceneActivation = true;
            yield return asyncOp;

            OnLoadProgress?.Invoke(1f);
            IsLoading = false;
            LoadingSceneName = null;

            GameLogger.LogInfo(TAG, $"Scene '{sceneName}' loaded.");
            EventSystem.Instance.Publish(GameEvents.SCENE_LOAD_COMPLETE, this,
                new SceneEventArgs(sceneName, 1f));

            onComplete?.Invoke();
        }

        private IEnumerator UnloadSceneCoroutine(string sceneName, Action onComplete)
        {
            GameLogger.LogInfo(TAG, $"Unloading scene '{sceneName}'...");

            var asyncOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            if (asyncOp == null)
            {
                GameLogger.LogError(TAG, $"Failed to unload scene '{sceneName}'.");
                yield break;
            }

            yield return asyncOp;

            GameLogger.LogInfo(TAG, $"Scene '{sceneName}' unloaded.");
            EventSystem.Instance.Publish(GameEvents.SCENE_UNLOAD_COMPLETE, this,
                new SceneEventArgs(sceneName, 1f));

            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// 场景相关事件参数
    /// </summary>
    public class SceneEventArgs : GameEventArgs
    {
        public override string EventId => GameEvents.SCENE_LOAD_START;

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; }

        /// <summary>
        /// 加载进度（0~1）
        /// </summary>
        public float Progress { get; }

        public SceneEventArgs(string sceneName, float progress)
        {
            SceneName = sceneName;
            Progress = progress;
        }
    }
}
