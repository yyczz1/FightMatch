using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidatePermanentSaveCodec
    {
        private readonly BusinessFields f;
        private readonly BusinessTable<CandidateGrowthDefinition> growth = new BusinessTable<CandidateGrowthDefinition>();
        internal readonly Dictionary<string, CandidateProgressionEndReceipt> Endings = new Dictionary<string, CandidateProgressionEndReceipt>(StringComparer.Ordinal);
        internal CandidatePermanentSaveCodec(BusinessFields fields) { f = fields; }
        internal CandidateCharacterState Character(CandidateCharacterState x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId"); var character = f.Text(x?.CharacterId, p + ".CharacterId");
            var definition = growth.Inline(f, x?.Definition, Growth, p + ".Definition");
            var level = f.Integer(x?.Level ?? 0, p + ".Level", 1); var experience = f.Integer(x?.Experience ?? 0, p + ".Experience");
            var slot = f.I(x?.OriginalSlot ?? 0, p + ".OriginalSlot"); var revision = f.Integer(x?.StateRevision ?? 0, p + ".StateRevision", 1);
            var rewards = f.List(x?.BaseRewards, ExperienceReceipt, p + ".BaseRewards");
            var ends = f.List(x?.ProcessedEnds, CharacterEnd, p + ".ProcessedEnds");
            var periods = f.List(x?.RecoveryPeriods, (period, path) =>
            {
                f.Required(period, path); var endId = f.Text(period?.EndReceipt.EndReceiptId, path + ".EndReceipt");
                var end = Find(ends, e => e.EndReceiptId == endId, path + ".EndReceipt");
                if (!f.Reading) Need(ReferenceEquals(end, period.EndReceipt), path + ".EndReceipt", "InconsistentBinding");
                var d = growth.Inline(f, period?.Definition, Growth, path + ".Definition");
                var last = Time(period?.LastAcceptedSample, path + ".LastAcceptedSample");
                var elapsed = f.Rational(period?.Elapsed, path + ".Elapsed"); var anomaly = Anomaly(period?.Anomaly ?? 0, path + ".Anomaly");
                Need(elapsed.Numerator.Sign >= 0 && elapsed.Compare(d.RecoveryDurationMilliseconds, f.Budget.Math) <= 0, path + ".Elapsed");
                return f.Reading ? new CandidateRecoveryPeriod(end, d, last, elapsed, anomaly, f.Budget.Math) : period;
            }, p + ".RecoveryPeriods");
            return f.Reading ? new CandidateCharacterState(definition, player, character, level, experience, slot, revision, rewards, ends, periods) : x;
        }
        private CandidateGrowthDefinition Growth(CandidateGrowthDefinition x, string p)
        {
            f.Required(x, p);
            if (f.Resolved != null)
            {
                var definitions = f.Definition(x?.Context, "growth", p);
                var definition = f.SchemaVersion >= 3 ? definitions.FindGrowth(f.Text(x?.ClassId, p + ".SubjectClassId")) : definitions.Growth;
                Need(definition != null, p + ".SubjectClassId", "UnsupportedBinding");
                f.SameDefinition(x, definition, (fields, v) => new CandidatePermanentSaveCodec(fields).Growth(v, p), p);
                return f.Reading ? definition : x;
            }
            var input = new GrowthDefinitionInput { Context = f.Context(x?.Context, p + ".Context"), ClassId = f.Text(x?.ClassId, p + ".ClassId"),
                ClassKind = (CharacterClassKind)f.Enum((int)(x?.ClassKind ?? 0), 1, f.SchemaVersion == 4 ? 2 : 1, p + ".ClassKind"), PassiveDefinitionId = f.Text(x?.PassiveDefinitionId, p + ".PassiveDefinitionId"),
                BaseStats = f.Stats(x?.BaseStats, p + ".BaseStats"), GrowthHp = f.Rational(x?.GrowthHp, p + ".GrowthHp"),
                GrowthAttack = f.Rational(x?.GrowthAttack, p + ".GrowthAttack"), GrowthDefense = f.Rational(x?.GrowthDefense, p + ".GrowthDefense"),
                CritBase = f.Rational(x?.CritBase, p + ".CritBase", f.SchemaVersion == 4), CritStep = f.Rational(x?.CritStep, p + ".CritStep", f.SchemaVersion == 4),
                CritCap = f.Rational(x?.CritCap, p + ".CritCap", f.SchemaVersion == 4), CritMultiplier = f.Rational(x?.CritMultiplier, p + ".CritMultiplier", f.SchemaVersion == 4),
                XpBase = f.Integer(x?.XpBase ?? 0, p + ".XpBase"), XpLinear = f.Integer(x?.XpLinear ?? 0, p + ".XpLinear"),
                XpQuadratic = f.Integer(x?.XpQuadratic ?? 0, p + ".XpQuadratic"), RecoveryDurationMilliseconds = f.Rational(x?.RecoveryDurationMilliseconds, p + ".RecoveryDuration") };
            var result = f.SchemaVersion == 4 ? CandidateCharacterGrowth.PreparePermanentDefinition(input, f.Budget.Math) : CandidateCharacterGrowth.PrepareDefinition(input, f.Budget.Math);
            Need(result.IsAccepted, p + "." + result.FieldPath, result.RejectionCode.ToString()); return f.Reading ? result.Definition : x;
        }
        private CandidateBaseExperienceReceipt ExperienceReceipt(CandidateBaseExperienceReceipt x, string p)
        {
            f.Required(x, p); var input = new CandidateBaseExperience { PlayerId = f.Text(x?.PlayerId, p + ".PlayerId"), CharacterId = f.Text(x?.CharacterId, p + ".CharacterId"),
                AttemptId = f.Text(x?.AttemptId, p + ".AttemptId"), SettlementId = f.Text(x?.SettlementId, p + ".SettlementId"),
                Context = f.Context(x?.Context, p + ".Context"), Amount = f.Integer(x?.Amount ?? 0, p + ".Amount") };
            return f.Reading ? new CandidateBaseExperienceReceipt(input, RuleContextChecks.Freeze(input.Context)) : x;
        }
        private CandidateCharacterEndReceipt CharacterEnd(CandidateCharacterEndReceipt x, string p)
        {
            f.Required(x, p);
            var input = new CandidateCharacterEndFacts { PlayerId = f.Text(x?.PlayerId, p + ".PlayerId"), CharacterId = f.Text(x?.CharacterId, p + ".CharacterId"),
                AttemptId = f.Text(x?.AttemptId, p + ".AttemptId"), EntryBaselineId = f.Text(x?.EntryBaselineId, p + ".EntryBaselineId"),
                EndReceiptId = f.Text(x?.EndReceiptId, p + ".EndReceiptId"), Context = f.Context(x?.Context, p + ".Context"),
                Kind = (CandidateCharacterEndKind)f.Enum((int)(x?.Kind ?? 0), 1, 3, p + ".Kind"),
                WasParticipant = f.Flag(x?.WasParticipant ?? false, p + ".WasParticipant"), WasDown = f.Flag(x?.WasDown ?? false, p + ".WasDown"),
                RecoveryId = f.Text(x?.RecoveryId, p + ".RecoveryId", true), TimeSample = null };
            var time = f.Optional(x?.TimeSample, Time, p + ".TimeSample");
            return f.Reading ? new CandidateCharacterEndReceipt(input, RuleContextChecks.Freeze(input.Context), time) : x;
        }
        private CandidateTimeAnomaly Anomaly(CandidateTimeAnomaly x, string p)
        { var value = f.I((int)x, p); Need((value & ~3) == 0, p); return (CandidateTimeAnomaly)value; }
        private PreparedCandidateTimeSample Time(PreparedCandidateTimeSample x, string p)
        {
            f.Required(x, p);
            // UTC may precede the epoch. The integer codec itself remains strictly canonical.
            var input = new CandidateTimeSample { WallUtcMilliseconds = Signed(x?.WallUtcMilliseconds ?? 0, p + ".WallUtcMilliseconds"),
                ObservedAtUtcMilliseconds = Signed(x?.ObservedAtUtcMilliseconds ?? 0, p + ".ObservedAtUtcMilliseconds"),
                MonotonicElapsedMilliseconds = f.Rational(x?.MonotonicElapsedMilliseconds, p + ".MonotonicElapsedMilliseconds", true),
                MonotonicScopeId = f.Text(x?.MonotonicScopeId, p + ".MonotonicScopeId", true), Source = f.Text(x?.Source, p + ".Source"),
                Trust = (CandidateTimeTrust)f.Enum((int)(x?.Trust ?? 0), 1, 1, p + ".Trust"), Anomaly = Anomaly(x?.Anomaly ?? 0, p + ".Anomaly") };
            var c = new GrowthChecks(f.Budget.Math); Need(CandidateRecoveryClock.PrepareTime(input, c, out var restored), p + "." + c.Path, c.Code.ToString());
            return f.Reading ? restored : x;
        }
        private BigInteger Signed(BigInteger x, string p) { return f.SignedInteger(x, p); }
        internal CandidateInventoryState Inventory(CandidateInventoryState x, string p)
        {
            f.Required(x, p); var definition = InventoryDefinition(x?.Definition, p + ".Definition");
            var player = f.Text(x?.PlayerId, p + ".PlayerId");
            var actor = f.SchemaVersion >= 3 ? null : Actor(x?.Actor, p + ".Actor");
            var holdings = f.List(x?.Holdings, (row, path) => { f.Required(row, path); var item = f.Text(row?.ItemId, path + ".ItemId");
                var total = f.Integer(row?.T ?? 0, path + ".T"); return f.Reading ? new CandidateInventoryHolding(item, total) : row; }, p + ".Holdings");
            var loadouts = f.SchemaVersion >= 3 ? f.List(x?.Loadouts, Loadout, p + ".Loadouts")
                : new List<CandidateInventoryLoadout> { Loadout(x?.Loadout, p + ".Loadout") };
            var carry = f.Optional(x?.ActiveCarry, Carry, p + ".ActiveCarry");
            var grants = f.List(x?.OrdinaryGrants, Grant, p + ".OrdinaryGrants");
            var ends = f.List(x?.Ends, (end, path) =>
            {
                f.Required(end, path); var original = Carry(end?.OriginalCarry, path + ".OriginalCarry");
                var input = new CandidateInventoryEndIntent { EndReceiptId = f.Text(end?.EndReceiptId, path + ".EndReceiptId"),
                    Kind = (CandidateInventoryEndKind)f.Enum((int)(end?.Kind ?? 0), 1, 3, path + ".Kind"),
                    SettlementId = f.Text(end?.SettlementId, path + ".SettlementId", true), NewAttemptId = f.Text(end?.NewAttemptId, path + ".NewAttemptId", true) };
                var remaining = f.List(end?.Remaining, (row, rp) => { f.Required(row, rp); var character = f.Text(row?.CharacterId, rp + ".CharacterId");
                    var item = f.Text(row?.ItemId, rp + ".ItemId"); var u = f.Integer(row?.U ?? 0, rp + ".U"); return f.Reading ? new CandidateInventoryRemaining(character, item, u) : row; }, path + ".Remaining");
                var rewards = f.List(end?.Rewards, Quantity, path + ".Rewards");
                var grantId = f.Text(end?.RewardReceipt?.SettlementId, path + ".RewardReceipt", true);
                var grant = grantId == null ? null : Find(grants, g => g.SettlementId == grantId, path + ".RewardReceipt");
                if (!f.Reading) Need(ReferenceEquals(grant, end.RewardReceipt), path + ".RewardReceipt", "ReceiptConflict");
                return f.Reading ? new CandidateInventoryEndReceipt(original, input, remaining, rewards, grant) : end;
            }, p + ".Ends");
            var revision = f.Integer(x?.StateRevision ?? 0, p + ".StateRevision", 1); var preference = f.Integer(x?.PreferenceRevision ?? 0, p + ".PreferenceRevision", 1);
            var ledger = f.SchemaVersion == 4 ? new CandidatePermanentCodec(f).Ledger(x?.GetPermanentLedger(), p + ".Permanent") : null;
            if (!f.Reading) return x;
            return f.SchemaVersion >= 3
                ? new CandidateInventoryState(definition, player, holdings, loadouts, carry, grants, ends, revision, preference, true, ledger)
                : new CandidateInventoryState(definition, player, actor, holdings, loadouts[0], carry, grants, ends, revision, preference);
        }
        private CandidateInventoryLoadout Loadout(CandidateInventoryLoadout x, string p)
        {
            f.Required(x, p); CandidateInventoryActor actor;
            if (f.SchemaVersion >= 3)
            {
                var id = f.Text(x?.Actor.CharacterId, p + ".CharacterId"); var character = f.Roster?.Find(id);
                Need(character != null, p + ".CharacterId", "InconsistentBinding");
                actor = new CandidateInventoryActor(new CandidateInventoryActorInput { CharacterId = id, ClassId = character.ClassId,
                    ClassKind = character.Definition.ClassKind, OriginalSlot = character.OriginalSlot });
                if (!f.Reading) Need(x.Actor.ClassId == actor.ClassId && x.Actor.ClassKind == actor.ClassKind &&
                    x.Actor.OriginalSlot == actor.OriginalSlot, p + ".Actor", "InconsistentBinding");
            }
            else actor = Actor(x?.Actor, p + ".Actor");
            var item = f.Text(x?.ItemId, p + ".ItemId", true); var amount = f.Integer(x?.L ?? 0, p + ".L");
            bool? enabled = f.Flag(x?.Enabled != null, p + ".Enabled") ? (bool?)f.Flag(x?.Enabled ?? false, p + ".Enabled.Value") : null;
            return f.Reading ? new CandidateInventoryLoadout(actor, item, amount, enabled) : x;
        }
        private CandidateInventoryDefinition InventoryDefinition(CandidateInventoryDefinition x, string p)
        {
            if (f.Resolved != null)
            {
                f.Required(x, p); var definition = f.Definition(x?.Context, "inventory", p).Inventory;
                f.SameDefinition(x, definition, (fields, v) => new CandidatePermanentSaveCodec(fields).InventoryDefinition(v, p), p);
                return f.Reading ? definition : x;
            }
            f.Required(x, p); var context = f.Context(x?.Context, p + ".Context");
            var rows = f.List(x?.Items, (row, path) => { f.Required(row, path); return new CandidateInventoryItem(new CandidateInventoryItemInput {
                ItemId = f.Text(row?.ItemId, path + ".ItemId"), Kind = (CandidateInventoryItemKind)f.Enum((int)(row?.Kind ?? 0), 1, 2, path + ".Kind"),
                EquipClassId = f.Text(row?.EquipClassId, path + ".EquipClassId", true) }); }, p + ".Items");
            var items = new List<CandidateInventoryItemInput>(); foreach (var row in rows) items.Add(new CandidateInventoryItemInput { ItemId = row.ItemId, Kind = row.Kind, EquipClassId = row.EquipClassId });
            var result = CandidateInventory.PrepareDefinition(new CandidateInventoryDefinitionInput { Context = context, Items = items }, f.Budget.Math);
            Need(result.IsAccepted, p + "." + result.FieldPath, result.RejectionCode.ToString()); return f.Reading ? result.Definition : x;
        }
        private CandidateInventoryActor Actor(CandidateInventoryActor x, string p)
        {
            f.Required(x, p); var input = new CandidateInventoryActorInput { CharacterId = f.Text(x?.CharacterId, p + ".CharacterId"),
                ClassId = f.Text(x?.ClassId, p + ".ClassId"), ClassKind = (CharacterClassKind)f.Enum((int)(x?.ClassKind ?? 0), 1, 1, p + ".ClassKind"),
                OriginalSlot = f.I(x?.OriginalSlot ?? 0, p + ".OriginalSlot") };
            Need(input.OriginalSlot >= 0 && input.OriginalSlot <= 2, p + ".OriginalSlot"); return f.Reading ? new CandidateInventoryActor(input) : x;
        }
        private CandidateCarryPlan Carry(CandidateCarryPlan x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId"); var attempt = f.Text(x?.AttemptId, p + ".AttemptId");
            var baseline = f.Text(x?.EntryBaselineId, p + ".EntryBaselineId"); var context = f.Context(x?.Context, p + ".Context");
            var actors = f.List(x?.ReadyParticipants, Actor, p + ".ReadyParticipants");
            var rows = f.List(x?.Rows, (row, path) => { f.Required(row, path); var actor = Actor(row?.Actor, path + ".Actor");
                var item = f.Text(row?.ItemId, path + ".ItemId"); var count = f.Integer(row?.C ?? 0, path + ".C"); f.Required(row?.Source, path + ".Source");
                var source = new CandidateOrdinaryPool(f.Text(row?.Source.PlayerId, path + ".Source.PlayerId"), f.Text(row?.Source.ItemId, path + ".Source.ItemId"));
                return f.Reading ? new CandidateCarryRow(actor, item, count, source) : row; }, p + ".Rows");
            return f.Reading ? new CandidateCarryPlan(player, attempt, baseline, RuleContextChecks.Freeze(context), actors, rows) : x;
        }
        private CandidateInventoryQuantity Quantity(CandidateInventoryQuantity x, string p)
        { f.Required(x, p); var item = f.Text(x?.ItemId, p + ".ItemId"); var amount = f.Integer(x?.Quantity ?? 0, p + ".Quantity"); return f.Reading ? new CandidateInventoryQuantity(item, amount) : x; }
        private CandidateOrdinaryGrantReceipt Grant(CandidateOrdinaryGrantReceipt x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId"); var attempt = f.Text(x?.AttemptId, p + ".AttemptId");
            var settlement = f.Text(x?.SettlementId, p + ".SettlementId"); var context = f.Context(x?.Context, p + ".Context");
            var items = f.List(x?.Items, Quantity, p + ".Items");
            return f.Reading ? new CandidateOrdinaryGrantReceipt(player, attempt, settlement, RuleContextChecks.Freeze(context), items) : x;
        }
        internal CandidateProgressionState Progression(CandidateProgressionState x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId");
            var definition = ProgressionDefinition(x?.Definition, p + ".Definition");
            var revision = f.Integer(x?.StateRevision ?? 0, p + ".StateRevision", 1);
            var challenges = f.List(x?.Challenges, (challenge, cp) =>
            {
                f.Required(challenge, cp); var id = f.Text(challenge?.ChallengeId, cp + ".ChallengeId");
                var teaching = f.SchemaVersion == 4 ? f.Optional(challenge?.GetTeachingBinding(), new CandidatePermanentCodec(f).Level, cp + ".TeachingBinding") : null;
                if (!f.Reading && f.Resolved != null) Need(challenge.Attempts.Count > 0 || teaching != null, cp + ".Attempts", "InconsistentBinding");
                var levelContext = !f.Reading && f.Resolved != null ? challenge.Attempts.Count > 0
                    ? challenge.Attempts[0].Begin.Context : new PreparedPublishedRuleContext(teaching.Content) : null;
                var level = Level(challenge?.Level, cp + ".Level", levelContext);
                var attempts = f.List(challenge?.Attempts, (attempt, ap) =>
                {
                    f.Required(attempt, ap); var begin = Begin(attempt?.Begin, ap + ".Begin");
                    var end = f.Optional(attempt?.End, (e, ep) => End(e, begin, ep), ap + ".End");
                    if (end != null) { Need(!Endings.ContainsKey(end.EndReceiptId), ap + ".End.EndReceiptId", "ReceiptConflict"); Endings.Add(end.EndReceiptId, end); }
                    return f.Reading ? new CandidateProgressionAttempt(begin, end) : attempt;
                }, cp + ".Attempts");
                var closed = EndingRef(challenge?.ClosedBy, cp + ".ClosedBy", true);
                return f.Reading ? new CandidateProgressionChallenge(id, level, attempts, closed, teaching) : challenge;
            }, p + ".Challenges");
            var clears = f.List(x?.FirstClears, (clear, cp) =>
            { f.Required(clear, cp); var end = EndingRef(clear?.End, cp + ".End"); return f.Reading ? new CandidateProgressionFirstClear(end) : clear; }, p + ".FirstClears");
            var opens = f.List(x?.OpenFacts, (open, op) =>
            {
                f.Required(open, op); var owner = f.Text(open?.PlayerId, op + ".PlayerId"); var level = Level(open?.Level, op + ".Level", open?.Context);
                var originalContext = f.Context(open?.Context, op + ".Context");
                var endId = f.Text(open?.SourceClear?.End.EndReceiptId, op + ".SourceClear", true);
                var clear = endId == null ? null : Find(clears, c => c.End.EndReceiptId == endId, op + ".SourceClear");
                if (!f.Reading) Need(ReferenceEquals(clear, open.SourceClear), op + ".SourceClear", "ReceiptConflict");
                return f.Reading ? new CandidateProgressionOpenFact(owner, level, RuleContextChecks.Freeze(originalContext), clear) : open;
            }, p + ".OpenFacts");
            var effects = f.SchemaVersion == 4 ? new CandidatePermanentCodec(f).Effects(x?.GetPermanentEffects(), p + ".Permanent", 5) : null;
            return f.Reading ? new CandidateProgressionState(player, definition, revision, opens, clears, challenges, effects) : x;
        }
        private CandidateProgressionDefinition ProgressionDefinition(CandidateProgressionDefinition x, string p)
        {
            f.Required(x, p);
            if (f.Resolved != null)
            {
                var definition = f.Definition(x?.Context, "progression", p).Progression;
                f.SameDefinition(x, definition, (fields, v) => new CandidatePermanentSaveCodec(fields).ProgressionDefinition(v, p), p);
                return f.Reading ? definition : x;
            }
            var context = f.Context(x?.Context, p + ".Context");
            var levels = f.List(x?.Levels, (v, path) => Level(v, path), p + ".Levels");
            var input = new CandidateProgressionDefinitionInput { Context = context, Levels = new List<CandidateProgressionLevelInput>() };
            foreach (var level in levels) input.Levels.Add(LevelInput(level));
            var prepared = CandidateProgression.PrepareDefinition(input, f.Budget.Math);
            Need(prepared.IsAccepted, p + "." + prepared.FieldPath, prepared.RejectionCode.ToString()); return f.Reading ? prepared.Definition : x;
        }
        private CandidateProgressionLevel Level(CandidateProgressionLevel x, string p, PreparedRuleContext context = null)
        {
            if (f.Resolved != null)
            {
                f.Required(x, p); var c = (PublishedRuleContext)f.Context(context, p + ".Content");
                var binding = f.LevelBinding(c.Binding, x?.LevelId, x?.LevelVersion, p);
                var definition = f.Resolved.FindExact(c.Binding);
                var level = ProgressionChecks.FindLevel(definition.Progression.Levels, binding.LevelId);
                Need(level != null && level.LevelVersion == binding.CanonicalLevelVersion, p, "UnsupportedBinding");
                f.SameDefinition(x, level, (fields, v) => new CandidatePermanentSaveCodec(fields).Level(v, p), p);
                return f.Reading ? level : x;
            }
            f.Required(x, p); var input = new CandidateProgressionLevelInput { LevelId = f.Text(x?.LevelId, p + ".LevelId"),
                LevelVersion = f.Text(x?.LevelVersion, p + ".LevelVersion"), UnlockRuleId = f.Text(x?.UnlockRuleId, p + ".UnlockRuleId"),
                EntryKind = (CandidateProgressionEntryKind)f.Enum((int)(x?.EntryKind ?? 0), 1, 1, p + ".EntryKind"),
                UnlockKind = (CandidateProgressionUnlockKind)f.Enum((int)(x?.UnlockKind ?? 0), 1, 2, p + ".UnlockKind"),
                UnlockAfterLevelId = f.Text(x?.UnlockAfterLevelId, p + ".UnlockAfterLevelId", true), RequiredFeatures = f.Strings(x?.RequiredFeatures, p + ".RequiredFeatures") };
            Need(input.RequiredFeatures.Count == 0, p + ".RequiredFeatures", "UnsupportedBinding");
            Need((input.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen) == (input.UnlockAfterLevelId == null), p + ".UnlockAfterLevelId");
            return f.Reading ? new CandidateProgressionLevel(input) : x;
        }
        private static CandidateProgressionLevelInput LevelInput(CandidateProgressionLevel x)
        { return new CandidateProgressionLevelInput { LevelId = x.LevelId, LevelVersion = x.LevelVersion, UnlockRuleId = x.UnlockRuleId,
            EntryKind = x.EntryKind, UnlockKind = x.UnlockKind, UnlockAfterLevelId = x.UnlockAfterLevelId, RequiredFeatures = new List<string>(x.RequiredFeatures) }; }
        private CandidateProgressionBeginReceipt Begin(CandidateProgressionBeginReceipt x, string p)
        {
            f.Required(x, p); var player = f.Text(x?.PlayerId, p + ".PlayerId"); var level = Level(x?.Level, p + ".Level", x?.Context);
            var context = f.Context(x?.Context, p + ".Context"); var challenge = f.Text(x?.ChallengeId, p + ".ChallengeId");
            var attempt = f.Text(x?.AttemptId, p + ".AttemptId"); var baseline = f.Text(x?.EntryBaselineId, p + ".EntryBaselineId");
            var participants = f.SchemaVersion >= 3 ? f.List(x?.Participants, Participant, p + ".Participants")
                : new List<CandidateProgressionParticipant> { Participant(x?.Participant, p + ".Participant") };
            BigInteger? formationRevision = null;
            if (f.SchemaVersion >= 3 && f.Flag(x?.FormationRevision != null, p + ".FormationRevision"))
                formationRevision = f.Integer(x?.FormationRevision ?? 0, p + ".FormationRevision.Value", 1);
            return f.Reading ? new CandidateProgressionBeginReceipt(player, level, RuleContextChecks.Freeze(context), challenge, attempt, baseline, participants, formationRevision) : x;
        }
        private CandidateProgressionParticipant Participant(CandidateProgressionParticipant x, string p)
        {
            f.Required(x, p);
            var participant = new CandidateProgressionParticipant(f.Text(x?.CharacterId, p + ".CharacterId"),
                f.Text(x?.ClassId, p + ".ClassId"), (CharacterClassKind)f.Enum((int)(x?.ClassKind ?? 0), 1, 1, p + ".ClassKind"),
                f.Integer(x?.CharacterRevision ?? 0, p + ".CharacterRevision", 1), f.I(x?.OriginalSlot ?? 0, p + ".OriginalSlot"));
            Need(participant.OriginalSlot >= 0 && participant.OriginalSlot <= 2, p + ".OriginalSlot");
            return f.Reading ? participant : x;
        }
        private CandidateProgressionEndReceipt End(CandidateProgressionEndReceipt x, CandidateProgressionBeginReceipt begin, string p)
        {
            f.Required(x, p); var attempt = f.Text(x?.Begin.AttemptId, p + ".Begin");
            Need(attempt == begin.AttemptId && (f.Reading || ReferenceEquals(x.Begin, begin)), p + ".Begin", "ReceiptConflict");
            var facts = new CandidateProgressionEndFacts { EndReceiptId = f.Text(x?.EndReceiptId, p + ".EndReceiptId"),
                Kind = (CandidateProgressionEndKind)f.Enum((int)(x?.Kind ?? 0), 1, 3, p + ".Kind"),
                SettlementId = f.Text(x?.SettlementId, p + ".SettlementId", true), FinalReportFingerprint = f.Text(x?.FinalReportFingerprint, p + ".FinalReportFingerprint", true),
                NewAttemptId = f.Text(x?.NewAttemptId, p + ".NewAttemptId", true) };
            var first = f.Flag(x?.IsFirstClear ?? false, p + ".IsFirstClear");
            return f.Reading ? new CandidateProgressionEndReceipt(begin, facts, first) : x;
        }
        internal CandidateProgressionEndReceipt EndingRef(CandidateProgressionEndReceipt value, string p, bool optional = false)
        {
            if (!optional) f.Required(value, p); var id = f.Text(value?.EndReceiptId, p, optional); if (id == null) return null;
            Need(Endings.TryGetValue(id, out var end), p, "ReceiptConflict");
            if (!f.Reading) Need(ReferenceEquals(end, value), p, "ReceiptConflict"); return end;
        }
        internal static T Find<T>(IEnumerable<T> rows, Func<T, bool> match, string p) where T : class
        { T result = null; foreach (var row in rows) if (match(row)) { Need(result == null, p, "ReceiptConflict"); result = row; }
            Need(result != null, p, "InconsistentBinding"); return result; }
        internal static void PermanentQuantities(IReadOnlyList<CandidateInventoryQuantity> rows, string p)
        {
            for (var i = 1; i < rows.Count; i++)
                Need(StringComparer.Ordinal.Compare(rows[i - 1].ItemId, rows[i].ItemId) < 0, p + ".Order");
        }

        internal string PermanentEffectReference(string operation, int owner, string p)
        {
            Need(f.I(owner, p + ".Owner") == owner, p + ".Owner");
            var actual = f.Text(operation, p + ".Operation");
            Need(f.I(0, p + ".EffectRow") == 0, p + ".EffectRow");
            return actual;
        }

        internal void PermanentEndpoints(CandidatePermanentEffect effect, int owner, string p)
        {
            Need(PermanentEffectReference(effect.OperationId, owner, p + ".Reference") == effect.OperationId, p + ".Reference");
            var inputs = effect.Quote.Inputs;
            Need(f.I(inputs.Count, p + ".Endpoints.Count") == inputs.Count, p + ".Endpoints.Count");
            for (var i = 0; i < inputs.Count; i++)
            {
                var input = new CandidatePermanentCodec(f).Portion(inputs[i], p + ".Endpoints.Input");
                Need(CandidatePermanentCodec.SameRoot(input, inputs[i], f.Budget) && input.UnitStart == inputs[i].UnitStart &&
                    input.UnitCount == inputs[i].UnitCount && input.Endpoint == CandidatePermanentEndpoint.Held, p + ".Endpoints.Input");
                var expected = effect.Quote.Kind == CandidatePermanentKind.Craft
                    ? CandidatePermanentEndpoint.Transformed : CandidatePermanentEndpoint.Consumed;
                Need(f.Enum((int)expected, 1, 3, p + ".Endpoints.Tag") == (int)expected, p + ".Endpoints.Tag");
                Need(PermanentEffectReference(effect.OperationId, 4, p + ".Endpoints.Effect") == effect.OperationId, p + ".Endpoints.Effect");
                if (effect.Quote.Kind == CandidatePermanentKind.UseExperienceCards)
                {
                    var xp = f.Budget.Math.Multiply(input.UnitCount, effect.Quote.UnitExperience);
                    Need(f.Integer(input.UnitCount, p + ".Card.Units", 1) == input.UnitCount, p + ".Card.Units");
                    Need(f.Integer(effect.Quote.UnitExperience, p + ".Card.UnitXp", 1) == effect.Quote.UnitExperience, p + ".Card.UnitXp");
                    Need(f.Integer(xp, p + ".Card.FixedXp", 1) == xp, p + ".Card.FixedXp");
                    Need(f.Text(effect.Quote.CharacterId, p + ".Card.Character") == effect.Quote.CharacterId, p + ".Card.Character");
                    Need(f.Text(effect.Quote.ClassId, p + ".Card.Class") == effect.Quote.ClassId, p + ".Card.Class");
                }
            }
        }

    }
}
