using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.BattleApplicationRig;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateBattlePresentationTests
    {
        [Test]
        public void B15_08_LatchPrecedesReturnedPlayAndRepeatedCompletionDoesNotReplayOrQueue()
        {
            using (var r = new BattleApplicationRig())
            {
                var request = r.Freeze(r.AttackDraft());
                var first = Is(r.System.Submit(request, B()), "Completed");
                Assert.AreEqual(CandidatePresentationDisposition.PlayOriginal, first.PresentationDisposition);
                Assert.AreSame(first.Presentation.Token, r.System.QueryView().PresentationToken);
                Assert.AreEqual(request.OperationId, first.Presentation.Token.OperationId);
                Assert.AreEqual(r.State.SceneRevision, first.Presentation.Token.SceneRevision);
                Assert.AreEqual(r.State.Baseline.Entry.AttemptId, first.Presentation.Token.AttemptId);
                Assert.AreSame(first.Application.OriginalLookup.BattleOperation.OrderedFacts, first.Presentation.OrderedFacts);
                var second = r.Freeze(r.AttackDraft(1));
                var calls = r.Runtime.Storage.Base.Calls;
                var busy = Is(r.System.Submit(second, B()), "Busy");
                Assert.AreEqual("Presentation", busy.DomainRejection.FieldPath);
                var repeated = Is(r.System.Submit(request, B()), "Completed");
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, repeated.PresentationDisposition);
                Assert.AreSame(first.Presentation.Token, repeated.View.PresentationToken);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, r.System.QueryOperation(request, B()).PresentationDisposition);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Is(r.System.ReportPresentationCompleted(first.Presentation.Token), "PresentationCompleted");
                Is(r.System.ReportPresentationCompleted(first.Presentation.Token), "PresentationIgnored");
                Assert.IsNull(r.System.Submit(request, B()).View.PresentationToken);
                Assert.IsNull(r.Runtime.Model.View.PendingOperationId);
                Assert.AreEqual(3, r.Head.Records.Count);
                Is(r.System.Submit(second, B()), "Completed");
                Assert.AreEqual(4, r.Head.Records.Count);
            }
        }

        [Test]
        public void B15_08_PublicationCallbackSeesFullHeadAndBusyReentryAndListenerCannotUndoCommit()
        {
            using (var r = new BattleApplicationRig())
            {
                var request = r.Freeze(r.AttackDraft());
                CandidateDemoView seen = null;
                CandidateBattleCallResult nested = null, query = null, rebuild = null, report = null;
                CandidateBattlePrepareResult prepared = null;
                var count = 0;
                var handle = r.Runtime.Architecture.RegisterEvent<CandidateApplicationPublished>(e =>
                {
                    count++;
                    seen = r.System.QueryView();
                    query = r.System.QueryOperation(request, B());
                    nested = r.System.Submit(request, B());
                    rebuild = r.System.RebuildLatest();
                    report = r.System.ReportPresentationCompleted(null);
                    prepared = r.System.Prepare(r.AttackDraft(1), B().Codec);
                    throw new IOException("019 listener failure after publication");
                });
                CandidateBattleCallResult result;
                try { result = Is(r.System.Submit(request, B()), "Completed"); }
                finally { handle.UnRegister(); }
                Assert.AreEqual(1, count);
                Assert.AreSame(r.Head, seen.PublishedSnapshot);
                Assert.IsTrue(seen.IsPublishedHeadVerified);
                Assert.AreEqual("Busy", seen.Code);
                Assert.AreEqual("Busy", seen.Attack.Reason);
                Assert.IsNull(seen.PresentationToken);
                Assert.AreEqual("Completed", query.Code);
                Assert.AreNotEqual(CandidatePresentationDisposition.PlayOriginal, query.PresentationDisposition);
                Is(nested, "Busy");
                Is(rebuild, "Busy");
                Is(report, "Busy");
                Assert.AreEqual("Busy", prepared.Code);
                Assert.IsNull(prepared.Request);
                Assert.IsTrue(result.Application.IsCommitted);
                Assert.AreEqual("System.IO.IOException", result.Application.NotificationFailure.ExceptionType);
                Assert.AreEqual(CandidatePresentationDisposition.PlayOriginal, result.PresentationDisposition);
                Assert.AreEqual(1, r.Runtime.Storage.Base.RealMarkerPromotionsFor(result.Application.OriginalCommitId));
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, r.System.Submit(request, B()).PresentationDisposition);
                Assert.AreEqual(1, count);
            }
        }

        [Test]
        public void B15_09_OldA1CompletionCannotReleaseA2AndTokenHasOnlyThreeOriginalValues()
        {
            using (var r = new BattleApplicationRig())
            {
                var first = r.Attack(finish: false);
                var a1 = first.Presentation.Token;
                Is(r.System.ReportPresentationCompleted(a1), "PresentationCompleted");
                var second = r.Attack(1, false);
                var a2 = second.Presentation.Token;
                Is(r.System.ReportPresentationCompleted(a1), "PresentationIgnored");
                Is(r.System.ReportPresentationCompleted(a1), "PresentationIgnored");
                Assert.AreSame(a2, r.System.QueryView().PresentationToken);
                CollectionAssert.AreEquivalent(new[] { "AttemptId", "SceneRevision", "OperationId" },
                    typeof(CandidatePresentationToken).GetProperties().Select(x => x.Name));
                Assert.IsEmpty(typeof(CandidatePresentationToken).GetConstructors());
                Is(r.System.ReportPresentationCompleted(a2), "PresentationCompleted");
                Assert.IsNull(r.System.QueryView().PresentationToken);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void B15_09_RebuildRetiresBothPlayingAndNotYetDeliveredPermission(bool pending)
        {
            using (var r = new BattleApplicationRig())
            {
                var request = r.Freeze(r.AttackDraft());
                if (pending) r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.System.Submit(request, B()), pending ? "SaveFailed" : "Completed");
                var calls = r.Runtime.Storage.Base.Calls;
                r.System.RebuildLatest();
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                var result = pending ? r.System.Retry(request, B()) : r.System.Submit(request, B());
                Is(result, "Completed");
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, result.PresentationDisposition);
                Assert.IsNull(result.View.PresentationToken);
                Assert.IsNull(result.Presentation);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void B15_09_ExternallyPublishedRollbackOrNewAttemptInvalidatesOldToken(bool newAttempt)
        {
            using (var r = new BattleApplicationRig())
            {
                var attack = r.Attack(finish: false);
                var token = attack.Presentation.Token;
                if (newAttempt) r.ExitAndEnter();
                else
                {
                    var range = r.Preview().Range;
                    var request = r.Freeze(r.RollbackDraft(range));
                    // An independent, already accepted 018 caller can publish while this page holds an old token.
                    ApplicationRuntimeRig.Is(r.Runtime.Submit(request.Intent, (basis, intent, budget) =>
                    {
                        var next = CandidateHistoryOperations.PrepareRollback(basis.Business.ActiveHistory,
                            new CandidateRollbackRequest
                            {
                                PlayerId = range.PlayerId, AttemptId = range.AttemptId, ExpectedSceneRevision = range.SceneRevision,
                                HistoryAnchorId = range.HistoryAnchorId, OperationId = intent.OperationId
                            }, range, budget.Math);
                        Assert.IsTrue(next.IsAccepted, next.FieldPath);
                        var b = basis.Business;
                        return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(
                            new CandidateBusinessInput(b.PlayerId, b.Character, b.Inventory, b.Progression, b.Rewards,
                                next.Next, b.RetainedRuns, b.RetainedRollbacks), budget)), new CandidateApplicationResultInput());
                    }), "Completed");
                }
                Assert.IsNull(r.System.QueryView().PresentationToken);
                Is(r.System.ReportPresentationCompleted(token), "PresentationIgnored");
                Assert.AreNotEqual(attack.View.CommitId, r.Head.Header.CommitId);
                if (newAttempt) Assert.AreNotEqual(token.AttemptId, r.State.Baseline.Entry.AttemptId);
                else Assert.AreNotEqual(token.SceneRevision, r.State.SceneRevision);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void B15_05_09_NewInstanceRestoreOrObservedResumeOnlyRebuildsEndState(bool pending)
        {
            string root;
            PreparedCandidateBattleRequest request;
            CandidateBattleApplicationSystem old;
            using (var r = new BattleApplicationRig())
            {
                root = r.Runtime.Root;
                old = r.System;
                request = r.Freeze(r.AttackDraft());
                if (pending) r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.System.Submit(request, B()), pending ? "SaveFailed" : "Completed");
            }
            using (var r = new BattleApplicationRig(false, root: root))
            {
                Assert.AreNotSame(old, r.System);
                ApplicationRuntimeRig.Is(r.Runtime.Open(SaveOpenMode.Existing), pending ? "Pending" : "Ready");
                if (pending) ApplicationRuntimeRig.Is(r.Runtime.Resume(r.Runtime.Model.View.ObservedCandidateCommitIds.Single()), "Completed");
                var calls = r.Runtime.Storage.Base.Calls;
                var result = Is(r.System.QueryOperation(request, B()), "Completed");
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, result.PresentationDisposition);
                Assert.IsNull(result.Presentation);
                Assert.IsNull(result.View.PresentationToken);
                Assert.IsNull(r.System.Submit(request, B()).Presentation);
                Is(r.System.RebuildLatest(), "RebuildLatest");
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Assert.AreEqual(0, r.Runtime.Builds);
            }
        }

        [Test]
        public void B15_10_ClosingH04RetainsOriginalS17AcrossFailureAndRestoreWithoutH06()
        {
            string root;
            string reserved;
            string fingerprint;
            PreparedCandidateBattleRequest closing;
            using (var r = new BattleApplicationRig())
            {
                root = r.Runtime.Root;
                while (r.State.Enemies.Count(x => x.Hp.Numerator.Sign > 0) > 1)
                    r.Attack(r.State.Enemies.ToList().FindIndex(x => x.Hp.Numerator.Sign > 0));
                var before = r.Head.Business;
                closing = r.Freeze(r.AttackDraft(r.State.Enemies.ToList().FindIndex(x => x.Hp.Numerator.Sign > 0)));
                r.Runtime.Storage.Base.Arm("Snapshot.Flush.after", null);
                var failed = Is(r.System.Submit(closing, B()), "SaveFailed");
                var commit = failed.View.ApplicationView.PendingCommitId;
                var original = File.ReadAllBytes(r.Runtime.Path("w-" + commit + ".snapshot.tmp"));
                CandidateApplicationSnapshot candidate;
                using (var stream = new MemoryStream(original))
                {
                    var summary = Accept(SaveEnvelopeCodec.ReadUncommittedRequirements(stream, B().Codec));
                    stream.Position = 0;
                    candidate = Accept(CandidateApplicationSaveCodec.Decode(
                        Accept(SaveEnvelopeCodec.Read(stream, summary.Descriptor, B().Codec)), B().Codec));
                }
                reserved = candidate.Continuation.ReservedOperationId;
                fingerprint = candidate.Business.ActiveHistory.CurrentRun.FinalReport.Fingerprint;
                Assert.IsTrue(Guid.TryParseExact(reserved, "N", out _));
                var result = Is(r.System.Retry(closing, B()), "Completed");
                Assert.AreEqual(BattlePhase.WonPendingSettlement, result.View.BattleSnapshot.Phase);
                Assert.AreEqual(CandidateApplicationContinuationStage.AwaitBaseSettlement, result.View.Continuation.Stage);
                Assert.AreEqual(reserved, result.View.Continuation.ReservedOperationId);
                Assert.AreEqual(closing.OperationId, result.View.Continuation.ClosingOperationId);
                Assert.AreEqual(fingerprint, result.View.FinalReport.Fingerprint);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(r.Runtime.Path(SnapshotName(commit))));
                CandidateBattleApplicationTests.SameOwners(before, r.Head.Business);
                var calls = r.Runtime.Storage.Base.Calls;
                var records = r.Head.Records.Count;
                Is(r.System.ReportPresentationCompleted(result.Presentation.Token), "PresentationCompleted");
                r.System.QueryView();
                Is(r.System.QueryOperation(closing, B()), "Completed");
                Is(r.System.Submit(closing, B()), "Completed");
                Is(r.System.RebuildLatest(), "RebuildLatest");
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
                Assert.AreEqual(records, r.Head.Records.Count);
                Assert.IsEmpty(r.Head.Business.Rewards.BaseRewards);
                Assert.IsEmpty(r.Head.Business.Inventory.OrdinaryGrants);
                Assert.IsEmpty(r.Head.Business.Progression.FirstClears);
                Assert.IsFalse(r.System.QueryView().Attack.IsAvailable);
            }
            using (var r = new BattleApplicationRig(false, root: root))
            {
                ApplicationRuntimeRig.Is(r.Runtime.Open(SaveOpenMode.Existing), "Ready");
                Assert.AreEqual(reserved, r.System.QueryView().Continuation.ReservedOperationId);
                Assert.AreEqual(fingerprint, r.System.QueryView().FinalReport.Fingerprint);
                Assert.AreEqual(CandidatePresentationDisposition.RebuildLatest, r.System.Submit(closing, B()).PresentationDisposition);
                Assert.IsEmpty(r.Head.Business.Rewards.BaseRewards);
            }
        }

        [Test]
        public void B15_11_WrongThreadEveryEntryLeavesPlayingTokenAndFilesUntouched()
        {
            using (var r = new BattleApplicationRig())
            {
                var request = r.Freeze(r.AttackDraft());
                var played = Is(r.System.Submit(request, B()), "Completed");
                var token = played.Presentation.Token;
                var draft = r.AttackDraft(1);
                var locator = r.Locator();
                var codes = new List<string>();
                Exception error = null;
                var calls = r.Runtime.Storage.Base.Calls;
                var thread = new Thread(() =>
                {
                    try
                    {
                        var prepare = r.System.Prepare(draft, B().Codec);
                        codes.Add(prepare.Code);
                        Assert.IsNull(prepare.Request);
                        codes.Add(r.System.Submit(request, B()).Code);
                        codes.Add(r.System.Retry(request, B()).Code);
                        codes.Add(r.System.Resolve(request, B()).Code);
                        codes.Add(r.System.End(request, B()).Code);
                        codes.Add(r.System.QueryOperation(request, B()).Code);
                        codes.Add(r.System.QueryView().Code);
                        codes.Add(r.System.PreviewRollback(locator, played.View.CommitId, B().Codec).Code);
                        codes.Add(r.System.ReportPresentationCompleted(token).Code);
                        codes.Add(r.System.RebuildLatest().Code);
                    }
                    catch (Exception caught) { error = caught; }
                });
                thread.Start();
                Assert.IsTrue(thread.Join(30000));
                Assert.IsNull(error);
                Assert.AreEqual(10, codes.Count);
                Assert.IsTrue(codes.All(x => x == "WrongThread"));
                Assert.AreSame(token, r.System.QueryView().PresentationToken);
                Assert.AreEqual(calls, r.Runtime.Storage.Base.Calls);
            }
        }

        [Test]
        public void B15_11_ClosedFacadeCannotTouchNewArchitectureOrLease()
        {
            CandidateBattleApplicationSystem old;
            PreparedCandidateBattleRequest request;
            CandidatePresentationToken token;
            using (var r = new BattleApplicationRig())
            {
                old = r.System;
                request = r.Freeze(r.AttackDraft());
                token = Is(old.Submit(request, B()), "Completed").Presentation.Token;
            }
            using (var fresh = new BattleApplicationRig())
            {
                var view = fresh.Runtime.Model.View;
                var calls = fresh.Runtime.Storage.Base.Calls;
                Assert.AreEqual("Disposed", old.Prepare(PlainDraft(CandidateApplicationKind.Attack), B().Codec).Code);
                Is(old.Submit(request, B()), "Disposed");
                Is(old.Retry(request, B()), "Disposed");
                Is(old.Resolve(request, B()), "Disposed");
                Is(old.End(request, B()), "Disposed");
                Is(old.QueryOperation(request, B()), "Disposed");
                Assert.AreEqual("Disposed", old.QueryView().Code);
                Assert.AreEqual("Disposed", old.PreviewRollback(null, null, B().Codec).Code);
                Is(old.ReportPresentationCompleted(token), "Disposed");
                Is(old.RebuildLatest(), "Disposed");
                Assert.AreSame(view, fresh.Runtime.Model.View);
                Assert.AreSame(fresh.System, FightMatchDemoArchitecture.Interface.GetSystem<CandidateBattleApplicationSystem>());
                Assert.AreNotSame(old, fresh.System);
                Assert.AreEqual(calls, fresh.Runtime.Storage.Base.Calls);
                Bad(LocalSaveStore.Open(fresh.Runtime.Storage, "player:015b", SavePurpose.CandidateValidation,
                    SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B()), "Busy");
            }
        }
    }
}
