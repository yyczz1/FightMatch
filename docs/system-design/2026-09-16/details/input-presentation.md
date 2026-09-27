# FightMatch · 首个白模Demo输入与表现限定设计

2026-09-20 · r3 · M01 · **SD12-DEMO-01-C1最终窄修设计交付，不构成Unity、正式美术、SDK、服务或Git实施授权。** 只覆盖“选人画线→完整已提交战斗反馈→回退／重来→基础结算显示”的本机白模闭环。

## 1. 输入、依据与边界

冻结输入为共同r18／扩展r15、[M02应用流程r1](application-flow.md)、[M04库存与偏好r4](inventory-loadout.md)、[M06战斗r3](battle-history.md)、[M07结算r3](settlement.md)及[M09攻略r2](guidance.md)。M06 SHA256为`0A9C30F131822DAD484A12B234A0262B49897F3190837A2655DA4024A665FE41`，M07为`0D4915DCD800AC97E957D697FCE2AB603B081BB1041DF75690B3E3A819598A77`；M09仅采用r2 §2～4冻结区`CAC0510AA58B28E8E1DB0941F19933B894072A80696192ABE02F06AC60FA9DA0`。

已确认规则继承[共同契约](../interaction-contracts.md)和[正式策划](../../../game-design/2026-09-14-fightmatch-game-design.md)：M01只持S18选择、指针／拖线草稿、窗口和播放进度；M02唯一接收业务意图并管理OperationId与演出门闩；M06唯一裁定路线、HP、阶段和历史；M07给基础结算；M04给当前偏好。本文不新增公共ID、可写战斗账或结算账。

Demo只需1～2个已发布固定样例、占位图形／音效和本机结算。在线分析、广告模拟成功、完整页面、最终动画与正式美术不在闭环内；缺规则的分支显示准确不可用原因，不补默认值。

## 2. 最小内部关系与语义入口

下列名称是M01内部建议，不是公共API，也不要求一名词一个类。

| 内部角色 | 只持有／输入 | 最少方法与输出 |
| --- | --- | --- |
| `CommittedViewBinder` | M02发布的同一Commit只读战斗／结算视图 | `Apply(view)`、`RebuildFinal(view)`；不接收未提交候选。 |
| `PointerGesture` | 一个PointerId、按下命中、起始SceneRevision、板内局部坐标、草稿格序列、是否越阈值／已终止 | `Down/Move/Up/Cancel`；只产`TapLocator`、`RouteDraftCompleted`或无意图。 |
| `IntentComposer` | 当前只读视图、S18选择、完整路线或确认对象 | 形成无OperationId的攻击／补线／回退确认／重来／退出／偏好目标草案，交M02分配或复用OperationId。 |
| `ResultPresenter` | H10 Completed后的前／后只读视图、OrderedFacts和M02令牌 | `Play`、`SkipToFinal`、`ReportCompleted(originalToken)`；动画只读且可丢弃。 |
| `GuideOverlay` | M09默认／当前合格／历史内容及适用条件 | 小窗与棋盘预览、重播、关闭；绝不提交路线或改SceneRevision。 |

M01消费的最小只读`DemoView`包括：AttemptId、SceneRevision、CommitId、DefinitionBinding、阶段；棋盘边界／端点／固化线／PendingLink；角色身份、存亡、HP／状态／CD与可选原因；敌方身份、HP／意图与射程提示；每角色已提交偏好值和PreferenceRevision；允许的业务入口及拒绝原因；可查询的历史定位；WonPendingSettlement／Closed、Settlement／BaseReward和M05给出的适用去向。缺字段不是空集合或0。

M02返回M01的结果分四类：①查询／预览，不改变视图；②Rejected／StaleContext／Busy，附准确原因且无OrderedFacts；③SaveFailed／CommitUnknown，仍显示旧权威视图及办理状态；④Completed，带CommitId、新只读视图、OrderedFacts及`AttemptId＋SceneRevision＋OperationId`原令牌。令牌三项由M02保存和匹配，M01不自增、不改写。

业务入口只有：`SubmitAction(character,pair,route,expectedScene,expectedPreference)`、`SubmitFreeLink(pair,route,expectedScene)`、`PreviewRollback(locator,expectedScene)`、`ConfirmRollback(anchor,expectedScene)`、`RestartAttempt()`、`ExitAttempt(attempt,expectedScene)`、`SetPreference(character,targetEnabled,expectedPreference)`和`ReportPresentationCompleted(originalToken)`；均先到M02。方法名只表达现有语义，不是新共同契约。

`ExitAttempt`是玩家在M02给出的适用活动阶段（含AwaitRescue）的明确退出意图，经M02走既有H06；WonPendingSettlement／Closed及过时Attempt／Scene拒绝。H10前保留原战场且不清线、不返还／发放收益；Completed后才显示M02／M05给出的实际去向。退出不建立新Attempt，重来则联合关闭旧Attempt并建立新Attempt，两者不能共用意图或显示。

## 3. 指针、画线与点按状态机

1. `PointerDown`只捕获PointerId，记录板内局部触点、命中的端点／固化线和原SceneRevision；不选历史锚点、不回退。非活动指针、演出Busy或棋盘外按下只给本地反馈。
2. 坐标依次从屏幕转Panel，再转棋盘局部；规则输入只含零基格坐标。推荐命中归最近合法格中心，边界采用半开区间，屏幕像素和摄像机坐标不进入M06。
3. 有效端点按下后，移动距离超过鼠标6 panel point、触控10 panel point即进入拖线并以该端点建立草稿；新格仍须等指针进入横竖相邻格才追加。推荐阈值再限制为格宽的0.15～0.25；上述阈值、端点热区和采样频率均为**待鼠标／触控实测**，不可写成已验收参数。
4. 拖线只能从当前阶段允许的端点开始。进入横竖相邻新格才追加；进入草稿倒数第二格即弹出尾格，连续沿原路线返回可逐格缩回；其他重复格、自交、穿端点或跨格只标本地无效，不替M06作最终合法裁定。
5. 仅在仍为同一Pointer、未取消、原SceneRevision仍可提交、并于配对端点有效松开时结束草稿。M01把完整路线提交一次并立即把该指针标为终止；重复`PointerUp`、多点第二指针或连按不能再发。
6. `PointerCancel`、失焦、捕获丢失、离开棋盘、未完成松开或视图修订变化都只清S18草稿／捕获，业务意图为无；不固化线、不消耗、不排队恢复后自动攻击。
7. 未越阈值的松开才是点按：端点定位该PairId最近一次实际出手；固化线中段定位形成它的击杀行动。无历史只显示选择反馈；不能把PointerDown或普通空格点按当回退。
8. `PointerUp`判为点按后先提交无写入的`TapLocator`查询；M02向M06取HistoryAnchorId、目标提交前态和所有受影响完整操作／路线范围。若范围仅一项且AttemptId／SceneRevision仍与按下时一致，这次松开直接形成`ConfirmRollback`；跨多项则只显示范围并等待第二次明确确认。两者都以确认提交时为业务时点并再次重核修订；查询／预览不擦线，过时即关闭并重读。

## 4. 角色、AwaitLinks、偏好与防双发

倒下角色以灰态／XX眼显示并从可选出手集合移除；当前选择在新视图中倒下时清除。新Attempt默认无选中角色；只有已批准且只改S18、不产生业务意图的UI选择规则才可给默认选择，不得默选一个会在后续回调自动出手的角色。

`AwaitLinks`只按PendingLink的PairId接收免费线路：端点用原色空心核心＋呼吸外环，多条可任意顺序，端点不写“待”。它不要求选择或存活角色；即使全队倒下也可补。此阶段普通攻击入口禁用，补线完成后的翻面、救援或胜利只取M06新视图，不由M01判断。

救援、翻面、补线和胜利都不由表现推进CD、DOT、随机、敌方意图或奖励。AwaitRescue保留M02给出的复活／广告通关／重来／退出适用入口；本机Demo可只提供已具备的重来／退出，不伪装广告成功。

同一Pointer序列只形成一个意图草案；M02受理后，M01对战斗动作进入该结果门闩，Busy立即显示且不排队。重复回调沿M02原OperationId查询；玩家改变角色、路线或目标是新意图，不能复用旧ID。

偏好点击把当前**已提交值**转换成明确`targetEnabled=true/false`并带期望PreferenceRevision；UI另显示“提交中目标值”，H10 Completed后才替换当前值。SaveFailed回到当前值，CommitUnknown保留未决标签并锁业务写；回退不恢复历史偏好。演出中可提交偏好，但其完成或独立永久到账都不能解除原战斗演出门闩。

## 5. 已提交结果的表现与中断

M01只在H10 Completed后按OrderedFacts播放：行动已提交节拍→逐发攻击／闪避／格挡／HP与状态→被动→职业技能→同批DOT→死亡→存活敌方意图→CD／到期→未杀消线或击杀固化→PendingLink／翻面／救援／WonPendingSettlement。事实缺失不由动画补算，溢出表现不反写HP或贡献。

播放层可在已提交前／后视图之间创建临时插值和特效，但缓存视图不具权威。窗口关闭、切场景、低内存或动画异常时调用`SkipToFinal`：先向M02查询当前完整头，丢弃插值并按该最新视图重建；若已有更晚A2，绝不回装A1。不补演敌方攻击、不重复扣血／奖励，再由M02核对当前门闩。

`ReportPresentationCompleted`必须原样回报令牌；A1旧回调不能解锁A2，新SceneRevision、回退或重来后的旧令牌只被忽略。重复收到原Completed结果时先查M02原结果、当前完整头和门闩：已经展示／跳到终态的令牌不再启动动画，仍在等待的当前令牌直接重建最新终态并回报。S18播放进度可丢，不能据它回装旧视图。偏好完成、奖励到账、Guide关闭和计时器到点都不是战斗令牌。

SaveFailed显示“本次未保存，战局未改变”，只允许沿M02原办理重试／结束；CommitUnknown显示“正在确认是否保存”，保留旧只读视图并封全部本机业务写。仅远端等待显示对应功能待确认，不占全局战斗锁。Busy显示当前等待对象，不承诺稍后自动执行。

WonPendingSettlement表示“战斗胜利已提交，基础奖励尚未完成”，只显示续办结算／恢复提示，不开放攻击、回退、新局或假奖励。Closed仅表示该Attempt已经合法结束；普通胜利还须读取Settlement Completed和实际BaseReward，才显示到账及M05给出的适用“下一关／地图”。Bonus远端待办单独标注，不锁正常去向；Closed不等于“立即重来”。

## 6. 攻略预览与首Demo交接

攻略卡必须显式标三类：`开局默认参考`列阵容／等级／技能／道具／随机前提；`当前局面已验证`列原ContextKey／SceneRevision和条件；`历史内容`列原适用条件与失效原因，禁用“照做”暗示。完整在线搜索未接入时，只显示默认参考和准确“当前解析未接入／不可用”，不造当前获胜步骤。

打开攻略只在小窗和棋盘覆盖层同步播放路线；上方角色、敌人、HP和意图保持真实战斗，不预演攻击／技能。默认参考的未击杀线消失、击杀线保留、免费补线纳入顺序；第一次越过拖线阈值时同时收起小窗与棋盘预览，且不吞掉这次真实拖线。

首Demo可直接接入：M02组合发布的DemoView、当前可操作范围／结果状态／门闩；M06战斗切片、历史预览、Completed OrderedFacts及最终报告阶段；M04偏好值／修订；M07 Settlement／BaseReward；M05适用去向；M09默认参考及以后算法返回的步骤、条件、验证分类和失效原因。

实施前确切缺项：M01↔M02具体DTO／线程与OperationId分配API；M06稳定单步拒绝码及历史范围视图；1～2个Published DefinitionBinding、EntryBaseline和默认参考见证；M02／M06／M07真实实现与H10故障能力；占位场景／预制体／音效；鼠标／触控阈值、热区、动画时长和性能实测。多盾、死亡时点状态、完整敌方发布表和火球终值只阻使用它们的样例，不可用默认值绕过。

## 7. 手工文档见证

以下均为设计推演；实际鼠标、触控、Unity、真机、保存故障、SDK与在线服务一律**NOT RUN**。

| 编号／输入事件 | 意图或无意图 | 已提交显示依据 | 中断／重复结果 |
| --- | --- | --- | --- |
| IP01 端点按下，未到配对端即松开 | 无意图，清草稿 | 原DemoView不变 | 丢捕获／重复松开均不发。 |
| IP02 拖过A-B-C再沿C-B-A缩回并完成另一合法路线 | 只提交最终完整路线一次 | H10后按M06后视图及OrderedFacts | 中途失焦全清；不提交被缩回格。 |
| IP03 同一端点短按与越阈值拖动对照 | 短按先定位：单项且修订仍匹配则确认，多项仅预览；拖动只画线 | 预览取M06 HistoryAnchor；攻击取Completed结果 | PointerDown不回退，Up只走一个分支。 |
| IP04 点击倒下角色再画攻击线 | 无攻击意图，显示不可选原因 | 存亡／可操作范围来自已提交M06视图 | 连按不排队，复活回调未成不改变。 |
| IP05 全队倒下且AwaitLinks有Pair P | 提交P的免费补线，无CharacterId | M06新阶段决定翻面／救援／胜利 | 重复Up只查原办理；不推进回合。 |
| IP06 回退预览跨三次行动，确认前SceneRevision变化 | 预览有意图，确认返回StaleContext | 仍显示最新已提交视图 | 旧范围关闭，不直接擦线或换锚点。 |
| IP07 合法松开后快速双击／双Up | 第一项受理，后续Busy或原结果 | 仅一个Operation及一组OrderedFacts | 不缓存为演出后自动攻击。 |
| IP08 攻击候选SaveFailed后点重试 | 沿M02原Operation／ticket续办 | 失败期间仍是旧视图且无伤害动画 | 重试不重算随机；完成后只播一次。 |
| IP09 攻击CommitUnknown，同时点偏好／重来 | 只Lookup原提交，其他写被封 | 旧视图＋“确认保存”状态 | 不换ID；确定结果后按最新头恢复。 |
| IP10 播敌方事实时窗口关闭，随后A1旧回调到A2 | 无新业务意图，查询M02当前头 | 已有A2则只显示A2最新视图及其门闩 | 不回装／补演A1，旧令牌不解锁A2。 |
| IP11 演出中明确关闭道具，偏好先完成 | 独立偏好意图，目标false | 当前值仅据M04 H10变更；战斗仍播原条件 | 偏好通知不解战斗门闩，回退不改false。 |
| IP12 打开默认、当前合格、已失效攻略后开始拖线 | 无战斗意图；开始拖线即收起预览 | 三类标签／条件取M09记录，真实HUD取M06 | 重播不改局面；旧内容不冒充当前。 |
| IP13 最后行动Completed后重复结算请求 | 先显示WonPendingSettlement，再续唯一正常H06 | 封闭报告取M06，Settlement／BaseReward取M07 H10 | 重复返回原结算，不重抽／重发。 |
| IP14 Closed且基础已成、Bonus仅远端等待 | 选择适用下一关或地图是新明确意图 | 去向取M05／M02，基础到账取M07 | 远端等待不锁普通玩法；本机Bonus Unknown则封写。 |
| IP15 AwaitRescue中明确退出，保存前关闭页面 | 提交含Attempt／期望Scene的ExitAttempt | H10前仍显示原战场且无收益；Completed后取M02／M05去向 | 重到返回原退出结果，不变成重来或再返还。 |
| IP16 A1 Completed已展示后重复送达，当前头已A2 | 只查询A1原结果，无新播放意图 | 权威显示保持M02当前A2完整头 | 不重播A1、不回装旧HP／路线、不解A2门闩。 |

共同契约无需新增身份或修改权。技术试调只影响手势和表现参数；业务反例回M02／M06／M07的既有拒绝与缺项，不在M01补规则。
