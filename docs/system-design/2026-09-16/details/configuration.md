# SD01 · 配置与内容发布详细设计

2026-09-19 · r3 · M10 · **SD01-C3输入状态窄修已由SD00独立审阅接收；r1／r2已接收范围保留。** 继承D01～D05及原版本交接；r2接收记录见9月17日integration-review第9节。本次仅同步N2-H裁定与迁移、N2-I1试调及固定入场评分已交接状态，不改配置结构／版本／API，不构成正式内容发布、格式迁移或Unity实施授权，不新增资格过期条件。

共同依据：[系统目录r3](../system-catalog.md)、[交互契约r6](../interaction-contracts.md)、[H11与公共版本资料](../runtime-extension-contracts.md)、[流程r5](../cross-system-flows.md)、[计划r17](../session-plan.md)及[SD01-R2任务包](../../2026-09-17/system-task-packets.md#2-sd01-r2--当前运行集合与配置解析)。公共定义由SD00统一维护；本文不另建同义版本身份。

## 1. 目标、已核对事实与验收范围

目标是让成长、战斗、结算、默认攻略及独立验证读取同一套不可变定义，同时把草稿、候选数值、策划算例和已发布内容分清。M10 拥有发布内容与版本索引；M11 拥有草稿及制作操作；M03 算角色成长；M06 算战斗；M07 算结算。M10 不保存玩家 HP、经验、库存、CD 或随机进度。

本设计验收要求：编辑源唯一；必要字段、引用及错误可定位；属性不双算；两面默认见证完整；旧尝试不热换配置；候选不能冒充正式包；G01及奖励取整有明确归属。验证方式是源文件实读、契约交叉检查与第9节样例推演，不执行模型或游戏测试。

| 实际只读核对的材料 | 当前事实及其限制 |
| --- | --- |
| [主策划](../../../game-design/2026-09-14-fightmatch-game-design.md)、[三组规则](../../../game-design/2026-09-15-three-group-recommendations.md)、[协调 F／G](../../../game-design/2026-09-15-content-planning-sync-reply.md)、[最新架构接收](../../../architecture/2026-09-15-content-requirements-sync-reply.md) | 首入16赠证与首通分离、法师8后入队9参战、整关两面、技能各自条件和G01为现行依据；旧14／15方案及旧攻略收费规则已失效。 |
| [配表说明](../../../game-design/balance/README.md)、[config.json](../../../game-design/balance/config.json)、[N2-I1输入](../../../game-design/balance/2026-09-19-calibration-inputs.md) | 配置仍为 `2026-09-15-entry16-tutorial-v4`，校准修订为 `2026-09-18-exact-winding`；N2-I1已交8行首章逐敌试调表及80组固定入场评分对照，未导入配置／主配表或发布；两组火球仍待整场比较与联合审定。 |
| [chapter_model.py](../../../game-design/balance/chapter_model.py)、[boss_model.py](../../../game-design/balance/boss_model.py) | 前者内含 `LEVELS / RECOMMENDED / enemy()`、奖励节点及公式，后者用 `Fraction` 做指定Boss计划；二者都不是共享游戏规则，前者也未完整输出角色法防成长。 |
| [calibrate.py](../../../game-design/balance/calibrate.py)、[build_workbook.mjs](../../../game-design/balance/build_workbook.mjs) | 棋盘试排、锡木拆分仍在Python；飞斧／投锤／毒箭配方、部分技能范围和资源广告报价又写在工作簿生成脚本。现状不是单一完整定义源。 |
| [结果清单](../../../game-design/balance/results/manifest.json)、[summary.json](../../../game-design/balance/results/summary.json)及CSV表头／样行 | 已核对15个结果文件：stages、enemies、progression、normal_actions、boss_actions、boss_scenarios、boss_sweep、ad_rounding、ad_progression_samples、prd、checks、boards、summary、manifest、workbook_data。manifest记录4份源的SHA-256及种子915；未覆盖工作簿脚本硬编码，不能作为运行包指纹。 |
| 结果证据范围 | summary记录421项策划检查、80行成长、7类Boss分支、54组对照、200条广告样本，且 `tutorial_persistence_verified=false`。本次只读未重跑；回放CSV是指定输入下的结果，不能作为角色定义、玩家存档或正式关卡证明。 |
| [FlowLevelData](../../../../Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs)、[FlowPairData](../../../../Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs)、[FlowLevelAsset](../../../../Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs) | 已有尺寸、几何端点／colorId、完整解、难度、生成种子和覆盖率；没有职业、属性、技能、敌人档位、RPG奖励或多面绑定。 |
| [JSON导出](../../../../Assets/Scripts/FlowPuzzle/Persistence/FlowLevelJsonExporter.cs)、[几何验证](../../../../Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs)、[编辑窗口](../../../../Assets/Scripts/FlowPuzzle/Editor/FlowLevelGeneratorWindow.cs)、[DraftMapper](../../../../Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs)、[资产仓储](../../../../Assets/Scripts/FlowPuzzle/Editor/Persistence/FlowLevelAssetRepository.cs) | 已有几何存取、草稿失效与完整路径校验；导出是 `level_{id}.json / solution_{id}.json`，不能当原子RPG发布。`FlowSolveRequest / FlowCompletionRequest` 只接几何与预算；不具有完整战斗验证能力。 |

下述编译、类型、发布索引、内容指纹和RPG见证均为**计划新增能力**。现有通用FlowPuzzle继续独立，不加职业依赖，不引入满盘或唯一解要求；r1只新建本文件，r2／r3只修改本文件。上表保留r1来源证据；r3复读H～K、N2-I1及上弦迁移脚本／manifest／summary，未运行模型，作者既有执行记录不冒称本次重跑。

## 2. 唯一编辑源与单向发布链

**推荐受版本控制的JSON草稿集为唯一编辑源。** 按用户主要由AI维护的工作方式，将定义按类别分文件、由一个源清单组织；各记录只出现一次。普通关敌人最终档位、奖励及配方显式登记，不再从Python分支或工作簿文字反推。未来M11可用UI Toolkit编辑同一草稿；不是再建一份可独立改值的Unity资产。

目标链：`JSON草稿快照 → 规范化编译／引用检查 → M13几何验证＋M06独立RPG见证 → 联合审定 → 不可变发布包 → 全部消费者`。编译器为普通C#制作能力，可供Editor及独立验证入口复用；运行时只装载已编译包，不解释Python、Excel公式或任意表达式。Python以后只作导出分析消费者，工作簿／CSV只作生成的审阅视图。

这是目标设计，不是本轮搬迁授权。后续迁移任务须逐项从JSON、Python和工作簿脚本摘录到唯一源，保留原始来源及差异报告，先比较旧算例再切换；未经审定的候选保持候选。不能因为旧表格里已有数字便省略字段审定。

| 备选 | 可取之处 | 本轮不推荐的成本 |
| --- | --- | --- |
| Excel唯一源→导入 | 策划直接按表编辑 | 必须建设公式求值、类型／空白／引用诊断、稳定导出和差异检查；战斗回放仍不会随单元格修改自动重跑。 |
| ScriptableObject唯一源→导出 | 接入现有Unity编辑体验 | 规则验证与在线消费还需无Unity导出；资产／meta审阅及多人改值较重，也不能复用现有几何资产冒充RPG定义。 |
| 保留JSON＋Python硬编码＋工作簿多源 | 初次整理最少 | 同一概率、敌人或奖励难以追溯唯一来源，修改易漂移；不满足D05，不能作为正式发布链。 |

目标文件类别如下；名称表达职责，具体目录／扩展名清单留给另行批准的实施任务，不在本轮创建。

| 类别 | 内容及可写者 | 是否被运行时读取 |
| --- | --- | --- |
| 编辑源 | 源清单、职业／成长／技能／状态、敌人／档位、物品／配方、关卡各面、奖励／教学事件、默认步骤与条件；M11维护 | 否；可含候选和制作注记。 |
| 编译候选 | 完整引用闭包、规范化坐标与数值、版本描述、候选身份；编译器生成 | 仅隔离验证，不能普通入场。 |
| 验证证据 | 草稿修订、候选内容指纹、几何结论、完整RPG步骤／结果、适用条件、G01等回归记录；M11组织 | 参考与诊断可读，不是玩家通关事实。 |
| 发布包 | 清单＋定义负载＋默认参考及证据；M10受控接收，发布后不可修改 | 是；全部消费者读取同一闭包。 |
| 派生审阅资料 | CSV、工作簿、平衡报告、时间预算与来源映射 | 否；携源指纹，不能回流覆盖源。 |

几何制作可继续使用FlowPuzzle资产作为工作材料；M11显式导入指定几何版本至JSON草稿并建立来源记录。RPG发布从已固定的JSON几何副本编译，原资产后来修改不自动更新它，避免双源；更新必须显式重新导入并失效见证。

## 3. ID、引用与最小共享记录

定义身份采用类别化ID，例如 `class:warrior / class:archer / skill:warrior-taunt / enemy:clockwork-worker / tier:worker-l16 / item:proof-general / recipe:throwing-axe / level:ch01-16`。后缀为小写ASCII字母、数字、连字符，非空、大小写敏感；显示名及W/M/K/A只是标签或导入别名，不作跨类别查询键。ID改名／重用视为兼容变更，不能把旧ID赋给另一含义。

`FaceId=(LevelId, faceKey)`，`PairId=(FaceId, pairKey)`，敌人配置实例键为 `(FaceId, instanceKey)`；局内 `CombatantId` 仍由M06包含AttemptId产生。敌种、档位、面内实例、几何配对各有不同身份：同一种敌人可多次出现，同一面每个PairId恰映射一个敌人实例，双方集合一一覆盖。几何整数colorId只用于M13 Adapter，包内保存显式映射，不用颜色或数组下标猜敌人。

编辑记录头为 `kind, id, recordVersion, valueStatus`；`recordVersion` 为正整数，`valueStatus=Candidate / Reviewed` 只用于制作审定，不进入运行负载或规则内容指纹。源注记另带来源和审定依据；审定切换不改变数值见证所指内容。包内记录保留前三项及定义，跨表引用必须含预期类别；禁止以空引用代表“自动挑一个”。除下列明确可空列表／联合分支，缺字段一律报诊断，不默认0、不默补继承值。

| 记录 | 最小字段与单位／范围／必填关系 |
| --- | --- |
| `ClassDefinition` | `baseStats{maxHp,attack,physicalDefense,magicDefense,evasion}`、`attackKind`、`attackRange`、`growthId`、`passiveSkillId`、`activeSkillId`、`acquisitionEventId`。maxHp>0，攻击／双防≥0，闪避∈[0,1]；射程是存活序号正整数，不是格距。每职业一名角色由M03保证。 |
| `GrowthDefinition` | 已命名成长规则及其参数：HP／攻击的每级比例、双防每级增加值、升级经验常数／一次／二次项、被动概率成长引用。等级从1起，需求经验必须为正整数，生成的基础属性在支持等级范围合法；不引入任意公式脚本或未定等级上限。 |
| `SkillDefinition` | `ownerClassId, activation=Passive/AutoActive, behaviorId, targetingId, range, effectParameters`；主动必有 `learnLevel, costItemId, costQuantity, cooldownRounds`，被动必有 `triggerId, probabilityModelId`。CD为全队有效回合整数≥0；学习等级≥1、消耗>0；无限射程用明确枚举，不能以0猜含义。 |
| `ProbabilityDefinition` | `modelId, baseProbability, perLevelIncrease, capProbability`；均为比例，0≤base≤cap≤1，增长≥0，按绑定规则计算目标率。PRD常数若编译产生，标为派生、携算法版本，不允许另手填一份。没有被动不能因概率字段缺省获得暴击。 |
| `StatusDefinition` | `behaviorId, durationUnit, durationCount, magnitudeParameters, refreshPolicy`；DOT次数为正整数，阶段状态按敌方阶段计，不能混作秒或CD。毒／烧伤独立；同类刷新、保留较强值和免疫作用点由规则实现，不能把任意字符串表达式塞入配置。 |
| `EnemyDefinition / EnemyTier` | 敌种记录 `behaviorId, damageKind, range, targetingId, elementResponses, statusImmunities`；档位记录 `enemyId, finalStats, intentCycle, abilityParameters`。HP>0、攻击／双防≥0、范围合法、意图非空且均受RuleVersion支持；恢复次数为整数≥0，恢复比例∈[0,1]，频率正整数。档位属性是最终值，不在战斗再次按推荐等级缩放。 |
| `LevelDefinition / FaceDefinition` | 整关 `recommendedLevel≥1, orderedFaceIds, rewardPolicyId, tutorialEventIds, defaultGuideId`；每面 `width,height>0, pairs, enemies`。面非空且唯一；敌人条目含 `tierId, originalSlot, pairId, linkedTargetId?`。原槽位为本面从0起不重复整数；上弦类能力必须指向同面合法目标，普通无绑定能力才可无此字段。 |
| `ItemDefinition / RecipeDefinition` | 物品 `category, stackLimit=99, allowedClassIds, useBehaviorId?`；战术物品须有职业、触发、攻击／状态参数；材料可无使用效果；经验卡经验为正整数。配方 `inputs[{itemId,quantity>0}], output{itemId,quantity>0}, unlockEventId`，不得同一材料重复列项或无产出。批量／库存属M04。 |
| `RewardPolicy` | `baseExperience≥0`整数、`scorePolicyId, levelCorrectionId, repeatDrops, firstClearDrops, unlockEvents, bonusPolicyId`；掉落为物品ID＋非负整数数量。无掉落写空列表，不能缺字段。当前固定材料直接列各物品数量；未来随机掉落须完整分布及受支持算法，不先造稀有掉落。 |
| `BonusPolicy / ResourceAdOffer` | 前者为倍率与权重列表、参与物品类别；倍率≥1、权重≥0且至少一个>0，编译规范化为总和1。后者为进度档、明确物品数量、开放事件及期间限额>0；局外资源报价不套结算轮盘，限额不是复活次数。 |
| `ProgressEvent / TutorialDefinition` | `trigger, levelId, onceKeyKind, grants, requiredSteps`；事件效果通过M05/M02交给各所有者提交，不由M10执行。首入赠证与首通、职业获得、配方开放均显式分类；教学步骤须有稳定ID及前置，不以显示顺序替代依赖。 |
| `DefaultGuide` | `levelId, assumptions, orderedSteps`；条件含阵容原槽、等级／已学技能、入场HP／携带、操作条件、完整随机初态和许可资源。步骤含面、操作种类、角色／目标／PairId／路径及当步条件，覆盖全部后续面与补线；只允许显式适用前提，不能把“关闭所有被动”的算例写成普通玩家可选规则。 |

这是首版所需记录关系，不是可脚本化效果系统。`behaviorId / triggerId / targetingId` 来自绑定规则实现的有限目录，例如战士真实直接命中暴击、普通法术命中穿透、物理直接受击格挡、普攻至多追加一箭；未知值返回 `UnsupportedBinding`，不能靠名称或反射悄悄执行。

所有比例和属性常数在源中用十进制文本或`分子/分母`，编译为约分后的精确常数；整数计数不接受小数，拒绝NaN、Infinity、零分母、负数量及无法被目标计算契约表示的值。数值上界由已命名计算契约的可表示范围校验，不把机器范围变成玩法等级封顶。

诊断至少给 `category, reasonCode, sourceFile, recordId, fieldPath, expected, actual`；引用失败再给目标类别／ID，冲突ID列两处源位置。重复JSON键、未知规则字段、错误单位及必填字段缺失均阻止编译；不以拼写近似自动修正。可选说明文字缺失不影响规则，其他默认值必须已经在对应schema明示。

### 已有数值与事件怎样映射

现有四职业1级 `(HP,攻击,物防,法防,射程)` 为W `(100,20,10,6,1)`、M `(72,22,4,12,2)`、K `(125,16,16,8,1)`、A `(80,19,5,5,3)`；均作为来源明确的试调参数迁入候选。主动与被动分表，不继续沿用role下一个模糊`cd`字段。

主动技能分别保留嘲讽、其他活队友群盾、固定邻位火球、当前活前排束缚的条件；候选CD按W/M/K/A为2/2/3/3。被动目标率W/M起点0.20、K/A起点0.25，每级+0.005，上限分别0.35/0.40来自当前config，不新增基础闪避概率。火球 `1.0/0.6` 与 `0.8/0.4` 必须整组分别建候选，禁止拼接或按文件较新自动选择。

技能学习为Lv.5＋通用职业之证＋玩家确认；证属于物品，不将“职业对应技能”误写成新的职业专用证。法师事件必须是 `AfterWholeLevelClear(level:ch01-08)`，按该关推荐等级Lv.3入队，第9关可参战，不追补第8关经验。21／37节点保留在完整编辑源；只发布首章时未提供后续关卡者须从运行闭包排除相应不可用内容，不能留下假装已可用的悬空引用。

`Level16EntryGift` 事件在首次进入16教学时发1枚；`FirstWholeLevelClear(level:ch01-16)` 发另1枚，是不同业务键。教学显式依赖已学嘲讽及用途说明，未达标只拦16开战；返回地图／重打旧关由M05/M02执行。M10提供条件及奖励定义，不保存领取标记，不把成长算例的`confirmed_learning=true`当玩家确认。

## 4. 属性、数值与随机契约

角色采用当前**基础值＋成长参数**的单一表达，M03在H02按等级求一次 `ComputedBaseStats`，携CharacterId、ClassId、Level、DefinitionBinding及EntryBaselineId交给M06。M06只接受已算基础值并施加战内变化；不再接受“基础值或已算值任选”的无标记字典。UI读取结果；M09/M11创建参考入场时也调用同一成长计算能力，不抄公式。

例如当前W4最大HP为`100×(1+0.08×3)=124`、攻击为23.6；战斗不能再次乘成长而变成153.76HP。敌人则读取EnemyTier最终值创建实例：推荐等级仅是内容参考／经验修正输入，不触发隐式二次成长。导出的逐级属性比较表只是派生审阅证据，不能与成长参数同时作为可编辑权威值。

| 契约职责 | 所有者与必须输出的内容 |
| --- | --- |
| 定义常数、字段单位和取值范围 | M10负责精确规范化、验证及版本引用；不是伤害／经验执行者。HP、盾、伤害为生命点，攻击／双防为属性值，射程为存活序号，概率为[0,1]比例，CD为全队有效回合，持续状态按显式单位，库存／经验为整数，现实恢复与预算为秒。 |
| `NumericContractVersion` | SD06牵头，与SD03/SD08及策划统一计算表示、溢出行为、除法／log₂误差与舍入时点；包只声明其支持版本。伤害所有修正后floor、有效贡献封顶于真实扣血、HP中间值保留精度，显示两位不反写。不能各消费者采用不同epsilon。 |
| 成长与评分参数 | M10供值；M03应用成长／卡经验，M07应用奖励与评分。现有HP+8%、攻击+6%、防御每级+1.5、升级需求60/20/5是候选参数；评分`score_floor=0.5`是曲线起点参数，**不是floor取整开关**。完整保护评分新权重未覆盖首章旧回放，不能混为同一计算版本。 |
| 奖励floor／ceil | 固定语义归数值／结算契约：普通经验先floor基础×等级修正×评分；广告通关取基础经验；增益经验floor原经验×倍率。卡／普通材料合并同类后ceil原数量×倍率，只发与已发基础的差额；证／解锁不乘。M10不提供可任意改变这些语义的每关`roundingMode`。 |
| `RandomContractVersion` | SD06定义算法、状态编码、机会触发顺序及PRD积累；M10记录目标概率及版本。输入保存完整流状态／位置和每角色每被动积累，不只seed。死亡／复活／翻面保留，回退恢复，新普通入场初始化，重来恢复入场初态；R01/R02另由SD00处理。 |
| 奖励随机 | SD08定义掉落／倍率抽取，使用与战斗分离的流及已持久输入；AttemptId／SettlementId和原包绑定。倍率权重来自包，不能因保存失败或新OperationId重抽；M10既不抽取也不保管玩家流状态。 |

**精度证据与必须新增的边界样例：** `chapter_model.py:floor()` 使用`floor(x+1e-9)`，其stats先round到4位；`boss_model.py:damage/player_stats`用Fraction，而`calibrate.py`轮盘期望用`ceil(count*m-1e-9)`。这些是不同策划实现，不能把某一实现直接升格为游戏规则。

计算契约验收必须含：精确值`13.9999999995`在普通floor为13、加epsilon为14；精确值`2.0000000005`普通ceil为3、减epsilon为2；同一W4基础属性跨客户端／验证一致；G01的log₂结果距离整数边界足够且最终为14。前两项是合成边界输入，说明差异，不声称当前关卡已命中该边界。

上弦已由N1裁定并经N2-H迁移：活的绑定目标恢复精确`maxHp×1/10`，按缺血封顶，仅正恢复耗一次；满血／死亡空过意图并保留次数，耗尽后沿原行为表。普通模型已复用Boss函数；[verify_winding.py](../../../game-design/balance/verify_winding.py)覆盖9边界、两小工、第12关合法四行动及原80／263行对照，作者记录6方法通过。最大HP19、缺2得1.9是构造边界，旧1仅作历史对照，不再待策划裁决；运行时Numeric／Random及正式发布门槛仍由SD06等落实，不能把离线迁移等同发布。

## 5. 小而可调用的Interface

M10是一个Module，其Interface覆盖读取与受控发布；不按每个表建一层仅转发的Module。编译检查可在内部组合，消费者只接触不可变视图。方法名是语义提案，不代表现有C#代码。

| 操作 | 输入 → 输出；顺序、失败及性能语义 |
| --- | --- |
| `Prepare(sourceSnapshot, expectedDraftRevision, ruleCapabilities)` | M11传稳定草稿快照及目标验证环境的规则能力；返回 `PreparedContent{candidateKey, contentFingerprint, immutableDefinitions, diagnostics}` 或分类失败。只解析／规范化／检查闭包，不发布、不算玩家状态、不运行真实战斗。可为V2准备候选，不据此改变当前R1能力；按字节／记录／引用线性扫描为目标，允许取消，不把长验证塞入运行时读取。 |
| `Publish(preparedContent, validationEvidence, reviewEvidence, currentDraftToken, operationId)` | 仅制作流程调用；M11在草稿发布串行区间内交当前修订令牌。M10核对令牌、指纹、规则能力、完整见证及内容审定，返回发布清单、ContentBinding及各关DefinitionBinding。内容与证据可共同取得才报告此发布的 `Completed`；不开放半包、不启用ReleaseSet，也不代表玩家档已提交。 |
| `ResolveExact(binding, consumerCapabilities)` | 接受包级ContentBinding或整关DefinitionBinding，按完整身份装载并核验指纹／版本及实际消费环境能力，返回只读 `DefinitionView`；整关另查LevelId／LevelVersion。包缺失或Rule／Numeric／Random不支持为 `UnsupportedBinding`，损坏为 `RecoveryBlocked`。不因该绑定已非当前而换值，不联网补数据；恢复组织者停止受影响流程。 |
| `GetCurrentBinding(scope, expectedReleaseSetId)` | scope为`PackageScope`或`LevelScope(levelId)`；核对M14权威的当前已启用ReleaseSetId与ReleaseCapabilities，只选其中实际支持、已取得并经M10解析通过的Published内容。集合已变返回 `StaleContext`；未声明该关为 `Rejected(LevelUnavailable)`，所需包缺失为 `UnsupportedBinding(MissingPackage)`。分别返回ContentBinding或DefinitionBinding及所据ReleaseSetId；不按全局最新发布项取值。预览／确认仍核对绑定与业务修订，不悄悄改成本或伪造当前关。 |

`DefinitionView`按类别化ID查记录、给整关全部面／默认参考／计算描述；查不到引用为 `Rejected(DefinitionMissing)`。已发布闭包出现内部缺引用则属于损坏，应阻止使用，不返回空对象让调用方猜。视图内部建只读索引，一次装载核验后按ID查询不逐次解析全包；缓存按指纹隔离，不返回可修改数组。

必要类型关系为：`SourceSnapshot → PreparedContent → ReleaseManifest + DefinitionView`；`ValidationEvidence → candidateKey + contentFingerprint + guide/assumptions/steps`；`ContentBinding → ReleaseManifest`；`DefinitionBinding → ContentBinding + LevelDefinition`。二者是显式类型，不能用空LevelId表示某个真实关。编译候选身份与正式binding分开：独立验证可用CandidateContext，真实入场只收已发布DefinitionBinding，避免验证前必须先伪造发布的循环。

公共错误分类沿用共同契约：结构／范围／缺字段是`Rejected(reason)`；过时草稿为`StaleContext`；规则不支持为`UnsupportedBinding`；损坏为`RecoveryBlocked`；发布已知未保存为`SaveFailed`、结果未知为`CommitUnknown`。几何报告保留`GeometryValid`，RPG结果用`RpgValidated`或`InvalidCandidate / Timeout / BudgetExceeded / Cancelled`，不得将候选失败改写成`ProvenNoSolution`。reasonCode是定位细节，不增加一套相反的全局完成状态。

成功例：草稿r8经完整两面见证、内容联合审定，Publish确认包与证据可恢复后返回绑定V1；ResolveExact为客户端／攻略／内容验证给出同一指纹。失败例：同包配方引用缺失材料，Prepare给出配方ID和`inputs[0].itemId`，整份候选不进入发布；不会拿工作簿同名材料补齐。

## 6. 发布状态、指纹与坐标

制作生命周期：`Draft → PreparedCandidate → ValidatedCandidate → ReviewedCandidate → Published`。校验失败留在对应制作阶段并带诊断；任何相关定义、几何、条件、步骤或规则版本变化都形成新修订并使旧见证失效。即使内容改回相同字节，旧异步结果仍不能按旧修订直接接纳，必须重新关联／核验。首版保守地使整候选见证失效，不先建设复杂增量失效图。

审定必须指向确切候选内容与证据，不能只是“火球大致可用”的文字标记。允许两组候选分别完整验证并并列审阅；两组都通过也不自动选一个。`Reviewed`意味着策划／配表对该版本给予可发布依据，不表示平衡已最终完成。发布后只创建新包，不能修改旧包同名记录。

清单最小内容：`packageId, packageSchemaVersion, compilerVersion, sourceFingerprint, contentFingerprint, ruleVersion, numericContractVersion, randomContractVersion, coordinateConvention, includedLevelVersions, payloadFileHashes, evidenceHashes, reviewReference`。源指纹用于追溯，内容指纹用于规则一致；编译器版本是生产证据，不替代规则版本。包schema版本独立于SD02存档Envelope／业务切片版本。

推荐内容指纹为规范化定义负载的SHA-256：UTF-8、固定字段名顺序、无无意义空白、ID记录排序、精确常数统一分数表示。**面序、原槽、意图循环、配对映射和步骤顺序不可排序丢失语义。** 默认攻略条件／步骤及所有规则相关字段纳入；制作注释／展示预算可排除但在文件清单各自校验。证据引用contentFingerprint，清单引用证据hash，避免内容把自己的hash或证据hash算进去产生循环。

包身份不可变；**包级ContentBinding**为 `PackageId, ContentFingerprint, RuleVersion, NumericContractVersion, RandomContractVersion`，用于局外学习、合成、经验卡、装配、资源广告报价及恢复规则等不隶属某关的操作。**整关DefinitionBinding**为`ContentBinding + LevelId + LevelVersion`，用于入场基线、战斗／历史、整关结算及攻略；教学的关卡规则也按该关绑定。前者不能冒充后者，局外操作不能伪造“当前关”。二者共享同一包与规则身份，非两套计算依据。

LevelVersion为正整数，整关自身或其规则依赖闭包改变时，编译与前版比较并要求递增；同一包所有面只属于该整关版本，不能各面自行选包。相同内容、同审定请求重复发布可返回原绑定；相同OperationId不同意图返回`OperationConflict`，同PackageId不同内容拒绝。文件写了一半不能登记Published；查询发布提交按稳定身份区分未提交与已完成，物理落盘及发布索引实现另交后续任务验证。

坐标目标统一为**左下原点、x向右、y向上、零基四邻接**；现有[FlowBoardViewGeometry](../../../../Assets/Scripts/FlowPuzzle/Editor/UI/FlowBoardViewGeometry.cs)绘制时使用`height-1-cell.y`，与此一致。策划`boards.json`仅表明一基x/y，源未正式标注原点方向：迁入时必须补`sourceCoordinateConvention`，不能靠看起来对称猜方向。

已确认一基左下时转换`(x-1,y-1)`；一基左上时转换`(x-1,height-y)`。只在导入／编译一次；运行包与现有FlowPos均零基，不再减一；屏幕坐标仅在表现Adapter转换。见证同时绑定转换后的几何及原约定，路径和端点一并转换，保留反查源坐标的诊断映射。`width×height`、端点唯一、相邻、不穿其他端点与路线重叠检查仍由适当几何能力完成。

原阵位不从棋盘x/y导出。固定邻位火球和上弦目标引用按面内实例；射程由M06按存活顺序推导；颜色、几何ID、敌名及界面斜九宫格都不能替代这些字段。

## 7. 发布与存档兼容协作

尝试建立时固定整关全部面与完整binding；发布或下载V2不改变仍运行R1的当前绑定。只有H11启用支持V2的兼容集合、M10解析通过后，后续普通入场才可选V2。当前V1尝试、回退、重演、默认攻略及在线验证继续ResolveExact(V1)；独立服务不支持V1则返回`UnsupportedBinding`并保留攻略权益。仅数值相同／LevelId相同不代表兼容。

`RecoveryRequirements`、`ReleaseCapabilities`和`ReleaseSetId`只采用[扩展第2～3节](../runtime-extension-contracts.md#3-h11-小启动层内容取得与版本启用)的定义。M12交与准确CommitId同代的摘要及当前／备份／未决引用根；M14选择覆盖所需保存和执行能力的集合。固定层先比较元资料，不实例化M10的热更业务类型；匹配代码入口可运行后，M10核验原定义，M02组织完整业务恢复。M10不读取存档内部结构、选择代码集合或自行取得缺包。

| H01／H11交接 | 完成依据与不能越过的边界 |
| --- | --- |
| Prepare → 隔离验证 | 仅候选及诊断；不支持的计算版本拒绝，未审定火球与N2-I1试调不能凭Prepare成功变成Published；上弦已裁定迁移也不替代完整发布证据。 |
| Publish → M14准备集合 | 发布绑定及内容／证据可取得的依据；重复查询原发布结果，未知查原操作。M14另核对发布信任、平台和代码／AOT／资源组合，哈希不独自证明发布授权。 |
| M12取得 → M14准备可用 | 原ReleaseSet声明的必要依赖完整且校验通过；半包或缺包仍不可用，重试固定原集合。有效缓存断网可继续；同一已启用集合按需取得声明的关卡依赖，不要求逐关重启。 |
| M14启用 → 当前读取 | 启用前重查CommitId及引用根，变化则重新核对；未知／损坏摘要阻断。当前读取只用已启用集合，下载完成不能代替启用，定义解析成功也不代替业务恢复。 |
| 精确读取 → M02恢复 | 返回原绑定的DefinitionView及兼容结论；缺旧包／旧执行能力阻断受影响恢复，不退旧玩家档。M02完成各业务核验后才可游玩；备份只作保留根，不因此成为可写当前头。 |

M12按业务适用范围保存完整ContentBinding或DefinitionBinding，不把它缩成“当前配置版本”。EntryBaseline、活动尝试／历史、未完成整关结算／增益报价、在线攻略及已保存参考保留DefinitionBinding；局外学习／合成／用卡和资源广告请求保留ContentBinding。角色复活请求还保存原CharacterId和原局内尝试／局外恢复期身份；涉及局内规则时保留DefinitionBinding，局外恢复保留ContentBinding，不能用某个当前LevelId代替目标范围。已固定BaseReward仍保存原依据，追加额不得用V2重算旧评分；已有尝试不会因长期升级追改基础属性。

教学中协调学习／赠证时，可同时需要教学关卡DefinitionBinding和成本ContentBinding；组织者必须核对二者的包级部分一致，不能拿V1教学条件搭配V2证成本。未确认预览遇到版本变更应重新预览／确认；已持久请求或已固定提交候选按原依据重试，不把恢复当作新的报价。

首版推荐**不自动裁剪已发布包**，用保留旧包换取简单恢复。后续若有空间需求，M10先接收M12给出的引用根：当前存档、仍可恢复的物理备份代、CommitUnknown完整候选，以及业务保留的记录；再证明无人依赖才提出回收。M10不自行窥探存档结构或删除备份。

规则实现也要能执行原`Rule/Numeric/Random`版本；只有旧数值包不足以恢复。新客户端发布检查必须验证受支持旧包＋规则能力，不能只检查JSON解析成功。首版不设计自动把活动尝试迁到新规则；真正迁移另开任务，逐项证明属性、历史、资源、权益及操作记录一致，禁止由配置加载器临时补偿。

若最新完整存档需要V1而V1缺失：ResolveExact给`UnsupportedBinding(MissingPackage)`，加载组织者给`RecoveryBlocked`并保留原资料、冻结和历史；不退回较旧存档、不改V2、不清空玩家。补回**同指纹包与兼容规则**后重新加载同一代可继续。包存在但损坏同样阻止受影响恢复；M14安排原包取得，M12执行，M02组织恢复，不假定在线下载可用。

内容发布与玩家存档不是一笔事务：发布可被完整取得、目标运行集合已启用且所需依赖可用后，才允许新尝试保存引用；恢复时按原绑定核验。发布取消或失败不能改变旧包与活动尝试。SD02 r2原范围已对齐保留根及“不因缺包降级”语义；本轮摘要物理一致性由并行SD02-R3交回，双方结案由SD00核对，不把本修订当成SD02-R3或整体更新对接已通过。

## 8. G01及正式发布前置

G01是**版本化回归见证**，不是运行配置中的“差8经验就送14”分支。它依赖初始属性／成长、Lv.5需求165、50经验卡、第一关两名E01各15HP、合法实际棋盘、伤害与评分／等级修正、首入16赠证事件和教学返回路径。上述依赖变化必须使对应见证过时并重验。

该见证从合法前15关前缀W4＋157／165、M5＋92／220、卡3开始；确认三卡给法师得到M6＋22、卡0；首入16已领证但不可学习，返回地图仅W4重打1。既有算例为原始伤害23、有效各15，2行动、HP124→119→119；经验`floor(20×0.5625×(0.5+0.5×log₂3))=14`，W5＋6，再入继续确认学习／说明。旧关不重发首通物，首通16第二证仍独立。

M10只提供定义与该见证输入的绑定；M03、M05、M07、M02与M12分别验证升级、教学、结算、返回及持久化。默认攻略完整胜利不能替代G01；G01的一个可达见证也不证明所有资源状态可恢复。普通达标路线不新增强制回刷。

正式发布门槛：纳入范围的全部引用闭合；无未审定候选值；Rule/Numeric/Random能力一致；完整默认攻略通过实际共享规则及几何；第6～9关职业对照、16教学／两面及G01等受影响验收有对应证据；保存恢复能保留绑定。当前421项离线检查与试排17面不满足这些实现门槛。默认参考若使用固定随机初态须明示只证明该前提，不宣称任意随机都可通关；关键教学不能靠暴击／穿透／闪避好运成立。

## 9. 独立设计验收样例

以下均为**设计验证，未运行测试**。获授权实现后，从上述Interface输入验证，不直接写内部字典制造“通过”；每项同时检查原玩家状态、库存与权益没有模拟副作用。

| 编号／关联 | 给定输入与操作 | 预期结果／完成点／中断保持 |
| --- | --- | --- |
| C01 引用／ID | 两处定义`item:tin`，配方又引用不存在`item:wood`；另把`class:archer`填进面内敌人引用。 | Prepare返回`Rejected`及重复位置／缺引用／类别错误，不产可发布候选；修正文档后新修订重编译，原发布索引不动。 |
| C02 A12 双算 | 当前候选W4按M03生成HP124、攻击23.6；M06接收该入场资料。 | 保持124／23.6基础，不能再应用8%／6%成长；把baseStats冒充ComputedBaseStats或混用binding应拒绝，保存失败不产生已开战状态。 |
| C03 A11 几何≠RPG | 完整不相交路线通过，但给定队伍在清场前倒下或步骤要求未携带毒箭。 | `GeometryValid`保留、RPG=`InvalidCandidate`，给失败步／条件；Publish拒绝，不写玩家救援、库存或奖励。 |
| C04 A11 两面遗漏 | 16候选含两面，默认步骤在第一面清空／补线后结束。 | 不能`RpgValidated`；报告仍有第二面或AwaitRescue，拒绝发布。补齐全部后续面并验证后才有整关见证。 |
| C05 A08 V1/V2 | A1绑定V1，R1只支持V1；V2已发布且下载完，分别检查原A1恢复与无活动尝试时的新入场；再按H11启用兼容R2。 | R1下新入场仍V1，R2启用且解析成功后才可选V2；重复读取不切换集合。A1、旧报价与增益始终ResolveExact(V1)；切换中断不装半份集合，在线端只懂V2则UnsupportedBinding并保留资格。 |
| C06 A08/H10 缺旧包 | 最新完整保存代引用V1，V1文件缺失但旧保存代或V2可读取。 | 停在RecoveryBlocked，保留最新代、历史及冻结；不回滚业务。恢复同指纹V1和算法后续办原操作，不重发奖励。 |
| C07 候选／草稿 | 两组火球有局部见证但未联合审定，或把N2-I1试调／固定入场评分当正式发布证据；另有r8验证期间技能改为r9。 | 未审定者Rejected(UnreviewedCandidate)，过时者StaleContext；不创建Published。上弦裁定不免除其余发布前置；重试不能以下载／代码兼容代替内容审定，r9不能继承r8的验证绿灯。 |
| C08 发布中断／重复 | Publish在包写入中断，或索引完成后返回丢失；同ID重试，再用同ID改内容。 | 前者未完整前不可见；结果未知查原提交，不另造包；完成后返回同binding；改意图为OperationConflict。旧包始终可解析。 |
| C09 G01/A09 | 用第8节指定见证；首次领证后、旧关结算后和学习确认后各中断一次。 | M10每步给绑定原值；各业务恢复后不重赠证／首通卡／重复扣证，最终W5＋6可继续说明。正式棋盘、UI与持久化必须另验。 |
| C10 奖励归属 | B经验14、同类材料2、倍率2.2、首通证1；后续角色升级和包更新后兑现。 | SD08按原binding算总经验30／追加16、材料5／追加3、证追加0；M03不再套评分，M10不逐关改floor/ceil；CommitUnknown不能重抽倍率。 |
| C11 精度／坐标 | 输入第4节边界值；一基左下(1,1)及一基左上(1,1)，高度6。 | 精度按同版本固定预期；坐标分别(0,0)、(0,5)，端点与路径一致。已有零基输入不得再次减一，缺源约定先拒绝，不猜布局。 |
| C12 H03/H07 局外包绑定 | 地图无活动尝试，V1合成候选或资源广告请求已保存；期间发布／下载V2，另有尚未确认的V1预览。 | 原操作按ContentBinding及配方／报价身份重试，不生成LevelId；仅发布／下载不使预览过时，实际启用绑定或业务修订变化才StaleContext。教学／成本包级一致；缺V1阻止恢复，不以包级绑定绕过整关校验。 |
| C13 X01 按需取得／离线 | R1及已有关卡完整，更新查询断网；恢复网络后取得R1已声明但尚未下载的另一关依赖。 | 已有关卡与局外操作继续原绑定；缺失关卡保持不可用。取得并校验原依赖后可读取，不换规则、不逐关重启；下载中断只续原集合，不影响有效缓存。 |
| C14 X02 半包／能力不符 | R2依赖未齐、可信依据失败或不支持摘要要求的旧Rule／Numeric／Random；V1数据包仍在。 | M14不启用R2，M10不把可解析JSON当可执行；重试仍检查原集合及完整能力。旧可用集合保留；无可兼容集合则阻断恢复，不清档、不宣称业务Completed。 |
| C15 X03 准备过时／代码回退 | 准备R2时摘要为C10；期间提交C11。另一路R2已写C11后申请回退不能读C11的R1。 | 启用／回退／清理前重核当前CommitId与根，旧C10检查不授权切换；R1不兼容C11则拒绝回退，保留最新资料与必要旧包。中断后仍读最新有效摘要，不以备份为当前档，旧Use／奖励不重放。 |

## 10. 真实未决、实施前置与提交SD00的建议

| 事项与证据 | 负责者／影响范围 |
| --- | --- |
| N2-I1逐敌材质／元素／免疫试调与固定入场评分已交；两组火球、完整评分接入、材料拆分及部分模板仍待校准／审定 | SD06／SD08接收现有输入后推进规则与整场对照，不以完整火球回放阻塞前置设计；试调及刷新／单盾建议不是发布裁决，不改旧成长。工作簿后续配方与资源报价仍非完整已核准定义。 |
| 上弦精确恢复已裁定迁移；其余Python精度差异、运行时Numeric／Random及弓手闪避等完整参数仍待落实 | SD06与策划、SD03／SD08补对应字段与黄金样例，不重复等待上弦裁决。N2-I1未覆盖的字段仍阻止对应候选发布，不能默认0或“无限”；不把局部精确计算推广成全模型一致。 |
| 17面试排未作正式趣味性评审，boards没有明确原点方向；Flow只验证完整几何 | SD10明确源坐标、配对和正式棋盘；SD06/SD09提供共享规则验证。不得以试排通过宣传整章RPG已验。 |
| Rule/Numeric/Random版本及旧执行能力尚无实现，G01 UI／保存尚未实测 | 下游设计／另行实施验证；不阻塞本文件设计接收，阻止正式发布完成声明。R01～R03继续由SD00统一收口。 |

以下保留r1提交的契约建议及反例；ContentBinding／DefinitionBinding、CandidateContext、ComputedBaseStats和保留根现已由共同r6接收，r2继承，不再请求创建同义定义：

1. **包级与整关绑定具体化。** 旧契约只列DefinitionBinding语义，强制LevelId会使H03局外学习／合成及资源广告伪造当前关；建议接收第6节五字段ContentBinding及增加LevelId／LevelVersion的DefinitionBinding，packageSchemaVersion另立。成本是各请求与保存按实际范围区分，不能以包级绑定启动战斗；影响H01/H02/H03/H07/H09/H10、A08，提交SD00统一修订并由SD02核对。
2. **候选身份与发布身份分离。** H01需先验证后发布；建议CandidateContext携草稿修订／内容指纹，真实EntryData只允许Published绑定。成本是制作入口显式区分，防止候选火球流入玩家；影响SD06/SD09/SD10。
3. **已算属性的类别明确。** 保留M03唯一计算权，在EntryData明确ComputedBaseStats及其绑定，不支持无标记的原始／已算混用。成本为输入检查，避免A12的124变153.76；SD03/SD06共同核对。
4. **恢复根和规则能力。** 旧包保留需覆盖物理备份代及CommitUnknown候选，不只当前Attempt；无旧规则实现与无旧包同样停止受影响恢复，不选旧业务代。成本为保留与升级验证；SD02/SD11承接。

r1验证记录：已完成上述源文件、结果清单、公共契约与样例的只读核对；文档链接／范围检查随原交付记录。未运行Python模型、工作簿生成、Unity、存档故障注入、真实SDK、在线分析或设备测试；未改源码、测试、配置源、资产／meta、依赖、公共文档或Git。r1设计接收不替代本次复核。

## 11. SD01-R2交付范围与证据

起始原文SHA256为`CCC6134530F3CF554951D4CD98808C477EFAF4FDB705BFEB62B4CF81682694ED`。输入为目录r3、契约r6、扩展r2、流程r5、计划r17、9月17日任务包r1、SD01 r1及SD02 r2；热更新研究与C4 r2只用于核对原边界。唯一改动为本文；没有新文件、配置字段、数值裁决或新公共版本身份。

契约影响：原“发布V2即影响后续入场”在R1不支持V2时不可成立；本修订按既有H11收窄当前读取，精确恢复保持原绑定。代价是显式传入预期ReleaseSetId，并区分发布、取得、启用和业务恢复；影响M10／M14／M12／M02及A08、X01～X03，不改变修改权。摘要物理协议交SD02-R3，完整生命周期与真实平台组合交SD13；本包没有新增需SD00裁决的共同语义。

设计推演已逐项核对C05／C07／C12～C15的输入、返回、完成依据及重复／中断结果；均为文档推演，游戏测试NOT RUN。PowerShell只读脚本核对全文28个本地链接、2个锚点，错误／行尾空白／冲突标记／TODO类占位均0，退出码0；全文无代码围栏。相对内存保存的起始原文为新增36行、删除15行，LF行尾保留。`git diff --check -- 'docs/system-design/2026-09-16/details/configuration.md'`退出码0；文件为未跟踪，故另作上述全文检查，不以空diff当通过。Unity、模型重算、SDK／云端／广告、物理保存、Android/AOT、包体与故障注入均未运行，本任务没有实施授权。
