using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Platform;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using FightMatch.Presentation;
using PlayerNavigationView = FightMatch.Application.PlayerNavigationView;
using static FightMatch.Core.Tests.PlayerSessionTestData;
using static FightMatch.Core.Tests.PlayerRosterTestData;

namespace FightMatch.Core.Tests
{
    internal sealed class NavigationPanel : IDisposable
    {
        private readonly GameObject root;
        private readonly GameObject eventRoot;
        private readonly FightMatch.Host.FightMatchHostView hostView;
        private readonly FightMatchResponsiveLayout responsiveLayout;
        private readonly EventSystem[] previousEventSystems;
        internal readonly FightMatch.Presentation.PlayerNavigationView View;
        internal readonly LocalizationService Localization;
        internal EventSystem EventSystem { get; }
        internal NavigationPanel(PlayerNavigationController controller)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab");
            Assert.IsNotNull(prefab, "The saved uGUI RuntimeRoot prefab is required.");
            Assert.IsFalse(prefab.activeSelf, "The serialized RuntimeRoot must bind before activation.");
            root = UnityEngine.Object.Instantiate(prefab);
            hostView = root.GetComponent<FightMatch.Host.FightMatchHostView>();
            responsiveLayout = root.GetComponentInChildren<FightMatchResponsiveLayout>(true);
            Assert.IsNotNull(hostView); Assert.IsNotNull(responsiveLayout);
            foreach (var behaviour in root.GetComponents<MonoBehaviour>()) behaviour.enabled = false;
            foreach (var id in root.GetComponentsInChildren<FightMatchViewId>(true))
            {
                if (id.Id.StartsWith("fm.page.", StringComparison.Ordinal))
                    id.gameObject.SetActive(id.Id == "fm.page.navigation");
                if (id.Id == "fm.popup.reference" || id.Id == "fm.popup.language" ||
                    id.Id == "fm.popup.license" || id.Id == "fm.popup.quit" || id.Id == "fm.popup.blocking")
                    id.gameObject.SetActive(false);
                if (id.Id == "fm.popup.confirmation" || id.Id == "fm.popup.recovery") id.gameObject.SetActive(false);
            }
            View = root.GetComponentInChildren<FightMatch.Presentation.PlayerNavigationView>(true);
            Assert.IsNotNull(View);
            previousEventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>().Where(x => x.enabled).ToArray();
            foreach (var previous in previousEventSystems) previous.enabled = false;
            eventRoot = new GameObject("NavigationTestEventSystem", typeof(EventSystem), typeof(FightMatchStandaloneInputModule));
            EventSystem = eventRoot.GetComponent<EventSystem>();
            Localization = new LocalizationService(new UguiTestTextSource(), SystemLanguage.English);
            View.gameObject.SetActive(true);
            View.Bind(controller, Localization);
            responsiveLayout.Bind(Localization);
            hostView.SynchronizeOverlayRoots();
            root.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }
        internal T Find<T>(string id) where T : Component
        {
            hostView.SynchronizeOverlayRoots();
            var component = FightMatchViewId.Find<T>(root.transform, id);
            Assert.IsNotNull(component, id);
            return component;
        }
        internal T Optional<T>(string id) where T : Component
        { hostView.SynchronizeOverlayRoots(); return FightMatchViewId.Find<T>(root.transform, id); }
        internal LocalizedTmpText[] VisibleTexts
        {
            get { hostView.SynchronizeOverlayRoots(); return root.GetComponentsInChildren<LocalizedTmpText>(true)
                .Where(x => x.gameObject.activeInHierarchy).ToArray(); }
        }
        internal int CountKey(string key) => VisibleTexts.Count(x => x.Key == key);
        internal void AssertVisible(string key, params KeyValuePair<string, string>[] args)
        {
            var matches = VisibleTexts.Where(x => x.Key == key).ToArray();
            Assert.AreEqual(1, matches.Length, key);
            AssertText(matches[0], key, args);
        }
        internal void AssertHeading(string key)
        {
            var matches = VisibleTexts.Where(x => x.Key == key && x.GetComponentInParent<UnityEngine.UI.Button>() == null).ToArray();
            Assert.AreEqual(1, matches.Length, key); AssertText(matches[0], key);
        }
        internal void AssertNoIdentity(params string[] identities)
        {
            foreach (var text in VisibleTexts)
                foreach (var identity in identities.Where(x => !string.IsNullOrEmpty(x)))
                    StringAssert.DoesNotContain(identity, text.Target.text);
        }
        internal void Click(string id) { Click(Find<UnityEngine.UI.Button>(id)); }
        internal void Click(UnityEngine.UI.Button button)
        {
            Assert.IsNotNull(button); Assert.IsTrue(button.isActiveAndEnabled, button.name);
            Assert.IsTrue(button.interactable, button.name);
            EventSystem.SetSelectedGameObject(button.gameObject);
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem) {
                button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            hostView.SynchronizeOverlayRoots();
        }
        internal void AssertText(string id, string key, params KeyValuePair<string, string>[] args)
        { AssertText(Find<LocalizedTmpText>(id), key, args); }
        internal void AssertCaption(string id, string key, params KeyValuePair<string, string>[] args)
        { AssertText(Find<UnityEngine.UI.Button>(id).GetComponentsInChildren<LocalizedTmpText>(true)[0], key, args); }
        private void AssertText(LocalizedTmpText text, string key, params KeyValuePair<string, string>[] args)
        {
            Assert.AreEqual(key, text.Key);
            Assert.IsNull(text.DiagnosticCode);
            var expected = Localization.Resolve(key, args);
            Assert.IsTrue(expected.IsSuccess, expected.DiagnosticCode);
            Assert.AreEqual(expected.Text, text.Target.text);
            Assert.AreNotEqual(LocalizedTmpText.Placeholder, text.Target.text);
        }
        internal void AssertReason(string id, string expectedCode)
        {
            var button = Find<UnityEngine.UI.Button>(id);
            Assert.IsFalse(button.interactable, expectedCode);
            var labels = button.GetComponentsInChildren<LocalizedTmpText>(true);
            Assert.GreaterOrEqual(labels.Length, 2);
            var reason = labels[1];
            Assert.IsTrue(reason.gameObject.activeInHierarchy, expectedCode);
            var expectedKey = expectedCode == "ActiveAttemptConflict" ? "fm.inventory.active_battle.locked" :
                expectedCode == "NoItemEquipped" ? "fm.inventory.no_equipment" : null;
            Assert.IsNotNull(expectedKey, "Add an explicit test expectation for this business reason.");
            Assert.AreEqual(expectedKey, reason.Key, expectedCode);
            Assert.IsNull(reason.DiagnosticCode, expectedCode);
            Assert.AreNotEqual(LocalizedTmpText.Placeholder, reason.Target.text, expectedCode);
            var args = expectedCode == "NoItemEquipped" ? new[] {
                new KeyValuePair<string, string>("characterName", Localization.Resolve("fm.name.character.w", null).Text) } : null;
            Assert.AreEqual(Localization.Resolve(expectedKey, args).Text, reason.Target.text, expectedCode);
        }
        internal static string Row(string businessId) { return FightMatchViewId.Row(businessId); }
        public void Dispose()
        {
            View.Unbind(); responsiveLayout.Unbind(); hostView.SynchronizeOverlayRoots();
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(eventRoot);
            foreach (var previous in previousEventSystems) if (previous != null) previous.enabled = true;
        }
    }
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
        // Immutable read-model fixtures exercise binding classification; they never mutate Controller.View or submit actions.
        internal static PlayerNavigationView ReadModel(PlayerNavigationView basis, PlayerNavigationRoute? route = null,
            PlayerNavigationReadResult read = null, string status = null, CandidateApplicationDiagnostic diagnostic = null,
            CandidateApplicationCallResult result = null, PlayerNavigationConfirmation confirmation = null,
            PlayerPermanentView permanent = null, PlayerNavigationHostRequest host = null)
        {
            var context = basis.Context;
            return new PlayerNavigationView(read ?? basis.Read, new PlayerNavigationContext(context.Owner, context.Revision,
                read ?? basis.Read, route ?? context.Route, context.ReturnAnchor, context.LevelId, context.LevelVersion,
                context.SelectedCharacterId, context.Parents), basis.Token, permanent ?? basis.Permanent, basis.DetailKind,
                basis.DefinitionId, confirmation, result, diagnostic, status, basis.SelectedCommitId, basis.SelectedOperationId, host, basis.Actions);
        }
        internal static PlayerNavigationReadResult ReadState(PlayerNavigationReadResult basis, CandidateApplicationPhase phase,
            CandidateApplicationDiagnostic diagnostic = null, string pending = null)
        {
            var app = new CandidateApplicationView(basis.Application.PlayerId, phase, basis.Head,
                basis.Application.IsPublishedHeadVerified, pending, null, Array.Empty<string>(), diagnostic, basis.Application.Purpose);
            return new PlayerNavigationReadResult(app, basis.Binding, basis.Levels, basis.Roster, basis.Inventory,
                basis.Progression, basis.Lifecycle, diagnostic);
        }
        internal static CandidateApplicationCallResult Saved(CandidatePermanentTestData data, PreparedCandidateLifecycleRequest request)
        {
            var lookup = TakeCore(CandidateApplicationProtocol.Lookup(data.Head, request.Intent, Codec()));
            var app = new CandidateApplicationView(data.Head.Business.PlayerId, CandidateApplicationPhase.Ready,
                data.Head, true, null, null, Array.Empty<string>(), null, SavePurpose.PlayerSave);
            return new CandidateApplicationCallResult("Completed", true, lookup.OriginalCommitId, lookup,
                data.Head.Header.CommitId, app, null, null);
        }
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
