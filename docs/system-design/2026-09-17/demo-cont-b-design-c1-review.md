# CONT-B 设计 C1 独立审查

VERDICT: ACCEPT

Scope: PASS — 仅接收 §346 的 F1/F2 设计纠正及派生集合。
Acceptance criteria: PASS — 两项原缺口均在最终设计、精确清单与拟验证中闭合。
Verification: 原生准确完成门、两 C1 实物、原设计差异、字段/路径集合/预算与受保护身份独立静态核对通过。
Notes: 本结论是设计 C1 接收；源码实施仍须 SD00 另签，CB01～20 尚未执行，不计 CONT-B 功能交付。

## 1. 准确完成门与唯一输入

| 项目 | 已核对身份 |
| --- | --- |
| C 原任务 | FightMatch 本机保存与应用接入实现；01a0c403-bfa1-7e90-b503-c0fcd61f23c1 |
| C 本次准确 turn | 01a0d8eb-a0ce-76e1-abd2-638a12f23543；原生 context 2026-09-25T14:15:16.037Z；gpt-6-astra / max；D:/Unity/UnityProj/FightMatch |
| C 完成门 | 原生 task_complete 2026-09-25T14:26:05.790Z；非异步正式返回 DESIGN_COMPLETED；completed / idle / error=null |
| R 原任务 | FightMatch Demo 独立代码审查；01a0c1cd-dce1-7ac3-8780-06163cb0acfc |
| R 本次准确 turn | 01a0d8ec-c177-77a2-b35c-64653da5604b；原生 context 2026-09-25T14:16:29.901Z；gpt-6-astra / max；同一正确工程根 |
| §346 / §347 冻结 | CRLF→LF，§346 标题至 §347 前 TrimEnd 加单 LF，UTF-8；4985 字节；2430da7b0bfee8e7823db8c1c5a2c47e295115ff7dfeafeacf680aff7dad0318 |

| 本次正式 C1 实物 | 字节 / 物理行 | SHA256 |
| --- | ---: | --- |
| [demo-cont-b-design-c1.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md) | 48374 / 242 | f16b638408a4ae7ee3361302b92c4c68a3f45dad3ec059c539643863e7cdca01 |
| [demo-cont-b-design-c1-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1-scope.json) | 3040284 / 67382 | 40076c0a359215ea6b83a221c067460e49b6246d22cbd58df0ed8fdcb495ef12 |

R 先通过 wait_threads 核本次准确 completed，再读对应原生 task_complete 的正式返回并核上述实物；没有以旧 NEEDS_FIX、旧 BLOCKED 或文件提前存在替代完成门。报告于本回合正式 final 交付，R 的自然 completed 由其后原生生命周期记录确定，不在文档内预填。

## 2. F1：v4 阵容物化分类设计已闭合

C1 [第121行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md:121) 明确真实 Format=v4，同时让原 IsMaterialized 接纳 PublishedPermanentV4；旧 v2=false/v3=true、公开属性形状和其他模型保持。[第165行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md:165) 和 scope.proposedImplementation.modifyExisting 已准确加入唯一所需的 PlayerSessionModels.cs，增删≤4行。

独立对照现行 [PlayerSessionModels.cs:27](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerSessionModels.cs:27) 与 [PlayerRosterSession.cs:31](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerRosterSession.cs:31)，直接修正 getter 能消除 QueryRoster 的格式/物化状态矛盾，无需伪装 v3 或增加另一查询模型。模型当前 3439 字节/70行/SHA256=4a41db0d9ed52f6fb321a58ea5123b1fa0d6aaf9877e177206c9a89fdea7af51，C1 所绑 before 与实物一致，源码尚未修改。

CB16/17 的 [第225～226行起点](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md:225) 与对应 JSON 均明确：合法 v3→v4→实际持久提交→对象重建恢复，通过真实 PlayerSession.QueryRoster 核 Format=v4、IsMaterialized=true，原 CharacterId、三个槽及 FormationRevision 不变，并保留旧 v2/v3 分类。该用例在已有 PlayerPermanentSessionTests.cs 内与 CB16 共享；其 450 行上限和六份新测试合计 2530 行保持，没有新增测试文件或重跑旧矩阵。

31 个拟改旧生产精确等于原 30 个加此模型；原 30 条路径、理由、before 身份和预算逐条未变。模型的 before/after 两路径真实进入对应数组，旧预算1634+4=1638、总生产1638+2230=3868、原744实现中其余713项保全，CB20 同步。F1 不再有未列入清单的必要修改。

## 3. F2：固定工具所有失败叶已进入有限集合

C1 [第194～196行起点](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-c1.md:194)、scope.proposedImplementation.futureEvidenceProposal 的 runLeaves、failureLeafPaths、finiteAllowedRelativeProjectPaths 及 designCorrections.F2 同步列出 001～012 的12条准确 failure.json，逐条唯一且无遗漏。

独立静态核对原固定工具的真实写入路径：

| 原工具分支 | 叶及依据 | C1 覆盖 |
| --- | --- | --- |
| 共同运行记录 | before.json、run.json、after.json、result.json；第207/214/222/242行 | 全部逐槽列入 |
| Compile 输出 | compile.log、compile.stdout.txt、compile.stderr.txt；第183～186/205/212行 | 全部逐槽列入 |
| Tests 输出 | tests.log、tests.stdout.txt、tests.stderr.txt、tests.xml；第183～187/191/205/212行 | 全部逐槽列入 |
| 编译/测试不通过 | [第243行](D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1:243) 写 failure.json，第245行 exit 1 | 全部逐槽列入，原失败语义保留 |
| try 内异常 | [第248行起](D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1:248) 必要时写 HarnessFailure，第252行重新抛出 | 全部逐槽列入，原异常语义保留 |

每槽12叶是成功/失败分支允许写入的并集；preflight 仍保留原实际行为，不要求补造原来不会生成的文件。manifest 仅列实际叶、排除自身，失败不覆盖、编号不复用；260上限没有扩成允许清单以外两条任意路径。原工具保持20544字节/253行/SHA256=aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1，未运行工具或人为制造失败。

R 独立生成“旧244条+模型两份副本+12个失败叶”集合，和 C1 完整数组相等；再按每槽真实12类叶生成集合，结果仍相等且无重复。F2 的机器可读范围与文字承诺一致。

## 4. 派生范围、预算与保护集合

| 检查项 | 独立计算与最终 C1 值 |
| --- | --- |
| 当前精确保护入口 | 845=原841+原CONT-B三正式报告+来源补签1；三协调稿最新单列，842项不可变 |
| 拟改旧生产 | 31，逐项预算1638；其中新增模型≤4行 |
| 拟新增生产/测试/meta | 10/6/16；准确路径、自然meta配对与原建议一致，目前32个新实物仍缺席 |
| 生产总预算 | 1638+2230=3868≤4000；逐文件上限同时成立 |
| 新测试预算 | 六份逐文件上限均保持，合计2530≤2600 |
| 原实现未改项 | 744−31=713 |
| 工具预算 | 仅固定CONT-B Compile/Tests建议；本次≤40，累计143+40≤183≤200，最终≤320行；旧阶段/SHA/进程协议保持 |
| 原文副本 | 32份before、64份after；精确等于拟改旧生产+工具，以及其后增加32个新源码/meta |
| 有限证据候选集合 | 17根叶+32before+64after+12×12run叶+1manifest=258；actual≤260、manifest≤512、槽位001～012 |
| 未来实现/Assets/GUID | 776/806/425；一个已有文件增加修改权限不增加文件或GUID数量 |
| 未来保护导出 | 880=原841+原设计3+C1设计3+补签1+新增32；集合准确、无重复 |
| 六份设计报告 | futureDesignGateFiles 与上述原/C1各三份精确一致，且全部包含在未来880导出中 |
| 有限读取路径表 | 原340条加原CONT-B三报告=343；meta只核元数据，SourceSlice文档仍仅原批准段落，原缺席路径保持缺席 |

未来实施 delivery/scope/唯一 code-review 的既有路径未改变，本轮未创建它们或 CONT-B 证据根。C1 scope 没有自嵌自身 SHA、预填作者 completed 时间或独立结论；其 ownOutputs 与正式返回均指向本次两份新 C1 报告。

## 5. 其他设计保持与独立验证

对照原最终设计正文，新增/替换内容限 F1/F2、其预算/集合/验证，以及本C1身份和完成门。JSON 的 interfaces、serialization、baselineTests、inheritedFiniteEvidence、priorBlockedDesign 与原稿相同；10个新生产、16个自然meta、原工具修改预算、未来实现/Assets集合和实施报告路径均保持。六份新测试只有 PlayerPermanentSessionTests.cs 的责任说明增加 CB16，文件路径与上限不变。

20项验收中只有 CB16、CB17、CB20 的内容因本纠正增加；其余17项逐项相同，全部仍为 NOT_RUN_DESIGN_ONLY。原H03拥有者、成本/来源、SourceSlice/EffectReceipt/已有包含依据、原操作优先/业务防重、未知续办、活动与Restart基线、v4格式及旧档/F2兼容的已审设计没有重新解释或扩展；原三正式稿及其历史结论只读保留。

R 于2026-09-25T14:28:56.0256312Z完成本轮独立只读核验：842不可变项、原12份已接收输入、三份原CONT-B正式报告、来源补签、31份拟改旧文件before、744实现/774Assets/409唯一旧GUID均准确，无路径集合或身份差异。36项DLL/PDB等于CONT-A最终dll-after.json、run010后/run011前后及当前实物，744源码同一基线；未执行DLL。

| 保留实物 | 字节 / SHA256 |
| --- | --- |
| 原CONT-B设计 | 44051；2c0fd41083cb855aab71926a3c8301776b486340c1f94d19d81bcaf668e01f9f |
| 原CONT-B scope | 3022161；6f33e81444a9f4f07e9ae63d8931ea095e9df4e081fc50a927ed5f639a4cfb0c |
| 原NEEDS_FIX报告 | 15785；0451147a5f372b3c63642173060a04079dc4d415baba8725af31d1c6dc95c529 |
| 来源补签文档 | 94934；a4fee0a8cec2e61991570032833685f8e4cd79bb6ac460ddb7e0f4aff04a8479 |
| CONT-A最终dll-after.json | 6934；4df50035ad334fa96877cf8210587dc351e8b0adb33660675b5f594db20574c0 |
| CONT-A run011/tests.xml | 2746564；497b9048da65fae79bb1c550e70f4e6e580c1b697d8a9882327047e5610ea240 |

七根的根/manifest和全部4897个item逐项核路径、长度/SHA通过：cont-a202、p2c166、p2b-publish116、p2b-prepare-c2 75、p2b-prepare-c1 1442、p2b-prepare1442、p2a1454，原失败记录保留。读取原XML确认3733实例全Passed、0失败/跳过、3728个不同fullname；具名重数集合SHA256=ac37f55c31f4d154addd11f37573f1d834ce32c786cecee425eb090246381c2c。此为继承证据核对，没有重跑有效Unity验证，也不证明未来新增实现已通过。

## 6. 结论边界

F1/F2 已在本 C1 设计中闭合，无待纠正的实质设计缺口。后续可由 SD00 据本次准确两稿与唯一审查结果另签源码任务；实施后仍须按已设计的实际会话断言、固定工具和完整3733原用例加新增用例取证，不能用本报告替代代码审查。

本轮R唯一新写本报告，没有改原三正式设计稿、协调稿、源码/测试/资产/meta/工具/证据/Git，没有新建任务或代理、没有运行Unity/产品DLL或恢复自动跟进。当前34功能/30已接收/余4/51计划交付保持；CONT-A/025/B17/027接收及CONT-C/028/029、真实内容、公开AwaitLinks、物理交互/PlayMode/Player/Android等既定边界不变。
