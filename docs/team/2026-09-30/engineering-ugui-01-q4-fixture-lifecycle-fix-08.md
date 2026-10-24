# UGUI-01-Q4-FIX-08：EditMode raycaster 登记与首帧深度生命周期修复

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目标、责任与结论边界

本包只修 DIAG-07 已证明的两个 **test-fixture lifecycle** 问题，不修改产品行为：

1. B16 与 H01：目标 `GraphicRaycaster` 未登记到 `RaycasterManager`；直接调用同一
   raycaster 时 board 均为首项，故不得改 EventSystem、输入、board、Prefab 或断言；
2. OwnerEnded：首次失败更早发生在 board/CanvasRenderer depth `-1`；同类 H01 fixture
   已用三个 `yield return null` 后得到有效 depth `3`，故只给这个同步测试同样的首帧就绪
   窗口，并同时修其 raycaster 登记。

- delivery owner：C thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：仓库 `AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已授权 scoped fixture fix。
- 固定上游 product candidate：
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。
- 通过终点只能是 `READY_FOR_ENGINEERING_REVIEW`；C 不得自判 Q4 或 UGUI-01 接受。

## 1. 固定输入与已诊断事实

| 输入 | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/engineering-ugui-01-q4-raycaster-manager-diag-07.md` | 8,510 | `ec3d1b80f9172482c0647cd2a49dfb8fb67459338da80f81064a2596ae3d203f` |
| `q4-diagnosis-07/final-receipt.json` | 66,418 | `0542f347c0a0108b63a2545bfd5d9cc456630a15ff85d5b7d6951a6f9ef2d0d6` |
| `q4-diagnosis-07/diagnostic-extraction.json` | 60,414 | `816b5bf63c899c9548aca8908025b4fc5014f59ee1a7d35c8cca1ab3fbe09968` |
| `q4-diagnosis-07/probe-run/tests.xml` | 33,080 | `91451b221082730f79bbfdc6a5b8d64064d9f641c6767ddeb5623e3e7d4ad74b` |
| `q4-diagnosis-07/probe-run/unity.log` | 96,434 | `74df6533e839dba280f77c6dcb6a3995f4f288b345a7be61155242e01045837b` |
| `q4-diagnosis-07/restore-proof.json` | 1,269 | `6306175fa855081ff7f52d644afa22f3fdda2a4eee244a5d9d1e4f915e03c893` |

表内 `q4-` 路径的 base 是：

```text
TestArtifacts/FightMatch/UGUI-01/
  0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
```

DIAG-07 是一次非 Null graphics exact-3：Unity exit `2`，XML `0/3`，34.1995s；四个
snapshot 完整且没有 probe exception。逐项首因：

| test | depth | manager exact member | direct board hit | classification |
| --- | ---: | --- | --- | --- |
| B16 before/after | `10` | false；count `0` | index `0` | `DIAGNOSED_RAYCASTER_MANAGER_REGISTRATION` |
| OwnerEnded | `-1` | false；count `0` | none | `DIAGNOSED_GRAPHIC_DEPTH_LIFECYCLE` |
| H01 | `3` | false；count `0` | index `0` | `DIAGNOSED_RAYCASTER_MANAGER_REGISTRATION` |

DIAG-07 后四源已恢复，53 source、37 resources 和旧 roots 一致；唯一 VBCSCompiler 用
exact pipe `-shutdown` 正常退出，exit 0，无 signal。当前固定源身份：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 16,206 | `9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |

`CandidateBoardInputTests.cs` 与 DIAG-07 的第四临时源必须保持 42,233 bytes／
`d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d`，本包不得修改。

## 2. 唯一代码写白名单与精确修法

只允许修改第 1 节列出的三个 fixture/test 文件；禁止新增文件或 `.meta`。

### 2.1 `CandidateBoardInputTestData.cs`

只在 `BattleUguiRoot` 构造函数调整现有测试 Canvas 的激活生命周期：

1. 创建带 `Canvas` 与 `GraphicRaycaster` 的 `Root` 后立即 `SetActive(false)`；
2. 设置现有 Canvas render mode；取得**现有且唯一** `GraphicRaycaster`，将
   `runInEditMode = true`；
3. 按原代码创建 `StaleButtons` 后 `Root.SetActive(true)`。

不得 AddComponent 第二个 raycaster，不得直接调用 `RaycasterManager.AddRaycaster`，不得
改 `UguiPointerDriver`、module.Process、route、input 或断言。

### 2.2 `UguiSceneCompositionTests.cs`

在 `UguiHostRig` 构造函数中，Prefab instance 已处于 inactive、`HostRoot` 尚未激活时：

- 取得该 instance 子树内（含 inactive）的现有 `GraphicRaycaster` 集合；要求精确一个；
- 将这个现有组件的 `runInEditMode = true`；
- 随后保持原 `HostRoot.SetActive(true)` 与 `Bind()` 的 inactive→active 顺序。

仅对 `OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag`：

- 用 fully-qualified `[UnityEngine.TestTools.UnityTest]` 与
  `System.Collections.IEnumerator` 改为 coroutine，不为此新增 using；
- `rig.Enter()` 后、读取 `before` 与第一次 raycast 前精确执行三个连续
  `yield return null`，再调用一次 `Canvas.ForceUpdateCanvases()`；
- 其余 observation、route、commit count、event ordering 与 active-pointer 断言逐字保留。

不得把等待放进所有测试，不得删减 assertion，不得直接设置 Graphic depth/renderer，
不得用 direct raycast 替换 EventSystem.RaycastAll。

### 2.3 `FightMatchHostTestFixture.cs`

在 `HostPanel.Rebind()` 中，Prefab instance 已 `SetActive(false)` 后、`View.Bind` 与重新激活
之前：

- 取得子树内（含 inactive）的现有 `GraphicRaycaster` 集合；要求精确一个；
- 将这个现有组件的 `runInEditMode = true`；
- 保持原 Bind、`Root.SetActive(true)`、`Canvas.ForceUpdateCanvases()` 顺序及 H01 三帧
  `Ready()` 不变。

不得改 Host routing/lifetime、Detach、Draw、input 或业务断言。

## 3. 永久禁止面与静态门

禁止修改：全部产品 source、Prefab/Scene/font/resource、`.meta`、asmdef、Packages、
ProjectSettings、Library/Temp/Logs/UserSettings、generated project files、Git、旧 evidence。
不得添加 reflection、test-only runtime API、manual manager registration、direct-raycast bypass
或跳过/放宽测试。

写入后、Unity 前必须证明：

- Git-independent 三文件 before/after 最小 diff 只包含 §2；
- `CandidateBoardInputTests.cs` 固定 hash 不变；产品/资源/meta/settings delta 为 0；
- 三个 fixture 中每个目标现有 raycaster 只设置一次 `runInEditMode = true`；
- `RaycasterManager.AddRaycaster`、`GraphicRaycaster.Raycast`、新增 EventSystem、断言删除为 0；
- OwnerEnded 的 route/commit/event/assertion 文本不变，仅 test kind 与就绪窗口改变。

## 4. 新候选、证据根与唯一执行顺序

三文件写入后重算排序 53-source canonical SHA 为 `<NEW_SOURCE_CANDIDATE>`；它必须不同于
上游 candidate。create-new evidence root：

```text
TestArtifacts/FightMatch/UGUI-01/<NEW_SOURCE_CANDIDATE>/q4-correction-08/
```

root 必须不存在；存在即停止，不清理、不复用。旧 0ff/q4-correction-02/03、
q4-diagnosis-04/05/07 与 PROCESS-06 全部只读。

严格按以下顺序，每步通过才进入下一步：

1. **static/preflight**：保存 packet、activation、三 preimage/minimal diff、53-source、
   37-resource、旧 roots 与 process/free-space；执行 §3 静态门。
2. **graphics exact-3**：一次，`-batchmode` 且不传 `-nographics`，单个分号 filter：

   ```text
   FightMatch.Core.Tests.CandidateBoardInputTests.B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce;FightMatch.Core.Tests.UguiSceneCompositionTests.OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag;FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce
   ```

   必须 XML 精确 3/3、failed/skipped/inconclusive 皆 0。失败即封存并停止，不跑 targeted9。
3. **targeted-9**：一次，沿用 q4-correction-02 的 exact filter 与原 `-nographics`：

   ```text
   FightMatch.Core.Tests.CandidateBoardInputTests.B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce;FightMatch.Core.Tests.UguiSceneCompositionTests.BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical;FightMatch.Core.Tests.UguiSceneCompositionTests.OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag;FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_ReadOnlyRefreshKeepsTheOriginalActionButtonAndContextClickable;FightMatch.Core.Tests.LocalizedTextBindingTests.MissingKeyParametersTemplateFontAndGlyphOpenTheRealHostGate;FightMatch.Core.Tests.PlayerNavigationPresentationTests.UGUI_COPY_B02_PreferenceSavedRequiresExactCommittedSubtype;FightMatch.Host.Tests.FightMatchHostResourceTests.H05_SceneReferencesOneHostPanelThemeDynamicFontAndViewableLicense;FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce;FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H04_ChangedBeforeHostRequestedStillDeliversOneOriginalH02AndRejectsDuplicates
   ```

   必须精确 9/9、其余 0。失败即停止，不跑 exact222。
4. **exact-222**：一次，使用已批准 implementation packet §6 的原 15-fixture 单参数 filter
   与 `-nographics`；结果名称集合必须精确等于已接受的 222 baseline，222/222、其余 0。
5. **final protection**：53 source 只允许三项预期 delta；37/37 resources、产品、meta、
   settings、旧 roots 全相等；保存 final receipt 后停止。

每个 Unity launch timeout：exact3/targeted9 `<=240s`，exact222 `<=600s`；最大 3 次，禁止
重跑或追加诊断。每个进程前确认没有另一 Unity；每次保存 argv/PID/UTC/duration/exit、
stdout/stderr/log/XML/hash 与 renderer 非 Null 证明。

Unity 退出后自然等待 owned children。若唯一剩余 current-launch descendant 是已机械确认的
本 Unity bundle VBCSCompiler，可复用 DIAG-07 的 exact executable/DLL/unique pipe
`-shutdown` 协议一次；禁止 signal、kill、全局 shutdown、retry 或碰非 owned process。
不得设置/覆盖 `HOME`、`home`、`CODEX_HOME`。

资源上限：每个 log/XML `<=8 MiB`，整个 evidence `<=96 MiB`，temp `<=128 MiB`，free-space
floor `2 GiB`。Q1/recovery/PrepareResources/SaveAssets、4181/Q5、Android/APK/device/Git 均
`NOT_RUN`。

## 5. 验收与回传

- [ ] B16/H01 仍走原 EventSystem/module 路径，未用 direct raycast 或 manager API 绕过；
- [ ] OwnerEnded 只增加与已通过 H01 等价的三帧 readiness，原业务断言全部保留；
- [ ] graphics exact3=3/3、targeted9=9/9、exact222=222/222；
- [ ] 三文件之外 source、37 resources、Prefab/Scene/font/meta/settings 与旧证据全相等；
- [ ] 每个 launch/process shutdown 均有真实证据，无 signal、无第二修法；
- [ ] 最终状态只为 `READY_FOR_ENGINEERING_REVIEW` 或最早失败门。

C 回传：packet/activation、owner thread/turn、new candidate、三文件 before/after bytes/SHA 与
minimal diff、static gates、三次真实 test receipt/XML/log、process/quiescence、53/37/旧 roots、
未运行项与 final receipt。完成后停止，不自行送 R、不提交/推送/建分支。

工程主程审实际 patch/evidence 后，才交原 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 独立复核并给唯一
`ACCEPT`／`NEEDS_FIX`／`REJECT`。
