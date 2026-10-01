using System;
using System.Collections.Generic;
using UnityEngine;

namespace FightMatch.Presentation
{
    internal sealed class LocalizationService
    {
        private readonly ILocalizedTextSource source;
        private readonly Dictionary<LocalizedTmpText, string> failures = new Dictionary<LocalizedTmpText, string>();
        internal LocaleId CurrentLocale { get; private set; }
        internal bool IsReady => source != null;
        internal event Action<LocaleId> LocaleChanged;
        internal event Action DiagnosticsChanged;
        internal string BindingDiagnostic
        {
            get
            {
                string first = null;
                foreach (var code in failures.Values)
                    if (first == null || string.CompareOrdinal(code, first) < 0) first = code;
                return first;
            }
        }

        internal LocalizationService(ILocalizedTextSource source, SystemLanguage systemLanguage)
        { this.source = source; CurrentLocale = LocalePolicy.FromSystemLanguage(systemLanguage); }

        internal bool SetLocale(LocaleId locale)
        {
            if (!LocalePolicy.IsKnown(locale)) throw new ArgumentOutOfRangeException(nameof(locale));
            if (locale == CurrentLocale) return false;
            CurrentLocale = locale;
            LocaleChanged?.Invoke(locale);
            return true;
        }

        internal LocalizedTextResult Resolve(string key, IReadOnlyList<KeyValuePair<string, string>> namedArgs)
        {
            if (string.IsNullOrEmpty(key) || !key.StartsWith("fm.", StringComparison.Ordinal))
                return LocalizedTextResult.Failure("InvalidLocalizationKey");
            var names = new HashSet<string>(StringComparer.Ordinal);
            var args = namedArgs ?? Array.Empty<KeyValuePair<string, string>>();
            foreach (var arg in args)
                if (string.IsNullOrEmpty(arg.Key) || arg.Value == null || !names.Add(arg.Key))
                    return LocalizedTextResult.Failure("InvalidNamedArguments");
            if (source == null) return LocalizedTextResult.Failure("LocalizationNotReady");
            try { return source.Resolve(key, CurrentLocale, args) ?? LocalizedTextResult.Failure("InvalidLocalizationResult"); }
            catch (Exception) { return LocalizedTextResult.Failure("LocalizationSourceFailure"); }
        }

        internal void ReportBinding(LocalizedTmpText binding, string code)
        {
            var changed = false;
            if (code == null) changed = failures.Remove(binding);
            else if (!failures.TryGetValue(binding, out var before) || before != code)
            { failures[binding] = code; changed = true; }
            if (changed) DiagnosticsChanged?.Invoke();
        }
    }
}
