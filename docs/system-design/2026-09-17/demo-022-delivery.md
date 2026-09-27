# FM-DEMO-022 固定开放事实、挑战与尝试关系

状态：WORKER_RETURNED；联合验证通过，D已核本包用例、范围、产物及A最终执行回执，交回R独立审查。本文不作独立审查结论。
D任务：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；本次turn：01a0c303-1f1c-7993-a74b-1238a5f1e8b9。
授权：system-task-packets.md r70 §97／99；A仅执行011，R按§100独立审查。

## 实现与职责

仅新增四份Core、一份EditMode及本报告／scope；五份meta由A通过Unity自动生成，D未启动Unity或手写meta。
公开纯入口集中CandidateProgression：PrepareDefinition、CreateCandidate、Read、CheckEntry、BeginAttempt、EndAttempt。
定义显式校验Ordinary、空额外能力、唯一关卡／规则ID、前置存在／无环／初始节点，保留输入定义顺序。
新建仅形成隔离空候选及明示初始开放依据；没有真实内容发布、旧档恢复／补发入口。
每关保留唯一未关闭Challenge及其先后Attempt；全候选至多一个未结束Attempt。旧挑战、入场收讫、结束和首通依据均保留。
CheckEntry不创建关系；通过020公开ComputeBaseStats消费真实CandidateCharacterState，以同一预算核身份／Context／原槽／M03修订及Ready。
Begin准确重试先返回当前state／原收讫，角色后来进入恢复或尝试已结束都不复开；新办理才核当前关系与修订，一次形成挑战和尝试。
胜利只关闭准确挑战，首次形成原Context／Version／Challenge／Attempt／Settlement／报告指纹依据，再按明示边开放直接后继。
重打保留原首通及开放来源；退出仅结束尝试，挑战保留；重来保留原Challenge／Baseline／角色修订及原槽，旧结束与新Attempt同一候选。
结束防重按原Attempt完整事实，Settlement与结束身份不可跨尝试重复归属；冲突及数学Limit无部分Next。
所有输出及集合只读，输入壳复制；本包历史修订及输入M03数值均经调用者的同一ExactMathBudget重验。
测试只经020公开入口构建Ready／Recovering／恢复完成的M03状态；未调用GrowthChecks、借friend构造M03状态或消费011／021接口。

## 七项验收映射

下列行为见证均已通过本批同版EditMode；CandidateProgressionTests共92/92 Passed。scope.independentVerification记录28组方法的展开数与通过数。

| 验收 | CandidateProgressionTests行为见证 |
| --- | --- |
| ①规则与查询 | NewCandidateOpensOnlyExplicitInitialNode、DefinitionRejectsMissing、PrerequisiteMustBeExplicit、DefinitionOrderAndOnlyExplicitImmediateSuccessors：P-A初始、P-B锁定、无环／明确能力／准确版本、查询不建关系、后继按定义顺序且不传递自动开放P-C。 |
| ②入场与角色 | ReadyBasisComesFromPublicM03、EntryIdentityNeverSilentlySwitches、EntryRequiresEveryField、CurrentM03PlayerContextRevisionAndSlot：M03真实公开Ready／恢复／恢复完成，原槽0也显式，低等级不锁关，输入角色不变。 |
| ③挑战复用 | ExitKeepsChallengeAndReentry、MultipleUnfinishedChallenges、ChallengeCannotMoveToAnotherLevel：退出后原Challenge新Attempt、拒绝强换／跨关复用、可保留多挑战但不并行活动尝试。 |
| ④胜利和首通 | WholeVictoryRetainsUniqueFirstClearAndOpeningSourceAcrossReplayAndNewChallenges：首通与开放一次，准确重试零新增，重打新挑战但不替换旧依据，P-A只开P-B，P-B胜利才开P-C。 |
| ⑤退出与重来 | RestartTransfersOriginalEntryBasis、RestartCannotReuseAnyOldOrCurrentAttemptIdentity、ExitKeepsChallenge：旧结束／新关系一次返回，原角色依据与Baseline保持，旧收讫重试不再建，拒绝占用／历史ID。 |
| ⑥来源与原子性 | ExactBeginRetryAfterEndAndM03Recovery、ProcessedEndCannotChange、OppositeEndAndReusedSettlement、OnlyNewOperationsRequireCurrentM05Revision：同尝试相反结束、结算／结束ID复用、错报告／Context／修订拒绝且全旧，其他关挑战不被关闭。 |
| ⑦隔离与预算 | InputAndOutputGraphsAreIsolated、NewSmallBudgetsRecheckDefinitionContextAndPublicM03Inputs、EndedAttemptRetainsLargeM03Revision、OneSharedBudgetCoversCompleteOperation、RevisionOverflowDoesNotPublishPartialEndOrFirstClear：修改壳隔离、历史M03修订重验、六入口末步不足及4位修订溢出原子拒绝、新预算一致；同版1667条通过、范围与GUID核验见下。 |

## 原基线与源码冻结

before明确继承demo-009-scope.json.after.files原377项及原UTC 2026-09-21T07:27:30.2472227Z，不冒称为D现场。
原scope SHA=a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec；原清单规范SHA=8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307。
D真实startedSnapshot为2026-09-21T08:11:28.7676538Z，377项完全相同、A尚无新增；before和startedSnapshot不回写。
CODE_READY为2026-09-21T08:30:22.4059100Z；5份C#共1110行，规范SHA=1b8cf273805460553239b042a7e7bc01e56a7ccbb6a360533acca1d2e6aed063。
冻结现场386项＝原377＋A4＋D5；原377项全部SHA保持，无其他源码／测试路径变化。A文件只核归属及SHA，D未审改其业务。
A联合冻结UTC 2026-09-21T08:36:22.6216656Z；9份C#规范SHA=00efd9b85e6128b86ddef1d22f4be74153740c3ad14ec939747373352ebca456。
验证前386项规范SHA=29ad2b4ff0649682f3252ffb829d5801c38b5f79c39e20e1bae1d9155d41a797；D逐份核9源码相同并在validationBefore保留A原抓取时间／来源。
D最终after为2026-09-21T08:42:47.4327921Z：395项规范SHA=28be3756cf902dcfe9b28bc99869c3f8f20447233000a9fe7842d4ac84900b94。
全部377旧项／原meta保持，新增A8＋D10项；9份源码与冻结一致，9个新GUID在全Assets中各出现一次，保护输入SHA保持。

| 新C# | 行数 | SHA256 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs | 134 | 5f9bb98dd191fd30672c519897d1fb3b7b262b895da1b646a3172f478e047227 |
| Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs | 161 | c1e7caabee302a8f8fb833fefce85aa9e08a11cf3532d1aabac2de6ff920f44f |
| Assets/Scripts/FightMatch/Core/CandidateProgressionRequests.cs | 44 | 14dc1da29930abb81042de5d963db0213c2a1a1d7394c1cda37ad800c1262dce |
| Assets/Scripts/FightMatch/Core/CandidateProgression.cs | 256 | 5e7863f065e3014722c8ec59647c4a1fb9982472ab632c0a843301245b395b6b |
| Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs | 515 | 3281992c87ba5110b7e26dd9ffd5ad0ca2793816a20e728254ab58289cb1d2b8 |

五份C#1110行＋五份Unity生成meta55行＝1165≤2000。meta路径为上表对应C#路径加.meta。

| meta所属C# | GUID | meta SHA256 |
| --- | --- | --- |
| CandidateProgressionDefinition.cs | 5122f868670fef348ad315bfd0a78d7d | a2530e2576845fc088e09b98cdca0728172ebcdb28ea84a4bbf90d93619ca20a |
| CandidateProgressionState.cs | 0747afc4eebc9c34ab2b02133c60e557 | 83b83db4d4fe0e6485d056e745305e4163b2352ed63c0477c6e0f6d76b4e14b4 |
| CandidateProgressionRequests.cs | 55e2a683a0fe0364cb6455d8c22850e6 | c461ac2b5dc5e8c3f79ca2568d541a47577274fb595b938dfc0cb14384e4e28d |
| CandidateProgression.cs | a08aa557aed74d14296c3fdcec953191 | 1797fecabe31515fbab6c18be08f999c9fce9b8a8f711f8c5108196181ea964f |
| CandidateProgressionTests.cs | fc576b65d9ab81e499890637c8f3176c | 997590d422c073422de1c40c1b6a88a57809609900111712e83a25a796f28ba3 |

## 验证状态及边界

静态核§99七项；五份源文件无行尾空白，Core无Unity／时间／浮点／IO、子预算、M03私有检查器、B05库存或B06阶段API引用。
A唯一执行固定Unity2022.3.18f1，进程Hidden，等待退出／Refresh后取实际ExitCode。编译与测试各一次成功，冻结后无源码修正或补跑。
编译PID48284，UTC 2026-09-21T08:37:01.2397521Z→08:37:14.3811806Z，ExitCode=0；EditMode PID11804，08:39:25.0913875Z→08:39:40.9461140Z，ExitCode=0。
scope.jointValidationEvidence保留A完整实际命令及原始CommandExecution回执；来源demo-011-scope.json最终SHA=772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859，D核三产物与395项输入一致，状态VERIFIED。
以下为A实际参数对应的等价命令展示，D未执行；测试没有-quit。

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB06Compile.log'
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemoB06-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB06Tests.log'
```

本批XML实际运行UTC 2026-09-21 08:39:32Z→08:39:39Z：1667/1667 Passed＝原1414＋011新161＋022新92，Failed／Skipped／Inconclusive均0。
D按Ordinal逐fullname比较原1414条（1409不同名称）的次数及Passed次数，完全相同；没有其他新增fixture。原XML SHA保持6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0。
D旧集合规范SHA=5d268d74d4fc4c0069ba98adf69186f11c783fd0047e9717a00db1ea65fbb63d；编码为Ordinal排序的fullname、次数、Passed次数，以TAB分隔、LF结尾、UTF8无BOM。

| A工具生成产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB06Compile.log | 7a901d0c28818c47130388a8a38356861e884f78c15468195eba28ae35425110 |
| Logs/FMDemoB06Tests.log | ed2e074e39201191ebfb333cf995e4143ca8e94460e28c3e31280548efc74f5e |
| FMDemoB06-EditMode.xml | a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e |

编译／测试无C#错误或警告；git diff --check退出0，五源码行尾空白检查通过。既有Git未提交改动不等于本包改动，范围由原377项SHA及明确新增路径核对。
只有明确隔离P-A/P-B/P-C规则，未把L1/L3样例连成自然开放链；本包不添加真实关卡边、16教学、职业／道具发放或奖励量。
M05开放／CheckEntry只证明本模块及所传M03依据，不证明M10/M14资源可用、M06已开战、真实通关或H10提交。
EndFacts是后续M07／M02组织的候选输入，012／023／024／025仍须核真实报告、结算、联合入场与正式内容依据。
没有广告／云合并、恢复导入、永久进度回退、存档或OperationId通用缓存；所有旧文件、依赖、配置、权限及Git未写入。
交回后保持源码、meta与验证产物冻结；仅待R按§100在双方原始turn结束后独立审查，无新增用户决策。
