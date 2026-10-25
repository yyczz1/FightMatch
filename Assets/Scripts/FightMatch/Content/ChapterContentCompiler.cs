using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using FightMatch.Core;
using FlowPuzzle.Core;
using FlowPuzzle.Validation;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    internal static class ChapterContentCompiler
    {
        internal static void Normalize(PublishedSource s, ContentConsumerCapabilities caps, ExactMathBudget math)
        {
            var chapter = s.Chapter; Root(chapter, "Chapter"); Text(chapter.ReleaseSetId, "Chapter.ReleaseSetId");
            Text(chapter.SourceLocator, "Chapter.SourceLocator");
            Need(chapter.ParameterStatus == ChapterParameterStatus.Trial || chapter.ParameterStatus == ChapterParameterStatus.Approved,
                "UnsupportedBinding", "Chapter.ParameterStatus");
            Need(s.Coordinates == DemoCoordinateCandidate.AssumedTopLeft, "UnsupportedBinding", "Coordinates");
            Root(chapter.Passives, "Passive"); Root(chapter.CharacterUnlocks, "CharacterUnlock"); Root(chapter.FirstClearItems, "FirstClearItem");
            Root(chapter.Cards, "Card"); Root(chapter.Skills, "Skill"); Root(chapter.Teachings, "Teaching");
            Need(s.Growths.Count == 2 && s.Growths.Count(g => g.ClassKind == CharacterClassKind.Warrior) == 1 &&
                s.Growths.Count(g => g.ClassKind == CharacterClassKind.Mage) == 1 &&
                s.Growths.Select(g => g.ClassId).Distinct(StringComparer.Ordinal).Count() == 2, "UnsupportedBinding", "CharacterGrowth.ClassKind");
            Need(s.Growths.Single(g => g.ClassKind == CharacterClassKind.Warrior).ClassId == s.NewProfile.ClassId,
                "UnsupportedBinding", "NewProfile.ClassId");
            Need(chapter.Passives.All(p => p != null) && chapter.CharacterUnlocks.All(p => p != null) && chapter.FirstClearItems.All(p => p != null) &&
                chapter.Cards.All(p => p != null) && chapter.Skills.All(p => p != null) && chapter.Teachings.All(p => p != null), "MissingField", "Chapter.Rows");
            s.Growths.Sort((a, b) => StringComparer.Ordinal.Compare(a.ClassId, b.ClassId));
            chapter.Passives.Sort((a, b) => StringComparer.Ordinal.Compare(a.ClassId, b.ClassId));
            chapter.CharacterUnlocks.Sort((a, b) => StringComparer.Ordinal.Compare(a.ClassId, b.ClassId));
            chapter.FirstClearItems.Sort((a, b) => Compare(a.LevelId, a.ItemId, b.LevelId, b.ItemId));
            chapter.Cards.Sort((a, b) => StringComparer.Ordinal.Compare(a.ItemId, b.ItemId));
            chapter.Skills.Sort((a, b) => StringComparer.Ordinal.Compare(a.SkillId, b.SkillId));
            chapter.Teachings.Sort((a, b) => StringComparer.Ordinal.Compare(a.TutorialId, b.TutorialId));
            foreach (var source in s.Sources)
                Need(source != null && !string.IsNullOrWhiteSpace(source.Path) && !string.IsNullOrWhiteSpace(source.Locator) &&
                    PublishedContentCatalog.IsSha(source.Sha256) && (source.Bytes == null || PublishedContentCodec.Sha256(source.Bytes) == source.Sha256),
                    "InconsistentBinding", "Sources");
            Need(s.Sources.Count > 0, "MissingField", "Sources");
            Need(s.Sources.Select(row => row.Path.Length + ":" + row.Path + row.Locator).Distinct(StringComparer.Ordinal).Count() == s.Sources.Count,
                "InconsistentBinding", "Sources.DuplicateLocator");
            Need(chapter.CharacterUnlocks.Count == 1 && chapter.CharacterUnlocks[0].ClassId == s.Growths.Single(g => g.ClassKind == CharacterClassKind.Mage).ClassId,
                "InconsistentBinding", "CharacterUnlock.Mage");
            foreach (var l in s.Levels)
            {
                Need(l.Reward.LevelId == l.Level.LevelId && l.Reward.LevelVersion == l.Level.LevelVersion,
                    "InconsistentBinding", "Reward.Level");
                var enemies = new HashSet<string>(StringComparer.Ordinal);
                foreach (var face in l.Level.Faces ?? new List<FaceInput>())
                {
                    Root(face, "Face"); Root(face.Pairs, "PairEnemy");
                    for (var i = 0; i < face.Pairs.Count; i++)
                    {
                        var e = face.Pairs[i]?.Enemy; Root(e, "PairEnemy.Enemy");
                        Need(enemies.Add(e.EnemyInstanceKey), "InconsistentBinding", "PairEnemy.EnemyInstanceKey");
                    }
                    Need(face.Pairs.Select(p => p.Enemy.OriginalSlot).OrderBy(x => x).SequenceEqual(Enumerable.Range(0, face.Pairs.Count)) &&
                        face.Pairs.Select(p => p.Enemy.StableOrder).OrderBy(x => x).SequenceEqual(Enumerable.Range(0, face.Pairs.Count)),
                        "InvalidValue", "PairEnemy.SlotOrder");
                }
                foreach (var item in l.Reward.Materials)
                    Need(!chapter.Teachings.Any(t => t.GiftItemId == item.ItemId) &&
                        !chapter.Skills.Any(t => t.CostItemId == item.ItemId), "UnsupportedBinding", "RewardMaterial.Certificate");
            }
        }
        private static int Compare(string a, string b, string c, string d)
        { var first = StringComparer.Ordinal.Compare(a, c); return first == 0 ? StringComparer.Ordinal.Compare(b, d) : first; }

        internal static PublishedContentCompiler.BuildResult Build(PublishedSource source, ContentBinding binding, DemoContentJob job,
            ContentConsumerCapabilities caps, ExactMathBudget math)
        {
            var result = new PublishedContentCompiler.BuildResult(); var context = new PublishedRuleContext(binding);
            var budget = new SaveCodecBudget(math); var growths = new List<CandidateGrowthDefinition>();
            foreach (var raw in source.Growths)
            {
                var input = Copy(raw, caps, math); input.Context = context;
                var prepared = CandidateCharacterGrowth.PrepareDefinition(input, math);
                Need(prepared.IsAccepted, prepared.RejectionCode.ToString(), "CharacterGrowth." + prepared.FieldPath); growths.Add(prepared.Definition);
            }
            var chapter = ChapterRuleDefinitions.Prepare(binding, source.Chapter.Passives, source.Chapter.CharacterUnlocks, source.Chapter.FirstClearItems, budget);
            Need(chapter.IsAccepted, chapter.RejectionCode, chapter.FieldPath);
            var parameters = ProveParameters(source, chapter.Value, job, math); result.Parameters = parameters;
            var inventoryInput = Copy(source.Inventory, caps, math); inventoryInput.Context = context;
            var inventory = CandidateInventory.PrepareDefinition(inventoryInput, math); Need(inventory.IsAccepted, inventory.RejectionCode.ToString(), inventory.FieldPath);
            var progressionInput = Copy(source.Progression, caps, math); progressionInput.Context = context;
            var progression = CandidateProgression.PrepareDefinition(progressionInput, math); Need(progression.IsAccepted, progression.RejectionCode.ToString(), progression.FieldPath);
            var p = source.NewProfile; var growth = growths.Single(g => g.ClassId == p.ClassId);
            var character = CandidateCharacterGrowth.CreateCandidate(growth, "validation:player", p.CharacterId, p.Level, p.Experience, p.OriginalSlot, math);
            Need(character.IsAccepted, character.RejectionCode.ToString(), character.FieldPath);
            var stats = CandidateCharacterGrowth.ComputeBaseStats(character.Next, math);
            Need(p.Hp.Compare(stats.EntryHp, math) == 0, "InconsistentBinding", "NewProfile.Hp");
            var levels = new List<PreparedLevel>(); var rewards = new List<CandidateRewardDefinition>(); var geometries = new List<object>();
            foreach (var l in source.Levels)
            {
                job.Check();
                var candidate = new CandidateContext { DraftId = source.DraftId, DraftRevision = source.Revision,
                    ContentFingerprint = binding.ContentFingerprint, RuleVersion = binding.RuleVersion, NumericContractVersion = binding.NumericContractVersion,
                    RandomContractVersion = binding.RandomContractVersion, SourceNotes = source.SourceNotes };
                var member = new MemberInput { CharacterId = p.CharacterId, ClassId = p.ClassId, ClassKind = stats.ClassKind,
                    OriginalSlot = p.OriginalSlot, Level = p.Level, IsReady = stats.IsReady, StatsOrigin = stats.StatsOrigin, StatsContext = candidate,
                    Stats = new StatsInput { MaxHp = stats.Stats.MaxHp, Attack = stats.Stats.Attack, PhysicalDefense = stats.Stats.PhysicalDefense,
                        MagicDefense = stats.Stats.MagicDefense, Evasion = stats.Stats.Evasion, AttackRange = stats.Stats.AttackRange }, EntryHp = p.Hp,
                    LearnedSkills = new List<string>(), Crit = new WarriorCritInput { PassiveDefinitionId = stats.PassiveDefinitionId,
                        TargetProbability = stats.TargetProbability, C = parameters[(int)BigInteger.Min(p.Level, 31) - 1].C, Multiplier = stats.CritMultiplier } };
                var input = new BattleEntryInput { Context = candidate, PlayerId = "validation:player", ChallengeId = "validation:challenge:" + l.Level.LevelId,
                    AttemptId = "validation:attempt:" + l.Level.LevelId, EntryBaselineId = "validation:baseline:" + l.Level.LevelId,
                    Level = l.Level, Members = new List<MemberInput> { member }, CarryMode = EntryCarryMode.Empty, RequiredFeatures = new List<string>() };
                var draft = new BattleEntryPreparer().PrepareCandidate(input, math);
                Need(draft.IsAccepted, draft.RejectionCode.ToString(), "Level." + draft.FieldPath);
                var db = DefinitionBinding.Prepare(binding, l.Level.LevelId, BigInteger.Parse(l.Level.LevelVersion, CultureInfo.InvariantCulture), budget);
                Need(db.IsAccepted, db.RejectionCode, db.FieldPath); result.LevelBindings.Add(db.Value);
                input.Context = context; member.StatsContext = context;
                var published = new BattleEntryPreparer().PreparePublished(input, db.Value, draft.Entry.Level, math);
                Need(published.IsAccepted, published.RejectionCode.ToString(), "Level." + published.FieldPath);
                geometries.Add(Geometry(published.Entry.Level, l.SourceRoutes, job)); levels.Add(published.Entry.Level);
                var rewardInput = Copy(l.Reward, caps, math); rewardInput.Context = context;
                var reward = CandidateBaseRewards.PrepareDefinition(rewardInput, math);
                Need(reward.IsAccepted, reward.RejectionCode.ToString(), "Reward." + reward.FieldPath); rewards.Add(reward.Definition);
            }
            var definitions = PublishedRuleDefinitions.Prepare(binding, growths, inventory.Definition, progression.Definition, levels, rewards, budget);
            Need(definitions.IsAccepted, definitions.RejectionCode, definitions.FieldPath);
            var withChapter = PublishedRuleDefinitions.PrepareChapter(definitions.Value, chapter.Value, budget);
            Need(withChapter.IsAccepted, withChapter.RejectionCode, withChapter.FieldPath);
            result.Definitions = Permanent(source, withChapter.Value, budget);
            result.Profile = new NewProfileDefinition(p, PublishedContentCodec.Encode(p, caps.MaxSourceBytes, caps, math));
            result.ChapterEvidence = new { Kind = "ch1-definition-validation-v1", Binding = ContentBindingRecord.From(binding),
                Parameters = parameters, Geometry = geometries, BattleReplayPerformed = false };
            return result;
        }
        private static T Copy<T>(T input, ContentConsumerCapabilities caps, ExactMathBudget math)
        { return PublishedContentCodec.Decode<T>(PublishedContentCodec.Encode(input, caps.MaxSourceBytes, caps, math), caps.MaxSourceBytes, caps, math); }
        private static List<DemoParameterEvidence> ProveParameters(PublishedSource source, ChapterRuleDefinitions chapter, DemoContentJob job, ExactMathBudget math)
        {
            var result = new List<DemoParameterEvidence>();
            for (var i = 0; i < source.Parameters.Count; i++)
            {
                var row = source.Parameters[i]; Root(row, "Parameters");
                foreach (var passive in chapter.Passives)
                    Need(row.Target != null && passive.ProbabilityAt(i + 1, math).Compare(row.Target, math) == 0,
                        "InconsistentBinding", "Parameters." + passive.Id + "." + (i + 1));
                var proof = DemoContentParameters.EvaluateParameterEvidence(row.Target, row.C, row.Epsilon, job, math);
                Need(proof.IsAccepted, proof.RejectionCode, proof.FieldPath);
                Need(proof.Evidence.WithinProposedTolerance, "ToleranceExceeded", "Parameters.Epsilon"); result.Add(proof.Evidence);
            }
            foreach (var passive in chapter.Passives)
                Need(passive.ProbabilityAt(31, math).Compare(passive.ProbabilityCap, math) == 0, "UnsupportedBinding", "Parameters.CapAt31");
            return result;
        }
        private static object Geometry(PreparedLevel prepared, List<DemoContentRouteInput> routes, DemoContentJob job)
        {
            var result = new List<object>(); var used = new HashSet<DemoContentRouteInput>();
            foreach (var face in prepared.Faces)
            {
                job.Check(); var level = new FlowLevelData { levelId = result.Count + 1, width = face.Width, height = face.Height };
                var solution = new FlowSolutionData { levelId = level.levelId };
                foreach (var pair in face.Pairs)
                {
                    var matches = routes.Where(r => r != null && r.FaceId == face.FaceId && r.PairId == pair.PairId).ToList();
                    Need(matches.Count == 1, "InconsistentBinding", "DefaultRouteCell." + face.FaceId + "." + pair.PairId);
                    var route = matches[0]; Root(route.Cells, "DefaultRouteCell.Cells"); used.Add(route);
                    var cells = new List<FlowPos>();
                    foreach (var cell in route.Cells)
                    {
                        Need(cell.x >= 1 && cell.x <= face.Width && cell.y >= 1 && cell.y <= face.Height, "InvalidValue", "DefaultRouteCell.Coordinate");
                        cells.Add(new FlowPos(cell.x - 1, face.Height - cell.y));
                    }
                    level.pairs.Add(new FlowPairData { colorId = pair.GeometryColorId, endpointA = pair.EndpointA, endpointB = pair.EndpointB });
                    solution.paths.Add(new FlowPathData { colorId = pair.GeometryColorId, cells = cells });
                    result.Add(new { FaceId = face.FaceId, PairId = pair.PairId, Cells = cells });
                }
                var validation = new FlowSolutionValidator().Validate(level, solution);
                Need(validation.isValid, "InvalidGeometry", face.FaceId + ":" + validation.errorCode);
            }
            Need(used.Count == routes.Count, "InconsistentBinding", "DefaultRouteCell.Unbound"); return result;
        }
        private static PublishedRuleDefinitions Permanent(PublishedSource source, PublishedRuleDefinitions basis, SaveCodecBudget budget)
        {
            var records = new List<CandidatePermanentDefinitionInput>(); var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var card in source.Chapter.Cards)
            {
                Need(seen.Add(card.ItemId), "InconsistentBinding", "Card.ItemId");
                Need(source.Inventory.Items.Any(i => i.ItemId == card.ItemId && i.Kind == CandidateInventoryItemKind.OrdinaryMaterial && i.EquipClassId == null),
                    "UnsupportedBinding", "Card.ItemId");
                records.Add(new CandidatePermanentDefinitionInput { Kind = CandidatePermanentDefinitionKind.Card, Id = card.ItemId,
                    RecordVersion = card.RecordVersion, UnitExperience = card.UnitExperience });
            }
            // A data row alone never activates an unavailable skill or tutorial consumer.
            Need(source.Chapter.Skills.Count == 0 && source.Chapter.Teachings.Count == 0, "UnsupportedCapability", "Chapter.TeachingAndSkill");
            var permanent = CandidatePermanentDefinitions.Prepare(basis.Binding, records, budget);
            Need(permanent.IsAccepted, permanent.RejectionCode, permanent.FieldPath);
            var attached = PublishedRuleDefinitions.PreparePermanent(basis, permanent.Value, budget);
            Need(attached.IsAccepted, attached.RejectionCode, attached.FieldPath); return attached.Value;
        }
    }
}
