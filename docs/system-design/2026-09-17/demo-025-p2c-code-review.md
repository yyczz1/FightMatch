# 025-P2C 独立代码与交付审查

VERDICT: ACCEPT

审查对象为 §322～323、§325～326 定义的正式玩家接入源码包；C01～C06 及 C1-V01～V05 在本包允许的源码和隔离内存协议验证范围内通过。未发现需要纠正的问题。本结论不代替完整 Demo、029 物理宿主、Windows 真实崩溃/断电或 Android 验证。

## 1. 准确回合与正式交付门

- C 原任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，标题“FightMatch 本机保存与应用接入实现”；本次准确 turn：01a0d666-8097-7780-b1a6-e70fe6d71e7a。
- R 原任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc，标题“FightMatch Demo 独立代码审查”；本次准确 turn：01a0d667-61ef-7ab2-88a5-dec75e498276。
- C 原生 task_started 为 2026-09-25T02:30:36.974Z，turn_context 为 02:30:37.025Z；R 对应为 02:31:34.669Z、02:31:34.736Z。两者均核 gpt-6-astra / max，cwd 为 D:/Unity/UnityProj/FightMatch。
- C 本次非异步 assistant response_item、phase=final_answer 于 05:19:04.994Z 正式声明 COMPLETED；原生 task_complete 于 05:19:05.246Z，turn_id 精确一致，last_agent_message 与 formal final 相同。任务工具状态为 idle / completed / error=null。
- 05:19:28Z R 核对下表两份实物与 formal final 的长度、SHA256 全符后，才开始交付实现审查；未把先前 PUBLISH completed、进行中 commentary 或 SD00 收件当作本次独立结论。审查后再次核对，作者报告身份未变。
- 原生依据为 C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl 与 rollout-2026-09-21T10-31-29-01a0c1cd-dce1-7ac3-8780-06163cb0acfc.jsonl；只抽取生命周期、context 及正式交付，不读取推理文本。

| 正式输入（D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/） | 字节 / 行 | SHA256 |
| --- | --- | --- |
| demo-025-p2c-delivery.md | 8497 / 85 | 52c62c41eabb6a0f626e464b2781aac8d5d32570dda5e3a811708f575911620e |
| demo-025-p2c-scope.json | 2333995 / 52676 | 60a2d6c681dbe60dd0270e82d6be15009a8824697b5f1c0d30e6367e73f31bf6 |

§322～323 规范化 SHA256 为 a0bfb5f579ccdc5d6d1c574af2e98179aa63b404210994db69dae9b3e3913655；§325 为 deeb3aef9ee1c84585ad00387e1fe944ede5e4d8114c8a614e5a872541921aa5；§326 为 91260cdf988a1fa1b0b75f332c344648f0e953cb06a2de98eea67e099a4f0dd2。R 重新按 CRLF→LF、准确段界、trimEnd+单 LF、UTF-8 计算，均未变。最新 §327 只是作者收件登记，不用于替代以下代码和证据核查。

## 2. 实际实现与验收逐项核对

| 准则 | 独立核查结果 |
| --- | --- |
| C01 | 正式编解码采用 PlayerSave、六切片 schema 2 与 fm.player.application.v1；解码必须具备精确 PublishedSaveContext，按完整五字段及定义身份解析。旧默认 Prepare/Encode/Decode 仍候选专用、schema 1，目的重标记不能升级。原 3597 用例及旧源码测试断言保全；正式正反例 18 项通过。 |
| C02 | PlayerSessionSystem 由原架构注册，注入原唯一 runtime、application、lifecycle，未另造状态模型、队列或保存器。PrepareNewProfile 经 GetCurrentBinding 与 ResolveExact 取得实际首包、已验证参数及新档；两个 Guid 由原准备函数生成。正式业务草稿不含可注入 PlayerId、定义、Stats、奖励或随机流；任意 builder 的公开 Submit 不能写正式档。 |
| C03 | 完整 F2 创建记录、持久记录先行/读回、原意图恢复、四物理状态的逻辑分支、M12 原候选续办、同 M02 门内确认、Active 原初始化锚点及后续头恢复均核对。V01～V05 及损坏/预算/owner/观察/冲突/proof 拒绝覆盖见下一节。 |
| C04 | 实际首包上的原 H02、战斗操作/终局报告、S17 保留 operation、原 H06 奖励及再次打开头完整贯通。新进入使用 48 字节平台 RNG；同 operation 的 Submit/Retry/Resolve 先查询原结果或复用原 ticket/字节，不再调用新 builder。重启保留原入场事实和三流初始值，普通重新进入重新采熵。 |
| C05 | 隔离合法 V2 双关内容走同一程序集和门面；原 V1 玩家始终解析其原包及旧根，不读当前 release 替换定义。原包缺失即拒绝。LocalPlayerClock 使用 UTC 与进程作用域 Stopwatch 证据、DeviceUntrusted；跨作用域按原恢复规则降为 UTC，不宣称跨进程单调连续。 |
| C06 | 白名单、两次补签、预算、自然 meta、旧 GUID、810 导出、166 项当前证据、五组历史证据、最终 722 实现和 36 DLL/PDB、原失败记录及串行 Unity 过程均独立核符。未发现越界实现变更。 |

关键实现已逐项阅读：新增五份生产源码和四份测试全文、全部既有源码/asmdef/工具差异，以及相关原有准备、提交、恢复、定义和保存门实现。验证依据是实际代码、原始 XML、过程记录与实物哈希，不仅是作者验收表。

### 2.1 正式编解码与候选兼容

- CandidateBusinessSaveCodec 的默认 Prepare(input,budget) 明确分派 CandidateValidation；新增 SavePurpose overload 只选择完整上下文校验，拒绝未知目的。正式 EncodePublished/DecodePublished 要求非空精确定义闭包；默认入口仍拒正式上下文。
- M02 application 与 M03～M07 目录/正文均按 schema 2 核对；默认候选仍为 schema 1。正式恢复验证目录、切片 requirements、完整正文、规范重编码及 application record/commit-index 关系。
- growth、inventory、progression 通过已解析包内单例身份及版本恢复；level 使用 DefinitionBinding；reward 保留实际 ID/版本/关卡身份。写入侧 SameDefinition 比较实际定义与精确解析值，不能借正式 Context 夹带替换后的参数。
- 玩家等级/经验、实际入场基线、HP/Stats/槽位、随机初始与当前状态、words consumed、历史、奖励和回执仍来自原业务快照；只解析定义，不从当前包重算已存玩家事实。
- ResolveRoots 同时处理 Current 与 RetainedRoots，要求 EvidenceComplete/RequirementsComplete、非退化证据，逐一检查 purpose/player/binding/capability；读取每个原快照并解码、验证初始化身份后才放行。缺旧包、坏旧根和 capability 不足不会通过“只看当前头”绕过。
- §326 的两处新增授权各只改一个 Prepare 调用参数：CandidateApplicationProtocol.Retain 与 CandidateBattleApplicationBuilders.ReplaceHistory，合计增删 4 行。LocalSavePendingRecovery 的三个旧签名仍默认候选，显式目的重载完整保留 owner、fresh observation、租约、原 parent/index、operation、descriptor、规范字节和能力检查。
- CandidateApplicationRuntime.Submit 的正式分支要求同程序集可信入口及 Published Context；公开任意 builder 在执行前拒绝。可信入口仍调用既有闭合生命周期/战斗 builder，并非把 purpose 参数变成写权限。

### 2.2 内容投影与实际新档来源

§325 的 Parameters 来自 Catalog.Resolve 已验证的 built.Replays[0].Candidate.Parameters，复制集合后只读公开；DemoParameterEvidence 及 ShortTerm 的成员是不可变值/只读集合。没有新求参或绕过验证读取 source。NewProfileDefinition 的六项只读标量由已验证输入复制，原 Id、RecordVersion、CanonicalBytes 深复制及 Sha256 保持。

实际新档仍是 new-profile:default / 1，规范字节 298，SHA256=36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646。Class/Level/Experience/Slot 来自该投影。Hp 未成为 UI 输入；原 Compiler 在 Build 时验证配方 Hp 等于相同 Growth 新角色的 EntryHp，空库存/携带/技能/恢复以及初始开放关卡也由原 Compiler 契约核定，因此原 Initialize 产生相同初态，不硬编码替代非空配方。

首包六件总 38452 字节及真实 content-review 原字节保持；scope=player、release-set:fightmatch-demo-r1、唯一 level:ch01-01 / 1 未变。五字段为 package:fightmatch-demo-r1 / b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 / demo-r1 / RC01 / PC01+SC01。本包未再次发布、启用或改签真实 review。

### 2.3 F2 记录、四态与确认门

PlayerProfileCreateRecord 为 Core 的不可变 format 1 值，编码头 FMPROF01；包含原 PlayerId/OperationId、五字段内容绑定、新档定义 ID/版本、完整规范定义字节、Initialize format 2 的长度/完整规范字节及其摘要、生成材料集合和记录摘要。当前 Initialize 除两个已写入意图的 Guid 外无额外熵，材料集合明确为空且拒绝凭空附加材料。

Read 先核整个 envelope 上限，BusinessFields.Space 在分配字符串/字节数组前核剩余长度、预算与 int.MaxValue；未知版本、截断、改变身份/配方/意图/材料、非规范布局或摘要不一致返回具体 Code/FieldPath。Freeze/Write/Read/RestoreIntent 的深复制和规范往返已经核查；不依赖摘要重建丢失的原意图。

LocalPlayerProfileLocator(IContentPublicationStorage) 使用宿主提供的独立记录存储；记录与确认各使用固定语义键的 SHA256，未把 TestArtifacts 或固定玩家 ID 写进生产路径。Read/RecordCreateIntent/ConfirmCommit 在其存储 writer lease 内操作；相同记录幂等、同 ID 异载荷冲突、未知写入保留，不覆盖旧证据。

| C1 状态/反例 | 实际路径及断言 |
| --- | --- |
| V01：已记录、尚无候选/头 | RecoverCreateIntent 只恢复记录中的原意图，ResolveExact 原绑定，并与原规范新档比较；M12 完整观测证明无头/候选后才按原意图提交第一次 Initialize。测试将当前 release 改 V2 并禁止读取 current，仍恢复原 V1/原两 ID/原 intent/record，CurrentReads=0、SnapshotCreates=1、初始化记录 1、奖励 0。 |
| V02：未决候选或结果未知 | ContinueCreate 优先 Retry 原 pending 或 ResumeObserved 唯一原候选；不重新 Initialize builder。snapshot-promoted、marker-before、marker-after 的故障后关闭/重建对象，原 commit/op/原 snapshot 字节不变、无第二次 snapshot 创建、初始化 1、奖励 0。 |
| V03：头已提交、确认前或确认未知 | M02 ConfirmProfile 从已验证头生成 Core proof，fresh ReadRecovery 比较完整当前 descriptor，再在 WithVerifiedRecovery 回调内调用 locator.ConfirmCommit；原 runtime 排他 admission 与保存租约仍在。确认 before/after/bad-read 故障重开只补确认；后续业务头存在时重复确认仍返回原初始化 commit，不回退当前头。 |
| V04：坏记录及身份冲突 | 截断、未知格式、长度、改身份、读取未知、超预算均不能当 Absent；完整原记录字节保全。改变冻结配方即使重新封成规范记录，也不能与原已发布配方不一致地恢复，同 ID 异载荷不能覆盖定位器。 |
| V05：已 Active | 以原 PlayerId 打开最新完整头，初始化 commit 只是锚点。Active 后存档丢失/损坏不返回 InitializationReady、不再创建；另一份有效 Initialize 的不同原始 commit 不能替换锚点。owner/物理观察、错误 profile proof 和冲突 commit 均有拒绝断言；对同一已确认结果允许幂等旧观察。 |

Core VerifyInitializationCommit 不接调用方填写的“成功”布尔值；它核原规范 intent 的 Lookup、PlayerSave/player、Initialize 类型、generation 1、首条 record 与 commit-index 的原 commit，再产生内部构造 proof。实际 Application 确认还验证物理当前头并持原 M02 恢复门。Locator 检查完整记录、player/op/content/definition/intent 身份和原初始化 commit。

未确认时新业务准备要求 Ready/Active，runtime 的 CreationPending 只允许原创建意图，End 和 EndObserved 拒绝结束创建。原通用非创建恢复接口仍保留。确认文件为严格 105 字节 FMAP01 文本记录，绑定完整创建记录 SHA 和原初始化 commit；坏确认/孤立确认不能降为不存在。

### 2.4 原战斗、结算和时间路径

正式 H02 沿原 Growth/Inventory/Progression/Entry/Random/History 实现，使用 System.Security.Cryptography.RandomNumberGenerator 生成 48 字节并记录来源。原 operation 查询早于新 builder；Retry/Resolve 复用 ticket，重开 Resume 校验并复用原 candidate/parent/commit-index/字节。

真实首包用例 RealH02AttackS17RewardAndReopenedHeadUseTheSameRulesExactlyOnce 的 run009 输出可追踪：

- player=8282f79ca43145c9bf726bddae77bcb1；create operation=77f288741d5e419ab366f8f42b58981c；创建记录 SHA=69d118fe2f72393f497eb34ae82f7afe802290c60d0ac9e57df8aa46374f88db。
- 原初始化头=419b5770caa1473cbf81bd9ae9cf6953；H02 头=fd26acae0cdb4fadb5690e7427a2991a；终局头=97b982049e84477bbd869f5f73b268fb；H06 头=035398adaa954cc59b2b71329cbef3e6。
- 真实终局报告 SHA=fd9ffb590ce091643b1f08719bf5f1cfda57845b272a8c5c08cf8f6f3b0395aa，与奖励引用一致；本次实得 XP=26，断言从 report-derived reward 取值比对玩家经验，没有固定“必须 26”的实现/测试。
- 入账后 tin=2、其他材料数量为 0、firstClear=1、基础奖励记录 1、ActiveHistory 清空；重复 Submit/Retry/Resolve 与重开同 operation 不再写 snapshot、不再增益。
- 重启断言保持原 entry baseline/challenge/入场 HP、三流 InitState/InitSequence；新 attempt 从 0 words/空操作历史开始。正常退场后再进入才采新随机材料。

LocalPlayerClock 的 UTC 与单调采样都是设备不可信时间证据；同一进程作用域才比较 Stopwatch。跨域测试明确得到 DomainChanged 和 UTC 差值，而不是继承另一域的单调累计量。QueryView 只读检查未推进恢复或写盘。

## 3. 实际验证与失败保全

R 没有运行 Unity、产品 DLL、旧算法矩阵或新磁盘 probe。以下是对 C 已实际完成过程的独立核对；工具 argv、PID、起止、真实 ExitCode、日志、XML 与前后身份互相一致。

| run | kind / PID | UTC 起止（2026-09-25） | exit | 实际结果 |
| --- | --- | --- | --- | --- |
| 001 | Compile / 19600 | 03:07:56.5199787～03:08:56.9346248 | 199 | License IPC 不存在/拒绝连接；原失败保留。 |
| 002 | Compile / 18176 | 03:25:24.7815686～03:25:26.4837153 | 1 | 无有效 headless Editor license；原失败保留。 |
| 003 | Compile / 16092 | 03:27:50.3870842～03:28:02.1187813 | 1 | 新测试 Core 辅助方法命名冲突，CS0118；原失败保留。 |
| 004 | Compile / 27288 | 03:30:00.9182625～03:30:30.981618 | 0 | 编译通过，尚不是最终源码。 |
| 005 | Tests / 26584 | 03:31:09.4876023～04:01:47.9459213 | 2 | 3655 总数、3648 通过、7 失败；含两项旧候选契约。 |
| 006 | Compile / 16596 | 04:12:27.971354～04:12:57.9340662 | 0 | §326 纠正后编译通过。 |
| 007 | Tests / 2908 | 04:13:24.2997856～04:44:38.9248304 | 2 | 3655 总数、3654 通过，1 个新重启测试失败。 |
| 008 | Compile / 4124 | 04:45:46.5452285～04:46:08.7553422 | 0 | 最终源码编译成功，0 编译错误。 |
| 009 | Tests / 11348 | 04:46:36.0631556～05:15:56.3191291 | 0 | 最终无 filter EditMode：3655/3655，0 失败、0 其他/跳过。 |

九个过程按时间串行，before/after 的 Unity 进程边界记录为空；Tests argv 无 quit、filter 或 category。Compile 008 的日志明确 return code 0；Tests 009 的实际进程结果和最终 XML 一致。日志中的 license/Mono 退出诊断保留，没有把这些文本删掉或把“无 CS 错误”单独当退出成功。

最终 XML：[tests.xml](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2c/runs/009/tests.xml)，2630563 字节，SHA256=5c6bdba060a7d4c7f2292bbe0b2ad39f59b3ef944a5d07184479c81bb6ea3be5。

- 与 PUBLISH run003 的 3597 个 fullname 多重集合逐项比较，旧项和数量无损；新增恰为 58。run005、007、009 的 3655 项多重集合完全相同，无删除、跳过或过滤失败用例。
- 005 XML 为 2574358 字节 / ef11f52df8c6c8c5d1a9757d03e659f26b3222f33fa18b7989a3a99477d3f0ad；007 为 2630952 / fb1d24312d0214284b1fb9a8ecbb52f929686c0dfdf2620e9268457baffeda13。五个失败 run 的 failure.json 与对应 result.json 实物相同。
- 三份 case 证据分别为 18/33/7 项；各 case result 与 XML 一致；1/58/14 条 witness 的每个字段均从实际 XML output 匹配，未以另造 JSON 代替结果。
- Compile 008 after、Tests 009 before/after、scope 最终 722 源码身份相同；36 项 DLL/PDB 与当前实物相同。相对 PUBLISH 变化为 Application、Content、Core、Core.Tests、Platform、Presentation 的 DLL/PDB 共 12 项；其他 24 项未变。
- 003/004 过程内的源码清单身份变化仅为九份新自然 meta：首次导入 59 字节、后续补全 243 字节，原 59 字节前缀和 GUID 全部保留。没有源码在有效测试进行时修改；008/009 的源码、资产、工具、内容及受保护项在各自运行内不变。

## 4. 精确范围、预算和证据链

R 于 02:33:21Z 在 C 修改前核 PUBLISH 的 787 导出中 784 不可变项全部匹配；三协调稿另列。当前 before704 与独立接收的 PUBLISH implementation704、原 source-before.json 均逐项相同。05:31:12Z 对最终实物重新核对：

| 项目 | 实际结果 |
| --- | --- |
| 原实现 | 704 中 23 项源码/asmdef 获准变化，681 项未变；两份允许但无必要改变的 Core 文件保持原字节。 |
| 新增 | 精确 5 生产 + 4 测试 + 9 自然 meta，18 项；没有额外 Assets 路径。 |
| 最终集合 | 722 实现 / 752 Assets / 398 唯一 GUID / 308 保护输入 / 810 导出，路径集合和逐项长度/SHA 与实物一致。 |
| meta | 原 389 项全字节保全；新增九项无重复 GUID。 |
| 原/补充入口 | entry-files 22；两次 entry-supplement-files 各 2，共逻辑入口 26；全部与已接收 PUBLISH 原身份相同。 |
| 最终副本 | final-files 44 项，与当前对应源文件实物逐字节相同。 |
| 保护边界 | 无越权修改旧测试/断言、旧 meta、Compiler/算法、首包/真实 review、Packages、ProjectSettings、配置、场景或构建输入。 |

独立用只读 git diff --no-index --numstat 计算增删，不采信作者数字代算：

| 预算 | 实际 / 上限 |
| --- | --- |
| 25 旧源码/asmdef 合计增删 | 479 / 2400 |
| 其中 §325 两项 | 17 / 45 |
| 其中 §326 两调用点 | 4 / 4 |
| 五份新生产源码物理行 | 651 / 1200 |
| 四份新测试物理行 | 857 / 2400 |
| 工具本次增删 | 21 / 80 |
| 工具相对原始入口累计增删 | 113 加 + 9 删 = 122 / 200 |
| 工具最终物理行 | 236 / 360 |
| C delivery / scope | 85 / 280 行；2333995 / 4194304 字节 |

Application asmdef 仅追加 FightMatch.Content 引用；工具只增加 P2C Compile/Tests 范围、固定入口身份/采集/12 槽控制，原阶段行为保持。PowerShell AST 解析无错误。未执行工具的验证动作。

当前有限根 D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2c：root-identity.json 为 668 字节 / c2aed95b9aabd2dae897623cd26af93158d505b5e0648cf673684708d4684869；read-manifest.json 为 36782 / 6697c91d2864a944360d11aa8697b9c4f9c3fa489203011d80ac9e2efb5958de。166 项与 scope、manifest 和实物全同，另加 manifest 共 167 文件；仅有 runs001～009。路径规范化、根包含关系、重复项、重解析点及物理多余文件均核过，无差异。

§325 的两份入口在修改前另有 R fresh 核对，补充采集时间 02:48:35.1025139Z；§326 的补充时间 04:12:15.202532Z。R 后次读取 §326 两源时已是获准修改后的实物，因此其原基线证明来自已接收 PUBLISH 身份和完全匹配的补充副本，不伪称 R 亲自提前看见其本次写入。原 entry、source-before、第一次补充均保留原哈希。

05:41:21Z，R 对以下五组历史有限清单重新核 scope/manifest/items、实际长度/SHA、路径/重解析点与全部物理成员，差异 0：

| 继承根（TestArtifacts/FMDemo025P2/ 下） | items / 实物含 manifest | manifest SHA256 |
| --- | --- | --- |
| p2b-publish | 116 / 117 | a68653f38151031488cf800a73fd529557f2c0cf8e9e889d514ebe3d5776ff26 |
| p2b-prepare-c2 | 75 / 76 | 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25 |
| p2b-prepare-c1 | 1442 / 1443 | c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c |
| p2b-prepare | 1442 / 1443 | 9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8 |
| p2a | 1454 / 1455 | 2cb78d0a45526e60a4435e7becf5f1c03a9ad8aafe04fec5cf392b3ae8ea197a |

首包和真实 review、所有原失败、恢复续接记录及旧 P1/027 导出身份继续保留；没有把旧模板提升成真实审批凭证。当前 C/R 报告身份从准确 formal final 外部登记，不制造自哈希环。

## 5. 接收范围与交接

本次接收的是 025-P2C 的正式 PlayerSave 源码、同 runtime 正式会话、完整创建记录及逻辑恢复、原战斗/结算/防重接入。可据此由 SD00 依原路线处理 025 整体/B17 接收，再推进 CONT-A/B/C、028、029；本 R 没有自行扩下一包、重开任务或改变功能计数。

profile-storage-plan.json 明确 ISOLATED_IN_MEMORY_PROTOCOL_TESTS_ONLY：本轮没有申请或执行新 locator/PlayerSave 物理 probe。对象关闭/重建和故障注入证明同一协议的逻辑结果，不证明真实进程强杀、磁盘 flush/掉电、Android 持久化或启动接入。物理存储根及首包平台获取仍由 029 宿主提供。原既有 B 的有限 lock/blob 证据保持，无扩路径。

完整可玩 Demo、页面/场景/物理输入/像素、Player/Android 设备、三槽和永久请求等后继范围保持未完成；§184 原未验证边界保持。34 功能 / 28 已接收 / 余 6 / 51 正向交付不由本源码子阶段擅自改计；五分钟自动跟进仍取消。

R 本次只写本独立报告，未改实现、工具、原报告、证据、真实 review 或协调稿；没有 Unity/产品 DLL 执行、Git 写操作、额外任务/代理/自动化。无待用户补答或确认，无需纠正包。本报告的字节数、SHA256 由本 R formal final 外部绑定；自然 completed 的准确时间由任务生命周期记录，报告不预填。

审查签署基准：2026-09-25T05:44:14Z；R turn=01a0d667-61ef-7ab2-88a5-dec75e498276；gpt-6-astra / max。

