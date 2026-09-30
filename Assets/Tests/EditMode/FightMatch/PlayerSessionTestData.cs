using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using FlowPuzzle.Core;
using NUnit.Framework;
using QFramework;

namespace FightMatch.Core.Tests
{
    internal static class PlayerSessionTestData
    {
        internal const string Release = "release-set:fightmatch-demo-r1";
        internal static SaveCodecBudget Codec() => new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000));
        internal static SaveStoreBudget Budget() => new SaveStoreBudget(Codec());
        internal static T TakeCore<T>(SaveCodecResult<T> result)
        { Assert.IsTrue(result.IsAccepted, result.RejectionCode + " " + result.FieldPath + " " + result.LimitReason); return result.Value; }
        internal static T Content<T>(PublicationResult<T> result) => PublishedContentTestData.Take(result);
        internal static CandidateApplicationCallResult Is(CandidateApplicationCallResult result, string code = "Completed")
        { Assert.AreEqual(code, result.Code, result.Diagnostic?.Code + " " + result.Diagnostic?.FieldPath + " " + result.Diagnostic?.ExceptionMessage); return result; }
        internal static PreparedPlayerProfile Prepared(PreparedPlayerProfileResult result)
        { Assert.IsTrue(result.IsAccepted, result.Code + " " + result.Diagnostic?.FieldPath); return result.Request; }
        internal static PreparedCandidateLifecycleRequest Prepared(CandidateLifecyclePrepareResult result)
        { Assert.IsTrue(result.IsAccepted, result.Code + " " + result.Diagnostic?.FieldPath); return result.Request; }
        private static byte[][] realFiles;
        private static readonly object realCatalogLock = new object();
        private static PublicationResult<FirstReleaseContentStorage> realStorageResult;
        internal static PublishedContentCatalog RealCatalog()
        {
            PublicationResult<FirstReleaseContentStorage> result;
            lock (realCatalogLock)
            {
                result = realStorageResult;
                if (result == null)
                {
                    if (realFiles == null) realFiles = new[] { "fmsource.json", "fmpackage.bytes", "fmvalidation.bytes", "fmreview.json", "fmpublish.json", "fmrelease.json" }
                        .Select(s => File.ReadAllBytes("Assets/StreamingAssets/FightMatch/first-release." + s)).ToArray();
                    result = FirstReleaseContentStorage.Create(realFiles[0], realFiles[1], realFiles[2], realFiles[3], realFiles[4], realFiles[5],
                        ContentConsumerCapabilities.Current, PublishedContentTestData.StoreBudget());
                    if (result.IsAccepted) realStorageResult = result;
                }
            }
            return new PublishedContentCatalog(Content(result), ContentConsumerCapabilities.Current);
        }
        internal static PublishedSource RealSource() => Content(PublishedContentCodec.DecodeSource(
            File.ReadAllBytes("Assets/StreamingAssets/FightMatch/first-release.fmsource.json"), ContentConsumerCapabilities.Current, PublishedContentTestData.Math()));
        internal static ResolvedPublication Resolve(PublishedContentCatalog catalog, string release = Release)
        { return Content(catalog.ResolveExact(Content(catalog.GetCurrentBinding("player", release, ContentConsumerCapabilities.Current)), ContentConsumerCapabilities.Current)); }
        internal static SaveRecoveryCapabilities Caps(params ResolvedPublication[] publications)
        {
            var bindings = new List<SaveBinding>();
            foreach (var p in publications)
            {
                var b = p.Binding;
                bindings.Add(new SaveBinding(SaveBindingKind.Content, b.PackageId, null, null, b.ContentFingerprint, b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion, null, null, null));
                foreach (var l in p.Definitions.Levels) bindings.Add(new SaveBinding(SaveBindingKind.Definition, b.PackageId, null, null, b.ContentFingerprint,
                    b.RuleVersion, b.NumericContractVersion, b.RandomContractVersion, null, l.LevelId, l.LevelVersion));
            }
            var names = new[] { "application", "character", "inventory", "progression", "battle", "rewards" };
            return new SaveRecoveryCapabilities(names.Select((n, i) => new RequiredSliceContract("fm.m0" + (i + 2) + "." + n, "M0" + (i + 2), 2)).ToArray(),
                bindings, publications.Select(p => p.Binding.RuleVersion).Distinct().ToArray(), publications.Select(p => p.Binding.NumericContractVersion).Distinct().ToArray(),
                publications.Select(p => p.Binding.RandomContractVersion).Distinct().ToArray(), new[] { "fm.player.application.v1" });
        }
        internal static SaveEnvelope Envelope(MemorySave storage, string commit)
        { using (var stream = storage.OpenRead("c-" + commit + ".snapshot")) {
            var summary = TakeCore(SaveEnvelopeCodec.ReadUncommittedRequirements(stream, Codec())); stream.Position = 0;
            return TakeCore(SaveEnvelopeCodec.Read(stream, summary.Descriptor, Codec())); } }
        internal static PublishedSaveContext Closure(params ResolvedPublication[] publications)
        { return TakeCore(PublishedSaveContext.Prepare(publications.Select(p => p.Definitions).ToArray(), Codec())); }
        internal static byte[] Encode(SaveEnvelope value)
        { using (var stream = new MemoryStream()) { TakeCore(SaveEnvelopeCodec.Write(stream, value, Codec())); return stream.ToArray(); } }
        internal static SaveEnvelope Mutate(SaveEnvelope envelope, SavePurpose? purpose = null, uint? schema = null, bool removeFeature = false, bool badBody = false)
        { return TakeCore(Repack(envelope, purpose, schema, removeFeature, badBody)); }
        internal static SaveCodecResult<SaveEnvelope> Repack(SaveEnvelope envelope, SavePurpose? purpose = null, uint? schema = null, bool removeFeature = false, bool badBody = false)
        {
            var slices = envelope.SliceDirectory.Select((s, i) => new SaveSliceInput {
                Contract = new RequiredSliceContract(s.Contract.SliceId, s.Contract.OwnerId, schema ?? s.Contract.SchemaVersion), Bytes = envelope.SliceBytes[i].ToArray(),
                Requirements = removeFeature && i == 0 ? new SaveRequirements(s.Requirements.Bindings, s.Requirements.RuleVersions,
                    s.Requirements.NumericContractVersions, s.Requirements.RandomContractVersions, Array.Empty<string>()) : s.Requirements }).ToArray();
            if (badBody) slices[1].Bytes[slices[1].Bytes.Length - 1] ^= 127;
            return SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { PlayerId = envelope.PlayerId, Purpose = purpose ?? envelope.Purpose,
                SaveGeneration = envelope.SaveGeneration, CommitId = envelope.CommitId, ParentCommitId = envelope.ParentCommitId, CommitIndex = envelope.CommitIndex,
                RequiredSliceContracts = slices.Select(s => s.Contract).ToArray(), Slices = slices }, Codec());
        }
        internal static Dictionary<string, byte[]> CopyFiles(Dictionary<string, byte[]> files)
        { return files.ToDictionary(x => x.Key, x => (byte[])x.Value.Clone(), StringComparer.Ordinal); }
        internal static void SameFiles(Dictionary<string, byte[]> expected, Dictionary<string, byte[]> actual)
        { CollectionAssert.AreEquivalent(expected.Keys, actual.Keys); foreach (var row in expected) CollectionAssert.AreEqual(row.Value, actual[row.Key], row.Key); }
        internal static void Witness(Rig rig, string stage)
        {
            var view = rig.Application.QueryView().View; var head = view.PublishedSnapshot; var record = rig.Profile.CreateRecord;
            var history = head?.Business.ActiveHistory; var reward = head?.Business.Rewards.BaseRewards.LastOrDefault();
            var fields = new Dictionary<string, string> { ["stage"] = stage, ["playerId"] = record.PlayerId, ["createOperationId"] = record.OperationId,
                ["recordSha256"] = record.RecordSha256, ["intentSha256"] = record.IntentSha256, ["definitionSha256"] = record.DefinitionSha256,
                ["packageId"] = record.ContentBinding.PackageId, ["packageSha256"] = record.ContentBinding.ContentFingerprint,
                ["newProfileId"] = record.NewProfileDefinitionId, ["newProfileVersion"] = record.NewProfileDefinitionVersion.ToString(),
                ["phase"] = view.Phase.ToString(), ["pendingCommit"] = view.PendingCommitId, ["head"] = head?.Header.CommitId,
                ["initialCommit"] = head?.Records[0].CommitId, ["generation"] = head?.Header.SaveGeneration.ToString(),
                ["rewardCount"] = head?.Business.Rewards.BaseRewards.Count.ToString(), ["experience"] = head?.Business.Character.Experience.ToString(),
                ["firstClears"] = head?.Business.Progression.FirstClears.Count.ToString(), ["activeAttempt"] = history?.CurrentRun.Baseline.Entry.AttemptId,
                ["entropySource"] = history?.Binding.SourceCapabilityId, ["battleWords"] = history?.CurrentRun.CurrentSnapshot.Random.Stream.WordsConsumed.ToString(),
                ["reportSha256"] = history?.CurrentRun.FinalReport?.Fingerprint ?? reward?.FinalReportFingerprint,
                ["rewardExperience"] = reward?.Experience.Single().Amount.ToString(),
                ["snapshotSha256"] = head == null ? null : PublishedContentCodec.Sha256(Encode(Envelope(rig.Storage, head.Header.CommitId))) };
            TestContext.WriteLine("P2C-WITNESS " + string.Join(";", fields.Select(x => x.Key + "=" + Uri.EscapeDataString(x.Value ?? ""))));
        }
        internal sealed class PublishedFixture
        {
            internal readonly PublishedContentTestData.MemoryStorage Storage = new PublishedContentTestData.MemoryStorage();
            internal readonly PublishedContentCatalog Catalog;
            internal readonly ResolvedPublication V1, V2;
            internal readonly PublishedSource Source;
            internal PublishedFixture()
            {
                Catalog = new PublishedContentCatalog(Storage, ContentConsumerCapabilities.Current);
                Source = PublishedContentTestData.Fixture(); V1 = Publish(Source, "fixture:publish:1");
                V2 = Publish(PublishedContentTestData.Fixture(2, true), "fixture:publish:2"); Activate(V1);
            }
            private ResolvedPublication Publish(PublishedSource source, string operation)
            {
                var job = new DemoContentDraft(source.DraftId).BeginJob(); var p = PublishedContentTestData.Prepare(source, job);
                return Content(Catalog.Publish(p, p.Validation, PublishedContentTestData.Review(p), job, operation, PublishedContentTestData.StoreBudget())).Publication;
            }
            internal void Activate(ResolvedPublication publication)
            {
                // Isolated activation fixture only; production has immutable independently signed release records.
                Storage.Blobs[PublishedContentCatalog.ReleaseSetKey("player", Release)] = Content(PublishedContentCodec.EncodeReleaseSet(new ContentReleaseSet {
                    SchemaVersion = 1, Scope = "player", ReleaseSetId = Release, Binding = ContentBindingRecord.From(publication.Binding),
                    PublicationReceiptSha256 = PublishedContentCodec.Sha256(publication.ReceiptBytes.ToArray()) }, PublishedContentTestData.StoreBudget()));
            }
        }
        internal sealed class Rig : IDisposable
        {
            internal readonly IArchitecture Architecture;
            internal readonly CandidateApplicationSystem Application;
            internal readonly CandidateLifecycleApplicationSystem Lifecycle;
            internal readonly CandidateBattleApplicationSystem Battle;
            internal readonly PlayerSessionSystem Session;
            internal readonly PublishedContentCatalog Catalog;
            internal readonly ResolvedPublication Publication;
            internal readonly SaveRecoveryCapabilities Capabilities;
            internal readonly PublishedContentTestData.MemoryStorage LocatorStorage;
            internal readonly LocalPlayerProfileLocator Locator;
            internal readonly PreparedPlayerProfile Profile;
            internal readonly MemorySave Storage;
            internal readonly PublishedSource Source;
            private bool closed;
            internal CandidateApplicationSnapshot Head => Application.QueryView().View.PublishedSnapshot;
            internal Rig(bool initialize = true, PublishedContentCatalog catalog = null, ResolvedPublication publication = null,
                SaveRecoveryCapabilities capabilities = null, MemorySave storage = null, PublishedContentTestData.MemoryStorage locatorStorage = null,
                bool recover = false, PublishedSource source = null)
            {
                Catalog = catalog ?? RealCatalog(); Publication = publication ?? Resolve(Catalog); Capabilities = capabilities ?? Caps(Publication);
                LocatorStorage = locatorStorage ?? new PublishedContentTestData.MemoryStorage(); Locator = new LocalPlayerProfileLocator(LocatorStorage);
                Source = source ?? RealSource(); Architecture = FightMatchDemoArchitecture.Interface;
                Application = Architecture.GetSystem<CandidateApplicationSystem>(); Lifecycle = Architecture.GetSystem<CandidateLifecycleApplicationSystem>();
                Battle = Architecture.GetSystem<CandidateBattleApplicationSystem>(); Session = Architecture.GetSystem<PlayerSessionSystem>();
                try
                {
                    Profile = recover ? Prepared(Session.RecoverCreateIntent(Locator.Read(Budget()).Observation, Catalog, Codec()))
                        : Prepared(Session.PrepareNewProfile(Catalog, Release, Codec()));
                    Storage = storage ?? new MemorySave(Profile.PlayerId);
                    if (initialize) Is(Session.CreateNew(Profile, Storage, Locator, Capabilities, Budget()));
                    Witness(this, recover ? "recovered-original" : initialize ? "created-active" : "prepared-new");
                }
                catch { Dispose(); throw; }
            }
            internal PreparedCandidateLifecycleRequest EntryRequest(string level = null)
            {
                var c = Head.Business.Character; var l = Publication.Definitions.Levels.Single(x => x.LevelId == (level ?? Publication.Definitions.Levels[0].LevelId));
                return Prepared(Session.PrepareLifecycle(new PlayerLifecycleDraft { ExpectedCommitId = Head.Header.CommitId,
                    Kind = CandidateApplicationKind.EnterAttempt, EnterAttempt = new CandidateApplicationEnterInput { LevelId = l.LevelId, LevelVersion = l.LevelVersion,
                        CharacterId = c.CharacterId, ExpectedCharacterRevision = c.StateRevision, OriginalSlot = c.OriginalSlot } }, Codec()));
            }
            internal PreparedCandidateLifecycleRequest Enter()
            { var request = EntryRequest(); Is(Lifecycle.Submit(request, Budget())); Witness(this, "entered-h02"); return request; }
            internal PreparedCandidateLifecycleRequest EndRequest(bool restart)
            {
                var s = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot; var e = s.Baseline.Entry;
                var input = new CandidateApplicationEndInput { AttemptId = e.AttemptId, ChallengeId = e.ChallengeId,
                    EntryBaselineId = e.EntryBaselineId, ExpectedSceneRevision = s.SceneRevision };
                return Prepared(Session.PrepareLifecycle(new PlayerLifecycleDraft { ExpectedCommitId = Head.Header.CommitId,
                    Kind = restart ? CandidateApplicationKind.RestartAttempt : CandidateApplicationKind.ExitAttempt,
                    RestartAttempt = restart ? input : null, ExitAttempt = restart ? null : input,
                    EndTimeSample = restart ? null : new LocalPlayerClock().Read() }, Codec()));
            }
            private List<FlowPos> Route(BattlePairKey pair)
            {
                var face = Head.Business.ActiveHistory.CurrentRun.Baseline.Entry.Level.Faces.Single(f => f.FaceId == pair.FaceId);
                var cells = Source.Levels[0].SourceRoutes.Single(r => r.FaceId == pair.FaceId && r.PairId == pair.PairId).Cells;
                // Published source coordinates are one-based; the prepared board uses the compiler's zero-based mapping.
                return cells.Select(c => new FlowPos(c.x - 1, Source.Coordinates == DemoCoordinateCandidate.AssumedBottomLeft ? c.y - 1 : face.Height - c.y)).ToList();
            }
            internal PreparedCandidateBattleRequest AttackRequest()
            {
                var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot;
                var target = state.Enemies.First(x => x.Hp.Numerator.Sign > 0);
                var route = Route(target.PairKey);
                var prepared = Battle.Prepare(new CandidateBattleDraft { PlayerId = Profile.PlayerId, ExpectedCommitId = Head.Header.CommitId,
                    Context = new PublishedRuleContext(Publication.Binding), Kind = CandidateApplicationKind.Attack,
                    Attack = new CandidateApplicationAttackInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision,
                        Actor = state.Members[0].CombatantKey, Pair = target.PairKey, Route = route, ExpectedPreferenceRevision = Head.Business.Inventory.PreferenceRevision,
                        ItemUseEnabled = false } }, Codec());
                Assert.IsTrue(prepared.IsAccepted, prepared.Code + " " + prepared.Diagnostic?.FieldPath); return prepared.Request;
            }
            internal void Win()
            {
                var count = 0;
                while (Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot.Phase != BattlePhase.WonPendingSettlement)
                {
                    Assert.Less(count++, 32); var state = Head.Business.ActiveHistory.CurrentRun.CurrentSnapshot; PreparedCandidateBattleRequest request;
                    if (state.Phase == BattlePhase.AwaitLinks)
                    {
                        var pair = state.Board.PendingLinks[0]; var route = Route(pair);
                        var result = Battle.Prepare(new CandidateBattleDraft { PlayerId = Profile.PlayerId, ExpectedCommitId = Head.Header.CommitId,
                            Context = new PublishedRuleContext(Publication.Binding), Kind = CandidateApplicationKind.Link,
                            Link = new CandidateApplicationLinkInput { AttemptId = state.Baseline.Entry.AttemptId, ExpectedSceneRevision = state.SceneRevision, Pair = pair, Route = route } }, Codec());
                        Assert.IsTrue(result.IsAccepted, result.Code); request = result.Request;
                    }
                    else { Assert.AreEqual(BattlePhase.AwaitAction, state.Phase); request = AttackRequest(); }
                    var call = Battle.Submit(request, Budget()); Is(call.Application);
                    if (call.Presentation != null) Battle.ReportPresentationCompleted(call.Presentation.Token);
                }
                Witness(this, "won-s17");
            }
            internal void Close() { if (closed) return; Architecture.Deinit(); closed = true; }
            public void Dispose() { Close(); }
        }
        internal sealed class MemorySave : ILocalSaveStorage
        {
            internal readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal string Fault; internal int SnapshotCreates; internal bool Exists; private bool leased;
            public SaveStorageProfile Profile { get; }
            internal MemorySave(string player) { Profile = new SaveStorageProfile(player, SavePurpose.PlayerSave, "isolated-memory:" + player, SaveFaultModel.EditorProcessCrash); }
            public IDisposable AcquireWriterLease(bool createDirectory)
            {
                if (leased) throw new IOException("injected lease contention");
                if (!Exists && !createDirectory) throw new DirectoryNotFoundException(); Exists = true; leased = true;
                if (!Files.ContainsKey("writer.lock")) Files.Add("writer.lock", Array.Empty<byte>()); return new Lease(() => leased = false);
            }
            private sealed class Lease : IDisposable { private Action close; internal Lease(Action close) { this.close = close; } public void Dispose() { close?.Invoke(); close = null; } }
            public IEnumerable<string> EnumerateNames() => Files.Keys.ToArray();
            public Stream OpenRead(string name) { if (!Files.TryGetValue(name, out var bytes)) throw new FileNotFoundException(name); return new MemoryStream(bytes, false); }
            public Stream CreateWork(string name)
            {
                if (Fault == "snapshot-before" && name.EndsWith(".snapshot.tmp")) { Fault = null; throw new IOException("before snapshot"); }
                if (Files.ContainsKey(name)) throw new IOException("existing work");
                if (name.EndsWith(".snapshot.tmp")) SnapshotCreates++;
                Files.Add(name, Array.Empty<byte>()); return new Work(this, name);
            }
            private sealed class Work : MemoryStream
            {
                internal readonly MemorySave Owner; internal readonly string Name;
                internal Work(MemorySave owner, string name) { Owner = owner; Name = name; }
                protected override void Dispose(bool disposing) { if (disposing) Owner.Files[Name] = ToArray(); base.Dispose(disposing); }
            }
            public void FlushFile(Stream stream) { var work = (Work)stream; Files[work.Name] = work.ToArray(); }
            public void PromoteNoReplace(string workName, string finalName)
            {
                if (Fault == "marker-before" && finalName.EndsWith(".commit")) { Fault = null; throw new IOException("before marker promotion"); }
                if (Fault == "snapshot-promoted" && finalName.EndsWith(".snapshot"))
                { Files.Add(finalName, Files[workName]); Files.Remove(workName); Fault = null; throw new IOException("snapshot persisted before marker"); }
                if (Files.ContainsKey(finalName)) throw new IOException("existing final"); Files.Add(finalName, Files[workName]); Files.Remove(workName);
                if (Fault == "marker-after" && finalName.EndsWith(".commit")) { Fault = null; throw new IOException("marker persisted, outcome unknown"); }
            }
            public void DeleteUncommitted(string name) { Files.Remove(name); }
            public void DeleteIndexedOld(string name) { Files.Remove(name); }
        }
        internal sealed class NoCurrentStorage : IContentPublicationStorage
        {
            private readonly IContentPublicationStorage inner;
            internal int CurrentReads;
            internal NoCurrentStorage(IContentPublicationStorage inner) { this.inner = inner; }
            public IDisposable AcquireWriter() => inner.AcquireWriter();
            public byte[] Read(string key, int maxBytes)
            {
                if (key == PublishedContentCatalog.ReleaseSetKey("player", Release))
                { CurrentReads++; throw new IOException("Recovery must never ask for current release"); }
                return inner.Read(key, maxBytes);
            }
            public void WriteImmutable(string key, byte[] bytes, int maxBytes) { throw new IOException("read-only recovery fixture"); }
        }
    }
}
