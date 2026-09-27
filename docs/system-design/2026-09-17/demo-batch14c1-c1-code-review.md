# DEMO-B14C1-C1-R1 · FM-DEMO-018C-1-C1 独立复审

VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS
Verification: R 独立核对实际源码、完整 XML、八次真实进程及其版本快照、原生记录和完整归档；未运行 Unity。
Notes: 无未关闭的实质问题；已执行范围和 NOT RUN 见下文。

**R1 已关闭。** 普通 PendingRig 的默认目录不再依赖专用参数或历史 baseline；本结论仅接收 018C-1 的本次 C1 纠正。没有新增待修项。生产字节未变部分引用[原 R1 报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-batch14c1-code-review.md)，不扩大生产复审。

## 1. 合同、对象与完成门

所有时刻均为 2026-09-22 UTC；本地日期跨入 2026-09-23。仓库为 D:/Unity/UnityProj/FightMatch。
- 冻结依据：[system-task-packets.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md) §213～215；派发 r105 全文 SHA256=`89d96a95bed9e20e5963995766fef2c0bf1a7cb492ad47dacc57f96ba3a3ff99`。
- 条款按 LF／TrimEnd／单末尾 LF 规范化 SHA256=`023b456f50429770e5de0ad6be11cd1b3ba2ae0db3d1df35178e4f818caeb284`；R 在 16:05:21 再核保持。
- C：FightMatch 本机保存与应用接入实现，task=`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`，准确 turn=`01a0c9b1-140e-7ed1-b06c-4447a042a023`。
- C 原生 task_started=15:17:00.601Z／ordinal6155；task_complete=15:56:42.663Z／ordinal6546。wait_threads 对该准确 turn 确认为 completed／error=null；原生 formal final 中两报告及最终绑定 SHA 与实物相同。完成门已满足。
- R：FightMatch Demo 独立代码审查，task=`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`，准确 turn=`01a0c9b1-c77e-7782-a778-9c69594e736e`；原生开始 15:17:46.529Z／ordinal12403。此报告不预写 R 当前回合 completed。
- E=`D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/c682c671374848baa91d6d1a50b1f4c6`；下文 E 内文件均指该绝对目录。

| 正式材料 | 实测长度／行数 | SHA256 |
| --- | --- | --- |
| [C delivery](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-c1-delivery.md) | 10680 bytes／97 行 | `958ab725af66bb72ba9272c24609a4fcfd0ad07b91e52e3cf25b2616610a2482` |
| [C scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-c1-scope.json) | 573301 bytes，≤2048 KiB | `954d76d6d25216483487d0ac0d968f876d594138e4f5bc0bf761f18aa2475e73` |
| [formal-bindings.json](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/c682c671374848baa91d6d1a50b1f4c6/late/formal-bindings.json) | 8569 bytes | `21109867ebb9d2d696984055be87b7685140c2628a7eacb4b6158b76d216ff77` |
| 原 R1 报告 | 20169 bytes／151 行，未变 | `fcfd70d38172ebde301a693f8984eb591d7efd990275aff812a9d85ceea8e8df` |

## 2. 规范／规格两轴与 R1 关闭依据

| 轴 | 独立检查结论 |
| --- | --- |
| 规范 | PASS：唯一实施差异在获准 TestRoot 方法体；另549项、签名、using、其余原文本、meta及原断言逐字节保持。没有新抽象、参数解析、回退、测试Case或配置变更。 |
| 规格 | PASS：直接复用已有 NewCase 的安全目录策略；标准全量3019项原多重集合全过，专用门保持，编译／全量／六P同版，完整证据与真实完成门成立。 |

[SavePendingRecoveryProcessCases.cs:43](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryProcessCases.cs:43) 的方法体现在仅为 `return NewCase();`。R 以入场原文件执行唯一文本替换，与实际完整文件字节一致：+1/-4，共5行≤20，201→198行，13135 bytes。目标 SHA 从 `a775786aabf52cca7b0a38a7354984cd3a8e1d16377d691425be886b11a70ad2` 变为 `773f37256532b0810d908c19057121f8428e153b76a7e8f5b73c5eee52860345`。

既有 static using 已引入 LocalSaveTestFiles。[LocalSaveStoreTests.cs:431](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs:431) 的 NewCase 每次新建 GuidN 子目录，沿固定项目下 B12 本轮新根执行 GetFullPath、containment、祖先无 reparse 检查并记录实际路径。普通 PendingRig 不再调用专用 EvidenceRoot；Writer／Reader 显式目录调用保持。Run、Argument、EvidenceRoot、Safe、nonce／身份／completed及全部测试断言原文未变。

目标 meta SHA 保持 `40df0a43d4e7145306a4f168785fe6431c984d4a076763ec495c99625dd9336e`。五个原包 C# 合计1271行≤1950，生产277行≤750；本次收证 helper 126行≤280。没有压行扩张范围。纠正可作为唯一方法体 hunk 独立回退；本次未执行回退或 Git 写入。

| 验收项 | R 独立结论及证据 |
| --- | --- |
| C1-01 | PASS：实际 Unity argv 不含任何 --b14c1-* 或 -quit；原3019项 fullname 多重集合逐项 Passed，遗漏／新增／失败／跳过均0；60项普通测试使用63个新 B12 目录。 |
| C1-02 | PASS：完整原文件仅上述方法体改变，另549项 SHA 原样；六P仍经过真实 baseline、专用参数、路径、nonce及身份门。 |
| C1-03 | PASS：六P为不同真实进程并串行退出，源码／DLL／PDB同版、身份及完整字节绑定，全部 exit0、reader build=0。 |
| C1-04 | PASS：550实施／563 Assets／300唯一GUID保持；归档源／payload／ZIP的全部文件路径、长度、SHA及目录集合一致；原生和事后绑定可核验。 |

## 3. 标准全量与八次真实进程

R 从实际 Unity 日志读取的全量 argv，与外部 start.json 一致：

```text
D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe -batchmode -nographics -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB14C1C1-EditMode.xml -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14C1C1Tests.log
```

该标准运行就是 R1 回归证据。R 解析前后 XML 的每个 test-case，按 Ordinal 统计 fullname 多重集合：3019项／3014个不同 fullname 完全一致，全部 Passed；不是仅对总数或作者汇总。最终 XML 为2072269 bytes，SHA256=`ee1bbd2dff1532050d5e9b35a9ba3b5443a0d775b4f25149d398aff13d5e915a`，与 runs/tests-01 的复制品一致；旧 XML SHA=`e7890775672bc620e6b7fdba3a33912bc2bfe3c5d9d39a843413a0e6149c37e4`。

普通60项的 XML CDATA 输出与63个实际唯一目录对应，均在本轮新 B12 根 `58a0951a9b064b538111ce74a0d201ee` 下；另有 B13 新根 `5de1a340889d4b5dac7f45114cbdcb1c`。两根均不在入场旧根集合，合计1260个自然输出文件完整归档，普通目录不依赖 E 的 baseline。

固定 exe SHA256=`ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895`。R 核对每轮启动前无 Unity、Hidden／PassThru、完整 argv／cwd／PID／UTC、WaitForExit 后立即保存实际 ExitCode、稳定日志与原生命令。全部严格串行，Unity 及对应执行命令均 exit0。

| stage | PID | start UTC | exit UTC | exit |
| --- | ---: | --- | --- | ---: |
| compile-01 | 40636 | 15:28:16.0761978Z | 15:29:00.2757985Z | 0 |
| tests-01 | 35548 | 15:30:15.5480228Z | 15:32:28.9056151Z | 0 |
| P01-writer | 21116 | 15:37:28.7523119Z | 15:38:11.0075990Z | 0 |
| P01-reader | 26904 | 15:38:37.3504672Z | 15:38:48.0224191Z | 0 |
| P02-writer | 7060 | 15:39:11.9000969Z | 15:39:22.6813303Z | 0 |
| P02-reader | 2452 | 15:39:48.5737274Z | 15:40:06.6191495Z | 0 |
| P03-writer | 8780 | 15:40:30.7271660Z | 15:40:43.2966808Z | 0 |
| P03-reader | 39956 | 15:41:06.9135000Z | 15:41:17.3560304Z | 0 |

对应原生命令：编译 `exec-6ed2aa13-8624-4456-a6f2-38aff67001cf`；全量 `exec-fd38f14b-ff59-4628-af03-140cef23eb5f`；六P串行命令 `exec-505859fa-b2b4-4175-a4a1-204ca895e989`。六P均使用原 executeMethod 和各自 root／case／mode 参数及唯一日志，实际内部 argv 与外部记录一致。

R 实查16份 before／after 快照，各806个实际复制文件的路径／长度／SHA均对应四类清单，无缺漏／额外。八轮源码均为最终550 canonical `af0036b6c2938694a6cf73cbcd171a59db978476860080a83e5372c6075e116a`。编译后的28个 DLL／PDB共2770444 bytes，canonical=`69abe160d2e8e46872f48e686e3e0daf1a66cc881fc37f28bf8766a149e13eb1`，等于后续七轮前后及实际当前程序集。编译前旧程序集快照保留，未冒充最终 DLL。

| P | 独立完整字节／恢复检查 | 最终状态 |
| --- | --- | --- |
| P01 | commit=8c433a244add4972983a5f4773034d98；9941-byte writer work与reader已提交snapshot相同，SHA=`31204f1327e2799654881c649c8e49cb46ef73c6f231dc85ffed2e0b4c0af470`；276-byte marker存在。 | Ready，build=0 |
| P02 | commit=cea87064538a42968e220e4d50a88e89；9941-byte原件SHA=`d19847cabb46114175ddf6eb7068a8208c9bd891216995828e2f38fd6a2fc1e3`；真实30-byte片段等于原件前30字节，SHA=`c3de65b09de17b810c28435e7a7d7bbf5e49a3ff2eccd84f6132812265cb4042`；reader候选文件清除（锁文件除外）。 | NoSave，build=0 |
| P03 | commit=983fa1f0d050401b9a70fda6487fb08a；前后9941-byte snapshot及276-byte marker完整字节保持，snapshot SHA=`70a195e51a65c3e849d189f891f254890d3624f9aa3bcb0e5d6fc41b20401335`。 | Ready，build=0 |

各组 writer／reader PID与start ticks不同，reader启动晚于writer实际退出；nonce、exe、cwd、NUL分隔后编码的完整argv、identity和双方completed均与外部记录绑定。R检查原封套、元数据及实际文件，未只信 completed。原 Decode-before-Resume、marker查询／加载、AlreadyCommitted拒绝及新candidate NotFound断言保持。

## 4. 起点、受保护范围与归档

R 15:19:30.1488849Z入场快照早于源码修改；原550 canonical=`d44285fa306efc089446cc4dc81bc9b4458c304bd236a4e462ab8bbf4e1187bb`。C保留原 capturedAtUtc=14:08:52.7809325Z，另记本次真实 startedSnapshot=15:19:43.9310315Z。完整原清单及新起点相互独立，未把历史捕获时间改为本轮时间。

| R 实测范围 | 前后结论／canonical SHA256 |
| --- | --- |
| 另549实施文件 | 全部路径／长度／SHA保持；`dc7b768660a55b37b4cf90505b76fd71d40ef0b1889f276a2dc607ee9b6f896f` |
| 最终550实施 | 2930575 bytes；`af0036b6c2938694a6cf73cbcd171a59db978476860080a83e5372c6075e116a` |
| 最终563 Assets | 536 Scripts+Tests、300 meta／300唯一GUID；`83f89418e74ff7205b0d42a3b5009f382ebb2b76cb5b9e9d5e7ffced166b4d74` |
| 36个旧 Artifacts 根 | 98964文件／4611731584 bytes逐项保持；入场与完成后全量复核一致，后者16:01:25.6836787Z结束；`bd461b83e50f3ba18974160ca28a8741ae5fda2a7edd83058d8a45389a7ed9f7` |
| R保护清单463项 | 文档、三协调稿、.agent、Packages、ProjectSettings、规则／权限等均保持；15:57:35复核；`2d34103e9a5a2ae5dcb766e8b8f5fdb1a0b218b9c78f368c7be7bd0718bbbeac` |
| 旧日志／XML／tools输出117项 | 41207134 bytes全部保持；16:15:45.8780895Z复核；`2851b89ea49357027eb0ae54648ec5051f87f38f382ea03911248ffdbe7dedc1` |

C自己的211项受保护输入也与实际及快照一致；R保护清单排除本包两份新作者报告与本份新R报告。16:15:44只读Git status仍有34个已有tracked差异，diff --check无输出、exit0；仓库不是clean，C1归属以入场字节清单及实际原生修改记录判定，没有把此前工作算成本次改动。

R于15:50:28～15:51:15独立枚举并散列实际源／payload／ZIP：各15948文件，展开总长963710673 bytes，各2001目录含空目录，ZIP15950 entries；路径、长度、SHA、目录集合完全一致，缺失／额外／重复／大小写重复／不安全路径均0。三方canonical=`dc5e6ff2c825d1a2d980a318fa1ae896e77831650ff96657a3fe678e5dcf8f99`。下述收证恢复后，R于16:16:35～16:16:57再次全量重枚举实际源：同15948文件／2001目录、长度及canonical均保持；ZIP实测SHA保持。

[FM-DEMO-018C-1-C1-evidence.zip](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/c682c671374848baa91d6d1a50b1f4c6/archive/FM-DEMO-018C-1-C1-evidence.zip)：203234596 bytes，SHA256=`f83283653f18bd9b0035c95b2674b55d97dfae20dff5cc3a38d462f84a1e1a87`。E/archive/verification.json SHA=`86cd5b6adfb8b3140cf3e712d79722d9282b118f696e507e09ef6d9252a8d5e0`。

R实际解压 complete-evidence.json.br：压缩4963507 bytes／SHA=`c5ed336ba24a6fd0326463d316f1acff934cc8d72180b228d3dfb36853dce269`；解压123646186 bytes／SHA=`8ba7084437c36d7a10cddda327404f7bb4810eca05fb4a692f868fd8a1ece02b`。14个完整JSON区段逐项规范化后等于实际来源，包括原98964清单、起点／最终审核、测试名单、普通目录、原生与三方归档全部清单，非截断汇总。

自引用排除为E/archive/**、E/late/**及事后两份正式报告；旧Artifacts核原字节而不递归塞入新ZIP。新自然输出完整归档。late实际18文件＝绑定所列17项＋formal-bindings本身，17项逐个长度／SHA吻合，canonical=`77e4395e5c882dee146d5e98a1b3c4ea5d19d8b0b5937e39dde85d087e8f11b3`；绑定自身由C原生formal final锚定。

## 5. 收证失败、恢复与原生证据

本次八次Unity没有失败。下列收证问题均保留原输出并闭合，不能写成全部收证命令首次成功：

| 事件 | 实际记录与闭合 |
| --- | --- |
| 原生ordinal6191／6235 | 只读Get-Process查询exit1完整保留，不是Unity退出失败。 |
| 空新增测试名单 | 首次空管道产生0-byte文件；记录原空字节及原因后规范化为JSON []，未增加Case或重跑测试。 |
| final-gate首次失败 | `exec-dd0fa393-d82a-4a44-af98-6b775cc7e14d` 因空late目录的路径列表报错；final-gate-attempt01.json保留，明确处理空清单后的 `exec-0b8bd209-8898-49c1-9c12-b4efe1ad9c3e` exit0。 |
| late导出路径错误 | `exec-59bc4a0b-7c94-459e-983f-0ed569337a23` 将两个元数据写回主E；后续 `exec-a6ceef79-87a8-4cc6-817d-6f2dd59bfd13` 因late/native-export缺失exit1。错误输出、实际848183-byte late命令导出及失败JSON完整保存在late。 |
| 元数据恢复 | `exec-847b6d0e-d7e6-48c8-a3ac-f0edf8c2e601` 先保留恢复前副本，再从已核SHA的归档payload恢复主E的native-file-change-events.jsonl与native-export.json；R实测失败副本、恢复文件和payload字节吻合。其余主E原生文件字节原样。修正导出及事后门均exit0，最终全源复核通过。 |

恢复没有修改源码、正式作者报告、payload、ZIP或旧Artifacts。final-gate.json SHA=`d55a5aaf39f5aed33b533e003fb3eaf65b15054718d138fa3190eda1e13eb6f2`；final-gate-after-export.json SHA=`243c8ccb676eb72b34dcd2405ca385e19d8847868acaa628d5b5171c25737c6e`。

R直接读取准确C turn的原生会话：36条completed CommandExecution、4条FileChange及start／complete生命周期；完整查看所有36条命令，35条已导出的不同命令与原生ordinal／时间／payload逐项一致，失败副本内事件也相符。最后自绑定命令 `exec-4687754d-fc02-42dc-aef0-1f2c09ce77ae` 与未来task_complete明示自引用排除，R直接核原生补足；没有伪造其已被自身导出。

C向SD00与R各一次通知均被自动审批拒绝，理由为目标所有权／信任及内部路径、哈希、任务信息披露授权未获确认；未重试。R于16:16:36核两条McpToolCall原生失败记录、late原文及actual-tool-results.json，参数和错误内容完全一致。按§215以formal final及准确completed回退收件，通知失败不构成源码待修。

## 6. 范围限制与接收动作

NOT RUN：R未运行Unity、项目测试或额外探针；未执行纠正前另一次红阶段、交互式Editor Test Runner、Player构建／发布、强杀／掉电矩阵或Demo体验。标准batch回归、正常真实进程退出／再开已有实际证据；不将未运行项记为通过。

R只新建本报告，未改作者代码／报告、未创建子代理或调用外部模型、未写Git。规范与规格审查均已闭合，无需另签纠正包。

SD00须在本次准确R turn正式completed后接收本结论，记录R1关闭及018C-1接收，并以本报告的最终550 canonical更新下一包起点；之后另签018C-2精确包。本结论不自动接收018整包、019、PlayerSave或Demo可体验完成。R正式final交本报告绝对路径／SHA与准确turn后停改；限定通知不能替代该完成门。

