using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[assembly: TestPlayerBuildModifier(typeof(FightMatch.Host.Editor.FightMatchAndroidBuild))]

namespace FightMatch.Host.Editor
{
    [InitializeOnLoad]
    public sealed class FightMatchAndroidBuild : ITestPlayerBuildModifier, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string Stage = "DEMO-029-MAC-R1";
        private const string Project = "/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch";
        private const string ScenePath = "Assets/Scenes/FightMatchDemo.unity";
        private const string Ui = "Assets/UI/FightMatch/";
        private const string SettingsPath = "ProjectSettings/ProjectSettings.asset";
        private const string BuildSettingsPath = "ProjectSettings/EditorBuildSettings.asset";
        private const string Signing = "/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/AndroidBuildCache/FightMatch/signing/";
        private static readonly string[] Args = Environment.GetCommandLineArgs();
        private static Settings saved;
        private static SceneSetup[] originalScenes;
        private static string apk, runDirectory, temporaryScene;
        private static bool qa, finishing, finished, buildObserved, loggedError;
        private static BuildReport actualReport;
        public int callbackOrder => int.MaxValue;

        static FightMatchAndroidBuild()
        {
            if (!Args.Contains("-fm029QaBuildOnly") || Arg("-fm029Stage") != Stage) return;
            qa = true;
            GuardOutput(Arg("-buildPlayerPath"), "fightmatch-qa.apk");
            Require(Arg("-assemblyNames") == "FightMatch.Android.Tests" && Arg("-testPlatform") == "Android", "QA filter");
            CheckProduction();
            saved = Settings.Capture();
            originalScenes = EditorSceneManager.GetSceneManagerSetup();
            WriteSettings();
            UnityEngine.Application.logMessageReceived += OnLog;
            EditorApplication.update += ObserveQaCompletion;
            EditorApplication.wantsToQuit += RestoreBeforeQuit;
        }
        private static string Arg(string key)
        {
            var positions = Args.Select((x, i) => new { x, i }).Where(x => x.x == key).ToArray();
            if (positions.Length == 0) return null;
            Require(positions.Length == 1 && positions[0].i + 1 < Args.Length, "Duplicate or missing " + key);
            return Args[positions[0].i + 1];
        }
        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("FM029: " + message);
        }
        private static string Sha(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create()) return string.Concat(hash.ComputeHash(stream).Select(x => x.ToString("x2")));
        }
        private static void GuardOutput(string output, string leaf)
        {
            Require(Arg("-fm029Stage") == Stage && Path.GetDirectoryName(UnityEngine.Application.dataPath) == Project, "Stage/project");
            Require(Sha(Project + "/docs/system-design/2026-09-17/demo-029-mac-r1-plan.json") ==
                "cfe1ef8ae52ba4fa9daea475d3f4571d88cdbfe47b87ce89ba7e5a8eb59bd6f1", "Plan identity");
            var prefix = leaf.EndsWith(".apk", StringComparison.Ordinal) ? "/Builds/FMDemo029/mac-r1/" : "/TestArtifacts/FMDemo029/mac-r1/runs/";
            var match = Regex.Match(output ?? "", "^" + Regex.Escape(Project + prefix) + "(00[1-9]|01[0-9]|020)/" + Regex.Escape(leaf) + "$");
            Require(match.Success && !File.Exists(output) && !Directory.Exists(output), "Exact create-new output");
            runDirectory = Project + "/TestArtifacts/FMDemo029/mac-r1/runs/" + match.Groups[1].Value;
            Require(File.Exists(runDirectory + "/run.json"), "Missing authorized run");
            var binding = JsonUtility.FromJson<RunBinding>(File.ReadAllText(runDirectory + "/run.json"));
            var expectedMode = leaf == "fightmatch-qa.apk" ? "QaBuild" : leaf == "fightmatch-demo.apk" ? "DemoBuild" : "PrepareAssets";
            Require(binding.stageId == Stage && binding.authorTurnId == "01a0eaf3-31c0-7d83-9dfc-31c24012946f" &&
                binding.mode == expectedMode && binding.argv.Contains(output), "Current author/mode/output binding");
            Require(binding.script.sha256 == Sha(Project + "/Tools/Invoke-FM029Validation.ps1") &&
                binding.rootIdentity.sha256 == Sha(Project + "/TestArtifacts/FMDemo029/mac-r1/root-identity.json"), "Run script/root identity");
            if (leaf.EndsWith(".apk", StringComparison.Ordinal)) apk = output;
        }
        public static void PrepareResources()
        {
            var output = Arg("-fm029Output");
            GuardOutput(output, "resource-preparation.json");
            Require(EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX, "Mac resource preparation target");
            var fontPath = Ui + "Fonts/NotoSansCJKsc-Regular.asset";
            var panelPath = Ui + "FightMatchPanelSettings.asset";
            Require(Sha(ScenePath) == "86872b3758377327dd3be4128a4343e35ceaafb60e7cd30178100417ea3d3808" &&
                Sha(ScenePath + ".meta") == "2f2cfa8c3f7d833ff3694fbfd20c45643e40c465e7209d6040fba48d76f37e7e", "Exact 009 scene");
            Require(Sha(fontPath) == "0db8ff6c3c3c21b582058f5a2000f399448ee0b0743970f12cf9927a50f01bea" &&
                Sha(fontPath + ".meta") == "d0119c373a656bbeb85b133339cf33c41c6ee8ca7aa0e189c9ab6e0aca7d541d", "Exact completed font");
            Require(Sha(panelPath) == "e37d3df12fb06088e9c1fa7ba88060dbc224c05c16e4e783060baff91cdf9316" &&
                Sha(panelPath + ".meta") == "01d556098f3cc5d0b14eb96f21e6f59a47466e2691c5bcdd59052cae46bbaed4", "Exact 007 panel");
            Require(!Directory.Exists("Assets/UI Toolkit") && !File.Exists("Assets/UI Toolkit.meta"), "Unapproved default theme remains");
            var previous = SceneManager.GetActiveScene();
            var defaultScene = SceneManager.sceneCount == 1 && string.IsNullOrEmpty(previous.path);
            Debug.Log("FM029_RESOURCE_SCENE_INITIAL " + string.Join(", ", SceneState()) +
                "; default=" + defaultScene + "; dirty=" + previous.isDirty + "; roots=" + previous.rootCount);
            Require(UnityEngine.Application.isBatchMode && (!defaultScene || !previous.isDirty), "Do not replace a dirty or user-owned scene");
            var asset = AssetDatabase.LoadAssetAtPath<FontAsset>(fontPath);
            Require(asset != null && asset.material != null && AssetDatabase.IsSubAsset(asset.material) &&
                asset.atlasTextures.Length == 1 && asset.atlasTextures.All(x => x != null && AssetDatabase.IsSubAsset(x)), "Completed embedded font");
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
            Require(panel != null && AssetDatabase.GetAssetPath(panel.themeStyleSheet) == Ui + "FightMatchTheme.tss", "Reuse approved panel theme");
            var scene = default(Scene);
            try
            {
                scene = EditorSceneManager.OpenScene(ScenePath, defaultScene ? OpenSceneMode.Single : OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                var hosts = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<FightMatchPlayerHost>(true)).ToArray();
                Require(hosts.Length == 1, "One existing host required");
                var document = hosts[0].GetComponent<UIDocument>();
                Require(document != null, "Existing UIDocument required");
                document.panelSettings = panel;
                var fields = new SerializedObject(document);
                fields.FindProperty("m_PanelSettings").objectReferenceValue = panel;
                fields.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(document);
                EditorSceneManager.MarkSceneDirty(scene);
                Require(document.panelSettings == panel, "Panel binding before save");
                Require(EditorSceneManager.SaveScene(scene, ScenePath), "Scene save");
                if (!defaultScene) EditorSceneManager.CloseScene(scene, true);
                scene = EditorSceneManager.OpenScene(ScenePath, defaultScene ? OpenSceneMode.Single : OpenSceneMode.Additive);
                hosts = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<FightMatchPlayerHost>(true)).ToArray();
                Require(hosts.Length == 1 && hosts[0].GetComponent<UIDocument>().panelSettings == panel &&
                    hosts[0].FontAsset == asset && AssetDatabase.GetAssetPath(hosts[0].FontLicense) == Ui + "Fonts/OFL.txt", "Saved scene resource bindings");
            }
            finally
            {
                if (defaultScene) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                else if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
            File.WriteAllText(output, JsonUtility.ToJson(new ResourceResult { stage = Stage, scene = ScenePath,
                font = fontPath, fontSourceSha256 = Sha(Ui + "Fonts/NotoSansCJKsc-Regular.otf"), fontBytes = new FileInfo(fontPath).Length,
                sceneSha256 = Sha(ScenePath), sceneBindingsReopened = true,
                androidIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) }, true));
        }
        private static void CheckProduction()
        {
            Require(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, "Android target");
            Require(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == "com.yyczz1.fightmatch", "Production package");
            Require(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP &&
                PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "IL2CPP/ARM64");
            Require(PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Minimal, "Minimal stripping");
            Require((int)PlayerSettings.Android.minSdkVersion == 22 && (int)PlayerSettings.Android.targetSdkVersion == 32 &&
                PlayerSettings.Android.bundleVersionCode == 1 && PlayerSettings.bundleVersion == "0.1", "Version/SDK");
            Require(!EditorUserBuildSettings.buildAppBundle && !PlayerSettings.Android.useAPKExpansionFiles, "Single APK required");
            Require(EditorUserBuildSettings.androidCreateSymbols == AndroidCreateSymbols.Disabled, "No separate symbols package");
        }
        [System.Runtime.InteropServices.DllImport("libSystem.B.dylib", EntryPoint = "umask")]
        private static extern ushort ProcessUmask(ushort mask);
        private static string UnixMetadata(string program, params string[] arguments)
        {
            var escaped = arguments.Select(x => "\"" + x.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
            var start = new System.Diagnostics.ProcessStartInfo(program, string.Join(" ", escaped)) {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            using (var process = System.Diagnostics.Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Require(process.ExitCode == 0 || (program == "/usr/bin/find" && process.ExitCode == 1 && error.Length == 0),
                    "Private cache metadata check failed");
                return output.TrimEnd();
            }
        }
        private static void CheckPrivateCaches()
        {
            Require(ProcessUmask(63) == 63, "Unity must inherit umask 077 before signing");
            var gradle = Environment.GetEnvironmentVariable("GRADLE_USER_HOME");
            Require(gradle == "/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/AndroidBuildCache/FightMatch/gradle-user-home", "Private Gradle root");
            var paths = new[] { Project + "/Library/Bee", gradle, gradle + "/gradle.properties" };
            var metadata = paths.Select(path => {
                for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
                    Require((File.GetAttributes(current) & FileAttributes.ReparsePoint) == 0, "Private cache path contains a link");
                var expected = path.EndsWith(".properties", StringComparison.Ordinal) ? "501:600:Regular File" : "501:700:Directory";
                var observed = UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path);
                Require(observed == expected, "Private cache owner, mode or type differs: " + path);
                Require(!Regex.IsMatch(UnixMetadata("/bin/ls", "-lde", path), @"(?m)^\s*\d+:"), "Private cache has an ACL");
                return path + "|" + observed + "|no ACL|umask 077";
            }).ToArray();
            Require(File.ReadAllText(gradle + "/gradle.properties") == "org.gradle.daemon=false\n", "Dedicated no-daemon configuration differs");
            saved.privateCacheMetadata = saved.privateCacheMetadata.Concat(metadata).ToArray();
            Require(UnixMetadata("/usr/bin/find", paths[0], gradle, "-type", "l", "-print", "-quit").Length == 0,
                "Private generated cache contains a link");
            foreach (var path in UnixMetadata("/usr/bin/find", paths[0], gradle, "-type", "f", "-exec", "/usr/bin/grep",
                "-a", "-l", "-F", "-f", Signing + "fightmatch-local-release.pass", "{}", "+").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var before = UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path);
                Require(before.StartsWith("501:", StringComparison.Ordinal) && before.EndsWith(":Regular File", StringComparison.Ordinal), "Private signing cache ownership/type");
                Require(!Regex.IsMatch(UnixMetadata("/bin/ls", "-lde", path), @"(?m)^\s*\d+:"), "Private signing file has an ACL");
                if (before != "501:600:Regular File") UnixMetadata("/bin/chmod", "600", path);
                Require(UnixMetadata("/usr/bin/stat", "-f", "%u:%Lp:%HT", path) == "501:600:Regular File", "Private signing file mode");
                saved.privateCacheMetadata = saved.privateCacheMetadata.Concat(new[] { path + "|501:600:Regular File|no ACL" }).ToArray();
            }
        }
        private static void Sign()
        {
            CheckPrivateCaches();
            Require(File.Exists(Signing + "fightmatch-local-release.jks") && File.Exists(Signing + "fightmatch-local-release.pass"), "Preserved signing key");
            var password = File.ReadAllText(Signing + "fightmatch-local-release.pass").TrimEnd('\r', '\n');
            Require(password.Length >= 24, "Signing password format");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Signing + "fightmatch-local-release.jks";
            PlayerSettings.Android.keyaliasName = "fightmatch";
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;
        }
        public static void BuildDemo()
        {
            GuardOutput(Arg("-fm029Output"), "fightmatch-demo.apk");
            CheckProduction();
            saved = Settings.Capture();
            WriteSettings();
            try
            {
                Sign();
                Directory.CreateDirectory(Path.GetDirectoryName(apk));
                actualReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath },
                    target = BuildTarget.Android, targetGroup = BuildTargetGroup.Android,
                    locationPathName = apk, options = BuildOptions.Development });
                WriteReport(actualReport);
                Require(actualReport.summary.result == BuildResult.Succeeded && File.Exists(apk), "Demo build failed");
            }
            finally { Restore(); }
        }
        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions options)
        {
            if (!qa) return options;
            Require(saved != null && options.target == BuildTarget.Android && Arg("-assemblyNames") == "FightMatch.Android.Tests", "QA modifier binding");
            Require(options.scenes.Length >= 1 && Regex.IsMatch(options.scenes[0], "^Assets/InitTestScene[0-9]+\\.unity$"), "One UTF bootstrap scene");
            Require(Directory.GetFiles("Assets", "InitTestScene*.unity").Length == 1, "Unexpected temporary scene");
            temporaryScene = options.scenes[0];
            saved.temporaryScene = temporaryScene;
            saved.temporarySceneSha256 = Sha(temporaryScene);
            saved.temporaryMetaSha256 = Sha(temporaryScene + ".meta");
            saved.temporarySceneBytes = new FileInfo(temporaryScene).Length;
            saved.temporaryMetaBytes = new FileInfo(temporaryScene + ".meta").Length;
            options.locationPathName = apk;
            options.scenes = new[] { temporaryScene };
            options.options &= ~(BuildOptions.AutoRunPlayer | BuildOptions.ConnectToHost | BuildOptions.WaitForPlayerConnection |
                BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
            Require((options.options & BuildOptions.IncludeTestAssemblies) != 0, "QA test assemblies required");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yyczz1.fightmatch.qa");
            Sign();
            WriteSettings();
            buildObserved = true;
            Directory.CreateDirectory(Path.GetDirectoryName(apk));
            return options;
        }
        public void OnPostprocessBuild(BuildReport report)
        {
            if (saved == null || report.summary.outputPath != apk) return;
            actualReport = report;
            WriteReport(report);
        }
        public void OnPreprocessBuild(BuildReport report)
        {
            if (saved != null && report.summary.outputPath == apk) actualReport = report;
        }
        private static void OnLog(string text, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) loggedError = true;
        }
        private static void ObserveQaCompletion()
        {
            if (finished || finishing || BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!buildObserved && !loggedError) return;
            finishing = true;
            EditorApplication.delayCall += FinishQa;
        }
        private static void FinishQa()
        {
            var success = false;
            try
            {
                Require(actualReport != null && actualReport.summary.outputPath == apk, "No current QA BuildReport");
                WriteReport(actualReport);
                saved.naturalCleanup = temporaryScene != null && !File.Exists(temporaryScene) && !File.Exists(temporaryScene + ".meta") &&
                    saved.ContextMatches() && saved.sceneSetup.SequenceEqual(SceneState());
                Require(saved.naturalCleanup, "UTF natural cleanup/Dispose incomplete");
                Require(actualReport.summary.result == BuildResult.Succeeded && File.Exists(apk) && !loggedError, "QA build failed");
                success = true;
            }
            catch (Exception error) { saved.failure = error.Message; Debug.LogError(error.Message); }
            finally
            {
                try { Restore(); }
                catch (Exception error) { success = false; saved.failure = error.Message; WriteSettings(); Debug.LogError(error.Message); }
                finished = true;
                EditorApplication.update -= ObserveQaCompletion;
                EditorApplication.Exit(success && saved.restored ? 0 : 1);
            }
        }
        private static bool RestoreBeforeQuit()
        {
            if (saved != null && !saved.restored) Restore();
            return true;
        }
        private static string[] SceneState() => EditorSceneManager.GetSceneManagerSetup()
            .Select(x => x.path + "|" + x.isLoaded + "|" + x.isActive).ToArray();
        private static void WriteSettings() => File.WriteAllText(runDirectory + "/temporary-settings.json", JsonUtility.ToJson(saved, true));
        private static void Restore()
        {
            if (saved == null || saved.restored) return;
            saved.Apply();
            if (qa && !saved.sceneSetup.SequenceEqual(SceneState())) EditorSceneManager.RestoreSceneManagerSetup(originalScenes);
            if (qa && temporaryScene != null && File.Exists(temporaryScene)) AssetDatabase.DeleteAsset(temporaryScene);
            AssetDatabase.SaveAssets();
            Require(saved.ContextMatches() && saved.PlatformMatches(), "Temporary fields did not restore");
            Require(!qa || saved.sceneSetup.SequenceEqual(SceneState()), "Original scenes did not restore");
            saved.settingsBeforeExactRestoreSha256 = Sha(SettingsPath);
            saved.buildSettingsBeforeExactRestoreSha256 = Sha(BuildSettingsPath);
            // Restore the captured serialization as well as the verified API values; temporary empty keys must not leak.
            File.WriteAllText(SettingsPath, saved.settingsText);
            File.WriteAllText(BuildSettingsPath, saved.buildSettingsText);
            Require(File.ReadAllText(SettingsPath) == saved.settingsText && File.ReadAllText(BuildSettingsPath) == saved.buildSettingsText, "Settings bytes");
            Require(!qa || temporaryScene == null || (!File.Exists(temporaryScene) && !File.Exists(temporaryScene + ".meta")), "Temporary scene remains");
            saved.restored = true;
            WriteSettings();
            CheckPrivateCaches();
            WriteSettings();
        }
        private static void WriteReport(BuildReport report)
        {
            var summary = report.summary;
            File.WriteAllText(runDirectory + "/build-report.json", JsonUtility.ToJson(new BuildResultRecord {
                stage = Stage, result = summary.result.ToString(), output = summary.outputPath, target = summary.platform.ToString(),
                options = summary.options.ToString(), errors = summary.totalErrors, warnings = summary.totalWarnings,
                startedUtc = summary.buildStartedAt.ToUniversalTime().ToString("O"), endedUtc = summary.buildEndedAt.ToUniversalTime().ToString("O"),
                bytes = File.Exists(apk) ? new FileInfo(apk).Length : 0, sha256 = File.Exists(apk) ? Sha(apk) : null,
                package = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
                backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android).ToString(),
                architecture = PlayerSettings.Android.targetArchitectures.ToString(),
                stripping = PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android).ToString(),
                files = report.GetFiles().Select(x => x.role + "|" + x.path + "|" + x.size).ToArray() }, true));
        }
        [Serializable] private sealed class ResourceResult
        {
            public string stage, scene, font, fontSourceSha256, sceneSha256, androidIdentifier;
            public long fontBytes;
            public bool sceneBindingsReopened;
        }
        [Serializable] private sealed class FileIdentity { public string sha256; }
        [Serializable] private sealed class RunBinding
        {
            public string stageId, authorTurnId, mode;
            public string[] argv;
            public FileIdentity script, rootIdentity;
        }
        [Serializable] private sealed class BuildResultRecord
        {
            public string stage, result, output, target, options, startedUtc, endedUtc, sha256, package, backend, architecture, stripping;
            public int errors, warnings;
            public long bytes;
            public string[] files;
        }
#pragma warning disable 618
        [Serializable] private sealed class Settings
        {
            public string identifier, product, aot, keystore, alias, settingsText, buildSettingsText;
            public string legacySocket = "Not accessed: installed UTF uses its Unity 2021.2+ non-legacy Android path.";
            public bool strip, background, resizable, splash, wait, nullChecks, bundle, expansion, customKey, autoGraphics;
            public int backend, architecture, fullScreen, resolution, lightmapping;
            public string[] graphics, sceneSetup, buildScenes;
            public string[] privateCacheMetadata = new string[0];
            public bool[] enabledScenes;
            public string temporaryScene, temporarySceneSha256, temporaryMetaSha256, settingsBeforeExactRestoreSha256, buildSettingsBeforeExactRestoreSha256, failure;
            public long temporarySceneBytes, temporaryMetaBytes;
            public bool naturalCleanup, restored;
            [NonSerialized] private string keyPassword, aliasPassword;
            internal static Settings Capture() => new Settings {
                identifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android), product = PlayerSettings.productName,
                aot = PlayerSettings.aotOptions, strip = PlayerSettings.stripEngineCode, background = PlayerSettings.runInBackground,
                fullScreen = (int)PlayerSettings.fullScreenMode, resolution = (int)PlayerSettings.displayResolutionDialog,
                resizable = PlayerSettings.resizableWindow, splash = PlayerSettings.SplashScreen.show,
                wait = EditorUserBuildSettings.waitForPlayerConnection, nullChecks = EditorUserBuildSettings.explicitNullChecks,
                lightmapping = (int)Lightmapping.giWorkflowMode,
                backend = (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android), architecture = (int)PlayerSettings.Android.targetArchitectures,
                bundle = EditorUserBuildSettings.buildAppBundle, expansion = PlayerSettings.Android.useAPKExpansionFiles,
                customKey = PlayerSettings.Android.useCustomKeystore, keystore = PlayerSettings.Android.keystoreName,
                alias = PlayerSettings.Android.keyaliasName, keyPassword = PlayerSettings.Android.keystorePass, aliasPassword = PlayerSettings.Android.keyaliasPass,
                autoGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android),
                graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(x => x.ToString()).ToArray(),
                sceneSetup = SceneState(), buildScenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
                enabledScenes = EditorBuildSettings.scenes.Select(x => x.enabled).ToArray(),
                settingsText = File.ReadAllText(SettingsPath), buildSettingsText = File.ReadAllText(BuildSettingsPath) };
            internal bool ContextMatches() => product == PlayerSettings.productName && aot == PlayerSettings.aotOptions &&
                background == PlayerSettings.runInBackground && fullScreen == (int)PlayerSettings.fullScreenMode &&
                resolution == (int)PlayerSettings.displayResolutionDialog && resizable == PlayerSettings.resizableWindow &&
                splash == PlayerSettings.SplashScreen.show && nullChecks == EditorUserBuildSettings.explicitNullChecks &&
                lightmapping == (int)Lightmapping.giWorkflowMode && buildScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.path)) &&
                enabledScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.enabled));
            internal bool PlatformMatches() => identifier == PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) &&
                strip == PlayerSettings.stripEngineCode && wait == EditorUserBuildSettings.waitForPlayerConnection &&
                backend == (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) &&
                architecture == (int)PlayerSettings.Android.targetArchitectures && bundle == EditorUserBuildSettings.buildAppBundle &&
                expansion == PlayerSettings.Android.useAPKExpansionFiles && autoGraphics == PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android) &&
                graphics.SequenceEqual(PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(x => x.ToString())) &&
                customKey == PlayerSettings.Android.useCustomKeystore && keystore == PlayerSettings.Android.keystoreName && alias == PlayerSettings.Android.keyaliasName;
            internal void Apply()
            {
                if (identifier != PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)) PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, identifier);
                if (strip != PlayerSettings.stripEngineCode) PlayerSettings.stripEngineCode = strip;
                if (product != PlayerSettings.productName) PlayerSettings.productName = product;
                if (aot != PlayerSettings.aotOptions) PlayerSettings.aotOptions = aot;
                if (background != PlayerSettings.runInBackground) PlayerSettings.runInBackground = background;
                if (fullScreen != (int)PlayerSettings.fullScreenMode) PlayerSettings.fullScreenMode = (FullScreenMode)fullScreen;
                if (resolution != (int)PlayerSettings.displayResolutionDialog) PlayerSettings.displayResolutionDialog = (ResolutionDialogSetting)resolution;
                if (resizable != PlayerSettings.resizableWindow) PlayerSettings.resizableWindow = resizable;
                if (splash != PlayerSettings.SplashScreen.show) PlayerSettings.SplashScreen.show = splash;
                if (wait != EditorUserBuildSettings.waitForPlayerConnection) EditorUserBuildSettings.waitForPlayerConnection = wait;
                if (nullChecks != EditorUserBuildSettings.explicitNullChecks) EditorUserBuildSettings.explicitNullChecks = nullChecks;
                if (lightmapping != (int)Lightmapping.giWorkflowMode) Lightmapping.giWorkflowMode = (Lightmapping.GIWorkflowMode)lightmapping;
                if (backend != (int)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)) PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, (ScriptingImplementation)backend);
                if (architecture != (int)PlayerSettings.Android.targetArchitectures) PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)architecture;
                if (bundle != EditorUserBuildSettings.buildAppBundle) EditorUserBuildSettings.buildAppBundle = bundle;
                if (expansion != PlayerSettings.Android.useAPKExpansionFiles) PlayerSettings.Android.useAPKExpansionFiles = expansion;
                if (autoGraphics != PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android)) PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, autoGraphics);
                if (customKey != PlayerSettings.Android.useCustomKeystore) PlayerSettings.Android.useCustomKeystore = customKey;
                if (keystore != PlayerSettings.Android.keystoreName) PlayerSettings.Android.keystoreName = keystore;
                if (alias != PlayerSettings.Android.keyaliasName) PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keystorePass = keyPassword;
                PlayerSettings.Android.keyaliasPass = aliasPassword;
                if (!buildScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.path)) ||
                    !enabledScenes.SequenceEqual(EditorBuildSettings.scenes.Select(x => x.enabled)))
                    EditorBuildSettings.scenes = buildScenes.Select((path, i) => new EditorBuildSettingsScene(path, enabledScenes[i])).ToArray();
            }
        }
#pragma warning restore 618
    }
}
