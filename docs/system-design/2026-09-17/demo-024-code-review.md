# FM-DEMO-024 独立代码审查

VERDICT: NEEDS_FIX

Scope: PASS。Acceptance criteria: 未全部满足，存在一项 P2 时间输入语义问题 F01。
Verification: 独立阅读实现与既有领域接口，并核验 C 的原始运行、文件副本、程序集、XML、范围和归档；本 R 未运行 Unity、项目测试或额外业务探针。
本报告仅裁决 024。3183 个测试全绿不消除 F01。G2 已按随后签发的 §246 在新 C 回合续接，其独立结论须待新回合正式交付，不以此前 BLOCKED 状态提前裁决。

## 身份、冻结输入与正式完成门

- 依据 [system-task-packets.md §240–243](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4634)、AGENTS.md、.agent/PROJECT_CONTEXT.md、CODING_RULES.md、VALIDATION.md、REVIEW_CHECKLIST.md、PLANS.md，以及本包指定的领域设计；后续 §246 明确保留本份 024 独立审查与原完成门。
- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，标题「FightMatch 本机保存与应用接入实现」；准确本批 turn：01a0cd85-45f2-7841-903c-20d443a6314a。
- C 原生 task_started：2026-09-23T09:07:38.635Z；turn_context：09:07:38.681Z 与 09:12:31.196Z，均为 local、gpt-6-astra／max、当前项目。
- C 非异步正式 final：2026-09-23T09:58:08.668Z；本批 task_complete：09:58:08.931Z。wait_threads revision 8 核 completed、error=null、任务 idle。没有将旧 019 完成、报告提前落盘或 09:42:20.250Z 的异步授权问题当成本批完成。
- C 首轮最终组状态为 024 COMPLETED／026-G2 BLOCKED，四份正式报告齐备并停止修改；C 的 COMPLETED 是交付状态，不是 R 接收结论。
- 随后 SD00 按 §246 签发 G2 单项旧断言例外。新 G2 C turn＝01a0cdbf-a687-7b03-859c-bd6e8dcf4f81，2026-09-23T10:11:24Z 开始；R 已核 wait revision 9 的 inProgress／error=null。该新回合不具有返修 024 权限。G2 最终必须核新 completed、formal final 及两份 resume1 报告；本份 024 不等待或重复原有效审查。
- R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确本批 R turn：01a0cd85-f31b-7ec3-b95f-59e73defee20。task_started：2026-09-23T09:08:22.990Z；turn_context：09:08:23.058Z，gpt-6-astra／max。
- r121 签发全文 SHA256：01d0ce5e023a3d9bd961587a292bc8bc7a7dacd1db8b1f70b29323ba604d0e12；R 在 09:09:32.7559343Z 独立核到该原入口。后续协调稿为 r122，不能冒称其全文等于 r121。
- §240–243 规范化 SHA256：a5d9afc9841b504685372cb3672c60f562c4cf04fb72f72c9003a33b3eed618c，入口与 10:06:20.9508384Z 复核一致；规范化为 CRLF→LF、段落末尾 TrimEnd 后加一个 LF、UTF-8。
- 019 三报告及 G1 C2 输入保持。既有 019 独立接收与 G1 C2 接收沿原报告，不重做旧包。本次 Standards／Spec 两轴由 R 本地完成；依 §243 不启动 code-review 技能的额外代理。
- R 只新建本报告及 G2 审查报告；未修作者源码、测试、scope、旧报告或证据，未写 Git、启动代理、新任务或发任务间通知。R 自己的 task_complete 由 SD00 在本次 final 后核验，不预填未来完成事件。

## Standards：范围、边界与实现组织

独立读完六份新生产 C#、五份新测试 C#及 Architecture 窄注册，未发现范围外修改或需要因“代码气味”扩大的重构。六类请求共用原 018 runtime／写门和同一 019 实例，没有第二个 store、租约、队列或事件总线。业务工作位于原 builder；保留旧结果／pending 优先和原双层诊断。

| 核验项 | R 结果 |
| --- | --- |
| 入口 | 019 原 589 项逐项匹配；canonical 为 2e11505885859ed5134dc29663afd07e300045ce1e88253667748afda0726a59。 |
| 024 实际范围 | 589→611；新 22 项为 11 份 C#及其自然 meta；唯一旧修改为 FightMatchDemoArchitecture.cs，+3/-1=4≤8；其余 588 项原字节保持。 |
| 阶段覆盖 | 截至 10:06:20，首轮 G2 未实施，024 冻结 611 项与当时当前全部长度／SHA 相同。随后 §246 允许 G2 覆盖 Core.Tests.asmdef 和 QFrameworkIntegrationTests.cs 两项，其他 609 项须保持；新阶段另审，不能按旧“一项覆盖”误报。 |
| 实现 canonical | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d。 |
| 文件／GUID | Scripts＋Tests 597，Assets 624，331 个唯一有效 GUID；无重复、无异常；原 320 个 meta 保持。 |
| 预算 | 生产 717/2600 行；测试 583/2600 行；evidence.ps1 84 行＋finalize.ps1 100 行＝184/320。delivery 89/280 行，scope 1195830 字节＜3 MiB。 |
| 保护输入 | R 的 479 项扩展保护基线于 10:03:46–49 逐项长度／SHA 复核无变化；其中协调三稿采用已追加协调记录的观测基线。 |
| G1 目录 | 388 文件、6630349 字节；入口／10:04:09 出口元数据 canonical 同为 f14e51bbccbe719163d468bd7a7c1fd0986a5a65bb325eef9f37e78b6cd5dea7。四个原输入另核字节 SHA。 |
| 旧证据 | R 对 53 个旧根入口／09:57:40 出口枚举共 207705 文件、11751984144 字节，路径／长度／UTC mtime 无变化；旧字节审计继承，未重新哈希 11.75 GB。 |

规范轴未发现额外阻断问题；以下 F01 属规格和正确性问题，不以改 Core 或泛化门面作为本轮隐含授权。

## Spec：F01 — P2，冻结时间样本丢失“未提供”标志

位置：[CandidateLifecyclePreparation.cs:136](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateLifecyclePreparation.cs:136)，关键赋值在第 137 行；当前文件 SHA256 为 0d2eae39698b077886e7ee40e8e4f36e473079a05c7e0ffbe6a0cc7ac75e1685。

在真实未完成恢复期和正确 revision／head／内容下，调用方传入以下样本，省略两个单调字段的 setter：

```csharp
new CandidateTimeSample {
    WallUtcMilliseconds = 110,
    ObservedAtUtcMilliseconds = 111,
    Source = "explicit-candidate-device-clock",
    Trust = CandidateTimeTrust.DeviceUntrusted,
    Anomaly = CandidateTimeAnomaly.None
}
```

预期：原 M03／018 返回 MissingField；未提供 MonotonicElapsedMilliseconds 或 MonotonicScopeId 不能被默认为“明确选择无单调样本”。显式给两个字段 null 才是合法的无单调时间输入。这一差别在 [§189:3626](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:3626) 明文要求，§240 又要求先经原预算检查再冻结、保持自然恢复规则和错误族。

实际静态调用链：

1. [CandidateRecoveryClock.cs:18](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs:18) 的两个 setter 分别记录 HasMonotonic／HasScope；未调用时均为 false。原 PrepareTime 第 108、109 行将其判为 MissingField。
2. 原 [CandidateApplicationIntentCodec.cs:204](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationIntentCodec.cs:204) 在复制前校验传入原对象，所以已接收的 018 没有该绕过。
3. 新 Preparation 第 69 行先调用 Time；其第 120–138 行仅检查两个 getter 的 null 配对，然后无条件调用两个新对象 setter。上述缺字段样本因此变成 HasMonotonic=true／HasScope=true 的显式 null 样本。
4. 第 109 行再将被改写的对象交 PrepareIntent，原缺字段检查已无法辨别。随后 [CandidateLifecycleEnd.cs:115](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateLifecycleEnd.cs:115) 将该样本交原 Advance；在其他绑定有效时走 wall delta 分支，可以推进并保存原本应拒绝的恢复请求。
5. 同一 Time 还用于第 71 行 EndTimeSample 和第 96 行 PrepareVictory，结束时间的准备边界同样丢失输入区别；未修改 Core 的校验代码不能抵消前置复制的影响。

这是确定的静态输入／控制流差异，本轮没有执行新探针，不能写成已观察到测试失败。原 47 个新测试全部通过；其时间 helper [CandidateLifecycleTestData.cs:107](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateLifecycleTestData.cs:107) 总会显式设置两个字段，未覆盖“省略”和“显式 null”的区别。

## 逐项验收

| ID | R 独立判断与证据边界 |
| --- | --- |
| B16A-01 | 已覆盖要求通过：显式配方／CreateNew、两 Player 隔离、原 OpenExisting 错误、坏档不覆盖、初始化 SaveFailed 后原 Player／Operation／候选保持。 |
| B16A-02 | 已覆盖要求通过：从当前成长读取 Ready／Stats／HP／revision，精确按 TargetProbability 查 C；锁关、旧 head／revision、缺 C、倒下及预算拒绝，合法入场／Restore 成立。 |
| B16A-03 | 正常 H04→S17→H06 公开链、reserved ID、原入场奖励、一次收益、升级后的下一次入场均有通过见证；时间准备边界仍受 F01 影响，不能宣称全部边界通过。 |
| B16A-04 | 正常退出无奖励／首通、真实倒下恢复与存活不恢复、Challenge 保留、End／无活动历史跨 Restore 有见证；缺字段 EndTimeSample 被归一化的问题未通过。 |
| B16A-05 | 已覆盖要求通过：实测推进随机及受伤后，以三域 InitState／InitSequence、原入场 HP／Stats／PRD 重建；Challenge／Baseline 保持，新 Attempt，仅一次 EndAttempt 创建新 Begin，无重新抽熵。 |
| B16A-06 | 未全部满足 F01；正常样本的未到／到点、回拨、换域、重复、旧 revision 与 Core 对照通过，不能据此接受缺字段样本。 |
| B16A-07 | 已声明范围通过：六 builder 的真实存储／Restore；入场、H06、恢复各 SaveFailed／CommitUnknown 共六组证明固定原候选、ID／时间／随机及 Retry／Resolve；未覆盖完整交叉组合保留。 |
| B16A-08 | 旧结果优先、pending、WrongThread、Disposed、回调突变重入、播放 Busy 无队列、Query 纯读有静态及通过用例；准备阶段缺字段语义受 F01 影响，错误边界未全部满足。 |
| B16A-09 | 通过：原 3136 fullname Ordinal 多重集合、旧方法及断言原字节保持；新 47 全 Passed，零跳过；611 项／331 GUID／运行版本绑定成立。 |

## Unity 与测试证据

C 是唯一 Unity 执行者。R 检查每次无 Unity 进程的前检、固定 Unity 2022.3.18f1、Start-Process -WindowStyle Hidden -PassThru、实际 argv／cwd／PID／UTC 与 WaitForExit 后立即读取的 Process.ExitCode；最终全量无 filter，测试无 -quit。未把 PowerShell 外壳 exit 当成 Unity exit。

| 运行 | PID | 真实开始 UTC（2026-09-23） | 退出码采集 UTC | Unity exit／结果 |
| --- | --- | --- | --- | --- |
| compile-01 | 23368 | 09:32:41.6043229 | 09:32:53.1012881 | 1；首次导入及新测试 OpenExisting 枚举写法错误，失败日志保留。 |
| compile-02 | 9872 | 09:33:46.0414463 | 09:34:13.4123122 | 0。 |
| tests-01 | 19648 | 09:34:42.2694545 | 09:38:09.4058653 | 2；3183 总数，3174 Passed／9 Failed；原 3136 全 Passed。 |
| compile-03 | 4172 | 09:39:48.2276553 | 09:40:08.6027119 | 0。 |
| tests-02 | 5800 | 09:41:08.2201275 | 09:44:34.4834243 | 0；3183/3183 Passed，0 Failed／Skipped；XML duration 194.8952272 秒。 |

tests-01 后只修新测试：七处区分原 BuilderRejected 外层与领域 Diagnostic；一处 int／BigInteger 预期类型；一处坏头下原 Completed 查询仍保留未核 view。R 对照实际差异，保留精确领域码、字段、未提交／未写盘等断言，没有用降低断言掩盖生产失败；此后生产源码未变。

每轮 before／after 的实现、Assets、保护输入及 DLL/PDB 清单与实际副本逐项核对；本轮四个既有 Tools 也有副本。首次 compile-01 前尚无 11 个新自然 meta；其后副本集合反映真实导入状态，未伪造提前存在的 meta。

- 最终 compile-03 后与 tests-02 前后 30 个 DLL/PDB canonical：3997f0cf1d5c55a80c009cb8ec36e40aebb937d0c53bb8b78676713ae155b9bd。
- 全 Assets 624 项 canonical：65746d65c0e6f8c0fc03511356af45bbe749c07986c3555b01af449ea5ed16bb。
- C 的 244 保护输入 canonical：2a06de6937d20d6ff7f68850abdab4816e355c60829f896b6856a17ebc849e1a。
- [最终原始 XML](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024/runs/tests-02/EditMode.xml)：2228175 字节；SHA256=0219aec144aaa9ac560ff39a38cc2d02111f6499f6afcb3317be02c88cbc994f。
- 原 3136 fullname Ordinal 多重集合 SHA256=3bbb69a0a41b69b08197d46dde74eb7c1ea062e69255ab33515a561f21ef51bd；在新 XML 提取同集合一致。全部 3183 名称 SHA256=ab16984d1358fac676a6b770d1de757bccadba72eb3bcee5a20a25387eb56caa。
- 非阻断报告瑕疵：C delivery 的五行 Unity 表为空。上述结果来自 scope.validation.runs 和各次原始 start／exit／日志／XML，不来自空表；纠正交付应补齐表格，不修改旧原物。

## 归档与正式文件绑定

E16＝D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c。
R 在 09:53:53 独立逐项读取源、payload 和 ZIP 条目核路径／长度／SHA；14939 文件、862683489 字节及 66 空目录集合一致，无差异。10:06:54 又枚举 E16（排除 archive／late）与四个自然新根，共 14034 文件均被归档覆盖，无遗漏或虚列。

- 新源／payload／ZIP 条目 canonical：35fd092f9384c7ad413f7965bf0ef78a18098ff70c35423020c8b99844d6e3bf。
- ZIP：E16/archive/stage024/FMDemoB16-stage024.zip，157919302 字节，SHA256=d8844ce81792943d97d5e48f7091e48b97fef60c48ed8fd7cdad9e431b95adf2。
- 自然新根：FMDemoB12/7327ac5b829c4f7996512af79917af8f、FMDemoB12/7e22702e99af4db98f402690b53d6252、FMDemoB13/68f001d9ac9f4136adbeae18a0fbd95c、FMDemoB13/fde3235df4004dddabad97ac269f1e83，均在项目 TestArtifacts 下。
- 自引用排除 archive／late／正式报告；stage024-bindings 的 19 条、stage026g2-bindings 的 11 条长度／SHA 全匹配。后者本体 3693 字节，SHA256=04da332bd0eb2abad046c525442699156ec3c91d4066708aabe6e4127a4c3821，与 C formal final 一致。
- 原生导出 stage024 的 65 条、stage026g2 的 68 条 CommandExecution／FileChange、两项 context 及 task_started 逐条与准确本轮原生文件相符，无推理输出。导出字段 ordinal 实际是 JSONL 的一基物理行号，不当作原生事件 ordinal。最后晚绑定命令及其后正式 final／task_complete 另从原生文件核验。
- [demo-024-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-delivery.md)：11183 字节，SHA256=0b218616a785ad76f2eeccfd29a92075ab8cec8bd9bab8349d7c593fb5bbde92。
- [demo-024-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-scope.json)：1195830 字节，SHA256=7ceb67db85b7282ccd9e7e943899959321642b524d3e7b421eea04c0942d0342。
- 原 G2 BLOCKED 两报告已核收并保持历史；其状态不是 §246 新 G2 的完成。新阶段须审 demo-026-g2-resume1-delivery.md／demo-026-g2-resume1-scope.json，并另写唯一 G2 独立报告。

## 最小纠正任务提案：FM-DEMO-024-C1

目标仅为保留 CandidateTimeSample 的字段提供语义，补回归见证；不重做已通过的六条业务主链。
本节是供 SD00 签发的纠正包，不是 R 越权修改或对受保护 Core／public API 的现成授权。现有 §240 禁止改 Core；若采用下述最小可见性改动，SD00 必须在新冻结包中逐项批准后再由原 C 实施。

建议白名单及预算：

- Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs：仅把现有 HasMonotonic、HasScope 两个只读查询属性的 getter 可见性从 internal 改 public，private setter 不变；+2/-2≤4 行。不改两个 setter 的记录逻辑、PrepareTime、Advance、枚举、时间规则或编码格式。
- Assets/Scripts/FightMatch/Application/CandidateLifecyclePreparation.cs：Time 在复制前拒绝未提供标志，保留 MissingField 和具体字段；仅该方法窄改，新增＋删除≤24 行。不得用临时伪造业务意图、反射、内部 friend 或另造领域状态绕过检查。
- Assets/Tests/EditMode/FightMatch/CandidateLifecyclePreparationTests.cs、CandidateLifecycleRecoveryTests.cs、CandidateLifecycleEndTests.cs：只增加本问题回归，三文件新增＋删除合计≤220 行；现有 3183 名称、方法和断言保持。
- 新交付仅建议 demo-024-c1-delivery.md（≤180 行）、demo-024-c1-scope.json（≤3 MiB），位于现有报告目录；证据新根由 SD00 新包明确指定，旧 E16 和旧报告不改。
- 不新增实现文件／meta／asmdef；不改 Architecture、018／019、存储格式、Packages、ProjectSettings、G1/G2、其他 Core 或旧测试。若公开只读属性方案不获签发，原 C 必须明确 BLOCKED，不自行扩大入口。

验收：分别省略两个字段、仅省略任一个时，Prepare／PrepareVictory／EndTimeSample 的适用路径拒绝并标出原缺字段；请求不发布、不触盘、不推进恢复。显式双 null 仍合法，显式零及相同／不同单调域仍沿原规则。用真实未完成恢复期对照 CandidateRecoveryClock.Advance，证明拒绝与正例；准备后修改原对象、同一 prepared 的 Retry／Resolve 仍冻结原时间。

验证：先保留窄回归在当前实现的失败证据，再做修正后编译及无 filter 全量 EditMode；不得为凑结果重跑旧强杀矩阵。核原 3183 全保留、全部新增回归 Passed／零跳过，真实 Unity exit、源码／DLL／XML同版、611 项及331 GUID、受保护输入和完整新证据。报告表需填实际 PID／时间／退出。由 R 按新包独立复审，不能用本次全绿替代修正验证。

## NOT RUN／NOT VERIFIED 与交接限制

- R 自行运行 Unity／项目测试／新问题探针：NOT RUN，依 §243；F01 是静态发现。
- 六操作×全部保存断点×所有 Retry／Resolve／End 的完整交叉组合未覆盖；只承认上列实际运行。
- 合法 AwaitLinks／全倒补线成功及由此形成最终全倒胜利、同 Scene 永久发布保留播放令牌的运行见证，沿既有缺口，不制造内部快照。
- 025 正式关卡／数值／PRD C 发布来源及正式 PlayerSave 能力未验证；仍为 CandidateValidation、CommitEligible=false。
- 交互式 Editor、PlayMode、物理鼠标／触控、027 动画、028 完整面板、029 正式场景、完整 Demo、Player build、CI／性能均未验证。
- 018／019 旧矩阵及 G1 C2 原 83／23／42 见证未重跑；§184 更早时间／stderr 等历史 NOT VERIFIED 原样保留，不要求无关已接收包重做。
- 024 的范围、验证和正常业务链可以保留，但 F01 修正前不能独立放行。G2 独立报告不能替代本问题修复；本批不得推进为两包均已接收。
- 本报告 SHA256 在本次正式 final 中给出，避免自引用；写入与复核后 R 停改，不向 SD00 发通知。
