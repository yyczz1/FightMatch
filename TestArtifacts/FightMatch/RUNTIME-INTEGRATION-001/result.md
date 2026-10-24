# RUNTIME-INTEGRATION-001

Current status: NEEDS_FIX — real BuildMac and normal Host startup succeeded; Scene five-field binding/save/close/reopen verified. GUI clicks produced no visible response, so language/save/gameplay acceptance remains open. Historical fake-test failures are preserved; their repaired cases passed focused-04 below.

Historical first preflight: BLOCKED_BEFORE_UNITY, not run; dependency restriction subsequently clarified below.

C turn: `01a13435-7e2c-7913-a47a-6a2a43bb41bd`; completion receiver: engineering.

- Process check succeeded; no Unity Editor or UnityPackageManager was present.
- Fixed editor: Unity 2022.3.18f1 Intel. Shared source candidate was selected; no projection substituted.
- All five requested fixture names and the FAD06 method were found. Exact proposed command/filter is in commands.txt. It did not run: no process/exit code, log or XML exists.
- Shared Library/PackageCache has no com.tuyoogame.yooasset package. Packages/manifest.json and packages-lock.json request Git commit 3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804.
- Selected external UPM cache TestArtifacts/FightMatch/RES-01A/P01/package-cache contains npm and registry packages only; Scriptable Build Pipeline 1.21.25 is available there, but the YooAsset Git payload is absent.
- Existing reusable payload: TestArtifacts/FightMatch/RES-01A/P01/projection/Library/PackageCache/com.tuyoogame.yooasset@3b4cfb36cc, package.json version 3.0.6, ordinary directory. It was inspected, not copied or executed.
- Starting normal UPM resolution with the shared cache incomplete could fetch the Git dependency. This dispatch prohibits downloads and manual Library changes, so Unity was not launched. This is an input-preparation limitation, not an automatic approval rejection or a code/test failure.

Minimal continuation: engineering may specify a bounded reuse of the existing fixed Git package in the shared project's derived package cache, or another verified offline UPM source. Keep manifest/lock/source unchanged. Then run the original focused command once; do not restart historical validation.

Build, staging, scene edits, GUI Play, save/reopen and device checks: NOT RUN.

Written only in this evidence directory: files-before.json, commands.txt, result.md. No source, scene, meta, package or ProjectSettings changes by C.

Current Assets/Packages/ProjectSettings differences from the preflight snapshot: [].

## Continuation

Previous preflight was NOT RUN, not a failed execution. Engineering removed its additional no-download limit only for the already-locked YooAsset commit. Normal UPM resolution is allowed; declarations remain fixed. C turn: `01a1343c-a423-7223-8cd6-a86febce3931`.

## Actual focused invocation result

- Unity ran once with the exact argv in commands.txt. Editor PID 83199 exited naturally with code 1 after 181.150028 seconds; 300-second timeout did not trigger. The Python recording command itself exited 0 after recording the Editor failure. No repeat or signal was issued.
- UPM resolved the existing package graph: Editor log first restored 51 cached packages, then registered 53, including the fixed YooAsset Git commit and Scriptable Build Pipeline 1.21.25. YooAsset cache package.json reports 3.0.6. Registry cache root remained the existing external RES-01A/P01/package-cache.
- UPM also logged missing `git lfs` during its normal clone (`git lfs pull --include Assets/YooAsset` returned 1), then returned project:resolve-packages 200. This warning is retained in upm-resolution.log; no Git LFS installation or package/reference change was attempted.
- Compilation stopped with one distinct compiler diagnostic (repeated in the log):

```text
Assets/Tests/EditMode/FightMatchHost/FightMatchRuntimeLocalizationIntegrationTests.cs(68,93): error CS0103: The name 'SavePurpose' does not exist in the current context
```

- Test XML absent: discovered/executed/passed counts are unavailable, not zero-test success. BuildMac, staging, scene fields, GUI Play/save/reopen, cold start and Android checks all NOT RUN.
- Both package declaration SHA256 values remained unchanged:
  - manifest.json: `741fbc8dab2f41e8acda1732a92fd5d6ca276e0f706557632d186ac070f8bc86`
  - packages-lock.json: `c65be7a6cb4308eb311c7dde8cdc5cc7d40b15a9fa6aa8553d76937a342777e9`
- After exit, Editor 83199, licensing child 83215 and UPM child 83220 were absent; no Unity Editor/UPM/shader process was found.
- Scope comparison: 0 existing Assets/Packages/ProjectSettings files modified or removed; Unity naturally added 16 meta files, listed in file-changes.json. One extra natural meta is `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`, outside the explicit new-fixture list. It is preserved and reported for engineering scope receipt, not silently deleted. No product source or assertion was edited by C.

Return to engineering for the minimum source correction at the reported test line, then only necessary focused revalidation. SavePurpose is declared in FightMatch.Core/SaveEnvelope.cs; this test currently imports FightMatch.Platform but not FightMatch.Core. No patch was made.

## Focused-02 after the minimum source correction

C turn `01a13445-1176-75c2-831f-9469bf178be8`. Author qualified SavePurpose at line 68; C did not edit source/assertions. Corrected test SHA256: `4b81ec26afef6a4392ec504259a91bc60df7549325ebc57e9f3fe1c842802a0c`.

RUN — exact second argv in commands.txt. Unity Editor PID84891 exit **2**, elapsed **47.929063 seconds**, no timeout/signal/retry within this invocation. Compilation completed and UTF produced focused-tests-02.xml. XML: **111 total, 103 Passed, 8 Failed, 0 skipped/inconclusive**, test duration 7.565625 seconds. Filter coverage exactly the five new fixtures plus only the old FAD06 method.

Fixture results:
- FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests: {"Failed": 3, "Passed": 2}.
- FightMatch.AssetAccess.Tests.FightMatchResourceBuildTests: {"Passed": 2}.
- FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeTests: {"Passed": 1}.
- FightMatch.Host.Tests.FightMatchLocalizationCatalogTests: {"Passed": 94}.
- FightMatch.Host.Tests.FightMatchResourceBridgeTests: {"Failed": 5}.
- FightMatch.Host.Tests.FightMatchRuntimeLocalizationIntegrationTests: {"Passed": 4}.

Failures (exact XML messages):
- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CancelWaitsForActualPendingRawAndReleasesIt` — Focused fake SDK operation did not finish in 240 frames.   Expected: True   But was:  False
- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CorruptRawNeverOpensAssetsAndClosesPackage` — Focused fake SDK operation did not finish in 240 frames.   Expected: True   But was:  False
- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.InspectDoesNotStartSdkAndAssetsWaitForTransport` — Focused fake SDK operation did not finish in 240 frames.   Expected: True   But was:  False
- `FightMatch.Host.Tests.FightMatchResourceBridgeTests.CancellationAfterAcquireReleasesLateLeaseAndPublishesNothing` — Method has non-void return value, but no result is expected
- `FightMatch.Host.Tests.FightMatchResourceBridgeTests.FormalTextAndSixBusinessFilesProduceDataReadyAndReleaseEveryLease` — Method has non-void return value, but no result is expected
- `FightMatch.Host.Tests.FightMatchResourceBridgeTests.WrongTextOrBusinessBytesNeverPublishPartialData("fm.text.full.manifest","RES_TEXT")` — Method has non-void return value, but no result is expected
- `FightMatch.Host.Tests.FightMatchResourceBridgeTests.WrongTextOrBusinessBytesNeverPublishPartialData("fm.text.full.artifact","RES_TEXT")` — Method has non-void return value, but no result is expected
- `FightMatch.Host.Tests.FightMatchResourceBridgeTests.WrongTextOrBusinessBytesNeverPublishPartialData("fm.content.package","RES_CONTENT")` — Method has non-void return value, but no result is expected

The three runtime failures point to `FightMatchBuiltInResourceRuntimeTests.cs:120`. The five bridge cases carry NUnit label `Invalid` and the non-void-return diagnostic; those cases did not execute successfully. No diagnosis is being substituted for source fixes or passing runtime evidence.

Package manifest/lock SHA256 unchanged. Post-run observation found no Unity Editor, UPM or shader process. Existing 16 natural meta files retained; engineering explicitly received LocalePreferenceStoreTests.cs.meta GUID46f54db8bfeb44d77af7d729d788d8e1. File comparison adds only the author's authorized test-source correction relative to the initial baseline, with full delta in file-changes-02.json.

BuildMac, staging, scene fields, GUI checks, save/reopen, cold start and device acceptance remain NOT RUN because the focused tests did not all pass. Return original XML/log to engineering for minimum corrections; no product/assertion edits or unrequested repeated validation by C.

## Focused-03 preparation

C turn `01a13452-357a-7750-909d-2119d1f6fea5`. Engineering received the minimum test-only FIX02. Restored the four explicitly named historical HCP/HISO files with apply_patch, byte-identical to the supplied PR19 base context. Original GUIDs preserved.

- Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs: 25128 bytes; SHA256 `c34c86a9686e85c145143a0294adbe0964b226a492dda76997732e0621d9ab90`.
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs.meta: 243 bytes; SHA256 `3772247bb6d5f6883840c20f6c5423341bd0fa9e43c60e3d3e4f6dcc65778273`.
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs: 15710 bytes; SHA256 `ed176ec6e87742359cfcd7767236619496180b25ad9783563582235ca1f8014d`.
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs.meta: 243 bytes; SHA256 `b6ecf5844647902cd3499b55c8948e0f2925bbdef9b379bfe13b2aa23e888535`.

## Focused-03 actual result

RUN — exact third argv in commands.txt. Editor PID87223 exit **2**, elapsed **42.675418 seconds**, no timeout or signal. XML: **16 total, 13 Passed, 3 Failed, 0 skipped/inconclusive**; test duration 6.8525882 seconds.

- FightMatchHostLocaleWiringTests: **8/8 passed**.
- FightMatchResourceBridgeTests: **5/5 passed**.
- The three selected FightMatchBuiltInResourceRuntimeTests: **0/3 passed**.
- Restored FightMatchHostSaveIsolationTests was compiled but not included in the execution filter. Prior 103 passing results were not rerun; this is scoped evidence, not a same-head full-suite claim.

All three runtime failures have this exact XML diagnostic: `Fake SDK task is still pending after its queued work drained. Expected: True But was: False`. Stack: Dispatcher.Complete at FightMatchBuiltInResourceRuntimeTests.cs:150, called at:

- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CancelWaitsForActualPendingRawAndReleasesIt`

```text
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests+Dispatcher.Complete (System.Threading.Tasks.Task task) [0x00008] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:150
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CancelWaitsForActualPendingRawAndReleasesIt () [0x0012c] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:99
```

- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CorruptRawNeverOpensAssetsAndClosesPackage`

```text
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests+Dispatcher.Complete (System.Threading.Tasks.Task task) [0x00008] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:150
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.CorruptRawNeverOpensAssetsAndClosesPackage () [0x00101] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:121
```

- `FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.InspectDoesNotStartSdkAndAssetsWaitForTransport`

```text
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests+Dispatcher.Complete (System.Threading.Tasks.Task task) [0x00008] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:150
at FightMatch.AssetAccess.Tests.FightMatchBuiltInResourceRuntimeTests.InspectDoesNotStartSdkAndAssetsWaitForTransport () [0x001e8] in /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchAsset/FightMatchBuiltInResourceRuntimeTests.cs:71
```

Manifest/lock unchanged. No Editor/UPM/shader process remained in the post-run check. The only modified pre-existing files relative to the original snapshot are the three test sources changed by the authorized authors. C restored the exact four historical files listed above; existing natural meta/GUIDs were retained. Full file delta: file-changes-03.json. No production source, Scene, existing meta, package declaration or ProjectSettings change by C.

BuildMac, staging, Scene pin/fields, GUI, save/reopen and device validation remain NOT RUN. Return the three precise failing XML cases to engineering; C made no assertion or production fix and performed no repeat without a changed input.

## Real BuildMac continuation

Engineering conveyed central removal of the three-fake-tests gate in favor of actual build/manual validation. All three focused-03 failures remain FAILED; final fake-only correction is UNTESTED. No focused-04 run. Production source unchanged. C turn `01a13459-ab52-78d1-9976-5550aa35da81`.

Real BuildMac: PASS, Editor exit0, 31.687276 seconds. Independently verified canonical boot/descriptor/mapping/receipt/scope hashes, eight current input bytes and all 14 actual SDK inventory leaves. Six original business files unchanged. Staged only inventory playerFiles plus boot TextAsset; natural meta import pending.

Boot SHA256: `5c1d6721f9e1c2d8bc3b45dce5aabfe5e730bcf66fcd02f511ba765010093b5b`; descriptor SHA256: `50093841baa1e948f9b22382f77b592d3c2be2053e2e43020b7b5e35230cb2c6`.

## Actual GUI and final receipt

- Normal Editor PID88800 launched with the exact HISO args in commands.txt; normal Play button invoked Host.Start. GUI exit0 after 701.686771 seconds including loading, initial CUA application read (120 seconds), inspection and normal close. No additional test or build invocation.
- Boot TextAsset GUID `ce31e6ed903a84cd89483cf21b47878f`, actual boot pin `5c1d6721f9e1c2d8bc3b45dce5aabfe5e730bcf66fcd02f511ba765010093b5b`, set `fm.builtin.r1.macos`, platform `macOS`, protocol1 were visible in the real Inspector. Final Scene delta is exactly those five fields (scene-binding-final.diff); original Scene GUID, font/runtimeRoot and Canvas/Prefab records retained. Cmd-S saved the Scene; Play was stopped, Scene closed to an unsaved Empty scene, then the same Demo was reopened read-only and Inspector references re-observed. The reopened Scene became dirty automatically; Quit → Don't Save discarded those in-memory changes.
- Real package loading and normal Host Session startup succeeded. gui-editor.log:655 records the isolated productRoot. Visible page was 本机资料, with 还没有本机资料。创建后即可开始冒险。, 创建本机资料, 继续原创建, 重新读取, 返回, 语言, 查看字体许可. Observed page had positive geometry, legible Chinese and no diagnostic placeholder in Play.
- Actual rendering was observed beyond a frozen image: Game Statistics changed from 698.3 FPS to 266.9 FPS, at720x1280. Normal startup changed placeholder content to the localized page.
- CUA clicked 创建本机资料 and 语言; no visible page change. A 200ms press and raising the window did not establish a response. Stop here: this is failed/unconfirmed interaction evidence, not proof of the root cause; native automation delivery versus actual EventSystem/raycast wiring still needs distinction. No reflective method invocation or direct business call substituted for clicking.
- Editor No Cameras Rendering overlay was temporarily disabled through its normal Game-view menu to inspect the overlay Canvas, then restored. Console showed0 errors/1 warning: DownloadSchedulerOperation has been aborted (gui-editor.log:670), retained without inferring a production failure cause.
- SDK produced its normal derived Editor cache under Library/yoo/FightMatchMain; no manual Library edits. Host activation and locator state are under the explicitly isolated root. No player profile or locale-preference file was created. No real player save was opened or edited by C.

Manual checklist outcome:

1. Normal resource/Host startup and visible local-profile page: observed; input response did not pass.
2. Initial visible Chinese page: placeholders replaced and legible; map/inventory/popups unverified.
3. Language switch: not verified; click did not open language controls.
4. Language preference save / Play reentry / process cold start: not performed because interaction did not progress. Scene close/reopen above is reference persistence only, not player save evidence.
5. Fresh isolated startup displayed Chinese; full system-language/default/override matrix unverified.
6. Map/battle entry and gameplay: not reached.
7. Player progress/currency/reward save and no-duplication: not reached; isolated root remains without a player profile.
8. Android device: not performed.

Screenshots are preserved as the native inline CUA outputs in this C turn: initial placeholder view; bound Inspector; Chinese local-profile page after normal Play; no-response page; two changing Statistics observations; read-only reopened Inspector. No standalone PNG was exported.

Automatic Unity asset side effects were contained after exit: dynamic TMP font changed from8573334B to8574938B; its post-GUI copy is font-after-gui.asset, and the exact preflight bytes were restored from the matching existing RES-P01 copy (SHA290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0). Newly generated SceneTemplateSettings.json was archived here and removed from ProjectSettings, where it did not previously exist. No package/settings or existing meta differences remain. Exact final changed files are in file-changes-final.json, including author test corrections, four authorized restored historical files, generated inputs/staging/boot and natural meta.

No runtime-source or assertion patch by C. Build success and initial normal Session startup are evidence for the Scene-binding correction, but the independent P1 is not declared closed because successful actual interaction remains unproven. Return to engineering for a minimal input/automation distinction and any justified correction.

Final Scene identities:
- {"path": "Assets/Scenes/FightMatchDemo.unity", "bytes": 13158, "sha256": "0d6f39a9d99392139fc956e084294a1c9a39e7d602dfd8140f650a5e71d93a88"}
- {"path": "Assets/Scenes/FightMatchDemo.unity.meta", "bytes": 155, "sha256": "2f2cfa8c3f7d833ff3694fbfd20c45643e40c465e7209d6040fba48d76f37e7e", "guid": "1d5124e5b55fe409d8216e78a117dec0"}

Original CUA screenshot call IDs: `call_fc513e02a6eb4662bd7537cf9bcfc3f6` (runtime page/698.3FPS), `call_c86473af650847c1a5569bfbe8d208bc` (266.9FPS), `call_2702467ad15c41b2940b2ea033e259da` (read-only reopened Inspector). Final publish delta uses file-changes-final.json; the earlier build-gui list intentionally retains the now-restored font/generated settings side effects.

## Focused-04: three repaired cases only

C turn `01a1347a-6402-7a63-93dd-588857b9d821`, Editor PID92333. RUN exact argv in commands.txt: normal exit **0**, **31.743277 seconds**, no timeout/retry. XML **3/3 Passed, 0 failed/skipped/inconclusive**, duration1.1207511 seconds. Executed only InspectDoesNotStartSdkAndAssetsWaitForTransport, CancelWaitsForActualPendingRawAndReleasesIt and CorruptRawNeverOpensAssetsAndClosesPackage. New evidence focused-tests-04.log/xml and focused-execution-04.json; prior runs retained.

After exit, frozen Fake/Scene/boot hashes and all232 recorded source/resource identities were unchanged; package manifest/lock unchanged. No Unity Editor/UPM/shader process remained. No product/source/Scene/meta edit by C this round.

Engineering's in-flight sequence correction deferred the manual GUI window until the platform P1 correction is frozen. GUI was not opened and is NOT READY. No extra test or build. Return to engineering for platform correction and later manual-window continuation; the prior interaction issue remains unresolved.

## PLATFORM-BOOT-FIX-01 stage A

C turn `01a13497-1f12-7620-bfa9-942213293479`. Six author-provided SHA256 values matched. Initial read-only process formatting command had a Python NameError before any Unity or writes; corrected read-only check found no Editor/UPM. New Android/Windows outputs absent. Scope baseline recorded; no source edits by C.

Stage A completed, with exactly one focused test invocation and one serial build per new platform. Fixed Intel Editor2022.3.18f1, existing UPM/Bee cache paths; no GUI or target switch.

| Invocation | PID | Editor exit | Elapsed seconds | Outcome |
| --- | --- | --- | --- | --- |
| focused-platform-01 | 95732 | 0 | 38.409911 |14 cases:13 Passed,0 Failed,1 Windows-native Ignored on Mac |
| BuildAndroid | 95872 | 0 | 18.322316 |RawFile build succeeded |
| BuildWindows | 96153 | 0 | 28.722599 |RawFile build succeeded |

All three ended normally, without timeout, signal or retry. The ignored case is WindowsHostPathsUseExistingGuardsWithoutMacIsolation; it does not count as Passed. Original logs retain licensing-client signature/access-token diagnostics followed by successful entitlement resolution; no compile/build failure occurred. A read-only ps call was sandbox-denied during receipt collection; an approved process read established no overlap, without rerunning Unity.

Actual-file checks are recorded concisely in platform-verification.json. Each platform inventory contains exactly14 plain leaves totaling164315B under its own build/<platform>/builtin/FightMatchMain. All source paths, destination prefixes, lengths and hashes match; no links, escape or extra/missing leaf. All8 raw files equal their8 source inputs. Boot10 physical records, canonical descriptor/mapping/receipt/scope hashes, platform/set/protocol and formal text identities match. The original inventory.json files retain the complete14-leaf source/destination/length/SHA lists.

Android candidate:
- boot SHA256:7819a00bba3f175908080fd590623c1b81f498098f219ad43b0f3357621960c6
- inventory SHA256:dbc22c112bf170e85ef5a9e7a0bc3ed98cbf3f8aea47fd8c33cd30adee13b70f
- descriptor SHA256:4b7448a020f10f48952db621bd6121345926f24c25c1e8bd629d14b99130d29c
- set/platform/protocol:fm.builtin.r1.android / android /1

Windows candidate:
- boot SHA256:1da6bc5daedb5f2f2d916ba772ff7f411f87f1f043cbc3a33c8f462cb6d5904e
- inventory SHA256:ff25e702c07888f9e1908760a1ecabc5adb2d4a73f09063d20b9d675d0781617
- descriptor SHA256:0e19cf995f8cbe26ef6c4aff70cc1c96aca48fff9e0268a039d8201438a9b73c
- set/platform/protocol:fm.builtin.r1.windows / windows /1

Final snapshot: all6 frozen sources match author hashes;1105 Assets/Packages/ProjectSettings files have no additions, removals or byte changes relative to platform-before.json. This includes Scene and meta. All37 Mac output files remain identical; Mac was not rebuilt or moved. Generated formal text remains identical to the two input copies. Fresh final process check found no Editor/UPM/shader compiler.

Stage A returns to engineering, the sole completion receiver, for independent pin acceptance and scoped stage B assembly/GUI dispatch. Engineering's Android independent receipt (chunk d46ee6) agrees with the candidate identities above. This is resource-build evidence only: Windows Player/native storage, Android device and the previous actual GUI interaction gap remain open. No Scene binding or new manual window was performed in stage A.

## PLATFORM-BOOT-FIX-01 stage B — assembly complete, GUI approval blocked

C actual turn `01a134a3-6f49-7ec1-aaad-e33e5c85a073`. Mac14 staging leaves and all29 file/folder meta-bearing records migrated with identical bytes; two new14-leaf platform sets and boots copied from independently accepted inventories. Standard AssetDatabase.Refresh ran exactly once: PID96865, exit0,18.239664s, no timeout. Unity generated all new meta. Scene changed only the superseded5 resource fields into mac/windows/android boot/pin/set structures; original Scene.meta retained. File delta relative to platform-before.json:65 new files,29 byte-identical moves,1 modified Scene,0 other deletions. Exact closure and hashes: file-changes-platform-final.json; scene-platform-binding.diff. Six frozen sources, original Mac build/inventory, packages/settings/UI/Prefab/font unchanged. No test/build rerun.

New isolation ID `9b94439178924b9e858784467e7e32d7`; candidate `2e6b642118eee2abdbbb7377a143d092fefcc8979e067b44f796708d2903e6a0` covers585 current source/resource/Scene input identities. Activation and empty owner.lock prepared under the binding in gui-platform-inputs.json; persistent root remains empty. Old GUI evidence and isolation remain intact.

GUI NOT READY; PID none, launches0. Two exec approval submissions were rejected before process creation. First rejection considered stage-B delegation untrusted and only N07 trusted. Read-only central read_thread then exposed the actual human userMessage01a13404-223c-7893-b069-dae699e44e19 within turn01a13404-21d1-7262-abae-9d52295727c2 authorizing simplified game integration/manual acceptance without per-step confirmation. The same launch was resubmitted to automatic review with that evidence; it was again rejected because thread-tool authorization was considered untrusted. No alternate route or further retry. Direct human approval in this chat is required to unblock this tool action. No page/Console/frame/keyboard/manual result is claimed. Full blocked receipt:gui-platform-result.json.
