using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    // Explicit schema primitives. Limits are checked before allocating strings, lists or bodies.
    internal sealed class BusinessFields
    {
        internal readonly SaveCodecBudget Budget;
        internal readonly bool Reading;
        internal bool StrictUnicode { get; set; }
        internal PublishedSaveContext Resolved { get; set; }
        internal bool ValidatePublished { get; set; }
        internal uint SchemaVersion { get; set; }
        internal CandidateRosterState Roster { get; set; }
        private readonly Stream stream;
        private readonly ulong length;
        private readonly byte[] scratch = new byte[8];
        internal ulong Used { get; private set; }
        internal BusinessFields(Stream stream, bool reading, SaveCodecBudget budget, ulong length = 0)
        { this.stream = stream; Reading = reading; Budget = budget; this.length = length; }
        internal static void Need(bool ok, string path, string code = "InvalidValue")
        { SaveCodecFailure.Require(ok, code, path); }
        internal void Required(object value, string path)
        { if (!Reading) Need(value != null, path, "MissingField"); }
        internal void Space(ulong count, string path)
        {
            if (Reading) Need(Used <= length && count <= length - Used, path, "Malformed");
            SaveCodecFailure.Limit(Used + count, Budget.MaxEnvelopeBytes, path, "EnvelopeBytes");
            SaveCodecFailure.Limit(Used + count, int.MaxValue, path, "ArrayLength");
        }
        internal ulong U(ulong value, int width, string path)
        {
            Space((ulong)width, path);
            if (Reading)
            {
                value = 0;
                for (var i = 0; i < width; i++)
                { var b = stream.ReadByte(); Need(b >= 0, path, "Malformed"); value |= (ulong)b << (8 * i); }
            }
            else
            { for (var i = 0; i < width; i++) scratch[i] = (byte)(value >> (8 * i)); stream.Write(scratch, 0, width); }
            Used += (ulong)width; return value;
        }
        internal int I(int value, string path) { return unchecked((int)(uint)U(unchecked((uint)value), 4, path)); }
        internal int Enum(int value, int min, int max, string path)
        { var n = I(value, path); Need(n >= min && n <= max, path, "UnsupportedBinding"); return n; }
        internal bool Flag(bool value, string path)
        { var b = U(value ? 1UL : 0UL, 1, path); Need(b <= 1, path, "Malformed"); return b == 1; }
        internal string Text(string value, string path, bool optional = false)
        {
            if (optional && !Flag(value != null, path)) return null;
            Required(value, path);
            var n = U((ulong)(value?.Length ?? 0), 4, path + ".Length");
            SaveCodecFailure.Limit(n, (ulong)Budget.MaxStringCodeUnits, path, "StringCodeUnits"); Space(n * 2, path);
            if (Reading)
            { var chars = new char[(int)n]; for (var i = 0; i < chars.Length; i++) chars[i] = (char)U(0, 2, path); value = new string(chars); }
            else for (var i = 0; i < value.Length; i++) U(value[i], 2, path);
            Need(!string.IsNullOrWhiteSpace(value), path);
            if (StrictUnicode) RuleContextChecks.Text(value, path, Budget, true);
            return value;
        }
        private string Token(string value, string path)
        {
            var n = U((ulong)(value?.Length ?? 0), 4, path + ".Length");
            SaveCodecFailure.Limit(n, (ulong)Budget.MaxNumericTokenBytes, path, "NumericTokenBytes"); Space(n, path);
            if (Reading)
            { var chars = new char[(int)n]; for (var i = 0; i < chars.Length; i++) chars[i] = (char)U(0, 1, path); return new string(chars); }
            for (var i = 0; i < value.Length; i++) U(value[i], 1, path); return value;
        }
        internal BigInteger Integer(BigInteger value, string path, int minimum = 0)
        { value = SignedInteger(value, path); Need(value >= minimum, path); return value; }
        internal BigInteger SignedInteger(BigInteger value, string path)
        {
            var token = Reading ? null : ExactSaveValueCodec.IntegerToken(value, Budget, n => Space((ulong)n + 4, path));
            token = Token(token, path);
            if (Reading) value = ExactSaveValueCodec.IntegerValue(token, Budget, path);
            return value;
        }
        internal ExactRational Rational(ExactRational value, string path, bool optional = false)
        {
            if (optional && !Flag(value != null, path)) return null;
            Required(value, path);
            var token = Reading ? null : Take(ExactSaveValueCodec.EncodeRational(value, Budget), path);
            token = Token(token, path);
            return Reading ? Take(ExactSaveValueCodec.DecodeRational(token, Budget), path) : value;
        }
        internal static T Take<T>(SaveCodecResult<T> result, string path)
        {
            if (!result.IsAccepted) throw new SaveCodecFailure(result.RejectionCode, path + "." + result.FieldPath,
                result.LimitReason, result.RequiredAtLeast, result.Allowed);
            return result.Value;
        }
        internal List<T> List<T>(IReadOnlyList<T> values, Func<T, string, T> visit, string path, ulong minimumBytes = 1)
        {
            Required(values, path);
            var count = U((ulong)(values?.Count ?? 0), 4, path + ".Count");
            SaveCodecFailure.Limit(count, (ulong)Budget.MaxCollectionEntries, path, "CollectionEntries"); Space(count * minimumBytes, path);
            var result = new List<T>((int)count);
            for (var i = 0; i < (int)count; i++) result.Add(visit(Reading ? default(T) : values[i], path + "[" + i + "]"));
            return result;
        }
        internal List<string> Strings(IReadOnlyList<string> values, string path)
        { return List(values, (s, p) => Text(s, p), path, 6); }
        internal T Optional<T>(T value, Func<T, string, T> visit, string path) where T : class
        { return Flag(value != null, path) ? visit(value, path) : null; }
        internal void End(string path) { if (Reading) Need(Used == length, path, "Malformed"); }
        internal void Header(int owner, string player, string path)
        {
            foreach (var b in new byte[] { 70, 77, 66, 73, 90, 48, 48, 49 }) Need(U(b, 1, path + ".Magic") == b, path + ".Magic", "Malformed");
            var schema = SchemaVersion == 0 ? (Resolved == null ? 1UL : 2UL) : SchemaVersion;
            Need(U(schema, 4, path + ".Schema") == schema, path + ".Schema", "UnsupportedSchema");
            Need(U((ulong)owner, 1, path + ".Owner") == (ulong)owner, path + ".Owner", "InconsistentBinding");
            Need(Text(player, path + ".PlayerId") == player, path + ".PlayerId", "InconsistentBinding");
        }
        internal RuleContext Context(PreparedRuleContext value, string path)
        {
            Required(value, path);
            if (Resolved != null || ValidatePublished)
            {
                Need(Reading || value is PreparedPublishedRuleContext, path, "UnsupportedBinding");
                var context = (PublishedRuleContext)PublishedContext((value as PreparedPublishedRuleContext)?.Binding, path);
                if (Resolved != null) Need(Resolved.FindExact(context.Binding) != null, path, "UnsupportedBinding");
                return context;
            }
            Need(Reading || value is PreparedCandidateContext, path, "UnsupportedBinding");
            var result = new CandidateContext { DraftId = Text(value?.DraftId, path + ".DraftId"),
                DraftRevision = Integer(value?.DraftRevision ?? 0, path + ".DraftRevision", 1),
                ContentFingerprint = Text(value?.ContentFingerprint, path + ".ContentFingerprint"),
                RuleVersion = Text(value?.RuleVersion, path + ".RuleVersion"),
                NumericContractVersion = Text(value?.NumericContractVersion, path + ".NumericContractVersion"),
                RandomContractVersion = Text(value?.RandomContractVersion, path + ".RandomContractVersion"),
                SourceNotes = Strings(value?.SourceNotes, path + ".SourceNotes") };
            Need(result.SourceNotes.Count > 0, path + ".SourceNotes"); return result;
        }
        internal RuleContext PublishedContext(ContentBinding value, string path)
        {
            Required(value, path);
            var package = Text(value?.PackageId, path + ".PackageId");
            var fingerprint = Text(value?.ContentFingerprint, path + ".ContentFingerprint");
            var rule = Text(value?.RuleVersion, path + ".RuleVersion");
            var numeric = Text(value?.NumericContractVersion, path + ".NumericContractVersion");
            var random = Text(value?.RandomContractVersion, path + ".RandomContractVersion");
            return new PublishedRuleContext(Take(ContentBinding.Prepare(package, fingerprint, rule, numeric, random, Budget), path));
        }

        internal StatsInput Stats(PreparedStats value, string path)
        {
            Required(value, path);
            return new StatsInput { MaxHp = Rational(value?.MaxHp, path + ".MaxHp"), Attack = Rational(value?.Attack, path + ".Attack"),
                PhysicalDefense = Rational(value?.PhysicalDefense, path + ".PhysicalDefense"), MagicDefense = Rational(value?.MagicDefense, path + ".MagicDefense"),
                Evasion = Rational(value?.Evasion, path + ".Evasion"), AttackRange = I(value?.AttackRange ?? 0, path + ".AttackRange") };
        }
        internal WarriorCritInput Crit(PreparedWarriorCrit value, string path)
        {
            Required(value, path);
            return new WarriorCritInput { PassiveDefinitionId = Text(value?.PassiveDefinitionId, path + ".PassiveDefinitionId"),
                TargetProbability = Rational(value?.TargetProbability, path + ".TargetProbability"), C = Rational(value?.C, path + ".C"),
                Multiplier = Rational(value?.Multiplier, path + ".Multiplier") };
        }
        internal FlowPos Position(FlowPos value, string path)
        { var x = I(value.x, path + ".x"); var y = I(value.y, path + ".y"); Need(x >= 0 && y >= 0, path); return new FlowPos(x, y); }
        internal List<FlowPos> Route(IReadOnlyList<FlowPos> value, string path) { return List(value, Position, path, 8); }
        internal BattleCombatantKey Combatant(BattleCombatantKey value, string path)
        {
            Required(value, path);
            var attempt = Text(value?.AttemptId, path + ".AttemptId");
            var kind = (BattleCombatantKind)Enum((int)(value?.Kind ?? 0), 0, 1, path + ".Kind");
            var character = Text(value?.CharacterId, path + ".CharacterId", true);
            var face = Text(value?.FaceId, path + ".FaceId", true); var enemy = Text(value?.EnemyInstanceKey, path + ".EnemyInstanceKey", true);
            Need(kind == BattleCombatantKind.Participant ? character != null && face == null && enemy == null : character == null && face != null && enemy != null, path);
            return Reading ? new BattleCombatantKey(attempt, kind, character, face, enemy) : value;
        }
        internal BattlePairKey PairKey(BattlePairKey value, string path)
        {
            Required(value, path); var attempt = Text(value?.AttemptId, path + ".AttemptId");
            var face = Text(value?.FaceId, path + ".FaceId"); var pair = Text(value?.PairId, path + ".PairId");
            return Reading ? new BattlePairKey(attempt, face, pair) : value;
        }
        internal Pcg32StreamState RandomStream(Pcg32StreamState value, string path)
        {
            Required(value, path);
            var s0 = U(value?.Initial.State ?? 0, 8, path + ".Initial.State"); var i0 = U(value?.Initial.Increment ?? 0, 8, path + ".Initial.Increment");
            var s1 = U(value?.Current.State ?? 0, 8, path + ".Current.State"); var i1 = U(value?.Current.Increment ?? 0, 8, path + ".Current.Increment");
            var words = Integer(value?.WordsConsumed ?? 0, path + ".WordsConsumed");
            Need((i0 & 1) == 1 && i0 == i1 && (!words.IsZero || s0 == s1), path, "InconsistentBinding");
            var restored = Pcg32StreamState.Restore(new Pcg32CoreState(s0, i0), new Pcg32CoreState(s1, i1), words, Budget.Math);
            return Reading ? restored : value;
        }
        internal CandidateRandomDomain Domain(CandidateRandomDomain value, string path)
        {
            Required(value, path); var purpose = (CandidateRandomPurpose)Enum((int)(value?.Purpose ?? 0), 0, 2, path + ".Purpose");
            var state = U(value?.InitState ?? 0, 8, path + ".InitState"); var sequence = U(value?.InitSequence ?? 0, 8, path + ".InitSequence");
            var initial = RandomStream(value?.Initial, path + ".Initial");
            Need(initial.WordsConsumed.IsZero, path + ".Initial", "InconsistentBinding");
            return Reading ? new CandidateRandomDomain(purpose, state, sequence, initial) : value;
        }
        internal PreparedBattleEntry Entry(PreparedBattleEntry value, string path)
        {
            Required(value, path);
            var input = new BattleEntryInput { PlayerId = Text(value?.PlayerId, path + ".PlayerId"), ChallengeId = Text(value?.ChallengeId, path + ".ChallengeId"),
                AttemptId = Text(value?.AttemptId, path + ".AttemptId"), EntryBaselineId = Text(value?.EntryBaselineId, path + ".EntryBaselineId"),
                Context = Context(value?.Context, path + ".Context") };
            var binding = value?.GetDefinitionBinding(); var resolvedLevel = value?.Level;
            if (Resolved != null)
            {
                binding = LevelBinding(((PublishedRuleContext)input.Context).Binding, value?.Level.LevelId, value?.Level.LevelVersion, path + ".Level");
                resolvedLevel = Resolved.FindExact(binding); Need(resolvedLevel != null, path + ".Level", "UnsupportedBinding");
                if (!Reading) Need(CandidateBattleReportFingerprint.Equal(value.Level, resolvedLevel, Budget.Math), path + ".Level", "InconsistentBinding");
                input.Level = new BusinessFields(Stream.Null, false, Budget).Level(resolvedLevel, path + ".Level");
            }
            else input.Level = Level(value?.Level, path + ".Level");
            var members = List(value?.Members, (x, p) => Reading ? new PreparedMember(Member(x, p)) : CheckedMember(x, p), path + ".Members");
            input.Members = new List<MemberInput>(); foreach (var member in members) input.Members.Add(MemberInputOf(member));
            input.CarryMode = (EntryCarryMode)Enum((int)(value?.CarryMode ?? 0), 1, 1, path + ".CarryMode");
            input.RequiredFeatures = Strings(value?.RequiredFeatures, path + ".RequiredFeatures");
            var preparer = new BattleEntryPreparer();
            var prepared = input.Context is PublishedRuleContext ? preparer.PreparePublished(input, binding, resolvedLevel, Budget.Math)
                : preparer.PrepareCandidate(input, Budget.Math);
            Need(prepared.IsAccepted, path + "." + prepared.FieldPath, prepared.RejectionCode?.ToString() ?? "InvalidValue");
            return Reading ? prepared.Entry : value;
        }
        private PreparedMember CheckedMember(PreparedMember value, string path) { Member(value, path); return value; }
        private MemberInput Member(PreparedMember value, string path)
        {
            Required(value, path);
            return new MemberInput { CharacterId = Text(value?.CharacterId, path + ".CharacterId"), ClassId = Text(value?.ClassId, path + ".ClassId"),
                ClassKind = (CharacterClassKind)Enum((int)(value?.ClassKind ?? 0), 1, 1, path + ".ClassKind"),
                OriginalSlot = I(value?.OriginalSlot ?? 0, path + ".OriginalSlot"), Level = Integer(value?.Level ?? 0, path + ".Level", 1),
                IsReady = Flag(value?.IsReady ?? false, path + ".IsReady"), StatsOrigin = (BaseStatsOrigin)Enum((int)(value?.StatsOrigin ?? 0), 1, 1, path + ".StatsOrigin"),
                StatsContext = Context(value?.StatsContext, path + ".StatsContext"), Stats = Stats(value?.Stats, path + ".Stats"),
                EntryHp = Rational(value?.EntryHp, path + ".EntryHp"), LearnedSkills = Strings(value?.LearnedSkills, path + ".LearnedSkills"), Crit = Crit(value?.Crit, path + ".Crit") };
        }
        private LevelInput Level(PreparedLevel value, string path)
        {
            Required(value, path); var id = Text(value?.LevelId, path + ".LevelId"); var version = Text(value?.LevelVersion, path + ".LevelVersion");
            var recommended = Integer(value?.RecommendedLevel ?? 0, path + ".RecommendedLevel", 1);
            var faces = List(value?.Faces, (x, p) => new PreparedFace(Face(x, p)), path + ".Faces");
            var inputs = new List<FaceInput>(); foreach (var face in faces) inputs.Add(FaceInputOf(face));
            return new LevelInput { LevelId = id, LevelVersion = version, RecommendedLevel = recommended, Faces = inputs };
        }
        private FaceInput Face(PreparedFace value, string path)
        {
            Required(value, path); var id = Text(value?.FaceId, path + ".FaceId"); var w = I(value?.Width ?? 0, path + ".Width"); var h = I(value?.Height ?? 0, path + ".Height");
            var pairs = List(value?.Pairs, (x, p) => new PreparedPair(Pair(x, p)), path + ".Pairs");
            var inputs = new List<PairInput>(); foreach (var pair in pairs) inputs.Add(PairInputOf(pair));
            return new FaceInput { FaceId = id, Width = w, Height = h, Pairs = inputs };
        }
        private PairInput Pair(PreparedPair value, string path)
        {
            Required(value, path);
            return new PairInput { PairId = Text(value?.PairId, path + ".PairId"), GeometryColorId = I(value?.GeometryColorId ?? 0, path + ".GeometryColorId"),
                EndpointA = Position(value?.EndpointA ?? default(FlowPos), path + ".EndpointA"), EndpointB = Position(value?.EndpointB ?? default(FlowPos), path + ".EndpointB"),
                Enemy = Enemy(value?.Enemy, path + ".Enemy") };
        }
        private EnemyInput Enemy(PreparedEnemy value, string path)
        {
            Required(value, path);
            var input = new EnemyInput { EnemyInstanceKey = Text(value?.EnemyInstanceKey, path + ".EnemyInstanceKey"),
                EnemyDefinitionId = Text(value?.EnemyDefinitionId, path + ".EnemyDefinitionId"), OriginalSlot = I(value?.OriginalSlot ?? 0, path + ".OriginalSlot"),
                StableOrder = I(value?.StableOrder ?? 0, path + ".StableOrder"), Behavior = (EnemyBehavior)Enum((int)(value?.Behavior ?? 0), 1, 2, path + ".Behavior"),
                Stats = Stats(value?.Stats, path + ".Stats") };
            var intents = List(value?.IntentCycle, (x, p) => new PreparedEnemyIntent(Intent(x, p)), path + ".IntentCycle");
            input.IntentCycle = new List<EnemyIntentInput>(); foreach (var intent in intents) input.IntentCycle.Add(IntentInputOf(intent)); return input;
        }
        private EnemyIntentInput Intent(PreparedEnemyIntent value, string path)
        {
            Required(value, path); var kind = (EnemyIntentKind)Enum((int)(value?.Kind ?? 0), 1, 2, path + ".Kind");
            var targeting = (EnemyTargeting)Enum((int)(value?.Targeting ?? 0), 1, 1, path + ".Targeting");
            EntryDamageKind? damage = Flag(value?.DamageKind != null, path + ".DamageKind")
                ? (EntryDamageKind?)Enum((int)(value?.DamageKind ?? 0), 1, 1, path + ".DamageKind.Value") : null;
            return new EnemyIntentInput { Kind = kind, Targeting = targeting, DamageKind = damage,
                DamageCoefficient = Rational(value?.DamageCoefficient, path + ".DamageCoefficient", true) };
        }
        internal DefinitionBinding LevelBinding(ContentBinding content, string levelId, string levelVersion, string path)
        {
            var id = Text(levelId, path + ".LevelId");
            var version = Reading ? BigInteger.Zero : Take(ExactSaveValueCodec.DecodeInteger(levelVersion, Budget), path + ".LevelVersion").Value;
            version = Integer(version, path + ".LevelVersion", 1);
            return Take(DefinitionBinding.Prepare(content, id, version, Budget), path);
        }
        internal PublishedRuleDefinitions Definition(PreparedRuleContext context, string role, string path)
        {
            var binding = ((PublishedRuleContext)Context(context, path + ".Context")).Binding;
            Need(Text(role, path + ".DefinitionId") == role, path + ".DefinitionId", "InconsistentBinding");
            Need(Integer(BigInteger.One, path + ".RecordVersion", 1).IsOne, path + ".RecordVersion", "UnsupportedSchema");
            var value = Resolved.FindExact(binding); Need(value != null, path, "UnsupportedBinding"); return value;
        }
        internal void SameDefinition<T>(T value, T expected, Action<BusinessFields, T> write, string path)
        {
            if (Reading) return;
            using (var a = new MemoryStream()) using (var b = new MemoryStream())
            {
                write(new BusinessFields(a, false, Budget) { ValidatePublished = true, StrictUnicode = true, SchemaVersion = SchemaVersion }, value);
                write(new BusinessFields(b, false, Budget) { ValidatePublished = true, StrictUnicode = true, SchemaVersion = SchemaVersion }, expected);
                Need(CandidateApplicationIntentCodec.SameBytes(a.ToArray(), b.ToArray()), path, "InconsistentBinding");
            }
        }
        internal static RuleContext ContextInput(PreparedRuleContext x)
        {
            return RuleContextChecks.Copy(x);
        }
        private static StatsInput StatsInputOf(PreparedStats x)
        { return new StatsInput { MaxHp = x.MaxHp, Attack = x.Attack, PhysicalDefense = x.PhysicalDefense, MagicDefense = x.MagicDefense, Evasion = x.Evasion, AttackRange = x.AttackRange }; }
        private static WarriorCritInput CritInputOf(PreparedWarriorCrit x)
        { return new WarriorCritInput { PassiveDefinitionId = x.PassiveDefinitionId, TargetProbability = x.TargetProbability, C = x.C, Multiplier = x.Multiplier }; }
        private static MemberInput MemberInputOf(PreparedMember x)
        { return new MemberInput { CharacterId = x.CharacterId, ClassId = x.ClassId, ClassKind = x.ClassKind, OriginalSlot = x.OriginalSlot, Level = x.Level,
            IsReady = x.IsReady, StatsOrigin = x.StatsOrigin, StatsContext = ContextInput(x.StatsContext), Stats = StatsInputOf(x.Stats),
            EntryHp = x.EntryHp, LearnedSkills = new List<string>(x.LearnedSkills), Crit = CritInputOf(x.Crit) }; }
        private static EnemyIntentInput IntentInputOf(PreparedEnemyIntent x)
        { return new EnemyIntentInput { Kind = x.Kind, Targeting = x.Targeting, DamageKind = x.DamageKind, DamageCoefficient = x.DamageCoefficient }; }
        private static PairInput PairInputOf(PreparedPair x)
        {
            var e = x.Enemy; var intents = new List<EnemyIntentInput>(); foreach (var intent in e.IntentCycle) intents.Add(IntentInputOf(intent));
            return new PairInput { PairId = x.PairId, GeometryColorId = x.GeometryColorId, EndpointA = x.EndpointA, EndpointB = x.EndpointB,
                Enemy = new EnemyInput { EnemyInstanceKey = e.EnemyInstanceKey, EnemyDefinitionId = e.EnemyDefinitionId, OriginalSlot = e.OriginalSlot,
                    StableOrder = e.StableOrder, Behavior = e.Behavior, Stats = StatsInputOf(e.Stats), IntentCycle = intents } };
        }
        private static FaceInput FaceInputOf(PreparedFace x)
        { var pairs = new List<PairInput>(); foreach (var pair in x.Pairs) pairs.Add(PairInputOf(pair)); return new FaceInput { FaceId = x.FaceId, Width = x.Width, Height = x.Height, Pairs = pairs }; }
    }

    // Separate instances are used for each schema type; references never carry runtime type names.
    internal sealed class BusinessTable<T> where T : class
    {
        internal readonly List<T> Rows = new List<T>();
        internal int Index(T value) { for (var i = 0; i < Rows.Count; i++) if (ReferenceEquals(Rows[i], value)) return i; return -1; }
        internal bool Add(T value, SaveCodecBudget budget, string path)
        {
            BusinessFields.Need(value != null, path, "MissingField"); if (Index(value) >= 0) return false;
            SaveCodecFailure.Limit((ulong)Rows.Count + 1, (ulong)budget.MaxCollectionEntries, path, "CollectionEntries"); Rows.Add(value); return true;
        }
        internal T Ref(BusinessFields f, T value, string path, bool optional = false)
        {
            var index = f.Reading ? 0 : Index(value);
            if (!f.Reading) BusinessFields.Need(index >= 0 || optional && value == null, path, "InconsistentBinding");
            var id = f.U(value == null && optional ? uint.MaxValue : unchecked((uint)index), 4, path);
            if (optional && id == uint.MaxValue) return null;
            BusinessFields.Need(id < (ulong)Rows.Count, path, "Malformed"); return Rows[(int)id];
        }
        internal T Inline(BusinessFields f, T value, Func<T, string, T> body, string path)
        {
            var index = f.Reading ? 0 : Index(value); if (!f.Reading && index < 0) index = Rows.Count;
            var id = f.U((uint)index, 4, path + ".Ref"); BusinessFields.Need(id <= (ulong)Rows.Count, path, "Malformed");
            if (id < (ulong)Rows.Count) return Rows[(int)id];
            var restored = body(value, path); Add(restored, f.Budget, path); return restored;
        }
    }
}
