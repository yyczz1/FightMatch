# CH1-SLICE-001：8／9关、16关教学与双面Boss工程设计

2026-10-25 · 详细设计，尚未实施。负责人：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；actual turn `01a16eee-2231-74d1-a85f-3348aa5d817b`；中央唯一接收者。用户已授权按计划继续细化并自主处理普通技术判断。

输入为[当前设计包](../../plans/2026-10-25-next-stage-design-index.md#当前设计包-ch1-slice-001)、计划快照 `1ae884bc`、工作区检查点 `f94240fa72eb9df4e616fe04c56a1fe3068b73b1`、已审产品 `dece3d8c589742b5ee3d75b2e255c97f6bf2495b`。工作区含WIP；下述“现有”指实际可读源码，不表示整树已审或功能已验收。规则由[主策本轮规则稿](../../game-design/content-planning/2026-10-25-ch1-slice-rules.md)统一，字段见[表结构](table-schema.md)。本轮只写这两份工程文档。

## 1. 实施结论和阅读入口

沿用分层、QFramework注册、自有uGUI控制器和“业务提交成功后播放事实”的结构。新能力集中在现有业务Module内；不增加仅转发调用的通用技能框架，不替换保存协议，也不把教学再做一套。

| 用例／实际入口 | 已有Implementation | 本切片必须补的能力 |
| --- | --- | --- |
| 8关结算：`PlayerSessionSystem.PrepareVictory` → `CandidateLifecycleApplicationSystem.Finish` | `CandidateBaseRewards.FixNormal`先固定原参与者经验；`CandidateCharacterGrowth.ApplyBaseReward`、`CandidateProgression.EndAttempt`、库存结束及原子提交 | 在同次结算候选中按首次通关依据加入唯一法师，保留取得凭据；不把法师加入第8关奖励向量 |
| 9关入战：`PrepareFormation`／`PrepareFormationEntry` → `CandidateLifecycleRoster.EnterFormation` | 三槽阵容、Ready筛选、成长计算、进度检查、入场冻结、随机种子和历史 | 多职业正式发布；法师普通魔攻／穿透；入战资料不再给所有角色套战士Crit或清空已学技能 |
| 16关战前：`PrepareTeachingEntry` → `PreviewPermanent`／`ConfirmPermanent` | `CandidatePermanentProtocol`及`CandidatePermanentProgression`已有赠证、准确来源用途限制、学习和独立说明步骤；已学技能可引用原结果 | 正式Teaching／Skill定义、页面接线、能力支持检查；用稳定教学身份跨版本识别原赠物，不能重领 |
| 战斗：`CandidateBattleApplicationSystem.Prepare/Submit` → builders → `CandidateBattleOperations.EvaluateAttack` | 路线合法性、修订／身份校验、顺序事实、历史锚点、保存和发布 | 法师普通命中、穿透、嘲讽／冷却、Boss循环和上弦；旧demo-r1执行分支保留 |
| 补线／翻面／撤销 | `CandidateBattleStage.CompleteLink/Finish`、`CandidateHistoryOperations.PrepareRollback`、原应用提交 | 新状态进入完整快照／事实／校验／指纹；翻面只换本面敌人与棋盘，保留队伍状态和同一Attempt |

源码入口：[Application](../../../Assets/Scripts/FightMatch/Application/CandidateLifecycleEnd.cs)、[永久办理](../../../Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs)、[战斗快照](../../../Assets/Scripts/FightMatch/Core/BattleSnapshot.cs)、[发布器](../../../Assets/Scripts/FightMatch/Content/PublishedContentCompiler.cs)。当前 `CandidateDirectAttack`、`CandidateBaseRewards`、`CandidatePermanentGrowth.CheckBattleCapability`仍限制Warrior／无已学技能；`PublishedSource`仍单Growth，发布器固定demo-r1及31个参数。因此“已有Mage枚举和多角色容器”不等于已支持法师战斗，不能仅删守卫即宣告完成。

## 2. 谁拥有状态

| 状态 | 唯一权威及最小内容 | 更新／回退规则 |
| --- | --- | --- |
| 内容 | `ContentBinding`＋`DefinitionBinding`；角色成长、关卡／面／敌人、技能、周期、教学、奖励、里程碑 | 发布不可变；表格是编辑权威，生成物不是第二份手改配置 |
| 永久角色／阵容 | `CandidateRosterState`；角色等级／经验／恢复，三槽明确选择；已学技能沿永久Effect。新增职业取得引用 | 8关结算才取得法师；不自动填空槽。战内撤销不撤销已取得职业／已学嘲讽 |
| 永久库存／进度 | 原库存来源端点、首通事实、Teaching Effect；新增本切片里程碑结果引用 | `Level16EntryGift`与16首通发证分来源。一个持久提交同时改变相关拥有者，不能先给证后写防重 |
| 当前战斗 | `BattleSnapshot`、冻结`EntryBaseline`；HP、当前面、路线、行动／敌阶段计数、每角色技能CD、被动PRD、每敌游标／剩余上弦次数、嘲讽目标和到期阶段 | 随有效操作生成新快照；不从动画、墙钟或UI缓存推导 |
| 历史 | `CandidateBattleHistory`、原操作事实、前后快照、原绑定、随机流、rollback记录 | 跨面恢复所有战斗状态；恢复后从对应原绑定计算，不能将现在配置套到历史 |
| 表现 | 原已提交事实投影、选择／临时拖线、播放位置、文本绑定 | 选择／播放／Skip／导航不推进CD、意图或随机；缺文本显示诊断占位符 |

法师射程仍按当前存活敌人顺序判断。`OriginalSlot`用于固定位置和未来邻接语义，死亡不重排原槽；不得为了火球兼容把普通攻击射程改成原槽距离。火球、DOT只保留既定兼容要求，本切片首次16前没有合法火球学习样本，不在此实现或另开数值设计。

## 3. 8关结算与9关首次法师

### 3.1 一次联合结算

1. 沿原`PrepareVictory`核对保留的结算OperationId、Attempt／EntryBaseline和FinalReport。胜利待结算不直接给经验或职业。
2. `FixNormal`仅按冻结的8关参与者固定奖励。先对那些角色应用固定整数经验，依原结束事实办理恢复与携带归还。
3. 若本次建立8关整关首通，读取`CharacterUnlock`定义，核对`(PlayerId, ClassId)`尚无正常取得结果。按**本次8关原绑定的RecommendedLevel**创建法师，XP=0、Ready、无主动技能、带天生被动；候选R8=3，不硬编码3。CharacterId生成一次并存入原候选，恢复时复用。
4. 同一候选包含首通／开放9、法师取得记录、角色与必要空战术槽、原奖励及原操作结果。`Formation`保持原值。若任一必需定义或成长计算不支持，整个结算候选拒绝，原胜利待结算和结算身份保留。
5. 经原`CandidateApplicationRuntime`提交／恢复校验后，页面才显示“法师已加入”；同意图重复请求返回原结果，新OperationId也不能绕过职业业务键。重复打8只给正常本次奖励，不再次创建法师。

新增能力拟为Core内部`CandidateChapterMilestones.ApplyVictory`，接收已固定的原结算结果和精确定义，返回完整拥有者变化及事实。它不存盘、不抽随机、不重新评分。既有`Finish`负责组合，不让UI单独调用“发职业”接口。

### 3.2 9关入场与一次普通法术

玩家明确确认样例阵容`[战士, 法师, 空]`，沿`SetFormation`保留修订；`PrepareFormationEntry`核当前提交、三槽、Ready及准确关卡绑定。8关新法师不自动成为8关参与者，未选入阵容也不强行加入9。

兼容细节：现有`CreateCandidate`还要求角色级`OriginalSlot`，但Roster的实际入场槽已由`PrepareEntry`遍历Formation下标产生。新法师可显式以0初始化该旧兼容字段，不改Formation、不据它推断参战；V5入场仍唯一取玩家选定的Formation槽。不能调用`CandidateRosterState.Legacy`重建阵容而把新角色自动塞入0槽。

`ComputeBaseStats`继续产生ComputedBaseStats；新增法师战斗定义直接引用独立`ArcanePenetration`参数，`Crit`允许按职业为空。入场冻结全部实际已学技能及其绑定；不能再构造`LearnedSkills = new List<string>()`。射程2的法师可以打当时第2位活敌；不合法路线／距离且无可执行技能的动作原样拒绝，不耗回合或随机。

有效普通魔法命中先获得该角色的穿透机会；不额外要求HP损失>0。使用原精确PRD算法／随机流，本角色被动失败计数独立；触发后本次法防乘`(1-ignoreFraction)`。不是对最终伤害加一个概率倍率，也不能套用战士暴击Multiplier。例：普通魔攻24.64、魔防5、未触发时中间值`2464/105`，既有伤害出口floor=23，目标HP20则实际损失20；三个值分别留作计算事实。p=`21/100`、忽略`1/2`均为试调值。PRD的C通过现有精确证明核生成／核验，不手填近似C；缺匹配参数证明拒绝发布。

## 4. 16关赠证、回刷、学习、说明

沿已有`CandidatePermanentDefinitionKind.Teaching/Skill`，不增加第二个教学存档。只在玩家明确进入已开放16的请求中调用`PrepareTeachingEntry(exactLevelBinding, expectedCommitId)`，查询页面不赠物。

| 当前事实／玩家操作 | 调用与结果 | 关闭、失败、重进 |
| --- | --- | --- |
| 尚未领首证，进入16 | `BeginTeachingGift`：库存准确来源+1、赠证Effect、可无Attempt的Challenge同次提交 | 写入结果未知沿原OperationId恢复；新进入请求查稳定赠证键，不再+1 |
| 已领证，战士Lv4 | 读永久View展示“需Lv5”及现状；允许返回旧关、培养预览 | 没有Attempt、没有自动扣卡；赠证事实及用途限制继续存在 |
| 达Lv5，玩家确认嘲讽 | `PreviewPermanent(LearnSkill)`核修订、职业、成本来源；`ConfirmPermanent`联合扣证、记录技能及学习步骤 | 取消预览无变化；过期预览StaleContext重读；重复提交不再扣证 |
| 战士此前已合法学嘲讽 | 复用原学习结果，补本教学关联；不重复收费 | 若首证尚在，继续受用途限制至说明完成 |
| 玩家确认用途说明 | `ConfirmTeachingExplanation`引用原学习Operation及StepId，独立持久步骤 | 不能因关闭页面当已读；完成只解除尚持有首证的用途限制，不加证／返证 |
| 教学完成，再点击入场 | 重读提交／阵容／能力→`PrepareFormationEntry`；冻结EntryBaseline后才有Attempt | 无Ready角色、资源或规则能力不足分别报原原因，不撤销已学技能或重新发证 |

正常路径为“首证1→学嘲讽后0→16整关首通第二证1”。首证限制的是准确赠物端点，不是通用证整个ItemId；其他合法证仍可按原规则使用。普通菜单与教学菜单都走同一永久办理核。重复进入、内容升级或换OperationId均不改变`Level16EntryGift`身份。缺失／矛盾来源保留为具体核对失败，不补证修复。

## 5. 两面Boss的计算Interface

外部仍由应用提交一次Attack或CompleteLink。建议新增Core内部Module `CandidateChapterBattleRules.EvaluateAttack`，把本切片多个阶段放在一个可读入口；它接收不可变快照、已校验请求、冻结定义和预算，返回含前后状态／顺序事实的结果。仅以本切片RuleVersion选择它；旧demo-r1继续原路径。其内部普通伤害／目标选择复用现有精确算法，避免UI各自判断。

```csharp
// 设计伪码：新规则分支，类型／函数名不是已存在的实现。
var direct = ResolveNormalHit(before, request);       // 没有合法普通命中则不抽被动
var passive = ResolveEligiblePassive(direct);
var skill = ResolveReadyTaunt(passive);              // 目标已死则不施放、不进CD
var enemy = ResolveSurvivingEnemyPhase(skill);       // 本切片无新增DOT执行
var after = FinishValidTeamTurn(enemy);             // 本次施放的技能不在本回合扣CD
return AssembleFactsAndStage(before, after);         // 补线/翻面/结算判断
```

后续DOT若接入，其位置仍是技能之后、敌方行动之前的全场一次阶段；本切片不新建DOT评分或脚本语言。仅技能行动在路线合法且技能可实际施放时成立：开场熊王在第3存活位，普攻够不到，已就绪嘲讽可作用该目标而形成有效行动。

| 能力 | 明确的状态与执行规则 |
| --- | --- |
| 嘲讽 | 目标是本次连接的仍存活敌人；不受普通攻击射程限制。该敌人本次敌方阶段优先合法存活战士；近战仍守自身合法范围，战士不合法时按原目标规则选择，不强行命中。目标在普攻中被杀则不施放／不扣CD |
| CD | 每角色每技能`CooldownRemaining`，入场0。成功施放设2（试调）；施放当次不减，以后每个完整有效全队行动减1至0。补线、翻面、待机和表现不减；CD=0且本次选该角色才可施放 |
| 敌周期 | 每个实例的`IntentCursor`已经存在；新增明确的Ranged／Melee／Rest／Wind动作。按本面配置顺序，存活敌依StableOrder执行；首游标由内容给出。意图预告从当前游标推导，不另存会漂移的“下一招” |
| 远程 | 普通选择合法存活法师；无合法法师按定义的FirstLegalLiving回退。嘲讽优先于职业偏好，不能选死人 |
| 上弦 | 每只小工的RemainingWindUses，固定同面目标实例；按行动时的目标当前HP算`min(maxHP*ratio, maxHP-HP)`。真实恢复>0才减一次，目标满血／死亡不消耗；两只小工顺序结算后者重读HP。恢复保留有理数，不floor |
| 上弦空过／耗尽 | 小工从本面首项低攻开始，下一项Wind循环。尚有次数但目标满血／死时本次空过且游标推进；次数已为0时，后续Wind项改执行已配置低攻，仍推进一次，不无限递归fallback |
| 核心易伤 | 重砸后只对紧接的下一次完整有效玩家行动开放自身直接受伤×5/4窗口；打别人或合法仅技能，该行动末也到期。无效请求、补线、翻面、表现不消耗；不等到下次命中核心才清除 |
| 伤害／贡献 | 中间值、原出口伤害、实际HP损失分开。上弦实际恢复不伪装玩家伤害；本切片不自行追加“辅助13分”等旧演算候选评分 |
| 面完成 | 当前面所有必需pair均已锁定才可翻面；直接击杀锁本次路线，间接死亡保留PendingLinks。补线无攻击／敌阶段／随机／CD；无全覆盖要求 |
| 翻面 | 同一Challenge／Attempt／EntryBaseline；新建第二面预配置敌人及其本面游标／用量，保留玩家HP、CD、PRD、随机流、全局计数和贡献。不额外敌阶段、不把护壳复活为核心 |
| 结束 | 第一面无结算。第二面全pair完成进入WonPendingSettlement，仍沿唯一保留结算OperationId；整关16首通才联合发第二枚通用证 |

嘲讽状态包含`TargetEnemyKey, CasterKey, AppliesToEnemyPhaseOrdinal`，只在指定阶段有效，处理后清除；翻面不能把旧面嘲讽套到新敌人。若未来允许跨阶段状态，应另有规则，本切片不预建该能力。上弦用量属于小工实例，不能按全队或Boss共用2次。

## 6. 快照、历史与恢复的最小增量

当前`BattleMemberState`只有HP，`BattleEnemyState`有HP和IntentCursor，`BattlePrdState`绑定`PreparedWarriorCrit`；必须补真实模型，不能将新状态塞进UI字典或只写日志。

| 拟增字段／记录 | 内容及保存位置 |
| --- | --- |
| `BattleMemberSkillState[]` | Character combatant key、SkillId、CooldownRemaining；冻结技能定义引用在Entry，当前CD在每个快照。施放当回合的排除集仅在本次运算中使用，无跨回合布尔 |
| `BattlePassivePrdState[]` | combatant key、PassiveDefinitionId、类型、参数绑定、FailureCount；替代新规则分支的战士专用PRD行，旧格式仍用原行 |
| `BattleEnemyState.RemainingWindUses` | 仅Wind能力实例有非负计数，其他实例为空；初值从冻结内容复制 |
| `BattleSnapshot.FaceEnemyPhasesCompleted` | 本面已完成敌阶段数，从0起；每个有效完整行动加1，即使该阶段已无活敌；翻面归0，整局EnemyPhasesCompleted保持累计。每实例首游标索引0，护壳从远程、护卫／核心从蓄力开始 |
| `BattleTauntState[]` | 上节三字段，记录当次技能效果及敌阶段消费结果；通常操作结束为空，事实仍能证明转移目标。暂停／复活不得重放已完成敌方行动 |
| `BattleEnemyVulnerabilityWindow` | 目标EnemyKey、精确倍率、唯一`EffectiveActionOrdinal`；核心重砸后绑定当前行动数+1，完整有效行动末清除，随快照／回退恢复。不是永久属性或动画计时器 |
| `ChapterMilestoneEffect[]` | 进度拥有，固定两种Kind：AcquireClass、FirstClearItem；业务键、SourceClear/SettlementId、原ContentBinding、创建CharacterId或库存Grant引用、原OperationId。角色／库存持引用，不复制第二个防重集合 |
| 新事实 | 普通魔攻、穿透机会／PRD前后、嘲讽施放／目标选择、CD变化、实际上弦及计数、周期前后；全部进入操作记录、指纹及恢复校验 |

`CandidateHistoryOperations`的锚点／定位／回退Interface复用；其验证必须覆盖新增字段。跨面回退恢复原面棋盘、敌人HP／游标／用量、玩家HP／CD、PRD和贡献，保留永久学技／赠物事实。原操作重放读取保存的事实；新动作从恢复后的随机状态继续，不能为了复播再抽一次。

## 7. 发布与存档兼容：仅提出一个受控新格式

建议新增**业务格式V5（暂名PublishedChapterV5）**与本切片规则`ch1-slice-r1`；这是实施候选，不是本轮迁移授权。保留现有五个业务切片与snapshot→commit marker原子协议。V5加入上节状态及结果引用，并对应用意图／结果增加对应编码与恢复校验；不新增第二个保存文件或更换`LocalSaveStore`。

现有五切片格式选择依首切片schema并统一校验，故不能只在battle末尾偷偷附字段。实施V5需明确五切片V5头，未变字段保持原语义；保留V2／V3／V4原reader/writer与原规则解释器。V5内保留的旧战斗记录按其**原RuleVersion及ContentBinding**解码；不得把历史补成新规则事实。未知V5能力或缺原发布定义应明确UnsupportedBinding/UnsupportedSchema，保留原文件，不能空状态降级。

存量L1仅安装新程序不会升级：有ActiveHistory、待结算Continuation或未决操作时，继续原输入并完成／退出原流程，不进入迁移。无活动状态时，另行显式执行一次带`SourceCommitId＋generation＋descriptor SHA/length＋target binding`的迁移意图；先沿已有V2→V3、V3→V4协议满足前提，再V4→V5。目标必须逐项证明现有职业成长／经验单位、物品单位、已开放LevelId、已学SkillId及旧来源可映射且语义不变；不符则具体拒绝，不重新评分或补领。

V4→V5只建立新规则所需的空当前战斗状态和已核对的身份引用，不创建法师／首证／首通奖励。既有永久Effect、首通、原获得凭据、保留战斗及绑定完整保存；目标内容替换当前定义的对应关系必须列入迁移结果。若发现“已8首通但无职业取得依据”等旧数据异常，报告核对缺口；不要猜测补角色。初次实现优先用显式新建的本切片档验证，存量L1仍沿原路径运行。

发布路径继续为Excel/CSV→现有Luban v5.1.0→生成中间数据→类型／引用／规则／几何校验→`PreparedPublication`→`PublishedContentCatalog.Publish`。新增`PublishedSourceV2`及本切片编译分支，旧Schema1/demo-r1/31参数检查原样保留。`PublishedRuleDefinitions.Growths`、`PreparePermanent`和`FindExact`已有基础；补正式加载接线，不绕过校验向容器注入数据。

一次ReleaseSet绑定内容指纹、RuleVersion、NumericContractVersion、RandomContractVersion和所需能力。新规则ID、数据和程序能力作为一个可解析集合启用；只下载某张表不激活半套规则。历史／活动局精确引用的发布定义保留；`ResolveExact`失败不得退到“最新”。一次业务键不含版本：职业`(PlayerId,ClassId)`、首通`(PlayerId,LevelId)`、教学`(PlayerId,Level16EntryGift)`；版本只作证据绑定。

## 8. 后续实施的最小候选文件范围

以下是设计提出的候选，后续实施包应按一项能力缩成白名单；不是本轮可写路径。文件名均以`Assets/Scripts/FightMatch/`为根。

| 工作 | 修改候选 | 新文件候选与理由 |
| --- | --- | --- |
| 正式内容入口 | `Content/PublishedContentModels.cs`、`PublishedContentCompiler.cs`、`PublishedContentCodec.cs`、`Core/PublishedSaveContext.cs` | `Content/ChapterContentCompiler.cs`、`ChapterContentModels.cs`：本切片类型化编译，不扩demo验证器的巨型分支 |
| 职业取得／第二证 | `Application/CandidateLifecycleEnd.cs`、`Core/CandidateRosterState.cs`、`CandidateInventoryState.cs`、`CandidateProgressionState.cs`及对应保存校验 | `Core/CandidateChapterMilestones.cs`：固定两种业务效果及其不重复核验 |
| 入场／永久办理接线 | `Application/CandidateLifecycleRoster.cs`、`PlayerPermanentSession.cs`、`Core/CandidatePermanentGrowth.cs`、`CandidatePermanentProgression.cs`、`BattleEntryInput.cs`、`BattleEntryPreparer.cs`、`PreparedBattleEntry.cs` | 不新建第二套教学Module；按实际差量补V5能力／原赠物关联 |
| 战斗计算 | `Core/CandidateBattleOperations.cs`、`CandidateDirectAttack.cs`、`CandidateEnemyPhase.cs`、`CandidateBattleStage.cs`、`BattleSnapshot.cs`、`BattleEntryBaseline.cs`、`CandidateBattleOperationRecord.cs`、`CandidateBaseRewards.cs` | `Core/CandidateChapterBattleRules.cs`：内部有序计算入口；类型化CD／被动／上弦记录与主模型相邻放置 |
| V5及验证 | `Core/CandidateBusinessSnapshot.cs`、`CandidateBusinessSaveCodec.cs`、`CandidateBusinessSaveValues.cs`、`CandidateBattleSaveCodec.cs`、`CandidateRosterSaveCodec.cs`、`CandidatePermanentSaveCodec.cs`、`CandidateApplicationIntent.cs`、`CandidateApplicationIntentCodec.cs`、`CandidateApplicationSaveCodec.cs`、`CandidateBusinessRestoreChecks.cs`、`CandidateBattleReportFingerprint.cs` | 新迁移函数放既有生命周期partial；确需独立文件才在实施包列全路径。不改Platform原子写协议 |
| 玩家入口与显示 | 现有`Presentation/PlayerNavigationController.cs`、`PlayerNavigationView.cs`、`PlayerBattleView.cs`及战斗表现投影；只接本切片事实／文本 | 具体uGUI资产及生成入口先核现物后列包，不预授权Scene／Prefab／Host构建器整片重写 |

代码按正常分行、领域命名和短分支书写。外部只学会既有Prepare/Submit、永久预览确认及查询Interface；复杂规则集中在Core内部，便于同一入口验证。只对真实变化点留Seam：发布内容、存储和UI投影已有Interface；不为一个Boss先造通用策略注册器。

## 9. 后续必要验收场景

本轮未运行以下检查；它们是后续实现的可观察标准，复用未受影响的旧证据。

| 场景 | 必须观察到的结果 |
| --- | --- |
| 8首通结算／相同请求恢复／新ID重复 | 原参与者固定XP一次；唯一法师R8/XP0，阵容不变；获得依据可追到原首通；无半个联合提交 |
| 9首次入场／普通魔法命中 | 正确冻结两角色、射程按活人排序；穿透机会条件和PRD前后可核对；火球／DOT没有穿透抽样 |
| 首证后Lv不足返回回刷 | 首证仍唯一保留，旧关正常可进；没有提前创建16的Attempt |
| 学习取消／过期／未知提交／已有嘲讽 | 取消无扣证；过期重读；未知恢复原请求；已有技能不再收费，说明仍独立 |
| 首证保留与普通菜单 | 首证不能用于未完成教学前的法师学习；另一合法证不被整类封锁 |
| 仅嘲讽有效行动／普攻击杀目标 | 前者推进一次；后者不再施放、不进CD；不可执行动作不耗回合 |
| Boss目标／周期 | 活法师优先；合法嘲讽改战士；死人或非法近战对象不选；两面首次游标与表一致 |
| 满血／部分缺血／两只上弦 | 满血不耗次；实际恢复保留分数；后执行小工读前一只结果，每只独立用量 |
| 核心易伤 | 重砸后下一有效行动对核心的直接伤害含5/4；打别的目标或合法仅技能也到期；无效输入／补线不耗 |
| CD与补线／翻面 | cast设2、后两次有效队伍行动依次1/0；补线和翻面不额外递减或抽随机 |
| 跨面回退／恢复／复播 | 面、HP、CD、用量、游标、PRD、贡献与锚点一致；复播不重算；永久教学不回退 |
| 16最终结算与复打 | 第一面不发奖励；整关一次结算，首次额外证一次且来源区别于首证；复打不重复首通证 |
| 新旧内容及保存 | 旧L1不迁移仍可按原绑定读取；缺旧定义明确失败；V5不能被旧reader误读为V4；迁移输入变更整项拒绝 |

设计交付不等于产品ACCEPT；后续源码仍需准确head的GitHub PR Code Review及相关验证。当前未发现需要用户重选的产品事项；试调概率、伤害／周期候选在表内标记，后续内容审定不阻止本轮结构交付。

本轮完成证据限文档静态核对：两文件链接目标存在，表的列数／本地引用／候选路线与精确算例一致，易伤窗口及上弦空过／耗尽语义已与主策本轮稿对齐。数值、生产内容、存量V5迁移和实机效果均未实施或运行；仅这两份工程文档由本聊天写入。
