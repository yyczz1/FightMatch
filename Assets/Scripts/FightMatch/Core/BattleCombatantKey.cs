using System;

namespace FightMatch.Core
{
    public enum BattleCombatantKind { Participant, Enemy }

    public sealed class BattleCombatantKey : IEquatable<BattleCombatantKey>
    {
        public string AttemptId { get; }
        public BattleCombatantKind Kind { get; }
        public string CharacterId { get; }
        public string FaceId { get; }
        public string EnemyInstanceKey { get; }

        internal BattleCombatantKey(string attemptId, BattleCombatantKind kind, string characterId,
            string faceId, string enemyInstanceKey)
        {
            AttemptId = attemptId;
            Kind = kind;
            CharacterId = characterId;
            FaceId = faceId;
            EnemyInstanceKey = enemyInstanceKey;
        }

        public static BattleCombatantKey ForParticipant(string attemptId, string characterId)
        {
            RequireText(attemptId, nameof(attemptId));
            RequireText(characterId, nameof(characterId));
            return new BattleCombatantKey(attemptId, BattleCombatantKind.Participant, characterId, null, null);
        }

        public static BattleCombatantKey ForEnemy(string attemptId, string faceId, string enemyInstanceKey)
        {
            RequireText(attemptId, nameof(attemptId));
            RequireText(faceId, nameof(faceId));
            RequireText(enemyInstanceKey, nameof(enemyInstanceKey));
            return new BattleCombatantKey(attemptId, BattleCombatantKind.Enemy, null, faceId, enemyInstanceKey);
        }

        public bool Equals(BattleCombatantKey other)
        {
            return other != null && Kind == other.Kind &&
                string.Equals(AttemptId, other.AttemptId, StringComparison.Ordinal) &&
                string.Equals(CharacterId, other.CharacterId, StringComparison.Ordinal) &&
                string.Equals(FaceId, other.FaceId, StringComparison.Ordinal) &&
                string.Equals(EnemyInstanceKey, other.EnemyInstanceKey, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) { return Equals(obj as BattleCombatantKey); }
        public override int GetHashCode()
        {
            // Collection hash only, never a persistent identifier or fingerprint.
            unchecked
            {
                var hash = (int)Kind;
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(AttemptId);
                hash = hash * 31 + (CharacterId == null ? 0 : StringComparer.Ordinal.GetHashCode(CharacterId));
                hash = hash * 31 + (FaceId == null ? 0 : StringComparer.Ordinal.GetHashCode(FaceId));
                return hash * 31 + (EnemyInstanceKey == null ? 0 : StringComparer.Ordinal.GetHashCode(EnemyInstanceKey));
            }
        }

        internal static void RequireText(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("A key component is required.", name);
        }
    }

    public sealed class BattlePairKey : IEquatable<BattlePairKey>
    {
        public string AttemptId { get; }
        public string FaceId { get; }
        public string PairId { get; }

        internal BattlePairKey(string attemptId, string faceId, string pairId)
        {
            AttemptId = attemptId;
            FaceId = faceId;
            PairId = pairId;
        }

        public static BattlePairKey Create(string attemptId, string faceId, string pairId)
        {
            BattleCombatantKey.RequireText(attemptId, nameof(attemptId));
            BattleCombatantKey.RequireText(faceId, nameof(faceId));
            BattleCombatantKey.RequireText(pairId, nameof(pairId));
            return new BattlePairKey(attemptId, faceId, pairId);
        }

        public bool Equals(BattlePairKey other)
        {
            return other != null && string.Equals(AttemptId, other.AttemptId, StringComparison.Ordinal) &&
                string.Equals(FaceId, other.FaceId, StringComparison.Ordinal) &&
                string.Equals(PairId, other.PairId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) { return Equals(obj as BattlePairKey); }
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(AttemptId);
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(FaceId);
                return hash * 31 + StringComparer.Ordinal.GetHashCode(PairId);
            }
        }
    }
}
