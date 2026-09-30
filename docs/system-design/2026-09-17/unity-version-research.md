# Unity 版本路线研究

2026-09-30 · Q16 研究及决定记录 · **用户已选择保留2022.3、发行准备先验证62f3；本稿不是实施包、迁移实测或 R 验收。**

提出 Unity 6.3 的理由是未来维护周期，以及在新 UI、认证、广告与热更新正式集成前确定共同版本基线。现有证据不支持“2022 做不了这些功能”或“Google Play 强制 Unity 6”。2022.3.18f1 可以继续开发；2022.3.62f3 是真实的发布补丁候选；是否现在迁移 6.3，要用预计上线时间、维护年限、设备范围和实际兼容结果决定。

## 1. 范围与已定事项

- 使用 research 技能，只读仓库与官方主来源；唯一新增文件为本稿。未安装、启动 Unity、构建、运行测试、查看私密凭据或修改 Git、Packages、设置及产品文件。
- 已定海外 Google Play 首发、runtime uGUI／Editor UI Toolkit、HybridCLR＋YooAsset、Luban、英语／简体中文；这些选择保留。后续Q16已选62f3作为验证候选；编辑器升级实施、框架与新 SDK 的精确版本仍须单独冻结。
- **事实**为仓库或主源直接支持；**判断**为基于事实的取舍；**待验**不能用来源阅读冒充项目实测。外部资料统一查询于 2026-09-30。

## 2. 当前项目实际基线

| 仓库事实与精确位置 | 对路线的影响 |
|---|---|
| [ProjectVersion.txt](../../../ProjectSettings/ProjectVersion.txt) L1–2：2022.3.18f1；[ProjectSettings.asset](../../../ProjectSettings/ProjectSettings.asset) L172–173：min API 22、target API 32 | 当前版本与当前发布设置要分别评估；改 target 数字不能自动补齐引擎支持。 |
| [FightMatchAndroidBuild.cs](../../../Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs) L147–153：固定 IL2CPP／ARM64、min22／target32、0.1／code1，并拒绝 AAB | 三条路线最终都需新 Play 构建合同；不能直接复用现有单 APK 验收条件。 |
| [manifest.json](../../../Packages/manifest.json) L5、9：TMP 3.0.6、uGUI 1.0.0；[packages-lock.json](../../../Packages/packages-lock.json) L93–94：UTF 1.1.33 | 2022 已能实现已定 UI；换 6.3 会改变相关包基线，不是首次获得 UI 能力。 |
| Core／Application／Content／Input／Platform 的 .cs 静态搜索未见直接 UnityEngine／UnityEditor 引用；五个 asmdef 均 `noEngineReferences:true` | 主要改动预计集中于表现、宿主与平台构建。Application 的 [asmdef](../../../Assets/Scripts/FightMatch/Application/FightMatch.Application.asmdef) L7 仍引用 QFramework，不能据此宣称完整依赖链无引擎耦合。 |
| 当前 manifest 未列 URP／HDRP、HybridCLR、YooAsset、认证／广告包；Assets 文件清单未发现 .dll／.so／.aar／.jar 或 .shader／.hlsl／.compute 文件 | 尚无大量这类插件或自定义渲染迁移债务的证据；不代表 Unity Player 没有原生库，也不证明未来 SDK 兼容。 |
| [uGUI 修正计划](runtime-ui-ugui-correction.md) L19–25、31、45–53 已列视图、宿主、场景、字体和验证迁移；L61–68 列双语言要求 | UI 重做本来就要进行，不能全计成升级成本，也不能把它描述成升级自动完成的收益。 |
| [029 冻结计划](demo-029-mac-r1-plan.json) L366–403 固定编辑器及文件身份；[验证入口](../../../Tools/Invoke-FM029Validation.ps1) L274–293 校验脚本、计划、任务包和运行时身份 | 换补丁同样要另签验证基线。旧结果保留原适用范围，不能改版本字符串后继续称为新版本证据。 |

## 3. Play 需求与安全修复不等于大版本要求

| 官方事实 | 本项目结论 |
|---|---|
| Google 当前规则要求 2026-08-31 起新应用及更新 target API 36；延期需实际获准。[目标 API 政策](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en) | 当前 target32 不是首发基线。target API 与最低 Android 版本不同，不因 target36 自动放弃旧设备。 |
| Play 面向 API35 及以上的 64 位应用须支持 16 KB 页；当前页面另注明 2027-02-01 起不兼容的更新将无法发布。[16 KB 官方说明](https://developer.android.com/guide/practices/page-sizes) | 以兼容为首发目标，不能把更新截止日期解读成当前新应用的豁免。实际 AAB／生成 APK、原生库及设备仍待验。 |
| Unity 2022.3 官方手册明确 API36 从 62f1 支持，16 KB 从 56f1 支持。[2022.3 Android 兼容性](https://docs.unity3d.com/2022.3/Documentation/Manual/android-requirements-and-compatibility.html) | 18f1 不在这些支持范围；62f3 已覆盖两项引擎基础要求。单凭这两项不能推出必须迁移 6.3。 |
| Google 新应用自 2021-08 起须用 AAB 发布。[App Bundle](https://developer.android.com/guide/app-bundle) | 当前“只 APK、拒 AAB”规则必须修订；这个工作与是否换 Unity 大版本无关。 |
| Unity 2025 安全公告列 2022.3.62f2 等为修复版本；62f3 后续包含该修复。[安全公告](https://unity.com/security/sept-2025-01)、[Unity 官方发布说明](https://discussions.unity.com/t/is-there-a-memory-leak-with-2022-3-62f2/1689592/41) | 18f1 不应作为不经修复审查的公开发行基线；2022 内即可修复，安全问题也不单独构成迁移 6 的理由。未对旧 APK 作漏洞检测。 |

## 4. 可获取版本、维护窗口与授权

| 路线 | 已证事实 | 实际代价与边界 |
|---|---|---|
| 保持 2022.3.18f1 | 当前工程和旧工具证据的原基线；低于上述平台支持及安全修复补丁 | 可以继续文档、配表和当前玩法开发；延后引擎改动并不消除发行前升级及构建改造。 |
| 2022.3.62f3 | 2025-10-28 发布；Unity 官方人员称其为最后公开 2022.3 补丁。[下载／发布页](https://unity.com/releases/editor/whats-new/2022.3.62f3)、[官方人员说明](https://discussions.unity.com/t/is-there-a-memory-leak-with-2022-3-62f2/1689592/41) | 变动范围较小的真实候选，具备 API36／16 KB 基础。2022.3 常规支持已于 2025-05 结束，不应为了少改就把它说成仍有持续常规维护。[Unity 官方维护公告](https://discussions.unity.com/t/unity-devops-build-automation-2026-dependency-deprecation-cycle/1724029) |
| 2022.3 扩展维护补丁 | 官方仍有标作 3-year LTS 的历史下载，例如 2022.3.76f1，发布于 2026-05-06。[发布页](https://unity.com/releases/editor/whats-new/2022.3.76f1) | 能下载不等于所有许可证有使用权。Enterprise 官方提供额外一年 LTS；本用户具体订阅、该补丁资格、特殊支持合同及费用未核定，不默认需要或已经购买。[Enterprise](https://unity.com/products/unity-enterprise) |
| Unity 6.3 LTS | 官方支持至 2027-12；Enterprise／Industry 另有一年扩展安排。6.0 仅到 2026-10，不能把“Unity 6”笼统当成同一个维护窗口。[版本支持表](https://unity.com/releases/unity-6/support) | 6.3 是较长常规维护窗口的候选。已能获取 6000.3.25f1（2026-09-24），但本稿未选它，也不能以最新为由跳过插件精确匹配。[发布页](https://unity.com/releases/editor/whats-new/6000.3.25f1) |

2022.3 的常规支持已结束，扩展维护也有期限；当前未找到可据以承诺本项目在 2026-09-30 后继续获得 2022.3 常规修复的官方依据。已有下载和历史扩展发布不重启维护时钟。若拟依靠付费扩展补丁，先核现有权益和剩余支持内容，再比较费用；本稿不推断用户许可证类别或报价。

## 5. Unity 6.3 对本项目的收益与成本

**实际收益判断：**若游戏预计在 2027 年持续发行和接入平台 SDK，6.3 能提供仍在常规支持期的引擎基线，降低以后被迫在已上线内容与 SDK 上同时迁移的概率。现在插件集成较少、UI 正待重做，可能比接入完成后更容易隔离版本问题；这是工作顺序判断，尚无工期或性能测量。

**不应计入收益：**uGUI、双语言文字、Luban 配表、HybridCLR／YooAsset、Android IL2CPP、账号和广告并非 6.3 独有。Luban 自身面向 Unity 等多种引擎；其选用不构成编辑器升级理由。[Luban 官方仓库](https://github.com/focus-creative-games/luban) 当前没有证据证明本项目迁移后帧率更高、包体更小或开发时间必然更短。

| 变化 | 本项目实际影响与待验事项 | 官方来源 |
|---|---|---|
| 最低设备范围 | 2022.3 最低 API22，6.3 最低 API25（Android 7.1）；若迁移，Android 5.1／6.0／7.0 将不再属于支持范围。是否要覆盖这些旧机型是产品取舍，不能偷偷改 minSdk。 | [2022.3 要求](https://docs.unity3d.com/2022.3/Documentation/Manual/android-requirements-and-compatibility.html)、[6.3 要求](https://docs.unity3d.com/6000.3/Documentation/Manual/android-requirements-and-compatibility.html) |
| Android 工具链 | 官方表中 2022.3 为 JDK11／NDK r23b，6.3 为 JDK17／NDK r27c；要重核 Android 模块、原生插件与打包模板。不能把本机现有 SDK32、旧 Java／Gradle 路径直接搬成 6.3 证据。 | [跨版本 SDK／NDK／JDK 表](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions) |
| Gradle／AGP 精确组合 | 6.3 内部补丁也改变组合。兼容表写 3.17–3.25 为 Gradle9.1.0／AGP9.0.0，而 3.25 发布页写 Gradle 升到9.3.1，两处存在粒度差异；候选冻结时须核实际随编辑器交付内容，不在此擅定组合。 | [兼容表](https://docs.unity3d.com/6000.3/Documentation/Manual/android-gradle-version-compatibility.html)、[3.25 发布说明](https://unity.com/releases/editor/whats-new/6000.3.25f1) |
| uGUI／TMP | 6.3 使用随编辑器固定的 uGUI；uGUI2.0 已合并 TMP。当前独立 TMP3.0.6、字体资源、材质及引用须核迁移；不能机械保留旧包锁，也不能因此重写玩法。中英文字体、图集、回退和真实页面检查仍需要。 | [uGUI 包说明](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ugui.html)、[2.0 变更](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/changelog/CHANGELOG.html) |
| 测试与旧证据 | 6.3 的 UTF 为随编辑器固定的核心包，官方链接至1.6；当前1.1.33测试发现、断言、结果XML及宿主测试需复核。没有依据声称全部业务测试须重写，也不能把旧绿测算新环境通过。 | [UTF 包说明](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.test-framework.html) |
| 升级指南中的实际接口风险 | 2022→6 的 Android UnityPlayer Java 类变化可能影响平台 SDK；6.2 的 UI Toolkit transform 弃用、6.3 更严格的 USS 解析可能影响保留的 Editor 工具。引擎升级不授权把 Editor UI 改成 uGUI。 | [6.0 指南](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html)、[6.2 指南](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity62.html)、[6.3 指南](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html) |

官方要求按中间版本顺序阅读升级指南，并核包兼容、代码和目标平台；API Updater 不等于项目验收。后续计划须覆盖 6.0→6.1→6.2→6.3，而不能只读目标版本新特性。[升级指南索引](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuides.html)、[升级准备](https://docs.unity.com/en-us/engine/6000.3/manual/upgrade-guides/upgrade-project)

## 6. 已选热更新框架与版本边界

| 主源事实 | 对本项目的约束 |
|---|---|
| HybridCLR 官方支持2022.3及6000.x；发布记录8.8.0加入6000.3，8.13.0明确合并到6000.3.21，8.15.0于2026-09-29调整 Installer 匹配策略。[支持表](https://www.hybridclr.cn/docs/basic/supportedplatformanduniyversion)、[官方发布记录](https://github.com/focus-creative-games/hybridclr_unity/blob/main/RELEASELOG.md) | “支持 Unity 6.3”不证明任一最新补丁已精确匹配；3.25不能直接当已通过，也不能因此断言不兼容。2022路线同样要固定编辑器、插件及 il2cpp_plus 对应分支／提交。 |
| HybridCLR 安装器基于本机编辑器 IL2CPP 文件与版本配置安装；构建步骤生成桥接、热更新程序集及补充元数据。[安装](https://www.hybridclr.cn/docs/basic/install)、[构建流程](https://www.hybridclr.cn/docs/basic/buildpipeline) | 换编辑器须重新建立对应工具链。AOT补充元数据应来自该 Player 实际构建，不能给旧包任意替换成新引擎生成物。[AOT 泛型说明](https://www.hybridclr.cn/docs/beginner/generic) |
| YooAsset 当前快速开始明确列2022.3与Unity6.0，未逐项列6.3。[官方兼容范围](https://www.yooasset.com/docs/guide-editor/QuickStart) | 保留已定 YooAsset；6.3精确版本需做加载、更新、缓存与回滚验证。页面没有写6.3既不是兼容证明，也不是不兼容证明。 |
| Unity 对旧 AssetBundle 在新版加载提供有条件的向后兼容；新版本构建的 Bundle 在旧 Player 的向前兼容不受支持，重大变化可能需重建。[AssetBundle 兼容边界](https://docs.unity.com/en-us/engine/6000.3/manual/assets-and-media/assets-managing-runtime/assetbundles-section/asset-bundles-intro) | 新 Player、Bundle、程序集和AOT元数据按版本配套发布；不能把6.3内容目录直接发给2022旧客户端。补丁升级也须检验，不预设所有资产必坏或必兼容。 |

**工程判断：**编辑器／原生 Player 升级应形成新的商店包基线，不应伪装成任意旧包都能接受的内容热更。引擎版本、资源版本和游戏存档 schema 是不同边界；版本迁移不授权清档或改变既定 Grant／Use、云冲突及旧局绑定规则。Google 对外部原生可执行代码及解释执行代码有不同条件，框架可运行不能替代 Play 分发合规判断。[Google 动态代码政策](https://support.google.com/googleplay/android-developer/answer/16559646?hl=en)

## 7. 三条路线的条件结论

| 路线 | 何时合理 | 必须接受的后果 |
|---|---|---|
| **现在迁移6.3：有条件倾向** | 预计维护到2027年、接受最低API25，并愿意在重做UI和集成SDK前投入一次独立版本验证；精确HybridCLR／YooAsset及必要SDK组合能够成立。 | 当前要承担编辑器、工具链、包、资源及证据重建成本。应先做有回退边界的版本适配，再迁移UI和装SDK，便于定位问题；本稿不授权执行这些步骤。 |
| **仅升2022.3后期补丁：真实备选** | 近期发行窗口紧、需要保留API22设备，或6.3出现具体且短期不可消除的兼容阻塞。先评估公开62f3，无需因API36／16KB默认购买扩展LTS。 | 获得已知平台基础和安全修复，但常规维护已结束；未来平台／SDK变化可能迫使再迁移。须在正式SDK及内容版本冻结前约定重新评估点，不能称为长期无成本路线。 |
| **暂不升级：限定开发阶段** | 当前主要做设计、配表、独立业务逻辑，尚不需要新平台构建，且需要先把产品与供应商要求定清。 | 可保留18f1减少眼前变量；不能据此冻结Play发行工具链。公开发行、SDK选型锁定或新原生包验证前仍须解决补丁／大版本选择。 |

我的推荐不是立刻把工程切到6.3，而是先确定“近期发布优先”还是“把2027年维护基线先立住”。后者成立时，6.3值得作为优先验证候选；前者成立时，62f3可能更合适。仅凭版本新旧、功能清单或现有4000余项旧测试，无法诚实给出无条件结论。

## 8. 尚缺信息与后续验证边界

- **用户可决定的取舍：**首发时间压力、预计持续维护多久、是否必须覆盖Android5.1–7.0，以及现有授权／可接受的维护预算。具体SDK兼容事实由工程调查，不让用户凭印象选版本。
- **工程待验：**冻结一个候选编辑器与完整包／SDK矩阵；检查API迁移和资源重导入；复用业务断言核测试发现与结果；验证uGUI双语言／字体／输入／生命周期；验证AAB生成APK、所有原生库16KB对齐及4KB／16KB设备行为。
- **版本待验：**HybridCLR真实IL2CPP构建与桥接、YooAsset冷启动／增量更新／回退、旧包隔离、存档和旧局连续性；需要新限定实施包、串行Unity证据及R独立审查。
- **不能从本研究推得：**迁移耗时、性能收益、当前APK已兼容、商店审核必过、特定扩展许可证可用、未来Google要求，或62f3已通过项目验收。当前所有运行验证均为 **NOT RUN**。

## 9. SD00 针对当前阶段的建议

SD00已实读本稿并核对本地包、程序集依赖、Android设置及冻结构建入口。**当前不建议立即迁移Unity6；优先保留2022.3路线，并在发行准备时验证公开62f3补丁候选。** 理由是已知API36／16KB要求有同系列路径，已定功能没有6.3独占依赖，而首Demo仍未完成设备/产品审查、uGUI迁移和正式SDK也尚未集成。

最初提出6.3的想法来自较长维护期，以及提前统一UI／SDK版本可能减少日后重复适配。这是合理的研究方向，但不足以在没有项目兼容结果时把大版本迁移列为必选。当前建议以减少近期变量为优先；保留旧系列也意味着接受常规支持结束后的维护代价。

若后续明确优先建立2027年维护基线，或确定的SDK／平台要求使2022.3路线不再合适，再以具体兼容矩阵评估6.3候选。**用户已接受当前建议：保留2022.3，发行准备先验证2022.3.62f3。** 本次仅完成源码静态与官方资料评估，没有进行安装或迁移实测。

用户随后选择Intel并要求兼顾Windows开发，按本题上下文记录为Mac Unity采用Intel版本；Windows采用同版本Windows编辑器，保留同一工程。只读确认现有外置2022.3.18f1可执行文件为x86_64、Mac主机为arm64。Mac编辑器的CPU架构不决定工程是否能在Windows开发；现有029工具中的Mac绝对路径仍需后续适配和实际验证。[官方宿主要求](https://docs.unity3d.com/2022.3/Documentation/Manual/system-requirements.html)。这项选择不改变Android ARM64构建目标，不代表新补丁已安装或双端工具已通过验收。
