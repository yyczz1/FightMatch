namespace FightMatch.Core
{
    // Supplied candidate material; no claim that the named capability is a verified platform source.
    public sealed class CandidateSeedMaterial
    {
        public byte[] Bytes { get; set; }
        public string SourceCapabilityId { get; set; }
        public string MappingId { get; set; }
    }
}
