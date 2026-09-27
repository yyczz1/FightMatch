using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static FightMatch.Core.BusinessFields;

namespace FightMatch.Core
{
    internal sealed class CandidatePermanentCodec
    {
        private readonly BusinessFields f;
        internal CandidatePermanentCodec(BusinessFields fields) { f = fields; }

        internal ContentBinding Binding(ContentBinding value, string p)
        {
            return ((PublishedRuleContext)f.PublishedContext(value, p)).Binding;
        }

        internal DefinitionBinding Level(DefinitionBinding value, string p)
        {
            f.Required(value, p);
            var content = Binding(value?.Content, p + ".Content");
            return f.LevelBinding(content, value?.LevelId, value?.CanonicalLevelVersion, p);
        }

        internal CandidateOriginalGrantRef Grant(CandidateOriginalGrantRef value, string p)
        {
            f.Required(value, p);
            var kind = (CandidatePermanentSourceKind)f.Enum((int)(value?.Kind ?? 0), 1, 2, p + ".Kind");
            var player = f.Text(value?.PlayerId, p + ".Player");
            var ordinary = kind == CandidatePermanentSourceKind.OrdinaryBaseReward;
            var attempt = ordinary ? f.Text(value?.AttemptId, p + ".Attempt") : null;
            var settlement = ordinary ? f.Text(value?.SettlementId, p + ".Settlement") : null;
            var grant = !ordinary ? f.Text(value?.GrantId, p + ".Grant") : null;
            var purpose = !ordinary ? f.Text(value?.Purpose, p + ".Purpose") : null;
            var fact = ordinary ? null : f.Text(value?.FactId, p + ".Fact", true);
            var use = ordinary ? null : f.Text(value?.UseId, p + ".Use", true);
            var binding = Binding(value?.Binding, p + ".Binding");
            Need(kind == CandidatePermanentSourceKind.OrdinaryBaseReward
                ? attempt != null && settlement != null && grant == null && purpose == null && fact == null && use == null
                : attempt == null && settlement == null && grant != null && purpose != null, p + ".Shape");
            if (!f.Reading) Need(ordinary ? value.GrantId == null && value.Purpose == null && value.FactId == null && value.UseId == null :
                value.AttemptId == null && value.SettlementId == null, p + ".Shape");
            return new CandidateOriginalGrantRef(kind, player, binding, attempt, settlement, grant, purpose, fact, use);
        }

        internal CandidateExistingInclusionRef Inclusion(CandidateExistingInclusionRef value, string p)
        {
            f.Required(value, p);
            var checkpoint = f.Text(value?.CheckpointId, p + ".Checkpoint");
            var branch = f.Text(value?.BranchId, p + ".Branch");
            var commit = f.Text(value?.SourceCommitId, p + ".Commit");
            var revision = f.Integer(value?.EvidenceViewRevision ?? 0, p + ".Revision");
            var grant = Grant(value?.Grant, p + ".Grant");
            var line = f.I(value?.GrantLine ?? 0, p + ".Line");
            var start = f.Integer(value?.UnitStart ?? 0, p + ".Start");
            var count = f.Integer(value?.UnitCount ?? 0, p + ".Count", 1);
            var disposition = (CandidateInclusionDisposition)f.Enum((int)(value?.Disposition ?? 0), 1, 6, p + ".Disposition");
            var endpoint = f.Text(value?.EndpointOperationId, p + ".EndpointOperation", true);
            Need(line >= 0, p + ".Line");
            return new CandidateExistingInclusionRef(checkpoint, branch, commit, revision, grant, line, start, count, disposition, endpoint);
        }

        internal CandidatePermanentSourceLine Source(CandidatePermanentSourceLine value, string p)
        {
            f.Required(value, p);
            var grant = Grant(value?.Grant, p + ".Grant");
            var line = f.I(value?.GrantLine ?? 0, p + ".Line");
            var item = f.Text(value?.ItemId, p + ".Item");
            var quantity = f.Integer(value?.OriginalQuantity ?? 0, p + ".OriginalQuantity", 1);
            var acquired = f.Integer(value?.AcquisitionOrder ?? 0, p + ".AcquisitionOrder");
            var operation = f.Text(value?.OriginalOperationId, p + ".OriginalOperation");
            var commit = f.Text(value?.OriginalCommitId, p + ".OriginalCommit");
            var branch = f.Text(value?.OriginalBranchId, p + ".OriginalBranch", true);
            var inclusion = f.Optional(value?.Inclusion, Inclusion, p + ".ExistingInclusion");
            var retraction = f.Flag(value?.RetractionOperationId != null, p + ".HasRetraction") ?
                new CandidatePermanentSaveCodec(f).PermanentEffectReference(value?.RetractionOperationId, 4, p + ".Retraction") : null;
            Need(line >= 0, p + ".Line");
            return new CandidatePermanentSourceLine(grant, line, item, quantity, acquired, operation, commit, branch, inclusion, retraction);
        }

        internal CandidatePermanentPortion Portion(CandidatePermanentPortion value, string p)
        {
            f.Required(value, p);
            var tag = f.Enum(value?.Source == null ? 2 : 1, 1, 2, p + ".Tag");
            var source = tag == 1 ? Source(value?.Source, p + ".Source") : null;
            var operation = tag == 2 ? new CandidatePermanentSaveCodec(f).PermanentEffectReference(value?.OutputOperationId, 4, p + ".OutputEffect") : null;
            var line = f.I(value?.OutputLine ?? 0, p + ".OutputLine");
            var item = f.Text(value?.ItemId, p + ".Item");
            var start = f.Integer(value?.UnitStart ?? 0, p + ".Start");
            var count = f.Integer(value?.UnitCount ?? 0, p + ".Count", 1);
            Need(line >= 0 && (source == null || line == 0 && item == source.ItemId), p + ".Identity");
            if (source != null) CandidatePermanentInventory.Range(start, count, source.OriginalQuantity, f.Budget);
            if (!f.Reading) Need((value.Source == null) != (value.OutputOperationId == null), p + ".Tag");
            var endpoint = (CandidatePermanentEndpoint)f.Enum((int)(value?.Endpoint ?? CandidatePermanentEndpoint.Held), 1, 3, p + ".Endpoint");
            var endpointOperation = endpoint == CandidatePermanentEndpoint.Held ? null :
                new CandidatePermanentSaveCodec(f).PermanentEffectReference(value?.EndpointOperationId, 4, p + ".EndpointEffect");
            if (!f.Reading) Need(endpoint != CandidatePermanentEndpoint.Held || value.EndpointOperationId == null, p + ".Held");
            return new CandidatePermanentPortion(source, operation, line, item, start, count, endpoint, endpointOperation);
        }

        internal CandidateInventoryQuantity Quantity(CandidateInventoryQuantity value, string p)
        {
            f.Required(value, p);
            var item = f.Text(value?.ItemId, p + ".Item");
            return new CandidateInventoryQuantity(item, f.Integer(value?.Quantity ?? 0, p + ".Quantity", 1));
        }

        internal CandidatePermanentQuote Quote(CandidatePermanentQuote value, string p)
        {
            f.Required(value, p);
            var kind = (CandidatePermanentKind)f.Enum((int)(value?.Kind ?? 0), 1, 7, p + ".Kind");
            var draft = new CandidatePermanentDraft
            {
                Kind = kind,
                CharacterId = f.Text(value?.CharacterId, p + ".Character", true),
                DefinitionId = kind != CandidatePermanentKind.Equip || f.Flag(value?.DefinitionId != null, p + ".HasItem")
                    ? f.Text(value?.DefinitionId, p + ".Definition") : null,
                Quantity = f.Flag(value?.Quantity != null, p + ".HasQuantity")
                    ? (BigInteger?)f.Integer(value?.Quantity ?? 0, p + ".Quantity") : null,
                Enabled = f.Flag(value?.Enabled != null, p + ".HasEnabled")
                    ? (bool?)f.Flag(value?.Enabled ?? false, p + ".Enabled") : null,
                StepId = f.Text(value?.StepId, p + ".Step", true)
            };
            var player = f.Text(value?.PlayerId, p + ".Player");
            var classId = f.Text(value?.ClassId, p + ".Class", true);
            var binding = Binding(value?.Binding, p + ".Binding");
            var generation = f.Integer(value?.SourceGeneration ?? 0, p + ".Generation", 1);
            var length = f.U(value?.SourceDescriptorLength ?? 0, 8, p + ".DescriptorLength");
            var hash = Digest(value?.SourceDescriptorSha256, p + ".DescriptorSha256");
            var version = f.Integer(value?.DefinitionVersion ?? 0, p + ".Version", 1);
            var character = f.Integer(value?.CharacterRevision ?? 0, p + ".CharacterRevision");
            var inventory = f.Integer(value?.InventoryRevision ?? 0, p + ".InventoryRevision", 1);
            var preference = f.Integer(value?.PreferenceRevision ?? 0, p + ".PreferenceRevision", 1);
            var progression = f.Integer(value?.ProgressionRevision ?? 0, p + ".ProgressionRevision", 1);
            var unitXp = f.Integer(value?.UnitExperience ?? 0, p + ".UnitExperience");
            var xp = f.Integer(value?.FixedExperience ?? 0, p + ".FixedExperience");
            var beforeLevel = f.Integer(value?.BeforeLevel ?? 0, p + ".BeforeLevel");
            var beforeXp = f.Integer(value?.BeforeExperience ?? 0, p + ".BeforeExperience");
            var level = f.Integer(value?.FinalLevel ?? 0, p + ".FinalLevel");
            var remainder = f.Integer(value?.FinalExperience ?? 0, p + ".FinalExperience");
            var teaching = f.Optional(value?.TeachingLevel, Level, p + ".Teaching");
            var inputs = f.List(value?.Inputs, Portion, p + ".Inputs");
            var costs = f.List(value?.Costs, Quantity, p + ".Costs");
            var outputs = f.List(value?.Outputs, Quantity, p + ".Outputs");
            var learned = f.Text(value?.OriginalLearningOperation, p + ".OriginalLearning", true);
            for (var i = 1; i < inputs.Count; i++)
            {
                var prior = inputs[i - 1];
                var next = inputs[i];
                Need(CandidatePermanentInventory.Compare(prior, next) < 0, p + ".Inputs.Order");
                Need(!SameRoot(prior, next, f.Budget) || f.Budget.Math.Add(prior.UnitStart, prior.UnitCount) < next.UnitStart,
                    p + ".Inputs.CanonicalRanges");
            }
            CandidatePermanentSaveCodec.PermanentQuantities(costs, p + ".Costs");
            CandidatePermanentSaveCodec.PermanentQuantities(outputs, p + ".Outputs");
            draft.SelectedInputs = inputs;
            if (draft.Kind == CandidatePermanentKind.SetPreference) draft.PreferenceRevision = preference;
            var result = new CandidatePermanentQuote(draft, player, classId, binding, generation, length, hash,
                version, character, inventory, preference, progression, unitXp, xp, beforeLevel, beforeXp,
                level, remainder, teaching, inputs, costs, outputs, learned);
            CandidatePermanentProtocol.Shape(result, f.Budget);
            if (f.Resolved != null && result.Kind != CandidatePermanentKind.Equip && result.Kind != CandidatePermanentKind.SetPreference)
            {
                var definitions = f.Resolved.FindExact(result.Binding)?.GetPermanentDefinitions();
                Need(definitions != null, p + ".Definitions", "UnsupportedBinding");
                definitions.CheckQuote(result, f.Budget);
            }
            return result;
        }

        internal IReadOnlyList<byte> Digest(IReadOnlyList<byte> value, string p)
        {
            if (!f.Reading) Need(value != null && value.Count == 32, p, "MissingField");
            var bytes = new byte[32];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = (byte)f.U(value?[i] ?? 0, 1, p);
            return bytes;
        }

        internal CandidatePermanentEffect Effect(CandidatePermanentEffect value, string p, int owner)
        {
            f.Required(value, p);
            var operation = f.Text(value?.OperationId, p + ".Operation");
            var quote = Quote(value?.Quote, p + ".Quote");
            var outcome = f.Text(value?.Outcome, p + ".Outcome");
            var related = f.Text(value?.RelatedOperationId, p + ".RelatedOperation", true);
            Need(outcome == "Applied" || outcome == "AlreadyLearned" || outcome == "Unchanged", p + ".Outcome");
            var result = new CandidatePermanentEffect(operation, quote, outcome, related);
            new CandidatePermanentSaveCodec(f).PermanentEndpoints(result, owner, p);
            return result;
        }

        internal IReadOnlyList<CandidatePermanentEffect> Effects(IReadOnlyList<CandidatePermanentEffect> values, string p, int owner = 4)
        {
            var list = f.List(values, (v, field) => Effect(v, field, owner), p);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var effect in list) Need(seen.Add(effect.OperationId), p + ".Operation");
            if (owner == 3)
                for (var i = 1; i < list.Count; i++)
                {
                    var previous = list[i - 1];
                    var current = list[i];
                    var order = StringComparer.Ordinal.Compare(previous.Quote.CharacterId, current.Quote.CharacterId);
                    Need(order < 0 || order == 0 && StringComparer.Ordinal.Compare(previous.OperationId, current.OperationId) < 0, p + ".Order");
                }
            return list;
        }

        internal CandidatePermanentInventoryLedger Ledger(CandidatePermanentInventoryLedger value, string p)
        {
            f.Required(value, p);
            var sources = f.List(value?.Sources, Source, p + ".Sources");
            for (var i = 1; i < sources.Count; i++)
                Need(CandidatePermanentInventory.Compare(
                    new CandidatePermanentPortion(sources[i - 1], null, 0, sources[i - 1].ItemId, 0, 1),
                    new CandidatePermanentPortion(sources[i], null, 0, sources[i].ItemId, 0, 1)) < 0, p + ".Sources.Order");
            var effects = Effects(value?.Effects, p + ".Effects");
            return new CandidatePermanentInventoryLedger(sources, effects);
        }

        internal CandidatePermanentResult Result(CandidatePermanentResult value, string p)
        {
            f.Required(value, p);
            var outcome = f.Text(value?.Outcome, p + ".Outcome");
            Need(outcome == "Applied" || outcome == "AlreadyLearned" || outcome == "Unchanged", p + ".Outcome");
            var character = f.Text(value?.CharacterEffectOperation, p + ".M03", true);
            var inventory = f.Text(value?.InventoryEffectOperation, p + ".M04", true);
            var progression = f.Text(value?.ProgressionEffectOperation, p + ".M05", true);
            var learned = f.Text(value?.OriginalLearningOperation, p + ".OriginalLearning", true);
            return new CandidatePermanentResult(outcome, character, inventory, progression, learned);
        }

        private static byte[] Bytes<T>(T value, Func<CandidatePermanentCodec, T, string, T> visit, SaveCodecBudget budget)
        {
            using (var stream = new MemoryStream())
            {
                visit(new CandidatePermanentCodec(new BusinessFields(stream, false, budget) { StrictUnicode = true }), value, "Permanent");
                return stream.ToArray();
            }
        }

        internal static bool SameGrant(CandidateOriginalGrantRef a, CandidateOriginalGrantRef b, SaveCodecBudget budget)
        {
            return CandidateApplicationIntentCodec.SameBytes(Bytes(a, (c, x, p) => c.Grant(x, p), budget), Bytes(b, (c, x, p) => c.Grant(x, p), budget));
        }

        internal static bool SameRoot(CandidatePermanentPortion a, CandidatePermanentPortion b, SaveCodecBudget budget)
        {
            if (a.ItemId != b.ItemId || a.OutputOperationId != b.OutputOperationId || a.OutputLine != b.OutputLine) return false;
            if (a.Source == null || b.Source == null) return a.Source == b.Source;
            return CandidateApplicationIntentCodec.SameBytes(Bytes(a.Source, (c, x, p) => c.Source(x, p), budget), Bytes(b.Source, (c, x, p) => c.Source(x, p), budget));
        }

        internal static bool SameQuote(CandidatePermanentQuote a, CandidatePermanentQuote b, SaveCodecBudget budget)
        {
            return CandidateApplicationIntentCodec.SameBytes(Bytes(a, (c, x, p) => c.Quote(x, p), budget), Bytes(b, (c, x, p) => c.Quote(x, p), budget));
        }
    }
}
