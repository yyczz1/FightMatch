using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using FightMatch.Application;
using FightMatch.Platform;
using NUnit.Framework;
using QFramework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BusinessSaveScenario;
using static FightMatch.Core.Tests.ApplicationRuntimeRig;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateApplicationRuntimeTests
    {
        [Test]
        public void A01_ExplicitInitializationPublishesOnlyAfterSixSliceCommitAndFreezesInputs()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                var initial = Is(r.Open(), "InitializationReady");
                Assert.IsNull(initial.View.PublishedSnapshot);
                Assert.IsTrue(initial.View.IsPublishedHeadVerified);
                Assert.AreEqual(0, r.Builds);
                var input = r.InitializeInput();
                var intent = r.Freeze(input);
                input.InitializeProfile.CharacterId = "mutated";
                var events = 0;
                var handle = r.Architecture.RegisterEvent<CandidateApplicationPublished>(e =>
                {
                    events++;
                    Assert.AreSame(r.Model.View, e.View);
                    Assert.AreSame(r.Head.Descriptor, e.Descriptor);
                    Assert.AreSame(e.View, r.Architecture.SendQuery(new CandidateApplicationViewQuery()).View);
                    Assert.AreEqual(6, r.ReadEnvelope(e.Descriptor).RequiredSliceContracts.Count);
                });
                try
                {
                    var result = Is(r.Submit(intent, r.InitialBuild), "Completed");
                    Assert.IsTrue(result.IsCommitted);
                    Assert.AreEqual(32, result.OriginalCommitId.Length);
                    Assert.AreEqual("W", result.OriginalLookup.Initialization.CharacterId);
                    Assert.AreEqual(1, events);
                    Assert.AreEqual(1, r.Builds);
                    Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(result.OriginalCommitId));
                    Assert.Throws<NotSupportedException>(() => ((IList<string>)result.View.ObservedCandidateCommitIds).Add("bad"));
                    Assert.Throws<NotSupportedException>(() => ((IList<byte>)r.Head.Descriptor.Sha256)[0] = 0);
                    Assert.Throws<NotSupportedException>(() => ((IList<CandidateApplicationRecord>)r.Head.Records).Clear());
                    var saved = result.View;
                    Is(r.Restore(), "Ready");
                    Assert.AreEqual(1, events);
                    Assert.AreEqual(saved.PublishedSnapshot.Header.CommitId, r.Head.Header.CommitId);
                    Assert.IsTrue(saved.IsPublishedHeadVerified);
                }
                finally { handle.UnRegister(); }
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void A01_ExistingMissingNeverInitializesAndLeaseOwnershipIsExplicit(bool existingDirectory)
        {
            var root = NewCase();
            if (existingDirectory)
            {
                var storage = new WindowsEditorSaveStorage(root, "player:015b", SavePurpose.CandidateValidation);
                using (LocalSaveTestFiles.Open(storage)) { }
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                Is(r.Open(SaveOpenMode.Existing), existingDirectory ? "InitializationRequired" : "NoSave");
                Assert.IsNull(r.Head);
                Assert.AreEqual(existingDirectory ? CandidateApplicationPhase.RecoveryBlocked : CandidateApplicationPhase.Unconfigured, r.Model.View.Phase);
                var second = LocalSaveStore.Open(r.Storage, "player:015b", SavePurpose.CandidateValidation,
                    SaveOpenMode.CreateNew, SaveFaultModel.EditorProcessCrash, B());
                if (existingDirectory) Bad(second, "Busy");
                else Ok(second, "Opened").Dispose();
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
            }
        }

        [TestCase("five")]
        [TestCase("bad-six")]
        public void A01_OldFiveAndCorruptBusinessCannotBecomeEmptyOrPartlyPublished(string kind)
        {
            var root = NewCase();
            SaveEnvelope envelope;
            using (var writer = new PendingRig("player:015b", root))
            {
                SaveCommitTicket ticket;
                if (kind == "five")
                {
                    var domain = New();
                    ticket = Ok(writer.Store.Prepare(null, new[] { "legacy" }, m =>
                        CandidateBusinessSaveCodec.Encode(Prepare(domain),
                            new CandidateBusinessSaveHeader(m.SaveGeneration, m.CommitId, m.ParentCommitId, m.CommitIndex), B().Codec), B()), "Prepared");
                }
                else ticket = writer.Application(out _, true);
                envelope = ticket.Envelope;
                Ok(writer.Store.Write(ticket, B()), "Committed");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                var result = r.Open(SaveOpenMode.Existing, Caps(envelope));
                if (kind == "five") Is(result, "MissingApplicationRecords");
                else Assert.AreNotEqual("Ready", result.Code);
                Assert.IsNull(r.Head);
                Assert.IsFalse(result.View.IsPublishedHeadVerified);
                Assert.AreEqual("Decode", result.Diagnostic.Stage);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Bad(LocalSaveStore.Open(r.Storage, "player:015b", SavePurpose.CandidateValidation,
                    SaveOpenMode.Existing, SaveFaultModel.EditorProcessCrash, B()), "Busy");
            }
        }

        [TestCase("slices")]
        [TestCase("bindings")]
        [TestCase("features")]
        public void A01_CurrentMissingCapabilityNeverPublishesAndAlreadyOpenCannotReplaceCapabilities(string field)
        {
            string root;
            using (var writer = new ApplicationRuntimeRig())
            {
                root = writer.Root;
                Is(writer.Open(), "InitializationReady");
                Is(writer.Initialize(), "Completed");
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                var result = r.Open(SaveOpenMode.Existing, Without(Capabilities(), field));
                Is(result, field == "bindings" ? "UnsupportedBinding" : "UnsupportedCapability");
                Assert.IsNull(r.Head);
                Assert.IsFalse(r.Model.View.IsPublishedHeadVerified);
                StringAssert.Contains("Recovery[c-", result.Diagnostic.FieldPath);
                Is(r.Open(), "AlreadyOpen");
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
            }
        }

        [Test]
        public void A01_BackupOnlyCapabilityIsCheckedBeforePublishingValidCurrentSixSlices()
        {
            string root;
            using (var writer = new ApplicationRuntimeRig())
            {
                root = writer.Root;
                Is(writer.Open(), "InitializationReady");
                Is(writer.Initialize(), "Completed");
                var old = writer.ReadEnvelope(writer.Head.Descriptor);
                Is(writer.Enter(), "Completed");
                var current = writer.ReadEnvelope(writer.Head.Descriptor);
                var slices = Slices(old);
                var needs = slices[0].Requirements;
                slices[0].Requirements = new SaveRequirements(needs.Bindings, needs.RuleVersions, needs.NumericContractVersions,
                    needs.RandomContractVersions, needs.FeatureIds.Concat(new[] { "backup-only:unavailable" }).ToArray());
                var backup = Repack(old, slices);
                var changed = ReplacePair(writer.Storage.Profile.DirectoryPath, backup);
                var index = current.CommitIndex.ToArray();
                var row = index[0];
                index[0] = new SaveCommitIndexEntry(row.Generation, row.CommitId, row.ParentCommitId,
                    changed.TotalLength, changed.Sha256, row.OperationIds);
                var rebuilt = Repack(current, Slices(current), index);
                Assert.IsTrue(CandidateApplicationSaveCodec.Decode(rebuilt, B().Codec).IsAccepted, "current business remains valid");
                ReplacePair(writer.Storage.Profile.DirectoryPath, rebuilt);
            }
            using (var r = new ApplicationRuntimeRig(root))
            {
                var result = Is(r.Open(SaveOpenMode.Existing), "UnsupportedCapability");
                StringAssert.Contains("FeatureIds", result.Diagnostic.FieldPath);
                Assert.IsNull(r.Head);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
            }
        }

        [Test]
        public void A02_OriginalResultPrecedesExpectedHeadNullBuilderAndSupersededRelationUsesPublishedView()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var init = Is(r.Initialize(), "Completed");
                Is(r.Enter(), "Completed");
                var attack = Is(r.Attack("attack", 0), "Completed");
                Is(r.Rollback("rollback", "attack"), "Completed");
                var head = r.Head;
                var calls = r.Storage.Base.Calls;
                var builds = r.Builds;
                var original = Is(r.Submit(r.Intents["init"], null), "Completed");
                Assert.AreEqual(init.OriginalCommitId, original.OriginalCommitId);
                var superseded = Is(r.Submit(r.Intents["attack"], null), "Completed");
                Assert.AreEqual(attack.OriginalCommitId, superseded.OriginalCommitId);
                Assert.AreEqual(CandidateApplicationRelation.Superseded, superseded.OriginalLookup.Relation);
                Assert.AreEqual(head.Header.CommitId, superseded.LookupViewCommitId);
                Assert.AreSame(head, r.Head);
                Assert.AreEqual(calls, r.Storage.Base.Calls);
                Assert.AreEqual(builds, r.Builds);
                var conflict = r.InitializeInput();
                conflict.InitializeProfile.InitialExperience = 1;
                Is(r.Submit(r.Freeze(conflict), null), "OperationConflict");
                File.WriteAllBytes(r.Path("foreign"), new byte[] { 1 });
                Is(r.Restore(), "RecoveryBlocked");
                var old = Is(r.Query(r.Intents["attack"]), "Completed");
                Assert.AreEqual(head.Header.CommitId, old.LookupViewCommitId);
                Assert.IsFalse(old.View.IsPublishedHeadVerified);
                Is(r.Query(r.Freeze(r.InitializeInput("unseen"))), "ResolutionRequired");
            }
        }

        [TestCase("reject")]
        [TestCase("throw")]
        [TestCase("null")]
        [TestCase("missing")]
        [TestCase("propose")]
        [TestCase("limit")]
        public void A03_RefusalsLeaveNoTicketOrWriteAndRetainDiagnostic(string kind)
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                CandidateApplicationBuilder builder = (basis, value, budget) =>
                {
                    r.Builds++;
                    if (kind == "reject") return CandidateApplicationBuildResult.Rejected("StaleContext", "Domain.Revision", "custom-limit", 9, 8);
                    if (kind == "throw") throw new IOException("builder failed");
                    if (kind == "null") return null;
                    if (kind == "limit")
                    {
                        var limited = new ExactMathBudget(maxPrimitiveSteps: 0);
                        ExactRational.Create(1, 1, limited);
                    }
                    return CandidateApplicationBuildResult.Success(Prepare(r.Domain),
                        new CandidateApplicationResultInput { HistoryAnchorId = "invalid-for-init" });
                };
                var result = r.Submit(intent, kind == "missing" ? null : builder);
                if (kind == "reject")
                {
                    Is(result, "BuilderRejected");
                    Assert.AreEqual("StaleContext", result.Diagnostic.Code);
                    Assert.AreEqual("Domain.Revision", result.Diagnostic.FieldPath);
                    Assert.AreEqual("custom-limit", result.Diagnostic.LimitReason);
                    Assert.AreEqual(9, result.Diagnostic.RequiredAtLeast);
                    Assert.AreEqual(8, result.Diagnostic.Allowed);
                }
                else if (kind == "throw" || kind == "null") Is(result, "BuilderFailed");
                else if (kind == "missing") Is(result, "BuilderUnavailable");
                else if (kind == "limit")
                {
                    Is(result, "Limit");
                    Assert.AreEqual("PrimitiveSteps", result.Diagnostic.LimitReason);
                }
                else Assert.AreNotEqual("Completed", result.Code);
                Assert.IsNull(result.View.PendingOperationId);
                Assert.IsNull(result.View.PendingCommitId);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Assert.AreEqual(CandidateApplicationPhase.InitializationReady, result.View.Phase);
                Is(r.Submit(intent, r.InitialBuild), "Completed");
            }
        }

        [Test]
        public void A03_PrepareFailureRetainsOneCandidateAndCopiedMutableResultForRetry()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                var resultInput = new CandidateApplicationResultInput();
                var result = r.Submit(intent, (basis, value, budget) =>
                {
                    r.Builds++;
                    var built = CandidateApplicationBuildResult.Success(Prepare(r.Domain), resultInput);
                    resultInput.HistoryAnchorId = "mutated-after-success";
                    r.Storage.Base.FailEnumerationAlways = true;
                    return built;
                });
                Is(result, "StorageFailure");
                Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, result.View.Phase);
                Assert.IsNull(result.View.PendingCommitId);
                Assert.AreEqual("init", result.View.PendingOperationId);
                r.Storage.Base.FailEnumerationAlways = false;
                var completed = Is(r.Retry(intent), "Completed");
                Assert.AreEqual(1, r.Builds);
                Assert.IsNull(completed.OriginalLookup.Record.Result.HistoryAnchorId);
                Assert.AreEqual(1, r.Head.Records.Count);
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(completed.OriginalCommitId));
            }
        }

        [Test]
        public void A03_EncodeLimitRetainsUnpreparedCandidateAndBudgetInformation()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                var bodyLength = Encode(Prepare(r.Domain)).BodyLength;
                var small = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(), maxEnvelopeBytes: bodyLength));
                var failed = Is(r.Submit(intent, r.InitialBuild, small), "Limit");
                Assert.AreEqual(CandidateApplicationPhase.PendingPreparation, failed.View.Phase);
                Assert.IsNull(failed.View.PendingCommitId);
                Assert.AreEqual("EnvelopeBytes", failed.Diagnostic.LimitReason);
                Assert.Greater(failed.Diagnostic.RequiredAtLeast, failed.Diagnostic.Allowed);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Is(r.Retry(intent), "Completed");
                Assert.AreEqual(1, r.Builds);
            }
        }

        [Test]
        public void A03_NewCandidateMissingCapabilityHasTicketButZeroWrite()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(caps: Without(Capabilities(), "slices")), "InitializationReady");
                var result = Is(r.Initialize(), "UnsupportedCapability");
                Assert.AreEqual(CandidateApplicationPhase.RecoveryBlocked, result.View.Phase);
                Assert.IsNotNull(result.View.PendingCommitId);
                StringAssert.Contains("pending-ticket", result.Diagnostic.FieldPath);
                Assert.AreEqual(0, r.Storage.Base.WriteCalls);
                Is(r.End(r.Intents["init"]), "Ended");
                Assert.AreEqual(CandidateApplicationPhase.InitializationReady, r.Model.View.Phase);
            }
        }

        [TestCase("Snapshot.Flush.after", "SaveFailed", false)]
        [TestCase("Snapshot.Flush.after", "SaveFailed", true)]
        [TestCase("Marker.Promote.after", "CommitUnknown", false)]
        public void A04_RealFailureKeepsOriginalIdentityAndBlocksAllNewConstruction(string fault, string code, bool end)
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                r.Storage.Base.Arm(fault, null);
                var failed = Is(r.Submit(intent, r.InitialBuild), code);
                Assert.IsTrue(r.Storage.Base.FaultUsed);
                Assert.AreEqual(code, failed.View.Phase.ToString());
                var commit = failed.View.PendingCommitId;
                Assert.IsNull(failed.OriginalLookup);
                var before = r.Disk();
                var calls = r.Storage.Base.Calls;
                Is(r.Submit(intent, null), code);
                Is(r.Restore(), code);
                var other = r.Freeze(r.InitializeInput("different"));
                Is(r.Submit(other, r.InitialBuild), "Busy");
                Is(r.Query(other), "ResolutionRequired");
                var conflictInput = r.InitializeInput();
                conflictInput.InitializeProfile.InitialExperience = 1;
                Is(r.Submit(r.Freeze(conflictInput), null), "OperationConflict");
                Assert.AreEqual(calls, r.Storage.Base.Calls);
                Assert.AreEqual(1, r.Builds);
                r.SameDisk(before);
                var resolved = r.Resolve(intent);
                if (code == "CommitUnknown")
                {
                    Is(resolved, "Completed");
                    Assert.AreEqual(commit, resolved.OriginalCommitId);
                }
                else
                {
                    Is(resolved, "ConfirmedNotCommitted");
                    Assert.AreEqual(commit, resolved.View.PendingCommitId);
                    Assert.AreNotEqual("NoEffectFinal", resolved.Code);
                    if (end)
                    {
                        Is(r.End(intent), "Ended");
                        Assert.IsNull(r.Model.View.PendingOperationId);
                        Assert.IsNull(r.Head);
                        Assert.AreEqual(CandidateApplicationPhase.InitializationReady, r.Model.View.Phase);
                        return;
                    }
                    var saved = Is(r.Retry(intent), "Completed");
                    Assert.AreEqual(commit, saved.OriginalCommitId);
                    CollectionAssert.AreEqual(before["w-" + commit + ".snapshot.tmp"], File.ReadAllBytes(r.Path(SnapshotName(commit))));
                }
                Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(commit));
                Assert.AreEqual(1, r.Head.Records.Count);
                Assert.AreEqual(1, r.Builds);
                Is(r.End(intent), "Completed");
            }
        }

        [TestCase("observe")]
        [TestCase("load")]
        [TestCase("decode")]
        [TestCase("capability")]
        [TestCase("budget")]
        public void A05_ConfirmedCommitSurvivesAllPostCommitRestoreFailures(string fault)
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var budget = B();
                var events = 0;
                var handle = r.Architecture.RegisterEvent<CandidateApplicationPublished>(_ => events++);
                var originals = new Dictionary<string, byte[]>();
                string extra = null;
                SaveCommitTicket foreign = null;
                if (fault == "capability")
                    using (var raw = new PendingRig("player:015b")) foreign = raw.Prepare(null, "unavailable");
                var closedSnapshotsAtLoad = 0;
                var faultFired = false;
                r.Storage.AfterClose = name =>
                {
                    if (fault != "budget" || r.Storage.PostMarkerEnumerations != 4 || !name.EndsWith(".snapshot", StringComparison.Ordinal)) return;
                    if (++closedSnapshotsAtLoad != 2) return;
                    faultFired = true;
                    // Deliberately consume the SAME caller budget at the completed Load boundary.
                    while (budget.Codec.Math.PrimitiveStepsUsed < budget.Codec.Math.MaxPrimitiveSteps)
                        budget.Codec.Math.Compare(BigInteger.Zero, BigInteger.Zero);
                };
                r.Storage.AfterMarkerEnumeration = n =>
                {
                    if ((fault == "observe" && n == 2) || (fault == "load" && n == 4))
                    {
                        faultFired = true;
                        throw new IOException("post-marker " + fault);
                    }
                    if (n != 2) return;
                    if (fault == "decode")
                    {
                        faultFired = true;
                        var id = r.Storage.PublishedCommit;
                        var path = r.Path(SnapshotName(id));
                        var bytes = File.ReadAllBytes(path);
                        SaveRecoverySummary summary;
                        using (var stream = new MemoryStream(bytes)) summary = Accept(SaveEnvelopeCodec.ReadUncommittedRequirements(stream, B().Codec));
                        var envelope = r.ReadEnvelope(summary.Descriptor);
                        originals[SnapshotName(id)] = bytes;
                        originals[MarkerName(id)] = File.ReadAllBytes(r.Path(MarkerName(id)));
                        var slices = Slices(envelope);
                        slices[0].Bytes = new byte[] { 1 };
                        ReplacePair(r.Storage.Profile.DirectoryPath, Repack(envelope, slices));
                    }
                    if (fault == "capability")
                    {
                        faultFired = true;
                        extra = r.Path(PendingRig.Work(foreign));
                        File.WriteAllBytes(extra, PendingRig.Bytes(foreign.Envelope));
                    }
                };
                try
                {
                    var intent = r.Freeze(r.InitializeInput());
                    var failed = Is(r.Submit(intent, r.InitialBuild, budget), "CommittedRestoreRequired");
                    Assert.IsTrue(faultFired, "the requested real storage boundary was reached");
                    Assert.IsTrue(failed.IsCommitted);
                    var commit = failed.OriginalCommitId;
                    Assert.IsNotNull(commit);
                    Assert.IsNull(failed.OriginalLookup);
                    Assert.IsNull(r.Head);
                    Assert.AreEqual(0, events);
                    Assert.AreEqual(CandidateApplicationPhase.RestoreRequired, failed.View.Phase);
                    if (fault == "decode" || fault == "budget") Assert.AreEqual("Decode", failed.Diagnostic.Stage);
                    if (fault == "budget")
                    {
                        Assert.AreEqual("Limit", failed.Diagnostic.Code);
                        Assert.AreEqual("PrimitiveSteps", failed.Diagnostic.LimitReason);
                        Assert.Greater(failed.Diagnostic.RequiredAtLeast, failed.Diagnostic.Allowed);
                    }
                    if (fault == "capability") Assert.AreEqual("UnsupportedCapability", failed.Diagnostic.Code);
                    Is(r.Submit(r.Freeze(r.InitializeInput("new")), r.InitialBuild), "Busy");
                    var writes = r.Storage.Base.WriteCalls;
                    r.Storage.AfterMarkerEnumeration = null;
                    r.Storage.AfterClose = null;
                    foreach (var pair in originals) File.WriteAllBytes(r.Path(pair.Key), pair.Value);
                    if (extra != null) File.Delete(extra);
                    var restored = Is(r.Resolve(intent), "Completed");
                    Assert.AreEqual(commit, restored.OriginalCommitId);
                    Assert.AreEqual(1, events);
                    Assert.AreEqual(writes, r.Storage.Base.WriteCalls);
                    Assert.AreEqual(1, r.Storage.Base.RealMarkerPromotionsFor(commit));
                    Assert.AreEqual(1, r.Builds);
                }
                finally { handle.UnRegister(); }
            }
        }

        [Test]
        public void A05_ResolveLoadsLatestHeadAndLooksUpOriginalInsteadOfInstallingOlderCandidate()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                Is(r.Initialize(), "Completed");
                var oldPublished = r.Head;
                r.Storage.AfterMarkerEnumeration = n =>
                {
                    if (r.Storage.PublishedCommit != oldPublished.Header.CommitId && n == 2)
                        throw new IOException("after enter committed");
                };
                var failed = Is(r.Enter(), "CommittedRestoreRequired");
                var original = r.Intents["enter"];
                Assert.AreSame(oldPublished, r.Head);
                r.Storage.AfterMarkerEnumeration = null;
                // Build a later real M12 commit in an independent test directory, then
                // inject its pair as external disk advancement while this runtime resolves.
                var other = new WindowsEditorSaveStorage(NewCase(), "player:015b", SavePurpose.CandidateValidation);
                SaveCommittedReference latest;
                using (var writer = LocalSaveTestFiles.Open(other))
                {
                    foreach (var pair in r.Disk()) File.WriteAllBytes(System.IO.Path.Combine(other.Profile.DirectoryPath, pair.Key), pair.Value);
                    var envelope = Ok(writer.Load(B()), "Loaded");
                    var basis = Accept(CandidateApplicationSaveCodec.Decode(envelope, B().Codec));
                    var domain = New();
                    domain.Use(basis.Business);
                    var state = domain.History.CurrentRun.CurrentSnapshot;
                    var input = new CandidateApplicationIntentInput { PlayerId = "player:015b", OperationId = "external-attack",
                        Kind = CandidateApplicationKind.Attack, Context = domain.Context, ExpectedCommitId = basis.Header.CommitId,
                        Attack = new CandidateApplicationAttackInput { AttemptId = domain.EntryInput.AttemptId,
                            ExpectedSceneRevision = state.SceneRevision, Actor = state.Members[0].CombatantKey,
                            Pair = state.Enemies[0].PairKey, Route = domain.Routes[0].ToList(),
                            ExpectedPreferenceRevision = domain.Inventory.PreferenceRevision, ItemUseEnabled = true } };
                    var intent = Accept(CandidateApplicationProtocol.PrepareIntent(input, B().Codec));
                    domain.Attack(input.OperationId, 0);
                    var candidate = Accept(CandidateApplicationProtocol.Propose(basis, Prepare(domain), intent,
                        new CandidateApplicationResultInput { HistoryAnchorId = "anchor:" + input.OperationId }, null, B().Codec));
                    var ticket = Ok(writer.Prepare(basis.Descriptor, new[] { input.OperationId }, m =>
                        CandidateApplicationSaveCodec.Encode(candidate, new CandidateBusinessSaveHeader(m.SaveGeneration,
                            m.CommitId, m.ParentCommitId, m.CommitIndex), B().Codec), B()), "Prepared");
                    latest = Ok(writer.Write(ticket, B()), "Committed");
                    foreach (var name in new[] { SnapshotName(latest.Descriptor.CommitId), MarkerName(latest.Descriptor.CommitId) })
                        File.WriteAllBytes(r.Path(name), File.ReadAllBytes(System.IO.Path.Combine(other.Profile.DirectoryPath, name)));
                }
                var writes = r.Storage.Base.WriteCalls;
                var result = Is(r.Resolve(original), "Completed");
                Assert.AreEqual(failed.OriginalCommitId, result.OriginalCommitId);
                Assert.AreEqual(latest.Descriptor.CommitId, r.Head.Header.CommitId);
                Assert.AreEqual(latest.Descriptor.CommitId, result.LookupViewCommitId);
                Assert.AreEqual(3, r.Head.Records.Count);
                Assert.AreEqual(writes, r.Storage.Base.WriteCalls);
                Assert.AreEqual(2, r.Builds);
            }
        }

        [Test]
        public void A05_NotificationFailureCannotUndoCommitAndEventReadsAreCompleteWhileWritesAreBusy()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                var laterListeners = 0;
                var first = r.Architecture.RegisterEvent<CandidateApplicationPublished>(e =>
                {
                    var calls = r.Storage.Base.Calls;
                    Assert.AreSame(e.View, r.System.QueryView().View);
                    Is(r.Query(intent), "Completed");
                    Is(r.Submit(intent, null), "Busy");
                    Is(r.Restore(), "Busy");
                    Assert.Throws<InvalidOperationException>(() => r.Architecture.Deinit());
                    Assert.AreEqual(calls, r.Storage.Base.Calls);
                    throw new IOException("listener stopped delivery");
                });
                var second = r.Architecture.RegisterEvent<CandidateApplicationPublished>(_ => laterListeners++);
                try
                {
                    var result = Is(r.Submit(intent, r.InitialBuild), "Completed");
                    Assert.IsTrue(result.IsCommitted);
                    Assert.IsTrue(result.View.IsPublishedHeadVerified);
                    Assert.AreEqual(typeof(IOException).FullName, result.NotificationFailure.ExceptionType);
                    Assert.AreEqual("listener stopped delivery", result.NotificationFailure.ExceptionMessage);
                    Assert.AreEqual(0, laterListeners);
                    var writes = r.Storage.Base.WriteCalls;
                    Assert.IsNull(Is(r.Restore(), "Ready").NotificationFailure);
                    Is(r.Submit(intent, null), "Completed");
                    Assert.AreEqual(writes, r.Storage.Base.WriteCalls);
                }
                finally { first.UnRegister(); second.UnRegister(); }
            }
        }

        [Test]
        public void A08_AllEntrypointsRejectWrongThreadBeforeIoAndDeinitBeforeFrameworkCleanup()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                var intent = r.Freeze(r.InitializeInput());
                var calls = r.Storage.Base.Calls;
                var results = new List<CandidateApplicationCallResult>();
                Exception deinit = null;
                var thread = new Thread(() =>
                {
                    results.Add(r.Open());
                    results.Add(r.Restore());
                    results.Add(r.Submit(intent, r.InitialBuild));
                    results.Add(r.Resolve(intent));
                    results.Add(r.Retry(intent));
                    results.Add(r.End(intent));
                    results.Add(r.Resume(Guid.NewGuid().ToString("N")));
                    results.Add(r.EndObserved(Guid.NewGuid().ToString("N")));
                    results.Add(r.Query(intent));
                    results.Add(r.Architecture.SendQuery(new CandidateApplicationViewQuery()));
                    try { r.Architecture.Deinit(); } catch (Exception error) { deinit = error; }
                });
                thread.Start();
                Assert.IsTrue(thread.Join(30000));
                Assert.AreEqual(10, results.Count);
                foreach (var result in results) Is(result, "WrongThread");
                Assert.IsInstanceOf<InvalidOperationException>(deinit);
                Assert.AreSame(r.System, r.Architecture.GetSystem<CandidateApplicationSystem>());
                Assert.AreEqual(calls, r.Storage.Base.Calls);
                Assert.AreEqual(0, r.Builds);
                Is(r.Submit(intent, r.InitialBuild), "Completed");
            }
        }

        [Test]
        public void A08_DeinitLeavesDiskCandidateAndNewInterfaceHasNoOldStateOrGlobalEvents()
        {
            string root;
            CandidateApplicationSystem oldSystem;
            CandidateApplicationModel oldModel;
            PreparedCandidateApplicationIntent intent;
            Dictionary<string, byte[]> before;
            IArchitecture oldArchitecture;
            var retainedInstanceEvents = 0;
            using (var r = new ApplicationRuntimeRig())
            {
                root = r.Root;
                oldArchitecture = r.Architecture;
                var eventHandle = oldArchitecture.RegisterEvent<string>(_ => retainedInstanceEvents++);
                Is(r.Open(), "InitializationReady");
                r.Storage.Base.Arm("Snapshot.Flush.after", null);
                Is(r.Initialize(), "SaveFailed");
                intent = r.Intents["init"];
                oldSystem = r.System;
                oldModel = r.Model;
                before = r.Disk();
                r.Close();
                r.SameDisk(before);
                Assert.AreEqual(0, r.Storage.Deletes);
                Assert.AreEqual(1, r.Storage.LeaseCloses);
                Assert.AreEqual(CandidateApplicationPhase.Disposed, oldModel.View.Phase);
                Is(oldSystem.Submit(intent, null, B()), "Disposed");
                oldArchitecture.SendEvent("framework retains instance subscribers");
                eventHandle.UnRegister();
            }
            using (var fresh = new ApplicationRuntimeRig(root))
            {
                Assert.AreNotSame(oldArchitecture, fresh.Architecture);
                Assert.AreNotSame(oldModel, fresh.Model);
                Assert.IsNull(fresh.Head);
                fresh.Architecture.SendEvent("new instance");
                Assert.AreEqual(1, retainedInstanceEvents);
                Is(fresh.Open(SaveOpenMode.Existing), "Pending");
                fresh.SameDisk(before);
                Is(fresh.Resume(fresh.Model.View.ObservedCandidateCommitIds.Single()), "Completed");
                Assert.AreEqual(0, fresh.Builds);
                Is(oldSystem.Retry(intent, B()), "Disposed");
            }
        }

        [Test]
        public void A08_StaleArchitectureDeinitCannotDetachCurrentInstance()
        {
            string root;
            IArchitecture stale;
            PreparedCandidateApplicationIntent original;
            using (var a = new ApplicationRuntimeRig())
            {
                root = a.Root;
                stale = a.Architecture;
                Is(a.Open(), "InitializationReady");
                Is(a.Initialize(), "Completed");
                original = a.Intents["init"];
                a.Close();
            }
            using (var b = new ApplicationRuntimeRig(root))
            {
                Is(b.Open(SaveOpenMode.Existing), "Ready");
                var architecture = b.Architecture;
                var model = b.Model;
                var system = b.System;
                var view = model.View;
                var calls = b.Storage.Base.Calls;
                Assert.AreNotSame(stale, architecture);
                Assert.Throws<InvalidOperationException>(() => stale.Deinit());
                Assert.AreSame(architecture, FightMatchDemoArchitecture.Interface);
                Assert.AreSame(model, architecture.GetModel<CandidateApplicationModel>());
                Assert.AreSame(system, architecture.GetSystem<CandidateApplicationSystem>());
                Assert.AreSame(view, model.View);
                Assert.AreSame(view, architecture.SendQuery(new CandidateApplicationViewQuery()).View);
                Assert.AreEqual(calls, b.Storage.Base.Calls);
                Assert.AreEqual(0, b.Storage.LeaseCloses);
                Assert.Throws<IOException>(() =>
                {
                    using (b.Storage.AcquireWriterLease(false)) { }
                });
                Is(b.Query(original), "Completed");
                Is(b.Enter(), "Completed");
                b.Close();
                Assert.AreEqual(1, b.Storage.LeaseCloses);
            }
            using (var reopened = new ApplicationRuntimeRig(root))
            {
                Is(reopened.Open(SaveOpenMode.Existing), "Ready");
                Is(reopened.Query(original), "Completed");
            }
        }

        [Test]
        public void A08_LeaseCloseExceptionPropagatesAndOldObjectsStayDisposed()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Is(r.Open(), "InitializationReady");
                r.Storage.FailLeaseClose = true;
                var error = Assert.Throws<IOException>(() => r.Architecture.Deinit());
                Assert.AreEqual("application lease close", error.Message);
                Assert.AreEqual(CandidateApplicationPhase.Disposed, r.Model.View.Phase);
                Assert.AreEqual("CloseFailed", r.Model.View.Diagnostic.Code);
                Assert.AreEqual(error.Message, r.Model.View.Diagnostic.ExceptionMessage);
                Is(r.System.Restore(B()), "Disposed");
                r.Storage.FailLeaseClose = false;
                r.Close();
                Assert.AreEqual(1, r.Storage.LeaseCloses);
                using (LocalSaveTestFiles.Open(r.Storage, SaveOpenMode.Existing)) { }
            }
        }

        [Test]
        public void A08_RequiredNullsEnumsAndProfilesKeepOriginalContractAndReleaseAdmission()
        {
            using (var r = new ApplicationRuntimeRig())
            {
                Assert.Throws<ArgumentNullException>(() => r.System.Open(null, "p", SaveOpenMode.CreateNew, Capabilities(), B()));
                Assert.Throws<ArgumentNullException>(() => r.System.Restore(null));
                Assert.Throws<ArgumentNullException>(() => r.System.Submit(null, null, B()));
                Assert.Throws<ArgumentNullException>(() => r.System.QueryOperation(null, B()));
                Assert.Throws<ArgumentNullException>(() => r.System.ResumeObserved(null, B()));
                Assert.Throws<ArgumentNullException>(() => r.System.EndObserved(null, B()));
                Is(r.Open((SaveOpenMode)999), "InvalidValue");
                Is(r.System.Open(r.Storage, "wrong-player", SaveOpenMode.CreateNew, Capabilities(), B()), "InconsistentBinding");
                Assert.AreEqual(CandidateApplicationPhase.Unconfigured, r.Model.View.Phase);
                Assert.AreEqual(0, r.Storage.Base.Calls);
                Is(r.Open(), "InitializationReady");
                Is(r.Query(r.Freeze(r.InitializeInput())), "NotFound");
                Is(r.Initialize(), "Completed");
            }
        }
    }
}
