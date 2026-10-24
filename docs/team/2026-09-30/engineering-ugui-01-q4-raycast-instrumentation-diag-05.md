# UGUI-01-Q4-DIAG-05：非枚举式 registry 探针重取

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 结论、目的与授权

DIAG-04 的一次 graphics exact-3 未取得产品诊断：三个测试都先在临时探针
`registry.Any(...)` 内抛出 `IndexedSet<T>.GetEnumerator()` 的
`NotImplementedException`。四个临时测试文件随后已在全部相关进程自然退出后精确恢复，
产品 candidate 仍为：

```text
0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91
```

DIAG-05 只重取同一组只读 snapshot，并将两处不受支持的 registry 枚举改为 uGUI 1.0.0
明确实现的 `IList<Graphic>.Contains(board)`。不得修改产品、Prefab、Scene、font、meta、
asmdef、设置、原断言、原输入、原 frame/yield 或诊断字段。

- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已授权 scoped common-root diagnosis。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发／接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 本包不是产品修复包；运行后四个 test source 必须再次恢复固定 preimage。

## 1. 固定输入与 DIAG-04 裁定

签包前机械读取：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/engineering-ugui-01-q4-raycast-instrumentation-diag-04.md` | 11,627 | `70ab8461a3cbc480df98b0af5606be6699d33c4014e717cfa1fe8a542002e30b` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-diagnosis-04/temporary.patch` | 25,898 | `58dfb36c7bbb917927d4bfa81fdd1e289d70b308d039cc62759d38f652f74b0d` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-diagnosis-04/probe-run/tests.xml` | 14,996 | `daf041f290e27464e0f5b23ad7314f9bac36d4720222fcee3997716950f4939f` |
| `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-diagnosis-04/restore-proof-followup.json` | 2,351 | `caccaec6022f9d43807aaf36620d53779d9a9f5bde0977ef40bf92668db16dde` |
| `Library/PackageCache/com.unity.ugui@1.0.0/Runtime/UI/Core/SpecializedCollections/IndexedSet.cs` | 7,119 | `67e82737a015c50084766be7bffd284f6950ecfd91e395e6c67d8f64a19aea8b` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,233 | `d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 16,206 | `9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |

源码事实：`IndexedSet<T>.GetEnumerator()` 明确抛 `NotImplementedException`；同一类型的
`Contains(T)` 直接调用内部 dictionary 的 `ContainsKey`，不枚举。DIAG-04 三项均在临时
helper 内失败、`asserts=0`、没有任何 `UGUI_RAYCAST_DIAG|` 行，因此裁定：

```text
NEEDS_FIX / INVALID_INSTRUMENTATION_RUNTIME
```

它没有确认或反证 H2A/H2B/H3/H4，也不改变 FIX-03 的产品红灯。

## 2. 临时写白名单与唯一允许差异

只允许临时修改以下四个文件：

```text
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs
```

以 DIAG-04 `temporary.patch` 为固定探针定义；从 §1 四个原始 preimage 重新应用时，临时
代码与 DIAG-04 必须逐项等价，唯一允许的两处文本差异均为：

```diff
- registry != null && registry.Any(graphic => ReferenceEquals(graphic, board))
+ registry != null && registry.Contains(board)
```

两处分别位于 Core.Tests 与 Host.Tests 的临时 `UguiRaycastDiag04.Capture` helper。helper
名称可保持 `UguiRaycastDiag04`，不得借机重命名或重排。禁止对 GraphicRegistry 返回值
使用 `Any`、`foreach`、`GetEnumerator`、`Cast`、`ToArray` 或其它隐式枚举；不得 reflection。

静态门必须证明：

- DIAG-04 patch 中恰有 2 个 `registry.Any`，本次临时 diff 中为 0；
- 本次临时 diff 中恰有 2 个 `registry.Contains(board)`；
- DIAG-04 的四个固定 snapshot 插入点、字段、原 Assert/name/attribute/yield/input calls
  数量和相对顺序不变；
- 无 production/resource/meta/asmdef/setting 写。

## 3. evidence root 与强制恢复

只允许永久 create-new：

```text
TestArtifacts/FightMatch/UGUI-01/
  0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
    q4-diagnosis-05/
```

root 必须在 preflight 时不存在。DIAG-04、q4-correction-02/03、b9/f21/8a3 全部只读。
保存 packet/preflight、四 preimage、临时 diff、静态门、run/XML/log、process/quiescence、
diagnostic extraction、restore proof 与 final receipt。

运行成功、测试失败、compile error 或 instrumentation runtime error都必须在相关进程自然
退出后应用精确反向 patch，使四文件恢复 §1 bytes/SHA。禁止 `git reset/checkout/stash`；
不得把临时 probe 保留到下一包。

## 4. 唯一执行序列

1. 核 packet/input identities、新 root absent、无其它 Unity；冻结 53-source、37-resource、
   五棵旧 evidence root 与 Git read-only状态。
2. create-new root；保存 preimage；按 §2 生成临时 instrumentation，保存 diff/hashes，先过
   非枚举静态门。
3. 仅一次 Unity graphics exact-3：`-batchmode`，不传 `-nographics`，filter 精确为：

```text
FightMatch.Core.Tests.CandidateBoardInputTests.B16B03_RealPanelDownCapturesOnlyAndLastUpCellSubmitsOnce;
FightMatch.Core.Tests.UguiSceneCompositionTests.OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag;
FightMatch.Host.Tests.FightMatchHostRoutingLifetimeTests.H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce
```

   实际 argv 中 filter 为同一分号连接的单参数；必须证明非 Null renderer。
4. 自然等待 Unity 与本次直接 owned compiler/test descendants 退出；不得 signal。待静止后先
   恢复四文件，再做诊断归类。不得启动第二次 Unity。
5. 提取每条 `UGUI_RAYCAST_DIAG|`；按 DIAG-04 §6 逐 test 使用首个不满足条件分类。若
   snapshot 缺字段或又发生 probe exception，返回 `DIAGNOSIS_INCOMPLETE`，不得猜产品修复。

唯一 Unity launch 上限 1；launch→最终静止总预算 `<=360s`，分段轮询，每次等待不超过
60s；一个顶层 process tree；无 signal。XML/log 各 `<=8 MiB`，diagnostic `<=2 MiB`，
evidence `<=32 MiB`，temp `<=64 MiB`，free-space floor `2 GiB`。

## 5. 诊断输出与判别

snapshot 字段与四个插入点完全沿用 DIAG-04 §4：B16 before/after down、OwnerEnded 原
Raycast 后、H01 原 RaycastAll 后。必须复用原 hit list，不得为探针再次派发 input/raycast
或改变状态。

逐 test 仍按 DIAG-04 §6 判别：registry/depth、screen rect、filter、foremost overlap、
module lifecycle、event dispatch state；不同首因返回 `DIAGNOSED_SPLIT_ROOTS`。本包取得
诊断后也不得修改产品或测试 fixture；主程另签最小 FIX-04。

## 6. 验收与回传

- [ ] 临时 diff 相对 DIAG-04 只有恰好两处 `Any` → `Contains`，且不存在 registry 枚举。
- [ ] exact 3 名称、原 Assert、pointer/input/route/frame 语义不变。
- [ ] graphics run 使用 Apple Metal/其它非 Null renderer；真实结果原样保存。
- [ ] 四个 snapshot 完整输出，或逐字段 `API_UNAVAILABLE`；无 probe exception。
- [ ] 四 test source 最终精确恢复 §1；53 source、37 resources、旧 evidence 不变。
- [ ] targeted 9、exact 222、Q1、recovery、Prepare/SaveAssets、4181、Q5、Android、APK、
  device 均 `NOT_RUN`。
- [ ] production/asset/meta/setting/Git 写均为 0；无第二次 Unity、无 signal。

Owner 回传 packet/preflight/static gate/run/XML/log/process/final receipt identities；临时 diff
与 temp/final hashes；全部 snapshots；逐 test classification；source/resource/old-root equality；
NOT_RUN 清单。完成后停止，不自行修产品、送 R 或写 Git。
