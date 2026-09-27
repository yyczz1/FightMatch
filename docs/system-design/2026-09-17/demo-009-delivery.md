# FM-DEMO-009 交付记录

状态：WORKER_RETURNED；009实现及B05联合编译／全量EditMode验证完成，等待R独立审查。
任务A：01a0c1fb-248d-7321-8b69-9efd26b817f1；本次turn：01a0c2bc-6642-7933-b942-bac235a5f11d。
任务D：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；021本次turn：01a0c2bd-1233-76d2-bed5-4d888f50817d。
依据：system-task-packets.md r67 §93/94；§95仅用于D冻结／联合证据。本文为作者交回，非独立ACCEPT。
范围：仅旧BattleSnapshot.cs授权新增internal完整值构造；新增4份Core、1份测试及其5份Unity meta。D源码由D实施、冻结，A未审改其业务。

实现：Evaluate按缺项／同一预算数值重验、基线与身份、阶段及完整当前键／HP、001几何、原槽活敌射程、008真实机会、精确伤害顺序求值。
伤害只在Attack×真实暴击倍率×100/(100+原物防)后floor；保留q、实际HP损失a、溢出o、前后HP及原Crit来源。
Frame复制请求与集合，仅更新目标HP、008随机／PRD、攻击者实际伤害累计；保留原Snapshot、成员及其他敌人／游标／承伤。
当前能力域的格挡／盾实际量显式为0；不实现敌方阶段、固化／翻面、胜利推进、完整快照提交、奖励或完整求解。
CoreTestAccess仅授权FightMatch.Core.Tests；新完整值构造均internal，原初始化行为、public属性／枚举与旧meta保持。
测试C为明确合成参数；本包不声称完成正式C校准。

| 验收 | 已通过的可失败见证 | 新用例数 |
| --- | --- | ---: |
| ①固定源攻击 | SourceCandidates：006→007A→008的L1/L3×两坐标；真实首字a15c02b7失败，q20/a15/o5及q16/HP余4，原Pair／Actor／来源保持。 | 4 |
| ②真实暴击／精确零 | CertainCrit、FloorZero、DamageFloors、FractionalCurrentHp：C1倍率3/2得25且0字；0/1攻击对防500仍真实机会；15/4→25/8最终floor3；HP5/2得a5/2、o35/2。 | 6 |
| ③射程与状态守门 | LivingOriginalSlotOrder及身份／阶段／死者／跨候选／后续面／完整当前状态／随机绑定：原槽跳空且Pair反序、StableOrder相反；先真实击杀前排，再明确旧固化／承伤局面，后排可达；无效输入零抽字。 | 46 |
| ④几何与原子性 | Geometry八类原001原因／CellIndex及ALocalLegalRoute：越界／斜线／自交／他对端点／重叠等精确拒绝，当前合法但隔断剩余Pair的路线仍可攻击；输入HP／累计／PRD保持。 | 9 |
| ⑤片段与来源 | RealSecondAttack：真实次字7b47f409，累计16+4=20、承伤5保持；成员HP95、意图游标、旧路线、原槽保持；击杀末敌仍保留旧阶段／修订／Board，无完成态输出。 | 1 |
| ⑥预算／隔离／验证 | NullRoots4、MissingField7、保留定义大值16、当前大值10、共享／后段Limit及重试1、已用字预算与必暴1、请求／构造列表复制及只读图1；均已通过。旧1194与范围验证见下。 | 40 |

原始BattleSnapshot.cs修改前6081字节完整base64保存在demo-009-scope.json.modifiedOriginalFiles[0]。
原SHA：f4583c3c6509800557bce1b96b74cbcb6b21da85dfe3ffe506994be98ef825bc；解码回算与B04原清单一致。
交回BattleSnapshot.cs共211行、+57/-0；SHA：f511f0801b66f24da81f34f01dbde5d47961da139cc006ca508b3cf3139d2cbb。
逐行核全部原行保留，仅增加五个internal构造；列表复制只读，CarryMode保持Empty，旧构造／属性／枚举未变。
009新C#1084行＋5 meta 55行＋旧文件新增57行，总新增＋删除1196≤1900。D新10项1377行仅作联合范围计数。

before继承已接收B04原357项／原时间2026-09-21T05:54:56.6929101Z，规范SHA：ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0。
B04整scope SHA：7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a。
A实际startedSnapshot为2026-09-21T06:55:51.4900728Z：357项逐SHA一致、自身12新路径不存在、D新路径未出现，随后才修改代码。
最初scope保存原字节后SHA：ac7f4a6500230fcc3d5a92321038e7b51130d7560d11f7a942e9d0bff2b0af18；现scope截取authorCodeFreeze前原前缀补闭括号可还原同SHA，未重写before或原字节。
A6源码冻结UTC 07:16:20.5968046；D明确CODE_READY UTC 07:19:50.1795326；11源码联合冻结UTC 07:22:55.2286664（均2026-09-21）。
11份冻结C#规范SHA：f564d424005fd26e8bb8a889897399e1d46db0f0139880656c6ce972cae16445。
导入前367项规范SHA：ebfdb3b7b198586289f01f7a8746da9787a0ae26fedd3e0ac25071fa1588c15f。
Unity导入后／测试前／测试后377项规范SHA均为：8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307。
after捕获UTC 2026-09-21T07:27:30.2472227Z；除授权BattleSnapshot.cs外356旧项SHA保持，20个新增路径准确，10个新GUID在全Assets中各出现一次。
最终scope 189634字节≤300KiB；SHA：a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec。
规范清单按Ordinal路径、path+TAB+sha256+LF、UTF-8无BOM计算；完整三份文件清单及前后证据保存在scope。
scope内readinessReports是冻结时CODE_READY报告的历史指纹，最终交付报告会更新；不将其当最终报告SHA。

| 归属 | 新实施文件 | 行数 | SHA256 | Unity生成GUID |
| --- | --- | ---: | --- | --- |
| 009 | Assets/Scripts/FightMatch/Core/BattleDamageFact.cs | 65 | 7fe4b33b8999cc63512050c155c757274cb12c7414db165dbff9c4cad1da46d5 | — |
| 009 | Assets/Scripts/FightMatch/Core/BattleDamageFact.cs.meta | 11 | 94d76cbb5cfb76fb2e4b8557a2b7e77fbc1d87dc482b3d4280b70ba4170b1be3 | 6cb712d591df9fc4788162f30691663d |
| 009 | Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs | 98 | 5dd15c88fe760679848d5689630a62a8965c2b7eb7341ec678c97440a5efca42 | — |
| 009 | Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs.meta | 11 | 544171f680ed16205569148a854e79fa40561a389dc7504c5f08ac969b82a978 | 1261659daeeebf642a9ce2fdb192aeb2 |
| 009 | Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs | 239 | 05fdda41bd6a3a8e1f936e1eea3a4ee9d0a826b27c845238224105e91b1efb5d | — |
| 009 | Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs.meta | 11 | db149e9f5d6c4c91a54d1f6c0b72a341c9837692f14c5d745cd44d0ac0d69d22 | e258bc1a984513747b3f2f50d7369595 |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventory.cs | 323 | 75cae49f49283505b8612503a537563c4f0903a067a4dae184aa0b096c8726ef | — |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventory.cs.meta | 11 | 469eeaa0373372ce519b023b0ec6cd0d0cefcd33bc3920a809ae2396becbea13 | 827c5a1225e59be40b1eeebb6d488571 |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs | 154 | 12f003facb2c7907c3122615203c22a2e5683013928bb0ddfe116df78b89900e | — |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs.meta | 11 | 0e29966cd1a58cb1ef386f03a999a5aa725c1867f07b15797da7aef79671fb08 | 605d1db34c922c949b9f12590ab25413 |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs | 75 | 75bc2686a425c560def6671c59ed7043fbf2b0d1edee8928ad587edb9966af0b | — |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs.meta | 11 | a3889fa4b90569d03138498686725b422e3e49b4d8e7d7f894609a454699fa71 | edbde89d738ee2846963ddd81777c6d1 |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs | 169 | 2c20314e5a4909940a67e89c27dec4856f1851a6b65f2012086e9e7b878646be | — |
| 021 | Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs.meta | 11 | 7afe1d266cabe832cdf8d5f0d8e3d726963c358523908fdbfa9ef959b53654a7 | 4a716ecee51a4d24b80795cdeb52330a |
| 009 | Assets/Scripts/FightMatch/Core/CoreTestAccess.cs | 3 | e325eef6629b360e5e621991e6d64fbad7bb560f3c426471b61cf5ac3c84b39f | — |
| 009 | Assets/Scripts/FightMatch/Core/CoreTestAccess.cs.meta | 11 | 7a2cef447ed0dab563da0a114c9cf7667158a33aaad4f8ad9b154d233387d166 | 0e8f2361ac18c3f43b4d185f31a0e187 |
| 009 | Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs | 679 | fac0e9e639dff3e7302fe44295dbdc8e25a192a05be34a0e9f617f7292f60a3c | — |
| 009 | Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs.meta | 11 | 28b00280930443a43c08ece21f14213f43f4f7c7ae631c4c782be6cd4c573449 | 22d25a0f782dacf4ea529b2750c290b1 |
| 021 | Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs | 601 | 9a6f2f623b6ac0aa3a6172e22e7c3c3a5d45276da706206b9199561eabb2da83 | — |
| 021 | Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs.meta | 11 | c4d97d7e3d75d7c82f7bb2806843e601ab2690bb19dbe1ec18d4906a374fd167 | f4403cbd9d3118241ad05915a8f50f98 |

两次运行前均核固定Unity 2022.3.18f1路径及本项目进程排他；未杀进程。D未运行Unity。
实际启动使用Start-Process -WindowStyle Hidden -PassThru，经WaitForExit、Refresh读取实际ExitCode；命令如下：
```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB05Compile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB05-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB05Tests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```
编译PID18684：UTC 2026-09-21T07:23:26.6312460Z→07:23:39.4787147Z，实际exit 0；测试PID50064：07:24:40.5365076Z→07:24:54.9065009Z，实际exit 0。
本批各运行一次；测试无-quit。编译／测试日志C# error／warning共0，验证后Unity进程0；冻结11份C#始终未变。
仅Unity生成下表三份验证产物及双方10份新meta，未手写meta、日志或XML。

| 验证产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB05Compile.log | 0e3bfc52b6df82b8acd2e26313ad640eed713b49292704ba12215c733e8fe5fb |
| Logs/FMDemoB05Tests.log | 4387bb8d5749a60f2161fb0a46da446f4a8a3ade7f7ca6e9e6b422ef88fa253b |
| FMDemoB05-EditMode.xml | 6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0 |

XML：1414/1414 Passed；failed/skipped/inconclusive均0，XML执行UTC 07:24:47Z→07:24:53Z。
原1194条、1189不同Ordinal fullname：每名出现次数及Passed次数与B04完全一致；新增009=106、021=114，无其他新增fixture／用例。
原及本次旧子集fullname+TAB+count+LF规范SHA均为f7a4f366d19e0bae3ca4c6a0f347e805abad4df7640440bb300a070e1e25fbe2。
009全部23测试方法及参数化计数记录在scope.validation.xmlSummary.new009Methods；完整逐例结果只在原XML，不拷入报告。
D的114条通过数来自联合XML，业务验收对应由D自行交回；A仅记录冻结、执行与文件证据。

下列只读输入于UTC 2026-09-21T07:17:52.6890081Z记录；协调稿后续状态由SD00维护，业务代码冻结独立核验。
| 输入 | SHA256 |
| --- | --- |
| AGENTS.md | c1a836ff14cc0834e16c33cbc49d53bec82cadff656a329c24c72f569d460528 |
| .agent/PROJECT_CONTEXT.md | 4cf1e141e39f12165103e52e44357b1c179175bd18d2c874f386f0360abfc9a9 |
| .agent/CODING_RULES.md | 35dcf6b76ec660509414580ba160dff080dc76d2450293b59a79a7cc91f91f9f |
| .agent/PLANS.md | 5039f9bc8a51eafac968b652b09906b90c6f335395dc3afec3899fb83a7324eb |
| .agent/VALIDATION.md | b65114c2444dee680741e52307fec57cc0abc4b1196767962bc8e4df322690a0 |
| .agent/REVIEW_CHECKLIST.md | b5c39a7c85d2fd41182ac4ad3db30cf7ed28e0d9a696354f4cfe5a507e57c8a7 |
| docs/system-design/2026-09-17/system-task-packets.md | 3eb643db88b4f406b50de7db09a6b157d64323fefa68034989f99f6f41417e1c |
| docs/system-design/2026-09-17/demo-008-delivery.md | 1d446b2271f93b55e8a62f667dac4b68dc7f39757362996e06f07892ad9e3c6c |
| docs/system-design/2026-09-17/demo-008-scope.json | 7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a |
| docs/system-design/2026-09-17/demo-020-delivery.md | 409dd4f7a3f300bc264d25a714439dd6e4294eee1cd2865f3826daef7834a7c7 |
| docs/system-design/2026-09-17/demo-020-scope.json | 9efd8a33f201e55604aaad5f762d9052d5795ee70d6c59746c6ebbd2a5c9f1d6 |
| docs/system-design/2026-09-17/demo-batch04-code-review.md | fc1facfd9ba0d30fa24afc02533a071e7191f24cad1ec3ab24d9558726fa56c5 |
| FMDemoB04-EditMode.xml | 37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95 |
| docs/system-design/2026-09-16/interaction-contracts.md | eab83027c412619731df1f7f3fd1650161915d96e31d68ea30f4af4af9f96f06 |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/battle-effects.md | 3cfd66243a2846cf5ace15bf21f88e7d6d8180481f44695cc2b588433bc3065d |
| docs/system-design/2026-09-16/details/contribution-scoring.md | 5194dc775eeb2bf359604eb1966a20a51062c7c48bfedfce5c15601ec1931912 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| docs/game-design/2026-09-14-fightmatch-game-design.md | 5495ae36bc1e03cb42ffd428416d2c36855370935dfda5769ede28317393e82a |
| docs/game-design/2026-09-15-three-group-recommendations.md | 1c86e32869355ad9ead54f333ef3df8ffb3238cee658efc92c2f37979d93f223 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |

git -c core.safecrlf=false diff --check退出0；既有tracked差异摘要34 files changed, 2809 insertions(+), 597 deletions(-)保持。
既有FlowPuzzle／Packages／用户本地权限差异不属于本包；未写权限、配置、asmdef、依赖、其他源码／文档，未Git写入、创建任务／子代理／分支／worktree。
本包可独立回滚：由审批者按scope原字节恢复BattleSnapshot.cs并删除009明确新增路径；不执行回滚，不触碰D或原有差异。
交回D联合证据供其完成021报告，再由R等待两作者本次turn completed后分别独立审查；A停在009，不启动后包。
