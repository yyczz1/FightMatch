using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.BattleApplicationRig;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateBattleApplicationTests
    {
        [Test]
        public void B15_02_RegisteredFacadeCommitsCanonicalAttackAndProjectsOneImmutableRoot()
        {
            using (var r = new BattleApplicationRig())
            {
                Assert.AreSame(r.System, r.Runtime.Architecture.GetSystem<CandidateBattleApplicationSystem>());
                var before = r.Head;
                var request = r.Freeze(r.AttackDraft());
                var budget = B();
                var result = Is(r.System.Submit(request, budget), "Completed");
                Assert.Greater(budget.Codec.Math.PrimitiveStepsUsed, 0);
                Assert.IsTrue(result.Application.IsCommitted);
                Assert.AreEqual(result.Application.OriginalCommitId, result.View.CommitId);
                Assert.AreSame(r.Head, result.View.PublishedSnapshot);
                Assert.AreSame(r.Head.Business.ActiveHistory, result.View.History);
                Assert.AreSame(r.State, result.View.BattleSnapshot);
                Assert.AreSame(r.Head.Business.Inventory, result.View.Inventory);
                Assert.AreSame(r.State.Baseline.Entry.Context, result.View.Context);
                Assert.AreEqual(before.Header.SaveGeneration + 1, r.Head.Header.SaveGeneration);
                SameOwners(before.Business, r.Head.Business);
                CollectionAssert.AreEqual(request.Intent.CanonicalBytes, r.Head.Records.Last().Intent.CanonicalBytes);
                Assert.AreEqual(request.OperationId, r.Head.Business.ActiveHistory.Archive.Last().OperationId);
                Assert.AreEqual(r.Head.Records.Last().Result.HistoryAnchorId, r.Head.Business.ActiveHistory.Archive.Last().HistoryAnchorId);
                var head = r.Head;
                var disk = r.Runtime.Disk();
                var calls = r.Runtime.Storage.Base.Calls;
                for (var i = 0; i < 3; i++)
                {
                    Assert.AreSame(head, r.System.QueryView().PublishedSnapshot);
                    Assert.AreNotEqual(CandidatePresentationDisposition.PlayOriginal, r.System.QueryOperation(request, B()).PresentationDisposition);
                }
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                r.Runtime.SameDisk(disk);
                Assert.AreEqual(SavePurpose.CandidateValidation, result.View.Purpose);
                Assert.IsFalse(result.View.CommitEligible);
                Assert.IsNull(result.View.ItemUseEnabled);
                Assert.IsFalse(result.View.PreferenceApplicable);
                Assert.AreEqual(false, result.View.RequiredAttackItemUseEnabled);
                Assert.AreEqual("NotImplemented", result.View.PreferenceWriting.Reason);
                Assert.AreEqual("NotImplemented", result.View.RewardDestination.Reason);
                Assert.AreEqual("NotImplemented", result.View.OnlineAndAds.Reason);
                Assert.Throws<NotSupportedException>(() => ((IList<CandidateHistoryEntry>)result.View.History.Archive).Clear());
                Assert.Throws<NotSupportedException>(() => ((IList<CandidateBattleOrderedFact>)result.Presentation.OrderedFacts).Clear());
            }
        }

        [Test]
        public void B15_02_NoBattleIsDistinctFromUnconfiguredAndRestoreBlocked()
        {
            using (var r = new BattleApplicationRig(false))
            {
                Assert.AreEqual("Unconfigured", r.System.QueryView().Code);
                ApplicationRuntimeRig.Is(r.Runtime.Open(), "InitializationReady");
                ApplicationRuntimeRig.Is(r.Runtime.Initialize(), "Completed");
                var view = r.System.QueryView();
                Assert.AreEqual("NoActiveBattle", view.Code);
                Assert.IsNull(view.BattleSnapshot);
                Assert.IsNull(view.History);
                Assert.IsNull(view.FinalReport);
                Assert.AreEqual("NoActiveBattle", view.Attack.Reason);
                Assert.IsNotNull(view.Inventory);
                r.Runtime.Storage.BeforeOpen = _ => { throw new IOException("read blocked"); };
                r.Runtime.Restore();
                Assert.AreEqual(CandidateApplicationPhase.RecoveryBlocked, r.System.QueryView().Phase);
                Assert.IsFalse(r.System.QueryView().Attack.IsAvailable);
                r.Runtime.Storage.BeforeOpen = null;
            }
        }

        [Test]
        public void B15_03_InvalidGeometryPreservesOriginalStageReasonAndCellIndex()
        {
            using (var r = new BattleApplicationRig())
            {
                var draft = r.AttackDraft();
                ((IList<FlowPos>)draft.Attack.Route)[1] = new FlowPos(100, 100);
                var request = r.Freeze(draft);
                var a = draft.Attack;
                var expected = CandidateBattleOperations.EvaluateAttack(r.Head.Business.ActiveHistory.CurrentRun,
                    new CandidateAttackRequest
                    {
                        PlayerId = draft.PlayerId, AttemptId = a.AttemptId, OperationId = request.OperationId,
                        ExpectedSceneRevision = a.ExpectedSceneRevision, Actor = a.Actor, Pair = a.Pair, Route = a.Route.ToList()
                    }, new CandidateBattleConditions { PreferenceRevision = a.ExpectedPreferenceRevision, ItemUseEnabled = false },
                    100, new RandomSamplingBudget(Math()));
                Assert.IsFalse(expected.IsAccepted);
                var before = r.Head;
                var disk = r.Runtime.Disk();
                var rejected = r.System.Submit(request, B());
                Assert.AreEqual("BuilderRejected", rejected.Application.Code);
                Assert.AreEqual(expected.RejectionCode, rejected.Code);
                Assert.AreEqual(expected.FieldPath, rejected.DomainRejection.FieldPath);
                Assert.AreEqual(expected.RejectionStage, rejected.DomainRejection.RejectionStage);
                Assert.AreEqual(expected.RouteReasonCode, rejected.DomainRejection.RouteReasonCode);
                Assert.AreEqual(expected.RouteCellIndex, rejected.DomainRejection.RouteCellIndex);
                Assert.AreSame(before, r.Head);
                r.Runtime.SameDisk(disk);
            }
        }

        [TestCase("scene", "StaleContext")]
        [TestCase("preference", "StaleContext")]
        [TestCase("enabled", "InconsistentBinding")]
        [TestCase("context-id", "InconsistentBinding")]
        [TestCase("context-revision", "InconsistentBinding")]
        [TestCase("context-content", "InconsistentBinding")]
        [TestCase("context-rule", "InconsistentBinding")]
        [TestCase("context-math", "InconsistentBinding")]
        [TestCase("context-random", "InconsistentBinding")]
        [TestCase("context-notes", "InconsistentBinding")]
        public void B15_03_StaleOrDifferentBindingsCannotPublish(string change, string code)
        {
            using (var r = new BattleApplicationRig())
            {
                var d = r.AttackDraft();
                switch (change)
                {
                    case "scene": d.Attack.ExpectedSceneRevision++; break;
                    case "preference": d.Attack.ExpectedPreferenceRevision++; break;
                    case "enabled": d.Attack.ItemUseEnabled = true; break;
                    case "context-id": d.Context.DraftId += "x"; break;
                    case "context-revision": d.Context.DraftRevision++; break;
                    case "context-content": d.Context.ContentFingerprint += "x"; break;
                    case "context-rule": d.Context.RuleVersion += "x"; break;
                    case "context-math": d.Context.NumericContractVersion += "x"; break;
                    case "context-random": d.Context.RandomContractVersion += "x"; break;
                    case "context-notes": d.Context.SourceNotes[0] += "x"; break;
                }
                var before = r.Head;
                var writes = r.Runtime.Storage.Base.WriteCalls;
                var result = Is(r.System.Submit(r.Freeze(d), B()), code);
                Assert.IsNotNull(result.DomainRejection);
                Assert.AreSame(before, r.Head);
                Assert.AreEqual(writes, r.Runtime.Storage.Base.WriteCalls);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void B15_03_UnavailableTargetAndDownedActorKeepDomainRefusal(bool downed)
        {
            using (var r = new BattleApplicationRig(hp: downed ? 1 : 100, level: downed ? 3 : 1))
            {
                r.Attack();
                if (downed) Assert.IsTrue(r.State.Members[0].Hp.Numerator.IsZero);
                else Assert.IsTrue(r.State.Enemies[0].Hp.Numerator.IsZero);
                var before = r.Head;
                var result = r.System.Submit(r.Freeze(r.AttackDraft()), B());
                Assert.AreEqual("BuilderRejected", result.Application.Code);
                Assert.IsFalse(result.Application.IsCommitted);
                Assert.IsNotNull(result.DomainRejection.RejectionStage);
                Assert.AreNotEqual("SaveFailed", result.Code);
                Assert.AreSame(before, r.Head);
            }
        }

        [TestCase("steps")]
        [TestCase("integer")]
        [TestCase("random")]
        public void B15_03_SharedMathAndRandomLimitsDoNotLeavePartialCandidates(string kind)
        {
            using (var r = new BattleApplicationRig())
            {
                var request = r.Freeze(r.AttackDraft());
                var budget = kind == "steps" ? new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0))) :
                    kind == "integer" ? new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(32))) : B();
                var before = r.Head;
                var calls = r.Runtime.Storage.Base.Calls;
                var result = Is(r.System.Submit(request, budget, kind == "random" ? 0 : 4096), "Limit");
                Assert.AreEqual(kind == "steps" ? "PrimitiveSteps" : kind == "integer" ? "IntegerBits" : "RandomWords",
                    result.Application.Diagnostic.LimitReason);
                Assert.Greater(result.Application.Diagnostic.RequiredAtLeast, result.Application.Diagnostic.Allowed);
                if (kind != "steps")
                {
                    Assert.AreEqual(result.Application.Diagnostic.LimitReason, result.DomainRejection.LimitReason);
                    Assert.AreEqual(result.Application.Diagnostic.RequiredAtLeast, result.DomainRejection.RequiredAtLeast);
                    Assert.AreEqual(result.Application.Diagnostic.Allowed, result.DomainRejection.Allowed);
                }
                Assert.AreSame(before, r.Head);
                Assert.IsNull(r.Runtime.Model.View.PendingOperationId);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Assert.Throws<ArgumentOutOfRangeException>(() => r.System.Submit(request, B(), -1));
                Is(r.System.Submit(request, B()), "Completed");
            }
        }

        [Test]
        public void B15_04_OldResultsWinOverLaterSceneLatchOtherPendingAndOriginalConflict()
        {
            using (var r = new BattleApplicationRig())
            {
                var oldDraft = r.AttackDraft();
                var old = r.Freeze(oldDraft);
                var first = Is(r.System.Submit(old, B()), "Completed");
                var originalCommit = first.Application.OriginalCommitId;
                var repeated = Is(r.System.Submit(old, B()), "Completed");
                Assert.AreEqual(originalCommit, repeated.Application.OriginalCommitId);
                Assert.AreSame(first.View.PresentationToken, repeated.View.PresentationToken);
                Is(r.System.ReportPresentationCompleted(first.Presentation.Token), "PresentationCompleted");
                var next = r.Freeze(r.AttackDraft(1));
                r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.System.Submit(next, B()), "SaveFailed");
                var pendingCommit = r.Runtime.Model.View.PendingCommitId;
                var calls = r.Runtime.Storage.Base.Calls;
                Is(r.System.Submit(old, B()), "Completed");
                Assert.AreEqual(pendingCommit, r.Runtime.Model.View.PendingCommitId);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Is(r.System.Retry(next, B()), "Completed");
                var latest = r.Head.Header.CommitId;
                var oldAgain = Is(r.System.Submit(old, B()), "Completed");
                Assert.AreEqual(originalCommit, oldAgain.Application.OriginalCommitId);
                Assert.AreEqual(latest, oldAgain.Application.LookupViewCommitId);
                Assert.AreEqual(latest, oldAgain.View.CommitId);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, oldAgain.PresentationDisposition);
                var conflict = new CandidateApplicationIntentInput
                {
                    PlayerId = oldDraft.PlayerId, ExpectedCommitId = oldDraft.ExpectedCommitId, Context = oldDraft.Context,
                    Kind = oldDraft.Kind, OperationId = old.OperationId, Attack = oldDraft.Attack
                };
                conflict.Attack.ItemUseEnabled = true;
                var conflicting = Accept(CandidateApplicationProtocol.PrepareIntent(conflict, B().Codec));
                ApplicationRuntimeRig.Is(r.Runtime.System.Submit(conflicting, null, B()), "OperationConflict");
            }
        }

        [TestCase("Snapshot.Flush.after", "SaveFailed", false)]
        [TestCase("Marker.Promote.after", "CommitUnknown", false)]
        [TestCase("Marker.Promote.after", "CommitUnknown", true)]
        public void B15_05_RealRetryOrResolutionUsesOriginalBytesAndDeliversOnlyOnce(string fault, string code, bool retry)
        {
            using (var r = new BattleApplicationRig())
            {
                var before = r.Head;
                var request = r.Freeze(r.AttackDraft());
                r.Runtime.Storage.Base.Arm(fault, null);
                var failed = Is(r.System.Submit(request, B()), code);
                Assert.IsTrue(r.Runtime.Storage.Base.FaultUsed);
                Assert.AreSame(before, failed.View.PublishedSnapshot);
                Assert.IsFalse(failed.View.IsPublishedHeadVerified);
                Assert.IsNull(failed.Presentation);
                Assert.AreEqual(CandidatePresentationDisposition.None, failed.PresentationDisposition);
                Assert.IsNull(failed.View.PresentationToken);
                var commit = failed.View.ApplicationView.PendingCommitId;
                var files = r.Runtime.Disk();
                var originalBytes = files[code == "SaveFailed" ? "w-" + commit + ".snapshot.tmp" : SnapshotName(commit)];
                var calls = r.Runtime.Storage.Base.Calls;
                Is(r.System.Submit(request, B(), 0), code);
                Is(r.System.QueryOperation(request, B()), code);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                var completed = code == "SaveFailed" || retry ? r.System.Retry(request, B()) : r.System.Resolve(request, B());
                Is(completed, "Completed");
                Assert.AreEqual(commit, completed.Application.OriginalCommitId);
                Assert.AreEqual(CandidatePresentationDisposition.PlayOriginal, completed.PresentationDisposition);
                CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(r.Runtime.Path(SnapshotName(commit))));
                Assert.AreEqual(1, r.Runtime.Storage.Base.RealMarkerPromotionsFor(commit));
                Assert.AreEqual(before.Records.Count + 1, r.Head.Records.Count);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, r.System.Retry(request, B()).PresentationDisposition);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, r.System.Resolve(request, B()).PresentationDisposition);
            }
        }

        [Test]
        public void B15_05_CommittedButLoadBlockedRetainsCommitAndLaterOriginalFacts()
        {
            using (var r = new BattleApplicationRig())
            {
                var before = r.Head;
                var request = r.Freeze(r.AttackDraft());
                r.Runtime.Storage.PublishedCommit = null;
                var fired = false;
                r.Runtime.Storage.AfterMarkerEnumeration = n =>
                {
                    if (n != 4) return;
                    fired = true;
                    throw new IOException("019 post-commit load");
                };
                var failed = Is(r.System.Submit(request, B()), "CommittedRestoreRequired");
                Assert.IsTrue(fired);
                Assert.IsTrue(failed.Application.IsCommitted);
                Assert.AreEqual(CandidateApplicationPhase.RestoreRequired, failed.View.Phase);
                Assert.IsNotNull(failed.Application.Diagnostic);
                Assert.AreSame(before, failed.View.PublishedSnapshot);
                Assert.IsNull(failed.Presentation);
                var commit = failed.Application.OriginalCommitId;
                var bytes = File.ReadAllBytes(r.Runtime.Path(SnapshotName(commit)));
                r.Runtime.Storage.AfterMarkerEnumeration = null;
                var restored = Is(r.System.Resolve(request, B()), "Completed");
                Assert.AreEqual(commit, restored.Application.OriginalCommitId);
                Assert.AreEqual(CandidatePresentationDisposition.PlayOriginal, restored.PresentationDisposition);
                CollectionAssert.AreEqual(bytes, File.ReadAllBytes(r.Runtime.Path(SnapshotName(commit))));
                Assert.AreEqual(1, r.Runtime.Storage.Base.RealMarkerPromotionsFor(commit));
            }
        }

        [Test]
        public void B15_05_ExplicitEndDiscardsFailedCandidateWithoutPublishing()
        {
            using (var r = new BattleApplicationRig())
            {
                var before = r.Head;
                var request = r.Freeze(r.AttackDraft());
                r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.System.Submit(request, B()), "SaveFailed");
                Is(r.System.Resolve(request, B()), "ConfirmedNotCommitted");
                Is(r.System.End(request, B()), "Ended");
                Assert.AreEqual(before.Header.CommitId, r.Head.Header.CommitId);
                Assert.IsNull(r.System.QueryView().PresentationToken);
                Assert.IsNull(r.Runtime.Model.View.PendingOperationId);
                Is(r.System.QueryOperation(request, B()), "NotFound");
            }
        }

        [TestCase("valid")]
        [TestCase("missing")]
        [TestCase("extra")]
        [TestCase("order")]
        [TestCase("target")]
        [TestCase("scene")]
        public void B15_06_PreviewAndExactOrderedConfirmationUseRealHistory(string change)
        {
            using (var r = new BattleApplicationRig(level: 3))
            {
                var firstRequest = r.Freeze(r.AttackDraft());
                var first = Is(r.System.Submit(firstRequest, B()), "Completed");
                Is(r.System.ReportPresentationCompleted(first.Presentation.Token), "PresentationCompleted");
                r.Attack(1);
                var preview = r.Preview();
                Assert.AreEqual(first.Application.OriginalLookup.Record.OperationId, preview.Range.OperationId);
                var range = preview.Range;
                Assert.AreEqual(2, range.Entries.Count);
                var draft = r.RollbackDraft(range);
                var ids = (IList<string>)draft.Rollback.ConfirmedRemovedOperationIds;
                switch (change)
                {
                    case "missing": ids.RemoveAt(1); break;
                    case "extra": ids.Add("not-confirmed"); break;
                    case "order": draft.Rollback.ConfirmedRemovedOperationIds = ids.Reverse().ToList(); break;
                    case "target": draft.Rollback.TargetOperationId = ids[1]; break;
                }
                var request = r.Freeze(draft);
                ids.Clear();
                draft.Rollback.HistoryAnchorId = "mutated-after-prepare";
                if (change == "scene") r.Attack(0);
                var before = r.Head;
                var calls = r.Runtime.Storage.Base.WriteCalls;
                var result = r.System.Submit(request, B());
                if (change != "valid")
                {
                    Is(result, change == "scene" ? "StaleContext" : "InconsistentBinding");
                    Assert.AreSame(before, r.Head);
                    Assert.AreEqual(calls, r.Runtime.Storage.Base.WriteCalls);
                    return;
                }
                Is(result, "Completed");
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, result.PresentationDisposition);
                Assert.IsNull(result.Presentation);
                Assert.Greater(r.State.SceneRevision, before.Business.ActiveHistory.CurrentRun.CurrentSnapshot.SceneRevision);
                Same(range.BeforeSnapshot.Random, r.State.Random);
                Same(range.BeforeSnapshot.Members, r.State.Members);
                Same(range.BeforeSnapshot.Enemies, r.State.Enemies);
                Assert.AreEqual(range.BeforeSnapshot.EffectiveActionsCompleted, r.State.EffectiveActionsCompleted);
                SameOwners(before.Business, r.Head.Business);
                Assert.AreEqual(CandidateApplicationRelation.RollbackRecorded, result.Application.OriginalLookup.Relation);
                Assert.IsTrue(typeof(CandidateApplicationResult).GetProperties().All(p => p.GetValue(result.Application.OriginalLookup.Record.Result) == null));
                var old = Is(r.System.QueryOperation(firstRequest, B()), "Completed");
                Assert.AreEqual(CandidateApplicationRelation.Superseded, old.Application.OriginalLookup.Relation);
                Assert.AreEqual(CandidateApplicationRelation.Superseded,
                    Is(r.System.Submit(firstRequest, B()), "Completed").Application.OriginalLookup.Relation);
            }
        }

        [Test]
        public void B15_06_PreviewIsReadOnlyAndPreservesHistoryRejections()
        {
            using (var r = new BattleApplicationRig())
            {
                r.Attack();
                var disk = r.Runtime.Disk();
                var calls = r.Runtime.Storage.Base.Calls;
                var view = r.System.QueryView();
                var good = r.Preview();
                Assert.AreEqual(view.CommitId, good.View.CommitId);
                Assert.AreSame(view.PublishedSnapshot, good.View.PublishedSnapshot);
                Assert.Throws<NotSupportedException>(() => ((IList<CandidateHistoryEntry>)good.Range.Entries).Clear());
                var locator = r.Locator();
                locator.ExpectedSceneRevision++;
                var direct = CandidateHistoryOperations.Locate(view.History, locator, Math());
                var stale = r.System.PreviewRollback(locator, view.CommitId, B().Codec);
                Assert.AreEqual(direct.RejectionCode.ToString(), stale.Code);
                Assert.AreEqual(direct.FieldPath, stale.Diagnostic.FieldPath);
                Assert.AreEqual("StaleContext", r.System.PreviewRollback(r.Locator(), "old-commit", B().Codec).Code);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                r.Runtime.SameDisk(disk);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void B15_07_LinkHasNoActorPreferenceAndTransmitsAvailableRejection(bool stale)
        {
            using (var r = new BattleApplicationRig())
            {
                var draft = r.LinkDraft();
                if (stale) draft.Link.ExpectedSceneRevision++;
                var request = r.Freeze(draft);
                var before = r.Head;
                var result = Is(r.System.Submit(request, B()), stale ? "StaleContext" : "InvalidPhase");
                if (!stale)
                {
                    var expected = CandidateBattleOperations.EvaluateLink(before.Business.ActiveHistory.CurrentRun,
                        new CandidateLinkRequest
                        {
                            PlayerId = draft.PlayerId, AttemptId = draft.Link.AttemptId, OperationId = request.OperationId,
                            ExpectedSceneRevision = draft.Link.ExpectedSceneRevision, Pair = draft.Link.Pair, Route = draft.Link.Route.ToList()
                        }, 100, Math());
                    Assert.AreEqual(expected.RejectionStage, result.DomainRejection.RejectionStage);
                    Assert.AreEqual(expected.FieldPath, result.DomainRejection.FieldPath);
                }
                Assert.IsNull(typeof(CandidateApplicationLinkInput).GetProperty("Actor"));
                Assert.IsNull(typeof(CandidateApplicationLinkInput).GetProperty("ExpectedPreferenceRevision"));
                Assert.AreSame(before, r.Head);
            }
        }

        internal static void SameOwners(CandidateBusinessSnapshot before, CandidateBusinessSnapshot after)
        {
            var old = Encode(before);
            var next = Encode(after);
            foreach (var slice in new[] { 0, 1, 2, 4 })
                CollectionAssert.AreEqual(old.Bodies[slice], next.Bodies[slice], "unchanged owner slice " + slice);
            CollectionAssert.AreEqual(before.RetainedRuns.Select(x => x.Baseline.Entry.AttemptId),
                after.RetainedRuns.Select(x => x.Baseline.Entry.AttemptId));
            for (var i = 0; i < before.RetainedRuns.Count; i++)
            {
                Same(before.RetainedRuns[i].CurrentSnapshot, after.RetainedRuns[i].CurrentSnapshot);
                Same(before.RetainedRuns[i].Records, after.RetainedRuns[i].Records);
            }
            CollectionAssert.AreEqual(before.RetainedRollbacks.Select(x => x.OperationId),
                after.RetainedRollbacks.Select(x => x.OperationId));
            for (var i = 0; i < before.RetainedRollbacks.Count; i++)
            {
                Same(before.RetainedRollbacks[i].BeforeRun.CurrentSnapshot, after.RetainedRollbacks[i].BeforeRun.CurrentSnapshot);
                Same(before.RetainedRollbacks[i].RestoredRun.CurrentSnapshot, after.RetainedRollbacks[i].RestoredRun.CurrentSnapshot);
            }
        }
    }
}
