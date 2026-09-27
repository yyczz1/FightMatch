# FM-DEMO-025-P1-C1 · 严格 Unicode 指纹纠正

作者状态：COMPLETED；本次准确 turn=01a0cebc-4911-7d02-9fb9-d1e5661a42d1，local / gpt-6-astra / max。作者交付等待 R 独立结论，不自判 ACCEPT。原 027 ACCEPT 保持。

授权：system-task-packets.md r131 §263–265，签发整稿 SHA256=efe585203d3beb21ec91236542aaf8ca641777619f26464c26a199b3cf10abac；冻结片段 SHA256=38d35d93c6f53c23e87333c2062ea5df93783ee28726976c62e5a2cf20c5edc5。§266 仅追加准确派发身份，入口实读整稿及冻结片段已保存。没有 Git 写入、新任务/代理、外部模型或协调通知。

## 实际修正与范围

原版 PrepareCandidate 在同一有效 DemoContentJob 下接纳两个只在 PlayerId 上不同的输入 P\uD800、P\uD801。Character/Entry 各自保留不同 UTF-16 码元，但默认 UTF-8 替换产生相同指纹；原红阶段真实输出与断言堆栈保留。

DemoContentCompiler.cs 仅 3 增 1 删：中央 Canonical.S 使用 new UTF8Encoding(false, true)，PrepareCandidate 捕获 EncoderFallbackException 并返回 InvalidValue / Fingerprint.String。非法输入不漏异常、不产生候选或指纹、不改写身份。所有规范字符串共用该编码器；原空值前缀、合法 UTF-8 字节、长度前缀、来源/语义顺序、整数/分数编码与既有前置诊断不变。

DemoContentCompilerTests.cs 只追加 57 行，具名回归恰为 F1-D800、F1-D801、F1-FFFD、F1-PAIR。非法值在方法体中构造，TestCase 只传整数，避免特性元数据编码先替换代理项。测试经真实公开入口，记录转义 UTF-16、成功/拒绝状态、Character/Entry 身份、指纹和 job 当前性；合法控制重复准备且互异。

入口完整 668 项逐路径长度/SHA 与原 demo-027-scope.json 相符，canonical=243121465a9724a87e3e93f570273a55a6f98e08085591b90974c5091389fab4。最终仍 668 项，其中仅两项获准修改、666 项保持；全部 Assets 681 项、361 个 meta/GUID 原字节保持。没有新增实现文件、测试文件、meta 或其他配置。生产改动 4/80 行，测试追加 57/180 行，两新 helper 合计 177/360 行。

## 红绿验证

| 运行 | PID | 实际开始 → 退出 UTC | Unity 实际退出码 | 结果 |
| --- | --- | --- | --- | --- |
| red-compile-01 | 37468 | 2026-09-23T15:00:36.7642056Z → 2026-09-23T15:01:36.1831336Z | 0 | 干净编译，无编译错误。 |
| red-tests-01 | 26800 | 2026-09-23T15:02:58.9243255Z → 2026-09-23T15:13:09.6724980Z | 2 | 3424 总数；3422 通过、2 预期失败、0 跳过。 |
| green-compile-01 | 20736 | 2026-09-23T15:16:02.1827880Z → 2026-09-23T15:16:17.2555717Z | 0 | 干净编译，无编译错误。 |
| green-tests-01 | 34696 | 2026-09-23T15:17:04.6835929Z → 2026-09-23T15:27:22.2403851Z | 0 | 3424/3424 通过；失败 0、跳过 0。 |

固定 Unity 2022.3.18f1，EXE SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。每轮无 Unity 时 Hidden/PassThru 启动，完整 argv、cwd、PID、时间、真实 Process.ExitCode、stdout/stderr 与稳定日志见 runs。tests 无 filter、无 -quit、无选择环境参数；每轮源/Assets/保护输入/DLL 前后实际副本和清单齐备，编译/测试/最终源码与 DLL 绑定一致。红轮生产 SHA 始终为 29b699aa0e264de686fe07c4135733dbe7de67e55e5f7df25600a90e568e2c15，红绿四回归源码相同。

两个红失败都位于 DemoContentCompilerTests.cs:153 的期望 InvalidValue 断言，实际值 null；不是编译或装配错误。原 XML 顶层实际状态为 Failed(Child)，本次初版 helper 只接受 Failed，故脚本自身退出 1。原 verification.json 的 expectedOutcome=false、wrapper-failure.json、Unity exit 2、XML、输出和旧 helper 实际副本均保留；result-interpretation.json 绑定原件 SHA 并补充准确判读，新 helper 接受该 NUnit 状态。有效红轮没有重跑。

首次范围审计脚本退出 1：PowerShell 将局部循环变量 phase 与带 ValidateSet 的 Phase 参数视为同名。仅重命名新 helper 的该局部变量，失败记录及运行时旧 helper 副本保留于新根；源码和 Unity 结果未变，没有重跑有效验证。

报告生成首次因点入旧只读函数库的有类型 Run 变量与局部 run 同名而在读清单前退出 1，未创建正式报告。只改内联生成命令的局部变量名；失败记录在 late/report-attempt-01-failure.json，未改旧工具或重跑 Unity。

红 XML SHA256=21b271198bcd45c692eca5fbd9ddf2b3b9c8c50fa8443bd7413b1fe11eda1bf4。绿 XML SHA256=b3331b5bcbc76794f487aa1401fe1c177be74fd0accf8df6c0c685e14134b385。
红阶段源码 canonical=e4cdc6c0b1e42b30a7689ffd43c3916d2c84c10fca200f241607c5d654378509；最终源码 canonical=c11a8a8aef0e46e00d364836e8baab3baea01fa45c81adddefae7cd7b8c84ea0；最终 36 DLL/PDB canonical=6609b54e1e0e89628866e6d5f4a51315512710d10e05efd468650e1296378bdd。

| 公开入口输入/用例 | 红阶段 | 绿阶段 |
| --- | --- | --- |
| P\uD800 / F1-D800 | 成功；原身份保留；旧碰撞指纹 7c47b4801487d6ea1042f126f250e123ad183bdba22a284fef16a09a2b195ab4 | InvalidValue；Candidate=null；无指纹/异常；job 仍有效。 |
| P\uD801 / F1-D801 | 成功；不同原身份保留；与上行相同旧指纹。 | InvalidValue；Candidate=null；无指纹/异常；job 仍有效。 |
| P\uFFFD / F1-FFFD | 成功、身份保留、重复稳定；7c47b4801487d6ea1042f126f250e123ad183bdba22a284fef16a09a2b195ab4 | 成功，指纹与红阶段完全一致。 |
| P\uD83D\uDE00 / F1-PAIR | 成功、身份保留、重复稳定；2a50ed6fef37733a2308e58fda31f4017ec1064d4c6e2cb8f06e4f4ddb36b9c7 | 成功，指纹与红阶段完全一致。 |
| P / 两合法用例内辅助控制 | 成功；d97476b542a3962959aeff3dff33381d507e4943eb7146ae3a793ffe720d71e1 | 成功，指纹不变；与上两合法身份均不同。 |

## 验收与证据保全

C1-01：真实红碰撞已复现；绿阶段两个非法输入通过公开错误封套明确拒绝。C1-02：合法控制红绿兼容，原取消/修订/预算等用例保持通过。C1-03：红绿均保留原 3420 fullname 多重集合；原测试体、断言、名称、设置原文保持，只新增四例。C1-04：668→668、两项变化/666 保持、361 GUID、全部保护输入与历史证据审计通过。

唯一新根为 TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1-c1。入口、每轮实际源码/Assets/保护输入/DLL、红失败与实际输出保留。308 项保护输入 SHA 不变；原 P1/027 作者与 R 报告、候选提案、E17 原 helper、stage025p1/stage027 完整树只读。旧 356954 个历史证据文件入口/出口路径、长度和 mtime 一致，历史全字节结论继承，没有重复全树哈希或复制。

本轮自然新根：TestArtifacts/FMDemoB12/a82dc5fbac7c488d8371e6b1faf70350；TestArtifacts/FMDemoB12/dc9812a86b32464abd3bb127f1781328；TestArtifacts/FMDemoB13/17312a984e0c481d9a3dc55ec1da0c4d；TestArtifacts/FMDemoB13/e6d412e2afe941098542e43dcc21200b。全部纳入新归档；新源/payload/ZIP 共 15561 文件、1218686444 字节、68 空目录，逐路径/长度/SHA/空目录集合全等，归档后源稳定，canonical=829b875d92dcc8d9e75e55796f8802717a45f3a6f72aec1dd1b78507670f8e4c。
ZIP=TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526/stage025p1-c1/archive/FMDemoB17-025-p1-c1.zip；178971919 字节；SHA256=625d7315d3e3bc4d1bc1f954799925499ea9ba709c26cc11219dab72da69427a。

scope 保存完整 before668、implementation668、两项 diff、保护清单、GUID、各轮进程及源码/DLL/XML绑定、四例输出/合法控制指纹、历史元数据和新归档清单定位。archive、late、两新正式报告排除归档自引用；late/bindings.json 最后绑定报告、归档、元数据、六稳定输出与准确回合非推理记录，其自身 SHA 由 formal final 给出。准确 task_complete 由 SD00/R 在 formal final 后核实，未提前声称原生回合已完成。

## 未运行与边界

原 P1/027 独立验证没有另跑；只执行本纠正规定的红/绿编译与全量测试，未重复有效轮。Player build、真实鼠标/触控/真机、交互式 PlayMode、像素绘制、完整 Demo、实际 OS 低内存、真实调度器内部抛错和额外故障交叉：NOT RUN。原 027 未列成功见证的 AwaitLinks/全倒补线、同 Scene 永久提交、零事实 PlayOriginal 与跨面中间表现边界保持。

坐标、身份/版本/行为/奖励映射、31 档 C/epsilon 与新档仍待内容批准；原 publication-proposal.md 未改。Publish、ResolveExact、PlayerSave 新档桥接和 025-P2：NOT IMPLEMENTED / NOT RUN。未实施 028/029，本技术纠正不接收 025 整体、不重开 027。回退仅限两个入口副本对应的本次生产三处修改和 57 行追加测试；不重置工作区或历史证据。
