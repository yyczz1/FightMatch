# FightMatch uGUI、本地化与资源接入最小架构裁决

2026-09-30 · `ARCH-ENG-001` · 状态：**READY_FOR_SYSTEM_DESIGN**  
交付负责人：独立架构负责人 `01a0f2e6-1ac3-7d10-8855-54192b9489d4` / `local`  
上级／回传对象：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`

本报告只固定依赖方向、状态所有权和接入缝，不是产品实施包，也不
声明 Luban、YooAsset、双语资源或 uGUI Prefab 已经存在。没有发现需要
改变用户既定路线的事项；本包不标 `BLOCKED`。精确类、字段、文件白
名单、GUID／`.meta`、工具版本、生成命令及测试预算留给系统技术设计。

## 1. 固定输入与已核事实

### 1.1 输入身份与权限

- 固定源码起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；实际只读
  `git rev-parse HEAD` 与该值一致。
- 任务与授权：根 `AGENTS.md` 第 6 节、`.agent/TEAM_WORKFLOW.md`、
  `docs/team/2026-09-30/README.md`、`dispatch-001.md` 和主程线
  `engineering.md`。用户已恢复本轮首 Demo 接续，并批准团队内限定派发
  与正式回执。
- 架构依据：`docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md`、
  `SYSTEM_DESIGN_CONTEXT.md`、现行系统目录／共同交互契约／运行时扩展／
  跨系统流程，以及
  `docs/system-design/2026-09-17/runtime-ui-ugui-correction.md`。
- 本包唯一写入是本文件；没有运行 Unity，没有写产品代码、场景、资源、
  `.meta`、Packages、ProjectSettings 或 Git。
- 工作区在本包开始前已有中央／主程负责的共享文档修改及 `docs/team/`
  新文件；本报告不接管、清理或评价这些并行成果。

### 1.2 已定且不可重选的产品／技术路线

- 玩家运行时 UI 使用 uGUI；UI Toolkit 只保留给编辑器工具。
- 首发文本为英语和简体中文。首次启动：中文系统语言选简体中文，其余
  选英语；玩家可手动切换并记住选择。
- 玩家可见 Prefab 文本默认是显眼诊断占位符，例如
  `【if you see this, it is a bug.】`，运行时绑定必须替换它。
- Excel／CSV 是文本和配置的主要编辑权威；Luban 是制作期生成工具；
  项目拥有运行时本地化绑定。不得用 Unity Localization 取代。
- 运行时资源使用 YooAsset，经 R2 Standard＋Cloudflare 缓存／CDN取得；
  不引入 Addressables。
- 新决定覆盖旧 `configuration.md` 中“JSON 草稿是唯一编辑源”的建议；
  JSON／二进制可以继续作为生成或发布产物，但不能再由人独立编辑成
  第二份权威。`runtime-ui-ugui-correction.md` 末尾曾保留的“官方
  Localization 对比”也已被后来的 Q19 决定覆盖。

### 1.3 当前仓库事实

- `Packages/manifest.json` 已有 `com.unity.ugui: 1.0.0`、
  `com.unity.textmeshpro: 3.0.6`；没有找到 Luban 或 YooAsset 包、生成物、
  运行时适配器或锁定版本。此事实不授权修改依赖。
- `FightMatch.Core`、`FightMatch.Input`、`FightMatch.Platform`、
  `FightMatch.Content`、`FightMatch.Application` 均保持无 Unity Engine 引用；
  `FightMatch.Presentation` 和 `FightMatch.Host` 是当前 Unity 视图／组合层。
- `FightMatch.Application` 已给出 `PlayerNavigationView`、`PlayerBattleView`
  等只读结果模型及 Session；现行 Presentation 控制器消费这些结果。
- 下列控制器不依赖 UI Toolkit 视图树，可直接作为 uGUI 接入基础：
  `CandidateBoardInputController`、`CandidateBattlePlaybackController`、
  `PlayerBattleController`。`PlayerNavigationController` 的流程本体同样可复用，
  但同文件的内部 `NavigationBindings` 直接依赖 UI Toolkit，须由系统设计
  拆出／替换。
- `FightMatchHostSession` 拥有启动、导航、战斗、退出、保存恢复及过期
  路由保护，不依赖 `UIDocument`；`FightMatchPlayerHost` 与
  `FightMatchHostView` 才是当前 UI Toolkit 组合根。
- `FightMatch.Input/RouteGesture` 和 `GestureModels` 已将指针样本与业务
  手势分离。当前 `CandidateBoardElement` 负责 UI Toolkit 指针捕获、屏幕
  到棋盘坐标映射和绘制；这些是视图 Adapter 职责，不是战斗规则。
- `IContentPublicationStorage`、`PublishedContentCatalog`、
  `FirstReleaseContentStorage` 已提供不可变字节、内容绑定、校验与发布准入；
  当前 `FightMatchStreamingAssetsLoader` 只是从 StreamingAssets 读取六份
  首发文件的 Host 实现，不是 YooAsset 集成。
- `ILocalSaveStorage`、`LocalSaveStore`、Player/Application/Core 中的保存
  封套、操作身份、CommitUnknown／SaveFailed 恢复和业务完成语义均已
  实现并有旧证据；UI／文本／资源迁移没有理由改写它们。
- 当前运行时 UI 是代码构造的 UI Toolkit，不是 uGUI Prefab：
  `CandidateBoardElement`、`CandidateBoardInputView`、
  `CandidateBattlePlaybackView`、`PlayerBattleView`、
  `PlayerDefaultReferenceView`、`PlayerNavigationView`、
  `PlayerNavigationRecoveryView`、`PlayerPermanentDetailView`、
  `FightMatchHostView`、`FightMatchPlayerHost`。
- 当前 `FightMatchDemo.unity` 只有 Host＋`UIDocument`，引用
  `FightMatchPanelSettings.asset`、`FightMatchTheme.tss`、现有 FontAsset 与
  OFL；Android 构建脚本也直接验证这些 UI Toolkit 对象。
- 当前玩家文案大量硬编码在 Presentation／Host；`FightMatchHostView` 的
  `Captions`／`PlayerText` 还是运行时字符串替换。这不是可扩展的双语
  绑定，也不能继续作为正式本地化实现。

## 2. 架构裁决

### AD-01：保持业务、应用与保存模块不动；只替换运行时视图 Adapter

现有 Core、Application、Content 准入、Platform 保存、HostSession、输入
手势和三组控制器继续作为权威 Module。uGUI 迁移位于 Presentation／Host
的 Unity 接入缝：uGUI 组件把 EventSystem 意图规范化后送入现有控制器，
再把 Application 已确认的只读结果渲染出来。

依赖方向固定为：

```text
uGUI View/Prefab ──意图──> Presentation Controller ──调用──> Application Session
      ^                         |                              |
      |                         └──已确认视图结果──────────────┘
      |
Host composition ──绑定页面、生命周期、本地化、Unity资源句柄

Application/Core/Save/Content truth  -X->  uGUI、EventSystem、YooAsset、Luban
```

UI 不得直接写业务 Model、重算战斗、发奖励、改库存或解释保存文件。视图
删除后，控制器的请求身份、epoch／页面令牌、保存恢复和播放完成语义仍然
存在；这使接入缝具有实际深度，而不是在 uGUI 上复制一套业务。

### AD-02：UI 框架隔离缝在 Presentation／Host，不在 Application

- Presentation 控制器与 `CandidateBattlePlaybackFrame` 保留为框架无关的
  Presenter／交互控制面；UI Toolkit 的 `VisualElement`、`Button`、
  `Label`、`schedule`、PointerCapture 只属于待替换实现。
- uGUI Adapter 负责 `IPointerDown/Move/Up` 等 EventSystem 事件、指针身份、
  捕获等效行为、取消／失焦、RectTransform 坐标换算、棋盘绘制、按钮绑定、
  文本字段读取与组件解绑。
- UI Toolkit `schedule` 的 100ms 刷新和 16ms 播放驱动改由 Unity 生命周期
  驱动现有 Controller 的 `Refresh`／`Advance`；驱动停止、异常、页面禁用、
  焦点丢失、低内存和销毁必须沿现有 Cancel／Skip／Detach 语义收口，不能
  让旧回调操作新页面或重复报告播放完成。
- Host 是唯一组合根：创建 Session／Controller，绑定 Canvas 页面，处理
  Android 返回键、安全区、pause/focus/low-memory、退出与资源句柄寿命。
  页面和 Prefab 不自行定位全局业务对象。
- 不新增输入包。系统设计先使用已安装的 uGUI EventSystem 能力；若实证
  不足，必须另提依赖变更，而不是在实施时静默加入 Input System。

### AD-03：本地化是项目拥有的显示 Module，不进入战斗／存档真值

本地化 Module 的小 Interface 只应向 UI 绑定提供“稳定文本键＋参数→当前
语言文本／诊断失败”和语言变更通知。它负责键查找、模板参数、格式化、
缺键／缺参数诊断及刷新；不翻译 Application 状态码来改变业务含义，也
不把显示文本送回业务层。

状态所有权固定如下：

| 状态 | 唯一负责者 | 约束 |
| --- | --- | --- |
| 文本键、英／简中模板、语境与参数声明 | 文本表制作／Content 发布链 | Excel／CSV 单一编辑权威；生成物不可手改。 |
| 当前有效语言 | 客户端本地化 Module | 首次按系统语言；之后读玩家明确选择。 |
| 手动语言偏好 | 独立客户端设置存储 | 不并入现有 PlayerSave 业务切片，不随战斗回退或选档改变。 |
| 系统语言观察 | Platform Adapter | 只用于没有手动偏好的首次选择。 |
| 页面文字与布局刷新 | uGUI 本地化 Binding | 语言变化后重取文本；不重建或重提业务操作。 |

现有 Application 的 `Code`、`Reason`、ID、枚举和诊断字段继续是稳定机器
语义。Presentation 用显式键映射；禁止以 `string.Replace`、枚举名直接
展示或散落 switch 充当本地化。动态句子使用表内模板和具名参数；物品、
关卡、职业等显示名从稳定业务 ID 映射到文本键。

Prefab 中所有玩家可见 Text／TMP 初值必须是诊断占位符。绑定成功才替换；
缺键、变量不匹配或表未加载时继续显示可辨认占位并记录诊断，不能回退到
另一处硬编码成品文案来掩盖问题。纯数值组件同样要在绑定后填入实际值。

语言偏好会产生一个新的、隔离的客户端设置持久状态，但不能改变现有
PlayerSave schema、SaveEnvelope、业务 OperationId 或 CommitId。具体使用
何种本地设置 Adapter、格式版本和故障行为由系统设计提出，并在实施包中
把“新增设置存储”作为显式序列化影响列出。

### AD-04：Luban 只跨制作／生成缝，运行时不泄漏其类型

目标链固定为：

```text
Excel/CSV（唯一人工权威）
  -> Luban 生成与schema/引用/多语言完整性检查
  -> 规范化生成快照（JSON/bytes/C#均可作为生成物）
  -> 既有内容验证、审查、不可变发布
  -> 运行时项目自有 Catalog / LocalizedTextSource
```

- Luban 不拥有玩家状态、ContentBinding、发布批准或运行时加载策略；它只
  生成候选数据及可定位诊断。
- 现有 `PublishedContentCompiler`／`PublishedContentCatalog` 的校验、内容
  指纹、不可变发布和消费者准入继续负责业务内容。Luban 输出先进入这条
  流程，不能绕过为“生成成功即可发布”。
- 对本地化文本，系统设计可以定义独立发布 artifact；翻译版本不应改写
  战斗规则的 `ContentBinding` 或玩家存档。它应被同一 `ReleaseSetId`／
  资源发布集合准确绑定，防止代码、Prefab 与文本键集合错配。
- 若选定 Luban 版本要求运行时库或生成 C# 类型，该依赖只能藏在 Content
  Adapter 后，不能让 Core/Application/Presentation 的接口暴露 Luban 类型。
- 启动／下载页所需的英／简中最小文本必须随固定启动层可用；它与远端
  完整文本表仍从同一 Excel／CSV 源生成。两个生成切片不是两套人工权威。

### AD-05：YooAsset 是资源取得 Adapter，不是业务内容或保存真值

YooAsset 位于 Host／运行时资源交付侧，R2＋Cloudflare 只是其远端传输源。
Core、Application、业务 Save 和 Presentation Controller 不引用 YooAsset
命名空间、Package、Handle 或下载器类型。

资源依赖方向固定为：

```text
R2/Cloudflare -> YooAsset Adapter -> 项目自有资源租约/结果 -> Host/Presentation
                                         |
                                         +-> 字节产物交现有内容准入与发布读取
```

- Prefab、Sprite、字体、动画等 Unity 对象通过项目拥有的资源接入缝交给
  Host／Presentation；YooAsset handle 的保留／释放由 Adapter 与页面寿命
  管理。对象仍在使用时不得提前释放，旧页面回调也不得释放新页面租约。
- 规则／配置／文本等字节即使由 YooAsset 运输，取得后仍须通过现有内容
  校验／发布准入；YooAsset manifest、缓存存在或下载成功都不能直接变成
  `ContentBinding`、业务可用或玩家保存完成。
- 固定启动层必须本地包含：下载／错误／重试／手机流量确认所需的最小
  uGUI、EventSystem、英简中文本与字体能力。它不能依赖尚未下载的远端
  Prefab 或文本表才能解释下载失败。
- H11 的 ReleaseSet 仍是启用单位：半包、旧代码＋新资源、新代码＋旧
  文本不得拼成可运行集合；下载完成与下次重启启用分开。
- 手机流量确认、Wi-Fi 中途切换、暂停／继续、断点续传、损坏恢复和缓存
  清理属于系统设计／设备验证。不得仅凭 YooAsset 宣传材料写成项目已具备。
- 本轮不批准完整 HybridCLR、账号、广告后台或远端发布实现；只为首 Demo
  所需 uGUI／文本／资源接入保留与 H11 兼容的缝。

### AD-06：资源、文本与玩家保存采用不同生命周期

| 对象 | 生命周期／身份 | 不允许的耦合 |
| --- | --- | --- |
| 业务定义 | `ContentBinding` / `DefinitionBinding` | UI语言或Sprite变更不能重算已有战斗。 |
| 代码／资源发布集合 | `ReleaseSetId` | 下载成功不能直接声明当前运行集合已切换。 |
| 本地化表 | ReleaseSet内的文本artifact身份 | 翻译修订不能写进玩家经验、库存或战斗历史。 |
| Unity资源实例 | 项目资源租约／页面或Session寿命 | YooAsset handle不能进入业务Model或存档。 |
| 玩家业务状态 | 现有SaveGeneration／CommitId | UI重建、语言切换、资源换图不能新建业务操作。 |
| 语言偏好 | 独立客户端设置 | 不随战斗撤销、游客选档或云进度覆盖。 |

## 3. 保留、替换与禁止清单

### 3.1 保留并复用

- `Assets/Scripts/FightMatch/Core/**` 的业务、绑定与保存编码。
- `Assets/Scripts/FightMatch/Application/**` 的 Session、请求／结果模型、
  页面令牌、过期上下文与保存恢复语义。
- `Assets/Scripts/FightMatch/Platform/**` 的玩家本地保存及
  `IContentPublicationStorage`；新增资源／设置 Adapter 不得削弱现有保证。
- `Assets/Scripts/FightMatch/Content/**` 的验证、指纹、不可变发布和
  `PublishedContentCatalog`。只在后续精确包中增加 Luban 生成输入适配。
- `FightMatch.Input` 的 Gesture Module；uGUI 仅替换 `PointerSample` 的来源。
- Presentation 的 `CandidateBoardInputController`、
  `CandidateBattlePlaybackController`、`PlayerBattleController`、
  `CandidateBattlePlaybackFrame`，以及剥离 `NavigationBindings` 后的
  `PlayerNavigationController`。
- `FightMatchHostSession` 的路由、生命周期和保存接续。
- 现有 Noto 字体原始 OTF 与 OFL 来源／许可证据；目标 uGUI/TMP FontAsset
  是否复用或重建须在系统设计和实际字形检查后决定。
- 旧测试与 APK 证据作为原 UI Toolkit 合同的历史证据；不删除、不改写。

### 3.2 必须替换或迁移

- 上述所有 `VisualElement` 视图、`CandidateBoardElement`、UI Toolkit 指针
  捕获／绘制、`NavigationBindings`、`FightMatchHostView`。
- `FightMatchPlayerHost` 的 `UIDocument`／PanelSettings 组合，改为 Canvas＋
  EventSystem＋uGUI 页面装配。
- `FightMatchDemo.unity` 的运行时 `UIDocument` 引用和对应场景装配。
- `FightMatchPanelSettings.asset`、`FightMatchTheme.tss` 及其构建脚本检查：
  只有所有运行时引用已替换、系统设计给出 GUID／回滚表后才能删除；不
  影响 Editor UI Toolkit 资产。
- 当前硬编码玩家文案、`Captions`、`PlayerText` 与动态字符串拼装，改为
  文本键、模板参数和项目自有 Binding。
- 直接依赖 UI Toolkit panel／事件的 EditMode／Host 测试，改为真实 uGUI
  EventSystem、Prefab／场景装配和生命周期测试；业务断言保留。
- `FightMatchAndroidBuild.PrepareResources` 中 UIDocument／PanelSettings／
  TSS 的构建前检查，改成对 uGUI 场景、Prefab、字体、本地化表及资源
  发布身份的准确检查。
- `FightMatchStreamingAssetsLoader` 的固定六文件启动接线需要由系统设计
  说明哪些仍为内置 bootstrap、哪些通过 YooAsset 获取；内容准入不取消。

### 3.3 明确禁止

- 禁止新建第二套战斗、导航、结算、保存或 Demo 专用业务实现。
- 禁止为迁 UI 改现有 PlayerSave／业务序列化、清档或迁移玩家资料。
- 禁止 Core／Application／Save 引用 Unity UI、Luban 或 YooAsset 类型。
- 禁止引入 Unity Localization、Addressables，或把 UI Toolkit 继续用于
  FightMatch 玩家运行时。
- 禁止把 JSON、Unity资产、代码字典和 Excel／CSV 变成多份可人工修改的
  文本／配置权威。
- 禁止 Prefab 预填成品文案、硬编码成品回退、字符串替换假本地化、
  直接展示内部状态码冒充玩家文案。
- 禁止因为 YooAsset 缓存有文件就跳过签名／哈希／版本／内容准入，或把
  下载完成当业务恢复完成。
- 禁止全仓删除 UI Toolkit／UIElements；FlowPuzzle 等 Editor 工具继续按
  原决定使用。
- 禁止假定 Luban／YooAsset 已安装、具体版本已选、R2 已配置、断点续传
  已验证或移动网络监测已完成。

## 4. 组件与依赖映射

| 当前组件 | 当前职责 | 目标 Module／Adapter | 架构处置 |
| --- | --- | --- | --- |
| `CandidateBoardElement` | UI Toolkit绘制、坐标换算、指针捕获 | uGUI棋盘Graphic＋EventSystem Adapter | 替换视图；继续产生同语义`PointerSample`。 |
| `CandidateBoardInputView` | 棋盘HUD、角色按钮、100ms刷新 | uGUI战斗输入Binder＋生命周期驱动 | 替换；Controller不变。 |
| `CandidateBattlePlaybackView` | 播放HUD、16ms schedule、跳过／低内存 | uGUI播放Binder＋Unity驱动 | 替换；继续调用现有Playback Controller。 |
| `PlayerDefaultReferenceView` | 默认参考覆盖与独立播放 | uGUI参考弹层／覆盖Binder | 替换；不能取得战斗提交权。 |
| `PlayerBattleView` | 战斗、回退、恢复、结算页面装配 | uGUI战斗页面Binder | 替换；Application `PlayerBattleView` 结果模型保留。 |
| 导航三View | 地图／队伍／背包／永久操作／保存恢复 | uGUI导航页面Binder | 替换；Session和请求身份保留。 |
| `FightMatchHostView` | 顶层路由、硬编码中文、布局 | uGUI Host shell＋本地化Binding | 替换；不保留`PlayerText`路线。 |
| `FightMatchPlayerHost` | UIDocument、启动加载、安全区／生命周期 | Canvas组合根＋bootstrap＋资源Adapter | 改接线；Session和保存根语义保留。 |
| `FightMatchHostSession` | 业务路由、退出、恢复、Controller寿命 | 同一Host Session Module | 保留；不得引用uGUI/YooAsset。 |
| `FightMatchStreamingAssetsLoader` | 内置六文件读取／首发Catalog | bootstrap内容读取＋YooAsset运输Adapter | 精确拆分留系统设计；Catalog准入保留。 |
| `PublishedContentCatalog` | 业务定义的不可变发布与准入 | 同一Content Module | 保留；Luban只提供生成输入。 |
| 本地化新增 | 当前不存在 | 项目自有Text Catalog＋Binder＋Locale Policy | 新增最小Module；不进入业务层。 |
| 资源新增 | 当前不存在YooAsset接入 | 项目资源租约接口＋YooAsset Adapter | 新增真实接入缝；第三方类型止于Adapter。 |
| 客户端设置新增 | 当前不存在语言偏好 | Locale Preference Store Adapter | 新增隔离状态；不改PlayerSave。 |

目标依赖只能向内：

```text
Host -> uGUI Presentation -> Presentation Controllers -> Application -> Core
Host -> Localization Binding -> Generated Text Catalog
Host -> Project Asset Lease <- YooAsset Adapter -> R2/Cloudflare
Generated/Luban Adapter -> Content validation/publication -> Core definitions
Host/Application -> Platform save interfaces -> OS storage adapters
```

`Content` 当前依赖 `Platform.IContentPublicationStorage` 的既有关系保留；新的
Unity对象资源租约不能塞进这个纯字节 Interface。系统设计可选择 Host 内
部实现或独立 Unity runtime assembly，但必须守住上述依赖和类型隔离。

## 5. 公开接口与序列化影响

### 必须保持为零的影响

- Core/Application 的业务接口、状态所有权、请求／结果、OperationId、
  CommitId、ContentBinding／DefinitionBinding。
- `ILocalSaveStorage`、SaveEnvelope、PlayerSave 格式、现有业务恢复与迁移。
- 战斗、永久操作、库存、进度、奖励和保存的既有语义。

### 允许由系统设计申报的最小新增

- Presentation／Host 内部的 uGUI Binder、页面引用、资源租约和本地化
  Binding Interface。它们应保持内部或最小可见性；不要为了每个控件
  建一层浅 Interface。
- 独立 Locale Preference Store 的新设置记录。它是新的客户端设置序列化，
  不是现有玩家业务存档迁移；实施包必须列格式、路径、默认、损坏／写入
  失败行为和跨设备不随云档覆盖的规则。
- 文本 artifact、资源 manifest／lease 和 Luban 生成物的发布格式。它们
  是生成／资源接口，不是玩家存档；版本与兼容策略须明确。

### 可能的源码表面变化

现有 `VisualElement` View 是 public 类型，但其调用者限于 Host 与测试。
迁移会改变这些 Presentation／Host 类型的基类、构造和序列化引用；这属于
运行时 UI Adapter 的替换，不得扩散为业务 public API 变化。系统设计必须
逐项决定“同名替换／新uGUI类型后删除旧类”，并列仓内调用点；不能笼统
声称 public API 完全无变化。场景／Prefab 的 Unity 序列化变化同样要进
精确 GUID 表，但不得触碰玩家存档格式。

## 6. 向系统技术设计的精确交接

`SYS-ENG-001` 必须以本报告和固定起点为输入，至少交付以下内容后，主程
才能形成可审查设计候选：

1. **精确文件清单。** 列全部修改／新建／删除路径，包括 Presentation、
   Host、Host/Editor、`FightMatchDemo.unity`、uGUI Prefab、字体、文本表、
   YooAsset接入、测试及每个自然`.meta`；注明共享文件唯一写入者。
2. **程序集图。** 给每个 asmdef 的最终 references；证明 Core/Application/
   Platform-save 不依赖Unity UI／Luban／YooAsset，第三方类型止于Adapter。
3. **uGUI页面与层级。** 给 Canvas／Scaler／GraphicRaycaster／EventSystem、
   安全区、启动／导航／战斗／弹层／参考覆盖／保存恢复的 Prefab层级与
   序列化引用；输入模块使用现有能力，不暗加依赖。
4. **控制器—视图绑定表。** 对每个旧 View 列新 Binder、事件、结果字段、
   启用／禁用、订阅／解绑、旧回调隔离和测试ID；明确100ms刷新、16ms播放、
   pause/focus/low-memory/destroy的替代时序。
5. **棋盘事件与几何。** 定义PointerId、鼠标／触控主键、多指拒绝、取消、
   拖线阈值、RectTransform→FlowPos、尺寸／安全区变化、遮挡和绘制层次；
   保持 `RouteGesture` 与Controller合同，不改规则坐标。
6. **本地化合同。** 定义键命名、模板参数类型、缺键／缺变量、诊断占位、
   语言规范化、首次选择、手动切换、页面刷新、动态显示名和字体字形；
   列所有现有硬编码玩家文案的迁移清单。
7. **Locale Preference Store。** 给独立存储接口、路径／故障／版本和测试；
   证明不改PlayerSave，不随战斗回退或局外选档覆盖。
8. **Luban链。** 固定候选版本、获取方式、schema、Excel／CSV目录、生成
   命令、输出目录、是否需要运行时库、跨Mac／Windows一致性、差异检查、
   key／语言／模板校验，以及如何进入既有审查／不可变发布。依赖改动另
   列单独实施权限。
9. **YooAsset链。** 固定候选版本与Package拓扑、内置bootstrap／远端集合、
   ReleaseSet映射、项目资源租约、handle寿命、R2 URL／CDN适配、校验、
   缓存、下载中断／恢复、移动网络确认、Wi-Fi切换和下次重启启用。哪些
   能力只待验证必须标明，不能预写通过。
10. **字体方案。** 核当前Noto源和许可，决定uGUI Text或TMP目标资产、英／
    简中字形、动态图集／静态图集、包体与bootstrap可用性；给实际画面验收。
11. **旧资源处置表。** 对PanelSettings、TSS、UIDocument引用和旧FontAsset
    给“保留至何门／替换GUID／何时删除／如何回滚”；Editor UI Toolkit
    明确保留。
12. **验证映射。** 将每个受影响旧UI测试映射到业务单测、真实uGUI
    EventSystem测试、场景保存重开、英／简中／占位符、Android画面与设备
    流程；复用旧业务证据，不因换视图无因重跑无变化范围。
13. **影响声明。** 明列Presentation public类型与Unity序列化变化；若任何
    Core/Application public API、PlayerSave、Packages、ProjectSettings或
    资源格式确需改变，先形成变更提案，不得夹在实现中。

系统设计不得把完整 HybridCLR、正式账号、广告SDK、完整R2发布后台或全
首版内容塞入本包。首 Demo 只实现本轮可试玩所需的最小 uGUI、本地化与
资源接入；H11后续能力保留明确缝即可。

## 7. 风险与未知

| 风险／未知 | 当前结论与下一责任 |
| --- | --- |
| Luban／YooAsset版本及Unity 2022.3兼容 | 未锁定、未安装、未验证；系统设计研究后由主程签依赖包。 |
| YooAsset断点续传与移动网络切换 | 只确定必须验证；不得依据框架宣传判通过。 |
| bootstrap循环依赖 | 最小下载UI、双语和字体必须内置且同源生成；系统设计给切片。 |
| 当前FontAsset与uGUI/TMP兼容 | 原OTF/OFL可保留，目标资产与中文字形实际核验未完成。 |
| public UI类型替换 | 可能造成仓内源码表面变化；限制在Presentation/Host并逐调用点列出。 |
| 场景／Prefab／meta | 当前没有目标Prefab；GUID策略和可恢复迁移必须先审查。 |
| 硬编码文案数量与语境 | 当前分散且含中英混排、状态码和字符串拼装；主策文本输入与系统盘点共同闭合。 |
| 语言偏好存储 | 必需但当前不存在；必须独立于PlayerSave并明示写失败／损坏策略。 |
| 资源句柄寿命 | 页面销毁、切页、旧回调、低内存可能早释／泄漏；项目资源租约与测试负责。 |
| 旧验证覆盖 | 旧4041、029 APK不证明uGUI／双语／新资源；QA按变化补真实事件和设备证据。 |
| H11完整热更 | 本轮只接资源取得边界；HybridCLR及完整版本切换不随本裁决自动实施。 |

以上均是系统设计／实现／验证任务，不构成改变已定路线的阻塞。

## 8. 验收自检

- [x] 绑定 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`，区分仓库事实、
  已定路线与未安装能力。
- [x] 固定uGUI迁移的依赖方向、模块所有权和UI框架隔离缝。
- [x] 明确复用 Application／Core／Content准入／Save／HostSession／Gesture
  契约，不重写业务或存档。
- [x] 列出UI Toolkit运行时的保留、替换和禁止范围；Editor UI Toolkit保留。
- [x] 固定项目自有本地化Binding、诊断占位、英／简中与语言偏好所有权。
- [x] 固定Excel／CSV→Luban→既有验证／发布的单向链，排除第二权威。
- [x] 固定YooAsset／R2只处于资源取得Adapter，排除第三方类型污染业务层。
- [x] 公开API与序列化影响已分“必须为零”“最小新增”“可能表面变化”。
- [x] 给系统设计13项精确交接和风险／未知，没有越权决定工具版本或文件白名单。
- [x] 没有引入Unity Localization／Addressables，没有假定Luban／YooAsset已安装，
  没有扩大到完整后台／账号／广告／全首版。
- [x] 本包未写产品代码、未运行Unity、未执行Git写入、未修改设置／依赖／资源／meta。

**架构结论：READY_FOR_SYSTEM_DESIGN。** 主程可将本文件作为
`SYS-ENG-001` 的固定架构输入；系统设计完成且独立R接受前，不得向C派发
产品实现。
