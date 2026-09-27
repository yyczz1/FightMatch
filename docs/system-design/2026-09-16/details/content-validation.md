# SD10 · 固定测试内容与制作验证

2026-09-20 · r1 · SD10-DEMO-01 · **待SD00独立审阅；仅设计与手工文档推演，算法／回放／Unity均NOT RUN。** 本文不是发布包、源码实施授权或自己的终审结论。
起始目标不存在（Test-Path=False）；唯一新建为本文。按[任务包§67](../../2026-09-17/system-task-packets.md#67-sd10-demo-01--固定测试内容与制作验证设计备包)及§1／§68执行。计划r66／审阅r48／任务包r50仅登记SG03派发，较派发口述r65／r47／r49递增；共同r18／扩展r15、配置r3、战斗r3、结算r3、搜索r4六份指定SHA全部吻合。
核验顺序：核输入与现有几何能力→列L1/L3字段及局部阻塞→定义同规则证据和发布门→手工反例→本文全文静态检查。未读取numeric-random-research.md、并行numeric-random.md或其他非白名单业务文件；派发附研究SHA不扩大只读权限。

## 1. 三种结论与最小制作链

| 层次／负责者 | 回答的问题／完成依据 | 不可外推 |
| --- | --- | --- |
| 单次路线有效／M06调用M13原语 | 当前Pair端点、界内四邻接、无自交／他对端点／固化线／禁占格冲突；角色／阶段／射程另由M06校验。 | 不要求其他Pair已画，不问未来能否完成；无满盘／唯一解要求。 |
| GeometryValid／M13 | 同一面所有Pair各有一条完整、不相交的合法路线；每面独立证据。 | 不证明任何攻击顺序、存活、随机或RPG胜利；局部路线通过不等于本层通过。 |
| RpgValidated／M06隔离全关回放 | 逐步同规则验证，从原EntryBaseline覆盖全部面和必需补线，最终为WonPendingSettlement且完整报告封闭。 | 条件分支结果必须附证据种类；当前面清空、几何完整解、模型说赢均不足，胜利也不等于M07奖励已提交。 |

实际读到[FlowSolutionValidator](../../../../Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs)会检查MissingPath，不要求满盘或唯一解；[FlowPathUtility](../../../../Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs)可复用四邻接。[FlowDraftMapper](../../../../Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs)的isValidated／[FlowLevelAsset](../../../../Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs)仅是几何资料，不能升格H01通过；不改其语义。
推荐沿[配置§5～6](configuration.md#5-小而可调用的interface)的单一JSON草稿→不可变候选链，用一个顺序验证作业记录，不另造验证框架、公开C#接口或共同ID。下列为M11语义操作：

| 步骤／输入 | 输出、完成门与失败 |
| --- | --- |
| CaptureDraft(draftId, expectedRevision) | 深复制草稿、来源SHA／字段定位及SourceCoordinateConvention，绑定不可复用的DraftRevision；记录作业身份／取消状态，不读玩家Model。 |
| M10.Prepare(snapshot, expectedRevision, ruleCapabilities) | PreparedContent含candidateKey、contentFingerprint、规范定义闭包及诊断；缺字段Rejected(fieldPath)，能力不足UnsupportedBinding，不生成Published。 |
| ValidateGeometry(prepared, faceRoutes) | 显式Pair→colorId映射后用M13核全部面；保存输入／输出和失败格。单步操作仍走独立路线入口，不能用MissingPath或未来NoSolution拒绝本次行动。 |
| ReplayWholeLevel(prepared, validationEntry, GuideStep[], conditions) | M03同一成长能力产生ComputedBaseStats；M06用显式CandidateContext建立隔离初态，复用BuildIsolationInput语义及EvaluateAction／EvaluateLink／ReplayRecorded。逐步接后态，失败停在首差异；不调用只收Published的生产CreateAttempt绕过门槛。 |
| CollectEvidence(job, geometry, replay, coverage) | 汇总§4，分GeometryOnly／ConditionalReplay／LegalFullReplay；缺输入、非法步、Timeout／BudgetExceeded／Cancelled各保留诊断，不能变ProvenNoSolution或完整获胜证据。 |
| Review(candidate, evidence) | 审定者、准确候选／证据指纹、范围、内容参数／坐标决定及依据齐全，才可成为ReviewedCandidate；文档见证不是运行通过，试调数字不是默认获准发布。 |
| M10.Publish(prepared, validationEvidence, reviewEvidence, currentDraftToken, operationId) | 在草稿发布串行区间再次核当前修订、候选、能力、完整证据／审定及取消闸门；内容与证据可共同取得才Completed并返回真实ContentBinding／DefinitionBinding及发布依据。 |

每步都检查同一作业仍有效；取消或草稿过时的迟到结果只保留诊断，不能进入发布。数据r8→r9→r10改回相同值，r8回调仍StaleContext，r10重新建立验证／审定关联。推荐首版重新完整验证，避免隐含复用。
已进入Publish的结果未知只能查原operationId，不能把取消传输写成Cancelled且声称未发布；完成后查询原绑定，不重发包。相同OperationId异意图拒绝；SaveFailed续同候选；CommitUnknown先查，半包不登记Published。M10发布不等于M14启用，更不等于玩家H10完成。

## 2. 固定资料逐字段账本

“可用”只指来源明确可供候选；“可推导”必须保留公式／映射及输入；“缺失”不补0。推荐两个独立固定测试资料L1／L3，各有条件分支和待补证的正式随机分支；均非从当前真实玩家抽取。来源短名及SHA见§8。

| 字段／用途 | 已可用来源值 | 可明确推导／实际缺失与处理 |
| --- | --- | --- |
| L1整关／面／尺寸／顺序 | stages第1行：stage=1、board=L01-1、推荐1；boards(stage=1,phase=1)：4×4，A/B。 | 映射候选level:ch01-01／faceKey=1；A原槽0、B原槽1来自LEVELS顺序，只有一面。LevelVersion、发布包身份缺失，不能以0.2.1／v4代替。 |
| L1两对端点与完整参考线 | 一基A=[(1,1),(1,2),(2,2)]，B=[(1,4),(2,4),(3,4)]；boards逐格原文。 | 每线首末格为该Pair端点；6/16占格不需补满。source仅写first-pass reflected template，方向仍缺，处理见§3。 |
| L3整关／面／尺寸／顺序 | stages第3行：stage=3、board=L03-1、推荐1；boards(stage=3,phase=1)：4×4，A/B。 | 映射候选level:ch01-03／faceKey=1；A原槽0、B原槽1，只有一面；不得把L3当Boss第三面。 |
| L3两对端点与完整参考线 | 一基A=[(1,4),(1,3),(2,3)]，B=[(1,1),(2,1),(3,1)]。 | 同样首末为端点、6/16格；calibrate.boards的y反射不证明原点向上或向下，未转换为已批准运行坐标。 |
| 配对／敌人／职业身份 | chapter.LEVELS：L1 A=E01、B=E01；L3 A=E02、B=E01；W为战士，敌A为面内实例。 | PairId=(FaceId,A/B)一一映射实例；M13候选colorId A=0/B=1只作显式技术映射。种类可命名enemy:clockwork-infantry／enemy:tin-hammer，须保存E01/E02别名来源，不靠显示名。 |
| 战士等级／入场基础属性 | config.roles[W]：Lv1 HP100、attack20、pdef10、mdef6、range1、unlock0；三组：基础闪避0、物理近战。 | 配置GrowthDefinition明确双防每级+1.5；HP=100×[1+0.08(L−1)]、攻=20×[1+0.06(L−1)]、双防=(10,6)+1.5(L−1)。法防不缺：W1=6，W4=10.5；M03算一次，不由M06二次成长。 |
| 固定参与资料／技能／资源 | 参考progression标准行L1 W1+0、L3 W1+53，均未学嘲讽；两关满HP入场。 | 推荐fixture显式W1单人原槽0，另外两槽空、恢复空、CD／盾／异常空、无主动、C/U空、物品开关明确关、许可空。L3经验53仅旧成长参考，固定fixture独立命名，不宣称新评分下必然自然到达。 |
| 被动及机会 | config W：p0=1/5、每级+1/200、cap7/20、倍率3/2；主稿／三组规定真实直接命中才判战士暴击。 | p(L)=min(7/20,1/5+(L−1)/200)为可推导候选。正式精确C／参数审定、完整三域初态、采样／PRD轨迹及执行能力证据尚缺；不拿目标p当每次q。 |
| E01档位与行为 | enemies L1 A/B及L3 B：hp=maxHp15、pdef0、mdef0、e10、winding_left0、link空；chapter.enemy及run_battle可追溯。 | E=8+2×推荐1=10；HP=floor(3/4×W1攻20)=15。每存活敌方阶段物理攻击当前活前排，伤害系数3/5，行动顺序A→B；推荐显式近战range1，单人fixture无目标歧义，多人正式敌人表仍须审定这一映射。 |
| E02档位与行为 | enemies L3 A：hp=maxHp20、pdef20、mdef0、e10、winding_left0、link空。 | HP=floor(1×20)=20；chapter按有效行动编号奇数蓄力、偶数物理重击，系数13/10，取活前排；游标从首个蓄力开始，免费补线不推进。推荐近战range1，不能把10×13/10先当属性再重复乘。 |
| E01/E02材质／状态 | N2-I1§2：机械金属，直接火系数1、烧伤／中毒免疫、闪避0。 | 是下一轮试调来源，尚非Published；冰／束缚免疫不在该表，本包无此能力不触及，不默认全免疫；若扩相应技能只阻其候选。敌法防0有明文，不属缺项。 |
| 战士主动前置 | 配置§3：嘲讽Lv5＋通用证1＋确认，CD2；本固定W1未学。 | 主动不触发有明确前置，不是缺技能填默认关；只纳入未学证明及必要定义闭包，不把完整火球／全职业作为W1阻塞。 |
| L1/L3正常奖励 | stages：base_xp20／24，tin_per_win2、wood_per_win0，首通卡0、入场／首通证0、unlock空；config／chapter奖励及calibrate输出可追溯。 | 固定锡片2、其余显式空集合；不将材料拆分算法视为随机掉落。真实ItemId／RewardPolicyId／ScorePolicyId及首通事件映射、审定／版本仍需M11→M10收齐。 |
| 评分／历史整数 | progression旧口径：L1经验25、终HP95；L3经验31、终HP90。配置／评分稿给E、推荐1、J=整关初始敌HP/2及精确曲线。 | 新承伤口径须重验，不能直接复制25／31：无暴击L1伤害30＋承伤5/4，L3伤害35＋承伤10/4。两口径分开绑定；原CSV不是新评分／新成长完整证明，准确整数出口交SD08／SD06。 |
| 随机／来源身份 | manifest记录v0.2.1、entry16-tutorial-v4、exact-winding及三个获准模型文件SHA，ad_seed=915。 | 这只是离线产物谱系；915不是战斗三域初态，源SHA不是ContentFingerprint。固定fixture身份及源定位可立即记录，Player/Challenge/Attempt/EntryBaseline须隔离生成，绝不复用真实ID／Commit。 |

推荐E01/E02行为显式记录damageKind=Physical、attackBase=10、range=1、targeting=FirstLiving、intentCycle及stableOrder；这里是从已知前排物理模型到配置的**待审映射**，不宣称旧CSV已有全部字段。缺正式行为ID／目标表只阻真实发布，不妨碍单人固定条件见证。
奖励曲线沿[评分§5](contribution-scoring.md#5-精确评分与整数出口)：B=floor(E×(3/4)^max(0,L−Lrec−1)×[1/2+1/2×log₂(1+C/J)])；伤害全部修正后floor、有效量封顶真实HP。旧模型epsilon／4位round不是运行契约；本包只抄源及手工算例，不执行数值算法。

## 3. 坐标候选与隔离身份

目标约定直接继承配置§6：零基、左下原点、x右y上。源README只定一基x/y、四邻接；boards及calibrate注记均未定义原点方向，不能从对称性／变量名／绘制惯例补出源方向。
推荐把“源为一基左下”作为明确待审转换候选C↑，(x,y)→(x−1,y−1)；若源方确认一基左上则C↓为(x−1,height−y)。举例L1 A在C↑为[(0,0),(0,1),(1,1)]，C↓为[(0,3),(0,2),(1,2)]，各有独立候选指纹；这两个结果都不是源事实。
M11同时固定sourceCoordinateConvention、转换公式／版本、源和目标尺寸、端点／逐格路径、反查位置；只转换一次。缺源方向时返回MissingSourceCoordinateConvention，阻L1/L3“忠实迁入”及正式发布；零基独立构造几何用例仍可执行后续授权测试。
fixture使用内部fixtureKey＋revision作为测试来源，CandidateContext保留草稿／内容指纹／Rule、Numeric、Random能力；隔离主体、CharacterId、ChallengeId、AttemptId、EntryBaselineId在隔离域建立并保存映射。固定Lv1、空物品和分支选择均列override及理由，不伪造真实玩家曾完成前置。
隔离对象不持H10写入口、真实存档路径、玩家Model、SDK／广告Use句柄；M06结果CommitEligible=false。胜利／奖励仅导出预期及证据，M07只核其纯候选，不发实际锡片、经验、通关或角色。真实入场必须另走M10的Published DefinitionBinding及M02原H02。

## 4. 证据封套与随机可达检查契约

| 证据组 | 必须保留／接纳检查 |
| --- | --- |
| 来源与候选 | 每个源路径／SHA／记录定位、草稿ID／修订、candidateKey、sourceFingerprint／contentFingerprint、规范化／坐标映射版本；语义有序面／槽／意图／路线不可被排序抹掉。 |
| 能力 | 实际执行器及编译器版本，Rule／Numeric／Random身份与可执行能力清单、支持域／预算、候选覆盖项；名称相同不证明版本兼容，NOT RUN与运行结果分栏。 |
| 原入场 | EntryBaseline及完整参与者／原槽／等级／已学技能／ComputedBaseStats／HP、C/U及来源、所有面、允许资源与许可、原种子来源／三域初态；未用项须明确空或有依据的0。 |
| 全部步骤及适用面 | 每个GuideStep种类、面、角色／目标／Pair／完整路径或非路线字段、阶段、ActionConditions／偏好修订、前后快照及事实／贡献／消费／历史。L1/L3各一面；两面测试必须覆盖两面及补线，不能拿第一面胜利替代全关。 |
| 随机轨迹 | 三域用途／映射身份；每域完整初态及前后(s,inc,w)、所属角色／被动的精确C、f、目标p与本次q；机会定位／顺序、全部原始字含拒绝组、采样结果、是否触发、终态；无机会与q=0/1耗字0要区分。 |
| 结论及审定 | 几何结论、全关阶段／报告指纹、证据种类／条件／覆盖与首差异；奖励原绑定／评分口径／精确整数出口另附。审定人、依据、候选和证据SHA、批准范围／未决、当前草稿令牌及取消检查齐全，M10才可检验发布条件。 |

固定被动“关／强制不触发／强制触发”仅为内部ConditionalReplay分支，不能变成玩家开关、合法后继或LegalFullReplay；“开”也只表示检查被动流程，并不证明指定结果随机可达。条件脚本必须给每个机会的有限选择和适用位置；已知C/f必出却强制不出者仅为反事实敏感性样本。
交SD06并由SD00整合的最小能力：按共同RC01／PC01／SC01创建或恢复三域状态、精确机会采样及PRD更新、逐字轨迹、完整纯转移和ReplayRecorded首差异；数值能力含有理数、最终floor／ceil、log₂整数出口及超限失败。本文不另选算法／C或读取并行稿。
LegalFullReplay检查：从完整来源初态按原算法实际产生每个字→重放拒绝采样及每次机会→核q和f变化／必出边界→同M06逐步规则结果比对→全关终态与报告相等。注入字列、seed标签或强制结果表不能替代此链；能力／C／状态／运行证据缺任一项只保留条件结论。
固定初态证明的是该初态及条件下的路径，不保证任意随机都赢；若主张自然进度可达，还须提供原来源资料、合法前置入场／进度链及无注入覆盖证明。固定测试值的合法范围检查与“真实玩家到达过这个状态”分别记证据，不能把旧L3的W1+53推成新评分后继。
奖励流独立于战斗流；固定材料无随机子过程须显式记不调用。增益若纳入另需原已完成基础、报价、精确倍率定义与独立流，绝不复用战斗seed。回退恢复战斗前态，重来新Attempt沿原EntryBaseline三域初态；未成、已成、未知按各完成域处理，不重复奖励或重抽。
M10发布至少要求：纳入闭包完整、坐标／值已审定、能力真实可执行、完整默认路线有LegalFullReplay及适用条件、受影响回归证据、内容和证据可取得、发布绑定可精确解析。不能凭ConditionalReplay放行；M14启用和玩家H10／保存恢复仍各自验证。

## 5. 手工文档见证（游戏／算法均NOT RUN）

以下输出是按条款手工推演的验收预期，未运行验证器或求解器；L1/L3采用显式C↑候选及W1满血、无主动／道具、被动未触发条件，非正式随机证明。

| 编号／输入 | 预期输出与完成依据 | 重复／中断结果 |
| --- | --- | --- |
| CV01 L1完整A/B参考线，仅6/16格 | 两对齐全、各步四邻接、互不交叉；手工几何成立，不需要满盘／唯一解，RPG尚未证明。 | 仅A线可为单步合法；拿全部level调用完整验证则缺B，不能当攻击拒绝。 |
| CV02 L3第一步W连A→E02 | floor(20×100/120)=16，A20→4未死，线消失；敌A蓄力、B攻击floor(10×3/5×100/110)=5，W100→95。 | 重演同条件得同值；取消／非法步不留下半条固化或局部HP／Random。 |
| CV03 L3再WA、再WB | 第二步A4→0、A线固化，B再击5使W90；B因A死成为首个存活敌，第三步15→0并固化B。全关WonPendingSettlement候选；仍无长期奖励。 | 原槽A0/B1不改，真实已成操作重复查原结果；纯重演必须重新求值。 |
| CV04 L1依次WA、WB | 有效伤害各15，原始各20；首步B还活击5，第二步清空，W终HP95；两线固化、全关报告候选。 | 评分分别保留旧伤害口径与新承伤事实；不能抄CSV25作为新口径已核奖励。 |
| CV05 独立4×4：A端(1,0)/(1,3)，B端(0,0)/(3,3)，A沿x=1竖连 | A当下合法并可固化；A占满该列后B被分隔，但不得因未来无完整几何解拒绝A。依据主稿§03／战斗§16。 | 允许后续按有效锚点回退或重来；“未来无解”不是当前非法，也不是RPG无解证明。 |
| CV06 构造已合法状态：最终面敌全死、PendingPair非空，甚至全员倒下 | 仍AwaitLinks，可免费补线；不攻击／不减CD／不抽随机，补完才WonPendingSettlement，无需强制复活。 | 中断保留完整前态／剩余补线；重复已成补线不再次产生效果。 |
| CV07 构造两面：首面敌空且补完，我方有伤／CD／U／PRD | 翻第二面，继承我方HP／倒下／CD／状态／U／Random；敌按新面新建，全倒则AwaitRescue；不是全关胜利。 | 翻面不推进回合，回退可回首面；本包未提供Boss数值，不声称该构造是L1/L3。 |
| CV08 回退攻击前锚点，随后同条件重演 | HP、线、贡献、消费、敌意图及战斗Random／f完整复原，新SceneRevision严格增加；ReplayRecorded结果及终态逐字段相同。 | 永久经验／当前偏好不复原；新行动用当前偏好，旧回调不覆盖新修订；未知先查原提交。 |
| CV09 立即重来后再对比普通退出再入 | 重来新Attempt、同Challenge／原基线、首面和原三域初态，无旧尝试收益；退出再入才取最新成长及新入场来源。 | 已确认学习／用卡／广告事实保留，准确Use撤回沿原H06；重试不换seed／重复转移冻结。 |
| CV10 条件表要求PRD必出时仍不触发，或只附seed不附状态 | 只能反事实／证据不足，拒LegalFullReplay和Publish；正常有限未触发条件也需实际随机链才升格。 | 扩预算只沿同前态，不跳拒绝字、不换种子挑胜利结果。 |
| CV11 作业r8期间改r9后改回r10同字节；另一路取消 | r8结果过时、取消结果失效，均不得发布；r10重新验证／审定。 | 迟到成功不能覆盖闸门；若Publish已在办且结果未知，只查原发布，不能宣称取消回滚。 |
| CV12 M13通过但M06缺精确C／完整路线，或第一面过而第二面失败 | 保留GeometryValid、明确缺项或失败步；非RpgValidated完整合法见证，非ProvenNoSolution，Publish拒绝。 | 重试只补受影响候选；不要求火球／全首章／在线部署先完成所有其他候选。 |

## 6. 可交源码的字段、阻塞与责任

| 范围 | 可立即供后续包使用 | 真正阻塞／责任者 |
| --- | --- | --- |
| D0单步几何 | 纯零基CV05、配对与占格约束、完整解区别、原语事实；不依赖L1/L3源方向。 | §65worker实现／验证后SD00独立审阅；本文不检查或接受并行源码。 |
| L1/L3条件fixture | §2全部原文路线、尺寸、W1含法防、E01/E02已知属性／循环、固定材料、显式条件及来源；可写入测试资料，不等于发布。 | 源方向阻忠实坐标导入：SD10保留两候选，SD00统筹内容源注记／审定；行为映射待M11/M10审定，仅阻其真实发布。 |
| L1/L3正式随机回放 | 同规则入口、完整证据字段和逐机会检查可先实现设计。 | 精确C／参数审定与完整状态及实际能力证据：SD06提供，概率差异由SD00汇集主策划裁决；缺哪一项就阻该LegalFullReplay。 |
| 新评分与奖励 | 固定E=20/24、锡片2、完整报告／参与者／公式输入；旧25/31仅历史对照，法防非阻塞。 | SD08核评分口径及整数出口，SD06给Numeric能力，M11补类别化奖励／事件映射；未核前只阻对应奖励候选及带该奖励的发布。 |
| 真实Demo入场 | 候选／固定测试域与玩家域隔离；完整绑定、发布证据、同结果恢复为必要条件。 | M11/M10收齐本小闭包、实际运行／审定后发布，M02/M12核H02/H10及原绑定恢复；不将包名或文档接收当Published。 |

仅建议下一份源码包：**D0接收后，固定测试输入／来源与修订隔离验证**；不实现整个M11编辑器。新建路径精确为 `Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs`、`Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs` 及各自 `.meta`（4份）；若复用完整验证器，另允许修改 `Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef` 仅追加FlowPuzzle.Validation引用（共5份，待SD00核实际程序集及新路径不存在）。
该包验收建议：显式源方向才转换一次，缺方向拒绝；来源／Pair／enemy映射完整、深副本不改原值；同值新修订不收旧证据；D0路线与M13完整解分开、固定fixture不能创建玩家写句柄。暂不承诺不存在的M06实现API；战斗／数值回放、奖励及真实发布分别另列后续窄包。
此为建议白名单，未创建以上文件、meta或程序集变更；其测试命令与Unity进程安全由SD00另包授权。完整编辑器、全首章、最终火球倍率或在线搜索部署均非统一前置。

## 7. 共同契约建议与实际检查边界

旧约束H01／D04／D05、CandidateContext／EntryBaseline及三域随机已具备；反例是把旧CSV、条件开关、同内容旧修订或几何绿灯作为发布证明。推荐在M11局部证据明确“证据种类＋源坐标约定＋适用范围＋当前修订”；代价是保存来源／条件及重新验证，影响H01、M06/M09/M10与A11，不新增第二套公共身份或改变玩法／收益。
本文只提交精确C／随机能力、评分出口、源坐标／行为映射和小闭包发布缺口；无新的玩法问卷。SD00只写三协调稿，SD06并行只写numeric-random.md，§65worker及原有改动另属各自范围；未修改、整理或归责为本任务污染。
静态检查使用Test-Path、Get-FileHash、Get-Content、内存只读PowerShell全文检查及git diff --check -- docs/system-design/2026-09-16/details/content-validation.md；实际行数、SHA、字节、链接／锚点／围栏、冲突／尾空白及退出码随交付报告给出。新文件未跟踪，空diff不能代替全文检查。
NOT RUN：Unity编译／EditMode／Player、任何游戏／模型／求解／数值／随机算法、真实回放、SDK／在线服务／存档、依赖安装；本包只做静态文档检查及CV01～12手工推演，没有后台任务或子代理。

## 8. 起始输入SHA256

以下为开始时只读快照；路径相对项目根，SHA按文件实际字节。对53份许可路径登记哈希（部分Core文件仅作清单记录），真正业务取值限§2所列来源；目标起始无文件，最终SHA在外部交付中给出，避免自引用。

| 输入路径 | SHA256 |
| --- | --- |
| `AGENTS.md` | `C1A836FF14CC0834E16C33CBC49D53BEC82CADFF656A329C24C72F569D460528` |
| `.agent/PROJECT_CONTEXT.md` | `4CF1E141E39F12165103E52E44357B1C179175BD18D2C874F386F0360ABFC9A9` |
| `.agent/PLANS.md` | `5039F9BC8A51EAFAC968B652B09906B90C6F335395DC3AFEC3899FB83A7324EB` |
| `.agent/VALIDATION.md` | `B65114C2444DEE680741E52307FEC57CC0ABC4B1196767962BC8E4DF322690A0` |
| `.agent/REVIEW_CHECKLIST.md` | `B5C39A7C85D2FD41182AC4AD3DB30CF7ED28E0D9A696354F4CFE5A507E57C8A7` |
| `.agent/tasks/FM-SD00-2026-09-17.md` | `B630D3844CE518B231BCD93BA81BE39FDFE58FB31D1F71BD544BFB191AEEFEE8` |
| `docs/handoffs/2026-09-17-system-design.md` | `821C37ACCD820E74E33A3C29AA35DD00FFF2706BDA878F48EF5646168850BA87` |
| `docs/system-design/2026-09-16/system-catalog.md` | `A58EBE88E951162EB047F95284409E4C937853D2359604514A71B4A37DF6ED6D` |
| `docs/system-design/2026-09-16/interaction-contracts.md` | `EAB83027C412619731DF1F7F3FD1650161915D96E31D68EA30F4AF4AF9F96F06` |
| `docs/system-design/2026-09-16/runtime-extension-contracts.md` | `A4FEE0A8CEC2E61991570032833685F8E4CD79BB6AC460DDB7E0F4AFF04A8479` |
| `docs/system-design/2026-09-16/cross-system-flows.md` | `8BEDC94B042E3002C453276AA1E2C4378CCCE52A9920AA4A99049B6D525B3D74` |
| `docs/system-design/2026-09-16/session-plan.md` | `4381F13384DBC6856B6672297EF83397311A3406C38A458FD175AB1259A98760` |
| `docs/system-design/2026-09-17/integration-review.md` | `3423CE08B2B3BC2DD09C76674C953C4A59B1366B89D0B8980C8686FD523E3900` |
| `docs/system-design/2026-09-17/system-task-packets.md` | `05E1A81C5C96CD9B35746F8122BB01E36F664841F8BEE959C823860643283D41` |
| `docs/system-design/2026-09-16/details/configuration.md` | `1AB07F8E5AFB1D2FDFC4A4A2CF5E436E8F876F1E4371A94A68CA9BFB25C4794B` |
| `docs/system-design/2026-09-16/details/battle-history.md` | `0A9C30F131822DAD484A12B234A0262B49897F3190837A2655DA4024A665FE41` |
| `docs/system-design/2026-09-16/details/guidance.md` | `815E0ABEC3A1E545D865390027AEFEA13A1E97F8B6FFC6C39ACA70A28F8ACA2E` |
| `docs/system-design/2026-09-16/details/character-growth.md` | `E83BA93810C729579D1D817D356C8B59FD6999C066EAD478D83BDE918735524B` |
| `docs/system-design/2026-09-16/details/contribution-scoring.md` | `5194DC775EEB2BF359604EB1966A20A51062C7C48BFEDFCE5C15601EC1931912` |
| `docs/system-design/2026-09-16/details/settlement.md` | `0D4915DCD800AC97E957D697FCE2AB603B081BB1041DF75690B3E3A819598A77` |
| `docs/game-design/2026-09-14-fightmatch-game-design.md` | `5495AE36BC1E03CB42FFD428416D2C36855370935DFDA5769EDE28317393E82A` |
| `docs/game-design/2026-09-15-three-group-recommendations.md` | `1C86E32869355AD9EAD54F333EF3DF8FFB3238CEE658EFC92C2F37979D93F223` |
| `docs/game-design/2026-09-15-content-planning-sync-reply.md` | `45D5CD3CBE5C0A80C295E98765F5D904FF3186E984A848B4CDF4BADAE0F94C08` |
| `docs/game-design/balance/2026-09-19-calibration-inputs.md` | `9664ED3B923E71720EA2D08ACE678E3E5033C4929EE809451576CB81A2984494` |
| `docs/game-design/balance/config.json` | `4E815411914BF86FE9E1A046EE80B86C24D048BD8455A1D4BB55A088255AA7F3` |
| `docs/game-design/balance/README.md` | `8216072DCBD0904B14E3A412AD73EB050CFC5534C5B8CF31286DACDF7D1D5548` |
| `docs/game-design/balance/calibrate.py` | `698CF18F125A7E63B27E1212F32F1985C16118A1ADF09A6FFE6E559790976052` |
| `docs/game-design/balance/chapter_model.py` | `2052792CCC3AAE007F6A23A2F799D4D724C3ECFDE97644D9731E4D706BBEC390` |
| `docs/game-design/balance/results/boards.json` | `671467CAF72C52EB83A03B396AD82F5557A47B2F9CBD3F9DA6D0EC560A7D2AAD` |
| `docs/game-design/balance/results/stages.csv` | `814536D3B9DE969F0F85673985E9B188857166C83967BF67AF74EAA6E3A9DB2D` |
| `docs/game-design/balance/results/enemies.csv` | `238493BEC0BD293B12D0CFDB0395D4246FBB379E59573EC94A590E5DC25EC9D9` |
| `docs/game-design/balance/results/progression.csv` | `D67E7FB3F6DEE1799E521D03724472A2B1997E775F531753040C12B48AACEB18` |
| `docs/game-design/balance/results/manifest.json` | `6E8EC70888FD67F7978AD8D7D6B2DA8F61C514C9576DB1EEC32DC57FB7BBA127` |
| `Assets/Scripts/FlowPuzzle/Editor/Draft/FlowDraftMapper.cs` | `2782BAB003FC69604E65BFE56E71CBC11B30CFB4DC3EA229CB13D81E408A233A` |
| `Assets/Scripts/FlowPuzzle/Persistence/FlowLevelAsset.cs` | `7B3A0DA2AFA376D6441B0E28BB020A3CE67B536380CCC723F94FFD2F10E295F4` |
| `Assets/Scripts/FlowPuzzle/Validation/FlowValidationResult.cs` | `6198F6816D3A0A861A00A2532CCF511B9491CE0F489C4E6DB20A9D61C22B3646` |
| `Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs` | `FB732BE5E11FCDF176A92D7219CE6B9894BFB9B8D976D6AE83EEB1C4E252B579` |
| `Assets/Scripts/FlowPuzzle/Core/FlowSolutionData.cs` | `6FB6D5C60BAF9A3B4E724DCBBCC6EE8AFD7DCE0DFE4104A0661CFAC9CA0CEC7C` |
| `Assets/Scripts/FlowPuzzle/Core/FlowPos.cs` | `52CC1AA7FA88C50B9CD3D17071F3DF99098ECAD1ADA58B9D5636FAFFE7CB4F6A` |
| `Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs` | `AF6914555F3374965337C36816FA839888A412C2A58202A019BD743043661949` |
| `Assets/Scripts/FlowPuzzle/Core/FlowPathData.cs` | `71E7B5D15AF03B9C4037D9F91136FEEEEB8EFF3CF738D43D5EB33644250B6E42` |
| `Assets/Scripts/FlowPuzzle/Core/FlowParameterSuggestion.cs` | `52FA0E8AB5690DAE5844E9AC80B31D84EAEC5690CED6DDC6AEF2F7C8988FE165` |
| `Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs` | `700458B31665FE69FE7E723A564ED7B0846AFCDE9202244813B180D2609B10F3` |
| `Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs` | `13F9D629009B95A58DAB452EE9429ABE6ADC492CA3037A8DA2BE27728C000CC6` |
| `Assets/Scripts/FlowPuzzle/Core/FlowGenerationResult.cs` | `A2E52B7562D638715BEC9B197688C825335BB0A91D505A0DEF107287C4FB46D7` |
| `Assets/Scripts/FlowPuzzle/Core/FlowGenerationConfig.cs` | `1CEEF60666AE3D805A85BAF7331EB45B08DCE81AFE89A4717424DC4F4AA2C2DD` |
| `Assets/Scripts/FlowPuzzle/Core/FlowGeneratedLevel.cs` | `11305D7698891D16E3E60ED41950A935AB7B23ADFCF9BDEDF15DE1FB12E2B466` |
| `Assets/Scripts/FlowPuzzle/Core/FlowFailureDiagnostic.cs` | `70435A173A5E2084D7942827A59BF066CC9A6E3A9A3368EF843A963A15B7D343` |
| `Assets/Scripts/FlowPuzzle/Core/FlowDifficultyTier.cs` | `B0A4C1F536BFE75258247062ECB9B9A3DD951DAE253EAAB297B8D0317F0FAEE7` |
| `Assets/Scripts/FlowPuzzle/Core/FlowDifficultyReport.cs` | `DF3A36B63E81AFA4D7B36134976CC259ED1BE19D033E928B1EA147DD39F3721A` |
| `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticMapper.cs` | `045D82F47CCF56BDB4F19FB320CE1926EA53980E82E4CB2EBD676C03D881EB8D` |
| `Assets/Scripts/FlowPuzzle/Core/FlowDiagnosticCodes.cs` | `84496D0D52F9EB4D54409DD0E47A3988D21AAE22DD15796F68483454B81F086B` |
| `Assets/Scripts/FlowPuzzle/Core/FlowBoard.cs` | `AD9382B82D7B72D35032B13C4BEAE87424B8AA14D91ACF109272E59BE81466FB` |
