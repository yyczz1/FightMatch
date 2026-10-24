# UGUI-01 Q4 correction 11 — real lifecycle, touch sequence, popup readiness, destroyed bindings

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标与责任

本包只修复 FIX10 首次越过全局 readiness 后暴露的 5 组真实失败，并修正 focused selector
把 6 个参数化 fullname 当 regex 语法而漏跑的问题。它不处理 ART-02 布局、Scene Canvas
serialized override、LOC、APK、设备、Git 或任何新功能。

- delivery owner / Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已指示普通修正继续，无需逐包重批。
- 前置 candidate：
  `d776c310bced809b9a7102f5587eda5958e78b7cc027d090f1b247751042bff4`。
- FIX10 owner turn：`01a0f4fc-0620-7711-bf30-b29f33c7c848`；final receipt
  23,475 B / SHA-256
  `3710d9cf15b7d198eb0b59e5df771cfb4c1000866a51f7450b201498532156f9`。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或明确 blocker，不能自判接受、创建 APK 或进入
LAYOUT。最终独立 R 仍由 QA 调度；C 不审自己的交付。

## 1. 固定失败证据与裁决

FIX10 focused run：22 actual，16 passed / 6 failed，exit 2。XML 29,813 B / SHA-256
`b677861dae00660e7e7cf790397f582d765750e80e3fd01977752f31cfb3ba8a`；failure inventory
10,448 B / SHA-256
`2c771efb38632453bb29f2c86aa05d906a990853349e8787670b6355540d271d`。

### 1.1 selector 不是 test-name drift

固定 28 名单中的 6 个参数化 fullname 因 raw `-testFilter` 中 `(` / `)` 等 regex
metacharacters 未 literal escaping 而没有执行。`unexpected=0`；这不是 discovery、改名或
test-count 漂移。FIX11 禁止复用 raw filter，也不得修改测试名称或参数来迎合 filter。

### 1.2 两个 shared cancellation 失败

`CandidateBattlePlaybackPanelTests.P27_03...` 与
`CandidateBoardInputTests.B16B04_Cancel...` 都在
`CandidateBoardInputTestData.cs:415` 失败：唯一 `FocusLost` 已发布，但 parent-first teardown
后 slot 仍非 null。

两个外层测试虽为 `UnityTest`，却都没有 `EnterPlayMode`；FIX10 只增加三帧 EditMode yield。
因此 `FightMatchPlayerHost.OnDisable` 的真实 PlayMode lifecycle 不会执行，而随后显式
`board.gameObject.SetActive(false)` 仍可走 board cancellation 路径。测试矩阵的已接受语义是
“真实 owner disable 只发布一次，teardown 随后清 slot”，不能把 line 415 改成接受残留。

### 1.3 WIRE01 revision 失败

`CandidateBoardInputTests.UGUI_COPY_WIRE01...` line 111 expected `revision + 1`，actual 多两次。
同一循环把 `InvalidStart` 与 `ActionAccepted` 放在一个 cleanup branch：两者都先发送
`driver.Up(...)`，再发下一次有效 `Down`。但 ActionAccepted 的真实 route 已经完成并移除该
touch；再发送一个无前置 Began 的 orphan `Ended` 会被 uGUI 输入模块按新触点 press/release
处理，制造额外反馈 revisions。该测试输入序列不代表设备合法触摸。产品 feedback revision
语义和精确计数断言都不得放宽。

### 1.4 language popup readiness

`LocalizedTextBindingTests.ThreeBindUnbindCycles...` 在打开 LanguagePopup 后同一帧点击中文
按钮；按钮尚未经过动态 active/layout 的 bounded readiness，真实 GraphicRaycaster 发现它
不是 foremost hit。不得改为 `button.onClick.Invoke()`、`ExecuteEvents` 或跳过 foremost hit。

### 1.5 copy 与 destroyed Unity references

- `fm.name.level.ch01_01` 的 accepted CSV/Generated 简中值为 `第 1 关`；测试 line 285 的
  `第1关` 是旧字面值。产品已经通过稳定 key 解析正式值，不得倒改 CSV/Generated/产品。
- `PlayerDefaultReferenceView.Close` line 245 对已销毁的 `explanation` / `beat`
  `LocalizedTmpText` 使用 C# null-conditional。Unity fake-null 不被 `?.` 阻止，导致
  `MissingReferenceException`。FIX10 已把 `BattleLocalizedRows.first` 改为 Unity 有效性判断；
  本包只补齐同一 Close 尾部的两个直接 Unity component references。

## 2. 唯一允许修改的 6 个文件

| path | preimage SHA-256 |
| --- | --- |
| `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` | `290cb2742b556b53e297f1d9baf707befd16c7dd742521788d79a9518c29a325` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | `ee7cb5e159fb932ea1557667cc7f2409ed607784a11da6fe373458829c5a2fe2` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | `19b8981e76e7fbc6bdf516c40368dc65e8df95a72c470d14aeccc55dc2e697fa` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | `cfb3425d38ec97427a21f4a9d902969cfecc0ffe7245928ce9daaf339553fcb6` |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | `3309446fa74d8a4a126340700f4723ac45e0cb18ede73f746f0021504d6a8224` |
| `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs` | `31d266ad47865101db60c9f769eec549707fbd11937d103be1fa546e42ec0c89` |

禁止修改/创建其它 source、test、scene、prefab、font、`.meta`、Config、Generated、Packages、
ProjectSettings、Library、旧 evidence、团队文档或 Git。Unity 动态字体的既有合法增量只可作为
运行副作用审计，不是授权手改 font。

## 3. 精确实现

### 3.1 shared cancellation 使用真实 PlayMode lifecycle

只改 `BattleCancellationCases.AssertDisableAndExplicitCancellationOrders()`：

1. 在创建任何 `UguiHostRig` / `PlaybackPanelRig` 前切换到一个干净 empty scene，并
   `yield return new EnterPlayMode()`；
2. 原两组矩阵、mode 顺序、真实 `enabled` / `SetActive` / `Close` / `Unbind` / low-memory
   动作和全部 assertions 原样保留；
3. 两组矩阵全部结束后 `yield return new ExitPlayMode()`；
4. 不直接调用 `OnEnable` / `OnDisable` / `OnDestroy` / `OnApplicationFocus` private Unity
   messages，不新增 reflection 或测试 hook；
5. parent-first / child-first / host-view / battle-view 仍必须唯一发布 `FocusLost`，显式
   close/unbind 为 `Cancelled`，low-memory 保留既有语义；teardown 后 slot 必须 null。

不得只包其中一个 caller；共享 helper 自己拥有 PlayMode 边界，保证两个原方法走同一真实
lifecycle。不得把两个原测试拆分、复制或新增测试方法。

### 3.2 删除 ActionAccepted orphan Ended

在 WIRE01 line 106–113 的既有 branch 内：

- `InvalidStart` 仍先发送匹配其 earlier Began 的 `driver.Up(...)`；
- `ActionAccepted` 不再额外发送 `Up`，因为 accepted route 已经 Ended；
- 两者随后仍发送下一条真实有效 route 的 `Down`，并继续断言 feedback slot 清空、revision
  **恰好** `+1`、绑定行消失；
- 其它 6 类 feedback、fault loop、业务零写与 exact revision assertions 不变。

不得通过 `>=`、重设 revision、额外 `ClearVisibleFeedback()` 或产品特判掩盖错误输入序列。

### 3.3 动态 popup 复用 bounded readiness

在 `UguiHostRig` 把现有 `Ready()` 提取/扩展为可接收目标 `RectTransform` 的共享 bounded
helper。这里冻结两种入口，且二者只走一次共同的三帧等待，不能互相嵌套成六帧：

- 既有无参 `Ready()` 保持其原调用合同：以 board rect/canvas 做几何 readiness，但**不要求**
  board `activeInHierarchy`。这是因为 `ThreeBindUnbindCycles...` 在 startup page 尚未进入 battle 时就用
  它等待宿主稳定，startup 合法隐藏 board；把 active 断言塞进无参入口会制造静态可知的假失败；
- 新的显式目标入口 `Ready(RectTransform target)` 才要求该 target `activeInHierarchy`，用于打开
  LanguagePopup 后的 Chinese button；
- 两个入口可委托给一个 `(target, requireActive)` 私有实现，但每次调用都只能执行以下一组 bounded
  readiness，不得改变其它现有 caller。

每次必须保持：

- 恰好三次 `yield return null`，随后 `Canvas.ForceUpdateCanvases()`；
- 仅当 `requireActive=true` 时断言 target active in hierarchy；两种入口都要求关联 Canvas 非 null、
  Canvas 上唯一 `GraphicRaycaster`、目标 rect width/height 均大于 0；
- 不使用 hand-driven `Update`、直接回调或全局等待。

`ThreeBindUnbindCycles...` 点击 language-open 后，取得同一 generation 的 Chinese button，
对其 RectTransform `yield return` 该 helper，再使用原 `rig.Driver.Click(chinese)`。三个 cycle、
旧控件 listener 隔离、locale change count、stale submit assertions 原样保留。

### 3.4 两项机械修正

- `PlayerBattlePresentationTests.cs:285` 只把参数值 `第1关` 改为 accepted `第 1 关`；不得改
  localization keys、CSV、Generated 或产品解析。
- `PlayerDefaultReferenceView.Close()` 尾部把 `explanation?.Unbind()`、`beat?.Unbind()` 改成
  显式 Unity object 有效性判断后调用。`BattleLocalizedRows.Clear()`、listener removal、
  generation、owner detach、dynamic button destruction、`gameObject.SetActive(false)` 顺序不变。

## 4. selector 修正（validation harness only）

FIX10 的固定 28 名单继续读取：

```text
TestArtifacts/FightMatch/UGUI-01/d776c310bced809b9a7102f5587eda5958e78b7cc027d090f1b247751042bff4/
q4-correction-10/static/expected-fullnames-28.json
```

新 runner 必须对每个原始 fullname 使用标准 regex literal escaping，再生成有首尾锚的单一
alternation，例如 `^(?:<escaped-1>|...|<escaped-28>)$`。runner receipt 必须保存：

- 28 个原始 fullname；
- 每项 escaped literal；
- 最终 filter bytes/SHA；
- Unity argv 中实际传入值；
- XML actual fullnames 与 expected 的 ordinal set comparison。

禁止编辑 expected 名单、删除参数 suffix、按方法名前缀接受额外 case、或把 raw string 再跑
一次。actual 必须恰好 28，missing/unexpected 都为 0。

## 5. 有序验证与 stop gate

### 5.1 preflight/static

1. 确认专属 Unity/MCP/Hub/CrashHandler 进程为零；项目未在其它 Unity 进程打开。
2. 六个 preimage 全匹配；FIX10 candidate/source/resource manifests 与字体 chain 可重算。
3. 创建新 create-once evidence root，candidate identity 由六文件 after hashes + 既有固定 source/
   resources 算出；不得覆盖任何旧 root。
4. static diff 只能是 §3 的 bounded changes。测试方法 fullnames/count 与固定 222 名单不变。

### 5.2 Unity launch 1 — corrected focused 28

使用 Intel Unity `2022.3.18f1`、Metal、有图形；只运行 literal-escaped fixed 28。必须：

- XML exists，total/passed=`28/28`，failed/skipped/inconclusive=`0`；
- actual fullname set exact；6 个参数化 case 全部实际出现；
- shared cancellation 两个 callers 都通过；WIRE01、dynamic language cycle、receipt level、
  disposed reference case 全通过；
- zero uncaught `MissingReferenceException` / `NullReferenceException` / assertion/error logs。

任何失败立即停止，不跑 222，不开第二次修补，不送 R；保留 evidence 返回 blocker。

### 5.3 Unity launch 2 — exact 222

仅在 28/28 后运行 FIX10 固定 expected-fullnames-222；selector 同样逐 fullname literal escape。
XML 必须 222/222，name set exact，零 failure/skip/inconclusive/error。

### 5.4 final native reopen / resource audit

只在 222/222 后运行原有 native resource reopen gate；不生成 APK。保存最终：

- source closed-set、37 resources、Prefab/Scene/TMP settings/font/license before-after；
- dynamic font additive audit：FIX10 final 393 chars/glyphs、4 atlas 是本轮 before；只允许测试实际
  解析所需合法 glyph additive growth，既有 entries 不变、零删除/重映射；
- Metal renderer、无软件 fallback、进程树、graceful exit、旧 evidence/read-only roots equality。

Scene Canvas runtime rect/lossyScale/scaleFactor/首帧与 ART-02 LAYOUT 不在本包；不得顺手把
scene scale 0 改 1，亦不得声称布局/画面已关闭。

## 6. 交付合同

返回工程主程：

- owner thread/host/turn、用户 authority、candidate identity；
- 六文件 minimal patch 与 pre/post bytes/SHA；
- focused/222/reopen exact argv、timing、exit、XML/log bytes/SHA、name-set receipts；
- selector raw→escaped receipt；
- source/resource/font/process/protection manifests；
- 每条 acceptance 的 pass/fail；
- 唯一状态 `READY_FOR_INDEPENDENT_R` 或最早 blocker；
- changed files 恰好六个；未运行项明确列出。

禁止 commit/push/branch/worktree、删除旧 evidence、启动 APK/device/cloud、修改 layout 或自动送
R。C 完成后先回主程；主程机械接收并由 QA 调度独立 R。
