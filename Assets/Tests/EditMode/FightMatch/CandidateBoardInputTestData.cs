using System;
using System.Collections;
using System.Collections.Generic;
using FightMatch.Presentation;
using FlowPuzzle.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using PointerType = UnityEngine.UIElements.PointerType;
using static FightMatch.Core.Tests.LocalSaveTestFiles;

namespace FightMatch.Core.Tests
{
    internal sealed class CandidateBoardTestWindow : EditorWindow { }

    // Programmatic panel events, not physical mouse/touch or a formal scene.
    internal sealed class BoardPanelRig : IDisposable
    {
        private static readonly List<BoardPanelRig> open = new List<BoardPanelRig>();
        internal readonly BattleApplicationRig Battle;
        internal readonly CandidateBoardTestWindow Window;
        internal readonly CandidateBoardInputView UI;
        internal readonly Button Peer = new Button { text = "Focus/capture peer" };
        internal CandidateBoardInputController Controller => UI.Controller;
        internal CandidateBoardElement Board => UI.Board;
        internal float CellSize = 40;

        internal BoardPanelRig(int level = 1, int hp = 100)
        {
            Battle = new BattleApplicationRig(hp: hp, level: level);
            try
            {
                Window = ScriptableObject.CreateInstance<CandidateBoardTestWindow>();
                Window.position = new Rect(50, 50, 700, 850);
                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                {
                    // The native window cannot draw under -nographics; the real managed panel is still required below.
                    LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                    LogAssert.Expect(LogType.Error, "No graphic device is available to show the window.");
                    LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                }
                Window.Show();
                Window.rootVisualElement.style.width = 700; Window.rootVisualElement.style.height = 850;
                UI = new CandidateBoardInputView(); Window.rootVisualElement.Add(UI); Window.rootVisualElement.Add(Peer);
                UI.Attach(Battle.System, B()); UI.style.flexGrow = 0; UI.style.width = 600;
                Resize(CellSize);
                open.Add(this);
            }
            catch { Window?.Close(); Battle.Dispose(); throw; }
        }

        internal void Resize(float points)
        {
            CellSize = points; var face = Battle.State.Board.Face;
            Board.style.flexGrow = 0; Board.style.flexShrink = 0; Board.style.minHeight = 0;
            Board.style.width = face.Width * points; Board.style.height = face.Height * points;
        }

        internal IEnumerator Ready()
        {
            yield return null; yield return null; yield return null;
            Assert.IsNotNull(Board.panel, "Real EditorWindow panel required");
            Assert.AreEqual(ContextType.Editor, Board.panel.contextType);
            // A headless window has no repaint loop; picking asks the actual panel to validate its styles/layout.
            Board.panel.Pick(Vector2.zero);
            Assert.Greater(Board.contentRect.width, 0); Assert.Greater(Board.contentRect.height, 0);
            TestContext.Out.WriteLine("Editor panel layout: " + Board.contentRect);
            Controller.Refresh();
        }

        internal Vector2 Point(FlowPos cell)
        {
            var rect = Board.contentRect; var face = Battle.State.Board.Face;
            return Board.LocalToWorld(new Vector2(rect.x + (cell.x + 0.5f) * CellSize, rect.y + (face.Height - cell.y - 0.5f) * CellSize));
        }
        internal Vector2 Local(float x, float y) { return Board.LocalToWorld(new Vector2(x, y)); }
        internal IReadOnlyList<FlowPos> Route(int pair = 0) { return Battle.Runtime.Domain.Routes[pair]; }
        internal void Down(Vector2 p, int id = 0, string type = null)
        { using (var e = PointerDownEvent.GetPooled(new PanelPointer(p, id, type, 1))) { e.target = Board; Board.SendEvent(e); } }
        internal void Move(Vector2 p, int id = 0, string type = null)
        { using (var e = PointerMoveEvent.GetPooled(new PanelPointer(p, id, type, 1))) { e.target = Board; Board.SendEvent(e); } }
        internal void Up(Vector2 p, int id = 0, string type = null)
        { using (var e = PointerUpEvent.GetPooled(new PanelPointer(p, id, type, 0))) { e.target = Board; Board.SendEvent(e); } }
        internal void Cancel(int id = 0, string type = null)
        { using (var e = PointerCancelEvent.GetPooled(new PanelPointer(Point(Route()[0]), id, type, 0))) { e.target = Board; Board.SendEvent(e); } }
        internal void Draw(int pair = 0, int id = 0, string type = null)
        {
            var route = Route(pair); Down(Point(route[0]), id, type);
            for (var i = 1; i < route.Count - 1; i++) Move(Point(route[i]), id, type);
            Up(Point(route[route.Count - 1]), id, type);
        }
        internal void Tap(FlowPos cell) { var p = Point(cell); Down(p); Up(p); }
        internal static void CloseAll() { foreach (var rig in open.ToArray()) rig.Dispose(); }
        public void Dispose()
        {
            if (!open.Remove(this)) return;
            try
            {
                UI?.Close();
                if (Window != null && SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                    LogAssert.Expect(LogType.Error, "No graphic device is available to initialize the view.");
                Window?.Close();
            }
            finally { Battle.Dispose(); }
        }

        private sealed class PanelPointer : IPointerEvent
        {
            public int pointerId { get; }
            public string pointerType { get; }
            public bool isPrimary => pointerId <= 1;
            public int button => 0;
            public int pressedButtons { get; }
            public Vector3 position { get; }
            public Vector3 localPosition => position;
            public Vector3 deltaPosition => Vector3.zero;
            public float deltaTime => 0;
            public int clickCount => 1;
            public float pressure => 1;
            public float tangentialPressure => 0;
            public float altitudeAngle => 0;
            public float azimuthAngle => 0;
            public float twist => 0;
            public Vector2 radius => Vector2.zero;
            public Vector2 radiusVariance => Vector2.zero;
            public Vector2 tilt => Vector2.zero;
            public PenStatus penStatus => default;
            public EventModifiers modifiers => EventModifiers.None;
            public bool shiftKey => false;
            public bool ctrlKey => false;
            public bool commandKey => false;
            public bool altKey => false;
            public bool actionKey => false;
            internal PanelPointer(Vector2 p, int id, string type, int buttons)
            { position = p; pointerId = id; pointerType = type ?? PointerType.mouse; pressedButtons = buttons; }
        }
    }
}
