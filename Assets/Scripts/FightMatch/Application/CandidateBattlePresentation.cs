using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;

namespace FightMatch.Application
{
    public enum CandidatePresentationDisposition { None, PlayOriginal, RebuildLatest }

    public sealed class CandidatePresentationToken
    {
        public string AttemptId { get; }
        public BigInteger SceneRevision { get; }
        public string OperationId { get; }
        internal CandidatePresentationToken(string attempt, BigInteger revision, string operation)
        { AttemptId = attempt; SceneRevision = revision; OperationId = operation; }

        internal bool Matches(CandidatePresentationToken other)
        {
            return other != null && AttemptId == other.AttemptId && SceneRevision == other.SceneRevision &&
                OperationId == other.OperationId;
        }
    }

    public sealed class CandidateBattlePresentation
    {
        public string OriginalCommitId { get; }
        public BattleSnapshot BeforeSnapshot { get; }
        public BattleSnapshot AfterSnapshot { get; }
        public IReadOnlyList<CandidateBattleOrderedFact> OrderedFacts { get; }
        public CandidatePresentationToken Token { get; }
        internal CandidateBattlePresentation(string commit, CandidateBattleOperationRecord record, CandidatePresentationToken token)
        {
            OriginalCommitId = commit; BeforeSnapshot = record.BeforeSnapshot; AfterSnapshot = record.AfterSnapshot;
            OrderedFacts = record.OrderedFacts; Token = token;
        }
    }
}
