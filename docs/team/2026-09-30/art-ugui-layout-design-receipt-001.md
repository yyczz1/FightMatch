# ART-03：UGUI-LAYOUT-001 主美专业核对回执

2026-10-01 · 限布局设计接收

## 1. 身份、输入与边界

- 交付负责人：主美，thread `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` / host `local` / turn `01a0f53e-359c-7a23-b57b-a7bef07011e9`。
- 派发来源：中央AI，thread `01a0e401-511d-79f2-b47f-3ab0ade1681b` / host `local`。
- 唯一新设计输入：`docs/team/2026-09-30/engineering-ugui-layout-system-design-001.md`，961 lines / 68,763 bytes / SHA-256 `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce`。
- 对照输入保持只读：中央层级裁定 SHA-256 `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03`、ART-001 `art.md` SHA-256 `3b9a504345e509df5085688e1c897e7e737612a4db03f5bda770ab8315eb6e57`、ART-02 SHA-256 `d9209887a26e9e04e92f58a0704b7a7de91b96f962c3b45b5ca25bed91dbfcf8`、灰盒 README SHA-256 `33dea5d90064ec62ff6e015d00b157cd16c592b907e4e29f1099cd366d3346ff`。
- 本回执只核视觉层级、响应式几何、遮挡、角色卡／真实按钮容量与长文本视口；未运行 Unity、未检查或修改源码／Prefab／Scene／资源／Git，也不替代工程技术审查、独立 R、Mac、Android 或实际画面验收。

## 2. F1–F5 闭合检查

| ART-02 项 | 本设计已闭合部分 | 尚需的最小修正 |
| --- | --- | --- |
| F1 战斗五区与方形棋盘 | shared TopBar 只出现一次；BattlePage 固定 Stage、BattleStatus、BoardFrame、BottomHud；整页 BattleScroll 删除；Board 宽高同取 `B`；敌我与成员均有固定 0..2 槽 | NormalHud 的诊断滚动链与 MainRow 宽度合同仍未冻结，见 N-02、N-04 |
| F2 Reference 遮挡棋盘 | Reference 已迁到 BottomHud 互斥抽屉；根背景不参与 raycast；Board rect 不变，首个真实拖动先关闭抽屉 | Close 仍可落在独立 Actions，最小高度下会挤空正文，见 N-03 |
| F3 普通弹窗 | 已分为全屏 RootMask、居中 Panel、固定 Header/Actions、正文专用 ScrollRect；宽度 `min(476,safeW-64)` | 设计结构闭合；中英最长正文仍须按 Q4 实际截图验证，不能提前转成视觉通过 |
| F4 保存恢复层级 | Recovery 已进入 SystemLayer 完整页面，普通 Popup 先关闭／失去交互；安全／危险动作分区；Header/Actions 固定、Body 局部滚动 | 设计结构闭合；运行优先级与正文容量留待实现证据 |
| F5 TopBar 与纵向预算 | 旧 88-unit ScreenLayer 扣减删除；TopBar 由 HudLayer 共享；Stage-first 压缩、BottomHud 下限和 `H<812k` 可见失败均与已接收几何表一致 | 1080 长屏示例使用了错误的安全区纵向原点，见 N-01 |

## 3. 最小修正项

### N-01 · 1080×2400 安全区样例原点与灰盒不一致

- **设计现状：** §4.4 与 Q4 把物理 safe rect 写为 `(32,80,1016,2160)`，即 bottom 80 / top 160（设计 `329`、`848` 行）。
- **冻结灰盒：** README §3 使用 `x=32, y=120, w=1016, h=2160`；上下各 120，左右各 32。
- **最小修正：** 两处统一改为 `(32,120,1016,2160)`，insets 写为 left/right 32、bottom/top 120。`W/H`、`sqrt(5)`、`k`、五区物理高度 112/332/96/1016/284、上下核心留白各 160 均不需要重算。
- **理由：** 当前差异不会改变宽高公式，却会让截图中的整块安全内容相对已接收灰盒垂直偏移 40 px，不能用“同尺寸”掩盖不同的四边 inset 样例。

### N-02 · NormalHud 诊断区声称局部滚动，但精确层级没有 ScrollRect 链

- **设计现状：** §0/§1 要求 diagnostic text 只在局部 viewport 滚动；精确层级却是 `StatusViewport/Content`（设计 `125–129` 行），字段路径也固定到该两级路径（`465`、`484`、`515` 行）。§8 只写“as needed”，不足以形成可静态检查的所有权。
- **最小修正：** 冻结为：

  ```text
  StatusViewport                  [vertical ScrollRect]
  └── Viewport                   [Image + Mask]
      └── Content                [VLG + vertical CSF]
          ├── Save
          ├── Availability
          └── PlaybackDiagnostic
  ```

  同步把所有字段路径改为 `StatusViewport/Viewport/Content/...`，明确 ScrollRect 的 `viewport` 与 `content` 引用、只开 vertical；Viewport/Image 仅为本地滚动接收 raycast，不得越出 BottomHud。
- **理由：** 否则 PlaybackDiagnostic 超长时既没有裁切所有者，也没有标准本地滚动路径；实现者可能再次用 ContentSizeFitter 推高 BottomHud 或把文字盖到 MainRow／棋盘。

### N-03 · Reference 抽屉在 BottomHud 下限时必须固定 Close 所在行

- **设计现状：** 抽屉有 `Header + BodyViewport + Actions`，但 §6.5 允许 `closeButton` 在 `Header/Close` **或** `Actions/Close`（设计 `148–159`、`528–535` 行）。BottomHud 最小仅 104k；若 Header 与独立 Actions 各保留一个 48k 热区，正文只剩 8k，无法显示条件／差异／步骤。
- **最小修正：** 将唯一固定 Close 冻结到 `Header/Close`，Header/控制条高 48k；Reference 的 `Actions` 默认折叠为 0，步骤／差异控件留在可滚 Body Content。这样正文在 BottomHud 142k 时有 94k，在最小 104k 时仍有 56k。若现有业务确有不可移入 Header/Body 的独立 48k 动作，则必须提高 BottomHud 下限并同步重算 fail-closed 边界，不能覆盖 Board。
- **理由：** 这与灰盒“控制条不超过 48、正文抽屉替换 BottomHud、不盖棋盘”一致，也消除实现时两种互不兼容的合法路径。

### N-04 · 三张角色卡与四个固定动作缺少可证明同排的宽度合同

- **设计现状：** `MainRow` 精确列出三个 MemberSlot 和 Retry/Resolve/Skip/HistoryOpen 四个动作，且只冻结 72k 行高与 48k 热区；未冻结横向 padding、各组最小宽、8k 间距或英文换行。仅凭层级不能证明七个真实交互项在 `B=508k` 内不挤压。
- **最小修正：** 对所有 `k` 冻结以下下限，剩余空间可弹性分配但不得削减：

  ```text
  MainRow width                    = 508k
  left/right padding               = 16k + 16k
  3 MemberSlot minimum             = 3 × 72k
  member internal gaps             = 2 × 8k
  group gap                        = 8k
  4 Action minimum                 = 4 × 48k
  action internal gaps             = 3 × 8k
  required total                   = 488k <= 508k
  remaining flexible width         = 20k
  ```

  MemberSlot 保持 72k 高；Action 在 72k 行内垂直居中，热区至少 48k×48k。英语／简中 Caption 用冻结文案实测，允许受控两行，但不得低于既定最小字号、截断动作语义或依赖未采用图标。若业务状态保证少于四个动作同时显示，仍需把互斥条件写入设计与测试，不能让实现者猜测。
- **理由：** 这是把当前层级转成可验收画面的最小横向约束；不新增动作、不采用最终画风，也不改变角色槽语义。

## 4. 专业结论

**Verdict: `NEEDS_FIX`**

五区、方形棋盘、共享 TopBar、Reference/History 的 BottomHud 互斥方向、System Recovery 与普通弹窗结构已经正确关闭 ART-02 的主要架构缺陷。修正仅限 N-01～N-04 的样例坐标、局部滚动链和内部容量合同；无需重开业务语义、画风、角色资源或总体响应式公式。

修订设计通过有限复核后，仍只能进入实施／独立 R 路径。实际 Prefab、Unity 首帧、Board 命中、Raycast、中英文案、Mac 与 Android 画面均保持未验证。
