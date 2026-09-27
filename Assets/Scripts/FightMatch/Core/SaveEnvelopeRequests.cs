using System;
using System.Collections.Generic;
using System.Numerics;

namespace FightMatch.Core
{
    public sealed class SaveEnvelopeInput
    {
        public SavePurpose? Purpose { get; set; }
        public string PlayerId { get; set; }
        public BigInteger? SaveGeneration { get; set; }
        public string CommitId { get; set; }
        public string ParentCommitId { get; set; }
        public IReadOnlyList<RequiredSliceContract> RequiredSliceContracts { get; set; }
        public IReadOnlyList<SaveSliceInput> Slices { get; set; }
        public IReadOnlyList<SaveCommitIndexEntry> CommitIndex { get; set; }
    }

    public sealed class SaveSliceInput
    {
        public RequiredSliceContract Contract { get; set; }
        public byte[] Bytes { get; set; }
        public SaveRequirements Requirements { get; set; }
    }

    // Limits apply to one complete call, including every nested field; Math is never replaced.
    // Byte limits bound encoded work, not total managed memory or gameplay progress.
    public sealed class SaveCodecBudget
    {
        public ExactMathBudget Math { get; }
        public ulong MaxEnvelopeBytes { get; }
        public ulong MaxMetadataBytes { get; }
        public int MaxCollectionEntries { get; }
        public int MaxStringCodeUnits { get; }
        public int MaxNumericTokenBytes { get; }
        public SaveCodecBudget(ExactMathBudget math, ulong maxEnvelopeBytes = 64UL * 1024 * 1024,
            ulong maxMetadataBytes = 4UL * 1024 * 1024, int maxCollectionEntries = 65536,
            int maxStringCodeUnits = 65536, int maxNumericTokenBytes = 4096)
        {
            Math = math ?? throw new ArgumentNullException(nameof(math));
            if (maxCollectionEntries < 0 || maxStringCodeUnits < 0 || maxNumericTokenBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(maxCollectionEntries));
            MaxEnvelopeBytes = maxEnvelopeBytes; MaxMetadataBytes = maxMetadataBytes;
            MaxCollectionEntries = maxCollectionEntries; MaxStringCodeUnits = maxStringCodeUnits;
            MaxNumericTokenBytes = maxNumericTokenBytes;
        }
    }

    public sealed class SaveCodecResult<T>
    {
        public bool IsAccepted { get; }
        public T Value { get; }
        public string RejectionCode { get; }
        public string FieldPath { get; }
        public string LimitReason { get; }
        public ulong? RequiredAtLeast { get; }
        public ulong? Allowed { get; }
        private SaveCodecResult(bool accepted, T value, string code, string path,
            string reason, ulong? required, ulong? allowed)
        {
            IsAccepted = accepted; Value = value; RejectionCode = code; FieldPath = path;
            LimitReason = reason; RequiredAtLeast = required; Allowed = allowed;
        }
        internal static SaveCodecResult<T> Accept(T value)
        { return new SaveCodecResult<T>(true, value, null, null, null, null, null); }
        internal static SaveCodecResult<T> Run(Func<T> call)
        {
            try { return Accept(call()); }
            catch (SaveCodecFailure error)
            { return new SaveCodecResult<T>(false, default(T), error.Code, error.Path, error.Reason, error.Required, error.Allowed); }
            catch (ExactMathLimitException error)
            {
                return new SaveCodecResult<T>(false, default(T), "Limit", "Budget.Math", error.ReasonCode,
                    (ulong)error.RequiredAtLeast, (ulong)error.Allowed);
            }
        }
    }

    internal sealed class SaveCodecFailure : Exception
    {
        internal string Code { get; }
        internal string Path { get; }
        internal string Reason { get; }
        internal ulong? Required { get; }
        internal ulong? Allowed { get; }
        internal SaveCodecFailure(string code, string path, string reason = null, ulong? required = null, ulong? allowed = null)
        { Code = code; Path = path; Reason = reason; Required = required; Allowed = allowed; }
        internal static void Require(bool condition, string code, string path)
        { if (!condition) throw new SaveCodecFailure(code, path); }
        internal static void Limit(ulong required, ulong allowed, string path, string reason)
        { if (required > allowed) throw new SaveCodecFailure("Limit", path, reason, required, allowed); }
    }
}
