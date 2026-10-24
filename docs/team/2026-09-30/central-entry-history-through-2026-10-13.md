# Central entry snapshots before simplification (2026-10-13)

Historical evidence only. Old approvals and current-status labels do not authorize execution. Relative links inside each snapshot retain their original file context.

## START_HERE.md

````markdown
# FightMatch 当前开发接续入口

更新：2026-10-13。用户已要求继续开发；“任务暂停？”是状态询问，没有取消继续指令。当前没有活动 Goal 对象（最近 get_goal 为 null），不新增定时任务。**首 Demo 尚未完成。**

## 1. 当前工作与下一步

2026-10-13 最新执行授权：用户确认按简化方案继续，复用已有结果、补必要检查后推进游戏接入，可人工验收，不逐步等待确认。中央回合 `01a13404-21d1-7262-abae-9d52295727c2`，人类消息 `01a13404-223c-7893-b069-dae699e44e19`。主程已接续[标准方案](docs/team/2026-09-30/engineering.md)：LOC 补隔离副本重复键检查，随后接通本地化/语言偏好/资源与 uGUI Host；主程接收组内结果并继续，C 串行执行必要 Unity 验证，代码仍交 GitHub PR 审查。旧 M17/FIX11 不恢复；不用旧自制验收或逐次激活约束阻断本轮已授权技术工作。以下为历史。

2026-10-13 用户最新纠偏：停止继续扩展 LOC 的 M17/FIX11 自制还原与验收链，改查官方发布包、标准构建和必要的人工验收。主程已确认 LOC 回合 `01a13358-c501-72f3-8eb8-2d8b40e6ff43` 结束，无在途工具或残留进程；FIX11 的目录缺失失败保留，未补跑、封存或开启新 native。整体开发未被用户暂停。后续按[官方常规路线研究](docs/team/2026-09-30/research-luban-standard-install-2026-10-13.md)收敛方案；工具可运行、许可证资料与游戏实际验收分别处理，不再把修旧校验器作为下一默认任务。以下为历史。

2026-10-13 当前：用户新授权的FIX10/M13已实际执行216.202724秒。首restore成功（6.029369秒）；第二因包缓存临时文件FileNotFoundError退出143，第三未启动。0下载、378保护项未变、已知进程结束、无manifest；中央实核原工具exec-eeefca3c-5a72-4e15-8810-696c9cbeee51与完整回执，唯一[native结论NEEDS_FIX](docs/team/2026-09-30/loc-m17-m13-native-verdict.json)。FIX10源码7be28523的GitHub审查与51项检查保留。主程actual01a1332f-61b9-7e53-a24b-d5451446635a正在按[本次诊断包](docs/team/2026-09-30/central-loc-m13-diagnosis.md)组内定位和最小修正；不再重复native接收，不自动第二次还原。近期实际启动共10轮；更早13材料准备/校验批次另列，详见[次数及原因](docs/team/2026-09-30/central-loc-execution-history-2026-10-13.md)。首Demo未完成，B/L、Unity未运行。以下为历史。

2026-10-12 M11当前结果：用户本轮直接批准修正版再执行一次，0下载/外置盘三次restore/总900秒，中央唯一收件。LOC actual `01a12fee-8c94-7a01-944a-13a3c4de81e3`实际执行127.410236秒；第一条restore exit0（6.523466秒），随后锁文件NuGet contentHash校验失败，后两条未启动。312项保护输入未变、已知进程结束、无材料manifest；中央实核原工具exec-98056bdf-a6e4-4c95-9c06-7c79ad25e4de与完整回执完全一致，结论[NEEDS_FIX](docs/team/2026-09-30/loc-m17-m11-native-verdict.json)。主程已确认归档全文SHA512误当NuGet contentHash；原包/feed/cache同字节。具体补丁见[唯一诊断](docs/team/2026-09-30/engineering-loc-m11-diagnosis.md)。修正的必要子集至少278847B，超原278528B上限319B，完整绑定尚未计入；FIX09未写driver、未跑静态/封存。自动审批拒绝将上限加4KiB，要求用户单独授权；原范围内一次简化判断未找到保留全部保护的直接办法。待用户决定有界调整（提案≤280KiB，其余资源预算不变）；没有第二次实际执行许可。

2026-10-12 M10已接续：用户当前要求继续之前卡住的任务，中央actual01a12f63-745a-70e0-8fd5-d5256a39e1fb／人类item01a12f63-7521-75c0-897e-2bdff545c277；LOC actual01a12f65-de4c-7901-a66a-9c6b58cd7c33。FIX08准确head a0bfe700已源码接收，新A/H仅填既审模板CONFIG，执行见 central-loc-m17-m10-execute.md。限定一次0下载、最多三restore、完整900秒；中央为本次有限执行唯一收件者。已实际启动host并于5.021159秒exit1，因旧OS工具编号回执不匹配在预检停止；restore0、下载0、312保护项不变、已知进程静止。中央实核原工具exec-69b3ec4e-284f-41fa-9edb-31ba86c5d175，loc-m17-m10-native-verdict.json为NEEDS_FIX；主程actual01a12f6b-6305-7281-9d8a-ebf6dcdaa1e5已完成组内派发及接收：仅更正回执中一个字符，真实consume_source/verify_activation静态启动检查5.746348秒通过；详细证据见engineering-loc-m10-correction-receipt.json。更正与证据已推PR2提交489b28dc62adaa24242c72e4e9e6bf2117fd11a8，源代码仍为已审a0bfe700，无新增源码审查。尚无成功还原结果；M10一次额度已使用，下一次明确提案见[修正后还原提案](docs/team/2026-09-30/central-loc-fix08-m11-proposal.md)。没有另启host，M09和M10失败保留。

2026-10-12 M09：用户对FIX07一次零下载/三restore/900秒提案直接答“执行”。中央actual `01a12ee5-5e38-7863-b291-95e758e24669` / 人类item `01a12ee5-62af-73d3-a5b5-c3827b86dfba`；LOC actual `01a12ee6-b260-7693-9f5f-ecf014c0482d`，中央唯一收件。FIX07源码head `bd92f6bcedaf89751256a4a7e0084d39cd244893`已源码限定接收；新A/H只填既审模板CONFIG，八叶与侧录已鲜核。按 `central-loc-m17-m09-execute.md`已实际执行失败：128.818624秒/host1，首restore6.342396秒/exit1，Luban.Gdscript项目根GUID.tmp被拒（非obj），后两条未启动；0下载、284保护不变、已知进程静止、manifest缺席。中央实核原工具exec-6240c5ef-e6bf-4d38-af3b-48716e784015；QA actual01a12eed-ac9b-76d3-a7b3-d71dd17569b9及机械归因修正01a12eef-83b2-7072-985d-508d1a59c592已接收，中央 `loc-m17-m09-native-verdict.json` 为NEEDS_FIX。一次额度已用；主程诊断actual01a12ef5-0026-7140-92cf-f98d0cb0d364已确认遗漏49项目根规则；LOC actual01a12efd-cd09-7972-8b24-057c07dd2217按 `central-loc-m17-fix08-source.md`实施98父目录覆盖与限定272KiB新driver，42/42静态差量完整调用7.008589秒、一次封存14.938371秒均通过，无新增restore。侧录记录者actual01a12f16-9330-7332-8381-e66f9fd33a61；中央实核原工具与八文件，31新文件已推PR2 head a0bfe7007be65eed6d1afda317934195cb6ebbd6、1883旧blob不变；trigger6120173614，主测试actual01a12f20-e81f-75f3-b746-85ebb8ee1906已接回GitHub完成结果6120235084，准确head未报新重大问题，非正式APPROVED；历史FIX01 P2保留。中央 loc-m17-fix08-source-verdict.json 为源码限定ACCEPT。新完整还原方案见 central-loc-fix08-next-material-proposal.md，尚未另获一次授权。独立B入口静态方案也已完成，见 `engineering-loc-fix07-next-b-plan.md`，材料尚未接收不启动B。M7/W7固定清单已捕获到 `central-loc-m09-partial-capture.json`（3701文件/1328目录）；旧证据保持。

2026-10-12 M08新授权：用户明确允许修正版再执行一次，复用全部归档、0新增下载、外置盘三次restore、总900秒、中央收件。中央actual `01a12e46-6af4-7b82-958d-d18c1146d85d` / 人类item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`；LOC actual `01a12e47-da22-7b32-a9be-046bc0919c4b`。FIX06源码及GitHub审查已接收，head `454eb4da250262a396b17da97d83c71de408bd48`；新A/H已按既审模板仅填CONFIG并绑定本次身份/准确预算/侧录，执行包 `central-loc-m17-m08-execute.md`。M08已实际失败：129.057504秒/host1，首restore5.987138秒/exit1，Luban.DataLoader.Builtin/obj/GUID.tmp仍被拒，后两条未启动；0GET、244保护项不变、已知进程静止、manifest缺席。中央实核原工具，QA actual `01a12e4f-7cf9-7b33-ba5a-390682fbecb2`已核收，中央 `loc-m17-m08-native-verdict.json` 为NEEDS_FIX。主程 actual `01a12e4f-69be-72e2-b3ef-ca33e7c4303b`已完成6个真实OS微型探针（约0.157秒）：生产计数重复表达被拒，等价展开成功，见 `engineering-loc-after-m08-failure.md`。FIX07 原作者 actual `01a12e5d-f4c9-7443-87c9-3eb4d3f47ce4`完成3个实际OS回归；首次aggregate因中央库存摘要JSON格式失败，旧记录保存。repair作者/封存执行者 actual `01a12e6f-1e7a-7732-a9ac-871c55a04271`按 `central-loc-m17-fix07-repair01.md`修正引用并复用OS证据，16/16差量6.432883秒及封存11.298968秒均exit0，未新增native/restore。中央实核原工具及八叶，45文件已推PR2 head `bd92f6bcedaf89751256a4a7e0084d39cd244893`、1838旧blob不变；trigger6118099176，主测试actual `01a12e80-9746-7f73-a30c-8b802e2f5f26`已接回GitHub完成结果6118132100：准确head未报新重大问题、非正式APPROVED；历史FIX01的P2缓存预检线程仍保留，当前FIX07为0GET范围。中央 `loc-m17-fix07-source-verdict.json` 为源码限定ACCEPT。下一次0GET/三restore/900秒方案见 `central-loc-fix07-next-material-proposal.md`；完整还原一次额度已用，尚未另启。M6/W6/E08及M07等全部旧证据保留。

2026-10-12 M07新授权：用户明确允许再执行一次，复用全部下载文件、零新增下载、外置盘三次restore、最长900秒，中央收件。中央actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类item `01a12dc8-5fb9-7e43-8659-7026352c2050`；LOC执行actual `01a12dca-5f12-7d21-8c65-31ac1d70c49e`。已按REVIEW02准确head01a5ab7a生成新A/H，仅填写既审host模板CONFIG；sourceCommandReceipt已绑定。执行包为 `docs/team/2026-09-30/central-loc-m17-m07-execute.md`，新外置M5/W5与E07四叶；M07已实际失败：115.929秒/host1，首个runner restore在Luban.Protobuf/obj/*.tmp写入时Operation not permitted，8.011秒/exit1，后两次未启动，下载0、保护216项未变、manifest缺席。QA actual `01a12dd3-79cc-7321-bae8-0545a149914e`已核收，中央 `loc-m17-m07-native-verdict.json` 为NEEDS_FIX；主程actual `01a12dd4-ca41-70d0-b5a7-e8528d83e67c`已完成最小方案；LOC actual `01a12dde-888b-7591-bb45-0a3576b1b5c5`已完成FIX06：49个图内obj目录的窄GUID.tmp规则、55/55一次4.638493秒、封存一次8.009652秒，均exit0。中央实核两条原工具/八叶与侧录，29文件已推PR2 head `454eb4da250262a396b17da97d83c71de408bd48`（1809旧blob不变）；trigger6116642338，QA actual `01a12def-6bc4-79c1-b89e-06044c8487ad`已接回GitHub完成结果6116669351：准确head未报major，非正式APPROVED。中央 `loc-m17-fix06-source-verdict.json` 为源码限定ACCEPT；M07仍NEEDS_FIX，无新native运行。下一次0GET/三restore/900秒方案已准备在 `central-loc-fix06-next-material-proposal.md`；本轮一次真实运行许可已使用，需新的明确许可才启动。原执行已知进程静止，补充默认沙箱ps被拒单独记录；旧M06及有效验证保留。

2026-10-12 接续：M06-resume1 已在正常工具审批后实际启动，随后回合被用户中断。原 LOC actual `01a12998-5f4d-7a03-83c3-c5670144e1cc`、工具 `exec-177da352-5c1b-4a9e-a39e-82082f70591f` 于2026-10-11T06:15:23.066145Z记录 HOST_STARTED/PID60792；工具中断的-1不是DUT退出码，尚无HOST_EXITED或最终材料收据。13个GET已有成功、进程静止收据；M4/W4存在，三次restore没有完成证据。中央本日只读ps确认原PID缺席。中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613` / 用户“继续任务”item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`：LOC actual `01a12bfe-0633-7001-9613-18780415e1a1`已完成中断核收，13归档9014649B实核一致、123保护项未变；QA actual `01a12c02-06c4-7c52-bc23-0ea3c637a286`已接收，中央 `loc-m17-m06-native-verdict.json` 为 NEEDS_FIX。FIX05 SOURCE已完成：准确源码作者actual `01a12c14-512d-7331-8acc-fd240ca1de4a`，封签执行actual `01a12c19-313d-7803-a6e2-07f57152e9d6`；81/81差量检查2.256秒通过，格式修正后封签4.916秒通过，原身份/格式失败记录保留。PR2新head `c4c7a5d313c1193191447b525ade226268b616e1` 已推送并核对26文件，trigger6111991590，QA actual `01a12c1f-be8b-70e1-9ba6-f932a8f01031`已接回两项P1（4242623018授权绑定、4242623020成功工具原记录）；中央 `loc-m17-fix05-source-verdict.json` 为NEEDS_FIX。旧成功工具原文已归档；LOC actual `01a12c28-d01f-7731-a6e9-51fc7ac522f4`已完成REVIEW01：15/15新检查5.351秒、封签4.738秒，外层完整工具回执已发布且匹配最终八叶。PR2新head `5c12ddabfc784927a936b428d9f1b90638cf108e`，trigger6112203396，复审返回新P1 4242690950：sidecar要求尚未在材料启动入口强制校验。中央REVIEW01源码NEEDS_FIX。主程已完成REVIEW02启动门设计；LOC actual `01a12c3f-ebd9-70d0-92f0-74666adab242`已实现并通过109/109新检查2.033秒、单次封签4.842秒。中央实核原工具及八叶/侧录身份，13文件推至PR2 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`，trigger6112501532，主测试actual `01a12c51-ea21-7f22-ad4c-ba6deab027a0`已接回GitHub完成结果6112531539：准确head无major，非正式APPROVED。中央REVIEW02源码限定ACCEPT，并在回复修正依据后关闭三条历史P1线程；原F5/R1证据保留。全归档复用、0GET候选尚未进入材料执行；下次材料提案已绑定新SOURCE与外层回执，源码审查已接收，M07的新一次材料执行许可现已取得，执行状态见本页顶部，中央唯一收件。保留partial根及旧原始证据，不重启旧host或重置一次执行/900秒额度。FIX04源码ACCEPT保持，材料M仍未接收。

M06原授权：用户允许一次复用已下载文件、补齐13个官方归档约9MB、外置盘3次依赖还原、最长15分钟，中央读取结果；授权actual `01a1298d-4509-7132-8e0d-9f433010ad0a` / item `01a1298d-4589-7d93-a269-75e3a3e1fc05`。首次审批超时发生在CreateProcess之前，其证据保留；随后resume1已真正启动，当前不再是旧审批超时阻塞。

2026-10-11：M05 已实际执行失败，不是工具审批拒绝。首次 owner 只生成 before 后结束，未启动；同次 resume1 实际 owner `01a12931-9e6f-79b1-ac3d-ab9ce3ab4ac0`、原工具 `exec-4f9e3510-3f50-4e1d-b33f-f2c983b48133` 获准执行，42.369108秒、OS1、`unsafe relative path`；5个官方GET共16158115正文B，restore0。旧74项保护对象不变，原日志和partial外置W保留；没有重试、B/L或Unity。中央actual `01a12926-663f-78a0-bee9-38392d8c0f5e` / 人类item `01a12926-6966-77b3-9e90-961521142cee` 仍为本次直接授权来源。QA机械接收已完成，中央 `loc-m17-m05-native-verdict.json` 记 NEEDS_FIX。主程已完成方案，定位到既存 Scriban 7.2.5 两条内部双斜线；LOC actual `01a1293b-3a74-7512-8a09-09b17fda765d` 已完成FIX04 SOURCE候选：66/66定向检查1.481秒，机械封签一次不到4秒，19包705项覆盖及五包复用。PR2新head `816263eaacf4f3e276c0cef7703c5824af086cc6` 已推送并逐18文件验证；trigger6105553315，GitHub结果6105581700已完成，QA actual `01a12948-fc42-7e33-905f-b6275bc02511` 已接收，中央 `loc-m17-fix04-source-verdict.json` 为源码限定ACCEPT。当时没有新材料执行；该13GET/9014649B+3restore/900秒提案现已获M06明确授权，最新状态见上。离线定位不冒充原堆栈路径。不会把历史通过或SOURCE接收当M通过。

LOC M04 已实际执行并失败：3.646504秒，`unsafe relative path`，下载0、依赖还原0。工具正常批准，旧审批阻塞已解除；用户明确的一次重试已用完，不能自动再跑。QA已核收，见 [失败结论](docs/team/2026-09-30/loc-m17-m04-native-verdict.json)。FIX03源码已推PR2 head `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`，GitHub结果6055811693已完成，中央源码限定ACCEPT；限定两个官方ZIP别名及新外置m17-fix03根。26项检查通过，完整SOURCE准备5.1037秒的0.1037秒超时已记录。该提案已由2026-10-11用户“继续任务”在明确提问上下文中接续为一次M05，详见当日授权记录和 `central-loc-m17-m05-execute.md`；M05实际失败见上，不再等待旧确认卡。

资源 V08 已完成12程序集编译及准确89项测试（89通过、0失败/跳过）；整轮因 SDK ADB 历史绑定未退役导致监护失败，164.364642秒，含准备324.364642秒。P1035和43项编译文件已恢复，closure=true、owned空。保留有效编译/测试证据，不默认重跑整套。ADB监护修正已推PR20 head `23bcfd10e7478a7a0ab275d3532e883e5805e908`，42项离线一次0.155085秒通过，GitHub结果6055530291已由QA接回，中央源码限定ACCEPT；原第二候选身份未记录，不能声称当时有两个同时存活的合法服务器。见 [原生结论](docs/team/2026-09-30/resource-combined-v08-native-verdict.json)。QA已补核AS06/AS07真实IO与链接断言：源码/本轮OSX程序集/XML对应成立；空目录是合同规定的清理结果，不证明真实进程重启。

本轮中央 actual `01a11a60-d890-7660-b7ee-1accdb3ffe0c`；用户授权 item `01a11a60-d8c2-71f0-b832-a4e0c8db237e`；LOC实际 `01a11a62-0e7c-7553-aa47-c16cd6a276b9`，C实际 `01a11a65-5b5a-7ac2-a8bf-fdcbec225ff1`。PR20已审旧源head `a360ad864171a3b49f7785ab7c797028d583d5c0`／GitHub6054341623仅关闭其源门；后续修改需新的准确head审查。

先读[团队快照](docs/team/2026-09-30/README.md)和最新明确派发包。仍缺正式 LOC、真实资源包/正常 Host 交互、新 uGUI 安卓包、实际玩法与冷启动/设备验收。云同步仍因 proxy:8080 失败，未进行云测试。普通源码工作继续，没有全项目暂停。

此前状态逐字节保存于[历史](START_HERE.history-before-m04-v08-results.md)，不得从其中恢复旧额度。

## 2. 执行规则

[AGENTS.md](AGENTS.md)是规则入口；另读[项目事实](.agent/PROJECT_CONTEXT.md)和[团队执行约定](.agent/TEAM_WORKFLOW.md)。中央分发/接收，五位负责人分工；仅 C 串行运行本机 Unity。代码审查只用 GitHub PR Code Review，自动化测试优先 dots；当前云环境缺就绪证据，本机验证遵守具体包的次数、时间、输入及恢复范围。

普通已授权技术修正继续完成，不逐包重问用户。仅在确需用户决定或工具审批实际拒绝时说明具体缺口；不要把历史授权当新工具批准，也不要把一个独立阻塞扩成全项目暂停。每包一个完成接收者，使用完成事件/等待工具接回结果后继续下一就绪包。

默认主要角色 `gpt-6-astra/xhigh`，明确机械任务可用 Luna。保留有效证据，源码固定后只做必要验证；新增失败/修复/源码变化才追加。旧 N07/N10/N11/I01 的 native 额度不可复用。

## 3. 产品与环境边界

Demo 与正式安卓产品共用代码、规则、存档和正式流程，只减少内容。运行时 UI 用 uGUI，Editor 用 UI Toolkit。英/简中、预制体诊断占位符、Luban 表格和自有本地化绑定、YooAsset 路线不变。美术继续使用占位资源，未授权购买新素材。

当前项目在外置盘，Unity2022.3.18f1 Intel。2022.3.62f3 仅是发行准备候选，未升级。Android7/API24、AdMob 单来源、R2 等批准设计不等于已实施/验证；首版网络范围及后置账号/云档见 AGENTS 和角色背景。

本机 master 基线125b849及既有 WIP 保留。审查分支已有独立推送，PR 均仍 draft 未合并；不要在脏工作区 pull/reset，不自动合并、发布、部署或购买服务。具体 Git 写权限沿用户已授权范围和任务包。

## 4. 角色背景与历史

需要背景时按职责读取：[系统设计](docs/migration/2026-09-28/SYSTEM_DESIGN_CONTEXT.md)、[架构](docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md)、[策划](docs/migration/2026-09-28/PLANNING_CONTEXT.md)。日常接包不重读全部历史；实际 thread/host/turn 在团队快照及任务回执登记。

原迁移启动模板、旧第4节授权文本、历史暂停和验收记录均逐字节保存在 [2026-10-08 接续入口历史](START_HERE.history-2026-10-08.md)。它们是历史证据，不是新的派发或执行指令。本页整理入口，没有改变用户决定、扩大实施范围或重新启动旧任务。

````

## .agent/PROJECT_CONTEXT.md

````markdown
# FightMatch project context

Updated2026-10-13. Start with [START_HERE](../START_HERE.md), [current team snapshot](../docs/team/2026-09-30/README.md) and the latest explicitly assigned packet. The user requested continued work; their pause-status question did not cancel it. No active Goal object was observed. First Demo acceptance is incomplete.

## Observed facts

Active authorization (2026-10-13): the user approved the simplified route, reuse of existing results, necessary remaining checks, then actual game integration, including manual acceptance where appropriate, without per-step confirmation. Central turn `01a13404-21d1-7262-abae-9d52295727c2`, human item `01a13404-223c-7893-b069-dae699e44e19`. Engineering owns ordinary dispatch/receipt and continuation; C remains the serial Unity executor. See the current engineering index. Old M17/FIX11 is retired from the execution path; actual runtime evidence and GitHub PR review remain required for their respective claims.

Latest user direction (2026-10-13): stop expanding the bespoke LOC M17/FIX11 restore/validation chain; investigate official binaries and ordinary build/use instructions, with manual acceptance where appropriate. Engineering confirmed LOC turn `01a13358-c501-72f3-8eb8-2d8b40e6ff43` completed with no outstanding tools/processes; no static rerun, sealing, native restore or download followed. This is a technical course correction, not a project pause. See [standard-route research](../docs/team/2026-09-30/research-luban-standard-install-2026-10-13.md). Historical failures remain evidence, not instructions to resume the old chain.

Current: FIX10/M13 ran once in216.203s. Runner restore succeeded; tests-generation restore exited143 after a missing transient cache file; third restore not started. Zero downloads,378 protected identities unchanged, known processes closed; M_NATIVE NEEDS_FIX. Exact FIX10 source review remains valid. Engineering owns bounded diagnosis/minimal correction per central-loc-m13-diagnosis.md; no second native attempt is authorized. See START_HERE and the execution-history report.

M10于5.021159秒因旧OS工具编号回执抄错一个字符失败，restore0/GET0。主程已接收更正及5.746348秒真实静态启动检查；记录更正已推PR2提交489b28dc，已审源码a0bfe700不变。材料仍未接收，没有新host；下一次还原提案见[该方案](../docs/team/2026-09-30/central-loc-fix08-m11-proposal.md)。此前全部失败保持历史，不复用一次额度。

2026-10-12 M09：用户对FIX07一次零下载/三restore/900秒提案直接答“执行”。中央actual `01a12ee5-5e38-7863-b291-95e758e24669` / 人类item `01a12ee5-62af-73d3-a5b5-c3827b86dfba`；LOC actual `01a12ee6-b260-7693-9f5f-ecf014c0482d`，中央唯一收件。FIX07源码head `bd92f6bcedaf89751256a4a7e0084d39cd244893`已源码限定接收；新A/H只填既审模板CONFIG，八叶与侧录已鲜核。按 `central-loc-m17-m09-execute.md`已实际执行失败：128.818624秒/host1，首restore6.342396秒/exit1，Luban.Gdscript项目根GUID.tmp被拒（非obj），后两条未启动；0下载、284保护不变、已知进程静止、manifest缺席。中央实核原工具exec-6240c5ef-e6bf-4d38-af3b-48716e784015；QA actual01a12eed-ac9b-76d3-a7b3-d71dd17569b9及机械归因修正01a12eef-83b2-7072-985d-508d1a59c592已接收，中央 `loc-m17-m09-native-verdict.json` 为NEEDS_FIX。一次额度已用；主程诊断actual01a12ef5-0026-7140-92cf-f98d0cb0d364已确认遗漏49项目根规则；LOC actual01a12efd-cd09-7972-8b24-057c07dd2217按 `central-loc-m17-fix08-source.md`实施98父目录覆盖与限定272KiB新driver，42/42静态差量完整调用7.008589秒、一次封存14.938371秒均通过，无新增restore。侧录记录者actual01a12f16-9330-7332-8381-e66f9fd33a61；中央实核原工具与八文件，31新文件已推PR2 head a0bfe7007be65eed6d1afda317934195cb6ebbd6、1883旧blob不变；trigger6120173614，主测试actual01a12f20-e81f-75f3-b746-85ebb8ee1906已接回GitHub完成结果6120235084，准确head未报新重大问题，非正式APPROVED；历史FIX01 P2保留。中央 loc-m17-fix08-source-verdict.json 为源码限定ACCEPT。新完整还原方案见 central-loc-fix08-next-material-proposal.md，尚未另获一次授权。独立B入口静态方案也已完成，见 `engineering-loc-fix07-next-b-plan.md`，材料尚未接收不启动B。M7/W7固定清单已捕获到 `central-loc-m09-partial-capture.json`（3701文件/1328目录）；旧证据保持。

2026-10-12 M08新授权：用户明确允许修正版再执行一次，复用全部归档、0新增下载、外置盘三次restore、总900秒、中央收件。中央actual `01a12e46-6af4-7b82-958d-d18c1146d85d` / 人类item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`；LOC actual `01a12e47-da22-7b32-a9be-046bc0919c4b`。FIX06源码及GitHub审查已接收，head `454eb4da250262a396b17da97d83c71de408bd48`；新A/H已按既审模板仅填CONFIG并绑定本次身份/准确预算/侧录，执行包 `central-loc-m17-m08-execute.md`。M08已实际失败：129.057504秒/host1，首restore5.987138秒/exit1，Luban.DataLoader.Builtin/obj/GUID.tmp仍被拒，后两条未启动；0GET、244保护项不变、已知进程静止、manifest缺席。中央实核原工具，QA actual `01a12e4f-7cf9-7b33-ba5a-390682fbecb2`已核收，中央 `loc-m17-m08-native-verdict.json` 为NEEDS_FIX。主程 actual `01a12e4f-69be-72e2-b3ef-ca33e7c4303b`已完成6个真实OS微型探针（约0.157秒）：生产计数重复表达被拒，等价展开成功，见 `engineering-loc-after-m08-failure.md`。FIX07 原作者 actual `01a12e5d-f4c9-7443-87c9-3eb4d3f47ce4`完成3个实际OS回归；首次aggregate因中央库存摘要JSON格式失败，旧记录保存。repair作者/封存执行者 actual `01a12e6f-1e7a-7732-a9ac-871c55a04271`按 `central-loc-m17-fix07-repair01.md`修正引用并复用OS证据，16/16差量6.432883秒及封存11.298968秒均exit0，未新增native/restore。中央实核原工具及八叶，45文件已推PR2 head `bd92f6bcedaf89751256a4a7e0084d39cd244893`、1838旧blob不变；trigger6118099176，主测试actual `01a12e80-9746-7f73-a30c-8b802e2f5f26`已接回GitHub完成结果6118132100：准确head未报新重大问题、非正式APPROVED；历史FIX01的P2缓存预检线程仍保留，当前FIX07为0GET范围。中央 `loc-m17-fix07-source-verdict.json` 为源码限定ACCEPT。下一次0GET/三restore/900秒方案见 `central-loc-fix07-next-material-proposal.md`；完整还原一次额度已用，尚未另启。M6/W6/E08及M07等全部旧证据保留。

2026-10-12 M07新授权：用户明确允许再执行一次，复用全部下载文件、零新增下载、外置盘三次restore、最长900秒，中央收件。中央actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类item `01a12dc8-5fb9-7e43-8659-7026352c2050`；LOC执行actual `01a12dca-5f12-7d21-8c65-31ac1d70c49e`。已按REVIEW02准确head01a5ab7a生成新A/H，仅填写既审host模板CONFIG；sourceCommandReceipt已绑定。执行包为 `docs/team/2026-09-30/central-loc-m17-m07-execute.md`，新外置M5/W5与E07四叶；M07已实际失败：115.929秒/host1，首个runner restore在Luban.Protobuf/obj/*.tmp写入时Operation not permitted，8.011秒/exit1，后两次未启动，下载0、保护216项未变、manifest缺席。QA actual `01a12dd3-79cc-7321-bae8-0545a149914e`已核收，中央 `loc-m17-m07-native-verdict.json` 为NEEDS_FIX；主程actual `01a12dd4-ca41-70d0-b5a7-e8528d83e67c`已完成最小方案；LOC actual `01a12dde-888b-7591-bb45-0a3576b1b5c5`已完成FIX06：49个图内obj目录的窄GUID.tmp规则、55/55一次4.638493秒、封存一次8.009652秒，均exit0。中央实核两条原工具/八叶与侧录，29文件已推PR2 head `454eb4da250262a396b17da97d83c71de408bd48`（1809旧blob不变）；trigger6116642338，QA actual `01a12def-6bc4-79c1-b89e-06044c8487ad`已接回GitHub完成结果6116669351：准确head未报major，非正式APPROVED。中央 `loc-m17-fix06-source-verdict.json` 为源码限定ACCEPT；M07仍NEEDS_FIX，无新native运行。下一次0GET/三restore/900秒方案已准备在 `central-loc-fix06-next-material-proposal.md`；本轮一次真实运行许可已使用，需新的明确许可才启动。原执行已知进程静止，补充默认沙箱ps被拒单独记录；旧M06及有效验证保留。

2026-10-12 接续：M06-resume1 已在正常工具审批后实际启动，随后回合被用户中断。原 LOC actual `01a12998-5f4d-7a03-83c3-c5670144e1cc`、工具 `exec-177da352-5c1b-4a9e-a39e-82082f70591f` 于2026-10-11T06:15:23.066145Z记录 HOST_STARTED/PID60792；工具中断的-1不是DUT退出码，尚无HOST_EXITED或最终材料收据。13个GET已有成功、进程静止收据；M4/W4存在，三次restore没有完成证据。中央本日只读ps确认原PID缺席。中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613` / 用户“继续任务”item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`：LOC actual `01a12bfe-0633-7001-9613-18780415e1a1`已完成中断核收，13归档9014649B实核一致、123保护项未变；QA actual `01a12c02-06c4-7c52-bc23-0ea3c637a286`已接收，中央 `loc-m17-m06-native-verdict.json` 为 NEEDS_FIX。FIX05 SOURCE已完成：准确源码作者actual `01a12c14-512d-7331-8acc-fd240ca1de4a`，封签执行actual `01a12c19-313d-7803-a6e2-07f57152e9d6`；81/81差量检查2.256秒通过，格式修正后封签4.916秒通过，原身份/格式失败记录保留。PR2新head `c4c7a5d313c1193191447b525ade226268b616e1` 已推送并核对26文件，trigger6111991590，QA actual `01a12c1f-be8b-70e1-9ba6-f932a8f01031`已接回两项P1（4242623018授权绑定、4242623020成功工具原记录）；中央 `loc-m17-fix05-source-verdict.json` 为NEEDS_FIX。旧成功工具原文已归档；LOC actual `01a12c28-d01f-7731-a6e9-51fc7ac522f4`已完成REVIEW01：15/15新检查5.351秒、封签4.738秒，外层完整工具回执已发布且匹配最终八叶。PR2新head `5c12ddabfc784927a936b428d9f1b90638cf108e`，trigger6112203396，复审返回新P1 4242690950：sidecar要求尚未在材料启动入口强制校验。中央REVIEW01源码NEEDS_FIX。主程已完成REVIEW02启动门设计；LOC actual `01a12c3f-ebd9-70d0-92f0-74666adab242`已实现并通过109/109新检查2.033秒、单次封签4.842秒。中央实核原工具及八叶/侧录身份，13文件推至PR2 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`，trigger6112501532，主测试actual `01a12c51-ea21-7f22-ad4c-ba6deab027a0`已接回GitHub完成结果6112531539：准确head无major，非正式APPROVED。中央REVIEW02源码限定ACCEPT，并在回复修正依据后关闭三条历史P1线程；原F5/R1证据保留。全归档复用、0GET候选尚未进入材料执行；下次材料提案已绑定新SOURCE与外层回执，源码审查已接收，M07的新一次材料执行许可现已取得，执行状态见本页顶部，中央唯一收件。保留partial根及旧原始证据，不重启旧host或重置一次执行/900秒额度。FIX04源码ACCEPT保持，材料M仍未接收。

M06原授权：用户允许一次复用已下载文件、补齐13个官方归档约9MB、外置盘3次依赖还原、最长15分钟，中央读取结果；授权actual `01a1298d-4509-7132-8e0d-9f433010ad0a` / item `01a1298d-4589-7d93-a269-75e3a3e1fc05`。首次审批超时发生在CreateProcess之前，其证据保留；随后resume1已真正启动，当前不再是旧审批超时阻塞。

2026-10-11：M05 已实际执行失败，不是工具审批拒绝。首次 owner 只生成 before 后结束，未启动；同次 resume1 实际 owner `01a12931-9e6f-79b1-ac3d-ab9ce3ab4ac0`、原工具 `exec-4f9e3510-3f50-4e1d-b33f-f2c983b48133` 获准执行，42.369108秒、OS1、`unsafe relative path`；5个官方GET共16158115正文B，restore0。旧74项保护对象不变，原日志和partial外置W保留；没有重试、B/L或Unity。中央actual `01a12926-663f-78a0-bee9-38392d8c0f5e` / 人类item `01a12926-6966-77b3-9e90-961521142cee` 仍为本次直接授权来源。QA机械接收已完成，中央 `loc-m17-m05-native-verdict.json` 记 NEEDS_FIX。主程已完成方案，定位到既存 Scriban 7.2.5 两条内部双斜线；LOC actual `01a1293b-3a74-7512-8a09-09b17fda765d` 已完成FIX04 SOURCE候选：66/66定向检查1.481秒，机械封签一次不到4秒，19包705项覆盖及五包复用。PR2新head `816263eaacf4f3e276c0cef7703c5824af086cc6` 已推送并逐18文件验证；trigger6105553315，GitHub结果6105581700已完成，QA actual `01a12948-fc42-7e33-905f-b6275bc02511` 已接收，中央 `loc-m17-fix04-source-verdict.json` 为源码限定ACCEPT。当时没有新材料执行；该13GET/9014649B+3restore/900秒提案现已获M06明确授权，最新状态见上。离线定位不冒充原堆栈路径。不会把历史通过或SOURCE接收当M通过。

LOC M04 已实际执行并失败：3.646504秒，`unsafe relative path`，下载0、依赖还原0。工具正常批准，旧审批阻塞已解除；用户明确的一次重试已用完，不能自动再跑。QA已核收，见 [失败结论](../docs/team/2026-09-30/loc-m17-m04-native-verdict.json)。FIX03源码已推PR2 head `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`，GitHub结果6055811693已完成，中央源码限定ACCEPT；限定两个官方ZIP别名及新外置m17-fix03根。26项检查通过，完整SOURCE准备5.1037秒的0.1037秒超时已记录。该提案已由2026-10-11用户“继续任务”在明确提问上下文中接续为一次M05，详见当日授权记录和 `central-loc-m17-m05-execute.md`；M05实际失败见上，不再等待旧确认卡。

资源 V08 已完成12程序集编译及准确89项测试（89通过、0失败/跳过）；整轮因 SDK ADB 历史绑定未退役导致监护失败，164.364642秒，含准备324.364642秒。P1035和43项编译文件已恢复，closure=true、owned空。保留有效编译/测试证据，不默认重跑整套。ADB监护修正已推PR20 head `23bcfd10e7478a7a0ab275d3532e883e5805e908`，42项离线一次0.155085秒通过，GitHub结果6055530291已由QA接回，中央源码限定ACCEPT；原第二候选身份未记录，不能声称当时有两个同时存活的合法服务器。见 [原生结论](../docs/team/2026-09-30/resource-combined-v08-native-verdict.json)。QA已补核AS06/AS07真实IO与链接断言：源码/本轮OSX程序集/XML对应成立；空目录是合同规定的清理结果，不证明真实进程重启。

本轮中央 actual `01a11a60-d890-7660-b7ee-1accdb3ffe0c`；用户授权 item `01a11a60-d8c2-71f0-b832-a4e0c8db237e`；LOC实际 `01a11a62-0e7c-7553-aa47-c16cd6a276b9`，C实际 `01a11a65-5b5a-7ac2-a8bf-fdcbec225ff1`。PR20已审旧源head `a360ad864171a3b49f7785ab7c797028d583d5c0`／GitHub6054341623仅关闭其源门；后续修改需新的准确head审查。

Shared input remains1036 leaves/32657794B, canonical `61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8`; validation projection1037 differs by its existing meta. Local master125b849 and all WIP are preserved. Remote PRs are draft, unmerged. Cloud synchronization failed at proxy:8080 before projection or tests. Earlier entry facts are preserved in [history](PROJECT_CONTEXT.history-before-m04-v08-results.md).

## Approved constraints

[AGENTS](../AGENTS.md) governs scope and protected files. Follow the [five-role workflow](TEAM_WORKFLOW.md); C alone executes local Unity until an explicit responsibility transfer. Primary roles use requested Astra/xhigh, mechanical work may use Luna. Receive real completion, preserve historical evidence, and perform only necessary new validation. GitHub PR Code Review owns code review; dots is preferred for supported automation but current readiness cannot be assumed.

Runtime UI is uGUI; UI Toolkit is for Editor tooling. Before UI work read the [correction plan](../docs/system-design/2026-09-17/runtime-ui-ugui-correction.md). English/Simplified Chinese use diagnostic prefab placeholders, Luban authoring tables, project-owned localization and YooAsset; do not introduce Unity Localization/Addressables. Demo/Android share code, business and save semantics. Placeholder art is permitted; selected commercial assets are not yet approved.

Preserve Mac/Windows compatibility and current versions;2022.3.62f3 is only a candidate. Approved Android minimumAPI24 and overseas Google Play first-release choices still require their own scoped implementation/device evidence. First release retains guest/local saves, rewarded ads with a minimal Go/sqlc/MySQL backend, small initial package and R2 content distribution; formal accounts/cloud saves and live walkthrough analysis are later work. Approved design does not itself authorize service purchases, deployment or release.

FlowPuzzle follows the approved [design](../docs/superpowers/specs/2026-06-24-flow-puzzle-level-tool-design.md) and [implementation plan](../docs/superpowers/plans/2026-06-24-flow-puzzle-level-tool-implementation.md): solution-first generation, no unique-solution detection, no full-board requirement, core separated from Editor UI.

## Context references

Use the role-specific [system design](../docs/migration/2026-09-28/SYSTEM_DESIGN_CONTEXT.md), [architecture](../docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md) and [planning](../docs/migration/2026-09-28/PLANNING_CONTEXT.md) only as needed. Current readiness and actual identities come from the team snapshot and exact packet receipts.

The previous long context, including original decisions, Windows migration facts and historical paused states, is preserved byte-for-byte in [the2026-10-08 history](PROJECT_CONTEXT.history-2026-10-08.md). Its old “current” headings describe their original dates. This entry-point correction prevents stale dispatch; it changes no product decision or execution permission.

````

## docs/team/2026-09-30/README.md

````markdown
# FightMatch 当前接续快照

当前执行（2026-10-13）：用户已批准简化方案并要求接着推进游戏接入，不逐步请示。主程负责 LOC 必要错表检查及后续本地化/偏好/资源/uGUI Host 接通，组内自行分发、接回和修正；C 串行 Unity，代码仍经 GitHub PR 审查。现有有效结果复用，视觉可人工验收。当前具体状态由[主程索引](engineering.md)唯一维护；旧 M17/FIX11 不恢复，以下为历史。

最新用户指令（2026-10-13）：改查 Luban 官方常规安装/使用方式，接受必要的人工验收；M17/FIX11 旧校验链已收住。主程确认 LOC 已结束且无在途进程，原失败和候选保留，不再补跑或叠补丁。整体开发没有暂停。下一方案见[常规路线研究](research-luban-standard-install-2026-10-13.md)，以下状态为历史。

最新：FIX10/M13已实际执行216.203秒，首还原成功，第二因缓存临时文件FileNotFoundError退出，第三未启动；0下载，保护与已知进程收尾通过，材料未完成。中央已收原工具，主程组内继续限定诊断/源码修正，不自动重跑native。次数、原因及当前详情见[START_HERE](../../../START_HERE.md)与[执行统计](central-loc-execution-history-2026-10-13.md)。以下为历史。

2026-10-12 M10已接续：用户当前要求继续之前卡住的任务，中央actual01a12f63-745a-70e0-8fd5-d5256a39e1fb／人类item01a12f63-7521-75c0-897e-2bdff545c277；LOC actual01a12f65-de4c-7901-a66a-9c6b58cd7c33。FIX08准确head a0bfe700已源码接收，新A/H仅填既审模板CONFIG，执行见 central-loc-m17-m10-execute.md。限定一次0下载、最多三restore、完整900秒；中央为本次有限执行唯一收件者。已实际启动host并于5.021159秒exit1，因旧OS工具编号回执不匹配在预检停止；restore0、下载0、312保护项不变、已知进程静止。中央实核原工具exec-69b3ec4e-284f-41fa-9edb-31ba86c5d175，loc-m17-m10-native-verdict.json为NEEDS_FIX；主程actual01a12f6b-6305-7281-9d8a-ebf6dcdaa1e5已完成组内派发及接收：仅更正回执中一个字符，真实consume_source/verify_activation静态启动检查5.746348秒通过；详细证据见engineering-loc-m10-correction-receipt.json。更正与证据已推PR2提交489b28dc62adaa24242c72e4e9e6bf2117fd11a8，源代码仍为已审a0bfe700，无新增源码审查。尚无成功还原结果；M10一次额度已使用，下一次明确提案见[修正后还原提案](central-loc-fix08-m11-proposal.md)。没有另启host，M09和M10失败保留。

2026-10-12 M09：用户对FIX07一次零下载/三restore/900秒提案直接答“执行”。中央actual `01a12ee5-5e38-7863-b291-95e758e24669` / 人类item `01a12ee5-62af-73d3-a5b5-c3827b86dfba`；LOC actual `01a12ee6-b260-7693-9f5f-ecf014c0482d`，中央唯一收件。FIX07源码head `bd92f6bcedaf89751256a4a7e0084d39cd244893`已源码限定接收；新A/H只填既审模板CONFIG，八叶与侧录已鲜核。按 `central-loc-m17-m09-execute.md`已实际执行失败：128.818624秒/host1，首restore6.342396秒/exit1，Luban.Gdscript项目根GUID.tmp被拒（非obj），后两条未启动；0下载、284保护不变、已知进程静止、manifest缺席。中央实核原工具exec-6240c5ef-e6bf-4d38-af3b-48716e784015；QA actual01a12eed-ac9b-76d3-a7b3-d71dd17569b9及机械归因修正01a12eef-83b2-7072-985d-508d1a59c592已接收，中央 `loc-m17-m09-native-verdict.json` 为NEEDS_FIX。一次额度已用；主程诊断actual01a12ef5-0026-7140-92cf-f98d0cb0d364已确认遗漏49项目根规则；LOC actual01a12efd-cd09-7972-8b24-057c07dd2217按 `central-loc-m17-fix08-source.md`实施98父目录覆盖与限定272KiB新driver，42/42静态差量完整调用7.008589秒、一次封存14.938371秒均通过，无新增restore。侧录记录者actual01a12f16-9330-7332-8381-e66f9fd33a61；中央实核原工具与八文件，31新文件已推PR2 head a0bfe7007be65eed6d1afda317934195cb6ebbd6、1883旧blob不变；trigger6120173614，主测试actual01a12f20-e81f-75f3-b746-85ebb8ee1906已接回GitHub完成结果6120235084，准确head未报新重大问题，非正式APPROVED；历史FIX01 P2保留。中央 loc-m17-fix08-source-verdict.json 为源码限定ACCEPT。新完整还原方案见 central-loc-fix08-next-material-proposal.md，尚未另获一次授权。独立B入口静态方案也已完成，见 `engineering-loc-fix07-next-b-plan.md`，材料尚未接收不启动B。M7/W7固定清单已捕获到 `central-loc-m09-partial-capture.json`（3701文件/1328目录）；旧证据保持。

2026-10-12 M08新授权：用户明确允许修正版再执行一次，复用全部归档、0新增下载、外置盘三次restore、总900秒、中央收件。中央actual `01a12e46-6af4-7b82-958d-d18c1146d85d` / 人类item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`；LOC actual `01a12e47-da22-7b32-a9be-046bc0919c4b`。FIX06源码及GitHub审查已接收，head `454eb4da250262a396b17da97d83c71de408bd48`；新A/H已按既审模板仅填CONFIG并绑定本次身份/准确预算/侧录，执行包 `central-loc-m17-m08-execute.md`。M08已实际失败：129.057504秒/host1，首restore5.987138秒/exit1，Luban.DataLoader.Builtin/obj/GUID.tmp仍被拒，后两条未启动；0GET、244保护项不变、已知进程静止、manifest缺席。中央实核原工具，QA actual `01a12e4f-7cf9-7b33-ba5a-390682fbecb2`已核收，中央 `loc-m17-m08-native-verdict.json` 为NEEDS_FIX。主程 actual `01a12e4f-69be-72e2-b3ef-ca33e7c4303b`已完成6个真实OS微型探针（约0.157秒）：生产计数重复表达被拒，等价展开成功，见 `engineering-loc-after-m08-failure.md`。FIX07 原作者 actual `01a12e5d-f4c9-7443-87c9-3eb4d3f47ce4`完成3个实际OS回归；首次aggregate因中央库存摘要JSON格式失败，旧记录保存。repair作者/封存执行者 actual `01a12e6f-1e7a-7732-a9ac-871c55a04271`按 `central-loc-m17-fix07-repair01.md`修正引用并复用OS证据，16/16差量6.432883秒及封存11.298968秒均exit0，未新增native/restore。中央实核原工具及八叶，45文件已推PR2 head `bd92f6bcedaf89751256a4a7e0084d39cd244893`、1838旧blob不变；trigger6118099176，主测试actual `01a12e80-9746-7f73-a30c-8b802e2f5f26`已接回GitHub完成结果6118132100：准确head未报新重大问题、非正式APPROVED；历史FIX01的P2缓存预检线程仍保留，当前FIX07为0GET范围。中央 `loc-m17-fix07-source-verdict.json` 为源码限定ACCEPT。下一次0GET/三restore/900秒方案见 `central-loc-fix07-next-material-proposal.md`；完整还原一次额度已用，尚未另启。M6/W6/E08及M07等全部旧证据保留。

2026-10-12。中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local唯一维护。用户要求继续开发；“任务暂停？”没有取消该要求。首Demo未完成，当前无活动Goal对象，不新增定时任务。

## 当前任务

2026-10-12 M07新授权：用户明确允许再执行一次，复用全部下载文件、零新增下载、外置盘三次restore、最长900秒，中央收件。中央actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类item `01a12dc8-5fb9-7e43-8659-7026352c2050`；LOC执行actual `01a12dca-5f12-7d21-8c65-31ac1d70c49e`。已按REVIEW02准确head01a5ab7a生成新A/H，仅填写既审host模板CONFIG；sourceCommandReceipt已绑定。执行包为 `docs/team/2026-09-30/central-loc-m17-m07-execute.md`，新外置M5/W5与E07四叶；M07已实际失败：115.929秒/host1，首个runner restore在Luban.Protobuf/obj/*.tmp写入时Operation not permitted，8.011秒/exit1，后两次未启动，下载0、保护216项未变、manifest缺席。QA actual `01a12dd3-79cc-7321-bae8-0545a149914e`已核收，中央 `loc-m17-m07-native-verdict.json` 为NEEDS_FIX；主程actual `01a12dd4-ca41-70d0-b5a7-e8528d83e67c`已完成最小方案；LOC actual `01a12dde-888b-7591-bb45-0a3576b1b5c5`已完成FIX06：49个图内obj目录的窄GUID.tmp规则、55/55一次4.638493秒、封存一次8.009652秒，均exit0。中央实核两条原工具/八叶与侧录，29文件已推PR2 head `454eb4da250262a396b17da97d83c71de408bd48`（1809旧blob不变）；trigger6116642338，QA actual `01a12def-6bc4-79c1-b89e-06044c8487ad`已接回GitHub完成结果6116669351：准确head未报major，非正式APPROVED。中央 `loc-m17-fix06-source-verdict.json` 为源码限定ACCEPT；M07仍NEEDS_FIX，无新native运行。下一次0GET/三restore/900秒方案已准备在 `central-loc-fix06-next-material-proposal.md`；本轮一次真实运行许可已使用，需新的明确许可才启动。原执行已知进程静止，补充默认沙箱ps被拒单独记录；旧M06及有效验证保留。

2026-10-12 接续：M06-resume1 已在正常工具审批后实际启动，随后回合被用户中断。原 LOC actual `01a12998-5f4d-7a03-83c3-c5670144e1cc`、工具 `exec-177da352-5c1b-4a9e-a39e-82082f70591f` 于2026-10-11T06:15:23.066145Z记录 HOST_STARTED/PID60792；工具中断的-1不是DUT退出码，尚无HOST_EXITED或最终材料收据。13个GET已有成功、进程静止收据；M4/W4存在，三次restore没有完成证据。中央本日只读ps确认原PID缺席。中央actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613` / 用户“继续任务”item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`：LOC actual `01a12bfe-0633-7001-9613-18780415e1a1`已完成中断核收，13归档9014649B实核一致、123保护项未变；QA actual `01a12c02-06c4-7c52-bc23-0ea3c637a286`已接收，中央 `loc-m17-m06-native-verdict.json` 为 NEEDS_FIX。FIX05 SOURCE已完成：准确源码作者actual `01a12c14-512d-7331-8acc-fd240ca1de4a`，封签执行actual `01a12c19-313d-7803-a6e2-07f57152e9d6`；81/81差量检查2.256秒通过，格式修正后封签4.916秒通过，原身份/格式失败记录保留。PR2新head `c4c7a5d313c1193191447b525ade226268b616e1` 已推送并核对26文件，trigger6111991590，QA actual `01a12c1f-be8b-70e1-9ba6-f932a8f01031`已接回两项P1（4242623018授权绑定、4242623020成功工具原记录）；中央 `loc-m17-fix05-source-verdict.json` 为NEEDS_FIX。旧成功工具原文已归档；LOC actual `01a12c28-d01f-7731-a6e9-51fc7ac522f4`已完成REVIEW01：15/15新检查5.351秒、封签4.738秒，外层完整工具回执已发布且匹配最终八叶。PR2新head `5c12ddabfc784927a936b428d9f1b90638cf108e`，trigger6112203396，复审返回新P1 4242690950：sidecar要求尚未在材料启动入口强制校验。中央REVIEW01源码NEEDS_FIX。主程已完成REVIEW02启动门设计；LOC actual `01a12c3f-ebd9-70d0-92f0-74666adab242`已实现并通过109/109新检查2.033秒、单次封签4.842秒。中央实核原工具及八叶/侧录身份，13文件推至PR2 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`，trigger6112501532，主测试actual `01a12c51-ea21-7f22-ad4c-ba6deab027a0`已接回GitHub完成结果6112531539：准确head无major，非正式APPROVED。中央REVIEW02源码限定ACCEPT，并在回复修正依据后关闭三条历史P1线程；原F5/R1证据保留。全归档复用、0GET候选尚未进入材料执行；下次材料提案已绑定新SOURCE与外层回执，源码审查已接收，M07的新一次材料执行许可现已取得，执行状态见本页顶部，中央唯一收件。保留partial根及旧原始证据，不重启旧host或重置一次执行/900秒额度。FIX04源码ACCEPT保持，材料M仍未接收。

M06原授权：用户允许一次复用已下载文件、补齐13个官方归档约9MB、外置盘3次依赖还原、最长15分钟，中央读取结果；授权actual `01a1298d-4509-7132-8e0d-9f433010ad0a` / item `01a1298d-4589-7d93-a269-75e3a3e1fc05`。首次审批超时发生在CreateProcess之前，其证据保留；随后resume1已真正启动，当前不再是旧审批超时阻塞。

2026-10-11：M05 已实际执行失败，不是工具审批拒绝。首次 owner 只生成 before 后结束，未启动；同次 resume1 实际 owner `01a12931-9e6f-79b1-ac3d-ab9ce3ab4ac0`、原工具 `exec-4f9e3510-3f50-4e1d-b33f-f2c983b48133` 获准执行，42.369108秒、OS1、`unsafe relative path`；5个官方GET共16158115正文B，restore0。旧74项保护对象不变，原日志和partial外置W保留；没有重试、B/L或Unity。中央actual `01a12926-663f-78a0-bee9-38392d8c0f5e` / 人类item `01a12926-6966-77b3-9e90-961521142cee` 仍为本次直接授权来源。QA机械接收已完成，中央 `loc-m17-m05-native-verdict.json` 记 NEEDS_FIX。主程已完成方案，定位到既存 Scriban 7.2.5 两条内部双斜线；LOC actual `01a1293b-3a74-7512-8a09-09b17fda765d` 已完成FIX04 SOURCE候选：66/66定向检查1.481秒，机械封签一次不到4秒，19包705项覆盖及五包复用。PR2新head `816263eaacf4f3e276c0cef7703c5824af086cc6` 已推送并逐18文件验证；trigger6105553315，GitHub结果6105581700已完成，QA actual `01a12948-fc42-7e33-905f-b6275bc02511` 已接收，中央 `loc-m17-fix04-source-verdict.json` 为源码限定ACCEPT。当时没有新材料执行；该13GET/9014649B+3restore/900秒提案现已获M06明确授权，最新状态见上。离线定位不冒充原堆栈路径。不会把历史通过或SOURCE接收当M通过。

LOC M04 已实际执行并失败：3.646504秒，`unsafe relative path`，下载0、依赖还原0。工具正常批准，旧审批阻塞已解除；用户明确的一次重试已用完，不能自动再跑。QA已核收，见 [失败结论](loc-m17-m04-native-verdict.json)。FIX03源码已推PR2 head `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`，GitHub结果6055811693已完成，中央源码限定ACCEPT；限定两个官方ZIP别名及新外置m17-fix03根。26项检查通过，完整SOURCE准备5.1037秒的0.1037秒超时已记录。该提案已由2026-10-11用户“继续任务”在明确提问上下文中接续为一次M05，详见当日授权记录和 `central-loc-m17-m05-execute.md`；M05实际失败见上，不再等待旧确认卡。

资源 V08 已完成12程序集编译及准确89项测试（89通过、0失败/跳过）；整轮因 SDK ADB 历史绑定未退役导致监护失败，164.364642秒，含准备324.364642秒。P1035和43项编译文件已恢复，closure=true、owned空。保留有效编译/测试证据，不默认重跑整套。ADB监护修正已推PR20 head `23bcfd10e7478a7a0ab275d3532e883e5805e908`，42项离线一次0.155085秒通过，GitHub结果6055530291已由QA接回，中央源码限定ACCEPT；原第二候选身份未记录，不能声称当时有两个同时存活的合法服务器。见 [原生结论](resource-combined-v08-native-verdict.json)。QA已补核AS06/AS07真实IO与链接断言：源码/本轮OSX程序集/XML对应成立；空目录是合同规定的清理结果，不证明真实进程重启。

本轮中央 actual `01a11a60-d890-7660-b7ee-1accdb3ffe0c`；用户授权 item `01a11a60-d8c2-71f0-b832-a4e0c8db237e`；LOC实际 `01a11a62-0e7c-7553-aa47-c16cd6a276b9`，C实际 `01a11a65-5b5a-7ac2-a8bf-fdcbec225ff1`。PR20已审旧源head `a360ad864171a3b49f7785ab7c797028d583d5c0`／GitHub6054341623仅关闭其源门；后续修改需新的准确head审查。

当前源码包：[LOC FIX05](central-loc-m17-fix05-source.md)；[ADB后续](central-resource-v08-adb-followup-source.md)已有源码接收，不重复审查。实际新回合由各自收据及中央完成事件记录。中央唯一收件，C当前无新Unity执行。

此前V01–V08过程保存在[原快照](run-history-through-v08-native.md)，失败不追认为通过，旧执行额度不可复用。

## 已固定的产品输入与剩余验收

共享1036文件/32657794B，canonical61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8；资源contracts、codec、runtime、activation store、YooAsset adapter/factory及53包图已采用。[PR19源码接收](resource-shared-integration-source-verdict.json)与本轮V08编译/89测试证据分别记录，整体监护尚未通过。P01原1035、拟1037，单独多的LocalePreferenceStoreTests.meta暂不进入共享；下一包不得暗改源身份。

Host接线10/10是旧准确输入的限定证据；PR17诊断观察和修正也有独立边界。PR4 P1仍未关闭，原Scene12900B保留，不能再机械删零覆盖或导入诊断后像。正式Host仍需非null文本源、真实资源准入与正常交互；[后续依据](engineering-post-host-compile-next.md)中的资源采用步骤已完成，不能再重复执行。

还缺正式LOC、真实内置资源包/正常Host交互、新uGUI APK、实际玩法和冷启动恢复/设备验收。iQOO Neo5未接入；正式美术未选，继续占位。源码、离线、Unity、视觉和设备验收分开记录，不扩大为完整安卓首版。

美术：Warrior Idle六帧版已由用户采用为初版Initial01，尚未导入Unity。候选历史、用户选择和专业检查由主美维护，见[美术记录 §10.4](art.md#104-用户采用为-initial-01同日)；后续动作与导入尚未执行。日常制作回执止于主美，需中央协调的交接按[团队约定第6节](../../../.agent/TEAM_WORKFLOW.md#6-回传与继续推进)汇总。

## 独立阻塞

LOC M05一次材料执行已失败；FIX04源码已修复、推送和接收GitHub审查。M06实际启动后中断，13GET已成功，中断收据已核收、M未接收，FIX05原审查两项P1已有REVIEW01纠正，R1 head5c12ddab返回启动门P1，REVIEW02已推head01a5ab7a，109/109通过且GitHub准确head无major，中央SOURCE ACCEPT；M07已实际失败，首restore写obj临时文件被拒；一次许可已使用，主程正定位源码合同缺口，旧许可不复用；资源编译/89测试证据保留。dots云执行通道已于本日actual01a11901-90ac-7658-8fef-b153f314b220恢复（Unity/SDK入口存在，未启动）；actual01a11903-453e-732a-8642-3e42d532820f已完成只读前提检查：云HEAD仍125b849，缺YooAsset/SBP及当前源码，许可仅旧日志/UNVERIFIED。详见[本日云就绪记录](testing-cloud-readiness-2026-10-08.json)；无安装/同步/测试。本机仍仅按明确C执行包例外。云输入快照PR21原head6359缺一枚既存meta；FIX01沿用Mac P01该meta，完整1037输入与拟投影一致，没有本机产品改写。新head92fb75c2549f31923bdda07b35032666f8157fdd，GitHub结果6050271680无major，主测试actual01a1191c-c565-7ab3-9992-a877ef844bcf接回，中央[源码限定ACCEPT](resource-cloud-input-fix01-source-verdict.json)。云actual01a11915-eb31-7771-9d25-d3f0b0c128d8尝试原head同步时，git fetch因proxy:8080连接失败exit128；没有投影/下载包/native，旧HEAD与工作区不变。[实际同步记录](resource-cloud-sync01-central-receipt.json)标BLOCKED_NETWORK_PROXY；不自动换网络路线重试。

## 执行与角色

[AGENTS](../../../AGENTS.md)和[团队约定](../../../.agent/TEAM_WORKFLOW.md)不变。仅C串行本机Unity，中央负责分发/收件；默认Astra/xhigh，明确机械任务可Luna。主程、主策、主美、主测试自行分发和接收普通已授权任务；需中央跨组协调或影响整体进度时才汇总（团队约定第6节）。每包一个完成接收者，不让多角色重复轮询；代码审查只交GitHub。只因源码变化/新失败做必要验证，普通技术修正不逐包重问。

| 角色 | thread（host均local，云例外） |
| --- | --- |
| 中央 | 01a0e401-511d-79f2-b47f-3ab0ade1681b |
| 主程 | 01a0f2e3-1a80-7671-a459-38d5c8de0e6b |
| 主策 | 01a0f2e3-2c47-7273-88da-20513bcbfaf4 |
| 主美 | 01a0f2e3-3a47-78f2-819c-cbe52e8d7e47 |
| 主测试 | 01a0f2e3-4bd2-7130-b43a-19afa702e7bb |
| 唯一C | 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c |
| 源码执行 | 01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c |
| LOC | 01a0f40d-b0c5-7bc0-a2b2-be9d1213648a |
| 云测试 | 01a0f13c-3d87-753a-ad11-16302add9084/durable |

本机master125b849及既有WIP保留，审查分支通过连接器发布；PR仍draft未合并，不自动merge/pull/reset、付费、部署或发行。Unity2022.3.18f1 Intel在外置盘；uGUI运行时、UITK编辑器、英/简中/Luban/YooAsset方向保持。

[截至V03草稿的完整原快照](run-history-through-2026-10-08-resource-v03-draft.md)保存所有原回合/封签/旧错误细节。它和更早历史只作证据，不重新授权旧N07/N10/N11/I01/V01/V02。

````
