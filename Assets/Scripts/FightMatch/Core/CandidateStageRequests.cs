using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    // Callers must not mutate requests or HP projections during stage evaluation.
    public sealed class CandidateLinkRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string OperationId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public BattlePairKey Pair { get; set; }
        public List<FlowPos> Route { get; set; }
    }

    // Stage input only. The action assembler must match this against actual combat evaluation.
    public sealed class CandidateFinalHpProjection
    {
        public List<CandidateHpInput> MemberHp { get; set; }
        public List<CandidateHpInput> EnemyHp { get; set; }
    }

    public sealed class CandidateHpInput
    {
        public BattleCombatantKey CombatantKey { get; set; }
        public ExactRational Hp { get; set; }
    }

    public enum CandidateStageOperation { AfterAttack, CompleteLink }

    public sealed class CandidateStageSource
    {
        public CandidateStageOperation Kind { get; }
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string OperationId { get; }
        public BigInteger ExpectedSceneRevision { get; }
        public BattleCombatantKey Actor { get; }
        public BattlePairKey Pair { get; }
        public IReadOnlyList<FlowPos> Route { get; }

        internal CandidateStageSource(CandidateStageOperation kind, string playerId, string attemptId,
            string operationId, BigInteger revision, BattleCombatantKey actor, BattlePairKey pair,
            IEnumerable<FlowPos> route)
        {
            Kind = kind; PlayerId = playerId; AttemptId = attemptId; OperationId = operationId;
            ExpectedSceneRevision = revision; Actor = actor; Pair = pair;
            Route = new List<FlowPos>(route).AsReadOnly();
        }
    }

    public sealed class CandidateHpValue
    {
        public BattleCombatantKey CombatantKey { get; }
        public ExactRational Hp { get; }

        internal CandidateHpValue(BattleCombatantKey key, ExactRational hp) { CombatantKey = key; Hp = hp; }
    }

    public sealed class CandidateFinalHpValues
    {
        public IReadOnlyList<CandidateHpValue> MemberHp { get; }
        public IReadOnlyList<CandidateHpValue> EnemyHp { get; }

        internal CandidateFinalHpValues(CandidateFinalHpProjection projection)
        {
            MemberHp = projection.MemberHp.ConvertAll(row => new CandidateHpValue(row.CombatantKey, row.Hp)).AsReadOnly();
            EnemyHp = projection.EnemyHp.ConvertAll(row => new CandidateHpValue(row.CombatantKey, row.Hp)).AsReadOnly();
        }
    }
}
