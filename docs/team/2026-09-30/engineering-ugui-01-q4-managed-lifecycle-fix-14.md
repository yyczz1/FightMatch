# UGUI-01 Q4 correction 14 — inactive-render barrier and managed teardown

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标、责任与固定输入

本包只修 FIX13 exact4 余下的 P27、B09 两个产品生命周期缺口：父层停用期间仍发生 hierarchy
结构写，以及 Unity fake-null 子视图仍保留托管订阅。不得修改测试、controller 业务顺序、
`LocalizedTmpText`、`BattleLocalizedRows`、LAYOUT、LOC、APK、设备或 Git。

- delivery owner / 唯一 Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求普通修正继续。
- FIX13 candidate：
  `15cfc5ce2da83661fc2c3aab01d4c8d7052af3e9421541a21f96ae7d838ed16f`；
  sourceCandidate：
  `fbc273bb02b6bd4071551ff2f291011a05cc982b45c148afe34ee42a345127a6`。
- FIX13 owner turn：`01a0f545-3e7b-73f3-8e1e-e7078d3473ae`；final receipt
  38,115 B / SHA-256
  `e12ec88ea2e718f4efc0dc66898e2663f72f1cd007f599086e44abb57f4ec646`；failure inventory
  10,510 B / SHA-256
  `fdfb57a5cea0ab7784733cc19fa8d0e6ac321346e16e311e1129a1a2994c82e0`。
- 固定失败 evidence root：
  `TestArtifacts/FightMatch/UGUI-01/15cfc5ce2da83661fc2c3aab01d4c8d7052af3e9421541a21f96ae7d838ed16f/q4-correction-13/`。
- ARCH 只读诊断：thread/turn
  `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `01a0f553-8562-7241-ba07-612deafd1bcb`，
  final `msg_0e75810e652b6a50016abdc914e91487d090c55ba67748d918`，`DIAGNOSIS_COMPLETE`。

FIX13 exact4 实际 2 passed / 2 failed：B16、WIRE01 已通过且证据继续有效；P27、B09失败；
222 与 native reopen 未运行。FIX13 receipt 的 `b09Evidence` prose误写 Passed，后续必须以
`actual4`、coverage flag、XML 与 failure inventory 的 Failed 为准。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或最早 blocker，不能自判接受、进入布局或创建 APK。

## 1. 冻结裁决

### 1.1 P27 是产品生命周期 bug

实际链：`SetActive(false)` → `CandidateBattlePlaybackView.OnDisable` → `Close(FocusLost)` →
`CandidateBoardInputController.CancelGesture` → `Changed` →
`CandidateBattlePlaybackController.OnInputChanged` → playback `Changed` →
`CandidateBattlePlaybackView.Render` → `BattleLocalizedRows.Bind` → `SetSiblingIndex`。

`closed=true` 已在 controller取消前发生，但 `Render()` 到末尾才检查 `closed`；因此父 `Playback` 的
Unity deactivate事务中仍写 sibling/hierarchy。controller 的合法 `Changed`、
logical-update-before-token-report 和外部观察者通知不得被压掉。FIX13 已正确修好测试 PlayMode边界，
本包不得再改测试。

### 1.2 B09 是托管订阅清理 bug

replacement page正常接管后，其 root被销毁；子 view成为 Unity fake-null。随后
`PlayerBattleController.Dispose()` 的合法终止通知进入 playback `Finish` → input `CancelGesture` →
input `Changed`。FIX13 外层 `if (child != null)` 使用 Unity fake-null语义，既保护 native访问，也错误地
跳过了该 child仍必需的托管 `Unbind`，故已销毁 `CandidateBoardInputView` 继续 `Render` 并访问 destroyed
`LocalizedTmpText`。

必须分开两个概念：Unity 对象无效时禁止 native UI/Transform/GameObject访问；CLR 实例仍存在时必须
幂等解除 controller/localization/static event订阅并清空托管引用。不得修改 controller Dispose/Finish
顺序，也不得给文本/row层加 null guard或吞异常来遮住 stale callback。

## 2. 唯一允许修改的 4 个文件

| path | FIX13 preimage bytes | FIX13 preimage SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs` | 24,951 | `430d1448841fc811f9bc7ee2706c0f1fb5d28363a7342b2d87e67b74c694adcf` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs` | 12,272 | `e2a6c1be10ea39d0d7ddb5b9d1e6ffc00f39a2bf3a86f6b1249acc36de436fc1` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | 32,658 | `2fd5106b7cab62afccea58fad26f715b6aff3450c5bb2b36ce5b505c4bfb3ef7` |
| `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` | 18,188 | `f88846f6f64bc0b67ff2322f420940b80fc10d7ab6e614537e4ec9a050d31957` |

实际 product patch必须恰为这四个文件；不得修改/创建 test、fixture、controller、Host、scene、prefab、
font、`.meta`、Config、Generated、Packages、ProjectSettings、Library、旧 evidence、团队文档或 Git。

以下相邻文件必须保持 exact preimage，作为禁止绕路的 closed set：

- `PlayerBattleController.cs`：8,552 B / `24287152a1c14cac8200825c0b43f66cb86984891249e633d9be6c752806f9bd`
- `CandidateBattlePlaybackController.cs`：8,146 B / `89866f30425232b8e964280bffd1250b58758c936d97459403dc5a02f09d9b62`
- `CandidateBoardInputController.cs`：21,703 B / `1cbe81a82c519e14a5f3b609b782dc872cd3a39fa833aa9b208548b5de6b27fc`
- `Localization/LocalizedTmpText.cs`：3,061 B / `c0579d136752f155b06d8adf99c9a8e38fd7af0cb651c7f81813aac737382626`

FIX13 的三个 test文件及全部 fixtures也须 byte-identical；fullname、matrix、断言均不得改变。

## 3. 精确实现合同

### 3.1 停用期结构写屏障

`CandidateBattlePlaybackView` 与 `CandidateBoardInputView` 各维护一个纯托管、幂等的
“允许结构刷新”状态：

- `OnDisable` 的第一步必须关闭该状态，早于 CancelPointer、CancelGesture、Close、Dispose或任何
  可能发 `Changed` 的调用；`OnDestroy`同样先关闭；
- 各自 `Render` 的第一条可观察行为必须检查该状态；playback还须在访问 board/text/row 前检查
  `closed`。不允许结构刷新时直接返回，不能读写任何 Unity native对象；
- `OnEnable` 才重新打开状态。若 controller/localization仍有效且 view未 closed/unbound，用现有
  `Render` 从 controller最新状态完整刷新；input view同时恢复既有 ticking/refresh时钟；
- `Bind/Attach` 到 inactive层级时不得强行结构写，后续 `OnEnable`负责第一次有效刷新；active层级
  的现有首次 Render 保持；
- 该屏障只抑制不可见/销毁 view 的 UI 写，不压 controller事件、不改取消原因、不改 playback token、
  request/head/files/clocks或业务顺序。

不得修改 `BattleLocalizedRows.Bind`、期待 Unity error、catch hierarchy异常或删除 P27 六种 mode断言。

### 3.2 纯托管 detach seam 与 native cleanup 分离

四个 view各自形成内部、幂等的 managed-detach seam；可命名不同，但合同必须明确可由父级在 child
Unity fake-null而 CLR reference仍存在时调用。managed seam只能操作托管字段/订阅，不得访问
GameObject、Transform、Component属性、Button、Board、TMP或 `GetComponents*`：

- `PlayerBattleView`：先解除自身 localization/controller订阅、失效 epoch/page callback，并按既有
  语义 detach page；再要求 reference/playback child做 managed detach；
- `CandidateBattlePlaybackView`：先设 closed/停止 schedule/失效 binding epoch，解除 low-memory、
  localization、playback controller订阅，再要求 input child做 managed detach；
- `CandidateBoardInputView`：先停止 ticking/失效 binding epoch，解除 localization与 input controller
  订阅，清空纯托管 listener delegates/owner references；
- `PlayerDefaultReferenceView`：失效 generation/play epoch、停止 ticking，解除 localization及 owner
  input/playback订阅，清空纯托管 owner/page/context/source引用。

父级区分两种 guard：

1. 仅为调用 managed seam，可用明确的 CLR-reference-exists判断（例如 `ReferenceEquals(child, null)`
   的反面或等价 object cast）；这里不得读 child 的 Unity native属性。
2. 只有 Unity `child != null` / `board != null` 为真时，才可运行 child完整 native cleanup、按钮/row/text
   清理、overlay清除、层级变化或其它 Unity API。

CLR存在判断绝不能被复用为“Unity对象有效”；Unity有效判断也绝不能再次跳过 managed detach。
正常有效对象路径可先调用 managed seam，再调用幂等 full `Unbind`完成 native cleanup；不得重复业务
取消、重复 page detach或改变事件顺序。fake-null路径只做 managed detach，绝不碰 native状态。

### 3.3 顺序与既有语义

- 所有 `Unbind/Close/Dispose/OnDisable/OnDestroy` 都先阻断新 Render，再解除托管订阅，最后才做
  仍有效 Unity对象的本地清理；即便中间 controller取消/终止发事件，旧 UI也已不可回入。
- `PlayerBattleController.Dispose()`、`CandidateBattlePlaybackController.Dispose/Finish()`、
  `CandidateBoardInputController.CancelGesture/ClearVisibleFeedback` 的既有顺序和通知继续合法，不修改。
- `PlayerBattleView` 的 `disposed` 终止语义、Host `BattleViewDisposed` fail-closed、临时 Unbind可重绑、
  reference首拖、B16缓存坐标、WIRE01 retry和所有 request identity保持。
- 不新增 public API/serialized字段/依赖/资源/持久化；internal helper只能服务上述四文件 teardown。

## 4. 有序验证与 stop gate

### 4.1 preflight/static

1. 确认专属 Unity/MCP/Hub/CrashHandler进程为零；项目未被其它 Unity打开。
2. 四个修改文件与 §2 preimage全匹配；FIX13 candidate/source/resource/font chain可重算；旧 evidence只读。
3. 创建新的 create-once evidence root：
   `TestArtifacts/FightMatch/UGUI-01/<fix14-candidate>/q4-correction-14/`；不得覆盖旧 root。
4. static diff必须恰含四文件且只含 §3；禁止相邻 closed set/test变化；无 catch/异常期待/断言弱化；
   证明每层 managed unsubscribe先于任何可发通知的 cleanup，native操作只在 Unity-valid分支。

### 4.2 Unity launch 1 — exact remaining 2

只运行以下两个原始 fullname，逐项 regex literal escape后组成首尾锚定单一 alternation：

```text
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources
FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback
```

使用 Intel Unity `2022.3.18f1`、Metal、有图形。必须 2/2 passed、actual fullname set exact、零
skip/inconclusive/error，日志无 uncaught hierarchy/fake-null/disposed异常。必须从原断言/output证明：

- P27 六种 mode全部到达；父停用期间零 hierarchy写错误；logical-update-before-token、A2隔离、
  resource/订阅/零业务写断言不变；
- B09 `new-page` 与 `host-dispose` 两种 mode均完成；replacement root销毁后旧 view零 Render/stale
  callback，旧 click/head/files/clocks/request断言不变。

任一失败立即停止：不跑 222/reopen、不做第二次修补、不送 R；保留新 evidence并回最早 blocker。

### 4.3 Unity launch 2 — exact 222

仅在 exact2通过且 source hashes冻结后运行 FIX13继承的 fixed expected-fullnames-222；逐 fullname
literal escape。必须 222/222、name set exact、零 failure/skip/inconclusive/error。receipt须证明：

- FIX13 已绿 B16、WIRE01在 actual222中重新通过；
- fixed28是 actual222的 exact已通过 subset；六个参数化 case单列；
- P27/B09与本轮 focused结果一致。

无需另跑 exact4或 focused28；222 已完整覆盖四项和 fixed28。任一失败立即停止，不二次修补、不送 R。

### 4.4 final native reopen / resource audit

只在 222/222后运行既有 native resource reopen gate；不生成 APK。保存四文件 patch/pre-post identity、
53-source closed set、37 resources、Prefab/Scene/TMP/font/license manifests、Metal renderer、进程树、
graceful exit、旧 evidence equality、exact argv/timing/exit/XML/log identities。字体以 FIX13 证明的
396 chars/glyphs、4 atlas为 before；本包零文本/字体变化，必须完全相等而非 additive growth。

LAYOUT rect/scale/画面验收不在本包，不得声称已完成。

## 5. 交付合同

回工程主程：owner thread/host/turn、用户 authority、FIX14 candidate/sourceCandidate；实际四文件
minimal patch/pre-post identities；managed/native cleanup顺序静态证明；exact2/222/reopen完整 receipts；
source/resource/font/process/protection manifests；P27/B09 acceptance及B16/WIRE01继承覆盖；唯一状态
`READY_FOR_INDEPENDENT_R` 或最早 blocker；未运行项明确列出。

禁止 commit/push/branch/worktree、删除旧 evidence、启动 APK/device/cloud、修改 layout、自动送 R。
C 完成后先回主程；主程机械接收，再由 QA 调度同一独立 R。
