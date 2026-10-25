using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using FightMatch.Core;
using FlowPuzzle.Core;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    // CSV remains the authoring authority; formal publication consumes the standard Luban JSON output.
    public static class ChapterContentTables
    {
        public static PublicationResult<PublishedSource> Read(string generatedDirectory, string authoringDirectory,
            PublishedSource initialProfileAndProofs, ContentConsumerCapabilities limits, ExactMathBudget math)
        { return ReadInternal(generatedDirectory, authoringDirectory, initialProfileAndProofs, limits, math, false); }

        // Auxiliary author feedback only. A successful CSV check is not standard generation or publication evidence.
        public static PublicationResult<PublishedSource> ValidateAuthoringCsv(string directory, PublishedSource initialProfileAndProofs,
            ContentConsumerCapabilities limits, ExactMathBudget math)
        { return ReadInternal(directory, directory, initialProfileAndProofs, limits, math, true); }

        private static PublicationResult<PublishedSource> ReadInternal(string directory, string authoringDirectory,
            PublishedSource initialProfileAndProofs, ContentConsumerCapabilities limits, ExactMathBudget math, bool csv)
        {
            return PublicationResult<PublishedSource>.Run(() => {
                Root(initialProfileAndProofs, "InitialProfileAndProofs"); Root(limits, "Limits"); Root(math, "Math");
                Need(initialProfileAndProofs.SchemaVersion == 1 && initialProfileAndProofs.RuleVersion == "demo-r1",
                    "UnsupportedBinding", "InitialProfileAndProofs");
                var encoded = PublishedContentCodec.Encode(initialProfileAndProofs, limits.MaxSourceBytes, limits, math);
                var source = PublishedContentCodec.Decode<PublishedSource>(encoded, limits.MaxSourceBytes, limits, math);
                var seedSha = initialProfileAndProofs.OriginalBytes != null && initialProfileAndProofs.OriginalCanonicalSha == PublishedContentCodec.Sha256(encoded)
                    ? PublishedContentCodec.Sha256(initialProfileAndProofs.OriginalBytes) : PublishedContentCodec.Sha256(encoded);
                var tables = new Tables(directory, authoringDirectory, limits, math, csv);
                var release = tables.Load("Release", "package_id,draft_id,revision,source_schema,rule_version,numeric_version,random_version,required_capabilities,release_set_id,parameter_status,source_locator");
                Need(release.Count == 1, "InvalidValue", "Release.Rows"); var r = release[0];
                source.SchemaVersion = r.Int("source_schema", 1); Need(source.SchemaVersion == 2, "UnsupportedSchema", r.Path("source_schema"));
                source.PackageId = r.Id("package_id"); source.DraftId = r.Id("draft_id"); source.Revision = r.Integer("revision", true);
                source.RuleVersion = r.Id("rule_version"); source.NumericContractVersion = r.Id("numeric_version"); source.RandomContractVersion = r.Id("random_version");
                source.RequiredCapabilities = r.Set("required_capabilities"); source.Coordinates = DemoCoordinateCandidate.AssumedTopLeft;
                var stateful = source.RequiredCapabilities.Contains(ChapterRuleDefinitions.StateFeatureId);
                if (stateful) tables.UseStatefulSourcePaths();
                source.Chapter = new ChapterContentInput { ReleaseSetId = r.Id("release_set_id"), ParameterStatus = r.Enum<ChapterParameterStatus>("parameter_status"),
                    SourceLocator = r.Id("source_locator"), Passives = new List<ChapterPassiveInput>(), CharacterUnlocks = new List<ChapterCharacterUnlockInput>(),
                    FirstClearItems = new List<ChapterFirstClearItemInput>(), Cards = new List<ChapterCardInput>(), Skills = new List<ChapterSkillInput>(), Teachings = new List<ChapterTeachingInput>() };
                source.SourceNotes = new List<string> { "ChapterOne authoring tables; parameter status: " + source.Chapter.ParameterStatus };
                source.Growth = null; source.Growths = new List<GrowthDefinitionInput>(); source.Levels = new List<PublishedLevelInput>();
                source.Inventory = new CandidateInventoryDefinitionInput { Items = new List<CandidateInventoryItemInput>() };
                source.Progression = new CandidateProgressionDefinitionInput { Levels = new List<CandidateProgressionLevelInput>() };
                var passives = tables.Load("Passive", "passive_id,class_id,kind,p_base,p_step,p_cap,crit_multiplier,ignore_magic_defense_fraction");
                foreach (var row in passives)
                    source.Chapter.Passives.Add(new ChapterPassiveInput { Id = row.Id("passive_id"), ClassId = row.Id("class_id"), Kind = row.Enum<ChapterPassiveKind>("kind"),
                        ProbabilityBase = row.Q("p_base"), ProbabilityStep = row.Q("p_step"), ProbabilityCap = row.Q("p_cap"),
                        CritMultiplier = row.OptionalQ("crit_multiplier"), IgnoreMagicDefenseFraction = row.OptionalQ("ignore_magic_defense_fraction") });
                tables.Unique(passives, "passive_id"); tables.Unique(passives, "class_id");
                var growthRows = tables.Load("CharacterGrowth", "class_id,class_kind,passive_id,base_hp,base_attack,base_pdef,base_mdef,evasion,attack_range,growth_hp,growth_attack,growth_defense,xp_base,xp_linear,xp_quadratic,recovery_ms");
                tables.Unique(growthRows, "class_id");
                foreach (var row in growthRows)
                {
                    var id = row.Id("class_id"); var passiveId = row.Id("passive_id");
                    var p = source.Chapter.Passives.SingleOrDefault(x => x.Id == passiveId);
                    Need(p != null && p.ClassId == id, "InconsistentBinding", row.Path("passive_id"));
                    var warrior = p.Kind == ChapterPassiveKind.WarriorCrit;
                    source.Growths.Add(new GrowthDefinitionInput { ClassId = id, ClassKind = row.Enum<CharacterClassKind>("class_kind"), PassiveDefinitionId = passiveId,
                        BaseStats = new StatsInput { MaxHp = row.Q("base_hp"), Attack = row.Q("base_attack"), PhysicalDefense = row.Q("base_pdef"),
                            MagicDefense = row.Q("base_mdef"), Evasion = row.Q("evasion"), AttackRange = row.Int("attack_range", 1) },
                        GrowthHp = row.Q("growth_hp"), GrowthAttack = row.Q("growth_attack"), GrowthDefense = row.Q("growth_defense"),
                        CritBase = warrior ? p.ProbabilityBase : null, CritStep = warrior ? p.ProbabilityStep : null,
                        CritCap = warrior ? p.ProbabilityCap : null, CritMultiplier = p.CritMultiplier,
                        PenetrationBase = warrior ? null : p.ProbabilityBase, PenetrationStep = warrior ? null : p.ProbabilityStep,
                        PenetrationCap = warrior ? null : p.ProbabilityCap, IgnoreFraction = p.IgnoreMagicDefenseFraction,
                        XpBase = row.Integer("xp_base", true), XpLinear = row.Integer("xp_linear"), XpQuadratic = row.Integer("xp_quadratic"),
                        RecoveryDurationMilliseconds = row.Q("recovery_ms") });
                }
                var items = tables.Load("Item", "item_id,kind,equip_class_id"); tables.Unique(items, "item_id");
                foreach (var row in items) source.Inventory.Items.Add(new CandidateInventoryItemInput { ItemId = row.Id("item_id"),
                    Kind = row.Enum<CandidateInventoryItemKind>("kind"), EquipClassId = row.OptionalId("equip_class_id") });
                var cards = tables.Load("Card", "item_id,record_version,unit_experience"); tables.Unique(cards, "item_id");
                foreach (var row in cards) source.Chapter.Cards.Add(new ChapterCardInput { ItemId = row.Id("item_id"),
                    RecordVersion = row.Integer("record_version", true), UnitExperience = row.Integer("unit_experience", true) });
                ReadPermanent(tables, source);
                var levelRows = tables.Load("Level", "level_id,level_version,recommended_level,unlock_rule_id,unlock_kind,unlock_after_level_id,reward_id,tutorial_id");
                var faces = tables.Load("Face", "level_id,face_id,face_order,width,height");
                var pairs = tables.Load("PairEnemy", "level_id,face_id,pair_id,color_id,a_row,a_col,b_row,b_col,enemy_instance_key,enemy_definition_id,original_slot,stable_order,max_hp,attack,physical_defense,magic_defense,evasion,attack_range,action_set_id,initial_cursor,wind_target_instance_key,wind_uses,wind_fraction");
                var actions = tables.Load("EnemyAction", "action_set_id,action_index,kind,targeting,damage_kind,attack_override,coefficient,range,exhausted_fallback_index,next_action_vulnerability");
                var routes = tables.Load("DefaultRouteCell", "level_id,face_id,pair_id,cell_order,row,column");
                var rewards = tables.Load("Reward", "reward_id,record_version,level_id,level_version,base_experience,damage_weight,taken_weight,curve_base,curve_log,reference_hp_divisor,level_penalty_base,overlevel_grace,zero_contribution_policy,drop_mode");
                var materials = tables.Load("RewardMaterial", "reward_id,item_id,amount");
                tables.Unique(levelRows, "level_id"); tables.Unique(rewards, "reward_id"); tables.Unique(rewards, "level_id");
                tables.Unique(faces, "level_id", "face_id"); tables.Unique(pairs, "level_id", "face_id", "pair_id");
                tables.Unique(pairs, "level_id", "enemy_instance_key"); tables.Unique(actions, "action_set_id", "action_index");
                tables.Unique(materials, "reward_id", "item_id"); tables.Unique(routes, "level_id", "face_id", "pair_id", "cell_order");
                foreach (var teaching in source.Chapter.Teachings)
                    Need(levelRows.Count(row => row.Id("level_id") == teaching.LevelId &&
                        row.Integer("level_version", true) == teaching.LevelVersion && row.OptionalId("tutorial_id") == teaching.TutorialId) == 1,
                        "InconsistentBinding", "Teaching.Level");
                var used = new HashSet<Row>();
                foreach (var row in levelRows)
                {
                    var id = row.Id("level_id"); var version = row.Integer("level_version", true).ToString(CultureInfo.InvariantCulture);
                    var level = new LevelInput { LevelId = id, LevelVersion = version, RecommendedLevel = row.Integer("recommended_level", true), Faces = new List<FaceInput>() };
                    var record = new PublishedLevelInput { Level = level, SourceRoutes = new List<DemoContentRouteInput>() };
                    var tutorialId = row.OptionalId("tutorial_id");
                    Need(tutorialId == null || source.Chapter.Teachings.Any(t => t.TutorialId == tutorialId && t.LevelId == id && t.LevelVersion.ToString(CultureInfo.InvariantCulture) == version),
                        "InconsistentBinding", row.Path("tutorial_id"));
                    var fs = Select(faces, used, f => f.Id("level_id") == id); Continuous(fs, "face_order", row.Path("level_id"));
                    foreach (var f in fs.OrderBy(x => x.Int("face_order")))
                    {
                        var face = new FaceInput { FaceId = f.Id("face_id"), Width = f.Int("width", 1), Height = f.Int("height", 1), Pairs = new List<PairInput>() }; level.Faces.Add(face);
                        var es = Select(pairs, used, e => e.Id("level_id") == id && e.Id("face_id") == face.FaceId);
                        Continuous(es, "original_slot", f.Path("face_id")); Continuous(es, "stable_order", f.Path("face_id"));
                        foreach (var e in es.OrderBy(x => x.Int("original_slot")))
                        {
                            var actionSet = e.Id("action_set_id"); var cycle = Select(actions, used, a => a.Id("action_set_id") == actionSet);
                            Continuous(cycle, "action_index", e.Path("action_set_id"));
                            var initialCursor = e.Integer("initial_cursor");
                            if (!stateful)
                            {
                                Need(initialCursor.IsZero, "UnsupportedCapability", e.Path("initial_cursor"));
                                Need(e.OptionalId("wind_target_instance_key") == null && e.OptionalInteger("wind_uses") == null && e.OptionalQ("wind_fraction") == null,
                                    "UnsupportedCapability", e.Path("wind_target_instance_key"));
                            }
                            Need(initialCursor < cycle.Count, "InvalidValue", e.Path("initial_cursor"));
                            var stats = new StatsInput { MaxHp = e.Q("max_hp"), Attack = e.Q("attack"), PhysicalDefense = e.Q("physical_defense"),
                                MagicDefense = e.Q("magic_defense"), Evasion = e.Q("evasion"), AttackRange = e.Int("attack_range", 1) };
                            var intents = cycle.OrderBy(x => x.Int("action_index")).Select(a => Action(a, stats, stateful)).ToList();
                            Need(stateful || intents.Count == 1 && intents[0].Kind == EnemyIntentKind.Strike || intents.Count == 2 && intents[0].Kind == EnemyIntentKind.Charge && intents[1].Kind == EnemyIntentKind.Strike,
                                "UnsupportedCapability", e.Path("action_set_id"));
                            var pair = new PairInput { PairId = e.Id("pair_id"), GeometryColorId = e.Int("color_id"),
                                EndpointA = Endpoint(e.Int("a_row", 1), e.Int("a_col", 1), face.Width, face.Height, e.Path("a_row")),
                                EndpointB = Endpoint(e.Int("b_row", 1), e.Int("b_col", 1), face.Width, face.Height, e.Path("b_row")),
                                Enemy = new EnemyInput { EnemyInstanceKey = e.Id("enemy_instance_key"), EnemyDefinitionId = e.Id("enemy_definition_id"),
                                    OriginalSlot = e.Int("original_slot"), StableOrder = e.Int("stable_order"), Stats = stats, IntentCycle = intents,
                                    Behavior = stateful ? EnemyBehavior.ChapterCycle : intents.Count == 1 ? EnemyBehavior.NormalStrike : EnemyBehavior.ChargeHeavy,
                                    InitialIntentCursor = stateful ? initialCursor : (BigInteger?)null,
                                    WindTargetEnemyInstanceKey = stateful ? e.OptionalId("wind_target_instance_key") : null,
                                    WindUses = stateful ? e.OptionalInt("wind_uses") : null,
                                    WindFraction = stateful ? e.OptionalQ("wind_fraction") : null } };
                            face.Pairs.Add(pair);
                            var cells = Select(routes, used, c => c.Id("level_id") == id && c.Id("face_id") == face.FaceId && c.Id("pair_id") == pair.PairId);
                            Continuous(cells, "cell_order", e.Path("pair_id"));
                            record.SourceRoutes.Add(new DemoContentRouteInput { FaceId = face.FaceId, PairId = pair.PairId,
                                Cells = cells.OrderBy(c => c.Int("cell_order")).Select(c => {
                                    var x = c.Int("column", 1); var y = c.Int("row", 1);
                                    Need(x <= face.Width && y <= face.Height, "InvalidValue", c.Path("row")); return new FlowPos(x, y);
                                }).ToList() });
                        }
                    }
                    var rewardId = row.Id("reward_id"); var rr = Select(rewards, used, x => x.Id("reward_id") == rewardId);
                    Need(rr.Count == 1, "InconsistentBinding", row.Path("reward_id")); var reward = rr[0];
                    Need(reward.Id("level_id") == id && reward.Integer("level_version", true).ToString(CultureInfo.InvariantCulture) == version,
                        "InconsistentBinding", reward.Path("level_id"));
                    record.Reward = new CandidateRewardDefinitionInput { RewardDefinitionId = rewardId, Version = reward.Integer("record_version", true).ToString(CultureInfo.InvariantCulture),
                        LevelId = id, LevelVersion = version, BaseExperience = reward.Integer("base_experience"), DamageWeight = reward.Q("damage_weight"),
                        TakenWeight = reward.Q("taken_weight"), CurveBase = reward.Q("curve_base"), CurveLog = reward.Q("curve_log"),
                        ReferenceHpDivisor = reward.Q("reference_hp_divisor"), LevelPenaltyBase = reward.Q("level_penalty_base"), OverlevelGrace = reward.Integer("overlevel_grace"),
                        ZeroContributionPolicy = reward.Enum<CandidateZeroContributionPolicy>("zero_contribution_policy"), DropMode = reward.Enum<CandidateRewardDropMode>("drop_mode"),
                        RequiredFeatures = new List<string>(), Materials = Select(materials, used, m => m.Id("reward_id") == rewardId).Select(m =>
                            new CandidateRewardMaterialInput { ItemId = m.Id("item_id"), Amount = m.Integer("amount") }).ToList() };
                    source.Levels.Add(record);
                    source.Progression.Levels.Add(new CandidateProgressionLevelInput { LevelId = id, LevelVersion = version, UnlockRuleId = row.Id("unlock_rule_id"),
                        UnlockKind = row.Enum<CandidateProgressionUnlockKind>("unlock_kind"), UnlockAfterLevelId = row.OptionalId("unlock_after_level_id"),
                        EntryKind = CandidateProgressionEntryKind.Ordinary, RequiredFeatures = new List<string>() });
                }
                foreach (var rows in new[] { faces, pairs, actions, routes, rewards, materials })
                    foreach (var row in rows) Need(used.Contains(row), "InconsistentBinding", row.Path("UnboundRow"));
                var metadata = tables.Load("SourceMetadata", "path,locator,sha256"); tables.Unique(metadata, "path", "locator");
                Need(metadata.Any(row => row.Id("path") == "Assets/FightMatchContent/demo-r1.source.json" && row.Id("sha256") == seedSha),
                    "InconsistentBinding", "SourceMetadata.InitialProfileAndProofs");
                source.Sources = new List<DemoContentSourceInput>(tables.Sources);
                foreach (var row in metadata)
                {
                    var sha = row.Id("sha256"); Need(PublishedContentCatalog.IsSha(sha), "InvalidValue", row.Path("sha256"));
                    source.Sources.Add(new DemoContentSourceInput { Path = row.Id("path"), Locator = row.Id("locator"), Sha256 = sha });
                }
                source.NewProfile.OpenLevels = source.Progression.Levels.Where(l => l.UnlockKind == CandidateProgressionUnlockKind.InitiallyOpen).Select(l => l.LevelId).ToList();
                Need(source.NewProfile.OpenLevels.Count == 1 && source.NewProfile.OpenLevels[0] == "level:ch01-01", "UnsupportedBinding", "NewProfile.OpenLevels");
                source.OriginalBytes = null; source.OriginalCanonicalSha = null;
                PublishedContentCompiler.Normalize(source, limits, math); return source;
            });
        }
        private static void ReadPermanent(Tables tables, PublishedSource source)
        {
            var unlocks = tables.Load("CharacterUnlock", "class_id,after_level_id,initial_level_source,initial_xp,initial_active_skill_ids"); tables.Unique(unlocks, "class_id");
            foreach (var row in unlocks) source.Chapter.CharacterUnlocks.Add(new ChapterCharacterUnlockInput { ClassId = row.Id("class_id"), AfterLevelId = row.Id("after_level_id"),
                InitialLevelSource = row.Id("initial_level_source"), InitialExperience = row.Integer("initial_xp"), InitialActiveSkills = row.Set("initial_active_skill_ids") });
            var first = tables.Load("FirstClearItem", "level_id,item_id,amount"); tables.Unique(first, "level_id", "item_id");
            foreach (var row in first) source.Chapter.FirstClearItems.Add(new ChapterFirstClearItemInput { LevelId = row.Id("level_id"), ItemId = row.Id("item_id"), Amount = row.Integer("amount", true) });
            var skills = tables.Load("Skill", "skill_id,record_version,class_id,cost_item_id,battle_capability_id,minimum_level,cost_count,cooldown_turns"); tables.Unique(skills, "skill_id");
            foreach (var row in skills) source.Chapter.Skills.Add(new ChapterSkillInput { SkillId = row.Id("skill_id"), RecordVersion = row.Integer("record_version", true),
                ClassId = row.Id("class_id"), CostItemId = row.Id("cost_item_id"), BattleCapabilityId = row.Id("battle_capability_id"),
                MinimumLevel = row.Integer("minimum_level", true), CostCount = row.Integer("cost_count", true), CooldownTurns = row.Integer("cooldown_turns") });
            var teaching = tables.Load("Teaching", "tutorial_id,record_version,level_id,level_version,class_id,skill_id,gift_item_id,learning_step_id,explanation_step_id,gift_once_key,gift_count,explanation_text_key"); tables.Unique(teaching, "tutorial_id");
            foreach (var row in teaching) source.Chapter.Teachings.Add(new ChapterTeachingInput { TutorialId = row.Id("tutorial_id"), RecordVersion = row.Integer("record_version", true),
                LevelId = row.Id("level_id"), LevelVersion = row.Integer("level_version", true), ClassId = row.Id("class_id"), SkillId = row.Id("skill_id"), GiftItemId = row.Id("gift_item_id"),
                LearningStepId = row.Id("learning_step_id"), ExplanationStepId = row.Id("explanation_step_id"), GiftOnceKey = row.Id("gift_once_key"),
                GiftCount = row.Integer("gift_count", true), ExplanationTextKey = row.Id("explanation_text_key") });
        }
        private static EnemyIntentInput Action(Row row, StatsInput enemy, bool stateful)
        {
            var kind = row.Enum<EnemyIntentKind>("kind");
            if (stateful) return StatefulAction(row, kind);
            Need(kind == EnemyIntentKind.Strike || kind == EnemyIntentKind.Charge, "UnsupportedCapability", row.Path("kind"));
            Need(row.OptionalQ("attack_override") == null && row.OptionalInteger("exhausted_fallback_index") == null && row.OptionalQ("next_action_vulnerability") == null,
                "UnsupportedCapability", row.Path("attack_override"));
            if (kind == EnemyIntentKind.Charge)
            {
                Need(row.OptionalId("targeting") == null && row.OptionalId("damage_kind") == null && row.OptionalQ("coefficient") == null && row.OptionalInteger("range") == null,
                    "InvalidValue", row.Path("kind"));
                return new EnemyIntentInput { Kind = kind, Targeting = EnemyTargeting.FirstLiving, DamageKind = null, DamageCoefficient = null };
            }
            var targeting = row.Id("targeting");
            Need(targeting == "FirstLegalLiving" || targeting == "LastLegalLiving", "UnsupportedCapability", row.Path("targeting"));
            Need(row.Int("range", 1) == enemy.AttackRange, "UnsupportedCapability", row.Path("range"));
            return new EnemyIntentInput { Kind = kind, Targeting = targeting == "FirstLegalLiving" ? EnemyTargeting.FirstLiving : EnemyTargeting.LastLegalLiving,
                DamageKind = row.Enum<EntryDamageKind>("damage_kind"), DamageCoefficient = row.Q("coefficient") };
        }
        private static EnemyIntentInput StatefulAction(Row row, EnemyIntentKind kind)
        {
            Need(kind == EnemyIntentKind.Strike || kind == EnemyIntentKind.Charge || kind == EnemyIntentKind.Rest ||
                kind == EnemyIntentKind.Wind, "UnsupportedCapability", row.Path("kind"));
            if (kind != EnemyIntentKind.Strike)
            {
                Need(row.OptionalId("targeting") == null && row.OptionalId("damage_kind") == null &&
                    row.OptionalQ("attack_override") == null && row.OptionalQ("coefficient") == null &&
                    row.OptionalInteger("range") == null && row.OptionalQ("next_action_vulnerability") == null,
                    "InvalidValue", row.Path("kind"));
                var fallback = row.OptionalInt("exhausted_fallback_index");
                Need(kind == EnemyIntentKind.Wind ? fallback.HasValue : !fallback.HasValue, "InvalidValue", row.Path("exhausted_fallback_index"));
                return new EnemyIntentInput { Kind = kind, Targeting = EnemyTargeting.Unspecified, ExhaustedFallbackIndex = fallback };
            }
            var targeting = row.Id("targeting");
            Need(targeting == "FirstLegalLiving" || targeting == "LastLegalLiving" || targeting == "PreferLivingMage",
                "UnsupportedCapability", row.Path("targeting"));
            Need(row.OptionalInteger("exhausted_fallback_index") == null, "InvalidValue", row.Path("exhausted_fallback_index"));
            var damageKind = row.Enum<EntryDamageKind>("damage_kind");
            Need(damageKind == EntryDamageKind.Physical, "UnsupportedCapability", row.Path("damage_kind"));
            return new EnemyIntentInput {
                Kind = kind,
                Targeting = targeting == "FirstLegalLiving" ? EnemyTargeting.FirstLiving :
                    targeting == "LastLegalLiving" ? EnemyTargeting.LastLegalLiving : EnemyTargeting.PreferMage,
                DamageKind = damageKind,
                DamageCoefficient = row.Q("coefficient"),
                AttackOverride = row.OptionalQ("attack_override"),
                AttackRange = row.Int("range", 1),
                NextActionVulnerability = row.OptionalQ("next_action_vulnerability")
            };
        }
        private static List<Row> Select(List<Row> rows, HashSet<Row> used, Func<Row, bool> predicate)
        { var selected = rows.Where(predicate).ToList(); foreach (var row in selected) used.Add(row); return selected; }
        private static void Continuous(List<Row> rows, string field, string path)
        { Need(rows.Count > 0 && rows.Select(r => r.Int(field)).OrderBy(n => n).SequenceEqual(Enumerable.Range(0, rows.Count)), "InvalidValue", path + "." + field); }
        public static FlowPos Endpoint(int row, int column, int width, int height)
        { return Endpoint(row, column, width, height, "PairEnemy.Endpoint"); }
        private static FlowPos Endpoint(int row, int column, int width, int height, string path)
        { Need(row >= 1 && row <= height && column >= 1 && column <= width, "InvalidValue", path); return new FlowPos(column - 1, height - row); }
        public static PublicationResult<ExactRational> ParseRational(string value, ExactMathBudget math)
        { return PublicationResult<ExactRational>.Run(() => Rational(value, math, "Q")); }
        private static ExactRational Rational(string value, ExactMathBudget math, string path)
        {
            Need(value != null && value.Length > 0 && value.Length <= 4096 && value.Length <= math.MaxIntegerBits + 1, "InvalidValue", path);
            var slash = value.IndexOf('/'); var dot = value.IndexOf('.');
            if (slash >= 0)
            {
                Need(dot < 0 && slash > 0 && slash == value.LastIndexOf('/') && slash < value.Length - 1, "InvalidValue", path);
                var n = SignedInteger(value.Substring(0, slash), math, path); var d = SignedInteger(value.Substring(slash + 1), math, path);
                Need(d.Sign > 0, "InvalidValue", path); return ExactRational.Create(n, d, math);
            }
            if (dot >= 0)
            {
                Need(dot > 0 && dot < value.Length - 1 && dot == value.LastIndexOf('.'), "InvalidValue", path);
                var digits = value.Substring(dot + 1); Need(digits.All(c => c >= '0' && c <= '9'), "InvalidValue", path);
                var integer = value.Substring(0, dot); SignedInteger(integer, math, path);
                var numerator = SignedInteger(integer + digits, math, path); var denominator = ExactRational.Create(1, 1, math); var ten = ExactRational.Create(10, 1, math);
                for (var i = 0; i < digits.Length; i++) denominator = denominator.Multiply(ten, math);
                return ExactRational.Create(numerator, denominator.Numerator, math);
            }
            return ExactRational.Create(SignedInteger(value, math, path), 1, math);
        }
        private static BigInteger SignedInteger(string value, ExactMathBudget math, string path)
        {
            Need(value != null && value.Length > 0 && value.Length <= 4096 && value.Length <= math.MaxIntegerBits + 1, "InvalidValue", path);
            var start = value[0] == '-' ? 1 : 0;
            Need(start < value.Length && value.Skip(start).All(c => c >= '0' && c <= '9'), "InvalidValue", path);
            var result = BigInteger.Parse(value, CultureInfo.InvariantCulture); ExactRational.Create(result, 1, math); return result;
        }
        private sealed class Row
        {
            private readonly string table; private readonly int index; private readonly Dictionary<string, string> fields; private readonly ExactMathBudget math;
            internal Row(string table, int index, string[] columns, List<string> cells, ExactMathBudget math)
            { this.table = table; this.index = index; this.math = math; fields = columns.Select((key, i) => new { key, value = cells[i] }).ToDictionary(p => p.key, p => p.value, StringComparer.Ordinal); }
            internal string Path(string field) => table + "[" + index + "]." + field;
            internal string Id(string field)
            { var value = fields[field]; Need(!string.IsNullOrWhiteSpace(value) && value == value.Trim() && !value.Contains("|"), "InvalidValue", Path(field)); return value; }
            internal string OptionalId(string field) => fields[field].Length == 0 ? null : Id(field);
            internal BigInteger Integer(string field, bool positive = false)
            { var value = SignedInteger(fields[field], math, Path(field)); Need(value >= (positive ? BigInteger.One : BigInteger.Zero), "InvalidValue", Path(field)); return value; }
            internal BigInteger? OptionalInteger(string field) => fields[field].Length == 0 ? (BigInteger?)null : Integer(field);
            internal int? OptionalInt(string field) => fields[field].Length == 0 ? (int?)null : Int(field);
            internal int Int(string field, int min = 0)
            { var value = Integer(field); Need(value >= min && value <= int.MaxValue, "InvalidValue", Path(field)); return (int)value; }
            internal ExactRational Q(string field) => Rational(fields[field], math, Path(field));
            internal ExactRational OptionalQ(string field) => fields[field].Length == 0 ? null : Q(field);
            internal T Enum<T>(string field) where T : struct
            { var value = Id(field); Need(System.Enum.IsDefined(typeof(T), value), "UnsupportedBinding", Path(field)); return (T)System.Enum.Parse(typeof(T), value); }
            internal List<string> Set(string field)
            {
                var value = fields[field]; if (value == "-") return new List<string>();
                var ids = value.Split('|').ToList(); Need(ids.All(s => !string.IsNullOrWhiteSpace(s) && s == s.Trim()) && ids.Distinct(StringComparer.Ordinal).Count() == ids.Count,
                    "InvalidValue", Path(field)); return ids;
            }
        }
        private sealed class Tables
        {
            private readonly string directory, authoringDirectory; private readonly ContentConsumerCapabilities limits; private readonly ExactMathBudget math; private readonly bool csv;
            internal readonly List<DemoContentSourceInput> Sources = new List<DemoContentSourceInput>();
            private bool statefulSources;
            internal Tables(string directory, string authoringDirectory, ContentConsumerCapabilities limits, ExactMathBudget math, bool csv)
            { Text(directory, "Directory"); Text(authoringDirectory, "AuthoringDirectory"); this.directory = directory;
                this.authoringDirectory = authoringDirectory; this.limits = limits; this.math = math; this.csv = csv; }
            internal void UseStatefulSourcePaths()
            {
                statefulSources = true;
                foreach (var source in Sources)
                    source.Path = source.Path.Replace("ChapterOneTrial/Luban/", "Generated/FightMatch/ChapterOneTrial/r3/")
                        .Replace("Config/FightMatch/ChapterOneTrial/", "Config/FightMatch/ChapterOneTrial-r3/");
            }
            internal List<Row> Load(string name, string header)
            {
                var authorPath = System.IO.Path.Combine(authoringDirectory, name + ".csv"); Need(File.Exists(authorPath), "MissingField", name + ".Source");
                var authorBytes = PublishedContentCodec.ReadBounded(authorPath, limits.MaxSourceBytes);
                var columns = header.Split(','); var result = new List<Row>();
                if (csv)
                {
                    string text;
                    try { text = PublishedContentCodec.Utf8.GetString(authorBytes); } catch (DecoderFallbackException) { throw new ContentFailure("InvalidUnicode", name); }
                    if (text.Length > 0 && text[0] == '\ufeff') text = text.Substring(1);
                    var cells = Csv(text, name, limits);
                    Need(cells.Count > 0 && cells[0].SequenceEqual(columns), "InvalidSchema", name + ".Header");
                    for (var i = 1; i < cells.Count; i++)
                    { Need(cells[i].Count == columns.Length, "InvalidSchema", name + "[" + (i + 1) + "]"); result.Add(new Row(name, i + 1, columns, cells[i], math)); }
                }
                else
                {
                    var path = System.IO.Path.Combine(directory, name + ".json"); Need(File.Exists(path), "MissingField", name + ".LubanOutput");
                    var bytes = PublishedContentCodec.ReadBounded(path, limits.MaxSourceBytes);
                    var parsed = PublishedContentCodec.Parse(bytes, limits.MaxSourceBytes, limits);
                    Need(parsed is List<object>, "InvalidSchema", name + ".LubanOutput"); var rows = (List<object>)parsed;
                    for (var i = 0; i < rows.Count; i++)
                    {
                        Need(rows[i] is SortedDictionary<string, object>, "InvalidSchema", name + "[" + (i + 2) + "]");
                        var fields = (SortedDictionary<string, object>)rows[i];
                        Need(fields.Count == columns.Length && columns.All(fields.ContainsKey), "InvalidSchema", name + "[" + (i + 2) + "].Columns");
                        Need(fields.Values.All(v => v is string), "InvalidValue", name + "[" + (i + 2) + "].ExactStringColumns");
                        result.Add(new Row(name, i + 2, columns, columns.Select(c => (string)fields[c]).ToList(), math));
                    }
                    Sources.Add(new DemoContentSourceInput { Path = (statefulSources ? "Generated/FightMatch/ChapterOneTrial/r3/" : "ChapterOneTrial/Luban/") + name + ".json", Locator = "standard-generator-output", Sha256 = PublishedContentCodec.Sha256(bytes) });
                }
                Sources.Add(new DemoContentSourceInput { Path = (statefulSources ? "Config/FightMatch/ChapterOneTrial-r3/" : "Config/FightMatch/ChapterOneTrial/") + name + ".csv", Locator = "authoring-source", Sha256 = PublishedContentCodec.Sha256(authorBytes) });
                return result;
            }
            internal void Unique(List<Row> rows, params string[] keys)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var row in rows)
                {
                    var values = keys.Select(k => row.Id(k)).ToList();
                    Need(seen.Add(string.Concat(values.Select(v => v.Length + ":" + v))), "InconsistentBinding", row.Path(string.Join("+", keys)));
                }
            }
        }
        private static List<List<string>> Csv(string text, string name, ContentConsumerCapabilities limits)
        {
            var rows = new List<List<string>>(); var row = new List<string>(); var cell = new StringBuilder(); var quoted = false; var closed = false; var atStart = true;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (quoted)
                {
                    if (c == '"') { if (i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; } else { quoted = false; closed = true; } }
                    else cell.Append(c);
                }
                else if (atStart && c == '"') { quoted = true; atStart = false; }
                else if (c == ',' || c == '\n' || c == '\r')
                {
                    row.Add(cell.ToString()); cell.Clear(); atStart = true; closed = false;
                    Need(row.Count <= limits.MaxCollectionEntries, "BudgetExceeded", name + ".Columns");
                    if (c != ',')
                    {
                        if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                        rows.Add(row); row = new List<string>(); Need(rows.Count <= 65536, "BudgetExceeded", name + ".Rows");
                    }
                }
                else { Need(!closed && c != '"', "InvalidSchema", name + ".CSV"); cell.Append(c); atStart = false; }
                Need(cell.Length <= limits.MaxStringCodeUnits, "BudgetExceeded", name + ".Cell");
            }
            Need(!quoted, "InvalidSchema", name + ".CSV");
            if (row.Count > 0 || cell.Length > 0 || closed) { row.Add(cell.ToString()); rows.Add(row); }
            return rows;
        }
    }
}
