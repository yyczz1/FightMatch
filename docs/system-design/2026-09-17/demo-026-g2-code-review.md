# FM-DEMO-026-G2 独立审查（含 RESUME1）

VERDICT: NEEDS_FIX

Scope: PASS；当前 G2 实现新增／修改／删除均为 0。Acceptance criteria: B16B-01～07 的 G2 行为验收全部 NOT RUN。
原因是已核实的精确包依赖遗漏 B01，以及因此仍未交付的 G2 实现；这不是观察到的 Unity 编译或运行失败。
§246 已解决原旧测试断言的范围问题，本报告不把那个已解除的问题继续当作阻塞。当前只需 SD00 补准新 Presentation 对既有 FightMatch.Platform 的引用，再按原目标实施并独立复审。

## 准确续接与正式完成门

- 依据 [system-task-packets.md §241–243](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4690) 及 [§245–246](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4798)，并遵守 AGENTS.md 和 .agent 的审查、验证与范围规则。
- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，标题「FightMatch 本机保存与应用接入实现」。
- 原 C turn＝01a0cd85-45f2-7841-903c-20d443a6314a，于 2026-09-23T09:58:08.931Z 完成，正式状态为 024 COMPLETED／G2 BLOCKED。该事件只满足 024 原完成门，不能作为本次 G2 完成。
- §246 准确新 C turn＝01a0cdbf-a687-7b03-859c-bd6e8dcf4f81；原生 task_started＝2026-09-23T10:11:24.448Z；turn_context＝10:11:24.490Z、10:16:03.403Z，均 gpt-6-astra／max、当前项目。
- 新 C 非异步 formal final＝2026-09-23T10:29:48.449Z；准确 task_complete＝10:29:48.638Z。wait_threads revision 10 独立核 completed、error=null、任务 idle；formal final、两份 resume1 正式报告与实际状态一致，均为 BLOCKED。
- R 任务＝01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确 R turn 始终为 01a0cd85-f31b-7ec3-b95f-59e73defee20，未因 SD00 中途交接伪称换回合。开始于 2026-09-23T09:08:22.990Z，gpt-6-astra／max。
- 原 §240–243 规范化 SHA256＝a5d9afc9841b504685372cb3672c60f562c4cf04fb72f72c9003a33b3eed618c；新增 §246＝617b9f611fb6e5adfca748014d7e45d1a3cc1a0a9bc405fe44e6ff1f84ad14c4。R 已核当前冻结段与源副本相符。
- r123 签发全文声明 SHA256＝27bbd6eb7a178e263c4a2906622ef37888ee342f72f0fa183d4e2bbad01292e7；R 收续接时当前已为追加 §247 的 r124，不把当前全文冒称 r123。§246 冻结段独立核验有效。
- 本报告在新 C 完成门满足后出具；未凭旧 BLOCKED 或提前落盘报告裁决。024 原独立审查已完成并保留，不重做其有效审查。
- R 按 §243 本地完成 Standards／Spec 两轴，未启动技能额外代理；未开 Unity、跑项目测试或业务探针，未改作者文件、证据或 Git，未发任务间通知。R 自己的完成事件由 SD00 在本次 final 后核验。

## Standards：作者守界与实际范围

C 遇到新的接口依赖缺口后没有猜测范围，没有移动预算类型、修改 019、使用 dynamic／反射或任意提交委托掩盖依赖。§246 的两行旧断言例外已经获准，但 C 在形成完整 G2 前未先改这两行；这是实际未实施，不是遗漏声明。

| 核验项 | R 独立结果 |
| --- | --- |
| 起点／当前 | before、after、implementation 均完整 611 项；与 024 冻结逐项长度／SHA相符，实际新增0、修改0、删除0。10:29:56 当前实物复核一致。 |
| 实现 canonical | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d。 |
| Assets／GUID | Scripts＋Tests597，Assets624，331 个唯一有效 GUID；Assets canonical＝65746d65c0e6f8c0fc03511356af45bbe749c07986c3555b01af449ea5ed16bb。 |
| 计划目标 | 26 个 G2 新目标均不存在；Input／Presentation 导入与 UI 实现尚未写入。637 项、650 Assets、345 GUID 仅是未来目标。 |
| 两项旧覆盖 | Core.Tests.asmdef 与 QFrameworkIntegrationTests.cs 均保持原字节；本轮尚无 §246 所准覆盖。 |
| 历史范围关系 | 当前相对原019仍为589→611、Architecture唯一旧修改／588保持、新22项。完成G2之后才是611→637、两旧修改／609保持；全组三旧修改／586保持、新48项。 |
| 原四份报告 | 024两份及首次G2 BLOCKED两份全保持。R的024报告也已冻结：146行、18933字节，SHA256=a3b77ef5dfce2a59ada4e47eb867dcf927192f9bc52c06657c78743517a55057。 |
| 保护输入 | C 248项保护清单完整；canonical＝b7cb61aa866f688be6f9cfcd30a034b3e4f6fd4eae75f156378bd4b4898081ba。R另以484项扩展基线于10:31:11–13逐项核长度／SHA，全部一致。协调三稿以获准§247追加后的基线核对。 |
| 预算 | 新生产0行、新测试0行、新helper0个；原两个helper共184行只读。续接delivery61/280行；scope1113422字节＜3MiB。 |
| Git／工具 | 原生记录仅见读文件、取证、归档、报告及只读Git查询；未见Git写入或Unity调用。没有将取证脚本的成功冒称业务通过。 |

规范轴未发现作者越界的阻断项；G2 没有业务补丁可作正向代码接收。

## Spec：B01 — 新 Presentation 引用白名单漏列 SaveStoreBudget 的所属程序集

具体位置为 [§241:4708](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4708)：新 Presentation.asmdef 只允许 FightMatch.Input、FightMatch.Application、FightMatch.Core、FlowPuzzle.Core，以及有直接使用时的 QFramework；未列 FightMatch.Platform。

同时本包要求控制器持有真实 019 系统与调用预算，并通过其唯一写门提交和恢复。R 独立读到的实际公开签名是：

| 源位置 | 既有接口事实 |
| --- | --- |
| [LocalSaveModels.cs:16](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveModels.cs:16) | SaveStoreBudget 是 FightMatch.Platform 命名空间中的 public sealed 类型，位于 FightMatch.Platform 程序集。 |
| [CandidateBattleApplicationSystem.cs:28](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs:28) | Submit(PreparedCandidateBattleRequest, SaveStoreBudget, int) 要求该类型；空budget明确拒绝。 |
| 同文件40、43、46、70行 | Retry、Resolve、End、QueryOperation 同样接收 SaveStoreBudget。 |
| [FightMatch.Application.asmdef:4](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/FightMatch.Application.asmdef:4) | 既有Application自己引用Platform；这不把Platform类型改为Application所有，也不构成Presentation新增引用的白名单许可。 |

预期：生产控制器直接使用真实公开预算／019接口，引用其实际所属程序集。
当前包约束：实现这个调用面需要消费 Platform 类型，但允许的 references 集合缺该项；§246只解决旧测试谓词，未覆盖此新引用。
最小修订：只给尚待新建的 FightMatch.Presentation.asmdef 追加一个明确允许的 references 值 "FightMatch.Platform"。保持既有 Platform／Core／Application 的源码与 asmdef 原字节，不移动类型、不改接口、不安装包。

该方向不会建立反向依赖：现有 Platform 仍仅引用 Core、noEngineReferences=true；新增引用来自 Presentation，不放宽 Core／Platform 的 Unity 禁引用。已批准的 §246 精确谓词也继续保留两生产程序集的原限制。

上述是源码与任务规格的静态冲突，不是已运行编译得到 CS0012 或其他错误；R 和本次 C 都没有执行该编译。纠正范围后仍须真正实现及编译，才能证明不存在其他问题。

## §246 例外与旧阻塞的处理

R 复核原 [QFrameworkIntegrationTests.cs:175](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:175) 与§246精确补丁。新 guard 只对 Core.Tests 排除两项 UnityEngine／UnityEditor 禁令；Core／Platform 的 Unity及QFramework禁令、原QFramework谓词、程序集枚举、方法和注册保持。
该两行技术修订与计划中的真实Editor测试宿主一致，已获准；它既不是本轮新阻塞，也不是 G2 已实现的证据。
当前该文件仍11066字节、SHA256=7d7a5712b1184500dcd8349b1542ace0c66c0e8d98c3699d1e622fa74ad2c580；Core.Tests.asmdef仍640字节、SHA256=154205439e7d707c4772b9891f3bd85ebff1179bbe73cfe45cce69983a3a189a。

## B16B 逐项状态

| ID | 独立判断 |
| --- | --- |
| B16B-01 | G1四来源长度／SHA符合签发输入；实际Assets导入、精确friend及83个独立Unity TestCase均NOT RUN，不能用G1历史83通过代替。 |
| B16B-02 | 已读取019真实view／接口与G1上下文契约；新控制器映射、选人零写入、新Attempt取消、Attack／FreeLink／全倒补线接线均NOT RUN。 |
| B16B-03 | 新棋盘坐标、半开边界、阈值单位、末Up样本及真实路线提交NOT RUN。 |
| B16B-04 | VisualElement／Editor panel真实事件、捕获、第二指针、Cancel／CaptureOut／失焦／Detach／重复Up均NOT RUN。已批准的旧断言例外不替代事件验证。 |
| B16B-05 | 单历史Up提交、多历史不可变有序确认及真实回退恢复NOT RUN。 |
| B16B-06 | G2保存失败／不明、原prepared重试、旧结果、Busy无队列、PlayOriginal一次交接及重建NOT RUN；仅沿原019已接收边界。 |
| B16B-07 | 611入口及旧测试保全通过；最终637／345、83独立导入和新增UI测试、B阶段源码／DLL／XML同版验证均NOT RUN。 |

## 证据独立核验

G1四原输入仍为：GestureModels.cs，12570字节、c18257cdecb0f993b27130380b071e2f092ad5c230311e30bbd559c5218e9464；RouteGesture.cs，14019字节、d6a95368ba46a307bac2cf03120a1c5e7a90ed24b94fe7866d91cb3dd1459e30；FightMatch.Input.asmdef，396字节、b12108ce004ea6364923ebb383a7d285846e31db2b1f6b6ccd60eb56f8899f2f；RouteGestureCases.cs，53011字节、7e0b1739d7014e645d4456a1178425d6f9e2c988b4480812830fc158676c114c。
ExternalWork/FM-DEMO-026-G1 的388文件元数据在10:29:56仍与原基线相符，SHA256=f14e51bbccbe719163d468bd7a7c1fd0986a5a65bb325eef9f37e78b6cd5dea7。原C2独立83／23／42结论继承，未重跑。

R 续接入口10:16:00与出口10:29:27独立枚举58个旧根，236693文件、13599367365字节，路径／长度／UTC mtime完全不变；其中包括旧E16但明确排除本次stage026g2-resume1子树。C入口完整元数据与R的58根逐根canonical匹配。未重新哈希或复制13.60GB旧字节。

本轮证据根：D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage026g2-resume1。

- sources/entry的906份实际副本与完整入口实现／Assets／保护输入／DLL联合清单逐项长度和SHA相符，包含611实现及既有Tools；无多余或缺失副本。
- entry-named-cases.json的3183条全部标Passed，fullname Ordinal多重集合SHA256=ab16984d1358fac676a6b770d1de757bccadba72eb3bcee5a20a25387eb56caa，与024最终XML一致。这是继承，不是本轮测试。
- 024最终XML SHA256=0219aec144aaa9ac560ff39a38cc2d02111f6499f6afcb3317be02c88cbc994f；原30个DLL/PDB canonical=3997f0cf1d5c55a80c009cb8ec36e40aebb937d0c53bb8b78676713ae155b9bd。本轮没有新的编译版本或测试结果。
- R于10:28:12逐项读源、payload和ZIP核1821文件／263685743字节；三方canonical均为ab02013651df8e2800ef3580f1f5fe33d7f12fc0d029b58b8c50522a12580a8f。ZIP为34495362字节，SHA256=b6e12c49ff16a3ca8fa433a7d37cd546d464f6aec95eaac681a568d419e78706。
- 实际payload空目录与ZIP目录条目均0；empty-directories.json是0字节空文件，未把它声称为有效的JSON数组，零目录结论由实际枚举支持。后续取证宜输出显式[]。
- 新阶段除archive／late外的915文件全部被归档覆盖；late及两正式报告单独绑定。归档只是预检／保全证据，不是G2实现或Unity测试产物。
- resume1-bindings.json的21项当前长度／SHA全匹配；本体6927字节、SHA256=b68a20ac20c406adaf2daace33d145a919c7bee23b68b67c986bd671ab398d7d，与正式final一致。
- 原生导出的28条执行／文件事实、两项context、task_started均逐条比对准确新C原生文件；导出ordinal按实际JSONL一基行号理解。最后晚绑定命令、正式final及task_complete另外核验；没有导出推理。
- 非阻断口径：一条console把“present=0”标成“absent=0”。gate已注明标签错误；R实际核26目标全不存在，scope与gate均正确记0 present／26 absent，未沿用错误标签。
- Unity B阶段运行数0；Logs/FMDemoB16BCompile.log、Logs/FMDemoB16BTests.log和FMDemoB16B-EditMode.xml均不存在。没有可引用的本轮PID、退出码或G2测试总数。

正式续接文件：

| 文件 | 字节／SHA256 |
| --- | --- |
| [demo-026-g2-resume1-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-g2-resume1-delivery.md) | 7537；a75a598f3f8cd99c9c2b22bb3717ebc8c67c357e9c3a858bc42422451581c8fa |
| [demo-026-g2-resume1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-g2-resume1-scope.json) | 1113422；9f9c4a63fefeab50a0aa9807c64dcb05c4b11c1634d924632673d663d618d332 |
| 原demo-026-g2-delivery.md（历史BLOCKED） | 5253；274e5628821cd4645ae195b37fa030d590c5d8acf3553291e0d1d23fedab3883 |
| 原demo-026-g2-scope.json（历史BLOCKED） | 1004783；e0142087f19fc0589e25f60673c49dded4f7a8d9d325beaef35ead23b1563c35 |

## 最小纠正任务提案：G2程序集引用范围补齐

任务目标：由SD00追加精确技术修订，只把既有FightMatch.Platform列入新Presentation.asmdef的references许可。其余§241／242／246保持，不新增功能包、代码文件、依赖安装或玩法决定。
负责方：SD00签发；原C在新的明确范围下继续。此报告只给纠正提案，不擅自批准或应用作者白名单之外的改动，不发送续接通知。

SD00文档范围建议仅为system-task-packets.md新增纠正节，以及session-plan.md／integration-review.md所需协调记录，均位于既有2026-09-16／2026-09-17目录。原冻结段正文和历史报告原字节保留。新节明确以下四点即可：

1. 在尚待新建的Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef的references白名单中，追加且只追加"FightMatch.Platform"，用于原SaveStoreBudget／019公开签名；其他字段与运行时UnityEditor禁引用不变。
2. 保持§246已准的两项旧修改和原26新项；仍611→637、两旧修改／609保持、全组三旧修改／586保持、新48、345GUID。不得借机修024的时间问题。
3. 新续接另指定证据子根与两正式报告名；原024、首次G2、RESUME1全部源码基线、报告和原始证据只读，不覆盖失败或阻塞历史。
4. 记录新冻结节SHA、准确C续接turn及独立R完成门。预检完整公开签名所用类型与其所属程序集；若发现新的真实范围冲突须具体报告，不用反射、代理提交接口或放宽旧程序集回避。

范围纠正验收：新节必须只增加上述一个现有程序集引用许可，未修改原生产Core／Platform／Application、Packages、原G1或冻结历史；原两行断言例外仍精确。此文档纠正本身不运行Unity，也不代表G2通过。
后续实施验证：原C按已修订的G2目标真正实现26项及两项旧覆盖，再进行B阶段真实编译和无filter全量EditMode；原3183＋83独立CaseIds＋新UI用例全部Passed／零跳过，保留真实事件、回退、保存、播放一次交接证据和源码／DLL／XML同版。缺合法AwaitLinks公开见证仍如实记录。最终由R审实际补丁及新完成门，不能用本轮保全归档或原024全绿替代。

## NOT RUN／NOT VERIFIED 与分包交接

- 本轮G2生产实现、导入、Unity编译／全量测试、实际panel事件及全部B16B行为均NOT RUN；R额外探针也NOT RUN。
- 025正式内容／PRD C／PlayerSave来源、027动画、028完整结束与回退面板、029正式场景／完整Demo，以及交互式Editor／PlayMode／物理鼠标触控均未验证。
- 合法AwaitLinks／全倒补线成功、同Scene永久发布播放令牌及未覆盖保存故障交叉组合沿原边界；§184更早历史时间／stderr等NOT VERIFIED不被本轮抹平。
- 024独立结论见[demo-024-code-review.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-code-review.md)，其P2时间输入问题需单独小包修正。G2引用范围修订不能修复或掩盖024；当前两包均不能放行，不能启动025＋027。
- 本报告SHA256在本次正式final给出；最终写入与复核后R停止修改，SD00主动核准确R完成事件，不依赖任务间通知。
