using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FightMatch.Host;
using FightMatch.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace FightMatch.Host.Tests
{
    public sealed class LocalePreferenceStoreTests
    {
        private string root;
        private string Target => Path.Combine(root, "FightMatch", "settings", "locale-preference-v1.json");
        private static byte[] Bytes(string value) => Encoding.UTF8.GetBytes(value);
        private static byte[] Canonical(string locale) => Bytes("{\"version\":1,\"locale\":\"" + locale + "\"}");

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "FightMatch-LocalePreference-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
        }

        [TearDown]
        public void TearDown() { if (Directory.Exists(root)) Directory.Delete(root, true); }

        private ScriptedFiles Adapter(byte[] old = null)
        {
            var files = new ScriptedFiles(Target);
            if (old != null) files.Data[Target] = (byte[])old.Clone();
            return files;
        }
        private ILocalePreferenceStore Store(ScriptedFiles files) => new FileLocalePreferenceStore(root, files);
        private static void Result(LocalePreferenceSaveResult result, LocalePreferenceSaveDisposition disposition, string code)
        {
            Assert.AreEqual(disposition, result.Disposition);
            Assert.AreEqual(disposition == LocalePreferenceSaveDisposition.Saved ? "fm.language.saved" :
                disposition == LocalePreferenceSaveDisposition.SaveFailed ? "fm.language.save_failed" :
                "fm.language.save_unknown", result.LocalizationKey);
            Assert.AreEqual(code, result.ErrorCode);
        }

        [TestCase(SystemLanguage.Chinese), TestCase(SystemLanguage.ChineseSimplified), TestCase(SystemLanguage.ChineseTraditional)]
        [Category("PREF-01")]
        public void MissingChineseUsesSystemDefaultWithoutWrites(SystemLanguage language)
        {
            var files = Adapter(); var result = Store(files).Load(language);
            Assert.AreEqual(LocaleId.ZhHans, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.MissingSystemDefault, result.Disposition);
            Assert.IsNull(result.DiagnosticCode); CollectionAssert.AreEqual(new[] { "load-exists" }, files.Calls);
        }

        [TestCase(SystemLanguage.English), TestCase(SystemLanguage.Japanese), TestCase(SystemLanguage.Unknown), TestCase(SystemLanguage.French)]
        [Category("PREF-02")]
        public void MissingOtherLanguageUsesEnglishWithoutWrites(SystemLanguage language)
        {
            var files = Adapter(); var result = Store(files).Load(language);
            Assert.AreEqual(LocaleId.En, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.MissingSystemDefault, result.Disposition);
            Assert.IsNull(result.DiagnosticCode); CollectionAssert.AreEqual(new[] { "load-exists" }, files.Calls);
        }

        [TestCase("en", SystemLanguage.Chinese), TestCase("zh-Hans", SystemLanguage.English)]
        [Category("PREF-03")]
        public void RememberedLocaleOverridesSystemLanguage(string locale, SystemLanguage language)
        {
            var bytes = Canonical(locale); var files = Adapter(bytes); var result = Store(files).Load(language);
            Assert.AreEqual(locale == "en" ? LocaleId.En : LocaleId.ZhHans, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.Remembered, result.Disposition);
            Assert.IsNull(result.DiagnosticCode); CollectionAssert.AreEqual(bytes, files.Data[Target]);
            CollectionAssert.AreEqual(new[] { "load-exists", "load-open", "load-read" }, files.Calls);
        }

        // Literal, ordered parameter sources: no I/O, session setup, or generated random values.
        private static IEnumerable<TestCaseData> InvalidDocuments()
        {
            yield return new TestCaseData("{", "PreferenceInvalidDocument");
            yield return new TestCaseData("[]", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":01,\"locale\":\"en\"}", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"e\\q\"}", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"en\"}{}", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"en\"} ", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"en\"}\n", "PreferenceInvalidDocument");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"en\",\"extra\":0}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1,\"locale\":\"en\",\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1,\"version\":1,\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"Version\":1,\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":null,\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1,\"locale\":null}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":\"1\",\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1.0,\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1e0,\"locale\":\"en\"}", "PreferenceInvalidSchema");
            yield return new TestCaseData("{\"version\":1,\"locale\":3}", "PreferenceInvalidSchema");
        }

        [TestCaseSource(nameof(InvalidDocuments)), Category("PREF-04"), Category("PREF-20")]
        public void InvalidDocumentPreservesBytesAndDoesNotRepair(string document, string code)
        {
            var bytes = Bytes(document); var files = Adapter(bytes); var result = Store(files).Load(SystemLanguage.Chinese);
            Assert.AreEqual(LocaleId.ZhHans, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.InvalidSystemDefault, result.Disposition);
            Assert.AreEqual(code, result.DiagnosticCode); CollectionAssert.AreEqual(bytes, files.Data[Target]);
            CollectionAssert.AreEqual(new[] { "load-exists", "load-open", "load-read" }, files.Calls);
        }

        [Test, Category("PREF-04"), Category("PREF-20")]
        public void InvalidUtf8IsPreserved()
        {
            byte[] bytes = { 0xff, 0xc0, 0xaf }; var files = Adapter(bytes);
            var result = Store(files).Load(SystemLanguage.English);
            Assert.AreEqual(LocaleId.En, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.InvalidSystemDefault, result.Disposition);
            Assert.AreEqual("PreferenceInvalidDocument", result.DiagnosticCode);
            CollectionAssert.AreEqual(bytes, files.Data[Target]);
            CollectionAssert.AreEqual(new[] { "load-exists", "load-open", "load-read" }, files.Calls);
        }

        [TestCase("{\"version\":2,\"locale\":\"en\"}", "PreferenceUnsupportedVersion")]
        [TestCase("{\"version\":-1,\"locale\":\"en\"}", "PreferenceUnsupportedVersion")]
        [TestCase("{\"version\":1,\"locale\":\"fr\"}", "PreferenceUnsupportedLocale")]
        [TestCase("{\"version\":1,\"locale\":\"EN\"}", "PreferenceUnsupportedLocale")]
        [TestCase("{\"version\":1,\"locale\":\"ZhHans\"}", "PreferenceUnsupportedLocale")]
        [Category("PREF-05"), Category("PREF-20")]
        public void UnsupportedSelectionUsesFallbackWithoutRepair(string document, string code)
        { InvalidDocumentPreservesBytesAndDoesNotRepair(document, code); }

        [TestCase("load-exists"), TestCase("load-open"), TestCase("load-read")]
        [Category("PREF-05"), Category("PREF-20")]
        public void ReadFailurePreservesFileAndUsesFallback(string stage)
        {
            var bytes = Canonical("zh-Hans"); var files = Adapter(bytes); files.Fail.Add(stage);
            var result = Store(files).Load(SystemLanguage.English);
            Assert.AreEqual(LocaleId.En, result.Locale);
            Assert.AreEqual(LocalePreferenceLoadDisposition.ReadFailedSystemDefault, result.Disposition);
            Assert.AreEqual("PreferenceReadFailed", result.DiagnosticCode);
            CollectionAssert.AreEqual(bytes, files.Data[Target]); Assert.IsFalse(files.Calls.Contains("directory"));
        }

        [Test, Category("PREF-03")]
        public void GrammarWhitespaceFieldOrderAndEscapesRemainValid()
        {
            var files = Adapter(Bytes(" \t{ \"locale\" : \"zh-\\u0048ans\", \"\\u0076ersion\" : 1 }"));
            Assert.AreEqual(LocaleId.ZhHans, Store(files).Load(SystemLanguage.English).Locale);
            Assert.AreEqual(3, files.Calls.Count);
        }

        [Test, Category("PREF-06")]
        public void SaveUsesTheCompleteOrderedStorageFlow()
        {
            var files = Adapter(Canonical("en"));
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Saved, null);
            CollectionAssert.AreEqual(Canonical("zh-Hans"), files.Data[Target]);
            CollectionAssert.AreEqual(new[] { "directory", "temp-create", "temp-write", "flush", "temp-open", "temp-read",
                "old-exists", "old-open", "old-read", "commit", "target-open", "target-read", "cleanup-temp", "cleanup-backup" }, files.Calls);
            Assert.AreEqual(1, files.Data.Count);
        }

        [TestCase("en"), TestCase("zh-Hans"), Category("PREF-06")]
        public void ProductionAdapterCreatesAndAtomicallyReplacesCanonicalBytes(string locale)
        {
            ILocalePreferenceStore store = new FileLocalePreferenceStore(root, new LocalePreferenceFiles());
            Result(store.Save(LocaleId.En), LocalePreferenceSaveDisposition.Saved, null);
            var desired = locale == "en" ? LocaleId.En : LocaleId.ZhHans;
            Result(store.Save(desired), LocalePreferenceSaveDisposition.Saved, null);
            CollectionAssert.AreEqual(Canonical(locale), File.ReadAllBytes(Target));
            Assert.AreEqual(desired, store.Load(SystemLanguage.Japanese).Locale);
            CollectionAssert.AreEqual(new[] { Target }, Directory.GetFiles(Path.GetDirectoryName(Target)));
        }

        private static IEnumerable<TestCaseData> PreCommitFaults()
        {
            yield return new TestCaseData("directory", "PreferenceTempCreateFailed");
            yield return new TestCaseData("temp-create", "PreferenceTempCreateFailed");
            yield return new TestCaseData("temp-write", "PreferenceTempWriteFailed");
            yield return new TestCaseData("flush", "PreferenceFlushFailed");
            yield return new TestCaseData("temp-open", "PreferenceTempReadFailed");
            yield return new TestCaseData("temp-read", "PreferenceTempReadFailed");
            yield return new TestCaseData("old-open", "PreferenceCommitFailed");
            yield return new TestCaseData("old-read", "PreferenceCommitFailed");
            yield return new TestCaseData("commit", "PreferenceCommitFailed");
        }

        [TestCaseSource(nameof(PreCommitFaults)), Category("PREF-07"), Category("PREF-08"), Category("PREF-09"), Category("PREF-10"), Category("PREF-20")]
        public void PreCommitFailureKeepsOldTargetAndDesiredSession(string stage, string code)
        {
            var old = Canonical("en"); var files = Adapter(old); files.Fail.Add(stage);
            var service = new LocalizationService(null, SystemLanguage.English); service.SetLocale(LocaleId.ZhHans);
            Result(Store(files).Save(service.CurrentLocale), LocalePreferenceSaveDisposition.SaveFailed, code);
            CollectionAssert.AreEqual(old, files.Data[Target]); Assert.IsFalse(files.Committed);
            Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale);
        }

        [TestCase("{"), TestCase("{\"version\":2,\"locale\":\"zh-Hans\"}"), TestCase("{\"version\":1,\"locale\":\"en\"}")]
        [Category("PREF-09"), Category("PREF-20")]
        public void TempVerificationFailureNeverCommits(string corrupt)
        {
            var old = Canonical("en"); var files = Adapter(old); files.ReadOverride["temp-read"] = Bytes(corrupt);
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.SaveFailed, "PreferenceTempVerificationFailed");
            Assert.IsFalse(files.Committed); CollectionAssert.AreEqual(old, files.Data[Target]);
        }

        [TestCase("temp-create", "PreferenceTempCreateFailed"), TestCase("temp-write", "PreferenceTempWriteFailed")]
        [TestCase("flush", "PreferenceFlushFailed"), TestCase("temp-open", "PreferenceTempReadFailed")]
        [TestCase("commit", "PreferenceCommitFailed"), Category("PREF-11")]
        public void SecondaryCleanupAndReadBackFaultsCannotUpgradePreCommitFailure(string stage, string code)
        {
            var old = Canonical("en"); var files = Adapter(old); files.Fail.UnionWith(new[] { stage, "cleanup-temp", "cleanup-backup" });
            files.FailSecondaryReadBack = true;
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.SaveFailed, code);
            Assert.IsFalse(files.Calls.Contains("secondary-read-back")); Assert.IsFalse(files.Committed);
            CollectionAssert.AreEqual(old, files.Data[Target]);
            CollectionAssert.Contains(files.Calls, "cleanup-temp"); CollectionAssert.Contains(files.Calls, "cleanup-backup");
        }

        [TestCase("target-open"), TestCase("target-read"), Category("PREF-12"), Category("PREF-13")]
        public void PostCommitIoFailureWithoutRollbackProofIsUnknown(string stage)
        {
            var files = Adapter(Canonical("en")); files.Fail.UnionWith(new[] { stage, "rollback" });
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Unknown, "PreferenceRollbackFailed");
            Assert.IsTrue(files.Committed); CollectionAssert.AreEqual(Canonical("zh-Hans"), files.Data[Target]);
            Assert.Less(files.Calls.IndexOf(stage), files.Calls.IndexOf("rollback"));
        }

        [TestCase("{"), TestCase("{\"version\":2,\"locale\":\"zh-Hans\"}"), TestCase("{\"version\":1,\"locale\":\"en\"}")]
        [Category("PREF-14")]
        public void PostCommitVerificationWithoutRollbackProofIsUnknown(string corrupt)
        {
            var files = Adapter(Canonical("en")); files.ReadOverride["target-read"] = Bytes(corrupt); files.Fail.Add("rollback");
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Unknown, "PreferenceRollbackFailed");
            Assert.IsTrue(files.Committed); CollectionAssert.AreEqual(Canonical("zh-Hans"), files.Data[Target]);
        }

        [TestCase("target-open", "PreferenceTargetReopenFailed"), TestCase("target-read", "PreferenceTargetReadFailed")]
        [TestCase("target-document", "PreferenceTargetVerificationFailed"), Category("PREF-15"), Category("PREF-20")]
        public void SuccessfulRollbackRequiresExactOldBytesAndRetainsOriginalDiagnostic(string stage, string code)
        {
            byte[] old = { 0xff, 0, 32, 17 }; var files = Adapter(old);
            if (stage == "target-document") files.ReadOverride["target-read"] = Bytes("{");
            else files.Fail.Add(stage);
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.SaveFailed, code);
            CollectionAssert.AreEqual(old, files.Data[Target]);
            Assert.Less(files.Calls.IndexOf("rollback"), files.Calls.IndexOf("proof-open"));
            Assert.Less(files.Calls.IndexOf("proof-open"), files.Calls.IndexOf("proof-read"));
        }

        [Test, Category("PREF-16")]
        public void OriginallyAbsentTargetRequiresAnAbsenceProof()
        {
            var files = Adapter(); files.Fail.Add("target-open");
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.SaveFailed, "PreferenceTargetReopenFailed");
            Assert.IsFalse(files.Data.ContainsKey(Target));
            Assert.Less(files.Calls.IndexOf("rollback"), files.Calls.IndexOf("proof-exists"));
        }

        [TestCase("rollback", "PreferenceRollbackFailed"), TestCase("proof-open", "PreferenceRollbackProofFailed")]
        [TestCase("proof-read", "PreferenceRollbackProofFailed"), TestCase("proof-bytes", "PreferenceRollbackMismatch")]
        [Category("PREF-17"), Category("PREF-20")]
        public void UnprovenOldBytesKeepUnknown(string stage, string code)
        {
            var files = Adapter(Canonical("en")); files.Fail.Add("target-open");
            if (stage == "proof-bytes") files.ReadOverride["proof-read"] = Canonical("zh-Hans"); else files.Fail.Add(stage);
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Unknown, code);
            Assert.IsTrue(files.Committed); CollectionAssert.Contains(files.Calls, "rollback");
        }

        [TestCase(false), TestCase(true), Category("PREF-17"), Category("PREF-20")]
        public void UnprovenOldAbsenceKeepsUnknown(bool ioFailure)
        {
            var files = Adapter(); files.Fail.Add("target-open");
            if (ioFailure) files.Fail.Add("proof-exists"); else files.ProofExists = true;
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Unknown,
                ioFailure ? "PreferenceRollbackProofFailed" : "PreferenceRollbackMismatch");
        }

        [TestCase(false), TestCase(true), Category("PREF-18"), Category("PREF-19")]
        public void FailureAndSameDesiredRetryUseRealLocaleServiceWithoutDuplicateEvents(bool unknown)
        {
            var files = Adapter(Canonical("en"));
            if (unknown) files.Fail.UnionWith(new[] { "target-read", "rollback" }); else files.Fail.Add("temp-write");
            var service = new LocalizationService(null, SystemLanguage.English); int localeEvents = 0, rebinds = 0;
            service.LocaleChanged += locale => { Assert.AreEqual(locale, service.CurrentLocale); localeEvents++; };
            service.LocaleChanged += locale => rebinds++;
            Assert.IsTrue(service.SetLocale(LocaleId.ZhHans));
            var first = Store(files).Save(service.CurrentLocale);
            Assert.AreEqual(unknown ? LocalePreferenceSaveDisposition.Unknown : LocalePreferenceSaveDisposition.SaveFailed, first.Disposition);
            Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale); Assert.AreEqual(1, localeEvents); Assert.AreEqual(1, rebinds);
            files.NextAttempt(); Assert.IsFalse(service.SetLocale(LocaleId.ZhHans));
            Result(Store(files).Save(service.CurrentLocale), LocalePreferenceSaveDisposition.Saved, null);
            CollectionAssert.AreEqual(new[] { "directory", "temp-create", "temp-write", "flush", "temp-open", "temp-read",
                "old-exists", "old-open", "old-read", "commit", "target-open", "target-read", "cleanup-temp", "cleanup-backup" }, files.Calls);
            Assert.AreEqual(LocaleId.ZhHans, service.CurrentLocale); Assert.AreEqual(1, localeEvents); Assert.AreEqual(1, rebinds);
        }

        [Test, Category("PREF-20")]
        public void InvalidLocaleThrowsBeforeStorageAndCleanupDoesNotChangeSaved()
        {
            var files = Adapter(Canonical("en"));
            Assert.Throws<ArgumentOutOfRangeException>(() => Store(files).Save((LocaleId)42));
            Assert.IsEmpty(files.Calls);
            files.Fail.UnionWith(new[] { "cleanup-temp", "cleanup-backup" });
            Result(Store(files).Save(LocaleId.ZhHans), LocalePreferenceSaveDisposition.Saved, null);
        }

        [Test, Category("PREF-21")]
        public void NewModuleHasNoBusinessOrCompositionDependencies()
        {
            string directory = Path.Combine(UnityEngine.Application.dataPath, "Scripts", "FightMatch", "Host");
            foreach (var name in new[] { "ILocalePreferenceStore.cs", "FileLocalePreferenceStore.cs" })
            {
                string source = File.ReadAllText(Path.Combine(directory, name));
                foreach (var forbidden in new[] { "PlayerSave", "SaveEnvelope", "OperationId", "Cloud", "FightMatchPlayerHost",
                    "LanguagePopup", "LocalizedTextSource", "YooAsset", "LOC-A", "public class", "public interface", "event " })
                    Assert.That(source, Does.Not.Contain(forbidden), name + ": " + forbidden);
            }
        }

        private sealed class ScriptedFiles : ILocalePreferenceFiles
        {
            internal readonly Dictionary<string, byte[]> Data = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal readonly Dictionary<string, byte[]> ReadOverride = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            internal readonly List<string> Calls = new List<string>();
            internal readonly HashSet<string> Fail = new HashSet<string>(StringComparer.Ordinal);
            private readonly Dictionary<Stream, string> readStages = new Dictionary<Stream, string>();
            private readonly string target;
            private string temp;
            private bool saving, rolledBack, failureSeen;
            internal bool Committed { get; private set; }
            internal bool FailSecondaryReadBack, ProofExists;
            internal ScriptedFiles(string target) { this.target = target; }
            internal void NextAttempt()
            {
                Fail.Clear(); ReadOverride.Clear(); Calls.Clear(); readStages.Clear();
                saving = rolledBack = failureSeen = Committed = FailSecondaryReadBack = ProofExists = false;
            }
            private void Step(string stage)
            {
                Calls.Add(stage);
                if (Fail.Contains(stage)) { failureSeen = true; throw new IOException("private /player/path payload ZhHans " + stage); }
            }
            public void EnsureDirectory(string path) { saving = true; Step("directory"); }
            public Stream CreateNew(string path)
            {
                Step("temp-create"); Assert.IsFalse(Data.ContainsKey(path)); temp = path;
                Data[path] = Array.Empty<byte>(); return new MemoryStream();
            }
            public void Write(Stream stream, byte[] bytes)
            { Step("temp-write"); stream.Write(bytes, 0, bytes.Length); Data[temp] = (byte[])bytes.Clone(); }
            public void Flush(Stream stream) { Step("flush"); }
            public Stream OpenRead(string path)
            {
                if (failureSeen && FailSecondaryReadBack && path == target)
                { Step("secondary-read-back"); throw new IOException("secondary private payload"); }
                string stage = path.EndsWith(".tmp", StringComparison.Ordinal) ? "temp" :
                    !saving ? "load" : rolledBack ? "proof" : Committed ? "target" : "old";
                Step(stage + "-open");
                if (!Data.ContainsKey(path)) throw new FileNotFoundException("private missing path");
                var stream = new MemoryStream(Data[path], false); readStages[stream] = stage + "-read"; return stream;
            }
            public byte[] Read(Stream stream)
            {
                string stage = readStages[stream]; Step(stage);
                return ReadOverride.TryGetValue(stage, out var bytes) ? (byte[])bytes.Clone() : ((MemoryStream)stream).ToArray();
            }
            public bool Exists(string path)
            {
                string stage = !saving ? "load-exists" : rolledBack ? "proof-exists" : "old-exists"; Step(stage);
                return rolledBack && ProofExists || Data.ContainsKey(path);
            }
            public void Replace(string source, string destination, string backup)
            {
                bool rollback = source.EndsWith(".bak", StringComparison.Ordinal); Step(rollback ? "rollback" : "commit");
                Assert.IsTrue(Data.ContainsKey(source)); Assert.IsTrue(Data.ContainsKey(destination));
                if (backup != null) Data[backup] = (byte[])Data[destination].Clone();
                Data[destination] = (byte[])Data[source].Clone(); Data.Remove(source);
                if (rollback) rolledBack = true; else Committed = true;
            }
            public void Move(string source, string destination)
            {
                Step("commit"); Assert.IsFalse(Data.ContainsKey(destination));
                Data[destination] = (byte[])Data[source].Clone(); Data.Remove(source); Committed = true;
            }
            public void Delete(string path)
            {
                if (path == target) { Step("rollback"); Data.Remove(path); rolledBack = true; }
                else { Step(path.EndsWith(".tmp", StringComparison.Ordinal) ? "cleanup-temp" : "cleanup-backup"); Data.Remove(path); }
            }
        }
    }

    // Editor-only assembly; this opt-in shim discovers names and never executes tests.
    [InitializeOnLoad]
    internal static class LocalePreferenceDiscovery
    {
        private const string Argument = "-fightMatchLocalePreferenceDiscover";
        private const string RelativeRoot = "TestArtifacts/FightMatch/LOC-IMPL-B-PREF/source-activation-001/discovery";
        private static TestRunnerApi api;

        static LocalePreferenceDiscovery()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Contains(Argument)) return;
            EditorApplication.delayCall += () => Start(args);
        }

        private static void Start(string[] args)
        {
            try
            {
                int at = Array.IndexOf(args, Argument);
                string project = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
                string directory = Path.Combine(project, RelativeRoot);
                if (args.Count(x => x == Argument) != 1 || at + 1 >= args.Length ||
                    !string.Equals(args[at + 1], directory, StringComparison.Ordinal) ||
                    args.Contains("-runTests") || args.Contains("-quit") || !Directory.Exists(directory))
                    throw new InvalidDataException("Invalid preference discovery invocation.");
                for (var d = new DirectoryInfo(directory); d != null; d = d.Parent)
                    if ((d.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Discovery link.");
                if (File.Exists(Path.Combine(directory, "leaves.json")) || File.Exists(Path.Combine(directory, "receipt.json")))
                    throw new InvalidDataException("Discovery output already exists.");
                api = ScriptableObject.CreateInstance<TestRunnerApi>();
                api.RetrieveTestList(TestMode.EditMode, tree => Finish(tree, directory));
            }
            catch (Exception error) { Debug.LogError("PreferenceDiscoveryFailed: " + error.GetType().Name); EditorApplication.Exit(3); }
        }

        [Serializable] private sealed class Leaf
        {
            public string name, fullName, runState;
            public string[] categories;
        }
        [Serializable] private sealed class Leaves { public Leaf[] leaves; }
        [Serializable] private sealed class DiscoveryReceipt
        {
            public string status = "DISCOVERED_NOT_EXECUTED";
            public int testsExecuted = 0;
            public int leafCount;
        }
        private static void Finish(ITestAdaptor tree, string directory)
        {
            try
            {
                var leaves = new List<Leaf>(); Collect(tree, leaves);
                leaves.Sort((a, b) => string.CompareOrdinal(a.fullName, b.fullName));
                if (leaves.Count == 0 || leaves.Any(x => x.runState != "Runnable") ||
                    leaves.Select(x => x.fullName).Distinct(StringComparer.Ordinal).Count() != leaves.Count)
                    throw new InvalidDataException("Incomplete preference discovery.");
                using (var stream = new FileStream(Path.Combine(directory, "leaves.json"), FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    writer.Write(JsonUtility.ToJson(new Leaves { leaves = leaves.ToArray() }, true));
                using (var stream = new FileStream(Path.Combine(directory, "receipt.json"), FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                    writer.Write(JsonUtility.ToJson(new DiscoveryReceipt { leafCount = leaves.Count }, true));
                UnityEngine.Object.DestroyImmediate(api); api = null; EditorApplication.Exit(0);
            }
            catch (Exception error) { Debug.LogError("PreferenceDiscoveryFailed: " + error.GetType().Name); EditorApplication.Exit(3); }
        }
        private static void Collect(ITestAdaptor node, List<Leaf> leaves)
        {
            if (!node.IsSuite && node.FullName.StartsWith("FightMatch.Host.Tests.LocalePreferenceStoreTests.", StringComparison.Ordinal))
                leaves.Add(new Leaf { name = node.Name, fullName = node.FullName, runState = node.RunState.ToString(),
                    categories = node.Categories.OrderBy(x => x, StringComparer.Ordinal).ToArray() });
            if (node.HasChildren)
            {
                if (node.Children == null) throw new InvalidDataException("Missing discovery children.");
                foreach (var child in node.Children) Collect(child, leaves);
            }
        }
    }
}
