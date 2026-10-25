# CH1-SLICE-001 必要验收场景与设计接收

2026-10-25。负责人：主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local`；唯一接收者：中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。

**状态：已按中央冻结输入完成17组场景的一次对齐。结论：ACCEPT（仅设计可交付）。** 下列预期是后续实施的验证要求，静态算例不记作运行通过；首Demo既有ACCEPT保持原范围。准确输入见第4节，交付及未验证边界见第5节。

## 1. 输入与本轮边界

- [当前设计包](../../plans/2026-10-25-next-stage-design-index.md#当前设计包-ch1-slice-001)及两份计划版本`1ae884bc`：[内容计划](../../plans/2026-10-25-chapter-one-content-design-plan.md)、[工程计划](../../plans/2026-10-25-android-first-release-engineering-design-plan.md)。检查点`f94240fa`和已审产品`dece3d8c`的采用边界沿索引，不把检查点全树视为已审产品。
- 冻结输入：[主策规则](../../game-design/content-planning/2026-10-25-ch1-slice-rules.md)、[工程设计](engineering-design.md)、[表结构](table-schema.md)。三份全文已读，SHA256与中央冻结通知一致；这里只接收这组指纹的设计，不把后续改稿自动纳入。
- 用户授权先细化8／9关法师、16关教学与两面Boss的规则和表结构，普通技术判断自主推进。主测试唯一写入本文件；本轮只核设计及静态例子，无产品、表资产、Unity、设备、测试运行、网络服务、Git或新代码审查。
- 最小贯通为8奖励／拥有角色→手动编队／9普攻对照、16领证／补级／学习／说明／入场、两面Boss／回退／最终结算。火球、DOT及后续职业只检查本次必需的兼容和拒绝边界，不加入首次8／9与16通关前提。

## 2. 使用方式与共用判据

以下每行是一组相同业务语义下的必要分支，未来实施只补新增规则和受影响的接线。表中的“复用”指旧证据在原输入上证明的通用合同，不能把新增法师、教学行或Boss内容标成已经运行通过。后续验证方式只列所需层次，不授权现在运行，也不要求每层重复所有分支。

永久状态包括角色拥有、永久成长／学习、赠证与保留、教学进度、奖励收据；战斗历史包括该局队伍HP／倒下、CD、持续效果、携带消耗、随机推进、线路及面进度。临时页面和表现不能成为这些状态的第二写入者。取消不消费；确定未提交保留原状态；结果未知保留原操作／提交身份并查询，只有Completed才能展示其业务结果，不能靠再次发放或再次扣除解决。所有新增战斗字段及事实须进入保存、指纹与历史恢复校验。

精确伤害中间值、按既有出口floor后的伤害、受护盾／当前HP限制的实际损失必须分别记录；上弦恢复不走伤害floor。Boss算例只采用`candidate_r26_m12`，不混入旧候选。概率、伤害、路线及掉落示例仍为Trial，不是最终平衡或生产内容。

文本观察只覆盖本次新增关键说明／门禁／结果：英语与简中含义一致，数值与状态来自绑定，缺绑定保留诊断占位。复用现有uGUI／本地化合同，不重跑旧Demo整套双语与设备验收。

## 3. 最小必要场景

### A. 8关奖励与9关法师

| ID | 固定输入 | 操作 | 预期可观察状态 | 保存／恢复 | 旧证据复用／后续验证方式 |
| --- | --- | --- | --- | --- | --- |
| A1 8奖励与拥有幂等 | 原8关WonPendingSettlement、法师未拥有；固定原参与者奖励／8关推荐等级／结算OperationId。A-N：战士Lv.3 XP87，已固定XP43 | 明确结算；同请求查询及新请求重复；保存结果未知后恢复；玩家配置9关阵容 | 同一候选联合提交原奖励、8首通、开9、法师取得和首通经验卡。A-N战士Lv.4 XP10；法师按原8推荐等级Lv.3／XP0／Ready，有天生穿透、主动技能空，不分原8经验。CharacterId稳定唯一；Roster新增而Formation不变，玩家自行选战士／法师／空位 | 职业键(PlayerId,ClassId)、首通键(PlayerId,LevelId)不含版本或新请求ID；未知期间法师不可用；Completed才完整可见。缺必要定义拒绝整项，仍保留待结算，不补发修复半提交 | 复用PrepareVictory→Finish、FixNormal／ApplyBaseReward／EndAttempt及结果查询；新增里程碑联合提交做Application验证，实际奖励→拥有列表→手动编队观察一次 |
| A2 9同甲普攻与穿透机会 | 同版本厚甲HP20／物防300／魔防5；战士Lv.3攻击22.4，法师Lv.3攻击24.64、射程2、未学主动。固定穿透不触发／触发1/2两分支，p=21/100仅为目标概率 | 同甲比较；检查9关活敌排序射程；分别推进符合／不符合条件的穿透机会，读取各角色PRD前后 | 战士28/5→floor5→损5，非暴击需4次；法师未触发2464/105→23→损20，触发4928/205→24→损20。未触发已一击，不靠火球或降甲。A-N入场战士Lv.4 HP124／法师Lv.3 HP83.52，三行动后103／83.52。射程按活敌排序、原槽不挪；初始C为第3活敌不可达，A死后C为第2可达 | 仅实际向活目标发出且未被闪避的普通魔攻产生机会；魔防0、floor伤害0或全盾吸收仍计机会。非法／未发射／仅技能／火球／DOT／补线／复播不抽。各角色／被动FailureCount独立；倒下／复活／翻面保留，回退恢复，重开按Entry初值；不读档重抽 | 复用精确算术、射程／原槽、一次行动与随机合同；新增Mage伤害、机会门和PRD分支做纯规则验证，9关实际目标反馈一次；静态数值不是运行证据 |

### B. 16关证书、教学与入场

| ID | 固定输入 | 操作 | 预期可观察状态 | 保存／恢复 | 旧证据复用／后续验证方式 |
| --- | --- | --- | --- | --- | --- |
| T1 首证一次与保留 | 首次合法进入16教学、无其他活动／待结算局。G为赠证事件、E为该证仍持有数量、O为其他证；初始G0／E0，分别O0／O1 | PrepareTeachingEntry领证、返回、重进；尝试以首证学习法师技能；另以合法普通证作兼容分支 | G1／E1；可建立Challenge但未建16 Attempt。E按准确来源端点保留给战士教学；E1／O0不能供法师用，E1／O1可在原能力与等级合法时用O且不动E。此分支不要求本切片发布法师主动技能 | 教学业务键(PlayerId,Level16EntryGift)固定，不因版本／新OperationId重赠；来源矛盾明确拒绝保档，不猜测补证。显示未更新／未知／重进均恢复原赠物结果；战斗回退不撤销 | 复用BeginTeachingGift、库存来源与AllowsCertificate；新增16绑定／保留接线做Application验证，实际门禁说明观察 |
| T2 等级不足后回刷 | 已领E1，战士低于Lv.5；卡足够／不足。G01：战士Lv.4 XP157/165、法师Lv.5 XP92/220，3张50经验卡 | 玩家明确将3卡用于法师；战士仍不足时返回旧关，战士单人回刷L1获固定XP14；重进16 | 法师Lv.6 XP22、卡0，战士仍Lv.4 XP157；回刷后战士Lv.5 XP6。学习前仍守Lv.5要求；不自动耗卡、不替玩家转分配、不要求广告。Card定义Id=item_id，ItemId=null，成本用DefinitionId扣正确物品 | 各永久成长结果分别保存；教学返回／回刷不重赠、不提前建16 Attempt，不把临时页面当进度。卡不足明确拒绝原请求，活动局限制沿原导航规则 | 复用CalculateCards、永久成长与旧关导航；新增教学↔现有导航接线，做G01针对验证及一条无广告实际补级／返回观察 |
| T3 明确学习与已有技能 | 战士Lv.5、未学嘲讽、E1；另设已用独立合法证学过嘲讽且原学习凭据完整。技能skill:warrior-taunt | 取消预览、确认、重复／过期确认、查询未知结果；已学分支引用原学习结果后确认说明 | 未学成功同一办理E1→0＋学习＋learning_step；取消不扣，StaleContext重读，重复不扣。已学分支不再扣证，不再学一次；若E1仍在，说明确认前继续保留，确认后解除来源限制而数量仍1 | 学习／扣证同一候选提交；未知保留原请求，Completed后下一步仅说明。已学引用OriginalLearningOperation；缺失／矛盾来源不能用新扣证或补证修复 | 复用PreviewPermanent／ConfirmPermanent、FindLearning及AlreadyLearned；新增教学关联／来源分支做Application验证，实际取消／确认各看一次 |
| T4 独立说明与入场门 | 未领、未达等级、未确认学习、已学未确认说明、说明已完成；tutorial:l16-taunt绑定准确16版本，步骤step:l16-learn／step:l16-explain | 从各阶段请求入场、退出重进；确认说明后重新读取成长／阵容／技能并按H02入场 | 每个缺口只引导所缺步骤；说明是独立明确提交，引用学习OperationId与StepId。完成后才创建Attempt，技能CD初值0；不换入口绕过、不用可变UI枚举作第二真相。入场失败不撤销已完成永久教学 | G／学习／说明由已有事实派生；重进恢复正确阶段。Boss回退／重来不倒退教学；已完成不重教、不重扣。新增说明键ch1.tutorial.taunt.explanation英／简中均有绑定 | 复用教学确认、导航准入、Formation Entry冻结；新增旁路门禁／接线做Application验证，实际uGUI完整教学→一次入场，不重复全部恢复分支 |
| T5 最终首通另一证 | 正常已学路径G1／E0／O0，F为16整关首通第二证；第一面完成、整场WonPendingSettlement、已结算三状态；另看已学旧技能而E1仍持有分支 | 第一面观察；第二面完成后经表现结束／Skip明确结算；重复／未知恢复及复打 | 第一面无整场奖励；WonPendingSettlement仍F0；唯一整场结算后F1／O1，教学G不变。第一枚未消费的已学分支可持有两枚。第二证来自FirstClearItem，不能混入每次RewardMaterial；不自动学法师技能，复打不再给首通证 | 整场原结算OperationId联合固定基础奖励／首通一次效果；胜利保存与结算分开。未知查询同一结果，不能因新版本／新请求重领；回退不能复制已兑现永久收益 | 复用胜利→表现→结算及一次到账；新增两面终局／首通身份做增量验证，实际第二面结束／奖励一次观察 |

### C. CD、嘲讽与两面Boss

| ID | 固定输入 | 操作 | 预期可观察状态 | 保存／恢复 | 旧证据复用／后续验证方式 |
| --- | --- | --- | --- | --- | --- |
| B1 CD与核心易伤窗口 | 嘲讽CD0、Skill.cooldown_turns=2；另有核心实际重砸后生成5/4窗口，绑定下一EffectiveActionOrdinal | 施放后推进两个完整有效队伍行动；对窗口分别打核心、打其他敌人、合法仅技能，以及无效输入／等待／Skip／补线／翻面 | 施放当次CD2，后续有效行动依次1、0，法师行动也推进战士旧CD。窗口仅覆盖紧接的一次完整有效玩家行动：打核心直接伤害乘5/4；打别人／仅技能也在该行动末到期。无效／表现／补线／翻面既不减CD也不耗窗口；不能等到下次命中核心才清 | CD按角色／技能存快照；窗口存EnemyKey、精确倍率、唯一行动序号；保存／回退恢复同值，复播不再次推进。C3行动8开窗，9法师对核心floor30、HP66→36并关窗 | 复用完整行动和表现无副作用；新增CD及窗口的相邻行动规则例；真实技能／易伤反馈观察，不引入墙钟计时测试 |
| B2 嘲讽目标与敌周期 | 普攻后目标活／死、超出普通射程目标、CD未好；固定原槽及活人排序。护壳周期远程26→蓄力→近战12×8/5→休息，核心蓄力→重砸→休息→远程 | 一次普通攻击／被动后处理当前角色技能，再按StableOrder处理活敌；另看战士死亡或非合法近战目标、嘲讽遇蓄力／休息 | 普攻击杀省略嘲讽且不设CD；合法远目标且CD0可仅技能、无近战伤害；普攻不合法且技能不可用则整项拒绝零推进。只影响本次连接且仍活的敌人。远程优先合法活法师，否则FirstLegalLiving；嘲讽优先合法活战士，战士不合法则回原规则。蓄力／休息也消费这一敌阶段的嘲讽，不能留到下次攻击 | TargetEnemyKey／CasterKey／AppliesToEnemyPhaseOrdinal及施放事实随操作保存；指定敌阶段后清，不能跨面套到新敌人。每实例IntentCursor只推进一次；预告从游标推导，恢复／复活不重演敌阶段 | 复用射程／原槽、目标与敌阶段基础；新增仅技能、击杀省略、职业优先及合法回退做纯规则验证；Boss意图→受击实际观察 |
| B3 每只小工独立上弦 | l16-1-a／b各RemainingWindUses=2，按A再B绑定同面l16-1-k，恢复比1/10；首项低攻，下一项Wind。另用MaxHP32／HP20／次数1精度例 | 分别从护壳HP60、59、50、0执行两只小工的Wind；再查次数耗尽后的Wind项 | HP60：各恢复0、次数2/2；59：A补1后B补0、次数1/2；50：A补6至56、B补4至60、次数1/1；0：不复活、不改绑且不耗次。32精度例恢复16/5、HP116/5、次数0，不floor为3。尚有次数但满血／死只空过；耗尽后Wind才执行配置的低攻fallback，两者均只推进游标一次 | 用量属于每只小工，不共享Boss总次数；后者读取前者实际HP。精确HP、各用量、目标及游标随历史保存；恢复／回退不能重复治疗。fallback必须指同周期Strike，不递归 | 复用精确有理数和快照；新增顺序恢复、独立次数、满血空过／耗尽低攻做纯规则边界，实际观察一次恢复／剩余量 |
| B4 补线、翻面与全员倒下 | 活敌未清、敌全灭但有PendingLinks、所有必需pair已锁；存在普通空格。另设间接击杀后全员倒下且仍欠线，分别第一面／最后一面 | 正常行动或免费CompleteLink；检查AwaitLinks、翻面和终局优先级 | 活敌未清不翻；间接死亡欠线先AwaitLinks，即使全员倒下也可补。所有必需线齐才翻面，无全覆盖要求。第一面补齐后新面若全员倒下则AwaitRescue；最后一面补齐则WonPendingSettlement，不强迫免费／广告复活。仍有活敌且全倒则AwaitRescue | 补线和翻面本身无伤害、敌阶段、回合、CD、持续效果或PRD推进；保存欠pair／准确面及阶段，恢复不跳线、不重复翻面。胜利待结算后不允许回退／重开／退出改写结果 | 复用CompleteLink／Stage.Finish及阶段合同；新增两面准入、全倒优先和零推进做纯规则例，实际一次补线→翻面观察 |
| B5 两面状态延续 | 同一Challenge／Attempt／EntryBaseline；队伍残血／倒下、携带已耗、非零CD、PRD／随机已推进；face:l16-1→face:l16-2 | 达成翻面后读状态、保存重开；对齐C3第6行动后的继续点 | 玩家HP／倒下／携带／CD／有效玩家状态／随机流／贡献延续，不回血补货。C3翻面战士HP66、法师89.28、CD1；新护卫20／核心90，各IntentCursor0从蓄力开始。本面FaceEnemyPhasesCompleted归0，全局EnemyPhasesCompleted保留；每个有效完整行动计一次，即使当次无活敌 | 新敌人按本面定义初始化，旧敌用量、嘲讽及目标绑定不移植；旧面历史仍留。保存包含准确面和新增状态，恢复同一继续点；永久教学不变。C3全11行动后战士28／法师89.28／CD1、WonPendingSettlement，结算不是第12行动 | 复用Attempt／Entry冻结和冷恢复；新增面、快照与编码做规则＋Application验证，实际第二面／一次恢复，不重跑旧三状态全套 |
| B6 跨面回退 | C4：C3第7行动后，第二面战士66／法师89.28／CD0、护卫20／核心66；选回到第6行动之前 | 预览撤销第6、7行动；取消；重新确认；另查保存未知及旧回调 | 取消零变化；确认恢复第一面A／B死且锁线、护壳19未锁、战士66／法师89.28／CD2、护壳下一项蓄力；道具、用量、PRD／随机、贡献等均还原锚点。再次法师击杀护壳后CD1，新面重新为20／90且蓄力初态 | 仅Completed发布恢复面与新SceneRevision，旧操作Superseded；未知沿原确认查询，不重复回退。永久学习／赠证消费／说明／偏好及受保护广告事实保持；复播读事实，不重抽或留下旧面回调 | 复用PrepareRollback、原确认失效和来源保护；新增跨面锚点／字段恢复做规则＋Application验证，实际预览范围可读与一次确认 |

### D. 表引用、支持域与旧档版本

| ID | 固定输入 | 操作 | 预期可观察状态 | 保存／恢复 | 旧证据复用／后续验证方式 |
| --- | --- | --- | --- | --- | --- |
| D1 正常表到发布映射 | 冻结schema与Trial代表行；拟SourceSchema 2／PublishedSourceV2、规则ch1-slice-r1；唯一Excel/CSV作者源，Luban生成产物非第二编辑源 | 逐项追到模型／入口；后续获准实现后再以同源生成与发布 | ID大小写敏感；Q保留整数／小数／分数精确值，空与0不同；引用限同ReleaseSet。表坐标左上1基(row,col)，端点FlowPos=(col−1,height−row)：高5、行1列4→(3,4)；路线保留SourceRoutesCells 1基(x=col,y=row)，只在现有几何入口转换一次。Card.Id=item_id／ItemId=null；新文本英简中参数一致 | 一次集合绑定内容指纹／规则／数值／随机／能力，精确历史引用不漂移。样表仅3关，缺7／15等闭包、完整新档和证明，必须保持Trial且不可发布；不能把反复生成当成运行验证 | 复用现有Luban、几何校验、PreparedPublication／Catalog.Publish；新增内容适配做生成／发布针对验证，旧L1只验证受影响兼容差量 |
| D2 引用与字段拒绝 | 每次只破坏一个稳定键／引用／数值／坐标／文本。代表例：穿透比50或分母0、Wind跨面目标、次数空、fallback回Wind、周期0/2或初游标99、行0 | 后续交标准生成及现有发布校验；静态预期明确表／行／字段 | 重复键、缺引用、越界／跳格／穿端点／交叉路线、缺简中／参数不符均拒绝。第二证填RewardMaterial拒绝，须FirstClearItem；把gift_once_key改成_v2规避一次身份拒绝；Card另造别名而扣不到item拒绝。不补默认、不丢坏行、不激活半包；不要求全覆盖或唯一解 | 失败保持原启用集合和玩家资料；纠正后的内容按完整新发布身份进入，不能原地改已发布同一版本；局部表有效不等于整个ReleaseSet可发布 | 复用类型／引用／发布检查；只补本批新字段与跨表约束错误例，不造额外验收框架 |
| D3 能力支持域与PRD证据 | 合法Mage普攻／Warrior嘲讽；错职业、未知效果、未实现能力、缺PRD证明。新增required_capabilities含CH1.MageTauntBoss、CH1.Milestones、CH1.BusinessV5，原精确数值／随机／证明能力继续保留 | 对发布、永久学习、Formation Entry及战斗入口追踪支持域；分别提供完整能力／缺一项 | 有表行不等于可执行。错职业／未知能力明确拒绝，不静默降级；现有Demo守卫不能简单删除。每个支持等级p须由现有精确PRD参数证明支持，p不是当前q，作者不手填C凑过。首链不要求随机穿透成功、火球、DOT或后续职业 | 不支持新定义时保留已学技能和旧局，拒绝相关新入口；不能通过清技能／重置档绕开。新规则、数据、程序能力须一起解析启用，缺项不切换 | 复用CheckBattleCapability、Growths／PreparePermanent和证明校验；仅新增最小合法／错误域与实际加载接线验证，不扩成全职业实现 |
| D4 旧档原绑定与迁移边界 | 旧L1活动局／待结算／未决操作原v1，对照新内容v2；原绑定可取／缺失。拟业务V5 PublishedChapterV5及保留的V2／V3／V4 | 载入原局、查原奖励；另在后续明确授权且无活动状态时检查V4→V5迁移意图 | 安装新程序不自动迁移；旧局仍用原RuleVersion／ContentBinding／数值／随机，不重算旧奖励；ResolveExact失败不退到latest。未知格式／能力明确UnsupportedSchema／UnsupportedBinding保档。V5五切片统一头，保留旧reader/writer，旧历史按原规则解释，不补造新事实 | 迁移绑定SourceCommitId、generation、descriptor SHA/length、target binding；沿已有V2→3→4前提再4→5。逐项证明成长／XP／物品／开放关卡／技能／来源语义不变，变动整项拒绝，沿snapshot→commit marker恢复。迁移不发法师／证／首通奖励；旧8已过而无取得依据拒绝猜补 | 复用精确绑定、存储原子协议和旧版本路径；新增格式／迁移差量只在实施获准后验证，当前不运行迁移、不扩所有设备／版本组合 |

## 4. 冻结输入及逐项映射

中央通知相关作者已停止写入，且主策已确认工程两稿无实质冲突。本次按下列SHA256读取并对齐；章节号均指这些冻结版本。

| 输入 | SHA256 | 采用范围 |
| --- | --- | --- |
| [主策规则](../../game-design/content-planning/2026-10-25-ch1-slice-rules.md) | `e470f0be5dccefa46f8fed5adc97c3575878289e89c3f1629fe643e92db9da15` | §1精确值；§2 A1～A3／A-N；§3 B1／B2及G01；§4 C1～C4；§5字段责任 |
| [工程设计](engineering-design.md) | `c62fb7082e2f8c90f4f60f7b951b445a3e8a77843709d46fcda0c29980518e53` | §1现有入口；§2状态拥有者；§3～6联合提交／教学／战斗／历史；§7兼容；§8仅候选实施范围 |
| [表结构](table-schema.md) | `1b4323166b858c07e37dfa3162569d7db99e2f71c8b01b145da008353b9be1ee` | §1类型／坐标；§2～4表字段；§5 Trial代表行；§6发布拒绝与支持边界 |

| 场景 | 冻结表字段／规则 | 状态归属与现有能力对应 |
| --- | --- | --- |
| A1 | 规则§2；schema§4 CharacterUnlock.after_level_id／initial_level_source=ClearedLevelRecommended／initial_xp=0／initial_active_skill_ids为空；FirstClearItem；Level推荐等级／解锁 | 工程§3：拟CandidateChapterMilestones.ApplyVictory并入既有Finish，先固定原参与者奖励。Progression拥有ChapterMilestoneEffect的业务键／SourceClear或SettlementId／原绑定／OperationId及角色或Grant引用；Roster／Inventory持引用，Formation仍由玩家编辑 |
| A2 | 规则§1、§2；schema§2 CharacterGrowth、Passive.kind／p_base／p_step／p_cap／ignore_magic_defense_fraction；§3 PairEnemy属性／original_slot | 工程§3.2、§6：EnterFormation冻结成长／技能／被动；新规则普通魔攻复用精确计算，BattlePassivePrdState按combatant／passive／参数绑定保存FailureCount；表不提供C／q |
| T1／T3／T4 | 规则§3 B1／B2；schema§4 Teaching.level_id／level_version／gift_once_key／gift_count／learning_step_id／explanation_step_id／explanation_text_key，Skill.class_id／cost_item_id／minimum_level=5／cost_count=1 | 工程§4：PrepareTeachingEntry→BeginTeachingGift；PreviewPermanent→ConfirmPermanent；独立ConfirmTeachingExplanation引用原学习。Roster持学习，Inventory持准确赠物端点，Progression持教学事实；状态派生，不新增第二份教学枚举 |
| T2 | 规则§3 G01；schema§4 Card.item_id／record_version／unit_experience=50 | 工程§1、§4复用CalculateCards与永久成长。已定Card.Id=item_id、Card.ItemId=null符合现有定义校验及按DefinitionId扣卡；不得另建card_id映射第二真相 |
| T5 | 规则§3 B2、§4 C3／C4；schema§4 FirstClearItem.level_id／item_id／amount=1，16指向item:profession-certificate | 工程§3、§5、§6：Stage终局后沿原Finish／保留结算请求；ChapterMilestoneEffect固定FirstClearItem类型及原首通来源。教学赠证仍是独立永久Effect，不合并身份 |
| B1／B2 | 规则§4 C1～C3；schema§4 Skill.cooldown_turns；§3 EnemyAction.kind／targeting／attack_override／coefficient／range／next_action_vulnerability | 工程§5拟CandidateChapterBattleRules.EvaluateAttack按普通攻击→被动→技能→活敌→旧CD／效果末处理→阶段判断；§6 BattleMemberSkillState、BattleTauntState、BattleEnemyVulnerabilityWindow及对应事实。外层仍复用Prepare／Submit，不让UI计时推进 |
| B3 | 规则§4 C2；schema§3 PairEnemy.wind_target_instance_key／wind_uses／wind_fraction，EnemyAction.exhausted_fallback_index | 工程§5、§6：每个BattleEnemyState.RemainingWindUses与IntentCursor；同面绑定、顺序读取当前HP、精确恢复；满血空过与耗尽Strike分开，事实记实际恢复与用量 |
| B4／B5 | 规则§4 C1／C3／C4；schema§3 Face.face_id／face_order，PairEnemy.enemy_instance_key／stable_order／initial_cursor | 工程§5、§6：复用Stage.CompleteLink／Finish及同一EntryBaseline，快照持面身份、锁线／PendingLinks、FaceEnemyPhasesCompleted；新敌初值与延续的玩家／全局状态分属不同对象 |
| B6 | 规则§4 C4；工程§6新增字段／事实的完整锚点 | 复用CandidateHistoryOperations.PrepareRollback与原确认协议；新增面、敌游标／用量、技能CD、窗口、PRD／随机及贡献都入指纹和恢复。永久Progression／Roster学习不被战斗历史反向覆盖 |
| D1／D2／D3 | schema§1～6，Release.source_schema／rule_version／required_capabilities／release_set_id／parameter_status；各跨表引用和本节明确拒绝例 | 工程§1、§7：现有Luban→校验→PreparedPublication→Catalog.Publish、FindExact及能力守卫；拟PublishedSourceV2／本切片编译分支为新增实现，保留Schema1／demo-r1／31参数检查。Trial及不完整集合不能启用 |
| D4 | 工程§7拟PublishedChapterV5／原版本读写／准确迁移来源；schema§6旧活动局错绑拒绝 | 五业务切片及应用意图／结果统一新增编码校验，继续现有snapshot→commit marker协议；旧绑定、旧事实和永久来源保留。SourceSchema 2与业务V5是不同层，不能把导表升级视作自动迁移授权 |

现有入口仅作定点映射核对：[CandidatePermanentProtocol](../../../Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs)可见按DefinitionId扣卡、已有学习不收费及StaleContext；[CandidatePermanentDefinitions](../../../Assets/Scripts/FightMatch/Core/CandidatePermanentDefinitions.cs)可见Card的ItemId必须为空。这是设计复用核对，不是对现有或未来代码追加Code Review。其余新增类型／分支仍以工程稿明确标记的“拟增”身份接收。

## 5. 设计接收结论

**ACCEPT（仅CH1-SLICE-001冻结设计可交付）。** 17组必要场景已对应到规则、表字段、状态拥有者、原请求恢复及现有能力入口；联合奖励、首证／第二证、已学免重复扣证、精确恢复、穿透机会、CD／易伤窗口、全倒补线、跨面回退及旧档绑定的分支相互一致。本轮未发现需要作者先行修正的实质冲突或需要用户重新选择的产品事项。

本轮完成证据限三份冻结稿全文与SHA核对、17组映射、上述少量现有入口核对，以及本文件链接／表列数／场景数量的静态检查。没有运行Unity、设备、产品测试、服务或Git，也没有取得本切片代码审查或运行通过结论。

后续仍须按获准实施范围完成：本批能力与实际加载接线；SourceSchema 2及业务V5的编码／兼容实现；完整生产表、发布闭包及PRD证明；Trial数值和内容审定；准确代码head的独立GitHub PR Code Review及相关增量验证。实际观察至少覆盖第3节指定的奖励／编队、教学回刷／学习／入场、Boss关键反馈／翻面／回退／最终结算和新增双语绑定；复用旧证据的部分保持原输入范围。样表不能直接发布，C3／C4静态推演不能替代真实操作、恢复或设备结果。

唯一回执交中央。本判定不升级首Demo历史结论，不授权现在迁移旧档、实施、运行、上传或发布，也不建立新的等待／轮询任务。
