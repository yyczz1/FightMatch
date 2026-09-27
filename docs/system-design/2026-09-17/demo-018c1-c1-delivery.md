# FM-DEMO-018C-1-C1 · 普通测试目录纠正交付

状态：COMPLETED。仅处理 R1；独立复审结论由 R 给出。所有时间为 UTC。

## 1. 对象、起点与准确回合

- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1。
- **本次准确原 turn：01a0c9b1-140e-7ed1-b06c-4447a042a023**，是新纠正回合，不是原 018C-1 失败回合的恢复。
- 冻结合同：system-task-packets.md §213～215／派发 r105；派发全文 SHA256=89d96a95bed9e20e5963995766fef2c0bf1a7cb492ad47dacc57f96ba3a3ff99；条款按 LF、TrimEnd、一个末尾 LF 规范化后 SHA256=023b456f50429770e5de0ad6be11cd1b3ba2ae0db3d1df35178e4f818caeb284。
- R1 来源：demo-batch14c1-code-review.md，SHA256=fcfd70d38172ebde301a693f8984eb591d7efd990275aff812a9d85ceea8e8df。已按 receiving-code-review 核实普通 PendingRig→TestRoot 的依赖及 P Writer／Reader 显式目录调用。
- 完整原 550 清单沿用原 capturedAtUtc=2026-09-22T14:08:52.7809325Z，canonical=d44285fa306efc089446cc4dc81bc9b4458c304bd236a4e462ab8bbf4e1187bb；本次真实 startedSnapshot=2026-09-22T15:19:43.9310315Z，两者分开记录。
- 本文 E 为 D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/c682c671374848baa91d6d1a50b1f4c6。冻结 baseline.json SHA256=da2663bfdc9ea33e2ef88931b230e5605eccc52c7eb633242ea76bf5ccd2d83d；源／Assets／受保护输入已复制并逐项核长与 SHA。
- 原 aee5ada2879c4a6aa69e3782119d26a5 根、原两作者报告、R 报告、旧日志及 XML 均只读。三协调稿没有由 C 写入；仅冻结条款不受 SD00 后续登记影响。

## 2. 唯一源码差异与 R1 处置

[SavePendingRecoveryProcessCases.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryProcessCases.cs:41) 仅将 TestRoot 方法体由专用 EvidenceRoot／test-cases 目录构造改为现有入口：

    return NewCase();

原 static using 已引入 LocalSaveTestFiles；既有 NewCase 为每次调用创建全新 Guid 目录，并执行固定项目父目录 containment／无 reparse 策略。普通夹具不再读取本包参数或要求历史 baseline。专用 P01～P03 仍显式使用新 E 与真实 baseline。

- 修改前 SHA256=a775786aabf52cca7b0a38a7354984cd3a8e1d16377d691425be886b11a70ad2；修改后 SHA256=773f37256532b0810d908c19057121f8428e153b76a7e8f5b73c5eee52860345。
- 只增加 1 行、删除 4 行，共 5 行≤20；文件 201→198 行。原文件字节经唯一方法体替换后与实际结果完全一致。
- 签名、using、其余方法及另 549 项实施文件逐字节保持。Run、Argument、EvidenceRoot、Safe、nonce／身份／completed、Writer／Reader 以及原所有测试断言均未改变；没有新增源码或 meta。
- 五个原包 C# 文件共 1271 行≤1950；生产 277 行≤750；本次 evidence.ps1 126 行≤280。
- 最终实施 canonical=af0036b6c2938694a6cf73cbcd171a59db978476860080a83e5372c6075e116a；550 实施／536 Scripts+Tests／563 Assets／300 唯一 GUID 均保持。

## 3. 实际标准命令与普通入口回归

固定 exe=D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；所有 cwd=D:/Unity/UnityProj/FightMatch。以下是传入 Unity 的实际参数，收证脚本的参数另行记录。

    -batchmode -nographics -quit -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14C1C1Compile.log
    -batchmode -nographics -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB14C1C1-EditMode.xml -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14C1C1Tests.log

第二条没有任何 --b14c1-* 参数，也没有 -quit。该实际全量运行通过 3019／3019，失败 0、跳过 0；它是 R1 的普通入口回归验证。没有将静态反例写成已执行红阶段，没有增设测试工程或新 Case。

原与最终 XML 的每个 test-case.fullname 用 StringComparer.Ordinal 计数，完整多重集合一致，遗漏 0、新增 0；每项均为 Passed。两份完整测试名单保存于 E，空新增名单为有效 JSON []。

- 原 XML SHA256=e7890775672bc620e6b7fdba3a33912bc2bfe3c5d9d39a843413a0e6149c37e4。
- 最终 FMDemoB14C1C1-EditMode.xml SHA256=ee1bbd2dff1532050d5e9b35a9ba3b5443a0d775b4f25149d398aff13d5e915a。
- 原 60 项 SavePendingRecoveryTests 的输出记录 63 个互不重复的新目录，均位于 TestArtifacts/FMDemoB12/58a0951a9b064b538111ce74a0d201ee/<GuidN>，没有目录在专用 E 中。
- 另一个自然新根为 TestArtifacts/FMDemoB13/5de1a340889d4b5dac7f45114cbdcb1c；两根 1260 个文件全部逐项纳入归档，旧根没有迁移、覆盖或删除。

## 4. 同一最终版本的八次实际进程

每次均先核无 Unity 进程，以 Hidden／PassThru 启动，WaitForExit 后先保存实际 Process.ExitCode；完整 exe／argv／cwd／PID／UTC、退出记录、stdout／stderr／稳定日志及每轮前后源码／DLL／PDB 均在 E/runs。各进程严格串行，没有强杀或重复有效验证。

| 运行 | PID | 实际开始 UTC | 实际退出 UTC | exit |
| --- | ---: | --- | --- | ---: |
| compile-01 | 40636 | 2026-09-22T15:28:16.0761978Z | 2026-09-22T15:29:00.2757985Z | 0 |
| tests-01 | 35548 | 2026-09-22T15:30:15.5480228Z | 2026-09-22T15:32:28.9056151Z | 0 |
| P01-writer | 21116 | 2026-09-22T15:37:28.7523119Z | 2026-09-22T15:38:11.0075990Z | 0 |
| P01-reader | 26904 | 2026-09-22T15:38:37.3504672Z | 2026-09-22T15:38:48.0224191Z | 0 |
| P02-writer | 7060 | 2026-09-22T15:39:11.9000969Z | 2026-09-22T15:39:22.6813303Z | 0 |
| P02-reader | 2452 | 2026-09-22T15:39:48.5737274Z | 2026-09-22T15:40:06.6191495Z | 0 |
| P03-writer | 8780 | 2026-09-22T15:40:30.7271660Z | 2026-09-22T15:40:43.2966808Z | 0 |
| P03-reader | 39956 | 2026-09-22T15:41:06.9135000Z | 2026-09-22T15:41:17.3560304Z | 0 |

六 P 的 executeMethod 均为 FightMatch.Tests.SavePendingRecoveryProcessEntry.Run，实际参数包含 --b14c1-case P01／P02／P03、--b14c1-mode writer／reader、--b14c1-root 本次 E，以及各自唯一日志。P 专用参数门和真实 baseline 前置条件保持。

每轮前后源码 canonical 均为最终 af0036…；编译后的 28 个 DLL／PDB canonical=69abe160d2e8e46872f48e686e3e0daf1a66cc881fc37f28bf8766a149e13eb1，与后续七次启动前后完全一致。编译前的旧程序集快照也完整保留，不把它写成编译后的程序集。

| Case | writer／reader PID | nonce | 最终状态 | reader build |
| --- | --- | --- | --- | ---: |
| P01 | 21116／26904 | 1715fb95d3c24848b54f45a0930d69ce | Ready | 0 |
| P02 | 7060／2452 | 1627508427dc46659d6efe5ab94b5c1f | NoSave | 0 |
| P03 | 8780／39956 | 353d92950dcf40e0aaf7b606183913ad | Ready | 0 |

每组 writer／reader 的真实 PID 不同，StartTime ticks、nonce、exe、cwd、完整 argv 与外部记录绑定；original-envelope.bin 与原元数据 SHA 完全一致，reader assertions 及双方 completed 标记齐全。原身份、完整字节及所有既有断言通过，详细 commit／SHA／文件列表在 final-audit.json。

## 5. 验收与完整证据

| 条件 | 作者验证结果 |
| --- | --- |
| C1-01 | PASS：标准无专用参数的全量 3019 项原 fullname 多重集合全过，无失败／跳过／遗漏；普通 60 项使用既有 B12 全新目录。 |
| C1-02 | PASS：仅 TestRoot 方法体 +1/-4；其余文本、另 549 实施、全部 meta 及专用门保持。 |
| C1-03 | PASS：同版六 P 真实 exit0，原身份／完整字节通过，全部 reader build=0。 |
| C1-04 | PASS：550／563／300 保持，同版编译／全量／六 P 与 28 程序集；完整归档三方路径、长度、SHA 及目录集合一致。 |

旧 Artifacts 的 98,964 个文件已按原路径、长度、SHA 全量重核，canonical=bd461b83e50f3ba18974160ca28a8741ae5fda2a7edd83058d8a45389a7ed9f7；211 项受保护输入保持。旧大归档不递归复制，新运行证据没有删行或用汇总替代原清单。

实际源／payload／ZIP 分别独立枚举 15,948 个文件，源长度合计 963,710,673 字节；全部 2,001 个目录（含空目录）一致，missing=extra=duplicates=0。源文件在归档后复核保持。ZIP=E/archive/FM-DEMO-018C-1-C1-evidence.zip，203,234,596 字节，SHA256=f83283653f18bd9b0035c95b2674b55d97dfae20dff5cc3a38d462f84a1e1a87；archive/verification.json SHA256=86cd5b6adfb8b3140cf3e712d79722d9282b118f696e507e09ef6d9252a8d5e0。

scope 保留原550与最终550完整清单、原捕获时间及真实新 start。完整大登记表以标准 Brotli 无损外置于 E/archive/complete-evidence.json.br：压缩 4963507 字节／SHA256=c5ed336ba24a6fd0326463d316f1acff934cc8d72180b228d3dfb36853dce269；解压 123646186 字节／SHA256=8ba7084437c36d7a10cddda327404f7bb4810eca05fb4a692f868fd8a1ece02b。已解压回读核长与 SHA，未截断任何清单行。

原生命令／FileChange／生命周期只导出本次准确 turn，原始事件保存。原生序号 6191、6235 的只读 Get-Process 查询 exit1 完整保留；它们不是 Unity 失败。本次八次 Unity 验证无失败。零新增测试的首次空管道产生零字节清单，已规范化为 []，原空字节与原因保存在 test-manifest-normalization.json，未重跑测试。

自引用排除明确：E/archive/** 不能递归归档自身；E/late/** 和归档后生成的两份正式报告通过事后原生事件及最终绑定收证。最终报告 SHA 由 E/late/final-gate.json、formal-bindings.json 与 formal final 锚定；最后绑定命令及未来 task_complete 由 R／SD00 直接读取原生记录，不伪造当前回合已 completed。

## 6. 正式交付、限制与停改

- 本交付：[demo-018c1-c1-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-c1-delivery.md)。
- 完整 scope：[demo-018c1-c1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-c1-scope.json)，573301 字节／SHA256=954d76d6d25216483487d0ac0d968f876d594138e4f5bc0bf761f18aa2475e73。
- NOT RUN：纠正前另一次执行红阶段、交互式 Editor Test Runner、018C-2、019、新业务实现、正式 PlayerSave／发布／Demo 体验、新强杀或掉电矩阵、额外验证工程、SDK／依赖安装、独立 R 的 C1 verdict。
- 本次没有 Git 写操作、子代理或外部模型。源码已停改；formal final 后不继续实现或扩展范围。
- 按 §215 向 SD00／R 各一次限定通知，仅交本包号、两报告路径／SHA、准确原 turn 与停改摘要；真实工具结果另存 E/late。通知不代替准确 turn completed，后续独立复审不由 C 自判。
