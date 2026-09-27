using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.PlayerSessionTestData;

namespace FightMatch.Core.Tests
{
    public class PlayerSessionTests
    {
        [Test] public void PublishedRecipeAndCoefficientsDriveRealGuidProfileAndVerifiedViews()
        {
            using (var r = new Rig())
            {
                Assert.IsTrue(Guid.TryParse(r.Profile.PlayerId, out _)); Assert.IsTrue(Guid.TryParse(r.Profile.OperationId, out _));
                Assert.AreNotEqual(r.Profile.PlayerId, r.Profile.OperationId);
                Assert.AreEqual(31, r.Publication.Parameters.Count);
                Assert.IsTrue(((IList<DemoParameterEvidence>)r.Publication.Parameters).IsReadOnly);
                var recipe = r.Publication.NewProfile; var character = r.Head.Business.Character;
                Assert.AreEqual(recipe.CharacterId, character.CharacterId); Assert.AreEqual(recipe.Level, character.Level);
                Assert.AreEqual(recipe.Experience, character.Experience); Assert.AreEqual(recipe.OriginalSlot, character.OriginalSlot);
                Assert.AreEqual(SavePurpose.PlayerSave, r.Application.QueryView().View.Purpose);
                Assert.AreEqual(SavePurpose.PlayerSave, r.Battle.QueryView().Purpose);
                Assert.IsTrue(r.Publication.Binding.Same(r.Lifecycle.QueryView().ContentBinding));
                Assert.IsNull(r.Head.Business.ActiveHistory); Assert.IsNull(character.ActiveRecovery);
                Assert.IsTrue(r.Locator.Read(Budget()).Observation.State == LocalPlayerProfileState.Active);
                var next = Prepared(r.Session.PrepareNewProfile(r.Catalog, Release, Codec()));
                Assert.AreNotEqual(r.Profile.PlayerId, next.PlayerId); Assert.AreNotEqual(r.Profile.OperationId, next.OperationId);
            }
        }
        [Test] public void PublicBuilderCannotWriteFormalStateAndReentrantOrWrongThreadRequestsAreRefused()
        {
            using (var r = new Rig())
            {
                var entry = r.EntryRequest(); var before = CopyFiles(r.Storage.Files); var calls = 0;
                var denied = r.Application.Submit(entry.Intent, (s, i, b) => { calls++; return null; }, Budget());
                Is(denied, "UnsupportedBinding"); Assert.AreEqual(0, calls); SameFiles(before, r.Storage.Files);
                var other = Task.Run(() => r.Session.PrepareNewProfile(r.Catalog, Release, Codec())).GetAwaiter().GetResult();
                Assert.AreEqual("WrongThread", other.Code); SameFiles(before, r.Storage.Files);
                CandidateApplicationCallResult nested = null;
                r.Architecture.RegisterEvent<CandidateApplicationPublished>(_ => nested = r.Session.CreateNew(r.Profile, r.Storage, r.Locator, r.Capabilities, Budget()));
                Is(r.Lifecycle.Submit(entry, Budget())); Assert.AreEqual("Busy", nested.Code);
                Assert.AreEqual(2, r.Head.Records.Count);
            }
        }
        [Test] public void RealH02AttackS17RewardAndReopenedHeadUseTheSameRulesExactlyOnce()
        {
            using (var r = new Rig())
            {
                r.Enter(); var entry = r.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry;
                var binding = r.Head.Business.ActiveHistory.Binding;
                Assert.AreEqual("local:System.Security.Cryptography.RandomNumberGenerator", binding.SourceCapabilityId);
                Assert.AreEqual("level:ch01-01", entry.Level.LevelId); Assert.AreEqual(BigInteger.One, entry.Members[0].Level);
                r.Win(); var won = r.Head; var report = won.Business.ActiveHistory.CurrentRun.FinalReport;
                Assert.IsNotNull(report); Assert.IsNotNull(won.Continuation);
                var request = Prepared(r.Session.PrepareVictory(new LocalPlayerClock().Read(), Codec()));
                Assert.AreEqual(won.Continuation.ReservedOperationId, request.OperationId);
                var result = Is(r.Lifecycle.Submit(request, Budget())); var settled = r.Head;
                var reward = settled.Business.Rewards.BaseRewards.Single();
                Witness(r, "settled-h06");
                Assert.AreEqual(report.Fingerprint, reward.FinalReportFingerprint);
                Assert.AreEqual(reward.Experience.Single().Amount, settled.Business.Character.Experience);
                Assert.AreEqual(new BigInteger(2), settled.Business.Inventory.Holdings.Single(h => h.ItemId == "item:tin").T);
                Assert.IsTrue(settled.Business.Inventory.Holdings.Where(h => h.ItemId != "item:tin").All(h => h.T.IsZero));
                Assert.AreEqual(1, settled.Business.Progression.FirstClears.Count); Assert.IsNull(settled.Business.ActiveHistory);
                var disk = CopyFiles(r.Storage.Files); var writes = r.Storage.SnapshotCreates;
                Is(r.Lifecycle.Submit(request, Budget())); Is(r.Lifecycle.Retry(request, Budget())); Is(r.Lifecycle.Resolve(request, Budget()));
                Assert.AreSame(settled, r.Head); Assert.AreEqual(writes, r.Storage.SnapshotCreates); SameFiles(disk, r.Storage.Files);
                var active = r.Locator.Read(Budget()).Observation.ActiveProfile; r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var lifecycle = arch.GetSystem<CandidateLifecycleApplicationSystem>();
                    var reopened = Is(session.OpenExisting(active, r.Storage, r.Catalog, r.Capabilities, Budget()), "Ready");
                    Assert.AreEqual(settled.Header.CommitId, reopened.View.PublishedSnapshot.Header.CommitId);
                    Assert.AreEqual(result.OriginalCommitId, Is(lifecycle.Submit(request, Budget())).OriginalCommitId);
                    Assert.AreEqual(reward.Experience.Single().Amount, reopened.View.PublishedSnapshot.Business.Character.Experience);
                    SameFiles(disk, r.Storage.Files);
                }
                finally { arch.Deinit(); }
            }
        }
        [Test] public void RestartKeepsOriginalEntryFactsAndThreeRandomInitialsWhileNormalReentrySamplesAgain()
        {
            using (var r = new Rig())
            {
                r.Enter(); var original = r.Head.Business.ActiveHistory.CurrentRun;
                var attacked = r.Battle.Submit(r.AttackRequest(), Budget()); Is(attacked.Application);
                if (attacked.Presentation != null) r.Battle.ReportPresentationCompleted(attacked.Presentation.Token);
                var restart = r.EndRequest(true); Is(r.Lifecycle.Submit(restart, Budget()));
                var next = r.Head.Business.ActiveHistory.CurrentRun;
                Assert.AreNotEqual(original.Baseline.Entry.AttemptId, next.Baseline.Entry.AttemptId);
                Assert.AreEqual(original.Baseline.Entry.EntryBaselineId, next.Baseline.Entry.EntryBaselineId);
                Assert.AreEqual(original.Baseline.Entry.ChallengeId, next.Baseline.Entry.ChallengeId);
                foreach (var pair in new[] { new[] { original.Binding.Battle, next.Binding.Battle }, new[] { original.Binding.BaseReward, next.Binding.BaseReward }, new[] { original.Binding.Bonus, next.Binding.Bonus } })
                { Assert.AreEqual(pair[0].InitState, pair[1].InitState); Assert.AreEqual(pair[0].InitSequence, pair[1].InitSequence); }
                Assert.AreEqual(0, original.InitialSnapshot.Members[0].Hp.Compare(next.InitialSnapshot.Members[0].Hp, Codec().Math));
                Assert.AreEqual(BigInteger.Zero, next.CurrentSnapshot.Random.Stream.WordsConsumed); Assert.IsEmpty(next.Records);
                var frozen = Encode(Envelope(r.Storage, r.Head.Header.CommitId)); Is(r.Application.Restore(Budget()), "Ready");
                CollectionAssert.AreEqual(frozen, Encode(Envelope(r.Storage, r.Head.Header.CommitId)));
                Is(r.Lifecycle.Submit(r.EndRequest(false), Budget())); r.Enter();
                var fresh = r.Head.Business.ActiveHistory.Binding;
                Assert.AreNotEqual(original.Binding.Battle.InitState + ":" + original.Binding.Battle.InitSequence,
                    fresh.Battle.InitState + ":" + fresh.Battle.InitSequence, "normal entry must obtain new platform entropy");
                Assert.IsEmpty(r.Head.Business.Rewards.BaseRewards);
            }
        }
        [Test] public void MultiLevelV2UsesTheSameFacadeWhileExistingV1KeepsItsExactPackage()
        {
            var fixture = new PublishedFixture(); var caps = Caps(fixture.V1, fixture.V2);
            using (var r = new Rig(catalog: fixture.Catalog, publication: fixture.V1, capabilities: caps, source: fixture.Source))
            {
                r.Enter(); var head = r.Head; var active = r.Locator.Read(Budget()).Observation.ActiveProfile;
                fixture.Activate(fixture.V2); r.Close();
                var arch = FightMatchDemoArchitecture.Interface;
                try
                {
                    var session = arch.GetSystem<PlayerSessionSystem>(); var opened = Is(session.OpenExisting(active, r.Storage, fixture.Catalog, caps, Budget()), "Ready");
                    Assert.AreEqual(head.Header.CommitId, opened.View.PublishedSnapshot.Header.CommitId);
                    Assert.IsTrue(((PreparedPublishedRuleContext)opened.View.PublishedSnapshot.Business.Character.Definition.Context).Binding.Same(fixture.V1.Binding));
                    var newProfile = Prepared(session.PrepareNewProfile(fixture.Catalog, Release, Codec()));
                    Assert.IsTrue(newProfile.CreateRecord.ContentBinding.Same(fixture.V2.Binding));
                }
                finally { arch.Deinit(); }
            }
            using (var r = new Rig(catalog: fixture.Catalog, publication: fixture.V2, capabilities: caps, source: PublishedContentTestData.Fixture(2, true)))
            {
                Assert.AreEqual(2, r.Publication.Definitions.Levels.Count);
                Is(r.Lifecycle.Submit(r.EntryRequest("fixture:beta"), Budget()));
                Assert.AreEqual("fixture:beta", r.Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Level.LevelId);
                Assert.IsTrue(r.Lifecycle.QueryView().ContentBinding.Same(fixture.V2.Binding));
            }
        }
        [Test] public void DeviceClockHasProcessScopedMonotonicEvidenceAndReadingViewsDoesNotWrite()
        {
            var first = new LocalPlayerClock().Read(); var second = new LocalPlayerClock().Read();
            Assert.AreEqual(CandidateTimeTrust.DeviceUntrusted, first.Trust); Assert.AreEqual(CandidateTimeAnomaly.None, first.Anomaly);
            Assert.IsTrue(Guid.TryParseExact(first.MonotonicScopeId, "N", out _)); Assert.AreEqual(first.MonotonicScopeId, second.MonotonicScopeId);
            Assert.GreaterOrEqual(second.MonotonicElapsedMilliseconds.Compare(first.MonotonicElapsedMilliseconds, Codec().Math), 0);
            Assert.AreNotEqual("isolated:P1", first.Source); Assert.Greater(first.WallUtcMilliseconds, BigInteger.Zero);
            using (var r = new Rig())
            { var before = CopyFiles(r.Storage.Files); r.Application.QueryView(); r.Lifecycle.QueryView(); r.Battle.QueryView(); SameFiles(before, r.Storage.Files); }
        }
        [Test] public void ClockFromAnotherProcessScopeUsesUntrustedUtcInsteadOfForeignMonotonicElapsed()
        {
            using (var r = new Rig())
            {
                var now = new LocalPlayerClock().Read(); var earlier = new CandidateTimeSample {
                    WallUtcMilliseconds = now.WallUtcMilliseconds - 1000, ObservedAtUtcMilliseconds = now.ObservedAtUtcMilliseconds - 1000,
                    MonotonicElapsedMilliseconds = ExactRational.Create(999999999, 1, Codec().Math), MonotonicScopeId = "isolated:previous-process",
                    Source = "isolated:prior-clock", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None };
                var character = r.Head.Business.Character;
                // Isolated domain fixture: the formal session itself is not supplied invented combat facts.
                var down = CandidateCharacterEnd.Propose(character, new CandidateCharacterEndFacts {
                    PlayerId = character.PlayerId, CharacterId = character.CharacterId, AttemptId = "isolated:attempt", EntryBaselineId = "isolated:entry",
                    EndReceiptId = "isolated:end", RecoveryId = "isolated:recovery", Context = new PublishedRuleContext(r.Publication.Binding),
                    Kind = CandidateCharacterEndKind.NormalExit, WasParticipant = true, WasDown = true, TimeSample = earlier }, character.StateRevision, Codec().Math);
                Assert.IsTrue(down.IsAccepted, down.RejectionCode + " " + down.FieldPath);
                var advanced = CandidateRecoveryClock.Advance(down.Next, "isolated:recovery", now, down.Next.StateRevision, Codec().Math);
                Assert.IsTrue(advanced.IsAccepted, advanced.RejectionCode + " " + advanced.FieldPath);
                Assert.AreEqual(CandidateTimeAnomaly.DomainChanged, advanced.RecoveryPeriod.Anomaly);
                Assert.AreEqual(0, advanced.RecoveryPeriod.Elapsed.Compare(ExactRational.Create(1000, 1, Codec().Math), Codec().Math));
                Assert.IsNull(r.Head.Business.Character.ActiveRecovery);
            }
        }
    }
}
