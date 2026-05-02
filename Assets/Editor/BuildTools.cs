using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 构建工具：通过菜单或命令行调用
/// </summary>
public class BuildTools
{
    [MenuItem("Build/PC 导出 (Windows)", false, 1)]
    public static void BuildPC()
    {
        // 配置场景列表
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/ResultScene.unity"
        };

        // 检查场景是否存在
        foreach (string scene in scenes)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
            {
                Debug.LogError($"场景不存在: {scene}");
                EditorUtility.DisplayDialog("导出失败", $"场景不存在: {scene}", "确定");
                return;
            }
        }

        // 配置 PlayerSettings
        PlayerSettings.companyName = "Talentcong";
        PlayerSettings.productName = "英语单词闯关";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, "com.Talentcong.WordQuest");
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

        // 输出路径（项目根目录）
        string projectPath = System.IO.Path.GetFullPath(".");
        string outputFile = projectPath + "/英语单词闯关.exe";

        // 执行构建
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFile,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"PC 导出成功: {outputFile}");
            EditorUtility.DisplayDialog("导出成功", $"PC 导出完成！\n{outputFile}", "确定");
        }
        else
        {
            Debug.LogError($"PC 导出失败: {report.summary.result}");
            EditorUtility.DisplayDialog("导出失败", $"PC 导出失败，请查看 Console 日志", "确定");
        }
    }

    [MenuItem("Build/PC 导出 (命令行)", false, 2)]
    private static void BuildPCCmd()
    {
        // 检查是否在命令行模式运行
        if (System.Environment.GetCommandLineArgs().Length > 1)
            BuildPC();
    }

    [MenuItem("Build/Android APK 导出", false, 10)]
    public static void BuildAndroid()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameScene.unity",
            "Assets/Scenes/ResultScene.unity"
        };

        foreach (string scene in scenes)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
            {
                Debug.LogError($"场景不存在: {scene}");
                EditorUtility.DisplayDialog("导出失败", $"场景不存在: {scene}", "确定");
                return;
            }
        }

        // 切换到 Android 平台
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("切换到 Android 平台失败");
                EditorUtility.DisplayDialog("导出失败", "切换到 Android 平台失败，请确保已安装 Android Build Support", "确定");
                return;
            }
        }

        // 配置 Android PlayerSettings
        PlayerSettings.companyName = "Talentcong";
        PlayerSettings.productName = "英语单词闯关";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.Talentcong.WordQuest");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        // 输出路径
        string projectPath = System.IO.Path.GetFullPath(".");
        string outputFile = projectPath + "/英语单词闯关.apk";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputFile,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"APK 导出成功: {outputFile}");
            EditorUtility.DisplayDialog("导出成功", $"APK 导出完成！\n{outputFile}", "确定");
        }
        else
        {
            Debug.LogError($"APK 导出失败: {report.summary.result}");
            EditorUtility.DisplayDialog("导出失败", $"APK 导出失败，请查看 Console 日志", "确定");
        }
    }
}
