# RES-CD-001：bootstrap 与正式资源桥接薄合同

2026-10-01 · `PREPARED_NOT_AUTHORIZED`。本轮仅新增本文；不授权产品写入、联网、安装、Unity／编译／测试、Git或派工。
owner SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，turn `01a0f6fa-335e-7b80-a020-cdbb055f9f3a`；唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。依据用户AGENTS §6及中央本轮委托；未来执行owner为C，另行绑定激活turn、Astra/xhigh、验证环境和预算。
复用[RES设计](engineering-yooasset-001.md)§10–19、[A包](engineering-yooasset-res-01a-implementation.md)、[B包](engineering-yooasset-res-01b-implementation.md)、[LOC-B](engineering-localization-luban-impl-b.md)、[B-PREF](engineering-localization-luban-impl-b-pref.md)、[Host接线稿](engineering-runtime-localization-integration-001.md)；不重复其算法、业务或保存合同。
固定文档SHA256：RES=`2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`；B=`c5a5714c5bf1f12e5d4387e7c0fa8232b898eb2578092120e51f3a86b2f4aec5`；[美术输入](art-res-01c-bootstrap-inventory.md)=`774238de5e25f62068b932fec2ac1f59f2c330410ce1ee36401bcef308adda4f`；Host稿=`8c0e793f52d8fce9aa39f2e281507e3408613484f8d09d6938cc48a1502ce56a`。

## 1. 拆门与最小真实路线

- `I0实施输入`＝固定uGUI源码／接口、资源字节和静态引用清单＋适用源码审查／定向证据；可对这些输入限定接收，**不要求实际Host完整原生复开／L1／设备ACCEPT**。FIX22仅callback／runner变化按中央冻结manifest复用未变产品／资源；本稿不运行或修改它。
- `I1模块接收`＝C代码／静态资源与D桥接各自聚焦证据；`I2集成接收`＝H接线后实际Host启动、存档隔离、保存—关闭—独立复开、双语画面与设备门。顺序为I0→C共享interface→C／D各自实现→H→I2；I2不能反过来阻塞C／D的源码／静态输入。
- 美术稿§7.4的“先完整复开再生成”在本合同候选中细化为：先核共享资源序列化引用／GUID闭包后才能引用；C自身离线运行验收和最终Host原生验收后置。字体缺引用仍阻断资源装配，不能据拆门宣称uGUI整体已ACCEPT。
- 内部Android试玩建议用**同一YooAsset运行模块的内置包模式**：由真实资源构建生成同一`FightMatchMain`、精确manifest／映射／raw文件，随测试Player装入；实际Host仍走D外层准入、LOC-B和既有六文件catalog。仅改变包的交付位置，不另建Demo业务或文件loader。
- 该路线无需R2账号／部署凭据；以已接受构建receipt和Player内固定boot SHA（内含descriptor SHA）作为显式本地信任锚，不称生产签名链。RES-A接受后的SDK源码／运行证据须确认内置模式、raw文件和manifest装载入口；不存在则该路线BLOCKED，不手写替代下载器。公开首发小bootstrap＋远端下载方向不变。
- 本轮不实现广告、后台、账号、Google Play发布或R2部署；内置包通过不证明HTTP Range、CDN、蜂窝停流／重复同意或远端更新。B的HTTPS限制保留，不为本地验证加入明文HTTP／跳过TLS校验的生产例外。

## 2. C：固定bootstrap（独立实施／回滚单元）

C交付项目资源运行interface、下载意图／进度／诊断uGUI、正式表格派生的内置最小文本及静态资源；不装配实际FightMatchDemo，不创建Session，不调用YooAsset类型。
代码可在I0和RES-B接口冻结后先写；纯状态测试使用同一interface的测试adapter。LOC-B调用部分等待其代码接收；实际文本生成／资源装配等待COPY补充、LOC-A／B及共享资源身份，不以mock文本验收真实bootstrap。
以下为**精确路径展开规则**：A=`Assets/Scripts/FightMatch/AssetAccess/`，Y=`Assets/Scripts/FightMatch/YooAssetAdapter/`，H=`Assets/Scripts/FightMatch/Host/`，TA=`Assets/Tests/EditMode/FightMatchAsset/`，TH=`Assets/Tests/EditMode/FightMatchHost/`；分号逐项列文件，不是glob。每个“新”内容路径还唯一允许同路径加`.meta`；“改”的原`.meta`只读。无隐含helper／资源路径。

| C类别 | 精确路径（按上行前缀展开） |
| --- | --- |
| 新生产／工具 | A`FightMatchResourceRuntimeContracts.cs`；H`FightMatchBootstrapController.cs`；H`Editor/FightMatchBootstrapBuild.cs` |
| 改生产／程序集 | H`FightMatchLocalizationCatalogParser.cs`（仅下述子集profile）；H`FightMatch.Host.asmdef`（只增AssetAccess引用） |
| 新测试 | TH`FightMatchBootstrapControllerTests.cs`；TH`FightMatchBootstrapResourceTests.cs` |
| 改测试 | TH`FightMatchLocalizationCatalogTests.cs`（保留full-profile断言，新增子集隔离）；TH`FightMatch.Host.Tests.asmdef`（只增AssetAccess引用） |
| 新资源 | `Assets/FightMatch/Bootstrap/Scenes/FightMatchBootstrap.unity`；`Assets/FightMatch/Bootstrap/Prefabs/FightMatchBootstrapRoot.prefab` |
| 新生成资源 | `Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.json`；`Assets/FightMatch/Bootstrap/Localization/fm-bootstrap-v1.receipt.json` |
| 新目录meta | `Assets/FightMatch.meta`；`Assets/FightMatch/Bootstrap.meta`；`Assets/FightMatch/Bootstrap/Scenes.meta`；`Assets/FightMatch/Bootstrap/Prefabs.meta`；`Assets/FightMatch/Bootstrap/Localization.meta` |

- C的interface归`FightMatch.AssetAccess`（纯.NET、noEngineReferences）；C控制器归Host，工具归既有Host.Editor；测试归Host.Tests。RES-B的Acquire interface、public业务／保存interface不变；无新friend、Presentation改动或SDK引用。
- `IFightMatchResourceRuntime : IDisposable`精确操作为`InspectAsync(long epoch, CancellationToken)`→`ResourceInspection`、`PrepareAsync(ResourceDownloadPermit, IProgress<ResourceProgress>, CancellationToken)`→`ResourceTransportResult`、只读`IFightMatchAssetProvider Assets`。Task均返结果／受控诊断，Unity对象与回调只在主线程消费；此新interface及其值类型集中在A新文件。
- `ResourceInspection`含runtimeInstanceId、inspectionId、exactResourceSetId、epoch、remainingBytes／files、requiresNetwork、networkKind、diagnostic；Inspect只读本地受信计划／cache，**不联网**，缺计划失败，不暗取manifest估算。Permit绑定全部身份／remaining／network；需联网时mobile须本次确认、未知网络不放行，旧inspection／epoch拒绝。BuiltIn或完整cache的requiresNetwork=false、网络remaining=0，可离线进入本地验证，不能误等蜂窝许可。
- `ResourceProgress`另含完成／剩余量、stage、retryability、safeCode；`ResourceTransportResult`只有`TransportVerified / Cancelled / Rejected`，绝不能表示Prepared或BusinessReady。Assets在TransportVerified前拒绝acquire；每次用户重试先取消／等停止，再Inspect新epoch，不永久记住mobile同意。
- C控制器Awake先以序列化内置子集／receipt／固定SHA建立bootstrap LocalizationService并只读B-PREF初始化语言，首帧前绑定；无D时仍能呈现本地诊断，不取full文本。internal装配interface为`Bind(IFightMatchResourceRuntime runtime)`、`TransportVerified`事件（该instance／set／epoch）、`CompleteAdmission(epoch, accepted, safeCode)`；H响应事件调用D桥接，C只接当前epoch结果，不创建Session／决定业务兼容性。
- 失焦／Wi-Fi转mobile／取消／销毁立即invalidate UI epoch并取消token；显示StopRequested，等待D证实请求停止后才Stopped。Dispose幂等；旧成功不可改新UI。下载策略沿RES §15：最多2并行、每帧1新请求、最多2自动重试；进度不冒充实际停流证明。
- 文本仍为同一CSV→Luban→校验发布的accepted full artifact的确定性子集；Editor工具只读源／receipt，按accepted key allowlist保留原模板、severity、参数，生成上述两个文件，绑定full-source／artifact／COPY／subset SHA。不得改Config、Tools、Generated或在C写正文。
- LOC-B原full-profile的精确count／SHA不得放宽：C只加internal `ParseBootstrap(bytes, acceptedSubsetIdentity)`入口，复用同一bounded parser／adapter；该identity绑定exact keys／count／schema／SHA，不能以任意count或少键冒充full source。两profile预算均机械实测＋签定margin，子集不引入第二套resolver；原full入口和其测试保持原语义。
- 待收16键沿美术稿§4：`fm.bootstrap.{checking,retry_button,offline.title,offline.body,corrupt.title,corrupt.body,storage.title,storage.body,fatal.title,fatal.body,restart_button,exit_button}`及`fm.network.kind.{wifi,mobile,offline,unknown}`（花括号只用于此键清单展开）；连同§3原20键由COPY冻结最终映射／双语／参数／severity，36仅候选数量。缺任何必需键则资源阶段BLOCKED，不能硬编码／跨语言fallback。
- 只引用美术稿§6.1列明的共享Noto TMP／OTF／OFL、TMP Settings／换行表／SDF shader及sub-assets，不复制字体、atlas、material或主RuntimeRoot。本版去掉可选PNG，使用纯色uGUI Image；Prefab仅Canvas／SafeArea／bootstrap控件，独立C场景另含唯一EventSystem，最终H复用实际场景已有EventSystem。
- 所有TMP序列化初值为诊断占位符；正常运行用已接受子集绑定。full资源失败仍用内置双语解释；若bootstrap文本／字体自身已坏，保留显眼占位符／安全诊断并阻断，不能靠另一份成品文案掩盖。C无玩家存档写入；初始语言由B-PREF只读加载，手选保存留H。
- C定向验证：两profile错用拒绝、exact key/args／双语字形、缺绑定可见；cancel／retry／stale／mobile逐次许可状态；依赖闭包无remote-only资源／主业务页面、单Canvas／EventSystem、保存后静态引用同GUID。真实空cache／无网双语页面须C+D实际运行补证，fake adapter绿测不关闭该门。
- C回滚只恢复本包改文件的preflight bytes，移除本包新文件／自然meta；有D/H消费者时先撤消费者，不能删共享interface留下编译断链。共享uGUI、LOC产物、真实存档与RES-B均保留。上限沿RES：4个生产C#／700行、3个测试C#／900行、1scene＋1prefab；超出先回中央，不自动加文件。

## 3. D：资源构建、YooAsset factory与Host准入（独立实施／回滚单元）

D源码依赖RES-A/B接收和C已固定的共享interface，不依赖C画面／Host完整复开；真实build／admission等待正式LOC与六件业务发布物、平台包／SDK身份。纯codec、factory、epoch／lease及activation fault代码可先准备；synthetic负例只证明模块，不替代真实产物。

| D类别 | 精确路径（同§2展开规则） |
| --- | --- |
| 新生产 | A`FightMatchResourceReleaseSet.cs`；Y`YooAssetRuntimeFactory.cs`；Y`YooAssetResourceRuntime.cs`；H`FightMatchResourceBridge.cs`；H`FightMatchResourceActivationStore.cs` |
| 改生产 | Y`YooAssetPackageLifecycle.cs`；Y`YooAssetAssetProvider.cs`（仅runtime factory／raw与epoch内部接线，保留B语义） |
| 新构建工具／程序集 | Y`Editor/FightMatchResourceBuild.cs`；Y`Editor/FightMatch.YooAssetAdapter.Editor.asmdef` |
| 改程序集 | TA`FightMatch.AssetAccess.Tests.asmdef`（只增上述Editor程序集）；TH`FightMatch.Host.Tests.asmdef`（只增YooAssetAdapter用于生产factory验证；先保留C增量） |
| 新测试 | TA`FightMatchResourceReleaseSetTests.cs`；TA`FightMatchResourceRuntimeTests.cs`；TA`FightMatchResourceBuildTests.cs`；TH`FightMatchResourceBridgeTests.cs`；TH`FightMatchResourceActivationStoreTests.cs` |
| 新资源／配置 | `Assets/FightMatch/ResourceBuild/FightMatchResourcePlan.json`；`Assets/FightMatch/ResourceBuild/Inputs/fm-text-v1.json`；`Assets/FightMatch/ResourceBuild/Inputs/fm-text-v1.manifest.json`；`Assets/FightMatch/ResourceBuild/fm-resource-boot-v1.json` |
| 新目录meta | `Assets/FightMatch/ResourceBuild.meta`；`Assets/FightMatch/ResourceBuild/Inputs.meta`；`Assets/Scripts/FightMatch/YooAssetAdapter/Editor.meta` |

- D runtime adapter程序集仍只引用AssetAccess与A核定的YooAsset runtime；新增Editor程序集只引用AssetAccess、YooAssetAdapter和A机械识别的SDK runtime／Editor／构建程序集。Host桥接仅依赖项目interface、Content和LOC-B；SDK类型、地址、URL不进入Host／Presentation。没有Host→adapter friend或可变全局factory。
- 唯一新增public构造入口为`YooAssetRuntimeFactory.Create(ResourceBootstrapInput input) -> ResourceRuntimeCreationResult`，成功给`IFightMatchResourceRuntime`，失败仅给受控诊断；provider／lifecycle构造器继续internal。Input／result／外层DTO／`FightMatchRawBytes`归A的D新文件，不依赖Unity；新增序列化只属resource schema v1，绝不复用业务`ContentReleaseSet`。
- Input只含Player内固定boot bytes／独立pinned SHA、预期platform／app protocol／exact resourceSetId、`BuiltIn`或`RemoteHttps`交付模式、受限budget及已批准HTTPS hosts；不接受latest／用户路径。Factory先核pin、严格schema／能力／平台／有界长度与映射，再创建SDK模块；BuiltIn不进入remote services，RemoteHttps沿B URL检查。
- boot计划包括RES §13描述符、准确manifest package version／SHA、logical mapping及物理file长度／SHA清单；descriptor与mapping只接受唯一exact字段、ordinal排序、无重复／traversal／trailing，预算在身份槽中冻结。expected SHA必须来自已接收Player构建锚，不是同一下载对象自报；本地锚不冒称生产授权签名。
- 明确区分outer资源set（沿B合法ASCII且非latest）与现有业务set `release-set:fightmatch-demo-r1`（含冒号）：记录映射并逐字核对，不把业务ID传B或改HostSession常量。六件现有文件沿`FightMatchStreamingAssetsLoader.FileNames`的固定顺序／名字采集，不重写其业务字节。
- 构建工具从accepted LOC复制两个Inputs候选、读取既有六文件及获准uGUI资源，生成固定logical map和`FightMatchMain`真实YooAsset包；它们是派生输出，source SHA／receipt驱动，不是第二套手编配置权威。资源Plan仅列来源身份／collector规则／逻辑ID，无凭据／公网部署。
- `fm.text.full`为scope，映射内分别固定artifact和manifest两个raw logical IDs；六业务raw ID与原六文件逐项对应。D的`FightMatchRawBytes`只暴露Length／bounded只读stream，走B的`AcquireAsync<FightMatchRawBytes>`及同一lease；消费期间持lease，stream由caller关闭，不能暴露可写backing array或SDK handle。
- H可调用的internal `FightMatchResourceBridge.OpenAsync(IFightMatchResourceRuntime runtime, long epoch, CancellationToken)`返回同一set的`PublishedContentCatalog`、full `ILocalizedTextSource`及owner lease组／安全诊断；它先核outer和manifest／source receipt，再调LOC-B full parser、`FirstReleaseContentStorage.Create`和`PublishedContentCatalog`。单文件≤min(16MiB,既有MaxRecordBytes)、六件总量≤32MiB，文本沿LOC预算，均先限长再分配；任何失败不给半catalog／source。
- Bridge只返回`DataReady`，不创建Session、不宣布BusinessReady；raw解析／既有storage复制完成即释放raw leases，需跨页面持有的Unity对象lease转交H并记录owner。SDK完成＝TransportVerified；内层准入／必要资源齐才Prepared，H还要现有save兼容性／真实入口才BusinessReady。
- C取消token必须在D传播到SDK实际abort／释放；运行Task到“本次活跃请求已终止”才返Cancelled，Dispose只能请求清理不能伪称已停止。所有回调校验runtime instance＋set＋epoch＋operation＋asset；stale成功释放自己的lease／handle，不触碰较新结果。重试重新Inspect，旧集合不清空。
- 失败映射：B的Package／Manifest／Sdk等保留原code/stage；D另用闭集`RES_TRUST/SCHEMA/HASH/RECEIPT/CONTENT/TEXT/BUDGET/STORAGE/STOP`安全码映射bootstrap文案。网络暂断可重试同目标；404、身份／schema／预算错误不可自动重试；storage待条件改变；停流未证实留StopRequested。不得显示SDK exception／路径／URL或拿ShowFailure／业务reload冒充资源恢复。
- ActivationStore只拥有`<approved-product-root>/resource-state/v1/active.json`、`prepared.json`、`restart.json`及各自同目录`.tmp`／`.bak`，不创建玩家profile。写入含schema／exact set／descriptor hash／阶段，bounded读、flush／原子替换／后验复读，未知提交不推进阶段；保留旧完整集合，绝不回滚PlayerSave。
- 已固定初始内置集在进程入口核锚／CodeEntered，内层和Host门齐后BusinessReady；更新才走Prepared→持久RequestRestart→下个进程CodeEntered→BusinessReady。部分CodeEntered失败只能固定诊断／退出重启；模拟重启不算实际Android进程证据。本Demo不增加后台更新或发布服务。
- D定向验证：错pin／set／平台／manifest／source receipt／六文件污染逐门拒绝；build map确定性、raw超限／释放、旧epoch／取消／双lease；同一生产factory的真实内置包烟测；activation各故障点不混集合。保留B与Content／LOC已有断言，GitHub审查对应实际head；不会用TransportVerified替代内层或设备门。
- D回滚只恢复D改动前bytes（包含C已接收增量）及移除D新文件／已列明的本包生成候选；先断开H消费者，不删除C／B、旧完整set、六文件权威或玩家存档，不把当前null-source旧Host称为可运行回退。上限沿RES：8生产C#／1600行、5测试C#／1400行、一个外层schema；无Packages／ProjectSettings写权。

## 4. 身份槽、实际接线与执行边界

- 激活单必须机械填满：`I0_SOURCE_RESOURCE_MANIFEST`（每叶path／bytes／SHA／GUID、PR/head及限定接收）；`A_GRAPH_SDK_ASSEMBLIES_BUILTIN_ENTRY`；`B_INTERFACE_SHA`；`C_INTERFACE_SHA`；`LOC_FULL_AND_SUBSET_RECEIPTS`（COPY／formal source／artifact／manifest／schema／exact keys／count／参数／两profile预算）；`BUSINESS_SIX_RECEIPT`；`PLATFORM_APP_RESOURCESET_MAP_MANIFEST`；`BOOT_ANCHOR_SHA`；`SHARED_TMP_GLYPH_CLOSURE`。缺槽只阻断依赖该槽的阶段，不猜数值／GUID。
- 新.meta由唯一Unity执行者自然生成后绑定GUID／SHA；生成前只冻结路径和absence。共享字体／TMP／场景旧meta不动；缺字需另签共享字形补充，不能在本包复制字体或悄悄扩图集。
- 构建输出先只进新`TestArtifacts/FightMatch/RES-CD-001/<activation>/D/build/`；实际SDK产出的bundle／manifest名字与长度／SHA生成闭合inventory并接收后，才逐项冻结`YOO_PLAYER_FILES`的精确Player staging路径和字节预算。此槽未绑定时禁止向StreamingAssets加文件，不能用目录通配符授权整包写入；原六文件只读。
- D验证仅在包专属技术state根／cache／build根写入；实际文件清单、filter／fullname／次数／限时／日志／进程退出及总字节预算在激活单一次冻结。自动化优先dots；Mac图形／内置包／Android设备由既有C例外合同另行授权。本文无可直接运行命令，不为设计重跑12／222／全量。
- H接线顺序固定：真实Host隐藏业务输入并实例化C prefab→用内置子集绑定→D factory／Inspect／本次许可／Prepare→D bridge→构造full LocalizationService并加载偏好→现有Session／存档门→交接主UI；保持唯一EventSystem，销毁bootstrap后才开放业务输入；失败留bootstrap，lease随owner teardown。
- **H白名单增量必须显式补签**：先前六路径还缺真实序列化装配；须加`Assets/Scenes/FightMatchDemo.unity`（只新增bootstrap prefab／boot TextAsset／独立pin引用，原GUID／meta不变），并在未来打包合同列明新资源staging及构建入口。C／D不改该场景，也不改现有`FightMatchAndroidBuild.cs`／FIX22工具；不靠测试注入或Resources.Load暗路径躲过这一步。
- 存档隔离沿[原生复开G3](testing-ugui-native-reopen-001.md)另包，不能用本包resource-state测试根冒充玩家存档隔离。最终I2仍需真实Host／原生复开／Android试玩及准确head审查；本文只解除实施输入的环，不宣称这些门已通过。
