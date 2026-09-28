# 029 Android APK 正式宿主与首 Demo 验收设计草案

2026-09-28 · SD00 · PREPARED_NOT_DISPATCHED。用户已授权目标；此稿用于提前具体化后继包，不授权越过 CONT-C 与 028 的独立接收门。最终文件白名单、冻结输入、预算和新证据根随正式包签发。用户最新明确最终交付为安卓APK，Mac仅作开发环境；旧Mac App/APK选择题撤回。此文件沿用准备期路径，内容以本次APK目标及任务包§401为准。

当前事实：Unity 2022.3.18f1；只有 Assets/Scenes/SampleScene.unity，EditorBuildSettings 场景数组为空；六份 first-release 已发布内容在 StreamingAssets/FightMatch。平台存储当前正在 CONT-C Mac 补充实施，不能先称通过。

首 Demo 的正式入口需要提供一个最小 FightMatch 场景和运行时宿主，挂载既有导航、028 战斗/结算界面、026 输入和 027 播放。发布同一工程的安卓APK，并在实际安卓环境安装、操作和关闭重开验收；Mac Editor仅作为同源开发与自动测试入口。Android物理存储、内容读取、触摸输入、构建和设备验收纳入029必需范围，产品状态、规则、保存、内容和页面继续复用。Mac可执行物或Editor绿测不能完成本目标。

## 启动与拥有者

宿主在 Unity 主线程创建并拥有既有 FightMatchDemoArchitecture.Interface，取得 CandidateApplicationSystem、PlayerSessionSystem、CandidateBattleApplicationSystem、CandidateLifecycleApplicationSystem。单一实例控制寿命，不修改 QFramework。页面重建不得 Deinit 并新建游戏业务；只有关闭该宿主时释放播放/页面事件、关闭架构。应用暂停、失焦或内存压力沿原播放跳至当前终态，不额外结算、退出或新建 Attempt。

六份 StreamingAssets 首包以固定文件名和有界读取交 FirstReleaseContentStorage.Create，再交原 PublishedContentCatalog 验证、GetCurrentBinding 和 ResolveExact；不读取 Assets/Tests、作者 source fixture 或历史 TestArtifacts 发布仓库来驱动产品。Android的StreamingAssets位于APK/JAR内，宿主以UnityWebRequest读取固定六件、核有界长度和原身份后交同一内容系统；不能把它当普通磁盘目录交File.ReadAllBytes。Mac开发路径可用本机读取，发布内容不因此复制成另一套。

存档根来自 Application.persistentDataPath 下专用产品子目录；真实绝对值须在 C 首次运行时记录，并先确认旧玩家数据存在性。不得把生产根硬编码成 TestArtifacts、固定 PlayerId 或隔离种子。默认不删旧档；发现既存数据按原 locator/恢复规则处理。最终包将精确授权实际Android应用数据根和新QA实物，并记录设备/API/包名/应用版本，不泛读其他应用数据；Mac开发档与Android真实验收档分别登记。

Android的Profile locator与PlayerSave需要通过既有IContentPublicationStorage/ILocalSaveStorage接口提供准确Android物理适配；Mac开发仍使用CONT-C的Mac适配。各自locator与profiles根分开，保存协议、UTF-16玩家身份派生、CreateNew、非覆盖发布、租约和原结果语义一致。Android适配必须在目标系统实证，不能拿Darwin原生调用/常量或Mac测试冒充Android支持。locator.Read=Absent只证明定位记录不存在；首次新建还须fresh核已授权产品根内没有遗留保存/候选/定位实物，允许的技术锁叶在正式包精确列出。Absent但有未归属实物时保全并阻塞，不生成新PlayerId。真正空根才显示明确新建意图；用户点新建时先持有唯一PreparedPlayerProfile和原观察，再CreateNew。失败/未知/页面重建只续原记录，不能再Prepare。CreateIntentRecorded 时显示继续原建档，调用 RecoverCreateIntent→ContinueCreate，复用原 CreateRecord。Active 时 OpenExisting；损坏/缺绑定/恢复阻塞如实显示，不能当新档静默初始化。只有locator已Active且PlayerSave当前头已核定后进入正常菜单。旧record1 F2先完成原schema2初始化与Active确认；旧Active schema2先续完活动局/S17/未决，再沿CONT-C显式2→3。新档schema3及旧档到schema3后的永久schema4迁移均独立显式确认，不自动改档。

SaveRecoveryCapabilities 使用 PlayerSessionSystem.RequiredRecoveryContracts/RequiredRecoveryFeatures 和本次真实 resolved publications 的 Content/Definition bindings 及版本，不能复制旧单 schema2 fixture。所有准备与操作由正式系统创建 ID、时间和熵，宿主只使用已有 LocalPlayerClock；不提供固定首通、随机结果或假到账。

## 页面与输入

导航 HostRequest 交028同一PlayerBattleSession/Controller，后者按正式契约重核并办理入场；029只路由页面，不重写草案或再创建入场请求。只在已提交入场后切战场，不每次强制准备页面。ResumeBattleRequested 读取原活动局，SettlementRequired 进入 028 原续办结算，RootBackRequested 显示准确退出应用含义，不变成退出战斗。

战斗页只复用 026/027/028；普通刷新不重复 Attach/RebuildLatest 丢失原办理引用。页面隐显、重复 HostRequest、迟到回调都有明确 epoch/owner 边界。地图/队伍/背包和结果返回后重读同一个已核头；完整底部导航、真实空配方、真实无下一关状态保留。UI 不显示技术实现术语作为普通玩家步骤；必要保存故障信息提供可执行的重试/确认入口。

最小白模应可读、可触摸，棋盘适应实际Android屏幕与安全区域，保留坐标原点对应，角色不默认自动选中。中文字体须在目标Android环境核实，不能依赖Mac本机字体；确需随APK分发字体时，在正式包列明来源、许可、精确资源及预算。最终实测检查字体、布局、触摸点击/拖线/短按、系统返回与前后台切换；UI Toolkit构造测试和Mac鼠标操作不能替代Android屏幕与输入证据。

## 执行与证据

正式 029 包将列出最小运行时宿主、场景、必要 PanelSettings、Editor 构建入口、程序集引用和自然 meta。优先复用现有程序集，确需 Editor-only asmdef 再精确列入。BuildPipeline显式传本场景，默认不改EditorBuildSettings；不更换依赖/Unity/渲染管线/产品身份。保持原 SampleScene，不覆盖已发布首包。

C唯一串行执行Unity。完成源码/静态后进行必要编译、宿主新测试及既定回归，再用BuildPipeline.BuildPlayer构建Android APK，并在指定安卓环境安装、启动与操作。记录BuildTarget、后端、ABI、SDK/API、NDK/JDK、包名、版本、签名证书指纹及最终APK哈希。构建成功、安装启动、正式玩法成功是不同证据门。首次验收后终止原应用进程并从同一PlayerSave冷启动读取；不能清数据、卸载或换包名来伪装重开。

体验矩阵至少包括：

1. 正常启动、新建/原建档恢复、地图与实际发布 L1 状态；进入队伍查看 W 与两个空槽，背包零或实际既有库存及真实配方空态。
2. 沿当前合法阵容入场；显式选人、物理拖线、敌我行动事实及当前已发布L1实际可达的消线/固化与阶段，正常胜利进入 WonPendingSettlement 再提交真实结算。补线/全倒补线只有在合法公开路径实际到达时记为验证，否则保留NOT VERIFIED，不制造快照、灌fixture或改正式内容来凑体验项。
3. 结算显示原 Settlement/BaseReward、经验和锡片实际增量，返回地图/队伍/背包核同一资料；没有下一关时不得伪造按钮。
4. 合法重打、原入场重来与明确退出各保持独立语义；原已验证回退/多项确认和默认参考沿 028 实际入口验收。
5. 同一次原结算重复查询/回调不发第二份奖励；关闭重开恢复同 PlayerId/初始化记录/已核头、经验、库存、首通与原结果。另在活动局正常关闭重开后续原 Attempt，不能自动开始另一局。
6. 保存失败/Unknown、对象重建及旧回调来自精确隔离故障测试；与正常可玩 App 的物理成功路径分开标注，不把故障 fixture 或手造 WPS 当试玩成功。

记录实际源码/首包/App/DLL/Git身份、开始结束/两个进程身份与实际退出、完整命令、具名测试与复用基线、UI截图/交互回执、只读业务见证、存档叶身份以及冷启动前后比较。冷启动核同一PlayerId/CreateRecord哈希/初始化commit/最新head；活动局另核Attempt/入场基线/随机状态/历史；结算局另核原operation/settlement/收据。页面或架构重建不能算两个进程冷启动，多项回退必须记录实际达到的行动路径。随机正常局必须沿真正已发布路线/输入和规则获胜，不能编辑血量、跳结算或灌入 fixture。必要界面修复后只重验受影响部分及源码变化要求的验证，保留已有效证据。

最终输出可安装的安卓APK、运行场景和简明安装/试玩说明，包含支持的Android/API/ABI、实际安装方式、触摸操作、存档位置、已实现范围及具体限制。R 在准确 C completed/formal/实际证据齐备后独立审查并给唯一 verdict；仅 ACCEPT 后 SD00 正常提交和推送、核远程版本，结束首 Demo Goal。Android构建依赖属于本次必需范围；广告、云服务和更多内容不自动扩入。

## 已核官方参考

- [Unity 2022.3 persistentDataPath](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-persistentDataPath.html)：路径由平台与产品身份决定，应记录本机返回值。
- [Unity 2022.3 Font assets](https://docs.unity3d.com/2022.3/Documentation/Manual/UIE-font-asset.html)：支持 Dynamic OS 字体；实际本机字形和运行时效果仍待验证。
- [Unity 2022.3 PanelSettings](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/UIElements.PanelSettings.html) 与 [BuildPlayer](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html)：宿主面板与构建只使用当前版本可用 API，不照搬 Unity 6 示例。

## Mac 开发机只读预检（历史事实，不是APK验收）

SD00已只读确认2022.3.18f1的Contents/PlaybackEngines/MacStandaloneSupport及Variations、构建程序存在；/usr/bin/codesign、CommandLineTools存在，xcode-select -p返回/Applications/Xcode.app/Contents/Developer。未执行Unity或构建，不把模块存在当可运行证明。

/Users/elliotyip/Library/Application Support/DefaultCompany/FightMatch存在且当前直接子项为空、不是symlink；unity.DefaultCompany.FightMatch位置不存在。实际运行仍须从Application.persistentDataPath读取并记录，而非只按文档猜路径。允许后继正式包精确选择专用locator与profiles子目录；新建的验证档保留用于正常冷启动和重复奖励核对，不为了截图/干净状态重置玩家资料。


## 联合验收的进程与证据矩阵（待正式包冻结）

正常结算后的资料恢复与活动局续局是两种不同见证。使用同一实际Android环境与真实资料依序运行三个不同的应用进程：P1完成正常建档/菜单/首场胜利/真实结算后正常退出；P2冷启动核原已结算资料，再依次合法重打、原基线重来、明确退出战斗、再次合法入场并完成至少一次非终局已提交行动，取得活动局保存点后正常退出；P3首次输入前核原活动局，再执行真实下一行动。这样两次冷启动各有前后进程与保存点对应，不把同一进程重建页面/架构算重开。具体路径与具名文件在029正式包按前序接口冻结。

| 编号 | 玩家路径与观察点 | 最少直接证据 |
| --- | --- | --- |
| M01 | 真正空根明确新建或既存根按原locator续办；唯一CreateRecord/初始commit核定后进入正常地图。F2/遗留未归属实物不当新档。 | 实际persistentDataPath、启动前有界目录清单、UI操作、locator/PlayerId/CreateRecord身份与已核head；故障分支另用隔离测试。 |
| M02 | 地图→队伍→背包→地图，正式L1/真实W与两空槽/真实库存及配方空态准确可读；中文、棋盘与按钮无裁切、覆盖或假按钮。 | 实际Android Player屏幕与具名交互回执、各页同一只读head；不能只用VisualElement构造测试。 |
| M03 | 当前合法阵容入场，显式选人、物理拖线/短按、敌我行动及当前L1实际可达阶段；参考可读、条件准确、首拖关闭且不吞输入。 | 实际Android触摸/按键或对应操作记录、真实输入接受结果、Attempt/scene/history/随机状态的前后只读见证；不调用测试入口代替物理输入。 |
| M04 | 正常胜利先WPS、完成/明确跳过最后播放，再原H06到账；准确展示20基础XP与2锡片等原收据，不由库存差额猜结果。 | WPS与结算两次原提交身份、ReservedOperation/settlement及原奖励/经验/库存/End记录、页面屏幕；等级/剩余经验按真实记录，不套旧算例。 |
| M05 | 重复结果/回调/原操作查询不再次发奖，返回地图/队伍/背包读取同一资料；无下一关准确结束内容段。 | 原operation/settlement/receipt身份保持，重复请求前后业务head/奖励记账的直接比较和实际返回页面。 |
| M06 | P1正常退出，P2重新启动读同一结算后资料，首通/成长/库存/原结果保持。 | P1/P2实际PID、起止/退出及启动命令；同一PlayerId/CreateRecord/初始commit和对应head、原结算/收据与保存叶身份。 |
| M07 | P2合法重打→原基线重来→明确退出→再次合法入场→至少一次非终局已提交行动→正常关闭；P3首次输入前核原状态，再续原Attempt执行下一行动。 | 实际操作序列与确认、活动保存点；P2/P3两PID及真实终态/启动，Attempt/Challenge/EntryBaseline/RNG/history逐项前后比较；不新H02伪装续局。 |
| M08 | 单项/多项回退及真实默认参考经028正式界面操作；保存失败/Unknown及对象重建另按精确隔离故障验证。 | 回退实际到达的历史与确认range；参考前后业务零写/真实token不变；故障测试具名结果与正常Player见证分开。 |
| M09 | 当前正式L1不能合法到达的补线、全倒免费补线或其他条件，保留准确NOT VERIFIED，不注入快照、改HP/RNG/内容来凑路径。 | 实际尝试路径与不可达/未测边界，明确区分隔离测试覆盖与正式可玩内容覆盖。 |
| M10 | 交付可安装并实际运行的同源安卓APK、场景/安装试玩说明、独立R结论及已正常推送Git版本。 | 最终源码/首包/构建身份、Android BuildPlayer结果、APK哈希/签名/包名/版本/ABI与实际设备安装身份、说明文件、R准确completed+唯一ACCEPT、Git本地/远端commit核对。 |

所有运行日志中的业务见证只能只读查询既有负责者，不能新增旁路写存档/初始化固定ID或熵/自动胜利的验收模式。新UI或构建后必须运行实际最终App；如修正影响某部分，保留无关有效见证，追加受影响的真实验证，并保持来源版本可追溯。上述仍是准备稿，不授权越过CONT-C/028独立接收。

只读构建预检补充：当前MacStandaloneSupport确有x64、arm64和x64arm64的Mono/IL2CPP开发与非开发变体；这仅是模块存在性，最终架构/后端必须由真实构建记录确定，不宣称多架构都已验证。FightMatch.Platform与Presentation的asmdef没有平台include/exclude限制；Platform无Engine引用。当前PlayerSettings为DefaultCompany/FightMatch、1920×1080、resizableWindow=0、fullscreenMode=1，未在准备阶段更改。

仅在确有必要的Mac开发预览中，可按[Unity 2022.3 Standalone参数](https://docs.unity3d.com/2022.3/Documentation/Manual/PlayerCommandLineArguments.html)使用-screen-fullscreen 0、受支持的-screen-width/-screen-height及精确-logFile覆盖本次窗口和日志；真实截图记录实际尺寸。这些Standalone参数不是Android安装或验收命令，Mac预览不作为本目标完成证据。Android须记录实际安装/启动命令、设备屏幕与输入行为。


R可执行性咨询收敛（turn 01a0e5de-a14e-7bc3-adee-e93189a36cde，completedAt1790563600）：029宿主在导航原操作查询到CommittedResult后，核其原kind/operation与当前lookup头，再交028新增的只读原收据入口；正常页面必须能明确打开原战斗结果，不能只展示operation/commit编号或凭内存保存上次结果。冷重开用例必须从已核Record.Intent→QueryOperation取得完整OriginalLookup；没有原记录则如实显示缺失，不能再结算补造。这个桥接放在029宿主及028新session/controller中，不隐式扩大CONT-C导航旧文件范围。

三Player进程分别证明已结算资料恢复与非终局活动保存点恢复。WPS/S17的持久化后完整应用对象重建由028具名测试覆盖；未额外真实关闭/重开Player停在WPS时，交付明确该WPS进程冷启动分支NOT VERIFIED，不与上述两项混称。最终029白名单还必须列出只读业务见证、宿主、构建入口、必要资源和直接测试的精确路径/预算，不能靠通配目录临时扩写。


## Android平台与外置盘环境准备约束

用户已明确APK为最终产物，Android平台适配不再属于排除项。029需精确签出Android存储/内容读取/宿主/构建/设备见证白名单及预算；原Core、业务规则、保存字节与已发布六件仍受保护。Android路径/原生租约/持久化能力、IL2CPP或Mono真实构建、反射与裁剪行为均按实际目标验证，不能把Mac同源编译或模拟存储通过写成Android已通过。原已接收Windows/Mac证据继续有效用于对应范围，不重复当热身。

主线最新只读ProjectSettings事实：applicationIdentifier与scriptingBackend均空映射，AndroidTargetArchitectures=1，MinSdk=22，TargetSdk=0，bundleVersion=0.1、versionCode=1，当前尚非已验证Android构建配置。正式包必须冻结一个持续沿用的产品包名、必要ABI/后端/API与APK模式；不得因“Demo”另外创造业务或存档格式，不能在安装后随意改包名/签名导致原数据无法续用。最小必需ProjectSettings/构建设置例外须逐字段列明；不升级Unity、不升级依赖或增加无关Android服务。

用户已指定iQOO Neo5真机作为最终APK验收设备，并明确同时需要模拟器。先复用本机已安装的模拟器/AVD；必要的模拟器、镜像及AVD数据优先外置盘。实际手机系统/API/ABI与连接授权状态在设备接入后读取，不能按型号上市参数推断。模拟器用于辅助开发/预检，不能代替iQOO Neo5最终真机证据。

设备见证必须记录具体设备/系统/API/ABI、安装的APK哈希与包名/版本。P1/P2/P3使用同一安装和同一数据根，实际PID/退出/冷启动可追溯；禁止clear data、卸载重装、注入存档或重新生成PlayerId来证明恢复。用原应用的正常退出/系统生命周期操作与实际重启建立证据；若当前只能运行模拟器，应准确标注其环境及真机仍未验证，继续独立可做部分并保留最终真机验收门。待环境盘点确认实际设备通路后再冻结具体步骤，不以缺少设备阻断独立可做的源码、构建及审查。

Android环境准备由新增独立会话FightMatch Android 环境准备负责，thread=01a0e5f8-216e-7a01-a649-5799010e2c8f、host=local、gpt-5.6-sol/high。该项是用户对原仅C/R限制的单项例外，环境会话不改产品/Packages/ProjectSettings/Git，不执行FightMatch Unity验证。按用户本会话最新要求，模块、SDK/NDK/JDK、下载暂存、解包及大型Gradle/设备镜像缓存优先放外置盘/Volumes/WD_BLACK_SN7100_2TB_Media；先核下载+解包+安装+后续构建的峰值空间，内置仅保留必要小型配置，不清理用户文件。下载可隔离进行；修改当前Unity安装、Unity Preferences、切目标平台或启动Unity须与C明确串行交接。

官方依据：[Android StreamingAssets读取](https://docs.unity3d.com/2022.3/Documentation/Manual/StreamingAssets.html)、[persistentDataPath及包身份](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Application-persistentDataPath.html)、[Unity 2022.3 Android环境](https://docs.unity3d.com/2022.3/Documentation/Manual/android-sdksetup.html)。文档规定不等于设备已验证；实际安装工具版本和结果由环境/构建证据落定。


## Android只读设计咨询已核差异（仍未派发）

R主咨询turn 01a0e5ff-e5ed-76f3-8cd4-d91d373ba1d3已completed/error=null（1790565148～1790565955）；设备补充turn 01a0e60d-a76a-7343-8927-290b92dc27e8已completed（1790566049～1790566077），其中“不装模拟器”又被用户最新要求及turn 01a0e60e-a8ca-7c02-8f21-55ab762526d7取代，后者已completed/error=null（1790566115～1790566144）。以上均为只读设计建议，不是CONT-C或029独立产品ACCEPT。

准备范围收敛如下，正式包须补准确预算/测试路径/构建与设备操作计划后才授权实施：

- 拟新增Platform/AndroidLocalSaveStorage.cs、Platform/AndroidContentPublicationStorage.cs及Host/FightMatchStreamingAssetsLoader.cs（均在Assets/Scripts/FightMatch下，另自然meta）。前两者复用原两个存储接口，原生调用与路径小辅助留在对应存储文件；Platform继续无UnityEngine引用，Unity API和平台选择在宿主。原Core、marker枚举值/字节与六份发布内容不变。Android目标验证不可直接挪用仅Editor的旧测试程序集。
- Android IL2CPP租约须独立通过Bionic open取得并持有fd，再flock非阻塞、解锁及close；不能把FileStream.SafeFileHandle指针转整数。NDK对应常量、errno和签名按实际ABI核定；只有真实锁争用映射Busy，其余异常保留。存储实例必须在所有写入期间持有同一进程原生租约，不能只依靠FileShare.None。
- CreateWork保持CreateNew；Flush只能接受本实例创建的流并执行Flush(true)，失败上抛；提升保持同目录、原文件种类与commit，冲突保留双方。本机2022.3.18f1的IL2CPP File.Move是先stat再rename；如复用，只在所有合作写者持同一原生租约的明确模型内承诺无覆盖，不把它称内核无覆盖。若确需renameat2或原生shim，先按MinSdk/ABI另列精确范围，不能静默增加依赖或提高系统下限。
- 保持EditorProcessCrash=1历史存储标识及同等进程崩溃恢复语义，不改PowerLossDurable；SaveFailed/CommitUnknown继续由原LocalSaveStore判定。Android根记录系统返回值及规范化可信根，允许系统根合法别名，再拒绝产品子树越界、末端/悬空链接。外部卷不可用、权限失败及未知I/O均不得当Absent，不能换根静默新建；locator/profiles分根和原UTF-16码元哈希保持。
- StreamingAssets读取固定六件，在接收时同时限制每件和总字节数，不是全部缓冲后再查大小；成功交原Create/Catalog，失败不回退测试内容。实际目标APK必须完成原六件解码、规范再编码、指纹及参考核验，以覆盖现有反射/Activator/AOT路径。首目标建议设备支持的ARM64/IL2CPP与Minimal裁剪；确有漏项再签精确link.xml，不默认保留全程序集。模拟器与手机分别记录实际API/ABI/页大小/后端；若同ARM64可复用同一APK，不无故加多个ABI或后端。
- Android存储QA与正式玩法验收分开：隔离授权QA根核实际跨进程租约争用和死亡释放、CreateNew/提升冲突、Flush失败与非缺失错误，以及原协议已列持久化边界的恢复。跨进程必须真实两PID，提前核可用的同UID/run-as通路，两个C#实例不算。仅保存协议的隔离验证可以使用精确测试数据；最终玩家流程仍只允许正式入口和正常输入，不加旁路写档/假结算。
- 最终P1/P2/P3仍在iQOO Neo5同安装/同数据根，记录旧PID消失和新PID。系统返回桌面不能冒充进程退出；若需要am force-stop，注明系统强停冷启，与正常退出分列。模拟器预检证据独立登记，不能代签手机路径、存储、触摸和恢复；不升级Unity来掩盖实际设备不兼容。

SD00直接复读本机IL2CPP os/Posix/File.cpp的MoveFile及icalls/mscorlib/System.IO/MonoIO.cpp的Open，与R指明语义吻合。官方依据：[Android Player设置/IL2CPP](https://docs.unity3d.com/2022.3/Documentation/Manual/class-PlayerSettingsAndroid.html)、[Android应用专用存储可用性](https://developer.android.com/training/data-storage/app-specific)、[Unity裁剪](https://docs.unity3d.com/2022.3/Documentation/Manual/ManagedCodeStripping.html)。实际设备API/ABI/页大小、原生租约/Flush及内容AOT均待真实运行，不预报通过。


环境已准备（§410）：2022.3.18f1配套AndroidPlayer/SDK31与32/Build Tools32.0.0/Platform Tools32.0.0/NDKr23b/OpenJDK11/Gradle7.2已在外置盘。现有BlueStacks Air5.21.735.7518已通过adb启动核验，Android13/API33/arm64-v8a；adb连接emulator-5554及localhost:5555为同一实例。已退出，无需补装模拟器。Android构建时显式设置外置GRADLE_USER_HOME与ANDROID_USER_HOME；根为/Volumes/WD_BLACK_SN7100_2TB_Media/application/Unity/AndroidBuildCache/FightMatch下对应目录。当前iQOO Neo5未连接，最终手机参数另读；模拟器模型SM-G998B不是验收手机身份。环境就绪不等于实际APK/存储/触摸/玩法通过。


六件加载预算具体化：现有FirstReleaseContentStorage.Create对每件上限为min(16MiB,budget.MaxRecordBytes)，合计32MiB，且会克隆输入；宿主接收器应在每次ReceiveData增长前以同一上限核每件/总量，失败即停止该次启动并给可理解的重试状态，不能等全部分配完才判断。当前受保护首包实际仅38452bytes：source20443、package11486、validation664、review4766、publish682、release411；这些长度用于绑定当前验收内容身份，不改变通用内容存储协议或另建Demo专用格式。正式源码仍交原Catalog核SHA/规范编码/发布身份，不以“文件都存在”代替准入。


中文显示准备：当前Assets中未发现ttf/otf/ttc或既存FightMatch字体资产；不能把Mac Editor中文显示推定为Android可读。029正式资源白名单需给一个确切方案：优先核目标系统可用字体与Dynamic OS行为；若不能稳定覆盖两目标环境的菜单/说明/结果文本，使用未修改的Noto Sans CJK SC Regular官方字体及随附SIL OFL1.1许可，来源分别为https://github.com/notofonts/noto-cjk/tree/main/Sans/OTF/SimplifiedChinese与https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE。正式下载前冻结准确tag/commit、文件路径、字节/SHA和许可副本，不取/打包Mac系统字体。字体原文件、Unity生成的TextCore FontAsset/必要子资源及自然meta必须逐项列入正式白名单/预算；许可证须随交付可查看。此处尚未下载、添加资源或指定新的软件依赖。

根据Unity2022.3字体文档，Dynamic OS依赖目标操作系统现有字体，而Dynamic使用随包字体源；最终用实际Android渲染检查地图/队伍/背包、战斗提示、原结算收据与恢复提示，逐页核中文缺字和溢出。只有源文件存在或FontAsset生成成功不能代替目标截图/交互证据。


## Android可执行验证方案（R咨询收敛，仍未派发）

R准确turn01a0e64c-ddca-70d3-8bb1-ffc0117d78e2已completed/error=null，1790570192～1790570852；正式回执msg_0d29bf8d726d8dc0016ab9f124141c87d08caa9d945dd08ba3。以下是最小任务包候选，仍待028实际接口/基线与精确预算冻结，不把咨询当产品ACCEPT。

在已拟两Android存储文件和StreamingAssets loader外，新增范围建议列全：

- Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef
- Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs
- Assets/Scripts/FightMatch/Host/Editor/FightMatch.Host.Editor.asmdef
- Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs
- Assets/Tests/Android/FightMatch.Android.Tests.asmdef
- Assets/Tests/Android/AndroidStoragePlayerTests.cs
- Assets/Tests/Android/AndroidContentPlayerTests.cs
- Assets/Tests/Android/AndroidQaRun.cs
- Assets/Scenes/FightMatchDemo.unity
- Assets/UI/FightMatch/FightMatchPanelSettings.asset
- Tools/FM029AndroidTestSettings.json

Assets项目及新目录另逐项列自然meta；字体方案及宿主直接EditMode测试也须补准确路径/预算后再签，不能以上述“候选”概括无限写权。Host单向引用Application/Presentation/Content/Platform/Core/QFramework，旧程序集不反向引用Host。Editor构建程序集仅Editor；目标测试程序集使用TestAssemblies/UnityEngine.TestRunner及所测生产程序集，不设Editor-only。Android用例按实际平台限定，不把排除的平台测试写成Mac已覆盖；既有EditMode具名集合保持。

本机UTF1.1.33确认支持-testPlatform Android；PlayMode本身仍在Editor。测试设置固定候选为{"scriptingBackend":"IL2CPP","architecture":"ARM64","androidBuildAppBundle":false}，architecture按实际SettingsMap<string>传字符串，不用数字2。唯一验证PS1拟封装batchmode/projectPath/buildTarget Android/runTests/testPlatform Android/assemblyNames FightMatch.Android.Tests/testSettingsFile/buildPlayerPath/logFile，实际所有绝对路径、设备标识及证据槽先冻结；不使用quit或runSynchronously。

QA构建需明确区分框架参数-doNotReportTestResultsBackToEditor和本包拟新增参数-fm029QaBuildOnly。后者由同一Editor构建文件中的ITestPlayerBuildModifier在严格核本stage/Android/目标测试程序集/证据输出路径后才生效；移除AutoRunPlayer/ConnectToHost/WaitForPlayerConnection，保留Development和IncludeTestAssemblies，实际成功构建后才退出，不无条件Exit(0)。modifier全局可发现，不能只靠assemblyNames防止误作用旧测试。设备TestRunCallback保存完整ToXml(true)结果后由C取回；正式APK不包含测试程序集和QA故障控制。

UTF本身临时修改测试包名、关闭engine stripping，并自然生成/清理一份Assets/InitTestScene{ticks}.unity及meta。正式包须精确授权这个工具生成物的唯一模式、每次最多一对、实际路径/长度/SHA/创建与清理证据，以及临时PlayerSettings逐字段快照/恢复；不扩成Assets任意写入，不把工具临时包身份当正式产品。Android QA Player的成功也不能证明正式Minimal裁剪APK的反射/AOT无缺口。

双PID首选实际设备run-as与toybox flock：QA包须debuggable，证书可以是持续保留的签名，并非必须debug证书。同UID shell打开固定锁叶至fd9、toybox flock -xn 9；记录UID/PID/锁路径，维持原shell/fd。不能套用桌面flock“文件名＋命令”语法。分别证明Player持锁时另一PID拒绝、另一PID持锁时Player返回Busy，以及正常释放和终止持锁进程后的重取。两设备上通路均待真实前置探针；若run-as/SELinux/toybox不可用，只据具体失败补最小方案，不预装服务框架。Android10起不能假定可执行应用可写目录中的原生helper。

AndroidQaRun只认固定case/phase与合法run ID；根固定QA包persistentDataPath下，拒绝任意外部路径。控制/XML/阶段见证与受测存储根分开。测试程序集的私有接口包装器只在签准snapshot提升后、marker发布前/发布后响应前、locator Create/Active确认边界截断，真实I/O仍委托生产Android适配。用同一QA APK/同一根、新PID恢复原操作；被故意终止的阶段没有XML不能算通过，必须有终止点见证＋实际进程终态＋后续恢复完整结果，区分异常注入与真实进程终止。正常宿主没有这些控制入口。

正式构建建议采用com.yyczz1.fightmatch、ARM64/IL2CPP、Minimal裁剪、已安装TargetSdk32，保留MinSdk22及当前版本号；包名是工程建议，正式签发前核无冲突既存安装，签名密钥长期保留于Git外。ProjectSettings.asset只允许逐字段明确的Android标识/后端/ABI/裁剪/目标SDK及必要签名引用，秘密不写进Git。BuildPipeline.BuildPlayer显式传FightMatchDemo场景即可，故不必为了构建改EditorBuildSettings；此前“允许场景登记”收窄为默认不改，确需再说明。外置Gradle/Android缓存变量每次显式带入唯一验证入口，不能仅靠文件夹已经创建。

执行顺序：源码及静态完成→必要Mac编译/既定完整EditMode→一次构建QA APK和正式APK→BlueStacks目标小型存储矩阵/内容AOT/页面预检→手机连接后复用同一两APK，在实际手机文件系统重验相关小矩阵，并完成正式APK真实触摸/生命周期/P1→P2→P3。不因换设备重跑无关完整EditMode；正式APK本身须核六件/AOT与Minimal裁剪，不能用测试程序集掩盖保留缺口。不清数据、卸载或更换签名/包名伪装冷启动。

依据：[UTF1.1命令行](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html)、[TestPlayerBuildModifier](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-attribute-testplayerbuildmodifier.html)、[AOSP run-as](https://android.googlesource.com/platform/system/core/+/master/run-as/run-as.cpp)、[Toybox help](https://android.googlesource.com/platform/external/toybox/+/refs/heads/main/android/linux/generated/help.h)、[Android10可执行文件限制](https://developer.android.com/about/versions/10/behavior-changes-10)。尚未运行设备探针/QA/正式APK；具体阻塞如实回报，不用文档支持冒充设备已验证。

字体候选来源已进一步固定（资源仍未导入）：官方Sans2.004标签指向commit523d033d6cb47f4a80c58a35753646f5c3608a78。SD00只读HTTPS完整流校验该提交的Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf，实际HTTP200、16437364bytes、SHA2562c76254f6fc379fddfce0a7e84fb5385bb135d3e399294f6eeb6680d0365b74b、OTTO头；未落盘、未安装、未改变Assets。该固定提交的许可实际在仓库根LICENSE，HTTP读取实际exit0、4301bytes/SHA2566a73f9541c2de74158c0e7cf6b0a58ef774f5a780bf191f2d7ec9cc53efe2bf2，为OFL1.1；先前同提交Sans/LICENSE的404保留为路径核查事实，不再是未决。固定许可来源https://raw.githubusercontent.com/notofonts/noto-cjk/523d033d6cb47f4a80c58a35753646f5c3608a78/LICENSE。官方版本来源https://github.com/notofonts/noto-cjk/tree/Sans2.004，固定字体来源https://raw.githubusercontent.com/notofonts/noto-cjk/523d033d6cb47f4a80c58a35753646f5c3608a78/Sans/OTF/SimplifiedChinese/NotoSansCJKsc-Regular.otf。后继C按正式资源白名单下载到外置项目并核精确身份，目标中文仍以Android实际渲染为准。

## 最小宿主与直接测试范围收敛（仍未派发）

R只读咨询turn01a0e66b-3294-7cb2-b9dc-e9bb96293b60于1790572679 completed/error=null，startedAt1790572180、duration499130ms，正式msg_0d29bf8d726d8dc0016ab9f843f9f887d091eaf86e74f69e79。未运行测试/Unity或修改资源。本段具体化新宿主范围；028尚未实现的接口仍以接收后的真实公共签名为前置，不是现有能力或产品ACCEPT。

实际现有类型是LocalPlayerProfileLocator；不引入虚构LocalPlayerProfileStore。架构通过FightMatchDemoArchitecture.Interface.GetSystem<T>()取得，无需旧业务友元/新公开构造器。正式新档使用现有PrepareNewRosterProfile，经已核当前首包建立阵容格式资料；旧schema2资料仍先走原恢复再明确迁移，不自动替换。Absent只表示两份定位记录未见，宿主仍须有界检查产品根；除精确允许的技术锁叶和已知空目录外，只要有实物就保全并阻塞新建，读/权限失败也不能转Absent。明确创建fresh重核后仅Prepare一次，立即保留原对象再CreateNew，重复点击/重入/页面重绑维持同对象。CreateIntentRecorded走RecoverCreateIntent→ContinueCreate；写结果未知不重新Prepare。Active走OpenExisting；已打开后恢复走Restore，不重复OpenExisting或初始化；正常页面同时要求Active和当前已核PlayerSave。

拟追加/收敛的精确文件（路径相对仓库，预算为最终物理行上限，正式包核完前序再冻结）：

| 文件 | 上限及职责 |
| --- | --- |
| Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef | 40；单向引用现有生产程序集 |
| Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs | 240；Mono主线程加载/平台组合/UIDocument/安全区/字体与生命周期 |
| Assets/Scripts/FightMatch/Host/FightMatchHostSession.cs | 520；有界根观察、创建/F2/打开、单一拥有者、路由及原结果桥接 |
| Assets/Scripts/FightMatch/Host/FightMatchHostView.cs | 220；启动/恢复按钮和页面容器，仅管理自己订阅 |
| Assets/Scripts/FightMatch/Host/FightMatchStreamingAssetsLoader.cs | 180；固定六件有界异步读取 |
| Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef | 45；Editor-only/TestAssemblies，单向引用Host和所用生产程序集 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs | 300；正式六件与真实隔离根、现有接口的委托故障包装 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs | 320；创建/F2/遗留实物/Active恢复 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostRoutingLifetimeTests.cs | 380；重复/旧路由、原收据、整树重绑与关闭 |
| Assets/Tests/EditMode/FightMatchHost/FightMatchHostResourceTests.cs | 140；场景/PanelSettings/字体/主题连接 |

上述Host新增四cs总≤1160行、直接测试四cs总≤1140行；预算提供平台连接余量，不要求写满。此前独立Host.Editor构建程序集与Android QA程序集保持；所有新目录及文件自然meta须正式逐项列出，不概括为目录通配。

HostSession方法只涵盖观察启动、明确创建、继续原创建、处理本导航请求、打开原结果和最终释放。构造仅接真实catalog、固定绝对根、现有存储接口或按PlayerId工厂；不接任意已核头/builder/信任布尔。HostSession持有保存的唯一架构引用、导航controller与未来028 battle controller，input/playback继续由028组合；重复宿主拒绝接管，不得Deinit别人的实例。普通detach仅解绑页面；最后关闭才依序释放controller和保存的架构引用一次。暂停/失焦沿原播放结束行为，不关闭业务架构。

导航Changed先于HostRequested，宿主不得在Changed中无条件Refresh造成重入；只在真正进入CommittedResult且原kind确属战斗结果时交028指定operation的只读入口，Return后仍保留的历史Result不得自动重开。每次明确再次查看同一原收据应允许，不能用永久operation黑名单压掉合法查看。HostRequest在交028前作有界去重，028的Application内核验internal navigation owner/revision/head，Host不要求公开owner。真正H02/续局/结算仍归028。

直接测试通过正式公共路径自建catalog和Mac隔离根，不引用旧Core.Tests的internal夹具，不加旧业务友元。涉及静态架构的用例串行且只释放自己的拥有者。必须覆盖重复/同步重入Create、页面重绑后原创建对象、记录后未初始化及初始化后未Active的F2、Absent但有遗留实物、现有Locator恢复、重复/过期HostRequest零新入场、完整对象重建后原收据、SaveFailed/Unknown整树重绑保持原request、detach不改变原播放token/完成次数。后四项等待028真实公开入口，不以伪造成功对象充数。

字体方案现在收敛为随包固定Noto Sans CJK SC Regular 2.004＋内置TextCore Dynamic FontAsset，以免把未连接手机的系统字体存在当作前提；不新增TMP或其他包。精确资源候选：Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular.otf（上方固定16437364bytes/SHA）、NotoSansCJKsc-Regular.asset（≤8MiB，材质/图集优先内嵌子资源，不另外放任意叶）、OFL.txt（固定4301bytes/SHA）及自然meta。该段收敛替代上文尚在探索的Dynamic OS优先选择。FontAsset保持源字体引用，使用Dynamic；宿主页面根设置FontDefinition.FromSDFFont，场景正常引用FontAsset/PanelSettings。许可TextAsset由场景/宿主引用并提供简短可查看入口，确保随APK分发；不用新增保存格式。

正常运行主题需显式连入PanelSettings，若使用新文件，精确候选为Assets/UI/FightMatch/FightMatchTheme.tss（≤4KiB，仅正常默认运行主题导入/必要适配）及自然meta。场景≤256KiB，PanelSettings≤64KiB。C可在正式签准的单次串行资源准备阶段用Unity正常API生成场景/FontAsset/内嵌材质图集，再冻结实际资源/meta身份进入Compile/测试；这一必要资源生成不是已冻结版验证，不得混写为测试通过。测试中文字形查询不把动态运行图集保存回已冻结源资产。Android正式渲染、中文缺字/溢出/安全区/触摸仍须实测。

正式启动与试玩文档候选路径固定为docs/demo/PLAY_FIRST_DEMO.md（≤220行，029正式白名单签发后由C依实际成品编写）；根START_HERE在最终接收后链接它。内容只写已实现的正常玩家入口与实际版本：APK位置/哈希/签名与包名、现有BlueStacks和iQOO设备安装/启动、创建或继续已有资料、地图/队伍/背包、入场与基础触摸、合法胜利后待结算→原奖励到账→返回/重打/Restart/Exit、同资料关闭重开、真实空配方/无下一关边界及保存恢复按钮含义。开发者附录可列对应现有外置Unity/SDK与实际构建命令；不要求普通玩家操作TestArtifacts/修改数据。不得在成品未产生时填假APK路径/通过数字。持久签名私钥及口令不进入指南或Git，仅公开证书指纹；最终同一APK/签名在模拟器与手机复用，既有正式应用数据不清除。直接触控与系统注入正常输入的证据分别说明，不能把ADB命令成功当成画面/业务结果已验收。


## 剩余新增文件预算与资源成员（仍未派发）

本段补齐029草案尚无准确预算的已拟文件，正式实施仍等待028的R ACCEPT和实际公共接口；不授权当前C创建这些文件。预算按清晰正常排版的最终物理行计，不要求写满，不以压缩多条语句规避。已有Host四cs和EditMode四cs预算保持。

| 已拟新增文件 | 最终上限与边界 |
| --- | --- |
| Assets/Scripts/FightMatch/Platform/AndroidLocalSaveStorage.cs | 420行；原保存接口、真实Bionic租约、路径/流所有权/提升和已有故障语义，仅该适配所需私有辅助 |
| Assets/Scripts/FightMatch/Platform/AndroidContentPublicationStorage.cs | 220行；原内容存储接口及真实Android隔离目录，不引用UnityEngine、不改正式内容字节 |
| Assets/Scripts/FightMatch/Host/Editor/FightMatch.Host.Editor.asmdef | 40行；Editor-only构建/资源准备，只引用必要既有程序集及UTF公开Editor入口 |
| Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs | 440行；串行明确资源准备、正式BuildPipeline入口、受精确参数约束的QA modifier及临时设置恢复，不在运行时程序集 |
| Assets/Tests/Android/FightMatch.Android.Tests.asmdef | 50行；TestAssemblies、所测生产程序集和必要TestRunner，保留正式APK排除测试的语义 |
| Assets/Tests/Android/AndroidStoragePlayerTests.cs | 520行；真实适配接口、进程边界见证与固定隔离保存用例，不复制业务实现 |
| Assets/Tests/Android/AndroidContentPlayerTests.cs | 220行；实际六件接收/内容准入/AOT及有限错误路径，不制造第二套Demo内容 |
| Assets/Tests/Android/AndroidQaRun.cs | 320行；固定case/phase/run ID控制、原操作恢复阶段及完整结果输出，全部仅QA程序集可达 |
| Tools/FM029AndroidTestSettings.json | 12行/2KiB；IL2CPP、ARM64、APK三个既定UTF设置，无运行秘密 |

上述Android生产适配两cs总≤640行；Editor构建cs≤440；Android测试三cs总≤1060。正式包应按当前UTF公开接口编译核定测试平台过滤，不能让Android专用QA假装成Mac已运行用例；不得为省事改旧测试程序集依赖或加入业务友元。测试夹具用现有公开接口调用真实生产流程，故障包装保持真实I/O委托。

当前已核目录：Assets/Scenes及其meta已经存在，必须保持原meta；Host、Host/Editor、Tests/Android、Tests/EditMode/FightMatchHost、UI、UI/FightMatch和UI/FightMatch/Fonts及其目录meta尚不存在。拟新增目录meta精确七项：Assets/Scripts/FightMatch/Host.meta、Assets/Scripts/FightMatch/Host/Editor.meta、Assets/Tests/Android.meta、Assets/Tests/EditMode/FightMatchHost.meta、Assets/UI.meta、Assets/UI/FightMatch.meta、Assets/UI/FightMatch/Fonts.meta。每项是Unity自然目录meta，不借此授权其他路径。

合并上文所有候选，Assets中预期24个新叶（14cs、4asmdef、1场景、PanelSettings和FontAsset各1、主题1、原字体1、许可1），各有一个自然文件meta，再加七个目录meta，共55个新Assets成员。正式签发必须展开全部相对路径并核前序没有冲突；不得直接按55数字推导未知新清单。普通cs/asmdef/meta/TSS与序列资源区分预算，FontAsset材质/图集优先内嵌，不自动新增独立材质/纹理。所有旧Assets（包括Scenes目录meta）、Packages与旧公开API默认保留；必要ProjectSettings字段、唯一验证工具和本包三份交付/范围/审查报告另逐项签准。

本机现有.gitignore已排除Build/Builds、TestArtifacts、*.apk与*.aab，所以029无需为了本地APK改忽略规则或把构建缓存强制纳入Git。最终正常APK在明确外置输出路径交付并记录字节/SHA/公开证书指纹；Git正常推送承载源码、合法资源、指南和可核审查/验收记录。设备原始玩家存档、签名私钥/口令、本机权限及大型缓存不进入版本提交。该段没有新增打包/发布GitHub Release操作授权，也不以代码push代替APK可运行证据。


## 构建目标与同版证据边界（仍未派发）

Unity2022.3的-buildTarget决定启动时的活动构建目标，UNITY_ANDROID等符号又影响条件编译；据此，029的Mac EditMode、Android QA构建和正式APK应分别记录目标及产物，不能把不同目标的DLL身份机械要求为同一组。依据：[官方命令行](https://docs.unity3d.com/2022.3/Documentation/Manual/EditorCommandLineArguments.html)、[官方平台编译符号](https://docs.unity3d.com/2022.3/Documentation/Manual/PlatformDependentCompilation.html)。这是证据划分设计，不是现有Android成功记录。

Mac阶段仍固定同一源码/资产/工具/目标，Compile→完整EditMode保持同版核验；转Android前保留其真实产物清单和测试终态。Android QA与正式构建命令均显式选择Android并记录实际目标、后端、ABI、裁剪、包身份及源/资产/六件发布内容指纹；两APK各自绑定实际BuildReport与APK哈希。Library内编译/导入缓存由Unity正常生成，禁止人工修改；目标变化产生的编译差异单列，不冒充产品源码变化或把Mac程序集用于证明IL2CPP执行。正式业务规则/存档语义依旧共用，条件编译仅服务已签平台适配与测试隔离，不能增一套Demo逻辑。

目标切换不能放松旧Assets/meta、Packages与未签ProjectSettings保护。UTF的唯一临时InitTestScene及逐字段临时设置按前述例外记录/恢复，正常源资产与正式设置在最终构建及试玩前重新核定。现场报错据实留下失败证据和最小修正，再按正式包及实际影响验证；不能为了换目标/换设备无故重跑未变旧阶段，也不能凭QA关闭裁剪后的成功代替正式Minimal APK的内容/AOT与实际流程。


## 工具目录整理后的当前路径（§427/428）

2026-09-28实际应用整理已把外置application并入Applications，download并入Downloads。上文小写路径保留为环境准备时的历史观察；后续现行Unity路径为/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity，PowerShell为/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/CodexTools/PowerShell/7.6.6/pwsh。两程序与原校验字节/SHA不变，Unity三个内置资源也与随包CodeResources清单相符。CONT-C的012旧路径资源读取异常已单列，新的短探针/完整验收尚须实证，不宣称路径比较即修复通过。

Android模块/SDK/NDK/JDK/Gradle和FightMatch AndroidBuildCache按同一实际Applications/Unity根定位；正式029前再次核各路径和版本。若Unity Preferences仍指向旧路径，仅在正式签准的C串行环境窗口校正，不在正在运行的Unity进程旁修改。GRADLE_USER_HOME与ANDROID_USER_HOME继续显式指向外置新规范路径；不把旧目录消失当安装组件丢失而重复下载，也不重建小写application目录。环境会话已确认冻结Unity、CodexTools及AndroidBuildCache，其他应用整理不迁动这些在用工具。
