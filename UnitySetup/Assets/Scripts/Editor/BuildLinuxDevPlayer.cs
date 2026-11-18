namespace Game.Editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// Builds a Linux development player (Mono) of the game, for measurements a real player gives
    /// and the Editor does not, such as the allocation scan (<c>-allocationScan &lt;file&gt;</c>,
    /// docs/systems/logging.md). Needs the Linux Mono player, which ships with the Linux Editor.
    /// Batch mode: <c>-executeMethod Game.Editor.BuildLinuxDevPlayer.BuildFromCommandLine -playerOut path/UnitySetup.x86_64</c>.
    /// Building for desktop rewrites a few Android-oriented settings for that target (URP shader
    /// prefiltering in the PC asset and the global settings, Standalone batching, a Connect flag):
    /// restore them with <c>git checkout</c> before committing.
    /// </summary>
    public static class BuildLinuxDevPlayer
    {
        /// <summary>Entry point for -executeMethod; exits with 0 when the player is built.</summary>
        public static void BuildFromCommandLine()
        {
            string output = CommandLine.Value("-playerOut");
            if (string.IsNullOrEmpty(output))
            {
                Debug.LogError("BuildLinuxDevPlayer: -playerOut <path/UnitySetup.x86_64> is required.");
                EditorApplication.Exit(2);
                return;
            }
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.Development,
            };
            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            Debug.Log($"BuildLinuxDevPlayer: {report.summary.result} -> {output}");
            EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
