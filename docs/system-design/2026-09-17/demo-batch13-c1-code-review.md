# DEMO-B13-C1-R1 独立复审

VERDICT: NEEDS_FIX

017 的 F1 分类错误与 F2 必需行为见证均已闭环；本次未发现新的生产代码缺陷。整包仍有两项限定于验证交付的缺口：E1 归档枚举漏掉实际 work 文件及配置副本；E2 首次失败 Unity 的实际退出码未留存。C04／原 R08 尚不能全部通过，不放行 018。保留当前修正版，不要求重复改代码或重跑已验证的红绿／矩阵。

- 原 C1 实施 turn：`01a0c766-c723-7453-a0bb-3ac300182c8f`，任务 `01a0c403-bfa1-7e90-b503-c0fcd61f23c1`。
- 本次原 R turn：`01a0c767-3e3f-7b11-b752-dbc8489b56af`，任务 `01a0c1cd-dce1-7ac3-8780-06163cb0acfc`。
- 唯一写入为本报告；R 未运行 Unity／项目测试、修代码、作 Git 写入、创建任务或子代理。
- C 完整 final 时间为 2026-09-22T05:07:42.927Z；准确原 turn 的 task_complete 为 05:07:43.260Z，completed、error=null。R 在此门槛后才审新代码与本次证据。read_thread 的空 items 未被当作未工作或完成证明。
- 依据 r87 §165～168、原 §158～162、[原 B13 审查](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-batch13-code-review.md)、.agent/REVIEW_CHECKLIST.md 与 VALIDATION.md。code-review 的规范／规格两轴在本任务本地执行，服从本包禁用子代理及原字节基线。

## 冻结对象与独立起点

派发稿整稿 SHA256：`85be277fb4124673894cdf95aad6e485512c12be073ddc7a19387a3899c35a9f`，R 派发时亲自核对。随后 SD00 登记状态／原 turn 并追加独立 GLM 包，整稿变为 `ed176e1725717d34be037302a9cad951c7c69d29595192ad30eb5f1e1c0bc9ef`；本包范围及验收未变。
R 于 04:38:27.0495084Z 在 C 修改前另存两份旧源完整字节、505 项及全部 meta/GUID；505 规范 SHA 为 `84305bcbd1117d7b83a1f54b1107ca9f2240c873eee58ae508999d2a35c90ce9`，与原交付及 C 04:43:18.2537212Z startedSnapshot 相同。
原 scope.after 的 501 项 capturedAtUtc=03:55:41.3479783Z；implementation 没有 capturedAtUtc，原 finalVerifiedAtUtc=03:55:42.1086241Z。未把本次时间倒填为原捕获时间，原起点仍为功能未接收的作者冻结版。

| 本次正式对象 | 实测 | SHA256 |
| --- | --- | --- |
| demo-017-c1-delivery.md | 106 行／13217 字节 | `8cb64b9674dde7c3c18be5e1721985caa1f62bdad5a63cc21a955f3f384e15fc` |
| demo-017-c1-scope.json | 1162840 字节 | `906139e1f954149c8d1d14377eeae91883f8cde297ef6fbc8bbddcab4aba71e0` |
| LocalSaveRecovery.cs | +4/-6，319 行 | `970412b74f77d444c9ea72a912adc43126f7b406a922f762c78d0f2525ef0d53` |
| SaveRecoveryTests.cs | +128/-1，549 行 | `d88b9e63dee0b966f2ba22f34c502d881a5bdb26a2e2313eb58d1f8038c56fc2` |

两原 SHA 分别为 `9efd226124d34c7318e81e25ee1c25adc92980dc02c7fd9ecb4b38c176840a88`、`96fe004334851d5601fe1d470a98cf8f70d4a6524296dbe94e264e11097892f4`。C scope 内原字节与 R 独立起点相同。原 B13 报告 SHA `ede6d3a9fc7a523dc444e22ac7916ff577626429c6fdb87b2b571cbcba887653` 保持，未重写其历史结论。

## E1 · P2：正式产物清单与 zip 漏掉 63 份证据

位置：[交付第 104 行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-017-c1-delivery.md:104)、scope.naturalProducts、验证根的 natural-products-manifest.json／all-validation-products.zip。
原归档 CommandExecution `exec-e4910ccc-e85e-427b-8891-2c836dc9e4e1` 用 `rg --files --hidden` 枚举各新实验根。该命令仍遵守 [.gitignore:15](D:/Unity/UnityProj/FightMatch/.gitignore:15) 的 `*.csproj` 和第 19 行的 `*.tmp`；zip 与同一漏项清单自比，无法发现漏项。

R 对六个本次根作不受 Git ignore 影响的实际文件枚举。排除 zip 自身、其清单及归档校验记录这三项合理的自引用项后，确认另有 **63 份／58082 字节**未进入正式清单和 zip：**61 个 .tmp 与 2 个 .csproj**。不是文件已被正确清理后的自然缺席；它们现在仍存在。

| 新实验根（均在 TestArtifacts/ 下） | 遗漏 |
| --- | ---: |
| FMDemoB12/af15d35ac74a4915a07c7c9d5a101ada | 5 个 work |
| FMDemoB12/c65d1a2064a141b18fa251f8ca883f69 | 5 个 work |
| FMDemoB13/242aec761e9140158c5ac33916218c68 | 23 个 work，含 F1 红失败现场 |
| FMDemoB13/820182926ca24b37a1715a81ae707a09 | 23 个 work |
| FMDemoB13/f17e61328e634b16a1afd59c8de2250a | P01/P02/P04/P05/P06 各 1 个 work |
| FMDemoB13/1889c36196bd4de69122cf7cfb1f4ad6 | red-source-bytes 与 green-source-bytes 下各 1 份工具 csproj |

可复查的实际例子：

- 红失败 `IndexedCompleteWorkKeepsAllRecoveryGatesConsistentAndSurvivesCleanup("marker",0)` 对应红根下 `35ea8afaf1da47afb382cd9f750d93a1/p-b13b93eaab842c1bba874db51bc20e2477ffcd712ab517d0f86744385f8d3072/candidate/w-b674b6578177434cb04ec42686b829ca.commit.tmp`，274 字节，SHA `129a47d26b5545ea1aa50e699ba8e275f890d8d21db3147a6f70b32690535b9a`；原 XML 指向此现场，但完整临时副本未进 zip。
- 矩阵根下 `dotnet-P01/data/p-179fc2d8ac60e1fe0d268ccd0fd7cfca541dbe42fd00be0ecf6fb43f660b38bc/candidate/w-8ccf3540576549a49964ab74ed471b1f.snapshot.tmp`，5134 字节，SHA `21451f06981d64bcd8dac99a3c145c19171e1c9d37e79fd8e9d28a3bd09b3933`，与 reader-result 原始文件证据一致，却不在正式归档中。
- 两份冻结配置的准确路径为 `red-source-bytes/Tools/FMDemoSaveRecovery/FMDemoSaveRecovery.csproj` 与 `green-source-bytes/Tools/FMDemoSaveRecovery/FMDemoSaveRecovery.csproj`；各 907 字节，SHA `f9fad2de9e99b5d5b4d2c31542bb5015bfb2cb9c4a3fcd731e7e9b6ff66da31b`。
- R 于 05:25:47.6636716Z 冻结这 63 份实际文件集合，Ordinal 路径＋TAB＋小写 SHA＋LF 的规范 SHA 为 `49b2d1c724c0f8f366fed41fa551079ffe9ccdc4a5c72e6c7d3fce2d07124806`。

2472 个已列 zip entry 的完整长度／SHA 全部正确，不能据此宣称“所有产物”完整。红源码 zip 也只有 13 份手写源／配置加 7 项记录，共 20 entries；第 14 份 csproj 在现场有完整字节，未进该 zip。运行证据可审，正式保全仍不满足 §167 的完整失败／实际文件归档要求。
同一枚举还使 C 的 priorArtifacts 3741 条漏掉 33 个旧 work。R 自己先前用实际目录及更早已冻结清单核过八旧根全部 3708 文件，本次复核同 SHA（规范 SHA `56b3ae40c393884eab4ef90d9507f1068c58b1ce15d21e47cfa28720a0a1402f`），包含这 33 个，故**未发现旧档被改**；但 C 的“完整旧产物清单”应在新补证报告中纠正，不能倒填原清单时间。
最小修正：保持 C1 原报告／zip／清单不可变，在新证据根补充完整枚举、遗漏文件原字节、来源映射及独立 zip 校验；将“已列项匹配”和“目录全集齐全”分别验证。不改 .gitignore 或生产／测试代码。

## E2 · P2：首次失败 Unity 的实际退出码未持久化

位置：[交付第 54 行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-017-c1-delivery.md:54)、scope.failedAttempts[0]、验证根 red-compile-attempt1-failed/failure.json。
`exec-7adf500f-e9bb-4489-9388-a0ebeb82832c` 启动 Unity PID 30168，StartTimeUTC=`2026-09-22T04:46:30.7557900Z`。授权 IPC 超时后，脚本在构造 DLL 清单时错误读取不存在的 FightMatch.Tests.dll；原工具实际 exit 1。脚本把 `$p.ExitCode` 赋到内存 record，但在首次 SaveJson result 之前抛错，原返回也没有打印该值。
完整 700 字节 Unity 日志 SHA `ba8a55b9e0c6c8759035d7c2a30f90fc3d52497fa594a4d99da37b92080d3dee` 写明将以 199 结束；这不是实际 Process.ExitCode 的捕获。C 明确留 null／unityExitCodeCaptured=false，未冒造结果；R 也不能把 wrapper exit 1、日志 199 或后续 retry exit 0 替换该历史值。
有效红重试、红测试、绿编译、绿测试和矩阵均有真实退出码；从重试开始结果先落盘再采 DLL，记录顺序已经修正。本项不否定这些有效运行，也不表示功能需再改。但 §167 沿用 §161 的每次失败 PID／实际 ExitCode 可追溯要求，当前没有授权历史例外，故 C04 的无条件 Passed 声明不足。
最小处理：只查原回合或原留存中是否存在准确关联 PID＋启动时间的实际结果；有则原样保全，无则永久标 NOT VERIFIED，由 SD00 明确历史例外及后续验收边界后再复审。不能补造值、回填时间或重跑一次冒充这次历史。R 不自行豁免冻结要求。

## F1 与 F2 的代码、见证结论

**原 F1 已闭环。** [LocalSaveRecovery.cs:179](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs:179) 与第 204 行仅让 Inspect 判为 Pending 的候选进入未决循环；去掉不再可达的 IndexedOldCopy marker 分支。完整 work 仍经全文件 HashRecoveryFile、原分类及 RecoveryEvidence 纳入证据，没有永久豁免。原选头、Load／Prepare、Cleanup 删除及租约代码未改。
新增测试第 345～419 行覆盖 marker／snapshot／both × G1／G3／G4，四代真实保存；Prepare 票据先 EndUncommitted。完整 work 的类别、Detail、CommitId、长度及 SHA 精确保持，Current 不变、三个 Backup 根、零 Pending 根、一次能力回调。只删 G1/G2 最终四文件，当前／准确 G3／所有 work 逐字节保持，重开幂等及四原键仍可查；随后破坏 work，旧 view StaleContext，新状态 Pending 且冲突门关闭。另八个部分／损坏／错提交／未知提交的负向组合均保持阻断。

**原 F2 已闭环且无需生产修改。** [SaveRecoveryTests.cs:421](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SaveRecoveryTests.cs:421) 的真实 Windows 包装器在 Cleanup 内第二次 Enumerate.after 之后、选定文件实际 Real.OpenRead 之前 File.WriteAllBytes，长度相同、SHA 不同，没有改 expected 或用异常代替替换。第一、第二目标分别在已删除 0／1 项后注入。
红绿 XML 都给出相同结构：最后枚举在该例输出第 44 行；首目标 Open／替换／Close 为 45／46／47，后续目标为 49／50／51，前面恰有一次 Delete.after。无替换对照为 44／45／46，随后共四次删除。它确实经过整份 ObserveRecovery 和 evidence 比较后的逐目标重核，不是调用 Cleanup 前改文件。
两替换均为 CleanupInterrupted／Diagnostic.Code=StaleContext、FieldPath=Cleanup.Target、Stage=Cleanup.<目标>.Verify、ExceptionType=null、Value=null；目标保留，已删项准确为 0／1，旧 view 再用失败；重开与新 ReadRecovery 后余下 4／3 项可清理。当前／准确前代字节、Load、全部历史键保持。原生产版本已让三项 F2 全过，74～75 行长度／SHA 防替换代码保持原样。

## 红绿执行、源码与 DLL 对应

R 回溯本次原 C 的 42 条 CommandExecution、5 次 FileChange，检查写入及启动脚本。baseline、首次失败、有效红编译、红测试、绿编译、绿测试、SDK／restore／build、矩阵共 10 条归档 request 均准确对应原命令；分段工具输出拼接后与原 aggregated_output 全字相同，工具实际退出码一致。

| 阶段 | 原 CommandExecution | 实际 PID／UTC 起止／exit |
| --- | --- | --- |
| 有效红编译 | exec-8b536c6e-86a7-4436-a074-bd7b2b7033d7 | 448；04:49:17.1224538～04:49:51.7960111；0 |
| 红全量 EditMode | exec-2aec721d-d232-49c3-8a45-f8c40f164630 | 37368；04:50:38.4527179～04:51:41.0771780；2 |
| 绿编译 | exec-f55b218d-28e8-4f87-b2a6-56d0c8776f6b | 38476；04:53:11.9186042～04:53:27.5154043；0 |
| 绿全量 EditMode | exec-7c9ea687-dfd1-469d-b8c6-ed6b93626d5f | 14264；04:54:20.2962713～04:55:24.1806199；0 |
| 新矩阵 | exec-407d2c24-3ae8-4bda-b20d-8c6d97775738 | 32560；04:56:51.7380772～04:57:48.9587763；0 |

日期均为 2026-09-22 UTC。五次运行前后 14／501／505 清单规范 SHA 自洽、实际时间包围运行、阶段内部不变。原测试文件在 04:45:22.183Z 一次修改后红绿相同；生产唯一修改在 04:52:56.347Z，晚于有效红结果及 04:52:36.946Z 红归档完成。
有效红阶段生产保持原 9efd…；505 规范 SHA=`fe9d37e4589822d9d050da27d380dd8b93a3c49abf3da2bcd8f094484940221a`。九个新 F1 正向都在测试第 369 行准确失败：Expected Ready / But was Pending，无无关测试失败。
R 逐个 Ordinal fullname 及重数比较原 XML：原 2747 cases／2742 distinct fullname 全部保留并 Passed。红为 2767 总数、2758 Passed、9 Failed；绿为 **2767 Passed**，零 Failed／Skipped／Inconclusive。新增恰 20＝9 个 F1 正向＋8 个负向＋3 个 F2；相同新增测试转绿，无旧断言弱化。
红 XML SHA=`0fc5f9dc6e660e2ff50e3e633ee78e88faf209339db6cd8b63462d780c3f718d`；绿 XML SHA=`3d4f6baf02ac0ba2a8cc6bb5e0b6439733ee2167e9a6007279fa0ecd16bf73ec`；原 XML 保持 `64835b6a3eaaaf5ca4ae95ba84cddf8c45c9e54dcb879f9cbf7fa93899fcdee1`。
全部 15 份阶段冻结记录、红绿各 14 份实际源／配置字节已核；最终 14 份规范 SHA=`354b66484fdfe1e3f0142585c8827ea9f36e15d2d983ece68b2e1c59b0c58978`，其中 csproj 归档遗漏按 E1 处理，不混淆“现场有字节”和“zip 已包含”。
SDK6.0.412、离线 restore、build 三条原退出均 0，build 零警告／错误；固定配置及链接 Cases 未改。绿编译与绿测试 DLL 一致，以下 Unity 原 DLL、工具 CopyLocal、控制器实际记录、矩阵后与 final-unity-assemblies 副本均同 SHA：

| DLL | 字节 | SHA256 |
| --- | ---: | --- |
| FightMatch.Platform.dll | 73216 | `c822e3347a599b7939f9ad433f03ed88f1610e41602f09b3602e0617ffca3def` |
| FightMatch.Core.dll | 520192 | `a5a102832fe92f2f6fd07be8790bd02ca53737b307dff24360bfc3e9dc5d7ff8` |
| FlowPuzzle.Core.dll | 18944 | `65149856030c2ed1ab426d85b9241556b25a30e35e75bd47d931aeaeac775db0` |

红 XML 审计初次 CDATA 序列化导致辅助 guard 失败，原命令／旧 JSON／更正后原 XML 可回溯；不是额外项目失败。R 自己只读审计也修正过数组成员、脚本括号及时间表示处理，未作项目执行或把审计脚本错误归罪于 C。

## 14 个新进程场景

矩阵根 `TestArtifacts/FMDemoB13/f17e61328e634b16a1afd59c8de2250a` 与旧根不重合。R 逐例直接解码原 TSV；startup／identity／barrier／kill-request／exit／expected／reader-result 与 scope 相符。16 个 UTC 字符串在 scope JSON 往返时省去尾零，原 TSV 按精确 UTC ticks 相同，不是运行时差异。
控制器只对本次持有、再次核 PID／StartTime／exe／nonce／根／case／writer 的进程 Kill(false)。13 个 writer 实际 -1，P10 为 0；14 个不同 PID reader 都在对应 writer 退出后启动、exit0。八个 Unity 进程顺序无交叠，原日志含固定 2022.3.18f1 和真实 executeMethod。验证 Unity 运行前均查无现存 Unity，Hidden 启动；EditMode 无 -quit，executeMethod 矩阵带 -quit 符合契约。

| 场景 | writer → reader | 结果 |
| --- | --- | --- |
| .NET P01 | 3052 → 37292 | G1 Pending，部分正文要求不完整 |
| .NET P02 | 17384 → 39548 | G1 Pending，Flush 完成候选不提升 |
| .NET P03 | 36188 → 3940 | G1 Pending，正文已 Promote 无封记 |
| .NET P04 | 34652 → 20920 | G1 Pending，部分 marker 要求不完整 |
| .NET P05 | 2580 → 4824 | G1 Pending，marker Flush 后 |
| .NET P06 | 37588 → 18608 | G1 Pending，marker Promote 前 |
| .NET P07 | 39644 → 9772 | G2 Ready／Committed，Write 尚未返回 |
| .NET P08 | 24356 → 32160 | G2 Ready／Committed，kill 前控制器确收回执 |
| .NET P09 | 6896 → 24088 | G4 Ready，准确 G3 保留，重取证删余下三项 |
| .NET P10 | 11452 → 13856 | 正常退出，G2 Ready／Committed |
| Unity P03 | 39600 → 19832 | G1 Pending |
| Unity P07 | 4744 → 19656 | G2 Ready／Committed |
| Unity P08 | 1896 → 33656 | G2 Ready／Committed，回执保留 |
| Unity P09 | 5232 → 10720 | G4 Ready，准确 G3 保留及中断续办 |

每例当前头、五片原始 SHA、代次、原键结果及实际留存文件长度／SHA 已核；随机值 1753877967969059832 保持，已提交场景 archive=1／rollbacks=1，未提交为 0／0。原共享 Cases 的角色／库存／奖励／历史／回退／随机断言未变，新 reader 实际执行通过。P01～P06 不提升、P08 已答回执不丢、P09 新证据续办成立。五个 work 的正式 zip 缺失仍按 E1 单列。
实际域为 Windows NT10.0.26200.0／NTFS、.NET6.0.20／Unity Mono6.13.0 的 EditorProcessCrash；没有新增 Android／断电持久性声明，PowerLossDurable 仍写前拒绝。

## 范围、规范与整包验收

两 C# 合计增删 139/360，生产 10/80；旧测试正文未变，唯一旧行变化是包装器在真实枚举完后追加 Enumerate.after，原前置 Hook 和错误行为保持。仅这两项 SHA 改变，其余 503 实施项相同；Assets/Scripts＋Tests 仍 501，含工具 505，完整 Assets 518，全部 276 meta 字节／GUID 保持且 GUID 唯一。没有新源码／meta、依赖、程序集、格式或公共 API 改动。
最终 501 规范 SHA=`b57f9488fb2095956054e2d31c51a4d16070726aa59e451ca9f9df3ce687ea80`；505=`94d317b8cc269d79f6d218c7c5e84b0e166bf7d06df3c5fefc6f8290820922fe`。
R 单独冻结的 500 个只读文档／配置／旧日志项中，只有三协调稿变化；已回溯 SD00 的 `exec-ccf5509d-c2f5-4306-b038-d6207f0398f7` 与 `exec-888bdcc1-3342-4446-b235-ceb3150ec649`，属状态登记及独立 GLM 包，非 C 污染。原 017／C1 之外 GLM 实现及 R-G1 报告不作为本次依赖或验收对象。
八旧根全 3708 文件与 C1 开始前独立实际基线、016／016-C1／017 原归档清单完全一致；C 所列 3741 旧产物也全同 SHA。旧 1207 条正式归档本身保持，允许自然重建的工具工作 bin/obj 不被误判成旧档污染。C priorArtifacts 漏 33 项与本次 zip 漏 63 项已分别说明。
本次 zip 已列 2472 entries 逐条完整解压流核 SHA、零损坏／缺列内 entry；zip SHA `1704465981b1be7bfd49c9726b1d8cc910d6e063fd1bd271671c50a84cdf06aa`，清单规范 SHA `54ec3aff53703c270a25ea613b806adc0206d9f1bf88357ae74798c1a0ee1b5b`。目录全集完整性因 E1 未通过。

| 轴／验收 | 独立结论 |
| --- | --- |
| 规范 | 代码最小、范围／预算／原字节／隔离保持；正式证据完整性受 E1/E2 阻断。 |
| 规格 | F1/F2、原入口／唯一头／未决门／准确前代／故障域行为成立；没有新增玩法或架构。 |
| C01 | 通过：原生产上的九个有效红失败、同测试转绿、三 work × 三归属。 |
| C02 | 通过：真正坏／未知 work 仍未决；完整 work 不删、后续字节变化使旧证据失效。 |
| C03 | 通过：首目标及一次删除后的实际等长异 SHA 替换、失败阶段与正对照、重新取证续办。 |
| C04 | 行为及最终同版运行通过；完整原始归档／首次失败退出码因 E1/E2 未全满足。 |
| C05 | 实施范围与旧字节保持通过；交付清单完整性由 E1 限定，不能以 2472 自比当全集。 |
| R01 | 原全量回归及新矩阵的五片业务 Load、原历史／随机、最新损坏不回退重新成立。 |
| R02 | Core 严格共享读取未改；原无 expected 摘要／七类损坏／原 expected 入口控制全 Passed。 |
| R03 | 原根类别／完整性控制全 Passed，F1 完整已索引 work 的分类缺口已补齐。 |
| R04 | 原能力四类 Binding／slice／版本／feature、过时证据、异 store／同门／回调控制全 Passed；F1 门一致性已补齐。 |
| R05 | 原四代／准确 Parent／异常与续办全 Passed，F1 合法清理与 F2 逐目标替换见证已补齐。 |
| R06 | 新 14 实际场景、控制器／新 reader／回执及同 DLL 已核；其现存 work 的正式保全缺口按 E1。 |
| R07 | 原租约、当前 I/O、预算、原016 Flush控制全 Passed；新进程身份／顺序／故障域保持。 |
| R08 | 代码／GUID／全量回归／同版验证通过，正式归档和失败实际退出记录因 E1/E2 未全满足。 |

## 更小的纠正任务包（提案，未派发）

任务名建议 `FM-DEMO-017-C2-EVIDENCE`。目标只补交付证据，保留本次已核行为结论；须由 SD00 明确批准后再执行，不沿用原大包开放源码修改。
允许新建 `docs/system-design/2026-09-17/demo-017-c2-delivery.md`（≤160行）、`demo-017-c2-scope.json`（≤1536KiB），及 `TestArtifacts/FMDemoB13/<本次全新GuidN>/` 内所需原字节副本、完整来源清单、zip／独立验证／原命令证据。不新建手写代码／脚本源文件。
只读输入为 C1 两报告、六新根／八旧根、原回合执行记录、本报告、原016／016-C1／017冻结清单与当前505实施项。全部源码／测试／meta／工具配置、旧 C1 及更早报告／清单／zip／现场、Packages／ProjectSettings／权限／Git／协调稿与 GLM 范围禁止改；禁止 Unity／项目测试／强杀／Git 写入／新任务／子代理。不要重跑历史来改写归档。

验收与步骤：
1. 核本报告绑定的两 C1 报告 SHA、505 最终 SHA、全部 276 meta 和旧原档；用不忽略 .tmp/.csproj 的实际枚举冻结新当前全集，记录真实当前 UTC。对 63 漏项核数量、58082 字节及规范 SHA `49b2d1c724c0f8f366fed41fa551079ffe9ccdc4a5c72e6c7d3fce2d07124806`；不一致先 BLOCKED，不改现场。
2. 在新根逐字节归档这些现存漏项、红绿完整各14源／配置、必要原记录，写清原路径／长度／SHA映射；补充 C priorArtifacts 漏掉的33旧 work，依据更早原清单核 SHA。保留旧清单原貌，在新报告纠正其“完整全集”措辞。
3. 验证时独立枚举源全集与 zip 两边，除显式自引用项外零漏项／额外项／重复路径，逐条解压长度／SHA相同；不能只用同一筛选清单自比。提供原命令、真实退出、时间及源文件前后不变证据。
4. E2 只接受原已存在、准确绑定 PID30168＋StartTimeUTC 的实际退出证据；找不到就继续 NOT VERIFIED，停止宣称历史全部有 ExitCode。SD00须显式裁决该一次历史缺口是否作为已披露例外、更新后续验收边界；R不代为授权，不能要求作者猜造。新的正确捕获流程已在有效重试实证，无需再改生产代码。
5. C正式交新报告／SHA与准确本次turn，停改并completed后由R仅复核补证、冻结字节保持及SD00历史裁决。只有整包最终独立接收才可派018；本次不增加已接收功能包。

用户无需人工转述、补跑验证或重复技术裁决。由 SD00 接收本报告并定界后续补证；本 R 正式交回后结束停改。
