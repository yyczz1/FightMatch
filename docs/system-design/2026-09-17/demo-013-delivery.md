# FM-DEMO-013 交付记录

状态：IMPLEMENTED／联合验证通过，交R独立审查；本文不作ACCEPT结论。
B任务：01a0c3c3-e163-7f01-b639-ca1f16e1a99d；本次013原实施turn：01a0c3c3-e3da-7c01-aa5c-53c31c6ba000。
授权：system-task-packets.md r74 §114／116；DEMO-B09联合验证沿§117，独立审查沿§118。
本次明确委派覆盖旧人工worker流程；原目录local、gpt-6-astra／max，无分支／worktree／提交／推送／新任务／子代理。
B只新增三份Core、一份测试及四份Unity meta；D的五份meta只由B调用Unity生成，不读D新实现作为013依赖。
全部旧源码、测试、meta、配置、依赖及三协调稿不修改。

纯入口集中CandidateHistoryOperations：CreateCandidate、Append、Locate、ReadRange、PrepareRollback、FindOperation。
Create重验012原入场；只接修订1、零记录／无报告且Current与Initial完整相同的原对象图，不导入中途快照。
Append核同一Binding／Baseline／Initial、原有效前缀逐记录保留、恰一新完整Record、原当前Before和准确After。
保留009／010／011原片段；012内部AssembleRecord仅从已保留片段核对完整后态／事实／贡献，不重跑敌方或抽随机。
固定模式Check／Equal只用于已有类型的数值复验和逐字段比较；另核原基线／定义引用及历史关联，不以hash代替来源。
Archive永久保留本Attempt所有已接纳战斗记录；EffectiveAnchors只引用CurrentRun.Records的当前有效顺序。
端点按当前面结构Pair找最近实际Attack；固定线须有原致死事实、RouteLocked事实及同一路线。
ReadRange允许明确有效Anchor跨面查询；返回原前态、完整后继列表、原面／配对及原操作。
确认再次核玩家／尝试、原绑定、当前修订、目标及完整范围；不会自动扩展旧预览或接收客户端任意快照。
回退共享目标Before的完整业务值，SceneRevision单独取当前+1，保留原Initial与目标之前的有效记录，FinalReport=null。
完整业务值包括面／阶段、线路／Pending、敌我HP／原槽／意图游标、行动／敌阶段计数、贡献、随机流及PRD。
回退只在新Archive中给本次撤去项加准确SupersededBy；更早撤去关系保持，旧History和原记录不变。
FindOperation区分Effective、Superseded、RollbackRecorded，未知返回NotFound；操作ID和Anchor都终生占用。
审计校验按原修订合并战斗记录和回退记录，核每次准确前态／范围／恢复前缀与最终有效头，不重放战斗。
AwaitRescue可回退；WonPendingSettlement／已有报告拒绝新增和回退，末条终局操作可正常Append。
原条件继续留在旧记录；后续012行动接受当前显式偏好，不读写成长、库存或当前永久偏好。
所有候选CommitEligible=false，无014重演器、015保存、奖励、复活／广告或UI实现。
输出集合只读且复制输入集合；共享已知不可变对象。失败与Limit均无半份Next或Superseded。
同一Math复验当前、Archive及RollbackRecords所有保留数值；不创建更大预算，不裁剪历史，不承诺总内存／字节／耗时上限。

| 七项验收 | CandidateHistoryOperationsTests已执行见证（全部Passed） |
| --- | --- |
| ①真实来源及追加 | RealPublicSourceOperationsAppendAndRetainTheTerminalStep；CreationCannotImportMiddleStateOrAnotherOriginalGraph；AppendRejectsMissingSkippedOrChangedPrefixesAndForeignBindings；AppendRequiresTheCompleteOriginalFragmentsAndAfterState；SyntheticLinkWithoutItsOriginalDeathHistoryCannotEnterThePublicHistory。 |
| ②定位与确认 | L3EndpointChoosesTheLatestActualAttackAndLockedRouteChoosesTheKillingAttack；LocatorUsesExplicitKindAndCurrentStructuralPairIdentity；ConfirmationMustMatchTheOriginalRevisionTargetAndEverySuccessor；PreviewCannotSilentlyGrowAfterAnotherRealOperation。 |
| ③完整回退 | RollingBackBothL3StepsRestoresEveryBusinessValueAndTheOriginalRandomState：完整业务值相同、仅新全局修订和新操作身份不同，原随机状态与字轨迹保持。 |
| ④跨面与救援 | CrossFaceRangesRestoreTheOldFaceAndPreserveRevisionGaps；RescueCanUndoTheLethalStepWithoutAdvancingUnexecutedIntentCursors；VictoryCannotBeUndoneOrReceiveMoreHistory。 |
| ⑤分支和旧回调 | BranchesKeepEachFirstSupersedingRollbackAndReportsContainOnlyTheNewEffectivePath；AcceptedAndSupersededAnchorsAndOperationIdsAreNeverReusable；RollbackIdsRemainOccupiedAfterMoreActionsAndQueriesDoNotActAsRetryCaches；IncompleteOrMisattributedAuditArchivesCannotBeReadAsValidHistory。 |
| ⑥条件与只读 | CurrentExplicitPreferenceCanChangeWithoutRewritingOldConditionsOrPermanentInput；IdentitiesRemainOrdinalAndDoNotMergeSeparatorOrCaseVariants；ContextFieldsAreExplicitAndZeroIsNotTreatedAsMissing；MissingOperationAndAnchorInputsDoNotAllocateHistory；NullRootsThrowWithoutChangingTheOriginalAggregate。 |
| ⑦预算与回归 | SupersededLargeOriginalNumbersAreRecheckedWithTheCallersNewBudget；AHighCurrentRevisionIsNumericallyCheckedBeforeAnyHistoryResultEscapes；SharedBudgetLimitsNeverPublishPartialCandidatesAndFreshRetriesAgree；UnknownHistoryRecordCapabilitiesProduceStructuredRejections。 |

L1／L3由006明确坐标来源、007A／008及012公开入口形成真实行动；C=1/1000、seed42／sequence54为合成条件见证，不称生产平衡保证。
单Warrior支持域自然没有AOE／Pending链；Link只核合成缺死亡历史无法导入，不冒称公开自然补线已出现。
2^100当前修订用内部无效审计探针验证预算守门，随后大预算仍拒绝；不声称已执行2^100步或开放任意恢复入口。

before继承demo-012-scope.json.after.files原415项，原UTC=2026-09-21T11:10:55.8788837Z。
原scope SHA=f14c371e5468e99451327e9bb573107e1b52e4334c767d8d2859a98a52cb2ff7。
原415规范SHA=3efae32d1ec770ef3b1ae2410330ad579225d78181f8aa263251e4deae3ef4b2。
startedSnapshot UTC=2026-09-21T11:43:46.9631845Z，415旧项逐SHA一致，自有路径不存在；未把D文件混入before。
派发稿SHA=79582117b50b78b0ac4b5bb3abf6ef45ed098985085ecc7b8c2ca00945988ed8；开工实际稿SHA=b9b47258b74975cc19060cf0d1eec92b5bf8404e701261f0b35582bb04a7c4b4。
四C#于2026-09-21T12:01:03.2877531Z冻结，规范SHA=9add1310653e942abb4ee82f7a7459c535cc18348c46ed00508f78dc4685c7c5。
源码1206行，四份Unity meta实际44行，共1250≤2100；四源码首次冻结后未修改。
规范为Ordinal相对路径＋TAB＋小写SHA＋LF（含末尾LF），UTF-8无BOM。
原B08 XML SHA=a384933b9f9f5b9c3c12017ec0cd8e1b64c6d4115cae44f06c438528176167fd；1911条Ordinal fullname完整多重集合及逐名Passed次数均保持。
git -c core.safecrlf=false diff --check已运行，退出0；旧tracked摘要仍34 files changed, 2809 insertions(+), 597 deletions(-)。
既有FlowPuzzle／Packages／用户本地权限差异不属本包，无Git写入；本包撤回仅八个新实施文件及本报告／scope。

D本次023原turn=01a0c3c4-b9ad-7b22-9b89-880f818d88d8，于2026-09-21T12:07:49.9657056Z明确CODE_READY。
D五源码规范SHA=26bd0e46cb083b30990c6b524e88b5fa93b16d9e78256e52fa513dc270b52175；逐SHA与其消息／scope一致。
九C#共同冻结规范SHA=54e16d3ded47377724c43262b4cbbc9539f9e7562d5b86576475e4bb0143bd01。
编译前UTC=2026-09-21T12:10:00.5366267Z，424项=415原项＋九源码，规范SHA=5b65a5578af865d11d39e25f91628d0ddb80e18bc414db0f5dc0bf19e176e153。
测试前／后均433项=415原项＋013八项＋023十项，规范SHA=2f695856ae72d8d7a8582d6b51e3480ded3913dbd006970c052180bf8c2c851e。
验证后UTC=2026-09-21T12:15:10.9719792Z；415旧项、九冻结源码和19个保护输入全部保持。
全部Assets共有241个GUID，无重复；九个新GUID唯一，原232个GUID路径均保持。
D十项1420行≤2400；其实际五源码／五meta及SHA／GUID／行数在scope.after.newFiles，明确owner=023。

| 本包新实施路径 | 行数 | SHA256 | GUID |
| --- | ---: | --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateBattleHistory.cs | 93 | deb90d0700b38f0e669a77e4f76b19dc82bc799584c94aa1c466b2c488132d37 | — |
| Assets/Scripts/FightMatch/Core/CandidateBattleHistory.cs.meta | 11 | 84ec0646c4f78637961ceaa88c40f3841d165d2fe9e93303c9e4ce565cbbf5da | 4c5fdbabf16b6d645af2922ee405813e |
| Assets/Scripts/FightMatch/Core/CandidateHistoryRequests.cs | 61 | e155d2a672e4733d968819f8e6bdc22fc259d37781184f7aa70e94b7184014a8 | — |
| Assets/Scripts/FightMatch/Core/CandidateHistoryRequests.cs.meta | 11 | beef70fe5165cdd8a4606a42c061610c5c4ce8f75ec3632175062ba3aa87683b | a45799ebb8a05474b9ba39c6986dd5b3 |
| Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs | 443 | 8b51d810ab8a01fdd620671c4c53c4f9887d63df196d34b7760167ba8dd34e28 | — |
| Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs.meta | 11 | 59c6177e252dc08aeebe54111abb0b88046b99b8f64e79507e418b259f89476b | 37fba5be8e8a939418ae1aa8e7bb4e56 |
| Assets/Tests/EditMode/FightMatch/CandidateHistoryOperationsTests.cs | 609 | 8fdeec0edcc8c0808af79b3aeb6773f934c66e73bbc303c1d777538fd3285906 | — |
| Assets/Tests/EditMode/FightMatch/CandidateHistoryOperationsTests.cs.meta | 11 | 232eeb4dc9b6ce12e7fc42274b6aaa5093af5fa99ed99174d041387f93c8d921 | 2d150af1620a14a49bcbe0186d3d8681 |

B唯一运行Unity，均先核无Unity进程；没有杀进程。一次Get-CimInstance进程读取被系统拒绝，改用Get-Process后排他检查成功。
沿前批已验证的许可证IPC约束，经自动审批在沙箱外执行；本批编译和EditMode各一次实际exit0，无失败重试、无源码修正／重冻。
Start-Process使用Hidden／PassThru／Wait，并由真实Process对象取得子进程ExitCode，不以外层shell的即时LASTEXITCODE代替。
Start-Process管道只返回Process对象，启动stdout／stderr均空；Unity应用完整输出保留在各自-logFile。
scope.rawValidationEvidence含实际完整命令、原工具初始／轮询／完成回执及CommandExecution；validationRuns含命令参数、PID／UTC／真实退出及输出边界。

| 运行（2026-09-21 UTC） | PID | 起止 | 子进程退出 | 原始CommandExecution |
| --- | ---: | --- | ---: | --- |
| 编译／导入 | 14956 | 12:10:45.6804088→12:10:57.7651525 | 0 | exec-4a77cfe3-a5a8-409b-aca8-eef1ec278e31 |
| 全量EditMode | 2080 | 12:12:18.1479171→12:12:43.7451411 | 0 | exec-e8eb166a-6f66-4269-9994-5f9210b5ed48 |

```powershell
$exe = 'D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe'
$compile = Start-Process -FilePath $exe -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:/Unity/UnityProj/FightMatch','-logFile','D:/Unity/UnityProj/FightMatch/Logs/FMDemoB09Compile.log') -WindowStyle Hidden -PassThru -Wait
$compile.WaitForExit(); $compile.ExitCode
$tests = Start-Process -FilePath $exe -ArgumentList @('-batchmode','-nographics','-projectPath','D:/Unity/UnityProj/FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:/Unity/UnityProj/FightMatch/FMDemoB09-EditMode.xml','-logFile','D:/Unity/UnityProj/FightMatch/Logs/FMDemoB09Tests.log') -WindowStyle Hidden -PassThru -Wait
$tests.WaitForExit(); $tests.ExitCode
```

两日志C#错误／警告均0；未将许可客户端等非C#日志提示误称为零错误文本。验证后Unity进程0。
XML执行UTC=12:12:25Z→12:12:41Z，2145/2145 Passed，failed／skipped／inconclusive均0。
原1911条／1906个不同fullname按StringComparer.Ordinal逐名比较；名称重数及逐名Passed次数差异0，保留重复名称。
原集合与本次旧子集fullname＋TAB＋count＋LF规范SHA=59b99d445f2468f4591bd6411039fe79af395cf03b3bf68b9a7feac07748420c。
fullname＋TAB＋count＋TAB＋passed＋LF规范SHA=fdb956ca0c72b564b828efc82eab4cff29d9bc1cf0f43f249d190bbb844529ff。
新增恰013的26方法／108条及023的19方法／126条，无其他新增；完整逐名／方法／类映射见scope.xmlSummary.newCases及两个Methods数组。
全部原名字和参数的原始来源仍为保留的B08 XML，不以Distinct集合或总数增长替代逐名验证。

| 原始验证产物 | 字节 | SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB09Compile.log | 64924 | d3bef0bfa530e7e06a6c6b6f67e077720fa2fa4def5c57e63a38972df3960667 |
| Logs/FMDemoB09Tests.log | 89029 | cc6728c0cde79d9be2750214d69eced92301738d0c3b31b765a7fd024e9688f9 |
| FMDemoB09-EditMode.xml | 1402504 | d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22 |

scope完整保留继承before、真实startedSnapshot、自有／九份共同冻结、validationBefore／After、完整after.files及原始进程／产物证据。
scope最终261367字节≤320KiB，SHA=d110349421486511971eb1a6c49b6adce38b9685f62e03db097643d4c9a136fb。
共同证据已交D与R；D已回报独立核同版126新例、旧1911集合、五源码冻结和其五meta／GUID／行数一致。
仅纯Core及EditMode验证，未运行PlayMode、Player构建、014／015／024或生产提交。
本次正式报告／scope交R及SD00后即结束原013 turn并停改；R待两原实施turn均completed再独立审查，不等待R先ACCEPT。
