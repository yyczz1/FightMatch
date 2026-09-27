using System;
using System.Collections.Generic;
using System.Numerics;
using static FightMatch.Core.BattleEntryRejectionCode;

namespace FightMatch.Core
{
    public static class CandidateRandomPreparer
    {
        public const string SupportedMappingId = "sc01-pcg32-le128-v1";

        public static CandidateRandomPreparationResult Prepare(PreparedBattleEntry entry,
            CandidateSeedMaterial material, ExactMathBudget budget)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (material == null) throw new ArgumentNullException(nameof(material));
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (string.IsNullOrWhiteSpace(material.SourceCapabilityId))
                return new CandidateRandomPreparationResult(MissingField, "Material.SourceCapabilityId");
            if (string.IsNullOrWhiteSpace(material.MappingId))
                return new CandidateRandomPreparationResult(MissingField, "Material.MappingId");
            if (material.Bytes == null) return new CandidateRandomPreparationResult(MissingField, "Material.Bytes");
            if (material.Bytes.Length != 48) return new CandidateRandomPreparationResult(InvalidValue, "Material.Bytes");
            if (!string.Equals(material.MappingId, SupportedMappingId, StringComparison.Ordinal))
                return new CandidateRandomPreparationResult(UnsupportedBinding, "Material.MappingId");

            var battle = Map(material.Bytes, 0, CandidateRandomPurpose.Battle, budget);
            var baseReward = Map(material.Bytes, 16, CandidateRandomPurpose.BaseReward, budget);
            var bonus = Map(material.Bytes, 32, CandidateRandomPurpose.Bonus, budget);
            var initials = new CandidateRandomInitials
            {
                Battle = battle.Initial, BaseReward = baseReward.Initial, Bonus = bonus.Initial,
                PrdStates = new List<CandidatePrdInitial>()
            };
            foreach (var member in entry.ReadyParticipants)
                initials.PrdStates.Add(new CandidatePrdInitial
                { CharacterId = member.CharacterId, PassiveDefinitionId = member.Crit.PassiveDefinitionId, FailureCount = BigInteger.Zero });
            var start = BattleStartAssembler.CreateCandidate(entry, initials, budget);
            if (!start.IsAccepted) return new CandidateRandomPreparationResult(start.RejectionCode.Value, start.FieldPath);
            return new CandidateRandomPreparationResult(new CandidateRandomBinding(start.Start,
                material.SourceCapabilityId, material.MappingId, battle, baseReward, bonus));
        }

        private static CandidateRandomDomain Map(byte[] bytes, int offset, CandidateRandomPurpose purpose, ExactMathBudget budget)
        {
            var a = ReadLittleEndian(bytes, offset);
            // SC01 alone maps the full 64-bit material to the core's strict 63-bit sequence input.
            var b = ReadLittleEndian(bytes, offset + 8) & 0x7fffffffffffffffUL;
            budget.CheckInteger(a);
            budget.CheckInteger(b);
            var state = Pcg32StreamState.Initialize(a, b);
            state.Validate(budget);
            return new CandidateRandomDomain(purpose, a, b, state);
        }

        private static ulong ReadLittleEndian(byte[] bytes, int offset)
        {
            ulong value = 0;
            for (var i = 0; i < 8; i++) value |= (ulong)bytes[offset + i] << (8 * i);
            return value;
        }
    }
}
