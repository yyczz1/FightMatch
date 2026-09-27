# FM-DEMO-018C-1 · 原候选恢复交付

COMPLETED — 本包实现与规定验证已完成。独立审查由 DEMO-B14C1-R1 执行；本报告不代替其 verdict。

## 回合与冻结边界

- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；按 gpt-6-astra／max、原项目 local 派发，无模型降级、ultra、子代理或外部模型。
- 原实施 turn：01a0c920-4203-7cb0-a718-ecf54e7fb831，2026-09-22T12:38:49.632Z 开始，于 13:02:03.669Z failed，error=server_overloaded，原文为 “Selected model is at capacity. Please try a different model.”；无 formal final，未补造 completed。
- 本次准确恢复 turn：01a0c95c-c32d-7bc3-8a79-e47a5ee4cce5，原生 task_started=2026-09-22T13:44:54.866Z。承接同一实施包；R／SD00 在本 formal final 后读取此 turn 的 completed／error=null 作为完成门。
- 原始 baseline.json 保持原字节；恢复时另写 resumed-turn.json，记录当时已有五份 C#、原 540 项全匹配、无 Unity 进程。
- 冻结依据：[system-task-packets.md §201–204](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:3860)，r101 派发全文 SHA256=068ee64d70d6d1180ba5c07a072fb115fb16046e2109d0d3eac2e9f0efa751ed。
- §201–204 经 LF／TrimEnd／一个末尾 LF 规范化 SHA256=7f6bea610bb829ac6b168c8edcb1c7cb2ce6ec7107cf7e7948be195c2e680f34。后续 SD00 派发／恢复登记未改变这些条款；协调稿只读。
- 本文时间均为 2026-09-22 UTC。E = D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/aee5ada2879c4a6aa69e3782119d26a5。

## 基线、范围与实现

从已接收 018B-C1 scope 的完整 540 清单继承，capturedAtUtc=2026-09-22T11:20:03.5448715Z 原样保留；canonical SHA256=fe791423a3b2422c08da18158b37556b747d7c1c19533afac8ba0e3440b592e0。本包真实 startedSnapshot=2026-09-22T12:45:55.2272083Z，不倒填原捕获时间。

仅新增以下五份源码及其五份 Unity 生成的 .meta；原 540 项、原 553 项 Assets、295 份旧 meta、205 项受保护输入保持。没有旧 API／序列化／存储适配器／asmdef／包／设置／权限／场景修改。

| 新文件 | 物理行 | 作用 |
| --- | ---: | --- |
| [SavePendingRecoveryModels.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SavePendingRecoveryModels.cs) | 44 | 两个 sealed 返回模型，internal 构造，冻结元数据、摘要及列表。 |
| [LocalSavePendingRecovery.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSavePendingRecovery.cs) | 233 | 原 LocalSaveStore partial 中的三个指定入口及私有检查。 |
| [SavePendingRecoveryTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryTests.cs) | 786 | 60 个新增用例、真实文件夹具及精确故障适配器。 |
| [SavePendingRecoveryProcessCases.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryProcessCases.cs) | 201 | 固定 P01–P03 writer／reader 及独立身份、字节与结果记录。 |
| [SavePendingRecoveryProcessEntry.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Editor/SavePendingRecoveryProcessEntry.cs) | 10 | 唯一 executeMethod 桥接 Run。 |

生产合计 277／750 行，全部 C# 1274／1950 行；仅收证 evidence.ps1 为 126／280 行。实际枚举：implementation 540→550、Scripts＋Tests 526→536、Assets 553→563、GUID 295→300，300 个 GUID 全唯一。compile-01 导入前 545 项、导入后 550 项：Unity 在该轮测试程序集编译报错前生成了五份允许的 meta，后续逐轮绑定其实际哈希。

ReadUncommittedCandidate 在原 Execute／租约内重核观察、唯一待定组、完整封套、当前头及完整历史前缀，返回前再次核证。ResumeRecoveredCandidate 重读原封套并检查当前、可读 backup、pending 的全部能力，最后才接回原 ticket 和 pending 槽。三入口仅支持 CandidateValidation；已有内存 ticket 时均 Busy。

EndObservedCandidate 允许 RequirementsIncomplete，但必须取得完整字节证据并确认未成。按 MarkerWork→SnapshotWork→未索引 Snapshot 逐项重核剩余目录、当前头／索引与目标长度／SHA，只允许本次成功删除的名称消失。I/O 保留原 Diagnostic／Stage 并返回 EndInterrupted；证据变化和 Limit 保留各自类别。末次删除后也复核；新观察已无该组时返回 CandidateNotFound。

## 实际执行与失败保全

所有 Unity 均由 E/evidence.ps1 使用 Start-Process -WindowStyle Hidden -PassThru 启动。每次先确认无 Unity，记录 PID、exe、完整 argv、cwd、UTC；WaitForExit 后先持久记录真实 Process.ExitCode，再冻结源码、全部 Assets、受保护输入、28 份 DLL／PDB及输出。没有并发或强杀。

| 阶段 | PID | 实际 Unity exit | 进程开始→结束 UTC | 结果 |
| --- | ---: | ---: | --- | --- |
| compile-01 | 18912 | 1 | 13:47:51.7848144→13:48:10.0865945 | 新测试误引用 Platform 内部字段；原日志与源码保留。 |
| compile-02 | 10092 | 0 | 13:49:24.6683796→13:50:26.5820754 | 改用公开 API 检查后的初步编译。 |
| tests-01 | 13864 | 2 | 13:51:25.6313363→13:54:23.0630245 | 3018／3019，零跳过；头变化夹具在 Existing 下初始化，被旧 API 正确拒绝。 |
| compile-03 | 19160 | 0 | 13:55:39.2456870→13:55:57.5169875 | 最终源码编译，无 C# 编译错误。 |
| tests-02 | 39044 | 0 | 13:56:53.5599143→13:59:25.8318802 | 最终 3019／3019 Passed，0 Failed，0 Skipped。 |

两次代码修正仅涉及新测试／进程夹具，生产两文件未因这些失败改写。首次测试改为先建已提交头再制造后继变化。失败日志、XML、当时源码／程序集、原始 stdout／stderr与实际退出均在各 runs/<stage>/ 保留；新一轮执行前还保存已有输出原字节。收证日期转换、OrderedDictionary 聚合及只读命令错误同样保留原事件；final-audit-attempt02.json 的 null 行数统计未用于结论，最终 final-audit.json 已准确重算。

最终 implementation canonical SHA256=d44285fa306efc089446cc4dc81bc9b4458c304bd236a4e462ab8bbf4e1187bb。有效编译后、测试前／后及六次 P 前／后的 28 份 DLL／PDB 完全一致，canonical SHA256=9d6f2482a99a306c4d7c08cd6dbfcffae1df2687528884b545a8628efca45bbb。

实际引擎固定 D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe，项目固定 D:\Unity\UnityProj\FightMatch。RUN — 以下为已执行参数形态，E 按上文绝对路径展开，逐次原 argv 在 runs/<stage>/start.json：

    Unity.exe -batchmode -nographics -quit --b14c1-root E -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14C1Compile.log
    Unity.exe -batchmode -nographics -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB14C1-EditMode.xml --b14c1-root E -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14C1Tests.log
    Unity.exe -batchmode -nographics -quit -executeMethod FightMatch.Tests.SavePendingRecoveryProcessEntry.Run --b14c1-case P0N --b14c1-mode writer或reader --b14c1-root E -projectPath D:\Unity\UnityProj\FightMatch -logFile E\runs\P0N-writer或reader\Unity-process.log

## PR01–PR08 验证结果

| 条目 | 已取得的验证证据 |
| --- | --- |
| PR01 | 完整 work／snapshot／一致双副本 × gen1／后继六组；Dispose／Existing 重开后原 Guid、代际、Parent、OperationIds、Descriptor、全 bytes／SHA保持。真实 018B 六片 Decode 与原操作结果 Lookup；buildEnvelope 不重跑。P01 再验证独立进程。 |
| PR02 | 读取与失败路径磁盘／pending 根不变，Load 仍拒 Pending；外来观察／候选、旧 ticket、文件／目录／头变化、错误历史行、历史 Operation 重用拒绝；返回集合只读。 |
| PR03 | current／backup／pending 缺能力，SourceNotes 顺序、rule／numeric／random／feature／slice 不符均拒绝；共享 Math 校准预算与存储 Limit；Busy、Disposed、完整候选读取 Close 异常保留。 |
| PR04 | 完整、残片、冲突双副本、部分 marker、仅 marker-work、多 Commit、未知项；已提交及索引祖先不可接回／结束，坏正式 marker 不降级。 |
| PR05 | 只结束所选残片组，核对准确名称顺序及当前／历史／其他组原字节；缺组、旧证据、不可读、正式 marker 不触发删除。 |
| PR06 | 三处删除各自前／后 I/O 六组，加中途目标／其他文件／正式 marker 变化；诊断、剩余文件及旧观察失效准确，新观察处理余组；已全删后 CandidateNotFound。 |
| PR07 | 原 2959 用例均保留；旧内存票据结束、显式新建、重入 Busy、Resume 后 SaveFailed／CommitUnknown 仍由同票据解析。独立好／坏业务夹具先 Decode 再 Resume，只作为调用次序见证。 |
| PR08 | 最终编译／全量测试及六次真实进程均 exit0；原 2959 fullname 的 Ordinal 多重集合完整且全 Passed，新增 60 Passed，零跳过；范围、行数、GUID和完整归档全部核验。 |

原基线 FMDemoB14BC1-EditMode.xml SHA256=b7fe984a389ff9686e71a92bb19235238ba9155a969d1f72b6b677ea44dead2a。最终 [FMDemoB14C1-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemoB14C1-EditMode.xml) SHA256=e7890775672bc620e6b7fdba3a33912bc2bfe3c5d9d39a843413a0e6149c37e4。E 中 baseline-test-cases.json／final-test-cases.json／added-test-cases.json 保留逐项结果与重复名称；test-verification.json 记录 Ordinal 计数方法。

## 六次真实进程

| 案例 | writer PID／开始 UTC | reader PID／开始 UTC | 两阶段实际 exit | 最终状态与断言 |
| --- | --- | --- | --- | --- |
| P01 | 21756／14:00:59.8267718 | 33316／14:01:43.8556074 | 0／0 | 完整原候选，原六片 Decode／Lookup，原 ticket 提交，Ready。 |
| P02 | 28788／14:02:32.0600570 | 4052／14:03:21.0416739 | 0／0 | 不完整 work 只读拒绝，显式准确删除，NoSave；再次新观察 CandidateNotFound。 |
| P03 | 37156／14:04:06.7499756 | 21756／14:04:53.4086832 | 0／0 | 原 marker 已成，Lookup／Load 原对象，Read／End 均 AlreadyCommitted，无删除／重建，Ready。 |

每案 writer 由旧 Prepare／Write 加明确故障生成状态，写原 metadata／envelope／intent／nonce 后正常退出。reader 以 Existing 打开，源内具体断言、原文件清单、identity／completed／reader-assertions 文本均保留在 E/P01～P03。外部启动记录与进程内 PID、StartTime ticks、exe、cwd、完整 argv、nonce逐值绑定，每个 reader 都在对应 writer 实际退出后启动。PID 21756 跨 P01 writer／P03 reader 被 Windows 复用，二者不同 start ticks 已明确记录；各案 writer≠reader。此处验证跨进程正常重开，未新增强杀或断电试验。

## 完整证据与自引用边界

原 55,882 个旧 Artifact（2,114,761,704 字节）逐项长度／SHA保持。两轮回归自然新建四个 Guid 根：FMDemoB12/9bd2f8ab26e34ccfa2e9ac7ca586fc6b、FMDemoB12/dbe9c658b0ad47a5b5f43877c03443ca、FMDemoB13/4a6ff3c5fbc94890897ebc2cd52cd93d、FMDemoB13/ae6d5041e1c043f2bdb68580a6699f2e，共 2110 文件，全部归入本包。

归档独立枚举实际源、payload 和 ZIP：21,934 文件、3288 目录（含空目录）、1,128,398,647 源字节；路径／长度／SHA逐项相同，missing=extra=duplicates=0，归档后再核源稳定。[ZIP](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/aee5ada2879c4a6aa69e3782119d26a5/archive/FM-DEMO-018C-1-evidence.zip) 为 251,903,182 字节，SHA256=fbf54f8fbde6998ac78566ee9713dcdacf373d2f690f494e5275a6755b7a0b4a。

[scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-scope.json) 为 621,398／2,097,152 字节，SHA256=13cc5aa76f949a95c326ca9d63797b505f9f667d73ee177869eecfbeb40019ae；原 540 与最终 550 清单直接保留。完整大清单使用 E/archive/complete-evidence.json.br 外置无损注册表，未删行凑限；载体 3,957,531 字节、SHA256=ec9b94fea8b83fd94a0bc3af3a02d61c23a07fc9d8e5173e4cd76864a1ab6841，解压 UTF-8 JSON 85,929,066 字节、SHA256=f52f2b110d6393076447206ff8c7e1e147ea603b343c2a09648463c7f497acb5，已回解验证。全部原始清单也保留在 E 与 archive；baseline／legacy／final audit 随 ZIP 交付。

E/native-command-events.jsonl 包含两个准确 turn 的 62 个原生 completed CommandExecution，另保留 7 个 FileChange 与 3 个生命周期事件，包含原 failed 完成事件；不导出推理或其他回合。原命令、真实 stdout／stderr、wrapper exit 与 Unity actual exit分别可查，不以 chunk_id 或后填数字代替。

明确自引用排除：E/archive/**（payload、ZIP与核验／压缩注册表）、E/late/**、两份事后正式报告。archive/verification.json 独立绑定归档清单及 ZIP，SHA256=3995f7f07fb4db71a5081545b60ab6c903f810fc6eaace02cec94b5c46118a12。归档／报告生成后的原生事件、最终报告哈希及收尾检查另存 E/late，最终 formal final 绑定这些外部证据；不回改已归档源或制造递归自包含。

## 停止范围

源码、测试、正式报告交付后停止修改，只提交 §204 已授权的一次限定通知及其原始结果记录。无 Git 写入、分支、worktree、commit、外部模型、子代理、SDK／包安装或额外 .NET 验证工程。

NOT RUN — 018C-2 的应用单槽／原操作优先／保存后完整发布／QFramework 生命周期／S17 路由；019；正式 PlayerSave／发布；场景与 Demo 体验；新强杀／断电矩阵。PlayerSave 仅做新增入口 UnsupportedBinding 的临时目录拒绝测试，未实现正式业务保存。独立 R verdict 待其按本次恢复 turn 完成门审查。
