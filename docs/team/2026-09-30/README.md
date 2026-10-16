# FightMatch 当前接续快照

2026-10-16：首Demo尚未验收；窄范围UI修正已结案，用户要求列清剩余内容并自主继续。当前为DEMO-NEXT-001，新Android普通包已构建成功，同版本源码独立审查已接收，待实际试玩；原UI两次启动和时间额度已结束。实际结果以[START_HERE](../../../START_HERE.md)及本线记录为准。

## 当前任务

**最新实际结果：** 新普通APK已生成，minSDK24/仅GLES3由Unity实际保存，原签名保持；一次构建及检查252.466秒。原2项针对检查通过＋FIX01单例通过，完整L1、视觉和设备仍未验。四源码＋真实Settings共5产品路径已发布到PR22产品提交`c78f8a52ba589534171b5990328e9cac2d42a752`；主测试已核[同head独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6211066540)未发现重大问题，仅源码审查门ACCEPT，首Demo整体NEEDS_FIX。唯一构建许可已使用，不追加安装/GUI/测试。详见[实际回执](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/execution-android01-result.json)及[当前试玩指南](../../demo/PLAY_FIRST_DEMO.md)；下面各段保留此前时点。

**DEMO-NEXT-001已接续：** 主程继续L1/新Android包的具体缺口与已定设置候选；主测试收敛剩余验收；主策更新唯一试玩指南。实际lead turns为主程`01a14307-7371-7d92-bd04-5c08dad5c90e`、QA`01a14307-8195-7750-a9b1-7737a4d084f6`、主策`01a14307-92ea-7bc1-a940-e5d671d348a1`，均local/Astra-xhigh。两项源码候选已冻结，用户已正常退出Unity；主程继续派C执行新短合同，旧UI额度不复用。美术继续占位，不激活无关角色。

主策已更新[可照做的L1试玩指南](../../demo/PLAY_FIRST_DEMO.md)，包含实际中英按钮、两条拖线路线和恢复步骤；主测试已合并[PQA-01至15剩余验收](testing.md#当前demo-next-001-首-demo-剩余验收2026-10-16)。这些是静态准备交付，不是新的通关或设备通过记录；普通工程接续由主程直接收件推进。

本轮UI修正ACCEPT：c3f0812已审查、接入、编译；用户确认冷开即English、原档/进度正常、切换语言后成功提示约2秒消失，最后保持English及Unity打开。追加启动2/2已使用；C收尾触发900秒主动时间守卫失败，已停止新增验证并保留超时事实，不重置额度或重试；详细预算与证据只在[主程顶部](engineering.md)。首Demo仍未完成。

| 工作 | 当前事实 | 唯一明细入口 |
| --- | --- | --- |
| 工具功能 | 复用264条真实导表；标准重复键检查2.541秒按预期拒绝，无下载/还原 | [结果](../../../TestArtifacts/FightMatch/LOC-STANDARD-001/result.md) |
| 游戏接入 | 本轮下拉/可见按钮、语言保存冷开和短时成功提示修正已接收；完整Demo仍待验 | [主程索引顶部](engineering.md) |
| Unity/实际观察 | c3f0812编译成功；用户冷开与2秒提示人工通过，保持English | [主程索引顶部](engineering.md) |
| 代码审查 | c3f0812独立审查未发现重大问题；主测试仅接收源码门 | [独立结果](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6207548412) |
| 美术 | 继续使用现有占位；Warrior Idle Initial01已有用户选择但尚未Unity导入 | [主美记录](art.md#104-用户采用为-initial-01同日) |

本轮UI集成判定ACCEPT，结合独立审查、编译和上述实际用户观察；新增UnityTest未运行，历史两个headless失败仍保留。该结论不代表独立通用Toast系统、全部页面、首Demo或安卓真机已完成。详情见[START_HERE](../../../START_HERE.md)。

新增已查明边界：WindowsEditorSaveStorage/WindowsContentPublicationStorage已存在，缺的是Host系统根与存储选择接线，并非缺少存储实现。本轮已复用接线；Windows原生运行仍未测，存档规则不变。

Luban M17/FIX11旧链已停止。旧Yaml依赖许可证资料未解决；源码/工具功能通过不等于该项通过。dots最近实际同步因proxy:8080失败，当前就绪不可假定，见[云记录](resource-cloud-sync01-central-receipt.json)。本轮必要本机验证已由用户授权，仍由C串行承担。

## 执行与角色

新派工遵循[任务分级与短流程](../../../.agent/TEAM_WORKFLOW.md#0-按任务选择流程)；五位负责人保留，普通修正直接交执行者，只在需要设计时启用设计层。同一任务一份记录、一个接收者，中央只接跨组协调、里程碑或整体阻塞。默认Astra/xhigh，明确机械任务可Luna。

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

本机master125b849及既有WIP保留；PR均为草稿、未合并。Unity2022.3.18f1 Intel与大型工具位于外置盘。运行时uGUI、Editor UI Toolkit、英/简中、Luban、YooAsset方向保持。

历史按需查[2026-10-13入口原文](central-entry-history-through-2026-10-13.md)、[V08及之前](run-history-through-v08-native.md)；历史“当前任务”和旧一次额度不得用于新派发。负责人索引只读顶部当前段，其后历史按需定位。
