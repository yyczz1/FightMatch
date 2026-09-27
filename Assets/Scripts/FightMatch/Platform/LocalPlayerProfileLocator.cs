using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;

namespace FightMatch.Platform
{
    public enum LocalPlayerProfileState { Absent, CreateIntentRecorded, Active }
    public sealed class LocalPlayerProfileRef
    {
        public string PlayerId => CreateRecord.PlayerId;
        public PlayerProfileCreateRecord CreateRecord { get; }
        public string OriginalInitializationCommitId { get; }
        internal LocalPlayerProfileRef(PlayerProfileCreateRecord record, string commit)
        { CreateRecord = record; OriginalInitializationCommitId = commit; }
    }
    public sealed class LocalPlayerProfileObservation
    {
        public LocalPlayerProfileLocator Owner { get; }
        public string PhysicalIdentity { get; }
        public LocalPlayerProfileState State { get; }
        public PlayerProfileCreateRecord CreateRecord { get; }
        public LocalPlayerProfileRef ActiveProfile { get; }
        internal LocalPlayerProfileObservation(LocalPlayerProfileLocator owner, PlayerProfileCreateRecord record, string commit, string identity)
        {
            Owner = owner; CreateRecord = record; PhysicalIdentity = identity;
            State = commit != null ? LocalPlayerProfileState.Active : record == null ? LocalPlayerProfileState.Absent : LocalPlayerProfileState.CreateIntentRecorded;
            ActiveProfile = commit == null ? null : new LocalPlayerProfileRef(record, commit);
        }
    }
    public sealed class LocalPlayerProfileResult
    {
        public bool IsAccepted => Diagnostic == null;
        public string Code { get; }
        public LocalSaveDiagnostic Diagnostic { get; }
        public string LimitReason { get; }
        public ulong? RequiredAtLeast { get; }
        public ulong? Allowed { get; }
        public LocalPlayerProfileObservation Observation { get; }
        internal LocalPlayerProfileResult(string code, LocalPlayerProfileObservation observation = null, LocalSaveFailure failure = null)
        { Code = code; Observation = observation; Diagnostic = failure == null ? null : new LocalSaveDiagnostic(failure);
            LimitReason = failure?.Reason; RequiredAtLeast = failure?.Required; Allowed = failure?.Allowed; }
    }

    // The host supplies a dedicated immutable-record store. These keys never identify player save heads.
    public sealed class LocalPlayerProfileLocator
    {
        public static string CreateRecordKey => Key("fightmatch.player-profile.create.v1");
        public static string ActiveRecordKey => Key("fightmatch.player-profile.active.v1");
        private readonly IContentPublicationStorage storage;
        private bool active;
        public LocalPlayerProfileLocator(IContentPublicationStorage storage)
        { this.storage = storage ?? throw new ArgumentNullException(nameof(storage)); }
        public LocalPlayerProfileResult Read(SaveStoreBudget budget)
        { return Execute(budget, "Read", () => Result(ReadCore(budget))); }
        public LocalPlayerProfileResult RecordCreateIntent(PlayerProfileCreateRecord record, SaveStoreBudget budget)
        {
            return Execute(budget, "RecordCreateIntent", () =>
            {
                var bytes = LocalSaveFailure.Core(PlayerProfileCreateRecordCodec.Write(record, budget.Codec));
                var before = ReadCore(budget);
                if (before.CreateRecord != null)
                {
                    if (before.CreateRecord.RecordSha256 == record.RecordSha256)
                        return new LocalPlayerProfileResult(before.ActiveProfile == null ? "AlreadyRecorded" : "AlreadyActive", before);
                    LocalSaveFailure.Need(before.CreateRecord.PlayerId != record.PlayerId && before.CreateRecord.OperationId != record.OperationId,
                        "CreateIntentConflict", "CreateRecord.Identity");
                    throw new LocalSaveFailure("CreationPending", "CreateRecord");
                }
                try
                {
                    storage.WriteImmutable(CreateRecordKey, bytes, Maximum(budget));
                    var after = ReadCore(budget);
                    LocalSaveFailure.Need(after.CreateRecord?.RecordSha256 == record.RecordSha256, "CreateIntentWriteUnknown", "CreateRecord.ReadBack");
                    return new LocalPlayerProfileResult("Recorded", after);
                }
                catch (Exception error) when (Expected(error)) { throw new LocalSaveFailure("CreateIntentWriteUnknown", "CreateRecord.Write", cause: error); }
            });
        }
        public LocalPlayerProfileResult ConfirmCommit(LocalPlayerProfileObservation expected, PlayerProfileInitializationCommit proof, SaveStoreBudget budget)
        {
            return Execute(budget, "ConfirmCommit", () =>
            {
                LocalSaveFailure.Need(expected != null && ReferenceEquals(expected.Owner, this), "StaleProfileObservation", "Profile.Owner");
                LocalSaveFailure.Need(proof != null, "MissingField", "Initialization.Proof");
                var before = ReadCore(budget); var r = before.CreateRecord;
                LocalSaveFailure.Need(r != null && r.RecordSha256 == proof.RecordSha256 && r.PlayerId == proof.PlayerId &&
                    r.OperationId == proof.OperationId && r.ContentBinding.Same(proof.ContentBinding) &&
                    r.NewProfileDefinitionId == proof.NewProfileDefinitionId && r.NewProfileDefinitionVersion == proof.NewProfileDefinitionVersion &&
                    r.IntentSha256 == proof.IntentSha256 && proof.ObservedHeadDescriptor.Purpose == SavePurpose.PlayerSave &&
                    proof.ObservedHeadDescriptor.PlayerId == r.PlayerId, "InconsistentCreateIntent", "Initialization.Proof");
                if (before.ActiveProfile != null)
                {
                    LocalSaveFailure.Need(before.ActiveProfile.OriginalInitializationCommitId == proof.OriginalInitializationCommitId,
                        "CreateIntentConflict", "Initialization.CommitId");
                    return new LocalPlayerProfileResult("AlreadyActive", before);
                }
                LocalSaveFailure.Need(before.PhysicalIdentity == expected.PhysicalIdentity, "StaleProfileObservation", "Profile.PhysicalIdentity");
                var bytes = ActiveBytes(r.RecordSha256, proof.OriginalInitializationCommitId, budget);
                try
                {
                    storage.WriteImmutable(ActiveRecordKey, bytes, Maximum(budget));
                    var after = ReadCore(budget);
                    LocalSaveFailure.Need(after.ActiveProfile?.OriginalInitializationCommitId == proof.OriginalInitializationCommitId,
                        "ConfirmationUnknown", "Profile.ReadBack");
                    return new LocalPlayerProfileResult("Active", after);
                }
                catch (Exception error) when (Expected(error)) { throw new LocalSaveFailure("ConfirmationUnknown", "Profile.Confirm", cause: error); }
            });
        }
        private LocalPlayerProfileObservation ReadCore(SaveStoreBudget budget)
        {
            var bytes = storage.Read(CreateRecordKey, Maximum(budget)); var confirmation = storage.Read(ActiveRecordKey, Maximum(budget));
            LocalSaveFailure.Need(bytes != null || confirmation == null, "CorruptCreateRecord", "Profile.OrphanConfirmation");
            var record = bytes == null ? null : LocalSaveFailure.Core(PlayerProfileCreateRecordCodec.Read(bytes, budget.Codec));
            string commit = null;
            if (confirmation != null)
            {
                LocalSaveFailure.Limit((ulong)confirmation.Length, budget.Codec.MaxEnvelopeBytes, "Profile.Confirmation", "EnvelopeBytes");
                LocalSaveFailure.Need(confirmation.Length == 105, "CorruptCreateRecord", "Profile.Confirmation.Length");
                var text = Encoding.ASCII.GetString(confirmation); commit = text.Substring(72, 32);
                LocalSaveFailure.Need(SaveFileNames.Commit(commit) && ContentPublicationStorage.Equal(confirmation, ActiveBytes(record.RecordSha256, commit, budget)),
                    "CorruptCreateRecord", "Profile.Confirmation.Identity");
            }
            var identity = (bytes == null ? "absent" : Hash(bytes)) + ":" + (confirmation == null ? "absent" : Hash(confirmation));
            return new LocalPlayerProfileObservation(this, record, commit, identity);
        }
        private static byte[] ActiveBytes(string recordSha, string commit, SaveStoreBudget budget)
        {
            LocalSaveFailure.Need(SaveFileNames.Commit(commit), "InconsistentCreateIntent", "Initialization.CommitId");
            LocalSaveFailure.Limit(105, budget.Codec.MaxEnvelopeBytes, "Profile.Confirmation", "EnvelopeBytes");
            LocalSaveFailure.Limit(64, (ulong)budget.Codec.MaxStringCodeUnits, "Profile.RecordSha256", "StringCodeUnits");
            return Encoding.ASCII.GetBytes("FMAP01\n" + recordSha + "\n" + commit + "\n");
        }
        private LocalPlayerProfileResult Execute(SaveStoreBudget budget, string stage, Func<LocalPlayerProfileResult> action)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (active) return new LocalPlayerProfileResult("Busy", failure: new LocalSaveFailure("Busy", "Profile"));
            active = true;
            try { using (storage.AcquireWriter()) return action(); }
            catch (Exception error) when (Expected(error))
            {
                var failure = error as LocalSaveFailure ?? new LocalSaveFailure(error is ContentStorageException content ? content.Code : "ProfileReadUnknown",
                    "Profile." + stage, cause: error, stage: stage);
                return new LocalPlayerProfileResult(failure.Code, failure: failure);
            }
            finally { active = false; }
        }
        private static bool Expected(Exception error) => LocalSaveFailure.Expected(error);
        private static int Maximum(SaveStoreBudget budget) => (int)Math.Min((ulong)int.MaxValue, budget.Codec.MaxEnvelopeBytes);
        private static LocalPlayerProfileResult Result(LocalPlayerProfileObservation observation)
        { return new LocalPlayerProfileResult(observation.State.ToString(), observation); }
        private static string Key(string text) => Hash(Encoding.UTF8.GetBytes(text));
        private static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
    }
}
