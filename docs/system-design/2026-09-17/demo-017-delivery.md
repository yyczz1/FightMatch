# FM-DEMO-017 / DEMO-B13 交付

C 已完成本包实施及实际验证，正式交回后停改，等待 R 的独立审查；本报告不替代 R 的 ACCEPT。

- 实施任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`。
- 本次原实施 turn：`01a0c702-ce2b-7700-a2d5-21f4c69ac020`。
- 合同：`system-task-packets.md` r85 §§158–162；派发整稿 SHA256：`9aca2c69935e5e72e9899610a0bcfc85af36b25119c1ebd6b521665bcca968fd`。
- 起点：已 ACCEPT 的 `demo-016-c1-scope.json`，SHA256 `3de415d1372ca61357a47f835e84c797d859f40077ed09691abfeab732e9f532`。
- 完整 before 沿用原 490 清单、时间 `2026-09-21T19:36:06.7136939Z`，另存本次 startedSnapshot；五份旧文件原字节全部进入 scope。

## 实现与范围

`Load` 在原排他门内重新检查唯一当前头，Ready 且无未决票据才读取完整封套。Pending 返回 CommitUnknown；最新损坏、未知格式、分叉或当前 I/O 失败均不改选旧档。

`ReadRecovery` 保留 Current、可验证的 Backup、完整文件候选及内存票据的 Pending 根。逐文件计算受限长度/SHA；未知条目和不完整候选明确标不完整。旧副本缺坏只影响冗余诊断，不变成业务头。内存票据也用本次 Codec/Math 预算重新校验。

`WithVerifiedRecovery` 在同门内重新读取并比较所属 store、文件集合、字节证据及票据身份，再核对所有根的 slice 三元组、四类 Binding 全字段、SourceNotes 顺序及版本/Feature。同步 action 只调用一次；其异常原样传播，finally 释放门。

`Cleanup` 重新核证后，只按当前自足索引删除更早代的最终 marker/snapshot，先 marker 后正文，逐文件再次比长度/SHA。保留当前与准确 Parent 的全部现存文件。删除中断保留原异常及阶段；旧 view 过时，重新取证可完成余项。孤儿、work、锁及未知文件不作为清理目标。

Core 仅新增 `ReadUncommittedRequirements`，复用同一 FMSAVE01 读取器，计算实际完整 SHA 而不保留业务正文。它没有最终封记背书；原两个 expected 入口的非 null、身份及摘要校验保持。

| 范围 | 实际结果 |
| --- | --- |
| 原五文件 | Core codec +29/-14；LocalSaveStore +1/-1；ILocalSaveStorage +1；WindowsEditorSaveStorage +7；旧测试包装器 +1。合计增删 54 行，原测试断言/用例名保持。 |
| 两份新生产代码 | SaveRecoveryModels 163 行；LocalSaveRecovery 321 行；共 484/1400 行。 |
| 新 Assets | 五份 C#、五份自然生成 script meta、一份自然生成 Editor.meta，恰 11 项。 |
| 离线工具 | Program.cs、csproj、NuGet.Config、global.json，恰四项，无 PackageReference。 |
| 总实施增删 | 1632/5200 行，包含六份新 meta；Core 43/250，旧测试 1/20，五旧合计 54/550。 |
| 清单/GUID | Assets 501；含工具 505；GUID 276 且无重复；原 485 其他项及 270 meta SHA 全部保持。 |

最终 14 份手写源码/配置冻结 SHA：`e392db795910d257041a198b4394ff0ffbb801e59234c68019788ef8bc6ea57d`。
501 清单规范 SHA：`da9c585852b04bcf6a00b1664e715b77f4988501925e268e4dc4e18eaab685b9`。
505 清单规范 SHA：`84305bcbd1117d7b83a1f54b1107ca9f2240c873eee58ae508999d2a35c90ce9`。
规范均为 Ordinal 路径 + TAB + 小写 SHA + LF，UTF-8 无 BOM。

## 实际验证

| 运行 | 实际 PID / exit | 结果 |
| --- | --- | --- |
| 最终 Unity compile-3 | 27020 / 0 | 2022.3.18f1；UTC 03:41:39.7121868 至 03:42:11.1681150，源码前后相同。 |
| 全量 EditMode | 13748 / 0 | UTC 03:43:26.6410490 至 03:44:24.4805917；2747 Passed，0 Failed/Skipped/Inconclusive。 |
| 原回归核对 | 完整 XML | 2688 个原 fullname 的 Ordinal 重数及 Passed 全保留；新增 59 项全过。 |
| 固定 SDK/offline restore | 各 exit 0 | SDK 6.0.412；global.json 禁止 roll forward；清空源，无依赖下载。 |
| 最终离线 build-2 | exit 0 | net6.0、C# 8，0 警告/错误；只链接共享 Cases 和三个本次 Unity 生产 DLL。 |
| 进程控制器 | 39408 / 0 | UTC 03:48:47.0192130 至 03:49:52.4902955；14 场景全过。 |

日期均为 2026-09-22 UTC。准确命令、cwd、原工具回执、冻结、真实进程时间及产物摘要在 scope 和验证根内；EditMode 命令不带 -quit，executeMethod 场景带 -quit。

XML SHA256：`64835b6a3eaaaf5ca4ae95ba84cddf8c45c9e54dcb879f9cbf7fa93899fcdee1`。
最终编译日志 SHA256：`ef8fef7420f2ea86fd6a485c35d8d88abb27baec05feb9a81dd2df3bd81f3184`。
测试日志 SHA256：`becc4b5af8ec0d8f8cc5aa0b688418d48ae547ce62b8f4e02a25d3f666f98930`。

| 最终生产 DLL | 字节数 | Unity 源与工具副本共同 SHA256 |
| --- | ---: | --- |
| FightMatch.Platform.dll | 73728 | `6e771ce5861c2b25a326b9bc646d3c91e1c2f10cbbbdac1c3e249184cbd7a331` |
| FightMatch.Core.dll | 520192 | `a5a102832fe92f2f6fd07be8790bd02ca53737b307dff24360bfc3e9dc5d7ff8` |
| FlowPuzzle.Core.dll | 18944 | `65149856030c2ed1ab426d85b9241556b25a30e35e75bd47d931aeaeac775db0` |

## R01–R08 自验与复核入口

| 项 | 实际证据与结论 |
| --- | --- |
| R01 | `FourBusinessGenerationsLoadRewardHistoryRollbackAndAllFiveOriginalBodiesAfterReopen`：四代五片、非空奖励、历史/回退、角色/库存、完整原正文及原随机回读保持。五类最新损坏不回退；已索引旧坏副本仍可 Load。通过。 |
| R02 | `UncommittedReaderRejectsIndependentWireDamage` 七种字节/预算损坏；原 expected null/摘要拒绝、流生命周期、I/O 传播；平台另核错 Player/Commit。共享 parser 的 retain=false 不留正文。通过。 |
| R03 | 完整/部分孤儿、内存票据、未知文件、无档、旧副本缺坏、完整但不匹配的 marker-work 均有真实文件测试。全部根沿原 Requirements，未启发式重造业务声明。通过。 |
| R04 | 四类 Binding 分别缺失、slice/owner/schema、Rule/Numeric/Random/Feature、fingerprint/SourceNotes 顺序共 13 情形；当前头不变时四种证据变化；异 store、Disposed、同步重入/并发和 callback 异常；冻结嵌套集合。通过。 |
| R05 | 四代只删更早两代，准确保留 Parent；缺坏 Parent 不选替代。真实删除前/后异常、半清理重开、旧 view 过时、新 view 完成、旧操作键继续可查。通过。 |
| R06 | 下表 10 个 .NET 场景及四个真实 Unity 场景；13 次外部 Kill(false)，14 个新 reader，正常 writer 对照一个。P08/P10 核五片 SHA、固定操作 ID、非空历史/回退与随机。通过。 |
| R07 | 同一真实 writer 租约/门、真实 sharing denial、Open/Read/Close/枚举异常、字节/目录和内存根四种低预算；原 Flush 故障等 2688 回归保持。PowerLossDurable 写前拒绝。所有 kill 身份、顺序与路径受限证据通过。 |
| R08 | 本节范围、完整旧名回归、全部 GUID、最终源码/DLL 与矩阵冻结、失败源码及原产物归档均已核对。独立 R 可对照本次原实施回合。通过。 |

## 跨进程结果

所有 reader exit 0；下表 writer exit -1 均为预期外部强杀。P10 writer exit 0。

| 运行 | writer PID | reader PID | 恢复结果 |
| --- | ---: | ---: | --- |
| .NET P01 部分业务正文 | 11296 | 38096 | Pending，原第 1 代；要求不完整。 |
| .NET P02 正文 Flush 后 | 38416 | 25596 | Pending，原第 1 代；完整候选不提升。 |
| .NET P03 正文 Promote 后 | 25756 | 5424 | Pending，原第 1 代；完整候选不提升。 |
| .NET P04 部分 marker | 3408 | 6696 | Pending，原第 1 代；要求不完整。 |
| .NET P05 marker Flush 后 | 31292 | 11548 | Pending，原第 1 代。 |
| .NET P06 marker Promote 前 | 14424 | 38376 | Pending，原第 1 代。 |
| .NET P07 Promote 已返、Write 未答 | 24200 | 36268 | Ready，第 2 代，原键 Committed。 |
| .NET P08 收到 Committed 回执 | 12240 | 10928 | Ready，第 2 代，回执对应提交仍在。 |
| .NET P09 旧 marker 已删 | 38280 | 39100 | Ready，第 4 代；准确保留第 3 代，重新取证删除余下 3 文件。 |
| .NET P10 正常退出 | 36012 | 33936 | Ready，第 2 代，固定业务完整。 |
| Unity P03 | 38668 | 1688 | Pending，原第 1 代。 |
| Unity P07 | 38124 | 39412 | Ready，第 2 代，原键 Committed。 |
| Unity P08 | 4924 | 32740 | Ready，第 2 代，回执对应提交仍在。 |
| Unity P09 | 17576 | 23124 | Ready，第 4 代，重新取证完成余项。 |

每例保存 startup/identity/barrier/kill/exit/reader-result；控制器再次核 PID、StartTimeUTC、exe、nonce/root 后才 Kill(false)，确认 writer 退出再起 reader。P08 另存控制器收到回执的记录。
P07/P08/P09/P10 的新头均有 archive=1、rollback=1，固定随机 state=`1753877967969059832`、increment=109、WordsConsumed=0，原操作为 fixed-attack/fixed-rollback；不是重算一次操作来恢复。
Unity 与 .NET 都使用真实 WindowsEditorSaveStorage/流包装；没有生产测试钩子、生产代码副本或内存文件系统替代。

## 证据位置与失败保全

- 验证/失败归档根：`TestArtifacts/FMDemoB13/6a03b3580de446d98f33e7f766e49bee`。
- 14 场景根：`TestArtifacts/FMDemoB13/410b3aee64244048a849c8b1874cf8d4`。
- 新 NUnit 用例根：`TestArtifacts/FMDemoB13/00b2be6763854f04800c1c0e94dc1e76`。
- 原 016/C1 测试保留已授权的固定测试根规则，本次自然新建 `TestArtifacts/FMDemoB12/0afa64f431314bc49499c079d032bdfb`；没有覆盖或清理旧根，未为换目录改旧断言。
- `all-validation-products.zip` 与逐文件 SHA 清单保全上述实际产物、完整日志/XML及最终工具运行文件。scope 内压缩块是 brotli+base64，附解压字节数/SHA；旧 before 保持完整原清单。
- 首次编译 PID 8700/exit 1：调用 Core 内部 CheckInteger，改用已有公开 ExactRational.Create。
- 第二次编译 PID 38400/exit 1：四处测试助手 Core 名称冲突，改成完整类名调用。两次失败均在覆盖固定日志前完整归档源码和日志；编译服务器自然退出，无强制结束。
- 首次矩阵预检因 shell cwd 使用工具目录而无法解析项目相对路径，启动控制器前即退出 1；只修正 shell 路径基准，同版矩阵随后全过。
- 首次事后时间审计把 PowerShell 本地 DateTime 与 UTC ticks 比较，产生误报；改用 DateTimeOffset.UtcDateTime 后逐例确认退出/启动顺序。两份审计命令及输出均保留，无源码变更。
- 活动回合的 read_thread 返回零 items；原工具执行请求/回执已转存 raw-validation-executions.json，首编译回执来自可见工具原文，其余直接序列化捕获。R 应在本原 turn completed 后独立复对原执行记录。
- 前两次失败保存 shell 观察 UTC、实际 PID/ExitCode；最终编译/测试及全部控制器/子进程另保存实际 Start/Exit 时刻。

## 限制与交回

本次实验证明的是这台 Windows NT 10.0.26200.0、NTFS 上的 EditorProcessCrash；.NET 6.0.20 与 Unity Mono 6.13.0 均实际执行。没有据此声称 Android 或掉电持久保证；PowerLossDurable 继续写前拒绝。
能力表是调用者的显式声明，本包 action 只做测试记录，不证明真实包已安装或启用；M10 ResolveExact、M14 能力接线和 M02 业务联合装载仍由后包负责。跨进程孤儿保留，不能凭完整候选自动终止/补偿/重做。
GLM 的独立工作仍属 `ExternalWork/FM-DEMO-026-G1` 及其外部报告，未依赖、修改或纳入本包源码基线。

正式交付为本文件及 `demo-017-scope.json`，最终 SHA 在本任务 final 给出。C 在 final 后结束本次原实施 turn 并停改，R 以 completed 门槛进入独立审查。
