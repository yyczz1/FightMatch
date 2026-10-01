# SYS-ENG-001：运行时 uGUI 迁移系统技术盘点

2026-10-01 · r4（COPY-IDENTITY-SYNC-05）· 状态：
**READY_FOR_IMPLEMENTATION_INPUT_SYNCED**。第 17 节以后是规范性冻结结果；
若与前述 r0 盘点候选冲突，以第 17 节以后为准。FIX-04 已获独立 R `ACCEPT`；
本次只同步已接收制作输入，实施仍以主程签发的逐文件任务包和验证门为准。

## 1. 身份、权限与固定输入

- 交付负责人：独立系统技术设计 `SYS-ENG-001`，thread
  `01a0f2e6-29bb-7092-abf0-705b41b7bf93` / host `local`。
- 上级／正式回传：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` /
  `local`。团队内分发与回执授权来自 `AGENTS.md` 第 6 节、
  `.agent/TEAM_WORKFLOW.md` 和本目录 `README.md`。
- 本线程唯一可写路径是本文。产品源码、场景、资源、`.meta`、Packages、
  ProjectSettings、测试、构建工具和 Git 均只读；未运行 Unity。
- 固定仓库起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`。当前工作区另有中央、
  主程、主策、主美维护的未提交文档；本文不接管或清理它们。
- 已读权威输入：`AGENTS.md`、`START_HERE.md`、`.agent/PROJECT_CONTEXT.md`、
  `.agent/TEAM_WORKFLOW.md`、本轮 `README.md`／`dispatch-001.md`、迁移
  `SYSTEM_DESIGN_CONTEXT.md`、uGUI 修正方案、SD12 `input-presentation.md`
  r3、SD13 `update-lifecycle.md` r1、029 交付／范围／现有证据、当前源码、
  场景、资源、程序集和 UI／Host 测试。
- 跨线输入版本：主策 `planning.md`（2026-09-30 PLAN-001，
  `DRAFT_FOR_CROSS_ROLE_REVIEW`，重点 §2～7）；COPY FIX-01 CSV 已获独立 R
  `ACCEPT`（turn `01a0f395-145a-79e3-ab3f-cbe5bdc90291`，final
  `msg_0d29bf8d726d8dc0016abd55c4739487d0a77167e28bcf1f79`），但仍是制作稿，
  不是正式运行时表；主美 `art.md`
  （2026-09-30，`LAYOUT_INTERFACE_READY / ART_SELECTION_PENDING`，重点
  §2～6、§8）。9/29 美术已否决，9/30 A/B/C 仍未采用；制作稿与美术候选
  均不是运行时权威或正式资源采用结论。
- 固定架构输入：`docs/team/2026-09-30/engineering-architecture-001.md`，
  SHA-256 `309762cb3c4062aab9beadbccb5e34fbc9225d53fb9339f85021e46668513325`，
  29037 bytes，主程 verdict `ACCEPT`，架构回执 turn
  `01a0f2e6-1baf-72c3-8157-60839eec5cee`。其第 6 节 13 项交接由本文
  第 17～25 节冻结。

## 2. 已观察的工程事实

### 2.1 已有依赖与未接入项

| 项 | 仓库事实 | 本轮含义 |
| --- | --- | --- |
| Unity | `2022.3.18f1` | 继续使用；`2022.3.62f3` 只是发行准备候选，不在本包升级。 |
| uGUI | `com.unity.ugui` `1.0.0`，builtin | 已具备 `Canvas`、`GraphicRaycaster`、`EventSystem`、`StandaloneInputModule` 等，无需仅为迁移改包。 |
| TMP | `com.unity.textmeshpro` `3.0.6` | 可作 uGUI 文本目标，但当前资源不是可直接绑定的 `TMPro.TMP_FontAsset`。 |
| 输入 | `ProjectSettings` 的 `activeInputHandler: 0`；无新 Input System 包证据 | 首包只能按现有旧输入模块设计；不得假定 `InputSystemUIInputModule` 已存在。 |
| Luban | manifest／lock／Assets 无安装证据 | 用户已选制作期路线，但确切版本、执行入口、schema、生成目录和适配器仍待单独冻结。 |
| YooAsset | manifest／lock／Assets 无安装证据 | 用户已选资源路线，但当前 `FightMatchStreamingAssetsLoader` 仍直接用 UWR 读取六件 StreamingAssets；不得写成已接入。 |
| Unity Localization／Addressables | 无安装证据，且不是用户选择 | 不引入为替代方案。 |

现有 `NotoSansCJKsc-Regular.asset` 的主对象是
`UnityEngine.TextCore.Text.FontAsset`（内置 script fileID `19001`），动态
图集为空并引用同目录 OTF；它服务当前 UI Toolkit。uGUI TMP 的
`TextMeshProUGUI.font` 要求 `TMPro.TMP_FontAsset`，不能只把序列化字段换名。
OTF（GUID `960a9cda7a6b34935b8c18586488d314`）和 OFL（GUID
`e6f3cf4d3cbf34266af76d305095c7e9`）的原始字节与许可须保留。

### 2.2 当前运行时 UI Toolkit 影响面

| 模块 | 当前路径 | 观察到的 UI Toolkit 职责 | 迁移中应保留的非 UI 语义 |
| --- | --- | --- | --- |
| 棋盘 | `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs` | `VisualElement` 自绘、屏幕／局部／格坐标换算、Pointer 捕获、取消、几何变化 | `RouteGesture`、PointerId、半开边界、阈值、只提交一次、覆盖帧数据 |
| 棋盘面板 | `.../CandidateBoardInputView.cs` | 标签、成员按钮、重试／确认按钮、100ms schedule | `CandidateBoardInputController` 的选择、原请求续办和状态读取 |
| 播放面板 | `.../CandidateBattlePlaybackView.cs` | 标签、跳过按钮、16ms schedule、blur／detach／低内存接线 | `CandidateBattlePlaybackController`、原 token、最新头重建和完成报告 |
| 战斗页 | `.../PlayerBattleView.cs` | 动态按钮／历史／确认／恢复／结果容器 | `PlayerBattleController` 页 token、上下文、原结果、结算、去向 |
| 默认参考 | `.../PlayerDefaultReferenceView.cs` | 浮层、步骤按钮、16ms schedule、棋盘覆盖 | 精确 binding／几何检查、只读参考、首个真实拖动关闭 |
| 导航页 | `.../PlayerNavigationView.cs` | 地图、队伍、背包、准备、合成动态树和字段 | `PlayerNavigationController`／Session 的 route、draft、token、host request |
| 永久操作 | `.../PlayerPermanentDetailView.cs` | 数量／来源输入、Toggle、报价显示 | 数值边界、显式来源、不可变 draft／quote |
| 保存恢复 | `.../PlayerNavigationRecoveryView.cs` | 候选、原操作、确认、诊断按钮和文本 | 原 OperationId、候选选择、Retry／Resolve／End 条件 |
| 导航按钮绑定 | `.../PlayerNavigationController.cs` 内 `NavigationBindings` | 直接创建 UI Toolkit Button／Label 并退订 | 控制器 `epoch` 和冻结 target／context 不应改变 |
| 宿主视图 | `Assets/Scripts/FightMatch/Host/FightMatchHostView.cs` | 根视图、页面显隐、ScrollView、中文替换、棋盘尺寸 | `FightMatchHostSession` 页面／返回／退出／路由；中文替换本身不是新本地化方案 |
| Unity 宿主 | `.../FightMatchPlayerHost.cs` | `UIDocument`、`rootVisualElement`、TextCore FontAsset、安全区 style | 内容加载、产品根、Session 创建、暂停／焦点／低内存／返回键／销毁 |
| 场景／资源 | `Assets/Scenes/FightMatchDemo.unity`、`Assets/UI/FightMatch/FightMatchPanelSettings.asset`、`FightMatchTheme.tss` | 场景只有一个 `FightMatch` 根，挂 Host＋UIDocument；Panel 参考 540×960 | 场景 GUID、Host script GUID、字体源／许可引用和现有产品根不应无因改变 |
| 构建检查 | `.../Host/Editor/FightMatchAndroidBuild.cs` | PrepareResources 绑定／复开验证 UIDocument、PanelSettings、TextCore 字体 | 精确场景保存复开、包设置恢复、构建／证据纪律必须保留 |

FlowPuzzle 的 Editor UXML／USS／`CreateGUI` 和 Editor 测试不在迁移范围；
禁止全仓删除 UI Toolkit 或移除 `com.unity.modules.uielements`。

### 2.3 建议保持不改的深模块与现有缝

下列模块已把大量业务行为藏在较小 interface 后，应作为迁移复用缝，而
不是让 uGUI 重算业务：

- `CandidateBoardInputController`：唯一把手势意图映射到 Application 请求，
  持有选择、原请求、回退预览和旧回调防重。
- `CandidateBattlePlaybackController`＋`CandidateBattlePlaybackFrame`：只读
  OrderedFacts，持有播放 token／generation，报告完成前先重建最新头。
- `PlayerBattleController`：持有页 epoch、Session、原结果／恢复／结算入口。
- `PlayerNavigationController`：持有冻结 target/context、epoch、单次
  `HostRequested`；仅其内嵌 `NavigationBindings` 属于旧视图实现。
- `FightMatchHostSession`：唯一宿主页面、创建／恢复、Navigation↔Battle
  路由与退出应用语义；uGUI 不得复制此状态机。
- Core、Application、Platform、保存封套、已发布内容和 `RouteGesture`：
  当前盘点没有发现为换 UI 必须修改的理由。

## 3. r0 场景与 uGUI 层级候选（历史盘点；已由 §20 替代）

主美输入固定 540×960、Match 0.5、安全区和五层。建议场景最终只有一个
玩家 UI Canvas 和一个 EventSystem：

```text
FightMatch
├── FightMatchPlayerHost
├── RuntimeCanvas                         Screen Space - Overlay
│   ├── CanvasScaler                      Scale With Screen Size / 540×960 / Match 0.5
│   ├── GraphicRaycaster
│   └── SafeAreaRoot                      anchors 由 Screen.safeArea 更新
│       ├── BackgroundLayer               可越安全区的背景放到 Canvas 同级专用根时另核
│       ├── ScreenLayer                   Startup / Navigation / Battle / Result
│       ├── HudLayer                      页头、战斗状态和底部操作
│       ├── PopupLayer                    Reference / Confirm / Recovery / License / Language
│       └── SystemLayer                   Loading / FatalError / localization diagnostics
└── EventSystem
    └── StandaloneInputModule
```

约束：

- 场景必须恰好一个 `EventSystem`；不得同时挂新 Input System 模块。
- `RuntimeCanvas` 必须有 `GraphicRaycaster`。仅交互 Graphic 开
  `raycastTarget`；装饰默认关闭；模态遮罩开启且覆盖整个安全区。
- 背景可铺满屏，文字、按钮、棋盘命中区和弹窗正文必须在
  `SafeAreaRoot`。安全区更新只改 anchors／offset，不改变业务状态。
- `ScreenLayer` 每次只显示当前页；页面 GameObject 显隐和视图 Bind／Unbind
  要分开定义，不能把 `SetActive(false)` 自动解释成业务退出或播放完成。
- 战斗基准：TopBar 56、Stage 190、BattleStatus 48、BoardFrame 正方形
  508×508、BottomHud 使用余高。优先保正方形棋盘→48×48 热区→正文
  可读→压缩舞台留白；普通热区间距至少 8。
- 地图／队伍／背包／准备／结算采用 TopBar＋Content＋BottomAction；长
  内容仅 Content 使用 `ScrollRect`，标题与主要按钮不跟随正文滚走。
- 当前无已采用美术。灰盒只能使用颜色块／单色剪影和明确
  `PLACEHOLDER`，不能引用 9/29 或 9/30 A/B/C 候选。

## 4. 棋盘输入与坐标映射

uGUI 适配器应把 `EventSystem` 事件转换成现有 `PointerSample`，不改
`RouteGesture`：

| 现 UI Toolkit 事件／条件 | uGUI 候选映射 | 必须保持的结果 |
| --- | --- | --- |
| `PointerDownEvent` | `IPointerDownHandler.OnPointerDown` | 仅主键／触摸；刷新后冻结原视图和 PointerId，不提交。 |
| `PointerMoveEvent` | `IInitializePotentialDragHandler` 关闭 EventSystem 内置阈值＋`IDragHandler` | 所有位移仍交现有 6／10 与格宽 0.15～0.25 规则；不让 Unity 默认阈值覆盖。 |
| `PointerUpEvent` | `IPointerUpHandler`＋必要的 `IEndDragHandler` 去重入口 | 同一 Pointer 只结束一次；合法配对松手才提交；短按才查历史。 |
| `PointerCancelEvent` | `ICancelHandler` | 只清草稿／捕获，无业务意图。 |
| pointer capture out | EventSystem 的 `pointerPress`／`pointerDrag` 所有权，加 `OnDisable`／解绑兜底 | 第二指不能接管；旧页销毁后不提交。 |
| `BlurEvent`／失焦 | Host 的 `OnApplicationFocus(false)` 只委托 `HandleApplicationFocus(false)`；pause 仍走既有私有回调 | handler 通过真实 `FightMatchHostView.PausePresentation()` 清手势并跳播放终态。 |
| 离开棋盘 | `IPointerExitHandler`（仅活动 Pointer） | 按 SD12 取消，不在重新进入后自动恢复。 |
| 几何变化 | `OnRectTransformDimensionsChange`＋安全区／Canvas 尺寸版本 | 立即取消旧几何手势；下一次事件重新取尺寸。 |
| Detach／Close | 显式 `Unbind`／`OnDisable`／`OnDestroy` | 退订事件、移除 Button listener、递增 epoch；旧回调无效。 |

坐标统一建议：用
`RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect,
eventData.position, eventData.pressEventCamera, out local)`，再按
`RectTransform.rect` 映射零基格。局部坐标采用左下半开边界；Y 轴翻转后
保持 SD12 的棋盘原点语义。手势距离也使用同一局部 Canvas 单位，阈值为
`Clamp(mouse ? 6 : 10, cell * 0.15, cell * 0.25)`，避免 CanvasScaler／设备
像素密度改变行为。最终必须用 540×960、1080×2400 和非零安全区实测，
不能仅凭数学推演接收。

棋盘表现候选为一个继承 `UnityEngine.UI.MaskableGraphic` 的自绘模块：
`OnPopulateMesh(VertexHelper)` 绘制网格、端点和线路；播放／参考只提供
`CandidateBattlePlaybackFrame` 覆盖并调用 `SetVerticesDirty()`。它同时作为
唯一交互 Graphic，覆盖层与装饰设 `raycastTarget=false`。该类型是否复用
`CandidateBoardElement.cs` 的 GUID／类名，或新建更准确名称，是 r0 当时的
待决项；现已由 §19 的同名原地替换冻结。

## 5. 播放、页面与生命周期映射

| 旧行为 | uGUI 候选实现 | 禁止退化 |
| --- | --- | --- |
| 棋盘 100ms Refresh schedule | 激活且已 Bind 的 MonoBehaviour 用 `unscaledDeltaTime` 累计到 100ms，调用同一 controller.Refresh | 不创建第二状态缓存；页面隐藏后旧实例不继续回调。 |
| 播放 16ms schedule | 可见且绑定的播放适配器在 `Update` 传实际 unscaled 毫秒给 controller.Advance | 不由动画改 HP／路线；异常交 `ReportSchedulingFailure`。 |
| 参考 16ms schedule | 参考浮层自己的 Update；关闭递增 generation、停计时、清棋盘 override | 参考不报告战斗 token；真实首次拖动关闭但不吞当前手势。 |
| Page Bind／Detach | `Bind(controller)` 捕获页 token／epoch；`Unbind` 精确退订、移除 listeners、清动态节点 | 旧按钮、旧确认、旧 retry 不能命中新 controller。 |
| 页面重建 | 重新 Bind 同一 Host 所有 controller；业务状态来自 controller/session 查询 | 不创建第二套 Session，不重新 Prepare 原请求。 |
| 播放中 App pause／失焦／低内存 | `FightMatchPlayerHost` 令真实 `FightMatchHostView.PausePresentation()` 先取消活动手势、再调用现有 `Session.PausePresentation()`；失焦入口按 §20.4 | 只跳最新终态并回报自己的 token；旧 A1 不解 A2。 |
| Back | 继续由 Host 每帧读取现有 Escape／Android Back，再调用 `Session.Back()` | 退出本局、退出应用、结束未提交请求保持三种不同意图。 |
| Destroy | 先 Unbind/Dispose 视图，再退 Host 事件，最后 `Session.Dispose()` | 不遗漏低内存监听，不重复 Deinit 架构。 |

Host 的页面显示应只消费 `FightMatchHostSession.Page`：Startup、Navigation、
Battle、QuitConfirmation。Navigation 和 Battle 内部 route 继续由原 controller
提供；uGUI 层只启用相应容器。弹窗遮罩属于显示状态，业务确认仍走原
confirmation/token。

## 6. 现有行为逐项迁移矩阵

| 现有行为／产品入口 | 业务来源 | uGUI 可见载体 | 新验证最低要求 |
| --- | --- | --- | --- |
| 空根创建、继续原创建、重新读取 | `FightMatchHostSession` | StartupPage | 真按钮事件；重复点击不建第二档；旧创建 intent 保留。 |
| 地图 L1、队伍、背包、准备 | `PlayerNavigationSession/Controller` | NavigationPage 各 section＋ScrollRect | 页面路由、Back、旧回调 epoch；不得展示 L3／假下一关。 |
| 队伍三槽与确认 | 原 Formation draft/confirmation | TMP_Dropdown（或同等 uGUI 明确选择控件）＋ConfirmPopup | 修改前后、取消零写、旧按钮零写；最终序列化引用按 §20 冻结。 |
| 空库存／无配方／装备偏好 | 原 inventory/permanent route | Bag/Craft/Detail sections | 空态不是错误；活动战斗准入原因不丢。 |
| 入场与继续当前战斗 | 原 HostRequest | PreparationPage | 单次 immutable HostRequest；活动局不创建第二局。 |
| 选角色与拖线 | `CandidateBoardInputController` | Battle HUD＋BoardGraphic | 真 EventSystem 鼠标／触摸、第二指、取消、边界、单提交。 |
| 播放／跳终态 | playback controller | BattleStatus＋Board override＋Skip Button | 原 facts 顺序、一次 token 报告、跳过不改业务。 |
| 默认参考 | `ReadDefaultReference` | PopupLayer ReferencePopup＋同棋盘覆盖 | binding／几何不匹配拒绝；真实拖动关闭且继续提交。 |
| 历史回退 | 原 history preview/confirm | History list＋ConfirmPopup | 多项范围不自动提交；过时确认零写。 |
| 重来／退出本局 | `PreviewEnd/Confirm` | 独立危险确认文案与按钮 | 二者不能混同；退出本局不新建 Attempt。 |
| 保存失败／未知 | 原 request、Retry/Resolve/End | RecoveryPopup/System status | 原 OperationId；Unknown 封冲突写；安全条件下才能 End。 |
| 胜利待结算 | Battle phase＋presentation token | BattleStatus＋Settle Button | 播放门未释放前按钮禁用；胜利已保存与奖励到账分开。 |
| 结果与四去向 | 原 receipt/next levels | ResultPage | 20 XP／2 锡片由真实结果显示且仅一次；无下一关常设。 |
| 根返回／退出应用 | HostSession.Back/Quit | QuitPopup | 取消保留上下文；确认不冒充退出本局。 |
| 字体许可 | Host 现有 fontLicense | LicensePopup | 可打开／关闭；资源缺失走不可恢复诊断。 |
| 更新已下载／移动数据 | SD13＋Q15/Q35，当前未实现 | 后续 PopupLayer 插槽 | 本轮仅保留布局／键，不冒充 YooAsset 已接入或下载已验。 |

## 7. r0 本地化、占位符与语言记忆（历史盘点；已由 §22 替代）

### 7.1 不可变要求

- 首发 locale 只含 `en` 与 `zh-Hans`。首次启动时所有中文系统语言映射
  `zh-Hans`，其余映射 `en`；手动选择覆盖系统默认并跨重启记住。
- 所有玩家可见 Prefab 的 TMP 默认文字使用完全一致、可静态扫描的
  `【if you see this, it is a bug.】`。最终中文／英文不能写入 Prefab
  掩盖漏绑定。
- 运行时文本通过稳定 key＋命名参数生成。同一 key 两语言参数集合必须
  完全相同；数字、物品名、错误码不靠随意字符串拼接改变语序。
- 缺键、重复键、未知 locale、参数缺失／多余／重复、模板解析失败和
  字体缺字均产生可定位诊断；玩家面保持显眼错误，不静默退回 Prefab
  成品或另一种语言冒充成功。
- 语言切换必须刷新当前已绑定树和以后重建页面；不得改变 controller、
  page token、OperationId、战斗播放 token 或保存头。

### 7.2 候选深模块

建议在 Presentation/Host 之间建立一个小 interface：按 key 和只读命名
参数返回本地化结果／诊断，并发布 `LocaleChanged`。生产 adapter 消费
Luban 生成且经项目校验的不可变文本表；测试 adapter 使用内存字典。
这样调用者不需要知道 CSV、Luban 生成格式或 YooAsset handle。具体
interface 名称、程序集归属和公开性等待 ARCH-ENG-001，避免先造浅层包装。

Prefab 上每个静态文本有显式 key 绑定；动态列表由 view 在生成 TMP 节点
时立即传 key/args，不允许先显示业务枚举字符串再由 Host 全树替换。
现 `FightMatchHostView.PlayerText` 和中英混合硬编码应被删除条件式替换，
而不是继续扩充 switch。

### 7.3 语言记忆决策点

语言必须在玩家档打开前即可用于 Startup/错误页，因此不应依赖当前
PlayerSave。推荐语义是“设备本地 UI 偏好”，由 Host 通过一个可测试存储
adapter 读取：无值→按 `Application.systemLanguage`；有合法值→使用手选；
写入完成后再宣布记住。实现可选既有平台文件原语或 `PlayerPrefs`，但这
会决定新序列化／清理／故障语义，须由 ARCH-ENG-001 固定并在实施包中
显式授权；本 r0 不自行选择。

## 8. 字体策略

- 保留现有 Noto Sans CJK SC OTF 和 OFL 原字节／GUID；它们覆盖英／简中
  方向且许可可继续在玩家界面查看。
- 新建独立 `TMPro.TMP_FontAsset`，不要把现 TextCore asset 原地改类型或
  复用 GUID。候选采用 Dynamic＋Multi Atlas，源 OTF 继续内嵌；确切 atlas
  尺寸、render mode、padding 和 fallback 要由一次资源生成包固定并记录。
- 所有 `TextMeshProUGUI`／`TMP_Dropdown`／`TMP_InputField` 显式绑定同一
  主字体或批准的 fallback；不使用 legacy `UnityEngine.UI.Text`。
- 静态扫描全部本地化表字符是补充检查；动态角色／物品／诊断参数还需
  运行时 `HasCharacters`／缺字诊断及英简中页面遍历。字体生成成功不等于
  Android 字形、内存或排版已验收。
- 现 TextCore asset（GUID `05ef1c5c9ac7e412caf6604e174ec91e`）只在
  所有 UIDocument/PanelSettings/构建检查引用清零、TMP 资源复开和回滚
  方案成立后，才进入后续清理候选。

## 9. Luban／YooAsset 最小版本与 adapter 待核项

### 9.1 Luban

当前只确认用户选择和“制作期接现有发布流程”，没有批准版本。冻结前
必须得到：

1. 官方 release/tag 或 commit、下载来源、SHA-256、许可证、Mac Intel 与
   Windows 同版本执行方式；禁止使用浮动 latest。
2. 唯一人工编辑根（Excel/CSV）、表 schema（key、en、zh-Hans、参数集合、
   页面／QA 元数据是否发布）、UTF-8/BOM/换行和重复键规则。
3. 生成目录与 Git 策略；生成 JSON/bytes 只能是派生产物，不可人工编辑；
   旧业务发布器仍是最终校验／不可变发布入口。
4. 项目 adapter 的输入格式、确定性生成、同输入跨 Mac/Windows 字节一致、
   非法模板／缺翻译停止发布的证据。
5. 当前 `planning-localization-draft.csv` 到正式表的单次映射；制作稿不得
   直接成为运行时权威或第二份长期编辑源。

### 9.2 YooAsset

仓库研究只把官方 `3.0.6` 当过事实样本，明确不是已批准安装版本。冻结前
必须得到：

1. 与 Unity `2022.3.18f1`、Android IL2CPP/ARM64、Mac Intel、Windows 的
   确切 package tag/commit、依赖、许可证和最小可构建样片。
2. 包名称、Builtin/Host play mode、包版本／清单／hash、R2 URL 与不可变
   命名、缓存根、校验、失败回退和 handle 释放职责。
3. 现六件发布物与新本地化表／Prefab／字体分别是基座内置、YooAsset
   built-in 还是远端内容；启动页所需英简中文本不能形成“先下载才能解释
   下载失败”的循环。
4. 断点续传最小文件阈值、Range/CDN 响应、同内容身份临时文件保留、重开
   续传和最终 hash；框架默认值不能当已开启证据。
5. Q35：Wi-Fi→移动数据时真正中止当前传输、显示剩余量并等待本次确认；
   `PauseDownload` 若只停止派发新文件则不足，必须以所选版本源码／设备
   行为验证。
6. 与 SD13 的 Prepared→RequestRestart→CodeEntered→BusinessReady 顺序、
   当前玩家摘要最终重核和旧集合保留；当前 uGUI 迁移不得私自缩短它。

版本未冻结前，C 包不得修改 Packages／lock、下载 SDK、生成缓存或宣称
Luban/YooAsset 已接入。

## 10. 路径矩阵 r0（历史候选；已由 §18 替代）

### 10.1 预计修改并保留原 `.meta`／GUID

```text
Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs
Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs
Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs
Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs
Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationController.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs
Assets/Scripts/FightMatch/Presentation/PlayerPermanentDetailView.cs
Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
Assets/Scripts/FightMatch/Host/FightMatchHostView.cs
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
Assets/Scenes/FightMatchDemo.unity
```

`PlayerNavigationController.cs` 只应移除／替换 `NavigationBindings` 的 UI
Toolkit 依赖；其 controller 逻辑不改。若架构决定把 binding 移到新文件，
则原 controller 的 public interface／meta 仍保留。

### 10.2 预计保持原字节（除非架构输入给出必要反例）

```text
Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs
Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackController.cs
Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackFrame.cs
Assets/Scripts/FightMatch/Presentation/PlayerBattleController.cs
Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs
Assets/Scripts/FightMatch/Host/FightMatchStreamingAssetsLoader.cs   # 在 YooAsset 单独包前
Assets/Scripts/FightMatch/Application/**
Assets/Scripts/FightMatch/Core/**
Assets/Scripts/FightMatch/Platform/**
Assets/Scripts/FightMatch/Input/**
```

### 10.3 预计新建（名称／拆分未冻结）

当时的候选最小集合如下；最终已由 §18/§20 冻结为一根 Prefab：

```text
Assets/UI/FightMatch/Runtime/
Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab
Assets/UI/FightMatch/Runtime/Templates/                 # 仅确有动态复用项时
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
Assets/Scripts/FightMatch/Presentation/Localization/    # interface、绑定、生成表 adapter 候选
Assets/Tests/EditMode/FightMatchUgui/                    # 真 uGUI EventSystem／Prefab 测试候选
```

新目录、脚本、Prefab、TMP asset 与测试的每个 `.meta` 都要在最终白名单
逐项列名，不允许用目录通配符授权。

### 10.4 删除候选（首包建议不删，接收后另清理）

```text
Assets/UI/FightMatch/FightMatchPanelSettings.asset
Assets/UI/FightMatch/FightMatchPanelSettings.asset.meta
Assets/UI/FightMatch/FightMatchTheme.tss
Assets/UI/FightMatch/FightMatchTheme.tss.meta
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset.meta
```

删除门：全 Assets 引用扫描为零；场景／Prefab 保存复开通过；构建检查已改
成 Canvas/EventSystem/TMP；准确 uGUI 候选通过 R；回滚不再依赖这些资产。
不得删除 OTF/OFL、FlowPuzzle Editor UI 或 Unity UIElements 模块。

## 11. GUID／`.meta` 与资源保存策略

- `FightMatchDemo.unity.meta` 的 GUID
  `1d5124e5b55fe409d8216e78a117dec0` 保留；场景内容迁移，不重建场景文件。
- `FightMatchPlayerHost.cs.meta` GUID
  `734984ebcddce44fab84ea4b5265f314` 保留；场景继续绑定同一 Host 类型。
- 现有被修改脚本一律保留 `.meta` 原字节。若改基类导致序列化字段替换，
  用 Unity SerializedObject／场景保存复开验证，不手改 GUID 冒充兼容。
- 新资产由同一 C 在唯一 Unity 实例中自然生成 `.meta`；生成后立即记录
  GUID、路径、类型、bytes/SHA，检查全 Assets GUID 唯一，再写场景／Prefab
  引用。不能复制旧 PanelSettings／TextCore GUID 给不同类型。
- Prefab、场景和 TMP 资源必须实际 SaveAssets／SaveScene、关闭、重新打开，
  核所有引用路径和组件类型；YAML 静态存在不足以证明引用有效。
- 首实施包保留旧 PanelSettings／Theme／TextCore FontAsset 作为可回滚实物，
  但场景和运行时不得继续引用；独立接收后再下精确清理包。
- 回滚以恢复同一场景／Prefab／脚本快照和移除本包新 GUID 为单位；不得
  清玩家档、重写历史提交或用删除旧证据回滚。

## 12. 测试迁移清单

### 12.1 直接依赖 UI Toolkit 的既有测试／fixture

```text
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTests.cs
Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs
Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs
Assets/Tests/EditMode/FightMatch/PlayerBattleFlowTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationSessionTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPermanentFlowTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs
```

其中 controller/session 断言应尽量保留原测试和原业务输入；只把
`EditorWindow.rootVisualElement`、`Q<T>`、`NavigationSubmitEvent`、UITK Pointer
事件、schedule／panel 断言替换为 uGUI GameObject、Prefab、EventSystem 的
真实 `ExecuteEvents`／raycast／Button.onClick 驱动。不能把“直接调用
controller 方法”当成真 uGUI 事件覆盖。

### 12.2 必须保留的旧行为组

- B16：按下零写、合法松手一次提交、双指不抢、取消／离开／失焦／几何
  变化零写、半开边界、阈值、历史短按、过时确认、SaveFailed／Unknown。
- P27：OrderedFacts、真计时、skip／close／detach／失焦／低内存／异常、
  rebind、不重播、旧 token 不释放新 token。
- B09/B10/B11/B12：页 epoch、旧按钮失效、整树重建、默认参考、回退范围、
  胜利门、receipt 和去向。
- CC：地图／队伍／背包、formation、永久操作、恢复、Back／Cancel／Return、
  immutable HostRequest、活动战斗准入。
- H01～H05：唯一 Host owner、根返回／退出、冷重建原结果、隐藏页面生命周期、
  场景中唯一 Host／Canvas／EventSystem／TMP／许可／Prefab 引用。

新增本地化测试至少覆盖：两语言键集合与参数集合完全一致、Prefab 占位符
扫描、全绑定后可见占位符为零、缺键／参数异常显眼失败、当前页与重建页
切换刷新、首次 locale 映射和手选跨重启记忆。

## 13. 分阶段验证预算草案

主测试拥有最终验证计划；本表只是给实施包的上限建议，复用旧 014／015／
016／018／019 证据且不重跑无变化的旧长套件。

| 阶段 | 目的 | 候选预算／停止条件 |
| --- | --- | --- |
| 静态 1 | 路径白名单、GUID 唯一、运行时 UI Toolkit 引用只剩 Editor、Prefab 占位符／本地化键、Packages 实际差异 | 每次源码冻结后 1 次；任何越界先停，不运行 Unity。 |
| 资源 1 | 生成／保存／复开 Prefab、TMP、场景；唯一 Canvas/EventSystem、Host 绑定、旧 Panel 引用零 | 1 次有效运行＋最多 1 次针对失败修正；不构建 APK。 |
| 编译 1 | 同一冻结源码与资源导入编译 | 1 次；源码再变才补 1 次。 |
| 聚焦 EditMode | 真 uGUI 输入、视图生命周期、本地化、Host 资源／路由 | 1 次；失败后只跑失败 fixture 至绿，再一次聚焦终核。 |
| 全量里程碑 | 证明未破坏业务／保存；基线已有 4181/4181 性能接收 | 设计／代码 R 认为变化稳定后最多 1 次无 filter；预计约 8～12 分钟，禁止角色重复运行。 |
| Android Build | 新 uGUI 正常包＋必要 QA 包；修正旧 019 原始字符串误报 | 每种最终候选各 1 次；构建失败只重试受影响包；不得复用旧 Toolkit APK。 |
| 设备／画面 | PQA-01～15 中与本轮变化相关场景；模拟器＋iQOO 事实分开 | 每候选一次正常链，必要故障／冷启动另列；缺设备保持 NOT VERIFIED。 |

旧证据仍证明原业务版本，不证明新 Canvas、EventSystem、双语、TMP、安全区
或触摸。源码／Prefab／本地化数据任一改变后，设备证据必须绑定新 APK
SHA、包名、证书和资料根；不能拿 019 旧 APK 补签本轮。

## 14. r0 C 实施白名单草案（历史记录；不可签发，已由 §17～§18 替代）

只有 ARCH-ENG-001、主策／主美最终输入、主测试验证计划和 R 设计 ACCEPT
齐备后，主程才能把下表展开成逐文件包：

1. **UGUI-01 结构与输入**：Presentation 九个旧视图／绑定、Host 两文件、
   单一 RuntimeRoot Prefab、TMP asset、场景、对应 uGUI tests；Core／
   Application／Platform／HostSession 保持原字节。
2. **LOC-01 制作与运行绑定**：固定 Luban 版本／schema／生成产物、项目内
   catalog／binding、语言偏好 adapter、文本测试。任何 PlayerSave 格式或
   Packages 变化必须单独列受保护变更。
3. **RES-01 YooAsset 最小接入**：仅在版本研究和启动资源分层冻结后，列
   Packages／lock、资源包、加载 adapter、缓存／续传／Q35 验证；不与
   UGUI-01 静默捆绑。
4. **BUILD-01**：AndroidBuild 的 Canvas/EventSystem/TMP/Prefab 保存复开
   检查、旧 019 验证器最小纠正和新证据入口；不得改签名／包设置之外的
   未授权项。
5. **CLEAN-01**：独立接收后删除 PanelSettings／Theme／TextCore FontAsset
   及其 meta；先核引用零和可回滚快照。

每包须列所有新 `.meta`、Prefab 内部引用、公共类型基类变化、序列化字段、
Packages／ProjectSettings 权限、运行次数、证据路径和停止条件。C 仍是
唯一串行本机 Unity 执行者；主程／本线程不运行 Unity。

## 15. 回滚、故障与当前阻塞

- UI 构建／复开失败：保留失败 scene/prefab/log；不覆盖已工作的旧场景，
  回到本包前同一 GUID 快照；不以重建整个 hierarchy 代替诊断。
- 输入失败：停止设备／全量验证，先用最小真 EventSystem 见证定位坐标、
  raycast、pointer ownership 或生命周期；每次修正一项并回滚失败尝试。
- 本地化失败：保持诊断占位可见并阻止产品接收；不写死中文／英文救场；
  生成表错误回到唯一 Excel/CSV 源和 Luban 诊断。
- 字体缺字／内存异常：保留原 OTF/OFL 与旧 TextCore 资产；调整新 TMP
  asset 的生成设置后重新保存复开，不替换为无许可字体。
- YooAsset／Luban 版本或包安装失败：UGUI-01 独立成果可保留，但不能把
  LOC-01/RES-01 标完成；不改用 Localization/Addressables、自研下载器或
  浮动最新版解围。
- 保存／业务失败：不得清档、重 Prepare、换 OperationId 或修改断言；交回
  原 Application／HostSession 负责人，复用已有恢复入口。
- r0 当时设计输入已齐、仍待独立 R；该历史门现已由 §26 记录的 FIX-04
  `ACCEPT` 关闭，本次 COPY identity 同步不重开整篇设计评审。
  Luban、YooAsset、API 24 和 019 验证器不阻断首包的结构迁移，但各自仍是
  后续受保护包的前置／发布门，不能在首包内假装完成。

## 16. 本轮未执行

未修改任何产品文件、测试、场景、资源、`.meta`、Packages、ProjectSettings
或 Git；未运行 Unity、编译、测试、构建、模拟器、真机、Luban、YooAsset、
下载、云服务或外部发布。本文 r0 只记录仓库只读盘点和待冻结候选。

## 17. r1 冻结范围、优先级与包边界

### 17.1 本轮可审查首包 `UGUI-01`

`UGUI-01` 是唯一在本设计中达到“可由主程展开为 C 包”的实现单元。它完成：

1. 将首 Demo 的运行时页面、棋盘、HUD、弹窗和 Host 从 UI Toolkit 换成
   uGUI Canvas／EventSystem／TMP 中性灰盒；
2. 保留现有 Controller、HostSession、Application、Core、Save、Content 和
   `RouteGesture` 的业务真值；
3. 建立项目自有本地化最小合同、可注入的内存测试 source 和 TMP binding；
   不生成正式文本表，不把内存 source 装成产品运行时权威；
4. 建立稳定页面／控件 ID、真实 EventSystem 测试、场景／Prefab 保存复开
   检查和 Host 生命周期测试；
5. 保留旧 PanelSettings、TSS 和 TextCore FontAsset 实物用于回滚，但新玩家
   入口对它们的引用必须为零。

它明确**不是**可试玩／可发行产品候选：没有 `LOC-TOOL-01` 的正式生成表时，
运行时只允许停在可诊断的本地化未就绪态；没有 `RES-01` 时不得宣称 YooAsset
接入；没有 `ANDROID-API24-01` 时不得宣称首发最低系统已落实；没有
`APK-STRUCT-01` 与设备门时不得宣称正常 APK 可交付。

### 17.2 后续独立受保护包

| 包 | 固定目的 | 前置／不得混入 |
| --- | --- | --- |
| `LOC-TOOL-01` | 锁 Luban 版本、schema、唯一 CSV 权威、确定性生成、正式 catalog、Locale Preference Store | 版本样片和主策文本差异先审；不得顺手加 YooAsset。 |
| `RES-01` | 锁 YooAsset 版本并实现项目资源租约、built-in bootstrap、远端 ReleaseSet 取得 | 包依赖和 lock 独立授权；完整 HybridCLR／后台不在内。 |
| `ANDROID-API24-01` | 把 `AndroidMinSdkVersion` 从 22 精确改为 API 24，并同步构建断言 | 只改准确 ProjectSettings／构建验证；不混 UI 重构。 |
| `APK-STRUCT-01` | 新增结构化程序集／类型检查，QA APK 正对照、正常 APK 负对照 | 不修改冻结的旧 029 证据或以字符串白名单放宽。 |
| `UGUI-CLEAN-01` | 在新候选独立接收后删除旧运行时 Panel/TSS/TextCore 资产 | 必须先满足第 24.4 节删除门。 |

依赖顺序为 `UGUI-01 + LOC-TOOL-01 -> RES-01 -> ANDROID-API24-01 +
APK-STRUCT-01 -> 构建／设备`；`ANDROID-API24-01` 与 `APK-STRUCT-01` 可在
互不写同一文件时并行设计，但本机 Unity 仍由 C 串行执行。

## 18. `UGUI-01` 精确文件白名单与所有权

### 18.1 修改并保留现有 GUID／`.meta`

```text
Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef
Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs
Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs
Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs
Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs
Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationController.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationView.cs
Assets/Scripts/FightMatch/Presentation/PlayerNavigationRecoveryView.cs
Assets/Scripts/FightMatch/Presentation/PlayerPermanentDetailView.cs
Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef
Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
Assets/Scripts/FightMatch/Host/FightMatchHostView.cs
Assets/Scripts/FightMatch/Host/Editor/FightMatch.Host.Editor.asmdef
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
Assets/Scenes/FightMatchDemo.unity
Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTestData.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackPanelTests.cs
Assets/Tests/EditMode/FightMatch/CandidateBattlePlaybackTests.cs
Assets/Tests/EditMode/FightMatch/PlayerBattleTestFixture.cs
Assets/Tests/EditMode/FightMatch/PlayerBattlePresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerDefaultReferenceTests.cs
Assets/Tests/EditMode/FightMatch/PlayerBattleFlowTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationTestFixture.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPresentationTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationSessionTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationPermanentFlowTests.cs
Assets/Tests/EditMode/FightMatch/PlayerNavigationRecoveryTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef
Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs
```

`FightMatchAndroidBuild.cs` 在首包只替换 PrepareResources／场景结构检查；其
Min SDK 22 断言留给 `ANDROID-API24-01`，其旧字符串式 APK 检查留给
`APK-STRUCT-01`。这两个保留点必须以 TODO／测试失败门可见，不能误报完成。

### 18.2 新建；每项同时允许同路径 `.meta`

```text
Assets/Scripts/FightMatch/Presentation/Localization.meta
Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs
Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs.meta
Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs
Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs.meta
Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs
Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs.meta
Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs
Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTmpText.cs.meta
Assets/Scripts/FightMatch/Presentation/FightMatchStandaloneInputModule.cs
Assets/Scripts/FightMatch/Presentation/FightMatchStandaloneInputModule.cs.meta
Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs
Assets/Scripts/FightMatch/Presentation/FightMatchViewId.cs.meta
Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs
Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs.meta
Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs
Assets/Scripts/FightMatch/Presentation/SafeAreaFitter.cs.meta
Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs
Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs.meta
Assets/UI/FightMatch/Runtime.meta
Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab
Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab.meta
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset.meta
Assets/TextMesh Pro.meta
Assets/TextMesh Pro/Resources.meta
Assets/TextMesh Pro/Resources/TMP Settings.asset
Assets/TextMesh Pro/Resources/TMP Settings.asset.meta
Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt
Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt.meta
Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt
Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt.meta
Assets/TextMesh Pro/Shaders.meta
Assets/TextMesh Pro/Shaders/TMP_SDF.shader
Assets/TextMesh Pro/Shaders/TMP_SDF.shader.meta
Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader
Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader.meta
Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc
Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc.meta
Assets/TextMesh Pro/Shaders/TMPro.cginc
Assets/TextMesh Pro/Shaders/TMPro.cginc.meta
Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs
Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs.meta
Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs
Assets/Tests/EditMode/FightMatch/LocalizationContractTests.cs.meta
Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs
Assets/Tests/EditMode/FightMatch/LocalizedTextBindingTests.cs.meta
Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs
Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs.meta
```

项目自有脚本、Prefab 和 Noto TMP asset 的 `.meta` 由唯一 Unity 实例自然生成；
上表已经逐项列名，不得出现其它新文件或子资产外置纹理。Noto TMP 字体材质
与 atlas 必须作为同一 `.asset` 的 sub-assets，避免未列路径。

FIX-03 对首包白名单的净变化精确为：在本节新增上述
`HostAssemblyInfo.cs/.meta` 两个路径；`FightMatch.Core.Tests.asmdef` 已在 §18.1
修改清单内，只扩大其允许的单字段差异，不再增加路径。规范性代码块当前为
§18.1 的 36 个现有修改路径＋§18.2 的 49 个新建路径＝85 个显式路径；FIX-03
相对上版只使新建数从 47 增至 49。其余白名单和所有权不变。

`Assets/TextMesh Pro/**` 是唯一例外：不自然生成 GUID，也不执行菜单的整包
`Import TMP Essential Resources`。C 只从仓库当前已安装的以下源包选择性复制
下列 `asset`／`asset.meta` 条目；目录也复制其官方 `asset.meta`：

```text
Library/PackageCache/com.unity.textmeshpro@3.0.6/Package Resources/TMP Essential Resources.unitypackage
```

路径、源条目 GUID 与用途冻结如下：

| 项目路径 | 必须保留的官方 GUID | 用途 |
| --- | --- | --- |
| `Assets/TextMesh Pro.meta` | `f54d1bd14bd3ca042bd867b519fee8cc` | 官方根目录 meta |
| `Assets/TextMesh Pro/Resources.meta` | `243e06394e614e5d99fab26083b707fa` | Resources 目录 meta |
| `Assets/TextMesh Pro/Resources/TMP Settings.asset(.meta)` | `3f5b5dff67a942289a9defa416b206f3` | `TMP_Settings.instance` |
| `Assets/TextMesh Pro/Resources/LineBreaking Leading Characters.txt(.meta)` | `d82c1b31c7e74239bff1220585707d2b` | CJK 禁止行首字符 |
| `Assets/TextMesh Pro/Resources/LineBreaking Following Characters.txt(.meta)` | `fade42e8bc714b018fac513c043d323b` | CJK 禁止行尾字符 |
| `Assets/TextMesh Pro/Shaders.meta` | `e9f693669af91aa45ad615fc681ed29f` | shader 目录 meta |
| `Assets/TextMesh Pro/Shaders/TMP_SDF.shader(.meta)` | `68e6db2ebdc24f95958faec2be5558d6` | `TextMeshPro/Distance Field` runtime shader |
| `Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader(.meta)` | `fe393ace9b354375a9cb14cdbbc28be4` | 标准 SDF shader 声明的 `TextMeshPro/Mobile/Distance Field` fallback |
| `Assets/TextMesh Pro/Shaders/TMPro_Properties.cginc(.meta)` | `3997e2241185407d80309a82f9148466` | `TMP_SDF.shader` 的直接 include |
| `Assets/TextMesh Pro/Shaders/TMPro.cginc(.meta)` | `407bc68d299748449bbf7f48ee690f8d` | `TMP_SDF.shader` 的直接 include |

上表精确为 10 个官方路径项（目录项只有 meta，文件项为 asset＋meta）；不得
以“Essential Resources”名义多复制第 11 项。

Mobile fallback 的源字节也冻结：`asset` SHA-256
`970db4b1e73b8289c1dd56cff60877f426b49c9682620b2d14da6cd70b64cb9c`，
`asset.meta` SHA-256
`f2112f50c72762d4e6731391495ea11f76975d677b698e9bcfdd38275ea18f9d`。
它的直接 include 只有已列入白名单的 `TMPro_Properties.cginc`；不得删改标准
`TMP_SDF.shader` 中的官方 fallback 声明，也不得以复制 shader 源码或改名
规避该依赖。

复制前对全 `Assets` 做 GUID 查重；任一 GUID 已被别处占用即 `BLOCKED`，不可
重写 GUID。复制后先生成 Noto TMP asset，再用 SerializedObject 将官方
`TMP Settings` 的 `m_defaultFontAsset` 改绑该 Noto asset，保留
`m_leadingCharacters/m_followingCharacters` 对上述两份换行表的官方 GUID，
清空 `m_fallbackFontAssets`、`m_defaultSpriteAsset` 和
`m_defaultStyleSheet`，并把 `m_defaultSpriteAssetPath`、
`m_defaultColorGradientPresetsPath` 置空、`m_enableEmojiSupport` 置 0；
`m_defaultFontAssetPath` 只作目录提示，运行时默认字体必须以直接对象引用为准。
从而移除未纳入白名单的 Liberation font、Emoji sprite、Default Style Sheet
悬空引用；不得导入 Examples & Extras、其它 Essential 资产或玩家可见成品
文字。最后 `SaveAssets`，关闭并重开项目；必须证明
`TMP_Settings.instance`、两份换行表、Noto font/material 均非空，
`Shader.Find("TextMeshPro/Distance Field")` 与
`Shader.Find("TextMeshPro/Mobile/Distance Field")` 均命中，标准 shader 的
fallback 仍指向后者，并且没有 missing reference 或未解析 shader dependency。

### 18.3 首包明确禁止修改／删除

`Packages/**`、`ProjectSettings/**`、全部 Core/Application/Platform/Content/
Input 源码、`FightMatchHostSession.cs`、三个保留 Controller、
`CandidateBattlePlaybackFrame.cs`、`FightMatchStreamingAssetsLoader.cs`、
所有 StreamingAssets、所有旧 UI Toolkit 资源及 `.meta` 均保持原字节。
首包没有删除路径。共享文件唯一写入者是 C；主程、系统设计、R、QA 只读。

## 19. 程序集与公开表面冻结

### 19.1 FIX-03 原因与最小 seam

现有程序集事实是：`FightMatch.Host` 没有任何 `InternalsVisibleTo`；
`FightMatch.Host.Tests` 已引用 Host，而 `FightMatch.Core.Tests` 没有 Host 引用。
若 `FightMatchHostView.Bind` 接收 internal `LocalizationService`，把 Bind 暴露为
public 会在 public interface 泄漏 internal 类型，同时 Host.Tests 仍不能编译
调用 Host 的 internal Bind。另一方面，冻结在 Core.Tests 的
`UguiSceneCompositionTests`／`LocalizedTextBindingTests` 必须实例化真实 Host
装配并观察真实 diagnostic gate；只用字符串 `GetType` 数量检查没有覆盖行为。

FIX-03 保持同一小 interface，选择测试直接跨 internal seam 编译访问：Host 只向
两个既有测试程序集开放 internals，Core.Tests 只增加对 Host 的测试引用。
Host 的产品依赖已指向 Application/Presentation/Content/Platform/Core；反向新增
的是“测试程序集 → Host”，不会形成产品程序集循环，也不新增运行时依赖。这比
扩大 public interface、测试镜像或运行时 test hook 更小，并把 Host 装配／诊断
行为的验证保持在其真实 implementation 上。

FIX-03 前后差异图；未列边保持原样：

```text
old: FightMatch.Core.Tests -X-> FightMatch.Host
     FightMatch.Host       -- no test friends

new: FightMatch.Core.Tests  -> FightMatch.Host
     FightMatch.Host internals -> FightMatch.Core.Tests, FightMatch.Host.Tests
```

### 19.2 最终引用图

```text
FightMatch.Core             -> FlowPuzzle.Core                         [不变/noEngine]
FightMatch.Platform         -> FightMatch.Core                         [不变/noEngine]
FightMatch.Content          -> Core, Platform, FlowPuzzle.Core/Validation [不变/noEngine]
FightMatch.Application      -> Core, Platform, Content, FlowPuzzle.Core, QFramework [不变/noEngine]
FightMatch.Input            -> FlowPuzzle.Core                         [不变/noEngine]
FightMatch.Presentation     -> Input, Application, Core, Platform,
                               FlowPuzzle.Core, QFramework,
                               UnityEngine.UI, Unity.TextMeshPro
FightMatch.Host             -> Application, Presentation, Content,
                               Platform, Core, QFramework,
                               UnityEngine.UI, Unity.TextMeshPro
FightMatch.Host.Editor      -> Host, UnityEditor.TestRunner,
                               UnityEngine.TestRunner, UnityEngine.UI,
                               Unity.TextMeshPro
FightMatch.Core.Tests       -> 现有引用 + FightMatch.Host
FightMatch.Host.Tests       -> 现有引用 + UnityEngine.UI + Unity.TextMeshPro
```

`FightMatch.Core.Tests.asmdef` 的唯一字段变化是在现有 `references` 数组新增
`"FightMatch.Host"`；其它引用、字段、平台和可选测试引用保持原字节语义。
`FightMatch.Host.Tests.asmdef` 已有 Host 引用，FIX-03 不修改它。首包不新增
运行时 asmdef。`LocaleId`、`ILocalizedTextSource`（定义在
`LocalizedTextSource.cs`）、`LocalizedTextResult` 和 `LocalizationService`
是 Presentation 内部接入合同并保持 `internal`。Presentation 的可见性载体仍是
`Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs`，内容
只允许三个 assembly attribute：

```csharp
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Host")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Core.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Host.Tests")]
```

因此 Host 可装配本地化服务，两测试程序集可直接覆盖内部合同；不得把这些
类型改 public；这三个 Presentation friends 精确不变，不得新增第四个 friend。
不要为每个 TMP 字段建立 Interface。

Host 的唯一可见性载体是 FIX-03 新文件
`Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs`，内容只允许以下两个
assembly attribute，不得有 using、类型或第三个 friend：

```csharp
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Core.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.Host.Tests")]
```

因此两个测试程序集可直接编译访问 internal `FightMatchHostView.Bind`、真实 Host
装配和 diagnostic gate；禁止反射、`SendMessage`、字符串 `GetType` 代替行为
验证，也禁止复制 Host 行为到测试镜像。不得把 `LocalizationService` 或 Bind
改 public，不新增 interface、运行时 test hook、define 或测试专用产品入口。

`FightMatchStandaloneInputModule` 是场景序列化组件，冻结为 `public sealed`
Presentation 类型；其归一化 helper 及 `CandidateBoardElement` 的 true-touch
cancel 入口均保持 `internal`，由上述两个测试 friend 直接覆盖，不得形成业务
层公开输入 API。

现有 public View 类型以**同名原地替换**冻结：`CandidateBoardElement` 改为
`MaskableGraphic`，其余旧 View 改为 `MonoBehaviour` Binder；构造器调用改为
序列化引用＋`Bind/Unbind`。这是 Presentation/Host public 源码表面变化，
仓内调用点仅 Host 与上述测试，必须一次迁完。Core/Application public API、
PlayerSave、ContentBinding、请求／结果、OperationId／CommitId 全部零变化。

### 19.3 FIX-03 风险与独立 R 清单

主要风险只有 assembly 名拼错导致 friend 无效、Core.Tests asmdef 出现额外字段
漂移、以及把测试可见性误扩成产品 public 表面；测试程序集引用 Host 本身不进入
Player，也不改变 Host 的产品依赖图。独立 R 必须逐项确认：

1. `HostAssemblyInfo.cs` 仅有精确两个 fully-qualified attributes，`.meta` 唯一；
2. `PresentationAssemblyInfo.cs` 的 Host/Core.Tests/Host.Tests 三个 friend 原字节
   不变，Host 没有第三 friend；
3. `FightMatch.Core.Tests.asmdef` 仅新增 `FightMatch.Host` 引用，其余字段和顺序
   无无关改写；`FightMatch.Host.Tests.asmdef` 无差异；
4. `LocalizationService`、Bind 和其它 internal seam 没有 public 扩张，且没有
   reflection、`SendMessage`、字符串 `GetType`、define、test hook 或测试镜像；
5. 实际 compile 与冻结的 207 target 证明两个测试程序集直接跨 seam 工作；
   数量、filter 和断言合同没有因可见性修正而改变。

## 20. uGUI Prefab、控件 ID 与控制器绑定

### 20.1 单 Prefab 层级

`FightMatchRuntimeRoot.prefab` 冻结为一根 Prefab，不另建页面 Prefab：

```text
FightMatchRuntimeRoot [FightMatchHostView]
└── RuntimeCanvas [Canvas:Overlay, CanvasScaler 540x960 Match=.5, GraphicRaycaster]
    ├── BackgroundLayer                       raycastTarget=false
    └── SafeAreaRoot [SafeAreaFitter]
        ├── ScreenLayer
        │   ├── StartupPage
        │   ├── NavigationPage
        │   │   ├── MapSection
        │   │   ├── PartySection
        │   │   ├── InventorySection
        │   │   └── PreparationSection
        │   ├── BattlePage
        │   │   ├── BattleStatus
        │   │   └── BoardFrame [CandidateBoardElement]
        │   └── ResultPage
        ├── HudLayer
        ├── PopupLayer
        │   ├── LanguagePopup
        │   ├── ReferencePopup
        │   ├── ConfirmationPopup
        │   ├── RecoveryPopup
        │   ├── LicensePopup
        │   └── QuitPopup
        ├── SystemLayer
        │   ├── LoadingOverlay
        │   └── BlockingDiagnostic
        └── TemplatePool                     inactive; dynamic row/button prototypes
```

场景保留 `FightMatch` Host 物体和 scene GUID，在其下放一个 Prefab instance；
场景另有且仅有一个 `EventSystem + FightMatchStandaloneInputModule`；后者是
`StandaloneInputModule` 的受限子类，唯一扩展是 §21 的真实触摸取消前置
归一化。Prefab 内不放 EventSystem，避免嵌套实例产生双 EventSystem。
Canvas、可见 TMP、按钮、ScrollRect、Dropdown/InputField 均使用
fully-qualified uGUI/TMP 类型；普通热区至少 48×48、净距至少 8。装饰
`raycastTarget=false`，模态遮罩为 true。

### 20.2 稳定测试 ID

`FightMatchViewId` 保存不可本地化、不可复用的序列化 ID。冻结根集合：

```text
fm.page.startup  fm.page.navigation  fm.page.battle  fm.page.result
fm.section.map   fm.section.party    fm.section.inventory  fm.section.preparation
fm.popup.language fm.popup.reference fm.popup.confirmation fm.popup.recovery
fm.popup.license fm.popup.quit       fm.popup.blocking
fm.board.candidate
fm.action.profile.create fm.action.profile.continue fm.action.language.open
fm.action.map.level01 fm.action.party.confirm fm.action.entry.enter
fm.action.battle.retry fm.action.battle.resolve fm.action.battle.skip
fm.action.history.open fm.action.reference.open fm.action.victory.settle
fm.action.result.map fm.action.result.party fm.action.result.inventory
fm.action.result.replay fm.action.back fm.action.close fm.action.quit.confirm
```

动态 member/row ID 只能由稳定业务 ID 追加，例如
`fm.member.{characterId}`，不使用本地化文本或 sibling index。测试以 ID 找控件，
不依赖 GameObject 显示名；运行时发现空、重复或未知固定 ID 即阻断装配。

### 20.3 绑定与时序

| 旧 View | 新同名 Binder | 输入／输出与解绑 |
| --- | --- | --- |
| `CandidateBoardElement` | `MaskableGraphic`＋EventSystem handler | 生成同语义 `PointerSample`；`OnDisable/Unbind` 取消 active pointer；只消费 frame 绘制。 |
| `CandidateBoardInputView` | Battle 输入 Binder | member/retry/resolve 真实 `Button.onClick`；100ms unscaled Refresh；解绑移除 listener、清 dynamic rows。 |
| `CandidateBattlePlaybackView` | playback Binder | 16ms `Update` 累计实际 unscaled ms；skip/异常/禁用只处理自己的 token。 |
| `PlayerDefaultReferenceView` | modal Binder | 自有 generation/Update；首个真实 drag down 关闭且继续当前手势；不报告 battle token。 |
| `PlayerBattleView` | BattlePage Binder | Bind 捕获页 epoch；history/confirm/recovery/result 字段来自 controller；隐藏不等于完成。 |
| `PlayerNavigationView` | NavigationPage Binder | Map/Party/Bag/Prep 容器显隐；只转发 controller intent；动态行从 inactive TemplatePool 克隆。 |
| `PlayerNavigationRecoveryView` | RecoveryPopup Binder | 原 request/operation；Retry/Resolve/End 严格按可用性开放。 |
| `PlayerPermanentDetailView` | detail Binder | TMP_InputField/TMP_Dropdown 显式解析；非法值零写。 |
| `NavigationBindings` | 从 Controller 文件移除，由 `PlayerNavigationView` 拥有 | Controller 保持 epoch/冻结 target；旧 listener 不得命中新实例。 |
| `FightMatchHostView` | Prefab 根路由 Binder | 只消费 `FightMatchHostSession.Page`；通过 §19 的 `FightMatch.Host` friend 直接绑定 internal `LocalizationService`；不保留 PlayerText/string.Replace。 |

每个 Bind 先 Unbind，递增本地 generation，保存 controller/epoch，再注册；每个
Unbind 顺序为停止 tick→取消 pointer/overlay→移除 listener→退订事件→清引用。
页面连续进入退出三次后 listener 数不增长。Host pause/focus/lowMemory 先取消
手势，再调用既有 `PausePresentation()`；Destroy 先拆 View，后 Dispose Session。

### 20.4 FIX-04 失焦生产 seam

`FightMatchPlayerHost` 新增生产用 `internal void HandleApplicationFocus(bool focused)`。
它不是 test-only hook，也不改变 Unity 回调可见性：现有私有 Unity message 必须是
无分支、单行、单次委托，精确写成
`private void OnApplicationFocus(bool focused) => HandleApplicationFocus(focused);`。
handler 的全部行为冻结为：`focused == false` 时精确调用一次
`runtimeRoot?.PausePresentation()`；`focused == true` 时零调用、零状态变化。
`FightMatchHostView.PausePresentation()` 继续是唯一真实收口，先
`board?.CancelPointer()`，再 `session?.PausePresentation()`；不得在 Host、测试或
新 adapter 复制这两步。

该 internal 方法是 Unity message 与确定性 EditMode 调用者共用的生产入口；若
移除它，测试只能反射、`SendMessage` 或复制分支，故它属于真实生产 seam，而非
为了测试暴露的新产品 API。FIX-03 已冻结的 Host 两个 friend 使两个现有测试
程序集可直接调用它；FIX-04 不新增 friend、asmdef 引用、文件或 case，也不扩大
public surface。

`CandidateBoardInputTests.B16B04_CancelCaptureOutBlurDetachCloseGeometrySelectionAndOutsideNeverSubmit`
的 `blur` 分支和
`CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources`
的 `blur` 分支都必须装配真实 `FightMatchPlayerHost` 与真实
`FightMatchHostView`/root，把各自已有 board、controller、session 置于该真实 root
下，然后直接调用 `HandleApplicationFocus(false)`。前者观察既有
`CancelPointer()` 语义（active gesture/capture 清除、零提交）；后者观察既有
`PausePresentation()` 语义（清 override、读取最新头、只报告自己的 token，旧
A1 不释放后续 A2）。每个分支还须调用 `HandleApplicationFocus(true)`，证明无
取消、无完成报告和无业务写。禁止反射、`SendMessage`、测试镜像、手动调用
`Update`、用 `SetActive(false)`／`OnDisable` 等等价路径替代或新增 public/test-only
入口。

FIX-04 只关闭 focus seam。现有私有 `OnApplicationPause(bool)` 和 `LowMemory()`
维持原实现与验证，不抽取 `HandleApplicationPause`、`HandleLowMemory` 或其它新
生命周期入口。风险仅限三项：Unity message 未精确委托一次、handler 在
`focused == true` 时误动作、测试仍走替代路径；分别由 Q1 静态门与 Q4 两个真实
Host 行为分支阻断，真实 OS→Unity message 投递则只由 Q7 设备门证明。

## 21. 棋盘 EventSystem 与几何冻结

- `CandidateBoardElement` 实现 `IPointerDownHandler`、
  `IInitializePotentialDragHandler`、`IDragHandler`、`IPointerUpHandler`、
  `IEndDragHandler` 和 `IPointerExitHandler`，另提供 internal
  `CancelTrueTouch(pointerId, screenPosition)` 给项目 input module。
  **不实现 `ICancelHandler` 来代表 `TouchPhase.Canceled`**；uGUI 的
  `ICancelHandler` 是导航 cancel，不携带这项触摸语义。
  `InitializePotentialDrag` 设置 `useDragThreshold=false`，Unity 默认阈值不
  替换现有 `RouteGesture` 阈值。
- `FightMatchStandaloneInputModule.Process()` 必须在调用 `base.Process()` 前
  扫描 `input.GetTouch(i)`：忽略 `TouchType.Indirect`；对每个
  `TouchPhase.Canceled`，从继承可见的 `m_PointerData[fingerId]` 取得既有
  `pointerDrag ?? pointerPress`，从该目标向父级解析现有
  `CandidateBoardElement`，并同步调用其 internal `CancelTrueTouch`。因此
  owner 的草稿和捕获在 `StandaloneInputModule` 随后的 PointerUp/EndDrag 前
  已经清除；随后同帧的
  Up/EndDrag 仍可由基类派发，但在棋盘端必须是无 owner 的零提交 no-op。
  禁止复制整份 StandaloneInputModule 源码或从帧末 `ICancelHandler` 补偿。
- 鼠标仅左键；触摸取 down 时的 `pointerId` 为唯一 owner。owner 活动时其它
  pointer 的 down/move/up/cancel 均零效果；不切换 owner。
- Up 与 EndDrag 进入同一 `FinishPointer(pointerId, position)`，用 generation＋
  finished 标志去重。正常 `TouchPhase.Ended` 即使同帧收到 Up 与 EndDrag 也
  精确提交一次。Exit、真实 touch cancel、经 §20.4 handler 的失焦、pause、OnDisable、尺寸／
  安全区版本变化调用同一 `CancelPointer`，不提交业务；非 owner 的 cancel
  不得清掉 owner。
- 通过 `RectTransformUtility.ScreenPointToLocalPointInRectangle` 使用
  `pressEventCamera` 得到 `localPoint`；以 `boardRect.rect` 左下半开区间和
  同一个正方格 `cell` 映射，公式唯一为
  `x=floor((localX-xMin)/cell)`、`y=floor((localY-yMin)/cell)`，**不再翻 Y**。
  先拒绝 `localX<xMin || localY<yMin || localX>=xMax || localY>=yMax`，再验证
  `0<=x<columns && 0<=y<rows`；`x==xMax`／`y==yMax` 是 outside。
- `CandidateBoardElement.OnPopulateMesh` 必须使用完全相同的 rect/cell 源；
  格心唯一为 `(xMin+(x+0.5)*cell, yMin+(y+0.5)*cell)`，路线、端点、草稿和
  命中测试都使用该坐标，禁止任何第二次 top-left/bottom-left 变换。
- mouse/touch 基础阈值仍为 6/10，并按 `cell * 0.15..0.25` clamp；所有距离
  使用同一局部 Canvas 单位。540×960、1080×2400、非零四边 inset 只允许
  产生等价格命中。
- `CandidateBoardElement.OnPopulateMesh(VertexHelper)` 绘背景、格、端点、
  已锁路线、草稿、playback/reference overlay；仅本体交互 Graphic 可 raycast，
  其它覆盖 Graphic 关闭 raycast。绘制层不持有 Application 状态。

## 22. 本地化、COPY-001、语言设置与字体合同

### 22.1 首包最小接口

`LocaleId` 仅有 `En` 和 `ZhHans`。系统语言规范化：
`ChineseSimplified`、`Chinese` 及实际平台返回的中文变体统一为 `ZhHans`；
其它值为 `En`。`ILocalizedTextSource.Resolve(key, locale, namedArgs)` 返回
`LocalizedTextResult`（成功文本＋业务 severity，或失败时稳定 binding
diagnostic code），
`LocalizationService` 拥有当前 locale 和 `LocaleChanged`。首包测试 source
只存在测试 fixture 内；产品 Host 若没有正式 source，显示 BlockingDiagnostic，
不把 216-key 制作稿 CSV 编进代码或把内存测试 source 装成运行时权威。

`LocalizedTmpText` 的 Prefab 初值必须逐字为
`【if you see this, it is a bug.】`；Bind 成功才替换。动态 TMP 由 Binder 创建
后在首次激活前调用同一 service，不先闪现枚举／ID／英文硬编码。

### 22.2 COPY-001 到正式 Luban 表的映射

固定制作输入是
`docs/team/2026-09-30/planning-localization-draft.csv`，SHA-256
`e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`，
50990 bytes，217 物理行＝表头＋216 数据行，9 列、216/216 唯一 key。它已获
COPY 独立 R `ACCEPT`，但仍只是制作稿和内存测试 source 的固定输入；
`LOC-TOOL-01` 完成以下单次迁移后，正式表才成为运行时权威：

当前文件前 172 物理行（表头＋原 171 数据行）SHA-256 为
`c4fc583344392153e97a096b68f5d740d8ea161d1bada87bae42343b20b6fb4c`；
COPY FIX-01 只在其后追加 45 键，原 171 行字节保持不变。计数增加不改变既有
key 的语义、参数或自然可达性。

| CSV 列 | 正式 schema | 运行时 |
| --- | --- | --- |
| `key` | 主键 string；必须 `fm.` 前缀、ordinal 唯一 | 保留 |
| `screen/element/state_or_trigger` | authoring context；非空 | 不发布或仅诊断 metadata |
| `zh_cn` | locale `zh-Hans` | 发布 |
| `en` | locale `en` | 发布 |
| `parameters` | `;` 分隔、ordinal 排序后的具名参数声明 | 发布并与模板双向校验 |
| `severity` | `blocking/error/warning/info` 枚举 | 发布 |
| `qa_notes` | 制作／QA 注释 | 不进玩家包 |

正式人工源冻结为 `Config/FightMatch/Localization/FightMatchText.csv`；迁移后
团队 draft 只保留历史证据，不再双向编辑。schema、生成命令和输出路径须由
`LOC-TOOL-01` 在锁定 Luban 后补齐；在此之前不得创建该正式路径冒充完成。

硬编码迁移全量边界如下：

| 旧来源 | 必须改为的 COPY key 组 |
| --- | --- |
| `FightMatchHostView` Captions/PlayerText | `fm.profile.*`、`fm.common.*`、`fm.exit_app.*`、`fm.language.*`、`fm.font_licenses.*`、`fm.diagnostic.*` |
| `PlayerNavigationView` | `fm.map.*`、`fm.party.*`、`fm.inventory.*`、`fm.crafting.*`、`fm.entry.*`、`fm.common.*` |
| `PlayerPermanentDetailView` | `fm.inventory.item.*`、`fm.inventory.quantity.*`、`fm.inventory.preference.*`、`fm.common.*` |
| `PlayerNavigationRecoveryView` | `fm.save_recovery.*` |
| `CandidateBoardInputView` | `fm.battle.hud.*`、`fm.battle.member.*`、`fm.battle.gesture.*`、`fm.history.*`、`fm.battle.restart.*`、`fm.battle.exit.*` |
| `CandidateBattlePlaybackView` | `fm.battle.playback.*`、`fm.victory.playback.*` |
| `PlayerDefaultReferenceView` | `fm.reference.*` |
| `PlayerBattleView` | 上述 battle/history/recovery＋`fm.victory.*`、`fm.result.*` |
| Host 条件页占位 | `fm.initial_download.*`、`fm.mobile_data.*`、`fm.update_ready.*`、`fm.guest_local_only.*`；首 L1 非自然链项不得伪造入口。 |

新增 45 键只关闭当前 L1 动态显示缺口，不增加业务行为或内容：稳定名称覆盖
角色、敌人、物品与关卡；phase/intent 与 operation 通过显式 key 映射；
playback/stage/history 使用玩家可读模板；reference 覆盖条件、差异和步骤；save
recovery 显示待处理行动与候选序号。所有名称和状态必须先由稳定值显式映射到
COPY key；禁止以 enum、ID、`OperationId`、`CommitId`、seed、revision、`FaceId`
或 `PairId` 的 `ToString()`／字符串拼接／硬编码回退生成玩家文案。

reference 差异标题仍且仅为 `fm.reference.differences.title`；四个差异行精确为
单数 `fm.reference.difference.party`、`fm.reference.difference.level`、
`fm.reference.difference.random`、`fm.reference.difference.state`。party 参数仅
`currentParty;referenceParty`，level 参数仅
`characterName;currentLevel;referenceLevel`，random/state 无参数；不得引用不存在
的 plural difference rows 或重新引入表中已删除／未声明参数。
`fm.history.snapshot.routes` 只接收 `lockedRouteCount`；成员／敌人生命与阶段分别
复用现有 HUD 模板和明确 phase 映射。`fm.save_recovery.operation_summary` 固定为
“待处理行动／Pending action”语义且只接收 `operationName`；`OperationId` 只作
内部绑定，不进入显示文本。

当前 216 键中首包／当前自然 L1 **不可达但必须保留为条件合同**的准确组仍是
`fm.initial_download.*`、`fm.mobile_data.*`、`fm.update_ready.*`、
`fm.guest_local_only.*`，以及依赖未发布内容的 `fm.first_clear_reward.*`。
这些原有条件组的语义与自然不可达性不因总数变化而改变，不得因不可达而删除。
`fm.language.save_failed` 已存在，参数精确为 `errorCode`、severity=`error`；偏好
保存失败时 popup 和本次 session 已选语言继续生效，不显示
`fm.language.saved`，并保留稳定诊断。其重试只重新保存语言偏好，不得借用业务
存档恢复入口或业务保存失败文案。

`severity` 是**已成功解析的业务文案语气／视觉强调元数据**，不是运行时
全页故障门：`info/warning/error/blocking` 只选择既定 tone/style；不得仅因
某条有效业务文案是 `error` 或 `blocking` 就打开 SystemLayer、冻结页面或
改写 Controller 可用性。动作门仍只来自现有 Controller／Application 状态。
尤其 active battle 必须仍可 Continue，CommitUnknown 必须仍开放既有恢复，
SaveFailed 必须仍开放既有 Retry；文本层不得盖掉这些入口。

只有 **binding/infrastructure failure** 才进入 BlockingDiagnostic 并阻断受影
响页面验收：缺 key、缺／多／重复参数、模板解析失败、未知 locale、重复键、
TMP/font/material 缺失或缺 glyph。失败结果没有可消费的业务 severity；统一
携带稳定 diagnostic code，由 Host 打开诊断 gate。不得回退另一语言、不得
把业务 `severity=blocking` 伪装成 binding failure，也不得用成品硬编码救场。

数字只用 invariant 数据值交给模板，locale service 决定显示格式；首版整数
XP/HP/数量不加分组，小数若出现必须由 key 指定精度。物品、角色、关卡、职业
显示名由稳定 ID→文本 key 显式表映射，禁止枚举 `ToString()` 和字符串拼接。

### 22.3 语言设置承载与偏好故障

`LanguagePopup` 是共享设置弹层，不扩做 Settings 全页。Startup、Navigation、
Result 顶栏及 Battle 非拖线时均有 `fm.action.language.open`；打开后用
`fm.language.entry.label` 作标题、两个 option key 作按钮、common close 关闭。
切换立即重绑当前活树，既有 controller/page/playback/request token 原样保留。

`LOC-TOOL-01` 新增 `ILocalePreferenceStore` 的 Host 文件实现，固定路径为
`Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json`；
格式仅 `{ "version": 1, "locale": "en|zh-Hans" }`。它不是 PlayerSave，
不随战斗回退、资料切换或云档覆盖。missing→系统语言；合法值→手选；损坏／
未知版本→保留原文件、记诊断、仅本次回退系统语言；读 I/O 失败同样回退且
不覆盖；写入使用同目录临时文件、Flush 后原子替换，失败删除本次临时文件、
保持旧持久值。切换可在内存生效，但只有随后重新读取验证相同值时显示
`fm.language.saved`；失败显示已接收的 `fm.language.save_failed`，继续使用本次
session 已选语言；重试只保存语言偏好。

以上整段文件持久化合同及验收**全部属于后续 `LOC-TOOL-01`**：UGUI-01
不得新增 `ILocalePreferenceStore`、不得读写 `persistentDataPath`、不得用 fake
文件系统测试损坏文件、I/O、原子替换或复读成功，也不得以这些绿测冒充语言
偏好已经跨重启实现。UGUI-01 的 `LocalePolicyTests` 只覆盖当前
`LocalizationService` 的系统语言规范化、session 内选择与事件传播，准确六槽
见 §25.1。

### 22.4 字体

首包新 TMP FontAsset 引用现有 OTF GUID
`960a9cda7a6b34935b8c18586488d314`，采用 Dynamic、Multi Atlas Enabled，
atlas population 在 bootstrap 本地可用；具体 atlas 1024×1024、padding 9、
SDFAA 由 C 生成后复开记录。其材质必须使用 §18.2 的
`TextMeshPro/Distance Field` shader；该 shader 的官方
`TextMeshPro/Mobile/Distance Field` fallback 不得删除。`TMP Settings`、两份
CJK 换行表、两个 shader 和它们的直接 includes 必须随 bootstrap 本地可用。
所有 TMP 显式绑定该资产，不用 legacy Text。
OTF/OFL 原字节与 GUID 保留，许可正文直接读取 OFL（不是翻译 key），标题／
入口用 `fm.font_licenses.*`。英简中 216-key 字符静态扫描、`HasCharacters`、
540×960/1080×2400 实际遍历和 Android 字形／内存证据缺一不可；首包只关闭
资源引用，不关闭设备视觉门。

## 23. Luban、YooAsset 与资源 seam 的受保护冻结

### 23.1 `LOC-TOOL-01` 版本门

当前仓库无 Luban，故本设计**不锁版本**。主程须先签只允许官方 release/tag
或 commit 的研究包，记录下载 URL、SHA-256、许可证、Mac Intel／Windows
同版本命令、Unity 2022.3.18f1 样片、确定性输出和是否需要运行时库。浮动
latest 或“本机能跑”不足以接收。版本接受后才能冻结：

```text
Config/FightMatch/Localization/FightMatchText.csv
Config/FightMatch/Localization/<exact schema files>
Tools/Luban/<pinned runner/config/license>
Assets/StreamingAssets/FightMatch/Localization/<generated bootstrap artifact>
Assets/Scripts/FightMatch/Presentation/Localization/<generated adapter only if required>
```

生成物必须由同一 CSV 单向生成、跨 Mac/Windows 字节或规范化语义一致，
缺翻译、重复键、参数集合差异、非法 severity 立即失败。Luban 类型不得进入
Core/Application/Presentation public contract；生成物仍需现有审查／不可变
发布门。以上尖括号不是 C 白名单，须由版本包替换为准确路径后再签发。

### 23.2 `RES-01` seam

当前仓库无 YooAsset；历史 `3.0.6` 仅研究样本，不在本文批准。版本包须以与
Unity 2022.3.18f1、Android IL2CPP/ARM64、Mac Intel、Windows 的最小样片
接受具体 tag/commit 后，才可改 `Packages/manifest.json` 和 lock。

项目合同冻结为 `IFightMatchAssetProvider.AcquireAsync(assetId, releaseSetId)`
返回项目自有 lease；lease 在最后一个 Host/page 使用者释放后才释放第三方
handle。YooAsset 类型只出现在独立 Adapter 实现，不进入 Core/Application/
Save/Presentation controller。六件现有业务字节取得后仍走
`PublishedContentCatalog` 校验；下载成功不等于 ContentBinding 或业务保存。

首 Demo bootstrap 本地包含 RuntimeRoot、EventSystem 能力、TMP 字体、英简中
下载／错误／重试／移动数据最小表；完整资源属于同一 ReleaseSet。R2 URL、
CDN、hash、缓存、Range、重开续传、Wi-Fi→移动数据中止和本次确认必须由
选定版本源码＋设备实证；`PauseDownload` 名称不是证据。Prepared 与下次重启
启用分开，旧集合保留到 BusinessReady；本轮不做 HybridCLR 或发布后台。

## 24. 序列化、旧资源、API 24 与 APK 结构包

### 24.1 Unity GUID／序列化

- Scene GUID `1d5124e5b55fe409d8216e78a117dec0`、Host script GUID
  `734984ebcddce44fab84ea4b5265f314` 及所有修改脚本 GUID 原样保留。
- 项目自有新脚本／Prefab／Noto TMP asset 由唯一 C、唯一 Unity 实例自然生成
  `.meta`；立即记录 GUID/path/type/SHA 并全 Assets 查重。§18.2 列出的官方
  TMP runtime resources 则必须从当前 3.0.6 Essential unitypackage 连同官方
  meta 选择性复制并保留表中 GUID；这是唯一允许的固定 GUID 来源。不得复制
  仓内旧 GUID、手造 GUID 或改变官方 GUID 对应类型。
- 保存顺序：导入 §18.2 精确 TMP runtime resources→生成 Noto TMP→改绑并
  保存 `TMP Settings`→保存 Root Prefab→实例化并保存场景→SaveAssets→关闭
  项目→重开→逐引用核对。复开必须核对 Settings/font/material、标准 SDF
  shader→Mobile fallback、两个 `Shader.Find`、直接 includes、line-breaking、
  Prefab 与场景，且无未解析 shader dependency；失败保留日志并停，不重建
  GUID 或删改官方 fallback 掩盖断链。

### 24.2 `ANDROID-API24-01`

准确修改候选只有：

```text
ProjectSettings/ProjectSettings.asset
Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
```

把 `AndroidMinSdkVersion: 22` 改为 24，并把构建前断言同步到 24；其它
ProjectSettings 字节必须相同。一次保存复开、一次 compile、一次 QA＋正常
构建报告均记录 Min 24。未执行时整合候选只能叫技术验证候选，不能叫首发／
Google Play 发行就绪。

### 24.3 `APK-STRUCT-01`

冻结的旧 `Tools/Invoke-FM029Validation.ps1` 及 029 证据不修改。新包候选路径：

```text
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs
Assets/Scripts/FightMatch/Host/Editor/FightMatchApkAssemblyManifest.cs.meta
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchApkAssemblyManifestTests.cs.meta
Tools/Test-FightMatchApkAssemblies.ps1
```

Unity Editor 侧从该次 Player build 的实际脚本程序集／类型结构生成带
source/APK SHA 的 manifest；外部脚本只解析 manifest 与 APK identity，不扫
`global-metadata.dat` 原始字符串。QA 包正对照必须结构化包含测试程序集和
指定 probe type；正常包负对照必须不含 `FightMatch.Core.Tests`、
`FightMatch.Host.Tests`、`FightMatch.Android.Tests`、NUnit/TestRunner 类型。
同一验证器对 QA/normal 各运行一次；QA 假阴性或正常假阳性均失败。不得以
字符串白名单、忽略命中或删除旧失败记录“修复”。实际 manifest API 可行性
须由该包的最小 Unity 样片先证；若 Unity 无法提供与最终包绑定的结构清单，
包返回 `BLOCKED`，不得退回原始字符串扫描。

### 24.4 旧资源处置

首包保留以下实物及 meta，但新场景／Prefab／Host/build-prepare 引用必须为零：

```text
Assets/UI/FightMatch/FightMatchPanelSettings.asset
Assets/UI/FightMatch/FightMatchTheme.tss
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.asset
```

`UGUI-CLEAN-01` 只有在全 Assets GUID 引用零、场景/Prefab/TMP 保存复开、
compile、207 目标测试、正常 APK 结构、R code `ACCEPT` 和可恢复快照齐备后，
才连同各自 `.meta` 删除。OTF、OFL 和所有 Editor UI Toolkit 资产永不随此包删。

## 25. 验证映射、预算、回滚与首包接收

### 25.1 跨平台 Linux/Mac EditMode 目标集

准确 `-testFilter` 为以下分号串（换行仅为文档可读性，执行时单参数）：

```text
FightMatch.Core.Tests.CandidateBoardInputTests;
FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests;
FightMatch.Core.Tests.CandidateBattlePlaybackTests;
FightMatch.Core.Tests.PlayerBattlePresentationTests;
FightMatch.Core.Tests.PlayerDefaultReferenceTests;
FightMatch.Core.Tests.PlayerBattleFlowTests;
FightMatch.Core.Tests.PlayerNavigationPresentationTests;
FightMatch.Core.Tests.PlayerNavigationSessionTests;
FightMatch.Core.Tests.PlayerNavigationPermanentFlowTests;
FightMatch.Core.Tests.PlayerNavigationRecoveryTests;
FightMatch.Host.Tests;
FightMatch.Core.Tests.UguiSceneCompositionTests;
FightMatch.Core.Tests.LocalizationContractTests;
FightMatch.Core.Tests.LocalizedTextBindingTests;
FightMatch.Core.Tests.LocalePolicyTests
```

旧 XML 证明前 10 fixture 为 153 cases、Host assembly 为 30 cases。四个新增
fixture 冻结为各 6 cases，因此首包目标总数必须精确为 **207**、failed 0、
skipped 0、inconclusive 0；若参数化展开改变，先回系统设计而不是放宽计数。
这 207 项覆盖真 EventSystem pointer/raycast、Controller 旧合同、三次重绑、
Host 路由／生命周期、场景唯一 Canvas/EventSystem、ID/placeholder、locale
规范化与缺键／参数诊断。不能用直接调用 Controller 替代其中的 uGUI 事件项。

新增 24 cases 的名称可由 C 按现有命名风格落地，但每个 fixture 的六个行为
槽位和单-case 展开冻结如下；表内“矩阵”必须在一个 test body 内循环并逐项
断言，不得被 NUnit 参数化展开成额外 case：

| fixture（6 each） | 冻结的六个可观察行为 |
| --- | --- |
| `UguiSceneCompositionTests` | (1) 通过 Core.Tests→Host 引用和 Host friend 直接实例化真实 Host 装配，断言单 Host/Canvas/EventSystem、组件是 `FightMatchStandaloneInputModule`、旧 runtime UI Toolkit 引用零、Prefab ID/placeholder 完整；(2) owner `TouchPhase.Ended` 经真 EventSystem Up＋EndDrag 仍只 commit 一次；(3) owner `TouchPhase.Canceled` 先命中 input-module normalization、后续 Up/EndDrag 零 commit；(4) 第二 finger Canceled 不清 owner，owner 后续 Ended 仍精确一次 commit；(5) 四角格心＋左/下含、右/上半开 outside 的局部坐标矩阵；(6) 540×960、1080×2400 与非零四边 inset 下相同格命中且绘制格心与命中公式一致。 |
| `LocalizationContractTests` | (1) 从固定制作稿生成的 `en/zh-Hans` 键集合一致且各精确 216 key；(2) 每键具名参数集合与模板一致；(3) key 前缀/唯一性/四值 business severity 合法；(4) 条件不可达组仍保留；(5) 系统中文变体→`ZhHans`、其它→`En`；(6) 两个测试程序集分别经 Presentation 三-friend 与 Host 两-friend 载体直接编译访问 internal seam，静态扫描无 public 扩张、反射、`SendMessage`、测试镜像或额外 friend。 |
| `LocalizedTextBindingTests` | (1) 通过真实 Host Bind seam 断言 Prefab 可见 TMP 初值全为诊断占位；(2) 成功 bind 在首次激活前替换且动态节点不闪硬编码；(3) locale change 同步刷新当前／重建页而不改业务 token；(4) 直接观察真实 diagnostic gate，缺 key/参数/模板/font/glyph 的 binding failure 打开该 gate；(5) 有效 `info/warning/error/blocking` 只改变 tone，不开全页 gate，且 active battle Continue、CommitUnknown recovery、SaveFailed Retry 仍服从现有 controller 可用性；(6) 三次 Bind/Unbind 无重复 listener 或旧 generation 回调。 |
| `LocalePolicyTests` | (1) 注入 `ChineseSimplified` 规范为 `ZhHans`；(2) Unity 2022.3 的 legacy/general `Chinese` 与 `ChineseTraditional` 矩阵也统一规范为 `ZhHans`；(3) `English/Japanese/Unknown` 等非中文矩阵规范为 `En`；(4) `LocalizationService` 以注入并规范后的 system locale 初始化 session 内 `CurrentLocale`，且不要求任何 store；(5) `En↔ZhHans` 实际变化先更新 `CurrentLocale` 再精确发一次 `LocaleChanged`，重复选择当前值不发事件；(6) 两个活跃订阅者各收一次同一变化，退订者不收后续变化，新绑定者直接读取当前 session locale，不重放旧事件。 |

FIX-03 只使上述既定行为可编译；四个 fixture 仍各 6、总目标仍为 207，filter、
行为槽和断言数均不变。不得把 friend/asmdef 静态存在当作 Host 行为通过；
`UguiSceneCompositionTests` 和 `LocalizedTextBindingTests` 必须走真实 Host 装配、
internal Bind 与 diagnostic gate，禁止反射、`SendMessage`、字符串 `GetType` 或
测试镜像替代。

FIX-04 也不改变任何 fullname、fixture、filter 或计数。它只一对一替换两个现有
case 内的失败占位：B16B04 与 P27_03 的 `blur` 分支各装配真实
`FightMatchPlayerHost + FightMatchHostView/root`，直接调用同一个 production
`HandleApplicationFocus`，并观察 §20.4 的现有取消／播放语义。不得保留
`WAITING_FOR_FOCUS_TEST_CONTRACT`，不得新增参数化 case，也不得用 reflection、
`SendMessage`、test mirror、手动 `Update`、disable-equivalent 或 public/test-only
hook 代替。`focused == false` 与 `focused == true` 的断言都留在各自原 test body
内，所以总目标仍精确为 207。

前三个触摸 case 必须走场景同型 EventSystem、GraphicRaycaster 和 input module；
owner cancel case 还须记录顺序为 `CancelTrueTouch` 在
`PointerUp/EndDrag` 之前。坐标两个 case 必须直接比较 §21 同一 mapper 和
`OnPopulateMesh` 格心源，不得各自复制一套期望翻转公式。现有 183 cases 不增
不减；若需更新旧输入断言，只能一对一替换而不能改变总 fullname 数。
`LocalePolicyTests` 六槽不得创建 fake `ILocalePreferenceStore` 或临时偏好文件；
文件持久化、坏文件、未知版本、I/O、原子写与成功复读断言留给
`LOC-TOOL-01` 的独立白名单和计数。

Linux 允许差异仅为路径分隔符、字体 raster 像素和系统语言原始枚举；测试
必须注入 locale/安全区，不读取执行机真实偏好。Linux 不运行 Android build、
APK、ADB、触摸设备或最终字形视觉，不因缺 AndroidPlayer 在运行时 skip；
Android 专用 Editor/工具代码不得进入该 207 集的执行前置。云端仍在旧
`9d416e6` 时不得运行，须先有精确候选 SHA／干净度／Unity 版本回执。

结果路径：本机
`TestArtifacts/FightMatch/UGUI-01/<candidate-sha>/editmode-target/`，云端
`/workspace/TestArtifacts/FightMatch/UGUI-01/<candidate-sha>/editmode-target/`；
必须含 `tests.xml`、`unity.log`、`run.json`、源 SHA 与 XML 根统计。单次目标
≤10 分钟；超时安全收尾并报环境／性能事实，不自动改 filter。

### 25.2 分阶段预算

| 门 | 次数与上限 | 接收事实 |
| --- | --- | --- |
| Q0 R 设计 | r3/FIX-04 已获独立 R `ACCEPT`；COPY-IDENTITY-SYNC-05 只同步另行独立接收的固定输入 | 本同步不改变设计，不要求重审整篇；保留既有 FIX-04 R identity。 |
| Q1 静态 | 每候选 1 次，≤10m | 既有白名单、10 个 §18.2 官方路径项/GUID 唯一、Mobile asset/meta SHA 精确；PresentationAssemblyInfo 精确三 friend 不变、HostAssemblyInfo 精确两 friend、无第三 Host friend／第四 Presentation friend；Core.Tests 仅有已冻结 Host 引用、Host.Tests asmdef 原字节；`FightMatchPlayerHost.OnApplicationFocus(bool)` 保持 private 且为无分支单行单次 `HandleApplicationFocus(focused)` 委托，handler 仅 internal、false 分支精确一次 `runtimeRoot?.PausePresentation()`、true 分支零动作；无反射、`SendMessage`、test mirror、manual Update、disable-equivalent、public/test-only hook，且 pause/lowMemory 未新增 handler；旧资源零新引用、Prefab placeholder、无 Packages/ProjectSettings 差异。 |
| Q2 资源复开 | 1 次有效运行＋最多 1 次定向修正，≤5m/次 | 单 Canvas/EventSystem、Root Prefab/TMP/scene、TMP Settings/Noto、标准 SDF→Mobile fallback、两个 Shader.Find、直接 includes/换行表引用有效且无未解析 shader dependency。 |
| Q3 Compile | 冻结候选 1 次，≤5m；源码再变补 1 次 | Unity 2022.3.18f1 clean compile。 |
| Q4 207 target | 每候选 1 次，≤10m；失败只跑失败 fixture，最终再跑 207 | 精确计数与零失败；B16B04/P27_03 均以真实 Host+root 直接调用 internal handler，false 分别证明取消与播放终态语义，true 证明零动作；无等待占位、skip 或 inconclusive。 |
| Q5 全量 | Q1～Q4、代码稳定后最多 1 次，≤15m | 对比已接收 4181 fullname Counter＋24 新 cases，预期 4205；删除/改名须逐项解释。 |
| Q6 Android | 后续整合候选 QA/normal 各 1 次，≤10m/包 | API24、结构验证、APK identity；不复用 018/019。 |
| Q7 设备 | BlueStacks 1 次≤45m；iQOO 1 次≤75m | ADB事实、双语、触摸/多指/安全区/冷启动，并以真实系统切前后台／失焦证明 OS→Unity `OnApplicationFocus` 投递会进入同一 handler；设备缺席为 DEVICE_PENDING。 |

### 25.3 需求到证据

- B16→`CandidateBoardInputTests`＋`UguiSceneCompositionTests` 的 Ended/owner
  Canceled/second-finger Canceled、四角半开与双分辨率 inset 矩阵＋设备触摸；
  P27→两个 playback fixture＋pause/focus/lowMemory；B09～B12→Battle/
  Reference fixtures；CC→Navigation fixtures；H01～H05→Host assembly。
- FIX-04 实施／接收一一映射：`FightMatchPlayerHost.cs` 只承载 internal handler
  与 private Unity message 的单次委托；`CandidateBoardInputTests.cs` 的现有 B16B04
  `blur` 分支证明真实 root 的 pointer cancel／零提交；
  `CandidateBattlePlaybackPanelTests.cs` 的现有 P27_03 `blur` 分支证明真实 root 的
  latest-head／own-token 收口且旧 A1 不释放 A2。Q1 只证明调用结构和不存在禁用
  绕路，Q4 只证明直接调用 handler 后的生产行为，Q7 才证明设备上的真实
  OS→Unity focus message 投递；三者不可互相替代，也不宣称 EditMode 合成了 OS
  focus 事件。
- 本地化→四个新增 fixture、business severity 非 gate／binding failure gate、
  216-key 生成校验、Prefab 静态扫描、双语页面遍历；字体→源/OFL hash、
  TMP Settings/标准 SDF→Mobile fallback/direct includes 复开、两个 Shader.Find、
  `HasCharacters`、Android 截图。语言偏好文件持久化及其故障验收仅归
  `LOC-TOOL-01`，不计入本包 207。
- PQA-01～15 正常链最终只以新正常 APK 自然入口验收；当前 L1 不可达项保持
  `NOT_VERIFIED`。旧 4181、029 APK 只证明旧候选，不替代新 Canvas/设备。

### 25.4 失败与回滚

- 超白名单、依赖/设置偷改、重复 GUID、第二 EventSystem、Prefab 成品文案、
  运行时 UI Toolkit 引用、Core/Application API 或 PlayerSave 变化：立即停包。
- FIX-03 回滚精确删除本包新建的 `HostAssemblyInfo.cs/.meta`，并只从
  `FightMatch.Core.Tests.asmdef` 撤去新增的 `FightMatch.Host` 引用；
  `PresentationAssemblyInfo.cs/.meta`、`FightMatch.Host.Tests.asmdef` 和产品
  asmdef 不随该回滚改变。若移除后出现依赖它的测试源码，候选整体回到
  FIX-03 前快照，不保留反射／public 临时绕路。
- FIX-04 回滚只撤去 `FightMatchPlayerHost` 的 internal focus handler 和 private
  message 委托，并把 B16B04/P27_03 两个 `blur` 分支整体恢复到 FIX-04 前快照；
  不改 FIX-03 已接收的 friend/asmdef，不增删测试，不保留 reflection、
  `SendMessage`、test mirror、manual Update、disable-equivalent 或 public 临时入口。
- 资源保存失败恢复本包前 scene/prefab/script 快照和相同旧 GUID；不得清
  玩家资料、删旧证据或重建场景 GUID。
- EventSystem 失败先在最小 fixture 定位 raycast／坐标／pointer ownership；
  本地化失败保持诊断可见；字体失败保留原 OTF/OFL；业务／保存失败交回原
  Owner，不改断言、OperationId 或请求字节。
- Luban/YooAsset/API24/APK 工具包失败不回滚已独立接收的 UGUI-01，但整合
  状态保持未完成；不得改用 Unity Localization、Addressables、自研临时下载
  或 Min SDK 22 冒充发布。

### 25.5 首包验收

`UGUI-01` 的可观察验收为：指定白名单内完成同名 uGUI Binder 和单 Root
Prefab；场景只有一个 Host/Canvas/EventSystem；真 EventSystem 事件保持
旧业务语义，Ended 单提交、owner Canceled 零提交、第二指 Canceled 不干扰；
绘制与命中共用无 Y 翻转公式；三次重绑无旧回调；
两个测试程序集经冻结 friend 直接编译访问 internal seam，Core.Tests 的真实
Host 装配／Bind／diagnostic gate 行为通过且没有 public、反射或测试镜像绕路；
private `OnApplicationFocus` 单次委托 production internal handler，B16B04/P27_03
以真实 Host+root 对 false/true 两路证明取消、own-token 播放收口与零动作，且
Q7 独立证明 OS→Unity 投递；pause/lowMemory 不扩 seam，destroy 精确收口；
所有玩家可见 TMP Prefab 初值为诊断占位，TMP Settings/Noto/SDF/换行资源复开
有效；内存 source 能证明两语言即时
刷新且不动业务 token，session 内 locale 规范化／选择／事件传播通过而不宣称
跨重启持久化；标准 SDF 与 Mobile fallback 均复开命中且无未解析依赖；有效
business severity 不盖住 controller 入口且只有 binding failure 开诊断 gate；
旧 UI 资源零运行时引用但仍可回滚；Q1～Q4
通过，R 对实际 patch 给唯一 `ACCEPT`。这只接收结构迁移，不接收正式运行时
COPY/Luban 表、YooAsset、API24、APK 或设备门。

## 26. r4 / COPY-IDENTITY-SYNC-05 影响声明与自检

- 架构第 6 节 13 项已分别覆盖：路径（§18）、程序集（§19）、层级（§20）、
  绑定（§20.3）、棋盘（§21）、本地化（§22）、Locale store（§22.3）、Luban
  （§23.1）、YooAsset（§23.2）、字体（§22.4）、旧资源（§24.4）、验证（§25）、
  public/序列化影响（§19/§24.1）。
- 主策 COPY 输入与 hash 已绑定；主美只提供布局接口，中性灰盒没有采用任何
  被拒或 pending 美术。主测试 Linux/Android 分组、预算、API24 和 019 门已
  回填。
- 本设计没有把未安装组件写成已接入，没有修改产品源码／资源／meta／设置／
  依赖／Git，没有运行 Unity 或重复旧长测试。
- FIX-03 相对已获 R `ACCEPT` 的 SHA
  `6f7fb2ba94112bd2072602297e16ce77895cf94951edcb59f83a4800402395dd`
  只关闭 Host internal seam 的编译可见性：新增两个白名单路径、冻结一个现有
  asmdef 的单引用差异，并补齐对应静态门／回滚；该已接收决定在 FIX-04 中不变。
- FIX-04 相对输入 SHA
  `887dfae28158271c2dc0387ed642e14e5d2fc52aa965b3c206da6cf5abac4263`
  只冻结一个 production focus handler、private Unity message 的单次委托、两个
  现有 blur 分支的真实 Host+root 调用和 Q1/Q4/Q7 证据边界；其 r3 输出 SHA
  `1738be494e82fc6b8a2235b70d269e61f051e91fab3bb12ce009340ea6823d0b`
  已获独立 R `ACCEPT`。COPY-IDENTITY-SYNC-05 不改变该设计或既有 R identity。
- 本轮没有把 pause/lowMemory 扩成新 seam，没有接受 reflection、`SendMessage`、
  test mirror、manual Update、disable-equivalent、public/test-only hook 或 EditMode
  冒充 OS focus 投递。Q1/Q4/Q7 分别承担结构、handler 行为、真实设备投递证据。
- COPY-IDENTITY-SYNC-05 只把已独立接收的 COPY FIX-01 身份、216-key 计数和动态
  映射同步进本文；COPY R turn/final 保持
  `01a0f395-145a-79e3-ab3f-cbe5bdc90291` /
  `msg_0d29bf8d726d8dc0016abd55c4739487d0a77167e28bcf1f79`。主程当前 implementation
  SHA 为 `fd03adb59e1775be88ece565ab32f39a42514158cc689de235f21ece8133aafc`。
- 本同步不改变接口、行为、§18 白名单、friends、asmdef、预算、filter、fixture
  或精确 207 测试总数；仍为 36 个既有文件＋49 个新增文件＝85 个路径，Host 两
  friend、Presentation 三 friend、Core.Tests 单 Host 引用和 Host.Tests 原字节
  不变。它不需要重新评审整篇，也不声称正式 Luban／运行时表、C 实现、Unity
  compile、测试、API24、APK、设备或发行已通过。

**SYS-ENG-001 结论：READY_FOR_IMPLEMENTATION_INPUT_SYNCED。** FIX-04 设计和
COPY FIX-01 输入均保留各自独立接收身份；本文现在只完成二者的固定输入同步，
不是正式运行时本地化表、Demo 或发行完成声明。
