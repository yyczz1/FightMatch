# UGUI-01 Q4 correction 13 — complete PlayMode matrices and terminal-view teardown

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标、责任与固定输入

本包只修 FIX12 exact4 的四个失败：两个测试边界、一个终止态战斗视图重绑、一个 Unity
fake-null 清理链。不得改变战斗、保存、手势、反馈或播放上报语义，也不处理 LAYOUT、LOC、APK、
设备、Git 或新功能。

- delivery owner / 唯一 Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求普通修正继续。
- FIX12 candidate：
  `3d6826d2782ce87399694194d778d926cd769de111752ae8308e345cb25a1a6e`；
  sourceCandidate：
  `da039561b0ffdc523965246562039a65d9cdccc9e88e4bc06cdc6eab8ad5e3e1`。
- FIX12 owner turn：`01a0f532-3984-79e0-8cb3-e933441c7fdb`；final receipt
  26,298 B / SHA-256
  `7727552ac143b67d939b56f4732b257fb1cf6aa73c8dd7934bc5b3149e50731f`。
- 固定失败 evidence root：
  `TestArtifacts/FightMatch/UGUI-01/3d6826d2782ce87399694194d778d926cd769de111752ae8308e345cb25a1a6e/q4-correction-12/`。
- ARCH 只读诊断：thread/turn
  `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `01a0f53a-9355-7602-a835-c476eda5a748`，
  `DIAGNOSIS_COMPLETE`。
- R-TOOLS 预审：thread/turn
  `01a0f3e3-9663-7be2-84a8-ed23be32f0b9` / `01a0f539-ff82-72b1-bb20-e46d2a21482f`；
  只读 findings，不是最终 acceptance。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或最早 blocker，不能自判接受、进入布局或创建 APK。

## 1. 四个失败的冻结裁决

FIX12 exact4 实际 0 passed / 4 failed / 0 skipped；222 与 native reopen 未运行。

1. **P27**：顶层 test 在 shared helper 后立即 `ExitPlayMode`，后续 skip/close/detach/... matrix
   在 EditMode 执行；输出证明 skip、close 通过，到依赖真实 `OnDisable` 的 detach 才失败。只修测试
   PlayMode 包围范围，不改产品播放上报顺序。
2. **B16**：同样过早退出 PlayMode；且 close 已正确 unbind 后，测试才调用 `Point(route.Last())`
   重算 release 坐标，触发 `Board geometry is unavailable`。必须在 cause 前缓存屏幕坐标，close 后只
   投递该物理坐标；不得放宽 `CellCenter` 的严格失败契约。
3. **WIRE01**：`battle-dispose` 是终止态，而父 `FightMatchHostView` 仍合法订阅 session changed；
   pending retry 后父 Host 把终止 `Dispose` 误作临时 `Unbind`，重绑同一 `PlayerBattleView` 并撞
   `ObjectDisposedException`。必须保留 `Bind` 的 disposed guard，并由 Host 识别终止态、禁止重绑、
   进入基础设施诊断 fail-closed；不能改 Retry、业务 controller 或先 Unbind 父级来绕开子级清臂断言。
4. **B09**：旧 page/root 已销毁后迟到清理到达
   `CandidateBattlePlaybackView.Unbind()`；`inputView?.Board?.ClearPlaybackOverride()` 只查 CLR null，
   绕过 Unity fake-null 并访问已销毁 `CandidateBoardElement`。清理仍须先退订托管 callbacks，再对
   仍有效 Unity object 做本地清理；不得吞异常或给 `CandidateBoardElement.ClearPlaybackOverride()`
   加顶层补丁。

导航页 `PlayerNavigationView.Dispose()` 当前仍只是可重绑 `Unbind()`；本包不得把战斗视图终止态
规则泛化到导航页。其相邻 fake-null 审计另立复现后续，不进入本包。

## 2. 唯一允许修改的 8 个文件

| path | FIX12 preimage bytes | FIX12 preimage SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | 13,812 | `95b12ee6ba1973853a3b1d17de08943c1f61a5c77cb7de371b75677a954ccc87` |
| `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs` | 24,769 | `173c8c91c384878d7e5ddbb1b39dfe94647e58e8e8d5737f8089f56d2088705d` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs` | 11,462 | `2e150eae838b5b16959f49745497f0cdd34b1caf7185bd16204aabde2053152d` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | 32,604 | `d153f5c8b3474e1f614ed70aa7fcb1566708c637927bbe0b599be92d2130bcdc` |
| `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` | 18,170 | `bdfb2c331b6f820b6de400f2ea82539364bb5f8c46f0b75fb84a303680ef43a1` |
| `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs` | 16,074 | `7cf3afbd94132b2d47b086611011a0c97da43f2a043ef6f6773df2d5efe295d7` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,749 | `dec18b9458cfc6cf7cb7bf9d2dfb63a0937c5d7b32887cc8791e8308d511d3ba` |
| `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs` | 27,609 | `3f4e56ead7feb7e3c85b9f28cf315240010cbbbb20b71fa5baf7ca270a56fcca` |

允许少于八个文件实际变化；未为明确验收断言所需时，不要修改
`PlayerBattlePresentationTests.cs`。禁止修改/创建其它 source、test、fixture、scene、prefab、font、
`.meta`、Config、Generated、Packages、ProjectSettings、Library、旧 evidence、团队文档或 Git。

## 3. 精确实现

### 3.1 P27/B16 的完整矩阵留在顶层 PlayMode

在两个顶层 `[UnityTest]` 中保留 FIX12 已建立的顺序：NewScene → EnterPlayMode → shared body。
删除 shared body 后的早期 `ExitPlayMode`，把唯一 `ExitPlayMode` 移到该顶层 test 的全部本地 matrix
完成后、method 返回前。不得复制 shared matrices、直接调用 `OnDisable`、删除 mode/cause、改
fullname/参数/顺序或削弱业务不变断言。

在 B16 每个 `BoardPanelRig` case 内：

- 在执行 cause（尤其 `close`）前，用当前有效 geometry 计算并缓存 `route.Last()` 的 release
  screen point；
- cause 后的两次真实 `Up` 均使用同一缓存坐标，绝不再次调用 `Point` / `CellCenter`；
- close 后继续证明 `raycastTarget == false`，两次 late-up 都不创建 request，不改变 head 或 storage；
- `detach` 的再启用/readiness、其它 cause 的现有行为与输出原样保留。

### 3.2 战斗视图终止态由 Host fail-closed

`PlayerBattleView` 增加只读、无副作用的内部终止态观察点（例如 `internal bool IsDisposed => disposed`）；
`Dispose()` 与 `Bind()` 的现有终止语义不变。

`FightMatchHostView.Render()` 在任何 `Controller` 访问或 `Bind` 前用 Unity 有效性检查战斗视图：

- 当前 content page 是 Battle 且 `battleView` 已 Unity-destroyed 或 `IsDisposed` 时，设置固定
  infrastructure diagnostic code `BattleViewDisposed`，不得调用 `Bind`；
- 本次 render 必须令 battle page fail-closed（不可把已终止 view 重新显示为可交互页），同时保留
  session 的合法 retry/commit；blocking diagnostic 由既有 `RefreshDiagnostic` 显示；
- 未终止的临时 `Unbind` 仍按原路径重绑；Navigation 行为不变；不得移除
  `PlayerBattleView.Bind()` 的 `ObjectDisposedException` guard。

WIRE01 原 6×2 teardown matrix、真实 pointer、pending retry、旧 accepted arm 清除、head/files 不变
全部保留；仅在 `battle-dispose` case 增加精确观察：retry 仍只提交原 pending operation，一次合法
业务变化不会重绑终止 view，旧 feedback 不恢复，Host 显示 `BattleViewDisposed` 基础设施诊断。
不得通过父 Host `Unbind/Dispose` 代替被测子视图的 cleanup。

### 3.3 Unity fake-null 清理边界

在以下现有 teardown 路径中，把对 UnityEngine.Object 的 `?.` 改为显式 Unity 有效性检查
（`obj != null`）；保持退订、状态清理和控制器取消的原顺序：

- `CandidateBattlePlaybackView.Unbind/Close/OnDisable/OnApplicationPause/OnApplicationFocus` 对
  `inputView` 及其 `Board`；先解除 localization/controller/button/low-memory 等托管订阅，再清有效
  board/input view；
- `CandidateBoardInputView.Unbind/OnDisable` 对 `board`；
- `PlayerBattleView.Unbind` 对 `referenceView`、`playbackView`、`headline`、`hud`；
- `PlayerDefaultReferenceView.ClearOverlay` 对 `board`；保留 FIX12 的 `this != null` self guard；
- `FightMatchHostView.Render/Unbind` 内本包实际可到达的 `battleView` 与被销毁 serialized child/text
  清理访问。

不得用 `ReferenceEquals(obj, null)`，不得 catch/ignore `MissingReferenceException`，不得跳过整个
Unbind，不得修改 `CandidateBoardElement.CellCenter`、`ClearPlaybackOverride`、
`PlayerBattleController.Dispose()`、Application/save 或业务模型。禁止顺手扩展到没有复现的
`PlayerNavigationView`。

## 4. 有序验证与 stop gate

### 4.1 preflight/static

1. 确认专属 Unity/MCP/Hub/CrashHandler 进程为零；项目未被其它 Unity 打开。
2. 八个允许文件 preimage bytes/SHA 全匹配；FIX12 candidate/source/resource/font chain 可重算。
3. 创建新的 create-once evidence root：
   `TestArtifacts/FightMatch/UGUI-01/<fix13-candidate>/q4-correction-13/`；不得覆盖旧 root。
4. static diff 仅含 §3；test fullname、参数、matrix/case count 不缩水；所有 Unity-object teardown
   guard 使用 Unity `!= null` 语义；终止态 diagnostic code 恰为 `BattleViewDisposed`。

### 4.2 Unity launch 1 — exact failing 4

只运行以下四个原始 fullname，逐项 regex literal escape 后组成首尾锚定单一 alternation：

```text
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources
FightMatch.Core.Tests.CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit
FightMatch.Core.Tests.CandidateBoardInputTests.UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite
FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback
```

使用 Intel Unity `2022.3.18f1`、Metal、有图形。必须 4/4 passed、actual fullname set exact、零
skip/inconclusive/error；日志中无 uncaught lifecycle/null/disposed exception。还必须从 test output/
assertions证明 P27/B16 matrices 完整、WIRE01 `battle-dispose + pending` 的 retry/diagnostic、B09 stale
callback/旧页解绑均真实执行。

任一失败立即停止：不跑 222/reopen、不做第二次修补、不送 R；保留新 evidence并返回最早 blocker。

### 4.3 Unity launch 2 — exact 222（包含 fixed 28）

仅在 exact4 通过且 source hashes 冻结后运行既有 fixed expected-fullnames-222；逐 fullname literal
escape。必须 222/222、name set exact、零 failure/skip/inconclusive/error；receipt 显式证明 fixed28
是 actual222 的已通过 exact subset并单列六个参数化 case。无需重复 focused28。

任一失败立即停止，不二次修补、不送 R。

### 4.4 final native reopen / resource audit

只在 222/222 后运行既有 native resource reopen gate；不生成 APK。保存允许文件 patch/pre-post
identity、53-source closed set、37 resources、Prefab/Scene/TMP/font/license manifests、Metal renderer、
进程树、graceful exit、旧 evidence equality、exact argv/timing/exit/XML/log identities。FIX12 字体
396 chars/glyphs、4 atlas 为 before；只允许真实解析所需 additive glyph growth，零删除/重映射。

LAYOUT 的 rect/scale/画面验收不在本包，不得声称已完成。

## 5. 交付合同

回工程主程：owner thread/host/turn、用户 authority、FIX13 candidate/sourceCandidate；实际 changed
files 与 minimal patch/pre-post identities；exact4/222/reopen 完整 receipts；source/resource/font/process/
protection manifests；四个 failure 各自 acceptance；唯一状态 `READY_FOR_INDEPENDENT_R` 或最早
blocker；未运行项明确列出。

禁止 commit/push/branch/worktree、删除旧 evidence、启动 APK/device/cloud、修改 layout、自动送 R。
C 完成后先回主程；主程机械接收，再由 QA 调度同一独立 R。
