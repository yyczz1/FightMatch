# RES-01A · YooAsset immutable package probe implementation packet

## 0. Packet state

| Field | Value |
|---|---|
| Task | `RES-01A` |
| Revision | `draft-01` |
| State | `PREPARED_NOT_AUTHORIZED` |
| Scheduling state | `WAITING_FOR_C_SERIAL_SLOT` |
| Execution state | `IMPLEMENTATION_NOT_RUN` |
| Sole future delivery owner | C, thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`, host `local` |
| Dispatch authority / return path | Engineering lead, thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b`, host `local` |
| Packet author | Architecture role, thread `01a0f2e6-1ac3-7d10-8855-54192b9489d4`, host `local` |
| Independent acceptance | Testing lead / QA identity must be bound at dispatch; the author and C cannot accept their own delivery |

This file is a prepared implementation packet, not permission to run it. It does
not resume the paused product goal. The engineering lead may sign and dispatch
it only after C's current uGUI delivery is complete, C's serial Unity slot is
free, and every dispatch-time placeholder in section 3 has been replaced with
observed evidence. An unbound placeholder means `BLOCKED`, not permission to
infer a value.

No Unity, UPM, Git, network, package installation, or product-file change was
performed while preparing this packet.

## 1. Fixed authority and decision

The only product-design authority for this packet is:

- `docs/team/2026-09-30/engineering-yooasset-001.md`
- SHA-256:
  `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`
- Identity: 859 lines / 62080 bytes

Before dispatch, the engineering lead must re-check that exact identity. A
different byte identity requires a new design receipt and packet revision.

The immutable package decision is fixed:

| Field | Required value |
|---|---|
| Package id | `com.tuyoogame.yooasset` |
| Release | `3.0.6` |
| Commit | `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` |
| Direct dependency URL | `https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` |
| Declared Unity floor | `2019.4` |
| License | Apache License 2.0 |
| Editor for this probe | Unity `2022.3.18f1`, macOS Intel (`x86_64`) Editor |

The package's declared dependency set to verify is:

- `com.unity.scriptablebuildpipeline` `1.21.25`;
- `com.unity.modules.assetbundle` `1.0.0`;
- `com.unity.modules.unitywebrequest` `1.0.0`;
- `com.unity.modules.unitywebrequestassetbundle` `1.0.0`.

No version selection is delegated to C. In particular, C must not try `3.0.5`,
another YooAsset version, a tag-only URL, a branch, or a shortened commit.

## 2. Result boundary

This task answers one question only: can the exact immutable YooAsset package
above be resolved, imported, identified, and compiled cleanly in the current
FightMatch project on the fixed Mac Intel Unity Editor without unrelated package
movement?

A successful result retains the accepted two-file package diff. It does not add
an adapter, resources, configuration, build content, runtime code, tests,
scenes, prefabs, localization data, Android settings, CDN settings, or release
logic.

Passing RES-01A unlocks RES-01B, RES-01C, and RES-01D only as candidates for
later Mac/Android Demo packets. It does not authorize any of them. It makes no
claim about Android IL2CPP or API 24, Windows, CDN/R2 delivery, production
release compatibility, or a Unity patch upgrade.

## 3. Dispatch-time bindings

The engineering lead must replace every token below before signing the packet.
Values must come from one fresh preflight after C's uGUI work is complete. They
must also be copied into the immutable run receipt at the evidence root.

| Token | Required binding |
|---|---|
| `[DISPATCH_AUTHORITY_RECEIPT]` | User authority and engineering-lead dispatch turn/receipt |
| `[C_DISPATCH_TURN_ID]` | Actual C turn receiving this exact packet revision |
| `[QA_THREAD_ID]` | Independent testing-lead/QA thread identity |
| `[QA_HOST_ID]` | QA host identity |
| `[PROJECT_ROOT]` | Canonical absolute FightMatch project path |
| `[UNITY_EDITOR_PATH]` | Canonical absolute Unity `2022.3.18f1` Intel Editor executable |
| `[UNITY_EDITOR_IDENTITY]` | Observed version, architecture, executable SHA-256, and install receipt |
| `[MANIFEST_BEFORE_SHA256]` | Actual SHA-256 of `Packages/manifest.json` immediately before execution |
| `[MANIFEST_BEFORE_BYTES]` | Actual byte count of that same file |
| `[LOCK_BEFORE_SHA256]` | Actual SHA-256 of `Packages/packages-lock.json` immediately before execution |
| `[LOCK_BEFORE_BYTES]` | Actual byte count of that same file |
| `[PREFLIGHT_GRAPH_RECEIPT]` | Create-once receipt containing the complete logical package graph before execution |
| `[PREFLIGHT_GRAPH_SHA256]` | SHA-256 of that graph receipt |
| `[ACTIVE_PROCESS_GATE_RECEIPT]` | Timestamped process-safety receipt proving this project is not open in another Unity process |
| `[ACTIVE_PROCESS_GATE_SHA256]` | SHA-256 of that receipt |
| `[BOUND_TWO_FILE_INPUT_IDENTITY]` | Exact `manifest-<manifest-sha256>__lock-<lock-sha256>` identity using the two bound lowercase SHA-256 values |
| `[CREATE_ONCE_EVIDENCE_ROOT]` | New, empty path resolving exactly to `[PROJECT_ROOT]/TestArtifacts/FightMatch/RES-01A/[BOUND_TWO_FILE_INPUT_IDENTITY]/[RUN_ID]/` |
| `[RUN_ID]` | Unique run identifier used by every receipt |
| `[TRANSIENT_INSTALLER_SOURCE_SHA256]` | SHA-256 of the fixed transient installer source before it enters the project |
| `[UPM_INSTALL_LAUNCH_ARGV]` | Exact direct-Editor install argv using the fixed `Install` entry point and evidence log path |
| `[UPM_ROLLBACK_LAUNCH_ARGV]` | Exact direct-Editor rollback argv using the fixed `Remove` entry point and evidence log path |
| `[COMPILE_LAUNCH_ARGV]` | Exact fresh-process compile/import argv and log destination |
| `[BOUND_INSTALL_TIMEOUT]` | One fixed install-resolution timeout |
| `[BOUND_COMPILE_TIMEOUT]` | One fixed success compile timeout |
| `[BOUND_ROLLBACK_TIMEOUT]` | One fixed rollback-resolution timeout |
| `[BOUND_ROLLBACK_COMPILE_TIMEOUT]` | One fixed rollback compile timeout |

The Unity 2022.3-compatible bootstrap is fixed rather than left to dispatch.
C temporarily creates
`Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs` with the
normal patch tool after the process gate. The first direct Editor import alone
may generate
`Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs.meta`.
Those are the only transient product-tree paths and both must be absent from
the accepted final tree. The package may not use Unity CLI live evaluation or
`com.unity.pipeline`: that path requires Unity 6.0+ and does not manage UPM for
this Unity 2022.3 project.

## 4. Exact write scope

### 4.1 Accepted final product-file whitelist

Only these two existing files may differ from their bound preflight identities:

1. `Packages/manifest.json`
2. `Packages/packages-lock.json`

The intended mutation must be produced only by Unity Package Manager's
asynchronous `UnityEditor.PackageManager.Client.AddAndRemove` request.
Hand-editing, scripted JSON editing, search/replace, or copying prepared package
files into place is forbidden.

### 4.2 Transient bootstrap whitelist

During an authorized UPM request only, C may create exactly:

1. `Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs`
2. `Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs.meta`
3. `Assets/Editor.meta`, only if `Assets/Editor/` did not exist at preflight;
4. `Assets/Editor/ProjectBootstrap.meta`, only if
   `Assets/Editor/ProjectBootstrap/` did not exist at preflight.

The source is created with the normal patch tool only after the serial/process
gate. C must not hand-create the first `.meta`; the first direct Unity Editor
import generates the source `.meta` and any required parent-directory `.meta`.
After each UPM Editor process exits, C archives exact copies and hashes of every
task-created file to the evidence root, then removes them with the normal patch
tool. If C created a parent directory that did not exist at preflight, C also
removes that now-empty directory after its generated `.meta` is archived and
removed. Every task-created path must be absent before the fresh compile and
from the accepted final tree; any parent path that existed at preflight must be
left byte-for-byte unchanged.

For a later independent-QA rejection or an immediate rollback, C may recreate
only that same task-owned tree from the archived byte-for-byte source and
`.meta` files. Their hashes must equal the first archived identities. They
exist only for the single removal request, are archived again, and are removed
before the rollback compile. No other `Assets` path is transiently authorized.

### 4.3 Zero-write and forbidden scope

Apart from the transient bootstrap paths above, there must be zero writes
to:

- `Assets/**`, including other scripts, assemblies, assets, scenes, prefabs,
  directories, and `.meta` files;
- `ProjectSettings/**`;
- every other `Packages/**` file;
- repository tools, documentation, plans, evidence indexes, generated project
  files, and Git metadata;
- user-managed files, including `.claude/settings.local.json`.

Do not delete or edit `Library/PackageCache`, do not clear package caches, and
do not reset or restore unrelated files. Git may be used read-only for frozen
preflight/final status and diff evidence; it may not implement, restore, clean,
checkout, reset, stage, commit, or otherwise mutate this task. Normal
Unity-generated transient state is not an accepted deliverable and cannot hide
a repository-scope violation.

Evidence may be written only within `[CREATE_ONCE_EVIDENCE_ROOT]`. The project
root `.gitignore` already ignores `/TestArtifacts/`; this evidence directory is
not part of the accepted product diff. It must not pre-exist and is
append-by-creation: no evidence filename may be reused or overwritten.

## 5. Isolation and scheduling gates

C is the sole Unity executor for this packet. Before any Unity process starts,
C must prove all of the following:

1. The uGUI delivery previously occupying C is complete and its owner has
   released the serial slot.
2. The authoritative design and both package-file before-identities match
   section 1 and the dispatch bindings.
3. The current logical package graph matches `[PREFLIGHT_GRAPH_RECEIPT]` and
   does not already contain YooAsset under another source or version.
4. No Unity process has this project open. If it is open, return `BLOCKED` and
   ask the user to close it; do not kill or attach to it.
5. No UGUI, localization, Android API-24, graphics, or other task can write the
   two package files or use this project's Unity process during this run.
6. `[CREATE_ONCE_EVIDENCE_ROOT]` is new and empty, and all intended direct
   Editor argv values are frozen in `run.json` before the first Unity launch.
7. A byte inventory of all `Assets/**` plus read-only Git status/diff evidence
   freezes C's already-present predecessor inputs. Those inputs may remain
   dirty but may not change during RES-01A.
8. The transient source hashes to `[TRANSIENT_INSTALLER_SOURCE_SHA256]`, has
   both fixed entry points, and can be created at the exact whitelisted path.

Any mismatch ends preflight as `BLOCKED`. It is not a failed probe and consumes
none of the resolution/compile budget.

## 6. Create-once evidence closure

Before execution, C creates `[CREATE_ONCE_EVIDENCE_ROOT]` exactly once and
writes a closed manifest naming every permitted evidence file. The closed set
is:

- `run.json` — run id, packet/design identities, owner/authority identities,
  frozen paths, hashes, time budgets, and exact argv arrays;
- `process-safety.json` — observed processes, project-path comparison, timestamp,
  tool identity, and gate verdict;
- `preflight-package-files.json` — both before SHA-256 and byte counts;
- `preflight-package-graph.json` — complete direct/resolved package graph and
  source/version information;
- `preflight-assets-inventory.json`, `preflight-git-status.txt`, and
  `preflight-git-diff.patch` — frozen Assets bytes, parent-path existence, and
  existing predecessor state before the transient bootstrap is created;
- `manifest.before.json` and `packages-lock.before.json` — byte-for-byte copies;
- `transient-installer.install.cs`,
  `transient-installer.install.cs.meta`,
  `transient-installer.install.editor-folder.meta`,
  `transient-installer.install.bootstrap-folder.meta`, and
  `transient-installer.install-identities.json` — exact source/generated-meta
  archive and hashes from the first direct Editor process; either folder-meta
  archive is `not_applicable` when that parent existed at preflight;
- `upm-install.stdout.log`, `upm-install.stderr.log`, and
  `upm-install.editor.log`, and `upm-install-result.json` — exact argv,
  request lifecycle, Editor log, and exit result;
- `manifest.after-install.json` and `packages-lock.after-install.json`;
- `accepted-package-diff.patch` — unified before/after diff for exactly the two
  whitelisted files;
- `resolved-package-graph.json` — full post-resolution graph plus graph delta;
- `yooasset-source-license.json` — requested URL, package id/version, resolved
  revision/source, package metadata hash, repository/license identity, and the
  immutable official source references from the authoritative design;
- `import-assemblies.json` — imported package path, all YooAsset `.asmdef` names
  and hashes, resulting Editor/runtime assembly names, and import diagnostics;
- `compile.stdout.log`, `compile.stderr.log`, `compile.editor.log`, and
  `compile-result.json`;
- `final-assets-inventory.json`, `final-git-status.txt`, and
  `final-git-diff.patch` — final Assets bytes/status comparison against the
  frozen predecessor state;
- `scope-receipt.json` — final hashes/bytes for the two accepted files, proof
  the transient paths are absent, and proof of zero other attributable writes;
- `installer-cleanup.json` — archived installer/meta identities, removal time,
  and proof all task-owned transient paths are absent;
- `final-verdict.json` — C's `PASS`, `FAIL_ROLLED_BACK`, or `BLOCKED` result and
  the exact evidence file hashes.

The following files are conditional and are declared now so failure evidence
does not expand the set:

- `upm-rollback.stdout.log`, `upm-rollback.stderr.log`, and
  `upm-rollback.editor.log`, and `upm-rollback-result.json`;
- `transient-installer.rollback.cs`,
  `transient-installer.rollback.cs.meta`,
  `transient-installer.rollback.editor-folder.meta`,
  `transient-installer.rollback.bootstrap-folder.meta`, and
  `transient-installer.rollback-identities.json` — recreated exact bytes and
  second archive/removal proof; folder-meta applicability matches install;
- `manifest.after-rollback.json` and `packages-lock.after-rollback.json`;
- `rollback-package-diff.patch`;
- `rollback-package-graph.json` and `rollback-graph-comparison.json`;
- `rollback-compile.stdout.log`, `rollback-compile.stderr.log`,
  `rollback-compile.editor.log`, and `rollback-compile-result.json`;
- `qa-rejection.json` — present only when independent QA rejects an otherwise
  completed install and triggers the authorized removal path.

`run.json` must mark each conditional file as `not_applicable` or record its
hash. No other evidence filename may appear. Logs must contain complete output,
must identify their argv without secrets, and must not be overwritten by a
second attempt.

## 7. Transient Editor bootstrap contract

`Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs` must be a
minimal, single-purpose Editor program with these fixed behaviors:

1. It confirms the target project path, packet/run id, Unity version, process
   architecture, and the two bound before-hashes before mutation.
2. The fully qualified install entry point is
   `FightMatch.Editor.ProjectBootstrap.FightMatchYooAssetProbeInstaller.Install`.
   It calls exactly one asynchronous request equivalent to:

   ```csharp
   Client.AddAndRemove(
       new[]
       {
           "https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804"
       },
       Array.Empty<string>());
   ```

3. The fully qualified removal entry point is
   `FightMatch.Editor.ProjectBootstrap.FightMatchYooAssetProbeInstaller.Remove`.
   It calls exactly one asynchronous request equivalent to:

   ```csharp
   Client.AddAndRemove(
       Array.Empty<string>(),
       new[] { "com.tuyoogame.yooasset" });
   ```

4. It polls the returned request from `EditorApplication.update`, records
   success/error/result data, enforces the bound timeout, unsubscribes its
   callback, flushes evidence, and exits through `EditorApplication.Exit`.
5. The install invocation must not use `-quit`; the asynchronous callback owns
   termination. It must not issue `Client.Add`, call `Client.Resolve` as a
   second resolution attempt, or manipulate JSON.
6. It never contains an alternate version/URL and cannot loop over candidates.
   It has no compile-time reference to YooAsset assemblies or types.
7. It emits one request receipt even on timeout, crash recovery, or API error,
   writing only inside the bound evidence root.

The source and both direct-Editor argv arrays are part of the signed input. The
argv shape is fixed as:

```text
[UNITY_EDITOR_PATH]
-batchmode
-projectPath [PROJECT_ROOT]
-executeMethod FightMatch.Editor.ProjectBootstrap.FightMatchYooAssetProbeInstaller.Install|Remove
-logFile [CREATE_ONCE_EVIDENCE_ROOT]/upm-install.editor.log|upm-rollback.editor.log
<fixed run/evidence/timeout arguments consumed by the installer>
```

There is no `-quit`. `[UPM_INSTALL_LAUNCH_ARGV]` and
`[UPM_ROLLBACK_LAUNCH_ARGV]` must expand this shape into exact non-shell argv
before dispatch. A source or command change after dispatch invalidates the run
and requires re-signing.

## 8. Ordered execution

### Phase A — immutable preflight

1. Create the exact ignored `[CREATE_ONCE_EVIDENCE_ROOT]` and its closed
   evidence manifest; do not overwrite an earlier run.
2. Capture the process-safety gate, current package graph, complete Assets byte
   inventory, and read-only Git status/diff including C's frozen predecessor
   inputs.
3. Copy and hash the two allowed package files; compare them with every bound
   before identity.
4. Capture the Editor version, architecture, executable identity, license
   readiness, disk space, and exact argv without starting Unity.
5. Reconfirm no placeholder remains. On any mismatch, preserve the create-once
   evidence and return `BLOCKED` without creating the transient source.

### Phase B — one UPM resolution

1. After the gates pass, create the signed source with the normal patch tool at
   `Assets/Editor/ProjectBootstrap/FightMatchYooAssetProbeInstaller.cs`; verify
   `[TRANSIENT_INSTALLER_SOURCE_SHA256]`. Do not create its `.meta` manually.
2. Launch `[UPM_INSTALL_LAUNCH_ARGV]` once using the fixed Mac Intel Unity
   `2022.3.18f1` Editor, direct `-batchmode -projectPath -executeMethod -logFile`,
   and no `-quit`. The Editor's first import generates the source `.meta`.
3. Let the installer issue the one install-mode `Client.AddAndRemove` request.
4. Wait for its terminal request result and Editor exit. Do not start a second
   Editor, retry the request, select another version, or repair JSON.
5. Immediately archive and hash the exact transient source and every generated
   `.meta`, then remove all task-owned files with the normal patch tool and any
   task-created now-empty parent directories.
6. Prove every transient path is absent. Copy the post-install package files
   and capture the exact two-file diff.

### Phase C — package and import acceptance

The install is acceptable only if all checks pass:

1. `Packages/manifest.json` contains one direct
   `com.tuyoogame.yooasset` dependency whose value is the exact full URL in
   section 1.
2. `Packages/packages-lock.json` records the dependency as Git-sourced at full
   revision `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`, with no floating ref.
3. Resolved `package.json` reports id `com.tuyoogame.yooasset`, version `3.0.6`,
   Unity floor `2019.4`, and exactly the four declared dependencies in section
   1 at the stated versions.
4. The graph delta introduces only YooAsset and any of its four declared
   dependencies that were absent at preflight: SBP `1.21.25` and the three
   listed Unity modules at `1.0.0`. Pre-existing declared dependencies remain at
   those versions. No unrelated direct dependency, package, source, depth,
   revision, or version moves.
5. `PackageInfo`/lock/resolved-path evidence agrees on Git source, package id,
   version, and immutable revision. The resolved package metadata hash and
   official repository/license references establish source identity and Apache
   License 2.0. If the `?path=` checkout omits the repository-root `LICENSE`,
   record that fact; do not fabricate or copy a license into the project.
6. Enumerate every YooAsset `.asmdef` from the resolved package, record its
   declared name/hash and the corresponding imported assembly name, and prove
   there are no duplicate-name or missing-reference import errors. The packet
   does not guess assembly names before resolution.
7. A repository-scope comparison finds the transient bootstrap already absent;
   every `Assets` byte and Assets-related Git status entry equals preflight,
   C's pre-existing predecessor inputs are unchanged, and the only new accepted
   product diff/status delta is the two whitelisted package files.

Any failed check enters phase E; it must not be normalized into success.

### Phase D — one fresh Mac Intel compile/import

After successful resolution and inspection, close the resolution process and
start one fresh Unity `2022.3.18f1` Intel process using
`[COMPILE_LAUNCH_ARGV]`. Capture separate stdout, stderr, and Editor log content.
Every transient source/meta/conditional parent path must already be absent when
this process starts, unless a parent directory existed at preflight.

Pass requires all of the following:

- the process is the bound Intel Editor and exits normally with code `0`;
- package resolution/import finishes;
- the Editor log contains no C# compilation error, package resolution error,
  assembly load error, or unresolved reference;
- the two package files and resolved graph still match the accepted phase-C
  state;
- every task-created transient path remains absent;
- every `Assets` byte and Assets-related read-only Git status entry equals the
  frozen preflight state, and all predecessor inputs remain unchanged;
- the only new accepted product diff/status delta is the two package files;
- no additional repository file was changed by this packet.

This is the only success-path compile. Do not run EditMode tests, a Player
build, Android/Windows validation, or a second “confirmation” compile.

### Phase E — failure or independent rejection rollback

Enter this phase after any post-mutation failure or after an independent QA
`REJECT`. The engineering lead must reacquire C's serial Unity slot before a
later QA-triggered rollback. Do not roll back a successful run merely to
simulate the probe.

1. Recheck process safety; an open project is `BLOCKED`, never a reason to kill
   Unity.
2. Recreate the exact archived task-owned source and `.meta` tree with the
   normal patch tool. Verify every applicable hash, then launch
   `[UPM_ROLLBACK_LAUNCH_ARGV]` once with the fixed direct Editor command and no
   `-quit`.
3. The `Remove` entry point issues one `Client.AddAndRemove` removal for
   `com.tuyoogame.yooasset`; it must not name SBP or Unity modules for removal.
   Let UPM resolve unused transitives. After the Editor exits, archive the
   transient tree again, remove every task-owned file and conditional empty
   directory, and prove absence.
4. Capture both package files and compare the complete logical graph with
   `[PREFLIGHT_GRAPH_RECEIPT]`. Logical equality is required; byte-for-byte JSON
   equality is recorded but is not substituted for graph equality.
5. Run one fresh rollback compile with the fixed Editor while the transient
   paths are absent, and capture its result.
6. Preserve all evidence. Do not delete `Library/PackageCache`, hand-edit either
   package file, restore copies over UPM output, reset user files, or invoke Git.

Rollback succeeds only when the logical graph equals preflight, the rollback
compile passes, and no out-of-scope repository change exists. Otherwise return
`BLOCKED_WITH_UNRESTORED_GRAPH` with exact evidence; do not improvise repair.

### Phase F — cleanup and return

1. Reconfirm the source/meta archives and prove every task-owned transient path
   is absent from the final tree.
2. Seal the evidence manifest with SHA-256 and byte counts for every present
   evidence file.
3. C returns a concise receipt to the engineering lead containing state,
   changed files, before/after identities, graph delta, resolved source/version,
   assembly list, compile result, cleanup proof, and evidence-root identity.
4. The engineering lead sends the sealed result to the bound independent QA
   owner. C and this packet's author do not issue acceptance.

## 9. Acceptance criteria

Independent QA may issue `ACCEPT` only when every item below is evidenced:

1. The owner, authority, design identity, before identities, Editor identity,
   process gate, argv, and create-once roots were bound before execution.
2. The final product diff contains exactly
   `Packages/manifest.json` and `Packages/packages-lock.json`.
3. The direct dependency is the exact immutable Git URL and resolves to package
   `com.tuyoogame.yooasset` version `3.0.6` at the full selected commit.
4. The only allowed dependency effect is YooAsset plus its declared SBP
   `1.21.25` and Unity-module dependencies; no unrelated graph movement exists.
5. Source and Apache-2.0 license identities are traceable to the immutable
   official source, and resolved package metadata agrees with them.
6. Resolved package path, import result, every package `.asmdef` name/hash, and
   resulting assembly names are captured without import/reference error.
7. One fresh Mac Intel Unity `2022.3.18f1` compile/import passed with exit `0`
   and clean relevant logs.
8. The transient installer source/meta entered only their exact bootstrap paths,
   were archived after each UPM Editor exit, and are absent from the final tree.
9. Evidence is complete, create-once, internally hash-consistent, and includes
   full argv/log/package-diff/process-safety/scope receipts.
10. No Android/API-24, Windows, CDN, release, adapter, runtime, asset, or other
    compatibility claim is presented as tested or accepted.

If success-path evidence is incomplete or any criterion fails, QA returns
`NEEDS_FIX` when a smaller evidence-only correction is safe, otherwise `REJECT`.
A `REJECT` triggers phase E; it does not authorize manual cleanup.

## 10. Resource and time ceiling

| Resource | Ceiling |
|---|---|
| Unity owners | C only |
| Concurrent Unity processes for this project | 1 |
| Install resolutions | 1 `Client.AddAndRemove` request |
| Success-path compiles | 1 fresh compile/import |
| Failure/rejection removals | 1 `Client.AddAndRemove` request |
| Failure/rejection compiles | 1 fresh rollback compile |
| Version candidates | 1: exact YooAsset `3.0.6` commit only |
| Product files allowed to change | 2 |
| Accepted final Assets / ProjectSettings / `.meta` changes | 0 |
| Transient bootstrap files | Exact source plus Unity-generated `.meta` only; all absent before compile/final receipt |
| Install resolution timeout | `[BOUND_INSTALL_TIMEOUT]` |
| Success compile timeout | `[BOUND_COMPILE_TIMEOUT]` |
| Rollback resolution timeout | `[BOUND_ROLLBACK_TIMEOUT]` |
| Rollback compile timeout | `[BOUND_ROLLBACK_COMPILE_TIMEOUT]` |

The four timeout tokens are mandatory dispatch bindings. A timeout is a failed
operation, not permission to retry. Infrastructure readiness may be
checked before mutation without consuming the budget, but must not resolve or
install packages.

## 11. Stop conditions

Return `BLOCKED` before mutation when any dispatch token is unresolved, the
design or preflight identities differ, C's slot is not free, this project is
open in Unity, the transient source cannot be created at its exact path, the
evidence root already exists, the fixed Intel Editor is unavailable, or
disk/license readiness is absent.

After mutation, stop expansion and enter rollback for any UPM error, timeout,
wrong URL/version/revision/source, undeclared package movement, license/source
identity mismatch, import/assembly problem, compile failure, out-of-scope write,
or evidence corruption. Do not test another version or alter the packet.

## 12. Required C return format

```text
RES-01A result: PASS | FAIL_ROLLED_BACK | BLOCKED | BLOCKED_WITH_UNRESTORED_GRAPH
Owner: C / 01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local / <turn-id>
Authority: <dispatch receipt>
Design identity: <sha256, lines, bytes>
Preflight identities: <manifest sha/bytes>; <lock sha/bytes>; <graph receipt sha>
Process gate: <receipt sha and verdict>
UPM request: <mode, request result, exit, elapsed>
Resolved identity: <id, version, full commit, source URL>
Graph delta: <exact nodes/edges changed>
Assemblies: <exact names and receipt sha>
Compile: <exit and compile-result sha>
Final product files: <path, before sha/bytes, after sha/bytes for each>
Rollback: <not applicable or result/graph comparison/compile>
Installer cleanup: <receipt sha and path-absent verdict>
Evidence root: <absolute path, sealed manifest sha>
Unrun claims: Android/API-24, Windows, CDN/R2, release compatibility, B/C/D
Return to: engineering lead 01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local
```

## 13. Packet-author static self-check

- Current packet creation writes only
  `docs/team/2026-09-30/engineering-yooasset-res-01a-implementation.md`.
- It does not modify the authoritative design or any product/package file.
- Future accepted product-file whitelist is exactly the two files in section
  4.1. The exact Editor bootstrap source and Unity-generated `.meta` are
  separately whitelisted only as transient inputs and must be absent from the
  accepted final tree; final `Assets`, `ProjectSettings`, and `.meta` changes
  are zero.
- The package id, release, full commit, Git URL, declared dependencies, Unity
  version, owner, and return path match the fixed request and design.
- Actual package-file hashes/bytes, live graph, active-process gate, exact
  create-once evidence root, launch argv, timeouts, C dispatch turn, and QA
  identity remain visibly unbound; none is fabricated here.
- Success retention, failure/rejection UPM rollback, logical graph restoration,
  no cache deletion, no user-file rollback, process safety, resource ceiling,
  and fresh compile requirements are explicit.
- RES-01A success only unlocks later packets as candidates and authorizes no
  follow-on implementation.
