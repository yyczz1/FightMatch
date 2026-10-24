# ART-04：UGUI-LAYOUT-001 修订设计主美复核回执

2026-10-01 · 限 ART-03 N-01～N-04 关闭复核

## 1. 身份、固定输入与边界

- 交付负责人：主美，thread `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` / host `local` / turn `01a0f56c-7d41-71b3-926f-06a7ca9c36e3`。
- 派发来源：中央AI，thread `01a0e401-511d-79f2-b47f-3ab0ade1681b` / host `local`。
- 组合设计基线：原设计 `engineering-ugui-layout-system-design-001.md`，68,763 bytes / SHA-256 `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce`；修订 `engineering-ugui-layout-system-design-correction-001.md`，46,746 bytes / SHA-256 `091b2e198b6674b9dceb7df27db9ed10bc405423ea45c1ee939d06aefb65c6f1`。两者按修订 §0.3 的覆盖规则共同构成实施合同。
- 固定裁定／问题输入：`central-layout-hierarchy-adjudication-001.md` SHA-256 `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03`；ART-03 `art-ugui-layout-design-receipt-001.md` SHA-256 `098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f`。
- 本回执只判断修订设计是否关闭 ART-03 N-01～N-04。未运行 Unity，未检查或修改源码、Prefab、Scene、资源、配置、`.meta` 或 Git；不替代工程技术 R、实现后截图验收、Mac／Android 验证，也不授权扩大制作范围。

## 2. N-01～N-04 关闭核对

| 项 | 修订合同中的关闭证据 | 主美判断 |
| --- | --- | --- |
| N-01 长屏 safe rect 原点 | 修订 §10.2 将 1080×2400 的 `Screen.safeArea` 唯一冻结为 `(32,120,1016,2160)`，四边 inset 为 `32/120/32/120`；并明确删除旧 `y=80` 表达。`W=1016`、`H=2160`、`B=1016`、`k=2` 与五区高度不变。 | 已关闭。示例坐标与灰盒一致，不再产生 40 px 纵向偏移。 |
| N-02 NormalHud 局部滚动链 | 修订 §3.1～§3.3 冻结 `StatusViewport [vertical ScrollRect] → Viewport [Image + Mask] → Content [VLG + vertical CSF]`；同时冻结 `viewport/content` 引用、仅 vertical、Content 顶部锚定与“不推高父容器”。状态行均位于 Content 内，`MainRow` 为固定兄弟节点。 | 已关闭。裁切、滚动与尺寸所有权完整，诊断长文不能越界推高 BottomHud 或覆盖棋盘。 |
| N-03 Reference 最小高度 | 修订 §6 将唯一 Close 固定在 48k Header，`Actions` 高度严格为 0 且不参与 raycast；Body 是独立纵向 ScrollRect，在 BottomHud 142k／104k 时分别得到 94k／56k 可见高度。 | 已关闭。Close 始终可达，最小高度下正文仍有非零可见视口，且抽屉不占 BoardRegion。 |
| N-04 三卡与全部动作容量 | 修订 §5.1 明确枚举 3 个成员选择、4 个固定动作 Retry／Resolve／Skip／HistoryOpen，以及 4 个动态动作 Exit／Restart／Settle／Reference。§5.2 冻结外层 `8 + 232 + 8 + 252 + 8 = 508k`；成员区为 `3×72k + 2×8k = 232k`；CommandStrip 是 252k 宽、72k 高的水平 ScrollRect，Content 最坏为 `280 + 16 + 280 = 576k`，可滚范围 324k。禁用固定动作仍占位，动作热区至少 64k×48k 并允许中英受控两行。 | **仅布局／滚动链部分关闭。** 三卡固定可见、8 个动作可滚达已经证明，但固定 64k 单元不能证明冻结长文案可读，见 N-04T。 |

## 3. 组合画面判断

- 方形 BoardRegion 仍使用 `B=min(W,508)`、宽高相等；NormalHud 与 Reference 都只替换 BottomHud 内部内容，不改变棋盘 rect，也不把页面改为整页滚动。
- TopBar 继续由 HudLayer 共享且只计一次；Stage、BattleStatus、Board、BottomHud 的预算与已接收的 `920k` preferred、`812k` fail-closed 边界不变。
- StatusViewport 负责纵向状态长文，CommandStrip 负责横向动作集合，两条滚动轴分属不同局部视口；成员卡保持固定可见。该分工足以证明各容器不会互相挤占，但不证明每个动作单元内部的冻结文字都能容纳。
- 滚动手感、首帧位置、Mask／Raycast 与真实设备显示仍须由实现证据验证；但冻结文案在当前尺寸合同下的确定性不容纳不是运行时抽查事项，必须先在设计中闭合。

## 4. 唯一剩余修正项

### N-04T · 动作总数可滚达，但冻结文案没有可容纳的宽度合同

- 修订 §5.2 把每个动作单元按固定 `64k × 48k` 计入 Content，并只允许最多两行、不得降低已接收最小字号、截断语义或依赖未批准图标。
- 在 `k=1`、18 号字且尚未扣除水平 padding 的有利上界下，64k 每行最多容纳 3 个全宽汉字，两行最多 6 个。冻结文案“确认原保存结果”为 7 字，“继续基础奖励结算”为 8 字，因此当前合同不能容纳；水平 ScrollRect 只解决按钮总数超过视口，不能解决单个按钮内部文字溢出。
- SYS 必须基于冻结中英文本与既定最小字号，为这些真实控件明确 `minWidth`／`preferredWidth`、水平 padding、允许行数及 Content 重算规则，并证明 `k=1` 最坏文本成立。可采用大于 64k 的按文案 preferred width 单元并相应增加水平滚动内容宽度；不得通过改冻结文案、降字号、语义截断或未批准图标规避。
- 修订后应同步更新固定／动态动作组的最坏 preferred width 与滚动范围，并让实现测试按真实 TMP 字体度量及中英冻结文案验证。现有 3×72k 成员区、252k CommandStrip viewport、局部滚动所有权与 BoardRegion 不需重开。

## 5. 专业结论

**Verdict: `NEEDS_FIX`**

修订设计已关闭 N-01、N-02、N-03，并关闭 N-04 的成员卡／动作数量／局部滚动部分；但 N-04T 的单按钮文字容量尚未闭合。请由原 SYS 仅补齐真实中英文案的宽度、padding、行数和 Content 重算合同，再交主美有限复核。该修正不要求重开棋盘、成员卡、滚动层级、业务语义或画风，也不授权先行实施当前有缺口的按钮尺寸合同。
