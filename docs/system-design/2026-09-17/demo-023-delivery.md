# FM-DEMO-023 正常封闭报告到固定基础奖励

状态：WORKER_RETURNED；B09联合编译／EditMode通过，D已核本包用例、范围、产物与B原始执行回执，交回R独立审查。本文不作独立审查结论。
D任务：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；本次原turn：01a0c3c4-b9ad-7b22-9b89-880f818d88d8。
授权：system-task-packets.md r74 §115～117；013属B，独立审查属R。未创建任务／子代理、分支、worktree或Git写入。

## 实现与职责

只新增四Core、一EditMode及本报告／scope；五份meta由B通过Unity生成，D不启动Unity、不手写meta。
CandidateBaseRewards集中PrepareDefinition、CreateCandidate、FindByAttempt、FixNormal四个纯入口；所有候选CommitEligible=false。
奖励定义复制完整Context、Level／奖励版本、整数E、精确权重／曲线／参考除数／等级修正、明确CurveIntercept、空额外能力及FixedOrdinaryMaterials。
普通材料ItemId／非负整数Amount向量明确且唯一；数量0、显式空表保留来源，不推断随机掉落、首通额外物、卡证或职业。
State从修订1及空结果建立；按形成次序保留原报告、指纹、定义、M05结束依据、经验明细、材料及随机依据。
先按Attempt／Settlement查原结果，完整同意图返回当前State／原BaseReward；旧预期修订不重评分，改变来源或身份冲突整体拒绝。
新办理才核ExpectedStateRevision；同次NormalVictory结束精确核Player／Attempt／Challenge／Baseline／Settlement／Context／Level／参与者／指纹及NewAttemptId=null。
报告核原Binding／Baseline／Initial对象图、Empty／单Warrior支持域、全部面HP、有效操作链、原终局操作／时间、WPS及完整固定线。
每条贡献一一对应原直接／敌方事实、原FactIndex、Actor／Target／Beneficiary；HP前后、损失与溢出守恒，片段后态、阶段事实及累计一致。
报告重新计算FMBR01指纹，但指纹正确不代替来源与覆盖检查；没有调用009／010／011／012求值重跑旧战斗或抽取旧随机机会。
计算C=Dealt×DamageWeight＋Taken×TakenWeight，J=全关原初始敌HP／除数；G使用原入场等级、原推荐等级和明确宽限。
G通过精确平方乘法求幂，指数0明确为1；同一scope.Budget.Math贯穿来源和005 Evaluate，没有先floor贡献或浮点评分。
Amount直接取005 ExactScoreResult；完整上下界及TermsUsed随经验明细保留，无第二本可改评分账。
固定材料保留原BaseReward域、SourceCapabilityId及Mapping，NotUsedFixedTable、前后态同值、Words为空且WordsConsumed=0；Battle／Bonus保持。
原State中已固定数值在新预算下重验；重复请求不使用log工作区，新的评分共用原scope并保留005输出预留。

## 七项验收映射

下表行为见证已通过本批同版EditMode；CandidateBaseRewardsTests共126/126 Passed，scope.independentVerification记录19组方法的展开数与通过数。

| 验收 | CandidateBaseRewardsTests见证 |
| --- | --- |
| ①真实报告 | PublicWholeLevelAndM05EndingProduceExactOriginalEntryReward；CorrectlyRehashedIncompleteOrAlteredReportIsStillRejected；FragmentFactsCannotLoseContributionsOwnershipOrHpConservation：012公开整关→022公开NormalVictory，拒绝漏首／末步、空记录、重操作／贡献、换受益者／索引、错HP／时间／原绑定及伪覆盖。 |
| ②精确经验 | PublicWholeLevelAndM05EndingProduceExactOriginalEntryReward：L1 W1的C125/4、J15、E20、G1→26；原W4的G9/16→14；L3 W1的C75/2、J35/2、E24→31，同时核005上下界floor相同。 |
| ③原入场与接收 | G01CurrentGrowthAndInventoryConsumeTheSameFixed14WithoutChangingTheScore：020公开应用原14使W4＋157→W5＋6，当前成长变更仍应用原14；021公开接同Settlement／Context锡片2；022原结束已备，未宣称H10／到账。 |
| ④固定与冲突 | DuplicateReturnsOriginalFromTheLaterStateAndReplayHasItsOwnOrdinaryReward；FixedIdentityReportDefinitionAndEndingCannotBeReplaced；EveryFixRequestFieldIsRequired；NewRequestMustMatchTheReportAndCurrentRewardState：当前更晚State返原结果，首通后重打独立普通奖励，换Settlement／报告／定义／结束冲突。 |
| ⑤参数与覆盖 | DefinitionNeverDefaultsMissingInvalidOrUnsupportedParameters；ExplicitZeroAndExactPowerPoliciesKeepCompleteSource；PenaltyZeroToTheZeroPowerIsOneAndOtherExactPowersRemainExact；WholeLevelReferenceIncludesEveryFaceAndFirstFaceCannotSettle：显式参数／能力、0量／空表、精确幂及多面全关基数，不因E0或权重0跳过来源。 |
| ⑥随机与只读 | FixedTableUsesZeroWordsAndRetainsBothOtherRandomDomains：BaseReward零字、两域保持、修改定义壳／列表无影响，递归检查输出深只读。 |
| ⑦预算与原子 | SharedLimitsFailWithoutPublishingAnyFixedResult；CompleteOperationSharesTheCallersMathThroughTheFinalScoringStep；NewSmallBudgetsRecheckLargeNumbersRetainedInsideTheOldFixedResult；OneEvaluationScopeAccumulatesOutputReservationsAndLogTermsAcrossRewards：Math／LogTerms／工作区真实限额、末评分不足无Next、旧固定大数重验、同scope累计与新预算重试。 |

## 原基线、冻结与范围

before原样继承demo-012-scope.json.after.files的415项及原UTC 2026-09-21T11:10:55.8788837Z，不冒称D现场。
原scope SHA=f14c371e5468e99451327e9bb573107e1b52e4334c767d8d2859a98a52cb2ff7；原415项规范SHA=3efae32d1ec770ef3b1ae2410330ad579225d78181f8aa263251e4deae3ef4b2。
D真实startedSnapshot UTC=2026-09-21T11:43:06.5300398Z，415项完全相同，B尚未新增；before／startedSnapshot不回写。
原B08 XML SHA=a384933b9f9f5b9c3c12017ec0cd8e1b64c6d4115cae44f06c438528176167fd，1911/1911 Passed，1906个不同fullname。
原用例规范SHA=fdb956ca0c72b564b828efc82eab4cff29d9bc1cf0f43f249d190bbb844529ff，Ordinal fullname＋TAB＋次数＋TAB＋Passed次数＋LF，UTF8无BOM。
原全Assets的232个meta路径／GUID全部保持；B09最终433项，9个新增GUID在全部241个meta中各出现一次。
CODE_READY UTC=2026-09-21T12:07:49.9657056Z，五C#规范SHA=26bd0e46cb083b30990c6b524e88b5fa93b16d9e78256e52fa513dc270b52175。
冻结现场424项＝原415＋B4＋D5；规范SHA=5b65a5578af865d11d39e25f91628d0ddb80e18bc414db0f5dc0bf19e176e153，原415及保护输入全部保持。

| 新C# | 行数 | SHA256 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/CandidateRewardDefinition.cs | 120 | a5110766ddc5524b54aad1b9d624b567765f472e695aac6d1cbfd7aaa493d0be |
| Assets/Scripts/FightMatch/Core/CandidateRewardState.cs | 88 | 4a29c7dec46ebbc70ba00dfe264568f205a2ef203f5a5686b42557d9e3207458 |
| Assets/Scripts/FightMatch/Core/CandidateRewardRequests.cs | 16 | eeca6346aaf08a48b90a0d7a6204e2bafdf7e8fed5c136dd0ce48a033a80dcc3 |
| Assets/Scripts/FightMatch/Core/CandidateBaseRewards.cs | 557 | 55b967584af59a83c78be32bc615e799b0f224b8e60bb4ef4a904a9b941c63a5 |
| Assets/Tests/EditMode/FightMatch/CandidateBaseRewardsTests.cs | 584 | d7051402f9b0c0006879677593001cc2b52ee865265682cdc4983a4cd902aeaf |

五C#共1365行＋五份Unity生成meta55行＝1420≤2400；五源码验证前后SHA完全相同，没有源码修正或补跑。
Core静态检查无新Math／Evaluation预算、旧战斗求值调用、Unity／系统时间／IO／浮点、M03／M04写入口或本批013 API依赖；五源码无行尾空白。

| meta所属C# | GUID | SHA256 |
| --- | --- | --- |
| CandidateRewardDefinition.cs | db095a6ffca90944fb34afea4a0224cc | f610a13dcc76023c1b256759b51d069f63655110ce039ec08a2fdb5b3877467e |
| CandidateRewardState.cs | 64890618de22b4248846867b0efce8ae | 3008f71b010af2cd239cae54240906f3c3fafb6f76863847135a69eec1b78959 |
| CandidateRewardRequests.cs | 83cd8f1deed9ce14a8de789ac7a81dba | 73b31db2a898fff278da9658e779f7931a259705e620a3d5793cafb7558b23d4 |
| CandidateBaseRewards.cs | e976fd1101c51394cbdaaf11d0c78107 | 127b3d75679058e787feb866f53a071afc301fd1794f1e7a9012bd1ea45e89cf |
| CandidateBaseRewardsTests.cs | 21b8939ee6b10f7469ccfa013cd76357 | e4fb76be6790aa12a0112f011aab486e6144f4348f40c07e7848314faf1ba362 |

meta路径为对应C#路径加.meta；D只读核验，未编辑。
B共同冻结UTC=2026-09-21T12:10:00.5366267Z，九份C#规范SHA=54e16d3ded47377724c43262b4cbbc9539f9e7562d5b86576475e4bb0143bd01；导入前424项SHA同上。
D最终after UTC=2026-09-21T12:15:18.3781717Z，433项规范SHA=2f695856ae72d8d7a8582d6b51e3480ded3913dbd006970c052180bf8c2c851e。
415旧项全部保持，新增B8＋D10；无未经授权的源码／测试路径及保护输入变化。

## 来源、验证状态及边界

config.json SHA=4e815411914bf86fe9e1a046ee80b86c24d048bd8455a1d4bb55a088255aa7f3；stages.csv SHA=814536d3b9de969f0f85673985e9b188857166c83967bf67af74eaa6e3a9db2d。
参数沿§115及9月19日明确试调交接；材料使用candidate:tin／candidate:wood隔离映射，未把显示名当正式发布身份，未抄旧progression.csv经验结果。
真实报告测试从006固定来源几何、020公开成长属性、007／008准备及012公开求值产生；C=1/1000是明确无暴击条件见证，不宣称生产随机保证。
RUN by B — 编译PID14956，UTC 2026-09-21T12:10:45.6804088Z→12:10:57.7651525Z，实际ExitCode=0。
RUN by B — EditMode PID2080，UTC 2026-09-21T12:12:18.1479171Z→12:12:43.7451411Z，实际ExitCode=0。
两次各执行一次、进程排他、Hidden／PassThru／Wait／WaitForExit，无失败或补跑；以下为实际参数的等价命令展示，D未执行。

~~~powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB09Compile.log'
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemoB09-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB09Tests.log'
~~~

scope.jointValidationEvidence保留B两条完整实际PowerShell命令、工具初始／完成输出、真实PID／ExitCode及原CommandExecution回执，状态VERIFIED。
来源demo-013-scope.json最终SHA=d110349421486511971eb1a6c49b6adce38b9685f62e03db097643d4c9a136fb；原执行标记为exec-4a77cfe3-a5a8-409b-aca8-eef1ec278e31及exec-e8eb166a-6f66-4269-9994-5f9210b5ed48。
XML运行UTC=2026-09-21 12:12:25Z→12:12:41Z，2145/2145 Passed＝原1911＋013新108＋023新126；Failed／Skipped／Inconclusive均0，无其他新增用例。
D逐Ordinal fullname核原1911条／1906不同名称的完整次数及Passed次数完全相同，规范SHA保持上述fdb956ca…529ff。

| B工具生成产物 | 字节 | SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB09Compile.log | 64924 | d3bef0bfa530e7e06a6c6b6f67e077720fa2fa4def5c57e63a38972df3960667 |
| Logs/FMDemoB09Tests.log | 89029 | cc6728c0cde79d9be2750214d69eced92301738d0c3b31b765a7fd024e9688f9 |
| FMDemoB09-EditMode.xml | 1402504 | d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22 |

编译／测试无C#错误或警告；git diff --check实际退出0，既有LF／CRLF提示未作修正。本包范围以415原项SHA和明确新增路径核验，不把既有未提交改动当作本包改动。
本包不提交M03／M04／M05状态，不持久化、不发H08或远端奖励、不替024／025完成联合结束或正式内容发布。
交回后保持源码、meta与验证产物冻结；仅待R按§118在双方原实施turn结束后独立审查，无新增用户决策。
