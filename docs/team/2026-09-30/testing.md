# FightMatch 主测试线：QA-001

## 当前：DEMO-NEXT-001 首 Demo 剩余验收（2026-10-18）

首次差距核对主测试actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a14307-8195-7750-a9b1-7737a4d084f6`；早期测试/构建收件actual `01a143b7-ee6b-7231-bd8e-94fb6d0876da`，最新收件actual见下段。依据中央用户任务及组间交接，只更新本文件、只读现有报告/XML/log和产物身份；QA没有运行Unity、测试、构建、设备或本地代码审查。原UI输入为产品`c3f0812404a1223a4b5024cca7617aa054e4cc40`及实际589文件候选`c51cd9d7…`；后续本机确认门/构建候选见下方，不能混称整个clean PR树全测。主程负责实施/执行，主策维护[唯一试玩指南](../../demo/PLAY_FIRST_DEMO.md)，QA维护测试证据与独立审查门，不重复等待C。

**首Demo集成结论仍为NEEDS_FIX：6babe331对应android-05已构建；本轮自动检查1过/2失败。模拟器已实见中英结算入口、显式领取26 XP＋2锡片、待结算/已结算/首行动后三段冷恢复及本次冷开不重奖。** 原用户序列“刚切简中关菜单→未选人首次按棋盘”的语言控件色差仍在；中性焦点对照通过不覆盖它。首次未归因退出、英文战斗文字重叠及其余PQA缺口保留。android-04的7/7、BusinessReady及既有活动局恢复有效；旧CUA无响应不再写成全局输入失败。原902006fa诊断6/6与android-03的RES_STORAGE、1d75386的8/8与android-02、旧RES_STATE各自保留输入边界，不覆盖失败历史或合称同批全测。已完成的窄UI修正不重开：c3f0812独立GitHub审查无重大问题、[第二次GUI日志](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/ui-button-dropdown-01/gui-resume-20261015-02.log)编译成功；用户确认真冷开直接English、原档/进度正常、切简中再English的成功提示约2秒消失。c07d807等宽左对齐、Esc只收列表、可见双语按钮不超框及Back/提示回归历史保持，旧2项headless失败不改写为全通过。

### 最新运行收件：board-input-run01／android-05限定通过，原序列色差未闭合

QA唯一收件actual `01a14e96-99ca-7e23-8d69-2758ab40e0d3`，依据中央本轮派发及真人新增A≤600秒/B active≤900秒许可（卡`call_0f527edc517c4f39b419f95b032fc1e9`）。新短C `01a14e81-4917-7440-b0db-b790ea4ddcde/local`／actual `01a14e81-4a4b-7e13-a5ef-c64d3607efa1`执行。QA实读[A原始结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input-run01/result.json)、完整[三例XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input-run01/focused.xml)、[B原始结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/result.json)、目标日志及实际截图，独立只读核源码/APK身份、业务SHA对和色差；不重审源码、不运行Unity/设备、不追旧C。受审6babe331源码门复用下方原始结果，无新产品diff。

- **自动后验与输入：** 精确三例为**1 Passed/2 Failed/0 Skipped**，fullname集合与批准filter相同。`ActorlessBoardPointerKeepsLocaleStateAndLegalActionCommitsOnce`在实际手势前因预期BoardFrame、实际LayoutDiagnostic失败；`PendingVictoryRevealsSettlementUntilExplicitReleaseCommitsOnce`在实际点击前因战士按钮非最前raycast失败；现有`DragAndRecoveryGuardOpeningAndPendingChoiceSynchronously`通过。显示修正候选只在隔离证据目录，**未应用/运行/发布**，不记3/3。Host SHA `e11204673858b9eb88410a269d1b1e10a6a4c4f53564336d9e89dff2d7aee060`、Battle `a9e4fce156befbdcb43d039d95cf9af2625ef4545d24c275cc2e5460b5ad5715`匹配受审输入；本机测试SHA `b66985b8296c1a451fa535a6c54f406af3e081459eec8347ea0b56040d2f4fdd`保留旧WIP，与发布测试`043296f4…`不同，不称整个clean PR树已测。
- **构建与预算：** 首构建进程因未提供`GRADLE_USER_HOME`在Sign/CheckPrivateCaches退出，尚未进BuildPipeline；[失败日志](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input-run01/build.log)保留。同入口补既有缓存环境后，唯一实际BuildPipeline成功；[BuildReport](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-05/build-report.json)Succeeded、0错误/0警告，IL2CPP/ARM64/Development、Auto=false/仅OpenGLES3。[APK](../../../Builds/DEMO-NEXT-001/android-05/fightmatch-demo.apk)36583902B／SHA256 `c468bae5255fcd99ab639a2cce3baca9f70feb56441be2e3ff1ea6b1a50a683e`；[manifest](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input-run01/manifest.txt)API24/target32，[签名](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input-run01/signature.txt)原证书保持，[设置](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-05/temporary-settings.json)已恢复、保护delta空、Editor已退。A上海18:15:39.265568～18:24:10.562344，连续**511.292055500/600秒**；B18:24:41.797928～18:36:48.528032，已具endedUtc，active **726.723513250/900秒**、humanWait=0。625.448692秒仅最后设备动作采样，不是最终耗时。旧预算/超限历史独立保留，不用新许可冲抵。
- **真实输入与结算：** 固定既有BlueStacks API33/ARM64，以标准`adb shell input touchscreen`的tap/swipe/DOWN/MOVE/UP送入，非硬件鼠标或真机；未注入回调/业务状态、未卸载清档。一次保档安装前后10份可读业务文件SHA相同，不外推所有userdata。[简中待结算](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/pending-after-cold.png)与[英文待结算](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/pending-english.png)均实见完整结算按钮；一次普通点按后[结算收据](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/settled-receipt.png)明确26基础XP、锡片×2、木材×0、已结算不重发。[XP基线](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/pending-hud-xp.png)0/60→结算后及冷开后26/60；[冷开后背包](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/inventory-after-settled-cold.png)持有锡片2/装备0。旧档按真实收据接收，不硬套新Lv1路线20XP；旧锡片0的基线行部分裁切，不冒称完整清晰基线图。
- **三段冷恢复：** 各段均只force-stop本包→pidof无PID→正常MAIN/LAUNCHER→新PID，未以Stop/Play替代。待结算3233→3340，前后10业务文件SHA同、95/100及两敌0/15/两固定线/待结算保留；已结算3340→3550，12文件SHA同，回地图后[队伍26/60](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/party-after-settled-cold.png)和锡片2保持，本次冷开无重奖；下一局首行动3550→3863，16文件SHA同，[行动前后状态](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/after-one-legal-action.png)与[冷开后](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/action-after-cold.png)同一固定线、95/100、两敌0/15与15/15、AwaitAction及简中偏好。该局合法线(1,1)→(1,2)→(2,2)恰新增一对snapshot/commit，旧业务文件无改删。**仅收本包/此模拟器/此测试档的三状态与一次提交/不重奖范围**，不扩成真机、Home、系统重启、所有Activity或所有重打路径通过。
- **原序列色差仍在：** 未选人英文按/放及简中中性焦点对照的语言框矩形(464,48,672,144)像素完全相同、业务14文件SHA相同。但[刚切简中关菜单](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/actorless-zh-before.png)→[首次按棋盘](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/actorless-zh-held.png)→拖动/释放仍有色差，QA独立核(470,55)由RGBA(49,59,71,255)变为(51,61,74,255)。没有伴随行动提交、菜单展开或语言值改变；[中性焦点对照](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/actorless-zh-neutral-held.png)通过不能替代原分支。C的“残余选中/高亮回Normal而非Disabled”解释与像素相容，但**无当时EventSystem选中对象/状态实读，只是支持性推断，未确诊**。
- **其他未闭合：** [PID3021日志](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/pid3021-logcat.txt)349–350行先onDestroy后自身SIG9，首次退出原因未归属；空crash buffer不证明无错，随后三次受控冷开正常也不抹除它。[英文待结算图](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/pending-english.png)敌人名/状态明显重叠裁切，属于功能可读性缺口，不按占位美术豁免。新两例自动化失败、原序列色差及其余PQA未观察项均保留，**唯一集成结论NEEDS_FIX**。仅更新本索引并向中央一次回传，不自动消费剩余额度或新增验证链。

### 最新GitHub收件：board-input01／6babe331源码门ACCEPT

QA唯一收件actual `01a14e65-c261-72d1-8893-e7426f15b8ba`。已核[PR22](https://github.com/yyczz1/FightMatch/pull/22) head `6babe331f048366e058e97b33b92213dc3a4e653`、parent `e1e2ab8b27dfd3292eb44fd9cdac46d0a81eebfe`、固定base `dc1bba0537779983647f5e261f8e1c33a1210a49`，draft/open/unmerged；实读发布diff/PR最终合同，对parent恰1提交、6路径：HostView +4/−3、PlayerBattleView +33/−1、UguiSceneCompositionTests +230/−0及三入口文档。只读核[三暂存发布候选](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/board-input01/)SHA为e1120467…、a9e4fce1…、043296f4…，本机旧Canvas/Responsive WIP未纳入本轮发布。合同保持完整语言打开/选择/收起门禁、actorless历史、真实行动一次提交及原播放/保存/显式结算；仅在结算可用边界把已有按钮移入视窗，不自动领取或每帧抢滚动。这是范围收件，非QA另做本地代码审查。

首查服务与请求只到15378d05、无本head自动运行。长载荷请求被auto-review以公开目的地披露风险拒绝，未发出；只读确认仓库public后，沿同一评论入口提交**仅已发布提交号及PR正文引用**的最小请求获准。[唯一实际触发6248396941](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6248396941)对应[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于2026-10-18 **17:49:37上海Completed**，精确head匹配、has_any_findings=false。[原始结果6248425619](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6248425619)为“Codex Review: Didn't find any major issues. Hooray!”，Reviewed commit `6babe331f0`。17:50:19终核head/base未变、draft/open/unmerged。**ACCEPT（仅该head源码审查门）**，issue comment非formal APPROVED；无新增review submission/inline，三个旧讨论及回复未变（两P1 outdated、下拉P2未outdated，均未resolved），不代关闭。无本地R或重复请求。

**本段为17:50源码门收件时点：** 当时中央交接及[主程记录](engineering.md)为列表已收起、未选战士点连接点仍变色、选人后合法线能攻击；用户图为两敌0/15、两固定线及胜利待结算。新三例尚待后验，新源码UNCOMPILED/UNTESTED、旧APK不含修正；QA没有把战胜截图当奖励收据/冷恢复通过。后续实际1过/2失败、android-05和限定奖励/恢复结果见顶部，不再把未运行当当前状态。**唯一集成结论NEEDS_FIX**，源码门、测试、APK与实际奖励/恢复分开。旧CUA无响应保留为工具观察历史，不据此认定触摸不支持。当轮仅回中央源码门，无Unity/设备/旧检查重跑。

### 前轮设备增量：定点输入无可见响应，新增B上界1188/1200秒

QA actual `01a14dfd-29e3-78e2-a848-6a667c928bcf`仅接主程最后设备差量，实读完整[input-resume-result.json](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/input-resume-result.json)并查看[input-before.png](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/input-before.png)/[input-after.png](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/input-after.png)。**最新状态以resume2为准**，顶层仍是首段历史，不误报当前端点持续不可用。执行者`/root/input_resume_c`已由主程确认completed，未由QA追问或追加运行。

- **首段保留：** 实测285.187215秒、保守计288秒；默认沙箱ADB批次未在首败后短路，触发三次失败daemon fallback，未成功启动新server。标准同入口提权获准后原端点not found；原生窗口为BlueStacks商店，My games点击/按住/聚焦比较无导航，不算游戏按钮测试。零字节失败截图已由执行者删除、原回执保留失败；不是审批拒绝或游戏触摸根因。
- **resume2实际观察：** 一次标准connect恢复原端点，必要MAIN/LAUNCHER无extras进入同包PID3051，此为到达目标准备，不增加启动/冷恢复验收通过项。前后实际图均AwaitAction、战士100/100、两敌15/15及同4×4无固定路线棋盘；两图各99613B、SHA256均`3836b42034de84716174a4edd53a0b920bbab7a1e2cc04be773805398be3ba1b`。许可按钮原生普通点击及800ms按住点击各一次，均无弹窗；语言控件点击一次无展开。执行记录中的光标位于目标内，但没有Down/Up事件轨迹，未确定键盘焦点、未做Submit；截图/高亮不能证明点击已送达。未重读profile身份，不把视觉相同外推为新存档字节核验。[限定input-logcat](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/input-logcat.txt)实核0B，记录命令exit0／仅PID3051、Unity:I、*:S、末100行，**这是缺少诊断输出，不是无错误证明**。
- **时间与执行偏差：** resume2北京时间15:44:34.485～15:48:18.004，单调时钟实测223.519360秒、保守计226；143.610464秒最后现场取证请求后只收尾，仍**超内部180秒段限43.519360秒**，不截断或写全程合规。新增B保守累计上界`674+288+226=1188/1200`，账面余12秒，主程明确不再运行；原A359.653499667/600及旧A692.094034167/B800.433024334各自保持。原B result缺endedUtc/438秒末采样不改，新分段结果不伪造原段精确结束时间。

**唯一集成结论仍NEEDS_FIX。** 7/7、android-04构建、BusinessReady及既有活动局冷恢复的有效边界不变；本增量无新选人/合法路线、完整L1、新行动后/待结算/已结算恢复或奖励通过，无新代码、安装、清档或Unity运行。原因仍未定，不能称触摸不支持。主程已向中央一次提交最少人工许可按钮分流建议，待实际人工结果；QA不另开研究、重测、重审或重复询问。

### 前次运行收件：修正7/7、android-04启动与既有活动局冷恢复通过

QA actual `01a14dec-7f6b-7a62-b247-122ea622a685`接主程actual `01a14de2-4812-7dd0-876b-a05779c85657`的一次回执及计时纠正。真人权源为中央turn `01a14d92-0467-7160-9615-f3cd56411ab6`／item `01a14d92-05a6-7750-8eba-560ed55ecc72`对卡`call_3c691ce4b43b48878d7818e83740825c`回答“允许追加收尾验证（推荐）”：新增A≤600秒、B≤1200秒，旧A692.094034167/B800.433024334单列，不借旧余额。QA实读[阶段A结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix-run01/result.json)、完整[七例XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix-run01/focused.xml)、[设备原始结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/result.json)及目标日志，查看[恢复前](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/active-before.png)/[恢复后](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/active-after.png)，只读核三共享源、APK及六业务hash身份；未运行Unity/设备、追C或重复审查。

- **自动后验及构建：** AS04/05、AS16–20精确7/7 Passed、0失败/0跳过，XML fullname集合与批准filter相同；测试PID18273自然exit0／78.536988秒，构建PID18681自然exit0／162.492035秒。三共享源SHA/字节数等于已审15378d05候选。[android-04 APK](../../../Builds/DEMO-NEXT-001/android-04/fightmatch-demo.apk)36583902B／SHA256 `640711899c7fc80212be9d516af4423f9fdc7decd30bf2ceecc81d5864162066`；[BuildReport](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-04/build-report.json)Succeeded／0错误0警告、IL2CPP/ARM64/Development、Auto=false/仅OpenGLES3。[manifest](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix-run01/manifest.txt)API24/target32及[原签名](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix-run01/signature.txt)保持，[临时设置](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-04/temporary-settings.json)restored=true，保护delta空、Editor已退。新增A北京时间13:57:05.437014～14:03:05.089098，连续359.653499667/600秒；BuildReport时区标签偏差仍以外层UTC/monotonic为准。本机WIP/字体与clean树差异保持，不称整PR树全测。
- **真实启动/资源写入及限定恢复：** 一次保档install-r后标准MAIN/LAUNCHER生产入口到战斗；`resource-state/v1/active.json`实际为schema2／`fm.builtin.r1.android`／BusinessReady。一次仅本包force-stop，停后无PID，再启动PID12131→12288；两张实际图均为等待行动、战士100/100、两敌15/15、同棋盘且无固定路线。activationId `0310055a71d84aa5b56067224129bf64`及descriptor保持，processToken从`a2-b13-p12131-e161544670`变为`a2-b13-p12288-e161801590`，备份保留旧token。**仅PASS_EXISTING_ACTIVE_STATE_ONLY**：恢复的是已有活动局，不是首条新行动后恢复。六份可读locator/snapshot/commit与旧轮、安装前后及本次冷恢复前后SHA相同；两锁未读，不外推所有数据。旧“resource-state不存在”只属旧包现场。
- **输入/未验边界：** C记录选Warrior两次、真实棋盘拖动及重开后聚焦窗口再Back点击，均未见状态变化；主程另转真人“简体中文只高亮”发生于BlueStacks实际游戏，不冒称QA亲测。原因未定，不推断事件是否送达、触摸不支持或业务失败。固定恢复按钮可见不等于未决保存，未点击重试/解决。完整L1、新行动后恢复、WonPendingSettlement/已结算恢复、奖励恰一次仍未验；图标入口、API24实机/Activity/系统重启及真机不因本轮通过。[启动日志](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/logcat-start.txt)与[冷开日志](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-04/logcat.txt)为对应PID/空CommandLine/GLES3，仍有libEGL错误，不能写“日志无错误”或据此归因输入。
- **不完整收尾与计时纠正：** 原C actual `01a14d94-a9f2-7dd0-8b5f-2485fd9a442b`及离线收尾actual `01a14de5-f58e-7c23-8bec-6bcbbfc06de1`均由主程核为remote-compact网络systemError，后者新工具/设备动作0，不是Unity失败、权限拒绝或用户暂停。原B result仍无endedUtc；438.106977750秒仅末次采样，不伪造精确最终耗时。**撤回内部14:23:39截止即“余量0”的解释**：主程复核真人卡只限新增段20分钟；原回合completedAt=1792303970（14:12:50上海）、末工具已完无pending，从14:03:39.470581起至终止秒上取14:12:51，保守上界552秒；连无新工具的离线段额外保守计122秒，则新增B上界674/1200、余至少526秒（定点接续前口径）。QA核算术，执行终止身份依据主程审计，不把上界称实测精确值，也不把回合已终止后的闲置计作操作。原result不改；当时主程已记录≤300秒短接续及单一执行职责转移、QA尚未收结果，现接续实际差量见顶部，不把674秒旧口径当最新累计。

**唯一集成结论NEEDS_FIX。** 15378d05独立源码门沿下方原结果复用，七例、构建、实际BusinessReady及既有活动局恢复均有效；剩余输入/L1/结算奖励缺口由主程同任务收口。QA仅更新本文件并回主程一次，不再触发GitHub、不诊断源码、不派现场执行，也不并行等待接续结果。

### 既有GitHub收件：startup-storage-fix01／15378d0存储修正源码门ACCEPT

主测试唯一收件actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a14d89-e6ae-7233-a05a-edae9262fc67`。已读[PR22](https://github.com/yyczz1/FightMatch/pull/22)正文与[主程已签平台合同](engineering.md)，核head `15378d05f0f8d8d27cd60f8dfc5b4d0e76ada0a2`、parent `902006fa002088556caa69002ecc1b021c9146f1`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`；实际一提交、22净变更路径，仅Store/StoreTests/PlayerHost三产品源，其余为接续、合同、试玩说明和诊断证据，无APK/玩家档。只读核三暂存候选、[补丁](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix01/source.patch)26218B／SHA256 `b8be629b16cb166ba6d5bdc5bed9f3510e9dc280e9c3c8ba0554b5599ae9b134`及本地[最终result](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-fix01/result.md)13173B／SHA256 `b9dff2a5084a5ec881079ec642cab4c2c5513e261bb2376580e6e0e56e26c08d`匹配。**最终result未包含在该受审head**，不冒称服务读取了本地文档；已发布engineering合同、PR正文及固定补丁确定本轮范围，文档后补不移动受审输入。

合同明确Android系统canonical应用目录锚A由Host显式传入，productRoot精确绑定A/FightMatch；锚上名字不再由应用枚举独立证明，但每次IO仍核全链目录/native无链接及锚身份，A及以下精确枚举/大小写/链接/布局保持，Host创建前校验、创建后复核。**这是已签平台证明责任调整，不称与旧模型完全等价**；禁止由任意产品根或token反推策略，访问/探测错误仍失败关闭，桌面及记录/提交/UnknownCommit/玩家存档边界保持。以上为审查输入合同，不是QA重复本地代码审查或native实测结论。

中央[唯一触发6246212428](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6246212428)对应[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于北京时间2026-10-18 **13:46:14** Completed，精确head匹配、`has_any_findings=false`。[原始结果6246233251](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6246233251)为“Codex Review: Didn't find any major issues. Breezy!”，`Reviewed commit: 15378d05f0`。13:47:05最终核head/base未变、draft/open/unmerged。**ACCEPT（仅该head源码审查门）**，issue comment不是formal APPROVED；无新review submission/inline，三个旧讨论及回复仍未变（两P1 outdated、下拉P2未outdated，均未resolved），未代关闭。

**本段为13:47源码门收件时点：** 当时三源尚未接入/编译/测试，7项、修复APK及同档实际启动/资源写入仍待，旧诊断6例和902006fa审查不覆盖修正。原A692.094034167/900、B800.433024334/1200不重置；当时A余207.905966秒不足，新增运行待中央确认，主程建议不当许可。后续真人许可及实际结果见顶部最新运行收件，不再把“尚未接入”作为当前状态。整体仍NEEDS_FIX，旧RES_STORAGE为历史失败，L1/剩余恢复奖励未验。当轮QA仅向中央/主程各回一次源码门，无重复触发、本地review、追C或自行增额。

### 前轮运行收件：诊断6/6通过，emulator-03确认失败操作

QA actual `01a14d6c-dc46-7723-a4f4-e28c7a1cb333`接主程一次最终回执；C actual `01a14d5e-4abc-7593-b740-4e86e8bfacf3`已由主程wait核completed/idle。QA实读[诊断执行结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/result.json)、完整[六例XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/focused.xml)、[设备最终结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-03/result.json)与目标日志，并查看实际[start.png](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-03/start.png)；只读核APK、四共享源、五份设备证据和六份业务hash记录，没有新增执行或追C。

- **诊断自动后验与APK：** AS04/05、AS13–15、H07精确**6/6 Passed，0失败/0跳过**，fullname与批准filter相同；测试PID7982自然exit0／54.919210秒，构建PID8181自然exit0／181.644621秒。四共享源等于902006fa候选。[android-03 APK](../../../Builds/DEMO-NEXT-001/android-03/fightmatch-demo.apk)36583902B／SHA256 `72186bd6acf65fba5bd02f6d7da3c67ad2ca532d768fddc68172850e4ebd47ee`；[BuildReport](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-03/build-report.json)Succeeded／0错误0警告、IL2CPP/Development、Auto=false/仅OpenGLES3。[manifest](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/manifest.txt)API24/target32/ARM64、[签名](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/signature.txt)原证书`2045e6c0…`、[设置](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-03/temporary-settings.json)restored=true均已核；执行记录保护delta0、Editor已退。本机旧WIP/字体与clean输入差异保持；新6例和旧8例分属不同输入，不写同批14/14。
- **真实失败操作：** [logcat第190行](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-03/logcat-start.txt)于13:08:52.312、PID11887报`stage=activation.guard.ancestor.entries; error=UnauthorizedAccessException`；第191行13:08:52.320与实际start.png均为RES_STORAGE。**祖先枚举失败阶段/受控异常类别已由同PID实测确认，不再只记猜测**；确切祖先路径、原生errno和更广平台行为未提供，不推断。记录中的issue.png是等待时桌面，不作为故障游戏屏；安全修正及修后运行仍未完成。
- **启动与数据边界：** 一次保档install-r成功；CUA单击/双击未证启动，人工卡已撤。中央/主程纠正普通方法后，先resolve-activity，再唯一标准MAIN/LAUNCHER精确生产UnityPlayerActivity启动成功、LaunchState COLD，空CommandLine/GLES3，无adb input/monkey/extras；这是生产入口Intent冷启动，**不是图标点击成功或存档冷恢复通过**。六份可读业务数据与emulator-02、安装前后、失败启动后SHA相同；两锁未读、权限未改，不外推全部资料字节。L1／活动局／待结算／已结算冷恢复／奖励恰一次均NOT_RUN，PID11887留故障屏，无玩法输入、force-stop、清档或手写技术状态。
- **连续预算：** 新A 287.285481542秒，累计692.094034167/900；新B北京时间13:02:42.919430～13:10:09.156012连续446.237748500秒，含等待与启动方法纠正，累计800.433024334/1200。原319.599023250秒收尾保留为provisional，已包含在同起点最终段中，不重置或双扣；剩余A207.905965833、B399.566975666秒。BuildReport时区标签偏差仍以外层UTC/monotonic口径处理，不增加额度。

**唯一集成结论NEEDS_FIX。** 902006fa源码门、六例及诊断运行有效，但诊断不是修复，图标入口、L1/恢复/奖励及真机均未通过。主程已交原作者同任务SOURCE-only安全修正准备；不因枚举异常自行放宽守卫、吞异常放行或调整信任边界。QA只更新本文件并向主程回一次，不重复审查/测试、追C或另报中央；旧失败与原始现场证据保持。

### 既有GitHub收件：startup-storage01／902006f诊断源码门ACCEPT

主测试唯一收件actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a14d5e-d498-7242-a75f-726d6bdcad50`。已读[PR22](https://github.com/yyczz1/FightMatch/pull/22)描述、[候选/设计与六项后验](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage01/result.md)，只读核四暂存候选SHA/字节数及[source.patch](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage01/source.patch)24900B／SHA256 `d84054d945b47afdafe0f6221d38c02c5e158d3f6a93fadda9a4eb4214089128`一致。准确head `902006fa002088556caa69002ecc1b021c9146f1`、parent `8f3d3e4fb74ad2324732f654504c32ca2e442d0f`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`；GitHub比较为一提交、**实际9路径＝4诊断源码＋3中央入口＋result/source.patch**。交接/触发所述10是构树请求项数，中央已确认engineering.md与parent相同，不构成净差量；无额外产品变化，不因此重触发或复验。

中央[唯一触发6245861297](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6245861297)对应[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于北京时间2026-10-18 **12:58:38** Completed，精确head匹配、`has_any_findings=false`。[原始结果6245876056](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6245876056)为“Codex Review: Didn't find any major issues. Breezy!”，`Reviewed commit: 902006fa00`。12:58:58最终核head/base未变、draft/open/unmerged。**ACCEPT（仅该head诊断源码审查门）**，issue comment不是formal APPROVED。没有新review submission/inline；三个旧讨论及回复未变，仍两P1 outdated、下拉P2未outdated且全部未resolved，不代关闭。

本候选仅补有界stage/层级及受控异常类别，不是根因修复；候选中的日志示例不是当时设备实测。提交审查时为UNCOMPILED/UNTESTED，审查收件时尚无六例/诊断APK回执；**后续实际结果见顶部最新运行收件**，不以旧1d75386审查/8项通过代替。原android-02的RES_STORAGE、六份可读数据边界、L1/三状态/奖励NOT_RUN保持，整体NEEDS_FIX。当轮只更新本文件并向中央、主程各回一次源码门结论；无重复触发、本地review、追C或追加运行额度。

### 前轮后验收件：startup-run01通过，emulator-02启动RES_STORAGE

QA actual `01a14d4c-9a28-7831-ba13-8c3588aec83a`接主程最终回执；C actual `01a14d36-0e46-7503-880c-9765e209ccd2`已由主程核completed/idle。权源为10月18日人类turn `01a14d27-e460-70b1-ab5d-a5ed1fa6aefd`／item `01a14d27-eb36-79a0-9a2b-ba591af92a3d`的新阶段许可，不复用旧绝对截止。QA实读两阶段结果、完整XML、构建/manifest/签名记录及目标日志，实际查看故障截图；只读核新APK、四共享源及设备证据身份，没有运行Unity/设备或追C。

- **阶段A通过：** [执行结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/result.json)及[原XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/focused.xml)为精确AS01–03、AS09–12、H06 **8/8通过、0失败/0跳过**，fullname集合与批准filter一致，不是4557项全测。测试PID1743自然exit0／64.840698秒；普通APK构建PID1966自然exit0／209.661718秒。[BuildReport](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-02/build-report.json)为Succeeded／0错误0警告。全段北京时间12:22:04.782147～12:28:49.588831，连续404.808553秒<900；原前检路径错误及BuildReport八小时时区标记偏差保留，不重置计时。
- **固定输入与产物：** [android-02 APK](../../../Builds/DEMO-NEXT-001/android-02/fightmatch-demo.apk)36583902B／SHA256 `e50b711ecbebc7d380fa62d1abd5d7bbedfda0760b292eba2cf6eb17ea8b2480`；QA实核四共享源等于已审1d75386候选，无本轮新代码差量，不重复review。[manifest](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/manifest.txt)为原包名0.1/code1、API24/target32、ARM64/GLES3；[签名](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/signature.txt)验证通过，原证书`2045e6c0…`保持。BuildReport为IL2CPP/Development、Auto=false/仅OpenGLES3；[临时设置](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-02/temporary-settings.json)restored=true，执行结果保护差量为空、Editor已退出。本机旧Build WIP/字体与clean树差异仍保留，不升级为整个PR树全测。
- **阶段B真实失败：** [设备结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/result.json)为`NEEDS_FIX_STARTUP_RES_STORAGE`。一次保留数据install-r成功／0.981235秒；普通CUA图标操作最终启动PID11584，记录3次输入尝试、实际启动1次，具体触发动作/延迟未证明。[logcat第191行](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/logcat-start.txt)于12:32:02.768报`FightMatch UI: RES_STORAGE`，CommandLine为空、GLES3，随后ShowFailure/InitializeHost。[实际截图](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/issue.png)为原诊断占位＋`RES_STORAGE`，**本次pre-bind安全code显示已实观**。阶段连续354.195276秒<1200；中间启动输入方法问题已撤为无需回答，未使用adb input。
- **数据及未验范围：** QA核设备记录中安装前、安装后、失败启动后三组相同的6份locator/snapshot/commit SHA；两份writer.lock拒读、未改权限，仅六份可读业务数据可作本轮字节保持结论，不推存档恢复通过。原profile/八叶名称保持，resource-state/v1仍不存在。L1、活动战斗／WonPendingSettlement／已结算三状态冷恢复、奖励恰一次全部**NOT_RUN**；XP/库存玩法基线未观察，不记0。原故障现场留活，无玩法输入、force-stop/冷重启、卸载/清档/手造状态或追加构建，模拟器不替代真机。

**唯一集成结论NEEDS_FIX。** 1d75386源码门与本轮8例/构建通过有效，但不覆盖新Host失败。日志未再见RES_STATE不等于JNI/API24、Activity/冷进程/系统重启所有分支通过。主程所述DirectoryGuard枚举Android受限父目录仅是候选；多个catch汇聚RES_STORAGE且原异常未采，QA不确认根因或批准放宽守卫。先由主程取得准确异常/失败操作证据再确定局部纠正；本次QA只更新原任务缺口，不增加审查、测试、设备或重建额度，旧失败证据不覆盖。

### 既有GitHub收件：startup-fix01／1d75386源码门ACCEPT

主测试唯一收件actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a147c5-b391-7912-910f-3c81becfeca9`。已读[PR22](https://github.com/yyczz1/FightMatch/pull/22)当前描述与下方已签候选记录，核head `1d75386b2ba30e6746cb0123b12e86dfa70f4907`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`；相对parent `79e736af81bff9100303bae7627666e937a0ee94`恰一提交、11路径（4源码＋3接续文档＋4记录），未夹带APK/字体/玩家档/本机旧Build WIP。中央[唯一触发6228712100](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6228712100)准确绑定该head及Android boot/PID/elapsed、schema2技术差量，QA无重复触发或本地审查。

[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于北京时间2026-10-17 **10:53:26** Completed，精确commit匹配且`has_any_findings=false`。[原始结果6228731368](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6228731368)为“Codex Review: Didn't find any major issues. Delightful!”，`Reviewed commit: 1d75386b2b`。10:54:25最终实核head/base未变、draft/open/unmerged。**ACCEPT（仅该head源码审查门）**；issue comment不是formal APPROVED，也不保证所有级别问题不存在。无新review submission或inline发现；三个旧讨论仍两P1 outdated、下拉P2未outdated，均未resolved且无新增回复，不能用本轮服务摘要将它们写成已关闭。

10月17日审查收件时，四源尚未接入本机共享源、编译、运行8项回归或构建修复APK；该时点不覆盖上方10月18日实际后验。JNI/API24/真实冷进程/系统重启及文件提交能力不因源码审查通过。原APK实际RES_STATE、L1/三状态恢复/奖励NOT RUN保留，**整体NEEDS_FIX**。当轮仅更新本文件并向中央回一份源码门结论，无Unity/设备/构建、追C、Git写入、merge/release。

### 历史候选收件：startup-fix01四源暂存（后验与审查见上方）

QA actual `01a147c2-f048-7f72-8af8-6edcbbca3e19`接主程候选里程碑，只读[唯一候选记录](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-fix01/result.md)及[合并补丁](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-fix01/source.patch)身份；四候选SHA/字节数与记录一致，四共享源仍等于记录前像，补丁28328B／SHA256 `1d6111a6e6b763838645c74fa5fc0ee5c3c43f992e52845335c87c08b3b4b576`。精确四路径为HostView、HostResourceTests、ResourceActivationStore、ResourceActivationStoreTests，完整路径/前后像沿唯一记录，不另造台账。作者actual `01a147b9-e6e1-7f92-8162-40cf4c0376e5`由主程核completed/idle；主程收件actual `01a147c0-97c4-7052-942a-1315b8dd05bd`。**当时SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING，整体NEEDS_FIX；尚未接入共享源，原APK与唯一实际RES_STATE红样本不变。** 本次是身份/范围收件，不是本地代码审查；后续独立审查以顶部1d75386收件为准。

主程已签的工程契约替换为Android `a2-b<bootCount>-p<PID>-e<startElapsedMs>`／schema2，不冒充启动UTC；桌面保持原v1。旧Android v1主/备或混合记录保留并fail-closed，不迁移/清状态；无ANDROID_ID、玩家存档规则变化或随机/墙钟fallback。旧程序不能读取v2，源码回退不等于资源技术状态可回退。pre-bind安全code候选已合并，正常本地化/解绑与诊断占位保留；这些是候选范围，不是实际运行通过。

建议最少自动后验沿result中的完整filter：AS01–03、AS09–12、H06共8例，**全部尚未执行**。注入token/样本不能证明JNI取值与缓存、API24、真实冷进程/Activity重建、系统重启或真实文件提交；L1、三状态冷恢复和奖励恰一次仍未验。主程已向中央提交精确差量及一次后验预算建议，**建议不构成Unity/重建/设备执行许可**，本次未派C。待中央固定新PR head后由QA唯一接GitHub独立结果，c78f8a5旧审查不覆盖这四源；不重复旧head、不另做本地review或追C。实际验证仍由主程一次回传。

### 历史设备收件：emulator-01已安装，Host启动RES_STATE

QA actual `01a147ac-1a79-7062-8485-81286328fc71`只读[接续结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/resume-result.json)、[安装结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/install.txt)、[logcat](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/logcat.txt)，并查看[故障截图](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/issue.png)。C actual `01a147a5-20ee-7771-b60c-d80f45cdafeb`已结束，主程已实收。10月17日用户确认现有模拟器档是测试档后，固定APK`4d72f265…`唯一一次保留数据覆盖安装成功（exit0/1.434142秒），普通图标一次启动、PID6236，日志命令行为空且使用GLES3；不是强制图形参数或QA入口。

**实际失败：** logcat第190行设备时间10:17:59.269为`FightMatch UI: RES_STATE`，随后栈`ShowFailure/InitializeHost/Start`；截图实际为`【if you see this, it is a bug.】`诊断占位。状态`NEEDS_FIX_ANDROID_STARTUP_RES_STATE`，不是安装/权限失败。**L1、活动战斗/待结算/已结算三状态冷恢复、XP/库存基线和奖励恰一次均NOT RUN/NOT OBSERVED**；未读取业务基线，不能把未观察数量写成0。现有profile及八个locator/profile文件名仍在，但未复读比对业务字节，不能宣称存档字节级连续性；故障进程和现场保留，无玩法输入、重试、清档、卸载或重建。

本段北京时间10:16:59.986469～10:20:16.854789，连续196.870357秒；昨日实际前检含额外只读枚举358.870665秒，累计555.741022秒。原绝对截止及首次安装0/启动0回执保留为历史；主程已按最新真人答复纠正内部时间约定，等待人类答复不计测试耗时，未另开30分钟，不将旧连续窗描述成仍在有效期。

**归因边界与接续：** 主程核定失败发生在资源激活前置、尚未完成资源准备/本地化/Session；占位符不是翻译表缺项的证明。C提出RealToken/Process.StartTime方向，主程本机IL2CPP源码的`GetProcessTimes`不支持事实支持该方向；后续主程已核静态调用链，实际设备异常仍未采集，**运行根因/修后闭环未完成**。最初要求保留真实PID＋启动UTC ticks；后续主程已明确批准Android a2/schema2技术契约替换，见上方候选收件，不能继续把旧UTC约定当Android候选合同。同进程稳定/新进程区分和玩家存档保护不变，不以随机值或当前时间绕过。QA仅登记红样本及候选缺口，不追加Unity/设备/构建诊断，不重复审旧head；必要后验由主程汇总后再接。

### 历史设备收件：10月16日安装前停止（用途阻塞已解除）

QA actual `01a14403-072d-7741-a7f9-090617d48574`只读[原结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/result.json)及[设备记录](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/device.txt)；C actual `01a143f9-2712-78d3-96d4-b7376a0cb819`已completed/idle，主程唯一执行收件。358.576141秒环境阶段已确认BlueStacks Air5.21.790.7505、`127.0.0.1:5555`、API33/arm64-v8a；ADB原已启用、配置未改。现装包精确匹配旧019 `cfdc39d8…`，待装候选仍为`4d72f265…`，不能把旧包识别当新包安装。

状态为`BLOCKED_EXISTING_PROFILE_TEST_PROVENANCE_UNVERIFIED`：本包外部目录只读枚举发现`p-083a69e8…f201`玩家档及两组snapshot/commit，旧9月30日/10月2日回执不能证明这份后来存档是测试资料。私有目录run-as读取被拒后未提权/改配置；仅列外部文件名，未证明存档内容或用途。按真人“仅测试资料、保留数据”在安装前停，**安装0、游戏启动0、游戏写0**；完整L1、创建确认/Back、三状态冷恢复及奖励观察全部NOT RUN，原包/原档保留，没有假install/logcat/截图。

当时中央唯一询问该档用途，原团队连续窗为北京时间**17:11:19.690534～17:41:19.690534**，等待计入、不重置。这是10月16日的原约定/停止事实；10月17日真人确认测试用途后已由主程明确纠正接续时间口径，见上方最新收件，原记录不覆盖。该历史用途阻塞不再是当前原因，当前原因是实际RES_STATE启动失败。

### 已完成GitHub审查：c78f8a5

主测试actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a143bf-de1a-7312-884c-410e7810a1dc`唯一收件。已实核[PR22](https://github.com/yyczz1/FightMatch/pull/22) head `c78f8a52ba589534171b5990328e9cac2d42a752`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`，draft/unmerged；相对parent `998257545595927abb39e9e252a6c46dc3bf98f0`恰一提交，5产品路径＋4文档＋7证据，不含APK二进制/动态字体/本机旧Build段。中央[唯一触发6211006177](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6211006177)已核，QA没有重复评论。

北京时间16:11:19最终实核head/base未变、draft/unmerged；[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)显示本head于16:09:59 Completed，`has_any_findings=false`。[原始结果6211066540](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6211066540)原文“Codex Review: Didn't find any major issues. Hooray!”，`Reviewed commit: c78f8a52ba`。**ACCEPT（仅该head源码审查门）**；这是issue comment，不是formal APPROVED，也不保证所有级别问题不存在。本轮没有新review submission或新inline发现；三个旧讨论状态未变（两个P1 outdated、下拉P2未outdated，均未resolved且无新增回复），不重计为新发现、不代服务关闭。

相应测试/构建沿下方已核原证据复用：原2过＋FIX01单例1过、普通APK252.466秒exit0及API24/GLES3/签名后检；不改写原红、字体保护停止或本机/clean差异。创建确认视觉、完整L1、战斗/待结算/已结算冷恢复、其余页面和Android设备门仍缺，整体Demo维持NEEDS_FIX。QA本轮只核服务/发布身份并更新本文件，无重复触发、Unity、测试、本地review、merge/release。

### 已收件：定向测试与首次普通APK

主程actual `01a143b5-00bf-7332-8ce2-5bc135f5e31d`交接，APK执行C actual `01a143aa-bddf-7e82-8fa5-72cf0d05560c`已completed/idle。QA实读以下原始XML、执行结果、manifest、签名及BuildReport摘要和正常Scene/Build Success日志，并只读核APK、执行结果、两XML和当前测试源SHA；无复跑或新reviewer。

- **测试分批接收：** [执行01](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/execution-01-result.json)／[原XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused.xml)实际3例：Back取消顺序/重绑、提示时限/解绑两例通过；建档确认例首次点击前`empty.Count`预期0实际1失败，87.807987秒/exit2。主程核实唯一文件为0字节`locator/writer.lock`，不是已证明提前创建业务档。FIX01仅纠正新用例的锁文件基线，按主程冻结差量保留后续完整文件/零业务写/确认/生命周期断言；最终测试源`d30177d5b1e3aaeea708f7bdc97a61ad7ce80a8056e57da62aeb8191d88522b5`。[FIX01结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/execution-fix01-result.json)／[修后XML](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused-fix01.xml)实际1过/0失败/0跳过，48.212324秒、自然exit0。**复用原2过＋修后单例1过，不写同批3/3。** 原红证据及FIX01字体保护门停止保留，不把wrapper停止改写为测试失败或整阶段成功。
- **普通APK构建证据成立：** [实际APK](../../../Builds/DEMO-NEXT-001/android-01/fightmatch-demo.apk)，36583902B／SHA256 `4d72f265bd437d9d0e5131128ae46f71fb4fd968310f3a2d93344a791c017ab6`。[本次执行结果](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/execution-android01-result.json) SHA `086104949b65698bbc937444cd662c0ca0fc32eaa56f41c5042d444037061cec`；新真人明确许可后唯一1/1构建，PID48183自然exit0，本段连续含落盘252.466047秒<900，FIX01累计约301.717秒<1200。没有新测试、GUI、安装、ADB或重试。
- **包与设置范围：** [manifest](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-01-manifest.txt)为`com.yyczz1.fightmatch`、0.1/code1、minAPI24/target32、ARM64、GLES3声明`0x30000`；[BuildReport](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-01/build-report.json)为IL2CPP/Development、Auto=false、单OpenGLES3，无IncludeTestAssemblies；[签名验证](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-01-signature.txt)成功、公有证书SHA `2045e6c0…`，主程已与旧019核同签名。日志801/803为正常FightMatchDemo Scene，4437为Build Success。实际Settings `fdb4a42b…`的原始diff仅minSDK/Android graphics列表/Auto三字段；[临时设置记录](../../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-01/temporary-settings.json)restored=true，执行结果报告正文精确恢复，其余保护输入保持。**0错误/1警告**，警告正文尚未定位；报告标UTC的时间比外层早8小时，保留原字段，实际耗时使用外层UTC/monotonic，不写零警告。
- **输入差异与后续门：** FIX01使TMP字体`58a6…→9d477445…`后曾触发保护停止；主程处置为保留/冻结本机输入、排除发布，未证明全部只是缓存。该字体在本次构建前后保持。APK采用本机Build旧WIP，**不是中央clean发布树构建**。五路径clean Build `7eddf74d…`、HostView `96c23d88…`、PlayerHost `77b7a7ca…`、Tests `d30177d5…`、真实Settings `fdb4a42b…`已由中央发布于c78f8a5，独立审查绑定上方新head，不以旧`9982575`或c3f0812审查代替。本轮只接上述限定事实，不关闭创建确认视觉、L1全链、战斗/待结算/结算后冷恢复、未到达页面及Android设备门，也不取得额外运行许可。

### 一张表看剩余工作

原8项编号简称Q1～Q8；PQA定义沿用[主策清单](planning.md#6-交给-qa-的可观察场景)。证据简称：**UI**＝上段已过实际观察；**旧自动**＝[Q4分组记录](../../../TestArtifacts/FightMatch/UGUI-01/q4-correction-22/grouped-exact222-coverage.json)及[FIX22回执](../../../TestArtifacts/FightMatch/UGUI-01/q4-correction-22/final-receipt.json)，绑定PR1 `3541930`时代原输入，只复用未改变的业务/契约范围，不升级为当前画面或整套通过。本轮抽读原09 XML（194过/28失败）和22 XML（12/12），原失败及后续分组修正不相互覆盖。**接入**＝[既有运行记录](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)的103项、Bridge5/HCP8、Fake3与平台13过/1跳过，分别属于其实际输入；资源raw构建不是APK。M＝C串行准备Mac界面＋实际观察者；S＝C按新合同运行安卓模拟器；D＝用户/设备执行者在真机观察。三者均不是本轮开跑授权。

| 策划项／原清单 | 已有有效证据（限定范围） | 实际还缺什么／缺口性质 | 可执行环境 |
| --- | --- | --- | --- |
| PQA-01／Q1、Q7 建档 | 原档打开/旧H03证据保留；新建档前可取消确认已接入编译，FIX01单例通过，c78f8a5源码门已接收 | 实际安全空档点创建→可取消确认→确认后地图，核仅一份资料、战士Lv1/XP0、两空槽、空包；取消/Back不建档。**缺当前确认界面观察**，不再写“没有确认实现”，不清真实档 | M；新包S/D |
| PQA-02／Q6、Q7 L1完整链 | android-05中英结算入口可见、一次显式领取收据26XP＋2锡片，XP0→26/60、持有锡片2及已结算冷开不重奖已实见；此前人工合法线至待结算保留 | 默认参考参与的完整新档链、四去向均逐一可用仍缺。新Lv1正常路线预期20XP＋2锡片，既有测试档按本次26XP真实收据核，不将其改写为新档全链 | S限定结算/奖励已过；M/S/D余项 |
| PQA-03／Q6、Q7 重打 | 旧自动保留；android-05结算冷开不重奖、从地图再次进入L1及一次合法行动已实见，无第二次结算 | 结果页“重新入场”按钮直达新本局、旧历史不复制及该去向的奖励语义仍缺；地图再入不替代结果按钮观察 | M/S，D顺路复查 |
| PQA-04／Q2、Q6 空态 | 旧自动覆盖空装备导航、缺配方不注入样例；264条正式文本与导出一致 | 实看队伍三槽/仅战士、空包、无配方及双语动态值。**缺画面观察**，不补假角色/物品/配方 | M；S/D看排版 |
| PQA-05／Q6 活动本局门禁 | 旧自动保留入场绑定/单次请求与导航控制证据 | 活动战斗中地图/队伍/背包可读；新入场、改阵容/装备被挡，继续回同一战斗。空包下不可达换装备不造内容强测。**缺真实入口观察** | M；S/D顺路 |
| PQA-06／Q6、Q8 真实输入 | android-05标准touchscreen输入下合法线恰增一对snapshot/commit；未选人英文及简中中性焦点按/拖/放未改业务/语言，控件矩形不变；旧CUA仅工具历史 | **刚切简中关菜单后的首次未选人按棋盘仍有色差**，中性对照不覆盖；焦点解释未由EventSystem现场实读证实。错端点/交叉/回撤/取消及真机多指、失焦/触点吻合仍缺 | S限定路径已观；M/S原分支；D多指必需 |
| PQA-07／Q6 默认攻略/播放 | 旧自动B10/B11覆盖精确绑定、真实首拖关闭参考、旧计时不能复活覆盖；P27保护播放token | 产品内已发布L1默认参考可打开、选步、重复看；按其适用开局条件真实拖线，首拖不丢手势、跳播不改业务。**缺当前参考和玩家链观察**；若匹配条件仍走不通，留步骤/条件交主程/主策，不注入胜利 | M；D触控复查 |
| PQA-08／Q6、Q7 回退/重来/退出本局 | 旧自动B03等保留取消零写、按原开局恢复及原请求语义 | 实看各自预览、取消/确认：回退仅所选历史，重来原开局，退出本局不等于关闭App。**缺当前按钮/预览观察** | M；D验证返回关联 |
| PQA-09／Q7 保存失败/未知 | 旧CC14/CC17原请求恢复及H04/输入去重证据保留；新提示生命周期定向用例已在执行01通过 | **不要求在L1人工制造磁盘故障，也不重跑已过提示用例。** 业务原请求证据沿未变范围复用；仅实际新增受影响保存路径才补原有故障fixture，正常状态冷恢复见PQA-10，不改玩家数据/数值 | 现有自动证据复用；新故障针对项须有实际差量/授权 |
| PQA-10／Q4、Q7 冷恢复 | Mac/旧既有活动局证据保留；**android-05待结算、已结算、首行动后三段force-stop冷开已过**，PID更换，10/12/16业务SHA分别不变，棋盘/HP/偏好保持，26XP与锡片2不重发 | 本包/该模拟器/测试档三状态不重跑；Home后台、其他Activity/系统重启及真机仍未验，不能外推所有中断和奖励路径 | S三段已过；D补后台/进程重启 |
| PQA-11／Q5 首次系统语言 | 旧自动LocalePolicy/LocalizationContract已过中文变体→简中、非中文→English | 不重跑解析矩阵；在无语言偏好的隔离安装/根实际观察中文与非中文两种首开。**缺平台接线观察**，无安全隔离则保留未验，不清原档 | M/S可配置测试系统；D实际系统，不以注入值冒充 |
| PQA-12／Q2、Q3、Q4 手选/重建页 | 旧Mac双向切换/English真冷开/短提示保持；android-05中英切换实见，首行动后冷开简中偏好保持，待结算中英按钮完整可见 | 原简中关菜单后首次按棋盘色差未闭合；英文敌人名/状态重叠裁切已见。其余页面/动态参数/缺字/尺寸仍缺，不外推全页面通过 | M/S剩余画面；D实际尺寸 |
| PQA-13／Q2 本地化诊断 | 旧诊断/动态克隆/正式264条/提示回归证据保留；H06及新增H07实际通过；emulator-02/03已见占位＋RES_STORAGE安全code，03同PID有限stage/类别已采 | **pre-bind诊断显示与本次有限日志已验证，不重列待验**；不等于Host正常本地化或翻译表缺项归因，其余页面/动态参数/缺字及字体显示仍缺，不破坏正式表造缺key | S故障code/有限日志已实观；M/D剩余画面 |
| PQA-14／Q8 安卓 | 6babe331源码门复用；android-05构建、结算/26XP＋2锡片、合法输入及三状态冷恢复限定通过，A511.292/600、B726.724/900合规；本轮自动1过/2失败 | 原序列色差、首次onDestroy/自身SIG9未归因、英文重叠仍在；图标/完整默认参考链、系统Back/真机未验。本机/clean边界及旧预算超限历史保持，不写全包全测 | S限定证据已收；D未验 |
| PQA-15／Q1、Q6、Q8 返回/退出 | **旧Mac下拉Esc实际已过**；执行01 Back取消顺序/重绑、FIX01确认/Back/quit单例通过；emulator-04聚焦后点Back未见变化，未判通过 | 实际子页Back、根页取消/确认、新建档确认、战斗退出提示与Android系统Back仍缺；旧CUA无响应不当当前输入根因，不重列已过Back UnityTest | M页面；S/D系统Back |

“缺观察”不声称所有接线已经正确，也不预判有代码缺陷；若实际入口失败，由主程在同任务最小纠正。首次Android构建已收，后续候选发布由中央处理，QA不重复源码盘点。L1自然不可达的补线、换面、非致命临时路线、蓄力、全员倒下，以及无装备/无配方动作，继续沿既有业务自动证据；不为人工验收改变正式敌人HP、伤害或内容。历史通过只覆盖原输入/语义，不声称每个不可达表现都有当前uGUI画面证据。

**前期主程回执（当前构建状态以上方最新收件为准）：** actual `01a14307-7371-7d92-bd04-5c08dad5c90e`当时确认暂无已证L1缺实现；旧019 APK是9月29日UI Toolkit/minSDK22阶段，模拟器默认Vulkan曾崩溃，强制GLES3只到创建资料页，没有完整玩法或战斗/结算冷恢复。旧包/旧模拟器启动仍不能填掉PQA-14；iQOO未有连接/验收，不重开旧长验证链。

**前期只读接线收件：** 原作者actual `01a1430e-b2f7-75e0-bc67-e2437c3fcb11`当时核创建/恢复、Start Battle、战斗内选人、pointer只提交一次、历史回退、免费参考、胜利待结算/显式结算、结果四去向与Continuation均有接线；不等于运行通过。后续已补建档前可取消确认，当前以顶部新候选收件为准。实际步骤沿[试玩指南](../../demo/PLAY_FIRST_DEMO.md)及相应版本，不寻找独立“Check Entry Conditions”按钮，准备页选人不替代战斗内选人；退出本局先显示无奖励结束记录的语义说明保留。工具存在与旧只读盘点不替代实际设备就绪。

### 开跑前与最少实际观察路线

1. **当前前置：** 6babe331独立源码门复用，android-05已生成并取得限定实际观察；新三例1过/2失败、显示候选未应用/运行/发布。新A511.292055500/600、B726.723513250/900已收尾，不自动续用余额。旧A359.653499667/600、B上界1188/1200、内部超限及原B缺endedUtc与更早累计单列。本机/clean差异不冒称全树已测，QA不另派/追C。
2. **只补剩余主线缺口：** 安全新测试档的创建/空态、免费默认参考、错线/取消、回退/重来预览和四去向/结果按钮重打仍按PQA补实际观察；沿途核未到达页面双语、动态值与返回。复用已见合法行动、结算收据及三状态恢复，不为勾表重跑；新Lv1正常路线20XP＋2锡片与本次既有档26XP收据分别绑定输入。
3. **恢复/设备只补未覆盖边界：** android-05三段force-stop冷开及本次不重奖已接收，不再列待验；Home后台恢复、系统Back、真机多指/安全区与平台接线另按实际风险补。原序列色差、未归因退出和文字可读性由负责组在同任务处理；具体后续运行范围/资源合同确认后执行，不由本QA收件增额或自启。

**阶段边界：** 首Demo必须是正常L1、免费默认参考、真实连线/结算/保存、uGUI双语和安卓实际体验；占位美术可用。Google Play首版的广告及最小后端、R2/CDN/首次内容下载、发行合规/授权素材等另排，不阻塞本次L1功能走线；正式账号、云档和在线攻略分析更后置，不冒充首Demo或首版已完成。正式美术品质不作为当前功能Demo门。

## 历史收件：PR22 语言UI同任务审查（2026-10-16）

### 常驻提示修正：c3f0812

主测试actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a142ca-24a1-74f0-9ab9-1042f8a28156`。北京时间11:41:11实核head `c3f0812404a1223a4b5024cca7617aa054e4cc40`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`，draft/unmerged。[单次触发6207525475](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6207525475)对应服务于11:40:05 Completed；[原始结果6207548412](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6207548412)：“Codex Review: Didn't find any major issues. What shall we delve into next?”，`Reviewed commit: c3f0812404`。**ACCEPT（仅该head源码审查门）**；本轮无新发现/新review submission，issue comment不是formal APPROVED。三个旧讨论状态未变、均未resolved，不重复计作新发现，也未由主测试关闭。

仅HostView的unscaled两秒成功/changed提示隐藏、失败/Unknown保留与旧计时清理，及既有ButtonDropdownTests一条回归。**11:41审查收件时**沿派发/PR记录为`STAGED_UNCOMPILED_UNRUN`，当时未收到新head编译/回归/自动消失实测证据；后续编译和人工结果已更新于本文件顶部，不再将历史未验当当前状态。旧`c07d807`实际Play三项人工通过和English落盘只归旧输入。明细沿用[主程记录](engineering.md)；该轮只接服务，无Unity、本地review、重复触发或merge/release。

### 前版c07d807复审（当时状态）

主测试actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a14008-2aca-71c0-a6b2-4f81ea697c29`。北京时间22:50:45实核head `c07d80758696cfdda3dd0558d2b730963cf87ac0`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`，draft未合并。[单次触发6194895759](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6194895759)对应服务于22:49:53 Completed；[结果6194951891](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6194951891)原文“Codex Review: Didn't find any major issues. Chef's kiss.”，`Reviewed commit: c07d807586`。**ACCEPT（仅此head源码审查门）**，无本轮新发现/新review submission，不是formal APPROVED。

旧返回键P2 `PRRT_kwDOTDyjrM6sv_qI`仍`is_resolved=false / is_outdated=false`、无新增逐条回复。本轮请求明确覆盖旧P2的事件顺序、真实选项取消和后续Back；修正候选获独立no-major结果，但不称线程已关闭或实际行为已验证。两个历史P1仍过期未解决，未调用resolve。

差量仅4源＋Prefab五路径，等宽/左对齐/下接样式及Host消费展开列表Back、新回归；Prefab六对象十二字段（按主程冻结与发布说明）。静态补丁检查结果沿用[主程记录](engineering.md)，**本候选未编译、未运行新增回归、未查看修后画面**。Editor已由用户关闭且2/2启动额度用尽；新视觉、中英切换、偏好保存及冷启动待验，旧通过不能覆盖新候选。本轮只接服务结果，无Unity/本地review/重复触发/merge/release。

### 前版fe1ced8发现（保留）

短流程，主测试唯一收件，actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a13f55-c692-7fc3-af0d-ebb9bfc76801`。2026-10-15北京时间19:37:05实核[PR22](https://github.com/yyczz1/FightMatch/pull/22) head `fe1ced8cbfec868c2812aa01fdd12dbc253dd044`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`未变，draft未合并。[中央唯一触发](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6191042677)对应[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于19:36:12 Completed；[review5532469642](https://github.com/yyczz1/FightMatch/pull/22#pullrequestreview-5532469642)为COMMENTED，原文 `Reviewed commit: fe1ced8cbf`。本轮源码结论：**NEEDS_FIX**（1项新P2，不是formal APPROVED）。

**P2：[Consume Escape while the language dropdown is open](https://github.com/yyczz1/FightMatch/pull/22#discussion_r4282460773)**，`Assets/Scripts/FightMatch/Host/FightMatchLocaleDropdown.cs:8`，线程 `PRRT_kwDOTDyjrM6sv_qI` 未解决/未过期。服务指出：展开的下拉框通过`TMP_Dropdown.OnCancel`处理Escape而隐藏，Host.Update却对同一按键调用`Session.Back()`，造成收起列表同时导航或触发退出/战斗退出。关键原文：“Override or otherwise coordinate cancel handling so an expanded dropdown consumes the key before session navigation.” 已直接交主程在原UI任务内追加最小修正和必要定向验证：展开时一次Back只收起，关闭后Back原语义及保存/解绑保护保持，不新增GUI启动额度。两个历史P1仍outdated但未resolved；本轮未关闭线程。

输入为本轮28路径差量（14产品/10文档/4定向证据），不含本机旧Build/测试WIP。按派发与PR正文复用15项中的12通过及旧控件定位修正后1/1；两项headless GameView失败保留，不能写15/15或整个clean PR树全测通过。[定向证据](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/ui-button-dropdown-01/)与主程/C实际GUI分开；本轮两次启动额度已用尽，冷启动复开仍缺，不以Stop/Play替代。主测试只接GitHub结果，不运行Unity、不触发重复审查、不merge/release。

## 历史：PR22 三平台绑定第三轮审查（23f0082，2026-10-13）

主测试唯一收件，actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a134c0-ead6-74a0-81c9-b0c77a391890`。北京时间18:19:32实核[PR22](https://github.com/yyczz1/FightMatch/pull/22) head `23f00824db710f0c35299e7006f824384a267f02`、base `dc1bba0537779983647f5e261f8e1c33a1210a49`未变，draft未合并；源码/资源在父提交`ef4c33f`，末提交为文档记录（中央发布范围说明，主测试未重做源码审查）。[第三轮单次触发](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6143290130)对应[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于18:17:56 Completed；[结果6143365353](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6143365353)原文为“Codex Review: Didn't find any major issues. You're on a roll.”，`Reviewed commit: 23f00824db`。本轮结论：**ACCEPT（仅此head源码审查门）**；未观察到第三轮新发现/新review submission，服务答复是issue comment，不是formal APPROVED，也不保证所有级别问题不存在。

**两个旧P1：** [缺失绑定](https://github.com/yyczz1/FightMatch/pull/22#discussion_r4256310196)（`PRRT_kwDOTDyjrM6rxQwi`）和[固定Mac身份](https://github.com/yyczz1/FightMatch/pull/22#discussion_r4256607300)（`PRRT_kwDOTDyjrM6rx-vm`）最终均为`is_outdated=true / is_resolved=false`，没有新增逐条回复。第三轮请求明确要求核这两项，当前修正候选获得独立无重大问题结论；但不能称GitHub线程已关闭或机器人逐条宣布解决。线程状态及前两轮NEEDS_FIX历史保留，主测试未调用resolve/评论接口。

证据按主程已接收范围复用：原103项、Bridge5＋HCP8及Fake3/3通过；平台14项为13通过/0失败/1 Windows原生Ignored，跳过不算通过；Android/Windows真实raw构建exit0，Mac历史构建复用，三平台装配后import通过。对应[平台证据](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/platform-verification.json)及[运行记录](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)，不升级为整个PR树全测或APK/设备通过。

GUI阶段B两次真实auto-review拒绝，新Editor/窗口未启动、新隔离档空、NOT_READY；既有Mac显示观察不冒充新候选GUI验证，Scene保存不等于玩家档或语言偏好保存。GUI授权由中央一次集中向用户说明，主测试不重复询问/运行/换入口；普通源码审查继续，总体Demo仍NEEDS_FIX。本轮只写本文件，不触发重复审查、不跑Unity或本地review。

## 历史：PR22 资源与场景绑定复审（8754eaf，2026-10-13）

主测试唯一收件，actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a13474-412a-7a72-9af5-0dd7a6de5e73`。北京时间16:53:51实核[PR22](https://github.com/yyczz1/FightMatch/pull/22)新head `8754eaf0362ad26e6a8bc905c013580e08930a64`未变，base仍为 `dc1bba0537779983647f5e261f8e1c33a1210a49`，累计84文件、draft未合并。中央[单次触发复审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141868476)后，[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于16:52:56变为Completed；[review 5501074947](https://github.com/yyczz1/FightMatch/pull/22#pullrequestreview-5501074947)为COMMENTED，原文 `Reviewed commit: 8754eaf036`。本轮源码结论：**NEEDS_FIX**（1项新P1）；未重复触发、未做本地代码审查或Unity验证。

**新P1：[Bind an Android resource set for Android builds](https://github.com/yyczz1/FightMatch/pull/22#discussion_r4256607300)**，`Assets/Scenes/FightMatchDemo.unity:160–161`，线程 `PRRT_kwDOTDyjrM6rx-vm` 未解决、未过期。服务发现正文原文：

> When this scene is used in an Android Player, the new binding still prevents startup: the fresh head replaces the previously missing values with the macOS-only set `fm.builtin.r1.macos` and platform `macOS`, while `YooAssetRuntimeFactory.Create` derives `android` from `Application.platform` and returns `RES_SCHEMA` on the mismatch before a `Session` can be created. A repository-wide search finds no Android build hook that rewrites these fields and no staged `fm.builtin.r1.android` boot/resources; `BuildAndroid()` only emits candidates under `TestArtifacts`. Bind/stage the platform-specific Android resource identity as part of the Player build instead of serializing the Mac identity into the shared Demo scene.

**旧P1闭合状态：** `PRRT_kwDOTDyjrM6rxQwi`仍未解决、未过期，服务未给整体闭合结论。新正文承认已用macOS值替换缺失字段，C也有macOS正常Session证据；但Android启动仍被新P1阻断，不能将Mac子路径补齐写成共享Demo启动全部修复。主测试没有自行关闭任一GitHub线程。

新P1已直接送主程处理平台正确的Player构建装配与资源身份，要求保留Mac启动及平台/完整性校验、组内锁定最小文件和验证；此事与CUA按钮无响应分别跟进。修正后由中央发布新head，再接同PR独立审查。

**运行证据边界：** 已读取C turn `01a13459-ab52-78d1-9976-5550aa35da81`的[BuildMac记录](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/build-macos-execution.json)（31.687276秒／exit0／依赖未变）与[GUI记录](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/gui-result.json)。C记录正常Host已建立Session、可见简中、Scene保存关闭再只读打开后五字段保留；不是主测试亲自操作。创建资料/语言按钮CUA点击无可见响应，原因未定，玩家档与语言偏好均未生成，Play重进/冷启动未执行。场景字段持久化不等于玩家存档；交互、语言切换、存档复开、完整玩法与设备不判通过。主程正独立定位交互，不等复审。Bridge5＋HCP8已有通过证据，Fake3修后未验，按主程收件复用；下方清单未整项通过，不因部分观察打勾。

## 历史：PR22 首次源码审查（c4e0ebc，2026-10-13）

主测试唯一收件，actual `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local/01a13455-5e6c-7313-8c59-3ab37339f5bc`。[PR22](https://github.com/yyczz1/FightMatch/pull/22) 于北京时间16:21:33实核 base `dc1bba0537779983647f5e261f8e1c33a1210a49`、head `c4e0ebc09a7bd7d06f51e95d87c2647c759967af`未变，44文件、draft未合并。中央[单次触发](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141312669)的[服务状态](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6141318771)于16:20:43变为 Completed；[review 5500712716](https://github.com/yyczz1/FightMatch/pull/22#pullrequestreview-5500712716)为 `COMMENTED`，原文标明 `Reviewed commit: c4e0ebc09a`。唯一源码接收结论：**NEEDS_FIX**（GitHub返回1项P1，不是formal APPROVED；测试/场景/设备另计）。本轮未重复触发、未做本地代码审查或Unity验证。

**P1：[Bind the resource bootstrap fields before startup](https://github.com/yyczz1/FightMatch/pull/22#discussion_r4256310196)**，`Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs:167–168`，线程 `PRRT_kwDOTDyjrM6rxQwi` 未解决、未过期。服务发现正文原文：

> In the normal `FightMatchDemo` scene, the `FightMatchPlayerHost` component still serializes only `fontAsset`, `fontLicense`, and `runtimeRoot`; repository-wide asset search finds no values for the newly added boot, SHA, set, or platform fields. Consequently `resourceBoot` is null and the remaining strings are empty here, so `YooAssetRuntimeFactory.Create` returns `RES_SCHEMA` and startup can never create a `Session`. The build utility explicitly does not update the scene, so these fields must be populated by the committed runtime composition rather than relying on the focused reflection tests.

已成功直接交主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 处理此单项纠正：限定正常Host启动装配及必要绑定，组内锁定文件；保留资源完整性、取消/释放和玩法/存档语义，以正常场景实际启动补证，不只用反射fixture。新源码由中央发布准确head后接GitHub复审，不追加本地review或旧链验证。

已有验证按本次派发及16:16收取的PR正文保留：编译通过，首次111项103通过/8失败。[主程运行记录](../../../TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)接续补验；主程随后回报03为13/16通过（Bridge5＋HCP8），余3项Fake关闭测试的状态修正尚未复验，不记全通过。主程已确认本P1由C当前turn `01a13459-ab52-78d1-9976-5550aa35da81` 处理真实BuildMac产物、boot文件及Demo Host五字段绑定，随后隔离档正常Start与人工操作；这是修正安排，不是修复完成或线程解决。主测试本轮未核收这些新运行结果、未代跑。构建、真实场景与设备仍须各自实际证据，继续复用下方8项清单。本机历史差异不覆盖为整个PR树全测通过。中央仅收审查完成和正常启动受阻的摘要。

## 原8项人工接收清单（2026-10-13建立，2026-10-18按DEMO-NEXT新证据更新）

按用户简化方案建立；建立时权源为中央 turn `01a13404-21d1-7262-abae-9d52295727c2`／人类 item `01a13404-223c-7893-b069-dae699e44e19`。主测试建立时 turn `01a13411-c2e4-7a90-8c7f-a705a36f2932`，只写本清单。当时主程组织接入、旧C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local` 串行操作；后续执行者/权源以各轮收件为准，本轮为顶部新短C，不向旧C续派。

**当前版本与证据分层：** c3f0812的English真冷开/原档连续性/短提示、c07d807下拉/可见双语按钮观察及Back/提示2过＋修正后建档确认1过保留；1d75386/8项/android-02、902006fa/6项/android-03与15378d05/7项/android-04各属其输入。6babe331对应本机后验1过/2失败、android-05构建及结算/奖励/三状态恢复限定通过；原序列色差、首次未归因退出和英文重叠未闭合，不判整Demo通过。原RES_STORAGE/UnauthorizedAccessException与旧CUA无响应保留历史，后者不当当前全局输入结论。剩余只维护在顶部PQA表；未整项通过保留未勾选。264条导出/重复键拒绝及旧自动化不证明全部新画面；后续只用安全测试档/副本，不覆盖真实档，不另造框架。

1. [ ] **正常启动与交互（部分通过）：** android-05实际启动、合法输入及可见结算入口已收；首次onDestroy/自身SIG9未归因，原关菜单后首次棋盘按压色差保留。图标/创建确认/返回退出及完整链未验，旧失败不抹除，见PQA-01、14、15。
2. [ ] **文本绑定（部分通过）：** 旧双语按钮排版、H06/H07及安全code保持；android-05中英待结算按钮完整可见，但英文敌人名/状态重叠裁切。其余页面/参数/缺字仍未全验，见PQA-04、12、13，不直接归因翻译表。
3. [ ] **中英切换（部分通过）：** 旧Mac双向切换/短提示保持；android-05实际中英切换及冷开简中偏好保持已收。原简中关菜单后的首次棋盘按压仍牵动外观，中性焦点对照不替代该分支；全页面重建仍见PQA-12。
4. [x] **偏好保存与复开（Mac已过）：** c3f0812第二次真实Editor进程冷开后Play直接English，未重选、原档正常；不是只Stop/Play。Android仍由Q8接收。
5. [ ] **首次默认语言：** 解析自动证据复用；两种系统语言的无偏好隔离首开尚缺，见PQA-11，不清原档。
6. [ ] **正常地图/战斗入口（部分通过）：** android-05已有显式基础结算收据26XP＋2锡片，从地图再入L1后的合法线恰新增一对snapshot/commit；默认参考/回退/四去向和结果按钮重打仍缺，见PQA-02～08、15，不把局部收据当完整新档链。
7. [ ] **存档不回退（部分通过）：** Mac/旧既有活动局证据保持；android-05安装前后10业务SHA同，待结算/已结算/首行动后三次冷开分别10/12/16业务SHA同、相应状态保持，26XP与锡片2本次不重发。仅此档/包/模拟器，不外推所有userdata/中断/重打，剩余见PQA-03、09、10；新Lv1路线20XP与本次26XP实际收据分别绑定。
8. [ ] **Android实际体验（部分通过）：** android-05 API33/ARM64模拟器上标准touchscreen输入、结算/奖励及三状态恢复限定通过；原色差、首次未归因退出/英文重叠、完整默认参考链、系统Back及真机仍未闭合。新A511.292/600、B726.724/900，旧预算/超限历史单列，见PQA-14、15；manifest API24不等于API24设备实测。

本次QA只收原始后验证据，未运行测试/Unity或重复触发。6babe331源码门沿原结果复用，15378d05/902006fa/1d75386/c78f8a5/c3f0812门仍各属旧输入；源码门、分批自动结果及限定恢复不替代其余玩法/设备缺口。不以旧M17/FIX11或“五条YAML记录必须消失”阻断功能观察；真实失败和未观察项分别保留。以下均为历史记录。

2026-10-03 最新限定收件：
[HOST-RESOURCE-MAC-003 机器回执](testing-host-resource-mac-003-receipt.json)为 `ACCEPT`，
仅接收 PR3 基底＋PR5 两源码＋PR6 四源码的组合候选：M03 编译补证、10 自然 meta
补全及一次精确 17/17（6 HISO＋11 纯契约）。M02 Bee 失败、M03 原 guard 失败保留；
旧 ADB 临时日志新增 36B 单列，不声称旧 tmp 全冻结。既有 GitHub no-major 证据复用，
本轮无代码复审／Unity／测试启动。LOC-B-PREF 测试、真实 Host/G3、Scene/Player/Android
及 Demo 门未关闭。下方既有段落保留各自历史时点，不覆盖本次限定回执。

2026-09-30 · 状态：**验证计划已建立；设计审查与环境核对进行中；产品候选尚未冻结**。

本文件由主测试会话
`01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / `local` 唯一维护。用户在中央
会话明确恢复 FightMatch 后续开发，并批准按五角色组织持续分发、接回、
修正和推进；授权出处为根目录 `AGENTS.md` 第 6 节、
`.agent/TEAM_WORKFLOW.md`、本目录 `README.md` 和 `dispatch-001.md`。
本轮固定起点为 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`。主测试不写产品
代码、不运行 Unity、不操作 Git；本机 Unity 仍仅由 C 串行执行，Git 仅由
中央协调。

## 1. 目标、责任与判定边界

- 目标：为首 Demo 的运行时 uGUI 迁移、英／简中本地化、字体与 Android
  实际入口建立差异验证；复用仍适用的旧证据，固定候选后用最小有效运行
  补足新变化，并接收专业设计结论与正确 PR／head 上的 GitHub PR Code Review。
- 用户最新规则：代码审查交 GitHub PR Code Review，dots继续承担自动化测试；即刻停止新增／重复
  的 Codex R 代码审查分发。已有 R 结论保留历史效力，本轮已完成的纯设计审查保留但不追加 R
  回合。主测试负责验收场景、预算、环境／设备安排、结果核验、缺陷分类、GitHub审查问题归并／
  修复跟踪和证据索引。作者自检、自动化测试通过、设备通过、专业设计接收与 GitHub PR审查结果
  是互相独立的事实。
- 只有实际证据能判通过。设备未连接、画面未见证、语言未遍历或结果未绑定
  固定候选时，一律记为 `未验证`，不得由旧 Toolkit APK、静态扫描、作者
  报告或模拟器结果替代。
- 产品缺陷回主程签最小纠正包；规则／文案问题回主策；资源／排版问题回
  主美；许可、组件、连接或工具链问题记为环境故障并协调主程。环境失败不
  记成产品失败，反之亦然。

## 2. 角色、会话与回传

| 角色／任务 | 实际 thread / host | 状态与边界 | 回传 |
| --- | --- | --- | --- |
| 中央 AI | `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` | 活跃；全局收件、Git 唯一协调 | 接 QA 阶段摘要、风险、下一步 |
| 主测试 QA-001 | `01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / `local` | 活跃；本文件唯一写入者 | 向中央及对应负责人回执 |
| 主程 ENG-001 | `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local` | 活跃；FIX17 `BLOCKED_CORE190_TIMEOUT` 后组织只读停点归因 | 未定位 test／domain 前不得原样重跑 Core190 或宣称222覆盖 |
| 程序实施 C | `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local` | `gpt-6-astra` / `max`；FIX17 completed；唯一串行本机 Unity 执行者 | idle；Host30保留，Core190 timeout；reopen未运行 |
| LOC materials owner | `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local` | M11 `BLOCKED_LICENSE_EXPRESSION` 后 idle；material absent | 等权威许可证／源码身份 short delta；不猜许可证、不恢复旧包或送 R |
| 独立 R | `01a0e404-e8ee-7310-8388-9260babd53f1` / `local` | 历史 lock-set design `ACCEPT` 保留；当前 idle | 不新增 Codex R 代码审查；后续代码审查改接 GitHub PR结果 |
| 补充独立 R-TOOLS | `01a0f3e3-9663-7be2-84a8-ed23be32f0b9` / `local` | LAYOUT FIX-002 design-only `ACCEPT` 与 FIX15 `PRE_REVIEW_PASS` 保留；idle | 不追加 R 回合；PRE_REVIEW_PASS不升级整体接受 |
| 独立 R-ANDROID | `01a0f40f-6bc6-72d2-b3dc-eb954ad0716a` / `local` | ANDROID-GFX FIX-01A design `ACCEPT` 保留；idle | 不新增 Codex R 代码审查；实际 Android 测试门不变 |
| 云环境准备 | `01a0f13c-3d87-753a-ad11-16302add9084` / `durable` | `QA-CLOUD-ENV-001` 已完成；Android 环境未就绪 | 回合与结论见 §4 |

R 最近准确回合为 `01a0f257-7778-7bf2-98a7-25e3489217dd`，于
1790772489 completed，final
`msg_0d29bf8d726d8dc0016abd04fd77b487d0a156507ba9784b0e`，对 D17 给唯一
`ACCEPT`。该事实只接收性能专项，不是本轮 uGUI 设计或代码审查。后续 R
不运行第二套 Unity；主程把完整、冻结的设计／代码审查申请交主测试后，
由主测试单线派发，避免并发覆盖同一 R。

`QA-R-READY-001` 已由同一 R 在回合
`01a0f2ef-b073-7b81-9f36-bb72bd46ec39` completed，final
`msg_0d29bf8d726d8dc0016abd2ba03ab087d0afc89a1170a0d675`，状态为 `READY`
而非设计 verdict。R 已核当前 HEAD、九份团队／迁移入口并确认可接
`QA-R-DESIGN-001` 和 `QA-R-CODE-001`；要求完整冻结候选、SHA、输入版本、
精确白名单／受保护变更、行为验证映射、预算、回滚及实际 patch／证据。
设备或视觉缺口须保持未验证。本回合未运行 Unity、改文件或写 Git，R 已停写。

中央于 2026-10-01 授权建立一个补充独立审查通道，减少 UGUI 与 LOC／工具
候选共用原 R 的排队瓶颈。`R-TOOLS` 实际 thread 为
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9` / `local`，由主测试
`01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / `local` 唯一派工和收件；注册回合
`01a0f3e3-9764-7301-9711-4be33f3da87c` completed，final
`msg_0543862903425801016abd69b460bc87d0990bb137aa982e46`，状态为 `READY`，
不构成任何候选 verdict。它已读完整协作、审查与验证入口，当前未获候选，
没有运行 Unity、Luban、构建、测试或 preflight，也未改文件、Git、资产、设置
或云资源。后续只可一次接一个明确固定的 LOC／工具链／版本／依赖／包采用与
证据候选；每个 `NEEDS_FIX` 修订必须返回首次审查该包的同一 R，不能换审查者。
当前 `QA-R-LOC-TOOL-01-DESIGN-R3` 仍由原 R 独占，R-TOOLS 不重复审查。
中央随后明确把尚未由任何 R 开始的 `QA-R-UGUI-COPY-BIND-01` 改派给空闲的
R-TOOLS，并将本次权限扩为该完整接线合同、两处受保护 API delta 与验证映射；
这不是把 `NEEDS_FIX` 修订换人复审。主测试已核候选
`engineering-ugui-copy-binding-001.md` 为 244 lines／41329 bytes／SHA-256
`f0c5d246c0af3570c6ad451d99313540b1f2e0068160ad83db5e8b02759e4e67`，
并于实际回合 `01a0f3e8-e601-76b3-9080-dbe7675fa9bd` 派发；该回合当前
in progress，最终回执未到。其冻结输入为 architecture
`309762cb3c4062aab9beadbccb5e34fbc9225d53fb9339f85021e46668513325`、system
`4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e`、
implementation `a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a`、
planning `8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`
与 CSV `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`；
完整哈希已随派工发送。该候选及其
后续 `NEEDS_FIX` 修订由 R-TOOLS 唯一审查，原 R 不接也不重复。两个 R 都不
运行第二套 Unity；正式回执都必须绑定实际 thread／host／turn、输入身份、逐项
证据和未验证门，并只给一个 `ACCEPT`／`NEEDS_FIX`／`REJECT`。

`QA-R-LOC-TOOL-01-DESIGN-R3` 已由原 R 在回合
`01a0f3e2-a494-7761-841c-43fa0da0b2cc` completed，final
`msg_0d29bf8d726d8dc0016abd6bf24d8487d0a2c5926eb81df19f`，唯一 verdict 为
**NEEDS_FIX**。候选 `engineering-localization-luban-001.md` 在结束前复核仍为
582 lines／36745 bytes／SHA-256
`22f1466a86c1e256a465b8d91b7fd776f1a402f75ae1166854411ad3ad1543f5`，
无身份漂移或尾随空白。四项 P2 必须由同一设计 owner 做最小修订后，作为
`LOC-TOOL-01-DESIGN-R3-FIX-01` 回同一 R 限定复审：

1. §5.2／§6.2／§9.4 同时容许跨平台 raw 字节不同、要求 manifest 含 raw SHA
   且 byte-identical，合同互相矛盾。实际 raw SHA 保留在每次生成 evidence；
   发布 manifest 只承载跨主机稳定的 source／canonical／semantic 身份，并
   同步确定性门，不得放宽实际文本、参数或严重度差异。
2. §7.2／§10 在原子替换成功后复读失败时仍一概保证旧文件原字节，不可实现。
   须冻结提交点、提交前／后验证失败、回滚失败的 store 结果和重试语义；保持
   session 新语言、只重试 preference、无 PlayerSave／OperationId 的边界。
3. 设计要求完整第三方许可证归档，却未把依赖→版本／DLL hash→许可证来源和
   证据 hash 清单、逐文件证据白名单、缺失／不明／漂移即 `BLOCKED` 写成 A
   接收门。132 项 runner hash 不能代替 41 项依赖许可证证据；本修订不授权
   下载、重建或重跑 005。
4. 统一 CLI 示例仍含 PowerShell 反引号，且工具 hash 未指向实际冻结 evidence。
   须改为无 shell 语法的 argv／单行合同，绑定外盘 dotnet，并引用
   `005/tool-identity.json` 的 `runner.criticalFiles["Luban.dll"]` 或冻结 manifest。

其余边界经 R 复核保持：COPY planning／CSV／prefix 身份与 263 unique、严重度
172/74/13/4、参数化 77 一致；005 run SHA
`ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232`
的 161 项 evidence 和缓存 132 项 runner identity 无 mismatch。B 白名单、现有
internal seam、解析预算冻结、无 Host 修改／生产加载、RES-01D 后置，以及
Windows／Unity／Android／设备未验证边界均未被否定。本回合未写文件，未运行
Unity／Luban／preflight／build，未下载／安装、操作 Git 或访问云凭据；不授权
A／B 实施。

主程已对 `LOC-TOOL-01-DESIGN r3 / LOC-R3-FIX-01` 给出内部 `ACCEPT`，但该
结论不替代独立 R，也不授权 A／B。主测试已只读复核修订候选
`engineering-localization-luban-001.md` 为 689 lines／46711 bytes／SHA-256
`f4455bb214627a08478cdf690e751601a45a388a84966f6d7d0c211f8aa4b6ed`，
状态 `READY_FOR_INDEPENDENT_REVIEW`，并核最终 COPY live planning／CSV 身份
仍分别为 `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`
和 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`；
设计文档 diff-check 通过。

FIX-01 已由首次审查者原 R 在实际回合
`01a0f3fb-a0fb-76a0-9a06-4719d8da83dc` completed，final
`msg_0d29bf8d726d8dc0016abd70ecf84887d0af99b775be765c1b`，唯一 verdict 为
**NEEDS_FIX**。F3 已关闭为设计门：§6.3／§9.1 绑定 005 工具身份、132-file
manifest、deps、packages lock 与实际 license／notice／nupkg entry，要求逐依赖
inventory 和缺失／身份不符的 `LICENSE_INVENTORY_INCOMPLETE` 阻断；这不证明
实际许可证已齐。

F1、F2、F4 仍须最小 `LOC-R3-FIX-02`：

1. **F1**：候选把已接收 live COPY draft SHA `ea05eae…1eedf` 标成加入控制列／
   三条控制行后的 formal CSV SHA，而正式字节必然不同；须分成
   `copySourceSha` 与由 A 生成后冻结的 `formalSourceSha`，同步 manifest 引用。
   同时修订额外放宽了原 file-set 门，容许文件名／拆分变化，却没有多文件
   normalizer／合并合同；须恢复 raw 相对路径和文件集合一致，只容许已列
   LF／CRLF、property 顺序等表示差异，raw 事实继续留 per-run evidence。
2. **F2**：提交前 temp／flush／pre-verify 失败且无法复读旧状态时，候选转
   `save_unknown`；但固定 AMEND-04 只允许该键用于提交点之后或回滚无法证明
   恢复。须把该分支收敛到现有 COPY 边界；若要扩大触发范围，必须由主策另走
   COPY 修订和独立接收，不能在当前输入下静默扩大。post-replace UNKNOWN、
   verified rollback 才降 `save_failed`、session 新 locale 和幂等重试其余部分
   已匹配。
3. **F4**：JSON argv 第 0 项是 executable，必须明确赋给 `FileName`，仅余项
   进入 `ProcessStartInfo.ArgumentList`。工具 SHA／commit 可来自
   `005/tool-identity.json`，但 SDK version 不在该文件，须精确引用
   `005/input-hashes.json.sdkArchive.version`；统一所有示例，避免重新推断。

R 复核 A 的 8 个生产路径、B 的 12 个新文件／meta 路径、预算、264 records、
ownership、fail-closed、无 Host／生产加载与 RES-01D 后置均无回归；Windows／
Unity／Android／device／CDN／发行门保持未验证。本轮只读，无文件／Git 写入，
未运行 Unity／Luban／preflight／构建／产品测试、下载、安装或联网补洞；A／B
继续禁止，FIX-02 只回同一原 R。

主程随后对 `LOC-R3-FIX-02` 给出内部 `ACCEPT`，但仍不替代独立 R。主测试已
只读复核候选为 743 lines／50126 bytes／SHA-256
`43f98e353b20eb583df8154fe604dd73f32b281378aefade22382916975fe1fc`，
设计文档 diff-check 通过。FIX-02 已由同一原 R 在实际回合
`01a0f408-5374-7660-80d8-3fa332c632aa` completed，final
`msg_0d29bf8d726d8dc0016abd73567b7c87d0824de3bb6f79ebff`，唯一 verdict 为
**ACCEPT**。

本轮只复核 F1／F2／F4：`acceptedCopySha256` 与 A 实测
`formalSourceSha256` 分离，raw 相对路径／file-set 跨主机保持、仅同路径表示
bytes 可不同；所有 replace 前失败恒为 `save_failed`，`save_unknown` 只用于
replace 后验证失败或 rollback 不能证明恢复；三处结构化调用严格区分
`fileName=argv[0]` 和 `argumentList=argv[1..]`，SDK version 来自 005
`input-hashes.json.sdkArchive.version`。已关闭 F3、A／B 白名单、预算、后置
接线和未验证门只核无回归。R 确认 F1 已关闭：COPY／formal source SHA 分列，
raw file-set／相对路径门恢复，publish manifest 排除平台相关 identity。F2 已
关闭：replace 前恒为 `save_failed`，replace 后验证或无法证明回滚才为
`save_unknown`，session／retry／业务隔离与固定 COPY 一致。F4 已关闭：三处
调用正确拆 executable／arguments，SDK／commit／tool hash 都绑定固定 005
证据。F3 保持关闭，实际许可证完整性仍待 A 验证；A／B 白名单、解析预算、
后置接线无回归。

该 `ACCEPT` 只接收精确设计。主程仍须另签 A 包，A 独立接收后才可签 B；
正式 264 生成、许可证实际完整性、Unity compile／EditMode、runtime 接线、
RES／Windows／Android／device／CDN／发行均未关闭。本轮只读，无文件／Git
写入，未运行 Unity／Luban／preflight／构建／产品测试、下载、安装或联网补洞。

`QA-R-UGUI-COPY-BIND-01` 已由 R-TOOLS 在回合
`01a0f3e8-e601-76b3-9080-dbe7675fa9bd` completed，final
`msg_0543862903425801016abd6c44ee2087d0b0196d478dcb4571`，唯一 verdict 为
**NEEDS_FIX**。候选 `engineering-ugui-copy-binding-001.md` 结束前复核仍为
244 lines／41329 bytes／SHA-256
`f0c5d246c0af3570c6ad451d99313540b1f2e0068160ad83db5e8b02759e4e67`；
五份固定输入及 CSV 217 行旧 prefix 身份均匹配。三项必须在
`QA-R-UGUI-COPY-BIND-01-FIX-01` 最小修订中闭合，再回同一 R-TOOLS：

1. **P1**：§7 只允许 8 个测试文件，未包含共享文本输入源
   `LocalizationContractTests.cs`；该源仍严格锁 216 键／50990 bytes／旧
   prefix，当前 263 键／64708 bytes CSV 会在 fixture 装配时被拒绝，Q4 因此
   不能通过。主程须补签该文件的输入身份更新权限，统一用完整 263 键并保留
   严格 hash／shape／参数检查与原测试数量，不能截旧 prefix 或放宽检查。
2. **P2**：§4.1 的 `FocusLost` 真实 Host 链未闭合。既有 Host handler 先经
   `PausePresentation` 调无原因 `board.CancelPointer()` 清捕获，之后棋盘回调
   可能丢失活动手势；同一无原因入口还服务其它取消来源，不能统一猜成失焦，
   Host 两文件也不在白名单。须冻结完整原因传递、兼容无参调用和去重顺序，
   必要 Host internal 文件逐一补签，并以真实 handler 测试回调先后不改分类；
   不扩 public API 或改变原取消／播放收口语义。
3. **P2**：一次性消费尚未和玩家可见保留分开。当前合同规定当次 Render 消费、
   后续不重放，却未定义已消费显示行的保留／替换／清除；100ms 周期刷新和播放
   收口的连续 Cancel／Refresh 可让反馈立即消失。须冻结显示生命周期，并在
   既有 WIRE01／WIRE02 单方法覆盖连续 `Changed`、周期刷新、语言变化和解绑，
   不增加队列、业务写或持久化。

R-TOOLS 另确认 15 核心／9 产品处置齐全，N-D01 三个只读事实和唯一构造来源
成立，B-WIRE-01 八值及 accepted 条件清楚；现有白名单确为 13 source＋8 test、
15 个新增方法名唯一、静态 207＋15＝222，Q2 仍保留主包首次合法资源生成／
保存／复开边界，且未发现第三组 public API、保存字节、业务路由、依赖或配置
扩张。以上只是静态审查；本回合未修改文件／Git，未运行 Unity／Luban／构建／
测试／preflight，也未下载或安装。实现、编译、222 测试、资源复开、设备和
视觉门均未验证。

主程已对 `QA-R-UGUI-COPY-BIND-01-FIX-01` 给出内部 `ACCEPT`，但该结论不
替代独立 R，也不授权交 C。主测试已只读复核修订候选
`engineering-ugui-copy-binding-001.md` 为 260 lines／51249 bytes／SHA-256
`90f28e5c7048524673801b42b124d9221d1bb0c0662b7eff316a0ae9510453d9`，
并确认冻结 snapshot／receipt 仍分别为
`d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`
（264 physical lines／64708 bytes／263 data）和
`980e8a942c06b1b6ebcec32d428135a656d5bd0ae089b087d82e3c4d5d3564b8`
（26 lines／1633 bytes），权限均为 `-r--r--r--`；设计文档 diff-check 通过。

FIX-01 已由首次审查者 R-TOOLS 在实际回合
`01a0f3fa-9bbe-75c3-a445-f863446af59b` completed，final
`msg_0543862903425801016abd70b3ac8087d08fe58ae537a83642`，唯一 verdict 为
**NEEDS_FIX**。P1 已闭合：`LocalizationContractTests.cs` 纳入白名单，完整
64708 bytes 先核 length＋SHA 后解码，禁止截断和 live fallback，原检查与方法数
保留。P3 已闭合：Input 一次性 drain 与 Controller 单槽保留分开，刷新、播放
收口和语言切换保留同一语义，替换／解绑／关闭清除，reference／action 共槽。

P2 仍未闭合：候选第 129 行要求 `FightMatchPlayerHost.OnDisable` 保持无参
`Cancelled`，第 130 行却要求棋盘／子视图 `OnDisable` 使用 `FocusLost`；实际
`FightMatchPlayerHost.OnDisable` 会先调无参 `PausePresentation()` 再解绑，
`FightMatchHostView.OnDisable` 和 `PlayerBattleView.OnDisable` 也直接进入
`Unbind()`，最终可先以无参取消清掉活动手势。若上层先执行，后续棋盘回调已
无法发布要求的唯一 `FocusLost`，分类仍依赖 Unity 回调顺序。

最小 `QA-R-UGUI-COPY-BIND-01-FIX-02` 只允许修改同一设计文档，补齐所有上层
`OnDisable → Unbind/Close` 的原因传递；显式关闭、普通解绑、几何失效、
pointer-exit 与低内存继续原分类。父级先、子级先、直接禁用 Host 的断言合并
进既有测试方法，保持零 submit、播放收口顺序和 222 目标。其它边界未回归：
15 source＋9 test、两组 public delta、15 个新增测试名、207＋15＝222、Q2 首次
资源生成／复开、冻结输入和 263-key snapshot 均保持。本轮只读，未写文件／
Git，未运行 Unity／测试／Luban／preflight／构建、下载或安装；不得交 C，后续
FIX-02 只回同一 R-TOOLS。

主程随后对 `QA-R-UGUI-COPY-BIND-01-FIX-02` 给出内部 `ACCEPT`，但仍不替代
独立 R。主测试已只读复核候选为 264 lines／54986 bytes／SHA-256
`a26b50a40828d4598423ad49a694b4361eca33391dbcfcabe1f7ffceb82528ac`，
snapshot／receipt 身份保持，设计文档 diff-check 通过。FIX-02 已由同一
R-TOOLS 在实际回合 `01a0f406-3c86-71a3-bd46-291a87ead2e4` completed，final
`msg_0543862903425801016abd7301b80c87d08844b314c81630bf`，唯一 verdict 为
**ACCEPT**。

本轮只复核 P2：所有 public／无参 teardown 默认 `Cancelled`；所有列明 Unity
disable／focus-loss／pause-loss 从最高层以 `FocusLost` 进入
HostView→PlayerBattleView→PlaybackView→InputView→BoardElement→Controller
的完整 internal cause 链，中途不得回落无参；父级先、子级先、直接禁用
HostView／PlayerBattleView 均须唯一 `FocusLost`、零 `Cancelled`／duplicate／
submit，而普通 Close／Unbind／low-memory 仍唯一 `Cancelled`。P1／P3、15＋9
白名单、两组 public delta、263-key snapshot、207＋15＝222、Q2 与未验证门
只核无回归。R 确认 P2 冲突已经消除：兼容入口默认 `Cancelled`，新增 overload
均 internal，完整 cause 链逐层闭合；所有 disable／focus／pause 路径以
`FocusLost` 首次发布，普通关闭／解绑／低内存仍为 `Cancelled`，session／播放／
token／override／detach／零 submit 顺序保持。四种 disable 顺序和普通取消对照
均合并既有两个测试方法。P1／P3、15＋9、两组 public delta、222、Q2 与未验证
门无回归。

实施审查须特别核：non-borrowed `Close(cause)` 必须在
`Controller.Dispose()` 内部默认取消前先完成带 cause 的活动手势取消；既有
`Dispose→Finish→CancelGesture` 路径存在，仅在末尾替换
`inputView.Close(cause)` 不满足合同。该要求可在既定白名单与 internal seam 内
实现。本 `ACCEPT` 只关闭设计门，不直接授权 C；实现、编译、222 测试、资源
复开、设备和视觉仍为 `NOT_RUN`／`NOT_VERIFIED`。本轮只读，未写文件／Git，
未运行 Unity／测试／Luban／preflight／构建、下载或安装。

主策随后提交 `COPY-001-AMEND-04-LANGUAGE-PREFERENCE-UNKNOWN`，主程要求 QA
组织独立审查。主测试已只读确认 live `planning.md` 为 372 lines／47305 bytes／
SHA-256 `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`，
live CSV 为 265 physical lines／65358 bytes／264 records／264 unique／9 columns／
SHA-256 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`。
新增仅为物理行 265 的 `fm.language.save_unknown(errorCode)`／`error`；严重度
为 info 172／warning 74／error 5／blocking 13，参数声明与双语 placeholder
无 mismatch。live 前 264 个物理行 SHA-256
`d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`
并与只读 snapshot byte-identical；snapshot receipt 仍为
`980e8a942c06b1b6ebcec32d428135a656d5bd0ae089b087d82e3c4d5d3564b8`，
两者均为 `-r--r--r--`，diff-check 无空白错误。

该候选由原 R 在回合 `01a0f3f2-c361-7311-9bad-db9b91046023` completed，final
`msg_0d29bf8d726d8dc0016abd6de210dc87d0a7a904c914de06f9`，唯一 verdict 为
**ACCEPT**。R 确认新增中英文没有虚称已保存或旧文件仍在；§10 精确区分
pre-commit 未触旧字节／已验证回滚恢复旧字节的 `save_failed`，与 post-commit
复读、解析、比对无法证明 desired／old 或回滚无法证明旧字节恢复的
`save_unknown`。所有 UNKNOWN 路径保留 session 新 locale，只幂等重写同一
desired locale 并复读核定；`errorCode` 仅受控诊断码，不进入 PlayerSave、
OperationId 或业务 save recovery。

R 复核本次只追加 1 条／650 bytes；旧 263 条内容与顺序、只读 snapshot 和
receipt 均未变化。planning 前 347 行仍为上次接收身份
`8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`，
仅追加 25 行；当前 78 条参数化记录，列宽、必填、唯一性、双语参数集合、
括号、UTF-8 与尾随空白检查均无问题。该接收可作为 LOC-R3 FIX-01 对齐 F2
的上游 264-key COPY 输入；UGUI 历史 263-key snapshot 身份继续固定。它不
关闭 LOC-R3 的 F1／F3／F4，不替代 F2 的 store 设计、故障验证或实施审查，
也不证明 runtime／Luban／Unity／画面／设备。R 全程只读，未修改文件／Git，
未运行 Unity、Luban、preflight、构建、产品测试或下载安装；不授权 A／B
实施。

为避免 Android 图形设计与 LOC-A／UGUI 源码审查混用，主测试建立独立
R-ANDROID。实际 thread 为 `01a0f40f-6bc6-72d2-b3dc-eb954ad0716a` / `local`，
唯一派工／收件仍是主测试；其首次候选 `ANDROID-GFX-01-DESIGN` 实际 turn
`01a0f40f-6d7c-7fb3-b6bd-272539f0f77b` completed，final
`msg_06adc457b2dc221e016abd767977d487d0a85a98fc00b984be`，唯一 verdict 为
**NEEDS_FIX**。

主测试已只读确认候选 `engineering-android-graphics-001.md` 为 307 lines／
23981 bytes／SHA-256
`09b54b1f448ba875372262b5a0e2c699fed3eb4a70b479b77a7403a5e7bd5134`，
状态 `READY_FOR_INDEPENDENT_REVIEW`，diff-check 通过。审查限定旧 019／
BlueStacks 证据外推、Auto／Vulkan→GLES3／GLES3-only 三候选、uGUI→API24→
graphics owner 顺序、两文件白名单、Unity API 写入／恢复、report、Q6／Q7、
rollback 与未验证门；设计 `ACCEPT` 也不授权实现、Unity、ADB 或设备。

唯一阻断在 Q6 恢复证据：当前只要求构建结束时同进程 `PlatformMatches()` 与
raw bytes restore，首次持久写入的复开和整包回滚也不能替代每次构建恢复。
最小 `ANDROID-GFX-01-DESIGN-FIX-01` 只修改同一设计文档的 Q6、失败门和最终
验收，明确 QA、normal 各自成功路径及一个可控 Capture 后失败路径都必须执行
restore→save→关闭原 Unity→启动独立 `2022.3.18f1` 新进程→复读 Auto／完整
有序 API list／`PlatformMatches()`／raw bytes＋SHA。若失败发生在 BuildReport
生成前，仍须在对应 create-once 目录留下绑定输入身份的 restore/reopen 回执。
可控失败入口只能在既有两文件内、Editor-only、无 public／runtime／test seam；
闭合不了则 `BLOCKED`，不得增加第三文件。

其余合同已通过：旧 019 只限定 BlueStacks Vulkan native crash 和 GLES3 诊断
对照，不外推真机；GLES3-only 首候选与另外两项拒绝成立；uGUI→API24→
graphics 顺序、两文件白名单、API list→Auto 恢复、CheckProduction／report、
manifest、BlueStacks／真机三组、Vulkan `NOT_SUPPORTED_BY_CANDIDATE`、rollback
与禁止范围准确。现源码的 list restore 缺口和旧 stage／001..020 路径事实成立，
新入口未冻结前不能构建。post-uGUI／post-API24 身份、APK／manifest／report、
BlueStacks 与真机证据均未验证；本轮零文件／Git 写入，未运行 Unity、ADB、
设备、构建或测试。

本包必要修订只回同一 R-ANDROID。此后 QA 优先复用原 R、R-TOOLS、R-ANDROID，
不再因题材自动新建审查角色；只有三者同时占用且形成真实关键路径时，才向中央
提交具体候选与冲突，由中央决定。Android 设计审查不是已接收 UGUI 进入源码／
222 验证的额外前置门。

中央同时确认 LOC-A 第一次 production pass 的真实失败为 Luban severity set
分隔语法，不改变 264 条文案、四值集合或发布语义。该缺陷由主程核固定源码后
直接签最小技术纠错，保留首次失败和新的 create-once 证据根继续；不再为同一
语法勘误重复等待整体设计前置审查。该调度不放宽独立接收：实际修正代码、正例
与负例证据仍须交原 R，只能由它给唯一 verdict；未获接收前不得把失败升级为
通过或授权 B。UGUI 源码／222 验证仍由唯一 C 推进，Android 设计线不得成为其
额外前置门。

主程已对 `LOC-IMPL-A FIX-02` 做 Standards／Spec 双轴审查并给内部
`ACCEPT`，只授权进入原 R 独立复核。主测试已直接读取实际文件，不能仅靠
`git diff`：任务包 `engineering-localization-luban-impl-a-fix-02.md` SHA-256
`3d437c485903ab25569f9f417f49fd1653125428a4f3913843d15b7c35a62a35`；
设计 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；
live COPY `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`。
8 个生产文件实际 SHA 已逐一核对，`FightMatch.Localization.Compiler.csproj`
虽被 `.gitignore:15` 的 `*.csproj` 忽略但文件存在并纳入候选，其余 7 个显示为
未跟踪新增。

FIX-02 成功 evidence root 恰有 74 个普通文件、0 symlink，且无 `bin`／`obj`／
`__pycache__`／`.meta`；关键 `run`／`inputs`／`commands`／`comparison`／license／
self-tests／diff-check SHA 与主程回执匹配。候选已派原 R，实际 turn
`01a0f437-52d6-7c30-a424-86b561f3c0f8` 当前 in progress，final 未到。R 必须
读取实际 8 文件、两个不可变失败根和 FIX-02 create-once 证据，核 10 commands、
264 records、12/12、三负例、双 raw、canonical／manifest／semantic、132 licenses、
scope 和未运行门；禁止重跑工具链。原 R 已在该 turn completed，final
`msg_0d29bf8d726d8dc0016abd8257794887d0871f3e7f431baaa0`，唯一 verdict 为
**NEEDS_FIX**；LOC-A 尚未接收。

两个阻断项为：

1. **P1／Spec**：`Program.cs:861–873` 在找不到本地 license／notice／COPYING
   entry 时，仅因 nuspec 存在便返回 `complete=true`。实际
   YamlDotNet.NetCore 1.0.0 nuspec 没有 license／licenseUrl，本地包与 nupkg 也
   无许可文件；NeoLua 1.3.14 只有浮动远程 licenseUrl，本地同样无许可文件，
   但 YamlDotNet 与 NeoLua 三个部署 DLL 仍被标 complete 并发布 Generated。
   这违反冻结的缺失即 `LICENSE_INVENTORY_INCOMPLETE` 门；132 文件覆盖／SHA
   完整不能替代许可内容完整，也不能联网或用 Luban 主仓 MIT 补洞。
2. **P2／Standards＋evidence**：10 项真实进程记录未成立。compiler-compile 的
   外层 shell 未保存 csc `$?`，exit 0 实际来自末尾 `date`，之后又事后写常量
   `0`／空 stderr；stdout 记录的是元数据而非 Roslyn 原始输出。generate-driver
   也自写空 stdout／stderr 和 exit，而真实执行记录含 `PASS_LOC_IMPL_A` 输出。
   DLL 可运行和 driver 真成功只是旁证，不能代替真实 argv／exit／两流／timing。

最小后续包只修 `Program.cs` 的许可证判定和真实进程 evidence recorder，保留
parser／canonicalizer、264 COPY／formal 数据和其余生产输入；补缺 license、
URL-only、source／entry SHA mismatch 的失败门，许可证材料只可在已授权 cache
只读寻找，找不到即如实阻断。主程须签唯一新 create-once 根，由外层真实捕获
Roslyn／driver 进程证据；原根、FIX-01、FIX-02、005 和两个未接收 Generated
候选全部 immutable，不得回填旧证据求绿。

主程随后完成只读许可证来源研究与 FIX-03 预备包。研究文件
`research-loc-a-license-sources.md` 为 170 lines／11681 bytes／SHA-256
`5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`；
预备包 `engineering-localization-luban-impl-a-fix-03.md` 为 319 lines／20411 bytes／
SHA-256
`374234b8ed0f015af13eeff3771c15343a2d497e7fc54909152dad207322a812`；
两者 `git diff --check` 均通过。研究确认现有本地／固定输入无法把足够的
YamlDotNet.NetCore 1.0.0 许可证证据绑定到确切 nupkg；候选上游 LICENSE 不能
替代该闭环。故 FIX-03 当前只可记为
`PREPARED_NOT_AUTHORIZED / BLOCKED_LICENSE_INPUT`：不得创建 FIX-03 evidence root、
不得运行 compiler／Luban，也不得发布 Generated。LOC-A 继续保持未接收，B 及
后续门继续关闭；若许可证输入未获可靠补齐，不能靠放宽门或改写旧证据推进。
该阻断是固定 Luban runner／当前依赖组合的局部输入缺口，不是全局停工。中央已
允许主程在不改变用户选择的 Luban 路线、输出和游戏语义前提下，继续只读评估
许可证可追溯的官方版本或最小构建依赖替换；用户并未固定 5.1.0 或旧
YamlDotNet.NetCore 包。该评估只能形成固定设计草案并回同一 R 审查，不得直接
实施或换 pin；只有实际改变用户路线、付费或人工账号等才升级用户决策。原候选
和证据保持不可变，C／UGUI 可继续其独立范围。

主程现已完成两份替代设计并给内部 `ACCEPT prepared only`。固定输入为：ARCH
`engineering-localization-license-alternative-architecture.md`，482 lines／33087 bytes／
SHA-256
`01c47276258569f3617b613f0ed72fe9c71043c633dab084f097e999dbd7e936`；
SYS `engineering-localization-license-alternative-system-design.md`，571 lines／39106 bytes／
SHA-256
`b43c17075c8012323b16673810124b9bdd0ca6ea52a6e0d1e38047d7f48d9257`；
许可证研究仍为 170 lines／11681 bytes／SHA-256
`5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`。
三文件 `git diff --check` 均通过。

本轮唯一候选是 official Luban `v5.1.0` source／commit
`df34215d2e22035073d1af83d44e3ac0744dc3b3` 加两处
`YamlDotNet.NetCore 1.0.0` → official `YamlDotNet 16.3.0` 的
`PackageReference` 替换，且必须标为 FightMatch patched runner；SYS 的候选 B／C、
YamlDotNet 17／18 与旧 DLL 考古只作排除背景，不进入本轮。主测试已把三份固定
输入退回同一原 LOC-A R，实际 design-review turn
`01a0f467-89a8-75b1-808c-de1d25aaa3b8` 当前 in progress。R 只读核一手 provenance、
package／source／license 绑定、两 csproj patch、offline closure、YAML／UnityAsset／
CSV oracle、official tests／005／两 pass／12+self-tests／negatives、`Program.cs`
唯一 repo delta、旧 cache／evidence 只读、预算与 rollback；只给一个
`ACCEPT`／`NEEDS_FIX`／`REJECT`。设计即使 `ACCEPT` 也只允许另签 proof 实施包，
不直接实施或接收 FIX-03／LOC-A／LOC-B。

原 R 已在该 turn completed，final
`msg_0d29bf8d726d8dc0016abd8d6b85d487d094d577ce867f83c9`，唯一 verdict 为
**NEEDS_FIX**。R 严格只审上述单一 YamlDotNet 16.3.0 路线，没有把 B／C、17／18
或旧 DLL 考古重新纳入候选；三份固定文件开始／结束身份均未变。本轮四个阻断为：

1. **F1／P1 Spec**：删除 nuspec fallback 后，允许的四份外部许可证仍无法覆盖
   runner 中保留且没有包内 license／notice／COPYING entry 的六个组件：
   ExcelDataReader 3.7.0、Google.Protobuf 3.29.0、MessagePack.Annotations 3.1.7、
   MessagePack 3.1.7、NLog 5.3.4、Scriban 7.2.5。须补 exact
   package→source/version→真实 license/notice 输入矩阵、独立 acquisition/prestage
   范围和完整路径白名单；找不到即继续阻断，不得用 SPDX／nuspec／通用 MIT 或
   Luban 许可替代。
2. **F2／P2 Standards**：SYS 把尚未执行 clean build／publish 才能得到的 runner
   count／manifest 和 candidate `Luban.dll` SHA 列为首个 proof 包的签包前置，形成
   授权循环。须拆为材料／完整离线 graph 冻结 → 外置 runner proof 实测并封存 →
   proof 独立接收后签 LOC-A `Program.cs` 适配三阶段。
3. **F3／P2 Standards**：ARCH 只允许一次 restore／build／publish，SYS 要求两个
   全新 root 的两次 clean build／publish；official tests 另有当前 cache 缺少的五个
   exact test packages，且 harness 会创建 temp project 并启动额外 `dotnet build`。
   须统一次数／预算，并把官方测试 graph、后代进程、离线 lock/cache、PATH／SDK、
   temp root、timeout／logs 全部纳入闭包，禁止自动联网 restore。
4. **F4／P2 Spec**：SYS 把任何 negative 返回 0 都判失败，与冻结的三项负例 harness
   协议相反。须逐层冻结：被测非法输入进程非零且产生固定诊断、零 publication；
   harness 验证成功后 exit 0，harness 非零才表示验证失败。

YamlDotNet 16.3.0 的 package／commit／DLL／两份许可证研究身份及两处 csproj
replacement 范围均获正面核对；package signature 仍只确认 entry 存在，实际验证留在
未来硬门。最小纠正只修改 ARCH／SYS 文档，保留 research、唯一版本和原证据；修订
后只回同一 R 复核，不直接 acquisition／proof／实施。

主程已按四项 finding 完成最终设计修订并给内部复核通过。当前固定 ARCH 为
651 lines／46233 bytes／SHA-256
`35afce4785af87f5cf0e562117a0ab2cc1dbe7429af06c8dbe17c4a82120d1ff`；
当前固定 SYS 为 881 lines／59206 bytes／SHA-256
`18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`；
research 仍为
`5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`，
三文件 `git diff --check` 通过。修订明确 ARCH B1＝SYS Candidate A，统一 M material
freeze → B external build proof → L `Program.cs` adapter 单向三阶段、目录／evidence
roots／硬预算和双空 runner root；M 不要求未来 runner hash。它补齐六个旧 fallback
组件的 official source／commit／真实许可证闭包门，也固定 official
`Luban.Tests.csproj` SHA
`d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1`
及五个 direct test seeds，并要求展开完整 transitive／analyzer／build／native／
contentfiles／testhost closure；DUT／harness 双层 exit 合同亦已统一。

该修订已回同一 R，实际复审 turn
`01a0f484-8745-7b62-8057-3d1c49210595` 当前 in progress。仍只做设计复审；若
`ACCEPT` 也仅允许另签 M 材料包，不授权 acquire／proof／build／test／LOC-A 实施。

同一 R 已在该 turn completed，final
`msg_0d29bf8d726d8dc0016abd94a6ec0087d0934d80e2063745ad`，唯一 verdict 仍为
**NEEDS_FIX**。旧 F1 的设计闭包、旧 F3 的五个 direct test seeds／完整 test closure
和旧 F4 的 DUT／harness 双层 exit 已补入；但 ARCH 仍有四项 P2 合同与 SYS 冲突：

1. **R1／Spec**：ARCH 材料阶段仍要求从未来 publish output／runner manifest 反向
   枚举 deployed entries。须改成 M 只从 locked graph、selected nupkg entries、
   official source 和只读旧 oracle 冻结 expected mapping；B 才对两个实际 publish
   output／runner manifest 做 exactly-once coverage 并与 M 集合核对，不反补 M。
2. **R2／Standards**：ARCH 禁止 R 接收前复制 final cache，但 SYS 要求 R 同时审
   temp proof 与 create-once inactive final bytes。须统一为技术通过 → create-once
   `CANDIDATE_NOT_ACTIVE` final copy → R 审 temp＋final → `B-ACCEPTED` → L；R 前
   禁止激活／派 L，而非禁止候选复制。
3. **R3／Standards**：ARCH 限“一个外部进程”，SYS 限“一次一个顶层 process tree”。
   须统一后者；树内 child 的最大同时数／启动数仍由 M exact DAG 冻结并真实记录，
   不得漏掉 dotnet／MSBuild／compiler／testhost。
4. **R4／Spec**：ARCH 允许删除未接收 candidate sibling，SYS 要求 create-once 失败
   候选保留。须统一保留 M material、B final 和 evidence 并标记不可用；新 revision
   只可另签 sibling root，不删除、覆盖或复用。

R 建议最小 `LOC-LIC-ALT-ARCH-FIX-03` 只写 ARCH；SYS
`18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`
与 research 保持只读。修后仍只回同一 R，不放行 M／B／L。

主程已完成该四项 ARCH-only 修订；当前 ARCH 为 681 lines／49110 bytes／SHA-256
`3dba4f92c17cc50fc158963857a69f70d308a8dab36dcbdf3a9fbbf821a06cb7`，
SYS／research 身份不变，三文件 `git diff --check` 通过。主测试已把 FIX-03 只按四个
P2 回同一 R，实际 turn `01a0f493-0083-7063-88e7-e9c17d65ae55` 当前
in progress：M expected mapping 与 B actual outputs 分层、inactive final copy→R→L
顺序、单 top-level process tree 加冻结 child counts、所有 create-once 失败候选保留
四项必须全部与 SYS 一致；其它路线和范围不重开。

同一 R 已在该 turn completed，final
`msg_0d29bf8d726d8dc0016abd96c5370487d0b6d99135bc6c7c1d`，唯一 verdict 为
**ACCEPT（仅设计接收）**。R 实读确认四项 P2 均与 SYS 对齐，固定 ARCH／SYS／
research 身份未变，本轮 Standards／Spec 剩余 findings=0。该 `ACCEPT` 只表示设计
可进入后续 M packet 签包；不授权 M／B／L 执行、acquire、restore、build、publish、
test、Luban、005、Unity 或 LOC-A publication，也不关闭实际许可证材料阻塞。

在上述仅设计 `ACCEPT` 之后，主程已另行完成 M 包审查并正式发出 activation receipt。
当前唯一 M 包 `engineering-loc-license-alt-m-materials.md` 为 582 lines／30837 bytes／
SHA-256 `0ce7c44844494e8b63c5e6c149fa357932c11c9a53457275a24a16ac4ef48d13`；delivery owner
仍为 LOC-A implementation thread `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`，
实际执行 turn `01a0f4a3-c824-7e70-af0a-66c501813e7a` 已核为 in progress。该授权只
允许三个 create-once root 内取得并核验 official source／package／license、生成两行
YamlDotNet replacement material、离线 closure／locks 和 finite expected deployment
mapping；严禁 build、publish、test、Luban、005、Unity、`Program.cs`、产品文件和 Git
写入。M 成功终点仅为 `READY_FOR_INDEPENDENT_M_R`，须回同一原 R 独立审查；当前尚无
M verdict，B／L 与 LOC-A 实施仍关闭。

M 实际 turn 已 completed，并按最早失败门封存为
`BLOCKED_PACKAGE_SIGNATURE`。42 个顺序 official NuGet 请求共取得 23,259,536 bytes；
YamlDotNet 16.3.0 nupkg SHA-256
`e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4` 与包内
`.signature.p7s` SHA-256
`ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308` 均匹配固定值。
但固定 `.NET SDK 8.0.425` osx-x64 的 `dotnet nuget verify --all` 在 macOS 27.0／
arm64 host 返回 exit 1、两行 `CSSM_ModuleLoad()` invalid parameter 和 `NU3003`。
`result.json` 为 2367 bytes／SHA-256
`a57094354718377a4b0204793fc2e8c3d14a4316812dd0b74ee834076f8fff5e`；evidence manifest
SHA-256 为
`51b4453620bde4d4e8fdbdc9093b30cef4a5349c03a2ceb3c4ed2ad5b8c66efc`；stderr／stdout
SHA-256 分别为
`94dacb4185870281e1eab3113877971153996501f5b5d4d5d83dd6a3d93d5821`、
`6d0a25b84a5b76b23bfe3bd2fbee2e0f67473d99888c2650592e522dd0aeb55f`。
旧 cache／005／三棵历史 evidence、八个生产路径及受保护区 before／after 全相等；
material／B／L roots 未创建，build／publish／test／Luban／005／Unity／产品／Git 写入
全部为 0。

QA 对照 Microsoft 的 `dotnet nuget verify` 与 signed-package verification 官方说明：
`NU3003` 合并表示“签名无效或当前平台无法验证”，且 signed-package verification 当前
明确不支持 macOS。因此现有证据支持**平台验签能力失败**，不能把包判坏，也不能把
匹配 entry hash 记成密码学验签通过；签名硬门保持开启。既有 durable Linux 会话随后
只读执行 `QA-CLOUD-NUGET-SIG-PREFLIGHT-001`，实际 turn
`01a0f4ac-72aa-7081-af3c-3b37f4f0dbc0` 返回 `NOT_READY`：Debian 13 x86_64 无
dotnet SDK、NuGet 或 Mono，只有 Unity 内嵌 .NET 6.0.21 runtime，故
`dotnet nuget verify` 不可用；未下载、安装、运行 workload 或修改 trust store。后续若
继续，须由主程另签最小受支持平台验签适配，不能静默关闭签名门或复用本次 M root。

中央随后授权只用官方固定输入做最小 Linux 适配。云端先执行 metadata-only
`QA-CLOUD-NUGET-SIG-METADATA-002`，实际 turn
`01a0f4b2-6d03-7148-b9be-d8b5b5ad58ce` 返回 `BLOCKED_NETWORK_POLICY`：唯一
`releases.json` GET 在代理 CONNECT 阶段收到 HTTP 403；curl exit 56／HTTP 000、
0 body bytes、0 redirects，SDK HEAD 未执行，SDK／nupkg 均未下载。证据根
`/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-METADATA-002/` 共 15 文件／
6399 bytes，`SHA256SUMS` 自校验通过；无空或伪造 metadata、无安装／trust／项目／
Git／Unity 写入。该结果只证明此 endpoint 被当前代理阻断，不否定主程本机已取得的
官方条目。

主程裁定不重复已知 403；以其 2026-10-01 取得的官方 metadata body
1,584,285 bytes／SHA-256
`21cc0b1f333822c9c86df690b9dad0191ee3bd4abebd05ad86a0bb353e38fc0b`
锁定 release 8.0.31／2026-09-08、SDK 8.0.425 linux-x64、SDK body
216,833,108 bytes 与 SHA-512
`934b8060a7190e5909ad1fd0785db542f487b3bbf6cdd14826b02095fdd0d0394298b1634085eff302928fccc33f7c1a7253e9b87df555fc36fce819bcd2e798`。
主测试据此起草 `testing-loc-license-alt-signature-linux.md`。208 lines／11577 bytes／
SHA-256 `c2e14a056dc7d862132bfcc44e60a99236a92cf9a52b88886b3f6c23a986ba1c`
只作初始 `PREPARED_FOR_ENGINEERING_REVIEW`；主程首个 252-line activation 因错误重设
`HOME` 与上位开发约束冲突，已在派云前撤销，旧 SHA
`87878087c1941c294f041baae3fef91d0889b6925f72df60d13dd7e775e97b99`
永久作废且从未执行。当前唯一有效包为 259 lines／14330 bytes／SHA-256
`2de9b041bd5296bbd776bb70ec74672df474f87a1dd43a4fcbbd6276cc3a5e48`；主程已给唯一
activation receipt，QA 机械复核后调度给固定 durable owner，实际 turn
`01a0f4be-6200-7730-a905-e8636b9b05ac` 当前 in progress。最终包禁止重请求 metadata，
只允许 fixed SDK／nupkg 各最多 HEAD＋GET、专用 create-once root、本地解压及一次
`--version`／`--info`／help／`verify --all`；明确禁止设置或覆盖
`HOME`／`home`／`CODEX_HOME`，仅使用任务专属 DOTNET/NUGET cache/scratch 变量和显式
空 config。镜像、installer、apt／sudo、trust、项目、Git、Unity 均禁止；真实结果尚未
返回，不得提前关闭原 M 签名门。

003 实际 turn 已 completed，唯一状态 `BLOCKED_AUTHORITY_MISMATCH`：包把三份
predecessor seal 误写成 `network/metadata.*`，实际文件在 METADATA-002 root 顶层；
执行者没有静默映射。`run.json` 与 `SHA256SUMS` identity 均匹配且自校验 exit 0，随后
在最早门停止；network requests=0，SDK／nupkg HEAD／GET、下载、解压、dotnet 和验签
全部 `NOT_RUN`。003 root 共 10 files／23060 bytes，`result.json`／
`evidence-files.tsv` SHA-256 分别为
`b199ae83586cbecc3880cb670fbc1bfed7254061c512c1a4521bc8958d321f6c`、
`a6207b732255561332ad584b0c842fee8a5f7312ee9e688c29f74c5beada5996`；用户
dotnet／NuGet 路径、profiles、trust、项目、Git、Unity 与环境配置均未变化，无遗留
进程。主程裁定该结果为签包 predecessor path typo 的 `NEEDS_FIX`，不是云执行错误或
package/signature verdict；003 永久保留，不得复用或覆盖。

QA 已创建 sibling `testing-loc-license-alt-signature-linux-004.md`，279 lines／
15275 bytes／SHA-256
`e9e8e9771052e0dd0fd40897487d3fc218d67401c363accdc9416fdbd23a2794`。
相对 003 最终合同仅变更 task/root 003→004、三条 predecessor path 改为真实 root-level
路径，并新增 003 零动作封存段；其余 URL／hash／预算／隔离／HOME 禁令／R 合同逐字
不变。主程已机械审查并给 activation receipt，QA 调度相同 durable owner；004 当前
preflight 一次核全五项 seal 和 `SHA256SUMS` 后才联网。执行中先后补入 A01 57 lines／
3093 bytes／SHA-256
`8f42f5aa484cec687df5a4aa285ea73f75c4567f04a59a0c48a09f2982eae48d`
与 A02 66 lines／3377 bytes／SHA-256
`f3e2b246d2125b66532ed43149836f6ba40b928963a76379fb55350ad3045c12`：A01 增加禁用
ASP.NET 开发证书与 global-tools PATH 副作用的两个子进程变量；因它在一次真实 `--info`
之后到达，A02 诚实保留旧命令并只约束尚未运行的 help／verify，同时把实测专用 root
上限从 768 MiB 修到 832 MiB，禁止删证据制造合规。

004 执行与 A02 final turns 分别为
`01a0f4c9-2606-74be-b8a3-59dcf1cc81c3`、
`01a0f4d2-cf3a-718f-931a-b92ec2c72f3b`，终态
`READY_FOR_INDEPENDENT_SIGNATURE_R`。official fixed SDK 216,833,108 bytes／SHA-512
匹配，5559 archive entries／4704 files 无危险成员，`--info` exit 0 且确认为 8.0.425／
linux-x64；没有重下载、重解压或重跑。YamlDotNet nupkg 776,880 bytes／SHA-256 与
唯一 `.signature.p7s` 12,920 bytes／SHA-256 均匹配固定值。一次
`dotnet nuget verify --all` exit 0，raw output 明确包含成功验证；0 errors／4 warnings，
四条 NU3018／NU3028 为吊销服务器不可达／吊销状态未知，已原样保存而未抑制。

共 4 次 fixed URL requests、0 retry／redirect；help／verify 使用 A01 子进程变量，真实
`~/.dotnet`／`~/.nuget` 仍不存在、profiles 未变，无 trust／项目／Git／Unity／环境写入
或遗留进程。最终 root 810,562,838 bytes，小于 872,415,232；free space
29,799,223,296 bytes，墙钟 1110 秒。`result.json`／4801-file
`evidence-files.tsv` SHA-256 分别为
`d03fad30914fe6e01f89cd77596fe3716022099d9e26f10fc319ac7391d1ef29`、
`43b7843df644582dff0bd467ed2c0d6d0a7365653adfac23797991980cf72bf2`。
当前已交同一原独立 R，唯一 turn
`01a0f4dd-0664-7b92-a5cf-2351ee317c5e` completed，final
`msg_0d29bf8d726d8dc0016abdab8dd80c87d0907a376faa94426b`，唯一 verdict 为
**ACCEPT（仅 package-signature gate）**。R 确认固定 SDK／package／signature entry／
唯一 verify 的身份与 raw success 一致，Standards／Spec findings 均为 0。四条
NU3018／NU3028 只是 OfflineRevocation／RevocationStatusUnknown；固定合同不要求零警告
或在线吊销成功，故不阻断此限定门，但不得声称证书当前未被吊销或在线 revocation 已
验证。旧 Mac M `BLOCKED_PACKAGE_SIGNATURE` result 保持不改；下一步必须另签新 sibling
M continuation，继续尚未执行的 source／license／closure、两个 locks、deployment mapping、
B/L command budgets、material seal 与整 M 独审，不能直接进 B／L。

其余核对成立：FIX-02 实际 9 个 diff hunk 只属获准 provenance／root／status；
264 records／264 unique、严重度 13／5／74／172、参数化 78、12／12、三负例、
双 raw、canonical／manifest／semantic／Generated 一致；74 文件闭集和 8 生产
路径符合范围，protected status 前后均为
`17817aa0f38cf604e3c99ea56f66c7dc0e65ed6bba239703244af6353c0eef5a`。
Unity／B／Host／Windows／device／CDN／发行均未开启。本轮只读，未修改文件／
Git 或重跑任何工具链。

中央同时允许在 C 的源码 Q1 已闭合并开始 Unity 后，若主程实际交付准确冻结
源码身份、完整真实 diff 与 Q1，可让空闲 R-TOOLS 先并行做 UGUI 只读源码／标准
预审；不得预派空候选、提前 `ACCEPT` 或重复派 R。资源、Q2／Q3／222 真实证据
齐备后，仍由同一 R 对同一完整候选终审；若编译导致源码变化，只回同一 R 最小
diff 和新身份。QA 不额外运行测试。

主程随后实际交付 UGUI 源码预审候选：packet
`engineering-ugui-01-implementation.md` SHA-256
`ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`
（435 lines／26488 bytes），candidate aggregate
`704247142aab5c17fa2569bdc14e9d0191a6e41474b63ddb3b8fd0d584cdbbc2`。
source manifest SHA-256
`382f34ed8062c6d41d0b0f7699b19ea9baeeaccc9681cd72cda385756490a2d3`
含 53 项；主测试已重新读取实际路径，53／53 存在且 bytes／SHA mismatch=0。
原 `q1.json` SHA-256
`76eab01877a15b2e4835eba2d1178bdb5948b6e9593b2be53cbbbe40aa180f42`
保持，`q1-effective.json` SHA-256
`ab1c35b1d8ad801b5d59f41a2874462dd80dd4f4a2c84df3341ef52c7012ae01`
只裁决 `LocalizationContractTests.cs` 三条禁止 reflection／`SendMessage` 的字面
断言误报；`unexpectedHits=[]`、manifest 未变、其它检查通过、没有产品／测试
写入。`expected-fullnames-222.json` SHA-256
`cf86495bdf8e0a035537be2f963b0915829d6c677fb57d62bb72cca4beb4c66b`
恰有 222 项，但只是冻结期望。

因原 R 正在审 LOC-A，UGUI 预审复用已接收 binding 合同的 R-TOOLS，实际 turn
`01a0f43a-279f-7991-8364-876fcf94273b` 当前 in progress，final 未到。本回合只
允许 `PRE-REVIEW CLEAN` 或 `PRELIMINARY FINDINGS`，不得给最终三态 verdict。
R-TOOLS 读取实际 53 文件并做 Standards／Spec 静态审查；资源／独立复开、Q3
compile、Q4 222 XML／log／result 齐备后，仍由它对同一完整候选终审。若 C 因
真实编译失败改源码，只回该 R 最小 diff、新 candidate／manifest 身份和影响面。

C 随后在真实 Intel Unity `2022.3.18f1` 首次导入发现两个测试夹具的确定性
编译错误：一处缺 namespace 引用，一处 `CanvasRenderer.GetMesh` 重载不适配
2022.3；资源生成方法尚未执行。C 正做最小修复并重新冻结，因此旧 candidate
`704247142aab5c17fa2569bdc14e9d0191a6e41474b63ddb3b8fd0d584cdbbc2` 不再能
作为最终源码身份。R-TOOLS 保留旧 manifest 中未变化文件的
已读结论；受影响两文件只在主程交付新旧 candidate／manifest、准确 path、old／
new SHA／bytes、最小实际 diff、错误映射、更新 Q1 和首次导入／编译日志身份后
续审，不把计划内修复误记为未知并发污染。新资源、独立复开、Q3 通过和 Q4
222 仍未到；此阶段继续禁止最终三态 verdict。

主程随后冻结新 candidate
`cb3922b36246dc2230d56f53a62a29e86a30770c237df86aacce608f2da388c5`，
source manifest SHA-256
`8f61a93db270e5de2a46b39fbb50b2544417e55633951d5c0fdf59e99871b601`。
主测试比较新旧两个 53 项 manifest，唯一变化为
`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`：15639 bytes／
`91f742d955d19bb3871b30c51f1f0a1bd25b26a92022cc9cfe1104272aa09be1`
变为 15498 bytes／
`f614405d8bdd86ec55c82634f13d241c9cabba445189c78c9bfd4ea5609ac136`；
其余 52／52 path／bytes／SHA 完全相同。修订只补 `FightMatch.Content` namespace，
并把不适配 2022.3 的带参数 `CanvasRenderer.GetMesh` 改为返回 renderer-owned
`Mesh` 的无参调用，不创建／销毁该 mesh，几何断言与测试方法数不变。

旧首次失败 `resources/prepare/run.json` SHA-256
`48da1eb56028dedc745a966252d4ed9ce3b14e9eb212651dc7ab799c4c9aafa2`
记录 Intel Unity `2022.3.18f1`、26.63 秒、exit 1、两项编译错误且 source 未变；
资源方法未执行。新 predecessor receipt SHA-256
`54036b521a5095ef61271e11cf0e9fa86c9e7307b637236ab2af5e0c732d88d1`。
新 Q1 SHA-256
`8bb7d0081a2e540328b700d82330c22af99beb2ab8132288975ac624d1b49077`
为 pass，仍只豁免三条禁止反射／`SendMessage` 的字面断言；222 fullname receipt
SHA 保持 `cf86495bdf8e0a035537be2f963b0915829d6c677fb57d62bb72cca4beb4c66b`。
以上最小变化已补给同一 active R-TOOLS turn；它继承 52 个未变文件结论，只补
审此文件、失败回链与新 Q1，仍不得在后续运行证据到齐前给最终三态 verdict。

R-TOOLS 在同一预审 turn 收尾重新核工作区时，先发现相对 `cb3922…` manifest
已有 4 项变化，未把它们纳入旧身份结论，也未给正式三态。主程随后交付完整
身份链。第一段为
`cb3922b36246dc2230d56f53a62a29e86a30770c237df86aacce608f2da388c5`
到
`fd8c9849e3fccfc76dfd909e2b3ee9ff55ebcb55008544f9cb44ee3a788faf60`；
新 53 项 manifest SHA-256 为
`be358649cbcdd61709b3f13252d60d6bc17b3237c370bf724f686906e73811fc`，
四文件 identity／diff／source-manifest-comparison SHA-256 依次为
`0a3a3ae4b85bbed39c035561bcd082da43270fd64b7f0bc41b6f91aaa5eced9e`、
`601d30cfbc98a895f809f8702a7ad6cc14f3784a7210ab69c8a91b4fcad0e9ac`、
`e29e522d907c01fc89ddb933de711103a09beb80e5578f9a67ab03de7e7075dd`。
主测试已读 comparison JSON，恰有 4 项变化、其余 49 项不变：

- `FightMatchAndroidBuild.cs`：70380 bytes／
  `c94f581b3c65f3b3d15a21e359b97c671964ec991dc58f3d8c0c0fc50668e0af`
  到 71156 bytes／
  `4e185d4fbb5fc21eb23f909c4faacfa231a28d4b074db4163579cb9b34100186`；
- `LocalizedTextBindingTests.cs`：13087 bytes／
  `8f411ed24060224453e6c61e949d977196267676118d403d40ebe540d436b095`
  到 13796 bytes／
  `997b951913c245b115c9846063b757fb3696cbccba69e3dea1692a0be3640cc9`；
- `UguiSceneCompositionTests.cs`：15498 bytes／
  `f614405d8bdd86ec55c82634f13d241c9cabba445189c78c9bfd4ea5609ac136`
  到 16190 bytes／
  `8b0d37e00d4aa6503c0a651543df3851f3a1c9b328d3a7899c44192ff642871b`；
- `FightMatchHostResourceTests.cs`：7285 bytes／
  `465febbd2b10587821eb8422c45c783bf71f533d961bd915c7d1cf6f8810f813`
  到 7935 bytes／
  `8caf1736ee6e28857ec98234de07264a3101e078cddebda8624c6428404cb1f0`。

对应语义修订把 119 个 TMP 文本对象的诊断占位符规则明确为：3 个
`TMP_InputField` 的 linked `input.text` 必须精确等于占位符，display text 可精确
等于占位符或只多一个尾随 U+200B；其余 116 个必须精确等于占位符。字体与材质
断言不变。该完整四文件差异已补给同一 R-TOOLS 预审。

第二段从 `fd8c…` 到当前源码预审候选
`b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669`；
新 manifest SHA-256 为
`c52dd78f25fc8ba9a51d930803185866d9e0ad5c931707c7e8da391db5e94849`。
只有两个测试文件各增加 16 bytes：`LocalizedTextBindingTests.cs` 变为
13812 bytes／
`a83b0085ff779ffd85a224f86fbd1cfd8f33c58f48724cf41890c14fbe3bab8b`，
`UguiSceneCompositionTests.cs` 变为 16206 bytes／
`9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0`。
两处只把新断言的 `Is.EqualTo(...)` 限定为
`NUnit.Framework.Is.EqualTo(...)`，修复既有静态 `Is` helper 造成的 CS0119，
谓词和计数未变；two-file identity／two-line diff SHA-256 为
`9ca975f4b4c5eca6b636ca9c84ea7beaeb05a255dc260c6bcbd4e69415a3bcc3`、
`aea085c24c7d536b929a0664c15aa8baa38cce46e0082d0072009417ee0d8768`。
新 `q1-effective.json` SHA-256 为
`2c6ee4fe78c58f6668cc25c2d2859593b114f3a145c0777bfeb194c510e7d5ec`；
222 fullname receipt SHA 仍为
`cf86495bdf8e0a035537be2f963b0915829d6c677fb57d62bb72cca4beb4c66b`。
`fd8c…` Q2 失败 run／log SHA-256 为
`3155aedf23cae85c22e6e54632b82687de045895292acc138055dbefee258e8d`、
`1c6e3e6f58e46bb5c10e0bb46240dbe3485399d54a1a3bb97cc4abacea53efc8`；
它发生在本轮源码／资源修订前，不是 `b9f2…` 的 Q2 证据。

随后 C 交付 `b9f2…` Q2 独立资源复开 `PASS`。主测试只读重算
`resources/reopen/run.json`、`unity.log`、`resource-reopen.json` SHA-256，依次为
`c6b2345c0cf5a28e4ff74f7495c2be525ae1c8d681661b5c65a13b8fedbde21f`、
`43f1d4d493beb34486de9f07defb461f245fc4e73714736577418403b9556274`、
`a64dde829df41fb933b1310e7372d7282d0dbac449efa04a4e70f5e2d112c300`。
receipt 记录 Intel Unity `2022.3.18f1 (d29bea25151d)`、PID 61836、exit 0、
23.417 秒、`VerifyResources` 真实执行、`sourceUnchangedAfter=true`、无 compiler／
Unhandled errors；资源 receipt 模式为 `Independent process reopened`，scene／prefab／
font SHA-256 分别保持
`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`、
`934e5bbec0c6cec2a16afa4778fb019a3420e3432cc0d75e07703e6f4cd60487`、
`d7d632b8d97db7a627d3883cae3058242f4b012c3082b36a916edce784800d2d`。
`SceneTemplateSettings` 副产物未再生成且既有 `ProjectSettings` 基线不变。Q2 已补给
同一 active R-TOOLS；Q3 尚待主程签收、Q4 尚未签收。当前仍只允许
`PRE-REVIEW CLEAN` 或 `PRELIMINARY FINDINGS`，不是接收，也不得给最终三态。

R-TOOLS 已在实际 turn `01a0f43a-279f-7991-8364-876fcf94273b` completed，
final `msg_0543862903425801016abd8452edd087d080b5ed743aabc23f`，唯一结果为
`PRE-REVIEW CLEAN（仅源码预审）`，没有输出正式三态 verdict。它确认当前
`b9f2…` 53／53 实际文件匹配 manifest，完整 Standards／Spec 源码预审未发现需
修正项；通过反向还原逐阶段核对 `cb3922…` → `fd8c…` → `b9f2…`，四文件占位符
规则、两行 NUnit 限定修复、Q1 两类窄例外及历史 Q2 独立重开记录均成立。
`q1.json` 实算 SHA-256 为
`f15a7325d2ff6c3a096328dbffc00b7cc244987e8aec126c77346a67b3bb3b78`；
它保留 `passed=false`，唯一失败为 Unity 原生 `SaveScene` 在四个空字段留下尾随
空格，effective receipt 与 raw 正确反绑，没有放宽产品源码或测试。

预审收尾另发现运行阶段资源身份须闭合：当前 scene／prefab 仍匹配 Q2，但
`Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` 当前实算为
8546563 bytes／
`66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb`，
mtime `2026-10-01T05:49:05+0800`，已不同于历史 Q2 receipt 中的 font SHA
`d7d632b8d97db7a627d3883cae3058242f4b012c3082b36a916edce784800d2d`；
53 个源码文件仍一致。该观察不是源码预审缺陷，当前也不归因为未知并发污染。
C／主程须在运行结束后诊断并交最终资源身份；历史 Q2 font SHA 不得冒充当前
资产身份。Q3、Q4 真实 222 XML／log／result 及最终资源身份均由 QA 签收后，才
回同一 R-TOOLS 做终审。本预审不放行完整候选。

随后主测试只读检查 `b9f2…` 的实际 Q3／Q4 文件。Q3 compile 的 `run.json`／
`unity.log` SHA-256 为
`4c540c4442a90ba9072a5846e0ea08a88bd601cfc08cfd405b276432baa1acd7`、
`37be470e7d51dff6a4872e82ac816cf2e47caeaa6cbffb1db017909a94b36861`；
Intel Unity `2022.3.18f1 (d29bea25151d)`、PID 62438、exit 0、12.140 秒，
compiler／Unhandled errors 为空且 `sourceUnchangedAfter=true`，故 Q3 运行本身
通过。Q4 首次 targeted EditMode 的 `run.json`／`unity.log`／`tests.xml`／
`xml-statistics.json` SHA-256 依次为
`5275d76ec3cb40b98164353e4813dc54f5b50bb64dee5d9fb62a068db1a1787f`、
`36e56f2da63765dfee6176a9cd92198faa7166502940e73b345be1e9b0ed31c4`、
`6e1738bc0f6def4fca4f0b57c13efd4037edd22168c63dc40bb3ab657ce00b13`、
`56c24f437f77369566f7eab4643ca3cc95c813ae114043bdd208dc262e53f7fe`。
该次 PID 62925、exit 2、174.191 秒；实际 222／期望 222，完整名称精确匹配且
missing／extra 均为空，但只有 146 pass、76 fail、0 inconclusive、0 skip；
源码未变且无 compiler／Unhandled errors。按失败首行聚类为 57 个
`NullReferenceException`、6 个 route empty、3 个 Board 缺 `CanvasRenderer`、
2 个 destroyed `Button` 未处理日志、2 个 real-board foremost-raycast 失败，以及
6 个各一例的剩余装配／资源／导航错误。故 Q4 明确 **FAIL**，完整候选仍不得
终审或接收；222 名称齐全不等于通过，也不得逐项改测试求绿。

C 用冻结的 9 个既有失败用例做只读定向诊断：8 项复现，H05 在 post-Q4 动态
atlas 状态下通过；运行前后 53 个源码和 37 个资源均 byte-identical，字体保持
冻结。根因收敛后，主程签发
`engineering-ugui-01-q4-correction.md`。首次 203 lines／13573 bytes／SHA-256
`04e06d88c41b644f1699c5f60aff297edaae6394ba1a84cf7446faa8531f7aba`
在执行前被 C 正确发现与既有 `VerifyResources` 输出路径合同冲突，现已作废；没有
据旧版运行 Unity。主程唯一补签是给 `VerifyResources` 增加修复后 Q2 和条件 final
reopen 两个精确新 evidence 输出形状，`Prepare` 与全部验收断言不变。当前唯一正式
版本为 215 lines／14364 bytes／SHA-256
`2e8615a0b38dc7e141043ee5d077d9bb5c13f85c5665206f64c1ccba9fb9bebc`，
`git diff --check` 通过，状态为 `AUTHORIZED_FOR_ASSIGNED_OWNER`；后续只认该 SHA。
唯一 owner 仍为 C；写范围严格限定 7 个源码文件和 1 个 Prefab，禁止 `.meta`、
Scene、字体、ProjectSettings 及其它路径变化。该包保留 H05 与
`VerifyUguiResources` 字体／subasset 原断言，不预先改检查口径；修复后的新 Q2
37-resource manifest 才是权威资源基线，不能复用历史 Q2 的旧字体 SHA。

执行顺序固定为 Q1 → 原生 Prefab recovery → 新 Q2 → Q3 → targeted 9 → exact
222 Q4；targeted 未达 9／9 即不跑 Q4。Q4 后若 37／37 与新 Q2 byte-identical，
直接冻结 final；若只有 dynamic font 可解释变化，最多允许一次独立复开证明稳定；
其它漂移或复开继续变化即 `BLOCKED_RESOURCE_DRIFT`。C 完成后必须先由主程审
actual patch／evidence，再由 QA 回同一 R-TOOLS 终审；此刻仍未放行。

Prefab recovery 首次因 Unity 原生 `LoadPrefabContents` 按 `RequireComponent` 自动补入
`CanvasRenderer` 而与“加载后组件数仍为 0”的旧前置冲突，C 按门禁停止。只读诊断包
`engineering-ugui-01-q4-recovery-diagnosis.md` 为 148 lines／9114 bytes／SHA-256
`895b5c9ae8446e0d5851dc59554f603402233357fd70d59588b747c6aad7c017`；诊断候选
`8a3cd796241ceb6fb953544771b8a4edb4bdf272d79a3631d6367dd7ef1b1c43` 证明磁盘序列化
`CanvasRenderer=0`、原生加载后为 1，目标 script／GUID／board 引用均正确。主程据此
最小签发 `engineering-ugui-01-q4-correction-fix-02.md`，164 lines／10795 bytes／
SHA-256 `7ef805179f0d4f7d4b7585857679f7124a7041b1f7a3b50417fc8bfbbb375f12`：只允许复用
该原生补入组件并保存，移除诊断代码。原生保存后的 Prefab 只增加该组件及引用，
其它 36 个资源不变。原 wrapper 因 45 秒时仍有 tail child 而按合同记录 blocked；
随后全部 13 个已记录 PID 自然退出。中央明确禁止重复这次非幂等 recovery，原失败
receipt 保留，只能用补充 quiescence 证据继续。

主程随后签发 `engineering-ugui-01-q4-validation-continuation.md`，152 lines／9168 bytes／
SHA-256 `fc69ddf8d6041de6bb3db75f25ee6acad3c1d93ed6e365c11a453639f6ba78b6`。
当前候选为
`0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`：资源独立复开和
compile 均 exit 0，53 个源码、37 个资源、Prefab 和字体相对新 Q2 baseline 全程
byte-identical，无 compiler／Unhandled error，所有记录进程自然退出。continuation
总 receipt SHA-256 为
`8ebe75bde864bf5b1ae48df163d133c86e04e973ea4759be4104e5ed3898a3db`；Q2 baseline
SHA-256 为
`19fbadd43e5aed4f885e09426794f3666cbed41dec14ac0be3581e210e005707`，compile run／log
SHA-256 分别为
`d7faf21de77056ac3f8cb70fb5435e7ac2dd1edeb5b83c7e263da00863937da4`、
`d426255b3339d1af7e00fce2ad0d963c984ce3acb411a002a0aa57f703d6677c`。

同一 continuation 的 targeted 9 精确执行为 6 pass／3 fail／0 skip，exit 2；run／
XML／statistics SHA-256 分别为
`a900822468943a6c66397ae923d37aaf58233991f5338a983edf08c56ee6038f`、
`590df0deec6cc046950327e5ab1ff4722cc210a0a60ec2365c402d5f1bc64ee3`、
`022b5c9256c0eca09bad0e9d148bb5e055ef57ca1653934e29ce45b16bdf4c37`。
失败固定为 B16B03 Down 后未捕获、OwnerEnded 的 real raycast 为空、Host H01 的
foremost-board 断言为 false；该 H01 断言本身不能区分空 hit 与被其它对象遮挡。按门禁
exact 222 未运行，根因仍为 `NOT_YET_DIAGNOSED`，候选不得终审。

主程的单变量图形对照包
`engineering-ugui-01-q4-raycast-graphics-fix-03.md` 为 190 lines／11721 bytes／SHA-256
`d5f1fda9bc8781cf271d723fa3cbf784cb426bcff59afadc69b74da6ac3f2787`。C 只从相同命令
移除 `-nographics`，日志确认 Apple M4 Metal 已启用，但 exact 3 仍为 0 pass／3 fail，
失败消息和堆栈与 Null-device 运行相同；源码 53、资源 37 均不变，进程全部自然退出。
run／statistics／A-B argv proof／final receipt SHA-256 分别为
`aa2be8775b4e7305bc67a1e8f2dbf5bca71f06807c40d1178a2768182fd67b2b`、
`b4cfb83f3456a2d75392d3ca754122541f0c23bcfeab364e41883cdeee149a7a`、
`65e152a8da0f34dafa7002a5f3a17361d44784fa4a00a56bc933cd7c75b0644d`、
`7d0a45b84c59bf3937d1442fc0a4d00a07b37eb4f6e56ac0dc605fccd3d95edb`。
故“Null 图形设备是公共根因”已被否定；targeted 9 graphics 与 222 均未运行。

当前主程已签发只诊断的
`engineering-ugui-01-q4-raycast-instrumentation-diag-04.md`，181 lines／11627 bytes／
SHA-256 `70ab8461a3cbc480df98b0af5606be6699d33c4014e717cfa1fe8a542002e30b`。
它只允许 C 对 exact 3 临时采集 hit 顺序、board depth／cull／mesh／GraphicRegistry、
Canvas／GraphicRaycaster 与 input module 状态，保留原断言和输入流程；执行后四个测试
文件必须恢复原哈希，同包禁止产品修复。实际一次 graphics exact-3 在产品断言前即因
临时探针对 uGUI `IndexedSet` 调用 `registry.Any(...)`，进入未实现的
`GetEnumerator()` 并在三项均抛 `NotImplementedException`；没有有效
`UGUI_RAYCAST_DIAG` snapshot，故裁定为
`NEEDS_FIX / INVALID_INSTRUMENTATION_RUNTIME`，不是产品根因或产品 verdict。
一项 compiler child 曾超过 240 秒但未被 signal，随后自然退出；四个临时测试文件已
逐字节恢复，53 source／37 resources 均未变化，最终无 Unity／compiler 遗留进程。
DIAG-04 temporary patch／XML／follow-up restore proof／uGUI `IndexedSet.cs` SHA-256
依次为
`58dfb36c7bbb917927d4bfa81fdd1e289d70b308d039cc62759d38f652f74b0d`、
`daf041f290e27464e0f5b23ad7314f9bac36d4720222fcee3997716950f4939f`、
`caccaec6022f9d43807aaf36620d53779d9a9f5bde0977ef40bf92668db16dde`、
`67e82737a015c50084766be7bffd284f6950ecfd91e395e6c67d8f64a19aea8b`。

主程已签发唯一最小纠正
`engineering-ugui-01-q4-raycast-instrumentation-diag-05.md`，148 lines／8494 bytes／
SHA-256 `d210851a7fb347e0dbedc240f1f046ab29c6192c636b7a889b1cbc5bcfec9a5f`。
它仅把两处临时 `registry.Any(...)` 改为 `registry.Contains(board)`，其余插入点、字段、
输入、yield、断言和 exact-3 不变，并静态禁止任何 registry 枚举；完成后仍须恢复相同
四文件，禁止产品修复、targeted 9 或 222。实际 C turn
`01a0f40c-69d4-7f12-90ee-50e6e9e198c3` 已封存，状态 `DIAGNOSIS_INCOMPLETE`：
Metal graphics exact-3 仍 0 pass／3 fail，Unity exit 2／35.775 秒；四份 snapshot 全部
完整，无 probe exception。按原始 fullName 对应：B16 before/after 的 board depth=10、
registry contains=true、rect/filter=true、hits=0，仍 `DIAGNOSIS_INCOMPLETE`；
OwnerEnded 的 board/renderer depth=-1，其余同样注册／坐标／filter 正常且 hits=0，分类
`DIAGNOSED_REGISTRY_OR_DEPTH_LIFECYCLE`；H01 depth=3、注册／坐标／filter 正常、
hits=0，仍 `DIAGNOSIS_INCOMPLETE`。故当前没有三项共同根因，不能从该证据直接签公共
产品修复。

Unity PID 98741 正常退出；owned compiler PID 98818 超过 360 秒等待上限，随后自然
消失，未 shutdown／signal；CLEANUP-06 为 `ALREADY_QUIESCENT`。四个临时测试文件已
精确恢复，53 source／37 resources／六棵旧 evidence root 全相等。final receipt／
diagnostic extraction／run／XML／log／restore follow-up SHA-256 依次为
`c4ad7cab81d6c95702bacb7288fac07648fba51450528b8616341ce93e34374e`、
`980664a12e7e2271a4c8915a59c2ca3470c42988ab59554b47a2aa3d048bd724`、
`d32c5e348e60f4923f7be633614d47f929c99487983f6a626d6a48bb0b0b5d1a`、
`d2fb1afbcf3b279491e8c4342567c8531d22cd38b9561d86a941c04f9d16c5f5`、
`bf1cde339ffb3d2dbeac1cd88f57afcc2cd1a4f8150043d1dca47bd483dd5021`、
`78e42f5f8183d602343e59519dfc674fc2a980872a6f256737dbe1eab5815d16`。
targeted 9、222、Q1、recovery、4181、Q5、Android、APK 与 device 均未运行；原三个
产品失败与 exact 222 未运行状态保持，待主程按 split evidence 签下一最小诊断。

主程随后签发
`engineering-ugui-01-q4-raycaster-manager-diag-07.md`，137 lines／8510 bytes／
SHA-256 `ec3d1b80f9172482c0647cd2a49dfb8fb67459338da80f81064a2596ae3d203f`。
C turn `01a0f4ca-1d98-7da1-be38-3a94e30e57f1` 取得有效
`DIAGNOSED_SPLIT_ROOTS`：B16 before/after 均 depth=10，direct raycast 的 board index=0，
但 `RaycasterManager` 为空／不含 exact raycaster，故为
`DIAGNOSED_RAYCASTER_MANAGER_REGISTRATION`；H01 depth=3 且同样 direct board index=0／
manager 缺失，分类相同；OwnerEnded 的首因仍为 board／renderer depth=-1，分类
`DIAGNOSED_GRAPHIC_DEPTH_LIFECYCLE`，manager 缺失只作 secondary。四 snapshot 完整、
无 probe exception；exact3 仍 0／3、Unity exit 2。一次 exact-pipe graceful shutdown
exit 0，无 signal／全局 shutdown；Unity 34.200 秒，相关进程在 39.039 秒内静止。四测试
文件精确恢复，53 source／37 resources／七棵旧 evidence root 全相等。

DIAG-07 final／diagnostic／run／XML SHA-256 分别为
`0542f347c0a0108b63a2545bfd5d9cc456630a15ff85d5b7d6951a6f9ef2d0d6`、
`816b5bf63c899c9548aca8908025b4fc5014f59ee1a7d35c8cca1ab3fbe09968`、
`e676a71be06aa629ffb7a99d67a17f8b243b300ce8de2911d1f39724d698a325`、
`91451b221082730f79bbfdc6a5b8d64064d9f641c6767ddeb5623e3e7d4ad74b`。
主程据此签发 FIX-08：187 lines／11520 bytes／SHA-256
`e007dbf7d1e922e199ee2128957a335c2dc020ae0d253d4f02d3a425303f98c2`，只修三个测试夹具的
raycaster lifecycle 与 OwnerEnded 三帧就绪窗口，不改产品／Prefab／资源／业务断言。
原包中后两轮 `-nographics` 与统一 non-Null renderer 门冲突；C 在启动 Unity 前正确
停止。中央裁定三轮真实 uGUI 套件统一 Metal，主程 addendum-02 为 27 lines／1397 bytes／
SHA-256 `12f37e8e9f210060629bb6f510a5f9f8f738fc1a9c6002520892d4faed995ea7`，
supersede addendum-01；C 当前按 graphics exact3 → targeted9 → exact222 串行执行，任一
失败即停。

FIX-08 实际 turn `01a0f4d5-d646-71d2-9ba0-a5004a7af1b6` 冻结新 candidate
`c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280`；graphics exact3
真实 3／3 pass、Unity exit 0、Metal，34.562 秒且相关进程在 37.372 秒内静止。三项业务
失败已关闭，但 wrapper 以 `FAILED_EXACT3_RESOURCE_PROTECTION` 停止：动态 TMP font
从 346 增至 350 characters／glyphs，只新增“现、提、交、H”，texture count 4、其它
结构字段和 GUID 不变；其余 36 resources byte-identical。font 从 8,546,563 bytes／
SHA-256 `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb`
变为 8,548,693 bytes／SHA-256
`2a186863bc12042842f77e257fc964af205a65e5de406cbdf81b22bcd007805b`；C 未人工编辑、
未回滚或接受该漂移，targeted9／222 均 `NOT_RUN`。

FIX-08 final receipt／minimal patch／final source manifest SHA-256 分别为
`aaee3cb4b6ca857734c79fc36bb089859e6a3a821c56256467a942a44f105380`、
`21574dd7ca9eaa6222b25cd7ea15eac95abf3de50f9a16c8a412a090e1e7f245`、
`b2e970c43fdd2c6c95a352bb43a0def2a1f78a927077157359d040715495944d`。
原 R-TOOLS 在 turn `01a0f4df-6a7f-75f0-b779-2dbd7257d94d` 给
`PRE-REVIEW CLEAN`，但严格只覆盖 0ff→c428 的三 fixture 增量，不是完整终审；它确认
diff／原断言／真实 EventSystem 与 lifecycle 修改成立，并实读 exact3 3／3 与 font
wrapper 红灯。相对上次 b9 源码预审，只有 45／53 可直接继承；7 个 b9→0ff 历史 source
delta 以及 RuntimeRoot Prefab／font 最终证据仍待同一 R 补审或引用精确回执。主程当前
按既有 dynamic-font final-reopen 合同准备续包：先证明合法字形／atlas 增量，再在同一
增长链运行 Metal targeted9／222，最后冻结新 37-resource identity 并原生复开；不得
回滚已证新增字形或重跑已通过 exact3。

主程签发 FIX-09 packet `engineering-ugui-01-q4-dynamic-font-finalize-fix-09.md`，
152 lines／9,568 bytes／SHA-256
`7aa0d33d6b8c067ddd558b48c1d91a01a1e52d96aa9681400a5116a6712e854c`。
C 实际 turn `01a0f4e5-57fb-75d3-8510-2748d91606d0` completed，final
`msg_0d179092dafe07fa016abdb072e1e087d0a251998fa1c48eea`，终态
`BLOCKED_EXACT222_TEST_FAILURE`。Metal targeted-9 为 9／9、Unity exit 0；run／XML
SHA-256 分别为
`0e560fbbedee2e60de1a999efbe2d3cb1c9743eff699ac7303df0e6cc4a35243`、
`e0beafe2d7687fbbbbdb06787e9afcae4ef4ddd5e81af1017a207909dd4f747b`，
editor／至静止分别 44.946／47.084 秒。随后 Metal exact-222 为 194／222、28 failed、
0 skipped、Unity exit 2；run／XML SHA-256 分别为
`80f6dff3749e0eeebe48034a33ec4a610f384301f590e077a70187bd1cb23e5b`、
`8dc95c1c316da41ca552e070c07451fb959e87b8d8d9a9ddae666815455b7bd1`，
editor／至静止分别 193.650／197.802 秒。失败观察分组为 raycast hits 空 5、
InvalidStart 实际 null 1、button raycast 空 1、单测 NRE 2、PlayerBattleRig.Step
result null 17、destroyed LocalizedTmpText `MissingReferenceException` 1、first-drag
布尔不符 1；此分组不是 root-cause verdict。

FIX-09 final receipt 69,207 bytes／SHA-256
`730d018c8661ea1fb4762c9f0617643f911474bcb72f34d4cc5d89d650c59331`；failure
inventory 27,798 bytes／SHA-256
`4a95f17c89ddb99c469bde0e4505fa66fe8bd84f5749039ccd45207f3fc923ec`。
53 source 与其余 36 resources 不变；字体当前 8,558,317 bytes／SHA-256
`f138d8c36bde60bbd8510b600c759eef9e3d0c78082e2cacb2a4818b96046eb9`，369
characters／glyphs、4 atlases，相对初始累计新增 23、本包新增 19；完整 additive audit
SHA-256 `8652891f082dd52ec56db77c07e809f80789f55b17c7214487e175e413cee278`，
引用与既有像素稳定。两次 launch 自然退出，0 signal／shutdown launch／手工 font／source
write。因 222 红灯，native final reopen 正确 `NOT_RUN`，字体和最终资源稳定性仍未接收。

同一 R-TOOLS 在 turn `01a0f4f1-08da-7453-8559-7ce15884cb80` completed，final
`msg_0543862903425801016abdaf88e66087d08e67f12e247a9258`，给累计
`PRE-REVIEW CLEAN`：补审 b9→0ff 七个历史 source delta 与真实固定
`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`，并继承 FIX-08 三 fixture
delta 与 45 个未变文件，至此静态范围覆盖 c428 相对 b9 的全部八个 source delta。
Prefab 1,060,568 bytes／SHA-256
`55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b`，仅新增
CanvasRenderer `3490000220422917678` 与 BoardFrame 引用；原 1,434 对象、顺序和 GUID
保留。本结论不因 222 红灯作废，但不是最终 verdict。

主程一次读齐 28 项后把 27 项归因于 fixture 创建 uGUI 后未经过真实帧注册就立即进入
EventSystem，另 1 项归因于产品清理路径对已销毁 Unity 对象使用 C# `?.`。FIX-10 packet
`engineering-ugui-01-q4-readiness-lifecycle-fix-10.md` 为 208 lines／13,370 bytes／
SHA-256 `7b010e32b6a2216a3003f9f819a0a919f25253af2965b29c18b3fcf2960dac0c`；
只允许十文件、共享三帧 readiness 与一行 Unity destroyed-object comparison，保留真实
GraphicRaycaster／EventSystem／业务断言。C active turn
`01a0f4fc-0620-7711-bf30-b29f33c7c848` 依序执行静态门、固定 28、exact-222 与一次 native
Metal reopen；成功只到 `READY_FOR_FINAL_R`。

LOC 新 sibling M02 packet
`engineering-loc-license-alt-m-materials-continuation-02.md` 为 205 lines／11,834 bytes／
SHA-256 `402ab00f2f1fa67e189e0a49fc42a97bfda8eec4c59d7e50777c0bd211d7b5f0`，
owner turn `01a0f4eb-5983-7ea2-b552-414d139889c6`。Linux 004／原 R 只关闭固定
YamlDotNet package-signature gate；M02 继续 source/license/closure、locks、deployment
mapping、B/L budgets 与 material seal。唯一 Mac offline runner lock-generation restore
实际 exit 1，stdout 只到 `Determining projects to restore...`；90 个互异 MSBuild
out-of-proc failure receipts 均为 named-pipe bind `Permission denied`，伴随
`CSSM_ModuleLoad`，runner lock 未生成。顶层 1 + 子节点 90 未超过包上限 96；当前准确
分类为 `BLOCKED_PROCESS_MODEL`，不是 receipt missing，也未授权原样重跑。old／005、三棵
LOC-A/M01、八个生产路径和 protected status after-equality 均 MATCH；material root 未创建。
该 owner 正封存最终 receipt；整 M 尚未交独立 R，B／L 继续关闭。

M02 随后在同一 turn completed，final
`msg_0a0722abe4bcea06016abdb1e85f7c87d081f07762daecb0d5`；最终 evidence 自校验
exit 0。`result.json` 3,647 bytes／SHA-256
`06bf3b7d330ed09e6836710c8797b2b6b198a00d752762e86562ec3d72367faa`；
`runner-lock-generation.json` 3,561 bytes／SHA-256
`73e243580aeb505e8b2c0ffeb86807ee02997470a9f13a84454498950b1a05ae`；
`evidence-files.tsv` 127 lines／18,185 bytes／SHA-256
`9065599e45aa5e29a00c1ae8d6153c9b28cfe0538f0d186220a3b9925bbc9a57`。
temp 共 2,159 files／93,114,157 bytes，manifest 284,073 bytes／SHA-256
`bc0b7752acbffa0a1de3714b2f8e5bc1cde770124927af3abc4aa317782d6c71`；
evidence 共 127 files／4,739,797 bytes。M02 网络 0 requests／0 bytes，累计仍为
M01 的 42／23,259,536；15 项 zero-action 合计 0。整 M 仍未形成 package/license/locks/
deployment manifests，后续 single-node restore 必须另签 sibling continuation，不能把此
blocked receipt 送 R 冒充整 M 验收。

主程随后签发 sibling M03
`engineering-loc-license-alt-m-materials-continuation-03.md`，269 lines／15,522 bytes／
SHA-256 `11abcdf14316ea7cb6755d7d6660e9b58baa2fe6c9e4c117350144d279bf88be`，
owner active turn `01a0f501-b9f1-7222-8fcb-525adf70c18d`。它保留 M02 失败与全部材料门，
只把 restore 拓扑改为显式单节点 MSBuild；preflight 在匹配 M01／Linux004／R／M02 seals
与固定命令模型前不得创建 M03 root 或运行 restore。M03 尚无结果，B／L 保持关闭。

M03 随后在该 turn completed，final
`msg_0a0722abe4bcea06016abdb56efce887d0a7fbebda97d12c47`，终态
`BLOCKED_MATERIAL_FREEZE`。显式单节点 MSBuild restore 本体 exit 0，1 个顶层 fixed-SDK
dotnet start、0 个 out-of-process node／named-pipe failure、M03 网络 0 requests／0 bytes；
但统一 `MSBuildProjectExtensionsPath`／`NuGetLockFilePath` 只保留 Luban.Core／Luban.Rust
dgspec 子图，生成 621-byte lock（SHA-256
`2781e0424a555858f2af4649c1d3de3a36be9d9ada3c8c1d248d93e2378cb299`）仅含
NLog 5.3.4 与 Scriban 7.2.5，缺 CommandLineParser 2.9.1、YamlDotNet 16.3.0 和其余
顶层 graph；不得冒充完整 runner closure。

M03 `result.json` 3,523 bytes／SHA-256
`97acafbe4ed1bb88b7788af9e2ab3a5304a8be00a37bbc9168bec985fb42cf6f`；
`runner-generation.json` 4,104 bytes／SHA-256
`5ccfbd6c53641a36b2b47c49ece20999cec66f7a0a081fc381241c9c17ec3c4a`；
`evidence-files.tsv` 44 lines／4,367 bytes／SHA-256
`33247b6e94c4695c00b63a3e3dad83f9b732d158e17e0e9a0aed8cf6f68281e6`。
temp 2,293 files／108,998,101 bytes，manifest SHA-256
`ac6358c79056ed0840c603979a1278553c8ba1515814fd84765193ef025a120d`；
old-root／repository equality 均 true，material root absent，tests lock／locked verification／
mapping／B/L budgets／material／整 M R 均未执行。下一 sibling 必须先固定 per-project locks 与
最终汇总口径；不能重复同 argv 或回退多节点。

主程为此提交 lock-set 设计修正
`engineering-localization-lock-set-system-design-correction.md`，285 lines／13,928 bytes／
SHA-256 `bc3b28f1f0f2000a583a373905350d9664d37c521c5e371e4d616c45a79fe080`。
原独立 R 在 turn `01a0f510-eea4-7cb3-a117-481da9e27615` completed，final
`msg_0d29bf8d726d8dc0016abdb9018b3087d0ac86ead8b02bdff8`，唯一 verdict 为
**NEEDS_FIX**。静态核实 runner 23 nodes／56 edges、tests 13 nodes／29 edges 的项目清单
准确；逐项目 lock、四次顶层 restore、single-node、既定版本／许可证及阶段边界方向成立。
只剩两项 P2，且最小修正范围仅该 addendum：

1. 现 schema 要求唯一 project row 却只有一个 `parent` 字段，不能完整表达多父引用；须分别
   定义节点集与完整 `ProjectReference` 边集，消除“另有 root row”的重复计数，并在各阶段
   比较完整集合。
2. raw `project.assets.json` 含隔离目录绝对路径，当前 canonical assets 身份不可跨目录复现；
   须冻结字段投影、路径归一化、排序／编码／哈希规则和 canonical artifact 身份，同时保留
   依赖版本、`contentHash`、项目引用及所选资产，使 generation／verification／material／
   future B 共用同一规则。

修订必须回同一原 R；在其 `ACCEPT` 前不得签 M04。R 本轮无文件修改、Git、联网、restore、
SDK 或 Unity 执行，跨聊天回执曾被自动权限检查拒绝，QA 已从原 R final 接回并按用户在
`AGENTS.md` §6 的团队通信授权路由给主程与中央；该通信事件不改变 verdict 或候选身份。

主程随后只改同一 addendum，形成 350 lines／18,765 bytes／SHA-256
`66fa832c11ac599da6f88003ad5af1146d0b2d0695d6003ab7fa472daa1ee42a` 的最小修订并退回同一
原 R。复审 turn `01a0f51d-a243-7a50-9b7c-763054fecaf6` completed，final
`msg_0d29bf8d726d8dc0016abdbaef4ff887d0936c8de816e42ccf`，唯一 verdict 仍为
**NEEDS_FIX**。node／完整 edge 模型与 23／56、13／29 已关闭；canonical 仍有两项 P2：

1. §3.1 只归一化 JSON value，遗漏 `packageFolders`、sources、project references 中以
   object key 表达的路径；须补 key normalization、归一化后 duplicate-key collision 拒绝，
   并允许目录根与配置文件的 exact match，不只接受带后续 `/` 的 prefix。
2. verification 会由固定 SDK 额外写 `restoreLockedMode: true`；当前又保留该字段并要求
   generation／verification 全对象 canonical bytes 相等，依赖完全相同时也必然失败。须先
   校验、封存各阶段要求的 restore 操作元数据，再以明确规则把该操作标记从跨阶段依赖身份
   比较中分离，不能静默删除或弱化 locked-mode 门。

本轮同样未运行 restore／SDK／Unity、联网或 Git，未改文件；最小修正仍只限同一 addendum，
须再回同一原 R，M04 继续禁止。

主程第三次只改该 addendum，现为 384 lines／21,402 bytes／SHA-256
`4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`。它把同一 tokenization
应用到 object key 与 value，定义 directory-root exact／prefix 和 config-file exact match、
tokenized key collision 拒绝；并在 canonicalization 前按阶段校验 exact JSON pointer
`/project/restore/restoreLockProperties/restoreLockedMode`，将其存在性／类型／值与 raw SHA
另存 receipt，随后只从跨阶段 dependency/asset canonical 副本删除该 exact property。fixed
positive／negative fixtures 覆盖路径 key、根精确匹配、阶段 marker、碰撞和关键依赖变化。
同一原 R 的限域复审 turn `01a0f524-3498-73e2-81dd-759bd648af3b` completed，final
`msg_0d29bf8d726d8dc0016abdbbf416a887d0b1d2a39e8b6101e5`，唯一 verdict 为
**ACCEPT（设计修正）**。R 确认 §3.1 两项 P2 均关闭、修改只限该节且固定正负 fixtures 覆盖
缺陷；本轮无写入或执行验证。该结论现在只允许主程签新的 M04 sibling，不表示 M04、材料接收、
整 M 或 B／L 已执行／已通过；后续完整材料仍须回同一原 R 给整 M verdict。

主程随后签发 `engineering-loc-license-alt-m-materials-continuation-04.md`，226 lines／14,058
bytes／SHA-256 `a359488d8d93eb7ee9df50438bf8cf0d9199473b1ce44d14a4410637209d2f10`。
它从 M01 approved material 重新创建四份 isolation，只允许四次 single-node top-level restore；
per-project default `obj`／adjacent locks、完整 graph、raw／canonical assets、locked-mode receipt、
package／license／deployment 与 future B/L budgets 都须 seal，禁止复用 M03 scratch、联网、build／
publish／test、B／L 或 Git。成功终点只为 `READY_FOR_INDEPENDENT_M_R`。

原 LOC owner continuation turn `01a0f52a-63a0-7961-bd0c-fdc900ff78b9` 已激活。其 first gate
先重算 M01／M02／M03、accepted addendum 与 R seals，并确认 23／56、13／29 graph 后才可创建
三个 create-once M04 roots。preflight 通过后，owner create-once 创建 partial roots；第一份 source
manifest 等值检查因 `awk` 把 TSV 重建为空格分隔而 byte-compare 失败。QA 与中央均要求按 §7
first-failure gate 原样封存、不得在同一 root 修 receipt 后继续；但 owner 随后仍修 TSV、补建
source copies 并创建 support source，形成额外流程违规。

该 turn 已 completed，final `msg_0a0722abe4bcea06016abdbe87fb5087d0a852b095e12ec8e1`，修正终态为
`BLOCKED_STOP_GATE_VIOLATION`；底层最早失败保持 `BLOCKED_RECEIPT_FORMAT`。复制字节后来匹配
不消除违规；当前 partial roots 必须原地保留并永不成为成功候选。owner 在第二次 STOP 后未运行
Python fixtures 或 restore，material root 未创建；network、build、publish、test、Unity、Luban、
005、Git 与产品写入均为 0。由于 STOP NOW 禁止进一步工具操作，本 sibling 没有补写 result／
manifest／文件哈希。后续只允许主程签新的 sibling／roots，且须同时引用原 receipt-format 首败
和 stop 后继续的完整事实；不得补写或复用本 M04 root。

主程已签 clean sibling `engineering-loc-license-alt-m-materials-continuation-05.md`，149 lines／
9,612 bytes／SHA-256 `a8dc95f48896345c97f5fcfda1c96bfcc21bb3d92d9ba02eb620cba90771f842`。
M05 沿用 accepted technical design 与 M04 §§1、3–8，但使用三个全新 roots；M04 temp／evidence
固定为只读失败历史，material 继续 absent，post-stop support source 禁止复用。新 standard-library
support source 从首次 invocation 即负责 graph/canonical fixtures 与 raw TSV writer，header／tab
count／row set／duplicate／missing／unexpected／MATCH 在任何 source copy、fixture 或 restore 前
校验；first failure 必须只做一次 bounded seal 后结束，不得同 root 修写继续。

原 owner M05 turn `01a0f535-caed-7d33-b7a8-40144271b7ad` 已开始。当前先重算 M04 partial roots
file／byte／ordered absolute-path hash、M05 三根 absent、M01/predecessor/addendum/R seals 与 Python
identity。该 turn 随后 completed，final
`msg_0a0722abe4bcea06016abdc17315e887d09b8c372d186ffc2f`，终态
`BLOCKED_M04_FAILURE_PRESERVATION`：M04 evidence 2 files／644,087 bytes，两个文件的相对路径、
bytes 与逐文件 SHA 全匹配，但未完整定义的 ordered absolute-path `shasum` stream 从 packet 固定
`d61c7fd8f8f2bca7632aa455901f2d51e52b2d0f13e122a3e0d07c83c9c518fd` 复算为
`251243120ca18d6be29b5882fd64d3ca23895de9870aba6c05f1374d71a6f78f`。这是 aggregate
serialization 合同缺口，不是 evidence file drift。

M05 temp／material roots 仍 absent，只创建 bounded `FAILED_EVIDENCE`。`result.json` 2,661 bytes／
SHA-256 `52fe1227405f216c35493373da6fe2db86ed8d247b475874edcd5da75d3ab4e3`；
`evidence-files.tsv` 657 bytes／SHA-256
`94aebc345f4d208257be36974c343cc1e85c35d6e82c83091c502864285dff3e`。restore／fixtures／
Unity／Luban／005、network、产品与 Git 均为 0。下一 sibling 只应把 preservation 改为
`relativePath / bytes / SHA-256` 逐文件集合比较，并保留 M04／M05 完整失败史；整 M、B／L
继续未接受。

主程已按该结论签发 `engineering-loc-license-alt-m-materials-continuation-06.md`，118 lines／
7,304 bytes／SHA-256 `fa0dfe59913b7edc57ddd0ea000eef1cddbecd7ca678feca2bd35a66c3bf62ec`。
M06 删除未定义的 absolute-path aggregate gate，改为冻结 M04 恰 2 files、M05 恰 6 files 的
relative path／bytes／content SHA exact sets；M04／M05 全部 roots 只读，三个 M06 roots 全新且只
从 M01 重放。其余 support writer、四份 isolation、四次 restore、lock/assets/material 与 first-
failure stop 合同均不变。原 owner turn `01a0f53c-e4ec-7af1-9106-69dbb4e70ce4` 已 completed，final
`msg_0a0722abe4bcea06016abdc326660887d0bb5cd4e7a306933d`，终态
`BLOCKED_PREFLIGHT`。owner 在 packet 定义的 explicit predecessor sets 之前自行增加 M02
`comparison/temp-files.tsv` bytes 必须为 287,627 的断言；实际 bytes 为 284,073，但权威内容
SHA-256 `bc0b7752acbffa0a1de3714b2f8e5bc1cde770124927af3abc4aa317782d6c71`
完全匹配，因此这是 owner-added unsupported gate，不是 predecessor drift。按 first-failure 合同，
同一 sibling 不得移除自加 gate 后继续。

M06 temp／material roots 保持 absent，只创建 bounded `FAILED_EVIDENCE`；Python runtime probe、
support Python、dotnet、network 均为 0，M04／M05 explicit sets、M01 replay、fixtures、graph、四次
restore、material 与 independent R 均未运行。`result.json` 3,246 bytes／SHA-256
`88a3ea48fdaf956b37d39db9aedb04854a2337e6f99f06a59b7aa10b475d9322`；
`evidence-files.tsv` 643 bytes／SHA-256
`9c5719f048875f39b11080a2805da04b631b0da85f7a5cc83799f4493356d9b3`。产品、Git、Unity、
Luban、005、B／L 和 predecessor 写入均为 0；整 M、B／L 继续未接受。下一 clean sibling 的
preflight 必须只执行 packet 明确列出的机器可读 gates，不得再次加入未授权的经验 bytes 断言。

主程已签并派发 clean sibling `engineering-loc-license-alt-m-materials-continuation-07.md`，157 lines／
9,912 bytes／SHA-256 `625dc3faa7b84e82f2edaa6ff86301ef6093b45fbd90f57add83ddbc6ce72e84`；
唯一 authority table `engineering-loc-license-alt-m-materials-continuation-07-preflight.tsv` 为
39 lines／9,396 bytes／SHA-256
`5e4cb6215d438add05491cc7bdf046d7a2c8d1ff8525a3f90c3888bf15a93ce8`，含 header＋38 data rows／
11 groups。签发方已用一个通用只读 checker 实跑 `PASS rows=38 groups=11 m01Files=2021`；owner 仍须
首次独立运行一次 table，禁止从文本／经验／stat 补造 expected bytes、hash、count 或 aggregate。
M02 特定 manifest 只按 SHA 比较；M 是 Assets 外纯离线材料线，Unity／MCP／Hub 进程不得成为其
全局阻断门，仍须保持自身 Unity invocation 为 0，并只写三个 M07 roots。

原 owner M07 turn `01a0f549-e1c6-70c0-b370-b024b463a780` 已 completed，final
`msg_0a0722abe4bcea06016abdc696ca8487d090c20cc8280809d3`，终态
`BLOCKED_PREFLIGHT_CHECKER`。checker 按合同仅运行一次、未修改、未重试；38 rows／11 groups／
2,021 个 M01 files 中 36 rows／9 groups 通过。最早失败 row 34：owner checker 用 `lstat` 把固定
`/opt/homebrew/bin/python3` symlink 判为 non-regular，但 table 的 `file/sha256-only` 没有授权拒绝
该 symlink。后续 row 36 又暴露签发顺序矛盾：table 要求 M07 temp absent，但完整 table 被规定在
创建 temp/checker 后运行，故该 live absent 永远失败。这两项是 checker／packet 合同错误，不是
predecessor drift。

M07 temp 只含 checker 与单次运行回执，3 files／19,102 bytes；material absent；evidence
9 files／27,748 bytes。`result.json` 3,539 bytes／SHA-256
`ebb08a7d4bf2b22f5acf04efc05ba64f6c3e632e6e4205bfa9152ab962fed5ea`；
`evidence-files.tsv` 1,050 bytes／SHA-256
`2673b85826947a7a17eab8a8ea3d71b42691de20ca8d955fbee315961b1903b6`；
`table-results.tsv` 7,593 bytes／SHA-256
`d49250c858c4810c045d6ccab77e2afc9ff6315d130faa006e34afafb708ca2d`。support Python、dotnet、
network、Unity、产品、Git 与 B／L 均为 0；M01 replay、restore、material 和 R 未运行。

中央已认可 M08 最小修法：主程发布只读 preflight helper＋authority table，签发预演与 owner 必须
使用同一冻结文件、同一 argv、同一 phase，并在任何 M08 root 创建前运行，禁止“等价脚本”。
runtime `file` 明确 follow symlink 后验证 target；`exact-file-set` 继续拒绝 symlink，避免 future hash
cycles。M07 失败原样保留，不扩工具信任、SDK 或产品，不新增设计 R；M08 尚待完整 packet 身份。

M08 经多次排队消息竞态后，中央最终冻结实际三件套：packet
`engineering-loc-license-alt-m-materials-continuation-08.md` 162 lines／10,715 bytes／SHA-256
`aa334b591ca6768fdd68ce50a7dc9ca63e535778ae17999c86d2ddd43b64d0bb`；checker
308 lines／12,393 bytes／SHA-256
`3eb16dea006dd69ff37c223e265a1955be51f9f36b3a4c83494dce8e4c2ab69c`；table
53 lines／12,946 bytes／SHA-256
`bb64603c6dad03e1e928f96cc2da40286f21ca1bf5fba0e22ef275e69c9a97bc`。真实时序须保留：初始
packet `4523bd…` 把 M06 turn 误写为 M07；owner 在该版本下唯一一次 checker 调用
`exec-31dbf858-aaec-4804-8861-e263049c4cd3` 得到 `PASS rows=52 groups=13 m01Files=2021`；QA
STOP 时三根仍 absent；中间曾出现 `ec64cdf…` 补签；最终回到中央冻结 `aa334…`。checker/table/
checked inputs 始终不变，禁止重复 checker。

owner 随后用 `os.statvfs().f_bavail * f_frsize` 正确得到 `/private/tmp` 95,846,400,000 bytes、两个
外置目标各 442,368,000,000 bytes，旧约 7.34 TB 观察标记无效；正确容量门后才合法创建空
temp/evidence roots，material 保持 absent。但 owner 生成的唯一 preflight stdout receipt 不是原工具
输出的逐字节副本：至少 row 7 的 M07 packet SHA 少一个 `d`、row 36 empty-file SHA 错误。按
first-failure gate，M08 turn `01a0f55a-992f-7e80-ba2a-b18ec9448462` 已 completed，final
`msg_0a0722abe4bcea06016abdcad387cc87d08a0fa6b7dbe271c6`，终态
`BLOCKED_RECEIPT_FORMAT`；没有改写 receipt、重跑 checker、创建 support/replay/isolation、运行
restore/network/Unity/Git/B／L，material absent。

M08 `result.json` 2,686 bytes／SHA-256
`9650a5177bd828d2a3d53a1a59d75fc5557696cf121d327c07e58053ac1706ea`；
`evidence-files.tsv` 503 bytes／SHA-256
`f23c4623989045c38022e5c6b4db1f7af606c53b1f5bf2f79f4e5890185fef43`；receipt-format failure
2,820 bytes／SHA-256 `69ce8512dfe9bcb0611d65d9f40de7d425217dbf090aa8732b13d92b71473547`。
任何较早的“继续 M08 material”排队消息均被该更晚首败取代，同一 M08 不得恢复。下一 clean
continuation 应引用真实 tool-call／exit／TOTAL verdict，不再要求人工逐字节重抄 52 行 stdout；
具体方案仍须中央／主程签发。

中央随后批准 M09 clean sibling。最终 packet
`engineering-loc-license-alt-m-materials-continuation-09.md` 为 158 lines／9,955 bytes／SHA-256
`7dd9857082f8295b6646507736288b90c8868c60f98bbd7454d16d3eb1d2d8ee`；checker 308 lines／
12,393 bytes／SHA-256 `b7839648158e37cdee9550b8a04e2e8c224930724bf0250f7d737a7158e5fa3f`；
table 65 lines／15,824 bytes／SHA-256
`953ae17ba10e00d9a9f6bcf9ff1df87cf33a2bf7a37c7fdab11b289033c20b94`。其唯一流程变化是 referenced
receipt：只写真实 tool-call／argv／exit／TOTAL／raw output reference，不复制逐行 stdout；M08 的
`BLOCKED_RECEIPT_FORMAT` 永久保留。

M09 owner turn `01a0f56d-30b4-7820-820d-a4541db00161` 已 completed，final
`msg_0a0722abe4bcea06016abdd0b6bfd487d0965c50fa0359ebd4`，唯一状态
`BLOCKED_MATERIAL_GAP`。owner 唯一 checker call
`exec-8e02a3d2-7571-4e35-907e-8c488cb1863c` exit 0，exact
`TOTAL PASS rows=64 groups=16 m01Files=2021`；referenced receipt 2,596 bytes／SHA-256
`4ca805e1b85009ce8be75dc942e5a8ba955ef0ca49dc90f0d01858c70fa13266`，且
`rowOutputCopied=false`、`checkerRerun=false`。

support 首次调用后未修改；M01 replay 2,021 files、patched source 931 files、runner graph
23 nodes／56 edges、tests graph 13 nodes／29 edges和 canonicalizer fixtures均已闭合。runner generation
与 runner verification 两次 single-node restore exit 0，23 projects全匹配；第三个 tests-generation
restore首次 exit 1，NuGet `NU1101`：闭合的 M01-only 28-file feed 缺
`System.Reflection.Metadata` 与 `xunit.abstractions`，M01 根中搜索两个 nupkg 均为0。按 first-failure
gate 未运行 tests verification／finalize／build／publish／test／Luban／005／Unity／Git／B／L，未补包、
改 feed、重试或联网；network 0 requests／0 bytes，material root absent，temp partial 保留，bounded
failure evidence已封存。

`result.json` 2,735 bytes／SHA-256
`f9a41ef6753109bb45a0323731853dd57a88dc6b777971520232a8b3f06c11cb`；
`authority/material-gap.json` 2,029 bytes／SHA-256
`52bfc6cc429a9fee8c3ba87763ff6631c07d22bfb7761331a7994876f1708372`；
`evidence-files.tsv` 489 bytes／SHA-256
`539cf3659e887784e2dc49f03bd25dc6f9c93f4fe4d7b34054c27dd77c7bc6bd`。M09 不得送原 R；下一
最小处置必须准确补闭离线 tests feed 材料并使用新 sibling/root，不得恢复同 root 或把 runner 两轮通过
外推为整 M 接收。

主程以一次只读闭包诊断把下一包固定为三项：`System.Reflection.Metadata 1.6.0`、其传递依赖
`System.Collections.Immutable 1.5.0`、`xunit.abstractions 2.0.3`。M10 packet
`engineering-loc-license-alt-m-materials-continuation-10.md` 为 236 lines／13,786 bytes／SHA-256
`cc8c023f5d18dae95f8df90d785876a3405ee1798f5f6915095931ff5c777b26`；checker/table 分别为
391 lines／16,587 bytes／SHA-256
`10ba490720ed8ddac03e9e36b4e97e68b610b44bc943b5bd036b7e7488b91149` 与
98 lines／23,812 bytes／SHA-256
`b2e58332a0868ded905e3aae516c5e97ad3faea17c225e1de3a8a1f984634b16`。网络合同仅允许固定11请求／
5 MiB，复用 M09 runner，tests generation／verification restore各一次。

M10 owner turn `01a0f58a-1f6f-7ee1-b15b-20223b8a9ffc` 已 completed，final
`msg_0a0722abe4bcea06016abdd77216d087d0b371d515126dff62`，唯一状态
`BLOCKED_ACQUISITION_DRIVER`。preflight唯一调用 exit 0、
`TOTAL PASS rows=97 groups=27 m01Files=2021`；referenced receipt 2,343 bytes／SHA-256
`7a543ec5cfc7ce151b091bc4e2646baf2730edcd014c9c39aaff72ce7eb0792b`。

第一个固定 registration 请求返回200、791 bytes、0 redirect；实际 `catalogEntry` 是 string URL，
本地 acquisition driver line237把它当对象调用 `.get`，抛 `AttributeError`。按 first-failure 未修脚本、
未重试或继续其余10请求；runner/tests restore均0，material absent，M09 evidence保持，Unity／Git／005／
B／L／build/test均未运行。`result.json` 3,201 bytes／SHA-256
`b98158f362d8ae502cdb6903ee09f1a5b595920058b2787f83e4106bae91db01`；
`authority/acquisition-driver-failure.json` 2,150 bytes／SHA-256
`249c6c6be3cf7a28c5246674398160332ead1b34d1e34f7844e72579e1c68ff9`；
`evidence-files.tsv` 504 bytes／SHA-256
`6fef57be9cb9dfe47cc2eab1b53e3e2527cbffbda8ddb0daf345cbcb9562b469`。该失败属于本地驱动解析器，
不是三包闭包或官方响应失败；下一 short delta 必须用新 sibling/root只修 string catalogEntry 解析，
保持三包、11请求与5 MiB预算，不得恢复 M10 或送 R。

主程随后签发 M11 clean sibling；packet
`engineering-loc-license-alt-m-materials-continuation-11.md` 为 130 lines／6,407 bytes／SHA-256
`88cfffa8cf0c8f29f6480501e4583eb4bab5027459f390fb506bd5431f7e2496`，preflight table为
112 lines／27,149 bytes／SHA-256
`3e51974a8237b7cc8168123fd7f5e4bd5a1d05383e0d4fbd715afcfe4819651b`。owner actual turn
`01a0f594-34c1-7161-8215-ee74b8132591` completed，final
`msg_0a0722abe4bcea06016abdda3d2ee487d0a736db36e5aa6534`，唯一状态
`BLOCKED_LICENSE_EXPRESSION`。string／object registration parser 离线 fixtures均通过并导出同一 catalog
URL，network=0；M10 `req-01` 仅只读复用。M11新网络只执行 `req-02..03`，2 requests／863,629 bytes，
合并 M10 为3 requests／864,420 bytes，host仅 `api.nuget.org`、redirect 0；包体 SHA512通过。

首个权威性缺口是 `System.Reflection.Metadata 1.6.0` 的精确 catalog `licenseExpression=null`、
`repository=null`，`.nuspec` 也没有 `<license>`／`<repository>`；唯一 license URL 指向 GitHub 可变
`master/LICENSE.TXT`，包内虽有 `LICENSE.TXT`，仍不足以证明要求的 MIT 表达式及官方源码 full commit／
path 绑定。owner按 first-failure 停止，未把“旧包现代 SPDX 字段为空”外推为无许可证，也未猜 MIT。
`req-04..11`、签名、license acquisition、local feed、runner import、两次 tests restore、material创建及
候选 build／publish／test均未运行；M09／M10保持只读，三处 material root均不存在。

M11 `result.json` 32 lines／3,471 bytes／SHA-256
`bf9c0d4b856cb8e29c2109e71296c8521f9d6d99d27603867c569ddac7d1dfc0`；
`authority/license-expression-gap.json` 85 lines／3,642 bytes／SHA-256
`8fbdb262f60b397acf2119bddda23fdf2b5fc6728d813eec2d34a2e9d7b52624`；
`evidence-files.tsv` 6 lines／500 bytes／SHA-256
`d1c8e8e83bd5c6e4061d6a96a4c604fe00c6e93cf02cdf5e9cf073093e2957b0`。sealed temp为10 files／
894,102 bytes，material absent。下一处置只能由主程基于实际 nuspec／包内 license／官方版本源声明
签权威许可证与源码身份 short delta；缓存 `req-01..03` 不重复下载，不恢复 M11、不送 R。

中央在不新增网络、提取、执行或写入的只读 ZIP 检查中补得 M11 已下载包内直接证据：固定 nupkg
852,113 bytes／SHA-256 `2497e068f6afed47c4878c9101074684b645d5baebd2d5163e5eaa99f356abf1`；
内嵌完整 `LICENSE.TXT` 1,139 bytes／SHA-256
`d7a68596ab69b06f51ca278a6545148e4269a9381c26d597c13df5d88e08cf5b`，首行为
`The MIT License (MIT)`；`THIRD-PARTY-NOTICES.TXT` 15,835 bytes／SHA-256
`7864a01e2fdef7e8fdf81b906efb1466f083206affea7ba7e6dadea429754765`。因此 M11 原 first-failure与
metadata字段缺失仍保留，但下一续作不得继续把现代 `licenseExpression` 作为唯一许可证明而漏读包内
固定字节。DLL中观察到 `release/2.1` 与40-hex源码上下文只算待核字节，尚未独立证明 repository绑定；
须区分二进制包许可证据与源码重建来源，不伪造字段或新增不必要下载。

FIX-10 实际候选为
`d776c310bced809b9a7102f5587eda5958e78b7cc027d090f1b247751042bff4`，
static source manifest 10,009 bytes／SHA-256
`977e2047c17617a603d1d01400324002e9271dd06dc2f9ea2ea3113739dcde14`，
minimal patch 29,951 bytes／SHA-256
`b3d1c62362a1a4b775e20116aedc3d5e9d3b54c02ab51a0e901a82cdc8c1fdc5`，
static proof SHA-256 `36a90a2057cf5d7f0ea79799ea14ff124244f1cd9ff2d14cf8a9490ac05eb16f`。
C turn `01a0f4fc-0620-7711-bf30-b29f33c7c848` completed，final
`msg_0d179092dafe07fa016abdb4ede6dc87d0a14680ea5e30f413`，终态
`BLOCKED_NAME_SET_DRIFT`。唯一 Metal focused launch 的 compile 通过，XML 实际 22 项／
16 passed／6 failed／0 skipped、exit 2，editor／至静止 143.329／146.079 秒；run／XML
SHA-256 分别为
`ed1c4c4a5296022b61566c630e934460fcd7b867f1bc4f1522681d28d9182989`、
`b677861dae00660e7e7cf790397f582d765750e80e3fd01977752f31cfb3ba8a`。

缺少的六个参数化 fullname 均因 C 把 literal fullnames 未转义地传给 UTF 1.1.33 正则型
`-testFilter`，不是 discovery／重命名证据；unexpected／duplicate actual 均 0。
`name-set-failure.json` 6,318 bytes／SHA-256
`e30697695e3f4abdc5b28666595c5c819bff08701a93b2b40795b8bff7a3ba97`。
22 项中六个真实失败分五组：cancellation feedback 未清 2；visible feedback revision
expected 2／actual 4 共 1；新语言弹窗按钮不是 foremost hit 1；receipt level 文案
`第1关`／`第 1 关` 不符 1；`PlayerDefaultReferenceView.Close` 对 destroyed
`LocalizedTmpText` 再次访问 1。failure inventory 10,448 bytes／SHA-256
`2c771efb38632453bb29f2c86aa05d906a990853349e8787670b6355540d271d`。

focused launch 把字体从 369 增至 393 characters／glyphs，4 atlases，当前 8,570,718
bytes／SHA-256 `293991f9f37daa10a5bd284ff5048f70f8be701bd22afe7326fd4e4138171abd`；
合法 additive chain SHA-256
`0b857a4aeaa9bb1002e501302e326cf74381a216524048052ace473b96c986a9`。
其它资源、固定 source 与旧 evidence 稳定；222／native reopen 正确未运行。final receipt
23,475 bytes／SHA-256
`3710d9cf15b7d198eb0b59e5df771cfb4c1000866a51f7450b201498532156f9`。

同一 R-TOOLS 在 turn `01a0f506-d93d-7782-a577-0ddf772c5c20` completed，final
`msg_0543862903425801016abdb559afbc87d0bffbdfc9bef0cd4c`，对 FIX-10 十文件
delta 给 `PRE-REVIEW CLEAN`：独立重建 diff 与封存 patch 逐字节一致；27 个 readiness
case、三帧／ForceUpdate／Canvas／唯一 Raycaster／正尺寸门，Host-disabled 时序、两个
固定 3-case 参数方法、79 个原测试主体、取消流程／反馈／零写断言与产品一行 Unity null
comparison 都符合静态合同。该结论只保留源码预审，不覆盖 selector defect 或六个真实失败，
最终 verdict 仍需后续 28／222／字体与 37 resources／native Metal reopen 全部完成。

主程随后签发 FIX-11 packet
`engineering-ugui-01-q4-input-lifecycle-fix-11.md`，217 lines／12,029 bytes／SHA-256
`741910eb57e2a83709149821ad4a5877f7032cb76d4e30844306a0a48ccd39b9`。它限定六文件，
分别修真实 PlayMode cancellation lifecycle、ActionAccepted 后重复 orphan `Ended`、动态 language
popup 三帧 raycast readiness、正式 `第 1 关` 字面值、两个 destroyed `LocalizedTmpText` 的
Unity 有效性判断，以及 validation harness 对固定 28／222 fullname 做 anchored literal
escaping；Canvas／ART-02 LAYOUT 明确另包。

C 实际 turn `01a0f511-5532-7bd3-8aed-832c5c8d41cf` completed，final
`msg_0d179092dafe07fa016abdbabae63887d0a19568e3011d978e`，在 static 阶段返回
`BLOCKED_STATIC_READINESS_CONTRACT`，未改文件、未运行 Unity；包 SHA 与六份 preimage 全匹配，
相关 Unity 进程为零。packet 当前把共享 `Ready()` 的每次调用都要求 target active，但启动页
测试调用无参 `Ready()` 时，战斗棋盘按产品逻辑应为 inactive。最小补签须保留无参 `Ready()`
既有断言，只让显式 popup-target overload 要求 active；两者仍共用三帧等待，六文件范围无需
扩大。其余修正方向未被否定；修订包重签前不得实施或消耗 focused／222／reopen 槽。

主程已按该 blocker 只补签同一 FIX-11：现为 227 lines／12,851 bytes／SHA-256
`753faa95d11827e2a0624fdea4dbddd565d2a731d64c3fc0cc7f79ea576a0124`。无参
`Ready()` 继续以 board rect／Canvas 做几何 readiness，但允许 startup 合法隐藏 board；显式
target overload 仅为动态 popup 传 `requireActive=true`。两种入口恰好共用一次三帧等待，禁止
互相嵌套成六帧；其它六文件、28→222→native reopen 顺序与首败停止门不变。原 C continuation
turn `01a0f525-0661-79b2-a5e5-1f7786da5d4a` completed，final
`msg_0d179092dafe07fa016abdbde7710c87d0a573920f38e2fbd9`，终态
`BLOCKED_FOCUSED28`。冻结候选／source identity 分别为
`5797520db7bccb82f97e961a838ad9206d7a8ee0cc61eb12a006ec6a7e2969d6`／
`10453c102023251159e0a390f0b1be700981e45da2da7ea3ffd0bb879ae1ee85`；minimal patch
6,167 bytes／SHA-256 `2fbef7b7c6fbba8ef3b7e1c1b0dc34f61faa96dd93fdb53c23f815fe4971ab12`，
observed source manifest 10,009 bytes／SHA-256
`6e9f7e1de86b15a7d7990c61c13d0ed4f4f4ad04335c007256e326f2c05b823c`。

唯一 Metal focused run 实际 name set 28／28、24 passed／4 failed／0 skipped，editor／至静止
146.746／149.982 秒，run／XML SHA-256 分别为
`f87881b296a9e10fa3a54e6bb01fd4decf203057d8f24c506b24f5235f49a0ac`／
`8850e4eac3266313d6b86e05832551ad0565ef8481c0e2f402927cf1891802cf`。selector、动态弹窗
真实点击、正式 `第 1 关` 和六个参数化 cases 均通过。四个剩余失败为：两个 shared tests 在
嵌套 coroutine 中 yield `EnterPlayMode` 被 EditMode framework 拒绝；WIRE01 已越过旧 revision
失败，但 line 144 expected `InvalidStart`／actual null；destroyed reference 异常移到
`PlayerDefaultReferenceView.Close` line 248 的 self `gameObject` access。failure inventory
4,734 bytes／SHA-256 `fc0e2f72ca537d40024297a391ccbea5e9d50175f47e178b6d004e898117d105`。

222 与 native reopen 按 gate 未运行。font 393→396 chars／glyphs、仍 4 atlases，最终
8,572,326 bytes／SHA-256 `707b2ddf7fa190e2e4eb133a7fcd300a1f1c5392b9a59b51891c7319be46681a`；
其余 36 resources、固定 source、旧 evidence 与保护门稳定。final receipt 32,910 bytes／
SHA-256 `de4f158586339d9f3d3ef6e675ecf26093026ec98ab26d8b9f53dcd7dfbc61e6`。
QA 已把冻结六文件 delta 交同一 R-TOOLS 做只读源码预审；该预审不覆盖四个运行失败、222、
最终资源复开或正式 verdict。

R-TOOLS turn `01a0f52e-faa5-75d0-91f5-58a783ab9dbc` completed，final
`msg_0543862903425801016abdbf41b7ac87d09f6735cd2de7d014`，结论为
`PRE-REVIEW FINDINGS` 三项。R 机械重算确认六份 pre/post、当前文件、minimal patch 和
candidate／sourceCandidate 一致，差异严格落在白名单；三项正对应四个运行失败：PlayMode
command 必须由顶层 tests 直接 yield、WIRE01 后续每个新 rig 须补既有 `Ready()`、Close 末尾
self `gameObject` 访问须加 Unity 有效性 guard。其余两种 readiness 入口各一次三帧、popup 真实
Click、正式文案和 literal selector／六参数 cases 均未发现静态问题。该预审不改变
`BLOCKED_FOCUSED28`，也不覆盖 222、最终资源／字体、native reopen 或 final R。

主程已据此签发 `engineering-ugui-01-q4-lifecycle-boundary-fix-12.md`；经 QA 机械证明 fixed 28
是 fixed 222 的 exact subset（28/222 均 unique，28-only=0、222-only=194）后，最终包为
192 lines／10,826 bytes／SHA-256
`46aa13edf4633f3077c2d51d979066dddc90367e46b9a02fead4fac269318b1a`。
只允许 1 个产品文件＋3 个测试文件：两个顶层 tests 直接包 Enter／Exit PlayMode 且共享 helper
只保留 matrices body；WIRE01 的所有真实 pointer matrices 留在同一顶层 PlayMode 区间，每个
新 rig 首次输入前走现有 `Ready()`；Close 只保护末尾 destroyed self 隐藏。C continuation turn
`01a0f532-3984-79e0-8cb3-e933441c7fdb` completed，final
`msg_0d179092dafe07fa016abdc0d5cb2887d0bd190e9038dbdf4b`，终态
`BLOCKED_EXACT4`。候选／source identity 分别为
`3d6826d2782ce87399694194d778d926cd769de111752ae8308e345cb25a1a6e`／
`da039561b0ffdc523965246562039a65d9cdccc9e88e4bc06cdc6eab8ad5e3e1`，四文件 patch
5,365 bytes／SHA-256 `5802b4b2f3af0583bb0ebc9ea7cb1343ac886b7de4760e396819c3953e17d410`。

exact4 实际 0 passed／4 failed／0 skipped，editor／至静止 84.438／87.176 秒，run／XML
SHA-256 分别为
`b7a97b9bd8a153372c3c9a8367557ed0985c91091fd4d43db8ba2249cd78d93a`／
`13900354bea366d970d46d9415b26266aa51d8053b26e9eebdef80dbbcea8083`。原 nested
`EnterPlayMode`、旧 revision 和 destroyed ReferenceView self 失败已消失；新失败为：P27 detach
清 override／读 latest 后再报告 token 的断言 false；B16 detach 后 geometry unavailable 仍调用
`CellCenter`；WIRE01 `RetryLast` 触发已释放 `PlayerBattleView` 重绑；B09 解绑访问已销毁
`CandidateBoardElement.ClearPlaybackOverride`。failure inventory 11,910 bytes／SHA-256
`7e49c983a00af6034228d245474a6e5fe5940321a3f1cb7260d712adbc9918c2`。

222／native reopen 按 gate 未运行；37 resources byte-identical，font 保持 396 chars／glyphs、
4 atlases，最终资源 SHA 不变。final receipt 26,298 bytes／SHA-256
`7727552ac143b67d939b56f4732b257fb1cf6aa73c8dd7934bc5b3149e50731f`。
QA 已把冻结四文件 delta 与 exact4 栈交同一 R-TOOLS 限域预审；它只区分 fixture 生命周期与产品
清理边界，不替代运行失败、222、reopen 或 final R。

R-TOOLS turn `01a0f539-ff82-72b1-bb20-e46d2a21482f` completed，final
`msg_0543862903425801016abdc2f1f6dc87d094539aaa1994eb83`，对 FIX-12 给
`PRE-REVIEW FINDINGS`，不改变 `BLOCKED_EXACT4`。R 独立核验 candidate／sourceCandidate、
四文件 patch、53 份 source 与 37 resources 身份一致。P27 是 test lifecycle omission：测试在
line 101 退出 PlayMode，后续 detach 依赖真实 `OnDisable`，不得为此修改产品 playback reporting；
B16 是 test order 缺陷，close→unbind 后在 line 310 重新算 `CellCenter`，最小修正为 close 前保存
release coordinate，并保持两次 Up／零 submit。WIRE01 是父子 teardown ownership 冲突：父 Host
仍订阅并在 RetryLast 时重绑同一个已永久 disposed 的 `PlayerBattleView`；现有证据不足以放松产品
`Bind` guard，须由 FIX-13 明确可重绑 Unbind 与永久 Dispose 边界，同时不得削弱 child clear
acceptance arm。B09 是真实 Unity destroyed-object cleanup gap：`PlaybackView.Unbind` 的 `?.`
不能可靠处理 destroyed object，必要产品审查边界含相邻 `BoardElement.Unbind` 与
`InputView.Unbind`，但不得跳过整段 cleanup 或改 `Controller.Dispose`。

ARCH 只读诊断 turn `01a0f53a-9355-7602-a835-c476eda5a748` 已 completed；中央要求主程结合该
诊断与上述 R 预审，签一个覆盖同根清理链的 FIX-13，导航层 `Dispose` 应落实为可重绑的
`Unbind` 而非泛化。无需新增设计 R 轮；后续实际代码仍回同一 R-TOOLS。

主程已签并派发 `engineering-ugui-01-q4-lifecycle-boundary-fix-13.md`，184 lines／12,063 bytes／
SHA-256 `572b008992034f530cceedc9cc22a6ddd40c547d95c3af575b6b575c1ec780e1`。白名单最多 8 文件：
P27/B16 完整矩阵继续留在顶层 PlayMode，B16 在 close 前缓存 release 坐标；Host 对终止态
`PlayerBattleView` fail-closed 并显示固定 `BattleViewDisposed`，保留 `Bind` disposed guard；
fake-null 链先退订 managed callbacks，再只清有效 Unity objects。导航 `Dispose=Unbind` 保持可重绑，
不得泛化。验证只允许 exact4 通过后 exact222，再 native resource reopen；任一失败立即停止。

C actual turn `01a0f545-3e7b-73f3-8e1e-e7078d3473ae` 已 completed，final
`msg_0d179092dafe07fa016abdc6b65ec487d0bc315f3ab9ec5e69`，终态 `BLOCKED_EXACT4`。候选／
sourceCandidate 分别为
`15cfc5ce2da83661fc2c3aab01d4c8d7052af3e9421541a21f96ae7d838ed16f`／
`fbc273bb02b6bd4071551ff2f291011a05cc982b45c148afe34ee42a345127a6`；实际 7 files，minimal
patch 16,431 bytes／SHA-256
`8c347a37cce6d5fd075a2173596e5fa8fdcbdb57776d94f0e7ffc1549cf9953a`，observed source manifest
10,009 bytes／SHA-256 `c4c2bca3334251337404a0f1bbd61a26034b6fba2be7b2eefa129f1ef2cc8e21`。

唯一 exact4 Metal run 实际 2 passed／2 failed／0 skipped，editor／至静止 90.673／95.653 秒；
run／XML SHA-256 分别为
`bae1ece7c32c622ec296ef474ab649c1399dd34c4075c09f4461d814587e8731`／
`4e170c3188a5ad1a367284f506a3578552385966a5e902ef278bcd8b939cd327`。B16 完整取消矩阵和
WIRE01 `battle-dispose + pending` 终止视图重试均通过。P27 在父 Playback 正停用时，
`OnDisable → Close → CancelGesture → Render → BattleLocalizedRows.Bind` 对 clone 调
`SetSiblingIndex`，产生 Unity error；B09 在 controller Dispose 链仍经 playback Finish／input
CancelGesture 进入 `CandidateBoardInputView.Render`，再由 `BattleTextLine.Bind` 访问已销毁的
`LocalizedTmpText.Unbind/Target`。

222／native reopen 按 gate 未运行；37 resources byte-identical，font 保持 396 chars／glyphs、
4 atlases、SHA-256 `707b2ddf7fa190e2e4eb133a7fcd300a1f1c5392b9a59b51891c7319be46681a`。
final receipt 38,115 bytes／SHA-256
`e12ec88ea2e718f4efc0dc66898e2663f72f1cd007f599086e44abb57f4ec646`；failure inventory
10,510 bytes／SHA-256 `fdfb57a5cea0ab7784733cc19fa8d0e6ac321346e16e311e1129a1a2994c82e0`。
主程须先做只读归因再签最小 FIX-14；不得在同 root 修补或继续 222／reopen。

ARCH FIX-14 只读诊断 turn `01a0f553-8562-7241-ba07-612deafd1bcb` 已 completed，final
`msg_0e75810e652b6a50016abdc914e91487d090c55ba67748d918`。它同时发现 FIX13 final receipt 的
`b09Evidence` prose 误写 `Passed complete`，但 actual4、coverage flag、XML、failure inventory 与
顶层状态都明确 B09 `Failed`；旧 receipt 不得覆盖修改，后续必须以 raw results 为准，不能复用为通过。

主程已签发 `engineering-ugui-01-q4-managed-lifecycle-fix-14.md`，199 lines／12,822 bytes／
SHA-256 `9ef4285b8c9a9d56bfb95542b40512cb364064c12bbe33a5e1b7d37ab3a362ba`。只允许四个
Presentation product files，test/controller/LocalizedTmpText/BattleLocalizedRows 及相邻 closed set
不得变化。P27 在 OnDisable 首步关闭结构刷新，Render 在任何 native访问前 fail-fast；B09 把纯托管
退订 seam 与仅 Unity-valid 时执行的 native cleanup 分离，保留 controller 终止顺序。验证固定
exact2→222→native reopen，任一步失败即停。

C actual turn `01a0f55f-32d8-7123-a65f-8fc66800cf5a` 已 completed，final
`msg_0d179092dafe07fa016abdcc97335c87d09be3d8b7cd054898`，终态
`BLOCKED_EXACT2_WITH_RESOURCE_DRIFT`。candidate／sourceCandidate 分别为
`f95c76420a72877ceea9465662e5532e6eaeab2438b78d3fadfdb6014098ebf9`／
`6da5acfe02d4bbdaab20df5ca916e49bfa77f67a16729d996c9290cd1d41edcb`；四文件 patch
19,420 bytes／SHA-256 `4b876195cc51ad8d289f066a5b2db297c9aead34066f7dd9f807a47a684a3d82`，
source manifest 10,009 bytes／SHA-256
`db378a9d35d5a4882dd9410244848e0d2540c9fbf0d3e3b68130d2b062ceff67`。

唯一 exact2 实际 1 passed／1 failed。P27 六种 mode 全部到达且无 hierarchy error；B09 在进入包所要
验证的 `new-page`／`host-dispose` 销毁场景前，即第一次
`PlayerBattleTestFixture.Step → Draw → UguiPointerDriver.Point → CandidateBoardElement.CellCenter`
报 `Board geometry is unavailable`，测试行 160。222／native reopen 按 stop gate 未运行；不能把
P27 单项通过外推为候选通过。

本轮字体从 396 增至 398 characters／glyphs，atlas 仍为 4；additive audit 证明新增仅为授权
`fm.battle.playback.interrupted` 简中文案的 `显` U+663E、`示` U+793A，既有 character／glyph
未变，其余 36 resources byte-identical。最终 font 8,573,334 bytes／SHA-256
`290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0`；该事实保留为
FIX14 resource drift，同时中央后续将其审定为 FIX15 一次性 prebaseline，不倒写 FIX14 成功。
final receipt 32,868 bytes／SHA-256
`c0d31a32db6ee18d60fc9d6c810e3d609dba92c8482e5eadfa0cb259ba609e66`；failure inventory
1,759 bytes／SHA-256 `41395767765eda53f2d4200f2e70c02b9fc29848212353953fa3852f7892df89`。

ARCH 后续只读诊断已由中央裁定：B09 是产品 active hierarchy 初始 native-bind 时序缺陷，不是
fixture 几何；测试、pointer 和 `CellCenter` 严格失败合同不得改变。主程据此签发
`engineering-ugui-01-q4-active-bind-fix-15.md`，183 lines／11,022 bytes／SHA-256
`b783763d60f5cffa6055044957a231dc55852caa7f914f8edf0c84707b5cd043`。只允许
`CandidateBattlePlaybackView.cs` 与 `CandidateBoardInputView.cs` 在 active Bind／Attach 时同步
`structuralRefresh`，另两 FIX14 产品文件、全部测试／controller／资源接口保持。验证仍为
exact2 → exact222 → native reopen，不增加 exact4／focused28；合法授权文案的 additive glyph
增长按冻结语义门审计，真实资源异常仍阻断。

C actual turn `01a0f571-8f91-7dd2-a767-df451b843c33` 已 completed，final
`msg_0d179092dafe07fa016abdd321219087d0bf3581ddba594fc2`，终态
`BLOCKED_EXACT222_TIMEOUT`。candidate／sourceCandidate 分别为
`1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b`／
`c45ed6d0c997867dc4bb60ac02d4f3ed832685b99149837c24c95aac7d8c2466`；实际只改上述两文件、
三个 active-state 同步点，minimal patch 1,897 bytes／SHA-256
`cfbcdaaaa7d9e6149151fe8d459ddba22e9196cda968765374518c83c7b84e57`，observed source manifest
10,009 bytes／SHA-256 `72df96a0b26a6baf92821d21765a23b109dd24ed4d4540eedbcd3f1a1b912629`。

exact2 实际 2 passed／0 failed／0 skipped，exit 0，editor／至静止 54.543／61.354 秒；P27 与 B09
`new-page`、`host-dispose` 两种销毁模式完整通过，37 resources byte-identical，字体保持
398 characters／glyphs、4 atlas、SHA-256
`290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0`。run／XML SHA-256
分别为 `f2acdbbd9801c29b7b63a268ba1d4e67a50cb8cba0777c1ce40f4f2f0baaa8d3`／
`d0fef5721fafa5dd872e37107425d5f53765f846df33e597cfeaa6aa1b4247e8`。

exact222 到 600.041 秒仍无 final XML、exitCode 为 null，故不能判通过；run／log SHA-256 分别为
`c4acc052ef5c38a3a6bdff47173fa2b8d49a6574fa17eec1d7433cf493631bf4`／
`2a8e4f0860700314f46b3a0bc398841723506cef409895c08a34fc8ebccf9a9c`。到限瞬间 owned Unity process
仍活跃；C 未发 signal、未重跑或二次修补，封存时已观察不到专属进程，但实际退出码与退出机制
未证实，不能写成正常／自然退出。该后续 absence 不洗写 original timeout，也不存在可晚收为通过的
XML。native reopen 未运行，未送 R。final receipt 100,609 bytes／
SHA-256 `b64f481353cca4b505aa02ce92fe98c431042cba4fa77a27f6a3268b8b408ae3`；空 failure inventory 仅表示
未取得测试失败项，不表示222通过。主程须先从封存日志定位最后运行 test／domain transition，再签能
回答该诊断问题的最小运行；不得原样重跑222或并发启动 Unity。

在主程确认两文件 candidate／sourceCandidate 冻结且只读诊断不与审查写冲突后，同一 R-TOOLS 已先做
有限 source／exact2 pre-review。actual turn `01a0f584-db60-7883-979c-df1d96e26d7d` completed，final
`msg_0543862903425801016abdd661937c87d0a9a8085e90b08f75`，verdict
`PRE_REVIEW_PASS`（不是整体 `ACCEPT`）。回执
`testing-ugui-fix15-source-pre-review-001.md` 为 91 lines／11,734 bytes／SHA-256
`e551522c2349c434a6418a857005a2ac5f3854f87f85ac1807a4a04027f96a41`，diff-check 通过。

R 独立确认两文件补丁只新增三处 active-state 同步，原 guards、managed/native teardown、另51 source、
1004 closed paths、tests／controller 均保持；原始 exact2 XML 确为2/2，P27 六 mode 与 B09 两分支覆盖
不靠名称外推；37 resources 与398／398／4字体 prebaseline一致。源码无纠正项。222仍为600秒无XML，
reopen仍未运行，空 failure inventory不证明通过；pre-review 不建议原样重跑222，也不解除 LAYOUT
前置。下一动作因此只需回答 timeout 的运行／诊断问题，不为当前两文件另签代码修正。

只读 timeout 诊断证明原 selector 为222个唯一 fullname，五次 PlayMode reload仅17.984秒且最后到达
H05 scene路径，但600秒证据不足以区分累计过慢、Host／场景清理、跨测试状态或 RunFinished未完成。
主程据此签发只验证冻结候选的
`engineering-ugui-01-q4-grouped-validation-fix-16.md`，190 lines／9,423 bytes／SHA-256
`201a3e27f30dca739e2b41be3db6975feb42383de0791dedc5245560730bbd7e`。产品／测试／资源零 delta；
复用 exact2 2/2，只从冻结 expected manifests机械分为 exact Host30（180秒）与排除 exact2／Host后的
Core190（420秒），三组两两互斥且 Counter并集精确222。Host失败即停；两组均过才做一次 native
reopen。每5～10秒保存进度，超时只对精确 owned PID有界处理，原单进程 timeout永久保留。

C actual turn `01a0f58e-4b80-7353-b20a-4d65e84086da` 已 completed，final
`msg_0d179092dafe07fa016abdd889b78c87d0a546c898be6850f0`，唯一状态
`BLOCKED_HOST30`。冻结53 source／37 resources与2＋30＋190 partition preflight通过，候选仍为
`1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b`，产品／测试／资源零 delta；
FIX15 exact2 2/2继续保留。Host30在66.59秒内完成，28 pass／2 fail，Unity exit 2。确定失败为
`H01_ActualPanelDetachKeepsPresentationTokenAndPauseCompletesItOnlyOnce` 与
`H04_RealBoardVictorySettlesOnceAndTheOriginalReceiptSurvivesWholeHostReconstruction`；二者均在
`CandidateBoardElement.CellCenter:101` 报 `Board geometry is unavailable`，调用经过 Host fixture
`HostPanel.Draw:251`，分别落在 H01 line 206／H04 line 247。H05 scene／resource tests 本分组通过，
但不能据此外推跨组无污染或解释原600秒 timeout。按门禁 Core190 与 native reopen 未运行，未送 final R。

正式 receipt 位于
`TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-16/UGUI-01-Q4-FIX16-C-RECEIPT.md`，
13,301 bytes／SHA-256 `2fb5a6e81594011bcf6e038fb2d969e66d4e60ccd30d49ff73d2adaf0424525e`；Host XML
29,837 bytes／SHA-256 `ad15b34cd27a47923558510bb2b073782e28bcb98d0599383643fb6120ec03e4`，run SHA-256
`878405a99326f83a009b82f0cd52d44369bdc5a321d801b1681fd84266c77061`。这两项只证明测试失败，尚未
归因为真实产品回归。主程须先由 ARCH 只读区分 Host 隐藏→显示／绑定生命周期的产品问题与 fixture
未建立有效 board geometry；归因前不得起 FIX17、修改 fixture、盲目重跑或拆分222宣称通过。

ARCH-ENG-001 actual thread `01a0f2e6-1ac3-7d10-8855-54192b9489d4`、turn
`01a0f59b-2580-7a10-a233-669cbe178c47` 已给正式只读归因：现有最强证据是 EditMode 生命周期覆盖缺口，
不是已证产品绑定缺陷，但 inactive Bind→activate→`OnEnable` 产品分支仍未被有效验证。H01/H04 都在
首次 Draw 前失败，尚未进入各自 Detach／Pause-once 或 victory／settle／reconstruction 主题；H05只排除
静态资源、场景和引用拓扑。prefab祖先链已机械确认：playback/input同挂 `BattleStatus` 且位于
`BattlePage→BattleContent` 子树。FIX15 exact2之所以通过，是其 active子树内 Bind直接执行
`BindNative`，没有覆盖 Host 的 Startup隐藏绑定后再进入 Battle。

中央据此批准、主程签发并实际下发
`engineering-ugui-01-q4-host-playmode-fix-17.md`，108 lines／6,280 bytes／SHA-256
`e02416674d34f03b2bd4d7b2c5a0089882e33b5a17a01a0dc9c7221ed43c6cdb`。唯一写域是
`FightMatchHostRoutingLifetimeTests.cs` preimage 14,051 bytes／SHA-256
`7b1319e8aea1a5d47820e311a39444b0fca0bad6bd004c704aab88026af6929f`；仅为原 H01/H04 各加完整
`EnterPlayMode`／`ExitPlayMode` 包围，原 using、语句、断言、循环、yield、fullname及测试数不变。
fixture、产品、prefab／scene／resource与 guards全冻结，禁止手调 `OnEnable`／`BindNative` 或强制激活
BattlePage。C actual turn `01a0f5a2-6e4d-76c0-82d8-0af31b44f62a` completed，final
`msg_0d179092dafe07fa016abddf0c5f7087d0bfe61067d3fe9af6`，唯一状态
`BLOCKED_CORE190_TIMEOUT`。test-only candidate为
`872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf`，validation sourceCandidate
`240311a710cdff1f235d09b1a9c15d8ffec1fe09facbcbd443a392d925cbb93b`；产品29文件 baseline
`766157a9f733c02bba4859bc7b73b517745a96787ed511a7a57aa0782d1b38db` 未变。单文件 patch
1,136 bytes／SHA-256 `be9a542cc9155b47fd36a14b60e46f091e3071d8039b1b5d356a11d57949f2d9`，
只新增四条 PlayMode envelope yield，未改 fixture／产品／原断言。

一次 Host30 为30/30、108.7336秒、exit 0；H01/H04均在真实 PlayMode越过首次 Draw并完成全部原
生命周期断言，H05继续通过。run 85,596 bytes／SHA-256
`1731c86feacb32f6afcba018ab0285b6c74394ce7be50ed4450944f0dcc2a67c`；XML 26,559 bytes／
SHA-256 `79b428cf22371302534406ea1e6634d7bd3af999685ab577e7abe8f00310986c`。该判别结果支持原 FIX16
红点为 EditMode 生命周期覆盖缺口，未触发 `BLOCKED_PRODUCT_ACTIVATION_PATH`，但不构成整体接受。

随后 Core190 在420.0346秒超时且无 XML；授权60秒自然退出等待后进程仍活，C仅在 PID、出生时间、
完整命令、executable与projectPath均精确匹配且查询成功后向本轮 root PID 发送一次 SIGTERM，取得
exit -15并清空进程树。Core run 167,975 bytes／SHA-256
`f5be04164e60248fac03d1fa0b9b226006df385e037a302fe566dce26fbe9f40`；timeout snapshot
71,421 bytes／SHA-256 `9117ca0a8839a2091b2d6213660117f0e7253abadb6a2e669eb3d38975d813e0`。
FIX15 exact2保留，产品与37 resources未变，font维持398／398／4；exact 222并集未完成，native reopen
未运行。final receipt 423 lines／45,643 bytes／SHA-256
`70bc7eb49f78d29ebc47692ac691b2f1d00772bbc1d13654bf71445c01228f07`。现有日志没有可靠 per-test
开始／结束标记，主程须先只读定位 Core190 的 test／domain停点；不得原样重跑、盲拆后宣称覆盖，或用
Host30绿测掩盖 Core timeout。若签 FIX18，证据合同必须能回答哪项／哪个域未完成，并保留跨组状态风险。

主美 ART-02 只读静态审计 turn `01a0f504-990f-7c11-be0c-5fc285dcb09d`
completed，报告 `art-ugui-prefab-layout-audit-001.md` 为 127 lines／16,320 bytes／
SHA-256 `d9209887a26e9e04e92f58a0704b7a7de91b96f962c3b45b5ca25bed91dbfcf8`，
verdict `NEEDS_FIX`。固定 Prefab／meta／scene identities 分别为
`55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b`／
`85c3eee205f94f79652eddf5d29cce3809b76ce03a0ba92311150d129409fce2`／
`0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`。
CanvasScaler 540×960／0.5、安全区大层顺序、五个主要 ScrollRect、45 个按钮的 64／72
高度、119 个诊断占位与 263-key 文案快照通过。

静态必修五项为：BattlePage 未实现 TopBar／Stage／48 状态／正方形 BoardFrame／BottomHud
五段骨架且棋盘只有 420 高、无 1:1 约束；ReferencePopup 占安全区底部 42% 且可命中，
可能覆盖棋盘下沿；普通弹窗把全屏阻断根与内容布局合一，缺居中 panel／32 边距／正文局部
滚动；RecoveryPopup 错在普通 PopupLayer 而非高优先级 SystemLayer；通用 HudLayer 80 高且
ScreenLayer 预留 88，与已接收 56 高 TopBar 压缩战斗纵向预算。主程须签必要 LAYOUT 包，
不改业务语义；该专业审查不替代代码 R 或实际 APK／触摸／安全区验收。

Scene 对 RuntimeCanvas 的 pivot／anchorMax／localScale 零 override 是确定序列化事实，但根
Screen Space Overlay Canvas 可能由 Unity 原生驱动；当前不作为静态阻断。后续固定场景实际
解析必须采集 Canvas rect、lossyScale、scaleFactor 与首帧截图。新 APK 画面还须覆盖
540×960、1080×2400、四边非零 inset、英／简中长正文／长按钮、0 占位残留、棋盘 1:1 与
命中重合、参考抽屉让位、恢复层高于普通弹窗。视觉 `NEEDS_FIX` 不阻断独立 FIX-11／M04
设计工作。

ART-02 的 F2／F4 与现行 SYS §20.1 存在真实合同冲突。中央已在实际回合
`01a0f2de-82ad-70a0-b7b3-075e56aa2437` 形成
`central-layout-hierarchy-adjudication-001.md`，69 lines／7,011 bytes／SHA-256
`d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03`，状态为
`DESIGN_DIRECTION_DECIDED / LAYOUT_IMPLEMENTATION_NOT_AUTHORIZED_BY_THIS_FILE`。
裁定 Reference 采用 Battle BottomHud 内与通常操作区互斥的替换抽屉，不覆盖棋盘或吞掉
允许的真实起笔／取消；Recovery 采用 SystemLayer 的高优先级完整界面，普通弹窗不可覆盖。
保留稳定 ViewId、业务状态机、输入／参考／播放／反馈／保存与本地化语义，不复活已被 §20
替代的历史 SYS §3，也不由本文件直接授权节点迁移。

主程须先让 SYS／ARCH 与主美形成完整 LAYOUT 设计，明确旧／新容器、序列化引用、查找／
路由／关闭／失焦／epoch／退订、响应式尺寸和有限验证入口。QA 在完整设计到齐后，才把
“中央裁定＋完整设计”一次性交同一 R-TOOLS 限域独审，不为半成品或单独裁定另加一轮。
最终仍须用实际画面／输入关闭两种尺寸、非零四侧安全区、英／简中长文案、正方形棋盘与
命中同源、抽屉不盖棋盘、正文局部滚动且动作可见、恢复层级及退出／返回、0 诊断占位，
并把 scene 序列化默认与 Canvas 原生运行后的 rect／lossyScale／scaleFactor／首帧分开举证。
FIX-11 与 LOC addendum 修订可独立继续。

主程现已提交完整 `engineering-ugui-layout-system-design-001.md`，961 lines／68,763 bytes／
SHA-256 `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce`，状态仅为
`READY_FOR_LIMITED_REVIEW`。QA 已把该设计与中央裁定 `d1a13b…` 作为一个固定候选交同一
R-TOOLS actual review turn `01a0f544-9cf7-70d1-a0dd-882c03c5bde8` 已 completed，final
`msg_0543862903425801016abdc7d141f087d09019415eb0ea9e92`，唯一 verdict **NEEDS_FIX**，
10 blockers。回执 `testing-ugui-layout-design-review-001.md` 为 161 lines／25,145 bytes／SHA-256
`ea0077d32238b0a09f898bf9a4630720cfa13daf41489f575b72a3fb17c308cc`，diff-check 通过；只新增
该回执，未运行 Unity、改候选或写 Git。

阻断项为：B01 `CandidateBoardElement : MaskableGraphic` 与同节点 Image／hud→Content 类型合同
不兼容；B02 enemy 0..2 未与 core nonnegative 合法域／首发 UI capacity 分离；B03 NormalHud 缺
完整 ScrollRect→Viewport(Image+Mask)→Content 链；B04 MainRow 漏算现有动态四动作与文本，
3 cards＋8 actions 最低 528k > 508k；B05 Reference Close 的 Header/Actions 双解释在 104k 下使正文
仅剩 8k；B06 Confirmation/Recovery 共用 RootMask 缺唯一启闭 owner，OriginalResult 固定 Recovery
会丢确认页入口；B07 可变候选／原操作按钮不能全塞 fixed Actions；B08 删除 ScreenLayer -88 后
Startup/Navigation/Result 缺 TopBar 避让；B09 长屏 safe rect 须回冻结 `(32,120,1016,2160)`；
B10 资源／测试顺序不可执行，现有 Q2 为 20 methods／32 instances 而非旧 28，WorldSpace 旧几何
不能充 Overlay CanvasScaler 证据。

中央 Reference/Recovery 层级方向、单 Canvas/EventSystem、SafeArea internal 可访问性、主几何算式、
同轴 layout 原则与 lifecycle 基本边界可保留。SYS 必须在同一冻结修订中逐项回应 R B01–B10 和
ART03 四项，不产生竞争候选；Q0 仍受当前 correction `BLOCKED_EXACT4` 阻断，不授权实施。

主美 ART03 同步专业复核已 completed，turn `01a0f53e-359c-7a23-b57b-a7bef07011e9`，报告
`art-ugui-layout-design-receipt-001.md` 7,567 bytes／SHA-256
`098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f`，verdict
`NEEDS_FIX`。四项为长屏 safe y120、NormalHud 完整滚动链、Reference Close 放入 48k Header、
以及 488k 内 3 卡＋4 action 的宽度合同。该报告已作为只读背景交 R-TOOLS；不能覆盖 R 的独立
唯一 verdict。即使 LAYOUT 设计后续通过，当前 uGUI correction baseline 仍为
`BLOCKED_EXACT4`，不自动形成实施授权。

主程随后提交第一次单一修订
`engineering-ugui-layout-system-design-correction-001.md`，作为本次有限复审输入时为
682 lines／46,746 bytes／SHA-256
`091b2e198b6674b9dceb7df27db9ed10bc405423ea45c1ee939d06aefb65c6f1`。读取优先级冻结为中央裁定
`d1a13b…` ＞ correction ＞ base design 未被明确替换条款；首次独立回执 `ea0077…` 与 ART03
`098efc…` 仅作 finding evidence。该修订在设计层面关闭 B01／02／03／05～10 及 ART N01～03：
独立 Board Graphic、敌方领域与首发 UI capacity 分离、完整 NormalHud ScrollRect 链、单一
Reference Header/Close、Host mask owner 与 OriginalResult 当前页归属、动态 choices Body scroll、
TopBar 几何、长屏 safe rect `(32,120,1016,2160)`、可执行资源／测试 DAG 与 32-instance Q2。

同一 R-TOOLS limited review turn `01a0f565-9ba6-7083-b062-a6b37b12f1ac` 已 completed，final
`msg_0543862903425801016abdcf15644087d0bdc992bbea79adc2`，唯一 verdict 仍为
**NEEDS_FIX**，且 blocker count=1。回执
`testing-ugui-layout-design-review-fix-001.md` 为 125 lines／16,266 bytes／SHA-256
`d58a4db2312a52dc3c6bda3f746b60afcc31685d60cfdb4eb2ae36de8156cc0b`，diff-check 通过；本轮仅新增
该回执，未运行 Unity 或改候选／产品／Git。

唯一 F01/B04/ART N04 是 64k×48k、最多两行的 command cell 无法容纳冻结文案：在接受下限18号、
k=1、零 padding 的最宽松条件下，当前 CJK glyph 每全宽字 advance=18，每行最多3字、两行最多
6字；`确认原保存结果` 为7字、`继续基础奖励结算` 为8字。英文 Resolve／Settle／Restart 的非空格
advance 也分别 187.1031／207.5875／212.2906，大于两行总预算128。水平滚动只解决八个 cell 的
外层可达性，不能增加单 cell 容量。下一修订只可保留252k viewport及八命令语义，按真实 EN/ZH、
最小字号、padding与正常字距增大 cell／Caption，并同步 action-group preferred width、Content width、
scroll range和八命令逐项容量表；不得缩短业务文案、缩小字体或压扁字形。其余九项不重开，Q0
仍须 FIX15 exact2→222→reopen→独立R后才可接收；LAYOUT 实施仍未授权。

主美 ART-04 已更正原专业误发的 ACCEPT，最终同样为 `NEEDS_FIX` 且只剩上述 N04T。更正文件
`art-ugui-layout-design-receipt-fix-001.md` 5,937 bytes／SHA-256
`7cc9cebcc9a499b2ee641aa2991ae4097cf581c6acb6cb23dacf20634483fab9`；不改变独立 R 的五份冻结
输入或唯一 verdict。主程已将 F01 退同一 SYS 做最小修订，完成后只能回同一 R-TOOLS limited
re-review，不重复整套设计审查。

F01 最小修订后，同一路径 correction candidate 更新为 701 lines／50,752 bytes／SHA-256
`9c3d6d60b1a9ed1743f0920cd80204fbe82f6558da35e5607e88bf88d988e540`；SYS turn/final 为
`01a0f572-138a-7b03-be3e-3a33be301d46`／
`msg_02dab7dbd2641d5a016abdd0f2283487d0b20cb12ce1273a46`。八命令 chosen widths 按真实 EN/ZH、
18号、正常 advance、左右各12k／上下各8k冻结为 184／224／200／144／152／256／248／152k；
Fixed=776k、Dynamic=832k、Content=1624k、viewport=252k、scroll range=1372k，MainRow仍72k。

同一 R-TOOLS FIX-002 turn `01a0f579-4c41-7133-a210-88c625fcc7b5` 已 completed，final
`msg_0543862903425801016abdd3afd80087d0abfcc009aa00d070`，唯一 verdict **ACCEPT**（design-only），
0 blockers。回执 `testing-ugui-layout-design-review-fix-002.md` 为 89 lines／8,751 bytes／SHA-256
`8c20bfd3a8dff10f678cdea4b821a716cfc7115d5864b866d45c087f3e499e32`，diff-check 通过。
R 逐项核八命令及 Skip 两状态，最小余量仍为正；48k 高度可容纳单行文字，首尾 cell 在滚动两端
完整可见，disabled 保留完整宽度，最小热区144k×48k。其余九项、ART N01～03、D1、26路径白名单／
第27路径 stop 与 Q0 未重开。本 ACCEPT 只关闭 LAYOUT 设计 blocker；实际 TMP／L5b／L6、资源、
Unity 与产品实现均未运行，且仍须 FIX15 → 独立 R 后由主程另签实施包。

主美 ART-05 对同一 N04T 修订也给 design-only `ACCEPT`；报告
`art-ugui-layout-design-receipt-fix-002.md` 为 20 lines／2,241 bytes／SHA-256
`0c7fa6f520e7c8db4bc4f6f61a60df32dc81f2dd983701d1885ced2188fa0c27`。实际 TMP、滚动、超宽中间
cell 的可用性仍必须由后续截图和运行门证明；LAYOUT 实施后的视觉验收回原主美接收，不再增加一轮
纯设计审批。

主程随后提交 `ANDROID-GFX-01-DESIGN-FIX-01A` 并给内部 `ACCEPT`，但不替代
独立 R。主测试已只读复核候选为 351 lines／34601 bytes／SHA-256
`4e00b6404658f7925bebe72530358bf32414fa22c2186a51d9e03003f88578bd`，
diff-check 通过；§1.1 当前唯一 uGUI 实施输入已更新为
`ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`
（435 lines／26488 bytes），旧
`a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a` 只作历史。

FIX-01A 已以 `gpt-6-astra`／`max` 退回同一 R-ANDROID，并在实际回合
`01a0f421-6151-76f2-93ac-03b6419b42d8` completed，final
`msg_06adc457b2dc221e016abd799d7a1887d0bf9fe77585f1498a`，唯一 verdict 为
**ACCEPT**。R 确认 QA／normal／fail-after-capture 三个独立 create-once 运行、
original finally 的 list→Auto restore／save／`PlatformMatches()`／两 settings
bytes＋SHA、关闭原 Unity 后独立 `2022.3.18f1` reopen、nonce／不同 PID／receipt
SHA 回链均已冻结。私有失败 probe 只在 Capture 完成后且 Sign／output／
BuildPipeline 前触发固定状态和诊断码；即使无 BuildReport 也有完整
capture／failure／restore／reopen 回链，最终 4a／4b／4c 与失败即阻断 Q7 明确。

首次已通过的两文件／空测试白名单、GLES3-only、uGUI→API24→graphics、
assert／report、Q7 真机门／`DEVICE_PENDING`、Vulkan 状态、rollback 与禁止范围
无回归。当前 Build 源文件已随上游 WIP 变化，不能采用设计中的旧观察 SHA
作为未来实施身份；graphics 实施前仍须执行 post-uGUI／post-API24 两文件真实
冻结门。真实构建入口、源码实现、三组恢复／复开证据、APK／manifest、
BlueStacks 和真机仍未验证。本轮只读，无文件／Git 写入，未运行 Unity、ADB、
设备、构建、测试、下载或安装；`ACCEPT` 只接收设计，不授权实现。

`QA-R-DESIGN-001` 已由同一 R 在回合
`01a0f314-54c6-7ef0-887f-b84f38f690f1` completed，final
`msg_0d29bf8d726d8dc0016abd378a5b1c87d08916fc514d80f11e`，唯一 verdict 为
**NEEDS_FIX**。审查绑定架构 SHA
`309762cb3c4062aab9beadbccb5e34fbc9225d53fb9339f85021e46668513325`、
系统设计 SHA
`0082a5d6eb17435047609bcf0ca8cbf079e8086052d1f19665329fee40d111d5`
和 COPY SHA
`fc2429d2cd1495a0a29c805a2ce202316a61f692a64debd7c46130b1f8c68ee5`。
首包在以下五项完成并重冻前不得交 C：

1. §18.2／§22.4 补齐 TMP Settings、运行时 SDF shader/include、依赖、目录、
   `.meta`、GUID／来源与保存复开验证；不得导入 Examples、成品文字或新包。
2. §21 在 `FinishPointer` 前识别真实 `TouchPhase.Canceled`；不能用
   `ICancelHandler` 冒充系统触摸取消，并须验证取消零提交、Ended 单次提交。
3. §22.2 把业务提示严重度与本地化解析失败分开；活动战斗继续、未知保存
   恢复和失败重试不得被全页遮罩错误封住，业务准入仍由既有 Controller 决定。
4. §21 移除 uGUI 左下局部坐标映射后的再次翻 Y，冻结与旧 `FlowPos` 合同
   一致的公式、同源绘制中心和固定物理角点验证。
5. §19／§20.3 闭合 Host、Core.Tests、Host.Tests 对本地化合同的跨程序集
   可见性，明确最小 public 入口或准确 friend 集合和承载文件；不得由 C 自行
   扩大 public 表面或用反射补洞。

R 独立复算旧测试为 153＋30＝183，四个新增 fixture 各 6 时计划 207 的数量账
成立，但没有执行新测试。最小纠正包编号为 `QA-R-DESIGN-001-FIX-01`，只修
系统设计 §18～§25 对应白名单、接口、事件／几何、本地化故障与验证映射；不改
架构／CSV 语义，不写产品代码／资源，不运行 Unity、不操作 Git。若新增测试，
须先显式重签 Counter。重冻行数、bytes、SHA 和必要差异后，由主测试送同一 R
只复核上述五项及受影响合同。

`QA-R-DESIGN-001-FIX-01` 已由同一 R 在回合
`01a0f33d-ceb5-75a0-883b-6f883845577f` completed，final
`msg_0d29bf8d726d8dc0016abd40235bc087d0bfa7a33cb878ff28`，唯一 verdict 仍为
**NEEDS_FIX**。本次绑定修订稿 1200 行、80442 bytes、SHA-256
`86bf125f6311e4ecdf5655d93914c67581b93078b30b3716b43c3e9acd5aa6f1`；
R 确认真实触摸取消、business severity／binding failure 分层、左下坐标和
internal localization IVT 四项已在设计层面关闭，仍待 C 实现和实际证据。
剩余最小问题为：

1. §18.2／§22.4 的官方 `TMP_SDF.shader` 声明
   `TextMeshPro/Mobile/Distance Field` fallback，白名单仍缺
   `TMP_SDF-Mobile.shader(.meta)`；须保留官方 GUID
   `fe393ace9b354375a9cb14cdbbc28be4` 并同步 Q2／§24.1 的两种
   `Shader.Find` 和未解析依赖检查。
2. §25.1 的 `LocalePolicyTests` 六槽误把 LOC-TOOL-01 才拥有的文件持久化、
   原子写、复读和 I/O 故障变成首包硬门；须改成首包内存语言选择／传播的
   六个真实行为，并把持久化验收归回后包，保持 183＋24＝207。

最小纠正编号升级为 `QA-R-DESIGN-001-FIX-02`；仍只改同一系统设计的必要
资源条目、Q2／§24.1 和 §25.1 六槽／后包归属，不扩首包权限，不写 Assets、
不运行 Unity、不操作 Git。重冻后由主测试送同一 R 只复核上述两项及必要
计数／权限影响；C 继续未放行。

`QA-R-DESIGN-001-FIX-02` 已由同一 R 在回合
`01a0f354-8c09-72a3-b4b0-358822ea3c01` completed，final
`msg_0d29bf8d726d8dc0016abd4620edfc87d098fdfccd157bb546`，唯一 verdict 为
**ACCEPT**。接收绑定系统设计 1233 行、83133 bytes、SHA-256
`6f7fb2ba94112bd2072602297e16ce77895cf94951edcb59f83a4800402395dd`。
R 确认 Mobile SDF 的路径、官方 GUID、源字节 SHA、复制查重、标准／Mobile
双 `Shader.Find` 与保存复开合同闭合；六个 `LocalePolicyTests` 槽只验证内存
语言策略／事件传播，持久化明确归后包；白名单相对 FIX-01 只增加 shader 与
`.meta` 两项，四个新 fixture 各 6，183＋24＝207，已关闭的四项合同未被破坏。

该 `ACCEPT` 只放行 `UGUI-01` **设计**，允许主程据最终 SHA 签发准确 C 实施
包；不是产品实现、compile、207 测试、里程碑回归、APK、设备、视觉或发行
通过。Luban／正式 catalog、YooAsset、语言持久化、API24、APK 结构、Android
设备、最终美术及旧资源删除仍各自保留。`COPY-001-AMEND-01` 未纳入本候选，
只随 LOC 后包交接。本回合 R 只读，未改文件、运行 Unity 或操作 Git。

主程随后正式签发 `docs/team/2026-09-30/engineering-ugui-01-implementation.md`：
状态 `AUTHORIZED`，306 行、16742 bytes。原 SHA-256
`5fb9ecfdf5b4afb50d0b077a2e8d4935031b97046cb33908eba95c43d7bcf82c`
因 §4／§5.1 把实际五个 asmdef 误写为“四个”而被
`UGUI-01-IMPL-AMEND-01` 取代；该版权威 SHA-256 曾为
`b472b82423b0176a38bd0704cff29b4c27ecbe8191ab4609ac202a5516e2fef3`。
勘误只改计数文字，五条 asmdef 路径、引用图、权限、行为、filter 和 207 均
不变；QA 已只读核五条路径、新身份和 `diff-check`，后续按五个 asmdef 对账。
`FIX-03` 经 R `ACCEPT` 后，主程再次重签该文档为 320 lines／17843 bytes，
当前唯一权威 SHA-256
`ddcc344a8f812ea5ef39baf812b3adb1a8a066cf5b473a233c0be0ee7d4ad5b8`，
旧 `b472…fef3` 被取代。增量只纳入 `HostAssemblyInfo.cs/.meta`、Core.Tests
asmdef 的 Host 引用权限及 Q1／rollback 合同；207／filter 不变。QA 已核新身份、
最终设计 SHA、R turn／final 与 `diff-check`，但这仍不是实现或测试通过。
该包绑定上述最终设计 SHA 与 R final。唯一实施／本机 Unity owner 为 C
`01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`；QA 已只读复核身份、
`diff-check`、白名单、Q1～Q4、单参数 15 段 filter、183＋24＝207、固定结果
路径和 `TRANSFER_PATH_NOT_AVAILABLE` 回退。该派发不是实现或测试通过。

QA 的候选接收入口为：C 必须回传实际 changed/new files、候选 SHA／逐文件
manifest、Unity 版本、进程互斥与清理状态、Q1～Q4 的准确命令／耗时／退出码，
以及 `TestArtifacts/FightMatch/UGUI-01/<candidate-sha>/editmode-target/` 下的
`tests.xml`、`unity.log`、`run.json`、源 SHA 与 XML 根统计。QA 收件后先核
白名单、filter、207 Counter、结果路径和未通过门；主程审实际 patch 后，再由
主测试调同一 R 做 `QA-R-CODE-001`。Q5、Android、APK、设备、云端重复执行和
4181 全量均不在该包授权内。

### 后续设计独审记录

`QA-R-ANDROID-API24-001` 已由同一独立 R 在回合
`01a0f382-98b0-71c2-bf81-c322885a492f` completed，final
`msg_0d29bf8d726d8dc0016abd51d0b0c887d0aad0c8ef373aacf4`，唯一 verdict 为
**ACCEPT**。接收绑定
`docs/team/2026-09-30/engineering-android-api24-001.md` 191 行、10808 bytes、
SHA-256 `5ed0c1d7eaab4d8e6ecaf7815afc30deacc9e58b05e0d6f6af38ed9aa91402f9`。
R 确认两文件白名单、唯一 minSdk 22→24 变化、构建前命名枚举断言、仅新增
`int minSdk` 报告字段、保存复开、两份新 APK manifest、失败停止及回滚合同
闭合；也确认共享 `FightMatchAndroidBuild.cs` 必须等 UGUI 稳定并审查后串行
最小合并。当前 029 只有未占用 020，一个槽不足 QA＋normal 两次新构建；若
稳定交付不能提供两个获准且不覆盖的输出路径，实施必须在构建前 `BLOCKED`。
旧 018／019、旧报告和当前设置文字均不是新 API24 证据。该 `ACCEPT` 只接收
设计，不授权实施、Unity、compile/build、设备或 Git；仍待主程另签准确 C 包。

`QA-R-DESIGN-001-FIX-03` 已由同一独立 R 在回合
`01a0f390-ef07-71b3-b632-9197fc1305e8` completed，final
`msg_0d29bf8d726d8dc0016abd5521074487d0990b62413cd8b606`，唯一 verdict 为
**ACCEPT**。旧 FIX-02 身份为 1233 lines／83133 bytes、SHA-256
`6f7fb2ba94112bd2072602297e16ce77895cf94951edcb59f83a4800402395dd`；
新设计为 1320 lines／89187 bytes、SHA-256
`887dfae28158271c2dc0387ed642e14e5d2fc52aa965b3c206da6cf5abac4263`。
R 确认净增 `HostAssemblyInfo.cs/.meta` 两路径，Host 精确两个 friend、
Presentation 原三个 friend 不变；Core.Tests 仅新增 Host 引用，不形成程序集
循环或 Player 测试依赖。internal 边界及真实 Host／Bind／diagnostic gate 测试
要求明确，四个 fixture 各 6、总 207、filter 和 Q2～Q7 预算不变，回滚范围
准确。该 `ACCEPT` 只接收 Host internal 可见性设计，供主程更新准确 C 包；
不证明实现、compile、207、COPY／LOC、APK、设备或发行通过。

`QA-R-SYS-FIX-04` 已由同一 R 在独立回合
`01a0f39f-2fdd-7812-b05e-eaf900164973` completed，final
`msg_0d29bf8d726d8dc0016abd58aef16087d090d5273f0d6913c1`，唯一 verdict 为
**ACCEPT**。输入 FIX-03 SHA-256 为
`887dfae28158271c2dc0387ed642e14e5d2fc52aa965b3c206da6cf5abac4263`；
候选为 1391 lines／95406 bytes、SHA-256
`1738be494e82fc6b8a2235b70d269e61f051e91fab3bb12ce009340ea6823d0b`。
R 确认 private `OnApplicationFocus(bool)` 无分支、单行、单次委托 production
internal handler；false 精确一次 `runtimeRoot?.PausePresentation()`，true
零动作。B16B04／P27_03 必须装配真实 Host＋root 直调同一 handler，禁止反射、
`SendMessage`、test mirror、manual Update、disable-equivalent 或 public/test-only
入口。Q1 只证结构、Q4 证 handler 行为、Q7 才证 OS→Unity 投递；pause／
lowMemory、白名单、friends、asmdef、filter、207、预算均未扩，回滚仅撤本次
handler／委托与两个 blur 分支。该 `ACCEPT` 只接收 Focus seam 设计，供主程
重签 C 包；不证明实现、compile、207 或设备通过。

`APK-STRUCTURE-001` 原设计 262 行／17532 bytes、SHA-256
`6a54ae64f6b59734799755badb8c198122b0a2bfbb5185aa530961ef8f51a9d4` 与新增
feasibility addendum 359 行／20833 bytes、SHA-256
`2e14c4473389d7a3bf0f823f03d5c13751e541aa8fcf7c55ae61bf02ff1b362b`
均已由 QA 只读核对。`/private/tmp/FightMatch-APK-STRUCT-FEAS-001/` 恰四件样片，
其 lines／bytes／SHA 均与主程申请一致。

`QA-R-APK-STRUCT-FEAS-001` 已由同一 R 在独立回合
`01a0f397-c597-7a31-abde-4383b734c25b` completed，final
`msg_0d29bf8d726d8dc0016abd57a7fd4487d085fd3c80566d2129`，唯一 verdict 为
**ACCEPT**。R 接收“原设计＋addendum”的版本锁定结构 Adapter 技术路线可进入
后续准确实施包：QA 58 images／58 assemblies／6986 definitions，normal
37／37／4771 且禁止定义 0；定义表＋nested name 判据、14 个内存异常拒绝、
首次失败记录、019 wrapper `passed=false` 与 BuildReport `Succeeded` 并存、
唯一非合同 assert 及完整身份链均经只读复算。官方公开 API 不足的历史结论
继续保留，只由精确版本 Adapter 路线补足；后续仍须 C# golden parity、同次
build 身份绑定、新双 APK、显式 asmdef／callback 权限和串行门。该 `ACCEPT`
不是旧 019 包装通过，不授权产品实现，也不证明新 APK、设备或发行通过。

`QA-R-RES-01-DESIGN` 已由同一 R 在独立回合
`01a0f3c5-e7d1-7b23-a264-28b37a5b1a4e` completed，final
`msg_0d29bf8d726d8dc0016abd64c6c08087d0a679d2096244e799`，唯一 verdict 为
**ACCEPT**。接收绑定
`docs/team/2026-09-30/engineering-yooasset-001.md` 859 lines／62080 bytes、
SHA-256 `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`。
R 核对 YooAsset 3.0.6/full commit、包元数据／依赖／Apache-2.0 许可与 Unity Git
URL；确认 pin selection、Mac package adoption、后续 Android／remote／Windows
三层门没有互相冒充。YooAsset 类型隔离在 Adapter，项目 lease／refcount／
epoch／ReleaseSet 身份与旧回调释放合同闭合；fixed bootstrap、外层 ReleaseSet
重启激活、R2/Cloudflare immutable/range、Q35 主动 cancel/release 而非
`PauseDownload()`，以及 RES-01A～E／YA-01～16 分包和回滚边界均可审。
该 `ACCEPT` 只接收设计与 3.0.6 精确探针选型；包安装、解析／import／compile、
Android、Windows、CDN、设备、发布信任与生产云操作全部仍 `NOT RUN`，须由
后续准确任务包另行授权和验证。

`QA-R-COPY-001-AMEND-02` 已由同一 R 在独立回合
`01a0f388-3c58-7680-a16a-87f5670405b5` completed，final
`msg_0d29bf8d726d8dc0016abd5407ea3887d083b99a86d431ba87`，唯一 verdict 为
**NEEDS_FIX**。固定 CSV 为 218 physical lines／51375 bytes／217 数据行与
唯一键，SHA-256
`92f7f7ce57d6cb11964d1c5571bbae2135af4fbd46cbad3b6e1d7d4f4153b65a`；
`planning.md` 为 215 lines／31032 bytes、状态 `R_REVIEWING`、SHA-256
`12125f875f2a3d4ce244740351dd30a1644d31ce3fa67d4991c598b2318f3887`。
R 确认前 172 物理行 SHA 仍为
`c4fc583344392153e97a096b68f5d740d8ea161d1bada87bae42343b20b6fb4c`，
原 171 数据行字节／顺序、9 列、唯一键、必填、severity 及双语参数均通过。
仍须最小修正三项：

1. CSV physical line 203 `fm.history.snapshot.routes` 移除 L1 不可达的
   `pendingLinkCount`，只保留 `lockedRouteCount`；line 218
   `fm.recovery.status.remaining` 及本次新增恢复映射撤下，原 171 行不改。
2. CSV line 216 `fm.save_recovery.operation_summary` 英文从内部术语
   `Operation awaiting resolution` 改为玩家文案 `Pending action`。
3. `planning.md:58` 区分标题 `fm.reference.differences.title` 与四个单数
   `fm.reference.difference.*` 行键，避免运行时命中缺失键。

最小纠正包为 `QA-R-COPY-001-AMEND-02-FIX-01`：仅写上述 CSV 和
`planning.md` 的追加区、映射、计数／哈希／状态；修后应为原 171＋新增 45＝
216 数据行／唯一键，前缀 SHA 不变，不为凑 217 补无关键。重冻两文件后由 QA
送同一 R 限定复审。该 verdict 只针对 COPY 制作稿，不证明运行时绑定、Luban、
画面、设备或正在实施的 UGUI 代码。

`QA-R-COPY-001-AMEND-02-FIX-01` 已由同一 R 在独立回合
`01a0f395-145a-79e3-ab3f-cbe5bdc90291` completed，final
`msg_0d29bf8d726d8dc0016abd55c4739487d0a77167e28bcf1f79`，唯一 verdict 为
**ACCEPT**。接收绑定 CSV 217 physical lines／50990 bytes／216 数据行与唯一键，
SHA-256 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`，
以及 `planning.md` 215 lines／31158 bytes、SHA-256
`829a13406705439436346ce53a430b31fa69be95346cc038370c49eb61c374c1`。
R 确认 F1～F3 均关闭：撤下不可达恢复键及待补路线参数，玩家文案改为
“待处理行动 / Pending action”，参考差异键映射纠正；原 171 行字节／顺序、
前缀 SHA、9 列、必填、severity、双语参数及排序保持通过。该 `ACCEPT` 只接收
COPY 制作稿及当前 UGUI／后续 LOC 交接，不证明运行时绑定、Luban 生成、画面
或设备通过。收到 verdict 后，主策仅把 `planning.md:15` 状态从 `R_REVIEWING`
改为 `ACCEPT`；当前文件 215 lines／31153 bytes、SHA-256
`cd8afca5b348b5f19d50a6d220ca857d620624e3a8825014c078cc72beb25147`。
该审后状态落盘已由 QA 核对且 `diff-check` 通过，不回溯改写 R 实质审查所绑定的
`829a…74c1`；CSV 仍为 `e546…e782f`。

`QA-R-COPY-001-AMEND-03` 已由同一 R 在独立回合
`01a0f3d2-340b-7c03-a494-ad14288b891b` completed，final
`msg_0d29bf8d726d8dc0016abd669b7cb487d0baa6097a5dec2979`，唯一 verdict 为
**NEEDS_FIX**。接收输入为 `planning.md` 345 lines／44197 bytes、SHA-256
`11d4c1891e0a21cabc1459695aac9473491e46e63de6008ce98c625cc5b30d9d`；
CSV 264 physical lines／64325 bytes／263 数据行与唯一键、SHA-256
`0228ca7ceadf70af140af7b03de498630c0dc5e6ebd30704457af1aa457bf765`；
旧 217 physical lines prefix SHA-256 仍为
`e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`；
`copy-gap-final.json` 6702 lines／285726 bytes、SHA-256
`185c37beeb867aff99ab6c43623e5c54885aa3130facb835a47df015f012ce85`。
R 确认 81 项处置完整唯一、47 个新增键与处置一致、旧 216 行字节不变、
9 列／唯一／severity／placeholder 参数及 B-DECISION-02 旧键复用通过；仍须
定点修三处：

1. CSV line 224／N-C12 `fm.input.nonnegative_integer` 须说明禁止多余前导零，
   且过长或数值超限时须减小，不能让 `01`／超限正整数在提示后再次失败。
2. CSV line 237／N-C35 `fm.save_recovery.select_candidate` 须从“已核定／verified”
   改为从当前列表选择原保存选项，严格区分观察、选择和最终核定。
3. CSV line 225／N-C13 的 `qa_notes` 只可称当前 L1 清空装备审核自然可达；
   偏好入口虽可见，但无已发布并装备物品时修改／审核不可达。

最小纠正包为 `QA-R-COPY-001-AMEND-03-FIX-01`：只改这三个新行的指定文案／
备注及 planning 对应 N-C12／N-C13／N-C35 说明／身份，保持 263 数据行、47 新键、
81 处置和旧 prefix 不变；不改 audit、产品代码、测试、资产、包或项目设置。
该 verdict 只针对制作稿语义，不证明运行时绑定、Luban、画面或设备。

`QA-R-COPY-001-AMEND-03-FIX-01` 已由同一 R 在独立回合
`01a0f3db-8a86-7e63-b77f-2f121cedbd0b` completed，final
`msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`，唯一 verdict 为
**ACCEPT**。接收绑定 `planning.md` 347 lines／44944 bytes、SHA-256
`8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`；
CSV 264 physical lines／64708 bytes／263 数据行与唯一键、SHA-256
`d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`；
旧 217 physical lines prefix SHA-256 仍为
`e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`。
R 确认 F1～F3 全部关闭：整数提示覆盖前导零与超限，保存选择不再暗示结果已
核定，偏好修改可达性备注已纠正。实际只改指定 7 个 CSV 单元格与对应 planning
交接说明；其余 44 新行、81 项处置、47 新键、263 唯一键及格式／参数校验保持
通过。该 `ACCEPT` 只接收 AMEND-03 修订制作稿及交接，不证明运行时绑定、
Luban、画面或设备通过。

## 3. 已有证据与可复用范围

| 证据 | 已成立事实 | 本轮不可替代的缺口 |
| --- | --- | --- |
| 029 `014` Compile | 原 `9d416e6` 时代合同编译通过 | 不覆盖 `125b849` 后的新 uGUI 源码 |
| 029 `015` Host | 30/30，通过且无失败／跳过 | 是旧 UI Toolkit Host 行为，不证明 Canvas/EventSystem |
| 029 `016` Full | 4041/4041，通过且无失败／跳过 | 不证明本轮新测试、双语、设备画面 |
| 029 `018` QA APK | 构建、签名与原范围检查成立 | APK 仍为旧 UI，未安装／未设备执行 |
| 029 `019` 正常 APK | Unity 构建成功；结构补证证明测试类型未打入 | 原包装验证器仍因原始字符串误报；APK 仍为旧 UI |
| D17 性能接收 | 原始全量 4181/4181；完整墙钟 492.633793 秒；本机 Release 本次低于 600 秒，经独立 R `ACCEPT` | 五分钟未达；D16 原 exit 2／两失败门保留；不覆盖未来候选、其它机器或设备 |

上述原件及摘要见：

- `docs/versioning/2026-09-30-evidence/evidence-index.json`
- `docs/system-design/2026-09-17/demo-029-mac-r1-delivery.md`
- `docs/demo/PLAY_FIRST_DEMO.md`
- `TestArtifacts/FMTestPerformance/2026-09-30/PLAN.md`
- `TestArtifacts/FMTestPerformance/2026-09-30/full06-recovery-reassessment.json`

没有代码／测试变化时不得重复 4181 套件。本轮 uGUI、绑定和 Host 发生实质
变化后，迭代阶段只跑受影响目标集；固定里程碑候选才允许一次完整回归，
并将新结果与 4181 基线按测试全名及出现次数比较。

## 4. 环境与设备实际状态

### 本机 Mac

- Unity 固定为 Intel `2022.3.18f1`；本轮主测试未启动 Unity。
- Android 支持目录实际存在：`PlaybackEngines/AndroidPlayer`，含 `SDK`、
  `NDK`、`OpenJDK`。ADB 实物为 `1.0.41 / 32.0.0`。
- 2026-09-30 23:21（Asia/Shanghai）只读 `adb devices -l` 返回空列表。
  BlueStacks 应用已安装但当前未运行；iQOO 未连接。当前设备门状态为
  `ENVIRONMENT_NOT_READY`，不是产品 verdict。
- 避免此前阻塞的 BlueStacks 原生 CUA 读取路线。候选冻结后使用 ADB 与
  包内／日志证据；需要人工视觉观察时记录明确步骤和观察，不用 CUA 长轮询。

### 既有云环境

`QA-CLOUD-ENV-001` 已由 `01a0f13c-3d87-753a-ad11-16302add9084` /
`durable` 在回合 `01a0f2e7-48a5-729c-9a6c-b46756472048` 完成，结论为
`ENVIRONMENT NOT READY FOR ANDROID ACCEPTANCE`；这是环境状态，不是产品
失败。本次未启动 Unity、未编译／测试／构建、未安装、未同步源码、未改仓库。

- 配置 draft revision 5 与 published base 均存在，但工具不能证明当前实例
  对应的 active version；仍有 draft，最新说明不能视为已发布到可复用快照。
  Unity 变量在当前实例存在，但用户名／密码元数据仍为
  `has_saved_binding:false`，未来恢复实例的许可证复用未证实。
- `/workspace/FightMatch` 工作区干净，但在无 upstream 的 `work` 分支，HEAD
  与本地 `origin/master` 缓存均为 `9d416e6`。只读远端查询显示 master 已是
  `125b849`，但本地无该对象，尚未验证 ancestry 或同步后的精确身份。
- Linux Unity `2022.3.18f1` 文件存在，Personal 只有 entitlement 与旧编译
  成功历史；当前时刻未启动 Unity 复验。Android Build Support、
  `AndroidPlayer`、Unity SDK／NDK／OpenJDK 均缺失；host 也无 `adb`、
  `sdkmanager`、`gradle`，不能承担 Android 候选。
- 资源为 5 核、33 GiB 内存、`/workspace` 约 29 GiB 可用；文本回执和可写
  结果目录可用，但 durable→local 的 APK／二进制证据回传渠道未验证。

云端进入候选验证前须另包完成：发布／应用配置并核 active version；安装准确
Android 组件；验证恢复实例的许可证；授权后同步精确候选并核 ancestry／HEAD／
干净度；固定二进制回传渠道。以上完成前，不向该环境签发 Unity 或构建运行。

中央签发的 `docs/team/2026-09-30/cloud-sync-001.md` 已执行完成：云端回合
`01a0f2f7-9625-71ba-a6d6-1adf28a53a5c` 结论 `BASELINE_SYNCED`。前置
`work@9d416e6` 干净，一次 fetch 后 `FETCH_HEAD` 精确为 `125b849`，祖先检查
exit 0，并一次切到 detached `125b849`；终态干净，原 work 引用仍在
`9d416e6`，`origin/master` 为 `125b849`。全程零 Unity／安装／测试／许可
操作。`125b849` 尚不包含本轮 `docs/team`，因此该目录缺席不作为同步失败，
终核只读根 `AGENTS.md` 与 `START_HERE.md`。

云端 **Android 构建／设备验收** 与 **Linux EditMode 差异验证** 分开。缺少
AndroidPlayer 不妨碍后者在其前置满足后使用：先由中央协调 Git，把干净的无
upstream `work` 保留并将执行状态切到精确提交；运行时先做许可证／版本／进程
preflight，再只执行主程冻结的跨平台目标测试。XML、日志和结构化摘要留在
`/workspace/TestArtifacts/FightMatch/<request-id>/`，回执给精确 SHA、XML 根
统计、失败全名和日志信号；原始文件由 durable 会话保持，并可由主测试／R 通过
该线程复查。没有验证跨 host 二进制通道前，不把云端用于 APK 或需要本地打开的
大附件交付。

`QA-CLOUD-LINUX-PREP-002` 已在回合
`01a0f2fd-e011-722a-91b8-92e61c350a23` 完成，结论
`TEMPLATE_READY / NO TEST EXECUTED`。已核 Linux 可运行的 Editor 程序集为
`FightMatch.Core.Tests`、`FightMatch.Host.Tests`、
`FightMatch.QFramework.Tests`、`FlowPuzzle.Tests`；
`FightMatch.Android.Tests` 受 `UNITY_ANDROID` 约束，不在云端范围。UTF 锁定
且缓存为 `1.1.33`；Unity 二进制 SHA-256 为
`a2c3a28ac72ecaa2f0f1cfa695d33eeca15a1caa2d75b0fffd5c70727e069247`。
未来正式包必须固定 request ID、40 位源码提交、设计文件 SHA、分号分隔完整
test fullname、预期 fullname Counter／总数、磁盘／结果上限与墙钟／进程停止
合同；命令不得带 `-quit`。XML 缺失、零测试、Counter 漂移、HEAD 漂移和结果
超限分类为 `CONTRACT_MISMATCH`，许可证／工具故障为 `ENVIRONMENT_FAIL`，
不得写成产品失败。本回合保持 Git 干净、Unity 进程为零、证据目录未变。

### 候选跨 host 传递核对（2026-10-01）

现有 Git 授权是独立接收后由中央推送 `master`；不得为了云端验证先把未接收
源码推到该分支。主测试限时只读核对当前可调用能力：线程消息／读取接口只传
文本，`attach_artifact` 只接受既有 Pull Request URL，工作树工具只管理调用
线程所在 host，当前没有把任意本机文件或归档直接上传给 durable 线程的参数。
工具清单里虽有 Google Drive 上传与 Sites 部署能力，但它们属于第三方存储／
发布路径，本任务既未授权也不用于绕开 Git 接收门；本轮没有上传、打包、建立
公共链接或暴露凭据。仅把完整源码粘进聊天也不构成可恢复、逐字节可核的候选
通道。因此当前结论为 **TRANSFER_PATH_NOT_AVAILABLE**，不能声称云端已能验证
未接收候选。

未来若出现受信文件通道，云模板应允许“基线提交＋完整不可变快照”而非只接
40 位新提交：请求须固定基线 SHA、快照 SHA-256／字节数、逐文件路径／状态／
SHA-256、零未登记文件、应用前后干净度、应用失败回滚，以及与本机候选相同的
设计和测试清单；云端应用后再生成同一 manifest 对账，任何漂移均为
`CONTRACT_MISMATCH`。通道验证前，必要回退是由唯一 C 在本机对冻结候选执行
Q1 静态、Q2 compile 与 Q3/Q4 的精确 207 目标集，并把候选 manifest、日志、
XML、计数和 SHA 交 QA／R；不重复旧测试冒充新证据。云端保持 detached
`125b849`，只在候选独立接收、中央正式推送后才可 fetch 该已接收提交并承担
后续 Linux 复核；这不会绕开代码 R 或 Git 接收门。

## 5. 本轮验收矩阵

以下是主程设计和 C 实施包必须映射的可观察场景。系统设计须为每项给出
对应旧行为、候选文件、自动化入口或设备步骤；没有明确入口的项保持受阻，
不得临时改弱断言。

### A. 范围与静态门

1. 固定候选记录提交或不可变快照哈希、配置／文本／资源版本、完整改动
   清单；白名单、`.meta`、场景、Prefab、字体、生成物与删除项全部可对账。
2. Android 玩家入口的棋盘、菜单、HUD、弹窗和 Host 使用 uGUI Canvas 与
   实际 EventSystem；运行时不再依赖 `UIDocument`、`VisualElement` 或运行时
   `PanelSettings`。Editor 工具的 UI Toolkit 保持原职责。
3. 不新增 Unity Localization、Addressables 或第二套手写 JSON 权威；Excel／
   CSV 仍是文本权威，Luban 输出、项目本地化绑定和 YooAsset 适配边界与
   版本明确。依赖／锁文件／ProjectSettings 变化须有独立受保护变更授权。
4. 所有玩家可见预制体 Text/TMP 默认值为显眼诊断占位符；运行时绑定、键、
   模板参数、字体与缺失回退可静态枚举。静态扫描只能作为前置门，不能替代
   实际画面。
5. 019 包装验证器的修正须以程序集／类型结构为准，QA 包作正对照；不得把
   已有补证改写成旧验证器曾通过，也不得放宽到“任何命中都忽略”。

### B. 真实 uGUI 输入与生命周期

1. 鼠标与单指：按下、拖线、命中、抬起、越界、取消及坐标换算与原手势
   规则一致；安全区或尺寸变化后命中位置仍一致。
2. 多指：不同 pointerId 不抢占；主指取消／离开／失焦时释放捕获，不残留
   拖线或把副指误当确认。
3. 回退／确认：合法回退只提交一次；非法回退不给业务状态副作用；重复
   事件和过期 requestId 被拒绝。
4. 页面连续进入退出至少 3 次，旧页面回调不得修改新页面；销毁、重建、
   App 失焦／恢复和语言切换不重复订阅。
5. 播放锁、动画中断与页面销毁：完成／取消／异常均恰好解绑并释放一次，
   旧回调不得解开新播放锁或重复结算。
6. 同一场景只有一个 EventSystem 且 Canvas 带 GraphicRaycaster；非交互装饰
   `Raycast Target` 关闭，透明 HUD 区不吞棋盘事件，模态遮罩必须阻断后层。
   `ReferenceInfoDrawer` 与参考线在真实拖线起笔时立即让位，不偏移端点或
   改变业务命中。

### C. 英语／简体中文与字体

1. 首次启动：简体中文系统选简中，其他系统选英语；测试夹具必须隔离首启
   状态，不污染正常玩家资料。
2. 设置中手动切换英／简中后当前页面即时刷新；关闭进程再启动仍记住选择。
3. 遍历首 Demo 玩家流程，最终画面不得残留诊断占位符、键名或未替换模板
   参数；缺键／参数不匹配时必须显式可诊断，不能静默显示另一语言硬编码。
4. 中英文动态句子、数值、物品名和按钮在目标比例下无关键截断／重叠；中文
   常用字与实际文本字形齐全，字体许可与资产引用可追溯。

主策已专业接收 `docs/team/2026-09-30/planning-localization-draft.csv`：170 个
唯一键、9 列，SHA-256
`fc2429d2cd1495a0a29c805a2ce202316a61f692a64debd7c46130b1f8c68ee5`；
中英文与 `parameters` 占位符集合一致，`severity` 为冻结四值。QA 以该身份做
制作稿键／参数／占位符静态对照和页面遍历，但它不是 Luban 运行时权威；
主程冻结正式表映射或内容变化后必须给新身份并重新对账。
主测试另以只读 CSV 解析独立复核：170/170 键唯一；`info 107`、
`warning 47`、`blocking 13`、`error 3`；每行中英占位符集合均与分号分隔的
`parameters` 完全一致，必填列无空值，发现问题数为 0。该结果仍只接收制作稿，
不证明 Luban 生成、运行时绑定或页面显示。

主美对 `ART-PROD-001` 的专业结论为 **ACCEPT**，且只覆盖中性 uGUI 灰盒
布局／资源接口。执行回合 `01a0f2ec-f1e8-7331-95d6-954dbb23c74e` 已
completed；唯一目录 `ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/`
恰有 8 个文件。`README.md` SHA-256 为
`33dea5d90064ec62ff6e015d00b157cd16c592b907e4e29f1099cd366d3346ff`，
`asset-register.json` SHA-256 为
`52eac5b72696161ae8c716468d143371f509ab654092c8e78b47e0e9c3b08b31`；
QA 已只读复核这两个哈希和文件数。三张 PNG 标称并经主美实测为
540×960、1080×2400、2400×1900；两张战斗图棋盘分别为 508／1016
正方形，长屏只增加呼吸区，总览含 Reference Drawer、保存恢复、长正文与
未来容量。该接收明确排除 9/29 被拒视觉和像素战斗 A/B/C，也不证明真实
Hierarchy、Raycast、字体、滚动、双语、安全区或设备效果；这些门保持开启。

### D. 正常玩法、保存与重开

1. 地图 → 队伍 → 背包 → 入场 → 战斗 → 真实结算 → 返回地图 → 重打的
   正常链路完整，状态来自既有 Application／保存语义，不由 UI 复制裁定。
2. 胜利结算只提交一次；Restart、Exit、非终局保存、同资料重开与同 Attempt
   续行动按既有 P1/P2/P3 契约恢复。
3. 关闭并重开实际 App 后页面路由、队伍／背包、战斗进度、结算收据、语言
   选择和资源引用保持；不通过注入胜利、HP、随机状态或手改资料制造通过。
4. 当前 L1 不可合法覆盖的补线／全倒免费补线等场景继续记 `NOT VERIFIED`，
   除非正式内容和入口自然可达。

### E. Android 构建、设备与视觉

1. 先产隔离 QA APK，再产正常 Demo APK；记录源码／资源／配置／APK SHA、
   包名、版本、证书、ABI、IL2CPP、Min/Target SDK 与构建报告。旧 018／019
   只作比较，不安装为新候选。
2. 已批准首发最低 Android 7 / API 24，但当前历史构建仍为 Min 22。若本轮
   实施包未明确授权 ProjectSettings 修订，则本轮 Demo 可标技术验证候选，
   不得标 Google Play 发行就绪；中央／主程须另签受保护变更包后补门。
3. BlueStacks 仅作中间触摸／流程／ADB 日志验证；其 Android 13/API 33/
   ARM64 历史事实需重新实读，不能替 iQOO 真机门。
4. iQOO 最终门须实读 serial、Android/API、ABI、安装结果；覆盖点击热区、
   拖线／取消／多指、中文、英文、遮挡层级、安全区、常见横纵比、正常流程、
   进程关闭重开与保存恢复。设备缺席时保持 `DEVICE_PENDING`。
5. 每次设备执行前记录既存安装／玩家数据；QA 使用隔离数据根，正常包不得
   清理或污染用户真实资料。结束时记录是否卸载、保留或恢复，禁止静默清除。
6. 画面基准至少覆盖 `540×960`、`1080×2400` 和带非零四边 inset 的模拟
   安全区，再以真机截图关闭设备门。棋盘须保持正方形且显示路线与实际命中
   一致；普通热区至少 `48×48`、相邻净距至少 8。英语从 1.35× 起始留白
   仍须用实际 COPY-001 文案测量，不得只按估算宣称通过。
7. 当前只允许中性灰盒用于结构装配；2026-09-29 视觉已被用户否决，像素
   战斗 A/B/C 均未采用。任何被拒／pending 候选进入 Prefab、场景或 APK 都是
   阻断性范围缺陷。主美专业视觉接收和 QA 真机通过分别记录。

## 6. 分阶段执行与预算

所有 Unity／构建运行均由 C 执行；QA 先核无其它 Unity 占用项目，再签准确
请求。一次运行回答一个明确问题，超过上限或出现首个确定性失败时安全收尾，
不自动重试、不改参数、不追加槽。

| 门 | 前置与执行 | 次数／上限 | 结果用途 |
| --- | --- | --- | --- |
| Q0 设计审查 | 主程提交一个自洽、可编译、可回滚的冻结实施包；R 只读比较决定、范围、映射和验证。无需等不属于本包的后续工具／发行包最终冻结 | 每个设计候选 1 次；`NEEDS_FIX` 后只审最小修订 | R `ACCEPT` 只放行该包；未覆盖产品门继续保留 |
| Q1 静态范围 | C 完成源码；核状态、文件清单、diff check、资源／meta／依赖保护与占位符扫描 | 每个候选 1 次；目标 5 分钟，上限 10 分钟 | 不启动 Unity 的第一门 |
| Q2 Compile | 精确 Unity `2022.3.18f1` batch compile；前后核进程与输入身份 | 冻结候选 1 次；目标 2 分钟，上限 5 分钟 | 编译失败即停，不进测试 |
| Q3 目标测试 | 系统设计冻结的 uGUI EventSystem、Host、路由、本地化、保存目标集；保留真实事件。跨平台 EditMode 可在云端 Linux 承担，Android 专用入口留本机 | 修正迭代每个候选最多 1 次；目标 5 分钟，上限 10 分钟 | 定位受影响行为，不代替里程碑回归 |
| Q4 里程碑全量 | 仅在源码／测试稳定且 Q1～Q3 通过后，无过滤 EditMode；按 fullname Counter 对比原 4181 与新增项 | 本轮固定里程碑最多 1 次；目标 10 分钟，上限 15 分钟 | 新候选整体回归；旧 D17 不重跑作重复证据 |
| Q5 Android 构建 | 先 QA 后正常包，修正后的结构验证器必须两包对照 | 每包最多 1 次；单包目标 6 分钟，上限 10 分钟 | 生成设备候选和发布结构证据 |
| Q6 BlueStacks | ADB 中间门；先实读设备，安装隔离 QA，再按包执行自动阶段与限定人工观察 | 1 个候选；目标 30 分钟，上限 45 分钟 | 仅中间证据，不替真机 |
| Q7 iQOO | ADB 真机；正常包＋必要 QA 包，M/P/双语/视觉/冷启动 | 1 个候选；目标 45 分钟，上限 75 分钟 | 首 Demo 设备门；未连接则待验 |
| Q8 代码／证据 R | R 只读检查实际 patch、Q1～Q7 适用证据和未验证项；不跑 Unity | 每个最终候选 1 次 | 唯一 `ACCEPT` 才能向中央申请接收 |

Q4 只有实质源码变化且 Q3 已通过时才执行；若候选在 Q3/Q5/Q6/Q7 失败，
对应负责人先签最小纠正，重跑只覆盖缺陷影响与失效证据，不机械重跑 Q4。
任何环境故障不授修改产品代码；任何产品断言失败不以换机器消除。

## 7. 测试申请与执行回执合同

主程交 QA 的每个候选必须包含：任务 ID、固定源码／配置／资源身份、具体
变更、设计版本、受影响旧证据、准确目标测试过滤、预计新增用例、构建入口、
允许结果路径和回滚边界，并列出后续包与未覆盖产品门。一个首包只要自身
契约、白名单、编译和回滚完整即可送 R；不因 Luban／YooAsset／发行包尚未
最终冻结而阻塞独立 uGUI 结构包。缺少本包必要输入则退回补齐，不启动 Unity。

C／云／设备执行者回执必须包含：准确 task、thread／host／turn；Unity、SDK、
ADB、设备或云环境版本；输入身份；实际命令与运行范围；exit code；用例总数、
失败／跳过和耗时；APK／日志／XML／截图路径及 SHA；是否改过输入／断言／
设置；清理状态。日志存在不等于通过。

缺陷回执使用以下分类：

- `PRODUCT_DEFECT`：可在固定候选复现，回主程最小纠正。
- `DESIGN_GAP`：验收与已定方案无法同时满足，回相应负责人；改变用户决定
  由中央处理。
- `CONTENT_OR_COPY`：规则／文字／参数问题，回主策。
- `ART_OR_LAYOUT`：资产、字形、排版、遮挡或规格问题，回主美。
- `ENVIRONMENT_FAILURE`：许可、组件、网络、ADB、设备、磁盘或执行器故障，
  保留产品项未验证。
- `NOT_VERIFIED`：设备、自然入口或证据缺失，不能升级为通过。

中央已明确后续优先序：FIX15／独立 R 与 LAYOUT 实施验证闭合后，优先生成同源 API24／GLES3 的
新 uGUI normal APK 做内部试玩；LOC／RES 按真实依赖继续，但未来 CDN／账号发行项不阻塞首个可运行
候选。QA 已据现有 PQA／Q6／Q7 合同准备
`testing-internal-playtest-l1-001.md`，65 lines／5,341 bytes／SHA-256
`c2421e5a355db8e35fc24a5aa715c77700a45c26c5fb2ba424ec460a66c2c5a6`，diff-check 通过，状态
`PREPARED_NOT_AUTHORIZED_TO_RUN`。最短链为普通图标 cold→空 QA 资料建档→双语往返→自然入 L1→
真实触摸→Home/Warm→活动局 force-stop＋图标冷启恢复→人工按默认参考完成→先保存后一次结算
20 XP／2 锡片→结算后图标冷启无重复入账→Back／退出应用取消与确认；证据绑定候选／APK／证书／
设备身份、PID／Activity／logcat／exit-info、GLES3 无 override、录屏／截图和业务前后事实。即使全过，
唯一状态也只是 `INTERNAL_PLAYTEST_CANDIDATE_READY`，不等于 Demo 最终验收；BlueStacks 不替代真机。

## 8. 当前状态、风险与下一步

- `testing.md`、证据复用、验收矩阵、预算与回传合同已建立。
- 原 R 会话真实可达，`QA-R-READY-001` 已 completed/READY；首轮
  `QA-R-DESIGN-001` 与 `FIX-01` 均为 `NEEDS_FIX`，`FIX-02` 已 completed/
  `ACCEPT`；后续 `FIX-03` Host seam 与 `FIX-04` Focus seam 也分别获同一 R
  独立 `ACCEPT`。主程已将正式 C 包更新到 FIX-03 身份并发送唯一 C，FIX-04
  仍须重签进准确 C 包。COPY 216 键修订、API24 设计与 APK-STRUCT 版本锁定
  可行性路线、RES-01 设计均有独立 R `ACCEPT`，但各自只关闭对应设计／
  制作稿门。COPY AMEND-03 经初审 `NEEDS_FIX` 后已由 FIX-01 独立
  `ACCEPT`，81 项处置／47 新键和三处修订闭合。所有执行与产品门仍按
  Q1～Q8 保持开启，当前尚未收到完整实现候选或新 Unity 证据。
- 云端已同步到干净 detached `125b849`；配置 active／许可证恢复仍未证、
  Android 组件缺失、二进制回传未证。它可继续准备 Linux EditMode 路径，
  但暂不能承接 Android 候选；缺项分类为环境未就绪而非产品失败。
- 本机 Android 工具已在，但 ADB 当前无设备；BlueStacks 未运行，iQOO 未连。
  设备执行因此保持待验，不阻塞架构／系统设计、R 设计审查与源码准备。
- 主要交付风险：Luban／YooAsset 尚无已证项目接入版本；API 24 尚未实施；
  实际设备缺席；019 验证器待最小修正；本地化动态字形和安全区只能由新画面
  证据关闭；最终视觉尚未选定，当前只能用明确可替换的中性灰盒。以上不允许
  靠旧 APK、旧全量绿测或主美灰盒专业接收消除。

当前下一步按依赖顺序是：接主程对 FIX17 Core190 timeout 的只读 test／domain停点归因；在有可审计
归因及有界进度证据合同前，不原样重跑 Core190、不拆组外推222。Host30 30/30与FIX15 exact2保持，
但仍须完成 Core覆盖、exact222并集证明与 native reopen；形成正确 PR/head后，代码终审改接 GitHub
PR Code Review，QA归并问题并跟踪修复，不再调 Codex R-TOOLS代码审查。
LAYOUT FIX-002 的有限独审已 `ACCEPT`，仅关闭设计 blocker；实施仍须等主程在 UGUI
correction 独立接收后另签。LOC M11 保持 `BLOCKED_LICENSE_EXPRESSION`，等主程按实际旧包许可载体与
官方版本源证据签 clean short delta；不猜 MIT，缓存 `req-01..03`，不恢复 M11 或送原 R。后续同源
API24／GLES3 normal APK 仅在这些前置与 LAYOUT 实施验证闭合后，按已准备的最短内部试玩清单运行；
任何内部试玩通过仍与最终 Demo 接收、真实设备门和独立产品 R 分开。

### Resource Codec PR10 FIX02 独立代码审查（2026-10-03）

PR #10 head `0377c31fef0a0bc08478175c6a22667bb21c8b73` 的 GitHub 审查已完成，原始结果为 “Didn't find any major issues”。此结论只关闭该 head 的代码审查门，不代表正式 APPROVED。旧 P2 `r4171291210` 仍未在 GitHub 标记 resolved。FIX02 D13+C10 原生验证另由主程/C 接收，PR 发布时 M02 测试为 0、无 XML。详见 [审查回执](testing-resource-codec-fix02-pr10-review-receipt.json)。

### Resource Codec D FIX02 + C Contract 联合 M01 原生回执（2026-10-03）

固定组合投影原生门分别接受：D FIX02 13/13、C Contract 10/10；XML 总计 23/23。此回执不代表单一 PR head 或整体验收；C 原始 reviewHead 仍为 null。真实资源清单/依赖、离线缓存、Host/UI、Player、Windows/Android/设备及共享接纳门仍未关闭。证据与边界见 [联合回执](testing-resource-cd-joint-m01-receipt.json)。

### Resource Contracts PR11 GitHub 代码审查（2026-10-03）

GitHub PR Code Review 已完成并绑定 head `f99b5a60b9ecad62aa01ddb1333a7db8a05c662f`，提交状态 `COMMENTED`，报告 1 项未解决 P2：阶段状态与剩余下载量之间允许不可能组合。该发现已转中央/主程判断；本记录不构成正式批准，也不改变独立的 D13+C10 原生回执。详见 [PR11 审查回执](testing-resource-contracts-pr11-review-receipt.json)。

### Resource Contracts FIX01 PR11 GitHub 代码审查（2026-10-03）

固定 head `19179f3a68725d4f806bbc99989bf514f626acbf` 的 GitHub 结果原文为 “Codex Review: Didn't find any major issues. Another round soon, please!”。这只记录该 head 的无 major issues 代码审查结果，不是正式 APPROVED；旧 head 上 P2 discussion `r4171468078` 仍由原记录跟踪，本回执未自行 resolve。FIX01 的 C11 原生验证尚未运行，旧 C10 证据不适用于 FIX01。详见 [FIX01 PR11 审查回执](testing-resource-contracts-fix01-pr11-review-receipt.json)。

### Resource RAW/B/D/C 联合 M01 原生回执（2026-10-03）

固定组合投影原生门分别接受：RAW FIX01 12/12、B 基线回归集 38/38、D FIX02 基线回归集 13/13、C FIX01 11/11；总计 74/74，XML 测试名与四组冻结清单完全匹配。该结果不代表任一单 PR 整树、Host/SDK真实下载、共享接纳或设备验收。RAW head 保持 null；C 绑定 FIX01 head `19179f3a68725d4f806bbc99989bf514f626acbf`。详见 [联合原生回执](testing-resource-raw-joint-m01-receipt.json)。

### Resource RAW PR12 GitHub 代码审查状态（2026-10-03）

PR #12 固定 head `4d4b07cc1b432add8d8102290802703ed704eb21` 的 GitHub 审查现已完成（review `PRR_kwDOTDyjrM8AAAABQdBD8g`），报告一项未解决 P2：`dispatcher.Post` 调度下一 raw chunk 抛错时，操作可能停滞且保留流与预留。此记录只收录审查发现，不作修复判断或正式批准；联合 M01 原生回执仍独立有效，不代表 PR 整树、Host/真实包或设备验收。详见 [PR12 审查回执](testing-resource-raw-pr12-review-receipt.json)。

### Resource RAW FIX02 PR12 GitHub 代码审查（2026-10-03）

固定 FIX02 head `1b4e091526444822e487240cd0d6aff736f0080c` 的 GitHub 结果原文为 “Codex Review: Didn't find any major issues. Bravo.”。该结果只适用于此 head，不是正式 APPROVED；旧 head P2 的状态不由本记录改写。FIX02 原生验证仍独立待执行，旧 74/74 不归属 FIX02。详见 [FIX02 PR12 审查回执](testing-resource-raw-fix02-pr12-review-receipt.json)。

### Locale Preference FIX01 PR3 GitHub 代码审查（2026-10-03）

固定 FIX01 head `229fdc604841e5efbbf490cbcc47b7dfc39b8db9` 的 GitHub 结果原文为 “Codex Review: Didn't find any major issues. 🚀”。只关闭该 head 的 no-major-issues 代码审查门，不是正式 APPROVED；原代码门及两次 discovery 失败仍保留历史状态。FIX01 原生发现/导入/编译/测试仍待完成，旧 77 matches 不算通过。详见 [FIX01 PR3 审查回执](testing-loc-b-pref-fix01-pr3-review-receipt.json)。

### RAW + PREF M02 原生结果与保护状态（2026-10-03）

M02 NUnit XML 对固定输入的 128 个唯一测试名全部通过：B38、RAW13、PREF77；旧 M01 PREF 发现清单为 77/77 Runnable、覆盖 21 个类别要求，discovery 本身未执行测试。测试结果可供该精确输入使用，但整体 execution receipt 仍为 `BLOCKED_OR_FAILED`：指定旧私有 TMP 的 `adb.501.log` 前缀 1396B 哈希一致，另观察到 36B 追加 `adb server killed by remote request\n`；写入者未知。不得将运行/保护门改判为通过或将副作用归因于测试 owner。详见 [M02 QA 回执](testing-resource-raw-pref-m02-receipt.json)。

### Resource Activation + Factory Joint M02（2026-10-03）

固定复合投影的 40 项 XML 测试全通过：AS8、RAW13、FAD6、D13；证据和边界见 [联合 M02 回执](testing-resource-as-factory-m02-receipt.json)。PR13 head `d9c15b361c…` 与 PR14 head `5cfa73524a…` 的 GitHub 审查均已完成，结果均为 “Didn't find any major issues”，但都不是正式 APPROVED，也不替代各自尚需的原生/Host/设备等验收门。详见 [PR13 回执](testing-resource-activation-pr13-review-receipt.json) 与 [PR14 回执](testing-resource-admission-pr14-review-receipt.json)。

### Host Continuation FIX01 PR16 GitHub 代码审查（2026-10-07）

PR #16 head `1e2f7ad5382e6c36ec402e14fa224e1de286d1fa` 的 GitHub 审查已完成，结果评论 `6041500204` 报告 “Didn't find any major issues”，对应 reviewed commit `1e2f7ad538`。全部评论、review 与 inline thread 页已读完，正式 review submission 和 inline findings 均为空；仅关闭该 head 的代码审查门，无正式 APPROVED。PR 保持草稿未合并。修正版编译与十项测试由中央另收唯一 C 回执，原 M01 编译失败保留，设备／视觉／Demo 验收未由本结果建立。详见 [PR16 审查回执](testing-host-continuation-pr16-review-receipt.json)。

### Host Continuation Natural Meta PR16 GitHub 代码审查（2026-10-08）

新 head `030898e9a317d10b610058d867529be8b0770c7c` 的 GitHub 审查已 Completed，机器人结果评论 `6041859816` 明确 reviewed commit `030898e9a3` 并报告 “Didn't find any major issues”。全部分页已读完，正式 review submission 和 inline findings 均为空；该 head 的代码审查门关闭，无正式 APPROVED，PR 为草稿未合并。旧 head 回执冻结。M03 Editor exit 0／Host.Tests 编译成功及旧 wrapper meta 字节门失败按中央来件记录，T01 十项测试由中央另收 C；本次未执行或接收测试，也未建立设备／Demo 接收。详见 [Natural Meta 审查回执](testing-host-continuation-pr16-natural-meta-review-receipt.json)。
