# 统一系统设计接续

时点：2026-09-28；先读根目录 [START_HERE.md](../../../START_HERE.md)。此页把原“FightMatch 统一系统设计”的有效停点交给没有旧聊天的新 SD00。原任务 ID `01a0ad71-fdc6-7811-a60d-b908d5111b5e` 仅作来源，不假设在 Mac 可调用。

## 最小阅读顺序

1. 三份协调稿的最新状态：[session-plan](../../system-design/2026-09-16/session-plan.md) §158～159、[integration-review](../../system-design/2026-09-17/integration-review.md) §152～153、[任务包](../../system-design/2026-09-17/system-task-packets.md) §376～378。
2. 当前 [RCV1 停改交付](../../system-design/2026-09-17/demo-cont-c-rcv1-delivery.md)与 [code-scope](../../system-design/2026-09-17/demo-cont-c-rcv1-code-scope.json)。先确定真实草稿与未完成项。
3. 已接收 [CONT-C 设计 C1](../../system-design/2026-09-17/demo-cont-c-design-c1.md)、[C1 scope](../../system-design/2026-09-17/demo-cont-c-design-c1-scope.json)、[C1 独立审查](../../system-design/2026-09-17/demo-cont-c-design-c1-review.md)，以及任务包冻结 §367、§374和登记 §375。
4. 本次涉及的共同契约：[system-catalog](../../system-design/2026-09-16/system-catalog.md)、[interaction-contracts](../../system-design/2026-09-16/interaction-contracts.md)、[runtime-extension-contracts](../../system-design/2026-09-16/runtime-extension-contracts.md)、[cross-system-flows](../../system-design/2026-09-16/cross-system-flows.md)。具体存档/应用/成长/库存条款再按引用下钻，不一次吞全部历史。

## 停点与原会话完成事实

| 角色 | 原会话及准确完成回合 | 正式结果 |
| --- | --- | --- |
| C | “FightMatch 本机保存与应用接入实现”；`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；turn `01a0e3b9-0fa6-7183-8406-8b1a116e5c56`；2026-09-27T16:48:59.077Z completed | STOPPED_FOR_DEVICE_MIGRATION，5 项源码 WIP，未编译/未测试，无活动执行命令。 |
| R | “FightMatch Demo 独立代码审查”；`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`；turn `01a0e379-0f90-7e82-b6aa-7dfa10ca24be`；2026-09-27T15:31:03.577Z completed | CONT-C **设计 C1** ACCEPT；没有当前 RCV1 代码 ACCEPT。 |

不要再次等待这些已经完成的旧回合。Mac 接续必须绑定新的准确回合、实际执行者、工具与证据；原线程 IDs 不是跨设备可达性的保证。旧 §374 正文的 APPROVED_FOR_IMPLEMENTATION 已被 §376 的迁移停改覆盖，用户在新设备明确恢复才继续。

CONT-C 待补的是正式导航、队伍/成长/背包/准备流程。5 项 WIP 都在 `Assets/Scripts/FightMatch/Application/`：CandidateApplicationRuntime、PlayerNavigationModels、PlayerNavigationQuery、PlayerNavigationSession、PlayerNavigationRecovery。剩余 4 份 Presentation、5 份测试、13 份自然 meta、唯一验证工具的 CONT-C 阶段尚未完成；全部 CC01～CC30 与恢复附加验收仍待证实。精确文件名、预算和公共接口以 C1 scope 与冻结包为准，不从本摘要另造白名单。

原技术缺口是 PlayerSave 对象重建后，再次 SaveFailed/CommitUnknown 无法取得原不可变 intent。§374只允许旧 runtime 新增受门控的 internal 只读 QueryResumedIntent；当前增加 19 行的草稿不是已验证修复。恢复验证须真的丢弃旧 request 引用，由生产入口取回原 intent；不得重新 Prepare、手造 intent 或用旧局部变量绕过缺口。C1 已闭合的 Craft 映射保持：实际 Draft.CharacterId=null，quoteCharacterId/ClassId=null；页面选中角色不能混入制作业务。

## 证据身份与复用

先按 [迁移说明](README.md)恢复 `continuation-evidence.zip`，核 [evidence-index.json](evidence-index.json)。126 个文件只覆盖 CONT-B-CODE-C1、CONT-C、CONT-C-RCV1 三段，不是全部历史原始证据。

| 实物 | 字节 | SHA256 |
| --- | ---: | --- |
| demo-cont-c-rcv1-delivery.md | 5099 | `93a8a82d81f66868894b9c731fceb3dac9424f9992d187969258fdf07170a869` |
| demo-cont-c-rcv1-code-scope.json | 50130 | `c75c1c37216754d04864d53b6f9a6d6a1df266aba79a425e4365d94550eb61a3` |
| demo-cont-c-design-c1-review.md | 7838 | `fea59bcacbab27b9f8bebfaacce9367c4bf7db85da8236128f486e001b29a04f` |
| cont-c-rcv1/root-identity.json | 3301 | `54a19e9418202364cf0d415dcaedf22d1de6a50ee94d0f3657dc09470097cfb0` |
| cont-c-rcv1/read-manifest.json | 7021 | `e7172046534ef532eaced72a54162488efaea4c5a310fd10ed2c3a873ab55267` |

两个冻结包按 UTF-8 无 BOM、CRLF→LF、末尾恰好一个 LF 计算；从相应 `##` 标题至**独占行**结束标记，不能误取正文中引用的同名标记：

- §367：11217 字节，`9b98b275f4066726466df278c683aebecf2cfb073420499533b96cae5533713b`，结束 `<!-- CONT-C-IMPLEMENTATION-PACKET-END -->`。
- §374：9017 字节，`17c60d6f6f8e082681eef8f7704915f98560ccd49058fb573cebe5f604e90dde`，结束 `<!-- CONT-C-RCV1-PACKET-END -->`。

保全旧失败、BLOCKED、WIP 与候选；平台接续新增补签和证据根，不回改历史成功/失败。迁移新增文档与身份另列，不能把旧有限输入总数机械地加到所有新文件上。必要历史实物缺失时列具体路径/身份并先做独立可做部分，不凭报告假称已经核到原始文件。

## 推进与验收

SD00 维护系统设计与精确分发，C 唯一写产品实现并串行运行 Unity，R 只做独立审查。旧外部 DeepSeek 手工转包流程不要求用户重新手工搬运当前已授权的 C/R 工作。新设备替换 C/R 要有用户的实际授权并登记映射，不能仅凭本文件创建会话。

完成门必须同时有准确回合 completed、非异步正式 final、正式报告/证据；作者 COMPLETED 不等于 R ACCEPT，文件提前出现也不是回合已完成。按返回事件立即接续；若工具没有完成通知能力，直说这个具体限制，不声称已建立自动衔接。不得恢复已取消的五分钟任务或反复轮询。

耗时要求：保留旧 3872/3872（3867 个不同 fullname）证据，先补完整源码/测试并核静态范围，再固定源码 Compile→一次无 filter 全量 EditMode。失败/修复/源码变化才追加必要验证，记录耗时、PID、退出、XML 和源码身份；测试期间不变更被测源码。Mac 工具平台适配另签，但不借此重复所有旧阶段。SD00不运行 Unity；C 不在同工程已有 Editor 占用时并行启动 batchmode，不将 IPC 失败直接说成许可证无效。

独立接收 CONT-C 后才进入 028（回退、重来、退出、结算 UI）和 029（正式宿主、导航整链、恢复、重打幂等与首 Demo 验收）。同源流程以任务包 §271～272 为硬要求。当前缺发布内容可以据实为空，但合法可达功能不能以 Demo 名义绕过；缺实现不能伪称“已锁定”。

物理鼠标/触控/像素、交互式 PlayMode、Mac/Player/Android、正式配方→PlayerSave 制作、公开 AwaitLinks、未覆盖保存故障/真实崩溃及 §184 NOT VERIFIED 按实际后续证据逐项维护。测试宿主、Git 校验和架构图都不能代替这些验收。首 Demo 完成后停止，不自动扩完整首版。
