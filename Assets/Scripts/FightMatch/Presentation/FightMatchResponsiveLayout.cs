using System;
using System.Collections.Generic;
using UnityEngine;

namespace FightMatch.Presentation
{
    // Geometry uses the fitted safe rect in Canvas units. Reference-sized child layouts
    // inherit k exactly once; their rendered extents, padding and glyphs scale together.
    public sealed class FightMatchResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private SafeAreaFitter safeArea;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform screenLayer, topBar, battleContent, stage, battleStatus, boardRegion, bottomHud;
        [SerializeField] private RectTransform normalHudRoot, historyDrawerRoot, referenceDrawerRoot, normalStatusViewport, normalMainRow;
        [SerializeField] private RectTransform startupViewport, navigationViewport, resultViewport;
        [SerializeField] private RectTransform[] dialogPanels;
        [SerializeField] private GameObject layoutDiagnostic;
        [SerializeField] private LocalizedTmpText layoutDiagnosticText;
        [SerializeField] private CanvasGroup battleInteraction;
        private LocalizationService localization;
        private bool subscribed, dirty = true, sampled, valid, validityKnown, applying;
        private long previousVersion = -1;
        private Vector2 previousSize;
        private Vector3 previousScale;
        private float previousFactor;
        internal event Action<bool> ValidityChanged;

        internal void Bind(LocalizationService service)
        {
            Unbind();
            localization = service ?? throw new ArgumentNullException(nameof(service));
            if (!BindingsPresent()) throw new InvalidOperationException("Responsive layout bindings are incomplete.");
            layoutDiagnosticText.Bind(service, "fm.diagnostic.missing_binding",
                new KeyValuePair<string, string>("errorCode", "LayoutInvalid"));
            dirty = true; sampled = false;
            if (isActiveAndEnabled) Subscribe();
            Apply();
        }

        private bool BindingsPresent()
        {
            if (safeArea == null || canvas == null || screenLayer == null || topBar == null || battleContent == null ||
                stage == null || battleStatus == null || boardRegion == null || bottomHud == null || normalHudRoot == null ||
                historyDrawerRoot == null || referenceDrawerRoot == null || normalStatusViewport == null || normalMainRow == null ||
                startupViewport == null || navigationViewport == null || resultViewport == null || layoutDiagnostic == null ||
                layoutDiagnosticText == null || battleInteraction == null || dialogPanels == null || dialogPanels.Length != 7) return false;
            foreach (var panel in dialogPanels) if (panel == null) return false;
            return true;
        }

        private void Subscribe()
        {
            if (subscribed || safeArea == null) return;
            safeArea.Changed += MarkDirty;
            Canvas.willRenderCanvases += BeforeRender;
            subscribed = true;
        }
        private void Unsubscribe()
        {
            if (!subscribed) return;
            if (!ReferenceEquals(safeArea, null)) safeArea.Changed -= MarkDirty;
            Canvas.willRenderCanvases -= BeforeRender;
            subscribed = false;
        }
        private void MarkDirty() { dirty = true; }
        private void OnRectTransformDimensionsChange() { if (!applying) dirty = true; }
        private void OnEnable()
        {
            dirty = true; sampled = false;
            Subscribe(); Apply();
        }
        private void BeforeRender()
        {
            if (canvas == null || safeArea == null) return;
            var rect = (RectTransform)safeArea.transform;
            if (!sampled || previousVersion != safeArea.Version || previousSize != rect.rect.size ||
                previousFactor != canvas.scaleFactor || previousScale != canvas.transform.lossyScale) dirty = true;
            if (dirty) Apply();
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void Apply()
        {
            if (applying || localization == null || !BindingsPresent()) return;
            var size = ((RectTransform)safeArea.transform).rect.size;
            var factor = canvas.scaleFactor;
            var scale = canvas.transform.lossyScale;
            if (sampled && previousVersion == safeArea.Version && previousSize == size && previousFactor == factor && previousScale == scale)
            { dirty = false; return; }
            previousVersion = safeArea.Version; previousSize = size; previousFactor = factor; previousScale = scale;
            sampled = true; dirty = false;
            var width = size.x; var height = size.y;
            var board = Mathf.Min(width, 508); var k = board / 508;
            var usable = Finite(width) && Finite(height) && Finite(factor) && factor > 0 &&
                Finite(scale.x) && Finite(scale.y) && scale.x > 0 && scale.y > 0 &&
                width > 64 && height >= 812 * k && height - 64 - 384 * k >= 48 * k;
            if (!usable) { Gate(false); return; }
            applying = true;
            try
            {
                var breathing = Mathf.Max(0, (height - 920 * k) / 2);
                var deficit = Mathf.Clamp(920 * k - height, 0, 108 * k);
                var stageHeight = 166 * k - Mathf.Min(deficit, 70 * k);
                var hudHeight = 142 * k - Mathf.Min(Mathf.Max(deficit - 70 * k, 0), 38 * k);
                var left = (width - board) / 2;
                Fill(screenLayer);
                Box(topBar, left, breathing, board, 56 * k, k);
                var pageTop = breathing + 56 * k;
                var pageHeight = height - pageTop - breathing;
                foreach (var viewport in new[] { startupViewport, navigationViewport, resultViewport })
                    Box(viewport, left, pageTop, board, pageHeight, k);
                Box(battleContent, left, pageTop, board, pageHeight);
                Box(stage, 0, 0, board, stageHeight, k);
                Box(battleStatus, 0, stageHeight, board, 48 * k, k);
                Box(boardRegion, 0, stageHeight + 48 * k, board, board);
                Box(bottomHud, 0, stageHeight + 48 * k + board, board, hudHeight);
                Fill(normalHudRoot);
                Box(normalStatusViewport, 0, 0, board, hudHeight - 80 * k, k);
                Box(normalMainRow, 0, hudHeight - 80 * k, board, 72 * k, k);
                Box(historyDrawerRoot, 0, 0, board, hudHeight, k);
                Box(referenceDrawerRoot, 0, 0, board, hudHeight, k);
                var panelWidth = Mathf.Min(476, width - 64);
                foreach (var panel in dialogPanels)
                    Box(panel, (width - panelWidth) / 2, 32, panelWidth, height - 64, k);
            }
            finally { applying = false; }
            Gate(true);
        }
        private static void Fill(RectTransform rect)
        {
            rect.localScale = Vector3.one; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Box(RectTransform rect, float x, float top, float width, float height, float childScale = 1)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.localScale = new Vector3(childScale, childScale, 1);
            rect.sizeDelta = new Vector2(width / childScale, height / childScale);
        }
        private void Gate(bool available)
        {
            battleInteraction.interactable = available;
            battleInteraction.blocksRaycasts = available;
            layoutDiagnostic.SetActive(!available);
            if (validityKnown && valid == available) return;
            validityKnown = true; valid = available; ValidityChanged?.Invoke(available);
        }
        internal void Unbind()
        {
            Unsubscribe(); dirty = true; sampled = false;
            if (layoutDiagnosticText != null) layoutDiagnosticText.Unbind();
            ReleaseGate(); localization = null;
        }
        private void ReleaseGate()
        {
            if (layoutDiagnostic != null) layoutDiagnostic.SetActive(false);
            if (battleInteraction != null) { battleInteraction.interactable = true; battleInteraction.blocksRaycasts = true; }
            validityKnown = false;
        }
        private void OnDisable() { Unsubscribe(); dirty = true; sampled = false; ReleaseGate(); }
        private void OnDestroy() { Unbind(); }
    }
}
