#if UNITY_SERVER
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bcom.SharedPlayground
{
class BuildServer
{
    // Invoked via command line only
    static void PerformDedicatedServerBuild()
    {
        // As a fallback use <project root>/BUILD as output path
        var buildPath = Path.Combine(Application.dataPath, "BUILD");

        // read in command line arguments e.g. add "-buildPath some/Path" if you want a different output path 
        var args = Environment.GetCommandLineArgs();

        string targetName = "Linux64";
        string fileExtension = ""; 
        BuildTarget target = BuildTarget.StandaloneLinux64;
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath")
            {
                buildPath = args[++i];
                Debug.Log("building at " + buildPath);
            }
            else if (args[i] == "-target")
            {
                targetName = args[++i];
                if (targetName == "Linux64")
                {
                    target = BuildTarget.StandaloneLinux64;
                }
                else if (targetName == "Win64")
                {
                    target = BuildTarget.StandaloneWindows64;
                    fileExtension = ".exe";
                }
                else
                {
                    Debug.LogError($"Unknown target: {targetName}");
                    return;
                }
            }
        }

        // if the output folder doesn't exist create it now
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = Path.Combine(buildPath, targetName, "SharedPlayground" + fileExtension);
        buildPlayerOptions.scenes = new[]{ "Packages/com.b-com.shared-playground/Samples/Demo/DemoScene.unity" };
        buildPlayerOptions.target = target;
        buildPlayerOptions.subtarget = (int)StandaloneBuildSubtarget.Server;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }
}
}
#endif