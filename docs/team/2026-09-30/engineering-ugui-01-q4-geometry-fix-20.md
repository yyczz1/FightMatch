# UGUI-01 FIX20：六项几何失败的只读诊断与最小修正提案

2026-10-01 · `PREPARED_NOT_AUTHORIZED`；只准备方案，未运行Unity／测试、未改源码、未派agent／R。本文件不作代码审查或集成ACCEPT。
主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`；authority为中央本轮FIX19诊断指令及用户已批准的AGENTS §6。后续C owner `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，由中央激活／收件，Astra/xhigh。
按 diagnosing-bugs 使用真实失败作红色基线；本轮明确不获准新复现，故跳过新运行／动态插桩，不把静态推断写成已实测修复。按 uGUI 技能检查真实Prefab层级；执行步骤按 writing-for-agents 分开准备、激活和停止条件。

## 1. 固定输入与本轮复核

FIX19 PR source head `302a95bb895f87ac3e4d308f56f167f2e055e3d8`、tree `bb025773e423655090fdd5d1eac43d8f99a6c295`；中央报告GitHub Code Review已Completed／未发现重大问题，旧receipt的pending是历史时点，不冒充新head审查。共享本地HEAD仍是 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`，以实际WIP manifest绑定诊断，不能把两者混称。
证据根 `E19=TestArtifacts/FightMatch/UGUI-01/q4-correction-19/`；本轮解析XML确为190 finished、184 Passed、6 Failed；事件382行／5domains，无未配对Started，Unity自然exit2、261.086s，268.505s全部owned退出。无超时或编译错误；未重跑这些验证。

| E19输入 | bytes | SHA-256 |
| --- | ---: | --- |
| `tests.xml` | 165890 | `547d226053955136175305804da8913d921fb6999ee59bec5ee55fe2a94b71c2` |
| `final-receipt.json` | 8043 | `8bc0a764149a01bf02409c91b00fafead8cb640ab06504e133418c5931c26ce9` |
| `test-events.jsonl` | 145439 | `a48a88426eda868368212c682dd8f4ac895f11624ab42ce2ed8d093016a0312d` |
| `unity.log` | 169630 | `7870b623f4eff115654122c9284e2137d2797237250e51c98a07f9b4f33e9e3e` |
| `before.json` | 383068 | `8478e18d70df29c8d6c7c24417aea6480a03f0063aaad47b8101f1173cd0fe48` |

本轮逐项重算当前文件与before：source53/53、product29/29、resource37/37、callback delta2/2身份相等；字体仍沿FIX19 398characters/398glyphs/4atlases封存。没有改旧证据。

## 2. 已证事实：不是“宽度为零”这一项单因

1. `LocalizedTextBindingTests.LocaleChangeAndRebuiltPagePreserveHeadRequestAndPlaybackToken` 在第96行首次BeginRoute失败；`UguiSceneCompositionTests` 的 `OwnerEndedCommitsOnceDespiteBothPointerUpAndEndDrag`、`OwnerCanceledNormalizesBeforeReleaseAndNeverCommits`、`SecondFingerCanceledLeavesOwnerAndOwnerEndedStillCommitsOnce` 同样在首次BeginRoute失败；`FourCornerCentersAndHalfOpenEdgesUseTheActualBoardMapper` 在首次CellCenter失败。五者均 `Board geometry is unavailable`，尚未到达各自核心业务断言。
2. `BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical` 的XML精确指向第263行 **`mesh.vertexCount > 0` 实际为0**，不是rect.width断言。现有`Ready()`只核Rect尺寸／Canvas／Raycaster，不能证明board已绑定controller或有效face；前四项虽调用Ready仍不能排除绑定缺口。
3. [HostView](../../../Assets/Scripts/FightMatch/Host/FightMatchHostView.cs)第143–147行：隐藏battlePage内调用battleView.Bind后再SetActive(true)。链经 `PlayerBattleView.Bind`→`CandidateBattlePlaybackView.Bind`→`CandidateBoardInputView.Bind`；后者第41–43行仅在isActiveAndEnabled时BindNative，第191–194行依赖OnEnable补接 `board.Bind(controller)`。这些普通MonoBehaviour未声明ExecuteAlways；UguiHostRig仅启用Raycaster／输入模块的runInEditMode，没有给上述产品组件模拟生命周期。
4. [CandidateBoardElement](../../../Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs)第73–86行的Geometry同时检查 **board自己的controller/face宽高、Rect正有限尺寸、正等比无旋转矩阵**；任何一项不合格均返回false。第184–191行OnPopulateMesh先Clear再在Geometry失败时返回，故“空mesh＋CellCenter异常”可来自未接线，不专指布局宽0。
5. 同一次FIX19已有判别性正证：`CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit`（20.857s）和`UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite`（21.172s）均Passed；它们先EnterPlayMode，再使用**同一UguiHostRig、同一Prefab、Enter/Ready/BeginRoute**。本组六项没有EnterPlayMode，两个几何方法还是同步[Test]。这比类比FIX17更直接，但不是本组六项改后通过证据。

## 3. Prefab／Scene边界与可证伪假设

已核真实Prefab链：RuntimeCanvas→SafeAreaRoot→ScreenLayer→BattlePage→BattleContent→BattleScroll→Viewport→Content→BattleStatus→BoardFrame。BoardFrame是唯一CandidateBoardElement，序列化sizeDelta100×100、祖先localScale均1；LayoutElement min/preferredHeight420，父VerticalLayoutGroup控制子尺寸。内容由ScrollRect/ContentSizeFitter和LayoutGroup驱动，不能用未求解的序列化sizeDelta0推断运行时不可见。
Prefab SHA `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b`（1,060,568 B）；rig直接Instantiate此Prefab，不加载FightMatchDemo场景。场景确有RuntimeCanvas(fileID1013562322995895578)的localScale x/y/z=0与anchorMax=0覆盖；本轮没有激活场景，不能判定这些受Canvas驱动的保存值在运行时保持0，也不能把它们归因于这六项Prefab-rig失败。原生复开须检查实际激活后的正等比matrix、几何与画面；当前不授权修改场景。

| 排序／假设 | 预测与判别方式 | 当前边界 |
| --- | --- | --- |
| H1：EditMode inactive-bind缺少native OnEnable接线 | 仅切真实PlayMode，原产品/Prefab不变，正Rect/matrix下board真实映射恢复；六项越过原失败点 | 静态路径＋同次运行两个正证支持最强；本组六项board私有controller/nativeBound没有动态快照，未最终确证 |
| H2：真实布局/矩阵条件不成立 | PlayMode中controller/face有效，但Rect或matrix仍失败；须输出从Canvas至Board祖先几何定位首个非法节点 | 尚无运行时尺寸为0证据，不先调offset/尺寸、删检查或换布局 |
| H3：绑定/几何正常但mesh被裁剪或尚未重建 | CellCenter/映射正常、正Rect/matrix，但仅mesh.vertexCount0；记录canvasRenderer.cull、active及mesh状态 | 不能单独解释另五项异常；若出现则独立保留渲染失败，不能用空mesh跳过断言 |

## 4. 拟交C的最小白名单（中央激活后）

| 唯一可改Assets路径 | preimage bytes／SHA-256 | 允许差异 |
| --- | --- | --- |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 17493 / `130fdfaabb834ddcfeb1dda04af84ef199faa517b805745e08ecbcf54a301a52` | 仅上述5方法的运行模式/就绪探针、测试类内诊断helper和失败后PlayMode清理；**UguiHostRig、UguiReleaseObservation保持逐字不变** |
| `Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs` | 14238 / `af512297d8adf2a92b066db5b80f8cecefff09c566a18b2f89dbddae301da60f` | 仅LocaleChange方法同类变换及该测试类清理钩子；其余5测试正文不变 |
| `Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs` | 13090 / `35eff6f2426ce7df1c902f1084475d2dfc1f04d16af9df203f242d29112a3c39` | 仅固定输出叶`q4-correction-19`→`q4-correction-20`；可同步更正异常信息中的FIX18→FIX20，不改事件/路径保护机制 |

1. 六项均在创建rig前按已有空场景测试模式建立临时EmptyScene（不保存），然后EnterPlayMode；断言isPlaying=true。原using及原业务语句/断言/次序保留，结束先Dispose rig，再ExitPlayMode；两个[Test]改UnityTest/IEnumerator，**fullname/参数/测试实例数不变**。不把整个fixture无差别放进PlayMode，原诊断占位符等EditMode断言保留。
2. 原四项沿现有Ready；两项几何测试在Enter后补同一bounded Ready（现有三帧＋ForceUpdateCanvases），不增加任意sleep或重试循环。六项于首次实际输入/几何断言前作只读探针：isPlaying、页/board active、inputView.Controller与Session.Battle.Input引用一致、其face宽高、board Rect、localToWorldMatrix m00/m11/m01/m10、DiagnosticVisible和canvasRenderer.cull；失败消息包含这些值及祖先路径。可在UguiSceneCompositionTests类内用internal静态test helper供LocaleChange调用，禁止修改共享rig或新增产品accessor。
3. 探针仅观察／断言，仍让原CellCenter、真实GraphicRaycaster、事件模块和mesh测试决定结果；禁止反射写controller、手调OnEnable/BindNative、提前激活battlePage来改绑定次序、伪造board尺寸、补零尺寸fallback、关闭mask/cull、catch后冒充通过、跳过或减弱半开边界/mesh顶点/业务断言。
4. 两个测试类各加一个guarded UnityTearDown：仅当本次受控测试仍在PlayMode时ExitPlayMode，以保证断言失败也归还模式。using保留对象/订阅/旧EventSystem清理；teardown错误单列，不能覆盖首个原失败。此共享钩子影响两类全部12项，所以验证选择12而非只6。
5. 所有产品29、其它原source、Prefab/Scene/字体及全部.meta、asmdef、Packages/ProjectSettings、业务/存档、旧证据保持字节相等；新测试不能访问真实存档。**没有产品布局修正白名单**。若真实PlayMode仍不满足H1预测，C只封存具体H2/H3/接线证据，回中央再签最小产品范围，不在同回合改产品或资源。

## 5. 必要验证与证据复用

当前仅准备，先由C提交SOURCE_READY与三文件diff/哈希/静态范围；中央绑定新PR实际head再批准执行。代码审查仍仅GitHub，测试成功不替代新head审查；不派本地R。
新create-once证据根拟为 `E20=TestArtifacts/FightMatch/UGUI-01/q4-correction-20/`；只准 `activation.json`、`static/{inputs,delta-manifest,expected-fullnames,partition-proof,static-result}.json`、`validation-tools/runner.py`、`test-filter.txt`、`test-events.jsonl`、`tests.xml`、`unity.log`、`editor.stdout.log`、`editor.stderr.log`、`before.json`、`after.json`、`preflight.json`、`run.json`、`progress.jsonl`、`process-events.jsonl`、`quiescence.json`、`grouped-exact222-coverage.json`、`final-receipt.json`；前像E19只读，禁止写回callback旧叶。
runner仅从E19 `validation-tools/runner.py`（51,642 B，SHA `db1aee4cfbc730cda2bd24566ce0e003d9cf8dbbfd96fab23f44dcca7b0ac74c`）派生新根/候选身份、exact12 selector、计数与并集、360s cap。保留FIX19的首个编译错/非零退出即时锁定、owned身份守卫与有限收尾；离线伪状态检查后冻结，不在运行中修脚本。其它资源与容量上限继承[FIX18](engineering-ugui-01-q4-progress-delta-fix-18.md)和[FIX19](engineering-ugui-01-q4-compile-fix-19.md)。
唯一新增运行：从E19 XML按classname精确抽取 `FightMatch.Core.Tests.UguiSceneCompositionTests` 的6项＋`FightMatch.Core.Tests.LocalizedTextBindingTests` 的6项，生成anchored escaped fullname selector和Counter断言，**exact12一次**；不跑独立Compile、完整190、Host30或exact2。沿现有Unity2022.3.18f1/graphics-Metal/`-releaseCodeOptimization`/EditMode入口（六项内部切PlayMode），启动至root退出≤360s，编译包含在该次启动；超时／首败按既有60s温和观察＋身份守卫单次SIGTERM／30s终止观察封存，禁止同根重试。
通过要求：12/12 Passed、0fail/skip/inconclusive、每个原失败点被实际越过；事件fullnames/started/finished各exact12、RunStarted/RunFinished齐全、无未闭合、零编译错误、实际退出0和owned进程清空。不要冻结domain数量；多个Enter/Exit会真实重载。四角/半开边界、两分辨率/四向inset/顶点中心、取消先归一化、重复release仅提交1次、双指不抢占、locale重建的head/request/token/files断言全部保持并执行。
证据并集：FIX19的184绿项中两类6个绿项受teardown改动影响，以新12替代；**其余178原绿项复用**，连同FIX17 Host30与FIX15 exact2构成 `178+12+30+2=222` 的exact fullname Counter。只有产品29、资源37、共同fixture及未改测试身份全部相等才允许复用；旧190的6fail、FIX17超时和FIX18编译失败永远保留，不能改写为通过。
三文件差异若越界、真实PlayMode仍geometry失败、就绪/face/matrix不合格或mesh空：首败停止，回 `BLOCKED_PRODUCT_ACTIVATION_OR_GEOMETRY` 并附具体分支数据；单纯业务断言失败则如实单列，不再统一归因几何。禁止追加完整跑、延长deadline、补资源或削弱断言。
测试并集成立也不代表Demo接收：**原生保存/关闭/复开与实际场景Canvas矩阵/引用检查、布局视觉/设备和新APK门仍待后续单独执行**；本包不触发资源重生成、布局方案变化、API24、B/L或APK。中央收新head GitHub审查和主测试实际证据后决定下一包。

## 6. 本轮交付结论

已证的是共同inactive-bind/依赖native激活的路径、六项EditMode缺口及同版本PlayMode对照成功；“这六项仅切模式即可全绿”仍是待C验证的判别性修正，不是已接受结论。只读检查已确认本轮Assets与旧证据身份未变；主程唯一新增本文，回中央后结束，不等待/派发新执行。
