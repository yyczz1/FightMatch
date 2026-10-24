# LAYOUT-N07-READONLY-DIAG-01 — 覆盖重现与关闭路径诊断

2026-10-05 · DESIGN_ONLY / ROOT_CAUSE_UNCONFIRMED。N07保持中央NEEDS_FIX，非代码审查/实施/N08授权。交付/唯一收件主程 01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local，issuer actual 01a10b8d-58b7-79c2-b71f-27b2d60692e9；SYS 01a0f2e6-29bb-7092-abf0-705b41b7bf93/local，fresh actual **01a10b8f-3a35-73e3-8a1f-9746ad6d9137**，已自读/主程确认。

权源：[中央回执](layout-n07-integration-receipt.json) nextPacket及本次派发；SHA256 f06d91d1ee5aa9b272f61f7c7370d80fb3352ea02f1b44d62d6d2fad3389eb60。只新增本文，不写engineering.md。亲读 diagnosing-bugs、unity:ui-ugui；本包覆盖其复现/最小化/执行要求，复用红证据，不称已最小复现。无Unity/编译/测试/探针/代理/安装/Git/产品或旧证据变更。

## 1. 冻结输入与引用约定

下文 B/T/L 的冒号数字为这些固定文件的一基行号；R的引用为JSON字段。E=TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N07/PExecution。未扫描旧全树或重跑验证器。

|别名/路径|字节|SHA256|
|---|---:|---|
|原计划 docs/team/2026-09-30/engineering-layout-s1-fix04-n07-source-plan.md|16368|12c0e7bb6fca1842b1e3f258de8c7cc8a6f66066af30486154cd01aa119e0984|
|B Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs|183600|4c99967906fe9afdd4f53d58db750acc7e8bbc1827f61e73577fcbc9b3b33116|
|T Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs|40209|6e52750705ccdc18e26f2bf4591fa9d675bb1e641cb7bb396f9c6a7f804f6f52|
|Scene Assets/Scenes/FightMatchDemo.unity|12900|0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd|
|Prefab Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab|1322960|0064bbee5e0d84dfdfd627310a9abada5093252bb26cf363d0646b38586f19d2|
|R E/receipt-manual.json|52934|b4b66ec5b10867d48bbaf839dee6d9eb98a6eed86d0d4d2cf3c00da6fa4a2548|
|L E/editor.log|109057|3efdfa45acbaf6150a53dc12abb8b0dc4f3b43f283ac2159d8cc5896fb427e5e|

## 2. 直接事实（不是根因解释）

|L行/快照|有效local/world scale|五条GetPropertyModifications记录|8个coverage|overridesValid / preservedOverrides|
|---|---|---|---|---|
|366 post-set|全1|空|全false|true / true|
|377 after-force|全1|仍空|全false|true / false|
|408 after-two-editor-updates|全0.769800365|同一source五项均为字符串0|全true|false / false|

三次均WaitingPreSave、同一source/parent/driver、dirty=false；anchors均(0,0)、pivot(.5,.5)、pixelRect640×480、factor0.769800365。最后矩阵/角点变化，最大角误差约7.05e-6像素，确实通过0.5门。

**只三条scale记录与有效scale不一致**；两条anchorMax记录0与有效anchorMax=0相同，但与源Prefab anchorMax=1不同。Prefab:29250、29257–29258仍scale1/min0/max1；Scene:267–284原本就有这五个0。Scene:259–264另有必须保护的pivot0，不能猜它就是after-force差异，也不能扩删它。

B:1429先取modifications，1431–1453抓取同一sample；1461拒绝stale。B:1227–1233比较记录与**源**SerializedProperty，不是实例有效值。L428首错为覆盖门；B:1552完整序列Require尚未执行，但377/408比较已为false。

L428：Set1/Save0/Force1/Queue1/decisive1/token1/event2；R.nativePhaseEvidence.saveReturnedReported为空，非Save实际返回false。保存/正式关闭复开/导出未达；L419/421证明cleanup尝试CloseScene。B:1600/1609保留首错，1607产生最后Scene关闭警告。

R.process：PID43205/exit1/首错23.6114秒/12 owned闭合；exports空，Scene/Prefab未变。L244/245及R.compileEvidence确认两程序集Csc、testsRun=false。三阶段/设备未验收，23叶冻结核验沿用主程结果。

## 3. 源码/官方语义与适用边界

1. 本地 Library/PackageCache/com.unity.ugui@1.0.0/Runtime/UI/Core/Layout/CanvasScaler.cs:237–247订阅preWillRender，301–346按显示尺寸计算因子，374–380仅在缓存因子改变时设置Canvas.scaleFactor。此源码可解释因子路径，**不能证明该次private缓存值**，也没有直接解释Prefab五条0；不新增反射。
2. [2022.3 UICanvas绑定:79–97](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/UI/ScriptBindings/UICanvas.bindings.cs#L79) 中Force触发preWill/will回调，内部原生RectTransform更新是另一个接口；[QueuePlayerLoopUpdate](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication.QueuePlayerLoopUpdate.html) 排队更新。B:1501–1534确实只在第二次update决定，不证明每项原生事务均完成，不能把“两事件”升级为序列化完成屏障。
3. [GetPropertyModifications](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.GetPropertyModifications.html) 与[SetPropertyModifications](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/PrefabUtility.SetPropertyModifications.html) 是提取/赋予相对Prefab的修改；[2022.3绑定:79–84](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/Prefabs/PrefabUtility.bindings.cs#L79) 委托原生实现。所读资料未保证跨更新的数组顺序稳定、Get等于磁盘文本，或Set后未来记录永不变化；也没有证明此调用用错了API。
4. [Auto Layout驱动属性](https://docs.unity3d.com/2022.3/Documentation/Manual/UIAutoLayout.html) 与[DrivenRectTransformTracker](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/DrivenRectTransformTracker.html) 说明驱动有效值不按普通Scene属性保存，变化可不标脏。这只是一般机制：不能推出五条0是无害占位或保存后必消失。[RectTransform绑定:165–166](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Runtime/Transform/ScriptBindings/RectTransform.bindings.cs#L165) 中driver对象可公开读，具体drivenProperties掩码是internal；日志ownDriver不能证明这五项逐项均被驱动，不反射掩码。

上述来源本次已打开；2022.3分支非18f1原生实现证明。相关模块未找到CONTEXT.md/命名ADR；沿[uGUI纠正方案](../../system-design/2026-09-17/runtime-ui-ugui-correction.md)，不改Application/玩法/存档。

## 4. 推断：三个可证伪候选（可并存，未试验）

|排序/候选|为何列入；仍缺什么|可证伪预测，均须另获授权|
|---|---|---|
|1 原生驱动与Prefab记录的更新/归一化不同步|driver为Canvas，几何随update恢复而零记录重现、dirty不变，与驱动语义相容；并无“零记录生成规则”证据|若只是内存表示，获准隔离保存后应能出现“磁盘五条已删、只读复开几何正确，但内存Get仍含0”；若磁盘仍有五条或复开失效，就不能用此解释放行。当前Save0无法判别|
|2 Force区间非五项记录内容或顺序变化|377五项空而SequenceEqual已假；回调可能修改内容，也可能仅Get顺序变化，缺原序列|多重集有增删/值/引用变化即否定仅重排；若多重集相同而索引不同则仅重排。两者都不能自动通过原顺序门，也不能单独解释五项重现|
|3 Set后的Prefab更新重新形成旧覆盖，清除尚未成为可持续状态|post-set立即正确不证明后续持续；最后五条与原磁盘五条同值。也可能是确实未修掉的持久错误，而非无害驱动表示|若未来许可保存后的磁盘仍有原五条且复开仍按其恢复，当前“只删五项”修复失败；若磁盘只删五条、其余逐byte保留且复开合格，则排除“这五条仍持久存在”这个版本|

候选2未定位写者：FightMatchResponsiveLayout.cs:51–83的Apply受localization约束；FightMatchHostView.cs:41–60在Bind才订阅，源码不证明此次执行。Save0不能区分候选1/3；不盲加update/Set/Revert/Record或Transform赋值。

## 5. preservedOverrides缺口与最小补证

B:1489只输出相等布尔及五项子集，完整sample.overrides/run.preserved未落盘。runner-manual.py:185–194仅复制日志。377证明**非五项键/数量/顺序/身份表示至少一处变化**，无法定位、断言仅重排或其余覆盖无损。

最少缺：Set前过滤期望及三阶段实际序列；每项原索引、target GUID/fileID或GlobalObjectId、path、原value（null/空/0分开）、reference、重复数；数量、ordered/多重集digest、首差异及完整增删/改值/移位。哈希不能替代原差量，排序比较不能取代顺序保护。

仅扩B的snapshot/log：基线冻结，原三点编码差量，日志/谓词复用sample；不额外Force/Queue/采样或用诊断hash放行。保留9条×8KiB/总96KiB，超限明示截断/不足并失败，不丢字段称完整；无private反射。

## 6. 最小未来文件/方法建议（当前不实施）

**可确定的清理修正仅在B**：LayoutSceneRepairRun增加本run空白Scene句柄；新增私有LayoutCloseOwnedScene，由LayoutContinueSceneRepair:1570与LayoutFinishSceneRepair:1606–1609共用。先核待关句柄确为run.scene；若它是最后loaded Scene，创建一次[EmptyScene + NewSceneMode.Additive](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.EditorSceneManager.NewScene.html)并记录自有句柄，再[CloseScene(run.scene,true)](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.EditorSceneManager.CloseScene.html)。Additive保留其它Scene；不用NewScene Single兜底，不保存、清空或关闭任何外部Scene。

LayoutOpenRepairReopen:1592改Additive保留空Scene，原验证仍限run.scene；复开后关闭也走helper。最终空Scene不再Close，由自有batch Exit释放；无路径/保存/导出。核创建/关闭返回与所有权，记录次级cleanup错、保留首错；退订/非0/owned闭包不变。新helper及内存Scene须另签白名单，不消除覆盖失败。

**观测补证仅B**：LayoutSceneCanvasSnapshot、LayoutOverrideKeys、LayoutCaptureSceneCanvas、LayoutLogSceneCanvas按§5补差量；原几何谓词/分类器/Prefab门/同步入口不改。尚无可安全过覆盖门的修复依据。T:138–261及两负例/其它测试保留，不删断言变绿。

若后续日志字段改变，另列新执行包专用runner.py的phase_evidence解析白名单，保留N07 runner:235两覆盖门、348磁盘仅删五项/其余文本相同及唯一Scene导出门；**绝不编辑E/runner-manual.py或其它冻结证据**。

## 7. 未知、中央决策与后置验证条件

当前无法确定：五条重现的原生写者/阶段、377具体差异、保存后实际序列化结果、只读复开后的Get与磁盘关系。已证“当时覆盖像素正确”只否定该时刻scale为0/画面覆盖不足，不证明资产、其它覆盖或未来重开正确；dirty=false也不是资产验收。

需中央单列决定（本文不代批）：若证据最终证明正常驱动表示与“每个内存快照Get均无这五项”不可兼容，是否把内存表示门改为独立持久化证明；如需在该前置门失败时仅为研究执行一次隔离保存，也必须明确批准这种诊断分支，保持共享导出0。当前不能先跳过失败保存再补理由。若要接受其它覆盖重排、增加Set/等待次数或允许磁盘残留五项，同样是原假设/合同变更，不能借“coverage已过”自动批准。

未来①源码包获批：scope/差量/日志/所有权静态核≤3次/累计30秒，不重跑旧检查。②新actual C＋独立授权：最多一次≤180秒、retry0，取三点差量并核cleanup无最后Scene警告；原门失败仍Save0/导出0。仅原门全过且获保存权限才Save1→关闭→只读复开，不另开Editor。自然60＋fresh-owned TERM30，无外来信号。诊断保存例外须先有中央决定。

最终仍须0.5真实四角/有限正几何/own driver、严格Prefab、其它覆盖与顺序、141/3/6 IDs、磁盘仅删五条且其余字节不变、Prefab/meta/资源身份、复开非dirty且字节不变、三JSON/exit0/owned闭合，才导出唯一Scene。受影响程序集新编；后置99测试另签运行一次且无缺失/失败/跳过，同源码＋Scene head的GitHub独立审查及设备/视觉门保留。均未激活，SYS停写交主程wait/read。
