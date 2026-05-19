using UnityEditor;
using UnityEngine;

/// <summary>
/// ThiefSim → Build Windows EXE
/// Builds a standalone Windows x64 player into /Build/ThiefSimulator/
/// </summary>
public static class BuildScript
{
    [MenuItem("ThiefSim/Build Windows EXE")]
    public static void BuildWindows()
    {
        string outputPath = "Build/ThiefSimulator/ThiefSimulator.exe";

        // Make sure scene is in build settings
        string scenePath = "Assets/Scenes/SampleScene.unity";
        // Try MainGame first
        if (System.IO.File.Exists("Assets/Scenes/MainGame.unity"))
            scenePath = "Assets/Scenes/MainGame.unity";

        var buildOptions = new BuildPlayerOptions
        {
            scenes          = new[] { scenePath },
            locationPathName = outputPath,
            target          = BuildTarget.StandaloneWindows64,
            options         = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"[ThiefSim] Build SUCCESS → {outputPath}");
        else
            Debug.LogError($"[ThiefSim] Build FAILED: {report.summary.totalErrors} errors");
    }
}
