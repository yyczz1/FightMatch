# LAYOUT-S1-ARCH-001：完整布局源码的独立激活边界

2026-10-02（Asia/Shanghai）· 薄架构判定；不是代码审查、实施回执或完整Demo接收。
ARCH `01a0f2e6-1ac3-7d10-8855-54192b9489d4/local`，实际turn `01a0fa4a-6790-7731-8159-e196f3a9b7a1`；唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据主程转达的用户2026-10-02继续首Demo及明显临时美术占位授权；主程接回后再派SYS/C。

## 1. 结论与固定输入

- **可激活完整LAYOUT源码，不必等LOC-B-PREF/LOC-A；交付切片是“完整布局源码＋适配fixture”，不是只改半个Battle/Recovery。** 原生资源、编译、测试及真实Host分别过门；源码交付时明确“旧Prefab尚未迁移、不可作为可试玩候选”，不引入旧/新两套兼容布局或新抽象。
- 理由：Host独占共享Confirmation/Recovery外根，Battle/Navigation各自持有内panel；BottomHud的Normal/History/Reference互斥、TopBar及responsive字段构成同一引用迁移。拆开这些源码会留下半套序列化契约；一次源码闭合、分门验证比另起架构切片更接近试玩。
- 功能起点按中央限定回执：PR #1 head `3541930877837e14b58020b910976b8b67b56e42`／tree `37326f759fc065f268c23f66a674be1a61f12268`；[GitHub结果](https://github.com/yyczz1/FightMatch/pull/1#issuecomment-5929800103) Completed/no-major。`ugui-fix22-integration-receipt.json` SHA256=`89255ba4c154a37168895d2daa7a249ac09b4249888cdb932ade2de06fc14b04`；仅FIX22限定ACCEPT及跨轮222，不是真实Host/布局/Demo ACCEPT。本轮读取本地回执，未联网重验。
- 复用已接受设计：中央裁定 `central-layout-hierarchy-adjudication-001.md`（`d1a13b1a…`）＞correction `engineering-ugui-layout-system-design-correction-001.md`（`9c3d6d60…`）＞base `engineering-ugui-layout-system-design-001.md`（`a2b7f967…`）；设计R `testing-ugui-layout-design-review-fix-002.md`（`8c20bfd3…`）、主美 `art-ugui-layout-design-receipt-fix-002.md`（`0c7fa6f5…`）。本轮已核完整SHA与实施delta draft表一致；不重设几何、Caption、槽位及生命周期。
- 当前指示覆盖旧 `engineering-ugui-layout-implementation-delta-draft.md` 的“完整功能R/native后才准一切源码”和234长全量激活顺序；其26路径、所有权和业务保护仍用。该覆盖只供本次源码/定向验证，不取消最终原生、主美或设备门。

## 2. 独立性与WIP移交

- 已读实际 `UguiHostRig`：它禁用 `FightMatchPlayerHost`，注入 `UguiTestTextSource`、内存存档和既有RealCatalog，再加载Runtime Prefab。LAYOUT测试无需新语言偏好仓储、Luban restore/正式生成表或真实Host Start；测试文案仍绑定冻结draft CSV，不升格为产品authority。
- 实际Host仍 `new LocalizationService(null, ...)` 并停在LocalizationNotReady；`FightMatchPlayerHost.cs`、LocalizationService、正式LOC注入/语言持久化均只读，不用“布局可测”宣称Host可玩。
- PR #3 head `6818405b859d71bf8c0f116299e7e9605a3d769a` 的 `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs`、`FileLocalePreferenceStore.cs` 及 `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs` 全部冻结；三份.meta本轮确认为不存在，不由LAYOUT顺带生成。Config/Generated/Tools的LOC工作区及其证据、材料也冻结。
- SYS先绑定上述PR #1真实tree与当前26路径的preimage/不存在状态；不能把本地master125b849当功能输入。任何重叠WIP差异交主程确认来源，不覆盖、不stash/reset、不混入PR #3。
- 写入前主程记录“功能修正已封存→LAYOUT C接管下列重叠源码”的具体thread/turn、停写时间及前像；SYS只写实施合同，C仍唯一产品写入者/本机Unity执行者。LOC负责人继续独占LOC范围；本回合不派下游。

## 3. 源码白名单与后续资源门（原26路径内）

以下路径均相对仓库；仅允许已接受布局所需的私有序列化字段/表现/fixture适配，未需改的文件保持字节不变。

- 必需修改 `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`、`Assets/Scripts/FightMatch/Host/FightMatchHostView.cs`。
- 必需修改 `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs`、`PlayerDefaultReferenceView.cs`、`PlayerNavigationRecoveryView.cs`、`CandidateBoardInputView.cs`、`CandidateBattlePlaybackView.cs`（后五项均同一Presentation目录）。
- 仅新增 `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs`、`Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs`；复用已接受12个LAYOUT用例，不新增业务接口。
- fixture适配候选：`Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs`、`PlayerBattleTestFixture.cs`、`PlayerNavigationTestFixture.cs`、`CandidateBattlePlaybackTestData.cs`（均同一FightMatch测试目录）；以及 `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs`。
- 断言/引用适配候选：`Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs`、`PlayerBattlePresentationTests.cs`、`PlayerDefaultReferenceTests.cs`、`PlayerNavigationRecoveryTests.cs`、`CandidateBattlePlaybackPanelTests.cs`、`UguiSceneCompositionTests.cs`、`LocalizedTextBindingTests.cs`（均同一FightMatch测试目录）；以及 `Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs`。SYS逐项证明必要性后从这13个既有测试/support文件中冻结最小子集，不因列名而批量重写。
- 后续自然meta门仅新增 `Assets/Scripts/FightMatch/Presentation/FightMatchResponsiveLayout.cs.meta`、`Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs.meta`；只能由获准Unity导入生成，源码阶段不手造。
- 后续native资源门仅改 `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`、`Assets/Scenes/FightMatchDemo.unity`；其.meta只读，GUID分别保持 `c36df3cfc25424c8cb3ec6cae6be1237`／`1d5124e5b55fe409d8216e78a117dec0`（本轮实读确认）。
- 原26=9个必需C#＋13个条件测试/support C#＋2个新meta＋2个资源。SafeAreaFitter、ViewId、Cb进度回调、其它源码/asmdef、TMP/字体/许可、现有meta、Packages/ProjectSettings、存档/规则/输入、文案键/CSV、ArtSource全部保护；第27个产品路径需求立即交主程。

## 4. 临时美术和安全原生入口

- 只用现有uGUI Image/纯色块/线框构成明显灰盒占位；角色/敌人美术槽使用可见的简化块或双色占位图案，编辑器节点名加 `TempArt_` 便于替换。占位属于原槽位内部装饰、raycastTarget=false，不改变热区、Caption容量或给BoardFrame增加第二Graphic；不导入未选素材/纹理/动画，也不另建Prefab。
- 不新增玩家“临时美术”等文案/key：非文字图形表达临时性；所有玩家TMP仍存诊断placeholder，运行时走原英/简中绑定。占位不能遮掉缺绑定提示；视觉可读性、正式画风均留待主美，不把灰盒当正式美术。
- 源码需在同一允许builder文件内提供**只迁移既有资源**的布局入口；SYS冻结准确方法名、参数、create-once证据路径及preimage门。当前 `PrepareResources()` 第607行起要求Prefab/font不存在并写字体/TMPSettings/旧UIDocument，严禁重跑；旧 `VerifyUguiResources()` 的119文字计数也不能当新布局验收。
- 新入口仅LoadPrefabContents既有Prefab→定点迁移/重连→原生保存，再更新既有Scene实例；保留根/存续对象身份、ViewId/GUID、字体/TMP引用，一个Canvas/GraphicRaycaster/项目EventSystem。布局专用核验按接受层级及每项绑定派生精确数量，不把119改成“至少”放宽；不改Android构建流程、重建字体、替换整个Prefab或手写YAML。

## 5. 分门及预算提案（均待SYS冻结/主程激活）

1. **S源码门可先行：** 前像/owner交接齐即写上述必需源码和必要fixture，静态核单一轴向owner、ScrollRect/Mask/Content、共享外根、epoch/退订及临时占位；无Unity、meta或资源写入，不要求PR #3先过审。收件标签SOURCE_READY，不是假装编译/可玩。
2. **R源码审查门：** 中央发布这一完整源码delta的新head并交GitHub PR Code Review，记录实际受审head/发现。旧PR #1无重大问题只证明旧输入；旧本地R不唤醒。新源码有未解决P1/P0先修，不执行原生生成。
3. **I自然导入/编译门：** 最多1次Unity2022.3.18f1源码编译/导入，建议硬上限360s，无失败循环。当前共享工程导入会自然生成PR #3三份未授权meta，因此不得直接启动；这阻塞共享工程导入，不阻塞S源码。
4. **冻结不冲突的验证输入：** SYS优先确认已有dots是否能承载本候选，否则提出C独占、无Git工作树变动的隔离候选目录：从已核PR #1内容＋本LAYOUT delta投影，排除PR #3/LOC WIP，带齐固定测试CSV/原资源/已装包；共享原文件不移走、不改名。目录/离线缓存/工具身份及空间须另获准确合同；未就绪只标I待激活，不下载依赖或借机生成LOC meta。
5. **P原生资源门：** I通过后才调用新布局入口，最多1次构造/保存/卸载/只读复开，建议≤180s；只有2个新自然meta及Prefab/Scene可成为候选delta，源/目标preimage完全核准后由C按合同交回，不能覆盖期间变化。原生“引用/序列化复开”不运行真实Host Start，不冒充真实运行时复开。
6. **T有界定向门：** P后发现并冻结真实NUnit fullnames，12个新LAYOUT＋32个受影响既有＋3个资源实例为现有设计的47上限，一次精确并集运行建议≤360s；两种物理safeRect、启用Overlay/CanvasScaler、首帧、滚动Caption首尾可达及生命周期断言保留。SYS按当前前像核真实实例数/依赖；出现额外必要case先回主程，不默增或删断言凑数。
7. 不运行旧234、222、历史全量或额外28循环；一个实例本阶段只跑一次。已有222只复用未受影响输入；新47结果不能说成234/完整Demo通过。若原生变化后有用例依赖缺口，明确未覆盖并交主测试，不以旧证据抵扣变化。
8. 每个获准Unity进程保留原owned-only身份核验及自然60s+终止后30s收尾上限；单日志≤8MiB、单阶段证据≤192MiB、剩余空间≥2GiB。首错/超时/非零退出、failed/skipped/inconclusive、集合漂移或意外写入均停止，不自动第二次compile或重跑；隔离准备开销另核，不能吞入“已通过”。不套用写死FIX22路径的进度runner/回调。
9. **最终候选审查/视觉门：** 自然meta/native资源产生后绑定更新head，GitHub只收该最终delta的实际审查结果（纯源码旧head不覆盖资源）；测试绑定实际执行字节与该tree。主美截图/滚动手感、真实Host正式LOC接入、真实保存关闭复开、Android/L1分别未完成；PR #3/LOC-A只阻断真实依赖它们的后续，不挡独立LAYOUT源码。

## 6. SYS仅机械冻结的事项与停止线

- 从固定设计和真实输入生成：上述13个条件文件的最小必要子集、字段/层级迁移表、builder准确入口及禁止副作用、源码/资源/meta/字体/TMP/CSV前像、两新文件absence、临时占位节点与图形；不重开布局/画风设计。
- 冻结WIP转移时点、候选投影与缓存可用性、验证目录/精确命令、自然meta回收/资源交回条件、证据文件白名单及47实际fullnames/预算；测试不得借真实Host/用户存档。任何第27路径、LOC依赖、旧资源异常、未知入口/预算缺失或不能保护冻结WIP，只暂停受影响门并回主程。
- 本文件仅给激活边界；主程可据此派SYS完成上述机械项，再向C发源码合同。本轮只新增本文，未运行Unity/编译/测试/联网/Git、未改产品/现存文档，未派C/R/agent。
