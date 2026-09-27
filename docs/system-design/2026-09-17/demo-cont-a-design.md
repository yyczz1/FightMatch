# CONT-A 三槽队伍与角色资料：实施设计交接

状态：DESIGN_COMPLETED，仅 §329 的设计交付；没有实施授权，不自判独立 ACCEPT。
C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确 turn：01a0d71f-f907-77e3-8827-1e6a32ef226d。
§330 登记原生 context=2026-09-25T05:53:12.019Z、gpt-6-astra / max；本回合 read_thread 已核 inProgress / error=null。completed 时间留给原生生命周期。
工程根 D:/Unity/UnityProj/FightMatch；下文源码路径相对此根。完整原身份、逐项白名单、读取来源和验收见 [scope](demo-cont-a-design-scope.json)。

## 1. 已接收起点与设计检查

- §328 已接收 P2C、025 整体及 B17；34 功能 / 29 接收 / 余 5 / 51 正向交付保持。旧 C/R 完成回合不重启。
- §329 冻结 SHA256=abd1c0be0cd77ccd97c93d9633e16af512bd4d79dc1f165bded402f6baa45ae6；只允许新写本报告和 scope，R 另写设计审查。
- 2026-09-25T06:16:37.344815Z 按 P2C 导出重新逐文件核长度/SHA：810 中 807 不可变项全符；三协调稿最新身份单列，P2C 两 C 报告及唯一 R 报告全符。当前 722 实现 / 752 Assets / 398 GUID。
- 2026-09-25T06:17:09.232146Z 有限核 P2C 166 项及五组继承 116/75/1442/1442/1454 项，共 4695 个有限实物，长度/SHA 零差异；根和 manifest 另核。未递归扩大证据读取。
- P2C run008 编译及 run009 全量 exit0、3655/3655 Passed 属已接收历史。静态解析原 XML 确认具名多重集合，未运行测试、Unity、产品 DLL 或求参器。
- 采用 codebase-design 的共享 Module / Interface 原则：领域规则和 M02 队列各保留一个 Implementation；只增加需要的集合数据与内部测试 Seam。
- 本轮无源码、测试、工具、meta、资产、旧报告、协调稿或 Git 改动；两输出起初均不存在。不存在待用户重新批准的产品内容。

## 2. 实际缺口及唯一拥有者

| 实际位置 | 已核限制与必要处理 |
| --- | --- |
| Core/CandidateBusinessSnapshot.cs:11、CandidateCharacterState.cs:11 | 单 Character 与 OriginalSlot 混在角色值中；增加 M03 Roster 和独立三槽，不修改原实例 ID。 |
| Core/CandidateInventory.cs:45/147/234 | Create/Freeze/End 各有单人假设；一个全局库存、每角色负载、一次活动冻结。 |
| Core/CandidateProgression.cs:86、CandidateProgressionState.cs:27 | CheckEntry/Begin 及重开只存一个 Participant；改集合重载及历史集合。 |
| Application/CandidateLifecycleEntry.cs:33/52、CandidateLifecycleEnd.cs:35/62 | 入场不推进到期恢复；结束只认一个角色；须同一联合候选和按冻结集合分配。 |
| Core/CandidateApplicationProtocol.cs:181/196、CandidateApplicationReferences.cs:165/425 | 入场强制角色不变、单修订计数、初始化槽等于当前槽；不能仅换 UI 或放宽入口。 |
| Core/BattleEntryPreparer.cs:95、CandidateBattleOperations.cs:308/388、CandidateEnemyPhase.cs:183 | 准入、历史证明与贡献汇总限定一人。敌方真正执行循环已按原槽挑存活前排（EnemyPhase:29），复用该规则。 |
| Core/CandidateBaseRewards.cs:101/222、CandidateHistoryOperations.cs:299 | 奖励/报告及 PRD 历史只看第一个成员；按原参与者 key 查找并逐人调用同一评分核。 |
| Core/CandidateBusinessRestoreChecks.cs:70/179/253 | 把历史槽等同当前角色槽、单人携带/结束/战斗检查；逐历史引用核对，不能删检查放行。 |

M03 的 CandidateRosterState 是角色集合和当前阵容唯一权威：Characters 按 CharacterId 唯一、ClassId 唯一，每个值仍是原 CandidateCharacterState；Formation 是恰好三个可空 CharacterId（槽 0/1/2），每个非空 ID 必须已持有且只出现一次。持有集合可多于三，受预算限制；本包无获得/删除角色命令，无新职业规则。
FormationRevision 从 1 起，仅已提交阵容变化递增；每角色 StateRevision 继续属于成长/恢复。空槽及全空阵容合法；全空只使入场 NoReadyMember，不自动填人。
角色中的旧 OriginalSlot 留作旧初始资料，不再被集合入口解释为当前槽；旧字节、成长收据及建档锚不改。新当前槽只能取 Formation；M06/M05/M04 carry 中 OriginalSlot 是该次冻结的历史位置。
M04 只拥有一份 T/来源、按 CharacterId 的 Loadouts 和 ActiveCarry。v3 负载持久化角色引用，不独立持久化可改阵容；Actor 的旧 class/slot 投影由 M03 身份校验，不能作为换位来源。
M05 只拥有开放/挑战/首通和 Begin.Participants 历史事实；M06 原 EntryBaseline/快照拥有该次实际参与者、Stats、EntryHp、PRD 和三域初态。当前 M03 换位不得修改这些历史。
角色内容只取已核定义。现首包仍是原 W（Lv1/xp0/HP100/初始槽0）；槽1/2空。集合能力不等于增加法师/盾骑士、解锁事件或赠品。

## 3. Interface 与准入契约

新增方法位于原 PlayerSessionSystem partial，继续返回原 CandidateLifecyclePrepareResult / PreparedCandidateLifecycleRequest：
- QueryRoster() → PlayerRosterView：Characters、三个 Slots、FormationRevision、各角色当前修订/恢复事实、格式和不可用原因；只读，不推进时钟。
- PrepareFormation(PlayerFormationDraft, SaveCodecBudget)：输入 ExpectedCommitId、ExpectedFormationRevision、恰好三格选择；返回深冻结预览与稳定原 OperationId。
- PrepareFormationEntry(PlayerFormationEntryDraft, CandidateTimeSample, SaveCodecBudget)：输入明确 LevelId/Version、ExpectedCommitId、FormationRevision、选中角色修订及库存/进度修订；内容与 PlayerId 由已核 session 取，不接 UI 定义/参与者/随机字节。
- PrepareRosterMigration(string expectedCommitId, SaveCodecBudget)：只准备 schema2 → roster3 的明确事务；不在 Query/Open 中暗写。
- PrepareNewRosterProfile(PublishedContentCatalog, string releaseSetId, SaveCodecBudget)：显式新集合建档。原 PrepareNewProfile 保持 schema2/旧 F2 行为，兼容原调用及 3655 基线；029 新宿主改选新方法，旧宿主可建旧档后走迁移。

确认仍调用原 Lifecycle.Submit；查询/失败/未知续办仍调用 QueryOperation/Retry/Resolve/End 或原 ResumeObserved。没有第二个 runtime/写队列，也不给公开任意 builder PlayerSave 权限。
Prepare 只冻结目标、修订、显式时间和规范意图；不扣物品、不推进恢复、不加经验、不创建 Challenge/Attempt/Recovery 或提前返回业务 Completed。提交时原 runtime 先查同 OperationId/规范字节；已完成返回原 Commit/结果，同 ID 异意图 OperationConflict，新操作再核 head/版本。
阵容提交用“设置三个明确值”，不用 Toggle/自动补位；同样选择作为原操作的 Unchanged 结果保存，FormationRevision 不增加，结果仍可幂等查回。换位一次同时清原格与设目标格，不能中途重复。
提交时 M03 再核拥有者、重复与修订，产不可变 FormationReceipt（OperationId、前后修订、原/新三格）；M02 记录精确引用。UI 保留准备请求用于重试，不能重 Prepare 后冒充同次操作。

| 已核当前状态 | 阵容 / 新集合入场 / 迁移 |
| --- | --- |
| Active 已确认、Ready、无 ActiveHistory/Continuation/pending，roster3 | 阵容允许；入场按联合恢复候选；迁移已不适用，查原迁移结果。 |
| 活动战斗（含 AwaitLinks/AwaitRescue） | 新阵容/普通入场/迁移拒 ActiveAttemptConflict；原 Attack/Link/Rollback/Exit/Restart 继续。 |
| S17 / AwaitBaseSettlement | 仅原 ReservedOperationId 的 H06；先结算再考虑迁移/换位；不自动 Exit。 |
| SaveFailed/CommitUnknown/PendingPreparation 或磁盘未决组 | 原查询/Retry/Resolve/ResumeObserved/End 路由；新业务拒绝，不另造迁移或入场。 |
| Absent/CreateIntentRecorded/CreationConfirmationRequired | 只原建档与确认；未确认前无阵容/入场/迁移。 |
| 某选中成员恢复中 | 可换位/取消选择且恢复事实不变；H02 只取同次时间核对后 Ready 的已选成员；其他已持有者不补位。 |
| schema2 Ready 且无上述阻断 | 旧查询继续；新集合操作给 RosterMigrationRequired；可明确提交迁移，不清档。 |

旧只读 Character、Inventory.Actor/Loadout、Progression.Participant、Lookup.CharacterEnd/CharacterExperience 和 Growth 单值入口在恰好一项时保留原值；新增集合属性与 TryGetSingle。多项访问旧单值时明确 AmbiguousCharacter，不能返回第一项；本来表示“无结果”的 null 语义仍保留。
旧 Character.OriginalSlot 仍可读原初始值，QueryRoster.Slots 才是当前位置。旧按 ID 的成长/恢复/装配请求可精确定位角色；旧 EnterAttempt(kind2) 只供旧格式办理/旧记录重查，不能用它进入 roster3 后默选一个人。
CandidateDemoView 新增按 CharacterId 的偏好/攻击条件查询；构造视图的全局可用性不调用有歧义的单值 getter。现 CandidateBoardInputController 的两处偏好读取改用 SelectedCharacterId，仅作原输入链的必要消费适配。
CONT-B 将消费 Roster.Characters、角色修订、Formation 与 M04 Loadouts/全局 T/L/R/F；它自己的永久成本/教学事务以后另签。CONT-C 消费 QueryRoster、准备结果、已提交 Commit/原因；页面不保存另一份阵容权威，不实施页面或导航。

## 4. 同一办理核中的集合行为

1. M03 初始化集合/准备入场复用 CandidateCharacterGrowth；不复制成长公式。当前支持 ClassKind.Warrior、空主动技能、零闪避及原参数能力；其他职业/未支持效果明确 UnsupportedBinding。
2. PublishedSaveContext 增 Growths 及按 ClassId 精确选择的 Prepare 重载，旧 Prepare(growth,...) 自动成单元素；旧 Growth getter 多值明确拒绝。M10 仍供应已验证的一个 W 定义，不改源格式或发布器。
3. M04 的 F=T−ΣL−ΣR；每角色最多一个 class-compatible ItemId，L/C≤99，L=0 保留选择。选中恢复者/未选者的 L 不借给参与者，未参者负载和偏好不变。
4. Freeze 取同一 M03 准备结果，逐角色把自己的 L 和合法 F 转成 C/R；ClassId 唯一且每战术物品只有一个 EquipClassId，不引入共享竞争的新排序规则。若未知内容不满足这项前提则拒绝。
5. M05 新 CheckEntry/Begin 重载消费准备后的 1..3 位参与者及实际槽/修订；历史 Begin.Participants 在重开中完整保留。旧标量重载与候选单人拒绝继续。
6. M06 正式 PreparePublished 允许 1..3 个合法 Ready 成员，全部来自准备结果、按槽稳定排序；Candidate PrepareCandidate 的原单人边界保留。人物身份、PRD、贡献、直接伤害核对按 CombatantKey 查找，不能把 [0] 替换为不校验的循环。
7. 现战斗能力仍是 EmptyCarry/无物品使用；M04 非空携带正例可独立验证，联合 H02 遇 NonEmpty 必须整体拒绝，不能先冻结/消耗再进场或忽略物品。补战术效果不是本包。已批准首包没有该物品。
8. 正常结束仅按原实际参与者处理。M07 逐人用同一 ExactScoreCalculator、原入场等级和该人的有效伤害/承伤计算固定整数；不均分池、不按末击，0贡献沿已核 CurveIntercept。材料/首通只固定一次。
9. M03 逐参与者应用其固定 XP，再根据最终 HP 为真正倒下者创建 RecoveryId；活着者无恢复，未参与者无本次 XP/End/新恢复。索引须是 (CharacterId,SettlementId) 与 (CharacterId,EndReceiptId)，同一次结束共享 EndId 合法。
10. M04 End 按原 carry 行关联 U：胜利扣 C−U 并返 U→L，退出返 C，重开原 R/C 整体转新 Attempt；非参与负载不变，普通材料奖励只入账一遍。正式 EmptyCarry 的 Remaining 为空；隔离库存正例检验非空行。
11. Restart 原 EntryBaselineId、原参与集合/槽/Stats/EntryHp/原三域 InitState/InitSequence 保持，只建合法新 Attempt；不纳入后来恢复者、不按现等级重算、不采新熵。正常新 Attempt 才由原 RNG 来源生成 48 字节一次材料。
12. 新 ApplicationTestAccess.cs 只授既有 FightMatch.Core.Tests 一个内部纯 builder Seam：fixture 和实际 Submit 调同一构造逻辑，结果仍 CommitEligible=false；不开放公开 PlayerSave builder，不伪造已验证 session。

联合恢复 H02 的确定顺序：
- 在同一个已核 head/队列准入中消费准备请求已深冻结的一个 CandidateTimeSample；宿主沿 LocalPlayerClock 采样，可信范围仍 DeviceUntrusted，不在重试/解码中再读时钟。
- 对原三格选中且有活动恢复期的成员调用原 CandidateRecoveryClock.Advance；同域单调、跨域 UTC、倒退/重复样本语义不改。记录每个 (CharacterId,RecoveryId) 的原/后修订、原样本与结果，不先发布。
- 按候选 Ready 状态选参与者，算当前 Stats/满血入场，M05 核关卡，M04 准备同一集合携带；无人可用或任一能力/预算失败则全部拒绝，恢复也不单独落盘。
- 准入通过后才创建 Challenge/Attempt/Baseline 和一次熵，形成 M03 恢复 + M04 冻结 + M05 Begin + M06 History + M02 Result 的一个候选；只调用一次原 M12 Prepare/Write/Complete。
- M02 Transition/OwnerHistory 必须先记联合 RecoveryResults，再核 Begin 中恢复后的修订；未到期者可有自然时间进展，但没有参与/经验/结束效果。不能沿旧“Enter 时 Character 引用不变”断言。
- SaveFailed/CommitUnknown 保留完整原候选、时间、ID、熵及 ExpectedHead；只原 Retry/Resolve 或磁盘 ResumeObserved。新视图/参与集合仅在 Complete 并恢复核验后发布。

## 5. 精确版本与序列选择

| 对象 | 旧兼容 | 新选择 |
| --- | --- | --- |
| CandidateValidation | 六切片 schema1、FMINT001/version1、旧指纹与拒绝边界原字节保持 | 本包不新增候选集合保存格式。 |
| PlayerSave 六切片目录 | [2,2,2,2,2,2] 读写/规范回写保持 | 依 M02/M03/M04/M05/M06/M07 顺序为 [3,3,3,3,2,2]；只接受这两组明确向量。 |
| Body header | FMBIZ001 + 原 schema1/2 | magic 保持，前四 owner 明确 schema3，M06/M07 仍2；不能按“最新”解码。 |
| M03 | 原单 Character | Characters 有界规范列表 + 恰好3槽 + FormationRevision + FormationReceipts；角色子记录复用，OriginalSlot 为旧初始事实。 |
| M04 | 原 actor/loadout | 按 CharacterId 的负载引用列表，T/来源、冻结与结束收据原含义；解码精确核 M03 身份，不另存当前阵容。 |
| M05 | 原单 Participant | Participants 列表 + 可空 FormationRevision；迁入旧 Begin 保留原成员/槽/修订，旧历史 formation revision 为 absent，不伪造新事实。 |
| M06/M07 | 当前列表 wire 与指纹算法 | 不改字节布局/版本；仅扩正式合法集合的校验及逐角色评分，历史原指纹保持。 |
| M02 | 原记录、结果及 S17 | schema3 可保存原 FMINT002 字节与新 FMINT003，新增迁移/阵容结果和 H02 RecoveryResults；完整 records/index/continuation 保留。 |
| Requirements | fm.player.application.v1 | 保留旧 feature，另需 fm.player.roster.v1；所有当前、历史、恢复定义及物理根的精确绑定继续闭包。 |
| growth 定义引用 | 旧 role=growth/version1 的单定义 | v3 明确附 ClassId subject key，按同包已解析 Growths 唯一查找；内容/规则/数值/随机版本不改。 |
| 新规范 intent | FMINT001/002 原 bytes/version 不变 | FMINT003/version3，仅正式：InitializeProfile(kind1) 集合初态、MigrateRoster(10)、SetFormation(11)、EnterFormation(12)。原 Attack/End 等 v2 意图可在 v3 head 办理。 |
| F2 创建记录 | FMPROF01 / record1 / intent2 完整兼容 | FMPROF02 / record2 / intent3；仍绑定相同已批准 new-profile:default/version1 的规范298字节，正式初态仍只有原 W。 |
| Active locator / M12 envelope | FMAP01 / 原 descriptor、marker、head | 不变；Active 仍锚原 Initialize commit，不是当前头。无需改 Platform/asmdef/首包。 |

新 v3 Initialize 载荷为角色初态列表（ID/ClassId/L/x/旧初始槽）与三个显式槽；每项按对应已核 Growth 定义创建。PlayerSession 只从真实 NewProfileDefinition 生成单 W 列表，不提供 UI 自填角色入口。
CandidateApplicationIntentInput 新增可空 uint FormatVersion；省略仍按候选1/正式2，显式3仅接受本表四种载荷。PreparedIntent.FormatVersion 从原tag冻结；旧初始化字段与新 RosterInitialize 字段互斥，不按软件当前版本选择。
MigrateRoster 载荷固定 ExpectedCommitId/SourceGeneration/SourceDescriptorLength/SourceDescriptorSha256、from=2/to=3；SetFormation 固定原阵容修订及目标三格；EnterFormation 固定目标版本、原三格/修订、各选中角色的前修订、库存/进度修订及 TimeSample。
v3 新输入在分配前做整个 wire 预算；向量排序、可空槽、字符串 Unicode、整数和列表逐项有界并深复制。unknown tag/schema/feature、乱序/重复、坏引用、错 Player/内容、漏必需项或规范回写不等均拒绝，原文件保全。
旧 intent 的格式必须从原 tag 保留到 frozen Data；ReadFrozenPublishedInitialize/RestoreProfile 的重编码必须选该原格式，禁止把 FMINT002 自动升级成003导致 F2 冲突。
新格式初态及迁移是不同事务：新 PrepareNewRosterProfile 直接产生 v3 Initialize；旧 PrepareNewProfile 仍产生 all-2 Initialize，若随后需要集合，明确办理 MigrateRoster。QueryRoster 的旧档三槽投影只能标 IsMaterialized=false，不能声称已迁移。

## 6. 旧档全状态与 F2 迁移路由

| 原状态 | 执行路由与迁移时点 |
| --- | --- |
| schema2、Active、无活动/未决 | ResolveExact 完整闭包并 DecodePublished all-2；准备 MigrateRoster，原 M02 队列核 ExpectedHead，一次新 Commit 保存 v3 与 MigrationReceipt。 |
| schema2 活动战斗/回退历史 | Open 恢复原 all-2，继续既有 Battle/Lifecycle 方法和旧格式写出；保持原参与者/随机/ID。结束后再迁移，不强退、不重采入场。 |
| schema2 S17 | 保留原 Continuation/ReservedOperationId/FinalReport，原 PrepareVictory→H06，先保存原合法 all-2 结果；之后新迁移。 |
| 内存 SaveFailed/CommitUnknown | 优先原 pending ticket/候选 Retry/Resolve；不得创建迁移。完成或明确 End 后重新查询实际头才允许准备迁移。 |
| 磁盘旧未知候选 | 原 ReadUncommittedCandidate/DecodePublished all-2、ResumeObserved/Resolve 或 EndObserved；原 canonical intent/候选字节/OperationId 一字不换。多组沿原显式选择。 |
| 迁移前崩溃/迁移候选未提交 | 原旧头仍有效；若已有 migration candidate，ResumeObserved 精确读 v3 候选并用原 intent/ticket 续办；没有候选才可重新准备技术请求。 |
| migration marker 已写、确认未知 | 先核实际 head/原操作，恢复新 v3 并返回原 migration Commit；不能再追加第二次迁移或降回 all-2。 |
| v3 当前头 + all-2 物理前代 | 每根分别用其明确版本解码，任一必需旧包/根缺失、损坏或能力不足都 RecoveryBlocked；不能只验证新头或 fallback 最新包。 |
| 历史/收据/回退保留根 | 原 Intent bytes、Records 顺序/Generation/CommitIndex、M06/M07 指纹、固定奖励、恢复样本保持；迁移只增一条记录和新 shape。 |
| F2 CreateIntentRecorded(record1) | Restore 原 FMINT002 及完整配方 bytes/PlayerId/op；原 Initialize all-2→原锚证明→Active；确认后才可迁移。 |
| F2 原 Initialize 已提交但未确认 | 找原 generation1/CommitIndex[0]/records[0]，确认原 locator；不能替换 Initialize。Active 之后按最新已核头办理迁移。 |
| F2 Active(record1) | 读最新 all-2 或 v3 头，Lookup 原 Initialize，继续核原锚；换槽/升级不改变 locator 指向。 |
| F2 新 record2 | 以原 FMINT003/集合初态续办相同四态；record/intent 版本交叉不配、材料/配方/身份错即拒绝。 |

MigrateRoster 的前置必须是已确认 Active、Ready、无 ActiveHistory/Continuation/内存或磁盘未决；恢复中的角色可迁移，保持全部 elapsed/样本/RecoveryId，迁移不读时钟。
迁移只包装原角色为同一实例集合，在其原 OriginalSlot 放同一 ID、其余格空，FormationRevision=1/空阵容收据；M04/M05 旧单值改可逆单元素表示。旧角色/库存/进度/奖励修订均不增加，唯 M02 generation/index 增一。
MigrationReceipt 固定来源 descriptor digest、commit/generation 及2→3；提交前核新旧业务的可逆单元素投影完全相同。恢复核 records 位置/原父 commit/迁移唯一次及历史覆盖；不要求每个古老物理祖先永远存在，但现 M12 声明保留的每个根必须完整。
M02 角色/经验/恢复引用用精确角色键；旧 Initialize.OriginalSlot 只核原初始化角色事实，不能拿现在 Formation 的槽比较。历史 Begin/Carry/Entry 槽与彼此一致，不能要求等于当前阵容。
所有旧记录可再次 Lookup；同 op 原字节返回原结果；同 op 改字节拒绝。Retain、ReplaceHistory、Complete 必须带格式与整个 Roster，不能调用旧单 Character 构造后丢成员。
能力由宿主传现有 SaveRecoveryCapabilities：ReadableSlices 明确列旧六个schema2及前四个schema3（共10个去重contract），新feature在M02与集合切片Requirements中列明；Entry.RequiredFeatures仍为空，不伪造新战斗效果标记。缺任何被根引用能力即拒；不改 M12 算法，也不把代码版本名当已可恢复的凭据。

## 7. 建议逐文件实施范围（待 SD00 另签）

仅下表 37 份旧生产 .cs、一个旧工具、6 新生产 .cs、5 新测试 .cs 及11个自然 meta。旧测试一律不改；没有全目录授权。
scope.proposedImplementation.modifyExisting 给每项当前字节数、完整 SHA、物理行数及理由；所有身份取当前 P2C 合法实物，不沿旧设计的过时 hash 回滚。

| 旧文件 | 唯一修改理由 |
| --- | --- |
| Core/CandidateBusinessSnapshot.cs | 增加格式标识与 M03 Roster；旧 Character 单值投影保留；新构造/复制保持集合格式，不掉回旧构造。 |
| Core/CandidateInventoryState.cs | M04 Loadouts 按 CharacterId；旧 Actor/Loadout 仅单值投影；Carry 的参与位置只来自冻结 M03 入场资料。 |
| Core/CandidateInventory.cs | 集合重载与按角色 Equip/Preference；ΣL/ΣR、冻结和结束逐角色；旧单 actor CreateCandidate 行为保持。 |
| Core/CandidateInventoryDefinition.cs | 逐 loadout/actor/冻结行预算与引用检查；无新物品或职业枚举能力。 |
| Core/CandidateProgressionState.cs | Begin/EntryCheck 增 Participants 与可空历史 FormationRevision；旧 Participant 投影仅 count=1。 |
| Core/CandidateProgression.cs | 集合 CheckEntry/Begin 与 Restart 原集合；旧标量重载保留。 |
| Core/CandidateProgressionRequests.cs | 新增仅含关卡/绑定及准备后参与集合引用的请求 DTO，旧请求形状不变。 |
| Core/CandidateProgressionDefinition.cs | 参与集合的修订/槽预算检查，保持原开放与首通规则。 |
| Core/CandidateBusinessSaveCodec.cs | 双正式布局分派、M03 集合 Requirements、五切片版本向量与格式保持；候选 schema1 不变。 |
| Core/CandidateBusinessSaveValues.cs | 显式切片 SchemaVersion 和 v3 growth subject key；旧 FMBIZ001/schema1/2 字节不变。 |
| Core/CandidatePermanentSaveCodec.cs | M03 单字符子记录复用；M04 v3 按角色引用解码；M05 v3 参与列表，保留旧单值读写。 |
| Core/CandidateBusinessRestoreChecks.cs | 按 CharacterId/历史 slot 核全部角色、负载、参与/经验/恢复；消除把历史槽等同当前阵容的假设。 |
| Core/CandidateApplicationIntent.cs | 显式 FMINT003 数据/格式标识、初始化集合与 kinds 10/11/12；原 kinds 1..9 数据保留。 |
| Core/CandidateApplicationIntentCodec.cs | 按原 tag/version 分派，v3 有限规范向量及 TimeSample；读旧 FrozenInitialize 后按原格式重编码。 |
| Core/CandidateApplicationProtocol.cs | 新格式初态/迁移/阵容/联合入场分派；Retain 传整个 Roster；单人旧分支继续原规则。 |
| Core/CandidateApplicationRecords.cs | 增加初始化集合、阵容/迁移结果、联合恢复列表及角色结束/经验列表；旧单值属性明确拒多值。 |
| Core/CandidateApplicationReferences.cs | (CharacterId,EndReceiptId/SettlementId) 索引与逐角色 OwnerHistory；原 intent 字节、历史槽和 F2 锚独立核对。 |
| Core/CandidateApplicationSaveCodec.cs | 六切片 [3,3,3,3,2,2]、M02 v3 结果、v1/v2/v3 intent 共存与新 capability；旧 all-2 保持。 |
| Core/PlayerProfileCreateRecord.cs | 新增 FMPROF02/record2/intent3 的显式解析与冻结；FMPROF01/record1/intent2 和原初始化锚完整保留。 |
| Core/PublishedSaveContext.cs | 新增只读 Growths 与按 ClassId 精确选择的 Prepare 重载；旧 Growth 单值投影；不代表发布授权。 |
| Core/BattleEntryPreparer.cs | 仅正式 PreparePublished 支持 1..3 合法 ready 成员，保留 Candidate 单人拒绝与 EmptyCarry/无技能限制。 |
| Core/CandidateBattleOperations.cs | 以 Actor key/成员集合核直接伤害、PRD、快照和贡献累计；保留原规则、三域及单人拒绝分支。 |
| Core/CandidateEnemyPhase.cs | 把单人验证改为按成员/PRD/Actor key；执行已有按原槽寻找存活前排的同一循环。 |
| Core/CandidateHistoryOperations.cs | 恢复 PRD 资料按原参与者 key 找 Crit，取消用第一个成员证明所有行。 |
| Core/CandidateBaseRewards.cs | 同一 ExactScoreCalculator 逐原参与者固定经验；报告/终态/贡献按 key 检查，材料仍只发一次。 |
| Application/CandidateApplicationModels.cs | BuildResult 冻结/复制新 M02 结果列表，预算与不可变性保持。 |
| Application/CandidateBattleApplicationBuilders.cs | 按请求 Actor 读偏好；ReplaceHistory 保持 Roster 和格式，不触碰永久资料。 |
| Application/CandidateDemoView.cs | 集合负载的 EmptyCarry 准入和按角色偏好查询；旧单值属性有明确多值错误。 |
| Application/CandidateLifecycleApplicationSystem.cs | 可信 Submit 分派新 kinds、原操作优先、M03 上下文集合核验及 shared internal builder seam。 |
| Application/CandidateLifecyclePreparation.cs | 原 Initialize/End/Recovery 准备保持；新集合数据深冻结和 F2 恢复按原 intent 版本。 |
| Application/CandidateLifecycleModels.cs | 集合内容输入/只读内容及新 draft 载荷，不允许 UI 替换正式 session 的定义。 |
| Application/CandidateLifecycleEntry.cs | Initialize 按原格式分派；旧 Enter 仍供 schema2 续办，集合 H02 使用新增同系统 partial。 |
| Application/CandidateLifecycleEnd.cs | 按冻结参与者分配经验/恢复、Restart 原集合；AdvanceRecovery 精确角色；供同一内部 builder 调用。 |
| Application/CandidateLifecycleView.cs | Roster/Formation/Characters/格式状态只读，旧 Character 单人兼容。 |
| Application/PlayerSessionSystem.cs | partial + 新档显式格式/恢复版本分派；从已核根取 Context，保留旧 PrepareNewProfile 和 F2 四态。 |
| Application/PlayerSessionModels.cs | 新阵容/集合入场 draft 与只读预览值，输入仅目标/预期版本/显式时间。 |
| Presentation/CandidateBoardInputController.cs | 仅两处标量偏好读取改用 SelectedCharacterId 的新只读查询，保持原手势/播放/页面行为；集合不能读取第一个角色偏好。 |
| 必须逐项新增的文件 | 内容/行预算上限 |
| --- | --- |
| Assets/Scripts/FightMatch/Core/CandidateRosterState.cs | M03 唯一角色集合/三槽/阵容修订及变更收据；纯准备/选择/到期恢复候选，复用角色成长/时钟。 ≤240行 |
| Assets/Scripts/FightMatch/Core/CandidateRosterSaveCodec.cs | M03 roster wrapper 与 v3 规范集合编码；复用旧角色子记录和 M04/M05 既有编码方法。 ≤150行 |
| Assets/Scripts/FightMatch/Core/CandidateRosterProtocol.cs | M02 新 kinds 的严格 transition/历史结果核验，联合恢复与迁移/阵容不变量。 ≤220行 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleRoster.cs | 现有 lifecycle partial 的 InitializeRoster/Migrate/SetFormation/EnterFormation 共用办理；旧 Finish 接同一集合。 ≤220行 |
| Assets/Scripts/FightMatch/Application/PlayerRosterSession.cs | 现有 PlayerSession partial 的正式查询/准备方法和明确迁移路由，复用原 Submit/Retry/Resolve。 ≤169行 |
| Assets/Scripts/FightMatch/Application/ApplicationTestAccess.cs | 仅 InternalsVisibleTo("FightMatch.Core.Tests")，隔离 fixture 调同一内部纯 builder，不开放运行时写门。 ≤1行 |
| Assets/Tests/EditMode/FightMatch/CandidateRosterTests.cs | 三槽/身份/重复/只读/恢复保位与多角色库存/进度纯领域。 ≤340行 |
| Assets/Tests/EditMode/FightMatch/CandidateRosterBattleTests.cs | 隔离多角色同一 builder→Core battle→奖励/结束/重开，按 key 和原槽核贡献/PRD。 ≤460行 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterSessionTests.cs | 真实首包 W 三槽、门控/幂等、到期恢复+H02 和保存失败/未知。 ≤440行 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterMigrationTests.cs | 旧 all-2 各状态、原 F2、v3 闭包/缺包/错引用/中断恢复与字节兼容。 ≤700行 |
| Assets/Tests/EditMode/FightMatch/PlayerRosterTestData.cs | 基于既有公开纯构造的隔离集合 genesis/内存 storage/fault helper，无反射造内部成功或假发布凭证。 ≤260行 |

上述11个 .cs 各有且仅有同路径加 .meta 的自然生成文件（scope 逐项展开），不手造 GUID、不新增目录 meta。ApplicationTestAccess 是程序集属性文件，不是 asmdef 或依赖变更。
生产建议总增删≤3600物理行：旧文件规划≤2600、新文件≤1000；新增测试≤2200（含helper）。这些是上限分配，不是已测 diff；实际超额须由 SD00 精确补签，禁止压缩/删校验凑预算。
仅 Tools/Invoke-FM025P2Validation.ps1 可建议≤60增删：增加固定 Stage=CONT-A，固定 TestArtifacts/FMDemoCONT/cont-a，Mode仅Compile/Tests；用已签新scope的精确路径并纳入P2C完整基线、3655及36DLL，原六Stage/发布行为不变。
未来实现集合=722+22=744，Assets=752+22=774，GUID=398+11=409；37旧生产与1工具允许变化，旧 .meta/首包/配置/asmdef/旧测试保持。新 C 交付报告为 demo-cont-a-delivery.md / demo-cont-a-scope.json，R另写 demo-cont-a-code-review.md，均位于本报告目录。
未来验证根只是本设计建议，当前不得创建。SD00源码包须另固定根身份/入口副本/有限导出/预算；不得把未来 scope 自行当新读写权限。
本方案不新增 M10 内容源格式、职业实现、物品战斗效果、H03永久成本、UI页面、第二战斗核、场景、依赖、构建命令或广告/云/Android能力。

## 8. 必须给出的未来验证

| ID | 见证与否证 |
| --- | --- |
| CA01 | 真实已批准首包原 W 依次移入槽0/1/2，回读相同实例；空格/全空合法，重复/未知 ID/错误 Class 引用拒绝且无提交。 |
| CA02 | 选中恢复者换位/取消后恢复依据与负载保持；显示倒计时不写；没有未选者自动入场。 |
| CA03 | 隔离合法集合 fixture：不同 CharacterId/ClassId，均只复用已支持 Warrior 公式；真初始化→规范v3→同一纯builder，禁止反射改内部成功状态或伪造发布凭证。 |
| CA04 | M04 多角色 T/ΣL/ΣR/F、独立装配/偏好、恢复者/未选者 L 不借；Freeze/End/Restart 行准确；非空 carry 联合 H02 整体拒绝无半状态。 |
| CA05 | 多角色 M05/Entry/正常攻击/敌方按槽/倒下/跨行动 PRD、贡献逐 key；故意打乱数组顺序、缺/重 PRD 或引用拒绝，不能拿首元素顶替。 |
| CA06 | 多角色真实整关报告→逐人评分 XP→M03/物料/首通同一结果；材料只一次、未参者零参与收益，重复 H06 新增收益0。 |
| CA07 | 同一 ExplicitTimeSample 到期恢复+H02 单次提交；一个未到期仍留槽，至少另一个Ready可进；全不Ready拒绝，恢复不先写。 |
| CA08 | H02 在 Prepare/Write/Complete 代表性失败或CommitUnknown，原head/候选 bytes/op/time/熵/ID精确；Retry/Resolve/重建对象Resume一次生效。 |
| CA09 | 活动/S17/pending/未确认建档/过期head/revision各门与原操作优先；同ID异意图拒；新阵容不追改历史。 |
| CA10 | Restart 逐项比原参与集合/位置/Stats/HP/三域初态；后来恢复者不加入，正常新入场取新熵。 |
| CA11 | schema2 idle/活动/回退/S17/历史XP/恢复各档、原物理前代和未知候选完整原路续办；checkpoint后迁移保全Player/W/包/收据/索引/修订。 |
| CA12 | 迁移中断于候选/写后未知/完成后失联；原技术op/字节续办，不双迁移、不自动回退或清档；旧操作在新头仍Lookup。 |
| CA13 | F2 record1 各未完成点按原 FMINT002完成；Active恢复最新头；新record2初态仍真实单W/3槽，原锚、配方298字节和版本混配负例。 |
| CA14 | v3 M03/M04/M05集合与六切片往返；错角色/重复槽、漏旧精确包/根、错误Player/包、未知schema/feature、低预算/不可变性全部拒绝并保全实物。 |
| CA15 | 新域的实例集合用纯 Core 已解析定义闭包/同一 Application内部builder 验证；其结果不冒称经真实 M10 发表或正式 session 额外赠角色。真实 session证据仍用批准W。 |
| CA16 | 原候选schema1、旧正式schema2、旧F2规范bytes和原拒绝保持；旧3655具名测试/断言不修改、跳过或删例，新增例另列。 |
| CA17 | 原C串行Unity2022.3.18f1 Compile及无filter全量EditMode，退出/XML/零skip/源及36DLL同版、37旧文件实际增删与自然meta证据齐全。 |
| CA18 | 缺能力不得被宽松集合解析变成成功；没有第二份角色/阵容/库存权威或公开任意builder获得PlayerSave权限。 |

多角色 fixture 的 ClassId 是隔离定义身份，各角色仍执行已支持 Warrior 规则；它证明集合算法，不声称新增实际职业或发布内容。Core PublishedRuleDefinitions 是解析闭包 DTO，不是发表回执；正式权限仍只来自 PlayerSession 已核 catalog/createRecord。
新测试的初态/战斗/报告都从现有及本包正式构造/校验函数产生；应用内部 Seam 只允许调用同一纯构造函数，不直接设置 runtime/pending/已核 profile 或插造 Canonical Record。
验证时既有 Windows probe 只沿已接收有限只读范围继承；不新建物理profile根或强杀矩阵。若必须新物理测试，先向SD00给精确技术路径/文件/字节计划，不自动扩文件。
本设计已完成静态接口/范围/版本一致性核查，Unity/测试/产品/物理输入/Player/Android均 NOT RUN（§329禁止）。旧有效运行不重复；新实现完成后源码已变，才需上述新验证。
风险集中在单人历史校验改集合、迁移与 F2 的原字节选择、预算实际增删；都有明确文件和否证用例。非空物品战斗/新职业仍拒绝，真实宿主与物理可靠性留029，不能外推首Demo或Android完成。

## 9. 交付门

作者只交 DESIGN_COMPLETED、准确turn和两文件外部长度/SHA，停止修改。scope 绑定本设计报告，scope自身摘要只放formal final，避免自哈希环。
R先核本次原生completed + formal + 两实物，再独立给唯一 ACCEPT/NEEDS_FIX/REJECT；本报告未占用R审查文件。
只有SD00接收独立设计结论并另签逐文件源码/格式/验证范围后才实施；当前没有源码开工权限，也没有新用户待答。
