# FM-DEMO-008 交付记录

状态：WORKER_RETURNED；008作者实施和DEMO-B04联合验证完成，等待R独立审查；本报告不作ACCEPT结论。
授权：system-task-packets.md r65 §86/87；§88仅用于同批冻结、文件范围及测试证据核对。
A任务：01a0c1fb-248d-7321-8b69-9efd26b817f1；008 turn：01a0c26e-2148-7a91-bf38-4acf4815edfb。
D任务：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；020 turn：01a0c26d-99e8-7d91-8432-136dca6d6155。

## 实施结果与验收

Prepare按根null→材料元信息/长度→映射/三域→007B处理；48字节按Battle、BaseReward、Bonus各16字节小端映射，只在此清t最高位，复用既有PCG及007B。
Binding保存原生成Player/Challenge/Attempt/EntryBaseline、Prepared Context、明确来源能力/映射及三域用途；不保留原可变壳或48字节权威。
EvaluateOpportunity重验完整绑定和当前随机态，核原战士/目标键/ordinal/初态/PRD，复用既有采样器，输出新随机态与只读机会事实，保留全部原始字。
真实机会和当前活目标由009确认；本包成功仅为纯候选计算，不表达攻击或提交成功。015/024后续须整份保留Binding的原生成依据。

| 验收 | 作者核对结果与可失败证据 |
| --- | --- |
| ①SC01逐字节 | PASS；三块零得s₀=6364136223846793006/inc1/w0；00～0f小端a/b、t最高位映射、全ff、相同三块均核。长度0/47/49、缺来源/映射、错映射明确拒绝。 |
| ②域与完整初态 | PASS；四组L1/L3×C上/下候选经006/007A/本入口，逐值比对007B；三域种子42/54、17/19、23/29顺序固定，Battle首字a15c02b7，原生成关联完整，无额外业务抽字。 |
| ③业务机会 | PASS；明示合成C1/4对应原字a15c02b7、7b47f409、ba1d3330，依次失败f1/f2、成功f0；合成C1零取字且不重置已用预算。另核拒绝原始字和耗尽重试，奖励初态不变；这些合成C不是正式20%审定。 |
| ④绑定拒绝 | PASS；跨Attempt/成员/面/敌实例、负ordinal、Initial/PRD参数错配均拒绝且无Next/Fact；错配来自其他合法候选，无反射。连续机会输入不变，值相等参数被接受，后续面键不冒充当前存活证明。 |
| ⑤精确预算/隔离 | PASS；两入口按本次budget重验保留Prepared大数、三域a/b/流、PRD和ordinal；步骤/字预算不足原样Limit，新预算重试一致。修改输入Bytes/材料字段不改变Binding，只读事实集合不可写入。 |
| ⑥范围/真实验证 | PASS；008仅新增下列10项实施文件和2份记录；旧331项逐项SHA保持，联合编译exit0、全量1194/1194通过；原961条fullname多重集合及逐名Passed次数相同，008新增58条，5个新GUID唯一。 |

无平台随机源、Unity业务依赖、Published、H10、奖励办理、恢复器或实际持久化；没有修改旧公共API/源码/测试/asmdef/meta、配置/依赖/权限或其他文档，没有Git写入。
008五份C#已完整回读，5份Unity生成meta逐份核对；源码918行、meta55行，实施合计973≤1400。

## 008完整文件指纹

| 路径 | 行数 | SHA256 | meta GUID |
| --- | ---: | --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateRandomBinding.cs | 113 | 94263e9be5e91d48ff3ca1efb46361f7b8cb5187a0507b13258cdfd1edfcd377 | — |
| Assets/Scripts/FightMatch/Core/CandidateRandomBinding.cs.meta | 11 | 6bf81f9966f30db0df0b41fcaa14927b2ad69618ac4271bbca68db0b497863b2 | 0e847a3d817d5bc42a296e51795a7ac6 |
| Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs | 63 | 22aa5044602fed30dd9b1a02aa8b1ffddebc19f6e762817a2be1dae228704796 | — |
| Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs.meta | 11 | a6158aea7f0d83fa625f9f243df2b03c8f45cf84c0bb5a8d52d5f1614e87a759 | 615d4fa46b56b8f48bd300bb8f5956df |
| Assets/Scripts/FightMatch/Core/CandidateSeedMaterial.cs | 10 | eef4d6f93a8cf4dfb6bfe372702bc363d1bf5052a946231090a7e75d010557a3 | — |
| Assets/Scripts/FightMatch/Core/CandidateSeedMaterial.cs.meta | 11 | 19523465982c9c31ae14f5f45e570221cbf7b8583dc3714d1194ea454411f1df | a7b46f53318f79c43b4a4a8b07802625 |
| Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs | 142 | e73e9be77a30c431eab2c58d95cfa129a94e9895468774371e671dd74be41082 | — |
| Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs.meta | 11 | 706d25c580aa387340c449256ff751d16c7f8eb9cf91f658dd0af7af0184d3fc | b4f857e2c10477841ba1e23f4ae962a6 |
| Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs | 590 | 33af55d2bd6e8cca49dd8064582216dbd821ca892daf4d7dc0610dedcbc10f9e | — |
| Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs.meta | 11 | 0944d96946baf71fe9eac64b09caf684dd1e6b5eebf4586729224acb13a894db | 592be2951369fd649b90e540cb7858c0 |

## 020同版联合验证输入指纹

以下只记录D明确CODE_READY后冻结的输入与Unity自动生成meta，不代表A对020代码作独立审查；020源码1403行、meta88行，合计1491。

| 路径 | 行数 | SHA256 | meta GUID |
| --- | ---: | --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateCharacterEnd.cs | 123 | b3a056f0f5e1f42938e0fea1a3185c00a2c02d9434753e1671cb6272b568d884 | — |
| Assets/Scripts/FightMatch/Core/CandidateCharacterEnd.cs.meta | 11 | e7b1be98883f84fdfdfe2b85cee171dd434ba75505e7c6fa58e3bddc0b1a3141 | fd8a3fd186e19f34ea5a6d4e3ceb09b0 |
| Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs | 117 | 6992bcf387b0434fa86dcfec9aa689b7a3de1cfaa8784210ae2e14ca0abffe21 | — |
| Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs.meta | 11 | 663b77954c614ac02986850ead6515ec353a76b016f3f33ed258f00c87738a94 | 699b798d9b049b64bb160607f02c2878 |
| Assets/Scripts/FightMatch/Core/CandidateCharacterState.cs | 94 | 333bbd2ae5f0e08508d3be493402e2cfad55de3556495ad5048bdb54230d8d83 | — |
| Assets/Scripts/FightMatch/Core/CandidateCharacterState.cs.meta | 11 | c849b0039fc9522f8721e24f3fb547ce4de7b0685c38e629c1a38ba0010c993a | 145e40f2788a8df4983e0cacd02406e9 |
| Assets/Scripts/FightMatch/Core/CandidateComputedStats.cs | 31 | 78929d05c7f103c10730dc2e76f1c6f774b5713ac4bd46ae05d721b589358c6a | — |
| Assets/Scripts/FightMatch/Core/CandidateComputedStats.cs.meta | 11 | e6e9e5522235ea6f7320971af0fd4ece4ba519d33f4b5b00d8e36264eaea192e | 240e1009f6fd13f468cb0adad6cc7c15 |
| Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs | 177 | 41d4bd2b75e209fa7be65120777caed435d4ddb60494b172581de9d9ac43b1d3 | — |
| Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs.meta | 11 | 23d49a1dbe3c7a83a913c2c61df34d13b844aae68ad59ea85f01adb1dd5e3f2d | 1dad6da4caffa1747a7ca87ba0da93fa |
| Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs | 135 | 86d8920bb392df984fac0d55294fa93fee736a7f81f4a79c3ef4927b724c954a | — |
| Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs.meta | 11 | 3a459586430427d260a5a2cd75199c08f0873bd79004de7b5302736282239c74 | db7bd9a119768c34881740b1657489ca |
| Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs | 372 | a3ccf8647a201218f2efbc97ef43214f4508f33fec0cb00042ec3c42d0d4aaa8 | — |
| Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs.meta | 11 | 1b38e12f7dc5c8434f422c9b1df7cc7ce03704181c367212d5528ae124c7dad7 | 6186e7cf9336c7546879181f0c917a7a |
| Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs | 354 | 9610afacd630138c0cc677a4c1f13457ff2fae6f1ec80fce389077e25a922f56 | — |
| Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs.meta | 11 | 0bfcffac376c53a8f52a06e13252942504bc0a5f9b3ceb8203a4ed9d0cce82e9 | 480c9c816a1d5894c8fd866644146702 |

13份新增meta均由本次Unity导入生成，每个GUID在Assets仅出现一次；没有手写或更改D代码/meta。

## 冻结、范围与基线

开工before时间UTC 2026-09-21T05:29:17.4967456Z：原331项路径/SHA与007B完全一致；008全部12个新路径不存在，并列明可并行出现的020文件。
331项规范路径/TAB/SHA/LF摘要：24f0e94da6104daa0565ea00a46d541d65ab886d4d43676bd7a458f557f3496b。
原scope全文SHA：c08109a94262c745477c7dd337f9d3399e6fdd4b730b23bee493c0775ec0e295；原before段字节保持，移除新增codeFreeze及之后字段还原的SHA与之相同。
A五份C#冻结UTC 2026-09-21T05:41:03.7281315Z；D明确CODE_READY并冻结后，A逐份核其8份声明SHA。
联合冻结UTC 2026-09-21T05:50:55.0969586Z；13份C#规范摘要：dec0749e599e2a5e9bebf6e8ecd282ee69bd0251cbed25c18db83ce5cf3446e2。
导入前344项（旧331＋新13份C#）摘要：9f9897e0ae251338faab582d35b702ee51147d1dc7a74eeeaf1e8b536256bc81。
测试前及最终357项（旧331＋008的10＋020的16）摘要均为：ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0。
最终after时间UTC 2026-09-21T05:54:56.6929101Z；旧331项全部保持，只有本批列明26项新增；冻结后源码没有变化。
demo-008-scope.json保存完整before/codeFreeze/validationBefore/after清单、作者归属、运行与XML摘要，不复制整份XML。
scope大小194804字节≤300KiB；最终SHA：7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a。
已有tracked差异34文件、+2809/-597（包括用户权限文件和旧FlowPuzzle/Packages差异）为进入本包前内容，未纳入008；只读git diff --check exit0，无空白错误；Git仅提示这些既有文件下次写入时LF将转CRLF，本包未执行Git写入。

## 唯一一次联合验证

Unity固定为2022.3.18f1。双方冻结后确认进程排他，再以隐藏进程串行启动；没有终止用户进程。为许可证IPC按既有授权在沙箱外运行。
两次调用均WaitForExit后Refresh取得真实ExitCode；编译结束并再次确认进程排他后才启动测试。测试没有-quit；运行后Unity进程数0。
下列为实际启动参数；日志/XML均由Unity生成，未手写。

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Compile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB04-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Tests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```

| 运行 | UTC起止 | PID | 实际退出码 |
| --- | --- | ---: | ---: |
| 编译/导入 | 2026-09-21T05:51:26.6966193Z → 2026-09-21T05:51:39.2744102Z | 55792 | 0 |
| 全量EditMode | 2026-09-21T05:52:31.9533564Z → 2026-09-21T05:52:43.6411037Z | 43052 | 0 |

XML用例执行UTC 2026-09-21 05:52:39Z → 2026-09-21 05:52:42Z；总计1194、Passed1194、Failed0、Skipped0、Inconclusive0，编译错误/警告0。
原961条（958个Ordinal不同fullname）逐名核出现次数及Passed次数，无缺失/改名/失败；旧集和当前旧子集fullname/TAB/count/LF摘要均为a3d4d9f300cfd29699d623cf4bdca344ef0a8300ae688aa9fd86da9258892207。
新增008 CandidateRandomBindingTests：58；新增020 CandidateCharacterGrowthTests：111、CandidateRecoveryTests：64，020共175；无其他新增测试。
逐例结果以XML为准；以上结果对应相同冻结代码和357项清单，成功后没有改源码或追加运行。

| Unity产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB04Compile.log | 40c467697d85c937292e3b2e1a13e00eb3615be847a3c001166b99279a41bc82 |
| Logs/FMDemoB04Tests.log | a3d4c3f0f8cb05ff12c5fea500db50c7c569f517cc024b04457b99f0843cfb49 |
| FMDemoB04-EditMode.xml | 37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95 |

## 只读输入指纹

以下为UTC 2026-09-21T05:46:15.2034251Z核对的输入版本；协调稿后续由SD00管理，A没有修改。

| 输入 | SHA256 |
| --- | --- |
| docs/system-design/2026-09-17/system-task-packets.md | 51a11778be86a1eb204941d87f246f98707b1b6a4b0f04b291390c5a192cc339 |
| docs/system-design/2026-09-17/demo-007b-delivery.md | cbccfecb259c49f13d97580e5e6ee5466679617bbc75b42339c19674c7ac0a22 |
| docs/system-design/2026-09-17/demo-007b-scope.json | 277bb5e05b48bc334e8d2f2a8a93d323a07ed5bd6d2ecddeface7cbd1a5ac177 |
| docs/system-design/2026-09-17/demo-batch03-code-review.md | 0a1fd6b6ae06699db4777b2fb77c178fabb951e0ac923227ab39d47a0a2093da |
| FMDemo007B-EditMode.xml | 9b0d12667df5a7bf3061377874efeb12ba1c694454446976ea50c763a8baf6e9 |
| docs/system-design/2026-09-16/interaction-contracts.md | eab83027c412619731df1f7f3fd1650161915d96e31d68ea30f4af4af9f96f06 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |

作者停在008；将本报告、scope及同版联合证据发给D，由D只读核其175条和SHA后补齐020记录。R须等待两位作者本次turn完成及报告齐全再逐包独立审查。本包不进入009/011，不创建任务/子代理。
