# DEMO-B08-R1 独立代码审查

VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS
Verification: 独立阅读全部12份实施文件；核验原始Unity进程回执、同版源码、1911/1911 Passed及原1802条逐名保留。
Notes: 无待修正缺陷；范围限012当前支持域，非存档、奖励或可玩Demo验收。

审查对象：FM-DEMO-012「完整行动与封闭报告」，冻结依据system-task-packets.md r73 §108～111。
R任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；原项目local，gpt-6-astra／max。
A任务：01a0c1fb-248d-7321-8b69-9efd26b817f1；012原实施turn：01a0c384-70db-7790-9bbb-12948f05fc65。
工具确认该turn于2026-09-21 11:16:27 UTC completed、任务idle；完整交付同时到齐后才开始正式审查。
未以旧010完成状态替代本次门槛。本文核验时间截至2026-09-21 11:26:37 UTC。

## Standards · 规范

按AGENTS.md、.agent/PROJECT_CONTEXT.md、CODING_RULES.md、REVIEW_CHECKLIST.md及VALIDATION.md逐项检查，未发现规范缺陷。
按code-review技能分开记录规范与规格；本包明确禁止子代理，两个维度均由R独立完成。
固定点为已接收010 scope的403项SHA清单；未将工作区既有未提交改动当成本包差异。

- 只新增五份Core、一份EditMode测试及各自Unity meta，12项均在白名单；未修改403个旧源码／测试／meta。
- Core只依赖既有纯C#类型、FlowPos及.NET库；没有UnityEditor／运行时引擎耦合、依赖、程序集或配置变化。
- 原009／010／011公共接口未改；新增公开入口、条件和只读输出均在§109许可范围。
- 固定编码器内部辅助形状用于数值复验与完整值比较；没有反射生产序列化、对象摘要、持久化或服务接口。
- 源码、测试1628行＋六meta共66行＝1694行≤2600。作者交付150行≤190；scope252014字节≤307200。
- 六meta均为标准MonoImporter、executionOrder=0；实际作者FileChange记录未写meta，生成时点位于Unity导入后。
- 既有Git差异仍为34 files changed, 2809 insertions(+), 597 deletions(-)；只读diff --check退出0。
- 原有FlowPuzzle／Packages／本地权限差异未混入本包；无Git写入、分支、worktree、新任务、子代理或辅助脚本。

R独立保存64份保护输入，正式开审时全部相同；终核61份非协调输入仍相同。
三协调稿由SD00更新接收／审查状态；逐行比较§108～111，仅状态和实际turn登记变化，业务、白名单及验收保持。
该状态更新不归因于A。作者32条原始CommandExecution与九次FileChange均已核查，源码最后修改早于冻结。
本批可按12份新实施文件及作者两份交付文件整体撤回；本次未执行回滚，既有文件和旧报告须保留。

## Spec · 规格

未发现缺失、错误实现或范围扩展。七项验收均有独立对应证据；下表行号指本次冻结源码／测试。

| 验收 | R代码核查 | 同版测试证据 |
| --- | --- | --- |
| ①原入场及链 | Operations 11～18、155～187、295～370：Create只取Binding.Start；重验准备资料、三域初态、PRD及原入场快照；完整有效前缀按Ordinal操作身份核验，允许仅提高跨记录修订号。 | CreationRetains…2、CreationRejects…8、RandomBindingDomains…6；RestoredHigherRevisions…3覆盖0／1／2前缀与2^80修订；根null9、条件／时间6、009拒绝7、重复／终局2。 |
| ②真实完整行动 | Operations 21～46实际新攻击只执行一次009→010→011，010最终HP逐键交011；135～152只生成一次后态，修订／行动／敌阶段各+1。 | PublicSourceLevelsCloseOnlyAfterTheRealFinalOperation 4例，从公开准备、008、Create和EvaluateAttack完成；L1两击W95、Dealt30／Taken5；L3三步W90、Dealt35／Taken10，首步Charge及两次E01承伤。 |
| ③阶段与新面 | 135～152：翻面只初始化准确NextFace的敌HP／游标0，保留原入场、成员、贡献和Random，不再执行新面敌人。保留010致死事实与未执行游标。 | FlippingCreates…2例覆盖两／三面及全关HP；N010Stops…2例覆盖1/3 HP致死、14/3溢出、后续Charge／Strike不执行；无敌阶段仍+1。 |
| ④补线接线 | 49～60直接交011，107～152共享组装，Link无条件、直接／敌方片段及贡献；仅修订增加。79～103先追加本次Record后判断终局。 | ExplicitSyntheticLinkAssembly…4例明确使用内部合成死亡／待补态，核最后Link终局或翻面；公共Run对缺死亡来源的状态／历史拒绝。PublicLinkKeeps011…1例保留原阶段拒绝。 |
| ⑤报告与来源 | 79～103、190～266、374～397：核完整状态链、原三段关联及阶段决定；重新派生事实顺序、贡献定位和后态，按原参战者从0精确汇总；全关HP累加所有面定义。 | IncompleteOrAlteredStateChains…10、MissingFragmentsAndForged…15、ForeignDefinitions…2，覆盖漏前／末步、错绑定／面／HP／累计、缺段、错操作、漏重贡献／换受益者／错索引及伪覆盖；公共报告包含末击与原19级入场。 |
| ⑥字节与身份 | Fingerprint 15～235、238～286：固定字段与枚举名；UTF-16代码单元、LE64 core和LE32 Words完整保留，唯一排除报告Fingerprint自身。 | 51字节向量1、文化／UTF-16 4、来源组敏感性8、固定声明顺序1、未知枚举1；独立读码详见下文。 |
| ⑦原子与预算 | 同一RandomSamplingBudget.Math贯穿新求值、历史复验及报告编码；不建立子预算，不重抽历史009；只读结果在完整成功后返回。 | 输出隔离／只读1、末步与共享Math／Sampling限制2、大保留值复验6、创建／编码限额1、真实0 HpLoss及必暴0字1；新预算重试同值。 |

以上测试名均属于CandidateBattleOperationsTests；同一例可覆盖多项，合计仍为26方法／109条，不重复计数。
真实L1／L3使用公开008固定seed42／sequence54及明确合成C=1/1000，不能外推为生产随机或成长平衡保证。
当前单Warrior域自然不产生Pending；正向Link验证是契约允许的内部合成组装，未冒称公共自然AOE链。
历史复核调用既有010／011确定性规则并核保留009算术；没有第二套敌循环／补线实现或旧随机机会重抽。
每条自身After修订=Before+1；记录间及当前快照仅容许修订提高，完整业务状态仍相等，013锚点／Superseded合法性仍由013负责。

## FMBR01独立核对

R在A交付前已从冻结文档及旧类型声明整理36种记录模式、246个字段；与实际写入器逐项比较，字段名／顺序差异0。
覆盖Report、Binding／三Domain、Baseline／准备定义、Snapshot／Board／角色／敌人／累计、随机／身份／几何、
Operation／Request／Conditions、直接／暴击／敌意图／敌伤害／阶段事实及贡献段。
ReadyParticipants为原成员键，定义只在Baseline保留；Snapshot定义引用由封闭前关联校验保证。
Operation的三事实列表派生OrderedFacts的Kind／Index，没有另编码一份可分歧包装列表。
直接事实的EffectIndex／DamageKind／DefeatedTarget、敌事实及Crit的全部原声明字段均未遗漏。
Compute仅确定编码与数值复验；其本身不是来源认证、签名或封闭语义验证。

基础向量：51字节，头为46 4d 42 52 30 31 0a，Record x=-1/2、z=中😀。
hex：464d425230310a0602000000040100000078000302020000002d3102010000003204010000007a0004030000002d4e3dd800de
R交付前独立按文档字节计算的SHA，与实际编码器已执行测试一致：
c3918a41722ceee31527635fd94a3b52faff87f80bd40235f136b0b8bd13465c
Integer使用Invariant规范文本；ExactRational沿既有规范化不可变值，分子／分母均复验预算。
字符串逐char写LE16，不修复孤立代理、不normalize；core每个ulong明确LE64，全部Words明确LE32。
未知枚举被拒绝；没有文化排序、JSON、反射属性遍历或GetHashCode充当载荷。

## 范围与版本证据

R于10:33:25 UTC独立保存原403项，10:35:58 UTC保存尚无codeFreeze的初始scope；不是交付后反推起点。
before继承010 after原时间2026-09-21T09:49:21.5765037Z及原路径／SHA。
010 scope SHA：376d7bd4a885be736ac4f6158653d6c15404f99ff108fae81ec72b2c68a67a1c
原403规范SHA：f20904bef62b1f3378837fd555a98eae655e04c72ac1c8449a90d234f7b690b5
A startedSnapshot：2026-09-21T10:34:13.7383130Z；403旧项相同，本包14条新路径不存在。
初始scope64935字节SHA：65d261cc81be37e197c8eaaed88e2574f209df45cc54ec676c028f9e4af0c336
从最终scope前64934字节补原闭合括号重建，得到同SHA；before／startedSnapshot原字节未被改写。

| 已全文审阅文件（各有已审meta） | C#行数 | meta行数 | 新GUID |
| --- | ---: | ---: | --- |
| [CandidateBattleRun.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleRun.cs) | 60 | 11 | 8e603cc42d25358488bbfa880de17a5e |
| [CandidateBattleOperations.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleOperations.cs) | 459 | 11 | b4a29303d128da54c9057c95fa83a813 |
| [CandidateBattleOperationRecord.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleOperationRecord.cs) | 77 | 11 | 83ed46d11225aec4aa50845076207447 |
| [CandidateFinalAttemptReport.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateFinalAttemptReport.cs) | 41 | 11 | 01b6b0ac084628844989b3eef3d0baf8 |
| [CandidateBattleReportFingerprint.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleReportFingerprint.cs) | 290 | 11 | 3d838c92492ddc344b42009e9e4213d4 |
| [CandidateBattleOperationsTests.cs](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleOperationsTests.cs) | 701 | 11 | 3880e178e195d8d41866447a7b9bf66d |

codeFreeze：2026-09-21T11:00:56.2046900Z；六C#规范SHA：
169ca8d6a130aedb1e4dd3c38e65f10d44b3133aafadffa1831c77d28716d2a8
validationBefore共409项，规范SHA：
093aee14927150948a2f020d8af79c9ef0dd301c583b9fbed21d66127b9d689d
测试前11:06:13.4201479Z、测试后11:10:55.8788837Z及R终核均415项，规范SHA：
3efae32d1ec770ef3b1ae2410330ad579225d78181f8aa263251e4deae3ef4b2
R重算403旧项差异0、恰12新增、缺项0、六C#冻结差异0；Assets路径420→432，无其他增加／删除。
232份Assets meta的GUID无重复；原226路径保持。另将原222规范SHA与010已接收证据比较，并核010新增四GUID，全部一致。
原226路径／GUID规范SHA：ddefcd00f321cc9061e774c8dbfbf60b38640784fc40015ad5ceb1058b05ba83

## 原始执行与测试

R未运行Unity或测试，直接读取A原012 turn的CommandExecution、实际文件及XML，并逐项比对scope复制回执。
固定Unity 2022.3.18f1、原项目绝对路径；三次启动均先查无Unity进程，未杀进程。
命令均Hidden／PassThru，WaitForExit＋Refresh取得真实子进程ExitCode；编译有-quit，EditMode无-quit。
不是用外层shell成功或日志存在代替实际Unity退出。scope所录command数组、stdout、stderr与原记录完全相同。

| 运行（2026-09-21 UTC） | PID | 开始→结束 | 实际退出 | 原始CommandExecution |
| --- | ---: | --- | ---: | --- |
| 首次编译 | 22668 | 11:01:57.6484454→11:02:58.0587602 | 199 | exec-9729946f-a4d3-4491-9ae3-fa81fe0ea724 |
| 同源码编译重试 | 6772 | 11:04:56.9623462→11:05:09.8016988 | 0 | exec-95c295ec-1d04-493b-8011-7ef900c08005 |
| 全量EditMode（一次） | 37360 | 11:06:46.5516841→11:07:06.9998619 | 0 | exec-cddcd214-931f-4916-a6b5-0890c7ccd2b2 |

首次因LicenseClient IPC Connection Refused 0x8000000a、等待60.04秒超时退出；尚未编译／生成六meta。
699字节原日志在重试前保存到scope.compileAttempt1.evidence.logText，R重建SHA一致：
8ada4f8a01f2f73a17b9e8a36f5027b71df751eb040abe9a360a830d8baf8e60
经审批后同源码重试成功；旧批日志／XML未覆盖。本批日志由Unity重写，首轮原文、时间、PID及退出仍可核验。
原codeFreeze.sourceLines聚合为null；11:01:28的逐文件行数补充在首次Unity前证明1628＋66，未覆盖原记录。
一次外层文件夹meta枚举预检失败发生在scope写入与Unity之前；修正枚举后通过，不是C#或测试失败。
成功日志C#错误／警告均0；仍有许可客户端签名／token提示及编译Curl42，不能称日志毫无错误文本。
实际成功子进程退出、测试XML及全部源码版本对应关系已核实；R终核Unity进程0。

| 验证产物 | 字节 | SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB08Compile.log | 64226 | 0876a2142fe3b4cce4755332292b3b112c0018a10c4c799e5be7a65e4e690e89 |
| Logs/FMDemoB08Tests.log | 89033 | 8bd25e8456b852e18e8e6f01f4c6142085e8ea59e6a4911465d5c557b71e93bd |
| FMDemoB08-EditMode.xml | 1246677 | a384933b9f9f5b9c3c12017ec0cd8e1b64c6d4115cae44f06c438528176167fd |

XML时间11:06:53Z→11:07:05Z；实际1911 test-case全部Passed，failed／skipped／inconclusive均0。
原B07 XML SHA：cc21070c6598530745d82d2773161e391319f6726a0c8b3500ba91b50a8d853d
R以StringComparer.Ordinal保留原完整fullname及重数，逐名比较Passed数量：原1802条／1797个不同名称差异0。
原三组重复名称（重数4、2、2）及各自Passed数均保持，未用Distinct集合掩盖丢例。
原fullname＋TAB＋count＋LF规范SHA：15e52a8aa724171d463aa4338eadb74f3e3873db7dcbc052ab84dd10907cbd69
R另算fullname＋TAB＋count＋TAB＋passed＋LF：df09ab994b642c66bd3bb306e05bf3994d37ec6cb37f25deee3a6da802975865
新增只来自CandidateBattleOperationsTests，26方法／109条全Passed，无其他新增测试。
上述规范均Ordinal排序、UTF-8无BOM并含末尾LF；各路径文件SHA也已独立比较。

## 交付标识与边界

作者[demo-012-delivery.md](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-012-delivery.md) SHA：
e68e2860781a892cac073e8c8ee59ae99489df38ebee009ed12bca903309b24a
作者[demo-012-scope.json](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-012-scope.json) SHA：
f14c371e5468e99451327e9bb573107e1b52e4334c767d8d2859a98a52cb2ff7

规范缺陷0，规格缺陷0；无需同批修正或用户动作。
本结论限Empty／单Warrior／E01、E02完整纯Core候选及正常胜利报告，CommitEligible仍为false。
FMBR01是报告编码，不是M12存档；未验收013恢复、023结算、奖励、复活／广告、UI、PlayMode或Player构建。
数学预算不承诺覆盖总内存／字节数／运行时间；报告不宣称来源签名或生产平衡保证。
R只新建本报告，未修改实现、测试、meta、旧报告、协调稿或运行产物；正式结果交SD00接收，未启动后批。
