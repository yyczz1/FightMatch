using System;
using FightMatch.Presentation;
using UnityEngine;

namespace FightMatch.Host
{
    internal interface ILocalePreferenceStore
    {
        LocalePreferenceLoadResult Load(SystemLanguage systemLanguage);
        LocalePreferenceSaveResult Save(LocaleId desiredLocale);
    }

    internal enum LocalePreferenceLoadDisposition
    {
        Remembered, MissingSystemDefault, InvalidSystemDefault, ReadFailedSystemDefault
    }

    internal sealed class LocalePreferenceLoadResult
    {
        internal LocaleId Locale { get; }
        internal LocalePreferenceLoadDisposition Disposition { get; }
        internal string DiagnosticCode { get; }

        internal LocalePreferenceLoadResult(LocaleId locale, LocalePreferenceLoadDisposition disposition, string code)
        {
            if (!LocalePolicy.IsKnown(locale)) throw new ArgumentOutOfRangeException(nameof(locale));
            bool valid = (disposition == LocalePreferenceLoadDisposition.Remembered ||
                disposition == LocalePreferenceLoadDisposition.MissingSystemDefault) && code == null;
            valid |= disposition == LocalePreferenceLoadDisposition.ReadFailedSystemDefault && code == "PreferenceReadFailed";
            valid |= disposition == LocalePreferenceLoadDisposition.InvalidSystemDefault &&
                (code == "PreferenceInvalidDocument" || code == "PreferenceInvalidSchema" ||
                 code == "PreferenceUnsupportedVersion" || code == "PreferenceUnsupportedLocale");
            if (!valid) throw new ArgumentException("Invalid preference load result.");
            Locale = locale; Disposition = disposition; DiagnosticCode = code;
        }
    }

    internal enum LocalePreferenceSaveDisposition { Saved, SaveFailed, Unknown }

    internal sealed class LocalePreferenceSaveResult
    {
        internal LocalePreferenceSaveDisposition Disposition { get; }
        internal string LocalizationKey { get; }
        internal string ErrorCode { get; }

        private LocalePreferenceSaveResult(LocalePreferenceSaveDisposition disposition, string key, string code)
        { Disposition = disposition; LocalizationKey = key; ErrorCode = code; }

        internal static LocalePreferenceSaveResult Saved() =>
            new LocalePreferenceSaveResult(LocalePreferenceSaveDisposition.Saved, "fm.language.saved", null);

        internal static LocalePreferenceSaveResult Failed(string code, bool unknown = false)
        {
            bool post = code == "PreferenceTargetReopenFailed" || code == "PreferenceTargetReadFailed" ||
                code == "PreferenceTargetVerificationFailed";
            bool rollback = code == "PreferenceRollbackFailed" || code == "PreferenceRollbackProofFailed" ||
                code == "PreferenceRollbackMismatch";
            bool pre = code == "PreferenceTempCreateFailed" || code == "PreferenceTempWriteFailed" ||
                code == "PreferenceFlushFailed" || code == "PreferenceTempReadFailed" ||
                code == "PreferenceTempVerificationFailed" || code == "PreferenceCommitFailed";
            if (!(unknown ? post || rollback : pre || post))
                throw new ArgumentException("Invalid preference save diagnostic.", nameof(code));
            return new LocalePreferenceSaveResult(unknown ? LocalePreferenceSaveDisposition.Unknown :
                LocalePreferenceSaveDisposition.SaveFailed, unknown ? "fm.language.save_unknown" :
                "fm.language.save_failed", code);
        }
    }
}
