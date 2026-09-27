using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Core
{
    // Callers must not mutate this request or its route during Evaluate.
    public sealed class CandidateAttackRequest
    {
        public string PlayerId { get; set; }
        public string AttemptId { get; set; }
        public string OperationId { get; set; }
        public BigInteger? ExpectedSceneRevision { get; set; }
        public BattleCombatantKey Actor { get; set; }
        public BattlePairKey Pair { get; set; }
        public List<FlowPos> Route { get; set; }
    }

    public sealed class CandidateAttackSource
    {
        public string PlayerId { get; }
        public string AttemptId { get; }
        public string OperationId { get; }
        public BigInteger ExpectedSceneRevision { get; }
        public BigInteger ActionOrdinal { get; }
        public BattleCombatantKey Actor { get; }
        public BattlePairKey Pair { get; }
        public IReadOnlyList<FlowPos> Route { get; }

        internal CandidateAttackSource(CandidateAttackRequest request, BigInteger actionOrdinal)
        {
            PlayerId = request.PlayerId;
            AttemptId = request.AttemptId;
            OperationId = request.OperationId;
            ExpectedSceneRevision = request.ExpectedSceneRevision.Value;
            ActionOrdinal = actionOrdinal;
            Actor = request.Actor;
            Pair = request.Pair;
            Route = new List<FlowPos>(request.Route).AsReadOnly();
        }
    }

    // Unfinished work after the direct attack. Later stages complete one operation and snapshot.
    public sealed class CandidateCombatFrame
    {
        public CandidateRandomBinding Binding { get; }
        public BattleSnapshot BeforeSnapshot { get; }
        public CandidateAttackSource Action { get; }
        public IReadOnlyList<BattleMemberState> Members { get; }
        public IReadOnlyList<BattleEnemyState> Enemies { get; }
        public BattleRandomSnapshot Random { get; }
        public IReadOnlyList<BattleContributionTotals> Contributions { get; }
        public IReadOnlyList<BattleDamageFact> DamageFacts { get; }

        internal CandidateCombatFrame(CandidateRandomBinding binding, BattleSnapshot before,
            CandidateAttackSource action, IEnumerable<BattleEnemyState> enemies, BattleRandomSnapshot random,
            IEnumerable<BattleContributionTotals> contributions, BattleDamageFact fact)
        {
            Binding = binding;
            BeforeSnapshot = before;
            Action = action;
            Members = new List<BattleMemberState>(before.Members).AsReadOnly();
            Enemies = new List<BattleEnemyState>(enemies).AsReadOnly();
            Random = random;
            Contributions = new List<BattleContributionTotals>(contributions).AsReadOnly();
            DamageFacts = new List<BattleDamageFact> { fact }.AsReadOnly();
        }
    }

    public enum CandidateAttackRejectionCode
    {
        MissingField, InvalidValue, InconsistentBinding, StaleContext, InvalidPhase,
        ActorUnavailable, TargetUnavailable, OutOfRange, InvalidRoute
    }

    public sealed class CandidateDirectAttackResult
    {
        public bool IsAccepted => Frame != null;
        public CandidateCombatFrame Frame { get; }
        public BattleDamageFact Fact { get; }
        public CandidateAttackRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }
        public string RouteReasonCode { get; }
        public int? RouteCellIndex { get; }

        internal CandidateDirectAttackResult(CandidateCombatFrame frame, BattleDamageFact fact)
        { Frame = frame; Fact = fact; }

        internal CandidateDirectAttackResult(CandidateAttackRejectionCode code, string path,
            RouteValidationResult route = null)
        {
            RejectionCode = code;
            FieldPath = path;
            RouteReasonCode = route?.ReasonCode;
            RouteCellIndex = route?.CellIndex;
        }
    }
}
