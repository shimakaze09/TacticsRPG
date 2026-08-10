using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// CI player build (issue #7): regenerates content through the validated
/// pipeline, then builds the enabled scenes. Used as game-ci/unity-builder's
/// custom buildMethod, so it honors the -customBuildPath / -customBuildTarget
/// arguments that action passes; exits non-zero on any failure.
/// </summary>
public static class CIBuild
{
    /// <summary>
    /// Entry point for game-ci (buildMethod: CIBuild.Build) or a bare
    /// -executeMethod run. Content generation failures abort before the
    /// player build starts — a build without generated content boots broken.
    /// </summary>
    public static void Build()
    {
        if (!ContentGenerationMenu.Run())
        {
            EditorApplication.Exit(1);
            return;
        }

        var target = ParseTarget(GetArg("-customBuildTarget") ?? GetArg("-buildTarget"));
        var path = GetArg("-customBuildPath") ?? DefaultPathFor(target);
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        var report = BuildPipeline.BuildPlayer(scenes, path, target, BuildOptions.None);
        Debug.Log($"[CIBuild] {report.summary.result}: {report.summary.totalErrors} error(s), output '{path}'");
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    // Reads one command-line argument's value; null when absent
    private static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    // Maps the argument value onto a BuildTarget, defaulting to Windows
    private static BuildTarget ParseTarget(string value)
    {
        return value != null && Enum.TryParse(value, true, out BuildTarget parsed)
            ? parsed
            : BuildTarget.StandaloneWindows64;
    }

    // Output path when the caller did not provide one
    private static string DefaultPathFor(BuildTarget target)
    {
        var extension = target == BuildTarget.StandaloneWindows64 ? ".exe" : "";
        return $"build/{target}/TacticsRPG{extension}";
    }
}
