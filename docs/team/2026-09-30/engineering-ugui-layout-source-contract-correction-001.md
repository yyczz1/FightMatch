# LAYOUT-S1 合同最小更正001

2026-10-02 · `READY_FOR_ENGINEERING_ACTIVATION`；SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，实际turn `01a0fa78-3c90-76d3-b05b-0873972c5aec`；唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。
仅回答主程转来的两项合同冲突；C turn `01a0fa6a-62bb-7c32-b577-4d268818b774`已有源码WIP，本文不评审/覆盖它。主程接回并明确激活后，以下覆盖才约束冲突部分；其余原合同、15文件/7,840及逐文件预算不变。

原始文件SHA256（本轮核对一致；均在同目录，保持原字节）：
- `engineering-ugui-layout-source-activation-system-001.md`：`3220629d60fdb279e9328353d5a90156f0abc66415cf24bf1e6da42d4beba06d`
- `engineering-ugui-layout-source-activation-inputs-001.json`：`5a2f1394e2b6fe03fdeaf7cb4d4a54a23f5b6f493d33b4dcf430e31f899a690d`
- `engineering-ugui-layout-source-activation-architecture-001.md`：`83a359f352cbc0ab953ab25179f6378529ec5334fc338e9e25419d8b0b5c7f84`

## 1. 槽位路径：转录笔误更正，不改行为

依据[base §3](engineering-ugui-layout-system-design-001.md)的冻结树及§6.3固定槽契约：保留`Stage/AllySlots/ActorSlot0..2`、`Stage/EnemySlots/ActorSlot0..2`；correction §4的槽位简称不删除分组。以下为原inputs的**虚拟覆盖表**，不改写该文件；每个old须精确相等，`0..2`按索引展开，路径自BattleContent起。
```json
[
  {"pointer":"/serializedContract/CandidateBoardInputView/add/allyStageSlots","old":"RectTransform[3]@Stage/AllySlot0..2","new":"RectTransform[3]@Stage/AllySlots/ActorSlot0..2"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/enemyStageSlots","old":"RectTransform[3]@Stage/EnemySlot0..2","new":"RectTransform[3]@Stage/EnemySlots/ActorSlot0..2"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/allyNames","old":"LocalizedTmpText[3]@AllySlot0..2/Name","new":"LocalizedTmpText[3]@Stage/AllySlots/ActorSlot0..2/Name"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/allyHp","old":"LocalizedTmpText[3]@AllySlot0..2/HP","new":"LocalizedTmpText[3]@Stage/AllySlots/ActorSlot0..2/Hp"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/allyIntent","old":"LocalizedTmpText[3]@AllySlot0..2/Intent","new":"LocalizedTmpText[3]@Stage/AllySlots/ActorSlot0..2/Intent"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/enemyNames","old":"LocalizedTmpText[3]@EnemySlot0..2/Name","new":"LocalizedTmpText[3]@Stage/EnemySlots/ActorSlot0..2/Name"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/enemyHp","old":"LocalizedTmpText[3]@EnemySlot0..2/HP","new":"LocalizedTmpText[3]@Stage/EnemySlots/ActorSlot0..2/Hp"},
  {"pointer":"/serializedContract/CandidateBoardInputView/add/enemyIntent","old":"LocalizedTmpText[3]@EnemySlot0..2/Intent","new":"LocalizedTmpText[3]@Stage/EnemySlots/ActorSlot0..2/Intent"}
]
```
大小写统一为`Name/Hp/Intent`：`Hp/Intent`沿用base冻结树，`Name`沿用SYS已列字段目标；字段名allyHp/enemyHp不变。MemberStrip/memberSlots、OriginalSlot语义及其它指针一概不动。

## 2. k：唯一实施坐标口径，不新增字段或行为

依据base §4.1–4.4的Canvas单位几何、§5.1的RectTransform写权限与§5.3单轴owner，以及[correction §5.2](engineering-ugui-layout-system-design-correction-001.md)“reference typography…scale with…geometry”、§8动作容量、§9 TopBar避让：允许**已引用子树根统一缩放一次**，不要求每个本地rect/fontSize再乘k。此处澄清坐标表示，不修改玩家看到的尺寸、文字或热区。
Responsive仅对现有typed字段`topBar, stage, battleStatus, startupViewport, navigationViewport, resultViewport, normalStatusViewport, normalMainRow, historyDrawerRoot, referenceDrawerRoot, dialogPanels[0..6]`赋`localScale=(k,k,1)`；每条后代路径最多经过其中一个根。禁止另建缩放包装节点、按名字查找或新增serialized/public字段。
其余布局节点localScale保持`(1,1,1)`，特别是RuntimeRoot、SafeAreaRoot、Screen/Hud/Popup/System层、页面根、battleContent、boardRegion/BoardFrame/BoardDecoration、bottomHud、normalHudRoot、共享外根/RootMask、LayoutDiagnostic、TemplatePool及上述缩放根的后代。RuntimeCanvas的变换由Unity/CanvasScaler管理，Responsive禁止写它，也不能断言其实际lossyScale为1。
先用SafeAreaRoot实际rect求`W,H,B=min(W,508),k=B/508`，校验有效后算**Canvas单位目标矩形**。上述缩放根本地宽高=`目标宽高/k`；其父链相对Canvas无k缩放，位置/边距/anchoredPosition按父Canvas单位计算，不再除/乘k。使用明确anchor/pivot和显式本地尺寸，不能把stretch已算出的尺寸再当参考尺寸；每次绝对赋值，不累乘旧scale。
例如MainRow本地`508×72`、Command viewport252/content1624、按钮宽`184/224/200/144/152/256/248/152`高48、padding12/12/8/8、TMP fontSize18及字形自身XY比例1，继承根k后才得到原合同Canvas值；子按钮/字体/spacing/padding不得再乘k。Stage短高只改目标rect，绝不额外压缩k或热区；History/Reference本地Header48、Reference Body94/56、Actions0保持。
BoardRegion与BoardFrame不走子树缩放，实际本地rect均为`B×B`（后者stretch零offset），render/hit仍同源。Panel目标Canvas宽`min(476,W-64)`、最大高`H-64`，再各除k写本地rect；32边距和64总扣除不变成32k/64k。Body Canvas下界仍`H-64-48k-336k≥48k`；普通页TopBar扣减只一次，所有根位置按原§9公式。
单轴owner/ScrollRect→Viewport→Content不变：Responsive不同时写CSF/LayoutGroup驱动的后代轴；子树内布局在参考单位工作，滚动content与viewport继承相同k，不能单独缩放Content。无效首次/resize保留最后有效rect并fail-closed，不能用缩放绕过812k或宽度/弹层容量门。
后续实际Overlay断言须同时记录local rect、Canvas坐标投影及物理像素，不把本地48误认48k：两safeRect仍为540×960 `(16,16,508,920)`及1080×2400 `(32,120,1016,2160)`；CanvasScaler启用540×960/match0.5，后者`k≈0.894427191, scaleFactor≈2.236067977`，48热区投影`48k≈42.9325` Canvas单位/96px。保留首帧、resize、48k热区、完整双语Caption、首末滚动可达、raycast和无重复k断言；没有新增测试轮或放宽断言。

本轮仅新增本文并核对格式/覆盖old值/文件hash；原三文件与S证据不修改，不重跑133 preflight，不读审C补丁、不重置WIP。未运行Unity、dotnet、测试、Git写操作或联网；无新字段需求。uGUI技能用于单轴/滚动坐标约束，codebase-design用于维持原typed引用边界；不派C/QA/R，不创建复审轮。
