# DEMO-B10-R1 独立审查

审查日期：2026-09-21；审查人：R；gpt-6-astra／max；原目录 local。
本报告按冻结 r75 §119–125 和 .agent/REVIEW_CHECKLIST 审核实际源码、测试、范围及原执行证据。

| 包 | 规范审查 | 规格审查 | 唯一结论 |
| --- | --- | --- | --- |
| FM-DEMO-014 | 通过：范围、纯 Core、原算法复用、精确比较及冻结输入符合约束 | 七项通过，Link 证据边界见下文 | **ACCEPT** |
| FM-DEMO-015A | 通过：范围、显式格式、严格标量、预算与流所有权符合约束 | 七项通过；只接收技术封套与启动摘要 | **ACCEPT** |

未发现需返修的问题；无必要用户动作。接收不包含 015B 业务恢复、真实保存提交、Published／H10 或全局可解性证明。

## 1. 冻结契约与审查门槛

派发契约为 [system-task-packets.md](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md) r75 §119–125，派发字节 SHA256：
`06425a9ae84a74ebaaab2ed71d59c6b83b66d49ce80242750dc5c46808cdeb2a`。
R 在实现读取前保存了固定范围、保护文件及契约文本；不采用后加 §126–127 作为本批验收条件。

| 角色 | task | 本批原 turn | 工具确认 completed（UTC） |
| --- | --- | --- | --- |
| B／014 | 01a0c3c3-e163-7f01-b639-ca1f16e1a99d | 01a0c404-71c6-7003-a4ed-241dcde05bd6 | 13:53:49 |
| C／015A | 01a0c403-bfa1-7e90-b503-c0fcd61f23c1 | 01a0c403-c2a4-7c50-851a-3335c3080a47 | 14:01:59 |
| R／本报告 | 01a0c1cd-dce1-7ac3-8780-06163cb0acfc | 01a0c405-002b-7513-8c0d-c72d4a87972c | 交回后结束本原回合 |

双份正式交付及联合证据齐备，两个指定原实施 turn 均 completed 后，R 于 14:01:59 UTC 开始正式实现审查；CODE_READY 或后续确认回合未替代该门槛。
R 全读十份 C#、十份 meta、两份 delivery／scope、实际日志与 XML，并独立追溯两个作者原回合；未启动 Unity、执行项目测试、修改实现或建立任务／子代理。

| 正式交付 | 行数／字节 | 独立核实 SHA256 |
| --- | --- | --- |
| [014 delivery](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-014-delivery.md) | 114 行 | 9883a36063aaa78c9f19bed21af1480b7d8831a508152cabfa83d9a99a265412 |
| [014 scope](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-014-scope.json) | 391886 字节 | 6b5948b9ee17e9621dda8ce55482e72eb2063a09b4828897b797e66fc6149ccd |
| [015A delivery](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-015a-delivery.md) | 139 行 | f58d1837568872ce74135446c9ba9bbd0e1c3b4757db08a30a0af3ababa38e5b |
| [015A scope](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-015a-scope.json) | 399491 字节 | 882df2123d5bc3e92d4a8b4c9672d0a9200d28945773fcc4a360e0f6c3aa5f73 |

两个 delivery 均不超过 210 行，scope 均不超过 400 KiB。

## 2. 范围、来源与同版冻结

固定 before 来自已接收 demo-013-scope.json.after.files：433 项，原时间 2026-09-21T12:15:10.9719792Z；原 scope SHA256 为 `d110349421486511971eb1a6c49b6adce38b9685f62e03db097643d4c9a136fb`。
R 的独立基线采集为 12:52:54.3253744 UTC；原 before 未被作者实际开工时间覆盖。B／C 另记 startedSnapshot 为 12:52:08.7683696／12:54:49.1684440 UTC。
规范清单统一采用 Ordinal 路径排序、路径＋TAB＋小写 SHA＋LF、UTF-8 无 BOM。重算结果如下。

| 集合 | 项数 | 规范 SHA256 |
| --- | --- | --- |
| 固定 before | 433 | 2f695856ae72d8d7a8582d6b51e3480ded3913dbd006970c052180bf8c2c851e |
| B 最终五 C# | 5 | 026ff10e139e82a44eff96424a6c6c4a0b1ba1cf1c67cf566faf74ba1a941836 |
| C 最终五 C# | 5 | c07edcd5db2767d0aed63ed0d681f9a5bd634fe213261907a0dc149f6b3f96be |
| 13:44:41.3269702 UTC 共同源码冻结 | 10 | b4e5b8b8df5d0f41a4f6225bbb29301fe9d1978094a796a98be2907afc958750 |
| 联合验证后及 R 实读 | 453 | 7dec93b9baef37c7196372087df1089ea49a6b27bcd37a145732bfae652202ae |

旧 433 项 SHA 全部保持；新增恰为 §121／122 各十份，共 20 份，无缺失或白名单外实施文件。双方 before／after 完整清单、共同冻结及当前文件一致。
全 Assets 共 251 个 GUID，无重复；原 241 个路径／GUID 保持，十个新增 GUID 唯一。meta 由 B 的 Unity 导入生成；C 无 Unity 或手写 meta。
额外保护的 81 项中 78 项未变；三份协调稿的变化来自 SD00 状态登记／后续备包，冻结 §121–125 的业务条件、白名单及验收未改变。作者原回合写入审计未发现其改写协调稿。

| 新 C#（Core 或 EditMode/FightMatch） | 行数 | SHA256 |
| --- | --- | --- |
| CandidateBattleIsolation.cs | 87 | 277d034ae8c95b8671ac6258c18ea9de8da1527095dfae143b1d911fc9893515 |
| CandidateReplayRequests.cs | 78 | 019337793be562a2eb9d7eec9773d51d90db818544892033ef9af7e63784d227 |
| CandidateReplayOperations.cs | 343 | 8c8fd73e48465bc864ebf4c5e4f7808a61a28231f8070d08d52752e36cea7b86 |
| CandidateReplayComparison.cs | 577 | ec15b1186f4a2e384e1c1308d1293f29fe02e9cf3470abaa90f4cb1a25ae93f4 |
| CandidateReplayOperationsTests.cs | 610 | 188bc0cea1b3c6f2e437d3af742ac5893ed5557f60d4e34e11a2fe9496eb9521 |
| SaveEnvelope.cs | 152 | f8582cf3e9f059e3fc5e30635de1073650ff4639b662605e61a667ff3a79bdb5 |
| SaveEnvelopeRequests.cs | 93 | 9f5286125fa7ac9286fb36c9f57f90ce64e3fccc596d22c77e11e775260ade06 |
| ExactSaveValueCodec.cs | 98 | 2baa45d3d0d5524fd9f209861282c4e13dba609971eb16cd3b71e5e3d86bb125 |
| SaveEnvelopeCodec.cs | 567 | c617a4f568594504159ab6a5ad4e047830da2307885949dae3cdc05604f24a14 |
| SaveEnvelopeCodecTests.cs | 891 | 5daea0301e4076a22c5fc5abf908600e8389509c8c06bf6cb618c296e29efc58 |

014 为 1695 C#＋55 meta＝1750 行；015A 为 1801 C#＋55 meta＝1856 行；均低于各 2600 行。
未引入依赖、程序集、公开旧 API／旧序列化格式变更、反射框架、第二套战斗算法或文件提交适配。两个包未相互依赖新 API。

## 3. 原执行证据与回归

R 从 B／C 指定原 turn 的本地原始会话读取实际命令与工具结果，分别核 62／43 条 shell 命令及补丁目标。
B 共五次实际 Unity 启动，另一次在启动前被冻结检查阻止；C 为零。未发现杀进程、Git 写入、跨作者源码修改或额外项目执行。
每次实际启动前均检查 Unity 占用及对应冻结；固定 2022.3.18f1，Start-Process Hidden／PassThru／Wait、真实 PID、WaitForExit 后 Process.ExitCode。
编译含 -batchmode -nographics -quit；测试含 -runTests -testPlatform EditMode 且无 -quit，产物路径均为 §124 授权位置。

| 原 CommandExecution | PID | UTC 开始→结束 | 实际退出码／结果 |
| --- | --- | --- | --- |
| exec-59291004-f8fe-4fe1-ac1e-45ba53f4044b | 43064 | 13:22:14.1464285→13:32:23.9448724 | 1；首次编译失败 |
| exec-263ea57c-766c-4e7b-adf9-621daabdc9e6 | 48828 | 13:34:55.4894257→13:35:14.3894830 | 0；修正后编译 |
| exec-7171c8a8-b60c-4c83-91a4-38ffd61ebc1f | 无 | 13:36:05.859 工具完成 | shell 1；meta 冻结拦截，未启动 Unity |
| exec-3b7c3d8e-916c-48ff-8710-5e80549842b2 | 39380 | 13:38:13.5900897→13:39:01.2747934 | 2；2420 Passed／6 Failed |
| exec-a1a9c864-40ba-40d8-81db-95262bd55629 | 29320 | 13:45:09.1791764→13:45:21.2384561 | 0；最终编译 |
| exec-4abcc726-fcd8-4e6d-ac3d-4115b67fa877 | 51844 | 13:45:53.6779293→13:46:38.8239555 | 0；最终 2426 Passed |

五条实际 Unity 命令与封存 rawValidationEvidence 逐字一致，原工具最终输出／退出码与六条 CommandExecution 标记相符。Unity 内容写入 -logFile；工具 stdout 是 PowerShell 包装结果，未冒称另存了 Unity 子进程 stdout／stderr。
双方 rawValidationEvidence 的 brotli 原文独立解码为 49883 字节，SHA256=`e29d0df6eeba1b8b31c748ae075c7f8858594cf385054ec1066e6d4eb8b740ee`，相互一致。

| 独立解码并复核的被覆盖产物 | 字节 | SHA256 |
| --- | --- | --- |
| 首次失败编译 log | 42394 | e0df3543a9687864367b3b6825f7289c5d20084b0d3a9efd2c69c52e3ce03c20 |
| 第二次成功编译 log | 63500 | f774a3e3368de6bc578de16b5d6c991c287688f5904aefc4abc2fcb20d0cbc55 |
| 首次失败测试 log | 89015 | 993430e5fdea76e3bca0918f0e21286f4a690c3a663c66b0663b0ce908474cc4 |
| 首次失败测试 XML | 1605772 | a1d5cda16db11a8cefedcda07607da589417af298b8b61067e27b9a35a898ad5 |

首次 CS1729 为 B 新测试错误使用 CandidateCritFact 的 11 参数构造，修为既有 7 参数；未改旧 API。
首次测试五个 B 失败均为 L3 Charge 测试输入遗漏显式 DamageKind／DamageCoefficient 字段，XML 报 MissingField；一个 C 失败为 Mono 分配计数返回 0，断言原期望 >2097152。
原作者只修自身新测试。C 改用实际 Stream.Read 数组身份、长度和复用次数观测；未跳过该测试或删掉分配边界断言。八份新 Core 自首轮共同冻结后未变。
首次失败导入留下两行占位 meta，后续成功导入扩展为 11 行 MonoImporter，触发测试前冻结检查。R 用原 GUID 重建 59 字节 LF／无末尾换行／无 BOM 的占位内容，十项均匹配原 SHA；GUID 未变。重新记录最终 meta 后才继续验证。

| 最终实读产物 | 字节 | SHA256 |
| --- | --- | --- |
| [Compile log](/D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Compile.log) | 61385 | e9810674d27cc3441f5a711bdff0fcfe0230d8728c492a1b3ee46faa855b1ba5 |
| [Tests log](/D:/Unity/UnityProj/FightMatch/Logs/FMDemoB10Tests.log) | 89006 | ff5f36f62c330d74acd10975404d065a6586568a5e1e9469bc6b7933a5fa4095 |
| [EditMode XML](/D:/Unity/UnityProj/FightMatch/FMDemoB10-EditMode.xml) | 1600224 | dfb08fb27958836e521021293faf502aba76a18d56873977ba1b83941497dc5a |

最终两 log 的 C# error／warning 均为 0；存在非致命 Licensing 签名／令牌消息及编译 log 的 Curl 42，故不宣称日志所有 error 文本均为零。
最终 XML 为 2426/2426 Passed，Failed／Skipped／Inconclusive 均为 0；014 新增 110 条／23 方法，015A 新增 171 条／31 方法，没有其他新增用例。
原 B09 XML 字节 SHA=`d0aea4d0a22a3de60cf7f6bb560a64ccf98e5b0cf8e0bbeeb97b7e2d7ffabd22`；旧 2145 条包含 2140 个不同 Ordinal fullname，原三个重复组的重数 4／2／2 均保留。
R 按完整 fullname 逐名比较重数及 Passed 数量，差异为 0；不以总测试数代替。排序后 fullname＋TAB＋count＋LF 的 SHA 为 `beb17a2c289c85f9fd21c1c7e8bf5113d23bfd8e3553a3ad89437a0b71897df5`；再加入 TAB＋PassedCount 的 SHA 为 `d13c918425980cdac786df6a24d366d6d4c4c87ace615fb9cddf33d07792595c`。

## 4. FM-DEMO-014 逐项规格审查

以下 Operations／Comparison／Tests 分别指新增 CandidateReplayOperations.cs／CandidateReplayComparison.cs／CandidateReplayOperationsTests.cs；行号为本报告冻结版本。

| 验收 | 独立源码与实际用例证据 | 判断 |
| --- | --- | --- |
| ①真实重演 | Operations 97–110、150–162 实际调用 012；Tests 14–33 从公开 008→012→013 建立原记录，断言新片段／原图不变及非缓存结果 | 通过 |
| ②旧分支 | Operations 54–85 精确选首次 SupersededBy 的 BeforeRun，校同一 Record 并截取其原前缀；Tests 35–67 验两代撤回、跨面与救援的旧意图游标 | 通过 |
| ③随机与差异 | Tests 70–103 核合法 PCG 两个拒绝字及后续接受字、q=1／真实 Link 拒绝零取字；283–349 核 22 类字段差异、随机优先及六类终局报告差异 | 通过；Link 成功路径限制见下 |
| ④条件与键 | Isolation 12–52 保存完整结构并精确比较；Operations 120–123 先冻结全部步骤；Tests 167–210 核条件差异、历史旧条件及外部请求／路线修改隔离 | 通过 |
| ⑤完整计划 | Operations 112–147 顺序调用 012，所有步接受后才给 PlanEvaluated；Tests 105–164 验 L1／L3 两朝向胜利末步报告、跨面、射程、救援、非法尾步及不完整计划 | 通过 |
| ⑥缺项与预算 | Operations 164–311 核原图及关系；直接透传同一 Math／SamplingBudget；Tests 213–280、351–434 核缺关系／错绑定／旧回退 ID／非 Empty／各阶段 Limit／新预算重试 | 通过 |
| ⑦回归与范围 | 110 条实际通过；旧 2145 逐名重数／Passed 保持，十实施文件 1750 行、旧 SHA 与 GUID 均满足联合证据 | 通过 |

[隔离构建](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateReplayOperations.cs:54) 保留原 Binding／Baseline／Initial，使用原 Record.BeforeSnapshot；已撤操作不从当前 HP、Archive 顺序或最新分支推断。
同一原不可变图可以共享；调用方条件、路线及步骤列表被冻结。ContextKey 包含完整上下文／定义、基线、起始快照／随机、源修订、条件、用途、目标和已用操作 ID；PlanEvidence 另冻结真实步骤和条件，未实现缓存。
[首差异比较](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateReplayComparison.cs:70) 按输入→随机→伤害→敌意图→阶段／Board→贡献→完整 After→完整 Report；集合 Count 先于元素，字符串 Ordinal、数值精确；固定类型逐字段，无反射。
Crit 比较先核完整 RawWords（含拒绝前缀）再核各随机流；EnemyPhase.DirectAttack 必须与 Record.DirectAttack 为同一片段，相关随机已在前序比较。Report 比较包括末操作与完整字段，非仅 HP／hash。
缺原关系、错误图先 Rejected；完整来源经实际新求值证伪才 Diverged。限制及计划失败不返回可提交 Next、部分成功证明或成功报告；所有结果 CommitEligible=false。

**Link 的证据边界：** [既有 012 测试](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleOperationsTests.cs:135) 已证明单 Warrior／仅主目标死亡时会立即锁定自身 pair，不能自然形成公开历史的 AwaitLinks；伪造缺连续关系的 imported history 会被 012 CheckRun 拒绝。
014 的 [Link 分派](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateReplayOperations.cs:154) 直接复用 012 并传同一 Math，现有新增测试只证明真实拒绝路径零取字，未证明成功 Link 重演。此为冻结 012 领域限制，不要求本包添加历史导入旁路或新死亡规则。
固定种子胜利仅证明该绑定及原随机轨迹的一条计划；未作搜索、全局无解／所有种子保证、永久状态写回或发布。

## 5. FM-DEMO-015A 逐项规格审查

以下 Codec／Tests 分别指新增 SaveEnvelopeCodec.cs／SaveEnvelopeCodecTests.cs；标量指 ExactSaveValueCodec.cs。

| 验收 | 独立源码与实际用例证据 | 判断 |
| --- | --- | --- |
| ①确定字节与精度 | Tests 19–168 独立固定完整向量、UTF-16／孤立代理单元／null 空差异、>2^53／1/3／负分数及非法词法；标量 55–92 先限长度再规范解析 | 通过 |
| ②技术完整性 | Codec 106–180 检验头／边界、每片／正文／整档摘要与完整 expected；Tests 394–410、446–557 覆盖未知 schema、损坏、每字节截断、尾字节及错身份／长度／摘要 | 通过 |
| ③目录与声明 | Codec 185–219、246–280 校 required 同序覆盖、唯一 ID、Owner／schema、连续 offset 及要求并集；Tests 257–391 覆盖缺漏／重复／间隙／重叠／错误摘要／要求差异 | 通过 |
| ④自足索引 | Codec 220–245 校 1..N 连续代、唯一 Commit／Operation、父链及本代无自摘要；Tests 394–444 验三代索引和各类断链，读取不依赖旧物理文件 | 通过 |
| ⑤用途与隔离 | Codec 282–367 比较四种绑定的完整组件，PlayerSave 拒绝候选；Prepare 冻结数组／列表；Tests 171–255 核用途、完整绑定、稳定并集和输入修改隔离 | 通过 |
| ⑥流与失败 | Codec 58–180、411–443 核预算先行、短读／非 Seek、I/O 透传及完整成功后给 Value；Tests 560–631 核所有权、当前位置、故障阶段、不可读写／null | 通过 |
| ⑦回归与规模 | Tests 637–696 核四级目录／正文边界及实际缓冲复用；171 条全部通过，旧 2145 保持，十文件 1856 行、范围／meta／联合证据满足 | 通过 |

[固定字段封套](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/SaveEnvelopeCodec.cs:58) 为 FMSAVE01／版本 1：小端整数、原始 UTF-16 代码单元、ASCII 规范整数；无浮点、Unicode 归一化、反射类型名或隐式业务对象恢复。
长度／计数及总和在相应分配或遍历前核字节界与溢出；所有数值复用调用方 ExactMathBudget。解码拒绝非约分有理数／-0／前导零等存储值，不静默修复。
绑定并集按目录及列表顺序保留首次出现，完整组件比较；内部 hash 只决定集合桶，不替代 Equals。SourceNotes 的业务顺序与重复内容保持。
ReadRequirements 仍遍历所有正文，核切片／正文／完整 expected 的身份、长度和摘要后才返回；仅保留受限技术元数据。Read 保留一次各切片数组供后续使用，无整档正文再聚合。
流从当前位置处理一份 snapshot，结束检查尾字节，要求调用方提供单份边界；无 Seek／Flush／Close／Dispose 调用方流或真实路径访问。中途 I/O 原样抛出，部分写入交 016 丢弃。

R 在读取新实现前按冻结规格独立计算：CandidateValidation、Player=p、Generation=1、Commit=c、Parent=null、空 Required／Directory／正文、一个本代索引行及五个空要求列表。
头 60＋元数据 68＝128 字节，与 [测试固定常量](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SaveEnvelopeCodecTests.cs:19) 完全相同；完整 SHA256=`3ad60355090274a1067a5480dd1bc8bf89161c97c5951941de012970ae42e0ea`。
固定十六进制如下；本证据不由生产 writer 生成后再喂生产 reader。

```text
464d5341564530310100000044000000000000000000000000000000e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b8550001000000700001000000310100000063000000000000000000000100000001000000310100000063000000000000000000000000000000000000000000000000000000
```

| XML 实际规模观测 | 正文字节 | 元数据字节 | 总字节 |
| --- | --- | --- | --- |
| 1 切片 | 0 | 204 | 264 |
| 10 切片 | 10240 | 1428 | 11728 |
| 100 切片 | 409600 | 14028 | 423688 |
| 1000 切片 | 1000 | 143628 | 144688 |

2 MiB＋1 正文的 ReadRequirements 实测仅一份 8192 字节正文缓冲，复用 257 次；Read 实测保留两份切片数组共 2097153 字节，未创建第二份整档正文。
这些证据验证传给 Stream.Read 的实际数组身份／大小／复用及保留边界，不等于总托管内存峰值测量。字节预算不是玩法回合上限，较大获准预算可重试。

## 6. 接收边界与交接

014 无需返修；015A 无需返修。规范与规格分开核对后，两包各自唯一结论如首页。
015A 的成功只证明技术字节和要求声明相符；业务引用有无漏报、完整 typed 对象图及历史恢复属于 015B，真实提交／耐久及封记属于后续包。技术发布字段不创造 Published 授权。
R 唯一新增本报告，其他文件只读；正式回传 SD00 报告路径／SHA、两项结论及“无用户动作”后结束本原回合停改。
后批放行仍由 SD00 确认本 R 原 turn completed 后办理，本报告不代替该完成门槛。
