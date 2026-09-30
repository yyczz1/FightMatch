using System;
using System.IO;
using System.Runtime.InteropServices;
using FightMatch.Application;
using FightMatch.Core;
using FightMatch.Platform;
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
                using (var first = new FightMatchHostView(rig.Session, "license")) { }
                using (var second = new FightMatchHostView(rig.Session, "license")) rig.Session.CreateProfile();
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
    }
}
