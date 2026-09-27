# DEMO-B11-R1 · FM-DEMO-015B 独立代码审查

审查日期：2026-09-22（Asia/Shanghai）；以下执行、冻结及检查时间均为 UTC。

```text
Task: FM-DEMO-015B
VERDICT: NEEDS_FIX
Scope: PASS
Standards: PASS（范围、工程约束及证据流程）
Spec: FAIL（F1：保留 Run 的 CurrentSnapshot 可错连另一个 Baseline）
Verification: 最终同版编译 exit 0；2522/2522 Passed；静态审查发现未覆盖的引用缺口
Required user action: NONE
```

## 1. 授权、门槛与审查边界

- 冻结依据：派发 r77 的 system-task-packets.md §§127–131；派发文档 SHA256 为 `02627b89e614e7ab271ab4b8ddec2d558eaa140b60dd59288930e88de65a6244`。§132仅作为前批接收依据。
- 原 C task：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；原实施 turn：`01a0c468-c6b6-79d0-b0c0-9944635525a1`。
- 本 R task：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本原审查 turn：`01a0c469-6015-7fc3-96fc-5384ea9e8258`。
- 工具确认 C 原 turn 于 2026-09-21 16:10:18 completed。R 在收到完整交付并核实 completed 后，于 16:11:14 开启正式源码审查。
- R 在源码实施前于 14:43:33 独立记录原453项、原旧文件完整字节、89份受保护文档／产物和251个原路径／GUID；14:50的早期 scope 也与该基线一致。
- 已逐份读取七份新 Core、两份新测试、九份新 meta，独立计算唯一旧文件的字节差异，并追查相关旧入口。审查未运行 Unity、项目测试或另一个运行时复现器，未修补实现。
- 本报告中的 F1 是完整源码路径推导，附可执行化的复现步骤；没有把它写成已经运行失败的测试。
- 本轮唯一写入为本报告；未改协调稿、旧报告、源码、测试、meta、证据产物、Git或配置；未创建任务或子代理。
- 当前协调稿 §§127–131 与派发文本的差异仅为四处派发／REVIEWING状态说明，业务、白名单、接口及七项验收保持 r77。

## 2. Standards：范围与工程规范

范围核对通过，未发现与下面 F1 分离的工程规范阻断项。

| 检查 | 独立结果 |
| --- | --- |
| 继承基线 | demo-014-scope.json 原453项，时间仍为2026-09-21T13:49:21.3729798Z；规范SHA `7dec93b9baef37c7196372087df1089ea49a6b27bcd37a145732bfae652202ae`。 |
| 唯一旧修改 | CandidateProgressionState.cs 第15–17行新增五参数 internal 构造；新增3行／324字节，删除0行。其余原字节完整保留，原meta不变。 |
| 旧字节复核 | R独立保存的原字节与 scope.originalMutableFile.content 完全相同；从当前文件移除独立定位的新增字节，恢复原SHA `c1e7caabee302a8f8fb833fefce85aa9e08a11cf3532d1aabac2de6ff920f44f`。 |
| 其他旧项 | 452项SHA不变。当前唯一旧C# SHA为 `4485ddece6b4e4ab6031d4d5d491dba105c654115d7cb24e0bf09f7811b2412e`。 |
| 新项和总量 | 正好18份允许的新文件，未发现额外Assets项；最终471项规范SHA `fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4`。 |
| 行数／交付预算 | 新18文件2596行，加旧文件3行，共2599≤4500；旧修改3≤25。交付116≤240行；scope 499982≤524288字节。 |
| meta／GUID | 原251个路径／GUID／meta字节保持；新增九个Unity MonoImporter meta均11行；全Assets 260个GUID无重复，compile-2生成后到最终验证均未变。 |
| 保护范围 | 首次正式核查89份保护项全未变；16:28:59复核仅session-plan.md、integration-review.md、system-task-packets.md三份协调文档发生状态登记变化，其余86份保持。C原回合写入记录不包含这三份协调稿。 |
| 依赖／格式／API | 未修改旧015A、asmdef、Packages、ProjectSettings、权限、旧序列化格式或旧公开接口；新增业务schema及三个公开入口在本包授权内。 |
| 实现边界 | 固定类型与字段，六张M06表按对象首次访问编号；没有反射加载器、随机初始化／抽样、行动重演、重新发奖、文件提交或Model安装。共享调用方Math预算。 |
| 原执行审计 | 独立读取C原turn的99条CommandExecution及20条FileChange；七次Unity启动与scope对应。源码最后修改15:56:36，早于freeze-3；后续只有scope／delivery写入。Git只有原旧文件的只读diff。 |

原453清单使用 Ordinal 路径＋TAB＋小写SHA＋LF、UTF-8无BOM复算；最终471清单采用同一规则。未用文件数量代替逐项比较。

## 3. Spec：阻断发现

### F1 [P2] 为所有保留 Run 核实 CurrentSnapshot 的同一 Baseline 引用

位置：[CandidateBusinessRestoreChecks.cs:316](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs:316)，涉及第316–325行，尤其第324行的当前快照比较。

`Run` 核实 Binding／Baseline／InitialSnapshot 的同一对象关系，却只对 `CurrentSnapshot` 做修订与 FMBR01 值比较，遗漏 `ReferenceEquals(run.CurrentSnapshot.Baseline, run.Baseline)`。FMBR01 的 Snapshot 比较仅写入 `Baseline.Entry.EntryBaselineId`，没有比较 Baseline 对象身份：[CandidateBattleReportFingerprint.cs:153](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleReportFingerprint.cs:153)。

活动History另经013的FindOperation获得引用检查；仅列在RetainedRuns的无活动旧Run不经过该检查。因此另一张表内“值相等但不是同一个原Baseline”的快照能被当成完整可恢复结果返回。这违反§127–129明确的同一原对象连接和保留根自身完整性要求，也漏掉验收②／⑥的错误引用拒绝。

**精确触发步骤（源码推导，R未执行项目测试）：**

1. 用相同合法 PreparedEntry、相同种子材料分别调用008／012，得到值相同但 Baseline、Binding、初态Snapshot对象不同的 `runA`、`runB`；两者Records为空且无FinalReport。
2. 将两Run按A、B顺序放入显式RetainedRuns；用022／020／021的正常退出形成完整永久状态，使ActiveHistory、ActiveAttempt、ActiveCarry均为空。两Run仍属于同一合法旧Attempt。
3. 正常编码该候选。此图的 Baseline、Snapshot、Binding、Run各表均按A、B顺序收集。
4. 独立修改M06第一个Run行的 `CurrentSnapshotRef`，从A的初态行改为B的初态行；其他字段保持，再经015A Prepare重建外层哈希／封套。
5. Decode分别还原两份合法基线和初态。第316行仍通过；第324行的值比较也通过；每份Snapshot单独对自己的Baseline有效。无活动History，故第45–51行不会调用013的完整History检查。
6. 重新收集时仍先访问A的Binding／Initial，再访问B的Current；B的Run随后复用这些行。所有表顺序仍为A、B，故第72–76行的CanonicalLayout比较也不能拦截。
7. 输出的首个RetainedRun仍是 `Binding/Baseline/Initial=A，Current.Baseline=B`。原012下一行动先执行CheckRun，并在 `SnapshotReferences` 第335行以 `InconsistentBinding / Run.CurrentSnapshot.Baseline` 拒绝：[CandidateBattleOperations.cs:162](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleOperations.cs:162)、[CandidateBattleOperations.cs:335](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleOperations.cs:335)。

同一问题也能通过internal负例构造这个混合Run后调用Prepare观察。最小修正是在现有Run完整性校验处显式核同一Baseline对象，并覆盖所有表内Run，保留当前允许的回退修订间隔。无需修改012／013／014／015A或扩大支持域。

**现有测试为何未覆盖：** [CandidateBattleSaveCodecTests.cs:67](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleSaveCodecTests.cs:67)检查合法无活动保留根；第183–200行的损坏分支都放在活动History；第242–252行的字节引用负例是越界RunRef／标志／表计数，没有“合法表序号指向另一同值Baseline”的负例。

## 4. 七项验收逐项结论

下表区分已核证据与缺口；测试通过不替代源码正确性。

| 验收 | 证据与判断 |
| --- | --- |
| ①基础五片 | BusinessSaveCodecTests:16–45、BattleSaveCodecTests:169–180及共享场景:251–311、378–390实际经020／021／022／023、015A内存Write／Read完成五片往返。准确owner/schema、CandidateValidation、CommitEligible=false、输入根列表复制与重复根共享均有断言；未发现该项阻断。 |
| ②历史／保留根 | BattleSaveCodecTests:15–103实际两代L3回退、a/b/c/d原序、首次rb1/rb2 Superseded、Range旧Entry、有效d、清活动态后的旧根、跨面和救援均通过。F1表明无活动保留Run的错误基线引用未拒绝，本项未完整满足。 |
| ③后续／随机 | BattleSaveCodecTests:51–63以相同条件／大时间继续原、恢复两分支至同一最终报告；第36–44及294–301行实际014 ReplayRecorded并Matched。原Participant／Conditions修订与当前大修订分离；三域Initial、初态流及Crit／Direct／After共享引用有断言。正常图路径证据成立；F1的畸形保留图仍需拒绝。 |
| ④两次提交 | BusinessSaveCodecTests:49–86检查合法WPS未结束／未发奖以及完整联合普通结束。原经验26／14／31，020、021、022重复办理无增量，023在0 log-term预算下取原固定结果。第129–162行16个负例加RestoreChecks:197–245的逐项关联检查覆盖主要缺账／冲突；未发现该项独立阻断。 |
| ⑤定义／时间／精确值 | BusinessSaveCodecTests:91–114保存不同旧成长定义、原Clock域、1/3毫秒Elapsed、旧绑定；BattleSaveCodecTests:107–154保留大修订、时间、材料计数。RewardSaveCodec:45–59安装原Score上下界／Amount／TermsUsed，未重新调用评分；要求由实际引用生成并在Decode重提取核对。该范围证据成立。 |
| ⑥腐坏／资源 | 已有独立十六进制见证、重新封套后的越界引用／表count／非法nullable／版本／截断／尾字节／指纹等负例，及完整记录片段、总字节、集合、字符串、数值token与末段Math预算负例；均无Value且可扩大预算重试。F1的同值错连引用未覆盖并能穿过现有检查，本项未完整满足。 |
| ⑦范围／回归 | 452旧项保持、仅允许构造器差异、18新项、471总项、260唯一GUID、最终同版编译／全量测试、所有失败归档均独立核对。2522全部Passed，其中原2426逐名重数／Passed保持、新增96全部通过，本项证据成立。 |

所有测试定位均指本次冻结文件；表中简写分别为 Assets/Tests/EditMode/FightMatch/CandidateBusinessSaveCodecTests.cs 和 CandidateBattleSaveCodecTests.cs，源码定位为 Assets/Scripts/FightMatch/Core/ 内对应文件。

R在接触新实现前于14:51:09按冻结§129独立推导最小M06：PlayerId=`p`、六空表、显式空History、两空保留列表，共52字节，SHA `560ac22a0c887e7455257b3692f9ebdc59b444ab4316a186e911e88d00822462`。新测试第38–41行用11代码单元的 `player:015b`，同一固定布局为72字节；另有M07固定字节。该见证确认布局，但不掩盖F1的对象图缺口。

## 5. 原Unity执行与失败保留

七次记录均按原CommandExecution ID回溯C原turn，解开工具展示的shell引号后，归档命令与原命令一致，stdout／shell退出码也一致。以下是实际Unity Process.ExitCode，而不是外层PowerShell的0。

| 原运行 | UTC开始→结束（2026-09-21） | PID | Unity退出／测试 |
| --- | --- | --- | --- |
| compile-1／freeze-1 | 15:41:30.1906737→15:42:30.6335858 | 25692 | 199，License IPC失败，未编译 |
| compile-2／freeze-1 | 15:43:45.7169397→15:44:00.6547368 | 35752 | 0，首次九meta导入 |
| tests-1／freeze-1 | 15:45:25.1846655→15:46:10.0134150 | 25304 | 2；2509/2518 Passed，9 Failed |
| compile-3／freeze-2 | 15:50:46.7910150→15:50:58.3246274 | 36040 | 0 |
| tests-2／freeze-2 | 15:51:49.2690419→15:52:34.1395055 | 13976 | 2；2517/2522 Passed，5 Failed |
| compile-4／freeze-3 | 15:58:13.5539102→15:58:25.6144594 | 2596 | 0 |
| tests-3／freeze-3 | 15:59:03.6233157→15:59:48.8978609 | 33120 | 0；2522/2522 Passed |

- 七次原命令均先核冻结SHA和Unity无占用，固定2022.3.18f1可执行路径、原项目路径，Hidden／PassThru／Wait／WaitForExit；测试不带-quit。每次运行后十份C#均匹配其对应freeze。
- compile-1原摘要因OrderedDictionary管道显示null；原启动命令实际保存Process.Id／ExitCode，原follow-up `exec-a5d3ac24-7f80-4a6e-9186-f40cb4d83471`读回25692／199，与700字节许可失败日志一致。没有把shell成功误报为Unity成功。
- 第一次9失败均为恢复后 `Binding.RandomDomains`；第二次5失败均为 `Binding.Start.Snapshot`。已读修正diff，分别恢复同一Initial流与Snapshot／Crit／Direct关联，增加引用断言；未放宽旧模块检查或移除旧测试。
- 完整失败日志／XML及两份被替换成功编译日志共七份，从Brotli bundle无损取回，逐份长度／SHA通过；bundle原文3645898字节，SHA `a895c2ae1e8ce0792410c5eef8d9d9c1277514ca0263fe14d722c604d671719b`。
- 七次CommandExecution压缩原文41233字节，SHA `57988d5b67f77466005677ca80d40092c44e96122f67e482053c8658c86b698b`，解压后七个ID／原命令／未截断输出均有实际记录。
- 第三次源码冻结15:57:58.1179819，晚于最后源码修正、早于最终编译／测试；最终meta与第二次编译生成字节相同。
- 最终日志无C#编译错误或警告；日志仍含许可客户端签名／token非致命消息，不能概括为全日志零Error。最终实际进程exit0和XML全通过已分别核实。
- 16:27:33检查没有Unity进程。R未启动或终止任何Unity进程。

最终CommandExecution：编译 `exec-bcb1ad01-3848-4826-81ed-5300c8fe8579`；测试 `exec-90015a18-511d-4fb1-9389-c66335f04dd6`。

## 6. 冻结证据与回归明细

| 证据 | SHA256 |
| --- | --- |
| C交付 demo-015b-delivery.md，12140字节 | `0b1b8ba11844b58d8cb47ed9ad05d888ab36526fdf5cb515369e1066f4392855` |
| C范围 demo-015b-scope.json，499982字节 | `15babb238beeff611d851a9b82f87ff96ae2017befef1be66a1a9e322885f0f3` |
| 十份C# freeze-3 | `50e4fc1e848eb70b7abcbf9562d428cd6fbbb8ad6ffac79627f994a756ff8dd1` |
| 最终471项规范清单 | `fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4` |
| Logs/FMDemoB11Compile.log，61859字节 | `787eeba15d35706fd49d6ed72b324758931549e1eb27536d07f13d957fde954d` |
| Logs/FMDemoB11Tests.log，89052字节 | `7cb236f8684af3abaef1fa511b5da3d08401f64b82de53fe27f9a603049bf64d` |
| FMDemoB11-EditMode.xml，1664438字节 | `26f220d226a40fdf9ff1d0fd772f89668b7c9f86624bf458f2d26810a2f22ce7` |
| 原FMDemoB10-EditMode.xml | `dfb08fb27958836e521021293faf502aba76a18d56873977ba1b83941497dc5a` |

原XML为2426案例／2421个Ordinal fullname。R直接读两份XML逐名比较出现数及Passed数，差异0；新增恰96，BattleSaveCodecTests 41例／17方法，BusinessSaveCodecTests 55例／11方法，全部Passed，最终Failed／Skipped／Inconclusive均0。scope内压缩的完整原案例及新增案例列表也解压后与实际XML多重集合逐项一致。

原fullname＋TAB＋Count＋LF规范SHA为 `0329f83314470008f49cb3897e50091e0339c88c384c3c47821f527d7e1f8688`；再加逐名Passed列的SHA为 `c489affd62c5c0343687ce421774e5c00fbd0a7ccd8065d343bf20cd32704446`。未把新增总数抵消旧例缺失。

## 7. 最小修正交接与复审门槛

由SD00据F1准备更小修正包，本报告不授权C直接开工，也不替SD00修改协调稿。

- 修正目标：在现有Run校验核准确CurrentSnapshot.Baseline对象引用，对Prepare及Decode都原子拒绝；不得按相同EntryBaselineId合并或替换错误引用。
- 建议最小源码范围：CandidateBusinessRestoreChecks.cs的Run校验和CandidateBattleSaveCodecTests.cs的相应用例；证据文件、运行者和新验证产物路径由SD00另行精确指定。
- 必需负例：无活动History、同值但不同Baseline的两份保留Run；Prepare的混合对象负例，以及独立改M06 CurrentSnapshotRef后重建015A封套的Decode负例。断言指定引用字段拒绝且Value为空。
- 必需正例：同一正确Baseline的旧保留Run／Rollback仍恢复；现有高修订、两代回退、继续行动／014重演、WPS与联合结束／重复增量0保持。
- 修改后重新冻结，C作为获准运行者完成同版编译及EditMode全量验证；保留本批2522完整fullname重数／逐名Passed和新增例，保存所有失败／重跑原始证据，再交R复审。
- 保持旧452项、旧构造器之外的原逻辑、所有meta、015A格式／入口、012／013／014规则、依赖及配置不变。无需回滚已接收前批。
- 成功Link历史仍受已接收012单Warrior支持域限制；本包不要求新增该能力。
- 真实文件保存、M02应用操作切片、Model安装、PlayerSave／Published／H10仍属于后续工作，未当成本包缺陷。

本报告交回后本原R回合结束；SD00须确认原回合completed后安排修正。用户当前无需操作。
