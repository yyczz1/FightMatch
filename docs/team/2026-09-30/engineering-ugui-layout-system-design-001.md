# UGUI-LAYOUT-001 — FightMatch responsive uGUI hierarchy and layout system design

Status: `READY_FOR_LIMITED_REVIEW`

Implementation authorization: **none**. This document freezes a candidate design for review. It does not authorize edits to product code, tests, assets, `.meta` files, project settings, packages, or Git history, and it does not authorize a Unity run.

Delivery owner: SYS-ENG-001 system-design owner

Return path: engineering lead chat `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` on host `local`

Authority: the five-role workflow in `AGENTS.md` section 6 and the central coordinator instruction from turn `01a0e401-511d-79f2-b47f-3ab0ade1681b`. QA is expected to send the central adjudication and this complete design to the same R-TOOLS reviewer. This status is not R-TOOLS acceptance.

## 0. Scope, exclusions, and fixed inputs

This design covers only the responsive hierarchy/layout correction that follows ART-02. It freezes:

- the target Runtime Prefab hierarchy;
- ownership of layout, visibility, raycasts, serialized references, and dynamic rows;
- migration of Reference and Recovery;
- the Battle page's five-region geometry;
- fixed actor/member slots;
- local scrolling for history, diagnostics, reference, modal, and recovery bodies;
- lifecycle and failure behavior;
- a bounded future implementation file set and validation contract.

It does **not** redesign gameplay, navigation, save semantics, localization authority, playback sequencing, gesture interpretation, route ownership, Android packaging, art style, or final copy. It does not add a new Canvas, EventSystem, business action, save write, dependency, assembly definition, public API, or serialized persistence format.

The following files were read as fixed design inputs. Their observed sizes and SHA-256 values are part of this design's provenance:

| input | lines | bytes | SHA-256 | role |
|---|---:|---:|---|---|
| `docs/team/2026-09-30/central-layout-hierarchy-adjudication-001.md` | 69 | 7,011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` | central hierarchy decision |
| `docs/team/2026-09-30/engineering-system-001.md` | 1,430 | 98,468 | `4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e` | frozen SYS-ENG-001 baseline; not edited |
| `docs/team/2026-09-30/art.md` | 157 | 19,544 | `3b9a504345e509df5085688e1c897e7e737612a4db03f5bda770ab8315eb6e57` | art direction |
| `docs/team/2026-09-30/art-ugui-prefab-layout-audit-001.md` | 127 | 16,320 | `d9209887a26e9e04e92f58a0704b7a7de91b96f962c3b45b5ca25bed91dbfcf8` | ART-02 defect audit |
| `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/README.md` | 77 | 5,065 | `33dea5d90064ec62ff6e015d00b157cd16c592b907e4e29f1099cd366d3346ff` | graybox prototype contract |
| `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/asset-register.json` | 134 | 5,824 | `52eac5b72696161ae8c716468d143371f509ab654092c8e78b47e0e9c3b08b31` | prototype asset register |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` | 36,039 | 1,060,568 | `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` | **ART-02 defect input only** |
| `Assets/Scenes/FightMatchDemo.unity` | 384 | 12,900 | `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` | **ART-02 defect input only** |

The Prefab and Scene hashes above are evidence of the inspected defect state, not authorized implementation preimages. A later implementation packet must rebind bytes/SHA/GUIDs after the current UGUI correction chain receives independent acceptance. In particular, `engineering-ugui-01-q4-lifecycle-boundary-fix-12.md` records FIX11 focused evidence as `24 passed / 4 failed`; therefore this design does not call FIX11 globally accepted. Evidence may be reused only when R explicitly accepted that exact evidence and every relevant source/resource input remains unchanged.

Observed immutable resource identities:

| resource | `.meta` SHA-256 | GUID requirement |
|---|---|---|
| Runtime Prefab | `85c3eee205f94f79652eddf5d29cce3809b76ce03a0ba92311150d129409fce2` | preserve `c36df3cfc25424c8cb3ec6cae6be1237` |
| Demo Scene | `2f2cfa8c3f7d833ff3694fbfd20c45643e40c465e7209d6040fba48d76f37e7e` | preserve `1d5124e5b55fe409d8216e78a117dec0` |

No current Prefab/Scene state is accepted merely because it was readable. The Scene currently has zero-valued RuntimeCanvas transform overrides, while the Prefab has a valid one-Canvas composition; only a future Unity save/reopen and runtime measurement can decide the corrected resource state.

## 1. Decision summary

The design freezes these decisions:

1. Keep exactly one `FightMatchRuntimeRoot` Prefab, one Screen Space Overlay `Canvas`, one `GraphicRaycaster`, one project `EventSystem`, and the existing five large safe-area children: `ScreenLayer`, `HudLayer`, `PopupLayer`, `SystemLayer`, and `TemplatePool`.
2. `TopBar` exists once under shared `HudLayer`. It is not repeated inside `BattlePage`. `ScreenLayer` becomes full-safe-area; the old 88-unit top deduction is removed.
3. `BattlePage` contains `Stage`, `BattleStatus`, the square `BoardFrame`, and `BottomHud`; shared `TopBar` is the fifth functional region.
4. The whole-page `BattleScroll` is removed. History, diagnostic text, reference steps, modal body text, and recovery body text scroll only within their local viewports.
5. Reference moves from `PopupLayer/ReferencePopup` to `BattlePage/.../BottomHud/ReferenceInfoDrawer`. It retains stable ViewId `fm.popup.reference`. It never covers or raycasts the board.
6. Navigation and battle Recovery move from `PopupLayer/RecoveryPopup` to high-priority `SystemLayer/RecoveryScreen`. The recovery owner remains the existing navigation/battle state machine. Stable ViewId `fm.popup.recovery` remains unique.
7. Ordinary popup dialogs use `RootMask/DialogPanel/{Header, BodyViewport/Viewport/Content, Actions}`. Recovery uses the same roles inside a full-page System overlay. Header and Actions stay fixed; only Body scrolls.
8. Actor and member placement is fixed by `OriginalSlot`: three ally stage slots, three enemy stage slots, and three member input slots. Down and empty slots stay in place. There is no compaction or fallback reorder.
9. `FightMatchResponsiveLayout` is the sole owner of responsive Battle rects and the visible layout diagnostic. It operates only on `RectTransform` and presentation state; it never calls a gameplay, navigation, save, or playback command.
10. An unrepresentable safe height fails closed visibly. It does not overlap regions, shrink the square board, or shrink 48-unit normalized hot zones.

## 2. Existing defect-state mapping

The current builder and saved resources have these relevant facts:

| current object/code | observed state | required disposition |
|---|---|---|
| `ScreenLayer` | full anchors with `anchoredPosition.y=-44`, `sizeDelta.y=-88` | reset to full SafeAreaRoot; no duplicate TopBar deduction |
| `HudLayer` | height 80, horizontal layout, Back/Language/License | replace contents with one shared responsive `TopBar`; keep existing actions and localization ownership |
| `BattlePage/BattleContent/BattleScroll/Viewport/Content` | whole Battle page scroll; content VLG + vertical ContentSizeFitter | delete the whole-page scroll chain; create fixed Battle regions |
| `Content/Headline` | inside Battle scroll | move binding target to `HudLayer/TopBar/Title`; retain `PlayerBattleView.headline` field semantics |
| `Content/BattleStatus` | owns board plus all input/playback labels/actions | split into Stage, Status, Board, BottomHud destinations below |
| `BattleStatus/BoardFrame` | `LayoutElement` preferred/min height 420; width parent-driven | move to direct square Board region; responsive owner writes both axes from `B` |
| `Content/BattleHud` | whole-page child | move reusable status/template functions to `BottomHud/NormalHud/StatusViewport` |
| `Content/Actions` | dynamic battle actions | move to `BottomHud/NormalHud/MainRow/Actions` |
| `Content/History` | dynamic history rows in whole-page flow | move to local `BottomHud/HistoryDrawer/BodyViewport/Viewport/Content` |
| `PopupLayer/ReferencePopup` | bottom 42%, blocking Image, VLG; component root toggles itself | move to `BottomHud/ReferenceInfoDrawer`; remove board-covering blocker; retain component root and ViewId |
| `PopupLayer/ConfirmationPopup` | full root with generic VLG | rebuild as ordinary dialog hierarchy; keep confirmation controller ownership |
| `PopupLayer/RecoveryPopup` | same priority as ordinary popup; navigation view reparents shared labels | replace with `SystemLayer/RecoveryScreen`, separate navigation/battle panel references |
| language/license/quit roots | full blockers with VLG; only License has a body ScrollRect | normalize each to RootMask/DialogPanel/{Header, BodyViewport/Viewport/Content, Actions} |
| `SystemLayer` | LoadingOverlay and BlockingDiagnostic only | add RecoveryScreen and LayoutDiagnostic; preserve System priority above Popup |
| `Modal()` builder helper | root Image blocker + root VLG | replace with a builder that returns explicit section references; no whole-root vertical ContentSizeFitter |

This mapping is structural only. Existing callbacks, controller requests, localization bindings, save state, playback token, and gesture contract are preserved.

## 3. Exact target hierarchy

Names below are frozen for the future packet. A name may not be substituted merely for convenience because static hierarchy tests and serialized reference audits will use these paths.

```text
FightMatchRuntimeRoot                         [FightMatchHostView]
└── RuntimeCanvas                            [Canvas: ScreenSpaceOverlay]
    │                                        [CanvasScaler: 540x960, Match 0.5]
    │                                        [GraphicRaycaster]
    ├── BackgroundLayer
    └── SafeAreaRoot                         [SafeAreaFitter]
        │                                    [FightMatchResponsiveLayout]
        ├── ScreenLayer                      [full SafeAreaRoot rect]
        │   ├── StartupPage
        │   ├── NavigationPage
        │   ├── BattlePage
        │   │   └── BattleContent
        │   │       ├── Stage
        │   │       │   ├── EnemySlots
        │   │       │   │   ├── ActorSlot0   [Hp, Intent]
        │   │       │   │   ├── ActorSlot1   [Hp, Intent]
        │   │       │   │   └── ActorSlot2   [Hp, Intent]
        │   │       │   └── AllySlots
        │   │       │       ├── ActorSlot0   [Hp, Intent]
        │   │       │       ├── ActorSlot1   [Hp, Intent]
        │   │       │       └── ActorSlot2   [Hp, Intent]
        │   │       ├── BattleStatus
        │   │       │   ├── Phase
        │   │       │   ├── PlaybackStage
        │   │       │   └── Beat
        │   │       ├── BoardFrame           [Image; CandidateBoardElement]
        │   │       │                        [FightMatchViewId: fm.board.candidate]
        │   │       └── BottomHud
        │   │           ├── NormalHud
        │   │           │   ├── StatusViewport
        │   │           │   │   └── Content
        │   │           │   │       ├── Save
        │   │           │   │       ├── Availability
        │   │           │   │       └── PlaybackDiagnostic
        │   │           │   └── MainRow
        │   │           │       ├── MemberSlots
        │   │           │       │   ├── MemberSlot0
        │   │           │       │   ├── MemberSlot1
        │   │           │       │   └── MemberSlot2
        │   │           │       └── Actions
        │   │           │           ├── Retry
        │   │           │           ├── Resolve
        │   │           │           ├── Skip
        │   │           │           └── HistoryOpen
        │   │           │                            [fm.action.history.open]
        │   │           ├── HistoryDrawer
        │   │           │   ├── Header
        │   │           │   │   ├── Title            [fm.history.title]
        │   │           │   │   └── Close
        │   │           │   └── BodyViewport         [ScrollRect]
        │   │           │       └── Viewport          [Image + Mask]
        │   │           │           └── Content       [VLG + vertical CSF]
        │   │           └── ReferenceInfoDrawer      [PlayerDefaultReferenceView]
        │   │               │                        [fm.popup.reference]
        │   │               ├── Header
        │   │               │   ├── Title
        │   │               │   └── Close
        │   │               ├── BodyViewport         [ScrollRect]
        │   │               │   └── Viewport          [Image + Mask]
        │   │               │       └── Content       [VLG + vertical CSF]
        │   │               │           ├── Explanation
        │   │               │           ├── Beat
        │   │               │           └── Steps
        │   │               └── Actions
        │   └── ResultPage
        ├── HudLayer
        │   └── TopBar
        │       ├── Back
        │       ├── Title                             [PlayerBattleView.headline target]
        │       ├── Language
        │       └── License
        ├── PopupLayer
        │   ├── LanguagePopup
        │   │   └── RootMask
        │   │       └── DialogPanel
        │   │           ├── Header
        │   │           ├── BodyViewport             [ScrollRect]
        │   │           │   └── Viewport              [Image + Mask]
        │   │           │       └── Content
        │   │           └── Actions
        │   ├── ConfirmationPopup
        │   │   └── RootMask
        │   │       ├── NavigationConfirmationPanel
        │   │       │   ├── Header
        │   │       │   ├── BodyViewport             [ScrollRect]
        │   │       │   │   └── Viewport              [Image + Mask]
        │   │       │   │       └── Content
        │   │       │   └── Actions
        │   │       └── BattleConfirmationPanel
        │   │           ├── Header
        │   │           ├── BodyViewport             [ScrollRect]
        │   │           │   └── Viewport              [Image + Mask]
        │   │           │       └── Content
        │   │           └── Actions
        │   ├── LicensePopup
        │   │   └── RootMask
        │   │       └── DialogPanel
        │   │           ├── Header
        │   │           ├── BodyViewport             [ScrollRect]
        │   │           │   └── Viewport              [Image + Mask]
        │   │           │       └── Content
        │   │           └── Actions
        │   └── QuitPopup
        │       └── RootMask
        │           └── DialogPanel
        │               ├── Header
        │               ├── BodyViewport             [ScrollRect]
        │               │   └── Viewport              [Image + Mask]
        │               │       └── Content
        │               └── Actions
        ├── SystemLayer
        │   ├── LoadingOverlay
        │   ├── RecoveryScreen                       [fm.popup.recovery]
        │   │   └── RootMask
        │   │       ├── NavigationRecoveryPanel
        │   │       │   ├── Header
        │   │       │   ├── BodyViewport             [ScrollRect]
        │   │       │   │   └── Viewport              [Image + Mask]
        │   │       │   │       └── Content
        │   │       │   └── Actions
        │   │       │       ├── SafeActions
        │   │       │       └── DangerActions
        │   │       └── BattleRecoveryPanel
        │   │           ├── Header
        │   │           ├── BodyViewport             [ScrollRect]
        │   │           │   └── Viewport              [Image + Mask]
        │   │           │       └── Content
        │   │           └── Actions
        │   │               ├── SafeActions
        │   │               └── DangerActions
        │   ├── LayoutDiagnostic
        │   └── BlockingDiagnostic
        └── TemplatePool
```

`RootMask/Panel/Header/BodyViewport/Actions` is the required role nesting. An ordinary panel's literal name is `DialogPanel`; confirmation/recovery panel names identify their owner. Every `BodyViewport` is the ScrollRect root and contains the standard `Viewport` (Image + Mask) → `Content` chain. Confirmation and Recovery use one RootMask with two mutually exclusive panels; each panel independently has Header, BodyViewport, and Actions.

Sibling priority is also frozen:

- `SystemLayer` renders and raycasts above `PopupLayer`.
- Inside `SystemLayer`, later siblings have higher priority: `LoadingOverlay`, `RecoveryScreen`, `LayoutDiagnostic`, `BlockingDiagnostic`.
- When Recovery is active, ordinary popups are closed or made non-interactable before the Recovery root is enabled. A Popup cannot cover or receive input over Recovery.
- `BlockingDiagnostic` remains the highest-priority product diagnostic. `LayoutDiagnostic` is below it but above Recovery because an invalid layout cannot safely expose Recovery actions either.

## 4. Responsive geometry contract

### 4.1 Coordinate source

The Canvas remains `Scale With Screen Size`, reference resolution `540 × 960`, screen match mode `Match Width Or Height`, match `0.5`.

All formulas use the **actual `SafeAreaRoot.rect` width and height in Canvas units** after `SafeAreaFitter` applies its anchors:

```text
W = SafeAreaRoot.rect.width
H = SafeAreaRoot.rect.height
B = min(W, 508)
k = B / 508
```

No formula uses serialized `RectTransform.sizeDelta` as a runtime measurement. `Screen.safeArea` is a physical-pixel input to `SafeAreaFitter`, not the responsive layout's direct coordinate source. The Canvas `scaleFactor` and `transform.lossyScale` are observed separately for diagnostics/evidence; neither is asserted to equal one.

### 4.2 Preferred dimensions

```text
TopBar    =  56k
Stage     = 166k
Status    =  48k
Board     = 508k = B
BottomHud = 142k
core      = 920k
```

When `H > core`, vertical breathing is:

```text
breathing = (H - core) / 2
top breathing = breathing
bottom breathing = breathing
```

`TopBar` is placed once under shared `HudLayer`, after top breathing. `BattlePage/BattleContent` starts below that TopBar and owns Stage, Status, Board, and BottomHud. The old ScreenLayer 88-unit deduction is deleted; it must not coexist with this placement.

Horizontal placement centers all five regions within the safe rect. Board width and height are both exactly `B`. Other regions use width `B` unless their child contract says otherwise. This makes board rendering and hit conversion consume the same final `BoardFrame.rect`.

### 4.3 Short-height adaptation

When `H < core`, remove all breathing first, then compute:

```text
D = clamp(920k - H, 0, 108k)
Stage = 166k - min(D, 70k)
BottomHud = 142k - min(max(D - 70k, 0), 38k)
TopBar = 56k
Status = 48k
Board = 508k
fixed minimum total = 812k
```

This freezes the main-art responsive geometry receipt from turn `01a0f52a-c461-74c3-8778-77854bf914eb`, whose verdict was `ACCEPT（仅响应式几何表）`. That verdict accepts only the geometry table. It is not acceptance of Unity resources, component ownership, interaction, localization, Mac validation, Android/device behavior, or this complete design.

At the minima:

- Stage is `96k`.
- BottomHud is `104k`.
- BottomHud reserves `72k` for the member/action row, `24k` for the status line, and `8k` bottom padding.
- A 48k member/action hot zone is placed within the same 72k row. The status line never overlaps it.
- Compression may hide only graybox/noninteractive decoration. It may not hide required state, shrink a 48k hot zone, shrink the board, or move an actor/member into another slot.

If `H < 812k`, `FightMatchResponsiveLayout` enters visible fail-closed mode. It retains the last valid rects, disables Battle interaction/raycast eligibility, and enables `SystemLayer/LayoutDiagnostic`. It does not apply overlapping or zero rects and does not emit any business action or save mutation. Once a valid rect returns, the component reapplies layout and releases only its own gate; the host/controller remains the authority for which page is visible.

### 4.4 Required numeric tables

#### 540 × 960 screen with four nonzero safe insets

Physical safe rect is `(x=16, y=16, width=508, height=920)` pixels. At this reference resolution the accepted runtime example is `W=508`, `H=920`, `k=1`.

| item | Canvas units | physical pixels | note |
|---|---:|---:|---|
| safe width `W` | 508 | 508 | actual SafeAreaRoot rect |
| safe height `H` | 920 | 920 | actual SafeAreaRoot rect |
| top breathing | 0 | 0 | core fills safe height |
| TopBar | 56 | 56 | shared HudLayer |
| Stage | 166 | 166 | fixed slots |
| Status | 48 | 48 | phase/stage/beat |
| Board | 508 × 508 | 508 × 508 | square |
| BottomHud | 142 | 142 | normal/history/reference replacement area |
| bottom breathing | 0 | 0 | core fills safe height |
| minimum hot zone | 48 | 48 | never reduced by short-height logic |

The four nonzero physical insets are left 16, right 16, bottom 16, and top 24.

#### 1080 × 2400 screen with four nonzero safe insets

Physical safe rect is `(x=32, y=80, width=1016, height=2160)` pixels, giving left/right 32 and bottom/top 80/160. With match `0.5`, the expected scale factor is `sqrt(5) ≈ 2.236067977`.

| item | Canvas units | physical pixels | derivation |
|---|---:|---:|---|
| safe width `W` | 454.369013 | 1,016 | `1016 / sqrt(5)` |
| safe height `H` | 965.981366 | 2,160 | `2160 / sqrt(5)` |
| `B` | 454.369013 | 1,016 | `min(W,508)` |
| `k` | 0.894427191 | — | `B/508` |
| core | 822.873016 | 1,840 | `920k` |
| top breathing | 71.554175 | 160 | `(H-core)/2` |
| TopBar | 50.087923 | 112 | `56k` |
| Stage | 148.474914 | 332 | `166k` |
| Status | 42.932505 | 96 | `48k` |
| Board | 454.369013 square | 1,016 square | `508k` |
| BottomHud | 127.008661 | 284 | `142k` |
| bottom breathing | 71.554175 | 160 | `(H-core)/2` |
| minimum hot zone | 42.932505 | 96 | `48k`; width-normalized, not height-compressed |
| fail-closed boundary | 726.274879 | 1,624 | `812k` |

The 48k Canvas-unit hot zone is smaller numerically only because width normalization uses `k`; it is 96 physical pixels in this example. The short-height algorithm never reduces it further.

## 5. Layout ownership and update timing

### 5.1 New component boundary

Exactly one new production component is proposed:

```text
Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs
Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta
```

It is a sealed presentation component on `SafeAreaRoot`. Its serialized references are limited to:

- the owning `SafeAreaFitter` and `Canvas`;
- `ScreenLayer`, `HudLayer/TopBar`, `BattlePage/BattleContent`;
- `Stage`, `BattleStatus`, `BoardFrame`, and `BottomHud`;
- the normal/history/reference BottomHud roots;
- ordinary popup panels and both recovery panels that require max-width sizing;
- `SystemLayer/LayoutDiagnostic` and the Battle interaction root or CanvasGroup used by the layout gate.

It may read `SafeAreaFitter.Version`, `SafeAreaRoot.rect`, `Canvas.scaleFactor`, and relevant `lossyScale`. It may write only `RectTransform` layout values, layout-only `CanvasGroup` interaction state, and LayoutDiagnostic visibility/text binding. It does not reference Application/domain/save/controller types and exposes no public business API.

`SafeAreaFitter.cs` remains unchanged. Its `Changed` event and `Version` are sufficient seams.

### 5.2 Update triggers

The component uses this bounded schedule:

1. `OnEnable`: subscribe to `SafeAreaFitter.Changed`, mark dirty, and attempt an immediate apply only if the safe rect is finite and positive.
2. `SafeAreaFitter.Changed`: mark dirty and record the new Version; do not recursively force a layout rebuild inside the callback.
3. `OnRectTransformDimensionsChange`: mark dirty for editor/device resize and orientation/safe-area changes.
4. `Canvas.willRenderCanvases`: when dirty, sample the final safe rect and apply once before rendering. Reapply only if Version, W/H, scaleFactor, or observed lossyScale tuple changed.
5. `OnDisable`/`OnDestroy`: unsubscribe idempotently and clear only the layout component's own pending callback/gate state.

First-frame behavior is fail-closed without a zero-sized flash:

- The Prefab stores valid 540-reference default rects for TopBar and Battle regions.
- The component never writes a zero, NaN, infinity, negative, or below-minimum rect.
- If the first `OnEnable` sample is not valid, defaults remain untouched, Battle input is gated, and one pre-render attempt is made.
- If the pre-render sample is still invalid or `H<812k`, LayoutDiagnostic becomes visible before Battle becomes interactive.
- A valid first sample applies the final rects before render and releases the layout gate.

This contract is necessary because serialized defaults, runtime Canvas rect, `lossyScale`, and `scaleFactor` are different evidence. Screen Space Overlay does not justify an assertion that runtime scale equals one.

### 5.3 No competing controllers

For every responsive-owned axis:

- `FightMatchResponsiveLayout` writes the parent region RectTransform.
- No `VerticalLayoutGroup`, `HorizontalLayoutGroup`, `GridLayoutGroup`, `ContentSizeFitter`, or `AspectRatioFitter` may control the same axis.
- The square board is written directly as width=`B`, height=`B`; it does not use `AspectRatioFitter`.
- Stage slot children may use a horizontal layout within the already-sized Stage, provided it does not write Stage's vertical axis.
- BottomHud child rows may use a horizontal layout within the already-sized row, provided it does not write BottomHud's vertical axis.
- A scroll `Content` may use `VerticalLayoutGroup + ContentSizeFitter(vertical preferred)`; its Viewport and owning region are not content-sized.
- DialogPanel may use a VLG with child `LayoutElement`s and a flexible BodyViewport, but the panel itself must not also have a same-axis ContentSizeFitter. The responsive component owns panel max width/height.

Any same-axis dual owner is a stop-gate failure, not a tolerated warning.

## 6. Component/reference ownership

Component attachment points are fixed:

| component | GameObject owner | lookup rule |
|---|---|---|
| `FightMatchHostView` | `FightMatchRuntimeRoot` | root composition owner; all child views/roots are serialized |
| `SafeAreaFitter` | `RuntimeCanvas/SafeAreaRoot` | existing component; no new lookup |
| `FightMatchResponsiveLayout` | `RuntimeCanvas/SafeAreaRoot` | serialized by Host and wired to explicit RectTransforms |
| `PlayerNavigationRecoveryView` | `SafeAreaRoot/PopupLayer` | attachment remains stable; serialized refs may cross to SystemLayer Recovery |
| `PlayerBattleView` | `ScreenLayer/BattlePage` | existing attachment; serialized child-view/container refs |
| `CandidateBoardInputView` | `BattlePage/BattleContent/BattleStatus` | existing attachment; serialized Stage/Board/BottomHud refs |
| `CandidateBattlePlaybackView` | `BattlePage/BattleContent/BattleStatus` | existing attachment; serialized input view and fixed output refs |
| `CandidateBoardElement` | `BattlePage/BattleContent/BoardFrame` | existing board Graphic/input owner |
| `PlayerDefaultReferenceView` | `BattlePage/BattleContent/BottomHud/ReferenceInfoDrawer` | component root is the drawer root |

Runtime code must not use `Transform.Find`, name search, scene-wide object search, or hierarchy-position inference to locate any item in this table. `FightMatchAndroidBuild.References(...)` assigns the serialized links when constructing the Prefab; Unity serialization supplies them after save/reopen. Controllers continue to enter through existing `Bind(...)` calls: Host receives session/controller ownership, Host binds `PlayerNavigationView` and `PlayerBattleView`, those views bind Recovery/Playback/Input/Reference children. Moving a transform does not create a second controller lookup or service locator.

### 6.1 FightMatchHostView

`FightMatchHostView` remains the sole page/route composition owner. A future implementation adds one serialized `FightMatchResponsiveLayout responsiveLayout` reference and includes it in reference validation. The host does not calculate region geometry and does not decide that a visual close completes a business operation.

Existing behavior remains:

- `Bind` validates and binds the current session once.
- `Render` consumes the session page and selects page roots.
- `PausePresentation` cancels board interaction before pausing the session.
- `RefreshDiagnostic` cancels board interaction and owns the existing blocking diagnostic.
- `Unbind` releases host subscriptions and view ownership.

LayoutDiagnostic is not a new host route. It is a presentation gate layered over the route currently owned by the host.

The Host fields affected by hierarchy movement remain private serialized refs and are rebound, not rediscovered:

| Host field group | target/disposition |
|---|---|
| `startupPage`, `navigationPage`, `battlePage`, `resultPage` | same named roots under full-rect ScreenLayer |
| `languagePopup`, `licensePopup`, `quitPopup` | same outer Popup roots; each now contains RootMask/DialogPanel sections |
| `loadingOverlay`, `blockingDiagnostic` | same System roots and priority semantics |
| `navigationView`, `battleView`, `board` | same component identities on Navigation content, BattlePage, and BoardFrame |
| `backButton`, `languageButton`, `licenseButton` | fixed children of shared HudLayer/TopBar |
| language/license/quit title/body/action refs | rebound to fixed Header, Body Content, or Actions child according to role |
| new `responsiveLayout` | SafeAreaRoot/FightMatchResponsiveLayout; validated on Bind |

Startup fields, navigation fields unrelated to Recovery, profile actions, and Result page refs do not change ownership in this design.

### 6.2 PlayerBattleView serialized migration

The existing serialized meanings are preserved and rebound as follows:

| field | current target/meaning | target target/meaning |
|---|---|---|
| `battleContent` | whole scrolling content owner | `BattlePage/BattleContent`; fixed-region root |
| `resultRoot` | Result page | unchanged semantic/root |
| `playbackView` (request shorthand: playback) | playback component on BattleStatus | same component owner, with refs split across Stage/Status/BottomHud |
| `referenceView` (request shorthand: reference) | PopupLayer ReferencePopup component | BottomHud ReferenceInfoDrawer component |
| `headline` | BattleScroll Headline | shared `HudLayer/TopBar/Title` |
| `hud` | BattleHud rows | `BottomHud/NormalHud/StatusViewport/Content` |
| `actions` | whole-page dynamic actions | `BottomHud/NormalHud/MainRow/Actions` |
| `history` | whole-page history container | `BottomHud/HistoryDrawer/BodyViewport/Viewport/Content` |
| `dialog` | confirmation dynamic rows | `PopupLayer/ConfirmationPopup/RootMask/BattleConfirmationPanel/BodyViewport/Viewport/Content` |
| `recovery` | PopupLayer recovery dynamic rows | `SystemLayer/RecoveryScreen/RootMask/BattleRecoveryPanel/BodyViewport/Viewport/Content` |
| `receipt` | Result receipt rows | unchanged semantic/root |

New serialized presentation roots are exact and private: `normalHudRoot`, `historyDrawerRoot`, `historyHeader`, `historyCloseButton`, `dialogActions`, and `recoveryActions`. They do not expose a new public interface.

`OwnContainer` and `ClearRows` remain the dynamic-row safety boundary. Their accepted container list changes to the exact dynamic contents/actions above: normal battle `actions`, history `Content`, dialog `Body Content`, dialog `Actions`, recovery `Body Content`, recovery `Actions`, and receipt. A row may be destroyed only when its parent is one of those owned containers. Fixed Headers, slot roots, buttons supplied as fixed fields, and templates are never cleared as dynamic rows.

Every button callback continues to capture controller identity, page ownership, and `renderEpoch`. Moving the transform does not weaken the stale-callback checks. Route change and Unbind increment `renderEpoch`, clear owned rows, reset BottomHud presentation to Normal, close the reference view, hide dialog/recovery roots owned by this view, and detach exactly once.

### 6.3 CandidateBoardInputView fixed slots

The current single enemies text plus dynamic member-list layout is replaced with these exact private serialized references:

- `Image board` — unchanged board input surface;
- `LocalizedTmpText phase` — `BattleStatus/Phase`;
- `LocalizedTmpText save` — `BottomHud/NormalHud/StatusViewport/Content/Save`;
- `LocalizedTmpText availability` — same Status Content;
- `RectTransform[] allyStageSlots` — exactly 3, ordered slot 0..2;
- `RectTransform[] enemyStageSlots` — exactly 3, ordered slot 0..2;
- `RectTransform[] memberSlots` — exactly 3, ordered slot 0..2;
- `Button memberTemplate`, `retry`, `resolve` — rebound to their fixed target areas.

The old `enemies` aggregate text and `members` dynamic flow container are removed as layout authorities. Stage slot labels are presentation bindings owned through CandidateBoardInputView; CandidateBattlePlaybackView is the exclusive writer of current HP/intent values through one internal presentation-only method on the input view. That method takes already-derived presentation values and never mutates the controller.

Fixed-slot rules:

1. Resolve every actor/member by `OriginalSlot`, range 0..2.
2. Ally actor data goes only to `allyStageSlots[OriginalSlot]`; enemy actor data only to the enemy equivalent; selectable member button only to `memberSlots[OriginalSlot]`.
3. Empty slots remain present and clear their labels/button child; siblings never shift.
4. A down actor remains in the original slot, shows the existing down/availability presentation, and is non-interactable when current rules require it. It is never compacted away.
5. Duplicate, missing-required, or out-of-range OriginalSlot fails closed through the existing blocking diagnostic path or a layout-binding diagnostic before board input becomes available. It never falls back to enumeration order.
6. Dynamic member buttons retain `fm.member.{characterId}`. Before replacement, the old child/ViewId is destroyed through the established dynamic-row lifecycle so `FightMatchViewId.Validate` never sees stale or duplicate inactive IDs.
7. All six Stage slot roots and all three member slot roots are fixed Prefab objects, not dynamically instantiated layout roots.

The board renderer and input converter continue to use the same actual `BoardFrame.rect`. The fixed-slot change does not alter route gestures, pointer ownership, candidate selection, validation, resolve, retry, or save semantics.

### 6.4 CandidateBattlePlaybackView mapping

The playback component keeps its controller/token lifecycle and remaps presentation outputs exactly:

| playback datum/action | new target |
|---|---|
| HP for allies/enemies | fixed Ally/Enemy ActorSlot HP label by OriginalSlot |
| intent for allies/enemies | same fixed ActorSlot Intent label |
| beat | `BattleStatus/Beat` |
| stage | `BattleStatus/PlaybackStage` |
| diagnostic | `BottomHud/NormalHud/StatusViewport/Content/PlaybackDiagnostic` |
| skip | `BottomHud/NormalHud/MainRow/Actions/Skip` |

At the serialized-field level, `inputView` stays bound to the CandidateBoardInputView component; aggregate `hp` and `intent` refs are removed in favor of that component's six fixed slot bindings; `beat`, `stage`, `diagnostic`, and `skip` are rebound to the exact targets in the table. No component searches its sibling/parent by name.

CandidateBattlePlaybackView remains the exclusive source of frame/current HP and intent display. It calls the input view's internal slot-presentation seam on Bind and every relevant Changed event, including the non-playing current-state case. CandidateBoardInputView does not independently race-write Stage labels from its own Render. On Unbind, playback invalidates its token, unsubscribes, clears slot presentation through that seam, and does not cancel or complete business work merely because the visual root is hidden.

### 6.5 Reference drawer

`PlayerDefaultReferenceView` remains attached to its own component root, now `ReferenceInfoDrawer`. Existing `gameObject.SetActive` behavior therefore toggles only the BottomHud replacement area.

The fields map as follows:

| field | target |
|---|---|
| `explanation` | BodyViewport/Viewport/Content/Explanation |
| `beat` | BodyViewport/Viewport/Content/Beat |
| `steps` | BodyViewport/Viewport/Content/Steps |
| `buttonTemplate` | inactive template used only under Body Content or Actions as appropriate |
| `closeButton` | Header/Close or Actions/Close; one fixed visible close action |

The view gains a private/internal close-notification seam supplied by `PlayerBattleView.Bind`; it is not a business event. The callback captures the same controller/page/renderEpoch ownership and restores `NormalHud` only if that owner is still current.

Reference behavior is frozen:

- Normal, History, and Reference are mutually exclusive BottomHud presentation modes.
- Opening Reference first closes History, activates Reference, and keeps the Board rect and board raycast fully unobstructed.
- The drawer's Images have `raycastTarget=false` unless they are actual buttons or the local BodyViewport mask target required for scrolling. Its rect is wholly inside BottomHud and never intersects BoardFrame.
- Close button and route/unbind close clear dynamic steps/subscriptions and restore Normal through the guarded callback.
- On the first real drag, the existing order remains: remember the active input, `Close()` the reference drawer, then `PublishReferenceClosedFeedback()`. Closing does not cancel that active pointer.
- Pointer cancel, focus loss, view destruction, or a synthetic/non-drag event does not publish first-drag feedback.
- Destroyed serialized label/button references are guarded using Unity object validity; stale close callbacks are ignored by epoch/owner checks.

True board/reference lines remain rendered on the board; this information drawer does not replace or overlay those lines.

### 6.6 History drawer

History is required functionality and cannot disappear. The existing fixed action ID `fm.action.history.open` and copy key `fm.history.title` remain authoritative.

`PlayerBattleView` owns one private presentation enum:

```text
BottomHudMode = Normal | History | Reference
```

Transitions:

| event | required result | forbidden side effect |
|---|---|---|
| bind/current Battle render | Normal unless a still-owned local mode is explicitly retained | no save write |
| HistoryOpen | close Reference; show History | no controller request |
| HistoryClose | show Normal | no controller request |
| ReferenceOpen | close History; show Reference | no controller request except existing reference-display/feedback contract |
| Reference close/first drag | show Normal through guarded callback | close is not action completion |
| locale change | rebuild visible localized rows, retain valid current mode | no mode-derived save |
| route away/unbind/page owner loss | close both drawers, reset Normal, invalidate epoch | no completion inferred from hiding |

History body is a local ScrollRect. The Header/Title/Close are fixed. Dynamic history rows belong to BodyViewport/Viewport/Content and retain their existing action semantics, including rollback preview. Opening or closing the drawer itself performs zero gameplay, save, navigation, or rollback writes.

### 6.7 Navigation confirmation and Recovery

`PlayerNavigationRecoveryView` remains attached to `PopupLayer`, which itself remains active as a stable component host. It may hold serialized cross-layer references; it does not derive Recovery by walking from its own transform. It currently reparents a shared title/status/original-result control between confirmation and recovery roots. That reparenting is removed. The view receives distinct serialized targets:

- `confirmationRoot`, `confirmationHeaderTitle`, `confirmationBody`, `confirmationActions`;
- `recoveryRoot`, `recoveryHeaderTitle`, `recoveryBody`, `recoverySafeActions`, `recoveryDangerActions`;
- existing fixed action buttons rebound to their exact Actions subgroup;
- `buttonTemplate` and `textTemplate` remain dynamic templates but may instantiate only into the corresponding owned Body/Actions container.

The exact old-to-new field migration is:

| existing field | new serialized target/disposition |
|---|---|
| `confirmationRoot` | `PopupLayer/ConfirmationPopup/RootMask/NavigationConfirmationPanel` |
| `recoveryRoot` | `SystemLayer/RecoveryScreen/RootMask/NavigationRecoveryPanel` |
| `confirmationRows` | replaced by `confirmationBody` = NavigationConfirmationPanel/BodyViewport/Viewport/Content |
| `recoveryRows` | replaced by `recoveryBody` = NavigationRecoveryPanel/BodyViewport/Viewport/Content |
| shared `title` | replaced by `confirmationTitle` and `recoveryTitle`, each fixed under its Header |
| shared `status` | replaced by `confirmationStatus` and `recoveryStatus`, each fixed in its Body Content |
| `confirmButton` | NavigationConfirmationPanel/Actions/SafeActions/Confirm |
| `endButton` | NavigationConfirmationPanel/Actions/DangerActions/End |
| `returnButton` | NavigationRecoveryPanel/Actions/SafeActions/Return |
| `resumeObservedButton` | NavigationRecoveryPanel/Actions/SafeActions/ResumeObserved |
| `retryButton` | NavigationRecoveryPanel/Actions/SafeActions/Retry |
| `resolveButton` | NavigationRecoveryPanel/Actions/SafeActions/Resolve |
| `refreshButton` | NavigationRecoveryPanel/Actions/SafeActions/Refresh |
| `endReviewButton` | NavigationRecoveryPanel/Actions/DangerActions/EndReview |
| `originalResultButton` | NavigationRecoveryPanel/Actions/SafeActions/OriginalResult; never reparented |
| `buttonTemplate` | inactive `TemplatePool/NavigationButtonTemplate`; cloned only into the active Actions group |
| `textTemplate` | inactive `TemplatePool/TextTemplate`; cloned only into the active Body Content |

`PlayerBattleView.dialog` and `.recovery` likewise split into `dialogBody`, `dialogActions`, `recoveryBody`, and `recoveryActions`, while their fixed root references are `dialogRoot` and `recoveryRoot`. Battle dynamic text is never instantiated into an Actions container, and a dynamic Button is never instantiated into Body Content.

No title, status label, button, or dynamic row is reparented at runtime. Text rows always go to Body Content. Buttons always go to Actions. `originalResultButton` has one fixed parent chosen by its existing action role; visibility changes, not parentage.

The view preserves:

- `generation` invalidation and guarded callbacks;
- bindings disposal and old-controller unsubscribe;
- button availability/interactable state;
- confirmation cancel behavior;
- all original confirm/end/return/resume-observed/retry/resolve/refresh/end-review/original-result actions;
- the original recovery route and state-machine authority.

Battle confirmation follows the same separation in `PlayerBattleView`: rollback-preview/end confirmation text goes to Body, existing accept/cancel buttons go to Actions, and hide/cancel/route behavior stays distinct from completion.

## 7. Popup and System recovery contract

### 7.1 Ordinary popup geometry

Every ordinary popup has a full-safe-area `RootMask` and centered `DialogPanel`:

```text
panelWidth = min(476, safeW - 64)
horizontal margin = 32 per side when safeW <= 540
panelMaxHeight = safeH - 64
```

If `safeW <= 64`, layout is invalid and the layout diagnostic gate applies; the panel is not assigned a negative width.

Required roles:

- `RootMask`: stretched to safe area, opaque/translucent blocker Image, `raycastTarget=true`.
- `DialogPanel`: centered, width rule above, Image may raycast to prevent click-through inside the panel.
- `Header`: fixed height; title/close remain visible.
- `BodyViewport`: flexible remaining height and vertical-only `ScrollRect`; its `viewport` reference points to the direct `Viewport` child.
- `Viewport`: stretch-filled under BodyViewport, with Image + `Mask`; its Image may be a raycast target only when needed to receive local scroll drag.
- `Content`: direct child of Viewport, top-anchored, `VerticalLayoutGroup`, `ContentSizeFitter.verticalFit=PreferredSize`; no horizontal fit.
- `Actions`: fixed outside the viewport. Actions are a vertical stack with at least 48k hot zones and at least 8k separation. This vertical baseline is the deterministic narrow-width degradation; no runtime horizontal/vertical mode race is needed.

Noninteractive decorative Images and TMP labels have `raycastTarget=false`. Buttons and RootMask remain raycast targets. The ScrollRect has only the vertical axis enabled, inertia/elasticity fixed by the implementation packet, and no whole-page parent scroll.

### 7.2 Recovery geometry and precedence

Recovery is a complete high-priority System page, not an ordinary Popup. `RecoveryScreen/RootMask` fills SafeAreaRoot and blocks all lower layers. Each active recovery panel uses the same max-width/margin/header/body/actions rules, with height allowed up to `safeH-64`.

Only one of `NavigationRecoveryPanel` and `BattleRecoveryPanel` is active. The inactive panel is non-interactable and has no subscribed stale dynamic actions. System Recovery closes/disables ordinary Popup interactions before activation; Popup cannot obscure it.

Danger actions are visually separated inside `Actions/DangerActions` by spacing/divider/tone. This separation introduces no new action and changes no callback, availability rule, save behavior, or confirmation requirement. Safe actions stay in `SafeActions`.

## 8. Raycast and interaction rules

| region | allowed raycast targets | required exclusions |
|---|---|---|
| BoardFrame | CandidateBoardElement's intended graphic | no BottomHud/Reference/History graphic overlaps its rect |
| Stage | actual stage buttons only if existing behavior requires them | HP/intent labels and decoration false |
| BattleStatus | actual buttons only | labels/background false |
| BottomHud Normal | member/action Buttons; local ScrollRect viewport as needed | backgrounds/labels false |
| History/Reference | Close/action buttons and local viewport | drawer root/background false; rect below board |
| Popup | RootMask, panel blocker, buttons, local viewport | noninteractive text/art false |
| System Recovery | RootMask, panel blocker, buttons, local viewport | all lower-layer interaction blocked |
| LayoutDiagnostic | its explicit diagnostic blocker only | no hidden Battle input remains active |

Static checks must prove the drawer rect does not overlap BoardFrame. Runtime checks must prove an actual pointer drag beginning on the board reaches `CandidateBoardElement` while the reference drawer is visible, closes the drawer through the existing first-drag path, and preserves the active pointer sequence.

## 9. Lifecycle and failure matrix

| trigger/failure | owner | required presentation result | business/input invariant |
|---|---|---|---|
| initial Bind | host + child view | bind once; render owned page; apply valid layout before input | no implicit action/save |
| repeated Bind/current owner | view | old subscriptions disposed before replacement | no duplicate callbacks |
| Unbind | each view | increment epoch/generation, clear owned rows, hide local roots, unsubscribe | presentation hide is not completion |
| route away from Battle | host/PlayerBattleView | reset BottomHud mode, close drawers/dialog, cancel board presentation as existing | controller/page owns route |
| Reference first real drag | Reference view | Close, guarded Normal restore, then feedback publication | active pointer survives; feedback once |
| pointer cancel/end before real drag | board/reference lifecycle | release only owned pointer resources | no first-drag feedback/action |
| focus loss/pause/detach | host/board/playback | use existing cancellation/pause boundaries | no orphan completion |
| playback token replaced | playback view | old schedule/callback cannot update fixed slots | token owner only; no stale frame |
| rollback preview open/close | PlayerBattleView | modal Body/Actions only; cancel hides | no rollback/save until existing accept action |
| confirmation cancel | navigation/battle view | hide confirmation and invalidate callbacks | no confirm/end request |
| Recovery route | state machine + recovery view | close Popup; show correct System panel | original recovery action set only |
| locale rebind | localization/view | update fixed and dynamic visible copy; preserve valid presentation mode | zero gameplay/save writes |
| destroyed TMP/Button/Rect reference | owning view | Unity-validity guard, diagnostic if required, safe cleanup | no MissingReference stale callback |
| stale callback after route/rebind | epoch/generation guard | ignored | no controller request |
| invalid/duplicate OriginalSlot | board presentation | visible blocking/layout binding diagnostic; input unavailable | no fallback reorder or resolve |
| duplicate/missing ViewId | resource validation | stop before acceptance | no runtime guess |
| `H < 812k` or invalid rect | responsive layout | last valid rect retained; LayoutDiagnostic visible; Battle interaction gated | no shrink/overlap/business write |
| SafeArea Version change | responsive layout | one pre-render recalculation from final safe rect | route/controller unchanged |
| runtime resize/orientation | responsive layout | mark dirty and reapply once from actual rect | no first-frame zero flash |
| Popup active then Recovery activates | recovery owner | Popup becomes non-interactable/closed; Recovery on top | Popup action cannot fire through System |
| destroyed responsive component | component | unsubscribe `willRenderCanvases` and SafeArea event idempotently | host/session remains owner |

Existing gesture, candidate-board, application, save, playback, recovery, and host-lifetime assertions remain mandatory. Layout work cannot weaken a test merely because its transform path changes.

## 10. Future candidate file scope — not authorized

The next implementation packet must first rebind accepted preimages. If authorized without a new scope decision, its entire maximum whitelist is the following exact set.

### 10.1 Existing production files that may be modified

1. `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`
2. `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs`
3. `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs`
4. `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs`
5. `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs`
6. `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs`
7. `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs`

`FightMatchAndroidBuild.cs` is included because it is the current authoritative Runtime Prefab/Scene construction path. It may construct the frozen hierarchy and assign exact serialized references; it may not change Android Player settings, API level, build pipeline, packages, or unrelated resources in this packet.

### 10.2 New production files

8. `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs`
9. `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta`

The new `.meta` must be generated once by the authorized Unity/editor workflow, recorded in the packet, and never regenerated during correction rounds.

### 10.3 Existing test/support files that may be modified

10. `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs`
11. `Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs`
12. `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs`
13. `Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs`
14. `Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs`
15. `Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs`
16. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs`
17. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs`
18. `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`
19. `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs`
20. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs`
21. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs`

Fixture files are listed because their serialized-ref construction must match the fixed slots and split Body/Actions roots. Tests may be adjusted only for new paths/refs and the explicitly frozen assertions; existing business expectations and accepted test names stay unchanged.

### 10.4 New test files

22. `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs`
23. `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta`

### 10.5 Unity resources

24. `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`
25. `Assets/Scenes/FightMatchDemo.unity`

Their existing `.meta` files are forbidden to modify. Prefab GUID `c36df3cfc25424c8cb3ec6cae6be1237` and Scene GUID `1d5124e5b55fe409d8216e78a117dec0` must remain byte-identical. The Scene retains exactly one Prefab instance and one project EventSystem; there are no unexplained overrides after native save/reopen.

### 10.6 Explicitly forbidden in this candidate packet

- `SafeAreaFitter.cs`, `FightMatchViewId.cs`, localization tables/generated output, font assets, domain/application/save models, gestures, board algorithms, all `.asmdef` files, Packages, ProjectSettings, CI/build-release configuration, and every unlisted file.
- any public API, serialized save format, new business action, new dependency, new Canvas, new EventSystem, extra Runtime Prefab, or GUID regeneration.
- broad formatting, renaming unrelated ViewIds, changing accepted test names to satisfy selectors, or deleting history/reference/recovery behavior.

If implementation discovers a required unlisted path, it stops `BLOCKED` and returns to engineering lead; it does not expand the whitelist silently.

## 11. Validation contract for a future authorized packet

No validation in this section has been executed by this design task. It is a finite future sequence.

### Q0 — design and preimage gate

Owner: engineering lead, then QA/R-TOOLS.

Required before editing:

1. R-TOOLS returns an independent design verdict on the central adjudication plus this full document.
2. The current UGUI correction chain has an independent accepted candidate.
3. Every whitelisted source/resource preimage gets fresh line/byte/SHA; both resource GUIDs and `.meta` hashes are recorded.
4. `git status --short` is captured; unrelated dirty files are declared and preserved.
5. Any mismatch stops before implementation.

### Q1 — static hierarchy, reference, raycast, and ownership tests

Run the exact new full names:

```text
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_01_ReferenceRecoveryAndModalHierarchyMatchesAdjudication
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_02_540SafeAreaUsesAcceptedFiveRegionGeometry
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_03_LongScreenConvertsPhysicalSafeAreaToAcceptedCanvasTable
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_04_ShortHeightCompressionStopsAtStageAndBottomHudMinima
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_05_TooShortHeightFailsClosedWithoutBoardOrHotZoneShrink
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_06_LayoutOwnershipHasNoSameAxisControllerConflict
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_07_RuntimeResizeAndSafeAreaVersionApplyOnceWithoutZeroFrame
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_08_ModalAndRecoveryScrollRectsKeepHeaderAndActionsFixed
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_09_FixedSlotsUseOriginalSlotWithoutCompaction
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_10_DrawersAreMutuallyExclusiveAndNeverCoverBoard
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_11_SystemRecoveryPrecedesEveryOrdinaryPopup
FightMatch.Core.Tests.UguiResponsiveLayoutTests.LAYOUT_12_RaycastTargetsExistOnlyOnInteractiveSurfaces
```

Additionally run existing resource assertions by their unchanged full names:

```text
FightMatch.Core.Tests.UguiSceneCompositionTests.SavedPrefabAssemblesOneRealHostCanvasAndProjectEventSystem
FightMatch.Core.Tests.UguiSceneCompositionTests.FourCornerCentersAndHalfOpenEdgesUseTheActualBoardMapper
FightMatch.Core.Tests.UguiSceneCompositionTests.BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical
```

Static inspection must prove every serialized reference resolves, both fixed ViewIds remain unique, `fm.action.history.open` exists once, dynamic member IDs retain `fm.member.*`, no whole `BattleScroll` remains, and no same-axis layout controller conflict exists.

### Q2 — focused lifecycle and behavior regressions

At minimum, execute these unchanged exact full names if their source and fixture inputs remain accepted and unchanged; otherwise rerun them and treat them as new evidence:

```text
FightMatch.Core.Tests.PlayerDefaultReferenceTests.UGUI_COPY_WIRE02_FirstDragNoticeSurvivesAndReplayAppearsOnlyAfterCompletion
FightMatch.Core.Tests.PlayerDefaultReferenceTests.UGUI_COPY_WIRE04_ReferenceInitialFrameNeverClaimsActionSubmitted
FightMatch.Core.Tests.PlayerDefaultReferenceTests.B10_ExactReferenceRejectsOtherLevelVersionOrBindingWithoutBusinessWork
FightMatch.Core.Tests.PlayerBattlePresentationTests.UGUI_COPY_D02_DurationUsesAcceptedSnapshotAndInvariantCeiling
FightMatch.Core.Tests.PlayerBattlePresentationTests.UGUI_COPY_WIRE03_ExistingHistorySaveSkipAndEndKeysFollowRealStateWithoutChecking
FightMatch.Core.Tests.PlayerBattlePresentationTests.B06_B09_EntireViewTreeRebuildPreservesHostInputPlaybackAndOriginalAttack
FightMatch.Core.Tests.PlayerNavigationRecoveryTests.CC14_CC15_WriteFaultKeepsTheOriginalCandidateAndConfirmation
FightMatch.Core.Tests.PlayerNavigationRecoveryTests.CC17_RCV1_RebuildDiscardsAllRequestsThenRecoversOriginalIntentThroughProductionQuery
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_05_ActualPanelLabelsBoardOverrideAndNextAttackChain
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_05_RealUIToolkitScheduleAdvancesActualFacts
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_ReattachUnsubscribesOldControllerAndDoesNotReplayPriorFacts
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_DuplicateOwnerCannotTakeOverOrDeinitializeTheActiveArchitecture
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_ChangedBeforeHostRequestedStillDeliversOneOriginalH02AndRejectsDuplicates
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_RootBackMeansApplicationQuitAndCancelPreservesTheSave
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_OriginalEndReceiptCanReturnAndBeExplicitlyOpenedAgainAfterColdRebuild
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_ReplacingTheWholeViewKeepsFailedEndRequestAndOriginalResolution
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_ColdObservedMenuSaveUsesOriginalNavigationRecovery
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_RealBoardVictorySettlesOnceAndTheOriginalReceiptSurvivesWholeHostReconstruction
```

Parameterized cases must be selected by NUnit full-name evidence, not a raw method-name filter that changes expected counts. The signed packet must store the exact expected test list and result list. It may reuse an R-accepted result only when the test binary/source, fixture, product inputs, Prefab, Scene, and selector are all unchanged. A FIX11 label alone is insufficient, and FIX12 currently documents that FIX11 focused had four failures.

### Q3 — Mac Unity compile/import and resource reopen

Only after Q0 authorization and after verifying no other Unity process has the project open:

1. Open/import in the approved Intel Mac Unity 2022.3.18f1 editor path bound by the packet.
2. Save the Runtime Prefab and Demo Scene natively.
3. Close and reopen both resources.
4. Verify one Canvas, one GraphicRaycaster, one EventSystem, one Prefab instance, no missing scripts, no unresolved refs, stable Prefab/Scene GUIDs, and only explained Scene overrides.
5. Compile/import with a fresh log if the packet permits the exact command.

This design task reports Mac Unity as `NOT_RUN`.

### Q4 — runtime geometry and interaction matrix

Capture a runtime table and screenshots for:

1. 540×960 with physical safe rect `(16,16,508,920)` — all four sides nonzero.
2. 1080×2400 with physical safe rect `(32,80,1016,2160)` — all four sides nonzero.
3. one exact short-height case where `812k <= H < 920k` showing Stage-first then BottomHud compression.
4. one exact below-minimum case `H < 812k` showing visible fail-closed LayoutDiagnostic and disabled Battle input.
5. one live SafeArea/runtime resize transition without a zero-sized rendered frame.

Every row records physical resolution/safe rect, `SafeAreaFitter.Version`, Canvas rect, SafeAreaRoot rect W/H, Canvas `scaleFactor`, Canvas/SafeAreaRoot `lossyScale`, k, all five region rects, board rendered corners, and board hit-conversion corners.

Runtime interaction evidence must include:

- board square and rendered/hit source identity;
- an actual board drag while Reference is open: drawer does not cover/swallow the board, first drag closes it, pointer sequence completes, feedback occurs once;
- History open/close with `fm.action.history.open` and `fm.history.title` visible;
- Normal/History/Reference mutual exclusion across route and locale rebind;
- long English and Simplified Chinese modal text scrolling while Header/Actions remain fixed;
- long English and Simplified Chinese navigation and battle Recovery bodies;
- System Recovery visibly/raycast-wise above Popup;
- fixed ally/enemy/member slots across alive/down/empty cases;
- retry/resolve/skip/member actions retain at least 48k hit zones;
- pointer cancel, focus loss, playback token replacement, rollback cancel, confirmation cancel, recovery route, destroyed references, and stale callback cases from the lifecycle matrix.

### Q5 — full regression and resource/log audit

Run the exact accepted full suite after focused success. The expected full-name manifest and count are rebound after the current correction chain; this design intentionally does not freeze the stale FIX11 count.

Final audit requires:

- zero failed/skipped tests unless an independently approved manifest explicitly says otherwise;
- zero unexpected Error/Exception/Assertion/MissingReference/missing binding/duplicate ViewId entries;
- no visible diagnostic placeholder such as `【if you see this, it is a bug.】` in any runtime screenshot;
- no missing glyph box or fallback warning for required EN/ZH copy;
- Prefab/Scene reopen identity and GUID checks;
- changed-file whitelist proof and `.meta` proof;
- before/after source/resource manifests and final candidate SHA.

### Q6 — Android/device and professional art acceptance

Android/device is a separate required lane and is `NOT_RUN` by this design. It must validate the same two inset/resolution classes on real or approved device infrastructure, touch slop/drag behavior, System Recovery precedence, glyphs, safe-area changes, and screenshot geometry. Mac Editor evidence cannot substitute for device evidence.

The art lead must issue a professional acceptance on the implemented resource/screens. The earlier `ACCEPT（仅响应式几何表）` cannot be promoted into visual/resource acceptance.

QA sends the central adjudication, this complete document, implementation receipt, changed-file manifest, test manifests/results, runtime tables/screens, resource hashes/GUIDs, Mac evidence, Android/device evidence, and art verdict to the same R-TOOLS reviewer. Only R returns `ACCEPT`, `NEEDS_FIX`, or `REJECT` for the delivery.

## 12. Evidence paths and stop gates

Future evidence root:

```text
TestArtifacts/FightMatch/UGUI-LAYOUT-001/<candidate-sha>/
```

Required sub-artifacts:

```text
preflight/status.txt
preflight/preimages.sha256
preflight/resource-guids.txt
static/exact-test-fullnames.txt
static/results.xml
focused/exact-test-fullnames.txt
focused/results.xml
compile/AICompile.log
runtime/layout-table.csv
runtime/540x960/
runtime/1080x2400/
runtime/short-height/
runtime/fail-closed/
runtime/modal-en-zh/
runtime/recovery-en-zh/
runtime/interaction.log
full/exact-test-fullnames.txt
full/results.xml
resources/reopen-manifest.sha256
resources/scene-overrides.txt
logs/unity-errors.txt
art/verdict.md
qa/receipt.md
r-tools/verdict.md
```

Stop immediately and report `BLOCKED` or return a corrective packet when any of these occurs:

1. no independent accepted baseline for the current UGUI correction chain;
2. any preimage/GUID/`.meta` mismatch;
3. another Unity process owns the project;
4. an unlisted file, public API, dependency, asmdef, package, ProjectSettings, save format, or business action appears necessary;
5. another Canvas/EventSystem/Runtime Prefab is introduced;
6. Reference covers or raycasts the board, Recovery remains in PopupLayer, or History is absent;
7. duplicate/missing fixed ViewId or stale dynamic `fm.member.*` object;
8. fixed slots reorder, compact, or fall back from invalid OriginalSlot;
9. Board is not square, hit conversion uses another rect, a 48k hot zone shrinks, or `H<812k` does not fail closed visibly;
10. a same-axis LayoutGroup/ContentSizeFitter/AspectRatioFitter/responsive-owner conflict exists;
11. first frame renders a zero/invalid layout or runtime evidence assumes scale/lossyScale equals one;
12. hide/close causes action completion, route change, playback completion, or save write;
13. focused/static/resource-reopen validation fails or the full-name manifest drifts;
14. placeholder, missing glyph, MissingReference, binding, raycast, or Unity log error remains;
15. Mac, Android/device, professional-art, QA, or R evidence is missing for a completion claim.

## 13. Review checklist and conclusion

The limited reviewer should answer exactly these questions:

- Does the hierarchy implement the central adjudication without adding a second architecture?
- Is every current container and serialized field mapped to one target owner?
- Are TopBar, Battle five-region geometry, short-height minima, and numeric examples internally consistent?
- Can any layout component compete for the same axis?
- Can Reference, History, Popup, Recovery, or LayoutDiagnostic swallow Board input incorrectly?
- Are controller/page/epoch/generation/token/unsubscribe semantics preserved?
- Are fixed slots deterministic by OriginalSlot, including down and empty cases?
- Is the candidate whitelist exact and sufficiently narrow?
- Does validation distinguish static, Mac Unity, Android/device, art, QA, and R evidence without fabricating results?

Conclusion: `READY_FOR_LIMITED_REVIEW`.

The design is complete enough for engineering-lead receipt and same-R-TOOLS review. It is not implementation authorization and makes no claim that Unity, Mac, Android/device, resources, tests, professional art, QA, or R have accepted the future implementation.
