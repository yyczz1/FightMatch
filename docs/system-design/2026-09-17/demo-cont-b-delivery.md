# CONT-B 实施正式交付

作者状态：`COMPLETED`。§350+353 的范围内实现与规定验证已完成，等待原 R 独立审查；这里不填独立 ACCEPT，也不提前把功能计数从 34 / 30 / 4 / 51 改成已接收。
C task `01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；准确 turn `01a0e09d-d68a-73c3-a25a-4ad32024a662`；原生 context `2026-09-27T02:07:15.669Z`，`gpt-6-astra / max`，工程根 `D:/Unity/UnityProj/FightMatch`。原生 completed 的实际事件须在 formal 返回后核，不预填时间。
原实施 turn `01a0d8ff-a35c-7bd3-9df1-0d882cc88946`、上一实施续接 `01a0db48-60c4-7590-83b4-b40675a06583`、BLOCKED turn `01a0e083-b83f-7993-9fbc-b4b0d208c26e` 及全部失败证据保留。原/C1 六份设计稿保持原文。

## 最终验证

012 Compile：实际 Unity exit 0，无编译错误；PID 17316，UTC 2026-09-27T02:09:39.8228300Z→2026-09-27T02:10:15.3847958Z，35.5619658 秒。
013 无 filter 完整 EditMode Tests：实际 Unity exit 0，3851/3851 通过，0 失败/0 跳过/0 其他；PID 16424，UTC 2026-09-27T02:11:09.7377216Z→2026-09-27T03:21:49.8765577Z，4240.1388361 秒。Tests 没有 -quit。
013 XML 2842709 字节 / `ab1de23bc26f4adc70084dfe0144523a90b4838ff43f35158d6b489565240550`；准确 argv、进程与时间、log/stdout/stderr/result 在 scope.validation.runs 和原 runs/012、013 实物。
最终 012 编译后 = 013 测试前 = 013 测试后 = 当前交付：776 实现、806 Assets、36 DLL/PDB 的 bytes/SHA 全一致，工具身份也保持。测试后记录 Unity 进程数 0；验证期间无源码修改。
原 3733 occurrence / 3728 fullname 全部保留并通过，多重集合 SHA `ac37f55c31f4d154addd11f37573f1d834ce32c786cecee425eb090246381c2c`；新增 118 项全部通过。新旧用例集合、实际结果与断言位置均在 scope.validation。

## §353 精确补签及本轮改动

旧 PlayerRosterSessionTests.cs:202 仅将合同期望 10→14，:203 仅增 fm.player.permanent.v1；共 2 增/2 删。UnsupportedBinding、callback=0、存储不变和所有其他断言保持，最终原 3733 项均通过。
旧测试 before 16870 字节 / `0693d2c08cda6f781849501eb31424f6d6ed6cf00a3f87ad223c4e796b736e18`；after 16896 字节 / `e8f4351b6029084bf08c2461e61fbe41cbf55ef0c26471130b4d85cb74ce869a`；新增准确 before/after 文本副本已保存。
CandidatePermanentProtocolTests.cs:285、293 的读取器流长度修正承接上个 BLOCKED turn；本轮在 012/013 验证，CB16 正常往返和受益职业篡改拒绝均通过。
工具仅在 CONT-B 把 maxRuns 12→13，并将 runRecord.authorTurnId 绑定本次准确 turn；原 root-identity 的首次实施 turn 保持。原其他 Stage、SHA 自检、进程安全及失败/异常写入协议保持。
当前工具 21835 字节 / `edcabaf8f655f55f8ceb467dae8f18bf852b16ca41f7dceaefc6e44cde655f2d`；相对原始入口 30/40 增删、累计 173/183（绝对 200）、271/320 物理行。12→13 改的是本包已新增行，不额外增加相对入口计数。
仅新增 013 的已签 12 个允许叶，实际按运行产生；没有预创建多余叶、复用旧槽或删除失败。精确 272 路径数组在 scope.technicalSupplement.proposedFiniteAllowedRelativeProjectPaths，最终 manifest 仅列真实文件。

## 同产品路径、接口与格式

PlayerSessionSystem 的 QueryPermanent(string)、PreviewPermanent(PlayerPermanentDraft, SaveCodecBudget)、ConfirmPermanent(PreparedPlayerPermanentPreview, SaveCodecBudget)、PreparePermanentMigration(string, SaveCodecBudget)、PrepareTeachingEntry(DefinitionBinding, string, SaveCodecBudget) 使用既有 Application/runtime/M02/M12。准确完整声明和位置见 scope.implementationContract.actualPublicDeclarationText。
查询/预览不分配操作/战斗身份、不取熵、不保存；确认冻结并核原 head、精确绑定、角色/M04/M05 修订和来源端点。已存在 OperationId 优先返回原结果，同 ID 异意图拒 OperationConflict。任意公开 builder 仍拒且 callback=0。
M03 拥有永久经验/等级/学习及效果；M04 拥有 T/L/R/F、来源份额与唯一终点；M05 拥有教学 once；M02 保存原意图、操作/提交和后继关系。正式路径没有平行 Demo 副本或绕开原可信门。
卡先广告既有合法持有份额再普通来源，固定同 q 的整卡最少量；所有经验完整保留余量。非卡成本有多个合法选择时明确选择；合并重复配方输入、多批产物保留实际完整前序，混合来源不被重标成广告卡。
首证保留给指定战士学习；另普通证可供合法法师。教学入场赠物、学习和说明各自沿 M05 once/原操作持久路径，确认之前不扣物、取消不消耗，不重复发证或返还已耗份额。
真实首包当前缺卡/技能/配方定义时返回 NoPublishedDefinition；没有改首包、伪造 M10 审定或正式赠物。普通正例为合法隔离定义→原 M07 发物→共用永久 Build/M02 Propose/codec→M12 内存保存重建。
已有广告证明仅在同一 owner 消费核的隔离 fixture 验证。真实会话缺既有认证来源索引时拒 UnsupportedSourceProof，不由 DTO/SelectedHolding 授予写权，不制造 InclusionEvidence。
SourceSlice/EffectReceipt 保持原 Grant、line、acquisition、operation/commit、binding、半开逻辑区间、当前唯一端点和完整后继。未提交的当前 M07 结算候选不提前暴露 Held；实际提交后只引用原 commit，缺提交的伪已成记录仍拒，CB05_CB19 回归最终通过。
PublishedPermanentV4=4、owner wire [4,4,4,4,2,2]、FMINT004 kind13 永久请求/kind14 显式迁移；恢复能力 14 合同、3 features。原 schema1/all2/v3、FMBIZ001、FMINT001/2/3、FMPROF01/02、298 字节原建档配方保留，没有 FMPROF03。
迁移保留角色 ID/三个槽/FormationRevision、所有原 owner 和 F2 初始创建锚；v4 QueryRoster 的 Format 仍 v4、IsMaterialized=true。all2 先原 idle2→3，未知先原版本续办，缺原内容绑定不换 latest。
活动成长/学习/制作/偏好保留原 M06 stats/HP/三域/入场成员/history/restart 基线；回退保当前偏好、重开保冻结基线。未支持的新 Mage/已学技能 H02 在 ID/熵/冻结前拒绝；原演出 token/运行门保持。

## 范围与预算

31 个旧生产白名单实际改 30 个，CandidateApplicationModels.cs 原文保留；旧生产增删 731/1638、新生产 1863/2230，合计 2594/3868（硬上限 4000）。6 新测试共 1737/2530（硬上限 2600）；16 个 Unity 自然 meta 保持。另 §353 旧测试例外 4 增删行，不计入生产预算。
33 before、65 after 已逐字节核；原 744 实现中授权例外外 712 项未改；848 原保护输入扣 33 可变项和 3 最新协调稿后，812 不可变项零差异。776 实现 / 806 Assets / 425 唯一 GUID / 880 导出与精确集合相符，409 原 meta 均保持。
表中旧生产省略共同前缀 Assets/Scripts/FightMatch/；完整路径、before/current 身份、原文副本和统一 diff 见 scope.scopeAudit.oldSourceAndToolChanges。

| 旧生产文件 | 实际增删 / 上限 |
|---|---:|
| Core/BattleEntryInput.cs | 2 / 4 |
| Core/CandidateGrowthDefinition.cs | 5 / 45 |
| Core/CandidateCharacterGrowth.cs | 56 / 110 |
| Core/CandidateInventoryState.cs | 8 / 40 |
| Core/CandidateInventory.cs | 2 / 30 |
| Core/CandidateProgressionState.cs | 11 / 30 |
| Core/CandidateProgression.cs | 8 / 47 |
| Core/CandidateRosterState.cs | 10 / 55 |
| Core/CandidateRosterSaveCodec.cs | 3 / 25 |
| Core/CandidateRosterProtocol.cs | 18 / 25 |
| Core/CandidateBusinessSnapshot.cs | 2 / 8 |
| Core/CandidateBusinessSaveCodec.cs | 59 / 110 |
| Core/CandidateBusinessRestoreChecks.cs | 90 / 130 |
| Core/CandidateBusinessSaveValues.cs | 4 / 15 |
| Core/CandidatePermanentSaveCodec.cs | 77 / 90 |
| Core/CandidateApplicationIntent.cs | 21 / 35 |
| Core/CandidateApplicationIntentCodec.cs | 35 / 90 |
| Core/CandidateApplicationProtocol.cs | 12 / 95 |
| Core/CandidateApplicationRecords.cs | 7 / 45 |
| Core/CandidateApplicationReferences.cs | 142 / 170 |
| Core/CandidateApplicationSaveCodec.cs | 71 / 100 |
| Core/PublishedSaveContext.cs | 19 / 60 |
| Application/CandidateApplicationModels.cs | 0 / 30 |
| Application/CandidateLifecycleApplicationSystem.cs | 5 / 45 |
| Application/CandidateLifecycleModels.cs | 6 / 40 |
| Application/CandidateLifecyclePreparation.cs | 26 / 50 |
| Application/CandidateLifecycleEntry.cs | 2 / 20 |
| Application/CandidateLifecycleRoster.cs | 9 / 35 |
| Application/PlayerRosterSession.cs | 15 / 30 |
| Application/PlayerSessionSystem.cs | 4 / 25 |
| Application/PlayerSessionModels.cs | 2 / 4 |

| 新源码文件 | 物理行 / 上限 |
|---|---:|
| CandidatePermanentDefinitions.cs | 230 / 230 |
| CandidatePermanentModels.cs | 269 / 270 |
| CandidatePermanentGrowth.cs | 136 / 160 |
| CandidatePermanentInventory.cs | 279 / 280 |
| CandidatePermanentProgression.cs | 152 / 180 |
| CandidatePermanentProtocol.cs | 269 / 270 |
| CandidatePermanentCodec.cs | 249 / 250 |
| PlayerPermanentSession.cs | 137 / 210 |
| CandidateLifecyclePermanent.cs | 37 / 230 |
| PlayerPermanentModels.cs | 105 / 150 |
| CandidatePermanentGrowthTests.cs | 207 / 400 |
| CandidatePermanentInventoryTests.cs | 395 / 420 |
| CandidatePermanentProgressionTests.cs | 144 / 360 |
| CandidatePermanentProtocolTests.cs | 337 / 400 |
| PlayerPermanentSessionTests.cs | 263 / 450 |
| CandidatePermanentTestData.cs | 391 / 500 |

## CB01～20 作者验收证据

各 CB 作者结论为 PASS（保留下述实际/隔离/未验证边界）；完整验收文本、实际方法/断言与 XML 映射见 case-evidence.json 及 scope.validation。一个具名用例可对应多个 CB，表中数量不能相加；该作者结论不替代 R 独立审查。

| 条目 | 最终证据 | 作者结论 |
|---|---|---|
| CB01 | 2 个映射用例通过 | PASS |
| CB02 | 9 个映射用例通过 | PASS |
| CB03 | 1 个映射用例通过 | PASS |
| CB04 | 2 个映射用例通过 | PASS |
| CB05 | 19 个映射用例通过 | PASS |
| CB06 | 7 个映射用例通过 | PASS |
| CB07 | 3 个映射用例通过 | PASS |
| CB08 | 1 个映射用例通过 | PASS |
| CB09 | 2 个映射用例通过 | PASS |
| CB10 | 2 个映射用例通过 | PASS |
| CB11 | 2 个映射用例通过 | PASS |
| CB12 | 10 个映射用例通过 | PASS |
| CB13 | 1 个映射用例通过 | PASS |
| CB14 | 35 个映射用例通过 | PASS |
| CB15 | 5 个映射用例通过 | PASS |
| CB16 | 15 个映射用例通过 | PASS |
| CB17 | 3 个映射用例通过 | PASS |
| CB18 | 3 个映射用例通过 | PASS |
| CB19 | 2 个映射用例通过 | PASS |
| CB20 | 012/013 exit 0；身份、范围、旧集合、保护和证据检查 | PASS |

## 全部运行与原证据

| 槽 | 模式 | Unity 实际 exit | 实际结果 |
|---|---|---:|---|
| 001 | compile | 199 | 失败/中断，原证据保留 |
| 002 | compile | 1 | 失败/中断，原证据保留 |
| 003 | tests | 1 | 失败/中断，原证据保留 |
| 004 | tests | null | 失败/中断，原证据保留 |
| 005 | compile | 1 | 失败/中断，原证据保留 |
| 006 | compile | 0 | 编译通过 |
| 007 | tests | 2 | 3845 总 / 3760 通过 / 85 失败 / 0 其他 |
| 008 | compile | 0 | 编译通过 |
| 009 | tests | 2 | 3850 总 / 3771 通过 / 79 失败 / 0 其他 |
| 010 | compile | 0 | 编译通过 |
| 011 | tests | 2 | 3851 总 / 3849 通过 / 2 失败 / 0 其他 |
| 012 | compile | 0 | 编译通过 |
| 013 | tests | 0 | 3851 总 / 3851 通过 / 0 失败 / 0 其他 |

004 中断时未观察到退出码/结束时刻且无 XML，仍为 InterruptedWithoutExitObservation，不补造 exit 0。旧 007/009/011 的失败及原 log/XML 全部保留；011 的长墙钟间隔仅保留原始时间，不猜测原因。成功最终门仅引用 012/013。
七个已接收历史根的 4897 个有限 item、各 root-identity/read-manifest 身份均 fresh 核对无差异；旧有效 P1/内容/CONT-A 验证未单独重跑。原/C1 六份设计报告、首包六实物与原建档配方保持。
本根 root-identity 保持 723 字节 / `61d65e1fe0c4b93a2b23784f6276bfebe4a36c76b5f493264b2b8d9cf7fc4854`。§350 冻结段 24904 字节 / `0dbc27d8d828bff99ac32d4514ec9c531da94d5d35655b90d6a296a96f571bb1`；本次 §353 段 3174 字节 / `fd72b4c8cbbd07bf6dc51849baa4557095d2ff009a2170926d055fbcbddd9ab1`。
最终 read-manifest 49187 字节 / `9626ae349f37da18b7fb109e6be12a461d97e31b5210240ba1dd3301ae73df9d`；排除自身列 218 项，含 manifest 实际 219 文件，均在 272 准确路径内；actual≤260、items≤512、manifest≤4 MiB。所有 33 before/65 after 与 17 根叶已完成。
上一 BLOCKED 两稿身份和原交付文字留在 scope.priorBlockedImplementation；保留此前失败、起点和授权变化。当前 scope 不嵌自身 SHA，两份最终报告由 formal 外部给身份。

## 下一门与真实限制

原 R 先核本次准确 C 原生 completed、非异步 formal COMPLETED 和两正式实物，再独立审实际 diff/契约/CB/格式/F2/来源/故障/保护及同版运行，给唯一 ACCEPT/NEEDS_FIX/REJECT；作者不代写 R 报告，不预填其 verdict。
后续 CONT-C 页面可据上述共用 PlayerSession API 和冻结 DTO 准备接入；范围与正式实施仍由 SD00 后续签发。本轮没有启动新模块、任务/代理或 Git 操作。
证据仍限真实首包/PlayerSession 门、合法普通来源的隔离定义路径、已有广告持有的同 owner 核与 M12 内存故障/对象重建。公开 AwaitLinks、物理鼠标/触控/像素、交互式 PlayMode、Player/Android/真机、未覆盖物理保存故障及 §184 NOT VERIFIED 继续保留，不据绿测清除。五分钟自动跟进仍取消。
