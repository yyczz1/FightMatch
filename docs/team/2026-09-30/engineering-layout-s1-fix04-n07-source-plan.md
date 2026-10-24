# LAYOUT FIX04 / N07 — 正常 Editor 更新后固定采样源码计划

2026-10-05 · `PREPARED_DESIGN_ONLY_NOT_SOURCE_ACTIVATED`。中央仅批准本设计，原几何/资产门不变。本轮只新增本文；源码/租约未激活，不创建S，不改产品/投影/旧证据，不运行Unity/C#/编译/测试/旧验证器/Git。

issuer/唯一收件：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10a15-67f9-71b0-9c37-e3846878ab61`；SYS owner：`01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a10a17-3354-7051-b7d0-c03d795f62e9`。主程wait/read收件。使用 writing-for-agents、diagnosing-bugs、unity:ui-ugui 固定边界、复用失败证据、保持uGUI职责。

## 1. 固定输入与证据界限

- B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`，162754B / SHA256 `1411149019d6a9c6713e37e9d80546b619acbda67342cbf9f17cea5063b9e34a`。
- T=`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`，36769B / `1d53ef04c21b6914a9aa9c9042cf98aba78c58e62b77f321edd11888e02acbdc`。
- Scene=`Assets/Scenes/FightMatchDemo.unity`，12900B / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`；Prefab=`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`，1322960B / `0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2`。
- E=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04`；N06/PExecution/receipt.json 32991B / `3ef75f34d330ead7f154a0ab4052bcb1468c30109b996f406b1043daf970bf51`；D03/PExecution/receipt.json 41052B / `fc3a1419f26d67ec6a79b881be590d16b9b9ff1367570ada2203c28c3906093d`。路径均相对 E。N06 在保存前覆盖判定失败，Save/复开/导出均0，没有数值快照；D03 数值只属于 D03。阶段不同步是待验证解释，不是已证根因；本方案固定观察失败即停止。
- 沿 [N06](engineering-layout-s1-fix04-n06-continuation.md)→[N04](engineering-layout-s1-fix04-n04-native.md)→[N03](engineering-layout-s1-fix04-n03-native.md) 冻结清单：15源/4native、1089共享/996投影/513GUID/51依赖/37资源/2自然meta/7保护；仅替换获准B/T项。不扩大历史扫描，N06/D03及更早根冻结。

## 2. API依据与有界弱保证

Unity 2022.3 [QueuePlayerLoopUpdate](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication.QueuePlayerLoopUpdate.html) 只排队，不提供完成回执；[EditorApplication.update](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication-update.html) 是非固定频率事件。[ForceUpdateCanvases 源码](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/UI/ScriptBindings/UICanvas.bindings.cs#L79) 调用两个 Canvas 回调，不证明内部原生 RectTransform 更新已完成。此处引用 2022.3 分支，不冒充安装的 18f1 原生实现证明。

选择唯一时序：每个边界先抓 immediate 快照→`Canvas.ForceUpdateCanvases()` 恰一次→抓 after-force 快照→设置 pending stage/token、ticks=0、deadline=`EditorApplication.timeSinceStartup+5`→`EditorApplication.QueuePlayerLoopUpdate()` 恰一次→返回 Editor。此后第一次 update 只计数并返回；第二次先消费 pending，再抓唯一 decisive 快照并调用该阶段 continuation。只保证观察到两次后续 update，不能称 player-loop/render/Canvas-driver completion fence；决定性快照原样过全部门才前进。

禁止等几何变绿、Queue/Force重试、delayCall链、组件禁启、localScale/anchor/pivot赋值、内部刷新反射或角点乘scaleFactor。实际角点按 `WorldToScreenPoint(null,p)`；[2022.3实现](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/UI/ScriptBindings/RectTransformUtility.cs#L60) 为p.xy，不补缩放。

## 3. B 的单 owner 状态机与三阶段

只让现有 `public static void RepairLayoutSceneOverrides()` 成为异步生命周期入口，签名不变。先通过原 LayoutGuard 和无 `-quit` 检查，再占有唯一 private static owner，准备字体/严格Prefab检查、开隔离Scene、固定6 IDs与保留覆盖；只订阅一个 `EditorApplication.update` handler，结束前只有此 owner 能消费 continuation。重复进入在任何第二次写入/订阅前拒绝并使当前 owner 失败，不能开启第二个任务。

私有状态固定为 `WaitingPreSave → WaitingPostSave → WaitingReopen → Finished`，另存 pending/token、arm 时 update serial、ticks、deadline、inCallback、failure latch、owned Scene、原始/保存后哈希、6 IDs、保留覆盖、targets。更新回调检查 owner、token、递增事件 serial、deadline、非重入及非 playing/compiling/updating；失败先锁存首错。新边界在旧 callback 内 armed 时不能把该次 callback 计为新边界事件；每个 token 只消费一次。无第二订阅、后台线程、全局“默认通过”状态或新 framework。

|阶段|固定操作与观察|过门后唯一后继|
|---|---|---|
|A / before-save|原分类器过滤五陈旧覆盖，SetPropertyModifications **1次**；`post-set`→Force1→`after-force`→Queue1→两次update→`after-two-editor-updates` decisive；完整验证根/几何/覆盖/ID|MarkSceneDirty、SaveScene **1次**；记录实际返回值、实际Scene后像和保存事实，进入B|
|B / post-save|**Set0**，名为 `post-save`，不能叫 post-set；Force1、Queue1、两次update、单 decisive；完整根/几何/覆盖/ID、非dirty、Prefab/其它保护|关闭当前Scene且不再保存；加载/卸载Prefab contents核targets；记录复开前Scene/Prefab字节，OpenScene只读复开，进入C|
|C / read-only-reopen|**Set0/Save0**，名为 `post-open`；Force1、Queue1、两次update、单 decisive；完整原 LayoutReopen 门和6 IDs|关闭自有Scene；核复开前后Scene/Prefab字节相同、保护完整；再写原三JSON，清理并Exit0|

一次完整成功路径共 Set1、Save1、Force3、Queue3、计数 update6、decisive3、全部快照9；失败路径只是相应前缀，不能为了补齐计数继续。每阶段都使用同一原始几何谓词：正且有限 local/world scale、rect/pixelRect宽高、scaleFactor及有限边界；四实际角点映射后八分量误差均≤0.5；活动 enabled root Overlay、本Canvas驱动、父子关系、精确 prefab/source/GUID/fileID 均保持。Prefab严格 scale1/min0/max1 不改。

五属性只限源 fileID1013562322995895578 的 `m_LocalScale.x/y/z`、`m_AnchorMax.x/y`，保留分类器原语义。其它覆盖 target/path/value/objectReference/顺序完整保留（跨复开以GUID/localFileID或GlobalObjectId表达持久身份，不用短命instanceID；null独立保留），包括pivot；每阶段对过滤后的固定序列比较，外层另核磁盘Scene只减少这五陈旧覆盖。141文本、3input、ViewIds/typedrefs/字体/许可证/输入模块/无UIDocument/缺引用门均不删。

A失败=Save0；Save已调用后（含返回false或B/C失败）保留调用/返回/磁盘后像，不写全程Save0、不回滚，后续保存/共享导出0。C全过才写三JSON；JSON/cleanup失败亦禁止导出，局部证据保留。

入口/回调/continuation异常统一进入幂等Finish：锁终态、清pending、退订、只关闭自有Scene且不保存、卸载自有Prefab contents、清preparingFont/owner；cleanup错误转失败，保留首错。隔离batch清理后 [EditorApplication.Exit](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication.Exit.html)(成功0/失败1)，Guard失败也显式非0；非batch误调用抛错，不退出用户Editor。边界5秒过期回调即失败；卡死/域重载丢回调由外层180秒监护失败收尾，非内部时限保证。禁止LockReloadAssemblies/跨reload续跑。

## 4. 单快照诊断与精确源码边界

private `LayoutSceneCanvasSnapshot` 只存公开可观察值：local/lossy scale、anchors/pivot/rect、position、localToWorldMatrix/父矩阵、pixelRect/scaleFactor、四world/screen corners、renderingDisplaySize及CanvasScaler公开配置、活动/driver/source/父级身份、dirty和五覆盖。一次Capture产生的数据同时供原谓词、各失败分量与日志；predicate不能再读Transform，日志不能晚抓新值充当先前快照。其它根/资源验证仍执行，但不能暗中再次Force或几何采样。

`CanvasScaler.m_PrevScaleFactor` 不可公开观察，**禁止新增反射读取**；诊断也不读取其它private缓存。日志固定 `[LAYOUT-N07-SAMPLE]`，≤9条、每条≤8KiB，包含run/stage/token/事件序号/快照类别、原值、8角差、finite/positive/coverage/override结果；额外阶段/终态≤8条且各≤2KiB，本次新增合计≤96KiB，并服从原单log≤8MiB。浮点用InvariantCulture/R字符串保留NaN/Infinity；只记录已有快照，结构缺失要明确失败，不补抓或截断必要字段冒充完整。

B只允许改 `LayoutGuard` 的sceneRepair分支（N06→N07及此mode禁 -quit）、`RepairLayoutSceneOverrides`、`LayoutVerifyRoot` 的私有可选快照参数及sceneInstance分派；原两参调用保持同步。允许新增私有 `LayoutVerifySceneCanvas(GameObject, LayoutSceneCanvasSnapshot)` 快照重载、`LayoutCaptureSceneCanvas`、`LayoutLogSceneCanvas`、`LayoutOverrideKeys`、`LayoutArmSceneRepair`、`LayoutUpdateSceneRepair`、`LayoutContinueSceneRepair`、`LayoutOpenRepairReopen`、`LayoutFinishSceneRepair`、数据类 `LayoutSceneCanvasSnapshot`/`LayoutSceneRepairRun`、枚举 `LayoutSceneRepairStage` 和唯一owner字段；不得增其它helper/type而不报告。

具体选择：N07入口直接沿原分类器过滤并Set，不调用会立即检查的旧 `LayoutRepairSceneCanvas`；N07专用 `LayoutOpenRepairReopen` 加C continuation逐条保留现 LayoutReopen 的准备/链接/targets/dirty/byte/保护逻辑，复用现有底层检查。不把旧 LayoutReopen 异步化：`MigrateLayoutResources`、`VerifyLayoutResources`、`LayoutReopen`、`LayoutRepairSceneCanvas`、旧单参 `LayoutVerifySceneCanvas` 方法体逐byte不改；两个其它入口仍同步、原有Force次数/退出语义不变。`LayoutUsableSceneCanvasGeometry`、`LayoutStaleCanvasOverride`、Prefab严格检查逐byte保持。

## 5. T 的 EditMode coroutine 改造

仅 `FightMatch.Core.Tests.UguiSceneCompositionTests.SavedDemoScenePreservesUsableCanvasWithoutStaleOverrides` 从 `[Test] void` 改为 `[UnityEngine.TestTools.UnityTest] System.Collections.IEnumerator`；名称/namespace/assembly/EditMode不变，不EnterPlayMode。安装的 `Library/PackageCache/com.unity.test-framework@1.1.33/Documentation~/reference-attribute-unitytest.md` 证明EditMode运行于update loop、yield null可跨迭代；同包 `UnityEngine.TestRunner/NUnitExtensions/Attributes/UnityTestAttribute.cs` 规定IEnumerator。无需新引用/asmdef/包。

原 Scene Additive打开、源身份/严格Prefab断言之后，几何读取之前，Force一次；订阅一次局部update委托、固定deadline5秒/ticks0后Queue一次，再 `while(!ready && time<deadline) yield return null`。委托第一事件只计数；第二事件先关闭pending并退订，**在该回调立即Capture一次**，捕获异常为failure、finally置ready；协程恢复后先核未超时/无异常，再只断言此快照，不能在协程稍后恢复时重新抓几何。等待条件只限事件/时间，绝不判断几何是否变绿；无Set/Save/组件赋值。

允许新增且仅新增T私有数据类 `SavedCanvasSample` 与 `CaptureSavedCanvasSample(Canvas)`，持有原断言使用的公开几何/活动/driver/parent和角点值；所有原正有限/边界/8个0.5角点、GUID/fileID、五覆盖断言语义不变，断言改读已捕获值。外层finally首先确保委托退订，然后保留原owned Scene关闭、originalActive恢复、Scene/Prefab字节比较，超时/捕获异常/断言失败/枚举器dispose也走此路径。现有UnityTearDown/enteredPlayMode不改。

原六测试及其rig/helper与两个数值/分类器负例（`SceneCanvasGeometryRejectsZeroNonFiniteAndIncompleteScreenCoverage`、`SceneCanvasOverrideClassifierRejectsAllFiveStaleFields`）逐byte保持；现有 `SceneCanvasCheck` 反射仅供原两负例，不扩到状态/等待/私有Unity缓存。测试fullname不变、测试数不增减：候选仍99（不是再加1）；未来发现记录必须证实该名唯一、UnityTest被发现且所选完整99无缺失，不以源码计数声称已发现/通过。

## 6. 将来 SOURCE 包（尚未激活）

唯一作者拟复用 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh；主程另行派发绑定actual turn和本文件最终SHA后才开工。唯一共享可写 B/T；新增仅 `E/N07/S/` 八叶：`before.json`、`FightMatchAndroidBuild.before.cs`、`UguiSceneCompositionTests.before.cs`、`FightMatchAndroidBuild.cs`、`UguiSceneCompositionTests.cs`、`source.patch`、`static.json`、`source-receipt.json`，总≤2MiB。前像/候选可机械复制，实际编辑apply_patch；禁止写I/P/PExecution/PExport、投影、Scene/Prefab/meta/资源/依赖/配置/旧证据。

B增删≤440行、净增≤300行/32768B；T增删≤140行、净增≤70行/8192B；合计增删≤580行、净增≤370行/40960B。禁止移动/压行/重排规避；超额报告主程，不能减门。无新public产品API/抽象/序列化格式。

SOURCE静态最多3次累计≤30秒：固定前像、精确两文件白名单、patch正反向重建、八叶/行/字节预算、保留方法与六旧/两负例逐byte、99名预期、单owner/退订/各路径调用数与失败终止的源码机械检查。技术性脚本错误仅窗口内修正并保留每次raw/计时，不运行旧42/长验证或假称静态证明事件实际发生。最终记录候选bytes/SHA/gitBlob、sourceS15After/native4完整兼容schema（仅B/T更新，其余13源和4native不变）、旧冻结清单身份、实际author/issuer/权源/每项验收、编译/原生/测试均NOT_RUN；SOURCE_READY只表示源码包可交接，不是产品ACCEPT。

## 7. 后置原生、测试及独立接收（均未激活）

源码封存、主程固定两后像后，绑定唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local` **新actual turn**和有效直接人类权源，禁止复用N06结束turn。C按另签执行包填写I/runner，锁定N06保护现状及真实投影前像（不是旧D03还原B）。当前不生成I/runner；仍须真实派发/工具批准。

将来runner差量仅：N07路径/新租约/封存B和T/新TMP与新增冻结集、两源**各sync一次**、要求两个程序集本次实际重编、argv删除单个 `-quit`、核三阶段日志和显式exit。仍 `-batchmode -force-metal -buildTarget StandaloneOSX -executeMethod FightMatch.Host.Editor.FightMatchAndroidBuild.RepairLayoutSceneOverrides`，其余固定Editor/cwd/UPM_CACHE_ROOT/managed flags保持；不加新驱动flag。Host.Editor和Core.Tests分别记录真实Csc输入及前后DLL身份，不能沿N04“只B且可复用旧Core DLL”条件。原生运行内compile不等于测试运行。

一次preflight≤120秒，唯一Editor≤180秒含compile/import，retry0；监护≤2秒/cache≤1GiB/派生≤4GiB/单log≤8MiB/free≥2GiB。首错保留，继承自然≤60秒→fresh owned PID/start/argv/后代核→每个仍活owned至多一次SIGTERM→≤30秒观察；无SIGKILL/外来信号/自动回滚。旧TMP精确缺席/存在规则继承N06且新旧实际清单需后置绑定，不泛化忽略丢失路径；新TMP仅一次mktemp、≤40UTF8/0700/无link/初始空/≤16MiB及512叶，保留。

未来N07沿N03的I一叶/P三叶/PExecution七叶/PExport两叶，加本S八叶共≤21叶/192MiB；三JSON原schema不改，phase数值留editor.log与执行receipt。只有真实exit0、两新编译证据、三阶段全门、三JSON、仅减五覆盖磁盘差量、只读复开、全部保护及owned闭包都通过，C才逐byte导出**唯一共享Scene**；Prefab/meta零导出。失败导出0，保留实际隔离保存与未完成门；不得另开D04/第二Editor/独立compile/discovery/99 tests/Play/device或安装、下载、Git。

主程wait/read收件；B/T＋导出Scene同一GitHub PR head。GitHub Code Review、99测试（优先dots、另签合同）、设备/视觉独立验收，均未完成。作者不自签ACCEPT，未运行/超时非通过；主程依证据给唯一集成判定。
