using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using FightMatch.Application;
using FightMatch.Content;
using FightMatch.Core;
using FightMatch.Platform;
using FightMatch.Presentation;
using QFramework;

namespace FightMatch.Host
{
    public enum FightMatchHostPage { Startup, Navigation, Battle, QuitConfirmation }

    public sealed class FightMatchHostSession : IDisposable
    {
        public const string ReleaseSetId = "release-set:fightmatch-demo-r1";
        private static FightMatchHostSession owner;
        private readonly int thread = Thread.CurrentThread.ManagedThreadId;
        private readonly IArchitecture architecture;
        private readonly PublishedContentCatalog catalog;
        private readonly LocalPlayerProfileLocator locator;
        private readonly Func<string, ILocalSaveStorage> storageFactory;
        private readonly SaveRecoveryCapabilities capabilities;
        private ILocalSaveStorage storage;
        private bool busy, disposed, preparationAttempted, routing, bridgedCommittedRoute;
        private FightMatchHostPage beforeQuit;
        public string ProductRoot { get; }
        public SaveStoreBudget Budget { get; }
        public CandidateApplicationSystem Application { get; }
        public PlayerSessionSystem Player { get; }
        public PlayerNavigationController Navigation { get; }
        public PlayerBattleController Battle { get; }
        public LocalPlayerProfileObservation Observation { get; private set; }
        public PreparedPlayerProfile OriginalProfile { get; private set; }
        public CandidateApplicationCallResult LastCall { get; private set; }
        public FightMatchHostPage Page { get; private set; }
        public string Status { get; private set; } = "ObserveRequired";
        public string StorageError { get; private set; }
        public bool CanCreate { get; private set; }
        internal bool AtNavigationRoot => Page == FightMatchHostPage.Navigation && Navigation.View.Context.Parents.Count == 0 &&
            (Navigation.View.Route == PlayerNavigationRoute.MapAdventure || Navigation.View.Route == PlayerNavigationRoute.Bag);
        public bool IsDisposed => disposed;
        public int DisposeCount { get; private set; }
        public event Action Changed;
        public event Action QuitRequested;

        public FightMatchHostSession(PublishedContentCatalog catalog, string absoluteProductRoot,
            IContentPublicationStorage profileStorage, Func<string, ILocalSaveStorage> storageFactory)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.storageFactory = storageFactory ?? throw new ArgumentNullException(nameof(storageFactory));
            if (absoluteProductRoot == null || !Path.IsPathRooted(absoluteProductRoot) ||
                Path.GetFullPath(absoluteProductRoot) != absoluteProductRoot)
                throw new ArgumentException("A canonical absolute product root is required.");
            ProductRoot = absoluteProductRoot;
            locator = new LocalPlayerProfileLocator(profileStorage);
            Budget = new SaveStoreBudget(new SaveCodecBudget(new ExactMathBudget(maxPrimitiveSteps: 64000000)));
            var binding = catalog.GetCurrentBinding("player", ReleaseSetId, ContentConsumerCapabilities.Current);
            if (!binding.IsAccepted) throw new InvalidDataException(binding.RejectionCode);
            var publication = catalog.ResolveExact(binding.Value, ContentConsumerCapabilities.Current);
            if (!publication.IsAccepted) throw new InvalidDataException(publication.RejectionCode);
            capabilities = Capabilities(publication.Value);
            if (owner != null) throw new InvalidOperationException("HostAlreadyActive");
            owner = this;
            try
            {
                architecture = FightMatchDemoArchitecture.Interface;
                Application = architecture.GetSystem<CandidateApplicationSystem>();
                Player = architecture.GetSystem<PlayerSessionSystem>();
                Navigation = new PlayerNavigationController(Player, Budget);
                Battle = new PlayerBattleController(Player, architecture.GetSystem<CandidateBattleApplicationSystem>(), Budget);
                Navigation.Changed += NavigationChanged;
                Navigation.HostRequested += AcceptHost;
                Battle.Changed += BattleChanged;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private static SaveRecoveryCapabilities Capabilities(ResolvedPublication publication)
        {
            var binding = publication.Binding;
            var bindings = new List<SaveBinding>
            {
                new SaveBinding(SaveBindingKind.Content, binding.PackageId, null, null, binding.ContentFingerprint,
                    binding.RuleVersion, binding.NumericContractVersion, binding.RandomContractVersion, null, null, null)
            };
            foreach (var level in publication.Definitions.Levels)
                bindings.Add(new SaveBinding(SaveBindingKind.Definition, binding.PackageId, null, null,
                    binding.ContentFingerprint, binding.RuleVersion, binding.NumericContractVersion,
                    binding.RandomContractVersion, null, level.LevelId, level.LevelVersion));
            return new SaveRecoveryCapabilities(PlayerSessionSystem.RequiredRecoveryContracts, bindings,
                new[] { binding.RuleVersion }, new[] { binding.NumericContractVersion },
                new[] { binding.RandomContractVersion }, PlayerSessionSystem.RequiredRecoveryFeatures);
        }

        private void Execute(Action action)
        {
            if (disposed || busy || Thread.CurrentThread.ManagedThreadId != thread) return;
            busy = true;
            try
            {
                StorageError = null;
                action();
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                                          error is NotSupportedException)
            {
                CanCreate = false;
                Status = "StorageUnavailable";
                StorageError = error.Message;
                Page = FightMatchHostPage.Startup;
            }
            finally
            {
                try { Changed?.Invoke(); }
                finally { busy = false; }
            }
        }

        private bool ReadLocator()
        {
            CanCreate = false;
            var read = locator.Read(Budget);
            Status = read.Code;
            if (!read.IsAccepted)
            {
                StorageError = read.Diagnostic?.ExceptionMessage;
                Page = FightMatchHostPage.Startup;
                return false;
            }
            Observation = read.Observation;
            if (OriginalProfile != null && Observation.CreateRecord != null &&
                OriginalProfile.CreateRecord.RecordSha256 != Observation.CreateRecord.RecordSha256)
            {
                Status = "OriginalCreationConflict";
                Page = FightMatchHostPage.Startup;
                return false;
            }
            return true;
        }

        public void ObserveStartup()
        {
            Execute(() =>
            {
                if (!ReadLocator()) return;
                Page = FightMatchHostPage.Startup;
                if (Observation.State == LocalPlayerProfileState.Absent)
                {
                    CanCreate = OriginalProfile == null && !preparationAttempted && EmptyRoot();
                    Status = CanCreate ? "EmptyRoot" : OriginalProfile != null ? "ContinueCreation" : "UnclaimedData";
                    return;
                }
                if (Observation.State == LocalPlayerProfileState.CreateIntentRecorded)
                {
                    Status = "ContinueCreation";
                    return;
                }
                var current = Application.QueryView().View;
                if (current.Phase == CandidateApplicationPhase.Unconfigured)
                {
                    storage = storage ?? storageFactory(Observation.ActiveProfile.PlayerId);
                    LastCall = Player.OpenExisting(Observation.ActiveProfile, storage, catalog, capabilities, Budget);
                }
                else LastCall = Application.Restore(Budget);
                RefreshOpened();
            });
        }

        public void CreateProfile()
        {
            if (OriginalProfile != null) { ContinueCreation(); return; }
            Execute(() =>
            {
                if (preparationAttempted || !ReadLocator()) return;
                if (Observation.State != LocalPlayerProfileState.Absent || !EmptyRoot())
                {
                    Status = "UnclaimedData";
                    return;
                }
                preparationAttempted = true;
                var prepared = Player.PrepareNewRosterProfile(catalog, ReleaseSetId, Budget.Codec);
                Status = prepared.Code;
                if (!prepared.IsAccepted) return;
                OriginalProfile = prepared.Request;
                storage = storageFactory(OriginalProfile.PlayerId);
                LastCall = Player.CreateNew(OriginalProfile, storage, locator, capabilities, Budget);
                CompleteCreationRead();
            });
        }

        public void ContinueCreation()
        {
            Execute(() =>
            {
                if (!ReadLocator()) return;
                if (OriginalProfile == null)
                {
                    if (Observation.CreateRecord == null) { Status = "ObserveRequired"; return; }
                    var recovered = Player.RecoverCreateIntent(Observation, catalog, Budget.Codec);
                    Status = recovered.Code;
                    if (!recovered.IsAccepted) return;
                    OriginalProfile = recovered.Request;
                    preparationAttempted = true;
                }
                storage = storage ?? storageFactory(OriginalProfile.PlayerId);
                if (Observation.State == LocalPlayerProfileState.Absent)
                {
                    if (!EmptyRoot()) { Status = "UnclaimedData"; return; }
                    LastCall = Player.CreateNew(OriginalProfile, storage, locator, capabilities, Budget);
                }
                else LastCall = Player.ContinueCreate(OriginalProfile, Observation, storage, locator, capabilities, Budget);
                CompleteCreationRead();
            });
        }

        private void CompleteCreationRead()
        {
            var actual = LastCall;
            if (!ReadLocator()) return;
            if (Observation.State == LocalPlayerProfileState.Active && actual.View.IsPublishedHeadVerified)
                RefreshOpened();
            else
            {
                Status = actual.Code;
                Page = FightMatchHostPage.Startup;
            }
        }

        private void RefreshOpened()
        {
            Status = LastCall?.Code ?? "Ready";
            Navigation.Refresh();
            var battle = Battle.Refresh();
            var observed = battle.Read.Application.ObservedCandidateCommitIds.Count != 0;
            Page = battle.Read.Head?.Continuation != null || (!observed && battle.Battle.History != null)
                ? FightMatchHostPage.Battle : FightMatchHostPage.Navigation;
        }

        public void AcceptHost(PlayerNavigationHostRequest request)
        {
            Execute(() =>
            {
                var actual = Battle.AcceptHost(request);
                Status = actual.Status;
                if (actual.Status == "RootBackRequested")
                {
                    beforeQuit = Page;
                    Page = FightMatchHostPage.QuitConfirmation;
                }
                else if (actual.Status == "CreationRequired") Page = FightMatchHostPage.Startup;
                else if (actual.Route != PlayerBattleRoute.HostNavigation && actual.Status != "StaleHostRequest")
                    Page = FightMatchHostPage.Battle;
            });
        }

        private void NavigationChanged()
        {
            if (disposed || routing) return;
            var view = Navigation.View;
            // Changed precedes HostRequested: never Refresh here and invalidate that original request.
            if (view.Route != PlayerNavigationRoute.CommittedResult) bridgedCommittedRoute = false;
            else if (!bridgedCommittedRoute && view.Result?.IsCommitted == true &&
                view.Result.LookupViewCommitId == view.Read.Head?.Header.CommitId)
            {
                var record = view.Result.OriginalLookup?.Record;
                if (record != null && (record.Intent.Kind == CandidateApplicationKind.SettleVictory ||
                    record.Intent.Kind == CandidateApplicationKind.ExitAttempt || record.Intent.Kind == CandidateApplicationKind.RestartAttempt))
                {
                    routing = true;
                    try
                    {
                        bridgedCommittedRoute = true;
                        var current = Battle.Refresh();
                        var result = Battle.OpenOriginalResult(record.OperationId, current.Context);
                        if (result.Receipt != null)
                        {
                            Navigation.ActionHandler(PlayerNavigationAction.Return)();
                            Page = FightMatchHostPage.Battle;
                        }
                    }
                    finally { routing = false; }
                }
            }
            Changed?.Invoke();
        }

        private void BattleChanged()
        {
            if (disposed || routing || busy || Page != FightMatchHostPage.Battle) return;
            routing = true;
            try
            {
                var view = Battle.View;
                if (view.Route == PlayerBattleRoute.HostNavigation && view.Status != "CreationRequired" &&
                    view.Status != "RootBackRequested")
                {
                    Page = FightMatchHostPage.Navigation;
                    Navigation.Refresh();
                    bridgedCommittedRoute = Navigation.View.Route == PlayerNavigationRoute.CommittedResult;
                }
            }
            finally { routing = false; }
            Changed?.Invoke();
        }

        public void CancelQuit() { Execute(() => Page = beforeQuit); }
        public void ConfirmQuit()
        {
            Execute(() => { if (Page == FightMatchHostPage.QuitConfirmation) QuitRequested?.Invoke(); });
        }
        public void Back()
        {
            if (disposed) return;
            if (AtNavigationRoot)
                Navigation.NavigationHandler(new PlayerNavigationTarget { Kind = PlayerNavigationTargetKind.RootBackRequested })();
            else if (Page == FightMatchHostPage.Navigation) Navigation.ActionHandler(PlayerNavigationAction.Back)();
            else if (Page == FightMatchHostPage.QuitConfirmation) CancelQuit();
            else Execute(() => { beforeQuit = Page; Page = FightMatchHostPage.QuitConfirmation; });
        }
        public void PausePresentation()
        {
            if (!disposed && Thread.CurrentThread.ManagedThreadId == thread) Battle.Playback.SkipToFinal();
        }

        [DllImport("libSystem.B.dylib", EntryPoint = "readlink", SetLastError = true)]
        private static extern IntPtr MacReadLink(string path, byte[] buffer, UIntPtr size);
        [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
        private static extern IntPtr AndroidReadLink(string path, byte[] buffer, UIntPtr size);
        private static void NoLinks(string path)
        {
            for (var current = path; !string.IsNullOrEmpty(current); current = Path.GetDirectoryName(current))
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                var read = AndroidReadLink(current, new byte[1], new UIntPtr(1));
#else
                var read = MacReadLink(current, new byte[1], new UIntPtr(1));
#endif
                if (read.ToInt64() >= 0) throw new NotSupportedException("Product root contains a symbolic link.");
                var error = Marshal.GetLastWin32Error();
                if (error != 22 && error != 2) throw new IOException("Root inspection failed: " + error);
            }
        }

        private bool EmptyRoot()
        {
            NoLinks(ProductRoot);
            try
            {
                if ((File.GetAttributes(ProductRoot) & FileAttributes.Directory) == 0) return false;
            }
            catch (FileNotFoundException) { return true; }
            catch (DirectoryNotFoundException) { return true; }
            foreach (var child in Directory.EnumerateFileSystemEntries(ProductRoot))
            {
                var name = Path.GetFileName(child);
                NoLinks(child);
                if ((name != "locator" && name != "profiles") || (File.GetAttributes(child) & FileAttributes.Directory) == 0)
                    return false;
                foreach (var leaf in Directory.EnumerateFileSystemEntries(child))
                {
                    NoLinks(leaf);
                    if (name != "locator" || Path.GetFileName(leaf) != "writer.lock" ||
                        (File.GetAttributes(leaf) & FileAttributes.Directory) != 0 || new FileInfo(leaf).Length != 0)
                        return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            if (Thread.CurrentThread.ManagedThreadId != thread) throw new InvalidOperationException("WrongThread");
            disposed = true;
            DisposeCount++;
            if (Navigation != null)
            {
                Navigation.Changed -= NavigationChanged;
                Navigation.HostRequested -= AcceptHost;
            }
            if (Battle != null) Battle.Changed -= BattleChanged;
            Changed = null;
            QuitRequested = null;
            try
            {
                Battle?.Dispose();
                Navigation?.Dispose();
            }
            finally
            {
                try { architecture?.Deinit(); }
                finally { if (ReferenceEquals(owner, this)) owner = null; }
            }
        }
    }
}
