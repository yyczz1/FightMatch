# UGUI-01 Q4 correction 12 — direct PlayMode boundaries and destroyed self guard

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标、责任与固定输入

本包只修复 FIX11 focused 28 的 4 个失败；不重新设计已通过的 readiness、selector、copy、
localization、font 或资源逻辑，也不处理 LAYOUT、LOC、APK、设备、Git 或任何新功能。

- delivery owner / Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已指示普通修正继续，无需逐包重批。
- FIX11 candidate：
  `5797520db7bccb82f97e961a838ad9206d7a8ee0cc61eb12a006ec6a7e2969d6`；
  sourceCandidate：
  `10453c102023251159e0a390f0b1be700981e45da2da7ea3ffd0bb879ae1ee85`。
- FIX11 owner turn：`01a0f525-0661-79b2-a5e5-1f7786da5d4a`；final receipt
  32,910 B / SHA-256
  `de4f158586339d9f3d3ef6e675ecf26093026ec98ab26d8b9f53dcd7dfbc61e6`。
- 固定失败 evidence root：
  `TestArtifacts/FightMatch/UGUI-01/5797520db7bccb82f97e961a838ad9206d7a8ee0cc61eb12a006ec6a7e2969d6/q4-correction-11/`。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或明确 blocker，不能自判接受、创建 APK 或进入
LAYOUT。最终独立 R 仍由 QA 调度；C 不审自己的交付。

## 1. 失败证据与裁决

FIX11 corrected focused run 实际恰好 28：24 passed / 4 failed / 0 skipped；literal escaping 与
6 个参数化 fullname 均已通过。四个失败不是 selector、discovery 或产品业务语义漂移。

### 1.1 两个 nested `EnterPlayMode`

以下两个顶层 `[UnityTest]` 都 `yield return` 同一个 shared helper，而 helper 内又 yield
`EnterPlayMode`：

- `CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources`
- `CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit`

Unity Test Framework `1.1.33` 明确拒绝这种嵌套 command yield：
`EnterPlayMode can only be yielded directly by the test method`。这是测试生命周期边界错误；两组
已接受 cancellation matrices、产品实现与 assertions 不得改弱。

### 1.2 WIRE01 teardown 在退出 PlayMode 后做真实 pointer 输入

`CandidateBoardInputTests.UGUI_COPY_WIRE01...` 的主矩阵在 PlayMode；但现有代码先
`yield return new ExitPlayMode()`，随后才进入 6×2 teardown matrix。该矩阵创建
`UguiHostRig` 后立即 `Driver.Down`，没有经过 FIX11 已接受的三帧 readiness，因此 line 144
expected `InvalidStart` / actual `null`。这不是允许放宽的 feedback 语义。

整个 WIRE01 的真实 uGUI pointer matrices 必须留在同一个顶层 PlayMode 区间；每个新 rig 在
首次 pointer input 前必须走既有 `Ready()`。不得 hand-drive `Update` 或直接调用回调。

### 1.3 destroyed `PlayerDefaultReferenceView` 自身引用

`PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback` 在旧
page 已销毁后，通过 controller dispose 再次到达 `PlayerDefaultReferenceView.Close()`。
FIX11 已保护销毁的 `explanation` / `beat`，但 Close 尾部仍无条件访问
`gameObject.SetActive(false)`；Unity fake-null component 会在取 `gameObject` 时抛
`MissingReferenceException`。Close 的解除订阅、清状态和有效对象隐藏顺序仍需保留。

## 2. 唯一允许修改的 4 个文件

| path | FIX11 preimage bytes | FIX11 preimage SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` | 18,152 | `0bbd47c8dfee86e910e5ad932f7e57a5d57edd214e35239784448e4cf9509bb4` |
| `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs` | 15,791 | `a148e76e1ebc9c1ce5d488fc09001f4dd695547bb2d1ce578387479442103db4` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 28,434 | `1a5e643a575febee0948d32657ec6e04ff1db1374613d5e5fff7dcad0fa99025` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,442 | `c8a0ed2ac8dbd9b971b53ca32c9f0f795d3c3d3eec6b678be09c538e27918ed9` |

禁止修改/创建其它 source、test、scene、prefab、font、`.meta`、Config、Generated、Packages、
ProjectSettings、Library、旧 evidence、团队文档或 Git。Unity 动态字体既有合法增量只可作为运行
副作用审计，不是授权手改 font。

## 3. 精确实现

### 3.1 让两个顶层测试直接拥有 PlayMode command

把 `BattleCancellationCases.AssertDisableAndExplicitCancellationOrders()` 改为只包含原两组
共享 matrices 的 body；它不得再创建 scene、yield `EnterPlayMode` 或 yield `ExitPlayMode`。
可将其重命名为 `AssertDisableAndExplicitCancellationOrdersBody()`，两个 caller 必须一致更新；
不得复制 matrices。

两个顶层 caller 各自在调用 shared body 前直接执行：

1. `EditorSceneManager.NewScene(...EmptyScene, ...Single)`；
2. `yield return new EnterPlayMode()`；
3. `yield return BattleCancellationCases.AssertDisableAndExplicitCancellationOrdersBody()`；
4. `yield return new ExitPlayMode()`；

随后各自原有 P27/B16 matrices、mode 顺序、readiness、断言及输出原样继续。只有顶层 test
method 可以 yield PlayMode command；shared helper 只能 yield 普通帧/readiness。

### 3.2 WIRE01 的 PlayMode 边界包住全部真实 pointer matrices

在 `UGUI_COPY_WIRE01...`：

- 删除当前位于 6×2 teardown matrix 之前的 `yield return new ExitPlayMode()`；
- 在该测试全部 teardown matrix 结束后、method 返回前才 yield `ExitPlayMode`；
- 每个 `UguiHostRig(true)` 在 `r.Enter()` 后、读取/发送首次 pointer input 前增加
  `yield return r.Ready()`；不得新增另一套 wait helper；
- 6 个 mode × pending false/true、真实 `Driver.Down/Up`、feedback exact assertions、pending
  retry 与零业务写保护全部保留。

不得把 expected `InvalidStart` 改成 null，不得直接调用 controller input 模拟 raycast，不得删除
teardown cases。

### 3.3 destroyed self 使用 Unity 有效性 guard

`PlayerDefaultReferenceView.Close()` 末尾仅把无条件
`gameObject.SetActive(false)` 改为在 `this` 仍是有效 Unity object 时才隐藏，例如：

```csharp
if (this != null) gameObject.SetActive(false);
```

必须保留其前面的 Generation、overlay clear、owner callbacks 解除、dynamic button listener/
binding 清理、`explanation` / `beat` guard 以及 `IsOpen/owner/page/board/context/Source` 清空顺序。
不得吞异常、用 `ReferenceEquals(this, null)`（它绕过 Unity fake-null）、重建视图或改
`PlayerBattleController.Dispose()`。

## 4. 有序验证与 stop gate

### 4.1 preflight/static

1. 确认专属 Unity/MCP/Hub/CrashHandler 进程为零；项目未在其它 Unity 进程打开。
2. 四个 preimage bytes/SHA 全匹配；FIX11 candidate/source/resource manifests 与字体 chain 可重算。
3. 创建新的 create-once evidence root：
   `TestArtifacts/FightMatch/UGUI-01/<fix12-candidate>/q4-correction-12/`；不得覆盖旧 root。
4. static diff 只能是 §3 的 bounded changes。测试 fullname、参数、case count 与固定 28/222
   名单不变；`EnterPlayMode` / `ExitPlayMode` 只由三个相应顶层 tests 直接 yield。

### 4.2 Unity launch 1 — exact failing 4

先只运行以下四个原始 fullname；逐项使用标准 regex literal escaping，再生成首尾锚定的单一
alternation：

```text
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources
FightMatch.Core.Tests.CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit
FightMatch.Core.Tests.CandidateBoardInputTests.UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite
FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback
```

使用 Intel Unity `2022.3.18f1`、Metal、有图形。XML 必须 4/4 passed，actual fullname set exact，
零 skipped/inconclusive/error，且无 uncaught `MissingReferenceException` / `NullReferenceException`。

任一失败立即停止：不跑 28/222、不做第二次修补、不送 R；保留 create-once evidence 并返回
最早 blocker。

### 4.3 Unity launch 2 — exact 222（包含 fixed 28）

只在 4/4 且四文件 source hashes 未再变化后运行固定 expected-fullnames-222；同样逐 fullname
literal escape。QA 已机械核实 FIX11 的 fixed 28 是 fixed 222 的 exact subset：28/222 均 unique，
28-only=0，222-only=194。中间另开一次 focused 28 不覆盖独立 acceptance，只会重复执行同样 cases，
因此本包不再启动该冗余进程。

XML 必须 222/222、name set exact、零 failure/skip/inconclusive/error；receipt还必须显式比较 actual
222 对 fixed 28 子集，证明 28 个 fullname 全部实际出现且通过，并单列其中 6 个参数化 case。
保留 FIX11 24/28 历史，不得把它重写成通过，也不得删除/跳过任何 test。

任一失败立即停止，不二次修补、不送 R。

### 4.4 final native reopen / resource audit

只在 222/222 后运行既有 native resource reopen gate；不生成 APK。保存最终：

- 四文件 patch 以及 source closed-set、37 resources、Prefab/Scene/TMP settings/font/license manifests；
- FIX11 final dynamic font 396 chars/glyphs、4 atlas 为本轮 before；只允许测试真实解析所需的
  合法 glyph additive growth，既有 entries 不变、零删除/重映射；
- Metal renderer、无软件 fallback、进程树、graceful exit、旧 evidence/read-only roots equality；
- exact argv、timing、exit、XML/log bytes/SHA、expected/actual fullname comparison。

Scene Canvas runtime rect/lossyScale/scaleFactor/首帧与 ART-02 LAYOUT 不在本包；不得顺手改
scene scale 或声称布局/画面已关闭。

## 5. 交付合同

返回工程主程：

- owner thread/host/turn、用户 authority、FIX12 candidate identity；
- 四文件 minimal patch 与 pre/post bytes/SHA；
- exact4/222/reopen 的 argv、timing、exit、XML/log bytes/SHA、222 exact set与 fixed28 subset receipts；
- source/resource/font/process/protection manifests；
- 每条 acceptance 的 pass/fail；
- 唯一状态 `READY_FOR_INDEPENDENT_R` 或最早 blocker；
- changed source/test files 恰好四个；未运行项明确列出。

禁止 commit/push/branch/worktree、删除旧 evidence、启动 APK/device/cloud、修改 layout 或自动送
R。C 完成后先回主程；主程机械接收并由 QA 调度独立 R。
