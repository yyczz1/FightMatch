# CONT-A 三槽队伍与角色资料：独立设计审查

唯一结论：**ACCEPT**。§329 的设计交接要求已满足，可供 SD00 另签 CONT-A 源码任务；本结论只接收设计，不是实现、运行验证或 Demo 完成结论。

R task：01a0c1cd-dce1-7ac3-8780-06163cb0acfc（FightMatch Demo 独立代码审查）。
准确 R turn：01a0d720-cf4b-76c1-9615-b71186a617cc。
原生 context：2026-09-25T05:54:06.900Z；续接 context 2026-09-25T06:01:20.285Z 均为 gpt-6-astra / max、D:/Unity/UnityProj/FightMatch。
本报告是本 R 回合唯一写入；完成时间由本回合正式交付后的原生生命周期登记，不预填未来 completed 时间。

## 1. 准确交付门与审查对象

依据最新三协调稿、§328～331、§329 全部要求、工程规则和已接收连续性设计。§329 规范化 SHA256 为 abd1c0be0cd77ccd97c93d9633e16af512bd4d79dc1f165bded402f6baa45ae6，独立复算一致。

| 项目 | 独立核对结果 |
| --- | --- |
| 原 C task | 01a0c403-bfa1-7e90-b503-c0fcd61f23c1，FightMatch 本机保存与应用接入实现 |
| 本次 C turn | 01a0d71f-f907-77e3-8827-1e6a32ef226d |
| 模型与环境 | 原生 context 核 gpt-6-astra / effort=max，正确工程根 |
| 自然完成 | 2026-09-25T06:38:39.520Z，原生 task_complete 正好一条；应用状态 completed / idle / error=null |
| 非异步正式交付 | 原生 task_complete.last_agent_message 为 DESIGN_COMPLETED，明确上述 turn、两份实物身份及停改；未把草稿、工具输出或另一回合交付当作完成 |
| 设计报告 | docs/system-design/2026-09-17/demo-cont-a-design.md；36,200 字节，238 物理行；SHA256=0f4ac05443ef7218f6ef2b892095a82f2abcd6e72455372c653572240204d5fe |
| 设计 scope | docs/system-design/2026-09-17/demo-cont-a-design-scope.json；2,968,593 字节；SHA256=2c9068d6e105ec789d2e6fb6476e9a000cb8652aa1fcc80b275f69e6aad67652 |

先独立核 completed、正式交付和两实物一致，再读取并审查设计内容；此前仅准备既有规格、源码约束与保护基线。2026-09-25T06:49:13.821Z 两文件身份再核未变，C 仍为同一 completed 回合。没有重启旧 P2C 回合或要求 C 提前修改。

设计报告未自判独立结论，scope 正确绑定报告；scope 自身身份在作者 formal 外部提供，无自哈希环。238≤320 行，2,968,593≤4,194,304 字节。

## 2. 已接收入口、保护集合与有限证据

P2C 已接收三报告的当前实物均再次核符：

| 报告 | 字节 | SHA256 |
| --- | ---: | --- |
| demo-025-p2c-delivery.md | 8497 | 52c62c41eabb6a0f626e464b2781aac8d5d32570dda5e3a811708f575911620e |
| demo-025-p2c-scope.json | 2333995 | 60a2d6c681dbe60dd0270e82d6be15009a8824697b5f1c0d30e6367e73f31bf6 |
| demo-025-p2c-code-review.md | 22456 | 5b07831d9c9d11a628f0f6a6ef0c7e471bd19f459057e82e5a1326cb9234ced0 |

以上路径均在 docs/system-design/2026-09-17。不重新审判 P2C 的既有结论，也不重复其有效运行。

2026-09-25T06:45:59.282Z 独立复算：当前实现集合722、Assets752、自然GUID398、导出810；设计 scope 与已接收 P2C 对应完整集合逐项身份一致。807 项不可变导出的实际长度/SHA 均相符；Assets 物理文件集合与声明一致，398 个 GUID 实物匹配且唯一。37 个建议修改旧生产文件和1个工具的原字节/SHA/物理行数均符，11个建议新.cs及11个自然.meta仍不存在。

三协调稿因 SD00 追加§331等内容单列，未误判为代码漂移。本次独立观察值：

| 文件 | 字节 | SHA256 |
| --- | ---: | --- |
| docs/system-design/2026-09-16/session-plan.md | 269817 | 4d09ab6a64c5d0b57a330a4be083ba7f063e43698d5d7a18f052f7a5d5935b39 |
| docs/system-design/2026-09-17/integration-review.md | 360978 | 91c0d9ebcf6324f04dd51cf42bb13f2cdc970ad0e8899dc0de51531a6136c7a9 |
| docs/system-design/2026-09-17/system-task-packets.md | 1229236 | 06f8a49835be705e3ac1b427aefa4811370db8acbc62d3fa576f789088624a12 |

2026-09-25T06:47:02.998Z 依已接收 P2C 闭包，逐组将设计 scope 的有限 items 与已接收 scope、原 read-manifest 对照，再核实物长度/SHA，零差异：

| 证据根（均在 TestArtifacts/FMDemo025P2） | items | root-identity SHA256 | read-manifest SHA256 |
| --- | ---: | --- | --- |
| p2c | 166 | c2aed95b9aabd2dae897623cd26af93158d505b5e0648cf673684708d4684869 | 6697c91d2864a944360d11aa8697b9c4f9c3fa489203011d80ac9e2efb5958de |
| p2b-publish | 116 | a65fb7d567267636ff9fcacf9bb235105f296670e1e3d356f855ff746cf20891 | a68653f38151031488cf800a73fd529557f2c0cf8e9e889d514ebe3d5776ff26 |
| p2b-prepare-c2 | 75 | 8b26bdfb385266d4d0d407559f0ec97d83686d5a29401faf74d8a33c1d743162 | 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25 |
| p2b-prepare-c1 | 1442 | c2e8bee41daeccfe2c83c6d10adb8faae930aaaa01320ab729f85adae87d64fd | c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c |
| p2b-prepare | 1442 | 49f2e30e864cfcb8b5289b7f57b4ee103ea0bb05c6fb911f1b18cd3709cd46d6 | 9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8 |
| p2a | 1454 | 985c8c9159f4778ce36b656448fa2ce050b70d7f4dc8d8aa4acbeedef1b74f4e | 2cb78d0a45526e60a4435e7becf5f1c03a9ad8aafe04fec5cf392b3ae8ea197a |

合计4695个有限item；根与manifest另核。没有递归读取未知证据目录。
原 p2c/runs/009/tests.xml 为2,630,563字节、SHA256=5c6bdba060a7d4c7f2292bbe0b2ad39f59b3ef944a5d07184479c81bb6ea3be5；静态解析为3655 Passed、0其他结果、3650个不同fullname（重名实例按次数保留）。具名多重集合SHA256=6ab79d7846638672b60b2913c9c1ed3540228436cdf683e3401847b31e303d24，与设计scope一致。这是既有运行证据的身份核对，不是新测试运行。

## 3. 同源业务与接口设计审查

| 核对项 | 设计与实际依据 | 结论 |
| --- | --- | --- |
| 角色与三槽唯一拥有者 | 设计31～36行：M03 Roster持有唯一CharacterId/ClassId集合及恰好三个可空引用；已有W仍同一实例，恢复者保位，无自动填充/赠角色。M04拥有全局库存与逐角色负载，M05/M06拥有冻结参与事实，页面仅消费。符合M03规格22～24、43、99行。 | 满足 |
| 当前槽与历史槽分离 | 设计33、63、99、134～137行保留旧Character.OriginalSlot为初始化事实；当前阵容由Formation提供，历史Begin/Carry/Entry用该次槽。实际CandidateApplicationReferences.cs:165～166原本核初始化槽，设计明确适配该处，不会把换位解释为重建角色。 | 满足 |
| 旧公开访问与新接口 | 设计40～65行、scope.interfaces/legacyCompatibility给正式查询/准备方法、目标/head/revision/time输入、旧单值保留范围及多值AmbiguousCharacter/TryGetSingle约定。旧kind2只供旧格式与历史查询；新集合入场不默选首人。CONT-B/C消费边界明确，无页面/永久成本实施。 | 满足 |
| 阵容准入与幂等 | 设计47～60行明确Prepare→原可信Submit→原结果；设置三个明确值，深冻结、同op异意图拒绝、Unchanged仍有原操作结果。活动/S17/pending/未确认创建/旧格式均有具体门控；恢复者可换位或取消，恢复事实不变。 | 满足 |
| 多角色携带与参与 | 设计71～75、78行沿全局T−ΣL−ΣR及各角色L/C/R/U，禁止借用未参与者L；M05与M06消费同一准备集合。非空携带战斗仍明确能力拒绝，独立M04正例验证携带守恒，不新增物品效果。 | 满足 |
| 战斗/经验/结束 | 设计69～80行列出五个实际战斗单人校验文件，按CombatantKey核成员、PRD、贡献与结果；逐人复用原评分和成长算法，材料/首通仅一次，未参与者无本次XP/End/新恢复。结束/经验索引用角色与收据联合键，允许同一结束共享EndId。 | 满足 |
| 恢复联合H02 | 设计82～88行冻结显式样本/head/各修订，在一个候选内推进所选恢复并形成Ready参与集合；无人或能力失败全部拒绝；准入后一次ID/熵、一次M12提交。M02 OwnerHistory先重放恢复结果再核入场修订；不保留旧Enter要求Character引用不变的错误断言。 | 满足 |
| 重开和失败续办 | 原基线/成员/槽/Stats/HP/三域初态保留，后来恢复者不加入；正常新入场才采新熵。SaveFailed/CommitUnknown保留原候选/op/time/ID/熵，沿原Retry/Resolve/ResumeObserved。 | 满足 |
| 合法隔离多角色见证 | 设计211、223、228～229行限定不同ClassId/CharacterId且仅现有Warrior规则；从合法纯构造/解析闭包和同一内部builder生成，不反射造成功、不声称新增真实发布内容。真实PlayerSession见证仍取已批准单W。 | 满足 |

独立源码追踪确认单人限制确实位于建议修改范围：BattleEntryPreparer、CandidateInventory/Progression、CandidateLifecycleEntry/End、CandidateApplicationProtocol/References、CandidateBattleOperations/EnemyPhase/HistoryOperations/BaseRewards、CandidateBusinessRestoreChecks和视图适配等。对有限允许源码中相关标量访问、成员数量和首元素用法另作交叉检索，没有发现本设计必须修改而漏列的阻断调用点。

保留文件也有实际依据：CandidateWarriorCritEvaluator已按角色key维护PRD；CandidateDirectAttack/CandidateBattleStage已有参与集合匹配；CandidateBattleSaveCodec已有列表及按CharacterId解析MemberDefinition。现M10首包编译/replay仍只处理批准单W，保留其单人限制符合“不扩真实内容”。没有仅删Count==1守卫就宣称集合闭合。

## 4. 格式、磁盘迁移及F2可行性

| 项目 | 独立判断 |
| --- | --- |
| 明确版本 | CandidateValidation六schema1/FMINT001保持；旧PlayerSave六schema2/FMINT002保持；新向量明确为[3,3,3,3,2,2]，M02/M03/M04/M05改变、M06/M07列表wire与指纹保持；新增fm.player.roster.v1，旧feature保留。 |
| 规范意图 | 显式FormatVersion/tag冻结；FMINT003/version3仅新初始化及10迁移/11阵容/12集合入场。旧攻击/结束/恢复意图在新头按原字节解释。旧格式重编码不得自动升级，错误混配明确拒绝。 |
| 迁移事务 | 已确认Active、已核schema2头、无活动/S17/内存或磁盘未决时才准备MigrateRoster。冻结来源head/generation/descriptor length/SHA，一次原M02/M12提交；包装同一角色、可逆单元素投影，业务修订/绑定/历史/恢复样本不重置，仅新增迁移记录与generation。 |
| 活动/回退/S17 | 旧all-2读取与写出持续可用，原Battle/Lifecycle完成活动或原保留H06，之后再迁移；不强退、不重入或重采基线。迁移限制没有堵死旧活动档。 |
| 未知/崩溃 | 先原pending或磁盘候选的Retry/Resolve/ResumeObserved/End；已有迁移候选用原v3 intent/ticket续办。marker已成则返回原迁移结果，不追加迁移、不降回旧格式。 |
| 旧物理根与闭包 | 当前及M12声明保留的每个旧根按自身明确版本与精确包解码；历史records/index/fingerprint/receipt不丢。无需捏造已合法淘汰的祖先，但不能遗漏仍必需的前代/候选根；缺旧包/根或能力不足拒绝。 |
| F2旧建档 | record1/FMPROF01/FMINT002未完成者按完整原字节完成all-2 Initialize，再按原generation1/index[0]/record[0]锚确认Active，之后才迁移；Active旧档读取最新头。当前Formation不参与原初始化槽证明。 |
| F2新建档 | 显式PrepareNewRosterProfile产生FMPROF02/record2/intent3；原PrepareNewProfile仍旧格式。两者均使用同一new-profile:default/version1、298规范字节、原单W初态，无新增赠品。 |
| locator与能力 | FMAP01原初始化锚、M12 envelope/marker/head不变。实际LocalPlayerProfileLocator通过Core record codec读写，不自行解析record1主体，因此新记录分派可留在已列Core文件；宿主明确提供旧六schema2加前四schema3共10个contract。 |
| 错误与写门 | 未知版本/feature、重复/错角色或槽、错Player/包、缺根/预算、非规范字节全部拒绝且保全实物。PlayerSession可信门与公开任意builder的候选边界保持，内部测试Seam不成为正式写入许可。 |

设计90～138行及scope.formatDesign/migration给出上述实际版本和状态路由；不只是内存投影升级。QueryRoster旧档IsMaterialized=false与已持久化迁移有明确区别。未发现需要改写原Player、W实例、首包绑定或历史事实才能完成本方案的矛盾。

## 5. 可签实施范围与未来验证

scope逐项列37旧生产文件及1旧工具的理由/原身份、6新生产、5新测试及11个准确自然meta；所有建议修改生产路径均在已授权语义读取集合。readBoundary为322条去重条目，其中321条存在、1条历史允许路径明确缺席；新增读取来源只来自§329与已接收PUBLISH/P2C补足，未以未来scope扩权。

| 项目 | 复核 |
| --- | --- |
| 生产预算 | 旧增删规划2600 + 新文件物理行上限1000 = 合计3600；新6文件各上限求和为1000；属于未来上限，不冒称实际diff。 |
| 测试预算 | 5新测试含helper上限340+460+440+700+260=2200；旧测试/断言不改、不删、不跳过。 |
| 工具 | 仅原Tools/Invoke-FM025P2Validation.ps1建议增删≤60，固定CONT-A/Compile/Tests及独立根；不改旧六Stage行为，不新CLI/依赖/构建。 |
| 汇编 | 仅新ApplicationTestAccess.cs的一条现有FightMatch.Core.Tests友元属性；无asmdef/依赖变化，不开放公开builder写门。 |
| 未来集合 | 722+22=744实现，752+22=774 Assets，398+11=409 GUID；完整未来路径集合去重、逐项算符。未来导出838=810+22+P2C三报告+本设计三报告，当前设计身份与未来待绑定身份明确分开。 |
| 证据建议 | 唯一未来根TestArtifacts/FMDemoCONT/cont-a，259个有限允许文件名加manifest最多260；尚无创建权限。未来根/入口/源码包身份须由SD00固定，不能用设计turn冒充实施turn。 |
| 报告 | 未来C两报告及R代码审查路径准确列出；未来scope的8MiB建议与当前设计4MiB上限明确分属不同输出。 |

CA01～CA18覆盖真实W三槽、空/重复/恢复保位、多角色同一核、材料与经验/结束/恢复/重开、联合H02及失败/未知、旧schema2活动/S17/回退/历史/物理根/候选、F2两版本与迁移中断、精确包与能力拒绝、原3655具名保全及新用例。未来由原C串行Unity2022.3.18f1编译与无filter全量EditMode，记录源码/36 DLL/PDB同版和自然meta；当前全部标为未执行。

实施风险仍在历史引用按角色拆分、F2原格式回写和真实增删预算。设计已给具体适配文件、状态路由与否证用例，未发现阻断设计接收的实质缺口。实际超预算、额外文件或物理probe只能由SD00准确补签；不能删校验、压行、改旧测试或扩大读取来规避。

## 6. 本次验证边界与交还

本R执行的是有限源码/规格/设计静态审查、文件身份/路径集合/GUID/预算算术及旧XML解析。未运行Unity、EditMode、产品DLL、求参器或新物理测试；没有创建工具/证据根/源码/测试/meta，没有Git写入、新任务或代理。本回合只新写本报告。

没有待纠正设计项或需重新询问用户的内容决定。SD00可据本报告与两份C实物另签精确源码任务；之后仍须实现与新验证、独立代码审查，不能把本设计结论直接算作CONT-A功能交付。

P2C、025整体/B17原接收保持；计数仍34功能／29接收／余5／51正向交付。同产品路线与真实首包保持。CONT-B/C、028、029、完整Demo、真实宿主/物理locator可靠性及Android等未完成、未验证边界均保留。
