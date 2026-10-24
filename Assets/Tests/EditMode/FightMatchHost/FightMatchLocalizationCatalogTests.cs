using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Presentation;
using NUnit.Framework;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchLocalizationCatalogTests
    {
        private const string CanonicalSha = "f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc";
        private const string Placeholder = "【if you see this, it is a bug.】";
        private const string TestKey = "fm.test";

        [Test]
        public void RealCanonicalCatalogResolvesEnglishChineseAndNamedArguments()
        {
            var path = Path.Combine(UnityEngine.Application.dataPath, "../Generated/FightMatch/Localization/fm-text-v1.json");
            var bytes = File.ReadAllBytes(path);
            Assert.AreEqual(121098, bytes.Length);
            Assert.AreEqual(CanonicalSha, Hash(bytes));
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes, CanonicalSha, 264, out var source, out var code), code);
            Assert.IsNull(code);
            Assert.AreEqual("Adventure", source.Resolve("fm.common.nav.adventure", LocaleId.En, null).Text);
            Assert.AreEqual("冒险", source.Resolve("fm.common.nav.adventure", LocaleId.ZhHans, null).Text);
            var args = new[] { Pair("errorCode", "E42") };
            Assert.AreEqual("Language changed for this session, but we could not confirm whether the language preference was saved. Save this language again. Diagnostic code: E42",
                source.Resolve("fm.language.save_unknown", LocaleId.En, args).Text);
            Assert.AreEqual("语言已在本次会话中切换，但无法确认语言偏好是否已保存。请重新保存此语言。诊断码：E42",
                source.Resolve("fm.language.save_unknown", LocaleId.ZhHans, args).Text);
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            {
                var placeholder = source.Resolve("fm.diagnostic.placeholder", locale, null);
                Assert.AreEqual(Placeholder, placeholder.Text);
                Assert.AreEqual(LocalizedTextSeverity.Blocking, placeholder.Severity);
                Assert.AreEqual(LocalizedTextSeverity.Error, source.Resolve("fm.language.save_failed", locale, args).Severity);
            }
        }

        [TestCase("info", 0), TestCase("warning", 1), TestCase("error", 2), TestCase("blocking", 3)]
        public void BusinessSeverityAndLiteralParameterValuesArePreserved(string severity, int expected)
        {
            var rows = Rows(); rows.Last().Severity = severity;
            var source = Parse(Document(rows), rows.Count);
            var args = new[] { Pair("name", "{uninterpreted}") };
            var english = source.Resolve(TestKey, LocaleId.En, args);
            var chinese = source.Resolve(TestKey, LocaleId.ZhHans, args);
            Assert.AreEqual("Value {uninterpreted}", english.Text);
            Assert.AreEqual("数值{uninterpreted}", chinese.Text);
            Assert.AreEqual((LocalizedTextSeverity)expected, english.Severity);
            Assert.IsTrue(english.IsSuccess); Assert.IsTrue(chinese.IsSuccess);
        }

        [TestCase("missing", "ParameterMismatch"), TestCase("extra", "ParameterMismatch")]
        [TestCase("case", "ParameterMismatch"), TestCase("duplicate", "InvalidNamedArguments")]
        [TestCase("empty-name", "InvalidNamedArguments"), TestCase("null-name", "InvalidNamedArguments")]
        [TestCase("null-value", "InvalidNamedArguments"), TestCase("key", "MissingLocalizationKey")]
        [TestCase("locale", "UnknownLocale")]
        public void AdapterFailuresHaveNoTextOrBusinessSeverity(string kind, string expected)
        {
            var source = Parse(Document(Rows()), 4);
            IReadOnlyList<KeyValuePair<string, string>> args = new[] { Pair("name", "one") };
            switch (kind)
            {
                case "missing": args = null; break;
                case "extra": args = new[] { Pair("name", "one"), Pair("extra", "two") }; break;
                case "case": args = new[] { Pair("Name", "one") }; break;
                case "duplicate": args = new[] { Pair("name", "one"), Pair("name", "two") }; break;
                case "empty-name": args = new[] { Pair("", "one") }; break;
                case "null-name": args = new[] { Pair(null, "one") }; break;
                case "null-value": args = new[] { Pair("name", null) }; break;
            }
            var result = source.Resolve(kind == "key" ? "fm.absent" : TestKey,
                kind == "locale" ? (LocaleId)99 : LocaleId.En, args);
            Assert.IsFalse(result.IsSuccess); Assert.IsNull(result.Text); Assert.IsNull(result.Severity);
            Assert.AreEqual(expected, result.DiagnosticCode);
            Assert.IsTrue(source.Resolve("fm.diagnostic.placeholder", LocaleId.En, null).IsSuccess);
        }

        [Test]
        public void PublishedSourceOwnsItsDataAndSurvivesAnotherFailedParse()
        {
            var bytes = Bytes(Document(Rows()));
            var source = Parse(bytes, 4);
            Array.Clear(bytes, 0, bytes.Length);
            Rejected(bytes, 4, "LocalizationCatalogInvalidSchema");
            var args = new List<KeyValuePair<string, string>> { Pair("name", "first") };
            var first = source.Resolve(TestKey, LocaleId.En, args);
            args[0] = Pair("name", "second");
            Assert.AreEqual("Value first", first.Text);
            Assert.AreEqual("Value second", source.Resolve(TestKey, LocaleId.En, args).Text);
        }

        [TestCase("hash"), TestCase("null-hash"), TestCase("hash-case"), TestCase("count"), TestCase("zero-count")]
        public void ExpectedIdentityMustMatch(string kind)
        {
            var bytes = Bytes(Document(Rows())); var hash = Hash(bytes); var count = 4;
            if (kind == "hash") hash = new string('0', 64);
            if (kind == "null-hash") hash = null;
            if (kind == "hash-case") hash = hash.ToUpperInvariant();
            if (kind == "count") count = 5;
            if (kind == "zero-count") count = 0;
            Assert.IsFalse(FightMatchLocalizationCatalogParser.TryParse(bytes, hash, count, out var source, out var code));
            Assert.IsNull(source);
            Assert.AreEqual(kind == "count" ? "LocalizationCatalogCountMismatch" : kind == "zero-count" ?
                "LocalizationCatalogInvalidCount" : "LocalizationCatalogHashMismatch", code);
        }

        [TestCase("2"), TestCase("1.0"), TestCase("1e0"), TestCase("01"), TestCase("\"1\""), TestCase("null")]
        public void SchemaVersionMustBeExactlyIntegerOne(string token)
            => Rejected(Bytes(Document(Rows()).Replace("\"schemaVersion\":1", "\"schemaVersion\":" + token)), 4, "LocalizationCatalogInvalidSchema");

        [TestCase("unknown-root"), TestCase("duplicate-root"), TestCase("case-root")]
        [TestCase("unknown-record"), TestCase("duplicate-record"), TestCase("missing-record")]
        [TestCase("duplicate-text"), TestCase("case-locale"), TestCase("locale-order"), TestCase("extra-locale")]
        [TestCase("null-text"), TestCase("object-parameters"), TestCase("deep-metadata")]
        public void ExactSchemaRejectsUnknownDuplicateMissingAndWrongTypeFields(string kind)
        {
            var json = Document(Rows());
            switch (kind)
            {
                case "unknown-root": json = json.Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"extra\":0"); break;
                case "duplicate-root": json = json.Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1"); break;
                case "case-root": json = json.Replace("schemaVersion", "SchemaVersion"); break;
                case "unknown-record": json = json.Replace("\"key\":", "\"extra\":0,\"key\":"); break;
                case "duplicate-record": json = json.Replace("\"screen\":\"s\"", "\"screen\":\"s\",\"screen\":\"s\""); break;
                case "missing-record": json = json.Replace(",\"qaNotes\":\"q\"", ""); break;
                case "duplicate-text": json = json.Replace("\"texts\":{\"en\":", "\"texts\":{\"en\":\"x\",\"en\":"); break;
                case "case-locale": json = json.Replace("\"zh-Hans\":", "\"zh-hans\":"); break;
                case "locale-order": json = json.Replace("[\"en\",\"zh-Hans\"]", "[\"zh-Hans\",\"en\"]"); break;
                case "extra-locale": json = json.Replace("[\"en\",\"zh-Hans\"]", "[\"en\",\"zh-Hans\",\"fr\"]"); break;
                case "null-text": json = json.Replace(Quote("Value {name}"), "null"); break;
                case "object-parameters": json = json.Replace("\"parameters\":[\"name\"]", "\"parameters\":{}"); break;
                case "deep-metadata": json = json.Replace("\"screen\":\"s\"", "\"screen\":" + new string('[', 9) + "0" + new string(']', 9)); break;
            }
            Rejected(Bytes(json), 4, "LocalizationCatalogInvalidSchema");
        }

        [TestCase("duplicate-key", "LocalizationCatalogInvalidKey"), TestCase("key-prefix", "LocalizationCatalogInvalidKey")]
        [TestCase("key-order", "LocalizationCatalogInvalidKey"), TestCase("severity", "LocalizationCatalogInvalidSeverity")]
        [TestCase("empty-parameter", "LocalizationCatalogInvalidParameters"), TestCase("duplicate-parameter", "LocalizationCatalogInvalidParameters")]
        [TestCase("parameter-order", "LocalizationCatalogInvalidParameters"), TestCase("locale-mismatch", "LocalizationCatalogInvalidParameters")]
        [TestCase("duplicate-placeholder", "LocalizationCatalogInvalidParameters"), TestCase("empty-placeholder", "LocalizationCatalogInvalidParameters")]
        [TestCase("position-placeholder", "LocalizationCatalogInvalidParameters"), TestCase("unclosed-brace", "LocalizationCatalogInvalidParameters")]
        [TestCase("extra-brace", "LocalizationCatalogInvalidParameters"), TestCase("unsafe-text", "LocalizationCatalogUnsafeText")]
        [TestCase("placeholder", "LocalizationCatalogRequiredEntryInvalid"), TestCase("save-contract", "LocalizationCatalogRequiredEntryInvalid")]
        [TestCase("missing-required", "LocalizationCatalogRequiredEntryMissing")]
        public void CatalogSemanticsFailClosed(string kind, string expected)
        {
            var rows = Rows(); var row = rows.Last();
            switch (kind)
            {
                case "duplicate-key": rows.Add(new Row(TestKey)); break;
                case "key-prefix": row.Key = "wrong"; break;
                case "key-order": rows.Reverse(); break;
                case "severity": row.Severity = "Info"; break;
                case "empty-parameter": row.Parameters = new[] { "" }; break;
                case "duplicate-parameter": row.Parameters = new[] { "name", "name" }; break;
                case "parameter-order": row.Parameters = new[] { "z", "a" }; break;
                case "locale-mismatch": row.Chinese = "{other}"; break;
                case "duplicate-placeholder": row.English = "{name}{name}"; break;
                case "empty-placeholder": row.English = "{}"; break;
                case "position-placeholder": row.English = "{0}"; break;
                case "unclosed-brace": row.English = "{name"; break;
                case "extra-brace": row.English = "{name}}"; break;
                case "unsafe-text": row.English = "CommitId {name}"; break;
                case "placeholder": rows[0].English = "changed"; break;
                case "save-contract": rows[1].Severity = "info"; break;
                case "missing-required": rows.RemoveAt(0); break;
            }
            Rejected(Bytes(Document(rows, kind != "key-order")), rows.Count, expected);
        }

        [TestCase("truncated"), TestCase("no-lf"), TestCase("extra-lf"), TestCase("trailing-space"), TestCase("second-root")]
        public void ExactTerminationAndCompleteDocumentAreRequired(string kind)
        {
            var json = Document(Rows());
            if (kind == "truncated") json = json.Substring(0, json.Length - 10);
            if (kind == "no-lf") json = json.TrimEnd('\n');
            if (kind == "extra-lf") json += "\n";
            if (kind == "trailing-space") json += " ";
            if (kind == "second-root") json += "{}";
            Rejected(Bytes(json), 4);
        }

        [TestCase("c080"), TestCase("eda080"), TestCase("f4908080"), TestCase("e282"), TestCase("80")]
        public void InvalidUtf8SequencesAreRejected(string hex)
        {
            var json = Document(Rows()).Replace("Value {name}", "UTF8MARK{name}");
            var marker = json.IndexOf("UTF8MARK", StringComparison.Ordinal);
            var invalid = Enumerable.Range(0, hex.Length / 2).Select(i => byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            var bytes = Bytes(json.Substring(0, marker)).Concat(invalid).Concat(Bytes(json.Substring(marker + 8))).ToArray();
            Rejected(bytes, 4, "LocalizationCatalogInvalidUtf8");
        }

        [TestCase("\\ud800"), TestCase("\\udc00"), TestCase("\\ud800\\u0041")]
        public void IsolatedEscapedSurrogatesAreRejected(string escape)
            => Rejected(Bytes(Document(Rows()).Replace("Value {name}", escape + "{name}")), 4, "LocalizationCatalogInvalidUtf8");

        [Test]
        public void BomAndUnescapedControlAreRejectedButUnicodeScalarsRoundTrip()
        {
            Rejected(new byte[] { 0xef, 0xbb, 0xbf }.Concat(Bytes(Document(Rows()))).ToArray(), 4, "LocalizationCatalogInvalidUtf8");
            Rejected(Bytes(Document(Rows()).Replace("Value {name}", "\u0001{name}")), 4, "LocalizationCatalogInvalidDocument");
            var rows = Rows(); rows.Last().English = "😀{name}";
            var document = Document(rows);
            foreach (var json in new[] { document, document.Replace("😀", "\\ud83d\\ude00") })
                Assert.AreEqual("😀ok", Parse(json, 4).Resolve(TestKey, LocaleId.En, new[] { Pair("name", "ok") }).Text);
        }

        [TestCase("key", 127, true), TestCase("key", 128, true), TestCase("key", 129, false)]
        [TestCase("text", 2047, true), TestCase("text", 2048, true), TestCase("text", 2049, false)]
        [TestCase("parameter-name", 63, true), TestCase("parameter-name", 64, true), TestCase("parameter-name", 65, false)]
        [TestCase("parameters", 15, true), TestCase("parameters", 16, true), TestCase("parameters", 17, false)]
        [TestCase("metadata", 4095, true), TestCase("metadata", 4096, true), TestCase("metadata", 4097, false)]
        public void TokenLimitsAreCheckedAtTheirBoundaries(string kind, int size, bool accepted)
        {
            var rows = Rows(); var row = rows.Last();
            switch (kind)
            {
                case "key": row.Key = "fm." + new string('a', size - 3); break;
                case "text": row.English = row.Chinese = new string('x', size); row.Parameters = Array.Empty<string>(); break;
                case "parameter-name": row.Parameters = new[] { new string('a', size) }; row.English = row.Chinese = "{" + row.Parameters[0] + "}"; break;
                case "parameters":
                    row.Parameters = Enumerable.Range(0, size).Select(i => "p" + i.ToString("D2", CultureInfo.InvariantCulture)).ToArray();
                    row.English = row.Chinese = string.Join(" ", row.Parameters.Select(p => "{" + p + "}")); break;
                case "metadata": row.Notes = new string('x', size); break;
            }
            var bytes = Bytes(Document(rows));
            if (accepted) Parse(bytes, rows.Count);
            else Rejected(bytes, rows.Count, "LocalizationCatalogLimitExceeded");
        }

        [TestCase(1023, true), TestCase(1024, true), TestCase(1025, false)]
        public void RecordLimitIsCheckedBeforeAddingAnotherEntry(int count, bool accepted)
        {
            var bytes = Bytes(Document(ManyRows(count)));
            if (accepted) Parse(bytes, count);
            else Rejected(bytes, 1024, "LocalizationCatalogLimitExceeded");
        }

        [TestCase(1048575, true), TestCase(1048576, true), TestCase(1048577, false)]
        public void ByteLimitPrecedesDecoding(int size, bool accepted)
        {
            var json = Document(Rows());
            var bytes = Bytes(json.Insert(1, new string(' ', size - Bytes(json).Length)));
            Assert.AreEqual(size, bytes.Length);
            if (accepted) Parse(bytes, 4);
            else Rejected(bytes, 4, "LocalizationCatalogLimitExceeded");
        }

        [TestCase(524287, true), TestCase(524288, true), TestCase(524289, false)]
        public void TotalDecodedStringBudgetIncludesMetadataAndPropertyNames(int size, bool accepted)
        {
            var rows = ManyRows(135);
            var remaining = size - DecodedChars(rows);
            foreach (var row in rows)
            {
                var add = Math.Min(4095, remaining);
                row.Notes = new string('x', add + 1); remaining -= add;
            }
            Assert.AreEqual(0, remaining); Assert.AreEqual(size, DecodedChars(rows));
            var bytes = Bytes(Document(rows));
            Assert.Less(bytes.Length, 1048576);
            if (accepted) Parse(bytes, rows.Count);
            else Rejected(bytes, rows.Count, "LocalizationCatalogLimitExceeded");
        }

        private sealed class Row
        {
            internal string Key, English = "Text", Chinese = "文本", Severity = "info", Notes = "q";
            internal string[] Parameters = Array.Empty<string>();
            internal Row(string key) { Key = key; }
        }

        private static List<Row> Rows() => new List<Row>
        {
            new Row("fm.diagnostic.placeholder") { English = Placeholder, Chinese = Placeholder, Severity = "blocking" },
            new Row("fm.language.save_failed") { English = "Save failed: {errorCode}", Chinese = "保存失败：{errorCode}", Severity = "error", Parameters = new[] { "errorCode" } },
            new Row("fm.language.save_unknown") { English = "Save unknown: {errorCode}", Chinese = "保存结果未知：{errorCode}", Severity = "error", Parameters = new[] { "errorCode" } },
            new Row(TestKey) { English = "Value {name}", Chinese = "数值{name}", Parameters = new[] { "name" } }
        };

        private static List<Row> ManyRows(int count)
        {
            var rows = Rows();
            for (var i = rows.Count; i < count; i++) rows.Add(new Row("fm.test." + i.ToString("D4", CultureInfo.InvariantCulture)));
            return rows;
        }

        private static int DecodedChars(IEnumerable<Row> rows)
        {
            const string rootStrings = "schemaVersion" + "locales" + "entries" + "en" + "zh-Hans";
            const string fieldNames = "key" + "screen" + "element" + "stateOrTrigger" + "texts" + "en" + "zh-Hans" + "parameters" + "severity" + "qaNotes";
            return rootStrings.Length + rows.Sum(r => fieldNames.Length + r.Key.Length + 3 + r.English.Length +
                r.Chinese.Length + r.Parameters.Sum(p => p.Length) + r.Severity.Length + r.Notes.Length);
        }

        private static string Document(IEnumerable<Row> rows, bool sorted = true)
        {
            if (sorted) rows = rows.OrderBy(r => r.Key, StringComparer.Ordinal);
            return "{\"schemaVersion\":1,\"locales\":[\"en\",\"zh-Hans\"],\"entries\":[" + string.Join(",", rows.Select(r =>
                "{\"key\":" + Quote(r.Key) + ",\"screen\":\"s\",\"element\":\"e\",\"stateOrTrigger\":\"v\",\"texts\":{\"en\":" + Quote(r.English) +
                ",\"zh-Hans\":" + Quote(r.Chinese) + "},\"parameters\":[" + string.Join(",", r.Parameters.Select(Quote)) +
                "],\"severity\":" + Quote(r.Severity) + ",\"qaNotes\":" + Quote(r.Notes) + "}")) + "]}\n";
        }

        private static string Quote(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t") + "\"";
        private static byte[] Bytes(string text) => new UTF8Encoding(false, true).GetBytes(text);
        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return string.Concat(sha.ComputeHash(bytes).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
        }
        private static KeyValuePair<string, string> Pair(string name, string value) => new KeyValuePair<string, string>(name, value);
        private static FightMatchLocalizedTextSourceAdapter Parse(string json, int count) => Parse(Bytes(json), count);
        private static FightMatchLocalizedTextSourceAdapter Parse(byte[] bytes, int count)
        {
            Assert.IsTrue(FightMatchLocalizationCatalogParser.TryParse(bytes, Hash(bytes), count, out var source, out var code), code);
            Assert.IsNotNull(source); Assert.IsNull(code); return source;
        }
        private static void Rejected(byte[] bytes, int count, string expected = null)
        {
            Assert.IsFalse(FightMatchLocalizationCatalogParser.TryParse(bytes, Hash(bytes), count, out var source, out var code));
            Assert.IsNull(source); Assert.IsNotNull(code);
            if (expected != null) Assert.AreEqual(expected, code);
            Assert.That(code, Does.StartWith("LocalizationCatalog"));
            Assert.That(code, Does.Not.Contain("fm."));
        }
    }
}
