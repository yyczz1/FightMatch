using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    // An enemy-phase fragment. The complete operation is assembled by the later operation layer.
    public sealed class CandidateEnemyPhaseFrame
    {
        public CandidateCombatFrame DirectAttack { get; }
        public IReadOnlyList<BattleMemberState> Members { get; }
        public IReadOnlyList<BattleEnemyState> Enemies { get; }
        public IReadOnlyList<BattleContributionTotals> Contributions { get; }
        public BattleRandomSnapshot Random => DirectAttack.Random;
        public BigInteger EnemyPhaseOrdinal { get; }
        public IReadOnlyList<CandidateEnemyIntentFact> OrderedIntents { get; }

        internal CandidateEnemyPhaseFrame(CandidateCombatFrame directAttack, BigInteger ordinal,
            IEnumerable<BattleMemberState> members, IEnumerable<BattleEnemyState> enemies,
            IEnumerable<BattleContributionTotals> contributions, IEnumerable<CandidateEnemyIntentFact> intents)
        {
            DirectAttack = directAttack;
            EnemyPhaseOrdinal = ordinal;
            Members = new List<BattleMemberState>(members).AsReadOnly();
            Enemies = new List<BattleEnemyState>(enemies).AsReadOnly();
            Contributions = new List<BattleContributionTotals>(contributions).AsReadOnly();
            OrderedIntents = new List<CandidateEnemyIntentFact>(intents).AsReadOnly();
        }
    }

    public enum CandidateEnemyPhaseRejectionCode
    { MissingField, InvalidValue, UnsupportedBinding, InconsistentBinding, InvalidPhase }

    public sealed class CandidateEnemyPhaseResult
    {
        public bool IsAccepted => Frame != null;
        public CandidateEnemyPhaseFrame Frame { get; }
        public CandidateEnemyPhaseRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }

        internal CandidateEnemyPhaseResult(CandidateEnemyPhaseFrame frame) { Frame = frame; }
        internal CandidateEnemyPhaseResult(CandidateEnemyPhaseRejectionCode code, string path)
        { RejectionCode = code; FieldPath = path; }
    }
}
