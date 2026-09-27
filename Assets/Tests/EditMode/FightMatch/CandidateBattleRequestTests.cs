using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FlowPuzzle.Core;
using NUnit.Framework;
using static FightMatch.Core.Tests.LocalSaveTestFiles;
using static FightMatch.Core.Tests.BattleApplicationRig;

namespace FightMatch.Core.Tests
{
    [TestFixture]
    public sealed class CandidateBattleRequestTests
    {
        [TestCase(CandidateApplicationKind.Attack)]
        [TestCase(CandidateApplicationKind.Link)]
        [TestCase(CandidateApplicationKind.Rollback)]
        public void B15_01_PrepareFreezesEachShapeAndUsesOneNonPubliclySuppliedId(CandidateApplicationKind kind)
        {
            using (var r = new BattleApplicationRig(false))
            {
                var draft = PlainDraft(kind);
                var request = r.Freeze(draft);
                var bytes = request.Intent.CanonicalBytes.ToArray();
                var notes = draft.Context.SourceNotes.ToArray();
                draft.Context.SourceNotes.Clear();
                draft.Context.DraftRevision = 99;
                draft.PlayerId = "changed";
                draft.ExpectedCommitId = "changed";
                if (draft.Attack != null)
                {
                    ((IList<FlowPos>)draft.Attack.Route).Clear();
                    draft.Attack.Actor = BattleCombatantKey.ForParticipant("different", "different");
                    draft.Attack.ExpectedPreferenceRevision = 7;
                    draft.Attack.ItemUseEnabled = true;
                }
                if (draft.Link != null)
                {
                    ((IList<FlowPos>)draft.Link.Route).Clear();
                    draft.Link.ExpectedSceneRevision = 55;
                }
                if (draft.Rollback != null)
                {
                    ((IList<string>)draft.Rollback.ConfirmedRemovedOperationIds).Clear();
                    draft.Rollback.TargetOperationId = "other";
                }
                CollectionAssert.AreEqual(bytes, request.Intent.CanonicalBytes);
                CollectionAssert.AreEqual(notes, request.Intent.Context.SourceNotes);
                Assert.AreEqual(kind, request.Kind);
                Assert.AreEqual(request.Intent.OperationId, request.OperationId);
                Assert.IsTrue(Guid.TryParseExact(request.OperationId, "N", out _));
                Assert.AreEqual(0, r.Runtime.Storage.Base.Calls);
                Assert.IsNull(typeof(CandidateBattleDraft).GetProperty("OperationId"));
                Assert.IsEmpty(typeof(PreparedCandidateBattleRequest).GetConstructors());
                CollectionAssert.AreEquivalent(new[] { "Intent", "Kind", "OperationId" },
                    typeof(PreparedCandidateBattleRequest).GetProperties().Select(x => x.Name));
                Assert.Throws<NotSupportedException>(() => ((IList<byte>)request.Intent.CanonicalBytes)[0] = 0);
                Assert.Throws<NotSupportedException>(() => ((IList<string>)request.Intent.Context.SourceNotes).Add("mutate"));
            }
        }

        [Test]
        public void B15_01_ExecutionUsesFrozenRouteContextAndConditionsAfterCallerMutation()
        {
            using (var r = new BattleApplicationRig())
            {
                var draft = r.AttackDraft();
                var route = draft.Attack.Route.ToArray();
                var request = r.Freeze(draft);
                draft.Context.SourceNotes.Clear();
                draft.Attack.Route = new[] { new FlowPos(-1, -1), new FlowPos(-2, -2) };
                draft.Attack.ExpectedSceneRevision = 900;
                draft.Attack.ItemUseEnabled = true;
                var result = Is(r.System.Submit(request, B()), "Completed");
                var record = result.Application.OriginalLookup.BattleOperation;
                CollectionAssert.AreEqual(route, record.Request.Route);
                Assert.IsFalse(record.Conditions.ItemUseEnabled);
                CollectionAssert.AreEqual(request.Intent.CanonicalBytes, result.Application.OriginalLookup.Record.Intent.CanonicalBytes);
                Assert.IsNull(result.DomainRejection);
            }
        }

        [TestCase("kind", "UnsupportedBinding")]
        [TestCase("null-kind", "MissingField")]
        [TestCase("two", "InvalidValue")]
        [TestCase("none", "MissingField")]
        [TestCase("wrong-payload", "MissingField")]
        [TestCase("context", "MissingField")]
        [TestCase("actor", "MissingField")]
        [TestCase("enabled", "MissingField")]
        [TestCase("revision", "MissingField")]
        [TestCase("route", "MissingField")]
        public void B15_01_InvalidShapeHasNoAcceptedRequestOrIo(string change, string expected)
        {
            using (var r = new BattleApplicationRig(false))
            {
                var d = PlainDraft(CandidateApplicationKind.Attack);
                switch (change)
                {
                    case "kind": d.Kind = (CandidateApplicationKind)999; break;
                    case "null-kind": d.Kind = null; break;
                    case "two": d.Link = PlainDraft(CandidateApplicationKind.Link).Link; break;
                    case "none": d.Attack = null; break;
                    case "wrong-payload": d.Kind = CandidateApplicationKind.Link; break;
                    case "context": d.Context = null; break;
                    case "actor": d.Attack.Actor = null; break;
                    case "enabled": d.Attack.ItemUseEnabled = null; break;
                    case "revision": d.Attack.ExpectedSceneRevision = null; break;
                    case "route": d.Attack.Route = null; break;
                }
                var result = r.System.Prepare(d, B().Codec);
                Assert.AreEqual(expected, result.Code, result.Diagnostic?.FieldPath);
                Assert.IsFalse(result.IsAccepted);
                Assert.IsNull(result.Request);
                Assert.IsNotEmpty(result.Diagnostic.FieldPath);
                Assert.AreEqual(0, r.Runtime.Storage.Base.Calls);
            }
        }

        [TestCase(CandidateApplicationKind.Attack)]
        [TestCase(CandidateApplicationKind.Link)]
        [TestCase(CandidateApplicationKind.Rollback)]
        public void B15_01_OversizedListsAreRejectedBeforeAnyElementAccess(CandidateApplicationKind kind)
        {
            using (var r = new BattleApplicationRig(false))
            {
                var draft = PlainDraft(kind);
                var route = new UnvisitedList<FlowPos>(65537);
                var ids = new UnvisitedList<string>(65537);
                if (draft.Attack != null) draft.Attack.Route = route;
                if (draft.Link != null) draft.Link.Route = route;
                if (draft.Rollback != null) draft.Rollback.ConfirmedRemovedOperationIds = ids;
                var result = r.System.Prepare(draft, B().Codec);
                Assert.AreEqual("Limit", result.Code);
                Assert.AreEqual("CollectionEntries", result.Diagnostic.LimitReason);
                Assert.AreEqual(65537UL, result.Diagnostic.RequiredAtLeast);
                Assert.AreEqual(65536UL, result.Diagnostic.Allowed);
                Assert.AreEqual(0, route.Visits + ids.Visits);
                Assert.AreEqual(0, r.Runtime.Storage.Base.Calls);
            }
        }

        [TestCase("notes", "CollectionEntries")]
        [TestCase("string", "StringCodeUnits")]
        [TestCase("integer", "IntegerBits")]
        [TestCase("token", "NumericTokenBytes")]
        [TestCase("steps", "PrimitiveSteps")]
        public void B15_01_PreCopyBudgetsPreserveExactLimitDetails(string change, string reason)
        {
            using (var r = new BattleApplicationRig(false))
            {
                var draft = PlainDraft(CandidateApplicationKind.Attack);
                SaveCodecBudget budget;
                switch (change)
                {
                    case "notes":
                        draft.Context.SourceNotes.Add("another");
                        budget = new SaveCodecBudget(new ExactMathBudget(), maxCollectionEntries: 1);
                        break;
                    case "string":
                        draft.PlayerId = new string('x', 100);
                        budget = new SaveCodecBudget(new ExactMathBudget(), maxStringCodeUnits: 32);
                        break;
                    case "integer":
                        draft.Attack.ExpectedSceneRevision = BigInteger.One << 200;
                        budget = new SaveCodecBudget(new ExactMathBudget(128));
                        break;
                    case "token":
                        draft.Attack.ExpectedPreferenceRevision = 10000;
                        budget = new SaveCodecBudget(new ExactMathBudget(), maxNumericTokenBytes: 4);
                        break;
                    default: budget = new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 0)); break;
                }
                var result = r.System.Prepare(draft, budget);
                Assert.AreEqual("Limit", result.Code);
                Assert.AreEqual(reason, result.Diagnostic.LimitReason);
                Assert.Greater(result.Diagnostic.RequiredAtLeast, result.Diagnostic.Allowed);
                Assert.IsNull(result.Request);
                Assert.AreEqual(0, r.Runtime.Storage.Base.Calls);
            }
        }
    }
}
