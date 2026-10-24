# FightMatch 当前开发接续入口

更新：2026-10-08。用户已要求继续开发；“任务暂停？”是状态询问，没有取消继续指令。当前没有活动 Goal 对象（最近 get_goal 为 null），不新增定时任务。**首 Demo 尚未完成。**

## 1. 当前工作与下一步

2026-10-08当前恢复：人类在中央actual01a11a60-d890-7660-b7ee-1accdb3ffe0c/item01a11a60-d8c2-71f0-b832-a4e0c8db237e明确批准LOC一次18官方归档/约25MB/三restore/外置盘/15分钟重试及中央读取。派发已成功，LOC actual01a11a62-0e7c-7553-aa47-c16cd6a276b9，M04中央新绑定就绪；旧拒绝和M03失效记录保留，尚无材料完成结论。PR20 V08 FIX01 head a360ad864171a3b49f7785ab7c797028d583d5c0 已收到GitHub6054341623 no-major、summaryCompleted；QA actual01a11a63-574e-7001-8edf-f4e963fa3204核收，中央resource-combined-v08-fix01-source-verdict.json限定ACCEPT。C已接一次V08 I→T89新包，实际结果待收。两条线分别写外置M/W和独立P/K/E，普通保护门不放宽。

先读 [团队当前快照](docs/team/2026-09-30/README.md)，再读本次实际收到的最新任务包。不要从历史聊天、旧提问或旧回执自行恢复已消耗的执行额度。

- 共享工程已采用资源模块、自然 meta 和固定 YooAsset 包图：1036 个输入，32657794B，canonical `61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8`。PR19 源码审查已限定接收；整个 PR 树不等于该共享输入。
- 资源组合验证V05真实FAILED：I1/Unity exit0，T89未启动，115.290570秒/含准备275.290570。新原始FD证据已定位TMP与Bee直接子目录之间多一条斜线，以及短命子PID身份查询退出竞态；无旧2.561ms超时。QA已核P1035/43compiler恢复、owned0。V06源码修正34/34回放通过并推PR20 head d8578a3，GitHub6051513575完成、源码门已接收；C actual01a1198b-3beb-7052-b04c-5cd441a5eee7实际FAILED：117.263279秒，T0，临时Bee端点消失及目录变化使闭合门未通过；独立恢复已实收RECOVERED（2.193398秒/OS0），P已回1035，43编译文件/62原前像/58移出档案核对通过，当前推进V07监护生命周期修正；不追认V05通过。见docs/team/2026-09-30/resource-combined-v05-native-verdict.json及central-resource-combined-v06-source.md。
- 云输入PR21修正缺失meta后，head92fb75c2549f31923bdda07b35032666f8157fdd已源码限定ACCEPT；实际云同步因proxy:8080无法连接失败，投影未创建、无云测试。该环境阻塞与本机V04独立。
- LOC 正式工具材料的具体派发被自动审批拒绝，等待用户确认卡；不能换执行者间接执行。独立资源工作继续。详见 [LOC 收件和拒绝记录](docs/team/2026-09-30/central-loc-passive-receipt-proposal.md)。
- 仍缺正式 LOC、真实资源包及正常 Host 交互、新 uGUI 安卓包、实际玩法和关闭重开/设备验收。旧测试、作者声明、文件出现或源码审查不能替代这些门。

- 当前V07 FIX02已审源码head `6da2ad59c99fd88c732ff56a8ca415aafa4b2cf2` 的文档P1已在95e21ca修正并由QA机械接收；代码字节不变，中央源码限定ACCEPT。C actual01a119ff-3be9-75a0-bf48-2a6464c1ea93实际V07 FAILED：I1/Unity0、T0、95.126369秒，child82401身份不完整；QA已核P1035/43恢复及closuretrue。当前定位这项新异常，不原样重跑。GitHub连接器已恢复。

- V08原head ce6c0728独立审查review5452286080发现P1 4215462510（cwd名称缺描述符仍被接受）。FIX01作者actual01a11a22-678b-7b43-a80c-179df9eda21a仅修details；inputs字节不变、其余103函数不变，15相关案例一次2.037791秒通过。已推PR20 head a360ad864171a3b49f7785ab7c797028d583d5c0，210发布叶/15020622B远端blob/size匹配，trigger6053659601。主测试actual01a11a27-e872-7e23-a73a-eff4741da301有限收件结束为PENDING；中央续读至06:35:27Z，GitHub仍Running（06:16:03Z开始），准确head无review。C未激活/native0。主程actual01a11a30-f8c1-7a50-9a56-f8d655d1f939核当前冻结窗口独立产品小包为NONE，详见engineering-demo-next-ready-2026-10-08.md。LOC具体18归档/约25MB/三次restore/外置根/15分钟重试审批已再次明确呈给用户，未实施。等待外部review及具体审批，无活动Goal或新增自动化；V07 FAILED/T0及已接恢复不变。

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
