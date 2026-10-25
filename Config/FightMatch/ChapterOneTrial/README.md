# 首章开发作者数据 · r2（1—9关）

2026-10-25。CH1-IMPL-001主策交付；actual turn `01a170ca-1104-75c3-833b-d2e2d4902352`，thread `01a0f2e3-2c47-7273-88da-20513bcbfaf4` / local。主程为接收者，准确18路径见[实施记录](../../../docs/implementation/2026-10-25-ch1/README.md)。

本目录的16张逻辑表和`SourceMetadata.csv`是唯一编辑输入，采用已接收的[表结构](../../../docs/system-design/2026-10-25-ch1-slice/table-schema.md)；不另交一份可独立修改的xlsx或运行JSON。当前Release修订2只包含正常1→9闭包：新档仅开放1，逐关整关首通开放下一关，8结算联合取得法师、9由玩家选入阵容。所有新增关卡、几何和数值均为Trial，尚未试玩校准。

## 内容及消费

- `Release.csv`：包`fightmatch:chapter-one-trial`，新`ContentBinding`；沿冻结能力标识，只有消费方确认实际支持本输入时才可发布。能力名称不代表未进入数据的嘲讽／Boss已实现。
- `CharacterGrowth.csv`、`Passive.csv`：战士沿原成长，法师沿冻结候选；有理数保留文本，概率不是当前机会概率，不手填PRD的C。
- `Level.csv`、`Face.csv`、`PairEnemy.csv`、`EnemyAction.csv`、`DefaultRouteCell.csv`：9关／9面／24敌实例／4条意图／96格默认路线。3关起重锤蓄力，6—8厚甲为18/19/20HP及100/200/300物防；8、9同款厚甲均20HP、300物防、5魔防。
- `Reward.csv`、`RewardMaterial.csv`、`FirstClearItem.csv`：奖励公式沿现行伤害／承伤贡献口径，不照抄旧完整成长表的最终XP。普通材料采用已授权开发候选锡片预算，每关2、4/8关3；4/8首通各发50XP卡1张。首通卡没有放进重复掉落，尚未展开新配方／广告。
- `Item.csv`、`Card.csv`、`CharacterUnlock.csv`：保留原锡片／木料ID，新增50XP卡，Card主身份就是`item_id`；8正常首通后按8的推荐等级3取得唯一法师，XP0、主动技能空，不改玩家阵容。
- `Skill.csv`、`Teaching.csv`：当前仅表头。1—9没有职业证、自动学技能或跳过后续关卡的入口。
- `SourceMetadata.csv`严格为`path,locator,sha256`，8项真实文件哈希，path相对仓库根。消费入口由主程的`ChapterContentTables.Read(directory, initialProfileAndProofs)`负责；原新档recipe和原31条精确PRD参数从下列旧源只读取得，不从玩家存档读取，不建立第二份参数作者表。

## L1保留与几何来源

[旧L1源](../../../Assets/FightMatchContent/demo-r1.source.json)SHA256为`fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a`。本目录精确映射其Level／Face／Pair／敌属性／路线／奖励及战士成长；`level:ch01-01`、Face `F1`、Pair/Enemy `A/B`保持原身份。新章包中的副本不等于原已验绑定，旧源、旧发布集合和旧档活动局均不修改。

作者坐标为左上1起(row,column)。旧源是`AssumedBottomLeft`：旧路线(x,y)变为`(height-y+1,x)`，旧FlowPos端点(x,y)变为`(height-y,x+1)`；消费端再按新表约定转换一次。禁止照搬旧路线数字后误标左上坐标。

除L1及下述L2外，3—9直接复用[17面旧试排](../../../docs/game-design/balance/results/boards.json)对应stage的路径，不重新生成关卡。L4/5保留试排4×4，与早期逐关稿的5×5“建议”区分，仍是未校准开发候选。9与冻结具体例图一致。

L2沿逐关稿“错误固化可堵住下一对出口”的目标，采用现有L9例的4×4双配对裁剪：A=(1,1)到(3,2)，B=(1,4)到(4,4)。两条默认线不相交；A另可合法走`(1,1)→(2,1)→(2,2)→(2,3)→(2,4)→(3,4)→(3,3)→(3,2)`，固化后B两端不可连通，需正常撤销。旧L2试排的B仅中间一格可通过，未能提供这个错误走法示例；本处是明确的Trial局部补缺，不声称旧布局已满足该教学目标。

## 实际检查及后续边界

2026-10-25 r2机械兼容修正（actual turn `01a170dd-48fa-7011-8842-c01a68b4c652`）：按主程指示，仅将`Release.csv`的`release_set_id`从`release:chapter-one-trial`改为`release-set:chapter-one-trial`，`revision`由1递增2，并更新本说明。真实资源入口[前缀检查](../../../Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs)第289行要求`release-set:`；这是资源ID格式对齐，仍为1—9闭包／Trial，没有新增10—16内容。其余16张CSV字节保持；r1冻结副本及原C生成证据保留，由主程另存r2并安排因输入变化所需的标准重新生成。

已用标准CSV／有理数读取做本次数据静态核对：17表列数、前置与奖励／物品／职业外键、8项源哈希、9面默认线的端点／边界／正交／互不相交、初游标及阵位连续；L1坐标逆变换、敌属性及奖励与旧源相同。L2上述错误线合法且堵住B的静态可达性已核。未跑旧校准模型、Luban、Unity或设备；正式编译／战斗／保存验证由主程交C执行。

17张CSV的文件名排序、每行`文件名:sha256\n`合并后，当前r2 SHA256：`5faccf6b422ac9348a46a19d7783edb369949dc86b304e71bccad9b83ad29f19`。原r1集合SHA256仍为`78f14252d2a895deb4abe0921012ffde8c49620c513016491c7ce2c265aaa0cb`，见[冻结副本](../../../TestArtifacts/FightMatch/CH1-IMPL-001/b01-b02/inputs/ChapterOneTrial-r1/)。接收后先固定本修订输入；向同目录追加10—16须与主程对齐新修订，不能覆盖消费中的输入。

后续16进入闭包沿现成10—15试排、普通E05的`LastLegalLiving`和H的精确1/10上弦，再接16两面；不靠改档或伪首通。16教学需新增本地化键`ch1.tutorial.taunt.explanation`，由后续准确白名单接入现有英简中表；不得用Prefab成品文字补缺。本轮没有改旧本地化文件或提前发布含未支持能力的包。
