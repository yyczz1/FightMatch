using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using FightMatch.Presentation;
using UnityEngine.EventSystems;


namespace FightMatch.Host.Tests
{
    public sealed class FightMatchHostResourceTests
    {
        private const string FontRoot = "Assets/UI/FightMatch/Fonts/";
        [Test]
        public void H05_SceneReferencesOneHostPanelThemeDynamicFontAndViewableLicense()
        {
            var previous = SceneManager.GetActiveScene();
            var defaultScene = SceneManager.sceneCount == 1 && string.IsNullOrEmpty(previous.path);
            Assert.IsFalse(defaultScene && previous.isDirty, "Do not discard a dirty untitled scene.");
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/FightMatchDemo.unity", defaultScene ? OpenSceneMode.Single : OpenSceneMode.Additive);
            try
            {
                var hosts = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<FightMatchPlayerHost>(true)).ToArray();
                Assert.AreEqual(1, hosts.Length);
                var host = hosts[0];
                Assert.IsNull(host.GetComponent<UnityEngine.UIElements.UIDocument>());
                Assert.IsNotNull(host.RuntimeRoot);
                Assert.AreEqual("Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab",
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(host.RuntimeRoot));
                var canvases = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Canvas>(true)).ToArray();
                var events = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<EventSystem>(true)).ToArray();
                Assert.AreEqual(1, canvases.Length); Assert.AreEqual(1, events.Length);
                Assert.AreEqual(1, events[0].GetComponents<BaseInputModule>().Length);
                Assert.IsNotNull(events[0].GetComponent<FightMatchStandaloneInputModule>());
                Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvases[0].renderMode);
                var scaler = canvases[0].GetComponent<UnityEngine.UI.CanvasScaler>();
                Assert.AreEqual(UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
                Assert.AreEqual(new Vector2(540, 960), scaler.referenceResolution);
                Assert.AreEqual(.5f, scaler.matchWidthOrHeight);
                Assert.IsNotNull(canvases[0].GetComponent<UnityEngine.UI.GraphicRaycaster>());
                Assert.AreEqual(FontRoot + "NotoSansCJKsc-Regular-TMP.asset", AssetDatabase.GetAssetPath(host.FontAsset));
                Assert.AreEqual(FontRoot + "OFL.txt", AssetDatabase.GetAssetPath(host.FontLicense));
                Assert.That(host.FontLicense.text, Does.Contain("SIL OPEN FONT LICENSE"));
                Assert.AreEqual(AtlasPopulationMode.Dynamic, host.FontAsset.atlasPopulationMode);
                Assert.IsTrue(host.FontAsset.isMultiAtlasTexturesEnabled);
                Assert.AreEqual(FontRoot + "NotoSansCJKsc-Regular.otf", AssetDatabase.GetAssetPath(host.FontAsset.sourceFontFile));
                Assert.IsTrue(AssetDatabase.IsSubAsset(host.FontAsset.material));
                Assert.IsTrue(host.FontAsset.atlasTextures.All(AssetDatabase.IsSubAsset));
                Assert.AreEqual("TextMeshPro/Distance Field", host.FontAsset.material.shader.name);
                Assert.IsNotNull(Shader.Find("TextMeshPro/Mobile/Distance Field"));
                Assert.IsNotNull(TMP_Settings.instance);
                Assert.AreSame(host.FontAsset, TMP_Settings.defaultFontAsset);
                var texts = host.RuntimeRoot.GetComponentsInChildren<TextMeshProUGUI>(true); var linkedInputs = 0;
                foreach (var text in texts)
                {
                    Assert.AreSame(host.FontAsset, text.font);
                    var input = text.GetComponentInParent<TMP_InputField>(true);
                    if (input != null && ReferenceEquals(input.textComponent, text))
                    {
                        linkedInputs++; Assert.AreEqual(LocalizedTmpText.Placeholder, input.text);
                        Assert.That(text.text, Is.EqualTo(LocalizedTmpText.Placeholder).Or.EqualTo(LocalizedTmpText.Placeholder + "\u200B"));
                    }
                    else Assert.AreEqual(LocalizedTmpText.Placeholder, text.text);
                }
                Assert.AreEqual(119, texts.Length); Assert.AreEqual(3, linkedInputs); Assert.AreEqual(116, texts.Length - linkedInputs);
                Assert.IsTrue(FightMatchViewId.Validate(host.RuntimeRoot.transform, out var diagnostic), diagnostic);
                var paths = AssetDatabase.GetDependencies(new[] { "Assets/Scenes/FightMatchDemo.unity",
                    "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab" }, true);
                foreach (var old in new[] { "Assets/UI/FightMatch/FightMatchPanelSettings.asset", "Assets/UI/FightMatch/FightMatchTheme.tss",
                    FontRoot + "NotoSansCJKsc-Regular.asset" }) CollectionAssert.DoesNotContain(paths, old);
            }
            finally
            {
                if (defaultScene) EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                else EditorSceneManager.CloseScene(scene, true);
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            }
        }

        [TestCase("NotoSansCJKsc-Regular.otf", 16437364, "2c76254f6fc379fddfce0a7e84fb5385bb135d3e399294f6eeb6680d0365b74b")]
        [TestCase("OFL.txt", 4301, "6a73f9541c2de74158c0e7cf6b0a58ef774f5a780bf191f2d7ec9cc53efe2bf2")]
        public void H05_FontAndLicenseHaveThePinnedOfficialBytes(string file, int length, string expected)
        {
            var bytes = File.ReadAllBytes(Path.Combine(HostRig.Project, FontRoot + file));
            Assert.AreEqual(length, bytes.Length);
            using (var sha = SHA256.Create())
                Assert.AreEqual(expected, string.Concat(sha.ComputeHash(bytes).Select(x => x.ToString("x2"))));
        }

        [TestCase(4, 9)]
        [TestCase(9, 4)]
        public void H02_ReceiverRejectsGrowthBeforeCopyForBothFileAndAggregateLimits(int file, int aggregate)
        {
            using (var receiver = new FightMatchStreamingAssetsLoader.BoundedReceiver(file, aggregate))
            {
                Assert.IsTrue(receiver.Append(new byte[] { 1, 2, 3 }, 3));
                Assert.IsFalse(receiver.Append(new byte[] { 4, 5 }, 2));
                Assert.AreEqual(3, receiver.Length);
                Assert.IsTrue(receiver.Rejected);
                Assert.Throws<InvalidOperationException>(() => receiver.TakeBytes());
                Assert.IsFalse(receiver.Append(new byte[0], 0));
            }
        }

        [Test]
        public void H02_ReceiverAcceptsTheExactBoundaryAndRejectsInvalidCounts()
        {
            using (var receiver = new FightMatchStreamingAssetsLoader.BoundedReceiver(4, 4))
            {
                var bytes = new byte[] { 1, 2, 3, 4 };
                Assert.IsTrue(receiver.Append(bytes, 4));
                CollectionAssert.AreEqual(bytes, receiver.TakeBytes());
                Assert.IsFalse(receiver.Append(bytes, 5));
                Assert.AreEqual(4, receiver.Length);
            }
        }

        [Test]
        public void A01_AndroidAdapterCannotBeMistakenForExecutedMacStorage()
        {
            Assert.Throws<PlatformNotSupportedException>(() => new AndroidLocalSaveStorage(HostRig.Project,
                "not-a-real-player", FightMatch.Core.SavePurpose.PlayerSave));
            Assert.Throws<PlatformNotSupportedException>(() => new AndroidContentPublicationStorage(HostRig.Project));
        }
    }
}
