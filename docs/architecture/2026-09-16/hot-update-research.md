# 热更新选型研究：Android 优先，预留 iOS 与微信小游戏

2026-09-16 · r1 · **只读技术研究与选型建议；未冻结框架／依赖版本，未授权安装、升级 Unity 或实施。**

建议先在当前 **Unity 2022.3.18f1** 上验证 **HybridCLR 社区版负责 C# 逻辑加载，YooAsset 负责配置／资源／代码文件取得与缓存**。Unity Addressables 1.21 系列是资源层的主要替代项。微信必须增加官方转换 SDK 与平台文件系统适配，iOS 必须单独核对运行兼容和分发规则；现在没有证据可以承诺一个版本组合在三端直接通过构建、真机和上架。

本文采用 `research` 工作方式，官方技术文档、官方仓库与渠道规则分别取证。厂商“全平台”“零成本”等介绍只作为其声明，不当作本项目已经验证的事实。以下“推荐／推论”是本项目的设计建议，不是新增共同契约。

## 1. 输入、范围与成功标准

实际读取了 [AGENTS.md](../../../AGENTS.md)、[项目上下文](../../../.agent/PROJECT_CONTEXT.md)、[任务生命周期](../../../.agent/PLANS.md)、[验证规则](../../../.agent/VALIDATION.md)、[会话计划第 12～13 节](../../system-design/2026-09-16/session-plan.md)及 [SD01 配置发布设计](../../system-design/2026-09-16/details/configuration.md)。6 月“仅样例场景／无源码”等清单已陈旧，本文不据此描述现有代码；没有开展代码库存盘点。

研究必须保留的已确认要求：

- 首版同时具备内容热更新与战斗逻辑、程序修复、新技能机制的代码热更新能力；Android 先支持，iOS 与微信小游戏后续必须考虑。
- 游戏已经启动且所需资源已下载时，断网仍可闯关和养成；不把更新检查、登录或云同步变成每次战斗的联网前置。
- 跨平台接续范围是已确认的局外进度，进行中战斗不跨设备同步；本机保存和恢复仍须保留原尝试。
- `ContentBinding / DefinitionBinding`、旧规则执行能力、历史、未完成事务及已获广告权益不能被更新流程覆盖；更新回退不能靠回滚玩家业务存档实现。

验收标准为：列清框架职责、实际版本证据、平台限制与渠道边界；提出可维持离线与旧绑定的架构方向；列出有依据的成本及真实验证缺口；仅创建本文件。外部网页的读取日期均为 **2026-09-16**，具体读取页面与支持的结论在对应段落直接链接。

## 2. 候选与版本证据

“文档覆盖”指官方声明支持的系列；“确切组合通过”必须有当前项目构建与设备证据。两者不能混写。

| 候选 | 本次实际读取的官方证据 | 对本项目的判断 |
| --- | --- | --- |
| HybridCLR 社区版 | [latest 平台表](https://www.hybridclr.cn/docs/basic/supportedplatformanduniyversion)及 [8.5.0 版本化平台表](https://www.hybridclr.cn/docs/8.5.0/basic/supportedplatformanduniyversion)列出 Unity 2022.3.x、Android armv7/arm64、iOS arm64、WebGL／微信小游戏；[官方主分支 package.json](https://github.com/focus-creative-games/hybridclr_unity/blob/main/package.json)本次显示 `8.14.1`。 | 2022.3.18f1 落在其声明的系列内，适合进入验证。`8.14.1` 是观察到的包版本，不是已选定依赖；8.5.0 文档中的机制不自动证明所有新版本组合。必须固定 package、HybridCLR／il2cpp_plus 修订与 Unity 小版本一起验证。 |
| YooAsset | [3.0.x 系统要求](https://www.yooasset.com/docs/guide-editor/QuickStart)与 [2.3.x 系统要求](https://www.yooasset.com/docs/2.3.x/guide-editor/QuickStart)均列 Unity 2022.3、Android／iOS／WebGL；[官方 releases](https://github.com/tuyoogame/YooAsset/releases)有 `3.0.5`、`2.3.19`；[yoo3 包清单](https://github.com/tuyoogame/YooAsset/blob/yoo3/Assets/YooAsset/package.json)显示 `3.0.5`、最低字段 `2019.4`，并依赖 SBP `1.21.25` 等包。 | 优先资源候选。3.0.5 与 2.3.19 可作为冻结版本时的调查起点，尚未二选一；不能混用 2.x 示例和 3.x API。传递依赖也必须纳入后续授权与验证，本文不安装。 |
| Unity Addressables | [Unity 官方 2022.3 手册](https://docs.unity.cn/Manual/com.unity.addressables.html)本次列 `1.21.21`；[1.21.21 内容更新文档](https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/content-update-builds-overview.html)说明远端 catalog、AssetBundle 与内容状态文件工作流。 | 有明确 Unity 2022.3 系列依据，适合作为资源替代。尚未在 2022.3.18f1 导入／构建；也未核实本项目的微信 Provider／缓存组合。 |
| 微信官方转换链 | [官方 Unity／团结 SDK 仓库](https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk)说明 Unity／团结接入；[其包清单](https://github.com/wechat-miniprogram/minigame-tuanjie-transform-sdk/blob/main/package.json)显示 `com.qq.weixin.minigame 0.1.1`，`unity=2019.4`、`unityRelease=29f1`。 | 最低版本字段不能证明 2022.3.18f1＋某转换 SDK＋HybridCLR＋资源框架的完整兼容。确切导出版本、构建路径和真机矩阵**未验证**，不得把团结专有能力视为当前 Unity 已具备。 |

目前原 [微信 Unity WebGL 转换仓库](https://github.com/wechat-miniprogram/minigame-unity-webgl-transform)页面返回仓库被禁用；新官方仓库 README 所链的 [GitHub Pages 文档](https://wechat-miniprogram.github.io/minigame-unity-webgl-transform/)返回 404。[微信开发者文档入口](https://developers.weixin.qq.com/minigame/dev/guide/game-engine/unity-webgl-transform.html)本次未成功取得正文。这是本次证据取得限制，不是“微信不支持 Unity”的结论；不使用个人 fork、转载或第三方兼容表填补这个缺口。

## 3. 代码热更新：推荐与限制

HybridCLR 在 IL2CPP 中加入解释执行能力；其 [加载文档](https://www.hybridclr.cn/docs/8.5.0/basic/runhotupdatecodes)明确由项目的资源系统提供 DLL 字节，再按依赖顺序 `Assembly.Load`。因此它不替本项目完成 CDN、完整版本发布、资源缓存、存档迁移或账号同步。它允许继续用 C# 维护规则，是本项目保持客户端、攻略验证与内容验证共享规则的主要选型理由。

以下成本来自实际机制，不采信介绍页的“零工程成本”：

- [打包工作流](https://www.hybridclr.cn/docs/8.5.0/basic/buildpipeline)要求生成桥接等文件、排除热更新程序集、保留裁剪后的 AOT DLL，iOS 需先导出 Xcode 再编译。需要可重复的构建工序与产物归档，不能只下载 DLL 就宣布接入完成。
- [AOT 泛型文档](https://www.hybridclr.cn/docs/8.5.0/basic/aotgeneric)要求补充适用元数据；不同 BuildTarget 的裁剪 AOT DLL 不可复用，应用包对应的 AOT 产物不能随每次热更换成最新生成的一份。元数据有常驻内存和启动成本；同页出现不同内存倍率描述，本文不把某个倍率当项目预算，必须实测。
- [已列限制](https://www.hybridclr.cn/docs/8.5.0/basic/notsupportedfeatures)包括部分 Marshal 操作、热更 DLL 的 `RuntimeInitializeOnLoadMethod` 时机，以及该文档版本的解释代码 C# 调试能力限制。按所冻结版本重新检查，不以 Editor 正常运行代替 IL2CPP 验证。
- [商业版对照](https://www.hybridclr.cn/docs/8.5.0/business/intro)将完全泛型共享、DHE、热重载等列为不同商业能力。**社区版的下载后加载，不等于运行中任意卸载／替换已加载程序集。** [热重载使用说明](https://www.hybridclr.cn/docs/business/reload/quickstart)也要求成功卸载及清理回调、异步引用；付费不能自动解决本项目旧战斗状态的语义兼容。

推荐首版采用**启动时选择一套完整、兼容的代码集合，运行中只准备下一版，明确重启后启用**。这是可降低对象、回调、静态状态混用风险的设计建议；用户尚未决定具体提示和强制更新策略。当前战斗不中途换规则，旧绑定不改成最新值；不以“返回地图”自动等同于程序集已可安全替换。

原生 SDK、引擎、IL2CPP 运行时和既有 AOT API 的改变应按基座更新处理，走适用平台的正式发版流程；不能承诺只热更 C# 就能加入未随应用交付的原生库能力。热更程序集可用的 AOT API 范围必须成为版本兼容检查的一部分。

代码替代项可保留 **xLua**：[腾讯官方仓库](https://github.com/Tencent/xLua)提供 Lua／C# 互调与 Lua 替换 C# 方法的方案。但对本项目会引入第二种规则实现语言、绑定维护和同规则验证成本；若选择它，规则仍须有唯一实现，不能客户端 Lua、验证器另抄一份 C#。本次未建立 xLua 的确切 Unity 2022.3.18f1／微信版本矩阵，因此不推荐作为首选，也不声称它能绕过任何商店规则。

## 4. 配置／资源热更新：YooAsset 与 Addressables

两者都适合管理可下载资源，但都不是 C# 代码执行器。[Unity AssetBundle 定义](https://docs.unity3d.com/2022.3/Documentation/Manual/AssetBundlesIntro.html)明确它是平台相关的非代码资源归档；把 DLL 当字节资源运输后，仍需要 HybridCLR 等加载能力。Android、iOS、WebGL 资源应各自构建，不能将 Android AssetBundle 发给微信客户端。

| 方案 | 适合本项目的点 | 必须承担的工作与限制 |
| --- | --- | --- |
| YooAsset，优先候选 | [初始化文档](https://www.yooasset.com/docs/guide-runtime/ResourceInit)有内置／沙箱／Web 文件系统组合；[资源更新文档](https://www.yooasset.com/docs/guide-runtime/ResourceUpdate)可按标签或资源依赖创建下载器，符合“下载关卡后离线可玩”。 | 业务必须自己定义完整可用内容范围、选择本地版本和失败恢复。`HostPlayMode` 名称不意味着业务必须在线；`OfflinePlayMode` 文档定位是无资源热更，不宜仅因产品要离线游玩就直接选它。3.0 文档的版本请求步骤可由项目自行管理，需验证持久化的确切本地版本而非每次强取远端最新。 |
| Addressables 1.21，主要替代 | 官方维护，catalog／依赖加载及内容增量发布工序明确，可与代码文件运输配合。 | [官方更新流程](https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/content-update-builds-overview.html)要求按平台保存发布对应的 `addressables_content_state.bin`；Unity／Addressables 升级涉及新的 Player 与内容构建。独立代码热更是额外体系，不能当成 Addressables 原生能力；旧类型树、catalog 与代码的兼容需要项目验证。 |
| 直接 AssetBundle＋自写下载缓存 | 减少第三方资源框架依赖。 | 版本清单、依赖闭包、下载恢复、缓存、引用计数与平台适配都要自行维护；依照前两项已有能力推断，其项目工程成本更高，首版不推荐。 |

不能照抄示例“远端清单更新完成后马上启用全部新资源”：这里必须先由发布集合证明代码、定义、资源可共同使用。框架的资源版本号不替代 SD01 的内容指纹、规则／数值／随机版本与绑定。

微信方面，[YooAsset 官方接入说明](https://www.yooasset.com/docs/minigame/Wechat)明确需要 WX 插件和微信文件系统扩展、使用 Web 模式，且不支持同步加载；也要求版本文件不被缓存。源码入口确实存在于 [官方微信文件系统扩展目录](https://github.com/tuyoogame/YooAsset/tree/yoo3/Assets/YooAsset/Samples~/Mini%20Game/Runtime/WechatFileSystem)。这比“全平台”口号提供了更具体的适配依据，但并非真机通过证明。Addressables 的 [WebGL 同步加载限制](https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/SynchronousAddressables.html)同样禁止 `WaitForCompletion`，共享流程应从一开始采用异步资源取得。

## 5. 三个平台及分发渠道分别验收

### Android，先实施验证

HybridCLR 平台表给出了 Android armv7／arm64 声明；建议先以 arm64 构建、真机更新和断网场景取得本项目证据，最低 Android 版本与设备档位还未冻结。

**用户未选择 Android 商店／市场。** 若以后选择 Google Play，[其 Device and Network Abuse 政策](https://support.google.com/googleplay/android-developer/answer/16559646?hl=en)限制 Play 之外自更新和下载可执行代码，同时对经虚拟机／解释器、间接访问 Android API 的代码规定例外及其他政策约束。不能因为文件扩展名是 `.dll` 或框架叫解释器就推定满足例外；必须检查具体桥接、代码范围和发布行为。这是 Google Play 渠道边界，不概括为全部 Android 分发禁止热更新；其他商店规则本次未核查。

另一个独立的技术验收项是 **16 KB 内存页设备兼容**：[Unity 2022.3.56f1 发布说明](https://unity.com/releases/editor/whats-new/2022.3.56f1)才列新增支持，当前 2022.3.18f1 没有据此得到兼容证明；热更 DLL 无法自行替换 Unity 预编译原生库。[Android 官方指南](https://developer.android.com/guide/practices/page-sizes)要求检查全部原生库的 ELF／打包对齐并在 16 KB 环境验证；本次页面在 Google Play 更新要求中写的是 2027-02-01，不沿用旧公告日期。最终以选定渠道和提交时规则为准，当前研究和 Android 原型工作不因此停止，也不在本任务升级 Unity。

### iOS，运行可行与 App Store 许可分开

HybridCLR 的 iOS arm64 支持和 Xcode 打包说明，只证明供应方提供技术路径。[Apple App Review Guidelines 2.5.2](https://developer.apple.com/app-store/review/guidelines/#software-requirements)限制下载、安装或执行会引入／改变 App 功能的代码；教育用途例外不能直接套给普通游戏。[4.7](https://developer.apple.com/app-store/review/guidelines/#mini-apps-mini-games-streaming-games-chatbots-plug-ins-and-game-emulators)针对特定软件类型并附加要求，未明确豁免普通 Unity C# DLL 热更新。

因此，**不能承诺 App Store 渠道可任意远端新增技能机制或更换战斗功能**；是否满足允许范围须单独核对，不能借资源后缀、混淆或厂商案例绕开评估。若某项代码变更不符合目标分发规则，按正式应用版本发布该变更；这并不取消 Android 的代码热更需求，也不擅自缩减用户要求，属于必须向 SD00 标出的平台交付差异。微信小游戏在 iOS 微信中运行也不等同于本项目原生 iOS App 的分发路径。

[Apple 当前 SDK 门槛](https://developer.apple.com/news/upcoming-requirements/?id=04282026a)要求自 2026-04-28 起上传 App Store Connect 的 App 使用 Xcode 26 或以上及对应 iOS 26 等 SDK。当前 Unity 2022.3.18f1＋所选 HybridCLR＋新 Xcode 的构建与签名组合未验证；不能拿历史文档中的旧 Xcode 最低版本保证现在可提交。此项留在后续发布矩阵，不自动转为升级引擎决定。

### 微信小游戏，独立的导出／运行适配链

建议保留 `Unity WebGL/IL2CPP → 官方转换 SDK → 微信小游戏运行环境` 的平台构建方向。官方 SDK 仓库有 Unity／团结接入入口，但当前确切版本组合证据缺口见第 2 节；[腾讯云同源适配介绍](https://intl.cloud.tencent.com/zh/document/product/1219/78928)可佐证 WebAssembly 与 wx API 转换的机制，其 TCSAS 宿主限制不直接当作微信当前配额。

[Unity 2022.3 WebGL 技术说明](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-technical-overview.html)列出托管线程、System.Net、缓存 API 等差异。由此推论，本项目共享规则不能依赖原生线程／文件路径；下载、保存和超时实现应放在平台接入边界。微信 SDK 可能提供额外能力，必须逐项实测，不能把 Android 实现原样搬过去，也不能将桌面浏览器支持表当微信真机支持表。

HybridCLR 的 [微信专门说明](https://www.hybridclr.cn/docs/basic/supportedplatformanduniyversion)指出转换工具的 `Faster (Smaller) builds` 与 AOT 泛型元数据有交互；采用社区版时应验证泛型、裁剪、内存峰值与冷启动成本。官方提到团结 `metadata slim` 的条件只适用于团结，不构成本项目迁引擎依据。

本次未成功读到微信当前审核对远端 C# DLL／WASM 更新的具体许可条款、缓存配额／回收规则和精确可用 SDK 清单，均标 **未验证**。后续必须分别测试 Android 微信和 iOS 微信的首次进入、已下载后断网、进后台被回收、再次进入及空间不足；目前只保留用户已确认的“启动后、资源已下载”的离线范围，不承诺所有微信离线冷启动都可行。

## 6. 提交 SD00 的架构方向

以下是职责建议，根会话决定系统编号、公共类型和接口，本文不修改共同定义。

1. **稳定启动基座**负责识别本机已完整安装的集合、检查与基座的兼容、验证发布来源／文件完整性、装载代码并交出控制权。它需要在游戏逻辑失效时仍能读更新元信息；不能依赖待加载的新游戏 DLL 才能决定怎么恢复。
2. **更新发布集合**把同一次发布需要的基座条件、平台／渠道、代码及 AOT 产物身份、资源清单、SD01 内容绑定和可执行规则能力联系起来。集合只引用 SD01 的定义包；不生成第二套职业／关卡配置权威源。
3. **下载和启用分开**：下载到候选位置，完成所需闭包校验与落盘后才登记“可启用”；运行中不混用半份新包。启动选择必须考虑最新完整业务存档的全部绑定，不能只看 CDN 最新版本。
4. **本地可用优先维持离线**：已启用集合及所需关卡资源完整时，版本查询失败或暂时断网不阻断已获准的闯关、养成。未下载内容可明确不可用；广告和在线分析仍各按其网络能力处理，不用它们的失败阻断普通游玩。
5. **旧规则必须可执行**：新代码集合必须证明支持仍被活动尝试、历史、未完成结算／广告记录、物理备份和未知提交候选引用的旧规则；只保留旧 JSON 不够。首版可在新规则代码内保留按版本选择的旧执行能力。无法证明时继续旧集合／停止受影响启用，或另批显式迁移，不能静默升级绑定。
6. **单进程不承诺同时装载同名 DLL 多版**：首版不依赖商业热重载，也不通过重复 `Assembly.Load` 自造多版本隔离。若只能用旧代码恢复本地战斗，保持那套代码直至具备安全切换条件，下一次完整启动再选择兼容集合。
7. **代码回退不回退玩家**：新集合启用前的失败保留原完整集合；新集合一旦写出旧集合不能读的存档，就不能只改版本指针盲目回退。回退也须通过“当前完整存档可读且语义兼容”检查，不满足时保留资料并给恢复阻断。
8. **云存档服务与下载服务独立**：更新服务器失联不代表普通离线进度不可提交本地；取得云端局外状态时也要核对当前代码／内容能力。不上传进行中战斗来解决更新，不用版本切换绕过玩家的冲突选档与广告权益保护。
9. **同规则的生产与验证**：客户端、攻略验证和内容验证使用同一规则来源及版本身份；各平台仍分别生成实际产物。更新前用同输入／随机初态检查结果一致，不能只换客户端 DLL 却沿用不匹配的验证服务。

第 5 点会带来版本保留成本，与 SD01 首版不自动裁剪旧包的建议一致；后续垃圾回收必须依据 SD02 提供的引用根，不能由资源框架按“不是当前版本”自行删除。发布签名／信任根、原子登记方式、恢复错误对应关系和用户提示仍交后续详细设计，不在这里凭研究建立公共协议。

## 7. 有依据的成本与替代取舍

| 成本 | 证据与项目影响 |
| --- | --- |
| 框架许可 | HybridCLR [MIT 说明](https://www.hybridclr.cn/docs/8.5.0/intro)及 [商业对照](https://www.hybridclr.cn/docs/8.5.0/business/intro)列社区版 0 元，商业版需商务报价；YooAsset [官方仓库](https://github.com/tuyoogame/YooAsset)采用 Apache-2.0。这里的无许可费不包括工程、测试、CDN 或商业支持成本；未询价、未承诺购买。 |
| 接入与维护 | HybridCLR 有 AOT／桥接／裁剪和基座对应产物；YooAsset 有资源构建／清单／文件系统，Addressables 有每平台内容状态文件。推论：都需要可重复发布、故障恢复与回归，不能以一个示例包运行作为完成。 |
| 运行内存与包体 | 社区版 AOT 补充元数据、DLL 装载及旧资源留存需要额外预算；微信更需要按设备测量。先测再决定是否需要商业泛型／元数据优化，不能先认定必须付费。 |
| CDN 与存储 | 按平台保存基座对应的代码／元数据／资源及受引用旧版；下载量近似“活跃安装数 × 每次实际增量字节数”，另加失败重试与回源，存储随保留版本增长。此为容量估算方法，不是供应商报价。未给下载体量、地区、并发与流量前不编造月费。 |
| iOS／微信验证 | iOS 需要可用 Mac／Xcode、签名和设备验证；微信需要官方转换链和两种宿主系统验证。它们是额外平台成本，不因 Android 已完成而归零；具体工期、设备和账号成本本次不报价。 |
| 替代方案 | Addressables 可以替换 YooAsset，但需另证微信缓存／Provider与代码加载顺序。xLua 会增加跨语言维护且不取消政策核对；直接 AssetBundle 方案把框架现成职责变成项目自建工作。当前项目没有证据表明这些成本比推荐组合更低。 |

因此推荐的是**优先验证的方向**，不是采购／依赖选择结案。当前 Unity 不为迎合框架而升级；以后若确切平台构建证据表明引擎是障碍，再单独提交范围明确的版本决策。

## 8. 实施前必须取得的验证证据

以下场景全部 **NOT RUN**；它们是后续任务的验收输入，不是本轮测试通过记录。

| 编号 | 验证输入 | 必须观察到的结果 |
| --- | --- | --- |
| HU01 确切版本 | 固定 2022.3.18f1、HybridCLR package／运行时修订、资源框架与传递依赖、IL2CPP、目标架构。 | Android Release 构建和真机 DLL 加载成功；反射、泛型、序列化、裁剪及新增技能入口覆盖，记录产物哈希。Editor 模拟不算。 |
| HU02 离线范围 | 已启动、完整下载原集合与关卡后断网，继续行动、通关、经验卡／合成与本机提交。 | 不因更新／登录／同步失败阻断；恢复联网后进入既定同步流程，无重复奖励。冷启动离线单独记录能力，不扩大既定范围。 |
| HU03 下载中断 | 下载到一半、校验损坏、磁盘不足、候选完成而启用登记中断，各点杀进程。 | 只恢复完整集合，无半包混用；当前业务状态、库存与广告权益不丢失；损坏文件不执行。 |
| HU04 版本不一致 | 新代码配旧资源、新资源配不支持的基座、错误 BuildTarget AOT DLL、缺签名／哈希不符。 | 拒绝不兼容集合；不自行拼接“最新”各部分，不以改业务绑定补救。 |
| HU05 旧规则引用 | 本地 V1 Boss 第二面、旧历史和未完成权益仍存在，下载 V2；客户端重启。 | 原尝试按 V1 继续且能回退／重演，已固定奖励不按 V2 重算；V2 无旧能力时不静默启用。 |
| HU06 回退兼容 | V2 已写出新存档后判定代码需回退；旧集合不能读取该存档。 | 不覆盖或加载旧业务存档假装修复；保留最新完整资料并报告阻断，等待兼容修复／批准迁移。 |
| HU07 共享规则 | 客户端 Android、候选 iOS／微信与独立验证器使用固定输入、旧新规则及同随机初态。 | 伤害、CD、评分、奖励与状态结果一致；差异使该发布失去可启用资格。 |
| HU08 平台与渠道 | 微信确切 SDK 在 Android／iOS 宿主；iOS 用现行上传 SDK；Android 对目标渠道和 16 KB 设备检查。 | 记录构建、缓存、启动与内存实测；分开报告技术通过和渠道更新范围核对。不能报告“框架支持三端，故通过”。 |
| HU09 跨设备版本 | 旧设备离线，新设备取得局外进度／新内容，之后重新连接。 | 普通进度进入既定兼容检查和冲突选档；广告权益独立保护，不同步活动战斗，不让 CDN 当前版本替代存档规则身份。 |

尚未冻结：具体 HybridCLR／YooAsset 或 Addressables 版本，微信转换版本，最低系统和设备，商店／地区，CDN、发布签名方式，强制更新及重启交互。它们不阻止本轮架构方向交付，阻止“已可三端正式发布”的声明。

## 9. 本轮验证记录

- **RUN — 官方网页只读核查。** 已读取本文对应链接的 HybridCLR 平台／加载／打包／泛型／限制／商业对照，YooAsset 系统要求／发布记录／包清单／初始化／下载／微信扩展，Unity AssetBundle／Addressables／WebGL 文档，微信官方 SDK 仓库与包清单，Apple 2.5.2／4.7／SDK 要求，Google Play 代码政策、Android 16 KB 指南及 Unity 2022.3.56f1 发布说明；未成功读取的微信文档已逐项注明。政策核查另由独立只读研究子任务复核。
- **RUN — 本地文档核对。** 第 1 节列出的本地输入通过 `Get-Content`／`rg` 读取，命令退出码 0；创建前 `Test-Path -LiteralPath 'docs/architecture/2026-09-16/hot-update-research.md'` 返回 False。
- **NOT RUN — Unity 导入、编译、EditMode、Player 构建、HybridCLR 安装／生成、资源构建、SDK 运行、真机和故障注入。** 原因：本任务仅授权研究与单文件交付，未安装任何候选依赖，也没有实际构建产物可验。
- **NOT RUN — 渠道提交、CDN 部署、真实广告、在线分析／外部模型。** 原因：不在本任务授权范围，本文不报告上线或费用承诺。
- **RUN — 文档静态检查。** PowerShell 解析本文件 Markdown 链接并对本地目标执行 `Test-Path`，全部可解析；外部来源链接共 40 处，未把不可取得正文的微信入口算作已验证正文。检查尾部空白、冲突标记及占位符，均为 0，命令退出码 0。
- **RUN — `git diff --no-index --check -- NUL 'docs/architecture/2026-09-16/hot-update-research.md'`。** 退出码 1，仅输出仓库 LF 将转 CRLF 的提示，无空白错误；新文件另用上述 PowerShell 实查。`git status --short -- 'docs/architecture/2026-09-16/hot-update-research.md'` 返回该文件为未跟踪新文件。没有把普通 `git diff` 对未跟踪文件的空输出当作内容检查通过。
- 文件范围：仅创建本研究文件；不修改共同契约、图、源码、测试、配置、资产／`.meta`或依赖，不执行 Git 提交／推送／分支等变更。仓库有其他会话并行工作，本文不对其变更作归属判断或整理。
