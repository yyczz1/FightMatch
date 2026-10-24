# FightMatch 首个 Demo：L1 试玩指南

更新：2026-10-18 · DEMO-NEXT-001 · 主策维护

> 当前 **android-06 已保档安装，原始未选人点击的语言外观及混合输入已完成限定验证**。结算入口、26经验/2锡片实际到账及三状态冷恢复沿用android-05已接收证据。英文战斗文字重叠、历史未归因退出和其余完整流程仍待处理，首Demo整体尚未验收；见[主测试](../team/2026-09-30/testing.md)。

## 1. 适用范围与启动前检查

- 本页记录已定 uGUI 正常玩家路线；当前 android-06 的身份及保留资料安装示例见第 1.1 节，实际通过与剩余问题以该节运行验收段为准，不把当前候选或历史失败包当作已完成 L1 验收的试玩包。
- 内容仅为“第 1 关 / Level 1”（L1，版本 1）、一名战士、两个空槽、单面 4×4 棋盘和两名发条步兵。使用占位美术；与正式产品共用业务和存档，不是另做一套演示数据。
- 真正的新资料为战士等级 1、经验 0、生命 100，背包及携带为空、无已学主动技能。已有资料保留真实成长、库存及战斗，不为匹配本指南清档。
- 没有下一关解锁、额外首通奖励、职业证、卡牌或已发布配方。不包含 L2、新职业、广告、后台服务、账号／云档、资源下载体验或最终美术。
- 界面上的语言下拉框提供“英语 / English”和“简体中文 / Simplified Chinese”；不是另一个“设置”页面。首次无手动偏好时按系统语言选择简中或英语，手动选择应跨启动保留。成功保存提示约 2 秒后消失；保存失败／结果不确定须按提示重新保存语言，不走战斗存档恢复。

既有 Mac 人工观察覆盖语言下拉、部分双语按钮、语言冷开记忆、原资料连续性和成功提示消失；本轮 Android 模拟器的限定通过项另见第 1.1 节。**模拟器不替代 iQOO Neo5 真机，也不代表所有页面和完整流程均已通过。** 本指南不新增 Unity 启动或设备测试授权。

### 1.1 当前 APK 与安装示例（本轮修正通过，整体仍待验收）

| 项目 | 本轮实际产物 |
| --- | --- |
| 文件 | [fightmatch-demo.apk](../../Builds/DEMO-NEXT-001/android-06/fightmatch-demo.apk)（`Builds/DEMO-NEXT-001/android-06/fightmatch-demo.apk`） |
| 大小／SHA256 | 46139736 字节；`66bafa41b30f773461bbd67642083f160ec3f4d1e2a4f5d1ccc663863f97290f` |
| 包名／版本 | `com.yyczz1.fightmatch`；0.1 / code 1 |
| 平台 | Android 7 / min API24，target API32；ARM64、IL2CPP、Development；仅 GLES3 |
| 公有签名证书 SHA256 | `2045e6c0b3dd319ae5682d10701ffcdd2337fdf5a539c7a065d1d7c8024bac51`，与现有普通包一致 |
| 当前边界 | 已保档安装并正常启动；语言修正限定验证完成，完整首Demo及Google Play发行尚未验收 |

依据：[运行回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/focus-run01/result.json)、[三项针对检查](../../TestArtifacts/FightMatch/DEMO-NEXT-001/focus-run01/focused.xml)、[构建报告](../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-06/build-report.json)、[签名](../../TestArtifacts/FightMatch/DEMO-NEXT-001/focus-run01/signature.txt)、[设备证据](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-06/result.json)。已审`af35b600`两源实际接入；三项菜单/混合输入、门禁、返回键检查 **3/3通过、零跳过**，一次构建及保档安装成功，整体主动执行含收尾873.734/900秒。没有第二次构建、下载、清档或全量测试。本机WIP边界保持，旧场景用例的两项布局前置失败不改写为通过。

**本轮设备实测：** 同一BlueStacks/API33/ARM64，标准Android输入。中英文均从“刚选择语言、收起菜单”直接开始，不先清焦点、不选战士，首次棋盘DOWN/hold/UP时语言矩形像素一致。键盘打开后触屏选项或遮罩关闭、触屏打开后键盘选择和Esc关闭均符合预期；Esc只收菜单，不退出战斗页。短拖线出现预览后释放，预览移除、无业务提交；拖线期间禁止切语言由指定自动用例证明，不把短拖线观察冒充第二指针门禁验证。安装前后及结束时所有可读业务字节一致。最终保留简中、第二局原红色锁定线、战士95/100、两敌0/15及15/15，战士仅在内存中被选中。

**复用的结算与恢复证据：** android-05已经显示中英文完整结算按钮；一次显式领取的[真实收据](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/settled-receipt.png)为26经验、2锡片，队伍0/60→26/60、背包持有锡片2。待结算、已结算、第二局首行动后三次真实进程冷恢复保持对应业务哈希和画面，已结算复开未重复奖励。本轮仅改语言组件，按用户许可复用这些已接收结果，没有在android-06再结算或重跑恢复。**26是现有测试档本次实得，不修改下文新建等级1路线的20经验预期。** 见[原设备回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/result.json)。

**仍有的缺口：** 英文战斗敌人名/状态文字重叠裁切、android-05初次未归因退出和主测试清单内其他完整流程尚未闭合；模拟器也不替代真机。已完成的语言修正不需重复确认，剩余项另按实际范围推进。

**以下是android-06保留资料安装示例。** 设备操作仍由唯一执行者按实际授权安排，本页不追加次数。先核目标设备、API/ABI及已有包身份，用现有SDK的`adb`，将“实际序列号”替换为目标；不卸载、清数据或强制降级。

```sh
adb devices -l
adb -s "实际序列号" shell pm path com.yyczz1.fightmatch
adb -s "实际序列号" install -r "Builds/DEMO-NEXT-001/android-06/fightmatch-demo.apk"
```

若签名冲突、版本降级拒绝或设备不满足要求，保留原安装与资料并核对原因。同签名覆盖安装不替代完整存档兼容验收。下文保留既定试玩路线，已观察范围以本节和主测试回执为准。

## 2. 最短试玩路线

以下列出完整试玩路线；本轮已观察的限定部分以第 1.1 节为准，其余成功现象仍是待验证目标。按钮名称中文在前、英语在后；若按钮禁用，先读当前原因，不反复点击或创建新请求。

1. **进入同一份本机资料。** 真正空资料时点“创建本机资料 / Create Local Profile”，先打开“创建本机资料？ / Create a local profile?”确认框，阅读“资料只保存在本机，设备数据全部丢失将无法找回”的告知；此时尚未发起建档。点“取消 / Cancel”或返回只关闭确认，不创建资料；点“确认创建 / Create Profile”才发起原建档请求。已有资料正常读取；若上次创建未完成，使用“继续原创建 / Continue Original Creation”或“重新读取 / Reload”，不再另建一份。成功现象：进入导航，读取同一份资料；重复点击不生成第二份进度。不要为试玩卸载或清数据。
2. **检查地图和队伍。** 在“冒险 / Adventure”看到“第 1 关 / Level 1”；切“队伍 / Party”检查战士和两个空槽，切“背包 / Inventory”检查实际库存。新档空包、暂无配方均正常。回“冒险”，点“第 1 关”，在“入场准备 / Battle Preparation”检查阵容，可点战士查看选中状态，再点“开始战斗 / Start Battle”。当前接线没有单独的“确认入场条件”按钮；正式检查由开始战斗请求执行。若已有活动局，改点“继续当前战斗 / Continue Current Battle”，不新开一局。成功现象：进入原活动局或本次新局，棋盘及敌我生命可见。
3. **先看免费参考，再选战士。** 点“播放参考 / Play Reference”，进入“已验证的开局默认参考 / Verified Starting Reference”，选择一个步骤查看棋盘覆盖，可重复查看，再点“关闭参考 / Close Reference”。参考只演示发布时验证的默认开局，不替当前战斗出手，不是任意局面的必胜提示，也不需要广告。准备页的选择不等于战斗选人：进入新局后点战斗中的“战士 / Warrior”成员按钮，确认出现“已选择”反馈。
4. **真实拖线两次。** 按下节路线，从端点按住，经横竖相邻格拖到同色配对端点后松手。完成第一条后等待表现，或点“跳到当前战局 / Skip to Current State”，再画第二条。成功现象：每次合法拖线只产生一次行动，目标敌人生命降至 0、路线固定；两名敌人都被击倒后出现“胜利已保存，基础奖励待结算”。参考覆盖本身不计行动；真实拖动开始时应关闭参考覆盖，不吞掉这次拖线。
5. **明确结算，再核对到账。** 等末段播放结束或明确跳过，待“继续基础奖励结算 / Continue to Base Rewards”可用后点击。成功现象：进入“战斗结果 / Battle Results”，显示本次真实收据。新建等级 1 战士按上述正常路线获胜，预期获得 **20 基础经验、2 锡片 / Tin Sheet**；已有成长档的经验受现有规则影响，以本次收据为准，不承诺每次重打都为 20。没有额外首通奖。用“查看当前队伍 / View Current Party”和“查看当前背包 / View Current Inventory”核对同一份资料的增量，不能只看表现文字。
6. **回地图或重打。** 结果页可点“回地图 / Back to Map”，应显示本内容段结束、暂无已发布且开放的下一关；仍可重新选 L1。也可在结果页点“重新入场挑战本关 / Challenge This Level Again”，以当前资料走正式新入场；进入后重新选择战士。它不是“按原开局重来”，不会复制上一局奖励或行动记录。保存复开检查见第 5 节。

## 3. L1 可照做的路线

棋盘为 4 列 × 4 行。这里用玩家侧 **1 起编号，左下角是（列 1，行 1），向右列数增加、向上行数增加**。不依赖具体颜色名；起点和终点仍须是同色的一对。

| 次序 | 手指／鼠标连续拖动 | 成功现象（待实际观察） |
| --- | --- | --- |
| 1 | 左下角（1,1）→ 上一格（1,2）→ 右一格（2,2），在端点松手 | 对应发条步兵被击倒，此路线固定；等待表现结束或跳过后继续 |
| 2 | 左上角（1,4）→ 右一格（2,4）→ 再右一格（3,4），在顶行第 3 列端点松手 | 第二名敌人被击倒，进入胜利待结算 |

不需要铺满棋盘，不走斜线，不跨格直跳，不穿过已固定路线。不要把点击参考步骤、短按端点或松开未到终点当成一次成功攻击。

路线依据为[正式 L1 内容](../../Assets/FightMatchContent/demo-r1.source.json)的 `SourceRoutes`；[内容编译器](../../Assets/Scripts/FightMatch/Content/DemoContentCompiler.cs)按已批准的左下原点转换坐标，[棋盘输入](../../Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs)也从左下向上计行。两条路径的边界、横竖相邻、对应端点及互不交叉已静态核对；本页没有执行 Unity、手机触摸或完整通关。默认参考还使用其自身已验证开局条件，当前玩家随机流不同，不承诺逐个表现完全一致。

## 4. 回退、按原开局重来和退出本局

这些是短分支检查，建议在第一条路线成功且表现结束后操作；不是完成最短通关的前置条件。每次先看预览，再决定确认或取消。

| 操作 | 当前入口与确认 | 应观察的结果 |
| --- | --- | --- |
| 回退 | “行动记录 / Action History”→ 选一条实际记录 → 检查回退数量和“回退后战局 / Battle State After Undo”→“确认回退这些行动 / Undo These Actions” | 仅撤销预览中的本局行动，恢复对应敌我生命和路线；可重新画第一条。选“保留当前战局 / Keep Current Battle”则不改变战局 |
| 按原开局重来 | “按原开局重来 / Restart from Original Entry”→ 看影响 →“确认按原开局重来 / Restart from Original Entry” | 结束当前尝试，按原入场阵容、成长、携带与随机初态重来；未完成的尝试不发奖。新尝试需重新选战士，不把它当当前成长档的普通重打 |
| 退出本局 | “退出本局 / Exit This Battle”→ 看影响 →“确认退出本局 / Exit This Battle” | 当前尝试结束、不发本局奖励、不自动开新局。当前代码先显示无奖励的结束记录，再点“回地图 / Back to Map”；不能把退出整个应用代替此操作 |

没有行动时历史入口不可用；保存待处理或正在播放时，回退等操作以当前可用状态为准。回退不撤销永久成长或已到账权益，也不能用来重复领奖。

## 5. 保存、恢复和语言复开

- **活动局复开：** 记录第一条路线后的敌我生命和历史，再按测试安排关闭、重开应用，应读取同一资料，使用“继续当前战斗”恢复原局，不能偷偷创建新局。本页未执行此检查。
- **结算后复开：** 先记录结果、队伍经验和背包锡片，再关闭重开，应保留同一资料和已结算事实，不因再次读取结果重复入账。不要清档、卸载重装或注入胜利来代替检查。
- **正常退出应用：** 导航根页“返回 / Back”打开退出确认；“留在游戏 / Stay in Game”取消，“退出应用 / Exit App”退出。这与“退出本局”不同；遇到弹窗时先按其当前按钮处理，不假设所有页面的返回键都直接退出。
- **保存受阻：** 保留原资料和诊断信息，只使用当前界面开放的“查询原结果 / Check Original Result”“重试原保存 / Retry Original Save”“确认原保存结果 / Confirm Original Result”或“继续选中的原保存 / Continue Selected Original Save”等原请求入口。未知结果不等于已失败；不可另建资料、另开战斗或改发一次新结算绕过。若没有可用恢复入口，记录当时页面和诊断码交主程／主测试。
- **已有旧资料：** 如界面要求“升级本机资料 / Upgrade Local Data”，先查看当前升级预览并按原流程确认；不把升级当作重新建档。
- **语言复开：** 在允许切换时选择英语或简中，确认文本更新和保存反馈；重开应保留手动选择。若出现 `【if you see this, it is a bug.】`、未替换的 `{参数}`、缺字或溢出，记录页面／语言／按钮，不将其当作正常成品文案。语言保存问题与战斗业务存档问题分别处理。

## 6. 首 Demo 仍需交付／观察的内容

1. 本轮语言修正已完成限定验证，结算/奖励/三状态恢复复用有效证据；仍需修复英文战斗文字重叠并查明历史初次退出原因。其余完整流程沿主测试缺口清单推进，复用已通过结果。
2. 主测试组织当前版本完整 L1 人工路线：创建／读取、地图／队伍、入场、参考、选人拖线、回退／重来／退出、胜利结算、重打和两类存档复开；已有有效证据只补缺口，不机械重跑全量。
3. 在实际交付版本检查中文和英语的相关页面、棋盘、弹窗、触摸命中与安全区。Android 模拟器观察与 iQOO Neo5 真机观察分别记录，模拟器不能替代真机；Windows 原生状态也单列，不写成已通过。

已核清的接线与修正：建档前可取消确认是现行要求，已补接，不再使用此前点击即直接建档的路径；真实 Prefab／catalog 的建档确认单例修正后通过，原返回键／语言反馈两项通过记录复用。准备页仍由 Start Battle 执行正式入场检查，准备页选人不替代战斗内选人；退出本局先显示真实无奖励结束记录，再回地图，符合既定业务目标。限定用例和静态接线不代替完整视觉、L1 或设备观察，也不由本指南批准产品方案变更。

当前不需要为写出可试玩 L1 追加关卡、职业、奖励、广告或最终美术。本轮指南验收仅要求：正式中英标签与实际入口一致、路线可由配置追溯、关键操作有可观察结果、静态证据与待实测清楚分开。

## 7. 失败包与旧 Toolkit／APK 资料（仅历史）

**android-01 是启动失败的历史包，不再推荐安装或试玩。** `Builds/DEMO-NEXT-001/android-01/fightmatch-demo.apk`（36583902 字节，SHA256 `4d72f265bd437d9d0e5131128ae46f71fb4fd968310f3a2d93344a791c017ab6`）虽于 2026-10-16 构建及静态包检查成功，但 2026-10-17 已实际安装、从普通图标启动，初始化报 `RES_STATE` 并停在诊断占位符，未进入 L1／奖励／恢复验收。失败事实保留于[模拟器原回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/resume-result.json)，不要把它与第 1.1 节的 android-06 混用，也不通过清档修复。

**android-02 是随后仍未进入玩法的历史包，不推荐 L1 试玩。** `Builds/DEMO-NEXT-001/android-02/fightmatch-demo.apk`（36583902 字节，SHA256 `e50b711ecbebc7d380fa62d1abd5d7bbedfda0760b292eba2cf6eb17ea8b2480`）的[8 项定向及构建阶段](../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/result.json)全部通过、构建成功，阶段用时约 6 分 45 秒。[emulator-02 回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/result.json)记录保留资料安装和普通图标 GLES3 启动后报 `RES_STORAGE`，设备阶段 354.195276 秒；L1／三状态复开／奖励均 `NOT_RUN`。当时 6 份可读业务文件哈希不变、2 份 lock 拒读；该包的检查通过与运行失败均保留，不由 android-03 诊断替代或改写。

**android-03 为历史诊断包，不是存储修复包。** `Builds/DEMO-NEXT-001/android-03/fightmatch-demo.apk`（36583902 字节，SHA256 `72186bd6acf65fba5bd02f6d7da3c67ad2ca532d768fddc68172850e4ebd47ee`）的[6 项检查与构建](../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/result.json)通过；[emulator-03 回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-03/result.json)记录标准 `MAIN/LAUNCHER` 启动、图标自动点击未验证，安全日志为 `activation.guard.ancestor.entries / UnauthorizedAccessException`，仍报 `RES_STORAGE`。该包 L1／三状态／奖励均未运行，原 6 份可读业务档保持、2 份 lock 未读；这些事实不由 android-04 的进展改写。

2026-09-29 的正常 019 包 `Builds/FMDemo029/mac-r1/019/fightmatch-demo.apk` 和 QA 018 包 `Builds/FMDemo029/mac-r1/018/fightmatch-qa.apk` 是旧 UI 时代的构建，不代表当前 uGUI 交付。019 曾在 BlueStacks 以 `-force-gles30` 到达“创建本机资料”，默认 Vulkan 启动仍崩溃；完整流程和冷启动未验收。不要按旧按钮、旧安装命令或旧 Min SDK22 声明推断本轮状态，也不要把 QA 包当普通试玩包。

旧包身份、签名、编译／测试和设备缺口保留在[原交付记录](../system-design/2026-09-17/demo-029-mac-r1-delivery.md)。其中旧日期的“暂停”“未推送”和旧验收数字只描述当时输入，不覆盖当前版本。

## 8. 本页核对输入

- 任务：DEMO-NEXT-001；中央派发、主策交付；仅修改本页，未改产品、配置、文案表或资产，未运行 Unity／Git。
- 内容：`demo-r1.source.json`，SHA256 `fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a`。
- 正式文本：[FightMatchText.csv](../../Config/FightMatch/Localization/FightMatchText.csv)，SHA256 `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030`。实际入口以 Host、PlayerNavigationView、PlayerBattleView 和 PlayerBattleSession 的绑定为准，不把表中尚未绑定的键写成可点击按钮。
- 既有观察基线：2026-10-16 接续入口所记 `c3f0812` 窄 UI 收件；本轮建档确认和 APK 证据另列，不扩大旧视觉观察范围。本页核对不是首 Demo 的 `ACCEPT`。
- 历史静态收件：原作者 actual turn `01a1430e-b2f7-75e0-bc67-e2437c3fcb11` 确认当时入口、战斗选人和路线坐标；其中直接建档已被本轮确认门修正取代，其余路线沿用，不把该只读回执当运行证据。
- 既有建档修正依据：[原定向记录](../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused.xml)为返回键、语言反馈两项通过及建档确认一项失败；修正测试基线后的[建档确认单例](../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused-fix01.xml)通过，不改写旧失败。
- 当前修正／产物依据：第1.1节`focus-run01`、`android-06`、`emulator-06`，执行者C actual turn `01a14ecc-21d4-7eb0-9865-c48bfc517784`；结算及恢复沿用`emulator-05`已接收记录。最新源码审查为af35b600/[6249275509](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6249275509)，实际运行无后续源码差量。中央按已登记的指南事实同步职责更新，不改策划方案、不增加执行；最终限定收件见主测试。
