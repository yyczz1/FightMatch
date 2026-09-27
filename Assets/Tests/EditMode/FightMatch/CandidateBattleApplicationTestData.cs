using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FightMatch.Application;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    internal sealed class BattleApplicationRig : IDisposable
    {
        internal readonly ApplicationRuntimeRig Runtime;
        internal readonly CandidateBattleApplicationSystem System;
        internal CandidateApplicationSnapshot Head => Runtime.Head;
        internal BattleSnapshot State => Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;

        internal BattleApplicationRig(bool ready = true, int hp = 100, string root = null, int level = 1)
        {
            Runtime = new ApplicationRuntimeRig(root);
            System = Runtime.Architecture.GetSystem<CandidateBattleApplicationSystem>();
            try
            {
                if (!ready) return;
                if (hp != 100 || level != 1)
                {
                    var domain = New(level, hp: hp);
                    Runtime.Domain.Context = domain.Context;
                    Runtime.Domain.Use(Prepare(domain));
                    Runtime.Domain.EntryInput = domain.EntryInput;
                    Runtime.Domain.Routes = domain.Routes;
                    Runtime.Domain.Level = level;
                }
                SaveRecoveryCapabilities caps = null;
                if (level != 1)
                {
                    var coverage = new ApplicationScenario(level);
                    coverage.Enter();
                    coverage.Win();
                    coverage.Settle();
                    caps = ApplicationRuntimeRig.Caps(coverage.Envelope);
                }
                ApplicationRuntimeRig.Is(Runtime.Open(caps: caps), "InitializationReady");
                ApplicationRuntimeRig.Is(Runtime.Initialize(), "Completed");
                ApplicationRuntimeRig.Is(Runtime.Enter(), "Completed");
            }
            catch { Runtime.Dispose(); throw; }
        }

        internal CandidateBattleDraft AttackDraft(int pair = 0)
        {
            var input = Runtime.AttackInput("unused-fixture-id", pair);
            input.Attack.ItemUseEnabled = false;
            return new CandidateBattleDraft
            {
                PlayerId = input.PlayerId, ExpectedCommitId = input.ExpectedCommitId, Context = CopyContext(input.Context),
                Kind = CandidateApplicationKind.Attack, Attack = input.Attack
            };
        }

        internal CandidateBattleDraft LinkDraft(int pair = 0)
        {
            var draft = AttackDraft(pair);
            var a = draft.Attack;
            draft.Attack = null;
            draft.Kind = CandidateApplicationKind.Link;
            draft.Link = new CandidateApplicationLinkInput
            {
                AttemptId = a.AttemptId, ExpectedSceneRevision = a.ExpectedSceneRevision, Pair = a.Pair, Route = a.Route
            };
            return draft;
        }

        internal PreparedCandidateBattleRequest Freeze(CandidateBattleDraft draft)
        {
            var prepared = System.Prepare(draft, B().Codec);
            Assert.IsTrue(prepared.IsAccepted, prepared.Code + " " + prepared.Diagnostic?.FieldPath);
            return prepared.Request;
        }

        internal CandidateBattleCallResult Attack(int pair = 0, bool finish = true)
        {
            var result = Is(System.Submit(Freeze(AttackDraft(pair)), B()), "Completed");
            if (finish) Is(System.ReportPresentationCompleted(result.Presentation.Token), "PresentationCompleted");
            return result;
        }

        internal CandidateHistoryLocator Locator(int pair = 0)
        {
            return new CandidateHistoryLocator
            {
                PlayerId = "player:015b", AttemptId = State.Baseline.Entry.AttemptId,
                ExpectedSceneRevision = State.SceneRevision, Pair = State.Enemies[pair].PairKey,
                Kind = CandidateHistoryLocatorKind.EndpointLatestAttack
            };
        }

        internal CandidateBattleDraft RollbackDraft(CandidateRollbackRange range)
        {
            return new CandidateBattleDraft
            {
                PlayerId = range.PlayerId, ExpectedCommitId = Head.Header.CommitId,
                Context = CopyContext(Runtime.Domain.Context), Kind = CandidateApplicationKind.Rollback,
                Rollback = new CandidateApplicationRollbackInput
                {
                    AttemptId = range.AttemptId, ExpectedSceneRevision = range.SceneRevision, HistoryAnchorId = range.HistoryAnchorId,
                    TargetOperationId = range.OperationId, ConfirmedRemovedOperationIds = range.Entries.Select(x => x.OperationId).ToList()
                }
            };
        }

        internal CandidateBattlePreviewResult Preview(int pair = 0)
        {
            var result = System.PreviewRollback(Locator(pair), Head.Header.CommitId, B().Codec);
            Assert.IsTrue(result.IsAccepted, result.Code + " " + result.Diagnostic?.FieldPath);
            return result;
        }

        internal void ExitAndEnter()
        {
            var begin = Head.Business.Progression.ActiveAttempt.Begin;
            var input = Runtime.Input(CandidateApplicationKind.ExitAttempt, "exit-for-new-attempt");
            input.ExitAttempt = new CandidateApplicationEndInput
            {
                AttemptId = begin.AttemptId, ChallengeId = begin.ChallengeId, EntryBaselineId = begin.EntryBaselineId,
                ExpectedSceneRevision = State.SceneRevision
            };
            ApplicationRuntimeRig.Is(Runtime.Submit(Runtime.Freeze(input), (basis, intent, budget) =>
            {
                Runtime.Domain.Use(basis.Business);
                Runtime.Domain.EndExit();
                return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Runtime.Domain.Input(), budget)),
                    new CandidateApplicationResultInput { EndReceiptId = "end:" + begin.AttemptId });
            }), "Completed");
            Runtime.Domain.EntryInput.AttemptId = "attempt:019-next";
            Runtime.Domain.EntryInput.EntryBaselineId = "baseline:019-next";
            ApplicationRuntimeRig.Is(Runtime.Submit(Runtime.Freeze(Runtime.EnterInput("enter-next")), Runtime.EnterBuild), "Completed");
        }

        internal static CandidateBattleCallResult Is(CandidateBattleCallResult result, string code)
        {
            Assert.AreEqual(code, result.Code, result.DomainRejection?.FieldPath + " " +
                result.Application?.Diagnostic?.Code + " " + result.Application?.Diagnostic?.FieldPath + " " +
                result.Application?.Diagnostic?.ExceptionMessage);
            return result;
        }

        internal static CandidateContext CopyContext(RuleContext c)
        {
            return new CandidateContext
            {
                DraftId = c.DraftId, DraftRevision = c.DraftRevision, ContentFingerprint = c.ContentFingerprint,
                RuleVersion = c.RuleVersion, NumericContractVersion = c.NumericContractVersion,
                RandomContractVersion = c.RandomContractVersion, SourceNotes = c.SourceNotes.ToList()
            };
        }

        internal static CandidateBattleDraft PlainDraft(CandidateApplicationKind kind)
        {
            var draft = new CandidateBattleDraft
            {
                PlayerId = "player", ExpectedCommitId = "commit", Kind = kind,
                Context = new CandidateContext
                {
                    DraftId = "draft", DraftRevision = 1, ContentFingerprint = "candidate", RuleVersion = "r1",
                    NumericContractVersion = "exact-r1", RandomContractVersion = "pcg-r1", SourceNotes = new List<string> { "source" }
                }
            };
            var pair = BattlePairKey.Create("attempt", "face", "pair");
            var route = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) };
            if (kind == CandidateApplicationKind.Attack) draft.Attack = new CandidateApplicationAttackInput
            {
                AttemptId = "attempt", ExpectedSceneRevision = 1, Actor = BattleCombatantKey.ForParticipant("attempt", "W"),
                Pair = pair, Route = route, ExpectedPreferenceRevision = 1, ItemUseEnabled = false
            };
            else if (kind == CandidateApplicationKind.Link) draft.Link = new CandidateApplicationLinkInput
            { AttemptId = "attempt", ExpectedSceneRevision = 1, Pair = pair, Route = route };
            else draft.Rollback = new CandidateApplicationRollbackInput
            {
                AttemptId = "attempt", ExpectedSceneRevision = 2, HistoryAnchorId = "anchor", TargetOperationId = "a1",
                ConfirmedRemovedOperationIds = new List<string> { "a1", "a2" }
            };
            return draft;
        }

        public void Dispose() { Runtime.Dispose(); }
    }

    internal sealed class UnvisitedList<T> : IReadOnlyList<T>
    {
        public int Count { get; }
        internal int Visits;
        internal UnvisitedList(int count) { Count = count; }
        public T this[int index] { get { Visits++; throw new InvalidOperationException("Oversized list indexed"); } }
        public IEnumerator<T> GetEnumerator() { Visits++; throw new InvalidOperationException("Oversized list enumerated"); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
