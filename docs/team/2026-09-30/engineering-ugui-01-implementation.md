# UGUI-01 实施包

2026-10-01 · 状态：AUTHORIZED（263-key 快照＋UGUI-COPY-BIND FIX-02 已签）

同一独立 R 与 R-TOOLS 已分别对冻结系统设计和 UGUI 文案绑定补充给出正式
`ACCEPT`；本文一次性绑定这些准确候选并由主程重签。C 可按本文恢复实施，
但不得扩大到后续 LOC/RES/API24/Android 图形/APK/设备包。

## 1. 身份、目标与固定输入

- 唯一交付与集成 owner：C
  01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local。
- C 是唯一可运行本机 Unity、保存 Scene/Prefab/TMP 资源并统一回传候选的
  执行者；主程、子执行会话、QA、R 均不得并发运行本项目 Unity。
- 固定代码起点：125b849be13fe2ebf5b1185f3cdb83d19c6327ef。现有其它
  角色改动不属于本包，不清理、不覆盖、不提交。
- 架构输入 SHA：
  309762cb3c4062aab9beadbccb5e34fbc9225d53fb9339f85021e46668513325。
- 当前系统设计输入 SHA（r4/COPY-IDENTITY-SYNC-05，包含已接收 FIX-03/04）：
  4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e。
  它只在已获 Focus R `ACCEPT` 的 FIX-04 SHA
  `1738be494e82fc6b8a2235b70d269e61f051e91fab3bb12ce009340ea6823d0b`
  上同步旧 216-key COPY identity；其基础行为与资源白名单继续有效。本次新增
  绑定补充只按下述独立 R 接受版本覆盖其精确 15 个缺口、9 项产品处置、两组
  public delta、快照输入、FocusLost cause 链和测试计数。
- 独立 R：01a0e404-e8ee-7310-8388-9260babd53f1 / local；turn
  01a0f39f-2fdd-7812-b05e-eaf900164973 completed；final
  msg_0d29bf8d726d8dc0016abd58aef16087d090d5273f0d6913c1；verdict ACCEPT。
  该 R 只新增接收 Focus seam；FIX-03 的 Host 可见性接受身份仍为 turn
  01a0f390-ef07-71b3-b632-9197fc1305e8、final
  msg_0d29bf8d726d8dc0016abd5521074487d0990b62413cd8b606。
- UGUI 唯一 COPY 输入固定为不可变快照
  `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv`，
  SHA-256 `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`，
  64,708 bytes、264 个物理行（表头＋263 数据记录）、9 列、263/263 唯一 key；
  只读 receipt `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/receipt.md`
  SHA-256 `980e8a942c06b1b6ebcec32d428135a656d5bd0ae089b087d82e3c4d5d3564b8`。
  COPY 独立 R turn `01a0f3db-8a86-7e63-b77f-2f121cedbd0b`、final
  `msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`，verdict `ACCEPT`。
  快照不是第二人工权威、运行时 catalog 或 Luban 产物；C 只读完整文件，不得
  改读当前 live CSV、旧 217 行 prefix 或截断内容。后续 live AMEND-04 的第 264
  个 key 属于 LOC，不改变本包输入。
- UGUI 文案绑定补充固定为
  `docs/team/2026-09-30/engineering-ugui-copy-binding-001.md`，SHA-256
  `a26b50a40828d4598423ad49a694b4361eca33391dbcfcabe1f7ffceb82528ac`，
  264 lines / 54,986 bytes；R-TOOLS thread
  `01a0f3e3-9663-7be2-84a8-ed23be32f0b9`，turn
  `01a0f406-3c86-71a3-bd46-291a87ead2e4`，final
  `msg_0543862903425801016abd7301b80c87d08844b314c81630bf`，verdict `ACCEPT`。
- Unity 固定 2022.3.18f1；不得升级 Editor、包、lock、依赖或设置。
- 目标是可独立编译、测试和回滚的 uGUI 结构迁移候选，不是可试玩 Demo、
  正常 APK、发行候选或正式本地化与 YooAsset 完成态。

## 2. 冻结行为

- 运行时 UI 使用 uGUI、Canvas、EventSystem、TMP；Core、Application、
  HostSession、存档、请求身份和 controller 业务真值不变。
- 场景只有一个 Host、Canvas 和 EventSystem。项目 InputModule 在
  base.Process 前归一化真实 TouchPhase.Canceled：Ended 精确提交一次，
  owner cancel 零提交，second-finger cancel 不清 owner。
- 棋盘以 RectTransform 左下局部坐标映射：
  x=floor((localX-xMin)/cell)，y=floor((localY-yMin)/cell)，不翻 Y；
  命中与绘制格心共享同一来源。
- 本包只实现 internal 本地化合同、内存测试 source 和 TMP binding。
  business severity 只决定 tone；只有 binding/infrastructure failure 打开
  diagnostic gate。正式表、locale 文件持久化、YooAsset、API24、APK 结构
  验证属于后续包。
- Prefab 玩家可见 TMP 初值逐字为【if you see this, it is a bug.】。
- 不引入 Unity Localization、Addressables、运行时 Luban 类型或临时下载器。

### 2.1 263-key 快照与绑定补充

- 快照相对旧 216-key 接受输入新增 47 个 key；其完整 SHA、64,708 bytes、263
  数据记录和参数合同必须由 `LocalizationContractTests` 对完整 bytes 校验后再
  UTF-8 解码。删除旧 `Take(ApprovedByteCount)`／prefix 接受；禁止 live fallback。
- C 严格实现绑定补充的 15 个核心 ID（N-D01..04、C-D01、N-B01..06、
  B-WIRE-01..04）和 9 项产品处置，不自行增删 key、参数、可见性或业务规则。
  稳定 ID、enum、OperationId、CommitId、seed、revision、FaceId、PairId、异常、
  field path 不得以 `ToString()`、硬编码或 raw fallback 进入玩家文本。
- gesture/action/reference 反馈使用 Controller 内唯一 retained visible slot 与
  monotonic revision；Render、100ms Refresh、Changed、播放收口和 LocaleChanged
  只重绑，不清除。下一次 accepted owner Down、新反馈替换、Unbind/Close/owner
  替换和上层 Dispose 按补充清槽；不得建队列、event bus、持久化或第二行。
- 本包仍只用内存 source 与上述只读快照验证双语和参数合同。C 不创建或修改
  `Config/FightMatch/Localization/`、Luban 工具／schema／生成物、语言偏好持久化
  或 YooAsset 资源；这些属于 LOC/RES 后续包。
- 原基线 207 个测试保持；本补充新增 15 个非参数化方法，目标精确 222。P1
  fixture 更新、P2 real-handler 顺序断言和 P3 生命周期断言均并入既定 fixture／
  方法，不另增计数，不得用 `Ignore`、`Inconclusive` 或 skip 代替验证。

### 2.2 Focus FIX-04 与最终 P2 cause 链

- `FightMatchPlayerHost` 新增生产用
  `internal void HandleApplicationFocus(bool focused)`；现有 private Unity message
  必须精确为无分支单行单次委托：
  `private void OnApplicationFocus(bool focused) => HandleApplicationFocus(focused);`。
  handler 在 `focused == false` 时精确调用一次
  `runtimeRoot?.PausePresentation(PointerCancellationCause.FocusLost)`，在 `true`
  时零调用、零状态变化。
- 保留所有现有 public/parameterless `Unbind/Close/Detach/PausePresentation/
  CancelPointer/CancelGesture`，固定默认 `Cancelled`。只增加补充列明的 internal
  cause overload，并沿 `HostView → PlayerBattleView → PlaybackView → InputView →
  BoardElement → Controller` 逐层传同一 cause；链中禁止回落无参入口。
- `FightMatchPlayerHost.OnDisable` 先 `PausePresentation(FocusLost)` 再
  `Unbind(FocusLost)`；HostView、PlayerBattleView、PlaybackView、InputView、
  BoardElement 的直接 `OnDisable` 也传 `FocusLost`。OnDestroy/Dispose、显式
  Close/Unbind/Detach/rebind、ShowFailure、geometry、pointer-exit、low-memory
  仍走 `Cancelled`。
- non-borrowed `CandidateBattlePlaybackView.Close(cause)` 必须在
  `Controller.Dispose() → Finish() → input.CancelGesture()` 的既有无参取消之前，
  先完成带 cause 的 active gesture 取消；只把 cause 留到末尾
  `inputView.Close(cause)` 不满足合同。不得改变 SkipToFinal、StopSchedule、
  ClearPlaybackOverride、token report、session pause、Controller detach 或零 submit
  的既有顺序和结果。
- `CandidateBoardInputTests` 的现有 B16B04 `blur` 分支和
  `CandidateBattlePlaybackPanelTests` 的现有 P27_03 `blur` 分支必须装配真实
  `FightMatchPlayerHost + FightMatchHostView/root`，通过 FIX-03 的 Host IVT 直接
  调用同一个 internal handler。前者证明 active gesture/capture 被清除且零提交；
  后者证明 latest-head／own-token 收口且旧 A1 不释放后续 A2；两者均证明
  `focused == true` 零动作。
- 移除两个原 test body 中的 `WAITING_FOR_FOCUS_TEST_CONTRACT` 显式失败；并在
  同两方法覆盖父级先、子级先、直接 HostView disable、直接 PlayerBattleView
  disable，断言唯一 FocusLost、零 Cancelled/duplicate/submit；普通 Close/Unbind/
  low-memory 只产生唯一 Cancelled。禁止
  reflection、`SendMessage`、test mirror、手动 `Update`、`SetActive(false)`／
  `OnDisable` 等价替代、public 或 test-only hook，也不得换成 Ignore、
  Inconclusive 或 skip。用例 fullname、参数化展开和 filter 不变；总数按 §6
  精确为 222。
- Q1 只证明 Unity message／handler 结构与禁用绕路；Q4 证明直接调用 handler
  后的生产取消／播放行为；Q7 才证明设备上的真实 OS→Unity focus 投递。三者
  不得互相替代，EditMode 不冒充合成了操作系统失焦事件。
- FIX-04 精确回滚只撤 handler、private message 委托及两个原 `blur` 分支；
  FIX-03 的 Host friend／asmdef 与其它测试保持不变。

## 3. 唯一文件白名单

### 3.1 可修改并保留现有 GUID/meta

- Assets/Scripts/FightMatch/Application/PlayerNavigationModels.cs
- Assets/Scripts/FightMatch/Application/PlayerNavigationSession.cs
- Assets/Scripts/FightMatch/Input/GestureModels.cs
- Assets/Scripts/FightMatch/Input/RouteGesture.cs
- Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef
- Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs
- Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs
- Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs
- Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs
- Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs
- Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs
- Assets/Scripts/FightMatch/Presentation/PlayerNavigationController.cs
- Assets/Scripts/FightMatch/Presentation/PlayerNavigationView.cs
- Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs
- Assets/Scripts/FightMatch/Presentation/PlayerPermanentDetailView.cs
- Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef
- Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
- Assets/Scripts/FightMatch/Host/FightMatchHostView.cs
- Assets/Scripts/FightMatch/Host/Editor/FightMatch.Host.Editor.asmdef
- Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
- Assets/Scenes/FightMatchDemo.unity
- Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef
- Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs
- Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
- Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs
- Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs
- Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs
- Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerBattleFlowTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs
- Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerNavigationSessionTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerNavigationPermanentFlowTests.cs
- Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs
- Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs
- Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs

FightMatchAndroidBuild.cs 只替换 PrepareResources 与场景结构检查；Min SDK 22
断言和旧字符串式 APK 检查分别留给后续包并保持可见失败门。

### 3.2 可新建；只有 C 可生成 meta 和 Unity 资产

- Assets/Scripts/FightMatch/Presentation/Localization.meta
- Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs
- Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs.meta
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs.meta
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs.meta
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs
- Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs.meta
- Assets/Scripts/FightMatch/Presentation/FightMatchStandaloneInputModule.cs
- Assets/Scripts/FightMatch/Presentation/FightMatchStandaloneInputModule.cs.meta
- Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs
- Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs.meta
- Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs
- Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs.meta
- Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs
- Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs.meta
- Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs
- Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs.meta
- Assets/UI/FightMatch/Runtime.meta
- Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab
- Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab.meta
- Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
- Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset.meta
- Assets/TextMesh Pro.meta
- Assets/TextMesh Pro/Resources.meta
- Assets/TextMesh Pro/Resources/TMP Settings.asset
- Assets/TextMesh Pro/Resources/TMP Settings.asset.meta
- Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt
- Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt.meta
- Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt
- Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt.meta
- Assets/TextMesh Pro/Shaders.meta
- Assets/TextMesh Pro/Shaders/TMP_SDF.shader
- Assets/TextMesh Pro/Shaders/TMP_SDF.shader.meta
- Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader
- Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader.meta
- Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc
- Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc.meta
- Assets/TextMesh Pro/Shaders/TMPro.cginc
- Assets/TextMesh Pro/Shaders/TMPro.cginc.meta
- Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs
- Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs.meta
- Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs
- Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs.meta
- Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs
- Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs.meta
- Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs
- Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs.meta

最终 TMP 官方 GUID、asset/meta hash 和复开步骤以上述系统设计 SHA 为唯一
权威。除上述路径外不允许新文件、删除、移动、重命名
或 meta 改写。旧 PanelSettings/TSS/TextCore 资产保留，只要求零新引用。

## 4. public、程序集与序列化权限

- 允许同名 View 从 VisualElement 改为序列化 MonoBehaviour binder，
  CandidateBoardElement 改为 MaskableGraphic；仓内 Host 与测试调用点一次
  迁完。除下述两组外不得改变 Core/Application/Input public API。
- 唯一 Application public delta：给 `PlayerNavigationConfirmation` 增加
  getter-only `CandidateApplicationKind? OriginalKind`、
  `CandidatePermanentKind? OriginalPermanentKind`、`bool OriginalClearsEquipment`；
  只由现有 internal constructor 接收，并由 `PlayerNavigationSession.View` 从同一
  retained intent 构造。不暴露 full quote／ID／canonical bytes，不进入 save、
  record、codec、Unity serialized field 或 persisted bytes。
- 唯一 Input public delta：精确增加八值 `GestureFeedbackKind` 与
  `RouteGesture.TakeFeedback() : GestureFeedbackKind?`；signal 原子取走并清除，
  Controller 立即 drain 到 internal retained visible slot。不把 feedback 加入
  `GestureView`，不新增第三组 public property/result/interface。
- 允许 FightMatchStandaloneInputModule 成为 public sealed 场景组件；其
  helper 和 true-touch cancel 入口保持 internal。
- LocaleId、ILocalizedTextSource、LocalizedTextResult 和
  LocalizationService 保持 internal。PresentationAssemblyInfo.cs 只允许
  FightMatch.Host、FightMatch.Core.Tests、FightMatch.Host.Tests 三个 friend；
  这三个 friend 精确不变，禁止第四 friend。
- HostAssemblyInfo.cs 只允许对 FightMatch.Core.Tests 与
  FightMatch.Host.Tests 的两个 fully-qualified `InternalsVisibleTo` attribute；
  不得有 using、类型或第三个 friend。LocalizationService 和 Host Bind 均不得
  改 public；禁止反射、SendMessage、字符串 GetType、测试镜像或运行时 test hook。
- 允许五个已列 asmdef 加入设计明确的既有 uGUI/TMP 程序集引用；此外
  FightMatch.Core.Tests.asmdef 只允许在现有 references 新增 FightMatch.Host，
  其它字段／引用保持；FightMatch.Host.Tests.asmdef 已有 Host 引用，不因
  FIX-03 再改引用。不得新增 asmdef、包、define、unsafe 或测试专用运行时入口。
- 场景 GUID、Host script GUID 和所有既有脚本 GUID 必须保留。

## 5. 分阶段并行所有权

C 先完成共享基础检查点后，才可创建必要执行子会话。所有会话共享同一
工作树，以下白名单互斥且不可转让。

### 5.1 C 独占共享基础、集成、Unity 和全部新文件

C 独占第 3.2 节全部新路径、五个 asmdef、Application/Input 的四个 public-seam
文件、CandidateBoardElement.cs、CandidateBoardInputController.cs、三个 Host
源文件、FightMatchAndroidBuild.cs、场景、四个新增 fixture、四个 Host 既有
测试文件、Prefab/TMP/官方资源及全部 Unity 运行。

C 先形成共享接口检查点：internal localization 合同、ViewId、SafeArea、
InputModule、board graphic/坐标 API、程序集可见性和 uGUI/TMP 引用。回执
列 public/internal 声明、序列化字段约定、changed paths 和 diff-check。
检查点前不得派发下面两个叶组。

### 5.2 可选子会话 B：Battle 叶视图

唯一可写：

- CandidateBoardInputView.cs
- CandidateBattlePlaybackView.cs
- PlayerBattleView.cs
- PlayerDefaultReferenceView.cs
- CandidateBoardInputTestData.cs
- CandidateBoardInputTests.cs
- CandidateBattlePlaybackTestData.cs
- CandidateBattlePlaybackPanelTests.cs
- CandidateBattlePlaybackTests.cs
- PlayerBattleTestFixture.cs
- PlayerBattlePresentationTests.cs
- PlayerDefaultReferenceTests.cs
- PlayerBattleFlowTests.cs

以上均指第 3.1 节列出的完整路径。子会话不运行 Unity，不写 meta、资源、
场景、asmdef、Git，不新增文件，不改 C 的共享 API；接口不足返回 BLOCKED。

### 5.3 可选子会话 N：Navigation 叶视图

唯一可写：

- PlayerNavigationController.cs
- PlayerNavigationView.cs
- PlayerNavigationRecoveryView.cs
- PlayerPermanentDetailView.cs
- PlayerNavigationTestFixture.cs
- PlayerNavigationPresentationTests.cs
- PlayerNavigationSessionTests.cs
- PlayerNavigationPermanentFlowTests.cs
- PlayerNavigationRecoveryTests.cs

约束同 5.2。NavigationBindings 从 Controller 移到 View，不得重写 controller
状态机或保存语义。

### 5.4 拆分结论

九个 View 不能从空接口直接并行：构造器转序列化 Bind/Unbind、Host 装配、
board 输入、localization 和 Prefab 字段高度耦合。允许的最小并行方式是
C 先冻结共享基础，再并行 Battle 与 Navigation 两个互斥叶组。若检查点
不能冻结，C 单独串行并记录原因。禁止继续按单个 View 细拆。

## 6. 执行与验证

1. C 重读仓库入口、验证规则、最终系统设计与本文，核对固定 hash、工作区、
   Unity 进程互斥和 Editor 版本；不符即 BLOCKED。
2. C 在项目外临时目录保存将修改的既有文件和 scene SHA；不得用 Git
   reset/checkout/stash，不碰其它角色文件。
3. C 完成共享基础静态检查点并决定是否启用两个叶组。
4. 子会话只写各自白名单并回传 changed files、acceptance mapping、
   diff-check 和未运行项；C 逐文件审查后接收或退回。
5. C 统一集成，再由唯一 Unity 实例按最终设计导入 TMP 必需资源、生成 Noto
   TMP、保存 Settings、Prefab、scene、SaveAssets、关闭重开。不得手写 Unity
   YAML/meta 冒充保存。
6. Q1 静态：每候选 1 次、上限 10m；核白名单、GUID、Presentation 精确三
   friend、Host 精确两 friend、Core.Tests 只增 Host 引用、Host.Tests 引用图
   不变、public API 精确只有 §4 两组、cause overload 全 internal、快照完整
   hash/length/263 rows 且无 live/prefix fallback、无反射／测试镜像绕路、
   placeholder、旧 runtime UI Toolkit 零新引用、Packages/ProjectSettings 零差异。
7. Q2 复开：1 次有效运行加最多 1 次定向修正、每次上限 5m；单
   Canvas/EventSystem、Prefab/scene/TMP Settings/Noto/SDF 标准与 Mobile
   fallback/includes/CJK 换行表全部有效。
8. Q3 compile：冻结候选 1 次、上限 5m；源码再变最多补 1 次。准确命令、
   日志和进程安全按 .agent/VALIDATION.md，不凭记忆自造。
9. Q4 目标测试：最终设计冻结同一 filter；基线 183 existing + 24 new = 207，
   本补充再新增 15 个非参数化测试，最终精确 **222**，
   failed/skipped/inconclusive 均为 0，上限 10m。失败先跑失败 fixture，
   修复后再跑完整 222；不得改 filter、数量或断言求绿。
10. C 回传源码/资源 SHA、Unity 版本、Q1-Q4 路径/耗时/退出码/XML 统计、
    工作区差异和未通过门。主程审实际 patch 后再申请独立 R code review。

Q5 全量、Android、APK 和设备由主测试基于稳定候选另排。未取得 LOC/RES
时显示本地化未就绪诊断是设计行为，不得塞入临时成品 copy。

Q4 的准确 testFilter 单参数为：

FightMatch.Core.Tests.CandidateBoardInputTests;
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests;
FightMatch.Core.Tests.CandidateBattlePlaybackTests;
FightMatch.Core.Tests.PlayerBattlePresentationTests;
FightMatch.Core.Tests.PlayerDefaultReferenceTests;
FightMatch.Core.Tests.PlayerBattleFlowTests;
FightMatch.Core.Tests.PlayerNavigationPresentationTests;
FightMatch.Core.Tests.PlayerNavigationSessionTests;
FightMatch.Core.Tests.PlayerNavigationPermanentFlowTests;
FightMatch.Core.Tests.PlayerNavigationRecoveryTests;
FightMatch.Host.Tests;
FightMatch.Core.Tests.UguiSceneCompositionTests;
FightMatch.Core.Tests.LocalizationContractTests;
FightMatch.Core.Tests.LocalizedTextBindingTests;
FightMatch.Core.Tests.LocalePolicyTests

执行时必须拼成一个无换行参数。结果固定写入
TestArtifacts/FightMatch/UGUI-01/候选SHA/editmode-target/，包含 tests.xml、
unity.log、run.json、源 SHA 和 XML 根统计。当前没有可信本机到云端的候选
传输路径，状态为 TRANSFER_PATH_NOT_AVAILABLE；因此 Q1-Q4 由 C 本机执行，
不得把云端准备或旧哈希结果写成已运行，也不得自动重复 222 或 4181 全量。

## 7. 可观察验收

- 单 Root Prefab、单 Host/Canvas/EventSystem，运行时玩家入口零 UI Toolkit
  资源引用。
- EventSystem 证明 Ended 单提交、owner cancel 零提交、second-finger cancel
  不干扰，且 cancel 在随后 Up/EndDrag 前。
- 命中与绘制共用无 Y 翻转公式，边界、双分辨率和非零 inset 满足合同。
- 三次 Bind/Unbind 不增长 listener，旧 generation/epoch/token 不回调新页；
  pause/focus/lowMemory/destroy 保持既有语义。
- active gesture 在父级先、子级先、直接禁用 HostView／PlayerBattleView 时只产生
  一次 FocusLost、零 Cancelled/duplicate/submit；显式 Close/Unbind/low-memory
  仍只产生一次 Cancelled。non-borrowed playback close 在 Controller.Dispose 的
  默认取消前先消费真实 cause。
- retained feedback 在 Changed／100ms refresh／LocaleChanged 后保持同一 semantic
  kind/revision，只在新 owner input、替换、Unbind/Close/Dispose 时按合同清除。
- TMP 初值全是诊断占位；内存 source 证明双语切换不动业务 token；有效
  severity 不盖 controller 入口；只有 binding failure 开 diagnostic gate。
- TMP Settings、Noto/material、标准 SDF、Mobile fallback、includes 和 CJK
  换行资源关闭重开有效且无 missing reference。
- Q1-Q4 通过、目标精确 222，实际 patch 获独立 R 唯一 ACCEPT。

## 8. 停止、回滚和回传

- 超白名单、超出 §4 两组的公共 API、序列化/依赖/设置扩大、GUID 冲突、第二
  EventSystem、Prefab 成品文案、未授权 Core/Application/Input/PlayerSave 改动：
  立即停止并返回 BLOCKED。
- Unity 保存失败保留日志和现场；用包前临时快照恢复相同 GUID，不重建
  GUID、不清玩家资料、不覆盖其它角色文件。
- FIX-03 回滚只删除本包新建的 HostAssemblyInfo.cs/.meta，并只撤去
  FightMatch.Core.Tests.asmdef 新增的 FightMatch.Host 引用；不得改
  PresentationAssemblyInfo 或 FightMatch.Host.Tests.asmdef，也不得保留
  public／反射临时绕路。
- 子会话接口冲突只回 C；任何文件只有一个作者。主程需要修包时发更小纠正
  白名单，不以整包重做代替诊断。
- C 回执逐条列 changed/new files、验收、证据、风险与 rollback；不得把
  代码已写称为 Demo 或发行完成。

## 9. 签发栏

- 冻结系统设计 SHA：4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e
- UGUI binding 补充 SHA：a26b50a40828d4598423ad49a694b4361eca33391dbcfcabe1f7ffceb82528ac
- 独立 R verdict：ACCEPT
- R-TOOLS turn / final message：01a0f406-3c86-71a3-bd46-291a87ead2e4 /
  msg_0543862903425801016abd7301b80c87d08844b314c81630bf
- 主程实施授权时间：2026-10-01
- C 收件 turn：PENDING
- 状态：AUTHORIZED
