# DEMO-028-MAC-R1 C 正式交付

状态：BLOCKED。源码修正已具体完成，补充验证尚未获准；当前不满足正式完成或派 R 最终验收的门槛，不作 ACCEPT 判定。
作者 C 01a0e404-d89d-7ab2-bece-3cd1df3fbc52；实际 turn 01a0e7e1-94a9-76c3-89cc-3a00f910b2c7；startedAt 1790596715；host local。
本轮实现同源战斗会话、原请求恢复、退出/原基线重来/结算、完整参与者原收据、借用视图与已核默认参考。最后运行 4010/4011 EditMode 通过，原 3956 次具名出现全部保留，新增 55，失败 1、跳过 0；该失败之后的一行测试修正尚未编译或执行。

## 固定输入与作者范围

- Git 起止 14f0bd76776a211ed588561691616b3523f73395；接收源码 7464efd10e4d2ed5341bbc1e2d179364dd94489c；未提交、推送、建分支或工作树。
- §434 冻结段：13458 bytes / SHA256 08988b13282b1dafa4fcc0a095b66721a0a6bd98c79532c57d9543da1e11d06c。
- 只读计划：205569 bytes / SHA256 037692abd5c262d7cd4efa40dcc93a2d74235fcd9f02c6c46e83ae5ea8725fe6；设计：25827 bytes / SHA256 3b2eb4e8863ebc0cc8842f98c425520af78c8df5ddd765242e7f6f8a513a0b39。
- root-identity：13137 bytes / SHA256 a1d036a804bd716dfebf769d54b10df7ed8a1b061089c527f89ec9389a7e4482；准确作者映射位于冻结段之外。
- 作者 28 个源/工具路径＋本报告与 code-scope 两文件，共 30；五份 SD00 元数据及其计划文件另列，不归为 C 产品修改。

| 文件 | 修改与行数预算 |
|---|---|
| `Assets/Scripts/FightMatch/Content/PublishedContentModels.cs` | +6 / −2，增删 8/36；最终 238 行 |
| `Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs` | +1 / −1，增删 2/24；最终 187 行 |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | +46 / −10，增删 56/60；最终 112 行 |
| `Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs` | +33 / −7，增删 40/90；最终 114 行 |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | +17 / −4，增删 21/75；最终 182 行 |
| `Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs` | +25 / −0，增删 25/56；最终 204 行 |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs` | +23 / −2，增删 25/90；最终 266 行 |
| `Tools/Invoke-FM025P2Validation.ps1` | +114 / −8，增删 122/200；最终 605 行 |
| `Assets/Scripts/FightMatch/Application/PlayerBattleSession.cs` | 329/520 行 |
| `Assets/Scripts/FightMatch/Application/PlayerBattleModels.cs` | 138/280 行 |
| `Assets/Scripts/FightMatch/Presentation/PlayerBattleController.cs` | 135/300 行 |
| `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs` | 153/350 行 |
| `Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs` | 134/240 行 |
| `Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs` | 230/320 行 |
| `Assets/Tests/EditMode/FightMatch/PlayerBattleFlowTests.cs` | 195/350 行 |
| `Assets/Tests/EditMode/FightMatch/PlayerBattleRecoveryTests.cs` | 222/420 行 |
| `Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs` | 167/360 行 |
| `Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs` | 156/280 行 |

10 个新 .meta 由 Unity 自然产生，每个 11 行；旧 441 个 .meta 原字节和 GUID 全部不变。具体逐件 before/after 长度、SHA、原文本与完整新文本见 code-scope 及 read-manifest。

## 行为和 B01–B12 实证

以下具名方法均在最后运行 `runs/006/tests.xml` 中有实际记录；通过/失败逐项列出。该 XML 对应修正前的测试版本，不能作为当前修正后源码的全量通过证据；参数化出现和源码行定位见 `case-evidence.json`。

### B01

同一缓存 session；实际 H02；重复/外来/旧头/RootBack 零新操作；到期恢复可越过静态 NoReady 提示；Resume/WPS 走原战斗。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B01_HostSelectionIsOwnerBoundConsumedOnceAndUsesRealFormationEntry`：1 Passed / 0 Failed（run006）。
- `PlayerBattleFlowTests.B01_StaleHeadAndRootBackNeverPrepareABattle`：1 Passed / 0 Failed（run006）。
- `PlayerBattleFlowTests.B01_DueRecoveryPassesStaticNoReadyHintAndIsCompletedInsideH02`：1 Passed / 0 Failed（run006）。

### B02

隔离多面实际历史列表选择同一 range，确认后实际 Rollback；取消/旧弹窗/旧头/重复提交零写；原单项手势保留。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B02_ActualMultiFaceHistoryListRetainsOriginalRangeAndCommitsItsConfirmationOnce`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B02_ActualHistoryListButtonShowsExactRangeAndStaleConfirmCannotSelectAnother`：1 Passed / 0 Failed（run006）。

### B03

确认冻结身份，取消零时钟/零写；Exit 不造新局；Restart 保留成员、成长和三域初态且不读结束时钟、不新 H02。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B03_EndPreviewCancelAndOldConfirmationAreZeroWrite`：2 Passed / 0 Failed（run006）。
- `PlayerBattleFlowTests.B03_RestartPreservesOriginalMembersGrowthAndThreeRandomInitialsWithoutH02`：1 Passed / 0 Failed（run006）。
- `PlayerBattleFlowTests.B03_ExitConfirmsOneOriginalRequestAndCreatesNoAttempt`：1 Passed / 0 Failed（run006）。

### B04

实际 WPS 与 S17；首个 Playback Changed 重入在真实 token 解除前拒绝且不读时钟；完成/Skip 后原预留 ID H06；ActiveHistory 移除。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B04_FirstPlaybackFinishCallbackStillRejectsSettlementUntilActualTokenRelease`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B04_B06_PersistedWpsRebuildContinuesItsReservedSettlementExactlyOnce`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B04_ButtonAvailabilityTracksRealTokenAndActualReceiptAfterHistoryRemoved`：1 Passed / 0 Failed（run006）。

### B05

三成员按 CharacterId＋原槽映射完整 OriginalLookup 的奖励、经验、结束、倒下和材料；页面来源为原收据。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B05_ThreeMemberReceiptMapsFullOriginalLookupByCharacterAndEntrySlot`：1 Passed / 0 Failed（run006）。

### B06

已通过用例覆盖 SaveFailed/Unknown 后重建视图保留原请求/候选、全对象重建取回原意图、丢失响应取准确 lookup；S17 marker 失败路径的 Retry、收据及一次奖励断言未执行到，待补充验证。
本项状态：BLOCKED: run006 witness failed。

- `PlayerBattleRecoveryTests.B06_LifecycleFaultRetriesOnlyTheRetainedOriginalCandidate`：4 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B06_PrepareFailureWithoutTicketStillKeepsAnExplicitRetry`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B06_WholeApplicationRebuildReadsOriginalIntentFromObservedDiskCandidate`：4 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B06_CommittedResponseLostRebuildUsesSavedRecordIntentAndExactLookup`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B04_B06_PersistedWpsRebuildContinuesItsReservedSettlementExactlyOnce`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway`：0 Passed / 1 Failed（run006）。
- `PlayerBattlePresentationTests.B06_B09_RecoveryPageEndsOnlyTheActualInputOwnedPendingRequest`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B06_B09_EntireViewTreeRebuildPreservesHostInputPlaybackAndOriginalAttack`：3 Passed / 0 Failed（run006）。

### B07

已通过用例覆盖 F2 委回原创建及错误身份/线程/忙碌/释放拒绝；失败用例已验证 S17 End 拒绝，但迁移拒绝及随后零写检查未执行到，不作完整通过声明。没有任意 pending getter。
本项状态：BLOCKED: run006 witness failed。

- `PlayerBattleRecoveryTests.B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway`：0 Passed / 1 Failed（run006）。
- `PlayerBattleRecoveryTests.B07_F2DelegatesOriginalCreationAndNeverStartsBattleOrASecondProfile`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B07_IdentityWrongThreadBusyAndDisposedRejectBeforeClockOrStorage`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B07_NarrowResumedBridgeRejectsWrongCommitOperationOwnerThreadAndBusy`：1 Passed / 0 Failed（run006）。

### B08

NotificationFailure 按实际已提交显示；重复查询/重开不再奖励或播放；全对象重建与返回地图后重开同一原收据，后来成长只改最新 HUD。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleRecoveryTests.B08_NotificationFailureAndRepeatedQueriesNeverResubmitOrReplayRewards`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B08_FullObjectRebuildReopensOriginalReceiptAndLaterGrowthDoesNotRewriteIt`：1 Passed / 0 Failed（run006）。
- `PlayerBattleRecoveryTests.B08_OriginalResultRejectsWrongKindMissingForeignAndStaleContextWithoutWrites`：1 Passed / 0 Failed（run006）。

### B09

借用 Detach 不结束播放、不清请求；显式关闭及宿主 Dispose 只完成原 token 一次；旧页/旧按钮失效；无变化刷新保留按钮与上下文；恢复入口指向实际 pending owner。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattlePresentationTests.B09_ReadOnlyRefreshKeepsTheOriginalActionButtonAndContextClickable`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B06_B09_RecoveryPageEndsOnlyTheActualInputOwnedPendingRequest`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_OriginalAttachRebuildsMemberButtonsForItsNewOwnedController`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B06_B09_EntireViewTreeRebuildPreservesHostInputPlaybackAndOriginalAttack`：3 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_BorrowedDetachDoesNotFinishButExplicitCloseAndFinalHostDisposeReportOnce`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_ActualPanelDetachAndOldConfirmationButtonCannotEndReboundBattle`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_ActualBorrowedInputStaleRetryButtonCannotCommitAfterRebind`：1 Passed / 0 Failed（run006）。
- `PlayerBattlePresentationTests.B09_StalePageEpochAndDisposedHostCannotSubmitOrAcknowledgePlayback`：1 Passed / 0 Failed（run006）。

### B10

六件首包和固定 fingerprint 未变；精确 Resolve 后不可变参考方法不进入反射序列；标签列实际条件、00..2f 种子与当前差异。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerDefaultReferenceTests.B10_ResolvePreservesAlreadyVerifiedImmutableReferencesWithoutSerializedProperties`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B10_ReferenceLabelsShowOpeningConditionsExactSeedAndCurrentDifferences`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B10_ExactReferenceRejectsOtherLevelVersionOrBindingWithoutBusinessWork`：3 Passed / 0 Failed（run006）。

### B11

独立覆盖播放原路线、未杀消线、击杀固化；真 scene/HP/意图/偏好/head/history 零写，不回报真实 token；首个 Dragging 关窗但同一指针实际提交；旧按钮/计时回调失效。免费补线边界见后文。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerDefaultReferenceTests.B11_OriginalNonkillClearAndKillingLockFactsPlayOnlyInIndependentOverlay`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B11_ActualFirstDragClosesWindowAndOverlayWhileTheSamePointerSubmits`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B11_RealPlaybackBlocksReferenceAndClosingReferenceCannotAcknowledgeItsToken`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B11_FaceMismatchKeepsExplanationButCannotOverlayAnotherFace`：1 Passed / 0 Failed（run006）。
- `PlayerDefaultReferenceTests.B11_ClosedReferenceOldStepButtonAndTimerCannotReviveOverlay`：1 Passed / 0 Failed（run006）。

### B12

结果实际回地图/队伍/背包并核同资料；合法重打新 H02；下一关只读真实发布/OpenFact，L1 无下一关且无配方准确为空。
本项状态：Run006 witnesses passed; corrected final source validation pending。

- `PlayerBattleFlowTests.B12_ResultDestinationsUseLatestSamePlayerAndRealPublishedEmptyContent`：3 Passed / 0 Failed（run006）。
- `PlayerBattleFlowTests.B12_ResultReplayCreatesANewH02AndCannotInventNextLevel`：1 Passed / 0 Failed（run006）。

## 串行验证与真实失败保留

唯一入口为固定外置 PowerShell：`-NoProfile -File Tools/Invoke-FM025P2Validation.ps1 -Stage DEMO-028-MAC -Mode Compile|Tests -ExpectedScriptSha256 <各次实际SHA>`。全部完整 argv、UTC、PID、运行源/工具/DLL/运行时身份和退出码见 platform-environment 与各 run/result。Tests 无 filter、category、assembly 或 -quit。

| 执行顺序 / 槽 | 模式 | Unity PID | wrapper / Unity exit | 实际结果 |
|---|---|---:|---|---|
| 1 / 001 | compile | 12635 | 1 / 1 | FAILED（原证据保留） |
| 2 / 003 | compile | 13369 | 1 / 0 | FAILED（原证据保留） |
| 3 / 004 | compile | 13744 | 0 / 0 | PASS |
| 4 / 002 | tests | 13888 | 1 / 2 | FAILED（原证据保留）；3996/4008 Passed |
| 5 / 005 | compile | 15156 | 0 / 0 | PASS |
| 6 / 006 | tests | 15296 | 1 / 2 | FAILED（原证据保留）；4010/4011 Passed |

001：测试编译报三处 internal 手势不可见、一处 Budget 歧义、两处 Content 命名空间冲突；仅修本轮测试入口/限定名，并修旧 Attach 成员按钮绑定与实际面板回归。
003：Unity exit 0，但工具将 10 个新自然 meta 的 59→243 byte 导入扩展误按旧资产拒绝。GUID 未变、旧 441 meta 未变；限定豁免仅 Compile 中的已批准新 meta，Tests 仍要求同版。003 不改判为通过。
工具保留预留首个完整 Tests 槽 002；必要 Compile 003/004 先行，按实际 UTC 排序并查找同版成功编译，不覆盖或伪造旧槽。
002：4008 次出现中 3996 通过、12 失败，原 3956 全部通过。初步诊断曾将 11 个新失败归为胜利收据检查；006 随后证实其中 S17 恢复用例另有测试空头解引用。真实历史记录保留在 platform-environment，不以初步归因替代最终结果。
002 后将公共原收据身份检查改用已核 Lookup.Baseline，并断言胜利 TerminalRun 为 null；仅将隔离 Recoverable 测试夹具的 AttackRange 设为 2，使其敌人 ordinal 2 选择合法。006 中相关失败已消除，剩余 S17 测试失败及修正详见后文。
002 结束后合并本轮 UI 检查确认的四项修正：无变化刷新保持按钮/上下文；替换页面及宿主释放时解绑旧子回调；参考关闭按钮绑定窗口 generation；恢复按钮只服务实际 pending owner。新增实际面板回归，005 Compile→006 无筛选完整 Tests 得到 4010/4011；仍不能称为最终全量通过。
槽 002 超过 90 分钟时于 2026-09-28T14:45:10.879813+00:00 做一次有界只读 PID/日志/3 秒采样；原始数据保存在 scope-audit.boundedRunDiagnostics；未终止或重启 Unity，未以 CPU/日志变化代替通过证据。
槽 006 超过 90 分钟时于 2026-09-28T16:37:45.573157+00:00 做一次有界只读 PID/日志/3 秒采样；原始数据保存在 scope-audit.boundedRunDiagnostics；未终止或重启 Unity，未以 CPU/日志变化代替通过证据。
最终工具：57515 bytes / SHA256 32b5cfac5c830e043af32f91be32946432b2fe5eb999e35e62a2ecf8bd86fed9；AST 实际通过（0 解析错误、10 函数）；git diff --check 实际 exit 0。
最近编译 005→Tests 006 的运行前后源码/Assets/DLL/工具/六运行文件逐件同版；当前额外一行测试修正尚未编译或执行。原 3956 次出现（3951 不同名）multiset SHA `4d92efa98eefb19e1f97ecb2435e605595a89b6a4e76bf150656c5b7dd1bddcd` 全部保留，原 3872 基线包含其中。

## 文件、证据与物理 I/O 核对

实现 808→828、Assets 838→858、GUID 441→451、DLL/PDB 36→36；933 protected inputs 原有 15 项缺失未制造，允许产品差异和 SD00 文档差异分开记录；六运行文件、四 golden、既有发布与旧测试均复核身份，未重跑恢复器或旧 CONT-C 阶段。
`read-manifest.json`：实际 104 叶，排自身 103 行，均在固定 128 白名单内；所有实际叶逐件路径/长度/SHA齐备，未执行/不适用叶保持缺失。
继承唯一物理根 `TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform/io`：旧 2605 GUID cases / 19778 条目逐件未变；本轮新增 1114 cases、8410 条目；旧 case 写入/删除/改变均 0。最终剩余 case 377、文件/链接 22554，未清旧目录或扩容；每次 Tests 前由唯一工具核 768/8192 余量。
I/O after：7146510 bytes / SHA256 404b157b2e5679369e6e7e523154e8ec98b8cced893187714dbb6a5e83502291；范围 JSON 只引用大 I/O 清单身份及差异，不重复内嵌。

## 具体阻塞与已完成的一行修正

唯一失败为 `PlayerBattleRecoveryTests.B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway`，run006 XML 为 NullReferenceException，源码第 123 行。完整对象恢复后的 marker 写入失败时尚无已核发布头，测试却读取 `head.Header.CommitId`。该用例已到达原 operation/canonical bytes 和 End 拒绝断言；迁移后的零写检查、原 S17 Retry 收据及一次奖励断言尚未执行到，B06/B07 因此不能完整交付。
已将该测试唯一一行参数由 `PrepareRosterMigration(head.Header.CommitId, Codec())` 改为 `PrepareRosterMigration(resumed.OriginalIntent.ExpectedCommitId, Codec())`，直接取恢复自磁盘的原意图预期头，未用旧内存请求假装恢复；没有在006之后改产品代码。
run006 测试文件：18621 bytes / SHA256 ad89842114c2ed87f74c0dc32946c9d12dbd54ca4cfa8d1c61441ed2fefdb7b8；当前：18640 bytes / SHA256 6e75264ab8e4e9b25b03ec7e277f0ed38c7d646d66386b2836ed44b2b23512b9。反向替换这一行可精确重建 run006 的长度/SHA；当前文件及全部28源/工具 after-text 均已保存，静态预算与 diff --check 通过，但新版本 Compile/Tests 为 NOT VERIFIED。
§434 的001～006槽全部实际使用，固定128叶不能自行加007/008或覆盖原失败；当前继承 I/O 为3719 cases，4096上限只余377，距下一次全量前768余量还差391。
最小补充范围：请 SD00 签发单独的有限纠正验证包，列新的 evidence/run 分配及配套 plan/root/工具身份，保留本根和001～006；仅对当前已修源码执行一次 Compile→一次无filter完整 EditMode，并明确把继承case上限调至至少4487（3719+768）。文件/链接17446，40000上限仍余22554，无需调高。无需新增产品源码、公共API、依赖、正式资源或重跑旧CONT-C/golden。
完成门：当前修正版本 Compile 成功、同版全量4011次出现全通过且原3956多重集合保留、B06/B07未到达断言实际执行、各身份与旧I/O复核、随后由SD00核收并派R独立审查。C未启动额外Unity运行、未扩容或清旧目录。

## 明确边界

- 本轮是 Mac EditMode：含真实 Editor 面板事件和内存存储中完整 Application/PlayerSession 对象重建；Android 宿主、真机物理输入、真实 Player 进程冷启动及 APK 尚未验证。
- 原首包验证器只选择 Attack，已核默认参考没有免费补线记录；没有伪造“已核 Link”。覆盖层复用原 ordered facts/frame 处理，免费补线的参考正例未在本轮公开参考数据中验证。
- 三个 internal 手势方法由新测试夹具反射调用原入口，不读取/改写私有字段；首拖与旧回调另由真实 UI Toolkit pointer/navigation-submit 事件验证。
- 未改 Core 算法、存档序列/协议、Packages、ProjectSettings、asmdef、正式发布资源或其他测试；未开新聊天/代理/自动任务，未安装或移动运行环境。

交付文件：本报告、`demo-028-mac-r1-code-scope.json`；主证据 `TestArtifacts/FMDemo028/mac-r1/read-manifest.json`、`case-evidence.json`、`runs/006/tests.xml`。本包尚未达到完成门，须先取得补充验证授权和实际通过证据，再由 SD00 决定派 R 独立审查。

BLOCKED
