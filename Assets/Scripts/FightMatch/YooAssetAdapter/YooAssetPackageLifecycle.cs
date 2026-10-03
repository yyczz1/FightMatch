using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using FightMatch.AssetAccess;
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

    internal interface IRawYooSdk
    {
        IRawYooOperation BeginRaw(object package, AssetMapping map, long limit, Action wake);
    }

    internal interface IRawYooOperation : IYooOperation
    {
        object Payload { get; }
        Code? Error { get; }
        void Pump(bool stopRequested);
    }

    internal sealed class RawReadState : IDisposable
    {
        private Stream input;
        private SHA256 sha;
        private byte[] bytes;
        private RawBuffer payload;
        private readonly long expectedLength;
        private readonly string expectedSha;
        private int position;
        internal bool Done { get; private set; }
        internal Code? Error { get; private set; }
        internal object Payload { get { var result = payload; payload = null; return result; } }
        internal RawReadState(Stream input, long expectedLength, string expectedSha, long limit, Func<int, byte[]> allocate)
        {
            this.input = input;
            this.expectedLength = expectedLength;
            this.expectedSha = expectedSha;
            try
            {
                if (input == null || !input.CanRead || !input.CanSeek) { Fail(Code.SdkFailure); return; }
                var actual = input.Length;
                if (expectedLength <= 0 || expectedLength > Math.Min(16777216L, limit) ||
                    actual > Math.Min(16777216L, limit)) { Fail(Code.BudgetExceeded); return; }
                if (actual != expectedLength || input.Position != 0) { Fail(Code.SdkFailure); return; }
                sha = SHA256.Create();
                bytes = allocate(checked((int)actual));
                if (bytes == null || bytes.LongLength != actual) Fail(Code.SdkFailure);
            }
            catch (Exception) { Fail(Code.SdkFailure); }
        }
        internal void Pump(bool stopRequested = false)
        {
            if (Done) return;
            if (stopRequested) { Fail(Code.SdkFailure); return; }
            try
            {
                if (position < bytes.Length)
                {
                    var count = input.Read(bytes, position, Math.Min(65536, bytes.Length - position));
                    if (count <= 0 || count > Math.Min(65536, bytes.Length - position))
                    { Fail(Code.SdkFailure); return; }
                    sha.TransformBlock(bytes, position, count, bytes, position);
                    position += count;
                    return;
                }
                if (input.ReadByte() != -1 || input.Length != expectedLength) { Fail(Code.SdkFailure); return; }
                sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                if (BitConverter.ToString(sha.Hash).Replace("-", "").ToLowerInvariant() != expectedSha)
                { Fail(Code.SdkFailure); return; }
                CloseInput();
                payload = new RawBuffer(bytes);
                bytes = null;
                Done = true;
            }
            catch (Exception) { Fail(Code.SdkFailure); }
        }
        private void Fail(Code code)
        {
            Error = code;
            Done = true;
            bytes = null;
            try { Dispose(); } catch (Exception) { } // A failed close is retried by Ticket.Release.
        }
        private void CloseInput()
        {
            if (input != null) { input.Dispose(); input = null; }
            if (sha != null) { sha.Dispose(); sha = null; }
        }
        public void Dispose()
        {
            if (!Done) { Done = true; Error = Code.SdkFailure; }
            bytes = null;
            payload?.Release();
            payload = null;
            CloseInput();
        }
    }

    internal sealed class RealYooSdk : IYooSdk, IRawYooSdk
    {
        internal static readonly RealYooSdk Instance = new RealYooSdk();
        private readonly HashSet<object> rawPackages = new HashSet<object>();
        private RealYooSdk() { }
        public bool Initialized => YooAssets.IsInitialized;
        public int PackageCount => YooAssets.GetPackages().Count;
        public void Initialize() => YooAssets.Initialize();
        public void Destroy() { YooAssets.Destroy(); rawPackages.Clear(); }
        public object Find(string name) => YooAssets.TryGetPackage(name, out var package) ? package : null;
        public object Create(string name) => YooAssets.CreatePackage(name);
        public bool Ready(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.Succeeded;
        public bool Busy(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.Processing;
        public bool Empty(object package) => ((ResourcePackage)package).InitializeStatus == EOperationStatus.None;
        public string Version(object package) => ((ResourcePackage)package).GetPackageVersion();
        public IYooOperation Initialize(object package, string root)
        {
            var operation = ((ResourcePackage)package).InitializePackageAsync(new OfflinePlayModeOptions
            { BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(root) });
            rawPackages.Add(package);
            return new Operation(operation);
        }
        public IYooOperation Manifest(object package, string version) =>
            new Operation(((ResourcePackage)package).LoadPackageManifestAsync(new LoadPackageManifestOptions(version, 60)));
        public bool HasLocation(object package, string location, Type type) => ((ResourcePackage)package).GetAssetInfo(location, type).IsValid;
        public IYooOperation Load(object package, string location, Type type) =>
            new Operation(((ResourcePackage)package).LoadAssetAsync(location, type));
        public IYooOperation Destroy(object package) => new Operation(((ResourcePackage)package).DestroyPackageAsync());
        public void Remove(string name)
        {
            var package = Find(name);
            YooAssets.RemovePackage(name);
            rawPackages.Remove(package);
        }
        public IRawYooOperation BeginRaw(object package, AssetMapping map, long limit, Action wake)
        {
            if (!rawPackages.Contains(package) || !Ready(package))
                throw new YooAssetPackageLifecycle.Failure(Code.PackageUnavailable, Stage.ValidateResult);
            return new RawOperation((ResourcePackage)package, map, limit, wake);
        }
        internal static bool IsPlainRawBundle(int kind, bool encrypted) =>
            kind == (int)EBundleType.RawBundle && !encrypted;

        private sealed class RawOperation : IRawYooOperation
        {
            private readonly EnsureBundleFileOperation ensure;
            private readonly AssetMapping map;
            private readonly long limit;
            private readonly Action wake;
            private RawReadState reader;
            private bool released;
            public bool Done { get; private set; }
            public bool Success { get; private set; }
            public UnityEngine.Object Asset => null;
            public object Payload => reader?.Payload;
            public Code? Error { get; private set; }
            public event Action<IYooOperation> Completed;
            internal RawOperation(ResourcePackage package, AssetMapping map, long limit, Action wake)
            {
                this.map = map; this.limit = limit; this.wake = wake;
                ensure = package.EnsureBundleFileAsync(new EnsureBundleFileOptions(map.Location));
                ensure.Completed += Notify;
            }
            private void Notify(AsyncOperationBase _) { wake(); }
            public void Pump(bool stopRequested)
            {
                if (Done || !ensure.IsDone) return;
                try
                {
                    ensure.Completed -= Notify;
                    if (stopRequested)
                    {
                        reader?.Pump(true);
                        Error = Code.SdkFailure;
                        End(false);
                        return;
                    }
                    if (ensure.Status != EOperationStatus.Succeeded) { End(false); return; }
                    if (reader == null)
                    {
                        var detail = ensure.Detail;
                        if (!IsPlainRawBundle(detail.BundleType, detail.IsEncrypted))
                        { Error = Code.WrongAssetType; End(false); return; }
                        reader = new RawReadState(new FileStream(detail.BundleFilePath, FileMode.Open,
                            FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan),
                            map.RawSource.Length, map.RawSource.Sha, limit, length => new byte[length]);
                    }
                    reader.Pump();
                    if (!reader.Done) { wake(); return; }
                    Error = reader.Error;
                    End(!Error.HasValue);
                }
                catch (Exception) { Error = Code.SdkFailure; End(false); }
            }
            private void End(bool success)
            {
                Done = true; Success = success;
                Completed?.Invoke(this);
            }
            public void Release()
            {
                if (released) return;
                if (!Done) throw new InvalidOperationException("Raw operation still pending.");
                ensure.Completed -= Notify;
                reader?.Dispose();
                reader = null;
                released = true;
            }
        }


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
            internal bool ManifestValidationFailed;
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
            private readonly long rawLimit;
            private bool terminal, released, failed;
            internal readonly AssetRequestIdentity Identity;
            internal Code? Error { get; private set; }
            internal Stage Stage { get; private set; } = Stage.InitializePackage;
            internal UnityEngine.Object Asset => handle?.Asset;
            internal object RawPayload => (handle as IRawYooOperation)?.Payload;
            internal Ticket(YooAssetPackageLifecycle owner, Package package, AssetMapping mapping, AssetRequestIdentity identity, long rawLimit)
            { this.owner = owner; this.package = package; this.mapping = mapping; Identity = identity; this.rawLimit = rawLimit; }
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
                    if (mapping.Raw && (!package.Owned || !(owner.sdk is IRawYooSdk)))
                    { Stage = Stage.ValidateResult; return End(Code.PackageUnavailable); }
                    Stage = Stage.ResolveLocation;
                    if (!owner.sdk.HasLocation(package.Object, mapping.Location, mapping.Raw ? null : mapping.Type)) return End(Code.LocationUnavailable);
                    Stage = Stage.Acquire;
                    try
                    {
                        handle = mapping.Raw ? ((IRawYooSdk)owner.sdk).BeginRaw(package.Object, mapping, rawLimit, owner.wake) :
                            owner.sdk.Load(package.Object, mapping.Location, mapping.Type);
                    }
                    catch (Failure failure) { Stage = failure.Stage; return End(failure.Code); }
                    if (handle == null) return End(Code.SdkFailure);
                    handle.Completed += Notify;
                }
                if (handle is IRawYooOperation raw)
                {
                    raw.Pump(failed);
                    if (raw.Error.HasValue) Stage = Stage.ValidateResult;
                }
                if (!handle.Done) return false;
                terminal = true;
                handle.Completed -= Notify;
                return End(failed ? Code.SdkFailure : (handle as IRawYooOperation)?.Error ??
                    (!handle.Success ? Code.SdkFailure : (Code?)null));
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

        internal Ticket Begin(AssetMapping map, AssetRequestIdentity identity, long rawLimit = 0)
        {
            if (closing) throw new InvalidOperationException("Lifecycle is closing.");
            if (scope == null)
            {
                if (!scopes.TryGetValue(sdk, out var registeredScope))
                {
                    registeredScope = new Scope { Owned = !sdk.Initialized };
                    if (registeredScope.Owned) sdk.Initialize();
                    scopes.Add(sdk, registeredScope);
                }
                registeredScope.Clients.Add(this);
                scope = registeredScope;
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
            if (package.Owned && package.ManifestValidationFailed)
            {
                package.ManifestValidationFailed = false;
                package.Phase = 1;
            }
            if (package.Version != map.Version)
            {
                package.Version = map.Version;
                if (package.Phase != 0) package.Phase = 1;
            }
            package.Clients.Add(this);
            packages.Add(package);
            var ticket = new Ticket(this, package, map, identity, rawLimit);
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
            if (package.ManifestValidationFailed)
            {
                code = Code.ManifestUnavailable;
                stage = Stage.SelectManifest;
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
            if (code.HasValue)
            {
                package.ManifestValidationFailed = true;
                foreach (var observer in package.Uses)
                    observer.ObservePhaseFailure(Code.ManifestUnavailable, Stage.SelectManifest);
                package.Wake();
            }
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
