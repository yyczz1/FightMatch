# FM-DEMO-024 交付

状态：COMPLETED。按 §240–242 完成候选生命周期接入，等待独立 R 技术裁决；C 不自判 ACCEPT。
准确实施 turn：01a0cd85-45f2-7841-903c-20d443a6314a；任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；gpt-6-astra／max，local。
证据根：TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c。源码、运行证据和原起点完整保留；无 Git 写入、新任务、代理或任务间通知。

## 实现与冻结输入

CandidateLifecycleApplicationSystem 与原 018／019 共用 Architecture、runtime 和保存写门。Initialize、Enter、SettleVictory、Exit、Restart、AdvanceRecovery 只在 018 builder 内连接现有 Core 负责者。
Prepare 冻结内容列表、角色配方、DTO 与设备时间样本；只接受已准备定义与明确 C 表。普通请求固定一次 OperationId，新档固定一次 PlayerId；胜利使用已保存 S17 reserved ID。
入场读取当前 ComputeBaseStats／Ready／revision，经 CheckEntry、BeginAttempt、Inventory.Freeze、BattleEntryPreparer 和真实 RandomNumberGenerator 生成 48 字节材料。C 按实际目标概率精确匹配，不自行求解。
H06 读取原 FinalReport 和入场等级固定奖励，一次候选包含成长、材料、进度及结束凭据；不等待动画。退出不调用奖励；真实倒下才建立恢复。
重开只 EndAttempt(ImmediateRestart) 一次；保持原 Challenge／Baseline，从公开 PreparedEntry 与 SC01 三域 InitState／InitSequence 重建原初态，不调用熵源，不把永久成长／奖励回滚。
恢复仅将冻结 TimeSample／RecoveryId／期望 revision 交 CandidateRecoveryClock.Advance；查询不推进。View 明确已核状态，结束显示无 ActiveHistory 与存储的 End／BaseReward，不伪造 Closed 战斗快照。
保留 018 双层错误：builder 拒绝的 Code 为 BuilderRejected，Diagnostic.Code 给 Locked／NoReadyMember／Busy 等实际原因；调用方不得只读外层标签。
r121 签发全文 SHA256：01d0ce5e023a3d9bd961587a292bc8bc7a7dacd1db8b1f70b29323ba604d0e12。§240–243 规范化 SHA256：a5d9afc9841b504685372cb3672c60f562c4cf04fb72f72c9003a33b3eed618c。入口复制为已追加 §244 的当前协调稿，全文观测 SHA 单列，不冒称 r121 全文副本。
019 的旧范围例外已含在 589 项入口中，本阶段未重改旧 asmdef 或 QFramework 断言。G1 C2 和 §184 更早缺证边界继续保留。

## 范围与预算

| 项目 | 实际结果 |
| --- | --- |
| 实现项 | 589→611；22 个新项，Architecture 一个旧修改，其余588项原字节保持。 |
| 窄修改 | Architecture +3/-1=4≤8；原 OnDeinit、runtime 与注册语义保持。 |
| 文件集合 | Scripts＋Tests 597；Assets 624；GUID 331，唯一且原320的meta字节不变。 |
| 行数 | 6份生产 717≤2600；5份测试 583≤2600；2份helper 184≤320。 |
| 新生产 | CandidateLifecycleModels、Preparation、ApplicationSystem、Entry、End、View.cs 及自然 meta。 |
| 新测试 | CandidateLifecyclePreparationTests、EntryTests、EndTests、RecoveryTests、TestData.cs 及自然 meta。 |
| 实现 canonical | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d |
| 全 Assets canonical | 65746d65c0e6f8c0fc03511356af45bbe749c07986c3555b01af449ea5ed16bb |
| 244 保护输入 canonical | 2a06de6937d20d6ff7f68850abdab4816e355c60829f896b6856a17ebc849e1a |
| 30 DLL/PDB canonical | 3997f0cf1d5c55a80c009cb8ec36e40aebb937d0c53bb8b78676713ae155b9bd |
scope 完整列 before／after 每项路径、长度、SHA，不只列差异。Canonical 为 Ordinal path＋TAB＋小写 SHA＋LF。每次运行保存实现（含4个Tools）、Assets、保护输入、DLL/PDB 副本及清单。

## 实际 Unity 验证

固定 Unity 2022.3.18f1；每次先核无 Unity 进程，Start-Process -WindowStyle Hidden -PassThru，实际 Process.ExitCode 在 WaitForExit 后立即保存。无过滤、无特殊用例选择参数，测试不带 -quit。
编译 argv：-batchmode -nographics -quit -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB16ACompile.log。
测试 argv：-batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB16A-EditMode.xml -projectPath D:/Unity/UnityProj/FightMatch -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB16ATests.log。

| 运行 | PID／实际退出 | 结果与保留原因 |
| --- | --- | --- |
|  | ／ |  |
|  | ／ |  |
|  | ／ |  |
|  | ／ |  |
|  | ／ |  |

最终 compile-03 与 tests-02 的当前源码、DLL/PDB 清单一致。旧3136 fullname按Ordinal多重集合逐项保留，旧测试文件原字节不变；新增47项全部Passed。未重复018/019已接收矩阵。
最终 XML SHA256：0219aec144aaa9ac560ff39a38cc2d02111f6499f6afcb3317be02c88cbc994f。每次argv、cwd、开始/结束UTC、实际PID和退出码见stage024/runs；不是包装器退出码替代Unity退出码。

## 验收映射

| ID | 结果／具名见证 |
| --- | --- |
| B16A-01 | PASS：显式 CreateNew、两 Player、NoSave／坏档／租约及初始化 SaveFailed；坏档查询旧 Completed 仍保留未核状态且不覆盖文件。 见 ExplicitProfileEntryAndRestoreUseRealGrowthAndEmptyCarry; SeparateExplicitProfilesRemainIsolatedInOneRoot; ExistingOpenFailuresNeverCreateAProfile; CorruptExistingHeadCannotBeOverwrittenByProfileConvenienceCall; InitializeSaveFailureRetainsPlayerOperationAndOriginalCandidate。 |
| B16A-02 | PASS：真实成长、revision、槽位、HP 和匹配 C 入场；锁关、旧头、旧 revision、缺 C、倒下与预算拒绝。 见 ExplicitProfileEntryAndRestoreUseRealGrowthAndEmptyCarry; InvalidEntryDoesNotPublishOrWrite; DownCharacterCannotEnterAfterExit; EntryDtoMutationDoesNotChangePreparedIntent; PreparationPreservesExactBudgetDiagnostics。 |
| B16A-03 | PASS：公开攻击形成 S17 后按 reserved ID 结算；固定收益一次、旧请求优先、重新打开后原结果、升级下一局新 Stats/C、旧报告仍原入场；不等待播放。 见 StoredS17SettlesOnceWithoutWaitingForPresentationAndNextEntryUsesNewGrowth; RewardEvaluationUsesTheSuppliedSaveMathAndTightenedScopeLimits; VictoryPendingCannotBeRewrittenAsExitOrRestart。 |
| B16A-04 | PASS：真实倒下退出产生恢复，存活退出不产生；无奖励／首通、未完成 Challenge 复用、End 与无活动历史可恢复。 见 ExitWithoutRewardPreservesChallengeAndActualEndAcrossRestore; NaturalRecoveryUsesCoreTimeRulesAndPersistsOriginalResult; DownCharacterCannotEnterAfterExit。 |
| B16A-05 | PASS：实际随机推进／受伤后，重开复原原入场 HP／Stats、三域 InitState／InitSequence、PRD；Challenge/Baseline 保持、新 Attempt，只有两个 Begin 记录、无恢复重起。 见 ImmediateRestartUsesOriginalEntryAndAllThreeInitialRandomDomains; EndRequestsRespectPlaybackAndNeverQueue。 |
| B16A-06 | PASS：未到／到点、回拨／换域、同样本、旧 revision／已完成重复均与 Core 结果对照；TimeSample 深复制且查询不推进。 见 NaturalRecoveryUsesCoreTimeRulesAndPersistsOriginalResult; RecoveryRejectsStaleRevisionButCompletedPeriodKeepsCoreAlreadyIncludedRule; QueryDoesNotAdvanceRecoveryOrResampleItsStart。 |
| B16A-07 | PASS：六种 builder 真实保存＋Restore；入场／H06／恢复 × SaveFailed/Unknown 六组证明 Retry/Resolve 原字节、原身份一次发布；未覆盖交叉组合单列。 见 OriginalCandidateSurvivesRealSaveFailureAndResolution; InitializeSaveFailureRetainsPlayerOperationAndOriginalCandidate; all successful builder/Restore tests。 |
| B16A-08 | PASS：WrongThread、Disposed、发布重入、播放 Busy 无队列、pending 拒绝新写、查询纯读；保留原 BuilderRejected＋领域 Diagnostic 双层错误。 见 OwnerThreadAndClosedFacadeRejectBeforeInspectingCallerInput; PublicationReentryCannotPrepareOrMutateAndQueriesSeeOneHead; EndRequestsRespectPlaybackAndNeverQueue; OriginalCandidateSurvivesRealSaveFailureAndResolution。 |
| B16A-09 | PASS：原3136具名多重集合及旧测试方法／断言原字节保持；最终全量、范围、GUID、源码／DLL／XML一致。 见 stage024/audit.json and final run manifests。 |

## 证据保全与归档

历史 207705 个文件作完整入口/出口路径、长度、UTC mtime枚举，全部未变；沿已接收字节审计，不宣称本次重新哈希或复制全部历史。
019 compile-02 的真实 start 属原019 turn，旧导出返回归属误标不传播；019 四个Tools未逐运行复制的历史限制如实保留，本轮入口和各运行已复制它们。
新源清单 14939 文件／862683489 字节／66 空目录；源、payload、ZIP路径/长度/SHA/空目录集合全部一致，归档后源稳定。
新源 canonical：35fd092f9384c7ad413f7965bf0ef78a18098ff70c35423020c8b99844d6e3bf。ZIP：TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/archive/stage024/FMDemoB16-stage024.zip，157919302 字节，SHA256=d8844ce81792943d97d5e48f7091e48b97fef60c48ed8fd7cdad9e431b95adf2。
- 自然测试新根：TestArtifacts/FMDemoB12/7327ac5b829c4f7996512af79917af8f。
- 自然测试新根：TestArtifacts/FMDemoB12/7e22702e99af4db98f402690b53d6252。
- 自然测试新根：TestArtifacts/FMDemoB13/68f001d9ac9f4136adbeae18a0fbd95c。
- 自然测试新根：TestArtifacts/FMDemoB13/fde3235df4004dddabad97ac269f1e83。
排除archive自引用、late与本阶段正式报告；late/stage024-bindings.json独立绑定。stage024-native.json仅导出本批准确turn的CommandExecution／FileChange及必要生命周期字段，不导出推理。

## NOT RUN／NOT VERIFIED

- 正式发布的关卡/数值/PRD C 来源和 PlayerSave 提交能力（025）；所有结果仍 CandidateValidation、CommitEligible=false。
- 六种操作 × 所有保存断点 × 全部 Retry/Resolve/End 的笛卡尔交叉组合；已实测入场/H06/恢复 × SaveFailed/CommitUnknown，以及 Initialize SaveFailed。
- 合法 AwaitLinks/全倒补线成功与由该链形成最终全倒胜利的运行见证；沿 019/Core 公开成功链缺口，未制造内部状态。
- 交互式 Editor、PlayMode、物理鼠标/触控、正式场景、完整 Demo、Player build、CI/性能验证；G2 尚等待精确旧断言范围裁决。
- 历史 019、018 强杀/六进程矩阵、G1 C2 原 83/23/42 数值见证未重复执行；§184 更早历史证据仍 NOT VERIFIED。
- 同 Scene 永久发布保留播放令牌的运行见证仍未补造；仅沿已接收 019 静态/运行边界。
- 本 C 不作独立 R 技术裁决；实际 task_complete 由 SD00/R 在 formal final 之后核验。

024 源码、测试和两报告在阶段绑定后停止修改。G2 只能沿已批准的阶段覆盖继续；当前已发现旧测试仍禁止 Core.Tests 引用 Unity，而 G2 要求其承载真实 UI Toolkit／EditorWindow 测试，精确断言例外已提问，尚未执行 G2。
scope：docs/system-design/2026-09-17/demo-024-scope.json。准确 task_complete／正式 final 由 SD00／R 随后主动核验；本报告不是独立 ACCEPT。
