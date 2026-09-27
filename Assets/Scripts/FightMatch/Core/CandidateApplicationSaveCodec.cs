using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidateApplicationSaveCodec
    {
        private static readonly string[] SliceIds = { "fm.m02.application", "fm.m03.character", "fm.m04.inventory", "fm.m05.progression", "fm.m06.battle", "fm.m07.rewards" };

        public static SaveCodecResult<SaveEnvelope> Encode(CandidateApplicationCandidate candidate, CandidateBusinessSaveHeader header, SaveCodecBudget budget)
        { return EncodeCore(candidate, header, null, budget); }
        public static SaveCodecResult<SaveEnvelope> EncodePublished(CandidateApplicationCandidate candidate, CandidateBusinessSaveHeader header,
            PublishedSaveContext resolved, SaveCodecBudget budget)
        { if (resolved == null) throw new ArgumentNullException(nameof(resolved)); return EncodeCore(candidate, header, resolved, budget); }
        private static SaveCodecResult<SaveEnvelope> EncodeCore(CandidateApplicationCandidate candidate, CandidateBusinessSaveHeader header,
            PublishedSaveContext published, SaveCodecBudget budget)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<SaveEnvelope>.Run(() =>
            {
                var records = Bind(candidate, header, budget);
                var five = Take(published == null ? CandidateBusinessSaveCodec.Encode(candidate.Business, header, budget)
                    : CandidateBusinessSaveCodec.EncodePublished(candidate.Business, header, published, budget), "Business");
                var resolved = new CandidateApplicationReferences(candidate.Business, budget).Validate(records);
                CandidateApplicationProtocol.CheckContinuation(candidate.Business, records, candidate.Continuation);
                var roster = (int)candidate.Business.Format >= 3;
                var permanent = candidate.Business.Format == CandidateBusinessFormat.PublishedPermanentV4;
                var requirements = Requirements(records, resolved, budget, published != null, roster, permanent);
                var measure = new BusinessFields(Stream.Null, false, budget) { Resolved = published, StrictUnicode = published != null, SchemaVersion = permanent ? 4U : roster ? 3U : 0U };
                Body(measure, candidate.Business.PlayerId, records, candidate.Continuation, out _);
                // Body offsets, lengths and hashes are fixed-width metadata. An empty M02 measures
                // the exact six-slice metadata before allocating the additional application body.
                var slices = Slices(five, 0);
                slices.Insert(0, new SaveSliceInput { Contract = Contract(0, published != null, roster, permanent), Bytes = new byte[0], Requirements = requirements });
                var measured = Envelope(five, slices, budget);
                var length = Take(SaveEnvelopeCodec.Write(Stream.Null, measured, budget), "Envelope.Measure").TotalLength;
                SaveCodecFailure.Limit(length + measure.Used, budget.MaxEnvelopeBytes, "Envelope.TotalLength", "EnvelopeBytes");
                var body = new byte[(int)measure.Used];
                using (var stream = new MemoryStream(body, true))
                    Body(new BusinessFields(stream, false, budget) { Resolved = published, StrictUnicode = published != null, SchemaVersion = permanent ? 4U : roster ? 3U : 0U }, candidate.Business.PlayerId, records, candidate.Continuation, out _);
                slices[0].Bytes = body;
                return Envelope(five, slices, budget);
            });
        }

        public static SaveCodecResult<CandidateApplicationSnapshot> Decode(SaveEnvelope envelope, SaveCodecBudget budget)
        { return DecodeCore(envelope, null, budget); }
        public static SaveCodecResult<CandidateApplicationSnapshot> DecodePublished(SaveEnvelope envelope, PublishedSaveContext resolved, SaveCodecBudget budget)
        { if (resolved == null) throw new ArgumentNullException(nameof(resolved)); return DecodeCore(envelope, resolved, budget); }
        private static SaveCodecResult<CandidateApplicationSnapshot> DecodeCore(SaveEnvelope envelope, PublishedSaveContext published, SaveCodecBudget budget)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateApplicationSnapshot>.Run(() =>
            {
                Need(envelope.Purpose == (published == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave), "Envelope.Purpose", "UnsupportedBinding");
                if (envelope.RequiredSliceContracts.Count == 5 && envelope.SliceDirectory.Count == 5 && envelope.Bodies.Length == 5)
                {
                    Take(published == null ? CandidateBusinessSaveCodec.Decode(envelope, budget)
                        : CandidateBusinessSaveCodec.DecodePublished(envelope, published, budget), "Business");
                    throw new SaveCodecFailure("MissingApplicationRecords", "M02");
                }
                Need(envelope.RequiredSliceContracts.Count == 6 && envelope.SliceDirectory.Count == 6 && envelope.Bodies.Length == 6,
                    "Envelope.Slices", "UnsupportedSchema");
                var roster = published != null && (envelope.RequiredSliceContracts[0].SchemaVersion == 3 || envelope.RequiredSliceContracts[0].SchemaVersion == 4);
                var permanent = published != null && envelope.RequiredSliceContracts[0].SchemaVersion == 4;
                for (var i = 0; i < 6; i++)
                {
                    CheckContract(envelope.RequiredSliceContracts[i], i, published != null, roster, permanent);
                    CheckContract(envelope.SliceDirectory[i].Contract, i, published != null, roster, permanent);
                    SaveCodecFailure.Limit((ulong)envelope.Bodies[i].Length, budget.MaxEnvelopeBytes, "Slices", "EnvelopeBytes");
                }
                var descriptor = Take(SaveEnvelopeCodec.Write(Stream.Null, envelope, budget), "Envelope");
                List<WireRecord> wire;
                CandidateApplicationContinuation continuation;
                using (var stream = new MemoryStream(envelope.Bodies[0], false))
                {
                    var fields = new BusinessFields(stream, true, budget, (ulong)stream.Length) { Resolved = published, StrictUnicode = published != null, SchemaVersion = permanent ? 4U : roster ? 3U : 0U };
                    wire = Body(fields, envelope.PlayerId, null, null, out continuation);
                    fields.End("M02");
                }
                var projection = Envelope(envelope, Slices(envelope, 1), budget);
                var business = Take(published == null ? CandidateBusinessSaveCodec.Decode(projection, budget)
                    : CandidateBusinessSaveCodec.DecodePublished(projection, published, budget), "Business");
                var records = new List<CandidateApplicationRecord>(wire.Count);
                foreach (var value in wire) records.Add(value.Resolve(business, budget));
                var header = new CandidateBusinessSaveHeader(envelope.SaveGeneration, envelope.CommitId, envelope.ParentCommitId, envelope.CommitIndex);
                CheckIndex(records, header, budget);
                var resolved = new CandidateApplicationReferences(business, budget).Validate(records);
                if (records.Count == 1) CandidateApplicationProtocol.Initial(business, records[0].Intent);
                CandidateApplicationProtocol.CheckContinuation(business, records, continuation);
                Need(BusinessRequirements.Same(Requirements(records, resolved, budget, published != null, roster, permanent), envelope.SliceDirectory[0].Requirements),
                    "M02.Requirements", "InconsistentBinding");
                using (var canonical = new MemoryStream())
                {
                    Body(new BusinessFields(canonical, false, budget) { Resolved = published, StrictUnicode = published != null, SchemaVersion = permanent ? 4U : roster ? 3U : 0U }, business.PlayerId, records, continuation, out _);
                    Need(CandidateApplicationIntentCodec.SameBytes(canonical.ToArray(), envelope.Bodies[0]), "M02.CanonicalLayout", "Malformed");
                }
                return new CandidateApplicationSnapshot(business, records, continuation, header, descriptor);
            });
        }

        private static List<CandidateApplicationRecord> Bind(CandidateApplicationCandidate candidate, CandidateBusinessSaveHeader header, SaveCodecBudget budget)
        {
            SaveEnvelopeCodec.CheckList(header.CommitIndex, budget, "CommitIndex");
            SaveEnvelopeCodec.CheckList(candidate.Records, budget, "Records");
            var basis = candidate.Basis;
            var expectedGeneration = basis == null ? BigInteger.One : budget.Math.Add(basis.Header.SaveGeneration, 1);
            Need(header.SaveGeneration == expectedGeneration && header.ParentCommitId == basis?.Header.CommitId,
                "Header", "InconsistentBinding");
            var count = candidate.Records.Count;
            Need(count > 0 && header.CommitIndex.Count == count, "CommitIndex", "IncompleteOperationHistory");
            var records = new List<CandidateApplicationRecord>(candidate.Records);
            var last = records[count - 1];
            Need(!last.Generation.HasValue && last.CommitId == null, "Records.Pending", "InvalidValue");
            records[count - 1] = new CandidateApplicationRecord(last.Intent, last.Result, header.SaveGeneration, header.CommitId);
            CheckIndex(records, header, budget);
            if (basis != null) for (var i = 0; i < basis.Header.CommitIndex.Count; i++)
            {
                var old = basis.Header.CommitIndex[i];
                var next = header.CommitIndex[i];
                var wasCurrent = i == basis.Header.CommitIndex.Count - 1;
                Need(old.Generation == next.Generation && old.CommitId == next.CommitId && old.ParentCommitId == next.ParentCommitId &&
                    CandidateApplicationReferences.SameStrings(old.OperationIds, next.OperationIds), "CommitIndex.History", "InconsistentBinding");
                var length = wasCurrent ? basis.Descriptor.TotalLength : old.SnapshotLength;
                var digest = wasCurrent ? basis.Descriptor.Sha256 : old.SnapshotSha256;
                Need(length == next.SnapshotLength && SameDigest(digest, next.SnapshotSha256), "CommitIndex.HistoryDigest", "InconsistentBinding");
            }
            return records;
        }

        private static bool SameDigest(IReadOnlyList<byte> a, IReadOnlyList<byte> b)
        { return a == null || b == null ? a == b : CandidateApplicationIntentCodec.SameBytes(a, b); }

        private static void CheckIndex(IReadOnlyList<CandidateApplicationRecord> records, CandidateBusinessSaveHeader header, SaveCodecBudget budget)
        {
            SaveEnvelopeCodec.CheckList(header.CommitIndex, budget, "CommitIndex");
            Need(records.Count > 0 && records.Count == header.CommitIndex.Count && header.SaveGeneration == records.Count,
                "CommitIndex.Count", "IncompleteOperationHistory");
            var commits = new HashSet<string>(StringComparer.Ordinal);
            var f = new BusinessFields(Stream.Null, false, budget);
            for (var i = 0; i < records.Count; i++)
            {
                var row = records[i];
                var index = header.CommitIndex[i];
                Need(index != null, "CommitIndex.Entry", "MissingField");
                SaveEnvelopeCodec.CheckList(index.OperationIds, budget, "CommitIndex.OperationIds");
                f.Integer(index.Generation, "CommitIndex.Generation", 1);
                f.Text(index.CommitId, "CommitIndex.CommitId");
                Need(row.Generation == i + 1 && index.Generation == row.Generation && row.CommitId == index.CommitId &&
                    index.ParentCommitId == (i == 0 ? null : records[i - 1].CommitId) && commits.Add(index.CommitId) &&
                    index.OperationIds.Count == 1 && index.OperationIds[0] == row.OperationId, "CommitIndex.Entry", "IncompleteOperationHistory");
                if (row.Intent.Kind == CandidateApplicationKind.MigrateRoster || row.Intent.Kind == CandidateApplicationKind.MigratePermanent)
                {
                    var source = row.Intent.Data.MigrateRoster ?? row.Intent.GetPermanentMigration();
                    Need(i > 0 && source.SourceGeneration == i && source.SourceDescriptorLength == header.CommitIndex[i - 1].SnapshotLength &&
                        SameDigest(source.SourceDescriptorSha256, header.CommitIndex[i - 1].SnapshotSha256),
                        "Migration.SourceDescriptor", "InconsistentBinding");
                }
                if (row.Intent.Kind == CandidateApplicationKind.PermanentRequest)
                {
                    var source = row.Intent.GetPermanent();
                    Need(i > 0 && source.SourceGeneration == i && source.SourceDescriptorLength == header.CommitIndex[i - 1].SnapshotLength &&
                        SameDigest(source.SourceDescriptorSha256, header.CommitIndex[i - 1].SnapshotSha256),
                        "Permanent.SourceDescriptor", "InconsistentBinding");
                }
                var current = i == records.Count - 1;
                Need(current ? index.SnapshotLength == null && index.SnapshotSha256 == null :
                    index.SnapshotLength > 0 && index.SnapshotSha256 != null && index.SnapshotSha256.Count == 32,
                    "CommitIndex.Snapshot", "InconsistentBinding");
            }
            var tail = header.CommitIndex[records.Count - 1];
            Need(header.CommitId == tail.CommitId && header.ParentCommitId == tail.ParentCommitId,
                "Header.CommitId", "InconsistentBinding");
        }

        private static RequiredSliceContract Contract(int i, bool published, bool roster, bool permanent) { return new RequiredSliceContract(SliceIds[i], "M0" + (i + 2), permanent && i < 4 ? 4U : roster && i < 4 ? 3U : published ? 2U : 1U); }
        private static void CheckContract(RequiredSliceContract actual, int i, bool published, bool roster, bool permanent)
        {
            Need(actual.SchemaVersion == (permanent && i < 4 ? 4 : roster && i < 4 ? 3 : published ? 2 : 1) && actual.SliceId == SliceIds[i] && actual.OwnerId == "M0" + (i + 2),
                "Slices[" + i + "].Contract", "UnsupportedSchema");
        }

        private static List<SaveSliceInput> Slices(SaveEnvelope envelope, int start)
        {
            var slices = new List<SaveSliceInput>();
            for (var i = start; i < envelope.Bodies.Length; i++)
                slices.Add(new SaveSliceInput { Contract = envelope.RequiredSliceContracts[i], Bytes = envelope.Bodies[i], Requirements = envelope.SliceDirectory[i].Requirements });
            return slices;
        }

        private static SaveEnvelope Envelope(SaveEnvelope basis, List<SaveSliceInput> slices, SaveCodecBudget budget)
        {
            var contracts = new List<RequiredSliceContract>();
            foreach (var slice in slices) contracts.Add(slice.Contract);
            return Take(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = basis.Purpose, PlayerId = basis.PlayerId,
                SaveGeneration = basis.SaveGeneration, CommitId = basis.CommitId, ParentCommitId = basis.ParentCommitId,
                CommitIndex = basis.CommitIndex, RequiredSliceContracts = contracts, Slices = slices }, budget), "Envelope");
        }

        private sealed class WireRecord
        {
            internal PreparedCandidateApplicationIntent Intent;
            internal BigInteger Generation;
            internal string CommitId, FormationOperation;
            internal CandidateApplicationResultInput Result;
            internal CandidatePermanentResult Permanent;
            internal readonly List<WireRecovery> Recoveries = new List<WireRecovery>();

            internal CandidateApplicationRecord Resolve(CandidateBusinessSnapshot business, SaveCodecBudget budget)
            {
                var recoveries = new List<CandidateApplicationRecoveryReceipt>();
                foreach (var wire in Recoveries) recoveries.Add(wire.Resolve(business, budget));
                CandidateFormationReceipt formation = null;
                if (Intent.Kind == CandidateApplicationKind.SetFormation)
                {
                    Need(FormationOperation == Intent.OperationId, "M02.Formation", "InconsistentBinding");
                    formation = CandidateRosterProtocol.Formation(business, FormationOperation);
                }
                var migration = Intent.Kind == CandidateApplicationKind.MigrateRoster ? new CandidateRosterMigrationReceipt(Intent) : null;
                return new CandidateApplicationRecord(Intent, new CandidateApplicationResult(Result, null, formation, migration,
                    recoveries.Count == 0 && Intent.Kind != CandidateApplicationKind.EnterFormation ? null : recoveries, Permanent), Generation, CommitId);
            }
        }

        private sealed class WireRecovery
        {
            internal string CharacterId, RecoveryId, End;
            internal BigInteger Revision;
            internal CandidateGrowthOutcome Outcome;
            internal PreparedCandidateTimeSample Last;
            internal ExactRational Elapsed;
            internal CandidateTimeAnomaly PeriodAnomaly, ResultAnomaly;
            internal CandidateApplicationRecoveryReceipt Resolve(CandidateBusinessSnapshot business, SaveCodecBudget budget)
            {
                var current = CandidateApplicationReferences.Period(business, RecoveryId, CharacterId);
                Need(current.EndReceipt.EndReceiptId == End, "M02.Recovery.EndReceiptId", "InconsistentBinding");
                var period = new CandidateRecoveryPeriod(current.EndReceipt, current.Definition, Last, Elapsed, PeriodAnomaly, budget.Math);
                return new CandidateApplicationRecoveryReceipt(Revision, Outcome, period, ResultAnomaly);
            }
        }
        private static WireRecovery Recovery(BusinessFields f, CandidateApplicationRecoveryReceipt r, string p)
        {
            var wire = new WireRecovery { End = f.Text(r?.EndReceiptId, p + ".EndReceiptId"),
                Revision = f.Integer(r?.AfterCharacterRevision ?? 0, p + ".AfterCharacterRevision", 1),
                Outcome = (CandidateGrowthOutcome)f.Enum((int)(r?.Outcome ?? 0), 1, 4, p + ".Outcome") };
            var sample = CandidateApplicationIntentCodec.Time(f,
                f.Reading ? null : CandidateApplicationIntentCodec.TimeInput(r.Period.LastAcceptedSample), p + ".LastAcceptedSample");
            wire.Last = CandidateApplicationIntentCodec.PrepareTime(sample, f.Budget, p + ".LastAcceptedSample");
            wire.Elapsed = f.Rational(r?.Period.Elapsed, p + ".Elapsed");
            wire.PeriodAnomaly = (CandidateTimeAnomaly)f.Enum((int)(r?.Period.Anomaly ?? 0), 0, 3, p + ".PeriodAnomaly");
            wire.ResultAnomaly = (CandidateTimeAnomaly)f.Enum((int)(r?.ResultAnomaly ?? 0), 0, 3, p + ".ResultAnomaly");
            return wire;
        }

        private static List<WireRecord> Body(BusinessFields f, string player, IReadOnlyList<CandidateApplicationRecord> records,
            CandidateApplicationContinuation route, out CandidateApplicationContinuation readRoute)
        {
            f.Header(2, player, "M02");
            var count = f.U((ulong)(records?.Count ?? 0), 4, "M02.Records.Count");
            SaveCodecFailure.Limit(count, (ulong)f.Budget.MaxCollectionEntries, "M02.Records", "CollectionEntries");
            f.Space(count * 15, "M02.Records");
            var values = new List<WireRecord>((int)count);
            for (var i = 0; i < (int)count; i++)
            {
                var row = f.Reading ? null : records[i];
                var p = "M02.Records[" + i + "]";
                var length = f.U((ulong)(row?.Intent.Bytes.Length ?? 0), 4, p + ".Intent.Length");
                SaveCodecFailure.Limit(length, f.Budget.MaxEnvelopeBytes, p + ".Intent", "EnvelopeBytes");
                SaveCodecFailure.Limit(length, int.MaxValue, p + ".Intent", "ArrayLength");
                f.Space(length, p + ".Intent");
                var bytes = f.Reading ? new byte[(int)length] : row.Intent.Bytes;
                for (var j = 0; j < bytes.Length; j++)
                {
                    var b = (byte)f.U(bytes[j], 1, p + ".Intent");
                    if (f.Reading) bytes[j] = b;
                }
                var wire = new WireRecord { Intent = f.Reading ? CandidateApplicationIntentCodec.Read(bytes, f.Budget) : row.Intent,
                    Generation = f.Integer(row?.Generation ?? 0, p + ".Generation", 1), CommitId = f.Text(row?.CommitId, p + ".CommitId"),
                    Result = new CandidateApplicationResultInput() };
                var input = row?.Result;
                Need(f.Resolved == null ? wire.Intent.Context is PreparedCandidateContext : wire.Intent.Context is PreparedPublishedRuleContext,
                    p + ".Intent.Context", "UnsupportedBinding");
                if (f.Resolved != null) Need(f.Resolved.FindExact(((PreparedPublishedRuleContext)wire.Intent.Context).Binding) != null,
                    p + ".Intent.Context", "UnsupportedBinding");
                var kind = wire.Intent.Kind;
                if (kind == CandidateApplicationKind.PermanentRequest)
                {
                    var quote = wire.Intent.GetPermanent();
                    Need(quote.Binding.Same(((PreparedPublishedRuleContext)wire.Intent.Context).Binding), p + ".Permanent.Binding");
                    if (quote.Kind != CandidatePermanentKind.Equip && quote.Kind != CandidatePermanentKind.SetPreference)
                    {
                        var definitions = f.Resolved?.FindExact(quote.Binding)?.GetPermanentDefinitions();
                        Need(definitions != null, p + ".Permanent.Definitions", "UnsupportedBinding");
                        definitions.CheckQuote(quote, f.Budget);
                        CandidatePermanentProgression.CheckHistory(quote,
                            values.Select(x => (x.Intent.OperationId, x.Intent.GetPermanent())), definitions);
                    }
                }
                Need(wire.Intent.FormatVersion <= (f.SchemaVersion >= 3 ? f.SchemaVersion : 2), p + ".Intent.FormatVersion", "UnsupportedSchema");
                if (kind == CandidateApplicationKind.EnterAttempt || kind == CandidateApplicationKind.EnterFormation)
                {
                    wire.Result.ChallengeId = f.Text(input?.ChallengeId, p + ".ChallengeId");
                    wire.Result.AttemptId = f.Text(input?.AttemptId, p + ".AttemptId");
                    wire.Result.EntryBaselineId = f.Text(input?.EntryBaselineId, p + ".EntryBaselineId");
                }
                if (kind == CandidateApplicationKind.Attack || kind == CandidateApplicationKind.Link)
                    wire.Result.HistoryAnchorId = f.Text(input?.HistoryAnchorId, p + ".HistoryAnchorId");
                if (kind == CandidateApplicationKind.SettleVictory || kind == CandidateApplicationKind.ExitAttempt || kind == CandidateApplicationKind.RestartAttempt)
                    wire.Result.EndReceiptId = f.Text(input?.EndReceiptId, p + ".EndReceiptId");
                if (kind == CandidateApplicationKind.SettleVictory) wire.Result.SettlementId = f.Text(input?.SettlementId, p + ".SettlementId");
                if (kind == CandidateApplicationKind.RestartAttempt) wire.Result.NewAttemptId = f.Text(input?.NewAttemptId, p + ".NewAttemptId");
                if (kind == CandidateApplicationKind.AdvanceRecovery)
                {
                    var recovery = Recovery(f, input?.Recovery, p);
                    recovery.CharacterId = wire.Intent.Data.AdvanceRecovery.CharacterId;
                    recovery.RecoveryId = wire.Intent.Data.AdvanceRecovery.RecoveryId;
                    wire.Recoveries.Add(recovery);
                }
                if (kind == CandidateApplicationKind.EnterFormation)
                {
                    var recoveries = f.List(input?.RecoveryResults, (r, field) => {
                        var character = f.Text(r?.CharacterId, field + ".CharacterId");
                        var recoveryId = f.Text(r?.RecoveryId, field + ".RecoveryId");
                        var recovery = Recovery(f, r, field); recovery.CharacterId = character; recovery.RecoveryId = recoveryId;
                        wire.Recoveries.Add(recovery); return r;
                    }, p + ".Recoveries");
                }
                if (kind == CandidateApplicationKind.SetFormation)
                    wire.FormationOperation = f.Text(input?.Formation.OperationId, p + ".Formation.OperationId");
                if (kind == CandidateApplicationKind.MigrateRoster)
                    Need(f.U(2, 4, p + ".Migration.From") == 2 && f.U(3, 4, p + ".Migration.To") == 3, p + ".Migration", "UnsupportedSchema");
                if (kind == CandidateApplicationKind.PermanentRequest)
                    wire.Permanent = new CandidatePermanentCodec(f).Result(input?.GetPermanent(), p + ".Permanent");
                if (kind == CandidateApplicationKind.MigratePermanent)
                    Need(f.U(3, 4, p + ".Migration.From") == 3 && f.U(4, 4, p + ".Migration.To") == 4, p + ".Migration", "UnsupportedSchema");
                values.Add(wire);
            }
            var routes = f.U(route == null ? 0UL : 1UL, 4, "M02.Continuation.Count");
            SaveCodecFailure.Limit(routes, (ulong)f.Budget.MaxCollectionEntries, "M02.Continuation", "CollectionEntries");
            Need(routes <= 1, "M02.Continuation.Count", "InvalidContinuation");
            readRoute = routes == 0 ? null : new CandidateApplicationContinuation(f.Text(route?.ClosingOperationId, "M02.Continuation.ClosingOperationId"),
                f.Text(route?.AttemptId, "M02.Continuation.AttemptId"), f.Text(route?.FinalReportFingerprint, "M02.Continuation.FinalReportFingerprint"),
                f.Text(route?.ReservedOperationId, "M02.Continuation.ReservedOperationId"));
            return values;
        }

        private static SaveRequirements Requirements(IReadOnlyList<CandidateApplicationRecord> records,
            Dictionary<string, CandidateApplicationResolution> resolved, SaveCodecBudget budget, bool published, bool roster, bool permanent)
        {
            var bindings = new List<SaveBinding>();
            var contexts = new List<PreparedRuleContext>();
            var rules = new List<string>();
            var numeric = new List<string>();
            var random = new List<string>();
            foreach (var row in records)
            {
                var c = row.Intent.Context;
                var begin = resolved[row.OperationId].Begin;
                var teaching = row.Intent.GetPermanent()?.TeachingLevel;
                var binding = RuleContextChecks.SaveBinding(c, budget, teaching?.LevelId ?? begin?.Level.LevelId, teaching?.CanonicalLevelVersion ?? begin?.Level.LevelVersion);
                var exists = false;
                for (var i = 0; i < bindings.Count; i++)
                    if (bindings[i].Kind == binding.Kind && bindings[i].LevelId == binding.LevelId && bindings[i].LevelVersion == binding.LevelVersion &&
                        CandidateApplicationReferences.SameContext(contexts[i], c)) { exists = true; break; }
                if (!exists)
                {
                    SaveCodecFailure.Limit((ulong)bindings.Count + 1, (ulong)budget.MaxCollectionEntries, "M02.Requirements.Bindings", "CollectionEntries");
                    bindings.Add(binding);
                    contexts.Add(c);
                }
                if (!rules.Contains(c.RuleVersion)) rules.Add(c.RuleVersion);
                if (!numeric.Contains(c.NumericContractVersion)) numeric.Add(c.NumericContractVersion);
                if (!random.Contains(c.RandomContractVersion)) random.Add(c.RandomContractVersion);
            }
            return new SaveRequirements(bindings.AsReadOnly(), rules.AsReadOnly(), numeric.AsReadOnly(), random.AsReadOnly(),
                Array.AsReadOnly(permanent ? new[] { "fm.player.application.v1", "fm.player.roster.v1", "fm.player.permanent.v1" } : roster ? new[] { "fm.player.application.v1", "fm.player.roster.v1" } : new[] { published ? "fm.player.application.v1" : "fm.candidate.application.v1" }));
        }
    }
}
