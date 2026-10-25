# 首章开发作者数据 · r3（1—16关）

2026-10-25。CH1-IMPL-001主策 actual turn `01a17122-c32f-7853-9295-ae67fb9e36f3` / thread `01a0f2e3-2c47-7273-88da-20513bcbfaf4` / local。主程唯一接收，范围见[实施记录](../../../docs/implementation/2026-10-25-ch1/README.md)。

**这是后续作者版本，Trial，尚未生成或发布。** 本目录16张逻辑表加`SourceMetadata.csv`是r3唯一编辑输入；[r2目录](../ChapterOneTrial/README.md)及其Generated/r2保留为冻结历史，不作为另一份可独立编辑的最新版。279键文本源仍冻结，本稿末尾只交新增文本需求。

## 版本与内容

- Release修订3，包仍`fightmatch:chapter-one-trial`，发布集合仍`release-set:chapter-one-trial`；`source_locator`指向本目录。沿原七项能力另加精确状态合同`CH1.TauntBossStateV1`。主程已确认：Skill的`battle_capability_id`仍为`CH1.MageTauntBoss`，新token不替换技能字段。教学与Boss完整执行／保存能力到齐后才一次发布r3。
- 从r2原文件逐字保留1—9的行与前缀；旧角色、职业、关卡、奖励和首通业务ID不换。三个Release字段变化之外只追加记录：16关、17面、47敌实例、20条意图、208格默认路线。没有重复获得法师、另建玩家档或跳关入口。
- 10—15的6面和16的2面直接使用[既有试排](../../../docs/game-design/balance/results/boards.json)，不重新生成棋盘。普通敌人属性来自[敌人表](../../../docs/game-design/content-planning/chapter-one-enemies.csv)，Boss沿`candidate_r26_m12`；均为开发候选，未声明试玩或最终平衡接受。
- 普通材料仍沿已授权锡片预算：10/11/13/14/15各2，12为3，16整场6。新增12首通50XP卡1张、16首通50XP卡2张及通用证1枚；既有4/8首通卡保持。证不放在重复掉落中，第一面没有独立关卡或奖励。
- SourceMetadata保持三列`path,locator,sha256`，共30条真实来源，包括r2的17张CSV及新增内容的具体来源。现有新档recipe／31条PRD证明仍按原消费协议从旧Demo定义只读导入，不手填C，不读取或修改真实玩家资料。

## B03教学与普通到达条件

16只能在正常完成15后开放。Teaching绑定`level:ch01-16@1`、`tutorial:l16-taunt`、`skill:warrior-taunt`；入场赠证键仍`Level16EntryGift`，数量1；学习与用途说明分别为`step:l16-learn`和`step:l16-explain`。学习要求战士Lv.5、通用证1枚，候选CD2。达到等级不自动学习，领证不建立活动Attempt，玩家明确学习和确认用途后才正式入场。

正常首通4/8/12可得到三张50XP卡，供玩家自行分配；若进入16时战士未达标，仍沿已定缺口提示、正常用卡或回到已开放关卡获取经验，不补档、不免费升到5。既有合法已学、未知提交及退出重进分支继续复用[规则B](../../../docs/game-design/content-planning/2026-10-25-ch1-slice-rules.md)。候选数量保证存在培养来源，不证明任意分配或未验证路径自动达标。

入场首证和首通第二证分开：`Teaching.gift_count=1`，`FirstClearItem(level16,certificate)=1`。本次没有法师主动技能行，火球／DOT仍不进入这版切片。

## B04周期、上弦及两面

| 作用域 | 首项及周期／目标 | 精确配置 |
| --- | --- | --- |
| 普通E05（10/11/15） | Charge→Strike，`LastLegalLiving`，范围3 | 物理系数9/10；后排不等同法师职业 |
| 普通H（12/13/14） | Strike→Wind，耗尽时Wind回退同周期Strike | 低攻系数1/4；各2次；恢复最大HP的1/10；12绑定C，13/14绑定A |
| 16第一面小工A/B | Strike→Wind，稳定顺序A→B→护壳 | 每只18HP、攻击4、各2次，均固定绑定`l16-1-k` |
| 第一面护壳 | 远程→Charge→重砸→Rest | 60HP、双防25/15；远程26、范围3、优先合法法师；近战12×8/5、范围1 |
| 第二面护卫 | Charge→重砸 | 20HP、双防10/5、16×8/5、范围1 |
| 第二面核心 | Charge→重砸→Rest→远程 | 90HP、双防10/5；近远程沿护壳；重砸后的下一次有效玩家行动对核心直接伤害×5/4 |

所有首次游标为0。每个活敌每阶段推进一次；Wind满血／死目标空过也推进，但余次保持；余次为0才使用已配置低攻fallback。绑定不跨面、不换目标、不复活；各小工顺序重读目标HP。12关C保留32HP，满次上弦恢复16/5=3.2，不复用早期稿的floor3。

核心易伤是下一次完整有效玩家行动窗口，不等待下一次打中核心；攻击别人或合法仅技能也会到期。无效请求、免费补线、翻面和表现不消耗。同Attempt翻面继续我方HP/CD/随机等状态，新面敌人从配置首项开始，整关只在最终面结算；跨面回退／全灭补线沿冻结规则，不由作者表另造状态机。

## 静态结果与实际接入前提

本轮常规CSV核对已完成：类型／列数、完整1→16前置链、职业／物品／奖励／技能／教学引用、30条源哈希、各面路线端点／边界／正交／互不相交、稳定阵位／初游标／完整周期、5处同面Wind绑定及耗尽fallback、唯一核心易伤行。新增8面坐标与旧试排逐格相同，r2其余16CSV前缀逐字保持。

17张CSV按文件名排序，每行`文件名:sha256\n`合并后SHA256：
`867a9ab630e8d6d6201fe3d5d8b71c8db4cdec016c5b348967242523eebf7462`。

原r2集合SHA仍`5faccf6b422ac9348a46a19d7783edb369949dc86b304e71bccad9b83ad29f19`；279文本源SHA仍`3a47be4b0efbddeccf185aea28be9336f4ed1a1fb755a294d0e3b224800dde35`。本次不运行Luban、Unity、旧校准模型或发布，不手工生成运行JSON。正常前置／培养资源／技能成本在作者层齐备；实际通关、敌人意图显示、永久步骤和故障恢复仍须对应实现及C验证，不能把静态几何当战斗可玩证据。

## 后续新增英简中文本清单（31键，未写279源）

`Teaching.explanation_text_key`唯一新表引用为`fm.ch1.tutorial.taunt.explanation`，当前279键没有它。以下是本轮交主程的具体文案输入，参数集合在两种语言中一致；接入时复用自有uGUI绑定，不往Prefab写成品文案。

| Key | 参数 | 简体中文 | English |
| --- | --- | --- | --- |
| `fm.name.item.profession_certificate` | — | 职业之证 | Class Certificate |
| `fm.name.skill.warrior_taunt` | — | 嘲讽 | Taunt |
| `fm.name.enemy.spring_thrower` | — | 弹簧投球手 | Spring Thrower |
| `fm.name.enemy.mechanic` | — | 上弦小工 | Winding Mechanic |
| `fm.name.enemy.bear_shell` | — | 熊王护壳 | Bear King's Shell |
| `fm.name.enemy.heavy_guard` | — | 重锤护卫 | Heavy Hammer Guard |
| `fm.name.enemy.bear_core` | — | 熊王核心 | Bear King's Core |
| `fm.teaching.taunt.title` | — | 战前教学：嘲讽 | Pre-battle Lesson: Taunt |
| `fm.teaching.taunt.gift_received` | amount;itemName | 已领取首次入场赠物：{itemName} ×{amount}。 | First-entry gift received: {itemName} ×{amount}. |
| `fm.teaching.taunt.requires_level` | characterName;requiredLevel | 学习嘲讽需要 {characterName} 达到等级 {requiredLevel}。可使用已有经验卡，或返回已开放关卡获取经验。 | To learn Taunt, {characterName} must reach level {requiredLevel}. Use an XP card you own or earn XP in an unlocked level. |
| `fm.teaching.taunt.learn_button` | — | 学习嘲讽 | Learn Taunt |
| `fm.teaching.taunt.learn_confirm` | amount;characterName;itemName | 确认消耗 {itemName} ×{amount}，让 {characterName} 学习嘲讽？ | Spend {itemName} ×{amount} for {characterName} to learn Taunt? |
| `fm.teaching.taunt.learned` | characterName | {characterName} 已学会嘲讽。 | {characterName} has learned Taunt. |
| `fm.ch1.tutorial.taunt.explanation` | — | 熊王的远程攻击优先瞄准法师。战士完成连线时，若熊王仍存活且嘲讽已就绪，就会自动施放，使熊王在本次敌方阶段优先攻击仍存活且在其射程内的战士。嘲讽在该阶段结束后失效；蓄力或休整也会使其到期。 | The Bear King's ranged attacks prefer a living Mage in range. Connect its endpoints with the Warrior: if the boss is still alive and Taunt is ready, Taunt activates automatically. During that enemy phase, the boss prefers the Warrior if the Warrior is a legal living target. Taunt expires after that phase, including a charge or rest phase. |
| `fm.teaching.taunt.explanation_confirm` | — | 已了解用途 | I Understand |
| `fm.battle.intent.wind` | — | 上弦 | Wind Up |
| `fm.battle.intent.rest` | — | 休整 | Rest |
| `fm.battle.intent.ranged` | — | 远程攻击 | Ranged Attack |
| `fm.battle.intent.heavy` | — | 重砸 | Heavy Strike |
| `fm.battle.skill.cooldown` | remainingTurns;skillName | {skillName}：剩余冷却 {remainingTurns} 回合 | {skillName}: {remainingTurns} cooldown turns remaining |
| `fm.battle.skill.ready` | skillName | {skillName}：已就绪 | {skillName}: Ready |
| `fm.battle.enemy.vulnerable` | — | 易伤（下一次行动） | Vulnerable (Next Action) |
| `fm.battle.playback.taunt` | characterName;enemyName | {characterName} 嘲讽了 {enemyName}。 | {characterName} taunted {enemyName}. |
| `fm.battle.playback.wind` | enemyName;hpAfter;hpBefore;remainingUses;targetName | {enemyName} 尝试为 {targetName} 上弦：{hpBefore} → {hpAfter}，剩余 {remainingUses} 次。 | {enemyName} attempted to wind up {targetName}: {hpBefore} → {hpAfter}; {remainingUses} uses left. |
| `fm.battle.playback.enemy_rest` | enemyName | {enemyName} 休整。 | {enemyName} rested. |
| `fm.battle.playback.face_changed` | faceNumber | 进入第 {faceNumber} 面。 | Entered board {faceNumber}. |
| `fm.operation.use_experience_cards` | — | 使用经验卡 | Use XP Cards |
| `fm.growth.target_level.label` | — | 目标等级 | Target Level |
| `fm.growth.cards.cost_preview` | — | 将消耗 | Will Consume |
| `fm.growth.cards.change_preview` | afterLevel;afterXp;beforeLevel;beforeXp;characterName;fixedXp;targetLevel | {characterName}：目标等级 {targetLevel}；固定获得 {fixedXp} XP。实际成长：等级 {beforeLevel}（该等级内经验 {beforeXp} XP）→ 等级 {afterLevel}（该等级内经验 {afterXp} XP）。 | {characterName}: Target level {targetLevel}; fixed XP gain {fixedXp}. Actual growth: level {beforeLevel} ({beforeXp} XP within that level) → level {afterLevel} ({afterXp} XP within that level). |
| `fm.operation.begin_teaching_gift` | — | 领取首次入场赠物 | Claim First-Entry Gift |

用卡入口及Pending操作名使用`fm.operation.use_experience_cards`；目标等级标签对应`Draft.Quantity`，不表示卡数。成本标题使用`fm.growth.cards.cost_preview`，下面复用`fm.inventory.item.row`的`itemName;amount`参数，分别取`Quote.Costs[].ItemId`对应的本地化物品名及`Quantity`。命名参数按Ordinal顺序声明，Quote含义不变：`characterName`为本地化角色名，`targetLevel`取`Quote.Quantity`，`fixedXp`取`Quote.FixedExperience`，`beforeLevel`／`beforeXp`取`Quote.BeforeLevel`／`Quote.BeforeExperience`，`afterLevel`／`afterXp`取`Quote.FinalLevel`／`Quote.FinalExperience`；前后XP分别是各自等级内经验。审核标题复用`fm.inventory.change_preview.title`的`operationName`参数。`fm.operation.begin_teaching_gift`是领取操作及Pending的中性名称，不表示赠物已经到账；只有永久提交完成后才使用既有已领取文案。

名称分别映射稳定ItemId／SkillId／EnemyDefinitionId；普通H与Boss小工共用`enemy:mechanic`名称。已领取／已学只在相应永久提交完成后显示；已有合法学习也复用已学提示，不再次收费。用途确认保持独立按钮。上弦反馈读实际HP前后及剩余次数，零恢复不能伪装扣次；易伤标签到期条件必须来自保存状态。

已有地图关名`fm.map.level.card_title`、通用确认／取消／返回、经验卡／物品数量、首通奖励标题和物品行、结果物品行、敌方Strike/Charge及其播放模板、待补线／救援状态继续复用，不另造关卡名或把休整写成蓄力。CD为0用Ready，正数用cooldown；翻面反馈来自已提交事实，不触发额外敌方行动或奖励。

