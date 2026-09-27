using System;
using System.Collections.Generic;
using FightMatch.Core;

namespace FightMatch.Application
{
    public sealed class CandidateCritCoefficient
    {
        public ExactRational TargetProbability { get; }
        public ExactRational C { get; }
        public CandidateCritCoefficient(ExactRational targetProbability, ExactRational c)
        { TargetProbability = targetProbability; C = c; }
    }

    // Definitions are already immutable Core values. Caller-owned lists are frozen by Prepare.
    public sealed class CandidateLifecycleContentInput
    {
        private CandidatePermanentDefinitions permanent;
        public CandidatePermanentDefinitions GetPermanentDefinitions() { return permanent; }
        public void SetPermanentDefinitions(CandidatePermanentDefinitions value) { permanent = value; }
        public CandidateGrowthDefinition Growth { get; set; }
        public IReadOnlyList<CandidateGrowthDefinition> Growths { get; set; }
        public CandidateInventoryDefinition Inventory { get; set; }
        public CandidateProgressionDefinition Progression { get; set; }
        public IReadOnlyList<PreparedLevel> Levels { get; set; }
        public IReadOnlyList<CandidateRewardDefinition> Rewards { get; set; }
        public IReadOnlyList<CandidateCritCoefficient> CritCoefficients { get; set; }
    }

    public sealed class CandidateLifecycleContent
    {
        private readonly CandidatePermanentDefinitions permanent;
        public CandidatePermanentDefinitions GetPermanentDefinitions() { return permanent; }
        public IReadOnlyList<CandidateGrowthDefinition> Growths { get; }
        public CandidateGrowthDefinition Growth => Growths.Count == 1 ? Growths[0] : throw new InvalidOperationException("AmbiguousCharacter");
        public CandidateGrowthDefinition FindGrowth(string classId)
        { foreach (var growth in Growths) if (growth.ClassId == classId) return growth; return null; }
        public bool TryGetSingle(out CandidateGrowthDefinition growth)
        { growth = Growths.Count == 1 ? Growths[0] : null; return growth != null; }
        public CandidateInventoryDefinition Inventory { get; }
        public CandidateProgressionDefinition Progression { get; }
        public PreparedRuleContext Context => Inventory.Context;
        public IReadOnlyList<PreparedLevel> Levels { get; }
        public IReadOnlyList<CandidateRewardDefinition> Rewards { get; }
        public IReadOnlyList<CandidateCritCoefficient> CritCoefficients { get; }
        internal CandidateLifecycleContent(CandidateLifecycleContentInput input)
        {
            permanent = input.GetPermanentDefinitions();
            Growths = new List<CandidateGrowthDefinition>(input.Growths ?? new[] { input.Growth }).AsReadOnly(); Inventory = input.Inventory; Progression = input.Progression;
            Levels = new List<PreparedLevel>(input.Levels).AsReadOnly();
            Rewards = new List<CandidateRewardDefinition>(input.Rewards).AsReadOnly();
            CritCoefficients = new List<CandidateCritCoefficient>(input.CritCoefficients).AsReadOnly();
        }
    }

    public sealed class CandidateLifecycleDraft
    {
        public string PlayerId { get; set; }
        public string ExpectedCommitId { get; set; }
        public CandidateApplicationKind? Kind { get; set; }
        public CandidateLifecycleContentInput Content { get; set; }
        public CandidateApplicationEnterInput EnterAttempt { get; set; }
        public CandidateApplicationEndInput ExitAttempt { get; set; }
        public CandidateApplicationEndInput RestartAttempt { get; set; }
        public CandidateApplicationRecoveryInput AdvanceRecovery { get; set; }
        public CandidateTimeSample EndTimeSample { get; set; }
    }

    public sealed class PreparedCandidateLifecycleRequest
    {
        public PreparedCandidateApplicationIntent Intent { get; }
        public CandidateApplicationKind Kind => Intent.Kind;
        public string OperationId => Intent.OperationId;
        public string PlayerId => Intent.PlayerId;
        public CandidateLifecycleContent Content { get; }
        public IReadOnlyList<string> FormationPreview => Input.SetFormation?.Slots ?? Input.EnterFormation?.Slots ?? Input.RosterInitialize?.Slots;
        internal CandidateApplicationIntentInput Input { get; }
        internal CandidateTimeSample EndTime { get; }
        internal PreparedCandidateLifecycleRequest(PreparedCandidateApplicationIntent intent,
            CandidateApplicationIntentInput input, CandidateLifecycleContent content, CandidateTimeSample time)
        { Intent = intent; Input = input; Content = content; EndTime = time; }
    }

    public sealed class CandidateLifecyclePrepareResult
    {
        public bool IsAccepted => Request != null;
        public PreparedCandidateLifecycleRequest Request { get; }
        public CandidateApplicationDiagnostic Diagnostic { get; }
        public string Code => IsAccepted ? "Prepared" : Diagnostic.Code;
        internal CandidateLifecyclePrepareResult(PreparedCandidateLifecycleRequest request, CandidateApplicationDiagnostic diagnostic = null)
        { Request = request; Diagnostic = diagnostic; }
    }
}
