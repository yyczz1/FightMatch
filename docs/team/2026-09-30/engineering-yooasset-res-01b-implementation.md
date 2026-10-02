# RES-01B · Project asset interface and YooAsset adapter implementation packet

## 0. Packet state

| Field | Value |
|---|---|
| Task | `RES-01B` |
| Revision | `prepared-02` |
| State | `PREPARED_NOT_AUTHORIZED` |
| Dependency state | `BLOCKED_ON_RES_01A_ACCEPT_AND_C_SERIAL_SLOT` |
| Execution state | `IMPLEMENTATION_NOT_RUN` |
| Sole future delivery owner | C, thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`, host `local` |
| Dispatch authority / return path | Engineering lead, thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b`, host `local` |
| Packet author | Architecture role, thread `01a0f2e6-1ac3-7d10-8855-54192b9489d4`, host `local` |
| Independent acceptance | Testing lead / unit-review identity must be bound before dispatch; C and the packet author cannot accept their own delivery |

This is a prepared packet, not permission to implement it. The engineering lead
may sign and dispatch it only after RES-01A has an independent `ACCEPT`, its
exact accepted package graph is frozen, C has finished the preceding work and
released/reacquired the serial Unity slot, and the stage-1 tokens in section 3
have been replaced with observed evidence. Stage 1 must then stop for the
separate verification appendix before any Unity action.

No Unity, UPM, network, Git write, product-file change, or RES-01B
implementation was performed while preparing this packet. It must not be sent
to the independent reviewer before an actual C delivery exists.

## 1. Fixed authority and dependency

The product-design authority is:

- `docs/team/2026-09-30/engineering-yooasset-001.md`
- SHA-256:
  `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`
- Identity: 859 lines / 62080 bytes
- Governing sections: §10, §16, RES-01B in §18, and YA-02 through YA-05 plus
  YA-16 in §19.

RES-01A is a hard dependency. Its package graph, immutable YooAsset source,
runtime assembly identity, Mac Intel compile, and independent verdict must be
accepted inputs. A merely installed package, C's own success claim, a design
approval, or this packet's existence is not sufficient.

The module is deliberately deeper and narrower than YooAsset. Callers provide
one validated project asset id, one exact release-set id, one bounded budget,
and one request epoch. They receive either one project lease or one bounded
project diagnostic. Manifest selection, YooAsset operations/handles, reference
counts, epoch filtering, URL construction, and SDK exception translation stay
inside the implementation.

## 2. Scope and non-goals

RES-01B creates the project-owned asset-access interface, one YooAsset adapter,
and focused EditMode tests. It does not connect that interface to Host or
Presentation and does not create real content mappings, a bootstrap scene,
ReleaseSet persistence/activation, download UI/policy, R2 configuration,
Android behavior, or Windows evidence.

This packet makes no change to Core, Application, current content/save/publication
logic, serialization, public business interfaces, uGUI, localization, Android
API 24, graphics, Packages, or ProjectSettings.

Passing RES-01B does not authorize RES-01C or RES-01D. It only makes them
eligible for separate packets after the engineering lead checks their other
dependencies and disjoint write scopes.

## 3. Mandatory two-stage authorization and bindings

This packet requires two separate, recorded authorizations to the same owner C:

1. **Source-creation dispatch** authorizes only the three directories and eight
   content files in section 4. C performs no Unity action, creates no `.meta`,
   completes static checks, seals a source checkpoint, and stops.
2. **Verification appendix** is issued only after the engineering lead inspects
   that checkpoint. It freezes the actual asmdef/friend/source hashes and alone
   authorizes Unity import/meta generation, compile, focused tests, and full
   regression. It authorizes no source edit.

Every token below is `UNBOUND` in this prepared template. Initial-source tokens
must be bound before stage 1; checkpoint/Unity/test tokens must be bound in the
verification appendix before stage 2. A required token remaining in either
stage means `BLOCKED`; C must not guess it or write/run ahead.

### 3.1 Authority, owner, and RES-01A inputs

| Token | Required binding |
|---|---|
| `[SOURCE_CREATION_AUTHORITY_RECEIPT]` | User authority and engineering-lead source-creation dispatch for this exact packet identity |
| `[C_SOURCE_CREATION_TURN_ID]` | Actual C turn receiving the source-only dispatch |
| `[VERIFICATION_APPENDIX_AUTHORITY_RECEIPT]` | Later engineering-lead appendix approving the inspected checkpoint for Unity verification |
| `[C_VERIFICATION_TURN_ID]` | Actual C turn receiving the verification appendix |
| `[REVIEW_THREAD_ID]` | Independent testing/unit-review thread identity |
| `[REVIEW_HOST_ID]` | Independent reviewer host identity |
| `[RES01A_INDEPENDENT_VERDICT_RECEIPT]` | Independent `ACCEPT` receipt, not C's self-report |
| `[RES01A_ACCEPTED_GRAPH_RECEIPT]` | Sealed accepted package graph and two-file diff receipt |
| `[RES01A_ACCEPTED_GRAPH_SHA256]` | SHA-256 of that receipt |
| `[RES01A_MANIFEST_SHA256]` | Accepted `Packages/manifest.json` SHA-256 |
| `[RES01A_LOCK_SHA256]` | Accepted `Packages/packages-lock.json` SHA-256 |
| `[RES01A_YOOASSET_PACKAGE_SOURCE_SHA256]` | Resolved YooAsset package-source identity from RES-01A evidence |
| `[RES01A_YOOASSET_RUNTIME_ASSEMBLY_NAME]` | Actual runtime assembly name imported by RES-01A |
| `[RES01A_YOOASSET_RUNTIME_ASMDEF_SHA256]` | Actual resolved YooAsset runtime `.asmdef` SHA-256 |

### 3.2 Worktree, candidate-source, and Unity inputs

| Token | Required binding |
|---|---|
| `[PROJECT_ROOT]` | Canonical FightMatch project path |
| `[SOURCE_C_SERIAL_SLOT_RECEIPT]` | Stage-1 proof C owns the free serial slot and no conflicting task is using it |
| `[SOURCE_ACTIVE_PROCESS_GATE_RECEIPT]` | Stage-1 timestamped proof this project is not already open in Unity |
| `[PREDECESSOR_SOURCE_TREE_RECEIPT]` | Exact existing `Assets/Scripts/FightMatch/**` and `Assets/Tests/EditMode/**` byte/status inventory after the preceding accepted work |
| `[PREDECESSOR_SOURCE_TREE_SHA256]` | SHA-256 of that inventory receipt |
| `[CREATE_ONCE_EVIDENCE_ROOT]` | New ignored path exactly `[PROJECT_ROOT]/TestArtifacts/FightMatch/RES-01B/[BOUND_INPUT_IDENTITY]/[RUN_ID]/` |
| `[BOUND_INPUT_IDENTITY]` | Canonical SHA-256 over the accepted A graph receipt and predecessor source-tree receipt; it does not depend on not-yet-created candidate bytes |
| `[RUN_ID]` | Unique run id; the evidence directory must not pre-exist |
| `[SOURCE_CHECKPOINT_RECEIPT]` | C's sealed stage-1 receipt containing all eight content paths, hashes, static results, and line counts |
| `[SOURCE_CHECKPOINT_SHA256]` | SHA-256 of the sealed checkpoint reviewed by the engineering lead |
| `[CANDIDATE_ASSETACCESS_ASMDEF_SHA256]` | Checkpoint SHA-256 of `FightMatch.AssetAccess.asmdef`, frozen by the appendix |
| `[CANDIDATE_ADAPTER_ASMDEF_SHA256]` | Checkpoint SHA-256 of `FightMatch.YooAssetAdapter.asmdef`, frozen by the appendix |
| `[CANDIDATE_TEST_ASMDEF_SHA256]` | Checkpoint SHA-256 of `FightMatch.AssetAccess.Tests.asmdef`, frozen by the appendix |
| `[CANDIDATE_FRIEND_DECLARATION_SHA256]` | Checkpoint SHA-256 of the single adapter-to-test `InternalsVisibleTo` declaration, frozen by the appendix |
| `[CANDIDATE_PRODUCTION_SOURCE_SET_SHA256]` | Checkpoint canonical path+SHA receipt for the four production `.cs` files |
| `[CANDIDATE_TEST_SOURCE_SET_SHA256]` | Checkpoint canonical path+SHA receipt for the test `.cs` file |
| `[UNITY_EDITOR_PATH]` | Appendix-bound canonical Unity `2022.3.18f1` Mac Intel executable |
| `[UNITY_EDITOR_IDENTITY]` | Appendix-bound observed version, architecture, executable SHA-256, and install receipt |
| `[VERIFY_C_SERIAL_SLOT_RECEIPT]` | Fresh stage-2 proof C still owns/reacquired the free Unity slot |
| `[VERIFY_ACTIVE_PROCESS_GATE_RECEIPT]` | Fresh stage-2 timestamped proof this project is not already open in Unity |

Candidate hashes do not exist at initial dispatch and are never fabricated.
They bind the actual stage-1 checkpoint inspected by the engineering lead. If
any content byte changes afterward, the appendix is void; C stops and the
engineering lead must issue a smaller source-correction authorization followed
by a new checkpoint and appendix before any Unity action.

### 3.3 Verification-appendix validation inputs

| Token | Required binding |
|---|---|
| `[COMPILE_ARGV]` | Exact one-run Unity compile/import argv and log paths |
| `[FOCUSED_TEST_ARGV]` | Exact EditMode argv selecting only `FightMatch.AssetAccess.Tests` |
| `[FOCUSED_TEST_FILTER]` | Actual assembly/filter expression used by the bound command |
| `[FOCUSED_EXPECTED_COUNT]` | Exact NUnit case count computed from the reviewed checkpoint before stage 2; runner discovery must match |
| `[BASELINE_SOURCE_SHA256]` | Source identity for the latest accepted existing EditMode baseline |
| `[BASELINE_FILTER]` | Exact filter or explicit unfiltered identity for that baseline |
| `[BASELINE_TOTAL_COUNT]` | Latest accepted baseline total test count |
| `[BASELINE_PASS_COUNT]` | Latest accepted baseline pass count |
| `[FULL_TEST_ARGV]` | Exact one-run unfiltered EditMode regression argv and log/XML paths |
| `[FULL_EXPECTED_TOTAL_COUNT]` | Exact expected baseline plus newly discovered RES-01B cases, adjusted only by an evidenced discovery delta |
| `[COMPILE_TIMEOUT]` | Fixed compile timeout |
| `[FOCUSED_TEST_TIMEOUT]` | Fixed focused-test timeout |
| `[FULL_TEST_TIMEOUT]` | Fixed full-suite timeout |

The historical 4181/4181 result in `START_HERE.md` is context only. It is not
silently bound as the current baseline after intervening uGUI or package work.

## 4. Exact final whitelist and path matrix

The following paths are the complete RES-01B product whitelist. All were
observed absent during preparation on 2026-10-01; dispatch must recheck absence
and symbol/assembly-name conflicts at the source-creation dispatch after RES-01A
and the preceding C work. At verification dispatch the eight content files must
instead exist with the sealed checkpoint identities, while all 11 `.meta` paths
must still be absent.

| Kind | New path | Required paired metadata / purpose |
|---|---|---|
| Directory | `Assets/Scripts/FightMatch/AssetAccess/` | `Assets/Scripts/FightMatch/AssetAccess.meta`; project-owned interface only |
| Assembly | `Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef` | same path plus `.meta`; `FightMatch.AssetAccess`, no engine references, no package reference |
| Production C# | `Assets/Scripts/FightMatch/AssetAccess/FightMatchAssetContracts.cs` | same path plus `.meta`; complete public interface and value/result/diagnostic types |
| Directory | `Assets/Scripts/FightMatch/YooAssetAdapter/` | `Assets/Scripts/FightMatch/YooAssetAdapter.meta`; third-party adapter implementation only |
| Assembly | `Assets/Scripts/FightMatch/YooAssetAdapter/FightMatch.YooAssetAdapter.asmdef` | same path plus `.meta`; references only `FightMatch.AssetAccess` and the RES-01A runtime assembly |
| Production C# | `Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetAssetProvider.cs` | same path plus `.meta`; provider, immutable mapping, lease/refcount/epoch/result translation, internal fake seam |
| Production C# | `Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetPackageLifecycle.cs` | same path plus `.meta`; exact package/manifest lifecycle and real YooAsset runtime adapter |
| Production C# | `Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetRemoteServices.cs` | same path plus `.meta`; bounded HTTPS URL construction and resolved YooAsset remote interface |
| Directory | `Assets/Tests/EditMode/FightMatchAsset/` | `Assets/Tests/EditMode/FightMatchAsset.meta`; focused tests only |
| Assembly | `Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef` | same path plus `.meta`; Editor TestAssemblies reference to AssetAccess and adapter |
| Test C# | `Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetProviderTests.cs` | same path plus `.meta`; all fake, adapter, quarantine, lease, epoch, budget, and URL cases |

Final count is fixed at:

- 4 production `.cs` files;
- 1 test `.cs` file;
- 3 `.asmdef` files;
- 3 new directories;
- 11 exact `.meta` files: one for every directory and every content file.

Unused headroom under the design ceiling is not authority to add files. The
hard ceiling remains at most 6 production C# / 1,000 physical lines and 2 test
C# / 900 physical lines; this packet's exact whitelist is narrower.

The single Unity owner lets Unity generate every new `.meta` naturally. C must
record each GUID/path/importer/SHA and scan all `Assets/**/*.meta` GUIDs for
duplicates. Do not hand-write GUIDs, copy a prior `.meta`, or omit a directory
`.meta`.

## 5. Forbidden scope

Do not modify any existing file. In particular, do not change:

- `Assets/Scripts/FightMatch/Core/**`, `Application/**`, `Content/**`,
  `Platform/**`, `Input/**`, `Presentation/**`, or `Host/**`;
- any existing test, assembly definition, scene, prefab, asset, `.meta`, save
  codec, publication/catalog type, localization binding, or public business
  interface;
- `Packages/**`, `ProjectSettings/**`, CI/build scripts, dependencies, generated
  project files, or user-managed permission files.

Do not create Host composition, a bootstrap/factory used by Host, a ReleaseSet
DTO/codec/store, R2 domains/credentials, actual asset-address mappings, resource
build configuration, bundles, downloadable content, PlayMode/local-HTTP tests,
Android behavior, or Windows evidence.

Do not refactor, reformat, rename, reorder, or comment-rewrite existing code.
Do not expose YooAsset types, handles, package objects, manifest objects,
locations, callbacks, or URLs through the project-owned interface.

## 6. Frozen project-owned interface

Namespace `FightMatch.AssetAccess` is the only external seam. Its public type
surface is fixed to the following declaration inventory and no others. The
notation intentionally omits method bodies; names, types, generic constraints,
parameter order, and members are exact:

```text
public sealed class FightMatchAssetId : IEquatable<FightMatchAssetId>
{
    public string Value { get; }
    public static bool TryCreate(string value, out FightMatchAssetId assetId);
}

public sealed class AssetAcquireBudget
{
    public AssetAcquireBudget(
        long maxRawBytes,
        int maxConcurrentAcquisitions,
        int maxQueuedCallbacks,
        int maxRetainedLeases);
    public long MaxRawBytes { get; }
    public int MaxConcurrentAcquisitions { get; }
    public int MaxQueuedCallbacks { get; }
    public int MaxRetainedLeases { get; }
}

public enum FightMatchAssetDiagnosticCode
{
    InvalidAssetId,
    UnknownAsset,
    WrongAssetType,
    WrongReleaseSet,
    BudgetExceeded,
    PackageUnavailable,
    ManifestUnavailable,
    LocationUnavailable,
    SdkFailure,
    StaleEpoch,
    InvalidRemoteUrl,
    ReleasedLease
}

public enum FightMatchAssetDiagnosticStage
{
    ValidateRequest,
    InitializePackage,
    SelectManifest,
    ResolveLocation,
    Acquire,
    ValidateResult,
    Release,
    BuildRemoteUrl
}

public sealed class FightMatchAssetDiagnostic
{
    public FightMatchAssetDiagnostic(
        FightMatchAssetDiagnosticCode code,
        FightMatchAssetDiagnosticStage stage,
        FightMatchAssetId assetId,
        string releaseSetId,
        bool retryable,
        string safeDetail);
    public FightMatchAssetDiagnosticCode Code { get; }
    public FightMatchAssetDiagnosticStage Stage { get; }
    public FightMatchAssetId AssetId { get; }
    public string ReleaseSetId { get; }
    public bool Retryable { get; }
    public string SafeDetail { get; }
}

public interface IFightMatchAssetLease<out T> : IDisposable where T : class
{
    FightMatchAssetId AssetId { get; }
    string ReleaseSetId { get; }
    string LeaseId { get; }
    T Asset { get; }
    bool IsReleased { get; }
}

public sealed class FightMatchAssetAcquireResult<T> where T : class
{
    public static FightMatchAssetAcquireResult<T> Accepted(
        IFightMatchAssetLease<T> lease,
        long requestEpoch,
        string releaseSetId);
    public static FightMatchAssetAcquireResult<T> Rejected(
        FightMatchAssetDiagnostic diagnostic,
        long requestEpoch,
        string releaseSetId);
    public bool IsAccepted { get; }
    public IFightMatchAssetLease<T> Lease { get; }
    public FightMatchAssetDiagnostic Diagnostic { get; }
    public long RequestEpoch { get; }
    public string ReleaseSetId { get; }
}

public interface IFightMatchAssetProvider
{
    Task<FightMatchAssetAcquireResult<T>> AcquireAsync<T>(
        FightMatchAssetId assetId,
        string releaseSetId,
        AssetAcquireBudget budget,
        long requestEpoch)
        where T : class;
}
```

The shown constructor/factory/member surface is exhaustive. Implementation may
override object equality/hash/text members for `FightMatchAssetId`, but it must
not add another public domain operation or type. No public member may mention
YooAsset.

### 6.1 Exact bounded value contracts

All validation and equality are ordinal over ASCII code units. Nothing is
Unicode-normalized, culture-folded, trimmed, truncated, or hashed into an
accepted identity.

| Value | Exact accepted form | Failure behavior |
|---|---|---|
| `FightMatchAssetId.Value` | 1..128 ASCII characters; first character `[a-z0-9]`; remaining characters `[a-z0-9._-]` | `TryCreate` returns `false`, sets `out assetId` to null, and retains/echoes none of the rejected raw text |
| Valid `releaseSetId` | 1..128 ASCII characters with the same grammar; ordinal case-sensitive; exact token `latest` is forbidden | Request rejects before SDK work; result and diagnostic `ReleaseSetId` are null and safe detail is exactly `reason=invalid-release-set` |
| `LeaseId` | Exactly 32 lowercase hexadecimal characters `[0-9a-f]{32}`, generated from a new GUID in `N` form; never caller supplied | Failure to create a conforming unique id rejects before a lease is published and releases the acquired handle |
| `SafeDetail` | Null becomes the empty string; otherwise 0..239 ASCII characters in the canonical grammar below | The complete non-null invalid value becomes exactly `reason=redacted`; no prefix, suffix, truncation, or hash of unsafe input is retained |
| `requestEpoch` | Integer 1 through `long.MaxValue` | Zero/negative values reject before SDK work as `StaleEpoch` at `ValidateRequest`; no raw caller text exists to echo |

The canonical `SafeDetail` grammar is zero fields (empty string), or
semicolon-separated `key=value` fields sorted by key with no duplicate key.
Allowed keys and values are exactly:

- `status`: one of `failed`, `pending`, `cancelled`, `timeout`;
- `reason`: one of `invalid-asset-id`, `unknown-asset`,
  `wrong-asset-type`, `invalid-release-set`, `wrong-release-set`,
  `budget-exceeded`, `package-unavailable`, `manifest-unavailable`,
  `location-unavailable`, `sdk-failure`, `stale-epoch`,
  `invalid-remote-url`, `released-lease`, `redacted`;
- `host`: one of `primary`, `fallback`, `none`;
- `http`: three decimal digits representing 100..599;
- `count`: `0` or an unsigned canonical decimal with at most 19 digits;
- `manifest` or `sha256`: exactly 64 lowercase hexadecimal characters.

Any unknown key, whitespace, control/non-ASCII character, slash, backslash,
colon, at-sign, percent sign, query/fragment character, repeated key, unsorted
key, invalid value, or total length above 239 replaces the whole detail with
`reason=redacted`. Adapter code constructs details only from enumerated status,
reason, and host categories; validated numeric codes/counts; and validated
hashes. It never feeds caller strings, URLs, local paths, SDK messages, exception
messages, stack traces, tokens, credentials, or device identifiers into this
formatter.

`AssetAcquireBudget` accepts only these inclusive constructor ranges:

| Field | Minimum | Hard maximum |
|---|---:|---:|
| `MaxRawBytes` | 1 | 67,108,864 bytes |
| `MaxConcurrentAcquisitions` | 1 | 32 |
| `MaxQueuedCallbacks` | 1 | 1,024 |
| `MaxRetainedLeases` | 1 | 4,096 |

Construction outside a range throws `ArgumentOutOfRangeException` before an
adapter exists. In RES-01B there is no second configurable ceiling: the adapter
enforces the caller's validated budget and these absolute maxima. These are
safety bounds, not measured capacity or release-performance promises.

### 6.2 Nullability and result invariants

Request validation order is fixed: asset id, release-set syntax, non-null
budget, positive epoch, immutable mapping/type, configured limits, then SDK
lifecycle. A failed step prevents every later step and every SDK call. Null
budget maps to `BudgetExceeded` at `ValidateRequest` after the two identifiers
have passed.

1. `FightMatchAssetDiagnostic.AssetId` is null **only** when `Code` is
   `InvalidAssetId`. It is non-null for every other code. No sentinel or
   fabricated legal asset id represents an invalid request.
2. A failed `FightMatchAssetId.TryCreate(raw, out assetId)` leaves `assetId`
   null. If a caller then passes null to `AcquireAsync`, the provider returns
   `InvalidAssetId` at `ValidateRequest`, with null `AssetId`, without receiving
   or reconstructing the raw rejected text and without starting the SDK.
3. `FightMatchAssetDiagnostic.ReleaseSetId` and
   `FightMatchAssetAcquireResult<T>.ReleaseSetId` are null only when asset-id
   validation failed before set validation, or for a missing/syntactically
   invalid release-set request. For a syntactically valid but wrong set they
   contain that bounded validated id. They never contain the raw invalid input.
   The two properties must agree on every rejected result.
4. The public diagnostic constructor enforces the null rules above and
   canonicalizes `SafeDetail` with §6.1. The public result factories enforce
   asset/release/epoch consistency. `Accepted` rejects a null/invalid set.
   `Rejected` validates its set argument ordinally: a null or invalid raw value
   is discarded to null and is permitted only with `InvalidAssetId` or
   `WrongReleaseSet` plus a diagnostic null set; a valid value must equal the
   diagnostic set ordinally. Other invalid object combinations throw before a
   result exists.
5. Exactly one of `Lease` and `Diagnostic` is non-null. `IsAccepted` is derived
   from that invariant, not independently mutable.
6. Accepted results always have a valid non-null asset id/release set, echo the
   exact positive request epoch, and own one unreleased lease. Rejected results
   contain no usable asset or lease.
7. The lease exposes no YooAsset handle. `Dispose` is idempotent. After release,
   `Asset` is no longer usable; development/test records one bounded
   `ReleasedLease` diagnostic through the adapter's internal diagnostic sink
   without touching the released SDK handle.
8. There is no `latest` release-set lookup, mutable current-set lookup,
   cancellation/progress/download-consent interface, or business binding/save
   mutation in this module.

## 7. Adapter implementation and assembly topology

### 7.1 Assembly definitions

`FightMatch.AssetAccess.asmdef` must declare:

- `name` and `rootNamespace`: `FightMatch.AssetAccess`;
- no assembly references;
- `noEngineReferences: true`;
- ordinary runtime platforms, no unsafe code, and no define/version constraints.

`FightMatch.YooAssetAdapter.asmdef` must declare:

- `name` and `rootNamespace`: `FightMatch.YooAssetAdapter`;
- references exactly `FightMatch.AssetAccess` and
  `[RES01A_YOOASSET_RUNTIME_ASSEMBLY_NAME]`;
- no Core/Application/Content/Save/Presentation/Host/test reference;
- ordinary runtime platforms, engine references enabled, no unsafe code, and
  no define/version constraints.

`FightMatch.AssetAccess.Tests.asmdef` must declare:

- `name` and `rootNamespace`: `FightMatch.AssetAccess.Tests`;
- Editor-only, `TestAssemblies`, engine references enabled;
- references exactly `FightMatch.AssetAccess` and
  `FightMatch.YooAssetAdapter`; it may add the actual YooAsset runtime assembly
  only if candidate test source directly compiles against it and the reason is
  recorded before dispatch;
- no Core/Application/Content/Save/Presentation/Host reference.

The only new friend declaration is:

```csharp
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.AssetAccess.Tests")]
```

It belongs to `FightMatch.YooAssetAdapter`, is bound by
`[CANDIDATE_FRIEND_DECLARATION_SHA256]`, and exposes only internal adapter seams
to the focused test assembly. AssetAccess does not friend the adapter or tests.

### 7.2 Internal seams

The adapter may define a small internal runtime interface and immutable internal
records inside the three exact adapter files. This is a real internal seam with
two adapters: the production YooAsset runtime adapter and the focused-test fake.
It must use project-owned operation/result data and must not pass YooAsset types
into the test fake.

The external `IFightMatchAssetProvider` seam likewise has two exercised
adapters: `YooAssetAssetProvider` and a nested in-test fake provider. The fake is
test code in the one whitelisted test file, not a production implementation or
an additional file.

`YooAssetAssetProvider` owns:

- immutable logical-id mapping to exact package/location/type/release-set
  expectations supplied only by tests in B; no real game mapping is added;
- request validation, result/diagnostic translation, global hard-limit checks,
  epoch/provider-instance checks, lease ids, refcounts, and stale completion;
- one underlying provider/handle shared across live leases for the same exact
  asset identity; each acquisition returns a distinct project lease;
- immediate underlying-handle release for a stale success and exactly-once
  release on the last live project lease.

`YooAssetPackageLifecycle` owns only the real SDK-facing package
initialization/exact-manifest/acquisition implementation required by the
internal runtime interface. It never selects latest, mutates a business
binding, or starts download policy.

`YooAssetRemoteServices` implements the actual RES-01A-resolved YooAsset remote
interface signature. It accepts only prevalidated HTTPS base hosts plus fixed
platform/release/package identities and an immutable relative filename. It
rejects absolute input, traversal, query/fragment injection, non-HTTPS/unknown
identity, and duplicate-primary-as-fallback. It contains no credential and no
project-specific production domain.

Adapter implementation types and constructors remain internal in RES-01B.
Tests create them through the single friend declaration. Host composition is a
later explicit packet, not a reason to expose a second public interface here.

### 7.3 Lease, epoch, and diagnostic rules

- Two live project leases for one exact asset may share one SDK handle but have
  different non-secret lease ids.
- First release leaves the other lease and handle valid; final release disposes
  the handle exactly once. Double release changes no count and releases nothing
  again.
- Provider instance, request epoch, release set, asset id, and operation id must
  all match before a completion may publish. A stale success releases only its
  own old handle immediately; it cannot release or replace a newer handle.
- Wrong/missing/`latest`/partially prepared release sets fail before acquisition.
- Concurrent acquisitions, queued callbacks, raw bytes, and retained leases
  are bounded before allocation/traversal where possible. `BudgetExceeded`
  produces no usable lease and leaks no handle.
- All SDK errors/exceptions become bounded project diagnostics. No SDK exception
  or callback escapes across `IFightMatchAssetProvider`.

## 8. Focused test matrix

All cases live in the one whitelisted test file and exercise the external
interface. Internal fake controls are used only to deterministically complete
or fail SDK-shaped operations; assertions remain on observable interface
results, handle release counts, and safe diagnostics.

| Case | Required setup and observable pass condition | Design evidence |
|---|---|---|
| B-00 Interface fake | A nested fake implements only `IFightMatchAssetProvider`, constructs accepted/rejected project results without YooAsset, and proves callers need no adapter/SDK type. | §10 deep seam |
| B-01 Result invariant | Fake success returns accepted result with one live lease and echoed epoch/set; fake failure returns diagnostic only. Impossible lease+diagnostic/neither states cannot be constructed. | YA-03 |
| B-02 Invalid/unknown id | Null id, failed-id parse, unknown logical id, and path/URL-shaped id are rejected before SDK call. Invalid parse returns null, diagnostic `AssetId` is null only for `InvalidAssetId`, and no raw input is retained. | YA-03 |
| B-03 Wrong type | Mapping expects one type and caller requests another; result is `WrongAssetType`, no SDK call/lease. | YA-03 |
| B-04 Wrong set | Null, blank, invalid grammar, `latest`, mismatched, and partially prepared set each reject before acquire; invalid raw set is discarded and represented by null, valid mismatch may echo only the bounded validated id, and exact set proceeds. | YA-03, §10 |
| B-05 SDK mapping | SDK failure, callback exception, null asset, and package/manifest/location failure map to distinct bounded project diagnostics; injected raw exception, secret, URL, and local-path markers appear nowhere in result/diagnostic text. | YA-03 |
| B-06 Multi-lease | Two acquisitions of the same exact asset return distinct lease ids, share one underlying handle/refcount, and expose the same valid asset. | YA-04 |
| B-07 First release | Disposing lease A leaves lease B usable and does not dispose the underlying handle. | YA-04 |
| B-08 Idempotent release | Disposing the same lease twice is harmless, does not decrement twice, and records at most one bounded released-lease diagnostic. | YA-04 |
| B-09 Last release | Releasing the final live lease disposes the underlying handle exactly once; later access does not touch it. | YA-04 |
| B-10 Stale epoch | Complete epoch N after cancel/reinitialize/retry to N+1; N cannot publish and releases its own handle, while N+1 and its handle remain untouched. | YA-05 |
| B-11 Wrong provider/set completion | Completion from a prior provider instance, operation, asset, or release set is stale and cannot publish. | YA-05 |
| B-12 Limits | Each acquisition/callback/raw-byte/retained-lease configured ceiling is hit independently; result is `BudgetExceeded`, with no extra allocation, SDK start, or leaked handle. Constructor minimum and hard maximum boundaries are verified for all four budget fields. | §16 |
| B-13 URL acceptance | Approved HTTPS base plus one safe immutable relative filename produces the exact bounded primary URL; distinct approved fallback is ordered only when present. | RES-01B URL gate |
| B-14 URL rejection | Absolute filename, `..`, query, fragment, control text, non-HTTPS base, unknown platform/release/package, and duplicate fallback all reject without returning a URL or leaking input. | RES-01B URL gate |
| B-15 Assembly quarantine | Assembly-reference graph and source scan find YooAsset references only inside `FightMatch.YooAssetAdapter` and, if required, its focused tests; none appear in AssetAccess, Core, Application, save/publication/business, Presentation, or Host. | YA-02 |
| B-16 No business side effects | All success/failure/release paths leave current `ContentBinding`, publication files, saves, and existing business modules untouched. | §10, YA-16 |
| B-17 Exact boundary grid | Asset id and release-set lengths 0/1 and 127/128/129, SafeDetail lengths 0/1 and 238/239/240, and lease-id lengths 31/32/33 exercise exact minimum/fixed/maximum boundaries. Budget fields exercise 0/1 and hard-max-1/hard-max/hard-max+1; epoch exercises 0/1/`long.MaxValue`. Only the exact accepted boundaries pass. | §6.1 |
| B-18 Leakage negatives | Raw inputs containing `https://user:token@host/path?sig=secret`, `/Users/example/private`, `C:\secret`, control/newline text, and an SDK exception marker are rejected or wholly replaced by `reason=redacted`; none appears in `Value`, `ReleaseSetId`, `SafeDetail`, lease id, result text, or diagnostic text. | §6.1–6.2 |

`[FOCUSED_EXPECTED_COUNT]` binds the actual NUnit-discovered case count,
including parameterized cases; the table's 19 logical rows must not be mistaken
for the runner's discovered count.

## 9. Ordered implementation and verification

### Phase A — source-creation dispatch and checkpoint

Only `[SOURCE_CREATION_AUTHORITY_RECEIPT]` and
`[C_SOURCE_CREATION_TURN_ID]` authorize this phase.

1. Verify the design identity and independent RES-01A acceptance/graph receipts.
2. Confirm all whitelist paths remain absent and no symbol/assembly-name
   conflicts exist.
3. Confirm C owns the free serial slot and the project is not open. If open,
   return `BLOCKED`; do not kill the process. Source creation while an Editor is
   open is forbidden because it could trigger unauthorized import/meta writes.
4. Freeze existing source/test bytes and read-only status, including predecessor
   WIP identities; create the exact ignored create-once evidence root.
5. Create only the three directories and eight content files with the normal
   patch tool. Do not start Unity and do not create any `.meta`.
6. Run static-only checks: exact path set, public-interface inventory and bounds,
   asmdef JSON/reference intent, single friend declaration, YooAsset quarantine,
   physical-line budgets, no existing-file change, and no unexpected `.meta`.
7. Seal `[SOURCE_CHECKPOINT_RECEIPT]` with individual file hashes and the six
   canonical candidate receipts, then stop with
   `SOURCE_CHECKPOINT_AWAITING_VERIFICATION_AUTHORITY`. Return it to the
   engineering lead. C may not continue in the same authority turn.

### Phase B — engineering verification appendix gate

The engineering lead inspects the checkpoint, exact source/asmdef/friend bytes,
static results, scope, and budgets. If acceptable, the lead binds every
checkpoint, Unity, command, filter/count, baseline, and timeout token in a
separate `[VERIFICATION_APPENDIX_AUTHORITY_RECEIPT]` and sends it to C in
`[C_VERIFICATION_TURN_ID]`.

Before any Unity action, C must prove the eight content files still hash exactly
to the appendix. Any byte change, missing file, unexpected `.meta`, changed A
graph, occupied slot, or open project returns `BLOCKED_APPENDIX_MISMATCH`.
Stage-2 authority permits no source/asmdef edit. A needed edit requires a
smaller source-correction authorization, a new checkpoint, engineering review,
and a replacement appendix.

The stage-2 product write whitelist is exactly the 11 Unity-generated `.meta`
paths in section 4. The eight content files are immutable inputs; every existing
file remains read-only. Unity-generated ignored cache state and append-only
evidence do not expand the accepted product diff.

### Phase C — Unity import and compile

1. Start the fixed Unity Editor once for import/compile using `[COMPILE_ARGV]`.
   Let the sole Unity owner generate all 11 `.meta` files naturally.
2. Capture exit code, complete Editor log, compiler diagnostics, imported
   assembly graph, every new path/GUID/importer/SHA, physical line counts, and
   duplicate-GUID scan.
3. Require no compile/import/package error, no source-byte drift, and no
   unexpected file write.

### Phase D — focused EditMode verification

Run `[FOCUSED_TEST_ARGV]` once. Pass requires:

- exact candidate source and assembly identities;
- exact `[FOCUSED_TEST_FILTER]` and `[FOCUSED_EXPECTED_COUNT]` discovered/executed;
- every focused case passes with zero ignored/inconclusive cases;
- YA-02 quarantine scan and every B-00 through B-18 logical row are evidenced;
- result XML, log, argv, time, exit code, Unity/package identity, and SHA receipt
  agree.

### Phase E — one full regression

After the focused pass, run `[FULL_TEST_ARGV]` once under the accepted EditMode
protocol. Pass requires exit `0`, zero failures/errors/skips/inconclusive, exact
`[FULL_EXPECTED_TOTAL_COUNT]`, and source/log/XML identity agreement. Existing
accepted tests must not be deleted, renamed, filtered out, or weakened to make
the count pass.

Do not run PlayMode, Player/Android builds, local HTTP, device, CDN, Windows, or
release tests. Those remain `NOT RUN`.

### Phase F — return and independent review

C returns the exact changed-file list, file/meta identities, assembly/friend
graph, public interface inventory, focused/full counts, line-budget receipt,
scope scan, and sealed evidence root to the engineering lead. Only then may the
engineering lead forward the delivery to the bound independent reviewer.

## 10. Create-once evidence closure

`[CREATE_ONCE_EVIDENCE_ROOT]` is under the repository-ignored
`/TestArtifacts/` root but is not part of the product diff. It must be new,
never overwritten, and contain a sealed manifest for exactly:

- source-creation authority/turn, verification-appendix authority/turn, design,
  owner, A-verdict, accepted-graph, Unity, process, predecessor, timeout, and
  argv receipts;
- source-checkpoint manifest, eight individual file hashes, six canonical
  asmdef/friend/source receipts, static-check output, and appendix review receipt;
- before/final source-tree inventories and read-only status/diff;
- candidate patch and exact final diff;
- compile stdout/stderr/Editor log and compile result;
- focused test stdout/stderr/Editor log/XML and result/count receipt;
- full test stdout/stderr/Editor log/XML and result/count receipt;
- assembly/reference/friend graph and YooAsset quarantine scan;
- path/GUID/importer/meta/SHA inventory and duplicate-GUID scan;
- production/test physical-line counts and budget verdict;
- public-interface inventory and compatibility/no-existing-file-change receipt;
- final scope receipt and C delivery receipt;
- conditional rollback inventory/compile log/test receipt when rollback occurs.

Every evidence item records its SHA-256 and byte count. Logs are never reused
between phases. Missing, overwritten, mismatched, or post-edited evidence is a
failed gate, not permission to rerun without a corrective packet.

## 11. Acceptance criteria

Independent review may issue `ACCEPT` only when all are proven:

1. RES-01A has an independent accepted immutable package graph and the actual
   YooAsset runtime assembly/source identities match every B asmdef and log.
2. The final diff contains exactly the 8 new content files and their 11
   Unity-generated `.meta` files at the section-4 paths; no existing file changed.
3. The public interface and invariants match section 6, and no public member or
   project-owned diagnostic exposes a YooAsset type or unsafe detail.
   `Diagnostic.AssetId` is null only for `InvalidAssetId`; invalid release-set
   input is represented by null rather than echoed raw text.
4. The assembly graph matches section 7. YooAsset appears only in the adapter
   and, only if justified, its focused tests. Core/Application/save/business/
   Presentation/Host remain byte-for-byte unchanged and free of YooAsset.
5. Result mapping, multi-lease, first/final/idempotent release, stale epoch,
   wrong set/type/id, all four budget limit classes, every §6.1
   limit-1/limit/limit+1 boundary, leakage negatives, and URL accept/reject
   cases pass.
6. Every stale or failed completion leaves no usable lease/asset and leaks no
   handle; final live release disposes exactly once.
7. Production code is at most 1,000 physical lines across 4 files; test code is
   at most 900 physical lines in 1 file. No unused budget expands scope.
8. Fresh compile/import, focused tests, and one full EditMode regression all
   pass with exact identities/counts and complete create-once evidence.
9. No Host/bootstrap/ReleaseSet/R2/download-policy/Android/Windows/release claim
   is presented as implemented or verified.
10. Source creation and Unity verification have distinct authority receipts and
    C turn ids; checkpoint bytes did not change between them.

## 12. Failure, rollback, and stop conditions

Before stage-1 write, return `BLOCKED` for an unbound stage-1 token, missing
independent A acceptance, changed A graph, occupied C/Unity slot, open project,
or a non-absent/conflicting target path. Stage 1 may not require or invent
candidate hashes before creating the authorized files.

After the source checkpoint, stop until the separate appendix. Before stage-2
Unity action, return `BLOCKED_APPENDIX_MISMATCH` for a missing appendix/turn,
candidate hash drift, unexpected `.meta`, changed A graph, occupied/open Unity,
unavailable fixed Editor, or invalid command/baseline/filter/count contract.

After stage-2 starts, stop expansion for a scope violation, compile/import error,
unexpected assembly/package reference, failed focused/full test, count mismatch,
budget overrun, GUID collision, unsafe diagnostic, handle leak, or evidence
corruption. Do not add helper files, relax an interface invariant, modify an
existing module, or absorb C/D scope to repair B.

Rollback removes only the section-4 new files/metas and newly empty packet
directories, and only after a dependency audit proves no later packet or user
work depends on them. It never removes or changes the YooAsset package; package
rollback belongs exclusively to RES-01A. Preserve evidence and run only the
bound rollback compile/focused baseline check authorized by the signed packet.
If any later dependency exists, return `BLOCKED_ROLLBACK_DEPENDENCY` and delete
nothing.

## 13. Resource ceiling

| Resource | Ceiling |
|---|---|
| Unity owner | C only |
| Concurrent Unity processes for this project | 1 |
| New production C# | 4 exact files, no more than 1,000 physical lines total |
| New test C# | 1 exact file, no more than 900 physical lines total |
| New asmdef | 3 exact files |
| New meta | 11 exact Unity-generated files |
| Existing product/test file changes | 0 |
| Source-creation authority | 1 source-only C turn ending at sealed checkpoint |
| Source edits after checkpoint | 0 under the verification appendix |
| Verification authority | 1 later C turn bound to the unchanged checkpoint |
| Compile/import | 1 fresh run |
| Focused EditMode | 1 run after compile |
| Full EditMode | 1 unfiltered run after focused pass |
| Network/UPM/package mutation | 0 |
| PlayMode/Player/Android/Windows/CDN/release runs | 0 (`NOT RUN`) |

Timeout is a failure, not permission to retry. A corrective rerun requires a
smaller signed packet tied to the failed evidence.

## 14. Required C return format

```text
RES-01B result: SOURCE_CHECKPOINT_AWAITING_VERIFICATION_AUTHORITY | PASS | FAIL_ROLLED_BACK | BLOCKED | BLOCKED_APPENDIX_MISMATCH | BLOCKED_ROLLBACK_DEPENDENCY
Owner: C / 01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local / <source-turn-id; verification-turn-id-or-not-run>
Source authority: <source-creation receipt>
Verification authority: <appendix receipt or not-run>
Design identity: <sha256, lines, bytes>
RES-01A acceptance: <independent verdict sha; graph sha; manifest/lock sha>
Unity/process/serial gate: <identities and verdicts>
Predecessor source identity: <receipt sha>
Source checkpoint: <sealed receipt sha and 8 individual file shas>
Changed paths: <exact 8 files + 11 metas, or none after rollback>
Assemblies/friend: <names, references, asmdef/friend shas>
Public interface: <type/member inventory and receipt sha>
Source identities: <production/test path+sha receipts>
Line budgets: <production physical lines; test physical lines>
Compile: <exit; log/result sha>
Focused tests: <filter; expected/discovered/passed counts; XML/log sha>
Full EditMode: <baseline identity; expected/discovered/passed counts; XML/log sha>
Quarantine/scope: <receipt sha and verdict>
Rollback: <not applicable or dependency audit/deletion/verification result>
Evidence root: <path and sealed manifest sha>
NOT RUN: RES-01C/D, Host/bootstrap, ReleaseSet, R2/CDN, Android, Windows, release
Return to: engineering lead 01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local
```

## 15. Packet-author static self-check

- Current preparation creates only
  `docs/team/2026-09-30/engineering-yooasset-res-01b-implementation.md`.
- The authoritative design and RES-01A packet are read-only.
- All 3 target directories, 8 target content files, and 11 target `.meta` paths
  were observed absent on 2026-10-01; no conflicting type or assembly name was
  found. Dispatch must recheck after A and preceding C work.
- Current Packages do not yet contain YooAsset, so no A package/assembly/verdict
  identity is fabricated; all are visible dispatch blockers.
- Initial dispatch no longer depends on nonexistent candidate hashes: it
  authorizes source creation only, requires C to stop at a sealed checkpoint,
  and reserves every Unity/meta/compile/test action for a later reviewed
  verification appendix and separate C turn.
- The exact final whitelist contains only new AssetAccess/adapter/test files and
  metas; no existing source, package, setting, asset, or business interface may
  change.
- The deep module keeps a small project-owned interface while YooAsset temporal
  complexity stays in one adapter implementation. Its internal runtime seam has
  exactly two justified adapters: real YooAsset and the focused-test fake.
- Test rows cover mapping, multi-lease, idempotent/final release, stale epoch,
  wrong set/type/id, exact text/budget/lease/epoch boundaries, leakage
  negatives, limits, URL rejection, quarantine, and regression.
- Asset id, release-set id, SafeDetail, lease id, request epoch, and all budget
  fields have exact ordinal grammar/ranges and failure behavior. Invalid ids use
  null only in their declared diagnostic state; no legal sentinel or raw-input
  echo is permitted.
- RES-01B acceptance cannot authorize C/D, Host/bootstrap, ReleaseSet, R2,
  Android, Windows, or release compatibility.
