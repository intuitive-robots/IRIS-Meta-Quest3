
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildCommand
{
    private const string BUILD_ROOT = "Builds";

    public static void BuildAndroid()
    {
        var buildOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = GetBuildPath(),
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build successful - Build written to {buildOptions.locationPathName}");
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }

    private static string GetBuildPath()
    {
        var buildDir = Path.Combine(BUILD_ROOT, "Android");
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }
        return Path.Combine(buildDir, "IRIS-Meta-Quest3.apk");
    }

    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }
}
