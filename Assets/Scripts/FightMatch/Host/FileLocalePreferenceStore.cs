using System;
using System.IO;
using System.Text;
using FightMatch.Presentation;
using UnityEngine;

namespace FightMatch.Host
{
    internal interface ILocalePreferenceFiles
    {
        void EnsureDirectory(string path);
        Stream CreateNew(string path);
        void Write(Stream stream, byte[] bytes);
        void Flush(Stream stream);
        Stream OpenRead(string path);
        byte[] Read(Stream stream);
        bool Exists(string path);
        void Replace(string source, string target, string backup);
        void Move(string source, string target);
        void Delete(string path);
    }

    internal sealed class LocalePreferenceFiles : ILocalePreferenceFiles
    {
        public void EnsureDirectory(string path) => Directory.CreateDirectory(path);
        public Stream CreateNew(string path) => new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        public void Write(Stream stream, byte[] bytes) => stream.Write(bytes, 0, bytes.Length);
        public void Flush(Stream stream) => ((FileStream)stream).Flush(true);
        public Stream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        public byte[] Read(Stream stream)
        {
            using (var buffer = new MemoryStream()) { stream.CopyTo(buffer); return buffer.ToArray(); }
        }
        public bool Exists(string path)
        {
            try { File.GetAttributes(path); return true; }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
        }
        public void Replace(string source, string target, string backup) => File.Replace(source, target, backup);
        public void Move(string source, string target) => File.Move(source, target);
        public void Delete(string path) => File.Delete(path);
    }

    internal sealed class FileLocalePreferenceStore : ILocalePreferenceStore
    {
        private readonly string target;
        private readonly ILocalePreferenceFiles files;

        internal FileLocalePreferenceStore() : this(UnityEngine.Application.persistentDataPath, new LocalePreferenceFiles()) { }

        internal FileLocalePreferenceStore(string root, ILocalePreferenceFiles files)
        {
            if (string.IsNullOrEmpty(root)) throw new ArgumentException("A preference root is required.", nameof(root));
            this.files = files ?? throw new ArgumentNullException(nameof(files));
            target = Path.Combine(root, "FightMatch", "settings", "locale-preference-v1.json");
        }

        public LocalePreferenceLoadResult Load(SystemLanguage systemLanguage)
        {
            var fallback = LocalePolicy.FromSystemLanguage(systemLanguage);
            byte[] bytes;
            try
            {
                if (!files.Exists(target))
                    return new LocalePreferenceLoadResult(fallback, LocalePreferenceLoadDisposition.MissingSystemDefault, null);
                using (var stream = files.OpenRead(target)) bytes = files.Read(stream);
            }
            catch (Exception)
            {
                return new LocalePreferenceLoadResult(fallback, LocalePreferenceLoadDisposition.ReadFailedSystemDefault,
                    "PreferenceReadFailed");
            }
            if (TryParse(bytes, out var locale, out var code))
                return new LocalePreferenceLoadResult(locale, LocalePreferenceLoadDisposition.Remembered, null);
            return new LocalePreferenceLoadResult(fallback, LocalePreferenceLoadDisposition.InvalidSystemDefault, code);
        }

        public LocalePreferenceSaveResult Save(LocaleId desiredLocale)
        {
            if (!LocalePolicy.IsKnown(desiredLocale)) throw new ArgumentOutOfRangeException(nameof(desiredLocale));
            string suffix = Guid.NewGuid().ToString("N");
            string temp = target + "." + suffix + ".tmp", backup = target + "." + suffix + ".bak";
            bool committed = false, oldExists = false;
            byte[] oldBytes = null;
            string code = "PreferenceTempCreateFailed";
            try
            {
                files.EnsureDirectory(Path.GetDirectoryName(target));
                using (var stream = files.CreateNew(temp))
                {
                    code = "PreferenceTempWriteFailed";
                    var bytes = Encoding.UTF8.GetBytes("{\"version\":1,\"locale\":\"" +
                        (desiredLocale == LocaleId.En ? "en" : "zh-Hans") + "\"}");
                    files.Write(stream, bytes);
                    code = "PreferenceFlushFailed"; files.Flush(stream);
                }
                code = "PreferenceTempReadFailed";
                byte[] prepared;
                using (var stream = files.OpenRead(temp)) prepared = files.Read(stream);
                code = "PreferenceTempVerificationFailed"; Verify(prepared, desiredLocale);
                code = "PreferenceCommitFailed";
                oldExists = files.Exists(target);
                if (oldExists)
                    using (var stream = files.OpenRead(target)) oldBytes = files.Read(stream);
                if (oldExists) files.Replace(temp, target, backup);
                else files.Move(temp, target);
                committed = true;
                code = "PreferenceTargetReopenFailed";
                byte[] actual;
                using (var stream = files.OpenRead(target))
                { code = "PreferenceTargetReadFailed"; actual = files.Read(stream); }
                code = "PreferenceTargetVerificationFailed"; Verify(actual, desiredLocale);
                return LocalePreferenceSaveResult.Saved();
            }
            catch (Exception)
            {
                // No cleanup/read-back can turn an uncommitted failure into Unknown.
                if (!committed) return LocalePreferenceSaveResult.Failed(code);
                string primary = code;
                try
                {
                    code = "PreferenceRollbackFailed";
                    if (oldExists) files.Replace(backup, target, null);
                    else files.Delete(target);
                    code = "PreferenceRollbackProofFailed";
                    if (oldExists)
                    {
                        byte[] restored;
                        using (var stream = files.OpenRead(target)) restored = files.Read(stream);
                        code = "PreferenceRollbackMismatch";
                        if (!Equal(restored, oldBytes)) return LocalePreferenceSaveResult.Failed(code, true);
                    }
                    else
                    {
                        bool stillExists = files.Exists(target);
                        code = "PreferenceRollbackMismatch";
                        if (stillExists) return LocalePreferenceSaveResult.Failed(code, true);
                    }
                    return LocalePreferenceSaveResult.Failed(primary);
                }
                catch (Exception) { return LocalePreferenceSaveResult.Failed(code, true); }
            }
            finally
            {
                try { files.Delete(temp); } catch (Exception) { }
                try { files.Delete(backup); } catch (Exception) { }
            }
        }

        private static bool Equal(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
            return true;
        }

        private static void Verify(byte[] bytes, LocaleId desired)
        {
            if (!TryParse(bytes, out var locale, out _) || locale != desired)
                throw new InvalidDataException("Preference verification failed.");
        }

        private static bool TryParse(byte[] bytes, out LocaleId locale, out string code)
        {
            locale = LocaleId.En; code = null;
            try { locale = new PreferenceReader(new UTF8Encoding(false, true).GetString(bytes)).Read(); return true; }
            catch (DecoderFallbackException) { code = "PreferenceInvalidDocument"; return false; }
            catch (PreferenceDocumentError error) { code = error.Code; return false; }
        }

        private sealed class PreferenceDocumentError : Exception
        {
            internal readonly string Code;
            internal PreferenceDocumentError(string code) { Code = code; }
        }

        // Fixed two-field grammar: no permissive DTO parser, normalization, or repair.
        private sealed class PreferenceReader
        {
            private readonly string text;
            private int at;
            internal PreferenceReader(string text) { this.text = text; }
            private char Current => at < text.Length ? text[at] : '\0';
            private static void Invalid(string code = "PreferenceInvalidDocument") { throw new PreferenceDocumentError(code); }
            private void Space()
            {
                while (Current == ' ' || Current == '\t' || Current == '\r' || Current == '\n') at++;
            }
            private void Expect(char value)
            {
                if (at >= text.Length || text[at] != value) Invalid();
                at++;
            }
            internal LocaleId Read()
            {
                Space(); Expect('{'); Space();
                string version = null, locale = null;
                bool versionSeen = false, localeSeen = false;
                if (Current != '}')
                {
                    while (true)
                    {
                        string key = Quoted(); Space(); Expect(':'); Space();
                        if (key == "version")
                        {
                            if (versionSeen) Invalid("PreferenceInvalidSchema");
                            versionSeen = true;
                            if (Current != '-' && (Current < '0' || Current > '9')) Invalid("PreferenceInvalidSchema");
                            version = Number();
                            if (version.IndexOfAny(new[] { '.', 'e', 'E' }) >= 0) Invalid("PreferenceInvalidSchema");
                        }
                        else if (key == "locale")
                        {
                            if (localeSeen || Current != '"') Invalid("PreferenceInvalidSchema");
                            localeSeen = true; locale = Quoted();
                        }
                        else Invalid("PreferenceInvalidSchema");
                        Space();
                        if (Current == '}') break;
                        Expect(','); Space();
                    }
                }
                Expect('}');
                // Whitespace after the root is also forbidden by the preference v1 contract.
                if (at != text.Length) Invalid();
                if (!versionSeen || !localeSeen) Invalid("PreferenceInvalidSchema");
                if (version != "1") Invalid("PreferenceUnsupportedVersion");
                if (locale == "en") return LocaleId.En;
                if (locale == "zh-Hans") return LocaleId.ZhHans;
                Invalid("PreferenceUnsupportedLocale"); return LocaleId.En;
            }
            private string Number()
            {
                int start = at;
                if (Current == '-') at++;
                if (Current == '0')
                { at++; if (Current >= '0' && Current <= '9') Invalid(); }
                else Digits();
                if (Current == '.') { at++; Digits(); }
                if (Current == 'e' || Current == 'E')
                { at++; if (Current == '+' || Current == '-') at++; Digits(); }
                return text.Substring(start, at - start);
            }
            private void Digits()
            {
                int start = at;
                while (Current >= '0' && Current <= '9') at++;
                if (start == at) Invalid();
            }
            private string Quoted()
            {
                Expect('"'); var value = new StringBuilder();
                while (at < text.Length)
                {
                    char c = text[at++];
                    if (c == '"') return value.ToString();
                    if (c < 0x20) Invalid();
                    if (c != '\\') { value.Append(c); continue; }
                    if (at == text.Length) Invalid();
                    c = text[at++];
                    switch (c)
                    {
                        case '"': case '\\': case '/': value.Append(c); break;
                        case 'b': value.Append('\b'); break;
                        case 'f': value.Append('\f'); break;
                        case 'n': value.Append('\n'); break;
                        case 'r': value.Append('\r'); break;
                        case 't': value.Append('\t'); break;
                        case 'u':
                            int code = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                if (at == text.Length) Invalid();
                                char digit = text[at++];
                                int v = digit >= '0' && digit <= '9' ? digit - '0' :
                                    digit >= 'a' && digit <= 'f' ? digit - 'a' + 10 :
                                    digit >= 'A' && digit <= 'F' ? digit - 'A' + 10 : -1;
                                if (v < 0) Invalid(); code = code * 16 + v;
                            }
                            value.Append((char)code); break;
                        default: Invalid(); break;
                    }
                }
                Invalid(); return null;
            }
        }
    }
}
