using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Map = System.Collections.Generic.SortedDictionary<string, object>;
using Items = System.Collections.Generic.List<object>;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.YooAssetAdapter")]

namespace FightMatch.AssetAccess
{
    public sealed class FightMatchResourceReleaseSet
    {
        private const int BootLimit = 262144, DescriptorLimit = 32768, MappingLimit = 131072;
        private const long FileLimit = 268435456, TotalLimit = 1073741824;
        private const long SmallFileLimit = 16777216, BusinessLimit = 33554432;
        private const string Schema = "RES_SCHEMA", Budget = "RES_BUDGET";
        private const string Trust = "RES_TRUST", Hash = "RES_HASH";
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private static readonly string[] BusinessNames =
        {
            "first-release.fmsource.json", "first-release.fmpackage.bytes", "first-release.fmvalidation.bytes",
            "first-release.fmreview.json", "first-release.fmpublish.json", "first-release.fmrelease.json"
        };
        private static readonly string[] BusinessIds =
        {
            "fm.content.source", "fm.content.package", "fm.content.validation",
            "fm.content.review", "fm.content.publication", "fm.content.release-set"
        };
        private static readonly string[] ScopeIds =
        {
            "fm.audio.runtime", "fm.content.first-release", "fm.image.runtime", "fm.text.full", "fm.ui.runtime"
        };
        private readonly byte[] canonical;
        private readonly Dictionary<string, RawEntry> rawEntries = new Dictionary<string, RawEntry>(8, StringComparer.Ordinal);
        internal bool TryGetRawEntry(string assetId, out RawEntry entry)
        {
            entry = null;
            return assetId != null && rawEntries.TryGetValue(assetId, out entry);
        }


        internal ResourcePlanProjection PlanProjection { get; }

        public int SchemaVersion { get; }
        public string ReleaseSetId { get; }
        public string BusinessReleaseSetId { get; }
        public string Platform { get; }
        public string DescriptorSha256 { get; }

        private FightMatchResourceReleaseSet(byte[] bytes, Map root)
        {
            var descriptor = Object(root["descriptor"]);
            var resources = Object(descriptor["resources"]);
            canonical = bytes;
            SchemaVersion = 1;
            ReleaseSetId = Text(descriptor["releaseSetId"]);
            BusinessReleaseSetId = Text(Object(descriptor["businessContent"])["businessReleaseSetId"]);
            Platform = Text(resources["platform"]);
            DescriptorSha256 = Text(root["descriptorSha256"]);
            PlanProjection = new ResourcePlanProjection(root);
            foreach (var node in (Items)Object(root["mapping"])["entries"])
            {
                var entry = Object(node);
                if (Text(entry["kind"]) != "raw") continue;
                var id = Text(entry["assetId"]);
                rawEntries.Add(id, new RawEntry(id, ReleaseSetId, Platform, DescriptorSha256,
                    Text(resources["packageName"]), Text(resources["yooManifestPackageVersion"]),
                    Text(entry["location"]), Text(((Items)entry["files"])[0]),
                    (long)entry["contentLength"], Text(entry["contentSha256"])));
            }
        }

        public static bool TryDecodePinned(byte[] bootBytes, string expectedBootSha256,
            out FightMatchResourceReleaseSet value, out string rejectionCode)
        {
            value = null;
            rejectionCode = null;
            if (bootBytes == null || bootBytes.Length == 0) { rejectionCode = Schema; return false; }
            if (bootBytes.Length > BootLimit) { rejectionCode = Budget; return false; }
            if (!IsHash(expectedBootSha256)) { rejectionCode = Trust; return false; }
            var snapshot = (byte[])bootBytes.Clone();
            if (Sha(snapshot) != expectedBootSha256) { rejectionCode = Trust; return false; }
            try
            {
                var root = Keys(new Parser(snapshot).Parse(),
                    "descriptor", "descriptorSha256", "mapping", "physicalFiles", "schemaVersion");
                var encoded = Canonical(root, BootLimit);
                Need(Equal(snapshot, encoded), Schema);
                Validate(root);
                value = new FightMatchResourceReleaseSet(snapshot, root);
                return true;
            }
            catch (Failure failure) { rejectionCode = failure.Code; }
            catch (DecoderFallbackException) { rejectionCode = Schema; }
            catch (EncoderFallbackException) { rejectionCode = Schema; }
            catch (OverflowException) { rejectionCode = Budget; }
            return false;
        }

        public byte[] EncodeCanonical()
        {
            return (byte[])canonical.Clone();
        }

        private sealed class Failure : Exception
        {
            internal readonly string Code;
            internal Failure(string code) { Code = code; }
        }

        private static void Need(bool condition, string code)
        {
            if (!condition) throw new Failure(code);
        }

        private static bool Equal(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (var i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        private static string Sha(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private static string Digest(object node, int maximum = BootLimit)
        {
            return Sha(Canonical(node, maximum));
        }

        private static void HashEqual(string a, string b) { Need(a == b, Hash); }
        private static Map Object(object node) { Need(node is Map, Schema); return (Map)node; }
        private static string Text(object node) { Need(node is string, Schema); return (string)node; }
        private static Map Keys(object node, params string[] keys)
        {
            var map = Object(node);
            Need(map.Count == keys.Length, Schema);
            foreach (var key in keys) Need(map.ContainsKey(key), Schema);
            return map;
        }

        private static Items Array(object node, int minimum, int maximum)
        {
            Need(node is Items, Schema);
            var array = (Items)node;
            Need(array.Count <= maximum, Budget);
            Need(array.Count >= minimum, Schema);
            return array;
        }

        private static long Number(object node, long maximum)
        {
            Need(node is long, Schema);
            var value = (long)node;
            Need(value > 0, Schema);
            Need(value <= maximum, Budget);
            return value;
        }

        private static void Version(object node) { Need(Number(node, int.MaxValue) == 1, Schema); }
        private static bool Letter(char c) { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z'; }
        private static bool Digit(char c) { return c >= '0' && c <= '9'; }
        private static bool IsHash(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (var c in value) if (!Digit(c) && (c < 'a' || c > 'f')) return false;
            return true;
        }

        private static string HashText(object node)
        {
            var value = Text(node);
            Need(IsHash(value), Schema);
            return value;
        }

        private static string Id(object node, int start = 0)
        {
            var value = Text(node);
            Need(value.Length - start >= 1 && value.Length - start <= 128, Schema);
            Need(value.Length - start != 6 || string.CompareOrdinal(value, start, "latest", 0, 6) != 0, Schema);
            for (var i = start; i < value.Length; i++)
            {
                var c = value[i];
                var first = c >= 'a' && c <= 'z' || Digit(c);
                Need(first || i > start && (c == '.' || c == '_' || c == '-'), Schema);
            }
            return value;
        }

        private static string Token(object node)
        {
            var value = Text(node);
            Need(value.Length >= 1 && value.Length <= 256, Schema);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                Need(Letter(c) || Digit(c) || i > 0 && (c == '.' || c == '_' || c == ':' || c == '+' || c == '-'), Schema);
            }
            return value;
        }

        private static string Path(object node)
        {
            var value = Text(node);
            Need(value.Length >= 1 && value.Length <= 240, Schema);
            var first = true;
            foreach (var c in value)
            {
                if (c == '/') { Need(!first, Schema); first = true; continue; }
                Need(Letter(c) || Digit(c) || !first && (c == '.' || c == '_' || c == '-'), Schema);
                first = false;
            }
            Need(!first, Schema);
            foreach (var forbidden in new[] { "fm-resource-boot-v1.json", "player-boot-pin.sha256", "resource-build-receipt-v1.json" })
                Need(!value.Equals(forbidden, StringComparison.OrdinalIgnoreCase) &&
                    !value.EndsWith("/" + forbidden, StringComparison.OrdinalIgnoreCase), Schema);
            return value;
        }

        private static string PlatformName(object node)
        {
            var value = Text(node);
            Need(value == "android" || value == "macOS" || value == "windows", Schema);
            return value;
        }

        private static string Scope(object node)
        {
            var value = Text(node);
            var known = false;
            foreach (var candidate in ScopeIds) if (candidate == value) known = true;
            Need(known, Schema);
            return value;
        }

        private static void TypeName(object node)
        {
            var value = Text(node);
            Need(value.Length >= 3 && value.Length <= 128, Schema);
            var first = true;
            var dots = 0;
            foreach (var c in value)
            {
                if (c == '.') { Need(!first, Schema); first = true; dots++; continue; }
                Need(Letter(c) || c == '_' || !first && Digit(c), Schema);
                first = false;
            }
            Need(!first && dots > 0, Schema);
        }

        private static void Increasing(ref string previous, string next)
        {
            Need(previous == null || string.CompareOrdinal(previous, next) < 0, Schema);
            previous = next;
        }

        private static void AddLength(ref long total, long next, long maximum)
        {
            Need(next <= maximum - total, Budget);
            total = checked(total + next);
        }

        private static Map FileRecord(object node, long maximum)
        {
            var file = Keys(node, "length", "name", "sha256");
            Path(file["name"]);
            Number(file["length"], maximum);
            HashText(file["sha256"]);
            return file;
        }

        private static void Validate(Map root)
        {
            Version(root["schemaVersion"]);
            var descriptorHash = HashText(root["descriptorSha256"]);
            var descriptor = Keys(root["descriptor"], "businessContent", "codeCompatibility", "publication",
                "releaseSetId", "resources", "schemaVersion", "text");
            Version(descriptor["schemaVersion"]);
            var set = Id(descriptor["releaseSetId"]);
            var business = Keys(descriptor["businessContent"], "binding", "businessReleaseSetId",
                "contentReleaseSetSha256", "files", "publicationReceiptSha256");
            var businessId = Token(business["businessReleaseSetId"]);
            Need(businessId.StartsWith("release-set:", StringComparison.Ordinal), Schema);
            Id(businessId, 12);
            var binding = Keys(business["binding"], "ContentFingerprint", "NumericContractVersion",
                "PackageId", "RandomContractVersion", "RuleVersion");
            HashText(binding["ContentFingerprint"]);
            foreach (var key in new[] { "NumericContractVersion", "PackageId", "RandomContractVersion", "RuleVersion" })
                Token(binding[key]);
            var businessFiles = Array(business["files"], 6, 6);
            long businessBytes = 0;
            var slots = new Dictionary<string, Map>(8, StringComparer.Ordinal);
            for (var i = 0; i < 6; i++)
            {
                var file = FileRecord(businessFiles[i], SmallFileLimit);
                Need(Text(file["name"]) == BusinessNames[i], Schema);
                AddLength(ref businessBytes, (long)file["length"], BusinessLimit);
                slots.Add(BusinessIds[i], file);
            }
            HashEqual(HashText(business["contentReleaseSetSha256"]), Text(Object(businessFiles[5])["sha256"]));
            HashEqual(HashText(business["publicationReceiptSha256"]), Text(Object(businessFiles[4])["sha256"]));
            HashEqual(Text(binding["ContentFingerprint"]), Text(Object(businessFiles[1])["sha256"]));
            var code = Keys(descriptor["codeCompatibility"], "appBuildIdentity", "baseProtocolVersion", "requiredCapabilities");
            Token(code["appBuildIdentity"]);
            Number(code["baseProtocolVersion"], int.MaxValue);
            string previous = null;
            foreach (var capability in Array(code["requiredCapabilities"], 1, 32))
                Increasing(ref previous, Token(capability));
            var text = Keys(descriptor["text"], "artifactLength", "artifactName", "artifactSha256",
                "manifestLength", "manifestName", "manifestSha256", "schemaVersion", "sourceReceipt");
            Need(Text(text["artifactName"]) == "fm-text-v1.json" && Text(text["manifestName"]) == "fm-text-v1.manifest.json", Schema);
            Number(text["schemaVersion"], int.MaxValue);
            slots.Add("fm.text.full.artifact", TextFile(text, "artifact"));
            slots.Add("fm.text.full.manifest", TextFile(text, "manifest"));
            var textReceipt = FileRecord(text["sourceReceipt"], SmallFileLimit);
            var resources = Keys(descriptor["resources"], "manifestLength", "manifestName", "manifestSha256",
                "mappingDescriptorSha256", "packageName", "platform", "requiredScopeHashes",
                "yooAssetPackageVersion", "yooManifestPackageVersion");
            Need(Text(resources["packageName"]) == "FightMatchMain" && Text(resources["yooAssetPackageVersion"]) == "3.0.6", Schema);
            var platform = PlatformName(resources["platform"]);
            Id(resources["yooManifestPackageVersion"]);
            Path(resources["manifestName"]);
            Number(resources["manifestLength"], FileLimit);
            HashText(resources["manifestSha256"]);
            HashText(resources["mappingDescriptorSha256"]);
            var publication = Keys(descriptor["publication"], "authorizationEvidenceId", "operationId", "receipt", "receiptSha256");
            Token(publication["authorizationEvidenceId"]);
            Token(publication["operationId"]);
            var receipt = Keys(publication["receipt"], "authorizationEvidenceId", "businessInputsSha256", "kind",
                "operationId", "schemaVersion", "sourcePlanSha256", "textSourceReceiptSha256");
            Version(receipt["schemaVersion"]);
            Need(Text(receipt["kind"]) == "resource-inputs-receipt-v1", Schema);
            Need(Token(receipt["authorizationEvidenceId"]) == Text(publication["authorizationEvidenceId"]) &&
                Token(receipt["operationId"]) == Text(publication["operationId"]), Schema);
            HashText(receipt["sourcePlanSha256"]);
            HashEqual(HashText(receipt["businessInputsSha256"]), Digest(businessFiles));
            HashEqual(HashText(receipt["textSourceReceiptSha256"]), Text(textReceipt["sha256"]));
            HashEqual(HashText(publication["receiptSha256"]), Digest(receipt));
            var physical = PhysicalFiles(root["physicalFiles"], out var manifest, out var sourceReceipt);
            DirectFile(resources, manifest, "manifest");
            DirectFile(textReceipt, sourceReceipt, "");
            var used = new HashSet<string>(StringComparer.Ordinal) { Text(manifest["name"]), Text(sourceReceipt["name"]) };
            var rawOwners = new HashSet<string>(StringComparer.Ordinal);
            var mapping = Keys(root["mapping"], "entries", "schemaVersion");
            Version(mapping["schemaVersion"]);
            var entries = Array(mapping["entries"], 8, 256);
            var scopeAssets = new SortedDictionary<string, Items>(StringComparer.Ordinal);
            var scopeFiles = new Dictionary<string, Map>(StringComparer.Ordinal);
            var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenSlots = new HashSet<string>(StringComparer.Ordinal);
            previous = null;
            foreach (var item in entries)
            {
                var entry = Keys(item, "assetId", "contentLength", "contentSha256", "files", "kind",
                    "location", "packageName", "platform", "releaseSetId", "scopeId", "unityType");
                var id = Id(entry["assetId"]);
                Increasing(ref previous, id);
                Need(Id(entry["releaseSetId"]) == set && PlatformName(entry["platform"]) == platform &&
                    Text(entry["packageName"]) == "FightMatchMain", Schema);
                Need(locations.Add(Path(entry["location"])), Schema);
                var scope = Scope(entry["scopeId"]);
                var kind = Text(entry["kind"]);
                Need(kind == "raw" || kind == "object", Schema);
                Number(entry["contentLength"], TotalLimit);
                HashText(entry["contentSha256"]);
                if (!scopeAssets.TryGetValue(scope, out var assets))
                {
                    assets = new Items(256);
                    scopeAssets.Add(scope, assets);
                    scopeFiles.Add(scope, new Map(StringComparer.Ordinal));
                }
                Need(assets.Count < 256, Budget);
                assets.Add(entry);
                var referenced = new Items(64);
                string priorFile = null;
                long length = 0;
                foreach (var nameNode in Array(entry["files"], 1, 64))
                {
                    var name = Path(nameNode);
                    Increasing(ref priorFile, name);
                    Need(physical.TryGetValue(name, out var file), Schema);
                    Need(Text(file["kind"]) == (kind == "raw" ? "raw" : "bundle"), Schema);
                    if (kind == "raw") Need(rawOwners.Add(name), Schema);
                    AddLength(ref length, (long)file["length"], TotalLimit);
                    referenced.Add(file);
                    used.Add(name);
                    scopeFiles[scope][name] = file;
                }
                Need((long)entry["contentLength"] == length, Schema);
                if (kind == "raw")
                {
                    Need(Text(entry["unityType"]) == "" && referenced.Count == 1, Schema);
                    Need(slots.TryGetValue(id, out var slot), Schema);
                    Need(scope == (id.StartsWith("fm.content.", StringComparison.Ordinal) ? "fm.content.first-release" : "fm.text.full"), Schema);
                    Need(seenSlots.Add(id) && (long)slot["length"] == length, Schema);
                    HashEqual(Text(entry["contentSha256"]), Text(Object(referenced[0])["sha256"]));
                    HashEqual(Text(entry["contentSha256"]), Text(slot["sha256"]));
                }
                else
                {
                    Need(scope != "fm.content.first-release" && scope != "fm.text.full" && !slots.ContainsKey(id), Schema);
                    TypeName(entry["unityType"]);
                    HashEqual(Text(entry["contentSha256"]), Digest(referenced));
                }
            }
            Need(seenSlots.Count == 8 && used.Count == physical.Count, Schema);
            var scopes = Array(resources["requiredScopeHashes"], 2, 5);
            Need(scopes.Count == scopeAssets.Count, Schema);
            previous = null;
            foreach (var node in scopes)
            {
                var record = Keys(node, "scopeId", "sha256");
                var scope = Scope(record["scopeId"]);
                Increasing(ref previous, scope);
                Need(scopeAssets.TryGetValue(scope, out var assets), Schema);
                var files = new Items(scopeFiles[scope].Count);
                foreach (var file in scopeFiles[scope].Values) files.Add(file);
                var closure = new Map(StringComparer.Ordinal) { ["assets"] = assets, ["files"] = files, ["scopeId"] = scope };
                HashEqual(HashText(record["sha256"]), Digest(closure));
            }
            HashEqual(Text(resources["mappingDescriptorSha256"]), Digest(mapping, MappingLimit));
            HashEqual(descriptorHash, Digest(descriptor, DescriptorLimit));
        }

        private static Map TextFile(Map text, string prefix)
        {
            Number(text[prefix + "Length"], SmallFileLimit);
            HashText(text[prefix + "Sha256"]);
            return new Map(StringComparer.Ordinal)
            {
                ["length"] = text[prefix + "Length"], ["name"] = text[prefix + "Name"], ["sha256"] = text[prefix + "Sha256"]
            };
        }

        private static Dictionary<string, Map> PhysicalFiles(object node, out Map manifest, out Map receipt)
        {
            var array = Array(node, 10, 512);
            var result = new Dictionary<string, Map>(array.Count, StringComparer.Ordinal);
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            manifest = null;
            receipt = null;
            long total = 0;
            string previous = null;
            foreach (var item in array)
            {
                var file = Keys(item, "kind", "length", "name", "sha256");
                var name = Path(file["name"]);
                Increasing(ref previous, name);
                Need(aliases.Add(name), Schema);
                AddLength(ref total, Number(file["length"], FileLimit), TotalLimit);
                HashText(file["sha256"]);
                var kind = Text(file["kind"]);
                Need(kind == "raw" || kind == "bundle" || kind == "manifest" || kind == "receipt", Schema);
                if (kind == "manifest") { Need(manifest == null, Schema); manifest = file; }
                if (kind == "receipt") { Need(receipt == null, Schema); receipt = file; }
                result.Add(name, file);
            }
            Need(manifest != null && receipt != null, Schema);
            return result;
        }

        private static void DirectFile(Map declared, Map physical, string prefix)
        {
            var name = prefix == "" ? "name" : prefix + "Name";
            var length = prefix == "" ? "length" : prefix + "Length";
            var hash = prefix == "" ? "sha256" : prefix + "Sha256";
            Need(Text(declared[name]) == Text(physical["name"]) && (long)declared[length] == (long)physical["length"], Schema);
            HashEqual(Text(declared[hash]), Text(physical["sha256"]));
        }

        private sealed class Parser
        {
            private readonly byte[] bytes;
            private int at, nodes, end;
            internal Parser(byte[] bytes) { this.bytes = bytes; end = bytes.Length; }
            internal object Parse()
            {
                var node = Value(1, "", 512);
                Need(at == bytes.Length, Schema);
                return node;
            }

            private void Node()
            {
                Need(nodes < 16384, Budget);
                nodes++;
            }

            private byte Peek()
            {
                Need(at < bytes.Length, Schema);
                Need(at < end, Budget);
                return bytes[at];
            }

            private byte Take() { var value = Peek(); at++; return value; }
            private void Expect(byte value) { Need(Take() == value, Schema); }

            private object Value(int depth, string context, int arrayMaximum)
            {
                Need(depth <= 12, Budget);
                Node();
                var token = Peek();
                if (token == '"') return String(256);
                if (token == '{')
                {
                    Take();
                    var map = new Map(StringComparer.Ordinal);
                    if (Peek() == '}') { Take(); return map; }
                    string previous = null;
                    while (true)
                    {
                        Need(map.Count < 16, Budget);
                        Node();
                        var key = String(64);
                        Increasing(ref previous, key);
                        Expect((byte)':');
                        var oldEnd = end;
                        if (depth == 1 && (key == "descriptor" || key == "mapping"))
                            end = Math.Min(end, checked(at + (key == "descriptor" ? DescriptorLimit : MappingLimit)));
                        var maximum = key == "entries" ? 256 : key == "requiredCapabilities" ? 32 :
                            key == "requiredScopeHashes" ? 5 : key == "files" ? (context == "businessContent" ? 6 : 64) : 512;
                        var value = Value(depth + 1, key, maximum);
                        end = oldEnd;
                        map.Add(key, value);
                        var separator = Take();
                        if (separator == '}') return map;
                        Need(separator == ',', Schema);
                    }
                }
                if (token == '[')
                {
                    Take();
                    var array = new Items(arrayMaximum);
                    if (Peek() == ']') { Take(); return array; }
                    while (true)
                    {
                        Need(array.Count < arrayMaximum, Budget);
                        array.Add(Value(depth + 1, "", 512));
                        var separator = Take();
                        if (separator == ']') return array;
                        Need(separator == ',', Schema);
                    }
                }
                Need(token >= '1' && token <= '9', Schema);
                long number = 0;
                var digits = 0;
                while (at < bytes.Length && bytes[at] >= '0' && bytes[at] <= '9')
                {
                    Need(digits < 10, Budget);
                    var digit = Take() - '0';
                    Need(number <= (long.MaxValue - digit) / 10, Budget);
                    number = checked(number * 10 + digit);
                    digits++;
                }
                return number;
            }

            private string String(int maximum)
            {
                Expect((byte)'"');
                var buffer = new char[maximum];
                var count = 0;
                var utf8Bytes = 0;
                while (true)
                {
                    var b = Peek();
                    if (b == '"') { Take(); return new string(buffer, 0, count); }
                    Need(b >= 32, Schema);
                    if (b == '\\')
                    {
                        Take();
                        var escape = Take();
                        char c;
                        if (escape == '"' || escape == '\\') c = (char)escape;
                        else
                        {
                            Need(escape == 'u', Schema);
                            Expect((byte)'0');
                            Expect((byte)'0');
                            var high = Hex(Take());
                            var low = Hex(Take());
                            c = (char)(high * 16 + low);
                            Need(c < 32, Schema);
                        }
                        Need(count < maximum && utf8Bytes < 1024, Budget);
                        buffer[count++] = c;
                        utf8Bytes++;
                    }
                    else
                    {
                        var length = b < 128 ? 1 : b >= 0xc2 && b <= 0xdf ? 2 :
                            b >= 0xe0 && b <= 0xef ? 3 : b >= 0xf0 && b <= 0xf4 ? 4 : 0;
                        Need(length > 0 && length <= bytes.Length - at, Schema);
                        Need(length <= end - at && length <= 1024 - utf8Bytes, Budget);
                        var units = Utf8.GetCharCount(bytes, at, length);
                        Need(units <= maximum - count, Budget);
                        Utf8.GetChars(bytes, at, length, buffer, count);
                        count += units;
                        utf8Bytes += length;
                        at += length;
                    }
                }
            }

            private static int Hex(byte b)
            {
                Need(b >= '0' && b <= '9' || b >= 'a' && b <= 'f', Schema);
                return b <= '9' ? b - '0' : b - 'a' + 10;
            }
        }

        private static byte[] Canonical(object node, int maximum)
        {
            var writer = new Writer(maximum);
            writer.Write(node, 1);
            return writer.Finish();
        }

        private sealed class Writer
        {
            private readonly byte[] buffer;
            private int used, nodes;
            internal Writer(int maximum) { buffer = new byte[maximum]; }
            private void Byte(byte value) { Need(used < buffer.Length, Budget); buffer[used++] = value; }
            private void Node() { Need(nodes < 16384, Budget); nodes++; }
            private void Quote(string value)
            {
                Need(value.Length <= 256, Budget);
                var count = Utf8.GetByteCount(value);
                Need(count <= 1024, Budget);
                var encodedLength = count + 2;
                foreach (var c in value) encodedLength += c < 32 ? 5 : c == '"' || c == '\\' ? 1 : 0;
                Need(encodedLength <= buffer.Length - used, Budget);
                var bytes = Utf8.GetBytes(value);
                Byte((byte)'"');
                foreach (var b in bytes)
                {
                    if (b == '"' || b == '\\') { Byte((byte)'\\'); Byte(b); }
                    else if (b < 32)
                    {
                        Byte((byte)'\\'); Byte((byte)'u'); Byte((byte)'0'); Byte((byte)'0');
                        Byte((byte)"0123456789abcdef"[b >> 4]);
                        Byte((byte)"0123456789abcdef"[b & 15]);
                    }
                    else Byte(b);
                }
                Byte((byte)'"');
            }

            internal void Write(object node, int depth)
            {
                Need(depth <= 12, Budget);
                Node();
                if (node is string text) { Quote(text); return; }
                if (node is long number)
                {
                    var digits = number.ToString(CultureInfo.InvariantCulture);
                    Need(digits.Length <= 10 && digits.Length <= buffer.Length - used, Budget);
                    foreach (var c in digits) Byte((byte)c);
                    return;
                }
                if (node is Items array)
                {
                    Need(array.Count <= 512, Budget);
                    Byte((byte)'[');
                    for (var i = 0; i < array.Count; i++)
                    {
                        if (i > 0) Byte((byte)',');
                        Write(array[i], depth + 1);
                    }
                    Byte((byte)']');
                    return;
                }
                var map = Object(node);
                Need(map.Count <= 16, Budget);
                Byte((byte)'{');
                var first = true;
                foreach (var pair in map)
                {
                    Node();
                    Need(pair.Key.Length <= 64, Budget);
                    if (!first) Byte((byte)',');
                    first = false;
                    Quote(pair.Key);
                    Byte((byte)':');
                    Write(pair.Value, depth + 1);
                }
                Byte((byte)'}');
            }

            internal byte[] Finish()
            {
                var result = new byte[used];
                Buffer.BlockCopy(buffer, 0, result, 0, used);
                return result;
            }
        }
    }

    // Values are copied only from the root already accepted by Validate.
    internal sealed class ResourcePlanProjection
    {
        internal int SchemaVersion { get; }
        internal string ReleaseSetId { get; }
        internal string BusinessReleaseSetId { get; }
        internal string Platform { get; }
        internal string DescriptorSha256 { get; }
        internal string AppBuildIdentity { get; }
        internal int BaseProtocolVersion { get; }
        internal IReadOnlyList<string> RequiredCapabilities { get; }
        internal string PackageName { get; }
        internal string YooAssetPackageVersion { get; }
        internal string YooManifestPackageVersion { get; }
        internal ResourcePlanFile Manifest { get; }
        internal IReadOnlyList<ResourcePlanFile> PhysicalFiles { get; }
        internal IReadOnlyList<ResourcePlanMapping> Mappings { get; }
        internal IReadOnlyList<ResourcePlanScopeHash> RequiredScopeHashes { get; }

        internal ResourcePlanProjection(Map root)
        {
            var descriptor = (Map)root["descriptor"];
            var resources = (Map)descriptor["resources"];
            var code = (Map)descriptor["codeCompatibility"];
            SchemaVersion = (int)(long)root["schemaVersion"];
            ReleaseSetId = (string)descriptor["releaseSetId"];
            BusinessReleaseSetId = (string)((Map)descriptor["businessContent"])["businessReleaseSetId"];
            Platform = (string)resources["platform"];
            DescriptorSha256 = (string)root["descriptorSha256"];
            AppBuildIdentity = (string)code["appBuildIdentity"];
            BaseProtocolVersion = (int)(long)code["baseProtocolVersion"];
            RequiredCapabilities = Strings((Items)code["requiredCapabilities"]);
            PackageName = (string)resources["packageName"];
            YooAssetPackageVersion = (string)resources["yooAssetPackageVersion"];
            YooManifestPackageVersion = (string)resources["yooManifestPackageVersion"];
            var physical = (Items)root["physicalFiles"];
            var files = new ResourcePlanFile[physical.Count];
            for (var i = 0; i < files.Length; i++)
            {
                files[i] = new ResourcePlanFile((Map)physical[i]);
                if (files[i].Name == (string)resources["manifestName"]) Manifest = files[i];
            }
            PhysicalFiles = System.Array.AsReadOnly(files);
            var entries = (Items)((Map)root["mapping"])["entries"];
            var mappings = new ResourcePlanMapping[entries.Count];
            for (var i = 0; i < mappings.Length; i++) mappings[i] = new ResourcePlanMapping((Map)entries[i]);
            Mappings = System.Array.AsReadOnly(mappings);
            var scopes = (Items)resources["requiredScopeHashes"];
            var hashes = new ResourcePlanScopeHash[scopes.Count];
            for (var i = 0; i < hashes.Length; i++) hashes[i] = new ResourcePlanScopeHash((Map)scopes[i]);
            RequiredScopeHashes = System.Array.AsReadOnly(hashes);
        }

        internal static IReadOnlyList<string> Strings(Items items)
        {
            var values = new string[items.Count];
            for (var i = 0; i < values.Length; i++) values[i] = (string)items[i];
            return System.Array.AsReadOnly(values);
        }
    }

    internal sealed class ResourcePlanFile
    {
        internal string Name { get; }
        internal string Kind { get; }
        internal long Length { get; }
        internal string Sha256 { get; }
        internal ResourcePlanFile(Map file)
        {
            Name = (string)file["name"]; Kind = (string)file["kind"];
            Length = (long)file["length"]; Sha256 = (string)file["sha256"];
        }
    }

    internal sealed class ResourcePlanMapping
    {
        internal string AssetId { get; }
        internal long ContentLength { get; }
        internal string ContentSha256 { get; }
        internal IReadOnlyList<string> Files { get; }
        internal string Kind { get; }
        internal string Location { get; }
        internal string PackageName { get; }
        internal string Platform { get; }
        internal string ReleaseSetId { get; }
        internal string ScopeId { get; }
        internal string UnityType { get; }
        internal ResourcePlanMapping(Map entry)
        {
            AssetId = (string)entry["assetId"]; ContentLength = (long)entry["contentLength"];
            ContentSha256 = (string)entry["contentSha256"]; Files = ResourcePlanProjection.Strings((Items)entry["files"]);
            Kind = (string)entry["kind"]; Location = (string)entry["location"]; PackageName = (string)entry["packageName"];
            Platform = (string)entry["platform"]; ReleaseSetId = (string)entry["releaseSetId"];
            ScopeId = (string)entry["scopeId"]; UnityType = (string)entry["unityType"];
        }
    }

    internal sealed class ResourcePlanScopeHash
    {
        internal string ScopeId { get; }
        internal string Sha256 { get; }
        internal ResourcePlanScopeHash(Map scope) { ScopeId = (string)scope["scopeId"]; Sha256 = (string)scope["sha256"]; }
    }

    internal sealed class RawEntry
    {
        internal readonly string AssetId, Set, Platform, DescriptorSha, Package, ManifestVersion, Location, PhysicalName, Sha;
        internal readonly long Length;
        internal RawEntry(string assetId, string set, string platform, string descriptorSha, string package,
            string manifestVersion, string location, string physicalName, long length, string sha)
        {
            AssetId = assetId; Set = set; Platform = platform; DescriptorSha = descriptorSha; Package = package;
            ManifestVersion = manifestVersion; Location = location; PhysicalName = physicalName; Length = length; Sha = sha;
        }
    }

    internal sealed class RawBuffer
    {
        private readonly object gate = new object();
        private byte[] bytes;
        internal long Length { get; }
        internal RawBuffer(byte[] bytes)
        {
            this.bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            Length = bytes.LongLength;
        }
        internal bool Alive { get { lock (gate) return bytes != null; } }
        internal void Check(Func<bool> released)
        {
            lock (gate)
                if (bytes == null || released()) throw new System.ObjectDisposedException("Raw lease");
        }
        internal int Read(long position, byte[] target, int offset, int count, Func<bool> released)
        {
            lock (gate)
            {
                if (bytes == null || released()) throw new System.ObjectDisposedException("Raw lease");
                if (target == null) throw new ArgumentNullException(nameof(target));
                if (offset < 0 || count < 0 || offset > target.Length - count)
                    throw new ArgumentOutOfRangeException(nameof(count));
                var amount = (int)Math.Min(count, Length - position);
                System.Buffer.BlockCopy(bytes, (int)position, target, offset, amount);
                return amount;
            }
        }
        internal void Release() { lock (gate) bytes = null; }
    }

    public sealed class FightMatchRawBytes
    {
        private readonly RawBuffer buffer;
        private readonly Func<bool> released;
        private readonly object streamGate = new object();
        private ReadStream active;
        public long Length { get; }
        internal FightMatchRawBytes(RawBuffer buffer, Func<bool> isReleased)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            released = isReleased ?? throw new ArgumentNullException(nameof(isReleased));
            Length = buffer.Length;
        }
        public System.IO.Stream OpenRead()
        {
            lock (streamGate)
            {
                buffer.Check(released);
                if (active != null) throw new InvalidOperationException("Only one raw stream may be open per lease.");
                return active = new ReadStream(this);
            }
        }
        private void Close(ReadStream stream)
        {
            lock (streamGate)
                if (ReferenceEquals(active, stream)) active = null;
        }

        private sealed class ReadStream : System.IO.Stream
        {
            private readonly FightMatchRawBytes owner;
            private readonly Func<bool> invalid;
            private int disposed;
            private long position;
            internal ReadStream(FightMatchRawBytes owner)
            {
                this.owner = owner;
                invalid = () => System.Threading.Volatile.Read(ref disposed) != 0 || owner.released();
            }
            public override bool CanRead => !invalid() && owner.buffer.Alive;
            public override bool CanSeek => CanRead;
            public override bool CanWrite => false;
            public override long Length { get { owner.buffer.Check(invalid); return owner.Length; } }
            public override long Position
            {
                get { owner.buffer.Check(invalid); return position; }
                set { Seek(value, System.IO.SeekOrigin.Begin); }
            }
            public override int Read(byte[] target, int offset, int count)
            {
                var read = owner.buffer.Read(position, target, offset, count, invalid);
                position += read;
                return read;
            }
            public override long Seek(long offset, System.IO.SeekOrigin origin)
            {
                owner.buffer.Check(invalid);
                long basis;
                if (origin == System.IO.SeekOrigin.Begin) basis = 0;
                else if (origin == System.IO.SeekOrigin.Current) basis = position;
                else if (origin == System.IO.SeekOrigin.End) basis = owner.Length;
                else throw new ArgumentOutOfRangeException(nameof(origin));
                if (offset < -basis || offset > owner.Length - basis)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                return position = basis + offset;
            }
            public override void Flush() { owner.buffer.Check(invalid); }
            public override void SetLength(long value)
            {
                owner.buffer.Check(invalid);
                throw new NotSupportedException("Raw streams are read-only.");
            }
            public override void Write(byte[] target, int offset, int count)
            {
                owner.buffer.Check(invalid);
                throw new NotSupportedException("Raw streams are read-only.");
            }
            protected override void Dispose(bool disposing)
            {
                if (System.Threading.Interlocked.Exchange(ref disposed, 1) == 0) owner.Close(this);
                base.Dispose(disposing);
            }
        }
    }
}
