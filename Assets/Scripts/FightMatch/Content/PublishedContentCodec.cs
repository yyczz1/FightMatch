using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FightMatch.Core;
using FlowPuzzle.Core;
using static FightMatch.Content.ContentChecks;

namespace FightMatch.Content
{
    // Fixed input type graphs; no type tags, runtime type loading, permissive JSON library, or floating point.
    public static class PublishedContentCodec
    {
        internal static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        public static string Sha256(byte[] bytes)
        { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        public static PublicationResult<byte[]> EncodeSource(PublishedSource source, ContentConsumerCapabilities limits, ExactMathBudget math)
        { return PublicationResult<byte[]>.Run(() => Encode(source, limits.MaxSourceBytes, limits, math)); }
        public static PublicationResult<PublishedSource> DecodeSource(byte[] bytes, ContentConsumerCapabilities limits, ExactMathBudget math)
        {
            return PublicationResult<PublishedSource>.Run(() => {
                var value = Decode<PublishedSource>(bytes, limits.MaxSourceBytes, limits, math);
                Need(value.SchemaVersion == 1, "UnsupportedSchema", "SchemaVersion");
                value.OriginalBytes = (byte[])bytes.Clone(); value.OriginalCanonicalSha = Sha256(Encode(value, limits.MaxSourceBytes, limits, math));
                return value;
            });
        }
        public static PublicationResult<ContentReviewEvidence> DecodeReview(byte[] bytes, ContentStoreBudget budget)
        {
            return PublicationResult<ContentReviewEvidence>.Run(() => {
                var r = Decode<ContentReviewEvidence>(bytes, Math.Min(65536, budget.MaxRecordBytes), ContentConsumerCapabilities.Current, budget.Math);
                r.OriginalBytes = (byte[])bytes.Clone(); r.OriginalCanonicalSha = Sha256(Encode(r, 65536, ContentConsumerCapabilities.Current, budget.Math)); return r;
            });
        }
        public static PublicationResult<byte[]> EncodeReview(ContentReviewEvidence review, ContentStoreBudget budget)
        { return PublicationResult<byte[]>.Run(() => Encode(review, Math.Min(65536, budget.MaxRecordBytes), ContentConsumerCapabilities.Current, budget.Math)); }
        public static PublicationResult<byte[]> EncodeReleaseSet(ContentReleaseSet release, ContentStoreBudget budget)
        { return PublicationResult<byte[]>.Run(() => Encode(release, Math.Min(65536, budget.MaxRecordBytes), ContentConsumerCapabilities.Current, budget.Math)); }
        internal static byte[] Encode(object value, int maxBytes, ContentConsumerCapabilities limits, ExactMathBudget math)
        {
            Root(limits, "Limits"); Root(math, "Math");
            try
            {
                using (var stream = new MemoryStream())
                {
                    Write(stream, Project(value, limits, math, 0, new ProjectionBudget(maxBytes)), maxBytes, limits, 0);
                    return stream.ToArray();
                }
            }
            catch (EncoderFallbackException) { throw new ContentFailure("InvalidUnicode", "String"); }
        }
        internal static T Decode<T>(byte[] bytes, int maxBytes, ContentConsumerCapabilities limits, ExactMathBudget math)
        {
            Root(bytes, "Bytes"); Need(bytes.Length <= maxBytes, "BudgetExceeded", "Bytes");
            try
            {
                var node = new Parser(Utf8.GetString(bytes), limits).Parse(); Need(node != null, "InvalidSchema", "Root");
                return (T)ConvertValue(node, typeof(T), limits, math, 0);
            }
            catch (DecoderFallbackException) { throw new ContentFailure("InvalidUnicode", "Bytes"); }
            catch (EncoderFallbackException) { throw new ContentFailure("InvalidUnicode", "String"); }
        }
        internal static object Parse(byte[] bytes, int maxBytes, ContentConsumerCapabilities limits)
        {
            Root(bytes, "Bytes"); Need(bytes.Length <= maxBytes, "BudgetExceeded", "Bytes");
            try { return new Parser(Utf8.GetString(bytes), limits).Parse(); }
            catch (DecoderFallbackException) { throw new ContentFailure("InvalidUnicode", "Bytes"); }
        }
        internal static byte[] ReadBounded(string path, int maxBytes)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Need(stream.Length <= maxBytes, "BudgetExceeded", "File"); var bytes = new byte[(int)stream.Length]; var at = 0;
                while (at < bytes.Length) { var n = stream.Read(bytes, at, bytes.Length - at); Need(n > 0, "RecoveryBlocked", "TruncatedFile"); at += n; }
                Need(stream.ReadByte() == -1, "RecoveryBlocked", "GrowingFile"); return bytes;
            }
        }
        private static PropertyInfo[] Properties(Type type) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0).OrderBy(p => p.Name, StringComparer.Ordinal).ToArray();
        private sealed class ProjectionBudget
        {
            private long remaining;
            private readonly Dictionary<Type, PropertyInfo[]> properties = new Dictionary<Type, PropertyInfo[]>();
            internal ProjectionBudget(int bytes) { remaining = bytes; }
            internal void Take(long count) { Need(count <= remaining, "BudgetExceeded", "ProjectionBytes"); remaining -= count; }
            internal PropertyInfo[] PropertiesFor(Type type)
            {
                if (!properties.TryGetValue(type, out var found))
                { found = Properties(type); properties.Add(type, found); }
                return found;
            }
        }
        private static object Project(object value, ContentConsumerCapabilities limits, ExactMathBudget math, int depth, ProjectionBudget work)
        {
            Need(depth <= 64, "BudgetExceeded", "Depth"); work.Take(1);
            if (value == null || value is bool) return value;
            if (value is string text) { Need(text.Length <= limits.MaxStringCodeUnits, "BudgetExceeded", "String"); work.Take(Utf8.GetByteCount(text)); return text; }
            if (value is Enum) return value.ToString();
            if (value is BigInteger big) { ExactRational.Create(big, 1, math); var token = big.ToString(CultureInfo.InvariantCulture);
                Need(token.Length <= 4096, "BudgetExceeded", "Integer"); work.Take(token.Length); return token; }
            if (value is ExactRational r) return Project(new SortedDictionary<string, object>(StringComparer.Ordinal) {
                ["numerator"] = r.Numerator, ["denominator"] = r.Denominator }, limits, math, depth + 1, work);
            if (value is byte || value is int || value is uint || value is long) return new JsonNumber(Convert.ToString(value, CultureInfo.InvariantCulture));
            if (value is ulong wide) return wide.ToString(CultureInfo.InvariantCulture);
            if (value is FlowPos pos) return Project(new { x = pos.x, y = pos.y }, limits, math, depth + 1, work);
            if (value is PreparedPublishedRuleContext pc) return Project(ContentBindingRecord.From(pc.Binding), limits, math, depth + 1, work);
            if (value is PublishedRuleContext rc) return Project(ContentBindingRecord.From(rc.Binding), limits, math, depth + 1, work);
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            if (value is IDictionary dict)
            {
                Need(dict.Count <= limits.MaxCollectionEntries, "BudgetExceeded", "Object");
                foreach (DictionaryEntry item in dict) { work.Take(Utf8.GetByteCount((string)item.Key)); result.Add((string)item.Key, Project(item.Value, limits, math, depth + 1, work)); }
                return result;
            }
            if (value is IEnumerable sequence)
            {
                if (value is ICollection collection) Need(collection.Count <= limits.MaxCollectionEntries, "BudgetExceeded", "Collection");
                var list = new List<object>(); foreach (var item in sequence)
                { Need(list.Count < limits.MaxCollectionEntries, "BudgetExceeded", "Collection"); list.Add(Project(item, limits, math, depth + 1, work)); }
                return list;
            }
            foreach (var p in work.PropertiesFor(value.GetType())) { work.Take(Utf8.GetByteCount(p.Name)); result.Add(p.Name, Project(p.GetValue(value), limits, math, depth + 1, work)); }
            Need(result.Count > 0, "UnsupportedSchema", "Type"); return result;
        }
        private static object ConvertValue(object node, Type type, ContentConsumerCapabilities limits, ExactMathBudget math, int depth)
        {
            Need(depth <= 64, "BudgetExceeded", "Depth");
            var optional = Nullable.GetUnderlyingType(type);
            if (node == null) { Need(!type.IsValueType || optional != null, "MissingField", type.Name); return null; }
            if (optional != null) type = optional;
            if (type == typeof(string)) { Need(node is string, "InvalidValue", "String"); return node; }
            if (type == typeof(bool)) { Need(node is bool, "InvalidValue", "Boolean"); return node; }
            if (type.IsEnum)
            { Need(node is string && Enum.IsDefined(type, node), "UnsupportedBinding", type.Name); return Enum.Parse(type, (string)node); }
            if (type == typeof(BigInteger)) { Need(node is string, "InvalidValue", "Integer"); return Integer((string)node, math); }
            if (type == typeof(int) || type == typeof(byte))
            {
                Need(node is JsonNumber, "InvalidValue", "Number"); var number = Integer(((JsonNumber)node).Text, math);
                Need(number >= (type == typeof(byte) ? 0 : int.MinValue) && number <= (type == typeof(byte) ? 255 : int.MaxValue), "InvalidValue", "Number");
                return type == typeof(byte) ? (object)(byte)number : (int)number;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) || type == typeof(byte[]))
            {
                Need(node is List<object>, "InvalidValue", "Array"); var list = (List<object>)node;
                Need(list.Count <= limits.MaxCollectionEntries, "BudgetExceeded", "Array"); var element = type == typeof(byte[]) ? typeof(byte) : type.GetGenericArguments()[0];
                if (type == typeof(byte[])) return list.Select(n => (byte)ConvertValue(n, element, limits, math, depth + 1)).ToArray();
                var target = (IList)Activator.CreateInstance(type); foreach (var n in list) target.Add(ConvertValue(n, element, limits, math, depth + 1)); return target;
            }
            Need(node is SortedDictionary<string, object>, "InvalidValue", type.Name); var fields = (SortedDictionary<string, object>)node;
            if (type == typeof(ExactRational))
            {
                Need(fields.Count == 2 && fields.ContainsKey("numerator") && fields.ContainsKey("denominator"), "InvalidSchema", "Rational");
                var n = (BigInteger)ConvertValue(fields["numerator"], typeof(BigInteger), limits, math, depth + 1);
                var d = (BigInteger)ConvertValue(fields["denominator"], typeof(BigInteger), limits, math, depth + 1);
                Need(d.Sign > 0, "InvalidValue", "Denominator"); var r = ExactRational.Create(n, d, math);
                Need(r.Numerator == n && r.Denominator == d, "NonCanonical", "Rational"); return r;
            }
            if (type == typeof(FlowPos))
            {
                Need(fields.Count == 2 && fields.ContainsKey("x") && fields.ContainsKey("y"), "InvalidSchema", "Cell");
                return new FlowPos((int)ConvertValue(fields["x"], typeof(int), limits, math, depth + 1), (int)ConvertValue(fields["y"], typeof(int), limits, math, depth + 1));
            }
            Need(!type.IsAbstract && (type.Namespace == "FightMatch.Content" || type.Namespace == "FightMatch.Core"), "UnsupportedSchema", type.Name);
            var properties = Properties(type); Need(properties.All(p => p.SetMethod?.IsPublic == true), "UnsupportedSchema", type.Name);
            Need(fields.Count == properties.Length && properties.All(p => fields.ContainsKey(p.Name)), "InvalidSchema", type.Name);
            var instance = Activator.CreateInstance(type);
            foreach (var property in properties) property.SetValue(instance, ConvertValue(fields[property.Name], property.PropertyType, limits, math, depth + 1));
            return instance;
        }
        private static BigInteger Integer(string token, ExactMathBudget math)
        {
            Need(token.Length > 0 && token.Length <= 4096 && token.Length <= math.MaxIntegerBits + 1, "BudgetExceeded", "Integer");
            var start = token[0] == '-' ? 1 : 0;
            Need(start < token.Length && (token.Length - start == 1 || token[start] != '0') && token != "-0" &&
                token.Skip(start).All(c => c >= '0' && c <= '9'), "NonCanonical", "Integer");
            var value = BigInteger.Parse(token, CultureInfo.InvariantCulture); ExactRational.Create(value, 1, math); return value;
        }
        private sealed class JsonNumber { internal readonly string Text; internal JsonNumber(string text) { Text = text; } }
        private static void Put(MemoryStream stream, string text, int maximum)
        {
            if (text.Length == 1 && text[0] < 128)
            {
                Need(1 <= maximum - stream.Length, "BudgetExceeded", "EncodedBytes");
                stream.WriteByte((byte)text[0]); return;
            }
            var count = Utf8.GetByteCount(text); Need(count <= maximum - stream.Length, "BudgetExceeded", "EncodedBytes");
            var bytes = Utf8.GetBytes(text); stream.Write(bytes, 0, bytes.Length);
        }
        private static string Quote(string text)
        {
            var plain = true;
            for (var i = 0; i < text.Length; i++)
                if (text[i] < 32 || text[i] == '"' || text[i] == '\\') { plain = false; break; }
            if (plain) return "\"" + text + "\"";
            var b = new StringBuilder("\""); foreach (var c in text)
            { if (c == '"' || c == '\\') b.Append('\\').Append(c); else if (c < 32) b.Append("\\u").Append(((int)c).ToString("x4")); else b.Append(c); }
            return b.Append('"').ToString();
        }
        private static void Write(MemoryStream stream, object node, int max, ContentConsumerCapabilities limits, int depth)
        {
            Need(depth <= 64, "BudgetExceeded", "Depth");
            if (node == null) Put(stream, "null", max);
            else if (node is string s) Put(stream, Quote(s), max);
            else if (node is bool b) Put(stream, b ? "true" : "false", max);
            else if (node is JsonNumber number) Put(stream, number.Text, max);
            else if (node is List<object> list)
            { Put(stream, "[", max); for (var i = 0; i < list.Count; i++) { if (i > 0) Put(stream, ",", max); Write(stream, list[i], max, limits, depth + 1); } Put(stream, "]", max); }
            else
            {
                Put(stream, "{", max); var first = true;
                foreach (var entry in (SortedDictionary<string, object>)node)
                { if (!first) Put(stream, ",", max); first = false; Put(stream, Quote(entry.Key) + ":", max); Write(stream, entry.Value, max, limits, depth + 1); }
                Put(stream, "}", max);
            }
        }
        private sealed class Parser
        {
            private readonly string text; private readonly ContentConsumerCapabilities limits; private int at; private int nodes;
            internal Parser(string text, ContentConsumerCapabilities limits) { this.text = text; this.limits = limits; }
            internal object Parse() { var value = Value(0); Space(); Need(at == text.Length, "InvalidSchema", "TrailingBytes"); return value; }
            private void Space() { while (at < text.Length && (text[at] == ' ' || text[at] == '\r' || text[at] == '\n' || text[at] == '\t')) at++; }
            private bool Eat(char c) { Space(); if (at >= text.Length || text[at] != c) return false; at++; return true; }
            private void Expect(char c) { Need(Eat(c), "InvalidSchema", "JSON"); }
            private object Value(int depth)
            {
                Need(depth <= 64 && ++nodes <= (long)limits.MaxCollectionEntries * 128, "BudgetExceeded", "JSON.Nodes"); Space();
                Need(at < text.Length, "InvalidSchema", "JSON.End"); var c = text[at];
                if (c == '"') return String();
                if (c == '{')
                {
                    at++; var map = new SortedDictionary<string, object>(StringComparer.Ordinal); if (Eat('}')) return map;
                    do { Space(); var key = String(); Expect(':'); Need(map.Count < limits.MaxCollectionEntries, "BudgetExceeded", "JSON.Object");
                        Need(!map.ContainsKey(key), "InvalidSchema", "DuplicateProperty"); map.Add(key, Value(depth + 1)); } while (Eat(',')); Expect('}'); return map;
                }
                if (c == '[')
                {
                    at++; var list = new List<object>(); if (Eat(']')) return list;
                    do { Need(list.Count < limits.MaxCollectionEntries, "BudgetExceeded", "JSON.Array"); list.Add(Value(depth + 1)); } while (Eat(',')); Expect(']'); return list;
                }
                foreach (var word in new[] { "null", "true", "false" }) if (at + word.Length <= text.Length && string.CompareOrdinal(text, at, word, 0, word.Length) == 0)
                { at += word.Length; return word == "null" ? null : (object)(word == "true"); }
                var start = at; if (c == '-') at++; while (at < text.Length && text[at] >= '0' && text[at] <= '9') at++;
                Need(at > start && at - start <= 4096, "InvalidSchema", "JSON.Number"); return new JsonNumber(text.Substring(start, at - start));
            }
            private string String()
            {
                Expect('"'); var b = new StringBuilder(); var closed = false;
                while (at < text.Length)
                {
                    var c = text[at++]; if (c == '"') { closed = true; break; }
                    Need(c >= 32, "InvalidSchema", "JSON.Control");
                    if (c == '\\')
                    {
                        Need(at < text.Length, "InvalidSchema", "JSON.Escape"); c = text[at++];
                        if (c == 'u')
                        {
                            Need(at + 4 <= text.Length, "InvalidSchema", "JSON.Unicode"); var v = 0;
                            for (var i = 0; i < 4; i++) { var digit = text[at++]; var n = digit >= '0' && digit <= '9' ? digit - '0' : digit >= 'a' && digit <= 'f' ? digit - 'a' + 10 : digit >= 'A' && digit <= 'F' ? digit - 'A' + 10 : -1; Need(n >= 0, "InvalidSchema", "JSON.Unicode"); v = v * 16 + n; }
                            c = (char)v;
                        }
                        else { var index = "\"\\/bfnrt".IndexOf(c); Need(index >= 0, "InvalidSchema", "JSON.Escape"); c = "\"\\/\b\f\n\r\t"[index]; }
                    }
                    Need(b.Length < limits.MaxStringCodeUnits, "BudgetExceeded", "JSON.String"); b.Append(c);
                }
                Need(closed, "InvalidSchema", "JSON.StringEnd"); var value = b.ToString();
                try { Utf8.GetByteCount(value); } catch (EncoderFallbackException) { throw new ContentFailure("InvalidUnicode", "JSON.String"); } return value;
            }
        }
    }
}
