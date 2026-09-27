using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FightMatch.Application;
using FightMatch.Core;
using FlowPuzzle.Core;

namespace FightMatch.Presentation
{
    public sealed class CandidatePlaybackActor
    {
        public BattleCombatantKey Key { get; }
        public string Label { get; }
        public ExactRational Hp { get; }
        public ExactRational MaxHp { get; }
        public string NextIntent { get; }
        public BigInteger? IntentCursor { get; }
        internal CandidatePlaybackActor(BattleCombatantKey key, string label, ExactRational hp, ExactRational maxHp,
            string nextIntent = null, BigInteger? cursor = null)
        { Key = key; Label = label; Hp = hp; MaxHp = maxHp; NextIntent = nextIntent; IntentCursor = cursor; }
        internal CandidatePlaybackActor Damage(ExactRational hp)
        { return new CandidatePlaybackActor(Key, Label, hp, MaxHp, hp.Numerator.IsZero ? "已击倒" : NextIntent, IntentCursor); }
        internal CandidatePlaybackActor Intent(string intent, BigInteger cursor)
        { return new CandidatePlaybackActor(Key, Label, Hp, MaxHp, intent, cursor); }
    }

    // Disposable display values only. This never constructs or replaces a BattleSnapshot.
    public sealed class CandidateBattlePlaybackFrame
    {
        public PreparedFace Face { get; }
        public IReadOnlyList<CandidatePlaybackActor> Actors { get; }
        public IReadOnlyList<BattleLockedRoute> LockedRoutes { get; }
        public IReadOnlyList<BattlePairKey> PendingLinks { get; }
        public IReadOnlyList<FlowPos> TemporaryRoute { get; }
        public BattlePhase? Phase { get; }
        public int OriginalFactIndex { get; }
        public CandidateBattleOrderedFact OriginalFact { get; }
        public string Beat { get; }
        public string StageFeedback { get; }
        public bool UsesKnownFinalState { get; }

        private CandidateBattlePlaybackFrame(PreparedFace face, IEnumerable<CandidatePlaybackActor> actors,
            IEnumerable<BattleLockedRoute> routes, IEnumerable<BattlePairKey> pending, IEnumerable<FlowPos> temporary,
            BattlePhase? phase, int index, CandidateBattleOrderedFact fact, string beat, string stage, bool knownFinal)
        { Face = face; Actors = new List<CandidatePlaybackActor>(actors).AsReadOnly(); LockedRoutes = new List<BattleLockedRoute>(routes).AsReadOnly();
            PendingLinks = new List<BattlePairKey>(pending).AsReadOnly(); TemporaryRoute = new List<FlowPos>(temporary).AsReadOnly();
            Phase = phase; OriginalFactIndex = index; OriginalFact = fact; Beat = beat; StageFeedback = stage; UsesKnownFinalState = knownFinal; }

        internal static CandidateBattlePlaybackFrame From(BattleSnapshot snapshot, string beat = "当前战局", bool knownFinal = true,
            IReadOnlyList<FlowPos> temporary = null)
        {
            var actors = new List<CandidatePlaybackActor>();
            if (snapshot != null)
            {
                foreach (var m in snapshot.Members) actors.Add(new CandidatePlaybackActor(m.CombatantKey, m.Member.CharacterId, m.Hp, m.Member.Stats.MaxHp));
                foreach (var e in snapshot.Enemies) actors.Add(new CandidatePlaybackActor(e.CombatantKey, e.PairKey.PairId, e.Hp, e.Enemy.Stats.MaxHp,
                    e.Hp.Numerator.IsZero ? "已击倒" : Intent(e.Enemy, e.IntentCursor), e.IntentCursor));
            }
            return new CandidateBattlePlaybackFrame(snapshot?.Board.Face, actors, snapshot?.Board.LockedRoutes ?? Array.Empty<BattleLockedRoute>(),
                snapshot?.Board.PendingLinks ?? Array.Empty<BattlePairKey>(), temporary ?? Array.Empty<FlowPos>(), snapshot?.Phase,
                -1, null, beat, snapshot?.Phase.ToString(), knownFinal);
        }
        internal static CandidateBattlePlaybackFrame Initial(CandidateBattlePresentation original, IReadOnlyList<FlowPos> route)
        {
            var crossedFace = original.BeforeSnapshot.Board.Face.FaceId != original.AfterSnapshot.Board.Face.FaceId;
            return From(crossedFace ? original.AfterSnapshot : original.BeforeSnapshot, "行动已提交，准备播放", crossedFace,
                crossedFace ? null : route);
        }
        internal CandidateBattlePlaybackFrame Apply(CandidateBattlePresentation original, CandidateBattleOrderedFact fact)
        {
            var actors = Actors.ToList(); var routes = LockedRoutes.ToList(); var pending = PendingLinks.ToList();
            var temporary = TemporaryRoute; var phase = Phase; var stage = StageFeedback; var beat = "";
            var knownFinal = UsesKnownFinalState; var face = Face;
            var crossFaceFinal = knownFinal && face?.FaceId != original.BeforeSnapshot.Board.Face.FaceId;
            if (fact.Kind == CandidateBattleFactKind.DirectAttack)
            {
                var d = fact.DirectAttack; beat = "命中 " + d.Pair.PairId + (d.Crit.Triggered ? " · 暴击" : "") + "：" + Hp(d.HpBefore) + " → " + Hp(d.HpAfter);
                if (!crossFaceFinal) ReplaceHp(actors, d.Target, d.HpAfter);
            }
            else if (fact.Kind == CandidateBattleFactKind.EnemyIntent)
            {
                var e = fact.EnemyIntent; beat = e.Pair.PairId + (e.IntentKind == EnemyIntentKind.Charge ? " 蓄力" : " 重击／攻击");
                if (e.Damage != null) beat += "：" + Hp(e.Damage.HpBefore) + " → " + Hp(e.Damage.HpAfter);
                if (!crossFaceFinal)
                {
                    if (e.Damage != null) ReplaceHp(actors, e.Damage.TargetMember, e.Damage.HpAfter);
                    var definition = original.BeforeSnapshot.Enemies.Single(x => x.CombatantKey.Equals(e.EnemyKey)).Enemy;
                    for (var i = 0; i < actors.Count; i++) if (actors[i].Key.Equals(e.EnemyKey)) actors[i] = actors[i].Intent(Intent(definition, e.CursorAfter), e.CursorAfter);
                }
            }
            else
            {
                var s = fact.Stage; stage = s.Kind.ToString(); beat = stage + (s.Pair == null ? "" : " " + s.Pair.PairId);
                if (s.Kind == CandidateStageFactKind.TemporaryRouteRemoved) temporary = Array.Empty<FlowPos>();
                else if (s.Kind == CandidateStageFactKind.FaceChanged || (s.FaceId != face?.FaceId && !crossFaceFinal))
                    return FinalWithFeedback(original, fact, beat, stage);
                else if (!crossFaceFinal && s.Kind == CandidateStageFactKind.RouteLocked)
                {
                    var locked = original.AfterSnapshot.Board.LockedRoutes.SingleOrDefault(x => x.PairKey.Equals(s.Pair));
                    if (locked == null || locked.PairKey.FaceId != face?.FaceId) return FinalWithFeedback(original, fact, beat, stage);
                    routes.RemoveAll(x => x.PairKey.Equals(s.Pair)); routes.Add(locked); temporary = Array.Empty<FlowPos>();
                }
                else if (!crossFaceFinal && s.Kind == CandidateStageFactKind.PendingLinkAdded)
                { if (!pending.Any(x => x.Equals(s.Pair))) pending.Add(s.Pair); }
                else if (!crossFaceFinal && s.Kind == CandidateStageFactKind.PendingLinkRemoved) pending.RemoveAll(x => x.Equals(s.Pair));
                if (s.Kind == CandidateStageFactKind.PhaseSelected) phase = s.Phase;
            }
            return new CandidateBattlePlaybackFrame(face, actors, routes, pending, temporary, phase, fact.Index, fact, beat, stage, knownFinal);
        }
        private static CandidateBattlePlaybackFrame FinalWithFeedback(CandidateBattlePresentation original, CandidateBattleOrderedFact fact, string beat, string stage)
        {
            var final = From(original.AfterSnapshot);
            return new CandidateBattlePlaybackFrame(final.Face, final.Actors, final.LockedRoutes, final.PendingLinks, final.TemporaryRoute,
                final.Phase, fact.Index, fact, beat + "（显示已知终态）", stage, true);
        }
        private static void ReplaceHp(List<CandidatePlaybackActor> actors, BattleCombatantKey key, ExactRational hp)
        { for (var i = 0; i < actors.Count; i++) if (actors[i].Key.Equals(key)) { actors[i] = actors[i].Damage(hp); return; } }
        private static string Intent(PreparedEnemy enemy, BigInteger cursor)
        { return enemy.IntentCycle[(int)(cursor % enemy.IntentCycle.Count)].Kind == EnemyIntentKind.Charge ? "蓄力" : "攻击"; }
        public static string Hp(ExactRational value)
        { return value.Denominator.IsOne ? value.Numerator.ToString() : value.Numerator + "/" + value.Denominator; }
    }
}
