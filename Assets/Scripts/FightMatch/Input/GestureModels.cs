using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Input
{
    public enum GestureMode
    {
        Attack = 0,
        FreeLink = 1
    }

    public enum GestureStage
    {
        Idle = 0,
        Pressed = 1,
        Dragging = 2
    }

    public enum GestureIntentKind
    {
        TapLocator = 0,
        AttackRoute = 1,
        FreeLinkRoute = 2
    }

    public enum OriginKind
    {
        Endpoint = 0,
        LockedLine = 1
    }

    // 公开输入：一对端点及其当前固化线路。构造即验证自身可判定的形状；跨 Pair 形状由 GestureContext 校验。
    public sealed class GesturePair
    {
        private readonly FlowPos[] _locked;

        public string PairId { get; }
        public FlowPos EndpointA { get; }
        public FlowPos EndpointB { get; }
        public bool CanDraw { get; }
        public IReadOnlyList<FlowPos> LockedCells { get; }

        public GesturePair(string pairId, FlowPos endpointA, FlowPos endpointB, bool canDraw,
            IReadOnlyList<FlowPos> lockedCells)
        {
            if (string.IsNullOrWhiteSpace(pairId))
                throw new ArgumentException("pairId is blank", nameof(pairId));
            if (lockedCells == null)
                throw new ArgumentNullException(nameof(lockedCells));
            if (endpointA.Equals(endpointB))
                throw new ArgumentException("endpoints must differ", nameof(endpointB));

            var copy = new FlowPos[lockedCells.Count];
            for (int i = 0; i < copy.Length; i++)
                copy[i] = lockedCells[i];

            for (int i = 0; i < copy.Length; i++)
                for (int j = i + 1; j < copy.Length; j++)
                    if (copy[i].Equals(copy[j]))
                        throw new ArgumentException("lockedCells has duplicate cell", nameof(lockedCells));

            if (copy.Length > 0)
            {
                bool hasA = false;
                bool hasB = false;
                for (int i = 0; i < copy.Length; i++)
                {
                    if (copy[i].Equals(endpointA)) hasA = true;
                    if (copy[i].Equals(endpointB)) hasB = true;
                }
                if (!(hasA && hasB))
                    throw new ArgumentException(
                        "non-empty lockedCells must contain both endpoints", nameof(lockedCells));
            }

            if (copy.Length > 0 && canDraw)
                throw new ArgumentException(
                    "pair with locked cells must have canDraw=false", nameof(canDraw));

            _locked = copy;
            PairId = pairId;
            EndpointA = endpointA;
            EndpointB = endpointB;
            CanDraw = canDraw;
            LockedCells = Array.AsReadOnly(_locked);
        }
    }

    // 公开输入：一次指针采样。坐标必须有限；cell=null 表示棋盘外，板内/越界由 RouteGesture 结合当前 Context 判定。
    public struct PointerSample
    {
        public int PointerId { get; }
        public double LocalX { get; }
        public double LocalY { get; }
        public FlowPos? Cell { get; }

        public PointerSample(int pointerId, double localX, double localY, FlowPos? cell)
        {
            if (double.IsNaN(localX) || double.IsInfinity(localX))
                throw new ArgumentException("localX must be finite", nameof(localX));
            if (double.IsNaN(localY) || double.IsInfinity(localY))
                throw new ArgumentException("localY must be finite", nameof(localY));
            PointerId = pointerId;
            LocalX = localX;
            LocalY = localY;
            Cell = cell;
        }
    }

    // 公开输入：一次绑定的完整只读上下文。构造即验证并复制所有集合，之后不可变。
    public sealed class GestureContext
    {
        private readonly GesturePair[] _pairs;

        public string AttemptId { get; }
        public BigInteger SceneRevision { get; }
        public BigInteger PreferenceRevision { get; }
        public string FaceId { get; }
        public int Width { get; }
        public int Height { get; }
        public bool Enabled { get; }
        public bool AllowHistoryTap { get; }
        public GestureMode Mode { get; }
        public string SelectedCharacterId { get; } // 可 null，但不允许空白
        public IReadOnlyList<GesturePair> Pairs { get; }

        public GestureContext(string attemptId, BigInteger sceneRevision, BigInteger preferenceRevision,
            string faceId, int width, int height, bool enabled, bool allowHistoryTap,
            GestureMode mode, string selectedCharacterId, IReadOnlyList<GesturePair> pairs)
        {
            if (string.IsNullOrWhiteSpace(attemptId))
                throw new ArgumentException("attemptId is blank", nameof(attemptId));
            if (sceneRevision.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(sceneRevision), "sceneRevision must be non-negative");
            if (preferenceRevision.Sign < 0)
                throw new ArgumentOutOfRangeException(nameof(preferenceRevision), "preferenceRevision must be non-negative");
            if (string.IsNullOrWhiteSpace(faceId))
                throw new ArgumentException("faceId is blank", nameof(faceId));
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "width must be positive");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "height must be positive");
            if (selectedCharacterId != null && string.IsNullOrWhiteSpace(selectedCharacterId))
                throw new ArgumentException("selectedCharacterId must be null or non-blank", nameof(selectedCharacterId));
            if (mode != GestureMode.Attack && mode != GestureMode.FreeLink)
                throw new ArgumentException("mode must be Attack or FreeLink", nameof(mode));
            if (pairs == null)
                throw new ArgumentNullException(nameof(pairs));

            var pairCopy = new GesturePair[pairs.Count];
            for (int i = 0; i < pairCopy.Length; i++)
            {
                if (pairs[i] == null)
                    throw new ArgumentException("pairs contains null element", nameof(pairs));
                pairCopy[i] = pairs[i];
            }

            // PairId 唯一（Ordinal，不 trim）
            for (int i = 0; i < pairCopy.Length; i++)
                for (int j = i + 1; j < pairCopy.Length; j++)
                    if (string.Equals(pairCopy[i].PairId, pairCopy[j].PairId, StringComparison.Ordinal))
                        throw new ArgumentException("duplicate pairId", nameof(pairs));

            // 所有端点在板内
            for (int i = 0; i < pairCopy.Length; i++)
            {
                if (!InBoard(pairCopy[i].EndpointA, width, height))
                    throw new ArgumentException("endpointA out of board at pair " + pairCopy[i].PairId, nameof(pairs));
                if (!InBoard(pairCopy[i].EndpointB, width, height))
                    throw new ArgumentException("endpointB out of board at pair " + pairCopy[i].PairId, nameof(pairs));
            }

            // 所有 Pair 端点互不重叠
            for (int i = 0; i < pairCopy.Length; i++)
            {
                for (int j = i + 1; j < pairCopy.Length; j++)
                {
                    if (pairCopy[i].EndpointA.Equals(pairCopy[j].EndpointA) ||
                        pairCopy[i].EndpointA.Equals(pairCopy[j].EndpointB) ||
                        pairCopy[i].EndpointB.Equals(pairCopy[j].EndpointA) ||
                        pairCopy[i].EndpointB.Equals(pairCopy[j].EndpointB))
                        throw new ArgumentException("endpoints overlap between pairs", nameof(pairs));
                }
            }

            // 固化格在板内、不同固化线路不重叠、固化格不占其他 Pair 端点
            for (int i = 0; i < pairCopy.Length; i++)
            {
                var li = pairCopy[i].LockedCells;
                for (int a = 0; a < li.Count; a++)
                {
                    if (!InBoard(li[a], width, height))
                        throw new ArgumentException("locked cell out of board at pair " + pairCopy[i].PairId, nameof(pairs));
                    // 不占其他 Pair 端点
                    for (int j = 0; j < pairCopy.Length; j++)
                    {
                        if (j == i) continue;
                        if (li[a].Equals(pairCopy[j].EndpointA) || li[a].Equals(pairCopy[j].EndpointB))
                            throw new ArgumentException(
                                "locked cell occupies another pair endpoint", nameof(pairs));
                    }
                    // 不同固化线路不重叠
                    for (int j = i + 1; j < pairCopy.Length; j++)
                    {
                        var lj = pairCopy[j].LockedCells;
                        for (int b = 0; b < lj.Count; b++)
                            if (li[a].Equals(lj[b]))
                                throw new ArgumentException("locked cells overlap between pairs", nameof(pairs));
                    }
                }
            }

            _pairs = pairCopy;
            AttemptId = attemptId;
            SceneRevision = sceneRevision;
            PreferenceRevision = preferenceRevision;
            FaceId = faceId;
            Width = width;
            Height = height;
            Enabled = enabled;
            AllowHistoryTap = allowHistoryTap;
            Mode = mode;
            SelectedCharacterId = selectedCharacterId;
            Pairs = Array.AsReadOnly(_pairs);
        }

        private static bool InBoard(FlowPos c, int width, int height)
        {
            return c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
        }
    }

    public enum GestureFeedbackKind
    {
        InvalidStart, NotAdjacent, Crossed, WrongEndpoint, Cancelled, FocusLost, SecondPointerIgnored, ActionAccepted
    }

    // 只读观察：每次 Read 返回独立副本，之后输入不影响旧观察。
    public sealed class GestureView
    {
        public GestureStage Stage { get; }
        public int? ActivePointerId { get; }
        public string PairId { get; }
        public FlowPos? OriginCell { get; }
        public IReadOnlyList<FlowPos> DraftCells { get; }
        public bool HasInvalidSample { get; }

        public GestureView(GestureStage stage, int? activePointerId, string pairId,
            FlowPos? originCell, IReadOnlyList<FlowPos> draftCells, bool hasInvalidSample)
        {
            Stage = stage;
            ActivePointerId = activePointerId;
            PairId = pairId;
            OriginCell = originCell;
            if (draftCells == null || draftCells.Count == 0)
            {
                DraftCells = Array.AsReadOnly(EmptyDraft);
            }
            else
            {
                var copy = new FlowPos[draftCells.Count];
                for (int i = 0; i < copy.Length; i++)
                    copy[i] = draftCells[i];
                DraftCells = Array.AsReadOnly(copy);
            }
            HasInvalidSample = hasInvalidSample;
        }

        internal static readonly FlowPos[] EmptyDraft = new FlowPos[0];
    }

    // 只读意图草案：路线含首尾完整格序列；Tap 的 Cells 显式空、CharacterId=null。
    public sealed class GestureIntent
    {
        public GestureIntentKind Kind { get; }
        public GestureContext Context { get; }
        public string PairId { get; }
        public string CharacterId { get; }
        public FlowPos OriginCell { get; }
        public OriginKind OriginKind { get; }
        public IReadOnlyList<FlowPos> Cells { get; }

        internal GestureIntent(GestureIntentKind kind, GestureContext context, string pairId,
            string characterId, FlowPos originCell, OriginKind originKind, IReadOnlyList<FlowPos> cells)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            Kind = kind;
            Context = context;
            PairId = pairId;
            CharacterId = characterId;
            OriginCell = originCell;
            OriginKind = originKind;
            if (cells == null || cells.Count == 0)
            {
                Cells = Array.AsReadOnly(GestureView.EmptyDraft);
            }
            else
            {
                var copy = new FlowPos[cells.Count];
                for (int i = 0; i < copy.Length; i++)
                    copy[i] = cells[i];
                Cells = Array.AsReadOnly(copy);
            }
        }
    }
}
