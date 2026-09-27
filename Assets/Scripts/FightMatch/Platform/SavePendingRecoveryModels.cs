using System.Collections.Generic;
using System.Linq;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public sealed class SaveRecoveredCandidate
    {
        public SaveEnvelope Envelope { get; }
        public SaveCommitMetadata Metadata { get; }
        public SnapshotDescriptor Descriptor { get; }
        public SnapshotDescriptor ExpectedHead { get; }
        public IReadOnlyList<string> OperationIds { get; }
        internal LocalSaveStore Owner { get; }
        internal SaveRecoveryView Observation { get; }

        internal SaveRecoveredCandidate(LocalSaveStore owner, SaveRecoveryView observation,
            SaveEnvelope envelope, SnapshotDescriptor descriptor)
        {
            Owner = owner;
            Observation = observation;
            Envelope = envelope;
            Metadata = new SaveCommitMetadata(envelope.PlayerId, envelope.Purpose, envelope.SaveGeneration,
                envelope.CommitId, envelope.ParentCommitId, envelope.CommitIndex);
            Descriptor = LocalSaveValues.Freeze(descriptor);
            ExpectedHead = LocalSaveValues.Freeze(observation.Current?.Descriptor);
            OperationIds = System.Array.AsReadOnly(Metadata.CommitIndex.Last().OperationIds.ToArray());
        }
    }

    public sealed class SaveCandidateEndResult
    {
        public string CommitId { get; }
        public SnapshotDescriptor CurrentHead { get; }
        public IReadOnlyList<string> RemovedNames { get; }

        internal SaveCandidateEndResult(string commitId, SnapshotDescriptor currentHead, IEnumerable<string> removedNames)
        {
            CommitId = commitId;
            CurrentHead = LocalSaveValues.Freeze(currentHead);
            RemovedNames = System.Array.AsReadOnly(removedNames.ToArray());
        }
    }
}
