using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    // Real signed first release and real PlayerSave facade; only physical storage is replaced by bounded memory faults.
    internal sealed class NavigationRig : IDisposable
    {
        internal CandidateApplicationRuntime Runtime;
        internal CandidateApplicationSystem App;
        internal CandidateBattleApplicationSystem Battle;
        internal CandidateLifecycleApplicationSystem Life;
        internal PlayerSessionSystem Player;
        internal PlayerNavigationSession Nav;
        internal readonly PublishedContentCatalog Catalog;
        internal readonly ResolvedPublication Publication;
        internal readonly NavigationProfileStorage ProfileStorage = new NavigationProfileStorage();
        internal readonly LocalPlayerProfileLocator Locator;
        internal readonly PreparedPlayerProfile Profile;
        internal readonly NavigationStorage Storage;
        internal readonly SaveRecoveryCapabilities Capabilities;
        internal Action<CandidateApplicationPublished> OnPublish;
        internal int Publications;
        internal CandidateApplicationSnapshot Head => App.QueryView().View.PublishedSnapshot;
        internal PlayerNavigationView View => Nav.Query(Codec());
        internal NavigationRig(bool initialize = true, bool legacy = false, PublishedContentCatalog catalog = null, ResolvedPublication publication = null)
        {
            Catalog = catalog ?? RealCatalog(); Publication = publication ?? Resolve(Catalog);
            Locator = new LocalPlayerProfileLocator(ProfileStorage);
            Capabilities = RosterCaps(Publication);
            Systems();
            Profile = Prepared(legacy ? Player.PrepareNewProfile(Catalog, Release, Codec()) : Player.PrepareNewRosterProfile(Catalog, Release, Codec()));
            Storage = new NavigationStorage(new MemorySave(Profile.PlayerId));
            if (initialize) Is(Player.CreateNew(Profile, Storage, Locator, Capabilities, Budget()));
        }
        private void Systems()
        {
            Runtime = new CandidateApplicationRuntime(e => { Publications++; OnPublish?.Invoke(e); });
            App = new CandidateApplicationSystem(Runtime); Battle = new CandidateBattleApplicationSystem(App);
            Life = new CandidateLifecycleApplicationSystem(App, Battle); Player = new PlayerSessionSystem(Runtime, App, Life);
            Nav = Player.GetNavigationSession();
        }
        internal CandidateApplicationCallResult Rebuild()
        {
            var active = Locator.Read(Budget()).Observation.ActiveProfile;
            Runtime.Close(); Nav = null; Player = null; Life = null; Battle = null; App = null; Runtime = null;
            Systems();
            return Player.OpenExisting(active, Storage, Catalog, Capabilities, Budget());
        }
        internal PlayerNavigationView Go(PlayerNavigationTargetKind kind, string character = null, string commit = null, string operation = null)
        {
            var view = View;
            return Nav.Navigate(new PlayerNavigationTarget { Kind = kind, CharacterId = character, CommitId = commit,
                OperationId = operation }, view.Context, Codec());
        }
        internal PlayerNavigationView PreparePage()
        {
            var v = View; var level = v.Read.Levels[0];
            return Nav.Navigate(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.Preparation,
                LevelId = level.LevelId, LevelVersion = level.LevelVersion, Binding = v.Read.Binding }, v.Context, Codec());
        }
        internal PlayerNavigationView Formation(params string[] slots)
        {
            var v = View;
            return Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Formation, Slots = slots }, v.Context, Codec());
        }
        internal PlayerNavigationView Act(PlayerNavigationAction action) => Nav.Act(action, View.Token, Budget());
        internal PlayerNavigationView Migrate()
        {
            var v = View; var format = (uint)Head.Business.Format;
            var preview = Nav.Preview(new PlayerNavigationDraft { Kind = PlayerNavigationDraftKind.Migration,
                FromFormat = format, ToFormat = format + 1 }, v.Context, Codec());
            Assert.AreEqual(PlayerNavigationRoute.Confirmation, preview.Route, preview.Status);
            return Nav.Act(PlayerNavigationAction.Confirm, preview.Token, Budget());
        }
        internal void Enter()
        {
            Is(Life.Submit(Prepared(Player.PrepareFormationEntry(EntryDraft(Head, Publication.Definitions.Levels[0]), Time(), Codec())), Budget()));
        }
        internal void SelectWarrior() { Go(PlayerNavigationTargetKind.SelectCharacter, Head.Business.Character.CharacterId); }
        internal Dictionary<string, byte[]> Files() => CopyFiles(Storage.Inner.Files);
        public void Dispose() { Runtime?.Close(); }
    }
    internal sealed class NavigationStorage : ILocalSaveStorage
    {
        internal readonly MemorySave Inner;
        internal bool FailPrepare, FailRead, FailMarkerWork, FailLease;
        internal Action DuringWrite;
        internal string Fault { set { Inner.Fault = value; } }
        internal NavigationStorage(MemorySave inner) { Inner = inner; }
        public SaveStorageProfile Profile => Inner.Profile;
        public IDisposable AcquireWriterLease(bool create)
        { if (FailLease) { FailLease = false; throw new IOException("navigation isolated create lease"); } return Inner.AcquireWriterLease(create); }
        public IEnumerable<string> EnumerateNames()
        {
            if (FailPrepare) { FailPrepare = false; throw new IOException("navigation isolated prepare inspection"); }
            return Inner.EnumerateNames();
        }
        public Stream OpenRead(string name)
        { if (FailRead) throw new IOException("navigation isolated readback"); return Inner.OpenRead(name); }
        public Stream CreateWork(string name)
        {
            DuringWrite?.Invoke();
            if (FailMarkerWork && !name.EndsWith(".snapshot.tmp"))
            { FailMarkerWork = false; throw new IOException("navigation isolated marker create"); }
            return Inner.CreateWork(name);
        }
        public void FlushFile(Stream stream) => Inner.FlushFile(stream);
        public void PromoteNoReplace(string work, string final) => Inner.PromoteNoReplace(work, final);
        public void DeleteUncommitted(string name) => Inner.DeleteUncommitted(name);
        public void DeleteIndexedOld(string name) => Inner.DeleteIndexedOld(name);
    }
    internal sealed class NavigationProfileStorage : IContentPublicationStorage
    {
        private readonly PublishedContentTestData.MemoryStorage inner = new PublishedContentTestData.MemoryStorage();
        internal bool FailConfirmation;
        public IDisposable AcquireWriter() => inner.AcquireWriter();
        public byte[] Read(string key, int maxBytes) => inner.Read(key, maxBytes);
        public void WriteImmutable(string key, byte[] bytes, int maxBytes)
        {
            if (FailConfirmation && key == LocalPlayerProfileLocator.ActiveRecordKey)
            { FailConfirmation = false; throw new IOException("navigation isolated profile confirmation"); }
            inner.WriteImmutable(key, bytes, maxBytes);
        }
    }
    internal static class NavigationAssertions
    {
        internal static void Committed(PlayerNavigationView v)
        {
            Assert.AreEqual(PlayerNavigationRoute.CommittedResult, v.Route, v.Status);
            Assert.IsTrue(v.Result.IsCommitted); Assert.IsTrue(v.Result.OriginalLookup.IsFound);
            Assert.AreEqual(v.Result.View.PublishedSnapshot.Header.CommitId, v.Result.LookupViewCommitId);
        }
        internal static void Unchanged(NavigationRig rig, CandidateApplicationSnapshot head, Dictionary<string, byte[]> files)
        { Assert.AreSame(head, rig.Head); SameFiles(files, rig.Storage.Inner.Files); }
    }
}
