# N10 持久化只读诊断

2026-10-06；LAYOUT-N10-PERSISTENCE-READONLY-DIAGNOSIS。
SYS `01a10ee4-4b9c-7c40-af7b-2d5244267c57`。唯一收件：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；issuer actual `01a10ee3-1daa-7210-a896-486b16f73329`；AGENTS§6授权。

## 结论与边界

保留 N10 集成 **NEEDS_FIX**，不撤P1。内存删除未持久化；原生driven重建、Set底层状态未更新及其它机制仍待区分。只建议隔离副本冷启动反事实，不重复Set/Force/等待/Save。

按 diagnosing-bugs/uGUI Understand只读诊断；依包跳过复现/修复。仅创建本稿，无Unity/测试/探针/保存/代码资产门修改/GitHub操作；engineering.md归主程。

## 固定依据

实读 [N10 集成回执](layout-n10-integration-receipt.json)，6947B，SHA256 `8c45b442696b7d5a6be24cd88a9c24ccc01583cff514405b1accadb819363da4`。
B/T/Scene/Prefab/native receipt/log哈希复核一致；三D、243冻结和清理审计复用回执，不重跑。
完整读取 [原 P1 回执](testing-layout-s1-pr4-native-review-receipt.json)，11930B，SHA256 `bd43d0afa20f541e95d08d85532323e81da14797271bf00e65aa2a17f005450f`。
历史被审 head=`e3d581df42f3a25cfdfc319bca5509f8b5382ada`；当前N10未获对应head审查。[原讨论](https://github.com/yyczz1/FightMatch/pull/4#discussion_r4166096874) 仍current/unresolved（中央回执）。

运行时输入（Assets/Scripts/FightMatch/；B/T按固定派发）：

- Presentation/FightMatchResponsiveLayout.cs：8707B / `f6f1f869b2eb138511311c2a528034cb41633448f5a423a5ce351a40b298db16`
- Presentation/SafeAreaFitter.cs：1577B / `34234229face2665d02c5806a890cec15a297f364b96cce80b457eabc731c8a7`
- Host/FightMatchPlayerHost.cs：6595B / `12dba7d11323da883f108ea416699a6cf638a9360587c984d7d070ebeb6cc287`
- Host/FightMatchHostView.cs：17703B / `e72de88638d6c25bfe4d80e2c1b8990dae5ae225c6d4fe976b4b1641b1c975b9`
- Presentation/Localization/LocalizationService.cs：2786B / `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408`

## 事实：三层不同

B1432–1436深拷贝记录；1937–1947冻结原27/保留22并绑定磁盘。1851–1866：必要时MarkSceneDirty→Set1→Get精确22→同路径Save1→磁盘差异门；Set/Save间无Force/yield。

N10 log366/377/408均在Set前：27→105→125条，Overlay/root/active/自身Canvas驱动均真，local/lossyScale≈.769800365，八个屏幕角分量通过，dirty=false。log424与receipt.immediateObservation确认Set1/Get22/Save1/True；磁盘仍原12900B/27条/SHA。失败在Save后、PostSave前；无PostSave/复开/导出，112候选用例未发现/运行。

已排除“未调用保存”“Get未接受22”“三个时点有效scale为0”。缺Set后属性、保存边界内存及冷复开观测，不能区分重建或底层状态仍旧。

## 语义：driven 不等于 default override

1. [2022.3 Set API](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SetPropertyModifications.html)定义赋予实例修改集合；[公开绑定79–99](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/Prefabs/PrefabUtility.bindings.cs#L79)的Get/Set/Record进入原生层，无driven删除必原样落盘契约。“Set只改集合”未获证明，不默认追加Record/Revert。
2. [SaveScene](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.EditorSceneManager.SaveScene.html)不论dirty都保存；True不保证指定文本差异，N10修复门仍失败。
3. [DrivenRectTransformTracker](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/DrivenRectTransformTracker.html)及[自动布局文档](https://docs.unity3d.com/2022.3/Documentation/Manual/UIAutoLayout.html)说明driven当前值不作普通Scene值保存、不标dirty；未规定五字段必须写0或删记录。“正常零占位”未证实。
4. [IsDefaultOverride](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.IsDefaultOverride.html)仅限最外层实例根且列表不含scale。RuntimeCanvas为子节点（Prefab29255/N10 parent），不能以默认覆盖解释五条。
5. 三scale记录0而有效值正；两anchorMax记录/有效值均0，但Prefab源为1。不能混为五值全错/全对。[RectTransform绑定165–166](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Runtime/Transform/ScriptBindings/RectTransform.bindings.cs#L165)driver公开、mask为internal，不能证明writer规则。
6. 本机uGUI1.0.0 CanvasScaler.cs237–247/301–346/374–380按显示/参考尺寸和match算scaleFactor，与.7698相容，不证明记录writer。[Canvas绑定79–97](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/UI/ScriptBindings/UICanvas.bindings.cs#L79)将原生Rect更新与Force事件分开，不支持再Force修好。

## 原功能风险：首次取样与恢复

PlayerHost.Start27–40使用Scene的runtimeRoot，创建localization后立即HostView.Bind；HostView57–58先订阅ValidityChanged再Responsive.Bind。Responsive.OnEnable67–70也Apply，但localization空则83行返回；Bind35–37重置sampled后取样；原生更新实际先后未知。

Apply84–96取safe rect/factor/lossyScale，先缓存再验有限/正xy缩放和尺寸。Gate(false)关闭battleInteraction双交互标记、显示LayoutDiagnostic；HostView173–174取消指针。不管全部Host按钮；谓词不查scale.z。

并非永久锁存：BeforeRender72–78在safeArea.Version、rect尺寸、factor或scale变化时重采；SafeAreaFitter15–29有效新输入更新Version。相同无效样本即使dirty也在87–88早退，不是每帧重新校验。新样本有效且完成布局后126/140–146才Gate(true)恢复该组/隐藏诊断，不恢复已取消手势。OnDisable/Unbind的ReleaseGate置true是清理，不是布局通过。

N10/T139–208检查Editor Canvas，不运行无ExecuteAlways的Host/Responsive/SafeArea生命周期。满屏不证明safe rect满足响应式高度；若占满N10的831.38×623.54 Canvas单位，height<812，尚有尺寸拒绝原因。不能据640×480样本证明Player首帧可用。

当前Host35传LocalizationService(null)，IsReady仅查source非空，37–40报告LocalizationNotReady后停止Session创建；首次布局Bind仍发生。LOC另阻挡可玩，本包不修。保留首帧/恢复/输入风险，不断言永久锁死。

## 唯一后继建议：冷启动反事实往返（待另包批准）

只区分假设，不交付修复。主程固定输入/路径白名单/预算后交C；同N10的2022.3.18f1及包版本，隔离副本，不碰共享项目/证据/存档。

启动前在Scene副本精确删五块，确认其余字节不变/磁盘22；Prefab/meta原样。新进程冷载入，禁Set/Revert/Record/Force/Play；读取load返回及最多两次自然Editor更新后的Get/五字段有效值/driver。加一个无组件独立临时根作保存阳性marker，不改Canvas层级；Save1/Close/Reopen1，记录各边界集合/磁盘块/marker/几何。手改YAML仅作反事实，不是生产修复。

判别：marker落盘且五条冷载入/保存中重现，支持“不依赖N10旧Set状态的重建”，不宣称已知C++writer。marker落盘且22保存复开成立，则“保存必重建五条”不成立，收窄至Set/会话路径，仍不能发布文本修复。marker未落盘/混合/未解释变化均UNKNOWN；保存前Get非22是观察，不提前丢弃后续证据。

启动1/Save≤1/复开≤1/重试0；每段≤两次自然更新/5秒，总Unity≤60秒。输入漂移/异常/超时/身份或隔离失配立即停，不追加Set/等待/保存；保留事件序列及清理证据。另包须列执行器/编译预算和探针文件；本稿不授权运行。

若支持重建，向中央提议“无五记录”改为“解释后的精确序列化包络＋功能门”，不直接删断言。须附反事实/保存复开、同候选head首次Host/Player序列（Bind/Apply原因、safeArea/scale、Gate转移、首个可交互渲染帧、输入取消/恢复）及LOC前置说明，交同head GitHub独立核实P1。当前证据仍缺，不预签ACCEPT或设备/首Demo通过。
