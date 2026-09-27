# DEMO-B14B-C1-R1 · FM-DEMO-018B-C1 独立复审

VERDICT: ACCEPT
Scope／规范轴: PASS
Spec／规格轴: PASS — 原 R1 已关闭，C1-01～06 均满足。
适用范围：仅 FM-DEMO-018B 经 C1 修正后的交付；本报告不放行 018C、018 整包、正式 PlayerSave 或真实写门。

## 审查回合与恢复事实

- R 任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`，原项目 local，按 gpt-6-astra／max 独立复审。
- 原审查 turn：`01a0c8c8-30b9-78e2-874c-0fde5e756576`，原生 task_started 为 2026-09-22T11:02:38.033Z。该回合异常中断，没有正式 final 或本报告；按 SD00 恢复指令保留 interrupted／error=null 事实，不补写为 completed。
- 本次实际恢复 turn：`01a0c905-2f49-7c20-82cb-e77c01c43184`，原生 task_started 为 2026-09-22T12:09:15.369Z。本次承接原审查，未重新实施 C1，也未倒填原审查的完成时间。
- C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；准确原 C1 实施 turn：`01a0c8c7-6576-7841-8e87-32b65a7541dd`。
- C formal final 为 11:35:11.066Z、原生 task_complete 为 11:35:11.252Z；原审查及本次恢复均直接确认 completed／error=null。R 在正式交付与准确原实施回合完成两条件齐备后才审补丁。
- 依据冻结 `system-task-packets.md` §196～198；r98 派发 SHA256=`6bf95baadb658c0d9f6690bc06ea6077fe5c857206dd36c2ca940c638417daa1`。本次恢复实核 §188～193 与 §196～198 均等于 C 入口保存的原文。
- §196～198 的 UTF-8 TrimEnd SHA256=`27ebc6dea4e266c66b0acf202d717fb46f41952608e7b8d507952cf44ad74dd1`；§188～193 为 `b7c41bb2fd6b6fc4d174ec2fcee4eac96de0ba8c79fad1c73b90aa9c3aedeed3`。
- 文内时间均为 2026-09-22 UTC。证据根 E 指 [本次 C1 证据目录](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14B/84c638ca1bea4160b6df02ed63ebf5e4/)。
- R 沿 code-review 的规范／规格两轴独立检查；按本包明确限制单任务执行，无子代理、外部模型、Unity、项目测试、额外探针或验证工程。仅新建本报告，其余只读。

## 正式交付与入口基线

| 对象 | R 实核 |
| --- | --- |
| [C1 交付报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018b-c1-delivery.md) | 9032 字节、100 行；SHA256=`101c8c1848341b5b54c7f5a7f44d68e63e61a148a7d8c0b82a6d4ed26e352efb`，低于 220 行。 |
| [C1 范围清单](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018b-c1-scope.json) | 1554907 字节；SHA256=`90a1ca214d0adbb5a9387b2c940ea1595b789b6cf064b41439627cf235a68467`，低于 1572864 字节。 |
| [原 R1 报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-batch14b-code-review.md) | 18856 字节；SHA256=`ee7a9fbccfffc60bf24d44db2e956be9a6dcfe3d9cb17714688a127616a5da3e`，保留原 NEEDS_FIX 结论与历史，不回改。 |
| 原 018B 作者报告／scope | SHA256 分别为 `551017d665de871bf3504194c06a4e0f8cc3aa5eeebb3e1b87f178ad5bcb01bb`／`26375dd34742298c69869491f47f7e9a540f07906ad6d8b747b62ecb2e795f65`，保持原字节。 |

R 于 11:04:05 独立核到原 540 项全部一致、三份可改文件仍为原版本，八个 C1 报告／日志／XML 目标均不存在；这早于 11:08:19 的首次测试修改。原 540 canonical=`466dda28a5316343cf90f0021c64ffaf3b61c0ab22c99ce9bb7f5a4f8a13096e`。

C1 的 beforeImplementation 完整继承原 540 清单及 capturedAtUtc=`2026-09-22T10:08:41.9021275Z`；另记本次 startedSnapshot=`2026-09-22T11:04:13.9061899Z`。R 逐字段比较继承清单，没有把新哈希标成旧时点。

## 规范轴：实际差异与范围

实际 diff 基于原 018B 冻结源码字节与当前文件。普通相对 HEAD 的空 diff 不作为无修改证明；三份文件原本未跟踪，R 使用只读 `git diff --no-index` 读取实际差异，并核对基线 SHA。

| 唯一修改文件 | 新增／删除 | 最终行数 | 最终 SHA256 |
| --- | --- | --- | --- |
| [CandidateApplicationReferences.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs:324) | 11／2 | 492 | `b18d245c66cd2aeec13a51ea356ed185e93ec8fa1a658d1b4149fab0505ffd06` |
| [CandidateApplicationProtocolTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateApplicationProtocolTests.cs:212) | 125／0 | 418 | `eea8f87b5e4cf530024dedd5072ef69ef7453395bb46e80f29e30a3e62faa9ee` |
| [CandidateApplicationSaveCodecTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateApplicationSaveCodecTests.cs:221) | 79／0 | 337 | `0cc35c3f4c0e8ec54e8831fdc9651ad0b87eca721853472585034f111289f484` |

- 总新增＋删除 217 ≤360；六生产文件共 1717 ≤2600 行，十份 C# 共 2959 ≤4600 行；收证 PowerShell 为 121 ≤260 行。
- 仅上述三份有差异，其余 537 项逐字节保持，含原 520 项、其余七份 018B C#、旧领域／测试、Tools 四份源文件、asmdef、QFramework 和全部 meta。无 API、schema、依赖、配置、资源或权限修改。
- 实施 540、Scripts＋Tests 526、完整 Assets 553；meta 295，GUID 295 个且全部唯一。独立枚举的 Assets 路径集合与冻结清单一致，无新增或缺失资产。
- 两份测试文件只有插入，无原行删除／替换；原测试断言和用例保留。生产差异只落在 IgnoredTimeRegression 校验分支，未添加抽象或相邻重构。
- 最终 540 canonical=`fe791423a3b2422c08da18158b37556b747d7c1c19533afac8ba0e3440b592e0`；不变 537 canonical=`702bfbe44bc785ee8ffd6f779f39d44d7b8a49752f769554d044fc1a5235699a`。
- 原 520 canonical=`fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6`；最终 Assets canonical=`96dc1463a27fed25efa1df132207d837c7b7cff0e97980e735b648f79f44bad9`；meta canonical=`07e878c201d58b3d238c7b81fc7d9eba1b64557623242276ec94ec77b7eec004`。

## 规格轴：R1 关闭及 C1-01～06

| 验收项 | 独立结论与直接依据 |
| --- | --- |
| C1-01 准确红阶段 | PASS。公开 ApplicationScenario 的 Enter→Attack(down)→Exit 链得到修订2、LastWall100；真实 Clock99 产生 Ignored，另一次真实 Clock101 明确产生 Applied／修订3／Elapsed1。原生产下两项新增“应拒绝”断言均因实际接受而失败，非编译、夹具或预算失败。 |
| C1-02 时间与异常 | PASS。原交叉输入现在于 Propose 以 InconsistentBinding 拒绝，basis／原结果保持；四例覆盖同样本、单调增大或相等而 Wall 反向，二例以真实领域结果交叉匹配合法 Sample.Anomaly，分别拒绝缺少／额外 DomainChanged。无内部成功结果伪造。 |
| C1-03 合法历史 | PASS。六例覆盖负 Wall、精确 1/3 对 2/3 的单调倒退、Wall 前进而单调倒退、跨域、无输入单调域、域名大小写及超过64位的负 Wall。历史链随后变域、推进、完成，旧 Ignored 仍恢复原 sample／elapsed／revision／Period.Anomaly 与 ResultAnomaly；四 Outcome 与 AlreadyIncluded 的旧 revision 优先保留。 |
| C1-04 Decode 语义拒绝 | PASS。真实合法 Ignored 六片存档中，仅替换 M02 原意图的 TimeSample 规范字节及长度，旧结果及其余五片保持；原 Envelope Prepare→Write→Read 成功后，Decode 才以 InconsistentBinding 拒绝，Value=null。历史查询保留原 Commit／Generation，五片独立编码 bytes 一致。 |
| C1-05 同版全量绿 | PASS。最后源码改动后真实编译与全量 EditMode 实际 exit0；原 2944 名称多重集合保留，新增15项全部 Passed，零跳过；红阶段两个方法正文与最终版逐字符相等。 |
| C1-06 范围与完整证据 | PASS。三文件差异、537保持、行数／清单／GUID符合限制；旧报告及证据保持，source／payload／ZIP实际全量枚举与逐项长度／SHA比对一致；末端记录与准确原生命令及正式交付哈希绑定。 |

[References:324](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs:324) 明确取 `historical = receipt.Period` 的 LastAcceptedSample。sameDomain 要求旧域非 null 且 StringComparison.Ordinal 相等；同域用 ExactRational.Compare，否则用共享 budget.Math.Compare 比较有符号 BigInteger Wall。条件严格为 comparison<0，ResultAnomaly 精确等于旧 Period.Anomaly、输入 Anomaly、必需 DomainChanged 与 ClockBackward 的并集。

此处只比较，未调用 Advance、系统钟或重新计算 Elapsed。PrepareTime 与数值比较继续使用同一调用者 Math 预算；新增历史往返用例还验证总预算减一步时结构化 Limit 拒绝。Applied／Unchanged／AlreadyIncluded 分支原字节保持；AlreadyIncluded 位于该新增分支之前，不被新时间检查或旧 ExpectedRevision 拦截。

影响核对覆盖共享调用路径：Propose 的 References.Validate→Resolve→Recovery、Decode 还原业务对象后的同一 Validate，以及 Lookup 对原记录的 Validate。OwnerHistory 仍按记录顺序验证原 Period、修订与完成状态。失败仅放弃局部候选／解析结果，原 basis 不追加记录；旧历史不会被后来当前 Period 的域或异常污染。

[历史回归](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateApplicationSaveCodecTests.cs:255) 特别保留旧回执 scope=clock-original、Wall=-500、单调与Elapsed=2/3、Period.Anomaly=None、ResultAnomaly=ClockBackward；后来当前 Period 已变域并完成。它能区分“使用历史来源”和“错误套用当前来源”，不是只验证同一当前对象。

原 B01～B04、B06 及 B07 的格式／旧五片／一般资源边界结论，已在原七份未改源码、原有效证据保持且全部旧测试通过后复用；B05 与 B07 的 R1 缺口由上述真实红绿关闭。原 B08 证据保持，C1 新证据另外核验。保留原已声明限制：成功战斗历史链使用 Attack，未新增公开完整 Run 的 Link 成功链；Context 继续既有七个具名字段和原布局，不按原文计数笔误扩格式。

## 红阶段来源、顺序与断言保全

- 原生 FileChange 于 11:08:19.807Z 只新增两份测试；首次生产修改为 11:13:56.5555198Z，三文件修改事件完成于 11:13:57.284Z，晚于有效红阶段退出。
- red-01 与 red-02 的 before／after 完整 540 中，仅两测试不同于入口。生产 References 仍为 `de4a94df44975a4c63306a448babf6445929b4a215048d1fefe13d1753136bb5`；红期 540 canonical=`9d5cfa83241b265baae2b22bf78659d90b8eb9cd7fbce054fdf79f4b421b0e42`。
- red-02 实读 71 项＝原69 Passed＋指定2 Failed，零跳过。失败分别为 `RealIgnoredRecoveryCannotCompleteAnIntentWhoseTimeWouldAdvance` 与 `ValidEnvelopeCannotPairAnIgnoredReceiptWithANonRegressingIntent`；两者均为 `Expected: False / But was: True`，栈落在 Rejected 的 IsAccepted 断言。
- 前一个测试在失败断言前已验证真实 Clock99／Clock101 的不同领域结果；后一个在失败断言前已完成有效外层摘要的 Write／Read。没有用错误摘要或伪造结果冒充 R1。
- 两个方法正文最终版与红期完全相等，UTF-8 SHA256 分别为 `58f79940e9285717ce4ae018a826c386984c2102223e48235f69c3ed32f989eb` 与 `7d89a75a51beb0955e801a741d497a847c362f726cdb2dd54cc4fdbd1dc5a0ba`。
- red-02 Core DLL 保持原 `346ea912dad161b7218d8a4b8594f2c6d48c057662f4b4683cbd01a681c2abd6`；Tests DLL 已变为 `00641d7c7ab28ace65805c94b5451a0d90451d4954ef4f015c02946d6388b9cb`。源码冻结、新测试名称与程序集共同绑定真实红运行。

## Unity、测试与同版绑定

C 唯一运行固定 `D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`，cwd／projectPath 均为 `D:/Unity/UnityProj/FightMatch`。四次启动前均保存无 Unity 进程证据，Hidden／PassThru 启动；WaitForExit 后先持久记录实际 Process.ExitCode，再收源码、日志和 DLL。参数与原生命令一致；编译带 -quit，测试不带；红期仅筛选 ProtocolTests／SaveCodecTests，绿期全量，无终止用户进程。

| 阶段 | PID | 实际起止 UTC | Unity exit／原生 wrapper exit | 结果 |
| --- | --- | --- | --- | --- |
| red-01 | 12304 | 11:08:24.2997535～11:09:24.5826118 | 199／1 | License Client IPC 拒绝及60秒超时，无 XML；完整保留，不计为缺陷复现。 |
| red-02 | 32320 | 11:10:49.1274846～11:11:34.2221180 | 2／1 | 69 Passed、准确2项错误接受断言 Failed。 |
| compile-01 | 3988 | 11:14:05.7504573～11:14:14.9012868 | 0／0 | 最终源码编译通过。 |
| tests-01 | 11944 | 11:15:52.9364559～11:17:06.3625788 | 0／0 | 2959／2959 Passed，零失败／跳过。 |

四个原生命令分别为 `exec-a4716167-788d-475a-946a-e93ad0e2ff75`、`exec-60fdcfdf-6c6a-4d30-91ed-300c5094a28f`、`exec-4b4c6923-d696-4d92-9acb-4d8845fd4dcb`、`exec-2a6b88e3-b085-4378-8188-0a03f96bf5b3`。R 核对其实际 stdout、cwd、参数、UTC、wrapper 退出与独立进程结果，不使用 chunk_id 代替真实退出。

R 实读原 XML 2944 case／2939 个不同 fullname；最终2959 case／2954 个不同 fullname。按 Ordinal 字符串及每个名称的出现次数比较，原多重集合全保留，新15例＝Protocol13＋SaveCodec2，全部 Passed。五个原重复名称没有被去重掩盖。

| XML | 字节 | SHA256 |
| --- | --- | --- |
| 原 018B 基线 | 2005804 | `60a89e5366a23fd374d7da0df2d1e271578e3df0918df43014fc7ed4a41e5808` |
| C1 Red | 55214 | `c6cbbf91f29fba46f4dc2ba4c64ce4ab2c754cfd8069c00f0d18dd0c5fc358b8` |
| C1 Green | 2016629 | `b7fe984a389ff9686e71a92bb19235238ba9155a969d1f72b6b677ea44dead2a` |

R 实核入口754份源码／保护副本、四次各 before／after 的737份副本（540实施＋197保护），及每轮28份 DLL／PDB。最终编译前后、测试前后和当前540源码一致；compile／tests／恢复时当前28程序集 canonical 同为 `c0c5eac0f7b94802ca71fd2b9ca32e38a02f865a6e6d851aacb76954a7d87362`。最终 Core DLL=`993b39696db3f1dfe85bac34247d85485049ed31fd9fc47b57fd38542b7c182a`；Tests DLL=`24889f1f6be5328b99bd6898a8fbf7344f6ef594b168ca29f75399e0732fdc99`。

## 旧证据、全集归档与末端绑定

- R 于11:05:08～11:06:30独立枚举28个旧根，11:38:38完成逐根重核：39271文件、1209926735字节、路径／长度／SHA均保持；canonical=`172700080c8b0d0e1ba6dc015d1451b54bfd9f5af55ae043c9eb7d4cc0175028`，无 reparse。
- R 另于11:46核到457份文档／规则／配置／权限、109份旧根 XML／日志／工具输出和295份meta均保持入口哈希。本次恢复又核197个包级保护输入、全部Assets／meta、作者原新报告、基线XML及最终程序集；无差异。
- 本次自然输出仅新增 B12/`2e5bbba73db04979a1d581721bcdada7` 与 B13/`4e1aae90283d4e399b482e54d2e4a736` 两个 Guid 根，共1055文件，完整纳入归档；E 为另一个获准的新根。
- R 于11:42:05～11:43:09独立枚举实际 Assets、E（排除 archive／late）、两个新自然根及770个明确额外输入的并集；payload 另行遍历，ZIP每条实际解压读取计算 SHA，未拿作者manifest互相比对代替物理枚举。
- source／payload／ZIP 各8692项、展开424490612字节；三方路径、长度、SHA一致，缺失／额外／重复／字节差异均0，无ZIP目录项或不安全路径。三个作者manifest与实际集合另行比对一致。
- 三方 canonical=`5cdac52a39bb20e9b47365ae35dfc3ac8a0ce0bb72a8d56f83d228b05e10d478`；ZIP为88060855字节，SHA256=`49372fd43387bfa9719647145ad73fd9f936700bc8e7d7f7dccafcbd7bea3313`。
- scope 的 Brotli 内容实际解码17786851字节，SHA256=`84fb91a0f160d010f0299f02701690f7e9ae2dcc7ed34524703366308e35a9f7`。恢复时再次核全量快照的数量、总字节与重算canonical，包括39271旧Artifacts、540起点、537保持、520原项及8692 archiveSource，无省略或重复行。
- 明示自引用排除为 E/archive、E/late 与最终两份作者报告；四次运行及失败源／产物完整在ZIP中。late及正式报告另绑定，没有宣称ZIP包含自己。
- 准确 C turn 共37个原生 CommandExecution。四份导出联合覆盖35个，逐行等于真实事件；余下是末端导出自身与最终哈希读取，分别由 final-tool-evidence 原请求／结果与 formal final 的哈希闭合。
- 恢复时核最终门的初次输出50字符＋后续1061字符，恰等于原生累计1111字符；请求、exit0与合并输出均一致。末端导出请求／返回也完全一致，原有分段差异不是作者证据缺失。
- [final-tool-evidence.json](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14B/84c638ca1bea4160b6df02ed63ebf5e4/late/final-tool-evidence.json) 为17029字节，SHA256=`067e98cd107e74e9d2c0a1c12435c93b39a46c3186da76556dc23e7223e53dab`，与原生最终哈希读取及 C formal final 一致。末端单文件计数脚本失败及其修正后的有效核验均保存在late，未冒充业务红阶段。

## 恢复复核与必要动作

原回合完成的范围、红绿、旧证据及全集归档核验，由本次从原回合原生 CommandExecution 取回实际结果后复用；不是只采信作者总结。原核验事件包括 `exec-8349dcf3-61ff-4cb4-982f-3769fc289e36`（旧根）、`exec-d45bf953-5d64-40da-aebc-37c9d6c652b3`（全集归档）、`exec-1563e099-8db2-43f6-b488-25f05778c9d9`（红绿与源码副本）、`exec-e0f2b4f0-88f8-47bf-8542-8981036999a2`（原生命令）。

本次12:13:59～12:14:08重新读取当前540实施、完整Assets、meta、197保护项、28程序集、作者报告、XML／日志、17项直接证据、三份后续证据、三份归档manifest及ZIP，哈希均保持；12:15:08完成剩余末端原生命令绑定。没有将这些新时点写成原历史，也没有重复运行 Unity／项目测试。

没有待修正发现，不需再签 C1 返修包。SD00应在本次恢复 turn completed 且本报告正式结果齐备后接收018B修正版；018C仍须按自身派发条件处理。本报告不改变功能包计数，不代替018C、019、024、025、026-G2、018整包、PlayerSave／真实M12写门、Domain Reload、场景／触控／029体验或正式发布的验收。

按本次恢复指令，由SD00直接读取报告及完成状态，不再次发送受限通知。R交付本报告后停止修改。

