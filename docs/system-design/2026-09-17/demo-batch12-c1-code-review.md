# FM-DEMO-016（C1 复审）/ DEMO-B12-C1-R1 独立审查

VERDICT: ACCEPT

Scope: PASS
Acceptance criteria: PASS
Verification: 原生产代码下 36 个目标红灯；原 2640 项及 12 个旧副本控制用例通过；最终同版编译与全量测试的 Unity Process.ExitCode 均为 0，2688/2688 Passed。
Notes: 证据限于同进程 Windows 文件行为；不证明跨进程强杀、Android 掉电、M02 发布或首 Demo 可玩。

原报告唯一 F1 已关闭；本结论覆盖 FM-DEMO-016 原七项整包契约及 C1 五项验收。未发现仍需修正的实质问题，不需要用户补充业务决定。

## 门槛与独立审查起点

- R：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本次原复审 turn：`01a0c56a-af28-7430-a031-c3c9c3117663`；local，gpt-6-astra / max。
- C：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；本次原 C1 turn：`01a0c56a-105b-7d11-a2f2-038303b4b31a`。工具确认此回合完整 final_answer 与 completed，完成于 2026-09-21 19:44:09 UTC，error=null；正式读补丁始于该门槛之后，没有借用旧 016 回合。
- 契约：r82 §§149–152，§152 为 R 范围、§§150–151 为修正及红绿协议；继承 r81 §§144–147。R 于 19:21:57.7430461 UTC 固定派发整稿 SHA=`8d47524fc3f0bcc6eb97e7b0f081e5cf61de2bdc9155975f628654c7d5bbfee1` 及完整相关章节。
- R 于 19:23:03.5743096 UTC 独立固定 490 项、三目标完整原字节、270 meta／GUID、507 个 Assets 路径和旧两轮 1090 实验文件；均在首次测试编辑 19:25:20.637 UTC 之前。19:23:55 UTC 另固定原 2640 条 XML 名称／结果及 484 个受保护输入。
- 继承起点是原 scope.after 的 `2026-09-21T18:38:31.1422784Z` 与规范 SHA=`4a88037f6d67b95e3ab46faf3d176d7bb7cce0ab8dd7166efecd1b37bc593c1d`。这是原范围已核、业务 NEEDS_FIX 的起点，不把旧结论改写为已接收。
- 原 scope SHA=`f96435dc84d05cf185ac7d4c1837ff9867470f180c5c4c3ec2b752defc0558bf`；原 R 报告 SHA=`b89ca3201bac068ffaf9748bc308f54a77f7fc37d4ac26c6090f11f0fbec0059`。本次实物均保持，原整包已审事实明确继承。
- 本次交付 [demo-016-c1-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-016-c1-delivery.md) 为 105 行，SHA=`f4137ba08ab3781a9b733fbf4944e8eb60712ec5296293e50bc6158868d99d47`；[demo-016-c1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-016-c1-scope.json) 为 604022 字节，低于 614400 字节，SHA=`3de415d1372ca61357a47f835e84c797d859f40077ed09691abfeab732e9f532`。
- 按既有 code-review 两轴方法、`.agent/REVIEW_CHECKLIST.md` 与 `.agent/VALIDATION.md`，直接检查实际源码、独立原字节差异、C 原回合事件、XML、日志、归档和文件现场。R 未运行 Unity／项目测试，未改实现／测试／meta／scope／旧证据，不建任务或子代理，不作 Git 提交、推送或分支操作；唯一写入为本报告。

## F1 关闭依据

[SaveHeadInspection.cs:147](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs:147) 在候选核验、完整当前索引及旧副本归属完成后，重新抛出尚不能归属为 IndexedOldCopy 的最终 marker 所携带的 StorageFailure。当前 snapshot 的读失败也记录在其候选 marker 上，因此同一分支涵盖两类当前头文件，且不会先暴露旧头。

[ReadFile:154](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/SaveHeadInspection.cs:154) 分别包装 Open、Read、Close，使用实际读取文件名生成 `Inspect.<文件名>.<阶段>`，finally 释放实际流。已有 LocalSaveFailure 原样保留；原异常类型／消息由 Cause 传递。snapshot 诊断不再被误标为候选 marker 文件名。

[LocalSaveStore.cs:50](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs:50)、[Prepare:66](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Platform/LocalSaveStore.cs:66) 及 Execute 的原有拒绝路径保持字节不变：Open 已取得租约后仍返回 Opened 会话；失败检查进入 InitialInspection，Inspect／Prepare 返回同一个 LocalSaveFailure，Value 为 null，不到编码回调或新票据接纳。

已确认坏格式／缺正文仍按完成的物理坏证据处理：Malformed 等不会触发新增 StorageFailure 分支；FileNotFoundException／DirectoryNotFoundException 明确排除。旧副本只有在有效最新索引可归属时才豁免 I/O 失败；未知／未归属的当前候选不能借此回退。Limit 仍由 RecordFailure 立即传播，Pending、写门与发布后 CommitUnknown 的原路径未改。

[新增回归:335](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs:335) 的 36 个组合为三个公开入口 × 两类当前最终文件 × Open／Read／Close × before／after。它们先通过真实 Store 提交完整头，再在原 Windows 文件适配和实际流上注入 IOException；未以枚举缺失、删除文件、改坏当前文件或预设结果替代故障。

每例在最终争议断言之前证明 FaultUsed、回调 0、无 pending ticket、目录名称和原 snapshot／marker 字节保持、第二 Store 为 Busy。随后同一会话按原 Commit＋Operation 查回原头，并成功 Prepare／End 新票据；一次性故障解除后无需重建资料。[HeadReadFailure:375](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs:375) 再核失败、空 Value、StorageFailure、原 IOException 类型／消息及精确 Stage／FieldPath。

[旧副本控制:389](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/LocalSaveStoreTests.cs:389) 的 12 例在红绿两阶段都通过。snapshot 组对已索引旧 marker 设置矛盾高代残片，确保选择器实际尝试旧正文并触发所选流故障；有效最新索引仍能归属旧副本，当前头 Ready、原键查询及后续合法准备保持。控制组不被误称为缺陷红灯。

## 两轴及全部验收

| 轴 | 结果 | 依据 |
| --- | --- | --- |
| 规范 | PASS | 两文件、105/200 行，原风格／LF、公开 API／格式／程序集／meta／GUID／配置保持；没有额外框架、清理或恢复范围。 |
| 规格 | PASS | F1 的检查失败与物理坏证据已区分，原异常和真实文件阶段传播；原七项整包行为及回归保持。 |

| C1 §150 项 | 独立结论 |
| --- | --- |
| ① 真实当前头故障 | 36 个组合确实经 Open 初检／Inspect／Prepare 和真实文件流触发；红 XML 的全部失败已到目标断言。 |
| ② 诊断与恢复 | Opened／排他租约保持；检查及 Prepare 失败、Value=null、原 Cause 与准确阶段，零回调／无新票据或文件；同会话恢复全部通过。 |
| ③ 先红后绿 | 红测时两生产 SHA 原样，完整红证据先归档再改生产；同一最终测试字节用于红 F2 和绿 F1，最终真实编译／测试 exit 0。 |
| ④ 原故障语义 | 12 个已归属旧副本 I/O 控制及原物理坏证据／Limit／Pending／无回退／Unknown／重试／End 用例全部保持。 |
| ⑤ 整包与范围 | 原 2640 Ordinal fullname 重数和逐名 Passed 完整保持，新增 48 全过；490 项、270 原 meta／GUID、其他 487 项及旧实验保持。 |

| 原 r81 §147 项 | 整包结论与证据 |
| --- | --- |
| ① 固定格式与隔离 | PASS。原独立最小 snapshot 252 字节／marker 256 字节黄金见证、完整格式破坏和原 Unicode／Purpose 隔离证据仍有效；相关生产／测试字节未改，全部原用例本次红绿均 Passed。 |
| ② 一次接纳与稳定票据 | PASS。原真实 015B 五片三代 Prepare／Encode、015A 回读／Decode、列表和摘要冻结、元数据及索引拒绝仍保持；C1 又实际证明检查失败不调用回调或接纳票据。 |
| ③ 真实提交及原键查询 | PASS。原三代同进程重开、双键一致、旧物理文件缺失后自足索引查询、旧成功票据幂等／异 Store／Ended／Disposed 拒绝全部保持。 |
| ④ 故障分界及同票据恢复 | PASS。原 55 个真实提交故障参数例均 Passed；源码的最终 marker 发布标志、SaveFailed／Blocked／CommitUnknown 分界及原票据稳定重试未改。 |
| ⑤ 排他与重入 | PASS。原真实锁争用／释放／遗留锁、重入并发、回调异常释放 Preparing、能力与 expectedHead 检查保持；新增 36 例再次证明诊断失败仍持租约。 |
| ⑥ 唯一头及未决处置 | PASS，F1 已关闭。新增区分检查未完成与已确认坏证据；索引旧副本规则、坏最新不回退、未知／分叉、Pending／Limit、原已证未提交票据的有限 End 保持。 |
| ⑦ 范围与验证 | PASS。原 19 新文件＋唯一旧 asmdef 引用及 471→490 继承事实保持；C1 仅两文件变更，全部原 2640 回归及新增 48 实际通过，命令／失败／冻结／实验归属可追溯。 |

原七项的详细实现与独立黄金字节论证见已冻结 [B12 原审报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-batch12-code-review.md)。本次以相关文件原字节保持、完整旧回归重跑和 C1 实际红绿确认继承；没有用 48 个新增通过替代整包验收。

## 精确差异、冻结与旧文件保持

| 目标 | 原字节／行 | 原 SHA-256 | 新增／删除 | 最终 SHA-256 |
| --- | --- | --- | --- | --- |
| SaveHeadInspection.cs | 9944／166 | `6ddbeb241187072079409fff12ef8f4663c69ef0aef2c878219eb2f4348a57a6` | +24/-5 | `cc1432c5ebc2ae45311b8c24c3254e828f634ed3cb05f89f45f4bd600f456c7a` |
| LocalSaveStore.cs | 20766／279 | `232fd021eca903d15ddaa43af8fb3c8a4c3db399f6b696831291493eb6592899` | +0/-0 | 与原值相同 |
| LocalSaveStoreTests.cs | 38312／537 | `cd459c4e9062fc5d8c960ae63ad619d364d33341ffeb28dc2955db3fc9355bfd` | +76/-0 | `81375ccf320573cb1b4ffe2a9290d9f401c28f486f39521845bbb152e6ef06d2` |

R 从自己预先保存的完整三份原字节计算行差异，与 C 归档及现物一致；总计 105/200 行，仅两文件变化，测试旧行无删除，LF 保持。最终源码 490 项，规范 SHA=`33328760c7503d1af4581159cfd7ac90312a69566c9b6ec8eab0e13eae53e5d5`；其他 487 项和未改 Store 共 488 项保持。

全部 270 meta 的内容及 GUID 与 R 起点一致，无 GUID 重复；507 个 Assets 文件路径无增删。R 固定的 484 个受保护输入均保持，包含旧报告／scope／日志／XML、三协调稿、规则、Packages、ProjectSettings、权限文件；作者的 45 个保护项也通过。现有 Git 34 个已跟踪差异是先前工作，不能当作 C1 补丁；本次只读 git status／diff 检查成功，diff --check exit 0，无非警告输出。

| 阶段冻结 | 时间 UTC（2026-09-21） | 同一十份手工文件规范 SHA |
| --- | --- | --- |
| B12C1-RED-F1 | 19:25:55.3507718 | `2675b9ee2d767e994a33c6616c1e12646d1d11d5ed5423128abd6ba581038786` |
| B12C1-RED-F2 | 19:30:12.5985604 | `12a4dd1d63aa5232f9dc84bb2af52987b885afddd8b8e1883577b8b71ddf3dbd` |
| B12C1-GREEN-F1 | 19:33:25.0995947 | `eb76899a9742f04d2569ba183d9c1ea8e238677d0929442443b64324cfc54959` |

每阶段十文件清单可重算到所记 SHA；用原 490 清单叠加阶段差异也精确重算全量 SHA。五次运行的前后十文件／全 490 均匹配对应冻结；两个红冻结的两生产文件仍为原 SHA。RED-F2 的测试 SHA 与最终相同，生产唯一修改发生在 19:33:24.359 UTC，晚于实际红测结束及 19:32:33.9208545 UTC 完整红归档，且早于绿冻结。最终测试后没有源码编辑。

## 原始执行及失败完整性

从 C 本次准确原回合读取 42 个 CommandExecution、6 个 FileChange。scope 归档的 22 个关键原命令经 shell 引号解析后，argv 与原事件完全相同；完整 output、cwd、状态、退出码和毫秒时长一致，未截断。全部 6 个非零辅助命令均归档。命令 AST 与 FileChange 显示实际 Unity 启动恰为下列 5 次；手工文件写入限于两份源码、交付及 scope，无手写 meta／日志／XML、额外脚本或 Git 写命令。

| 运行／原 CommandExecution | PID | UTC 起止（2026-09-21） | Process.ExitCode |
| --- | ---: | --- | ---: |
| RED-COMPILE-1 · `exec-f6135ff5-5a0e-43d9-9545-0aabfdf639cd` | 17648 | 19:26:01.1463000 → 19:29:26.2454144 | 1 |
| RED-COMPILE-2 · `exec-8c447567-e331-4f0c-8edd-a6702e4a1ea1` | 34148 | 19:30:17.5182054 → 19:30:26.0586807 | 0 |
| RED-TEST-1 · `exec-0e8ff439-7af3-4018-9279-731581e205e9` | 41864 | 19:30:46.2757013 → 19:31:36.0304073 | 2 |
| GREEN-COMPILE-1 · `exec-4e93aa0e-4df4-4e74-8419-6d07b3fd43f7` | 22028 | 19:33:30.4468708 → 19:33:39.0407483 | 0 |
| GREEN-TEST-1 · `exec-83c45840-980d-48ef-8542-8f2cbcb24cf7` | 9748 | 19:33:58.6161296 → 19:34:48.4142922 | 0 |

五个原命令均校验固定 Unity `D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe` 版本 `2022.3.18f1_d29bea25151d` 和启动前无其他 Unity，使用 Hidden／PassThru／Wait，WaitForExit 后读取实际 Process.Id／ExitCode。项目、logFile、testResults 为正确引用的绝对路径；编译含 -quit，两个测试不含 -quit。原即时输出的参数、PID、UTC、退出码和产物摘要均与 scope／现物或完整归档相符，没有把外层 shell exit 0 当作 Unity exit。

首次红编译真实失败为 `CS0117: Assert does not contain a definition for Multiple`；仅测试兼容断言修正后重冻。原失败日志 41178 字节、SHA=`e30a464a96f8ac5c3127bd8b5910e243b6e916fcb6e6d3547a2837a2e95defe0`，在 19:30:11.335 UTC 归档完成，早于第二次同名编译日志覆盖。该编译错不算 F1 红灯。

原日志显示失败 Unity 已以 1 退出；进程探查只余本轮 19:26:04.473495 UTC 创建的 Roslyn dotnet 服务器 PID 7152。原 CIM 输出及停止守卫核对 Unity 自带运行时路径、VBCSCompiler 命令行、父 PID 56140 与精确创建 UTC，并确认无 Unity。第一次守卫因本地时间字面量不匹配拒绝停止；修正为精确 UTC 文本后仅停止该服务器，原 Start-Process -Wait 随即返回 Unity exit 1。未停止 Unity；首次 CIM 权限失败及诊断守卫记录均保留。

另两次最终辅助核对分别误用实验摘要属性名和宽泛命令文本筛选，原失败命令、退出码、完整输出均保留；后续检查改为 canonicalFileSha256 及实际运行输出标识后通过。没有修源码或重写实验产物来掩盖这些失败。scope 全部 11 份 Brotli 证据均独立解压核长度／SHA，覆盖三原文件、名称集合、完整失败日志、红 log／XML、原命令及实验清单。

## 红绿结果及文件现场

原 `FMDemoB12-EditMode.xml` SHA=`888092f8871282eb309c4209325cde94383f2b2917e9f40f8c8779af4e35859d`，2640 条、2635 个 Ordinal fullname。R 的 fullname＋Count＋Passed 规范 SHA=`e505e00450e6adc734ede8e05ade6804fe78bf35af57e84ec77dc3d795dddd31`；红、绿两轮逐名／重数／Passed 差异均为空，C 保存的基线和新增完整集合与实际 XML 相同。

红 XML 共 2688 条：2652 Passed、36 Failed、0 skipped／inconclusive。24 个 Open／Inspect 失败信息精确为成功 Inspected／非空 SaveHeadInspection／无原诊断；12 个 Prepare 失败精确为 RecoveryBlocked／空异常／Stage=Head，而预期 StorageFailure。全部栈到 HeadReadFailure 第 379 或 381 行，并经过测试第 371 行的最后调用，前置真实故障、现场、租约和恢复断言已通过。最终 XML 实数 2688，全 Passed，36 个当前头及 12 个旧副本新增用例均绿，零跳过／不确定。

| 自然产物 | 字节数 | SHA-256 |
| --- | ---: | --- |
| Logs/FMDemoB12C1RedCompile.log | 61135 | `a90403094b6deb01992cd4c7cab4dd9ea7acd283394a09f7b9cd98e8d349dd01` |
| Logs/FMDemoB12C1RedTests.log | 89051 | `b1277d60488298e3af96123a45545df31c73d7e2aa99a425bfc2203f740fac26` |
| FMDemoB12C1Red-EditMode.xml | 1853822 | `d53b81e219849de9600337fe4829e7fa9f08d18824ed997a1b5caab0bf3ce94c` |
| Logs/FMDemoB12C1Compile.log | 61370 | `0fba8179e54275511bd3d9f95ab9addec1dbe30665ba657b42d55ee48aef24fa` |
| Logs/FMDemoB12C1Tests.log | 89037 | `42d8364ef2f25d225553511c6babec4bf96a07e43b1728810bc1d43b92d1b625` |
| FMDemoB12C1-EditMode.xml | 1810539 | `dd64480e0d72ba79c32ab798bab908c4ee032eb0d63cf46d8fdbfda0d827f0bd` |

六个自然产物均与即时原执行及 scope 摘要相符；红 log／XML 完整压缩归档与现物一致。除已明确记录的首次兼容编译失败外，两次成功编译日志无 C# 编译错误；最终编译明确 return code 0，最终测试 XML、日志和实际进程互相对应。

本次红根为 `TestArtifacts/FMDemoB12/4c26efd60ee14771a31fc8bd26d15b54`，绿根为 `TestArtifacts/FMDemoB12/71e108849d1d4d6bbbf38e4ea1296af6`。各 164 个 GuidN 用例目录／713 文件，全部目录逐项对应实际 XML fullname 输出；1426 个文件的路径／长度／SHA 全部重算，规范 SHA=`3f111550a92216585aeb39a55846dbf5ad5dab4779065ff7517b1cddaab1b910`，无遗漏、未知根、根级文件或 reparse 项。

原两根 `133da444453b41fb94f9c6db1a9a0041`、`9e1e5826674e407aabd3c9f637c06600` 仍各 116 用例／545 文件；完整实际遍历无增删，全部长度／SHA 保持，总 1090 文件规范 SHA=`39dba4bd1539b930c4103ef525910e83483bfe38ccd56e18b48e8b4b6e2f7a9f`。沿用的 LocalSaveTestFiles 绝对路径／reparse 防护及限定单文件删除未改；未清理父目录、旧根或其他现场。

## 交回与停止

本次无需修正包或用户动作。R 将本报告、SHA、本次准确原 R turn 和整包结论交回 SD00，随后 final 结束停改。由 SD00 在本次 C1 与本次 R 准确原回合均 completed 后登记 016 为第 22 个已接收功能包，并按既有流程决定下一 017；R 不代登记或开展 017／018。

