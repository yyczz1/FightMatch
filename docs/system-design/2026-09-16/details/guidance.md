# FightMatch · 攻略内容、验证结果、搜索与共享规则设计

2026-09-20 · r4 · M09 · **SD09-DELIVERY-01／C1原范围完整保留；SD09-SEARCH-01-C1只收紧完整证明范围和非路线步骤，不构成Unity实现、在线求解、AI模型或服务接入授权。** 当前范围为S14既有交付／历史／GP-R1，加候选生成、共享规则验证、受控搜索与证据；默认攻略始终免费。

依据：[精确包第47节](../../2026-09-17/system-task-packets.md#47-sd09-delivery-01--攻略内容验证结果与交付记录)、[目录M09／S14](../system-catalog.md)、[共同H09／H10／§20](../interaction-contracts.md#11-h09-攻略请求验证与使用)、[扩展§13～14](../runtime-extension-contracts.md#13-gp01在线服务许可交付确认与关闭恢复)及[正式主稿§11](../../../game-design/2026-09-14-fightmatch-game-design.md#11--免费默认攻略与在线剩余步骤)。

## 1. 实际输入与边界

启动时`guidance.md`不存在，无起始正文／SHA256。本包实读版本与哈希如下；计划r45、审阅r29和任务包r30只较包内锁定r44／r28增加用户委派，按SD00明示交接不改业务规则。

| 输入 | 实际版本 | SHA256 |
| --- | --- | --- |
| `system-catalog.md` | r3 | `A58EBE88E951162EB047F95284409E4C937853D2359604514A71B4A37DF6ED6D` |
| `interaction-contracts.md` | r18 | `EAB83027C412619731DF1F7F3FD1650161915D96E31D68EA30F4AF4AF9F96F06` |
| `runtime-extension-contracts.md` | r15 | `A4FEE0A8CEC2E61991570032833685F8E4CD79BB6AC460DDB7E0F4AFF04A8479` |
| `cross-system-flows.md` | r5 | `8BEDC94B042E3002C453276AA1E2C4378CCCE52A9920AA4A99049B6D525B3D74` |
| `session-plan.md` | r45 | `EC79E3722C83449089B889C6ADE398083F7045E5E7E646A8258E63959949D626` |
| `integration-review.md` | r29 | `3F0B4E1B3B812A22D4E1B138632C274DB0D0AFB2462717DF1C1034A838779E32` |
| `system-task-packets.md` | r30 | `50EFD6365DC24A890E8333B1AFEB5B227BD0F357B11D125F76B41C9F5E38C2D8` |
| 正式主稿 | 0.2，含9月17～18日补充 | `5495AE36BC1E03CB42FFD428416D2C36855370935DFDA5769EDE28317393E82A` |
| [SD07](ad-entitlements.md)／[SD06历史](battle-history.md)／[SD06效果](battle-effects.md) | r1／r1／r1 | `CE9220BC2934DCDD365FFCEB1BC6787D5C012ADA6B8E577BA7E88C07588A6C3B`／`B88D88BD9119E7D8C520F260B6A4D3864A38AE3219B5A992947F819E0D1B0B5D`／`3CFD66243A2846CF5ACE15BF21F88E7D6D8180481F44695CC2B588433BC3065D` |
| [SD02](platform-save.md)／[SD11](application-flow.md) | r3／r1 | `9AA10036F9615745035185A7887B0F2E73C94702CEF0A60CAF42B19BA059ECCF`／`12BFC335B926D2530BAA4238B7D9E6B7906AC1DE534AFECD1EB5904310BF21BE` |

r1成稿检查时SD00并行登记已使`session-plan.md` r45变为`FDA60BE85FBDA4F63E1B7ECE0C27E309E7CE215EBBF0A0456F47BF1FD5295221`、`system-task-packets.md` r30变为`C82B232A75118CFEF2DEAD39234A9CBAE64BD5DAB0565B11E8F060716ECC920B`；C1启动时进一步实检为计划r46 `C474146A543EA4566A60484484CDE62A8651174EC31CF5B33CA3619BAD445E5E`、审阅r30 `EDA0CE23A8CAAB16AC5EBBFE663BD078144C1256796349336C880D028F681A61`、任务包r31 `DE69A9EC70A02E71133EB47356A54DE95827661B487437C1D289144B194AE6C2`。上表保留本任务首读哈希；这些变化只是委派／审阅记录，共同r18、扩展r15及业务details哈希未变，不作业务重基线或范围污染。

仓库规则实读SHA256：`AGENTS.md`=`C1A836FF14CC0834E16C33CBC49D53BEC82CADFF656A329C24C72F569D460528`，`PROJECT_CONTEXT`=`4CF1E141E39F12165103E52E44357B1C179175BD18D2C874F386F0360ABFC9A9`，`PLANS`=`5039F9BC8A51EAFAC968B652B09906B90C6F335395DC3AFEC3899FB83A7324EB`，`VALIDATION`=`B65114C2444DEE680741E52307FEC57CC0ABC4B1196767962BC8E4DF322690A0`，`REVIEW_CHECKLIST`=`B5C39A7C85D2FD41182AC4AD3DB30CF7ED28E0D9A696354F4CFE5A507E57C8A7`，上游SD00包=`B630D3844CE518B231BCD93BA81BE39FDFE58FB31D1F71BD544BFB191AEEFEE8`，交接r2=`821C37ACCD820E74E33A3C29AA35DD00FFF2706BDA878F48EF5646168850BA87`。

已实读现有Flow几何Interface：`IFlowPuzzleSolver`=`1BD6351715C929EB1127CB7B655095950C74044F4B470DF63885ACA27917F6D0`，`IFlowLevelCompletionProvider`=`16BCA32FA930E3D402601BCBD5BF17B543E7C84F7E5AB96E5B18CC2DC6B53B5F`，`FlowSolveRequest/Result/Status`=`F15D7AAFB6F48784E9EB792C7709ED0566F64428FD61287CCD36B43D63477492`／`08D2B00045223128142E1150C2A59B927B517A7AC07D77A756B85C812AE9A95D`／`DCB4B5C55ECE0AF3B95F29D4FC95E46B9C9C07FC9F12F3000BB4EDF48D89A9DC`，`FlowCompletionRequest/Result`=`EDFB09B0BDC71A51196A02423619A0B4676091D3E4369E104D66CFC4ABAA61BB`／`066BC99CBBC8E3F87577E8C1AA47FF5FBAE86150754E0830FE0A5F4788E8A283`，`FlowSolutionValidator/Result`=`FB732BE5E11FCDF176A92D7219CE6B9894BFB9B8D976D6AE83EEB1C4E252B579`／`6198F6816D3A0A861A00A2532CCF511B9491CE0F489C4E6DB20A9D61C22B3646`，`FlowLevelData/SolutionData`=`13F9D629009B95A58DAB452EE9429ABE6ADC492CA3037A8DA2BE27728C000CC6`／`6FB6D5C60BAF9A3B4E724DCBBCC6EE8AFD7DCE0DFE4104A0661CFAC9CA0CEC7C`。它们只表达四邻接路径、端点、不重叠、固定前缀和几何求解状态；`NoSolution`仅是该几何问题的状态，不是RPG当前局面的`ProvenNoSolution`。

## 2. 唯一归属与对外交接

M09唯一维护S14，在同一切片中分开下列记录，不新建公共ID：

| M09记录 | 内容与不可混同的边界 |
| --- | --- |
| 默认参考 | LevelId＋DefinitionBinding、从开局开始的全面步骤和阵容／等级／技能／道具／操作／随机前提；永久免费离线可读，不冒充当前局面保证。 |
| 请求及原归属 | AnalysisRequestId、Player／Level／Challenge／Attempt、原ContextKey、服务Grant／Use引用、输入摘要、请求状态及原Operation／Commit依据；每次分析独立记录但不单独消耗广告。 |
| 候选与验证 | 原始候选、服务分类、完整验证见证或无解证明范围、绑定与预算信息；候选或哈希不等于当前有效或已交付。 |
| 当前适用与历史 | 经当前完整ContextKey复核的结果单列；过时内容保留原条件仅作历史，不回装战斗，不用相同内容哈希当新验证。 |
| 交付证据与关闭摘要 | 当前合格结论、内容／依据可读性、接纳OperationId／CommitId、服务引用及全请求覆盖；第一次交付与后续更新均可追溯，不被最后一次失败覆盖。 |

| 交接方 | M09必须取得 | M09输出／明确不负责 |
| --- | --- | --- |
| M08／SD07 | 准确Grant／Use、原Level／Challenge服务关系、当前资格修订、许可／恢复决定及查询依据 | 交实际交付证据、所有请求覆盖和关闭摘要；不修改Grant／Use，不恢复资格。 |
| M05 | 合法Challenge、原Level、开／闭依据和后续同关新Challenge | 交该Challenge的内容／交付摘要；不延长已通关Challenge或改进度。 |
| M06 | 同一完整头的BattleSnapshot、允许动作、ActionConditions、完整随机／PRD和可隔离重放的共享战斗规则 | 交候选操作序列或具体缺口；不写HP、SceneRevision、历史、道具或随机流。 |
| M02／M12 | 单队列中的当前头，请求／接纳OperationId，H10 Commit／Lookup结果，外部回调原归属 | 提出S14完整候选；只有H10后显示已交付，不持锁等网络，不以服务器回调直写玩家状态。 |

## 3. ContextKey、条件与隔离验证

`AnalysisRequestId`识别一次请求，`ContextKey`只表达结果对业务输入的适用性。请求记录另存取得隔离副本时的CommitId／SaveGeneration及请求登记CommitId作取证引用，它们不是ContextKey的相等字段。`ContextKey`使用共同定义，至少完整绑定：

- 归属：PlayerId、LevelId、ChallengeId、AttemptId、SceneRevision、DefinitionBinding（含Rule／Numeric／Random版本及全部剩余面）和准确服务关系；归属或服务目标改变才是业务输入改变。
- 局面：阶段、棋盘／固化线／待补Pair、敌我全实例与原槽、HP／倒下／CD／异常／盾／意图／能力次数、当前面与余下面、本局携带／余量、冻结和已用资源引用。
- 动作：每步角色、目标、路线、顺序、免费补线、可用阶段及每步`ActionConditions`；包含已提交的PreferenceRevision和显式道具开／关，不用UI当前选中或动画状态。
- 资源：仅允许当前已取得道具、余量和准确的复活资格副本；记录相关资格修订与使用前提，验证中不真实消耗，不假设未来广告、无限复活或未获物品。
- 随机：全部相关流状态／域、原始字计数、PRD累积`f`、机会顺序及已固定分支依据；不用同名版本、显示数字或新随机流代替。

接纳时M02重读最新头，以它作本次H10的`expectedHead`并从领域切片重建上述业务键。仅请求登记、保存历史或M09／M08对本次交付的内容／服务投影H10使头变新，而局面、偏好、资源及服务目标语义均未变时，不使结果自致`Stale`；真实业务键变化才失效。候选提交前`expectedHead`又改变时，按H10返回过时／Busy并重读，不覆盖新头。

候选在隔离副本上调用M06同一规则，按同一精度、效果顺序、随机消耗和PRD执行到整关胜利或所声明的全部合法后续已穷尽。现有Flow能力可作为路径四邻接、端点、重叠和固定前缀的局部见证；队伍伤害／生存、状态、资源、随机及Boss余下面仍必须由战斗验证。

| 结果 | 精确含义及交付性 |
| --- | --- |
| `ValidatedWin` | 全部剩余面、每步合法性、资源／随机条件及整关胜利见证齐全；当前适用并按§4保存才计交付。 |
| `ProvenNoSolution` | 对声明的当前绑定、动作、资源与随机范围有可检查的完备穷尽证明；按OP-R1与有效获胜同样计交付，不承诺任意局面即时可证。 |
| `InvalidCandidate` | 候选未通过共享规则，给出失败步／缺口；非无解，不计交付。 |
| `Timeout / BudgetExceeded / Cancelled / ServiceUnavailable` | 保留原请求、已知进度与准确原因；均非无解，不计交付，同挑战可免费再请求。 |
| `UnsupportedBinding` | 缺原绑定或执行能力；保留资格／证据并只阻受影响分析，不默换新规则。 |
| `Stale` | 候选原键与当前不同；可带原条件存历史，不计当前交付，需新AnalysisRequestId／ContextKey重验。 |

## 4. 交付、历史、补证与关闭

1. M09在外发前以H10登记原服务关系、AnalysisRequestId、ContextKey、完整输入摘要及恢复引用；只向M12交分析输入，不传整份永久存档。外部等待不持业务写锁。
2. 回来的候选先留在隔离记录；M02入队后按§3重读最新完整头作`expectedHead`，M09重查Player／Level／Challenge、服务关系和真正ContextKey。过时时只提历史候选，不改当前战斗。
3. 当前合格的获胜或可靠无解，将可读内容、完整验证／证明依据、原适用键、服务引用、接纳OperationResult和交付事实按该`expectedHead`同一H10候选保存。这一接纳对自身服务投影的更新不算分析输入变化；只有`Completed`才显示已获得当前分析，后续真实局面／偏好／资源／服务目标变化再使其过时。预览不自动操作、不推进SceneRevision、不消耗真实资源。
4. 旧内容仅作历史。既成当前交付后因玩家行动变旧，交付事实和内容仍保留，不返还成未交付；玩家未照做或后续更新失败也不倒改事实。
5. 同一Challenge内，首次合格交付后的行动、撤销、重来和退出续局均免费重新分析；新Attempt或新局面使用新请求／键，但不新造Grant／Use。有效获胜与可靠当前无解都按OP-R1计交付，服务仍免费更新到该Challenge通关。
6. 挑战关闭摘要覆盖该服务全部已登记AnalysisRequestId，分别列当前合格交付、仅历史／失败、未决接纳、终止及确认待办。同源H06关闭必须把M05的Challenge关闭、M09该全请求／交付摘要、M08的准确服务引用与确认待办同一H10保存，并向远端一起交这份完整依据；不先宣布服务结清再补交付证据。任一合格交付后关闭则服务结束；全部未交付且旧接纳不可再成时，M09才交完整未交付证明给M08走正常释放。一次失败、最后一次请求或空内容不能代替覆盖。
7. 补证只补原事实：本机精确H10／同提交检查点已有合格交付时，回传原内容、依据、Commit和Use引用；云端已确认而本机缺记录时，取原结果同H10安装为可回看内容，不冒充今日当前攻略。只有候选／哈希、空档或单条失败不补造交付成功。
8. GP-R1只在以下全部准入同时成立时进入：原主体、广告Grant、准确在线服务Use及其许可可核验；原Challenge已有合法关闭依据；源设备及本机档已永久丢失；全部可取证据仍无法判明该Use是否实际交付。M09只交原Use全请求范围、已知交付／历史内容、查询结果与后到补证；由M08作RecoveryDecision，恢复原Level服务资格并接受可能已交付。原Challenge仍可继续走原服务，普通断网／超时／CommitUnknown查原操作，已知交付走正常结束，完整未交付走正常释放，原事实／许可不可核验或不同Level返回准确缺口；均不套GP-R1。M09不自产资格、不重开原Challenge、不伪造NoEffectFinal；恢复后的合法新Challenge仍用新ContextKey验证，旧证据迟到只补历史，不追回资格或改后继Use。

## 5. 手工文档见证

下列全为设计推演，每条的游戏、求解、保存、SDK和云端实测均 **NOT RUN**。

| 编号 | 完整输入 | 预期输出 | 完成依据 | 重复／中断结果 |
| --- | --- | --- | --- | --- |
| OP01-1 | C7／A1／r18、全局面／剩余面、合法Use U1、道具／偏好／随机完整，候选覆盖至整关胜利 | `ValidatedWin`，保存完整步骤和适用键；真实HP／道具／随机／SceneRevision不变 | 内容／验证／U1引用／接纳结果同H10 Completed后记当前交付 | 同回调或换OperationId查原请求，只返一次交付；Unknown先Lookup |
| OP01-C1 | 取样头G40后只完成请求登记到G41，局面／偏好／资源／服务目标均未变；返回时又只保存历史或本次服务投影 | 以最新头作`expectedHead`重建业务ContextKey，键相同则不因G40→G41自致`Stale` | 合格内容及投影按当前`expectedHead`同H10成；接纳自身不改分析输入 | 提交前头再变则返回过时／Busy重读；真实资源或关系改变必须失效 |
| OP01-3 | 分析取r8，期间玩家行动／撤销／重来到r10，或PreferenceRevision改变 | 原结果`Stale`，仅带原条件保存历史；新Attempt／键另请求重验 | 历史H10只新增S14，不回装r8；未形成当前交付 | 相同哈希不复用；过去若已交付，变旧不撤销该事实 |
| OP01-5 | 同一当前键分别输入“所有合法后续完整穷尽”与“仅用完节点／时间预算” | 前者`ProvenNoSolution`，后者`BudgetExceeded`，绝不改名无解 | 前者证明范围／内容／服务引用同H10成则按OP-R1计交付；后者不计 | 预算失败保留原服务免费重试；已成无解后仍免费更新到通关 |
| OP01-6 | 当前合格结果提交分别遭遇SaveFailed、CommitUnknown或Completed但回执丢失 | 分别为未交付、未决并锁写、查得原已交付 | 仅原OperationId／CommitId及完整头可决定；候选可读不代替H10 | 重试原候选／Lookup，不增Use、不换ID记二次，不回装旧整档 |
| OP01-7 | 已交付D1后玩家行动使其变旧，下次更新超时，随后立即重来A2 | D1和交付事实保留；超时不退资格；A2以新键免费分析 | D1原Commit／适用键与A2新请求分开，同C7服务关系不变 | 再次超时仍免费；新Attempt不复用旧回调，通关才结束服务 |
| OP01-8 | 合格接纳H10与C7通关关闭H10分别以两种先后发生 | 接纳先成则记已交付并关闭服务；关闭先成则迟到结果只存历史，未交付资格保留 | 关闭时M05 C7关闭＋M09全请求／交付摘要＋M08引用／确认待办同一H10，再向远端交整份依据 | 迟到回调不反改已成关闭；Unknown先查原联合操作，不先结清再补证 |
| OP01-11 | C7当前只有完备无解证明，其后玩家用独立广告直接通关 | 无解按OP-R1已交付；通关结束C7服务，内容可回看，新Challenge需新权益 | 无解内容／证明／适用键／服务同H10已成，与直接通关Use分开 | 通关前更新仍免费；直接通关重试不重记攻略交付 |
| GP01-3 | 当前合格交付的H10／同提交检查点完整，但上传交付确认失败 | 保留本机已交付、原内容和补证待办，将精确证据后补M08 | 原Commit、ContextKey、Use引用和验证依据共同证明；今日已Stale不撤销当时交付 | 确认超时查原Use／Operation，不重算、不重扣，同补证只安装一次 |
| GP01-5 | 同C7早已有D1合格交付，后来一次分析失败 | 不得报全服务未交付；C7内更新仍免费，C7关闭时服务结束 | 关闭摘要同时含D1与后续失败，不以最后一条覆盖 | 重复失败只增原请求结果；不释放Use、不返未交付资格 |
| GP01-6 | C7关闭时全部已登记请求均无合格交付，所有Unknown已查清且旧接纳意图已持久终止 | M09交完整覆盖／终止／未交付证明；M08按H14正常释放，同关后续合法目标新Use | M05关闭＋M09全AnalysisRequest清单／终止＋M08引用／确认待办同一H10，完整依据一起交远端；M09不自恢复Grant | 旧回调只归原查询，不抢改新关系；缺一请求或联合提交未决时不释放 |
| GP01-8 | A／B同源C7；B合法通关且零攻略，A的原主体／Grant／准确Use与许可可核验，但A源设备及本机档永久丢失，全证据仍无法判交付 | B只证明自己；M09保留A全请求范围和具体Unresolved，全部GP-R1准入成立后才交M08恢复原关资格 | A来源查询范围、B合法关闭、M09覆盖和M08当前记录共同证明；B普通进度不受影响 | 原C可继续、普通断网／Unknown、已知交付或完整未交付各走原路；迟到A证据只补历史，不反转决定 |

## 6. 后续精确能力与未证明范围

| 负责者 | M09所需的精确后续能力 |
| --- | --- |
| SD07／M08 | 可查的Grant／Use与Level／Challenge服务关系，当前资格修订的条件裁定，交付确认／关闭／正常释放及GP-R1 RecoveryDecision的持久查询；必须接受M09的全请求摘要而不自建内容权威。 |
| SD02／M12 | S14请求、内容、验证、历史／当前标记、交付证据和关闭摘要的H10完整持久／Lookup；外部请求跨进程原归属和回执补取能力必须如实申明，不达标时返回准确Unknown／CannotReconcile。 |
| SD06／M06 | 可按原DefinitionBinding执行的完整BattleSnapshot、所有合法动作／资源前提、精确效果顺序／数值／随机／PRD和剩余面的确定性隔离重放；并提供能区分合法胜利、候选失败与声明范围完备无解的可检查证据。 |

本包尚未证明：在线搜索算法、任意RPG局面的无解证明或即时响应、模型服务商，M06完整攻击／所有敌我能力，正式PRD表与跨端确定性，多盾，真实H10 durability、SDK／远端认证／条件提交，跨设备排他和Unity接入。缺完整火球回放只阻相关实际候选的完整战斗验证，不阻本文已定的记录、分类、交付、历史、补证和关闭Interface。

## 7. 限定结论与共同契约建议

本文不增公共身份、不改唯一修改权，不建议修改共同契约。旧约束已给出S14、H09／H10、OP-R1及GP-R1；本文只把“最后一次失败”可能覆盖早期交付、“几何无解”可能被误当RPG无解和“目标档零内容”可能抹掉源证据三个反例落到M09内部记录分离。代价是持久全请求索引、原适用条件和交付／关闭证据；影响M09、M08、M05、M02／M12和M06，不扩大任一方的业务写入权。

**NOT RUN：** Unity、Flow／RPG算法、模型、SDK／广告／在线服务、真实存档、掉电／跨进程／跨设备、构建和性能。本文的12条见证全为手工文档推演，不得记为游戏测试通过。

## 8. SD09-SEARCH-01实际输入与追加边界

起始全文为r2／116行，SHA256 `FE1B8F92DF86B4D9103E4341BC740B8505FAEC25E83F214C13DB78B5402087A3`；C1起始r3 SHA256为`581FD5553D008D355519A3EEC0DB5CE3687F0687648C414165192BA7BE8F4ED8`。原§1～7、12条交付／历史／GP-R1见证及其顺序保持。本轮前置已由SD00实际ACCEPT：[战斗r3](battle-history.md) `0A9C30F131822DAD484A12B234A0262B49897F3190837A2655DA4024A665FE41`、[结算r3](settlement.md) `0D4915DCD800AC97E957D697FCE2AB603B081BB1041DF75690B3E3A819598A77`；C1只处理审阅§44反例。

| 实读组 | 实际版本／SHA256 |
| --- | --- |
| 共同基线 | 目录r3 `A58EBE88E951162EB047F95284409E4C937853D2359604514A71B4A37DF6ED6D`；共同r18 `EAB83027C412619731DF1F7F3FD1650161915D96E31D68EA30F4AF4AF9F96F06`；扩展r15 `A4FEE0A8CEC2E61991570032833685F8E4CD79BB6AC460DDB7E0F4AFF04A8479`；流程r5 `8BEDC94B042E3002C453276AA1E2C4378CCCE52A9920AA4A99049B6D525B3D74`。 |
| 协调状态 | C1实读计划r62 `DC965DF636CD7608CE925A8378642DED20F1F8ED93CC53481A4013E2F6C7BC90`；审阅r44 `188DAEC76365D428A8B032C57A60C2BA137781AD131BA40EBDE15B66226B2F8A`；任务包r46 `284E76EC5F0ABCF43A9592CFE3F674F9715E224A33C9B3CA94D413C44032BE13`。只增加SG02审阅／修正记录，不是业务重基线。 |
| 本包直接业务输入 | 效果r1 `3CFD66243A2846CF5ACE15BF21F88E7D6D8180481F44695CC2B588433BC3065D`；DOT r2 `CBD33BC51AF950ECCBA259721DAC364D002852587704F49C7BA9BAB884AF7ADD`；配置r3 `1AB07F8E5AFB1D2FDFC4A4A2CF5E436E8F876F1E4371A94A68CA9BFB25C4794B`；保存r3 `9AA10036F9615745035185A7887B0F2E73C94702CEF0A60CAF42B19BA059ECCF`；流程r1 `12BFC335B926D2530BAA4238B7D9E6B7906AC1DE534AFECD1EB5904310BF21BE`。 |
| 其余共同业务输入 | 成长r3 `E83BA93810C729579D1D817D356C8B59FD6999C066EAD478D83BDE918735524B`；库存r4 `7E5E6B72CCCDF0A577E66D448F78558C2505ACC2584856F14E7FF2ADAB145A40`；进度r2 `833C537686A1CB117989215064EF26B08836B3BE1DCDC9C8EF891E8E6422FA67`；权益r3 `B1AD295765A2C8E77A78F7913CF854E597457C271763E13C01BF9D3B312ED389`；评分r1 `5194DC775EEB2BF359604EB1966A20A51062C7C48BFEDFCE5C15601EC1931912`。 |
| 玩法与校准 | 主稿0.2 `5495AE36BC1E03CB42FFD428416D2C36855370935DFDA5769EDE28317393E82A`；三组稿 `1C86E32869355AD9EAD54F333EF3DF8FFB3238CEE658EFC92C2F37979D93F223`；H～K协调稿 `45D5CD3CBE5C0A80C295E98765F5D904FF3186E984A848B4CDF4BADAE0F94C08`；校准输入 `9664ED3B923E71720EA2D08ACE678E3E5033C4929EE809451576CB81A2984494`。 |
| 六份几何源码 | `FlowPathUtility` `AF6914555F3374965337C36816FA839888A412C2A58202A019BD743043661949`；`FlowSolutionValidator` `FB732BE5E11FCDF176A92D7219CE6B9894BFB9B8D976D6AE83EEB1C4E252B579`；`IFlowPuzzleSolver` `1BD6351715C929EB1127CB7B655095950C74044F4B470DF63885ACA27917F6D0`；Request `F15D7AAFB6F48784E9EB792C7709ED0566F64428FD61287CCD36B43D63477492`；Result `08D2B00045223128142E1150C2A59B927B517A7AC07D77A756B85C812AE9A95D`；Status `DCB4B5C55ECE0AF3B95F29D4FC95E46B9C9C07FC9F12F3000BB4EDF48D89A9DC`。 |

## 9. 最小内部关系、调用方法与方案选择

这些名称是M09内部资料／能力，不新增公共ID、程序集或持久服务；S14仍是唯一内容权威，M06仍是规则权威。

| 内部能力 | 最少输入与方法 | 输出／禁止 |
| --- | --- | --- |
| `AnalysisInputBuilder` | `Build(requestRecord, isolationInput, serviceView, searchScope)` | 形成不可变`SearchInput`或列首个缺项；不从UI、模型回答或当前配置补值。 |
| `CandidateGenerator` | `Propose(searchInput, proposalBudget, cancellation)` | 给有序`GuideStep[]`候选、来源及假设；AI或纯规则均无合法性权威。 |
| `SuccessorEnumerator` | `EnumerateAll(ruleState, declaredScope)` | 给该状态在声明动作空间内的全部后继意图及覆盖见证；启发式只能排序。 |
| `SharedRuleValidator` | `Validate(searchInput, steps)` | 在深副本逐步调用M06 `EvaluateAction`／`EvaluateLink`及适用结束能力，返回首个非法步／缺项或整关胜利见证。 |
| `SearchController` | `Run(root, generator, enumerator, budgets, cancellation)` | 管开放／闭合集、去重、停止与进度；不持业务锁、不提交玩家状态。 |
| `SearchEvidenceBuilder` | `RecordRoot`、`RecordEdge`、`CloseState`、`Finish` | 形成候选验证链或闭合证明；节点哈希、模型文字和“搜过很多”都不能单独作证明。 |

调用固定为：登记请求／隔离副本 → 构造完整输入 → 候选按序提出 → 共享规则逐步验证；候选失败或需要补搜时，由同一规则状态的完整后继枚举继续 → 得到结果证据 → 回到原§3～4按当前ContextKey接纳。候选生成、规则求值和接纳是三个完成对象。

| 至多两种方案 | 优点 | 限制与结论 |
| --- | --- | --- |
| A：联网AI提出／排序候选＋共享规则验证／搜索 | 保留产品的在线AI方向，先试高质量步骤，失败仍可用规则搜索补齐；模型可替换而不改胜负标准。 | **推荐。** AI不得改规则、资源或随机，也不得在证明模式剪掉合法后继；供应商、部署与模型均后置。 |
| B：仅共享规则完整枚举 | 最容易解释闭合证明，不依赖模型。 | RPG分支可能很大且真实上界未测；保留作本地基准／证明路径，不承诺任意局面即时完成。 |

## 10. 输入、合法动作与逐步获胜验证

`SearchInput`至少包含原§3全部ContextKey业务字段、M06 `BuildIsolationInput`深副本、EntryBaseline、当前及全部剩余面、已提交PreferenceRevision／ActionConditions、完整DefinitionBinding、Random各流状态／原始字计数／PRD累积、当前C／U与实际已取得资源、准确服务关系，以及本次`SearchScope`是否包含偏好调整、历史回退、重来和复活。取样CommitId／SaveGeneration、请求登记提交及搜索进度只作取证，不参与ContextKey相等；仅登记本请求、保存历史或接纳自己的服务投影不能让结果自致`Stale`。

合法后继按阶段生成：`AwaitAction`枚举每个可行动角色、合法目标、攻击形式和全部单步合法简单路线；合法偏好改变先作为显式步骤，不能暗开道具。`AwaitLinks`枚举每个Pending Pair可选择的顺序及全部合法免费补线路线；`AwaitRescue`枚举当前已有准确复活许可及合法结束选择。三类活动阶段都须向M06询问回退／重来：`AwaitAction`、`AwaitLinks`不得漏掉已有合法入口，`AwaitRescue`仅在M06仍报告合法时纳入；`WonPendingSettlement`／`Closed`不造回退或重来。

候选查找可以声明受限Scope以控制成本，但证明模式必须覆盖当前状态全部真实合法后继；故不能用Scope排除合法偏好、已获资源、回退或重来后仍称当前无解。全部步骤只在隔离状态求值，不申请、激活、撤回或消费真实服务。

`GuideStep`公共字段只有`StepKind、expectedStage、ActionConditions／相关修订`；其余按种类必填，非路线步骤不得伪造`PairId`：

| `StepKind` | 必填资料 |
| --- | --- |
| 攻击路线 | CharacterId、TargetId、PairId、完整route、攻击形式及资源份额。 |
| 免费补线 | Pending PairId、完整route；无CharacterId／TargetId。 |
| 偏好 | CharacterId、ItemId、明确`Enabled=true/false`、expectedPreferenceRevision及适用阶段。 |
| 回退 | 准确HistoryAnchorId、受影响范围／谱系依据、当前Attempt／SceneRevision。 |
| 重来 | 当前Attempt／Challenge、原EntryBaseline、冻结／准确Use撤回依据。 |
| 复活 | 准确Grant／Use许可、受益CharacterId、Attempt／阶段／绑定及资格修订。 |

物品只能用当前已获且副本中剩余的份额；复活在副本中至多按精确许可模拟。未来广告、未获道具、无限机会、换新随机流和关闭正式被动均不是合法后继。

验证从根副本依步骤顺序执行：每步先核身份／阶段／条件／资源，再调用M06同一规则；拒绝时返回`InvalidCandidate(stepIndex, reasonCode, missingInputs, lastValidState)`，不执行后步。全部步骤结束后必须已经覆盖所有剩余面、必要免费补线和资源／随机变化，并由M06达到整关`WonPendingSettlement`／完整隔离报告候选，才是`ValidatedWin`。完整几何解、当前面清空、第一面翻面成功、候选被保存或模型声称胜利均不足。

验证器没有玩家写句柄、SDK句柄、真实Use或H10能力；真实HP、SceneRevision、U、Random和历史保持不变。返回后仍按原§3用最新头重建业务ContextKey，并按§4同H10接纳；接纳自己的请求／内容／服务投影不是业务输入变化，提交前真实头再变则按既有过时／Busy路径重读。

## 11. 搜索等价、完整展开、预算与可靠无解

`RuleStateKey`只有在未来合法后继及其规则结果完全相同时才等价，至少纳入：绑定与整关／剩余面、阶段、棋盘／固化线／Pending Pair、全部敌我身份／原槽／HP／CD／状态／盾／意图／次数、C／U及资源许可、已提交偏好、全部Random／PRD状态与计数。证明模式还须纳入有效历史头、可回退锚点／被替代谱系中影响合法性的资料、原基线、准确Use撤回资格和任何次数／终止条件；候选模式即使不探索它们，也不能据此合并会产生不同真实合法后继的状态。

页面选择、动画、请求登记次数、搜索访问次数、纯显示计数及不影响规则的提交代际不进入`RuleStateKey`；但服务／资源修订、已用资格或历史若会改变合法后继则必须进入。有限棋盘不能证明RPG图有限：Random／PRD、能力计数、敌意图、资源、历史回退／重来及未给上界都可能扩大状态；不得凭格数宣布终止或无解。

搜索以根为开放集；每次按AI／启发式次序取状态。候选模式可按声明Scope取后继；证明模式的`EnumerateAll`必须覆盖**当前全部合法动作空间**，并给角色／目标／路线／阶段、显式偏好、已获资源及适用回退／重来的覆盖依据。每个后继都经共享规则求值；合法边记录完整前后态／动作／随机轨迹，非法意图记录稳定拒绝而不入图。找到整关胜利立即返回`ValidatedWin`；节点、规则求值、时间或证据容量达到预算分别返回`BudgetExceeded`／`Timeout`，取消返回`Cancelled`，缺原绑定／规则／完整枚举能力返回`UnsupportedBinding`或内部`Unsupported(reason)`，服务故障保持`ServiceUnavailable`。

开放集为空只证明所搜子图闭合；任一受限Scope闭合均只能记内部`ScopedClosureNoWin`／“受限候选搜索未找到”。只有证书另行证明子图等于当前ContextKey下全部合法后续，列全后继覆盖、每条可重放M06边、等价依据且无整关胜利，才升格`ProvenNoSolution`，再按原H09重核当前ContextKey并由H10接纳后计OP-R1交付。未来未获广告不属于当前资源；但不能排除一条现有偏好、已获资源、回退或重来获胜边。启发式剪枝、深度／预算、缺规则或不完整枚举一律非无解。

最小完整符号证书例：证据先证明`S0`当前全部合法后继恰为`a→S1, b→S1`，且无偏好／已获资源／回退／重来入口；两边可重放并得到同一完整Key，`S1`全部合法后继为空，`S0/S1`均非胜利，闭合集`{S0,S1}`才可升格。反例：攻击子图同样闭合，但遗漏合法`preference(on)→S2→Win`或已有复活边，只能记`ScopedClosureNoWin`；发现任一漏边、随机计数差异即使证书无效。

## 12. 几何复用、并发与缓存边界

| 实际源码 | 可复用事实／不能外推 |
| --- | --- |
| `FlowPathUtility` | 四邻接、Manhattan距离、步数／转角、重复格和绕行量纯几何；不含角色、目标、资源或胜负。 |
| `FlowSolutionValidator` | 对全部Pair检查端点、边界、相邻、自交、异色端点和路径重叠；要求每Pair有路径，但**不要求满盘，也不检查唯一解**。 |
| `IFlowPuzzleSolver` | 有进度与`CancellationToken`的完整几何求解入口；不是M06动作枚举器。 |
| `FlowSolveRequest` | 只带LevelData、fixedPrefixes、节点／时间／进度预算；没有RPG状态、Random、道具或历史。 |
| `FlowSolveResult` | 只有几何solution、访问节点与耗时；没有逐步战斗见证或闭合证明。 |
| `FlowSolveStatus` | `Solved/NoSolution/Timeout/Cancelled/InvalidInput/Error`只回答该几何请求；`NoSolution`不等于RPG `ProvenNoSolution`。 |

M13依赖方向保持纯几何：M06／M09可复用原语或完整几何诊断，M13不得引用职业、战斗或服务。SD10应组合发布绑定、几何证据和M06整关回放；若需稳定单步路线诊断，沿M06既有`SingleStepRouteResult`细化或向M13提纯几何请求，不能把RPG后继枚举塞回通用求解器。无满盘／无唯一解要求保持。

搜索只在深隔离副本运行，网络、AI和长搜索不持M02业务锁；取消、超时、预算耗尽和候选失败不新建Grant／Use、不耗新服务资格。旧回调始终定位原AnalysisRequest／ContextKey；新头只决定它能否当前接纳，不把旧候选改绑新请求。

可缓存的是不可变DefinitionView／几何诊断、原条件下的候选内容，以及以完整`RuleStateKey＋动作＋绑定`为键的纯转移；缓存命中仍须核执行能力和证据可读。AI内容缓存、服务器已收结果或相同内容哈希都不是当前服务交付；每次展示当前结果仍重核ContextKey并走H10。旧绑定缺执行能力返回`UnsupportedBinding`，不改用当前版本。节点、内存、延迟、并发及模型成本尚未测量，不填写性能数字或默认手机预算。

## 13. 搜索与验证手工文档见证

下列均为输入→输出→完成依据→重复／中断的设计推演；游戏、Flow／RPG求解、战斗、AI、保存和服务均 **NOT RUN**。

| 编号／输入 | 预期输出 | 完成依据；重复／中断 |
| --- | --- | --- |
| GS01 完整几何解，但第一步角色射程／技能条件不足 | `InvalidCandidate`指首个非法步骤；不是胜利或无解 | M06稳定拒绝，副本零变化；同候选重验同结果。 |
| GS02 第一面清空并补完，第二面候选缺失 | 只到翻面，非`ValidatedWin` | 剩余面未覆盖；补候选用新请求／原服务免费续办。 |
| GS03 战士目标仅技能可达，已学且CD0、目标合法 | 该步合法并记录嘲讽／新CD；后续仍须整关验证 | 与BC03同规则；保存／模型中断不改真实状态。 |
| GS04 两个Pending Pair可任意顺序免费补线 | 枚举两种顺序及各合法路线，补线不推进CD／DOT／PRD | 每步`EvaluateLink`可重放；取消保留根副本。 |
| GS05 已获复活许可与角色／阶段／绑定完整 | 可在副本模拟准确Use一次并继续搜索 | 许可及修订进Key，真实Grant／Use不变；旧许可回调不换靶。 |
| GS06 候选使用未获复活、未来广告或U之外道具 | 首个资源步骤`InvalidCandidate`／具体缺项 | 不向SDK申请、不借无限次数；换资源即新输入。 |
| GS07 完整规则可用但节点预算先耗尽 | `BudgetExceeded`，明确开放集及进度，非无解 | 扩预算沿同根重搜；不计OP-R1交付。 |
| GS08 只搜攻击／补线，未纳入合法回退／重来且没找到 | `ScopedClosureNoWin`／`Unsupported(IncompleteSuccessorSpace)` | 即使子图闭合也不得写当前无解或计OP-R1交付。 |
| GS09 证据证明符号图已覆盖当前全部合法后继且无胜利 | 当前键得`ProvenNoSolution`候选 | 根／全后继／等价／无胜利可重放；H09当前重核＋H10后才交付。 |
| GS10 搜索期间真实Random流或PRD累积改变 | 原结果`Stale`，只保存原条件历史 | 重建当前ContextKey；相同路线哈希不复用。 |
| GS11 搜索期间PreferenceRevision由开变关 | 原步骤条件失效；新请求按关重新验证 | 不静默开回；回退不恢复旧偏好。 |
| GS12 原请求Q1回调晚到，新Attempt Q2已登记 | Q1只归原请求／历史，不能当前接纳Q2 | 核Attempt／Scene／服务关系；重复旧回调仍同结果。 |
| GS13 验证完整获胜候选 | 返回`ValidatedWin`及逐步证据，真实HP／U／Random／SceneRevision不变 | 只有当前键＋H10接纳后计交付；CommitUnknown查原操作。 |
| GS14 原绑定缺敌方意图执行能力／搜索被取消 | 分别`UnsupportedBinding`／`Cancelled`，均保留资格 | 补回同绑定能力或新明确请求再做；不切新版本、不报无解。 |
| GS15 攻击子图闭合，但漏`偏好开→获胜`或已有复活边 | 只报`ScopedClosureNoWin`并列漏项，非`ProvenNoSolution` | 补齐全部合法后继后重建证书；旧闭合记录不交付。 |
| GS16 `AwaitLinks`先偏好关→补线→回退锚点→重来候选 | 各步按种类携准确字段，非路线步无PairId；全部在隔离副本求值 | 任一步不合法即返首个原因；不改真实偏好／历史／Attempt／Use。 |

## 14. SD10／SD12交接、具体缺口与验收映射

| 接续方 | 本包提供／对方仍须完成 |
| --- | --- |
| SD10默认参考 | 取得Published DefinitionBinding、完整入场假设、全部面、固定ActionConditions／资源／Random条件及完整步骤；以共享M06逐步回放到整关胜利，保存每步前后态、拒绝为零、最终报告候选与内容指纹。CandidateContext证据不能冒充发布；发布后的默认参考只保证其明示前提，永久免费。 |
| SD12输入表现 | 按StepKind交§10对应必填资料、顺序、ActionConditions及资源前提；非路线步骤不带伪PairId／路线。另给`Default/CurrentValidated/Historical`类别和失效原因（局面、偏好、资源、随机、绑定、服务关系）。显示不自动提交，不以缓存或动画升级结果。 |
| M08原交付接口 | 继续只接AnalysisRequest全覆盖、当前合格交付／失败／历史／未决／终止摘要及原Use引用；有效获胜／可靠无解仍按OP-R1，分类、资格、关闭和GP-R1均不改。 |

算法实施仍缺：M06可调用的完整RPG后继枚举及可证明等价键；每个生产敌人的意图／目标／参数、涉及多盾和死亡时点状态的准确表、火球唯一生产绑定及原Rule／Numeric／Random执行能力。缺哪项只让含该能力的候选／证明返回具体`Unsupported`，不阻本包Interface，也不要求完整火球整场回放或最终数值先完成。若要宣称可靠无解，还须对当前全部合法空间实际取得闭合证书；回退／重来、计数或资源上界未证明时不能预设有限。AI供应商／部署、隐私传输、真实预算、跨端确定性、H10 durability与远端信任也均待实现／测量。

| §57验收 | 本文落点与结论 |
| --- | --- |
| ①职责／调用／方案 | §9给六项最少能力、方法、调用链及两方案，推荐AI候选＋共享验证／搜索。 |
| ②合法动作空间 | §10覆盖角色／目标／路线／顺序、偏好、补线、已获资源、Random／PRD和回退Scope。 |
| ③获胜验证 | §10逐步同规则、首非法步／缺项及整关胜利；接纳沿原§3～4。 |
| ④搜索／预算／证明 | §11给等价、完整展开、停止、预算、取消、分类和闭合证书。 |
| ⑤几何边界 | §12逐份核六源码，保留无满盘／无唯一解及M13单向依赖。 |
| ⑥并发／版本／资源 | §12隔离、旧回调、缓存、旧绑定及不虚构性能数字。 |
| ⑦不少于10条见证 | §13共16条，保留原14条并补完整范围漏边与非路线序列。 |
| ⑧可接续输入 | §14交SD10／SD12／M08及准确缺口；不重问OP-R1／GP-R1。 |

本轮不建议修改共同契约：现有ContextKey、S14、H09／H10、OP-R1、GP-R1和M06隔离入口足够承载内部设计。代价是实现时必须保留完整状态键、后继覆盖与可重放证据；影响M09、M06、SD10／SD12，不改变M08分类或任何业务写入权。C1冻结S14按UTF-8无BOM、LF、不Trim的`## 2. `至`## 5. `前区间复核。**NOT RUN：** Unity、Flow／RPG算法、AI／模型、SDK／广告／在线服务、真实保存、跨端、性能与构建。
