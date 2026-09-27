# DEMO-B13-R1 独立审查：FM-DEMO-017

唯一 verdict：**NEEDS_FIX**。

发现一项恢复分类错误（F1）和一项删除安全边界的必需验证缺口（F2）。已交付的编译、2747 项测试及 14 个独立进程场景证据真实且相互一致，但不能替代这两个未满足项。017 尚不可计接收，018 继续等待 SD00 的修正闭环。

## 审查身份与交付门槛

- C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；本次原实施 turn：`01a0c702-ce2b-7700-a2d5-21f4c69ac020`。
- R 任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本次原审查 turn：`01a0c703-4c5d-7ac2-b5ea-b975e6b20290`。
- 冻结契约为 r85 §§158–162，派发整稿 SHA256：`9aca2c69935e5e72e9899610a0bcfc85af36b25119c1ebd6b521665bcca968fd`。R 在内存逆向核对 SD00 的状态登记差异，重建整稿 SHA 精确相同；未修改任务稿。
- R 于 2026-09-22 02:51:53.4004671 UTC 固定独立 490 项清单、五旧文件原字节、270 份 meta/GUID，早于 C 的首次源码修改。
- C 原记录的 final_answer 为 2026-09-22 04:06:30.870 UTC；task_complete 为 04:06:31.167 UTC；wait_threads 同时确认该原 turn completed、error=null。
- read_thread 对本回合返回空 items，R 因此读取该原 turn 的真实事件记录，取得完整 final 与完成事件后才开始代码及新证据审查。没有以旧 C1 回合、文件出现或摘要空白替代门槛。
- [C 交付报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-017-delivery.md)：118 行，11676 字节，SHA256 `0bb9d080475c1a8d7f2e06fef53f14bf8318c3718a84ffa824d0d6208973436a`。
- [C 范围及证据](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-017-scope.json)：1021850 字节，SHA256 `65c72b7bd1c7b1a259168c5963de1e292de9aab002cbfcb981afc276e64b4902`。
- R 只做静态审查和只读证据核验；没有启动 Unity、运行项目测试或进程矩阵、修代码、Git 写入、新任务或子代理。唯一新增文件是本报告。

## F1 · P2：把已确认的完整临时副本重新判为未决

位置：[LocalSaveRecovery.cs:179](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs:179)、[LocalSaveRecovery.cs:204](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs:204)、[LocalSaveRecovery.cs:231](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs:231)。

原选头逻辑会把与当前自足索引中原 Descriptor 完全匹配的 work 文件归为 `IndexedOldCopy / IndexedCompleteWorkCopy`，保持 Ready。新逻辑却对所有 SnapshotWork、MarkerWork 无条件置 `unresolved=true`；SnapshotWork 还被加入 Pending 根，最后将 Ready 改成 Pending。

最小反例：已有四代完整提交 G1→G4，无内存票据；把原 `c-G1.commit` 的完整字节复制为合法的 `w-G1.commit.tmp`。该副本匹配 G4 自足索引里的 G1 Descriptor，原 Inspect 为 Ready，且没有未决候选。新 ReadRecovery 在 204–206 行仍把它算成未决，得到 `Pending / HasUnresolvedCandidate=true`；Cleanup 在 60 行拒绝，无法清理 G1/G2 的最终文件。此时 Load 仍按原 Inspect 返回 Loaded，Prepare 也通过原 Ready 门，四入口对同一物理状态产生矛盾。完整 snapshot-work 或两种完整 work 同在，也会触发。

正确行为是保留已确认的 work 文件及其字节证据，维持原已确认分类；它不是新的未决提交，也不是允许删除的目标。Cleanup 可以清理符合条件的更早最终文件，但必须保留 work、当前和准确前代。损坏、不完整或不能归属的 work 仍须 Pending 并关闭冲突入口。

依据是冻结 §158–160 的原选头复用、未决候选定义及 work 不删约束。已有 [SaveCommitMarkerTests.cs:202](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SaveCommitMarkerTests.cs:202) 的 `CompleteIndexedTemporaryCopiesAreRecognizedWhileIncompleteCopiesRemainPending` 已在本次 XML Passed，明确验证完整副本 Ready、随后破坏副本才 Pending；该旧用例没有调用新恢复入口，因而未捕获此回归。

这是 R 根据已通过旧控制和新代码路径确定的静态反例，R 未执行新增项目测试。最小修正限于新恢复逻辑对候选的筛选；为完整 marker-work、snapshot-work、二者同在及不完整对照补新入口回归。不要改旧选头器或放宽旧断言。

## F2 · P2：缺少删除目标在重核后被替换的真实反例

位置：[SaveRecoveryTests.cs:262](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SaveRecoveryTests.cs:262)、[SaveRecoveryTests.cs:282](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SaveRecoveryTests.cs:282)；待验证边界为 [LocalSaveRecovery.cs:74](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs:74)。

已完整读取 59 个新增测试、共享进程 Cases、控制器及实际 XML。现有 `SameHeadDoesNotAuthorizeAfterFileOrPendingEvidenceChanges("old-bytes")` 在调用 Cleanup 前改字节，验证的是整份旧 view 失效。两个 PartialDeleteFailure 用例只在删除前后抛 I/O；P09 在旧 marker 已删除后强杀。它们均没有在 `RecheckRecovery` 完成后、逐目标重读/删除前替换目标，因此没有实证验证 74–75 行的独立字节重核。

本项不是已观察到误删，也不否定已有中断证据；它是冻结 §160、R05“不能偷换目标”及本次审查要求中的必需见证缺失。当前所有绿色用例仍不能区分“每个目标重新匹配原字节”和“只在清理开始时核一次”。

最小补充：用现有真实 Windows 存储与测试包装器，在清理重核结束后、选中旧目标的逐目标读取前更换其实际字节，至少包含等长度不同 SHA 的替换；核实该替换文件未被删除、失败诊断准确、当前/前代与原键保持、已删部分被准确保留，重新取证后才继续。增加无替换成功对照；不能只修改 expected 元数据或重复调用前失效测试。现有逐目标重核若满足反例，无须为此改生产代码。

## 规范与规格两轴

| 审查轴 | 独立结果 |
| --- | --- |
| 规范 | 按 .agent/REVIEW_CHECKLIST 核对范围、原字节 diff、预算、程序集和依赖边界、失败保全及实际命令。范围与证据要求成立；兼容性及必需验证项分别受 F1、F2 影响。 |
| 规格 | 复用了原唯一选头、同一 lease/pending/Execute 门和 FMSAVE01 读取器，没有另设业务头、迁移格式或业务补偿。R03/R04/R05 的完整覆盖仍受两项发现阻断，不能以作者逐项 Passed 代替独立结论。 |

| 验收 | 独立核对 |
| --- | --- |
| R01 | 四代五片真实业务档，原正文逐片相等后 Decode；角色/库存/非空奖励及历史/回退、原随机得到断言。最新 marker/正文/SHA/版本/分叉及当前 I/O 不回退，旧坏副本不阻 Load。已有场景证据成立。 |
| R02 | 新入口复用 ReadCore，retain=false；仅在该入口无 expected，原 Read/ReadRequirements 的 CheckRoots 和全部 expected 校验保持。七类损坏、原 expected null/摘要及流生命周期用例全过；无业务解码或正文保留。证据成立。 |
| R03 | 当前、有效旧代、完整/部分孤儿、内存票据、缺坏备份、无档及未知文件已覆盖；嵌套根/摘要冻结。F1 漏掉已确认完整 work 与未决 work 的必要区别，不能标全项通过。 |
| R04 | Binding 四种、slice/owner/schema、各版本/Feature、fingerprint/SourceNotes 缺失均零回调；同头字节/集合/ticket 变化拒绝，异 store、Disposed、并发/重入、原 IOException 身份及释门有证据。F1 导致一类 Pending view 与 Load/Prepare 的门语义矛盾。 |
| R05 | 四代清理顺序、准确 Parent、缺坏 Parent 不替换、旧键查询、删除前后异常、半清理重开与新 view 幂等均有证据。F1 错阻合法清理；F2 缺少重核后的真实替换反例。 |
| R06 | 独立核实 .NET P01–P10 和真实 Unity P03/P07/P08/P09；13 个 writer 实际外部强杀，14 个新 reader 全部成功。P08 控制器在 kill 前实际接收回执；P08/P10 原五片、历史/回退和随机断言成立。 |
| R07 | 原物理 writer 租约、同门排他、当前 Open/Read/Close/枚举、真实 sharing denial、目录/字节及内存根预算证据成立。原 016/C1 Flush/故障用例保持。进程身份、路径、顺序及 PowerLossDurable 写前拒绝成立；F1 的分类矛盾另列。 |
| R08 | 原字节、范围/GUID、2747 XML、同版源码/DLL、真实命令及失败归档均独立核实；原 2688 名称与重数保持。绿色总数没有覆盖 F1/F2，故不能据此接收整包。 |

## 范围、差异与不可变输入

R 的起点与 C before/startedSnapshot 均为 490 项，规范 SHA256 `33328760c7503d1af4581159cfd7ac90312a69566c9b6ec8eab0e13eae53e5d5`；继承时间仍为 2026-09-21T19:36:06.7136939Z。R 对五份独立原字节执行内存行对照，结果与 C 计量相同：

| 原文件 | 增/删 | 核对结果 |
| --- | ---: | --- |
| Core/SaveEnvelopeCodec.cs | +29/-14 | 仅新增候选摘要入口及 shared reader 的 expected 分支，旧入口拒绝条件保持。 |
| Platform/LocalSaveStore.cs | +1/-1 | 仅 sealed partial 声明。 |
| Platform/ILocalSaveStorage.cs | +1/-0 | 仅 DeleteIndexedOld。 |
| Platform/WindowsEditorSaveStorage.cs | +7/-0 | 仅合法最终名删除，沿原路径及 reparse 防护。 |
| Tests/EditMode/FightMatch/LocalSaveStoreTests.cs | +1/-0 | 仅故障包装器直接转发；旧断言/用例不改。 |

五旧增删 54/550，Core 43/250，旧测试 1/20；新生产两文件 484/1400，新文件含六 meta 共 1578 行，总实施增删 1632/5200，均在预算内。其余 485 项 SHA 全部保持。恰新增 11 项 Assets 与 4 个工具源/配置；原 270 meta SHA/GUID 保持，当前 276 GUID 唯一。501 指 Assets/Scripts 与 Assets/Tests 实施项；完整 Assets 树为 518 项，含工具的实施清单为 505 项。

- 501 项规范 SHA：`da9c585852b04bcf6a00b1664e715b77f4988501925e268e4dc4e18eaab685b9`。
- 505 项规范 SHA：`84305bcbd1117d7b83a1f54b1107ca9f2240c873eee58ae508999d2a35c90ce9`。
- 最终 14 份手写源码/配置规范 SHA：`e392db795910d257041a198b4394ff0ffbb801e59234c68019788ef8bc6ea57d`。

R 启动另固定 494 个受保护输入，最终只有三份协调稿变化；原 SD00 turn `01a0c712-0c9a-7950-98a7-fb2ee0531a09` 的三个 FileChange（85a72792、b1cdfb93、e54786b8）明确对应 G1 审查派发登记，未改本包 §§158–162 契约。其余受保护文件无缺失/变化。GLM 独立目录、两作者报告及专用审查产物不作为本包修改、依赖或验收阻断。

## 实际验证与原执行回溯

R 读取 C 本次原 turn 的 68 条 CommandExecution 和 17 次 FileChange，检查启动脚本、写入路径与控制器。关键 10 次运行的 request/cwd/实际 exit 与归档逐一对应：八次完整工具输出逐字节相同，另两次 SDK version/restore 的原 stdout 与独立 JSON 记录除终止换行外相同。未采信单独汇总。

| 运行 | 原 CommandExecution | 实际结果 |
| --- | --- | --- |
| compile-1 | exec-c25f62fe-1939-43d9-b179-f184a2eb936f | Unity PID 8700，exit 1；CS1061 完整源/日志保全。 |
| compile-2 | exec-169560bc-6857-4c96-8379-1d6aa4c7b75f | Unity PID 38400，exit 1；CS0118 完整源/日志保全。 |
| compile-3 | exec-6fca0daa-2816-4f8c-9b6b-e704e248bed8 | Unity PID 27020，exit 0；03:41:39.7121868–03:42:11.1681150 UTC。 |
| EditMode | exec-4866e866-78a5-4b80-9083-9fc5dd78de9c | Unity PID 13748，exit 0；03:43:26.6410490–03:44:24.4805917 UTC。 |
| SDK/restore | exec-64d423ab-d6cc-4afb-9ed4-d9ea982c405e；exec-4631f48e-4d89-43c6-ad67-6ff208eb6533 | 分别 exit 0，6.0.412；清空 NuGet 源，限定 CLI_HOME/packages。 |
| build-1/build-2 | exec-ea834427-d63d-4b2f-906d-1001557bbc1b；exec-8e969c76-0458-40d8-9392-c9825fbf17b2 | 分别 exit 0，最终 0 警告/错误；net6.0、C# 8、仅 Program 和链接 Cases、三个 Unity DLL 引用。 |
| 矩阵预检/实际矩阵 | exec-e90af7fa-407b-4c4b-8142-dfef1562b4d3；exec-5fa376a3-1afb-44f4-b6a0-f4668680d9bc | 预检 cwd 错误 exit 1，未启动 controller；修正 shell 后同版 controller PID 39408、exit 0。 |

日期均为 2026-09-22 UTC。四个 Unity 验证启动均检查已有 Unity，Hidden/PassThru/Wait 后读取实际 Process.ExitCode；EditMode 无 -quit。最后源码修改在 03:41:29，之后 compile-3、EditMode、矩阵前后手写源码 SHA 完全相同。compile-1 自然生成六 meta；compile-3 仅这六个新 meta 继续自然变化，原 meta 不变；随后 501 项冻结在 EditMode 和矩阵前后保持。

独立 XML 核验：2747/2747 Passed，0 Failed/Skipped/Inconclusive；原 2688 条、2683 个不同 fullname 的 Ordinal 重数及 Passed 全保留；新增 59 条全部 Passed。XML SHA256：`64835b6a3eaaaf5ca4ae95ba84cddf8c45c9e54dcb879f9cbf7fa93899fcdee1`。

| 最终 DLL | 长度 | Unity 源、工具副本、controller 报告共同 SHA256 |
| --- | ---: | --- |
| FightMatch.Platform.dll | 73728 | `6e771ce5861c2b25a326b9bc646d3c91e1c2f10cbbbdac1c3e249184cbd7a331` |
| FightMatch.Core.dll | 520192 | `a5a102832fe92f2f6fd07be8790bd02ca53737b307dff24360bfc3e9dc5d7ff8` |
| FlowPuzzle.Core.dll | 18944 | `65149856030c2ed1ab426d85b9241556b25a30e35e75bd47d931aeaeac775db0` |

## 独立进程与产物核验

R 逐例读取 startup/identity/barrier/kill-request/exit/expected/reader-result；核 PID、StartTimeUTC、绝对 exe、nonce/root、case、writer 模式，killEntireProcessTree=False，writer 退出早于不同 PID 的 reader 启动；四组 Unity 的八个进程时间区间无交叠，日志均含固定 2022.3.18f1 和真实 executeMethod。控制器只对自身保存并重核的 writer 调用 Kill(false)，失败路径没有补杀任意进程。

| 场景 | writer → reader PID | writer/reader exit | 独立核对结果 |
| --- | --- | --- | --- |
| .NET P01 | 11296 → 38096 | -1 / 0 | 第1代 Pending；部分正文根要求不完整。 |
| .NET P02 | 38416 → 25596 | -1 / 0 | 第1代 Pending；正文 Flush 后完整候选不提升。 |
| .NET P03 | 25756 → 5424 | -1 / 0 | 第1代 Pending；正文 Promote 后完整候选不提升。 |
| .NET P04 | 3408 → 6696 | -1 / 0 | 第1代 Pending；完整正文根保留，部分 marker 使要求不完整。 |
| .NET P05 | 31292 → 11548 | -1 / 0 | 第1代 Pending；marker Flush 后未封记。 |
| .NET P06 | 14424 → 38376 | -1 / 0 | 第1代 Pending；最终 marker Promote 前。 |
| .NET P07 | 24200 → 36268 | -1 / 0 | 第2代 Ready，原键 Committed。 |
| .NET P08 | 12240 → 10928 | -1 / 0 | 第2代 Ready，控制器收到的 Committed 回执仍可查。 |
| .NET P09 | 38280 → 39100 | -1 / 0 | 第4代 Ready；保留准确第3代，新证据清除剩余3文件。 |
| .NET P10 | 36012 → 33936 | 0 / 0 | 正常退出对照；第2代及固定业务保持。 |
| Unity P03 | 38668 → 1688 | -1 / 0 | 第1代 Pending；实际 Unity writer 强杀后独立读。 |
| Unity P07 | 38124 → 39412 | -1 / 0 | 第2代 Ready，原键 Committed。 |
| Unity P08 | 4924 → 32740 | -1 / 0 | 第2代 Ready，已接收回执提交仍在。 |
| Unity P09 | 17576 → 23124 | -1 / 0 | 第4代 Ready，准确前代及半清理恢复保持。 |

P08 和 P10 通过原五片 SHA 比较及 015B Decode 检查固定业务：archive=1、rollback=1，原 fixed-attack/fixed-rollback，随机 State=1753877967969059832、Increment=109、WordsConsumed=0；reader 不重做攻击或回退。运行域仅此 Windows/NTFS、.NET 6.0.20 和 Unity Mono 6.13.0 的 EditorProcessCrash，不扩大到 Android/断电持久保证。

- 本次验证归档根：`TestArtifacts/FMDemoB13/6a03b3580de446d98f33e7f766e49bee`；矩阵根：`TestArtifacts/FMDemoB13/410b3aee64244048a849c8b1874cf8d4`；新 NUnit 根：`TestArtifacts/FMDemoB13/00b2be6763854f04800c1c0e94dc1e76`。
- 原旧测试按未修改的既有规则自然新增 `TestArtifacts/FMDemoB12/0afa64f431314bc49499c079d032bdfb`，没有为切目录改旧测试。
- 本次产物清单 1207 项，实际长度/SHA 均吻合；规范 SHA `ff3a5e04fc85d86383b2c83d7872cb2732619616aaf12915e85f57e1494ac6f6`。全量 zip 的 1207 个条目逐个与清单 SHA 匹配，zip SHA `388cb07b47872d17b5f4e05d5a6f424d4477da60fb318adc01493d6bf09fe23a`。
- 两个失败编译 zip 各含 14 份对应冻结源码/配置和完整日志，源码全匹配当次冻结，实际 CS1061/CS0118 与原退出 1 相符。
- 原 B12/C1 四轮 2516 个实验文件长度/SHA 完全保持，规范 SHA `35ca1874c47e9bcafab989a35ce939d4f541ed4e777c35a6e23bfae63b2212ee`。
- R 的只读审计脚本曾遇到空记录字段和 PowerShell 自动 Length 属性问题；更正读取逻辑后完成复核，未把这些辅助脚本错误记成 C 缺陷或项目测试失败。

## 最小修正包边界（由 SD00 另行编号、派发）

建议只允许修改 `Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs` 与 `Assets/Tests/EditMode/FightMatch/SaveRecoveryTests.cs`；新建修正交付/scope及自然验证产物的精确路径由 SD00 指定。其余源码、原测试、Core、选头器、工具、meta/GUID、依赖/配置、原交付和实验档、GLM独立工作保持只读。

验收先用新增真实文件测试见证 F1 的现版失败，再验证修正；区分完整已索引 work 与真实未知/部分候选，覆盖 ReadRecovery、Load/Prepare 一致性及 Cleanup 保留 work 的行为。补齐 F2 的重核后等长度替换拒绝和成功对照。不得通过修改旧预期或把所有 work 一律放行解决。

生产恢复逻辑改变后，C 须重新冻结同版、编译、全量 EditMode，保留本次 2747 fullname 重数及结果并加新增项；按包重新跑 .NET 十项和真实 Unity 四项，核同版 DLL、独立进程与完整失败保全。全部用新验证根，不把本次旧矩阵冒作修正版结果。清单仍应为 501/505、276 GUID，其他文件逐字节保持。

R 不直接修复或派发，不修改协调稿。本报告正式交回后结束本次原审查 turn 并停改；SD00 负责修正包与后续独立复审。用户当前无需操作或重复技术裁决。

