using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Input;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine.TestTools;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerDefaultReferenceTests
    {
        [UnityTest]
        public IEnumerator UGUI_COPY_WIRE02_FirstDragNoticeSurvivesAndReplayAppearsOnlyAfterCompletion()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
                UnityEditor.SceneManagement.NewSceneMode.Single);
            yield return new EnterPlayMode();
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                var reference = Read(r); var overlay = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board;
                var input = r.Host.Input; input.SelectMember("W");
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads; var page = r.Page.Page;
                var starts = r.Host.Playback.Starts; var reports = r.Host.Playback.CompletionReports;
                overlay.Show(r.Host, page, board, reference); Assert.IsTrue(overlay.PlayStep(0));
                Func<int, LocalizedTmpText> caption = index => overlay.Find<UnityEngine.UI.Button>(
                    FightMatchViewId.Row("reference-step:" + reference.Steps[index].OperationId)).GetComponentInChildren<LocalizedTmpText>(true);
                Assert.AreEqual("fm.reference.step.row", caption(0).Key);
                foreach (var fact in reference.Steps[0].OrderedFacts)
                {
                    overlay.Advance(CandidateBattlePlaybackController.FactMilliseconds);
                    Assert.AreSame(fact, overlay.Frame.OriginalFact); Assert.AreEqual("fm.reference.step.row", caption(0).Key);
                }
                overlay.Advance(179); Assert.AreEqual("fm.reference.step.row", caption(0).Key);
                var frame = overlay.Frame; var generation = overlay.Generation;
                r.Canvas.Localization.SetLocale(LocaleId.En);
                Assert.AreSame(frame, overlay.Frame); Assert.AreEqual(generation, overlay.Generation); Assert.AreSame(page, r.Page.Page);
                overlay.Advance(1); Assert.AreEqual("fm.reference.replay_button", caption(0).Key);
                Assert.AreEqual(BattleText.Resolve(r.Canvas.Localization, "fm.reference.replay_button"), caption(0).Target.text);
                Assert.IsTrue(Enumerable.Range(1, reference.Steps.Count - 1).All(index => caption(index).Key == "fm.reference.step.row"));
                Assert.Greater(reference.Steps.Count, 1); Assert.IsTrue(overlay.PlayStep(1));
                Assert.AreEqual("fm.reference.step.row", caption(0).Key); Assert.AreEqual("fm.reference.step.row", caption(1).Key);
                overlay.Close(); Assert.IsNull(input.VisibleFeedback, "Manual close publishes no first-drag notice.");
                overlay.Show(r.Host, page, board, reference); Assert.IsTrue(overlay.PlayStep(0));
                var route = r.Route(r.State.Enemies[0].PairKey); var driver = r.Canvas.Driver;
                using (var observed = new BattleFeedbackObserver(input))
                {
                    driver.Down(UguiPointerDriver.Point(board, route[0]), 1);
                    driver.Move(UguiPointerDriver.Point(board, route[1]), 1);
                    Assert.IsFalse(overlay.IsOpen); Assert.IsNull(board.ReferenceOverride); Assert.AreEqual(1, input.Gesture.ActivePointerId);
                    Assert.IsTrue(board.HasActivePointer); Assert.AreEqual(GestureStage.Dragging, input.Gesture.Stage);
                    CollectionAssert.AreEqual(new[] { CandidateBoardVisibleFeedbackKind.ReferenceClosedForInput }, observed.Published);
                    var revision = input.VisibleFeedbackRevision; var cells = input.Gesture.DraftCells.ToArray();
                    BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, "fm.reference.closed_for_input");
                    input.Refresh(); yield return new UnityEngine.WaitForSecondsRealtime(.23f);
                    r.Canvas.Localization.SetLocale(LocaleId.ZhHans);
                    Assert.AreEqual(revision, input.VisibleFeedbackRevision); Assert.AreEqual(1, input.Gesture.ActivePointerId);
                    CollectionAssert.AreEqual(cells, input.Gesture.DraftCells); Assert.AreSame(page, r.Page.Page);
                    BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, "fm.reference.closed_for_input");
                    Assert.AreEqual(starts, r.Host.Playback.Starts); Assert.AreEqual(reports, r.Host.Playback.CompletionReports);
                    Assert.IsNull(input.LastRequest); r.Unchanged(head, files, clocks);
                    driver.Up(UguiPointerDriver.Point(board, route.Last()), 1);
                    Assert.IsTrue(input.LastResult.Application.IsCommitted); Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count);
                    CollectionAssert.AreEqual(new[] { CandidateBoardVisibleFeedbackKind.ReferenceClosedForInput,
                        CandidateBoardVisibleFeedbackKind.ActionAccepted }, observed.Published);
                    BattleCopyAssert.Feedback(r.Page.PlaybackView.InputView, input, r.Canvas.Localization, "fm.battle.action.accepted");
                    r.Host.Playback.SkipToFinal(); Assert.IsFalse(overlay.IsOpen); Assert.IsNull(board.ReferenceOverride);
                }
            }
            yield return new ExitPlayMode();
        }

        [Test]
        public void UGUI_COPY_WIRE04_ReferenceInitialFrameNeverClaimsActionSubmitted()
        {
            using (var r = new PlayerBattleRig())
            {
                var reference = Read(r); var overlay = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var starts = r.Host.Playback.Starts; var reports = r.Host.Playback.CompletionReports;
                overlay.Show(r.Host, r.Page.Page, board, reference);
                BattleCopyAssert.Localized(overlay, "reference-beat", r.Canvas.Localization, "fm.reference.step.select_prompt");
                foreach (var locale in new[] { LocaleId.ZhHans, LocaleId.En })
                {
                    r.Canvas.Localization.SetLocale(locale); Assert.IsTrue(overlay.PlayStep(0));
                    Assert.IsNull(overlay.Frame.OriginalFact); Assert.AreEqual(-1, overlay.Frame.OriginalFactIndex);
                    BattleCopyAssert.Localized(overlay, "reference-beat", r.Canvas.Localization, "fm.reference.step.select_prompt");
                    Assert.IsTrue(overlay.GetComponentsInChildren<LocalizedTmpText>(true).Any(x => x.gameObject.activeInHierarchy && x.Key == "fm.reference.scope_notice"));
                    Assert.IsFalse(overlay.GetComponentsInChildren<LocalizedTmpText>(true).Any(x => x.gameObject.activeInHierarchy && x.Key == "fm.battle.playback.action_submitted"));
                    StringAssert.DoesNotContain(BattleText.Resolve(r.Canvas.Localization, "fm.battle.playback.action_submitted"), BattleCopyAssert.VisibleText(overlay));
                    overlay.Advance(179); Assert.IsNull(overlay.Frame.OriginalFact);
                    BattleCopyAssert.Localized(overlay, "reference-beat", r.Canvas.Localization, "fm.reference.step.select_prompt");
                    overlay.Advance(1); Assert.AreSame(reference.Steps[0].OrderedFacts[0], overlay.Frame.OriginalFact);
                    Assert.AreEqual("fm.battle.playback.beat_row", overlay.Find<LocalizedTmpText>(FightMatchViewId.Row("reference-beat")).Key);
                }
                Assert.AreEqual(starts, r.Host.Playback.Starts); Assert.AreEqual(reports, r.Host.Playback.CompletionReports);
                Assert.IsNull(r.Host.Input.LastRequest); Assert.IsNull(r.Host.Input.View.PresentationToken); r.Unchanged(head, files, clocks);
            }
        }

        private static PlayerDefaultReference Read(PlayerBattleRig r)
        {
            var view = r.View; var entry = r.State.Baseline.Entry;
            var result = r.Host.ReadReference(r.Page.Page, entry.Level.LevelId, entry.Level.LevelVersion, view.Context.Binding);
            Assert.IsTrue(result.IsAvailable, result.Code); return result.Value;
        }
        [Test] public void B10_ResolvePreservesAlreadyVerifiedImmutableReferencesWithoutSerializedProperties()
        {
            var catalog = RealCatalog(); var publication = Resolve(catalog); var references = publication.GetDefaultReferences();
            Assert.AreEqual("b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129", publication.Binding.ContentFingerprint);
            Assert.AreEqual(1, references.Count); Assert.IsTrue(references[0].IsAccepted);
            Assert.AreSame(references, publication.GetDefaultReferences()); Assert.AreSame(references[0].Run.Records, references[0].ExecutedSteps);
            CollectionAssert.AreEqual(Enumerable.Range(0, 48).Select(i => (byte)i), references[0].SeedBytes);
            Assert.Throws<NotSupportedException>(() => ((IList<DemoContentReplayResult>)references).Add(references[0]));
            Assert.IsFalse(typeof(ResolvedPublication).GetProperties().Any(p => p.Name.Contains("Reference") || p.Name == "Replays"));
            var again = Resolve(catalog); Assert.IsTrue(again.Binding.Same(publication.Binding));
            Assert.AreEqual(references[0].Report.Fingerprint, again.GetDefaultReferences()[0].Report.Fingerprint);
        }
        [UnityTest] public IEnumerator B10_ReferenceLabelsShowOpeningConditionsExactSeedAndCurrentDifferences()
        {
            using (var r = new PlayerBattleRig(false))
            {
                var h = r.Head;
                Is(r.N.Life.Submit(Prepared(r.N.Player.PrepareFormation(new PlayerFormationDraft { ExpectedCommitId = h.Header.CommitId,
                    ExpectedFormationRevision = h.Business.Roster.FormationRevision, Slots = new[] { null, "W", null } }, Codec())), Budget()));
                r.Enter(); yield return r.Ready(); Is(r.Step().Application); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var reference = Read(r); Assert.AreEqual("已验证的开局默认参考", reference.Title);
                StringAssert.Contains("技能", string.Join(";", reference.Conditions)); StringAssert.Contains("偏好", string.Join(";", reference.Conditions));
                StringAssert.Contains("00010203", string.Join(";", reference.Conditions)); StringAssert.Contains("2c2d2e2f", string.Join(";", reference.Conditions));
                Assert.IsTrue(reference.CurrentDifferences.Any(x => x.StartsWith("阵容：")));
                Assert.IsTrue(reference.CurrentDifferences.Any(x => x.StartsWith("局面："))); Assert.IsTrue(reference.CurrentDifferences.Any(x => x.StartsWith("随机：")));
                Assert.AreEqual("当前局面分析未接入", reference.CurrentAnalysis); Assert.IsNull(reference.ReadStep(0).Token);
                Assert.IsNull(reference.ReadStep(0).OriginalCommitId); Assert.AreSame(reference.Steps[0].OrderedFacts, reference.ReadStep(0).OrderedFacts);
                Assert.Throws<NotSupportedException>(() => ((IList<string>)reference.Conditions).Add("fake verified current state"));
                r.Unchanged(head, files, clocks);
            }
        }
        [TestCase("level")] [TestCase("version")] [TestCase("binding")]
        public void B10_ExactReferenceRejectsOtherLevelVersionOrBindingWithoutBusinessWork(string mismatch)
        {
            using (var r = new PlayerBattleRig())
            {
                var context = r.View.Context; var binding = context.Binding; var level = r.State.Baseline.Entry.Level;
                if (mismatch == "binding") binding = TakeCore(ContentBinding.Prepare(binding.PackageId, "foreign", binding.RuleVersion,
                    binding.NumericContractVersion, binding.RandomContractVersion, Codec()));
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var result = r.Session.ReadDefaultReference(mismatch == "level" ? "foreign" : level.LevelId,
                    mismatch == "version" ? "999" : level.LevelVersion, binding, context, Codec());
                Assert.IsFalse(result.IsAvailable); Assert.AreEqual(mismatch == "binding" ? "UnsupportedBinding" : "NoPublishedReference", result.Code);
                r.Unchanged(head, files, clocks);
            }
        }
        [Test] public void B11_OriginalNonkillClearAndKillingLockFactsPlayOnlyInIndependentOverlay()
        {
            using (var r = PlayerBattleRig.DurableReference())
            {
                var reference = Read(r); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var state = r.State; var starts = r.Host.Playback.Starts; var reports = r.Host.Playback.CompletionReports;
                var overlay = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board;
                overlay.Show(r.Host, r.Page.Page, board, reference); var cleared = false; var locked = false;
                for (var i = 0; i < reference.Steps.Count; i++)
                {
                    var record = reference.Steps[i]; Assert.IsTrue(overlay.PlayStep(i), overlay.OverlayReason);
                    CollectionAssert.AreEqual(record.Request.Route, board.ReferenceOverride.TemporaryRoute);
                    foreach (var fact in record.OrderedFacts)
                    {
                        overlay.Advance(CandidateBattlePlaybackController.FactMilliseconds);
                        Assert.AreSame(fact, board.ReferenceOverride.OriginalFact);
                        if (fact.Stage?.Kind == CandidateStageFactKind.TemporaryRouteRemoved)
                        { cleared = true; Assert.IsEmpty(board.ReferenceOverride.TemporaryRoute); }
                        if (fact.Stage?.Kind == CandidateStageFactKind.RouteLocked)
                        { locked = true; Assert.IsTrue(board.ReferenceOverride.LockedRoutes.Any(x => x.PairKey.Equals(fact.Stage.Pair))); }
                    }
                }
                Assert.IsTrue(cleared); Assert.IsTrue(locked); Assert.IsNull(board.PlaybackOverride); Assert.IsNull(r.Host.Input.LastRequest);
                Assert.AreSame(state, r.State); Assert.AreEqual(starts, r.Host.Playback.Starts); Assert.AreEqual(reports, r.Host.Playback.CompletionReports);
                r.Unchanged(head, files, clocks); overlay.Close(); Assert.IsNull(board.ReferenceOverride); r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B11_ActualFirstDragClosesWindowAndOverlayWhileTheSamePointerSubmits()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); r.Host.Input.SelectMember("W");
                var reference = Read(r); var window = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board;
                window.Show(r.Host, r.Page.Page, board, reference); Assert.IsTrue(window.PlayStep(0));
                var route = r.Route(r.State.Enemies[0].PairKey); var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                panel.Pointer(route[0], 0); panel.Pointer(route[1], 1);
                Assert.AreEqual(GestureStage.Dragging, r.Host.Input.Gesture.Stage); Assert.AreEqual(-1, r.Host.Input.Gesture.ActivePointerId);
                Assert.IsFalse(window.IsOpen); Assert.IsNull(board.ReferenceOverride); r.Unchanged(head, files, clocks);
                for (var i = 2; i < route.Count - 1; i++) panel.Pointer(route[i], 1);
                panel.Pointer(route[route.Count - 1], 2); Is(r.Host.Input.LastResult.Application);
                Assert.AreEqual(head.Records.Count + 1, r.Head.Records.Count); Assert.AreEqual(CandidateApplicationKind.Attack, r.Host.Input.LastRequest.Kind);
            }
        }
        [UnityTest] public IEnumerator B11_RealPlaybackBlocksReferenceAndClosingReferenceCannotAcknowledgeItsToken()
        {
            using (var r = new PlayerBattleRig())
            {
                yield return r.Ready();
                var reference = Read(r); Is(r.Step(false).Application); var token = r.N.Battle.QueryView().PresentationToken;
                var window = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board; var reports = r.Host.Playback.CompletionReports;
                var original = r.Host.Playback.Original; window.Show(r.Host, r.Page.Page, board, reference);
                Assert.IsFalse(window.PlayStep(0)); Assert.AreEqual("PresentationPending", window.OverlayReason);
                Assert.IsNull(board.ReferenceOverride); window.Close(); window.Advance(100000);
                Assert.AreSame(token, r.N.Battle.QueryView().PresentationToken); Assert.AreSame(original, r.Host.Playback.Original);
                Assert.AreEqual(reports, r.Host.Playback.CompletionReports);
            }
        }
        [UnityTest] public IEnumerator B11_FaceMismatchKeepsExplanationButCannotOverlayAnotherFace()
        {
            using (var r = PlayerBattleRig.TwoFaces())
            {
                yield return r.Ready();
                var reference = Read(r); var first = r.State.Board.Face.FaceId;
                while (r.State.Board.Face.FaceId == first) Is(r.Step().Application);
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                var window = r.Page.ReferenceView; window.Show(r.Host, r.Page.Page, r.Page.PlaybackView.InputView.Board, reference);
                Assert.IsTrue(window.IsOpen); Assert.IsFalse(window.PlayStep(0)); Assert.AreEqual("FaceGeometryMismatch", window.OverlayReason);
                Assert.IsNull(r.Page.PlaybackView.InputView.Board.ReferenceOverride); StringAssert.Contains("已验证的开局", window.Find<TMPro.TextMeshProUGUI>(FightMatchViewId.Row("reference-explanation")).text);
                r.Unchanged(head, files, clocks);
            }
        }
        [UnityTest] public IEnumerator B11_ClosedReferenceOldStepButtonAndTimerCannotReviveOverlay()
        {
            using (var r = new PlayerBattleRig()) using (var panel = new PlayerBattlePanel(r))
            {
                yield return panel.Ready(); var reference = Read(r); var window = r.Page.ReferenceView; var board = r.Page.PlaybackView.InputView.Board;
                window.Show(r.Host, r.Page.Page, board, reference); var old = window.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("reference-step:" + reference.Steps[0].OperationId)); var oldClose = window.Find<UnityEngine.UI.Button>(FightMatchViewId.Row("reference-close"));
                panel.Keep(old); panel.Keep(oldClose);
                Assert.IsTrue(window.PlayStep(0)); window.Close(); var generation = window.Generation;
                var head = r.Head; var files = r.N.Files(); var clocks = r.ClockReads;
                panel.ClickStale(old); window.Advance(100000); Assert.IsFalse(window.IsOpen); Assert.IsNull(board.ReferenceOverride);
                Assert.AreEqual(generation, window.Generation); r.Unchanged(head, files, clocks);
                window.Show(r.Host, r.Page.Page, board, reference); Assert.IsTrue(window.PlayStep(0));
                generation = window.Generation; var frame = window.Frame; panel.ClickStale(oldClose);
                Assert.IsTrue(window.IsOpen); Assert.AreEqual(generation, window.Generation); Assert.AreSame(frame, window.Frame);
                r.Unchanged(head, files, clocks); window.Close();
            }
        }
    }
}
