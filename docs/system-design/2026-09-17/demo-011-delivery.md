# FM-DEMO-011 交付记录

状态：IMPLEMENTED／验证通过，交回独立审查；本文不作ACCEPT结论。
任务A：01a0c1fb-248d-7321-8b69-9efd26b817f1；本次011 turn：01a0c302-7e91-7ce2-b3c8-8ef26440b270。
授权：system-task-packets.md r70 §97／98；D按§99负责022，R按§100独立审查；沿原目录gpt-6-astra／max。
本包仅新增3份Core、1份EditMode及4份Unity meta、本文与scope；8份实施文件1324行≤1800。
所有377旧源码／测试／meta保持；BattleSnapshot.cs、CoreTestAccess.cs、asmdef、依赖和配置未改。

实现：AfterAttack消费009请求与完整最终HP投影，CompleteLink消费无Actor的免费补线请求。
两个纯入口返回不可变StageDecision／明确拒绝；保留BeforeSnapshot、原Baseline、请求来源和最终HP依据。
校验原身份／修订／参与者／当前面／棋盘，HP完整唯一且不得治疗或复活；几何委托001并保留原因和CellIndex。
攻击目标仍活则临时线消失，死则固化；其他死亡按原Face定义顺序加入Pending，玩家可任选待补Pair。
免费补线固化准确路线、移除准确Pending；全队倒下也可补线，不接收外部HP覆写。
出口依次为待补→本面活敌→准确下一面→最终面胜利；保留旧面HP依据供012组装下一面。
不推进修订／行动／敌阶段／HP／意图／贡献／随机／PRD，不重算射程／伤害，不产生PostSnapshot、FinalAttemptReport或Completed。
阶段投影不证明真实攻击或敌阶段已完成；012仍须核CombatFrame并封装完整后态。合成多死敌用例不冒称AOE／DOT已实现。

| 七项验收 | 已执行的行为见证（CandidateBattleStageTests） |
| --- | --- |
| ①攻击线结果 | AttackLineUsesFinalHpAndPreservesExistingHistory（8）、Real009AttackFeedsStageWithoutRepeatingItsDamageOrRandomWork（1）：活／死、全倒与反向组合，既有固定线、非零修订／计数／累计／已消耗随机保持；真实009结果可投影且不重复伤害／抽样。 |
| ②其他死亡与顺序 | OtherDeathsUseDefinitionOrderAndPlayerCanLinkTheLaterPendingFirst（2）：四Pair定义顺序与HP输入顺序不同，另两死者按原定义进入Pending，玩家先补后列Pair。 |
| ③全倒免费补线 | 同上全倒／非全倒组合，以及FreeLinkOnlyAcceptsAnExactPendingPair（5）：全0仍可补线，余Pending继续AwaitLinks，补完且有活敌则按成员存亡取AwaitRescue／AwaitAction。 |
| ④翻面与终局 | ClearingTheLastLineFlipsExactlyOneFaceOrWinsEvenWhenAllDown（10）、FinalFaceVictoryDoesNotRequireAnActionableMemberOrFullBoard（1）：攻击／补线、单／双／三面、全倒组合；只翻一面，新Board为空，旧HP依据保留，最终面即使全0且未满盘也胜利。 |
| ⑤前态与覆盖 | BothEntrypointsRejectInvalidOriginalStateCoverage（22）、ProjectionMustCoverExactlyTheOriginalKeysWithoutHealing（14）、OriginalBoardRequiresEveryDeadPairExactlyOnce（8）及缺字段／身份／阶段用例：漏重错键、负HP／超Max／增HP／复活、错面、锁线待补冲突、旧修订等拒绝且输入不变。 |
| ⑥几何 | AttackAndFreeLinkPropagateExact001Reasons（7）、OriginalFixedRoutesMustBeLegalAndMutuallyDisjoint（2）、ReverseFreeLinkAndUnsolvableFutureAreStillLocallyLegal（2）：两入口正反向、原ReasonCode／CellIndex、旧线互撞及当前合法却堵后续解；无求解器或满盘检测。 |
| ⑦隔离与预算 | EveryRequestAndHpListIsDetachedAndAllReturnedCollectionsAreReadOnly（2）、两组SmallBudgetRechecks（13＋11）、FinalHpNumeratorAndDenominatorUseTheSameNewBudget（4）、SharedAndLateMathLimitsProduceNoPartialDecisionAndFreshBudgetReplays（2）：同一预算重验保留基线／下一面／原大数／投影分子分母，零／末步／共享耗尽无部分结果，深只读和新预算重放。 |

本fixture全部28方法／161用例均Passed；上述映射不另计重复覆盖，完整方法计数见scope.validation.xmlSummary.new011Methods。
无MonoBehaviour／Editor UI变更，本包验证为纯Core及EditMode，不涉及PlayMode／Player发布。

before继承B05原capturedAtUtc=2026-09-21T07:27:30.2472227Z的377项，不冒称本轮新现场。
B05原scope SHA=a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec。
原清单规范SHA=8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307。
A实际开工UTC 2026-09-21T08:10:40.6207137Z，逐项核377相同，自身10新路径不存在，D新路径尚未出现。
初始scope SHA=9e14324631d2bd40c82d0f4cf024f5acb98a629682376ef9bd268c63ae76ce31；before／startedSnapshot原字节保留，之后只追加证据。
A四份CODE_READY为08:30:44.7841659Z，规范SHA=da5fe45c21c9d9631f315f67e3aef2b559c76e76ad9d368108d13c02994148c9。
D五份CODE_READY为08:30:22.4059100Z，规范SHA=1b8cf273805460553239b042a7e7bc01e56a7ccbb6a360533acca1d2e6aed063；D通过任务消息明确交A，不等待D turn completed。
D任务01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e，本次022 turn为01a0c303-1f1c-7993-a74b-1238a5f1e8b9。
联合冻结UTC 2026-09-21T08:36:22.6216656Z，9份规范SHA=00efd9b85e6128b86ddef1d22f4be74153740c3ad14ec939747373352ebca456。
验证前386项＝377旧＋9新C#，规范SHA=29ad2b4ff0649682f3252ffb829d5801c38b5f79c39e20e1bae1d9155d41a797。
测试前UTC 2026-09-21T08:38:28.1365170Z及测试后UTC 2026-09-21T08:43:35.5982311Z均395项，规范SHA=28be3756cf902dcfe9b28bc99869c3f8f20447233000a9fe7842d4ac84900b94。
最终395＝377旧＋18新；原377逐SHA不变、九份冻结源码始终不变、无其他源码／测试路径变动。
规范统一为Ordinal相对路径＋TAB＋小写SHA＋LF（含末尾LF），UTF-8无BOM；完整逐路径清单见scope。
九份meta全部由A的统一Unity生成；原源码meta原字节保持，新9GUID在全Assets的222份meta中各只出现一次。
011新8文件1324行；022新10文件1165行≤2000，D只核其归属，由D独立交验收映射。
scope.preTestSnapshot的两项聚合行数因shell字典管道为null；测试前preTestLineBudgetCheck已逐文件重新读取并核1324／1165，after再次确认；原记录保留。

| 归属 | 新实施路径 | 行数 | SHA256 | 新GUID |
| --- | --- | ---: | --- | --- |
| 011 | Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs | 361 | 49df8f388234fe1e0549a9fa7c17c1af534b195b718bdb2c13eaae4396516397 | — |
| 011 | Assets/Scripts/FightMatch/Core/CandidateBattleStage.cs.meta | 11 | 5471dec04f93fdbf297b0db7cc18150ac5bd7eda309fd3aa9c5b220bd90f6aeb | 1c264dea39eff8149882ec390ca94425 |
| 011 | Assets/Scripts/FightMatch/Core/CandidateStageRequests.cs | 73 | e75486f7762148c4be35dfcdfa0c76c1925a9ce3809575ac35d3851c7a65d4c1 | — |
| 011 | Assets/Scripts/FightMatch/Core/CandidateStageRequests.cs.meta | 11 | a1e90a226f07b1a08de0c21e913520db7eed499e469ce097cc20da25642a75d8 | 6e64a05934f930b4da2ca2ef043dbea7 |
| 011 | Assets/Scripts/FightMatch/Core/CandidateStageDecision.cs | 77 | 91f2da1aaeb429c432e9b7e4b793b4f595da4dd1253c40ebb2cfa54fbcf06589 | — |
| 011 | Assets/Scripts/FightMatch/Core/CandidateStageDecision.cs.meta | 11 | 374b4ed5a19c35ae398fee4f1f693474496c321b3f7f0e9353dfcd8437c8daa7 | df75bc9dda2ea684da2bb1493ce84333 |
| 011 | Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs | 769 | c97fefd71e86c4027610dfc2667c931e324352c1a2f6230bcd6d2421d4098a9b | — |
| 011 | Assets/Tests/EditMode/FightMatch/CandidateBattleStageTests.cs.meta | 11 | f488900deb9e7f9145b74784026d69f67ebe36ecf3831f11cf9560ae815f4c98 | 4615dd06ac298204cbd3484a48c43839 |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs | 134 | 5f9bb98dd191fd30672c519897d1fb3b7b262b895da1b646a3172f478e047227 | — |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionDefinition.cs.meta | 11 | a2530e2576845fc088e09b98cdca0728172ebcdb28ea84a4bbf90d93619ca20a | 5122f868670fef348ad315bfd0a78d7d |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs | 161 | c1e7caabee302a8f8fb833fefce85aa9e08a11cf3532d1aabac2de6ff920f44f | — |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs.meta | 11 | 83b83db4d4fe0e6485d056e745305e4163b2352ed63c0477c6e0f6d76b4e14b4 | 0747afc4eebc9c34ab2b02133c60e557 |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionRequests.cs | 44 | 14dc1da29930abb81042de5d963db0213c2a1a1d7394c1cda37ad800c1262dce | — |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgressionRequests.cs.meta | 11 | c461ac2b5dc5e8c3f79ca2568d541a47577274fb595b938dfc0cb14384e4e28d | 55e2a683a0fe0364cb6455d8c22850e6 |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgression.cs | 256 | 5e7863f065e3014722c8ec59647c4a1fb9982472ab632c0a843301245b395b6b | — |
| 022 | Assets/Scripts/FightMatch/Core/CandidateProgression.cs.meta | 11 | 1797fecabe31515fbab6c18be08f999c9fce9b8a8f711f8c5108196181ea964f | a08aa557aed74d14296c3fdcec953191 |
| 022 | Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs | 515 | 3281992c87ba5110b7e26dd9ffd5ad0ca2793816a20e728254ab58289cb1d2b8 | — |
| 022 | Assets/Tests/EditMode/FightMatch/CandidateProgressionTests.cs.meta | 11 | 997590d422c073422de1c40c1b6a88a57809609900111712e83a25a796f28ba3 | fc576b65d9ab81e499890637c8f3176c |

两次运行前核固定Unity 2022.3.18f1路径、Unity进程为空，未杀进程；A唯一运行，D未启动Unity。
实际均Start-Process -WindowStyle Hidden -PassThru，WaitForExit与Refresh后读取进程ExitCode；测试无-quit。
```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB06Compile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB06-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB06Tests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```
编译PID48284：UTC 2026-09-21T08:37:01.2397521Z→08:37:14.3811806Z，实际exit0。
测试PID11804：UTC 2026-09-21T08:39:25.0913875Z→08:39:40.9461140Z，实际exit0。
本批编译／全量测试各一次，无失败后源码修正；两日志C#错误／警告均0，结束后Unity进程0。
原始CommandExecution回执：编译exec-353a4a0a-5467-40e8-b137-98937301ebf4；测试exec-bec3ad5b-8f61-44e4-97cd-026f39e42ce2。
完整实际命令、进程输出、原始回执路径／ID见scope.validation；未以日志存在或PowerShell外层退出码代替Unity实际ExitCode。

| Unity验证产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB06Compile.log | 7a901d0c28818c47130388a8a38356861e884f78c15468195eba28ae35425110 |
| Logs/FMDemoB06Tests.log | ed2e074e39201191ebfb333cf995e4143ca8e94460e28c3e31280548efc74f5e |
| FMDemoB06-EditMode.xml | a655ff9a47802338c9318b5bd2496abcc7dece04723313c15b6060e56c807c6e |

XML执行UTC 08:39:32Z→08:39:39Z，1667/1667 Passed，failed／skipped／inconclusive均0。
原B05 XML SHA=6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0，原1414条／1409不同Ordinal fullname完整保留。
原集合与本次旧子集的fullname＋TAB＋count＋LF规范SHA、Passed计数SHA均为9f02563b07c81569d758b57e3a687736ede5de25c51667bc53970c26959e8c4e。
新增仅FightMatch.Core.Tests.CandidateBattleStageTests=161和CandidateProgressionTests=92，均全Passed，无其他新增用例。
D业务实现未由A审改；此处只记录联合冻结／编译／测试／meta证据，D按其包自行交回。
scope最终265240字节≤300KiB，SHA=772e52405711fe4fadbab2602cf478573c6d3acf2cb7de5e07191bedb3b83859。

只读输入于UTC 2026-09-21T08:36:22.6216656Z记录，并在after逐项复核无变化：
| 输入 | SHA256 |
| --- | --- |
| AGENTS.md | c1a836ff14cc0834e16c33cbc49d53bec82cadff656a329c24c72f569d460528 |
| .agent/PROJECT_CONTEXT.md | 4cf1e141e39f12165103e52e44357b1c179175bd18d2c874f386f0360abfc9a9 |
| .agent/CODING_RULES.md | 35dcf6b76ec660509414580ba160dff080dc76d2450293b59a79a7cc91f91f9f |
| .agent/PLANS.md | 5039f9bc8a51eafac968b652b09906b90c6f335395dc3afec3899fb83a7324eb |
| .agent/VALIDATION.md | b65114c2444dee680741e52307fec57cc0abc4b1196767962bc8e4df322690a0 |
| .agent/REVIEW_CHECKLIST.md | b5c39a7c85d2fd41182ac4ad3db30cf7ed28e0d9a696354f4cfe5a507e57c8a7 |
| docs/system-design/2026-09-17/system-task-packets.md | 187bc5701505ec4ab012ec0becc5c8482caa8a4173d49a9d67d454d45964ca61 |
| docs/system-design/2026-09-17/demo-009-delivery.md | f50340250a20092d3248ff29e22d4d4d079a772868e684edae3ec1dedba52bbc |
| docs/system-design/2026-09-17/demo-009-scope.json | a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec |
| docs/system-design/2026-09-17/demo-021-delivery.md | fd5d8cc102cc8ebc011c7cb7bb4fb6b0809c7a24bab5d4de2b49ae4c09b79d24 |
| docs/system-design/2026-09-17/demo-021-scope.json | a9a2f9548fbd131782810df9158b8f58c7286b462e344533a422582be5f162ab |
| docs/system-design/2026-09-17/demo-batch05-code-review.md | 0544131a93fd7113818e7cf1e44420773e628500dbfe5421997fc8b8e27d2e71 |
| FMDemoB05-EditMode.xml | 6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0 |
| docs/system-design/2026-09-16/interaction-contracts.md | eab83027c412619731df1f7f3fd1650161915d96e31d68ea30f4af4af9f96f06 |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/battle-effects.md | 3cfd66243a2846cf5ace15bf21f88e7d6d8180481f44695cc2b588433bc3065d |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/game-design/2026-09-14-fightmatch-game-design.md | 5495ae36bc1e03cb42ffd428416d2c36855370935dfda5769ede28317393e82a |
| docs/game-design/2026-09-15-three-group-recommendations.md | 1c86e32869355ad9ead54f333ef3df8ffb3238cee658efc92c2f37979d93f223 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |

git -c core.safecrlf=false diff --check退出0；既有tracked摘要34 files changed, 2809 insertions(+), 597 deletions(-)保持。
既有FlowPuzzle／Packages／本地权限等差异不属本包；未写旧文件、配置、依赖、权限、其他业务或运行辅助文件，无Git写入、新任务／子代理。
本包回滚范围仅011自身8新实施文件及2交付文件；实际未执行回滚，不触碰022或原有差异。
联合证据交D补齐022报告；两作者本次turn completed后由R独立审查，A停在011，不启动010／012或其他后包。
