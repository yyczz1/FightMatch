# FM-DEMO-020 实现交付

状态：WORKER_RETURNED；020 实施和作者验证完成，等待 R 独立代码审查；本报告不作独立 ACCEPT。
8份C#在本次CODE_READY后保持冻结；D已只读核验A执行的DEMO-B04联合编译、EditMode、日志、XML与同版文件清单。
任务 D：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；本次 turn：01a0c26d-99e8-7d91-8432-136dca6d6155。
授权：system-task-packets.md r65 §86、88、89；仅 FM-DEMO-020，不执行后续包。
项目：D:/Unity/UnityProj/FightMatch；Unity 2022.3.18f1。D 未启动 Unity、未手写 meta，无 Git 写入。

## 实现与边界

六个纯入口位于 CandidateCharacterGrowth、CandidateCharacterEnd、CandidateRecoveryClock。
PrepareDefinition 直接校验 CandidateContext 并深复制为 PreparedCandidateContext，不依赖入场结果。
CreateCandidate 仅新建明示隔离候选，初始 revision=1、Ready；不是加载、损坏档修复或已有恢复绕过入口。
固定普通经验按 N(L)=A+B(L−1)+C(L−1)² 逐级求值，保留跨级余量；Amount=0 也保留来源并升修订。
重复 Settlement 先核原 Attempt、受益者、数量、完整 Context，合法重复返回当前 state 与原收讫，增量为0。
基础属性保留精确分数、原槽与候选来源，输出 ComputedBaseStats、目标 p 和倍率，无 C/f；恢复者 EntryHp=null。
结束依据保留原 Attempt、EntryBaseline、EndReceipt、角色、Context、结束类型与显式参与／倒下事实。
同 Attempt 换 EndReceipt 或改固定事实冲突；已成重复返回当前状态。普通结束仅为 Ready 且参战倒下者建恢复期。
立即重来不建新恢复；非参战恢复者保留旧期。恢复 ID 不可再归另一结束，即使原期已完成。
时钟只消费显式毫秒样本；同域单调用精确差，其余用 UTC 并标 DomainChanged/DeviceUntrusted。
倒退返回 IgnoredTimeRegression，保留上次接受样本与累计量；相同样本无变化，零间隔但字段变化可更新一次。
完成记录永久保留原期限与来源；旧完成 ID 返回当前 state 和原期，不影响后来的活动期。
TimeSample 的原 Anomaly 保留在只读样本，推导诊断保留在期／结果；避免给样本补标记后破坏重复比较。
所有公开调用共用调用者 ExactMathBudget，重验 Definition、L/x、revision、历史收讫及结束／恢复时间的整数和分数。
数据错误返回 CandidateGrowthRejectionCode 与 FieldPath、Next=null；根 null 抛 ArgumentNullException，Limit 原样传播。
只读输出构造 internal，集合为只读副本；无第二份累计经验、长期战斗 HP、真实保存或玩家资料写入口。
没有学习、卡证、广告、角色解锁、评分、时间源／SDK、C 参数审定或 H10 提交能力。

## 七项验收映射

以下七项作者核验均 PASS。实际运行是A执行的本次联合验证，D核对逐例结果与同版SHA；独立裁决仍归R。

| §88 | 独立可失败的行为测试 |
| --- | --- |
| ①经验防重 | GrowthTests：FixedReward 的三项指定见证、多级／0奖励；DuplicateReward 返回当前值；ChangedReceiptFacts、NewReward、缺项／负量拒绝。 |
| ②属性交接 | StatsAreExact 覆盖 Lv1/4/20/31/40；ComputedStats_HandOff 用明确合成 C=1/4 经007A/007B，后续升级不改旧基线；RecoveryTests 核恢复者无 EntryHp、原槽2及007A NoReadyMember。 |
| ③精确与拒绝 | MissingDefinitionFields、InvalidDefinitionFields、InvalidInitialCharacter；大等级／经验超过2^53；逐字段大分母／整数以较小预算重验；历史奖励量和组合步骤预算不足后重试一致。 |
| ④普通结束 | OrdinaryDownParticipant、NoNewRecovery、ExistingNonParticipantRecovery；DuplicateEnd、ChangedEndFacts；恢复 ID 再归属、矛盾／缺少事实、无用参数、过时修订拒绝。 |
| ⑤自然恢复 | SameMonotonicDomain 高水位；CrossDomain UTC／Ordinal 域；UtcRegression／前拨封顶；179999→180000与1/3毫秒；旧完成ID和结束遇新期；字段变化零间隔。 |
| ⑥隔离完整性 | MutableInputsAreCopied、EndTimeAndDefinitionInputsCannotMutate；只读图及集合；全部目标／Context／来源保留；服务器可信冒称拒绝，原来源 Anomaly 保留。 |
| ⑦联合验证 | PASS；8份冻结代码无变化；A联合编译／EditMode exit0，1194/1194；本包111＋64=175条全Passed，旧961多重集合保持，16项1491行、8个GUID唯一。 |

## 冻结清单

| 文件（Core 或 FightMatch EditMode 目录） | 行数 | SHA256 |
| --- | ---: | --- |
| Core/CandidateGrowthDefinition.cs | 177 | 41d4bd2b75e209fa7be65120777caed435d4ddb60494b172581de9d9ac43b1d3 |
| Core/CandidateCharacterState.cs | 94 | 333bbd2ae5f0e08508d3be493402e2cfad55de3556495ad5048bdb54230d8d83 |
| Core/CandidateCharacterGrowth.cs | 117 | 6992bcf387b0434fa86dcfec9aa689b7a3de1cfaa8784210ae2e14ca0abffe21 |
| Core/CandidateCharacterEnd.cs | 123 | b3a056f0f5e1f42938e0fea1a3185c00a2c02d9434753e1671cb6272b568d884 |
| Core/CandidateRecoveryClock.cs | 135 | 86d8920bb392df984fac0d55294fa93fee736a7f81f4a79c3ef4927b724c954a |
| Core/CandidateComputedStats.cs | 31 | 78929d05c7f103c10730dc2e76f1c6f774b5713ac4bd46ae05d721b589358c6a |
| Tests/CandidateCharacterGrowthTests.cs | 372 | a3ccf8647a201218f2efbc97ef43214f4508f33fec0cb00042ec3c42d0d4aaa8 |
| Tests/CandidateRecoveryTests.cs | 354 | 9610afacd630138c0cc677a4c1f13457ff2fae6f1ec80fce389077e25a922f56 |

8份C#共1403行；A用Unity生成8份meta共88行，16份实施文件合计1491≤2200行。
scope.json 保留完整相对路径／SHA；before=331项，规范摘要24f0e94da6104daa0565ea00a46d541d65ab886d4d43676bd7a458f557f3496b。
before已由工具输出且未改写；18个自身新路径（8代码、8meta、报告、scope）开工均不存在。
CODE_READY 静态核对：旧331项变化0；双方白名单以外新增0；D meta 0；Core 无Unity／时间源／浮点或新预算调用。
RUN — git diff --check，exit0；另以原before逐路径SHA核对本任务范围，避免未跟踪文件被git diff漏掉。
已有共享工作区修改保留；A本批5份代码及5份meta按§87单列允许，不当作本包越界。

## Unity生成的8份meta

下表文件均为上方同名C#的.meta，每份11行；SHA和完整路径同时列在scope.after.ownFiles。GUID在全Assets各出现一次。

| 对应C# | GUID | meta SHA256 |
| --- | --- | --- |
| CandidateGrowthDefinition.cs | 1dad6da4caffa1747a7ca87ba0da93fa | 23d49a1dbe3c7a83a913c2c61df34d13b844aae68ad59ea85f01adb1dd5e3f2d |
| CandidateCharacterState.cs | 145e40f2788a8df4983e0cacd02406e9 | c849b0039fc9522f8721e24f3fb547ce4de7b0685c38e629c1a38ba0010c993a |
| CandidateCharacterGrowth.cs | 699b798d9b049b64bb160607f02c2878 | 663b77954c614ac02986850ead6515ec353a76b016f3f33ed258f00c87738a94 |
| CandidateCharacterEnd.cs | fd8a3fd186e19f34ea5a6d4e3ceb09b0 | e7b1be98883f84fdfdfe2b85cee171dd434ba75505e7c6fa58e3bddc0b1a3141 |
| CandidateRecoveryClock.cs | db7bd9a119768c34881740b1657489ca | 3a459586430427d260a5a2cd75199c08f0873bd79004de7b5302736282239c74 |
| CandidateComputedStats.cs | 240e1009f6fd13f468cb0adad6cc7c15 | e6e9e5522235ea6f7320971af0fd4ece4ba519d33f4b5b00d8e36264eaea192e |
| CandidateCharacterGrowthTests.cs | 6186e7cf9336c7546879181f0c917a7a | 1b38e12f7dc5c8434f422c9b1df7cc7ce03704181c367212d5528ae124c7dad7 |
| CandidateRecoveryTests.cs | 480c9c816a1d5894c8fd866644146702 | 0bfcffac376c53a8f52a06e13252942504bc0a5f9b3ceb8203a4ed9d0cce82e9 |

## 本次联合验证与D核验

A任务01a0c1fb-248d-7321-8b69-9efd26b817f1，本次008 turn=01a0c26e-2148-7a91-bf38-4acf4815edfb，是唯一Unity执行者。
D于UTC 2026-09-21T05:48:57.2018467Z标CODE_READY，8代码规范摘要76f911afaf1091beb08a7713774efacfc38ba8d8db3a11420168ba5b488dddce。
CODE_READY当时编译／测试确未运行；其报告原SHA为06fed4ae539841426cecbb79c690a6e025e2b86271214148c7c321ad13b827ea，A的codeFreeze记录该历史就绪依据。
A于05:50:55.0969586Z核齐13份C#，联合规范摘要dec0749e599e2a5e9bebf6e8ecd282ee69bd0251cbed25c18db83ce5cf3446e2。
A确认Unity2022.3.18f1路径与进程排他，用Start-Process -WindowStyle Hidden串行执行，WaitForExit/Refresh取得真实ExitCode；D未运行Unity。
实际命令参数与日志中COMMAND LINE ARGUMENTS逐项一致；测试没有-quit：

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Compile.log') -WindowStyle Hidden -PassThru
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB04-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Tests.log') -WindowStyle Hidden -PassThru
```

| 运行 | UTC起止 | PID | 退出码 |
| --- | --- | ---: | ---: |
| 编译／导入 | 2026-09-21T05:51:26.6966193Z → 05:51:39.2744102Z | 55792 | 0 |
| 全量EditMode | 2026-09-21T05:52:31.9533564Z → 05:52:43.6411037Z | 43052 | 0 |

D读取两份日志，无error CS、Compilation failed或Unhandled Exception；编译日志530行确认return code 0，测试日志335行确认本批XML路径。
XML执行时间05:52:39Z→05:52:42Z，1194/1194 Passed，failed/skipped/inconclusive均0；本包GrowthTests=111、RecoveryTests=64，全175条Passed。
D用StringComparer.Ordinal字典逐条比较原961条（958个不同fullname）的出现次数和当前Passed次数，差异0；233条新增恰为008的58与020的175。
本包175条按Ordinal fullname/TAB/result/LF排序（保留重复名称）摘要：1a45eef7375744ff159edd88025b61bb06dad2a098d3dd960dcc2a02e6653e3c。
旧007B XML仅作回归基线，SHA=9b0d12667df5a7bf3061377874efeb12ba1c694454446976ea50c763a8baf6e9；本次通过证据均来自B04产物。

| 联合产物 | D实际重算SHA256 |
| --- | --- |
| Logs/FMDemoB04Compile.log | 40c467697d85c937292e3b2e1a13e00eb3615be847a3c001166b99279a41bc82 |
| Logs/FMDemoB04Tests.log | a3d4c3f0f8cb05ff12c5fea500db50c7c569f517cc024b04457b99f0843cfb49 |
| FMDemoB04-EditMode.xml | 37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95 |
| demo-008-delivery.md | 1d446b2271f93b55e8a62f667dac4b68dc7f39757362996e06f07892ad9e3c6c |
| demo-008-scope.json | 7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a |

D after于2026-09-21T05:59:53.1365126Z捕获357项，逐项与A的after／测试前清单相同，规范SHA=ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0。
旧331项SHA不变、自身仅16项新增，另允许A的10项；全部8份D源码与CODE_READY相同。scope保留原before/codeFreeze字节前缀，追加完整357项after、自身16项与验证摘要，未复制XML。
本次无失败修正、源码重新冻结或额外Unity运行。作者停在020，等待R按§89独立审查；不执行021/022，不审A代码，不创建任务／子代理。
