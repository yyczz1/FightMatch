using System;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Host;
using FightMatch.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class LocalizedTextBindingTests
    {
        private bool enteredPlayMode;

        [UnityEngine.TestTools.UnityTearDown]
        public System.Collections.IEnumerator ExitControlledPlayModeAfterFailure()
        {
            if (enteredPlayMode && UnityEngine.Application.isPlaying)
            {
                TestContext.Out.WriteLine("FIX20 teardown: exiting controlled PlayMode after test failure.");
                yield return new UnityEngine.TestTools.ExitPlayMode();
            }
            enteredPlayMode = false;
        }

        private sealed class SeveritySource : ILocalizedTextSource
        {
            private readonly UguiTestTextSource source = new UguiTestTextSource();
            private readonly LocalizedTextSeverity severity;
            internal SeveritySource(LocalizedTextSeverity severity) { this.severity = severity; }
            public LocalizedTextResult Resolve(string key, LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> args)
            {
                var result = source.Resolve(key, locale, args);
                return result.IsSuccess ? LocalizedTextResult.Success(result.Text, severity) : result;
            }
        }
        private static LocalizedTmpText Probe(UguiHostRig rig)
        {
            var root = new GameObject("BindingProbe", typeof(RectTransform)); root.SetActive(false);
            root.transform.SetParent(rig.Root.GetComponentInChildren<Canvas>(true).transform, false);
            var text = root.AddComponent<TextMeshProUGUI>(); text.font = rig.Host.FontAsset;
            text.fontSharedMaterial = rig.Host.FontAsset.material; text.text = LocalizedTmpText.Placeholder; text.raycastTarget = false;
            var binding = root.AddComponent<LocalizedTmpText>(); UguiHostRig.SetReference(binding, "target", text);
            return binding;
        }
        private static void Submit(UguiHostRig rig, UnityEngine.UI.Button button)
        {
            Assert.IsNotNull(button); Assert.IsTrue(button.isActiveAndEnabled); Assert.IsTrue(button.interactable);
            rig.Driver.EventSystem.SetSelectedGameObject(button.gameObject);
            ExecuteEvents.Execute(button.gameObject, new BaseEventData(rig.Driver.EventSystem), ExecuteEvents.submitHandler);
        }

        [Test]
        public void EverySavedTmpStartsWithTheExactDiagnosticPlaceholderBeforeHostBind()
        {
            using (var rig = new UguiHostRig(bind: false))
            {
                var texts = rig.Root.GetComponentsInChildren<TextMeshProUGUI>(true);
                Assert.Greater(texts.Length, 20);
                var linkedInputs = 0;
                foreach (var text in texts)
                {
                    var input = text.GetComponentInParent<TMP_InputField>(true);
                    if (input != null && ReferenceEquals(input.textComponent, text))
                    {
                        linkedInputs++; Assert.AreEqual(LocalizedTmpText.Placeholder, input.text, text.name);
                        Assert.That(text.text, NUnit.Framework.Is.EqualTo(LocalizedTmpText.Placeholder).Or.EqualTo(LocalizedTmpText.Placeholder + "\u200B"), text.name);
                    }
                    else Assert.AreEqual(LocalizedTmpText.Placeholder, text.text, text.name);
                }
                UguiResponsiveLayoutTests.AssertExactTextTargets(rig.Root);
                Assert.AreEqual(141, texts.Length); Assert.AreEqual(3, linkedInputs); Assert.AreEqual(138, texts.Length - linkedInputs);
                rig.Bind();
                Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                Assert.AreEqual(rig.Localization.Resolve("fm.profile.title", null).Text,
                    rig.Root.GetComponentsInChildren<LocalizedTmpText>(true).Single(t => t.Key == "fm.profile.title").Target.text);
            }
        }

        [Test]
        public void InactiveBindingAndDynamicCloneResolveBeforeTheirFirstActivation()
        {
            using (var rig = new UguiHostRig())
            {
                var template = Probe(rig);
                template.Bind(rig.Localization, "fm.common.action.back");
                Assert.IsFalse(template.gameObject.activeInHierarchy);
                Assert.AreEqual(rig.Localization.Resolve("fm.common.action.back", null).Text, template.Target.text);
                var clone = UnityEngine.Object.Instantiate(template, template.transform.parent, false);
                Assert.IsFalse(clone.gameObject.activeSelf);
                clone.Bind(rig.Localization, "fm.common.action.confirm");
                var resolved = clone.Target.text;
                Assert.AreEqual(rig.Localization.Resolve("fm.common.action.confirm", null).Text, resolved);
                clone.gameObject.SetActive(true);
                Assert.AreEqual(resolved, clone.Target.text); Assert.AreNotEqual(LocalizedTmpText.Placeholder, resolved);
                Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator LocaleChangeAndRebuiltPagePreserveHeadRequestAndPlaybackToken()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            enteredPlayMode = true;
            Assert.IsTrue(UnityEngine.Application.isPlaying);
            using (var rig = new UguiHostRig(true))
            {
                rig.Enter(); TestContext.Out.WriteLine(UguiSceneCompositionTests.BoardDiagnostic(rig));
                yield return rig.Ready(); UguiSceneCompositionTests.AssertBoardReady(rig);
                rig.BeginRoute(); rig.EndRoute();
                var input = rig.Session.Battle.Input; var token = input.View.PresentationToken;
                var request = input.LastRequest; var head = rig.Head; var page = rig.View.BattleView.Page;
                Assert.IsNotNull(token); var files = CopyFiles(rig.Storage.Files);
                Assert.IsTrue(rig.Localization.SetLocale(LocaleId.ZhHans));
                Assert.AreSame(head, rig.Head); Assert.AreSame(request, input.LastRequest);
                Assert.AreSame(token, input.View.PresentationToken); Assert.AreSame(page, rig.View.BattleView.Page);
                var name = rig.Localization.Resolve("fm.name.character.w", null).Text;
                Assert.IsTrue(rig.Root.GetComponentsInChildren<TextMeshProUGUI>(true).Any(t => t.gameObject.activeInHierarchy && t.text.Contains(name)), "Nested display-name arguments refresh with the locale.");
                rig.Bind();
                Assert.AreEqual(LocaleId.ZhHans, rig.Localization.CurrentLocale);
                Assert.AreSame(head, rig.Head); Assert.AreSame(request, input.LastRequest); Assert.AreSame(token, input.View.PresentationToken);
                Assert.IsTrue(rig.Root.GetComponentsInChildren<TextMeshProUGUI>(true).Any(t => t.gameObject.activeInHierarchy && t.text.Contains(name)));
                SameFiles(files, rig.Storage.Files);
                Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
            }
            yield return new UnityEngine.TestTools.ExitPlayMode();
            enteredPlayMode = false;
        }

        [Test]
        public void MissingKeyParametersTemplateFontAndGlyphOpenTheRealHostGate()
        {
            var entries = new UguiTestTextSource().Entries.Concat(new[] {
                new UguiTestTextSource.Entry("fm.test.bad_template", "{n", "{n", "n"),
                new UguiTestTextSource.Entry("fm.test.missing_glyph", "\U0010FFFF", "\U0010FFFF") });
            using (var rig = new UguiHostRig(source: new UguiTestTextSource(entries)))
            {
                foreach (var failure in new[] { "key", "parameters", "template", "font", "glyph" })
                {
                    var probe = Probe(rig);
                    TMP_FontAsset missingFont = null;
                    try
                    {
                        var key = failure == "key" ? "fm.test.absent" : failure == "parameters" ? "fm.battle.phase.label" :
                            failure == "template" ? "fm.test.bad_template" : failure == "glyph" ? "fm.test.missing_glyph" : "fm.common.action.back";
                        if (failure == "font")
                        {
                            missingFont = UnityEngine.Object.Instantiate(rig.Host.FontAsset);
                            missingFont.material = null;
                            UguiHostRig.SetReference(probe.Target, "m_fontAsset", missingFont);
                        }
                        probe.Bind(rig.Localization, key);
                        probe.gameObject.SetActive(true);
                        Assert.IsNotNull(probe.DiagnosticCode, failure); Assert.IsNull(probe.Severity, failure);
                        if (failure == "font") Assert.AreEqual("MissingFontOrMaterial", probe.DiagnosticCode);
                        Assert.AreEqual(LocalizedTmpText.Placeholder, probe.Target.text, failure);
                        Assert.IsTrue(rig.View.DiagnosticVisible, failure);
                        Assert.IsNotNull(rig.View.DiagnosticCode, failure);
                    }
                    finally
                    {
                        probe.Unbind(); UnityEngine.Object.DestroyImmediate(probe.gameObject);
                        if (missingFont != null) UnityEngine.Object.DestroyImmediate(missingFont);
                    }
                    Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                }
            }
        }

        [Test]
        public void AllBusinessSeveritiesKeepContinueRetryAndResolveUnderControllerAuthority()
        {
            foreach (var severity in new[] { LocalizedTextSeverity.Info, LocalizedTextSeverity.Warning,
                LocalizedTextSeverity.Error, LocalizedTextSeverity.Blocking })
            {
                using (var rig = new UguiHostRig(true, new SeveritySource(severity)))
                {
                    rig.Enter(); var head = rig.Head; var navigation = rig.Session.Navigation;
                    var level = navigation.View.Read.Levels[0];
                    navigation.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                        LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = navigation.View.Read.Binding })();
                    // Bind the actual navigation surface to the existing active-battle state for its availability contract.
                    rig.Find<FightMatchViewId>("fm.page.battle").gameObject.SetActive(false);
                    rig.View.NavigationView.Bind(navigation, rig.Localization);
                    rig.Find<FightMatchViewId>("fm.page.navigation").gameObject.SetActive(true);
                    var resume = rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("navigation.ResumeBattleRequested"));
                    Assert.IsTrue(resume.interactable); Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                    Submit(rig, resume);
                    Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page); Assert.AreSame(head, rig.Head);
                    var probe = Probe(rig); probe.Bind(rig.Localization, "fm.common.action.back");
                    Assert.AreEqual(severity, probe.Severity); Assert.IsNull(probe.DiagnosticCode);
                    Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                }
                foreach (var fault in new[] { "snapshot-before", "marker-after" })
                using (var rig = new UguiHostRig(true, new SeveritySource(severity)))
                {
                    rig.Enter(); var controller = rig.Session.Battle; var page = rig.View.BattleView.Page;
                    var preview = controller.PreviewEnd(page, CandidateApplicationKind.ExitAttempt, controller.Refresh().Context);
                    Assert.IsNotNull(preview.Confirmation); rig.Storage.Fault = fault;
                    var result = controller.Confirm(page, preview.Confirmation);
                    Assert.AreEqual(fault == "snapshot-before" ? "SaveFailed" : "CommitUnknown", result.Result.Code);
                    var retry = rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("retry-battle-original"));
                    var resolve = rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("resolve-battle-original"));
                    Assert.AreEqual(result.CanRetry, retry.interactable); Assert.AreEqual(result.CanResolve, resolve.interactable);
                    Assert.IsTrue(fault == "snapshot-before" ? retry.interactable : resolve.interactable);
                    Assert.IsFalse(rig.View.DiagnosticVisible, rig.View.DiagnosticCode);
                }
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator ThreeBindUnbindCyclesRemoveOldListenersAndProtectTheNextGeneration()
        {
            var retained = new GameObject("RetainedOldControls");
            var stale = new List<UnityEngine.UI.Button>(); var changes = 0;
            try
            {
                for (var cycle = 0; cycle < 3; cycle++)
                using (var rig = new UguiHostRig())
                {
                    rig.Localization.LocaleChanged += _ => changes++;
                    foreach (var button in stale)
                    {
                        var before = changes;
                        ExecuteEvents.Execute(button.gameObject, new BaseEventData(rig.Driver.EventSystem), ExecuteEvents.submitHandler);
                        Assert.AreEqual(before, changes, "Old controls must not invoke an old or new localization service.");
                    }
                    yield return rig.Ready();
                    rig.Driver.Click(rig.Find<UnityEngine.UI.Button>("fm.action.language.open"));
                    var chinese = rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("language.zhHans"));
                    yield return rig.Ready((RectTransform)chinese.transform);
                    rig.Driver.Click(chinese);
                    Assert.AreEqual(cycle + 1, changes);
                    Assert.AreEqual(LocaleId.ZhHans, rig.Localization.CurrentLocale);
                    var english = rig.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("language.en"));
                    english.transform.SetParent(retained.transform, false);
                    rig.View.Unbind(); english.gameObject.SetActive(true); stale.Add(english);
                    ExecuteEvents.Execute(english.gameObject, new BaseEventData(rig.Driver.EventSystem), ExecuteEvents.submitHandler);
                    Assert.AreEqual(cycle + 1, changes);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(retained); }
        }
    }
}
