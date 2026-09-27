# FM-DEMO-026-G2-RESUME1 交付

状态：BLOCKED。§246 已解决旧测试断言的范围冲突；本次预检发现新 Presentation 程序集引用白名单漏列既有 SaveStoreBudget 所在的 FightMatch.Platform。G2 尚未写入实现，也未运行 Unity。
准确 C turn：01a0cdbf-a687-7b03-859c-bd6e8dcf4f81；任务01a0c403-bfa1-7e90-b503-c0fcd61f23c1；local，gpt-6-astra／max。
本次只写两份 resume1 正式报告及 E16/stage026g2-resume1/ 新证据；原源码、024、原四份报告和 E16 其他历史保持。C 不自判 ACCEPT。

## 具体阻塞与最小修订

[§241:4708](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4708) 限制新 FightMatch.Presentation.asmdef 的 references 为 FightMatch.Input、FightMatch.Application、FightMatch.Core、FlowPuzzle.Core，以及实际直接使用 QFramework API 时的 QFramework；其中没有 FightMatch.Platform。
同节要求控制器持有真实019系统与预算，并直接走019的 Submit／Retry／Resolve／End。实际 [SaveStoreBudget](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveModels.cs:17) 定义在 FightMatch.Platform 程序集；019 [Submit](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs:28)、Retry:40、Resolve:43、End:46、QueryOperation:70 的公开签名均要求该类型。现有 Application 对 Platform 的引用不代替 Presentation 对所使用类型的程序集引用。

这是源码签名与精确白名单的冲突。没有执行编译探针或 Unity，因此不声称观察到任何 G2 编译错误码，也不把推断包装成运行结果。
所需最小修订：允许新 Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef 在 references 中增加既有 "FightMatch.Platform"，仅消费原 SaveStoreBudget 及原019公开签名；不改旧 Platform／Core／Application 源码、程序集、协议或 Packages。
该修订仍保持26新增、两旧修改、637目标总项，不要求安装依赖或新增实现文件。提案未应用，保存于 [proposed-platform-reference.txt](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume1/late/proposed-platform-reference.txt)。
没有把预算类型迁移到其他程序集、修改019接口、引入任意提交委托或通过 dynamic／反射隐藏缺失引用。按仓库范围规则停止依赖此修订的实现，交 SD00 修订精确包；没有向其他任务发送通知。

## 已获准例外与实际范围

§246 对 QFrameworkIntegrationTests.cs 的两行 Unity 引用谓词例外已获准，不再等待原询问答复；Core.Tests.asmdef 的两个新引用与 noEngineReferences=false 许可也保持。由于上述独立阻塞，这两个旧文件本次均尚未改动，不能把授权写成已经实施。

| 项目 | 本次实际结果 |
| --- | --- |
| 起点／当前实现 | 完整611→611；新增0、修改0、删除0；原611项逐项长度／SHA相同。 |
| 实现 canonical | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d |
| Scripts＋Tests／Assets／GUID | 597／624／331，均保持；26个G2目标全部不存在。637／623／650／345仅为后续目标。 |
| 保护输入 | 248项当前长度／SHA保持，含原四报告及原G1输入；DLL／PDB也保持入口版本。 |
| 旧证据 | 236693项独立入口／出口元数据一致；无新增B12／B13自然根。未重哈希或重拷全部历史字节。 |
| 预算 | 新生产C#0行、新测试0行、新helper文件0；原两个helper184行只读复用。 |
| Git | 只读status／diff stat／name-only／check，退出码均0；无Git写入。 |

§246实施完成后才是611→637、两个旧修改／609保持；全组相对019原589项才是三个旧修改／586保持、新48。当前仍只是原024阶段的589→611、Architecture一旧修改／588保持、新22，不存在G2覆盖。
本次签发r123全文SHA256=27bbd6eb7a178e263c4a2906622ef37888ee342f72f0fa183d4e2bbad01292e7；当前§247协调追加单独记录，不重绑原签发全文。
§240～243规范化SHA256=a5d9afc9841b504685372cb3672c60f562c4cf04fb72f72c9003a33b3eed618c；§246规范化SHA256=617b9f611fb6e5adfca748014d7e45d1a3cc1a0a9bc405fe44e6ff1f84ad14c4；本轮均复核保持。

## 验证与验收状态

024作者交付保持 COMPLETED／独立技术裁决按R正式结果：原compile-03与tests-02均实际exit0，3183／3183 Passed、0 Failed／Skipped，其中原3136＋新47；本轮没有单独重跑024或019矩阵。
原A阶段XML SHA256=0219aec144aaa9ac560ff39a38cc2d02111f6499f6afcb3317be02c88cbc994f；本次3183具名多重集合另存entry-named-cases.json，旧测试源码全部原字节保留。
G1原四输入长度／SHA匹配§241，C2正式独立审查SHA256=b1a66ea477835e84a7b38b0292ec0fac987776a2d945260465b413c01bdcc3f8；原83／23／42独立结论沿用，不重做旧红绿历史。

| ID | 本轮状态与边界 |
| --- | --- |
| B16B-01 | 来源字节核对通过；导入及83个独立Unity TestCase均NOT RUN。 |
| B16B-02 | 019真实签名／DTO已读；控制器映射、选择、Attack／FreeLink及全倒补线运行见证NOT RUN。 |
| B16B-03 | 坐标／阈值／末Up样本与真实019路线提交NOT RUN。 |
| B16B-04 | 真实panel／UI Toolkit指针捕获、取消、失焦、Detach、重复Up均NOT RUN。 |
| B16B-05 | 单／多历史回退确认与实际状态恢复NOT RUN。 |
| B16B-06 | G2保存失败／不明、pending／旧结果、令牌一次交接、Busy与重建NOT RUN；019既有证据沿用。 |
| B16B-07 | 611起点与旧用例源码保全通过；637最终范围、83导入及新UI全量验证NOT RUN。 |

规定B阶段compile与EditMode均NOT RUN，原因是未获准的程序集引用；本轮Unity运行数0，三个B输出仍不存在。没有伪造PID、退出码、G2 XML、测试总数或同版编译通过结论。
正式发布／PlayerSave来源、025正式内容、027动画播放、028完整面板、029正式场景及完整Demo、交互式Editor／PlayMode／物理鼠标触控均NOT RUN；合法AwaitLinks成功链缺口、未覆盖保存故障交叉组合及§184历史NOT VERIFIED保持原边界。

## 本轮证据与停止点

新证据根：TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume1/。entry／audit完整保存611项起点与当前值、保护项、GUID、DLL和旧证据元数据；sources/entry包含实现与Tools实际副本。
预检／保全归档共1821文件、263685743字节、0空目录；源、payload、ZIP路径／长度／SHA与空目录集合逐项一致，归档源在打包后保持。
ZIP：archive/FMDemoB16-G2-RESUME1.zip，34495362字节；SHA256=b6e12c49ff16a3ca8fa433a7d37cd546d464f6aec95eaac681a568d419e78706。它仅是本轮预检／保全证据归档，不是G2测试交付。
archive／late及两新正式报告按自引用规则排除并由late/resume1-bindings.json独立绑定；原生CommandExecution／FileChange只导出本次准确turn的实际事实和所选上下文字段，不含推理。
scope完整列before／after／implementation、changes、protected、GUID、budgets、namedCases、validation、archive和notRun。准确task_complete由SD00／R在formal final后核验，不预先宣称。
本次最终为 G2-RESUME1 BLOCKED，024既有完成保持；交付后停止源码与报告修改。收到精确引用范围修订后仍从同一611冻结起点续做G2，保留本轮证据，不重跑已有有效验证。
