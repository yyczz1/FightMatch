using System;
using System.Collections.Generic;
using System.IO;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    public static class CandidateBusinessSaveCodec
    {
        private static readonly string[] Names = { "character", "inventory", "progression", "battle", "rewards" };

        internal static bool SamePermanentOwners(CandidateRosterState leftRoster, CandidateInventoryState leftInventory,
            CandidateRosterState rightRoster, CandidateInventoryState rightInventory, SaveCodecBudget budget)
        {
            return Bytes(PermanentOwnerBytes(leftRoster, leftInventory, budget), PermanentOwnerBytes(rightRoster, rightInventory, budget));
        }

        private static byte[] PermanentOwnerBytes(CandidateRosterState roster, CandidateInventoryState inventory, SaveCodecBudget budget)
        {
            using (var stream = new MemoryStream())
            {
                var fields = new BusinessFields(stream, false, budget) {
                    SchemaVersion = 4, Roster = roster, ValidatePublished = true, StrictUnicode = true };
                new CandidateRosterSaveCodec(fields).Roster(roster, "M03");
                new CandidatePermanentSaveCodec(fields).Inventory(inventory, "M04");
                return stream.ToArray();
            }
        }
        public static SaveCodecResult<CandidateBusinessSnapshot> Prepare(CandidateBusinessInput input, SaveCodecBudget budget)
        { return Prepare(input, SavePurpose.CandidateValidation, budget); }
        public static SaveCodecResult<CandidateBusinessSnapshot> Prepare(CandidateBusinessInput input, SavePurpose purpose, SaveCodecBudget budget)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateBusinessSnapshot>.Run(() =>
            {
                Need(!string.IsNullOrWhiteSpace(input.PlayerId), "PlayerId", "MissingField");
                Need(input.Roster != null, "Character", "MissingField"); Need(input.Inventory != null, "Inventory", "MissingField");
                Need(input.Progression != null, "Progression", "MissingField"); Need(input.Rewards != null, "Rewards", "MissingField");
                SaveEnvelopeCodec.CheckList(input.RetainedRuns, budget, "RetainedRuns"); SaveEnvelopeCodec.CheckList(input.RetainedRollbacks, budget, "RetainedRollbacks");
                Need(purpose == SavePurpose.CandidateValidation || purpose == SavePurpose.PlayerSave, "Purpose", "UnsupportedBinding");
                Need(purpose == SavePurpose.PlayerSave ? input.Format == CandidateBusinessFormat.PublishedV2 ||
                    (input.Format == CandidateBusinessFormat.PublishedRosterV3 || input.Format == CandidateBusinessFormat.PublishedPermanentV4) : input.Format == CandidateBusinessFormat.CandidateV1, "Format", "UnsupportedBinding");
                var snapshot = new CandidateBusinessSnapshot(input); Verify(snapshot, budget, out _, null, purpose == SavePurpose.PlayerSave); return snapshot;
            });
        }
        public static SaveCodecResult<SaveEnvelope> Encode(CandidateBusinessSnapshot snapshot, CandidateBusinessSaveHeader header, SaveCodecBudget budget)
        { return EncodeCore(snapshot, header, null, budget); }
        public static SaveCodecResult<SaveEnvelope> EncodePublished(CandidateBusinessSnapshot snapshot, CandidateBusinessSaveHeader header,
            PublishedSaveContext resolved, SaveCodecBudget budget)
        { if (resolved == null) throw new ArgumentNullException(nameof(resolved)); return EncodeCore(snapshot, header, resolved, budget); }
        private static SaveCodecResult<SaveEnvelope> EncodeCore(CandidateBusinessSnapshot snapshot, CandidateBusinessSaveHeader header,
            PublishedSaveContext resolved, SaveCodecBudget budget)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (header == null) throw new ArgumentNullException(nameof(header));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<SaveEnvelope>.Run(() =>
            {
                Need((int)snapshot.Format < 3 || resolved != null, "Format", "UnsupportedBinding");
                var lengths = Verify(snapshot, budget, out var battle, resolved);
                var slices = Write(snapshot, battle, budget, lengths, resolved); var contracts = new List<RequiredSliceContract>();
                foreach (var slice in slices) contracts.Add(slice.Contract);
                return Take(SaveEnvelopeCodec.Prepare(new SaveEnvelopeInput { Purpose = resolved == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave, PlayerId = snapshot.PlayerId,
                    SaveGeneration = header.SaveGeneration, CommitId = header.CommitId, ParentCommitId = header.ParentCommitId,
                    CommitIndex = header.CommitIndex, RequiredSliceContracts = contracts, Slices = slices }, budget), "Envelope");
            });
        }
        public static SaveCodecResult<CandidateBusinessSnapshot> Decode(SaveEnvelope envelope, SaveCodecBudget budget)
        { return DecodeCore(envelope, null, budget); }
        public static SaveCodecResult<CandidateBusinessSnapshot> DecodePublished(SaveEnvelope envelope, PublishedSaveContext resolved, SaveCodecBudget budget)
        { if (resolved == null) throw new ArgumentNullException(nameof(resolved)); return DecodeCore(envelope, resolved, budget); }
        private static SaveCodecResult<CandidateBusinessSnapshot> DecodeCore(SaveEnvelope envelope, PublishedSaveContext resolved, SaveCodecBudget budget)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            return SaveCodecResult<CandidateBusinessSnapshot>.Run(() =>
            {
                Need(envelope.Purpose == (resolved == null ? SavePurpose.CandidateValidation : SavePurpose.PlayerSave), "Envelope.Purpose", "UnsupportedBinding");
                Need(envelope.SliceDirectory.Count == 5 && envelope.RequiredSliceContracts.Count == 5 && envelope.Bodies.Length == 5, "Envelope.Slices", "UnsupportedSchema");
                var rosterFormat = resolved != null && (envelope.SliceDirectory[0].Contract.SchemaVersion == 3 || envelope.SliceDirectory[0].Contract.SchemaVersion == 4);
                var permanentFormat = resolved != null && envelope.SliceDirectory[0].Contract.SchemaVersion == 4;
                var total = 0UL;
                for (var i = 0; i < 5; i++)
                {
                    Contract(envelope.SliceDirectory[i].Contract, i, resolved, rosterFormat, permanentFormat); Contract(envelope.RequiredSliceContracts[i], i, resolved, rosterFormat, permanentFormat);
                    total += (ulong)envelope.Bodies[i].Length; SaveCodecFailure.Limit(total, budget.MaxEnvelopeBytes, "Slices", "EnvelopeBytes");
                }
                var battle = new CandidateBattleSaveCodec(budget); CandidateCharacterState character = null; CandidateRosterState roster = null; CandidateInventoryState inventory = null;
                CandidateProgressionState progression = null; CandidateRewardState rewards = null; CandidateBattleHistory history = null;
                List<CandidateBattleRun> retainedRuns = null; List<CandidateRollbackRecord> retainedRollbacks = null;
                Dictionary<string, CandidateProgressionEndReceipt> endings = null;
                for (var i = 0; i < 5; i++) using (var stream = new MemoryStream(envelope.Bodies[i], false))
                {
                    var fields = new BusinessFields(stream, true, budget, (ulong)stream.Length) { Resolved = resolved, StrictUnicode = resolved != null,
                        SchemaVersion = NewContract(i, resolved, rosterFormat, permanentFormat).SchemaVersion, Roster = roster }; var p = "M0" + (i + 3);
                    fields.Header(i + 3, envelope.PlayerId, p); var permanent = new CandidatePermanentSaveCodec(fields);
                    switch (i)
                    {
                        case 0:
                            if (rosterFormat) roster = new CandidateRosterSaveCodec(fields).Roster(null, p);
                            else character = permanent.Character(null, p);
                            break;
                        case 1: inventory = permanent.Inventory(null, p); break;
                        case 2: progression = permanent.Progression(null, p); endings = permanent.Endings; break;
                        case 3: battle.Body(fields, null, out history, out retainedRuns, out retainedRollbacks); break;
                        case 4: rewards = new CandidateRewardSaveCodec(fields, battle, endings).State(null, p); break;
                    }
                    fields.End(p);
                }
                var input = rosterFormat
                    ? new CandidateBusinessInput(envelope.PlayerId, roster, inventory, progression, rewards, history, retainedRuns, retainedRollbacks, permanentFormat ? CandidateBusinessFormat.PublishedPermanentV4 : CandidateBusinessFormat.PublishedRosterV3)
                    : new CandidateBusinessInput(envelope.PlayerId, character, inventory, progression, rewards, history, retainedRuns, retainedRollbacks);
                var snapshot = new CandidateBusinessSnapshot(input);
                var lengths = Verify(snapshot, budget, out var collected, resolved); var canonical = Write(snapshot, collected, budget, lengths, resolved);
                for (var i = 0; i < 5; i++)
                {
                    // This also rejects unreachable table rows, duplicate definitions and noncanonical reference numbering.
                    Need(Bytes(canonical[i].Bytes, envelope.Bodies[i]), "M0" + (i + 3) + ".CanonicalLayout", "Malformed");
                    Need(BusinessRequirements.Same(canonical[i].Requirements, envelope.SliceDirectory[i].Requirements), "M0" + (i + 3) + ".Requirements", "InconsistentBinding");
                }
                return snapshot;
            });
        }
        private static int[] Verify(CandidateBusinessSnapshot snapshot, SaveCodecBudget budget, out CandidateBattleSaveCodec battle,
            PublishedSaveContext resolved = null, bool validatePublished = false)
        {
            battle = new CandidateBattleSaveCodec(budget); CandidateBusinessRestoreChecks.Shape(snapshot, budget); battle.Collect(snapshot);
            var lengths = Measure(snapshot, battle, budget, resolved, validatePublished); CandidateBusinessRestoreChecks.Check(snapshot, battle, budget); return lengths;
        }
        private static int[] Measure(CandidateBusinessSnapshot snapshot, CandidateBattleSaveCodec battle, SaveCodecBudget budget,
            PublishedSaveContext resolved, bool validatePublished)
        {
            var lengths = new int[5]; var total = 0UL; Dictionary<string, CandidateProgressionEndReceipt> endings = null;
            for (var i = 0; i < 5; i++)
            {
                var f = new BusinessFields(Stream.Null, false, budget) { Resolved = resolved, ValidatePublished = validatePublished, StrictUnicode = resolved != null };
                Body(f, i, snapshot, battle, ref endings);
                BusinessRequirements.For(snapshot, battle, i, budget);
                total += f.Used; SaveCodecFailure.Limit(total, budget.MaxEnvelopeBytes, "Slices", "EnvelopeBytes"); lengths[i] = (int)f.Used;
            }
            return lengths;
        }
        private static List<SaveSliceInput> Write(CandidateBusinessSnapshot snapshot, CandidateBattleSaveCodec battle, SaveCodecBudget budget, int[] lengths, PublishedSaveContext resolved)
        {
            var slices = new List<SaveSliceInput>(); Dictionary<string, CandidateProgressionEndReceipt> endings = null;
            for (var i = 0; i < 5; i++)
            {
                var body = new byte[lengths[i]];
                using (var stream = new MemoryStream(body, true)) Body(new BusinessFields(stream, false, budget) { Resolved = resolved, StrictUnicode = resolved != null }, i, snapshot, battle, ref endings);
                slices.Add(new SaveSliceInput { Contract = NewContract(i, resolved, (int)snapshot.Format >= 3, snapshot.Format == CandidateBusinessFormat.PublishedPermanentV4), Bytes = body, Requirements = BusinessRequirements.For(snapshot, battle, i, budget) });
            }
            return slices;
        }
        private static void Body(BusinessFields f, int index, CandidateBusinessSnapshot snapshot, CandidateBattleSaveCodec battle,
            ref Dictionary<string, CandidateProgressionEndReceipt> endings)
        {
            var p = "M0" + (index + 3);
            if ((int)snapshot.Format >= 3) f.SchemaVersion = index < 3 ? (uint)snapshot.Format : 2U;
            f.Roster = snapshot.Roster;
            f.Header(index + 3, snapshot.PlayerId, p); var permanent = new CandidatePermanentSaveCodec(f);
            switch (index)
            {
                case 0:
                    if ((int)snapshot.Format >= 3) new CandidateRosterSaveCodec(f).Roster(snapshot.Roster, p);
                    else permanent.Character(snapshot.Character, p);
                    break;
                case 1: permanent.Inventory(snapshot.Inventory, p); break;
                case 2: permanent.Progression(snapshot.Progression, p); endings = permanent.Endings; break;
                case 3: battle.Body(f, snapshot, out _, out _, out _); break;
                case 4: new CandidateRewardSaveCodec(f, battle, endings).State(snapshot.Rewards, p); break;
            }
        }
        private static RequiredSliceContract NewContract(int index, PublishedSaveContext resolved, bool roster = false, bool permanent = false)
        { return new RequiredSliceContract("fm.m0" + (index + 3) + "." + Names[index], "M0" + (index + 3), permanent && index < 3 ? 4U : roster && index < 3 ? 3U : resolved == null ? 1U : 2U); }
        private static void Contract(RequiredSliceContract actual, int index, PublishedSaveContext resolved, bool roster = false, bool permanent = false)
        { var expected = NewContract(index, resolved, roster, permanent); Need(actual.SchemaVersion == expected.SchemaVersion, "Slices[" + index + "].SchemaVersion", "UnsupportedSchema");
            Need(actual.SliceId == expected.SliceId && actual.OwnerId == expected.OwnerId, "Slices[" + index + "].Contract", "UnsupportedSchema"); }
        private static bool Bytes(byte[] a, byte[] b)
        { if (a.Length != b.Length) return false; for (var i = 0; i < a.Length; i++) if (a[i] != b[i]) return false; return true; }
    }

    internal sealed class BusinessRequirements
    {
        private readonly List<SaveBinding> bindings = new List<SaveBinding>();
        private readonly List<string> rules = new List<string>(), numeric = new List<string>(), random = new List<string>(), features = new List<string>();
        private readonly SaveCodecBudget budget;
        private BusinessRequirements(SaveCodecBudget budget) { this.budget = budget; }
        internal static SaveRequirements For(CandidateBusinessSnapshot s, CandidateBattleSaveCodec battle, int owner, SaveCodecBudget budget)
        {
            var r = new BusinessRequirements(budget);
            if ((int)s.Format >= 3 && owner < 3) r.Unique(r.features, "fm.player.roster.v1");
            if (s.Format == CandidateBusinessFormat.PublishedPermanentV4 && owner < 3)
            {
                r.Unique(r.features, "fm.player.permanent.v1");
                var effects = owner == 0 ? s.Roster.GetPermanentEffects() : owner == 1 ? s.Inventory.GetPermanentLedger().Effects : s.Progression.GetPermanentEffects();
                foreach (var effect in effects)
                {
                    var q = effect.Quote;
                    r.Add(new PreparedPublishedRuleContext(q.Binding), q.TeachingLevel?.LevelId, q.TeachingLevel?.CanonicalLevelVersion);
                    foreach (var input in q.Inputs) if (input.Source != null) r.Add(new PreparedPublishedRuleContext(input.Source.Grant.Binding));
                }
                if (owner == 1) foreach (var source in s.Inventory.GetPermanentLedger().Sources) r.Add(new PreparedPublishedRuleContext(source.Grant.Binding));
            }
            if (owner == 0)
            {
                foreach (var character in s.Roster.Characters)
                {
                    r.Add(character.Definition.Context); foreach (var x in character.BaseRewards) r.Add(x.Context);
                    foreach (var x in character.ProcessedEnds) r.Add(x.Context);
                    foreach (var x in character.RecoveryPeriods) { r.Add(x.EndReceipt.Context); r.Add(x.Definition.Context); }
                }
            }
            if (owner == 1)
            {
                r.Add(s.Inventory.Definition.Context); if (s.Inventory.ActiveCarry != null) r.Add(s.Inventory.ActiveCarry.Context);
                foreach (var x in s.Inventory.OrdinaryGrants) r.Add(x.Context);
                foreach (var x in s.Inventory.Ends) { r.Add(x.OriginalCarry.Context); if (x.RewardReceipt != null) r.Add(x.RewardReceipt.Context); }
            }
            if (owner == 2)
            {
                foreach (var x in s.Progression.Definition.Levels) r.Level(s.Progression.Definition.Context, x);
                foreach (var c in s.Progression.Challenges) foreach (var a in c.Attempts) r.Level(a.Begin.Context, a.Begin.Level);
                foreach (var x in s.Progression.FirstClears) r.Level(x.Context, x.End.Begin.Level);
                foreach (var x in s.Progression.OpenFacts) r.Level(x.Context, x.Level);
            }
            if (owner == 3) foreach (var x in battle.Baselines.Rows) r.Entry(x.Entry);
            if (owner == 4) foreach (var x in s.Rewards.BaseRewards)
            { r.Entry(x.Report.Baseline.Entry); r.Add(x.Definition.Context, x.Definition.LevelId, x.Definition.LevelVersion); foreach (var feature in x.Definition.RequiredFeatures) r.Unique(r.features, feature); r.Level(x.Ending.Begin.Context, x.Ending.Begin.Level); }
            return new SaveRequirements(r.bindings.AsReadOnly(), r.rules.AsReadOnly(), r.numeric.AsReadOnly(), r.random.AsReadOnly(), r.features.AsReadOnly());
        }
        private void Entry(PreparedBattleEntry x)
        { Add(x.Context, x.Level.LevelId, x.Level.LevelVersion); foreach (var m in x.Members) Add(m.StatsContext, x.Level.LevelId, x.Level.LevelVersion); foreach (var feature in x.RequiredFeatures) Unique(features, feature); }
        private void Level(PreparedRuleContext context, CandidateProgressionLevel level)
        { Add(context, level.LevelId, level.LevelVersion); foreach (var feature in level.RequiredFeatures) Unique(features, feature); }
        private void Add(PreparedRuleContext context, string level = null, string version = null)
        {
            var b = RuleContextChecks.SaveBinding(context, budget, level, version);
            foreach (var existing in bindings) if (BindingSame(existing, b)) return;
            SaveCodecFailure.Limit((ulong)bindings.Count + 1, (ulong)budget.MaxCollectionEntries, "Requirements.Bindings", "CollectionEntries"); bindings.Add(b);
            Unique(rules, b.RuleVersion); Unique(numeric, b.NumericContractVersion); Unique(random, b.RandomContractVersion);
        }
        private void Unique(List<string> list, string value)
        { if (list.Contains(value)) return; SaveCodecFailure.Limit((ulong)list.Count + 1, (ulong)budget.MaxCollectionEntries, "Requirements", "CollectionEntries"); list.Add(value); }
        internal static bool Same(SaveRequirements a, SaveRequirements b)
        {
            if (a.Bindings.Count != b.Bindings.Count || !StringsSame(a.RuleVersions, b.RuleVersions) || !StringsSame(a.NumericContractVersions, b.NumericContractVersions) ||
                !StringsSame(a.RandomContractVersions, b.RandomContractVersions) || !StringsSame(a.FeatureIds, b.FeatureIds)) return false;
            for (var i = 0; i < a.Bindings.Count; i++) if (!BindingSame(a.Bindings[i], b.Bindings[i])) return false; return true;
        }
        private static bool BindingSame(SaveBinding a, SaveBinding b)
        { return a.Kind == b.Kind && a.PackageId == b.PackageId && a.DraftId == b.DraftId && a.DraftRevision == b.DraftRevision && a.ContentFingerprint == b.ContentFingerprint &&
            a.RuleVersion == b.RuleVersion && a.NumericContractVersion == b.NumericContractVersion && a.RandomContractVersion == b.RandomContractVersion &&
            a.LevelId == b.LevelId && a.LevelVersion == b.LevelVersion && StringsSame(a.SourceNotes, b.SourceNotes); }
        private static bool StringsSame(IReadOnlyList<string> a, IReadOnlyList<string> b)
        { if (a == null || b == null) return a == b; if (a.Count != b.Count) return false; for (var i = 0; i < a.Count; i++) if (a[i] != b[i]) return false; return true; }
    }
}
