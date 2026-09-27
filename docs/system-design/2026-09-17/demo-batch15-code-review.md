# FM-DEMO-019 / DEMO-B15-R1 独立审查

VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: PASS，范围包含 §234 明确保留的运行缺口。Verification: C 最终编译和全量 EditMode 的实际 Unity exit 均为 0；R 独立审阅源码、测试、原生记录、文件字节和归档，未运行 Unity。没有未解决的实质问题；两处证据表述的准确口径见下文。

审查仅针对 019。依据 [任务包 §231～235、§237](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md)、仓库审查清单及已批准的应用／输入表现设计。在本任务自行完成规范、规格两轴，不启动代理，不修改实现、作者报告、协调稿或 Git。

## 准确身份与正式完成门

- R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本报告准确 turn：01a0cd4a-9e03-7a41-bd60-f92c959d7d60。
- R 原生 task_started=2026-09-23T08:03:34.560Z；turn_context=08:03:34.612Z，gpt-6-astra／max，cwd=D:/Unity/UnityProj/FightMatch。08:25:35.111Z 的续接上下文仍是同一 turn／模型／effort。
- C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1，任务名「FightMatch 本机保存与应用接入实现」；准确交付 turn：01a0cd4a-8cee-74f1-b46c-d5c44aea027f。
- C 原生 task_started=08:03:30.183Z；turn_context=08:03:30.248Z，gpt-6-astra／max、同一项目。正式 AgentMessage final_answer 于 08:31:05.360Z 发出，delivery=null，明确 COMPLETED，列出两份报告及 SHA。
- C 原生 task_complete=08:31:05.547Z，无 error；wait_threads 随后确认同一 turn completed／error=null、任务 idle，cursor=5c8a73c1-d962-408b-880b-35f970baaab2:6。两份文件的实测 SHA 与正式 final 一致，完成门成立。
- 首轮 C turn=01a0cd29-6366-7d20-be26-e661b6d53cf2 于 08:03:14.532Z 因 usage_limit_exceeded 结束，last_agent_message=null。08:00:45.738Z 的 async final_answer 是范围例外问题，不是交付。
- 首轮 R turn=01a0cd2a-1986-7ea0-8832-312abc64b3e8 同样额度中断，未给出本包裁决。本报告不沿用任一 failed 回合冒充完成。

以下时间均为 2026-09-23 UTC。C 原生记录为 C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl；下文 ordinal 指文件从 1 起的行号，区别于记录自带的 ordinal。

| 正式输入 | 字节／物理行 | R 实测 SHA256 |
| --- | --- | --- |
| [C 交付报告](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-019-delivery.md) | 14286／101，限额 280 行 | af514c1aa52bfb48536ea2307515122c8f0496d329739e0e76b6c2b561435fcf |
| [C scope](/D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-019-scope.json) | 1235900，限额 3145728 字节 | 99d714b0c05cbc71ae73b483050f46d9a2388aeff86aa2128c20d9458eca4db2 |
| [最终独立绑定表](/D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/late/formal-bindings.json) | 6264，21 个绑定项 | d12c83cfb5afb090ce47edfc2d35e40519a68f1ab71fde93448fa26fb89aba93 |

证据根 E15 = D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/。本报告中 E15 下的短路径均按此绝对根解释。

## 起点、授权与规范轴

签发 r117 全文 SHA=d4565a7a711ca9ef9ca6fb7da150c938008e8ba78c8a78f173ecad4f707e5e98。冻结 §232～235 以 CRLF→LF、TrimEnd、单一末尾 LF 后 UTF-8 哈希，始终为 d7d19d42c8dd0b762dc7274dc984892d05f8d7e44f7e2aca224fdbdff8eda88d。

保留 C1 原最终 569 清单及 capturedAtUtc=06:40:30.3566435Z，canonical=5da1c80cde84ca32af8be3c3b43eff7ee0c02b98a476e5953b26d9040c00f3a5。R 于 07:33:53.6134158Z 已独立核对全部起点字节；正式 scope 中 beforeImplementation、entryImplementation 与原 baseline 完全一致，没有用 019 中途状态重建起点。baseline.json 的 SHA=ab68def8a73b0cef7dcfd274a26c7ccd9bb4c31fd3d2b654d937f54886c569e8。

用户于 08:01:48.152Z 在 C 任务原生消息明确回复「允许这一项引用修正并登记范围例外」。该授权仅允许 Application.asmdef 的 references 追加 FlowPuzzle.Core；08:01:48.161Z 对应 UserMessage 完成事件亦核实。它覆盖 §234 的这一项 asmdef 禁令，不覆盖其他配置／meta，也不改变原 569 起点。

| 旧文件 | 独立差异核验 |
| --- | --- |
| [FightMatchDemoArchitecture.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs:19) | Init 只增加一行门面注册，74→75 行；去掉该行即还原起点 SHA=22f074564f8d779363afae7a38dffe7395b6f98827844ec9ddc778a46a83f1db。原 OnDeinit/C1 保护保持。最终 SHA=e5a1ae8cd157db87f30471c0db232f30ca4101cdadfd2f8bfa3a54bfbfee0aa0。 |
| [FightMatch.Application.asmdef](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/FightMatch.Application.asmdef) | 只增加获准引用及必要逗号，461→488 字节；逆转这一项即还原 SHA=f199ecccaa2c33330c8bf0b1e2197c26eecda29129f92eba36ec8a4794d4eb1e。最终 SHA=4a0cc6db82ce715189c9fabb4c2a7c00663681b35636a8fbd74c4c4425c0ea1c，noEngineReferences 等其余字段保持。 |

最终 589 实施项＝原 569 中 567 项逐字节不变＋上述两项授权修改＋20 个新增 Assets 文件。实际 Assets 路径集合恰为原 582＋20＝602；Scripts＋Tests 为 575。新增为指定 10 个 C# 及 10 个 Unity 自然生成的 .cs.meta，没有新目录 meta。320 个 GUID 均有效且唯一，原 310 个 GUID 及 meta 字节不变；原生命令／FileChange 没有手写 meta，逐轮快照显示 compile-01 自然生成 GUID、compile-02 补齐导入器内容。

六份生产文件 693／2000 行，四份测试含 fixture 1197／2400 行，共 1890／4400；两个取证 helper 191／280 行。没有无关重构、额外生产 store／队列／静态业务状态、第三方依赖、正式场景、M04 写入或 H06 奖励实现。原 ApplicationRuntime、Core、Platform、旧测试及权限文件保持，既有公共协议和序列化格式未改。

R 入口的 472 项广义保护输入中，仅三协调稿发生变化；469 项非协调输入全部原字节不变。三稿最终 SHA 与 SD00 turn=01a0cd4e-09cc-7cb0-b6a8-46224e69ad31 于 08:14:15.484Z 的原生写入结果一致，修改内容是版本摘要和续接登记，冻结段未变。另对 C 正式 231 项保护输入全部重算 SHA，与起点 canonical=8177a49b650cc20903377547c7c0a7e403d3e880fc2f17d961b9c4991b960ba5 一致。

规范轴：PASS。按当前用户精确授权判断范围；未将获准引用当成污染，也未放宽其他禁改项。Git 使用现有未提交工作的已接收字节起点；C 原生命令仅见只读 diff/status 检查，R 未做 Git 写入。

## 完整代码与规格轴

R 已逐份阅读六个生产 C#、四个新增测试／fixture、Architecture 改动及相关既有协议，独立检查最终字节，并复核 tests-01 后唯一测试修订。下表锚定所审版本；同目录分别为 Assets/Scripts/FightMatch/Application 和 Assets/Tests/EditMode/FightMatch。

| 新 C# | 行数 | 最终 SHA256 |
| --- | --- | --- |
| [CandidateBattleRequestModels.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleRequestModels.cs) | 110 | 587192094c754ad748e76295b67f4e678e37727a43f549810b3d16f84b759b3e |
| [CandidateBattleRequestFactory.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleRequestFactory.cs) | 154 | d04eff956957b96397f787bb0dd3b5a0d8c1fe30d37edc249e9b7c9e2baf5684 |
| [CandidateBattleApplicationSystem.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs) | 179 | 604834a19da96acd7981f95a0944b4fdfc286b358d5a0c8c66223fcdc26593dd |
| [CandidateBattleApplicationBuilders.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationBuilders.cs) | 143 | f0116d275fa22d969c55ef1fcaef706052574ad8b7597f6dbbdcefcf65ba982a |
| [CandidateDemoView.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateDemoView.cs) | 70 | 5402b0cd0a709caa4cc91b356ac1f7450dce45e76f581425ff92c02628b34e10 |
| [CandidateBattlePresentation.cs](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattlePresentation.cs) | 37 | 60738f5146ee62a490bd71867cfc37cd8b8329267728a0140060cd28f90432be |
| [CandidateBattleRequestTests.cs](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleRequestTests.cs) | 184 | 839f59720cf6b6ab1900be98fd9cea2cea8c49a0dba225306e9fc59e96634522 |
| [CandidateBattleApplicationTests.cs](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleApplicationTests.cs) | 466 | 76006878103c7eaaaed3320eb01315b691b8964667ba12eeb66546e7d5cecb39 |
| [CandidateBattlePresentationTests.cs](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattlePresentationTests.cs) | 345 | 80ef0c1d784a158ffce7e9136d2a333626ccd5af12c9b42e3598c208cee4c904 |
| [CandidateBattleApplicationTestData.cs](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateBattleApplicationTestData.cs) | 202 | 6a353f882e3bb0daadb5a3d23f56aec6892206e1b2ad8d536d950b1d2d80e2d4 |

[Factory.Prepare](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleRequestFactory.cs:17) 在任何调用方列表遍历／复制前检查所有 Count，再核字符串和数字预算；复制完整 Context、Route、确认列表，以一次内部 Guid 生成 canonical 与后续领域输入。公开 prepared 只读，不通过反射或外部 ID 构造业务身份。

[Submit 与恢复委托](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs:28) 保留原 018 的 Lookup／pending 优先次序，新办理才进入门闩及绑定检查。三类 [builder](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationBuilders.cs:24) 复用同一 budget.Codec.Math、原 EvaluateAttack／EvaluateLink／History 操作，仅替换 ActiveHistory；其他四领域和保留根交回原存储协议。时刻、Anchor、随机状态、关闭报告和 S17 身份只在首次 builder 产生，pending 重试不重建。

[只读视图](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateDemoView.cs:42) 从同一完整不可变 PublishedSnapshot 投影，区分无战斗、未配置、恢复受阻等状态。空槽 Enabled=null 原值保留，PreferenceApplicable=false；无道具动作的显式 false 仅在指定 Empty carry 条件成立时有效，不生成默认永久偏好。

[表现交接](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs:138) 只在本实例首次已成且恢复已核的 Attack／Link 后交一次原事实；令牌先于返回 PlayOriginal 挂起。重复结果和 Query 不重播，回退及 Attempt／Scene 变更使旧令牌失效，同 Scene 的单纯 Commit 变化不自动清闩。RebuildLatest 丢弃播放及待交付许可，新实例只重建终态。原 IsCommitted、恢复失败、Lookup、Diagnostic 和 NotificationFailure 保留；H04/S17 不触发 H06。

规格轴：PASS。未发现需要修正的实现缺陷。B15-07、B15-09 的成功／永久发布运行边界按冻结包保留，不将静态检查或拒绝路径测试冒充该分支运行成功。

## B15-01～12 独立验收

表内 Request、Application、Presentation 分别指上述三个测试类。R 已读实际断言；测试名及参数完整多重集合另与 XML 核对。

| 组 | R 核验结果及证据 |
| --- | --- |
| B15-01 | PASS。Request 的 PrepareFreezesEachShapeAndUsesOneNonPubliclySuppliedId、ExecutionUsesFrozenRouteContextAndConditionsAfterCallerMutation、InvalidShapeHasNoAcceptedRequestOrIo、OversizedListsAreRejectedBeforeAnyElementAccess、PreCopyBudgetsPreserveExactLimitDetails：三形状冻结、只读输出、准备后 DTO 变更无效、超限列表零元素访问、精确限制诊断及零存档调用。 |
| B15-02 | PASS。Application 的 RegisteredFacadeCommitsCanonicalAttackAndProjectsOneImmutableRoot 和 NoBattleIsDistinctFromUnconfiguredAndRestoreBlocked：唯一 Architecture 门面、合法入口／真实 M12 profile、canonical／Head／History 绑定、四 owner 保存切片字节及保留根不变、同根查询无业务推进。 |
| B15-03 | PASS。InvalidGeometry 保留原 Stage／Reason／CellIndex；StaleOrDifferentBindings 覆盖 Scene、Preference、Enabled 和 Context 全字段；倒下 Actor／不可用目标原拒绝不丢失。共享 Math 的 step／integer 和 RandomWords 预算分别实测，失败无部分候选；负执行预算按参数错误处理。 |
| B15-04 | PASS。OldResultsWinOverLaterSceneLatchOtherPendingAndOriginalConflict 证明旧结果优先；回退测试补核原 request 的 Query／Submit 为 Superseded。same-ID 异 canonical 冲突沿原 018 公开能力，没有扩大 prepared 生产构造入口。 |
| B15-05 | PASS。真实 Snapshot.Flush.after、Marker.Promote.after、提交后 Load 失败与 End；Retry／Resolve 保存完整候选字节、一次 marker 提升、固定时间／Anchor／Random／资格。用零随机执行预算重复 pending 仍走原办理；新实例 Restore／ResumeObserved 只恢复终态，不播旧事实。 |
| B15-06 | PASS。PreviewAndExactOrderedConfirmationUseRealHistory 的 valid／missing／extra／order／target／scene；PreviewIsReadOnlyAndPreservesHistoryRejections。预览沿 Locate／ReadRange、不分配 ID／保存；确认顺序严格，真实回退恢复前态并生成新 Scene，不覆盖四领域。 |
| B15-07 | PASS，限冻结边界。Link Prepare 与请求映射没有 Actor／Preference；实际 InvalidPhase／过时拒绝透传。静态只见 EvaluateLink＋Append，无额外敌方或随机步骤。合法 AwaitLinks 成功及全倒成功的存储／恢复 NOT RUN。 |
| B15-08 | PASS。LatchPrecedesReturnedPlayAndRepeatedCompletionDoesNotReplayOrQueue 与 PublicationCallbackSeesFullHeadAndBusyReentryAndListenerCannotUndoCommit：先挂闩、快速新请求 Busy、无队列、重复不播；回调可见完整头、突变重入 Busy，监听异常不推翻已成结果。 |
| B15-09 | PASS，保留运行缺口。OldA1CompletionCannotReleaseA2AndTokenHasOnlyThreeOriginalValues；ExternallyPublishedRollbackOrNewAttemptInvalidatesOldToken；RebuildRetiresBothPlayingAndNotYetDeliveredPermission；新实例终态重建。只比较 Attempt＋Scene 的静态路径核实，同 Scene 永久 Commit 的实际发布见证 NOT RUN。 |
| B15-10 | PASS。ClosingH04RetainsOriginalS17AcrossFailureAndRestoreWithoutH06 通过真实公开攻击链到 WonPendingSettlement；失败候选、Retry 后字节和重新打开后的原报告／唯一 S17 一致。Query、重复、播放完成及重建不发经验、掉落、首通或 H06。 |
| B15-11 | PASS。WrongThreadEveryEntryLeavesPlayingTokenAndFilesUntouched、ClosedFacadeCannotTouchNewArchitectureOrLease 覆盖全部十入口、旧门面、新 Architecture 与租约；回调完整头／Busy 已实测。原 C1 Deinit 测试文件字节及回归结果保持。 |
| B15-12 | PASS。原 3069 测试 fullname 的 Ordinal 多重集合精确保留，方法／断言所在旧文件未改；新增 67 项全 Passed，零失败／跳过。589 最终清单、567 原项、320 GUID、实际进程退出、源码／程序集／XML 和完整新增归档已独立核验。 |

## C 实际执行与同版证据

R 未启动 Unity。下表来自 C 真实 Process.ExitCode、运行快照、日志／XML及原生 CommandExecution，相互核对；wrapper exit 不替代 Unity exit。

| C 运行 | PID／开始 UTC | Unity exit | 用途及判定 |
| --- | --- | --- | --- |
| compile-01 | 34380／07:59:23.0706829Z | 1 | 缺 FlowPuzzle.Core 引用；失败留存。 |
| compile-02 | 15044／08:02:49.6562663Z | 0 | 日志仍含旧导入错误、meta 发生自然补齐，wrapper exit=1；不作为最终干净编译。 |
| compile-03 | 16680／08:04:22.5621297Z | 0 | 修订前干净编译；helper 的旧 turn 字面值由原生记录及 turn-correction.json 明确纠正。 |
| tests-01 | 34708／08:06:02.7940314Z | 2 | 3132 通过、4 失败、0 跳过；失败 XML 保留。 |
| compile-04 | 33220／08:13:20.0263311Z | 0 | 最终编译；实际退出 08:13:30.4013917Z，08:13:30.4661612Z 捕获 exit。 |
| tests-02 | 15092／08:14:26.4491781Z | 0 | 最终全量；实际退出 08:17:12.4485576Z，08:17:12.5003043Z 捕获 exit。 |

tests-01 的三项失败来自以战斗专用 fingerprint 比较其他 owner，一项负坐标在 Prepare 提前拒绝。最终只修新增 ApplicationTests：改为四份真实 owner 切片字节比较、非负越界坐标进入领域拒绝，另补 Superseded 断言。R 核前后差异确认没有删除／跳过／弱化用例、没有改生产实现或旧测试。失败 XML SHA=c767a2b2a9490fa74461f9c3c64363407d27393a6440796003e0068dd49f62b9。

六轮启动前 process-check 均记录无 Unity；固定 exe=D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，实测 SHA=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。实际 helper 使用 Start-Process Hidden／PassThru，WaitForExit 后立即存 Process.ExitCode，再采集证据。最终 argv 如下，cwd/projectPath 均为 D:/Unity/UnityProj/FightMatch：

- 编译：-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB15Compile.log。
- 全量：-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB15-EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB15Tests.log。无 -quit、filter 或特殊测试选择环境参数。

compile-04 与 tests-02 的前后实施清单及当前源码 canonical 都为 2e11505885859ed5134dc29663afd07e300045ce1e88253667748afda0726a59。R 重算逐项字节，检查四个时点的 Assets／保护输入／DLL-PDB 副本。compile-04 前后仅 FightMatch.Core.Tests.dll／pdb 因测试修订重新编译；编译后＝测试前＝测试后＝当前 30 个程序集文件，canonical=81203e797c1ff5b058b02b6a10ace0470eee847a9d60e2f5d87dae283f8d52e8。无需错误地要求编译前 DLL 与编译后相同。

| 最终输出 | 字节 | SHA256 |
| --- | --- | --- |
| [Compile 日志](/D:/Unity/UnityProj/FightMatch/Logs/FMDemoB15Compile.log) | 62871 | e92ebbb67a518c76ec964c9cc7a9e7459556da66e1b28a371102b13430f6a329 |
| [Tests 日志](/D:/Unity/UnityProj/FightMatch/Logs/FMDemoB15Tests.log) | 90829 | 03da0971641e872cc5b816486ef3ef8e56ae70db881d4f5845192d2cd2baf0b7 |
| [EditMode XML](/D:/Unity/UnityProj/FightMatch/FMDemoB15-EditMode.xml) | 2184239 | cbd4a490c2092f5574633cad3b0599ece4e407a26d704ac17f2219aa738955a0 |

最终日志与对应 Unity.log／Unity-final.log 副本相同，无编译错误。R 实际解析 XML：3136 个 test-case 全部 Passed，Failed=0、Skipped=0，duration=154.3970904 秒。旧 3069／3064 个不同 fullname 的多重集合保持；新总计 3136／3131 个不同 fullname，新增 Request 22、Application 33、Presentation 12。未把 distinct 数当成用例数。

## 独立归档、保护和原生溯源

R 的旧证据入口元数据采集于 07:35:18.1353309Z，正式完成后出口复核于 08:34:57.3067034Z：48 根、177239 文件、9819238770 字节的全部路径／长度／UTC mtime 一致，0 项需按规则重新哈希。C 的入口清单此前也独立分组比对过这 48 根。遵守已冻结的旧证据继承边界，未重抄历史树、重复旧故障／六进程／强杀矩阵。

除 E15 外仅四个本次自然输出新根，合计 3508 文件；其余根未新增。四根均在 TestArtifacts 下：FMDemoB12/0d3a0d025ae04da8aaf0784271f6cc0a、FMDemoB12/f321a8b9e77e4739b3210985f4c9700e、FMDemoB13/773b3afce39a4e128419c401443d3519、FMDemoB13/f534549889924cc6a2d76218a7420fa1，已完整纳入本包归档。

R 从实际文件系统重新枚举源与 payload，并逐个打开 ZIP entry 计算 SHA，未以 C 的 PASS 字段或 ZIP 清单代替核验：三者 15661 文件、898128032 字节，逐项路径／长度／SHA 全等；实际源、payload、ZIP 与空目录索引均为相同 46 个空目录。source-before／source-after／payload-manifest／zip-manifest 均与实际结果一致，canonical=b5077a2fd1bd643de0afe4e97675b3c5526b9a84e5b5651fc9517c5758c4d447。

[ZIP](/D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB15/2b64e73e69bc4cf0bd85ba8218058402/archive/FMDemoB15-evidence.zip) 为 169629262 字节，SHA=3de9bc14a81c6d097b59519f0d5cb7792ca806713141bdec85816d56ad966b42。明确排除 archive/**、late/** 和两份作者报告以避免自引用；R 核实 scope 的 138 条 evidenceRegistry 和 late/formal-bindings 的全部 21 项当前字节／SHA，零差异。后者单独绑定作者报告、索引、ZIP、日志／XML和原生证据。

late/native-execution.json 为 2778747 字节，SHA=73cb2fb90df5cfa23cdd7c62b84aa4139c67ac2798f3be1c33006c5347a3cdd8。R 将导出的 62 个 CommandExecution＋15 个 FileChange 与实际原生行逐项匹配：77 项 item 内容及时间均相等，截止 ordinal=8223 前无遗漏或重复。另独立读 ordinal=8230 的最终索引校验／Native／Bind 命令、8237 正式 final、8241 task_complete，补足合法自引用截止后的交付门。未导出推理。

证据口径勘正，两项均不影响本次功能验收或最终同版证明：

1. 导出器按最近 task_started 填 associatedTurn，使 ordinal=7975、08:03:34.074Z 才返回的 compile-02 被标为续接 turn。其原生 payload.turn_id、原启动记录及 start.json 均明确属于首轮 turn=01a0cd29-6366-7d20-be26-e661b6d53cf2；准确启动是 08:02:49.6562663Z。此处以原生 payload 为准。最终 compile-04／tests-02 的实际归属均是当前续接，未借该旧运行通过完成门。
2. 作者交付第 91 行的「原 569 源码副本」不能解释为每轮存在 569 份源副本。helper 实际复制 AllAssets＋protected＋DLL，四个未变的 Tools/FMDemoSaveRecovery 输入没有每轮副本：FMDemoSaveRecovery.csproj、NuGet.Config、Program.cs、global.json。它们在完整 569／589 清单中，R 已逐项哈希当前字节并确认与原已接收起点一致；本次未单独重跑该工具的历史矩阵。完整 Unity 输入与程序集副本已核实，旧工具矩阵依冻结规则继承。本报告不扩大为“四个工具均有新副本”的声明。

canonical 计算统一为 Ordinal 排序后的 path＋TAB＋SHA256＋LF，UTF-8 SHA256；旧证据元数据比较另用 path＋TAB＋bytes＋TAB＋UTC mtime＋LF。完整 Assets canonical=686c6727e12c15fca9a1ea43881fbf26805cbc424cfafb4b5be5ae19aac88905。

## 未运行范围与交接

NOT RUN／NOT VERIFIED：

- R 未运行 Unity、项目测试、额外业务代码探针，也未重复 C 验证；本报告区分 C 的运行和 R 的静态／证据复核。
- 合法 AwaitLinks 的 Link 成功、全倒成功及其存储恢复；既有公开链没有已接收合法夹具。实际仅验证可得拒绝路径与静态调用边界。
- 同 Attempt／Scene 下仅永久 Commit 改变的实际发布／表现门闩见证；当前只有静态比较证明，不伪造永久意图。
- 交互式 Editor、PlayMode／Domain Reload 模式、真实触控、正式场景、024 及后续包和完整可玩 Demo 体验。Player build、CI、lint 无已定义命令。

CandidateValidation／CommitEligible=false 的候选边界保持。本裁决只接收 019 规定能力，不宣称整个 Demo 已可玩，也不放行后包。没有需要执行的纠正任务包。若以后撤回本包，应另获明确指令，仅处理这两项授权旧差异和新增白名单，保留全部证据，不改 Git 历史。

R 唯一写入为本报告；交付后停止修改，不发任务间通知。SD00 按本 R 准确 turn 的随后 completed 与唯一正式裁决收件；本报告不预先声称自己的 task_complete 已发生。

