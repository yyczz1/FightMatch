# DEMO-B09 独立代码审查

日期：2026-09-21；审查人：R（gpt-6-astra／max，原项目 local）。
R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次原 turn：01a0c3c5-67a3-7c10-94c7-f919bf9761a8。
依据：r74 §114～118、.agent/REVIEW_CHECKLIST.md、批准的历史／结算／评分契约及两包实际补丁、原始执行记录。
R逐份阅读9份新C#及9份meta，分别检查规范与规格；未运行Unity或项目测试，未写实施文件、旧文档、配置、Git或验证产物。

## 1. 独立结论

| 任务包 | 规范检查 | 规格检查 | 唯一结论 |
| --- | --- | --- | --- |
| FM-DEMO-013：完整历史、回退范围与旧操作关系 | 范围、纯Core、不可变输出、共享数值预算及联合验证规则符合 | 七项验收均有同版代码与已执行测试依据，未发现需修正问题 | **ACCEPT** |
| FM-DEMO-023：正常封闭报告到固定基础奖励 | 范围、职责分离、精确数值、来源保留及联合验证规则符合 | 七项验收均有同版代码与已执行测试依据，未发现需修正问题 | **ACCEPT** |

无返修项，无必要用户动作。两包独立结论齐备；B09状态收口由SD00处理，R不修改协调稿。
结论限定当前Empty／单Warrior／E01、E02候选域；不代表014／015／024、持久提交、Player或生产内容已验证。

## 2. 门槛与交付身份

- 013原实施task：01a0c3c3-e163-7f01-b639-ca1f16e1a99d；turn：01a0c3c3-e3da-7c01-aa5c-53c31c6ba000。
- 023原实施task：01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e；turn：01a0c3c4-b9ad-7b22-9b89-880f818d88d8。
- 两份正式完整交付、双方scope及B共同验证消息均收到后，工具分别确认上述原turn于12:20:35Z、12:21:50Z completed。
- R于12:22:16Z打开正式审查门槛；此前只备审契约、旧基线和起始清单，没有提前读取两包新实现。
- 下表SHA均由R直接重算，和正式交回一致；报告、scope均在指定上限内。

| 正式交付 | 大小 | SHA256 |
| --- | ---: | --- |
| demo-013-delivery.md | 113行／13155字节 | 79e10dffd04a8a027df707b732eef0ba1211d9438f1f802409295ced2d76b1a2 |
| demo-013-scope.json | 261367字节≤327680 | d110349421486511971eb1a6c49b6adce38b9685f62e03db097643d4c9a136fb |
| demo-023-delivery.md | 101行／11634字节 | e6082df2327a3e9adc7c1349b82ed067473009abd2ba65823c32a1dbccc70d7e |
| demo-023-scope.json | 300956字节≤327680 | 601e4ab8ce89a42428bb815107e250c3e6fb4efc2298d70bd77c4cb8a55a3953 |

以上四文件位于D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/。
作者报告用于定位证据；结论基于R检查的实际文件、原始任务记录和运行产物。

## 3. 规范与范围检查

R于11:42:36.7977232Z独立保存415项旧基线；原来源为demo-012-scope.json.after.files。
原after时间2026-09-21T11:10:55.8788837Z和来源scope SHA均保持，未把当次观察时间冒充继承时间。
来源scope SHA：f14c371e5468e99451327e9bb573107e1b52e4334c767d8d2859a98a52cb2ff7。
415项规范SHA：3efae32d1ec770ef3b1ae2410330ad579225d78181f8aa263251e4deae3ef4b2。
规范均为Ordinal相对路径＋TAB＋小写SHA256＋LF，含末尾LF，UTF-8无BOM。

013真实startedSnapshot为11:43:46.9631845Z，023为11:43:06.5300398Z。
R在作者仍实施时保存起始scope SHA，并重新计算两份before：均415项、逐项差异0、原时间／来源SHA保持。
最终433项恰为415旧项＋013八项＋023十项；旧SHA差异0，越界新增0，缺白名单项0。
最终433项规范SHA：2f695856ae72d8d7a8582d6b51e3480ded3913dbd006970c052180bf8c2c851e。

| 包 | Core文件 | 测试文件 | C#行数 | meta行数 | 合计／上限 |
| --- | --- | --- | ---: | ---: | ---: |
| 013 | CandidateBattleHistory.cs；CandidateHistoryRequests.cs；CandidateHistoryOperations.cs | CandidateHistoryOperationsTests.cs | 1206 | 44 | 1250／2100 |
| 023 | CandidateRewardDefinition.cs；CandidateRewardState.cs；CandidateRewardRequests.cs；CandidateBaseRewards.cs | CandidateBaseRewardsTests.cs | 1365 | 55 | 1420／2400 |

Core根目录：D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/。
测试根目录：D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/；每个C#对应同路径加.meta。
九份meta全文均为Unity标准MonoImporter，11行／份；未见手写额外配置。

| 新meta对应C# | GUID |
| --- | --- |
| CandidateBattleHistory | 4c5fdbabf16b6d645af2922ee405813e |
| CandidateHistoryRequests | a45799ebb8a05474b9ba39c6986dd5b3 |
| CandidateHistoryOperations | 37fba5be8e8a939418ae1aa8e7bb4e56 |
| CandidateHistoryOperationsTests | 2d150af1620a14a49bcbe0186d3d8681 |
| CandidateRewardDefinition | db095a6ffca90944fb34afea4a0224cc |
| CandidateRewardState | 64890618de22b4248846867b0efce8ae |
| CandidateRewardRequests | 83cd8f1deed9ce14a8de789ac7a81dba |
| CandidateBaseRewards | e976fd1101c51394cbdaaf11d0c78107 |
| CandidateBaseRewardsTests | 21b8939ee6b10f7469ccfa013cd76357 |

R检查全部Assets的241个meta：九个新GUID各出现一次；原232个路径／GUID全部保持，无缺失或重复。
两包不引用彼此的新API；Core未引入Unity、IO、时钟、浮点、反射或另建Math／Evaluation预算。
013只调用已接收012的创建和纯片段组装；023调用005评分，不重跑009／010／011求值或调用020／021写候选。
旧415项保持，未改变旧公开API、序列化、配置或依赖；九份新C#无行尾空白。
R只读执行git -c core.safecrlf=false diff --check，退出0；未把既有未提交差异归入本批。

R保存的72项保护输入中69项SHA未变。
三协调稿SHA因派发登记变化；对冻结§114～118逐行比较，只变化状态／task／turn及实际派发说明，业务契约与白名单未变。
任务稿派发SHA为79582117b50b78b0ac4b5bb3abf6ef45ed098985085ecc7b8c2ca00945988ed8，当前为b9b47258b74975cc19060cf0d1eec92b5bf8404e701261f0b35582bb04a7c4b4。
B／D原始文件写入记录均未触及三协调稿；其余旧报告、规则、设计、config.json、stages.csv和B08证据均保持。

## 4. FM-DEMO-013 规格审查

[CandidateHistoryOperations.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs:11)核原012初态；Append核原Binding／Baseline／Initial引用、完整原前缀、恰一后继及原片段对应。
[PrepareRollback](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs:100)重新核当前修订和完整确认范围，恢复目标Before业务值，SceneRevision只取当前+1。
[历史审计](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs:187)按原修订合并攻击和回退记录，校验有效前缀、完整撤去范围、恢复状态及每项首次Superseded关系。
历史／范围／关系输出复制集合并只读，保留原不可变片段；业务拒绝结果无Next，Limit原样传播。

| 七项验收 | R核查的代码与已执行测试依据 | 判断 |
| --- | --- | --- |
| ①真实来源与追加 | Create仅初态；Append完整前缀及唯一新增记录，旧条件／时间／来源变动、跳步、重复Anchor或操作ID拒绝。测试RealPublicSourceOperations…、CreationCannotImport…、AppendRejects…、AppendRequires…及SyntheticLink…覆盖真实008→012和反例。 | 满足 |
| ②定位与确认 | Locate按当前面结构Pair倒序选实际Attack；固定线额外核致死事实、RouteLocked及路线。ReadRange显式Anchor可跨面。L3Endpoint…、LocatorUses…、ConfirmationMust…、PreviewCannotSilentlyGrow…覆盖最近攻击、精确后继和过时确认。 | 满足 |
| ③完整回退 | 构造新Snapshot保留面／阶段／Board／敌我成员／游标／计数／贡献／Random，修订当前+1，原Initial与有效前缀保留。RollingBackBothL3Steps…核完整业务等值、PRD／字轨迹及新ID／修订差别。 | 满足 |
| ④跨面与救援 | CrossFaceRanges…从第二面撤回第一面，并继续接受修订有间隔的真实行动；RescueCanUndo…核全倒与未执行敌意图游标；VictoryCannot…核WPS／已有报告拒绝回退和新增。 | 满足 |
| ⑤分支与旧回调 | Archive与RollbackRecords共享操作ID占用域，旧Anchor永久保留；分支撤销只标记当前有效尾部。BranchesKeep…验证第一次撤销关系不覆盖、全部旧操作可查及终局报告仅含新有效路径；复用ID、漏档和错归属反例均拒绝。 | 满足 |
| ⑥条件与只读 | CurrentExplicitPreference…核旧7／false条件和新88／true条件分离，修改请求／输入列表不影响输出；递归只读检查、Ordinal大小写／分隔符身份及根null行为有实际用例。无成长、库存、进度写入。 | 满足 |
| ⑦预算与回归 | History先检查全部Archive和RollbackRecords数值，包含已Superseded大数；新小预算限额、末步预算不足、无部分Next及新预算重试均有测试。原回归集合完整保持。 | 满足 |

本包108条／26个方法全部Passed。[测试源码](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateHistoryOperationsTests.cs:16)全部609行已读。
高修订2^100用例明确为无效内部审计探针：小预算先限额，大预算仍拒绝；不冒称已执行2^100次操作。
当前单Warrior域没有自然AOE／Pending链；测试明确验证缺原死亡历史的合成Link不能导入，没有虚报自然Link正例。
013不含014重演器；重新行动测试证明回退后真实012路径恢复一致业务状态，不等于完成重演产品功能。

## 5. FM-DEMO-023 规格审查

[FixNormal](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBaseRewards.cs:65)先核输入／保留数值，再按Attempt及Settlement查原结果；查重早于预期修订相等比较和005评分。
同完整意图返回当前State及原BaseReward；冲突整体拒绝，新结果修订只+1。
[报告检查](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBaseRewards.cs:214)独立核原绑定、初态、全部面HP、有效记录链、终局及原末操作／时间，最后重新计算FMBR01。
[片段与守恒](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBaseRewards.cs:282)核事实来源、索引、贡献一一对应、实际HP前后及溢出、分段后态和逐参与者累计；正确指纹不跳过这些检查。
原单Warrior参与者按原槽保留；[评分](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBaseRewards.cs:101)使用原入场Level与原RecommendedLevel、精确有理数平方乘幂及同一个005 scope。

| 七项验收 | R核查的代码与已执行测试依据 | 判断 |
| --- | --- | --- |
| ①真实正常封闭来源 | 012公开整关报告＋022公开NormalVictory；MatchesEnd核同玩家／尝试／Challenge／Baseline／Settlement／Level／Context／参与者／指纹及无NewAttempt。CorrectlyRehashed…、FragmentFacts…核缺首末步、重复、错受益者／FactIndex、HP／时间、伪覆盖和外来绑定，均整体拒绝。 | 满足 |
| ②精确经验出口 | 三个PublicWholeLevel…用例分别得到L1 W1的26、L1 W4的14、L3 W1的31；检查C／J／G及005上下界floor同Amount，保留真实ExactScoreResult。代码无浮点、显示log、提前floor或第二评分账。 | 满足 |
| ③原入场与原负责者接收 | G01CurrentGrowth…实际用020将原14应用到W4＋157得到W5＋6；当前成长先改变后仍接原14；021公开接同Settlement／Context的candidate:tin 2，022原结束已备。023 Core不代做这些写候选或H10。 | 满足 |
| ④原结果优先与冲突 | DuplicateReturnsOriginal…从后续更晚State及旧expected返回原对象，零log／零工作区重试不评分；FixedIdentity…核改Settlement／报告／定义／结束及另一Attempt复用Settlement拒绝。新Attempt有自己的普通奖励，不自行发首通额外物。 | 满足 |
| ⑤参数、全关和零值 | 明确空能力、固定材料向量和CurveIntercept；缺失、负数、重复Item、未知枚举／能力拒绝。WholeLevelReference…核两面60HP／J30及首面不能结算；E0、零权重、空材料、精确幂与非幂评分保留全部来源校验。 | 满足 |
| ⑥随机与只读 | 固定表保留原BaseReward域／SourceCapability／Mapping，NotUsedFixedTable、前后同初态、空Words／0消耗；Battle／Bonus未变。FixedTableUses…核定义壳修改不影响结果及递归深只读。 | 满足 |
| ⑦共享预算与原子性 | StateNumbers复验全部旧固定结果，计算沿scope.Budget.Math并把原scope传005。SharedLimits…、CompleteOperation…、NewSmallBudgets…、OneEvaluationScope…核Math／LogTerms／LiveIntegerBits限额、末评分失败无结果、保留预留及累计预算不重置。 | 满足 |

本包126条／19个方法全部Passed。[测试源码](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBaseRewardsTests.cs:15)全部584行已读。
R另以整数幂不等式核算26／14／31：L1的x=37/12满足2^16<x^10<2^17；L3的x=22/7满足2^19<x^12<2^20。
W4的G=9/16；x²>8且x³<32使45/8×(1+log2(x))严格处于225/16与15之间，floor为14。
这些是独立文档算术核对；005运行证据来自B同版测试，R没有额外执行项目代码。
材料身份保持candidate映射；合成C=1/1000随机条件见证明示为测试来源，不作为生产平衡或正式发布承诺。

## 6. 联合版本与原始执行证据

013四源码冻结12:01:03.2877531Z，规范SHA=9add1310653e942abb4ee82f7a7459c535cc18348c46ed00508f78dc4685c7c5。
023五源码冻结12:07:49.9657056Z，规范SHA=26bd0e46cb083b30990c6b524e88b5fa93b16d9e78256e52fa513dc270b52175。
R独立合并九份冻结行并计算SHA=54e16d3ded47377724c43262b4cbbc9539f9e7562d5b86576475e4bb0143bd01。
B／D保存的共同冻结逐文件差异0；当前九C#与各自CODE_READY全部一致。

编译前12:10:00.5366267Z为424项，R从原415＋九源码重算SHA=5b65a5578af865d11d39e25f91628d0ddb80e18bc414db0f5dc0bf19e176e153。
测试前12:11:47.184343Z为433项；R再加九份生成meta，重算为上述最终433项SHA。
B验证后12:15:10.9719792Z、D核验12:15:18.3781717Z及R现场结果均相同；无源码重冻或用旧测试结果覆盖新版本。

| 实际运行，UTC 2026-09-21 | PID | 起止 | 实际子进程退出 | 原CommandExecution |
| --- | ---: | --- | ---: | --- |
| 编译／导入 | 14956 | 12:10:45.6804088Z→12:10:57.7651525Z | 0 | exec-4a77cfe3-a5a8-409b-aca8-eef1ec278e31 |
| 全量EditMode | 2080 | 12:12:18.1479171Z→12:12:43.7451411Z | 0 | exec-e8eb166a-6f66-4269-9994-5f9210b5ed48 |

R读取B原JSONL第292／318行CommandExecution，完整命令及stdout与scope.rawValidationEvidence逐字一致。
实际命令先重算冻结SHA并核Get-Process Unity为空，再Start-Process Hidden／PassThru／Wait、WaitForExit并读取真实Process.ExitCode。
固定Unity为D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe；编译有-quit，EditMode无-quit，日志中实际参数相符。
scope保留的初始会话88684／62870和完成回执，对应真实子进程，而非外层shell即时退出码；D保存的原证据和B完全相同。
R还解析双方原turn全部执行命令：B仅上述两次Unity启动，D零次，未见其他编译／测试或Git写入。
B开工Get-CimInstance读取受限后改用Get-Process；这是只读进程查询失败，正式Unity两次均一次成功，没有需覆盖归档的失败运行。
双方文件写入／FileChange路径仅各自白名单及本包报告／scope；最后源码修改早于各自首次冻结，meta仅在共同Unity导入后出现。

| B原始运行产物 | 字节 | R重算SHA256 |
| --- | ---: | --- |
| Logs/FMDemoB09Compile.log | 64924 | d3bef0bfa530e7e06a6c6b6f67e077720fa2fa4def5c57e63a38972df3960667 |
| Logs/FMDemoB09Tests.log | 89029 | cc6728c0cde79d9be2750214d69eced92301738d0c3b31b765a7fd024e9688f9 |
| FMDemoB09-EditMode.xml | 1402504 | d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22 |

两日志均标明Unity 2022.3.18f1，C#错误／警告扫描0；未将非C#许可或退出诊断混称为日志完全无提示。
XML执行12:12:25Z→12:12:41Z，2145/2145 Passed，Failed／Skipped／Inconclusive均0。
R逐StringComparer.Ordinal fullname核旧1911条／1906个不同名称的完整多重集合，并分别比较逐名Passed次数，差异均0。
原XML SHA=a384933b9f9f5b9c3c12017ec0cd8e1b64c6d4115cae44f06c438528176167fd；三个既有重复名称组的4／2／2次数全部保留。
旧fullname＋TAB＋count＋LF规范SHA=59b99d445f2468f4591bd6411039fe79af395cf03b3bf68b9a7feac07748420c。
旧fullname＋TAB＋count＋TAB＋passed＋LF规范SHA=fdb956ca0c72b564b828efc82eab4cff29d9bc1cf0f43f249d190bbb844529ff。
新用例恰为CandidateHistoryOperationsTests 108与CandidateBaseRewardsTests 126，无其他新增；完整参数化名称由原XML和两scope可追溯。
R依据源码行为、测试断言和原始执行证据共同判定，没有只凭总数增长或作者自检接受。
