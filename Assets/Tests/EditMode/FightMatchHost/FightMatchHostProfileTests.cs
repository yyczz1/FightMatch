using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using NUnit.Framework;

namespace FightMatch.Host.Tests
{
    public sealed class FightMatchHostProfileTests
    {
        [Test]
        public void H03_ExplicitCreateKeepsOneOriginalAcrossDuplicateReentryAndRebind()
        {
            using (var rig = new HostRig(false))
            {
                rig.Profile.Writing = () => rig.Session.CreateProfile();
                rig.Session.Changed += rig.Session.CreateProfile;
                rig.Create();
                rig.Session.Changed -= rig.Session.CreateProfile;
                var original = rig.Session.OriginalProfile;
                var head = rig.Head;
                var files = rig.Files();
                using (var first = new HostPanel(rig)) { }
                using (var second = new HostPanel(rig)) rig.Session.CreateProfile();
                Assert.AreSame(original, rig.Session.OriginalProfile);
                Assert.AreEqual(1, rig.FactoryCalls);
                Assert.AreEqual(head.Header.CommitId, rig.Head.Header.CommitId);
                Assert.AreEqual(SavePurpose.PlayerSave, rig.Storage.Profile.Purpose);
                rig.SameFiles(files);
            }
        }

        [TestCase("unknown-file")]
        [TestCase("orphan-profile")]
        [TestCase("unknown-lock")]
        public void H03_AbsentLocatorDoesNotAuthorizeCreationOverExistingPhysicalData(string kind)
        {
            using (var rig = new HostRig(false))
            {
                if (kind == "orphan-profile") Directory.CreateDirectory(Path.Combine(rig.Root, "profiles/p-orphan"));
                else if (kind == "unknown-lock")
                {
                    Directory.CreateDirectory(Path.Combine(rig.Root, "locator"));
                    File.WriteAllText(Path.Combine(rig.Root, "locator/unknown.lock"), "preserve");
                }
                else File.WriteAllText(Path.Combine(rig.Root, "unknown"), "preserve");
                rig.Session.ObserveStartup();
                Assert.IsFalse(rig.Session.CanCreate);
                var before = rig.Files();
                rig.Session.CreateProfile();
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(0, rig.FactoryCalls);
                rig.SameFiles(before);
            }
        }

        [DllImport("libSystem.B.dylib", SetLastError = true)]
        private static extern int symlink(string target, string path);
        [TestCase(false)]
        [TestCase(true)]
        public void H03_ProductSubtreeLinksIncludingDanglingAreBlocked(bool dangling)
        {
            using (var rig = new HostRig(false))
            {
                var target = Path.Combine(Path.GetDirectoryName(rig.Root), dangling ? "missing" : "real");
                if (!dangling) Directory.CreateDirectory(target);
                Assert.AreEqual(0, symlink(target, Path.Combine(rig.Root, "profiles")));
                rig.Session.ObserveStartup();
                Assert.IsFalse(rig.Session.CanCreate);
                rig.Session.CreateProfile();
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(0, rig.FactoryCalls);
            }
        }

        [Test]
        public void H03_FreshCreateRechecksTheRootAfterTheEmptyScreenWasShown()
        {
            using (var rig = new HostRig(false))
            {
                rig.Session.ObserveStartup();
                Assert.IsTrue(rig.Session.CanCreate);
                Directory.CreateDirectory(Path.Combine(rig.Root, "profiles/p-late-orphan"));
                rig.Session.CreateProfile();
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.IsTrue(Directory.Exists(Path.Combine(rig.Root, "profiles/p-late-orphan")));
            }
        }

        [Test]
        public void H03_ReadFailureNeverFallsBackToAbsentOrANewPlayer()
        {
            using (var rig = new HostRig(false))
            {
                rig.Profile.ReadFailure = true;
                rig.Session.ObserveStartup();
                rig.Session.CreateProfile();
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(0, rig.FactoryCalls);
                Assert.AreEqual(FightMatchHostPage.Startup, rig.Session.Page);
                rig.Profile.ReadFailure = false;
                rig.Create();
            }
        }

        [Test]
        public void H03_PreparedIdentityIsRetainedBeforeTheStorageFactoryCanFail()
        {
            using (var rig = new HostRig(false))
            {
                rig.Session.ObserveStartup();
                rig.BeforeStorage = () => { throw new IOException("029 isolated factory failure"); };
                rig.Session.CreateProfile();
                var original = rig.Session.OriginalProfile;
                Assert.IsNotNull(original);
                Assert.IsNull(rig.Storage);
                rig.BeforeStorage = null;
                rig.Session.CreateProfile();
                Assert.AreSame(original, rig.Session.OriginalProfile);
                Assert.AreEqual(original.PlayerId, rig.Session.Observation.ActiveProfile.PlayerId);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void H03_F2ColdReconstructionContinuesOriginalCreationAtBothDurableBoundaries(bool initialized)
        {
            using (var rig = new HostRig(false))
            {
                rig.Profile.FailCreateAfterWrite = !initialized;
                rig.Profile.FailActive = initialized;
                rig.Session.ObserveStartup();
                rig.Session.CreateProfile();
                var record = rig.Session.OriginalProfile.CreateRecord;
                Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, rig.Session.Observation.State);
                var commit = rig.Head?.Header.CommitId;
                Assert.AreEqual(initialized, commit != null);
                rig.Profile.FailActive = false;
                rig.Rebuild();
                Assert.AreEqual(FightMatchHostPage.Startup, rig.Session.Page);
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.IsNull(rig.Session.OriginalProfile);
                rig.Session.ContinueCreation();
                Assert.AreEqual(record.RecordSha256, rig.Session.OriginalProfile.CreateRecord.RecordSha256);
                Assert.AreEqual(record.PlayerId, rig.Session.Observation.ActiveProfile.PlayerId);
                Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State);
                if (initialized) Assert.AreEqual(commit, rig.Head.Header.CommitId);
            }
        }

        [Test]
        public void H03_ActiveReopenAndRepeatedObserveDoNotInitializeAgain()
        {
            using (var rig = new HostRig())
            {
                var player = rig.Session.Observation.ActiveProfile.PlayerId;
                var commit = rig.Head.Header.CommitId;
                var files = rig.Files();
                rig.Rebuild();
                rig.Session.ObserveStartup();
                rig.Session.ObserveStartup();
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(player, rig.Session.Observation.ActiveProfile.PlayerId);
                Assert.AreEqual(commit, rig.Head.Header.CommitId);
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                rig.SameFiles(files);
            }
        }

        [Test]
        public void H03_LegacyTwoThenThreeRequireSeparateExplicitMigrationConfirmations()
        {
            using (var rig = new HostRig(false))
            {
                var prepared = rig.Session.Player.PrepareNewProfile(HostRig.Catalog, FightMatchHostSession.ReleaseSetId, rig.Session.Budget.Codec);
                Assert.IsTrue(prepared.IsAccepted, prepared.Code);
                var locator = new LocalPlayerProfileLocator(rig.Profile);
                Assert.IsTrue(locator.RecordCreateIntent(prepared.Request.CreateRecord, rig.Session.Budget).IsAccepted);
                rig.Session.ObserveStartup();
                rig.Session.ContinueCreation();
                for (uint format = 2; format <= 3; format++)
                {
                    Assert.AreEqual(format, (uint)rig.Head.Business.Format);
                    var files = rig.Files();
                    rig.Session.Navigation.Refresh();
                    rig.Session.Navigation.PreviewHandler(() => new PlayerNavigationDraft {
                        Kind = PlayerNavigationDraftKind.Migration, FromFormat = format, ToFormat = format + 1 })();
                    Assert.AreEqual(PlayerNavigationRoute.Confirmation, rig.Session.Navigation.View.Route);
                    Assert.AreEqual(format, (uint)rig.Head.Business.Format);
                    rig.SameFiles(files);
                    rig.Session.Navigation.ActionHandler(PlayerNavigationAction.Confirm)();
                    Assert.IsTrue(rig.Session.Navigation.View.Result.IsCommitted, rig.Session.Navigation.View.Status);
                    rig.Session.Navigation.ActionHandler(PlayerNavigationAction.Return)();
                }
                Assert.AreEqual(4, (int)rig.Head.Business.Format);
                rig.Rebuild();
                Assert.AreEqual(4, (int)rig.Head.Business.Format);
            }
        }

        private const string PreferenceName = "locale-preference-v1.json";
        private const string PreferenceResidue = PreferenceName + ".0123456789abcdef0123456789abcdef";

        private static FileLocalePreferenceStore Preferences(HostRig rig) =>
            new FileLocalePreferenceStore(Path.GetDirectoryName(rig.Root), new LocalePreferenceFiles());

        private static string Settings(HostRig rig) => Path.Combine(rig.Root, "settings");
        private static System.Collections.Generic.Dictionary<string, byte[]> PreferenceFiles(HostRig rig) =>
            Directory.GetFiles(Settings(rig)).ToDictionary(Path.GetFileName, File.ReadAllBytes);

        private static void SamePreferences(HostRig rig, System.Collections.Generic.Dictionary<string, byte[]> expected)
        {
            var actual = PreferenceFiles(rig);
            CollectionAssert.AreEquivalent(expected.Keys, actual.Keys);
            foreach (var row in expected) CollectionAssert.AreEqual(row.Value, actual[row.Key], row.Key);
        }

        private static void AddPreferenceResidues(HostRig rig)
        {
            foreach (var suffix in new[] { ".tmp", ".bak" })
                File.WriteAllText(Path.Combine(Settings(rig), PreferenceResidue + suffix), "{\"version\":1,\"locale\":\"zh-Hans\"}");
        }

        private static void AssertPreference(HostRig rig, LocalePreferenceLoadDisposition disposition, LocaleId locale)
        {
            var loaded = Preferences(rig).Load(UnityEngine.SystemLanguage.English);
            Assert.AreEqual(disposition, loaded.Disposition);
            Assert.AreEqual(locale, loaded.Locale);
            Assert.AreEqual(disposition == LocalePreferenceLoadDisposition.InvalidSystemDefault ?
                "PreferenceInvalidDocument" : null, loaded.DiagnosticCode);
        }

        [DllImport("libSystem.B.dylib", EntryPoint = "readlink", SetLastError = true)]
        private static extern IntPtr LocatorReadLink(string path, byte[] buffer, UIntPtr size);

        private static void SameFilesAfterLocatorRead(HostRig rig,
            System.Collections.Generic.Dictionary<string, byte[]> expected, string[] directories)
        {
            var locator = Path.Combine(rig.Root, "locator");
            var writerLock = Path.Combine(locator, "writer.lock");
            foreach (var path in new[] { locator, writerLock })
            {
                var link = LocatorReadLink(path, new byte[1], new UIntPtr(1)).ToInt64();
                var error = Marshal.GetLastWin32Error();
                Assert.AreEqual(-1L, link, path);
                Assert.AreEqual(22, error, path);
                var expectedType = path == locator ? FileAttributes.Directory : (FileAttributes)0;
                Assert.AreEqual(expectedType, File.GetAttributes(path) &
                    (FileAttributes.Directory | FileAttributes.ReparsePoint), path);
            }
            Assert.AreEqual(0L, new FileInfo(writerLock).Length);
            // A read keeps its lease file; preserve any pre-existing expectation instead of replacing it.
            var files = new System.Collections.Generic.Dictionary<string, byte[]>(expected, StringComparer.Ordinal);
            var lockKey = writerLock.Substring(rig.Root.Length);
            if (!files.ContainsKey(lockKey)) files.Add(lockKey, Array.Empty<byte>());
            rig.SameFiles(files);
            var expectedDirectories = directories.Contains(locator, StringComparer.Ordinal) ? directories :
                directories.Concat(new[] { locator }).ToArray();
            CollectionAssert.AreEquivalent(expectedDirectories, Directory.GetDirectories(rig.Root, "*", SearchOption.AllDirectories));
        }

        [Test]
        public void H03_SettingsOnlyAllowsExplicitFirstCreate()
        {
            foreach (var locale in new[] { LocaleId.En, LocaleId.ZhHans })
            using (var rig = new HostRig(false))
            {
                Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, Preferences(rig).Save(locale).Disposition);
                var before = rig.Files();
                var directories = Directory.GetDirectories(rig.Root, "*", SearchOption.AllDirectories);
                CollectionAssert.AreEquivalent(new[] { Settings(rig) }, directories);
                CollectionAssert.AreEquivalent(new[] { Path.Combine(Settings(rig), PreferenceName).Substring(rig.Root.Length) }, before.Keys);
                var preferences = PreferenceFiles(rig);
                rig.Session.ObserveStartup();
                Assert.IsTrue(rig.Session.CanCreate, rig.Session.Status);
                Assert.AreEqual(LocalPlayerProfileState.Absent, rig.Session.Observation.State);
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(0, rig.FactoryCalls);
                SameFilesAfterLocatorRead(rig, before, directories);
                rig.Session.CreateProfile();
                Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State, rig.Session.Status);
                Assert.IsTrue(rig.Session.Application.QueryView().View.IsPublishedHeadVerified);
                var original = rig.Session.OriginalProfile;
                var commit = rig.Head.Header.CommitId;
                var created = rig.Files();
                rig.Session.CreateProfile();
                Assert.AreSame(original, rig.Session.OriginalProfile);
                Assert.AreEqual(1, rig.FactoryCalls);
                Assert.AreEqual(commit, rig.Head.Header.CommitId);
                AssertPreference(rig, LocalePreferenceLoadDisposition.Remembered, locale);
                SamePreferences(rig, preferences);
                rig.SameFiles(created);
            }
        }

        [Test]
        public void H03_OwnedPreferenceResiduesPreserveBytes()
        {
            foreach (var kind in new[] { "empty", "empty-target", "utf8", "json", "tmp", "bak", "residues", "remembered", "bad-residues" })
            using (var rig = new HostRig(false))
            {
                Directory.CreateDirectory(Settings(rig));
                var target = Path.Combine(Settings(rig), PreferenceName);
                var invalid = kind == "empty-target" || kind == "utf8" || kind == "json" || kind == "bad-residues";
                if (kind == "remembered")
                    Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, Preferences(rig).Save(LocaleId.En).Disposition);
                else if (invalid) File.WriteAllBytes(target, kind == "empty-target" ? Array.Empty<byte>() :
                    kind == "utf8" ? new byte[] { 0xff, 0xc0, 0xaf } : new byte[] { (byte)'{' });
                if (kind == "tmp" || kind == "bak")
                    File.WriteAllText(Path.Combine(Settings(rig), PreferenceResidue + "." + kind), "{\"version\":1,\"locale\":\"zh-Hans\"}");
                else if (kind == "residues" || kind == "remembered" || kind == "bad-residues") AddPreferenceResidues(rig);
                var disposition = invalid ? LocalePreferenceLoadDisposition.InvalidSystemDefault :
                    kind == "remembered" ? LocalePreferenceLoadDisposition.Remembered : LocalePreferenceLoadDisposition.MissingSystemDefault;
                var preferences = PreferenceFiles(rig);
                AssertPreference(rig, disposition, LocaleId.En);
                rig.Create();
                AssertPreference(rig, disposition, LocaleId.En);
                Assert.AreEqual(1, rig.FactoryCalls);
                SamePreferences(rig, preferences);
                Assert.IsTrue(Directory.Exists(Settings(rig)));
            }
        }

        [Test]
        public void H03_UnknownSettingsOrPlayerDataStillBlock()
        {
            foreach (var kind in new[] { "unknown", "target-directory", "nested", "settings-file", "Settings",
                "root-file", "orphan-profile", "unknown-lock", "Locale-preference-v1.json",
                PreferenceName + "." + new string('a', 31) + ".tmp", PreferenceName + "." + new string('a', 33) + ".tmp",
                PreferenceName + "." + new string('A', 32) + ".tmp", PreferenceName + "." + new string('g', 32) + ".bak",
                PreferenceResidue + ".TMP", PreferenceResidue + ".bak.extra", PreferenceName + ".tmp" })
            using (var rig = new HostRig(false))
            {
                if (kind == "settings-file") File.WriteAllText(Settings(rig), "preserve");
                else if (kind == "Settings") Directory.CreateDirectory(Path.Combine(rig.Root, kind));
                else if (kind == "target-directory") Directory.CreateDirectory(Path.Combine(Settings(rig), PreferenceName));
                else if (kind == "Locale-preference-v1.json")
                {
                    Directory.CreateDirectory(Settings(rig));
                    File.WriteAllText(Path.Combine(Settings(rig), kind), "preserve");
                }
                else
                {
                    Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, Preferences(rig).Save(LocaleId.En).Disposition);
                    if (kind == "nested") Directory.CreateDirectory(Path.Combine(Settings(rig), "nested"));
                    else if (kind == "orphan-profile") Directory.CreateDirectory(Path.Combine(rig.Root, "profiles/p-orphan"));
                    else if (kind == "unknown-lock")
                    {
                        Directory.CreateDirectory(Path.Combine(rig.Root, "locator"));
                        File.WriteAllText(Path.Combine(rig.Root, "locator/unknown.lock"), "preserve");
                    }
                    else File.WriteAllText(kind == "root-file" ? Path.Combine(rig.Root, "unknown") :
                        Path.Combine(Settings(rig), kind), "preserve");
                }
                var files = rig.Files();
                var directories = Directory.GetDirectories(rig.Root, "*", SearchOption.AllDirectories);
                rig.Session.ObserveStartup();
                Assert.IsFalse(rig.Session.CanCreate, kind);
                Assert.AreEqual("UnclaimedData", rig.Session.Status, kind);
                SameFilesAfterLocatorRead(rig, files, directories);
                rig.Session.CreateProfile();
                Assert.AreEqual("UnclaimedData", rig.Session.Status, kind);
                Assert.IsNull(rig.Session.OriginalProfile, kind);
                Assert.AreEqual(0, rig.FactoryCalls, kind);
                SameFilesAfterLocatorRead(rig, files, directories);
            }
        }

        [Test]
        public void H03_SettingsLinksNeverAuthorizeCreation()
        {
            foreach (var dangling in new[] { false, true })
            foreach (var name in new[] { "settings", PreferenceName, PreferenceResidue + ".tmp", PreferenceResidue + ".bak" })
            using (var rig = new HostRig(false))
            {
                var outside = Path.Combine(Path.GetDirectoryName(rig.Root), "outside");
                Directory.CreateDirectory(outside);
                var sentinel = Path.Combine(outside, "sentinel");
                var bytes = new byte[] { 0, 255, 17 };
                File.WriteAllBytes(sentinel, bytes);
                var target = dangling ? Path.Combine(outside, "missing") : name == "settings" ? outside : sentinel;
                if (name != "settings") Directory.CreateDirectory(Settings(rig));
                var link = name == "settings" ? Settings(rig) : Path.Combine(Settings(rig), name);
                Assert.AreEqual(0, symlink(target, link));
                rig.Session.ObserveStartup();
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.AreEqual("StorageUnavailable", rig.Session.Status);
                rig.Session.CreateProfile();
                Assert.AreEqual("StorageUnavailable", rig.Session.Status);
                Assert.IsNull(rig.Session.OriginalProfile);
                Assert.AreEqual(0, rig.FactoryCalls);
                CollectionAssert.AreEqual(bytes, File.ReadAllBytes(sentinel));
                CollectionAssert.AreEquivalent(new[] { sentinel }, Directory.GetFileSystemEntries(outside));
                CollectionAssert.Contains(Directory.GetFileSystemEntries(Path.GetDirectoryName(link)), link);
                Assert.IsFalse(Directory.Exists(Path.Combine(rig.Root, "profiles")));
                Assert.IsFalse(File.Exists(Path.Combine(rig.Root, "locator", LocalPlayerProfileLocator.CreateRecordKey)));
            }
        }

        [Test]
        public void H03_SettingsAreRecheckedBeforeCreation()
        {
            foreach (var prepared in new[] { false, true })
            using (var rig = new HostRig(false))
            {
                rig.Session.ObserveStartup();
                Assert.IsTrue(rig.Session.CanCreate);
                if (prepared)
                {
                    rig.BeforeStorage = () => { throw new IOException("settings test before storage"); };
                    rig.Session.CreateProfile();
                    Assert.IsNotNull(rig.Session.OriginalProfile);
                    Assert.IsNull(rig.Storage);
                    Assert.AreEqual(LocalPlayerProfileState.Absent, rig.Session.Observation.State);
                    rig.BeforeStorage = null;
                }
                var original = rig.Session.OriginalProfile;
                Directory.CreateDirectory(Settings(rig));
                File.WriteAllText(Path.Combine(Settings(rig), "late-unknown"), "preserve");
                var files = rig.Files();
                var directories = Directory.GetDirectories(rig.Root, "*", SearchOption.AllDirectories);
                if (prepared) rig.Session.ContinueCreation(); else rig.Session.CreateProfile();
                Assert.AreEqual("UnclaimedData", rig.Session.Status);
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.AreSame(original, rig.Session.OriginalProfile);
                Assert.AreEqual(LocalPlayerProfileState.Absent, rig.Session.Observation.State);
                Assert.IsNull(rig.Head);
                if (!prepared) Assert.AreEqual(0, rig.FactoryCalls);
                rig.SameFiles(files);
                CollectionAssert.AreEquivalent(directories, Directory.GetDirectories(rig.Root, "*", SearchOption.AllDirectories));
            }
        }

        [Test]
        public void H03_SettingsSurviveInterruptedCreation()
        {
            foreach (var initialized in new[] { false, true })
            using (var rig = new HostRig(false))
            {
                Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, Preferences(rig).Save(LocaleId.En).Disposition);
                AddPreferenceResidues(rig);
                var preferences = PreferenceFiles(rig);
                rig.Profile.FailCreateAfterWrite = !initialized;
                rig.Profile.FailActive = initialized;
                rig.Session.ObserveStartup();
                rig.Session.CreateProfile();
                var record = rig.Session.OriginalProfile.CreateRecord;
                Assert.AreEqual(LocalPlayerProfileState.CreateIntentRecorded, rig.Session.Observation.State);
                var commit = rig.Head?.Header.CommitId;
                Assert.AreEqual(initialized, commit != null);
                SamePreferences(rig, preferences);
                rig.Profile.FailActive = false;
                rig.Rebuild();
                Assert.IsFalse(rig.Session.CanCreate);
                Assert.IsNull(rig.Session.OriginalProfile);
                rig.Session.ContinueCreation();
                Assert.AreEqual(record.RecordSha256, rig.Session.OriginalProfile.CreateRecord.RecordSha256);
                Assert.AreEqual(record.PlayerId, rig.Session.Observation.ActiveProfile.PlayerId);
                Assert.AreEqual(LocalPlayerProfileState.Active, rig.Session.Observation.State);
                if (initialized) Assert.AreEqual(commit, rig.Head.Header.CommitId);
                var files = rig.Files();
                rig.Session.ContinueCreation();
                rig.SameFiles(files);
                AssertPreference(rig, LocalePreferenceLoadDisposition.Remembered, LocaleId.En);
                SamePreferences(rig, preferences);
            }
        }

        [Test]
        public void H03_ActiveProfileWithSettingsReopensUnchanged()
        {
            using (var rig = new HostRig(false))
            {
                Assert.AreEqual(LocalePreferenceSaveDisposition.Saved, Preferences(rig).Save(LocaleId.ZhHans).Disposition);
                AddPreferenceResidues(rig);
                rig.Create();
                var player = rig.Session.Observation.ActiveProfile.PlayerId;
                var commit = rig.Head.Header.CommitId;
                foreach (var invalid in new[] { false, true })
                {
                    if (invalid) File.WriteAllBytes(Path.Combine(Settings(rig), PreferenceName), new byte[] { 0xff });
                    var files = rig.Files();
                    rig.Rebuild();
                    rig.Session.ObserveStartup();
                    rig.Session.ObserveStartup();
                    Assert.IsNull(rig.Session.OriginalProfile);
                    Assert.AreEqual(player, rig.Session.Observation.ActiveProfile.PlayerId);
                    Assert.AreEqual(commit, rig.Head.Header.CommitId);
                    Assert.IsFalse(rig.Session.CanCreate);
                    Assert.AreEqual(FightMatchHostPage.Navigation, rig.Session.Page);
                    AssertPreference(rig, invalid ? LocalePreferenceLoadDisposition.InvalidSystemDefault :
                        LocalePreferenceLoadDisposition.Remembered, invalid ? LocaleId.En : LocaleId.ZhHans);
                    rig.SameFiles(files);
                }
            }
        }
    }
}
