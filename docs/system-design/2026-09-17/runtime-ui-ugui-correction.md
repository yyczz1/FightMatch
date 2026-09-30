# FightMatch 运行时 UI 选型纠正与迁移范围

2026-09-30 · SD00 · 状态：**用户选型已明确；迁移未实施；追加本地化要求**。

## 1. 生效决定及责任

用户本轮指出：“没有经过讨论就用了 uitoolkit，而不是 ugui……我要求的是编辑器用 uitoolkit”。FightMatch 的游戏运行时 UI 采用 uGUI；UI Toolkit 用于编辑器工具。权威约束已写入根目录 [AGENTS.md](../../../AGENTS.md)。旧任务包中的运行时 UI Toolkit 要求由本决定覆盖。

原 AGENTS.md 和 FlowPuzzle 设计仅要求 Editor 主界面使用 UI Toolkit。SD00 后续在任务包§241把它用于可挂载到游戏场景的棋盘，CONT-C设计又明确要求 Editor/Player/Android 共用 UI Toolkit 视图，029宿主继承了该选择。没有找到对此运行时选型的单独用户确认；不能把允许持续实施、同产品共用业务代码或旧R技术审查当成这种确认。SD00承担选型及派发边界失守的责任。

本决定不对所有项目中的 UI Toolkit 作普遍性能结论。当前工程按用户要求纠正；业务共用要求不意味着编辑器和玩家必须共用同一种视图树。

主Goal仍按§457暂停。本轮修正规则、整理实际影响与后续验收，不启动C产品实施、Unity、APK重建或新的长测试。独立性能专项按自己的现有授权执行。恢复UI实施时先完成下述具体设计及独立审查；uGUI选型本身不重复询问用户。

## 2. 已核实的影响范围

| 层 | 实际文件或对象 | 迁移责任 |
| --- | --- | --- |
| 棋盘输入 | `Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs`、`CandidateBoardInputView.cs` | VisualElement绘制、指针捕获、坐标换算和控件绑定改为uGUI适配；保留已有手势规则。 |
| 战斗表现 | 同目录 `CandidateBattlePlaybackView.cs`、`PlayerBattleView.cs`、`PlayerDefaultReferenceView.cs` | 替换视图树和UI Toolkit调度，保留已提交事实、播放令牌及默认攻略预览语义。 |
| 菜单与永久操作 | 同目录 `PlayerNavigationView.cs`、`PlayerPermanentDetailView.cs`、`PlayerNavigationRecoveryView.cs` | 地图、队伍、背包、合成、确认、恢复及原结果显示改用uGUI视图。 |
| 导航绑定 | 同目录 `PlayerNavigationController.cs` | 同文件内的`NavigationBindings`直接创建UI Toolkit按钮；迁移此绑定，保留控制器的请求身份和过期回调保护。 |
| 真实宿主 | `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`、`FightMatchHostView.cs`；`Assets/Scenes/FightMatchDemo.unity` | 替换UIDocument、VisualElement、PanelSettings依赖，接入Canvas及EventSystem，重接安全区、返回键和生命周期。 |
| 资源与装配 | `Assets/UI/FightMatch/FightMatchPanelSettings.asset`、`FightMatchTheme.tss`；`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`及关联程序集 | 替换仅服务运行时UI的装配和检查。Editor脚本构建Player UI时也须按uGUI目标执行；并非将编辑器工具UI改成uGUI。 |
| 现有UI验证 | `Assets/Tests/EditMode/FightMatch/`中的棋盘、播放、导航、默认攻略和战斗界面测试；`Assets/Tests/EditMode/FightMatchHost/`中的宿主资源、路由及fixture | 用真实uGUI事件/场景覆盖原行为；按具体测试映射保留业务断言，不以旧EditorWindow/panel绿测证明新运行时UI。 |

这是迁移影响清单，**不是产品文件修改白名单**。C设计必须给出全部精确修改/新建/删除路径、meta/GUID安排、程序集和必要公开类型变化，随后单独签实施包。

`Packages/manifest.json`已经包含`com.unity.ugui: 1.0.0`；本次没有安装包或改锁文件。`FightMatch.Input`的手势规则、Core/Application/Platform业务、HostSession和大部分控制器具备复用基础，迁移前仍逐项检查生命周期边界。不得重建第二套战斗、结算或存档。

字体原始文件和许可继续保留；现有TextCore FontAsset与目标文字组件的兼容性须在具体方案中核对，不能仅换字段而遗漏中文、字体图集或许可显示。

## 3. 修正设计必须交付的具体内容

1. **玩家流程与页面结构**：列出现有页面、弹窗、控件和实际业务入口，给出uGUI层级/Prefab与绑定表；场景和可复用资源能在Unity编辑器中检查、编辑。页面导航和确认/恢复行为保持既定规则，布局与视觉变化单列供评审。
2. **输入与播放接线**：从EventSystem事件到既有Gesture/Controller的映射，覆盖指针身份、拖线、回退确认、双指不抢占、取消、失焦、安全区/尺寸变化、页面销毁及重建。明确替代UI Toolkit schedule的生命周期和解绑位置，旧回调不得改新页面或解新播放锁。
3. **视觉与美术接口**：明确Canvas层级、棋盘/HUD/角色舞台、遮罩、弹窗和触摸遮挡关系，以及不同手机比例下的布局。PNG及其他原始素材保持独立，不再要求美术为VisualElement、UXML/USS或PanelSettings制作运行时交付。
4. **资源与场景迁移**：给出新资源引用、旧运行时专用资源的移除条件、GUID保留/替换表和字体方案。编辑器工具及其UI Toolkit测试保持原职责，不能全仓搜索删除UI Toolkit引用或移除Unity内置UIElements模块。
5. **验证映射与分包**：每个旧行为映射到新uGUI验证，区分业务单测、真实uGUI事件和设备试玩。先修源码与静态检查，固定实现后按影响执行必要验证；已有有效证据保留，旧长测试不因换文档无因重跑。

本决定只确定uGUI。Spine、其他动画包、UI框架、渲染管线或新的输入包不由此自动获批；美术会话中的讨论图和候选方案也不自动变成依赖授权。具体方案先复用当前工程能力，新增选型若确有必要，说明其影响再作决定。

## 4. 迁移验收条件

- Android正常玩家入口使用uGUI Canvas和实际EventSystem；FightMatch运行时棋盘、菜单、HUD及宿主不再依赖UIDocument/VisualElement/运行时PanelSettings。Editor专用UI Toolkit可以继续存在。
- 地图/队伍/背包→入场→战斗→真实结算→返回/重打，以及保存待办、冷启动恢复沿同一业务系统与原存档继续；更换UI不能要求玩家清档。
- 实际uGUI事件覆盖触摸/鼠标、指针取消、页面反复进入退出、重复和过期回调、播放中断。仅直接调用业务方法或旧panel事件不算完成此项。
- Android实际画面检查文字、热区、比例与安全区、层级/遮挡、路线与命中位置一致；资产引用须保存、关闭、重开后仍有效。
- R独立检查准确迁移补丁、范围和证据，给出唯一ACCEPT/NEEDS_FIX/REJECT。源码出现、作者声明、旧4041绿测或旧APK能启动不能代替这轮验收。

## 5. 旧成果及后续安排

CONT-C/028等旧审查和运行证据仍准确描述其当时的源码及合同，不删除、不改写成uGUI证据；其中运行时UI框架选择已被用户本次纠正。029尚未产品接收，后续接收必须先满足本决定，不能继续把旧Toolkit宿主当最终基础。

首批美术导入提案 `ArtSource/FightMatch/2026-09-29/unity-import-plan.md` 中关于VisualElement、UXML/USS、PanelSettings的运行时接入建议已失效；源图身份和创作证据不因此作废。由美术任务后续更新自己的提案，C实施前按已审uGUI资源映射接收。

后续顺序：C给具体迁移设计与精确范围 → R独立设计审查 → SD00签限定实施包 → C实现及串行必要验证 → R代码/设备证据审查。当前仍停在文档纠正；未派迁移实施，未改产品代码、场景或美术文件。

## 6. 多语言与预制体文字要求（2026-09-30）

用户要求面向海外采用多语言，所有玩家可见预制体Text使用类似`【if you see this, it is a bug.】`的显眼占位符，运行时由代码替换；游玩中出现该文字就是未正确接入。访谈Q12已明确首发英语和简体中文，Q19已选择Luban文本表＋项目内本地化绑定，资源走YooAsset。此要求属于uGUI迁移设计的必要输入；没有安装新包，不能另以官方Localization/Addressables代替已选路线。

- 玩家可见的Text/TMP默认文字须保留诊断占位符；静态标签、按钮、弹窗及带数值/物品名的句子统一通过本地化键和模板生成，不在预制体放最终正文来隐藏漏绑定。纯运行时数值也应由实际数据正确填充。
- 文本表遵守Q7的Excel/CSV主要编辑入口，校验后生成发布数据；不能让Unity表、JSON和Excel各自成为独立编辑权威。既有内容校验、不可变发布和旧局绑定继续适用。
- Q13已定首次按手机系统选择：中文系统用简体中文，其余用英语；设置中可手动切换并记住选择。设计须覆盖漏绑定、缺键、变量不匹配、字体字形覆盖、页面重建以及语言切换后的刷新；未正确替换必须可识别，不能凭另一处写死文案冒充本地化成功。具体刷新接线和缺翻译回退仍待细化。
- 迁移验收须在英语/简体中文下实际遍历玩家流程，检查占位符残留、排版/截断和动态句子；静态扫描或旧UI测试不能代替实际画面。扫描绑定/键/占位符可作为额外检查，具体路径及必要测试随实施包签定。

当前官方Localization与表格轻量绑定是待比较方案。官方包默认使用Addressables，不能忽略它与已定YooAsset的桥接和发布成本；也不凭“只做两种语言”直接决定自行编写一套。精确依赖及范围待讨论和技术验证后再签，现阶段未改Packages、资源或产品源码。

访谈Q14另明确：广告首通16与另一档普通首通16重叠、选择后者时，仅保留1张首通证。通关奖励的UI说明区域须明确写出“首次通关奖励”，不以弹窗/飘字代替常设说明。Q15确定新版后台下载完成后，在地图等自然页面边界提示重启，并允许稍后更新；迁移设计须包含这两个界面要求。
