using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FightMatch.Application;
using FightMatch.Presentation;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.NavigationAssertions;

namespace FightMatch.Core.Tests
{
    public sealed class PlayerNavigationRecoveryTests
    {
        [Test] public void UGUI_COPY_D01_ColdAdoptUsesExactOriginalKindAndNeverDraftDefault()
        {
            foreach (var kind in new[] { CandidateApplicationKind.SetFormation, CandidateApplicationKind.MigrateRoster,
                CandidateApplicationKind.MigratePermanent, CandidateApplicationKind.PermanentRequest })
            using (var r = new NavigationRig(legacy: kind == CandidateApplicationKind.MigrateRoster))
            {
                FightMatch.Application.PlayerNavigationView preview;
                if (kind == CandidateApplicationKind.SetFormation) preview = r.Formation(null, "W", null);
                else if (kind == CandidateApplicationKind.PermanentRequest)
                {
                    Committed(r.Migrate()); r.Act(PlayerNavigationAction.Return); r.Go(PlayerNavigationTargetKind.Bag); r.SelectWarrior();
                    r.Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Detail, PermanentKind = CandidatePermanentKind.Equip }, r.View.Context, Codec());
                    preview = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Permanent,
                        Permanent = new PlayerPermanentDraft { Kind = CandidatePermanentKind.Equip, CharacterId = "W", Quantity = 0 } }, r.View.Context, Codec());
                }
                else
                {
                    var format = (uint)r.Head.Business.Format;
                    preview = r.Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                        FromFormat = format, ToFormat = format + 1 }, r.View.Context, Codec());
                }
                r.Storage.Fault = "snapshot-promoted";
                var failed = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
                var commit = failed.Read.Application.PendingCommitId; var operation = failed.Read.Application.PendingOperationId;
                r.Rebuild(); Assert.IsNull(r.View.Confirmation);
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    panel.Click(NavigationPanel.Row("save.candidate." + commit));
                    r.Storage.FailMarkerWork = true; panel.Click(NavigationPanel.Row("save.ResumeObserved"));
                    var adopted = controller.View; var value = adopted.Confirmation;
                    Assert.AreEqual(kind, value.OriginalKind); Assert.AreEqual(operation, value.OperationId);
                    Assert.AreEqual(PlayerNavigationDraftKind.Formation, value.Kind);
                    Assert.IsEmpty(value.Slots); Assert.IsNull(value.Quote);
                    Assert.AreEqual(kind == CandidateApplicationKind.PermanentRequest, value.OriginalClearsEquipment);
                    var expected = kind == CandidateApplicationKind.SetFormation ? "fm.operation.set_formation" :
                        kind == CandidateApplicationKind.PermanentRequest ? "fm.operation.clear_equipment" : "fm.profile.data_upgrade.title";
                    var token = adopted.Token; var head = r.Head; var files = r.Files();
                    foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    {
                        panel.Localization.SetLocale(locale);
                        panel.AssertText(NavigationPanel.Row("save.Pending"), "fm.save_recovery.operation_summary",
                            new KeyValuePair<string, string>("operationName", panel.Localization.Resolve(expected, null).Text));
                        panel.AssertNoIdentity(operation, commit); Assert.AreSame(token, controller.View.Token);
                    }
                    // Binding-only adversarial facts do not simulate a business transition or mutate the adopted owner.
                    var mismatch = new PlayerNavigationConfirmation(PlayerNavigationDraftKind.Formation, null, new[] { "W", null, null },
                        0, 0, "different-original", false, CandidateApplicationKind.SetFormation, null, false);
                    panel.View.Render(ReadModel(adopted, confirmation: mismatch));
                    Assert.IsNull(panel.Optional<LocalizedTmpText>(NavigationPanel.Row("save.Pending")));
                    foreach (var mapping in new[] {
                        Tuple.Create((CandidateApplicationKind?)CandidateApplicationKind.Link, (CandidatePermanentKind?)null, false, "fm.operation.link"),
                        Tuple.Create((CandidateApplicationKind?)CandidateApplicationKind.AdvanceRecovery, (CandidatePermanentKind?)null, false, "fm.operation.advance_recovery"),
                        Tuple.Create((CandidateApplicationKind?)CandidateApplicationKind.PermanentRequest, (CandidatePermanentKind?)CandidatePermanentKind.SetPreference, false, "fm.operation.set_item_preference"),
                        Tuple.Create((CandidateApplicationKind?)CandidateApplicationKind.PermanentRequest, (CandidatePermanentKind?)(CandidatePermanentKind)999, false, ""),
                        Tuple.Create((CandidateApplicationKind?)(CandidateApplicationKind)999, (CandidatePermanentKind?)null, false, "") })
                    {
                        var facts = new PlayerNavigationConfirmation(PlayerNavigationDraftKind.Formation, null, null, 0, 0, operation,
                            false, mapping.Item1, mapping.Item2, mapping.Item3);
                        var model = ReadModel(adopted, confirmation: facts);
                        Assert.AreEqual(mapping.Item4, PlayerNavigationRecoveryView.PendingOperationKey(model));
                        panel.View.Render(model);
                        var text = panel.Find<LocalizedTmpText>(NavigationPanel.Row("save.Pending"));
                        if (mapping.Item4.Length == 0) Assert.IsNotNull(text.DiagnosticCode);
                        else panel.AssertText(NavigationPanel.Row("save.Pending"), "fm.save_recovery.operation_summary",
                            new KeyValuePair<string, string>("operationName", panel.Localization.Resolve(mapping.Item4, null).Text));
                    }
                    foreach (var hidden in new[] { CandidatePermanentKind.Equip, CandidatePermanentKind.Craft,
                        CandidatePermanentKind.UseExperienceCards, CandidatePermanentKind.LearnSkill,
                        CandidatePermanentKind.ConfirmTeachingExplanation, CandidatePermanentKind.BeginTeachingGift })
                    {
                        var facts = new PlayerNavigationConfirmation(PlayerNavigationDraftKind.Formation, null, null, 0, 0,
                            operation, false, CandidateApplicationKind.PermanentRequest, hidden, false);
                        panel.View.Render(ReadModel(adopted, confirmation: facts));
                        Assert.IsNull(panel.Optional<LocalizedTmpText>(NavigationPanel.Row("save.Pending")));
                    }
                    Unchanged(r, head, files); Assert.AreSame(adopted, controller.View);
                }
            }
        }
        [Test] public void UGUI_COPY_B06_KnownDiagnosticIsDeduplicatedAndNotificationStaysDistinct()
        {
            using (var r = new NavigationRig())
            {
                r.Formation(null, "W", null); r.Storage.Fault = "snapshot-promoted"; r.Act(PlayerNavigationAction.Confirm);
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    var basis = controller.View; var head = r.Head; var files = r.Files();
                    foreach (var pair in new[] { Tuple.Create("SaveFailed", "fm.save_recovery.failed"),
                        Tuple.Create("CommitUnknown", "fm.save_recovery.unknown"), Tuple.Create("CreationPending", "fm.profile.creation_pending.body"),
                        Tuple.Create("SettlementRequired", "fm.victory.pending.title"), Tuple.Create("ResolutionRequired", "fm.save_recovery.blocking_notice") })
                    {
                        var diagnostic = new CandidateApplicationDiagnostic(pair.Item1, "Navigation");
                        var phase = pair.Item1 == "SaveFailed" ? CandidateApplicationPhase.SaveFailed :
                            pair.Item1 == "CommitUnknown" ? CandidateApplicationPhase.CommitUnknown : CandidateApplicationPhase.Ready;
                        panel.View.Render(ReadModel(basis, read: ReadState(basis.Read, phase, diagnostic), status: pair.Item1, diagnostic: diagnostic));
                        panel.AssertVisible(pair.Item2, NavigationBindings.ReasonArguments(pair.Item2, pair.Item1));
                        Assert.IsNull(panel.Optional<LocalizedTmpText>(NavigationPanel.Row("save.Diagnostic")));
                        Assert.IsTrue(panel.Find<UnityEngine.UI.Button>(NavigationPanel.Row("save.Retry")).interactable);
                    }
                    Unchanged(r, head, files);
                    r.OnPublish = _ => throw new InvalidOperationException("private notification exception");
                    controller.Refresh(); panel.Click(NavigationPanel.Row("save.Retry"));
                    Committed(controller.View); Assert.IsNotNull(controller.View.Result.NotificationFailure);
                    panel.AssertVisible("fm.save_result.title");
                    panel.AssertVisible("fm.save_result.confirmed", new KeyValuePair<string, string>("operationName",
                        panel.Localization.Resolve("fm.operation.set_formation", null).Text));
                    panel.AssertVisible("fm.save_result.notification_failed", new KeyValuePair<string, string>("errorCode", "NOTIFICATION_FAILED"));
                    Assert.AreEqual(0, panel.CountKey("fm.save_recovery.failed"));
                    Assert.AreEqual(0, panel.CountKey("fm.common.business_attention"));
                    panel.AssertNoIdentity("private notification exception", "System.InvalidOperationException", "NotificationFailure");
                    head = r.Head; files = r.Files(); var token = controller.View.Token;
                    panel.Localization.SetLocale(LocaleId.ZhHans);
                    Assert.AreSame(token, controller.View.Token); Assert.AreEqual(1, panel.CountKey("fm.save_result.notification_failed"));
                    Unchanged(r, head, files);
                }
            }
        }
        [TestCase("snapshot-before", "SaveFailed")]
        [TestCase("snapshot-promoted", "SaveFailed")]
        [TestCase("marker-before", "CommitUnknown")]
        [TestCase("marker-after", "CommitUnknown")]
        public void CC14_CC15_WriteFaultKeepsTheOriginalCandidateAndConfirmation(string fault, string code)
        {
            using (var r = new NavigationRig())
            {
                var before = r.Head; var preview = r.Formation(null, before.Business.Character.CharacterId, null);
                r.Storage.Fault = fault;
                var failed = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
                Assert.AreEqual(code, failed.Result.Code); Assert.AreSame(before, r.Head);
                var commit = failed.Read.Application.PendingCommitId; var op = failed.Read.Application.PendingOperationId;
                var bytes = PlayerRosterSessionTests.CandidateBytes(r.Storage.Inner, commit);
                var calls = r.Storage.Inner.SnapshotCreates; var files = r.Files();
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    var token = controller.View.Token;
                    foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    {
                        panel.Localization.SetLocale(locale);
                        panel.AssertText(NavigationPanel.Row("save.Pending"), "fm.save_recovery.operation_summary",
                            new KeyValuePair<string, string>("operationName", panel.Localization.Resolve("fm.operation.set_formation", null).Text));
                        Assert.AreSame(token, controller.View.Token);
                        Assert.AreEqual(op, controller.View.Confirmation.OperationId);
                        var text = panel.Find<LocalizedTmpText>(NavigationPanel.Row("save.Pending")).Target.text;
                        StringAssert.DoesNotContain(op, text);
                        if (commit != null) StringAssert.DoesNotContain(commit, text);
                    }
                    SameFiles(files, r.Storage.Inner.Files);
                }
                r.Act(PlayerNavigationAction.Cancel); SameFiles(files, r.Storage.Inner.Files);
                Assert.AreEqual(op, r.View.Confirmation.OperationId);
                var repeated = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
                Assert.AreEqual(code, repeated.Result.Code); SameFiles(files, r.Storage.Inner.Files);
                var done = r.Act(PlayerNavigationAction.Resolve);
                if (done.Route != PlayerNavigationRoute.CommittedResult) done = r.Act(PlayerNavigationAction.Retry);
                Committed(done); Assert.AreEqual(commit, done.Result.OriginalCommitId);
                Assert.AreEqual(op, done.Result.OriginalLookup.Record.OperationId);
                Assert.AreEqual(before.Records.Count + 1, r.Head.Records.Count);
                if (bytes != null) CollectionAssert.AreEqual(bytes, r.Storage.Inner.Files["c-" + commit + ".snapshot"]);
                Assert.LessOrEqual(r.Storage.Inner.SnapshotCreates, calls + (fault == "snapshot-before" ? 1 : 0));
            }
        }
        [Test] public void CC14_PrepareFailureWithoutTicketStillRetriesTheRetainedRequest()
        {
            using (var r = new NavigationRig())
            {
                var preview = r.Formation(null, r.Head.Business.Character.CharacterId, null);
                r.Storage.FailPrepare = true;
                var failed = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
                Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, failed.Read.Application.Phase);
                Assert.IsNull(failed.Read.Application.PendingCommitId);
                var operation = failed.Confirmation.OperationId;
                Assert.AreEqual("RetryRequired", failed.ReasonFor(PlayerNavigationAction.Resolve));
                var done = r.Act(PlayerNavigationAction.Retry); Committed(done);
                Assert.AreEqual(operation, done.Result.OriginalLookup.Record.OperationId);
            }
        }
        [TestCase("save-failed", "Retry")]
        [TestCase("unknown-before", "Resolve")]
        [TestCase("unknown-after", "Resolve")]
        [TestCase("save-failed", "End")]
        public void CC17_RCV1_RebuildDiscardsAllRequestsThenRecoversOriginalIntentThroughProductionQuery(string fault, string finish)
        {
            using (var r = new NavigationRig())
            {
                // No request/intent reference escapes this helper. Only disk identity and immutable bytes survive rebuilding.
                var original = PersistUnfinished(r);
                var oldCount = r.Head.Records.Count; var calls = r.Storage.Inner.SnapshotCreates;
                var opened = r.Rebuild(); CollectionAssert.Contains(opened.View.ObservedCandidateCommitIds, original.Item1);
                Assert.IsNull(r.View.Result); Assert.AreEqual("OriginalOwnerRequired", r.View.ReasonFor(PlayerNavigationAction.Retry));
                using (var controller = new PlayerNavigationController(r.Player, Budget()))
                using (var panel = new NavigationPanel(controller))
                {
                    var candidates = controller.View.Read.Application.ObservedCandidateCommitIds.OrderBy(x => x, StringComparer.Ordinal).ToArray();
                    var token = controller.View.Token;
                    foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    {
                        panel.Localization.SetLocale(locale);
                        for (var i = 0; i < candidates.Length; i++)
                            panel.AssertCaption(NavigationPanel.Row("save.candidate." + candidates[i]), "fm.save_recovery.candidate_row",
                                new KeyValuePair<string, string>("candidateNumber", (i + 1).ToString(CultureInfo.InvariantCulture)));
                        Assert.AreSame(token, controller.View.Token);
                    }
                    panel.Click(NavigationPanel.Row("save.candidate." + original.Item1));
                    Assert.AreEqual(original.Item1, controller.View.SelectedCommitId);
                }
                if (fault == "save-failed") r.Storage.FailMarkerWork = true;
                else r.Storage.Fault = fault == "unknown-before" ? "marker-before" : "marker-after";
                var resumed = r.Act(PlayerNavigationAction.ResumeObserved);
                Assert.AreEqual(fault == "save-failed" ? "SaveFailed" : "CommitUnknown", resumed.Result.Code);
                var captured = r.Player.ReadNavigationResumedIntent(original.Item1, original.Item2, out var intent);
                Assert.IsNotNull(intent, captured.Code);
                CollectionAssert.AreEqual(original.Item3, intent.CanonicalBytes);
                r.Player.ReadNavigationResumedIntent(original.Item1, original.Item2, out var second);
                Assert.AreSame(intent, second);
                Assert.AreEqual(original.Item2, resumed.Confirmation.OperationId);
                if (finish == "End")
                {
                    r.Go(PlayerNavigationTargetKind.EndConfirmation); var ended = r.Act(PlayerNavigationAction.End);
                    Assert.AreEqual(oldCount, r.Head.Records.Count); Assert.IsNull(ended.Read.Application.PendingOperationId);
                }
                else
                {
                    var done = r.Act(finish == "Retry" ? PlayerNavigationAction.Retry : PlayerNavigationAction.Resolve);
                    if (done.Route != PlayerNavigationRoute.CommittedResult) done = r.Act(PlayerNavigationAction.Retry);
                    Committed(done); Assert.AreEqual(original.Item1, done.Result.OriginalCommitId);
                    CollectionAssert.AreEqual(original.Item3, done.Result.OriginalLookup.Record.Intent.CanonicalBytes);
                    Assert.AreEqual(oldCount + 1, r.Head.Records.Count);
                    CollectionAssert.AreEqual(original.Item4, r.Storage.Inner.Files["c-" + original.Item1 + ".snapshot"]);
                    Assert.AreEqual(PlayerNavigationRoute.MapAdventure, r.Act(PlayerNavigationAction.Return).Route);
                }
                Assert.AreEqual(calls, r.Storage.Inner.SnapshotCreates);
            }
        }
        private static Tuple<string, string, byte[], byte[]> PersistUnfinished(NavigationRig r)
        {
            var preview = r.Formation(null, r.Head.Business.Character.CharacterId, null);
            r.Storage.Fault = "snapshot-promoted";
            var failed = r.Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
            var commit = failed.Read.Application.PendingCommitId;
            var candidate = TakeCore(CandidateApplicationSaveCodec.DecodePublished(Envelope(r.Storage.Inner, commit), Closure(r.Publication), Codec()));
            return Tuple.Create(commit, failed.Read.Application.PendingOperationId, candidate.Records.Last().Intent.CanonicalBytes.ToArray(),
                PlayerRosterSessionTests.CandidateBytes(r.Storage.Inner, commit));
        }
        [Test] public void RCV1_ReadonlyIntentQueryRejectsWrongIdentityThreadDisposedBusyAndF2()
        {
            using (var r = new NavigationRig())
            {
                var original = PersistUnfinished(r); r.Rebuild();
                r.Go(PlayerNavigationTargetKind.ObservedCandidate, commit: original.Item1); r.Storage.FailMarkerWork = true;
                r.Act(PlayerNavigationAction.ResumeObserved); var files = r.Files();
                var other = new PlayerSessionSystem(r.Runtime, r.App, r.Life);
                Assert.AreEqual("InconsistentBinding", r.Runtime.QueryResumedIntent(other, original.Item1, original.Item2, out var wrong).Code);
                Assert.IsNull(wrong);
                Assert.AreEqual("StaleContext", r.Player.ReadNavigationResumedIntent("wrong", original.Item2, out wrong).Code);
                Assert.IsNull(wrong);
                Assert.AreEqual("StaleContext", r.Player.ReadNavigationResumedIntent(original.Item1, "wrong", out wrong).Code);
                Assert.AreEqual("WrongThread", Task.Run(() => r.Player.ReadNavigationResumedIntent(original.Item1, original.Item2, out var ignored).Code).GetAwaiter().GetResult());
                SameFiles(files, r.Storage.Inner.Files);
                string busy = null;
                r.Storage.DuringWrite = () => busy = r.Player.ReadNavigationResumedIntent(original.Item1, original.Item2, out var ignored).Code;
                Committed(r.Act(PlayerNavigationAction.Retry)); Assert.AreEqual("Busy", busy);
                r.Runtime.Close();
                Assert.AreEqual("Disposed", r.Player.ReadNavigationResumedIntent(original.Item1, original.Item2, out wrong).Code);
            }
            using (var r = new NavigationRig(initialize: false))
            {
                r.Storage.Fault = "snapshot-promoted";
                r.Player.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget());
                Assert.AreEqual("CreationPending", r.Player.ReadNavigationResumedIntent(r.App.QueryView().View.PendingCommitId,
                    r.Profile.OperationId, out var forbidden).Code); Assert.IsNull(forbidden);
            }
        }
        [Test] public void CC18_OriginalResultSelectionSurvivesLaterHeadAndDoesNotResubmit()
        {
            using (var r = new NavigationRig())
            {
                r.Formation(null, r.Head.Business.Character.CharacterId, null); var first = r.Act(PlayerNavigationAction.Confirm);
                var op = first.Result.OriginalLookup.Record.OperationId; var commit = first.Result.OriginalCommitId;
                r.Act(PlayerNavigationAction.Return); r.Formation(null, null, r.Head.Business.Character.CharacterId);
                Committed(r.Act(PlayerNavigationAction.Confirm)); r.Act(PlayerNavigationAction.Return);
                var current = r.Head; var files = r.Files();
                r.Go(PlayerNavigationTargetKind.OriginalOperation, operation: op);
                var selected = r.Act(PlayerNavigationAction.SelectOriginalOperation); Committed(selected);
                Assert.AreEqual(commit, selected.Result.OriginalCommitId); Assert.AreEqual(current.Header.CommitId, selected.Result.LookupViewCommitId);
                Assert.AreNotEqual(commit, current.Header.CommitId); Unchanged(r, current, files);
            }
        }
        [Test] public void CC19_EndRequiresSeparateConfirmationAndCancelRetainsOriginalPending()
        {
            using (var r = new NavigationRig())
            {
                var original = PersistUnfinished(r); var files = r.Files();
                Assert.AreEqual("EndConfirmationRequired", r.Act(PlayerNavigationAction.End).Status);
                r.Go(PlayerNavigationTargetKind.EndConfirmation); r.Act(PlayerNavigationAction.Cancel);
                Assert.AreEqual(original.Item2, r.View.Read.Application.PendingOperationId); SameFiles(files, r.Storage.Inner.Files);
                r.Go(PlayerNavigationTargetKind.EndConfirmation); var ended = r.Act(PlayerNavigationAction.End);
                Assert.IsNull(ended.Read.Application.PendingOperationId); Assert.IsNull(ended.Result);
                Assert.IsFalse(r.Head.Records.Any(x => x.OperationId == original.Item2));
            }
        }
        [Test] public void CC19_EndedRequestStaysUnavailableUntilExplicitRestoreSucceeds()
        {
            using (var r = new NavigationRig())
            {
                var head = r.Head; var files = r.Files();
                r.Formation(null, head.Business.Character.CharacterId, null); r.Storage.FailPrepare = true;
                var failed = r.Act(PlayerNavigationAction.Confirm);
                Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, failed.Read.Application.Phase);
                Assert.IsNull(failed.Read.Application.PendingCommitId); var operation = failed.Confirmation.OperationId;
                r.Go(PlayerNavigationTargetKind.EndConfirmation); r.Storage.FailRead = true;
                var ended = r.Act(PlayerNavigationAction.End);
                Assert.IsNull(ended.Read.Application.PendingOperationId); Assert.IsNull(ended.Confirmation);
                Assert.AreEqual(PlayerNavigationRoute.Recovery, ended.Route); Assert.IsFalse(ended.Read.IsAvailable);
                Assert.IsNotNull(ended.Diagnostic); SameFiles(files, r.Storage.Inner.Files);
                var blocked = r.Formation(null, null, head.Business.Character.CharacterId);
                Assert.IsNotNull(blocked.ReasonFor(PlayerNavigationAction.Confirm)); Assert.IsNull(blocked.Confirmation);
                r.Storage.FailRead = false; var restored = r.Act(PlayerNavigationAction.Refresh);
                Assert.IsTrue(restored.Read.IsAvailable, restored.Read.Code);
                Assert.AreEqual(head.Header.CommitId, r.Head.Header.CommitId);
                Assert.IsFalse(r.Head.Records.Any(x => x.OperationId == operation)); SameFiles(files, r.Storage.Inner.Files);
            }
        }
        [Test] public void CC19_OrdinaryQueryDoesNotAdoptAnotherOwnersPending()
        {
            using (var r = new NavigationRig())
            {
                var request = Prepared(r.Player.PrepareFormation(new PlayerFormationDraft { ExpectedCommitId = r.Head.Header.CommitId,
                    ExpectedFormationRevision = r.Head.Business.Roster.FormationRevision, Slots = new[] { null, r.Head.Business.Character.CharacterId, null } }, Codec()));
                r.Storage.Fault = "snapshot-promoted"; r.Life.Submit(request, Budget()); var files = r.Files();
                Assert.AreEqual("OriginalOwnerRequired", r.View.ReasonFor(PlayerNavigationAction.Retry));
                Assert.AreEqual("OriginalOwnerRequired", r.Go(PlayerNavigationTargetKind.EndConfirmation).Status);
                Assert.IsNull(r.View.Confirmation); SameFiles(files, r.Storage.Inner.Files);
            }
        }
        [Test] public void CC25_NotificationFailureRemainsACommittedResult()
        {
            using (var r = new NavigationRig())
            {
                r.OnPublish = _ => throw new InvalidOperationException("isolated notification failure");
                using (var oldController = new FightMatch.Presentation.PlayerNavigationController(r.Player, Budget()))
                using (var oldView = new NavigationPanel(oldController)) { }
                r.Formation(null, r.Head.Business.Character.CharacterId, null); var done = r.Act(PlayerNavigationAction.Confirm);
                Committed(done); Assert.IsNotNull(done.Result.NotificationFailure);
                var head = r.Head; var files = r.Files();
                Assert.AreEqual("ResultAvailable", done.ReasonFor(PlayerNavigationAction.Retry));
                using (var controller = new FightMatch.Presentation.PlayerNavigationController(r.Player, Budget()))
                using (var view = new NavigationPanel(controller))
                {
                    Committed(controller.View); Assert.AreEqual(done.Result.OriginalCommitId, controller.View.Result.OriginalCommitId);
                    Assert.AreSame(done.Result.NotificationFailure, controller.View.Result.NotificationFailure);
                    controller.ActionHandler(PlayerNavigationAction.Retry)(); Unchanged(r, head, files);
                }
            }
        }
    }
}
