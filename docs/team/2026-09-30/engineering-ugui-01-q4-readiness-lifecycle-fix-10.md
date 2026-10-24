# UGUI-01-Q4-FIX-10：真实帧 readiness 与已销毁本地化行清理

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目标、责任与结论边界

FIX09 在同一 Metal run 中得到 targeted-9 `9/9`，随后 exact-222 `194/222`、28 failed、
0 skipped。失败不是一个可统一忽略的“测试环境”标签：27 项由 uGUI fixture 创建后未经过
真实帧注册就立即经 EventSystem 做 raycast/交互触发；另 1 项是真实产品生命周期缺陷，
`BattleLocalizedRows.Clear` 对已被 Unity 销毁的 `LocalizedTmpText` 使用 C# null-conditional，
绕过 `UnityEngine.Object` 的 destroyed-object 比较并抛 `MissingReferenceException`。

本包只修这两个已由 raw XML 支持的根因，保留所有业务、真实 GraphicRaycaster、
EventSystem、交互和生命周期断言；不把交互替换成直接 handler 调用，不吞异常，不降低
expected name/count，不重构产品。

- delivery owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- authority：`engineering-ugui-01-q4-dynamic-font-finalize-fix-09.md`，152 lines /
  9,568 B / SHA
  `7aa0d33d6b8c067ddd558b48c1d91a01a1e52d96aa9681400a5116a6712e854c`；
  FIX09 owner turn `01a0f4e5-57fb-75d3-8510-2748d91606d0`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求一次读齐 28 项、按真实失败栈
  区分 fixture 时序与产品生命周期并签最小纠正。

成功终点只能是 `READY_FOR_FINAL_R`；同一 R-TOOLS 才给最终 verdict。APK、Android、
device、LOC、B/L、release 与 Git 均不在本包。

## 1. 固定失败证据与当前基线

FIX09 evidence base：

```text
TestArtifacts/FightMatch/UGUI-01/
  c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280/
    q4-correction-09/
```

| item | identity |
| --- | --- |
| final receipt | 69,207 B；SHA `730d018c8661ea1fb4762c9f0617643f911474bcb72f34d4cc5d89d650c59331`；status `BLOCKED_EXACT222_TEST_FAILURE` |
| failure inventory | 27,798 B；SHA `4a95f17c89ddb99c469bde0e4505fa66fe8bd84f5749039ccd45207f3fc923ec` |
| targeted-9 XML | SHA `e0beafe2d7687fbbbbdb06787e9afcae4ef4ddd5e81af1017a207909dd4f747b`；9/9 |
| exact-222 XML | 205,639 B；SHA `8dc95c1c316da41ca552e070c07451fb959e87b8d8d9a9ddae666815455b7bd1`；194/28/0 |
| source manifest | SHA `b2e970c43fdd2c6c95a352bb43a0def2a1f78a927077157359d040715495944d` |
| resource manifest after exact-222 | SHA `75f3c6f6511b7215e6f899c11fe404bc3463e944d943b83c2f87465db58c5b3c` |
| current font | 8,558,317 B；SHA `f138d8c36bde60bbd8510b600c759eef9e3d0c78082e2cacb2a4818b96046eb9` |

执行前须机械重算 final-receipt bytes 与全部 hash。FIX09 exact-222 已使字体从 350 增至 369 characters/
glyphs（本轮新增 19），通过 additive audit；其它 36 resources、53 source 未变化。
禁止恢复到 66e796...、2a1868... 或重跑旧 candidate。

失败 inventory 固定分组：

```text
UguiHostRig.BeginRoute empty raycast hits                         5
Expected InvalidStart but actual result null                     1
UguiPointerDriver.Click empty raycast hits                       1
NullReferenceException in individual test                        2
PlayerBattleRig.Step expected non-null result                   17
Destroyed LocalizedTmpText MissingReferenceException             1
First-drag notice expected False but actual True                  1
```

其中前五组、最后一组共 27 项都在真正 pointer/raycast 后出现，且已有正对照：FIX08
`OwnerEnded...` 加三帧和 ForceUpdate 后 exact3 通过；FIX09 targeted-9 中所有显式 Ready
路径 9/9；exact-222 中未等待的 `OwnerCanceled...` / `SecondFinger...` 为空 hits，而同 fixture
等待的 `OwnerEnded...` 通过。`B09_AnotherPageOrHostDisposal...` 单独由 destroyed binding
stack 证明产品清理缺陷，不能归入 readiness。

## 2. 唯一允许修改的 10 个文件与 preimage

| path | preimage SHA-256 |
| --- | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | `44405bbe47e563c32c3fdae64f2589f3def5b120807c9a1f01e7feaec1296200` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | `123105294a056e509c37c759876aab8ed11b7199486976b2d0caa827328f16c1` |
| `Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs` | `4bab2235e4b0253d5fa7e4fe3227c9be5d70b4f597ee65b87540f2daf96f1627` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | `d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d` |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | `095ce3720258d860c70883201288c009efa397cddcb6bad5ec7bc713f6358cd1` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | `91402138964cc1c60d723da3f0dcebb3348f666e3bf5f3d6edcd3eee9b76a9c3` |
| `Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs` | `7dc87fe5703087f5cec795e3597a18e9847503869b49ca36d5022936b6316e7e` |
| `Assets/Tests/EditMode/FightMatch/PlayerBattleFlowTests.cs` | `2e5e3342a44c35d23946e65f6846e65cd74243345002acfb3d48f751a4fb48b8` |
| `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs` | `da4b746c416d70f7b6e65540de66b2e0ad215a9c58ccc8f97bf3ce10bf007324` |
| `Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs` | `fb490672918c76585ca746c0f4a33d91c3b4af5b3d02b0d9ad1eb510309757f6` |

其它 source/test/resource/meta/prefab/scene/settings/package/Git 全部只读。不得创建新 source
或 `.meta`；不得修改断言文字、业务调用顺序、测试数据、production public API、serialized
field、Prefab/Scene、字体或 runner validation tool。

## 3. exact implementation contract

### 3.1 fixture readiness，只在真实交互前等待

1. `UguiHostRig` 增加一个共享 `IEnumerator Ready()`：恰好等待三次 `yield return null`，
   然后 `Canvas.ForceUpdateCanvases()`；断言 board canvas、其唯一 GraphicRaycaster、正的
   rect width/height均存在。不得启用 `FightMatchPlayerHost.Start`、打开真实 player store、
   直接注册 internal RaycasterManager 或跳过 EventSystem。
2. `PlayerBattleRig` 增加同样三帧/ForceUpdate/board canvas/raycaster/positive rect 的
   `IEnumerator Ready()`；`PlayerBattlePanel.Ready()` 改为复用它，避免两套 readiness。
3. `BattleCancellationCases.AssertDisableAndExplicitCancellationOrders` 改为 `IEnumerator`。
   每个 `UguiHostRig` 在首次 `BeginRoute` 前 `yield return r.Ready()`；每个
   `PlaybackPanelRig` 在首次真实 pointer 前 `yield return r.Ready()`。两个既有 UnityTest
   caller 必须 `yield return` 此 helper；原取消顺序、预期 feedback 与零写断言逐字保留。
4. 其余 27-failure 相关测试在每个新 rig 的**首次真实 pointer/raycast/Step 前**
   `yield return rig.Ready()`。原 `[Test] void` 按需最小改为 `[UnityTest] IEnumerator`。
   原 `[TestCase]` 参数化 UnityTests 必须按 Unity Test Framework 1.1.33 本地文档改用
   `[ValueSource]` 的固定 closed arrays；case value、显示 fullname 与总 case 数保持不变。
5. 不给纯 read/controller tests普遍加等待，不在 driver中 fallback到 direct
   `ExecuteEvents`，不改变 `RaycastAll` foremost-hit 断言，不用 sleep/realtime wait替代帧。

需覆盖的具体 test methods（参数化 case 仍各计一项）：

- `CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources`；
- `CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit`、
  `UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite`；
- `LocalizedTextBindingTests.LocaleChangeAndRebuiltPagePreserveHeadRequestAndPlaybackToken`、
  `ThreeBindUnbindCyclesRemoveOldListenersAndProtectTheNextGeneration`；
- `UguiSceneCompositionTests.OwnerCanceledNormalizesBeforeReleaseAndNeverCommits`、
  `SecondFingerCanceledLeavesOwnerAndOwnerEndedStillCommitsOnce`；现有 `OwnerEnded...` 复用
  helper，不能删除等待；
- `PlayerBattleFlowTests` 的 `B01_DueRecovery...`、`B02_ActualMultiFace...`、
  `B03_Restart...`、`B04_FirstPlayback...`、三例 `B12_ResultDestinations...`、
  `B12_ResultReplay...`；
- `PlayerBattlePresentationTests` 的 `UGUI_COPY_D02...`、`UGUI_COPY_WIRE03...`、
  三例 `B06_B09_EntireViewTreeRebuild...`、`B09_BorrowedDetach...`、
  `B04_ButtonAvailability...`、`B09_StalePageEpoch...`；
- `PlayerDefaultReferenceTests` 的 `UGUI_COPY_WIRE02...`、`B10_ReferenceLabels...`、
  `B11_FaceMismatch...`、`B11_RealPlayback...`。

### 3.2 产品生命周期一行修正

在 `BattleLocalizedRows.Clear()` 中保留 extra rows 的逐项 Remove；把
`first?.Unbind()` 改为显式 Unity destroyed-object 有效性判断：只有 `first != null` 时
调用 `first.Unbind()`。这是使用 `UnityEngine.Object` overload 的产品修正；不得 catch
`MissingReferenceException`、改 `LocalizedTmpText`、跳过 service unsubscribe/report、
改 Dispose/Unbind 顺序或让 stale row继续存活。

## 4. 静态验收

- 只改 §2 十文件；完整 patch逐项绑定 pre/post SHA；source candidate重新计算；
- 27 个 readiness失败对应的真实交互前都有共享 readiness，无其它测试无差别等待；
- 2 个参数化方法仍分别是 3 cases，fullnames 与 FIX09 exact-222 expected set一致；
- exact-222 discovery仍精确 222；不新增/删除/重命名 case；
- 产品 diff只有 `BattleLocalizedRows.Clear` 的 Unity null comparison；
- 断言、期望业务结果、fixture input、生产 public/serialized surface不变；
- 当前字体和其它 36 resources在首个 Unity launch前等于 FIX09 after baseline。

任一不成立即 `BLOCKED_STATIC_SCOPE`，不运行 Unity。

## 5. 唯一验证序列（Metal graphics）

新 evidence sibling，执行前必须不存在：

```text
TestArtifacts/FightMatch/UGUI-01/<new-source-candidate>/q4-correction-10/
```

所有测试 launch 使用 Unity 2022.3.18f1、`-batchmode`、Metal graphics，**不传
`-nographics`、不传 `-quit`**；每轮 expected/actual fullnames 精确、0 skipped、exit0、
无 compiler/unhandled/missing-reference/serialization error，进程自然静止。

1. **focused-28**：只运行 FIX09 failure inventory 的 exact 28 fullnames；预期 28/28。
   timeout `<=360s`。若 ValueSource 导致 fullname/count drift，先停为
   `BLOCKED_NAME_SET_DRIFT`，不得放宽到模糊匹配。
2. **exact-222**：沿 FIX09 同一 15-fixture filter与原 expected 222 fullnames；预期
   222/222。timeout `<=600s`。该 run自身也覆盖旧 targeted-9 name set；不另跑旧
   targeted-9/exact3，避免无信息增益的第四次测试启动。
3. **native final reopen**：仅在 28/28 和 222/222 后，一次独立 Metal
   `-batchmode -quit`，无 tests、无 `-nographics`，timeout `<=300s`。复开前后新 source
   closed set、37 resources、font structure/bytes、Prefab/Scene/meta/settings/old roots稳定。

新 Unity launches最多 3；失败不重跑。每阶段保存 argv/PID/UTC/duration/exit/stdout/
stderr/log/XML/hash、renderer、expected/actual names、source/resource manifests、font
before/after/structure/additive audit、process tree/quiescence。current-launch唯一确认的专属
VBCSCompiler可按既有 exact pipe `-shutdown`一次；禁止 signal/global shutdown。

字体沿 FIX09 dynamic contract：每轮可 byte-identical或只 additive characters/glyphs；
existing entries/pixels、material/source/config/GUID/reference不变，atlas仍4张；其它36资源
严格不变。合法新增直接成为下一 baseline；不得回滚或手写字体。最终 reopen必须 byte稳。

## 6. 预算、失败与回传

- new evidence `<=128 MiB`；temp `<=128 MiB`；log/XML each `<=8 MiB`；free space
  `>=2 GiB`；
- compile/Q1由 focused-28 同一真实 Unity compile覆盖；无独立 compile launch；
- Android/APK/device/Windows/LOC/B/L/SaveAssets/PrepareResources/Git均 `NOT_RUN`；
- earliest failure后停止依赖步骤并封存，合法 status：`BLOCKED_STATIC_SCOPE`、
  `BLOCKED_NAME_SET_DRIFT`、`BLOCKED_FOCUSED28`、`BLOCKED_EXACT222`、
  `BLOCKED_RESOURCE_DRIFT`、`BLOCKED_PROCESS_DRIFT`；成功只写
  `READY_FOR_FINAL_R`。

C 回传：packet/activation、owner turn、10-file patch/pre-post manifests/new candidate、
focused28、exact222、dynamic-font chain、final reopen、最终53+新增受审文件/37/old roots、
process/resource totals、final receipt与 NOT_RUN。完成后停止，不自行送 R。

主程审 actual evidence 后交同一 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local`；其累计 pre-review 已覆盖 b9→c428
八个源码差异与 Prefab，最终只需补本包十文件 delta、失效动态证据、最终字体、222与
native reopen，但 verdict仍必须覆盖完整 candidate，只能一个 `ACCEPT` / `NEEDS_FIX` /
`REJECT`。

## 7. 签包回合未执行声明

本文件创建回合只新增 packet。没有修改十个 source/test 文件、资源、字体、Prefab、Scene、
settings或Git；没有启动 Unity、测试、reopen、APK或设备动作。实际执行等待主程回传本文件
final SHA-256。
