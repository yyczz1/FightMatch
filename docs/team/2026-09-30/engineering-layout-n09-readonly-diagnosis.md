# LAYOUT-N09-READONLY-DIAGNOSIS

2026-10-06 · DESIGN_ONLY；中央NEEDS_FIX不变。[授权/输入](layout-n09-integration-receipt.json) nextApprovedStep。issuer/唯一收件01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10d22-13e4-7a13-8978-72efceb35234；SYS 01a0f2e6-29bb-7092-abf0-705b41b7bf93/local，fresh actual **01a10d23-d6e1-7e32-81cb-7fd447a0a4d7**（自读确认）。

已读diagnosing-bugs、ui-ugui/ScrollView；按本包免复现/最小化，仅解码旧文件。无Unity/探针/编译/fixture/测试/代理/产品变更，只写本文，非N10授权。

## 1. 证据与完整差量

N=TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N09；D0/1/2=N/D/override-diff-00/01/02.json，3430/12350/15988B，完整SHA见授权。L=N/PExecution/editor.log。逐索引/四段长度前缀解码，保留重复和顺序；Prefab父链定位目标。

B=Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs；T=Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs。Scene=Assets/Scenes/FightMatchDemo.unity，Prefab=Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab；均派发固定身份，B与N/S同像。

原27删五得22；D0/D1/D2=22/100/134。**原22值/target/reference/次数(各1)/相对序不变，D1的100项亦原序留在D2；无删除/替换/重复，插入改变绝对索引。**引用全null，target GUID均c36df3cfc25424c8cb3ec6cae6be1237。

原22依次：根GO m_Name=FightMatchRuntimeRoot/m_IsActive=1；Canvas Rect m_Pivot.xy=0；TMP Language/Caption、License/Caption、Create/Caption、ProfileTitle、StartupStatus各m_isOrthographic=1；根Transform m_LocalPosition.xyz=0、m_LocalRotation.w=1/xyz=0、m_LocalEulerAnglesHint.xyz=0；TMP Back/Caption、Reload/Caption、Continue/Caption各m_isOrthographic=1。D0索引0..21，D1索引为[14,15,16,17,24,25,26,27,42,43,44,45,46,47,48,49,50,51,52,53,74,75]；D2为[16,17,18,19,33,35,37,39,57,59,60,61,62,63,64,65,66,67,68,70,97,99]，均0基。

C=RuntimeCanvas/SafeAreaRoot/ScreenLayer/StartupPage/StartupScroll/Viewport/Content；H=RuntimeCanvas/SafeAreaRoot/HudLayer/TopBar。R6=m_AnchorMax.xy+m_AnchorMin.xy+m_SizeDelta.xy；P2=m_AnchoredPosition.xy。下表为全部新增，每分量字符串0、每target/property恰1。

|目标RectTransform fileID／路径|D1新增|D2新增|
|---|---|---|
|20060541972559401 C/Create|R6|R6+P2|
|525732035437362821 C/Reload/Caption|R6+P2|同左|
|2083101828246034960 C/ProfileTitle|R6|R6+P2|
|3603424187799618609 C/Reload|R6|R6+P2|
|3613386703531820566 C/Create/Caption|R6+P2|同左|
|5408553659951591733 H/Back|R6|R6+P2|
|6280369087512753137 C/Continue/Caption|R6+P2|同左|
|6783473654260464751 C/Continue|R6|R6+P2|
|8076177029604565045 H/Language|R6|R6+P2|
|8217930521597258139 H/License|R6|R6+P2|
|9020564264068189081 H/Title|R6|R6+P2|
|9181528876988089927 C/StartupStatus|R6|R6+P2|
|6960656654093734010 C本体|0|m_SizeDelta.y=0|
|1013562322995895578 RuntimeCanvas|0|m_AnchorMax.xy、m_LocalScale.xyz=0|
|9054230707113910417 RuntimeCanvas的Canvas组件|0|m_AdditionalShaderChannelsFlag=25|

D2另加9条TMP m_TextStyleHashCode=-1183493901（各1）：2223056809955240070 H/Language/Caption；2452066831200993119 H/License/Caption；2783145704458770550 C/Create/Caption；3538173933649760411 C/ProfileTitle；4530181991579543270 C/StartupStatus；5257332161253344251 H/Title；5265726911075511648 H/Back/Caption；7259531018279518139 C/Reload/Caption；7481568776225615129 C/Continue/Caption。

78=12×6+3×2；112=12×8+1+5+1+9；跨阶段+34=18(P2)+1+5+1+9，替换0；新增是否落盘未知。

## 2. 时序、真实值与证据边界

红证据原命令：receipt.process.argv的18f1/StandaloneOSX/projection/RepairLayoutSceneOverrides/N09 I/P/log，未重跑。C actual01a10d02-87d6-7023-99be-1fdb5b249d7b；25.880972208秒首错/exit1，Save/复开/导出0，11 owned闭合，含自然60秒后VBCS一次TERM。

B:1819–1825固定22/Set1→1685 L361/D0→1687 Force1→1689 L372/D1→1693 Queue1→1703–1711第二update L405/D2。1466覆盖门失败；1732全序列Require未达。L416诊断完整/cleanupFailures=0；NewScene/Close次数UNOBSERVED。

L361/372有效scale1/覆盖8false；L405有效local/world scale0.769800365，角误差≤7.05e-6px/覆盖8true。三点factor0.769800365、anchors(0,0)、pivot(.5,.5)、dirty=false。**三条scale0不等有效scale；两条anchorMax0等于有效值但不同源1。**子Rect有效值未采，Prefab不是实测替代。Save0磁盘仍原五条，不能推断保存结果。

## 3. 源语义、推断和未知

[2022.3驱动属性](https://docs.unity3d.com/2022.3/Documentation/Manual/UIAutoLayout.html)可不标脏且有效值不按普通Scene属性保存；不证明所有0可忽略。[Get/Set原生绑定](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/Prefabs/PrefabUtility.bindings.cs#L79)是内存实例修改接口，非磁盘等价保证。[Force源码](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/UI/ScriptBindings/UICanvas.bindings.cs#L82)只触发回调；两update非序列化屏障。均本次打开；2022.3源码非18f1原生writer证明。

本机Library/PackageCache/下，com.unity.ugui@1.0.0/Runtime/UI/Core/：CanvasUpdateRegistry.cs:91/162–215（SHA前12:6d74c8cfa350）回调→Rebuild；Layout/LayoutGroup.cs:294–310（e38988736ce9）驱动anchors/position/size；Layout/ContentSizeFitter.cs:86–119（a6b30a3b6e69）驱动SizeDelta。C含启用VerticalLayoutGroup和PreferredSize fitter，形态相容，非writer实证。

同根com.unity.textmeshpro@3.0.6/Scripts/Runtime/：TMP_Text.cs:359–378（2cfcb00a4464）补Normal；TMP_RichTextTagsCommon.cs:153（93c4a4f8bf47）NORMAL=-1183493901；TMPro_UGUI_Private.cs:4418–4426（1f8ba223bdd2）OR channels25。源初值0与新增相容；这10项不是Rect驱动值，不能统一免责。

实证：22保留/新增明细/根几何好/磁盘未动。布局/TMP参与为推断；writer、0规则、落盘及复开未知，未证明门一定错。

## 4. 候选：后置单Set＋分层验证（待中央）

冻结原27及保留22的副本→固定Force/Queue/两update验证写入前几何→原22完整子序列且额外只限本表/原五项、每项≤1，否则失败→同一continuation唯一Set(22)→即时Get逐项逐序等22→Save1，无中间Force/yield/Queue。不得把旧几何快照标成Set后。Save或会重建记录，此方案待实证，不保证修好。

|拟变旧门|保留原意的替代；依据|
|---|---|
|更新后Get总数组等22|事务时仍精确等22；更新时保留原22次数/相对序、限定额外项。磁盘只减五条/其余字节不变；依据实测插入而非替换|
|更新后Get无五零值|内存与几何分判；保存后/复开磁盘必须无五条，严格Prefab与真实0.5不变；依据记录0不等有效scale，非永久豁免|
|保存前几何采于Set后|改事务前几何＋Set后记录门，保存/复开后仍采真实几何；确属前置门改变，必须中央批准|

未来B限RepairLayoutSceneOverrides/LayoutSceneRepairRun/LayoutContinueSceneRepair/快照重载LayoutVerifySceneCanvas及日志；新增私有LayoutVerifyRepairOverrideEnvelope、LayoutVerifyRepairDiskDelta。原谓词/分类器/同步入口/关闭所有权不改。T限SavedDemoScenePreservesUsableCanvasWithoutStaleOverrides、CaptureSavedCanvasSample作磁盘/内存分层，保留几何/identity/finally/byte门；同类增未知项/重复/丢失/换序/磁盘残留负例。旧六测试及两负例不删，新增数另签。

资源仅未来隔离Scene保存、全过才同路径导出；Prefab/meta/包零写。新包runner仅适配阶段/内存门/测试清单，磁盘精确差量/三JSON/exit/保护/唯一导出门不减，N09证据不改。

后继须中央确认三项变门、另签源包/新C actual/一次≤180秒retry0权限，受影响程序集新编。验证：Set后是否精确22？Save是否只删五条、未写112项？关闭只读复开几何/字节/身份是否合格？任一否则NEEDS_FIX/导出0，保留保存事实，不重试/回滚；自然60＋owned TERM30。旧99＋新负例、同head GitHub及设备门另签，均未运行。主程wait/read收件。
