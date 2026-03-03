#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace GameFramework.Editor
{
    /// <summary>
    /// 微信小游戏一键配置工具。
    /// 提供 SDK 检测、自动导入（从工作空间本地包）、编译符号管理、平台切换等功能。
    /// SDK 已预下载在 Workspace/Packages/com.qq.weixin.minigame/，新项目可一键引用。
    /// 菜单入口：GameFramework / WX MiniGame
    /// </summary>
    public static class WXMiniGameSetup
    {
        private const string TAG = "WXMiniGameSetup";
        private const string MENU_ROOT = "GameFramework/WX MiniGame/";
        private const string WX_DEFINE_SYMBOL = "WEIXINMINIGAME";
        private const string WX_SDK_PACKAGE_NAME = "com.qq.weixin.minigame";
        private const string WX_SDK_GIT_URL =
            "https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk.git";
        private const string WX_SDK_DETECT_NAMESPACE = "WeChatWASM";

        /// <summary>
        /// SDK 本地包在工作空间中的相对目录名
        /// </summary>
        private const string WX_SDK_LOCAL_DIR = "com.qq.weixin.minigame";

        #region 一键配置

        /// <summary>
        /// 一键配置微信小游戏环境（导入 SDK → 添加符号 → 切换平台）
        /// </summary>
        [MenuItem(MENU_ROOT + "One-Click Setup", false, 0)]
        public static void OneClickSetup()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "微信小游戏一键配置",
                "将执行以下操作：\n\n" +
                "1. 检测/导入微信 SDK（从工作空间本地包）\n" +
                "2. 添加 WEIXINMINIGAME 编译符号\n" +
                "3. 切换到 WebGL 平台\n\n" +
                "确定继续？",
                "开始配置", "取消");

            if (!confirmed) return;

            // Step 1: 检测 SDK，未导入则自动从本地包引用
            bool sdkFound = IsWXSDKImported();
            bool sdkImported = false;
            if (!sdkFound)
            {
                sdkImported = TryImportSDKFromLocalPackage();
                if (!sdkImported)
                {
                    bool openPM = EditorUtility.DisplayDialog(
                        "未检测到微信 SDK",
                        "工作空间中未找到预下载的 SDK 包。\n\n" +
                        "请通过以下方式导入：\n\n" +
                        "方式1: Package Manager → Add package from git URL:\n" +
                        $"  {WX_SDK_GIT_URL}\n\n" +
                        "方式2: 在工作空间根目录执行:\n" +
                        "  git clone " + WX_SDK_GIT_URL + "\n" +
                        $"    Packages/{WX_SDK_LOCAL_DIR}\n\n" +
                        "导入后再次运行本工具即可。",
                        "打开 Package Manager", "取消");

                    if (openPM)
                    {
                        EditorApplication.ExecuteMenuItem("Window/Package Manager");
                    }
                    return;
                }
                sdkFound = true;
            }

            // Step 2: 添加编译符号
            bool symbolAdded = AddDefineSymbol();

            // Step 3: 切换平台
            bool platformSwitched = SwitchToWebGL();

            // 汇总结果
            string summary = "=== 配置完成 ===\n\n";
            summary += $"微信 SDK：{(sdkImported ? "已自动导入（本地包）" : "已检测到")}\n";
            summary += $"编译符号 {WX_DEFINE_SYMBOL}：{(symbolAdded ? "已添加" : "已存在")}\n";
            summary += $"WebGL 平台：{(platformSwitched ? "已切换" : "当前已是 WebGL")}\n";
            summary += "\nPlatformManager 将自动使用微信小游戏实现。";

            EditorUtility.DisplayDialog("配置结果", summary, "确定");
            UnityEngine.Debug.Log($"[{TAG}] {summary.Replace("\n", " | ")}");
        }

        #endregion

        #region 单独操作菜单

        /// <summary>
        /// 检测微信 SDK 当前状态
        /// </summary>
        [MenuItem(MENU_ROOT + "Check SDK Status", false, 20)]
        public static void CheckSDKStatus()
        {
            bool found = IsWXSDKImported();
            string localPath = FindLocalSDKPath();
            string symbolStatus = HasDefineSymbol() ? "已添加" : "未添加";
            string platformStatus = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL
                ? "WebGL" : EditorUserBuildSettings.activeBuildTarget.ToString();

            string msg = $"SDK 状态：{(found ? "已导入" : "未导入")}\n" +
                $"本地 SDK 包：{(localPath != null ? localPath : "未找到")}\n" +
                $"编译符号 {WX_DEFINE_SYMBOL}：{symbolStatus}\n" +
                $"当前平台：{platformStatus}";

            EditorUtility.DisplayDialog("微信小游戏环境状态", msg, "确定");
        }

        /// <summary>
        /// 仅导入 SDK（往 manifest.json 写入本地包引用）
        /// </summary>
        [MenuItem(MENU_ROOT + "Import SDK (Local Package)", false, 10)]
        public static void ImportSDKMenu()
        {
            if (IsWXSDKImported())
            {
                EditorUtility.DisplayDialog("导入 SDK", "微信 SDK 已导入，无需重复操作。", "确定");
                return;
            }

            bool imported = TryImportSDKFromLocalPackage();
            if (imported)
            {
                EditorUtility.DisplayDialog(
                    "导入 SDK",
                    "已成功将微信 SDK 引用写入 manifest.json。\n" +
                    "Unity 正在刷新，请等待编译完成。",
                    "确定");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "导入 SDK",
                    "未在工作空间中找到预下载的 SDK 包。\n\n" +
                    "请在工作空间根目录执行：\n" +
                    $"git clone {WX_SDK_GIT_URL} Packages/{WX_SDK_LOCAL_DIR}\n\n" +
                    "或通过 Package Manager 用 git URL 直接导入。",
                    "确定");
            }
        }

        /// <summary>
        /// 添加 WEIXINMINIGAME 编译符号
        /// </summary>
        [MenuItem(MENU_ROOT + "Add Define Symbol", false, 21)]
        public static void AddDefineSymbolMenu()
        {
            bool added = AddDefineSymbol();
            EditorUtility.DisplayDialog(
                "编译符号",
                added
                    ? $"已成功添加 {WX_DEFINE_SYMBOL} 编译符号，Unity 正在重新编译..."
                    : $"{WX_DEFINE_SYMBOL} 编译符号已存在，无需重复添加。",
                "确定");
        }

        /// <summary>
        /// 移除 WEIXINMINIGAME 编译符号
        /// </summary>
        [MenuItem(MENU_ROOT + "Remove Define Symbol", false, 22)]
        public static void RemoveDefineSymbolMenu()
        {
            bool removed = RemoveDefineSymbol();
            EditorUtility.DisplayDialog(
                "编译符号",
                removed
                    ? $"已移除 {WX_DEFINE_SYMBOL} 编译符号，Unity 正在重新编译...\n" +
                      "PlatformManager 将回退到默认平台实现。"
                    : $"{WX_DEFINE_SYMBOL} 编译符号不存在。",
                "确定");
        }

        /// <summary>
        /// 切换到 WebGL 平台
        /// </summary>
        [MenuItem(MENU_ROOT + "Switch to WebGL", false, 40)]
        public static void SwitchToWebGLMenu()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                EditorUtility.DisplayDialog("平台切换", "当前已是 WebGL 平台。", "确定");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "切换平台",
                "将切换到 WebGL 平台，这可能需要较长时间。\n确定继续？",
                "切换", "取消");

            if (confirmed)
            {
                SwitchToWebGL();
            }
        }

        /// <summary>
        /// 切换回 Standalone 平台（开发调试用）
        /// </summary>
        [MenuItem(MENU_ROOT + "Switch to Standalone (Dev)", false, 41)]
        public static void SwitchToStandaloneMenu()
        {
            BuildTarget target;
#if UNITY_EDITOR_OSX
            target = BuildTarget.StandaloneOSX;
#else
            target = BuildTarget.StandaloneWindows64;
#endif
            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                EditorUtility.DisplayDialog("平台切换", "当前已是 Standalone 平台。", "确定");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "切换平台",
                "将切换回 Standalone 平台（开发调试用）。\n确定继续？",
                "切换", "取消");

            if (confirmed)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone, target);
                UnityEngine.Debug.Log($"[{TAG}] Switched to Standalone platform.");
            }
        }

        #endregion

        #region SDK 导入

        /// <summary>
        /// 查找工作空间中预下载的 SDK 本地路径。
        /// 查找逻辑：从项目 Assets 向上遍历找到工作空间根目录下的
        /// Packages/com.qq.weixin.minigame/package.json
        /// </summary>
        /// <returns>相对于项目 Packages 目录的 file: 路径，未找到返回 null</returns>
        private static string FindLocalSDKPath()
        {
            // Application.dataPath = <ProjectRoot>/Assets
            // 项目结构: Workspace/Projects/<ProjectName>/Assets
            // SDK 位置: Workspace/Packages/com.qq.weixin.minigame/
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string projectPackagesDir = Path.Combine(projectRoot, "Packages");

            // 向上查找工作空间根目录（包含 Packages/com.qq.weixin.minigame）
            string current = projectRoot;
            for (int i = 0; i < 5; i++)
            {
                string parent = Path.GetDirectoryName(current);
                if (string.IsNullOrEmpty(parent) || parent == current) break;

                string sdkDir = Path.Combine(parent, "Packages", WX_SDK_LOCAL_DIR);
                string sdkPackageJson = Path.Combine(sdkDir, "package.json");

                if (File.Exists(sdkPackageJson))
                {
                    // 计算从项目 Packages/ 到 SDK 的相对路径
                    Uri from = new Uri(projectPackagesDir + "/");
                    Uri to = new Uri(sdkDir + "/");
                    string relativePath = Uri.UnescapeDataString(
                        from.MakeRelativeUri(to).ToString());

                    // 确保使用 file: 协议格式
                    return $"file:{relativePath}";
                }

                current = parent;
            }

            return null;
        }

        /// <summary>
        /// 尝试从工作空间本地包导入 SDK（写入 manifest.json）
        /// </summary>
        /// <returns>是否成功导入</returns>
        private static bool TryImportSDKFromLocalPackage()
        {
            string localPath = FindLocalSDKPath();
            if (localPath == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{TAG}] Local SDK package not found in workspace.");
                return false;
            }

            string manifestPath = Path.Combine(
                Application.dataPath, "..", "Packages", "manifest.json");

            if (!File.Exists(manifestPath))
            {
                UnityEngine.Debug.LogError(
                    $"[{TAG}] manifest.json not found: {manifestPath}");
                return false;
            }

            string manifest = File.ReadAllText(manifestPath);

            // 检查是否已有该包
            if (manifest.Contains(WX_SDK_PACKAGE_NAME))
            {
                UnityEngine.Debug.Log(
                    $"[{TAG}] SDK already referenced in manifest.json.");
                return true;
            }

            // 在 "dependencies": { 后面插入 SDK 引用
            string insertLine = $"    \"{WX_SDK_PACKAGE_NAME}\": \"{localPath}\",";
            string pattern = "(\"dependencies\"\\s*:\\s*\\{)";
            string replacement = $"$1\n{insertLine}";

            string newManifest = Regex.Replace(manifest, pattern, replacement);

            if (newManifest == manifest)
            {
                UnityEngine.Debug.LogError(
                    $"[{TAG}] Failed to parse manifest.json dependencies block.");
                return false;
            }

            File.WriteAllText(manifestPath, newManifest);
            UnityEngine.Debug.Log(
                $"[{TAG}] SDK imported via local package: {localPath}");

            // 触发 Unity 刷新
            AssetDatabase.Refresh();
            return true;
        }

        #endregion

        #region SDK 检测

        /// <summary>
        /// 检测微信 SDK 是否已导入（三重检测）
        /// </summary>
        private static bool IsWXSDKImported()
        {
            // 方式1：通过类型反射检测
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    if (types.Any(t => t.Namespace == WX_SDK_DETECT_NAMESPACE))
                    {
                        return true;
                    }
                }
                catch
                {
                    // 忽略无法加载的程序集
                }
            }

            // 方式2：通过文件检测（兼容 .unitypackage 导入方式）
            string[] guids = AssetDatabase.FindAssets("WX t:Script");
            if (guids.Length > 0)
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("WeChatWASM") || path.Contains("WX-WASM-SDK"))
                    {
                        return true;
                    }
                }
            }

            // 方式3：检查 manifest.json 中是否有 SDK 包引用
            string manifestPath = Path.Combine(
                Application.dataPath, "..", "Packages", "manifest.json");
            if (File.Exists(manifestPath))
            {
                string manifest = File.ReadAllText(manifestPath);
                if (manifest.Contains(WX_SDK_PACKAGE_NAME))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 编译符号管理

        /// <summary>
        /// 检查是否已有编译符号
        /// </summary>
        private static bool HasDefineSymbol()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            return defines.Split(';').Contains(WX_DEFINE_SYMBOL);
        }

        /// <summary>
        /// 添加编译符号到 WebGL 和当前平台
        /// </summary>
        private static bool AddDefineSymbol()
        {
            bool added = false;
            added |= AddDefineSymbolForGroup(BuildTargetGroup.WebGL);

            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (currentGroup != BuildTargetGroup.WebGL)
            {
                added |= AddDefineSymbolForGroup(currentGroup);
            }

            return added;
        }

        /// <summary>
        /// 为指定 BuildTargetGroup 添加编译符号
        /// </summary>
        private static bool AddDefineSymbolForGroup(BuildTargetGroup group)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var symbolList = defines.Split(';').ToList();

            if (symbolList.Contains(WX_DEFINE_SYMBOL))
            {
                return false;
            }

            symbolList.Add(WX_DEFINE_SYMBOL);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                group, string.Join(";", symbolList));

            UnityEngine.Debug.Log($"[{TAG}] Added {WX_DEFINE_SYMBOL} to {group}");
            return true;
        }

        /// <summary>
        /// 移除编译符号
        /// </summary>
        private static bool RemoveDefineSymbol()
        {
            bool removed = false;

            BuildTargetGroup[] groups = {
                BuildTargetGroup.WebGL,
                BuildTargetGroup.Standalone,
                BuildTargetGroup.Android,
                BuildTargetGroup.iOS
            };

            foreach (var group in groups)
            {
                removed |= RemoveDefineSymbolForGroup(group);
            }

            return removed;
        }

        /// <summary>
        /// 从指定 BuildTargetGroup 移除编译符号
        /// </summary>
        private static bool RemoveDefineSymbolForGroup(BuildTargetGroup group)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var symbolList = defines.Split(';').ToList();

            if (!symbolList.Contains(WX_DEFINE_SYMBOL))
            {
                return false;
            }

            symbolList.Remove(WX_DEFINE_SYMBOL);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                group, string.Join(";", symbolList));

            UnityEngine.Debug.Log($"[{TAG}] Removed {WX_DEFINE_SYMBOL} from {group}");
            return true;
        }

        #endregion

        #region 平台切换

        /// <summary>
        /// 切换到 WebGL 平台
        /// </summary>
        private static bool SwitchToWebGL()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                UnityEngine.Debug.Log($"[{TAG}] Already on WebGL platform.");
                return false;
            }

            UnityEngine.Debug.Log($"[{TAG}] Switching to WebGL platform...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.WebGL, BuildTarget.WebGL);
            return true;
        }

        #endregion
    }
}
#endif
