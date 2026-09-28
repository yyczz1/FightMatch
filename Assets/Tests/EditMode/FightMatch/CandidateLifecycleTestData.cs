using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    internal sealed class LifecycleRig : IDisposable
    {
        internal readonly string Root;
        internal readonly BusinessSaveScenario Fixture;
        internal readonly CandidateLifecycleContentInput Content;
        internal readonly IArchitecture Architecture;
        internal readonly CandidateApplicationSystem Application;
        internal readonly CandidateLifecycleApplicationSystem System;
        internal readonly CandidateBattleApplicationSystem Battle;
        internal readonly PreparedCandidateLifecycleRequest Profile;
        internal readonly ApplicationFaultStorage Storage;
        internal readonly SaveRecoveryCapabilities Caps;
        internal bool Closed;
        internal CandidateApplicationSnapshot Head => Application.QueryView().View.PublishedSnapshot;
        internal BattleSnapshot State => Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
        internal LifecycleRig(bool initialize = true, int level = 1, int hp = 100, int experience = 0, string root = null)
        {
            Root = root ?? NewCase(); Fixture = New(level, hp: hp); Content = MakeContent(Fixture);
            Architecture = FightMatchDemoArchitecture.Interface;
            Application = Architecture.GetSystem<CandidateApplicationSystem>(); System = Architecture.GetSystem<CandidateLifecycleApplicationSystem>();
            Battle = Architecture.GetSystem<CandidateBattleApplicationSystem>();
            try
            {
                Profile = Accepted(System.PrepareNewProfile(Content, new CandidateApplicationInitializeInput { CharacterId = "W", ClassId = "warrior",
                    InitialLevel = 1, InitialExperience = experience, OriginalSlot = 2 }, B().Codec));
                Storage = new ApplicationFaultStorage(CreateStorage(Root, Profile.PlayerId, SavePurpose.CandidateValidation));
                var coverage = new ApplicationScenario(level); coverage.Enter(); coverage.Win(); coverage.Settle(); Caps = ApplicationRuntimeRig.Caps(coverage.Envelope);
                if (initialize) Is(System.OpenNewProfile(Profile, Storage, Caps, B()), "Completed");
            }
            catch { Dispose(); throw; }
        }
        internal static CandidateLifecycleContentInput MakeContent(BusinessSaveScenario f)
        {
            var first = new BattleEntryPreparer().PrepareCandidate(f.EntryInput, Math()); Assert.IsTrue(first.IsAccepted, first.FieldPath);
            var id = f.EntryInput.Level.LevelId;
            f.EntryInput.Level.LevelId = "next-level";
            var next = new BattleEntryPreparer().PrepareCandidate(f.EntryInput, Math()); Assert.IsTrue(next.IsAccepted, next.FieldPath);
            f.EntryInput.Level.LevelId = id;
            var levels = new[] { first.Entry.Level, next.Entry.Level };
            var rewards = levels.Select(l => CandidateBaseRewards.PrepareDefinition(new CandidateRewardDefinitionInput {
                Context = f.Context, RewardDefinitionId = "reward:" + l.LevelId, Version = "candidate-r1", LevelId = l.LevelId, LevelVersion = l.LevelVersion,
                BaseExperience = 20, DamageWeight = R(1), TakenWeight = R(1, 4), CurveBase = R(1, 2), CurveLog = R(1, 2), ReferenceHpDivisor = R(2),
                LevelPenaltyBase = R(3, 4), OverlevelGrace = 1, ZeroContributionPolicy = CandidateZeroContributionPolicy.CurveIntercept,
                DropMode = CandidateRewardDropMode.FixedOrdinaryMaterials, RequiredFeatures = new List<string>(), Materials = new List<CandidateRewardMaterialInput> {
                    new CandidateRewardMaterialInput { ItemId = "candidate:tin", Amount = 2 }, new CandidateRewardMaterialInput { ItemId = "candidate:wood", Amount = 0 } } }, Math()).Definition).ToList();
            Assert.IsTrue(rewards.All(x => x != null));
            return new CandidateLifecycleContentInput { Growth = f.Character.Definition, Inventory = f.Inventory.Definition, Progression = f.Progression.Definition,
                Levels = levels.ToList(), Rewards = rewards, CritCoefficients = new List<CandidateCritCoefficient> {
                    new CandidateCritCoefficient(R(1, 5), R(1, 1000)), new CandidateCritCoefficient(R(41, 200), R(2, 1000)) } };
        }
        internal static PreparedCandidateLifecycleRequest Accepted(CandidateLifecyclePrepareResult result)
        { Assert.IsTrue(result.IsAccepted, result.Code + " " + result.Diagnostic?.FieldPath + " " + result.Diagnostic?.LimitReason); return result.Request; }
        internal static CandidateApplicationCallResult Is(CandidateApplicationCallResult result, string code)
        { return ApplicationRuntimeRig.Is(result, code); }
        internal static void BuilderRefusal(CandidateApplicationCallResult result, string code)
        { Is(result, "BuilderRejected"); Assert.IsFalse(result.IsCommitted); Assert.AreEqual(code, result.Diagnostic.Code); Assert.IsNotEmpty(result.Diagnostic.FieldPath); }
        internal CandidateLifecycleDraft Draft(CandidateApplicationKind kind)
        { return new CandidateLifecycleDraft { Kind = kind, PlayerId = Profile.PlayerId, ExpectedCommitId = Head.Header.CommitId, Content = Content }; }
        internal CandidateLifecycleDraft EntryDraft(string levelId = null)
        {
            var d = Draft(CandidateApplicationKind.EnterAttempt); var c = Head.Business.Character;
            d.EnterAttempt = new CandidateApplicationEnterInput { LevelId = levelId ?? Fixture.EntryInput.Level.LevelId, LevelVersion = "candidate-r1",
                CharacterId = c.CharacterId, ExpectedCharacterRevision = c.StateRevision, OriginalSlot = c.OriginalSlot }; return d;
        }
        internal PreparedCandidateLifecycleRequest Freeze(CandidateLifecycleDraft d) { return Accepted(System.Prepare(d, B().Codec)); }
        internal PreparedCandidateLifecycleRequest Enter()
        { var request = Freeze(EntryDraft()); Is(System.Submit(request, B()), "Completed"); return request; }
        internal CandidateLifecycleDraft EndDraft(bool restart = false, CandidateTimeSample time = null)
        {
            var e = State.Baseline.Entry; var d = Draft(restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt);
            var input = new CandidateApplicationEndInput { AttemptId = e.AttemptId, ChallengeId = e.ChallengeId,
                EntryBaselineId = e.EntryBaselineId, ExpectedSceneRevision = State.SceneRevision };
            if (restart) d.RestartAttempt = input; else d.ExitAttempt = input; d.EndTimeSample = time; return d;
        }
        internal CandidateBattleCallResult Attack(int pair = 0, bool finish = true)
        {
            var s = State;
            var draft = new CandidateBattleDraft { PlayerId = Profile.PlayerId, ExpectedCommitId = Head.Header.CommitId, Context = Fixture.Context,
                Kind = CandidateApplicationKind.Attack, Attack = new CandidateApplicationAttackInput { AttemptId = s.Baseline.Entry.AttemptId,
                    ExpectedSceneRevision = s.SceneRevision, ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision, ItemUseEnabled = false,
                    Actor = s.Members[0].CombatantKey, Pair = s.Enemies[pair].PairKey, Route = Fixture.Routes[pair].ToList() } };
            var prepared = Battle.Prepare(draft, B().Codec); Assert.IsTrue(prepared.IsAccepted, prepared.Code);
            var result = BattleApplicationRig.Is(Battle.Submit(prepared.Request, B()), "Completed");
            if (finish) BattleApplicationRig.Is(Battle.ReportPresentationCompleted(result.Presentation.Token), "PresentationCompleted");
            return result;
        }
        internal void Win(bool finishLast = true)
        {
            for (var i = 0; Head.Business.ActiveHistory.CurrentRun.FinalReport == null; i++)
            { Assert.Less(i, 20); Attack(State.Enemies.ToList().FindIndex(x => x.Hp.Numerator.Sign > 0), finishLast); if (!finishLast && State.Phase != BattlePhase.WonPendingSettlement) Battle.RebuildLatest(); }
        }
        internal PreparedCandidateLifecycleRequest Victory()
        { return Accepted(System.PrepareVictory(Content, null, B().Codec)); }
        internal static CandidateTimeSample Time(int wall, int? mono = null, string scope = "clock:024")
        { return new CandidateTimeSample { WallUtcMilliseconds = wall, ObservedAtUtcMilliseconds = wall + 1,
            MonotonicElapsedMilliseconds = mono.HasValue ? R(mono.Value) : null, MonotonicScopeId = mono.HasValue ? scope : null,
            Source = "explicit-candidate-device-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None }; }
        internal void DownAndExit()
        {
            Enter();
            for (var i = 0; State.Members[0].Hp.Numerator.Sign > 0; i++)
            { Assert.Less(i, 10); Attack(0); }
            Assert.AreEqual(BattlePhase.AwaitRescue, State.Phase);
            Is(System.Submit(Freeze(EndDraft(time: Time(100, 0))), B()), "Completed");
            Assert.IsNotNull(Head.Business.Character.ActiveRecovery);
        }
        internal CandidateLifecycleDraft RecoveryDraft(CandidateTimeSample time)
        {
            var d = Draft(CandidateApplicationKind.AdvanceRecovery); var c = Head.Business.Character;
            d.AdvanceRecovery = new CandidateApplicationRecoveryInput { CharacterId = c.CharacterId, RecoveryId = c.ActiveRecovery?.RecoveryId ?? c.RecoveryPeriods.Last().RecoveryId,
                ExpectedCharacterRevision = c.StateRevision, TimeSample = time }; return d;
        }
        internal CandidateApplicationSnapshot Restore()
        { var commit = Head.Header.CommitId; Is(Application.Restore(B()), "Ready"); Assert.AreEqual(commit, Head.Header.CommitId); return Head; }
        internal Dictionary<string, byte[]> Disk()
        { return Directory.GetFiles(Storage.Profile.DirectoryPath).Where(x => Path.GetFileName(x) != "writer.lock").ToDictionary(Path.GetFileName, File.ReadAllBytes); }
        internal void SameDisk(Dictionary<string, byte[]> before)
        { var after = Disk(); CollectionAssert.AreEquivalent(before.Keys, after.Keys); foreach (var row in before) CollectionAssert.AreEqual(row.Value, after[row.Key]); }
        internal void Close() { Architecture.Deinit(); Closed = true; }
        public void Dispose() { if (!Closed) Close(); }
    }
}
