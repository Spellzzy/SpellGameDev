#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace GameFramework.Editor
{
    /// <summary>
    /// 微信小游戏一键构建发布工具。
    /// 封装 SDK 的 WXConvertCore.DoExport 流程，提供一键构建+转换。
    /// 菜单入口：GameFramework / WX MiniGame / Build
    /// 
    /// 发布流程：
    /// 1. 一键构建：Unity WebGL 构建 → SDK 自动转换为小游戏格式
    /// 2. 产物在导出目录的 minigame/ 子目录
    /// 3. 用微信开发者工具打开 minigame/ 目录即可预览/上传
    /// </summary>
    public static class WXMiniGameBuilder
    {
        private const string TAG = "WXMiniGameBuilder";
        private const string MENU_ROOT = "GameFramework/WX MiniGame/";

        // SDK 核心类型名（通过反射调用，避免直接引用 WxEditor 程序集）
        private const string WX_CONVERT_CORE_TYPE = "WeChatWASM.WXConvertCore";
        private const string WX_UNITY_UTIL_TYPE = "WeChatWASM.UnityUtil";
        private const string WX_EDITOR_WINDOW_TYPE = "WeChatWASM.WXEditorWin";

        // 默认导出目录名（相对于项目根目录）
        private const string DEFAULT_EXPORT_DIR = "WXExport";

        #region 菜单入口

        /// <summary>
        /// 一键构建并转换为微信小游戏（完整流程）
        /// </summary>
        [MenuItem(MENU_ROOT + "Build MiniGame (Full)", false, 60)]
        public static void BuildFull()
        {
            if (!PreBuildCheck()) return;

            bool confirmed = EditorUtility.DisplayDialog(
                "一键构建微信小游戏",
                "将执行完整构建流程：\n\n" +
                "1. 构建 WebGL\n" +
                "2. 转换为微信小游戏格式\n\n" +
                "构建可能需要较长时间，确定继续？",
                "开始构建", "取消");

            if (!confirmed) return;

            InvokeDoExport(buildWebGL: true);
        }

        /// <summary>
        /// 仅转换（跳过 WebGL 构建，复用上次构建产物）
        /// </summary>
        [MenuItem(MENU_ROOT + "Convert Only (Skip Build)", false, 61)]
        public static void ConvertOnly()
        {
            if (!PreBuildCheck()) return;

            bool confirmed = EditorUtility.DisplayDialog(
                "转换微信小游戏",
                "将跳过 WebGL 构建，直接将已有产物转换为小游戏格式。\n\n" +
                "请确保之前已成功构建过 WebGL。\n确定继续？",
                "开始转换", "取消");

            if (!confirmed) return;

            InvokeDoExport(buildWebGL: false);
        }

        /// <summary>
        /// 打开 SDK 自带的转换窗口（完整配置面板）
        /// </summary>
        [MenuItem(MENU_ROOT + "Open SDK Panel", false, 62)]
        public static void OpenSDKPanel()
        {
            try
            {
                // 尝试通过菜单打开
                bool opened = EditorApplication.ExecuteMenuItem(
                    "微信小游戏/转换小游戏");

                if (!opened)
                {
                    // 菜单不存在，尝试反射打开窗口
                    Type windowType = FindType(WX_EDITOR_WINDOW_TYPE);
                    if (windowType != null)
                    {
                        EditorWindow.GetWindow(windowType, false, "微信小游戏");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "SDK 面板",
                            "未找到微信 SDK 转换窗口。\n" +
                            "请确认 SDK 已正确导入。",
                            "确定");
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[{TAG}] Failed to open SDK panel: {e.Message}");
            }
        }

        /// <summary>
        /// 打开导出目录（方便用微信开发者工具导入）
        /// </summary>
        [MenuItem(MENU_ROOT + "Open Export Folder", false, 63)]
        public static void OpenExportFolder()
        {
            string exportPath = GetExportPath();
            if (string.IsNullOrEmpty(exportPath))
            {
                EditorUtility.DisplayDialog(
                    "导出目录",
                    "未找到导出路径配置。\n" +
                    "请先通过 SDK 面板配置导出路径，或执行一次构建。",
                    "确定");
                return;
            }

            string minigamePath = Path.Combine(exportPath, "minigame");
            string openPath = Directory.Exists(minigamePath)
                ? minigamePath : exportPath;

            if (Directory.Exists(openPath))
            {
                EditorUtility.RevealInFinder(openPath);
                UnityEngine.Debug.Log(
                    $"[{TAG}] Opened export folder: {openPath}");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "导出目录",
                    $"目录不存在：{openPath}\n\n" +
                    "请先执行一次构建。",
                    "确定");
            }
        }

        #endregion

        #region 构建核心

        /// <summary>
        /// 构建前检查
        /// </summary>
        private static bool PreBuildCheck()
        {
            // 检查平台
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                bool switchNow = EditorUtility.DisplayDialog(
                    "平台检查",
                    "当前不是 WebGL 平台，构建需要先切换。\n\n" +
                    "是否立即切换到 WebGL？",
                    "切换并继续", "取消");

                if (!switchNow) return false;

                if (!BuildPipeline.IsBuildTargetSupported(
                        BuildTargetGroup.WebGL, BuildTarget.WebGL))
                {
                    EditorUtility.DisplayDialog(
                        "WebGL 模块未安装",
                        "请通过 Unity Hub → Installs → Add Modules 安装 WebGL Build Support。",
                        "确定");
                    return false;
                }

                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // 检查 SDK
            Type convertType = FindType(WX_CONVERT_CORE_TYPE);
            if (convertType == null)
            {
                EditorUtility.DisplayDialog(
                    "SDK 检查",
                    "未检测到微信 SDK 的构建模块（WXConvertCore）。\n\n" +
                    "请先运行：GameFramework → WX MiniGame → One-Click Setup",
                    "确定");
                return false;
            }

            // 检查导出路径是否已配置
            if (!EnsureExportPath())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 确保 SDK 配置中的导出路径已设置。
        /// 若未配置，自动设置默认路径或弹窗让用户选择。
        /// </summary>
        private static bool EnsureExportPath()
        {
            try
            {
                Type utilType = FindType(WX_UNITY_UTIL_TYPE);
                if (utilType == null) return true; // 无法检查则放行，让 SDK 自己报错

                MethodInfo getConf = utilType.GetMethod(
                    "GetEditorConf",
                    BindingFlags.Public | BindingFlags.Static);
                if (getConf == null) return true;

                var conf = getConf.Invoke(null, null);
                if (conf == null) return true;

                // 获取 ProjectConf
                var projectConfField = conf.GetType().GetField("ProjectConf");
                if (projectConfField == null) return true;

                var projectConf = projectConfField.GetValue(conf);
                if (projectConf == null) return true;

                // 检查 relativeDST
                var relativeDstField = projectConf.GetType().GetField("relativeDST");
                if (relativeDstField == null) return true;

                string relativeDst = relativeDstField.GetValue(projectConf) as string;
                if (!string.IsNullOrEmpty(relativeDst))
                {
                    return true; // 已配置
                }

                // 未配置，弹窗让用户选择
                int choice = EditorUtility.DisplayDialogComplex(
                    "导出路径未配置",
                    "微信小游戏导出路径尚未设置。\n\n" +
                    $"• 点击「使用默认」将导出到项目根目录下的 {DEFAULT_EXPORT_DIR}/\n" +
                    "• 点击「自定义」选择其他目录\n" +
                    "• 点击「取消」中止构建",
                    "使用默认", "取消", "自定义");

                string selectedPath = null;

                switch (choice)
                {
                    case 0: // 使用默认
                        selectedPath = DEFAULT_EXPORT_DIR;
                        break;
                    case 1: // 取消
                        return false;
                    case 2: // 自定义
                        string absPath = EditorUtility.SaveFolderPanel(
                            "选择游戏导出目录", "", "");
                        if (string.IsNullOrEmpty(absPath))
                        {
                            return false; // 用户取消了文件夹选择
                        }
                        selectedPath = absPath;
                        break;
                }

                if (string.IsNullOrEmpty(selectedPath))
                {
                    return false;
                }

                // 写入 relativeDST
                relativeDstField.SetValue(projectConf, selectedPath);

                // 同步写入 DST（绝对路径）
                var dstField = projectConf.GetType().GetField("DST");
                if (dstField != null)
                {
                    string absExportPath;
                    if (Path.IsPathRooted(selectedPath))
                    {
                        absExportPath = selectedPath;
                    }
                    else
                    {
                        // 相对路径基于项目根目录
                        string projectRoot = Path.GetFullPath(
                            Path.Combine(Application.dataPath, ".."));
                        absExportPath = Path.Combine(projectRoot, selectedPath);
                    }
                    dstField.SetValue(projectConf, absExportPath);
                }

                // 回写 ProjectConf（struct 可能需要回写）
                projectConfField.SetValue(conf, projectConf);

                // 标记 ScriptableObject 脏，保存配置
                if (conf is ScriptableObject so)
                {
                    EditorUtility.SetDirty(so);
                    AssetDatabase.SaveAssets();
                }

                UnityEngine.Debug.Log(
                    $"[{TAG}] Export path configured: {selectedPath}");

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{TAG}] Failed to check export path: {e.Message}");
                return true; // 检查失败放行，让 SDK 自己处理
            }
        }

        /// <summary>
        /// 通过反射调用 WXConvertCore.DoExport(bool)。
        /// 使用反射避免 GameFramework.Editor 直接引用 WxEditor 程序集。
        /// </summary>
        /// <param name="buildWebGL">true=完整构建+转换，false=仅转换</param>
        private static void InvokeDoExport(bool buildWebGL)
        {
            try
            {
                Type convertType = FindType(WX_CONVERT_CORE_TYPE);
                if (convertType == null)
                {
                    UnityEngine.Debug.LogError(
                        $"[{TAG}] Type not found: {WX_CONVERT_CORE_TYPE}");
                    return;
                }

                // WXConvertCore.DoExport(bool buildWebGL)
                MethodInfo doExport = convertType.GetMethod(
                    "DoExport",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(bool) },
                    null);

                if (doExport == null)
                {
                    // 尝试无参版本
                    doExport = convertType.GetMethod(
                        "DoExport",
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        Type.EmptyTypes,
                        null);

                    if (doExport != null)
                    {
                        UnityEngine.Debug.Log(
                            $"[{TAG}] Starting export (parameterless)...");
                        var result = doExport.Invoke(null, null);
                        LogExportResult(result);
                        return;
                    }

                    UnityEngine.Debug.LogError(
                        $"[{TAG}] Method DoExport not found on {WX_CONVERT_CORE_TYPE}");
                    return;
                }

                UnityEngine.Debug.Log(
                    $"[{TAG}] Starting export (buildWebGL={buildWebGL})...");

                var exportResult = doExport.Invoke(null, new object[] { buildWebGL });
                LogExportResult(exportResult);
            }
            catch (TargetInvocationException tie)
            {
                UnityEngine.Debug.LogError(
                    $"[{TAG}] Export failed: {tie.InnerException?.Message ?? tie.Message}");
                UnityEngine.Debug.LogException(tie.InnerException ?? tie);

                EditorUtility.DisplayDialog(
                    "构建失败",
                    $"构建过程中出错：\n{tie.InnerException?.Message ?? tie.Message}\n\n" +
                    "请查看 Console 日志获取详细信息。",
                    "确定");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[{TAG}] Export failed: {e.Message}");
                UnityEngine.Debug.LogException(e);

                EditorUtility.DisplayDialog(
                    "构建失败",
                    $"构建过程中出错：\n{e.Message}\n\n" +
                    "请查看 Console 日志获取详细信息。",
                    "确定");
            }
        }

        /// <summary>
        /// 记录导出结果日志
        /// </summary>
        private static void LogExportResult(object result)
        {
            if (result == null)
            {
                UnityEngine.Debug.Log($"[{TAG}] Export completed.");
                return;
            }

            // WXExportError 枚举：SUCCEED=0, NODE_NOT_FOUND=1, BUILD_WEBGL_FAILED=2
            int code = Convert.ToInt32(result);
            switch (code)
            {
                case 0:
                    UnityEngine.Debug.Log(
                        $"[{TAG}] Export succeeded!");

                    string exportPath = GetExportPath();
                    string minigamePath = !string.IsNullOrEmpty(exportPath)
                        ? Path.Combine(exportPath, "minigame") : "";

                    string successMsg = "构建成功！\n\n";
                    if (!string.IsNullOrEmpty(minigamePath) &&
                        Directory.Exists(minigamePath))
                    {
                        successMsg += $"产物目录：{minigamePath}\n\n";
                    }
                    successMsg += "下一步：\n" +
                        "1. 打开微信开发者工具\n" +
                        "2. 导入 minigame 目录\n" +
                        "3. 填入 AppID\n" +
                        "4. 预览/真机调试/上传";

                    EditorUtility.DisplayDialog("构建成功", successMsg, "确定");
                    break;

                case 1:
                    UnityEngine.Debug.LogError(
                        $"[{TAG}] Export failed: Node.js not found.");
                    EditorUtility.DisplayDialog(
                        "构建失败",
                        "未找到 Node.js，SDK 转换需要 Node.js 环境。\n\n" +
                        "请安装 Node.js：https://nodejs.org/",
                        "确定");
                    break;

                case 2:
                    UnityEngine.Debug.LogError(
                        $"[{TAG}] Export failed: WebGL build failed.");
                    EditorUtility.DisplayDialog(
                        "构建失败",
                        "WebGL 构建失败，请查看 Console 日志排查编译错误。",
                        "确定");
                    break;

                default:
                    UnityEngine.Debug.LogWarning(
                        $"[{TAG}] Export returned unknown code: {code}");
                    break;
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取 SDK 配置中的导出路径
        /// </summary>
        private static string GetExportPath()
        {
            try
            {
                Type utilType = FindType(WX_UNITY_UTIL_TYPE);
                if (utilType == null) return null;

                MethodInfo getConf = utilType.GetMethod(
                    "GetEditorConf",
                    BindingFlags.Public | BindingFlags.Static);
                if (getConf == null) return null;

                var conf = getConf.Invoke(null, null);
                if (conf == null) return null;

                // 获取 ProjectConf 属性
                var projectConfField = conf.GetType().GetField("ProjectConf");
                if (projectConfField == null) return null;

                var projectConf = projectConfField.GetValue(conf);
                if (projectConf == null) return null;

                // 获取 DST 字段
                var dstField = projectConf.GetType().GetField("DST");
                return dstField?.GetValue(projectConf) as string;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 在所有已加载程序集中查找类型
        /// </summary>
        private static Type FindType(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(fullTypeName);
                    if (type != null) return type;
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
            return null;
        }

        #endregion
    }
}
#endif
