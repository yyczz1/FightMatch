# DEMO-B07-R1 独立代码审查

VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS
Verification: 独立审读实现／测试，并核对原始 Unity 执行记录、冻结 SHA、日志和完整 XML；R 未启动 Unity。
Notes: 未发现待修实质问题；结论限 FM-DEMO-010 纯敌方阶段片段。

审查者：R，任务 01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次审查 turn 01a0c342-f2e6-7832-a811-e255d12a7412。
作者：A，任务 01a0c1fb-248d-7321-8b69-9efd26b817f1；010 原实施 turn 01a0c342-4fb8-7873-aec5-7af5d71bf4d0。
wait_threads 已明确返回该 010 turn completed，completedAt=1789984667（2026-09-21 09:57:47 UTC），之后才正式阅读本批新实现和测试。
本次沿原目录 local、gpt-6-astra／max；无新任务、子代理、Git 写入或实现修补。

## 固定输入与范围

依据派发版 system-task-packets.md r72 §104～106，SHA c834315d9d2a0e610c68a7b5c687678c886200e7873e47df3ce3678d2e216992。
冻结 session-plan r87 SHA cc3b9690cc509214b2cddecf048e41d5704dd0604f8d72697705ef34c560916f；integration-review r70 SHA d3894e767ee9a3f0b575807f422e2894f4b908e1bda3bc03989629a0d626357e。
已读 AGENTS、PROJECT_CONTEXT、CODING_RULES、PLANS、REVIEW_CHECKLIST、VALIDATION及本包指定策划／数值／贡献／来源范围。
逐项依照 REVIEW_CHECKLIST 检查；规范与规格分别见下表。§107 的未来 012 设计未作为本包验收要求。
三协调稿后来仅登记实施／备审状态和实际 turn；与 R 留存的 §104～106 原文比较，业务、白名单、验收及执行限制保持。
54 份其他保护输入 SHA 不变，包含原 B06 报告、011／022 交付与范围、原日志／XML、策划、校准脚本、规则、包及工程版本。
本次 R 唯一写入为此新报告；没有改动旧报告、三协调稿、实现、测试、meta 或工具产物。

| 交付输入 | 独立核验结果 |
| --- | --- |
| [010 交付报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-010-delivery.md) | 128 行；SHA 760d7776ab47c083d4df61433f197d4eb2d412a468d221e84ba400fe3a844967 |
| [010 范围记录](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-010-scope.json) | 261580 字节 ≤ 300 KiB；SHA 376d7bd4a885be736ac4f6158653d6c15404f99ff108fae81ec72b2c68a67a1c |
| 原 395 项 | 继承 demo-011-scope.json.after.files，原 UTC 2026-09-21T08:43:35.5982311Z；逐项 SHA 全部一致 |
| R 独立基线快照 | UTC 2026-09-21T09:20:15.9755385Z：395 项，未出现本包新实施路径 |
| A startedSnapshot | UTC 2026-09-21T09:20:43.2908113Z，395 项无差异，自身八项实施及两个交付路径均原先不存在 |
| 初始 scope | R 在 09:23:09 UTC 留存 SHA 0a1e3938803ab047841bdbca383af76ed1b28df13aa7bfd41c9b8db461b1cce8；最终 scope 还原的初始 62704 字节与其一致 |
| 最终源码／测试范围 | 403 = 395 旧项 + 白名单 8 新项，无缺项、旧项改动或额外路径 |
| 全 Assets 路径 | 412 → 420，仅增加本包八项，无删除 |
| 行数 | Core 394 + 44 + 68，测试 633，四份 meta 各 11；合计 1183 ≤ 1800 |
| 既有工作区差异 | 原 tracked 摘要仍为 34 files changed, 2809 insertions(+), 597 deletions(-)；不是本包改动 |

路径／SHA 规范均使用 Ordinal 相对路径排序，path + TAB + 小写 SHA + LF，含末尾 LF，UTF-8 无 BOM。
395 项原清单规范 SHA：28be3756cf902dcfe9b28bc99869c3f8f20447233000a9fe7842d4ac84900b94。
原 B06 scope 整体 SHA：772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859。
四份冻结 C# 规范 SHA：a15f8947d4a2357fcbe0d07eb554b8279ab85c0cd7659a4f4bb72088e9428902。
编译前 399 项规范 SHA：f48c1ddcb9e794d276aeeb7335e66beab78e45a30ccbc3bcff6357348c07fef7。
测试前／后及 R 当前 403 项规范 SHA：f20904bef62b1f3378837fd555a98eae655e04c72ac1c8449a90d234f7b690b5。

## 规范审查

| 检查 | 判断与依据 |
| --- | --- |
| 单一职责及范围 | 仅实现 010 的纯敌方阶段；三份 Core 文件容纳执行／校验、只读片段、意图／伤害事实及结果类型，无新接口抽象、配置或缓存。 |
| Runtime／Editor 边界 | 纯 Core、既有程序集；无 UnityEngine／UnityEditor 调用，无 MonoBehaviour、Editor UI、包、asmdef 或序列化变更。 |
| 兼容性 | BattleSnapshot、CandidateCombatFrame、BattleDamageFact 及全部旧文件 SHA 保持；未增 friend 或改已有 API。新 API 属 §104 明确授权。 |
| 错误与预算 | 根 null 抛具名 ArgumentNullException；结构化拒绝含原因／字段且 Frame=null；同一预算贯穿校验及执行，ExactMathLimitException 不吞掉或替换。 |
| 不可变与隔离 | 原数据由 009 保留；新集合复制后 AsReadOnly，事实与帧属性只读，计算只替换本地列表项，失败不返回半份片段。 |
| 变更卫生 | 新 C# 无尾空白／冲突标记；git diff --check 实际 exit 0；未混入旧文件格式化、重命名或清理。 |
| 保护与回滚 | 原始 FileChange 仅四份新 C# 和自身交付报告，PowerShell 文件写入仅自身 scope；meta／验证产物由 Unity 生成。可按本包八项实施及两份交付独立撤回，本次未执行撤回。 |

## 规格审查：七项验收

以下执行文件均为 [CandidateEnemyPhase.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateEnemyPhase.cs)，测试均为 [CandidateEnemyPhaseTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateEnemyPhaseTests.cs)。

| 验收 | 独立审读及已运行见证 |
| --- | --- |
| ① E01／E02 与精确伤害 | 执行 35～53 行从绑定意图读取系数，先精确乘法和护甲修正，最后 Floor 一次。测试 16～67 行覆盖 L1／L3 双坐标来源、L3 E02 20→4 后 Charge、E01 对 W100 扣 5；10×13/10×100/110=130/11，Floor=11。另有 Raw=13/10 和系数 7/3，能区分提前取整及名称硬编码。 |
| ② 稳定序与原游标 | 执行 21～38、60 行按 StableOrder 排序，原 BigInteger Cursor 取有界余数，实际处理才 +1。测试 69～87 行令槽位 0/3/6、稳定序 30/10/20、输入反序，并使用 2^90、2^90+1 及独立原游标，验证原实例语义及输出身份顺序。 |
| ③ 死亡、空阶段、N010 | 执行 17、25～38、60～63 行：死敌跳过；每个活敌处理前重新查存活成员，没有则 break，Charge 也不推进；空段仍有 EnemyPhaseOrdinal。测试 89～158 行覆盖先前死亡、009 击杀、全死空段、末次致死事实及后续 Charge／Strike 保留。 |
| ④ HP 封顶及贡献 | 执行 45～58 行只增加 HpLoss，保留 Dealt 与旧 Taken，不计 Overflow、不开 1/4 权重。测试 160～191 行区分 Charge 的 null Damage 与真实 0 伤害 Strike，核有理 HP 封顶、多击中途倒下、旧 Dealt 5/3 + 009 的 2 仅累计一次。 |
| ⑤ 来源绑定及支持域 | 执行 73～177、180～257 行核原 Baseline／完整候选上下文／007B 域、Player／Attempt／Operation／Face／Actor／Revision／ActionOrdinal、HP、游标、键覆盖及原随机证据；限定单 Warrior／零闪避／E01-E02。测试 106～132、212～399 行给出错绑定、缺失／重复、非法值、阶段及能力反例。 |
| ⑥ 随机与组合 | Frame 9～26 行保留整个 DirectAttack，Random 直接引用其原 Random；无再次 Crit／随机抽样、011 调用、完整 BattleSnapshot 或全局防重缓存。测试 193～210、351～368 行核正常、全蓄力、全死、全倒停止及随机反例；同输入重复求值保持一致。 |
| ⑦ 隔离与预算 | 执行 88～105、259～368 行重验完整保留基线、前态、直接事实中的整数与有理分子分母，含未使用面、原累计和随机计数。测试 193～210、401～485、610～630 行覆盖源请求后改、深只读图、64 bit 超限、0／末步／已消耗共享预算、晚阶段溢出和新预算重试。 |

N010 的具体见证：首敌打中仅余 1/3 HP 的成员，保留 HpLoss=1/3、Overflow=14/3、DefeatedTarget=true。
后续 E02 原 Cursor 分别取 0／1／2，后排 E01 原 Cursor=5，均不产生事实且原 Cursor 保留；已完成首敌仅 8→9。
之后构造明确标注的独立恢复后前态，再经真实 009 入口生成新攻击，E02 从保留意图继续；没有复活入口、瞬时补打或旧操作重放。
这依据已确认 N010；旧 chapter_model 的 Charge 先 continue、boss_model 的无目标 continue 及全局阶段奇偶均未冒充本包规则。

[CandidateEnemyIntentFact.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateEnemyIntentFact.cs:7) 保留原 Baseline／Player／Attempt／Operation／Face／Revision／行动及敌方阶段编号。
其 15～33 行保留连续本段序号、原 EnemyKey／Pair／StableOrder／前后 Cursor；37～65 行另定义准确敌攻事实，不改玩家 BattleDamageFact 的 Crit 语义。
[CandidateEnemyPhaseFrame.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateEnemyPhaseFrame.cs:9) 仅返回完整成员、敌人、贡献和本段事实；不推进原计数、Board、Face、Phase或生成最终报告。
成功测试输入均经公开 CandidateDirectAttack.Evaluate（测试 503～507 行）；internal 构造用于明确合成前态或拒绝反例。
L1／L3 无暴击样例使用真实抽样且注明 conditional 与合成 C=1/4 来源，不视为生产随机或自然进度证明。

## 原始验证与同版证据

原始记录：[A 原实施会话](C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T11-20-56-01a0c1fb-248d-7321-8b69-9efd26b817f1.jsonl)。
R 按 turn_id 独立读取 CommandExecution／FileChange，并逐字比对 scope 保存的两条实际命令、stdout、stderr 和退出码。
最后源码修改记录在 1787 行，UTC 09:40:59.271；四份源码冻结记录在 1797 行，UTC 09:42:19.908，先于两次 Unity 启动。
冻结回执、编译前 399 项、测试前 403 项、测试后 403 项及当前文件逐 SHA 一致。

| 运行 | 原始记录及实际回执 |
| --- | --- |
| 编译／导入 | 1806 行；exec-ea0bd2c5-e7b8-4132-8ff8-86c69367d19d；PID 2912；UTC 09:42:45.8568229→09:42:59.4495341；实际 Unity exit 0 |
| 全量 EditMode | 1828 行；exec-1ac89092-c2cb-4f7e-ae0e-90a913b61518；PID 39196；UTC 09:45:36.6272273→09:45:55.4249233；实际 Unity exit 0 |
| 启动排他 | 两条命令先检查固定 Unity 路径与全部 Unity 进程为零；无终止进程动作，先编译完成再测试 |
| 退出获取 | Start-Process -WindowStyle Hidden -PassThru，WaitForExit／Refresh 后读取进程 ExitCode；未用外层 shell 状态替代 |
| 固定参数 | Unity 2022.3.18f1；项目 D:/Unity/UnityProj/FightMatch；编译 -batchmode -nographics -quit，测试 -batchmode -nographics -runTests -testPlatform EditMode 且无 -quit |

两条命令仅指定本批绝对日志／XML 路径；各实际运行一次。编译日志 214～220 行有 Core／Tests 的 Csc 和 build success，520～521 行确认正常退出。
两日志 C# error／warning 均为 0；有 Licensing Client／access token 及一次 Curl 环境消息，随后授权解析、编译、测试与实际退出成功，不能表述成“日志完全无错误字样”。
原会话 1818 行的一次预检 PowerShell 因枚举表达式生成根 .meta 路径而失败；尚未写 scope 或启动测试，1825 行已修正预检并通过。此事未修改源码、meta 或运行产物，也不是一次失败的 Unity 运行。
R 当前查见 Unity 进程为零；仅检查现成验证产物，没有另跑测试。

| 验证产物 | 独立计算 SHA256 |
| --- | --- |
| [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB07Compile.log) | 48f7592a854521988c79cd34122a4079616d8b476ad02d04da0909516243bb28 |
| [EditMode 日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB07Tests.log) | 4995f53bd96023c1533f34ff8c324c3d28048b86a38049881d9cd6c46ca1d302 |
| [EditMode XML](D:/Unity/UnityProj/FightMatch/FMDemoB07-EditMode.xml) | cc21070c6598530745d82d2773161e391319f6726a0c8b3500ba91b50a8d853d |

XML 根及实际 test-case 均为 1802/1802 Passed；failed／skipped／inconclusive=0，执行 UTC 09:45:44→09:45:53。
原 B06 XML SHA a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e 保持。
独立按 Ordinal fullname 比较完整多重集合及逐名 Passed：1667 条／1662 个不同名称全部保留，差异 0。
原三个重复名称的次数 4／2／2 与 Passed 次数逐项保持，未通过仅比较总数替代。
旧 fullname + TAB + count + LF 规范 SHA c837f7b0ecc9939161771fb54636efbad78b1114d6ed30bf4637437160fb7f05。
R 另算 fullname + TAB + count + TAB + passedCount + LF，规范 SHA ef8e04699d244f476fe10f716be4157659db9768fc967ea1bd0ec6a4eedd924b。
新增恰为 CandidateEnemyPhaseTests 的 23 个方法／135 条 Passed；没有其他新增 fixture 或用例。
135 条按方法实际分布：4、3、2、2、1、1、3、2、3、4、2、9、5、14、22、5、7、6、13、8、12、4、3（源码顺序）。

## 新文件锁定与 GUID

| 新文件 | 行数 | SHA256 |
| --- | ---: | --- |
| CandidateEnemyPhase.cs | 394 | a3249bec9874af9c5222d1048ddc58b29972c902030bc0bbd01cae35635b21c7 |
| CandidateEnemyPhaseFrame.cs | 44 | 16e14da19d68bf5f29f8a5679024b5a013feb9a94829753c4a7544e7163a8f2a |
| CandidateEnemyIntentFact.cs | 68 | b3358476259cb638c96da4dc0283d5e600c07391b15c6a97c5c90b5da660eabf |
| CandidateEnemyPhaseTests.cs | 633 | fd0f72ef68038b29fc881939fb7c32cfc8ee02ffc2d3f75535330a7662659ce7 |

前三份位于 Assets/Scripts/FightMatch/Core，测试位于 Assets/Tests/EditMode/FightMatch；每份对应唯一同名 .meta。
R 完整阅读四份 Unity 标准 MonoImporter meta，全 Assets 226 份 meta 无缺 GUID、无重复 GUID；原 222 项路径／GUID 集保持。
原 GUID 清单按 path + TAB + guid + LF 的 Ordinal 规范 SHA：5f01dd740529bbbcdab94454aed412fd97500d4721589b886b97f5383c76ed7f。

| 对应文件 | 新 GUID | meta SHA256 |
| --- | --- | --- |
| CandidateEnemyPhase.cs | de49c4a4bc4783c4fa2c85699d50c4e1 | 6b5620ead8a83f54702bc68ce26bbe09ea2315fb6015c93b92642a5299059820 |
| CandidateEnemyPhaseFrame.cs | e636bac120acb1e4e8c556c6feae46e1 | 487d07b33f0a350ce1a86411116b313f59a532f7f5a4eacb843cf0a16f4b52f3 |
| CandidateEnemyIntentFact.cs | 859446c8565aa0e4b8e64001c8634eb8 | 8bf738d9e75ab6de556a769f112fb86bf6b24384f5854b8402bf243fe9fe2e1e |
| CandidateEnemyPhaseTests.cs | 3577275642aa8db4e87b41ae2b65f4d9 | f16673016781f84be7e1a0a1b3a01145c0e2a820b05b64208ccb831cbb26f06e |

无修正包要求，无当前用户操作。交 SD00 接收本报告并决定 B07 生命周期。
本结论不覆盖未来 012 的完整行动封装、真实存档、生产随机校准、多人内容、PlayMode／Player 或可玩 Demo。
