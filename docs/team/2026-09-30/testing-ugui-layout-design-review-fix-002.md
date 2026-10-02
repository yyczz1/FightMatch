# LAYOUT-001 FIX-002 有限独立设计复审

2026-10-01（Asia/Shanghai） · 同一 R-TOOLS · design-only

**VERDICT: ACCEPT**

**Blocker count: 0。** FIX-001 唯一保留项 F01／原 B04／ART N04 的命令 Caption 容量在设计合同层面闭合。本结论仅接收本次设计修正；不代表产品实现、TMP 实际渲染、字体资源或 Unity 验证已通过。

**身份、权限与交付**

- Reviewer thread：`01a0f3e3-9663-7be2-84a8-ed23be32f0b9`；host：`local`。
- Reviewer turn：`01a0f579-4c41-7133-a210-88c625fcc7b5`。
- 派发／收件：主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb`；authority：用户 `AGENTS.md` §6 standing workflow 及本轮有限复审委派。
- 唯一新增文件：`docs/team/2026-09-30/testing-ugui-layout-design-review-fix-002.md`。候选、旧回执、testing.md、产品／测试源码、资源、配置与 meta 均未修改；未调用 Unity、build、test 或 Git。
- `final_message_id` 尚未产生，由 QA 从上述 thread/turn 的完成事件取得真实值；本文件不猜测 ID，也不内嵌自身哈希。

**固定输入身份与差异范围**

入口与写入前均核对固定身份。F 表示当前 correction 的一基行号；读取优先级仍为中央裁决 ＞ correction ＞ 未被替换的 base 条款。

| 输入 | lines | bytes | SHA-256 |
| --- | ---: | ---: | --- |
| F：`engineering-ugui-layout-system-design-correction-001.md`，本次候选 | 701 | 50752 | `9c3d6d60b1a9ed1743f0920cd80204fbe82f6558da35e5607e88bf88d988e540` |
| 同路径 FIX-001 preimage，内存中恢复的原始读取字节 | 682 | 46746 | `091b2e198b6674b9dceb7df27db9ed10bc405423ea45c1ee939d06aefb65c6f1` |
| `testing-ugui-layout-design-review-fix-001.md` | 125 | 16266 | `d58a4db2312a52dc3c6bda3f746b60afcc31685d60cfdb4eb2ae36de8156cc0b` |

候选 SYS turn：`01a0f572-138a-7b03-be3e-3a33be301d46`；SYS final：`msg_02dab7dbd2641d5a016abdd0f2283487d0b20cb12ce1273a46`。先前 R turn：`01a0f565-9ba6-7083-b062-a6b37b12f1ac`；先前 R final：`msg_0543862903425801016abdcf15644087d0bdc992bbea79adc2`。

preimage 由上一复审保留的完整分段读取在内存中重建，独立匹配原 lines／bytes／SHA 后用 Python difflib 比较；没有写回旧版或调用 Git。完整差异仅涉及 F:L242–246、L259–290 的 cell 宽度、容量证明与行为约束，L613、L623–624 的验收断言，以及 L5、L699 的待复审状态文字。

其余九项闭合结论与 ART N01–N03 复用旧回执；D1、26 路径白名单／第 27 路径 stop、Q0 及其余设计条款字节未变，未重新开展全案审查。以下复用输入也再次匹配：

| 复用输入 | lines / bytes | SHA-256 |
| --- | --- | --- |
| `central-layout-hierarchy-adjudication-001.md` | 69 / 7011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` |
| `engineering-ugui-layout-system-design-001.md` | 961 / 68763 | `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce` |
| `testing-ugui-layout-design-review-001.md` | 161 / 25145 | `ea0077d32238b0a09f898bf9a4630720cfa13daf41489f575b72a3fb17c308cc` |
| `art-ugui-layout-design-receipt-001.md` | 81 / 7567 | `098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f` |

**F01 容量复算与闭合依据**

按冻结 CSV 的实际 EN/ZH 字符串逐字符查询现有 TMP 字体 glyph 映射，包含空格，累计 HorizontalAdvance × 18 / 90；该字体 face scale=1，kerning pairs 与 glyph pair adjustment records 均为空。以下为 k=1，余量为 cell−24−max(ZH advance, EN advance)。每个所选宽度都等于最低需求向上取整到 8 的倍数，和 F:L267–280 一致。

| 命令 | 完整 ZH Caption / advance | 完整 EN Caption / advance | cell / 可用文字宽 | 最小余量 |
| --- | --- | --- | ---: | ---: |
| Retry | 重试原保存 / 90 | Retry Original Save / 159.450000 | 184 / 160 | 0.550000 |
| Resolve | 确认原保存结果 / 126 | Confirm Original Result / 195.165625 | 224 / 200 | 4.834375 |
| Skip（Battle） | 跳到当前战局 / 108 | Skip to Current State / 175.093750 | 200 / 176 | 0.906250 |
| Skip（Victory） | 跳过末段表现 / 108 | Skip Final Sequence / 168.843750 | 200 / 176 | 7.156250 |
| HistoryOpen | 行动记录 / 72 | Action History / 118.321875 | 144 / 120 | 1.678125 |
| Exit | 退出本局 / 72 | Exit This Battle / 125.065625 | 152 / 128 | 2.934375 |
| Restart | 按原开局重来 / 108 | Restart from Original Entry / 224.384375 | 256 / 232 | 7.615625 |
| Settle | 继续基础奖励结算 / 144 | Continue to Base Rewards / 219.681250 | 248 / 224 | 4.318750 |
| Reference | 播放参考 / 72 | Play Reference / 124.365625 | 152 / 128 | 3.634375 |

八个命令、Skip 两种状态均使用完整既有文案，单行、无换行点。Resolve 的七个汉字与 Settle 的八个汉字有足够容量；最长英文 Restart 也有 7.615625k 文字区余量。Retry 是最窄余量 0.55k，仍为正数；这只是度量证明，后续实际 TMP 断言不能省略。

字号 18，auto-size off，character/word spacing=0，glyph X/Y scale=1；左右各 12k、上下各 8k。文字区高度为 48−8−8=32k，字体单行高度 130.32/90×18=26.064k，剩余 5.936k；不需要第二行或缩高。F:L280、L289 明确禁止缩字、压字距／字形、减 padding、改短文案、换图标、截断或超出两行。所有值随既有 k 同比缩放，MainRow 保持 72k。

**整体算式、首尾可达与占位**

- MainRow：8+232+8+252+8=508k；成员区 232k 与命令 Viewport 252k 不变。
- Fixed：184+224+200+144+3×8=776k；Dynamic 最大值：152+256+248+152+3×8=832k。
- Content：776+16+832=1624k；最大横向滚动范围：1624−252=1372k。旧 64／280／576／324k 不再作为本节有效几何常量。
- 顺序固定为 Retry、Resolve、Skip、HistoryOpen、Exit、Restart、Settle、Reference。首格内容坐标 [0,184]，末格 [1472,1624]；offset=0 时首格在 Viewport [0,184]，offset=1372 时末格在 [100,252]，两端均整格可见。
- F:L285–290 保留 disabled 的完整 chosen width；每个按钮热区为 chosen width×48k，最小宽度 144k，满足 ≥48k×48k；在 72k 行内居中。滚动提示只可使用剩余 24k，不能侵占按钮高度；成员卡仍为 72k×72k。

Restart 的 256k cell 略宽于 252k Viewport；冻结要求是首 Retry 与末 Reference 整格可见，且所有命令完整文案可达。Restart 的实际英文 advance+左右 padding=248.384375k，小于 252k，文字可完整进入 Viewport；这不构成本轮额外 blocker，也不把整格同时可见扩成新增要求。

**L5b / L6 验收合同**

F:L613 明确 L5b 检查八个命令实际 EN/ZH 绑定及 Skip 两状态、完整单行文字、冻结 padding／宽度、首尾整格可达、disabled 占位和 ≥48k 点击高度。F:L624 要求 L6 controlled full 重复相同 Caption 断言、冻结 18 号字与 chosen width×48k 热区，并明确 focused 证据不能替代 controlled full。

新增断言未删除原有两种物理 safe rect、CanvasScaler、首帧、真实滚动交互、mask、非法布局取消或最终 native reopen 要求，也未改变测试集合、manifest 冻结顺序或执行资源边界。这里接收的是可供未来实施和验证的设计约束，没有运行测试或观察屏幕渲染。

**只读测量背景身份**

| 文件 | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv` | 64708 | `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c` |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` | 8573334 | `290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0` |

字体是只读容量度量来源；本回执不接收进行中的 FIX15 字体／资源状态，也不将观察到的资源身份当作 LAYOUT 实施 preimage。既有 ui-ugui skill 与 ScrollView 参考沿用上一复审所读版本。

**收件边界与验证回传**

F01 设计闭合，无需新增纠正包。主测试可将本回执交主程，供后续单独签发实施包使用。Q0 仍未解除：按本轮最新委派，实际前置链为 **FIX15 → 独立 R**；候选 D1 中未变的历史 FIX14 文本不构成重跑旧门或跳过当前前置的授权。本回执不启动 LAYOUT 实施、Unity、资源写入、设备／云测、发布或 Git。

本文件落盘后进行非 Git 静态 diff-check（空 preimage→唯一新增回执、UTF-8、结尾换行、尾随空白、缩进空格后 tab、冲突标记、多余 EOF 空行），并再次核对固定输入。具体 PASS 与本文件 path／lines／bytes／SHA-256 由本回合 final 返回；最终 message ID 由 QA 完成事件补取。回传后保持 idle。
