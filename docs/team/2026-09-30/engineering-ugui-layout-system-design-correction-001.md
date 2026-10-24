# UGUI-LAYOUT-001 system-design correction addendum 001

2026-10-01（Asia/Shanghai）

Status: `READY_FOR_LIMITED_R`

Implementation authorization: **none**. This addendum is a design-only correction. It does not authorize product, test, Prefab, Scene, `.meta`, localization, package, project-setting, evidence, or Git changes; it does not authorize Unity, build, test, APK, device, or cloud execution.

Author/owner: SYS-ENG-001 system-design owner

- author thread: `01a0f2e6-29bb-7092-abf0-705b41b7bf93`
- host: `local`
- author turn: `01a0f556-c69d-7bb2-b41c-5ec70b34258c`
- dispatch source: engineering lead `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`
- authority: user-approved `AGENTS.md` section 6 workflow and central thread `01a0e401-511d-79f2-b47f-3ab0ade1681b`

## 0. Frozen inputs and reading rule

The following four files were read and matched the delegated identities before this file was created:

| input | lines | bytes | SHA-256 |
|---|---:|---:|---|
| `central-layout-hierarchy-adjudication-001.md` | 69 | 7,011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` |
| `engineering-ugui-layout-system-design-001.md` | 961 | 68,763 | `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce` |
| `art-ugui-layout-design-receipt-001.md` | 81 | 7,567 | `098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f` |
| `testing-ugui-layout-design-review-001.md` | 161 | 25,145 | `ea0077d32238b0a09f898bf9a4630720cfa13daf41489f575b72a3fb17c308cc` |

The base design remains frozen and is not rewritten. The only executable reading of the design candidate is:

1. user request and `AGENTS.md`;
2. central hierarchy adjudication;
3. this correction addendum;
4. every base-design clause not explicitly superseded here.

ART-03 and the R-TOOLS report are findings/evidence, not competing design authorities. This addendum supersedes only the base clauses named in sections 2–11 below. If an implementation packet quotes the base without this addendum, or quotes this addendum without the frozen base, it is incomplete and must stop.

The combined candidate preserves:

- Reference → BottomHud replacement drawer;
- Normal/History/Reference mutual exclusion;
- Recovery → SystemLayer;
- one Runtime Prefab, one player Canvas, one GraphicRaycaster, one project EventSystem;
- existing business, save, gesture, playback-token, route, epoch/generation, feedback, and localization ownership;
- diagnostic Prefab placeholders and Q0 dependency gates.

The current Q0 baseline is not accepted. FIX13 ended `BLOCKED_EXACT4`: 2 passed / 2 failed / 0 skipped. B16 cancellation and WIRE01 terminal-view retry passed; P27 disable-time row ordering and B09 destroyed-text access failed. Candidate `15cfc5ce2da83661fc2c3aab01d4c8d7052af3e9421541a21f96ae7d838ed16f`; source candidate `fbc273bb02b6bd4071551ff2f291011a05cc982b45c148afe34ee42a345127a6`. Exact 222 and native final reopen did not run. FIX14 is authorized and in progress; no accepted baseline exists yet. These are failure facts, not LAYOUT implementation preimages.

## 1. Closure matrix

| finding | disposition | closure section |
|---|---|---|
| R B01 | BoardFrame keeps only CandidateBoardElement Graphic; decoration is a separate node; every container and LocalizedTmpText target is typed separately | §2, §3 |
| R B02 | ally domain, first-release enemy content, three-slot display capacity, and legal unsupported enemy input are four distinct facts | §4 |
| R B03 / ART N-02 | NormalHud receives an exact vertical ScrollRect → Viewport → Content chain and single-axis ownership | §3 |
| R B04 / ART N-04 | all fixed/dynamic Battle actions and status rows are enumerated; fixed and dynamic actions use separate groups inside a bounded horizontal command ScrollRect | §5 |
| R B05 / ART N-03 | Reference has one 48k Header Close; Actions height is exactly zero; body is 94k/56k | §6 |
| R B06 | Host uniquely owns both outer roots/masks; panels remain view-owned; state table covers enter/switch/close/unbind/reopen/return | §7 |
| R B07 | candidate/original-operation selections move into bounded scroll-body choice lists; execution actions remain fixed outside | §8 |
| R B08 | Startup/Navigation/Result receive exact shared-TopBar avoidance and single-axis owners | §9 |
| R B09 / ART N-01 | long-screen safe rect returns to `(32,120,1016,2160)` with SVG-top/Unity-bottom conversion | §10 |
| R B10 | validation becomes an executable dependency DAG with true Overlay evidence, exact manifest freeze points, 32-instance Q2 identity, controlled union, and final reopen last | §11 |

ART N-01 through N-03 are closed by the corresponding R closures above. ART N-04's `488k` arithmetic is acknowledged as correct only for three 72k cards plus four 48k actions; it is **not adopted** because it omits four retained PlayerBattleView actions and their status rows. Section 5 replaces it with a capacity design that includes every current entry point.

## 2. Corrected board node and Graphic ownership — B01

The base hierarchy's `BoardFrame [Image; CandidateBoardElement]` is superseded by:

```text
BattleContent
└── BoardRegion                         [RectTransform; responsive B × B owner]
    ├── BoardDecoration                 [UnityEngine.UI.Image; raycastTarget=false]
    └── BoardFrame                      [CanvasRenderer]
                                        [CandidateBoardElement; only Graphic]
                                        [FightMatchViewId: fm.board.candidate]
```

Rules:

1. `CandidateBoardElement`, which already derives from `UnityEngine.UI.MaskableGraphic`, is the only `UnityEngine.UI.Graphic` on `BoardFrame`.
2. No `UnityEngine.UI.Image` is added to `BoardFrame`. This avoids the uGUI `DisallowMultipleComponent` conflict.
3. `BoardDecoration` is the only optional decorative Image for this region. It stretches to `BoardRegion`, renders behind BoardFrame, and never raycasts.
4. `FightMatchResponsiveLayout` writes `BoardRegion` width and height to `B`. BoardFrame and BoardDecoration stretch to that rect with zero offsets. Board rendering and hit conversion continue to consume `CandidateBoardElement.rectTransform`, which is therefore the same B × B rect.
5. `CandidateBoardInputView.board` remains typed `CandidateBoardElement`; it is never changed to Image, Graphic, RectTransform, or an untyped lookup.

No implementation may add a second Graphic to BoardFrame to satisfy a background requirement.

## 3. Corrected NormalHud hierarchy and typed bindings — B01, B03, N-02

The base `StatusViewport/Content` shorthand is superseded by this exact hierarchy:

```text
BottomHud
└── NormalHud
    ├── StatusViewport                   [UnityEngine.UI.ScrollRect; vertical=true; horizontal=false]
    │   └── Viewport                     [RectTransform + UnityEngine.UI.Image + UnityEngine.UI.Mask]
    │       └── Content                  [RectTransform + VerticalLayoutGroup + ContentSizeFitter]
    │           ├── Save                 [LocalizedTmpText]
    │           ├── BattleHudFirstLine   [LocalizedTmpText; PlayerBattleView.hud]
    │           ├── AvailabilityFirstLine[LocalizedTmpText]
    │           ├── PlaybackDiagnostic   [LocalizedTmpText]
    │           └── BattleNotice rows    [dynamic LocalizedTmpText clones]
    ├── MainRow                          [fixed 72k high]
    │   ├── MemberStrip
    │   └── CommandStrip                 [horizontal ScrollRect; §5]
    └── BottomPadding                    [fixed 8k high; no Graphic]
```

Required ScrollRect references and axes:

- `StatusViewport.viewport` = `StatusViewport/Viewport`;
- `StatusViewport.content` = `StatusViewport/Viewport/Content`;
- vertical enabled, horizontal disabled;
- Viewport Image exists because Mask requires it; it may raycast only inside StatusViewport to receive the local scroll drag;
- Content is top-anchored, uses `VerticalLayoutGroup`, and `ContentSizeFitter.verticalFit=PreferredSize`, `horizontalFit=Unconstrained`;
- ContentSizeFitter sizes only Content. It never writes StatusViewport, NormalHud, BottomHud, or MainRow height.

Typed serialized mappings supersede all base paths that ended in `StatusViewport/Content`:

| owner/field | exact target/type |
|---|---|
| CandidateBoardInputView `save` | `.../Content/Save`, `LocalizedTmpText` |
| PlayerBattleView `hud` | `.../Content/BattleHudFirstLine`, `LocalizedTmpText` |
| CandidateBoardInputView `availability` | `.../Content/AvailabilityFirstLine`, `LocalizedTmpText` |
| CandidateBattlePlaybackView `diagnostic` | `.../Content/PlaybackDiagnostic`, `LocalizedTmpText` |
| PlayerBattleView new `battleNotices` | `.../Content`, `RectTransform` |
| FightMatchResponsiveLayout `normalStatusViewport` | `NormalHud/StatusViewport`, `RectTransform`/ScrollRect owner |
| FightMatchResponsiveLayout `normalMainRow` | `NormalHud/MainRow`, `RectTransform` |

The full affected field disposition is therefore unambiguous:

| component/field | corrected disposition |
|---|---|
| CandidateBoardInputView `board` | remains `CandidateBoardElement` on BoardFrame |
| CandidateBoardInputView `phase` | fixed `BattleStatus/Phase` LocalizedTmpText |
| CandidateBoardInputView `save` | fixed Status Content/Save LocalizedTmpText |
| CandidateBoardInputView `enemies` | retired; aggregate enemy rows are replaced by fixed Stage slot labels |
| CandidateBoardInputView `availability` | fixed Status Content/AvailabilityFirstLine LocalizedTmpText |
| CandidateBoardInputView `members` | retired; replaced by exactly three serialized member-slot RectTransforms |
| CandidateBoardInputView `memberTemplate` | remains inactive template; clones only inside one fixed member slot |
| CandidateBoardInputView `retry`, `resolve` | fixed Buttons in FixedBattleActions |
| CandidateBattlePlaybackView `inputView` | unchanged component reference |
| CandidateBattlePlaybackView `hp`, `intent` | retired aggregate labels; playback writes fixed Stage labels through the internal input-view presentation seam |
| CandidateBattlePlaybackView `beat`, `stage` | fixed BattleStatus labels |
| CandidateBattlePlaybackView `diagnostic` | fixed Status Content/PlaybackDiagnostic label |
| CandidateBattlePlaybackView `skip` | fixed Button in FixedBattleActions |
| PlayerBattleView `hud` | fixed Status Content/BattleHudFirstLine LocalizedTmpText |
| PlayerBattleView `actions` | DynamicBattleActions RectTransform only |
| PlayerBattleView new `battleNotices` | Status Content RectTransform for settlement/notification text only |
| PlayerBattleView new `historyOpenButton` | fixed FixedBattleActions/HistoryOpen Button |

`BattleLocalizedRows(hud, "battle-hud")` clones only LocalizedTmpText siblings after `BattleHudFirstLine`. Candidate availability clones only siblings after `AvailabilityFirstLine`. The Content is never passed where a LocalizedTmpText is required, never hidden as a label, and never cloned.

BottomHud height allocation is exact:

```text
MainRow height      = 72k
BottomPadding       = 8k
StatusViewport      = BottomHud - 80k
preferred 142k HUD  -> StatusViewport 62k
minimum 104k HUD    -> StatusViewport 24k
```

The local StatusViewport clips and scrolls long Save, HUD, Availability, PlaybackDiagnostic, settlement-pending, and notification rows. It cannot grow BottomHud or overlap MainRow/Board.

## 4. Slot domains and unsupported display handling — B02

Four facts must not be collapsed:

| concern | frozen rule |
|---|---|
| ally/member domain | valid published members have unique `OriginalSlot` in 0..2; this existing Core rule is unchanged |
| first-release enemy content | the current first-release level contains two enemies with OriginalSlot 0 and 1 |
| visual capacity | the first-release Battle Stage has three enemy visual slots 0..2 and three ally/member visual slots 0..2 |
| enemy domain | valid enemy OriginalSlot is unique and nonnegative; Core has no upper bound of 2 and this design does not add one |

Slot binding rules:

1. Allies and members bind by their valid 0..2 OriginalSlot. Empty and down slots stay in place; no compaction.
2. Enemies with OriginalSlot 0..2 bind to `EnemySlot0..2`. Empty/down slots stay in place.
3. The first-release source identity and its enemy slot set `{0,1}` must be rebound at implementation Q0. The design does not generalize that content observation into a domain rule.
4. A legal non-first-release/legacy enemy using slot greater than 2, or more than three present enemies, produces `UnsupportedEnemySlotLayout` at the presentation seam. The snapshot, Core validation, slot number, save, and route are not rewritten or rejected.
5. In `UnsupportedEnemySlotLayout`, CandidateBoardInputView binds the already-approved `fm.entry.binding_unsupported` copy into `AvailabilityFirstLine`, disables Board raycast/member/retry/resolve/skip and non-exit Battle commands, leaves the existing Exit route reachable when its existing lifecycle availability permits, and performs zero business/save writes. It does not show a fake compacted enemy.
6. Duplicate/negative enemy slots remain the existing invalid-domain/binding path; they are not called UI unsupported. An impossible ally/member slot outside 0..2 remains an infrastructure/data diagnostic, not an enemy-capacity fallback.
7. `CandidateBoardInputView` may expose one internal read-only display-support value consumed by PlayerBattleView. This is an internal presentation seam, not a public/domain interface.

Required tests retain legal sparse enemy examples such as slots 7/2 or 8 as domain-valid inputs, assert no Core validation change, and then assert the explicit presentation-only unsupported state. No test may change those inputs to 0..2 merely to make the three-slot UI pass.

## 5. Complete Battle controls, status mapping, and capacity — B04, N-04

### 5.1 Every entry point and status row

All current Battle controls are preserved and mapped:

| item | kind | visible/present rule | exact target |
|---|---|---|---|
| three member selections | dynamic child in fixed slot | one per existing member; down remains visible disabled; empty has no child | `MemberStrip/MemberSlot0..2` |
| Retry | fixed Button | always occupies its cell; interactable = existing `CanRetry` | `FixedBattleActions/Retry` |
| Resolve | fixed Button | always occupies; interactable = existing `CanResolve` | `FixedBattleActions/Resolve` |
| Skip | fixed Button | always occupies; interactable only during existing playback | `FixedBattleActions/Skip` |
| HistoryOpen | fixed Button, `fm.action.history.open` | active when `battle.History != null`; enabled only when the drawer has content allowed by existing state | `FixedBattleActions/HistoryOpen` |
| Exit | dynamic Button | created whenever `battle.History != null`; existing lifecycle availability controls interactable | `DynamicBattleActions/Exit` |
| Restart | dynamic Button | same creation rule; existing restart availability | `DynamicBattleActions/Restart` |
| Settle | dynamic Button, `fm.action.victory.settle` | same creation rule; existing `CanSettle`; disabled still occupies | `DynamicBattleActions/Settle` |
| Reference | dynamic Button, `fm.action.reference.open` | same creation rule and existing reference lookup | `DynamicBattleActions/Reference` |

Status text is not placed in either action group:

| text | exact target |
|---|---|
| page headline/current phase/result title | shared `HudLayer/TopBar/Title` |
| input phase | `BattleStatus/Phase` |
| playback stage | `BattleStatus/PlaybackStage` |
| playback beat | `BattleStatus/Beat` |
| actor HP/intent | fixed Stage slot labels |
| save state | NormalHud Status Content/Save |
| party level/xp/recovery and inventory HUD rows | Status Content/BattleHudFirstLine + clones |
| availability, selected member, gesture instruction, visible feedback | Status Content/AvailabilityFirstLine + clones |
| playback diagnostic | Status Content/PlaybackDiagnostic |
| settlement-pending/unavailable | dynamic BattleNotice row in Status Content |
| application/battle notification | dynamic BattleNotice row in Status Content, or Recovery StateRows when Recovery route owns it |

History rows remain in HistoryDrawer local vertical ScrollRect. Rollback confirmation details remain in the ordinary confirmation Body. No status label occupies an action-cell LayoutElement.

### 5.2 MainRow and horizontal command capacity

The ART N-04 `488k` proposal is insufficient because it omits Exit/Restart/Settle/Reference. The corrected 72k MainRow is:

```text
MainRow                                           width B = 508k
├── left padding                                  8k
├── MemberStrip                                   232k
│   ├── MemberSlot0                               72k × 72k
│   ├── gap                                        8k
│   ├── MemberSlot1                               72k × 72k
│   ├── gap                                        8k
│   └── MemberSlot2                               72k × 72k
├── member/command gap                             8k
├── CommandStrip                                  252k × 72k, horizontal ScrollRect
│   └── Viewport                                  Image + Mask
│       └── Content                               HLG + horizontal CSF
│           ├── FixedBattleActions                776k × 72k
│           │   └── 184k, 224k, 200k, 144k cells × 48k; 3 × 8k gaps
│           ├── group gap                          16k
│           └── DynamicBattleActions              max 832k × 72k
│               └── 152k, 256k, 248k, 152k cells × 48k; 3 × 8k gaps
└── right padding                                  8k
```

Outer width proof:

```text
8k + 232k + 8k + 252k + 8k = 508k = B
```

Command content worst-case proof:

```text
FixedBattleActions  = 184k + 224k + 200k + 144k + 3 × 8k = 776k
DynamicBattleActions= 152k + 256k + 248k + 152k + 3 × 8k = 832k
group gap           = 16k
Content preferred   = 776k + 16k + 832k = 1624k
Viewport            = 252k
scroll range        = 1624k - 252k = 1372k
```

Caption capacity is frozen from the existing EN/ZH localization values and `NotoSansCJKsc-Regular-TMP.asset` glyph advances. The following values are at the k=1 reference geometry with TMP font size 18, auto-size off, character spacing 0, word spacing 0, and X/Y glyph scale 1.0. Every cell has left/right padding `12k + 12k` and top/bottom padding `8k + 8k`. The font face line height at size 18 is `130.32 / 90 × 18 = 26.064`, which fits inside the 32k-high text rect without vertical compression. `min cell width = max(ZH advance, EN advance) + 24k`; chosen preferred width rounds that value up to the next 8k boundary.

| command, frozen localization key | ZH Caption / advance | EN Caption / advance | min text width | min cell width | chosen preferred width | lines / wrap breakpoint |
|---|---:|---:|---:|---:|---:|---|
| Retry, `fm.save_recovery.retry_button` | 重试原保存 / 90.0000k | Retry Original Save / 159.4500k | 159.4500k | 183.4500k | 184k | 1 / none |
| Resolve, `fm.save_recovery.confirm_result_button` | 确认原保存结果 / 126.0000k | Confirm Original Result / 195.1656k | 195.1656k | 219.1656k | 224k | 1 / none |
| Skip, `fm.battle.playback.skip_button` or `fm.victory.playback.skip_button` | 跳到当前战局 or 跳过末段表现 / max 108.0000k | Skip to Current State or Skip Final Sequence / max 175.0938k | 175.0938k | 199.0938k | 200k | 1 / none for both states |
| HistoryOpen, `fm.history.title` | 行动记录 / 72.0000k | Action History / 118.3219k | 118.3219k | 142.3219k | 144k | 1 / none |
| Exit, `fm.operation.exit_attempt` | 退出本局 / 72.0000k | Exit This Battle / 125.0656k | 125.0656k | 149.0656k | 152k | 1 / none |
| Restart, `fm.operation.restart_attempt` | 按原开局重来 / 108.0000k | Restart from Original Entry / 224.3844k | 224.3844k | 248.3844k | 256k | 1 / none |
| Settle, `fm.victory.settle.button` | 继续基础奖励结算 / 144.0000k | Continue to Base Rewards / 219.6813k | 219.6813k | 243.6813k | 248k | 1 / none |
| Reference, `fm.reference.play_button` | 播放参考 / 72.0000k | Play Reference / 124.3656k | 124.3656k | 148.3656k | 152k | 1 / none |

The inherited two-line limit remains an upper bound, but none of these frozen captions uses a second line: each is one line with no wrap breakpoint. In particular, Resolve's seven-character `确认原保存结果` and Settle's eight-character `继续基础奖励结算` fit their chosen cells without wrapping, and every English Caption is closed by its measured normal advance. A future Caption that cannot satisfy this table is a new design input, not permission to auto-size, tighten character spacing, condense glyphs, shorten/replace localization, add an icon, truncate, or exceed two lines.

Rules:

- `CommandStrip` is horizontal-only; Content uses `HorizontalLayoutGroup` plus `ContentSizeFitter.horizontalFit=PreferredSize`, vertical unconstrained.
- FixedBattleActions and DynamicBattleActions are separate RectTransform containers with the exact preferred widths above. A disabled button still occupies its full chosen cell width.
- Every action hit area is its chosen preferred width × `48k`, and therefore remains greater than the accepted `48k × 48k` minimum. Buttons are vertically centered inside the 72k row. Member hit/card area is `72k × 72k`.
- At the long-screen sample every dimension scales by k; no additional short-height shrink is applied.
- A small scrollbar or equivalent nonblocking scroll affordance may occupy the unused 24k vertical band; it cannot reduce the 48k action hit height.
- The reference typography, padding, normal advances, one-line counts, and no-wrap breakpoints above scale with the existing responsive geometry; TMP auto-size remains off. No locale may reduce the accepted 18 reference-size font, character spacing, word spacing, glyph width/height, or padding to make a Caption fit.
- Content uses clamped horizontal movement. At normalized position 0 the first Retry cell is wholly visible; at normalized position 1 the final Reference cell is wholly visible. The eight-command order is exactly Retry, Resolve, Skip, HistoryOpen, Exit, Restart, Settle, Reference.
- CommandStrip, StatusViewport, and all their raycast receivers remain wholly inside BottomHud. They never overlap BoardRegion.
- UI unsupported enemy layout from §4 leaves Exit reachable by scrolling to its retained cell; other commands are presentation-disabled without removal/reorder.

This is the only accepted MainRow capacity contract. No implementation may substitute the four-action 488k calculation.

## 6. Reference drawer capacity — B05, N-03

The Reference drawer is exactly:

```text
ReferenceInfoDrawer
├── Header                              fixed 48k
│   ├── Title
│   └── Close                           one fixed 48k × 48k Button
├── BodyViewport                        vertical ScrollRect
│   └── Viewport                        Image + Mask
│       └── Content                     VLG + vertical CSF
│           ├── Explanation
│           ├── Beat
│           └── Steps / difference controls
└── Actions                             fixed height 0; inactive; no raycast
```

`PlayerDefaultReferenceView.closeButton` binds only to `Header/Close`. It has no alternative Actions target. Existing reference behavior has no footer-only business action, so Actions remains a structural zero-height node for consistent panel topology.

Capacity:

```text
BottomHud preferred 142k - Header 48k = Body 94k
BottomHud minimum   104k - Header 48k = Body 56k
Actions = 0k in both cases
```

Open, Close, reopen, locale change, first real drag, pointer cancel, focus loss, route exit, and Unbind use the base lifecycle contract. The single Close callback is cleaned and rebound once. First real drag still records the pointer, closes presentation, then publishes reference-closed feedback without canceling that active pointer.

## 7. Shared outer-root ownership and transition table — B06

### 7.1 Unique owner

`FightMatchHostView` is the unique writer of these two outer roots and their blockers:

```text
PopupLayer/ConfirmationPopup                    [fm.popup.confirmation]
└── RootMask                                    [full blocker]
    ├── NavigationConfirmationPanel             [owned by PlayerNavigationRecoveryView]
    └── BattleConfirmationPanel                 [owned by PlayerBattleView]

SystemLayer/RecoveryScreen                      [fm.popup.recovery]
└── RootMask                                    [full blocker]
    ├── NavigationRecoveryPanel                 [owned by PlayerNavigationRecoveryView]
    └── BattleRecoveryPanel                     [owned by PlayerBattleView]
```

Host receives exact serialized refs:

- `confirmationPopupRoot`, `confirmationRootMask`;
- `navigationConfirmationPanel`, `battleConfirmationPanel`;
- `recoveryScreenRoot`, `recoveryRootMask`;
- `navigationRecoveryPanel`, `battleRecoveryPanel`.

Child views may set only their own panel active state and content/interactable state. They never set the shared outer root/mask. At the end of every Host render, after both relevant child renders, Host computes the outer state from these explicit serialized panel refs; it does not use `Transform.Find` or descendant search. Host Unbind disables both outer roots after unbinding children.

Outer activation formulas:

```text
recoveryActive = navigationRecoveryPanel.activeSelf XOR battleRecoveryPanel.activeSelf
confirmationActive = !recoveryActive
    && (navigationConfirmationPanel.activeSelf XOR battleConfirmationPanel.activeSelf)
```

Both panels active under one outer root is an infrastructure diagnostic and stop gate, not a winner-selection rule. When Recovery becomes active, Host first disables Confirmation and all ordinary Popup interaction, then enables RecoveryScreen. BlockingDiagnostic > LayoutDiagnostic > RecoveryScreen > Popup remains the priority order.

### 7.2 Lifecycle table

| transition | child-panel owner action | Host outer-owner action | stale callback/result |
|---|---|---|---|
| enter navigation confirmation | nav view rebuilds nav panel; battle panel off | enable Confirmation root/mask | current generation only |
| enter battle confirmation | battle view rebuilds battle panel; nav panel off | enable Confirmation root/mask | current page/epoch only |
| switch navigation ↔ battle owner | old owner clears/hides before new owner builds | one render recomputes; never two panels | old callbacks invalid |
| enter navigation Recovery | nav view clears confirmation and builds nav Recovery | disable Popup interaction, then enable Recovery | current generation only |
| enter battle Recovery | battle view clears confirmation and builds battle Recovery | same order | current page/epoch only |
| cancel/close confirmation | active view clears rows/listeners and hides panel | disable Confirmation when no panel remains | hide is not completion |
| Recovery safe Return/exit | existing action completes through controller; active panel then hides | disable Recovery mask before returned page becomes interactive | no click-through/old callback |
| Recovery switches candidate/original selection | same panel is rebuilt; outer remains on | no mask toggle/flicker | request identity captured per row |
| route to ordinary page | child panels both off | both outer roots off; ordinary Popup may then open | no blocker remains |
| locale rebind | rebuild active panel only | outer state unchanged | callbacks rebound once |
| Unbind/Dispose | child clears bindings and hides its panels | Host disables both outer roots/masks | old generation/epoch invalid |
| reopen | starts from both outers off; current route builds exactly one panel | enable exactly one matching outer | no row/button reparent |

### 7.3 OriginalResult without reparenting

The existing fixed `originalResultButton` field is retired. `OriginalResult` becomes one dynamic choice-row clone created in the active panel's scroll-body `ChoiceRows` when the existing `IsPublishedHeadVerified`/action availability supports it.

- identity remains exactly one `FightMatchViewId.Row("save.OriginalResult")` object;
- callback remains `PlayerNavigationAction.SelectOriginalOperation`;
- it is reachable in every route where the current view exposes it, including Confirmation and Recovery/CommittedResult;
- route switch disposes/destroys the old clone before creating the new one;
- it is never runtime-reparented and never duplicated across inactive panels.

## 8. Scroll-body choices versus fixed execution actions — B07

Every Confirmation/Recovery panel Body uses:

```text
BodyViewport                             [vertical ScrollRect]
└── Viewport                             [Image + Mask]
    └── Content                          [VLG + vertical CSF]
        ├── StateRows                    [VLG + vertical CSF]
        └── ChoiceRows                   [VLG + vertical CSF]
```

The outer Content VLG has `childControlHeight=false` and `childForceExpandHeight=false`; StateRows and ChoiceRows size themselves to preferred height. Each section's VLG controls its own child heights. This prevents a parent VLG from overriding the section ContentSizeFitter on the same axis.

Placement and cardinality:

| row/action | container | cardinality | identity/callback |
|---|---|---:|---|
| titles | fixed Header | exactly 1 per active panel | fixed LocalizedTmpText |
| save/application/pending/quote/notification text | Body/StateRows | 0..N text rows | existing row identities/bindings |
| observed navigation candidates | Body/ChoiceRows | 0..N | `fm.row.save.candidate.{commit}` → existing ObservedCandidate target |
| published original operations | Body/ChoiceRows | 0..N | `fm.row.save.operation.{operationId}` → existing OriginalOperation target |
| OriginalResult lookup | Body/ChoiceRows | 0..1 | `fm.row.save.OriginalResult` → SelectOriginalOperation |
| observed battle candidates | Body/ChoiceRows | 0..N | existing `FightMatchViewId.Row("resume:" + commit)` → ResumeObserved(page, commit, context) |
| Confirm | fixed Confirmation SafeActions | 0..1 | existing Confirm action |
| End confirmation | fixed Confirmation DangerActions | 0..1 | existing End action |
| navigation Return/ResumeObserved/Retry/Resolve/Refresh | fixed Recovery SafeActions | each 0..1; max 5 | existing action and reason |
| navigation EndReview | fixed Recovery DangerActions | 0..1 | existing EndConfirmation target |
| battle Query/Retry/Resolve original | fixed Recovery SafeActions | each 0..1 | existing Continue action with captured original request |
| battle Query input original | fixed Recovery SafeActions | 0..1 | only if captured input request still identical |
| battle End original / End input | fixed Recovery DangerActions | each 0..1 | existing availability and captured request |

Choice buttons are interactive content inside the Body ScrollRect by explicit exception to the base sentence “Button never in Body.” Execution actions remain outside the scroll body. Every choice/action row is at least 48k high with 8k vertical separation.

Fixed Actions worst case is six 48k rows plus five 8k gaps and one 8k safe/danger divider: `336k`. At k=1 and the minimum valid Battle height, a max-height panel has `812 - 64 = 748` Canvas units; after a 48 Header and 336 Actions, 364 remains for Body. For general k, the exact Body lower-bound expression is `H - 64 - 48k - 336k`. Recovery/Confirmation layout is valid only when that value is at least `48k`; otherwise LayoutDiagnostic fails closed instead of shrinking action hot zones or allowing Actions to consume the Body. Thus fixed actions never become scroll-body children.

Final dynamic-row cleanup owners are also fixed. PlayerBattleView `OwnContainer` recognizes only `DynamicBattleActions`, `battleNotices`, History Body Content, confirmation StateRows/ChoiceRows/SafeActions/DangerActions, Recovery StateRows/ChoiceRows/SafeActions/DangerActions, and receipt Content. It destroys only rows in its own `rows` list under those parents. Fixed Headers, fixed Buttons, MemberSlot roots, Stage slot roots, ScrollRect/Viewport/Content objects, and templates are never destroyed. Navigation uses `NavigationBindings.Dispose` for the corresponding StateRows/ChoiceRows and fixed-button listeners before any repopulation.

Choice count is intentionally unbounded by presentation. More than one screen remains scrollable; it never increases Actions height. On every render, `NavigationBindings.Dispose` or PlayerBattleView `ClearRows` removes listeners/bindings/clones from the exact StateRows/ChoiceRows owned containers before repopulation. Controller/page/epoch/generation and captured request identity checks remain unchanged.

## 9. Shared TopBar geometry for all pages — B08

The old `ScreenLayer.offsetMax.y=-88` remains deleted. `ScreenLayer` and every page root stretch to SafeAreaRoot. `FightMatchResponsiveLayout` receives serialized refs to the actual content viewports:

- `StartupPage/StartupScroll`;
- `NavigationPage/NavigationScroll`;
- `ResultPage/ResultScroll`;
- `BattlePage/BattleContent` regions;
- shared `HudLayer/TopBar`.

For every valid W/H:

```text
B = min(W,508)
k = B/508
topBreathing = max(0,(H-920k)/2)
bottomBreathing = topBreathing
TopBarHeight = 56k
PageViewportLeft = (W-B)/2
PageViewportWidth = B
PageViewportTopInset = topBreathing + 56k
PageViewportBottomInset = bottomBreathing
PageViewportHeight = H - PageViewportTopInset - PageViewportBottomInset
```

For Startup, Navigation, and Result, responsive layout writes the outer scroll viewport's X, width, top inset, bottom inset, and resulting height. The existing ScrollRect writes no outer-axis value; its Content VLG/CSF controls only scroll-content height. Page roots remain visibility owners and do not write those axes.

For Battle, the same topBreathing and one shared 56k TopBar are consumed once, followed by Stage, Status, Board, BottomHud, and bottomBreathing. BattleContent does not also receive the generic PageViewport top deduction. This prevents both the old 88-unit deduction and a second 56k deduction.

Required checks cover each non-Battle page's first rendered row, first interactive control, top scroll boundary, TopBar buttons, and hit/raycast separation at both frozen resolutions and after live resize.

## 10. Corrected physical safe rectangles — B09, N-01

The 540 example remains:

```text
screen: 540 × 960 physical pixels
SVG top-origin safe rect:  x=16, yTop=24, width=508, height=920
Unity bottom-origin y:     960 - 24 - 920 = 16
Unity Screen.safeArea:     (32-bit values) (16,16,508,920)
insets L/T/R/B:            16/24/16/16
```

The 1080 example is corrected everywhere to:

```text
screen: 1080 × 2400 physical pixels
SVG top-origin safe rect:  x=32, yTop=120, width=1016, height=2160
Unity bottom-origin y:     2400 - 120 - 2160 = 120
Unity Screen.safeArea:     (32,120,1016,2160)
insets L/T/R/B:            32/120/32/120
```

Because width and height are unchanged, the base numeric results remain valid:

- Canvas scale factor `sqrt(5) ≈ 2.236067977`;
- W `≈454.369013`, H `≈965.981366`, k `≈0.894427191`;
- core `≈822.873016`;
- top and bottom **core breathing** each `≈71.554175 Canvas units = 160 physical px`;
- five region physical heights 112 / 332 / 96 / 1016 / 284.

The 160-pixel core breathing is internal layout surplus, not the physical safe-area inset. Physical top and bottom insets are both 120 pixels. The former base `(32,80,1016,2160)` example is deleted from the candidate and cannot substitute for the frozen graybox case.

## 11. Executable validation dependency DAG — B10

No step below has been run by this design correction.

### 11.1 Gate graph

```text
D0  Same R-TOOLS accepts central + base + this addendum
 |
D1  Current UGUI baseline independently accepted
 |   (current actual state: FIX13 BLOCKED_EXACT4, 2 pass / 2 fail)
 v
L0  Rebind exact source/test/resource/meta preimages and authorized file whitelist
 |
L1  Source edits + no-Unity static checks
 |
L2  Unity component compile/import
 |
L3  C native hierarchy construct -> save -> close -> reopen
 |
L4  Freeze source/resource identities and exact fullname manifests
 |\
 | +-> L5a resource/static/component focused run (39 exact instances)
 |       |
 |       v
 |     L5b true Overlay/CanvasScaler focused run (8 exact instances)
 |       |
 +-------+
         v
L6  Controlled full union (234 exact instances)
 |
L7  Final native reopen/hash/GUID/font/log/process audit after all runs
 |
L8  Android/device + implemented-art acceptance + QA receipt + same R final verdict
```

Every edge is a hard dependency. Failure stops all descendants. L3's reopen proves that the newly saved hierarchy can be reopened before tests; it is not the final identity audit. L7 occurs after every test/runtime run because those runs can affect dynamic font/resources/log evidence.

### 11.2 D0/D1 — prerequisites

- D0 must return a limited design verdict for the combined candidate; this file cannot self-accept.
- D1 must close the current UGUI correction chain first. FIX13's 2/4 exact run is not sufficient. The authorized FIX14 gate is: remaining exact2 2/2; exact222 222/222 proving P27 and B09 together with B16 and WIRE01, including fixed28 subset proof; native final reopen; then independent R `ACCEPT`. No separate exact4 or focused28 run is authorized.
- No LAYOUT source/resource preimage is frozen before D1, because ongoing correction changes overlapping files.

### 11.3 L0/L1 — no-Unity preflight and source/static work

L0 records fresh bytes/SHA for every allowed source/test/resource and both stable resource GUIDs, checks the project is not open in another Unity process, and binds a create-once evidence root.

L1 may implement the authorized source/test changes and run only no-Unity checks: changed-file whitelist, C# textual/static invariants, no second Canvas/EventSystem, no forbidden settings/dependencies, explicit serialized field names in builder/fixtures, no same-axis dual-owner declarations, and arithmetic tables. L1 cannot claim compiled components, resolved Unity serialization, saved hierarchy, or runtime geometry.

### 11.4 L2 — compile/import before resource construction

The authorized C runs a fresh Intel Mac Unity 2022.3.18f1 compile/import with the exact packet command and process budget. New `FightMatchResponsiveLayout` and modified builder/tests must compile before the builder can create target resources. Compile success is not a resource or test pass.

### 11.5 L3 — native construction and first reopen

Only after L2 succeeds, C invokes the authorized native builder to construct/update the Runtime Prefab and Demo Scene, saves them through Unity, closes/unloads them, then reopens them natively. It verifies one Canvas/GraphicRaycaster/EventSystem composition, required components, no missing refs/scripts, preserved Prefab/Scene GUIDs, explained overrides, and exact saved hashes. Handwritten YAML is forbidden.

Tests that load the target Prefab/Scene cannot run before this stage.

### 11.6 L4 — exact full-name manifest generation and freeze

After compiled test assemblies and saved resources exist, but before the first focused test process, generate and hash these ordinal-sorted, unique manifests:

| manifest | source | exact size rule |
|---|---|---:|
| `accepted-baseline-222.json` | independently accepted post-D1 manifest; byte copied/read-only | 222 unique instances |
| `layout-new-12.json` | the 12 nonparameterized `UguiResponsiveLayoutTests` full names frozen in base §11 | 12 unique instances |
| `layout-impacted-existing-32.json` | base §11 Q2's 20 methods expanded using the accepted compiled parameter cases | 32 unique instances |
| `layout-resource-existing-3.json` | the three existing UguiSceneComposition full names in base §11 | 3 unique instances |
| `layout-focused-47.json` | exact-name union of new12, impacted32, resource3 | 47 unique instances |
| `layout-controlled-full-234.json` | exact-name union of accepted222 and new12 | 234 unique instances |

Freeze checks:

1. impacted existing Q2 is **20 methods / 32 instances**. It must never be labeled “28.”
2. old fixed28 is an exact subset of accepted222 and receives no separate LAYOUT run.
3. impacted32 and resource3 must each be exact subsets of accepted222; otherwise stop for manifest drift instead of silently increasing the full set.
4. new12 must be disjoint from accepted222.
5. Union is by complete NUnit fullname, including parameter text. Duplicate fullname is rejected; it is not executed twice to inflate counts.
6. Method-name filters and unanchored regex are forbidden. Each run selector is generated from its frozen instance manifest with literal escaping and full-string anchors.

The focused phase intentionally reruns changed/impacted accepted cases after resource changes. The controlled-full phase then reruns them once as members of the final 234-instance candidate; that second execution is a phase gate, not an accidental duplicate within a manifest.

### 11.7 L5a — resource/static/component focused 39

Run the exact union of:

- impacted existing 32 instances;
- existing resource 3 instances;
- new component/static tests `LAYOUT_01`, `LAYOUT_04`, `LAYOUT_06`, and `LAYOUT_09`.

Expected: 39/39, exact name multiset, zero failed/skipped/inconclusive/error.

The existing `BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical` remains useful only for mapper/source-rect equality because its current fixture disables CanvasScaler and uses WorldSpace. It is never cited as Overlay, ScaleWithScreenSize, first-frame, or physical-pixel evidence.

### 11.8 L5b — true Overlay/CanvasScaler focused 8

Run these eight new exact names:

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

Each real-runtime test is a top-level Unity test that owns its one EnterPlayMode/ExitPlayMode lifetime; helpers do not nest or prematurely exit PlayMode. The target Canvas remains `ScreenSpaceOverlay`; CanvasScaler stays enabled in `ScaleWithScreenSize`, 540×960, match 0.5. WorldSpace or a disabled CanvasScaler cannot satisfy this lane.

Observed evidence includes:

- both frozen physical safe rects, actual Canvas/SafeAreaRoot rect, scaleFactor, lossyScale, and first rendered frame;
- Stage-first/BottomHud-second short compression plus below-minimum fail-closed;
- Startup/Navigation/Result TopBar avoidance;
- BoardRegion square, BoardFrame same-source render/hit rect, and no second Graphic;
- actual Reference first drag and CommandStrip drag without Board interception;
- exact EN/ZH binding for all eight command Captions, including both Skip states; complete single-line text with the §5.2 padding and widths; wholly reachable first Retry and final Reference cells; disabled cells retaining their chosen width; and every command hit rect at least 48k high;
- Normal/History/Reference mutual exclusion and Reference 94k/56k body;
- long EN/ZH status, popup, recovery, candidate, and original-operation lists with fixed actions visible;
- outer mask enter/switch/close/Unbind/reopen/Recovery-return states and zero click-through;
- transition to invalid layout during an active drag followed by late Up/EndDrag, existing cancellation cleanup, and zero submission;
- zero runtime placeholders, missing glyphs, duplicate IDs, missing refs, or unexpected Unity errors.

### 11.9 L6 — controlled full 234

Only after L5a 39/39 and L5b 8/8, run `layout-controlled-full-234.json`: accepted baseline222 plus new12, each exact fullname once. Expected 234/234 and exact name set, with zero failed/skipped/inconclusive/error.

L6 repeats the §5.2 two-locale Caption assertions: all eight complete EN/ZH strings and both Skip states, one-line/no-wrap rendering at font size 18 with frozen padding, first and last cells wholly reachable, disabled cells still occupying their chosen widths, and every action hot zone at least its chosen width × 48k. Focused evidence alone cannot satisfy this controlled-full gate.

This is the complete authorized LAYOUT regression set. It is not “all repository tests,” and it does not add a separate 28 run. Any desire to expand beyond 234 requires a new explicit packet and evidence budget.

### 11.10 L7/L8 — final identity and external acceptance

After L6, run one final native reopen/audit. Record final source/resource/font manifests, stable GUIDs/meta, Scene overrides, process tree/quiescence, exact commands/timing/exit, XML/log hashes, no placeholder/glyph/log errors, and equality of protected historical evidence.

Android/device evidence, implemented professional-art acceptance, QA receipt, and same-R final verdict remain separate L8 gates. Mac Editor evidence cannot substitute for device evidence; design acceptance cannot substitute for implementation acceptance.

## 12. Corrected future candidate file scope — still not authorized

The base §10 whitelist remains except that fixed-slot/domain UI coverage requires one additional existing test file. For independent checking, the corrected maximum whitelist is repeated in full:

### Production source

1. `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`
2. `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs`
3. `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs`
4. `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs`
5. `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs`
6. `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs`
7. `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs`
8. new `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs`
9. new `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta`

### Existing test/support source

10. `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs`
11. `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs`
12. `Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs`
13. `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs`
14. `Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs`
15. `Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs`
16. `Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs`
17. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs`
18. `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs`
19. `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`
20. `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs`
21. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs`
22. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs`

### New test source

23. new `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs`
24. new `Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta`

### Unity resources

25. `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`
26. `Assets/Scenes/FightMatchDemo.unity`

Existing Prefab/Scene `.meta` files remain forbidden and byte-identical; existing GUIDs remain stable. `SafeAreaFitter.cs`, `FightMatchViewId.cs`, localization/config/generated/font files, Core/domain/application/save/gesture code, asmdefs, packages, settings, CI/build-release files, and all unlisted paths remain forbidden. The existing `fm.entry.binding_unsupported` copy is reused; this correction does not authorize a localization-table change.

An implementation need for any 27th path is `BLOCKED`, not implicit scope expansion.

## 13. Corrected stop gates and conclusion

In addition to the base stop gates, stop on any of these:

1. BoardFrame contains Image or any second Graphic.
2. a RectTransform Content is assigned to a LocalizedTmpText field, or `hud` is not a real label.
3. NormalHud lacks the exact ScrollRect/Viewport/Mask/Content chain or Content controls BottomHud height.
4. enemy OriginalSlot is rewritten/compacted, Core enemy validation is tightened, or legal non-first-release input lacks the explicit UI-unsupported state.
5. any of the eight Battle command entries is removed, shares an unintended container, falls below its hit size, or becomes unreachable; disabled is incorrectly treated as no layout occupancy.
6. ART's four-action 488k calculation is used as the final MainRow proof.
7. Reference Close appears outside its 48k Header or Reference Actions has nonzero height.
8. any child view writes a shared outer root/mask, both inner panels are active, or no-panel state leaves a blocker active.
9. OriginalResult is runtime-reparented, duplicated, or unavailable in a route that previously supported it.
10. candidate/original-operation choices grow fixed Actions rather than the scroll Body ChoiceRows.
11. Startup, Navigation, or Result content enters the shared TopBar rect, or Battle receives a second top deduction.
12. the long-screen sample uses y=80 instead of Unity-bottom y=120.
13. Q2 is called 28, a separate 28 run is added, exact manifests are frozen before compile/resource creation, WorldSpace is cited as Overlay evidence, or final reopen occurs before the controlled full run.
14. current FIX13 2/4 is represented as accepted or as a LAYOUT preimage.

Conclusion: `READY_FOR_LIMITED_R`.

The combined central adjudication + frozen base design + this correction is ready only for the same R-TOOLS limited design re-review. It is not ready for implementation, Unity execution, professional implemented-art acceptance, Android/device validation, release, or Git delivery.
