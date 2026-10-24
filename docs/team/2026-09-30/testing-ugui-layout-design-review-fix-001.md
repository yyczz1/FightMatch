# LAYOUT-001 FIX-001 有限独立设计复审

2026-10-01（Asia/Shanghai） · 同一 R-TOOLS · design-only

**VERDICT: NEEDS_FIX**

**Blocker count: 1。** 首次 B01–B10 中九项在设计合同层面闭合；B04／ART N04 的命令可达性与外层容量已修正，但单个按钮的固定文案容量仍与其两行／最小字号约束不相容。下述 F01 是本次唯一 blocker。不重开中央架构，不扩展产品实施范围。

**身份与权限**

- Reviewer thread：`01a0f3e3-9663-7be2-84a8-ed23be32f0b9`；host：`local`。
- Reviewer turn：`01a0f565-9ba6-7083-b062-a6b37b12f1ac`。
- 派发／收件：主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb`；主程经主测试委派；用户 authority 为 `AGENTS.md` §6 standing workflow。
- 唯一新增文件为本回执；没有修改候选、prior review、ART、testing.md、产品／测试源码、Prefab、Scene、资源、配置或 meta。没有 Unity、build、test 或 Git 写操作。
- `final_message_id`：写入本文件时尚未产生，由 QA 从本 thread/turn 的完成事件取得真实 final message ID；不预填、推测或使用其他回合的 ID。

**固定输入身份门**

以下五项在任何候选内容审查前逐项核验成功，写入前再次匹配。读取优先级为 C ＞ F ＞ D 中未被明确替换的条款；R0 与 A3 仅作 finding evidence。后文 `F:Lnn`、`D:Lnn` 指下表所列冻结文件的一基行号。

| 简称／文件 | lines | bytes | SHA-256 |
| --- | ---: | ---: | --- |
| C：[central-layout-hierarchy-adjudication-001.md](central-layout-hierarchy-adjudication-001.md) | 69 | 7011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` |
| D：[engineering-ugui-layout-system-design-001.md](engineering-ugui-layout-system-design-001.md) | 961 | 68763 | `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce` |
| F：[engineering-ugui-layout-system-design-correction-001.md](engineering-ugui-layout-system-design-correction-001.md) | 682 | 46746 | `091b2e198b6674b9dceb7df27db9ed10bc405423ea45c1ee939d06aefb65c6f1` |
| R0：[testing-ugui-layout-design-review-001.md](testing-ugui-layout-design-review-001.md) | 161 | 25145 | `ea0077d32238b0a09f898bf9a4630720cfa13daf41489f575b72a3fb17c308cc` |
| A3：[art-ugui-layout-design-receipt-001.md](art-ugui-layout-design-receipt-001.md) | 81 | 7567 | `098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f` |

本轮复用已读 `unity:ui-ugui` 与 ScrollView 参考。对实际代码、冻结文案、字体 YAML、已有 full-name manifests 进行了只读核对和静态算术／集合检查；未运行 TMP 渲染或 Unity 测试，以下闭合仅表示设计合同闭合。

**逐项 closure verdict**

| 首次项 | Closure verdict | combined candidate 证据与判定 |
| --- | --- | --- |
| B01 | CLOSED（design） | F:L65–L86 用 BoardRegion 包含独立 Decoration Image 和唯一 CandidateBoardElement Graphic，BoardFrame zero-offset stretch，输入仍持有 CandidateBoardElement。F:L118–L152 将 hud／availability 首行等 LocalizedTmpText 与 battleNotices、Actions 等 RectTransform 容器分开；不再把 Content 当 label 或克隆整组 Content。 |
| B02 | CLOSED（design） | F:L170–L187 分开 ally 0..2、首发敌人 {0,1}、三槽显示容量和 enemy nonnegative 领域契约。合法 slot>2／超出容量明确为 presentation-only `UnsupportedEnemySlotLayout`，保留 snapshot/Core/save/slot/route，使用既有文案；Exit 在既有 lifecycle 允许时可达。合法 7/2、8 的测试输入保留，不能改 Core 验证。 |
| B03 | CLOSED（design） | F:L92–L116 给出完整 vertical ScrollRect→Viewport(Image+Mask)→Content(VLG+vertical CSF) 与准确 viewport/content refs。F:L154–L164 冻结 MainRow=72k、padding=8k、status=62k／24k；Content 不得撑高 BottomHud。 |
| B04 | NOT_CLOSED | F:L193–L223 已列全 Retry/Resolve/Skip/HistoryOpen/Exit/Restart/Settle/Reference 八个命令，状态行移出 Actions。F:L230–L276 的水平 CommandStrip 解决外层溢出和可达性；但固定 64k×48k cell、最多两行与冻结 EN/ZH 文案不可同时成立，见 F01。 |
| B05 | CLOSED（design） | F:L282–L308 固定唯一 Header/Close，Header=48k、Actions=0，Body=94k／56k；没有继续保留 D 的 Header 或 Actions 两种选择。保留首拖关闭与 pointer/feedback 顺序。 |
| B06 | CLOSED（design） | F:L314–L345 冻结 Host 唯一 outer root/mask writer、八个准确 refs、子视图仅写自有 panel、Host 在 child renders 后汇总。F:L349–L362 覆盖 enter/switch/cancel/Return/普通页/locale/Unbind/reopen；双 panel 同时 active 为 diagnostic。F:L364–L372 将 OriginalResult 改为当前 panel Body 中唯一动态 clone，保留原 ID／动作和 Confirmation、Recovery/CommittedResult 可达性，不 reparent。实际实现必须建立该 render 顺序，不能假定现有事件顺序已满足，见非阻断落实项 N1。 |
| B07 | CLOSED（design） | F:L376–L406 将 0..N candidates／original-operation choices 和 OriginalResult 放到 Body/ChoiceRows；fixed execution actions 留在外部 Safe/DangerActions。父 Content VLG 不控制 section 高度，section 各自 CSF，不再同轴双写。F:L408 冻结最多六个 fixed actions 的 336k 预算及 Body>=48k 的 fail-closed 条件；F:L410–L412 规定完整动态行清理范围。 |
| B08 | CLOSED（design） | F:L416–L443 明确 Startup/Navigation/Result 的实际 ScrollRect outer refs 和 top/bottom/width/height 唯一 writer，Content VLG/CSF 只控内容高度；Battle 单独消费 TopBar 一次，不叠加通用 top deduction。 |
| B09 | CLOSED（design） | F:L445–L475 将 long safe rect 统一为 (32,120,1016,2160)，区分 120 px safe insets 与 160 px core breathing；540 SVG yTop=24 到 Unity yBottom=16 的转换也明确。 |
| B10 | CLOSED（design） | F:L483–L613 的依赖图先 compile/import、native construct/save/reopen，再 freeze manifests 和资源；39＋8 focused 后执行 234 union，最后 native reopen/audit。Q2=20 methods/32 instances、WorldSpace 证据局限和真实 Overlay 顶层 PlayMode 生命周期均明确。26 文件白名单与第27路径 stop gate 在 F:L617–L659；D1 与 active FIX14 一致，见下文独立对照。 |

| ART 项 | Closure verdict | 证据映射 |
| --- | --- | --- |
| N01 | CLOSED（design） | F:L457–L475，长屏原点及 safe inset/core breathing 分离；对应 B09。 |
| N02 | CLOSED（design） | F:L90–L164，完整 NormalHud ScrollRect 链及固定 MainRow；对应 B03。 |
| N03 | CLOSED（design） | F:L282–L308，单一48k Header Close，Actions=0、Body=94k／56k；对应 B05。 |
| N04 | NOT_CLOSED | F 正确拒绝只算四个动作的488k方案，并用水平滚动容纳全部八命令；单 cell 的 EN/ZH 实际文本仍装不下，因此该项未完全闭合；对应 B04／F01。 |

**F01 · blocker · P1：64k 两行命令 Caption 无法容纳冻结文案**

- 准确候选定位：F:L242–L246、L259–L264、L269–L274；D:L865 的热区边界继续有效，C:L49–L50 保留文案与可读性约束。
- F:L274 同时要求 Caption 在 64k×48k 内最多受控两行、不得低于 accepted minimum font size、不得截断语义或依赖未批准图标。当前已采用字体与 `art.md:82–L83` 给出的最小正文18、主按钮建议22和实际文案测量要求不支持用更小字号硬塞。下面取更宽松的18号、k=1、零 padding，而不是以22号加大反例。
- 冻结 CSV `snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv:114,122,190` 分别为「确认原保存结果 / Confirm Original Result」「继续基础奖励结算 / Continue to Base Rewards」「按原开局重来 / Restart from Original Entry」。它们分别是 Resolve、Settle、Restart 的现有键，不是人为加长的压力文案。
- 只读字体数据：Noto Sans CJK SC 的 `m_PointSize=90`、`m_Scale=1`；上述中文所需 glyph 的 `m_HorizontalAdvance=90`，在18号即每个全宽字18单位。故 64 宽每行最多 floor(64/18)=3字，两行最多6字；Resolve 为7字、Settle为8字。即使不计 padding，这两项也至少需要三行，已经违反候选约束。

| 现有命令 | 英文非空格 glyph advance 总和，18号 | 两行64宽的总预算 | 说明 |
| --- | ---: | ---: | --- |
| Resolve：Confirm Original Result | 187.1031 | 128 | 按当前字体 advance 求和，尚未计空格、边距和单词换行损耗 |
| Settle：Continue to Base Rewards | 207.5875 | 128 | 同上 |
| Restart：Restart from Original Entry | 212.2906 | 128 | 同上 |

这些是字体表与冻结字符串的静态容量反例，不是声称执行了 TMP 画面测试。中文完整字符容量已独立足以否定当前64宽两行合同；英文也不能仅凭“支持换行”视为已满足。

影响：水平 ScrollRect 只增加八个 cell 的总可达范围，不会增加单个 cell 的文字宽度。按候选照做只能多于两行、截断、压缩字体／字距或让 Caption 越出按钮；这些均未被本候选允许。它仍不能作为无需补设计的实现输入。

最小修正方向：保留已正确的 MemberStrip、固定252k CommandStrip viewport和八命令语义，基于这八个真实 EN/ZH Caption、既定最小字号、必要 padding 和正常字距冻结足够的 cell 宽度／Caption 约束，再同步修订 F:L242–L264 的两个 action-group preferred width、总 Content width 和 scroll range。已有横向滚动可以承接更宽的命令内容，无需增加 Canvas、改业务入口或引入替代架构。若选择调整行高或行数，也必须明确重算72k MainRow及 BottomHud 下限，不能让实施者暗改。不要修改本地化表、缩短语义、缩小字体或压扁字形来使旧算式成立。

修订后的设计须提供八命令 EN/ZH 逐项容量表；实际 TMP 两语言画面、首尾 cell 可达、禁用态仍占位、完整 Caption 与48k以上热区仍留在原 L5b/L6 实施验证门内。本轮不运行这些门。

**非阻断落实项与独立复核结果**

- N1：Host 唯一 owner 及“child renders 之后再汇总”已经形成明确设计合同，具体内部通知／渲染调用属于获授权实现。当前 `FightMatchHostSession.cs:74–L78,293,312` 先订阅 controller Changed 并转发 session Changed；视图随后订阅，不能假定现有广播天然使 Host 最后执行。实施须在现有白名单内落实 F:L335 的顺序并以 L5b outer-mask 用例证明；不能为此修改未列入的 HostSession/PlayerNavigationView，若确需第27路径即停。此项不再重复计为一个设计 blocker。
- N2：B01/B03/B07/B08 的每个 responsive-owned 轴和 scroll-content-owned 轴可以分别落到单一所有者。普通 vertical Content 的 VLG＋CSF 和 Command Content 的 HLG＋horizontal CSF 本身不是双写；Section CSF 与父 Content 的 `childControlHeight=false` 已明确。所有 CSS/USS、第二 Canvas/EventSystem 和领域接口替代方案均不在本候选中。
- N3：Base 的 Reference 首拖、所有权/epoch/generation、幂等退订及 FIX14 managed/native teardown 基线要求仍须承接；F 没有授权放宽它们。共享 root refs 和新的跨层 row containers 必须同步生成器及 fixtures，不能用旧后代遍历范围假定已清理。真实生命周期、首帧、零占位和 EN/ZH 渲染尚未执行，这不是设计闭合即实现通过。
- N4：SafeAreaFitter.Changed/Version 的 internal 可访问性、Presentation 与测试的 assembly 关系沿用首次已核事实；F 未要求 public 化、改 SafeAreaFitter 或 asmdef。独立 Decoration 仍只有一个玩家 Canvas／GraphicRaycaster；Scene 唯一 EventSystem 与稳定资源 GUID 的约束保留。

**D1 与 active FIX14 的逐项对照**

本次只读核对 `engineering-ugui-01-q4-managed-lifecycle-fix-14.md`：199 lines / 12822 bytes / SHA-256 `9ef4285b8c9a9d56bfb95542b40512cb364064c12bbe33a5e1b7d37ab3a362ba`。该文件是 active correction 的只读执行合同，不能替换五份固定设计输入。

| D1 要求 | FIX14 对照 | 判断 |
| --- | --- | --- |
| remaining exact2，仅 P27、B09 | FIX14:L152–L169 | 与 F:L518 一致；不是重跑 exact4。 |
| exact222，覆盖 P27/B09/B16/WIRE01，fixed28 为其已通过子集 | FIX14:L171–L180 | 与 F:L518 一致；六个参数化 case 的 proof 仍须保留，不另跑 focused28。 |
| native final reopen 在222之后 | FIX14:L182–L189 | 与 F:L518 一致，资源／字体／过程身份必须审计。 |
| 作者交主程、QA再派同一独立R | FIX14:L191–L199 | 与 D1 独立接收依赖一致；作者状态不是独立验收。 |

Q0／D1 当前仍未接受；FIX14 active。F:L46、L487 对 FIX13 2 pass／2 fail 的记录不能当作产品接收。即使后续设计复审通过，也必须等待 FIX14 exact2→222→native reopen→独立R全部完成，之后才可由主程另签 LAYOUT 实施。没有增加 exact4 或 focused28 运行。

**静态集合／算术核验**

使用 FIX13 继承的 `static/expected-fullnames-222.json` 与 `expected-fullnames-28.json`，对 D 的原方法名单、F 的分组进行只读集合运算。它们目前是继承身份依据，不称为 accepted baseline。未来 L4 仍须在 D1 接收和新资源构造之后从准确编译产物重绑并冻结。

| 核验 | 结果 |
| --- | --- |
| accepted baseline 的预期继承集合 | 当前证据222个唯一实例；fixed28为其子集。未来 D1 必须独立接收。 |
| Q2 | 20 methods／32 instances，resource-existing为3；两集合均在继承222中。 |
| 新案例与分组 | 12个新保留名与旧222不相交；L5a=32＋3＋4=39，L5b=8，focused union=47。 |
| controlled full | 222∪new12=234个唯一 fullname；参数文本保留，无集合内重复，无另跑28。 |
| whitelist | 26条准确唯一产品／测试／资源路径，包含新增授权候选 CandidateBoardInputTests；第27路径 stop。这仍是未来候选范围，不是当前实施权限。 |
| MainRow 外层 | 8＋232＋8＋252＋8=508，正确。 |
| Command 几何 | 280＋16＋280=576，576−252=324，几何算术正确；F01指出 cell 文案条件不成立，故这些常量须随修正同步更新。 |
| Reference | 142−48=94；104−48=56，正确。 |
| Recovery fixed footer | 6×48＋5×8＋8=336；H=812、k=1时 Body=812−64−48−336=364，且一般式要求 Body>=48k。 |

L2 compile/import→L3 native construct/save/reopen→L4 source/resource/manifest freeze→L5a/L5b→L6→L7 final native reopen 的先后明确；L7是所有相关本机测试／runtime runs之后的最后身份审计。L8保留 Android/device、实施美术、QA和同一R的独立门。WorldSpace mapper测试不再冒充 Overlay，真实测试要求 CanvasScaler 开启、两个冻结 safe rect、顶层 Enter/Exit、首帧及 rect/lossyScale/scaleFactor、长列表和 Recovery exit/return 证据。

**本次反例的只读背景身份**

| 文件 | bytes | SHA-256 | 用途 |
| --- | ---: | --- | --- |
| `docs/team/2026-09-30/snapshots/ugui-copy-amend-03-fix-01/planning-localization-draft.csv` | 64708 | `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c` | F01 的真实既有命令文案 |
| `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset` | 8573334 | `290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0` | 只读观察到的 point size／glyph advance；不是 FIX14 资源接收或 LAYOUT preimage |

没有据字体观察值判定 active FIX14 的资源状态；本回执仅使用其既有字形度量证明固定64宽的容量问题。FIX14 资源接收仍由该包的实际资源审计与独立R处理。

**收件边界**

只需针对 F01 修订命令 Caption／cell 容量及受影响的精确数字、验收断言，再交同一 R 作有限复核；其余九项和 ART N01–N03 不因本次保留项重开。不得借修正文案容量重开业务、保存、slot、层级或基线验证顺序。

本文件落盘后的 diff-check 与最终 path／lines／bytes／SHA-256 由本回合 final 报告；文件不内嵌自身哈希。final message ID 由 QA 在完成事件中读取。回传后保持 idle，等待后续。
