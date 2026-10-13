# FightMatch 当前开发接续入口

更新：2026-10-13。**当前继续开发，首 Demo 尚未完成。** 没有活动 Goal 对象，不新增定时任务。

## 1. 当前工作与下一步

用户已明确批准：按简化方案继续，复用已有结果、补必要检查后推进游戏接入；可人工验收，普通技术步骤不逐项请示。中央回合 `01a13404-21d1-7262-abae-9d52295727c2`，人类消息 `01a13404-223c-7893-b069-dae699e44e19`。

- **已完成：** 当前264条文本与既有导出逐字段一致；标准Luban重复键检查2.541秒，按预期exit1并报告重复key，正式输入未修改。见[实际结果](TestArtifacts/FightMatch/LOC-STANDARD-001/result.md)。没有新下载、依赖还原或上游重建。
- **当前完成与阻塞：** 正式文本/内置资源/Host已接线；原Mac正常Session和简中曾实际观察，按钮自动点击无响应原因未定。三平台资源选择、Windows既有存储/路径入口接线已修，平台定向检查13通过/1 Windows原生跳过/0失败，Android和Windows真实raw构建成功，Mac产物复用，Scene已绑定三平台。原103pass、Bridge5+HCP8及最后Fake3/3均保留。源码/资源已推送`ef4c33f`至PR22，等待该修正的独立复审。详见[运行记录](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)、[平台证据](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/platform-verification.json)，只读[主程索引顶部](docs/team/2026-09-30/engineering.md)取具体范围，不从历史恢复旧任务。
- **真实工具阻塞，仅限GUI：** 阶段B隔离窗口启动被auto-review拒绝两次，第二次附本轮人类原文仍被拒，要求用户直接在执行聊天确认。C当前未运行Unity，新Game窗口未打开，不能请用户点未打开的按钮；不得换工具/入口/聊天绕过此拒绝。需用户在“FightMatch C-MAC-HISO 定向验证”（`01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`）直接批准“使用冻结候选和新HISO隔离档启动一次Unity GUI，进入Demo Play并留窗人工检查；允许隔离状态写入，不重跑测试/构建、不扩大范围，结果按既定链路交主程和中央”。这是本次工具审批要求，不是新增逐包请示规则。发布、独立审查及其普通代码修正继续；窗口READY后中央再统一请求一次人工点击判别。
- **验收：** 按[主测试8项清单](docs/team/2026-09-30/testing.md)检查真实启动、文本、语言切换、保存复开、地图/战斗及存档连续性。源码、自动化、实际观察、真机分开记录；新增代码交GitHub PR Code Review。
- **仍缺：** 修正后独立复审、真实点击/切语种/玩家保存复开、新uGUI安卓包及设备证据。[PR22](https://github.com/yyczz1/FightMatch/pull/22)前两次审查为NEEDS_FIX，旧问题已做接线修正，不能在新审查前自行关闭。iQOO Neo5未接入；Windows原生未测。资源构建不等于APK/设备通过，Scene保存不等于玩家存档保存。
- **已停止：** Luban M17/FIX11自制还原/验收链。旧失败与候选保留；Yaml旧依赖许可证资料单列未解决，不再以重建整套工具链作为游戏接入前置。

角色与跨组状态见[团队快照](docs/team/2026-09-30/README.md)。详细历史只在定位特定证据时读取：[本次入口归档](docs/team/2026-09-30/central-entry-history-through-2026-10-13.md)、[旧执行次数与原因](docs/team/2026-09-30/central-loc-execution-history-2026-10-13.md)。历史中的“当前”“待激活”和一次授权均描述原时点。

## 2. 执行规则

[AGENTS.md](AGENTS.md)是规则入口；另读[项目事实](.agent/PROJECT_CONTEXT.md)和[团队执行约定](.agent/TEAM_WORKFLOW.md)。中央分发/接收，五位负责人分工；仅 C 串行运行本机 Unity。代码审查只用 GitHub PR Code Review，自动化测试优先 dots；当前云环境缺就绪证据；本轮本机验证由C按已授权接入范围执行必要短命令，遵循团队约定第5节的简化规则。

普通已授权技术修正继续完成，不逐包重问用户。仅在确需用户决定或工具审批实际拒绝时说明具体缺口；不要把历史授权当新工具批准，也不要把一个独立阻塞扩成全项目暂停。每包一个完成接收者，使用完成事件/等待工具接回结果后继续下一就绪包。

默认主要角色 `gpt-6-astra/xhigh`，明确机械任务可用 Luna。保留有效证据，源码固定后只做必要验证；新增失败/修复/源码变化才追加。旧 N07/N10/N11/I01 的 native 额度不可复用。

## 3. 产品与环境边界

Demo 与正式安卓产品共用代码、规则、存档和正式流程，只减少内容。运行时 UI 用 uGUI，Editor 用 UI Toolkit。英/简中、预制体诊断占位符、Luban 表格和自有本地化绑定、YooAsset 路线不变。美术继续使用占位资源，未授权购买新素材。

当前项目在外置盘，Unity2022.3.18f1 Intel。2022.3.62f3 仅是发行准备候选，未升级。Android7/API24、AdMob 单来源、R2 等批准设计不等于已实施/验证；首版网络范围及后置账号/云档见 AGENTS 和角色背景。

本机 master 基线125b849及既有 WIP 保留。审查分支已有独立推送，PR 均仍 draft 未合并；不要在脏工作区 pull/reset，不自动合并、发布、部署或购买服务。具体 Git 写权限沿用户已授权范围和任务包。

## 4. 角色背景与历史

需要背景时按职责读取：[系统设计](docs/migration/2026-09-28/SYSTEM_DESIGN_CONTEXT.md)、[架构](docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md)、[策划](docs/migration/2026-09-28/PLANNING_CONTEXT.md)。日常接包不重读全部历史；实际 thread/host/turn 在团队快照及任务回执登记。

原迁移启动模板、旧第4节授权文本、历史暂停和验收记录均逐字节保存在 [2026-10-08 接续入口历史](START_HERE.history-2026-10-08.md)。它们是历史证据，不是新的派发或执行指令。本页整理入口，没有改变用户决定、扩大实施范围或重新启动旧任务。
