# DEMO-B06 独立代码审查

FM-DEMO-B06-R1 · R · 2026-09-21

| 包 | 唯一结论 | 限定接收范围 |
| --- | --- | --- |
| FM-DEMO-011（A） | **ACCEPT** | 线路固化、免费补线与面阶段的纯候选决定；不代表完整行动或实际胜利已提交。 |
| FM-DEMO-022（D） | **ACCEPT** | 普通候选开放事实、Challenge／Attempt及首通关系；不代表正式内容发布、资源可用或真实结算完成。 |

两包的规范检查与任务契约检查均通过，未发现需要返修的可复现缺陷。B06满足两包通过的关批条件，交SD00登记；本报告不派发后批。

## 1. 正式审查入口与依据

- A原实施turn `01a0c302-7e91-7ce2-b3c8-8ef26440b270` 于08:47:37 UTC completed；D原实施turn `01a0c303-1f1c-7993-a74b-1238a5f1e8b9` 于08:51:15 UTC completed。R通过原任务状态确认两者完成，收到完整交付后才开始本次正式审查。
- 已完整阅读 [011交付](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-011-delivery.md)（131行）与 [022交付](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-022-delivery.md)（94行）、各自scope、全部9份C#及9份meta，并核对实际日志、XML及原执行事件。
- 依据 [任务包§97～100](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:1648)、AGENTS及.agent规则、已批准交互／战斗历史／进度／成长／应用流程／配置文档。B05已由SD00依据R双ACCEPT接收；未以旧测试结果替代B06验证。
- 开工冻结任务稿SHA256：`00c2945a6b37e6b5f94d682467b7be65dd9e3c22d8dee88938bda641cc6bdef0`；收尾复核当前：`179d0eb3f0fbd5daf62fed450b7f89a3c2e62323ef5f6d55cf423c25ee537105`。
- R逐行比对冻结§97～100，仅1652、1668、1714、1773四行更新派发／交回／审查状态；业务契约、白名单、验收及审查边界均未改变。
- R只新增本报告；未启动Unity、写测试／实现／meta／运行产物、改旧报告或协调稿，未创建任务／子代理或进行Git写入。

## 2. 范围、基线与冻结核验

原始基线为 [009 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-009-scope.json) 的after.files：377项，原时间2026-09-21T07:27:30.2472227Z。整份scope SHA256为 `a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec`。

| 清单 | 项数 | 规范SHA256 |
| --- | ---: | --- |
| 原B05源码／测试清单 | 377 | `8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307` |
| A四份C#冻结，08:30:44.7841659Z | 4 | `da5fe45c21c9d9631f315f67e3aef2b559c76e76ad9d368108d13c02994148c9` |
| D五份C#冻结，08:30:22.4059100Z | 5 | `1b8cf273805460553239b042a7e7bc01e56a7ccbb6a360533acca1d2e6aed063` |
| A联合冻结，08:36:22.6216656Z | 9 | `00efd9b85e6128b86ddef1d22f4be74153740c3ad14ec939747373352ebca456` |
| 编译前，旧377＋新9 C# | 386 | `29ad2b4ff0649682f3252ffb829d5801c38b5f79c39e20e1bae1d9155d41a797` |
| 测试前／两作者after／R现场 | 395 | `28be3756cf902dcfe9b28bc99869c3f8f20447233000a9fe7842d4ac84900b94` |

规范均为Ordinal相对路径＋TAB＋小写SHA256＋LF（含末尾LF），UTF-8无BOM。R重新计算各清单，并逐路径核当前文件；377项旧文件零差异，新增恰好白名单18项，无遗漏或越界源码／测试。

- A startedSnapshot为08:10:40.6207137Z；D为08:11:28.7676538Z，均核377原项且自身新增尚不存在。R于08:11:34.1113454Z独立抓到同样377项；继承before与真实现场时间明确分开。
- R早期保存的A／D初始scope SHA分别为`9e14324631d2bd40c82d0f4cf024f5acb98a629682376ef9bd268c63ae76ce31`／`89711fe82ee12ffb1755db5be79c2013e7d94f5e52e2157f28b5c3f45336fecd`；从最终scope还原原前缀后的SHA完全一致，before／started未事后改写。
- D CODE_READY在前，A核两方共9份C#后冻结；原执行记录1519行确认当时9份meta尚不存在。编译日志226～234行逐项导入同9份源码／GUID；测试前395项已包括Unity生成的9份meta。
- A after时间08:43:35.5982311Z，D after为08:42:47.4327921Z。R现场与编译前386项、测试前395项、两份after、双方及联合冻结清单逐项匹配。
- 实施行数：011为1280 C#＋44 meta＝1324／1800；022为1110 C#＋55 meta＝1165／2000。交付报告均≤180行，scope分别265240／244227字节，均≤307200。
- R读取全部9份标准MonoImporter meta；全Assets共222份meta，9个新GUID各出现一次，无无效GUID或重复GUID。Assets路径从394增至412，仅增加这18项；旧源码meta逐字节保持。
- 47项受保护输入中44项SHA保持，另3项为SD00协调稿。B01～B05旧报告、原XML、依赖锁及ProjectVersion均未改变；BattleSnapshot.cs／CoreTestAccess.cs亦在377项零差异内。
- 既有Git工作区仍为34个已跟踪文件、+2809／-597；其中FlowPuzzle、Packages及本地权限改动先于本批，未归因给011／022。R的只读git diff --check退出0，9份新C#无行尾空白。

## 3. FM-DEMO-011 七项验收

| 验收 | 独立代码判断与实际通过的测试见证 |
| --- | --- |
| ①攻击线结果 | [AfterAttack](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:31)核原活Actor／活目标；最终目标存活则临时线消失，死亡才固化，旧固定线不重发事实。[测试](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:26)覆盖8种生死／全倒／反向组合，保留非零修订、计数、累计、游标、随机及PRD；Real009AttackFeedsStage…另核真实009片段接入。 |
| ②其他死亡与顺序 | [Pending构造](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:49)按原Face.Pairs顺序，完整投影按键读取。[OtherDeathsUseDefinitionOrder…](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:87)交换定义顺序并反转投影，另两敌死亡后可先补后一个；输入明确为合成阶段投影，无AOE／DOT实现声明。 |
| ③全倒免费补线 | [CompleteLink](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:62)无Actor及外部HP字段，只读before HP，固化准确Pending并删除该项。测试87行的全倒分支及276行FreeLinkOnlyAccepts…覆盖余Pending→AwaitLinks、补完有活敌→AwaitRescue，拒绝已锁／存活／别面／不存在Pair。 |
| ④翻面与终局 | [Finish](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:97)严格按Pending、活敌、准确下一面、最终胜利的优先级选择；翻面用新面初始Board，保留旧面完整HP依据。[ClearingTheLastLine…](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:120)含双面／三面只翻一面；151行最终面全倒补线仍得WonPendingSettlement且无需满盘。 |
| ⑤前态与覆盖 | [CheckBefore](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:152)核面引用、原参与者／敌人／贡献覆盖、死敌与锁线／Pending恰一归属；233行逐键核HP且不得高于前态。[覆盖测试](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:315)及349、403、428行分别覆盖漏重错键、负／超Max／增HP／复活、错面、集合冲突、修订与阶段。拒绝Decision=null。 |
| ⑥几何 | [CheckRoute](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:249)调用既有001单步校验，旧固定线也逐条校验互斥。[几何见证](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:436)保留越界／斜线／自交／外端点／重叠／长度／端点原ReasonCode和CellIndex；474行反向且堵未来完整解仍接受，无solver或FlowSolutionValidator依赖。 |
| ⑦隔离与预算 | [数值重验](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs:287)用同一budget遍历Before／Baseline／投影保留值；请求与HP输出复制为只读集合。[预算及隔离测试](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs:492)覆盖旧面／未用新面数值、大修订／累计／随机／PRD、分子分母、共享／末步Limit及新预算一致；587行验证深只读与壳隔离。 |

阶段决定保留原BeforeSnapshot／Baseline及不可变请求来源，事实以原OperationId／SceneRevision／Face／Pair与段内序号定位；详见 [Decision和事实](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateStageDecision.cs:11)。没有新业务ID、完整PostSnapshot、FinalAttemptReport或Completed，也不推进战斗数值。

## 4. FM-DEMO-022 七项验收

| 验收 | 独立代码判断与实际通过的测试见证 |
| --- | --- |
| ①规则与查询 | [PrepareDefinition](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:11)核完整Context、显式Ordinary／空能力、唯一Level／Rule、显式null前置、至少一初始节点及无环；Read只按定义读取。[规则测试](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:15)、33、61、74行覆盖28组非法定义、P-A初始、P-B锁定、稳定顺序与只开放直接后继，无可执行资源声明。 |
| ②入场与角色 | [CheckEntry](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:86)与215行Entry使用同一budget的020公开ComputeBaseStats，核Player／完整Context／Warrior／角色／修订／原槽与IsReady。[公开M03见证](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:86)真实建立Ready→Recovering→恢复完成；100、121、129行核错身份及槽0，不改角色、不引入推荐等级锁关。 |
| ③挑战复用 | [BeginAttempt新办理](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:116)只复用同关未关闭Challenge，否则要求全历史未用ChallengeId；Entry阻止第二个活动Attempt。[ExitKeepsChallenge…](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:142)及156、248行覆盖退出后原Challenge／新Attempt、强换与跨关复用拒绝、多关未闭Challenge和全局单一活动Attempt。 |
| ④胜利和首通 | [胜利关系](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:182)只关闭当前准确Challenge，首通一次、保留原结束依据，按显式边新增开放事实。[WholeVictoryRetainsUniqueFirstClear…](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:172)覆盖原Context／Version／Settlement／指纹、准确重试零新增、新挑战重打不替换首通与开放来源；P-A不能自动开P-C。 |
| ⑤退出与重来 | [重来构造](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:185)保留原Challenge／EntryBaseline／Participant，仅新建未使用Attempt；普通退出不关闭Challenge、不推进进度。[RestartTransfersOriginalEntryBasis…](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:195)和215行核旧结束重试不再生成、最新恢复状态不替代原依据、现用／历史ID均拒绝。 |
| ⑥来源与原子性 | [Begin原收讫](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs:105)先比固定意图；157行End按原Attempt完整结束事实防重，再核新办理的修订及当前关系，175行核全历史Settlement／EndReceipt唯一。[来源测试](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:225)、241、280、288、326行核已结束／恢复后准确重试、异意图／相反结束／复用结算／错指纹／身份／修订均全旧；156行确认别关Challenge不被关闭。 |
| ⑦隔离与预算 | [保留数值重验](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs:122)覆盖定义／状态及所有历史Attempt的M03修订和槽位；247行Change以同一budget做修订+1后才返回。[隔离与Limit见证](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs:337)、355、370、390、400行核可变壳隔离、历史大修订／M03输入、六入口末步Limit、4位修订溢出无半份首通／关闭，新预算一致。 |

所有状态／收讫集合只读，首通及开放来源保留准确旧引用，见 [CandidateProgressionState](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs:16)。D实现与测试只用既有公开M03能力，未调用GrowthChecks或friend构造M03状态，也未消费011／021候选API。

## 5. 实际联合验证

R读取A原始任务JSONL的CommandExecution事件，对照scope嵌入的完整命令、stdout、退出码及时间，逐字段一致；原记录不是交付文案中的等价命令。

| 运行 | 原事件位置与ID | 实际Unity结果（UTC） |
| --- | --- | --- |
| 编译／导入 | A JSONL第1530行；`exec-353a4a0a-5467-40e8-b137-98937301ebf4` | PID48284，08:37:01.2397521→08:37:14.3811806；ExitCode=0 |
| 全量EditMode | A JSONL第1554行；`exec-bec3ad5b-8f61-44e4-97cd-026f39e42ce2` | PID11804，08:39:25.0913875→08:39:40.9461140；ExitCode=0 |

两次使用固定D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，均先检查Get-Process Unity为空并逐项复核输入SHA；Start-Process使用Hidden／PassThru，WaitForExit后Refresh读取实际ExitCode。编译参数含-batchmode -nographics -quit；测试含-runTests -testPlatform EditMode及固定XML／日志路径，不含-quit。

原事件来源：[A原任务执行记录](C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T11-20-56-01a0c1fb-248d-7321-8b69-9efd26b817f1.jsonl:1530)

- 原A turn共34个命令完成事件，D共27个；已核两方写文件事件及进程相关命令，A仅上述两次Unity启动，D未启动Unity、两方未杀进程或手写meta，冻结后无源码修改。
- A两次非零命令分别是静态禁用符号rg未命中和Get-CimInstance拒绝访问；后者改用Get-Process并在实际启动前再次检查成功。没有失败Unity运行或测试失败后未重验的代码修补。
- 原preTestSnapshot的两项总行数因shell字典管道得到null；原记录保留，A于08:39:12.8606315Z逐文件重算并追加preTestLineBudgetCheck=1324／1165。原事件1549行证实更正发生在08:39:25测试启动之前；R再次独立重算一致。
- 日志无C# error／warning、Compilation failed或Unhandled Exception。编译内部Bee的ExitCode 4对应日志203～204行要求重建DAG，随后编译成功、Unity实际退出0；许可客户端及无图形渲染提示未被误报为干净无消息日志。

| 实际产物 | SHA256 |
| --- | --- |
| [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB06Compile.log) | `7a901d0c28818c47130388a8a38356861e884f78c15468195eba28ae35425110` |
| [测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB06Tests.log) | `ed2e074e39201191ebfb333cf995e4143ca8e94460e28c3e31280548efc74f5e` |
| [B06实际XML](D:/Unity/UnityProj/FightMatch/FMDemoB06-EditMode.xml) | `a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e` |

XML实际区间08:39:32Z～08:39:39Z，总计1667／1667 Passed，failed／skipped／inconclusive均0。R独立遍历test-case，不只读取根节点计数。

| 集合 | 实际结果 |
| --- | --- |
| B05原测试 | 1414条、1409个Ordinal fullname；每名出现次数及Passed次数完整保持，无减少／改名。 |
| CandidateBattleStageTests | 新161条，28组测试方法，全部Passed；对应§3七项。 |
| CandidateProgressionTests | 新92条，28组测试方法，全部Passed；对应§4七项。 |
| 其他新增测试 | 0条。 |

原重复名称仍为BattleRouteValidator的EndpointMismatch参数组×4及CandidateCharacterGrowth的CritBase／CritCap参数组各×2，全部通过。fullname＋TAB＋count＋LF规范SHA为`9f02563b07c81569d758b57e3a687736ede5de25c51667bc53970c26959e8c4e`；追加TAB＋passedCount的规范SHA为`5d268d74d4fc4c0069ba98adf69186f11c783fd0047e9717a00db1ea65fbb63d`。两种口径不同，R分别重算并逐名比较通过。

## 6. 已审新文件指纹

下列9份C#及对应9份meta均已完整阅读；行数为实际文件行数，SHA256均取当前字节。

| C#文件 | 行数 | SHA256 |
| --- | ---: | --- |
| [CandidateBattleStage.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs) | 361 | `49df8f388234fe1e0549a9fa7c17c1af534b195b718bdb2c13eaae4396516397` |
| [CandidateProgression.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs) | 256 | `5e7863f065e3014722c8ec59647c4a1fb9982472ab632c0a843301245b395b6b` |
| [CandidateProgressionDefinition.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs) | 134 | `5f9bb98dd191fd30672c519897d1fb3b7b262b895da1b646a3172f478e047227` |
| [CandidateProgressionRequests.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionRequests.cs) | 44 | `14dc1da29930abb81042de5d963db0213c2a1a1d7394c1cda37ad800c1262dce` |
| [CandidateProgressionState.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs) | 161 | `c1e7caabee302a8f8fb833fefce85aa9e08a11cf3532d1aabac2de6ff920f44f` |
| [CandidateStageDecision.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateStageDecision.cs) | 77 | `91f2da1aaeb429c432e9b7e4b793b4f595da4dd1253c40ebb2cfa54fbcf06589` |
| [CandidateStageRequests.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateStageRequests.cs) | 73 | `e75486f7762148c4be35dfcdfa0c76c1925a9ce3809575ac35d3851c7a65d4c1` |
| [CandidateBattleStageTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs) | 769 | `c97fefd71e86c4027610dfc2667c931e324352c1a2f6230bcd6d2421d4098a9b` |
| [CandidateProgressionTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs) | 515 | `3281992c87ba5110b7e26dd9ffd5ad0ca2793816a20e728254ab58289cb1d2b8` |

| meta文件（各11行） | GUID | SHA256 |
| --- | --- | --- |
| [CandidateBattleStage.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs.meta) | `1c264dea39eff8149882ec390ca94425` | `5471dec04f93fdbf297b0db7cc18150ac5bd7eda309fd3aa9c5b220bd90f6aeb` |
| [CandidateProgression.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgression.cs.meta) | `a08aa557aed74d14296c3fdcec953191` | `1797fecabe31515fbab6c18be08f999c9fce9b8a8f711f8c5108196181ea964f` |
| [CandidateProgressionDefinition.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs.meta) | `5122f868670fef348ad315bfd0a78d7d` | `a2530e2576845fc088e09b98cdca0728172ebcdb28ea84a4bbf90d93619ca20a` |
| [CandidateProgressionRequests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionRequests.cs.meta) | `55e2a683a0fe0364cb6455d8c22850e6` | `c461ac2b5dc5e8c3f79ca2568d541a47577274fb595b938dfc0cb14384e4e28d` |
| [CandidateProgressionState.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs.meta) | `0747afc4eebc9c34ab2b02133c60e557` | `83b83db4d4fe0e6485d056e745305e4163b2352ed63c0477c6e0f6d76b4e14b4` |
| [CandidateStageDecision.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateStageDecision.cs.meta) | `df75bc9dda2ea684da2bb1493ce84333` | `374b4ed5a19c35ae398fee4f1f693474496c321b3f7f0e9353dfcd8437c8daa7` |
| [CandidateStageRequests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateStageRequests.cs.meta) | `6e64a05934f930b4da2ca2ef043dbea7` | `a1e90a226f07b1a08de0c21e913520db7eed499e469ce097cc20da25642a75d8` |
| [CandidateBattleStageTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs.meta) | `4615dd06ac298204cbd3484a48c43839` | `f488900deb9e7f9145b74784026d69f67ebe36ecf3831f11cf9560ae815f4c98` |
| [CandidateProgressionTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs.meta) | `fc576b65d9ab81e499890637c8f3176c` | `997590d422c073422de1c40c1b6a88a57809609900111712e83a25a796f28ba3` |

| 作者交付／scope | SHA256 |
| --- | --- |
| [011交付](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-011-delivery.md) | `0882f13ec45e0af19ca2cb6b5b5def32f6532f48cc17d884b696e1579ff07b65` |
| [011 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-011-scope.json) | `772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859` |
| [022交付](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-022-delivery.md) | `8675a64fc437ccc392251e9d3b6fe643cafdf03d0d4c50277cba073b395b54eb` |
| [022 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-022-scope.json) | `f7890d0f7f267f522b52049a5b6135474733b6c913451acfd9e8f63a76966a17` |

## 7. 接线限制与当前动作

011的最终HP只代表候选输入；012必须与真实CombatFrame核一致，并完成敌方阶段／行动片段的统一组织和必要推进。022的NormalVictory只核候选关系，012／023仍须校验真实报告及结算来源；M02还须核M06／H06和M10／M14内容可用性。

022测试P-A→P-B→P-C是隔离规则，不是已发布L1→L3链；025仍需提供真实可发布节点、前置、规则ID与绑定。以上是已批准的后续责任，不构成本批返修项。

没有修正包或新增用户动作。R将本报告路径／SHA、两包限定结论及实际验证摘要回传SD00，由SD00登记本批结论。
