# FM-DEMO-012 交付记录

状态：IMPLEMENTED／验证通过，交R独立审查；本文不作ACCEPT结论。
A任务：01a0c1fb-248d-7321-8b69-9efd26b817f1；本次012原实施turn：01a0c384-70db-7790-9bbb-12948f05fc65。
授权：system-task-packets.md r73 §108～111，业务／白名单／验收沿§109，验证沿§110。
本批仅012，原目录local、gpt-6-astra／max；A唯一作者与Unity执行者，R独立代码审查，SD00负责系统设计。
仅新增五份Core、一份测试及六份Unity meta，共12份实施文件1694行≤2600；本文及scope另计。
403个旧源码／测试／meta逐SHA保持，未改旧接口、配置、权限、依赖、数据资产或其他包。

CreateCandidate只取原Binding.Start，核原准备资料／三随机域及修订1、零计数、首面、原HP／PRD、空线路与贡献。
BattleEntryPreparer重验原定义的支持域，保留原Entry／Baseline／InitialSnapshot，不把重建校验值替换入场来源。
新攻击一次实际调用009→010→011，RandomSamplingBudget.Math贯穿；原009与010事实、随机前后态／全部原始字完整保留。
012按010最终HP投影交011，之后一次组装修订+1、有效行动+1、敌方阶段+1，即使010为空或N010中断也仅计一阶段。
翻面只按准确NextFace初始化新敌／Cursor0；我方HP、累计及随机保持，新面敌不在本次出手。
免费补线仅调用011，修订+1，其他计数、HP、随机及贡献不变；条件明确null，没有虚构角色／偏好。
可变请求由原片段复制，条件另复制成只读值；操作时间为调用方冻结的非负整数，允许0及不单调墙钟，不读取系统时间。
记录持原前后态、请求／条件、三段来源、连续OrderedFacts与逐真实HpLoss贡献段；Charge与阶段无伪伤害，真实0量仍保留段。
历史OperationId按Ordinal唯一；旧修订、错来源、缺段、错贡献／后态拒绝，无防重缓存、无半份NextRun或Record。
历史数值逐项重验；旧009与随机机会不重抽，010／011确定性片段交原模块复核，012不另写敌循环或补线规则。
高修订恢复兼容：前缀间仅允许抬升SceneRevision，业务状态完整连续；每条自身After=Before+1，原记录保留原修订。
空前缀只能对应原入场业务值；本包不提供恢复／任意导入入口，013仍须验证HistoryAnchor和Superseded。
末条有效Record先入Records；仅最终面WonPendingSettlement、全敌0HP、无Pending且固定线完整时形成报告。
报告含原入场／全关定义、完整有效Operations、终态、按事实顺序展开的贡献、原等级、三域与暴击轨迹。
从原0累计核Dealt／Taken与终态一致，全关初始敌HP包含所有面；终局时间和TerminalOperationId取最后Record。
消费覆盖明确EmptyCarryNoUse，Outcome=NormalVictory，Run／Report始终CommitEligible=false；未转Closed、未发奖励。

FMBR01使用手工固定字段顺序和已接收声明，SHA-256输出64位小写hex，唯一排除报告自身Fingerprint。
ASCII头FMBR01＋LF、u32小端长度、规范整数／有理数、原UTF-16代码单元、LE64 PCG core及LE32原始字均显式编码。
生产编码无反射、JSON框架、对象摘要、平台字节序或文化排序；内部比较用完整显式值而非哈希相等冒充来源。
Compute只提供确定编码及同预算数值复验，不是来源认证或封闭校验；023仍需核原报告、绑定、覆盖和精确汇总。
无保存、回退、复活／广告、奖励、UI、发布或其他职业／效果；只验证纯Core及EditMode，不涉及PlayMode／Player。

| 七项验收 | 已执行见证（CandidateBattleOperationsTests） |
| --- | --- |
| ①原入场及链 | CreationRetainsTheOriginalEntryAndAllThreeRandomDomains（2）、CreationRejectsMiddleOrUnsupportedBindings（8）、RandomBindingDomainsAndInitialPrdCannotBeSubstituted（6）；RestoredHigherRevisionsKeepTheEffectivePrefixAndOriginalRecordRevisions（3）核0／1／2前缀及2^80高修订；根null（9）、条件时间（6）、原009拒绝／几何（7）、重复／终局（2）保留准确原因与零变化。 |
| ②真实完整行动 | PublicSourceLevelsCloseOnlyAfterTheRealFinalOperation（4）：L1／L3各两种明确坐标来源，均从007A／008公开准备、012公开创建和攻击完成。L1两击W95、Dealt30／Taken5、两行动／敌阶段、两固定线及含末击报告；L3三步W90、Dealt35／Taken10，首步Charge和两次E01承伤保留。 |
| ③阶段与新面 | FlippingCreatesOnlyTheNextFaceWithoutGivingItAnEnemyPhase（2）核双／三面逐次翻转、全关初始HP和空敌段计数；N010StopsLaterChargeAndStrikeAndPublishesRescueWithTheLethalFact（2）核1/3HP致死、14/3溢出、后续意图游标保持、AwaitRescue且无报告。 |
| ④补线接线 | ExplicitSyntheticLinkAssemblyHasNoCombatAndCannotBecomeAnImportedPublicRun（4）显式合成死亡／待补前态，011补线经012内部组装成无战斗记录，保留计数／随机；最后Link记录产生终局或准确新面。缺死亡历史的公共Run拒绝。PublicLinkKeeps011InvalidPhaseCodeAndNeverInventsConditions（1）保留原011阶段拒绝。 |
| ⑤报告与来源 | IncompleteOrAlteredStateChainsCannotBeContinuedToAReport（10）、MissingFragmentsAndForgedContributionOrFactRowsCannotSeal（15）、NumericallyIdenticalForeignDefinitionsAreNotOriginalFragmentReferences（2）核漏前／末步、错绑定／面／HP／累计、缺三段、错操作、漏重贡献／换受益者／错FactIndex、伪覆盖、错误伤害及异定义引用。原入场19级另在完整报告保持。 |
| ⑥字节与身份 | Fmbr01HasTheExactIndependent51ByteVector（1）逐字核51字节及c3918a…SHA；FingerprintIsCultureIndependentAndPreservesOriginalUtf16（4）核en-US／tr-TR／ar-SA／zh-CN、分隔符／中文／代理对与孤立代理代码单元、null／空及不规范化；来源组变化（8）与未知枚举（1）核敏感性及Fingerprint自身唯一排除。 |
| ⑦原子与预算 | RequestsConditionsAndAllPublicOutputCollectionsAreIsolated（1）、LateSharedMathAndSamplingLimitsPublishNoPartialRunOrReport（2）、TheSameSmallBudgetRechecksLargeRetainedInputsAndHistory（6）、CreationBudgetAndTerminalEncodingHaveNoPartialResult（1）核输入后改、只读图、0／末步／封闭编码／共享预算、零随机字预算、原大基线／历史时间／偏好／修订；新预算重试相同。ZeroHpLossStillHasAContributionSegmentAndCertainCritUsesNoWords（1）核真实0量与必暴0字。 |

全部26方法／109用例Passed；完整方法计数见scope.validation.xmlSummary.new012Methods，同例覆盖多项不重复计总数。
FixedSchemaIncludesEveryFrozenOriginalFactFieldInDeclarationOrder（1）使用测试侧独立解码器核固定字段顺序、原事实所有声明、流与Words字节；反射仅用于测试核冻结声明，不进入生产编码。
L1／L3使用真实固定seed42／sequence54和显式合成C=1/1000，三次机会实际均未暴击；只作有来源条件样例，不称生产随机保证或自然进度证明。
当前单Warrior域自然无Pending；合成补线只证明内部组合和拒绝缺历史，未伪称AOE／DOT或公开整关自然补线已出现。
数学预算覆盖约定的整数与有理数，不声称覆盖全部堆内存、载荷字节或总运行时间；未裁剪历史／事实。

before继承demo-010-scope.json.after.files原403项及原UTC 2026-09-21T09:49:21.5765037Z。
原scope整SHA=376d7bd4a885be736ac4f6158653d6c15404f99ff108fae81ec72b2c68a67a1c，原403规范SHA=f20904bef62b1f3378837fd555a98eae655e04c72ac1c8449a90d234f7b690b5。
真实startedSnapshot UTC 2026-09-21T10:34:13.7383130Z，逐项核403相同，本包14条实施／交付路径均未存在。
初始scope64935字节、SHA=65d261cc81be37e197c8eaaed88e2574f209df45cc54ec676c028f9e4af0c336；原字节未回写，之后仅追加。
派发稿SHA=9bd574b0201524e9363bcb37caa02571cfcad4304a1563260cabc392b5a4c890；开工稿已含SD00获准状态／turn登记，实际SHA列于下表。
开工§109全文SHA=574e57ec0ae484424ab6f02e89e0a1bdfce1e4c15842fba9c6967918aeb7329a，业务契约保持。
CODE_READY UTC 2026-09-21T11:00:56.2046900Z，六C#规范SHA=169ca8d6a130aedb1e4dd3c38e65f10d44b3133aafadffa1831c77d28716d2a8。
编译前409项＝403旧＋6 C#，规范SHA=093aee14927150948a2f020d8af79c9ef0dd301c583b9fbed21d66127b9d689d。
原codeFreeze.sourceLines因OrderedDictionary聚合为null，11:01:28.7656922Z的codeFreezeLineBudgetCheck在Unity前逐项核1628＋66=1694。
另一次只读路径预检误含不在原清单的两个外层文件夹meta，调整枚举后通过；没有文件写入或Unity运行。
测试前UTC 2026-09-21T11:06:13.4201479Z、测试后UTC 2026-09-21T11:10:55.8788837Z均415项，规范SHA=3efae32d1ec770ef3b1ae2410330ad579225d78181f8aa263251e4deae3ef4b2。
最终415＝403旧＋12新；403旧SHA、六源码冻结SHA、232份meta中的6新GUID唯一性及原226个GUID路径均通过。
原226个GUID路径规范SHA=ddefcd00f321cc9061e774c8dbfbf60b38640784fc40015ad5ceb1058b05ba83。
文件规范为Ordinal相对路径＋TAB＋小写SHA＋LF（含末尾LF），UTF-8无BOM；GUID规范相同但第三项为GUID。

| 新实施路径 | 行数 | SHA256 | GUID |
| --- | ---: | --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateBattleRun.cs | 60 | f6b94a85e1c7aef00a21984a2649ed7d11585b7dea21c206ffecc2eb27bac0f0 | — |
| Assets/Scripts/FightMatch/Core/CandidateBattleRun.cs.meta | 11 | fe5676cd32b3cb72a1f58e801053a65fb4acd5deb189edf5b4e264d0f22a4ce0 | 8e603cc42d25358488bbfa880de17a5e |
| Assets/Scripts/FightMatch/Core/CandidateBattleOperations.cs | 459 | 4e487c141f3c2a25f6ae838e5f364d5018f3a116329208ce3757e52d85fab856 | — |
| Assets/Scripts/FightMatch/Core/CandidateBattleOperations.cs.meta | 11 | e0051bb5a9364c3e14107af0eccb60780a058eb7c2605d5bc89b3928099f1f0e | b4a29303d128da54c9057c95fa83a813 |
| Assets/Scripts/FightMatch/Core/CandidateBattleOperationRecord.cs | 77 | 8f1b1eab45e9cc694fa3ff064b368988f5dde4e3aa196fbfbfb6c36ab5a3eada | — |
| Assets/Scripts/FightMatch/Core/CandidateBattleOperationRecord.cs.meta | 11 | 1976c6d983aba7a74e8792149a01d70969fba82a266fe945202c35c64779e160 | 83ed46d11225aec4aa50845076207447 |
| Assets/Scripts/FightMatch/Core/CandidateFinalAttemptReport.cs | 41 | ba8b7340aa230571061edbdee5971a8491b2e1a5f571b781ca51be4f38767bad | — |
| Assets/Scripts/FightMatch/Core/CandidateFinalAttemptReport.cs.meta | 11 | 3959c3201df599a731890c5af3792c8f31f7d30df14e61f58866b4085e9351c9 | 01b6b0ac084628844989b3eef3d0baf8 |
| Assets/Scripts/FightMatch/Core/CandidateBattleReportFingerprint.cs | 290 | bb35f4fb3c6943ec4f6e8f17a688d95fcd4f596592fd0839ed596915c798654e | — |
| Assets/Scripts/FightMatch/Core/CandidateBattleReportFingerprint.cs.meta | 11 | 316e057fad5aa6cc7b8c00408173dd2aafd128e5d5d8b8af1e4a84dc65b985f2 | 3d838c92492ddc344b42009e9e4213d4 |
| Assets/Tests/EditMode/FightMatch/CandidateBattleOperationsTests.cs | 701 | 64cbe40545a754f698b92f36a3c2a2c0689bab6b1dfa275092f52a0235ff6cd2 | — |
| Assets/Tests/EditMode/FightMatch/CandidateBattleOperationsTests.cs.meta | 11 | 02922d8ecb9c6b4b4df74463542b458b0c6b5d4d243b1991a5a986278d2121b3 | 3880e178e195d8d41866447a7b9bf66d |

A每次启动前查固定Unity路径与进程排他，无杀进程。均Hidden／PassThru，WaitForExit＋Refresh后取实际子进程ExitCode。
首次普通沙箱编译启动PID22668，UTC 11:01:57.6484454Z→11:02:58.0587602Z，实际exit199。
原日志为许可证IPC连接拒绝0x8000000a、等待60.04s超时，未进入编译，也未生成meta；不能表述为C#失败或许可证无效。
其699字节全文先保存scope.compileAttempt1.evidence.logText，原SHA=8ada4f8a01f2f73a17b9e8a36f5027b71df751eb040abe9a360a830d8baf8e60。
自动审批通过沙箱外执行后，同源码编译重试PID6772，UTC 11:04:56.9623462Z→11:05:09.8016988Z，实际exit0。
全量EditMode仅一次，PID37360，UTC 11:06:46.5516841Z→11:07:06.9998619Z，实际exit0；日期均2026-09-21。
六份C#自首次冻结起未修正；成功编译与测试两日志C#错误／警告均0，结束后Unity进程0。
成功编译日志仍由Unity写指定B08路径；旧B07产物未覆写，失败首轮原文／SHA／命令／回执仍可核验。
```powershell
$unityPath = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $unityPath -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB08Compile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $unityPath -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB08-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB08Tests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```
原始CommandExecution依次：exec-9729946f-a4d3-4491-9ae3-fa81fe0ea724、exec-95c295ec-1d04-493b-8011-7ef900c08005、exec-cddcd214-931f-4916-a6b5-0890c7ccd2b2。
scope.validation含完整实际命令、原始command数组、stdout／stderr、PID／UTC／退出、记录路径及SHA；未用外层shell退出或日志存在冒充成功。

| 最终验证产物 | 字节 | SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB08Compile.log | 64226 | 0876a2142fe3b4cce4755332292b3b112c0018a10c4c799e5be7a65e4e690e89 |
| Logs/FMDemoB08Tests.log | 89033 | 8bd25e8456b852e18e8e6f01f4c6142085e8ea59e6a4911465d5c557b71e93bd |
| FMDemoB08-EditMode.xml | 1246677 | a384933b9f9f5b9c3c12017ec0cd8e1b64c6d4115cae44f06c438528176167fd |

XML执行UTC 11:06:53Z→11:07:05Z，1911/1911 Passed，failed／skipped／inconclusive均0。
原B07 XML SHA=cc21070c6598530745d82d2773161e391319f6726a0c8b3500ba91b50a8d853d，1802条／1797不同Ordinal fullname全保持。
原集合与本次旧子集的fullname＋TAB＋count＋LF规范SHA及逐名Passed计数SHA均为15e52a8aa724171d463aa4338eadb74f3e3873db7dcbc052ab84dd10907cbd69。
新增仅FightMatch.Core.Tests.CandidateBattleOperationsTests的109条，无其他新增用例。
scope最终252014字节≤300KiB，SHA=f14c371e5468e99451327e9bb573107e1b52e4334c767d8d2859a98a52cb2ff7。

下列30份只读输入开工留SHA、验证后均不变：
| 输入 | SHA256 |
| --- | --- |
| AGENTS.md | c1a836ff14cc0834e16c33cbc49d53bec82cadff656a329c24c72f569d460528 |
| .agent/PROJECT_CONTEXT.md | 4cf1e141e39f12165103e52e44357b1c179175bd18d2c874f386f0360abfc9a9 |
| .agent/CODING_RULES.md | 35dcf6b76ec660509414580ba160dff080dc76d2450293b59a79a7cc91f91f9f |
| .agent/PLANS.md | 5039f9bc8a51eafac968b652b09906b90c6f335395dc3afec3899fb83a7324eb |
| .agent/VALIDATION.md | b65114c2444dee680741e52307fec57cc0abc4b1196767962bc8e4df322690a0 |
| .agent/REVIEW_CHECKLIST.md | b5c39a7c85d2fd41182ac4ad3db30cf7ed28e0d9a696354f4cfe5a507e57c8a7 |
| docs/system-design/2026-09-17/system-task-packets.md | 2c50cda9cf33297c686be31123f344181627809800fa5bbf2aedc4a097a87c92 |
| docs/system-design/2026-09-16/session-plan.md | 2e89cda84e4037cbe51c6abdd68891d54b0e357eae5ff193e8f5ce9826d7d480 |
| docs/system-design/2026-09-17/integration-review.md | 61a3b45594f18173227455349e270e4b6470ae9dae0fcfe817095294dd3385bc |
| docs/system-design/2026-09-17/demo-009-delivery.md | f50340250a20092d3248ff29e22d4d4d079a772868e684edae3ec1dedba52bbc |
| docs/system-design/2026-09-17/demo-010-delivery.md | 760d7776ab47c083d4df61433f197d4eb2d412a468d221e84ba400fe3a844967 |
| docs/system-design/2026-09-17/demo-010-scope.json | 376d7bd4a885be736ac4f6158653d6c15404f99ff108fae81ec72b2c68a67a1c |
| docs/system-design/2026-09-17/demo-011-delivery.md | 0882f13ec45e0af19ca2cb6b5b5def32f6532f48cc17d884b696e1579ff07b65 |
| docs/system-design/2026-09-17/demo-batch07-code-review.md | 5a5df261f89af2fe8ad57a19314f152ea4ac8b217a041d07809319eeed2c9a24 |
| FMDemoB07-EditMode.xml | cc21070c6598530745d82d2773161e391319f6726a0c8b3500ba91b50a8d853d |
| Logs/FMDemoB07Compile.log | 48f7592a854521988c79cd34122a4079616d8b476ad02d04da0909516243bb28 |
| Logs/FMDemoB07Tests.log | 4995f53bd96023c1533f34ff8c324c3d28048b86a38049881d9cd6c46ca1d302 |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/system-design/2026-09-16/details/contribution-scoring.md | 5194dc775eeb2bf359604eb1966a20a51062c7c48bfedfce5c15601ec1931912 |
| docs/system-design/2026-09-16/details/settlement.md | 0d4915dcd800ac97e957d697fce2ab603b081bb1041df75690b3e3a819598a77 |
| docs/system-design/2026-09-16/interaction-contracts.md | eab83027c412619731df1f7f3fd1650161915d96e31d68ea30f4af4af9f96f06 |
| docs/game-design/2026-09-14-fightmatch-game-design.md | 5495ae36bc1e03cb42ffd428416d2c36855370935dfda5769ede28317393e82a |
| docs/game-design/2026-09-15-three-group-recommendations.md | 1c86e32869355ad9ead54f333ef3df8ffb3238cee658efc92c2f37979d93f223 |
| docs/game-design/balance/chapter_model.py | 2052792ccc3aae007f6a23a2f799d4d724c3ecfde97644d9731e4d706bbec390 |
| docs/game-design/balance/boss_model.py | 5851df08a924ac3bb31e9a70e154176f66f3cf8b7309a3a9dd7b7ebb7d5e5716 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |

git -c core.safecrlf=false diff --check退出0；既有tracked摘要34 files changed, 2809 insertions(+), 597 deletions(-)保持。
既有FlowPuzzle／Packages／用户本地权限差异不属本包；无Git写入、新任务／子代理／worktree或辅助脚本。
本包回滚范围仅十二个新实施文件及两个交付文件；本次未回滚，不触碰原有文件。
完整交付交R／SD00后结束本次012原实施turn并停止修改；R待该turn completed独立审查，不等待R先ACCEPT，不启动013／023或后包。
