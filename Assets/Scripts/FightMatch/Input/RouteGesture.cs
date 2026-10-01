using System;
using System.Collections.Generic;
using System.Numerics;
using FlowPuzzle.Core;

namespace FightMatch.Input
{
    // 单指针画线手势模块。普通同步实例，无接口抽象、无事件总线、无静态可变缓存、无可插拔策略。
    // 调用者串行送入已映射的棋盘格与局部指针坐标；模块只管理临时手势并返回意图草案。
    public sealed class RouteGesture
    {
        private GestureContext _context;
        private double _threshold;
        private bool _active;
        private int _activePointerId;
        private FlowPos _originCell;
        private string _originPairId;
        private OriginKind _originKind;
        private FlowPos _endpointTarget;
        private GestureStage _stage;
        private double _startX;
        private double _startY;
        private bool _thresholdExceeded;
        private readonly List<FlowPos> _draft = new List<FlowPos>();
        private bool _hasInvalidSample;
        private GestureFeedbackKind? _feedback;
        private bool _feedbackReported;

        public GestureFeedbackKind? TakeFeedback()
        { var value = _feedback; _feedback = null; return value; }

        private void Feedback(GestureFeedbackKind kind)
        { if (!_feedbackReported) { _feedback = kind; _feedbackReported = true; } }

        public RouteGesture()
        {
        }

        public void Bind(GestureContext context, double dragThreshold)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (!(dragThreshold > 0.0) || double.IsNaN(dragThreshold) || double.IsInfinity(dragThreshold))
                throw new ArgumentOutOfRangeException(nameof(dragThreshold), "dragThreshold must be positive finite");

            ResetActive();
            _feedback = null; _feedbackReported = false;
            _context = context;
            _threshold = dragThreshold;
        }

        public void Down(PointerSample sample)
        {
            // PointerSample 为值类型不可为 null；无绑定或禁用时无业务输出。
            if (_context == null || !_context.Enabled)
                return;
            if (_active)
                return; // 已有活动，忽略重复/第二指针 Down

            FlowPos? cell = sample.Cell;
            if (!cell.HasValue)
                return; // 棋盘外，不捕获
            FlowPos c = cell.Value;
            if (!InBoard(c))
                return; // 越界，不捕获

            _feedback = null; _feedbackReported = false;

            // 端点优先于自身固化线归类
            for (int i = 0; i < _context.Pairs.Count; i++)
            {
                GesturePair p = _context.Pairs[i];
                if (c.Equals(p.EndpointA) || c.Equals(p.EndpointB))
                {
                    FlowPos target = c.Equals(p.EndpointA) ? p.EndpointB : p.EndpointA;
                    Capture(sample, c, p.PairId, OriginKind.Endpoint, target);
                    return;
                }
            }
            for (int i = 0; i < _context.Pairs.Count; i++)
            {
                GesturePair p = _context.Pairs[i];
                var locked = p.LockedCells;
                for (int j = 0; j < locked.Count; j++)
                {
                    if (c.Equals(locked[j]))
                    {
                        Capture(sample, c, p.PairId, OriginKind.LockedLine, default(FlowPos));
                        return;
                    }
                }
            }
            // 普通空格，不捕获
            Feedback(GestureFeedbackKind.InvalidStart);
        }

        public void Move(PointerSample sample)
        {
            if (!_active || sample.PointerId != _activePointerId)
                return;
            if (_context == null || !_context.Enabled)
                return;

            FlowPos? cell = sample.Cell;
            if (!cell.HasValue || !InBoard(cell.Value))
            {
                // 活动指针越界/棋盘外立即取消
                ResetActive();
                return;
            }
            FlowPos c = cell.Value;

            if (!_thresholdExceeded)
            {
                if (ExceedsThreshold(sample.LocalX, sample.LocalY))
                {
                    _thresholdExceeded = true;
                    _stage = GestureStage.Dragging;
                    _draft.Clear();
                    if (IsDrawableOrigin())
                    {
                        _draft.Add(_originCell);
                        _hasInvalidSample = false;
                    }
                    else
                    {
                        // 不可拖起点越过阈值：只记本地无效，Up 不能退回 Tap
                        _hasInvalidSample = true;
                        Feedback(GestureFeedbackKind.InvalidStart);
                    }
                }
                else
                {
                    // 仍 Pressed，不处理格
                    return;
                }
            }
            ProcessCell(c);
        }

        public GestureIntent Up(PointerSample sample)
        {
            if (!_active || sample.PointerId != _activePointerId)
                return null; // 重复 Up / 无 Down 的 Up / 第二指针 Up
            if (_context == null || !_context.Enabled)
            {
                ResetActive();
                return null;
            }

            FlowPos? cell = sample.Cell;
            if (!cell.HasValue || !InBoard(cell.Value))
            {
                // 越界/棋盘外：取消，无意图
                ResetActive();
                return null;
            }
            FlowPos c = cell.Value;

            // F1: Up 先按与 Move 相同规则处理末样本的阈值位移
            if (!_thresholdExceeded)
            {
                if (ExceedsThreshold(sample.LocalX, sample.LocalY))
                {
                    _thresholdExceeded = true;
                    _stage = GestureStage.Dragging;
                    _draft.Clear();
                    if (IsDrawableOrigin())
                    {
                        _draft.Add(_originCell);
                        _hasInvalidSample = false;
                    }
                    else
                    {
                        _hasInvalidSample = true;
                        Feedback(GestureFeedbackKind.InvalidStart);
                    }
                }
            }

            GestureIntent intent = null;
            if (_thresholdExceeded)
            {
                // Up 先按同一位移/格规则处理最后样本
                ProcessCell(c);
                if (!_hasInvalidSample && _draft.Count >= 2 && _draft[_draft.Count - 1].Equals(_endpointTarget))
                {
                    intent = BuildRouteIntent();
                }
                else Feedback(GestureFeedbackKind.WrongEndpoint);
            }
            else
            {
                // 短按：从未越阈值、Up 仍在原格、且允许历史点按
                if (c.Equals(_originCell) && _context.AllowHistoryTap)
                {
                    intent = new GestureIntent(GestureIntentKind.TapLocator, _context,
                        _originPairId, null, _originCell, _originKind, GestureView.EmptyDraft);
                }
            }
            ResetActive();
            return intent;
        }

        public void Cancel(int? pointerId = null)
        {
            if (!pointerId.HasValue)
            {
                // 失焦/失捕获，全清
                ResetActive();
                return;
            }
            if (_active && pointerId.Value == _activePointerId)
            {
                ResetActive();
            }
            // 非活动指针 Cancel 不影响原手势
        }

        public GestureView Read()
        {
            if (!_active)
            {
                return new GestureView(GestureStage.Idle, null, null, null, null, false);
            }
            return new GestureView(_stage, (int?)_activePointerId, _originPairId,
                (FlowPos?)_originCell, _draft, _hasInvalidSample);
        }

        private void Capture(PointerSample sample, FlowPos cell, string pairId, OriginKind kind, FlowPos target)
        {
            _active = true;
            _activePointerId = sample.PointerId;
            _originCell = cell;
            _originPairId = pairId;
            _originKind = kind;
            _endpointTarget = target;
            _stage = GestureStage.Pressed;
            _startX = sample.LocalX;
            _startY = sample.LocalY;
            _thresholdExceeded = false;
            _draft.Clear();
            _hasInvalidSample = false;
        }

        private void ResetActive()
        {
            _active = false;
            _stage = GestureStage.Idle;
            _draft.Clear();
            _hasInvalidSample = false;
            _thresholdExceeded = false;
            _activePointerId = 0;
            _originPairId = null;
        }

        private bool InBoard(FlowPos c)
        {
            return c.x >= 0 && c.y >= 0 && c.x < _context.Width && c.y < _context.Height;
        }

        // 有限 double 均为 2^-1074 的整数倍；先精确转换再相减、平方，保留严格大于。
        private bool ExceedsThreshold(double localX, double localY)
        {
            BigInteger dx = FiniteUnits(localX) - FiniteUnits(_startX);
            BigInteger dy = FiniteUnits(localY) - FiniteUnits(_startY);
            BigInteger threshold = FiniteUnits(_threshold);
            return dx * dx + dy * dy > threshold * threshold;
        }

        private static BigInteger FiniteUnits(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            int exponent = (int)((bits >> 52) & 0x7ff);
            long significand = (bits & 0x000fffffffffffffL) | (exponent == 0 ? 0L : 0x0010000000000000L);
            BigInteger units = new BigInteger(significand) << (exponent == 0 ? 0 : exponent - 1);
            return bits < 0 ? -units : units;
        }

        private bool IsDrawableOrigin()
        {
            if (_originKind != OriginKind.Endpoint)
                return false;
            GesturePair p = FindOriginPair();
            if (p == null || !p.CanDraw)
                return false;
            if (_context.Mode == GestureMode.Attack)
                return !string.IsNullOrEmpty(_context.SelectedCharacterId);
            return true; // FreeLink 不要求角色
        }

        private GesturePair FindOriginPair()
        {
            for (int i = 0; i < _context.Pairs.Count; i++)
            {
                if (string.Equals(_context.Pairs[i].PairId, _originPairId, StringComparison.Ordinal))
                    return _context.Pairs[i];
            }
            return null;
        }

        private void ProcessCell(FlowPos c)
        {
            // 起点或任何状态下，新增格前先判停馏与退格（合法重访）
            if (_draft.Count > 0 && c.Equals(_draft[_draft.Count - 1]))
            {
                // 停留尾格：若处于无效标志则清除（其余不变），否则本就合法停留
                _hasInvalidSample = false;
                return;
            }
            if (_draft.Count >= 2 && c.Equals(_draft[_draft.Count - 2]))
            {
                // 进入倒数第二格弹尾
                _draft.RemoveAt(_draft.Count - 1);
                _hasInvalidSample = false;
                return;
            }

            // 无效标志期间：除上述停馏/退格外不续画，保留标志
            if (_hasInvalidSample)
                return;

            // 试图追加新格
            if (_draft.Count == 0)
            {
                // 无可拖起点已越过阈值（draft 空 + 无效），不追加
                _hasInvalidSample = true;
                Feedback(GestureFeedbackKind.InvalidStart);
                return;
            }
            FlowPos tail = _draft[_draft.Count - 1];
            // 到配对终点后不能穿过终点续画
            if (tail.Equals(_endpointTarget))
            {
                _hasInvalidSample = true;
                Feedback(GestureFeedbackKind.WrongEndpoint);
                return;
            }
            if (IsAdjacent(tail, c) && !InDraft(c) && !IsOtherEndpoint(c) && !IsAnyLockedCell(c))
            {
                _draft.Add(c);
            }
            else
            {
                // 斜格/跨格/自交/他对端点/固化占格
                _hasInvalidSample = true;
                Feedback(!IsAdjacent(tail, c) ? GestureFeedbackKind.NotAdjacent :
                    InDraft(c) ? GestureFeedbackKind.Crossed : IsOtherEndpoint(c) ? GestureFeedbackKind.WrongEndpoint : GestureFeedbackKind.Crossed);
            }
        }

        private static bool IsAdjacent(FlowPos a, FlowPos b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            if (dx == 0 && dy == 0)
                return false;
            int manhattan = (dx < 0 ? -dx : dx) + (dy < 0 ? -dy : dy);
            return manhattan == 1;
        }

        private bool InDraft(FlowPos c)
        {
            for (int i = 0; i < _draft.Count; i++)
            {
                if (_draft[i].Equals(c))
                    return true;
            }
            return false;
        }

        private bool IsOtherEndpoint(FlowPos c)
        {
            // 配对终点（_endpointTarget）允许到达；其他 Pair 端点及本 Pair 端点 origin 之外的端点属于“他对端点”
            for (int i = 0; i < _context.Pairs.Count; i++)
            {
                GesturePair p = _context.Pairs[i];
                if (c.Equals(p.EndpointA) || c.Equals(p.EndpointB))
                {
                    if (c.Equals(_endpointTarget))
                        return false; // 合法终点
                    return true; // 他对端点
                }
            }
            return false;
        }

        private bool IsAnyLockedCell(FlowPos c)
        {
            for (int i = 0; i < _context.Pairs.Count; i++)
            {
                var locked = _context.Pairs[i].LockedCells;
                for (int j = 0; j < locked.Count; j++)
                {
                    if (locked[j].Equals(c))
                        return true;
                }
            }
            return false;
        }

        private GestureIntent BuildRouteIntent()
        {
            GestureIntentKind kind = _context.Mode == GestureMode.Attack
                ? GestureIntentKind.AttackRoute
                : GestureIntentKind.FreeLinkRoute;
            string characterId = _context.Mode == GestureMode.Attack
                ? _context.SelectedCharacterId
                : null;
            return new GestureIntent(kind, _context, _originPairId, characterId,
                _draft[0], OriginKind.Endpoint, _draft);
        }
    }
}
