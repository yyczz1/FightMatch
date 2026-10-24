# FightMatch 当前接续快照

**2026-10-25 首 Demo 已结案：ACCEPT。** 主测试 actual `01a16eb2-f323-7472-9419-b6043df3be30/local`及主程已正式接收最后有效Skip证据，既定首Demo无剩余必验项。原APK14/iQOO仅新增一次首红L＋一次有效Skip，当前95HP／敌0＋15／单红L，未领奖或重开；旧有效证据按原输入复用，未重跑。代码PR22 `dece3d8c`已推送并获准确head独审，仍draft未合并。详见[最终QA](testing.md)、[主程](engineering.md)、[试玩指南](../../demo/PLAY_FIRST_DEMO.md)。本轮完成，不自动扩展完整安卓、发行、服务或最终美术；以下旧时点的待办／阻塞不是新执行清单。

**2026-10-24两项UI限定ACCEPT，APK14真人触控与系统操作已确认，剩余验收接续：** PR22 `dece3d8c`五文件准确差量已推并获独审无重大问题，QA结合3项有效检查／构建接收源码门，draft未合并。iQOO安装成功、包与签名匹配，实际英简中页面已观察战士白框与普通恢复控件隐藏，8业务文件保持，最后简中。用户已确认单指白线、多指不抢操作、Home清线且战斗未变、真实Back确认与取消、必要按钮不被系统区遮挡；集中交QA收件，不重测。模拟器两处Reference文字、取消预览后导航及受控英文无偏好冷开已完成，原系统语言／偏好／18可读业务文件已恢复或保持。剩余手机行动＋本机短录屏被自动审批拒绝，未认可跨会话权源；动作0，已停止重试。QA复核后复用导航／回退和内部owner证据，撤销额外竞速组合；唯一剩余是有效播放时实际Skip。原C等待人类在执行聊天直接许可一次行动＋仅游戏本机短录屏，不让真人跨三页抢约0.9秒窗口。整体Demo尚未完成。旧失败和验证保留，详见[主程](engineering.md)。

**2026-10-23 APK13阶段限定ACCEPT：** 含已审`ec4be08d`的新包已一次构建并保档更新原BlueStacks，两次正常冷启动及一次游戏内正常退出通过；18业务文件＋语言偏好在五时点保持，待结算未领奖。旧验证不重跑，无新增源码/独审/Git动作。主程唯一收C，QA已接收；首Demo整体NEEDS_FIX，下一步等待iQOO Neo5 USB连接与调试授权，已通知用户。详见[主程](engineering.md)、[主测试](testing.md)与[新版试玩指南](../../demo/PLAY_FIRST_DEMO.md)。

**2026-10-23 最新收件入口：** 先读[主测试当前记录](testing.md)与[主程顶部](engineering.md)。PR22 `ec4be08d`的两P1及相关异步释放P2修正已限定ACCEPT：有效2＋4＋3例通过，准确head独审无重大问题，旧验证未重跑。PR保持draft未合并。APK12为历史未安装包；现APK13已补启动/退出/冷复开，玩法证据复用android-11，首Demo整体NEEDS_FIX。下方旧时点不是重跑清单。

2026-10-19：继续首Demo完整流程与异常退出排查，复用android-07及已通过证据；主程统一分发/接收，主测试据实际结果验收。当前事实见[START_HERE](../../../START_HERE.md)。

## 当前任务

**2026-10-19当前里程碑：** 模拟器新档L1、26XP/2锡片、攻略/回退/重来/退出与结果四去向已有实证。Android建档读取、活动局导航、双语确认正文及Reference布局四项修正已实际观察；当前android-11，拖线Home返回清未提交手势已补，原user0简中待结算未领奖/18业务文件不变。已过证据复用；主程唯一接C，QA接实际证据，主策持有试玩指南。

**发布与剩余：** [七路径材料](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/navigation-fix01/publish/inputs.json)同范围刷新最新Build/Prefab，排除旧WIP；历史失败不改绿、不扩测试框架。2026-10-22用户批准后，七文件已发PR22 `4e78fdec`（实核1提交/7路径），draft未合并，QA已接回[同head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6339540133)未发现重大问题，源码审查门已补；[发布回执](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/navigation-fix01/publish/publication-2026-10-22.json)。RES-BOOT-CORE-001已完成下载控制纯逻辑及16/16针对用例；新增四文件获用户另行许可后已发PR22 `d06faab14a`，原P2已在`f79a0ed2`修正，3项针对回归和准确head独审完成，CORE纯逻辑限定ACCEPT。Host接线原8项新检查与b141发布保留。2026-10-23按用户授权，连续Editor Play和失败清理重试两项P1已修正，复审发现的同一路径异步最终释放P2也已完成；最终提交`ec4be08d895b7c0d6ea01404b658ea892915764f`已推送PR22。有效必要证据为P1清理2例（p1-run02）＋Editor4例（p1-run04）＋本轮新增异步关闭3例（p2-run01），既有通过用例未重跑，失败历史完整保留。[准确head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6347414049)于10月23日01:48:58上海完成、未发现重大问题，QA本轮限定`ACCEPT`；PR22及旧恢复PR23保持draft未合并，未自动resolve旧审查线程。本轮未重建APK或操作设备，APK12不含这些修正且尚未安装；首Demo整体仍`NEEDS_FIX`。详细回执只见[主程](engineering.md)／[主测试](testing.md)。真实场景smoke01因窗口工具延迟未进入Play，正常退出且保护未变；未验证和超时事实保留。包含新Host/CORE的android-12已一次构建成功，原签名/保护文件保持，未安装；详见[实际构建回执](../../../TestArtifacts/FightMatch/RES-BOOT-HOST-001/apk12/result.md)。旧检查不重跑。真机和HTTP/CDN仍未验证；APK13模拟器启动/退出/冷开见顶部新结论。主策说明同步完成，主程继续首Demo独立可做项，旧验证不重跑。Reference已补双语条件/范围/差异正文和两Steps，英文差异标题与中文选步骤说明仍缺完整截图；不把部分截图写成全过或永久不可达。已请用户连接iQOO；真机触控/显示/系统返回、Skip时序、无偏好系统首开及这两处文字保留。旧两退出缺日志未归因；整体NEEDS_FIX。唯一详细证据见[主程](engineering.md)/[主测试PQA表](testing.md)。下列为历史，不是待重跑清单。

**2026-10-18排版修正收尾：限定集成ACCEPT，首Demo整体仍NEEDS_FIX。** English-layout01修后针对用例1/1通过，android-07一次构建并保档安装，中英活动/待结算文字及实际格中心拖线通过局部观察；正常动作到待结算，未领奖。执行含证据收尾1021.429/1200秒，Unity退出，旧证据直接复用。用户已在中央明确批准4文件上传PR22及独审、不合并；中央实际推送1e4cfeaea7de5d9b7eeeddddf74a8b45a2c06a25，对745a5f5仅4路径。旧拒绝已由新真人许可解除；QA接回[准确head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6250967784)无重大问题，结合实际双语/高度/拖线证据接收本轮排版修正ACCEPT。PR保持draft未合并，无新增运行。首Demo其它缺口保留。唯一明细见[主程当前条目](engineering.md)。

**本轮限定执行完成：** 原短C接入已审af35b600；三项针对检查3/3、零跳过，android-06构建1/2、保档安装成功，主动执行含收尾873.734/900秒。原始中英“选语言关菜单后首次未选人按棋盘”像素保持，混合输入与Esc设备通过；短拖线未提交，打开门禁依对应绿测。旧结算26XP/2锡片、三状态冷恢复及此前A/B累计复用保留，没有再结算。QA已给本轮语言/结算修正限定ACCEPT，中央同步指南与Git；无新代码审查轮、全量或下载。整体Demo仍有英文战斗排版及历史退出等缺口，不能扩大为全Demo通过；唯一明细见[主程](engineering.md)/[主测试](testing.md)。下面为历史里程碑。

**2026-10-18里程碑：** 四源已接入，8项定向全过，修复APK `android-02`成功，A段404.808553秒；保留原测试档安装／普通启动后报新的`RES_STORAGE`，B段354.195276秒，L1及三状态仍未运行。主程继续最小存储诊断与修正准备，唯一接执行结果；主策同步[试玩指南](../../demo/PLAY_FIRST_DEMO.md)，主测试接实际证据。源码仍为已审`1d75386b`，无新增源码差量，不重复审查。原两阶段15／20分钟边界及数据保护保持，首Demo仍NEEDS_FIX。详情见[主程当前记录](engineering.md)及[设备结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/result.json)。

**2026-10-17实际启动失败及修正候选：** 用户已确认模拟器原档为测试档；普通覆盖安装/图标启动/GLES3成功，但资源初始化报RES_STATE，L1与三状态恢复未运行。主程已收startup-fix01四源静态候选，替换Android不可用进程时间接口，保留玩家档并增加绑定前安全错误码。产品提交`1d75386b2ba30e6746cb0123b12e86dfa70f4907`已发布，[同head独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6228731368)无重大问题，源码门ACCEPT；本机未接入/编译/运行，待必要后验；首Demo仍NEEDS_FIX。唯一[候选与后验说明](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-fix01/result.md)，下方为此前里程碑。

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

**2026-10-18职责接回：** focus-run01及QA限定收件已完成，中央本轮临时工程收件结束；主程恢复日常工程分发/收件及`engineering.md`所有权。短C `01a14e81-4917-7440-b0db-b790ea4ddcde`已实核idle，继续作为可复用串行Unity执行者；旧长上下文C不自动恢复。当前没有中央另派的产品写入或运行；主程下一步仅整理既有英文战斗排版证据/源码定位。已完成运行合同不用于其他任务，旧累计保留，后续实际运行另据适用授权。

本轮指南事实同步临时由中央接管：主策接续因remote compact网络错误failed，未写文件（原SHA b72ea980…保持）；只据已收运行证据更新`docs/demo/PLAY_FIRST_DEMO.md`，不改策划方案。该组普通写入已停止，不并行修改。

本轮临时执行负责人/C：`01a14e81-4917-7440-b0db-b790ea4ddcde` / local / 请求Astra-xhigh，标题“FightMatch C-UI收尾｜短上下文”。用户2026-10-18明确要求换短上下文并沿用已批范围；旧主程接续回合因remote compact网络错误失败，仅有只读命令，没有本轮运行。新会话承接同一A≤600秒、B主动≤900秒许可，中央唯一收件；五位长期负责人身份保留，旧C/主程不并行执行。

新派工遵循[任务分级与短流程](../../../.agent/TEAM_WORKFLOW.md#0-按任务选择流程)；五位负责人保留，普通修正直接交执行者，只在需要设计时启用设计层。同一任务一份记录、一个接收者，中央只接跨组协调、里程碑或整体阻塞。默认Astra/xhigh，明确机械任务可Luna。

| 角色 | thread（host均local，云例外） |
| --- | --- |
| 中央 | 01a0e401-511d-79f2-b47f-3ab0ade1681b |
| 主程 | 01a0f2e3-1a80-7671-a459-38d5c8de0e6b |
| 主策 | 01a0f2e3-2c47-7273-88da-20513bcbfaf4 |
| 主美 | 01a0f2e3-3a47-78f2-819c-cbe52e8d7e47 |
| 主测试 | 01a0f2e3-4bd2-7130-b43a-19afa702e7bb |
| 唯一串行C（本轮运行已结束） | 01a14e81-4917-7440-b0db-b790ea4ddcde |
| 旧C（不自动恢复） | 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c |
| 源码执行 | 01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c |
| LOC | 01a0f40d-b0c5-7bc0-a2b2-be9d1213648a |
| 云测试 | 01a0f13c-3d87-753a-ad11-16302add9084/durable |

本机master125b849及既有WIP保留；PR均为草稿、未合并。Unity2022.3.18f1 Intel与大型工具位于外置盘。运行时uGUI、Editor UI Toolkit、英/简中、Luban、YooAsset方向保持。

历史按需查[2026-10-13入口原文](central-entry-history-through-2026-10-13.md)、[V08及之前](run-history-through-v08-native.md)；历史“当前任务”和旧一次额度不得用于新派发。负责人索引只读顶部当前段，其后历史按需定位。
