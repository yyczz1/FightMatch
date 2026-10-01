using System;
using System.Collections.Generic;

namespace FightMatch.Presentation
{
    internal enum LocalizedTextSeverity { Info, Warning, Error, Blocking }

    internal interface ILocalizedTextSource
    {
        LocalizedTextResult Resolve(string key, LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> namedArgs);
    }

    internal sealed class LocalizedTextResult
    {
        internal bool IsSuccess => DiagnosticCode == null;
        internal string Text { get; }
        internal LocalizedTextSeverity? Severity { get; }
        internal string DiagnosticCode { get; }

        private LocalizedTextResult(string text, LocalizedTextSeverity? severity, string diagnosticCode)
        { Text = text; Severity = severity; DiagnosticCode = diagnosticCode; }

        internal static LocalizedTextResult Success(string text, LocalizedTextSeverity severity = LocalizedTextSeverity.Info)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (severity < LocalizedTextSeverity.Info || severity > LocalizedTextSeverity.Blocking)
                throw new ArgumentOutOfRangeException(nameof(severity));
            return new LocalizedTextResult(text, severity, null);
        }

        internal static LocalizedTextResult Failure(string code)
        {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException("A binding diagnostic code is required.", nameof(code));
            return new LocalizedTextResult(null, null, code);
        }
    }
}
