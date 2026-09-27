# 同一产品流程设计交接独立审查

VERDICT: NEEDS_FIX

审查范围：仅 system-task-packets.md §273.2 的本次设计交接质量。七条流程的复用方向可保留；F1、F2 的文件权限衔接和新档恢复契约尚需补齐。本结论不接收 P2 代码、025 整体、B17 或首 Demo。

## 1. 准确回合与正式完成门

- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 turn：01a0d16f-ccca-7e11-a648-aa4f58c2f5f1。
- C task_started：2026-09-24T03:22:40.237Z；turn_context：03:22:54.689Z，gpt-6-astra / max，工程根 D:/Unity/UnityProj/FightMatch。
- C 正式 AgentMessage：2026-09-24T04:01:11.277Z，phase=final_answer；明确 COMPLETED、两报告路径/SHA 和停止修改。task_complete：2026-09-24T04:01:11.385Z，turn_id 与 last_agent_message 一致。
- R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次 turn：01a0d16f-e6e9-7573-99ce-cc123ff1889b；本次原生 turn_context 核为 gpt-6-astra / max。
- 当前没有 read_thread / wait_threads 工具，故只读原任务原生 task_started、turn_context、正式 AgentMessage 和 task_complete。未读取/导出无关历史推理，未改应用私有任务存储，未发送续接或创建代理/任务。
- C 原生记录：C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl。
- R 原生记录：C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T10-31-29-01a0c1cd-dce1-7ac3-8780-06163cb0acfc.jsonl。
- 完成门满足后才审定 C 报告；此前只作本次授权输入的前置核对。旧 C1 completed 不作为本次门槛。

## 2. 正式实物及独立范围核对

| 实物 | 长度/行数 | SHA256 |
| --- | --- | --- |
| [C plan](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-plan.md) | 24285 字节；138 行，≤320 | 24bc8ae17f7c682fcf3bcbdc2a1ef6f020b2d7a5d51e0d839664db14672edfdc |
| [C scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-scope.json) | 486838 字节，≤4 MiB | 9d946be13720b10844e2f534f4c52bb5f1462c20f6a16f7aedfd804e0ff4acf5 |

两 SHA 与 C 非异步 formal final 完全一致；scope 对 plan 的长度/行数/SHA 绑定也匹配。scope 自身 SHA 由 final 外部绑定，无循环自哈希。

本次重新读取实际文件，2026-09-24T04:04:52.9866726Z 记录的核对结果：

- scope 的 234 项 inputFiles：234/234 长度和 SHA 一致。
- §268 implementation668 与当前 scope fresh.files：668/668 一致，总字节 3569979；没有后来差异，未回滚。
- 原接受集合按 path + TAB + sha + LF 得 c11a8a8aef0e46e00d364836e8baab3baea01fa45c81adddefae7cd7b8c84ea0。
- C 新清单按其明确的 path + TAB + bytes + TAB + sha + LF 格式独立重算，得 81db0fb2a13d3e8e1dc372b66e0bbe66be79ffae462e65b8f52220719a822c09，与 scope 一致；两种摘要格式未混用。
- 建议修改的 58 个唯一既有路径均存在，当前长度/SHA 全符；建议新增路径当前均不存在。Assets 当前文件数 681；此计数不冒称重新审核全部资产逻辑。
- §271 四实物分别为 7196、34768、96954、12528619 字节，SHA 与 §271 及 scope 全符；原候选/提案/回放未改。
- R 启动及写入前确认唯一 review 路径不存在。本次只新建本文件；未运行 Unity、测试、产品或求参器，未写 Git。

Scope: PASS（本次报告交付及当前文件身份）。
Acceptance criteria: 未闭合 F1/F2；不是现有产品实现或旧测试失败。

## 3. 规格和原接口核对

| §272.2 流程 | 独立核对所得 | 对 C 交接的判断 |
| --- | --- | --- |
| 启动/恢复 | CandidateApplicationRecovery.cs:22 固定 CandidateValidation；81～95 区分无头初始化与加载；ApplicationSaveCodec.cs:48 拒 PlayerSave。 | 正式准入、精确解析、实际宿主确有缺口；单 runtime/恢复发布链可复用。新档首次提交前的恢复契约见 F2。 |
| 冒险/选关 | CandidateProgression.cs:59/73/86 已有集合、开放/首通和入场核对；未发现产品地图或三底栏入口。 | P2 内容集合与 CONT-C/029 页面分工有依据；没有把 L1 临时按钮当完成。 |
| 队伍/成长 | CandidateCharacterState 单角色；CandidateInventory.cs:45 限一个 actor；LifecycleEntry.cs:33～64 构造单人入场。 | CONT-A 的角色集合/三槽和 CONT-B 的永久办理是实际新增需求；不是只补头像 UI。 |
| 背包/制作 | CandidateInventory.Read/Equip/SetPreference/End 可复用；未发现配方合成、卡/证消费与联合成长办理入口。 | 空目录、资源不足、未实现分开；CONT-B/C 所列真实办理与页面仍待实施。 |
| 入场/战斗 | LifecycleEntry.cs:45、67～70 使用 Guid 和真实 RNG；BattleApplicationSystem、BoardInput/Playback 共用应用链。 | 不需要另造 Demo 战斗核；正式内容/存档、阵容、页面/宿主各有明确责任。 |
| 结算/继续 | LifecyclePreparation.cs:77～96 取真实终局和 S17 原 reserved ID；LifecycleEnd 组织各域结果。 | 028 做真实到账与去向，CONT-C 做页面路由，029 联验；XP26 没有被写成固定奖励。 |
| 返回/中断 | Lifecycle 的 Exit/Restart、Runtime 原操作优先、Recovery 未决候选处理和视图关闭各不相同。 | C 保留这些语义和旧未验证边界，未拿 Close 视图冒充退出或保存。 |

本表源码目录为 D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch；表中简名分别属 Core、Application 或 Presentation，完整定位已核 C scope.flowMapping。

共享 RuleContext 与候选/正式分支的方向可用：草稿字段确实进入上下文复制、比较、指纹、意图及编解码，不能仅翻转用途。scope 的既有类型迁移文件覆盖了本次搜索到的 CandidateContext/PreparedCandidateContext 直接声明/签名位置；这只是静态范围检查，不是编译通过。

SaveEnvelopeCodec.cs:297～320 已区分正式 Content/Definition 与候选绑定；业务和 M02 codec、BusinessFields.Context、Requirements 仍固定候选，C 所列适配有实际依据。共享规则、旧候选隔离、可信内部提交、Publish/启用/PlayerSave 三门及 Core/Platform/Content/Application 依赖方向未见需推翻之处。

正式 ID、正整数版本到既有字符串的明确转换、31 个 C 和 ε、L1 范围、新档/奖励语义均沿 §271；新候选/证据与准确 review 另绑定的要求得到保留。CONT-A/B/C、028、029 的缺口分配可保留，建议 34 功能/51 交付仅为 SD00 待登记计划数。

新增内容不会自动替换旧玩家定义，旧活动局和历史必须继续 ResolveExact 原包；C 已披露后续采用新内容另需兼容办理。这不是当前已验证的升级能力，也不代表未来内容可直接覆盖存档。

## 4. F1：分阶段读取白名单未继承前序新增交付（P2）

定位：[scope:8168](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-scope.json:8168)、8487、8767 及对应 readFutureGatePaths；[plan:67](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-plan.md:67)、74～84。

四个阶段的 readPaths 都是相同的本次 234 个现有输入。P2B-PREPARE/P2C 的 readFutureGatePaths 为空，P2B-PUBLISH 仅列一份未来内容 review JSON；没有声明前序新增文件的读取继承。

具体缺失包括：

- P2B-PREPARE 使用 A 的正式上下文/绑定，但没有 D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ContentBindings.cs、RuleContext.cs、RuleContextChecks.cs、PublishedSaveContext.cs 的读取项。
- P2B-PUBLISH 的公开 Publish 接口需要 sourcePath，却未允许读取前序新建的 D:/Unity/UnityProj/FightMatch/Assets/FightMatchContent/demo-r1.source.json；同一发布实现、前序报告和新验证材料也未形成明确读取衔接。
- P2C 接入 catalog 和发布包，但读取表没有前序新建的 PublishedContentCatalog.cs 或五个 first-release 文件。共同验证脚本 D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1 也仅列 A 的新建，后继未明确读取/执行使用。
- 后继修改项仍记录本次原始 current 身份，未说明 A/B 获接收后如何以其交付重新绑定阶段入口。

因此按该“逐文件机器白名单”直接签发，后继必要读取会落到表外；不能以 dependsOn 或“共同脚本”推断整批新增文件自动授权。

最小修正：在两报告内逐阶段展开必要的前序源码、source/发布物、meta、报告、验证脚本及准确证据读取项；可以由明确的前序 create 列表机械展开，但须说明展开规则、阶段入口重新取证及前序独立接收门。已有/未来证据路径分开，不伪造未来 SHA，不增加目录级修改权限。F1 不要求当前创建这些文件或运行任何代码。

## 5. F2：新档定位器未定义首个完整保存前的原意图恢复（P2）

定位：[scope:7503](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-scope.json:7503)、[scope:7531](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-scope.json:7531)、9254；[plan:90](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-plan.md:90)～93。

报告承诺先持久保留建档意图，崩溃后查询原建档结果且不生成另一身份；但拟定的 RecordCreateIntent(string playerId, string operationId, SaveStoreBudget budget) 只接收两个 ID，未定义所保存的准确内容绑定、冻结初始化载荷/规范意图或可恢复引用，也没有给出无首个快照时的恢复/终止入口和状态转换。

直接受影响的窗口是：定位器已记录建档意图 → 第一个候选/完整头尚未落盘 → 进程中断。若此时启用集合改变，单凭两个 ID 不能判定原建档采用哪个包、哪个新档配方；即使集合未变，也未约定怎样恢复原 PreparedPlayerProfile 并继续原意图。

现有依据：[CandidateLifecyclePreparation.cs:16](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateLifecyclePreparation.cs:16)～30 每次重新准备生成新的 PlayerId/OperationId；[CandidateApplicationIntentCodec.cs:50](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationIntentCodec.cs:50)～75 的原意图还包含 Context、CharacterId、ClassId、等级/经验/槽位；[CandidateApplicationRecovery.cs:81](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateApplicationRecovery.cs:81)～91 无保存时只恢复 InitializationReady。现有代码没有可替该新定位器补回丢失内存意图的入口。仅“查询原 OperationId”不足以定义这一阶段的继续行为。

最小修正：补齐 P2C 的建档记录数据和接口契约，说明如何保存/恢复原 ID、准确绑定、初始化载荷或等价可解析引用及规范意图身份；区分“只有创建记录”“已有未决候选”“已成头而 active 确认未成”。继续/重试使用同一原意图；若选择显式终止未提交创建，也须给出确认未提交的依据、状态和入口。缺原包或坏记录保全并报错，不从最新内容重建。保持 Platform→Core 的方向，不为接收 Application DTO 引入反向依赖。

补入针对性未来验证：在上述三个窗口分别中断/重开，并在首个窗口切换启用集合；应恢复原 ID/绑定/意图，只形成一个初始化结果，或进入明确的原意图终止/保全分支。坏记录、原包缺失和再次确认不得隐式新建/改绑。此次只补报告中的验证设计，不运行测试。

## 6. 更小的报告纠正包

目标：只关闭 F1/F2，保留其余已核交接内容，不重发原宽包。

- 纠正对象仅为 D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-product-continuity-plan.md 与 demo-product-continuity-scope.json 的受影响条目；仍守 ≤320 行、≤4 MiB。
- 按 F1 补依赖读取/阶段入口继承；按 F2 补创建记录、精确接口、恢复状态与负例；同步 signatureAdaptations、阶段范围和 verificationPlan。
- 验证仅为文档/JSON解析、逐路径静态可达性、签名与数据责任一致性、当前输入/基线身份、新 formal final 对两文件的 SHA 绑定。
- 不改源码、测试、程序集、资产/meta、配置、四批准实物、旧接收报告或协调稿；不运行 Unity/测试/求参器，不写 Git，不建任务/代理。
- 由 SD00 按 §273.3 在原任务签发本小纠正；本 R 没有修改 C 文件、续接 C 或提前授权 P2 实施。本次唯一 review 保留，后续准确纠正回合及 R 输出身份由 SD00 登记。

## 7. 保留结论与实际验证边界

本次核过报告正文/JSON、完成门、文件身份、原入口/签名及上述直接依赖；没有执行 C 提议的未来验证，也没有将静态设计问题称为运行失败。

027、025-P1-C1 的历史接受和旧有效测试保持。合法 AwaitLinks/全倒补线、物理输入/像素/交互式 PlayMode、Player/Android 真机、未覆盖保存故障及 §184 历史 NOT VERIFIED 不被本报告消除。

本次交付后停止修改。F1/F2 只需限定报告纠正；经准确新完成门独立核对后，SD00 才能按 §273.3 接收设计交接并签发源码包。
