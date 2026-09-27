using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    // Valid states come from the existing public domain operations, including the prior
    // fixture's public-operation recipes. No fabricated successful completion graph.
    internal sealed class ApplicationScenario
    {
        internal readonly BusinessSaveScenario Domain;
        internal CandidateApplicationSnapshot Head;
        internal SaveEnvelope Envelope;
        internal CandidateApplicationCandidate Candidate;
        internal readonly Dictionary<string, PreparedCandidateApplicationIntent> Intents = new Dictionary<string, PreparedCandidateApplicationIntent>();
        private int attempts;

        internal ApplicationScenario(int level = 1, int entryLevel = 1, int hp = 100)
        {
            Domain = New(level, entryLevel: entryLevel, hp: hp);
            var input = Intent(CandidateApplicationKind.InitializeProfile, "init");
            input.InitializeProfile = new CandidateApplicationInitializeInput { CharacterId = Domain.Character.CharacterId,
                ClassId = Domain.Character.ClassId, InitialLevel = Domain.Character.Level, InitialExperience = Domain.Character.Experience,
                OriginalSlot = Domain.Character.OriginalSlot };
            Commit(input, new CandidateApplicationResultInput());
        }

        internal CandidateApplicationIntentInput Intent(CandidateApplicationKind kind, string operation)
        {
            return new CandidateApplicationIntentInput { PlayerId = Domain.Character.PlayerId, OperationId = operation, Kind = kind,
                ExpectedCommitId = Head?.Header.CommitId, Context = BusinessFields.ContextInput(Domain.Character.Definition.Context) };
        }

        internal void Commit(CandidateApplicationIntentInput input, CandidateApplicationResultInput result, string reserved = null)
        {
            var prepared = Accept(CandidateApplicationProtocol.PrepareIntent(input, Budget()));
            Candidate = Accept(CandidateApplicationProtocol.Propose(Head, Prepare(Domain), prepared, result, reserved, Budget()));
            Assert.IsNull(Candidate.Records.Last().CommitId);
            Assert.IsNull(Candidate.Records.Last().Generation);
            var header = Header(Head, input.OperationId);
            Envelope = Accept(CandidateApplicationSaveCodec.Encode(Candidate, header, Budget()));
            using (var stream = new MemoryStream())
            {
                var descriptor = Accept(SaveEnvelopeCodec.Write(stream, Envelope, Budget()));
                stream.Position = 0;
                var loaded = Accept(SaveEnvelopeCodec.Read(stream, descriptor, Budget()));
                Head = Accept(CandidateApplicationSaveCodec.Decode(loaded, Budget()));
            }
            Domain.Use(Head.Business);
            Intents.Add(input.OperationId, prepared);
        }

        internal static CandidateBusinessSaveHeader Header(CandidateApplicationSnapshot basis, string operation)
        {
            var generation = basis == null ? BigInteger.One : basis.Header.SaveGeneration + 1;
            var commit = "application-commit:" + generation;
            var index = basis == null ? new List<SaveCommitIndexEntry>() : basis.Header.CommitIndex.ToList();
            if (basis != null)
            {
                var row = index.Last();
                index[index.Count - 1] = new SaveCommitIndexEntry(row.Generation, row.CommitId, row.ParentCommitId,
                    basis.Descriptor.TotalLength, basis.Descriptor.Sha256, row.OperationIds);
            }
            index.Add(new SaveCommitIndexEntry(generation, commit, basis?.Header.CommitId, null, null, new[] { operation }));
            return new CandidateBusinessSaveHeader(generation, commit, basis?.Header.CommitId, index);
        }

        internal void Enter(string operation = "enter")
        {
            var input = Intent(CandidateApplicationKind.EnterAttempt, operation);
            input.EnterAttempt = new CandidateApplicationEnterInput { LevelId = Domain.EntryInput.Level.LevelId,
                LevelVersion = Domain.EntryInput.Level.LevelVersion, CharacterId = Domain.Character.CharacterId,
                ExpectedCharacterRevision = Domain.Character.StateRevision, OriginalSlot = Domain.Character.OriginalSlot };
            var entry = Domain.EntryInput;
            entry.AttemptId = "attempt:" + ++attempts;
            entry.EntryBaselineId = "baseline:" + attempts;
            var open = Domain.Progression.Challenges.LastOrDefault(x => !x.IsClosed);
            entry.ChallengeId = open?.ChallengeId ?? "challenge:" + attempts;
            RefreshMember();
            Domain.Begin();
            Commit(input, new CandidateApplicationResultInput { ChallengeId = entry.ChallengeId, AttemptId = entry.AttemptId, EntryBaselineId = entry.EntryBaselineId });
        }

        private void RefreshMember()
        {
            var stats = CandidateCharacterGrowth.ComputeBaseStats(Domain.Character, Math());
            var m = Domain.EntryInput.Members[0];
            m.Level = stats.Level;
            m.EntryHp = stats.EntryHp;
            m.IsReady = stats.IsReady;
            m.Stats = new StatsInput { MaxHp = stats.Stats.MaxHp, Attack = stats.Stats.Attack, PhysicalDefense = stats.Stats.PhysicalDefense,
                MagicDefense = stats.Stats.MagicDefense, Evasion = stats.Stats.Evasion, AttackRange = stats.Stats.AttackRange };
            m.Crit.TargetProbability = stats.TargetProbability;
        }

        internal CandidateApplicationIntentInput AttackInput(string operation, int pair)
        {
            var input = Intent(CandidateApplicationKind.Attack, operation);
            var state = Domain.History.CurrentRun.CurrentSnapshot;
            input.Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                Actor = state.Members[0].CombatantKey, Pair = state.Enemies[pair].PairKey, Route = Domain.Routes[pair].ToList(),
                ExpectedPreferenceRevision = Domain.Inventory.PreferenceRevision, ItemUseEnabled = true };
            return input;
        }

        internal void Attack(string operation, int pair)
        {
            var input = AttackInput(operation, pair);
            Domain.Attack(operation, pair);
            Commit(input, new CandidateApplicationResultInput { HistoryAnchorId = "anchor:" + operation },
                Domain.History.CurrentRun.FinalReport == null ? null : "settle:" + Domain.EntryInput.AttemptId);
        }

        internal void Rollback(string operation, string target)
        {
            var input = Intent(CandidateApplicationKind.Rollback, operation);
            var h = Domain.History;
            var range = CandidateHistoryOperations.ReadRange(h, new CandidateHistoryRangeRequest { PlayerId = Domain.Character.PlayerId,
                AttemptId = Domain.EntryInput.AttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision, HistoryAnchorId = "anchor:" + target }, Math());
            Assert.IsTrue(range.IsAccepted, range.FieldPath);
            input.Rollback = new CandidateApplicationRollbackInput { AttemptId = Domain.EntryInput.AttemptId, ExpectedSceneRevision = range.Range.SceneRevision,
                HistoryAnchorId = range.Range.HistoryAnchorId, TargetOperationId = range.Range.OperationId,
                ConfirmedRemovedOperationIds = range.Range.Entries.Select(x => x.OperationId).ToList() };
            Domain.Rollback(operation, target);
            Commit(input, new CandidateApplicationResultInput());
        }

        internal void Win()
        {
            var n = 0;
            while (Domain.History.CurrentRun.FinalReport == null)
            {
                Assert.Less(n, 20);
                var pair = Domain.History.CurrentRun.CurrentSnapshot.Enemies.ToList().FindIndex(x => x.Hp.Numerator.Sign > 0);
                Attack("win:" + attempts + ":" + n++, pair);
            }
        }

        internal CandidateApplicationIntentInput VictoryInput()
        {
            var report = Domain.History.CurrentRun.FinalReport;
            var input = Intent(CandidateApplicationKind.SettleVictory, Head.Continuation.ReservedOperationId);
            input.SettleVictory = new CandidateApplicationVictoryInput { AttemptId = report.AttemptId, ChallengeId = report.ChallengeId,
                EntryBaselineId = report.EntryBaselineId, FinalReportFingerprint = report.Fingerprint, TerminalOperationId = report.TerminalOperationId,
                RewardDefinitionId = "reward", RewardDefinitionVersion = "candidate-r1" };
            return input;
        }

        internal void Settle()
        {
            var input = VictoryInput();
            Domain.EndVictory();
            var end = Domain.Progression.Challenges.Last().Attempts.Last().End;
            Commit(input, new CandidateApplicationResultInput { EndReceiptId = end.EndReceiptId, SettlementId = end.SettlementId });
        }

        internal CandidateApplicationIntentInput EndInput(string operation, bool restart = false)
        {
            var input = Intent(restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt, operation);
            var b = Domain.Progression.ActiveAttempt.Begin;
            var payload = new CandidateApplicationEndInput { AttemptId = b.AttemptId, ChallengeId = b.ChallengeId,
                EntryBaselineId = b.EntryBaselineId, ExpectedSceneRevision = Domain.History.CurrentRun.CurrentSnapshot.SceneRevision };
            if (restart) input.RestartAttempt = payload;
            else input.ExitAttempt = payload;
            return input;
        }

        internal void Exit(string operation = "exit", bool down = false)
        {
            var input = EndInput(operation);
            Domain.EndExit(down);
            Commit(input, new CandidateApplicationResultInput { EndReceiptId = "end:" + input.ExitAttempt.AttemptId });
        }

        internal void Restart(string operation = "restart")
        {
            var input = EndInput(operation, true);
            var b = Domain.Progression.ActiveAttempt.Begin;
            var newAttempt = "attempt:" + ++attempts;
            var endId = "end:" + b.AttemptId;
            var p = CandidateProgression.EndAttempt(Domain.Progression, new CandidateProgressionEndFacts { PlayerId = b.PlayerId,
                LevelId = b.Level.LevelId, LevelVersion = b.Level.LevelVersion, Context = Domain.Context, ChallengeId = b.ChallengeId,
                AttemptId = b.AttemptId, EntryBaselineId = b.EntryBaselineId, EndReceiptId = endId, Kind = CandidateProgressionEndKind.ImmediateRestart,
                SettlementId = null, FinalReportFingerprint = null, NewAttemptId = newAttempt }, Domain.Progression.StateRevision, Math());
            Assert.IsTrue(p.IsAccepted, p.FieldPath);
            var c = CandidateCharacterEnd.Propose(Domain.Character, new CandidateCharacterEndFacts { PlayerId = b.PlayerId, CharacterId = b.Participant.CharacterId,
                AttemptId = b.AttemptId, EntryBaselineId = b.EntryBaselineId, EndReceiptId = endId, Context = Domain.Context, Kind = CandidateCharacterEndKind.ImmediateRestart,
                WasParticipant = true, WasDown = Domain.History.CurrentRun.CurrentSnapshot.Members[0].Hp.Numerator.IsZero, RecoveryId = null, TimeSample = null },
                Domain.Character.StateRevision, Math());
            Assert.IsTrue(c.IsAccepted, c.FieldPath);
            var inventory = CandidateInventory.End(Domain.Inventory, new CandidateInventoryEndIntent { PlayerId = b.PlayerId, AttemptId = b.AttemptId,
                EntryBaselineId = b.EntryBaselineId, EndReceiptId = endId, Context = Domain.Context, Kind = CandidateInventoryEndKind.ImmediateRestart,
                SettlementId = null, NewAttemptId = newAttempt, Remaining = new List<CandidateInventoryRemainingInput>(), Rewards = new List<CandidateInventoryQuantityInput>() },
                Domain.Inventory.StateRevision, Math());
            Assert.IsTrue(inventory.IsAccepted, inventory.FieldPath);
            Domain.Progression = p.Next;
            Domain.Character = c.Next;
            Domain.Inventory = inventory.Next;
            Domain.EntryInput.AttemptId = newAttempt;
            var prepared = new BattleEntryPreparer().PrepareCandidate(Domain.EntryInput, Math());
            Assert.IsTrue(prepared.IsAccepted, prepared.FieldPath);
            var bytes = new byte[48];
            for (var i = 0; i < bytes.Length; i += 16) { bytes[i] = 42; bytes[i + 8] = 54; }
            var random = CandidateRandomPreparer.Prepare(prepared.Entry, new CandidateSeedMaterial { Bytes = bytes,
                SourceCapabilityId = "isolated:018b", MappingId = CandidateRandomPreparer.SupportedMappingId }, Math());
            Assert.IsTrue(random.IsAccepted, random.FieldPath);
            var run = CandidateBattleOperations.CreateCandidate(random.Binding, Math());
            Assert.IsTrue(run.IsAccepted, run.FieldPath);
            var history = CandidateHistoryOperations.CreateCandidate(run.Run, Math());
            Assert.IsTrue(history.IsAccepted, history.FieldPath);
            Domain.History = history.Next;
            Commit(input, new CandidateApplicationResultInput { EndReceiptId = endId, NewAttemptId = newAttempt });
        }

        internal CandidateCharacterResult Recover(string operation, CandidateTimeSample time, BigInteger? expected = null)
        {
            var period = Domain.Character.RecoveryPeriods.Last();
            var input = Intent(CandidateApplicationKind.AdvanceRecovery, operation);
            input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = Domain.Character.CharacterId, RecoveryId = period.RecoveryId,
                ExpectedCharacterRevision = expected ?? Domain.Character.StateRevision, TimeSample = time };
            var result = CandidateRecoveryClock.Advance(Domain.Character, period.RecoveryId, time, input.AdvanceRecovery.ExpectedCharacterRevision.Value, Math());
            Assert.IsTrue(result.IsAccepted, result.FieldPath);
            Domain.Character = result.Next;
            Commit(input, new CandidateApplicationResultInput { RecoveryResult = result });
            return result;
        }

        internal CandidateApplicationLookup Lookup(string operation)
        { return Accept(CandidateApplicationProtocol.Lookup(Head, Intents[operation], Budget())); }

        internal static CandidateApplicationIntentInput Simple(CandidateApplicationKind kind)
        {
            var input = new CandidateApplicationIntentInput { PlayerId = "p", OperationId = "i", Kind = kind, ExpectedCommitId = null,
                Context = new CandidateContext { DraftId = "d", DraftRevision = 1, ContentFingerprint = "c", RuleVersion = "r",
                    NumericContractVersion = "n", RandomContractVersion = "q", SourceNotes = new List<string> { "s" } } };
            switch (kind)
            {
                case CandidateApplicationKind.InitializeProfile:
                    input.InitializeProfile = new CandidateApplicationInitializeInput { CharacterId = "x", ClassId = "w", InitialLevel = 1, InitialExperience = 0, OriginalSlot = 0 }; break;
                case CandidateApplicationKind.EnterAttempt:
                    input.EnterAttempt = new CandidateApplicationEnterInput { LevelId = "l", LevelVersion = "v", CharacterId = "x", ExpectedCharacterRevision = 1, OriginalSlot = 0 }; break;
                case CandidateApplicationKind.Attack:
                    input.Attack = new CandidateApplicationAttackInput { AttemptId = "a", ExpectedSceneRevision = 1, Actor = BattleCombatantKey.ForParticipant("a", "x"),
                        Pair = new BattlePairKey("a", "f", "p"), Route = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) }, ExpectedPreferenceRevision = 1, ItemUseEnabled = false }; break;
                case CandidateApplicationKind.Link:
                    input.Link = new CandidateApplicationLinkInput { AttemptId = "a", ExpectedSceneRevision = 1, Pair = new BattlePairKey("a", "f", "p"),
                        Route = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) } }; break;
                case CandidateApplicationKind.Rollback:
                    input.Rollback = new CandidateApplicationRollbackInput { AttemptId = "a", ExpectedSceneRevision = 2, HistoryAnchorId = "h", TargetOperationId = "o",
                        ConfirmedRemovedOperationIds = new List<string> { "o", "o2" } }; break;
                case CandidateApplicationKind.SettleVictory:
                    input.SettleVictory = new CandidateApplicationVictoryInput { AttemptId = "a", ChallengeId = "c", EntryBaselineId = "b", FinalReportFingerprint = "f",
                        TerminalOperationId = "o", RewardDefinitionId = "r", RewardDefinitionVersion = "v" }; break;
                case CandidateApplicationKind.ExitAttempt:
                    input.ExitAttempt = new CandidateApplicationEndInput { AttemptId = "a", ChallengeId = "c", EntryBaselineId = "b", ExpectedSceneRevision = 1 }; break;
                case CandidateApplicationKind.RestartAttempt:
                    input.RestartAttempt = new CandidateApplicationEndInput { AttemptId = "a", ChallengeId = "c", EntryBaselineId = "b", ExpectedSceneRevision = 1 }; break;
                case CandidateApplicationKind.AdvanceRecovery:
                    input.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = "x", RecoveryId = "r", ExpectedCharacterRevision = 1,
                        TimeSample = Clock(-5, R(1, 3)) }; break;
            }
            return input;
        }

        internal static CandidateTimeSample Clock(BigInteger wall, ExactRational monotonic = null, string scope = "clock-original")
        {
            return new CandidateTimeSample { WallUtcMilliseconds = wall, ObservedAtUtcMilliseconds = wall + 1,
                MonotonicElapsedMilliseconds = monotonic, MonotonicScopeId = monotonic == null ? null : scope,
                Source = "test-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
        }
    }
}
