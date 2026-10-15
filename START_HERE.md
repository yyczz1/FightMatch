# FightMatch 当前开发接续入口

更新：2026-10-15。**当前继续开发，首 Demo 尚未完成。** 没有活动 Goal 对象，不新增定时任务。

## 1. 当前工作与下一步

用户2026-10-15直接批准：使用隔离测试档启动Unity最多两次，完成下拉框、中英文按钮和语言保存复开；推送本轮修改到PR22并发起GitHub审查，普通步骤不再确认。主程接收本轮UI收尾，C唯一运行Unity，中央负责发布。此为上轮实际拒绝后的新授权，不复用旧N07额度；实际启动和结果以主程当前记录为准。

- **已完成：** 当前264条文本与既有导出逐字段一致；标准Luban重复键检查2.541秒，按预期exit1并报告重复key，正式输入未修改。见[实际结果](TestArtifacts/FightMatch/LOC-STANDARD-001/result.md)。没有新下载、依赖还原或上游重建。
- **当前完成与阻塞：** 正式文本/内置资源/Host已接线；原Mac正常Session和简中曾实际观察，按钮早期自动点击无响应；随后人工操作反馈正常（见本轮修正）。三平台资源选择、Windows既有存储/路径入口接线已修，平台定向检查13通过/1 Windows原生跳过/0失败，Android和Windows真实raw构建成功，Mac产物复用，Scene已绑定三平台。原103pass、Bridge5+HCP8及最后Fake3/3均保留。源码/资源已推送`ef4c33f`至PR22，第三轮独立审查已完成，详见下方收件。详见[运行记录](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)、[平台证据](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/platform-verification.json)，只读[主程索引顶部](docs/team/2026-09-30/engineering.md)取具体范围，不从历史恢复旧任务。
- **本轮UI修正已在本机完成，待最终验收/发布：** 原46个Button（含3模板）已统一调整中英排版；语言窗口改为TMP下拉框，保留偏好保存与输入/解绑保护。14产品路径固定；Prefab SHA`77c17069…`，最终测试SHA`f0052bb5…`。Apply27.57秒exit0；15项定向为12通过/3失败，其中重绑定一项只修测试旧控件定位后1/1通过38.09秒；两个无图形窗口用例仍未通过。不得写成15/15或整个clean PR树全测。详见[主程索引顶部](docs/team/2026-09-30/engineering.md)和[实际记录](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)。
- **GUI待验：** 上轮C恢复隔离旧资料到简中冒险地图，540×960可见按钮未见溢出，Console0error；自动点语言/Return未见展开，输入路径与产品交互原因未分清，随后已退出Editor。英语/其他页/动态克隆、语言选择及保存复开尚待实际验证。上轮人工预览与Git协调的审批拒绝保留为历史；用户已给出上述本轮新授权，主程已接到继续安排。没有新结果前不称已通过或已推送，不让用户操作未准备好的窗口。
- **可发布候选已保留：** 10源码+3meta+Prefab；两份远端基线适配及精确patch已从临时目录复制到`TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/ui-button-dropdown-01/publish/`。只发布该clean适配，不将旧长验证链或本机独有UguiResponsiveLayoutTests整份带入。旧资料及原失败/成功证据保持。
- **验收：** 按[主测试8项清单](docs/team/2026-09-30/testing.md)检查真实启动、文本、语言切换、保存复开、地图/战斗及存档连续性。源码、自动化、实际观察、真机分开记录；新增代码交GitHub PR Code Review。
- **上一轮独立审查已收件：** [PR22第三轮结果](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6143365353)于2026-10-13 18:17:56完成，准确审查版本`23f00824db710f0c35299e7006f824384a267f02`，服务未发现重大问题，主测试接收此版本的源码审查门。两个旧P1已过期但GitHub线程未关闭，不能称formal APPROVED。当时后续只补文档；本轮按钮/下拉框新代码未推送，尚未获得新的独立审查。
- **集成判定：NEEDS_FIX。** 本机代码已修正，英文排版/下拉人工核对、玩家保存复开、新uGUI安卓包及设备证据仍缺。iQOO Neo5未接入；Windows原生未测。资源构建不等于APK/设备通过，Scene保存不等于玩家存档保存。
- **已停止：** Luban M17/FIX11自制还原/验收链。旧失败与候选保留；Yaml旧依赖许可证资料单列未解决，不再以重建整套工具链作为游戏接入前置。

角色与跨组状态见[团队快照](docs/team/2026-09-30/README.md)。详细历史只在定位特定证据时读取：[本次入口归档](docs/team/2026-09-30/central-entry-history-through-2026-10-13.md)、[旧执行次数与原因](docs/team/2026-09-30/central-loc-execution-history-2026-10-13.md)。历史中的“当前”“待激活”和一次授权均描述原时点。

## 2. 执行规则

用户于2026-10-14批准流程精简：新派工先按[任务分级](.agent/TEAM_WORKFLOW.md#0-按任务选择流程)选择短流程或详细设计；小修正由负责人直接交执行者，同一记录内修正和接收。本次UI与PR22权限来自上节2026-10-15人类新授权，流程变更本身不构成工具批准。

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
