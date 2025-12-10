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
        string originalPath = "Assets/Oculus/OculusProjectConfig.asset";
        string tempResourcePath = "Assets/Resources/OculusProjectConfig.asset";
        bool assetCopied = false;

        try
        {
            // The Oculus SDK build hooks expect the config asset to be in a Resources folder.
            // If it's not there, we temporarily copy it for the duration of the build.
            if (!File.Exists(tempResourcePath))
            {
                Debug.Log($"'{tempResourcePath}' not found. Attempting to copy from '{originalPath}'.");
                if (File.Exists(originalPath))
                {
                    // Ensure the 'Assets/Resources' directory exists.
                    if (!Directory.Exists("Assets/Resources"))
                    {
                        Directory.CreateDirectory("Assets/Resources");
                    }

                    AssetDatabase.CopyAsset(originalPath, tempResourcePath);
                    assetCopied = true;
                    AssetDatabase.Refresh(); // Make sure Unity's asset database sees the new file.
                    Debug.Log("Asset copied successfully.");
                }
                else
                {
                    Debug.LogError($"The source asset '{originalPath}' was not found. The build cannot proceed.");
                    EditorApplication.Exit(1);
                    return;
                }
            }

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
        finally
        {
            // If we copied the asset, clean it up to leave the project state unchanged.
            if (assetCopied)
            {
                Debug.Log($"Cleaning up temporary asset '{tempResourcePath}'.");
                AssetDatabase.DeleteAsset(tempResourcePath);
                AssetDatabase.Refresh();
            }
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