# RES-01 · YooAsset resource-system design

- Status: `DRAFT_READY_FOR_ENGINEERING_REVIEW`
- Delivery owner: engineering lead（主程）
- Design author task: system design / `01a0f2e6-29bb-7092-abf0-705b41b7bf93`
- Research date / source access date: **2026-10-01**
- Allowed sources: YooAsset official website/documentation, the official `tuyoogame/YooAsset` GitHub repository (releases, tags, and source), and Unity official Package Manager documentation.
- Repository action taken: research only. No package was installed, no Unity process was run, and no project/package/Git setting was changed.
- Scope: fixed/remote resource boundaries, YooAsset adapter seam, release-set activation, R2/Cloudflare delivery, download policy, leases, and separately reviewable implementation packets. This is a design and dispatch draft, not implementation authorization.

## 1. Decision snapshot

### Provisional recommendation

Use **YooAsset `3.0.6`**, pinned to the immutable commit
`3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`, as the version-probe candidate.

Exact candidate coordinates:

| Field | Exact value | Official evidence |
|---|---|---|
| Release/tag | `3.0.6` | [official release](https://github.com/tuyoogame/YooAsset/releases/tag/3.0.6), accessed 2026-10-01 |
| Commit | `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` | [official commit](https://github.com/tuyoogame/YooAsset/commit/3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804), accessed 2026-10-01 |
| UPM package id | `com.tuyoogame.yooasset` | [`Assets/YooAsset/package.json`, line 2](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json#L2) |
| Package version | `3.0.6` | [`package.json`, line 4](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json#L4) |
| Declared Unity floor | `2019.4` | [`package.json`, line 5](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json#L5) |
| Direct dependencies | `com.unity.scriptablebuildpipeline` `1.21.25`; `com.unity.modules.assetbundle` `1.0.0`; `com.unity.modules.unitywebrequest` `1.0.0`; `com.unity.modules.unitywebrequestassetbundle` `1.0.0` | [`package.json`, lines 43-46](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json#L43-L46) |
| License | Apache License 2.0 | [repository `LICENSE`](https://github.com/tuyoogame/YooAsset/blob/3.0.6/LICENSE), accessed 2026-10-01 |
| Immutable Git URL candidate | `https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` | Repository package location above plus Unity's official [Git URL and `?path=` syntax](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html), accessed 2026-10-01 |

`3.0.6` is the newest stable release found on the official releases page on the access date. It is a better probe target than `3.0.5` because the `3.0.6` release includes subsequent fixes, including a Linux case-sensitive path fix, while retaining the `3.x` API line. This is a research recommendation, not an adoption result.

### Three distinct acceptance gates

1. **Version-pin research — `PIN_SELECTED_FOR_PROBE`.** This design selects
   `3.0.6` at the full commit above as the sole RES-01A probe pin. This approves
   neither a package-file mutation nor project/platform compatibility.
2. **Current-project package graph — `BLOCKED_PENDING_RES-01A`.** A separately
   authorized RES-01A may use Unity Package Manager's asynchronous API to change
   only this project's `Packages/manifest.json` and `Packages/packages-lock.json`.
   Exact resolution/import plus a clean Mac Intel Unity `2022.3.18f1` compile is
   the package-adoption gate. Passing it may unlock B/C/D only as **Mac/Android
   Demo implementation candidates**; it is not Android, Windows, CDN, or release
   acceptance.
3. **Platform and delivery evidence — `NOT RUN`.** Android IL2CPP/API-24 build,
   real remote/cache/network-policy behavior, and Windows resolution/import/
   compile are separate later acceptance gates after the relevant integration
   exists. The unavailable Windows environment does not block current Mac/
   Android Demo implementation, but no Windows compatibility or release claim
   is permitted until that evidence passes.

The package metadata declares a Unity `2019.4` minimum, so Unity `2022.3.18f1`
is above the declared floor. That is candidate evidence only. Official evidence
does not establish any project-specific gate above, and “fully compatible with
FightMatch” remains unsupported.

## 2. Stable candidates considered

| Candidate | Official status/evidence | Relevance | Research disposition |
|---|---|---|---|
| `3.0.6` (`3b4cfb36…`) | [stable release, 2026-09-18](https://github.com/tuyoogame/YooAsset/releases/tag/3.0.6); [changelog lines 5-30](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/CHANGELOG.md#L5-L30) | Newest stable; fixes after `3.0.5`; package declares Unity `2019.4` floor. | **Recommended version-probe candidate.** |
| `3.0.5` (`94422fc…`) | [official release, 2026-07-20](https://github.com/tuyoogame/YooAsset/releases/tag/3.0.5) | Immediate predecessor in the same major line; useful rollback comparison. | Do not prefer without a regression specific to `3.0.6`. |
| `3.0.4` (`5d478a9…`) | [official release, 2026-07-14](https://github.com/tuyoogame/YooAsset/releases/tag/3.0.4) | Earlier stable `3.x` baseline. | Secondary diagnostic comparison only. |
| `2.3.19` (`5df594f…`) | [official release, 2026-06-02](https://github.com/tuyoogame/YooAsset/releases/tag/2.3.19); [`package.json`](https://github.com/tuyoogame/YooAsset/blob/2.3.19/Assets/YooAsset/package.json) | Maintained stable `2.x` line with the same package id and declared Unity floor/dependency set. | Fallback comparison only; choosing the older major line requires a demonstrated `3.x` blocker. |

Pre-release tags (for example `3.0.3-beta`) were not treated as stable candidates. Short hashes above are release-page identifiers; only the full `3.0.6` commit is proposed as the immutable pin.

## 3. Official source control-flow findings (`3.0.6`)

All source findings below are for tag `3.0.6`, commit
`3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`, accessed 2026-10-01.

### 3.1 Download temporary file and resume decision

Source: [`DownloadAndCacheFileOperation.cs`](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/Internal/DownloadAndCacheFileOperation.cs)

- Lines 21-33 derive one sandbox temporary path per bundle through `GetTempFilePath`.
- Lines 43-59 create the parent directory and enable resume only when `Bundle.FileSize >= ResumeDownloadMinimumSize`; smaller files use a normal request.
- Lines 160-188 inspect an existing temp file. A temp length greater than or equal to the expected bundle size is deleted; otherwise its length becomes `resumeOffset`. The resume request appends to the existing file and sets `removeFileOnAbort = false`.
- Lines 193-201 delete any existing temp file before a normal request.
- Lines 63-79 report resumed progress as `existingLength + bytesReceived`.
- Lines 85-111 preserve a partial resume file after most failed resumed requests, but delete it for HTTP `416`; all failed non-resume requests delete their temp file.
- Lines 115-141 pass a successful transfer to cache writing and delete the download temp file after cache write success **or** failure.

Interpretation: resumability is threshold-controlled and persists partial bytes across ordinary resume failures/abort disposal. This is not evidence that every server/CDN safely supports resume.

### 3.2 Exact HTTP `Range` behavior and response handling

Source: [`UnityWebRequestFile.cs`, lines 38-53](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Services/UnityWebRequest/UnityWebRequestFile.cs#L38-L53)

- `DownloadHandlerFile(savePath, appendToFile)` is used and receives `removeFileOnAbort`.
- When `ResumeOffset > 0`, YooAsset sets exactly `Range: bytes={ResumeOffset}-`.

Source: [`UnityWebRequestBase.cs`, lines 239-276](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Services/UnityWebRequest/UnityWebRequestBase.cs#L239-L276)

- Completion records `responseCode`/`error` and accepts `UnityWebRequest.Result.Success`; otherwise it reports failure, then disposes the underlying request.
- The inspected code does **not** independently require HTTP `206` or validate `Content-Range` before accepting a resumed request.

Consequence: the claim “resume is safe against a server that ignores Range and replies `200` with the full object” is **unsupported**. The later size/CRC verification should reject an appended overflow/corrupt file, but the server behavior must be tested. A compliant CDN/origin range response is an external prerequisite.

### 3.3 Abort and pause semantics

Source: [`UnityWebRequestBase.cs`, lines 154-183](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Services/UnityWebRequest/UnityWebRequestBase.cs#L154-L183)

- `AbortRequest` moves a running request to `Aborting` and calls Unity's `Abort`; the source explicitly notes that abort is not instantaneous.
- Disposal performs request cleanup. Combined with `removeFileOnAbort = false` in the resume path, already-written partial bytes are intended to remain available for later resume.

Source: [`DownloadSchedulerOperation.cs`, lines 65-72](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L65-L72) and [lines 101-158](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L101-L158)

- `PauseScheduler()`/`ResumeScheduler()` only toggle a scheduler flag (lines 179-193).
- Each update drives the backend and updates existing download operations **before** checking `Paused`.
- While paused, no new operation starts, but already-started operations continue. This is stated explicitly in lines 69-71.
- An operation whose reference count reaches zero is aborted (lines 121-127).

Therefore scheduler pause is **not transport pause** and cannot by itself implement “stop cellular transfer now.” Any such product behavior requires explicit application ownership/release/abort semantics and validation that a resumable partial remains usable.

### 3.4 Retry and URL selection

Source: [`SFSDownloadBundleOperation.cs`, lines 57-76](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/SFSDownloadBundleOperation.cs#L57-L76), [lines 101-153](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/SFSDownloadBundleOperation.cs#L101-L153), and [lines 177-184](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/SFSDownloadBundleOperation.cs#L177-L184)

- A bundle download first reuses a scheduler operation keyed by the bundle; otherwise it creates and registers one.
- On failure, it feeds the URL/HTTP code/error into the retry controller. If allowed, it waits, releases the failed operation, and creates/reuses the next operation.
- Candidate URLs come from `IRemoteServices.GetRemoteUrls`; `DownloadUrlPolicy.SelectUrl` chooses one.

Source: [`DownloadRetryController.cs`, lines 29-90](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/DownloadRetryController.cs#L29-L90)

- Retry is bounded by `maxRetryCount` and `IDownloadRetryPolicy.IsRetryableError`.
- Starting a retry increments the retry count and obtains the delay from `CalculateRetryDelay`; the wait uses realtime.

The exact error classes and backoff curve are policy-dependent. A claim such as “all 5xx/timeout failures retry three times with exponential backoff” is **unsupported** until the project selects and verifies concrete policy/settings.

### 3.5 Concurrency and duplicate suppression

Source: [`DownloadSchedulerOperation.cs`, lines 29-58](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L29-L58), [lines 140-160](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L140-L160), and [lines 195-227](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L195-L227)

- Configuration supplies `MaxConcurrency` and `MaxRequestsPerFrame`.
- Starts are limited by both the number of available concurrency slots and the per-frame start cap.
- The scheduler map is keyed by `BundleGuid`; requesting an existing operation increments its reference count instead of creating a second transfer.

These are configured ceilings, not evidence of target-device throughput or memory behavior. Appropriate values remain a performance-probe question.

### 3.6 Verification, CRC, and atomic cache publication

Source: [`SBCWriteCacheOperation.cs`, lines 44-83](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/Operations/SBCWriteCacheOperation.cs#L44-L83)

- Before publishing, YooAsset builds a `TempFileInfo` from the downloaded path plus the manifest bundle's `FileCrc` and `FileSize`, then runs `VerifyTempFileOperation`.

Source: [`VerifyTempFileOperation.cs`, lines 44-82](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/Operations/Internal/VerifyTempFileOperation.cs#L44-L82)

- Verification is queued to the thread pool when possible and delegates to `FileVerifyHelper.VerifyFile`.

Source: [`FileVerifyHelper.cs`, lines 18-55](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/FileVerifyHelper.cs#L18-L55)

- Missing file fails.
- A nonzero expected size rejects both incomplete and overflow files.
- A nonzero expected CRC computes CRC32 and requires equality.
- Size `0` and CRC `0` are sentinels that skip those checks.

Source: [`SBCWriteCacheOperation.cs`, lines 85-139](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/Operations/SBCWriteCacheOperation.cs#L85-L139)

- After verification, data and info files are written to cache-temporary paths.
- Existing final files are removed and the temporary files are moved into the final paths; exceptions clean the cache-temporary paths.
- The info record stores manifest CRC, size, and timestamps.

The verified integrity primitive is CRC32 plus size, not a cryptographic content hash. Claims of SHA-256 authentication, signed manifests, or resistance to malicious tampering are **unsupported** by the inspected path.

### 3.7 Cache cleanup

Source: [`ClearCacheFilesOperation.cs`, lines 42-75](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/Operations/Internal/ClearCacheFilesOperation.cs#L42-L75)

- Cleanup processes an explicit list of bundle GUIDs, removes entries incrementally, reports progress, and yields when busy.

Source: [`SandboxBundleCache.cs`, lines 292-307](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/SandboxBundleCache.cs#L292-L307) and [`SandboxBundleCacheEntry.cs`, lines 42-64](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/BundleCache/Services/SandboxBundleCache/SandboxBundleCacheEntry.cs#L42-L64)

- Removal updates the in-memory maps/space counter, then recursively deletes the cache entry directory.
- Physical deletion catches/logs exceptions and returns failure. The caller shown above does not surface that Boolean result as an operation failure.

Therefore “clear cache always guarantees physical deletion” is **unsupported**; disk-full, permission, file-lock, and interrupted-cleanup behavior needs a target-platform test and postcondition check.

### 3.8 Foreground/background lifecycle

Source: [`YooAssetsDriver.cs`, lines 19-32](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/YooAssetsDriver.cs#L19-L32)

- YooAsset is advanced by a `MonoBehaviour.Update()` call to `YooAssets.Update()`.
- The inspected driver has no `OnApplicationPause` or `OnApplicationFocus` handling; its `OnApplicationQuit` cleanup is editor-only.

Official source inspected here does **not** establish continued download while an Android/iOS app is suspended, nor automatic pause/resume on app focus changes. Those claims are **unsupported** and are platform/application responsibilities. Test foreground loss, OS suspension/termination, process restart, and resume from the persisted temp file separately.

## 4. Gaps that must remain explicit

| Claim/question | Current status | Required evidence |
|---|---|---|
| `3.0.6` is the sole probe pin | `PIN_SELECTED_FOR_PROBE` | This research record plus the exact tag/full commit; no package adoption implied. |
| `3.0.6` resolves/imports/compiles in the current FightMatch package graph on Mac Intel | `BLOCKED_PENDING_RES-01A` | Authorized two-file UPM change, exact graph, and fresh Mac Intel compile evidence. |
| Android IL2CPP/API-24 support | `NOT RUN`; later platform gate | Build and device smoke evidence after relevant integration; not required to begin Mac/Android Demo implementation after RES-01A passes. |
| Windows support | `NOT RUN`; later cross-platform/release gate | Exact package resolution/import/compile on an authorized Windows environment; absence does not block current Mac/Android Demo work. |
| CDN supports reliable resume | `NOT RUN`; later delivery gate | Controlled tests for `206`, correct `Content-Range`, `416`, ignored Range/`200`, disconnect, and changed remote object/ETag. |
| Cellular/Wi-Fi policy can immediately stop traffic | Unsupported by scheduler pause alone | App-owned network reachability policy plus abort/release integration and packet/device verification. |
| Background download continues while app is suspended | Unsupported | OS-specific design and physical-device lifecycle test. |
| Content is cryptographically authenticated | Unsupported | Separate signed-manifest/content-authentication design; CRC32 is corruption detection only. |
| Cache deletion is always successful | Unsupported | Verify postconditions and define retry/reporting for deletion failures. |
| Exact retry count/backoff/error classes | Not selected | Fix the project policy and parameters, then test them deterministically. |

## 5. Installation/process note for the later probe

Unity's official Package Manager documentation supports installing a package from a Git URL and selecting a package in a repository subfolder with `?path=`. A future, exact RES-01A task packet may authorize the two package-file changes in this current project; this design turn does not. RES-01A must use the immutable full commit, Unity Package Manager's supported asynchronous workflow, and the existing serial Unity owner C. No worktree, branch, clone, copied project, or alternate checkout is assumed or authorized here.

Official Unity source: [Install a package from a Git URL (Unity 2022.3 manual)](https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html), accessed 2026-10-01.

## 6. Official-source index

All links were accessed 2026-10-01.

- YooAsset official releases: <https://github.com/tuyoogame/YooAsset/releases>
- YooAsset `3.0.6` release: <https://github.com/tuyoogame/YooAsset/releases/tag/3.0.6>
- YooAsset `3.0.6` immutable commit: <https://github.com/tuyoogame/YooAsset/commit/3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804>
- YooAsset `3.0.6` package metadata: <https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json>
- YooAsset `3.0.6` changelog: <https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/CHANGELOG.md>
- YooAsset `3.0.6` license: <https://github.com/tuyoogame/YooAsset/blob/3.0.6/LICENSE>
- YooAsset official source paths are linked inline in section 3 with exact tag and line ranges.
- Unity 2022.3 official Git dependency documentation: <https://docs.unity3d.com/2022.3/Documentation/Manual/upm-ui-giturl.html>

## 7. Research conclusion

`3.0.6` at commit `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` is the strongest official-evidence candidate and is selected as the sole current-project package-adoption probe pin. Its inspected source has threshold-based resumable temp files, HTTP Range requests, bounded policy-driven retries, bundle-GUID deduplication, concurrency controls, size/CRC verification, cache-temporary publication, and explicit cache cleanup. It does **not** by itself prove package adoption or project/platform compatibility, robust handling of a noncompliant Range server, transport-level pause, background execution, cryptographic authenticity, or guaranteed physical cache deletion. Those boundaries must survive into the implementation plan and acceptance tests.

## 8. Fixed inputs and current repository facts

This design inherits, and does not reopen, the accepted decisions indexed by
`START_HERE.md`, `engineering-architecture-001.md`, the frozen
`engineering-system-001.md`, and the accepted update-lifecycle design:

1. Runtime player UI is uGUI. The asset layer does not choose or extend a second
   UI framework.
2. English and Simplified Chinese are the initial player locales. Luban-authored
   text and project-owned localization bindings remain authoritative; Unity
   Localization and Addressables are not introduced.
3. Resources use YooAsset, served eventually from R2 Standard through a
   Cloudflare custom domain/cache. A production account, bucket, domain,
   certificate, public URL, credentials, and deploy pipeline do not yet exist as
   accepted implementation evidence.
4. The first install contains a small bootstrap and downloads the main content.
   Every mobile-data use requires an explicit prompt showing remaining bytes;
   a Wi-Fi-to-mobile transition must stop transfer and prompt again.
5. `ReleaseSetId` is the activation unit. Download completion is not activation;
   the accepted lifecycle is
   `Prepared -> RequestRestart -> CodeEntered -> BusinessReady`.
6. The existing six-file business publication remains authoritative and enters
   through `FirstReleaseContentStorage` and `PublishedContentCatalog`. Its files
   are source, payload, validation, review, publication receipt, and content
   release set. YooAsset may transport those bytes but may not bypass admission.
7. The accepted Luban output is a generated text artifact plus manifest. RES-01
   consumes the currently accepted artifact identity from that manifest; it does
   not hard-code the older 171-row research identity or create a second editable
   text authority.
8. Unity remains `2022.3.18f1`. `2022.3.62f3` is only a future patch candidate;
   Android API 24 is an accepted release target but is not implemented evidence.

Direct inspection on 2026-10-01 found no `com.tuyoogame.yooasset` or
`com.unity.scriptablebuildpipeline` entry in `Packages/manifest.json` or
`Packages/packages-lock.json`. The project already declares Unity's
`assetbundle` and `unitywebrequest*` modules. Therefore the repository state is
**not installed, not resolved, not compiled, not integrated, and not device
validated**. The current loader is a bounded `UnityWebRequest` reader for six
fixed StreamingAssets files; that is the replacement seam, not proof of remote
resource support.

## 9. Version conclusion, package topology, and conditional rollback

### 9.1 Version conclusion

The design selects exactly **YooAsset 3.0.6 at full commit
`3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`** as the only intended integration
pin. “Selected” means the pin research is accepted and implementation packets
shall not improvise another version. It does not mean the package is adopted or
any platform is compatible. RES-01A's Mac gate must pass before B/C/D become
Mac/Android Demo implementation candidates; Android, real-delivery, and Windows
evidence remain separate later gates. `2.3.19` is only a diagnostic comparison
if a specific 3.0.6 regression is demonstrated; changing major line returns to
engineering review rather than silently falling back.

The future Package Manager request is the full immutable URL:

`https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`

Unity's 2022.3 manual requires a full SHA for a commit revision and places
`?path=` before `#revision`. No moving branch, short SHA, floating tag, embedded
copy, OpenUPM registry, or hand-edited lock result is permitted.

### 9.2 Dependency topology

| Layer | Exact identity | Allowed dependency direction |
|---|---|---|
| Project contracts | Proposed `FightMatch.AssetAccess` assembly | Unity object types may be generic payloads, but no YooAsset type/name. Referenced by Host/Presentation integration only; not by Core business rules or save codecs. |
| Third-party package | `com.tuyoogame.yooasset` 3.0.6, exact commit above | Referenced only by proposed `FightMatch.YooAssetAdapter` and its Editor build tooling. |
| Declared build dependency | `com.unity.scriptablebuildpipeline` 1.21.25 | Resolved by UPM because YooAsset declares it; no project code directly references it outside future Editor build tooling. |
| Existing Unity modules | `assetbundle`, `unitywebrequest`, `unitywebrequestassetbundle` 1.0.0 | Already present; not independently upgraded. |
| Fixed bootstrap | Player-built scene/prefab/font/text/minimal graphics | No remote dependency is needed to explain download/startup failure. |
| Remote resource package | One stable Yoo package name: `FightMatchMain` | One manifest per platform and release-set-bound package version; tags partition text/content/ui/image/audio without multiplying package lifecycles. |

Only `FightMatch.YooAssetAdapter` may reference the YooAsset runtime assembly.
Core, Application, current save/publication code, Presentation controllers, and
public business contracts must remain free of `YooAsset.*`, `AssetHandle`,
`ResourcePackage`, package locations, manifest objects, and CDN URLs.

### 9.3 Future install and rollback whitelist draft

A future, exact RES-01A task packet may authorize changes only to
`Packages/manifest.json` and `Packages/packages-lock.json` in this current
project. C remains the serial Unity owner and must use Unity Package Manager's
asynchronous `Client.AddAndRemove` path in the fixed Editor, not manual JSON
editing. Any temporary installer source must live outside the repository or be
separately whitelisted and absent from the final tree. Any unrelated package or
version movement is `BLOCKED`.

On success, the accepted final state retains one direct immutable Git dependency
plus exactly the transitives resolved from its package metadata; the exact
two-file diff is evidence and is **not** rolled back merely to simulate a probe.
On failure or rejection, rollback runs through `Client.AddAndRemove`, removing
only `com.tuyoogame.yooasset`; Unity resolves the now-unused transitive dependency.
The failed path must prove the final logical package graph matches the captured
pre-probe state. Do not delete `Library/PackageCache`, hand-edit the lock, reset
unrelated work, or remove user files.

## 10. Project-owned deep module and interface

The module boundary is deliberately narrower than YooAsset. Callers ask for a
project asset under one exact release set and receive either a project lease or
a project diagnostic. They do not coordinate manifests, handles, retries, cache
paths, or URL selection.

Semantic contract (names are frozen for packet planning; exact C# syntax is
finished in RES-01B):

```text
IFightMatchAssetProvider
  AcquireAsync<T>(FightMatchAssetId assetId,
                  string releaseSetId,
                  AssetAcquireBudget budget,
                  long requestEpoch)
    -> FightMatchAssetAcquireResult<T>

FightMatchAssetAcquireResult<T>
  IsAccepted | Lease? | Diagnostic? | RequestEpoch | ReleaseSetId

IFightMatchAssetLease<T> : IDisposable
  AssetId | ReleaseSetId | LeaseId | Asset | IsReleased

FightMatchAssetDiagnostic
  Code | Stage | AssetId | ReleaseSetId | Retryable | SafeDetail
```

Rules:

- `FightMatchAssetId` is a validated logical project identifier, not a YooAsset
  address or file path. The adapter owns its immutable mapping to package,
  location, kind, expected release set, and expected hash/manifest entry.
- A failed result never contains a usable asset. A successful result always owns
  one releasable lease. Exceptions from SDK callbacks are translated to bounded
  diagnostics at the adapter boundary.
- `SafeDetail` may contain status, HTTP code, selected host class, manifest
  identity, counts, and hashes; it must never contain credentials, signed URLs,
  tokens, device identifiers, or unrestricted local paths.
- The caller passes the exact running/target `ReleaseSetId`; the adapter refuses
  missing, latest, mismatched, or partially prepared sets. It never looks up a
  “current business version” of its own.
- Cancellation, progress, and download consent belong to the Host/update
  coordinator. Presentation renders a project snapshot and sends intent; it does
  not call YooAsset or make policy decisions.
- Raw text/content acquisition returns bounded immutable bytes through a sibling
  project result/lease shape. The six business files are handed to
  `FirstReleaseContentStorage.Create` and `PublishedContentCatalog`; the text
  artifact is handed to the project localization parser/binding. Transport
  success never changes a business binding or save.

The deep module's hidden implementation owns package initialization, exact
manifest selection, prefetch/download, handle reference counts, epoch filtering,
cache diagnostics, URL construction, and translation of YooAsset callbacks. A
single call replaces that temporal complexity for the rest of the project.

## 11. Fixed bootstrap boundary

The bootstrap must be Player-built and usable with the network disabled and the
Yoo cache absent. It contains only what is required to explain and recover from
startup/download failure:

| Built-in item | Required content | Excluded content |
|---|---|---|
| Root | One bootstrap scene/prefab with `RuntimeRoot`, one `EventSystem`, safe-area root, and the update/download controller seam | Main navigation, battle/map/roster pages, business systems instantiated early |
| Text | Minimal English + Simplified Chinese rows generated from the same CSV/Luban authority for checking, remaining size, mobile-data confirmation, cancel, retry, corruption, storage, offline, restart, and fatal bootstrap diagnostics | Independently hand-maintained finished player copy; the prefab defaults remain obvious diagnostic placeholders |
| TMP | Bootstrap font assets covering every bootstrap glyph, material presets, TMP shaders/includes/resources, fallback chain, and leading/following line-break tables | Assuming a remote font can render the error that says remote loading failed |
| Graphics | Solid/background, progress treatment, buttons, warning/network icon, and any mandatory sprite-atlas/material dependency | Main game artwork, page illustrations, animation/audio polish |
| Code/data | Release-set descriptor parser, safe diagnostics, network-policy prompt state, last-known validated set reference, and minimal local settings read for locale | A second business/save database, gameplay rules, arbitrary remote scripts |

At build verification, dependency traversal must prove the bootstrap has no
reference to remote-only prefabs/fonts/materials. Starting with an empty cache
and unreachable host must still show legible bilingual error/retry/mobile-data
UI. Handoff to the frozen uGUI work occurs only after `BusinessReady`; the
bootstrap does not extend or redesign those pages.

## 12. Remote content map and admission

`FightMatchMain` uses immutable logical scopes/tags within one package:

| Scope/tag | Payload | Consumer and admission |
|---|---|---|
| `fm.content.first-release` | The existing six exact publication files | Acquire as raw bytes; apply existing per-record/total budgets; construct `FirstReleaseContentStorage`; publish/query only through `PublishedContentCatalog`. |
| `fm.text.full` | `fm-text-v1.json` and its generated manifest | Verify ReleaseSet-recorded SHA-256 and schema version, parse into project localization DTOs, then create project bindings. Luban runtime types are not required. |
| `fm.ui.runtime` | Main uGUI prefabs plus their material/font dependencies | Acquire through typed project leases; Host composes, page lifetime releases. Prefab visible text remains diagnostic until project localization binding replaces it. |
| `fm.image.runtime` | Sprites, atlases, textures, materials | Acquire through leases; no direct address in controllers. |
| `fm.audio.runtime` | Music/SFX clips and mixer-connected resource dependencies | Acquire through leases; audio routing remains the audio owner's concern. |

The mapping descriptor records logical id, expected Unity type/raw kind, package,
location, scope tag, platform, ReleaseSetId, and content identity. It is produced
with the resource build and covered by the outer ReleaseSet hash. Duplicate ids,
unknown kinds, path traversal, missing entries, or a different release set fail
before load.

YooAsset manifest/cache/download success proves only that transport-layer files
are locally available according to its manifest CRC/size checks. It does **not**
prove the outer ReleaseSet's SHA-256/authorization, the six-file publication
receipt, text schema, `ContentBinding`, save compatibility, or `BusinessReady`.

## 13. ReleaseSet descriptor and activation

Do not silently overload the existing business `ContentReleaseSet` DTO. A new,
versioned outer **resource/update descriptor** is required, while the existing
sixth business file remains one hashed member. Minimum fields are:

```text
schemaVersion
releaseSetId
codeCompatibility:
  baseProtocolVersion, appBuildIdentity, requiredCapabilities[]
businessContent:
  six file names + lengths + sha256
  contentReleaseSetSha256, publicationReceiptSha256, ContentBinding identity
text:
  artifactName, artifactLength, artifactSha256, schemaVersion, sourceReceipt
resources:
  platform, packageName=FightMatchMain
  yooAssetPackageVersion=3.0.6
  yooManifestPackageVersion, manifestName, manifestLength, manifestSha256
  mappingDescriptorSha256, requiredScopeHashes[]
publication:
  operationId, receiptSha256, authorizationEvidenceId
```

The descriptor is canonical, immutable, length bounded, and authenticated by a
future approved publication-trust mechanism. HTTPS, R2 ETag, YooAsset CRC, or a
hash stored only beside the same untrusted object is not release authorization.
Selecting the signature/key service remains a central security decision; until
then a Demo may use an explicitly local trust root and must not be called a
production trust chain.

Activation invariants:

1. **Prepared** — exact descriptor accepted; all bootstrap-required content,
   full text, six business files, required object bundles, and manifest are
   locally present; outer SHA-256 values and inner admissions pass; required
   old complete set remains retained. Nothing switches in this process.
2. **RequestRestart** — the user's restart intent and exact prepared set are
   durably recorded through the accepted device/update record boundary. The
   running set remains unchanged.
3. **CodeEntered** — on the next process, the fixed bootstrap reloads and
   revalidates the exact target, initializes the exact manifest/package, and
   completes the code/entry handshake. There is no partial fallback to “latest.”
4. **BusinessReady** — the text artifact, six-file catalog, latest player head,
   original bindings, required resource leases, and uGUI entry are jointly
   validated. Only now may ordinary business input open.

If failure occurs before any new code/package entry, a new process may select
the previously successful **complete** set only after rechecking current save
compatibility. Once code/package entry has partially occurred, that process
shows the fixed diagnostic UI and exits/restarts; it never mixes old and new.
The old full set is retained through BusinessReady and, for the first release,
is not automatically deleted afterward. Player save rollback is never part of
resource rollback.

## 14. R2 Standard plus Cloudflare delivery contract

### 14.1 Object and URL layout

Use a production custom domain, not `r2.dev`, and immutable object keys:

```text
https://<asset-domain>/fightmatch/<platform>/<releaseSetId>/
  descriptor/<descriptor-sha256>.json
  yoo/FightMatchMain/<manifest-package-version>/<exact Yoo output name>
  receipts/<receipt-sha256>.json
```

The ReleaseSet-specific base URL is injected only into the adapter's exact
YooAsset 3.0.6 `IRemoteServices` implementation. `GetRemoteUrls` returns an
ordered, bounded list from approved HTTPS base hosts plus the supplied immutable
relative file name. It rejects absolute input, `..`, query/fragment injection,
and unknown release/platform/package. If there is no independently operated
fallback host, return only the primary; do not pretend the same origin is an
availability fallback.

There is no mutable client-facing `latest` URL. Publish every object, verify it,
then publish/authorize the immutable descriptor reference through the separately
approved release channel. Never overwrite an object key; corruption produces a
new hash/key and ReleaseSet. This also avoids Cloudflare's documented stale
overwrite and negative-404 cache behavior.

### 14.2 HTTP/cache behavior

- Immutable objects: `Cache-Control: public, max-age=31536000, immutable` and a
  stable ETag. ETag is useful for transport diagnostics/conditional requests but
  is not the project content hash or trust proof.
- Bundle/range representation must be unencoded (`Content-Encoding` absent or
  identity). A valid single Range returns `206`, concrete `Content-Range`, exact
  `Content-Length`, stable total length, and consistent ETag. Unsatisfiable
  offsets return `416` with `Content-Range: bytes */<size>`.
- A full request returns `200` and the exact immutable bytes. A resumed request
  that receives `200`, malformed/mismatched `Content-Range`, changed ETag/length,
  truncation, or final CRC/SHA mismatch is failed; the adapter must not report
  Prepared. Targeted retry may restart the exact object from zero after the
  failure is recorded.
- `404` for an immutable key is a release/deploy defect, not “not yet visible.”
  The client reports it and does not poll a mutable filename or another release.
  Operators must not upload a new object later under the same negatively cached
  key.
- Cache purge is an operator recovery tool, never the client correctness model.
  Correctness comes from immutable names plus final identity verification.

Cloudflare's official docs state that an R2 custom domain is required for Cache,
while `r2.dev` is non-production and does not provide those features. They also
document `206`/`Content-Range` requirements, Range/If-Range behavior, possible
`200` fallback, file-size cache limits, and stale 404/overwrite behavior:
[R2 cache](https://developers.cloudflare.com/cache/interaction-cloudflare-products/r2/),
[Range requests](https://developers.cloudflare.com/cache/reference/range-requests/),
[R2 consistency](https://developers.cloudflare.com/r2/reference/consistency/),
accessed 2026-10-01.

### 14.3 Human/deployment prerequisites

Account ownership, Standard-class bucket creation, billing/cost alerts, custom
domain/DNS, TLS certificate, public-read/access-control policy, WAF/rate limits,
CORS if later required, object upload permissions, release publisher identity,
and cache rules are explicit human/deployment TODOs. They do not block the
no-credential adapter design or a local HTTP proof. No access key, account id,
token, signed URL, certificate private key, or secret value may appear in source,
logs, evidence, test fixtures, URLs, or this repository.

## 15. Download and network-policy state machine

```text
Idle -> Inspecting -> (ReadyFromCache | ConsentRequired | Queued)
ConsentRequired --approve--> Queued
ConsentRequired --decline--> Cancelled
Queued -> Downloading -> Verifying -> Prepared
Downloading -> StopRequested -> Stopped
Downloading/Verifying -> FailedRetryable -> Queued
Downloading/Verifying -> FailedTerminal
Stopped --retry on Wi-Fi--> Queued
Stopped --mobile confirmation every time--> Queued
```

Every UI snapshot contains operation id, release set, epoch, total files/bytes,
completed files/bytes, remaining bytes, current network class, consent-required
flag, retryability, and bounded diagnostic. Remaining is recomputed from the new
downloader/cache inspection after every stop/recreate; it is not a permanently
decremented business value.

Rules:

1. On Wi-Fi, start only after the required scope and remaining bytes are known.
   On mobile, display those bytes and await explicit approval for **that download
   attempt**. Consent is not persisted as a blanket preference.
2. On Wi-Fi-to-mobile, enter `StopRequested`, invalidate the callback epoch, and
   explicitly cancel/release the current downloader so active requests abort.
   Do not use `PauseDownload()` as the stop mechanism: official 3.0.6 source says
   paused scheduling still updates already-started operations. Do not show
   `Stopped` until active transfer termination is observed. A few in-flight bytes
   may arrive because Unity abort is not instantaneous; device packet/byte
   evidence must bound this behavior.
3. To continue, inspect the exact target again, create a new downloader, and ask
   again if the current network is mobile. Preserved temp bytes may enable Range,
   but the UI never promises resume until the local HTTP and Android tests pass.
4. SDK retry is bounded by a selected retry policy; user retry creates a new
   project operation/epoch for the same immutable target. No retry retargets
   latest. `404`, trust/identity mismatch, unsupported schema/capability, and
   insufficient storage are terminal until their cause changes.
5. A corrupt/overflow/`416` partial may be cleared only for the exact bundle/temp
   entry after proving it is not a retained complete object. Never expose “clear
   all cache” as generic recovery and never delete an old complete ReleaseSet.
6. Start with conservative, explicitly test-only limits of two concurrent files,
   at most one new request per frame, and two automatic retries. RES-01E must
   measure and either accept or revise them; they are not performance promises.
7. Background/foreground is application policy: foreground loss records state
   and stops/reconciles the operation; return to foreground re-inspects network,
   cache, and consent. No background download claim is made because YooAsset's
   driver is Update-based and inspected source provides no mobile suspension
   contract.

Unverified mandatory cases are: local server honor/ignore Range, correct and bad
`Content-Range`, ETag/object change, 416, process reopen using a partial, Android
Wi-Fi-to-mobile byte stop, repeated consent, retry after airplane mode, app
background/foreground, OS kill/relaunch, disk full, corrupt cache, concurrency,
memory, and targeted cleanup postcondition.

## 16. Lease, epoch, release, and memory rules

- Multiple consumers may acquire the same logical asset. Each gets a distinct
  project lease and lease id; the adapter may share one underlying Yoo provider/
  handle only while maintaining an internal refcount. One caller's release must
  not invalidate another caller's live lease.
- The adapter retains the underlying `AssetHandle` until the last project lease
  is released. Official source shows `HandleBase.Release/Dispose` decrements the
  provider and may unload a zero-reference bundle; callers never receive that
  handle directly.
- `Dispose` on a project lease is idempotent. Use-after-release returns a project
  diagnostic in development/test rather than touching an invalid SDK handle.
  The owner page/controller releases in its teardown path; instantiated objects
  get an explicit instance lease/ownership rule rather than assuming the source
  handle can be dropped immediately.
- Each provider initialization, manifest switch attempt, download operation, and
  acquisition batch has a monotonically increasing epoch. Callbacks must match
  provider instance, epoch, ReleaseSetId, operation id, and asset id before they
  can publish. Stale success releases its SDK handle immediately and cannot
  replace a newer result, dismiss consent, or mark Prepared.
- Hard limits cover concurrent acquisition count, raw-byte length, queued
  callbacks, and retained lease count. Reject before allocation/traversal where
  possible with `BudgetExceeded`. Unity object memory is observed and reported;
  no unmeasured MB promise is invented. Unload-unused operations run only at a
  coordinated safe point after project refcounts reach zero.
- Diagnostics expose live leases by id/asset/owner/release/age, pending operation
  epochs, package/manifest/release identity, cache/download counts, and last safe
  error. They do not expose secrets or become writable game state.
- Download/cache/lease registries are technical state only. `PublishedContentCatalog`,
  current `ContentBinding`, the player's latest save head, and the update
  lifecycle remain the only business/readiness authorities.

## 17. What each environment can prove

| Demo / locally verifiable without credentials | R2 or manual/device prerequisite | Full release work explicitly out of scope |
|---|---|---|
| Mac Intel UPM adoption/compile evidence; package and asmdef isolation | Exact Windows package resolution/import/compile evidence | Approving Unity patch upgrade or a different YooAsset version |
| Fake-provider tests for results, leases, epochs, wrong set, limits | Android profiler/lifecycle evidence for memory and process suspension | iOS/mini-game support, HybridCLR, remote code |
| Offline empty-cache bootstrap and bilingual TMP rendering | Real device locale, safe area, touch, low-memory checks | Final art/audio polish and all gameplay screens |
| Local HTTP server matrix for 200/206/416/404, corruption, Range-ignore, retry, reopen | R2 Standard bucket + custom domain + Cloudflare cache/header observations | Production account procurement, secrets, billing, WAF, incident runbook |
| Exact six-byte tamper/admission tests and Luban artifact hash/schema checks | Upload/download one non-production immutable ReleaseSet and compare hashes | Production signing/key service and live release publication |
| Prepared/restart state simulation with old complete set retained | Android Wi-Fi/mobile/airplane/background/process-kill matrix | Store submission, rollout, telemetry/alerting, automatic cache reclamation |

Passing the first column supplies technical evidence for separately authorized
Mac/Android Demo integration packets. The Android/cloud parts of the second
column are required before Android release readiness; its Windows item is the
separate cross-platform/release gate. Nothing in either column authorizes work
by itself or authorizes the third.

## 18. Ordered implementation packets

These are dispatch drafts, not current write authorization. Every packet needs a
fresh preflight, exact starting hashes, a sole delivery owner, and independent
testing-lead review. C remains the serial Unity executor until responsibility is
formally transferred.

### RES-01A — immutable package probe

- Owner: C remains the serial Unity implementation owner; independent receipt:
  testing lead.
- Net whitelist: `Packages/manifest.json`, `Packages/packages-lock.json` only.
- Work: `Client.AddAndRemove` the exact Git SHA; capture resolved package graph,
  license, import, compilation, and assembly names; do not write adapters/assets.
- Gate: exact direct URL/version/SHA, only declared transitives, and a clean Mac
  Intel Unity `2022.3.18f1` import/compile. Passing adopts that exact package graph
  and unlocks B/C/D only as Mac/Android Demo implementation candidates. Android
  IL2CPP/API-24, real delivery, and Windows compatibility remain `NOT RUN` here.
- Success retention / failure rollback: success retains the accepted exact
  two-file diff. Failure or rejection uses Package Manager removal and proves the
  logical graph returned to preflight; no manual lock edit/cache deletion.
- Budget: two package files changed; zero Assets/ProjectSettings changes; one
  compile after resolution and one after rollback only if the probe fails.

### RES-01B — project contract and YooAsset adapter

- Owner: engineering architecture -> implementation; independent unit review.
- Proposed new whitelist:
  `Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef`,
  `FightMatchAssetContracts.cs`,
  `Assets/Scripts/FightMatch/YooAssetAdapter/FightMatch.YooAssetAdapter.asmdef`,
  `YooAssetAssetProvider.cs`, `YooAssetPackageLifecycle.cs`,
  `YooAssetRemoteServices.cs`, their exact `.meta` files, and
  `Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef`,
  `FightMatchAssetProviderTests.cs` plus exact `.meta` files.
- Gate: fake and adapter tests cover result/diagnostic mapping, multi-lease,
  idempotent release, last-release handle disposal, stale epoch, wrong set,
  limits, URL rejection, and no YooAsset reference outside adapter/its tests.
- Rollback: remove only new packet files/metas after confirming no later packet
  depends on them; remove package only through RES-01A rollback.
- Budget: at most 6 production C# files / 1,000 physical lines and 2 test C# /
  900 lines; no public business/save/serialization changes.

### RES-01C — fixed bootstrap and download policy

- Owner: engineering integration with art/localization inputs; testing lead owns
  the empty-cache and network-policy cases.
- Proposed whitelist is a new `Assets/FightMatch/Bootstrap/` scene/prefab and its
  explicitly enumerated TMP/font/material/shader/line-break/minimal graphic
  assets/metas; one Host bootstrap controller/composition file; one generated
  bootstrap localization artifact/receipt; corresponding EditMode/PlayMode
  tests. Exact filenames, GUIDs, source-font license, and accepted text receipt
  must be frozen before dispatch; until then this packet is **BLOCKED**.
- Gate: no-network/empty-cache bilingual boot; diagnostic prefab placeholders
  replaced at runtime; one RuntimeRoot/EventSystem; mobile consent snapshot;
  Wi-Fi switch invokes cancel/epoch path, not scheduler pause.
- Rollback: restore prior startup scene/reference and remove only the enumerated
  new bootstrap assets/metas after dependency audit.
- Budget: one scene, one root prefab, only bootstrap glyph/font material set, at
  most 4 production C# / 700 lines and 3 focused tests / 900 lines.

### RES-01D — resource build, content bridge, and ReleaseSet

- Owner: engineering system design -> Editor tooling implementer; content and
  localization owners provide accepted input receipts.
- Proposed whitelist: new YooAsset collector/build config under
  `Assets/FightMatch/ResourceBuild/`; one versioned outer ReleaseSet DTO/codec in
  the asset-access module; one Host raw-byte bridge replacing only the current
  loader seam; editor build/publish-plan code; focused tests and generated test
  fixtures. No generated remote bundles/manifests are committed unless the task
  packet enumerates them and their size budget.
- Gate: deterministic logical map; full text and all six bytes hashed; all six
  still rejected/accepted by existing catalog rules; local HTTP tests; exact
  manifest identity; Prepared cannot switch current; restart simulation retains
  old set; no latest/partial fallback.
- Rollback: switch Host composition back to the existing loader; do not alter or
  delete accepted six-file source artifacts/player saves.
- Budget: at most 8 production C# / 1,600 lines, 5 tests / 1,400 lines, one outer
  schema version; no Core/Application/save public API change.

### RES-01E — R2/Cloudflare and Android evidence gate

- Owner: testing lead coordinates manual cloud/device execution; human account
  owner performs console/domain/credential actions; engineering only diagnoses.
- Repository whitelist: one new evidence index/report and explicitly named
  sanitized raw header/device logs under a new packet evidence root. No source,
  Packages, ProjectSettings, credentials, or production release mutation.
- Gate: custom-domain cache/header matrix; immutable upload order; 404/corruption;
  resume/reopen; Wi-Fi-to-mobile immediate stop and per-attempt consent;
  background/foreground, process kill, low storage, cache reuse, concurrency,
  memory, and exact hashes. Windows exact package resolution/import/compile is a
  separate cross-platform/release evidence item in this later gate. Missing
  cloud/device/Windows evidence is `NOT RUN`, not a failed local design and not a
  blocker for the already-scoped Mac/Android Demo implementation.
- Rollback: delete/purge only disposable non-production cloud test objects by a
  separately reviewed human runbook; repository evidence is retained.
- Budget: one ReleaseSet, bounded fixtures, two Android devices if available,
  no production traffic or store rollout.

Dependencies are A -> B -> C/D (C and D may run only with disjoint write scopes)
-> E. A's **Mac package-adoption** gate must pass before B/C/D start; that pass
does not pass Android, remote delivery, Windows, or release acceptance. E holds
the later environment-specific evidence. Windows `NOT RUN` does not block B/C/D
Mac/Android Demo work, but it does block a Windows/cross-platform release claim.
An A failure stops dependents. No packet may absorb another packet's scope merely
to “finish integration.”

## 19. Test and evidence matrix

| ID | Level | Case | Required evidence / pass condition |
|---|---|---|---|
| YA-01 | Static/UPM | Exact Mac package adoption; conditional rollback | Capture manifest/lock preflight hashes, full Git revision, resolved graph, Editor request success, and clean Mac compile. Success retains and records the accepted exact two-file diff; failure/rejection removes through UPM and proves the logical graph returned to preflight. No unrelated package drift. |
| YA-02 | Static | Assembly quarantine | Reference graph proves YooAsset appears only in adapter/editor adapter/tests; repository search finds none in Core/Application/save/business/Presentation controllers. |
| YA-03 | EditMode | Contract failure mapping | Null/unknown/wrong-type/wrong-set/SDK error yield bounded project diagnostics and no lease. |
| YA-04 | EditMode | Multi-lease/release | Two leases share safely; first release leaves second valid; last release disposes handle; double release is harmless and observable. |
| YA-05 | EditMode | Epoch races | Old completion after cancel/retry/reinitialize cannot publish, mark Prepared, or release a newer handle. |
| YA-06 | EditMode | Raw admission | Missing, oversize, tampered, wrong schema/hash/receipt/release-set six-file inputs retain existing exact rejection; accepted input reaches the catalog once. |
| YA-07 | EditMode | Text admission | Artifact SHA/schema/source receipt match outer descriptor; mismatch/old identity rejects before UI binding. |
| YA-08 | PlayMode | Bootstrap independence | Empty Yoo cache + blocked network still renders English/Chinese errors, size/consent/retry controls and TMP glyphs with exactly one root/event system. |
| YA-09 | Local HTTP | Resume matrix | 200 full, valid 206, malformed/wrong Content-Range, ignored Range 200, 416, disconnect, ETag/length change, truncated/corrupt bytes; only exact verified bytes become cached/Prepared. |
| YA-10 | Local HTTP | Retry/concurrency/cancel | Bounded selected retry and delay, two-file concurrency/one-start-per-frame ceiling, abort acknowledgment, recreation uses same immutable target. |
| YA-11 | Lifecycle | Set activation | R1 BusinessReady, R2 download/Prepared while R1 stays current, durable RequestRestart, next-process CodeEntered then BusinessReady; injected failures never mix sets or choose latest. |
| YA-12 | Lifecycle | Retention/rollback | R1 remains complete through R2 readiness; failed R2 restarts to R1 only if current save still compatible; no player save rollback/cache-wide delete. |
| YA-13 | R2 manual | CDN contract | Custom-domain URL, cache status, immutable key, Cache-Control/ETag, 200/206/416/404 behavior and SHA comparison; sanitized headers only. |
| YA-14 | Android device | Q35 network policy | Remaining bytes shown; each mobile attempt asks; Wi-Fi switch stops active bytes within measured abort behavior; stale callbacks cannot continue UI/progress. |
| YA-15 | Android device | Process/storage | Fore/background, kill/reopen partial, offline cold start, disk full, corrupt partial/cache, targeted cleanup result, cache reuse, memory peak. |
| YA-16 | Regression | Existing project | Relevant existing publication/localization/uGUI tests plus full EditMode suite under the accepted validation protocol; source/log/XML identities agree. |

Every run records fixed source revision, Unity/package/platform/device identity,
exact command or manual steps, start/end time, exit/result, logs, hashes, and
`PASS`/`FAIL`/`NOT RUN`. Editor success cannot substitute for Android, and a
YooAsset success callback cannot substitute for outer hash/business admission.
Windows evidence remains `NOT RUN` until a separately authorized Windows
environment run; this does not invalidate Mac/Android Demo implementation, but
it forbids a Windows or cross-platform release-compatibility claim.

## 20. Impacts, risks, and decisions returned to the engineering lead

### 20.1 Expected impacts

- **Dependency:** one new direct immutable Git dependency and its declared SBP
  dependency, subject to a future authorized RES-01A. Its exact two package files
  are that packet's only permitted project mutation.
- **Public API:** new project-owned asset-access contracts are public only across
  project assemblies. Existing Core/Application/save/publication contracts do
  not change. YooAsset types remain private to the adapter.
- **Serialization:** no existing player-save or business serialization changes.
  A new outer update/ReleaseSet descriptor and device update-stage record require
  explicit schema/version/budget tests in RES-01D; they are not player state.
- **Assets:** bootstrap scene/prefab/fonts and remote collector config are future
  asset/meta changes and require exact whitelists. No asset or meta is changed by
  this design.
- **Operations:** R2/domain/credentials are deployment prerequisites, never
  repository configuration or reasons to embed secrets.

### 20.2 Principal risks

1. Package metadata's Unity floor is not project compatibility; compile/Android
   proof may expose SBP or API issues.
2. YooAsset CRC32/size detects corruption but is not release authentication.
3. `PauseDownload` naming invites a Q35 violation because active transfers keep
   running; abort is asynchronous and must be measured.
4. A server ignoring Range can append a full object to a partial. Final checks
   should reject it, but bandwidth and recovery behavior need evidence.
5. Cloudflare can cache 404s and stale overwritten keys; immutable naming and
   publish order are mandatory.
6. Leaked/misreleased handles can pin bundles or invalidate visible assets;
   project lease diagnostics and epoch tests are mandatory.
7. Bootstrap font/shader dependency mistakes can make the recovery UI unreadable
   exactly when remote content fails.
8. Full text/six-byte transport could accidentally become a second truth if Host
   skips existing admission; tests must prove the bridge rather than replacement.
9. Background execution, storage deletion, performance, and memory remain
   platform-dependent and unverified.

### 20.3 Decisions requested from the central/engineering review

1. Approve 3.0.6/full-SHA as the sole RES-01A probe pin; separately authorize
   package adoption only through RES-01A's exact two-file/Mac gate. Record that a
   pass unlocks B/C/D only as Mac/Android Demo implementation candidates, while
   Android, remote-delivery, Windows, and release acceptance remain later gates.
2. Approve one remote Yoo package (`FightMatchMain`) with tag scopes, rather than
   separate package lifecycles for text/UI/audio.
3. Approve the project `IFightMatchAssetProvider` + lease/result/diagnostic seam
   and a single YooAsset-only adapter assembly.
4. Approve a new outer ReleaseSet/update descriptor instead of expanding the
   existing six-file business `ContentReleaseSet` silently.
5. Assign the publication-trust/signature mechanism and operator ownership as a
   separate security/deployment decision before production; local Demo trust is
   not production authorization.
6. Freeze exact bootstrap asset/font licenses, filenames, GUIDs, and localization
   receipt before RES-01C dispatch.

No unresolved item above prevents review of this design. Items 1, 4, 5, and 6
do prevent their dependent implementation or production claim.

## 21. Delivery boundary and self-check

This RES-01 turn created/modified only
`docs/team/2026-09-30/engineering-yooasset-001.md`. It did not install YooAsset,
run Unity, download an SDK/package, create an R2 resource, use credentials,
modify assets/metas/settings/packages, resume the paused product goal, or write
Git. All implementation, local-server, cloud, and device tests are `NOT RUN`.

Final document verification must report path, line/byte count, SHA-256, and the
repository diff limited to this path. Engineering review should issue a concrete
accept/fix response before RES-01A is dispatched.
