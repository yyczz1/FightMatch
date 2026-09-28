using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Platform;
using FightMatch.Tests;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BusinessSaveScenario;

namespace FightMatch.Core.Tests
{
    // All application calls use the real instance Command/Query path; business changes
    // reuse the accepted public-operation recipes, and all storage delegates to real files.
    internal sealed class ApplicationRuntimeRig : IDisposable
    {
        internal readonly string Root;
        internal readonly ApplicationFaultStorage Storage;
        internal readonly BusinessSaveScenario Domain = New();
        internal readonly IArchitecture Architecture;
        internal readonly CandidateApplicationSystem System;
        internal readonly CandidateApplicationModel Model;
        internal readonly Dictionary<string, PreparedCandidateApplicationIntent> Intents =
            new Dictionary<string, PreparedCandidateApplicationIntent>(StringComparer.Ordinal);
        internal int Builds;
        internal bool Closed;
        internal CandidateApplicationSnapshot Head => Model.View.PublishedSnapshot;

        internal ApplicationRuntimeRig(string root = null)
        {
            Root = root ?? NewCase();
            Storage = new ApplicationFaultStorage(CreateStorage(Root, "player:015b", SavePurpose.CandidateValidation));
            Architecture = FightMatchDemoArchitecture.Interface;
            System = Architecture.GetSystem<CandidateApplicationSystem>();
            Model = Architecture.GetModel<CandidateApplicationModel>();
            Assert.AreEqual(CandidateApplicationPhase.Unconfigured, Model.View.Phase);
        }

        internal CandidateApplicationCallResult Open(SaveOpenMode mode = SaveOpenMode.CreateNew,
            SaveRecoveryCapabilities caps = null, SaveStoreBudget budget = null)
        {
            return Architecture.SendCommand(new OpenCandidateApplicationCommand(Storage, "player:015b", mode, caps ?? Capabilities(), budget ?? B()));
        }

        internal static CandidateApplicationCallResult Is(CandidateApplicationCallResult result, string code)
        {
            Assert.AreEqual(code, result.Code, result.Diagnostic?.FieldPath + " " + result.Diagnostic?.ExceptionMessage);
            return result;
        }

        internal CandidateApplicationIntentInput Input(CandidateApplicationKind kind, string operation)
        {
            return new CandidateApplicationIntentInput { PlayerId = "player:015b", OperationId = operation,
                Kind = kind, ExpectedCommitId = Head?.Header.CommitId, Context = Domain.Context };
        }

        internal CandidateApplicationIntentInput InitializeInput(string operation = "init")
        {
            var input = Input(CandidateApplicationKind.InitializeProfile, operation);
            input.InitializeProfile = new CandidateApplicationInitializeInput { CharacterId = Domain.Character.CharacterId,
                ClassId = Domain.Character.ClassId, InitialLevel = Domain.Character.Level,
                InitialExperience = Domain.Character.Experience, OriginalSlot = Domain.Character.OriginalSlot };
            return input;
        }

        internal PreparedCandidateApplicationIntent Freeze(CandidateApplicationIntentInput input)
        {
            var intent = Accept(CandidateApplicationProtocol.PrepareIntent(input, B().Codec));
            Intents[input.OperationId] = intent;
            return intent;
        }

        internal CandidateApplicationCallResult Submit(PreparedCandidateApplicationIntent intent,
            CandidateApplicationBuilder builder, SaveStoreBudget budget = null)
        {
            return Architecture.SendCommand(new SubmitCandidateApplicationCommand(intent, builder, budget ?? B()));
        }

        internal CandidateApplicationBuildResult InitialBuild(CandidateApplicationSnapshot basis,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            Builds++;
            Assert.IsNull(basis);
            Assert.AreEqual("player:015b", intent.PlayerId);
            return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Domain.Input(), budget)),
                new CandidateApplicationResultInput());
        }

        internal CandidateApplicationCallResult Initialize()
        {
            return Submit(Freeze(InitializeInput()), InitialBuild);
        }

        internal CandidateApplicationIntentInput EnterInput(string operation = "enter")
        {
            Domain.Use(Head.Business);
            var input = Input(CandidateApplicationKind.EnterAttempt, operation);
            input.EnterAttempt = new CandidateApplicationEnterInput { LevelId = Domain.EntryInput.Level.LevelId,
                LevelVersion = Domain.EntryInput.Level.LevelVersion, CharacterId = Domain.Character.CharacterId,
                ExpectedCharacterRevision = Domain.Character.StateRevision, OriginalSlot = Domain.Character.OriginalSlot };
            return input;
        }

        internal CandidateApplicationBuildResult EnterBuild(CandidateApplicationSnapshot basis,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            Builds++;
            Assert.AreSame(Head, basis);
            Domain.Use(basis.Business);
            Domain.Begin();
            return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Domain.Input(), budget)),
                new CandidateApplicationResultInput { ChallengeId = Domain.EntryInput.ChallengeId,
                    AttemptId = Domain.EntryInput.AttemptId, EntryBaselineId = Domain.EntryInput.EntryBaselineId });
        }

        internal CandidateApplicationCallResult Enter()
        { return Submit(Freeze(EnterInput()), EnterBuild); }

        internal CandidateApplicationIntentInput AttackInput(string operation, int pair)
        {
            var input = Input(CandidateApplicationKind.Attack, operation);
            var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
            input.Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId,
                ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey,
                Pair = state.Enemies[pair].PairKey, Route = Domain.Routes[pair].ToList(),
                ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision, ItemUseEnabled = true };
            return input;
        }

        internal CandidateApplicationCallResult Attack(string operation, int pair)
        {
            return Submit(Freeze(AttackInput(operation, pair)), (basis, intent, budget) =>
            {
                Builds++;
                Assert.AreSame(Head, basis);
                Domain.Use(basis.Business);
                Domain.Attack(operation, pair);
                return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Domain.Input(), budget)),
                    new CandidateApplicationResultInput { HistoryAnchorId = "anchor:" + operation },
                    Domain.History.CurrentRun.FinalReport == null ? null : "settle:" + Domain.EntryInput.AttemptId);
            });
        }

        internal CandidateApplicationCallResult Rollback(string operation, string target)
        {
            Domain.Use(Head.Business);
            var h = Domain.History;
            var range = CandidateHistoryOperations.ReadRange(h, new CandidateHistoryRangeRequest { PlayerId = "player:015b",
                AttemptId = Domain.EntryInput.AttemptId, ExpectedSceneRevision = h.CurrentRun.CurrentSnapshot.SceneRevision,
                HistoryAnchorId = "anchor:" + target }, Math());
            Assert.IsTrue(range.IsAccepted, range.FieldPath);
            var input = Input(CandidateApplicationKind.Rollback, operation);
            input.Rollback = new CandidateApplicationRollbackInput { AttemptId = Domain.EntryInput.AttemptId,
                ExpectedSceneRevision = range.Range.SceneRevision, HistoryAnchorId = range.Range.HistoryAnchorId,
                TargetOperationId = range.Range.OperationId, ConfirmedRemovedOperationIds = range.Range.Entries.Select(x => x.OperationId).ToList() };
            return Submit(Freeze(input), (basis, intent, budget) =>
            {
                Builds++;
                Domain.Use(basis.Business);
                Domain.Rollback(operation, target);
                return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Domain.Input(), budget)), new CandidateApplicationResultInput());
            });
        }

        internal CandidateApplicationCallResult Win()
        {
            CandidateApplicationCallResult result = null;
            for (var n = 0; Head.Business.ActiveHistory.CurrentRun.FinalReport == null; n++)
            {
                Assert.Less(n, 20);
                var pair = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Enemies.ToList().FindIndex(x => x.Hp.Numerator.Sign > 0);
                result = Is(Attack("win:" + n, pair), "Completed");
            }
            return result;
        }

        internal CandidateApplicationIntentInput VictoryInput()
        {
            var report = Head.Business.ActiveHistory.CurrentRun.FinalReport;
            var input = Input(CandidateApplicationKind.SettleVictory, Head.Continuation.ReservedOperationId);
            input.SettleVictory = new CandidateApplicationVictoryInput { AttemptId = report.AttemptId,
                ChallengeId = report.ChallengeId, EntryBaselineId = report.EntryBaselineId,
                FinalReportFingerprint = report.Fingerprint, TerminalOperationId = report.TerminalOperationId,
                RewardDefinitionId = "reward", RewardDefinitionVersion = "candidate-r1" };
            return input;
        }

        internal CandidateApplicationBuildResult SettleBuild(CandidateApplicationSnapshot basis,
            PreparedCandidateApplicationIntent intent, SaveCodecBudget budget)
        {
            Builds++;
            Assert.AreSame(Head, basis);
            Assert.AreEqual(basis.Continuation.ReservedOperationId, intent.OperationId);
            Domain.Use(basis.Business);
            Domain.EndVictory();
            var end = Domain.Progression.Challenges.Last().Attempts.Last().End;
            return CandidateApplicationBuildResult.Success(Accept(CandidateBusinessSaveCodec.Prepare(Domain.Input(), budget)),
                new CandidateApplicationResultInput { EndReceiptId = end.EndReceiptId, SettlementId = end.SettlementId });
        }

        internal CandidateApplicationCallResult Query(PreparedCandidateApplicationIntent intent)
        { return Architecture.SendQuery(new CandidateApplicationOperationQuery(intent, B())); }
        internal CandidateApplicationCallResult Restore()
        { return Architecture.SendCommand(new RestoreCandidateApplicationCommand(B())); }
        internal CandidateApplicationCallResult Resolve(PreparedCandidateApplicationIntent intent)
        { return Architecture.SendCommand(new ResolveCandidateApplicationCommand(intent, B())); }
        internal CandidateApplicationCallResult Retry(PreparedCandidateApplicationIntent intent)
        { return Architecture.SendCommand(new RetryCandidateApplicationCommand(intent, B())); }
        internal CandidateApplicationCallResult End(PreparedCandidateApplicationIntent intent)
        { return Architecture.SendCommand(new EndCandidateApplicationCommand(intent, B())); }
        internal CandidateApplicationCallResult Resume(string commit)
        { return Architecture.SendCommand(new ResumeObservedCandidateCommand(commit, B())); }
        internal CandidateApplicationCallResult EndObserved(string commit)
        { return Architecture.SendCommand(new EndObservedCandidateCommand(commit, B())); }
        internal string Path(string name) { return Safe(global::System.IO.Path.Combine(Storage.Profile.DirectoryPath, name)); }
        internal Dictionary<string, byte[]> Disk()
        {
            return Directory.GetFiles(Storage.Profile.DirectoryPath).Where(x => global::System.IO.Path.GetFileName(x) != "writer.lock")
                .ToDictionary(global::System.IO.Path.GetFileName, File.ReadAllBytes, StringComparer.Ordinal);
        }
        internal void SameDisk(Dictionary<string, byte[]> before)
        {
            var after = Disk();
            CollectionAssert.AreEquivalent(before.Keys, after.Keys);
            foreach (var pair in before) CollectionAssert.AreEqual(pair.Value, after[pair.Key], pair.Key);
        }
        internal void Close() { Architecture.Deinit(); Closed = true; }
        public void Dispose() { if (!Closed) Close(); }

        internal static SaveRecoveryCapabilities Capabilities()
        {
            // The accepted fixture determines actual requirements; none are wildcarded.
            var s = new ApplicationScenario();
            s.Enter();
            s.Win();
            s.Settle();
            return Caps(s.Envelope);
        }
        internal static SaveRecoveryCapabilities Caps(SaveEnvelope envelope)
        {
            var r = envelope.RecoveryRequirements;
            return new SaveRecoveryCapabilities(envelope.RequiredSliceContracts, r.Bindings,
                r.RuleVersions, r.NumericContractVersions, r.RandomContractVersions, r.FeatureIds);
        }
        internal static SaveRecoveryCapabilities Without(SaveRecoveryCapabilities caps, string field)
        {
            return new SaveRecoveryCapabilities(field == "slices" ? Array.Empty<RequiredSliceContract>() : caps.ReadableSlices,
                field == "bindings" ? Array.Empty<SaveBinding>() : caps.Bindings, caps.RuleVersions, caps.NumericContractVersions,
                caps.RandomContractVersions, field == "features" ? Array.Empty<string>() : caps.FeatureIds);
        }

        internal static SaveEnvelope Repack(SaveEnvelope original, IReadOnlyList<SaveSliceInput> slices,
            IReadOnlyList<SaveCommitIndexEntry> index = null)
        {
            return Accept(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { PlayerId = original.PlayerId,
                Purpose = original.Purpose, CommitId = original.CommitId, SaveGeneration = original.SaveGeneration,
                ParentCommitId = original.ParentCommitId, CommitIndex = index ?? original.CommitIndex,
                RequiredSliceContracts = slices.Select(x => x.Contract).ToArray(), Slices = slices }, B().Codec));
        }
        internal static SnapshotDescriptor ReplacePair(string directory, SaveEnvelope envelope)
        {
            var bytes = PendingRig.Bytes(envelope);
            var descriptor = Accept(SaveEnvelopeCodec.Write(Stream.Null, envelope, B().Codec));
            File.WriteAllBytes(global::System.IO.Path.Combine(directory, SnapshotName(envelope.CommitId)), bytes);
            File.WriteAllBytes(global::System.IO.Path.Combine(directory, MarkerName(envelope.CommitId)), SaveCommitMarkerTests.IndependentMarker(descriptor));
            return descriptor;
        }
        internal SaveEnvelope ReadEnvelope(SnapshotDescriptor descriptor)
        {
            using (var stream = File.OpenRead(Path(SnapshotName(descriptor.CommitId))))
                return Accept(SaveEnvelopeCodec.Read(stream, descriptor, B().Codec));
        }
    }

    internal sealed class ApplicationFaultStorage : ILocalSaveStorage
    {
        internal readonly FaultStorage Base;
        internal Action<int> AfterMarkerEnumeration;
        internal Action<string> BeforeOpen, AfterClose, AfterDelete;
        internal bool FailLeaseClose;
        internal string PublishedCommit;
        internal int PostMarkerEnumerations, Deletes, LeaseCloses;
        internal ApplicationFaultStorage(ILocalSaveStorage real) { Base = new FaultStorage(real); }
        public SaveStorageProfile Profile => Base.Profile;
        public IDisposable AcquireWriterLease(bool createDirectory)
        { return new Lease(this, Base.AcquireWriterLease(createDirectory)); }
        public IEnumerable<string> EnumerateNames()
        {
            if (PublishedCommit != null)
            {
                PostMarkerEnumerations++;
                AfterMarkerEnumeration?.Invoke(PostMarkerEnumerations);
            }
            return Base.EnumerateNames();
        }
        public Stream OpenRead(string name)
        {
            BeforeOpen?.Invoke(name);
            return new TrackingStream(Base.OpenRead(name), point => { if (point == "Close") AfterClose?.Invoke(name); });
        }
        public Stream CreateWork(string name) { return Base.CreateWork(name); }
        public void FlushFile(Stream stream) { Base.FlushFile(stream); }
        public void PromoteNoReplace(string work, string final)
        {
            Base.PromoteNoReplace(work, final);
            if (final.EndsWith(".commit", StringComparison.Ordinal))
            {
                PublishedCommit = final.Substring(2, 32);
                PostMarkerEnumerations = 0;
            }
        }
        public void DeleteUncommitted(string name)
        {
            Base.DeleteUncommitted(name);
            Deletes++;
            AfterDelete?.Invoke(name);
        }
        public void DeleteIndexedOld(string name) { Base.DeleteIndexedOld(name); }
        private sealed class Lease : IDisposable
        {
            private readonly ApplicationFaultStorage owner;
            private readonly IDisposable inner;
            internal Lease(ApplicationFaultStorage owner, IDisposable inner) { this.owner = owner; this.inner = inner; }
            public void Dispose()
            {
                owner.LeaseCloses++;
                inner.Dispose();
                if (owner.FailLeaseClose) throw new IOException("application lease close");
            }
        }
    }
}
