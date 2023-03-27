#if UNITY_EDITOR && UNITY_SERVER
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

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildPath")
            {
                buildPath = args[i + 1];
                Debug.Log("building at " + buildPath);
            }
        }

        // if the output folder doesn't exist create it now
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        BuildTarget target = BuildTarget.StandaloneLinux64;
        string ext = target == BuildTarget.StandaloneWindows64 ? ".exe" : "";
        string archDir = target == BuildTarget.StandaloneWindows64 ? "Win64/" : "Linux64/";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = Path.Combine(buildPath, archDir + "SharedPlayground" + ext);
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