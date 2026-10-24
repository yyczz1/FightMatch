# UGUI-COPY-BIND-01：uGUI 文案绑定与数据合同补充

2026-10-01 · 架构／系统设计补充 · 状态：`READY_FOR_INDEPENDENT_REREVIEW`  
本文只补充冻结的系统设计与实施包，不修改或重开它们。作者不自验收；COPY FIX-01 已独立 `ACCEPT`。首次工程 R 的 P1/P3 已在 FIX-01 关闭；同一 R-TOOLS 对 FIX-01 再审仍给出 `NEEDS_FIX`，本次 `QA-R-UGUI-COPY-BIND-01-FIX-02` 只闭合 P2 的 disable→unbind 顺序漂移。再次复审 `ACCEPT` 前不得实施。

## 1. 任务身份、权威与固定输入

- 任务 ID：`UGUI-COPY-BIND-01`。
- 最小纠正任务 ID：`QA-R-UGUI-COPY-BIND-01-FIX-01`。首次工程 R：thread `01a0f3e3-9663-7be2-84a8-ed23be32f0b9`、turn `01a0f3e8-e601-76b3-9080-dbe7675fa9bd`、final `msg_0543862903425801016abd6c44ee2087d0b0196d478dcb4571`，唯一 verdict `NEEDS_FIX`；本次只修其固定输入、FocusLost 链和可见反馈生命周期。
- 第二次最小纠正任务 ID：`QA-R-UGUI-COPY-BIND-01-FIX-02`。FIX-01 再审：同一 R-TOOLS thread `01a0f3e3-9663-7be2-84a8-ed23be32f0b9`、turn `01a0f3fa-9bbe-75c3-a445-f863446af59b`、final `msg_0543862903425801016abd70b3ac8087d08fe58ae537a83642`，唯一 verdict `NEEDS_FIX`。P1 snapshot/fixture 与 P3 retained slot 已关闭；FIX-02 只修 P2，冻结 disable cause 穿过真实 unbind 链且不被任一无参 `Cancelled` 入口抢先消费。
- 唯一交付负责人：FightMatch 架构设计会话，thread `01a0f2e6-1ac3-7d10-8855-54192b9489d4`，host `local`，turn `01a0f3d2-f9d9-7021-8b26-44549b870f7d`。
- 上级／返回：主程会话 thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b`，host `local`。授权来自用户已批准的五角色执行约定和该主程会话派发的本任务；不扩大 Git、Unity、产品、测试或发布权限。
- C 仍是唯一源码与 Unity 执行者。本文作者只读源码／证据并只修改本文，没有运行 Unity、Git 或修改产品文件。
- C audit：`/private/tmp/FightMatch-UGUI-01-p5hj_ch6/copy-gap-final.json`，SHA-256 `185c37beeb867aff99ab6c43623e5c54885aa3130facb835a47df015f012ce85`。
- C receipt：`/private/tmp/FightMatch-UGUI-01-p5hj_ch6/delivery-audit-final.json`，SHA-256 `f04684151ebdc1efc7e1a673e2fafd9c1fb919b5ce6f6e8bcf1394730c398627`。
- COPY 派发 provenance：`planning.md` SHA-256 `11d4c1891e0a21cabc1459695aac9473491e46e63de6008ce98c625cc5b30d9d`；`planning-localization-draft.csv` SHA-256 `0228ca7ceadf70af140af7b03de498630c0dc5e6ebd30704457af1aa457bf765`。这两个旧哈希只保留任务来源记录，不再是当前实施输入。
- COPY FIX-01 已接受输入：`planning.md` SHA-256 `8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`；当时 live `planning-localization-draft.csv` 的完整 SHA-256 `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`；旧 217 行 prefix SHA-256 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f` 只作历史 provenance。独立 R：thread `01a0e404-e8ee-7310-8388-9260babd53f1`、turn `01a0f3db-8a86-7e63-b77f-2f121cedbd0b`、final `msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`，结论 `ACCEPT`；F1～F3 已关闭，81/47/263 已接收，但该结论不证明 runtime、binding 或设备表现。
- UGUI 测试唯一固定文案输入改为不可变只读快照 `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv`，完整 SHA-256 `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`，`64708` bytes、`264` 个物理行、`263` 条数据记录。快照回执是 `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/receipt.md`，SHA-256 `980e8a942c06b1b6ebcec32d428135a656d5bd0ae089b087d82e3c4d5d3564b8`；不是 `.json`。
- 快照保存的是上述独立接收时 live CSV 的逐字节副本，但不是第二个人工编辑权威，也不得覆盖 live CSV。live 文案仍按其独立 LOC/COPY 流程演进；后续 AMEND-04 或更晚 live 变化不得改变、重算或使本 UGUI 测试夹具改读 live 文件。该快照也不是运行时 catalog 或已发布 Luban 产物。
- 冻结系统设计：`engineering-system-001.md`，SHA-256 `4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e`。
- 冻结实施包：`engineering-ugui-01-implementation.md`，SHA-256 `a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a`。
- 上述两份冻结文档不得因本文回写或改哈希。本文优先级仅覆盖本任务列出的 15 个核心缺口和 9 个产品处置；其它实现仍服从冻结文档。

当前输入门：实施与本文独立 R 必须精确使用上列 263-key 快照完整身份；不得从当前 live CSV、旧 217 行 prefix 或任意截断内容组装测试输入。本文作者没有修改、回滚或覆盖 planning/live CSV/快照。快照中的 263 个 key 唯一，本文引用的 56 个 key 全部存在。

## 2. 结论与模块边界

本补充把本地化视为 Presentation 的深模块：输入接口是 Application／Input 已确认的结构化事实，输出接口是固定 COPY key、参数集合和可见性；内部映射、格式化和去重是实现细节。View 不复制保存、手势、路由、恢复时钟或奖励业务规则，也不把 enum、ID、异常或任意 `Code` 字符串当玩家文案。

当前有两处公开接口不足；它们属于已授权 uGUI 迁移的普通技术细化，本文把最小形状、所有权和生命周期冻结如下，不留给 C 再设计：

1. `N-D01`：`PlayerNavigationOperation.Kind/Intent` 是 `FightMatch.Application` internal；冷恢复后的 Presentation 只见默认 `PlayerNavigationDraftKind`、空 Slots 和 OperationId。需要纳入本文独立 R 的最小只读 Application API。
2. `B-WIRE-01`：`GestureView` 只有 `Stage/HasInvalidSample`，无法区分 invalid start／not adjacent／crossed／wrong endpoint，也无法区分具体取消来源。需要纳入本文独立 R 的最小结构化 Input feedback API。

`PointerCancellationCause` 和可见反馈槽都只位于 Presentation implementation 内，不构成第三个 public delta。本文状态为 `READY_FOR_INDEPENDENT_REREVIEW`，不是等待用户选择的 `BLOCKED`。主程应保持已关闭的 P1/P3 不动，只把两处受保护 public delta、FIX-02 的完整 P2 internal 链和验证回送同一 R-TOOLS；只有该 R `ACCEPT` 且主程下发准确 C 包后方可执行。不得先实现其余映射后以空键、通用 reason 或默认枚举假装整体闭合。

程序集现状：

- `FightMatch.Core`：`ExactRational`、`CandidateApplicationKind`、`CandidatePermanentKind`，public；只 friend `FightMatch.Core.Tests`。
- `FightMatch.Input`：`RouteGesture/GestureView`，public；只 friend `FightMatch.Core.Tests`，现有测试 `CandidateBoardInputTests.B16B01_ExactFriendAndSeparateRuntimeAssemblies` 在第 28 行保护该事实。
- `FightMatch.Application`：导航／战斗 View DTO public，`PlayerNavigationOperation` internal；只 friend `FightMatch.Core.Tests`。
- `FightMatch.Presentation`：各 uGUI Binder public，文本 helper／binding seam internal；friends 精确为 `FightMatch.Host`、`FightMatch.Core.Tests`、`FightMatch.Host.Tests`。
- 测试程序集：`FightMatch.Core.Tests` 已引用 Core/Input/Application/Presentation/Host。本文不允许改 asmdef 或 friend 清单。

## 3. 数据与格式合同（5/5）

### N-D01 — 原请求身份与 operationName

- 真实触发／可见性：Navigation 或 Battle recovery 确有 `PendingOperationId`，且 retained/adopted original intent 的 `OperationId` 与之逐字相等时，才显示 `fm.save_recovery.operation_summary(operationName)`。自然 L1 仅在保存故障／冷恢复条件下可达。
- 唯一事实源：`PlayerNavigationOperation.Intent.Kind`（`PlayerNavigationModels.cs:26-55`）；PermanentRequest 还必须取同一 intent 的 `GetPermanent()?.Kind`，清空装备还必须由同一 quote 的 `Kind == Equip && DefinitionId == null` 证明。Battle 已有两条合法来源：`PlayerBattleView.cs:181-185` 的 `view.OriginalIntent` 或 `Controller.Input.LastRequest`，两者都先核 OperationId。
- 精确映射：InitializeProfile→`fm.operation.initialize_profile`；EnterAttempt→`fm.operation.enter_attempt`；EnterFormation→`fm.operation.enter_formation`；Attack→`fm.operation.attack`；Link→`fm.operation.link`；Rollback→`fm.operation.rollback`；SettleVictory→`fm.operation.settle_victory`；ExitAttempt→`fm.operation.exit_attempt`；RestartAttempt→`fm.operation.restart_attempt`；AdvanceRecovery→`fm.operation.advance_recovery`；SetFormation→`fm.operation.set_formation`；MigrateRoster/MigratePermanent→`fm.profile.data_upgrade.title`；PermanentRequest 仅 ClearEquipment→`fm.operation.clear_equipment`、SetPreference→`fm.operation.set_item_preference`。参数只有外层 `operationName`。
- 未发布：PermanentRequest 的非空 Equip、Craft、UseExperienceCards、LearnSkill、ConfirmTeachingExplanation、BeginTeachingGift 均隐藏，不回退 generic permanent；未知 enum 触发 binding diagnostic。
- 禁止推断：不得由 `PlayerNavigationConfirmation.Kind` 默认 Formation、Slots、route、status、OperationId 形状、最后点击按钮或 record 顺序反推；不得显示 OperationId／CommitId／enum 名。
- 实现位置：现有缺口在 `PlayerNavigationRecoveryView.PendingOperationKey`（`PlayerNavigationRecoveryView.cs:152-162`）和 `NavigationBindings.OperationKey`（`PlayerNavigationView.cs:380-393`）。冻结的最小只读 seam 是在 `PlayerNavigationConfirmation` 精确增加 `CandidateApplicationKind? OriginalKind`、`CandidatePermanentKind? OriginalPermanentKind`、`bool OriginalClearsEquipment` 三个 getter-only fact；只由现有 internal constructor 接收，只由 `PlayerNavigationSession.View`（`PlayerNavigationSession.cs:65-74`）从同一个 `operation.Intent` 构造。`OriginalPermanentKind = intent?.GetPermanent()?.Kind`；`OriginalClearsEquipment` 只有 `intent.Kind == PermanentRequest && quote.Kind == Equip && quote.DefinitionId == null` 才为 true。Presentation 必须先核 `Confirmation.OperationId == Read.Application.PendingOperationId`，再读这些 facts。
- 最小性说明：现有 `CandidatePermanentQuote.Kind/DefinitionId` 对暖路径足以推导清空装备，但冷 `Adopt` 路径的 `preview` 为 null，因此现有 `Confirmation.Quote` 也为 null。不得把 retained intent 的完整 quote 填入 `Confirmation.Quote` 来扩大数据暴露；`OriginalClearsEquipment` 在冷路径并不冗余，它把唯一需要的业务谓词冻结成一个 scalar。三 fact 不暴露 intent、DefinitionId、角色／物品身份或 canonical bytes，也不把整个 quote 扩散到 Presentation。
- 测试：新增 1 个非参数化用例 `PlayerNavigationRecoveryTests.UGUI_COPY_D01_ColdAdoptUsesExactOriginalKindAndNeverDraftDefault`，在方法内覆盖 SetFormation、Link、两类 migration、ClearEquipment、SetPreference、未知／未发布隐藏和 OperationId 不匹配；Battle 现有 B06 recovery 用例增加同身份断言。
- 影响：这是唯一的 public Application API delta 组；类型为非序列化只读 view DTO，构造归 `PlayerNavigationSession.View`，消费归 Navigation Presentation。它不进入 save codec、record、intent canonical bytes、Unity serialized field 或 persisted bytes，不改路由、保存与恢复选择。形状已冻结，仍须随本文独立 R，不得由 C 改名、合并成 full quote 或另造第四 fact。

### N-D02 — duration／remainingTime

- 真实触发／可见性：`Character.IsReady == false` 或 `ActiveRecovery != null` 才显示 `fm.party.member.recovering(remainingTime)`；当前已批准 L1 正常两次击倒链不自然产生非 ready 导航成员，属于非 L1／历史恢复条件。结果历史用 `fm.result.recovery_started(duration)`，其值是原 end 打开的完整 `period.Duration`，不是当前剩余量。
- 唯一事实源：`CandidateRecoveryPeriod.Duration/Elapsed`（`CandidateRecoveryClock.cs:44-60`）。当前剩余量精确为同一已接受快照的 `Duration.Subtract(Elapsed, ExactMathBudget)`；不得采样 `Time.realtimeSinceStartup`、系统时钟或 Presentation timer。
- 不 clamp：Core 在 advance 时把 `elapsed >= duration` clamp 为 duration（`CandidateRecoveryClock.cs:92-94`）；restore invariant 要求 `0 <= Elapsed <= Duration`（`CandidateBusinessRestoreChecks.cs:139-145`），定义还要求 Duration 严格为正（`CandidateCharacterGrowth.cs:37-39`；测试 `CandidateCharacterGrowthTests.cs:168`）。Presentation 不进行第二次 clamp。违反该 invariant 是 binding/data failure。
- 格式：接受已通过上述 invariant 与 `ExactMathBudget` 的非负 `ExactRational` 毫秒；精确计算 `ceil(milliseconds / 1000)`，输出 invariant culture 的无固定宽度 minutes、冒号、两位 seconds，即 `0:00`、`0:01`、`1:00`、`123:04`。中英文参数字节相同，单位由模板表达。零合法且为 `0:00`；负值、null、分母/整数超预算或计算异常不显示数字，进入 binding diagnostic；BigInteger 不截断为 Int32/Int64，不溢出回绕。
- 现有实现／调整：`BattleText.Duration`（`CandidateBoardInputView.cs:216-221`）已做 ceil 和 invariant `m:ss`，需补输入／budget guard；Navigation `Characters`（`PlayerNavigationView.cs:174-188`）改用同一 helper，不复制格式；`PlayerBattleView.RenderHud`（`:269-271`）继续用 snapshot remaining；receipt 的 `:223-228` 改用 `fm.result.recovery_started(duration)` 与完整 Duration。
- 禁止：不得传 raw rational、浮点、当前帧累计、负值取绝对值、最小强制 1 秒或历史页 remainingTime。
- 测试：新增 `PlayerBattlePresentationTests.UGUI_COPY_D02_DurationUsesAcceptedSnapshotAndInvariantCeiling`，单方法覆盖 0、1/3ms、999/1000/60000ms、超过 99 分钟、Duration-Elapsed=0、负值和超预算；同时在 zh/en 断言参数完全相同，并断言 Render 不推进业务时钟。
- 影响：仅 internal Presentation helper／绑定；无 public API、业务、serialization 影响。

### N-D03 — source editor

- 真实触发／可见性：只属于未来非 L1 Craft 的显式来源编辑；当前发布没有玩家侧稳定来源编号或范围基数，处置为 `DEFER_OUT_OF_CURRENT_SCOPE`。
- 当前事实：`PlayerPermanentDetailView.cs:88-104` 会为 `SourceChoices` 克隆两输入字段；`SourceKey`（`:155-168`）拼接 grant、binding、commit、operation、branch、endpoint 等内部身份；quote 在 `:142-153` 也用它作内部行 identity。
- 本轮行为：隐藏并禁用 `explicitSources`、`explicitSourcesLabel`、`sourceRows`，不克隆来源编辑行；保留内部 SourceKey 仅作不可见节点 identity，不进入 LocalizedText 参数。未来重新启用必须先由玩家侧稳定事实提供 `sourceNumber,itemName,rangeStart,amount` 及 1/0 基数决定。
- 允许 key／参数：当前没有玩家行、没有 key、没有参数。未来合同另审，不预占 key。
- 禁止：不得显示 SourceKey、commit、operation、endpoint、branch、hash、版本、UnitStart 原值或 synthetic 名；不得把 `fm.save_recovery.candidate_row` 当来源行。
- 测试：新增 `PlayerNavigationPermanentFlowTests.UGUI_COPY_D03_UnpublishedSourceEditorIsHiddenAndLeaksNoIdentity`，用现有多来源 fixture 证明控件不可见／不可交互、无内部标识进入 TMP、业务头和文件不变。
- 影响：仅 hide/defer 的 Presentation binding；不删业务能力，不改 public API／serialization。

### N-D04 — reason／diagnostic 分类

- 真实触发：Navigation `Status/Diagnostic`（`PlayerNavigationView.cs:92-104`）、recovery `Diagnostic`（`PlayerNavigationRecoveryView.cs:51-55`）、Battle input unavailable（`CandidateBoardInputView.cs:131-143`）、settlement unavailable（`PlayerBattleView.cs:120-129`）或 playback scheduler failure。自然 L1 有交互拒绝；保存、恢复、通知、调度为 fault-conditional。
- 事实源：实际 action/domain、`CandidateApplicationDiagnostic.Code + FieldPath`、Application phase、`OriginalLookup/NotificationFailure`；`CandidateApplicationDiagnostic` 的开放字段见 `CandidateApplicationModels.cs:14-58`。`FieldPath/Stage/ExceptionType/ExceptionMessage/LimitReason/revision` 只进日志。
- 安全映射：
  - confirmed SaveFailed→`fm.save_recovery.failed(errorCode)`；CommitUnknown→`fm.save_recovery.unknown()`；CreationPending→`fm.profile.creation_pending.body()`；SettlementRequired→`fm.victory.pending.title()`；ResolutionRequired→`fm.save_recovery.blocking_notice()`。
  - `UnsupportedBinding + Navigation.Level`／实际 entry binding→`fm.entry.binding_unsupported()`；同码在 Permanent、reference 或其它域不得借用。
  - `ActiveAttemptConflict` 必须由实际 entry／party／equipment command source 分别选 `fm.entry.active_battle.blocked`、`fm.party.active_battle.locked`、`fm.inventory.active_battle.locked`。
  - selection/owner/retry/review 的显式已批准映射：OperationSelectionRequired→`fm.save_recovery.select_operation`；CandidateSelectionRequired→`fm.save_recovery.select_candidate`；OriginalOwnerRequired→`fm.save_recovery.return_to_original_flow`；RetryRequired→`fm.save_recovery.retry_before_resolve`；PreviewRequired/EndConfirmationRequired→`fm.common.state.review_required`；ResultRequired→`fm.save_recovery.result_required`；StaleContext/StaleNavigationContext/StaleNavigationToken/StaleConfirmation/StaleHostRequest→`fm.common.state.changed_review_again`。这些键无参数。
  - 已确认保存但 NotificationFailure→`fm.save_result.notification_failed(errorCode)`，不得说保存失败。
  - playback scheduler failure→`fm.battle.playback.interrupted(errorCode)`；errorCode 是代码内显式 allowlist 映射出的稳定玩家诊断码，不是异常 type/message。
  - 其它真正可见且已分类的业务拒绝可用 `fm.common.business_attention(errorCode)`；`errorCode` 仍必须来自显式 allowlist。未知 code／未知 domain 返回空 key，触发诊断 placeholder／blocking gate。
- 禁止：不得 `ReasonKey(reason)` 默认透传、`ToString()`、显示 exception/message/stack/field/revision、把 notification failure 当 SaveFailed，或把 `fm.diagnostic.missing_binding` 当业务错误。
- 实现位置：把 `NavigationBindings.ReasonKey`（`PlayerNavigationView.cs:405-417`）改为“domain + explicit code”内部 adapter；`BattleText.Save`（`CandidateBoardInputView.cs:267-275`）也只显式映射；日志仍保存内部诊断。
- 测试：新增 `PlayerNavigationPresentationTests.UGUI_COPY_D04_ReasonMappingRequiresDomainAndStableAllowlistedCode`，同码跨 domain、通知失败、未知码、exception 泄漏均覆盖；Battle fixture 断言 scheduler 只呈现稳定 code。
- 影响：internal mapping only；无 public/API/serialization 影响。

### C-D01 — Host/session route 与可见性

- 事实源／现状：Host startup 由 `FightMatchHostSession.ObserveStartup`（`FightMatchHostSession.cs:150-175`）所有；opened page 由 `RefreshOpened`（`:238-246`）决定；Battle Query 在 `PlayerBattleSession.cs:91-110` 选择 HostNavigation/Recovery/Confirmation/Result/Battle/Gate；成功 handoff 在 `AcceptHost`（`:122-156`）消费；Navigation `RequestHost` 把 `kind.ToString()` 写 Status（`PlayerNavigationSession.cs:254-270`）。
- 可见性表：Unconfigured/InitializationReady/CreationConfirmationRequired 只在 Host Startup／创建表面表达；Ready 不显示 lifecycle 行；PendingPreparation/SaveFailed/CommitUnknown 进入现有 recovery；RestoreRequired/RecoveryBlocked 只在 Recovery 表面以 N-D04 的受控业务诊断表达；Disposed 只触发 teardown/不可交互，不显示玩家状态。成功 CreationRequired/BattleSelectionRequested/ResumeBattleRequested/SettlementRequired/RootBackRequested handoff 的 TargetKind 状态全部隐藏，目标页由 Host 接管。
- 头与技术状态：published head 的 commit/hash/generation 全部隐藏；`RecipeAvailability=Defined` 隐藏；同一原因的 outer status/inner diagnostic 只显示一次。成功 handoff 的 internal status 既不是成功 toast 也不是失败。
- 允许输出：只使用上述真实可见状态已有 key；generic fault 最多 `fm.common.business_attention(errorCode)`。无新 route/lifecycle key。
- 禁止：不得改 Host routing、消费 revision、session lifetime、save、恢复选择；不得以 phase 名、TargetKind、Defined、head identity 或空 key 生成文字。
- 实现位置：Presentation 过滤发生在 `PlayerNavigationView.Render`（`:71-105`）、`PlayerNavigationRecoveryView.Render`（`:57-94`）、`CandidateBoardInputView.Render`（`:105-145`）、`PlayerBattleView.Render`（`:97-137`）；Host/Application 文件为只读事实源，不在无 API delta 的实施白名单。
- 测试：新增 `PlayerNavigationPresentationTests.UGUI_COPY_CD01_LifecycleRoutingShowsOnlyOwningSurfaceAndHidesInternalFacts`，覆盖各 phase、TargetKind、head、Defined、重复外层行；复用现有 host/session route tests 证明路由与保存零变化。
- 影响：默认仅 Presentation visibility；无 public/API/serialization 影响。

## 4. 既有键绑定合同（10/10）

下表的“测试”均为新增的单个非参数化测试方法；方法内部可循环覆盖多个状态，不因参数化扩展测试计数。

| ID | 真实触发／L1 | 事实源与允许输出 | 禁止与实现位置 | 测试；影响 |
| --- | --- | --- | --- | --- |
| N-B01 | fault-conditional；`IsEndConfirmation=true` | `Confirmation.IsEndConfirmation`；内层保留 `fm.save_recovery.title()`，外层重复 title 隐藏 | 不借其它 state key；`PlayerNavigationView.Render` `:81-91`、`PlayerNavigationRecoveryView.Render` `:70-73` | `PlayerNavigationPresentationTests.UGUI_COPY_B01_EndConfirmationHasOneSaveAttentionTitle`；internal binding，无 API/serialization |
| N-B02 | 非 L1；偏好保存已确认 | 必须同时满足 `Result.IsCommitted`、`OriginalLookup.IsFound`、record intent `PermanentRequest`、`GetPermanent().Kind=SetPreference`；输出 `fm.inventory.preference.saved()` | 不能泛化到其它 PermanentRequest；`PlayerNavigationView.Render` `:95-104`、`PlayerNavigationRecoveryView.Render` `:74-81` | `PlayerNavigationPresentationTests.UGUI_COPY_B02_PreferenceSavedRequiresExactCommittedSubtype`；internal binding |
| N-B03 | invalid-binding conditional | `Diagnostic.Code=UnsupportedBinding` 且 FieldPath/domain 是 Navigation.Level／真实 entry；`fm.entry.binding_unsupported()` | Permanent/reference 同码不可借用；`NavigationBindings` `PlayerNavigationView.cs:405-417` | `PlayerNavigationPresentationTests.UGUI_COPY_B03_UnsupportedBindingIsEntryOnlyInEntryDomain`；internal binding |
| N-B04 | interaction-conditional，自然 L1 | command owner 是 entry／party／equipment；分别用三个 active-battle key，均无参数 | 不能靠同一个 status 猜来源，migration 不借用；`PlayerNavigationView.Render/Preparation/Equipment` `:92-100,:166-170,:274-295` | `PlayerNavigationPresentationTests.UGUI_COPY_B04_ActiveBattleReasonFollowsActualCommandDomain`；internal binding |
| N-B05 | fault-conditional | Application phase／diagnostic 确认 SaveFailed，即使 retained Result 为空；`fm.save_recovery.failed(errorCode)`，errorCode 经 N-D04 allowlist | Pending/unknown 不得叫 failed；`PlayerNavigationView.Render` `:92-100` | `PlayerNavigationPresentationTests.UGUI_COPY_B05_ReadSideSaveFailedReusesFailureKeyWithStableCode`；internal binding |
| N-B06 | fault-conditional | 已知 SaveFailed/CommitUnknown/CreationPending/SettlementRequired/ResolutionRequired 复用各专键；同页重复 Diagnostic 隐藏；NotificationFailure 独立 `fm.save_result.notification_failed(errorCode)` | 不创建空诊断行，不把 notification 混成 failure；`PlayerNavigationView.cs:99-104`、`PlayerNavigationRecoveryView.cs:51-55,92-93` | `PlayerNavigationRecoveryTests.UGUI_COPY_B06_KnownDiagnosticIsDeduplicatedAndNotificationStaysDistinct`；internal binding |
| B-WIRE-01 | 自然 L1：invalid route、cancel、focus loss、second pointer、accepted | 只由 §4.1 的结构化 outcome 输出八个既有无参完整 key：`fm.battle.gesture.invalid_start`、`fm.battle.gesture.not_adjacent`、`fm.battle.gesture.crossed`、`fm.battle.gesture.wrong_endpoint`、`fm.battle.gesture.cancelled`、`fm.battle.gesture.focus_lost`、`fm.battle.gesture.second_pointer_ignored`、`fm.battle.action.accepted` | `HasInvalidSample` 不能分类；View 不复制路线算法。位置 `RouteGesture.cs:43-203`、`GestureModels.cs:235-267`、`CandidateBoardElement.cs:62-141`、`CandidateBoardInputController.cs:130-175,241-263`、`CandidateBoardInputView.cs:131-144` | `CandidateBoardInputTests.UGUI_COPY_WIRE01_ExactInputOutcomeBindsOneExistingFeedbackWithoutBusinessWrite`；唯一 public Input feedback delta 见 §7.2，随本文送独立 R |
| B-WIRE-02 | 自然 L1：首个真实 drag 关闭参考、步骤完整播放后 replay | close 原因来自 `PlayerDefaultReferenceView.OnInput` `:206-213`；关闭后把 `fm.reference.closed_for_input()` 写入 §4.1 同一个保留可见反馈槽；只有所选 step 完成后其控件才改为 `fm.reference.replay_button()` | 关闭仍继续同一指针，不能 submit/ack token；未完成不可叫 replay。实现 `PlayerDefaultReferenceView.cs:123-133,159-215` 与 `CandidateBoardInputView` 保留反馈 binding | `PlayerDefaultReferenceTests.UGUI_COPY_WIRE02_FirstDragNoticeSurvivesAndReplayAppearsOnlyAfterCompletion`；Presentation internal seam，无 public/serialization |
| B-WIRE-03 | 自然 L1：history、save blocked、final playback、end review | 有 history 即显示 `fm.history.title()`；Rollback 因 save pending 不可用时显示 `fm.history.undo.blocked_by_save()`；胜利末段 skip 用 `fm.victory.playback.skip_button()`，其它 playback 保留 generic skip；end confirmation/body 用 `fm.save_recovery.end_uncommitted_confirm()` | Query 是同步方法，不显示 `fm.save_recovery.checking`；不创造空历史提示。位置 `PlayerBattleView.cs:113-145,176-210`、`CandidateBattlePlaybackView.cs:48-58,93-139`、`PlayerNavigationRecoveryView.cs:94-101` | `PlayerBattlePresentationTests.UGUI_COPY_WIRE03_ExistingHistorySaveSkipAndEndKeysFollowRealStateWithoutChecking`；internal binding |
| B-WIRE-04 | 自然 L1：选择 reference step 后、第一 fact 前 | reference 初始 frame 明确绑定 `fm.reference.step.select_prompt()`；scope 说明继续用 `fm.reference.scope_notice()` | 不能走共享 `BattleText.Beat` 的 `fm.battle.playback.action_submitted`。位置 `PlayerDefaultReferenceView.BindBeat` `:198-205`、`BattleText.Beat` `CandidateBoardInputView.cs:285-306` | `PlayerDefaultReferenceTests.UGUI_COPY_WIRE04_ReferenceInitialFrameNeverClaimsActionSubmitted`；internal binding |

### 4.1 B-WIRE-01 精确产生、消费与清除合同

- 唯一分类：`GestureFeedbackKind` 精确为 `InvalidStart`、`NotAdjacent`、`Crossed`、`WrongEndpoint`、`Cancelled`、`FocusLost`、`SecondPointerIgnored`、`ActionAccepted` 八值；逐项映射上表同顺序完整 key，无 generic／unknown fallback。`RouteGesture` 在原判定分支产生前四值：`Down` 位于 board 内普通非端点／非历史线格而无法 capture，或从 captured 但不可绘制的起点越过阈值，均为 `InvalidStart`；非相邻追加为 `NotAdjacent`；self-intersection 或 locked-cell overlap 为 `Crossed`；other endpoint、越过本对终点或 Up 未落在目标 endpoint 为 `WrongEndpoint`。board 外 Down 不产生反馈。若 Application 返回结构化 `DomainRejection.RouteReasonCode`，只允许 `NonAdjacent→NotAdjacent`、`SelfIntersection|LockedOverlap→Crossed`、`EndpointMismatch|ForeignEndpoint→WrongEndpoint`；其它 route reason 不猜。
- 唯一 public Input seam：`RouteGesture.TakeFeedback()` 原子返回并清除一个 nullable `GestureFeedbackKind`；`Down/Move/Up` 后 Controller 立即 drain 到下述 Presentation 可见槽。一次非法手势只保留首个终止分类；退格不能抹掉已产生的 Input feedback，也不能二次产生。`RouteGesture.Bind`／下一次被接受的 `Down` 清理 RouteGesture 内尚未 drain 的旧值，`RouteGesture.Cancel` 本身不猜 cause。这里的 Input 一次性 signal 与玩家可见槽是两个不同生命周期，View 永远不直接消费 `RouteGesture.TakeFeedback()`。
- 取消 cause 的精确内部形状：在 `FightMatch.Presentation` 命名空间增加且只增加 `internal enum PointerCancellationCause { Cancelled, FocusLost }`。所有现有 public 或 parameterless `Unbind/Close/Detach/PausePresentation/CancelPointer/CancelGesture` 保持原名称和可见性，且固定委托对应 internal cause overload 的 `PointerCancellationCause.Cancelled`；显式 close、detach、rebind、destroy/dispose 因而不会被误标为失焦。`CandidateBoardInputController` 只增加 `internal void CancelGesture(PointerCancellationCause cause)`；它只在取消前 `Gesture.ActivePointerId` 确有值时把 cause 映射为同名 `GestureFeedbackKind` 写入可见槽。无 active gesture 时不发布、不递增 feedback revision，后续重复层只完成自身 teardown。
- internal overload 精确清单与链：只在 §7 的现有 15-source 白名单内增加 `FightMatchHostView.Unbind(PointerCancellationCause cause)`、`FightMatchHostView.PausePresentation(PointerCancellationCause cause)`、`PlayerBattleView.Unbind(PointerCancellationCause cause)`、`CandidateBattlePlaybackView.Unbind(PointerCancellationCause cause)`、`CandidateBattlePlaybackView.Close(PointerCancellationCause cause)`、`CandidateBoardInputView.Unbind(PointerCancellationCause cause)`、`CandidateBoardInputView.Close(PointerCancellationCause cause)`、`CandidateBoardElement.Unbind(PointerCancellationCause cause)`、`CandidateBoardElement.CancelPointer(PointerCancellationCause cause)` 和上述 Controller overload；全部为 internal，不新增 public interface、第三个 public delta、asmdef/friend 或 serialized field。完整 teardown 传递固定为 `FightMatchHostView.Unbind(cause) → PlayerBattleView.Unbind(cause) → CandidateBattlePlaybackView.Unbind(cause) → CandidateBoardInputView.Unbind(cause) → CandidateBoardElement.Unbind(cause) → CancelPointer(cause) → CandidateBoardInputController.CancelGesture(cause)`。HostView 保持现有先直接 `board.CancelPointer(cause)`、再下传 `battleView.Unbind(cause)` 的顺序；下游收到同一 cause，但因 no-active 不重复发布。Playback 的非 borrowed `Close(cause)` 同样把 cause 传入 `CandidateBoardInputView.Close(cause)`；任何 overload 都不得在中途回落到无参入口。
- 普通取消调用点保持 `Cancelled`：`CandidateBoardElement.CancelTrueTouch`、`InvalidateGeometry`、`Geometry` 失效、`Sample` 失败、`OnPointerExit`、现有显式 pointer cancel；各层显式 `Bind/Unbind/Detach/Close/Dispose`；`FightMatchHostView.RefreshDiagnostic/ShowFailure`；`FightMatchPlayerHost.LowMemory`。它们继续走 parameterless 入口。`CandidateBattlePlaybackView.OnLowMemory` 仍只 `SkipToFinal`，不得新增 FocusLost。`OnPointerDown` 发现已捕获不同 pointer 时只 `PublishFeedback(SecondPointerIgnored)` 后返回，原 pointer、draft、capture 不变；同 pointer 重入只忽略。
- Unity disable 的唯一 FocusLost 路径：`FightMatchPlayerHost.OnDisable` 必须依次调用 `runtimeRoot?.PausePresentation(FocusLost)`、`runtimeRoot?.Unbind(FocusLost)`；第一步仍按既有顺序先 `board.CancelPointer(FocusLost)` 再 `session?.PausePresentation()`，第二步用同一 cause 完成 teardown，且不得先调用任何无参 overload。`FightMatchHostView.OnDisable` 直接 `Unbind(FocusLost)`；`PlayerBattleView.OnDisable` 直接 `Unbind(FocusLost)`。`CandidateBattlePlaybackView.OnDisable` 在 non-borrowed 分支调用 `Close(FocusLost)`，在 borrowed 分支保持 `StopSchedule → board.CancelPointer(FocusLost) → ClearPlaybackOverride`；`CandidateBoardInputView.OnDisable` 保持先停止 ticking、再 `board.CancelPointer(FocusLost)`；`CandidateBoardElement.OnDisable` 调 `CancelPointer(FocusLost)` 后才 `base.OnDisable()`。`FightMatchPlayerHost.HandleApplicationFocus(false)`／`OnApplicationPause(true)`、board 与 playback 的 focus false／pause true 仍使用 FocusLost；focus true／pause false 不动作。
- teardown 终点与顺序不变：各类 `OnDestroy`、public `Dispose` 和显式 `Unbind` 继续走 parameterless `Cancelled`；若 active object 已先收到 `OnDisable(FocusLost)`，它们因 no-active 不发布 `Cancelled`，若是显式调用则仍产生唯一 `Cancelled`。`PlayerBattleView.ClosePresentation` 和各 `Detach` 也保持 `Cancelled`。cause 只改变 active gesture 的 feedback 分类；不得改变 Host session pause、playback `SkipToFinal/StopSchedule/ClearPlaybackOverride`、presentation token report、Controller detach、零 submit 或业务结果。无论父级先、子级先或某个中层对象被直接禁用，首个看到 active gesture 的 disable 路径发布唯一 `FocusLost`，其余层只 teardown，不得发布 `Cancelled` 或 duplicate。
- 唯一保留可见反馈槽：在 Presentation 增加 `internal enum CandidateBoardVisibleFeedbackKind`，精确九值为八个 `GestureFeedbackKind` 同名值加 `ReferenceClosedForInput`。`CandidateBoardInputController` 精确持有 `internal CandidateBoardVisibleFeedbackKind? VisibleFeedback { get; private set; }` 与从 0 起且 controller lifetime 内不复位的 `internal long VisibleFeedbackRevision { get; private set; }`；只暴露 `internal void PublishFeedback(GestureFeedbackKind kind)`、`internal void PublishReferenceClosedFeedback()`、`internal void ClearVisibleFeedback()`。两个 publish 都替换单槽、把 revision 加一并同步触发一次 `Changed`；clear 只在槽非空时清空并把 revision 加一，且不自行触发第二次 `Changed`。没有队列、历史列表、event bus、Unity serialized field、save 或持久化。
- 可见绑定生命周期：`CandidateBoardInputView.Render` 只读取 `VisibleFeedback/VisibleFeedbackRevision`，把 semantic kind 显式映射到固定完整 key，并以 `board-visible-feedback:<revision>` 为行 identity 在现有状态／instruction 行之后最多追加一行到 `availabilityRows`；Render 不清槽、不再次 publish。连续 `Changed`、100ms `Controller.Refresh()`、playback 收口触发的 Render 以及 `LocaleChanged` 都重绑同一 semantic slot；locale 切换只换语言，不换 kind 或 revision。新反馈替换旧槽。下一次通过 Controller admission 的 owner `Down` 在 `gesture.Down(sample)` 前清旧槽；被拒绝的第二 pointer 不先清而以 `SecondPointerIgnored` 替换。`CandidateBoardInputView.Unbind/Close` 及 Bind/Attach 的 owner 替换在取消和退订完成后清槽；`CandidateBattlePlaybackView.Dispose`、`PlayerBattleView.Dispose`、`FightMatchHostView.Dispose` 等现有上层 Dispose 路径必须经既有 Close/Unbind 级联到同一清除，不给 `CandidateBoardInputView` 新增 public `Dispose`。不得让旧 controller 的 feedback 泄漏到新 owner。
- reference 与 action 共槽：八个 gesture／`ActionAccepted` 都经 `PublishFeedback(GestureFeedbackKind)`；首个真实 drag 的 `PlayerDefaultReferenceView.OnInput` 必须先保存 `owner.Input`、调用 `Close()` 解除自身 `Changed` 订阅，再调用该 controller 的 `PublishReferenceClosedFeedback()`，避免重入并让 `ReferenceClosedForInput→fm.reference.closed_for_input` 留在同一槽。手动 close、stale/resolution close 不发布。reference notice、gesture outcome 和 `ActionAccepted` 相互替换，不得各建一条 persistent row。
- `ActionAccepted` 只在同一手势对应请求取得已提交且已验证的业务结果后产生一次。route-valid intent 本身不产生；`Prepare.IsAccepted` 只用来 arm 当前 `Request.OperationId + Request.Intent.CanonicalBytes`，也不产生。`Receive(CandidateBattleCallResult result)` 必须同时满足：`DomainRejection == null`；`Application.IsCommitted`；`OriginalLookup.IsFound`；`Application.OriginalCommitId == OriginalLookup.OriginalCommitId`；lookup record 的 `OperationId` 与 armed operation 相等且其 intent canonical bytes 与 armed bytes `SequenceEqual`；`Application.View.Phase == Ready` 且 `IsPublishedHeadVerified`；`LookupViewCommitId == View.PublishedSnapshot.Header.CommitId`。`PresentationDisposition` 是否 `PlayOriginal` 不替代这些事实，已提交后的 `NotificationFailure` 也不撤销 accepted。
- accepted 生命周期：`SaveFailed`、`CommitUnknown`、未验证 view 或不同 operation 不显示，也不把 route-valid/Prepare 误报为成功；同 operation 的后续 `Retry/Resolve/Query` 首次满足全部条件时才显示。显示后记录已报告 operation 并清除 arm，重复 Query／Rebuild 不再显示；明确 domain rejection、另一个新手势成功 Prepare、Unbind/Dispose 清除或替换 arm。这个 transient 状态只在 Controller 内存中存在，不序列化、不影响请求、提交、重试或 presentation token。
- 去重与测试固定：扩展既有 `CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit` 与 `CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources`，在这两个原方法内部用真实对象、既有 internal seam 覆盖父级先、子级先、直接禁用 `FightMatchHostView`、直接禁用 `PlayerBattleView` 四类顺序；每类都观察 teardown 清槽前的 publish，断言唯一 `FocusLost`、零 `Cancelled`、零 duplicate、零 submit，且 focus true 无动作。相同方法还必须证明普通显式 `Close/Unbind` 与 low-memory 在 active gesture 时只产生唯一 `Cancelled`。不得增加测试 hook、reflection、测试方法或计数。WIRE01/WIRE02 的 P3 assertions 保持 FIX-01 原样；目标仍为 §8 的 222。

## 5. 产品处置（不重做 COPY）

这些处置采用 COPY FIX-01 已独立 `ACCEPT` 的 263-key 候选中的既定 key/参数；COPY verdict 身份见 §1，工程不改写文本。

| ID | 最终处置、触发与可见性 | 实现／测试 | 禁止与影响 |
| --- | --- | --- | --- |
| N-P01 | 同一结果／原因只显示一个状态区；Recovery 内层已有标题时隐藏外层；verified committed success 用 `fm.save_result.title()` + `fm.save_result.confirmed(operationName)` 一次，NotificationFailure 单列 | `PlayerNavigationView.cs:81-104`、`PlayerNavigationRecoveryView.cs:70-93`；由 N-B01/N-B02/N-B06 测试覆盖 | 不用额外空 Diagnostic 制造缺键；无 API/serialization |
| N-P02 | `save.Head` 永久隐藏；玩家不依赖 current published head 文本 | 删除/不创建 `PlayerNavigationRecoveryView.cs:83-84` 的空 key 行；C-D01 测试 | 不显示 commit/hash/generation，不把 candidate 当 confirmed；无 API |
| N-P03 | Recovery 普通页的入口绑定 `fm.save_recovery.review_end_button()`；只有进入 EndConfirmation 且 `ActionReason(End)==null` 后才显示／执行既有 `fm.save_recovery.end_uncommitted_button()`，body 为既有 confirm key | `PlayerNavigationRecoveryView.cs:97-101,133-139`；B-WIRE03 测试 | 不提前声称 safe，不改 admission；无 API/serialization |
| N-P04 | 保留自然可达 V3→V4 相邻迁移入口；按钮／预览用 `fm.profile.data_upgrade.title/body`，参数为空 | `PlayerNavigationView.cs:117-123`、`PlayerNavigationRecoveryView.cs:104-113`；扩展 `PlayerNavigationSessionTests.CC23_ExplicitAdjacentMigrationPreservesCreationAnchorAndRejectsSkipping` | 不显示 from/to 技术号，不自动迁移；无业务/API/serialization 改动 |
| N-P06 | `RecipeAvailability=Defined` 隐藏；当前不存在的 source/candidate empty row 不新增。真实 NoPublishedDefinition 仍用 `fm.crafting.no_recipes` | `PlayerNavigationView.cs:297-307`、recovery candidates `PlayerNavigationRecoveryView.cs:125-132`；C-D01 测试 | 不借 inventory.empty／continue_candidate；无 API |
| N-P07 | 成功 Host handoff 的 TargetKind/Status 全隐藏，Host 目标页即反馈 | `PlayerNavigationView.cs:92-100`；C-D01 测试并复用 Host routing tests | 不翻译 enum、不当失败；无 routing/API/serialization 改动 |
| N-P08 | 保留测试保护的无装备 clear 页面／控件；ClearEquipment 使用 N-D01 精确 operation key，相关 review keys 按 263-key 表；未来 exact source editor 按 N-D03 延后并隐藏 | `PlayerNavigationView.cs:274-295`、`PlayerPermanentDetailView.cs:64-140`；扩展 `PlayerNavigationPermanentFlowTests.CC07_CC10_CC27_RealEmptyEquipmentFlowHasBoundedParentsAndBackConsumesOneLayer` 和 N-D03 测试 | 不删控件求绿，不启用 exact source；ClearEquipment 只消费 §7.2 已冻结 N-D01 facts，须随本文独立 R |
| B-DECISION-01 | 初始空 history 不增加提示；真实 history 区只显示 `fm.history.title()` | `PlayerBattleView.cs:138-145`；B-WIRE03 测试 | `fm.history.title` 不能伪装成 empty message；无 API |
| B-DECISION-02 | 固定 L1 继续显示 `fm.result.no_first_clear_bonus()`；当前合同只覆盖已固定发布物。扩展其它 publication 前必须取得实际 first-clear reward configuration fact | `PlayerBattleView.cs:231-237`；在 B-WIRE03 测试内断言固定 L1，另保留未来 expansion gate | 不仅凭 `Reward != null` 向未来内容外推；当前无新 API，未来缺事实则 BLOCKED |

`N-P05` 不在本任务直接处置范围：它继续 `DEFER_OUT_OF_CURRENT_SCOPE`，不得顺带实现 Craft 成本／产出新表面。

## 6. 263-key 快照输入与实现分类

- UGUI 固定测试输入是 §1 的只读 snapshot CSV，不再是可变 live `planning-localization-draft.csv`。snapshot 含 263 个唯一 key／264 个物理行／64708 bytes；本文引用的是其中现有/追加候选 key，不创建新 key，不编辑 snapshot 或 live CSV。该 COPY verdict 不替代 runtime/binding/device 验证，后续 live AMEND-04 也不得反向改变本包输入。
- `REUSE_EXISTING`：N-B01～N-B05；N-B06 复用后去重。
- `BINDING_ONLY`：B-WIRE-02～04；B-WIRE-01 在结构化 signal 获批后才是 binding-only。
- `DATA_CONTRACT`：N-D02/N-D04/C-D01 可由 internal adapter 完成；N-D03 是 hide/defer；N-D01 需受保护 API。
- `HIDE_INTERNAL_OR_DUPLICATE`：N-P01/N-P02/N-P06/N-P07/B-DECISION-01。
- `DEFER/HIDE`：N-D03、N-P05 以及 N-D01 未发布 Permanent kinds。
- 任何中英文模板参数集合必须与 CSV `parameters` 完全相同；不得新增 optional 参数。`operationName` 是已解析文本；`remainingTime/duration` 是 N-D02 数字格式；`errorCode` 是 N-D04 allowlist 结果。

## 7. C 执行白名单 delta 草案

### 7.1 本文独立 R `ACCEPT` 后的唯一源码白名单

现有 WIP 文件中仅可修改：

```text
Assets/Scripts/FightMatch/Application/PlayerNavigationModels.cs
Assets/Scripts/FightMatch/Application/PlayerNavigationSession.cs
Assets/Scripts/FightMatch/Input/GestureModels.cs
Assets/Scripts/FightMatch/Input/RouteGesture.cs
Assets/Scripts/FightMatch/Host/FightMatchHostView.cs
Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs
Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs
Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs
Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs
Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs
Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs
Assets/Scripts/FightMatch/Presentation/PlayerPermanentDetailView.cs
```

测试仅可修改：

```text
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs
Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs
Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationSessionTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPermanentFlowTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs
```

白名单精确为 15 个 source＋9 个 test＝24 个既有文件。本附录不新增源码、测试、asset、scene、prefab、meta、asmdef、package、resource、Luban 输出或 ProjectSettings 文件。public API 只允许 §7.2 精确冻结的两个 delta 组；不得改 serialization、save、routing、lifetime、controller admission 或业务计算。两个 Host 文件只可实现 §4.1 已冻结的 cause 转发与现有 handler 断言，不得借机改 Host 路由、session lifetime、启动或低内存语义；`LocalizationContractTests.cs` 只可实施下述固定输入替换。

`LocalizationContractTests.cs` 的唯一允许 delta：`ReadApprovedDraft` 路径精确改为 `../docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv`；常量精确改为 `ApprovedByteCount = 64708`、`ApprovedRowCount = 263`、`ApprovedSha256 = "d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c"`。读取后必须对完整 `bytes` 做 length 与 SHA-256 校验，并直接 `Encoding.UTF8.GetString(bytes)`；删除旧 `bytes.Take(ApprovedByteCount)`／`pinned` 前缀截取，禁止任何 truncation、prefix acceptance 或 live fallback。现有 9 列 shape、263 data rows＋header、唯一 key、双语言完整性、四级 severity、参数集合与 malformed-input 检查全部保留；fixture 现有测试方法数不变。

### 7.2 受保护 seam 精确变更

1. `N-D01`：精确增加 `PlayerNavigationConfirmation.OriginalKind : CandidateApplicationKind?`、`OriginalPermanentKind : CandidatePermanentKind?`、`OriginalClearsEquipment : bool` 三个 immutable getter；现有 internal constructor 增加对应参数，唯一构造者 `PlayerNavigationSession.View` 从同一 retained `operation.Intent` 按 §3 公式赋值。所有权是 Application read-model；Presentation 只消费。不新增 Unity serialized field，不进入 intent／record／codec／save，不改变 persisted bytes。现有 `Confirmation.Quote` 不改语义、不在冷路径填入完整 quote。
2. `B-WIRE-01`：在 `FightMatch.Input` 精确增加八值 public enum `GestureFeedbackKind`，并给 public `RouteGesture` 精确增加 `GestureFeedbackKind? TakeFeedback()`；不把 feedback 塞进 `GestureView`，不增加其它 public property/result/interface。分类产生、Input drain、Presentation `PointerCancellationCause`、Controller 的 `CandidateBoardVisibleFeedbackKind` 单槽/revision 和 accepted 判定全部按 §4.1。cause 的 internal overload 必须完整覆盖 `HostView Unbind/PausePresentation → PlayerBattleView Unbind → PlaybackView Unbind/Close → InputView Unbind/Close → BoardElement Unbind/CancelPointer → Controller CancelGesture`，所有现有 public/parameterless 入口保留并默认 `Cancelled`，所有 Unity `OnDisable` 调用点精确传 `FocusLost`。这些 overload／publish／clear／read properties均为 internal；它们不进入 input context、application request、save、serialization 或 persisted bytes，不改变 route intent、业务写入、session pause、playback finalization 或 token report。

以上两个 public delta 组的名称、类型、构造者、消费方与生命周期已冻结；§4.1 的 cause 与可见槽只是 Presentation internal implementation，不是第三组。主程将它们连同调用和测试一次送独立 R。C 不得改用 `GestureView` property、full quote、event bus、反馈队列、测试 hook、reflection、friend 扩张或 hardcode 绕过。

## 8. 验收与验证映射

### Q1 — 静态门（C 可在授权后执行）

- 精确 changed path 全在 §7 的 15 source＋9 test；无新文件、asset/meta/scene/asmdef/package/resource/ProjectSettings。
- §1 snapshot 的 64708 bytes／264 physical lines／263 data keys／完整 SHA 不变；fixture 只读完整 snapshot，不读 live、不截 prefix。扫描所有本轮 binding key 均存在、参数集合精确；live AMEND-04 变化不能令此门失败或改变此包输入。
- Core/Application/Input/Presentation 依赖方向不反转；friend 清单不变。
- public API 精确只有 §7.2 两个 delta 组；无第三组，无 serialization/save/routing/lifetime 变化。
- 扫描不得出现玩家侧 `ToString()`、raw enum/ID/commit/operation/endpoint/exception/message/field/revision fallback；未知映射必须进入 binding diagnostic。
- `PointerCancellationCause`、全部 cause overload、可见反馈槽与 revision 全为 Presentation internal；逐文件扫描每个 public/parameterless `Unbind/Close/Detach/PausePresentation/CancelPointer/CancelGesture` 都委托 `Cancelled`，每个已列 Unity `OnDisable` 都显式传 `FocusLost`，且 FocusLost 链中不存在先行无参调用。父级先／子级先／直接 HostView disable／直接 PlayerBattleView disable 的去重和 playback/session 顺序符合 §4.1。
- 15 个核心 ID 各有一个命名测试，均为非参数化；FocusLost 复用两个既有 real-handler 方法，fixture 输入更新不增方法，故目标测试数仍只增加 15。

### Q2 — 资源复开

本附录不增加冻结主包之外的新资源范围。冻结 UGUI 主包尚未执行的合法首次 Canvas/EventSystem/Prefab/TMP 资源生成、保存和 save-reopen 仍按其原 Q2 白名单与证据执行；本文不能把那些预定资源变化误判为 scope failure。本文新增 binding／动态行只能消费主包已批准的 serialized references 和 inactive templates，不得另加 asset/scene/prefab/meta。Q2 需区分“冻结主包预定资源差异”与“本附录额外资源差异”，后者才是 scope failure。

### Q3 — Compile

沿用 Unity `2022.3.18f1`、进程安全和冻结命令。源码冻结后 clean compile exit 0、日志无 `error CS`／`Compilation failed`／Unhandled Exception。本文作者未运行；只能由 C 串行执行并回证据路径、时间、退出码。

### Q4 — 目标 EditMode

- testFilter 保持冻结实施包列出的同一 15 个 fixtures，不新增 fixture 名。
- 基线 207 + 本补充 15 个非参数化测试 = **精确 222**；failed/skipped/inconclusive 均 0。
- `LocalizationContractTests` 已在原 15-fixture filter 内；只把其现有测试数据源改到 §1 snapshot，保持原测试方法数、严格 full-hash/shape/parameter assertions 与过滤器。15 个新增测试名仍是 §3 的 5 个与 §4 的 10 个；产品处置和 WIRE01/WIRE02 lifecycle 保持 FIX-01 原样。FIX-02 的父级先、子级先、直接 HostView disable、直接 PlayerBattleView disable、普通 Close/Unbind/low-memory 对照全部合并进既有 B16B04/P27_03 方法，不额外改变计数。
- 失败先只跑失败 fixture；修复后再跑完整 222。不得删/改断言、改 filter、用 Ignore/Inconclusive 求绿。
- Q4 证明 mapping/visibility/handler 行为；不冒充 Q2 资源复开、设备视觉或 COPY 独立 R。

## 9. 独立 R、停止条件与返回

- 本文作者只做静态自检，不能给自己的文档 `ACCEPT`。
- 两次工程 R-TOOLS 身份与 `NEEDS_FIX` verdict 均见 §1；FIX-01 已关闭 P1 snapshot/fixture 与 P3 retained visible feedback，FIX-02 只补 P2 的完整 disable→unbind cause 链。本次完成后只回同一 R-TOOLS 作 `QA-R-UGUI-COPY-BIND-01-FIX-02` 复审，不自行发 R，不得把 P1/P3 重开或把任何旧 verdict 当作 `ACCEPT`。
- 本文独立复审范围：FIX-02 只核 §4.1 的 public/parameterless default、完整 internal overload 链、所有 disable call points、父子顺序去重、普通 Cancelled 对照及对应 §7/Q1/Q4 映射；P1/P3、15/15 core、9 项产品处置、15＋9 白名单、两处 public delta、snapshot fixture 和 222 计数保持不动。R 必须只给一个 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`。
- COPY FIX-01 的 263-key planning/CSV 已由 §1 记录的独立 R 正式 `ACCEPT`；F1～F3 closed，81/47/263 accepted。它仍不证明本文 runtime/binding/device 行为，因此不能替代本文独立 R。
- 本文状态是 `READY_FOR_INDEPENDENT_REREVIEW`。两处 seam、P1/P3 和 FIX-02 的 P2 链已作为授权迁移内技术细化冻结，不再等待用户选择；但在同一 R-TOOLS 对 FIX-02 `ACCEPT` 和主程准确下发前不得交 C。这也不允许回写冻结 system/impl。
- 未来 C 返回必须含：snapshot/receipt 输入 SHA、实际 changed files、FIX-02 独立复审身份、15 条 acceptance 对照、P1 full-file fixture、P2 四类 disable 顺序＋普通 Cancelled 对照、P3 retained lifecycle 的证据、Q1～Q4 原始证据、源码 SHA、Unity 版本、测试 XML 计数、diff-check、未执行项。任何 cause 链中回落无参入口、live fallback、新业务规则、第三个 public API、附录外资源变化或 serialization 需求立即 `BLOCKED + proposal`。

## 10. 作者静态自检

- 核心覆盖：`N-D01..04`、`C-D01`、`N-B01..06`、`B-WIRE-01..04` = 15/15。
- 产品处置：`N-P01..04/06/07/08`、`B-DECISION-01/02` = 9/9；未重做 COPY，N-P05 明确保留 defer。
- 字段事实：核对 `Intent.Kind/GetPermanent`、Duration/Elapsed invariant、SourceKey、Diagnostic、TargetKind、head、Defined、GestureView、history/reference/recovery 当前成员与行号。
- 业务边界：没有复制 route、save、recovery clock、gesture 或 reward 规则；两个最小 API delta 已冻结为 §7.2 的唯一形状，无“或等价结果”分叉。
- fallback：没有允许 raw string/enum/ID/exception fallback；未知值统一 diagnostic/block。
- FIX-01 P1/P3 保持：fixture 固定为完整 64708-byte snapshot 且禁 prefix/live fallback；gesture/action/reference 共用一槽一 revision，刷新与切语言保留，owner input／解绑／关闭／上层 Dispose 清除。
- FIX-02 P2：所有 parameterless teardown 默认 `Cancelled`；所有列明 `OnDisable` 从实际最高层以 `FocusLost` 进入同一 internal overload 链，链内不回落无参入口；首个 active 层唯一发布，后续层 no-active，业务、播放、token 与 submit 顺序不变。
- scope：本次只修改本文；未修改冻结文档、planning/live CSV、snapshot/receipt、源码、测试、asset/meta/package/ProjectSettings；未运行 Unity 或 Git。
