# LAYOUT-001 独立设计审查回执

2026-10-01 · R-TOOLS · 仅设计审查

**VERDICT: NEEDS_FIX**

Blockers：**10**（B01–B10）。中央确定的层级方向可以保留；工程候选仍有不能按字面实现或不能据此验收的合同缺口，需要先修订设计。本结论不是实现授权，不是产品、Unity、设备或美术完成验收。

**责任、权限与身份**

- Reviewer：FightMatch 工具独立审查 R-TOOLS。
- Reviewer thread：`01a0f3e3-9663-7be2-84a8-ed23be32f0b9`；host：`local`。
- Reviewer turn：`01a0f544-9cf7-70d1-a0dd-882c03c5bde8`。
- 派发／收件：主测试 thread `01a0f2e3-4bd2-7130-b43a-19afa702e7bb`，经主程委派；人类授权依据为仓库 `AGENTS.md` §6 的 standing team workflow。
- 唯一文件写入范围：本回执。未修改候选、ART 报告、源码、测试、Prefab、Scene、资源、配置或台账；未运行 Unity、构建或测试，未做 Git 写操作。
- `final_message_id`：本回执写入时，本回合 final 尚未产生，不能预填真实 ID。QA 应以以上 thread/turn 的 completed 事件及 `agentMessage.phase=final` 记录该 ID 到上游收件记录；不以 commentary ID 或旧回合 ID 代替。

**冻结候选核验**

以下两份是唯一被判定的设计候选。开始审查时全部匹配，写入回执前再次核验仍匹配；本回执简称中央文件为 C、工程文件为 D，下面的 `D:Lnn` 均指此表中的准确冻结文件和一基行号。

| 标识／候选 | lines | bytes | SHA-256 |
| --- | ---: | ---: | --- |
| C：[central-layout-hierarchy-adjudication-001.md](central-layout-hierarchy-adjudication-001.md) | 69 | 7011 | `d1a13b1aedee4b5b64d709b07f2c7d9ed980c3c8c39fdb76e1b338324ab6aa03` |
| D：[engineering-ugui-layout-system-design-001.md](engineering-ugui-layout-system-design-001.md) | 961 | 68763 | `a2b7f9677993b0194a008dce8d443aa743d5b0152ea61e145ec0bb75f2a47fce` |

ART-02、uGUI correction plan、实际代码与资源只作背景。另核 ART-03 回执为 81 lines / 7567 bytes / SHA-256 `098efce17af21d10553caeed90a4a0a82c8f854362b928c57c0ff22d5decc45f`；其四项问题已分别独立核对，没有用 ART 结论代替本次判定。

本轮采用 `unity:ui-ugui` 技能及其 ScrollView 参考，结合项目已安装 `com.unity.ugui@1.0.0` 的只读源码核对组件限制。所有算式、路径、声明和既有 XML/manifest 检查都是静态工作，不是新测试结果。

**B01 · blocker · P1：棋盘组件类型和部分 serialized target 不能按所列合同绑定**

- 候选定位：D:L121、L465、L480–L489；D:L456 声称保留 existing serialized meanings。
- 证据：精确层级将 BoardFrame 标为 `Image; CandidateBoardElement`，字段表又写 `Image board`。实际 `CandidateBoardElement.cs:11` 继承 `UnityEngine.UI.MaskableGraphic`；`CandidateBoardInputView.cs:15` 的字段类型是 `CandidateBoardElement`。已安装 uGUI `Graphic.cs:17` 声明 `DisallowMultipleComponent`；同一 GameObject 不能再以 Image 充当第二个 Graphic，Image 也不能提供现有棋盘 API。现有生成器 `FightMatchAndroidBuild.cs:395–L403` 添加的是该自定义 Graphic，并直接把它绑定给 input。
- 同类目标错误：D:L465 将 `PlayerBattleView.hud` 指向整个 Status Content，但当前该字段是 `LocalizedTmpText`（`PlayerBattleView.cs:17`），`BattleLocalizedRows` 以这个 label 为首行并克隆其兄弟（`CandidateBoardInputView.cs:178–L205`）。Content 是滚动容器，不能无说明地兼任首行 label；隐藏／克隆它还会影响 Save/Availability/Diagnostic 整组。
- 影响：按精确层级生成将发生组件／引用不兼容，或为迁就表格改变棋盘与 HUD 所有权。后续作者必须自行选择合同解释。
- 最小修正方向：BoardFrame 只保留既有 CandidateBoardElement Graphic，`board` 明确保持该类型；如确有装饰 Image，另列独立非交互子节点。逐项区分 RectTransform 容器和 LocalizedTmpText 模板／首行，给 `hud` 一个明确的子 label 或明确设计内部字段迁移；同步层级、生成器引用表和 fixture 引用，不扩大业务/API/保存格式范围。

**B02 · blocker · P1：敌方 OriginalSlot 的 UI 容量限制未与领域合法性分开**

- 候选定位：D:L63、L486–L499、L511–L518、L687、L936。
- 证据：D:L495 要求所有 actor/member 的 OriginalSlot 都为 0..2，L499 对超界统一 fail-closed。实际 `BattleEntryPreparer.cs:79–L81` 对 ally/member 验证 0..2；敌方在 L165–L167 只验证 nonnegative 与唯一性，没有上限 2。`BattleEntryPreparerTests.cs:90–L108` 接受敌方 slot 8；`BattleStartAssemblerTests.cs:112–L126` 保留 7、2；`CandidateDirectAttackTests.cs:147` 使用 5，`CandidateEnemyPhaseTests.cs:564` 使用 `i * 3`。实际首发 `first-release.fmsource.json:185,238` 的两名敌人是 0、1。
- 判断：候选把门放在 presentation，并在 D:L751 禁止修改 Core，因而不能说它已经更改领域验证；但它没有把敌方 0..2 明确限定为首发内容集合的 UI 呈现能力，反而将合法敌方 slot 与非法数据一起表述。它不足以成为清楚的首发容量合同；照所有合法快照执行会屏蔽原本合法的状态。
- 影响：实现者可能把展示不支持误当作存档／内容无效，或为了让 UI 通过而缩紧 enemy 的领域契约、重写 slot。不得用 ally 的 <=2 验证为敌方背书。
- 最小修正方向：明确区分三者：ally 领域范围、首发敌方输入集合、三枚视觉槽的支持范围。若保留敌方 0..2 呈现门，明确它只报告 UI unsupported、不会重写／拒绝领域数据，并锁定适用内容和合法非首发／旧输入的处置；超出既有首发范围的选择交主程按中央边界确认。保留稀疏合法敌方用例，禁止给 Core 的 enemy 验证补 <=2，也禁止隐式紧凑重排。

**B03 · blocker · P1：NormalHud 的局部诊断滚动链缺失**

- 候选定位：D:L59、L125–L129、L465、L484–L485、L515、L661。
- 证据：D 声称长诊断局部滚动，精确路径却只有 `StatusViewport/Content`；没有规定 ScrollRect、独立 Viewport/Mask 和它们的引用。Reference/History/Modal 明确采用三层链，NormalHud 没有同等合同，字段表还重复固定到两层路径。
- 影响：长 Save、Availability、PlaybackDiagnostic 无确定裁切／滚动所有者；实施可能让 Content 撑高 BottomHud 或遮住 MainRow／棋盘。D:L403 的通用 Content 规则不能补出缺失的实际节点和 serialized refs。
- 最小修正方向：明确 `StatusViewport [ScrollRect] → Viewport [Image+Mask] → Content [VLG+vertical CSF]`，同步所有字段路径、vertical-only 设置、viewport/content 引用和限定于 BottomHud 的 raycast。不得让 ContentSizeFitter 控制 BottomHud 轴。用 EN/ZH 长诊断验证内容可滚、MainRow 固定且不遮 Board。

**B04 · blocker · P1：MainRow 容量漏算保留的动态动作，四动作宽度公式也不足以修复**

- 候选定位：D:L130–L140、L298–L302、L402、L466、L474。
- 证据：精确层级已有三个 MemberSlot 和 Retry/Resolve/Skip/HistoryOpen 四个固定动作；D:L466 又把 `PlayerBattleView.actions` 放到同一 MainRow/Actions。实际 `PlayerBattleView.cs:118–L140` 在有战斗 History 时创建 Exit、Restart、Settle、Reference 四个额外按钮，L92 明确使创建的按钮 active，`interactable=false` 不会移除布局占位；另有 L127、L131、L222 的状态文本也进入 actions。
- 容量反例：三张卡＋八个动作，即使全部只取 48k 宽且不留任何间距，也需 `11×48k=528k > B=508k`；采用卡宽 72k 则至少 600k。最小 MainRow 只有 72k 高，也放不下两排 48k 热区。ART-03 的 488k 算式只覆盖三卡＋四固定动作，不能作为保留全部现有入口的证明。
- 影响：按钮必然溢出、缩热区、被遮挡或被无声删除；禁用态仍占位、英文两行容量与状态文本也未解决。
- 最小修正方向：先列出全部固定／动态战斗入口与文本，逐个冻结可见条件和目标容器；在中央既定 BottomHud／局部滚动关系内给出可行宽高预算及额外动作的可达路径。不得删 Exit/Restart/Settle/Reference，也不得仅凭按钮 disabled 推定它不占布局。之后再给 EN/ZH caption、padding、间距和最小字号／热区的实测标准。

**B05 · blocker · P2：Reference 最小高度下的 Close 位置仍有两种冲突解释**

- 候选定位：D:L148–L159、L298–L302、L528–L535。
- 证据：精确层级写 Header/Close，字段表却允许 `Header/Close or Actions/Close`。BottomHud 下限 104k；若 Header 和独立 Actions 各留 48k，正文只剩 8k，无法有效呈现条件、差异与步骤。
- 影响：两个均符合字面的实现具有完全不同正文容量，后者会使参考信息无法阅读，或迫使作者越界增高抽屉、覆盖棋盘。
- 最小修正方向：固定唯一 Close 到 48k Header，若没有必须独占 footer 的既有动作则将 Reference Actions 折叠为 0；明确 Body 在 142k／104k 下的容量。若确有独立动作需要保留，先给出有证据的重算并交原责任人确认。同步关闭按钮绑定、重新打开和首拖关闭测试，不改参考业务语义。

**B06 · blocker · P1：共用 RootMask 的启闭所有权和跨路由固定控件迁移不完整**

- 候选定位：D:L176–L225、L231–L238、L428–L450、L472–L476、L576–L607、L650、L675–L683。
- 证据：Confirmation 与 Recovery 分别共用一个 RootMask，两个 inner panel 互斥；navigation 的 `confirmationRoot/recoveryRoot` 又明确指向 inner panel（L587–L588），battle 也有自己的 panel root。候选没有给 common RootMask／outer root 明确 serialized owner、启闭信号和“无 panel 时关闭”的状态规则。现有 `PlayerNavigationRecoveryView.cs:54–L55,149` 只切自己引用的 roots；Host 的新字段表只有 language/license/quit 等既有 popup roots。只关闭 panel 会留下一个仍有效的全屏 blocker；若 common parent 默认关闭，仅启用 panel 又不会使它出现。
- 另一个确定迁移缺口：D:L601、L607 把 `originalResultButton` 永久放在 NavigationRecoveryPanel。当前 `PlayerNavigationRecoveryView.cs:60–L63,137–L149` 会在 Confirmation 和 Recovery/CommittedResult 当前容器中呈现这个按钮以及原操作选择项；`PlayerNavigationRecovery.cs:47–L48` 的动作可用性并不以 Recovery 路由为前提。固定到不可见 Recovery panel 不能保留原有确认页入口。
- 影响：恢复／确认可能不出现，退出后可能残留阻断，或者丢失既有原结果入口。单列“mutually exclusive”和通用 Unbind 文字不足以生成准确可测试的 root ownership。
- 最小修正方向：冻结 common outer root／mask 与两个 panels 的准确字段、唯一启闭负责人及优先级状态表；明确 enter、switch owner、close、Unbind、reopen、Recovery exit/return 后谁关闭 blocker。保留 OriginalResult 在原支持路由的可达性，给出固定引用／唯一 ViewId 的迁移方案。不要重新引入运行时 reparent 或删除回调；补一次真正的 Recovery→安全返回→普通页面／Popup 的射线和旧回调验证。

**B07 · blocker · P1：可变数量的恢复选择按钮被强制塞入固定 Actions**

- 候选定位：D:L578–L581、L602–L607、L618、L639–L648。
- 证据：D:L605、L607 规定 Button 一律进入固定 Actions，Body 只能放 text；L642 又规定 Actions 纵排、每项至少 48k＋8k 间距。当前 `PlayerNavigationRecoveryView.cs:123–L145` 为每个 observed candidate 和每条 published record 创建选择按钮；`PlayerBattleView.cs:205–L210` 同样为每个 observed candidate 创建按钮。没有把这些数量限制为可放进固定 footer 的常数，也没有规定超量时的滚动归属。
- 影响：正文可滚并不能解决动作区不断增高；足够多的合法记录／候选会挤空 Body 或把安全动作推出 panel。它也会影响 confirmation 中已有的原操作列表。设计没有可验证的最坏容量。
- 最小修正方向：区分可滚的候选／原操作选择列表与固定的安全／危险执行动作，保留原回调和请求身份；准确冻结各类交互 row 的容器、所有权、ViewId 清理和数量边界。允许正文内必要的选择控件应写成明确合同，而不是让实现者违反“Button never in Body”。用超过一屏的真实候选／原操作列表验证滚动和固定动作，不只用长字符串替代容量用例。

**B08 · blocker · P1：删除 ScreenLayer 统一顶部留白后，非战斗页面没有替代几何合同**

- 候选定位：D:L57、L73、L103–L106、L160、L276、L361–L368、L444、L452。
- 证据：候选将 ScreenLayer 改为 full SafeAreaRoot，并只说明 BattleContent 位于共享 TopBar 下方。实际 `FightMatchAndroidBuild.cs:290` 的 -88 offset 原来为所有 ScreenLayer pages 留出页头空间；`Rect(fill=true)` 与 `Scroll()` 在 L125–L129、L155–L166 使用四向 stretch，ResultPage/ResultScroll 亦如此（L414–L415）。Startup/Navigation/Result 不在新组件所列的逐页响应式 refs 中，候选未给它们替代顶部 inset 或 viewport 起点。
- 影响：若按所述保留非战斗页布局而删除共同扣减，其首行／热区会进入 HudLayer/TopBar 的覆盖范围；长屏 TopBar 还带 top breathing，不能用另一个未注明的固定常数解决。作者必须临场设计三个页面的避让方式。
- 最小修正方向：补齐共享 TopBar 对 Startup/Navigation/Result 的几何所有权和准确 viewport 可用区域，明确由哪一个现有布局负责人写哪个轴，避免与 VLG/CSF 双写；继续保留一个 TopBar，不恢复 Battle 的双重扣减。验收覆盖这三个页面的首行、首个按钮、滚动边界和页头命中。

**B09 · blocker · P2：1080×2400 的安全区样例偏离已冻结灰盒**

- 候选定位：D:L329、L848；对照 C:L54–L56。
- 证据：候选是 `(32,80,1016,2160)`，灰盒 README:L37 是 `(32,120,1016,2160)`。两者宽高相同，但候选 bottom/top=80/160，冻结灰盒是 120/120；整块布局相差 40 physical pixels。没有记录该样例变更的裁定。独立复算确认 sqrt(5)、W/H、k、五区高度及上下 160 px breathing 本身正确，不需重算这些值。
- 注意：540 样例不是同一个错误。README:L29 的 SVG 顶部 y=24 对应 Unity 底部原点 y=16，D:L310、L325 的四边 inset 与此相容。
- 最小修正方向：将长屏两处统一回 `(32,120,1016,2160)`、四边 32/120/32/120，并明确 SVG 顶部坐标与 Unity 底部坐标的转换。保留该非对称 80/160 样例只能作为另一个明确批准的附加用例，不能替代冻结样例。

**B10 · blocker · P1：Q1–Q5 不是可执行的资源／测试依赖顺序，真实运行证据入口须分清**

- 候选定位：D:L759、L773–L800、L802–L840、L843–L880、L898–L925。
- 证据：文档称有限未来顺序，却先在 Q1/Q2 运行新组件、真实 Prefab 和新 refs 的测试，再到 Q3 compile/import/native save/reopen。Q1 的 `SavedPrefabAssemblesOneRealHostCanvasAndProjectEventSystem` 确实通过 fixture 加载磁盘 Runtime Prefab；将其放在新代码／资源构造之前无法验证目标装配，不能把隐含的提前 Unity import/生成/保存从执行预算中抹掉。Q1 中 resize/first-frame 等新名字也不是无需 Unity 的静态文本检查。
- 现有名字核验通过：Q1 的三个 existing names、Q2 的二十个方法均真实存在，旧名称中的 `RealUIToolkitSchedule` 不能仅凭词面改名。用 FIX12 冻结 full-name manifest 展开，Q2 是 **20 methods / 32 instances**，与原固定 28 仅交集 7；它不是那组 28。原固定 28 是 222 的子集。新 12 个 LAYOUT 名字是未来保留名，尚非已执行结果。
- 真实几何证据边界：既有 `BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical` 在 `UguiSceneCompositionTests.cs:251–L252` 明确关闭 CanvasScaler 并切 WorldSpace；它可保留 mapper 断言，但不能证明 Overlay＋match 0.5 的 sqrt(5)、首帧或屏幕 pixel 命中。Q4 必须使用实际运行 CanvasScaler 的新夹具／入口，并说明真实 PlayMode 命令在顶层测试，不能重复 nested Enter/Exit 错误。
- 影响：执行者无法从 Q 编号决定何时编译、生成、冻结、运行和最终 reopen；可能拿旧资源／WorldSpace 结果充当新布局证据，或重复旧 28 及泛化为未授权长测。Q3 的提前 reopen 也不能自动充当后续 runtime/full 测试之后的最终资源身份。
- 最小修正方向：写清依赖边，区分无 Unity 的源／层级静态检查与需要编译和新资源的测试；新资源原生构造、保存、重开必须先于依赖它的装配／focused 测试，全部相关运行之后再做最终 reopen/hash/GUID/log 审计。Q0 当前 correction 的 4→222（含28）→final reopen 合同保持；未来 LAYOUT 只在该 baseline 独立接收后重绑源、fixture、资源和 exact full-name manifests，说明 Q2/Q5 的去重／重跑理由及全量具体范围，不能凭一个 suite 标签增跑整仓。补齐 Overlay 两尺寸、短屏两段压缩、below-minimum、首帧、恢复退出／返回与长列表的有限可观察入口。

**非阻断项与必须保留的正确边界**

| 核对项 | 本次判断／后续保留条件 |
| --- | --- |
| 中央层级裁定 | D 的 Reference→BottomHud、Normal/History/Reference 互斥、drawer 不覆盖 Board，以及 Recovery→SystemLayer 的方向与 C:L35–L37 一致；不重开替代架构。 |
| 单 Canvas/EventSystem | 当前资源静态解析：Prefab 1 Canvas、1 GraphicRaycaster、0 EventSystem；Scene 1 project EventSystem、1 PrefabInstance。目标没有新增此类实例，GUID 保留约束明确。不是运行时装配验收。 |
| System 优先级 | D:L233–L238 将 System 放在 Popup 之上、诊断放在 Recovery 之上，优先级本身没有反向覆盖；B06 要补的是可执行的 root/mask 启闭与退出合同。 |
| SafeArea 接口可访问性 | `SafeAreaFitter.cs:12–L13` 的 Changed/Version 是 internal；新组件位于同一 FightMatch.Presentation assembly。PresentationAssemblyInfo 也向 FightMatch.Core.Tests／Host.Tests 开放 internal。无需 public 化、修改 asmdef 或修改 SafeAreaFitter。 |
| 主几何算式 | 56＋166＋48＋508＋142=920；Stage 最低96、BottomHud最低104，总和812。长屏 s=sqrt(5)，W=454.369013、H=965.981366、k=.894427191，各高度与 D 表一致。短屏两阶段压缩和禁止压缩 Board/48k 热区可保留；内层容量由 B03–B05 关闭。 |
| 同轴布局所有权 | D:L396–L406 的 responsive region 单一 owner、无同轴 LayoutGroup/CSF/AspectRatioFitter、scroll Content 使用 VLG＋vertical CSF 的方向正确。不要把 Content 控制其子项与外部组件同时控制该 Content 轴混淆。 |
| 生命周期基本约束 | 单次绑定、幂等退订、page/epoch/generation/token 保护、关闭不完成业务、真实首拖保留 active pointer、locale rebind 保留有效 mode 都应保留。新 root／跨层 refs 需要按 B01/B06 落到真实字段，不能只依赖原 GetComponentsInChildren 清理范围。 |
| 诊断与输入验证 | D 已要求无效尺寸可见 gate、保留最后有效 rect、仅释放自己的 gate。验收时还要在有效拖动中进入无效布局，再发送 late Up/EndDrag，确认既有 cancellation 链清掉 capture 且零提交；不能用“遮罩已开启”替代此证据。此处列为必须验证的既有约束，不据此声称已观察到新产品缺陷。 |
| 字体／本地化／placeholder | Prefab 保留诊断默认，runtime EN/ZH 长文案、固定动作可见、0 placeholder／缺字的要求正确。新 LayoutDiagnostic 也须通过现有绑定显示；不能硬编码正文或修改冻结文本／字体来掩盖缺口。 |
| 首帧与 Canvas | C:L58–L60、D:L255、L384–L392 正确区分 serialized defaults 与 runtime rect/lossyScale/scaleFactor。Scene 零 override 不独自证明黑屏，runtime scale 也不要求恒为 one；需保留真实 pre-render／首帧表和截图。 |

**Q0 及各门状态**

当前 uGUI correction 的 FIX12 冻结证据仍为 `BLOCKED_EXACT4`：4 个 exact full names，0 passed / 4 failed；222 与 native final reopen 未运行。本设计审查没有改变这个状态。D:L768、L929 的“当前 correction chain 独立接收后才可进入实施”依赖必须保留。

FIX12 candidate：`3d6826d2782ce87399694194d778d926cd769de111752ae8308e345cb25a1a6e`；sourceCandidate：`da039561b0ffdc523965246562039a65d9cdccc9e88e4bc06cdc6eab8ad5e3e1`。所读 exact4 XML 为 19300 bytes / SHA-256 `13900354bea366d970d46d9415b26266aa51d8053b26e9eebdef80dbbcea8083`。这些是失败证据，不是 LAYOUT 的实现 preimage。

| 门 | 本次审查结果／必要边界 |
| --- | --- |
| Q0 | 候选身份门通过；设计问题未关闭，产品 baseline 依赖也未关闭，因此实施仍不可启动。 |
| Q1 | 新12名＋既有3名的意图可追踪；组件类型、refs、滚动、槽位、共享遮罩及布局容量须先修设计，真实资源测试顺序见 B10。 |
| Q2 | 现有20方法／32实例真实；保持完整参数化名与原断言，不以方法数或固定28代替实际实例 multiset。 |
| Q3 | 本回合 NOT_RUN；未来必须由获授权 C 串行执行，先满足依赖该资源的验证，再给所有后续运行后的 final reopen 身份。 |
| Q4 | 本回合 NOT_RUN；要求 Overlay/CanvasScaler 真运行，两种冻结 inset、EN/ZH、方形/命中同源、drawer 不盖 board、滚正文而 actions 固定、Recovery precedence 和 exit/return、无占位、Canvas rect/lossyScale/scaleFactor/首帧均有可观察证据。 |
| Q5 | 本回合 NOT_RUN；重绑 accepted baseline 实例集合并计入新案例，保留现有断言；结合 final source/resource/字体/日志审计。没有默认新增一轮28或整仓长测。 |
| Q6 | 本回合 NOT_RUN；Android/device 与实施后的专业美术接收独立保留，Mac 静态或 Editor 证据不能替代。 |

**实际背景版本记录**

下列源码来自只读工作区背景，并非两份冻结候选的一部分，也不是未来实施 preimage。并行 correction 可能继续修改产品；本报告按以下观察版本给出源码行号，未来 Q0 必须重绑。快照观察截至 `2026-10-01 02:33:28 UTC`。

| 背景文件 | bytes | SHA-256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | 12750 | `956744b6f5f1fb571e5c069589d422e0dd8f668b604f50c35f00b22c8cdc357d` |
| `Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs` | 32658 | `2fd5106b7cab62afccea58fad26f715b6aff3450c5bb2b36ce5b505c4bfb3ef7` |
| `Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs` | 24951 | `430d1448841fc811f9bc7ee2706c0f1fb5d28363a7342b2d87e67b74c694adcf` |
| `Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs` | 11337 | `dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9` |
| `Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs` | 1577 | `34234229face2665d02c5806a890cec15a297f364b96cce80b457eabc731c8a7` |
| `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | 14379 | `d7a29970b0fb8b594e7114a883b82ffef92c47181218df1bec1f420dfda72136` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` | 83965 | `2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac` |
| `Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs` | 17779 | `f14dc701f083095d04e091aeaf9333c28383809578acf9b389799376e515d95b` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 17493 | `130fdfaabb834ddcfeb1dda04af84ef199faa517b805745e08ecbcf54a301a52` |
| `Assets/StreamingAssets/FightMatch/first-release.fmsource.json` | 20443 | `fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a` |
| `Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` | 1060568 | `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` |
| `Assets/Scenes/FightMatchDemo.unity` | 12900 | `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |

**修订及收件边界**

修订责任返回主程／SYS；本轮没有创建纠正包或扩展实施白名单。B01–B10 应在同一可核冻结修订中逐项回应，中央已定 Reference/Recovery 归属、单 Canvas/EventSystem、业务／保存／播放所有权与 Q0 依赖保持。后续限域复核对修订差异与证据进行，不以本文提前授权任何产品修改或 Unity 运行。

本文件的落盘 diff-check、最终 lines/bytes/SHA-256 由本回合 final 回执报告；不在文件内嵌自身哈希以造成循环身份。最终消息产生后由 QA 收件，本 reviewer 随后保持 idle。
