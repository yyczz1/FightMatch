# RES-SDK-ADOPT-G02：SDK／RAW／factory 最小采用计划

2026-10-08，`PROPOSED_NOT_DISPATCHED`，只新增本计划。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，turn `01a11838-e662-7180-9549-8849333dd1d5`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。执行合同另签，旧租约不恢复。

## 1. 结论与前像

按 **SDK01 → BRIDGE01 → FACTORY01** 采用已接收字节：15新增、3覆盖、0删除、8新GUID。LOC为null不阻碍源码及隔离单测采用，但不接通Host。共享根零下载解析尚未证明，见§4。

根 R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/`。下文 E=`TestArtifacts/FightMatch/`，V=`docs/versioning/`，M=`docs/team/2026-09-30/`，Y=`Assets/Scripts/FightMatch/YooAssetAdapter/`，T=`Assets/Tests/EditMode/FightMatchAsset/`，均相对 R。

起点见 `M/resource-source-adopt-g01-central-receipt.json` 及其group-freeze：1021文件、32464207B，canonical SHA256 `a8a5c77a0190e4a4b9999d0b75de3d7aa75aded8d57c8de77e118facbb525ffc`，实核一致，编译待验证。

接收依据为M下 `yooasset-res-01a-integration-receipt.json`、`yooasset-adapter-fix03-integration-receipt.json`、`resource-raw-pref-m02-integration-receipt.json`、`resource-as-factory-m02-integration-receipt.json`；固定head/审查/历史失败沿回执。旧组合通过不是共享接收，不重复本地审查。

## 2. 三包源码白名单

### SDK01：仅两个 Packages 文件

采用既有UPM输出，不手写依赖或重跑安装器。来源在 `E/RES-01A/P02/`；前后SHA256取 `V/2026-10-03-yooasset-package-pr/publication-manifest.json` 对应条目，前像与G01相同。

| 目标 | 来源 | 前像 bytes | 后像 bytes |
| --- | --- | --- | --- |
| `Packages/manifest.json` | `manifest.after-install.json` | 1775 | 1913 |
| `Packages/packages-lock.json` | `packages-lock.after-install.json` | 11101 | 11812 |

固定 `com.tuyoogame.yooasset` 3.0.6，URL `https://github.com/tuyoogame/YooAsset.git?path=/Assets/YooAsset#3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`；间接依赖 `com.unity.scriptablebuildpipeline` 1.21.25。包图51→53，旧51节点版本/source/depth/edges不变。不拷SDK进Assets，不生成Packages meta。

### BRIDGE01、FACTORY01：一次采用最终版本

来源前缀：B=`E/RES-B-ADAPTER-001/FIX03/S/source/`；RAW=`E/RES-D-RAW-001/FIX02/S/source/`；F=`E/RES-FACTORY-ADMISSION-001/S/source/`。每行来源为前缀加完整目标路径。除测试 asmdef 外，目标前像全为不存在。

| 包 | 来源 | 目标 | bytes |
| --- | --- | --- | --- |
| BRIDGE01 | B | Y`FightMatch.YooAssetAdapter.asmdef` | 444 |
| BRIDGE01 | RAW | Y`YooAssetAssetProvider.cs` | 19671 |
| BRIDGE01 | RAW | Y`YooAssetPackageLifecycle.cs` | 27260 |
| BRIDGE01 | B | Y`YooAssetRemoteServices.cs` | 3494 |
| BRIDGE01 | B | T`FightMatchAssetProviderTests.cs` | 47591 |
| BRIDGE01 | B | T`FightMatch.AssetAccess.Tests.asmdef` | 553 |
| FACTORY01 | F | Y`YooAssetRuntimeFactory.cs` | 14380 |
| FACTORY01 | F | T`FightMatchResourceRuntimeTests.cs` | 78064 |

测试asmdef前像515B，hash沿G01输入；只增 `FightMatch.YooAssetAdapter` 引用，原meta不变。新增adapter asmdef引用 `FightMatch.AssetAccess`、`YooAsset`，故先采用SDK。AssetAccess、Host、Host.Tests asmdef及现有friend不改。

后像SHA256分别取 `E/RES-B-ADAPTER-001/FIX03/S/source-receipt.json/source_inventory`、`V/2026-10-03-resource-raw-pr/fix02/publication-manifest.json`、`V/2026-10-03-resource-factory-pr/manifest.json` 的精确目标条目。直接采用最终版本，不先写旧RAW测试/B实现。G01的43990B codec、合约、AS及测试保持。

## 3. meta、差异与完成条件

八份meta复用自然生成字节：

- BRIDGE01：表中五个新增文件各自的 `.meta`，另加 `Assets/Scripts/FightMatch/YooAssetAdapter.meta`。源/目标/bytes/hash/GUID取 `V/2026-10-03-yooasset-adapter-pr/fix01/metadata-receipt.json` 对应六个entries，来源为 `E/RES-01A/P01/projection/`＋目标路径。
- FACTORY01：T`FightMatchResourceRuntimeTests.cs.meta` 取 `E/RES-RAW-JOINT-001/M01/receipt.json/naturalMeta`；Y`YooAssetRuntimeFactory.cs.meta` 取 `E/RES-ACTIVATION-FACTORY-JOINT-001/M02/receipt.json/naturalMetas[目标]`。两条均含243B原text/hash/GUID；按UTF-8解码text，保留尾换行。

本轮核八 GUID 无共享冲突。SDK01后1021文件/32465056B；BRIDGE01后1032/32564864B；FACTORY01后 **1036/32657794B**，预期 canonical SHA256 `61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8`。计算域/算法沿G01，只读内存合成，不是已采用状态。

每包小回执记录实际身份、来源、前后hash、差异及GUID；整组冻结1036输入后验证。中央派发前固定回执路径。前像/来源漂移即停，不扩白名单或自动回滚。18目标外全部保护；采用阶段不启动Editor，状态仅 `SOURCE_ADOPTED_COMPILE_PENDING`。

## 4. 跨平台与缓存边界

manifest/lock 无 `file:` 或机器绝对路径，固定Git提交/registry版本可共用。保留 Unity2022.3.18f1、原工具链、换行和GUID。Windows复用项目输入/包内容，不携带Mac编译缓存或执行路径；Windows/Android实际验证仍缺。

缓存前缀 P=`E/RES-01A/P01/`，位于外置盘：

| 缓存 | 可用证据与边界 |
| --- | --- |
| P`package-cache/` | 子进程 `UPM_CACHE_ROOT` 候选，不写manifest。SBP位于其 `npm/packages.unity.com/com.unity.scriptablebuildpipeline/1.21.25/package.tgz`，183947B，hash与原source-license记录相同 |
| P`projection/Library/PackageCache/com.tuyoogame.yooasset@3b4cfb36cc/` | 2070文件/30567744B，零symlink |
| 同级 `com.unity.scriptablebuildpipeline@1.21.25/` | 327文件/1024192B，零symlink |

两包全树本轮复核与 `E/RES-01A/P03/run.json/installedPackagePayloads` 的精确条目相同；元数据、四正式asmdef和来源沿 `E/RES-01A/P02/resolved-package-graph.json` 及原license记录。

**缺口是缓存接入。** 共享 `Library/PackageCache` 无两包；UPM_CACHE_ROOT只有registry层，不能证明Git包可离线解析。不自动clone/下载、改本机 `file:`、链接旧树或整份复制projection/Library；本计划不授权写共享Library。

可选载体是原位复用P01，但须另签其输入到1036的精确差异及缓存边界，排除旧源，重新绑定编译证据。若直接运行共享根，先解决UPM识别两包及缓存权限；仅复制两包内容也须单独批准并验证。当前不切换载体或恢复旧租约。

## 5. 最小验证及剩余真实输入

G01由主测试负责。仅在其未启动、中央/主测试确认1036输入和缓存合同就绪时，可合为一次编译＋一次 **89项**：G01原43＋`FightMatch.AssetAccess.Tests.FightMatchAssetProviderTests`27＋同namespace的 `FightMatchResourceRuntimeTests`19（RAW13/FAD6）。依据：G01源不改、测试程序集增引用、fixture可同批执行。须新运行，不拼历史结果；已启动任务不插入源码或改合同。

若G01已完成，SDK组候选最小集为B38＋D13＋RAW13＋FAD6＝70；C11/AS8只在输入未变且主测试确认适用时复用G01证据，否则由主测试签必要补充。不另跑全量。正式编译门含四SDK程序集 `YooAsset`、`YooAsset.Editor`、`Unity.ScriptableBuildPipeline`、`Unity.ScriptableBuildPipeline.Editor`，以及实际受影响项目程序集的源码/引用/输出绑定；不导入Samples~的九个asmdef。

中央/主测试启动前固定预算、载体、owner和输出；一次运行、0重试。通过须exit0、无编译/UPM/domain错误、53节点及保护一致、测试名各一次且0失败/跳过/不确定、日志/XML及收尾齐全。需联网、图漂移、GUID冲突或输入不明即停。GitHub集成审查门保留。

源码采用不需新设计。真实运行仍缺正式LOC、可信boot/descriptor/mapping及物理包，外部boot pin、set/platform、protocol、capabilities/type白名单、模式/预算/HTTPS hosts，以及Host接入、真实下载/移动网络同意、重启和设备证据。fixture/草稿LOC不得充正式输入；factory采用不等于初始化成功。
