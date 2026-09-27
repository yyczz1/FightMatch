# FM-DEMO-010 交付记录

状态：IMPLEMENTED／验证通过，交回独立审查；本文不作ACCEPT结论。
任务A：01a0c1fb-248d-7321-8b69-9efd26b817f1；本次010原实施turn：01a0c342-4fb8-7873-aec5-7af5d71bf4d0。
授权：system-task-packets.md r72 §104／105；R按§106独立审查，SD00负责系统设计；沿gpt-6-astra／max、local、原目录。
本包仅新增CandidateEnemyPhase、CandidateEnemyPhaseFrame、CandidateEnemyIntentFact、CandidateEnemyPhaseTests及四份Unity meta。
八份实施文件共1183行≤1800；本文与demo-010-scope.json另计。所有395旧源码／测试／meta保持，未改BattleSnapshot、CandidateCombatFrame或BattleDamageFact。

纯入口CandidateEnemyPhase.Evaluate只接009的CandidateCombatFrame与调用方ExactMathBudget，返回不可变敌方片段或结构化拒绝。
保留DirectAttack／Binding／BeforeSnapshot／Action／原直接伤害事实和原Random，完整返回成员HP、敌HP／游标、累计贡献与本段意图。
EnemyPhaseOrdinal为原EnemyPhasesCompleted+1，仅标明归属；原SceneRevision、行动／阶段计数、Board、Face和Phase均不推进。
本包只消费已接收单Warrior、空主动／物品／盾／异常、零闪避及E01／E02候选绑定，不以显示名硬编码伤害系数。
按StableOrder递增处理活敌，按每个实例Cursor的有界余数取原IntentCycle；原BigInteger游标保留，实际Charge／Strike才各+1。
N010：每个活敌处理前检查存活成员，全倒立即停止所有后续意图，包括Charge；未执行者HP／Cursor不变，无意图或攻击事实。
致死末击事实完整保留，本轮照常有阶段归属；直接攻击后全敌死也返回空本段。复活瞬间不补打，本包不提供复活入口。
Strike按当前存活成员OriginalSlot选FirstLiving；Raw→护甲修正→最后floor一次，再按真实HP封顶。
贡献只增加真实HpLoss，原Dealt／Taken和009直接贡献保持，Overflow不计，不提前乘承伤评分1/4。
Charge的Damage为null；真实0伤害Strike仍有完整伤害事实。敌伤害不伪造玩家Crit或复用BattleDamageFact的敌目标语义。
所有保留基线／前态／直接片段的整数和有理分子分母用同一预算重验；拒绝／Limit无部分Frame，输入不变，新预算可重放。
本包没有PostSnapshot／FinalAttemptReport／Completed；012仍须组合009／010／011并一次封装完整行动、计数、历史和报告。

| 七项验收 | 已执行见证（CandidateEnemyPhaseTests） |
| --- | --- |
| ①E01／E02 | SourceL1AndL3UseReal009AndRetainConditionalSourceFacts（4）：公开准备及009真实抽样，L3 E02 20→4后蓄力、E01击5、W100→95；HeavyReadsBoundCoefficientAndFloorsOnlyAfterDefense（3）核10×13/10对防10为11，并以分数Raw及自定义系数反证提前floor／显示名硬编码。 |
| ②顺序／原游标 | StableOrderOverridesOriginalSlotAndCollectionOrderWithIndependentLargeCursors（2）：原槽、列表与StableOrder不一致，反序输入仍按10/20/30行动；不同E02用2^90与2^90+1，仅原游标+1，不受全局行动奇偶覆盖。 |
| ③死亡／空阶段 | DirectlyKilledAndPreviouslyDeadEnemiesNeitherActNorAdvanceAndEmptyPhaseStillHasOrdinal（2）、N010LastMemberDeathStopsEveryLaterIntentAndRetainsTheLethalFact（3）：死敌跳过、空阶段有归属；剩1/3HP的致死完整保留，后续0／1／2游标的Charge／Strike及后排攻击均停止。另一个显式合成恢复后输入从保留意图开始，没有调用或实现复活。 |
| ④精确量／贡献 | ActualHpLossIsUnweightedAnd009ContributionIsNotCountedTwice（3）、ZeroDamageStrikeRemainsDistinctFromChargeAndDoesNotChangeContribution（2）：有理HP封顶、多击途中致死、溢出分离，原5/3 Dealt＋009的2只算一次，旧Taken保留；Charge无伤害，floor到0仍是真实Strike。 |
| ⑤来源／支持域 | SourceBindingsAreExactAndCannotBeSilentlySubstituted（14）、CompleteHpCursorAndContributionCoverageCannotBeForged（22）、InternalNegativeProbesCannotExtendTheAcceptedSupportedDomain（13）及缺项／计数／阶段组：错绑定、身份、修订、ActionOrdinal、当前面、HP、游标、累计及能力拒绝。CurrentFaceUsesItsOwnEnemyInstanceEvenWhenOtherFacesReuseDisplayIdentities（1）与ADeadEnemyCannotReappearInTheDirectAttackProjection（1）核跨面实例和不复活。 |
| ⑥随机／组合 | Existing009RandomEvidenceMustRemainBoundAndIsNeverResampled（6）、RepeatedEvaluationIsIdenticalAndAllOutputsRetainAnImmutable009Source（4）：正常、全蓄力、全死、全倒停止都共享原Random且深图不变；同输入完全重放。无011调用或线路／完整行动／奖励推进。 |
| ⑦隔离／预算 | 上述只读组及SmallBudgetsRecheckEveryRetainedCurrentLargeValue（8）、SmallBudgetsRecheckTheFullRetainedBaselineIncludingUnusedFaces（12）、OneBudgetCoversValidationAndLateExecutionWithoutPublishingPartialResults（4）、ArithmeticOverflowCannotPublishEarlierLocalEnemyResults（3）：覆盖旧大数／未用面、0／末步／共享预算、阶段／后续游标／伤害溢出；早先局部计算不泄露Frame。 |

全部23方法／135用例Passed，完整方法计数见scope.validation.xmlSummary.new010Methods；表内同一用例可覆盖多项，不重复计总数。
成功见证均经009公开Evaluate生成输入；已有internal构造只用于明确合成前态和被拒绝损坏数据，未扩大friend或修改旧测试。
L1／L3固定几何方向及合成C=1/4均显式记入来源；真实固定首字产生未暴击，不将条件样例称作生产随机／自然进度证明。
旧chapter／boss模型仅作来源阅读；N010已确认规则和实例Cursor优先，未运行或改写校准脚本。
无MonoBehaviour／Editor UI变更；本包验证为纯Core与EditMode，不涉及PlayMode／Player发布。

before继承已接收demo-011-scope.json.after.files的395项，原UTC 2026-09-21T08:43:35.5982311Z。
原scope SHA=772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859。
原清单规范SHA=28be3756cf902dcfe9b28bc99869c3f8f20447233000a9fe7842d4ac84900b94。
真实startedSnapshot UTC 2026-09-21T09:20:43.2908113Z：逐项核395旧SHA相同，自身10新路径均不存在。
初始scope 62704字节、SHA=0a1e3938803ab047841bdbca383af76ed1b28df13aa7bfd41c9b8db461b1cce8；before／startedSnapshot原字节未回写，后续证据只追加。
任务稿派发SHA=c834315d9d2a0e610c68a7b5c687678c886200e7873e47df3ce3678d2e216992；开工所读版本已含获准的派发状态／turn登记，实际SHA列于下表。
所读§104／105原文字节SHA=e7bbced76808d13fedc410ff28778509b4dd7528b2bf236a9f7ed4de812fd790，实现与验收依据在本轮保持。
四份源码CODE_READY UTC 2026-09-21T09:42:19.7785001Z，规范SHA=a15f8947d4a2357fcbe0d07eb554b8279ab85c0cd7659a4f4bb72088e9428902。
编译前399项＝395旧＋4 C#，规范SHA=f48c1ddcb9e794d276aeeb7335e66beab78e45a30ccbc3bcff6357348c07fef7。
测试前UTC 2026-09-21T09:45:28.9309917Z、测试后UTC 2026-09-21T09:49:21.5765037Z均403项，规范SHA=f20904bef62b1f3378837fd555a98eae655e04c72ac1c8449a90d234f7b690b5。
最终403＝395旧＋8新；395旧项逐SHA相同、四份冻结源码验证前后完全一致、无其他源码／测试路径变化。
规范均为Ordinal相对路径＋TAB＋小写SHA＋LF（含末尾LF），UTF-8无BOM；完整路径见scope.before／validationBefore／preTestSnapshot／after。
四份meta均由Unity生成，4GUID在全Assets226份meta中各只出现一次。
原222份Assets GUID的路径＋TAB＋GUID＋LF规范SHA始终为5f01dd740529bbbcdab94454aed412fd97500d4721589b886b97f5383c76ed7f。

| 本包新实施路径 | 行数 | SHA256 | 新GUID |
| --- | ---: | --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateEnemyPhase.cs | 394 | a3249bec9874af9c5222d1048ddc58b29972c902030bc0bbd01cae35635b21c7 | — |
| Assets/Scripts/FightMatch/Core/CandidateEnemyPhase.cs.meta | 11 | 6b5620ead8a83f54702bc68ce26bbe09ea2315fb6015c93b92642a5299059820 | de49c4a4bc4783c4fa2c85699d50c4e1 |
| Assets/Scripts/FightMatch/Core/CandidateEnemyPhaseFrame.cs | 44 | 16e14da19d68bf5f29f8a5679024b5a013feb9a94829753c4a7544e7163a8f2a | — |
| Assets/Scripts/FightMatch/Core/CandidateEnemyPhaseFrame.cs.meta | 11 | 487d07b33f0a350ce1a86411116b313f59a532f7f5a4eacb843cf0a16f4b52f3 | e636bac120acb1e4e8c556c6feae46e1 |
| Assets/Scripts/FightMatch/Core/CandidateEnemyIntentFact.cs | 68 | b3358476259cb638c96da4dc0283d5e600c07391b15c6a97c5c90b5da660eabf | — |
| Assets/Scripts/FightMatch/Core/CandidateEnemyIntentFact.cs.meta | 11 | 8bf738d9e75ab6de556a769f112fb86bf6b24384f5854b8402bf243fe9fe2e1e | 859446c8565aa0e4b8e64001c8634eb8 |
| Assets/Tests/EditMode/FightMatch/CandidateEnemyPhaseTests.cs | 633 | fd0f72ef68038b29fc881939fb7c32cfc8ee02ffc2d3f75535330a7662659ce7 | — |
| Assets/Tests/EditMode/FightMatch/CandidateEnemyPhaseTests.cs.meta | 11 | f16673016781f84be7e1a0a1b3a01145c0e2a820b05b64208ccb831cbb26f06e | 3577275642aa8db4e87b41ae2b65f4d9 |

A为本批唯一Unity执行者；两次启动前固定路径检查与Unity进程排他通过，未杀进程。
实际Start-Process为Hidden／PassThru，WaitForExit与Refresh后取进程ExitCode；测试不带-quit。
```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB07Compile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB07-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB07Tests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```
编译PID2912：UTC 2026-09-21T09:42:45.8568229Z→09:42:59.4495341Z，实际exit0。
测试PID39196：UTC 2026-09-21T09:45:36.6272273Z→09:45:55.4249233Z，实际exit0。
本批编译与全量EditMode各一次，无运行失败或冻结后源码修正；两日志C#错误／警告均0，结束后Unity进程0。
原始CommandExecution：编译exec-ea0bd2c5-e7b8-4132-8ff8-86c69367d19d；测试exec-1ac89092-c2cb-4f7e-ae0e-90a913b61518。
完整实际命令、PID／UTC／实际退出输出及原始回执路径／ID见scope.validation，未以外层shell退出或日志存在冒充成功。

| Unity验证产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB07Compile.log | 48f7592a854521988c79cd34122a4079616d8b476ad02d04da0909516243bb28 |
| Logs/FMDemoB07Tests.log | 4995f53bd96023c1533f34ff8c324c3d28048b86a38049881d9cd6c46ca1d302 |
| FMDemoB07-EditMode.xml | cc21070c6598530745d82d2773161e391319f6726a0c8b3500ba91b50a8d853d |

XML执行UTC 09:45:44Z→09:45:53Z，1802/1802 Passed；failed／skipped／inconclusive均0。
原B06 XML SHA=a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e，1667条／1662不同Ordinal fullname完整保持。
原集合及当前旧子集的fullname＋TAB＋count＋LF规范SHA、逐名Passed计数SHA均为c837f7b0ecc9939161771fb54636efbad78b1114d6ed30bf4637437160fb7f05。
新增仅FightMatch.Core.Tests.CandidateEnemyPhaseTests的135条，无其他新增fixture或用例。
scope最终261580字节≤300KiB，SHA=376d7bd4a885be736ac4f6158653d6c15404f99ff108fae81ec72b2c68a67a1c。

只读输入在开工或追加来源记录时留SHA，验证后逐项核相同：
| 输入 | SHA256 |
| --- | --- |
| AGENTS.md | c1a836ff14cc0834e16c33cbc49d53bec82cadff656a329c24c72f569d460528 |
| .agent/PROJECT_CONTEXT.md | 4cf1e141e39f12165103e52e44357b1c179175bd18d2c874f386f0360abfc9a9 |
| .agent/CODING_RULES.md | 35dcf6b76ec660509414580ba160dff080dc76d2450293b59a79a7cc91f91f9f |
| .agent/PLANS.md | 5039f9bc8a51eafac968b652b09906b90c6f335395dc3afec3899fb83a7324eb |
| .agent/VALIDATION.md | b65114c2444dee680741e52307fec57cc0abc4b1196767962bc8e4df322690a0 |
| .agent/REVIEW_CHECKLIST.md | b5c39a7c85d2fd41182ac4ad3db30cf7ed28e0d9a696354f4cfe5a507e57c8a7 |
| docs/system-design/2026-09-17/system-task-packets.md | 9d6c1c90816785fbb5d47d01cf797801dfb63b20259373dab6fa0810d60a0dad |
| docs/system-design/2026-09-16/session-plan.md | 23ceca068a2c10cd2d4c15bd9929737d150823668cd921942c9bf7f0d368bb0d |
| docs/system-design/2026-09-17/integration-review.md | 5f89b6a6f9ac6f21f321d80ccb73246c2165e90f18af4bbceea9e15fcba632cf |
| docs/system-design/2026-09-17/demo-011-delivery.md | 0882f13ec45e0af19ca2cb6b5b5def32f6532f48cc17d884b696e1579ff07b65 |
| docs/system-design/2026-09-17/demo-011-scope.json | 772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859 |
| docs/system-design/2026-09-17/demo-022-delivery.md | 8675a64fc437ccc392251e9d3b6fe643cafdf03d0d4c50277cba073b395b54eb |
| docs/system-design/2026-09-17/demo-022-scope.json | f7890d0f7f267f522b52049a5b6135474733b6c913451acfd9e8f63a76966a17 |
| docs/system-design/2026-09-17/demo-batch06-code-review.md | f088d040a6276db98fdd7367d2938700852301e61da17a3d9b50611d31ba88c9 |
| FMDemoB06-EditMode.xml | a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/system-design/2026-09-16/details/contribution-scoring.md | 5194dc775eeb2bf359604eb1966a20a51062c7c48bfedfce5c15601ec1931912 |
| docs/game-design/2026-09-14-fightmatch-game-design.md | 5495ae36bc1e03cb42ffd428416d2c36855370935dfda5769ede28317393e82a |
| docs/game-design/2026-09-15-three-group-recommendations.md | 1c86e32869355ad9ead54f333ef3df8ffb3238cee658efc92c2f37979d93f223 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |
| docs/game-design/balance/chapter_model.py | 2052792ccc3aae007f6a23a2f799d4d724c3ecfde97644d9731e4d706bbec390 |
| docs/game-design/balance/boss_model.py | 5851df08a924ac3bb31e9a70e154176f66f3cf8b7309a3a9dd7b7ebb7d5e5716 |

git -c core.safecrlf=false diff --check退出0；既有tracked摘要34 files changed, 2809 insertions(+), 597 deletions(-)保持。
既有FlowPuzzle／Packages／本地权限等差异不属本包；无Git写入、新任务／子代理／worktree、旧文件或辅助运行文件修改。
本包回滚范围仅010自身八个新实施文件及两个交付文件；实际未执行回滚。
完整交付交R／SD00后结束本次原实施turn并停止修改；R待该turn completed独立审查，不等待R先ACCEPT，不启动012或其他后包。
