# FM-DEMO-019 / DEMO-B15-C1 作者交付

状态：COMPLETED。019 战斗门面、冻结请求、三类办理、同根只读视图和可丢弃表现门闩已实施，规定编译与最终全量 EditMode 已通过。本报告是 C 的交付事实，不自判 ACCEPT；等待 R 独立裁决。

## 身份、依据与连续起点

- task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次准确续接 turn：01a0cd4a-8cee-74f1-b46c-d5c44aea027f。
- 当前 task_started：2026-09-23T08:03:30.183Z；08:03:30.248Z 原生 turn_context 为 gpt-6-astra／max、cwd=D:\Unity\UnityProj\FightMatch。无降级、ultra、新任务、代理或外部模型。
- 原实施 turn：01a0cd29-6366-7d20-be26-e661b6d53cf2，07:27:16.892Z 开始；08:03:14.532Z 原生 task_complete 带 usage_limit_exceeded 且 last_agent_message=null，不构成正式交付。用户“继续任务”后沿原根续接；详见 continuation-resolution.json。
- 依据 system-task-packets.md §231～235，续接登记 §237；签发 r117 全文 SHA256=d4565a7a711ca9ef9ca6fb7da150c938008e8ba78c8a78f173ecad4f707e5e98。
- 冻结 §232～235 按 CRLF→LF、TrimEnd、末尾单 LF 的 SHA256=d7d19d42c8dd0b762dc7274dc984892d05f8d7e44f7e2aca224fdbdff8eda88d，最终核验保持。协调稿后续追加不冒充原签发全文。
- E15 固定为 TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/。以下证据文件名均相对 E15，完整路径／长度／SHA 索引见 scope。
- 原 ACCEPTed C1 最终 569 项和原 capturedAtUtc=2026-09-23T06:40:30.3566435Z 完整保留；019 实际入口 capturedAtUtc=2026-09-23T07:32:52.3394522Z。没有把部分 019 成果重建为起点。
- 原 569 项 canonical SHA256=5da1c80cde84ca32af8be3c3b43eff7ee0c02b98a476e5953b26d9040c00f3a5；baseline.json SHA256=ab68def8a73b0cef7dcfd274a26c7ccd9bb4c31fd3d2b654d937f54886c569e8。
- [完整 scope](demo-019-scope.json)：1235900 字节；SHA256=99d714b0c05cbc71ae73b483050f46d9a2388aeff86aa2128c20d9458eca4db2。含完整 before／entry／final 清单，不以省略号代替路径。

## 用户已批准的单项范围例外

首轮实际编译证实 Application 缺少 FlowPuzzle.Core 的直接引用，无法使用既有公开 Route DTO 的 FlowPos。§234 原文禁止“任何既有meta／asmdef”变更，故在已有实现与实际失败证据可审阅后请求精确例外。2026-09-23T08:01:48.152Z 同任务原生用户消息明确答复“允许这一项引用修正并登记范围例外”；问题只允许在 Assets/Scripts/FightMatch/Application/FightMatch.Application.asmdef 的 references 追加 FlowPuzzle.Core，其余配置不变。

授权及原消息见 scope-exception.json、authorized-diffs.json 和 late/native-execution.json。此例外优先于冻结包对应禁改项，未改冻结文本、起点时间或旧验证记录。范围据实由原其他 568 项保持调整为 567 项保持。

| 唯一两项旧文件修改 | 起始 SHA256 | 最终 SHA256 |
| --- | --- | --- |
| Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs：Init 增加一行注册，74→75 行，OnDeinit/C1 保护不变 | 22f074564f8d779363afae7a38dffe7395b6f98827844ec9ddc778a46a83f1db | e5a1ae8cd157db87f30471c0db232f30ca4101cdadfd2f8bfa3a54bfbfee0aa0 |
| Assets/Scripts/FightMatch/Application/FightMatch.Application.asmdef：仅上述一条引用及必要逗号，noEngineReferences 等均保持 | f199ecccaa2c33330c8bf0b1e2197c26eecda29129f92eba36ec8a4794d4eb1e | 4a0cc6db82ce715189c9fabb4c2a7c00663681b35636a8fbd74c4c4425c0ea1c |

## 实现和范围

新增生产文件均在 Assets/Scripts/FightMatch/Application/：CandidateBattleRequestModels.cs、CandidateBattleRequestFactory.cs、CandidateBattleApplicationSystem.cs、CandidateBattleApplicationBuilders.cs、CandidateDemoView.cs、CandidateBattlePresentation.cs。新增测试均在 Assets/Tests/EditMode/FightMatch/：CandidateBattleRequestTests.cs、CandidateBattleApplicationTests.cs、CandidateBattlePresentationTests.cs、CandidateBattleApplicationTestData.cs。

- Prepare 先核所有列表长度，再核字符串／数字预算，复制 Context／payload 后只生成一次 OperationId；canonical 和领域请求来自同一份冻结值。没有公开 prepared 构造器、UI 注入 ID 或反射读取 Core 内部数据。
- Submit 先交原 018 查原结果和 pending；表现门闩及领域修订检查在新办理 builder 内。Attack／Link／Rollback 仅复用原公开操作和同一 Math，更新 ActiveHistory，其他领域及保留根交原协议保持。
- 空槽保留 M04 Enabled=null，PreferenceApplicable=false，显式无道具条件为 false；true 草案拒绝，不创建永久默认偏好。
- 保存、提交元数据、完整恢复、原 Diagnostic／NotificationFailure／Lookup 均沿原应用系统。SaveFailed／CommitUnknown／CommittedRestoreRequired 不生成新播放；同实例原 Retry／Resolve 可交原事实一次。
- 仅持当前三值令牌及一次待交付操作身份；先挂门闩再返回 PlayOriginal。旧 A1 回报不能清 A2；新 Attempt／Scene 使旧令牌失效，重复结果和 Query 不重播，重建／新实例仅显示最新终态。
- H04 到 WonPendingSettlement 时保存原 FinalReport 和固定 S17；不组织 H06，不发永久经验、掉落或首通。

最终实施 589、Scripts＋Tests 575、完整 Assets 602、唯一 GUID 320。新增 20 项 Assets 为 10 C#＋Unity 自然生成的 10 .cs.meta；原 567 项逐字节保持，310 旧 meta／GUID 保持，无删除。10 个新 meta 在 compile-01 生成 GUID，在 compile-02 自然补齐 MonoImporter；没有手工创建或改写 meta。

6 生产 C# 合计 693 行／上限 2000；4 测试含 fixture 合计 1197 行／上限 2400；总 1890／4400；两个取证 helper 合计 191／280 行。Architecture 仅 +1／-0，未超 8 行限额。保护输入 231 项 SHA 保持，全部旧测试方法／断言字节保持。Git 四项只读检查 exit0；未 commit、push、建分支／worktree、改权限文件或发任务通知。

## 实际运行和失败保留

每轮先核无 Unity 进程；使用固定 Unity 2022.3.18f1、Start-Process Hidden／PassThru，立即保存实际 Process.ExitCode。exe SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。完整 argv、PID、开始／退出捕获 UTC、源码／保护输入／DLL-PDB 前后清单及日志均在 runs 对应目录。

| 运行 | PID／开始 UTC | 实际 Unity exit | 结果 |
| --- | --- | --- | --- |
| compile-01 | 34380／07:59:23.0706829Z | 1 | 缺少 FlowPuzzle.Core 引用；完整保留。早期缺门面诊断来自新文件导入前编译图，并非第二个生产程序集。 |
| compile-02 | 15044／08:02:49.6562663Z | 0 | 导入后编译完成，但日志含导入前旧错误且 10 meta 自然补齐，wrapper 判未通过；不充作最终干净编译。 |
| compile-03 | 16680／08:04:22.5621297Z | 0 | 稳定源码、无编译错误；原 helper 错留旧 turn 字面值，实际属当前续接，保留 start.json 并以 turn-correction.json＋原生命令明确纠正。 |
| tests-01 | 34708／08:06:02.7940314Z | 2 | 3132/3136 Passed、4 Failed、0 Skipped；原 3069 全通过。 |
| compile-04 | 33220／08:13:20.0263311Z | 0 | 修正新测试后的最终同版编译，无编译错误。 |
| tests-02 | 15092／08:14:26.4491781Z | 0 | 最终完整 3136/3136 Passed、0 Failed、0 Skipped；退出捕获 08:17:12.5003043Z。 |

tests-01 三项误用固定 FMBR01 比较器比较非战斗 owner，一项用负坐标导致 Prepare 提前拒绝。仅修正新增 CandidateBattleApplicationTests.cs：四个 owner 改用真实保存切片字节比较；路线改用非负越界点进入领域拒绝；同时在原回退测试补核门面 Superseded 查询。未删、跳过或弱化用例，未改生产实现或任何旧测试。原失败 XML SHA256=c767a2b2a9490fa74461f9c3c64363407d27393a6440796003e0068dd49f62b9。

RUN — 最终编译，cwd/projectPath=D:\Unity\UnityProj\FightMatch：

    D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe -batchmode -nographics -quit -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB15Compile.log

RUN — 最终完整 EditMode，不带 filter、-quit 或特殊测试选择环境参数：

    D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe -batchmode -nographics -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB15-EditMode.xml -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB15Tests.log

最终 compile-04／tests-02 的源码前后与当前 canonical 均为 2e11505885859ed5134dc29663afd07e300045ce1e88253667748afda0726a59；DLL/PDB canonical=81203e797c1ff5b058b02b6a10ace0470eee847a9d60e2f5d87dae283f8d52e8。canonical 算法为 Ordinal 路径排序后的 path＋TAB＋SHA256＋LF，UTF-8 SHA256。

最终 XML：2184239 字节，SHA256=cbd4a490c2092f5574633cad3b0599ece4e407a26d704ac17f2219aa738955a0。Compile 日志 SHA256=e92ebbb67a518c76ec964c9cc7a9e7459556da66e1b28a371102b13430f6a329；Tests 日志 SHA256=03da0971641e872cc5b816486ef3ef8e56ae70db881d4f5845192d2cd2baf0b7。原 3069 fullname 的 Ordinal 多重集合完整保持；新增 Request 22、Application 33、Presentation 12，共 67 项全部 Passed。

## B15 验收逐项事实

| 组 | 结果／直接证据 |
| --- | --- |
| B15-01 | PASS：三 payload 冻结、准备后调用方修改、形状拒绝、超限列表零访问、字符串／数字限制、只读集合及零存档调用。 |
| B15-02 | PASS：唯一注册门面经真实 M12 profile 完成攻击；canonical／Head／History 绑定、四 owner 原字节与保留根、同根查询无写入。 |
| B15-03 | PASS：原领域 Stage／RouteReason／CellIndex，完整 Context、Scene／偏好／空槽 bool、倒下／不可用目标拒绝；Math／RandomWords 大小预算实测。 |
| B15-04 | PASS：重复、更晚 Scene、另一 pending、当前门闩下原结果优先；回退后 Superseded；原 018 同 ID 异 canonical 的 OperationConflict 复用。 |
| B15-05 | PASS：真实 SaveFailed→Retry、CommitUnknown→Resolve／Retry、已成但 Load 阻塞→恢复、End；同一快照字节／时间／Anchor／Random／资格，一次 marker 提交，新实例仅终态。 |
| B15-06 | PASS：Locate／ReadRange 预览零写；完整有序确认，漏／增／调序／换目标／过时拒绝；真实回退、新 Scene、原四领域保持。 |
| B15-07 | 冻结范围内 PASS；Prepare／领域映射无 Actor／Preference，InvalidPhase／过时透传。合法 AwaitLinks 成功及全倒成功 NOT RUN；静态确认仅 EvaluateLink／Append，无额外敌方或随机调用。 |
| B15-08 | PASS：返回 PlayOriginal 前已有门闩，新动作 Busy 不排队；重复 Completed／Query 不重播；监听异常保留已成结果和原 NotificationFailure。 |
| B15-09 | PASS，保留指定缺口：旧／重复 A1 不解 A2，真实回退／新 Attempt 失效，显式重建及新实例不重挂。同步只比 Attempt＋Scene、不比 Commit；同 Scene 永久发布仅静态核，运行 NOT RUN。 |
| B15-10 | PASS：公开攻击链的 WonPendingSettlement、原报告与固定 S17，经失败／Retry／重新打开一致；Query／重复 H04／播放完成／重建不办 H06、不发收益。 |
| B15-11 | PASS：所有入口 WrongThread／Disposed 无 ID／IO／门闩／新实例副作用；唯一实例、旧门面和租约隔离；发布回调完整头＋突变 Busy；原 C1 Deinit 回归保持 Passed。 |
| B15-12 | PASS：3069＋67 全通过、零跳过；原 567 与旧 310 GUID 保持，新增 20／10 合法，589 最终范围及源码／DLL／XML／真实退出绑定一致。 |

具名完整用例见 scope.validation.newCases 和最终 XML；字段／调用静态定位见 static-contract-review.json。通过事实不替代 R 的独立规范／规格审查。

## 完整证据与交接边界

旧证据 177239 个文件经独立入口／出口路径、长度、UTC mtime 枚举全部一致，复用 C1 已接收字节审计，0 项需要重新哈希；未重拷历史树或重复已接收六进程／强杀矩阵。完整清单分别在 old-artifacts-entry.json／old-artifacts-exit.json，原 569 源码副本及每轮源码／程序集副本均保留。

本次两个全量运行自然新增四个 Guid 根，完整收入源／payload／ZIP：FMDemoB12/0d3a0d025ae04da8aaf0784271f6cc0a、FMDemoB12/f321a8b9e77e4739b3210985f4c9700e、FMDemoB13/773b3afce39a4e128419c401443d3519、FMDemoB13/f534549889924cc6a2d76218a7420fa1，均位于 TestArtifacts 下。

[归档核验](../../../TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/archive/verification.json)：源／payload／ZIP 独立枚举均为 15661 文件、898128032 字节，46 个空目录逐项一致；复制后源 SHA 与 mtime 稳定。完整清单在 archive/source-before.json、source-after.json、payload-manifest.json、zip-manifest.json、empty-directories.json。ZIP 为 169629262 字节，SHA256=3de9bc14a81c6d097b59519f0d5cb7792ca806713141bdec85816d56ad966b42。

[证据 ZIP](../../../TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/archive/FMDemoB15-evidence.zip) 明确排除自身 archive、late 和本次两份正式报告，避免自引用。late/formal-bindings.json 单独绑定最终报告、索引、ZIP、日志／XML和原生证据；自身不嵌自身 SHA。late/native-execution.json 仅导出准确回合实际完成的 CommandExecution／FileChange、生命周期、模型上下文和相关用户消息，不导出推理；采集截止明确，当前 formal final／随后 task_complete 由 R／SD00从准确原生记录核验。

NOT RUN／NOT VERIFIED：合法 Link 成功与全倒成功存储恢复；同 Scene 永久提交的表现门闩运行见证；交互式 Editor、PlayMode、真实触控、正式场景、024 及后包、完整可玩 Demo 体验。Player build／CI／lint 无已定义命令。候选仍为 CandidateValidation／CommitEligible=false；在线、广告、偏好写入和奖励去向明确 NotImplemented。

本次正式交付后停止源码与报告修改；不自发开启后包，不向 R／SD00发送任务通知。回滚仅限本包授权项，仍须另有明确指令，保留全部证据且不修改 Git。
