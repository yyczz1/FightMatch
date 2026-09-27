# FM-DEMO-007B 实现交付

状态：WORKER_RETURNED；作者验证完成，等待 R 按 §85 独立审查；本文不作独立 ACCEPT。
批次：DEMO-B03；实现任务 A：01a0c1fb-248d-7321-8b69-9efd26b817f1。
本次 turn：01a0c238-7a84-7ad2-bbf4-9a3d15c15ed0；交回日期：2026-09-21。
授权：system-task-packets.md r64 §83、84，精确实施范围为 §84；007A 已获 R ACCEPT。

唯一组装入口为 `BattleStartAssembler.CreateCandidate(PreparedBattleEntry, CandidateRandomInitials, ExactMathBudget)`。
调用依次处理根 null、全部 Prepared 数值的本次预算、Battle/BaseReward/Bonus、PRD 输入行和缺行，最后构造结果。
缺项、重复及绑定错误沿用 BattleEntryRejectionCode 与 FieldPath；拒绝的 Start 为 null，预算异常原样传播。
三域必须显式提供未消费初态；PRD 必须逐角色／被动提供显式 f=0。不初始化 PCG、不抽样、不生成 ID 或 seed。
Baseline 保留原不可变 Prepared、全部面、三域初态及 PRD 初值；快照只持有 Battle／PRD 当前态。
成员保留 EntryHp 和原槽，首面敌人按原 Pair 顺序保留属性、原槽与行动序，游标为0；贡献为精确0/1。
只生成当前 W 无主动、空携带、E01/E02 的候选初态；CarryMode.Empty 表示本域 C/U/D 为空，不表示库存 T=0或冻结提交。
值键逐组件 Ordinal 比较，哈希只用于集合；只读输出不保存输入可变壳或列表，FlowPos 路线按值复制。
没有生产入场、恢复、真实随机绑定、SDK/M12/Use、Published/H10、奖励当前态或四职业能力。数值预算不承诺总堆内存预算。

## 七项验收

以下 PASS 为作者实际验证；全部新用例位于 BattleStartAssemblerTests，逐用例证据留在 XML。

| §84 项 | 结果 | 可复核见证 |
| --- | --- | --- |
| ①完整初态 | PASS | SourceCandidates 四例：006 L1/L3 × C↑/C↓ 经007A接入；同一 Baseline、首面几何映射、空固化／待补线、W EntryHp=97/2、E01/E02 HP=15/20、首个 Strike/Charge、全部计数、贡献0/1、三域与PRD初值均核对。 |
| ②身份／原槽／多面 | PASS | slot2 成员、敌槽7/2、StableOrder17/3和 B/A 输入顺序不改；双面局部A的键按面／Attempt区分，第二面HP34仅留定义。键包含分隔符、大小写与空格时逐组件区分，相等键哈希一致；8项工厂组件逐一拒绝 null/空白。 |
| ③随机与PRD | PASS | 三域各自缺流及消费w拒绝；缺列表／行／字段／f、重复、额外角色、错被动、f正负值均无Start。输入行先于缺行核验；原始字符串按Ordinal匹配。数值相同三域、合法State=0及显式f=0通过。每次拒绝核输入不变。 |
| ④预算与不二次求值 | PASS | 3项巨大等级／推荐等级／Context见证；28个可变有理字段各验巨大分子与分母（56例，含后续面），三域数学值12例，f正负超限2例。共享步骤预算含预消费、末步不足、无部分输出及新预算重试。W4保留124、118/5、29/2、21/2及EntryHp247/2；推荐等级不缩放敌人。 |
| ⑤深隔离／重复 | PASS | 相同输入两次逐值一致；替换三域引用、修改并清空PRD行／列表后旧输出不变，Prepared不变。递归检查只读属性、非公开构造和集合写入拒绝。当前公开007A只接单Ready战士，没有反射伪造不可到达的Prepared。 |
| ⑥所有权／范围 | PASS | BattleRandomSnapshot仅Stream/PrdStates；三域初值留Baseline，非零w/f明确拒绝。入口只有CreateCandidate，模型无公开安装／恢复／WithHp旁路；候选Context原样保留。合成C、坐标和数学初态均不构成生产审批。 |
| ⑦实际验证 | PASS | 编译exit0；EditMode exit0，961/961通过，新增110。原851条按Ordinal fullname及出现次数完整匹配并全过。319旧文件SHA不变，恰12新实施文件、1266行；6个Unity生成meta的GUID全Assets唯一。 |

Prepared 数值重验覆盖两处 Context 修订、成员／推荐等级、全成员属性／EntryHp／Crit、所有面敌属性／非空意图系数；合法Evasion固定0/1仍逐项检查。
未测试未实现的攻击、回退、翻面执行、保存、Player/PlayMode或真实发布；没有用测试数增长代替上述行为见证。

## 新文件与范围证据

仅新建下面12份实施文件、本文和 demo-007b-scope.json；三份运行日志/XML由Unity命令生成。
现有源码／测试／asmdef／meta保持，未修改协调稿、其他设计稿、工程设置、依赖、权限或Git。原tracked差异仍为34文件、+2809/-597，不属于本包。
六份C#已全文检查，六份生成meta已全文检查；git status、diff --stat、--name-only、--check均实际执行，exit0，diff --check无问题。

| 新建路径 | 行数 | SHA256 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/CandidateRandomInitials.cs | 21 | f7a769c2c8b8c9647278ff1f513762689817e463e6e70b8b279853f4ee8eeb12 |
| Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs | 76 | 3cdede0006e090eb63d8b15e2c999787ba62454b791571c84c6f07e351c500b8 |
| Assets/Scripts/FightMatch/Core/BattleSnapshot.cs | 154 | f4583c3c6509800557bce1b96b74cbcb6b21da85dfe3ffe506994be98ef825bc |
| Assets/Scripts/FightMatch/Core/BattleCombatantKey.cs | 108 | a1269c134e2564b7323c39ced88e7a9787819f1d37f5b498191723767efe1748 |
| Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs | 118 | 803bff5f1e3f9fde1f788ddfb932f97349db7df9f13aac756ef420d2f295d805 |
| Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs | 723 | 5c87422b014a25f22e68d24efef9305940943be1ba83f0ee5dee4a5fd2c39578 |
| Assets/Scripts/FightMatch/Core/CandidateRandomInitials.cs.meta | 11 | 07a50983e25b7e6b21cfe007efcdb92aef9d0256a7e857a461ff491a424453b4 |
| Assets/Scripts/FightMatch/Core/BattleEntryBaseline.cs.meta | 11 | 44732fdfb2ba38be3dce2f4e49524f126c62a691f17d9cb362415d4db5e8df53 |
| Assets/Scripts/FightMatch/Core/BattleSnapshot.cs.meta | 11 | cf917965123eb036fd00c8b266d565e19e0478e0a8f28d63093dbeec920b98b8 |
| Assets/Scripts/FightMatch/Core/BattleCombatantKey.cs.meta | 11 | 55f4600174cd54d7b115b7f8e34776b6fe4ad8cf4148d8e3986477265016faae |
| Assets/Scripts/FightMatch/Core/BattleStartAssembler.cs.meta | 11 | 581b97d690c31cb585561c907e6fbefdb5571e5fc054b4bdf821635896db768d |
| Assets/Tests/EditMode/FightMatch/BattleStartAssemblerTests.cs.meta | 11 | 40b2315119c84fc8a5d88cc6235645d9ae31f626ba2728b735140e681c71fbe7 |

修改前于UTC 2026-09-21T04:33:18.2287838Z采集319项，14个交付新路径均不存在；原始scope SHA256为 c4fc27e83b391a4c7a39e475391717127524ccc55f60d303632575a266098331。
before规范摘要：ba26d102e53096c71e95d0665d964371f7a5952829be58c1485601054a8b543c；最终after于UTC 2026-09-21T04:50:12.6047894Z采集331项，规范摘要：24f0e94da6104daa0565ea00a46d541d65ab886d4d43676bd7a458f557f3496b。
规范为Ordinal路径排序，逐项“相对路径+TAB+小写SHA256+LF”，UTF-8无BOM且含末尾LF。after通过追加写入，before原文前缀逐字保留。
实施总计1266行≤1600；scope为118999字节≤300KiB，SHA256=277bb5e05b48bc334e8d2f2a8a93d323a07ed5bd6d2ecddeface7cbd1a5ac177。清单只保存路径／SHA／时间和必要范围元数据，不复制XML用例列表。
编译前六份C# SHA于UTC 2026-09-21T04:45:49.2753686Z记录，编译／测试后与上表完全一致；Packages、锁文件和ProjectVersion的SHA亦保持。

## 实际执行与回归证据

两次启动前均确认Unity路径存在、无活动Unity Editor；未杀进程。使用已恢复许可证，在沙箱外运行；本包两次实际启动均成功。
下面为实际Start-Process启动及等待／取码步骤，编译结束后才运行测试，测试参数不含-quit。

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
$compileProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007BCompile.log') -WindowStyle Hidden -PassThru
$compileProcess.WaitForExit()
$compileProcess.Refresh()
$compileProcess.ExitCode
$testProcess = Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemo007B-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemo007BTests.log') -WindowStyle Hidden -PassThru
$testProcess.WaitForExit()
$testProcess.Refresh()
$testProcess.ExitCode
```

RUN 编译：PID 53244，UTC 2026-09-21T04:46:20.4699934Z～2026-09-21T04:46:32.6973396Z，exit0；无C#错误／警告。
RUN EditMode：PID 16784，UTC 2026-09-21T04:47:17.1677122Z～2026-09-21T04:47:29.1343549Z，exit0；XML运行区间2026-09-21 04:47:25Z～2026-09-21 04:47:27Z。
XML result=Passed，total/passed=961，failed/skipped/inconclusive=0；全部961个test-case均Passed，新增110均属于本新测试类。
原851例共848个Ordinal fullname，当前对应子集逐名称计数与通过计数均一致，缺失／多次／失败差异为0。
“fullname+TAB+次数+LF”规范SHA两侧均为 5a8cc1a4d4bd42de84a06df600d2abf54a53deb765f89d8ef6abcce073c4c437；未只比较总数。
成功后没有修改验证输入，未重跑验证；所有新输出的独立审查仍归R。

| 运行产物 | SHA256 |
| --- | --- |
| Logs/FMDemo007BCompile.log | c2f844c521cb5c69f3bc335be11bd709cf659d5f4d8a84b23da32be4a194efb7 |
| Logs/FMDemo007BTests.log | 2dc47d5f1dbbd61fcef7a2b0f07ee297b502cbbd527081dc22355283750edd28 |
| FMDemo007B-EditMode.xml | 9b0d12667df5a7bf3061377874efeb12ba1c694454446976ea50c763a8baf6e9 |

## 输入与基线SHA

以下为本次实际读取／验证对应的文件摘要，编译前与交付整理时一致。协调稿只读，后续状态登记归SD00。

| 输入路径 | SHA256 |
| --- | --- |
| docs/system-design/2026-09-17/system-task-packets.md | b35b7cecc63bca0e8141878dba64b480c536e9118627455a47368b28d7e00f82 |
| docs/system-design/2026-09-17/demo-batch02-code-review.md | 7c01758c0f79b00f10fc8c299685e98b38119ecedf833afec77aa4279ee131ad |
| docs/system-design/2026-09-17/demo-007a-delivery.md | bb2cd05057c1c92cdf2f1a8aafd6fb9f3c7ed515dc3325f02e5cbf61b324caf5 |
| docs/system-design/2026-09-17/demo-007a-scope.json | da9770fd250339b02676eb881219923a3d91611abf04a02d942add378669c8e6 |
| FMDemo007A-EditMode.xml | 631a94ee9347188c8e7bb1e645653121a08d4b54725dbaa2aee2285146591492 |
| docs/system-design/2026-09-16/interaction-contracts.md | eab83027c412619731df1f7f3fd1650161915d96e31d68ea30f4af4af9f96f06 |
| docs/system-design/2026-09-16/details/battle-history.md | 0a9c30f131822dad484a12b234a0262b49897f3190837a2655da4024a665fe41 |
| docs/system-design/2026-09-16/details/numeric-random.md | 45adbd3ff517c2db91f1fc8a7c967241c681771fe0a61e2d4e860404387b47bc |
| docs/system-design/2026-09-16/details/content-validation.md | b430f3384aeb20bf5742bf29c5949af13cf25eb4eaea31bedbbac5da8beeafc9 |
| docs/system-design/2026-09-16/details/contribution-scoring.md | 5194dc775eeb2bf359604eb1966a20a51062c7c48bfedfce5c15601ec1931912 |
| Packages/manifest.json | e15e302b5c4342d530ae52f31c785a9626b4ff42ecc1aa7612fcc3f0637b872a |
| Packages/packages-lock.json | 160073f2cd54a18fc4a3995c64b53de964356c5f36b66020fb66eb165e01c31a |
| ProjectSettings/ProjectVersion.txt | 9b7f178dd8c050e64943db5709939f39fd3191c18c6c557143963975891b50e2 |

007B作者实现与验证在此结束，等待R独立审查；没有执行008或其他包，没有创建任务／子代理。
