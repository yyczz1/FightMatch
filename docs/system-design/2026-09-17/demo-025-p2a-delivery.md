# 025-P2A 共享规则与正式内容绑定：作者交付

作者状态：COMPLETED；本报告不代替原 R 的独立 verdict，也不宣告 025、B17 或首 Demo 完成。

- 工程：D:/Unity/UnityProj/FightMatch；host=local。
- 原 C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1。
- 准确 turn：01a0d207-3f3f-7801-a775-692b0b60d52d；本轮未重启、未新建任务或代理。
- 实际模型／推理：gpt-6-astra / max；原生 turn_context 已核，启动时间 2026-09-24T06:08:05.463Z。最终完成时间由本 turn 后续原生 formal final / task_complete 建立，不预填。
- 前置：§278 已独立 ACCEPT 的同源设计 C1；原计划、范围、review 及 668 起点保持。
- 授权：system-task-packets.md §280–282、§285、§287；冻结摘要分别为 eea36b604d5646c3ef5104cc2d37ed9eda64bb6126978ad846708cac4909cac8、0608638f1d84e41443bc7588dc6e795872e1a25c768e361ef8ccbd7bdb8936bd、ee3c2320b6b926355bada2d698236ab49c1a1389469a3c863a55c005b14c260a。
- 正式范围：本目录 demo-025-p2a-scope.json；证据根：TestArtifacts/FMDemo025P2/p2a。两正式报告的最终长度／SHA 由本 turn 的 formal final 外部绑定。

## 实际结果

候选与正式来源进入同一个既有规则核。新增不可变 ContentBinding / DefinitionBinding、共享 RuleContext / PreparedRuleContext 和准确定义闭包 PublishedSaveContext；原 Candidate 子类、候选生命周期与旧保存准入保留。

ContentBinding 比较 PackageId、ContentFingerprint、RuleVersion、NumericContractVersion、RandomContractVersion 全五字段；DefinitionBinding 另外固定 LevelId 和正整数 BigInteger LevelVersion。准备、复制、比较和预算按真实类型分派，正式分支不制造 DraftId / DraftRevision / SourceNotes。

BattleEntryPreparer.PrepareCandidate 保持候选限定；新增 PreparePublished(BattleEntryInput, DefinitionBinding, PreparedLevel, ExactMathBudget)，核完整绑定、关卡身份和整个解析关卡载荷，再走同一私有准备核。PreparedBattleEntry.GetDefinitionBinding() 显式提供绑定；使用方法避免改变旧候选公开属性投影，候选 FMBR01 字节不变。

正式意图采用 FMINT002 / schema 2，正式报告使用 fm.published.entry.v1 / fm.published.context.v1 tag 和完整绑定。正式意图拒绝坏代理及非正／非规范入关版本；旧候选 FMINT001 与原始 UTF-16 单元语义保持。P1-C1 内容编译器的 FFFD／代理对保全和坏代理拒绝未改。

Requirements 映射遵循既有 SaveEnvelope：正式 SourceNotes 为 null，候选保留原列表；正式定义版本先核规范正整数，重复绑定可正确去重。PublishedSaveContext 只承载不可变准确定义闭包，不提供发布、启用、会话或写入能力。

Application 请求／视图／生命周期和原控制器仅迁移绑定复制、比较和类型。Purpose 从实际 snapshot descriptor 取得；本阶段没有正式会话。原 runtime／队列／store、输入阈值、PRD／SC01、攻击、敌方、评分、奖励、经验与背包规则未建立第二套实现。

## 验收

| 条件 | 作者核验结果与精确依据 |
| --- | --- |
| A01 | PASS。PublishedRuleContextTests 覆盖五字段逐项变化、正整数／规范版本、错误类型、null、Unicode、字符串／集合／数学预算、深复制及闭包缺项／重复／跨包拒绝；正式 Requirements 完成既有信封的内存往返。准确 34 个 fullname 见 named-cases-diff.json。 |
| A02 | PASS。SharedRuleContinuityTests 的同源用例以同一内存事实和冻结随机输入执行候选／正式两来源；完整操作记录逐字节比较 HP、随机状态／消耗、伤害、敌方、阶段与贡献事实；实际终局 HP=95、经验=26、材料=2，并执行成长、库存和进度收尾。来源身份另断言，不剔除业务字段。 |
| A03 | PASS。候选 100 字节 FMINT001 固定 golden SHA=dc9d0a7e42a08310d68fd546199dd0f484f818da5deee3f4d5568e799a95383d；51 字节 FMBR01 scalar golden SHA=c3918a41722ceee31527635fd94a3b52faff87f80bd40235f136b0b8bd13465c。完整报告 expected 来自入口原 writer（bb35f4fb…），仅改测试类／构造器名，机械比对正文相同。原 F1-FFFD／PAIR／D800／D801 四用例全部通过。 |
| A04 | PASS。正式上下文不能走 PrepareCandidate；旧 business/application save overload 仍拒正式准入／改 purpose 的信封。正式来源隔离与真实重放均 CommitEligible=false。没有内容发布、启用、假正式 session 或真实 PlayerSave 成功。 |
| A05 | PASS。最终 011 XML 全部 3464 项通过，失败 0、跳过 0；原 3424 fullname 多重集完整保留，新增 40 项均通过。旧测试仅 §281／285 的 11 个 helper 参数替换，方法体、输入、断言与预期保持。010 编译和 011 测试进程均 exitCode=0；680 源、脚本和 36 DLL/PDB 的编译后→测试前→测试后→交付身份相符。 |
| A06 | PASS。51 旧修改文件增删共 567 行，其中 helper 22 行；617 旧实现保持。新实现自然组成 680 项、allAssets=693，旧 361 GUID 保持且新增 6 唯一 GUID。308 受保护输入、原报告／起点、6 个准确旧日志／XML 输出保持；三协调稿仅 SD00 正常追加。精确逐文件身份和 diff 在 scope 与 scope-diff.json。 |

## 范围和预算

| 项目 | 实际／上限 |
| --- | --- |
| 既有文件 | 51 个获准修改；增删 567／2200 行；其余 617 个逐字节／SHA 不变 |
| 旧测试 helper | 11 个参数替换，增删 22／22 行；§285 的十个 Candidate 参数，加原 Prepared 参数 |
| 新 Core | ContentBindings.cs、RuleContext.cs、RuleContextChecks.cs、PublishedSaveContext.cs；共 409／1200 物理行 |
| 新测试 | PublishedRuleContextTests.cs、SharedRuleContinuityTests.cs；共 853／1600 物理行 |
| 自然 meta | 与上述 6 个 cs 一一对应，全部由 Unity 生成，旧 meta 未改 |
| 工具 | Tools/Invoke-FM025P2Validation.ps1；132／360 物理行 |
| 正式报告 | 本 delivery ≤280 行；scope ≤4 MiB，最终实际值由外部 final 绑定 |
| 环境证据 | environment-diagnosis.json ≤49152 字节，准确长度／SHA 在有限 manifest |
| 字节副本 | entry-files：入口 668 项；final-files：最终 680 项加最终脚本；全部长度／SHA 校对 |

scope 中完整列出 51 路径、前后 SHA、逐文件 diff、原始输入、最终实现／资产／GUID和导出入口。旧测试 helper 的整个文件内容已按“仅精确参数字符串替换”核对。冻结原包不重写；§285、§287 以 sourcePacketAmendments 另绑。未修改旧 asmdef、Packages、ProjectSettings、场景、策划或旧证据，没有任何 Git 写操作。

## 实际运行和失败保留

所有运行独占串行，使用同一 Unity 2022.3.18f1 可执行文件；完整参数、PID、时刻、真实退出、stdout/stderr、日志与 before/after 已逐号保留。没有测试过滤，测试命令不带 -quit。

| 编号 | 类型／实际结果 | 解释 |
| --- | --- | --- |
| 001 | Compile；真实退出码未成功持久化；日志声明 199 | IPC 超时，之后日志被许可子进程持有导致哈希采集失败。原 failure 保留，result 明确 null，未把 199 冒充读取的进程退出码。 |
| 002 | Compile；exitCode=1 | 已连接许可客户端但未取得可用 entitlement；原 No valid Unity Editor license found / No ULF / Token not found 证据保留。 |
| 003 | Compile；exitCode=1 | 已进入实际编译；首次导入出现旧缓存类型诊断，最终生产错误为新闭包对 BigInteger? 的 Sign 访问；修正 nullable 取值。 |
| 004 | Compile；exitCode=1 | 新测试将 PreparedRuleContext 转为原始类型；修正为 PreparedPublishedRuleContext。 |
| 005 | Compile；exitCode=0 | 中间版本编译成功。 |
| 006 | EditMode；exitCode=2；3446/3450 | 原 3424 完整且全过；新增 4 失败。三项特性参数中的 lone surrogate 被运行时替换，改为运行时 char 构造；一项读取了不适用的 replay.Input，改查已返回的 isolation.Input。 |
| 007 | Compile；exitCode=1 | 新补用例的 List 参数／prepared intent.Data 访问适配；仅修新测试。 |
| 008 | Compile；exitCode=0 | 正式意图 Unicode／入关版本校验补全后的编译通过。 |
| 009 | EditMode；exitCode=0；3458/3458 | 中间版本全部通过，随后发现并修正 Requirements 的 null provenance／去重契约；本成功保留，不冒作最终源码证明。 |
| 010 | Compile；exitCode=0；11.2861361 秒 | 最终版本；PID 35100；07:16:23.3734975Z → 07:16:34.6596336Z。 |
| 011 | EditMode；exitCode=0；393.8678971 秒 | 最终版本；PID 15256；07:18:00.6843449Z → 07:24:34.5522420Z；3464/3464，失败／跳过 0。 |

最终 XML：TestArtifacts/FMDemo025P2/p2a/runs/011/tests.xml，2434965 字节，SHA256=7dcd867c4b4f1ac53f3252dcece9130084dea95a708bda0aeb72758b8176f528。010 compile.log SHA=21c7d64a22fca8fe035eddcf3cba197fb5c33caaf0ec914e6cdfb14c8f84900f；011 tests.log SHA=d53e654bd6f4c897406f84579ba6192ff9fd2b61992148033a384cfce433ca0b。

最后成功后未再修改源码或重复 Unity。旧有效基线、旧独立崩溃矩阵与求参没有另跑；新增测试仅用内存，旧全量测试沿原隔离配置运行。

## 许可环境事实与边界

按 §287 撤回“日志证明账号未激活”的外推。已核用户准确截图及其 SHA，Hub 展示已有 Personal 许可证；截图本身不作为编译成功证据。

001 使用受限执行，IPC 超时；002 起通过 exec_command 正常 require_escalated 审批通道执行。002 已连接却仍无 entitlement，003 起能够进入编译，005／008／010 及 009／011 均有实际成功记录。当前 Editor SHA 与 P1-C1 已接受编译完全相同；Hub／许可客户端／011 的 SessionId 均为 1。历史 Owner、完整性和令牌限制标志未取得，不能声称历史账户／令牌相同或把沙箱判成唯一原因。

environment-diagnosis.json 记录四个可证伪假设、最少脱敏元数据、旧成功对照和实际审批结果。客户端启动时间与恢复先后相符，只支持可能的状态变化，不证明具体原因。未转储凭据／许可证／全环境，未改缓存、安全设置或 Hub，未杀用户 Editor。没有待用户激活事项。

## 后续交接与保留边界

stageExports.implementationFiles 导出最终 680 实现、脚本及必要旧只读输入的准确身份；stageExports.evidence 外部绑定 root-identity、read-manifest 和相同有限 items。manifest 不含自身哈希；副本、固定元数据和实际 runs/001～011 有限展开，不授目录泛读或执行数据中命令。delivery-bindings 先绑定本报告与已生成证据，再形成 manifest，最后形成 scope；两正式报告由 native final 外部绑定，无自哈希循环。

原 R 仍须等待本准确 turn completed、非异步 formal final 及两报告匹配后给独立 verdict。只有独立 ACCEPT 后，SD00 才签 025-P2B-PREPARE。后继须从本次 stageExports fresh 重绑定，不能拿旧 current 要求已批准变化仍不变；本脚本按 §281 固定 P2A 参数与根，后续工具执行范围由新阶段包明确。

未执行：正式发布／启用、PlayerSave overload、首次建档持久记录／平台定位器、后续页面和角色集合、029 整体体验、Player、Android、CI、lint、性能及旧独立故障矩阵。原 §184、027／P1 的接收与 NOT VERIFIED 保持；本全绿不证明整个 Demo 或 Android 已完成。回退依据是本轮入口 668 精确副本及 51 路径 diff；本轮没有执行回退。

