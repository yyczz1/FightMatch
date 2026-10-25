using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FightMatch.Host.Editor
{
    // Normal scene configuration only. Resource build and independent approval happen before this menu is used.
    public static class FightMatchChapterResourceSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/FightMatchDemo.unity";
        private const string BootRoot = "Assets/FightMatch/ResourceBuild/";
        private static readonly string[] Platforms = { "macos", "windows", "android" };
        private static readonly string[] ChapterFields = { "chapterMacResourceBoot", "chapterWindowsResourceBoot", "chapterAndroidResourceBoot" };
        private static readonly string[] DemoFields = { "macResourceBoot", "windowsResourceBoot", "androidResourceBoot" };
        [Serializable] private sealed class BootHeader { public Descriptor descriptor; }
        [Serializable] private sealed class Descriptor { public string releaseSetId; public Resources resources; }
        [Serializable] private sealed class Resources { public string packageName, yooManifestPackageVersion, platform; }
        private sealed class Values
        {
            internal UnityEngine.Object Boot;
            internal string Pin, Set;
        }

        [MenuItem("FightMatch/Setup/Bind ChapterOne R3 Resource Boots")]
        public static void BindR3()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                throw new InvalidOperationException("Configure the scene only while the Editor is idle and outside Play mode.");
            for (var i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save or discard existing scene edits before configuring chapter resources.");

            // Validate all three fixed inputs before opening or changing the scene.
            var expected = Platforms.Select(ReadBoot).ToArray();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var host = FindHost(scene);
            var serialized = new SerializedObject(host);
            serialized.Update();
            var demoBefore = DemoFields.Select(field => Read(serialized, field)).ToArray();
            foreach (var field in ChapterFields) Read(serialized, field);
            Undo.RecordObject(host, "Bind ChapterOne R3 resource boots");
            for (var i = 0; i < ChapterFields.Length; i++)
            {
                var field = serialized.FindProperty(ChapterFields[i]);
                field.FindPropertyRelative("boot").objectReferenceValue = expected[i].Boot;
                field.FindPropertyRelative("pinnedSha256").stringValue = expected[i].Pin;
                field.FindPropertyRelative("resourceSetId").stringValue = expected[i].Set;
            }
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("The chapter resource scene could not be saved.");

            // Reopen the saved scene through Unity's serializer, then check both the new fields and old Demo fields.
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            serialized = new SerializedObject(FindHost(scene));
            serialized.Update();
            for (var i = 0; i < ChapterFields.Length; i++)
            {
                RequireSame(expected[i], Read(serialized, ChapterFields[i]));
                RequireSame(demoBefore[i], Read(serialized, DemoFields[i]));
            }
            Debug.Log("Saved ChapterOne R3 resource references in " + ScenePath + "; Play was not started.");
        }

        private static Values ReadBoot(string platform)
        {
            var path = BootRoot + "fm-resource-boot-chapter-" + platform + "-r3.json";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null || asset.bytes.Length == 0 || asset.bytes.Length > 262144)
                throw new InvalidDataException("Missing or invalid imported chapter boot: " + path);
            var descriptor = JsonUtility.FromJson<BootHeader>(asset.text)?.descriptor;
            var set = "fm.ch1.trial.r3." + platform;
            if (descriptor?.resources == null || descriptor.releaseSetId != set ||
                descriptor.resources.packageName != "FightMatchChapterOneR3" ||
                descriptor.resources.yooManifestPackageVersion != "chapter-one-trial-r3" ||
                descriptor.resources.platform != (platform == "macos" ? "macOS" : platform))
                throw new InvalidDataException("Unexpected chapter boot identity: " + path);
            string hash;
            using (var sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(asset.bytes)).Replace("-", "").ToLowerInvariant();
            return new Values { Boot = asset, Pin = hash, Set = set };
        }
        private static FightMatchPlayerHost FindHost(Scene scene)
        {
            var hosts = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<FightMatchPlayerHost>(true)).ToArray();
            if (scene.path != ScenePath || hosts.Length != 1)
                throw new InvalidOperationException("The fixed FightMatch scene must contain exactly one player host.");
            return hosts[0];
        }
        private static Values Read(SerializedObject serialized, string name)
        {
            var field = serialized.FindProperty(name);
            if (field?.FindPropertyRelative("boot") == null || field.FindPropertyRelative("pinnedSha256") == null ||
                field.FindPropertyRelative("resourceSetId") == null)
                throw new InvalidOperationException("Missing serialized resource field: " + name);
            return new Values { Boot = field.FindPropertyRelative("boot").objectReferenceValue,
                Pin = field.FindPropertyRelative("pinnedSha256").stringValue,
                Set = field.FindPropertyRelative("resourceSetId").stringValue };
        }
        private static void RequireSame(Values expected, Values actual)
        {
            if (expected.Boot != actual.Boot || expected.Pin != actual.Pin || expected.Set != actual.Set)
                throw new InvalidDataException("Saved resource references differ from the requested scene change.");
        }
    }
}
