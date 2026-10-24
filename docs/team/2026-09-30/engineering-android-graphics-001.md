# ANDROID-GFX-01-DESIGN · Android 图形 API 首候选

2026-10-01 · 工程架构设计 · 状态：`DESIGN_COMPLETE / READY_FOR_INDEPENDENT_REREVIEW / IMPLEMENTATION_NOT_RUN`

任务：`ANDROID-GFX-01-DESIGN`

最小纠正：`ANDROID-GFX-01-DESIGN-FIX-01`。首次独立 R-ANDROID：thread `01a0f40f-6bc6-72d2-b3dc-eb954ad0716a`、turn `01a0f40f-6d7c-7fb3-b6bd-272539f0f77b`、final `msg_06adc457b2dc221e016abd767977d487d0a85a98fc00b984be`，唯一 verdict `NEEDS_FIX`。其它合同已通过；FIX-01 只冻结 Q6／失败门／最终验收的逐次独立新进程恢复证据，不重开 GLES3-only、两文件白名单、API24/uGUI、report、Q7 或 Vulkan 状态。

唯一交付负责人：工程架构会话 `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `local`

返回：主程会话 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`

本文只冻结新 uGUI 候选的最小 Android graphics interface、未来实施白名单和验证门。本文没有运行 Unity、ADB、模拟器或真机，没有重建 APK，没有修改 `ProjectSettings`、源码、测试、旧证据或 Git；作者不能批准自己的设计，也没有授权 C。

## 1. 固定输入与边界

### 1.1 当前仓库输入

| 输入 | SHA-256 | 本文使用方式 |
| --- | --- | --- |
| `docs/team/2026-09-30/engineering-system-001.md` | `4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e` | 冻结 uGUI Q6/Q7、串行执行与设备证据边界 |
| `docs/team/2026-09-30/engineering-ugui-01-implementation.md` | `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623` | 当前主程重签且 C 正在执行的唯一冻结 uGUI 实施身份：435 lines／26488 bytes，包含 263-key snapshot、已接受 binding addendum/FIX-02 与目标 222；Q6/Q7 不由本包吸收。旧 SHA `a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a` 只属重签前历史，不得作为 post-uGUI 输入 |
| `docs/team/2026-09-30/engineering-ugui-copy-binding-001.md` | `a26b50a40828d4598423ad49a694b4361eca33391dbcfcabe1f7ffceb82528ac` | 同一 R-TOOLS 已最终 `ACCEPT`：thread `01a0f3e3-9663-7be2-84a8-ed23be32f0b9`、turn `01a0f406-3c86-71a3-bd46-291a87ead2e4`、final `msg_0543862903425801016abd7301b80c87d08844b314c81630bf`；候选身份 264 lines／54986 bytes。本文只消费该已接收上游身份，不重开其技术合同；这不授权 graphics、uGUI 或其它实现 |
| `docs/team/2026-09-30/engineering-android-api24-001.md` | `5ed0c1d7eaab4d8e6ecaf7815afc30deacc9e58b05e0d6f6af38ed9aa91402f9` | 独立 R 已 `ACCEPT`，但实现未运行；本文只串接，不合并其范围 |
| `ProjectSettings/ProjectSettings.asset` | `2ea02ff3b5acc6127f29d58049317eee4a52a84d9afde809eaf1efb30ed4ce1b` | 当前序列化事实；未来实施前须重新冻结真实起点 |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | `b0b0c0c020a5ad2402c11cfc0eb3228d0dfb2e7e9410ccadb85fb6d3dff8916d` | 当前 capture/restore/assert/report 事实；不是 uGUI/API24 后的未来合并 SHA |

当前 `ProjectSettings.asset` 的 Android graphics 序列化事实是：

```yaml
m_BuildTargetGraphicsAPIs:
- m_BuildTarget: AndroidPlayer
  m_APIs: 150000000b000000
  m_Automatic: 1
```

其中保存的有序列表对应 Vulkan→OpenGLES3，Auto 开启。`openGLRequireES31`、`openGLRequireES31AEP`、`openGLRequireES32` 均为 0，Android graphics jobs 为 0；本文首候选不改变这些字段。当前 `AndroidMinSdkVersion` 仍为 22，说明 API24 已批准设计尚未实施。

`FightMatchAndroidBuild.Settings.Capture()` 已同时读取 `GetUseDefaultGraphicsAPIs(Android)` 与 `GetGraphicsAPIs(Android)`；`PlatformMatches()` 也同时核对两者。但 `Settings.Apply()` 当前只恢复 Auto 开关，没有恢复捕获的 API 有序列表。未来一旦本包改变列表，这个不对称必须在同一实施包关闭，不能仅依靠最后的原始 YAML 覆盖掩盖 API 恢复遗漏。

### 1.2 旧 019 只作为复现输入

证据根：`TestArtifacts/FightMatch/BlueStacksPlay/20260930T180040Z/`

receipt：`receipt.json`，SHA-256 `3283018bc25b875f43926c875745207855b7837b344d647556b94e8ee60881e9`

证据准确证明：

- APK 是旧 019 UI Toolkit Development 包，SHA-256 `cfdc39d87a4f0152230594eb5dce9ed582e332fb91416c17d83157a749628094`，不是待验的新 uGUI 候选。
- 环境是 BlueStacks `5.21.790.7505`、Android 13/API 33、arm64；默认普通启动两次均进入 Vulkan，并在 `UnityGfxDeviceW` 线程发生 native `SIGSEGV`。栈顶经过 BlueStacks/Android 模拟层的 `/system/vendor/lib64/hw/vulkan.default.so` `vk_common_SetDebugUtilsObjectNameEXT` 与 Unity 的 `vk::DebugToolsUtils::SetVkDebugObjectName`。
- 同一 APK 仅在一次性 Intent extra `--es unity -force-gles30` 下通过 8 秒存活/前台检查；日志明确为 `Forcing GfxDevice: OpenGL ES 3`、OpenGL ES 3.2、`GLES: 3`、`threaded=1`，并到达中文“创建本地资料”画面。
- 没有玩法、保存、uGUI、API24、真机或独立产品接收证据；普通图标冷启仍失败。

证据边界必须保持：它证明“该 BlueStacks 版本/Android 模拟层在旧 019 的 Vulkan 路径稳定复现崩溃，GLES3 是同包的有效诊断对照”，不证明“所有 Android 真机 Vulkan 都不兼容”，也不证明新 uGUI 候选已经修复。Unity 2022.3 官方还明确不支持 Android emulator 作为受支持运行环境，因此 BlueStacks 是回归复现门，不是真机兼容性替代品。

## 2. Unity 2022.3 官方 interface 事实

- `PlayerSettings.GetGraphicsAPIs(BuildTarget)` 返回某平台的有序 graphics API 数组；`SetGraphicsAPIs` 设置数组，Unity 按传入顺序尝试并使用第一个可用项。
- `GetUseDefaultGraphicsAPIs`／`SetUseDefaultGraphicsAPIs` 读写 Auto Graphics API。Auto 默认选择 Unity 认为最佳的可用 API；Android 2022.3 Player Settings 文档说明 Auto 默认先尝试 Vulkan，设备不支持 Vulkan 时才回退 GLES3.2/3.1/3.0。
- Android 2022.3 同时支持 Vulkan 与 OpenGL ES 3.0/3.1/3.2；`GraphicsDeviceType.OpenGLES3` 是 Unity 的 OpenGL ES 3 interface 枚举。
- Auto 关闭且列表不含 GLES2 时，Unity 会在 Android manifest 中加入相应 GLES3 requirement，使不支持设备不出现在 Google Play；因此 GLES3-only 是明确的兼容边界，不是“所有 API24 设备无条件可运行”的声明。

官方资料：

- [PlayerSettings.GetGraphicsAPIs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerSettings.GetGraphicsAPIs.html)
- [PlayerSettings.SetGraphicsAPIs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerSettings.SetGraphicsAPIs.html)
- [PlayerSettings.GetUseDefaultGraphicsAPIs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerSettings.GetUseDefaultGraphicsAPIs.html)
- [PlayerSettings.SetUseDefaultGraphicsAPIs](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PlayerSettings.SetUseDefaultGraphicsAPIs.html)
- [GraphicsDeviceType](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Rendering.GraphicsDeviceType.html)
- [Android Player settings](https://docs.unity3d.com/2022.3/Documentation/Manual/class-PlayerSettingsAndroid.html)
- [Android requirements and compatibility](https://docs.unity3d.com/2022.3/Documentation/Manual/android-requirements-and-compatibility.html)

## 3. 三候选比较与唯一首选

| 候选 | 普通图标行为 | 对旧 019 证据的解释 | 兼容/验证代价 | 结论 |
| --- | --- | --- | --- | --- |
| Auto | Unity 2022.3 默认先尝试 Vulkan，不需要额外设置 | BlueStacks 已把 Vulkan 报为可用并完成设备选择，随后 native 崩溃；Auto 的“不支持才回退”条件没有发生 | 最宽但不能消除已复现启动故障 | `REJECT_AS_FIRST_CANDIDATE` |
| 手动 Vulkan→GLES3 | 按列表先 Vulkan，只有 Vulkan 被判不可用才试 GLES3 | BlueStacks 的 Vulkan 不是“不可用”，而是在已选择后的 worker thread SIGSEGV；进程死亡后不存在列表级回退机会。这是由官方顺序语义和旧日志共同得出的限定推论 | 保留 Vulkan，但不能满足普通图标启动目标 | `REJECT_AS_FIRST_CANDIDATE` |
| GLES3-only | 普通图标没有 Vulkan 可选项，直接建立 GLES3 device | 与同 APK 的 `-force-gles30` 诊断对照一致，但必须由新 APK 普通入口重新证明 | 放弃本候选的 Vulkan 路径；需 Play manifest、BlueStacks 回归和至少一台真机 | **`SELECTED_FIRST_CANDIDATE`** |

唯一首候选冻结为：

```text
Auto Graphics API = false
Graphics APIs = [GraphicsDeviceType.OpenGLES3]
Require ES3.1 = false
Require ES3.1+AEP = false
Require ES3.2 = false
```

选择范围是“新 uGUI 首候选”，不是永远禁止 Vulkan。该候选对 Vulkan 的验证状态必须写为 `NOT_SUPPORTED_BY_CANDIDATE`，不得写成 `PASS`、`FALLBACK_VERIFIED` 或“真机 Vulkan 不兼容”。以后重新引入 Vulkan 必须另立候选和证据矩阵。

该选择必须持久写入 `ProjectSettings.asset`，而不是只在旧 029 特殊构建入口临时覆盖后恢复：graphics API 是所有 Android 构建都应共享的候选兼容合同，若只藏在一个 build command 内，Editor/其它获准构建入口仍可能重新产出 Auto/Vulkan 包。两项持久设置加一个现有 Build 模块断言，是比运行时 selector 或额外 wrapper 更小、更可见的 interface。

## 4. 未来实施的深模块与唯一顺序

图形配置仍由现有 Android Build 模块所有。它的 interface 是：固定 PlayerSettings、构建前拒绝错误输入、生成自描述 build report、失败也恢复捕获状态；具体序列化与恢复步骤留在模块 implementation 内。不得新增第二个设置脚本、运行时 graphics selector、启动 Intent wrapper、设备型号 hardcode 或第三方 Adapter。

### 4.1 串行前置

1. 新 uGUI 实施候选先完成其 Q1～Q4 和专业审查，冻结实际 `FightMatchAndroidBuild.cs` 输入身份。
2. `ANDROID-API24-01` 再按已接受设计独立实施、验证和接收；它只拥有 min SDK 22→24 与 `minSdk` report 字段。
3. 本包最后基于实际 post-uGUI/post-API24 两文件身份实施。若两包尚未接收，状态为 `BLOCKED_ON_PREDECESSOR`，不得抢写共享文件。
4. 本包完成后由 Q6/Q7 整合候选共同消费；不能反向把 uGUI、API24 或 APK 结构差异塞入本包。

当前 029 build 入口的 stage/plan/output slots 已属于旧包。主程在实施前必须提供 §4.4 的新 create-once QA/normal/fail-after-capture 路径、逐次 reopen argv 与获准验证入口；若现有入口不能在本包两文件白名单内消费这些路径，本包在构建前 `BLOCKED`，不得修改旧 019、复用槽位或顺带重写验证器。

### 4.2 设置写入顺序

实施者先捕获真实起点：`GetUseDefaultGraphicsAPIs(Android)`、`GetGraphicsAPIs(Android)` 的枚举顺序、`ProjectSettings.asset` 原始 bytes/SHA。然后只通过 Unity 2022.3 API 执行：

1. `PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false)`。
2. `PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 })`。
3. 立即 read-back：Auto 必须 false；数组长度必须 1；唯一元素必须 `OpenGLES3`。
4. `AssetDatabase.SaveAssets()`，关闭 Unity；新进程重开后再次用相同 API read-back。
5. 比较 `ProjectSettings.asset`：只允许 §5.1 的 Android graphics 两项序列化差异，加上已独立实施并接收的 API24 差异；其它差异停止。

不得手写 YAML、调用一次性 `-force-gles30`、在运行时检查 BlueStacks 后切 API，或把命令行 override 留在桌面图标/launcher。

### 4.3 Capture/restore 顺序

`Settings.Capture()` 继续在任何临时 QA package/signing 变化前同时保存 `autoGraphics` 和有序 `graphics` 名称。`Settings.Apply()` 的 graphics 恢复精确改为：

1. 把捕获的名称严格解析为 `GraphicsDeviceType[]`；未知值立即失败，不忽略。
2. 读取当前 Auto 与数组；只有数组不等时才写列表。
3. 数组不等且当前 Auto=true 时，先 `SetUseDefaultGraphicsAPIs(Android, false)`。
4. 调用 `SetGraphicsAPIs(Android, capturedArray)` 恢复完整顺序。
5. 最后调用 `SetUseDefaultGraphicsAPIs(Android, capturedAuto)` 恢复捕获开关。
6. `PlatformMatches()` 必须同时证明 Auto 与数组逐项相等；既有原始 `settingsText` 回写及 bytes equality 仍是第二层证明，不替代 API read-back。

恢复失败、未知 enum、数组为空、顺序不同或原始 bytes 不同，构建退出失败并保留证据；不得继续下一包。

### 4.4 三条独立恢复证据与受控失败入口

QA 成功、normal 成功、capture 后受控失败是三个独立运行，不共享可写目录或 Unity 进程。每条都必须使用预授权的新 create-once evidence 目录，精确形状为：

```text
TestArtifacts/FightMatch/ANDROID-GFX-01/<candidate-sha>/q6/qa-success/<run-id>/
TestArtifacts/FightMatch/ANDROID-GFX-01/<candidate-sha>/q6/normal-success/<run-id>/
TestArtifacts/FightMatch/ANDROID-GFX-01/<candidate-sha>/q6/fail-after-capture/<run-id>/
```

每个 `<run-id>` 目录只服务一次原进程和紧随其后的一个独立 reopen 进程；目录已有任一结果文件即拒绝重跑。所有 receipt 以 create-new 语义写入，禁止覆盖。预授权 `run.json` 必须绑定 stage `ANDROID-GFX-01`、`candidate-sha`、实际 post-uGUI/post-API24 输入身份、`FightMatchAndroidBuild.cs` SHA、唯一 mode（`QaBuild`、`NormalBuild` 或 `GraphicsFailAfterCapture`）、该绝对目录及原进程／reopen 进程的完整 argv。任一 binding 不符即在 capture/mutation/build 前失败。

三条运行都必须产生 `capture-receipt.json`、`restore-receipt.json`、`reopen-receipt.json`、原进程日志和 reopen 进程日志；成功运行另有既定 `build-report.json` 与 APK identity，失败运行另有 `failure-receipt.json` 且不得有 APK/BuildReport。`capture-receipt.json` 及目录内只读的 captured byte copies 必须固定：candidate/input/source/run identity、Unity `2022.3.18f1`、mode、捕获的 Auto、完整有序 graphics list，以及 `ProjectSettings.asset`、`EditorBuildSettings.asset` 的原始 byte count、SHA-256 和逐字节副本。

原进程无论 build 成功或失败，都在同一个 `finally` 中执行生产 `Restore()`：按 §4.3 先恢复完整 ordered list、最后恢复 Auto，恢复其它临时设置／scene，调用 `AssetDatabase.SaveAssets()`，再完成 `PlatformMatches()`、两份 settings 原始 bytes/SHA 比对。随后以 create-new 写 `restore-receipt.json`，记录 restore 是否完成及 before/restored 两组 byte count/SHA，退出并彻底关闭该 Unity 进程。同一进程中的 read-back 只能进入 original-process receipt，不能充当 reopen 证据。

原进程关闭后，必须另启一个独立 Unity `2022.3.18f1` 进程，带精确 private arg `-fmGraphicsVerifyRestoreReceipt` 和 `-fmGraphicsEvidencePath <absolute-run-directory>`。该 arg 只由现有 `FightMatchAndroidBuild.cs` 的 private Editor-only 初始化分支消费：先复核同一个 `run.json` 和 source/candidate/input/argv binding，再读取 capture/restore receipts 与 captured byte copies，禁止写 PlayerSettings。它必须重新读取并逐项证明 captured Auto、完整有序 graphics list、`Settings.PlatformMatches()`、`ProjectSettings.asset` 原始 bytes/SHA、`EditorBuildSettings.asset` 原始 bytes/SHA 全部一致，然后 create-new 写 `reopen-receipt.json` 并退出。三种 mode 的 `restore-receipt.json` 都必须记录唯一 run nonce、原进程 PID 和 expected reopen path；`reopen-receipt.json` 必须记录相同 nonce、不同 PID，并反向绑定 capture/restore receipt SHA。缺字段、顺序不同、SHA/bytes 不同、回链不符、原进程未关闭、Unity 版本不同或 reopen 与原进程 PID 相同都失败；同进程检查不得替代。

唯一受控失败 flag 精确为 `-fmGraphicsFailAfterCapture`，只能出现在 mode=`GraphicsFailAfterCapture` 且 `run.json` 已绑定该 flag 与新 `fail-after-capture/<run-id>` 目录的既有获准 normal build 入口。`BuildDemo` 在 `Settings.Capture()`、captured byte copies 和 `capture-receipt.json` 全部 create-new 成功后，且在 `Sign()`、输出目录创建和 `BuildPipeline.BuildPlayer` 之前进入 private Editor-only 分支。分支用相同 Unity PlayerSettings API 暂时设置已知且与候选 capture 不同的探针状态：先 Auto=false，ordered list=`[Vulkan, OpenGLES3]`，再 Auto=true，`SaveAssets()` 后 read-back 必须精确为该状态；随后只抛稳定诊断码 `FMGFX_FAIL_AFTER_CAPTURE`。它不得调用 BuildPipeline、签名、创建 APK、暴露 public/runtime/test hook 或改变候选持久设置。

受控失败的 nested `finally` 必须即使在 BuildReport 产生前也 create-new 写 `failure-receipt.json` 和 `restore-receipt.json`。两者精确绑定 candidate/input/source/run identity、mode、failure injection point=`AfterSettingsCaptureBeforeBuildPipeline`、诊断码、captured Auto/list、probe Auto/list、`buildPipelineCalled=false`、`apkCreated=false`、两份 settings 的 before/restored bytes/SHA、原进程 restore 结果、随机 run nonce 与唯一 expected reopen receipt path；`restore-receipt.json` 明确 `reopenRequired=true`，不得由原进程预填 reopen 成功。第二进程的 `reopen-receipt.json` 必须反向绑定同一 nonce 以及 capture/failure/restore receipts 的 SHA-256，并记录新进程 PID、Unity 版本和全部 reopen comparisons；这组三个相互绑定的 receipts 才共同证明“原进程 restore＋新进程 reopen”。任一必填项缺失、不一致、回链 SHA 不符或目录已有结果都是失败，且不得覆盖 QA/normal/旧证据。

上述 private arg 分支、receipt 数据类型和 helper 只能位于现有 `FightMatchAndroidBuild.cs`，不得增加第三文件或 public/runtime/test seam。若在 §5 两文件内不能同时保证 create-once binding、finally receipt、受控 mutation/restore 和独立 reopen，实施状态立即为 `BLOCKED`，不得扩大白名单。

## 5. 精确草案白名单

### 5.1 产品/Editor 文件

未来实施只允许修改：

```text
ProjectSettings/ProjectSettings.asset
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

`ProjectSettings.asset` 的本包自有差异只允许 Android graphics entry 从 Auto + Vulkan/GLES3 变为 manual + GLES3-only。以当前序列化形态表达，预期为：

```diff
 - m_APIs: 150000000b000000
 - m_Automatic: 1
 + m_APIs: 0b000000
 + m_Automatic: 0
```

实际必须由 Unity API 保存得到；若 Unity 版本写出的 canonical bytes 不同，保留现场并重新审查，不能手工逼成上述文本。`openGLRequireES31*`、`openGLRequireES32`、graphics jobs、multithreaded rendering、Vulkan tuning、颜色空间、纹理压缩及其它 PlayerSettings 均不变。API24 的 `AndroidMinSdkVersion: 24` 是前置包所有，不计入本包自有差异。

`FightMatchAndroidBuild.cs` 只允许：

- 引用 `UnityEngine.Rendering.GraphicsDeviceType`。
- 在 `CheckProduction()` 增加单一 graphics 断言：Auto=false 且 `GetGraphicsAPIs(Android)` 精确 `SequenceEqual([OpenGLES3])`；错误文本固定为 `Android graphics API`。
- 在 `BuildResultRecord` 增加 `bool graphicsAutomatic` 与 `string[] graphicsApis`，`WriteReport` 从 PlayerSettings 现场填值。QA/normal 预期均为 `false` 与 `['OpenGLES3']`。
- 按 §4.3 补齐 `Settings.Apply()` 的列表恢复；既有 capture、`PlatformMatches()` 与 raw bytes restore 保留。
- 在同一 Editor 类内增加且只增加 §4.4 的 private create-once receipt/helper、`-fmGraphicsFailAfterCapture` 分支和 `-fmGraphicsVerifyRestoreReceipt` 初始化分支；它们必须复用既有获准 QA/normal build interface、`Settings.Capture/Apply/PlatformMatches` 和 command-line run binding。不得增加 public method、第二个 build class、runtime selector 或测试 hook。
- QA、normal、failure 三条原进程的同一 production `finally` 都必须在 restore＋`SaveAssets`＋raw bytes equality 后写 original-process receipt；独立 reopen 分支只读复核并单独写 reopen receipt。失败分支的 probe 固定为 Auto=true＋`[Vulkan, OpenGLES3]`，诊断码固定为 `FMGFX_FAIL_AFTER_CAPTURE`，且在 `Sign/BuildPipeline/output create` 前终止。
- 消费已独立实现的 `minSdk` report 字段，但不新增、改名或重写其判断。

BuildReport 字段证明“构建时配置”，不证明设备实际选择；实际 renderer 只能由新 APK 的设备日志证明。

### 5.2 测试文件

测试源码白名单精确为空：不新增、不修改任何 `Assets/Tests/**`、asmdef 或 meta。现有 Build 模块是 Editor build seam，验证通过静态差异、Unity save/reopen、真实 QA/normal BuildReport、恢复 bytes 和设备日志完成；不得为测试暴露 private/public hook、reflection seam 或复制 Settings implementation。

### 5.3 明确禁止

禁止修改 Tools、Packages、其它 ProjectSettings、runtime/Presentation/Application/Core、场景、Prefab、资源、manifest 模板、Gradle、旧 APK/证据或 Git。禁止升级 Unity、添加依赖、改 API24、target SDK、ABI、IL2CPP、签名、包名、版本、BuildOptions 或 uGUI 行为。

## 6. 验证矩阵与新证据

结果根草案：

```text
TestArtifacts/FightMatch/ANDROID-GFX-01/<candidate-sha>/
  static/
  reopen/
  compile/
  q6/
    qa-success/<run-id>/
    normal-success/<run-id>/
    fail-after-capture/<run-id>/
  bluestacks/
  real-device/
  review/
```

`candidate-sha` 必须绑定 post-uGUI/post-API24 的 `ProjectSettings.asset`、`FightMatchAndroidBuild.cs` 和新 uGUI 源/资源身份；旧 019 SHA 不得成为候选身份。Q6 三条运行各用 §4.4 的独立 create-once `<run-id>`，不覆盖彼此或旧证据。

### Q1 — 静态与保存复开

- changed paths 精确为 §5.1 两文件；测试改动 0，额外 ProjectSettings 字节 0。
- Unity API 在保存前和独立新进程复开后均返回 Auto=false、数组精确 `[OpenGLES3]`。
- 序列化 Android graphics entry 与 API read-back 一致；ES3.1/AEP/3.2 requirements 均 false。
- `CheckProduction()`、BuildResultRecord、capture/restore 与 `PlatformMatches()` 逐项符合 §5；无 `-force-gles30`、设备型号、BlueStacks 或 Vulkan blacklist hardcode。
- API24 前置身份准确、min SDK=24；本包不产生第二次 API24 diff。

### Q2 — Compile

在同一冻结候选上使用获准的 Unity `2022.3.18f1` 入口 clean compile；exit 0，无 C# compile error。项目已被其它 Unity 进程占用时不得启动。源码或设置再变会使该证据失效。

### Q6 — QA/normal Android build 与报告

沿冻结 uGUI Q6，QA 成功、normal 成功和 capture 后受控失败各用 §4.4 的一个新 create-once 路径；不得复用 018/019，也不得把一个 Unity 进程或 reopen receipt 复用于两条。QA/normal 每包同时满足：

- Build result `Succeeded`，APK path/bytes/SHA 与实际文件一致。
- `build-report.json.graphicsAutomatic == false`。
- `build-report.json.graphicsApis` 精确一个值 `OpenGLES3`，顺序不可放宽。
- API24 包拥有的 `minSdk == 24` 与最终 manifest min-sdk 24 同时成立；graphics 包只消费该事实。
- 最终 merged manifest 声明 GLES3 requirement，且不存在由本包新增的 Vulkan requirement；保留原始查询输出。
- QA/normal 原进程各自在 `finally` 中按 list→Auto 恢复、`SaveAssets`，并证明 `PlatformMatches()` 与 raw `ProjectSettings.asset`/`EditorBuildSettings.asset` bytes/SHA 恢复；原 Unity 完全关闭后，各自独立启动 Unity `2022.3.18f1` 复读同一 captured Auto/list、`PlatformMatches()` 与两文件原始 bytes/SHA，`restore-receipt.json` 和 `reopen-receipt.json` 均为 ACCEPT。
- 报告不得声称实际设备选择 OpenGLES3；那属于 Q7 日志。

capture 后受控失败条必须同时满足：

- mode/run binding/create-once path 精确为 `GraphicsFailAfterCapture` 与 `q6/fail-after-capture/<run-id>`，唯一 flag 是 `-fmGraphicsFailAfterCapture`；capture receipt 在 mutation 前已落盘。
- probe 现场 read-back 精确为 Auto=true、ordered `[Vulkan, OpenGLES3]`，随后稳定失败码为 `FMGFX_FAIL_AFTER_CAPTURE`；`BuildPipeline`、签名、APK/output creation 均未发生。
- 即使没有 BuildReport，`failure-receipt.json` 与 `restore-receipt.json` 仍完整包含 §4.4 的身份、注入点、captured/probe、before/restored bytes/SHA 和 original-process restore 结果。
- 原 Unity 完全关闭后再启动独立 Unity `2022.3.18f1`；`reopen-receipt.json` 再次证明 captured Auto、完整有序 list、`PlatformMatches()` 及两份 settings 原始 bytes/SHA 全部一致。

三条中任一 receipt 缺失、字段缺失、same-process 冒充 reopen、PID/Unity 版本不符、Auto/list 顺序不同、`PlatformMatches()` false、任一 settings bytes/SHA 不同，Q6 即失败且不得继续 Q7。

### Q7-A — BlueStacks 回归门

使用 BlueStacks `5.21.790.7505` 时先记录实际版本、Android/API/ABI、安装 APK SHA。所有验收启动都从普通 launcher 图标进入；ADB 只做前后状态、日志和截图采集，启动命令不得包含 `--es unity`、`-force-gles30`、`-force-vulkan` 或其它 graphics override。

1. **Cold**：force-stop 后由普通图标启动，至少 30 秒进程存活且 FightMatch activity 保持 resumed；到达新 uGUI 初始页。
2. **Warm**：Home 退后台后由普通图标恢复，原进程保持或按 Android 正常生命周期恢复，无 native crash、黑屏或退回 Game Center。
3. **Relaunch**：完整关闭/force-stop 后再次由普通图标启动，重复 Cold 标准。
4. 每次先清空应用相关 logcat，保留完整日志、`pidof`、activity state、exit-info、截图/录屏与时间。Unity 日志必须显示建立 OpenGL ES 3 device；不得出现 Vulkan device selection/init，也不得出现 graphics override command line。

BlueStacks 失败是回归失败；通过只关闭旧模拟层复现门，因为 Unity 2022.3 官方不支持 Android emulator，不能代替真机。

### Q7-B — 至少一台实际 Android 真机

首选现有 iQOO；若不可用，可用另一台实际 Android，但必须记录品牌/型号、Android/API、ABI、GPU/renderer、驱动信息和 APK SHA，不能把模拟器冒充真机。用相同 normal APK、普通 launcher 图标和无 override 条件执行 Cold、Warm、Relaunch 三组，并证明：

- 进程/Activity 稳定，无 native crash/ANR；新 uGUI 初始页可见并完成 Q7 原有触摸、安全区、双语及真实前后台/focus 检查。
- Unity 日志实际选择 OpenGL ES 3，记录 renderer/version；不得出现 Vulkan init 或 graphics override。
- 关闭重开不破坏本地资料/存档语义；graphics 包不创建、清除或迁移玩家数据。

没有真机时结果只能是 `DEVICE_PENDING`，不能以 BlueStacks `PASS` 接收本候选。若真机不支持 GLES3 或 GLES3 初始化失败，记录真实兼容边界并返回 `NEEDS_FIX/BLOCKED`；不得临时恢复 Auto/Vulkan 求绿。

### Q7-C — Vulkan 状态

因为首候选只包含 OpenGLES3，Vulkan 在所有 Q6/Q7 回执中统一记为：

```text
Vulkan: NOT_SUPPORTED_BY_CANDIDATE
```

不得运行 `-force-vulkan` 冒充候选行为，也不得把“没有跑 Vulkan”写成通过。未来 Vulkan 候选至少要证明真实 Vulkan 选择、目标真机冷暖启，以及在一台确实报告 Vulkan unsupported 的设备上发生列表级 GLES3 fallback；BlueStacks 当前的“报告支持后崩溃”不属于 fallback。

## 7. 与 uGUI/API24 的整合但不吸收

- uGUI Q6 仍拥有新 QA/normal 候选的 UI、资源和 APK identity；graphics 只增加配置/report/renderer 条件。
- uGUI Q7 仍拥有双语、触摸、多指、安全区、冷启动和真实 OS→Unity focus 投递；graphics 只增加普通图标入口、实际 renderer 与无 override 条件。
- API24 包仍独自拥有 `AndroidMinSdkVersion: 24`、`CheckProduction` 的命名枚举断言和 `minSdk` report 字段；graphics 不修改这些差异。
- 新候选可以在同一 Q6/Q7 运行里同时提供三包的证据，但回执必须按 owner 分栏；任一门失败不能把其它门的通过外推为整体通过。
- 旧 019 只证明复现输入。新 uGUI、API24、GLES3-only 的候选身份、构建报告、APK 和设备证据全部必须新建。

## 8. 失败停止与回滚

立即停止条件：

- 前置 uGUI/API24 尚未稳定接收，或共享文件实际身份与主程签发不符。
- 项目已由另一 Unity 进程占用；Unity 版本不是 2022.3.18f1。
- 出现第三个 changed file、额外 ProjectSettings 字节、测试/meta/资源/Tools 差异。
- API read-back、save/reopen、build report、manifest、restore 或 raw bytes 任一不一致。
- QA/normal/failure 任一没有独立 create-once 目录、original `finally` receipt 或关闭原 Unity 后的独立 reopen receipt；reopen 与原进程相同、未复核完整 ordered list/Auto/`PlatformMatches()`/两份 settings bytes+SHA，均立即停止。
- `-fmGraphicsFailAfterCapture` 未由 mode/run/argv/path 精确授权，capture receipt 未先完成，probe 不是固定 Auto=true＋`[Vulkan, OpenGLES3]`，受控失败调用了 Sign/BuildPipeline/创建 APK，或诊断码不是 `FMGFX_FAIL_AFTER_CAPTURE`。
- normal APK 仍需 Intent override、普通图标进入 Vulkan、BlueStacks 仍 native crash，或真机 GLES3 失败。
- 新输出路径未获授权、已存在、会覆盖旧 018/019，或验证入口仍只接受旧 001..020 槽位。

回滚单位是 §5.1 两文件相对真实实施起点的最小补丁：

1. 用捕获的有序数组与 Auto 值按 §4.3 顺序通过 Unity API 恢复 PlayerSettings，保存、关闭、复开并核原始 bytes/SHA。
2. 只撤销 `FightMatchAndroidBuild.cs` 的 graphics assert/report/list-restore 差异，保留已接收的 uGUI/API24 修改。
3. 不使用 `git reset/checkout`，不删旧证据，不改写失败目录，不回滚玩家数据。

需要 manifest 模板、runtime selector、第三文件、Unity 升级、Vulkan blacklist 或产品兼容范围变化时，返回 `BLOCKED + proposal`，不得由 C 自行扩大。

### 8.1 当前明确未验证

- post-uGUI/post-API24 两文件的真实实施起点与新 create-once 构建入口尚未冻结。
- API24 尚未实施；当前项目仍为 min SDK 22。
- Unity API 保存后 GLES3-only 的实际 canonical YAML、merged manifest 与 Google Play 设备过滤尚未产生。
- 新 uGUI QA/normal APK、build-report graphics 字段、三条 original restore receipts、三条独立 reopen receipts、受控 capture-after-failure receipt 和 APK identity 尚未产生。
- 新候选普通图标的 BlueStacks Cold/Warm/Relaunch、实际 renderer 与 30 秒稳定性尚未运行。
- iQOO 或其它实际 Android 真机尚未验证；不能据旧 019 或 BlueStacks 预测真机结果。
- Vulkan 对本候选是 `NOT_SUPPORTED_BY_CANDIDATE`，没有性能、视觉或兼容数据。

## 9. 独立 R 与可观察验收

FIX-01 完成后须回首次 R-ANDROID 的同一 thread 复审；作者不自行发 R。复审只补查：三条 create-once 目录、original finally receipt、逐次独立 Unity reopen、失败 probe/binding/diagnostic/no-BuildPipeline 及最终验收 4a～4c；首次 R 已通过的证据外推边界、三候选选择、两文件白名单、API24/uGUI、report、Q7 和 Vulkan 状态不重开。R 仍只给一个 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。

未来实现只有全部满足才可接收：

1. ProjectSettings 保存/复开为 manual GLES3-only，额外序列化差异为零。
2. Build 前准确拒绝 Auto、Vulkan 或多项列表；QA/normal 报告都记录 false + `[OpenGLES3]`。
3. API24 仍为 24，uGUI/API24/graphics 各自 owner 与证据不混淆。
4a. QA 新 APK 构建成功；其 create-once 原进程 `finally` restore receipt 与关闭该 Unity 后的独立 `2022.3.18f1` reopen receipt 都对 captured Auto、完整 ordered list、`PlatformMatches()`、`ProjectSettings.asset` 与 `EditorBuildSettings.asset` 原始 bytes/SHA 给出 ACCEPT。
4b. normal 新 APK 构建成功；在不同 create-once 目录重复 4a 的完整 original restore＋独立 reopen ACCEPT，不复用 QA 进程或 receipt。
4c. capture 后受控失败在第三个 create-once 目录以 `FMGFX_FAIL_AFTER_CAPTURE` 结束且未调用 BuildPipeline/产出 APK；failure/restore receipts 完整，并在原 Unity 关闭后由独立 `2022.3.18f1` reopen 对同一 captured Auto/list、`PlatformMatches()` 和两份 settings 原始 bytes/SHA 给出 ACCEPT。
5. normal APK 从普通图标、无任何 Intent override，通过 BlueStacks Cold/Warm/Relaunch 且日志实际为 GLES3。
6. 同一 normal APK 在至少一台实际 Android 真机通过普通图标 Cold/Warm/Relaunch，日志实际为 GLES3。
7. Vulkan 明确标 `NOT_SUPPORTED_BY_CANDIDATE`；没有伪造 Vulkan/fallback 验证。
8. 新 uGUI 设备门其它条件仍按冻结 Q7 完成；旧 019 不冒充新证据。
9. 独立 R 对实际 patch/证据给唯一 `ACCEPT`。

## 10. 本设计自检与返回状态

- 本次 FIX-01 只修改本文；其它文件全只读。
- 没有运行 Unity、ADB、设备、compile、tests 或 build，没有改 settings/source/evidence/Git。
- 首候选唯一为 GLES3-only；Auto 与 Vulkan→GLES3 有明确拒绝理由。
- BlueStacks 结论限定到模拟器 Vulkan implementation/layer；没有外推真机 Vulkan。
- 普通图标是唯一设备验收入口；一次性 `-force-gles30` 只保留为旧诊断事实，不是长期修复。
- `ProjectSettings.asset` 与 `FightMatchAndroidBuild.cs` 两文件草案白名单明确；测试白名单为空。
- 最小 interface 集中在现有 Android Build 模块，没有新增浅 wrapper 或重复设置 Adapter。
- FIX-01 把 QA、normal、capture 后失败冻结为三个 create-once 运行；每条都要求 original finally restore 后关闭 Unity，再由独立 `2022.3.18f1` 复读 Auto、ordered list、`PlatformMatches()` 和两份 settings 原始 bytes/SHA。失败 probe 是同一 Editor Build 模块内唯一 private flag，不新增第三文件或 public/runtime/test seam。

唯一状态候选：`READY_FOR_INDEPENDENT_REREVIEW`。本文不授权 C、Unity 或任何实现。
