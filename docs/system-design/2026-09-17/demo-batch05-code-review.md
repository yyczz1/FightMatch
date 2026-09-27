# FM-DEMO-B05-R1 独立代码审查

审查人：R；日期：2026-09-21；依据：冻结 r67 的 [system-task-packets.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:1504) §§93～96、仓库 AGENTS／.agent 审查与验证规则及两包指定设计。审查在两作者原任务均完成、完整交付与联合证据齐备后开始。

| 任务包 | 唯一结论 | 规范与范围 | 规格与行为 |
| --- | --- | --- | --- |
| FM-DEMO-009 | **ACCEPT** | 唯一旧文件差异、10项新文件、内部构造与指定 friend 均符合白名单 | 六项验收均有实现、可失败断言和同版执行证据；仅接受未完成的直接攻击片段 |
| FM-DEMO-021 | **ACCEPT** | 10项新文件；未改旧文件、未消费009新能力或 friend 入口 | 七项验收均满足；仅接受普通来源的隔离库存候选及冻结／结束生命周期 |

未发现本批需修正的问题，无纠正包、缺失证据或当前用户动作。B05满足双包关批条件，由SD00办理状态更新；本报告不授权后续任务实现。

## 1. 审查对象与完成门

- A任务 `01a0c1fb-248d-7321-8b69-9efd26b817f1`；009原turn `01a0c2bc-6642-7933-b942-bac235a5f11d`，工具确认completed，完成于07:36:38 UTC。
- D任务 `01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e`；021原turn `01a0c2bd-1233-76d2-bed5-4d888f50817d`，工具确认completed，完成于07:35:57 UTC。
- 已全文审阅两份交付、scope相关记录、11份本批C#、10份meta；009先还原原字节并核完整前后文本，再审新增实现和测试；021逐份核公开入口、状态、输入壳和测试。
- 未将D的CODE_READY、后续补证消息turn或上一批结果当作本批完成。后续补证只转交已有执行事件，源码／报告／产物未变。
- R只读核验源码／原始工具记录／日志／XML／哈希，并通过任务消息取回已有记录；未运行Unity或测试、未创建任务或子代理。本报告是R本轮唯一文件写入。

| 完整交付 | 实际规模 | SHA256 |
| --- | --- | --- |
| [demo-009-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-009-scope.json) | 1行／189634字节 | `a6c9b98b16516db4aab994bb719e93caf7075cabcb3ba893dab404db9008d9ec` |
| [demo-021-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-021-scope.json) | 5098行／225506字节 | `a9a2f9548fbd131782810df9158b8f58c7286b462e344533a422582be5f162ab` |
| [demo-009-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-009-delivery.md) | 127行／14508字节 | `f50340250a20092d3248ff29e22d4d4d079a772868e684edae3ec1dedba52bbc` |
| [demo-021-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-021-delivery.md) | 93行／10109字节 | `fd5d8cc102cc8ebc011c7cb7bb4fb6b0809c7a24bab5d4de2b49ae4c09b79d24` |

两份delivery均≤180行，scope均≤300KiB；以下结论绑定上述交付及第6节源码指纹。

## 2. 原基线、完整旧文件差异及范围

- 已接收B04来源为 [demo-008-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-008-scope.json) 的 `after.files`：357项，原采样时间 `2026-09-21T05:54:56.6929101Z`；整份scope SHA `7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a`。
- R重新计算357项规范SHA为 `ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0`。A／D的before逐路径与该清单相等，均保留原时间；D另存06:56:16.1804978Z实际startedSnapshot，仍为原357项，无冒充稍后现场。
- R于06:55:41.2481302Z独立捕获357项全同；A于06:55:51.4900728Z记录修改前全同、12个自有新路径不存在、D新路径尚未出现。A初始scope原文前缀还原SHA仍为 `ac7f4a6500230fcc3d5a92321038e7b51130d7560d11f7a942e9d0bff2b0af18`，其before／原字节未被后续证据覆盖。
- 从009 `modifiedOriginalFiles.bytesBase64` 独立解码 [BattleSnapshot.cs:1](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:1) 原6081字节，SHA `f4583c3c6509800557bce1b96b74cbcb6b21da85dfe3ffe506994be98ef825bc`，与B04清单及R早期原字节逐字节一致。
- 完整逐行比较：原154行→当前211行，**+57／−0**，两版均LF；原初始化构造、public属性、枚举及旧逻辑全部保留。新增仅为Snapshot完整内部构造64～82、Board98～104、Member134～140、Enemy165～175、Contribution203～209行及必要注释／空行。
- 新构造显式接收当前修订／计数／HP／游标／累计，不偷偷补0；列表复制成只读，原不可变元素可共享，CarryMode维持Empty。[CoreTestAccess.cs:1](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CoreTestAccess.cs:1)仅三行，对准确程序集 `FightMatch.Core.Tests` 开放internal，无public任意恢复／安装入口；隔离断言见 [CandidateDirectAttackTests.cs:487](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:487)。
- 其余356项原源码／测试／meta SHA全部不变，包括BattleSnapshot.cs.meta（`cf917965123eb036fd00c8b266d565e19e0478e0a8f28d63093dbeec920b98b8`）。当前恰377项，新增恰为双方各10项，无删除或其他改动。
- 009新文件1139行，加旧文件57行共**1196≤1900**；021新文件**1377≤2200**。全Assets由374项增至394项，路径差集恰20个白名单文件；全部新meta为11行MonoImporter，10个GUID在全Assets各出现一次。
- 36份保护文件中33份SHA保持；差异仅SD00已声明的三协调稿。对冻结§§93～96全文比较，仅派发登记及WORKER_RETURNED／REVIEWING状态变化，业务、白名单、验收和验证协议保持；后续设计准备未纳入本批验收。
- 旧B01～B04报告保持；B04报告SHA仍为 `fc1facfd9ba0d30fa24afc02533a071e7191f24cad1ec3ab24d9558726fa56c5`。既有tracked diff仍34文件、+2809／−597，未将已有FlowPuzzle／Packages／本地Claude权限差异归因本批。

## 3. 009 六项验收

| 项 | 独立核查的行为与断言 |
| --- | --- |
| ①固定源攻击 | [CandidateDirectAttack.cs:36](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs:36)将当前几何交001，61行调用真实008；73～93行形成精确伤害及累计。[CandidateDirectAttackTests.cs:21](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:21)覆盖006的L1／L3、两坐标候选：首字0xa15c02b7、合成C=1/4首机会失败；E01 q20/a15/o5，E02 q16/余HP4，来源键／原随机字均断言。 |
| ②真实暴击／精确零 | [CandidateDirectAttack.cs:70](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs:70)只在完整乘除后floor；a=min(当前HP,q)，o=q−a。测试61／77／91／102行分别验证C=1真实触发且0抽字、floor0仍有机会、5/2攻击不提前取整、HP5/2时a5/2/o35/2；无最低1伤害。 |
| ③射程与状态守门 | [CandidateDirectAttack.cs:26](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs:26)先重验数字再核同一Baseline／Ordinal身份／修订，97行起核阶段、完整成员／敌人／贡献键、HP和原定义。55行按存活敌OriginalSlot形成1基序号。[CandidateDirectAttackTests.cs:143](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:143)用明确旧行动后的死前排＋固化线验证跳空前移，反转Pair顺序／改变StableOrder不影响结果；196～327行覆盖死亡、后续面、旧修订、四种非活动阶段、跨基线及错配状态，拒绝均0抽字。 |
| ④几何与原子性 | [CandidateDirectAttack.cs:36](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs:36)只投影PreparedFace和当前固化线，显式空blockedCells沿当前支持域，不读难度float或调用完整solver。[CandidateDirectAttackTests.cs:337](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:337)逐断言001的越界／斜线／自交／他对端点／固化重叠等原因与CellIndex；361行验证局部合法但阻断另一对未来解仍可攻击。拒绝helper逐值核输入HP／累计／PRD不变。 |
| ⑤片段与来源 | [BattleDamageFact.cs:7](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleDamageFact.cs:7)保留Player／Attempt／Face／Operation／修订／行动序号、Actor／Target／Pair、物理effect0、Attack／Defense／m／r／z／q／HP前后／a／o及真实Crit；当前支持域内格挡／盾实际量为0，击杀仅派生。[CandidateCombatFrame.cs:44](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs:44)保留BeforeSnapshot、复制后的行动及完整集合；只替换目标HP、随机和攻击者累计+a。[CandidateDirectAttackTests.cs:116](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:116)核第二次真实攻击保留旧累计16＋4、承伤5、成员HP95、其他敌／游标／原槽；143行杀最后敌仍不推进阶段或修订。 |
| ⑥预算／隔离／验证 | [CandidateDirectAttack.cs:166](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs:166)重验binding、基线全部面、当前状态、意图系数、PRD及大整数／有理分量，复用同一budget.Math和WordsUsed。[CandidateDirectAttackTests.cs:406](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs:406)及433行覆盖保留大值先于坏身份／几何触发Limit；459／475行验证算术／字预算不足无部分结果、已消耗预算不重置、新预算重试逐值相同；487行验证输入与构造列表深隔离、结果只读。第5节106条全部Passed。 |

009保持无主动技能、Empty、Evasion=0的已批准域；同一Baseline引用仅作为纯对象图关联，业务身份仍使用结构键和原字符串。测试的C=1/4／C=1是明示合成输入，未宣称已校准20%暴击或确认源坐标方向。

## 4. 021 七项验收

| 项 | 独立核查的行为与断言 |
| --- | --- |
| ①数量与固定来源 | [CandidateInventory.cs:11](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventory.cs:11)校验并复制定义；37行只创建一名Warrior投影的空候选，T0／StateRevision1／PreferenceRevision1。70行Grant按Settlement＋原Attempt／完整Context／Ordinal规范向量查原收讫，重复返回当前态／原收讫／零增量，再办理新来源；拒绝广告／导入／混合。[CandidateInventoryTests.cs:26](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:26)及41／57／68行验证显式0／空向量、顺序无关防重、冲突先于旧修订、超2^53精确总量及F117→1满叠余18。 |
| ②装配／偏好 | [CandidateInventory.cs:90](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventory.cs:90)无活动冻结才可换装，准确角色／Tactical／Class、L0～99，释放原L后核新池；同物只改L保留Enabled，新选择默认true，明确null清选择。123行按当前选择和PreferenceRevision设置显式目标，无Toggle／操作缓存。[CandidateInventoryTests.cs:82](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:82)及94／401／443行验证不足全旧态、L0保留选择、清除／修订、false重复不反转、同数量Unchanged。 |
| ③入场与Empty | [CandidateInventory.cs:139](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventory.cs:139)校验Player／Attempt／Baseline／完整Context／准确Ready投影；同Attempt先比原事实，另一活动Attempt即便R0也拒绝。C=min(99,L+F)，L0、R=C、T不变。[CandidateInventoryTests.cs:114](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:114)及128／143／307行验证T101/L2→C99/F2、T5/L5→R5，空冻结保留身份并禁换装，活动中Grant4仅F＋4，偏好不改变C／来源。 |
| ④三种结束 | [CandidateInventory.cs:176](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventory.cs:176)核完整结束意图并原子构造；胜利T减(C−U)、L=U、清R后奖励入F；退出T不变、L=C；重来原C／普通池来源／Baseline移交新Attempt、R不先释放、L0、偏好保持。[CandidateInventoryTests.cs:157](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:157)及175／203行验证C5/U3的T3／T7、退出T5及晚到4后的T9、重来不按新增100补C、旧目标禁止再移交、重复结束零增量。 |
| ⑤覆盖与原子性 | [CandidateInventory.cs:204](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventory.cs:204)按原Attempt和EndReceiptId核已办理事实；227行先核奖励收讫冲突，302行要求完整唯一U行、准确角色／Item及0≤U≤C。[CandidateInventoryTests.cs:216](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:216)及230／255／278／291行验证奖励已含不重发、冲突不先扣量，来源余额耗尽仍保留原收讫；缺／多／重复／负／超额U、改结束身份／事实均拒绝。 |
| ⑥隔离／预算 | [CandidateInventoryDefinition.cs:137](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs:137)使用本次budget重验Context、状态／偏好修订、T／L／C／历史奖励与结束U；单一T为当前总量，历史授予量不作第二份余额。所有输入0／false／null的合法含义由可空字段或Has提供标记区分。[CandidateInventoryTests.cs:452](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:452)及463／481／490／508行验证保留大值、晚步骤Limit无Next、新预算重试、深复制、只读集合与根null。未依赖M03私有GrowthChecks或009新friend／攻击API。 |
| ⑦联合验证 | D先冻结5份C#并CODE_READY，A冻结11份后统一编译／测试；R独立重算全部SHA、原1194多重集合及114条021结果，10新项／5个自有GUID和总377项均符合。[CandidateInventoryTests.cs:320](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs:320)明确验证隔离NonEmpty计划仍被既有BattleEntryPreparer以UnsupportedBinding／CarryMode拒绝。 |

普通聚合池键是原Player／ItemId；没有逐广告单位账或持久化。ReadyParticipants仅保存已批准匹配依据，后续M02资格核对、024与M06／H10逐项绑定尚未实现，不能从本包通过推导真实战术携带可用。

## 5. 同版联合执行与XML复核

原始工具证据由R只读会话JSONL独立复读，均属于上述A原009 turn；`read_thread`返回items=[]未被当作已验证记录。A补证只提供定位，R核对了实际command、stdout、状态及shell退出码。

- A原始记录文件：`C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T11-20-56-01a0c1fb-248d-7321-8b69-9efd26b817f1.jsonl`。
- D原始记录文件：`C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T13-25-57-01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e.jsonl`。
- D第590行事件 `exec-e8ce7e31-240e-4a56-9f34-68e2f8d71e3d`：07:19:50.1795326Z冻结5份，规范SHA `08a2ce72db54aa01813ac4c32e5f1dfbb008b069efe4ed90700d2d5b69f554c7`，meta尚未生成。
- A第1141行事件 `exec-73e325a8-7c9c-42eb-ba1b-88755d6929d3`：07:22:55.2286664Z逐份核双方11份C#、356旧项及保存原字节；新meta不存在、Unity进程数0，再冻结11份。冻结SHA `f564d424005fd26e8bb8a889897399e1d46db0f0139880656c6ce972cae16445`，导入前367项SHA `ebfdb3b7b198586289f01f7a8746da9787a0ae26fedd3e0ac25071fa1588c15f`。
- A第1152行事件 `exec-0dc824a4-aff6-413d-94b0-fa86db0b7191` 为编译；第1167行事件 `exec-8bdb4351-6ac9-4416-a10a-2d063748d32f` 为测试。固定Unity2022.3.18f1，均启动前排他检查、Start-Process Hidden、WaitForExit→Refresh→实际Process.ExitCode；测试没有-quit，没有杀进程。
- 原始A turn内仅这两次Unity启动，串行各一次；D未启动Unity。作者查询命令的无匹配／路径通配符／PowerShell语法非零已区别核验，均发生验证前，不是被隐藏的Unity失败。11份冻结C#在启动前、A／D after及R当前检查均一致，无源码修正后复用旧结果。

| 实际执行 | PID | UTC开始→结束 | 实际进程退出码 |
| --- | --- | --- | --- |
| 编译／导入 | 18684 | 07:23:26.6312460→07:23:39.4787147 | 0 |
| 全量EditMode | 50064 | 07:24:40.5365076→07:24:54.9065009 | 0 |

日志参数与原始命令一致。编译日志确认FightMatch.Core／Tests重新编译成功；许可Code10／token提示随后许可更新及entitlements成功，Bee首次ExitCode4说明DAG需重建，后续成功，退出时有Curl42；测试有无图形renderer提示。未发现C# error／warning或编译失败，不将这些环境提示误写成“日志无任何Error”。

| 运行产物 | SHA256 |
| --- | --- |
| [FMDemoB05Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB05Compile.log) | `0e3bfc52b6df82b8acd2e26313ad640eed713b49292704ba12215c733e8fe5fb` |
| [FMDemoB05Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB05Tests.log) | `4387bb8d5749a60f2161fb0a46da446f4a8a3ade7f7ca6e9e6b422ef88fa253b` |
| [FMDemoB05-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemoB05-EditMode.xml) | `6ef269fa7debed42a64ff4afcea40d1b8d6d605766122fd4891894a02795b2b0` |
| [FMDemoB04-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemoB04-EditMode.xml) | `37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95` |

R独立解析XML全部test-case，运行07:24:47Z～07:24:53Z：**1414／1414 Passed，Failed／Skipped／Inconclusive均0**。原1194条（1189个Ordinal fullname）逐名count及Passed次数全部相同；新增仅009 fixture的106条／23方法组、021 fixture的114条／35方法组，无其他新用例混入。

- 原多重集合按Ordinal fullname排序，以 `fullname<TAB>count<LF>`、UTF-8无BOM规范化，B04及B05保留部分SHA均为 `f7a4f366d19e0bae3ca4c6a0f347e805abad4df7640440bb300a070e1e25fbe2`。
- 若加Passed次数为第三列，两版均为 `6c7df5fb5ece94b06042a98c7a60999317b606a6a1593389ccedf533db6579e6`，与D的另一规范算法一致，不混用这两个摘要。
- 保留3个重复fullname：BattleRouteValidator的EndpointMismatch(List)为4条，CandidateCharacterGrowth的CritBase／CritCap对应InvalidDefinition各2条，均Passed；未用简单集合掩盖5条重复。

## 6. 最终源码及meta指纹

路径规范统一为正斜线；按StringComparer.Ordinal排序，将 `path<TAB>小写SHA256<LF>` 编码为UTF-8无BOM后再取SHA256。实际启动测试前、A after、D after及R07:48:29.5608745Z现读377项规范SHA均为：
`8b20dfd85136983b5b1d3db9caf5e3850c471c1b6e26cc660879e96406e72307`。

唯一旧文件当前 [BattleSnapshot.cs:1](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleSnapshot.cs:1)（211行）SHA：
`f511f0801b66f24da81f34f01dbde5d47961da139cc006ca508b3cf3139d2cbb`。

| 归属 | 新文件精确路径 | 行数 | SHA256 |
| --- | --- | --- | --- |
| 009 | `Assets/Scripts/FightMatch/Core/BattleDamageFact.cs` | 65 | `7fe4b33b8999cc63512050c155c757274cb12c7414db165dbff9c4cad1da46d5` |
| 009 | `Assets/Scripts/FightMatch/Core/BattleDamageFact.cs.meta` | 11 | `94d76cbb5cfb76fb2e4b8557a2b7e77fbc1d87dc482b3d4280b70ba4170b1be3` |
| 009 | `Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs` | 98 | `5dd15c88fe760679848d5689630a62a8965c2b7eb7341ec678c97440a5efca42` |
| 009 | `Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs.meta` | 11 | `544171f680ed16205569148a854e79fa40561a389dc7504c5f08ac969b82a978` |
| 009 | `Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs` | 239 | `05fdda41bd6a3a8e1f936e1eea3a4ee9d0a826b27c845238224105e91b1efb5d` |
| 009 | `Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs.meta` | 11 | `db149e9f5d6c4c91a54d1f6c0b72a341c9837692f14c5d745cd44d0ac0d69d22` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventory.cs` | 323 | `75cae49f49283505b8612503a537563c4f0903a067a4dae184aa0b096c8726ef` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventory.cs.meta` | 11 | `469eeaa0373372ce519b023b0ec6cd0d0cefcd33bc3920a809ae2396becbea13` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs` | 154 | `12f003facb2c7907c3122615203c22a2e5683013928bb0ddfe116df78b89900e` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs.meta` | 11 | `0e29966cd1a58cb1ef386f03a999a5aa725c1867f07b15797da7aef79671fb08` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs` | 75 | `75bc2686a425c560def6671c59ed7043fbf2b0d1edee8928ad587edb9966af0b` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs.meta` | 11 | `a3889fa4b90569d03138498686725b422e3e49b4d8e7d7f894609a454699fa71` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs` | 169 | `2c20314e5a4909940a67e89c27dec4856f1851a6b65f2012086e9e7b878646be` |
| 021 | `Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs.meta` | 11 | `7afe1d266cabe832cdf8d5f0d8e3d726963c358523908fdbfa9ef959b53654a7` |
| 009 | `Assets/Scripts/FightMatch/Core/CoreTestAccess.cs` | 3 | `e325eef6629b360e5e621991e6d64fbad7bb560f3c426471b61cf5ac3c84b39f` |
| 009 | `Assets/Scripts/FightMatch/Core/CoreTestAccess.cs.meta` | 11 | `7a2cef447ed0dab563da0a114c9cf7667158a33aaad4f8ad9b154d233387d166` |
| 009 | `Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs` | 679 | `fac0e9e639dff3e7302fe44295dbdc8e25a192a05be34a0e9f617f7292f60a3c` |
| 009 | `Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs.meta` | 11 | `28b00280930443a43c08ece21f14213f43f4f7c7ae631c4c782be6cd4c573449` |
| 021 | `Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs` | 601 | `9a6f2f623b6ac0aa3a6172e22e7c3c3a5d45276da706206b9199561eabb2da83` |
| 021 | `Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs.meta` | 11 | `c4d97d7e3d75d7c82f7bb2806843e601ab2690bb19dbe1ec18d4906a374fd167` |

| 新meta精确路径 | GUID | 全Assets出现次数 |
| --- | --- | --- |
| `Assets/Scripts/FightMatch/Core/CandidateDirectAttack.cs.meta` | `e258bc1a984513747b3f2f50d7369595` | 1 |
| `Assets/Scripts/FightMatch/Core/CandidateCombatFrame.cs.meta` | `1261659daeeebf642a9ce2fdb192aeb2` | 1 |
| `Assets/Scripts/FightMatch/Core/BattleDamageFact.cs.meta` | `6cb712d591df9fc4788162f30691663d` | 1 |
| `Assets/Scripts/FightMatch/Core/CoreTestAccess.cs.meta` | `0e8f2361ac18c3f43b4d185f31a0e187` | 1 |
| `Assets/Tests/EditMode/FightMatch/CandidateDirectAttackTests.cs.meta` | `22d25a0f782dacf4ea529b2750c290b1` | 1 |
| `Assets/Scripts/FightMatch/Core/CandidateInventoryDefinition.cs.meta` | `605d1db34c922c949b9f12590ab25413` | 1 |
| `Assets/Scripts/FightMatch/Core/CandidateInventoryState.cs.meta` | `4a716ecee51a4d24b80795cdeb52330a` | 1 |
| `Assets/Scripts/FightMatch/Core/CandidateInventoryRequests.cs.meta` | `edbde89d738ee2846963ddd81777c6d1` | 1 |
| `Assets/Scripts/FightMatch/Core/CandidateInventory.cs.meta` | `827c5a1225e59be40b1eeebb6d488571` | 1 |
| `Assets/Tests/EditMode/FightMatch/CandidateInventoryTests.cs.meta` | `f4403cbd9d3118241ad05915a8f50f98` | 1 |

## 7. 交接边界

本次证据支持已冻结候选域的正确性与回归。009不包含敌方阶段、线路固化、翻面、完整BattleSnapshot提交或结算；021不包含真实战内U、广告／云／存档，也不放行M06 NonEmpty。Unity验证由A执行，R核验同版实际证据，未声称R重跑。

交SD00：两包结论见首页，无纠正任务／缺证据／用户确认事项；SD00可据此关B05。R不更新三协调稿，不派发或预先验收下一批。
