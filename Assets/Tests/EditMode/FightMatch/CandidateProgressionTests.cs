using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using static FightMatch.Core.CandidateProgressionRejectionCode;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateProgressionTests
    {
        [Test]
        public void NewCandidateOpensOnlyExplicitInitialNodeAndQueriesNeverCreateRelations()
        {
            var state = New(); var before = Describe(state); var view = CandidateProgression.Read(state, new ExactMathBudget());
            CollectionAssert.AreEqual(new[] { "P-A", "P-B", "P-C" }, view.Levels.Select(x => x.Level.LevelId));
            Assert.AreEqual(BigInteger.One, state.StateRevision); Assert.AreEqual(1, state.OpenFacts.Count);
            Assert.AreEqual("open-A", view.Levels[0].OpenFact.Level.UnlockRuleId); Assert.IsNull(view.Levels[0].OpenFact.SourceClear);
            Assert.IsNull(view.Levels[1].OpenFact); Assert.IsNull(view.Levels[2].OpenFact); Assert.IsEmpty(state.FirstClears);
            var allowed = Check(state, Intent()); Assert.IsTrue(allowed.IsAccepted); Assert.AreEqual(2, allowed.Participant.OriginalSlot);
            Assert.AreEqual(BigInteger.One, allowed.Participant.CharacterRevision); Assert.IsNull(allowed.OpenChallenge);
            var locked = Check(state, Intent("P-B")); Assert.AreEqual(Locked, locked.RejectionCode); Assert.IsNull(locked.Participant);
            Reject(Enter(state, Intent("P-B"), accepted: false), Locked, "LevelId");
            Assert.AreEqual(before, Describe(state)); Assert.IsEmpty(state.Challenges); Assert.IsNull(state.ActiveAttempt);
            // These isolated definitions carry no downloaded/published resource or H10 execution proof.
            Assert.IsNull(typeof(CandidateProgressionEntryCheck).GetProperty("IsExecutable"));
            Assert.IsNull(typeof(CandidateProgressionResult).GetProperty("Completed"));
        }

        [TestCaseSource(nameof(BadDefinitions))]
        public void DefinitionRejectsMissingUnsupportedAndConflictingFields(string field, object value, CandidateProgressionRejectionCode code)
        {
            var input = DefinitionInput(); Set(input, field, value); var result = CandidateProgression.PrepareDefinition(input, new ExactMathBudget());
            Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Definition); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(field, result.FieldPath);
        }
        private static IEnumerable<TestCaseData> BadDefinitions()
        {
            foreach (var field in new[] { "Context", "Context.DraftId", "Context.ContentFingerprint", "Context.RuleVersion", "Context.NumericContractVersion",
                "Context.RandomContractVersion", "Context.SourceNotes", "Levels", "Levels[0]", "Levels[0].LevelId", "Levels[0].LevelVersion", "Levels[0].UnlockRuleId", "Levels[0].RequiredFeatures" })
                yield return new TestCaseData(field, null, MissingField);
            yield return new TestCaseData("Context.DraftRevision", BigInteger.Zero, InvalidValue);
            yield return new TestCaseData("Context.SourceNotes", new List<string>(), InvalidValue);
            yield return new TestCaseData("Context.SourceNotes[0]", " ", MissingField);
            yield return new TestCaseData("Levels", new List<CandidateProgressionLevelInput>(), InvalidValue);
            yield return new TestCaseData("Levels[0].EntryKind", CandidateProgressionEntryKind.Unspecified, MissingField);
            yield return new TestCaseData("Levels[0].EntryKind", (CandidateProgressionEntryKind)9, UnsupportedBinding);
            yield return new TestCaseData("Levels[0].UnlockKind", CandidateProgressionUnlockKind.Unspecified, MissingField);
            yield return new TestCaseData("Levels[0].UnlockKind", (CandidateProgressionUnlockKind)9, UnsupportedBinding);
            yield return new TestCaseData("Levels[0].RequiredFeatures", new List<string> { "Tutorial16" }, UnsupportedBinding);
            yield return new TestCaseData("Levels[0].UnlockAfterLevelId", "P-C", InvalidValue);
            yield return new TestCaseData("Levels[1].UnlockAfterLevelId", null, MissingField);
            yield return new TestCaseData("Levels[1].UnlockAfterLevelId", "P-B", InvalidValue);
            yield return new TestCaseData("Levels[1].UnlockAfterLevelId", "missing", InconsistentBinding);
            yield return new TestCaseData("Levels[1].LevelId", "P-A", InvalidValue);
            yield return new TestCaseData("Levels[1].UnlockRuleId", "open-A", InvalidValue);
        }

        [Test]
        public void PrerequisiteMustBeExplicitAndGraphMustHaveInitialNodeWithoutCycles()
        {
            var input = DefinitionInput(); input.Levels[0] = new CandidateProgressionLevelInput { LevelId = "P-A", LevelVersion = "v1",
                UnlockRuleId = "open-A", EntryKind = CandidateProgressionEntryKind.Ordinary, UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen, RequiredFeatures = new List<string>() };
            var missing = CandidateProgression.PrepareDefinition(input, new ExactMathBudget()); Assert.AreEqual(MissingField, missing.RejectionCode);
            Assert.AreEqual("Levels[0].UnlockAfterLevelId", missing.FieldPath); input.Levels[0].UnlockAfterLevelId = null; Assert.IsTrue(Definition(input) != null);
            input.Levels[1].UnlockAfterLevelId = "P-C";
            var cycle = CandidateProgression.PrepareDefinition(input, new ExactMathBudget()); Assert.AreEqual(InvalidValue, cycle.RejectionCode);
            input.Levels[0].UnlockKind = CandidateProgressionUnlockKind.AfterWholeLevelClear; input.Levels[0].UnlockAfterLevelId = "P-C";
            var noInitial = CandidateProgression.PrepareDefinition(input, new ExactMathBudget()); Assert.AreEqual(InvalidValue, noInitial.RejectionCode); Assert.AreEqual("Levels", noInitial.FieldPath);
        }

        [Test]
        public void DefinitionOrderAndOnlyExplicitImmediateSuccessorsArePreserved()
        {
            var input = DefinitionInput(); input.Levels.Insert(1, Node("P-D", "open-D", "P-A")); input.Levels.Reverse();
            var state = New(Definition(input)); var started = Enter(state, Intent()); var win = Finish(started.Next, End(started.BeginReceipt));
            CollectionAssert.AreEqual(input.Levels.Select(x => x.LevelId), CandidateProgression.Read(win.Next, new ExactMathBudget()).Levels.Select(x => x.Level.LevelId));
            CollectionAssert.AreEqual(new[] { "P-B", "P-D" }, win.NewOpenFacts.Select(x => x.Level.LevelId));
            CollectionAssert.AreEqual(new[] { "P-B", "P-D" }, win.Successors.Select(x => x.Level.LevelId));
            Assert.IsNull(View(win.Next, "P-C").OpenFact); Assert.IsFalse(Check(win.Next, Intent("P-C")).IsAccepted);
            foreach (var fact in win.NewOpenFacts) { Assert.AreEqual("v1", fact.Level.LevelVersion); Assert.AreSame(win.NewFirstClear, fact.SourceClear); }
        }

        [Test]
        public void ReadyBasisComesFromPublicM03AndRecoveryCompletionAllowsNewEntry()
        {
            var state = New(); var character = Character(); var old = Describe(character);
            var ready = Enter(state, Intent(character: character), character); Assert.AreEqual("warrior", ready.BeginReceipt.Participant.CharacterId);
            Assert.AreEqual(2, ready.BeginReceipt.Participant.OriginalSlot); Assert.AreEqual(BigInteger.One, character.Level); Assert.AreEqual(old, Describe(character));
            var down = Down(character); Assert.IsFalse(down.IsReady);
            var check = Check(state, Intent(character: down), down); Assert.AreEqual(NoReadyMember, check.RejectionCode);
            Reject(Enter(state, Intent(character: down), down, false), NoReadyMember, "Character.IsReady");
            var recovered = CandidateRecoveryClock.Advance(down, "recovery", Time(10), down.StateRevision, new ExactMathBudget());
            Assert.IsTrue(recovered.IsAccepted); Assert.IsTrue(recovered.Next.IsReady);
            Assert.IsTrue(Enter(state, Intent(character: recovered.Next), recovered.Next).IsAccepted);
            Assert.IsEmpty(state.Challenges); Assert.IsFalse(down.IsReady);
        }

        [TestCase("PlayerId", "wrong", InconsistentBinding)]
        [TestCase("LevelId", "p-a", UnsupportedBinding)]
        [TestCase("LevelVersion", "v2", InconsistentBinding)]
        [TestCase("CharacterId", "other", InconsistentBinding)]
        [TestCase("ExpectedOriginalSlot", 1, InconsistentBinding)]
        [TestCase("ExpectedOriginalSlot", -1, InvalidValue)]
        [TestCase("ExpectedOriginalSlot", 3, InvalidValue)]
        public void EntryIdentityNeverSilentlySwitchesRequestedBasis(string field, object value, CandidateProgressionRejectionCode code)
        {
            var input = Intent(); Set(input, field, value); var state = New(); var character = Character();
            var check = Check(state, input, character); Assert.IsFalse(check.IsAccepted); Assert.AreEqual(code, check.RejectionCode); Assert.AreEqual(field, check.FieldPath);
            Reject(Enter(state, input, character, false), code, field); Assert.IsEmpty(state.Challenges);
        }

        [TestCase("PlayerId")]
        [TestCase("LevelId")]
        [TestCase("LevelVersion")]
        [TestCase("Context")]
        [TestCase("CharacterId")]
        [TestCase("ExpectedCharacterRevision")]
        [TestCase("ExpectedOriginalSlot")]
        public void EntryRequiresEveryField(string field)
        {
            var input = Intent(); Set(input, field, null); var check = Check(New(), input);
            Assert.IsFalse(check.IsAccepted); Assert.AreEqual(MissingField, check.RejectionCode); Assert.AreEqual(field, check.FieldPath);
            Reject(Enter(New(), input, accepted: false), MissingField, field);
        }

        [Test]
        public void CurrentM03PlayerContextRevisionAndSlotAreVerifiedWithExactStrings()
        {
            var state = New(); var input = Intent(); var wrongPlayer = Character(player: "Player");
            Reject(Enter(state, input, wrongPlayer, false), InconsistentBinding, "Character.PlayerId");
            var context = Context(); context.ContentFingerprint = "different";
            Reject(Enter(state, input, Character(context: context), false), InconsistentBinding, "Character.Context.ContentFingerprint");
            input.ExpectedCharacterRevision = 0; Reject(Enter(state, input, accepted: false), StaleContext, "ExpectedCharacterRevision");
            input.ExpectedCharacterRevision = -1; Reject(Enter(state, input, accepted: false), InvalidValue, "ExpectedCharacterRevision");
            input = Intent(); input.Context.SourceNotes[0] += " "; Reject(Enter(state, input, accepted: false), InconsistentBinding, "Context.SourceNotes[0]");
            var zeroSlot = Character(slot: 0); Assert.AreEqual(0, Enter(state, Intent(character: zeroSlot), zeroSlot).BeginReceipt.Participant.OriginalSlot);
        }

        [Test]
        public void ExitKeepsChallengeAndReentryUsesNewAttemptButOriginalChallenge()
        {
            var first = Enter(New(), Intent()); var exit = Finish(first.Next, End(first.BeginReceipt, CandidateProgressionEndKind.NormalExit));
            Assert.IsNull(exit.Next.ActiveAttempt); Assert.IsFalse(exit.Next.Challenges[0].IsClosed); Assert.IsEmpty(exit.Next.FirstClears);
            Assert.AreEqual(1, exit.Next.OpenFacts.Count); Assert.IsEmpty(exit.NewOpenFacts); Assert.IsNull(exit.NewFirstClear);
            var retry = Intent(challenge: "other", attempt: "b"); Reject(Enter(exit.Next, retry, accepted: false), ChallengeConflict, "ChallengeId");
            retry.ChallengeId = "c-a"; retry.EntryBaselineId = "baseline-b"; var second = Enter(exit.Next, retry);
            Assert.AreEqual(1, second.Next.Challenges.Count); Assert.AreEqual(2, second.Next.Challenges[0].Attempts.Count);
            Assert.AreEqual("baseline", first.BeginReceipt.EntryBaselineId); Assert.AreEqual("baseline-b", second.BeginReceipt.EntryBaselineId);
            Assert.AreSame(exit.EndReceipt, second.Next.Challenges[0].Attempts[0].End); Assert.AreEqual("b", second.Next.ActiveAttempt.Begin.AttemptId);
            Assert.AreEqual(exit.Next.StateRevision + 1, second.Next.StateRevision);
        }

        [Test]
        public void MultipleUnfinishedChallengesAreAllowedButOnlyOneAttemptCanBeActive()
        {
            var input = DefinitionInput(); input.Levels[1].UnlockKind = CandidateProgressionUnlockKind.InitiallyOpen; input.Levels[1].UnlockAfterLevelId = null;
            var first = Enter(New(Definition(input)), Intent("P-B", "c-b", "b"));
            Reject(Enter(first.Next, Intent(), accepted: false), ActiveAttemptConflict, "AttemptId");
            Assert.AreEqual(ActiveAttemptConflict, Check(first.Next, Intent()).RejectionCode);
            var state = Finish(first.Next, End(first.BeginReceipt, CandidateProgressionEndKind.NormalExit)).Next;
            var second = Enter(state, Intent()); state = Finish(second.Next, End(second.BeginReceipt, CandidateProgressionEndKind.NormalExit)).Next;
            Assert.AreEqual(2, state.Challenges.Count); Assert.IsTrue(state.Challenges.All(x => !x.IsClosed)); Assert.IsNull(state.ActiveAttempt);
            var view = CandidateProgression.Read(state, new ExactMathBudget());
            Assert.AreEqual("c-a", view.Levels[0].OpenChallenge.ChallengeId); Assert.AreEqual("c-b", view.Levels[1].OpenChallenge.ChallengeId);
            var next = Enter(state, Intent(attempt: "a2")); var won = Finish(next.Next, End(next.BeginReceipt));
            Assert.IsTrue(View(won.Next, "P-A").Challenges[0].IsClosed); Assert.IsFalse(View(won.Next, "P-B").Challenges[0].IsClosed);
        }

        [Test]
        public void WholeVictoryRetainsUniqueFirstClearAndOpeningSourceAcrossReplayAndNewChallenges()
        {
            var first = Enter(New(), Intent()); var facts = End(first.BeginReceipt); var won = Finish(first.Next, facts);
            Assert.IsTrue(won.EndReceipt.IsFirstClear); Assert.AreSame(won.EndReceipt, won.NewFirstClear.End);
            Assert.AreEqual("player", won.NewFirstClear.PlayerId); Assert.AreEqual("P-A", won.NewFirstClear.LevelId); Assert.AreEqual("v1", won.NewFirstClear.LevelVersion);
            Assert.AreSame(won.Next.Definition.Context, won.NewFirstClear.Context); Assert.IsTrue(won.Next.Challenges[0].IsClosed);
            Assert.AreEqual("P-B", won.NewOpenFacts[0].Level.LevelId); Assert.AreSame(won.NewFirstClear, won.NewOpenFacts[0].SourceClear);
            Assert.AreEqual("settlement-a", won.NewFirstClear.End.SettlementId); Assert.AreEqual("report-a", won.NewFirstClear.End.FinalReportFingerprint);
            Assert.IsNull(View(won.Next, "P-C").OpenFact); Assert.AreEqual(first.Next.StateRevision + 1, won.Next.StateRevision);
            var repeated = CandidateProgression.EndAttempt(won.Next, facts, 0, new ExactMathBudget());
            Assert.AreSame(won.Next, repeated.Next); Assert.AreSame(won.EndReceipt, repeated.EndReceipt); Assert.IsNull(repeated.NewFirstClear); Assert.IsEmpty(repeated.NewOpenFacts);
            Reject(Enter(won.Next, Intent(attempt: "replay"), accepted: false), ChallengeConflict, "ChallengeId");
            var replay = Enter(won.Next, Intent(challenge: "c-replay", attempt: "replay")); var again = Finish(replay.Next, End(replay.BeginReceipt));
            Assert.IsFalse(again.EndReceipt.IsFirstClear); Assert.IsNull(again.NewFirstClear); Assert.IsEmpty(again.NewOpenFacts);
            Assert.AreSame(won.NewFirstClear, again.Next.FirstClears[0]); Assert.AreSame(won.NewOpenFacts[0], View(again.Next, "P-B").OpenFact);
            Assert.AreEqual(1, again.Next.FirstClears.Count); Assert.AreEqual(2, again.Next.Challenges.Count);
            Assert.IsNull(View(again.Next, "P-C").OpenFact);
            var b = Enter(again.Next, Intent("P-B", "c-b", "b")); var bWon = Finish(b.Next, End(b.BeginReceipt));
            Assert.AreEqual("P-C", bWon.NewOpenFacts[0].Level.LevelId); Assert.AreEqual(2, bWon.Next.FirstClears.Count);
            Assert.AreSame(won.NewFirstClear, bWon.Next.FirstClears[0]);
        }

        [Test]
        public void RestartTransfersOriginalEntryBasisAndItsReceiptCannotCreateAnotherAttempt()
        {
            var character = Character(); var started = Enter(New(), Intent(), character); var facts = End(started.BeginReceipt, CandidateProgressionEndKind.ImmediateRestart);
            var result = Finish(started.Next, facts); var next = result.Next;
            Assert.AreEqual("next-a", next.ActiveAttempt.Begin.AttemptId); Assert.AreEqual("c-a", result.BeginReceipt.ChallengeId);
            Assert.AreEqual("baseline", result.BeginReceipt.EntryBaselineId); Assert.AreSame(started.BeginReceipt.Participant, result.BeginReceipt.Participant);
            Assert.AreEqual(started.Next.StateRevision + 1, next.StateRevision); Assert.IsFalse(next.Challenges[0].IsClosed);
            Assert.AreEqual(2, next.Challenges[0].Attempts.Count); Assert.AreSame(result.EndReceipt, next.Challenges[0].Attempts[0].End);
            Assert.IsEmpty(next.FirstClears); Assert.IsEmpty(result.NewOpenFacts); Assert.IsEmpty(result.Successors);
            var duplicate = CandidateProgression.EndAttempt(next, facts, 0, new ExactMathBudget());
            Assert.AreSame(next, duplicate.Next); Assert.AreSame(result.EndReceipt, duplicate.EndReceipt); Assert.IsNull(duplicate.BeginReceipt);
            var down = Down(character); var beginAgain = Intent(attempt: "next-a");
            var original = CandidateProgression.BeginAttempt(next, beginAgain, down, 0, new ExactMathBudget());
            Assert.AreSame(next, original.Next); Assert.AreSame(result.BeginReceipt, original.BeginReceipt);
            facts.NewAttemptId = "another"; Reject(CandidateProgression.EndAttempt(next, facts, 0, new ExactMathBudget()), InconsistentBinding, "NewAttemptId");
            var exit = Finish(next, End(result.BeginReceipt, CandidateProgressionEndKind.NormalExit));
            facts.NewAttemptId = "next-a"; Assert.AreSame(exit.Next, CandidateProgression.EndAttempt(exit.Next, facts, 0, new ExactMathBudget()).Next);
        }

        [Test]
        public void RestartCannotReuseAnyOldOrCurrentAttemptIdentity()
        {
            var first = Enter(New(), Intent()); var exit = Finish(first.Next, End(first.BeginReceipt, CandidateProgressionEndKind.NormalExit));
            var second = Enter(exit.Next, Intent(attempt: "b")); var restart = End(second.BeginReceipt, CandidateProgressionEndKind.ImmediateRestart);
            restart.NewAttemptId = "a"; Reject(Finish(second.Next, restart, false), InconsistentBinding, "NewAttemptId");
            restart.NewAttemptId = "b"; Reject(Finish(second.Next, restart, false), InconsistentBinding, "NewAttemptId");
            Assert.AreEqual("b", second.Next.ActiveAttempt.Begin.AttemptId); Assert.AreEqual(2, second.Next.Challenges[0].Attempts.Count);
        }

        [Test]
        public void ExactBeginRetryAfterEndAndM03RecoveryReturnsOriginalReceiptAndCurrentState()
        {
            var character = Character(); var intent = Intent(); var started = Enter(New(), intent, character);
            var state = Finish(started.Next, End(started.BeginReceipt, CandidateProgressionEndKind.NormalExit)).Next;
            var down = Down(character); var retry = CandidateProgression.BeginAttempt(state, intent, down, 0, new ExactMathBudget());
            Assert.AreSame(state, retry.Next); Assert.AreSame(started.BeginReceipt, retry.BeginReceipt); Assert.IsNull(state.ActiveAttempt);
            Assert.AreEqual(CandidateProgressionOutcome.AlreadyIncluded, retry.Outcome); Assert.IsFalse(down.IsReady);
            var active = Enter(state, Intent(attempt: "b")); retry = CandidateProgression.BeginAttempt(active.Next, intent, down, 0, new ExactMathBudget());
            Assert.AreSame(active.Next, retry.Next); Assert.AreEqual("b", retry.Next.ActiveAttempt.Begin.AttemptId);
            Assert.IsNull(typeof(CandidateProgressionBeginIntent).GetProperty("OperationId"));
        }

        [TestCase("ChallengeId", "other")]
        [TestCase("EntryBaselineId", "other")]
        [TestCase("CharacterId", "other")]
        [TestCase("ExpectedOriginalSlot", 0)]
        public void ExistingAttemptCannotBeReboundByChangingBeginIntent(string field, object value)
        {
            var started = Enter(New(), Intent()); var input = Intent(); Set(input, field, value);
            Reject(CandidateProgression.BeginAttempt(started.Next, input, Character(), 0, new ExactMathBudget()), InconsistentBinding, field);
        }

        [Test]
        public void ChallengeCannotMoveToAnotherLevelAndOldCharacterRevisionCannotBeRewritten()
        {
            var first = Enter(New(), Intent()); var won = Finish(first.Next, End(first.BeginReceipt));
            Reject(Enter(won.Next, Intent("P-B", "c-a", "b"), accepted: false), ChallengeConflict, "ChallengeId");
            var intent = Intent(); intent.ExpectedCharacterRevision = 2;
            Reject(CandidateProgression.BeginAttempt(won.Next, intent, Character(), 0, new ExactMathBudget()), InconsistentBinding, "ExpectedCharacterRevision");
            intent = Intent("P-B"); Reject(CandidateProgression.BeginAttempt(won.Next, intent, Character(), 0, new ExactMathBudget()), InconsistentBinding, "LevelId");
        }

        [TestCase("PlayerId")]
        [TestCase("LevelId")]
        [TestCase("LevelVersion")]
        [TestCase("ChallengeId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        [TestCase("EndReceiptId")]
        [TestCase("Context")]
        [TestCase("SettlementId")]
        [TestCase("FinalReportFingerprint")]
        public void VictoryEndRequiresCompleteFacts(string field)
        {
            var started = Enter(New(), Intent()); var input = End(started.BeginReceipt); Set(input, field, null);
            Reject(Finish(started.Next, input, false), MissingField, field); Assert.IsEmpty(started.Next.FirstClears); Assert.IsFalse(started.Next.ActiveAttempt.IsEnded);
        }

        [TestCase("PlayerId")]
        [TestCase("LevelVersion")]
        [TestCase("ChallengeId")]
        [TestCase("EntryBaselineId")]
        [TestCase("EndReceiptId")]
        [TestCase("SettlementId")]
        [TestCase("FinalReportFingerprint")]
        public void ProcessedEndCannotChangeItsFixedSourceFacts(string field)
        {
            var started = Enter(New(), Intent()); var original = End(started.BeginReceipt); var state = Finish(started.Next, original).Next;
            var input = End(started.BeginReceipt); Set(input, field, "different"); var before = Describe(state);
            Reject(CandidateProgression.EndAttempt(state, input, 0, new ExactMathBudget()), InconsistentBinding, field); Assert.AreEqual(before, Describe(state));
        }

        [Test]
        public void OppositeEndAndReusedSettlementOrReceiptRejectAtomically()
        {
            var first = Enter(New(), Intent()); var original = End(first.BeginReceipt); var won = Finish(first.Next, original);
            var opposite = End(first.BeginReceipt, CandidateProgressionEndKind.NormalExit);
            Reject(Finish(won.Next, opposite, false), AttemptAlreadyClosed, "Kind");
            var next = Enter(won.Next, Intent("P-B", "c-b", "b")); var ending = End(next.BeginReceipt); var before = Describe(next.Next);
            ending.SettlementId = original.SettlementId; Reject(Finish(next.Next, ending, false), InconsistentBinding, "SettlementId");
            Assert.AreEqual(before, Describe(next.Next)); Assert.IsNull(View(next.Next, "P-C").OpenFact); Assert.IsNull(View(next.Next, "P-B").FirstClear);
            ending = End(next.BeginReceipt); ending.EndReceiptId = original.EndReceiptId; Reject(Finish(next.Next, ending, false), InconsistentBinding, "EndReceiptId");
            ending = End(next.BeginReceipt); ending.Context.DraftRevision = 2; Reject(Finish(next.Next, ending, false), InconsistentBinding, "Context.DraftRevision");
            ending = End(next.BeginReceipt); ending.LevelId = "P-A"; Reject(Finish(next.Next, ending, false), InconsistentBinding, "LevelId");
            Assert.AreEqual(before, Describe(next.Next));
        }

        [Test]
        public void NullModeFieldsRequireExplicitAssignmentsAndEnumsRejectUnspecifiedOrUnknown()
        {
            var started = Enter(New(), Intent()); var begin = started.BeginReceipt;
            var input = new CandidateProgressionEndFacts { PlayerId = "player", LevelId = "P-A", LevelVersion = "v1", Context = Context(),
                ChallengeId = "c-a", AttemptId = "a", EntryBaselineId = "baseline", EndReceiptId = "end-a", Kind = CandidateProgressionEndKind.NormalExit };
            Reject(Finish(started.Next, input, false), MissingField, "SettlementId"); input.SettlementId = null;
            Reject(Finish(started.Next, input, false), MissingField, "FinalReportFingerprint"); input.FinalReportFingerprint = null;
            Reject(Finish(started.Next, input, false), MissingField, "NewAttemptId"); input.NewAttemptId = null; Assert.IsTrue(Finish(started.Next, input).IsAccepted);
            input = End(begin); input.Kind = CandidateProgressionEndKind.Unspecified; Reject(Finish(started.Next, input, false), MissingField, "Kind");
            input.Kind = (CandidateProgressionEndKind)9; Reject(Finish(started.Next, input, false), UnsupportedBinding, "Kind");
            input = End(begin, CandidateProgressionEndKind.NormalExit); input.SettlementId = "wrong"; Reject(Finish(started.Next, input, false), InvalidValue, "SettlementId");
            input.SettlementId = null; input.FinalReportFingerprint = "wrong"; Reject(Finish(started.Next, input, false), InvalidValue, "FinalReportFingerprint");
            input = End(begin); input.NewAttemptId = "wrong"; Reject(Finish(started.Next, input, false), InvalidValue, "NewAttemptId");
            input = End(begin, CandidateProgressionEndKind.ImmediateRestart); input.NewAttemptId = null; Reject(Finish(started.Next, input, false), MissingField, "NewAttemptId");
        }

        [TestCase("ChallengeId")]
        [TestCase("AttemptId")]
        [TestCase("EntryBaselineId")]
        public void BeginRequiresExplicitBusinessIdentities(string field)
        { var input = Intent(); Set(input, field, null); Reject(Enter(New(), input, accepted: false), MissingField, field); }

        [Test]
        public void OnlyNewOperationsRequireCurrentM05RevisionAndIdsRemainOrdinal()
        {
            var started = Enter(New(), Intent()); var facts = End(started.BeginReceipt, CandidateProgressionEndKind.NormalExit);
            Reject(CandidateProgression.EndAttempt(started.Next, facts, 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            var state = Finish(started.Next, facts).Next; var next = Intent(attempt: "a ");
            Reject(CandidateProgression.BeginAttempt(state, next, Character(), 1, new ExactMathBudget()), StaleContext, "ExpectedRevision");
            Assert.AreEqual("a ", Enter(state, next).BeginReceipt.AttemptId);
            facts.AttemptId = "missing"; Reject(Finish(state, facts, false), InconsistentBinding, "AttemptId");
            Reject(CandidateProgression.CreateCandidate(Definition(), " ", new ExactMathBudget()), MissingField, "PlayerId");
        }

        [Test]
        public void InputAndOutputGraphsAreIsolatedIncludingRulesRequestsAndEndFacts()
        {
            var input = DefinitionInput(); var definition = Definition(input); var state = New(definition); var character = Character(); var characterBefore = Describe(character);
            var intent = Intent(); var check = Check(state, intent, character); var started = Enter(state, intent, character);
            var facts = End(started.BeginReceipt); var won = Finish(started.Next, facts); var view = CandidateProgression.Read(won.Next, new ExactMathBudget());
            var texts = new[] { Describe(definition), Describe(check), Describe(started), Describe(won), Describe(view) };
            input.Context.SourceNotes.Clear(); input.Levels[1].UnlockAfterLevelId = "P-C"; input.Levels[0].RequiredFeatures.Add("Tutorial"); input.Levels.Clear();
            intent.Context.SourceNotes[0] = "changed"; intent.CharacterId = "changed"; intent.ExpectedOriginalSlot = 0; intent.ChallengeId = "changed";
            facts.SettlementId = "changed"; facts.FinalReportFingerprint = "changed"; facts.Context.DraftRevision = 9;
            CollectionAssert.AreEqual(texts, new[] { Describe(definition), Describe(check), Describe(started), Describe(won), Describe(view) });
            Assert.AreEqual(characterBefore, Describe(character)); Immutable(definition); Immutable(check); Immutable(won); Immutable(view);
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateProgressionChallenge>)won.Next.Challenges).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateProgressionAttempt>)won.Next.Challenges[0].Attempts).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<CandidateProgressionOpenFact>)won.NewOpenFacts).Clear());
        }

        [Test]
        public void NewSmallBudgetsRecheckDefinitionContextAndPublicM03Inputs()
        {
            var input = DefinitionInput(); input.Context.DraftRevision = BigInteger.One << 100; var definition = Definition(input);
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.PrepareDefinition(input, new ExactMathBudget(64)));
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.CreateCandidate(definition, "player", new ExactMathBudget(64)));
            var state = New(definition); var character = Character();
            foreach (var operation in new[] { "Read", "Check", "Begin", "End" })
                Assert.Throws<ExactMathLimitException>(() => Invoke(operation, state, character, new ExactMathBudget(64)), operation);
            var growth = GrowthInput(); growth.XpBase = BigInteger.One << 100;
            var huge = Character(growth: growth); var ordinary = New();
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.CheckEntry(ordinary, Intent(), huge, new ExactMathBudget(64)));
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.BeginAttempt(ordinary, Intent(), huge, 1, new ExactMathBudget(64)));
        }

        [Test]
        public void EndedAttemptRetainsLargeM03RevisionAndChecksItEvenWithoutActiveRelationship()
        {
            var character = Character();
            for (var i = 0; i < 15; i++) character = CandidateCharacterGrowth.ApplyBaseReward(character, new CandidateBaseExperience {
                PlayerId = "player", CharacterId = "warrior", AttemptId = "growth-" + i, SettlementId = "growth-" + i, Context = Context(), Amount = 0 },
                character.StateRevision, new ExactMathBudget()).Next;
            Assert.AreEqual(new BigInteger(16), character.StateRevision);
            var started = Enter(New(), Intent(character: character), character); var state = Finish(started.Next, End(started.BeginReceipt, CandidateProgressionEndKind.NormalExit)).Next;
            Assert.IsNull(state.ActiveAttempt); Assert.AreEqual(new BigInteger(3), state.StateRevision);
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.Read(state, new ExactMathBudget(4)));
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.EndAttempt(state, End(started.BeginReceipt, CandidateProgressionEndKind.NormalExit), 0, new ExactMathBudget(4)));
            Assert.AreSame(started.BeginReceipt, state.Challenges[0].Attempts[0].Begin);
        }

        [TestCase("Prepare")]
        [TestCase("Create")]
        [TestCase("Read")]
        [TestCase("Check")]
        [TestCase("Begin")]
        [TestCase("End")]
        public void OneSharedBudgetCoversCompleteOperationAndLateFailureLeavesOldState(string operation)
        {
            var character = Character(); var state = operation == "End" ? Enter(New(), Intent(), character).Next : New();
            var before = Describe(state); var characterBefore = Describe(character); var budget = new ExactMathBudget(); var first = Invoke(operation, state, character, budget);
            Assert.Throws<ExactMathLimitException>(() => Invoke(operation, state, character, new ExactMathBudget(32768, (int)budget.PrimitiveStepsUsed - 1)));
            Assert.AreEqual(before, Describe(state)); Assert.AreEqual(characterBefore, Describe(character));
            Assert.AreEqual(Describe(first), Describe(Invoke(operation, state, character, new ExactMathBudget())));
        }

        [Test]
        public void RevisionOverflowDoesNotPublishPartialEndOrFirstClear()
        {
            // End needs no current M03 stats: retained values fit 4 bits until the revision increment.
            var active = Enter(New(), Intent());
            while (active.Next.StateRevision < 15) { var restart = End(active.BeginReceipt, CandidateProgressionEndKind.ImmediateRestart); active = Finish(active.Next, restart); }
            var end = End(active.BeginReceipt); var activeBefore = Describe(active.Next);
            Assert.Throws<ExactMathLimitException>(() => CandidateProgression.EndAttempt(active.Next, end, 15, new ExactMathBudget(4)));
            Assert.AreEqual(activeBefore, Describe(active.Next)); Assert.IsEmpty(active.Next.FirstClears); Assert.AreEqual(1, active.Next.OpenFacts.Count);
            Assert.IsTrue(Finish(active.Next, end).EndReceipt.IsFirstClear);
        }

        [Test]
        public void RootNullsThrowWithoutCreatingCandidateRelations()
        {
            var state = New(); var character = Character(); var intent = Intent(); var budget = new ExactMathBudget();
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.PrepareDefinition(null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.PrepareDefinition(DefinitionInput(), null));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.CreateCandidate(null, "p", budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.CreateCandidate(Definition(), "p", null));
            foreach (var op in new[] { "Read", "Check", "Begin", "End" })
            { Assert.Throws<ArgumentNullException>(() => Invoke(op, null, character, budget)); Assert.Throws<ArgumentNullException>(() => Invoke(op, state, character, null)); }
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.CheckEntry(state, null, character, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.CheckEntry(state, intent, null, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.BeginAttempt(state, null, character, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.BeginAttempt(state, intent, null, 1, budget));
            Assert.Throws<ArgumentNullException>(() => CandidateProgression.EndAttempt(state, null, 1, budget));
        }

        private static CandidateContext Context() { return new CandidateContext { DraftId = "progression-isolated", DraftRevision = 1,
            ContentFingerprint = "P-A-P-B-P-C-v1", RuleVersion = "candidate", NumericContractVersion = "exact", RandomContractVersion = "pcg32",
            SourceNotes = new List<string> { "Isolated ordinary P-A/P-B/P-C rules; not a published L1 to L3 chain or a submitted victory." } }; }
        private static CandidateProgressionLevelInput Node(string id, string rule, string prerequisite)
        { return new CandidateProgressionLevelInput { LevelId = id, LevelVersion = "v1", UnlockRuleId = rule, EntryKind = CandidateProgressionEntryKind.Ordinary,
            UnlockKind = prerequisite == null ? CandidateProgressionUnlockKind.InitiallyOpen : CandidateProgressionUnlockKind.AfterWholeLevelClear,
            UnlockAfterLevelId = prerequisite, RequiredFeatures = new List<string>() }; }
        private static CandidateProgressionDefinitionInput DefinitionInput() { return new CandidateProgressionDefinitionInput { Context = Context(),
            Levels = new List<CandidateProgressionLevelInput> { Node("P-A", "open-A", null), Node("P-B", "open-B", "P-A"), Node("P-C", "open-C", "P-B") } }; }
        private static CandidateProgressionDefinition Definition(CandidateProgressionDefinitionInput input = null)
        { var result = CandidateProgression.PrepareDefinition(input ?? DefinitionInput(), new ExactMathBudget()); Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Definition; }
        private static CandidateProgressionState New(CandidateProgressionDefinition definition = null)
        { var result = CandidateProgression.CreateCandidate(definition ?? Definition(), "player", new ExactMathBudget()); Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Next; }
        private static ExactRational R(BigInteger n, BigInteger? d = null) { return ExactRational.Create(n, d ?? BigInteger.One, new ExactMathBudget()); }
        private static GrowthDefinitionInput GrowthInput(CandidateContext context = null) { return new GrowthDefinitionInput { Context = context ?? Context(), ClassId = "W",
            ClassKind = CharacterClassKind.Warrior, PassiveDefinitionId = "isolated-crit", BaseStats = new StatsInput { MaxHp = R(100), Attack = R(20),
                PhysicalDefense = R(10), MagicDefense = R(6), AttackRange = 1, Evasion = R(0) }, GrowthHp = R(0), GrowthAttack = R(0), GrowthDefense = R(0),
            CritBase = R(1, 5), CritStep = R(0), CritCap = R(1, 5), CritMultiplier = R(3, 2), XpBase = 60, XpLinear = 20, XpQuadratic = 5, RecoveryDurationMilliseconds = R(10) }; }
        private static CandidateCharacterState Character(string player = "player", int slot = 2, CandidateContext context = null, GrowthDefinitionInput growth = null)
        {
            var definition = CandidateCharacterGrowth.PrepareDefinition(growth ?? GrowthInput(context), new ExactMathBudget()); Assert.IsTrue(definition.IsAccepted, definition.FieldPath);
            var result = CandidateCharacterGrowth.CreateCandidate(definition.Definition, player, "warrior", 1, 0, slot, new ExactMathBudget()); Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Next;
        }
        private static CandidateTimeSample Time(int t) { return new CandidateTimeSample { WallUtcMilliseconds = t, ObservedAtUtcMilliseconds = t,
            MonotonicElapsedMilliseconds = null, MonotonicScopeId = null, Source = "isolated-time", Trust = CandidateTimeTrust.DeviceUntrusted, Anomaly = CandidateTimeAnomaly.None }; }
        private static CandidateCharacterState Down(CandidateCharacterState character)
        {
            var result = CandidateCharacterEnd.Propose(character, new CandidateCharacterEndFacts { PlayerId = "player", CharacterId = "warrior", AttemptId = "growth-end",
                EntryBaselineId = "growth-baseline", EndReceiptId = "growth-end", Context = Context(), Kind = CandidateCharacterEndKind.NormalExit,
                WasParticipant = true, WasDown = true, RecoveryId = "recovery", TimeSample = Time(0) }, character.StateRevision, new ExactMathBudget());
            Assert.IsTrue(result.IsAccepted, result.FieldPath); return result.Next;
        }
        private static CandidateProgressionBeginIntent Intent(string level = "P-A", string challenge = "c-a", string attempt = "a", CandidateCharacterState character = null)
        { return new CandidateProgressionBeginIntent { PlayerId = "player", LevelId = level, LevelVersion = "v1", Context = Context(), CharacterId = "warrior",
            ExpectedCharacterRevision = character == null ? BigInteger.One : character.StateRevision, ExpectedOriginalSlot = character == null ? 2 : character.OriginalSlot,
            ChallengeId = challenge, AttemptId = attempt, EntryBaselineId = "baseline" }; }
        private static CandidateProgressionEndFacts End(CandidateProgressionBeginReceipt begin, CandidateProgressionEndKind kind = CandidateProgressionEndKind.NormalVictory)
        { return new CandidateProgressionEndFacts { PlayerId = begin.PlayerId, LevelId = begin.Level.LevelId, LevelVersion = begin.Level.LevelVersion, Context = Context(),
            ChallengeId = begin.ChallengeId, AttemptId = begin.AttemptId, EntryBaselineId = begin.EntryBaselineId, EndReceiptId = "end-" + begin.AttemptId, Kind = kind,
            SettlementId = kind == CandidateProgressionEndKind.NormalVictory ? "settlement-" + begin.AttemptId : null,
            FinalReportFingerprint = kind == CandidateProgressionEndKind.NormalVictory ? "report-" + begin.AttemptId : null,
            NewAttemptId = kind == CandidateProgressionEndKind.ImmediateRestart ? "next-" + begin.AttemptId : null }; }
        private static CandidateProgressionEntryCheck Check(CandidateProgressionState state, CandidateProgressionLevelRequest input, CandidateCharacterState character = null)
        { return CandidateProgression.CheckEntry(state, input, character ?? Character(), new ExactMathBudget()); }
        private static CandidateProgressionResult Enter(CandidateProgressionState state, CandidateProgressionBeginIntent intent, CandidateCharacterState character = null, bool accepted = true)
        { var r = CandidateProgression.BeginAttempt(state, intent, character ?? Character(), state.StateRevision, new ExactMathBudget()); if (accepted) Assert.IsTrue(r.IsAccepted, r.FieldPath); return r; }
        private static CandidateProgressionResult Finish(CandidateProgressionState state, CandidateProgressionEndFacts input, bool accepted = true)
        { var r = CandidateProgression.EndAttempt(state, input, state.StateRevision, new ExactMathBudget()); if (accepted) Assert.IsTrue(r.IsAccepted, r.FieldPath); return r; }
        private static CandidateProgressionLevelView View(CandidateProgressionState state, string id)
        { return CandidateProgression.Read(state, new ExactMathBudget()).Levels.Single(x => x.Level.LevelId == id); }
        private static void Reject(CandidateProgressionResult result, CandidateProgressionRejectionCode code, string field)
        { Assert.IsFalse(result.IsAccepted); Assert.IsNull(result.Next); Assert.IsNull(result.BeginReceipt); Assert.IsNull(result.EndReceipt); Assert.IsNull(result.NewFirstClear); Assert.AreEqual(code, result.RejectionCode); Assert.AreEqual(field, result.FieldPath); }
        private static object Invoke(string operation, CandidateProgressionState state, CandidateCharacterState character, ExactMathBudget budget)
        {
            switch (operation)
            {
                case "Prepare": return CandidateProgression.PrepareDefinition(DefinitionInput(), budget);
                case "Create": return CandidateProgression.CreateCandidate(Definition(), "player", budget);
                case "Read": return CandidateProgression.Read(state, budget);
                case "Check": return CandidateProgression.CheckEntry(state, Intent(), character, budget);
                case "Begin": return CandidateProgression.BeginAttempt(state, Intent(), character, state == null ? 1 : state.StateRevision, budget);
                default:
                    var facts = state != null && state.ActiveAttempt != null ? End(state.ActiveAttempt.Begin) : new CandidateProgressionEndFacts();
                    return CandidateProgression.EndAttempt(state, facts, state == null ? 1 : state.StateRevision, budget);
            }
        }
        private static void Set(object value, string path, object replacement)
        {
            var parts = path.Replace("[", ".").Replace("]", "").Split('.');
            for (var i = 0; i < parts.Length - 1; i++) value = value is IList list ? list[int.Parse(parts[i])] : value.GetType().GetProperty(parts[i]).GetValue(value);
            if (value is IList items) items[int.Parse(parts[parts.Length - 1])] = replacement;
            else value.GetType().GetProperty(parts[parts.Length - 1]).SetValue(value, replacement);
        }
        private static string Describe(object value)
        {
            if (value == null) return "null";
            if (value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return value.ToString();
            if (value is IEnumerable items) return "[" + string.Join(",", items.Cast<object>().Select(Describe)) + "]";
            return "{" + string.Join(",", value.GetType().GetProperties().Select(p => p.Name + ":" + Describe(p.GetValue(value)))) + "}";
        }
        private static void Immutable(object value)
        {
            if (value == null || value is string || value is BigInteger || value.GetType().IsPrimitive || value.GetType().IsEnum) return;
            if (value is IList list) { Assert.IsTrue(list.IsReadOnly); foreach (var item in list) Immutable(item); return; }
            Assert.IsEmpty(value.GetType().GetConstructors()); foreach (var p in value.GetType().GetProperties()) { Assert.IsNull(p.SetMethod); Immutable(p.GetValue(value)); }
        }
    }
}
