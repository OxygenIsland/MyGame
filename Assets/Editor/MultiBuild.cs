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
            int stage = 0;
            Settings settings = new Settings();
            settings.outputFolder = $"./Release/{DateTime.Now:yyyy-MM-dd}/";

            for (int i = 0; i < args.Length; ++i)
            {
                switch (stage)
                {
                    case 0:
                        if (args[i].Equals("MultiBuild.Builder.BuildCommandLine"))
                            stage++;
                        break;
                    case 1: // target
                        settings.target = GetTarget(args[i]);
                        settings.platform_name = args[i];
                        stage++;
                        break;
                    case 2: // version
                        settings.version = args[i] + GetDateTime("MMdd");
                        stage++;
                        break;
                    case 3: // stage
                        settings.stage = args[i];
                        stage++;
                        break;
                    case 4: // gitId
                        settings.gitId = args[i];
                        stage++;
                        break;
                    case 5: // developmentBuild
                        if (!bool.TryParse(args[i], out settings.developmentBuild))
                            throw new ArgumentException("developmentBuild 不是有效的布尔值: " + args[i]);
                        stage++;
                        break;
                    case 7: // domain
                        if (args[i].Equals("All"))
                            PCUpdateUrl = PCUploadUrl_TESTNG;
                        stage++;
                        break;
                }
            }

            Build(settings);
        }

        #endregion

        #region 手动打包入口（菜单）

        public static void BuildPC()
        {
            Settings settings = new Settings();
            settings.outputFolder = $"./Release/{DateTime.Now:yyyy-MM-dd}/";
            settings.target = GetTarget("Windows");
            settings.platform_name = "Windows";
            settings.version = "1.2.0" + GetDateTime("MMdd");
            settings.stage = "Release";
            settings.gitId = "gitidforTmpBuild";
            PCUpdateUrl = null;

            Build(settings);
        }

        [MenuItem("MultiBuild/BuildPC/云渲染", priority = 0)]
        public static void BuildPCCloudRendering()
        {
            AddDefineSymbol(BuildTargetGroup.Standalone, "CLOUD_RENDER");
            BuildPC();
        }

        [MenuItem("MultiBuild/BuildPC/客户端/部署版", priority = 1)]
        public static void BuildPCStandaloneForDeploy()
        {
            RemoveDefineSymbol(BuildTargetGroup.Standalone, "CLOUD_RENDER");
            BuildPC();
        }

        [MenuItem("MultiBuild/BuildPC/客户端/操控版", priority = 2)]
        public static void BuildPCStandaloneForCtrl()
        {
            RemoveDefineSymbol(BuildTargetGroup.Standalone, "CLOUD_RENDER");
            BuildPC();
        }

        [MenuItem("MultiBuild/BuildWebGL", priority = 3)]
        public static void BuildWebGL()
        {
            Settings settings = new Settings();
            settings.target = GetTarget("WebGL");
            settings.platform_name = "WebGL";
            string[] vp = PlayerSettings.bundleVersion.Split('.');
            settings.version = $"{vp[0]}.{vp[1]}.{vp[2][0]}{GetDateTime("MMdd")}";
            string folderToday = DateTime.Now.ToString("yyyy-MM-dd");
            settings.outputFolder = $"./Release/{folderToday}/";
            settings.stage = "Release";
            settings.gitId = "gitidforTmpBuild";
            settings.developmentBuild = false;

            Build(settings);
            OpenInFolder(Path.Combine(Application.dataPath, $"../Release/{folderToday}/"));
        }

        [MenuItem("MultiBuild/BuildAndroid", priority = 4)]
        public static void BuildAndroid()
        {
            Settings settings = new Settings();
            settings.target = GetTarget("Android");
            settings.platform_name = "Android";
            string[] vp = PlayerSettings.bundleVersion.Split('.');
            settings.version = $"{vp[0]}.{vp[1]}.{vp[2][0]}{GetDateTime("MMdd")}";
            settings.outputFolder = "./Release/";
            settings.stage = "Release";
            settings.gitId = "gitidforTmpBuild";
            settings.developmentBuild = false;

            Build(settings);
            OpenInFolder(Path.Combine(Application.dataPath, "../Release/"));
        }

        [MenuItem("MultiBuild/Upload2Minio/RobotTest-0", priority = 5)]
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
            ApplyBuildSettings(settings);
            BuildPlayerOptions buildSteps = BuildOpts(settings);
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildSteps.targetGroup, buildSteps.target);
            var report = BuildPipeline.BuildPlayer(buildSteps);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Build error: See log");

            if (string.IsNullOrEmpty(versionPath))
                versionPath = settings.outputFolder;
            ReportVersion(settings.version, versionPath);
            return true;
        }

        private static void ReportVersion(string version, string outputPath)
        {
            File.WriteAllText(Path.Combine(outputPath, "version.txt"), version);
        }

        public static BuildPlayerOptions BuildOpts(Settings settings)
        {
            BuildPlayerOptions o = new BuildPlayerOptions();
            o.scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
            o.target = settings.target;

            string subFolderName = $"{settings.version}_{settings.stage}";
            if (o.target == BuildTarget.StandaloneWindows64)
            {
                o.targetGroup = BuildTargetGroup.Standalone;
                settings.outputFolder += $"/{subFolderName}/";
                o.locationPathName = Path.Combine(settings.outputFolder, PlayerSettings.productName + ".exe");
            }
            else if (o.target == BuildTarget.Android)
            {
                o.targetGroup = BuildTargetGroup.Android;
                o.locationPathName = Path.Combine(settings.outputFolder, $"{PlayerSettings.productName}_{Application.version}.apk");
            }
            else if (o.target == BuildTarget.WebGL)
            {
                o.targetGroup = BuildTargetGroup.WebGL;
                string version = Application.version.Replace(".", "");
                o.locationPathName = Path.Combine(settings.outputFolder, WebGLExportedFolderName + version + DateTime.Now.ToString("HHmm"));
                versionPath = o.locationPathName;
            }

            if (settings.developmentBuild)
                o.options = BuildOptions.Development;
            return o;
        }

        #endregion

        #region PlayerSettings 配置

        public static void ApplyBuildSettings(Settings settings)
        {
            PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
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
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (defines.Contains(symbol))
            {
                Debug.Log($"BuildSetting中已存在\"{symbol}\"宏定义");
                return;
            }

            defines = string.IsNullOrEmpty(defines) ? symbol : $"{defines};{symbol}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"已添加BuildSetting中的\"{symbol}\"宏定义");
        }

        public static void RemoveDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defines.Contains(symbol))
            {
                Debug.Log($"BuildSetting中未找到\"{symbol}\"宏定义");
                return;
            }

            defines = defines.Replace($"{symbol};", "").Replace($";{symbol}", "").Replace(symbol, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
            Debug.Log($"已移除BuildSetting中的\"{symbol}\"宏定义");
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
    }
}
