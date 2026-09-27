using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using FlowPuzzle.Core;

namespace FightMatch.Input
{
    // 命名行为用例集。暴露只读 CaseIds 与 Run(caseId)；console 与未来 Unity 包装共用同一份期望。
    // 未知 ID 必须失败；零用例或任一失败均非 0 退出。不依赖 NUnit。
    public static class RouteGestureCases
    {
        private static readonly FlowPos[] Empty = new FlowPos[0];
        private static readonly string[] _ids = BuildIds();

        public static IReadOnlyList<string> CaseIds { get { return Array.AsReadOnly(_ids); } }

        public static bool Run(string caseId)
        {
            switch (caseId)
            {
                case "G01A": return G01A();
                case "G01B": return G01B();
                case "G01C": return G01C();
                case "G02": return G02();
                case "G03A": return G03A();
                case "G03B": return G03B();
                case "G03C": return G03C();
                case "G03D": return G03D();
                case "G03E": return G03E();
                case "G03F": return G03F();
                case "G03G": return G03G();
                case "G04A": return G04A();
                case "G04B": return G04B();
                case "G04C": return G04C();
                case "G04D": return G04D();
                case "G04E": return G04E();
                case "G04F": return G04F();
                case "G04G1": return G04G1();
                case "G04G2": return G04G2();
                case "G04G3": return G04G3();
                case "G04H": return G04H();
                case "G04I": return G04I();
                case "G04J": return G04J();
                case "G05A": return G05A();
                case "G05B": return G05B();
                case "G05C": return G05C();
                case "G05D": return G05D();
                case "G05E": return G05E();
                case "G05F": return G05F();
                case "G05G": return G05G();
                case "G05H": return G05H();
                case "G06A": return G06A();
                case "G06B": return G06B();
                case "G06C": return G06C();
                case "G06D": return G06D();
                case "G07A": return G07A();
                case "G07B": return G07B();
                case "G07C": return G07C();
                case "G07D": return G07D();
                case "G07E": return G07E();
                case "G07F": return G07F();
                case "G08A": return G08A();
                case "G08B": return G08B();
                case "G08C": return G08C();
                case "G08D": return G08D();
                case "G08E": return G08E();
                case "G08F": return G08F();
                case "G08G": return G08G();
                case "G08H": return G08H();
                case "G08I": return G08I();
                case "G08J": return G08J();
                case "G08K": return G08K();
                case "G09A": return G09A();
                case "G09B": return G09B();
                case "G09C": return G09C();
                case "G09D": return G09D();
                case "G09E": return G09E();
                case "G09F": return G09F();
                case "G09G": return G09G();
                case "G09H": return G09H();
                case "G09I": return G09I();
                case "G09J": return G09J();
                case "G09K": return G09K();
                case "G09L": return G09L();
                case "G09M": return G09M();
                case "G09N": return G09N();
                case "G09O": return G09O();
                case "G09P": return G09P();
                case "G10A": return G10A();
                case "G10B": return G10B();
                case "G11A": return G11A();
                case "G11B": return G11B();
                case "G11C": return G11C();
                case "G11D": return G11D();
                case "G11E": return G11E();
                case "G12A": return G12A();
                case "G12B": return G12B();
                case "G12C": return G12C();
                case "G15A": return G15A();
                case "G15B": return G15B();
                case "G15C": return ThresholdCase(-1e308, 0, 1e308, 0, 6, true);
                case "G15D": return ThresholdCase(0, 0, double.Epsilon, double.Epsilon, double.Epsilon, true);
                case "G15E": return G15E();
                default: return false;
            }
        }

        private static string[] BuildIds()
        {
            return new string[] {
                "G01A","G01B","G01C","G02","G03A","G03B","G03C","G03D","G03E","G03F","G03G",
                "G04A","G04B","G04C","G04D","G04E","G04F","G04G1","G04G2","G04G3","G04H","G04I","G04J",
                "G05A","G05B","G05C","G05D","G05E","G05F","G05G","G05H",
                "G06A","G06B","G06C","G06D","G07A","G07B","G07C","G07D","G07E","G07F",
                "G08A","G08B","G08C","G08D","G08E","G08F","G08G","G08H","G08I","G08J","G08K",
                "G09A","G09B","G09C","G09D","G09E","G09F","G09G","G09H","G09I","G09J","G09K","G09L","G09M","G09N","G09O","G09P",
                "G10A","G10B",
                "G11A","G11B","G11C","G11D","G11E",
                "G12A","G12B","G12C",
                "G15A","G15B","G15C","G15D","G15E"
            };
        }

        // ---- helpers ----
        private static void Assert(bool cond, string msg)
        {
            if (!cond) throw new InvalidOperationException("ASSERT FAIL: " + msg);
        }
        private static void AssertEq<T>(T actual, T expected, string msg)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
                throw new InvalidOperationException("ASSERT FAIL: " + msg + " | expected=" + expected + " actual=" + actual);
        }
        // 精确异常家族断言：只接受 TException 或其子类，其他异常类型或无异常则失败。
        private static void AssertThrowsExact<TException>(Action action, string msg) where TException : Exception
        {
            try { action(); }
            catch (TException) { return; }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "ASSERT FAIL: expected " + typeof(TException).Name + " but got " + ex.GetType().Name + " | " + msg);
            }
            throw new InvalidOperationException("ASSERT FAIL: expected throw " + typeof(TException).Name + " | " + msg);
        }

        private static GesturePair Pair(string id, int ax, int ay, int bx, int by, bool canDraw, FlowPos[] locked)
        {
            return new GesturePair(id, new FlowPos(ax, ay), new FlowPos(bx, by), canDraw, locked ?? Empty);
        }
        private static GestureContext CtxL(string attempt, long scene, long pref, string face,
            int w, int h, bool en, bool tap, GestureMode mode, string charId, params GesturePair[] pairs)
        {
            return new GestureContext(attempt, new BigInteger(scene), new BigInteger(pref),
                face, w, h, en, tap, mode, charId, pairs);
        }
        private static PointerSample S(int id, double x, double y, int? cx, int? cy)
        {
            FlowPos? cell = (cx.HasValue && cy.HasValue) ? new FlowPos(cx.Value, cy.Value) : (FlowPos?)null;
            return new PointerSample(id, x, y, cell);
        }

        // 标准 5x5：P0 A(0,0)B(3,0) 可画；可选第二对。
        private static GestureContext CtxStd(GestureMode mode, string charId, bool tap, bool enabled)
        {
            return CtxL("att1", 1, 1, "F1", 5, 5, enabled, tap, mode, charId,
                Pair("P0", 0, 0, 3, 0, true, null));
        }

        // ---- G01 完整路线 ----
        private static bool G01A()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 11, 5, 0, 0));      // 距离恰 6，严格不大于 -> 不拖
            Assert(g.Read().Stage == GestureStage.Pressed, "G01A: exact-threshold stays Pressed");
            Assert(g.Read().DraftCells.Count == 0, "G01A: empty draft while Pressed");
            g.Move(S(1, 12, 5, 0, 0));      // 距离 7 > 6 -> 拖，仍停留原格
            Assert(g.Read().Stage == GestureStage.Dragging, "G01A: Dragging after exceed");
            Assert(g.Read().DraftCells.Count == 1, "G01A: draft has origin");
            g.Move(S(1, 13, 5, 1, 0));       // 追加
            g.Move(S(1, 14, 5, 2, 0));      // 追加
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0)); // Up 到终点
            Assert(intent != null && intent.Kind == GestureIntentKind.AttackRoute, "G01A: AttackRoute");
            AssertEq(intent.Cells.Count, 4, "G01A: 4 cells");
            AssertEq(intent.Cells[0], new FlowPos(0, 0), "G01A: c0");
            AssertEq(intent.Cells[1], new FlowPos(1, 0), "G01A: c1");
            AssertEq(intent.Cells[2], new FlowPos(2, 0), "G01A: c2");
            AssertEq(intent.Cells[3], new FlowPos(3, 0), "G01A: c3");
            AssertEq(intent.CharacterId, "W1", "G01A: char W1");
            AssertEq(intent.OriginKind, OriginKind.Endpoint, "G01A: origin endpoint");
            AssertEq(intent.PairId, "P0", "G01A: pair P0");
            return true;
        }
        private static bool G01B()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 3, 0));       // 从 B 端
            g.Move(S(1, 12, 5, 3, 0));      // 超 6 拖
            g.Move(S(1, 13, 5, 2, 0));
            g.Move(S(1, 14, 5, 1, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 0, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.AttackRoute, "G01B: AttackRoute reverse");
            AssertEq(intent.Cells.Count, 4, "G01B: 4 cells");
            AssertEq(intent.Cells[0], new FlowPos(3, 0), "G01B: c0 reverse");
            AssertEq(intent.Cells[1], new FlowPos(2, 0), "G01B: c1 reverse");
            AssertEq(intent.Cells[2], new FlowPos(1, 0), "G01B: c2 reverse");
            AssertEq(intent.Cells[3], new FlowPos(0, 0), "G01B: c3 reverse");
            return true;
        }
        private static bool G01C()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 11, 5, 0, 0));      // 恰 6 不拖
            Assert(g.Read().Stage == GestureStage.Pressed, "G01C: stays Pressed at exact threshold");
            return true;
        }

        // ---- G02 退格 ----
        private static bool G02()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.FreeLink, null, false, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));  // 拖
            g.Move(S(1, 13, 5, 1, 0));  // draft (0,0)(1,0)
            g.Move(S(1, 14, 5, 2, 0));  // draft (0,0)(1,0)(2,0)
            g.Move(S(1, 13, 5, 1, 0));  // 退格 -> (0,0)(1,0)
            Assert(g.Read().DraftCells.Count == 2 && !g.Read().HasInvalidSample, "G02: backspace clean");
            g.Move(S(1, 12, 5, 0, 0));  // 退格 -> (0,0)
            Assert(g.Read().DraftCells.Count == 1, "G02: back to origin only");
            // 第二段
            g.Move(S(1, 5, 6, 0, 1));
            g.Move(S(1, 6, 6, 1, 1));
            g.Move(S(1, 7, 6, 2, 1));
            g.Move(S(1, 8, 6, 3, 1));
            GestureIntent intent = g.Up(S(1, 8, 5, 3, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.FreeLinkRoute, "G02: FreeLinkRoute");
            AssertEq(intent.Cells.Count, 6, "G02: 6 cells");
            AssertEq(intent.Cells[0], new FlowPos(0, 0), "G02: c0");
            AssertEq(intent.Cells[1], new FlowPos(0, 1), "G02: c1");
            AssertEq(intent.Cells[2], new FlowPos(1, 1), "G02: c2");
            AssertEq(intent.Cells[3], new FlowPos(2, 1), "G02: c3");
            AssertEq(intent.Cells[4], new FlowPos(3, 1), "G02: c4");
            AssertEq(intent.Cells[5], new FlowPos(3, 0), "G02: c5");
            Assert(intent.CharacterId == null, "G02: char null");
            return true;
        }

        // ---- G03 点按分支 ----
        private static bool G03A()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            GestureIntent intent = g.Up(S(1, 5, 5, 0, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.TapLocator, "G03A: TapLocator on endpoint");
            AssertEq(intent.OriginKind, OriginKind.Endpoint, "G03A: endpoint origin");
            AssertEq(intent.PairId, "P0", "G03A: pair P0");
            Assert(intent.Cells.Count == 0, "G03A: empty cells");
            Assert(intent.CharacterId == null, "G03A: char null for tap");
            return true;
        }
        private static bool G03B()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, false, new FlowPos[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) })), 6.0);
            g.Down(S(1, 5, 5, 1, 0));      // 固化线中段
            GestureIntent intent = g.Up(S(1, 5, 5, 1, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.TapLocator, "G03B: TapLocator on locked line");
            AssertEq(intent.OriginKind, OriginKind.LockedLine, "G03B: locked origin");
            return true;
        }
        private static bool G03C()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            GestureView v = g.Read();
            Assert(v.Stage == GestureStage.Pressed, "G03C: Down -> Pressed, no intent");
            Assert(v.DraftCells.Count == 0, "G03C: empty draft after Down");
            return true;
        }
        private static bool G03D()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));  // 超 6 拖
            g.Move(S(1, 5, 5, 0, 0));   // 缩回原点
            GestureIntent intent = g.Up(S(1, 5, 5, 0, 0));
            Assert(intent == null, "G03D: exceed then return -> no Tap");
            return true;
        }
        private static bool G03E()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 6, 5, 1, 0));   // 距离 1 未拖，但落不同格
            GestureIntent intent = g.Up(S(1, 6, 5, 1, 0));
            Assert(intent == null, "G03E: Up on different cell, no Tap");
            return true;
        }
        private static bool G03F()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", false, true), 6.0); // allowHistoryTap=false
            g.Down(S(1, 5, 5, 0, 0));
            GestureIntent intent = g.Up(S(1, 5, 5, 0, 0));
            Assert(intent == null, "G03F: allowHistoryTap=false -> no Tap");
            return true;
        }
        private static bool G03G()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, false, new FlowPos[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) })), 6.0);
            g.Down(S(1, 5, 5, 1, 0));      // 固化线中段
            g.Move(S(1, 12, 5, 1, 0));      // 超 6，但固化不可拖
            Assert(g.Read().HasInvalidSample, "G03G: locked line exceed -> invalid");
            Assert(g.Read().DraftCells.Count == 0, "G03G: no draft from locked");
            GestureIntent intent = g.Up(S(1, 13, 5, 1, 0));
            Assert(intent == null, "G03G: no route from locked line");
            return true;
        }

        // ---- G04 非法格 ----
        private static RouteGesture DragSetup(out GestureContext ctx)
        {
            ctx = CtxStd(GestureMode.Attack, "W1", true, true);
            var g = new RouteGesture();
            g.Bind(ctx, 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));  // 拖，draft=(0,0)
            return g;
        }
        private static bool G04A()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 6, 1, 1));   // 斜格
            Assert(g.Read().HasInvalidSample, "G04A: diagonal invalid");
            Assert(g.Read().DraftCells.Count == 1, "G04A: prefix kept");
            return true;
        }
        private static bool G04B()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 2, 0));   // 跨格（曼哈顿 2）
            Assert(g.Read().HasInvalidSample, "G04B: jump invalid");
            Assert(g.Read().DraftCells.Count == 1, "G04B: prefix kept");
            return true;
        }
        private static bool G04C()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0));   // (0,0)(1,0)
            g.Move(S(1, 14, 5, 2, 0));   // (0,0)(1,0)(2,0)
            g.Move(S(1, 5, 5, 0, 0));    // 自交首格（非尾非倒数第二）
            Assert(g.Read().HasInvalidSample, "G04C: self-intersect invalid");
            Assert(g.Read().DraftCells.Count == 3, "G04C: prefix kept");
            return true;
        }
        private static bool G04D()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null),
                Pair("P1", 0, 1, 3, 1, true, null)), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));   // 拖
            g.Move(S(1, 5, 6, 0, 1));    // P1 端点，相邻 -> 他对端点
            Assert(g.Read().HasInvalidSample, "G04D: other pair endpoint invalid");
            Assert(g.Read().DraftCells.Count == 1, "G04D: prefix kept");
            return true;
        }
        private static bool G04E()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null),
                Pair("P1", 0, 1, 0, 3, false, new FlowPos[] { new FlowPos(0, 1), new FlowPos(0, 2), new FlowPos(0, 3) })), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));   // 拖
            g.Move(S(1, 5, 6, 0, 1));    // P1 固化格，相邻
            Assert(g.Read().HasInvalidSample, "G04E: locked cell invalid");
            return true;
        }
        private static bool G04F()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            g.Move(S(1, 15, 5, 3, 0));   // 到终点 draft=(0,0)(1,0)(2,0)(3,0)
            g.Move(S(1, 15, 6, 3, 1));   // 穿过终点续画
            Assert(g.Read().HasInvalidSample, "G04F: cross-endpoint invalid");
            Assert(g.Read().DraftCells.Count == 4, "G04F: prefix kept at endpoint");
            return true;
        }
        private static bool G04G1()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 6, 1, 1));   // 斜，置无效
            Assert(g.Read().HasInvalidSample, "G04G1: invalid set");
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0)); // 未恢复，终点 Up
            Assert(intent == null, "G04G1: not recovered -> no route");
            return true;
        }
        private static bool G04G2()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0));   // draft (0,0)(1,0)
            g.Move(S(1, 14, 5, 2, 0));   // draft (0,0)(1,0)(2,0)
            g.Move(S(1, 13, 6, 1, 1));   // (1,1) 跨格 -> 无效，draft 保持
            Assert(g.Read().HasInvalidSample, "G04G2: invalid set before recovery");
            g.Move(S(1, 13, 5, 1, 0));   // 回倒数第二 -> 退格 + 清除
            Assert(!g.Read().HasInvalidSample, "G04G2: recovered by backspace");
            Assert(g.Read().DraftCells.Count == 2, "G04G2: backspace applied");
            g.Move(S(1, 14, 5, 2, 0));   // 再追加
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.AttackRoute, "G04G2: route after recovery");
            return true;
        }
        // N2: 原点即尾格的无效后回尾恢复。draft 仅含原点 (0,0)，斜格无效后回到原点应清除标志。
        private static bool G04G3()
        {
            var g = DragSetup(out var ctx); // draft=(0,0)，tail 即 origin
            g.Move(S(1, 13, 6, 1, 1));   // 斜格 -> 无效，draft 保持 [(0,0)]
            Assert(g.Read().HasInvalidSample, "G04G3: invalid set");
            Assert(g.Read().DraftCells.Count == 1, "G04G3: prefix kept");
            g.Move(S(1, 5, 5, 0, 0));    // 回尾格（即原点）-> 清除
            Assert(!g.Read().HasInvalidSample, "G04G3: recovered by returning to tail=origin");
            Assert(g.Read().DraftCells.Count == 1, "G04G3: draft still has origin");
            return true;
        }
        // N2: 普通尾格（非原点）的无效后回尾恢复。draft=[(0,0),(1,0)]，斜格无效后回到 (1,0) 应清除标志。
        private static bool G04H()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0));   // draft [(0,0),(1,0)]，tail=(1,0) 非原点
            g.Move(S(1, 5, 6, 0, 1));    // (0,1) 斜格（manhattan=2 from (1,0)）-> 无效
            Assert(g.Read().HasInvalidSample, "G04H: invalid set");
            Assert(g.Read().DraftCells.Count == 2, "G04H: prefix kept");
            g.Move(S(1, 13, 5, 1, 0));    // 回尾格 (1,0) -> 清除
            Assert(!g.Read().HasInvalidSample, "G04H: recovered by returning to tail");
            Assert(g.Read().DraftCells.Count == 2, "G04H: draft unchanged after tail recovery");
            return true;
        }
        // N2: 独立横竖相邻自交（同时不是跨格、不是退格、不是他对端点）。
        // 路径 (0,0)->(1,0)->(1,1)->(0,1)，再采样 (0,0)：相邻（manhattan=1）且已访问、非尾非倒数第二 -> 自交。
        private static bool G04I()
        {
            var g = DragSetup(out var ctx); // draft=[(0,0)]
            g.Move(S(1, 13, 5, 1, 0));   // draft [(0,0),(1,0)]
            g.Move(S(1, 13, 6, 1, 1));   // draft [(0,0),(1,0),(1,1)]
            g.Move(S(1, 5, 6, 0, 1));    // draft [(0,0),(1,0),(1,1),(0,1)]
            g.Move(S(1, 5, 5, 0, 0));    // (0,0) 相邻且已访问（非尾非倒数第二）-> 自交
            Assert(g.Read().HasInvalidSample, "G04I: adjacent self-intersect invalid");
            Assert(g.Read().DraftCells.Count == 4, "G04I: prefix kept");
            return true;
        }
        // N2: 独立固化线路中段占格（同时不是端点）。P1 固化 [(0,1),(0,2),(0,3)]，
        // (0,2) 是中段非端点。路径 (0,0)->(1,0)->(1,1)->(1,2)->采样 (0,2)：相邻且固化 -> 无效。
        private static bool G04J()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null),
                Pair("P1", 0, 1, 0, 3, false, new FlowPos[] { new FlowPos(0, 1), new FlowPos(0, 2), new FlowPos(0, 3) })), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));   // 拖，draft=[(0,0)]
            g.Move(S(1, 13, 5, 1, 0));   // draft [(0,0),(1,0)]
            g.Move(S(1, 13, 6, 1, 1));   // draft [(0,0),(1,0),(1,1)]
            g.Move(S(1, 13, 7, 1, 2));   // draft [(0,0),(1,0),(1,1),(1,2)]
            g.Move(S(1, 5, 7, 0, 2));    // (0,2) 是 P1 固化中段（非端点），相邻 -> 无效
            Assert(g.Read().HasInvalidSample, "G04J: locked mid-cell invalid");
            Assert(g.Read().DraftCells.Count == 4, "G04J: prefix kept");
            return true;
        }

        // ---- G05 取消 ----
        private static bool G05A()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 20, 20, null, null)); // 离开棋盘
            Assert(g.Read().Stage == GestureStage.Idle, "G05A: cancelled on out-of-board");
            Assert(g.Up(S(1, 21, 21, 0, 0)) == null, "G05A: no intent after cancel");
            return true;
        }
        private static bool G05B()
        {
            var g = DragSetup(out var ctx);
            g.Cancel(null);
            Assert(g.Read().Stage == GestureStage.Idle, "G05B: Cancel(null) clears");
            Assert(g.Up(S(1, 5, 5, 0, 0)) == null, "G05B: no intent after cancel");
            return true;
        }
        private static bool G05C()
        {
            var g = DragSetup(out var ctx);
            g.Cancel(1);
            Assert(g.Read().Stage == GestureStage.Idle, "G05C: Cancel(id) clears active");
            return true;
        }
        private static bool G05D()
        {
            var g = DragSetup(out var ctx);
            g.Cancel(2); // 他指针
            Assert(g.Read().Stage == GestureStage.Dragging, "G05D: Cancel(other) no effect");
            return true;
        }
        private static bool G05E()
        {
            var g = DragSetup(out var ctx);
            g.Bind(CtxL("att2", 2, 2, "F2", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G05E: Bind cancels in-progress");
            return true;
        }
        private static bool G05F()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 20, 20, null, null)); // 取消
            g.Move(S(1, 13, 5, 1, 0));        // 回板
            Assert(g.Read().Stage == GestureStage.Idle, "G05F: no revive after cancel");
            return true;
        }
        private static bool G05G()
        {
            var g = DragSetup(out var ctx);
            g.Cancel(null);
            Assert(g.Up(S(1, 5, 5, 0, 0)) == null, "G05G: stale Up no intent");
            return true;
        }
        private static bool G05H()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0));  // draft (0,0)(1,0)，未到终点
            GestureIntent intent = g.Up(S(1, 13, 5, 1, 0)); // 停留尾，非终点
            Assert(intent == null, "G05H: release before endpoint -> no intent");
            return true;
        }

        // ---- G06 防双发 ----
        private static bool G06A()
        {
            var g = DragSetup(out var ctx);
            g.Down(S(1, 9, 9, 2, 0)); // 重复 Down 同指针
            Assert(g.Read().ActivePointerId == 1 && g.Read().DraftCells.Count == 1, "G06A: repeat Down no reset");
            g.Down(S(2, 9, 9, 2, 0)); // 第二指针
            Assert(g.Read().ActivePointerId == 1, "G06A: second pointer ignored");
            return true;
        }
        private static bool G06B()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent i1 = g.Up(S(1, 15, 5, 3, 0));
            GestureIntent i2 = g.Up(S(1, 15, 5, 3, 0));
            Assert(i1 != null, "G06B: first Up intent");
            Assert(i2 == null, "G06B: repeat Up no intent");
            return true;
        }
        private static bool G06C()
        {
            var g = DragSetup(out var ctx);
            g.Down(S(2, 9, 9, 1, 0));   // 第二指针 Down
            g.Move(S(2, 9, 9, 1, 0));   // 第二指针 Move
            g.Up(S(2, 9, 9, 1, 0));     // 第二指针 Up
            g.Cancel(2);                // 第二指针 Cancel
            Assert(g.Read().ActivePointerId == 1 && g.Read().Stage == GestureStage.Dragging, "G06C: second pointer no interference");
            return true;
        }
        private static bool G06D()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            g.Up(S(1, 15, 5, 3, 0));
            // 新手势
            g.Down(S(1, 5, 5, 0, 0));
            Assert(g.Read().Stage == GestureStage.Pressed, "G06D: new Down starts new gesture");
            return true;
        }

        // ---- G07 阶段/角色 ----
        private static bool G07A()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, null, true, true), 6.0); // Attack 无角色
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));  // 超 6
            Assert(g.Read().HasInvalidSample, "G07A: Attack no char -> invalid");
            Assert(g.Up(S(1, 15, 5, 3, 0)) == null, "G07A: no route");
            return true;
        }
        private static bool G07B()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, false, null)), 6.0); // canDraw=false 无固化
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            Assert(g.Read().HasInvalidSample, "G07B: non-drawable pair -> invalid");
            Assert(g.Up(S(1, 15, 5, 3, 0)) == null, "G07B: no route");
            return true;
        }
        private static bool G07C()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, false), 6.0); // enabled=false
            g.Down(S(1, 5, 5, 0, 0));
            Assert(g.Read().Stage == GestureStage.Idle, "G07C: disabled no capture");
            Assert(g.Up(S(1, 5, 5, 0, 0)) == null, "G07C: no intent when disabled");
            return true;
        }
        private static bool G07D()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.FreeLink, null, true, true), 6.0); // FreeLink 无角色
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.FreeLinkRoute, "G07D: FreeLink completable without char");
            return true;
        }
        private static bool G07E()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.FreeLink, null, true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            Assert(intent.CharacterId == null, "G07E: FreeLink CharacterId always null");
            return true;
        }
        private static bool G07F()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.FreeLink, null,
                Pair("P0", 0, 0, 3, 0, false, null)), 6.0); // canDraw=false
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            Assert(g.Read().HasInvalidSample, "G07F: non-drawable cannot use FreeLink");
            Assert(g.Up(S(1, 15, 5, 3, 0)) == null, "G07F: no route");
            return true;
        }

        // ---- G08 版本与隔离 ----
        private static RouteGesture G08Setup(out GestureContext ctx)
        {
            ctx = CtxStd(GestureMode.Attack, "W1", true, true);
            var g = new RouteGesture();
            g.Bind(ctx, 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            return g;
        }
        private static bool G08A()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxL("att2", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G08A: attempt change cancels");
            return true;
        }
        private static bool G08B()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxL("att1", 1, 1, "F2", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G08B: face change cancels");
            return true;
        }
        private static bool G08C()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxL("att1", 2, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G08C: scene revision change cancels");
            return true;
        }
        private static bool G08D()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxL("att1", 1, 2, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G08D: preference revision change cancels");
            return true;
        }
        private static bool G08E()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxStd(GestureMode.Attack, "M1", true, true), 6.0);
            Assert(g.Read().Stage == GestureStage.Idle, "G08E: character change cancels");
            return true;
        }
        private static bool G08F()
        {
            var g = G08Setup(out var ctx);
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, false), 6.0); // enabled=false
            Assert(g.Read().Stage == GestureStage.Idle, "G08F: enabled=false cancels");
            return true;
        }
        private static bool G08G()
        {
            BigInteger big = BigInteger.Parse("9007199254740993"); // 2^53 + 1
            var ctx = new GestureContext("att1", big, new BigInteger(1), "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                new GesturePair[] { Pair("P0", 0, 0, 3, 0, true, null) });
            var g = new RouteGesture();
            g.Bind(ctx, 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            Assert(intent != null, "G08G: route with big revision");
            AssertEq(intent.Context.SceneRevision, big, "G08G: big revision preserved");
            return true;
        }
        private static bool G08H()
        {
            var list = new List<GesturePair> { Pair("P0", 0, 0, 3, 0, true, null) };
            var ctx = new GestureContext("att1", new BigInteger(1), new BigInteger(1), "F1", 5, 5, true, true, GestureMode.Attack, "W1", list);
            list.Add(Pair("P1", 0, 1, 3, 1, true, null)); // 改原 List
            AssertEq(ctx.Pairs.Count, 1, "G08H: original list change does not affect context");
            return true;
        }
        private static bool G08I()
        {
            var g = DragSetup(out var ctx);
            g.Move(S(1, 13, 5, 1, 0)); // draft (0,0)(1,0)
            GestureView v = g.Read();
            g.Move(S(1, 14, 5, 2, 0)); // 继续追加
            AssertEq(v.DraftCells.Count, 2, "G08I: old Read snapshot preserved");
            return true;
        }
        private static bool G08J()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            g.Bind(CtxL("att2", 2, 2, "F2", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null)), 6.0);
            AssertEq(intent.Cells.Count, 4, "G08J: old Intent preserved after Bind");
            return true;
        }
        private static bool G08K()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 5, 5, 0, 0));
            g.Move(S(1, 12, 5, 0, 0));
            g.Move(S(1, 13, 5, 1, 0));
            g.Move(S(1, 14, 5, 2, 0));
            GestureIntent intent = g.Up(S(1, 15, 5, 3, 0));
            var asList = intent.Cells as IList<FlowPos>;
            Assert(asList != null, "G08K: cells is IList");
            bool threw = false;
            try { asList.Add(default(FlowPos)); }
            catch (NotSupportedException) { threw = true; }
            Assert(threw, "G08K: output collection not writable");
            return true;
        }

        // ---- G09 坏输入 ----
        private static bool G09A()
        {
            var g = new RouteGesture();
            AssertThrowsExact<ArgumentException>(() => g.Bind(null, 6.0), "G09A: Bind null context");
            return true;
        }
        private static bool G09B()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            AssertThrowsExact<ArgumentException>(() => g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 0.0), "G09B: zero threshold");
            return true;
        }
        private static bool G09C()
        {
            var g = new RouteGesture();
            AssertThrowsExact<ArgumentException>(() => g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), double.NaN), "G09C: NaN threshold");
            return true;
        }
        private static bool G09D()
        {
            var g = new RouteGesture();
            AssertThrowsExact<ArgumentException>(() => g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), double.PositiveInfinity), "G09D: Infinity threshold");
            return true;
        }
        private static bool G09E()
        {
            AssertThrowsExact<ArgumentException>(() => CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null), Pair("P0", 0, 1, 3, 1, true, null)), "G09E: duplicate pairId");
            return true;
        }
        private static bool G09F()
        {
            AssertThrowsExact<ArgumentException>(() => CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 3, 0, true, null), Pair("P1", 0, 0, 3, 1, true, null)), "G09F: overlapping endpoints");
            return true;
        }
        private static bool G09G()
        {
            AssertThrowsExact<ArgumentException>(() => CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 9, 0, true, null)), "G09G: out-of-board endpoint");
            return true;
        }
        private static bool G09H()
        {
            AssertThrowsExact<ArgumentException>(() => new GestureContext("att1", new BigInteger(1), new BigInteger(1), "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                new GesturePair[] {
                    Pair("P0", 0, 0, 3, 0, false, new FlowPos[] { new FlowPos(0, 0), new FlowPos(1, 0), new FlowPos(2, 0), new FlowPos(3, 0) }),
                    Pair("P1", 0, 1, 0, 3, false, new FlowPos[] { new FlowPos(0, 1), new FlowPos(1, 0), new FlowPos(0, 3) })
                }), "G09H: overlapping locked cells");
            return true;
        }
        private static bool G09I()
        {
            AssertThrowsExact<ArgumentException>(() => new PointerSample(1, double.NaN, 0, new FlowPos(0, 0)), "G09I: NaN localX");
            return true;
        }
        private static bool G09J()
        {
            AssertThrowsExact<ArgumentException>(() => new PointerSample(1, 0, double.PositiveInfinity, new FlowPos(0, 0)), "G09J: Infinity localY");
            return true;
        }
        private static bool G09K()
        {
            AssertThrowsExact<ArgumentException>(() => new GesturePair("P0", new FlowPos(0, 0), new FlowPos(3, 0), false,
                new FlowPos[] { new FlowPos(0, 0), new FlowPos(0, 0), new FlowPos(3, 0) }), "G09K: duplicate locked cell");
            return true;
        }
        private static bool G09L()
        {
            AssertThrowsExact<ArgumentException>(() => new GesturePair("P0", new FlowPos(0, 0), new FlowPos(3, 0), false,
                new FlowPos[] { new FlowPos(0, 0), new FlowPos(1, 0) }), "G09L: non-empty locked missing endpoint");
            return true;
        }
        private static bool G09M()
        {
            AssertThrowsExact<ArgumentException>(() => new GesturePair("P0", new FlowPos(0, 0), new FlowPos(3, 0), true,
                new FlowPos[] { new FlowPos(0, 0), new FlowPos(3, 0) }), "G09M: locked with canDraw=true");
            return true;
        }
        private static bool G09N()
        {
            var g = DragSetup(out var ctx); // 已合法 Bind 并拖动
            AssertThrowsExact<ArgumentException>(() => g.Bind(null, 6.0), "G09N: illegal Bind throws");
            Assert(g.Read().Stage == GestureStage.Dragging, "G09N: illegal Bind keeps prior state");
            return true;
        }
        // F3: pairs 含 null 元素时必须按参数异常家族拒绝，不得泄漏 NullReferenceException。
        private static bool G09O()
        {
            AssertThrowsExact<ArgumentException>(() => new GestureContext("att1", new BigInteger(1), new BigInteger(1), "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                new GesturePair[] { null }), "G09O: null Pair must throw ArgumentException");
            return true;
        }
        // F4: 非法 GestureMode（如 99）必须在构造时拒绝，不得默入 FreeLink 分支。
        private static bool G09P()
        {
            AssertThrowsExact<ArgumentException>(() => new GestureContext("att1", new BigInteger(1), new BigInteger(1), "F1", 5, 5, true, true, (GestureMode)99, "W1",
                new GesturePair[] { Pair("P0", 0, 0, 3, 0, true, null) }), "G09P: invalid Mode must throw ArgumentException");
            return true;
        }

        // ---- G10 独立性与证据 ----
        private static bool G10A()
        {
            Assert(typeof(RouteGesture).Namespace == "FightMatch.Input", "G10A: RouteGesture namespace");
            Assert(typeof(GestureContext).Namespace == "FightMatch.Input", "G10A: GestureContext namespace");
            Assert(typeof(GestureIntent).Namespace == "FightMatch.Input", "G10A: GestureIntent namespace");
            return true;
        }
        private static bool G10B()
        {
            // 验证模块只读引用 FlowPos（值类型来自 FlowPuzzle.Core）：构造 FlowPos 并经合法端点捕获后回读。
            FlowPos endpoint = new FlowPos(0, 0);
            var ctx = CtxStd(GestureMode.Attack, "W1", true, true);
            var g = new RouteGesture();
            g.Bind(ctx, 6.0);
            g.Down(new PointerSample(1, 5, 5, endpoint));
            var v = g.Read();
            Assert(v.Stage == GestureStage.Pressed, "G10B: captured via FlowPos endpoint");
            Assert(v.OriginCell.HasValue && v.OriginCell.Value.Equals(endpoint), "G10B: FlowPos from FlowPuzzle.Core echoed");
            return true;
        }

        // ---- G11 F1: Up 末样本位移处理 ----
        // F1 反例：Down 后无 Move，Up 本身越阈值但原代码不处理末样本位移 -> 误发 Tap。
        private static bool G11A()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 0, 0, 0, 0));       // 局部 (0,0)，cell (0,0)
            // 不发 Move
            GestureIntent intent = g.Up(S(1, 7, 0, 0, 0)); // Up 局部 (7,0) 距离 7 > 6，cell 仍 (0,0)
            Assert(intent == null, "G11A: Up exceeds threshold -> no Tap");
            return true;
        }
        // F1 反例：相邻两端点，最后一格只在 Up 出现 -> 原代码丢路线。
        private static bool G11B()
        {
            var g = new RouteGesture();
            g.Bind(CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 1, 0, true, null)), 6.0); // 相邻端点 (0,0)-(1,0)
            g.Down(S(1, 0, 0, 0, 0));       // 局部 (0,0)，cell (0,0)
            // 不发 Move，Up 直接到 (1,0) 且局部 (7,0) 超阈值
            GestureIntent intent = g.Up(S(1, 7, 0, 1, 0));
            Assert(intent != null && intent.Kind == GestureIntentKind.AttackRoute, "G11B: Up route to adjacent endpoint");
            AssertEq(intent.Cells.Count, 2, "G11B: 2 cells");
            AssertEq(intent.Cells[0], new FlowPos(0, 0), "G11B: c0");
            AssertEq(intent.Cells[1], new FlowPos(1, 0), "G11B: c1");
            return true;
        }
        // F1 对照：Up 恰阈值仍 Pressed，不拖；同格且允许点按 -> TapLocator（验证 F1 修不误触发 Dragging）。
        private static bool G11C()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 0, 0, 0, 0));
            GestureIntent intent = g.Up(S(1, 6, 0, 0, 0)); // 恰 6，严格不大于 -> 未越阈值
            Assert(intent != null && intent.Kind == GestureIntentKind.TapLocator, "G11C: exact threshold -> Tap not Dragging");
            return true;
        }
        // F1 对照：异指针 Up 不干扰主指针。
        private static bool G11D()
        {
            var g = DragSetup(out var ctx); // 主指针 1 拖动中
            GestureIntent intent = g.Up(S(2, 15, 5, 3, 0)); // 异指针 Up
            Assert(intent == null, "G11D: other pointer Up no intent");
            Assert(g.Read().ActivePointerId == 1, "G11D: main pointer still active");
            return true;
        }
        // F1 对照：重复 Up 只得一个草案。
        private static bool G11E()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 6.0);
            g.Down(S(1, 0, 0, 0, 0));
            g.Up(S(1, 0, 0, 0, 0)); // 第一次 Up（短按，Tap）
            GestureIntent i2 = g.Up(S(1, 0, 0, 0, 0)); // 重复 Up
            Assert(i2 == null, "G11E: repeat Up no intent");
            return true;
        }

        // ---- G12 F2: 输出构造可见性与集合冻结 ----
        // F2 反例：GestureIntent 构造器当前 public -> 应收窄为 internal。
        private static bool G12A()
        {
            var ctor = typeof(GestureIntent).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(GestureIntentKind), typeof(GestureContext), typeof(string),
                        typeof(string), typeof(FlowPos), typeof(OriginKind), typeof(IReadOnlyList<FlowPos>) },
                null);
            Assert(ctor != null, "G12A: Intent ctor found via reflection");
            Assert(!ctor.IsPublic, "G12A: Intent ctor must not be public");
            return true;
        }
        // F2 反例：Intent 构造后修改源集合 -> Cells 不应变。
        private static bool G12B()
        {
            var ctx = CtxStd(GestureMode.Attack, "W1", true, true);
            var src = new List<FlowPos> { new FlowPos(0, 0), new FlowPos(1, 0) };
            var intent = new GestureIntent(GestureIntentKind.AttackRoute, ctx, "P0", "W1",
                new FlowPos(0, 0), OriginKind.Endpoint, src);
            AssertEq(intent.Cells.Count, 2, "G12B: cells count before mutation");
            src.Clear(); // 修改源集合
            AssertEq(intent.Cells.Count, 2, "G12B: cells count unchanged after source mutation");
            return true;
        }
        // F2 反例：View 构造后修改源集合 -> DraftCells 不应变。
        private static bool G12C()
        {
            var src = new List<FlowPos> { new FlowPos(0, 0) };
            var view = new GestureView(GestureStage.Dragging, 1, "P0", new FlowPos(0, 0), src, false);
            AssertEq(view.DraftCells.Count, 1, "G12C: draft count before mutation");
            src.Clear();
            AssertEq(view.DraftCells.Count, 1, "G12C: draft count unchanged after source mutation");
            return true;
        }

        // ---- G15 F5: 极端有限 double 阈值比较 ----
        // F5 反例 A：阈值 1e200，Move 距离恰 1e200（平方上溢为 Infinity > 1e200 -> 误判 Dragging）。
        private static bool G15A()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 1e200);
            g.Down(S(1, 0, 0, 0, 0));
            g.Move(S(1, 1e200, 0, 0, 0)); // 同格，距离恰等于阈值
            Assert(g.Read().Stage == GestureStage.Pressed, "G15A: equal large finite threshold stays Pressed");
            return true;
        }
        // F5 反例 B：阈值 1e-200，Move 距离 2e-200（平方下溢为 0 < 1e-200 -> 误判 Pressed）。
        private static bool G15B()
        {
            var g = new RouteGesture();
            g.Bind(CtxStd(GestureMode.Attack, "W1", true, true), 1e-200);
            g.Down(S(1, 0, 0, 0, 0));
            g.Move(S(1, 2e-200, 0, 0, 0)); // 同格，距离 2e-200 > 阈值 1e-200
            Assert(g.Read().Stage == GestureStage.Dragging, "G15B: small finite exceed enters Dragging");
            Assert(g.Read().DraftCells.Count == 1, "G15B: draft has origin");
            return true;
        }

        // 每个数值见证分别经过 Move、原格 Up 和相邻终点 Up，避免只验证一个入口。
        private static bool ThresholdCase(double startX, double startY, double x, double y, double threshold, bool exceeds)
        {
            var ctx = CtxL("att1", 1, 1, "F1", 5, 5, true, true, GestureMode.Attack, "W1",
                Pair("P0", 0, 0, 1, 0, true, null));
            var g = new RouteGesture();
            g.Bind(ctx, threshold);
            g.Down(S(1, startX, startY, 0, 0));
            g.Move(S(1, x, y, 0, 0));
            AssertEq(g.Read().Stage, exceeds ? GestureStage.Dragging : GestureStage.Pressed, "G15: Move threshold");
            g.Bind(ctx, threshold);
            g.Down(S(1, startX, startY, 0, 0));
            var tap = g.Up(S(1, x, y, 0, 0));
            Assert(exceeds ? tap == null : tap != null && tap.Kind == GestureIntentKind.TapLocator, "G15: same-cell Up");
            g.Down(S(1, startX, startY, 0, 0));
            var route = g.Up(S(1, x, y, 1, 0));
            if (!exceeds) Assert(route == null, "G15: below/equal threshold cannot route");
            else
            {
                Assert(route != null && route.Kind == GestureIntentKind.AttackRoute, "G15: adjacent Up route");
                AssertEq(route.Cells.Count, 2, "G15: complete two-cell route");
                AssertEq(route.Cells[0], new FlowPos(0, 0), "G15: route origin");
                AssertEq(route.Cells[1], new FlowPos(1, 0), "G15: route endpoint");
            }
            return true;
        }
        private static bool G15E()
        {
            foreach (double t in new[] { double.Epsilon, 1e-200, 6.0, 1e200, double.MaxValue })
            {
                ThresholdCase(0, 0, t, 0, t, false);
                if (t < double.MaxValue) ThresholdCase(0, 0, BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(t) + 1), 0, t, true);
                ThresholdCase(0, 0, t, double.Epsilon, t, true);
            }
            foreach (double scale in new[] { double.Epsilon, Math.Pow(2, -1022), 1.0, Math.Pow(2, 1018) })
            {
                ThresholdCase(0, 0, 3 * scale, 4 * scale, 5 * scale, false);
                ThresholdCase(0, 0, 5 * scale, 12 * scale, 13 * scale, false);
            }
            ThresholdCase(-double.Epsilon, 0, double.MaxValue, 0, double.MaxValue, true);
            ThresholdCase(double.MaxValue, 0, double.MaxValue, 0, double.Epsilon, false);
            ThresholdCase(0, 1e308, 0, -1e308, double.MaxValue, true);
            return true;
        }
    }
}
