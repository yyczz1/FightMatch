using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchButtonDropdownTests
    {
        private const string Prefab = "Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab";
        private bool enteredPlayMode;

        private static LocalizationService RealLocalization()
        {
            var bytes = File.ReadAllBytes(Path.Combine(UnityEngine.Application.dataPath, "../Generated/FightMatch/Localization/fm-text-v1.json"));
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes,
                "f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc", 264, out var source, out var code), code);
            return new LocalizationService(source, SystemLanguage.English);
        }
        private static T Field<T>(FightMatchHostView view, string name) =>
            (T)typeof(FightMatchHostView).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
        private static string License => File.ReadAllText(Path.Combine(HostRig.Project, "Assets/UI/FightMatch/Fonts/OFL.txt"));
        private static bool Click(GameObject target) => ExecuteEvents.Execute(target,
            new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
        private static void Submit(GameObject target) => ExecuteEvents.Execute(target,
            new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        private static UnityEngine.UI.Toggle[] Items(FightMatchLocaleDropdown dropdown)
        {
            Assert.IsTrue(dropdown.IsExpanded);
            // TMP can retain an inactive, same-named list until Destroy runs at the end of this frame.
            var lists = dropdown.transform.Cast<Transform>().Where(x => x.name == "Dropdown List" && x.gameObject.activeInHierarchy).ToArray();
            Assert.AreEqual(1, lists.Length, "Exactly one live dropdown list must own the options.");
            var list = lists[0];
            var items = list.GetComponentsInChildren<UnityEngine.UI.Toggle>(); Assert.AreEqual(2, items.Length);
            Assert.AreEqual(0, list.GetComponentsInChildren<FightMatchViewId>(true).Length);
            return items;
        }
        private static void GlyphsInside(TextMeshProUGUI text, int maxLines = int.MaxValue)
        {
            text.ForceMeshUpdate(true);
            Assert.LessOrEqual(text.textInfo.lineCount, maxLines, text.text);
            Assert.IsFalse(text.isTextTruncated, text.text);
            Assert.GreaterOrEqual(text.fontSize, 16);
            foreach (var character in text.textInfo.characterInfo.Take(text.textInfo.characterCount).Where(x => x.isVisible))
            {
                Assert.GreaterOrEqual(character.bottomLeft.x, text.rectTransform.rect.xMin - .1f, text.text);
                Assert.LessOrEqual(character.topRight.x, text.rectTransform.rect.xMax + .1f, text.text);
                Assert.GreaterOrEqual(character.bottomLeft.y, text.rectTransform.rect.yMin - .1f, text.text);
                Assert.LessOrEqual(character.topRight.y, text.rectTransform.rect.yMax + .1f, text.text);
            }
        }
        private static void ActiveButtonsFit(FightMatchHostView view)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var button in view.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                var labels = button.GetComponentsInChildren<LocalizedTmpText>();
                foreach (var label in labels)
                {
                    Assert.IsNull(label.DiagnosticCode, button.name);
                    Assert.AreNotEqual(LocalizedTmpText.Placeholder, label.Target.text, button.name);
                    GlyphsInside(label.Target);
                }
                if (labels.Length == 2)
                {
                    var first = new Vector3[4]; var second = new Vector3[4];
                    labels[0].Target.rectTransform.GetWorldCorners(first); labels[1].Target.rectTransform.GetWorldCorners(second);
                    Assert.GreaterOrEqual(first[0].y, second[1].y, "Caption and disabled reason must not overlap: " + button.name);
                }
            }
        }

        [Test]
        public void SavedButtonsAndTemplatesUseWrappedOwnedLayoutAndTwoLanguageOptions()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab); Assert.IsNotNull(prefab);
            var buttons = prefab.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            Assert.AreEqual(42, buttons.Length, "46 original buttons minus the language entry and three popup buttons.");
            foreach (var button in buttons)
            {
                Assert.IsNotNull(button.GetComponent<UnityEngine.UI.VerticalLayoutGroup>(), button.name);
                Assert.GreaterOrEqual(button.GetComponent<UnityEngine.UI.LayoutElement>().minHeight, 48, button.name);
                Assert.IsNull(button.GetComponent<UnityEngine.UI.ContentSizeFitter>(), button.name);
                foreach (var text in button.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    Assert.AreEqual(LocalizedTmpText.Placeholder, text.text, button.name);
                    Assert.AreEqual(18, text.fontSize); Assert.AreEqual(16, text.fontSizeMin);
                    Assert.IsTrue(text.enableWordWrapping); Assert.AreEqual(TextOverflowModes.Overflow, text.overflowMode);
                    Assert.AreEqual(-1, text.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight);
                }
            }
            var dropdown = prefab.GetComponentInChildren<FightMatchLocaleDropdown>(true);
            Assert.AreEqual(2, dropdown.options.Count); Assert.IsNotNull(dropdown.placeholder);
            Assert.AreEqual(-1, dropdown.value); Assert.AreEqual(0, dropdown.template.GetComponentsInChildren<FightMatchViewId>(true).Length);
            Assert.IsNull(prefab.transform.Find("RuntimeCanvas/SafeAreaRoot/PopupLayer/LanguagePopup"));
            Assert.IsTrue(FightMatchViewId.Validate(prefab.transform, out var error), error);
        }

        [UnityTest]
        public IEnumerator RealPointerAndKeyboardSelectionPreserveRetryFeedbackAndLifetime()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var rig = new HostRig(false))
            using (var panel = new HostPanel(rig))
            {
                var view = panel.View; var service = RealLocalization(); var saves = 0;
                var result = LocalePreferenceSaveResult.Saved();
                Func<LocaleId, LocalePreferenceSaveResult> select = locale => { saves++; service.SetLocale(locale); return result; };
                view.Bind(rig.Session, service, License, select);
                yield return null; Canvas.ForceUpdateCanvases();
                Assert.IsNull(view.DiagnosticCode);
                var dropdown = Field<FightMatchLocaleDropdown>(view, "languageDropdown");
                var feedback = Field<LocalizedTmpText>(view, "languageFeedback");
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    foreach (var outcome in new[] { LocalePreferenceSaveResult.Failed("PreferenceCommitFailed"),
                        LocalePreferenceSaveResult.Failed("PreferenceTargetReopenFailed", true), LocalePreferenceSaveResult.Saved() })
                    {
                        result = outcome;
                        var previous = saves; Click(dropdown.gameObject);
                        var items = Items(dropdown);
                        Click(items[locale == LocaleId.En ? 0 : 1].gameObject);
                        yield return null; yield return null; Canvas.ForceUpdateCanvases();
                        Assert.AreEqual(previous + 1, saves, "Same-language choice retries the save.");
                        Assert.AreEqual(locale, service.CurrentLocale); Assert.AreEqual(-1, dropdown.value);
                        Assert.IsFalse(dropdown.IsExpanded); Assert.IsTrue(feedback.gameObject.activeInHierarchy);
                        Assert.AreEqual(outcome.LocalizationKey, feedback.Key); Assert.IsNull(feedback.DiagnosticCode);
                        Assert.AreEqual(service.Resolve(locale == LocaleId.En ? "fm.language.option.en" : "fm.language.option.zh_cn", null).Text,
                            Field<LocalizedTmpText>(view, "languageCaption").Target.text);
                        GlyphsInside(feedback.Target);
                        ActiveButtonsFit(view);
                        var license = Field<UnityEngine.UI.Button>(view, "licenseButton");
                        GlyphsInside(license.GetComponentInChildren<TextMeshProUGUI>(), 2);
                        Assert.IsTrue(FightMatchViewId.Validate(view.transform, out var error), error);
                    }
                }
                Submit(dropdown.gameObject); var keyboardItems = Items(dropdown);
                EventSystem.current.SetSelectedGameObject(keyboardItems[0].gameObject);
                ExecuteEvents.Execute(keyboardItems[0].gameObject,
                    new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Down }, ExecuteEvents.moveHandler);
                Assert.AreSame(keyboardItems[1].gameObject, EventSystem.current.currentSelectedGameObject);
                var beforeKeyboard = saves; Submit(EventSystem.current.currentSelectedGameObject);
                yield return null; yield return null;
                Assert.AreEqual(beforeKeyboard + 1, saves); Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale);
                Submit(dropdown.gameObject); Assert.IsTrue(dropdown.IsExpanded);
                ExecuteEvents.Execute(dropdown.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.cancelHandler);
                yield return null; yield return null; Assert.IsFalse(dropdown.IsExpanded);
                Assert.AreEqual(beforeKeyboard + 1, saves, "Cancel does not save.");

                var stale = Field<UnityAction<int>>(view, "localeSelection");
                Click(dropdown.gameObject); Assert.IsTrue(dropdown.IsExpanded);
                var retiredList = dropdown.transform.Find("Dropdown List"); var retiredOption = Items(dropdown)[0];
                view.Bind(rig.Session, service, License, select);
                Assert.IsFalse(dropdown.IsExpanded);
                Assert.IsFalse(retiredList.gameObject.activeInHierarchy, "Rebind must disable the old list synchronously.");
                var beforeRebind = saves; stale(0); Assert.AreEqual(beforeRebind, saves);
                Assert.IsFalse(Click(retiredOption.gameObject), "The retired option must not receive pointer events.");
                Assert.AreEqual(beforeRebind, saves);
                Click(dropdown.gameObject);
                Assert.AreSame(retiredList, dropdown.transform.Find("Dropdown List"), "The old name-only lookup still finds the retired list in this frame.");
                var reboundOption = Items(dropdown)[0]; Assert.AreNotSame(retiredOption, reboundOption);
                Assert.AreEqual(-1, dropdown.value); Assert.IsFalse(reboundOption.isOn);
                Assert.IsTrue(dropdown.CanOpen(), "The new binding must accept the current epoch and guards.");
                var selectionEvents = 0; UnityAction<int> observeSelection = _ => selectionEvents++;
                dropdown.onValueChanged.AddListener(observeSelection);
                try
                {
                    Assert.IsTrue(Click(reboundOption.gameObject), "The live option must receive the pointer event.");
                    Assert.AreEqual(1, selectionEvents, "The live Toggle must emit exactly one dropdown selection before the save assertion.");
                }
                finally { dropdown.onValueChanged.RemoveListener(observeSelection); }
                yield return null; yield return null;
                Assert.AreEqual(beforeRebind + 1, saves, "Rebind leaves one selection listener.");
                Click(dropdown.gameObject); view.Unbind();
                Assert.IsFalse(dropdown.IsExpanded); Assert.IsNull(dropdown.CanOpen);
                Assert.IsFalse(view.GetComponentsInChildren<Canvas>(true).Any(x => x.name == "Blocker" && x.gameObject.activeInHierarchy));
                var afterUnbind = saves; dropdown.onValueChanged.Invoke(1); service.SetLocale(LocaleId.ZhHans);
                Assert.AreEqual(afterUnbind, saves); Assert.AreEqual(LocalizedTmpText.Placeholder, feedback.Target.text);
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }

        [UnityTest]
        public IEnumerator DragAndRecoveryGuardOpeningAndPendingChoiceSynchronously()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var rig = new HostRig())
            using (var panel = new HostPanel(rig))
            {
                var view = panel.View; var service = RealLocalization(); var saves = 0;
                view.Bind(rig.Session, service, License, locale => { saves++; service.SetLocale(locale); return LocalePreferenceSaveResult.Saved(); });
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                { service.SetLocale(locale); ActiveButtonsFit(view); }
                rig.Enter(); yield return panel.Ready();
                var dropdown = Field<FightMatchLocaleDropdown>(view, "languageDropdown");
                var board = view.BattleView.PlaybackView.InputView.Board;
                var member = rig.State.Members.First(x => x.Hp.Numerator.Sign > 0);
                Assert.IsTrue(rig.Session.Battle.Input.SelectMember(member.Member.CharacterId));
                var route = rig.Route(rig.State.Enemies.First(x => x.Hp.Numerator.Sign > 0).PairKey);
                var pointer = new PointerEventData(EventSystem.current) { pointerId = 3,
                    position = RectTransformUtility.WorldToScreenPoint(null, board.rectTransform.TransformPoint(board.CellCenter(route[0]))) };
                board.OnPointerDown(pointer); Assert.IsTrue(board.HasActivePointer);
                Click(dropdown.gameObject); Submit(dropdown.gameObject); Assert.IsFalse(dropdown.IsExpanded);
                dropdown.onValueChanged.Invoke(1); Assert.AreEqual(0, saves); Assert.IsTrue(board.HasActivePointer);
                board.CancelPointer();
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    service.SetLocale(locale); Canvas.ForceUpdateCanvases();
                    var button = FightMatchViewId.Find<UnityEngine.UI.Button>(view.transform, FightMatchViewId.Member(member.Member.CharacterId));
                    var text = button.GetComponentInChildren<TextMeshProUGUI>();
                    StringAssert.Contains(service.Resolve("fm.name.character.w", null).Text + "\n", text.text);
                    StringAssert.Contains("/", text.text); GlyphsInside(text, 2);
                    ActiveButtonsFit(view);
                }
                Click(dropdown.gameObject); var item = Items(dropdown)[1];
                Field<GameObject>(view, "navigationRecoveryPanel").SetActive(true);
                Click(item.gameObject); Assert.AreEqual(0, saves); Assert.IsFalse(dropdown.IsExpanded);
                Assert.AreEqual(-1, dropdown.value);
                view.SynchronizeOverlayRoots();
                Click(dropdown.gameObject); Submit(dropdown.gameObject); Assert.IsFalse(dropdown.IsExpanded);
                Field<GameObject>(view, "navigationRecoveryPanel").SetActive(false); view.SynchronizeOverlayRoots();
                Click(dropdown.gameObject); Assert.IsTrue(dropdown.IsExpanded);
                view.ShowFailure("DropdownTestFailure"); Assert.IsFalse(dropdown.IsExpanded);
                dropdown.onValueChanged.Invoke(0); Assert.AreEqual(0, saves);
                Click(dropdown.gameObject); Submit(dropdown.gameObject); Assert.IsFalse(dropdown.IsExpanded);
                view.Unbind();
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }

        [UnityTest]
        public IEnumerator BackClosesDropdownBeforePageForEitherCancelOrderAndAfterRebind()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var rig = new HostRig())
            using (var panel = new HostPanel(rig))
            {
                var view = panel.View; var service = RealLocalization(); var saves = 0; var quits = 0;
                Func<LocaleId, LocalePreferenceSaveResult> select = locale =>
                { saves++; service.SetLocale(locale); return LocalePreferenceSaveResult.Saved(); };
                view.Bind(rig.Session, service, License, select);
                rig.Session.QuitRequested += () => quits++;
                // Exercise Update's real route without starting another host or opening product storage.
                var hostObject = new GameObject("DropdownBackTestHost"); hostObject.SetActive(false);
                var host = hostObject.AddComponent<FightMatchPlayerHost>();
                var sessionField = typeof(FightMatchPlayerHost).GetField("<Session>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                sessionField.SetValue(host, rig.Session);
                typeof(FightMatchPlayerHost).GetField("runtimeRoot", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(host, view);
                try
                {
                    var dropdown = Field<FightMatchLocaleDropdown>(view, "languageDropdown");
                    var quitPopup = Field<GameObject>(view, "quitPopup");
                    foreach (var page in new[] { FightMatchHostPage.Navigation, FightMatchHostPage.Battle })
                    {
                        if (page == FightMatchHostPage.Battle) rig.Enter();
                        yield return panel.Ready();
                        Assert.AreEqual(page, rig.Session.Page);
                        var head = rig.Head; var files = rig.Files();
                        foreach (var eventSystemFirst in new[] { false, true })
                        foreach (var cancelOption in new[] { false, true })
                        {
                            Submit(dropdown.gameObject);
                            var option = Items(dropdown)[1].gameObject;
                            var target = cancelOption ? option : dropdown.gameObject;
                            EventSystem.current.SetSelectedGameObject(target);
                            if (eventSystemFirst)
                            {
                                Assert.IsTrue(ExecuteEvents.Execute(target, new BaseEventData(EventSystem.current), ExecuteEvents.cancelHandler),
                                    "The actual generated option and dropdown root must both receive native Cancel.");
                                Assert.IsTrue(dropdown.IsExpanded, "TMP defers destruction even at zero fade speed until after Update.");
                            }
                            host.HandleBack();
                            if (!eventSystemFirst)
                            {
                                Assert.AreEqual(!cancelOption,
                                    ExecuteEvents.Execute(target, new BaseEventData(EventSystem.current), ExecuteEvents.cancelHandler),
                                    "Host-first closes the option synchronously; a later root Cancel is harmless.");
                            }
                            Assert.IsFalse(dropdown.IsExpanded);
                            Assert.IsFalse(option.activeInHierarchy);
                            Assert.IsFalse(view.GetComponentsInChildren<Canvas>(true).Any(x => x.name == "Blocker" && x.gameObject.activeInHierarchy));
                            Assert.AreEqual(page, rig.Session.Page, "One Back must only close the dropdown.");
                            Assert.IsFalse(quitPopup.activeInHierarchy);
                            Assert.AreEqual(0, saves); Assert.AreEqual(0, quits);
                            Assert.AreSame(head, rig.Head); rig.SameFiles(files);
                            yield return null;
                            host.HandleBack();
                            Assert.AreEqual(FightMatchHostPage.QuitConfirmation, rig.Session.Page, "The next Back retains the existing page behavior.");
                            Assert.IsTrue(quitPopup.activeInHierarchy);
                            host.HandleBack();
                            Assert.AreEqual(page, rig.Session.Page);
                            Assert.AreEqual(0, saves); Assert.AreEqual(0, quits);
                            Assert.AreSame(head, rig.Head); rig.SameFiles(files);
                        }
                    }
                    var finalHead = rig.Head; var finalFiles = rig.Files();
                    foreach (var rebindBeforeBack in new[] { false, true })
                    {
                        Submit(dropdown.gameObject);
                        var option = Items(dropdown)[1].gameObject;
                        EventSystem.current.SetSelectedGameObject(option);
                        Assert.IsTrue(ExecuteEvents.Execute(option, new BaseEventData(EventSystem.current), ExecuteEvents.cancelHandler));
                        view.Unbind();
                        if (rebindBeforeBack) view.Bind(rig.Session, service, License, select);
                        Assert.IsFalse(dropdown.IsExpanded);
                        host.HandleBack();
                        Assert.AreEqual(FightMatchHostPage.QuitConfirmation, rig.Session.Page, "Unbind/rebind must not retain a swallowed Back.");
                        host.HandleBack(); Assert.AreEqual(FightMatchHostPage.Battle, rig.Session.Page);
                        if (!rebindBeforeBack) view.Bind(rig.Session, service, License, select);
                        Assert.AreEqual(0, saves); Assert.AreEqual(0, quits);
                        Assert.AreSame(finalHead, rig.Head); rig.SameFiles(finalFiles);
                    }
                }
                finally
                {
                    sessionField.SetValue(host, null); // HostRig retains ownership of the session.
                    UnityEngine.Object.DestroyImmediate(hostObject);
                }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }

        [UnityTest]
        public IEnumerator LanguageFeedbackExpiresOnlyForLatestSuccessAndClearsOnUnbind()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode(); enteredPlayMode = true;
            using (var rig = new HostRig(false))
            using (var panel = new HostPanel(rig))
            {
                var view = panel.View; var service = RealLocalization(); var saves = 0;
                var result = LocalePreferenceSaveResult.Saved();
                Func<LocaleId, LocalePreferenceSaveResult> select = locale =>
                { saves++; if (result != null) service.SetLocale(locale); return result; };
                view.Bind(rig.Session, service, License, select);
                yield return null; Canvas.ForceUpdateCanvases();
                var dropdown = Field<FightMatchLocaleDropdown>(view, "languageDropdown");
                var feedback = Field<LocalizedTmpText>(view, "languageFeedback");
                var feedbackPanel = Field<GameObject>(view, "languageFeedbackPanel");
                Action<int> choose = index =>
                {
                    Assert.IsTrue(dropdown.interactable, "Feedback must not block another language choice.");
                    Submit(dropdown.gameObject);
                    Assert.IsTrue(Click(Items(dropdown)[index].gameObject));
                    Assert.AreEqual(-1, dropdown.value);
                };
                var originalTimeScale = Time.timeScale;
                try
                {
                    choose(0);
                    Assert.IsTrue(feedbackPanel.activeInHierarchy); Assert.AreEqual("fm.language.saved", feedback.Key);
                    yield return new WaitForSecondsRealtime(1.1f); yield return null;
                    result = null; choose(0);
                    Assert.AreEqual("fm.language.saved", feedback.Key, "A null callback result must preserve the existing message and deadline.");
                    yield return new WaitForSecondsRealtime(1.1f); yield return null;
                    Assert.IsFalse(feedbackPanel.activeSelf); Assert.IsFalse(feedback.gameObject.activeSelf);
                    Assert.AreEqual(2, saves);

                    Time.timeScale = 0;
                    result = LocalePreferenceSaveResult.Saved(); choose(1);
                    yield return new WaitForSecondsRealtime(1.2f); yield return null;
                    Assert.IsTrue(feedbackPanel.activeInHierarchy);
                    choose(1); // Same-language success starts a fresh interval while play is paused.
                    yield return new WaitForSecondsRealtime(1.1f); yield return null;
                    Assert.IsTrue(feedbackPanel.activeInHierarchy, "The first success deadline must not hide the second success.");
                    yield return new WaitForSecondsRealtime(1.1f); yield return null;
                    Assert.IsFalse(feedbackPanel.activeSelf); Assert.IsFalse(feedback.gameObject.activeSelf);
                    Assert.AreEqual(4, saves);

                    view.Bind(rig.Session, service, License);
                    choose(0);
                    Assert.AreEqual("fm.language.changed", feedback.Key); Assert.IsTrue(feedbackPanel.activeInHierarchy);
                    Assert.AreEqual(LocaleId.En, service.CurrentLocale);
                    yield return new WaitForSecondsRealtime(2.1f); yield return null;
                    Assert.IsFalse(feedbackPanel.activeSelf); Assert.AreEqual(4, saves);
                    view.Bind(rig.Session, service, License, select);
                    foreach (var failure in new[] { LocalePreferenceSaveResult.Failed("PreferenceCommitFailed"),
                        LocalePreferenceSaveResult.Failed("PreferenceTargetReopenFailed", true) })
                    {
                        result = LocalePreferenceSaveResult.Saved(); choose(1);
                        yield return null; yield return null;
                        result = failure; choose(1);
                        Assert.AreEqual(failure.LocalizationKey, feedback.Key);
                        yield return new WaitForSecondsRealtime(2.1f); yield return null;
                        Assert.IsTrue(feedbackPanel.activeInHierarchy, "A later failed or unknown save must outlive the old success deadline.");
                        Assert.IsTrue(feedback.gameObject.activeInHierarchy); Assert.IsNull(feedback.DiagnosticCode);
                    }
                    Assert.AreEqual(8, saves);

                    foreach (var cleanup in new[] { "Unbind", "Rebind", "Disable" })
                    {
                        result = LocalePreferenceSaveResult.Saved(); choose(0);
                        yield return null; yield return null;
                        Assert.IsTrue(feedbackPanel.activeInHierarchy);
                        if (cleanup == "Unbind") view.Unbind();
                        else if (cleanup == "Rebind") view.Bind(rig.Session, service, License, select);
                        else view.enabled = false;
                        Assert.IsFalse(feedbackPanel.activeSelf, cleanup);
                        Assert.AreEqual(LocalizedTmpText.Placeholder, feedback.Target.text, cleanup);
                        if (cleanup == "Disable") view.enabled = true;
                        if (cleanup != "Rebind") view.Bind(rig.Session, service, License, select);
                        result = LocalePreferenceSaveResult.Failed("PreferenceTargetReopenFailed", true); choose(1);
                        yield return new WaitForSecondsRealtime(2.1f); yield return null;
                        Assert.IsTrue(feedbackPanel.activeInHierarchy, cleanup + " must retire the old success deadline.");
                        Assert.AreEqual(result.LocalizationKey, feedback.Key); Assert.IsNull(feedback.DiagnosticCode);
                    }
                    Assert.AreEqual(14, saves);
                }
                finally { Time.timeScale = originalTimeScale; }
            }
            yield return new ExitPlayMode(); enteredPlayMode = false;
        }

        [UnityTearDown]
        public IEnumerator LeavePlayModeAfterFailure()
        {
            if (enteredPlayMode && UnityEngine.Application.isPlaying) yield return new ExitPlayMode();
            enteredPlayMode = false;
        }
    }
}
