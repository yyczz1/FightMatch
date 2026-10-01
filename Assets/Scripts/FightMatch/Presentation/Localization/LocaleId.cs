using UnityEngine;

namespace FightMatch.Presentation
{
    internal enum LocaleId { En, ZhHans }

    internal static class LocalePolicy
    {
        internal static LocaleId FromSystemLanguage(SystemLanguage language)
        {
            return language == SystemLanguage.Chinese || language == SystemLanguage.ChineseSimplified ||
                language == SystemLanguage.ChineseTraditional ? LocaleId.ZhHans : LocaleId.En;
        }

        internal static bool IsKnown(LocaleId locale) => locale == LocaleId.En || locale == LocaleId.ZhHans;
    }
}
