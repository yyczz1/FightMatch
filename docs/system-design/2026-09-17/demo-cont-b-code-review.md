# CONT-B 独立代码审查

VERDICT: NEEDS_FIX

本次实施完成门成立；CONT-B 尚不能接收。独立检查确认 4 项 P2 缺陷，涉及必须执行的来源优先规则、明确清空装配、M03 规范排序及学习业务唯一键。012/013 编译测试通过和文件保护成立，不消除这些代码反例。CONT-A/025/B17/027 不重开，计数保持 34 / 30 / 4 / 51。

## 1. 准确回合与完成门

- 依据：冻结 §350、§351 及本次最小补签 §353；不是此前 BLOCKED/中断回合、设计 ACCEPT 或文件先出现。
- C 原任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；本次 turn：`01a0e09d-d68a-73c3-a25a-4ad32024a662`；原生 context：2026-09-27T02:07:15.669Z。
- R 原任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；本次 turn：`01a0e09e-0753-76b2-b6a5-4b95829c453f`；原生 context：2026-09-27T02:11:47.563Z。
- 两回合原生记录均为 `gpt-6-astra / max`，cwd=`D:/Unity/UnityProj/FightMatch`。
- 已独立核 C 本次原生 `task_complete`：2026-09-27T03:25:16.217Z，准确 turn 匹配；wait_threads 为 completed/idle/error=null，非异步 final 为 COMPLETED，声明停改。
- 03:25:56Z 完成四份正式实物身份核对后开始结论性代码审查；03:43:40.641566Z 再核最终源码、保护、证据和同版链，issues=[]。
- R 自身自然 completed 只能在本报告正式返回后由原生事件核验；此处不预填未来完成时间。

| 正式输入（工程根下相对路径） | bytes | SHA256 |
|---|---:|---|
| docs/system-design/2026-09-17/demo-cont-b-delivery.md | 13734（152 行） | 3fb958cf1fa64494a90b8f7f96d71a58209804a427f0bcf1f2b62895842e7158 |
| docs/system-design/2026-09-17/demo-cont-b-scope.json | 4279599 | 1dba63a87ca3828e68f8c5ca17252a42154c4bf7a2830c10f4005a7cd99a0b9a |
| TestArtifacts/FMDemoCONT/cont-b/root-identity.json | 723 | 61d65e1fe0c4b93a2b23784f6276bfebe4a36c76b5f493264b2b8d9cf7fc4854 |
| TestArtifacts/FMDemoCONT/cont-b/read-manifest.json | 49187 | 9626ae349f37da18b7fb109e6be12a461d97e31b5210240ba1dd3301ae73df9d |

冻结 §350 规范化 24904 bytes / `0dbc27d8d828bff99ac32d4514ec9c531da94d5d35655b90d6a296a96f571bb1`；§353 3174 bytes / `fd72b4c8cbbd07bf6dc51849baa4557095d2ff009a2170926d055fbcbddd9ab1`。均独立按 CRLF→LF、标题至下一节前 TrimEnd+单 LF、UTF-8 重算。
C1 设计：48374 bytes / `f16b638408a4ae7ee3361302b92c4c68a3f45dad3ec059c539643863e7cdca01`；C1 scope：3040284 bytes / `40076c0a359215ea6b83a221c067460e49b6246d22cbd58df0ed8fdcb495ef12`；原 C1 设计审查：11053 bytes / `9d371f6b832f2c0751a9413bd4e36639dac3ca17167bd77f61504b23a5827c62`。设计接收身份保持，不能代替本次实现审查。

## 2. 必须纠正的问题

以下均为已核源码分支、接口契约与断言范围所得的静态反例；R 未运行 Unity、产品 DLL 或新红测，不把静态推演伪装成已执行失败用例。路径和行号对应上述最终交付实物。

### R1 · P2：显式 SelectedInputs 绕过必须执行的消费优先规则

主位置：[CandidatePermanentInventory.cs:117](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentInventory.cs:117)；相关：[CandidatePermanentProgression.cs:47](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentProgression.cs:47)。
Select 原先从全部合法 Held 计算 available，但显式输入会整体替换该集合；广告优先随后仅排序这个子集。PreferTeachingCertificate 又在 SelectedInputs 非空时直接跳过首证优先。因此显式输入仍可合法但违反强制业务分配。
反例 A：复用现有 ExistingOwnerFixture，W4+157、同 q 的 G1+N1、目标5；SelectedInputs 只给 N 的一个 Held 单位。Preview/Build 会耗 N、留下 G。CB05 要求该场景只能耗 G；隔离广告证据边界不改变同一 owner 核应执行的优先政策。
反例 B：W5、教学已赠首证且未说明、另普通证1；Learn("W") 的 SelectedInputs 只给普通证。用途筛选允许普通证，Preview/Build 会耗普通证并推进学习，首证留下；CB10 明确要求合法 W 学习优先准确首证。
ValidatePreview/Build 均重放 quote.Draft 的相同显式选择，未重建完整合法集合的强制分配。Progression.CheckHistory 也只核首证用途，不核应优先而被绕开的首证。
现 InventoryTests:176–197 与 ProgressionTests:75–99 只证明未显式选择时的优先；未覆盖这两项反例。需在预览与冻结校验中核显式份额符合强制优先，保留 CB07 合法材料选择和 CB12 相邻区间规范合并。

### R2 · P2：合法的明确清空装配只能预览，不能确认与持久恢复

主位置：[CandidatePermanentCodec.cs:116](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentCodec.cs:116)；相关：[CandidateApplicationReferences.cs:588](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs:588)。
C1 §2 的 Equip(HasItem,ItemId,L) 复用既有 Equip 语义；CandidateInventory.cs:113–145 允许明确 ItemId=null、L=0，并把 Enabled 置 null。这与“仍选择 weapon、L=0”是两个合法且不同的目标。
新永久入口给 Equip/CharacterId=W/DefinitionId=null/Quantity=0 时，CheckLoadout:223–224 调既有 Equip，Preview 可通过；但确认的 SameQuote 和意图编码一律用非 optional 的 f.Text 写 DefinitionId，报 MissingField，无法创建可保存的合法清空请求。
即使只放宽该字段编码，OwnerHistory:587–589 在旧物品非空→null 时仍生成 Enabled=true，与实际 Equip 的 Enabled=null 不符，后续恢复核对会拒绝。
现 InventoryTests:148–169 只测 weapon/L=0 保留选择，没有测明确空选择。需按种类严格编码已批准的空装配语义，修复历史重建，并覆盖有物品→空槽及空槽→空槽 Unchanged；非 Equip 的 DefinitionId 仍必须存在。

### R3 · P2：M03 永久效果列表的非规范顺序未被拒绝

主位置：[CandidatePermanentCodec.cs:191](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidatePermanentCodec.cs:191)。
C1 §7 明确 M03 追加记录按 CharacterId 排序，新集合非规范顺序拒绝。ApplyFrozen:105–108 生成时按 CharacterId/OperationId 排序；读写 Effects 却只检查 OperationId 不重复，不核顺序。
CandidateRosterSaveCodec:28 以 owner=3 调此方法；Roster 构造函数只复制列表，Roster.Check 和 BusinessRestoreChecks.Permanent 都未补顺序约束。DecodePublished 的 CanonicalLayout 比较重写同一顺序，不能补救。
反例：取两个角色各一条完整合法卡效果，保留条目及 M02/M04 引用，只逆序 M03 列表后按合法 envelope 重封装。当前读写保留该逆序且所有逐条引用仍相等，同一语义因此存在多份可接受的 M03 布局，违反 CB16 的不规范列表拒绝要求。
现 CB16 往返、混合 schema、字段篡改用例未覆盖 M03 顺序。应仅对 owner=3 核生成器既定顺序，另测同角色 OperationId 顺序；不得为了此修复把 M04/M05 的历史消费顺序统一改成字典序。

### R4 · P2：恢复只按 OperationId 防重，未核角色与技能业务唯一键

主位置：[CandidateBusinessRestoreChecks.cs:253](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs:253)；相关：[CandidateApplicationReferences.cs:267](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs:267)。
正常 Build 的 CandidatePermanentGrowth.Apply:88–90 用 FindLearning 防重复学习；恢复 M03 时却仅 seen.Add(OperationId)，逐条检查职业、经验和对应成本，没有核 (CharacterId,SkillId) 唯一。
反例形状：无教学的 Mage/spell 有两份独立普通证，两条不同 OperationId 的 Applied LearnSkill 使用同一角色/技能，OriginalLearningOperation 均 null，分别消费不同证；第二条修订、前后经验、原头与 M02/M04 引用均合法接续。上述局部检查不会拒绝这个重复业务键。
PermanentRecord 只有 OriginalLearningOperation 非空时才查原学习；OwnerHistory:527–575 仅在 TeachingLevel 非空时跟踪教学键，普通 Mage 学习没有学习集合校验。Transition 还用 ApplyFrozen 重建 M03，不能借用正常 Apply 的重复检查。
因此恢复不能保证 CB09 所要求的技能业务唯一键；不能仅以两个操作 ID 不同接受两次 Applied/扣证。现 ProgressionTests:34–49 测正常第二请求自动变 AlreadyLearned，没有损坏历史反例。
需在完整恢复链核学习业务键只出现一次 Applied，后续操作只能引用该原结果且零成本、零新 owner 效果；增加两条独立普通成本、不同操作 ID 的恢复拒绝用例，保持原保存全旧。

## 3. 逐项核对（不是复制作者 PASS）

已逐份实读 10 个新生产、6 个新测试及 31 个旧生产的实际差异，另核工具和 §353 旧测试例外。以下行号为测试方法/断言定位；多条标准共用用例，具名命中数不能相加当作测试总数。
测试简称 G/I/P/X/S 分别为 CandidatePermanentGrowthTests / CandidatePermanentInventoryTests / CandidatePermanentProgressionTests / CandidatePermanentProtocolTests / PlayerPermanentSessionTests，均在 Assets/Tests/EditMode/FightMatch/。

| ID | 独立核对结果与证据 |
|---|---|
| CB01 | 实际首包 Query/Preview 无卡/技能/配方定义；合法隔离定义零量 InsufficientResources。S:17、X:250；查询/预览无 ID/熵/保存，六首包和298字节配方保护成立。 |
| CB02 | G:16/31/47/54/74；q50 两个指定结果、q7/q500、多跨级/余量、目标/定义/预算拒绝；原 Need/Accumulate 整数公式复用，Mage 独立定义不放宽旧 Warrior 工厂。 |
| CB03 | G:88–113；G01 三卡全给 M→M6+22，W 保持157、卡0/首证1；原 M07 旧关14→W5+6；明确学习、说明及 reopen，未自动补级/返卡。 |
| CB04 | G:117/136；身份/职业、未参战与恢复中成长，其他角色/阵容/恢复期/活动基线保持；已核非首角色路径。 |
| CB05 | 区间[0,2)/[2,3)、证明字段、耗尽与重复/错误端点拒绝用例和默认广告优先通过；显式选择可绕过优先，R1 阻断。I:26/47/55/176/208/223/360、X:300。 |
| CB06 | I:98/119/232/255；重复输入合并、多批/非正数/预算、120产出、前序链/混合产卡拒绝广告重标；数量与转换端点共用正式核。 |
| CB07 | I:131/338/379；T/L/R/F、已分配/冻结不借用、材料多源要求明确选择及冻结；R1 修复须保留这项合法选择能力。 |
| CB08 | I:148 的99→0保留选择及主动关通过；明确清空目标确认/恢复缺口见 R2，不能把这项表为完整通过。 |
| CB09 | P:12/34；等级/职业/明确确认、正常新ID AlreadyLearned 零成本和修订不变通过；恢复业务唯一键缺口 R4 阻断。 |
| CB10 | P:54/122；法师禁首证、独立普通证正常花、教学依据篡改拒绝；默认 W 首证优先通过，显式选择绕过见 R1。 |
| CB11 | P:75/103；赠物/学习/说明独立 once、空 Attempt 原整关、同提交与已学原引用、已耗首证不返；受 R1 影响的错误来源分配另列。 |
| CB12 | I:286、X:17/37/201/250；列表复制、相邻份额合并、头/绑定/修订/目标/原来源字段过时拒绝；旧 Inclusion 不覆盖已耗事实。优先选择漏洞由 R1 单列。 |
| CB13 | X:59；旧ID原结果先于当前余额/头等检查，同ID异意图拒绝；AlreadyLearned/Unchanged 无重复 owner 效果。损坏历史唯一键见 R4。 |
| CB14 | X:107/142 的7种请求×4写入故障及7个 Prepare 预算拒绝共35项通过；逐项核原 candidate/ticket/bytes、完整旧/新头和一次性。正例属共用 Core/M12 内存链；未宣称真实首包每种功能已开放。 |
| CB15 | X:165、S:106；预览丢弃无事务，真实 runtime 的迁移未知、重建/ResumeObserved/Resolve/End 复用原结果；原共享 runtime 的 lookup-before-write 与冻结槽代码保持。 |
| CB16 | schema/F2锚/迁移、规范引用往返和字段篡改已核；v4 QueryRoster 真实分类通过。M03非规范列表及重复学习历史分别被 R3/R4 阻断，空装配亦需 R2。I:74/308/322、P:122、X:182/219/271、S:49/191/227。 |
| CB17 | S:49 的 idle/active/playback 实际 v3→v4→重建；原身份/三槽/阵容修订，F2/保存/S17 门及演出 token 保持；v4 不伪装 v3。 |
| CB18 | G:169、S:152；原 stats/HP/U/Scene/RNG/重开冻结基线不回写，回退保当前偏好；Mage/未支持已学技能 H02 在 ID/熵/冻结前拒绝。 |
| CB19 | X:300、S:171；合法定义→原 M07→共用 Build/Propose/codec/M12，广告仅 owner 核隔离；正式无认证来源索引拒绝，任意 builder 回调0，未造 M10 审定/发奖权限。 |
| CB20 | 012 Compile→013 无filter全量、3733旧+118新、同版链、逐文件预算/副本/保护/全部失败证据独立核对通过；§353 例外后的712/812/33/65/272准确，详见下节。 |

G01 特核：CandidatePermanentTestData:65–71 的 BaseExperience=20、DamageWeight=1、TakenWeight=1/4、CurveBase/CurveLog=1/2、ReferenceHpDivisor=2、LevelPenaltyBase=3/4、OverlevelGrace=1；不是恒定14发奖。test:materials 与 test:L1 分离，后者无额外材料/卡。实际断言走原 CandidateBaseRewards 并得到14。
CA18 特核：PlayerRosterSessionTests:202–203 仅10→14及新增 fm.player.permanent.v1，before 16870 bytes / `0693d2c08cda6f781849501eb31424f6d6ed6cf00a3f87ad223c4e796b736e18`，after 16896 bytes / `e8f4351b6029084bf08c2461e61fbe41cbf55ef0c26471130b4d85cb74ce869a`。UnsupportedBinding、callback=0、存储不变及其他旧断言保持。
CB16 读取器修正特核：X:285/293 使用真实流长度；013 已覆盖正确字段往返和受益职业篡改拒绝。但这些通过不能证明缺失的 R3/R4 列表/业务唯一键校验。

## 4. 运行与范围证据

- 012 Compile：PID17316，2026-09-27T02:09:39.8228300Z→02:10:15.3847958Z，exit0、35.5619658s，无编译错误。
- 013 完整 EditMode：PID16424，02:11:09.7377216Z→03:21:49.8765577Z，exit0、4240.1388361s；argv 无 filter，Tests 无 quit；执行前 Unity 进程集合为空，串行先编译。
- 013 XML：2842709 bytes / `ab1de23bc26f4adc70084dfe0144523a90b4838ff43f35158d6b489565240550`；实际3851 passed、0 failed、0 skipped、0 inconclusive。
- 原3733 occurrence/3728 fullname 的多重集合保持，SHA256=`ac37f55c31f4d154addd11f37573f1d834ce32c786cecee425eb090246381c2c`；与旧 XML 独立重算比对，无删除/跳过替代。
- 新118项：G13、I34、P6、X52、S13，全部在最终 XML 通过；六个新测试文件中的第六个是共享夹具，不另算 fixture。
- 012 after = 013 before = 013 after = 当前：776实现、806 Assets、36 DLL/PDB 的 bytes/SHA逐项一致；仅读取 DLL/PDB 元数据。工具当前21835 bytes / `edcabaf8f655f55f8ceb467dae8f18bf852b16ca41f7dceaefc6e44cde655f2d`，运行前后相同。
- 31个旧生产白名单实际改30个，CandidateApplicationModels.cs 未改；独立按 C1 原字节副本计算逐行增删，不使用作者预填数字代替。
- 33 before / 65 after 与对应原文/最终实物逐字节一致；409原meta不变，16自然新meta合法且425 GUID唯一；776/806/880精确集合无额外条目。
- 848原保护输入扣33授权可变项及3协调稿，812不可变项零差异；744原实现扣31生产及1旧测试例外，712项未改。原/C1六设计报告均保持。
- 七历史根仅按既有 manifest 的4897精确项核实际bytes/SHA，根身份/manifest也保持；原001～011及失败证据未删除、覆盖或冒充最终门。
- §353有限路径集合272项与独立派生一致；本根实际219文件，其中manifest列218项、排除自身；不以未生成叶补齐，不越260实际文件/512 manifest项/4MiB限制。
- 工具现有13槽无复用；成功、编译/测试失败及catch路径的固定写入叶均落在已签集合，failure.json 覆盖保持，其他Stage与SHA/进程安全分支未被本包放宽。

| 预算项 | 实际 / 已签上限 |
|---|---:|
| 旧生产增删 | 731 / 1638 |
| 10新生产物理行 | 1863 / 2230 |
| 合计生产 | 2594 / 3868（硬4000） |
| 6新测试物理行 | 1737 / 2530（硬2600） |
| 工具本包增删 | 24增+6删=30 / 40 |
| 工具累计增删、物理行 | 173 / 183（绝对200）；271 / 320 |
| §353旧CA18例外 | 2增+2删=4，仅两条期望 |
| 逐文件上限 | 所有最终文件均在各自上限内 |

## 5. CONT-B-CODE-C1 最小纠正包

目标：只消除 R1～R4，保持本次已核通过的行为及全部原交付/失败证据；仍由原 C 实施、原 R 独立复核，不重开其他已接收模块。
此为范围明确的纠正建议；现13个运行槽已用尽，部分文件接近逐文件上限。须由 SD00 按原协调流程复签纠正的精确逐文件预算、有限证据路径/运行槽及正式报告路径后执行；本报告不授权复用旧槽、覆盖原delivery/scope/root或自行加run014。
建议源码白名单仅下列8项；无新生产/测试文件、meta、配置、依赖、资产、权限或 Git 操作。若实际修复确需其他文件，先报告具体阻塞与最小增补，不顺带扩展。

| 工程根下准确路径 | 唯一允许目的 |
|---|---|
| Assets/Scripts/FightMatch/Core/CandidatePermanentInventory.cs | R1在完整合法Held集合上约束显式强制优先，保留材料选择/区间身份。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentProgression.cs | R1首证优先不被显式输入绕过，原用途/once限制保持。 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentCodec.cs | R2按kind表示合法空装配；R3只核M03既定规范顺序，旧schema字节保持。 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs | R4恢复时核M03角色/技能唯一Applied业务键。 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs | R2空槽Enabled=null历史；R4后续学习必须引用原结果且无新成本/owner效果。 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentInventoryTests.cs | R1广告显式绕过拒绝、R2明确清空与Unchanged/恢复；保留L0选中及材料显式选择正例。 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProgressionTests.cs | R1首证显式绕过拒绝；R4同角色/技能不同操作重复Applied恢复拒绝及正常AlreadyLearned。 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProtocolTests.cs | R2引用/编码恢复，R3乱序/重复拒绝，R4完整M02/M03/M04一致但业务键重复的恢复反例。 |

最小验收：
1. G1+N1用1、G1+N2用2的显式非优先选择不能保存；合法规范显式份额/相邻拆分与未显式默认结果一致。W首证+普通证场景不能显式先花普通；法师仍可花独立普通证。
2. 有物品→明确空槽全流程保存/重建得到ItemId=null/L0/Enabled=null；空槽重复为Unchanged；同物品L0仍保留ItemId及主动关，非Equip缺Definition拒绝。
3. v4 M03 两角色顺序、同角色既定OperationId顺序不规范均拒绝，规范排序往返逐字节相同；M04/M05消费历史不得被重新排序。
4. 两条不同操作、相同角色/技能的Applied学习即使成本端点独立、修订/引用一致也拒绝恢复；合法原学习+AlreadyLearned保留原引用、成本0、无新owner效果。
5. 新反例先给可复核的失败证据，再在固定最终源码上按复签槽串行Compile→无filter完整EditMode；保留原3851具名多重集合及全部旧断言，新例单列，最终源码/Assets/DLL同版。
6. C正式交付给逐文件原文diff/预算、精确证据manifest、全部运行/失败、两份报告外部身份及准确自然completed；R再核实际回合与完成门，只审本纠正增量及必要回归。

不得通过删/跳过测试、降低规范性、取消明确空选择、限制所有显式材料输入或把历史效果清空来修复。首包/M10/广告准入/真实跨档/新技能战斗支持不进入本纠正。

## 6. 审查边界与唯一写入

真实首包仍无卡/技能/配方/教学16定义；正例的合法隔离定义和M12内存存储不冒充内容审定或真实玩家功能全部开放。广告已有证明只核同一owner，不授予真实会话写权。
公开 AwaitLinks、物理鼠标/触控/像素/交互式PlayMode、Player/Android、未覆盖保存故障与§184 NOT VERIFIED保持；本审查不以绿测清除这些边界。
R没有运行Unity/产品DLL、修改源码/工具/原报告/协调稿、调用外部模型、创建任务/代理、写Git或向其他任务发消息；五分钟自动跟进保持取消。
R本次唯一新增文件是本报告 `docs/system-design/2026-09-17/demo-cont-b-code-review.md`，首次写入前不存在；本报告身份在final外部给出，不把自身哈希写入自身。
