// Build pipeline entry points. Called via Unity's -executeMethod CLI flag.
// Each public static void method corresponds to a build mode from docs/build-and-release.md §2.
//
// Usage (from Tools/build-android.sh):
//   -executeMethod Game.Editor.BuildPipeline.BuildDev
//   -executeMethod Game.Editor.BuildPipeline.BuildProfile
//   -executeMethod Game.Editor.BuildPipeline.BuildRelease
namespace Game.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// Static build pipeline entry points. Called via Unity's -executeMethod CLI flag.
    /// Each method configures the required player settings, runs the build, and either
    /// logs success (with artifact path) or exits Unity with code 1 on failure.
    /// </summary>
    public static class BuildPipeline
    {
        private const string BuildRoot   = "Build";
        private const string ProductName = "artificial-pi";

        // ------------------------------------------------------------------ public entry points

        /// <summary>
        /// Builds a development APK with development symbols enabled.
        /// IL2CPP, ARM64, no stripping, project's debug keystore.
        /// Invoked via: -executeMethod Game.Editor.BuildPipeline.BuildDev
        /// </summary>
        public static void BuildDev()
        {
            ApplyCommonAndroidSettings();
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            EditorUserBuildSettings.buildAppBundle = false;

            BuildPlayerOptions opts = CreateBaseOptions(
                Path.Combine(BuildRoot, "dev", $"{ProductName}-dev.apk"));
            opts.options = BuildOptions.Development;

            RunBuild(opts, "dev");
        }

        /// <summary>
        /// Builds a profiling APK matching release runtime with deep profiling symbols.
        /// IL2CPP, ARM64, no stripping, project's debug keystore.
        /// Invoked via: -executeMethod Game.Editor.BuildPipeline.BuildProfile
        /// </summary>
        public static void BuildProfile()
        {
            ApplyCommonAndroidSettings();
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Minimal);
            EditorUserBuildSettings.buildAppBundle = false;

            BuildPlayerOptions opts = CreateBaseOptions(
                Path.Combine(BuildRoot, "profile", $"{ProductName}-profile.apk"));
            opts.options = BuildOptions.Development | BuildOptions.EnableDeepProfilingSupport;

            RunBuild(opts, "profile");
        }

        /// <summary>
        /// Builds a release AAB signed with the upload keystore.
        /// Reads ANDROID_KEYSTORE_PATH, ANDROID_KEYSTORE_PASS, ANDROID_KEY_ALIAS,
        /// ANDROID_KEY_PASS from environment; throws if any are missing.
        /// IL2CPP, ARM64, strips engine code + managed (Low), auto-increments versionCode.
        /// Invoked via: -executeMethod Game.Editor.BuildPipeline.BuildRelease
        /// </summary>
        public static void BuildRelease()
        {
            // Validate and read signing settings from env before touching PlayerSettings.
            string keystorePath = GetRequiredEnv("ANDROID_KEYSTORE_PATH");
            string keystorePass = GetRequiredEnv("ANDROID_KEYSTORE_PASS");
            string keyAlias     = GetRequiredEnv("ANDROID_KEY_ALIAS");
            string keyAliasPass = GetRequiredEnv("ANDROID_KEY_PASS");

            if (!File.Exists(keystorePath))
                throw new FileNotFoundException(
                    $"Keystore not found at '{keystorePath}'. Set ANDROID_KEYSTORE_PATH correctly.",
                    keystorePath);

            // Cache original signing settings for restoration after the build.
            string origKeystoreName  = PlayerSettings.Android.keystoreName;
            string origKeystorePass  = PlayerSettings.Android.keystorePass;
            string origKeyAliasName  = PlayerSettings.Android.keyaliasName;
            string origKeyAliasPass  = PlayerSettings.Android.keyaliasPass;

            try
            {
                // Apply signing.
                PlayerSettings.Android.keystoreName = keystorePath;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyAlias;
                PlayerSettings.Android.keyaliasPass = keyAliasPass;
                Debug.Log($"[BuildPipeline] Signing configured: keystore={keystorePath}, alias={keyAlias}");

                // Auto-increment version code.
                int nextCode = PlayerSettings.Android.bundleVersionCode + 1;
                PlayerSettings.Android.bundleVersionCode = nextCode;
                Debug.Log($"[BuildPipeline] AndroidBundleVersionCode bumped to {nextCode}");

                ApplyCommonAndroidSettings();
                PlayerSettings.stripEngineCode = true;
                PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
                EditorUserBuildSettings.buildAppBundle = true;

                BuildPlayerOptions opts = CreateBaseOptions(
                    Path.Combine(BuildRoot, "release", $"{ProductName}-release.aab"));
                opts.options = BuildOptions.None;

                RunBuild(opts, "release");  // throws BuildFailedException on failure
            }
            finally
            {
                // Restore original signing values so ProjectSettings.asset stays clean
                // (only the version code bump remains as a staged change).
                // Note: this finally block runs on success (normal return) but NOT when
                // Unity exits via EditorApplication.Exit or Environment.Exit. Since
                // RunBuild throws instead of calling Exit, this does run on success.
                PlayerSettings.Android.keystoreName = origKeystoreName;
                PlayerSettings.Android.keystorePass = origKeystorePass;
                PlayerSettings.Android.keyaliasName = origKeyAliasName;
                PlayerSettings.Android.keyaliasPass = origKeyAliasPass;
            }
        }

        // ------------------------------------------------------------------ helpers

        private static void ApplyCommonAndroidSettings()
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        }

        private static BuildPlayerOptions CreateBaseOptions(string locationPath)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException(
                    "[BuildPipeline] No enabled scenes in Build Settings.");

            string dir = Path.GetDirectoryName(locationPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            return new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = locationPath,
                target           = BuildTarget.Android,
                targetGroup      = BuildTargetGroup.Android,
                options          = BuildOptions.None,
            };
        }

        private static void RunBuild(BuildPlayerOptions opts, string mode)
        {
            Debug.Log($"[BuildPipeline] Starting '{mode}' build → {opts.locationPathName}");

            BuildReport  report  = UnityEditor.BuildPipeline.BuildPlayer(opts);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(
                    $"[BuildPipeline] Build SUCCEEDED: {summary.totalSize} bytes → {opts.locationPathName}");
            }
            else
            {
                string message =
                    $"[BuildPipeline] Build FAILED ({summary.result}): "
                    + $"{summary.totalErrors} error(s). See build log for details.";
                Debug.LogError(message);
                throw new BuildFailedException(message);
            }
        }

        private static string GetRequiredEnv(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value))
                throw new InvalidOperationException(
                    $"Required environment variable '{name}' is not set. "
                    + "Configure it in ~/.config/artificial-pi/build.env.");
            return value;
        }
    }
}
