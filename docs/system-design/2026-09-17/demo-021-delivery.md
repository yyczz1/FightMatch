# FM-DEMO-021 普通库存与候选冻结生命周期

状态：WORKER_RETURNED；A联合验证成功，D已独立核证并完整交回，等待R独立审查。本文不是独立审查结论。
任务：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；本次turn：01a0c2bd-1233-76d2-bed5-4d888f50817d。
授权：system-task-packets.md §93／95；同批A执行009，R按§96独立审查。

## 范围与实现

只新建四份Core、一份EditMode及本报告／scope。五份meta由A通过Unity生成；D未启动Unity或手写meta。
公开入口集中CandidateInventory：PrepareDefinition、CreateCandidate、Read、GrantOrdinary、Equip、SetPreference、Freeze、End。
定义使用007A既有公开Context输入域；Actor限一名Warrior、原槽0～2，不读取或修改M03成长。
库存仅同一Player／Item的普通聚合池，T独立保存，L/R/F与满叠／余量按精确整数派生。
Grant保留普通基础奖励的完整规范向量收讫；零／空向量也留据，重复Settlement回当前state与原收讫，先于过时修订检查。
选择／显式开关与数量各循既定修订；有活动冻结时含Empty均禁止换装，偏好仍可明确设置。
Freeze保留Attempt／Baseline／Context／原参战槽／普通池，C=min(99,L+F)，迟到奖励只增加F。
End完整核U覆盖：胜利扣C−U并接固定奖励；退出原T不变并L=C；重来原C／来源／Baseline移交新Attempt且L0。
结束、固定奖励和所有历史依据随同一个不可变候选返回；同事实重复不增量，冲突／Limit不返回部分Next。
所有输出集合只读；输入壳复制。每次使用调用者同一个ExactMathBudget重验保留数量、Context／状态／偏好修订和全部历史数值。
未新增依赖、程序集、配置、权限、序列化、服务抽象或Git操作；测试未使用本批A的friend／internal入口或攻击能力。

## 七项验收见证

以下行为见证均已通过同版B05 EditMode；CandidateInventoryTests共114/114 Passed。scope.independentVerification保留35组方法的展开数量及通过数。

| 验收 | CandidateInventoryTests中的行为见证 |
| --- | --- |
| ①数量与固定来源 | NewCandidateIsEmpty、OrdinaryGrantKeepsFixedSource、FreshRequestWithSameSettlement、SameSettlementChangedFacts、TotalsBeyondDoubleIntegerRange：T0／5、重复当前态、改来源事实冲突、空／零留据、超2^53精确及F117=1叠余18。 |
| ②装配／偏好 | EquipReleasesOldAllocation、SelectionAtZeroClear、EquipLimitsAndOptionalClear、EquipRejectsUnsupported：换物释放、池不足原态、材质／职业限制、零选择／明确清除、false重复、独立修订与空冻结禁换。 |
| ③入场与Empty | FreezeBuildsBoundedCarry、EmptyFreezeHasIdentity、FreezeRejectsConflictingIdentity、ActiveGrantOnlyAddsFree、IsolatedNonEmptyCarryDoesNotPassExistingBattleEntryGate：C99／5／0、空冻结身份、原计划重试、迟到奖励只入F、007A仍拒绝NonEmpty。 |
| ④三种结束 | NormalEndConservesCurrentTotals、RestartTransfersOriginalC、RestartReceiptRejectsChangedTarget、ZeroCarryRetainsSelection：胜利3／另奖7、退出5／迟到9、重来105中仍只冻结5、偏好保持、当前态重复0增量。 |
| ⑤覆盖与原子性 | EndRequiresExactRemainingCoverage、CompletedEndCannotBeRewritten、EndReceiptIdentity、AlreadyGrantedSettlement、DepletedPoolRetainsGrantReceipt：U缺／重／多／越界、身份／模式／奖励冲突、已含奖励不再加、余额耗尽仍留原收讫。 |
| ⑥隔离／预算 | MutableInputsAreDeeplyIsolated、EveryPublicEntryRechecksRetainedLargeContextAndQuantities、IntegerOverflowAndLateStepFailure、OneSharedStepBudgetCoversEachOperation、RootNullsThrow：壳修改隔离、全只读、小新预算、历史256但T0仍Limit、加法溢出与末步不足后重试一致。 |
| ⑦联合验证 | D先冻结5份，A核11份后统一验证；D独立核原1194完整多重集合／Passed、新库存114、新攻击106、11份冻结SHA和377项范围；10个新GUID全局唯一，其余356旧项保持。 |

## 基线与冻结

scope.before继承已接收B04的demo-008-scope.json.after.files原357项／原时间，非本次D现场。
原scope SHA：7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a。
原清单规范SHA：ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0。
D实际startedSnapshot：2026-09-21T06:56:16.1804978Z，357项与原清单相同；原before／startedSnapshot保持不回写。
CODE_READY：2026-09-21T07:19:50.1795326Z；五份C#共1322行；规范SHA：08a2ce72db54aa01813ac4c32e5f1dfbb008b069efe4ed90700d2d5b69f554c7。
冻结现场367项；仅A获准的BattleSnapshot.cs旧SHA变化，其余356项保持，新增为A／D各5份C#，无越界差异。
A联合冻结：2026-09-21T07:22:55.2286664Z，11份规范SHA：f564d424005fd26e8bb8a889897399e1d46db0f0139880656c6ce972cae16445。
D于2026-09-21T07:24:07.0227449Z逐份核这11项；scope.validationBefore引用A原367项验证前时间／指纹，不冒称D重抓。
D最终after：2026-09-21T07:28:26.7697709Z，共377项，规范SHA：8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307。
新增为A／D各10项；仅A获准旧BattleSnapshot.cs从f4583c3c6509800557bce1b96b74cbcb6b21da85dfe3ffe506994be98ef825bc变为f511f0801b66f24da81f34f01dbde5d47961da139cc006ca508b3cf3139d2cbb。其他356项／旧meta保持。

| 新C# | SHA256 | 行数 |
| --- | --- | ---: |
| Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs | 12f003facb2c7907c3122615203c22a2e5683013928bb0ddfe116df78b89900e | 154 |
| Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs | 2c20314e5a4909940a67e89c27dec4856f1851a6b65f2012086e9e7b878646be | 169 |
| Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs | 75bc2686a425c560def6671c59ed7043fbf2b0d1edee8928ad587edb9966af0b | 75 |
| Assets/Scripts/FightMatch/Core/CandidateInventory.cs | 75cae49f49283505b8612503a537563c4f0903a067a4dae184aa0b096c8726ef | 323 |
| Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs | 9a6f2f623b6ac0aa3a6172e22e7c3c3a5d45276da706206b9199561eabb2da83 | 601 |

五份C#1322行＋五份Unity生成meta55行＝1377≤2200行。以下每个GUID在全部Assets meta中仅出现一次；meta完整路径为对应C#路径加.meta。

| meta所属C# | GUID | meta SHA256 |
| --- | --- | --- |
| CandidateInventoryDefinition.cs | 605d1db34c922c949b9f12590ab25413 | 0e29966cd1a58cb1ef386f03a999a5aa725c1867f07b15797da7aef79671fb08 |
| CandidateInventoryState.cs | 4a716ecee51a4d24b80795cdeb52330a | 7afe1d266cabe832cdf8d5f0d8e3d726963c358523908fdbfa9ef959b53654a7 |
| CandidateInventoryRequests.cs | edbde89d738ee2846963ddd81777c6d1 | a3889fa4b90569d03138498686725b422e3e49b4d8e7d7f894609a454699fa71 |
| CandidateInventory.cs | 827c5a1225e59be40b1eeebb6d488571 | 469eeaa0373372ce519b023b0ec6cd0d0cefcd33bc3920a809ae2396becbea13 |
| CandidateInventoryTests.cs | f4403cbd9d3118241ad05915a8f50f98 | c4d97d7e3d75d7c82f7bb2806843e601ab2690bb19dbe1ec18d4906a374fd167 |

## 验证与边界

静态逐项核对§95；Core未出现UnityEngine／UnityEditor、GrowthChecks、子预算、时间、float／double、本批攻击API引用。
只读git diff --check退出0；仓库本来存在既有未提交修改，不据Git工作区“脏”状态判断本包污染，以上述B04精确SHA清单为准。
A使用固定Unity 2022.3.18f1，Start-Process Hidden并WaitForExit／Refresh后取实际ExitCode；D从009 scope读取回执并独立对上日志／XML／11份源码及377项输入。过程收据保存于scope.jointValidationEvidence。
编译PID18684，UTC 07:23:26.631246Z至07:23:39.4787147Z，ExitCode=0；测试PID50064，UTC 07:24:40.5365076Z至07:24:54.9065009Z，ExitCode=0。日期均2026-09-21；编译在联合冻结之后，测试在编译退出之后。

```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB05Compile.log'
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemoB05-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemoB05Tests.log'
```

以上为A实际参数对应的等价命令展示，D未执行。两次均一次成功，冻结后没有源码修正或补跑；五份meta全部由同次Unity导入产生。
本批XML运行UTC 2026-09-21 07:24:47Z至07:24:53Z；1414总数＝原1194＋009新106＋021新114，均Passed；Failed／Skipped／Inconclusive均0。
D按Ordinal fullname逐项比较B04的1194条（1189不同名称）及其Passed次数，全等；并非只比总数，未出现其他新增fixture。
旧XML SHA：37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95。
旧集合规范SHA：6c7df5fb5ece94b06042a98c7a60999317b606a6a1593389ccedf533db6579e6；编码为Ordinal排序的fullname、重复次数、Passed次数，以TAB分隔、LF结尾、UTF8无BOM。

| A工具生成产物 | SHA256 |
| --- | --- |
| Logs/FMDemoB05Compile.log | 0e3bfc52b6df82b8acd2e26313ad640eed713b49292704ba12215c733e8fe5fb |
| Logs/FMDemoB05Tests.log | 4387bb8d5749a60f2161fb0a46da446f4a8a3ade7f7ca6e9e6b422ef88fa253b |
| FMDemoB05-EditMode.xml | 6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0 |

两日志初始有Licensing Client签名Code10／access token提示，随后许可证与entitlements成功；无error CS或脚本编译失败。本报告不把日志称作“零error文本”。
这只是隔离M04普通来源候选；Ready资格由后续M02核M03，非空携带尚不能进入当前M06。
没有真实道具效果、广告／导入／混合来源、云合并、存档恢复或H10持久提交；候选成功不代表实际完成。
本包作者实现／验证／交付已完毕；仅待R独立审查，无新增用户决策。完整before／startedSnapshot／codeFreeze／验证前后／after与逐文件归属均在demo-021-scope.json。
