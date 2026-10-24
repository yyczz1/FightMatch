# ART-05：N-04T 文字容量有限复核回执

2026-10-01（Asia/Shanghai）· design-only

## 身份与范围

- 主美：thread `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` / host `local` / turn `01a0f583-fc25-7f71-a083-42f5bb0f08b4`；派发：中央AI thread `01a0e401-511d-79f2-b47f-3ab0ade1681b`。
- 候选：`engineering-ugui-layout-system-design-correction-001.md`，701 lines / 50,752 bytes / SHA-256 `9c3d6d60b1a9ed1743f0920cd80204fbe82f6558da35e5607e88bf88d988e540`。
- 独立 R：`testing-ugui-layout-design-review-fix-002.md`，89 lines / 8,751 bytes / SHA-256 `8c20bfd3a8dff10f678cdea4b821a716cfc7115d5864b866d45c087f3e499e32`；R turn `01a0f579-4c41-7133-a210-88c625fcc7b5`。
- 本回执只复核 ART-04 N-04T；不重审 N-01～N-03，不修改旧回执，不运行 Unity，也不接收实际滚动操作或最终视觉效果。

## N-04T 关闭判断

- §5.2 已按冻结 EN/ZH Caption 与 `NotoSansCJKsc-Regular-TMP.asset` 实际 glyph advance 逐项给出 Retry、Resolve、Skip 两状态、HistoryOpen、Exit、Restart、Settle、Reference 的文字宽度。合同冻结 `font size=18`、auto-size off、字／词距 0、X/Y scale 1、左右 padding 各 12k、上下各 8k，且禁止缩字、压字距／字形、减 padding、改短文案、截断或用未批准图标规避。
- 八个 chosen preferred widths 依次为 `184/224/200/144/152/256/248/152k`。此前超限的“确认原保存结果”和“继续基础奖励结算”分别使用 224k、248k 单元，完整单行成立；英文最坏宽度也包含在同一合同内。
- 动作组与滚动内容已随真实单元宽度重算：Fixed `776k`、Dynamic `832k`、组间 `16k`，Content `1624k`，252k Viewport 的最大滚动范围 `1372k`；禁用项保留完整宽度，首 Retry／末 Reference 整格可达。§11.8 与 §11.9 又将真实双语绑定、单行、padding、chosen width、首尾可达和热区写入后续运行验收门。

**Verdict: `ACCEPT`**

N-04T 在设计合同层面闭合，ART-04 的该项不再需要 SYS 修正。此接收不代表 TMP 实际渲染、滚动手感、中间超宽单元的视觉可用性或设备画面已通过；这些仍须在 Unity 实现后凭截图／录屏及既定运行证据另行专业验收。
