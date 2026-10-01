# UGUI-01 Q4 correction 15 — active native-bind reconciliation

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标、责任与固定输入

本包只修 FIX14 exact2 中 B09 在进入原销毁场景前暴露的 active hierarchy 初始 native bind漏绑；
保留 FIX14 已通过的 P27及四文件 managed/native teardown候选。不得修改测试、controller、布局、
本地化文案、业务、保存、APK、设备或 Git。

- delivery owner / 唯一 Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` 已明确同意最小两文件 FIX15、当前
  398 chars/glyphs / 4 atlas 字体新 prebaseline，以及 exact2 → exact222 → native reopen。
- FIX14 candidate：
  `f95c76420a72877ceea9465662e5532e6eaeab2438b78d3fadfdb6014098ebf9`；
  sourceCandidate：
  `6da5acfe02d4bbdaab20df5ca916e49bfa77f67a16729d996c9290cd1d41edcb`。
- FIX14 owner turn/final：`01a0f55f-32d8-7123-a65f-8fc66800cf5a` /
  `msg_0d179092dafe07fa016abdcc97335c87d09be3d8b7cd054898`；状态
  `BLOCKED_EXACT2_WITH_RESOURCE_DRIFT`。
- FIX14 final receipt：32,868 B / SHA-256
  `c0d31a32db6ee18d60fc9d6c810e3d609dba92c8482e5eadfa0cb259ba609e66`；failure inventory
  1,759 B / SHA-256
  `41395767765eda53f2d4200f2e70c02b9fc29848212353953fa3852f7892df89`。
- 固定旧 evidence root：
  `TestArtifacts/FightMatch/UGUI-01/f95c76420a72877ceea9465662e5532e6eaeab2438b78d3fadfdb6014098ebf9/q4-correction-14/`。
- ARCH read-only diagnosis：thread/turn
  `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `01a0f569-0c2c-7652-8e3c-66dc663c41c3`，
  final `msg_0e75810e652b6a50016abdce8adec887d0b9831441fe3dbcf9`，verdict `NEEDS_FIX`。

FIX14 exact2实际 P27 PASS、B09 FAIL；222与 reopen未运行。P27已完整到达六种 mode且无 hierarchy
error。B09在第一次 `r.Step(false)` 的 `CellCenter` 即失败，尚未进入 `new-page` 或 `host-dispose`。
FIX14 raw失败及资源漂移结论永久保留，不倒写为成功。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或最早 blocker，不能自判接受、进入布局或创建 APK。

## 1. 根因与最小产品边界

`PlayerBattleView.Bind` 的既有 teardown先令 `CandidateBoardInputView.Unbind` 调用
`CandidateBoardElement.Unbind`，把 board controller清空。随后 input `Bind`只恢复托管订阅，且仅在
缓存的 `structuralRefresh == true` 时调用 `BindNative`。同步 EditMode路径中，组件当前已 active，但
有效 `OnEnable` 转换并不一定发生在这次 Bind之后，故 `board.Bind(controller)` 被永久跳过；
`Ready()`虽证明 rect有效，却无法给无 owner 的 board提供 face，首个 `CellCenter`遂报
`Board geometry is unavailable`。

这是产品 OnEnable/native-attach timing缺陷，不是测试、pointer、布局帧或 callback暂停语义缺陷。
测试无需修改；`CandidateBoardElement.CellCenter` 的严格失败契约保持。

## 2. 精确文件范围与 preimage

FIX15 delta只能修改以下两个文件：

| path | FIX14 current bytes | FIX14 current SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs` | 13,828 | `3e79a24e00614bcdd7691cbc05ed671b649d31c693f93208514c2a218f316083` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | 34,488 | `3b2355f306cbc3b5062fce8833415a07470b09e00b69f7702a147e7ede6dc9a5` |

FIX14 另外两个产品文件必须 byte-identical：

- `PlayerBattleView.cs`：25,884 B / `3baa9fc9f2f4a565a4933130e7f5c46e6c16edf4e7a3a0d13c8dac9761201e7c`
- `PlayerDefaultReferenceView.cs`：18,882 B / `b3f28fd28df9687e8a92b95a81b2bd8d65fe974e43b962436b2a75370bfb8cac`

禁止修改/创建 test、fixture、`CandidateBoardElement`、controllers、`LocalizedTmpText`、
`BattleLocalizedRows`、Host、scene/prefab/meta、Config/Generated、Packages/ProjectSettings、旧 evidence、
团队文档或其它产品文件。FIX14 fixed closed set与三个测试 fullname/name matrices全部继续冻结。

## 3. 精确实现合同

### 3.1 `CandidateBoardInputView`

在 `Bind(CandidateBoardInputController, LocalizationService)` 完成既有 `Unbind`、参数校验与新 owner/
service赋值后、调用 `Subscribe` 前，把 `structuralRefresh` 与当前组件真实
`isActiveAndEnabled` 同步。

- 当前 active：继续既有 `Subscribe → BindNative → Render`，确保本次 Bind内完成
  `board.Bind(controller)`；不得等待未来 OnEnable。
- 当前 inactive：继续只建立合法托管 owner/subscription；不访问 native层级，后续既有 OnEnable
  执行 `Subscribe → BindNative → Render`。

不得删除/放宽 `Render` guards，不改 `OnDisable/OnDestroy` teardown、pointer取消、100ms刷新、
member/retry/resolve listener epoch或 owner语义。

### 3.2 `CandidateBattlePlaybackView`

在 `Attach` 与 `Bind` 两个入口，各自在完成既有 `Unbind`、参数/serialized binding检查和新
controller/localization赋值后、调用 `BindCallbacks` 前，把 playback自己的 `structuralRefresh` 与当前
`isActiveAndEnabled` 同步。

- 当前 active：既有 `BindCallbacks` 必须立即到达 `BindNative`，随后现有 Render生效；
- 当前 inactive：保持 OnEnable-delayed native attach，不得强行 Render；
- nested input仍由其自身 §3.1判断，不能由 parent缓存状态替代。

不得改 playback `closed/callbacksActive/nativeBound` guards、schedule、low-memory、Close/Unbind顺序，
不得回退 FIX14 managed detach seam。P27 inactive/deactivating屏障必须原样保留。

### 3.3 禁止替代方案

不得修改 fixture使其手动 OnEnable/Bind、在 `Ready()` 注入 owner、给 `CellCenter` fallback geometry、
捕获异常、移除 active guard、改 controller通知或重建 UI。不得新增 public API、serialized字段、依赖、
资源格式或持久化。

## 4. 字体 prebaseline 与后续语义门

FIX14新增 `显` U+663E、`示` U+793A，来源是已冻结授权键
`fm.battle.playback.interrupted` 的简中文案；P27 schedule-error分支首次真实到达该文案。additive audit
证明只新增两条 character/glyph、既有 entry无变更/删除、atlas仍4、其它36 resources不变：

- audit：`q4-correction-14/font-chain-additive-audit.json`，SHA-256
  `531a5e016a72febf586290cd457e4a174e7dc9a6415dbab91fe9f9eaba1511c2`
- 当前 font asset：8,573,334 B / SHA-256
  `290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0`
- 结构：398 characters / 398 glyphs / 4 atlas。

中央已把该当前 asset明确提升为 FIX15 one-time resource prebaseline；这不改变 FIX14 的失败结论，也不
授权手改/回滚/重建字体。FIX15 source diff仍须只有§2两文件。

exact2与222各自运行前后都保存37-resource manifest、font bytes/structure/additive audit：

- 其它36 resources、source/meta/prefab/scene/settings必须 byte-identical；
- font可 byte-identical或仅 additive characters/glyphs；既有 characters/glyphs/pixels不得改变/删除/
  重映射，material/source/config/external GUID/reference与其它非 dynamic字段不变，atlas保持4；
- 每个新增字符必须能机械映射到本轮实际解析的、已冻结授权 localization key/value；记录 key、locale、
  Unicode与触达测试。未授权字符、无法追溯字符、丢字、既有 entry/pixel变化、atlas/material/shader/
  GUID漂移均 `BLOCKED_UNEXPECTED_RESOURCE_DRIFT`；
- 合法 additive growth直接成为下一阶段 provisional baseline；不得回滚或为补 cache重跑测试。

final native reopen相对222结束时 baseline必须全部37 resources byte-identical。

## 5. 有序验证与 stop gate

### 5.1 preflight/static

1. 确认专属 Unity/MCP/Hub/CrashHandler为零，项目未被其它 Unity打开。
2. 核 §2 source preimage、§4 font prebaseline、FIX14 candidate/source/receipt及旧 evidence只读。
3. 实现后固定新的 sourceCandidate/candidate；创建新的 create-once root
   `TestArtifacts/FightMatch/UGUI-01/<fix15-candidate>/q4-correction-15/`。
4. static diff必须仅两文件的 Bind/Attach active-state同步；另两 FIX14文件、tests/controllers与closed set
   byte-identical；无 catch、test hook、反射、日志期待或断言弱化。

### 5.2 Unity launch 1 — unchanged exact2

只运行以下两个原始 fullname，逐项 literal escape并首尾锚定：

```text
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources
FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback
```

使用 Intel Unity `2022.3.18f1`、Metal、有图形。必须 2/2、actual fullname set exact、零
failure/skip/inconclusive/error。P27六种 mode、logical-update-before-token、A2隔离、零 hierarchy error
继续通过；B09必须完成 `new-page` 与 `host-dispose`，且 stale click/head/files/clocks/request断言不变。
任一失败立即停止，不跑222/reopen、不二次修补、不送R。

### 5.3 Unity launch 2 — exact222

仅在 exact2与其字体门通过、source hashes冻结后运行 FIX14继承的 fixed expected-fullnames-222。
必须222/222、name set exact、零 failure/skip/inconclusive/error；证明 B16/WIRE01、P27/B09、fixed28及
六个参数化 case均为 actual222已通过 subset。无需另跑 exact4或 focused28。失败立即停止。

### 5.4 final native reopen

仅在222/222及字体门通过后运行既有一次 native Metal reopen/resource audit；不生成APK。保存两文件
patch/pre-post identity、53-source closed set、37 resources、font/prefab/scene/license manifests、renderer、
进程树、graceful exit、old evidence equality及 exact argv/timing/exit/XML/log。按§4要求 reopen前后37
resources全部 byte-identical。LAYOUT rect/scale/画面验收不在本包。

新 Unity launches最多3；first failure后停止，不重跑。evidence/temp各 `<=128 MiB`，log/XML各
`<=8 MiB`，free space `>=2 GiB`。禁止 signal/global shutdown、SaveAssets/PrepareResources、临时 Editor
script、APK/device/cloud/Windows/LOC/B/L/Git。

## 6. 交付合同

回工程主程：owner thread/host/turn、用户 authority、FIX15 candidate/sourceCandidate；实际两文件
minimal patch/pre-post identities；另两 FIX14文件与closed set稳定证明；exact2/222/reopen receipts；
每阶段 source/resource/font/process/protection manifests及新增字符到授权文案映射；P27/B09 acceptance、
B16/WIRE01继承覆盖；唯一状态 `READY_FOR_INDEPENDENT_R` 或最早 blocker；未运行项明确列出。

禁止 commit/push/branch/worktree、删除旧 evidence、启动布局/APK/device/cloud或自动送 R。C 完成后先
回主程；主程机械接收，再由 QA 调度同一独立 R。
