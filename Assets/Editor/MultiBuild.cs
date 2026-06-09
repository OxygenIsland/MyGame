using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiBuild
{
    public static class Builder
    {
        #region 常量与字段

        public const string WebGLExportedFolderName = "WebGL";

        public const string PCUploadUrl_DEV =
            "https://dev-cloudrenderscheduler-disp-serv.lenovo-r.cloud/api/v3/dispatcher/apps";
        public const string PCUploadUrl_TESTNG =
            "https://testng-starworld.lenovo-r.cloud:30000/api/v3/dispatcher/apps";

        public static string PCUpdateUrl = PCUploadUrl_DEV;

        private static string versionPath = null;

        #endregion

        #region CI/CD 命令行入口

        public static void BuildCommandLine()
        {
            // args 顺序：<target> <version> <stage> <GIT_COMMIT> <isDevelop> <useEditorScenes> <domain>
            var args = System.Environment.GetCommandLineArgs();
            var settings = ParseCommandLineSettings(args);
            Build(settings);
        }

        private static Settings ParseCommandLineSettings(string[] args)
        {
            const string EntryPoint = "MultiBuild.Builder.BuildCommandLine";
            int entryIndex = Array.IndexOf(args, EntryPoint);
            if (entryIndex < 0)
                throw new ArgumentException($"未找到命令行入口参数: {EntryPoint}");

            var payload = args.Skip(entryIndex + 1).ToArray();
            if (payload.Length < 5)
            {
                throw new ArgumentException(
                    "命令行参数不足，至少需要: <target> <version> <stage> <GIT_COMMIT> <isDevelop>"
                );
            }

            var settings = new Settings
            {
                outputFolder = $"./Release/{DateTime.Now:yyyy-MM-dd}/",
                target = GetTarget(payload[0]),
                platform_name = payload[0],
                version = payload[1] + GetDateTime("MMdd"),
                stage = payload[2],
                gitId = payload[3]
            };

            if (!bool.TryParse(payload[4], out settings.developmentBuild))
                throw new ArgumentException("developmentBuild 不是有效的布尔值: " + payload[4]);

            if (payload.Length >= 6 && bool.TryParse(payload[5], out bool useEnabledEditorScenes))
                settings.useEnabledEditorScenes = useEnabledEditorScenes;

            if (payload.Length >= 7 && payload[6].Equals("All", StringComparison.OrdinalIgnoreCase))
                PCUpdateUrl = PCUploadUrl_TESTNG;

            // 可选：payload[7] 传入逗号分隔的场景路径，优先级高于 useEnabledEditorScenes。
            if (payload.Length >= 8)
            {
                settings.scenePaths = payload[7]
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            return settings;
        }

        #endregion

        #region 手动打包入口（菜单）

        [MenuItem("MultiBuild/BuildPC", priority = 0)]
        public static void BuildPC()
        {
            Settings settings = CreateMenuBuildSettings(
                "Windows",
                "1.2.0" + GetDateTime("MMdd"),
                $"./Release/{DateTime.Now:yyyy-MM-dd}/"
            );
            PCUpdateUrl = null;

            Build(settings);
        }

        [MenuItem("MultiBuild/BuildWebGL", priority = 1)]
        public static void BuildWebGL()
        {
            string[] vp = PlayerSettings.bundleVersion.Split('.');
            string version = $"{vp[0]}.{vp[1]}.{vp[2][0]}{GetDateTime("MMdd")}";
            string folderToday = DateTime.Now.ToString("yyyy-MM-dd");
            Settings settings = CreateMenuBuildSettings("WebGL", version, $"./Release/{folderToday}/");

            Build(settings);
            OpenInFolder(Path.Combine(Application.dataPath, $"../Release/{folderToday}/"));
        }

        [MenuItem("MultiBuild/BuildAndroid", priority = 2)]
        public static void BuildAndroid()
        {
            string[] vp = PlayerSettings.bundleVersion.Split('.');
            string version = $"{vp[0]}.{vp[1]}.{vp[2][0]}{GetDateTime("MMdd")}";
            Settings settings = CreateMenuBuildSettings("Android", version, "./Release/");

            Build(settings);
            OpenInFolder(Path.Combine(Application.dataPath, "../Release/"));
        }

        [MenuItem("MultiBuild/Upload2Minio/RobotTest-0", priority = 3)]
        public static void UploadToMinIOToTest0() => UploadToMinIO("unity3D-demo-0");

        #endregion

        #region MinIO 上传

        private static void UploadToMinIO( string minioFolder = "unity3D-demo-0")
        {
            string pythonScript = Path.Combine(Application.dataPath, "../Tools/upload_file_to_minio.py");
            string bucketName = "starworld";
            string prefix = $"static/{minioFolder}";
            string folder = Path.Combine(Application.dataPath, "../Release");

            if (!Directory.Exists(folder))
            {
                Debug.LogError("文件不存在: " + folder);
                return;
            }

            Debug.Log("开始上传 MinIO");
            ShellHelper.RunPythonAsync(
                $"{pythonScript} {bucketName} {prefix} {folder}",
                result =>
                {
                    if (!result.Success)
                        Debug.LogError($"MinIO 上传失败（退出码 {result.ExitCode}）");
                    else
                        Debug.Log("MinIO 上传完成");
                }
            );
        }

        #endregion

        #region 核心打包逻辑

        private static BuildTarget GetTarget(string arg) => arg switch
        {
            "Android" => BuildTarget.Android,
            "Windows" => BuildTarget.StandaloneWindows64,
            "UWP"     => BuildTarget.WSAPlayer,
            "WebGL"   => BuildTarget.WebGL,
            _         => throw new ArgumentException($"Invalid target '{arg}'")
        };

        public static bool Build(Settings settings)
        {
            versionPath = null;
            ValidateSettings(settings);
            var context = BuildEditorContext.Capture();
            try
            {
                RunBuildStage("应用 PlayerSettings", () => ApplyBuildSettings(settings));
                BuildPlayerOptions buildSteps = RunBuildStage("生成 BuildPlayerOptions", () => BuildOpts(settings));
                bool addedUrpDefine = RunBuildStage(
                    "设置 URP 兼容模式宏",
                    () => EnsureDefineSymbol(buildSteps.targetGroup, "URP_COMPATIBILITY_MODE")
                );
                if (addedUrpDefine)
                {
                    throw new InvalidOperationException(
                        "已自动添加 URP_COMPATIBILITY_MODE 宏。Unity 需要先完成脚本重编译，请再次执行打包。"
                    );
                }
                RunBuildStage(
                    "切换构建目标",
                    () => EditorUserBuildSettings.SwitchActiveBuildTarget(buildSteps.targetGroup, buildSteps.target)
                );
                var report = RunBuildStage("执行 BuildPipeline.BuildPlayer", () => BuildPipeline.BuildPlayer(buildSteps));
                PrintBuildSummary(report, buildSteps.locationPathName);
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"Build error: {report.summary.result}, 输出: {buildSteps.locationPathName}");

                RunBuildStage(
                    "写入版本文件",
                    () =>
                    {
                        if (string.IsNullOrEmpty(versionPath))
                            versionPath = settings.outputFolder;
                        Directory.CreateDirectory(versionPath);
                        ReportVersion(settings.version, versionPath);
                    }
                );
                return true;
            }
            finally
            {
                RunBuildStage("恢复编辑器上下文", context.Restore);
            }
        }

        private static void ReportVersion(string version, string outputPath)
        {
            File.WriteAllText(Path.Combine(outputPath, "version.txt"), version);
        }

        public static BuildPlayerOptions BuildOpts(Settings settings)
        {
            BuildPlayerOptions o = new BuildPlayerOptions();
            o.scenes = ResolveBuildScenes(settings);
            o.target = settings.target;

            string subFolderName = $"{settings.version}_{settings.stage}";
            if (o.target == BuildTarget.StandaloneWindows64)
            {
                o.targetGroup = BuildTargetGroup.Standalone;
                settings.outputFolder += $"/{subFolderName}/";
                Directory.CreateDirectory(settings.outputFolder);
                o.locationPathName = Path.Combine(settings.outputFolder, PlayerSettings.productName + ".exe");
            }
            else if (o.target == BuildTarget.Android)
            {
                o.targetGroup = BuildTargetGroup.Android;
                Directory.CreateDirectory(settings.outputFolder);
                o.locationPathName = Path.Combine(settings.outputFolder, $"{PlayerSettings.productName}_{Application.version}.apk");
            }
            else if (o.target == BuildTarget.WebGL)
            {
                o.targetGroup = BuildTargetGroup.WebGL;
                string version = Application.version.Replace(".", "");
                o.locationPathName = Path.Combine(settings.outputFolder, WebGLExportedFolderName + version + DateTime.Now.ToString("HHmm"));
                Directory.CreateDirectory(o.locationPathName);
                versionPath = o.locationPathName;
            }

            o.options = GetBuildOptions(settings, o.target);
            return o;
        }

        private static string[] ResolveBuildScenes(Settings settings)
        {
            if (settings.scenePaths is { Length: > 0 })
                return settings.scenePaths;

            if (settings.useEnabledEditorScenes)
                return EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();

            return EditorBuildSettings.scenes.Select(x => x.path).ToArray();
        }

        private static BuildOptions GetBuildOptions(Settings settings, BuildTarget target)
        {
            BuildOptions options = BuildOptions.None;
            bool debugStage = string.Equals(settings.stage, "Debug", StringComparison.OrdinalIgnoreCase);

            if (settings.developmentBuild || debugStage)
                options |= BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;

            // WebGL 不使用此压缩选项，避免平台差异导致的无效配置。
            if (target != BuildTarget.WebGL && !settings.developmentBuild)
                options |= BuildOptions.CompressWithLz4HC;

            return options;
        }

        private static void RunBuildStage(string stageName, Action action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Debug.Log($"[MultiBuild] 开始阶段: {stageName}");
            try
            {
                action();
                Debug.Log($"[MultiBuild] 完成阶段: {stageName}, 耗时 {watch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiBuild] 阶段失败: {stageName}, 耗时 {watch.ElapsedMilliseconds} ms, 异常: {ex.Message}");
                throw;
            }
        }

        private static T RunBuildStage<T>(string stageName, Func<T> action)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Debug.Log($"[MultiBuild] 开始阶段: {stageName}");
            try
            {
                var result = action();
                Debug.Log($"[MultiBuild] 完成阶段: {stageName}, 耗时 {watch.ElapsedMilliseconds} ms");
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiBuild] 阶段失败: {stageName}, 耗时 {watch.ElapsedMilliseconds} ms, 异常: {ex.Message}");
                throw;
            }
        }

        private static Settings CreateMenuBuildSettings(string platformName, string version, string outputFolder)
        {
            return new Settings
            {
                outputFolder = outputFolder,
                target = GetTarget(platformName),
                platform_name = platformName,
                version = version,
                stage = "Release",
                gitId = "gitidforTmpBuild",
                developmentBuild = false,
                useEnabledEditorScenes = true
            };
        }

        private static void ValidateSettings(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(settings.version))
                throw new ArgumentException("version 不能为空");

            if (string.IsNullOrWhiteSpace(settings.stage))
                throw new ArgumentException("stage 不能为空");

            if (string.IsNullOrWhiteSpace(settings.outputFolder))
                throw new ArgumentException("outputFolder 不能为空");

            var scenes = ResolveBuildScenes(settings);
            if (scenes.Length == 0)
                throw new InvalidOperationException("没有可用于构建的场景，请检查 Build Settings 或命令行场景参数");

            foreach (var scene in scenes)
            {
                if (!File.Exists(scene))
                    throw new FileNotFoundException("构建场景不存在", scene);
            }
        }

        private static void PrintBuildSummary(BuildReport report, string outputPath)
        {
            var s = report.summary;
            Debug.Log(
                "[MultiBuild] Build Summary => "
                + $"result={s.result}, "
                + $"platform={s.platform}, "
                + $"output={outputPath}, "
                + $"time={s.totalTime.TotalSeconds:F2}s, "
                + $"warnings={s.totalWarnings}, errors={s.totalErrors}, "
                + $"size={s.totalSize} bytes"
            );
        }

        private readonly struct BuildEditorContext
        {
            private readonly BuildTarget _activeBuildTarget;
            private readonly string _bundleVersion;
            private readonly bool _runInBackground;
            private readonly WebGLExceptionSupport _webGLExceptionSupport;

            private BuildEditorContext(
                BuildTarget activeBuildTarget,
                string bundleVersion,
                bool runInBackground,
                WebGLExceptionSupport webGLExceptionSupport
            )
            {
                _activeBuildTarget = activeBuildTarget;
                _bundleVersion = bundleVersion;
                _runInBackground = runInBackground;
                _webGLExceptionSupport = webGLExceptionSupport;
            }

            public static BuildEditorContext Capture()
            {
                return new BuildEditorContext(
                    EditorUserBuildSettings.activeBuildTarget,
                    PlayerSettings.bundleVersion,
                    PlayerSettings.runInBackground,
                    PlayerSettings.WebGL.exceptionSupport
                );
            }

            public void Restore()
            {
                PlayerSettings.bundleVersion = _bundleVersion;
                PlayerSettings.runInBackground = _runInBackground;
                PlayerSettings.WebGL.exceptionSupport = _webGLExceptionSupport;

                var targetGroup = BuildPipeline.GetBuildTargetGroup(_activeBuildTarget);
                if (targetGroup != BuildTargetGroup.Unknown)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, _activeBuildTarget);
            }
        }

        #endregion

        #region PlayerSettings 配置

        public static void ApplyBuildSettings(Settings settings)
        {
            PlayerSettings.bundleVersion = settings.version;
            PlayerSettings.runInBackground = true;

            if (settings.target == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.exceptionSupport = settings.stage.Equals("Debug")
                    ? WebGLExceptionSupport.FullWithStacktrace
                    : WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            }
        }

        public static void OpenInFolder(string folderPath)
        {
            Application.OpenURL("file:///" + folderPath);
        }

        #endregion

        #region HTTP 文件上传

        public static IEnumerator UploadFile(string uploadUrl, string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl) || !File.Exists(fileUrl))
                yield break;

            string fileName = Path.GetFileName(fileUrl);
            string deleteUrl = $"{uploadUrl}/{Path.GetFileNameWithoutExtension(fileUrl)}";

            Debug.Log("MultiBuild delFile: " + deleteUrl);
            using (UnityWebRequest requestDel = UnityWebRequest.Delete(deleteUrl))
            {
                yield return requestDel.SendWebRequest();
                if (requestDel.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                    Debug.LogWarning("MultiBuild delFile Error: " + requestDel.error);
            }

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", File.ReadAllBytes(fileUrl), fileName, "application/octet-stream");

            using (UnityWebRequest requestUpload = UnityWebRequest.Post(uploadUrl, form))
            {
                yield return requestUpload.SendWebRequest();
                if (requestUpload.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                    Debug.LogError("MultiBuild UploadFile Error: " + requestUpload.error);
                else
                    Debug.Log("MultiBuild UploadFile complete: " + requestUpload.downloadHandler.text);
            }
        }

        #endregion

        #region 宏定义管理

        public static void AddDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            string defines = GetScriptingDefineSymbols(buildTargetGroup);
            var defineList = defines
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (defineList.Contains(symbol))
            {
                Debug.Log($"BuildSetting中已存在\"{symbol}\"宏定义");
                return;
            }

            defineList.Add(symbol);
            SetScriptingDefineSymbols(buildTargetGroup, string.Join(";", defineList));
            Debug.Log($"已添加BuildSetting中的\"{symbol}\"宏定义");
        }

        public static void RemoveDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            string defines = GetScriptingDefineSymbols(buildTargetGroup);
            var defineList = defines
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (!defineList.Contains(symbol))
            {
                Debug.Log($"BuildSetting中未找到\"{symbol}\"宏定义");
                return;
            }

            defineList.RemoveAll(x => x == symbol);
            SetScriptingDefineSymbols(buildTargetGroup, string.Join(";", defineList));
            Debug.Log($"已移除BuildSetting中的\"{symbol}\"宏定义");
        }

        private static string GetScriptingDefineSymbols(BuildTargetGroup buildTargetGroup)
        {
#if UNITY_2021_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            return PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
        }

        private static void SetScriptingDefineSymbols(BuildTargetGroup buildTargetGroup, string defines)
        {
#if UNITY_2021_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
#endif
        }

        private static bool EnsureDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            string defines = GetScriptingDefineSymbols(buildTargetGroup);
            var defineList = defines
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (defineList.Contains(symbol))
                return false;

            defineList.Add(symbol);
            SetScriptingDefineSymbols(buildTargetGroup, string.Join(";", defineList));
            Debug.Log($"[MultiBuild] 已为 {buildTargetGroup} 添加宏: {symbol}");
            return true;
        }

        #endregion

        #region 工具方法

        private static string GetDateTime(string format) => DateTime.Now.ToString(format);

        #endregion
    }

    public class Settings
    {
        public BuildTarget target;
        public string version;
        public string stage;
        public string platform_name;
        public int bundleVersionCode;
        internal string gitId;

        internal string outputFolder;
        internal bool developmentBuild;
        internal bool useEnabledEditorScenes = true;
        internal string[] scenePaths;
    }
}
