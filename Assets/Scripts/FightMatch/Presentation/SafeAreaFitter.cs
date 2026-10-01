using System;
using UnityEngine;

namespace FightMatch.Presentation
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect previousSafeArea;
        private Vector2 previousScreen;
        private bool applied;
        internal event Action Changed;
        internal long Version { get; private set; }

        private void LateUpdate() { Apply(Screen.safeArea, new Vector2(Screen.width, Screen.height)); }

        internal void Apply(Rect safeArea, Vector2 screen)
        {
            if (!Finite(screen.x) || !Finite(screen.y) || screen.x <= 0 || screen.y <= 0 ||
                !Finite(safeArea.x) || !Finite(safeArea.y) || !Finite(safeArea.width) || !Finite(safeArea.height) ||
                safeArea.width <= 0 || safeArea.height <= 0 || safeArea.xMin < 0 || safeArea.yMin < 0 ||
                safeArea.xMax > screen.x || safeArea.yMax > screen.y) return;
            if (applied && previousSafeArea == safeArea && previousScreen == screen) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safeArea.xMin / screen.x, safeArea.yMin / screen.y);
            rect.anchorMax = new Vector2(safeArea.xMax / screen.x, safeArea.yMax / screen.y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            previousSafeArea = safeArea; previousScreen = screen; applied = true; Version++;
            Changed?.Invoke();
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
