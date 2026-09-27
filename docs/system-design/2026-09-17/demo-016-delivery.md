# FM-DEMO-016 / DEMO-B12 实施交付

状态：实施及同版验证完成，等待 R 独立审查；本报告不代替 R 的 ACCEPT。

- 实施任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`，local，gpt-6-astra / max。
- 本次原实施 turn：`01a0c51f-7ccd-73c3-84cb-ddd4eaa629be`；以本任务正常 final 结束后的 completed 为正式审查门槛。
- 合同：r81 `system-task-packets.md` §§143–148；派发 SHA `97002e90ce13fd2a8b94b68432f9ee7796b9cd2e630d83cadac7aef31d622e8d`。开始读取整稿 SHA 已在 scope 保留，后续状态记录不改变业务白名单。
- 前置：015B C1 R2 唯一 ACCEPT，报告 SHA `ea960a290d5b4394a58e143060d111d71c1cef0545cce9aab7ea6daadf3e5aed`。
- 完整清单、字节证据、实验归属和原命令：同目录 `demo-016-scope.json`。

## 结果与边界

新增纯 C# `FightMatch.Platform` 程序集，仅引用 `FightMatch.Core`。提供真实 Windows 本机文件适配、排他租约、冻结提交票据、snapshot/marker 写入与完整回读、唯一头观察、原 Commit/Operation 查询和明确的失败分类。

Open 的 Opened 只表示取得诊断会话。Prepare 每次重核当前头和未决证据；编码回调只运行一次。Write 在最终 marker 提升开始前后分别处理 SaveFailed、RecoveryBlocked、CommitUnknown，只有最终完整重查确认才返回 Committed。旧 ticket 重试查询原提交，并返回最新头。EndUncommitted 仅处理本 store 的原未提交票据。

证据范围是 **同进程真实 Windows 文件、故障注入与释放租约后重开**。未验证独立进程强杀、Android 掉电、M02 发布或首 Demo 可玩；未开展 017/018，也未把技术提交回执解释为玩家 Completed。

## 七项验收证据

| 项 | 作者验证结果与可复核见证 |
| --- | --- |
| ① 固定格式与隔离 | 通过。`SaveCommitMarkerTests.FixedFullSnapshotAndMarkerBytesAreReadableWithoutEitherProductionWriter` 使用独立固定完整字节，经公开 Open/Inspect/Lookup 读取；`ThreeWrittenEmptyEnvelopesAndMarkersMatchIndependentByteRulesIncludingOldDigests` 逐字段独立构造三代写出字节。16 种 marker 破坏、7 种非规范整数词法拒绝；Candidate/Player 及原始 UTF-16（含孤立代理、NUL、空格）物理隔离。 |
| ② 一次接纳与稳定票据 | 通过。`LocalSaveStoreTests.ThreeRealBusinessGenerationsRoundTripAndOldKeysUseTheLatestIndexAfterReopen` 对真实 015B 五片候选执行 Begin/Attack/Rollback 三代写盘，经 015A 回读、015B Decode 核状态及五片字节。回调计数为一次；可变列表/摘要防御复制；8 种元数据/索引修改拒绝且不落候选文件。空操作初始化合法，重复/已有操作拒绝。 |
| ③ 真实提交及原键查询 | 通过。上述三代用例释放真实锁后同进程重开；删去第一代物理副本后，仍由第三代自足索引返回原 Descriptor 和当前头。旧成功 ticket 重试不增加写调用；两键冲突、异 store、已结束和 Disposed 门槛分别覆盖。 |
| ④ 故障分界及同票据恢复 | 通过。`RealFileFaultsKeepTheSameTicketBytesAndClassifyThePublicationBoundary` 实际运行 **55 个参数化用例**，包装真实文件注入 snapshot/marker Create、Write、短写/中途抛错、Flush、Close、校验读、Promote 和最终枚举/读取前后故障。验证发布前完整旧头可证才 SaveFailed，marker 提升前/后抛均先 CommitUnknown，随后按原键查明；同 ticket 重试原字节、身份、业务随机状态和一次回调保持。另测最终回读 Math 预算耗尽、旧头核对不完整和删除中断。 |
| ⑤ 排他与重入 | 通过。`LeaseIsARealExclusiveHandleAndCapabilitiesAreCheckedBeforeWriting` 证明真实 FileShare.None 排他、第二 store Busy、Dispose 后重新取得遗留锁文件；PowerLossDurable 拒绝时存储调用为零。`CallbackReentryConcurrencyExceptionAndCodecRejectionReleaseOnlyPreparingState` 覆盖回调重入、并发、异常及预算拒绝后再次 Prepare；Existing 无目录不创建，空目录不自动初始化，过时 expectedHead 拒绝。 |
| ⑥ 唯一头及未决处置 | 通过。`DamagedOrUnattributedFinalEvidenceNeverSelectsAnOlderHead` 覆盖最高正文缺失、坏封记、未知封记/目录、同代分叉、独立高代；均 RecoveryBlocked 且 Current 为空。已索引旧坏副本仅降低冗余，完整旧临时副本可归属；陌生残片保留并封新写。目录/marker/Math 低预算与枚举失败保留诊断会话，更大预算可查原证据。End 仅删本票据获准候选，Unknown/Committed 最终封记和其他候选不删除。 |
| ⑦ 范围与验证 | 通过。19 个新增实施文件、唯一旧 asmdef 仅 +1 行；原 470 其他文件及 260 meta 不变，最终 490 文件、270 唯一 GUID。最终同版实际 Unity 编译 exit 0、全量 EditMode exit 0，2640 全过；原 2525 的 Ordinal fullname 重数及逐名 Passed 保持，新增 115 全过。原 CommandExecution、辅助检查失败、完整被覆盖产物与实验文件清单可追溯。 |

## 固定格式见证

| 独立样本 | 字节数 | 完整文件 SHA-256 |
| --- | ---: | --- |
| 最小 FMSAVE01 snapshot | 252 | `6de3d0102b5f261399b5355eb454f13d7a84c4caf27e7158498bcc67b4a98041` |
| 对应 FMCMT001 marker | 256 | `dc0cc1dfa707b0cb8c5610f5e386b9748663625b8688b2494dcf4b9cb8847e9e` |

固定 Commit 为 `00000000000000000000000000000001`，Player 为 `p`，CandidateValidation，第 1 代，无正文、无操作。生产写入的随机 Commit 来自 ticket，独立写出见证按公开字段规则核对；不为测试开放 Commit 分配。

## 文件与冻结

新增 6 个平台源码：`LocalSaveModels.cs`、`LocalSaveStore.cs`、`SaveCommitMarkerCodec.cs`、`SaveHeadInspection.cs`、`ILocalSaveStorage.cs`、`WindowsEditorSaveStorage.cs`，均在 `Assets/Scripts/FightMatch/Platform/`。

新增 2 个测试源码：`Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs`、`SaveCommitMarkerTests.cs`；新增纯平台 asmdef，以及 Unity 导入生成的 10 份 meta。19 个精确路径及逐文件增删行数全部在 scope。

唯一旧文件 `Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef` 增加 `FightMatch.Platform` 引用，553 → 584 字节，原 LF 换行保持。移除该唯一新增行后 SHA 恢复为 `829acdb13ac53136202aaf85b7e9cc5572085e720bb78f149a649ae63dfd66e5`。原完整字节已归档。

实施新增/删除合计 **1853 行 / 3600 行上限**，旧 asmdef +1/-0。Core API/格式/友元、旧测试、Packages、ProjectSettings、权限配置及原 C1 证据保持；无 Git 写入、分支/worktree、新任务或子代理。

- 继承 before：471 文件，原 capturedAtUtc=`2026-09-21T17:09:01.0995492Z`；规范 SHA `2e447f19aabed033939d88d90c3b712c8cf0505fb19f4ec9d089db45d7bd1b99`。
- 本批 startedSnapshot：`2026-09-21T18:02:42.6722216Z`，471 文件逐项匹配继承基线。
- 唯一源码冻结 B12-F1：10 个手工源码/配置，`2026-09-21T18:31:32.2151205Z`；规范 SHA `3ee913061417319cde0674e0fa13939e31d3dab80d1739bf5582f75cf7857750`。
- 导入后及两次测试前后：490 文件规范 SHA `4a88037f6d67b95e3ab46faf3d176d7bb7cce0ab8dd7166efecd1b37bc593c1d`；手工冻结未变化。

## 实际 Unity 验证

固定可执行程序：`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`，ProductVersion=`2022.3.18f1_d29bea25151d`。每次启动前无其他 Unity；使用 Hidden / PassThru / Wait，WaitForExit 后读取真实 PID 与 Process.ExitCode。

| 运行 | PID | UTC 开始 → 结束 | Unity 退出码 | 结果 |
| --- | ---: | --- | ---: | --- |
| B12-COMPILE-1 | 33864 | 18:31:51.9635511 → 18:32:23.9896150，2026-09-21 | 0 | 导入/编译通过，480 → 490，新增 10 meta |
| B12-TEST-1 | 17804 | 18:33:07.0438671 → 18:33:54.7444076，2026-09-21 | 0 | 2640/2640；完整日志/XML 覆盖前无损归档 |
| B12-TEST-2（最终） | 23044 | 18:35:15.8813573 → 18:36:03.3413074，2026-09-21 | 0 | 2640/2640，failed/skipped/inconclusive 均 0 |

编译参数为 `-batchmode -nographics -quit -projectPath` 绝对项目路径及 `-logFile` 绝对编译日志路径。测试参数为 `-batchmode -nographics -projectPath` 同项目，`-runTests -testPlatform EditMode -testResults` 绝对 XML 路径及 `-logFile` 绝对测试日志路径，不含 `-quit`。scope 保留实际引用参数和未裁剪原 CommandExecution。

| 最终自然产物 | 字节数 | SHA-256 |
| --- | ---: | --- |
| `Logs/FMDemoB12Compile.log` | 65917 | `4301c63c407217ce48f5129681613ab5dc96a3121164d95f07bf4a16a2ef543f` |
| `Logs/FMDemoB12Tests.log` | 89027 | `7a1bb2016d3719071021cc507c47c00c2b5ffa1ffbcf62897ccdab296ff97b29` |
| `FMDemoB12-EditMode.xml` | 1765962 | `888092f8871282eb309c4209325cde94383f2b2917e9f40f8c8779af4e35859d` |

完整最终产物留在上述原路径；首轮完整日志/XML 在 scope.productArchives 以 Brotli+Base64 保留，已解压复核长度和 SHA。没有手工改写自然产物。

## 检查修正与现场保留

首次 GUID 辅助检查错误地把 Scripts/Tests 内 259 份 meta 与全 Assets 应有的 270 比较；当时首轮测试已开始。修正为全 Assets 后，确认 270 GUID 唯一、原 260 不变，再执行最终全量 TEST-2；首轮并非测试失败，仍完整归档。

最终证据整理时，旧 asmdef 检查误用 CRLF（实际为 LF），以及实验 XML 检查误读节点而非 InnerText，均导致辅助检查失败；仅修正检查表达式后通过，源码未变。其他只读检索无匹配/通配写法及一次 PowerShell 诊断语法错误也保留原失败命令。Unity 编译和两次测试均 exit 0。

两个新建实验根均保留，每根 116 个独占用例目录、545 个最终文件：

- TEST-1：`TestArtifacts/FMDemoB12/133da444453b41fb94f9c6db1a9a0041/`。
- TEST-2：`TestArtifacts/FMDemoB12/9e1e5826674e407aabd3c9f637c06600/`。

共 1090 个文件的相对路径、长度、SHA 及用例 fullname 归属在 scope.experiments.manifest；规范 SHA=`39dba4bd1539b930c4103ef525910e83483bfe38ccd56e18b48e8b4b6e2f7a9f`。每次测试生成独立 GuidN 根；写入/单文件删除前校验绝对路径与 reparse，未清理父目录、系统 temp 或玩家实际保存资料。

本次 final 后停改。R 读取正式交付和本原 turn 的 completed，再按七项与原证据独立审查；用户无需补充业务决定，下一阶段由 SD00 在审查门槛满足后派发。
