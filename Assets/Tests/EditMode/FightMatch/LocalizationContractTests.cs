using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace FightMatch.Core.Tests
{
    internal sealed class UguiTestTextSource : ILocalizedTextSource
    {
        // COPY FIX-01 is test input only. The product does not load this authoring draft.
        internal const int ApprovedByteCount = 64708;
        internal const int ApprovedRowCount = 263;
        internal const string ApprovedSha256 = "d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c";
        private const string ParameterPattern = @"\{([A-Za-z][A-Za-z0-9_]*)\}";
        private static Entry[] approved;
        private readonly Dictionary<string, Entry> index = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly string catalogDiagnostic;
        internal IReadOnlyList<Entry> Entries { get; }
        internal int ResolveCount { get; private set; }

        internal sealed class Entry
        {
            internal readonly string Key, Chinese, English, Parameters, Severity;
            internal Entry(string key, string chinese, string english, string parameters = "", string severity = "info")
            { Key = key; Chinese = chinese; English = english; Parameters = parameters; Severity = severity; }
            internal string[] ParameterNames => string.IsNullOrEmpty(Parameters) ? Array.Empty<string>() : Parameters.Split(';');
        }

        internal UguiTestTextSource() : this(ReadApprovedDraft()) { }
        internal UguiTestTextSource(IEnumerable<Entry> entries)
        {
            Entries = entries.ToArray();
            foreach (var entry in Entries)
            {
                if (string.IsNullOrEmpty(entry.Key) || !entry.Key.StartsWith("fm.", StringComparison.Ordinal))
                { catalogDiagnostic = "InvalidLocalizationKey"; break; }
                if (index.ContainsKey(entry.Key)) { catalogDiagnostic = "DuplicateLocalizationKey"; break; }
                if (!TrySeverity(entry.Severity, out _)) { catalogDiagnostic = "InvalidSeverity"; break; }
                index.Add(entry.Key, entry);
            }
        }

        public LocalizedTextResult Resolve(string key, LocaleId locale, IReadOnlyList<KeyValuePair<string, string>> namedArgs)
        {
            ResolveCount++;
            if (!LocalePolicy.IsKnown(locale)) return LocalizedTextResult.Failure("UnknownLocale");
            if (catalogDiagnostic != null) return LocalizedTextResult.Failure(catalogDiagnostic);
            if (key == null || !index.TryGetValue(key, out var entry)) return LocalizedTextResult.Failure("MissingLocalizationKey");
            var template = locale == LocaleId.ZhHans ? entry.Chinese : entry.English;
            if (string.IsNullOrEmpty(template)) return LocalizedTextResult.Failure("MissingLocaleText");
            var declared = entry.ParameterNames;
            var names = Regex.Matches(template, ParameterPattern).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
            var remaining = Regex.Replace(template, ParameterPattern, "");
            if (remaining.IndexOfAny(new[] { '{', '}' }) >= 0 || declared.Any(string.IsNullOrEmpty) ||
                declared.Distinct(StringComparer.Ordinal).Count() != declared.Length ||
                !new HashSet<string>(declared, StringComparer.Ordinal).SetEquals(names))
                return LocalizedTextResult.Failure("InvalidTemplate");
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var arg in namedArgs ?? Array.Empty<KeyValuePair<string, string>>())
            {
                if (string.IsNullOrEmpty(arg.Key) || arg.Value == null || values.ContainsKey(arg.Key))
                    return LocalizedTextResult.Failure("InvalidNamedArguments");
                values.Add(arg.Key, arg.Value);
            }
            if (!new HashSet<string>(declared, StringComparer.Ordinal).SetEquals(values.Keys))
                return LocalizedTextResult.Failure("ParameterMismatch");
            TrySeverity(entry.Severity, out var severity);
            return LocalizedTextResult.Success(Regex.Replace(template, ParameterPattern, m => values[m.Groups[1].Value]), severity);
        }

        private static bool TrySeverity(string value, out LocalizedTextSeverity severity)
        {
            switch (value)
            {
                case "info": severity = LocalizedTextSeverity.Info; return true;
                case "warning": severity = LocalizedTextSeverity.Warning; return true;
                case "error": severity = LocalizedTextSeverity.Error; return true;
                case "blocking": severity = LocalizedTextSeverity.Blocking; return true;
                default: severity = default; return false;
            }
        }

        private static Entry[] ReadApprovedDraft()
        {
            if (approved != null) return approved;
            var path = Path.Combine(UnityEngine.Application.dataPath, "../docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv");
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length != ApprovedByteCount) throw new InvalidDataException("Approved COPY input is incomplete.");
            using (var sha = SHA256.Create())
                if (string.Concat(sha.ComputeHash(bytes).Select(x => x.ToString("x2"))) != ApprovedSha256)
                    throw new InvalidDataException("Approved COPY input changed; obtain the new fixed mapping before testing.");
            var rows = ReadCsv(Encoding.UTF8.GetString(bytes)).ToArray();
            if (rows.Length != ApprovedRowCount + 1 || rows.Any(r => r.Length != 9))
                throw new InvalidDataException("Approved COPY shape changed.");
            return approved = rows.Skip(1).Select(r => new Entry(r[0], r[4], r[5], r[6], r[7])).ToArray();
        }

        private static IEnumerable<string[]> ReadCsv(string text)
        {
            var field = new StringBuilder(); var fields = new List<string>(); var quoted = false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (!quoted && c == ',') { fields.Add(field.ToString()); field.Clear(); }
                else if (!quoted && (c == '\r' || c == '\n'))
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    fields.Add(field.ToString()); field.Clear(); yield return fields.ToArray(); fields.Clear();
                }
                else field.Append(c);
            }
            if (quoted) throw new InvalidDataException("Unclosed CSV field.");
            if (field.Length != 0 || fields.Count != 0) { fields.Add(field.ToString()); yield return fields.ToArray(); }
        }
    }

    public sealed class LocalizationContractTests
    {
        [Test]
        public void EnglishAndSimplifiedChineseHaveTheSameCompleteKeySet()
        {
            var source = new UguiTestTextSource();
            Assert.AreEqual(UguiTestTextSource.ApprovedRowCount, source.Entries.Count);
            var english = source.Entries.Where(e => !string.IsNullOrEmpty(e.English)).Select(e => e.Key).ToArray();
            var chinese = source.Entries.Where(e => !string.IsNullOrEmpty(e.Chinese)).Select(e => e.Key).ToArray();
            CollectionAssert.AreEquivalent(english, chinese);
            Assert.AreEqual(source.Entries.Count, english.Length);
            foreach (var entry in source.Entries)
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                    Assert.IsTrue(source.Resolve(entry.Key, locale, Args(entry)).IsSuccess, entry.Key);
        }

        [Test]
        public void EveryNamedParameterMatchesBothTemplatesAndRejectsMalformedInput()
        {
            var source = new UguiTestTextSource();
            foreach (var entry in source.Entries)
            {
                var arguments = Args(entry);
                foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
                {
                    Assert.IsTrue(source.Resolve(entry.Key, locale, arguments).IsSuccess, entry.Key);
                    Assert.IsFalse(source.Resolve(entry.Key, locale, arguments.Concat(new[] { Pair("extra", "1") }).ToArray()).IsSuccess, entry.Key);
                    if (arguments.Length != 0)
                        Assert.IsFalse(source.Resolve(entry.Key, locale, arguments.Skip(1).ToArray()).IsSuccess, entry.Key);
                }
            }
            var invalid = new UguiTestTextSource(new[] { new UguiTestTextSource.Entry("fm.test.invalid", "{n", "{n", "n") });
            var result = invalid.Resolve("fm.test.invalid", LocaleId.En, new[] { Pair("n", "1") });
            Assert.AreEqual("InvalidTemplate", result.DiagnosticCode);
            Assert.IsNull(result.Severity);
            var declared = new UguiTestTextSource(new[] { new UguiTestTextSource.Entry("fm.test.exact", "{n}", "{n}", "n") });
            Assert.IsFalse(declared.Resolve("fm.test.exact", LocaleId.En, new[] { Pair("n", "1"), Pair("n", "2") }).IsSuccess);
        }

        [Test]
        public void KeysArePrefixedUniqueAndAllFourBusinessSeveritiesRemainSuccessful()
        {
            var source = new UguiTestTextSource();
            Assert.AreEqual(source.Entries.Count, source.Entries.Select(e => e.Key).Distinct(StringComparer.Ordinal).Count());
            Assert.IsTrue(source.Entries.All(e => e.Key.StartsWith("fm.", StringComparison.Ordinal)));
            foreach (var severity in new[] { "info", "warning", "error", "blocking" })
            {
                var sample = new UguiTestTextSource(new[] { new UguiTestTextSource.Entry("fm.test.tone", "内容", "Content", severity: severity) });
                var result = sample.Resolve("fm.test.tone", LocaleId.En, null);
                Assert.IsTrue(result.IsSuccess, severity); Assert.IsNotNull(result.Severity); Assert.IsNull(result.DiagnosticCode);
            }
            var row = source.Entries[0];
            Assert.AreEqual("DuplicateLocalizationKey", new UguiTestTextSource(new[] { row, row }).Resolve(row.Key, LocaleId.En, null).DiagnosticCode);
            Assert.AreEqual("InvalidLocalizationKey", new UguiTestTextSource(new[] { new UguiTestTextSource.Entry("other", "字", "Text") }).Resolve("other", LocaleId.En, null).DiagnosticCode);
            Assert.AreEqual("InvalidSeverity", new UguiTestTextSource(new[] { new UguiTestTextSource.Entry("fm.test.bad", "字", "Text", severity: "fatal") }).Resolve("fm.test.bad", LocaleId.En, null).DiagnosticCode);
        }

        [Test]
        public void UnreachableConditionalCopyGroupsAreRetained()
        {
            var keys = new UguiTestTextSource().Entries.Select(e => e.Key).ToArray();
            foreach (var prefix in new[] { "fm.initial_download.", "fm.mobile_data.", "fm.update_ready.", "fm.guest_local_only.", "fm.first_clear_reward." })
                Assert.IsTrue(keys.Any(k => k.StartsWith(prefix, StringComparison.Ordinal)), prefix);
            CollectionAssert.Contains(keys, "fm.language.save_failed");
        }

        [Test]
        public void ChineseVariantsNormalizeToSimplifiedAndOtherLanguagesToEnglish()
        {
            foreach (var language in new[] { SystemLanguage.Chinese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional })
                Assert.AreEqual(LocaleId.ZhHans, LocalePolicy.FromSystemLanguage(language));
            foreach (var language in new[] { SystemLanguage.English, SystemLanguage.Japanese, SystemLanguage.Unknown, SystemLanguage.French })
                Assert.AreEqual(LocaleId.En, LocalePolicy.FromSystemLanguage(language));
        }

        [Test]
        public void InternalContractsUseOnlyTheApprovedPresentationAndHostFriends()
        {
            ILocalizedTextSource source = new UguiTestTextSource();
            LocalizedTextResult text = new LocalizationService(source, SystemLanguage.English).Resolve("fm.common.action.back", null);
            Assert.IsTrue(text.IsSuccess);
            var root = Path.Combine(UnityEngine.Application.dataPath, "Scripts/FightMatch/Presentation");
            var friends = File.ReadAllLines(Path.Combine(root, "PresentationAssemblyInfo.cs")).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            CollectionAssert.AreEquivalent(new[] {
                "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"FightMatch.Host\")]",
                "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"FightMatch.Core.Tests\")]",
                "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"FightMatch.Host.Tests\")]" }, friends);
            var hostRoot = Path.Combine(UnityEngine.Application.dataPath, "Scripts/FightMatch/Host");
            CollectionAssert.AreEquivalent(new[] {
                "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"FightMatch.Core.Tests\")]",
                "[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(\"FightMatch.Host.Tests\")]" },
                File.ReadAllLines(Path.Combine(hostRoot, "HostAssemblyInfo.cs")).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray());
            var hostCode = File.ReadAllText(Path.Combine(hostRoot, "FightMatchHostView.cs"));
            Assert.That(hostCode, Does.Contain("internal void Bind("));
            foreach (var forbidden in new[] { "System.Reflection", "BindingFlags", "SendMessage", "Type.GetType" })
                Assert.That(hostCode, Does.Not.Contain(forbidden));
            using (var host = new UguiHostRig())
                Assert.IsFalse(host.View.DiagnosticVisible, host.View.DiagnosticCode);
            foreach (var file in new[] { "LocaleId.cs", "LocalizedTextSource.cs", "LocalizationService.cs" })
            {
                var code = File.ReadAllText(Path.Combine(root, "Localization", file));
                Assert.IsFalse(Regex.IsMatch(code, @"public\s+(?:sealed\s+)?(?:class|interface|enum)\s+(?:LocaleId|ILocalizedTextSource|LocalizedTextResult|LocalizationService)\b"), file);
                Assert.That(code, Does.Not.Contain("System.Reflection"));
                Assert.That(code, Does.Not.Contain("BindingFlags"));
            }
            var hostFixture = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs"));
            Assert.That(hostFixture, Does.Contain("ILocalizedTextSource"), "Host tests directly compile against the same internal contract.");
        }

        private static KeyValuePair<string, string> Pair(string key, string value) => new KeyValuePair<string, string>(key, value);
        private static KeyValuePair<string, string>[] Args(UguiTestTextSource.Entry entry) => entry.ParameterNames.Select(n => Pair(n, "1")).ToArray();
    }
}
