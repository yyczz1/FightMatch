using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FightMatch.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class LocalizedTmpText : MonoBehaviour
    {
        internal const string Placeholder = "【if you see this, it is a bug.】";
        [SerializeField] private TextMeshProUGUI target;
        [SerializeField] private string key;
        private LocalizationService service;
        private KeyValuePair<string, string>[] arguments = Array.Empty<KeyValuePair<string, string>>();
        internal string Key => key;
        internal TextMeshProUGUI Target => target != null ? target : GetComponent<TextMeshProUGUI>();
        internal string DiagnosticCode { get; private set; }
        internal LocalizedTextSeverity? Severity { get; private set; }

        internal void Bind(LocalizationService localization, string localizationKey = null,
            params KeyValuePair<string, string>[] namedArgs)
        {
            Unbind();
            if (localizationKey != null) key = localizationKey;
            arguments = namedArgs == null ? Array.Empty<KeyValuePair<string, string>>() :
                (KeyValuePair<string, string>[])namedArgs.Clone();
            service = localization ?? throw new ArgumentNullException(nameof(localization));
            service.LocaleChanged += OnLocaleChanged;
            RefreshText();
        }

        internal void Unbind()
        {
            var previous = service; service = null;
            if (previous != null) { previous.LocaleChanged -= OnLocaleChanged; previous.ReportBinding(this, null); }
            DiagnosticCode = null; Severity = null;
            if (Target != null) Target.text = Placeholder;
        }

        private void OnLocaleChanged(LocaleId locale) { RefreshText(); }
        private void RefreshText()
        {
            if (service == null) return;
            var result = service.Resolve(key, arguments);
            var text = Target;
            var failure = result.DiagnosticCode;
            if (text == null) failure = "MissingTmpBinding";
            else if (text.font == null || text.font.material == null) failure = "MissingFontOrMaterial";
            else if (failure == null && !text.font.HasCharacters(result.Text, out uint[] missing, true, true))
                failure = "MissingGlyph";
            DiagnosticCode = failure;
            Severity = failure == null ? result.Severity : null;
            if (text != null)
            {
                text.text = failure == null ? result.Text : Placeholder;
                text.color = failure != null ? new Color(1, .3f, .3f) :
                    Severity == LocalizedTextSeverity.Warning ? new Color(1, .8f, .4f) :
                    Severity == LocalizedTextSeverity.Error || Severity == LocalizedTextSeverity.Blocking ? new Color(1, .5f, .5f) : Color.white;
            }
            service.ReportBinding(this, failure);
        }

        private void OnDestroy() { Unbind(); }
    }
}
