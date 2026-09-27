using System;
using System.Collections.Generic;
using System.Numerics;
using FightMatch.Core;
using FlowPuzzle.Core;

namespace FightMatch.Application
{
    internal static class CandidateBattleRequestFactory
    {
        private sealed class Refusal : Exception
        {
            internal readonly CandidateApplicationDiagnostic Diagnostic;
            internal Refusal(CandidateApplicationDiagnostic diagnostic) { Diagnostic = diagnostic; }
        }

        internal static CandidateBattlePrepareResult Prepare(CandidateBattleDraft draft, SaveCodecBudget budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            try
            {
                Check(draft != null, "MissingField", "Draft");
                Check(draft.Kind.HasValue, "MissingField", "Kind");
                Check(draft.Kind == CandidateApplicationKind.Attack || draft.Kind == CandidateApplicationKind.Link ||
                    draft.Kind == CandidateApplicationKind.Rollback, "UnsupportedBinding", "Kind");
                var count = (draft.Attack == null ? 0 : 1) + (draft.Link == null ? 0 : 1) + (draft.Rollback == null ? 0 : 1);
                Check(count == 1, count == 0 ? "MissingField" : "InvalidValue", "Payload");
                Check(draft.Kind == CandidateApplicationKind.Attack ? draft.Attack != null :
                    draft.Kind == CandidateApplicationKind.Link ? draft.Link != null : draft.Rollback != null,
                    "MissingField", draft.Kind.ToString());
                Check(draft.Context != null, "MissingField", "Context");
                var a = draft.Attack;
                var l = draft.Link;
                var r = draft.Rollback;
                var c = draft.Context;
                // Check every count before enumerating or copying any caller collection.
                var context = RuleContextChecks.Prepare(c, budget);
                if (!context.IsAccepted) return new CandidateBattlePrepareResult(null, CandidateApplicationDiagnostic.From(context, "Prepare"));
                if (a != null) Count(a.Route, "Attack.Route", budget);
                if (l != null) Count(l.Route, "Link.Route", budget);
                if (r != null) Count(r.ConfirmedRemovedOperationIds, "Rollback.ConfirmedRemovedOperationIds", budget);
                Text(draft.PlayerId, "PlayerId", budget);
                Text(draft.ExpectedCommitId, "ExpectedCommitId", budget);
                if (a != null)
                {
                    Text(a.AttemptId, "Attack.AttemptId", budget);
                    Number(a.ExpectedSceneRevision, budget);
                    Number(a.ExpectedPreferenceRevision, budget);
                    if (a.Actor != null)
                    {
                        Text(a.Actor.AttemptId, "Attack.Actor.AttemptId", budget);
                        Text(a.Actor.CharacterId, "Attack.Actor.CharacterId", budget);
                        Text(a.Actor.FaceId, "Attack.Actor.FaceId", budget);
                        Text(a.Actor.EnemyInstanceKey, "Attack.Actor.EnemyInstanceKey", budget);
                    }
                    Pair(a.Pair, "Attack.Pair", budget);
                    Route(a.Route, budget);
                }
                if (l != null)
                {
                    Text(l.AttemptId, "Link.AttemptId", budget);
                    Number(l.ExpectedSceneRevision, budget);
                    Pair(l.Pair, "Link.Pair", budget);
                    Route(l.Route, budget);
                }
                if (r != null)
                {
                    Text(r.AttemptId, "Rollback.AttemptId", budget);
                    Number(r.ExpectedSceneRevision, budget);
                    Text(r.HistoryAnchorId, "Rollback.HistoryAnchorId", budget);
                    Text(r.TargetOperationId, "Rollback.TargetOperationId", budget);
                    foreach (var id in r.ConfirmedRemovedOperationIds) Text(id, "Rollback.ConfirmedRemovedOperationIds", budget);
                }
                var input = new CandidateApplicationIntentInput
                {
                    PlayerId = draft.PlayerId, ExpectedCommitId = draft.ExpectedCommitId, Kind = draft.Kind,
                    Context = RuleContextChecks.Copy(context.Value),
                    Attack = a == null ? null : new CandidateApplicationAttackInput
                    {
                        AttemptId = a.AttemptId, ExpectedSceneRevision = a.ExpectedSceneRevision, Actor = a.Actor,
                        Pair = a.Pair, Route = new List<FlowPos>(a.Route).AsReadOnly(),
                        ExpectedPreferenceRevision = a.ExpectedPreferenceRevision, ItemUseEnabled = a.ItemUseEnabled
                    },
                    Link = l == null ? null : new CandidateApplicationLinkInput
                    {
                        AttemptId = l.AttemptId, ExpectedSceneRevision = l.ExpectedSceneRevision,
                        Pair = l.Pair, Route = new List<FlowPos>(l.Route).AsReadOnly()
                    },
                    Rollback = r == null ? null : new CandidateApplicationRollbackInput
                    {
                        AttemptId = r.AttemptId, ExpectedSceneRevision = r.ExpectedSceneRevision,
                        HistoryAnchorId = r.HistoryAnchorId, TargetOperationId = r.TargetOperationId,
                        ConfirmedRemovedOperationIds = new List<string>(r.ConfirmedRemovedOperationIds).AsReadOnly()
                    },
                    OperationId = Guid.NewGuid().ToString("N")
                };
                var prepared = CandidateApplicationProtocol.PrepareIntent(input, budget);
                return prepared.IsAccepted
                    ? new CandidateBattlePrepareResult(new PreparedCandidateBattleRequest(prepared.Value, input))
                    : new CandidateBattlePrepareResult(null, CandidateApplicationDiagnostic.From(prepared, "Prepare"));
            }
            catch (Refusal error) { return new CandidateBattlePrepareResult(null, error.Diagnostic); }
        }

        private static void Check(bool accepted, string code, string path)
        {
            if (!accepted) throw new Refusal(new CandidateApplicationDiagnostic(code, path, stage: "Prepare"));
        }

        private static void Limit(int required, int allowed, string path, string reason)
        {
            if (required > allowed) throw new Refusal(new CandidateApplicationDiagnostic("Limit", path,
                reason, (ulong)required, (ulong)allowed, "Prepare"));
        }

        private static void Count<T>(IReadOnlyList<T> list, string path, SaveCodecBudget budget)
        {
            Check(list != null, "MissingField", path);
            Limit(list.Count, budget.MaxCollectionEntries, path, "CollectionEntries");
        }

        private static void Text(string text, string path, SaveCodecBudget budget)
        { if (text != null) Limit(text.Length, budget.MaxStringCodeUnits, path, "StringCodeUnits"); }

        private static void Number(BigInteger? number, SaveCodecBudget budget)
        {
            if (!number.HasValue) return;
            var encoded = ExactSaveValueCodec.EncodeInteger(number.Value, budget);
            if (!encoded.IsAccepted) throw new Refusal(CandidateApplicationDiagnostic.From(encoded, "Prepare"));
        }

        private static void Pair(BattlePairKey pair, string path, SaveCodecBudget budget)
        {
            if (pair == null) return;
            Text(pair.AttemptId, path + ".AttemptId", budget);
            Text(pair.FaceId, path + ".FaceId", budget);
            Text(pair.PairId, path + ".PairId", budget);
        }

        private static void Route(IReadOnlyList<FlowPos> route, SaveCodecBudget budget)
        { foreach (var cell in route) { Number(cell.x, budget); Number(cell.y, budget); } }
    }
}
