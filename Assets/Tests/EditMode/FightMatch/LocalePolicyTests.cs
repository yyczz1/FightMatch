using System.Collections.Generic;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FightMatch.Core.Tests
{
    public sealed class LocalePolicyTests
    {
        [Test]
        public void SimplifiedChineseInitializesTheSimplifiedSessionLocale()
        {
            Assert.AreEqual(LocaleId.ZhHans, new LocalizationService(null, SystemLanguage.ChineseSimplified).CurrentLocale);
        }

        [Test]
        public void LegacyAndTraditionalChineseNormalizeToTheSameSimplifiedLocale()
        {
            foreach (var language in new[] { SystemLanguage.Chinese, SystemLanguage.ChineseTraditional })
                Assert.AreEqual(LocaleId.ZhHans, new LocalizationService(null, language).CurrentLocale, language.ToString());
        }

        [Test]
        public void NonChineseSystemLanguagesNormalizeToEnglish()
        {
            foreach (var language in new[] { SystemLanguage.English, SystemLanguage.Japanese, SystemLanguage.Unknown, SystemLanguage.French })
                Assert.AreEqual(LocaleId.En, new LocalizationService(null, language).CurrentLocale, language.ToString());
        }

        [Test]
        public void InjectedSystemLanguageInitializesTheSessionWithoutAPreferenceStore()
        {
            var source = new UguiTestTextSource();
            var chinese = new LocalizationService(source, SystemLanguage.ChineseTraditional);
            var english = new LocalizationService(source, SystemLanguage.Japanese);
            Assert.AreEqual(LocaleId.ZhHans, chinese.CurrentLocale); Assert.AreEqual(LocaleId.En, english.CurrentLocale);
            Assert.AreNotEqual(chinese.Resolve("fm.common.action.back", null).Text, english.Resolve("fm.common.action.back", null).Text);
            Assert.AreEqual(2, source.ResolveCount);
        }

        [Test]
        public void RealChangesUpdateCurrentLocaleBeforeOneEventAndRepeatedChoicesAreQuiet()
        {
            var service = new LocalizationService(null, SystemLanguage.English);
            var seen = new List<LocaleId>();
            service.LocaleChanged += locale => { Assert.AreEqual(locale, service.CurrentLocale); seen.Add(locale); };
            Assert.IsFalse(service.SetLocale(LocaleId.En));
            Assert.IsTrue(service.SetLocale(LocaleId.ZhHans));
            Assert.IsFalse(service.SetLocale(LocaleId.ZhHans));
            Assert.IsTrue(service.SetLocale(LocaleId.En));
            CollectionAssert.AreEqual(new[] { LocaleId.ZhHans, LocaleId.En }, seen);
        }

        [Test]
        public void ActiveSubscribersReceiveOneChangeAndNewSubscribersDoNotReplayOldEvents()
        {
            var service = new LocalizationService(null, SystemLanguage.English);
            var first = 0; var second = 0; var newest = 0;
            System.Action<LocaleId> onFirst = locale => { first++; Assert.AreEqual(locale, service.CurrentLocale); };
            service.LocaleChanged += onFirst;
            service.LocaleChanged += locale => { second++; Assert.AreEqual(locale, service.CurrentLocale); };
            service.SetLocale(LocaleId.ZhHans);
            Assert.AreEqual(1, first); Assert.AreEqual(1, second);
            service.LocaleChanged -= onFirst;
            service.LocaleChanged += locale => { newest++; Assert.AreEqual(locale, service.CurrentLocale); };
            Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale); Assert.AreEqual(0, newest);
            service.SetLocale(LocaleId.En);
            Assert.AreEqual(1, first); Assert.AreEqual(2, second); Assert.AreEqual(1, newest);
        }
    }
}
