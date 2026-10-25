# CH1-SLICE-001：逻辑表、字段映射与代表行

2026-10-25 · 主程，身份／输入版本沿[工程设计](engineering-design.md)。本稿是Excel/CSV作者可用的表结构提案；所有行仅在Markdown中展示，未创建生产表、生成JSON或修改发布器。行为依据[本轮规则](../../game-design/content-planning/2026-10-25-ch1-slice-rules.md)，数值沿`candidate_r26_m12`等明确候选，不代表最终平衡。

## 1. 作者格式与发布约定

每张下列逻辑表对应一个Excel工作表或同名UTF-8 CSV。建议正式实施以版本库CSV作为唯一编辑输入，Excel用于编辑／导入导出同一份数据，不保留另一份独立修改的xlsx真源。沿现有Luban v5.1.0和`Config/FightMatch/Luban/luban.conf`组织生成；具体新增文件白名单由后续内容包列出。本轮不改该配置。

类型约定：`ID`为大小写敏感稳定字符串，非显示文案；`V`为正BigInteger十进制文本；`I`为非负BigInteger十进制文本；`Slot`为0起int；`Q`为精确有理数文本；`TextKey`引用已有本地化表。所有字段必填，只有带`?`的字段可空（CSV空单元格=明确null，不等于0）；禁止用空值触发隐式默认。集合字段使用`|`分隔，`-`表示明确空集合，ID不允许含`|`。枚举严格按下表，不接受大小写别名。

`Q`允许`24.64`或`616/25`等有限十进制／整数／分数；作者列须设Excel文本，解析时直接以十进制字符转BigInteger分子分母并约分，不经过float/double。运行生成物使用既有`ExactRational`分子／正分母规范表达。拒绝百分号、科学计数法、千位分隔、NaN、零分母及超出现有ExactMathBudget的输入。概率／比例是0—1的量，`21/100`不是21。伤害计算／显示舍入／实际HP损失不混成一个字段。

引用默认在同一ReleaseSet内解析，禁止查“最近一个同名定义”。改数值递增record/level版本并重新生成内容指纹，不改业务身份；ClassId、LevelId、SkillId、`Level16EntryGift`不能为重发奖励而换ID。记录版本／内容指纹不参与一次业务键。

表内坐标统一为**左上原点、1起(row,column)**；每面独立。生成`PairInput.Endpoint*`时`FlowPos(x=column-1,y=height-row)`；生成`DemoContentRouteInput.Cells`时仍为1起`(x=column,y=row)`，传`Coordinates=AssumedTopLeft`由现有Geometry转换一次。不可把端点的已转换坐标再次交给路线转换。阵位`OriginalSlot`、动作序号和面序号均0起，不能套坐标减一。

映射标记：**复用**=已有字段／算法；**扩展**=已有拥有者缺本切片字段或接线；**新增**=目前无该能力。引用真实源码均相对`Assets/Scripts/FightMatch/`；所有“拟增”类型不是现有API。

## 2. 发布与成长

### Release（单行）

| 字段／类型 | 单位／约束 | 映射 |
| --- | --- | --- |
| `package_id:ID, draft_id:ID, revision:V` | 包、草稿身份及正修订 | 复用`PublishedSource.PackageId/DraftId/Revision` |
| `source_schema:V` | 本切片拟为2；Schema1仍走旧demo-r1分支 | 扩展`PublishedContentModels`／`ContentConsumerCapabilities.Check` |
| `rule_version:ID, numeric_version:ID, random_version:ID` | 精确绑定；例`ch1-slice-r1 / RC01 / PC01+SC01`；保留现有字段版本名字，不误把NC01文档标题改成存档版本 | 复用`ContentBinding.RuleVersion/NumericContractVersion/RandomContractVersion`，新增受支持规则分支 |
| `required_capabilities:ID集合` | 原NC01.ExactRational／RC01.PCG32.PRD／PC01.ExactParameterEvidence／SC01.sc01-pcg32-le128-v1，加本切片能力 | 扩展能力声明／逐能力检查；不是填字符串即支持 |
| `release_set_id:ID` | 一次启用集合；不可把新内容配旧规则 | 复用Catalog的release-set索引，不新建独立可变规则指针 |
| `parameter_status:枚举, source_locator:ID` | `Trial`或`Approved`；本稿全为Trial；位置指向来源文件／小节 | 新增作者元数据进入规范payload／证据，不影响概率算法 |

内容指纹／PRD证明／生成器版本由编译过程输出，不手填假SHA；同ReleaseSet保存完整`ContentBinding`和Level的`DefinitionBinding`。正式发布还需其引用的1—7、10—15关及初始档定义完整；本稿只展示切片行，不能拿这些行单独冒充全章包。

### CharacterGrowth

| 字段／类型 | 单位／约束 | 映射 |
| --- | --- | --- |
| `class_id:ID, class_kind:枚举, passive_id:ID` | 本切片仅Warrior/Mage；每职业一条；passive必须属于该职业 | 复用`GrowthDefinitionInput.ClassId/ClassKind/PassiveDefinitionId`，发布入口由单Growth扩成Growths |
| `base_hp:Q, base_attack:Q, base_pdef:Q, base_mdef:Q, evasion:Q, attack_range:正int` | HP>0，其余数值≥0；当前evasion必须0；range为活敌序号上界 | 复用`BaseStats.MaxHp/Attack/PhysicalDefense/MagicDefense/Evasion/AttackRange` |
| `growth_hp:Q, growth_attack:Q, growth_defense:Q` | 前两项为每级相对基础值增幅，第三项为每级防御点；非负 | 复用`GrowthHp/GrowthAttack/GrowthDefense`，等级偏移L−1 |
| `xp_base:I, xp_linear:I, xp_quadratic:I` | XP/级需求的既有系数，base>0；不另存累计经验 | 复用`XpBase/XpLinear/XpQuadratic` |
| `recovery_ms:Q` | 毫秒，>0 | 复用`RecoveryDurationMilliseconds`；不使用本机时区秒数代替 |

### Passive（小型固定类型表）

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `passive_id:ID, class_id:ID, kind:枚举` | `WarriorCrit`或`ArcanePenetration`；职业／被动一一对应 | Warrior复用Crit；Mage需新类型化被动定义，不能借CritMultiplier表示忽略比例 |
| `p_base:Q, p_step:Q, p_cap:Q` | `p(L)=min(p_base+(L−1)*p_step,p_cap)`；0<p_base≤p_cap≤1，step≥0 | Warrior映射`GrowthDefinitionInput.CritBase/CritStep/CritCap`；Mage拟增同公式参数 |
| `crit_multiplier:Q?` | 仅WarriorCrit必填且≥1，Mage必须空 | 复用`CritMultiplier` |
| `ignore_magic_defense_fraction:Q?` | 仅ArcanePenetration必填，0—1；Warrior必须空 | 新增穿透参数，`D'=D*(1-ignore)` |

不设可编辑C或本次q列。沿既有精确PRD证明核，按支持等级的实际p生成匹配参数／误差证据，再绑定Entry；无证明不发布。机会只在实际普通法术命中时产生；无效操作、仅技能、火球和DOT不抽样。机会条件属于RuleVersion，不能被表格任意脚本覆盖。

## 3. 关卡、面、配对和默认路线

### Level

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `level_id:ID, level_version:V, recommended_level:V` | level/version联合唯一，推荐等级非入场硬门槛 | 复用`LevelInput.LevelId/LevelVersion/RecommendedLevel` |
| `unlock_rule_id:ID, unlock_kind:枚举, unlock_after_level_id:ID?` | `InitiallyOpen`须空前置；`AfterWholeLevelClear`须有前置，图无环 | 复用`CandidateProgressionLevelInput.UnlockRuleId/UnlockKind/UnlockAfterLevelId`；EntryKind本切片固定Ordinary |
| `reward_id:ID, tutorial_id:ID?` | reward同关同版；仅16引用其Teaching | Reward复用`PublishedLevelInput.Reward`；Teaching通过精确Level绑定关联，不新设第二份进度门槛 |

### Face

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `level_id:ID, face_id:ID, face_order:Slot` | 同关FaceId唯一；order从0连续；16恰两面 | 复用`LevelInput.Faces`顺序及`FaceInput.FaceId` |
| `width:正int, height:正int` | 格数；预算内；无需所有格子被覆盖 | 复用`FaceInput.Width/Height`及Flow验证 |

### PairEnemy（本切片敌人实例直接带属性，避免预建模板覆盖系统）

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `level_id:ID, face_id:ID, pair_id:ID, color_id:非负int` | 每面pair／几何色唯一；色是几何标识，不代替敌人类型 | 复用`PairInput.PairId/GeometryColorId` |
| `a_row:正int, a_col:正int, b_row:正int, b_col:正int` | 边界内、两端不同、全端点不重叠 | 复用`EndpointA/B`，按§1转换 |
| `enemy_instance_key:ID, enemy_definition_id:ID` | 实例在本关唯一且含面；definition只标种类，不额外加载另一个未列出的属性权威 | 复用`EnemyInput.EnemyInstanceKey/EnemyDefinitionId` |
| `original_slot:Slot, stable_order:Slot` | 每面唯一连续；样例二者相同；死亡保留槽 | 复用`OriginalSlot/StableOrder` |
| `max_hp:Q, attack:Q, physical_defense:Q, magic_defense:Q, evasion:Q, attack_range:正int` | HP>0，其余非负，evasion本切片0；range是通常近战范围 | 复用`EnemyInput.Stats`六字段；初始HP由max_hp唯一产生 |
| `action_set_id:ID, initial_cursor:Slot` | 非空周期；初游标小于周期长度 | 周期复用`EnemyInput.IntentCycle`；初游标需扩展，当前默认0不可暗当通用 |
| `wind_target_instance_key:ID?, wind_uses:I?, wind_fraction:Q?` | 三项同时有或同时空；仅含Wind动作实例使用；本面不同活目标，uses>0，0<fraction≤1 | 新增冻结上弦绑定／初量；当前计数在Snapshot，不回写作者表 |

### EnemyAction（有限枚举，无脚本／表达式语言）

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `action_set_id:ID, action_index:Slot, kind:枚举` | 每集合index从0连续；kind=`Strike/Charge/Rest/Wind` | Strike/Charge复用`EnemyIntentInput.Kind`，Rest/Wind为扩展 |
| `targeting:枚举?` | 攻击必填`FirstLegalLiving`或`PreferLivingMage`；非攻击空 | 前者映射既有FirstLiving并保留合法性校验；后者扩展，失败回退FirstLegalLiving |
| `damage_kind:枚举?, attack_override:Q?, coefficient:Q?, range:正int?` | Strike四项中damage_kind/coeff/range必填；override空则取实例Attack。非攻击全空。当前仅Physical | damage_kind/coefficient复用；override/range扩展，不能用一项Attack同时暗示26远程与12近战 |
| `exhausted_fallback_index:Slot?` | 仅Wind必填，引用同集合Strike；余次0时使用，执行后仍推进原Wind游标；不允许链式fallback | 新增明确耗尽行为；满血／死目标而尚有次数时空过，不走fallback |
| `next_action_vulnerability:Q?` | 仅核心重砸Strike用，>1；开启下一次有效玩家行动的自身直接受伤倍率，其他行空 | 新增有到期行动序号的快照状态和事实；不靠播放休整动画开关 |

Wind的奇偶来自本面每次有效行动的敌阶段和从0开始的`Strike→Wind`游标：第一阶段低攻、第二阶段上弦。每个活敌每阶段消耗一个意图，Charge/Rest/Wind空过也推进。全部敌人死时仍完成本次有效回合尾，但没有死人行动；不会给第二面提前推进一次。

### DefaultRouteCell

`level_id:ID, face_id:ID, pair_id:ID, cell_order:Slot, row:正int, column:正int`全部必填。联合主键为前三者+cell_order，每pair顺序从0连续；路线从A到B或B到A、正交相邻、无重格、不穿其他端点；同面这组默认完成线路互不相交。映射`PublishedLevelInput.SourceRoutes → DemoContentRouteInput.Cells`及既有`FlowSolutionValidator`。这是候选默认路线，不是唯一解证明或出手前封堵判断，允许玩家形成以后需撤销的合法坏路线。

## 4. 物品、学习、教学、正常奖励和一次效果

### Item、Card与Skill

| 表／字段 | 约束 | 映射 |
| --- | --- | --- |
| Item：`item_id:ID, kind:枚举, equip_class_id:ID?` | 证和经验卡使用已有`OrdinaryMaterial`容器语义且equip空；不是战术可装备物。普通材料同类 | 复用`CandidateInventoryItemInput.ItemId/Kind/EquipClassId`；作用由Skill/Card定义识别，不新增“战士证” |
| Card：`item_id:ID, record_version:V, unit_experience:I` | 本例50经验卡，单位XP>0；一物品仅一个有效Card定义 | 复用永久Card定义`Id=item_id/RecordVersion/UnitExperience`，该Kind的`ItemId`必须null；现有扣卡直接使用DefinitionId，不能另造card_id后扣不到物品 |
| Skill：`skill_id:ID, record_version:V, class_id:ID, cost_item_id:ID, battle_capability_id:ID` | 本切片仅战士嘲讽；证必须存在，能力须已实现 | 复用永久Skill定义`Id/RecordVersion/ClassId/ItemId/BattleCapabilityId` |
| Skill：`minimum_level:V, cost_count:I, cooldown_turns:I` | 本切片minimum_level必须5、cost_count必须1；CD2为样例。传3或2张证应报规则不支持，不能默改现行逻辑 | 前两者核对`CandidatePermanentProtocol`现有常量，不伪称已有可配字段；CD需新战斗定义 |

`Skill.Id`与其被Teaching引用的SkillId使用同一稳定ID；施放目标=连接目标、施放自动、嘲讽持续紧接一个敌阶段及不受普攻射程约束都由本切片规则固定，不造十余开关。

### Teaching

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `tutorial_id:ID, record_version:V, level_id:ID, level_version:V` | 仅本例16；必须绑定本Release的精确Level；身份跨数值版本稳定 | 复用永久Teaching `Id/RecordVersion/Level(DefinitionBinding)` |
| `class_id:ID, skill_id:ID, gift_item_id:ID` | 战士、嘲讽、同一通用证；须与Skill成本一致 | 复用`ClassId/SkillId/ItemId` |
| `learning_step_id:ID, explanation_step_id:ID` | 不同且稳定，不能随翻译变更 | 复用`LearningStepId/ExplanationStepId` |
| `gift_once_key:ID, gift_count:I` | 本例固定`Level16EntryGift`及1；不是可随版重置的行ID | 业务键关联既有赠证Effect／准确来源；count校验现有赠1语义 |
| `explanation_text_key:TextKey` | en/zh_cn均存在，参数签名一致 | 复用`FightMatchText.csv`及项目自有运行绑定；新增键另走后续内容包 |

教学阶段从原永久事实派生，不建一张玩家可编辑状态表。表不能自动学习或跳过说明；按钮只发原预览／确认请求。

### Reward及RewardMaterial

| 字段／类型 | 约束 | 映射 |
| --- | --- | --- |
| `reward_id:ID, record_version:V, level_id:ID, level_version:V, base_experience:I` | 与Level准确对应；base XP不是最终到账XP | 复用`CandidateRewardDefinitionInput.RewardDefinitionId/Version/LevelId/LevelVersion/BaseExperience` |
| `damage_weight:Q, taken_weight:Q, curve_base:Q, curve_log:Q, reference_hp_divisor:Q` | 非负，divisor>0；原评分公式保持 | 逐项复用`DamageWeight/TakenWeight/CurveBase/CurveLog/ReferenceHpDivisor` |
| `level_penalty_base:Q, overlevel_grace:I, zero_contribution_policy:枚举, drop_mode:枚举` | 0<base≤1；枚举`CurveIntercept`、`FixedOrdinaryMaterials` | 逐项复用同名字段；本切片不加入辅助13分或新DOT评分 |
| RewardMaterial：`reward_id:ID, item_id:ID, amount:I` | 联合主键；只普通每次结算奖励；0是明确0 | 复用`Materials[].ItemId/Amount`；首通证不能填在每次掉落里 |

`RequiredFeatures`由Release和本行需要的能力生成／校验，本切片无作者任意表达式。经验仍按原冻结参与者固定；材料由同结算联合入库存。

### CharacterUnlock与FirstClearItem

| 表／字段 | 约束 | 映射 |
| --- | --- | --- |
| CharacterUnlock：`class_id:ID, after_level_id:ID, initial_level_source:枚举, initial_xp:I, initial_active_skill_ids:ID集合` | 一职业一行；本例Mage/8；level source固定`ClearedLevelRecommended`，XP0、技能空；不含Formation槽 | 新增`CandidateChapterMilestones`职业取得能力；复用CreateCandidate、Roster和首通结果，获得证据保存原推荐等级值和原绑定 |
| FirstClearItem：`level_id:ID, item_id:ID, amount:I` | level/item联合唯一；amount>0；本例8/50经验卡/1及16/通用证/1 | 新增同结算的一次附加项；复用普通来源入账与首通事实，不能伪造广告来源 |

两表无可配置“按版本重发”。职业业务键为Player/Class，首通物品依原Player/Level首通结果；同首通完整向量一次固定，item行不是独立可重领按钮。16赠证另有Level16EntryGift，不与FirstClearItem共键。

## 5. 代表行（仅设计样例）

以下ID是提议的稳定作者ID；正式采用前核现有同义ID，采用后不得为了重领换ID。L8的5×5几何只是为展示字段构造，复用9的几何样本，不宣称已批准8棋盘。L9、16两面取既有逐关候选。所有普通奖励样例仍是候选；不会将8样例的W＋43XP当成`base_experience=43`。

```csv
# Release（required_capabilities为原四项加以下三项；此处明列完整集合）
package_id,draft_id,revision,source_schema,rule_version,numeric_version,random_version,required_capabilities,release_set_id,parameter_status,source_locator
fightmatch:chapter-one,draft:ch1-slice,1,2,ch1-slice-r1,RC01,PC01+SC01,NC01.ExactRational|RC01.PCG32.PRD|PC01.ExactParameterEvidence|SC01.sc01-pcg32-le128-v1|CH1.MageTauntBoss|CH1.Milestones|CH1.BusinessV5,release:ch1-slice-example,Trial,docs/game-design/content-planning/2026-10-25-ch1-slice-rules.md

# CharacterGrowth
class_id,class_kind,passive_id,base_hp,base_attack,base_pdef,base_mdef,evasion,attack_range,growth_hp,growth_attack,growth_defense,xp_base,xp_linear,xp_quadratic,recovery_ms
class:warrior,Warrior,passive:warrior-crit,100,20,10,6,0,1,2/25,3/50,3/2,60,20,5,180000
class:mage,Mage,passive:arcane-penetration,72,22,4,12,0,2,2/25,3/50,3/2,60,20,5,180000

# Passive
passive_id,class_id,kind,p_base,p_step,p_cap,crit_multiplier,ignore_magic_defense_fraction
passive:warrior-crit,class:warrior,WarriorCrit,1/5,1/200,7/20,3/2,
passive:arcane-penetration,class:mage,ArcanePenetration,1/5,1/200,7/20,,1/2

# Level（7和15只作外部同包引用，正式完整输入必须提供）
level_id,level_version,recommended_level,unlock_rule_id,unlock_kind,unlock_after_level_id,reward_id,tutorial_id
level:ch01-08,1,3,unlock:l08,AfterWholeLevelClear,level:ch01-07,reward:l08,
level:ch01-09,1,3,unlock:l09,AfterWholeLevelClear,level:ch01-08,reward:l09,
level:ch01-16,1,5,unlock:l16,AfterWholeLevelClear,level:ch01-15,reward:l16,tutorial:l16-taunt

# Face
level_id,face_id,face_order,width,height
level:ch01-08,face:l08-1,0,5,5
level:ch01-09,face:l09-1,0,5,5
level:ch01-16,face:l16-1,0,6,6
level:ch01-16,face:l16-2,1,5,5

# PairEnemy
level_id,face_id,pair_id,color_id,a_row,a_col,b_row,b_col,enemy_instance_key,enemy_definition_id,original_slot,stable_order,max_hp,attack,physical_defense,magic_defense,evasion,attack_range,action_set_id,initial_cursor,wind_target_instance_key,wind_uses,wind_fraction
level:ch01-08,face:l08-1,pair:a,0,1,1,3,2,l08-1-a,enemy:e01,0,0,16,14,0,0,0,1,cycle:e01,0,,,
level:ch01-08,face:l08-1,pair:b,1,1,4,4,4,l08-1-b,enemy:e03,1,1,20,14,300,5,0,1,cycle:e03,0,,,
level:ch01-08,face:l08-1,pair:c,2,3,5,5,2,l08-1-c,enemy:e01,2,2,16,14,0,0,0,1,cycle:e01,0,,,
level:ch01-09,face:l09-1,pair:a,0,1,1,3,2,l09-1-a,enemy:e01,0,0,16,14,0,0,0,1,cycle:e01,0,,,
level:ch01-09,face:l09-1,pair:b,1,1,4,4,4,l09-1-b,enemy:e03,1,1,20,14,300,5,0,1,cycle:e03,0,,,
level:ch01-09,face:l09-1,pair:c,2,3,5,5,2,l09-1-c,enemy:e01,2,2,16,14,0,0,0,1,cycle:e01,0,,,
level:ch01-16,face:l16-1,pair:a,0,1,1,3,2,l16-1-a,enemy:mechanic,0,0,18,4,0,0,0,1,cycle:mechanic,0,l16-1-k,2,1/10
level:ch01-16,face:l16-1,pair:b,1,1,5,4,4,l16-1-b,enemy:mechanic,1,1,18,4,0,0,0,1,cycle:mechanic,0,l16-1-k,2,1/10
level:ch01-16,face:l16-1,pair:k,2,3,6,6,2,l16-1-k,enemy:bear-shell,2,2,60,12,25,15,0,1,cycle:shell,0,,,
level:ch01-16,face:l16-2,pair:a,0,2,1,3,3,l16-2-a,enemy:heavy-guard,0,0,20,16,10,5,0,1,cycle:guard,0,,,
level:ch01-16,face:l16-2,pair:k,1,1,4,5,2,l16-2-k,enemy:bear-core,1,1,90,12,10,5,0,1,cycle:core,0,,,

# EnemyAction
action_set_id,action_index,kind,targeting,damage_kind,attack_override,coefficient,range,exhausted_fallback_index,next_action_vulnerability
cycle:e01,0,Strike,FirstLegalLiving,Physical,,3/5,1,,
cycle:e03,0,Strike,FirstLegalLiving,Physical,,13/20,1,,
cycle:mechanic,0,Strike,FirstLegalLiving,Physical,,1,1,,
cycle:mechanic,1,Wind,,,,,,0,
cycle:shell,0,Strike,PreferLivingMage,Physical,26,1,3,,
cycle:shell,1,Charge,,,,,,,
cycle:shell,2,Strike,FirstLegalLiving,Physical,,8/5,1,,
cycle:shell,3,Rest,,,,,,,
cycle:guard,0,Charge,,,,,,,
cycle:guard,1,Strike,FirstLegalLiving,Physical,,8/5,1,,
cycle:core,0,Charge,,,,,,,
cycle:core,1,Strike,FirstLegalLiving,Physical,,8/5,1,,5/4
cycle:core,2,Rest,,,,,,,
cycle:core,3,Strike,PreferLivingMage,Physical,26,1,3,,

# Item
item_id,kind,equip_class_id
item:profession-certificate,OrdinaryMaterial,
item:xp-card-50,OrdinaryMaterial,
item:tin,OrdinaryMaterial,
item:wood,OrdinaryMaterial,

# Card
item_id,record_version,unit_experience
item:xp-card-50,1,50

# Skill
skill_id,record_version,class_id,cost_item_id,battle_capability_id,minimum_level,cost_count,cooldown_turns
skill:warrior-taunt,1,class:warrior,item:profession-certificate,CH1.MageTauntBoss,5,1,2

# Teaching
tutorial_id,record_version,level_id,level_version,class_id,skill_id,gift_item_id,learning_step_id,explanation_step_id,gift_once_key,gift_count,explanation_text_key
tutorial:l16-taunt,1,level:ch01-16,1,class:warrior,skill:warrior-taunt,item:profession-certificate,step:l16-learn,step:l16-explain,Level16EntryGift,1,ch1.tutorial.taunt.explanation

# CharacterUnlock
class_id,after_level_id,initial_level_source,initial_xp,initial_active_skill_ids
class:mage,level:ch01-08,ClearedLevelRecommended,0,-

# FirstClearItem
level_id,item_id,amount
level:ch01-08,item:xp-card-50,1
level:ch01-16,item:profession-certificate,1

# Reward：8/9的基础E=18+2*关号，16基础E乘Boss系数3，均为候选，不是最终到账
reward_id,record_version,level_id,level_version,base_experience,damage_weight,taken_weight,curve_base,curve_log,reference_hp_divisor,level_penalty_base,overlevel_grace,zero_contribution_policy,drop_mode
reward:l08,1,level:ch01-08,1,34,1,1/4,1/2,1/2,2,3/4,1,CurveIntercept,FixedOrdinaryMaterials
reward:l09,1,level:ch01-09,1,36,1,1/4,1/2,1/2,2,3/4,1,CurveIntercept,FixedOrdinaryMaterials
reward:l16,1,level:ch01-16,1,150,1,1/4,1/2,1/2,2,3/4,1,CurveIntercept,FixedOrdinaryMaterials

# RewardMaterial：为映射构造的0掉落行，不覆盖策划正式卡/材料产出
reward_id,item_id,amount
reward:l08,item:tin,0
reward:l09,item:tin,0
reward:l16,item:tin,0
```

RewardMaterial的零行只演示普通材料映射，不替代正式材料产出；8首通卡已在FirstClearItem单列，卡经验定义沿既有Card协议，能够表达主策A-N及G01的卡来源／用卡交接，不另发明全章掉落。`ch1.tutorial.taunt.explanation`是拟新增TextKey，尚未声称当前264键包含它；实施时en/zh_cn缺任一即拒绝内容验收。

DefaultRouteCell以下按可读路径缩写展示，实际作者表每个格一行，不以路径字符串执行脚本；每条从左到右产生`cell_order=0…n−1`。L8示意采用L9同路径，生成时使用L8自己的关／面ID。

| level_id / face_id / pair_id | 1起(row,column)有序路径 |
| --- | --- |
| level:ch01-09 / face:l09-1 / pair:a | (1,1) (1,2) (2,2) (3,2) |
| level:ch01-09 / face:l09-1 / pair:b | (1,4) (2,4) (3,4) (4,4) |
| level:ch01-09 / face:l09-1 / pair:c | (3,5) (4,5) (5,5) (5,4) (5,3) (5,2) |
| level:ch01-16 / face:l16-1 / pair:a | (1,1) (1,2) (2,2) (3,2) |
| level:ch01-16 / face:l16-1 / pair:b | (1,5) (2,5) (2,4) (3,4) (4,4) |
| level:ch01-16 / face:l16-1 / pair:k | (3,6) (4,6) (5,6) (6,6) (6,5) (6,4) (6,3) (6,2) |
| level:ch01-16 / face:l16-2 / pair:a | (2,1) (2,2) (3,2) (3,3) |
| level:ch01-16 / face:l16-2 / pair:k | (1,4) (2,4) (3,4) (4,4) (5,4) (5,3) (5,2) |

```csv
# DefaultRouteCell首条完整四行，其他路径按相同结构展开
level_id,face_id,pair_id,cell_order,row,column
level:ch01-09,face:l09-1,pair:a,0,1,1
level:ch01-09,face:l09-1,pair:a,1,1,2
level:ch01-09,face:l09-1,pair:a,2,2,2
level:ch01-09,face:l09-1,pair:a,3,3,2
```

## 6. 发布检查、错误例与不支持范围

沿现有标准工具及发布验证分层：Luban负责类型／表引用，项目编译器负责规则约束、精确数值、版本绑定和几何，Catalog负责不可变发布和精确解析。错误定位必须含表、行主键、字段及原因；全部成功前不激活ReleaseSet，不以警告后填0继续。

| 错误输入 | 必须给出的错误／结果 |
| --- | --- |
| Passive忽略比例`50`或分数`1/0` | 概率范围或分母错误，禁止当50%／0 |
| Mage行填写crit_multiplier，或给学习最低级别3 | 字段组合／本规则不支持；不能复用战士暴击完成穿透，不能悄悄改Lv5 |
| L16小工绑定`l16-2-k` | 跨面上弦目标拒绝；不能去另一面给核心回血 |
| Wind余次空，或fallback指向自身Wind | 必需参数／循环fallback错误；不用无限次或递归解释 |
| 敌周期index为0、2或初游标99 | 非连续／越界，预告与执行均不发布 |
| 把第二证填入RewardMaterial每次奖励 | 本切片通用证位置约束拒绝，要求FirstClearItem；首证仍独立Teaching |
| 跨版本另建`Level16EntryGift_v2` | 业务身份冲突；同教学不可借版本再赠 |
| L9端点row0或不转换就当FlowPos | 坐标错误；(row1,col4)在5高面应为FlowPos(3,4) |
| 默认路线跳格、穿端点或同面相交 | 几何错误；不要求满板，不要求唯一解 |
| 新TextKey缺zh_cn或参数名不同 | 本地化缺项，不用成品Prefab文字掩盖 |
| 同ClassId重复／奖励经验含未参战新法师 | 作者重复身份／运行固定向量不一致；拒绝相应发布或结算 |
| 仅本稿3关行作为完整包发布 | 缺前置7/15、完整新档／内容引用、完整路线／PRD证明；本稿片段不能独立发布 |
| 旧活动局请求新RuleVersion定义 | Exact绑定不一致；继续旧精确集合或报缺原定义，不默用最新 |

本切片缺口不是Luban安装：现有文本链已使用v5.1.0；缺的是这些作者表、类型化转换和能力实现。技能／Boss／里程碑／V5标“新增/扩展”的行目前均不能进入现有demo-r1消费者。保留现有发布／保存拒绝，不关闭预算／支持域守卫。

本稿静态核对只证明文档字段、示例引用、几何和算术内部一致；不等于代码测试、完整内容可发布、棋盘战斗能通关或最终平衡接受。工程行为／失败恢复和后续验收入口见[工程设计](engineering-design.md#9-后续必要验收场景)。

2026-10-25静态结果：16张逻辑表的CSV列数一致；4面／11敌实例／14意图的本地引用、初游标、同面上弦绑定及枚举组合一致，明确的外部前置7/15和拟新增TextKey保持未提供状态。默认路径占格分别L8示意14、L9为14、16第一面17／第二面11，均正交连续、端点正确且同面不交叉；精确魔攻两支、成长／p曲线及32HP目标恢复16/5的算例相符。两份工程文档共10个本地链接目标存在。没有执行产品测试、Luban生成或发布。
