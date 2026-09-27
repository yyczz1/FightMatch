# FM-DEMO-015B / DEMO-B11 · C交付

状态：实施及最终同版验证完成，交R独立审查；C不出审查结论。
交付日期：2026-09-22（Asia/Shanghai）；验证时间以下均为UTC。
原C任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1。
原实施turn：01a0c468-c6b6-79d0-b0c0-9944635525a1。
执行环境：原项目local，gpt-6-astra／max；C为本批唯一Unity执行者。
契约：system-task-packets.md §§127～131；未创建任务、子代理、分支、worktree或Git提交。

## 实施结果

CandidateBusinessSaveCodec公开Prepare／Encode／Decode，保存并恢复M03角色、M04背包、M05进度、M06历史和M07固定奖励五个owner。
输入显式提供所有owner、可空ActiveHistory及两个必填保留根列表；Prepare复制根列表，已知不可变值可共享。
结果仅为完整候选快照或拒绝，失败无部分Value，CommitEligible恒false。
Encode使用调用方给定的generation／commit／parent／index，经015A Prepare生成CandidateValidation封套；Decode拒绝PlayerSave及非准确五片/schema1。
正文使用FMBIZ001固定字段布局、UTF-16代码单元、小端定宽数和015A精确整数／有理数词法，不使用FMBR01字节作为加载格式。
M06按Baseline／Snapshot／Binding／Record／Report／Run六表恢复共享对象，并保留历史、首次Superseded、回退原Range及显式保留根。
随机域初始流、初态Snapshot流、Direct输出与AfterSnapshot.Random、Crit前后流均按原关系相连；保留原三域、Words、PRD和条件。
M05 End／FirstClear／OpenFact及M07 Ending、Report按原业务键或专属表引用连接，不用当前参与者状态替换原修订。
恢复校验原报告指纹、伤害／HP／贡献、有效前缀、历史分支、活动尝试与冻结、完整结束和各owner收讫。
记录和评分使用原值；加载不执行行动／重演、抽样、Initialize、奖励评分或经验／材料应用。
History完整性复用013的FindOperation检查，所用AssembleRecord仅比较原片段的组合关系，不调用行动求值。
所有检查沿同一调用方SaveCodecBudget.Math，正文总字节和各表／列表／字符串／数字长度有界，不把字节界声称为总堆内存界。

## 文件与范围

新增Core路径均在Assets/Scripts/FightMatch/Core/；测试在Assets/Tests/EditMode/FightMatch/。

| 新文件 | 职责 |
| --- | --- |
| CandidateBusinessSnapshot.cs | 必填输入、只读快照、显式保存头 |
| CandidateBusinessSaveCodec.cs | 三个公开入口、固定五片、要求声明、原子拒绝 |
| CandidateBusinessSaveValues.cs | 显式字段原语、原定义还原、专属引用表 |
| CandidateBattleSaveCodec.cs | M06六表、全部行动片段、历史与保留根 |
| CandidatePermanentSaveCodec.cs | M03／M04／M05定义、状态及收讫 |
| CandidateRewardSaveCodec.cs | M07原固定结果、评分证据及引用 |
| CandidateBusinessRestoreChecks.cs | 数值、定义、历史和跨owner完整性 |
| CandidateBattleSaveCodecTests.cs | 41个战斗恢复、继续行动及损坏用例 |
| CandidateBusinessSaveCodecTests.cs | 55个五片、结算、声明及资源用例 |

上述九文件各有Unity生成的.meta，共18个新增实施文件。
唯一旧文件CandidateProgressionState.cs仅在15～17行增加Participant的五参数internal原值构造；新增3行、删除0行、324字节，原.meta不变。
移除该准确新增文本后，字节SHA恢复为c1e7caabee302a8f8fb833fefce85aa9e08a11cf3532d1aabac2de6ff920f44f。
原完整字节仍在scope.originalMutableFile.content（Base64）；R可独立复算差异。
其余452个旧实施项SHA全部保持；原453项before和原时间不变，实际起点另存startedSnapshot。
最终453旧项＋18新项＝471；2599行包含九新源码、九meta和旧文件增删，低于4500行；旧文件增删低于25行。
原251个Assets路径／GUID／meta字节保持；九个新GUID由compile-2导入生成，全Assets共260个GUID且唯一，后续meta未扩展或改变。
受保护输入、依赖、设置、权限和B10验证文件均未修改；无手写meta、辅助脚本或真实存档文件。

## 七项验收及实际证据

| 项 | 通过的行为与用例 |
| --- | --- |
| ①基础五片 | FreshPublicOwnersRoundTripThroughTheActualEnvelope经020／021／022／023公共入口创建同Player，完成Prepare→Encode→015A Write／Read→Decode。PreparedRootCollectionsAreCopiedAndAllPublishedCollectionsAreReadOnly验证输入列表后改、只读集合及重复根共享。 |
| ②历史及保留根 | TwoRollbackGenerationsKeepOriginalRangesReferencesAndRecordedReplay经008→012→013构造L3两代回退，保留a／b／c／d、首次Superseded、原Range和有效d；ExplicitOldRunAndRollbackRootsSurviveRemovalOfTheActiveHistory验证退出后旧根；CrossFaceBoundary...和RescueState...覆盖跨面及救援。 |
| ③后续与随机 | LoadedAndOriginalBranchesHaveIdenticalNextActionRandomPrdAndFinalReport用相同行动／条件／时间继续至相同原报告；两代旧操作和有效操作实际调用014 ReplayRecorded并Matched。HistoricalParticipantAndConditionRevisions...保持原Participant修订1、旧偏好1与当前大修订分离。共享随机引用亦有直接断言。 |
| ④提交界限 | WpsIsASeparateRecoverableSubmissionBeforeAnyEndOrReward验证WPS尚未结束／未奖励的合法中间态。NormalEnding...用022结束、023固定、020经验／结束、021材料／结束完成普通候选；恢复后重复各公共入口不增量，FixNormal在0 log-term预算下返回原固定结果。16个PartialOrConflictingNormalSettlement变体拒绝缺收讫、错金额／材料／参与者修订、重复及异Player。 |
| ⑤定义、时间、数值 | RecoveryKeepsOldDefinitionAndExactClockDomainWithoutAdvancingTime保留旧成长定义、原时钟域、1/3毫秒精确已累计时间及大UTC值，未加载时推进。HugeSceneRevisionAndMaterialCounts...保留2^97+13级别的修订／材料整数，后续行动时间为2^100量级。普通奖励原经验26／14／31、上下界及TermsUsed保持。遗漏旧绑定、错误Rule／Numeric／Random及未知schema均拒绝。 |
| ⑥腐坏及资源 | EmptyBattleAndRewardSlicesMatchIndependentFixedBytes使用独立十六进制正文；独立修改引用、表count、nullable tag、截断／尾字节及指纹等，再经015A重建外层哈希，仍原子拒绝。历史、条件、贡献、伤害及终态负例拒绝。总字节／集合／字符串／数字／Math紧预算和末段Math耗尽无Value，扩大预算对同源重试一致。 |
| ⑦范围及回归 | 最终2522/2522 Passed、Failed=0、Skipped=0。以StringComparer.Ordinal逐个原fullname核出现重数及Passed重数，原2426条全部保持；新增96条全过，范围仅上述两个新测试类。所有最终源码对应freeze-3，编译／测试前后SHA一致；471清单及260 GUID复核通过。 |

完整原2426案例数组、实际96新增案例数组及核对规则在scope中；未以总数增长代替回归核对。
成功Link重演仍沿已接收012单Warrior支持域的边界，本包没有扩大该支持域或声称补成成功Link历史。

## Unity命令与运行

每次启动前Get-Process Unity均为空；未杀进程。所有Unity启动均使用-WindowStyle Hidden -PassThru -Wait，并读取实际Process.ExitCode。
首次沙箱启动许可证IPC超时后，后续同一批准流程使用工具沙箱外执行；自动审批未拒绝。
固定可执行文件：D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe。
编译参数：-batchmode -nographics -quit -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB11Compile.log。
测试参数：-batchmode -nographics -projectPath D:\Unity\UnityProj\FightMatch -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB11-EditMode.xml -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB11Tests.log；没有-quit。

| 运行 | Freeze | UTC开始→结束（2026-09-21） | 实际PID | Unity ExitCode／结果 |
| --- | --- | --- | --- | --- |
| compile-1 | 1 | 15:41:30.1906737→15:42:30.6335858 | 25692 | 199，许可证IPC超时，尚未编译 |
| compile-2 | 1 | 15:43:45.7169397→15:44:00.6547368 | 35752 | 0，首次编译／九meta导入 |
| tests-1 | 1 | 15:45:25.1846655→15:46:10.0134150 | 25304 | 2；2509/2518通过，9失败 |
| compile-3 | 2 | 15:50:46.7910150→15:50:58.3246274 | 36040 | 0 |
| tests-2 | 2 | 15:51:49.2690419→15:52:34.1395055 | 13976 | 2；2517/2522通过，5失败 |
| compile-4 | 3 | 15:58:13.5539102→15:58:25.6144594 | 2596 | 0，无编译错误 |
| tests-3 | 3 | 15:59:03.6233157→15:59:48.8978609 | 33120 | 0；2522/2522通过 |

首轮9失败暴露三域Initial与Baseline初始流的对象引用断裂；第二轮5失败暴露初态Snapshot流的同一要求。修复均限新白名单文件，未放宽原库检查；第三版同时恢复Crit前后流和Direct输出关联。
每次源码修正均重新冻结并完成编译与全量测试。失败日志／XML及被替换成功编译日志均先完整保存，再复用固定产物路径。
shell进程退出码与Unity退出码分开保留。compile-1打印摘要误用OrderedDictionary的Select-Object而显示null；实际Process字段已在该原命令中写入scope，随后读取确认PID=25692／ExitCode=199，未把shell的0当编译成功。

最终原始CommandExecution：
- compile-4：exec-bcb1ad01-3848-4826-81ed-5300c8fe8579。
- tests-3：exec-90015a18-511d-4fb1-9389-c66335f04dd6。
全部七次原始记录均由read_thread回读且output.truncated=false，连同准确命令、输出、状态、时长在scope.rawCommandExecutions中无损保留；validationRuns逐项连接原ID。

## 证据索引与SHA256

| 证据 | SHA256 |
| --- | --- |
| 原demo-014-scope.json | 6b5948b9ee17e9621dda8ce55482e72eb2063a09b4828897b797e66fc6149ccd |
| 原453项规范清单 | 7dec93b9baef37c7196372087df1089ea49a6b27bcd37a145732bfae652202ae |
| 原FMDemoB10-EditMode.xml | dfb08fb27958836e521021293faf502aba76a18d56873977ba1b83941497dc5a |
| 最终十份C# freeze-3 | 50e4fc1e848eb70b7abcbf9562d428cd6fbbb8ad6ffac79627f994a756ff8dd1 |
| 最终471项规范清单 | fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4 |
| Logs/FMDemoB11Compile.log | 787eeba15d35706fd49d6ed72b324758931549e1eb27536d07f13d957fde954d |
| Logs/FMDemoB11Tests.log | 7cb236f8684af3abaef1fa511b5da3d08401f64b82de53fe27f9a603049bf64d |
| FMDemoB11-EditMode.xml | 26f220d226a40fdf9ff1d0fd772f89668b7c9f86624bf458f2d26810a2f22ce7 |
| demo-015b-scope.json（499982字节） | 15babb238beeff611d851a9b82f87ff96ae2017befef1be66a1a9e322885f0f3 |

规范清单按Ordinal路径＋TAB＋小写SHA＋LF，以UTF-8无BOM计算。原before时间为2026-09-21T13:49:21.3729798Z，实际startedSnapshot原时间保留。
派发稿记录的SHA为02627b89e614e7ab271ab4b8ddec2d558eaa140b60dd59288930e88de65a6244；C实际起点及最终协调稿SHA均为ab0d00e7ea20c03881f96f0d2ecfd15ca22f3df3214afafebe50d6cdfda1adf0，业务契约未变。

scope解码：
- originalMutableFile.content：Base64直接解码原旧文件字节。
- startedSnapshot.fileList、baselineTests.caseList、regressionComparison.actualNewCaseList、rawCommandExecutions：Base64→Brotli→UTF-8 JSON，先核decodedBytes／decodedSha256；分别是完整实际453清单、原2426案例、新96案例和七条原CommandExecution。
- evidenceBundle：Base64→Brotli得到3645898原始拼接字节，SHA=a895c2ae1e8ce0792410c5eef8d9d9c1277514ca0263fe14d722c604d671719b。
- failedAttempts[*].artifacts与retiredSuccessfulOutputs[*].artifact：在上述bundle按offset截取decodedBytes个字节，再核各自decodedSha256；共七个原日志／XML，已全部解码复核。

交付边界：此包完成内存中的候选业务编码和恢复；真实文件写入、保存队列、Models安装、应用提交、PlayerSave及迁移继续由后续已规划包负责。
无需用户动作。R应等待本原实施turn completed后按§131独立审查；C交付后停止修改。
