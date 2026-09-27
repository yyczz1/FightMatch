using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FightMatch.Core;
using FlowPuzzle.Core;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    public static class PublishedContentCompiler
    {
        public static PreparedPublicationResult Prepare(PublishedSource input, DemoContentJob job,
            ContentConsumerCapabilities capabilities, ExactMathBudget budget)
        {
            return new PreparedPublicationResult(PublicationResult<PreparedPublication>.Run(() => {
                Root(input, "Source"); Root(job, "Job"); Root(capabilities, "Capabilities"); Root(budget, "Math"); job.Check();
                var encoded = PublishedContentCodec.Encode(input, capabilities.MaxSourceBytes, capabilities, budget);
                Need(input.OriginalBytes == null || input.OriginalBytes.Length <= capabilities.MaxSourceBytes, "BudgetExceeded", "OriginalSourceBytes");
                var original = input.OriginalBytes != null && input.OriginalCanonicalSha == PublishedContentCodec.Sha256(encoded)
                    ? (byte[])input.OriginalBytes.Clone() : encoded;
                var source = PublishedContentCodec.Decode<PublishedSource>(encoded, capabilities.MaxSourceBytes, capabilities, budget);
                Need(source.DraftId == job.DraftId && source.Revision == job.Revision, "StaleContext", "Source.Revision");
                Normalize(source, capabilities, budget);
                var payload = PublishedContentCodec.Encode(source, capabilities.MaxSourceBytes, capabilities, budget);
                var binding = Bind(source, payload, budget);
                var build = Build(source, binding, job, capabilities, budget, true);
                var evidence = new ValidationRecord { SchemaVersion = 1, Binding = ContentBindingRecord.From(binding), DraftId = source.DraftId,
                    Revision = source.Revision, SourceBytes = original.Length, SourceSha256 = PublishedContentCodec.Sha256(original),
                    PayloadBytes = payload.Length, PayloadSha256 = PublishedContentCodec.Sha256(payload),
                    DefinitionBindings = build.LevelBindings.Select(DefinitionBindingRecord.From).ToList(),
                    EvidenceSha256 = PublishedContentCodec.Sha256(EvidenceBytes(build.Replays, budget)) };
                var validation = PublishedContentCodec.Encode(evidence, 65536, capabilities, budget); job.Check();
                return new PreparedPublication(source, job, binding, build.Definitions, build.Profile, build.LevelBindings,
                    original, payload, validation, build.Replays);
            }));
        }
        internal static ContentBinding Bind(PublishedSource source, byte[] payload, ExactMathBudget math)
        {
            var b = ContentBinding.Prepare(source.PackageId, PublishedContentCodec.Sha256(payload), source.RuleVersion,
                source.NumericContractVersion, source.RandomContractVersion, new SaveCodecBudget(math));
            Need(b.IsAccepted, b.RejectionCode, b.FieldPath); return b.Value;
        }
        internal static void Normalize(PublishedSource s, ContentConsumerCapabilities caps, ExactMathBudget math)
        {
            caps.Check(s); Text(s.PackageId, "PackageId"); Text(s.DraftId, "DraftId"); Need(s.Revision.Sign > 0, "InvalidValue", "Revision");
            Root(s.Growth, "Growth"); Root(s.Inventory, "Inventory"); Root(s.Progression, "Progression"); Root(s.NewProfile, "NewProfile");
            Need(s.Growth.Context == null && s.Inventory.Context == null && s.Progression.Context == null, "UnsupportedBinding", "CompilerOwnedContext");
            Root(s.Levels, "Levels"); Need(s.Levels.Count > 0, "MissingField", "Levels");
            Root(s.Inventory.Items, "Inventory.Items"); Root(s.Progression.Levels, "Progression.Levels"); Root(s.Sources, "Sources");
            Root(s.SourceNotes, "SourceNotes"); Need(s.SourceNotes.Count > 0, "MissingField", "SourceNotes");
            Root(s.Parameters, "Parameters"); Need(s.Parameters.Count == 31, "MissingField", "Parameters");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var l in s.Levels)
            {
                Root(l, "Levels[]"); Root(l.Level, "Level"); Root(l.Reward, "Reward"); Root(l.SourceRoutes, "SourceRoutes");
                Need(l.Reward.Context == null, "UnsupportedBinding", "Reward.Context");
                Text(l.Level.LevelId, "Level.Id"); Need(seen.Add(l.Level.LevelId), "InconsistentBinding", "DuplicateLevel");
                var v = ExactSaveValueCodec.DecodeInteger(l.Level.LevelVersion, new SaveCodecBudget(math));
                Need(v.IsAccepted && v.Value.HasValue && v.Value.Value.Sign > 0, "InvalidValue", "Level.Version");
                Root(l.Reward.Materials, "Reward.Materials");
                Need(l.Reward.Materials.All(m => m != null && s.Inventory.Items.Any(i => i != null && i.ItemId == m.ItemId)), "InconsistentBinding", "Reward.Items");
                l.Reward.Materials.Sort((a, b) => StringComparer.Ordinal.Compare(a.ItemId, b.ItemId));
                l.SourceRoutes.Sort((a, b) => StringComparer.Ordinal.Compare(a?.FaceId + ":" + a?.PairId, b?.FaceId + ":" + b?.PairId));
            }
            Need(s.Levels.Count == s.Progression.Levels.Count, "InconsistentBinding", "Progression.Levels");
            s.Levels.Sort((a, b) => StringComparer.Ordinal.Compare(a.Level.LevelId, b.Level.LevelId));
            Need(s.Inventory.Items.All(i => i != null) && s.Progression.Levels.All(l => l != null) && s.Sources.All(p => p != null), "MissingField", "Definitions[]");
            s.Inventory.Items.Sort((a, b) => StringComparer.Ordinal.Compare(a.ItemId, b.ItemId));
            s.Progression.Levels.Sort((a, b) => StringComparer.Ordinal.Compare(a.LevelId, b.LevelId));
            s.Sources.Sort((a, b) => StringComparer.Ordinal.Compare(a.Path + ":" + a.Locator, b.Path + ":" + b.Locator));
            s.RequiredCapabilities.Sort(StringComparer.Ordinal);
            var p = s.NewProfile; Text(p.Id, "NewProfile.Id"); Text(p.CharacterId, "NewProfile.CharacterId");
            Need(p.RecordVersion.Sign > 0 && p.Level.Sign > 0 && p.Experience.Sign >= 0 && p.ClassId == s.Growth.ClassId,
                "InconsistentBinding", "NewProfile.Identity");
            Root(p.EmptySlots, "NewProfile.EmptySlots"); Root(p.Inventory, "NewProfile.Inventory"); Root(p.Carry, "NewProfile.Carry");
            Root(p.LearnedActiveSkills, "NewProfile.Skills"); Root(p.OpenLevels, "NewProfile.OpenLevels"); Root(p.Hp, "NewProfile.Hp");
            Need(p.OriginalSlot >= 0 && p.OriginalSlot < 3 && p.EmptySlots.Count == 2 && p.EmptySlots.Distinct().Count() == 2 &&
                p.EmptySlots.All(slot => slot >= 0 && slot < 3 && slot != p.OriginalSlot), "InvalidValue", "NewProfile.Slots");
            Need(p.Inventory.Count == 0 && p.Carry.Count == 0 && p.LearnedActiveSkills.Count == 0 && p.Recovery == null,
                "UnsupportedBinding", "NewProfile.EmptyState");
            Need(p.OpenLevels.Count > 0 && p.OpenLevels.Distinct(StringComparer.Ordinal).Count() == p.OpenLevels.Count &&
                p.OpenLevels.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(s.Progression.Levels.Where(l => l.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen)
                .Select(l => l.LevelId).OrderBy(x => x, StringComparer.Ordinal)), "InconsistentBinding", "NewProfile.OpenLevels");
            p.OpenLevels.Sort(StringComparer.Ordinal); p.EmptySlots.Sort();
        }
        internal sealed class BuildResult
        {
            internal PublishedRuleDefinitions Definitions; internal NewProfileDefinition Profile;
            internal readonly List<DefinitionBinding> LevelBindings = new List<DefinitionBinding>();
            internal readonly List<DemoContentReplayResult> Replays = new List<DemoContentReplayResult>();
        }
        internal static BuildResult Build(PublishedSource source, ContentBinding binding, DemoContentJob job,
            ContentConsumerCapabilities caps, ExactMathBudget math, bool replay)
        {
            var result = new BuildResult(); var context = new PublishedRuleContext(binding);
            var growthInput = Copy(source.Growth, caps, math); growthInput.Context = context;
            var growth = CandidateCharacterGrowth.PrepareDefinition(growthInput, math); Need(growth.IsAccepted, growth.RejectionCode.ToString(), growth.FieldPath);
            var inventoryInput = Copy(source.Inventory, caps, math); inventoryInput.Context = context;
            var inventory = CandidateInventory.PrepareDefinition(inventoryInput, math); Need(inventory.IsAccepted, inventory.RejectionCode.ToString(), inventory.FieldPath);
            var progressionInput = Copy(source.Progression, caps, math); progressionInput.Context = context;
            var progression = CandidateProgression.PrepareDefinition(progressionInput, math); Need(progression.IsAccepted, progression.RejectionCode.ToString(), progression.FieldPath);
            var p = source.NewProfile;
            // Validation identities and entropy are isolated making inputs, never stored in the new-profile recipe.
            var character = CandidateCharacterGrowth.CreateCandidate(growth.Definition, "validation:player", p.CharacterId, p.Level, p.Experience, p.OriginalSlot, math);
            Need(character.IsAccepted, character.RejectionCode.ToString(), character.FieldPath);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(character.Next, math);
            Need(p.Hp.Compare(stats.EntryHp, math) == 0, "InconsistentBinding", "NewProfile.Hp");
            var levels = new List<PreparedLevel>(); var rewards = new List<CandidateRewardDefinition>();
            foreach (var l in source.Levels)
            {
                job.Check(); var raw = CandidateInput(source, l);
                // Existing geometry and exact PRD proof kernel is used without replacing or searching coefficients.
                var proof = DemoContentCompiler.PrepareCandidate(raw, job, math);
                Need(proof.IsAccepted, proof.RejectionCode, proof.FieldPath); var c = proof.Candidate;
                var db = DefinitionBinding.Prepare(binding, l.Level.LevelId, BigInteger.Parse(l.Level.LevelVersion, CultureInfo.InvariantCulture), new SaveCodecBudget(math));
                Need(db.IsAccepted, db.RejectionCode, db.FieldPath); result.LevelBindings.Add(db.Value);
                var member = new MemberInput { CharacterId = p.CharacterId, ClassId = stats.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = p.OriginalSlot, Level = p.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin, StatsContext = context,
                    Stats = new StatsInput { MaxHp = stats.Stats.MaxHp, Attack = stats.Stats.Attack, PhysicalDefense = stats.Stats.PhysicalDefense,
                        MagicDefense = stats.Stats.MagicDefense, AttackRange = stats.Stats.AttackRange, Evasion = stats.Stats.Evasion },
                    EntryHp = p.Hp, LearnedSkills = new List<string>(p.LearnedActiveSkills), Crit = new WarriorCritInput {
                        PassiveDefinitionId = stats.PassiveDefinitionId, TargetProbability = stats.TargetProbability,
                        C = c.Parameters[(int)BigInteger.Min(p.Level, 31) - 1].C, Multiplier = stats.CritMultiplier } };
                var entry = new BattleEntryPreparer().PreparePublished(new BattleEntryInput { Context = context, PlayerId = raw.PlayerId,
                    ChallengeId = raw.ChallengeId, AttemptId = raw.AttemptId, EntryBaselineId = raw.EntryBaselineId, Level = l.Level,
                    Members = new List<MemberInput> { member }, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>() }, db.Value, c.Entry.Level, math);
                Need(entry.IsAccepted, entry.RejectionCode.ToString(), entry.FieldPath);
                var rewardInput = Copy(l.Reward, caps, math); rewardInput.Context = context;
                var reward = CandidateBaseRewards.PrepareDefinition(rewardInput, math); Need(reward.IsAccepted, reward.RejectionCode.ToString(), reward.FieldPath);
                levels.Add(entry.Entry.Level); rewards.Add(reward.Definition);
                if (!replay) continue;
                var formal = new DemoPreparedContent(job, source.Coordinates, entry.Entry, character.Next, stats, reward.Definition,
                    c.Sources.ToList(), c.Routes.ToList(), c.Parameters.ToList());
                using (var scope = new ExactEvaluationScope(new ExactEvaluationBudget(math)))
                {
                    var seed = new CandidateSeedMaterial { SourceCapabilityId = "isolated-validation:bytes-00-through-2f",
                        MappingId = CandidateRandomPreparer.SupportedMappingId, Bytes = Enumerable.Range(0, 48).Select(i => (byte)i).ToArray() };
                    var r = DemoContentReplay.ReplayCandidate(formal, seed, new CandidateBattleConditions { PreferenceRevision = 1, ItemUseEnabled = false },
                        snapshot => Select(formal, snapshot), 64, job, scope);
                    Need(r.IsAccepted, r.RejectionCode, r.FieldPath); result.Replays.Add(r);
                }
            }
            var definitions = PublishedRuleDefinitions.Prepare(binding, growth.Definition, inventory.Definition, progression.Definition, levels, rewards, new SaveCodecBudget(math));
            Need(definitions.IsAccepted, definitions.RejectionCode, definitions.FieldPath); result.Definitions = definitions.Value;
            result.Profile = new NewProfileDefinition(p, PublishedContentCodec.Encode(p, caps.MaxSourceBytes, caps, math)); return result;
        }
        private static T Copy<T>(T value, ContentConsumerCapabilities caps, ExactMathBudget math)
        { return PublishedContentCodec.Decode<T>(PublishedContentCodec.Encode(value, caps.MaxSourceBytes, caps, math), caps.MaxSourceBytes, caps, math); }
        private static DemoContentInput CandidateInput(PublishedSource s, PublishedLevelInput l)
        {
            var p = s.NewProfile;
            return new DemoContentInput { RuleVersion = s.RuleVersion, NumericContractVersion = s.NumericContractVersion,
                RandomContractVersion = s.RandomContractVersion, Sources = s.Sources, SourceNotes = s.SourceNotes, Coordinates = s.Coordinates,
                Growth = s.Growth, PlayerId = "validation:player", CharacterId = p.CharacterId, CharacterLevel = p.Level,
                CharacterExperience = p.Experience, OriginalSlot = p.OriginalSlot, ChallengeId = "validation:challenge:" + l.Level.LevelId,
                AttemptId = "validation:attempt:" + l.Level.LevelId, EntryBaselineId = "validation:baseline:" + l.Level.LevelId,
                Level = l.Level, SourceRoutes = l.SourceRoutes, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>(),
                LearnedSkills = p.LearnedActiveSkills, Parameters = s.Parameters, Reward = l.Reward };
        }
        private static CandidateReplayStep Select(DemoPreparedContent content, BattleSnapshot snapshot)
        {
            var enemy = snapshot.Enemies.First(e => e.Hp.Numerator.Sign > 0);
            var route = content.Routes.Single(r => r.FaceId == enemy.PairKey.FaceId && r.PairId == enemy.PairKey.PairId);
            return new CandidateReplayStep { Kind = CandidateBattleOperationKind.Attack, OperationId = "reference:" + snapshot.EffectiveActionsCompleted,
                OccurredAtUnixMilliseconds = 1000 + snapshot.EffectiveActionsCompleted, Actor = snapshot.Members[0].CombatantKey,
                Pair = enemy.PairKey, Route = new List<FlowPos>(route.Cells) };
        }
        internal static byte[] EvidenceBytes(IReadOnlyList<DemoContentReplayResult> replays, ExactMathBudget math)
        {
            return PublishedContentCodec.Encode(replays.Select(r => new { Parameters = r.Candidate.Parameters, Geometry = r.Candidate.Routes,
                r.Binding, r.SeedBytes, r.Run, r.RecordedReplays, r.Reward }).ToList(), 16 * 1024 * 1024,
                new ContentConsumerCapabilities(ContentConsumerCapabilities.Current.Capabilities, maxCollectionEntries: 65536), math);
        }
    }
    internal sealed class ValidationRecord
    {
        public int SchemaVersion { get; set; }
        public ContentBindingRecord Binding { get; set; }
        public string DraftId { get; set; }
        public BigInteger Revision { get; set; }
        public int SourceBytes { get; set; }
        public string SourceSha256 { get; set; }
        public int PayloadBytes { get; set; }
        public string PayloadSha256 { get; set; }
        public List<DefinitionBindingRecord> DefinitionBindings { get; set; }
        public string EvidenceSha256 { get; set; }
    }
}
