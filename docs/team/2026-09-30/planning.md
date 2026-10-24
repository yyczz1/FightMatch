# FightMatch 主策线：首 Demo 体验与内容交接

2026-09-30 · PLAN-001 · 主策负责人：thread `01a0f2e3-2c47-7273-88da-20513bcbfaf4` / host `local`

**当前交付（2026-10-22）：** 当前包仍为 android-11；[参考文本修正回执](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/reference-text-run01/result.json)已收尾，一次应用／构建／保档安装成功，未运行测试。双语参考正文与两步骤已观察，但英文差异小标题完整字形、简中步骤选择说明全文仍有截图边界；未点击步骤或追加行动／奖励，原档18份可读业务SHA保持。既有导航／六弹窗、26XP／2锡片和三态恢复证据按原范围复用；七路径已于10月22日发布PR22 head `4e78fdecc4bb3cbe579d7d13a166b507131d06fc`，[同head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6339540133)未发现重大问题，QA已收源码门，PR仍draft未合并、非formal APPROVED。clean树未运行，整体仍 `NEEDS_FIX`；下载控制器新增四文件／16项测试是另一个未发布未接UI的包，不记成APK能力。Skip因果、无偏好系统首开、历史退出及硬件边界不改绿；中央已请求USB连接iQOO，真机验收仍待。包身份见[试玩指南](../../demo/PLAY_FIRST_DEMO.md)，保护与预算仅引用[主程记录](engineering.md)及原回执；主程接收本次差量，中央仅收完成短回执，不新增运行。

## 1. 本轮职责与边界

本轮目标是把已经批准的首 Demo 玩法整理成可直接供主程、主美和主测试执行的页面、文本与验收输入。固定源码起点为 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；用户在中央会话恢复后续开发并要求按五角色组织推进，授权出处见本目录 `README.md` 与 `dispatch-001.md`。

本文件由主策会话唯一维护。主策不写产品代码、不运行 Unity、不操作 Git，也不修改中央、主程、主美或主测试的文档。当前工作树已有中央维护的 `START_HERE.md`、`.agent/PROJECT_CONTEXT.md` 与 `docs/team/` 变更，均保留。

### 主策子任务登记

| 任务 | 交付负责人 | 状态 | 唯一写入路径 | 输入与返回 |
| --- | --- | --- | --- | --- |
| COPY-001 英／简中文案制作稿 | 协作子会话 `/root/copy_001b`（原 `/root/copy_001` 超时停用，未写文件）；AMEND-01／02 由主策维护 | `ACCEPT` | `docs/team/2026-09-30/planning-localization-draft.csv` | 当前 216 个稳定键；主策独立复核 CSV、唯一键、字段、severity 与双语参数集合通过；不接产品源码或 Unity 资源 |

`planning-localization-draft.csv` 是供策划、主程和 QA 评审的制作稿，不是运行时权威表，也不是已发布内容。正式 Excel/CSV 表结构、Luban 版本、生成目录和项目内绑定由主程在技术设计与实施包中冻结；在此之前不得复制成第二套手工 JSON 或 Unity 内独立编辑文本。

COPY-001 经一次 `NEEDS_FIX` 将玩家可见的内部“候选 / Candidate”改为“选中的原保存 / Selected Original Save”，随后主策给出唯一 `ACCEPT`。接受文件 SHA256 为 `fc2429d2cd1495a0a29c805a2ce202316a61f692a64debd7c46130b1f8c68ee5`；准确 9 列、170 行数据、170 个唯一非空键，允许 `parameters` 为空，其余必填列非空，severity 仅 `blocking/error/warning/info`，每行简中、英语与分号参数列的 `{parameter}` 集合一致。主测试随后独立只读复核同一 SHA 与 37867 字节，计数 `info 107 / warning 47 / blocking 13 / error 3`、`issue_count=0`；该结论只接收制作稿静态质量，不证明 Luban 生成、运行时绑定或页面显示。170 行超过执行时建议的约 70～110 行，但仍全部属于本任务列出的同一页面／条件界面，没有扩入账号、广告或下载管理完整页面，因此按同一连贯交付接收。

**COPY-001-AMEND-01（2026-10-01）：** 以以上接受文件为固定输入，仅在 `fm.language.saved` 后新增 `fm.language.save_failed`。该次输出 CSV SHA256 为 `c4fc583344392153e97a096b68f5d740d8ea161d1bada87bae42343b20b6fb4c`，38308 字节、准确 9 列、171 行数据／171 个唯一键；移除新增行后与原 170 行逐行内容及顺序完全一致。新增行参数精确为 `errorCode`、severity 为 `error`，简中与英语均表达“本次会话切换已生效，但偏好未保存，可重试”，不借用业务存档恢复。全表必填列、severity 和三方参数集合复核 `issues=[]`；这仍是制作稿，不证明正式表、运行时保存或错误展示已接入。

**COPY-001-AMEND-02-FIX-01（2026-10-01）：** 以 AMEND-01 的 171 行文件（SHA256 `c4fc583344392153e97a096b68f5d740d8ea161d1bada87bae42343b20b6fb4c`）和 `engineering-system-001.md` §§20～22 为固定输入，补 45 个当前 L1 动态显示键。初次 46 行候选经独立 R 给出 `NEEDS_FIX` 后，只在新增区撤下不可达的恢复倒计时键、移除历史快照的待补路线参数、将英文内部术语 Operation 改为玩家行动，并修正本文件参考差异键前缀。当前 CSV SHA256 为 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`，50990 字节、217 个物理行、准确 9 列、216 行数据／216 个唯一键。新增行集中追加在原文件末尾；取前 172 个物理行（表头＋原 171 行）所得 SHA256 仍为上述 AMEND-01 哈希，证明移除本次 45 行后原字节与顺序不变。全表必填列、severity、参数声明排序及简中／英语／声明三方参数集合复核 `issues=[]`。

AMEND-02 只解决当前 L1 实际动态值，不改变玩法或发布内容。审计依据是 `demo-r1.source.json`：唯一角色 `W`，唯一敌人定义 `enemy:clockwork-infantry`、实例 `A/B`，物品 `item:tin/item:wood`，唯一关卡 `level:ch01-01` version 1、单面 `F1`；战士攻击 20，两敌各 15 HP／物防 0，因此每次直接命中都击倒目标并锁定该路线，当前 L1 不会自然进入补线、非致命临时路线、换面、蓄力或全员倒下恢复。代码枚举仍是业务输入，不能直接成为玩家文本。

#### AMEND-02 稳定值映射

| 稳定业务值 | 精确文本键或组合 | 约束 |
| --- | --- | --- |
| `CharacterId=W` | `fm.name.character.w` | 显示“战士／Warrior”；任何列表、HUD、历史、参考、结果均先解析此键。 |
| `EnemyDefinitionId=enemy:clockwork-infantry` | `fm.name.enemy.clockwork_infantry` | 显示“发条步兵／Clockwork Infantry”。 |
| `EnemyInstanceKey=A/B` | 上述敌人名＋`fm.battle.enemy.numbered_name`，`enemyNumber` 精确为 `A→1`、`B→2` | 显示序号来自冻结映射／`OriginalSlot+1`，不得解析 A/B 字符或显示 PairId。 |
| `ItemId=item:tin` | `fm.name.item.tin` | 显示“锡片／Tin Sheet”。 |
| `ItemId=item:wood` | `fm.name.item.wood` | 显示“木材／Wood”；数量 0 时不凭空产生奖励行。 |
| `LevelId=level:ch01-01`, `LevelVersion=1` | `fm.name.level.ch01_01` | 显示“第 1 关／Level 1”；当前没有获批的单关专名，不把“发条庭院”拼成新标题。 |
| `BattlePhase.AwaitAction` | `fm.battle.phase.await_action` | 当前 L1 开局及首个有效行动后可达。 |
| `BattlePhase.WonPendingSettlement` | `fm.battle.phase.won_pending_settlement` | 清除第二名敌人后可达；用 `fm.battle.phase.label` 或阶段反馈承载。 |
| `EnemyIntentKind.Strike` | `fm.battle.intent.strike` | 当前 L1 唯一发布意图。 |
| 敌人 `HP=0` | `fm.battle.intent.defeated` | 这是派生显示状态，不伪造一个意图枚举。 |
| `CandidateStageFactKind.RouteLocked` | `fm.battle.stage.route_locked` | 参数 `enemyName` 由敌人实例映射得到。 |
| `CandidateStageFactKind.PhaseSelected` | `fm.battle.stage.phase_selected` | 参数 `phaseName` 只能来自上表 BattlePhase 映射。 |
| `CandidateApplicationKind.InitializeProfile` | `fm.operation.initialize_profile` | 保存恢复只显示操作语义，不显示 OperationId。 |
| `CandidateApplicationKind.EnterAttempt` | `fm.operation.enter_attempt` | 同上。 |
| `CandidateApplicationKind.EnterFormation` | `fm.operation.enter_formation` | 同上。 |
| `CandidateApplicationKind.Attack`／`CandidateBattleOperationKind.Attack` | `fm.operation.attack` | 历史、参考与保存恢复共用同一玩家称谓。 |
| `CandidateApplicationKind.Rollback` | `fm.operation.rollback` | 同上。 |
| `CandidateApplicationKind.SettleVictory` | `fm.operation.settle_victory` | 同上。 |
| `CandidateApplicationKind.ExitAttempt` | `fm.operation.exit_attempt` | 必须与退出应用区别。 |
| `CandidateApplicationKind.RestartAttempt` | `fm.operation.restart_attempt` | 必须保留“按原开局”语义。 |
| `CandidateApplicationKind.SetFormation` | `fm.operation.set_formation` | 当前队伍确认链可达。 |

#### AMEND-02 动态模板路由

- `CandidateBattleFactKind.DirectAttack` 按 `Crit.Triggered` 精确选择 `fm.battle.playback.direct_hit` 或 `fm.battle.playback.critical_hit`；`EnemyIntentKind.Strike` 且有伤害事实选择 `fm.battle.playback.enemy_strike`。节拍外框用 `fm.battle.playback.beat_row`，传玩家侧 1 起序号，不能显示 `OriginalFactIndex` 原值。
- 播放初态与行动提交分别用 `fm.battle.playback.current_state`、`fm.battle.playback.action_submitted`。`CandidateBattleFactKind` 只作选择分派，不显示其枚举名。
- 当前 L1 历史逐项用 `fm.history.action_row`；回退总数继续用既有 `fm.history.undo_preview`，快照标题／已固定路线计数用 `fm.history.snapshot.title/routes`，成员与敌人生命复用 `fm.battle.hud.member_hp/enemy_hp`。不得显示 FaceId、PairId、IntentCursor、OperationId 或 SceneRevision。
- 默认参考总入口继续用既有 `fm.reference.conditions`，其 `conditions` 由 `fm.reference.condition.level/member/carry_empty/random_verified` 的已解析行组成；空技能用 `fm.reference.value.none`。差异标题用 `fm.reference.differences.title`，当前 L1 可达的阵容、等级、随机、局面差异分别用 `fm.reference.difference.party/level/random/state`；参考步骤用 `fm.reference.step.row`。package、seed 与修订号不进入参数。
- 保存恢复用 `fm.save_recovery.operation_summary` 显示原操作类型；观察到多个候选时用 `fm.save_recovery.candidate_row` 的稳定排序 1 起序号。真实按钮仍绑定原 OperationId／CommitId，但两者不进入玩家文案。失败、未知、查询、重试、确认、继续和安全结束继续复用既有 `fm.save_recovery.*`。
- 任何 `{phaseName}`、`{operationName}`、`{characterName}`、`{enemyName}`、`{itemName}`、`{levelName}`、`{reason}` 都必须是已解析玩家文本，不得传原 ID、枚举、诊断 reason code 或 `ToString()` 结果。

#### AMEND-02 明确不新增

- 不为当前 L1 不可达的 `BattlePhase.AwaitLinks/AwaitRescue/Closed`、`CandidateStageFactKind.TemporaryRouteRemoved/PendingLinkAdded/PendingLinkRemoved/FaceChanged`、`CandidateBattleOperationKind.Link`、`EnemyIntentKind.Charge/Unspecified` 新增玩家键。
- 不为 `CandidateBattleFactKind`、`CandidateCharacterEndKind`、`CandidateApplicationPhase`、`CandidatePermanentKind` 建通用枚举翻译表；当前页面分别由播放模板、结果模板、保存恢复状态模板和真实空装备／无配方模板表达产品语义。
- 不为 `F1`、PairId `A/B`、OperationId、CommitId、RecoveryId、package/fingerprint、随机 seed、PreferenceRevision、SceneRevision、IntentCursor、字段路径、异常类型或内部 reason/diagnostic code 建显示名。诊断码只进入已有明确含 `{errorCode}` 的诊断文案。
- 不补 L2/L3、后续职业／敌人／技能／配方／装备、账号、广告、后台或下载管理文案。当前队伍、装备和恢复的一般空态／动作已由既有 `fm.party.*`、`fm.inventory.*`、`fm.crafting.*`、`fm.save_recovery.*` 覆盖，不因旧 View 暴露内部对象而扩张产品范围。

## 2. 决策状态

### 2.1 本轮直接执行的已定事实

- 首 Demo 是安卓版正式产品链上的内容较少版本，共用业务规则、存档语义与正式流程；不是一次性假流程。
- 当前正式发布内容只含 L1：`level:ch01-01` / LevelVersion 1 / recordVersion 1，原点解释 `AssumedBottomLeft`。L3 仍是隔离样例，不能显示为已开放关卡。
- 新档为战士 W，Lv1 / XP0 / HP100，另外两个槽为空；库存与携带为空、未学主动技能。L1 基础参数 `E=20`，不是固定实发20；新档W1正常两线、Dealt30／Taken5按[已批准规则（§269.2／§271）及§115算例](../../system-design/2026-09-17/system-task-packets.md)评分后预期26 XP＋2锡片。其他报告按评分公式和真实收据，不将所有胜利固定为26；无首通额外奖励、无下一关解锁。
- 运行时 UI 必须迁移到 uGUI；UI Toolkit 仅保留编辑器职责。页面换框架不能改变业务结果、要求清档或重建第二套战斗／结算／保存。
- 首发文本为英语与简体中文。首次启动：中文系统选简中，其余选英语；设置内可手动切换并记住。所有玩家可见 prefab 文本默认必须是显眼诊断占位符，并在运行时由绑定替换。
- 文本以 Excel/CSV 为主要编辑源，制作阶段走 Luban，运行时用项目自有本地化绑定，资源加载走 YooAsset；官方 Localization/Addressables 不是本轮替代方案。
- 地图、队伍、背包在活动战斗存在时可读；不能新开战斗、改阵容或换装备。合法制作及明确的道具偏好仍按原业务准入，但当前 L1 无已发布配方，应显示真实空态。
- 默认完整攻略免费并可重复看，只播放已验证的走线参考，不预演上方战斗。在线当前局面分析后置，当前 Demo 不显示假广告成功或假在线结果。
- 赢得战斗后必须先保存胜利，再完成或跳过末段播放，然后由玩家明确继续基础奖励结算。结算只能入账一次。
- 关闭应用与“退出本局”不同；关闭重开必须恢复同一资料和当前可继续业务状态，不能通过清档绕过恢复。

### 2.2 已定但不应扩进当前 L1 的完整产品规则

- 第一章方向、每 16 关 Boss、法师第 8 关后入队、盾骑士第 21 关、弓箭手第 37 关，以及 16 关赠证／嘲讽教学与两面 Boss 规则继续有效，但当前 L1 不生产、不展示这些未发布内容。
- 每职业只有一个可培养角色；当前 Demo 只有战士，不为展示队伍页临时造第二角色。
- 广告奖励、广告复活、广告攻略、首通 16 证、资源移动流量确认、账号与云档等正式规则保留，但它们不是当前 L1 正常试玩链的可用功能。

### 2.3 候选或尚未验证，不得写成完成

- A/B/C 角色造型仍是候选；试玩占位必须明确可撤销，不能写成用户已定稿。
- 当前旧 APK 和旧画面是 UI Toolkit 宿主，不能作为新 uGUI 页面、双语、触控或视觉验收证据。
- 中文字形、安全区、实际热区、触摸／多指／取消、页面重建、冷启动恢复及 Android 真机均待本轮新证据。
- 首 Demo 设备接收、029 独立产品 R、验证器误报修正均未完成；旧 4041/4041 只证明原固定版本和原范围。

### 2.4 Q1～Q35 继承索引（不重开）

本轮界面直接依赖 Q7、Q12～Q19、Q26～Q29、Q35；后续首版功能继续依赖其余决定。完整原题、答复与覆盖关系以 `docs/system-design/2026-09-17/system-task-packets.md` §465 为准。本文件只给执行索引：

- 广告／跨档／战斗：Q1 同源分叉只留所选结果；Q2/Q5 未收独有材料／经验卡补原物；Q3 倒下清本人持续负面；Q9 广告来源先扣；Q10 免疫目标仍耗已开启毒箭；Q11 施法者倒下不移除队友既有盾；Q14 重叠的 16 首通证只留一份且常设区域标明首通奖励；Q18 直接广告经验按唯一职业角色接入并去重。
- 账号：Q6 Google＋邮箱；Q8 同一邮箱提供密码／数字验证码；Q22 核验后更换／解绑且至少留一入口；Q23 正式账号不合并；Q24 后续提供人工申诉；Q28 游客本机凭据全失不可找回且提前告知。正式账号／云档／申诉后置，不进入当前 L1 UI。
- 内容、语言与更新：Q7 Excel/CSV；Q12 英／简中；Q13 随系统＋手选记忆；Q15 自然页面边界提示重启且可稍后；Q17 Luban；Q19 项目内绑定＋YooAsset；Q21 R2；Q26 小启动包首次下载主要内容；Q27 首版只保免费默认攻略；Q35 移动数据每次先提示剩余量并等待确认，Wi-Fi 中途切换同样暂停。
- 平台、后端与发行：Q4 海外 Google Play；Q16 保留 2022.3、发行准备验证 62f3 候选；Q20A Go＋sqlc、Q33 MySQL 社区版；Q20B 完整账号托管后置；Q25 保留激励广告和最小广告后端；Q29 最低 Android 7/API24；Q30 单一广告源；Q31 主要 13 岁以上；Q32 先验 AdMob＋Google。具体发行地区与广告后端部署商仍未选。

## 3. 首 Demo 正常玩家流程

页面名称描述产品语义；括号内列当前已有业务入口，供 uGUI 迁移保持，不要求复刻旧 UI Toolkit 布局。

1. **启动／本机资料**：空根显示“创建本机资料”；点击后进入明确建档确认并完成保存。已有资料直接读取；上次创建未完成时只允许“继续原创建”或重新读取，不能新建第二份。
2. **地图冒险**：底部／主导航可进入地图、队伍、背包。地图只显示 L1 可挑战；没有已发布且开放的下一关。点击 L1 进入入场准备。
3. **队伍**：显示战士与两个空槽、等级／经验／恢复状态。允许检查阵容；提交前显示修改前后并确认。L1 正常初始阵容不需要玩家造假填满三人。
4. **背包**：显示真实库存、装备、预留、可用和本局携带。空库存为正常状态；合成页明确“当前内容暂无已发布配方”。原操作结果只从真实记录进入。
5. **入场准备**：显示 L1、当前阵容、入场检查与当前可选角色。玩家选择存活战士，再点“确认入场条件”；若已有活动战斗，按钮禁用并提供“继续当前战斗”。
6. **战斗**：默认参考可打开／关闭；玩家选择存活成员，从同色端点沿横竖相邻格拖到配对端点，松手后只提交一次行动。真实操作开始时关闭参考覆盖。表现播放中可“跳到当前战局”。
7. **战斗管理**：可从历史项预览回退范围，再“确认回退这些行动”或“保留当前战局”；“按原开局重来…”和“退出本局…”必须先显示准确影响再确认，二者都不能等同于退出应用。
8. **胜利门**：胜利保存后显示“胜利已保存，基础奖励待结算”。末段表现未完成时提示待办；可继续基础奖励结算的唯一时点开放按钮。
9. **结果**：显示 L1、每位原槽成员经验、倒下／恢复结果及材料到账；L1 基础参数 `E=20`，新档W1正常两线、Dealt30／Taken5的评分结果为26 XP＋2锡片，与本次[demo-flow01 新档实得](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/new-result-after-wait.png)一致；其他报告按评分公式和真实收据，不固定实发20或26。提供“回地图”“查看当前队伍”“查看当前背包”“重新入场挑战本关”。常驻说明“本内容段结束，暂无已发布且开放的下一关”。
10. **返回与重开**：根页返回打开退出应用确认；取消后留在原页，确认后退出。重新启动后恢复同一资料；已结算结果、活动战斗或待处理保存各回到其合法继续入口。

## 4. 页面、主要操作与异常体验映射

| 页面／状态 | 主要可见信息 | 玩家操作 | 异常或受阻反馈 | 不允许的伪完成 |
| --- | --- | --- | --- | --- |
| 启动／资料 | 本机资料状态、已有资料会保留 | 创建、继续原创建、重新读取 | 未认领数据：保留数据并检查；读取失败：显示可诊断原因 | 清档、新建覆盖、把未知提交当失败 |
| 地图 | L1 开放状态、无下一关事实 | 选 L1；去队伍／背包 | 无已发布定义时真实空态；活动战斗时仍可读但不可新入场 | 显示 L2/L3、虚构解锁或奖励 |
| 队伍 | 三槽、角色等级经验、恢复状态 | 选择角色、检查阵容修改、确认／取消 | 无可出战成员时解释需调整或等待恢复；活动战斗时禁止改阵容 | 自动补角色、静默换人 |
| 背包／合成 | 持有／装备／预留／可用；配方状态 | 选角色、查看装备／偏好／原结果 | 空包和无配方为正常空态；未选角色、未装备、活动战斗给明确原因 | 假配方、假材料、把空态写成错误 |
| 入场准备 | 关卡、阵容、入场条件、已选角色 | 选战士、检查并确认入场；继续旧战斗 | 未选角色；活动战斗冲突；保存待处理；内容绑定不支持 | 绕过入场检查、重算第二份属性 |
| 战斗 HUD／棋盘 | 成员 HP、敌方 HP／意图、阶段、选中成员、棋盘 | 选人、拖线、短按历史、开参考、跳播放 | 非端点起笔、断裂／交叉／错端点、取消／失焦均不提交；双指不抢占 | 无合法路线也结算、一次手势提交多次 |
| 回退／重来／退出本局 | 将移除的行动、恢复前快照或重来／退出影响 | 二次确认或取消 | 保存未知时回到原请求恢复，不打开新动作 | 回退长期成长／已到账权益；把退出应用当退出本局 |
| 保存恢复 | 当前头、原请求、候选与诊断 | 查询原结果、重试原保存、确认原保存结果、继续原候选、在安全时结束未提交请求 | 未确认前阻止新资料和冲突操作；错误信息仍提供安全下一步 | 自动把“最后一项”当原请求、重复奖励 |
| 胜利待结算 | 胜利已保存、末段播放／结算待办 | 完成或跳过播放，再继续基础奖励结算 | 结算不可用时明确原因 | 自动结算、双击重复结算 |
| 结果 | 基础奖励、成员经验、材料、结束记录 | 地图、队伍、背包、重打 | 无下一关显示内容段结束 | 显示首通额外奖、广告倍增或下一关 |
| 退出应用 | 已保存进度保留、未完成保存下次继续 | 退出应用／留在游戏 | 保存待处理仍明确会在下次继续 | 把关闭 App 作为业务操作提交 |

### 4.1 已定首版条件界面（不冒充当前 L1 已实现）

这些状态不属于当前 L1 正常试玩链，但 uGUI／本地化设计必须预留明确产品语义，后续启用时不能重新发明规则：

| 条件界面 | 触发与展示 | 玩家选择 | 关键边界 |
| --- | --- | --- | --- |
| 首通奖励说明 | 有首通奖励的关卡准备／奖励说明区域常设显示“首次通关奖励”；16 关说明通用职业证 | 只读；结算仍走真实结果 | 不靠一次弹窗或飘字；Q14 跨档重叠只留一张，当前 L1 不显示首通奖励 |
| 更新已下载 | 新版后台下载完成且玩家回到地图等自然边界 | “重启并更新”“稍后” | 不在战斗中打断；稍后仍保持当前版本／存档绑定，不假装已启用新版 |
| 首次资源下载 | 启动包缺主要内容，显示总量／剩余量、网络与可取消状态 | 开始／取消；移动数据时另行确认 | 首次缺包可阻断进入；失败保留原可用集合；当前未验证 YooAsset 断点续传 |
| 移动数据确认 | 下载将使用移动数据，或 Wi-Fi 中途切换后检测到移动数据 | “使用移动数据继续”“等待 Wi-Fi／取消” | 未确认前不继续；本次同意不自动授权以后下载；显示剩余量 |
| 游客不可恢复告知 | 创建本机游客资料前的简短常设／确认信息 | 继续创建／返回 | 明确凭据及本机副本全失则无法找回；不许诺恢复码、手工备份或游客申诉 |

## 5. 英／简中文本制作合同

COPY-001 制作稿每行至少包含：稳定键、页面、控件／状态、触发语境、简中、英语、参数列表、严重级别与 QA 备注。参数用 `{name}`，同一键两种语言的参数集合必须完全相同；ID、数字、物品名和错误码不得靠字符串拼接改变语序。

文本语气规则：

- 正常操作短、直接、面向玩家；不用内部类名、阶段枚举或提交哈希作主文案。
- 保存不确定不是普通“失败”。必须表达正在核定原结果，并避免诱导重试造成重复操作。
- 危险／不可逆按钮明确对象：“退出本局”“退出应用”“结束未提交请求”不能都写成“退出”。
- 空态说明事实并给下一步；当前无配方、无下一关是产品状态，不用红色错误语气。
- 默认参考明确是“按指定条件的开局参考”，不是当前局面必胜提示。
- 诊断占位符使用统一可搜索值 `【if you see this, it is a bug.】`；实际游玩出现即为阻断性本地化缺陷，不得回退到 prefab 成品中文或英文掩盖。

当前 L1 文本包至少覆盖：启动、通用导航、地图、队伍、背包／合成、入场、战斗 HUD／操作提示、默认参考、回退、重来／退出本局、保存恢复、胜利待结算、结果、退出应用、字体许可与通用诊断。COPY-001 另为 §4.1 的已定条件界面提供可复用键，但明确标为非当前 L1 正常链。账号、广告、下载、设置全页不因本包临时扩做；语言选择入口与切换反馈由主程确定承载页面后引用同一文本合同。

## 6. 交给 QA 的可观察场景

以下是策划验收场景，不规定测试框架；主测试负责选择静态、uGUI 事件、本机、云端和真机的最小有效组合并绑定准确候选。

| ID | 场景 | 前置与操作 | 可观察通过条件 |
| --- | --- | --- | --- |
| PQA-01 | 空档首次建档 | 清洁的合法测试根；启动→创建→确认 | 只生成一份资料；战士 Lv1/XP0、两空槽、空库存；进入地图；无占位符 |
| PQA-02 | L1 正常完整链 | 地图→L1→选战士→确认入场→合法连线直至胜利→结算 | 胜利先保存后结算；一次到账 20 XP、2 锡片；结果页四个去向可用；无下一关事实准确 |
| PQA-03 | 重打连续性 | 结算后“重新入场挑战本关” | 重新走正式入场，不复制上一局奖励／历史，不需要清档 |
| PQA-04 | 队伍与背包空态 | 新档依次进入队伍、背包、合成 | 三槽与唯一战士准确；库存空；无配方是正常空态；不存在假角色／配方 |
| PQA-05 | 活动战斗门 | 开始 L1 后返回导航查看地图／队伍／背包 | 页面可读；新入场、阵容和装备修改被准确阻止；“继续当前战斗”返回原局 |
| PQA-06 | 手势不误交 | 棋盘上测试非端点、错端点、交叉、沿路回退、取消、失焦、第二指加入 | 非法／取消手势零提交；首指身份稳定；合法松手只提交一次；命中与画线一致 |
| PQA-07 | 播放与参考生命周期 | 打开默认参考、选步骤、开始真实拖线；播放中跳到当前战局；反复进出页面 | 真实拖线关闭参考但不丢手势；旧回调不覆盖新页；跳过只影响表现、不改变业务事实 |
| PQA-08 | 回退／重来／退出区分 | 预览历史回退并取消／确认；分别预览重来和退出本局 | 每次先显示范围；取消零变化；回退只撤对应历史；重来按原入场基线；退出本局不新建一局 |
| PQA-09 | 保存未知与恢复 | 在允许的故障注入环境制造失败／未知，关闭并重开 | 新操作受阻；原请求可查询／重试／确认；恢复后最多一次提交／到账；不从页面残留猜结果 |
| PQA-10 | 关闭重开 | 分别在地图、活动战斗、胜利待结算、已结算结果关闭并冷启动 | 恢复同一资料与合法继续点；活动局不跨成本机第二局；已结算不重复入账 |
| PQA-11 | 双语首次选择 | 简中系统首次启动；非中文系统首次启动 | 前者简中、后者英语；所有同一页面文本同语种且字形完整 |
| PQA-12 | 手动切换与记忆 | 设置中中→英、英→中，切换时停留不同页面；重启 | 当前可见页面和重建页面刷新；选择跨重启保留；动态参数正确；无混语和占位符 |
| PQA-13 | 本地化故障可见 | 测试缺键、变量不匹配、漏绑定和字体缺字 | 产生可定位诊断并使验收失败；不静默显示 prefab 成品文案、不吞参数 |
| PQA-14 | Android 画面 | 准确 uGUI APK 在目标模拟器及真机走 PQA-01～02 的画面段 | Canvas/EventSystem 生效；安全区、竖屏比例、滚动、按钮热区、文字不截断；触摸路线与命中一致 |
| PQA-15 | 返回键与退出 | 各子页按返回，根页按返回并取消／确认 | 子页回上一合法页；根页弹退出应用确认；取消保留上下文，确认不冒充退出本局 |

阻断条件：任何诊断占位符可见；中英文参数集合不一致；L1 外内容被展示为已开放；结算重复；保存未知仍允许冲突操作；触摸显示路线与实际命中不一致；冷启动丢失或复制业务事实。

PQA-02 的操作输入优先使用产品内已发布、已接受的 L1“开局默认参考”逐步人工走线；参考只说明其标注阵容、等级、技能、偏好与随机条件，不保证匹配任意当前局面。QA 必须用真实触摸／指针完成路线，不能调用参考回放或隔离接口代替提交。若准确产品条件下仍无法自然完成，保留当前随机／绑定／步骤和失败画面，作为内容或实现缺陷交回；不得临时注入胜利。主策不另造一套未验证路线。

## 7. 跨线交接

### 给主程

- 以上页面是产品语义与行为映射；uGUI 层级、Prefab 拆分、绑定接口及 Luban 表结构由主程冻结，但不能合并“退出本局／退出应用／结束未提交请求”等不同业务动作。
- 需要主程回填：稳定的页面／控件 ID、语言设置承载页、缺键／变量错误策略、运行时语言刷新边界、数字／物品名格式化规则，以及 COPY-001 CSV 到正式表的映射。
- 当前旧 `FightMatchHostView` 的中英混写和运行时替换只用于识别已有入口，不是应照搬的最终文案实现。

### 给主美

- 视觉布局至少覆盖：启动、三主导航、准备、战斗 HUD＋棋盘、参考浮层、确认弹窗、保存恢复、结算和退出确认。
- 为英语扩展预留，按钮不能依赖中文字宽；动态状态／错误至少允许两行；棋盘交互区不被 HUD、弹窗透明区域或安全区遮挡。
- 战斗中默认参考是覆盖层且真实拖线立即让位；保存恢复与危险确认应有明确层级，避免与普通空态混淆。
- 主美已按接受 CSV 实测容量：普通长正文视口至少容纳五行英语，`fm.guest_local_only.body` 约六行；再超出时只滚动正文，标题和动作固定可见。长按钮采用全宽纵排或 56/64 高两行，不截断。该布局结论不要求缩短当前接受文案。
- `ART-PROD-001` 已由主美在 `docs/team/2026-09-30/art.md` §9 给出 `ACCEPT`，交付目录为 `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/`。主策接收其页面容量与布局接口，不将单色 `PLACEHOLDER`、灰盒图或该 verdict 解释为最终画风、角色／头像、正式 Prefab、设备效果或用户视觉选择。

### 给主测试

- PQA-01～15 是验收语义清单；复用旧业务证据时必须标出只覆盖业务、不覆盖 uGUI／双语／设备。
- L1 正常链的产品门以真实输入和原保存为准；不得用隔离胜利注入、直接业务方法调用或旧 UI Toolkit APK替代玩家路径。

## 8. 当前状态与下一步

- 已完成：正式规则／当前 L1／旧页面入口／uGUI 与本地化决定的主策整理；页面正常链、异常体验、QA 场景及 COPY-001 英／简中制作稿。主测试已回传 PQA-01～15 可执行映射且无策划冲突；设备当前无 ADB 目标，保持 `DEVICE_PENDING`。
- 进行中：等待主程提供正式 Luban／绑定映射和技术可达性反馈；主美已绑定接受 CSV，`ART-PROD-001` 灰盒布局／容量获专业 `ACCEPT` 且无产品动作冲突，最终视觉仍待用户选择；主测试将在固定实现候选后执行差异验证。
- 本文件当前是 `CROSS_ROLE_INPUT_READY`，不是产品实施、Unity 或设备验收。收到跨线问题后只补具体缺口，不重开 Q1～Q35 或擅自增加关卡、职业、经济、广告及后台范围。

## 9. COPY-001-AMEND-03：最终缺口处置与交回

### 9.1 固定输入、边界与策划结论

本次只依据以下固定输入完成最终缺口判定：

- `copy-gap-final.json`：SHA-256 `185c37beeb867aff99ab6c43623e5c54885aa3130facb835a47df015f012ce85`，6702 行，285726 bytes；
- `delivery-audit-final.json`：SHA-256 `f04684151ebdc1efc7e1a673e2fafd9c1fb919b5ce6f6e8bcf1394730c398627`，149 行，7713 bytes；
- 接受的 216 键 CSV：SHA-256 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`，其 217 个物理行作为本次不可改写前缀。

处置覆盖 `56 copy_semantics + 10 binding + 5 data/format + 10 product/scope = 81` 组，`BLOCKED=0`。本次追加 47 个稳定键，优先闭合自然 L1、故障／生命周期安全路径及现有批准测试确实触达的通用玩家可见状态。隔离夹具的角色、敌人、技能、物品、配方和关卡 ID 不是发布内容，主策不为它们虚构名称；未来内容专属显示待真实发布定义和名称合同齐备后再启用。

明确产品决定：

- 恢复／结果页只保留一次真实业务状态；外层路由、Host TargetKind、确认头元数据和重复标题保持隐藏，不增加玩家文案。
- 保留自然的旧资料升级入口和确认页，但只说“升级本机资料”，不向玩家显示 V3、V4、迁移枚举或格式版本。
- “审核结束请求”与最终“结束未提交请求”是两个动作；前者只打开审核，后者仅在已证明未提交且审核完成后执行。
- 保留现有批准测试保护的清空装备页、数量字段、偏好入口和队伍三槽约束；不删除控件来规避本地化。
- 初始 L1 历史为空时只显示既有 `fm.history.title`，不添加“暂无历史”提示。

### 9.2 81 组唯一处置

`origin_ids` 保留跨视图合并来源；同一来源的不同语义片段会明确标注，而不是把动作名、历史行和参考步骤误并成同一模板。

| group ID | origin_ids / 范围 | disposition | 固定输出或理由 |
| --- | --- | --- | --- |
| N-C01 | N-C01 | APPEND_KEY | `fm.common.action.refresh` |
| N-C02 | N-C02 | APPEND_KEY | `fm.save_recovery.refresh_button` |
| N-C03 | N-C03 | APPEND_KEY | `fm.common.action.review_changes` |
| N-C04 | N-C04 | APPEND_KEY | `fm.operation.clear_equipment` |
| N-C05 | N-C05 | DEFER_OUT_OF_CURRENT_SCOPE | 具体装备物品未发布；不得给隔离 ItemId 起名 |
| N-C06 | N-C06 | APPEND_KEY | `fm.operation.set_item_preference` |
| N-C07 | N-C07 | APPEND_KEY | `fm.common.field.quantity` |
| N-C08 | N-C08 | DEFER_OUT_OF_CURRENT_SCOPE | 按物品偏好行随真实装备内容发布 |
| N-C09 | N-C09 | DEFER_OUT_OF_CURRENT_SCOPE | 手选材料来源不是当前 L1 玩家功能 |
| N-C10 | N-C10 | DEFER_OUT_OF_CURRENT_SCOPE | 范围起点只随未来来源编辑器启用 |
| N-C11 | N-C11 | DEFER_OUT_OF_CURRENT_SCOPE | 来源行需要未来真实物品名和 N-D03 |
| N-C12 | N-C12 | APPEND_KEY | `fm.input.nonnegative_integer`；明确非负、无多余前导零及过长／超限处理，保留测试保护控件 |
| N-C13 | N-C13 | APPEND_KEY | `fm.inventory.change_preview.title`；当前 L1 仅清空装备审核可达，偏好入口可见但无已发布且已装备物品时修改／审核不可达 |
| N-C14 | N-C14 | APPEND_KEY | `fm.inventory.equipment.clear_preview` |
| N-C15 | N-C15 | DEFER_OUT_OF_CURRENT_SCOPE | 具体装备预览随发布物品启用 |
| N-C16 | N-C16 | DEFER_OUT_OF_CURRENT_SCOPE | 具体偏好预览随发布装备启用 |
| N-C17 | N-C17 | DEFER_OUT_OF_CURRENT_SCOPE | 当前无发布配方，不命名隔离配方 |
| N-C18 | N-C18 | DEFER_OUT_OF_CURRENT_SCOPE | 未来合成消耗行 |
| N-C19 | N-C19 | DEFER_OUT_OF_CURRENT_SCOPE | 未来合成产出行 |
| N-C20 | N-C20; B-COPY-03(迁移动作名) | APPEND_KEY | `fm.profile.data_upgrade.title` |
| N-C21 | N-C21 | APPEND_KEY | `fm.profile.data_upgrade.body` |
| N-C22 | N-C22 | APPEND_KEY | `fm.save_result.title` |
| N-C23 | N-C23 | APPEND_KEY | `fm.save_result.confirmed` |
| N-C24 | N-C24 | APPEND_KEY | `fm.save_recovery.ended_uncommitted` |
| N-C25 | N-C25; B-COPY-06; B-COPY-07; B-COPY-17 | APPEND_KEY | `fm.common.business_attention`；仅受控 `errorCode` |
| N-C26 | N-C26 | APPEND_KEY | `fm.save_result.notification_failed` |
| N-C27 | N-C27 | DEFER_OUT_OF_CURRENT_SCOPE | 当前无锁定关卡产品表面 |
| N-C28 | N-C28 | DEFER_OUT_OF_CURRENT_SCOPE | 当前无发布前置关卡链 |
| N-C29 | N-C29 | DEFER_OUT_OF_CURRENT_SCOPE | 当前无未来关卡通关标签需求 |
| N-C30 | N-C30 | DEFER_OUT_OF_CURRENT_SCOPE | 非空装备摘要等待真实发布物品 |
| N-C31 | N-C31 | DEFER_OUT_OF_CURRENT_SCOPE | 非空携带摘要等待真实发布物品 |
| N-C32 | N-C32; B-COPY-01 | DEFER_OUT_OF_CURRENT_SCOPE | 不为 synthetic-only ID 编造玩家名称 |
| N-C33 | N-C33; B-COPY-03(动作名补充) | APPEND_KEY | `fm.operation.link`、`fm.operation.advance_recovery`；永久变更按具体子类键 |
| N-C34 | N-C34 | APPEND_KEY | `fm.save_recovery.select_operation` |
| N-C35 | N-C35 | APPEND_KEY | `fm.save_recovery.select_candidate`；只要求从已观察候选列表选择，不把选择写成结果已核定 |
| N-C36 | N-C36 | APPEND_KEY | `fm.save_recovery.return_to_original_flow` |
| N-C37 | N-C37 | APPEND_KEY | `fm.save_recovery.retry_before_resolve` |
| N-C38 | N-C38 | APPEND_KEY | `fm.common.state.review_required` |
| N-C39 | N-C39 | APPEND_KEY | `fm.save_recovery.result_required` |
| N-C40 | N-C40 | APPEND_KEY | `fm.common.state.changed_review_again` |
| N-C41 | N-C41 | APPEND_KEY | `fm.party.duplicate_member` |
| N-C42 | N-C42 | APPEND_KEY | `fm.party.upgrade_required` |
| N-C43 | N-C43 | DEFER_OUT_OF_CURRENT_SCOPE | 启用／停用值随未来偏好编辑器提供 |
| B-COPY-02 | B-COPY-02 | APPEND_KEY | `fm.battle.phase.await_links`、`await_rescue`、`closed` |
| B-COPY-03 | B-COPY-03(补线历史／参考行) | APPEND_KEY | `fm.history.link_row`、`fm.reference.step.link_row`；动作名由 N-C33 统一 |
| B-COPY-04 | B-COPY-04 | APPEND_KEY | 四个受控 stage fact 键 |
| B-COPY-05 | B-COPY-05 | APPEND_KEY | `fm.battle.intent.charge`、`fm.battle.playback.enemy_charge` |
| B-COPY-08 | B-COPY-08 | APPEND_KEY | `fm.battle.playback.interrupted`；不暴露异常文本 |
| B-COPY-09 | B-COPY-09 | APPEND_KEY | `fm.history.snapshot.pending_links` |
| B-COPY-10 | B-COPY-10 | DEFER_OUT_OF_CURRENT_SCOPE | 非空技能／携带需要真实发布名称；既有空值继续复用 |
| B-COPY-11 | B-COPY-11 | DEFER_OUT_OF_CURRENT_SCOPE | 偏好修订号不能代表语义差异；待结构化事实 |
| B-COPY-12 | B-COPY-12 | APPEND_KEY | `fm.reference.difference.party_empty`、`member_absent` |
| B-COPY-13 | B-COPY-13 | DEFER_OUT_OF_CURRENT_SCOPE | 当前 L1 无下一关；随未来发布关卡启用 |
| B-COPY-14 | B-COPY-14 | APPEND_KEY | `fm.result.no_reward.title`、`body`，区分退出／重开原结果 |
| B-COPY-15 | B-COPY-15 | APPEND_KEY | `fm.result.member.original_slot` |
| B-COPY-16 | B-COPY-16 | APPEND_KEY | `fm.result.recovery_started`，陈述历史事实而非当前倒计时 |
| N-B01 | N-B01 | REUSE_EXISTING | 恢复页标题复用 `fm.save_recovery.title`；隐藏重复外层标题 |
| N-B02 | N-B02 | REUSE_EXISTING | 仅确认保存后复用 `fm.inventory.preference.saved` |
| N-B03 | N-B03 | REUSE_EXISTING | 不支持入口复用 `fm.entry.binding_unsupported` |
| N-B04 | N-B04 | REUSE_EXISTING | 按真实控件复用 entry／party／inventory 活动战斗锁定键 |
| N-B05 | N-B05 | REUSE_EXISTING | 仅 SaveFailed + 受控码复用 `fm.save_recovery.failed` |
| N-B06 | N-B06 | HIDE_INTERNAL_OR_DUPLICATE | 复用状态专键，隐藏重复诊断状态行 |
| B-WIRE-01 | B-WIRE-01 | BINDING_ONLY | 接线既有手势结果键，不新增同义文案 |
| B-WIRE-02 | B-WIRE-02 | BINDING_ONLY | 初始参考关闭输入复用既有键；只在完成后显示 replay |
| B-WIRE-03 | B-WIRE-03 | BINDING_ONLY | 历史／保存／跳过／结束复用既有键；同步查询不显示 checking |
| B-WIRE-04 | B-WIRE-04 | BINDING_ONLY | 初始参考帧用 `fm.reference.step.select_prompt`，不得显示 action_submitted |
| N-D01 | N-D01 | DATA_CONTRACT | 从实际原请求 kind + permanent subkind 提供 `operationName` |
| N-D02 | N-D02 | DATA_CONTRACT | `remainingTime` 只来自当前恢复业务状态并本地化格式化 |
| N-D03 | N-D03 | DATA_CONTRACT | 未来来源编辑器提供玩家侧序号、范围和物品名；当前不启用表面 |
| N-D04 | N-D04 | DATA_CONTRACT | 原因归为稳定玩家码后才提供 `errorCode`；不传异常／枚举文本 |
| C-D01 | C-D01 | DATA_CONTRACT | Host／session lifecycle 只提供可见性和真实业务状态；内部 TargetKind 不出屏 |
| N-P01 | N-P01 | HIDE_INTERNAL_OR_DUPLICATE | 隐藏路由重复状态；N-C22/23 显示一次，N-C26 单列通知故障 |
| N-P02 | N-P02 | HIDE_INTERNAL_OR_DUPLICATE | 确认头元数据保持内部，不新增玩家行 |
| N-P03 | N-P03 | APPEND_KEY | `fm.save_recovery.review_end_button` 与最终安全结束键分离 |
| N-P04 | N-P04 | APPEND_KEY | 保留自然旧资料升级；使用 N-C20/21 且不显示版本号 |
| N-P05 | N-P05 | DEFER_OUT_OF_CURRENT_SCOPE | 合成成本／产出区域等待真实配方；未来可复用 inventory row |
| N-P06 | N-P06 | HIDE_INTERNAL_OR_DUPLICATE | 隐藏 `Defined`；不增加无真实触发的来源／候选空态 |
| N-P07 | N-P07 | HIDE_INTERNAL_OR_DUPLICATE | Host TargetKind 与目标状态保持内部 |
| N-P08 | N-P08 | APPEND_KEY | 保留测试保护控件；当前清空装备用 N-C04/06/07/12/13/14，未来来源编辑延后 |
| B-DECISION-01 | B-DECISION-01 | HIDE_INTERNAL_OR_DUPLICATE | 初始空历史不加提示，`fm.history.title` 足够 |
| B-DECISION-02 | B-DECISION-02 | REUSE_EXISTING | 固定 L1 复用 `fm.result.no_first_clear_bonus`；未来其他发布内容必须由实际首通奖励配置事实守卫，不得仅凭 `Reward != null` 推断 |

处置统计：`APPEND_KEY=39`、`REUSE_EXISTING=6`、`BINDING_ONLY=4`、`DATA_CONTRACT=5`、`HIDE_INTERNAL_OR_DUPLICATE=6`、`DEFER_OUT_OF_CURRENT_SCOPE=21`、`BLOCKED=0`，合计 81。

### 9.3 主程映射与非文案动作

主程可直接按以下合同解除 `Q1=WAITING_FOR_COPY_KEYS`：

- Navigation 普通刷新绑定 `fm.common.action.refresh`；恢复页刷新绑定 `fm.save_recovery.refresh_button`。两者不得互换请求语义。
- `CandidateApplicationKind` 继续使用既有 9 个映射；补充 Link→`fm.operation.link`、AdvanceRecovery→`fm.operation.advance_recovery`、自然资料升级→`fm.profile.data_upgrade.title`。PermanentRequest 必须先读取已确认的具体 kind：ClearEquipment→`fm.operation.clear_equipment`，SetPreference→`fm.operation.set_item_preference`；未发布的 Equip/Craft/UseExperienceCards/LearnSkill/Teaching 保持不可见，不可回退 enum 或 generic permanent 文案。
- `PreviewRequired/EndConfirmationRequired`→`fm.common.state.review_required`；`ResultRequired`→`fm.save_recovery.result_required`；过期预览／上下文→`fm.common.state.changed_review_again`；其余选择、所有权、重试前置分别用 N-C34～37 新键。
- BattlePhase、Link 历史／参考、stage facts、Charge、pending-link snapshot 按新增键逐项显式映射；未知枚举继续触发阻断诊断，禁止 `ToString()`。
- 播放调度异常显示 `fm.battle.playback.interrupted`，只传 N-D04 的稳定 `errorCode`；异常类型、堆栈和原始 message 仅进日志。参考初始帧改绑 `fm.reference.step.select_prompt`。
- 无奖励原结果同时使用 `fm.result.no_reward.title(operationName)` 与 `body(levelName)`；仅 ExitAttempt／RestartAttempt 等实际原操作可提供 operationName。成员原槽使用玩家侧 1 起 `slotNumber`；历史恢复用 `fm.result.recovery_started(duration)`，不得传当前 `remainingTime`。
- N-D01 必须以请求身份重新读取真实 kind，不从页面残留推断；N-D02 格式化当前恢复剩余时长；N-D04 将业务原因映射为稳定诊断码。C-D01 负责生命周期可见性，不将 route/Host/session 身份渲染为玩家文本。
- 隐藏重复标题、TargetKind、`Defined` 和确认头元数据；不得删除 N-P08 的测试保护控件。安全结束先显示 `fm.save_recovery.review_end_button`，审核完成且业务证明未提交后才显示／执行既有 `fm.save_recovery.end_uncommitted_button`。
- 对延后组，当前实现必须保持对应产品入口不可见或不可启用；不能用 prefab placeholder、内部 ID、英语硬编码或 synthetic 名称作为“临时闭合”。

### 9.4 验证与返回路径

本次只做文本／结构验证，不运行 Unity。CSV 追加后要求：旧 217 个物理行逐字节保持；首 217 行 SHA-256 仍为 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`；全表键唯一；每行 9 列；中英文参数集合与 `parameters` 完全一致；不得出现 `OperationId`、`CommitId`、`FaceId`、`PairId`、内部版本号或 enum 原值作为玩家文案。

返回工程主程做独立审阅并生成正式 Luban／绑定映射；随后由主测试组织独立 R。主策不自验收本次修订，也不请求或运行 R。

`QA-R-COPY-001-AMEND-03-FIX-01` 修订身份：独立 R thread `01a0e404-e8ee-7310-8388-9260babd53f1` / turn `01a0f3d2-340b-7c03-a494-ad14288b891b` / final `msg_0d29bf8d726d8dc0016abd669b7cb487d0baa6097a5dec2979` 给出限定 `NEEDS_FIX`。本次只同步 N-C12 的严格整数输入提示、N-C35 的观察／选择／核定区分、N-C13 的真实可达性；键数、81 项 disposition、旧 217 行前缀和其他产品决定不变。修订后交主测试送同一 R 限定复审。

## 10. COPY-001-AMEND-04：语言偏好保存结果未知

### 10.1 来源与限定处置

`QA-R-LOC-TOOL-01-DESIGN-R3` 由独立 R thread `01a0e404-e8ee-7310-8388-9260babd53f1` / turn `01a0f3e2-a494-7761-841c-43fa0da0b2cc` / final `msg_0d29bf8d726d8dc0016abd6bf24d8487d0a2c5926eb81df19f` 给出 `NEEDS_FIX`。四项 P2 中，本增补只承接 F2：原子替换已经发生后若复读失败，系统不能一概保证旧文件原字节仍在，必须区分确定失败与持久状态未知。

| source | disposition | key | 参数／严重度 | 限定 |
| --- | --- | --- | --- | --- |
| LOC R F2 | APPEND_KEY | `fm.language.save_unknown` | `errorCode` / `error` | 只用于语言偏好提交点之后无法证明 desired 或 old 的状态；不借用业务存档未知文案 |

本次在已接受的 263-key live CSV 末尾追加一个键，不插入或重排旧行。冻结的 UGUI 263-key 证据仍位于 `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/`，不得因本增补改变。

### 10.2 故障边界与重试合同

| 可证明事实 | 玩家文案 | session locale | 后续动作 |
| --- | --- | --- | --- |
| 原子替换前失败，未触及原偏好字节 | 既有 `fm.language.save_failed` | 继续使用新语言 | 只重试写入同一 desired locale |
| 替换后验证失败，但已验证回滚恢复旧字节 | 既有 `fm.language.save_failed` | 继续使用新语言 | 只重试写入同一 desired locale |
| 原子替换已发生，复读／解析／比对失败，无法证明 desired 或 old | 新增 `fm.language.save_unknown` | 继续使用新语言 | 幂等重写同一 desired locale，再复读核定 |
| 回滚失败，无法证明旧字节恢复 | 新增 `fm.language.save_unknown` | 继续使用新语言 | 幂等重写同一 desired locale，再复读核定 |

`fm.language.save_unknown` 不得借用 `fm.save_recovery.unknown`：前者只描述本机语言偏好文件的持久结果未知，不创建或恢复 PlayerSave、OperationId 或任何业务保存请求。`errorCode` 必须是受控诊断码；不得显示异常类型、堆栈、路径、原始文件字节或 locale 内部枚举。

本增补仍是英／简中制作稿及实现交接，不证明运行时绑定、原子写、复读、Luban、Unity、画面或设备结果。主策完成静态校验后交主测试组织独立 R，不自验收。
