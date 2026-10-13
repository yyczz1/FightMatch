using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FightMatch.Presentation;

namespace FightMatch.Host
{
    internal static class FightMatchLocalizationCatalogParser
    {
        internal const int MaxArtifactBytes = 1024 * 1024;
        internal const int MaxRecords = 1024;
        internal const int MaxKeyChars = 128;
        internal const int MaxTextChars = 2048;
        internal const int MaxParameters = 16;
        internal const int MaxParameterNameChars = 64;
        internal const int MaxMetadataChars = 4096;
        internal const int MaxTotalDecodedChars = 524288;
        internal const int MaxDepth = 8;
        private const string Placeholder = "【if you see this, it is a bug.】";

        // The caller owns bytes and keeps them unchanged until this synchronous call returns.
        internal static bool TryParse(byte[] bytes, string expectedSha256, int expectedRecordCount,
            out FightMatchLocalizedTextSourceAdapter source, out string diagnosticCode)
        {
            source = null;
            diagnosticCode = null;
            try
            {
                Require(bytes != null && bytes.Length != 0, "LocalizationCatalogInvalidDocument");
                Require(bytes.Length <= MaxArtifactBytes, "LocalizationCatalogLimitExceeded");
                Require(expectedRecordCount > 0 && expectedRecordCount <= MaxRecords,
                    "LocalizationCatalogInvalidCount");
                Require(HashMatches(bytes, expectedSha256), "LocalizationCatalogHashMismatch");
                var entries = new Reader(bytes).ReadCatalog(expectedRecordCount);
                source = new FightMatchLocalizedTextSourceAdapter(entries);
                return true;
            }
            catch (CatalogFailure failure) { diagnosticCode = failure.Code; return false; }
            catch (CryptographicException) { diagnosticCode = "LocalizationCatalogHashFailure"; return false; }
        }

        private static bool HashMatches(byte[] bytes, string expected)
        {
            if (expected == null || expected.Length != 64) return false;
            const string hex = "0123456789abcdef";
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                for (var i = 0; i < hash.Length; i++)
                    if (expected[i * 2] != hex[hash[i] >> 4] || expected[i * 2 + 1] != hex[hash[i] & 15])
                        return false;
            }
            return true;
        }

        private static void Require(bool valid, string code)
        {
            if (!valid) throw new CatalogFailure(code);
        }

        private sealed class CatalogFailure : Exception
        {
            internal readonly string Code;
            internal CatalogFailure(string code) { Code = code; }
        }

        // Only the canonical schema is read. Unknown values are never skipped or materialized.
        private sealed class Reader
        {
            private readonly byte[] bytes;
            private int position, depth, decodedChars;

            internal Reader(byte[] bytes) { this.bytes = bytes; }

            internal List<FightMatchLocalizedTextSourceAdapter.Entry> ReadCatalog(int expectedCount)
            {
                Require(!(bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf),
                    "LocalizationCatalogInvalidUtf8");
                Open('{');
                Property("schemaVersion"); Expect('1'); Expect(',');
                Property("locales"); Open('[');
                Require(ReadString(7) == "en", "LocalizationCatalogInvalidSchema"); Expect(',');
                Require(ReadString(7) == "zh-Hans", "LocalizationCatalogInvalidSchema"); Close(']'); Expect(',');
                Property("entries"); Open('[');
                var entries = new List<FightMatchLocalizedTextSourceAdapter.Entry>();
                string previous = null;
                var hasPlaceholder = false; var hasSaveFailed = false; var hasSaveUnknown = false;
                if (!At(']'))
                {
                    do
                    {
                        Require(entries.Count < MaxRecords, "LocalizationCatalogLimitExceeded");
                        var entry = ReadEntry(out var parameters);
                        Require(previous == null || string.CompareOrdinal(previous, entry.Key) < 0,
                            "LocalizationCatalogInvalidKey");
                        previous = entry.Key;
                        if (entry.Key == "fm.diagnostic.placeholder")
                        {
                            Require(entry.English == Placeholder && entry.Chinese == Placeholder &&
                                entry.Severity == LocalizedTextSeverity.Blocking,
                                "LocalizationCatalogRequiredEntryInvalid");
                            hasPlaceholder = true;
                        }
                        if (entry.Key == "fm.language.save_failed" || entry.Key == "fm.language.save_unknown")
                        {
                            Require(parameters.Count == 1 && parameters[0] == "errorCode" &&
                                entry.Severity == LocalizedTextSeverity.Error,
                                "LocalizationCatalogRequiredEntryInvalid");
                            if (entry.Key == "fm.language.save_failed") hasSaveFailed = true;
                            else hasSaveUnknown = true;
                        }
                        entries.Add(entry);
                    } while (Comma());
                }
                Close(']'); Close('}');
                // The compiler writes exactly one LF after the root, with no extra whitespace.
                Require(position == bytes.Length - 1 && bytes[position] == '\n', "LocalizationCatalogInvalidDocument");
                Require(entries.Count == expectedCount, "LocalizationCatalogCountMismatch");
                Require(hasPlaceholder && hasSaveFailed && hasSaveUnknown, "LocalizationCatalogRequiredEntryMissing");
                return entries;
            }

            private FightMatchLocalizedTextSourceAdapter.Entry ReadEntry(out List<string> parameters)
            {
                Open('{');
                Property("key"); var key = RequiredString(MaxKeyChars);
                Require(key.StartsWith("fm.", StringComparison.Ordinal), "LocalizationCatalogInvalidKey"); Expect(',');
                Property("screen"); RequiredString(MaxMetadataChars); Expect(',');
                Property("element"); RequiredString(MaxMetadataChars); Expect(',');
                Property("stateOrTrigger"); RequiredString(MaxMetadataChars); Expect(',');
                Property("texts"); Open('{');
                Property("en"); var english = RequiredString(MaxTextChars); Expect(',');
                Property("zh-Hans"); var chinese = RequiredString(MaxTextChars); Close('}'); Expect(',');
                Property("parameters"); Open('[');
                parameters = new List<string>();
                if (!At(']'))
                {
                    do
                    {
                        Require(parameters.Count < MaxParameters, "LocalizationCatalogLimitExceeded");
                        var parameter = ReadString(MaxParameterNameChars);
                        Require(IsParameterName(parameter) && (parameters.Count == 0 ||
                            string.CompareOrdinal(parameters[parameters.Count - 1], parameter) < 0),
                            "LocalizationCatalogInvalidParameters");
                        parameters.Add(parameter);
                    } while (Comma());
                }
                Close(']'); Expect(',');
                Property("severity"); var severity = ReadSeverity(); Expect(',');
                Property("qaNotes"); RequiredString(MaxMetadataChars); Close('}');
                ValidateTemplate(english, parameters); ValidateTemplate(chinese, parameters);
                return new FightMatchLocalizedTextSourceAdapter.Entry(key, english, chinese, parameters, severity);
            }

            private LocalizedTextSeverity ReadSeverity()
            {
                switch (ReadString(8))
                {
                    case "info": return LocalizedTextSeverity.Info;
                    case "warning": return LocalizedTextSeverity.Warning;
                    case "error": return LocalizedTextSeverity.Error;
                    case "blocking": return LocalizedTextSeverity.Blocking;
                    default: throw new CatalogFailure("LocalizationCatalogInvalidSeverity");
                }
            }

            private static void ValidateTemplate(string text, List<string> parameters)
            {
                foreach (var token in new[] { "OperationId", "CommitId", "FaceId", "PairId", "TargetKind" })
                    Require(text.IndexOf(token, StringComparison.Ordinal) < 0, "LocalizationCatalogUnsafeText");
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (var i = 0; i < text.Length; i++)
                {
                    Require(text[i] != '}', "LocalizationCatalogInvalidParameters");
                    if (text[i] != '{') continue;
                    var close = text.IndexOf('}', i + 1);
                    Require(close > i + 1 && close - i - 1 <= MaxParameterNameChars,
                        "LocalizationCatalogInvalidParameters");
                    var name = text.Substring(i + 1, close - i - 1);
                    Require(IsParameterName(name) && parameters.Contains(name) && seen.Add(name),
                        "LocalizationCatalogInvalidParameters");
                    i = close;
                }
                Require(seen.Count == parameters.Count, "LocalizationCatalogInvalidParameters");
            }

            private static bool IsParameterName(string name)
            {
                if (name.Length == 0 || !IsLetter(name[0])) return false;
                for (var i = 1; i < name.Length; i++)
                    if (!IsLetter(name[i]) && !char.IsDigit(name[i]) && name[i] != '_') return false;
                return true;
            }

            private static bool IsLetter(char value)
                => value >= 'A' && value <= 'Z' || value >= 'a' && value <= 'z';

            private string RequiredString(int maximum)
            {
                var value = ReadString(maximum);
                Require(!string.IsNullOrWhiteSpace(value), "LocalizationCatalogInvalidSchema");
                return value;
            }

            private void Property(string expected)
            {
                Require(ReadString(32) == expected, "LocalizationCatalogInvalidSchema");
                Expect(':');
            }

            private bool At(char token)
            {
                while (position < bytes.Length && (bytes[position] == ' ' || bytes[position] == '\t' ||
                    bytes[position] == '\r' || bytes[position] == '\n')) position++;
                return position < bytes.Length && bytes[position] == token;
            }

            private void Expect(char token)
            {
                Require(At(token), "LocalizationCatalogInvalidSchema"); position++;
            }

            private bool Comma()
            {
                if (!At(',')) return false;
                position++; return true;
            }

            private void Open(char token)
            {
                Expect(token);
                Require(++depth <= MaxDepth, "LocalizationCatalogLimitExceeded");
            }

            private void Close(char token) { Expect(token); depth--; }

            private string ReadString(int maximum)
            {
                Expect('"');
                var start = position; var scan = start; var count = 0;
                while (scan < bytes.Length && bytes[scan] != '"')
                {
                    var scalar = ReadScalar(ref scan);
                    count += scalar > 0xffff ? 2 : 1;
                    Require(count <= maximum && count <= MaxTotalDecodedChars - decodedChars,
                        "LocalizationCatalogLimitExceeded");
                }
                Require(scan < bytes.Length, "LocalizationCatalogInvalidDocument");
                // Validate length, escapes and UTF-8 before allocating the decoded token.
                decodedChars += count;
                var chars = new char[count]; var output = 0; var cursor = start;
                while (cursor < scan)
                {
                    var scalar = ReadScalar(ref cursor);
                    if (scalar <= 0xffff) chars[output++] = (char)scalar;
                    else
                    {
                        scalar -= 0x10000;
                        chars[output++] = (char)(0xd800 + (scalar >> 10));
                        chars[output++] = (char)(0xdc00 + (scalar & 0x3ff));
                    }
                }
                position = scan + 1;
                return new string(chars);
            }

            private int ReadScalar(ref int cursor)
            {
                Require(cursor < bytes.Length, "LocalizationCatalogInvalidDocument");
                var first = bytes[cursor++];
                if (first == '\\')
                {
                    Require(cursor < bytes.Length, "LocalizationCatalogInvalidDocument");
                    switch (bytes[cursor++])
                    {
                        case (byte)'"': return '"';
                        case (byte)'\\': return '\\';
                        case (byte)'/': return '/';
                        case (byte)'b': return '\b';
                        case (byte)'f': return '\f';
                        case (byte)'n': return '\n';
                        case (byte)'r': return '\r';
                        case (byte)'t': return '\t';
                        case (byte)'u':
                            var value = Hex4(ref cursor);
                            if (value >= 0xd800 && value <= 0xdbff)
                            {
                                Require(cursor + 2 <= bytes.Length && bytes[cursor] == '\\' && bytes[cursor + 1] == 'u',
                                    "LocalizationCatalogInvalidUtf8");
                                cursor += 2;
                                var low = Hex4(ref cursor);
                                Require(low >= 0xdc00 && low <= 0xdfff, "LocalizationCatalogInvalidUtf8");
                                return 0x10000 + ((value - 0xd800) << 10) + low - 0xdc00;
                            }
                            Require(value < 0xdc00 || value > 0xdfff, "LocalizationCatalogInvalidUtf8");
                            return value;
                        default: throw new CatalogFailure("LocalizationCatalogInvalidDocument");
                    }
                }
                Require(first >= 0x20, "LocalizationCatalogInvalidDocument");
                if (first < 0x80) return first;
                int following, minimum, scalar;
                if (first >= 0xc2 && first <= 0xdf) { following = 1; minimum = 0x80; scalar = first & 31; }
                else if (first >= 0xe0 && first <= 0xef) { following = 2; minimum = 0x800; scalar = first & 15; }
                else if (first >= 0xf0 && first <= 0xf4) { following = 3; minimum = 0x10000; scalar = first & 7; }
                else throw new CatalogFailure("LocalizationCatalogInvalidUtf8");
                Require(cursor + following <= bytes.Length, "LocalizationCatalogInvalidUtf8");
                for (var i = 0; i < following; i++)
                {
                    var next = bytes[cursor++];
                    Require((next & 0xc0) == 0x80, "LocalizationCatalogInvalidUtf8");
                    scalar = (scalar << 6) | (next & 63);
                }
                Require(scalar >= minimum && scalar <= 0x10ffff && (scalar < 0xd800 || scalar > 0xdfff),
                    "LocalizationCatalogInvalidUtf8");
                return scalar;
            }

            private int Hex4(ref int cursor)
            {
                Require(cursor + 4 <= bytes.Length, "LocalizationCatalogInvalidDocument");
                var value = 0;
                for (var i = 0; i < 4; i++)
                {
                    var c = bytes[cursor++];
                    var digit = c >= '0' && c <= '9' ? c - '0' :
                        c >= 'a' && c <= 'f' ? c - 'a' + 10 : c >= 'A' && c <= 'F' ? c - 'A' + 10 : -1;
                    Require(digit >= 0, "LocalizationCatalogInvalidDocument");
                    value = value * 16 + digit;
                }
                return value;
            }
        }
    }
}
