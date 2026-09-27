# CONT-C-DESIGN-C1 同源导航与准备流程设计纠正

状态：DESIGN_COMPLETED（R1 作者设计纠正交付，原设计唯一 NEEDS_FIX 已收件；C1 独立 verdict 待原R，未授权实现）。

- 授权：system-task-packets.md §363～364；本轮只新写 demo-cont-c-design-c1.md 和 demo-cont-c-design-c1-scope.json。原 C：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确 C1 设计 turn：01a0e36f-c41b-7dd2-8c61-bc702a903506；gpt-6-astra / max；工程 D:/Unity/UnityProj/FightMatch。
- 审查者：原 R 01a0c1cd-dce1-7ac3-8780-06163cb0acfc；原R准确turn 01a0e35d-3ef5-7f82-8899-854646c131ba 于2026-09-27T15:12:53.238Z自然completed并给唯一NEEDS_FIX。C1复核turn由SD00在本轮准确自然completed及formal DESIGN_COMPLETED后另行续接，不预填旧等待turn；作者不代填ACCEPT。
- 计数保持 34 功能／31 已接收／余 3（CONT-C、028、029）／51 正向交付。五分钟自动跟进仍取消。本设计不启动 CONT-C 实现、028 或 029。
- 工作顺序及成功标准：核R1实际Copy/Preview/种类字段契约 → 同步修正Markdown/JSON及CC09/CC11的生产Draft输入路径 → 机械核原三稿/45输入保全和未变实施预算/派生集合。没有运行Unity、产品DLL或游戏测试，没有Git动作。

本C1继承原设计准确turn 01a0e2b4-870f-7f21-ac5e-48b253638bea 的已交付内容；仅纠正原R报告R1。原 design（46997 bytes／d82a00ac61b8925be6569264bcd5f23086655d375cf9b2e45a4a94d42bad5ecb）、scope（255111 bytes／a5fe61500e91c7ab9fef11fdd28f79c09a8b910e4f63cc4256ba2047f25af561）及R报告（11161 bytes／c70151dff4a527f248ea502c80de57f1a85ef2a973fd32351d04ea2577d850cf）全部只读。C1稿≤260物理行，C1 scope≤327680 bytes；不改变原8生产+5测试cs/13meta及141证据提案。

## 1. 起点、事实与同一产品范围

CONT-A 已接收，真实当前三槽来自 QueryRoster().Slots/FormationRevision，不取角色旧 OriginalSlot。025-P2C 已接收，PlayerSession 的精确内容解析、建档确认、正式 PlayerSave 信任门及 M02/M12 保存核是唯一生产入口。CONT-B 采用已接收 demo-cont-b-design-c1.md；原代码报告 demo-cont-b-code-review.md 的 NEEDS_FIX 是历史问题，后续 §359 已登记 CONT-B-CODE-C1 的独立 ACCEPT，不能把旧报告当最终 verdict，也不虚构一份 C1 R 文件。

§359 绑定的原 C1 完成 turn 为 01a0e1bd-5b28-7bc0-bd42-f86b25c95fd9，原 R 接收 turn 为 01a0e283-98f1-7f03-a1e0-3dc5235aff36。复用其 002 Compile exit 0、003 无 filter 全量 EditMode 3872/3872、0 fail、0 skip；005 的 8 项真实红测及 001/004 IPC 失败全部保留。本轮不重跑、不重新解释为 CONT-C 通过。776 实现、806 Assets、425 GUID、36 DLL/PDB 是该已接收交付的基数，本C1保护原45输入、原三稿及另列1项必要字段契约，不声称重新审查全部历史源码或 4897 个历史证据。

真实内容仍仅 level:ch01-01/version1 和 new-profile:default/version1；新档为原 W、L1、xp0、HP100、槽0，槽1/2空。没有已发布卡、技能/证书、配方或16关教学定义。页面必须显示真实空集合／NoPublishedDefinition，不把隔离“3+1→6”配方、Mage 或教学案例放进产品。CONT-C 仅提供同一 Android 产品的正常地图/冒险、队伍三槽、背包，以及准备→制作列表→配方详情→预览→明确确认→返回；Demo 内容数量由同一 Application 的已解析发布集合限制，没有 demo 分支、临时档或 UI 余额。

本轮下一实施提案包含：地图/冒险查询与选择；准备页；三槽明确编辑/确认；背包与装配/明确偏好详情；制作及其来源选择、确认、结果/恢复页。用卡、学习、教学事实照现有 QueryPermanent 只读展示；不在此小包新增这三类办理页面或教学发放按钮，不改其已接收内核。BeginTeachingGift 仍只能走原 PrepareTeachingEntry 的已确认合法事件。028 负责入场/重开/退出/真实结算确认及对应战斗流程；029 负责正式宿主、存储根、创建/恢复与整段产品连接。以上是明确交接，不把尚未实现的宿主或不存在的内容列为本包成果。

## 2. 实际 Interface 与最小补充

| 现有 Interface / 文件定位 | CONT-C 的准确消费方式 |
| --- | --- |
| PlayerSessionSystem.cs:26～104 | OpenExisting、PrepareNewRosterProfile/RecoverCreateIntent/ContinueCreate 由原宿主办理。导航不接 storage/locator/catalog，不另建 runtime，不用 PrepareNewProfile 偷换新档身份。CreationConfirmationRequired 时只交回原创建流程。 |
| PlayerRosterSession.cs:24、55、66、82 | QueryRoster 只读；PrepareFormation 返回冻结的 PreparedCandidateLifecycleRequest；阵容 Confirm 后交原 Lifecycle.Submit。PrepareFormationEntry 留给 028；PrepareRosterMigration 是明确的局外 2→3 技术事务，查询不迁移。 |
| PlayerPermanentSession.cs:9、43、71、93、119 | QueryPermanent 给只读定义、T/L/R/F、Held SourceChoices 和原操作；PreviewPermanent 冻结报价；ConfirmPermanent 只返回 Prepared，尚未提交。PreparePermanentMigration 明确 3→4；PrepareTeachingEntry 不作为导航/查询副作用。 |
| PlayerPermanentModels.cs:9～33 | 实际 Draft 字段为 ExpectedCommitId、Kind、CharacterId、DefinitionId、Quantity、Enabled、PreferenceRevision、StepId、SelectedInputs。Craft 的 DefinitionId 是 RecipeId、Quantity 是正整数批数、CharacterId 必须为 null，报价 CharacterId/ClassId 均为空；Equip/SetPreference 的 CharacterId 仍为明确目标；Equip 的 null DefinitionId 是明确空装配，Quantity=0；同物品 Quantity=0 与空装配不同。不得凭设计术语另造并行 DTO。 |
| CandidateLifecycleApplicationSystem.cs:65、103～131 | Submit/QueryOperation/Retry/Resolve/End 接同一 Prepared request；Submit 走内部可信 builder。公开 Application.Submit(intent,任意builder) 不得用于 PlayerSave。 |
| FightMatchDemoArchitecture.cs:63～81；CandidateApplicationRecovery.cs:222～278 | 原 Application.QueryOperation、Restore、ResumeObserved(commitId)、EndObserved(commitId)；沿原观测及精确绑定，不从页面反序列化或重建保存候选。 |
| CandidateApplicationModels.cs:62～112 | Phase、IsPublishedHeadVerified、PendingOperationId/CommitId、ObservedCandidateCommitIds、PublishedSnapshot；结果使用 IsCommitted、OriginalCommitId、OriginalLookup、LookupViewCommitId、Diagnostic/NotificationFailure，不能只比较 Code=="Completed"。 |
| CandidateLifecycleView.cs；CandidateDemoView.cs | 活动历史、Continuation、Battle phase、PresentationToken 与原可用性。多角色用集合/CharacterId；旧单值 getter 不能默取第一项。旧 CandidateDemoView.PreferenceWriting=NotImplemented 不替代 CONT-B 的永久偏好入口。 |
| CandidateBoardInputView/Controller；CandidateBattlePlaybackView/Controller | 复用已有战斗输入/演出拥有者；菜单不报告演出完成、不重建战斗、不改令牌。菜单关闭只清自己的事件/草稿；不能因移除旧演出树而触发其 Close/Finish。 |

已有公开查询未提供“当前 PlayerSession 精确发布关卡目录 + 全局库存 + 创建门”的一个导航视图；已有 ConfirmPermanent 也不为页面处理重复按钮和页面重建。因此提出两个最小新增 Interface，全部位于新增 Application 文件，不改既有 Core、旧 DTO 公开属性、序列格式或保存核：

1. PlayerSessionSystem.QueryNavigation(SaveCodecBudget) → 新不可变 PlayerNavigationReadResult。沿 runtime.PlayerAdmission/原 Ready 信任条件核实例、Active、PlayerSave、已核头；仅用 For(head.Business.Progression.Definition.Context).Definitions 取当前精确 Levels、Progression 定义/事实和 Inventory，只读投影当前 roster/库存/阶段/原因。用同一头的身份比较前后各项查询，发生重入或头变化整组 StaleContext，不拼接两个版本。它不读 catalog.current、不 Resolve latest、不取时钟/Guid/熵、不 Prepare 或写盘。
2. PlayerSessionSystem.GetNavigationSession() → 此 PlayerSession 所有、只创建一次的 PlayerNavigationSession。它只持 UI 路由、未确认草稿、一个冻结的确认记录及原 request/结果引用；不持可写业务状态，不拥有另一个队列或 store。Presentation 的实例重建仍取这个相同对象。旧 owner Dispose/WrongThread 后不可继续，另一个 PlayerSession 不能接走原 preview。

PlayerNavigationSession 的小 Interface 固定为 Query(codec)、Navigate(target,context,codec)、Preview(draft,context,codec)、Act(action,token,storeBudget)。target/draft/action 为封闭判别值，不是自由字典；Preview 的三种输入仅为 Formation（三个明确值）、Permanent（页面目标按§2.1共用生产构造路径转成Craft/Equip/SetPreference的既有实际Draft）和 Migration（明确2→3或3→4的技术提示）。Act 仅 Back、Cancel、Confirm、Return、Retry、Resolve、ResumeObserved、End、Refresh、SelectOriginalOperation；恢复/结果选择按 §6 的准确键。每次返回新不可变 PlayerNavigationView（Route、Context、可用动作及原因、原查询视图、确认记录/结果的只读投影）。token 只能由该 session 生成并带 owner/route/flow epoch；没有 public trust=true、可注入 builder 或可替换 catalog 的参数。

QueryNavigation 不新增游戏地图定义：地图/冒险是一页当前发布集合的关卡节点，节点由精确 ContentBinding + LevelId + LevelVersion 标识；边只呈现原 Progression.Levels 的 UnlockAfterLevelId/UnlockRuleId，缺显示名时显示原 LevelId，不从字符串 ch01-01 猜章节或制造 MapId。M05 开放/首通/挑战事实直接投影，不另写一套解锁计算器。进入准备是查看选中关卡，锁定关也可看准备说明；不是准入成功。原 Lifecycle.Enter 的全局拒绝原因可提前展示，其他显示 EntryCheckRequired；真正关卡锁、到期恢复和实际参与者由 028 调原 H02 检查，不能用 UI 的“已选择/已准备”作准入证明。

背包的 T/负载/活动 carry 全局视图取同一个已核 Inventory 及其原只读核；不需要虚构一个角色来查询余额。NavigationContext.SelectedCharacterId 只保留明确的查询/返回角色，传给原 QueryPermanent(characterId) 并参与展示、返回锚及失效检查；无此查询角色时仅可读全局余额，ActorSelectionRequired 是查询上下文原因，不是 Craft 事务需要角色。Craft 的实际 PlayerPermanentDraft.CharacterId=null，原报价 CharacterId/ClassId 均为空，不从上下文复制角色或制造受益人；Equip/SetPreference 继续传明确目标 CharacterId。缺定义与零余额分别保留 NoPublishedDefinition 和 InsufficientResources，不统称 Locked。

### 2.1 R1：查询角色与事务角色的唯一映射

| 操作 | NavigationContext.SelectedCharacterId / QueryPermanent | 实际 PlayerPermanentDraft.CharacterId | 原报价 CharacterId / ClassId |
| --- | --- | --- | --- |
| Craft | 保留明确查询/返回角色，传给 QueryPermanent，参与显示、返回锚和失效检查 | null；不从导航上下文复制角色，不制造受益人 | 均为 null，由原隔离核或日后合法正式内容的原 Preview 产生并验证 |
| Equip | 保留查询/返回角色；本次目标须明确并与确认上下文核对 | 明确目标 CharacterId；不得清空或暗换角色 | 原核从目标角色产生真实 CharacterId/ClassId |
| SetPreference | 保留查询/返回角色；本次目标须明确并与确认上下文核对 | 明确目标 CharacterId；不得清空或暗换角色 | 原核从目标角色产生真实 CharacterId/ClassId |

未来生产调用链固定为 PlayerPermanentDetailView/PlayerNavigationView → PlayerNavigationController → PlayerNavigationSession.Preview → PlayerNavigationSession 内部共用 BuildPermanentDraft → 原 PlayerSessionSystem.PreviewPermanent(actualDraft,codec) → actualDraft.Copy() → 原 CandidatePermanentProtocol.Preview。BuildPermanentDraft 放在原拟议 PlayerNavigationSession.cs 的350行预算内，仅把页面目标按种类构造成既有 PlayerPermanentDraft；不是新公开接口或新文件。Craft 写 null，Equip/SetPreference 写明确目标；ExpectedCommitId、RecipeId→DefinitionId、Quantity、SelectedInputs及已有偏好字段照原目标冻结，不改变原成本/来源/优先规则。不能在测试另写一个“正确构造器”，也不能要求旧 Copy/Preview/Core 为错误UI输入擦除角色。

未来 CC09/CC11 通过现有 FightMatch.Core.Tests 友元调用这一生产构造路径：给非空查询/返回角色，取得同一个 actualDraft，断言其 CharacterId=null，再把 actualDraft.Copy() 送入原隔离永久核，核报价 CharacterId/ClassId 均null、合法形状、原来源/份额及固定产出，之后才验证确认/返回转移。PlayerPermanentDraft 没有 ClassId 字段，不能伪造该字段的断言。真实 PlayerSession 首包缺配方仍只验证 NoPublishedDefinition；不向正式 session 注入隔离定义、不新增友元、公开信任后门或正式配方。

本项依据原 PlayerPermanentModels.Copy:21～33、PlayerPermanentSession.Query/Preview/Confirm:9～21/43～90，以及 CandidatePermanentProtocol.Preview:25～26/68～78/97～102 和 Shape:231～241。原 Copy 不按种类清 CharacterId；Craft 不是 hasCharacter 种类，报价 CharacterId/ClassId 必须同时为空。这里只核该必要字段契约，不重审历史 Core，不运行 DLL/Unity。本项的静态设计纠正已完成，生产构造函数与全部验收仍 NOT_RUN_DESIGN_ONLY。

## 3. Module 职责与宿主 Seam

PlayerNavigationSession 是保存交互次序的 Module；复杂性集中在一次确认、原操作续办和返回失效，供所有页面共用。PlayerSession/Core 仍拥有准入和业务真值。新 Presentation controller 只是 UI Adapter：绑定事件、调用上述 Interface、显示结果；VisualElement 不直接调用 M03/M04/M05/M02/PlayerSave，不计算配方成本或恢复进度，不把请求成功当已落盘。

Application 新值一律不可变并有集合/整数预算；新增类型可以有自身只读属性，旧 Begin/EntryCheck/持久 DTO 不加公开属性，避免重演 CONT-A 反射回放字节变更。运行列表沿 SaveCodecBudget/ExactMathBudget 的现有上限；三槽保持3，路由最多5层（Map→Preparation→Bag→CraftList→Detail），确认/恢复/结果是覆盖层。切主页替换路径，不无限入栈。数量用规范整数，无 float、夹到99或按 UI 格子裁输出；Equip 的99限制由原内核检查。

028/029 接收新只读 HostRequest：CreationRequired、BattleSelectionRequested、ResumeBattleRequested、SettlementRequired、RootBackRequested。它带本次 Context 和实际原头/关卡/活动/Continuation 引用，不带 fabricated 成功或新 OperationId。Query 只显示请求可用性，不自动发出；明确按钮的 token 只发一次。BattleSelectionRequested 只移交选择，028 重新核 Context 后在它的明确确认边界调用 PrepareFormationEntry、采原 LocalPlayerClock 样本并提交。CONT-C 不调用 H02、PrepareVictory、Exit/Restart、随机或结算；旧战斗保存请求的 Retry/Resolve 仍由原拥有者办理。

菜单作为同宿主的兄弟层；活动战斗页/演出拥有者在打开菜单时继续存活，取消未完成手势使用现有 controller 的 CancelGesture，不能为展示菜单 RemoveFromHierarchy/Dispose 原 playback。菜单返回原战斗只发 ResumeBattleRequested，由原宿主核当前 token/Attempt/Scene；不能重放原已完成演出。对象全关闭/低内存的既有行为归 027/宿主，不用导航逻辑重新实现。该宿主约束在实现测试可验证调用关系；真实场景连接及设备生命周期仍待 029。

## 4. 状态与转移

下表每步都先读当前原头；“允许”仅代表本导航步骤。业务提交仍须原内核再次验证。

| 状态 | 入口及可显示事实 | 明确动作 → 下一状态／效果 |
| --- | --- | --- |
| N0 Gate | 未打开/未确认创建/未核恢复/Disposed；可显示诊断，不显示可操作新头 | 创建问题发 CreationRequired；恢复问题去 N8；Disposed 清本地回调并拒动作。没有默认新档或临时玩家。 |
| N1 MapAdventure | 当前精确目录、M05事实及已选节点 | 选精确节点→N2；Team→N3；Bag→N4。主根 Back→RootBackRequested，一次通知，无保存。 |
| N2 Preparation | 选中关卡、3槽、角色/恢复及装配、EntryCheckRequired或现有拒绝 | Team→N3、Bag/制作→N4/N5；BattleSelectionRequested→028。Back→N1，仅取消未确认页面选择。 |
| N3 Team | QueryRoster 的持有集合/3槽；当前选择和本地草稿分列 | 设置三个明确值（允许全空）→N6 Formation；Confirm 才 PrepareFormation→Submit；Cancel/Back 丢草稿回父页。换位不暂存重复角色，不自动补位。 |
| N4 Bag | 原全局 T、各角色 L、活动 R、F 及用途原因；可选角色 | 制作→N5；明确装配/偏好→N6 Permanent详情。查看不卸装、不转移 L/R、不补来源。 |
| N5 CraftList | QueryPermanent 同头的 RecipeAvailability/定义记录 | 已有准确 RecipeId/recordVersion→N6；首包显示 NoPublishedDefinition，禁预览/确认；Back 返回其真实父页。 |
| N6 Detail/Edit | 精确配方/装配目标、批数、Held 输入选择、三槽草稿或明确迁移提示 | Preview→N7，失败留本页并显示原 Code/FieldPath；Back→上一页；Cancel 结束此未确认流程，回 ReturnAnchor。不分配消费操作。 |
| N7 Confirmation | 永久报价原成本/全部份额/产出、或三槽前后值，显式“确认” | Confirm 消费 token 一次→先准备再提交；成功→N9，pending→N8，准备拒绝→N6并使旧 token 失效。Back 仅回 N6 丢本报价；Cancel 回锚。 |
| N8 Recovery | 保留的已提交视图（可能是旧头，按IsPublishedHeadVerified标明当前是否核定）+原 op/candidate/诊断；不显示推算的新余额 | 仅按 §6 Query/Retry/Resolve/ResumeObserved/End/Refresh 或交原创建/战斗拥有者；新业务关闭。Back/Cancel 留恢复事实与请求，不能取消保存。 |
| N9 CommittedResult | 原 IsCommitted/OriginalLookup、原 Commit、受影响身份/收据与当前查询头分别显示 | Return 一次→N10；重复 Confirm/Return 返回同一结果或拒过时 token，不能再扣或再弹栈。通知回调失败不当保存失败。 |
| N10 ReturnRevalidate | 取真实最新头，重核 ReturnAnchor | 锚仍合法→N2/N3/N4/N1并显示原结果摘要；身份/绑定不合法→N0或N1；活动/S17/pending优先→N8或对应宿主请求。绝不恢复旧余额/旧阵容。 |

完整制作链：N1 选关→N2→N4→N5→N6（选准确配方、批数及必要份额）→N7→N9→N10→原 N2。完成前保持已提交的旧业务视图；成功返回后的准备/背包重新查询，显示实际新 T/L/R/F，原配方成本/结果只作为该次历史回执。若没有正式配方，链在 N5 的真实空状态终止，不能用成功动画补齐业务。

从 Bag 根进入制作时 ReturnAnchor=Bag；从 Preparation 子流进入时锚=该 Preparation；从 Team 装配进入时锚=Team。Back 按当前路由退一层，Cancel 结束当前未确认子流直接回锚，Return 仅消费已确定结果。导航到其他主根必须先使未确认报价失效；同一已确认请求存在时切根只显示 N8/N9，不静默清掉请求。根/父页不是无限浏览历史，任何跨玩家旧路径全部丢弃。

## 5. 上下文键、冻结与失效

| 层 | 必需键／来源 | 失效及处理 |
| --- | --- | --- |
| 会话 | 内存 owner 引用/epoch、PlayerId、SavePurpose=PlayerSave、创建确认状态 | 新 owner/玩家/用途/Disposed →拒 StaleNavigationContext 或原信任原因；旧 preview 不迁入新 owner。owner/epoch 不写入存档，不代替创建 proof。 |
| 同源头 | CommitId、SaveGeneration、descriptor 长度/SHA、原 ContentBinding 五字段、Business Format | 任一相关头/绑定变化→未确认报价失效；新下载 V2而当前精确V1未变不失效。只读页可刷新，Confirm 不默换成本。 |
| 选择／返回锚 | Route、父链、ReturnAnchor、LevelId/LevelVersion/完整绑定、选中 CharacterId、flow epoch及draft revision | 错行、改批数/来源/目标、换角色/槽、切主页→旧 token 失效。同源自提交后的锚可保留，但必须用新头重建上下文，不把旧 ExpectedCommitId继续使用。 |
| 角色／阵容 | FormationRevision、明确3槽、所有相关 CharacterId/StateRevision/恢复事实；不用旧 OriginalSlot | 相关修订变化需重新编辑/预览；恢复倒计时显示不推进业务时钟；只在028原H02/原恢复请求应用时间样本。 |
| 永久报价 | 原 PreparedPlayerPermanentPreview/Quote 的全部定义版本、M03/M04/M05修订、PreferenceRevision、用途/教学依据、Held来源身份/区间、成本和固定输出 | 源端点已耗/用途变化/不完整证明→原 StaleContext/FirstCertificateReserved/UnsupportedSourceProof 等；禁止拿 F 数量替代证明，禁止 UI 重排选另一批。 |
| 活动历史 | ChallengeId、AttemptId、EntryBaselineId、HistoryAnchor/SceneRevision（存在时）、Continuation.ReservedOperationId、演出 token（观察值） | 回准备不改变战斗历史。活动/S17/令牌变化触发新门控；永久提交造成 Commit 改变但 Attempt/Scene 相同，不能清或重放原 token。 |
| 确认之后 | 确认序号、同一 Prepared request/完整 Intent canonical bytes、OperationId、ExpectedHead；进入 runtime 后的 candidate/commit/ticket仍归原runtime | 先查原请求/结果，再判断新的业务条件；绝不以新头不同、材料归零或已装配为由重新 Confirm。 |
| 已提交结果 | 原 OperationId、OriginalCommitId、OriginalLookup、LookupViewCommitId、当前 view CommitId | 原结果与最新头分开；旧操作成功不能把当前头倒退。未能核对原结果时仍待恢复，不拼装 UI receipt。 |

导航原因 StaleNavigationContext、ActorSelectionRequired、EntryCheckRequired 是新 UI/编排诊断，必须与原业务 Diagnostic 分字段呈现，不能把它们伪称 Core 返回码。原错误 Stage/FieldPath/Limit/RequiredAtLeast/Allowed/NotificationFailure 不吞并成一个“失败”。容量预算不足是 Limit，缺定义是 NoPublishedDefinition，真正关卡锁才显示 Locked。

一次确认的具体顺序：检查 token 的 owner/flow/draft → 若该确认已有 prepared request 或 terminal result，直接返回/查询原记录 → 否则把本 token 置为正在消费（阻重入）→永久调用一次 ConfirmPermanent，阵容调用一次 PrepareFormation → 立即把成功返回的原 request 放进 session 的一个槽 →调用 Lifecycle.Submit。不能等 Submit 成功才保存 request，也不能为了刷新按钮再次 Prepare。Migration 的预览只显示原格式、目标格式和原descriptor，不生成请求；Confirm才调用对应的原PrepareMigration。Formation 的页面预览只是本地前后值比较；实际合法性由 Confirm 时原 Prepare/Submit核验；它与 CONT-B 不分配ID的业务 Preview 明确区分。未准备成功的拒绝没有业务效果；修正输入后必须显式重新预览生成新 token。

来源继承 C1 全部修正：显式卡仍必须满足完整 Held 的广告/取得顺序政策（本包无新用卡页）；首证用途不能因返回/装配/制作绕过；Craft 的合法材料选择不套卡优先；Equip 的 null/0/Enabled=null 与“同物品L0保留偏好”严格区分；M03顺序、M04/M05历史次序及学习业务唯一键不改。普通 AlreadyLearned/Unchanged 只显示原结果；合法“赠物前已学习→引用旧学习补M05教学步、零M03/M04成本”仍是既有明确例外，导航不制造或删除这一步。

## 6. 保存、重试、结束与对象重建

| 动作 | 唯一语义／前置与结果 |
| --- | --- |
| Back | 未确认时退一层并废弃该层报价；确认后只切到保留请求的恢复/结果视图，不调用 End/Exit/Restore，不消耗新操作。旧 Back token 不能连续退两层。 |
| Cancel | 未确认子流放弃到 ReturnAnchor；确认请求已交出时不能撤销，用结果中的“保存处理中”明确阻止。不同于 End，也不同于战斗退出。 |
| Confirm | 用户确认当前完整目标/成本，只消费该 token 一次。已经准备出的 request 不重新生成；未验证持久完成不显示制作完成/新阵容。 |
| Retry | 有本 session 原 request 且 PendingOperationId匹配：Lifecycle.Retry。PendingPreparation 的无 ticket 情形也允许原 Retry；SaveFailed 也只写原票据。即使 UI状态误判，runtime 自己先核原结果/未知写结果；不能重跑 builder/Preview。 |
| Resolve | 有原 request 的 CommitUnknown/RestoreRequired等：Lifecycle.Resolve；只查原票据/恢复真实头，ConfirmedNotCommitted 转可 Retry 的 SaveFailed，仍不分配新ID。无 ticket 的 PendingPreparation 不以反复 Resolve 冒充进展，需原 Retry。 |
| ResumeObserved | 没有内存pending且有原 observed commit：选准确 commit 后 Application.ResumeObserved。它会恢复并续写原候选，不是纯查询；按钮须写“继续原保存”，一次点击锁住恢复动作，不能遍历全部候选自动提交。 |
| End | 独立的“结束未完成保存”确认，不是页面 Cancel。内存原请求调用 Lifecycle.End；磁盘观察调用 Application.EndObserved。CreationPending 禁止；S17/其他业务的保留结算不能被本菜单创建/删除。只由原M12决定能否结束；AlreadyCommitted 回原恢复/成功，未知保持N8。不得承诺回滚已提交消费。 |
| Refresh | Query 为只读刷新。需要重新读物理恢复观测时明确调用原 Application.Restore，保留原pending优先；不自动迁移/重建新档。对创建状态交 ContinueCreate，不用通用恢复绕开确认。 |
| SelectOriginalOperation / Return | 从已核 snapshot.Records 或 QueryPermanent.OriginalOperations 的准确原 record 取原 Intent 再 QueryOperation；不能自己按OperationId重新构造意图。Return只消费显示结果，不生成保存。 |

End的提示用 Navigate(EndConfirmation,context)进入N7的专门分支，只显示原op/candidate及不可撤销已成结果的含义；该分支只有带原确认token的Act(End)可执行结束，Cancel/Back只回N8且保留原请求，不走业务Confirm。Ended之后仍须检查真实恢复view；若仍RecoveryBlocked/RestoreRequired则留N8，不能仅凭Ended字样开放新业务。

Retry/Resolve/Submit 的成功以原 IsCommitted/OriginalLookup 及已核头为准。同 ID 同 canonical intent 应先查到原结果，同 ID异意图仍 OperationConflict；“材料现在不足/已学/关卡已变”不覆盖原结果。NotificationFailure 只说明发布通知失败，须重查实际头，不能重办已提交业务。ResumeObserved 返回 ObservedAlreadyCommitted 时可能没有 OriginalLookup；用所选准确 commit 在恢复后的原 Records 查原 Intent，再 QueryOperation；找不到唯一原记录就保留恢复诊断，不选“最后一条”补结果。

页面 VisualElement/controller 的重建：取消自身输入/解除回调，保留 PlayerNavigationSession、原 request、flow epoch、ReturnAnchor；新 view重新 Query。延迟回调携旧 UI epoch 直接拒绝，旧对象不能提交/弹两次栈。队列属于原runtime，同一session最多一个本导航已确认流程；有其他战斗/创建操作占用时只展示 owner/原因，绝不接管其request或调 End。

完整 Application/PlayerSession 对象重建：不保存路由 JSON、PlayerPrefs 或另一份候选。走原宿主 F2/OpenExisting/Restore；observed candidates 用原 ResumeObserved/EndObserved；已提交原结果从原Records准确取回。已核候选中的 OperationId、canonical bytes、来源/份额/固定效果和commit不变。ReturnAnchor作为内存UI提示可丢失；丢失时回 Map（或先恢复/结算门），不猜原关卡/角色。结果列表可人工选择真实原操作，不能把恢复后最后一项自动认为“刚才那次”。

内存中尚未形成任何持久候选的确认请求不能保证跨进程复原，这是原保存协议的边界：页面重建必须保留它，真正进程丢失后只报告完整原观测中的实际事实；没有原候选/记录时不宣称成功、不自动换ID重发。只有恢复核明确排除未决后，用户才可重新明确预览/确认一项新请求。该窗口不新增UI日志或第二种事务持久化，强进程崩溃仍 NOT VERIFIED。

## 7. 状态门与迁移交接

| 已核状态 | 查询/新操作/返回 |
| --- | --- |
| Active + Ready + PlayerSave + verified、无pending/observed/Continuation | 允许正常导航；每个操作仍核定义、格式、角色、用途/修订。Query/Back/Cancel全程零业务写。 |
| AwaitAction/AwaitLinks/AwaitRescue 有活动历史 | 地图、队伍、背包可读；新入场/改阵容/Equip拒 ActiveAttemptConflict或原精确原因。Craft和明确SetPreference按CONT-B合法F继续；卡/学习内核的原许可不改。制作后只改长期资料，当前HP/Stats/carry/PRD/Scene/RNG/Restart基线不动。 |
| 演出 token 存在 | 合法永久请求仍串行；不清/替换token。菜单没有“确认演出”按钮，不能通过换页重放/完成演出。 |
| WonPendingSettlement / Continuation（S17） | 丢未确认报价，保留原FinalReport/ReservedOperationId；显示 SettlementRequired→028。新永久/阵容/普通入场关闭；原操作查询仍允许。不自动 H06，不把 Back当结算。 |
| PendingPreparation / SaveFailed | 原已提交视图只读，当前本菜单request可Retry；别的owner只交回原owner。不以无PendingCommitId推断无操作。 |
| CommitUnknown / RestoreRequired / observed candidate | 原 Resolve/Restore/ResumeObserved；新业务关闭。未知不变失败，失败不变成功；原物理证据保全。 |
| RecoveryBlocked / 原精确包或历史root缺失 | 显示实际 UnsupportedBinding/RecoveryBlocked等；保留证据，不切latest、不清档、不建新PlayerId。 |
| 未打开 / CreateIntentRecorded / CreationConfirmationRequired | 仅原创建/确认流程；头已提交但尚未Active也不能办理菜单写或 End/EndObserved。Active定位器锚原Initialize，重复确认不能倒退当前头。 |
| schema2 | 可读；新集合页显示RosterMigrationRequired。明确技术迁移交现有PrepareRosterMigration，活动/S17/pending先按旧格式办完；无自动2→4。 |
| schema3 | 三槽正常；永久办理需明确PreparePermanentMigration 3→4；它可在合法活动头办理，不重写旧战斗。新档原FMPROF02仍先v3；不产生FMPROF03或新配方。 |
| schema4 | QueryRoster保持Format=v4/IsMaterialized=true；正常永久处理。迁移结果/原操作重查不回写旧记录为latest。 |
| WrongThread / Busy / Disposed / Limit | 原诊断传播；新token/按钮不能放宽runtime门。Busy解除后刷新上下文，已确认request仍原样。 |

技术迁移不新增日常产品功能页：由同一会话的明确升级提示生成一次确认 token，调用现有 PrepareRosterMigration 或 PreparePermanentMigration，再沿同一确认/保存槽；每步提交成功后重新查询，不把两个迁移合成一个新事务。旧活动schema2须先交028结束/结算。未发布配方仍缺失，迁移成功不解锁配方、技能或赠物。

## 8. 拟议下一实施包的有限白名单

下表全是提案，当前写权限仍只有本设计两文件。新增位置均在既有目录，不新增目录meta/asmdef或场景。生产仅新增8个cs；测试新增5个cs；每个新增cs对应唯一自然.cs.meta，共26项新Assets。旧生产/旧测试全部不改。不得因方便增加测试友元、测试断言例外或反射私有字段伪造正式会话。

| 新文件（相对项目根） | 物理行上限 | 职责 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Application/PlayerNavigationModels.cs | 260 | 封闭路由/上下文/token/不可变查询及操作结果；不加旧DTO属性。 |
| Assets/Scripts/FightMatch/Application/PlayerNavigationQuery.cs | 220 | PlayerSession partial：精确发布查询及所属导航session；沿原信任门，无catalog写权。 |
| Assets/Scripts/FightMatch/Application/PlayerNavigationSession.cs | 350 | 路由/草稿/一次确认/返回；原Prepare/Submit组合，不复制业务计算。 |
| Assets/Scripts/FightMatch/Application/PlayerNavigationRecovery.cs | 240 | 同一navigation partial：原结果/Retry/Resolve/Observed/End及对象重建门。 |
| Assets/Scripts/FightMatch/Presentation/PlayerNavigationController.cs | 180 | 单一session调用、UI epoch、订阅/解除、HostRequest转交。 |
| Assets/Scripts/FightMatch/Presentation/PlayerNavigationView.cs | 250 | Map/Preparation/Team/Bag/CraftList的真实查询UI Toolkit树；不带固定业务数据。 |
| Assets/Scripts/FightMatch/Presentation/PlayerPermanentDetailView.cs | 220 | Craft/Equip/SetPreference明确目标、份额、报价与确认显示。 |
| Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs | 140 | 原保存诊断、明确恢复/End与结果/Return控制。 |
| Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs | 280 | 有界共用夹具：原真实首包/F2/storage故障；隔离数据明确标识且无正式写权。 |
| Assets/Tests/EditMode/FightMatch/PlayerNavigationSessionTests.cs | 350 | 真实session信任门、同头查询、路由/阵容/迁移/失效。 |
| Assets/Tests/EditMode/FightMatch/PlayerNavigationPermanentFlowTests.cs | 360 | 真实缺定义与隔离永久结果的共用导航转移、输入/返回/一次性。 |
| Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs | 420 | 原保存失败/未知/Observed/原结果/对象重建组合。 |
| Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs | 260 | 同controller/view事件和关闭/重绑，区分回调测试与物理输入证据。 |

新生产合计1860行、新测试1670行；13个meta各≤12行、合计≤156行，GUID均新且唯一，旧425个meta逐字节保持。不能借总预算在未列文件加行。未来实现计数提案为802实现/832Assets/438GUID，36 DLL/PDB集合沿原定义，仅身份随新编译更新；不是当前已存在数量。若预算不足先给具体差异由SD00另签，不压长行绕过上限。

唯一拟改旧文件是 Tools/Invoke-FM025P2Validation.ps1：本包新增/删除合计≤36行，最终≤318物理行。只增 CONT-C 阶段、设计scope/C1起点身份、精确路径集合/turn与root核对、8槽及原进程身份采集；其余各Stage/模式、SHA自检、串行/noEditor门、参数、退出码/XML/failure分支不改。现C1累计194/旧绝对200，因此提案累计上限230，必须由SD00在未来实施包明确补签这个准确例外；本设计不视作已获工具预算提升。不能另写绕过固定工具的脚本。

未来正式交付只提案 demo-cont-c-delivery.md≤300行、demo-cont-c-code-scope.json≤8MiB；原R独立代码审查 demo-cont-c-code-review.md≤260行。设计本文件、scope及未来R设计review均保护；三协调稿只允许其已有独立写者更新。§360本轮不创建这些文件/新证据根。

## 9. 可执行验收与证据分级

“实会话”使用原首包、原catalog/F2/PlayerSession；“隔离”沿已接收普通来源/永久Build→M02规范协议→M12内存保存核，不能伪造M10审定、反射设置私有profile/publications或把测试输入注入正式PlayerSession。新增导航Module对查询/报价/准备结果/保存结果的状态转移必须是生产同一路径；内部纯转移Seam可由既有friend测试调用，用真实内核产出的不可变结果验证，不设置万能成功开关或产品fallback。新模块内部Seam限定为接受已核查询/preview的转移、AcceptPrepared(token,request)和Receive(token,applicationResult)，由生产PlayerSession编排直接调用；它们只验证owner/flow/原Intent关联并更新UI状态，不能调用store或创建业务结果。隔离测试用真实内核的冻结结果走这些相同转移，实会话Formation覆盖实际Prepare→Submit编排与故障路由。实会话接口组合与隔离转移测试分开统计；隔离正例不证明正式配方发布。

| 编号 / 状态 | 证据类型 | 必须可核对的结果 | 拟验证位置 |
| --- | --- | --- | --- |
| CC01 / 边界/同源 | 实会话＋静态 | N0～N10只消费同一PlayerSession/Application；六首包、298字节新档配方和原W/L1/三槽身份不变；无demo分支/临时档/页面业务写。 | PlayerNavigationSessionTests.cs／范围核验 |
| CC02 / N0/F2 | 实会话 | 原记录未候选、候选未知、头已成待确认、Active四态分别进入原创建/恢复/Ready；坏记录不当Absent；未确认新写/End/EndObserved全拒，重复确认不倒退当前头。仅补导航门断言，原F2矩阵沿3872回归。 | PlayerNavigationSessionTests.cs |
| CC03 / N1/Query | 实会话 | QueryNavigation准确取当前head的For绑定；下载V2而仍用V1不换目录；Query/Refresh/查看/返回零ID/熵/写入/时间推进；混合两头投影拒StaleContext。 | PlayerNavigationSessionTests.cs |
| CC04 / N1→N2 | 实会话 | 只列原精确LevelId/Version及原Progression关系；篡改目标绑定、缺定义不进入可确认状态；锁定关可读但准备不冒称H02成功；不猜MapId/关卡16或补角色。 | PlayerNavigationSessionTests.cs |
| CC05 / N2/交接 | 实会话 | Preparation显示真实3槽/恢复/负载；BattleSelectionRequested同token一次、携原精确上下文，零H02/时间采样/熵；Active/S17按原因转原宿主，不能普通新入场。 | PlayerNavigationSessionTests.cs |
| CC06 / N3/Formation | 实会话；多角色为隔离 | 本地三槽编辑/Back/Cancel零写；全空不补位；Confirm经原PrepareFormation/Submit后才变化；重复/未持有/错revision全拒，合法明确设置可查回Unchanged。真实W移槽/清空；多人换位不得伪称首包多人。 | PlayerNavigationSessionTests.cs |
| CC07 / N4/Bag | 实会话 | 全局库存及各角色负载来自同头，无角色时可只读余额；装配/偏好须显式角色；空装配与同Item L0不同；不借他人L或活动R、不自动卸装。原数量语义复用已接收C1回归。 | PlayerNavigationSessionTests.cs |
| CC08 / N5/空内容 | 实会话 | 真实首包RecipeAvailability/Preview显示NoPublishedDefinition；零余额与未发布定义分开；不能出现隔离配方、Craft确认成功、虚构输出或教学赠物。 | PlayerNavigationPermanentFlowTests.cs |
| CC09 / N6/制作 | 共用生产Draft输入＋隔离同核＋真实拒绝 | 查询/返回上下文SelectedCharacterId非空时，调用页面共用的生产BuildPermanentDraft构造实际Craft PlayerPermanentDraft，断言CharacterId=null且ExpectedCommitId/RecipeId→DefinitionId/Quantity/SelectedInputs保持原意；将这一个实际Draft经原Copy送CandidatePermanentProtocol.Preview的合法隔离定义/普通来源，断言报价CharacterId/ClassId均null、原合法形状/来源及固定产出成立。保留CostSelectionRequired、合法明确份额、完整成本/输出显示及零UI重算；不得以手填正确报价后Receive/AcceptPrepared替代这段输入验证。真实PlayerSession缺配方NoPublishedDefinition单列且不注入正式配方。 | PlayerNavigationPermanentFlowTests.cs |
| CC10 / N6/N7取消 | 实会话＋隔离转移 | 从Preparation/Bag/Team三种锚各走Back、Cancel；退层数准确，未确认零提交；旧报价token失效；重复Back/Cancel不多退层，已确认Cancel不丢原request。 | PlayerNavigationPermanentFlowTests.cs |
| CC11 / N7/一次确认 | 实会话Formation；制作生产Draft→隔离核→转移 | 保留实会话Formation的双击/同步重入/重复回调至多一次成功Prepare/原Submit、先存原request后提交及旧owner/flow/token拒绝。制作分支从非空查询/返回角色开始，走页面共用生产BuildPermanentDraft→同一实际Draft.Copy()→原隔离永久核，先断言实际Draft.CharacterId=null、报价CharacterId/ClassId=null和原合法来源/固定产出，再测确认槽及AcceptPrepared/Receive一次消费；已有request不再ConfirmPermanent。只手填正确报价/请求的后置纯转移不能满足本项；真实PlayerSession缺配方拒绝另列，无正式配方注入/新测试友元/信任后门。 | PlayerNavigationPermanentFlowTests.cs |
| CC12 / 版本/失效 | 实会话＋隔离转移 | 确认前更改头/目标角色/三槽/批数/份额/相关owner修订/用途使旧token失效；调用后改原列表不改被冻结值；V2仅下载不影响V1；确认后头变化仍先查询原operation，不能再报价。 | PlayerNavigationPermanentFlowTests.cs |
| CC13 / N9→N10 | 实会话Formation；制作为隔离同核 | 原实际IsCommitted/Lookup/Commit到达才显示结果；返回原Preparation或Bag/Team后重查当前头；结果只留历史摘要，新T/L/R/F由query得；Return双击只消费一次，锚失效回合法根。 | PlayerNavigationPermanentFlowTests.cs |
| CC14 / N8/Prepare与Write失败 | 实会话阵容/迁移＋原M12故障 | Prepare失败无ticket仍保留原request并允许原Retry；明确Write失败保留原op/candidate/来源/bytes，Retry不重Prepare/build；当前已提交视图全旧或完整新头。 | PlayerNavigationRecoveryTests.cs |
| CC15 / N8/CommitUnknown | 实会话＋原M12故障 | marker前/后及回执丢失分别Resolve；已成取原结果、ConfirmedNotCommitted才可原Retry，未知保持N8；不得用新ID/份额/时间绕过。故障证据与对象重建不称真实崩溃。 | PlayerNavigationRecoveryTests.cs |
| CC16 / 页面重建 | 实会话 | 销毁/重绑view和controller，GetNavigationSession仍同owner、保留冻结请求与锚；旧UI epoch回调零效果；Back/Cancel/Detach不End、不再次Submit，不丢pending。 | PlayerNavigationPresentationTests.cs |
| CC17 / Application重建/Observed | 实会话＋原M12故障 | 重建原Application/PlayerSession后先Open/Restore；显式选择原Observed commit续办，原intent/commit/bytes保持；多候选不自动逐个提交。路由内存丢失回Map，缺原候选/包保持真实诊断，不推测原操作。 | PlayerNavigationRecoveryTests.cs |
| CC18 / 原结果优先 | 实会话＋隔离同核 | 保存后回执丢失、当前头后移、余额变化时从原Records选准确Intent查询原op/Commit；与LookupViewCommit/current头分列。ObservedAlreadyCommitted无Lookup时按准确commit反查，禁止取最后一条假冒；同ID异意图仍冲突。 | PlayerNavigationRecoveryTests.cs |
| CC19 / End与Cancel | 实会话＋原M12故障 | End需独立确认；原未成才结束，已成返回原完成，未知不结束；F2禁止，其他owner不接管，S17不删保留结算；Cancel End提示仅回N8。Ended且恢复失败不能释放成Ready。 | PlayerNavigationRecoveryTests.cs |
| CC20 / 活动战斗门 | 实会话拒绝；合法制作隔离同核 | 活动期间Map/Bag只读、阵容/Equip/新入场关闭；Craft和明确SetPreference沿原准入。永久提交不改入场成员/Stats/HP/RNG/carry/Restart基线；缺正式内容仍拒，非成功代用品。 | PlayerNavigationSessionTests.cs／PlayerNavigationPermanentFlowTests.cs |
| CC21 / 演出token | 实会话＋现有presentation | 同Attempt/Scene的永久提交或导航Query不清原token、不重放；菜单Attach/Detach不调用原playback Close/Dispose/ReportPresentationCompleted；旧战斗输入取消手势归原controller。 | PlayerNavigationPresentationTests.cs |
| CC22 / S17/028 | 实会话 | Continuation/ReservedOperationId/FinalReport保留；进入菜单只显示SettlementRequired，明确交接同token一次；零PrepareVictory/H06/Exit/Restart，原operation结果仍可查。 | PlayerNavigationSessionTests.cs |
| CC23 / 明确迁移 | 实会话 | schema2先原局外2→3，schema3明确3→4，各自确认/原request/提交；Query/Back不迁移；旧活动2不强退，合法活动3可原迁移；v4 Format/IsMaterialized及原创建锚不变。 | PlayerNavigationSessionTests.cs |
| CC24 / 错误/信任 | 实会话 | 缺原精确包/root、恢复不完整、错误player/purpose、WrongThread/Disposed/Busy/Limit均传播原原因，按钮不能绕门；任意builder仍正式拒且回调0；只读数据不授写权。 | PlayerNavigationSessionTests.cs |
| CC25 / 通知失败 | 实会话＋原M12结果 | IsCommitted已真而NotificationFailure存在时只刷新真实头/原结果，不能Retry为新业务；Closed view未收到回调后重绑仍得准确原结果。 | PlayerNavigationRecoveryTests.cs |
| CC26 / UI Adapter | EditMode实际view/controller回调 | 按钮名/稳定行键绑定精确Level/Character/Recipe/source元组，多个来源不按当前数组下标换身份；回调调用生产Interface，关闭解除自身订阅且不销毁session；所有拒绝/空集合可见。事件回调测试不冒称物理鼠标/触控。 | PlayerNavigationPresentationTests.cs |
| CC27 / 预算/纯转移 | 静态＋同Module测试 | 路由≤5、一个确认槽、原集合/规范整数预算；负数/极大数/超预算零业务效果，不能float/clamp/重复新建队列；内部纯转移Seam与生产调用一致且不能授PlayerSave权限。 | PlayerNavigationPermanentFlowTests.cs |
| CC28 / C1继承 | 仅未来规定全量回归 | 原3872具名多重集合含卡优先/首证、空装配、M03顺序、学习唯一键及合法教学补记完整保留，不改旧测试/来源/配方/首包；不为本设计额外重跑这些已通过核验。 | 原3872／范围核验 |
| CC29 / 宿主/Android边界 | 静态计划；物理项NOT VERIFIED | 同Application/Presentation源码，无Android业务分叉；移交028/029的HostRequest语义可测；实际场景、设备Back/触控/像素/生命周期/Player与正式配方整链单独留待准确授权证据。 | PlayerNavigationPresentationTests.cs／宿主交接 |
| CC30 / 交付/有限证据 | 未来实现验证 | 获准工具例外后固定CONT-C Compile→无filter全部EditMode，原3872＋新例全通过0skip；同源802实现/832Assets/438GUID/36DLL及预算核对，8槽141路径实际manifest/所有失败保全，无新脚本或越界写。 | 固定工具/XML/scope |

每项未来验收状态均 NOT RUN。正向制作的“页面→真实已发布配方→正式PlayerSave”当前没有合法内容起点，明确 NOT VERIFIED；不能把隔离流程称正式产品全链已完成。实施包可接收真实空内容路径、已核编排和既有内核的隔离组合；将来发布者正式提供配方后，须以该准确发布身份补实会话制作整链，而非本包偷增内容。若R认为这影响本包可接收范围，应在设计verdict明确指出，作者不自封完整Demo。

## 10. 未来验证入口、有限证据与不重复原则

当前仅静态设计核对。未来设计ACCEPT且SD00签发源包（包括§8精确工具预算例外）后，原C串行使用同一固定工具的 CONT-C Compile → Tests；先查没有Unity进程占用，Tests不加-quit、不加filter，Unity2022.3.18f1。不先重复已有3872基线；新源码固定后一次最终编译和全量验证，有失败/源码变化才按必要范围补跑。许可/IPC失败仅登记环境失败，不计代码失败或通过，不能覆写旧槽。

唯一拟议证据根 TestArtifacts/FMDemoCONT/cont-c；16个根级元数据叶、1份旧工具before-text、27份新文件/工具after-text、runs/001～008每槽12个允许叶，再加read-manifest自身，精确141路径在scope逐项列出。槽叶为 before.json/run.json/after.json/result.json、compile.log/compile.stdout.txt/compile.stderr.txt、tests.log/tests.stdout.txt/tests.stderr.txt/tests.xml、failure.json；它们是写入并集，不要求伪造未产生文件。覆盖固定工具的Compile/Tests成功、实际失败与HarnessFailure；manifest只列实际叶并排除自身、≤140项，根实际≤141。preflight在建槽前拒绝不伪造PID/结果。槽用尽交SD00，不静默扩容。

每次实际run保留参数、PID/时刻、真实exit、源码/Assets/工具/DLL前后身份及原log/XML/所有失败；最终compile后=tests前=tests后=交付。原3872个fullname多重集合（3867不同名）逐项保全，新例另列，0fail/0skip；不能仅比较总数。Source/GUID预算及旧源码/测试/首包/298字节新档配方、原C1两报告/root/manifest和历史根均保留；沿已接收有限manifest核验，不重跑旧运行。三协调稿由独立写者可能变化须before/after单列，不能作为覆盖旧证据的例外。

## 11. 同一Android路线与未验证边界

CONT-C 的 UI Toolkit视图和同一个 PlayerNavigationSession 在编辑器/Player/Android共用，无 UNITY_ANDROID 业务分支、平台特供配方、测试资源fallback、编辑器菜单存档或不同SavePurpose。代码接收后仍要由029把同一组对象挂入真实宿主，原028处理战斗生命周期。未来设备证据必须绑定该次准确源码/内容/构建身份和实际Player日志，不以编辑器绿测替代。

物理证据计划：真实鼠标/触控按 Map→Preparation→Team三槽编辑/Cancel→Bag→CraftList 空内容返回，核画面/焦点/连续两次点击/系统Back/旋转或前后台导致重绑时的实际行为；在获准合法配方发布后再跑 Detail→Confirm→持久完成→返回实际Preparation，并重开同一档查原结果。PlayMode/Player/Android分别登记宿主、构建/设备、实际输入、日志与像素证据；没有运行的项保持NOT VERIFIED，不预建证据或编造命令。本包不发明目前未定义的Player/Android build命令，也不强杀进程制造保存故障。

继承 NOT VERIFIED：物理鼠标/触控/像素、交互式PlayMode、Player/Android真机、公开AwaitLinks可达性、未覆盖强保存故障/真实崩溃、广告发放/云同步、正式新增配方/技能/教学内容及§184原边界。既有内存M12故障只证明对应故障点和对象重建；设备时钟仍DeviceUntrusted。CONT-C设计不清这些边界，不声称028/029或完整Demo完成。

## 12. 本轮静态交付与下一门

本轮只新写两份C1 UTF-8文档；scope保护原45个输入、原设计/原scope/原R报告三份正式实物，并另列1项必要Core字段契约身份，共49项；三协调稿的SD00变化按原设计结束→本轮before→after单列。其余状态/接口/范围/预算和141证据路径保持，仅把三份C1交接文档加入保护/未来导出集合，918→921项。没有改源码/测试/旧meta/内容/配置/依赖/工具/存档/旧报告；没有运行Unity、产品DLL、写证据根或恢复自动跟进。

原R在本C1准确自然completed、非异步formal DESIGN_COMPLETED和两份实物身份后，只复核R1及必要派生索引，保全原设计其余已审内容；唯一新写 demo-cont-c-design-c1-review.md（≤200物理行）。若NEEDS_FIX只另签剩余最小纠正；若ACCEPT仍由SD00另签CONT-C源码与精确工具预算例外，不能自动进入实现。作者本turn最终formal后停止修改，不自行跨任务发消息。
