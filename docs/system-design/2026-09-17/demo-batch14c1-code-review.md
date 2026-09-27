# DEMO-B14C1-R1 · FM-DEMO-018C-1 独立代码审查

VERDICT: NEEDS_FIX

审查日期：2026-09-22；以下时间均为 UTC。唯一待修项 R1 是普通 EditMode 测试对专用进程证据参数的依赖。三个生产入口及已交付验证证据未发现另一个可确认缺陷；本结论只覆盖 018C-1。

## 1. 审查对象与完成门

- 冻结合同：`system-task-packets.md` §201～204；派发 r101 全文 SHA256=`068ee64d70d6d1180ba5c07a072fb115fb16046e2109d0d3eac2e9f0efa751ed`。
- §201～204 以 LF、TrimEnd、一个末尾 LF 规范化后的 SHA256=`7f6bea610bb829ac6b168c8edcb1c7cb2ce6ec7107cf7e7948be195c2e680f34`；R 入口、恢复及最终复核一致。
- C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`。原实施 turn=`01a0c920-4203-7cb0-a718-ecf54e7fb831`，12:38:49.632 开始，13:02:03.669 因 `server_overloaded` 失败，不能作为正式完成门。
- C 恢复实施 turn=`01a0c95c-c32d-7bc3-8a79-e47a5ee4cce5`，13:44:54.866 开始；14:35:31.873 formal final，14:35:34.253 原生完成事件；`wait_threads` 核实 completed／error=null 后才出本报告。
- R 任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`。原审查 turn=`01a0c920-fb8e-7b32-a97f-dcf6e50c4d72`，12:39:37.153 开始、13:03:19.597 完成，仅记录执行 BLOCKED，没有技术 verdict。
- 本次恢复审查 turn=`01a0c95d-69c8-7452-a82a-42977d84f87b`，13:45:37.521 开始；沿用原独立基线，不重新审判已接收的 018B，不将 C 原 turn 的服务故障列为代码缺陷。

| 正式交付物 | R 实核长度／SHA256 |
| --- | --- |
| [demo-018c1-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-delivery.md) | 98 行，13,831 字节；`430cc910cdc7a501b1046c7ae240264ddca3b635aaafc42432ce5daadcbdf7e6` |
| [demo-018c1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-018c1-scope.json) | 621,398 字节；`13cc5aa76f949a95c326ca9d63797b505f9f667d73ee177869eecfbeb40019ae` |

本文 E 指 `D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/aee5ada2879c4a6aa69e3782119d26a5`。E/late/formal-bindings.json 为 4,310 字节，SHA256=`8d755a89c6f1a6b8c9aa346f6d79dd7e05ba42b69380782d3325f29c2321c617`，已与 C formal final 及实际文件逐项核对。

## 2. 唯一发现 R1

**[P2] 普通 EditMode 入口必须能创建自己的测试目录。**

- 位置：[SavePendingRecoveryProcessCases.cs:43](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryProcessCases.cs:43)，关联该文件 166～179 行及 [SavePendingRecoveryTests.cs:655](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SavePendingRecoveryTests.cs:655)。
- 新增 60 项普通 NUnit 用例经默认 `PendingRig` 调用 `TestRoot()`；后者无条件进入 `EvidenceRoot(Environment.GetCommandLineArgs())`，强制要求唯一 `--b14c1-root`，并要求该 GuidN 根已存在 `baseline.json`。
- 仓库 [.agent/VALIDATION.md:104](D:/Unity/UnityProj/FightMatch/.agent/VALIDATION.md:104) 的普通全量 EditMode 命令没有该参数；正常从 Editor Test Runner 运行也不需要携带本次作者证据根。缺参数时夹具在到达被测逻辑之前必然抛出 `InvalidOperationException: B14C1: --b14c1-root`；只提供全新目录仍会因缺冻结 baseline 失败。
- 作者的 3,019 项通过结果是真实的，但其专用运行带了 `--b14c1-root` 并准备过 baseline，不能证明普通入口兼容。专用 P01～P03 的严格根校验有必要，普通单元测试不应共享这个外部前置条件。
- 该判断来自完整源码调用链及实际运行参数；R 按权限没有再启动一次无参数 Unity 运行，不把静态结论写成已执行失败。

Required correction：只解除普通测试对专用根／历史 baseline 的强制依赖，给普通测试创建安全的全新用例目录；显式 P01～P03 入口继续执行原严格根、baseline、nonce 和进程身份校验。

Do not change：生产代码、另外四份新 C#、全部旧源码／测试、任何 meta、持久化格式／公开 API／配置、原交付报告及证据。具体窄纠正包见第 8 节。

Verification required：C 在修正后的同一源码版本运行最终编译、**不带 `--b14c1-root` 的全量 EditMode**、原六次 P 进程；完整留存真实退出、XML、源码／DLL／PDB 与证据绑定。

## 3. 规范与规格两轴结论

| 轴 | 独立检查结果 |
| --- | --- |
| 规范 | 五份实际新增 C# 全文审阅；只新增白名单十项资产，旧 540 项未变；无新增包、配置、存储抽象、第二租约或旧 private 提权。R1 使普通测试入口回归，需窄修。 |
| 规格 | 三个入口遵守原 Execute／writer lease、唯一 pending 槽和完整观察证据；原身份读取／恢复及明确物理结束的实现与 §202 一致。专用 PR01～PR08 证据成立；不能据此跳过 R1 或宣称应用接入完成。 |

- `SaveRecoveredCandidate`／`SaveCandidateEndResult` 均为 sealed、internal 构造；元数据、摘要、索引、操作列表冻结，候选保有原完整 Envelope 及 store／观察绑定。
- Read 先复核观察、要求完整证据和要求集合，只选 Pending Snapshot／SnapshotWork；双副本逐字节一致、完整 codec 读取、原当前头＋1／Parent／全历史精确前缀及操作不交叉均校验，返回前再复核。
- Resume 重读同一候选并比较完整原封套；核当前、可读 backup、pending 的所有能力，最后复核后才设置原 pending 槽。保留原 Commit／Generation／Parent／OperationIds／ExpectedHead／Descriptor，不调用业务 build，不写文件或分配新身份。
- End 可处理 RequirementsIncomplete，但仍要求完整字节证据及确证 Pending；索引内 Commit 或正式 marker 不删。只按 MarkerWork→SnapshotWork→未索引 Snapshot 删除所选组，每次删除前核剩余证据、头／索引、目标长度／SHA；仅允许本次已成功删除项消失，末尾再核头与组消失。
- End 的读取／删除 IO 保留原 Diagnostic／Stage 并返回 EndInterrupted；Limit／语义拒绝保留原类别。中断后新观察处理余项；已无该组为 CandidateNotFound。三个入口对 Busy／Disposed／外来或过期观察、非 CandidateValidation Purpose 保持合同门。

| 验收 | 已核源码与实际证据 |
| --- | --- |
| PR01 | work／snapshot／一致双副本，gen1 与后继；重开后接回原 ticket 并由旧 Write 提交；完整六片 Decode 及原 M12 Guid／操作结果查询；reader build=0。 |
| PR02 | 只读磁盘不变；外来 owner／ticket、文件／目录／头变化、历史摘要／长度／操作错误拒绝；冻结集合。 |
| PR03 | 当前／backup／pending 能力与完整 SourceNotes 顺序，slice／rule／numeric／random／feature；共享 Math 预算、Limit、Busy／Disposed、完整读取 Close 异常。 |
| PR04 | 完整副本、残片、冲突、部分 marker、marker-work、多个 Commit、未知文件、坏正式 marker；当前与已被索引的旧 Commit 均保护。 |
| PR05 | 所选组三种物理名称准确删除；其他组及当前／历史保留；正式 marker、证据变化／不可读、没有组时保护。 |
| PR06 | 三处删除前／后六种故障、删除中途目标／其他文件／marker 变化、Open／Read／Close IO；余项与新观察重试、最后一删已完成后的 CandidateNotFound。 |
| PR07 | Resume 后 SaveFailed／CommitUnknown 仍用原 ticket 解析；旧 pending／显式新建／重入门；先完整业务 Decode 再 Resume，坏业务不写。仅是调用次序见证。 |
| PR08 | 同版最终 compile、全量 XML、六次独立进程、范围及完整归档已逐项实核；普通无专用参数入口存在 R1。 |

## 4. 范围、基线与行数

- R 原入口独立快照从 12:41:49 开始，早于 C 写五份源码；恢复时及正式完成后再次核对。继承旧 540 清单原 capturedAtUtc=`2026-09-22T11:20:03.5448715Z`；C 自身真实入口另记 `12:45:55.2272083Z`，未倒填。
- 旧 540 项全部原字节，canonical SHA256=`fe791423a3b2422c08da18158b37556b747d7c1c19533afac8ba0e3440b592e0`。
- 最终实施 550 项／2,930,791 字节，canonical=`d44285fa306efc089446cc4dc81bc9b4458c304bd236a4e462ab8bbf4e1187bb`；Scripts＋Tests=536；完整 Assets=563，旧 553 不变，canonical=`d2c540a618ebcbea435faecc3e6b60926daa0d4e88531d9322a4cb0aa6507832`。
- meta=300，GUID 唯一值=300，无缺失、非法或重复；旧 295 项未改。首次导入补字段仅涉及五个新 meta：compile-02 前后各 59→243 字节，GUID 原值保持，MonoImporter 字段自然补齐，双态已在实际运行快照中保留。
- 五 C# 行数依次为 Models 44、Platform partial 233、Tests 786、ProcessCases 201、Editor bridge 10：生产 277／750，总计 1,274／1,950；evidence.ps1 为 126／280 行。两正式报告亦在行数／字节限额内。
- R 对旧 31 个 Artifacts 根独立再枚举：55,882 文件、2,114,761,704 字节，14:36:03～14:41:19 实核全部不变，无 reparse；canonical=`e8259236b369046746fbc09d1fcba5f1f146a9caf1263fc781de89396b647d48`。
- 旧日志／XML／工具 bin/obj 共 114 文件、38,993,231 字节全部不变；canonical=`f6eaba5ce761e23251833f19c126674029e8c7e0eaea5b3e396bb135e5506f52`。
- 原保护清单 460 项中 457 项不变。仅 session-plan.md、integration-review.md、system-task-packets.md 随协调更新，已逐条归因到 SD00 原生 FileChange 事件，非 C 越界；冻结 §201～204 未变。包括 Packages、ProjectSettings、规则、旧报告、权限文件在内的其他保护项未改。
- 四个自然新增回归根为 B12/9bd2f8ab26e34ccfa2e9ac7ca586fc6b、B12/dbe9c658b0ad47a5b5f43877c03443ca、B13/4a6ff3c5fbc94890897ebc2cd52cd93d、B13/ae6d5041e1c043f2bdb68580a6699f2e；合计 2,110 文件，纳入本次归档，未覆盖旧根。
- `git diff --check` 实际 exit=0。工作树本来已有其他历史未提交修改；Git 长路径枚举出现旧 Artifacts 路径警告，范围结论以以上独立实际文件清单／SHA 比较为准，未声称 Git 工作树干净。

canonical 的共同口径是 Ordinal 路径排序后，对 UTF-8 `path<TAB>sha256<LF>` 求 SHA256；源／payload／ZIP 使用相同相对路径域。

## 5. 实际执行、失败保留与 XML

R 全读 126 行收证脚本，核实际固定 exe／项目 cwd／完整 argv、同项目进程预检、Hidden／PassThru、PID／起止时间；脚本在 WaitForExit 后先保存实际 Process.ExitCode，再收产物。下表是进程真实退出码，不是 chunk_id 或外层 PowerShell 成功推断。

| 阶段 | PID | 起止 UTC | 实际退出 |
| --- | --- | --- | --- |
| compile-01 | 18912 | 13:47:51.7848144～13:48:10.0865945 | 1 |
| compile-02 | 10092 | 13:49:24.6683796～13:50:26.5820754 | 0 |
| tests-01 | 13864 | 13:51:25.6313363～13:54:23.0630245 | 2 |
| compile-03 | 19160 | 13:55:39.2456870～13:55:57.5169875 | 0 |
| tests-02 | 39044 | 13:56:53.5599143～13:59:25.8318802 | 0 |
| P01 writer／reader | 21756／33316 | 14:00:59.8267718～14:01:12.6494558／14:01:43.8556074～14:01:57.9858078 | 0／0 |
| P02 writer／reader | 28788／4052 | 14:02:32.0600570～14:02:45.5195893／14:03:21.0416739～14:03:34.2898510 | 0／0 |
| P03 writer／reader | 37156／21756 | 14:04:06.7499756～14:04:20.3686715／14:04:53.4086832～14:05:07.7786675 | 0／0 |

- compile-01 为新测试引用不可访问的内部成员失败；tests-01 为唯一 head fixture 用 Existing 模式尝试初始化，报 `InitializationRequired OpenMode`，3,018／3,019 通过。修正仅在新测试／场景文件；原失败日志、XML、源码、DLL 及真实非零退出全部保留。tests-01 的 Unity exit=2，外层 wrapper exit=1，二者没有混写。
- 最后源码变更时间 13:55:14.177，早于 compile-03。11 次运行每次前后实际源码／配置／资产／程序集拷贝均重新哈希，与对应四份清单一致，无额外／缺失／错哈希；最终 compile／tests／六 P 的实现 canonical 均为第 4 节同一版本。
- 最终编译后及后续所有前后快照的 28 份 DLL／PDB 与当前实际文件相同，canonical=`9d6f2482a99a306c4d7c08cd6dbfcffae1df2687528884b545a8628efca45bbb`；最终编译日志无 C# 编译错误。
- 原 XML 为 2,959 个 test-case／2,954 个不同 fullname；最终为 3,019／3,014。按 Ordinal fullname **多重集合**核对，原 2,959 个全部存在且 Passed；增加 60 个 SavePendingRecoveryTests 用例，全部 Passed；失败、跳过、原用例缺失均为 0。
- [最终 XML](D:/Unity/UnityProj/FightMatch/FMDemoB14C1-EditMode.xml) 为 2,060,648 字节，SHA256=`e7890775672bc620e6b7fdba3a33912bc2bfe3c5d9d39a843413a0e6149c37e4`；原基线 XML SHA256=`b7fe984a389ff9686e71a92bb19235238ba9155a969d1f72b6b677ea44dead2a`。
- 最终 compile 原生命令=`exec-179ab13b-75bb-4f3c-87d7-8105a2005343`；最终 tests=`exec-aa2443bf-501e-4f75-87eb-a5324397d064`；六 P 串行原生收证命令=`exec-f565350d-b332-4eb9-a49c-570fe0861803`。运行参数与每份 Unity 日志 COMMAND LINE ARGUMENTS 逐项相同。

## 6. 六次真实进程与原字节

R 对每案 writer／reader 的 identity、completed、外部 start／end／result、nonce、启动 ticks、exe／cwd、NUL 解码 argv 及目录记录逐项比对；每个 reader 都在 writer 实际退出后启动且 PID 不同。PID 21756 在 P01 writer 与 P03 reader 复用，但 start ticks 不同，不混淆为同一进程。

| 案例 | 原提交及独立核验结果 |
| --- | --- |
| P01 | Commit=`83e51a0a5e74452faf541b4e248244f4`；原 9,941 字节 SHA=`612019ec70415897967b1844c847d9604601bba4503b05df095f105caeeb56db`。writer 完整 work 与 reader 已提交 snapshot 原字节相同，reader 新增 276 字节正式 marker；完整六片 Decode／原操作查询在 Resume 前，build=0，最终 Ready。 |
| P02 | Commit=`a459441e0b9840169e88176a1f26da61`；原完整 SHA=`d2a40bf2a8513da3f946f943c2d5cd5a2bca9c375d5c0a41340bfd1760756387`。writer 实际残片 30 字节、SHA=`715d43b90764b3a0b8058835e8100f2a66b7eba52706da283e0afcbc78369645`，实核恰为原字节前缀；reader 明确 End 后仅保留锁文件，NoSave、新观察 CandidateNotFound、build=0。 |
| P03 | Commit=`cb39ecd6aff04008a29897343dbda984`；原完整 SHA=`e43eaa09b3e92bb4556c6f2aa8cd421ea83d930fcce8fc0d930ca4a2b6fc5534`。writer／reader snapshot 及正式 marker 完全相同；走原 Lookup／Load，新 Read／End 均 AlreadyCommitted，build=0、Ready。 |

这些是正常退出后的独立进程恢复验证，没有将同进程 Dispose／Reopen 冒充重启，也没有声称新增强杀或断电测试。

## 7. 完整归档与原生事件

- R 于 14:43:28～14:47:47 独立枚举真实源、payload、ZIP，而非只读取作者统计：三方各 **21,934 文件、3,288 目录、1,128,398,647 展开字节**；逐路径／长度／SHA 全相同，遗漏、额外、重复、大小写重复、不安全路径均 0，空目录也相同。
- 三方 canonical=`ea8c9ad4769ec42d2c6b0f0518632aa0f09054bebb1afc05a057e028e0f8e9a6`；ZIP 21,938 entries（含 4 个显式空目录项），251,903,182 字节，SHA256=`fbf54f8fbde6998ac78566ee9713dcdacf373d2f690f494e5275a6755b7a0b4a`。
- [完整 ZIP](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB14C1/aee5ada2879c4a6aa69e3782119d26a5/archive/FM-DEMO-018C-1-evidence.zip) 包含全部有效及失败运行、前后源码／DLL／PDB、XML、原始完整清单、P 记录及自然新增回归输出。
- 无损注册表 E/archive/complete-evidence.json.br：3,957,531 字节，SHA=`ec9b94fea8b83fd94a0bc3af3a02d61c23a07fc9d8e5173e4cd76864a1ab6841`；实际解压 85,929,066 字节，SHA=`f52f2b110d6393076447206ff8c7e1e147ea603b343c2a09648463c7f497acb5`。其中 12 个完整 JSON 分区分别与原始清单规范化逐项绑定，未以摘要或截断数组替代完整列表。
- 明确自引用排除为 E/archive/**、E/late/**、两份事后正式报告；archive 清单／ZIP 与 late 九文件均在外部绑定，最终报告 SHA 由 formal final 及 formal-bindings 独立闭合。
- 两个 C turn 的原生记录包含 77 个已完成 CommandExecution、9 个 FileChange、4 个生命周期事件；实际导出的 76 个独立命令、全部 FileChange 与相应生命周期逐事件 ordinal／时间／内容一致。余下 `exec-0bb0948c-7136-471d-94d9-d51e4944b1a8` 是导出后的收尾自绑定命令，R 直接读其原生全文；最终完成事件亦从原生记录补核，没有伪称递归自包含。
- 收证脚本 SHA=`14cbf1a5bde6bc2815ebd599d55c2281d2316309518a77976bfc58d2a1976d58`；R 核源码写入、报告／证据生成与实际运行命令，没有将作者自述或外层 exit 代替实现与进程证据。

## 8. 最小纠正 TASK_PACKET 草案（由 SD00 签发，当前未派发）

建议包号：`FM-DEMO-018C-1-C1`。唯一目标：关闭 R1，保留全部生产及跨进程行为。

允许修改的唯一现有实现文件：`Assets/Tests/EditMode/FightMatch/SavePendingRecoveryProcessCases.cs`，只调整普通 `TestRoot()` 的目录来源／必要局部帮助；不变更签名，不扩大场景。

拟允许新增的交付与验证输出：

- `docs/system-design/2026-09-17/demo-018c1-c1-delivery.md`（≤220 行）、`demo-018c1-c1-scope.json`（同目录，≤2 MiB）。
- `Logs/FMDemoB14C1C1Compile.log`、`Logs/FMDemoB14C1C1Tests.log`、`FMDemoB14C1C1-EditMode.xml`。
- `TestArtifacts/FMDemoB14C1/<另一个全新GuidN>/`：新基线、失败／有效输出、收证帮助（≤280 行）、六 P、完整源／payload／ZIP 及最终外部绑定；普通回归自然新增的 B12／B13 Guid 根逐项纳入，不覆盖旧根。

禁止修改：另外 549 项现有实施文件（含全部 meta）、全部生产代码与旧测试、API／格式／包／设置／权限／协调稿、此前作者及 R 报告与证据。禁止 Git 写入、子代理、外部模型、SDK 安装、新验证工程、018C-2 扩做。

接受标准与验证：

1. 普通 NUnit／EditMode 不传本包参数，也没有本包历史 baseline 时，自动创建全新且满足 containment／无 reparse 的用例目录；可以复用现有 `NewCase()` 的安全策略，不添加通用配置层。
2. 显式 P01～P03 仍使用原 `EvidenceRoot` 严格校验；缺参数、非法／越界／reparse 根及缺 baseline 仍拒绝，不以放宽专用进程门修普通测试。
3. 当前 550 项作为纠正前完整基线，保留原捕获时间并另记新真实入口；只有上述一个文件可变，仍为 550／563／300 唯一 GUID，行数预算不扩大。
4. C 作为唯一 Unity 执行者，以最终同版源码完成编译及**不带 `--b14c1-root` 的全量 EditMode**，使用新日志／XML路径，实际 exit=0；原 2,959 个及本次新增 60 个 fullname 多重集合全部 Passed、无缺失／失败／跳过。这次正常入口运行就是 R1 的回归验证，无需另造验证工程。
5. 同版重跑原 P01～P03 六个独立进程，均 exit=0；使用另一个全新证据根，保留原身份／字节、进程及源码／DLL／PDB绑定。失败先保存、再修正重跑；完整归档仍须独立源／payload／ZIP零差异与明确自引用排除。
6. C 交正式新报告与准确 turn 完成后停止，R 再独立复审。须先由 SD00 签发窄包及对应审查，不由本 R 直接派发 C 或开放后续包。

## 9. R 执行边界与必要动作

R 只新建本报告；完成了源码／原生记录阅读、实际文件枚举／哈希、XML 多重集合及 ZIP／Brotli 核验。Unity、项目测试、无参数复现运行、额外探针均 NOT RUN by R；未改作者代码／测试／报告／证据，未创建子代理或执行 Git 写操作。

018C-2、019、正式 PlayerSave／发布、场景与 Demo 体验、新强杀／断电矩阵仍 NOT RUN；本报告不能作为其完成或开放依据。

必要动作：SD00 在本恢复审查 turn completed 后接收本报告并签发上述窄纠正包；修正与复审完成前不开放 018C-2。R 提交本报告路径／SHA、准确 turn 及一次限定通知后停止修改。
