# FM-DEMO-016-C1 / DEMO-B12-C1 实施交付

状态：F1 已由实际红绿验证关闭，等待 R 对 FM-DEMO-016 整包独立复审。本报告不代替 R 的 ACCEPT。

- C 任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；local，gpt-6-astra / max。
- 本次原 C1 实施 turn：`01a0c56a-105b-7d11-a2f2-038303b4b31a`；本任务 final 后结束停改，R 须确认此 turn completed。
- 契约：r82 `system-task-packets.md` §§149–152；原 r81 §§144–147 继续有效。派发整稿 SHA=`8d47524fc3f0bcc6eb97e7b0f081e5cf61de2bdc9155975f628654c7d5bbfee1`。
- 原 R 报告：`demo-batch12-code-review.md`，唯一 NEEDS_FIX，SHA=`b89ca3201bac068ffaf9748bc308f54a77f7fc37d4ac26c6090f11f0fbec0059`。本次只关闭其 F1。
- 完整范围、原字节、冻结、红绿结果及原执行证据：同目录 `demo-016-c1-scope.json`。

## 实际红灯

先仅添加公开 Store 入口回归及真实文件包装的一行窄辅助。两份生产源码均保持原 SHA，之后才运行实际红阶段。

`CurrentHeadReadIoFailureKeepsTheDiagnosticSessionAndOriginalCause` 组合 Open 初检／独立 Inspect／Prepare、当前最终 marker／snapshot、打开／读取／关闭、调用前／后故障，共 36 例。注入真实流边界上的 IOException，未删除或改坏当前文件。

每例先确认异常确实触发、回调零次、无 pending ticket、无新文件、原 snapshot/marker 字节相同、原租约仍排他；随后故障解除，同会话按原键查回原头，并成功 Prepare/End 新票据。最后才核对争议结果，因此红灯不是无效 fixture、提前异常或无关字段失败。

- Open／Inspect 的红灯实际输出：`Accepted=True; Code=Inspected; Value=FightMatch.Platform.SaveHeadInspection; Type=; Message=; Stage=`。
- Prepare 的红灯实际输出：`Accepted=False; Code=RecoveryBlocked; Value=; Type=; Message=; Stage=Head`；应为保留原原因的 StorageFailure。
- `IndexedOldCopyReadIoFailureDoesNotBlockTheCompleteCurrentHead` 的 12 例控制组原本全部通过，未伪造全红。snapshot 控制使用已索引旧 marker 的矛盾高代残片，使旧正文确实被尝试读取，然后由有效最新索引归属旧副本。
- 红阶段 XML 实数 **2688**：原 **2640** 条逐名、重数及 Passed 保持，新增 **36 Failed + 12 Passed**，零 skipped/inconclusive。36 个失败均到达最终 `HeadReadFailure` 断言。实际 Unity exit **2**。

## 最小修正

仅修改 `Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs` 和 `Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs`。

头选择仍先完成候选核验及索引归属，再传播不能归属为旧冗余副本的文件检查 I/O 失败，防止笼统提前抛错破坏旧副本规则。文件读取保留原异常并标明 `Inspect.<实际文件名>.Open/Read/Close`；打开、读取或关闭失败均可追溯，流仍被释放。

已确认坏格式、缺正文继续形成 Inspected/RecoveryBlocked 观察；FileNotFoundException/DirectoryNotFoundException 不被混同为暂时无法完成检查的 I/O。Limit、Pending、最新坏不回退、发布后 CommitUnknown、原 ticket 重试/结束保持。

`LocalSaveStore.cs` 未改：其现有 Execute 已能传播原 LocalSaveFailure，Open 仍返回持租约的 Opened 诊断会话。修正后 InitialInspection/Inspect 失败、Value=null；Prepare 原样保留 StorageFailure、原 IOException 类型/消息及精确文件阶段，回调为零。

| 三目标 | 新增 | 删除 | 最终 SHA-256 |
| --- | ---: | ---: | --- |
| `Platform/SaveHeadInspection.cs` | 24 | 5 | `cc1432c5ebc2ae45311b8c24c3254e828f634ed3cb05f89f45f4bd600f456c7a` |
| `Platform/LocalSaveStore.cs` | 0 | 0 | `232fd021eca903d15ddaa43af8fb3c8a4c3db399f6b696831291493eb6592899` |
| `Tests/EditMode/FightMatch/LocalSaveStoreTests.cs` | 76 | 0 | `81375ccf320573cb1b4ffe2a9290d9f401c28f486f39521845bbb152e6ef06d2` |

总计 **105/200 行**；生产代码净增 19 行，无新实施文件、公共 API、格式、程序集、meta 或 GUID。其余 487 项保持；白名单中的 Store 也保持，共 488 项原字节不变。

## 本次五项验收

| §150 项 | 作者验证与证据 |
| --- | --- |
| ① 真实当前头故障 | 36 个参数例通过公开 Open/Inspect/Prepare，覆盖当前最终 marker/snapshot 的真实 Open/Read/Close 前后 IOException；原文件逐字节保持。 |
| ② 诊断与恢复 | 最终 36 例全部 Passed：Opened 会话和排他租约保持；检查失败无 Value，Prepare 保留同一异常类型/消息及精确阶段；回调 0、无新票据/文件；故障解除后原键查询及合法 Prepare/End 恢复。 |
| ③ 先红后绿 | 原生产 SHA 下实际 36 个 F1 红灯、12 控制通过，原 2640 全过；记录完整红 XML/log 后才修改生产。最终同版编译及全量测试均 Unity exit 0。 |
| ④ 原故障语义 | 12 个已索引旧副本 I/O 控制全部通过；原坏格式/缺正文、Pending/Limit、坏最新不回退、发布后 Unknown、同票据重试及结束用例全部保持 Passed。 |
| ⑤ 整包与范围 | 最终 2688/2688 Passed，原 2640 的 Ordinal fullname 重数和逐名 Passed 保持，新增 48 全过；490 文件、原 270 meta/GUID 全保持，旧 1090 实验文件及旧证据不变。 |

原 r81 七项整体回归均包含在原 2640 条的完整比较中：①独立固定字节/隔离；②一次接纳/冻结候选；③三代重开/原键；④55 个提交故障/同票据恢复；⑤排他/重入；⑥唯一头/未决处置并补 F1；⑦范围及真实 Unity 验证。未削弱原测试断言或更改其 fullname。

## 起点与冻结

before 完整继承原 `demo-016-scope.json.after` 的 490 项及 `2026-09-21T18:38:31.1422784Z`，规范 SHA=`4a88037f6d67b95e3ab46faf3d176d7bb7cce0ab8dd7166efecd1b37bc593c1d`。源 scope SHA=`f96435dc84d05cf185ac7d4c1837ff9867470f180c5c4c3ec2b752defc0558bf`。这是范围已核、业务 NEEDS_FIX 的起点，没有标为 ACCEPT。

本批 startedSnapshot=`2026-09-21T19:22:58.5799935Z`，与继承 490 项逐项相同。三目标完整原字节经可验证压缩保留；原 B12 XML 2640 条及 SHA=`888092f8871282eb309c4209325cde94383f2b2917e9f40f8c8779af4e35859d` 固定为回归基线。

| 冻结 | 十份手工文件规范 SHA | 用途 |
| --- | --- | --- |
| B12C1-RED-F1 | `2675b9ee2d767e994a33c6616c1e12646d1d11d5ed5423128abd6ba581038786` | 首次测试兼容性编译失败，不算 F1 红灯 |
| B12C1-RED-F2 | `12a4dd1d63aa5232f9dc84bb2af52987b885afddd8b8e1883577b8b71ddf3dbd` | 编译通过后的真实红测，两生产原 SHA |
| B12C1-GREEN-F1 | `eb76899a9742f04d2569ba183d9c1ea8e238677d0929442443b64324cfc54959` | 最终同版编译/全量绿 |

最终 490 文件规范 SHA=`33328760c7503d1af4581159cfd7ac90312a69566c9b6ec8eab0e13eae53e5d5`。每次运行前后十文件和全 490 SHA 均一致，最后测试后源码未变。

## 实际执行与失败保留

固定 Unity：`D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe`，版本 `2022.3.18f1_d29bea25151d`。每次先查无其他 Unity，使用 Hidden/PassThru/Wait，WaitForExit 后记录真实 PID/Process.ExitCode。编译带 `-quit`，EditMode 不带；项目/logFile/testResults 均为正确引用的绝对路径。完整原命令和输出在 scope.rawCommandExecutions。

| 运行 | PID | UTC 开始 → WaitForExit 后（2026-09-21） | Unity exit |
| --- | ---: | --- | ---: |
| RED-COMPILE-1 | 17648 | 19:26:01.1463000 → 19:29:26.2454144 | 1 |
| RED-COMPILE-2 | 34148 | 19:30:17.5182054 → 19:30:26.0586807 | 0 |
| RED-TEST-1 | 41864 | 19:30:46.2757013 → 19:31:36.0304073 | 2 |
| GREEN-COMPILE-1 | 22028 | 19:33:30.4468708 → 19:33:39.0407483 | 0 |
| GREEN-TEST-1 | 9748 | 19:33:58.6161296 → 19:34:48.4142922 | 0 |

首次红编译为 `CS0117: Assert does not contain a definition for Multiple`，仅改为兼容断言后重冻；不把编译错当缺陷复现。覆盖前完整归档原日志 41178 字节，SHA=`e30a464a96f8ac5c3127bd8b5910e243b6e916fcb6e6d3547a2837a2e95defe0`。

该失败 Unity 已退出后，遗留本次创建的 Roslyn 服务器 PID 7152，使 Start-Process -Wait 等待子进程。先核本次启动时间、Unity 运行时路径、VBCSCompiler 命令行和父 PID 56140；第一次 UTC 检查因本地时间字面量不匹配而拒绝，未停止进程。核精确 UTC 后仅停止此已确认的子编译服务器，原命令随即返回 Unity exit 1。未停止任何 Unity 或用户进程。只读 CIM 首次权限失败及这些诊断/守卫命令完整保留。

| 本批保留自然产物 | 字节数 | SHA-256 |
| --- | ---: | --- |
| `Logs/FMDemoB12C1RedCompile.log` | 61135 | `a90403094b6deb01992cd4c7cab4dd9ea7acd283394a09f7b9cd98e8d349dd01` |
| `Logs/FMDemoB12C1RedTests.log` | 89051 | `b1277d60488298e3af96123a45545df31c73d7e2aa99a425bfc2203f740fac26` |
| `FMDemoB12C1Red-EditMode.xml` | 1853822 | `d53b81e219849de9600337fe4829e7fa9f08d18824ed997a1b5caab0bf3ce94c` |
| `Logs/FMDemoB12C1Compile.log` | 61370 | `0fba8179e54275511bd3d9f95ab9addec1dbe30665ba657b42d55ee48aef24fa` |
| `Logs/FMDemoB12C1Tests.log` | 89037 | `42d8364ef2f25d225553511c6babec4bf96a07e43b1728810bc1d43b92d1b625` |
| `FMDemoB12C1-EditMode.xml` | 1810539 | `dd64480e0d72ba79c32ab798bab908c4ee032eb0d63cf46d8fdbfda0d827f0bd` |

红测完整 log/XML 还在 scope.redVerification.completeProducts 中无损保留；所有压缩项均解压核长度/SHA。六个自然产物未手工改写，未覆盖 B12 或更早证据。

最终证据检查曾引用错误的实验摘要属性名，并在归档该失败命令后用宽泛文本筛选误计 Unity 命令。分别改为已存的 canonicalFileSha256 和实际运行输出标识后重新核对。两次失败原命令均已归档，源码和实验文件未变。

## 文件实验与交回

本次两个全新 GuidN 根均保留，各 164 个用例目录、713 个文件：红阶段 `TestArtifacts/FMDemoB12/4c26efd60ee14771a31fc8bd26d15b54/`，绿阶段 `TestArtifacts/FMDemoB12/71e108849d1d4d6bbbf38e4ea1296af6/`。

新增 1426 文件的相对路径/长度/SHA及 XML fullname 归属在 scope.experiments.manifest，规范 SHA=`3f111550a92216585aeb39a55846dbf5ad5dab4779065ff7517b1cddaab1b910`。原两根的 1090 文件已在开始及结束逐项核长度/SHA，仍为 `39dba4bd1539b930c4103ef525910e83483bfe38ccd56e18b48e8b4b6e2f7a9f`，未修改或清理；路径/reparse 防护沿原已审测试辅助。

本次无 Git 写入、分支/worktree、新任务或子代理；未改旧测试语义、Core/API/marker 格式/asmdef/meta/GUID/配置/权限/旧证据/三协调稿。证据仍仅为同进程 Windows 文件行为，未实施 017/018、跨进程强杀、Android 掉电或 M02 发布。

正式交回后停改。用户没有待补业务决定；R 在本次原 C1 turn completed 后独立复审 F1 和原七项整包门槛，再由 SD00 决定后续派发。
