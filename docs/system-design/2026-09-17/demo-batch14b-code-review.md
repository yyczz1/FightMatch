# DEMO-B14B-R1 · FM-DEMO-018B 独立审查

VERDICT: NEEDS_FIX
Scope: PASS
Acceptance criteria: FAIL — R1 恢复倒退结果未绑定到本次意图时间。
Verification: 已独立核对 C 的有效及失败 Unity 运行、2944 项测试、冻结源文件及完整归档；R 未运行 Unity 或项目代码。

## 审查对象与正式交付门

- 仅审 FM-DEMO-018B；R 任务为 `01a0c1cd-dce1-7ac3-8780-06163cb0acfc`。
- 准确 R turn：`01a0c862-8b16-7360-89b1-9c33673e5238`；原生 task_started：2026-09-22T09:11:36.501Z。
- C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`。
- 准确 C 原实施 turn：`01a0c861-71eb-7be1-9675-bce27f7a8645`；formal final 为 10:30:53.709Z，task_complete 为 10:30:53.900Z，completed／error=null。
- R 经 wait_threads 与准确原 turn 原生事件确认上述门；此前只冻结旧基线、读既有接口和等待，没有以文件出现、旧018A completed或通知 turn代替门。
- 派发 r95：`system-task-packets.md` SHA256=`597f84b8c03ef58caffa6b5e6eb3e1f52a4b93dc0d05b2d876a0872d5c2dfe38`；R 于09:11:57.2472929Z独立命中。
- §188～193 冻结文本 UTF-8 TrimEnd SHA256=`b7c41bb2fd6b6fc4d174ec2fcee4eac96de0ba8c79fad1c73b90aa9c3aedeed3`，与审查时协调稿相同；后续018C设计没有进入本次验收。
- [C交付报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018b-delivery.md)：9118字节／106行，SHA256=`551017d665de871bf3504194c06a4e0f8cc3aa5eeebb3e1b87f178ad5bcb01bb`。
- [C范围清单](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018b-scope.json)：1512588字节，小于1572864；SHA256=`26375dd34742298c69869491f47f7e9a540f07906ad6d8b747b62ecb2e795f65`。
- 本文证据根简称 E：`TestArtifacts/FMDemoB14B/522f3781d7064ad6bbe024cfc41bc983/`。所有时间均为2026-09-22 UTC。

## 规范轴

| 核对项 | 独立结果 |
| --- | --- |
| 精确变更面 | 新增10份C#及10份meta恰好等于§191白名单；旧520实施文件和533个旧Assets逐项不变，无旧接口、格式、配置或QFramework改动。 |
| 结构与预算 | 五个指定业务入口、九种封闭意图、内部构造只读产物；没有新增持久写门、队列、发布器或依赖。生产1708行／全部2746行，低于2600／4600；证据帮助脚本121行，低于260。 |
| 归属与风格 | 逐份读完六份Core和四份测试；测试使用既有公开领域操作构建结果。未以重复压缩语句规避行数限额，未见无关重构。 |
| 验证责任 | C为唯一Unity执行者；R只读检查源码、原生执行事件、进程证据、XML和归档。R未调用外部模型、子代理或额外验证工程。 |
| 工作区既有差异 | Git仍有历史FlowPuzzle／Packages／本地权限等改动；旧文件SHA与R入口基线相同，不能把整仓相对HEAD的既有差异归给本包。只读git diff --check exit0，未据此替代完整哈希审计。 |

## 规格轴与B01～B08

| 项 | 结果及对应实现／测试 |
| --- | --- |
| B01 | PASS。103项意图测试；九Kind、payload唯一性、关键字段／次序、完整Context、数值和时间缺省语义、只读输出。IntentCodec沿旧BusinessFields宽度、UTF-16原码元、规范整数／有理数；完整100字节常量及固定SHA构成独立oracle。 |
| B02 | PASS。ProtocolTests第14～76行覆盖1→2→3代、候选未绑定、旧当前行补完整六片摘要、错误历史／Parent／ID／多操作拒绝；Bind和CheckIndex逐项一致，basis未变。 |
| B03 | PASS。Find先比较Player／OperationId／全意图字节，Lookup不套用当前ExpectedCommit；旧结果保留原Commit／Generation／入场对象。新ID旧ExpectedCommit另行StaleContext，已有同ID先AlreadyRecorded／Conflict。 |
| B04 | PASS。真实Attack→回退→新分支→再次回退→退出→新Attempt→Restart链；Validate按原记录顺序取第一次真实Range，核活动Archive，保留结束Run与回退两端，缺根／缺记录／假Anchor／夹带行动或所有者变更拒绝。 |
| B05 | 有缺口：四个合法RecoveryClock outcome及历史sample／elapsed／revision／双Anomaly均有通过用例；未覆盖R1“真实倒退结果配上非倒退意图”的交叉输入，当前实现会接受。 |
| B06 | PASS。真实终局固定AwaitBaseSettlement路由，Lookup不结算；H06必须使用预留ID并拥有完整胜利收讫，另一个提交消费路由；错误身份／缺路由／多路由／ID碰撞和未收讫消费均有拒绝证据。 |
| B07 | 编码结构、旧五片兼容、聚合预算及一般畸形拒绝通过；恢复记录的语义校验沿用R1缺口，不能宣称所有跨片历史都已验证完毕。 |
| B08 | PASS。三轮编译、三轮测试实际进程证据完整；最终同版exit0、2944全部Passed；计数、旧文件、GUID、scope压缩清单和12993项归档独立核验相符。 |

旧五片编码器文件及API不变；新Encode先取其五片字节，再量出含M02要求的六片元数据与总量，分配M02前执行总限额检查。Decode严格六片、用内部五片投影恢复领域对象，并由原六片Write(Stream.Null)取得Descriptor；合法旧五片明确MissingApplicationRecords。M02字段顺序、九Kind结果布局、代际与提交索引一一对应、Requirements首次出现顺序及FeatureId均逐项读核；调用者同一个budget／Math贯穿嵌套调用，Limit诊断沿用。

成功历史链选用Attack满足B04“至少一条含攻击／补线”的要求。C明确未构造真实Link成功链；既有测试显示低层合成Link不能成为公开完整Run历史，因此这里不把合成成功冒充验证。Context沿既有PreparedCandidateContext的七个具名字段和原布局；§189“八字段”的计数笔误没有导致删字段或臆造第八字段。上述两点不是本次阻断项。

## R1 · P2：拒绝与本次时间不符的恢复倒退结果

位置：[CandidateApplicationReferences.cs:322](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs:322)，重点322～323；关联[CandidateApplicationProtocol.cs:248](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationProtocol.cs:248)的248～270及References的451～469。

`Recovery`对IgnoredTimeRegression只要求ResultAnomaly含ClockBackward且包含Period.Anomaly；`RecoveryTransition`只要求同一个未变Character／Period和正确修订。两处均不验证本次意图的TimeSample是否真的早于原LastAcceptedSample，也不核由该Sample决定的异常组合。真实、IsAccepted且Next正确的另一次倒退结果可因此配给一个本应推进的意图，违反§188本次结果及§189意图执行依据／原真实结果绑定。

可复现输入（沿现有ApplicationScenario夹具，所有成功对象均来自公开领域操作）：

1. `new ApplicationScenario(3, hp: 1)`；依次`Enter()`、`Attack("down", 0)`、`Exit(down: true)`。取`basis=s.Head`。
2. 此时角色修订2、RecoveryId为`recovery`，Period.Elapsed=0；LastAcceptedSample为Wall=100、Observed=101、Monotonic=0、Scope=`clock-original`、Source=`test-clock`、DeviceUntrusted、Anomaly=None；尚未完成。
3. 在该Character上真实调用`CandidateRecoveryClock.Advance(character, "recovery", ApplicationScenario.Clock(99), 2, Math())`。Clock(99)为Wall=99／Observed=100／单调值与域均显式null；原算法返回IgnoredTimeRegression，Next与原Character同实例、Period不变、ResultAnomaly=ClockBackward|DomainChanged（3）。
4. 新意图OperationId=`recover-future`，Player／Character／Context沿basis，ExpectedCommitId=basis.Header.CommitId，Kind=AdvanceRecovery，RecoveryId=`recovery`，ExpectedCharacterRevision=2；但TimeSample改用`ApplicationScenario.Clock(101)`（Wall=101／Observed=102／单调值与域均显式null，其余同前）。
5. PrepareIntent后调用`Propose(basis, basis.Business, prepared, new CandidateApplicationResultInput { RecoveryResult = actualFrom99 }, null, Budget())`；后续沿夹具Header完成Encode→Decode→Lookup。

**预期：** 第5步结构化拒绝，例如InconsistentBinding，不创建完成记录。若原RecoveryClock真正执行Clock(101)，按100→101推进1毫秒，返回Applied、修订3；它不可能产生第3步的Ignored结果。

**当前实际（逐条件源码推导，R未执行该新增用例）：** 第5步的IsAccepted／Next引用／Period引用／Context／修订／双异常范围均满足；References 322～323只看3含ClockBackward且旧Period异常为0，OwnerHistory保持修订2和原Period也通过。因此Propose可接受，Encode／Decode重复同一不足检查，Lookup会把Clock(101)返回为已记录的Ignored结果。同OperationId重试优先命中该错误完成记录，无法补做原本应发生的推进。

该输入不构造内部成功对象、不修改旧领域算法，也不需要伪造文件摘要。现有35项Protocol测试只按同一个sample生成和登记真实结果；第273～290行的“不相关actual”用例因Next不一致被拒，覆盖不到此处Next相同的错误组合。

## 独立范围与入口基线证据

- R于09:13:45.5868255Z独立读取：旧520／Assets533／GUID285一致，27个本包目标均不存在；这是R自己的入口观察，未将SD00更早的派发前证据冒称为R证据。
- 旧520 canonical=`fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6`；旧533 Assets canonical=`39040a3188007e521954baa18b56a4652be00ca45bf181d0f2d616085e46304f`。
- R在09:14:53～09:15:26完整枚举21个既有Artifacts根／14047文件；10:36:44重枚举每根路径集合、SHA及数量均相同，压缩清单中的每项长度也核对；无reparse。
- 另独立冻结的106份旧根XML／日志／工具输出于10:36:40仍不变，canonical=`dde0803c50acfc722a24f1e9f5dbbf694cc9d3a6e7b692cb14173ed910681d47`。
- R检查453个既有文档／规则／配置／权限文件，仅三份协调稿变化；SD00原生FileChange在09:13:42、09:18:19～20、09:20:01及09:20:50明确归属这些变化。冻结§188～193不变，C原turn修改范围不含这些协调稿。
- 当前实施540、Scripts＋Tests526、完整Assets553、meta295／唯一GUID295；现有文件逐项命中scope，新增集合无多项或遗漏。
- 当前540 canonical=`466dda28a5316343cf90f0021c64ffaf3b61c0ab22c99ce9bb7f5a4f8a13096e`；553 Assets canonical=`f33da595e89eca0ed67f431ef5c5d0b07db6428c78ab35dff548a6f8b5af3948`。
- scope.beforeImplementation逐字段等于018A完整520清单，包括原capturedAtUtc=`2026-09-22T07:10:46.6968667Z`；另有真实startedSnapshot=`2026-09-22T09:16:52.4364578Z`，没有重标旧时间。
- 首轮编译前为旧520＋新10C#共530，Unity导入后才有10新meta共540；后续最终四份前后清单540项相同。未将新meta生成误记为旧meta变更，也未声称其此前存在。
- scope的Brotli内容解码为10655101字节，SHA256=`8e4106e856e8f523601076528adff5404637ea1c9c440e002db65cc56352a2a8`；完整旧Artifacts／保护输入／新旧Assets／meta／自然输出／archiveSource／startedSnapshot均解码核对，无省略行替代。

## Unity与同版证据

C唯一执行的引擎是`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`，cwd及projectPath是`D:/Unity/UnityProj/FightMatch`。六次均先保存无Unity进程证据、Hidden／PassThru启动，WaitForExit后先记录真实Process.ExitCode，再收日志／源码／DLL；各PID、UTC、参数与原turn CommandExecution相符。

| 轮次 | 编译PID／实际exit／UTC | 测试PID／实际exit／UTC | XML实读 |
| --- | --- | --- | --- |
| 01 | 28560／0；09:55:48.6842382～09:56:24.7979043 | 36232／2；09:56:51.7150718～09:58:07.3179648 | 2936/2943 Passed，7失败，均为Transition.Challenges |
| 02 | 35432／0；10:00:39.5055426～10:00:49.0015515 | 31824／2；10:01:36.5099804～10:02:47.3886222 | 2943/2944 Passed，1失败，NUnit Int32比较异常 |
| 03 | 37624／0；10:03:48.2425131～10:03:58.4285357 | 18176／0；10:04:52.5297326～10:06:03.7377621 | 2944/2944 Passed，零失败／跳过 |

原生最终编译事件`exec-919d273e-9822-44da-a848-efaa8b608ff1`、测试事件`exec-a81f4a6f-3658-40bc-8d8b-f5e009c85eb0`均wrapper exit0。前两轮测试wrapper exit1与Unity actual exit2分别保留，未混淆或事后填0。

最终完整参数：

```text
-batchmode -nographics -quit -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14BCompile.log
-batchmode -nographics -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB14B-EditMode.xml -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14BTests.log
```

最后C#修改原生时间10:03:41.333Z，早于最终编译。R逐项核验全部六轮before／after源码与191个保护输入副本、28个DLL／PDB副本；最终编译前后、测试前后540源文件及当前文件相同，最终compile／test／当前28程序集canonical=`e4e7aa3de366610a549c9c1b3f28759d00069685b6ab34a069dc001c1f8ea653`。各轮compiler error／Compilation failed／Unhandled Exception为0；当前日志与最终归档日志相同。

最终XML2005804字节，SHA256=`60a89e5366a23fd374d7da0df2d1e271578e3df0918df43014fc7ed4a41e5808`。原XML的2772个case／2767个不同fullname，按Ordinal名称与重数完整保留且全部Passed；名称多重集合canonical=`9b4fb5cff20f9a714892aff390d840fc4e6a8c7e86a5ab9feb0a9f0efca8d5ac`。新增172项恰为Intent103＋Protocol35＋SaveCodec34，均Passed。测试通过不能覆盖尚未写入测试的R1反例。

## 归档核验

R于10:40:25.2139119Z完成独立实际目录及ZIP流枚举，没有以作者清单充当实际枚举结果：Assets、本次E根、相对R入口新增的六个B12／B13 Guid根（3165文件），另加明确保护输入、工具源、最终程序集和自然日志／XML；payload另行遍历，ZIP逐条读到末尾计算真实长度和SHA。

- 实际source／payload／ZIP均12993项；ZIP展开486819779字节；三方集合缺失、额外、字节差异、重复、大小写冲突、路径越界、条目长度错误、reparse均0。
- 三方canonical均`ceff065752ba5869dfed18144484e96e70f729f2fce5841716a15b3d9e9c7d50`；三个作者manifest另与实际集合比对，也无差异。
- ZIP100792893字节，SHA256=`22da328d9fa1bf69a0aced35ad578826800404b8845b8aded80b09feee1d5550`。
- 自引用排除明确为E/archive、E/late及最终两报告；旧失败输出与其源码／程序集均在ZIP中。E/late另经最终交付SHA和原生事件绑定，未宣称ZIP包含它自己。
- 原生71个CommandExecution中，五份导出联合覆盖69个且逐行等于准确原turn事件；余下为导出自身及末端哈希读取，前者的原请求／返回由final-tool-evidence绑定，后者由正式final的哈希绑定。最终门及导出请求、退出、返回文本与原生事件一致。
- E/late/final-tool-evidence.json SHA256=`13c59c68be38a53feeddda4e5cdecd6d22bc0688bb386916633a60679c144e15`，独立实读命中C正式final；未把chunk_id当真实退出证据。

## 最小纠正任务包建议：FM-DEMO-018B-C1

本节交SD00作为后续派发依据；本次R不执行纠正，也不自动推进018C。目标仅关闭R1，不重做九类协议或旧领域算法。

**允许修改的现有代码，仅三份：**

1. `Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs`
2. `Assets/Tests/EditMode/FightMatch/CandidateApplicationProtocolTests.cs`
3. `Assets/Tests/EditMode/FightMatch/CandidateApplicationSaveCodecTests.cs`

**允许新建的交付与自然证据：** `docs/system-design/2026-09-17/demo-018b-c1-delivery.md`（≤220行）、`demo-018b-c1-scope.json`（同目录，≤1536KiB）；`Logs/FMDemoB14BC1Compile.log`、`Logs/FMDemoB14BC1Tests.log`、`Logs/FMDemoB14BC1RedTests.log`；根`FMDemoB14BC1-EditMode.xml`及`FMDemoB14BC1-Red-EditMode.xml`；`TestArtifacts/FMDemoB14B/<全新GuidN>/`。既有回归测试的B12／B13仅允许新增自然Guid根，旧根只读。

**必须完成：**

- 先补可复现R1的回归，用同一basis上真实Clock(99)结果配Clock(101)意图，证明旧实现错误接受；修正后在Propose拒绝且basis不变。不要以合成内部结果或仅比较writer／reader替代真实领域oracle。
- 对IgnoredTimeRegression按原“同单调域使用单调值，否则使用Wall”规则核输入确实倒退，并核由旧Period.Anomaly、输入Anomaly、域变化和ClockBackward构成的准确ResultAnomaly。只做精确比较与一致性检查，不调用Advance、不读取系统钟、不重算／修补已保存Elapsed。
- 同时覆盖真实负Wall、同域单调倒退、跨域／无单调域倒退的合法保留行为；非倒退／同样本冒充Ignored、遗漏或额外异常位应拒绝，保留已完成期AlreadyIncluded优先语义。
- 增加六片Decode畸形历史用例：从真实合法Ignored存档，仅将M02该意图改为结构合法的非倒退时间、保持旧结果，重新封装有效Envelope；Decode必须结构化拒绝，不能返回半份Snapshot。不得改变FMINT001、M02 schema、公开API或既有五片字节。
- C唯一运行Unity。用新增定向回归先记录red，再在最后源码修改后完成既有2022.3.18f1批模式编译及全量EditMode，测试命令不带-quit；Green路径使用本节C1日志／XML名，其余命令参数沿本报告最终命令。原2944个fullname及重数全部Passed，新增全Passed、零跳过。
- 完整保留本次B14B有效／失败证据、两份C报告、本R报告和所有旧产物；新证据不得覆盖旧日志／XML。beforeImplementation继承当前540清单及真实捕获时间，另记start；除上述3个C#外其余537项不变、原520全部不变；仍为540／Assets553／GUID295。
- 维持原生产2600／全部4600行限额；完整进程／UTC／真实exit／源码-DLL-XML冻结和独立source／payload／ZIP核验照§191；交付逐条回应R1及新增回归，随后停止等待新的独立审查。

**禁止修改：** 其余7份018B C#、所有旧领域源码／旧测试／meta／asmdef／QFramework、Packages、ProjectSettings、场景／资源、旧报告／scope／证据、协调三稿及权限；禁止Git写操作、额外SDK／probe工程／外部模型或顺带接018C。不能在允许三份代码内完成时先报告BLOCKED，不扩大白名单。

## 边界

R仅创建本审查文件；无实现、测试、作者报告修订或验证工程。R1为独立静态代码结论，新增反例运行明确NOT RUN，由纠正包C产生红／绿证据。018C、019、024、025、026-G2、018整包、PlayerSave／真实M12写门、Domain Reload、场景／触控／029体验及正式发布均NOT RUN；本结论不改变已接收包或功能完成数量。
