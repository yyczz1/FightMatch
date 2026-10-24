# FightMatch 首个 Demo：L1 试玩指南

更新：2026-10-25 · DEMO-NEXT-001 · 主策维护

> **首 Demo 已于2026-10-25正式结案，QA集成总判定 `ACCEPT`，既定范围内无剩余必验项。** android-14已保档安装到iQOO Neo5；最后一次首红L与有效Skip已完成，手机保留战士95HP、敌0＋15、单红L现场，未领奖、重开或执行第二行动。旧L1／奖励／恢复／导航／回退／双语及真人观察按各自输入复用，不称APK14重跑全套。PR仍draft未合并，不扩展为完整Android产品或发行验收。详见第6节和[主测试结案回执](../team/2026-09-30/testing.md)。

## 1. 适用范围与启动前检查

- 本页记录已定 uGUI 正常玩家路线；当前android-14身份及iQOO实际观察见第1.1节。android-13模拟器新增观察、此前android-11等历史及android-12未安装构建各按原版本记录，不把分段证据当作同一次完整L1流程。
- 内容仅为“第 1 关 / Level 1”（L1，版本 1）、一名战士、两个空槽、单面 4×4 棋盘和两名发条步兵。使用占位美术；与正式产品共用业务和存档，不是另做一套演示数据。
- 真正的新资料为战士等级 1、经验 0、生命 100，背包及携带为空、无已学主动技能。已有资料保留真实成长、库存及战斗，不为匹配本指南清档。
- 没有下一关解锁、额外首通奖励、职业证、卡牌或已发布配方。不包含 L2、新职业、广告、后台服务、账号／云档、资源下载体验或最终美术。
- 界面上的语言下拉框提供“英语 / English”和“简体中文 / Simplified Chinese”；不是另一个“设置”页面。首次无手动偏好时按系统语言选择简中或英语，手动选择应跨启动保留。成功保存提示约 2 秒后消失；保存失败／结果不确定须按提示重新保存语言，不走战斗存档恢复。

既有 Mac 人工观察覆盖语言下拉、部分双语按钮、语言冷开记忆、原资料连续性和成功提示消失；Android模拟器限定观察按各自APK版本见第1.1节。各分段按原输入复用，2026-10-25首Demo总验收结论见第6节。

### 1.1 当前 APK 与安装示例（android-14 已在 iQOO 保档安装；首Demo于2026-10-25验收结案）

| 项目 | 本轮实际产物 |
| --- | --- |
| 文件 | [fightmatch-demo.apk](../../Builds/DEMO-NEXT-001/android-14/fightmatch-demo.apk)（`Builds/DEMO-NEXT-001/android-14/fightmatch-demo.apk`） |
| 大小／SHA256 | 46156870 字节；`c3880130093c928516ca559cc9c36bbf612004a58d958cc4055936e2f191bd8d` |
| 包名／版本 | `com.yyczz1.fightmatch`；0.1 / code 1 |
| 平台 | Android 7 / min API24；目标 API32；ARM64、IL2CPP、Development；仅 GLES3 |
| 公有签名证书 SHA256 | `2045e6c0b3dd319ae5682d10701ffcdd2337fdf5a539c7a065d1d7c8024bac51`，与android-11相同 |
| 当前边界 | 首Demo既定范围已ACCEPT；APK14在iQOO补齐一次真实首红L／有效Skip，旧项目按原证据复用；PR仍draft未合并，不扩展为完整Android产品或发行验收 |

APK14构建、安装与页面观察（2026-10-24）：[android-14 BuildReport](../../TestArtifacts/FightMatch/DEMO-NEXT-001/android-14/build-report.json)、[安装结果](../../TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/ui-feedback-01-device/retry01/install-result.json)、[点选战士画面记录](../../TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/ui-feedback-01-device/retry01/warrior-selection.json)及[页面后检](../../TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/ui-feedback-01-device/retry01/post-ui-state.json)。一次构建Succeeded、0错误／0警告，原签名、API24／ARM64／IL2CPP／GLES3保持；APK14已保档覆装iQOO Neo5原资料。中英页面观察后恢复简中，战士持久白框可见，普通状态隐藏无关恢复控件；初始4×4、战士100／双敌15、无连线，未领奖；安装后未启动时9份可读文件与安装前一致；页面观察后8份业务文件保持，资源启动记录正常更新，并新增最终为zh-Hans的语言偏好；未推进战斗。PR22 `dece3d8c`的匹配独审[未发现重大问题](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6380505631)；限定3项用例为原Outline 1项复用、FIX03修正2项通过，不表述为新增测试3/3。此前android-13在BlueStacks的启动／退出复开证据和L1／奖励／三状态／语言／布局证据仍按原输入复用；本轮未重跑。该段构建／安装／页面观察本身不代表clean PR整树验证或首Demo总验收；10月25日结案另见第6节。本机WIP与clean PR边界仍分别记录。

**APK14真人已确认：** [主测试当前收件](../team/2026-09-30/testing.md)已接中央真人turn `01a16db6-11b1-7560-b01a-4f9349a152f9`的实答：选中白框持续、单指拖动白线与松手清除、第二指不夺操作、拖线中Home后返回清除预览且可见战局不变，以及实际系统Back弹出退出应用确认、取消正常、必要按钮未被系统区域遮挡。该组为用户直接观察，不新增内部ID、精确行动数或业务文件SHA。

**APK13模拟器本次补齐：** [定点观察回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-remaining-01/result.md)已补英文Differences小标题、简中选步骤说明的完整画面；未播放参考步骤。user10实际打开“按原开局重来”预览后取消，经Adventure→Level 1→Continue返回同一可见战局（95HP、敌0／15、红L及一条历史），没有新行动、确认重来／回退或领奖；此项不是历史回退预览取消，也不证明user10文件零写。[受控英文首开回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/nonchinese-first-open-01/controlled01/result.md)记录真实Android系统以English为首语言，仅暂移既有语言偏好后普通冷开自动显示English；这是既有APK13安装上的无偏好受控冷开，不是全新安装、新资料或APK14真机英文首开。随后原偏好文件名、字节及属性、完整系统语言顺序均恢复，user0回到原简中待结算未领奖现场，18份可读业务文件集合及SHA保持，2个不可读锁仍排除；资源技术记录随启动变化另列。QA结合既有iQOO中文自然首开已接收两类默认语言覆盖；未变实现及内部合同证据复用，不要求APK14模拟器重测。下列各历史段仍保留当时输入与缺口。

**既有已安装／有限观察历史：** [android-13](../../Builds/DEMO-NEXT-001/android-13/fightmatch-demo.apk)，46153060字节、SHA256 `b070e9c6d26de0632e6b56eb1658b1d3e9f3d685fa73ee9beeafe56864c9795c`；[android-11](../../Builds/DEMO-NEXT-001/android-11/fightmatch-demo.apk)，46143120字节、SHA256 `73c8da9bd3d166848e1eb56d75052f68078eef16fcfa0d9aa8e041b5d0fe011f`。以下Reference及既有路线结果仍按各自原APK输入，不扩展为android-14重新通过。

android-11运行依据：[reference-text-run01摘要](../../TestArtifacts/FightMatch/DEMO-NEXT-001/reference-text-run01/result.json)与[参考设备回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/reference-text-device01/result.md)。一次标准构建成功、零错误／警告，一次保档安装，未运行测试；API24／ARM64／IL2CPP Development／GLES3及签名保持。[旧导航](../../TestArtifacts/FightMatch/DEMO-NEXT-001/navigation-device01/result.md)、[六弹窗](../../TestArtifacts/FightMatch/DEMO-NEXT-001/modal-text-device01/result.md)及其QA限定收件继续复用；原失败和未执行项不改绿。七路径源码审查另以第6节10月22日同head新回执为准；本机 Build 旧 WIP 不等于 clean 发布树已测。android-12身份见其历史构建记录；它是未安装旧包，不再作为最新包。

**已完成的新档与奖励观察（flow01）：** 在 BlueStacks/API33/ARM64 的独立 user10 中，未清除原档；建档确认／取消／Android Back／确认创建、[新战士Lv1／XP0和两空槽](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/new-party.png)、空背包及正常两条拖线／播放已观察。一次显式结算的[收据为26经验＋2锡片](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/new-result-after-wait.png)，[队伍26/60](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/reward-party.png)与[背包锡片2](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/reward-inventory.png)已核；后续未追加领奖。基础参数 `E=20` 不是最终到账20，公式及适用条件见第2节第5步。

**参考、管理与四个结果去向：** flow01已点结果“查看当前队伍”；[flow02](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow02/result.md)通过查询原胜利结果补验“回地图”“查看当前背包”“重新入场”，重入为100HP／两敌15／空棋盘，没有再领奖。同段已观察默认参考条件／两步／重复／首拖收起并保留手势，以及回退一条、按原开局重来、退出本局的预览／取消／确认；[flow03](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow03/result.md)补验可见英语参考／历史、队伍／背包／无配方、旧胜利收据和无奖励退出结果。它们是分段证据，不冒称同一次参考参与至结算全链；该历史段未证实Skip因果，10月25日真机补验另见第6节。

**活动局导航：** android-09已实际经 Adventure 到 Map／Party／Inventory，只读查看现有资料；Start Battle、Review及选战士后的 Clear Equipment 均禁用并已实点观察。Continue回战斗后仍为95HP、敌0／15、原红L及一条历史，与导航前稳定画面一致。这里只证明导航接线、可见门禁和外观保持，不证明内部 attempt、播放 token／回退 owner 或 user10 文件零写；详见上述导航回执。

**android-10双语六弹窗：** 中英 Undo／Restart／Exit 的全文、标题和固定按钮均可见；android-09原英文Restart／Exit、中文Restart的遮挡已由本包解决，原红图保留。六次均只取消，未重新确认回退／重来／退出，功能证据复用flow02。Undo当前只有一条历史且一屏容纳，上滑无位移、未另做回顶下滑，不算长正文溢出滚动通过。取消后战局外观保持，未追加行动、Skip或奖励。

**android-11参考文本：** 原首段／范围截断已修，双语正文与两个步骤可见，固定Reference标题和Close在滚动中保持。本轮未点击步骤，既有交互证据复用。`en-reference-08`差异小标题受滚动顶缘裁切，后续帧未补齐完整字形，不据此判定永久不可达；简中步骤选择说明全文也未由保存帧单独证明，不宣称所有字形全覆盖。

**android-11最终现场与历史复用：** 该次保档终核已完成，user10恢复English、95HP／敌0＋15／红L／一条历史；同实例自然加载后的[user0恢复图](../../TestArtifacts/FightMatch/DEMO-NEXT-001/reference-text-device01/user0-final-restored.png)显示简中95HP／双敌0待结算且未领奖，原占位早帧保留。18份可读业务SHA与本次安装前后及既有flow03／弹窗证据一致，不扩大到锁文件或全部系统字节。user10外观不作内部身份或零写证明。旧 [android-07 排版](../../TestArtifacts/FightMatch/DEMO-NEXT-001/english-layout01/result.md)、[android-06 语言／混合输入](../../TestArtifacts/FightMatch/DEMO-NEXT-001/focus-run01/result.json)、[android-05 奖励／三态冷恢复](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/result.json)继续复用，未重跑三状态或追加领奖；旧 `ACCEPT` 不覆盖新七路径候选。

**当前剩余：无。** 2026-10-25有效业务播放中的实际Skip已由QA收件，首Demo既定范围总判定`ACCEPT`。两处Reference文字、导航、回退取消、两类系统默认语言、真人触控与系统操作及内部合同均复用已接收证据；历史缺口与失败保留原时点，不重跑或另加门槛。

**以下是android-14保留资料安装示例。** 本次iQOO保档安装已完成；如后续设备合同明确授权执行，再先核目标设备、API/ABI及已有包身份，用现有SDK的`adb`并将“实际序列号”替换为目标。本指南不授权追加安装；保档安装，不卸载、不清数据或强制降级。

```sh
adb devices -l
adb -s "实际序列号" shell pm path com.yyczz1.fightmatch
adb -s "实际序列号" install -r "Builds/DEMO-NEXT-001/android-14/fightmatch-demo.apk"
```

若签名冲突、版本降级拒绝或设备不满足要求，保留原安装与资料并核对原因。同签名覆盖安装不替代完整存档兼容验收。下文保留既定试玩路线；APK14未重新执行完整路线或奖励验收，已观察范围按本节各自证据为准。

## 2. 最短试玩路线

以下列出完整试玩路线；新档结算、默认参考交互、管理分支和四个结果去向分别已有观察，准确范围见第 1.1 节，不将分段证据改写为同一次完整流程；有效Skip另有10月25日iQOO补验证据。按钮名称中文在前、英语在后；若按钮禁用，先读当前原因，不反复点击或创建新请求。

1. **进入同一份本机资料。** 真正空资料时点“创建本机资料 / Create Local Profile”，先打开“创建本机资料？ / Create a local profile?”确认框，阅读“资料只保存在本机，设备数据全部丢失将无法找回”的告知；此时尚未发起建档。点“取消 / Cancel”或返回只关闭确认，不创建资料；点“确认创建 / Create Profile”才发起原建档请求。已有资料正常读取；若上次创建未完成，使用“继续原创建 / Continue Original Creation”或“重新读取 / Reload”，不再另建一份。成功现象：进入导航，读取同一份资料；重复点击不生成第二份进度。不要为试玩卸载或清数据。
2. **检查地图和队伍。** 在“冒险 / Adventure”看到“第 1 关 / Level 1”；切“队伍 / Party”检查战士和两个空槽，切“背包 / Inventory”检查实际库存。新档空包、暂无配方均正常。回“冒险”，点“第 1 关”，在“入场准备 / Battle Preparation”检查阵容，可点战士查看选中状态，再点“开始战斗 / Start Battle”。当前接线没有单独的“确认入场条件”按钮；正式检查由开始战斗请求执行。若已有活动局，改点“继续当前战斗 / Continue Current Battle”，不新开一局。成功现象：进入原活动局或本次新局，棋盘及敌我生命可见。
3. **先看免费参考，再选战士。** 点“播放参考 / Play Reference”，进入“已验证的开局默认参考 / Verified Starting Reference”，选择一个步骤查看棋盘覆盖，可重复查看，再点“关闭参考 / Close Reference”。参考只演示发布时验证的默认开局，不替当前战斗出手，不是任意局面的必胜提示，也不需要广告。准备页的选择不等于战斗选人：进入新局后点底部“战士 / Warrior”成员按钮；iQOO APK14页面观察中，选中白框会持续显示。
4. **真实拖线两次。** 按下节路线，从端点按住，经横竖相邻格拖到同色配对端点后松手。完成第一条后等待表现，或点“跳到当前战局 / Skip to Current State”，再画第二条。成功现象：每次合法拖线只产生一次行动，目标敌人生命降至 0、路线固定；两名敌人都被击倒后出现“胜利已保存，基础奖励待结算”。参考覆盖本身不计行动；真实拖动开始时应关闭参考覆盖，不吞掉这次拖线。iQOO单指短拖的白线与松手清除已获真人确认；10月25日实际有效Skip亦已验收，当前手机可直接续接第二条路线体验，点选／短拖说明见第3节。
5. **明确结算，再核对到账。** 等末段播放结束或明确跳过，待“继续基础奖励结算 / Continue to Base Rewards”可用后点击。成功现象：进入“战斗结果 / Battle Results”，显示本次真实收据。L1 的基础参数为 `E=20`；新档等级1战士按上述正常两线获胜、有效伤害30／承伤5时，按[已批准规则（§269.2／§271）及§115算例](../system-design/2026-09-17/system-task-packets.md)评分后预期获得 **26 经验、2 锡片 / Tin Sheet**，[demo-flow01 新档结果](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/new-result-after-wait.png)已显示同样数值。其他报告仍按评分公式和真实收据，不承诺所有胜利或重打固定为26。没有额外首通奖。用“查看当前队伍 / View Current Party”和“查看当前背包 / View Current Inventory”核对同一份资料的增量，不能只看表现文字。
6. **回地图或重打。** 结果页可点“回地图 / Back to Map”，应显示本内容段结束、暂无已发布且开放的下一关；仍可重新选 L1。也可在结果页点“重新入场挑战本关 / Challenge This Level Again”，以当前资料走正式新入场；进入后重新选择战士。它不是“按原开局重来”，不会复制上一局奖励或行动记录。保存复开检查见第 5 节。

## 3. L1 可照做的路线

棋盘为 4 列 × 4 行。这里用玩家侧 **1 起编号，左下角是（列 1，行 1），向右列数增加、向上行数增加**。不依赖具体颜色名；起点和终点仍须是同色的一对。

| 次序 | 手指／鼠标连续拖动 | 预期结果（已观察范围见下文） |
| --- | --- | --- |
| 1 | 左下角（1,1）→ 上一格（1,2）→ 右一格（2,2），在端点松手 | 对应发条步兵被击倒，此路线固定；等待表现结束或跳过后继续 |
| 2 | 左上角（1,4）→ 右一格（2,4）→ 再右一格（3,4），在顶行第 3 列端点松手 | 第二名敌人被击倒，进入胜利待结算 |

不需要铺满棋盘，不走斜线，不跨格直跳，不穿过已固定路线。不要把点击参考步骤、短按端点或松开未到终点当成一次成功攻击。

路线依据为[正式 L1 内容](../../Assets/FightMatchContent/demo-r1.source.json)的 `SourceRoutes`；[内容编译器](../../Assets/Scripts/FightMatch/Content/DemoContentCompiler.cs)按已批准的左下原点转换坐标，[棋盘输入](../../Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs)也从左下向上计行。两条路径的边界、横竖相邻、对应端点及互不交叉已静态核对。默认参考还使用其自身已验证开局条件，当前玩家随机流不同，不承诺逐个表现完全一致。

**本次 iQOO 操作说明：** 点底部战士后白框持续显示；普通状态隐藏无关恢复控件。未连到配对终点的短拖只产生预览，不作为一次成功攻击。真人已确认单指拖动有白线、松手后消失，第二指没有抢走操作、第一指仍能控制白线；拖线中Home再返回时预览清除且可见战斗未变。这些观察已收件，不要求重复操作。10月25日首红L及有效Skip完成后，手机为战士95HP、敌0＋15、单红L；可接着按上表第二条路线体验胜利，再按第2节第5步明确结算。这是可选试玩，不是要求重跑验收。

**已有实测：** [demo-flow01 回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/demo-flow01/result.md)记录独立新档的两条真实拖线、正常播放至胜利待结算及一次显式结算；未使用默认参考或跳播。旧 android-05／07 的拖线证据继续有效，见第 1.1 节及历史记录；各段仍保留原版本边界，APK14真机首红L及有效Skip见第6节；本次指南编辑未运行Unity或设备。

## 4. 回退、按原开局重来和退出本局

这些是短分支检查，建议在第一条路线成功且表现结束后操作；不是完成最短通关的前置条件。每次先看预览，再决定确认或取消。

| 操作 | 当前入口与确认 | 应观察的结果 |
| --- | --- | --- |
| 回退 | “行动记录 / Action History”→ 选一条实际记录 → 检查回退数量和“回退后战局 / Battle State After Undo”→“确认回退这些行动 / Undo These Actions” | 仅撤销预览中的本局行动，恢复对应敌我生命和路线；可重新画第一条。选“保留当前战局 / Keep Current Battle”则不改变战局 |
| 按原开局重来 | “按原开局重来 / Restart from Original Entry”→ 看影响 →“确认按原开局重来 / Restart from Original Entry” | 结束当前尝试，按原入场阵容、成长、携带与随机初态重来；未完成的尝试不发奖。新尝试需重新选战士，不把它当当前成长档的普通重打 |
| 退出本局 | “退出本局 / Exit This Battle”→ 看影响 →“确认退出本局 / Exit This Battle” | 当前尝试结束、不发本局奖励、不自动开新局。当前代码先显示无奖励的结束记录，再点“回地图 / Back to Map”；不能把退出整个应用代替此操作 |

没有行动时历史入口不可用；保存待处理或正在播放时，回退等操作以当前可用状态为准。回退不撤销永久成长或已到账权益，也不能用来重复领奖。

## 5. 保存、恢复和语言复开

**已有实测：** [android-05 原设备回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-05/result.json)已验证既有测试档的待结算、已结算、第二局首行动后三次真实进程冷恢复，保留对应业务字节和状态；已结算复开保持 26/60 经验、2 锡片，没有重复发奖。后续包继续复用这些已接收证据，未重跑三状态。flow01 另观察原 user0 的 Home／回前台、根退出取消及确认退出后普通启动恢复原待结算；最新原档18份可读业务SHA保持的范围见第1.1节。旧3021／9723退出触发仍未归因，不由正常退出对照或有限日志无异常匹配关闭。历史边界与本次结案范围见第6节。

- **活动局复开：** 记录第一条路线后的敌我生命和历史，再按测试安排关闭、重开应用，应读取同一资料，使用“继续当前战斗”恢复原局，不能偷偷创建新局。
- **结算后复开：** 先记录结果、队伍经验和背包锡片，再关闭重开，应保留同一资料和已结算事实，不因再次读取结果重复入账。不要清档、卸载重装或注入胜利来代替检查。
- **正常退出应用：** 导航根页“返回 / Back”打开退出确认；“留在游戏 / Stay in Game”取消，“退出应用 / Exit App”退出。这与“退出本局”不同；遇到弹窗时先按其当前按钮处理，不假设所有页面的返回键都直接退出。
- **保存受阻：** 保留原资料和诊断信息，只使用当前界面开放的“查询原结果 / Check Original Result”“重试原保存 / Retry Original Save”“确认原保存结果 / Confirm Original Result”或“继续选中的原保存 / Continue Selected Original Save”等原请求入口。未知结果不等于已失败；不可另建资料、另开战斗或改发一次新结算绕过。若没有可用恢复入口，记录当时页面和诊断码交主程／主测试。
- **已有旧资料：** 如界面要求“升级本机资料 / Upgrade Local Data”，先查看当前升级预览并按原流程确认；不把升级当作重新建档。
- **语言复开：** 在允许切换时选择英语或简中，确认文本更新和保存反馈；重开应保留手动选择。若出现 `【if you see this, it is a bug.】`、未替换的 `{参数}`、缺字或溢出，记录页面／语言／按钮，不将其当作正常成品文案。语言保存问题与战斗业务存档问题分别处理。

## 6. 首 Demo 验收结论与范围（2026-10-25）

1. **首Demo集成总判定：`ACCEPT`，既定范围内无剩余必验项。** QA actual `01a16eb2-f323-7472-9419-b6043df3be30/local`已接收主程正式交付。依据本次有效Skip与既有L1／奖励／三态恢复／导航／回退／双语／真人触控和系统操作、内部合同，以及PR22 `dece3d8c`准确head独审共同结案。范围为已批准L1、占位美术及正式业务／存档，各段保留原输入；不称APK14重跑全套、全新安装或完成完整Android产品／发行验收。PR仍draft未合并。
2. **有效Skip已补齐：** [本次结果](../../TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/remaining-01/authorized-20261025-result.json)与[执行回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/remaining-01/authorized-20261025-result.md)记录C在其执行聊天获人类直接精准授权后，于原APK14／iQOO完成一次首红L及一次有效Skip；QA逐帧确认第三反馈提前转为当前战局，未播放第四反馈。原9份可读文件及SHA保持，仅新增同ID的snapshot／commit一对，旧不可读锁不扩展证明。手机最终95HP、敌0＋15、单红L，未领奖、重开或第二行动；仅游戏本机录屏交付9.902056秒，原容器时长及静止尾帧裁剪说明留在原回执。操作及证据收尾216.481秒，含回传276.186／300秒。原导航、回退取消及owner／token证据复用，没有重跑已撤销的跨页竞速组合。
3. user10未取业务文件哈希、长历史溢出滚动未覆盖、旧3021／9723退出缺触发日志而未归因，这些历史边界继续保留；不把它们、clean PR整树测试或未来CDN另升为首Demo门槛。已通过的语言、文字、导航、奖励、恢复和真人项不重跑；原失败不改写为通过。
4. **审批拒绝历史（2026-10-24）：** 原自动一次行动＋本机短录屏因跨聊天授权证据未获认可被明确拒绝；该段行动0、录屏0，停止重试，手机当时为战士100／两敌15。此后等待执行聊天的直接许可，已于10月25日由C取得人类直接精准授权并完成上述限定操作；旧拒绝不改作成功，两段授权、执行和耗时分别保留。主程接收正式回执，QA完成总验收，不再处于待许可或待Skip状态。

已核清的接线与修正：建档前可取消确认是现行要求，已补接，不再使用此前点击即直接建档的路径；真实 Prefab／catalog 的建档确认单例修正后通过，原返回键／语言反馈两项通过记录复用。准备页仍由 Start Battle 执行正式入场检查，准备页选人不替代战斗内选人；退出本局先显示真实无奖励结束记录，再回地图，符合既定业务目标。限定用例和静态接线不代替完整视觉、L1 或设备观察，也不由本指南批准产品方案变更。

当前不需要为写出可试玩 L1 追加关卡、职业、奖励、广告或最终美术。本轮指南验收仅要求：正式中英标签与实际入口一致、路线可由配置追溯、关键操作有可观察结果、各段实际证据与验收范围清楚分开。

## 7. 旧 APK 限定验收、失败包与 Toolkit 资料（仅历史）

**android-07 的英文排版修正已获限定 `ACCEPT`，不是完整 Demo 验收。** 原包46134553字节、SHA256 `dbd5a5cc0646b15009f72a5a26bb6ab7c006ce06a659761eff81217bf7c59204`；已发布产品 `1e4cfeaea7de5d9b7eeeddddf74a8b45a2c06a25` 的[独审6250967784](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6250967784)与 [English-layout01 原回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/english-layout01/result.md)、[emulator-07 设备证据](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-07/result.json)共同构成该次限定收件。中英920／812高度检查最终1/1及真实拖线观察通过，原夹具、溢出失败及未领奖边界保留；后续新 Host 修正不沿用此源码门。

**android-01 是启动失败的历史包，不再推荐安装或试玩。** `Builds/DEMO-NEXT-001/android-01/fightmatch-demo.apk`（36583902 字节，SHA256 `4d72f265bd437d9d0e5131128ae46f71fb4fd968310f3a2d93344a791c017ab6`）虽于 2026-10-16 构建及静态包检查成功，但 2026-10-17 已实际安装、从普通图标启动，初始化报 `RES_STATE` 并停在诊断占位符，未进入 L1／奖励／恢复验收。失败事实保留于[模拟器原回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/resume-result.json)，不要把它与第 1.1 节的 android-11 混用，也不通过清档修复。

**android-02 是随后仍未进入玩法的历史包，不推荐 L1 试玩。** `Builds/DEMO-NEXT-001/android-02/fightmatch-demo.apk`（36583902 字节，SHA256 `e50b711ecbebc7d380fa62d1abd5d7bbedfda0760b292eba2cf6eb17ea8b2480`）的[8 项定向及构建阶段](../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/result.json)全部通过、构建成功，阶段用时约 6 分 45 秒。[emulator-02 回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/result.json)记录保留资料安装和普通图标 GLES3 启动后报 `RES_STORAGE`，设备阶段 354.195276 秒；L1／三状态复开／奖励均 `NOT_RUN`。当时 6 份可读业务文件哈希不变、2 份 lock 拒读；该包的检查通过与运行失败均保留，不由 android-03 诊断替代或改写。

**android-03 为历史诊断包，不是存储修复包。** `Builds/DEMO-NEXT-001/android-03/fightmatch-demo.apk`（36583902 字节，SHA256 `72186bd6acf65fba5bd02f6d7da3c67ad2ca532d768fddc68172850e4ebd47ee`）的[6 项检查与构建](../../TestArtifacts/FightMatch/DEMO-NEXT-001/startup-storage-run01/result.json)通过；[emulator-03 回执](../../TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-03/result.json)记录标准 `MAIN/LAUNCHER` 启动、图标自动点击未验证，安全日志为 `activation.guard.ancestor.entries / UnauthorizedAccessException`，仍报 `RES_STORAGE`。该包 L1／三状态／奖励均未运行，原 6 份可读业务档保持、2 份 lock 未读；这些事实不由 android-04 的进展改写。

2026-09-29 的正常 019 包 `Builds/FMDemo029/mac-r1/019/fightmatch-demo.apk` 和 QA 018 包 `Builds/FMDemo029/mac-r1/018/fightmatch-qa.apk` 是旧 UI 时代的构建，不代表当前 uGUI 交付。019 曾在 BlueStacks 以 `-force-gles30` 到达“创建本机资料”，默认 Vulkan 启动仍崩溃；完整流程和冷启动未验收。不要按旧按钮、旧安装命令或旧 Min SDK22 声明推断本轮状态，也不要把 QA 包当普通试玩包。

旧包身份、签名、编译／测试和设备缺口保留在[原交付记录](../system-design/2026-09-17/demo-029-mac-r1-delivery.md)。其中旧日期的“暂停”“未推送”和旧验收数字只描述当时输入，不覆盖当前版本。

## 8. 本页核对输入

- 本轮同步（2026-10-25）：仅更新本指南当前状态与余项，依据[QA结案回执](../team/2026-09-30/testing.md)及`authorized-20261025-result.json`／`.md`。QA actual `01a16eb2-f323-7472-9419-b6043df3be30/local`确认首Demo总判定`ACCEPT`、无剩余必验项；本次文档编辑未运行Unity、设备、测试、Git或新审查，中央接收完成短回执。
- 历史同步（2026-10-24）：仅更新本指南，依据主测试当前收件、APK13两份定点回执及中央真人实答。QA actual `01a16dde-e1b2-7c62-83d6-6c8f85b86941/local`已接收本轮限定观察；最终唯一剩余项已由QA actual `01a16dee-6b3d-7621-b675-98fd64589a8a/local`确认，首Demo整体仍`NEEDS_FIX`。未运行Unity、设备、测试、Git或新审查；中央接收本页完成短回执。
- 历史同步（2026-10-22）：DEMO-NEXT-001；2026-10-19 中央将本页写权归还主策，10月22日本次仅同步指南及planning现有当前段的七路径发布／源码审查状态；仅改这两份主策文档，未改产品、配置、文案表或资产，未运行 Unity、设备、测试或 Git。
- 内容：`demo-r1.source.json`，SHA256 `fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a`。
- 正式文本：[FightMatchText.csv](../../Config/FightMatch/Localization/FightMatchText.csv)，SHA256 `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030`。实际入口以 Host、PlayerNavigationView、PlayerBattleView 和 PlayerBattleSession 的绑定为准，不把表中尚未绑定的键写成可点击按钮。
- 既有观察基线：2026-10-16 接续入口所记 `c3f0812` 窄 UI 收件；本轮建档确认和 APK 证据另列，不扩大旧视觉观察范围。本页核对不是首 Demo 的 `ACCEPT`。
- 历史静态收件：原作者 actual turn `01a1430e-b2f7-75e0-bc67-e2437c3fcb11` 确认当时入口、战斗选人和路线坐标；其中直接建档已被本轮确认门修正取代，其余路线沿用，不把该只读回执当运行证据。
- 既有建档修正依据：[原定向记录](../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused.xml)为返回键、语言反馈两项通过及建档确认一项失败；修正测试基线后的[建档确认单例](../../TestArtifacts/FightMatch/DEMO-NEXT-001/profile-confirm-focused-fix01.xml)通过，不改写旧失败。
- 历史修正／产物依据（android-11）：`android-11`及 `reference-text-run01`／`reference-text-device01` 摘要，C actual turn `01a1508c-c49d-7a42-8354-243b734ad762`，保档终核与自然恢复补图已收尾。旧导航／弹窗的QA actual `01a1506c-6396-7c20-b210-c147a9b14485`及旧奖励／语言／排版／冷恢复仅按原范围复用，不作为本次独审或完整字形验收。七路径已发布 `4e78fdec`，QA actual `01a16356-952d-7563-8060-ac8d93d48124/local`已接同head独审，整体仍 `NEEDS_FIX`；主程接收本次同任务文档差量，中央仅收完成短回执，保护与预算引用原回执，不新增运行或验收。
