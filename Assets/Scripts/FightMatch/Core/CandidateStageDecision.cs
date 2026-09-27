using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public enum CandidateStageFactKind
    {
        TemporaryRouteRemoved, RouteLocked, PendingLinkAdded, PendingLinkRemoved, FaceChanged, PhaseSelected
    }

    public sealed class CandidateStageFact
    {
        public CandidateStageFactKind Kind { get; }
        public string OperationId { get; }
        public BigInteger SceneRevision { get; }
        public string FaceId { get; }
        public BattlePairKey Pair { get; }
        public int SegmentIndex { get; }
        public string NextFaceId { get; }
        public BattlePhase? Phase { get; }

        internal CandidateStageFact(CandidateStageFactKind kind, BattleSnapshot before,
            CandidateStageSource source, BattlePairKey pair, int index, string nextFaceId, BattlePhase? phase)
        {
            Kind = kind; OperationId = source.OperationId; SceneRevision = before.SceneRevision;
            FaceId = before.Board.Face.FaceId; Pair = pair; SegmentIndex = index;
            NextFaceId = nextFaceId; Phase = phase;
        }
    }

    // A stage decision is not a post-action snapshot, a victory report or a commit capability.
    public sealed class CandidateStageDecision
    {
        public BattleSnapshot BeforeSnapshot { get; }
        public BattleEntryBaseline Baseline => BeforeSnapshot.Baseline;
        public CandidateStageSource Source { get; }
        public CandidateFinalHpValues FinalHp { get; }
        public BattleBoardState Board { get; }
        public int NextFaceIndex { get; }
        public BattlePhase NextPhase { get; }
        public bool DidFlipFace => NextFace != null;
        public PreparedFace NextFace { get; }
        public IReadOnlyList<CandidateStageFact> OrderedFacts { get; }

        internal CandidateStageDecision(BattleSnapshot before, CandidateStageSource source,
            CandidateFinalHpValues finalHp, BattleBoardState board, int nextFaceIndex, BattlePhase nextPhase,
            PreparedFace nextFace, IEnumerable<CandidateStageFact> facts)
        {
            BeforeSnapshot = before; Source = source; FinalHp = finalHp; Board = board;
            NextFaceIndex = nextFaceIndex; NextPhase = nextPhase; NextFace = nextFace;
            OrderedFacts = new List<CandidateStageFact>(facts).AsReadOnly();
        }
    }

    public enum CandidateStageRejectionCode
    {
        MissingField, InvalidValue, InconsistentBinding, StaleContext, InvalidPhase,
        ActorUnavailable, TargetUnavailable, NotPendingLink, InvalidRoute
    }

    public sealed class CandidateStageResult
    {
        public bool IsAccepted => Decision != null;
        public CandidateStageDecision Decision { get; }
        public CandidateStageRejectionCode? RejectionCode { get; }
        public string FieldPath { get; }
        public string RouteReasonCode { get; }
        public int? RouteCellIndex { get; }

        internal CandidateStageResult(CandidateStageDecision decision) { Decision = decision; }
        internal CandidateStageResult(CandidateStageRejectionCode code, string path, RouteValidationResult route = null)
        {
            RejectionCode = code; FieldPath = path;
            RouteReasonCode = route?.ReasonCode; RouteCellIndex = route?.CellIndex;
        }
    }
}
