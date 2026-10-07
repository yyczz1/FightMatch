# RES-SOURCE-ADOPT-G01：已接收资源源码采用组

2026-10-08；`PROPOSED_NOT_DISPATCHED`。依据中央本回合委托，主程仅设计。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，turn `01a11826-456f-7372-994f-325afac2539e`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。中央另派 `gpt-6-luna/medium` 机械执行者并记录实际身份。

## 1. 采用结论与固定输入

正式 LOC 仍为 null。本组不接通 Host 或构造正式文本源。四包顺序 B01 → C01 → D01 → AS01；合计 **12 新增、1 覆盖、0 删除**，六个新增 GUID。源码与测试逐字节采用。

根目录 R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/`。下文路径均相对 R，别名拼接为精确路径：

| 别名 | 路径 |
| --- | --- |
| A | `Assets/Scripts/FightMatch/AssetAccess/` |
| T | `Assets/Tests/EditMode/FightMatchAsset/` |
| H | `Assets/Scripts/FightMatch/Host/` |
| HT | `Assets/Tests/EditMode/FightMatchHost/` |
| E | `TestArtifacts/FightMatch/` |
| V | `docs/versioning/` |
| M | `docs/team/2026-09-30/` |

起点见 `E/RES-CODEC-ADOPT-001/S01/receipt.json`：Assets、Packages、ProjectSettings 共 1009 文件、32319243B，canonical SHA256=`0d3d45c86c15d77bb2f430712ce45c2847c74da7ea03af284c0c6afca48d803e`，本轮实核一致。

canonical 算法沿用原回执：三个目录下全部普通文件按相对路径排序，逐项 UTF-8 `path + NUL + Git-blob-SHA1 + NUL + decimalBytes + LF`，串联后 SHA256。遇符号链接、来源/前像不符或并行改动，停止交中央；不按新状态自行扩白名单。

接收链复用 M 下 `testing-resource-contracts-pr6-review-receipt.json`、`yooasset-adapter-fix03-integration-receipt.json`（B）、`resource-contracts-fix01-integration-receipt.json`（C11）、`resource-as-factory-m02-integration-receipt.json`（D/AS，40项固定组合）；固定 PR/head 及独立证据见各回执。不把限定接收当整个 PR 树或共享组合通过，不追加本地代码审查。

## 2. 四包精确白名单

源码来源为“来源根＋完整目标路径”。除 D01 外，目标及 meta 前像必须不存在。七份源码后像如下；meta 见下一节，无额外目录或 asmdef。

| 包 | 来源根（E 下） | 目标 | bytes | SHA256 |
| --- | --- | --- | ---: | --- |
| B01 | `RES-B-ADAPTER-001/FIX03/S/source/` | A`FightMatchAssetContracts.cs` | 11990 | `4ababded06a870408ebb74e157eeb4ea3233509fb7e6b48d6b904f1d6ac04330` |
| B01 | 同上 | T`FightMatchAssetContractsTests.cs` | 16708 | `6c65da27c7847d50e5a3c0beb1da30d58db025646be21b2eec493c816c3ba5fc` |
| C01 | `RES-C-CONTRACT-001/FIX01/S/source/` | A`FightMatchResourceRuntimeContracts.cs` | 16774 | `4ef8e56e70a19f7df81105fa46b3b1d334f39ce956610285845f1c594ec0520b` |
| C01 | 同上 | T`FightMatchResourceRuntimeContractsTests.cs` | 28399 | `4fe6bb1ed6e2c5bc0d62685b807de42db9a38e2069d07547abbd27502503e13b` |
| D01 | `RES-FACTORY-ADMISSION-001/S/source/` | A`FightMatchResourceReleaseSet.cs` | 43990 | `64fa2538ad84fbcbf68871cbe0d0df8f647bb667c575893bec1ec7f1edb180ca` |
| AS01 | `RES-D-ACTIVATION-001/FIX01/S/source/` | H`FightMatchResourceActivationStore.cs` | 21172 | `94a86aaf5413f91913492486e86f03113435ada247558044535c9787cc5c8155` |
| AS01 | `RES-D-ACTIVATION-001/FIX02/S/source/` | HT`FightMatchResourceActivationStoreTests.cs` | 36336 | `572c46a5c37734fe6cb8db4ec9b3a0c0c133ce7284ce6e0458451e6bcf9cb029` |

可复用的完整身份清单：B 为 `E/RES-B-ADAPTER-001/FIX03/S/source-receipt.json` 的 source_inventory；C 为 `V/2026-10-03-resource-runtime-contracts-pr/fix01/publication-manifest.json`；D 为 `V/2026-10-03-resource-factory-pr/manifest.json`；AS 为 `V/2026-10-03-resource-activation-pr/manifest.json`。只选表内行，不整目录复制。

B01 为纯资产合约；C01 依赖 B01 的 provider/诊断声明，不依赖 SDK。D01 直接采用最终 codec 的 RAW 字节类型及 admission 元数据投影，仅依赖 BCL；保留原有 `InternalsVisibleTo("FightMatch.YooAssetAdapter")`，它不是 asmdef 引用。AS01 使用原 Host 程序集和 Host.Tests friend，不加入启动调用、存档迁移或资源激活行为。

**唯一允许覆盖：** D01 前像 31863B / `1dcd1288125b2c1f2ed26d6be9a4cfdd28ae59ae024e584b1b7a42137cc6061a`。其原 meta 243B / `900e5f8ccba2464369b17e4e73258e5431ef329a24e31658a597c61d0fdbb62c` 必须保持；现有 `T/FightMatchResourceReleaseSetTests.cs`（56352B / `e2fc16d89145bf7f502426545f1a6d0bf0b822adcc45db89842d0dc89ded5842`）及 meta 保持。

## 3. 六份 meta 与程序集边界

B01 两份 meta 来源为 `E/RES-01A/P01/projection/`＋目标路径。C01 来源为 `V/2026-10-03-resource-runtime-contracts-pr/metadata-receipt.json` 的 `naturalMeta[目标路径].text`；AS01 来源为 `E/RES-ACTIVATION-FACTORY-JOINT-001/M02/receipt.json` 的 `naturalMetas[目标路径].text`（该回执 SHA256 `91a6b41505875a0a86e8c1ee2fe4aa37b18166d036c0a076896e567fd923f23b`）。JSON text 解码后按 UTF-8 原样保存，保留尾换行；不是重制 meta。

| 包 | 目标 | SHA256（均243B） | GUID |
| --- | --- | --- | --- |
| B01 | A`FightMatchAssetContracts.cs.meta` | `cc18e3364c4f4b7e2eeb066924d754603135c61f23c2235fea36eeee3b99d20a` | `0cca28a39ee5f4e4cadd2748215dcf90` |
| B01 | T`FightMatchAssetContractsTests.cs.meta` | `3c2a4529ca7e380f23312c718b89725d6f9228034c2e1e438b7cdb4a88891d4e` | `4081e276687c843c0a2069c541573465` |
| C01 | A`FightMatchResourceRuntimeContracts.cs.meta` | `53601a3177452db4edeccfce18a42cb4c9117d3fecf1812a0d4741b9b97100ce` | `e6b6f41e1459b485582f8fca69731ecf` |
| C01 | T`FightMatchResourceRuntimeContractsTests.cs.meta` | `5d8c39285bc2726254b995b13dc9dafa73cd0287a52d3ae1da0525c557709e49` | `54669ba686cc245eb819089826861cfa` |
| AS01 | H`FightMatchResourceActivationStore.cs.meta` | `54139aad330584a064b5bbd96f96910a1f8cc2eddf8472885db97ed87bccdf38` | `586d3d3787fdb4e7e97cca2a7d4f1061` |
| AS01 | HT`FightMatchResourceActivationStoreTests.cs.meta` | `a681ad80e8dc9b163bf3e9cf9945b981822334bb59743a5f51d0fd61fa35dec7` | `ae2e036e825944332b32cbb3593b23c0` |

六 GUID 已核无冲突，执行前重核。四份 asmdef 及相关 meta、目录 meta 保持：

| 路径 | bytes / SHA256 |
| --- | --- |
| A`FightMatch.AssetAccess.asmdef` | 377 / `09347e70d10f8cb3e26751c004ba005cadc32d1f3b6ecadf78bb6c9cf1fb2e45` |
| T`FightMatch.AssetAccess.Tests.asmdef` | 515 / `1d605c9ea6e3727c7fe4f595078a12469def24ec7055489f1a467951dde244a0` |
| H`FightMatch.Host.asmdef` | 346 / `85acabf7d4ec5e2b9970f9299ab2687cef131a4fa8213aa4f0aea9f70b4c0c7e` |
| HT`FightMatch.Host.Tests.asmdef` | 479 / `50da0a6cb45f6ba0b7a4b843bdc8d865595df7d15bdc1976d01dcd52f2437e03` |

AssetAccess 仍为 references=[]、noEngineReferences=true；其测试仅引用 AssetAccess。**不采用** FIX03 中553B的 AssetAccess.Tests asmdef，它还引用 YooAssetAdapter。Host 及 Host.Tests 不新增引用或 friend。

## 4. 小包回执与整组冻结

每包依次核前像/来源、采用、核全部受保护文件。完成条件：目标 bytes/hash 相同、GUID 无冲突、仅该包预期差异。机械阶段 Unity/测试/下载/Git 写入为0；有消费者则交中央处理，不自行关进程或让 Editor 导入中间组合。

新增证据仅 `E/RES-SOURCE-ADOPT-G01/` 下 `B01/receipt.json`、`C01/receipt.json`、`D01/receipt.json`、`AS01/receipt.json`、`group-freeze.json`。每包记录实际身份/授权、目标/来源、前后 bytes/hash、差异和 GUID 检查；状态 `SOURCE_ADOPTED_COMPILE_PENDING`。异常停止后续包，保留现状交中央签恢复，不自行回滚。D01 原字节见 `E/RES-D-CODEC-001/FIX02/S/source/` 对应路径。

只读内存合成的预期中间值为 B01 1013文件/32348427B，C01 1017/32394086B，D01 1017/32406213B；最终 AS01 **1021文件/32464207B**，canonical SHA256=`a8a5c77a0190e4a4b9999d0b75de3d7aa75aded8d57c8de77e118facbb525ffc`。这些不是已采用结果。

group-freeze 固定1021文件清单、13个变化后像及四回执身份。所有白名单外文件保持；尤其 Packages/ProjectSettings、Host 既有代码、场景/Prefab、缓存、玩家存档、旧证据和 Git 不属于采用范围。

## 5. 一次组合验证与暂缓项

四包完成后，中央交主测试安排**一次**新组合验证，固定最终1021文件身份，不逐包启动 Unity。先用已验证可用的 dots；若需本机，仍由唯一 C 按另签合同串行执行。环境、导入/编译写权限、证据路径、资源/时间预算及退出收尾须在派发前确定，本设计不授予运行权限，也不设计新监护器。

最小验证内容：新组合导入/编译成功，AssetAccess、AssetAccess.Tests、Host、Host.Tests 实际输入/输出归属可核；一次定向执行以下四 fixture：

- `FightMatch.AssetAccess.Tests.FightMatchAssetContractsTests`：11。
- `FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeContractsTests`：11。
- `FightMatch.AssetAccess.Tests.FightMatchResourceReleaseSetTests`：13。
- `FightMatch.Host.Tests.FightMatchResourceActivationStoreTests`：8。

总计43项，各原名称恰一次，无遗漏/额外/失败/跳过/不确定项；保留真实退出码、日志和 XML。AS 测试仍用隔离文件域。只验证新组合，不重跑旧全量；旧1008缓存不能冒充1021来源。失败收口，无额外重试。后续集成保留 GitHub PR Code Review 门和独立测试收件；机械回执不自签 ACCEPT。

本组不采用 RAW/provider/lifecycle/remote-services、`YooAssetRuntimeFactory.cs` 或合并 RAW/FAD 测试。Factory 文件本身虽只用 BCL 和合约，所属 YooAssetAdapter asmdef 明确引用缺失的 `YooAsset`；合并测试也依赖适配器。共享 manifest/lock 未接入 YooAsset/SBP；此前 RES-01A 仅隔离组合接收。本轮不复制包配置、不下载、不改 asmdef 拆程序集、不搬文件绕开依赖。

后续缺口：SDK 共享采用须有 package/lock/元数据及平台验证合同；RAW/factory 随后采用现有已审版本；runtime/Host 接通仍缺正式 LOC、真实 release-set/Player pin 及产品策略输入。本组不证明 RAW 加载、admission、激活、Android 或 Demo 可运行。测试夹具不能填补正式文本源。
