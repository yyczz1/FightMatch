using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    // Explicit candidate inputs; the caller must not mutate them during assembly.
    public sealed class CandidateRandomInitials
    {
        public Pcg32StreamState Battle { get; set; }
        public Pcg32StreamState BaseReward { get; set; }
        public Pcg32StreamState Bonus { get; set; }
        public List<CandidatePrdInitial> PrdStates { get; set; }
    }

    public sealed class CandidatePrdInitial
    {
        public string CharacterId { get; set; }
        public string PassiveDefinitionId { get; set; }
        public BigInteger? FailureCount { get; set; }
    }
}
