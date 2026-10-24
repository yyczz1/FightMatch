# LAYOUT-001 implementation delta draft

2026-10-01（Asia/Shanghai）

Status: `DRAFT_READY_NOT_AUTHORIZED`

Owner: SYS-ENG-001 system-design owner

- thread / host / turn: `01a0f2e6-29bb-7092-abf0-705b41b7bf93` / `local` / `01a0f59e-13c3-7310-81bc-1c1472b5b916`
- return path: engineering lead `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`
- deliverable: this planning document only
- implementation, Unity, test, resource, Git, APK, device, cloud, and release authority: **none**

## 1. Purpose and accepted inputs

This is a short execution delta for a later task packet. It does not repeat the accepted hierarchy, geometry, ownership, Caption, or lifecycle design. A future implementer must read the base and correction together; this draft only narrows file scope, owner order, preimage slots, selectors, and stop gates.

Mechanically verified inputs:

| input | lines | bytes | SHA-256 | role |
|---|---:|---:|---|---|
| `central-layout-hierarchy-adjudication-001.md` | 69 | 7,011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` | central hierarchy and layer adjudication |
| `engineering-ugui-layout-system-design-001.md` | 961 | 68,763 | `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce` | accepted base design clauses not superseded by the correction |
| `engineering-ugui-layout-system-design-correction-001.md` | 701 | 50,752 | `9c3d6d60b1a9ed1743f0920cd80204fbe82f6558da35e5607e88bf88d988e540` | accepted correction and exact 26-path maximum scope |
| `testing-ugui-layout-design-review-fix-002.md` | 89 | 8,751 | `8c20bfd3a8dff10f678cdea4b821a716cfc7115d5864b866d45c087f3e499e32` | independent design `ACCEPT`; implementation/TMP/Unity not accepted |
| `art-ugui-layout-design-receipt-fix-002.md` | 20 | 2,241 | `0c7fa6f520e7c8db4bc4f6f61a60df32dc81f2dd983701d1885ced2188fa0c27` | N04T design-only `ACCEPT`; actual visual/scroll acceptance remains open |
| `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md` | 70 | 10,210 | `360fb117d8f85a22c13eef62a28d66f9315f26d2611858c08439ff394bb1837b` | runtime uGUI, localization, resource, device, and R constraints |
| `AGENTS.md` | 260 | 14,834 | `602f74cbd568d4f5eb00d9cf0a83cb67a5732ecc09b28cc823d2ca446a44cde8` | repository authority, protection rules, and five-role workflow |
| `START_HERE.md` | 95 | 18,747 | `6384037dd260725f90a318c7637df957f2b740954abfbe20b1013a49bb28ecce` | current continuation and delivery state |
| `.agent/PROJECT_CONTEXT.md` | 144 | 25,348 | `9f1bf3dbab2fd2e25ea2c1d6b9c55c7b15ebe661867cc99e233d1ccd999f421e` | current observed/approved/unknown separation |
| `.agent/TEAM_WORKFLOW.md` | 132 | 9,567 | `48a289eecf737bf8d90c42f90640c09ac070efa58d282df0bdc3c548931a4480` | owner, C, QA, art, and R handoff rules |

Repository authority remains the current user request, `AGENTS.md`, `START_HERE.md`, `.agent/PROJECT_CONTEXT.md`, `.agent/TEAM_WORKFLOW.md`, and the accepted inputs above. C remains the sole Unity executor.

## 2. Dispatch gate and mechanical fill slots

At draft time the functional uGUI chain has no final independent R `ACCEPT`. The latest read-only team record is FIX16 `BLOCKED_HOST30`: Host30 was 28/30; Core190, native reopen, and final R did not run. This observation is not a frozen implementation preimage and may be superseded only by an actual completion receipt.

The engineering lead may convert this draft into an executable task packet only after all slots below are mechanically filled from the finally accepted functional candidate:

```text
FUNCTIONAL_R_RECEIPT_PATH         = [TO_FILL]
FUNCTIONAL_R_THREAD_TURN_FINAL    = [TO_FILL]
FUNCTIONAL_R_VERDICT              = ACCEPT
FUNCTIONAL_CANDIDATE_SHA256       = [TO_FILL]
FUNCTIONAL_SOURCE_CANDIDATE_SHA256= [TO_FILL]
FINAL_SOURCE_PREIMAGE_MANIFEST    = [TO_FILL path / lines / bytes / SHA-256]
FINAL_RESOURCE_PREIMAGE_MANIFEST  = [TO_FILL path / lines / bytes / SHA-256]
PREFAB_META_SHA256 / GUID         = [TO_FILL; GUID must remain c36df3cfc25424c8cb3ec6cae6be1237]
SCENE_META_SHA256 / GUID          = [TO_FILL; GUID must remain 1d5124e5b55fe409d8216e78a117dec0]
FONT_PREIMAGE_SHA256              = [TO_FILL]
LOCALIZATION_SNAPSHOT_SHA256      = [TO_FILL]
EXACT_ACCEPTED_222_MANIFEST       = [TO_FILL path / count / SHA-256]
EVIDENCE_ROOT                     = [TO_FILL create-once path]
UNITY_EXECUTABLE_IDENTITY         = [TO_FILL Intel Mac 2022.3.18f1 path / SHA or version receipt]
SOURCE_DIFF_BUDGET                = [TO_FILL after final preimage inspection]
```

L0 records line/byte/SHA for every existing path below and absence for every new path. It does not copy any current WIP hash into these slots. Any mismatch, missing final R receipt, or non-`ACCEPT` verdict leaves this document non-executable.

## 3. Exact maximum whitelist

Everything not listed in §3.1 or §3.2 is forbidden. The later task packet may reduce this set but may not add a 27th path.

### 3.1 Planned required paths

These changes are mechanically implied by the accepted design. The final packet must still bind their preimages or absence before writing.

#### Production source and resource builder

1. `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` — build the accepted hierarchy and serialize exact references only; no Android settings/build-pipeline change.
2. `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` — responsive-layout reference and Host-only shared outer-root/mask ownership.
3. `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs` — accepted status/action/drawer/panel containers and cleanup ownership.
4. `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` — accepted Reference Header/Body mapping.
5. `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs` — accepted confirmation/recovery panel, ChoiceRows, and OriginalResult lifecycle.
6. `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` — fixed slots and presentation-only unsupported enemy layout.
7. `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs` — fixed Stage labels and accepted playback mappings.
8. new `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs` — sole responsive Battle/page rect and LayoutDiagnostic owner.
9. new `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta` — only C's authorized Unity import may generate it.

#### New layout tests

10. new `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs` — the accepted 12 exact `LAYOUT_*` cases.
11. new `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta` — only C's authorized Unity import may generate it.

#### Unity resources

12. `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` — native Unity save only.
13. `Assets/Scenes/FightMatchDemo.unity` — native Unity save only; retain exactly one Runtime Prefab instance and one project EventSystem.

### 3.2 Conditionally permitted existing test/support paths

Each path is writable only when the final preimage inspection proves that its fixture, serialized reference, or existing assertion must change for the accepted hierarchy. No business expectation may be weakened. An unchanged file stays byte-identical.

14. `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs`
15. `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs`
16. `Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs`
17. `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs`
18. `Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs`
19. `Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs`
20. `Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs`
21. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs`
22. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs`
23. `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`
24. `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs`
25. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs`
26. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs`

### 3.3 Forbidden

- existing Prefab/Scene `.meta` files; their bytes and GUIDs remain unchanged;
- `SafeAreaFitter.cs`, `FightMatchViewId.cs`, localization CSV/config/generated data, TMP/font/license files, Core/domain/application/save/input/gesture code, asmdefs, packages, locks, ProjectSettings, CI, build-release settings, and every unlisted path;
- any new public interface, persistence/serialization change, localization key/copy edit, dependency, Canvas, EventSystem, Runtime Prefab, or regenerated stable GUID;
- handwritten Prefab/Scene YAML, broad formatting, unrelated cleanup, or deletion of old runtime UI resources;
- API24, GLES3, APK, YooAsset, Luban, account, ad, CDN, or release work in this packet.

## 4. Owner and resource-generation order

| order | owner | permitted action | required receipt / stop |
|---:|---|---|---|
| 0 | engineering lead | after functional R `ACCEPT`, fill §2 and issue a separate executable packet with this reduced whitelist and exact command/run budgets | missing token or 27th path → stop before dispatch |
| 1 | C | with Unity closed, apply only authorized source/test edits; add the two new `.cs` paths but do not hand-author Unity resources or `.meta` | no-Unity scope/static checks; no product claim |
| 2 | C, sole Unity executor | process-safety check, then one authorized Unity 2022.3.18f1 compile/import; Unity may generate only the two explicitly authorized new `.meta` files | compile/import failure, unexpected importer write, or another project Editor → stop |
| 3 | C | invoke the existing `FightMatchAndroidBuild` construction seam after compiled types exist; build the accepted hierarchy, fields, ScrollRects/LayoutGroups, responsive owner, placeholder text, and exact references | builder may write only the Prefab and Scene; no Android setting or other asset write |
| 4 | C | save Prefab/Scene natively, close/unload, reopen both, and verify references/components/GUIDs/overrides | no missing refs/scripts; one Canvas/GraphicRaycaster/EventSystem/Prefab instance; otherwise stop |
| 5 | C | freeze post-resource source/resource identities and the six exact-name manifests before the first focused run | manifest drift or duplicate fullname → stop; never silently recalculate expected counts after a run |
| 6 | C | run authorized focused and controlled selectors in §5, then one final native reopen/audit | any failure/skip/inconclusive/error or identity drift stops descendants |
| 7 | QA / art / same independent R | inspect exact evidence; retain actual Android/L1 and implemented-art gates | author cannot self-accept; no layout completion until required independent verdicts arrive |

The builder is an adapter at the resource-construction seam; `FightMatchResponsiveLayout` owns responsive geometry behind its serialized interface. No view, fixture, or builder becomes a second responsive writer.

## 5. Staged acceptance and selectors

No command is authorized by this draft. A future packet supplies exact Mac commands, time/process limits, result/log paths, and expected exit handling.

| gate | selector / procedure | binary acceptance |
|---|---|---|
| L0 | final functional R `ACCEPT`; §2 tokens; exact 26-path preimage/absence and resource meta/GUID checks | every token filled and matched; otherwise stop |
| L1 | no-Unity whitelist, static hierarchy/reference/raycast/single-axis ownership, Caption arithmetic, and forbidden-change checks | zero violations; no Unity claim |
| L2 | C compile/import only | exit 0 and no compiler/import error |
| L3 | C native construct → save → close/unload → reopen | saved resources satisfy the accepted hierarchy and identity rules |
| L4 | freeze `accepted-baseline-222`, `layout-new-12`, `layout-impacted-existing-32`, `layout-resource-existing-3`, `layout-focused-47`, and `layout-controlled-full-234` | counts 222/12/32/3/47/234; impacted32/resource3 subsets of 222; new12 disjoint; exact escaped fullnames |
| L5a | exact union of impacted32 + resource3 + `LAYOUT_01`, `LAYOUT_04`, `LAYOUT_06`, `LAYOUT_09` | exactly 39/39, zero failed/skipped/inconclusive/error |
| L5b | the eight exact fullnames below, true `ScreenSpaceOverlay` + enabled 540×960 ScaleWithScreenSize CanvasScaler | exactly 8/8 plus all accepted runtime geometry, interaction, bilingual Caption, first/last cell, disabled occupancy, mask, and cancellation assertions |
| L6 | exact `layout-controlled-full-234` manifest; no separate exact28/focused28 | exactly 234/234 and exact result-name set; repeat all §5.2 Caption assertions |
| L7 | final native Prefab/Scene reopen and source/resource/font/GUID/override/log/process audit after every run | all identities/refs stable and zero unexpected error/placeholder/glyph issue |
| L8 | Android/device L1, implemented-art screenshots/recording, QA receipt, same independent R | remains a later mandatory gate; Mac/layout tests cannot substitute |

L5b exact fullnames:

```text
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_02_540SafeAreaUsesAcceptedFiveRegionGeometry
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_03_LongScreenConvertsPhysicalSafeAreaToAcceptedCanvasTable
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_07_RuntimeResizeAndSafeAreaVersionApplyOnceWithoutZeroFrame
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_08_ModalAndRecoveryScrollRectsKeepHeaderAndActionsFixed
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_10_DrawersAreMutuallyExclusiveAndNeverCoverBoard
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_11_SystemRecoveryPrecedesEveryOrdinaryPopup
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_12_RaycastTargetsExistOnlyOnInteractiveSurfaces
```

## 6. Delta stop gates

Stop and return `BLOCKED` on the first occurrence of:

1. functional uGUI has no final independent R `ACCEPT`, or any §2 token is unfilled/mismatched;
2. a required write outside §3.1–§3.2, including any 27th path;
3. Unity is already open on this project, the approved Editor identity differs, or the run/process budget is absent;
4. an existing `.meta` changes, a stable GUID changes, or an unexpected asset/importer write appears;
5. compile/import fails before resource construction, or Prefab/Scene is edited outside native Unity serialization;
6. a second Canvas/GraphicRaycaster/EventSystem/Runtime Prefab, BoardFrame second Graphic, missing serialized ref/script, unexplained Scene override, or runtime hierarchy search is introduced;
7. a same-axis second layout owner, unbounded ContentSizeFitter growth, incorrect ScrollRect viewport/content/mask, or child view writing a shared outer root/mask;
8. accepted slot, lifecycle, save, localization, Caption, padding, hit-zone, command order, or first/last reachability semantics are weakened;
9. an exact manifest count/set drifts, a method/unanchored selector replaces fullnames, WorldSpace is cited as Overlay evidence, or a separate 28 run is added;
10. any focused/controlled run is not exact green, or final reopen/audit is attempted before L6;
11. actual Android/L1 or implemented-art evidence is replaced by Editor/static evidence;
12. C or the author attempts to self-accept the delivery.

## 7. Remaining derivation decision

No implementation fork remains in the accepted LAYOUT design: the hierarchy, responsive owner, serialized references, fixed/dynamic controls, ScrollRects, Caption cells, selectors, resource identities, and later visual/device gates are mechanically specified.

The sole dispatch blocker is external to the design: the current functional uGUI correction chain must first receive final independent R `ACCEPT`, after which the engineering lead must fill §2 from that exact accepted state and issue a new executable packet. Until then this draft remains `DRAFT_READY_NOT_AUTHORIZED`.
