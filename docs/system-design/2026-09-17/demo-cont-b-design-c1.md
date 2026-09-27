# CONT-B 永久请求精确实施设计 C1
状态：DESIGN_COMPLETED（C1作者设计完成，待原R对本次准确完成门独立审查；不是实施授权或代码接收）。
作者任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 turn：01a0d8eb-a0ce-76e1-abd2-638a12f23543。
原生 context：2026-09-25T14:15:16.037Z，gpt-6-astra / max，D:/Unity/UnityProj/FightMatch；本次 completed 由原生生命周期证明，不预填时间。
C1权限为§346（§347冻结）：4985字节/SHA256=2430da7b0bfee8e7823db8c1c5a2c47e295115ff7dfeafeacf680aff7dad0318；原§340/342业务边界保持。完整逐文件身份、精确数组与预算见 [C1 scope](demo-cont-b-design-c1-scope.json)。

## 1. 输入与设计结论
§339 已独立接收 CONT-A：744 实现／774 Assets／409 GUID，34 功能／30 已接收／余4／51计划交付；本设计不增加接收数。
沿原322路径、已接收11份新源码/测试及既定报告读表，另读原CONT-B三正式报告；当前343条只读输入，SourceSlice文档仍限原批准段落。原缺席的demo-025-p2b-prepare-code-review.md保持缺席。
C1精确保护入口845=原841+原CONT-B三正式报告+来源补签文档1；3协调稿最新单列，其余842保持。原有效3733全绿只继承，不重跑。
共享依据：interaction-contracts.md §4、character-growth.md §§3–5、inventory-loadout.md §§3/5/6/9、progress-tutorial.md §§3–6、application-flow.md §§3/4/7。
TB-READ01 已由 SD00 §342 直接补齐并完成有限读取：runtime-extension-contracts.md 的 §2 SourceSlice/EffectReceipt/InclusionEvidence 三行及 §7 来源/定额卡条款；全文件仅绑定元数据94934字节/SHA256=a4fee0a8cec2e61991570032833685f8e4cd79bb6ac460ddb7e0f4aff04a8479，不追读其他章节/外链。§342 规范化3195字节/SHA256=0f1704a381d2550c85046d6f1e04b8fd25be65ab7d85e31855070c427201921e，原读取续接见§343，本C1准确新回合按§347登记。
当前 M03 只有奖励成长，M04 只有普通结算来源及装配/偏好，M05 只有开放/挑战/首通。卡、学习、配方、来源消费和教学持久字段确有缺口，不能仅写 UI 包装。
设计采用原 M03/M04/M05 的不可变扩展、原 M02 SubmitTrusted/保存槽及原 M12；没有第二套培养、库存、队列、保存或通用事务框架。
当前真实发布闭包没有卡/证/配方/教学16定义；真实入口诚实返回 NoPublishedDefinition。新增 pure definition 闭包是 Core 值，仍不能证明 M10 发布或授予 PlayerSave 会话写权。
M10源格式、发布器、首包六实物、298字节新档配方、实际角色W/L1均保持；本轮不发布试调配方、额外角色或技能战斗效果。

原正式设计turn=01a0d8c3-46b9-7590-a675-dbf77eaefe44于2026-09-25T13:53:56.676Z completed/DESIGN_COMPLETED；R turn=01a0d8a3-1cdc-7bd3-a620-bbbd921026e4于14:11:19.149Z completed/NEEDS_FIX。原三稿仅只读继承，本C1完整承接设计，仅改F1/F2及其派生集合；原R覆写建议由§346明确的新C1输出规则取代。
F1：新增旧PlayerSessionModels.cs的IsMaterialized v4分类建议≤4增删行；F2：固定001～012每槽增加failure.json。两项仅为设计修正，现源码中的getter和工具保持原样。

## 2. CONT-C 可消费的 Interface
PlayerSessionSystem 新增 QueryPermanent(characterId)、PreviewPermanent(draft,budget)、ConfirmPermanent(preview,budget)、PreparePermanentMigration(expectedHead,budget)；已确认教学入场另用 PrepareTeachingEntry(exactLevelBinding,expectedHead,budget)，不能以查询触发赠物。
Query 返回同一已核 Commit 下的角色/技能/恢复、卡/证/配方目录、T/L/R/F、用途可用量、装配/偏好、教学步骤、原永久操作结果和逐项可操作原因；返回不可变值。
Draft 是带判别种类的明确目标：UseExperienceCards(CharacterId,TargetLevel,CardItemId)、LearnSkill(CharacterId,SkillId)、Craft(RecipeId,PositiveBatches,SelectedInputs)、Equip(CharacterId,HasItem,ItemId,L)、SetPreference(CharacterId,ItemId,Enabled,PreferenceRevision)、ConfirmTeachingExplanation(TutorialId,StepId)。
未指定材料份额而存在多个合法非卡分配时返回 CostSelectionRequired；Query 给选择范围。只在唯一普通聚合成本时可直接形成完整报价；确认后不能换份额。
预览固定 PlayerId、CharacterId/ClassId、Head Commit/Generation/descriptor身份、精确 ContentBinding、所需 DefinitionBinding、M03目标修订/技能键、M04 State/PreferenceRevision、M05修订/教学依据、全部来源端点和数额、目标/合并成本/确定效果及计算版本。
Preview 不创建 OperationId、不调用时钟/熵、不生成保存候选、不冻结或消耗；调用方原列表/字符串输入复制后不得反向修改预览。
Confirm 是明确用户确认的调用，只对新确认复核上述依据并生成一个稳定 OperationId 与原 PreparedCandidateLifecycleRequest；返回 Prepared，尚不是 Completed。
已确认 ticket 委托现 CandidateLifecycleApplicationSystem.Submit；重试/未知查询/终止复用其 Retry/Resolve/End 和原应用恢复入口，不重新 Confirm，不重新分配来源。
同 ID 同意图先返回原 M02 结果及原 CommitId；异意图 OperationConflict。此顺序先于当前余额、等级、场次、修订与新操作时点检查。
新的已学业务请求返回 AlreadyLearned/原学习引用，成本0；Unchanged/AlreadyLearned只在M02记录本次明确结果并引用原事实，不为记录无变化而增加owner修订或再造消费/技能凭据。
完成只在原 H10 已核持久门槛后一次发布完整头；结果给原操作、受影响身份、owner收据、新修订和 CommitId。未决只展示原已提交视图及诊断。
错误明确区分 NoPublishedDefinition、InsufficientResources、LevelRequirement、FirstCertificateReserved、TeachingRequired、CostSelectionRequired、UnsupportedSourceProof、UnsupportedBattleCapability、StaleContext、Busy/ResolutionRequired；Locked只保留真正进度锁含义。
新只读能力在旧类型上采用 GetPermanent…/GetLearnedSkills 等方法；不增加旧公开属性/字段，不重写反射回放、旧断言或首包。

## 3. 唯一拥有者与一次提交
M03：原 CandidateCharacterState 继续拥有 L/x/StateRevision；CandidateRosterState 内的永久扩展按 CharacterId 保存固定卡经验与学习收据，技能集合由该收据得到。阵容修订、原身份及恢复期间不因用卡改动。
M04：原 Holdings/Loadouts/ActiveCarry 是唯一 T/L/R 权威；附加持有来源、消费/转换端点和永久收据。来源是数量的证据，不是另一个可独立修改的余额。
M05：附加已给一次事件、准确 Level16EntryGift 物品端点、学习步骤/独立说明事实；不存第二份证余额或技能布尔。教学完成从这些事实核得。
M02：只保存原确认意图、三方收据引用、原结果和必要迁移事实；预览成本/效果是历史确认依据，不成为当前成长/库存权威。
一次 Build 从同一已提交 basis 调 M03/M04/M05 并核目标/绑定/成本效果；任一拒绝整项不发布。M06/M07/随机/基线以原引用保持，除现有合法独立业务外不改。
M03卡经验总和严格等于 M04准确卡单位×该绑定单位经验；学习与准确证耗、相应教学步同次；配方输入/输出及来源后继同次；装配和显式偏好同次。
仍只用原内部可信 Build；纯 Core 候选的 CommitEligible=false 及纯定义 不授会话提交权。公开任意 builder 对正式玩家的拒绝和零回调执行保留。
新 receipt 一一对应 M02 原操作及受影响 owner；读取/恢复检查遗漏、重复、错角色、错绑定、少一个owner、变造结果，均拒绝并保留原实物。

## 4. 用卡、来源与数值
复用 CandidateCharacterGrowth 的 N(L)=XpBase+(L−1)×(XpLinear+XpQuadratic×(L−1))；只把既有同一 Need/固定整数升级循环提成内部共用核，不开放任意经验写口。
目标t>L：need=N(L)−x+ΣN(k), k=L+1…t−1；kCards=ceil(need/q)。q来自当前精确卡定义，必须正整数；固定增加kCards×q并完整跨级，超出目标的经验保留。
ExactMathBudget/SaveCodecBudget约束求和、乘法、商余、循环及集合；q≤0、t≤L、负数、不能表示/预算不足、卡不足均无状态变化。不另设暗中等级上限、不用float或截断。
指定算例：N(4)=165/N(5)=220；W4+157用q50一张→W5+42；M5+92用三张→M6+22；q不同的合法定义另测，不能把50硬编码为卡通则。
G01：W4+157、M5+92、三卡给M后卡0/M6+22/W仍差8；原首证保留；既定旧关1固定14经验由原M07交付后W5+6，明确学嘲讽再明确说明。不能由H03补14、免等级、返卡或额外赠证。
Mage仅经新的显式 PreparePermanentDefinition 取得永久成长定义，用原整数成长和明确给定基础成长输入；旧PrepareDefinition仍只接受Warrior，旧不支持输入不放宽。Mage战斗字段明确不适用，不借Warrior身份或假暴击值冒充执行能力。
同ItemId且同q，只从合法F份额按广告先于普通分配；各类内按持久取得顺序，再按原来源/行/单位身份打破并列。预览冻结所有范围，确认/Retry不再排序挑另一批。
来源端点必须已持有、同玩家/同绑定、覆盖完整、范围不重叠、未永久消费且用途合法；F只是数量上界，不代替证明。耗尽后保留原来源/消费/固定经验关联。
定额卡按实际份额逐段保留固定经验及 CharacterId；同源换 OperationId/EffectId/受益人不能重新使用。普通聚合量只在必要混合投入时保留确切原投入依据。
不新接广告SDK、在线发放、账号/跨档接入、云同步或InclusionEvidence生成；本机消费不宣告远端真实性。缺已有合法持有依据返回 UnsupportedSourceProof/ReconciliationRequired。

### 4.1 公共来源到 v4 的准确映射
以下是共同契约在本包的字段投影，非新一套来源身份。文本/整数/可选值/集合复用 BusinessFields 的规范编码及预算；逻辑序号用非负整数，数量用正整数。每行按所列字段顺序编码，严格判别分支，不用自由字典、字符串拼接ID或调用方 trust=true。字段只在 schema4 新扩展内出现。
| 公共概念 | v4 字段与原值来源 | owner 与必须核对的条件 |
| --- | --- | --- |
| 原发放身份 | OriginalGrantRef：tag=OrdinaryBaseReward 时为 PlayerId、AttemptId、SettlementId、原 ContentBinding；tag=ExistingAdvertisementHeld 时为 PlayerId、原 GrantId、原 Purpose、原 FactId/UseId 的有无及原值、原 ContentBinding。 | M04。普通来源引用现有 M07/M04 原结算，不伪造 GrantId；受保护来源必须已有完整持有证明，保留原契约要求的全部关联字段，不能仅给 GrantId 便新发物。 |
| 原发放行 | SourceLine：OriginalGrantRef、OriginalGrantLine、ItemId、OriginalLineQuantity、原取得依据 AcquisitionRef、OriginalOperationRef、OriginalCommitRef、已有 OriginalBranchId/ExistingInclusionRef 的有无及值。 | M04。原行号沿不可变发放向量，不是当前堆叠/排序后的下标；普通向量沿旧规范 ItemId 排序行。完整原行数量与已合法取得区间分别核对，不把未接入区间当持有。 |
| 取得顺序 | AcquisitionRef 指已有 M04 持有索引的稳定次序及其原操作；普通来源沿原 M02 提交记录次序/原发放行。 | 不取新时钟/随机，不按当前列表顺序重编号。已有广告来源缺该依据即缺证；同种同q先广告，再普通，类别内沿此原顺序。 |
| SourceSlice | SourceLineRef、UnitStart、UnitCount；逻辑区间为 [UnitStart,UnitStart+UnitCount)。 | M04。0≤起点、数量>0、末端≤原行数量；身份由原发放/行/逻辑位置决定，分支复制、拆堆/并堆保持；相邻范围可合并表达，不能重叠或把三张Grant当一张卡。 |
| 当前有效终点 | Endpoint：准确 InputRef、Held/Consumed/Transformed tag、后两者的 EffectRef；输入为 SourceSlice 或原转换的 EffectOutputRef。 | M04。核已取得范围的完整终点覆盖及唯一性；新消费只取当前已核的合法F持有份额，不能凭旧包含证明覆盖当前已耗终点。来源/效果耗尽仍保留防重历史。 |
| EffectReceipt 身份/用途 | EffectRef=(Owner,OperationId,EffectRow)，EffectKind、原用途；已有外部效果引用保留其原身份。 | 对本轮效果使用同一确认 OperationId 下的确定行，无额外ID分配；不同OperationId不能重用已耗份额。普通无变化结果只引用旧事实。 |
| EffectReceipt 输入 | Inputs 按规范排序列每个准确 SourceSlice/EffectOutputRef；普通混合投入列 OriginalOrdinaryContributionRef=(OriginalGrantRef,OriginalGrantLine,Quantity,所需原M04结束/永久成本历史引用)。 | M04固定实际分摊；M03/M05引用同一成本收据。普通贡献沿真实结算/已耗依据，仅补本次转换所需证据，不要求所有普通物品逐件编号；原聚合 L/R/D 无法证明来源归属时拒绝，不能猜另一来源来凑F。 |
| 固定效果/受益 | FixedEffects：kind、ItemId或SkillId、整数增量/产出；卡行另存 UnitXp、Units、FixedXp、CharacterId、ClassId；共同带 PlayerId、准确 ContentBinding/适用 DefinitionBinding。 | M03固定卡经验=实际单位×q，保存原前后L/x/修订；学习一条准确技能事实；M04保存制作完整输出及原输入关系。不会从当前等级差重新推算效果或改投另一角色。 |
| 原操作/提交 | OriginalOperationId；本机 CommitRef 为指向原 M02 记录的该 OperationId，完成查询解析该记录的 CommitId/Generation。已有来源提交/分支原值随 SourceLine 保持。 | 候选准备时不预造提交号；原 CandidateApplicationSaveCodec 完成头与最后记录绑定。恢复必须能唯一解析原提交；本地头没有公共 BranchId 时不凭空创建分支身份。 |
| 后继/准确撤回 | EffectOutputRef=(原 EffectRef,OutputLine,UnitStart,UnitCount)；后继 receipt 的 Inputs 回指前序，PredecessorRefs/已有 RetractionRef 明确记录关系。 | 原 receipt 不追加可变全局used；后继索引从不可变引用核得。CONT-B不新增撤回请求；已有撤回只保留准确原依据，缺链/循环/错输出/重复有效终点拒绝。 |
| 混合不可分转换 | 同一 EffectRef 保留完整 Inputs、FixedEffects 和不可分关联；下游仅引用真实输出行的逻辑单位及该原效果。 | 本机可按真实产出单位消费并保留整条前序；不能把受保护输入按产量比例分派或制造可独立跨档接入的子效果。混合产卡没有获准分类/分摊依据时不能套广告优先，返回明确来源缺口。 |
| InclusionEvidence 引用 | ExistingInclusionRef=(原 CheckpointId,BranchId,SourceCommitId,EvidenceViewRevision,准确 SourceSlice/EndpointRef,原 disposition)；整个引用有显式存在标记，沿既有证据索引定位。 | 只引用已有依据，不生成检查点、证据视图或跨档比较器；Absent/SameEffect/DescendantConsumed/SelectedConsumption/SelectedHolding/Unresolved仅保留原已证明判定，不在本轮计算。活动消费改查当前 CommitId 索引。 |
M02 FMINT004 永久载荷冻结相同输入区间、成本、绑定、目标和计算结果；M04写上述来源/成本/输出，M03写卡/技能固定结果，M05只写一次赠物和教学引用。编码器共用同一字段访问器，三方引用恢复必须逐项一致；未知 tag、错角色/绑定/单位、原行缺失、重叠、悬空/循环后继、已耗终点重用、伪造包含依据均拒绝。
所有来源集合以原身份的字段元组按序数文本/整数排序后规范编码，引用按值核对；线内区间按起点排序并合并相邻同终点范围。排序或数组下标不创造身份。原始收据行保持原顺序，后续序列化不能改变原发放行号。
已核合法广告持有的正例仅在同一 owner 核的隔离数据中验证消费与编码关系；构造该只读值不获得正式 PlayerSession 写权。真实首包没有该来源接入路径；缺实际原索引/证明时拒绝，不添加临时导入器、M08/M16接口或新的 M12 resolver。
原普通来源投影从已提交M07/M04/M02收据只读派生；3→4迁移仍追加空扩展，不补发/重编号。只有真实新永久转换才把所用证明写入其收据，查询/预览不物化新来源。
此映射落在原拟议 CandidatePermanentModels/Inventory/Growth/Protocol/Codec 与既有 RestoreChecks/References 文件职责及上限内；未增加源码/测试文件或放宽总预算。CB05/06/12/16/19补齐逐字段验收，全部仍为未来未运行项。

## 5. 学习和教学
明确核Character/Class/Skill归属、Lv≥5、未学、一个合法通用证、当前完整M05依据及用户确认；目标等级不足给缺口，不自动消耗经验卡。
未领首证必须由完整一次索引证明；已领教学未完必须对应准确物品端点及合法目标战士嘲讽；找不到原赠证、错绑定或教学/学习矛盾不能当成无限制。
首证1/其他0，法师Lv6在普通菜单学习拒绝；另有独立合法证1则仅扣另一证，不推进战士教学，首证端点原样保留。
战士合法教学学习优先消耗准确首证，同次保存M03技能、M04消费和M05学习依据；已合法学过则引用旧真实学习完成待补学习步，成本0。说明必须另一次明确确认。
首证未消费且教学完成只解除用途约束，不增加T；已消费首证不返还。普通菜单、返回地图、整理堆叠不能绕过规则。
BeginTeachingGift是已确认合法入场事件的限定内部H03分支：核精确已发布教学定义/开放事实和Level16EntryGift唯一键，赠物端点与M05一次事实同提交；没有公开任意发物接口。
其合法隔离事件可用于验证首证流程；真实首包不存在该定义，返回NoPublishedDefinition，不能凭LevelId字符串16发证/开功能。首通第二证及新章节投放仍由原内容/结算负责者以后另签。
需要教学Challenge时由M05同一次事件建立或复用该关原Challenge；v4明确保存空Attempt挑战的原DefinitionBinding，后续H02沿原Challenge；旧schema不放宽。
新内容H02必须核全部实际Ready参与者的已学技能/职业执行能力；现有核不支持的技能或Mage战斗明确UnsupportedBattleCapability，在取ID/熵/冻结前拒绝。旧活动尝试沿原空技能基线，不能受后来学习追写或抹掉新技能。

## 6. 制作、装配与活动时点
制作固定RecipeId/recordVersion/ContentBinding与正整数批数；先按ItemId合并重复输入，再乘批数，输出按定义逐项算总数；数量预算失败无扣物。
每种输入不得超过F=T−ΣL−ΣR且须用途/来源合法；明确选择输入范围，不能借其他角色L或活动R、自动卸装或套用经验卡来源优先政策。
输入端点转为一次消费/转换结果，输出总量只计一次；保存普通投入与受保护投入的不可分关系，不为混合产物虚构可拆经验或跨档补偿政策。
产量不受UI格数限制，显示堆叠仍由T/L/R/F派生；候选“3+1→6”不能自动成为正式配方。隔离数值配方明确标测试输入。
装配复用原Equip：仅无活动尝试，角色/职业适配、0≤L≤99、全角色ΣL/F核对；新Item默认Enabled=true，同Item只调数量保留偏好，L0保留ItemId选择。
偏好复用原SetPreference：明确Enabled目标及PreferenceRevision；同操作重试不翻转。已是目标值可给无变化结果，偏好和库存修订按原实现规则递进。
| 最新事实 | 永久请求准入 |
| --- | --- |
| Ready局外、无S17/未决/F2阻断 | 全部合法请求可预览/确认；装配仍核F/身份。 |
| AwaitAction/AwaitLinks/AwaitRescue活动中 | 用卡/学习/制作只改长期资料及合法F；明确偏好可改；装配、阵容和局外数量转移拒绝。 |
| 演出门闩存在 | 合法明确偏好及永久成长仍可串行提交；不能解除原演出令牌、推进SceneRevision/HP/U/三域。 |
| WonPendingSettlement或S17基础结算 | 先续原结算；新永久写拒绝，旧操作结果照常查询。 |
| SaveFailed/CommitUnknown/未决磁盘候选 | 只读原头、原候选重试/Resolve/ResumeObserved；不受理冲突新写。 |
| 未确认F2、恢复未核完整/原包缺失 | CreationPending/ResolutionRequired/UnsupportedBinding；不借预览建新档。 |
永久增长不改入场Stats/HP/参与者/三域初态、当前战斗历史和Restart基线；旧尝试结束核当前长期资料，不能把入场快照覆盖最新成长。
偏好历史核对改为按M02操作顺序演进：每次攻击核当时PreferenceRevision/实际ActionConditions，而非要求所有历史攻击等于最终修订；回退不退当前偏好，重新行动读取最新偏好。
恢复中或未参战角色仍可进行合法永久成长；不自动完成恢复、不把它插入现有战斗、不改变其他角色的L/来源或收益。

## 7. 格式、迁移、未知及F2
旧Candidate schema1、正式all-2、CONT-A [3,3,3,3,2,2]、FMINT001/002/003、FMPROF01/02和创建锚逐字节保持；不重编码旧记录为latest。
新增PublishedPermanentV4=4，仅新完整向量[4,4,4,4,2,2]；M06/M07仍schema2，FMBIZ001外层不变；新增feature fm.player.permanent.v1，保留application/roster旧features。
| Owner/schema4追加部分 | 最低持久内容 |
| --- | --- |
| M02 | kind13永久请求、kind14永久格式迁移，原预览目标/成本/来源/效果/修订，owner receipt引用与结果；旧记录仍保存原canonical intent。 |
| M03 | 原roster/角色记录后追加按CharacterId排序的卡固定效果与学习记录；每条带原操作、绑定、输入引用和前后L/x/修订或SkillId。 |
| M04 | 原库存后追加合法持有根、准确剩余/消费/转换端点、永久成本/产物收据与装配/偏好历史依据；原T是唯一余额。 |
| M05 | 原进度后追加教学/一次赠物/学习和说明引用；空Attempt教学挑战显式带原整关绑定，不能猜最新Level。 |
FMINT004=magic+schema4+原Player/Operation/ExpectedHead/PublishedContext+kind严格判别载荷；kind13只容上述确定请求，kind14只容原descriptor/3→4，kind1沿原roster初始化形状支持纯Core隔离v4初态（来源扩展/技能/库存仍空）。真实F2仍仅用原format2/3初始化。
各新字段按BusinessFields原文本/整数/集合/UTF-16LE规范和预算编码；新集合排序/重复/不合法Unicode/未知tag/不匹配schema拒绝。只在schema4写扩展，旧字段顺序和值保持。
F1：PlayerRosterView原IsMaterialized只以Format==PublishedRosterV3判定；未来仅在同一getter加入PublishedPermanentV4，真实Format继续来自business.Format，不伪装v3。v2=false/v3=true、旧公开属性形状和其他模型保持；v3→v4→实际保存/对象重建恢复后的QueryRoster仍给原CharacterId、三个槽与FormationRevision，且Format=v4/IsMaterialized=true。
PreparePermanentMigration明确将已确认、已核的v3头变v4，冻结Commit/Generation/descriptor长度SHA及3→4；追加空扩展并原样保留所有owner数值/收据/原引用，只有M02记录和保存代际递进。
迁移可在无S17/保存/F2阻断的活动头进行，因为不改阵容或历史；v3活动已确认未决须先按原格式续完。all-2仍先按CONT-A原局外2→3路径，不能把原迁移限制偷偷放宽。
新档仍由原FMPROF02建v3，随后明确技术迁移；FMPROF01旧创建未知也先照原intent2完成，再走既有迁移。没有FMPROF03、清档、赠物或新初始化配方。
恢复要求增加M02–M05 schema4四合同（合计14已知合同）及一个feature；每个owner声明当前/历史/未决所用卡/技能/配方/教学/来源的精确包与整关，M12仍核全部当前/前代/未知根。
ResolveExact缺V1即拒绝并保留；仅V2已下载不切换有效V1成本。运行集合真正改变相关绑定、头、owner修订/来源端点，旧未确认预览StaleContext。
候选一旦交原runtime提交槽，Intent/来源/成本/效果/字节/ticket冻结。SaveFailed重写原票据；CommitUnknown先查询，marker已成返回原结果，未成才续原候选；任何路径不重算报价或重取源份额。
取消/返回只丢未确认预览。确认后未提交票据没有永久效果；已进入保存槽不能靠关页撤销。重开应用先用原观察/ResumeObserved/Resolve恢复准确Commit及候选；仅未确认预览不恢复成消费事务。
M02公开原Operation结果查询及界面重建读取持久收据；已提交回执丢失只读原结果，不因余额0/已学换ID再扣。End保留原M12“已成不能当未成删除”语义。

## 8. 逐文件实施建议
以下全部仅为待SD00另签建议，当前无源码写权。既有精确bytes/SHA及新增文件缺席核对在scope；未列路径禁止修改。
生产31旧文件逐项增删上限合计1638；10新生产物理行合计2230；合计3868≤4000。6新测试合计2530≤2600；每文件上限与总上限同时成立，不压行规避。
| 既有生产文件 | 增删上限 | 准确修改目的 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/BattleEntryInput.cs | 4 | 仅追加 Mage=2 供永久成长身份；旧 Warrior 枚举值及战斗支持集不变。 |
| Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs | 45 | 新增显式永久 Mage 成长定义的内部严格校验；原 PrepareDefinition 的 Warrior 支持/拒绝形状保持。 |
| Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs | 110 | 新增显式 PreparePermanentDefinition，复用内部 N(L)/固定整数成长；Mage只用于永久资料，旧Warrior工厂/奖励行为保持。 |
| Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs | 40 | M04 只读永久来源/消费扩展及内部构造；旧公开属性不增改。 |
| Assets/Scripts/FightMatch/Core/CandidateInventory.cs | 30 | 现有 Change/Equip/SetPreference/End 路径保留来源扩展，复用既有 T/L/R/F 和明确偏好。 |
| Assets/Scripts/FightMatch/Core/CandidateProgressionState.cs | 30 | M05 只读一次事件/教学扩展，旧公开反射形状保持。 |
| Assets/Scripts/FightMatch/Core/CandidateProgression.cs | 47 | 复制路径保留教学依据，并在共同入场核对处接教学约束。 |
| Assets/Scripts/FightMatch/Core/CandidateRosterState.cs | 55 | M03 集合保存逐角色永久经验/技能收据；Replace/SetFormation/恢复保持扩展。 |
| Assets/Scripts/FightMatch/Core/CandidateRosterSaveCodec.cs | 25 | 仅 schema4 追加 M03 永久字段；schema3 字节保持。 |
| Assets/Scripts/FightMatch/Core/CandidateRosterProtocol.cs | 25 | v4 继续消费原阵容办理，核扩展不被改动；2→3 原迁移不改语义。 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessSnapshot.cs | 8 | 追加 PublishedPermanentV4 格式；仍是同五 owner。 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessSaveCodec.cs | 110 | 只增加 [4,4,4,4,2,2] 对应五业务切片分派及完整恢复引用。 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs | 130 | 核卡/证/制作来源守恒、技能/教学/receipt覆盖及角色/库存真实修订，保留原历史校验。 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessSaveValues.cs | 15 | v4 字段访问/规范编码所需最小适配，旧文本/整数/预算规则保持。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentSaveCodec.cs | 90 | v4 M04/M05 扩展与 Mage 纯成长定义读取；旧 schema1/2/3 字节保持。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationIntent.cs | 35 | 新增 kind13 永久请求/kind14永久格式迁移及方法访问载荷，旧枚举/公开属性保持。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationIntentCodec.cs | 90 | FMINT004 严格判别与冻结确定成本/目标/来源；FMINT001/002/003 编码不变。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationProtocol.cs | 95 | 永久联合 transition/result/迁移及复制保全；无部分发布或新 owner 写口。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationRecords.cs | 45 | M02 结果新增 GetPermanentReceipt/GetPermanentMigration 方法及内部不可变引用。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs | 170 | 逐操作重演永久/偏好修订，保留当时 ActionConditions；核 owner receipt 和已学业务键。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationSaveCodec.cs | 100 | schema4 M02 消息/结果引用及 feature/绑定；旧初始化锚和历史记录各按原格式。 |
| Assets/Scripts/FightMatch/Core/PublishedSaveContext.cs | 60 | 新增纯定义闭包的 GetPermanentDefinitions/PreparePermanent；默认现发行永久目录为空，不授发布权。 |
| Assets/Scripts/FightMatch/Application/CandidateApplicationModels.cs | 30 | 冻结/复制永久结果引用；旧返回形状不增属性。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleApplicationSystem.cs | 45 | 新增共享 Build 分派及各请求合法时点，保留 SubmitTrusted/原操作优先。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleModels.cs | 40 | 生命周期内容/请求方法式接入永久定义，保留旧公开反射面。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecyclePreparation.cs | 50 | 同一冻结规范及版本分派接入永久意图；预览不创建 ID/候选/随机。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleEntry.cs | 20 | 旧单角色入口明确核当前已学执行能力/教学，未支持拒绝而非丢弃。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecycleRoster.cs | 35 | v4 集合入场、已学能力与原入场基线保全；先检查后 ID/熵。 |
| Assets/Scripts/FightMatch/Application/PlayerRosterSession.cs | 30 | v4 阵容兼容及恢复合同/feature；不把阵容活动门复用于 H03。 |
| Assets/Scripts/FightMatch/Application/PlayerSessionModels.cs | 4 | 仅原 IsMaterialized 分类接纳 PublishedPermanentV4，真实Format=v4，旧v2=false/v3=true与公开形状保持；3439字节/70行/SHA256=4a41db0d9ed52f6fb321a58ea5123b1fa0d6aaf9877e177206c9a89fdea7af51。 |
| Assets/Scripts/FightMatch/Application/PlayerSessionSystem.cs | 25 | 从当前已验证精确目录接永久定义；真实首包为空，F2 仍用原两格式。 |
| 拟新增生产文件 | 物理行上限 | 责任 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/CandidatePermanentDefinitions.cs | 230 | 纯且不可变的卡/技能/配方/教学定义，精确绑定、数量预算和闭包验证。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentModels.cs | 270 | 明确意图/预览/来源份额/成本效果/逐 owner 收据及诊断 DTO；只读值和有界集合。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentGrowth.cs | 160 | 用卡最少数量、完整跨级余量、技能等级/职业/业务键；复用 M03 算法。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentInventory.cs | 280 | F 与用途筛选、卡广告先扣、明确材料份额、制作合并和来源/效果守恒。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentProgression.cs | 180 | 首证依据、教学学习与独立说明候选；只处理合法已给事件。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs | 270 | v4 显式迁移、各永久请求跨 owner 绑定/修订/效果核合及恢复历史覆盖。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentCodec.cs | 250 | v4 永久字段/意图/收据的规范有界访问，旧编码不改。 |
| Assets/Scripts/FightMatch/Application/PlayerPermanentSession.cs | 210 | 查询/预览/明确确认/格式迁移入口，消费原 runtime 与 exact publication。 |
| Assets/Scripts/FightMatch/Application/CandidateLifecyclePermanent.cs | 230 | 实际 Submit 和隔离正例共用的唯一永久 Build 核，逐操作时点与原候选续办。 |
| Assets/Scripts/FightMatch/Application/PlayerPermanentModels.cs | 150 | CONT-C 可消费的 typed draft/view/preview/ticket/result，无页面状态权威。 |
| 拟新增测试文件 | 物理行上限 | 验证责任 |
| --- | ---: | --- |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentGrowthTests.cs | 400 | CB02–04: 卡/跨级/G01/多角色/预算 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentInventoryTests.cs | 420 | CB05–08: 来源/制作/装配/明确偏好 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProgressionTests.cs | 360 | CB09–11: Lv5/首证/业务重复/教学 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProtocolTests.cs | 400 | CB12–16: 冻结/原操作/owner 拒绝/恢复/兼容 |
| Assets/Tests/EditMode/FightMatch/PlayerPermanentSessionTests.cs | 450 | CB01/16–20: 真实首包/会话/迁移物化视图/同源/未支持入场 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentTestData.cs | 500 | 合法纯定义/原 M07 普通发物与内存故障夹具；不改旧 helper |
每份新.cs仅允许未来Unity自然生成同路径.meta，共16份；旧meta/asmdef/Content/Platform/Packages/ProjectSettings/资产/旧测试与断言保持。预计776实现／806Assets／425唯一GUID。
工具只拟改原Invoke-FM025P2Validation.ps1：固定CONT-B Compile/Tests、输入/保护集合和准确新根绑定；增删≤40，累计143+本次≤183≤200，最终≤320物理行。旧七Stage/SHA自检/进程安全不改。
不改CandidateApplicationRuntime、LocalSaveStore或F2 codec：原先查操作、冻结候选、持久门槛与恢复通道继续使用。确需超出任一文件/预算/接口须交SD00，不能现场扩白名单。

## 9. 未来有限验证建议
设计阶段Unity/产品DLL/游戏测试均NOT RUN。原CONT-A run010/011及3733原具名多重集合是有效入口，未来实现有新源码后才跑完整回归。
继承原正式设计的静态完成核验（2026-09-25T13:52:28.232155Z）：838不可变导出及补签文档、12份已接收输入身份、744实现/774Assets/409唯一GUID、36DLL/PDB均相符；32拟新增源码/meta及未来证据根仍缺席。七根4897items沿用13:20:29.743040Z有效完整核对，本次仅续核七组root/manifest、原3733 XML及为修正DLL引用所需的最终身份记录；没有重跑原验证。
拟议唯一新根TestArtifacts/FMDemoCONT/cont-b；固定17个根级元数据叶、32份before-text、64份after-text及runs/001…012各12个明确候选叶，另read-manifest自身，共258条精确路径；准确枚举见C1 scope，无通配目录授权。
F2静态对应原工具：共同before/run/after/result四JSON（207/214/222/242行）；Compile与Tests各自log/stdout/stderr（183～186/205/212行），Tests另有tests.xml（187/191行）；编译/测试不通过写failure.json并exit1（236～245行），try内异常按原分支写HarnessFailure并throw（247～252行）。这些12类叶均逐槽展开；不删除失败分支、不改变结果、不伪造任何日志。
每槽的12叶是允许写入的并集，非要求每次产生全部文件；保留原preflight/异常的实际写入行为。manifest只列实际文件且排除自身，260上限不授权258条清单外的额外路径。新增12条failure.json准确全路径另列在scope.futureEvidenceProposal.failureLeafPaths，并包含在完整有限数组中。
实际文件≤260、manifest≤512项且只列实际叶，排除自身；失败不覆盖、run编号不复用、槽位用尽交SD00。本设计不创建根、源码副本、run或任意脚本。
保留七继承根CONT-A 202items及六历史根4695items/根/manifest，原失败/来源/配方/验收报告不改。原36DLL/PDB只核长度SHA，不把Library目录变成语义读取范围。
原正式设计已纠正其草案误沿用CONT-A实施前设计中的36项DLL身份；现准确绑定CONT-A dll-after.json（6934字节/SHA256=4df50035ad334fa96877cf8210587dc351e8b0adb33660675b5f594db20574c0），其36项与run010后/run011前/run011后及当前实物相同，744源码也同版。仅修正设计引用，原证据不改、Unity不重跑；错误旧引用及定位过程保留在scope。
后续原C串行Unity2022.3.18f1，先查无同项目Editor；固定工具CONT-B Compile后完整无filter EditMode，Tests不加quit；ExpectedScriptSha256取实际获准改后脚本，不预填未来哈希。
每run保留实际参数/PID/时刻/exit、前后进程/源码/资产/工具/DLL身份、原stdout/stderr/log/XML；最终compile后=tests前=tests后=交付实物。旧3733实例按fullname多重集合精确保全，新例单列，失败/skip为0。
只用既有内存storage故障夹具与原M12协议，不新建physical probe、不强杀、不写persistentDataPath，不把对象重建称进程崩溃证据。通过且源码不变不重复运行。
普通资源正例经合法隔离定义→原M07普通整关发物→同一永久Build→M02 Propose/规范往返→原M12内存保存/恢复。隔离closure不成为M10审定或真实PlayerSession。
广告已有持有份额的定额消费正例在同一M03/M04永久核验证完整原证明与端点；不造正式广告发放，不能用UI状态或反射修改私有字段冒充合法起点。
真实PlayerSession验证首包缺定义、已确认F2/迁移/原操作优先/任意builder拒绝等真实入口；隔离正例与正式会话证据分别列明，不把两者混算成额外已发布内容。

## 10. 完整验收矩阵
| 编号 | 必须可核对的结果 | 拟验证位置 |
| --- | --- | --- |
| CB01 | 真实catalog/原W/L1会话Query/Preview卡、证/技能、配方均NoPublishedDefinition；合法隔离定义已存在但零可用量返回InsufficientResources，非Locked；查询/预览无ID/熵/保存/冻结，六首包及298字节原配方保持。 | PlayerPermanentSessionTests.cs |
| CB02 | W4+157,q50,t5→1卡/W5+42；M5+92,q50,t6→3卡/M6+22；另q7/q500、多跨级及高于目标余量完整；目标≤当前、q≤0/缺字段/低预算/不足全旧。 | CandidatePermanentGrowthTests.cs |
| CB03 | 合法隔离W4+157/M5+92、3卡全给M后M6+22/W仍157/卡0；首证保留，原M07旧关固定14使W5+6，再明确学习和说明；不自动返卡/补级/赠证。 | CandidatePermanentGrowthTests.cs |
| CB04 | 角色ID/Class绑定逐项核对；给M成长不改变W/其他人/阵容/恢复；恢复者和未参战者可合法成长但不插入活动，不能用首个角色替代。 | CandidatePermanentGrowthTests.cs |
| CB05 | 同种同q G1+N1用1只耗G；G1+N2用2耗G1+N1，逐来源经验合计2q；按持久取得/原来源行单位稳定排序；少量、重叠、已消费/外来/用途错/证明缺失/篡改拒绝；份额耗尽证据保留。补核原Grant三单位只耗[0,2)、余[2,3)，拆并堆不重编号；换OperationId/EffectRef或受益人不能重耗；原行/取得顺序/区间/当前唯一终点任一缺失或篡改拒绝。 | CandidatePermanentInventoryTests.cs |
| CB06 | 合法隔离非试调正式配方、重复ItemId输入先合并；批数1/多批、正数/0/负数、预算溢出；输出可超99按显示拆叠；T改变恰等各合法输入/产出，来源转换保留且F足额。混合转换保留完整输入/真实输出及前序引用，后继消费不同时保留原物；不按比例拆受保护来源，不把混合产卡误判广告卡。 | CandidatePermanentInventoryTests.cs |
| CB07 | 固定全局T、各角色L及活动R；永久成本≤F，不借其他角色L/R，不暗改装配；多个非卡可用来源未明确选择返回CostSelectionRequired，指定合法份额后固定同一结果。 | CandidatePermanentInventoryTests.cs |
| CB08 | 合法无活动Equip量0/99/不足/错职业；新Item默认开，同Item数量变化及跨关补充保持主动关；L0保留选择；SetPreference明确Enabled和修订，无Toggle，同结果重试不反转。 | CandidatePermanentInventoryTests.cs |
| CB09 | 错职业/技能、Lv4拒绝；Lv5一证明确学习、取消不扣；同ID原结果优先，新ID再学AlreadyLearned且成本0；成长学习凭据及技能唯一键可恢复。 | CandidatePermanentProgressionTests.cs |
| CB10 | 首证1其他0的Lv6法师普通菜单拒绝；另普通证1只耗普通；合法W学优先首证；错赠物/绑定/缺端点/过时M05拒绝，首证不充当L/R，页面返回不解除限制。 | CandidatePermanentProgressionTests.cs |
| CB11 | 合法隔离首次教学入场事件原onceKey只赠一次；赠物+M05同提交；学习+扣证+步骤同提交；说明单独确认；已学时引用原结果不再扣，完成不返已耗首证/不增加未耗T；空Attempt原Challenge兼容。 | CandidatePermanentProgressionTests.cs |
| CB12 | 输入/列表调用后改动无效；头、角色/M04/M05修订、源端点、目标绑定变化旧预览拒绝；仅下载V2而当前V1未变不失效，确认不默换成本/目标；每个owner单独拒绝均全旧。确认逐项核原Grant行、逻辑区间、取得索引、原操作/提交和当前端点；旧InclusionEvidence不能覆盖后来已消费事实。 | CandidatePermanentProtocolTests.cs |
| CB13 | 同ID同意图在现余额0、已升级、活动/其他头之后仍原Commit/结果；同ID异意图OperationConflict；AlreadyLearned/Unchanged没有新owner效果/扣物，只有允许的原结果引用。 | CandidatePermanentProtocolTests.cs |
| CB14 | 卡/学/制作/Equip/Preference/说明/合法教学事件分别注入Prepare拒绝、Write明确失败、结果未知、marker已成但回执失；沿原intent/candidate/ticket/份额/bytes Retry/Resolve，原头全旧或完整新头，恰好一次。 | CandidatePermanentProtocolTests.cs |
| CB15 | 未确认预览丢弃无事务；保存后重建原M12对象、ReadRecovery/ResumeObserved/Resolve取得原意图与候选，未知不换ID/源，已成读原结果；缺原绑定/必要root保持RecoveryBlocked，End不删已成。 | CandidatePermanentProtocolTests.cs |
| CB16 | schema1/all2/3及FMINT001/2/3原字节；FMPROF01/02原14个断点由原3733回归覆盖，不另重复旧矩阵；新增旧未知先原格式续→迁移v4及原generation1/index0锚、v3→4活动/空闲保全组合；混合schema/tag、篡改/缺记录/不规范列表/低预算拒绝。v4来源/效果/ExistingInclusionRef逐字段规范往返；原Grant行次序、OperationId→原CommitId、完整后继链保留；未知tag、重叠/悬空/循环/假包含引用拒绝；旧格式不写新字段。补F1：合法v3→v4→实际保存/对象重建恢复后，真实QueryRoster().Format=v4且IsMaterialized=true，原CharacterId/三个槽/FormationRevision不变，旧v2=false/v3=true；实际会话断言与CB17在PlayerPermanentSessionTests共用同一迁移恢复用例，不另跑旧矩阵。 | CandidatePermanentProtocolTests.cs；真实QueryRoster与CB17共用PlayerPermanentSessionTests.cs用例 |
| CB17 | 实际原runtime门保持：未确认F2/本机未决/恢复未核拒绝新写，S17先结算；活动允许合法成长/制作/偏好，拒Equip/阵容；演出偏好不解原token、不推进HP/U/Scene/RNG，原操作结果仍可读。补F1：通过实际PlayerSession.QueryRoster核v3迁移前、v4持久提交后及对象重建恢复后的Format/IsMaterialized组合；v4不伪装v3，角色身份、三个槽、阵容修订保全；旧v2/v3分类保持。 | PlayerPermanentSessionTests.cs |
| CB18 | 用卡/学/偏好在既定合法时点后原Stats/HP/三域/入场成员/Restart基线不变；攻击按当时pref、回退保当前pref；已学未支持技能/Mage新H02在ID/熵/冻结前明确拒绝，不能清技能后入场。 | PlayerPermanentSessionTests.cs |
| CB19 | 合法纯定义与原M07普通发物驱动同一Build/Propose/codec/M12内存核完成各正例；广告已有证明仅测同一owner消费核；没有假M10审定、正式赠物、反射造成功；任意builder正式提交仍拒且回调0。广告持有隔离证据仅证明同一owner核及v4引用一致性，不生成InclusionEvidence；真实会话缺已有来源索引时明确拒绝，不能由只读DTO或旧SelectedHolding判定授写权。 | PlayerPermanentSessionTests.cs |
| CB20 | 未来实现后串行固定Compile/完整EditMode，无filter；原3733具名多重集合/断言保留，新例单列0失败0skip；最终776实现与36DLL同源，31旧/10新/6测试/16meta及工具预算、未改项、七历史根和所有失败证据保全。补F1/F2：31个拟改旧生产的精确身份与713个其余实现保全；32 before/64 after；12个槽各包含failure.json，工具成功/编译失败/测试失败/异常写入叶静态全覆盖；258条精确路径、actual≤260/manifest≤512、未来880导出及原/C1六设计报告逐项保护。 | 工具/原XML/静态身份核对 |
范围验证另核所有拟改旧文件原文/差异、新文件及自然GUID；其余当前744实现中的713项保持，842受保护导出按具体实施白名单扣除允许文件，其余精确保全，3协调稿最新单列。
未来作者唯一交付demo-cont-b-delivery.md≤300行、demo-cont-b-scope.json≤8MiB；唯一R代码审查demo-cont-b-code-review.md≤260行，原设计三稿及历史报告只读。本轮不创建这些未来文件。

## 11. C1纠正对应、身份与完成门
本C1静态核验于2026-09-25T14:24:21.435697Z通过：845精确入口中842不可变项/原三正式稿零差异，744实现/774Assets/409GUID及36DLL身份保持；31旧文件预算/32before/64after/258证据路径/880未来导出与六设计保护集合逐项相符。原工具各写入分支的12类叶已静态穷举，未来证据根及32新源码/meta仍缺席；本轮未运行Unity、产品DLL或验证工具。
原正式三稿只读身份：design 44051字节/231行/SHA256=2c0fd41083cb855aab71926a3c8301776b486340c1f94d19d81bcaf668e01f9f；scope 3022161字节/SHA256=6f33e81444a9f4f07e9ae63d8931ea095e9df4e081fc50a927ed5f639a4cfb0c；NEEDS_FIX review 15785字节/122行/SHA256=0451147a5f372b3c63642173060a04079dc4d415baba8725af31d1c6dc95c529。本C1没有覆盖这三稿。
F1闭合到31旧文件清单/原身份/≤4行、1638+2230=3868、其余713、32before/64after及CB16/17/20；实际QueryRoster的迁移/保存/对象重建用例在原拟议PlayerPermanentSessionTests.cs内，与CB16格式核对共用，测试逐文件及2530总预算不加。
F2闭合到runLeaves 12叶、12条准确failure.json、有限数组258条及CB20；保持工具所有真实失败分支、实际文件≤260、manifest≤512/排除自身、不覆盖失败或复用槽位。设计阶段没有运行工具/Unity或提前造证据。
最终建议776实现/806Assets/425GUID保持。未来导出880=原841+原设计3+C1设计3+来源补签1+新增32；futureDesignGateFiles明确列六报告。未来两C实施报告及唯一代码审查路径保持，不在本阶段创建。
原TB-READ01 BLOCKED turn=01a0d8a2-b41c-7272-9b6d-f91225fb11fe于13:27:33.571Z completed；旧草案34999字节/0cca944938eb8ce8e3f531c5623b651240ef444fbc4ee5b7989bf605774e0a8e与2992667字节/f4c4f88f85f7f5b2f1689494f46543b09749b540e1d9923d53032ce35bd76ff5及被拒发送经过沿原scope继承。§342直接补读已解决该项，不重试跨任务发送。
本回合仅新建demo-cont-b-design-c1.md和demo-cont-b-design-c1-scope.json。scope绑定本C1设计与所有只读输入，scope自身SHA仅由formal final外部绑定，不预填原生完成或独立verdict。
原R本次准确新turn=01a0d8ec-c177-77a2-b35c-64653da5604b；须先核本C1准确原生completed、非异步formal DESIGN_COMPLETED及两C1实物，再只写demo-cont-b-design-c1-review.md给唯一设计结论。旧NEEDS_FIX/BLOCKED不能代替新门；作者正式完成后停改，C1独立接收后仍需SD00另签源码。
当前34功能/30已接收/余4/51计划交付；无新用户内容待答。CONT-C页面/导航、028战斗出口、029实际宿主及广告/云/新内容/战斗技能执行均不并入；公开AwaitLinks、§184、物理交互/PlayMode/Player/Android未验证边界及自动跟进取消保持。
