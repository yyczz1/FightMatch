# CH1-IMPL-001 当前实施记录

2026-10-25。主程为交付负责人／组内唯一完成接收者；中央负责Git发布、草稿PR和里程碑，主测试负责独立GitHub审查与验收。状态：**执行中，尚未验收**。

## 授权、输入和路线

中央聊天`01a0e401-511d-79f2-b47f-3ab0ade1681b/local`本轮转交用户明确要求：“按这个设计继续实现，先做法师8／9关，再接16关教学和双阶段Boss……只补受影响的验证，代码审查继续交GitHub PR，不合并……普通步骤不要再等我确认。”此前仅设计边界已被本次实现授权替代，历史N07等次数／时限不复用。

采用已ACCEPT的[规则](../../game-design/content-planning/2026-10-25-ch1-slice-rules.md)、[工程](../../system-design/2026-10-25-ch1-slice/engineering-design.md)、[表结构](../../system-design/2026-10-25-ch1-slice/table-schema.md)与[17组验收](../../system-design/2026-10-25-ch1-slice/acceptance.md)。冻结SHA以验收第4节为准。已审产品基线`dece3d8c589742b5ee3d75b2e255c97f6bf2495b`，检查点`f94240fa72eb9df4e616fe04c56a1fe3068b73b1`，计划`1ae884bc`；中央已确认Core/Application/Content/Platform在前两者间无差异。本地master仍`125b849`且有旧WIP；只发布本任务准确差量，不整体提交工作区。

执行顺序：B01内容及必要保存基础→B02法师取得／9战斗贯通→B03教学→B04Boss。对同一包可按独立文件并行实现，交叉类型先直接对齐；实现者不自行扩大范围。普通修正沿本记录追加，不重复开设计／审批。涉及旧UI的前像须与已审输入单独核对后才能列入，不复制旧UI测试WIP进PR。

## 初始文件分工／白名单

下列路径以`Assets/Scripts/FightMatch/`为根，除明确“新增”外均修改现有文件；新`.cs`各自同名`.meta`一并允许。每位实现者先保存其实际前像／哈希至`TestArtifacts/FightMatch/CH1-IMPL-001/baseline/<角色>/`，不得覆盖现有前像。新增路径不存在也记入清单。需要其他路径先向主程报具体理由，主程在本记录更新后继续；无需用户例行批准。

| 负责人／分段 | 准确初始源码范围 | 新增必要测试范围 |
| --- | --- | --- |
| content，B01 | `Content/PublishedContentModels.cs`、`PublishedContentCompiler.cs`、`PublishedContentCodec.cs`、`PublishedContentCatalog.cs`；`Core/PublishedSaveContext.cs`；新增`Content/ChapterContentModels.cs`、`ChapterContentCompiler.cs`、`ChapterContentTables.cs`、`Core/ChapterRuleDefinitions.cs` | `Assets/Tests/EditMode/FightMatch/ChapterContentTests.cs`、`ChapterContentTestData.cs`及各自.meta |
| combat，B02计算基础 | `Core/BattleEntryInput.cs`、`BattleEntryPreparer.cs`、`PreparedBattleEntry.cs`、`BattleEntryBaseline.cs`、`BattleSnapshot.cs`、`BattleStartAssembler.cs`、`CandidateCharacterGrowth.cs`、`CandidateGrowthDefinition.cs`、`CandidateDirectAttack.cs`、`CandidateCombatFrame.cs`、`CandidateBattleOperations.cs`、`CandidateBattleOperationRecord.cs`、`CandidateBattleReportFingerprint.cs`、`CandidateBaseRewards.cs`、`CandidateRandomPreparer.cs`；新增`Core/CandidateChapterBattleRules.cs` | `Assets/Tests/EditMode/FightMatch/ChapterMageBattleTests.cs`、`ChapterBattleTestData.cs`及各自.meta |
| save，B01/B02保存基础 | `Core/CandidateBusinessSnapshot.cs`、`CandidateBusinessSaveCodec.cs`、`CandidateBusinessSaveValues.cs`、`CandidateBattleSaveCodec.cs`、`CandidateRosterSaveCodec.cs`、`CandidatePermanentSaveCodec.cs`、`CandidatePermanentCodec.cs`、`CandidateApplicationIntent.cs`、`CandidateApplicationIntentCodec.cs`、`CandidateApplicationSaveCodec.cs`、`CandidateBusinessRestoreChecks.cs`、`CandidateApplicationProtocol.cs`、`CandidateApplicationRecords.cs`、`CandidateApplicationReferences.cs`；新增`Core/CandidateChapterSaveCodec.cs` | `Assets/Tests/EditMode/FightMatch/ChapterSaveCompatibilityTests.cs`及.meta |
| 主程，B02业务集成 | `Application/CandidateLifecycleEnd.cs`、`CandidateLifecycleRoster.cs`、`CandidateLifecycleEntry.cs`、`CandidateLifecycleModels.cs`、`CandidateLifecyclePreparation.cs`、`CandidateLifecycleApplicationSystem.cs`、`PlayerRosterSession.cs`、`PlayerSessionSystem.cs`；`Core/CandidateRosterState.cs`、`CandidateProgressionState.cs`、`CandidateProgression.cs`、`CandidateInventoryState.cs`、`CandidateInventory.cs`、`CandidatePermanentGrowth.cs`；新增`Core/CandidateChapterMilestones.cs`、`Application/CandidateLifecycleChapter.cs` | `Assets/Tests/EditMode/FightMatch/ChapterMageLifecycleTests.cs`及.meta |

其他路径只读。作者表／正常内容闭包由主策先给本任务数据依据和具体文件清单，随后登记；不能把样例缺7/15闭包当成生产包。B03/B04准确增量在各自启动前列入；已有接口先复用，不提前实施火球、DOT或后续职业。冻结设计文档不在本次修改范围。

## 验收与验证预算

| 包 | 完成要求／受影响检查 | 执行资源 |
| --- | --- | --- |
| B01 | Schema1仍能精确解析；Schema2的类型、引用、坐标、有理数和能力拒绝；V5可保存必要新状态，原格式/原绑定/只读历史不漂移。对应D1—D4 | 先源码静态核；必要编译及定向EditMode交唯一C。预计每调用3—10分钟，单调用上限900秒，B01+B02累计主动验证上限3600秒，包含普通修正；超预期先诊断，不盲重跑 |
| B02 | 8联合结算只固定原参与者XP并唯一取得法师、首通卡和开9，Formation不自动变；9实际普通魔攻／穿透／范围／随机及恢复正确。对应A1/A2及受影响D项 | 同上预算；固定输入后只跑新增/受影响检查，保留失败与实际耗时；不重跑旧L1完整验收 |
| B03 | 赠证→不足级回刷→合法学习→独立说明→入场，重复／未知不重扣发；T1—T5 | 启动时根据已实现接线选择必要自动化／实际观察并列适配预算 |
| B04 | 两面周期／上弦／嘲讽CD／易伤／补线／跨面恢复／唯一终局；C组 | 启动时列准确源码与受影响检查及预算；不沿用B01/B02剩余额度冒充新授权 |

本轮工具发现未提供可用dots测试能力；向QA核现有就绪环境，不为此安装或重建云环境。无可用dots时沿新授权由原C `01a14e81-4917-7440-b0db-b790ea4ddcde/local`串行本机验证；先核Unity占用，不与打开的Editor并跑batch、不强退其他应用。保留外置缓存，不安装升级依赖／服务，不清玩家档。运行结果未知/失败如实保留；没有独立GitHub结果不能自批完成。

## 发布与收件

每个连贯差量冻结准确文件/输入、原始验证证据及PR描述后交中央，用其独立索引向新`codex/ch1-*`草稿PR发布，base沿已审`dece3d8c`；不改旧PR22、不写普通索引、不合并。QA唯一接收GitHub head对应结果并回主程修复；等待期间继续独立非冲突工作。主程只在发布候选／里程碑／整体阻塞时回中央。

当前：已完成只读接续与旧WIP识别；准备并行B01内容、保存与B02计算，主程接联合结算。各实际执行身份、增补白名单和结果随本记录追加。

### 实施启动及范围增补

- 实际subagent：`/root/ch1_content`、`/root/ch1_combat`、`/root/ch1_save`，均显式请求`gpt-6-astra/xhigh`、短上下文且无下层代理；主程保留业务集成。仅组内实现协作，不是独立代码审查。
- combat增补现有`Assets/Scripts/FightMatch/Core/BattleDamageFact.cs`、`CandidateEnemyPhase.cs`、`CandidateBattleStage.cs`：物理／Crit事实和阶段校验含战士专用假设，需类型化普通魔攻／穿透事实及新分支校验，保留旧路径。
- save增补现有`Assets/Scripts/FightMatch/Core/CandidatePermanentProtocol.cs`、`CandidateRosterProtocol.cs`、`CandidatePermanentInventory.cs`：V5支持及首次通关物品的原来源核验，不得只增背包数量。
- 主策独占新增`Config/FightMatch/ChapterOneTrial/`内`Release.csv`、`CharacterGrowth.csv`、`Passive.csv`、`Level.csv`、`Face.csv`、`PairEnemy.csv`、`EnemyAction.csv`、`DefaultRouteCell.csv`、`Item.csv`、`Card.csv`、`Skill.csv`、`Teaching.csv`、`Reward.csv`、`RewardMaterial.csv`、`CharacterUnlock.csv`、`FirstClearItem.csv`、`SourceMetadata.csv`、`README.md`。仅试调作者数据/来源说明；不修改原L1 JSON或已发布定义。正常内容闭包分1—9、10—16接入，无测试跳关入口。
- 主策读取旧`Assets/FightMatchContent/demo-r1.source.json`的L1真实输入；新章包包含其L1数据的明示副本用于正常从1开局，LevelId仍`level:ch01-01`，但由新包的ContentBinding隔离，不冒称新包L1就是已验旧绑定。旧源文件、旧发布集合、旧档/活动局绑定不改。普通E05需要的`LastLegalLiving`作为既有后排规则的有限枚举增补，不以PreferLivingMage替代；本切片阶段能力未就绪前不将含它的完整包启用。
- QA已接收并在其`validation.md`维护最小验证映射；目前dots旧回执未证明当前就绪，QA仅做一次只读现状核对。本地C等待固定候选，不据此启动Unity。
- 职责交接：按中央本轮要求，主程保持设计／分发／接收，不直接写产品。初始表中“主程业务集成”的Core范围转`/root/ch1_save`（含新增`CandidateChapterMilestones.cs/.meta`及`CandidatePermanentProgression.cs`的状态保留）；Application范围与`ChapterMageLifecycleTests`留给首个空出的短C，再登记实际接管。主程只完成14文件前像保存，尚无本轮产品写入；baseline/root已保存前像可直接复用，不重复覆盖。
- `ChapterContentTables.Read(directory, PublishedSource initialProfileAndProofs)`允许只读原demo-r1作为已验证新档recipe及31条PRD精确参数的生成种子；不从旧玩家存档建新档，不改旧源，不手写新的近似C。SourceMetadata采用`path,locator,sha256`三列真实来源。
- QA最新dots只读调用在执行前即失败`No usable execution environment is available.`，当前HEAD/依赖/许可检查均NOT_RUN；不声称复现旧proxy错误。本机C继续准备，不等待云环境修复。
- save增补`Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs`：Mage的真实空战术槽／携带参与者需通过Actor校验，由有Context的调用入口限制到新规则；其他职业和旧规则支持域保留。
- 后续Application接管范围增补`Assets/Scripts/FightMatch/Application/CandidateLifecyclePermanent.cs`与`PlayerPermanentSession.cs`：必须保留Chapter definitions并按实际格式提交永久操作，不能在V5局发固定V4意图。教学具体UI仍另列范围。
- save增补`Assets/Scripts/FightMatch/Core/PlayerProfileCreateRecord.cs`：当前新档原始办理记录只接RecordFormat1/2（intent2/3），V5显式新建需保留旧格式并增加准确RecordFormat4/intent5支持和恢复，不把未识别格式重建为新档。
- Trial解释收口：新用户授权明确允许Trial作为开发候选。冻结设计中“样表不可发布”针对缺闭包／证明的三关片段，不能扩成完整开发Trial集合永久不可进入产品。完整作者闭包、实际验证及真实审查证据满足原Catalog门时可发布仍标Trial的开发集合；不改成Approved、不伪造独审、不声称正式平衡／发行批准。
- 唯一Unity C已在actual `01a170cd-6f7e-7b90-89e2-b14148c18ada/local`只读确认：2022.3.18f1 Intel可执行，无Unity/Hub进程和Lockfile，现有Test Framework／EditMode入口可用，外置Library/PackageCache/Bee保持；运行次数0，等待固定候选。
- Application职责接管：`/root/ch1_combat`在当前计算静态收敛后继续接初始表中全部Application白名单、上述LifecyclePermanent／PlayerPermanentSession及`ChapterMageLifecycleTests.cs/.meta`；由同一短C贯通入场／正常结算，不等一次空闲审批。root继续仅协调收件。
- combat增补`Assets/Scripts/FightMatch/Core/CandidateHistoryOperations.cs`：历史验证现假定Crit非空且仅Physical，需在新规则分支核同一被动／魔攻事实以支持真实回退复播，历史协议不重写。
- combat一并增补`Assets/Scripts/FightMatch/Core/CandidateReplayOperations.cs`、`CandidateReplayComparison.cs`：复播验证／对照须覆盖Penetration并保留旧Crit差异判断，属于A2原随机／回退兼容，不扩展表现功能。
- 主策r1实际作者输入已固定：17 CSV集合SHA `78f14252d2a895deb4abe0921012ffde8c49620c513016491c7ce2c265aaa0cb`（按排序文件名:SHA换行）；18路径副本及manifest在`TestArtifacts/FightMatch/CH1-IMPL-001/b01-b02/inputs/ChapterOneTrial-r1/`及同级`chapter-one-r1-manifest.json`。9关／9面／24敌／4意图／96路线格；旧L1源SHA `fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a`保持。当前作者目录暂不递增revision，先完成本轮固定输入验证；10—16只读准备。
- combat/Application增补`Assets/Scripts/FightMatch/Application/CandidateBattleRequestFactory.cs`：Attack／Link／Rollback的实际意图构造必须沿已校验head传V5格式，防新战斗动作落入旧schema。
- 同属combat增补`Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs`：在已验证head与commit匹配时将实际V5格式传给RequestFactory，旧V3/V4编码行为保持。
- 内容生成接线由content拥有新增`Config/FightMatch/Luban/ChapterOneTrial/luban.conf`、`schema.xml`，如现标准CSV加载器需标记行则新增必要薄适配`Tools/FightMatch.ChapterContent/prepare_luban.py`（仅补Luban标记／schema，不承担业务生成或验收框架）。正式链必须调用现有外置Luban5.1.0；consumer读取其17个同名JSON数组，所有单元格保留string以保证BigInteger／Q无float途径，再做既有类型／引用／规则校验。作者17CSV只用于输入／SHA，不能绕过生成输出赋业务值。
- 生成输出白名单：`Generated/FightMatch/ChapterOneTrial/r1/{Release,CharacterGrowth,Passive,Level,Face,PairEnemy,EnemyAction,DefaultRouteCell,Item,Card,Skill,Teaching,Reward,RewardMaterial,CharacterUnlock,FirstClearItem,SourceMetadata}.json`，由唯一执行C运行标准Luban产生，禁止手工填生成值。准备阶段仅在`TestArtifacts/FightMatch/CH1-IMPL-001/b01-b02/luban/`存放临时标记CSV／日志／原始证据。此生成预计<30秒、单调用上限120秒、累计含普通修正上限600秒；与Unity主动3600秒分开记录，不重建工具链或联网下载。
- combat增补`Assets/Scripts/FightMatch/Application/PlayerSessionModels.cs`：`PlayerRosterView.IsMaterialized`须识别V5，使用本轮Lifecycle用例确认真实QueryRoster，不改变旧格式判断。
- content接管本轮最小表现兼容：`Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs`、`CandidateBattlePlaybackFrame.cs`，新增`Assets/Tests/EditMode/FightMatch/ChapterMagePlaybackTests.cs/.meta`。只消除法师普通命中无Crit时的反馈空引用，沿现有本地化普通命中及既有表现结构；不将穿透误称暴击，不新增布局／场景／资源。两既有路径前像须核已审dece3d8c，再记录差量；同轮使用unity:ui-ugui技能与现有uGUI纠正要求。
- save候选已回传28路径，测试13实例均未运行；真实V4→V5定义映射仍缺，当前明确拒绝`MigrationMappingRequired`，不据此声称完整D4完成。`CandidatePermanentProgression.cs`前像系编辑后反向重建，manifest已标注；保留此程序偏差，不冒称事前捕获。后续可与只读不可变Git基线核对以证明字节来源。
- Luban生成已交原C，仅两条顺序命令及17JSON；此刻尚无生成／Unity测试成功证据，产品作者继续完成静态接线后统一停写冻结。
- r1生成回执：C actual `01a170da-5c46-7f11-a930-f2906ebd0c00/local`，两命令各一次exit0，合计3.436107秒；17JSON／181行／1547单元格与冻结CSV精确一致（含103空串），[原始回执](../../../TestArtifacts/FightMatch/CH1-IMPL-001/b01-b02/luban/result.md)。含输入核对及收尾178.967／600秒，Unity仍0。
- 首次源码冻结：89本轮差量、517执行输入，`b01-b02/source-freeze-01.json` SHA `81185a93e7e0ab57ce3ade82ca980cec18c194f261516c1d8c67f90c9a4874c1`；content／save／combat全部停写。C派工`editmode-01`采用冻结r1，新增55实例与旧相关边界15实例，共预计70，标准EditMode无图形、不带quit。主程原filter误写不存在的暴击测试名，已改为真实`CertainCrit_UsesOriginalMultiplierAndZeroWordsInEitherRouteDirection`；如已启动则保留未匹配记录，不为此中断或盲重跑。
- 实际资源格式要求`businessReleaseSetId`以`release-set:`开头。主策actual `01a170dd-48fa-7011-8842-c01a68b4c652/local`仅修`Release.csv`为`release-set:chapter-one-trial`、revision2及README；其余16CSV字节与r1一致。r2的18文件另冻于`b01-b02/inputs/ChapterOneTrial-r2/`，CSV集合SHA `5faccf6b422ac9348a46a19d7783edb369949dc86b304e71bccad9b83ad29f19`；旧r1副本和生成不覆盖。待首次测试结束后才修改生成配置／用例并按新输入重生成。
- 存量迁移边界收口：工程§7明确首次优先显式新V5；D4真实迁移属于后续显式授权检查。本轮保持`MigrationMappingRequired`，不开放玩家迁移入口，不阻新V5正常8／9链，未实施部分不标通过。前像补证只读Git验证`CandidatePermanentProgression.cs`重建前像与dece3d8c逐字节一致，10113字节／SHA `7449a18b1b954210f6737317adc175ba8eb3f1973587e20f3197b4ce99eb9c64`；原偏差记录保留。
- 中央按冻结工程§7定稿本轮开发／验收Host入口细化：启动显式“继续原Demo资料／第一章试行”，两个固定内容集各有独立locator、profiles及resource-state根；旧根与旧绑定保留，第一章显式新建或继续本地V5，同一Host且一次一个QFramework owner，语言偏好沿原共享路径。此为保护旧档的开发入口，不扩通用多账号／多档，不增加存档协议或自动迁移。准确Host／资源／uGUI／文本白名单在执行前另列；当前未写入这些路径。
- `editmode-01`首次Unity已退出：`CandidateApplicationReferences.cs`的201／260／493／526行`busines(int)s.Format`导致10条编译语法诊断，测试未发现／未执行、无XML，全部执行输入哈希保持。原作者save按既有范围修正，原失败完整保留；不归为Unity拒绝或测试通过。
- r2生成增量准确范围：content修改既有`Config/FightMatch/Luban/ChapterOneTrial/luban.conf`的临时输入为`b01-b02/luban/staging-r2`，既有`ChapterContentTests.cs`正式数据改读`Config/FightMatch/ChapterOneTrial/`及`Generated/FightMatch/ChapterOneTrial/r2/`，避免干净checkout依赖本机历史证据目录。C新增生成`Generated/FightMatch/ChapterOneTrial/r2/`下与r1相同17个白名单JSON；薄适配输入取已冻r2，日志／回执在`b01-b02/luban-r2/`，累计仍受原600秒约束，不覆盖r1生成或staging。此输入纠正与编译修正合并下一次必要验证。
- r2测试准备修正：真实source revision2必须使用同revision的`DemoContentDraft` job；content在已报停写后追加了该测试准备，造成一次SHA回执交叉。C在Unity启动前核到并暂停，主程确认最终SHA `fa260f9d3f4698d5acc042f55d8e2faadad87cbb63f8c6eaa1cf334304a100d3`后继续原第二轮合同，不重置预算；候选其余仅原定保存语法／Luban配置变化。后续作者停写后发现新问题先回主程，不自行恢复写入。
- 主策文本范围增补：独占`Config/FightMatch/Localization/FightMatchText.csv`，仅本章开发入口、法师／两种新敌／经验卡名、地图状态／前置、首通角色提示与穿透反馈15个新key，及既有Warrior名称的W-only说明纠正；英简中参数一致、旧264键文本保持。准确key随本段派工，不手改任何Generated/资源/源码；C当前核心编译输入不含该作者CSV，可独立完成。新文本后续生成独立CH1产物，旧264键产物／旧资源plan保持字节不变。
- 第二轮回执：r2标准生成17JSON／1547单元格一致，生成累计270.143287／600秒。`editmode-02`Unity一次28.234秒退出，35条错误全为新增测试的`Input`、`Content`、`Is`名称遮蔽；70例未执行、无XML，89候选及553执行输入未变。四测试文件已由各原作者仅作名称限定、保留断言。此前自写输入清单比较器误将source JSON当C#／asmdef，修正后运行；该前检偏差保留，后续以准确本轮差量SHA／实际r2输入和标准Unity日志/XML核对，不继续扩建全库前置检查。
- 唯一Unity执行职责交接：原C `01a14e81-4917-7440-b0db-b790ea4ddcde/local`在actual `01a170e4-a4fa-7bd2-9ac5-2db761203f71`完成后已idle且Unity退出，主程已明确通知停止后续执行。因其发生历史上下文回摆，按中央认可的团队管理转短上下文subagent `/root/ch1_executor`（本主程聊天/local）；先核无占用后串行执行，其他实现者停写。沿用生成270.143287／600秒、B01/B02主动验证457.079／3600秒及两次Unity失败记录，不重置额度、不复跑旧就绪或旧N07。
- 主策文本已交：279键，新增15键；旧264键正文／参数不变，其中263行完全不变，Warrior行仅两项说明元数据纠正。作者CSV SHA `3a47be4b0efbddeccf185aea28be9336f4ed1a1fb755a294d0e3b224800dde35`，前像与核对在`baseline/planning-text/`；不声称旧264完整行全部相同。
- content可与当前Unity检查独立实施文本编译薄入口：修改`Tools/FightMatch.Localization.Compiler/Program.cs`（当前与dece3d8c无差异，仅partial/Main新分支、两解析方法默认264的数量参数），新增`Tools/FightMatch.Localization.Compiler/ChapterOneCompile.cs`。复用现有解析／一一对应／规范化／往返函数，新模式`compile-chapter-one`固定279键及已批准新输入，独立输出真实来源manifest；旧模式与旧manifest保持，不调用历史generate/self-test/negative/验收runner。本阶段仅Tools两文件及前像，Unity的Assets／既定候选／测试均继续停写。
- 文本生成准确输出预定：Luban原始JSON在`TestArtifacts/FightMatch/CH1-IMPL-001/host-text/luban-r2/fm-text-v1.json`；标准编译生成`Generated/FightMatch/Localization/ChapterOneTrial-r2/fm-text-v1.json`、`fm-text-v1.manifest.json`；后续C仅复制至`Assets/FightMatch/ResourceBuild/Inputs/ChapterOneTrial-r2/`下同名二件并生成同名两.meta及目录.meta。旧Generated、旧导入pair与旧plan保持，只在收到实际生成哈希后接CH1 pin；当前未运行这些生成／导入。
- 中断续接：主程及短C回合被中断，原Unity PID61965成为launchd子进程继续执行，未重复启动。通过专用collaboration恢复同一`/root/ch1_executor`（actual thread `01a170eb-eb44-73c0-b1e4-8b9f9dc7c53e/local`），接管原02:54:27.783 UTC截止。中央一次3秒sample只显示活跃托管栈，未能定位具体用例，未据此声称死锁；最终原进程在期限内自然退出。
- `editmode-03`已完成：编译成功，XML70例=69通过／1失败／0跳过；全部553固定输入后核一致。唯一失败为`ChapterMageUsesMagicDefenseAndRetainsThreeDistinctDamageValues`把2464/105与规范化352/15逐分子分母比较，原作者仅修该测试期望，产品逻辑和其他断言不变，后续只补该1例。XML总338.281秒，其中Lifecycle319.381秒解释长CPU段；因中断丢失Popen句柄，退出码如实unknown，XML／退出观察分别记录。完整证据在`b01-b02/editmode-03/`；累计主动验证1125.181／3600秒（含明确保守准备120秒），生成270.143287／600秒。
- 首个可审快照已交中央：`TestArtifacts/FightMatch/CH1-IMPL-001/review-candidate-01/`，128路径（89本轮实现、18作者r2、17生成r2、4冻结设计），manifest SHA `f9cd83b9f68ec6126ad67c786780de3985b096351f14dc71a0184eaa5fdd8694`。仅多含最后1行规范化期望修正，原69通过复用、1例补跑中；排除后续文本／Tools／Host／旧WIP。中央从快照发布draft，QA接准确head；不声称Host或整章验收。

### B02实际入口与显示增量（单例补跑退出后开写）

沿中央已定的两个固定开发入口与已有uGUI组件实施，不增通用多档协议。原Demo入口继续原资料／资源；第一章入口用固定独立根，选择前没有业务Session或资源激活；共享语言偏好，重复／过期点击不重复创建。完整设计、真实内容审查和原存档语义保持。

- combat独占Host：`Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs`、`FightMatchPlayerHost.cs`、`FightMatchHostView.cs`、`FightMatchResourceBridge.cs`、`FightMatchResourceActivationStore.cs`。必要时同组`FightMatchLocalizationCatalogParser.cs`、`FightMatchLocalizedTextSourceAdapter.cs`仅用于固定279文本读取；既有三.meta保持不动。复用Startup既有按钮，不改RuntimeRoot.prefab。`Assets/Scenes/FightMatchDemo.unity`仅在真实生成哈希／GUID交付后接共享文本与平台CH1 boot引用；不填虚假pin。新增`Assets/Tests/EditMode/FightMatchHost/FightMatchChapterEntryTests.cs/.meta`；必要测试输入适配仅`FightMatchHostTestFixture.cs`及`FightMatchResourceActivationStoreTests.cs`。测试两根／原绑定、未知恢复、单owner、共享偏好与选择前无激活；不假称实际发布包已验。
- save接管Presentation本次显示：`Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs`、`CandidateBattlePlaybackView.cs`、`PlayerBattleView.cs`、`PlayerDefaultReferenceView.cs`、`PlayerNavigationView.cs`、`PlayerNavigationRecoveryView.cs`、`PlayerPermanentDetailView.cs`；`Application/PlayerNavigationModels.cs`、`PlayerNavigationQuery.cs`仅新增同head精确Chapter只读展示。新增`Assets/Tests/EditMode/FightMatch/ChapterPresentationTests.cs/.meta`。角色名取权威Class／当前Roster／原receipt参与者；映射1—9关、两新敌和经验卡，保留未知诊断；首通预告与实际取得项来自定义和本operation效果，不按关号猜、不重复算普通奖励。BattleText此前由content的两空引用修正保留，本段起其文件职责明确转save。使用uGUI技能，既有控件／占位结构不重建。
- content独占发布／资源：新增`Assets/Scripts/FightMatch/Content/ChapterContentAuthoring.cs/.meta`及`Assets/Tests/EditMode/FightMatch/ChapterContentAuthoringTests.cs/.meta`，固定r2/真实seed、复用Prepare／Publish／Catalog精确审查门，准备状态为UNREVIEWED并如实`BattleReplayPerformed=false`；真实审查回执前不制造ACCEPT。修改`Assets/Scripts/FightMatch/YooAssetAdapter/Editor/FightMatchResourceBuild.cs`、`Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs`，仅保留旧固定包及新增固定`FightMatchChapterOne`第二包，映射／描述器／rule／release-set严格一致。新增`Assets/FightMatch/ResourceBuild/FightMatchChapterOneResourcePlan.json/.meta`，实际文本哈希到齐后才填；测试限`Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceBuildTests.cs`、`FightMatchResourceReleaseSetTests.cs`。ResourceBridge由combat所有，不共享写。
- 以上新.cs同名meta允许创建；现有文件先保存本轮前像，核已审dece3d8c及core-review快照的差量，保留旧WIP。源脚本／测试收敛后再给唯一C必要编译、已有工具生成和页面检查；作者不得并行启动Unity。六件实际发布输出、平台资源build产物、预算在执行前按真实入口和manifest另列，不以此段提前运行／激活。
- 显示相关测试增补转save：`Assets/Tests/EditMode/FightMatch/ChapterMagePlaybackTests.cs`仅把Warrior显示用例的合成ClassId设为真实`class:warrior`，以配合权威class映射；原断言及Mage未知诊断保留。此是新显示差量，后续复验受影响3个播放实例，不重跑本批全部70。
- 首稿已发布[PR24](https://github.com/yyczz1/FightMatch/pull/24)，head `e642d337ca2d8f8d5cf0307ff263eed4771f7134`，base dece3d8c，draft不合并；中央使用快照128路径及7份必要现成上下文，共135路径，QA唯一接GitHub审查。补验`editmode-04`唯一例1/1通过、exit0、31.700秒；与03原69例合并覆盖70项，非一次新全跑。验证累计1197.368362／3600秒。
- 279文本标准生成已完成：dotnet构建0警告0错误、Luban、新投影均各一次exit0。新artifact128809字节／SHA `038a9fed918495f04bb259ab48bf157d55df0e1a81a37e649dd7469c1953177c`；manifest906字节／SHA `4072b5f149e609b8fdb810ebc9c1fb46d5bda6374bf77a12d872188ce091a8b5`。旧pair／plan未变，新文案未据此声称已独审／视觉通过；准确证据`host-text/result.json`。生成累计394.433／600秒，余205.567秒，不重置。
- **首批基础集成判定：ACCEPT，仅PR24 head `e642d337ca2d8f8d5cf0307ff263eed4771f7134`的B01/B02内容、法师业务／数值及存档基础。** QA actual `01a170fc-5d34-7970-a4db-d5d4b6b2c054/local`已核快照、XML与原始[GitHub独审](https://github.com/yyczz1/FightMatch/pull/24#issuecomment-6392713312)；主程亦只读核服务摘要完整head相同、completed且finding／未解决finding均false，未追加审查或运行。采用69＋定向1证据，PR仍draft未合并。该判定不包括真实表平衡可玩、Host／279文本／资源包／页面／设备、真实V4→V5映射或B03/B04，整轮仍执行中。
- 发布准备的C输出准确叶名登记：`TestArtifacts/FightMatch/CH1-IMPL-001/host-content/r2/prepare/`下`source-copy.json`、`prepared.fmpackage.bytes`、`prepared.fmvalidation.bytes`、`prepared-descriptor.json`、`field-map.json`、`chapter-evidence.json`、`review-template.json`、`result.json`。程序输入的真实审查文件固定`host-content/r2/approved-review.json`，当前不存在，须由QA在准确准备产物／源head对应后提供；作者／生成器不能自填ACCEPT。后续publication存储及六件导出仍待独立执行合同，本段仅允许准备接口与输出设计，不启动Unity。
- Host第一轮源码已固定：`host/source-freeze-01.json` SHA `f060a40b8ceef4dd38b22f65f8790f0a9e666808c197256f72cbe57adea1cc11`，36本轮路径／91直接输入。实际3作者均停写；测试选择新增／受影响40实例及旧Bridge5实例，共预计45，不重跑已接收70。C标准EditMode带图形设备（不带nographics／quit），预计2—8分钟、单次900秒；沿原B01/B02 Unity累计1197.368362／3600秒，不重置。若编译及45例通过，再由同一C标准executeMethod做一次上述8件prepare（不publish／activate／资源build／Scene），预计<60秒、单次180秒，同计3600秒。Luban／文本工具生成394.433／600秒保持。两个Unity调用均使用标准进程级timeout；失败先报原始诊断，不自主修复重跑。
- Host增量实际结果：`host/editmode-01`因新测试的`MemorySave`未限定类型而编译失败，未执行45项；save只改为`Data.MemorySave`后形成`source-freeze-02.json`。`host/editmode-02`编译成功、45项中36通过／9失败／0跳过，exit2，Unity调用52.841580秒，91输入保持；prepare未启动。累计主动验证1356.268451／3600秒，生成仍394.432760／600秒。原始失败、日志和XML保留。
- 本轮9失败分为三处，交原作者定点修正：authoring四项在准备序列化时触发旧单角色`Growth`歧义守卫；Presentation四项因测试临时TMP销毁前未解绑，随后切语言调用已销毁对象；Host一项为启动按钮可见性断言。已收到save的测试清理修正，仅销毁前显式`Unbind()`，产品与断言保持，文件SHA `4f1f732c3c241f4f5a76f6f617a00f6593a3f2f93c67dd109a50095e1be3680a`。另外两处收齐、冻结后只补这9项及确有影响的检查；原36通过证据复用，不将失败改写为通过。
- 三处修正收齐并停写：content仅将新field-map导出改为显式多职业DTO，并加真实两Growth／旧单Growth仍拒绝歧义断言；combat核实Prefab根默认不激活、真实Scene有激活覆写，测试实例补激活并加强`activeInHierarchy`断言，产品按钮／epoch代码未改。`host/source-freeze-03.json` SHA `44d8098080a3d00090f5d11af150dcc8310139068544cbfb7c7c3f8e3ee7b51f`仍为36候选／91输入，仅四个申报文件改变。C下一次`host/editmode-03`只选Authoring四项、Presentation四项及`StartupReusesBothButtonsAndRejectsRepeatedOrStaleSelectionCallbacks`一项，预计1—3分钟、单调用900秒，累计沿1356.268451／3600秒。九项通过后，同一C才按既有合同执行一次prepare（180秒）；其他输出／操作范围保持。
- Host补验及实际准备完成：`host/editmode-03`为9/9通过、0跳过、exit0、50.870888秒；同02原36通过组合覆盖45项，非新一次全跑。`host/prepare-01`仅一次prepare，exit0、16.171017秒，准确8件输出且91输入保持；9关几何／31参数行，模板UNREVIEWED、`BattleReplayPerformed=false`，未发布或激活。实际source／payload均44332字节、SHA `3127954bccf58fee33f983c6a5c37a307f282da5d6d66ab48fc687596cf25544`，validation1046字节、SHA `c68acb21e0e42022bc7ebda1ee3045d2e9e66ac7746ab5121ec33ce2adec7d12`。验证累计1503.723548／3600秒，生成394.432760／600秒；两个Unity进程已退出。
- 第二个准确审查快照：`review-candidate-02/`，36文件、base `e642d337ca2d8f8d5cf0307ff263eed4771f7134`，manifest SHA `a30d21de9185fca43bac0d5a19398de777b9f2dfc554f53cc09dc8200055d49e`。涵盖Host／显示／279文本工具及生成物／发布准备与资源接线源码，排除Scene、六件实际发布、原始资源产物、设备与B03/B04。主程核当前91输入与快照、8件prepare哈希一致；原首批128路径中仅两项本轮明列显示差量变化。交中央发布差量draft PR、不合并；QA接准确head及实际内容待审记录。
- 中央已发布并核远端[PR25](https://github.com/yyczz1/FightMatch/pull/25)，head `1e87c7960fc429b7229862c720520ed9ea42e207`、base `e642d337ca2d8f8d5cf0307ff263eed4771f7134`，draft未合并；37路径为准确36文件快照加当时更新的本实施记录。主程已挂接，QA独占首次GitHub审查请求／收件；当前未据此认定源码门或完整B02通过。

### B02实际发布、资源及页面接续

主程继续沿本轮已授权实现／相关验证范围组织；唯一C仍为`/root/ch1_executor`。下述执行须先收到QA对准确prepare产物的真实`approved-review.json`及对应源码独审收件；文件未具备时不执行发布，不由作者替填审查身份。C每步成功后继续下一依赖步骤，失败保留原证据后回主程，不自动重试。沿现有1503.723548／3600秒主动验证预算，不另发或重置额度。

执行前提已具备：PR25 head `1e87c7960fc429b7229862c720520ed9ea42e207`的[原始GitHub独审](https://github.com/yyczz1/FightMatch/pull/25#issuecomment-6393017173)未发现重大问题，服务摘要6393002000完整head匹配且completed／finding和unresolved均false，主程已只读核原返回。QA actual `01a1711b-6465-7f33-b9ee-8d302cdcf729/local`签发真实`host-content/r2/approved-review.json`：4636字节、SHA `ec3398e4c0732c6f1a52ca6a60ad71e1090d4f837ea77d30c0a19df6611cb175`，独立身份、source-freeze-03、field-map、source/payload/validation均精确绑定。原prepare模板不变，未追认战斗回放或实际试玩。C现可执行下面第1—2步；第3—4步仍待真实inventory固定。

**本批集成判定：ACCEPT，仅PR25上述head的入口／显示／内容准备实现与准确r2 Trial内容准备门。** QA已正式回传相同限定结论，依据为准确GitHub独审、36＋9覆盖45项及8件实际prepare核对；无待修finding。完整B02实际资源／Scene／作者表试玩及B03/B04仍执行中，不据本判定跳过后续缺口。

首次本地发布调用被自动审批在执行前阻止：只识别历史N07授权，认为publish／activate-export超出范围；Unity及所有后段命令均0，没有新增发布目录。`host/publication-01/result.json`保留原拒绝，91输入与267保护哈希已核；准备及回执101.734912秒，累计1605.458460／3600秒，余1994.541540秒。主程随后直接读取中央真实人类turn `01a170c6-cdaf-74d0-b9b6-709641639061`／userMessage `01a170c6-d0cc-74a1-b745-1e462eae70ca`，确认当前“按这个设计继续实现……普通步骤不要再等我确认”原文；已交同一C亲读并把该新证据及工程§7的准确本地副作用提交原审批入口核验一次，不换工具／改含义规避。若仍拒绝即停在该门并向用户提出准确确认，不追加盲试；独立r3作者准备继续。

该阻挡现已解除：C亲自通过`read_thread`核真实userMessage与工程§7，向同一`exec_command`审批入口补交上述直接证据一次后获准，原命令链已实际启动（session38503）。未换入口或扩大副作用，原拒绝与耗时保留，后续记`host/publication-02/`；仍逐步成功才继续，不因批准冒称运行完成。

1. 以已冻结r2、source-freeze-03和真实QA文件，串行各一次标准`ChapterContentAuthoring.Run -fmChapterMode publish`、`activate-export`、`verify-release`，`-batchmode -nographics -quit`，每次timeout180秒；预计合计1—3分钟。写入只限`host-content/r2/publication/`既有工具固定存储／回执，以及`Assets/StreamingAssets/FightMatchChapterOne/`六个准确叶名：`first-release.fmsource.json`、`first-release.fmpackage.bytes`、`first-release.fmvalidation.bytes`、`first-release.fmreview.json`、`first-release.fmpublish.json`、`first-release.fmrelease.json`；允许Unity导入产生各自.meta及该目录.meta。实际冷读必须仅凭六件成功，旧Demo六件及旧发布根保持。
2. 六件冷读通过后，串行各一次既有`FightMatchResourceBuild.BuildChapterMac`、`BuildChapterWindows`、`BuildChapterAndroid`，标准executeMethod，每次timeout180秒，预计合计1—3分钟，同计上述3600秒。允许固定`host-resources/r2/{macOS,windows,android}/`中的packages／builtin／boot／inventory输出；工具将真实新文本复制至`Assets/FightMatch/ResourceBuild/Inputs/ChapterOneTrial-r2/fm-text-v1.json`及`fm-text-v1.manifest.json`，允许两文件.meta和新目录.meta。原Generated／旧文本pair／旧plan／旧包保持，不改Player设置、不执行Player build、不操作设备。输出日志及命令回执只放本任务`host/publication-01/`、`host/resource-01/`。
3. 上述产物形成真实inventory后才登记准确runtime复制清单与三平台boot的SHA／长度／GUID。预定boot导入路径为`Assets/FightMatch/ResourceBuild/fm-resource-boot-chapter-macos-r2.json`、`fm-resource-boot-chapter-windows-r2.json`、`fm-resource-boot-chapter-android-r2.json`及各自.meta；runtime目标只能为`Assets/StreamingAssets/FightMatchResources/{macOS,windows,android}/FightMatchChapterOne/`下inventory明确列出的文件及必要.meta。此段先规定接续边界，当前尚无真实inventory，不提前复制、填pin或保存Scene。
4. 最后由C按准确产物接`Assets/Scenes/FightMatchDemo.unity`三项chapter boot／pin／resourceSetId，不动旧三平台字段和Prefab。实际页面验证复用既有Host隔离根与正常Scene／Play：从新档正常结算1—8，观察8原结算与唯一法师／首通卡、阵容未自动改；手动编法师进入9并观察普通魔攻及新增双语文本。使用实际作者CSV路线；r2没有已发布战斗参考，不调用不存在的默认回放替代实际观察。隔离ID、身份、最终候选SHA、准确操作和剩余时间在产物到齐后再固定；保留r2测试资料，不先在真实玩家根创建r2资料，不手改存档跳关，不新增runner。此页面步骤当前未启动。

实际执行回执：`host/publication-02`的publish／activate-export／verify-release分别exit0、21.599850／21.629873／19.807615秒。六件实际导出及仅凭六件的冷读成功，未读作者目录或publication store。随后`host/resource-01`首个Mac资源构建exit1、16.965618秒，SDK `TaskCreateManifest_RFBP`报ErrorCode602／`fm.content.source.rawfile` bundle hash冲突；Windows／Android均0，无boot／inventory。source与payload合法地同为44332字节／SHA3127954b…cf25544，不改业务字节规避。91输入及267旧保护保持，Unity退出；新六件、导入文本、10个meta与9个部分资源输出见两份原始回执。累计1830.980031／3600秒，余1769.019969秒，生成仍394.432760／600。

content已定位本机YooAsset3.0.6标准配置：默认RawFileBuildParameters.IncludePathInHash=false导致相同字节的PackageFileHash冲突；SDK公开IncludePathInHash=true可将准确asset路径纳入MD5。仅同一白名单的`FightMatchResourceBuild.cs`对chapter启用该选项，旧Demo保持false，八逻辑ID／位置及业务SHA／长度均保持，不改vendor／命名风格／守卫、不手造manifest。收齐准确SHA后仅补真实Mac资源构建，并在成功后继续尚未执行的Windows／Android；不重跑45项或本地发布／冷读。

修正已停写：Builder新SHA `0d8432d75682dedf78cab69f4f150061be701d67b6a5165f6df99437240b23c0`，21413字节，仅注释和`IncludePathInHash=chapter`两行。`host/source-freeze-04.json` SHA `9d9e03c81ce8c997e55d71e98f674f6a48d529a46e09f2d11a503ed911016b9b`，36候选、98输入（原91仅Builder差量，加QA批准和真实六件）；实际内容与批准原字节不变。C下一段`host/resource-02`各一次Mac→Windows→Android、每次180秒、预计1—3分钟，仍从1830.980031／3600秒累计；先保留第一次9件部分输出的证据副本，再走SDK自身正常构建／缓存行为，不手改manifest或哈希。须确认八逻辑映射、source/package不同物理名但原SHA／长度一致。修复随后与实际资产候选交GitHub独审；既有内容门不重新发证，未变45项和六件冷读不重跑。

`host/resource-02`实际三平台均exit0：Mac22.743秒、Windows15.549秒、Android15.968秒（四舍五入；精确值以回执为准）。各8逻辑映射／14件inventory、source与package不同物理名且原SHA保持，SDK构建及CreateBoot严格读回成功；98输入／267旧保护／10个已有meta保持，Unity退出。验证累计1981.995418／3600秒，余1618.004582秒。主程另外逐件核3个inventory共42件和3boot字节、各8个逻辑资源与真实六件／279文本源一致，形成`host/runtime-copy-01.json` SHA `c107e3fc93f01df9312d4ce81221e7e44921fd75fd10edcbb028bb17ced376cf`，准确列出45个source→destination、哈希／长度及3个Scene目标字段；未复制或写Scene。

**第3—4步职责与执行固定：** combat此前未写Scene，现将`Assets/Scenes/FightMatchDemo.unity`三项chapter boot接线的限定写职责移交唯一C。允许依上述45条清单原字节复制到新目标，并由Unity生成45个文件.meta及3个新包目录.meta；原目录／旧包保持。C以现有Intel Unity GUI＋CUA原生控制在Inspector接三项boot、pin、resourceSetId，仅保存该Scene；不安装Unity6 Pipeline、不新增runner、不全局SaveAssets。保护旧三平台引用、Prefab、字体.asset及其.meta、其他98输入和原267中除明许Scene外的文件。

本段含复制／GUI／收尾上限1500秒，仍沿1981.995418／3600秒累计；预留正常退出和回执时间，只有本任务Unity一个实例。新隔离ID `d556377f23e549a096f007ce3a258d8c`，candidate参数为runtime-copy-01的上述SHA；隔离根准确在该manifest，按原Host record格式填C的本次真实thread／turn，不复用旧activation授权。一次正常Scene／Play，按第4步正式1→8结算→手动编队→9法师攻击，必要新增文本中英观察及原操作历史只读查看；允许正常测试进度保存到此隔离根，保留所有提交记录，禁止手改档／跳关／花卡培养／额外战斗或重跑旧验收。若已无法在剩余时间内完成则正常退出，记录实际覆盖缺口，不自加重试。结束保留r2资料及截图／原始日志，资产与修正候选尚需后续准确GitHub独审和QA接收。

### B03／B04无冲突作者准备

教学先实施、双面Boss随后贯通；只有完整新状态／执行合同到齐才发布后续实际集合，避免中间半套能力进入资料。当前允许主策独占准备新作者目录`Config/FightMatch/ChapterOneTrial-r3/`，准确18叶名沿r2相同：`Release.csv`、`CharacterGrowth.csv`、`Passive.csv`、`Level.csv`、`Face.csv`、`PairEnemy.csv`、`EnemyAction.csv`、`DefaultRouteCell.csv`、`Item.csv`、`Card.csv`、`Skill.csv`、`Teaching.csv`、`Reward.csv`、`RewardMaterial.csv`、`CharacterUnlock.csv`、`FirstClearItem.csv`、`SourceMetadata.csv`、`README.md`。revision为3，保持既定职业、关卡及业务身份和Trial状态，补10—16、正式教学／嘲讽、双面Boss以及冻结设计要求的新能力。旧`Config/FightMatch/ChapterOneTrial/`及Generated/r2作为已发布r2输入冻结保留，不作为第二个可独立编辑的最新版；新目录README明确该版本关系。

此准备只写新18文件并做常规CSV静态核对，不生成／运行／发布／改真实资料。1—9已定数据不顺带调平衡；如10—16衔接确需产品决策，集中报具体冲突。原279文本CSV和生成物仍固定供B02使用，主策只回新增文本需求清单，暂不写其原路径。后续代码白名单、精确能力token和验证预算在实际启动前登记；此段不提前授权源码写入或宣告B03/B04完成。

三作者只读接口核对已收口：r3新增精确`CH1.TauntBossStateV1`为第八cap，完整冻结技能／敌意图参数、CD／嘲讽／Wind／易伤状态及全部事实一次定稿，含合法仅技能的零普通伤害事实；B03先实现、B04随后，实际r3只在全能力齐备后发布。旧r2七cap及空Entry features永远选原布局，原r2 canonical payload/hash保持；`PublishedContentCodec`的嵌套反射也须按精确能力分支，不能只改存档codec。永久Quote／Effect／教学Challenge正文保持原字节布局，`GiftOnceKey`通过原精确Teaching定义解析；即使没有活动Battle，Application／Business存档头也须从精确引用定义收集新cap。Skill的`battle_capability_id`仍按冻结设计为`CH1.MageTauntBoss`，不得以新状态token替换。此为后续准确实施依据，尚无相关源码写入。

主策r3作者18文件已交付，actual turn `01a17122-c32f-7853-9295-ae67fb9e36f3/local`：16关／17面／47敌／20意图／208路线格，30条来源；其静态列型／引用／几何／周期／同面Wind／来源检查通过，未生成或运行。17CSV集合SHA `180efb9d4addb8b7ddeb51a4260ed995087078f9efbcf3852accdf7f9e23e9dd`，README SHA `285b931fd8748709881fdcea407e5a2521af5221c8741e95b68ffa3a2c9aa4aa`；r2与279源保持。新增26条英简中key／参数需求在r3 README，尚未接原文字表；唯一教学新引用`ch1.tutorial.taunt.explanation`待后段接入。主策不另向中央重复回传。

主程已核18文件集合及README准确哈希、279源保持，并保存`b03-b04/inputs/ChapterOneTrial-r3/`不可变副本；同级`chapter-one-r3-manifest.json` SHA `6f88c2fd13a8c88b3fa6c2183098a2941de9bf3c9729aa670a2378f4e90c99db`。复用主策静态核对，不额外启动生成或验证。

C进行r2 GUI期间，content仅可新增非Assets的`Config/FightMatch/Luban/ChapterOneTrial-r3/luban.conf`及`schema.xml`，复用现有17张全string表与标准薄适配，staging固定为本任务`b03-b04/luban/staging-r3`，后续生成输出为`Generated/FightMatch/ChapterOneTrial/r3`。不得修改原r2配置、薄适配源码、279源／Tools、Assets或任何冻结输入；不运行生成器／Unity。此为既定r3作者输入的准备接线，不改变执行状态或提前发布能力。

r3未发布输入的技术纠正：教学说明key统一为`fm.ch1.tutorial.taunt.explanation`，匹配现有LocalizationService／CatalogParser／文本编译器的`fm.`命名空间守卫；不放宽守卫，原文案／参数／业务语义不变，不改冻结设计。主策仅改Teaching.csv和作者README，17CSV集合SHA现为`867a9ab630e8d6d6201fe3d5d8b71c8db4cdec016c5b348967242523eebf7462`，Teaching SHA `19faf3749e6d954e0b691cb6b72d518d84c5238d3de4bac93315a2529678545b`。实际用卡／领取教学赠物页面还需5个明确key，合计31个新增需求；参数直接取现有Quote，目标等级不误作卡数，前后XP明确为各自等级内经验。README SHA `da28f7f1346f1543941b39b3b21777db1b8d503de7af14bb2f635d38d9a8fdeb`。原279文字表未改；新18文件副本在`b03-b04/inputs/ChapterOneTrial-r3-ui-keys/`，manifest `chapter-one-r3-ui-keys-manifest.json` SHA `c7b700b7d6277bdf1fb96db7dba6a2175db1b81cbb4d59d8b9fd17d158c5de33`，旧副本保留。

content已交上述两项Luban配置并停写：`luban.conf`445字节、SHA `d3a772c2a46dadf4b644bd8f0e600aaf444de7f7e5af2768bc46ed8117be4d6b`；`schema.xml`9166字节、SHA `27cebe7ff5a83bd40bbe9b3bfa330d5407998f3896762e8649a80adf2d7254d0`。17张表全部兼容现有string schema，薄适配无需改动；无生成器／Unity运行。

r2 GUI现场接续：原C actual turn `01a17139-8987-7e73-8c24-92d16abb41c5`已复制45文件并启动唯一Editor PID92339／监督PID92338，phase起点1792900983.9546208、绝对截止1792902483.9546208。首次CUA getApp(path)等待508.3秒后随C回合中断而中止；原Editor／监督进程继续存活，未重启。C恢复turn `01a17143-1e4b-7502-926c-076b2ff36093`只接管同一现场，原activation身份／隔离根／累计预算保持；恢复时Scene仍为原SHA9911cf81…bad35。桌面控制失败与Unity功能结果分开记录，未观察页面不得标通过；最新收尾结果待同一C回传。

C回传准确工具诊断：bundle标识`com.unity3d.UnityEditor5.x`匹配两份安装，2.432秒返回Ambiguous app identifier；随后按工具建议选择已知准确运行路径并设置20000ms，实际等待505.8秒后才随第二次回合中断结束，仍无app对象／AX／截图。此为桌面连接失败，不是审批拒绝。C已停止所有连接重试，无可用原生控制，不能声称正常GUI退出；仅收回文件核对并等待原有1368秒进程级监督结果，不手动kill、不另启控制渠道／Editor。Scene接线、保存及Play均NOT_RUN，实际进程收尾和累计时间另以原回执确定。

### B03／B04实施范围登记

现有冻结设计及本记录已对齐的精确能力合同作为实施计划，按教学永久流程→嘲讽和双面Boss贯通；不重新做设计或提前发布半套r3。r2实际页面缺口独立保留，不将工具失败写成产品通过。以下源码写入须等唯一C确认原Editor结束、r2实际资产候选快照完成后，由主程明确启动。三个作者分别持有互斥文件，不操作Git／Unity；主程只协调、保存候选和收件。所有路径下列根为`Assets/Scripts/FightMatch/`，新增测试根为`Assets/Tests/EditMode/FightMatch/`。

| 作者 | 允许修改的既有源码 | 允许测试／新增路径 |
| --- | --- | --- |
| content | `Content/PublishedContentModels.cs`、`PublishedContentCodec.cs`、`ChapterContentCompiler.cs`、`ChapterContentTables.cs`；`Core/ChapterRuleDefinitions.cs` | 修改`ChapterContentTestData.cs`，显式保留旧七cap夹具；新增`ChapterTauntBossContentTests.cs`及同名.meta |
| save | `Core/CandidateBusinessSaveCodec.cs`、`CandidateBusinessSaveValues.cs`、`CandidateBattleSaveCodec.cs`、`CandidateChapterSaveCodec.cs`、`CandidateApplicationSaveCodec.cs`、`CandidatePermanentCodec.cs`、`CandidatePermanentDefinitions.cs`、`CandidatePermanentProtocol.cs`、`CandidatePermanentProgression.cs`、`CandidatePermanentInventory.cs`、`CandidatePermanentGrowth.cs`、`CandidateProgression.cs`；`Application/PlayerPermanentSession.cs`、`CandidateLifecyclePermanent.cs`、`PlayerNavigationSession.cs`、`PlayerNavigationModels.cs`、`PlayerNavigationQuery.cs`；`Presentation/PlayerNavigationController.cs`、`PlayerNavigationView.cs`、`PlayerPermanentDetailView.cs`、`PlayerNavigationRecoveryView.cs` | 新增`ChapterTeachingProtocolTests.cs`、`ChapterStateSaveCompatibilityTests.cs`、`ChapterTeachingNavigationTests.cs`及各自.meta。复用既有Draft／Quote模型，无需改PlayerPermanentModels |
| combat | `Core/BattleEntryInput.cs`、`PreparedBattleEntry.cs`、`BattleEntryPreparer.cs`、`BattleStartAssembler.cs`、`BattleSnapshot.cs`、`CandidateCombatFrame.cs`、`CandidateEnemyPhaseFrame.cs`、`CandidateEnemyIntentFact.cs`、`CandidateBattleOperationRecord.cs`、`CandidateDirectAttack.cs`、`CandidateChapterBattleRules.cs`、`CandidateEnemyPhase.cs`、`CandidateBattleOperations.cs`、`CandidateBattleStage.cs`、`CandidateBattleReportFingerprint.cs`、`CandidateHistoryOperations.cs`、`CandidateReplayOperations.cs`、`CandidateReplayComparison.cs`、`CandidateBaseRewards.cs`、`BattleDamageFact.cs`；`Application/PlayerRosterSession.cs`、`CandidateLifecycleRoster.cs`、`CandidateLifecycleEnd.cs`、`CandidateLifecycleEntry.cs`、`CandidateLifecyclePreparation.cs` | 新增`Core/ChapterBattleSkillModels.cs`、`Core/ChapterEnemyActionModels.cs`及各自.meta；新增测试`ChapterTauntBattleTests.cs`、`ChapterTeachingLifecycleTests.cs`、`ChapterBossBattleTests.cs`、`ChapterBossLifecycleTests.cs`及各自.meta；既有`ChapterBattleTestData.cs`仅作必要共享fixture扩展 |

content验收：真实r2 canonical SHA3127954…cf25544严格保持；真实r3正常生成数据Read／Prepare得到16关17面、八cap和完整技能／教学／敌参数；未知／缺失能力、非法引用和新字段注入旧七cap拒绝；Skill能力仍为CH1.MageTauntBoss。save验收：T1—T4的稳定赠证与准确来源限制、免费引用已有学习、独立说明、目标等级用卡预览和原Unknown恢复；永久正文布局保持；无活动Battle仍声明新cap，旧／新战斗正文按精确能力分支且完整恢复新状态／事实，不支持时保档拒绝。端到端教学入场及最终首通证由combat负责，不重复实现或覆盖同一测试源。

combat验收：教学任一缺口均在Attempt分配前拒绝，永久教学已完成事实不因战内回退而撤销；合法仅嘲讽／普通击杀不施放／CD两行动递减；准确目标、周期、独立Wind用量与分数恢复、易伤下一有效行动消费；补线与翻面零额外推进、全倒的阶段优先、同Attempt跨面状态延续、完整事实指纹／保存／回退与复播；16唯一终局及第二证来源。具体值复用冻结验收T1—T5、B1—B6及C3/C4例，内容／codec分别由其拥有者接入。新public模型／字段应先互发准确形状，避免三方猜接口；正常分行、清晰领域命名和小函数，不顺带格式化旧代码。

B03／B04针对验证另列为新工作预算：仅新增／受影响用例及相关发布接线，标准Unity单调用上限900秒，累计主动验证上限3600秒（包含必要普通修正），不重置或挪用B01/B02既有耗时。r3标准Luban／文本生成预计数十秒，含必要修正累计上限600秒；原r2生成394.432760／600秒证据保留且不重复。实际用例选择及命令、真实r3发布／资产白名单在固定候选后再给唯一C，不提前运行。桌面控制不可用时不把这些额度当作GUI重试许可；真实页面剩余验证须有可操作入口后单独界定。既有70及45项有效证据按输入变化复用，只补有影响的缺口；准确候选交中央发布draft、不合并，QA独占GitHub审查和接收。

r2 GUI最终收尾已收：C actual closeout `01a1714b-cb2c-78e1-ae61-bda20ef3a173`，原监督1368.0664秒timeout，exitCode=null，Unity与监督均已退出；不是正常GUI退出。全段1476.494703／1500秒，累计3458.490121／3600秒，余141.509879秒，原生成预算不变。45复制／48meta／98输入／270保护最终一致，Scene未变、隔离业务文件0，接线／保存／Play全部NOT_RUN。原始工具失败和process-result保留，C停写，无额外启动。

主程已固定实际112路径候选`review-candidate-03/`，manifest SHA `ce9f5cfee7993337bc4d6e7e0888d2ec2b5dbfd5940200311d28b3156f58f5ef`，base为PR25 `1e87c7960fc429b7229862c720520ed9ea42e207`。只含SDK两行修正、六件发布、文本导入、三平台runtime／boot及其实际meta；不含未接线Scene、B03/B04、旧WIP或玩家档。该包可单独审查资源产物，不宣告实际页面可玩。

B03／B04上述53项既有源码／夹具前像已保存在`baseline/b03-b04/`，manifest SHA `233807e8a1bc1b0ff74734c0c90e0cd7f7e714db309191bdb7bd0cfa17203c62`；逐项与PR25原字节相同，没有旧WIP夹带。C已确认无Editor，现解除这53项及明列新增路径的源码冻结，由原content／save／combat按分工实施并回主程；Unity／生成仍待固定候选后派唯一C。主策可将已定31个新key接入原`Config/FightMatch/Localization/FightMatchText.csv`，保持279旧行逐字段不变；只做通常CSV静态检查，不手改Generated或资源。

save最小增补`Application/CandidateLifecycleModels.cs`：现有Content输入与冻结Content携带可选`PublishedSaveContext`原定义闭包，仅引用传递不序列化；Permanent提交必须能按旧gift quote.Binding解析原Teaching的GiftOnceKey，不能拿当前同ID定义代替旧来源。该文件唯一归save，combat不并写；前像与PR25相同，保存在`baseline/b03-b04-added-save-context/`。不改变Quote／Effect正文或迁移边界，旧调用保持原行为，缺精确原定义明确拒绝。
