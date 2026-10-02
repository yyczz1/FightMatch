using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;
using Code = FightMatch.AssetAccess.FightMatchAssetDiagnosticCode;
using Stage = FightMatch.AssetAccess.FightMatchAssetDiagnosticStage;

namespace FightMatch.YooAssetAdapter
{
    internal interface IYooOperation
    {
        bool Done { get; }
        bool Success { get; }
        UnityEngine.Object Asset { get; }
        event Action<IYooOperation> Completed;
        void Release();
    }

    internal interface IYooSdk
    {
        bool Initialized { get; }
        int PackageCount { get; }
        void Initialize();
        void Destroy();
        object Find(string name);
        object Create(string name);
        bool Ready(object package);
        bool Busy(object package);
        bool Empty(object package);
        string Version(object package);
        IYooOperation Initialize(object package, string root);
        IYooOperation Manifest(object package, string version);
        bool HasLocation(object package, string location, Type type);
        IYooOperation Load(object package, string location, Type type);
        IYooOperation Destroy(object package);
        void Remove(string name);
    }

    internal sealed class RealYooSdk : IYooSdk
    {
        internal static readonly RealYooSdk Instance = new RealYooSdk();
        private RealYooSdk() { }
        public bool Initialized => YooAssets.IsInitialized;
        public int PackageCount => YooAssets.GetPackages().Count;
        public void Initialize() => YooAssets.Initialize();
        public void Destroy() => YooAssets.Destroy();
        public object Find(string name) => YooAssets.TryGetPackage(name, out var package) ? package : null;
        public object Create(string name) => YooAssets.CreatePackage(name);
        public bool Ready(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.Succeeded;
        public bool Busy(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.Processing;
        public bool Empty(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.None;
        public string Version(object package) => ((ResourcePackage)package).GetPackageVersion();
        public IYooOperation Initialize(object package, string root) =>
            new Operation(((ResourcePackage)package).InitializePackageAsync(new OfflinePlayModeOptions
            { BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(root) }));
        public IYooOperation Manifest(object package, string version) =>
            new Operation(((ResourcePackage)package).LoadPackageManifestAsync(new LoadPackageManifestOptions(version, 60)));
        public bool HasLocation(object package, string location, Type type) => ((ResourcePackage)package).GetAssetInfo(location, type).IsValid;
        public IYooOperation Load(object package, string location, Type type) =>
            new Operation(((ResourcePackage)package).LoadAssetAsync(location, type));
        public IYooOperation Destroy(object package) => new Operation(((ResourcePackage)package).DestroyPackageAsync());
        public void Remove(string name) => YooAssets.RemovePackage(name);

        private sealed class Operation : IYooOperation
        {
            private readonly AsyncOperationBase operation;
            private readonly AssetHandle handle;
            private Action<IYooOperation> completed;
            private bool subscribed, released;
            internal Operation(AsyncOperationBase operation) { this.operation = operation; }
            internal Operation(AssetHandle handle) { this.handle = handle; }
            public bool Done => handle != null ? handle.IsDone : operation.IsDone;
            public bool Success => (handle != null ? handle.Status : operation.Status) == EOperationStatus.Succeeded;
            public UnityEngine.Object Asset => handle?.AssetObject;
            private void Notify(AsyncOperationBase _) => completed?.Invoke(this);
            private void Notify(AssetHandle _) => completed?.Invoke(this);
            public event Action<IYooOperation> Completed
            {
                add
                {
                    completed += value;
                    if (subscribed) { if (Done) value(this); return; }
                    subscribed = true;
                    if (handle != null) handle.Completed += Notify;
                    else operation.Completed += Notify;
                }
                remove
                {
                    completed -= value;
                    if (completed != null || !subscribed) return;
                    if (handle != null) handle.Completed -= Notify;
                    else operation.Completed -= Notify;
                    subscribed = false;
                }
            }
            public void Release()
            {
                if (released) return;
                if (subscribed) throw new InvalidOperationException("Detach before release.");
                handle?.Release();
                released = true;
            }
        }
    }

    internal sealed class YooAssetPackageLifecycle
    {
        internal sealed class Failure : Exception
        {
            internal readonly Code Code;
            internal readonly Stage Stage;
            internal Failure(Code code, Stage stage) { Code = code; Stage = stage; }
        }
        private static readonly Dictionary<IYooSdk, Scope> scopes = new Dictionary<IYooSdk, Scope>();
        private readonly IYooSdk sdk;
        private readonly Action wake;
        private readonly HashSet<Package> packages = new HashSet<Package>();
        private Scope scope;
        private bool closing;
        internal bool GlobalDestroyed { get; private set; }
        internal YooAssetPackageLifecycle(IYooSdk sdk, Action wake)
        { this.sdk = sdk ?? throw new ArgumentNullException(nameof(sdk)); this.wake = wake; }

        private sealed class Scope
        {
            internal bool Owned;
            internal readonly HashSet<YooAssetPackageLifecycle> Clients = new HashSet<YooAssetPackageLifecycle>();
            internal readonly Dictionary<string, Package> Packages = new Dictionary<string, Package>(StringComparer.Ordinal);
        }

        internal sealed class Package
        {
            internal object Object;
            internal bool Owned, Removed, CleanupFailed;
            internal string Name, Root, Version;
            internal int Phase;
            internal IYooOperation Operation, Destruction;
            internal readonly HashSet<YooAssetPackageLifecycle> Clients = new HashSet<YooAssetPackageLifecycle>();
            internal readonly HashSet<Ticket> Uses = new HashSet<Ticket>();
            internal void Notify(IYooOperation sender)
            {
                if (!ReferenceEquals(sender, Operation) && !ReferenceEquals(sender, Destruction)) return;
                Wake();
            }
            internal void Wake() { foreach (var client in Clients.ToArray()) client.wake(); }
        }

        internal sealed class Ticket
        {
            private readonly YooAssetPackageLifecycle owner;
            private readonly Package package;
            private readonly AssetMapping mapping;
            private IYooOperation handle;
            private bool terminal, released, failed;
            internal readonly AssetRequestIdentity Identity;
            internal Code? Error { get; private set; }
            internal Stage Stage { get; private set; } = Stage.InitializePackage;
            internal UnityEngine.Object Asset => handle?.Asset;
            internal Ticket(YooAssetPackageLifecycle owner, Package package, AssetMapping mapping, AssetRequestIdentity identity)
            { this.owner = owner; this.package = package; this.mapping = mapping; Identity = identity; }
            private void Notify(IYooOperation sender)
            { if (ReferenceEquals(sender, handle)) owner.wake(); }
            internal void Invalidate() { failed = true; }
            internal void ObservePhaseFailure(Code code, Stage stage)
            {
                if (terminal || handle != null) return;
                Stage = stage;
                End(code);
            }
            internal bool Fail()
            {
                failed = true;
                try
                {
                    if (handle == null ? package.Operation == null || package.Operation.Done : handle.Done)
                    {
                        if (handle != null) handle.Completed -= Notify;
                        return End(Code.SdkFailure);
                    }
                }
                catch (Exception) { }
                return false;
            }

            internal bool Poll()
            {
                if (terminal) return true;
                if (handle == null)
                {
                    if (failed)
                    {
                        if (package.Operation != null && !package.Operation.Done) return false;
                        return End(Code.SdkFailure);
                    }
                    var ready = owner.Ready(package, out var code, out var stage);
                    Stage = stage;
                    if (!ready) return false;
                    if (code.HasValue || failed) return End(code ?? Code.SdkFailure);
                    Stage = Stage.ResolveLocation;
                    if (!owner.sdk.HasLocation(package.Object, mapping.Location, mapping.Type)) return End(Code.LocationUnavailable);
                    Stage = Stage.Acquire;
                    handle = owner.sdk.Load(package.Object, mapping.Location, mapping.Type);
                    if (handle == null) return End(Code.SdkFailure);
                    handle.Completed += Notify;
                }
                if (!handle.Done) return false;
                terminal = true;
                handle.Completed -= Notify;
                return End(failed || !handle.Success ? Code.SdkFailure : (Code?)null);
            }

            private bool End(Code? code) { Error = code; terminal = true; return true; }
            internal void Release()
            {
                if (released) return;
                if (!terminal) throw new InvalidOperationException("SDK operation still pending.");
                handle?.Release();
                package.Uses.Remove(this);
                released = true;
                package.Wake();
            }
        }

        internal Ticket Begin(AssetMapping map, AssetRequestIdentity identity)
        {
            if (closing) throw new InvalidOperationException("Lifecycle is closing.");
            if (scope == null)
            {
                if (!scopes.TryGetValue(sdk, out scope))
                {
                    scope = new Scope { Owned = !sdk.Initialized };
                    if (scope.Owned) sdk.Initialize();
                    scopes.Add(sdk, scope);
                }
                scope.Clients.Add(this);
            }
            if (!scope.Packages.TryGetValue(map.Package, out var package))
            {
                var found = sdk.Find(map.Package);
                if (found != null)
                {
                    if (!sdk.Ready(found)) throw new Failure(Code.PackageUnavailable, Stage.InitializePackage);
                    try { if (sdk.Version(found) != map.Version) throw new Failure(Code.ManifestUnavailable, Stage.SelectManifest); }
                    catch (Failure) { throw; }
                    catch (Exception) { throw new Failure(Code.ManifestUnavailable, Stage.SelectManifest); }
                }
                package = new Package { Object = found ?? sdk.Create(map.Package), Owned = found == null,
                    Name = map.Package, Root = map.Root, Version = map.Version };
                scope.Packages.Add(map.Package, package);
            }
            if (package.Removed || package.Destruction != null || package.Root != map.Root ||
                (package.Version != map.Version && (package.Uses.Count != 0 || !package.Owned)))
                throw new InvalidOperationException("Package identity is busy.");
            if (package.Owned && package.Operation != null && package.Operation.Done && !package.Operation.Success)
            {
                var stage = package.Phase == 0 ? Stage.InitializePackage : Stage.SelectManifest;
                var code = package.Phase == 0 ? Code.PackageUnavailable : Code.ManifestUnavailable;
                package.Operation.Completed -= package.Notify;
                foreach (var observer in package.Uses) observer.ObservePhaseFailure(code, stage);
                package.Operation = null;
                package.Wake();
            }
            if (package.Version != map.Version)
            {
                package.Version = map.Version;
                if (package.Phase != 0) package.Phase = 1;
            }
            package.Clients.Add(this);
            packages.Add(package);
            var ticket = new Ticket(this, package, map, identity);
            package.Uses.Add(ticket);
            return ticket;
        }

        private bool Ready(Package package, out Code? code, out Stage stage)
        {
            code = null;
            stage = Stage.InitializePackage;
            if (!package.Owned)
            {
                if (!sdk.Ready(package.Object)) code = Code.PackageUnavailable;
                else
                {
                    stage = Stage.SelectManifest;
                    try { if (sdk.Version(package.Object) != package.Version) code = Code.ManifestUnavailable; }
                    catch (Exception) { code = Code.ManifestUnavailable; }
                }
                return true;
            }
            while (package.Phase < 2)
            {
                stage = package.Phase == 0 ? Stage.InitializePackage : Stage.SelectManifest;
                if (package.Operation == null)
                {
                    package.Operation = package.Phase == 0 ? sdk.Initialize(package.Object, package.Root) :
                        sdk.Manifest(package.Object, package.Version);
                    package.Operation.Completed += package.Notify;
                }
                if (!package.Operation.Done) return false;
                package.Operation.Completed -= package.Notify;
                if (!package.Operation.Success)
                {
                    code = package.Phase == 0 ? Code.PackageUnavailable : Code.ManifestUnavailable;
                    return true;
                }
                package.Operation = null;
                package.Phase++;
            }
            stage = Stage.SelectManifest;
            try { if (sdk.Version(package.Object) != package.Version) code = Code.ManifestUnavailable; }
            catch (Exception) { code = Code.ManifestUnavailable; }
            return true;
        }

        internal bool TryClose(bool retry)
        {
            closing = true;
            if (scope == null) return true;
            foreach (var package in packages.ToArray())
            {
                if (sdk.Busy(package.Object)) return false;
                if (package.Uses.Count != 0 || (package.Operation != null && !package.Operation.Done)) return false;
                if (!package.Removed && package.Owned && package.Clients.All(client => client.closing))
                {
                    try
                    {
                        if (package.CleanupFailed && !retry) return false;
                        package.CleanupFailed = false;
                        if (package.Operation != null) package.Operation.Completed -= package.Notify;
                        if (package.Destruction == null)
                        {
                            package.Destruction = sdk.Destroy(package.Object);
                            package.Destruction.Completed += package.Notify;
                        }
                        if (!package.Destruction.Done) return false;
                        package.Destruction.Completed -= package.Notify;
                        if (!package.Destruction.Success)
                        {
                            package.Destruction = null;
                            package.CleanupFailed = true;
                            return false;
                        }
                        if (!ReferenceEquals(sdk.Find(package.Name), package.Object) || !sdk.Empty(package.Object)) return false;
                        sdk.Remove(package.Name);
                        package.Removed = true;
                        scope.Packages.Remove(package.Name);
                    }
                    catch (Exception) { package.CleanupFailed = true; return false; }
                }
                package.Clients.Remove(this);
                packages.Remove(package);
                if (!package.Owned && package.Clients.Count == 0) scope.Packages.Remove(package.Name);
            }
            if (scope.Clients.Count == 1 && scope.Owned && scope.Packages.Count == 0)
            {
                try
                {
                    if (sdk.PackageCount == 0) { sdk.Destroy(); GlobalDestroyed = true; }
                }
                catch (Exception) { return false; }
            }
            scope.Clients.Remove(this);
            if (scope.Clients.Count == 0 && scope.Packages.Count == 0) scopes.Remove(sdk);
            scope = null;
            return true;
        }
    }
}
