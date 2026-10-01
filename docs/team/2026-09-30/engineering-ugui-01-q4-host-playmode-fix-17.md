# UGUI-01 Q4 FIX17 — Host lifecycle tests in PlayMode

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. Delta目标与责任

本包只修 FIX16 Host30 暴露的测试运行模式缺口：H01/H04 的真实 prefab 生命周期必须在
PlayMode 内构造、交互和清理。产品、fixture、其余测试、资源与所有业务断言保持不变。

- delivery owner／唯一 Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发/收件：工程主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- authority：`AGENTS.md` §6 standing workflow；中央已接 ARCH 只读诊断并批准最小 FIX17。
- ARCH diagnosis：thread/turn
  `01a0f2e6-1ac3-7d10-8855-54192b9489d4` /
  `01a0f59b-2580-7a10-a233-669cbe178c47`。
- 固定产品候选：candidate
  `1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b`；
  sourceCandidate
  `c45ed6d0c997867dc4bb60ac02d4f3ed832685b99149837c24c95aac7d8c2466`。
- FIX16 packet：`engineering-ugui-01-q4-grouped-validation-fix-16.md`，190 lines /
  9,423 B / SHA-256
  `201a3e27f30dca739e2b41be3db6975feb42383de0791dedc5245560730bbd7e`。
- FIX16 receipt：`q4-correction-16/final-receipt.json`，13,301 B / SHA-256
  `2fb5a6e81594011bcf6e038fb2d969e66d4e60ccd30d49ff73d2adaf0424525e`。

作者只能返回 `READY_FOR_INDEPENDENT_R` 或最早 blocker，不能自判接受、进入 LAYOUT、API24、
APK 或设备阶段。

## 1. 固定归因

Host30 正常结束 30 total / 28 pass / 2 fail；H05 scene/resource通过。两项失败均在首次
`HostPanel.Draw` 的 `CandidateBoardElement.CellCenter` 报 geometry unavailable：

- `H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce`；
- `H04_RealBoardVictorySettlesOnceAndTheOriginalReceiptSurvivesWholeHostReconstruction`。

失败早于 H01 的 Detach/Pause-once 和 H04 的 victory/settle/reconstruction，所以未执行到测试主题。
Prefab祖先链已核正确。HostPanel 是 Startup隐藏绑定，`rig.Enter()` 后才在 inactive battlePage内
`battleView.Bind`，随后依赖普通 runtime MonoBehaviour `OnEnable` 完成 native bind；Host30却以
EditMode运行。FIX15 exact2只覆盖 EditMode active-bind 与 PlayMode delayed-OnEnable，未覆盖
EditMode inactive-bind→activate。

本轮是判别性修正：若真实 PlayMode仍在同点失败，立即升级为产品激活路径缺陷；不得继续迁就
fixture或放宽几何合同。

## 2. 唯一写域与实现合同

唯一允许修改：

| path | preimage bytes | preimage SHA-256 |
| --- | ---: | --- |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs` | 14,051 | `7b1319e8aea1a5d47820e311a39444b0fca0bad6bd004c704aab88026af6929f` |

只允许对 §1 两个现有 `[UnityTest]` 做下列结构变换：

1. 每个方法在创建 `HostRig`／`HostPanel` 前 `yield return new EnterPlayMode()`；
2. 原 using、原语句、原断言、原循环和原 `yield return` 顺序逐字保留在 PlayMode区间；
3. using完整结束、真实对象已清理后 `yield return new ExitPlayMode()`；
4. fullname、attribute、参数和测试数量不变，不新增 helper或测试。

禁止修改 `FightMatchHostTestFixture.cs`、所有产品源码、`CandidateBoardElement.CellCenter`、prefab、
scene、font、`.meta`、asmdef、Packages/ProjectSettings、生成资源或旧 evidence。禁止手调
`OnEnable`／`BindNative`、强行提前激活 BattlePage、reflection、catch异常、fallback geometry、
跳过或削弱断言。

以下相邻文件必须 byte-identical：

- `FightMatchHostTestFixture.cs`：21,662 B /
  `b38559e449bb9f87c0f4b2159196000894b1b0a2893be2a1c66fadb609623733`；
- `CandidateBoardInputView.cs`：34,540 B /
  `659d879b2154e3e7b9347638cfeaf5f3f33b4daea8908b14711712170b5ed3ed`；
- `CandidateBattlePlaybackView.cs`：13,944 B /
  `38c7cfbb999897db56e90c37b6c7cacdef95d0aaabae87b8c671e1f262ea6216`；
- `PlayerBattleView.cs`：25,884 B /
  `3baa9fc9f2f4a565a4933130e7f5c46e6c16edf4e7a3a0d13c8dac9761201e7c`；
- `PlayerDefaultReferenceView.cs`：18,882 B /
  `b3f28fd28df9687e8a92b95a81b2bd8d65fe974e43b962436b2a75370bfb8cac`；
- `FightMatchHostView.cs`：14,379 B /
  `d7a29970b0fb8b594e7114a883b82ffef92c47181218df1bec1f420dfda72136`。

## 3. 有序验证与 stop gates

全部沿用 FIX16 的进程、资源、字体、manifest、超时、evidence与 owned-PID规则；新 create-once root
命名 `q4-correction-17/`。签发后先冻结新的 test-only candidate；产品 sourceCandidate必须仍为
`c45ed6d…c2466`，37 resources与 398/398/4 font baseline不变。

1. **static**：delta恰为 §2 单一文件、两个方法各一对 Enter/ExitPlayMode；两个 fullname和原断言
   集合不变；相邻 closed set、产品、资源均相等。
2. **Host30 一次**：沿用 FIX16 exact Host30 selector和180s门，必须 30/30、零
   fail/skip/inconclusive/error。H01/H04必须在 PlayMode中越过首次 Draw，并从原输出/断言证明：
   - H01 完成首次提交、Detach、Rebind、同一 token/request与 Pause exactly-once；
   - H04 完成全部绘制、victory、settle、whole-host reconstruction、原 receipt/commit与文件不变。
   不另跑 focused2，避免相同测试重复启动。
3. Host30任一失败立即停止；若 H01/H04仍为首次 Draw同点，回
   `BLOCKED_PRODUCT_ACTIVATION_PATH`。不得跑 Core190或二次修补。
4. **Core190**：仅在 Host30通过后沿用 FIX16 exact Core190 selector和420s门，必须190/190。
5. **并集证明**：FIX15 frozen exact2保持有效，因为两项产品源码及其测试未变；证明
   exact2 + Host30 + Core190 的 fullname集合 exact等于 fixed222，且所有成员实际通过。不得把
   FIX16的600s无XML写成通过或归因完成。
6. **native reopen/resource audit**：仅在并集222成立后按 FIX16执行一次；37 resources byte-identical，
   font 398 characters / 398 glyphs / 4 atlas，零生成漂移。

任一门失败即 bounded seal并返回；不在同一 root改代码、重跑失败选择器或追加启动。成功回传
actual thread/host/turn、test-only candidate、单文件 diff、Host30/Core190/union/reopen identities、
进程/超时/资源证据与 `READY_FOR_INDEPENDENT_R`。

