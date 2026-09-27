# CONT-B 永久请求设计独立审查

VERDICT: NEEDS_FIX

本次审查对象为 §340、§342 授权的设计交付。方案可继续收束，但拟议实施白名单遗漏 v4 阵容视图适配，有限证据集合遗漏固定工具必写的失败文件；两项修正前不能据当前设计签发源码包。以下结论没有把设计或继承的测试通过计作 CONT-B 实现完成。

## 1. 准确完成门与输入身份

| 项目 | 独立核对结果 |
| --- | --- |
| C 原任务 | FightMatch 本机保存与应用接入实现；01a0c403-bfa1-7e90-b503-c0fcd61f23c1 |
| C 本次准确续接 turn | 01a0d8c3-46b9-7590-a675-dbf77eaefe44；原生 context 2026-09-25T13:31:11.493Z；gpt-6-astra / max；正确工程根 |
| C 正式完成 | 原生 task_complete 于 2026-09-25T13:53:56.676Z；非异步正式返回 DESIGN_COMPLETED；completed / idle / error=null |
| R 原任务与本次 turn | FightMatch Demo 独立代码审查；01a0c1cd-dce1-7ac3-8780-06163cb0acfc；turn 01a0d8a3-1cdc-7bd3-a620-bbbd921026e4 |
| R 原生配置 | context 2026-09-25T12:56:03.641Z；gpt-6-astra / max；D:/Unity/UnityProj/FightMatch |
| §340 冻结 | CRLF→LF、标题至下一节前 TrimEnd 加单 LF、UTF-8：8897 字节；70abc387a55a003d770fd2916eee6b90cf1f8aeda69aad4ffc27558e5d416f3d |
| §342 补签冻结 | 同一规范：3195 字节；0f1704a381d2550c85046d6f1e04b8fd25be65ab7d85e31855070c427201921e |

| 正式设计实物 | 字节 / 物理行 | SHA256 |
| --- | ---: | --- |
| [demo-cont-b-design.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design.md) | 44051 / 231 | 2c0fd41083cb855aab71926a3c8301776b486340c1f94d19d81bcaf668e01f9f |
| [demo-cont-b-design-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design-scope.json) | 3022161 / 66955 | 6f33e81444a9f4f07e9ae63d8931ea095e9df4e081fc50a927ed5f639a4cfb0c |

先核本次 C 的原生完成事件、正式返回及上述两实物，再作本报告结论。旧 turn 01a0d8a2-b41c-7272-9b6d-f91225fb11fe 于 13:27:33.571Z 的 BLOCKED 不是完成门；其草案身份和发送被拒经过已在 §342 与当前两稿保留，没有重复等待或重试该发送动作。

TB-READ01 已由 §342 解决。R 仅语义读取 runtime-extension-contracts.md 的 §2 三条公共身份定义及 §7 来源/定额卡条款；全文件只另核 94934 字节 / a4fee0a8cec2e61991570032833685f8e4cd79bb6ac460ddb7e0f4aff04a8479。未读取其余章节或外部链接，未扩展广告、云或跨档权限。R 的原生 completed 时间由本回合正式交付后的生命周期记录确定，本文不预填。

## 2. Issues

### F1 [P2] v4 阵容视图仍被判为未物化，必要模型文件没有实施权限

设计 [第109行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design.md:109) 引入 PublishedPermanentV4=4，第141行要求继续原阵容办理，[第160行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design.md:160) 仅将 PlayerRosterSession.cs 的 v4 适配列入旧文件白名单。

实际 [PlayerSessionModels.cs:27](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerSessionModels.cs:27) 的 sealed PlayerRosterView 将 IsMaterialized 固定定义为 Format == PublishedRosterV3；其第34行直接取 business.Format。[PlayerRosterSession.cs:31](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerRosterSession.cs:31) 直接返回此模型。该模型文件不在 scope.proposedImplementation.modifyExisting 的 30 条中。

因此按当前清单完成合法 v3→v4 后，QueryRoster().Format 为 v4、角色与槽位仍已物化，IsMaterialized 却必为 false。仅放开会话中的格式门不能修复这个 getter；把返回 Format 伪装成 v3 又破坏真实格式查询。依赖原只读接口的 CONT-C 会拿到相互矛盾的物化状态。这是下一实施范围的确定缺口，不是要求改动已接收的 CONT-A 基线。

Required correction：只将 Assets/Scripts/FightMatch/Application/PlayerSessionModels.cs 的 IsMaterialized v4 分类适配加入拟议修改清单，保持原公开属性形状、旧 v2=false/v3=true；登记当前 3439 字节 / 70 行 / SHA256=4a41db0d9ed52f6fb321a58ea5123b1fa0d6aaf9877e177206c9a89fdea7af51、理由与小额增删预算。同步 before/after 路径、受保护余项和 CB16/17/20，不新增生产模块。

Verification required：在现有拟新增测试内明确 v3→v4→保存/对象重建恢复的 QueryRoster 断言：Format=v4、IsMaterialized=true，原 CharacterId、槽位、FormationRevision 不变；旧 v2/v3 分类仍保持。当前设计纠正仅登记该验证，不运行 Unity。

### F2 [P2] 精确证据叶遗漏固定工具的 failure.json

设计 [第183行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design.md:183) 保留原验证工具协议，[第189行](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-cont-b-design.md:189) 与 scope.proposedImplementation.futureEvidenceProposal.runLeaves 却只列每 run 的 11 个叶；该 proposal 的 finiteAllowedRelativeProjectPaths 中 runs/001…012/failure.json 共 12 个路径全部缺席。

实际 [Invoke-FM025P2Validation.ps1:243](D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1:243) 在 compile/tests 不通过时写 failure.json，[第248行](D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1:248) 的异常分支也写该文件以保存 HarnessFailure。未来任何相应失败都会写出当前有限授权集合，和“失败保全、无通配目录授权”同时冲突；不能靠删掉旧失败写入分支解决。

Required correction：每个既定 run 槽位增加准确 failure.json 叶，并同步完整有限路径数组、计数、manifest 覆盖与 CB20；仍只允许 001…012，保留失败、禁止覆盖及槽位复用。无需新目录或提高 260 文件上限。

Verification required：静态逐项比对固定工具的成功、compile/tests 失败及异常分支写入叶，确保新阶段全部包含；manifest 只列实际生成文件且排除自身。此设计阶段不为验证失败叶而运行工具或制造失败。

## 3. 业务与兼容设计核对

H03 使用同一 M02 runtime，M03 记录固定成长/学习效果、M04 记录唯一库存与来源端点、M05 记录教学事件；预览冻结 Player/Character/Head/精确绑定/相关修订/来源区间/成本效果，确认后才生成操作与不可变请求。原操作结果优先于当前余额与准入，同一学习业务键不因新 OperationId 再扣；失败全旧、未知沿原候选/意图/票据/字节续办，未见第二套页面写口。

经验卡沿原 N(L) 求最少整卡并保留完整余量，G01 多角色分账与固定 14 经验旧收益明确；广告优先限同种同 q 的合法已持有来源。SourceSlice/EffectReceipt/ExistingInclusionRef 已映射原发放行、半开逻辑区间、固定效果、受益、绑定、原操作与提交、后继端点和既有包含依据；禁止凭余额重造来源、覆盖后来消费或按比例拆混合不可分来源。真实会话缺合法来源时拒绝，隔离正例不授正式发物权。

学习保留 Lv5、职业/技能、首证受限用途、独立普通证及当前教学依据；一次赠物、学习扣证与教学进度联合提交，说明单独确认。制作按合并输入乘正整数批数及 F 核算，不借 L/R；装配仅无活动尝试时变更，偏好采用明确 Enabled/Revision，L0 与主动关闭保留。活动中合法永久成长/制作/偏好不改原 Stats/HP/三域/Restart 或历史 ActionConditions，未支持已学技能/Mage 在新 H02 的 ID/熵/冻结之前拒绝。

旧 schema1/all-2/v3、FMINT001/002/003、FMPROF01/02/创建锚及原精确内容闭包保持；新向量仅 [4,4,4,4,2,2]、FMINT004 与 fm.player.permanent.v1。v3→v4 显式办理、旧未知先按原格式结束，all-2 先走既有闲时 2→3；新扩展不伪造旧来源。旧 M06/M07 和 F2 初始化仍用原格式；实际首包无卡/证/配方如实 NoPublishedDefinition。F1 是该兼容设计遗漏的具体只读 API 适配。

## 4. CB01～20 设计覆盖

下表评估“方案及拟验证是否覆盖”，不是声称新测试通过。scope 全部 20 项状态均为 NOT_RUN_DESIGN_ONLY；设计阶段遵守无 Unity/产品 DLL/游戏测试执行权。

| 编号 | 独立设计核对 |
| --- | --- |
| CB01 | 真实首包缺定义与合法定义资源不足分开；纯查询/预览、六实物及 298 字节初态保留。 |
| CB02 | 固定 q、最少整卡、跨级余量与非法数值/预算/不足全旧明确。 |
| CB03 | G01：M 用三张 q50 后 M6+22、W 不变；旧 14 经验使 W5+6，再明确学习/说明。 |
| CB04 | CharacterId/Class、其他角色/槽位/恢复隔离及不插入活动明确。 |
| CB05 | 同卡广告优先、原行单位区间、稳定取得次序、防换操作/受益重耗明确。 |
| CB06 | 重复输入合并、批量成本/产出、预算、真实转换和混合不可分边界明确。 |
| CB07 | T/L/R/F 与明确来源选择闭合，不把 UI 叠数当库存上限。 |
| CB08 | 闲时装配、0/99、新物默认开、L0、主动关闭与明确偏好修订有覆盖。 |
| CB09 | 职业/技能/Lv5/明确确认、业务唯一键及新操作零重复成本有覆盖。 |
| CB10 | 首证 W 嘲讽限制、普通证区别、菜单不能绕过与 M05 依据有覆盖。 |
| CB11 | onceKey 赠物、学习/扣证/教学同次提交、说明单独确认和空 Attempt 绑定明确。 |
| CB12 | 不可变预览、逐 owner/端点/修订过时拒绝、有效 V1 不因下载 V2 失效有覆盖。 |
| CB13 | 原操作优先、同 ID 异意图冲突、AlreadyLearned/Unchanged 不重复 owner 效果明确。 |
| CB14 | 各请求 Prepare/Write/Unknown/marker 回执丢失，原候选和字节恰好一次矩阵明确。 |
| CB15 | 对象重建、原绑定缺失阻断、End 不删已成及未确认预览无事务有覆盖。 |
| CB16 | 旧格式/锚/未知续办和 v4 来源规范往返有覆盖；须补 F1 的物化视图迁移/恢复断言。 |
| CB17 | 实际 runtime、F2/未决/S17/活动/演出权限有覆盖；须补 F1 的实际会话查询断言。 |
| CB18 | 长期资料与已冻结战斗/Restart/历史分离、未支持执行能力明确拒绝有覆盖。 |
| CB19 | 普通来源同核正例、广告既有证明 owner 核、真实负例与隔离证据区别明确。 |
| CB20 | 全量原用例、同版 DLL、旧范围和失败保全有安排；精确实施/证据集合受 F1/F2 阻断。 |

## 5. 独立静态验证与继承证据

R 于 2026-09-25T14:01:10.0597928Z 完成只读身份与集合核对：841 精确导出入口中 838 不可变实物、12 份已接收输入及补签文件无差异；三协调稿最新单列。744 实现、774 Assets、409 个唯一旧 GUID 保全；30 个拟改旧文件 before 身份准确，16 个新 .cs/16 个对应 .meta 及未来 CONT-B 证据根均仍缺席。

当前提案预算逐项求和为旧生产 1634 + 新生产 2230 = 3864≤4000，新测试 2530≤2600；工具本次≤40、累计143+40≤183≤200、最终≤320行。预计新增后 776 实现/806 Assets/425 GUID 与 877 精确导出集合计算一致；新增补签文档不在旧 841 内，已单独计入 877。此处是现提案静态算术结果，未消除 F1/F2。

scope 的 36 项 DLL/PDB 已逐项等于 CONT-A 最终 dll-after.json、run010 后、run011 前后及当前实物；744 源码亦沿同一已接收身份。草案引用旧 DLL 基线的问题确已修正，错误经过保留，未修改原证据或重跑 Unity。

| 继承证据 | 独立结果 |
| --- | --- |
| CONT-A root-identity.json | 701 字节；a8d319e2dda4bc878f7a7f9c3949c02e7b1741d6868d212776273173233e30db |
| CONT-A read-manifest.json | 46021 字节；e8d36e282adf58ad19f53d2b1386b7733eef1364f481b85ffc51d67df52e76a2 |
| CONT-A dll-after.json | 6934 字节；4df50035ad334fa96877cf8210587dc351e8b0adb33660675b5f594db20574c0 |
| CONT-A run011/tests.xml | 2746564 字节；497b9048da65fae79bb1c550e70f4e6e580c1b697d8a9882327047e5610ea240；3733 实例全 Passed，0 失败/跳过 |
| 原具名用例集合 | 3728 个不同 fullname、3733 实例；fullname+重数规范集合 SHA256=ac37f55c31f4d154addd11f37573f1d834ce32c786cecee425eb090246381c2c |
| 原固定工具 | 20544 字节/253 行；aa3ad8bc3f052608622c4cb4b7cb8e5f96ede3c67eb498f78a6766e9fbaadae1 |

七根逐项核集合、长度/SHA 与根/manifest：cont-a=202、p2c=166、p2b-publish=116、p2b-prepare-c2=75、p2b-prepare-c1=1442、p2b-prepare=1442、p2a=1454，共 4897 items，无身份或集合差异。这里核验已接收证据，没有执行新的编译/测试，3733 通过也不证明尚未实现的 CB01～20。

## 6. 最小纠正 TASK_PACKET 建议（交 SD00 签发）

目标：仅修复 F1/F2，使当前 CONT-B 设计的实施范围和失败证据集合可执行；不重开业务设计、已接收 CONT-A 或扩展产品范围。本段给出纠正内容，不直接授予作者新回合、源码或执行权限。

- Allowed modify：仅 docs/system-design/2026-09-17/demo-cont-b-design.md 与 demo-cont-b-design-scope.json；仍≤320物理行/4MiB。R 后续仍只修订本唯一设计审查报告≤260行，须另核原 C 新准确完成门。
- F1：加入上述 PlayerSessionModels.cs 精确当前身份、仅 IsMaterialized 接纳 v4 的修改理由，建议增删预算≤4；旧文件30→31、旧生产预算1634→1638、合计3864→3868≤4000。旧公开形状、其他模型及旧测试不改。
- 同步 F1 全部依赖集合：744 中无需改动的原实现714→713，before-text31→32、after-text63→64；补 CB16/17 的实际 QueryRoster 迁移/保存恢复断言与 CB20 范围描述。仍在已有6份新测试/2530提案内安排，若重新分配须逐项列明且总≤2600。
- F2：在 runLeaves 和有限路径数组中准确加 runs/001/failure.json 至 runs/012/failure.json 的12条；每run11→12叶，禁止用通配符代替清单，保留原工具写失败证据协议。
- 两修正合并后应为17根叶+32份before+64份after+12×12个run叶+1份manifest=258条精确候选路径；actual≤260、manifest≤512项及12槽位上限保持。manifest只收实际叶，排除自身。
- 新生产10、新测试6、自然meta16，以及最终776实现/806 Assets/425 GUID/877导出不因新增一个旧文件修改权限而变化；工具累计预算与3733原用例保留要求不变。更新所有重复计数/路径引用，不能只改设计表不改scope。
- Verification required：两稿引用与准确当前身份一致；新增模型属于既有允许读取且只拟改getter；增删/行数/集合算术无漏项；有限证据叶覆盖原工具各失败写入；CB矩阵新增上述断言、全部仍标NOT_RUN_DESIGN_ONLY；复核受保护实物与原输入身份。
- Do not change：源码/测试/meta/资产/工具/证据根、三协调稿、已接收报告、Packages/ProjectSettings/依赖、Git；不执行Unity/产品DLL、不造未来run或哈希、不新增内容/广告/跨档/物理探针，不新建任务或代理。读权限沿§340+§342，不扩目录。
- Formal handoff：C 在原任务准确新回合正式交付两稿、字节/SHA/配置和修正对应关系，原生completed后停改；R随后核该准确门与实物再给唯一正式结论。源码包仍须SD00另签。

当前保持34功能/30已接收/余4/51正向实施计划，CONT-A、025/B17、027接收不受影响。CONT-C/028/029、真实未发布内容、公开AwaitLinks、物理交互/PlayMode/Player/Android等边界继续按既有证据保留。本轮R唯一写入为本报告，不另建纠正文档或修改协调稿。
