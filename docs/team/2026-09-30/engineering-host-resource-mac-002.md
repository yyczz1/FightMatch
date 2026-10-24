# HOST-RESOURCE-MAC-002

2026-10-03 · PREPARATION_AUTHORIZED；Unity须准备回执完成后由主程绑定实际执行turn及argv激活。

## 授权与继承

中央因QA正式云回执READINESS_BLOCKED（实例未就绪、无shell、所有环境检查/测试NOT_RUN）批准本次局部Mac例外：PR5六HISO＋PR6十一纯契约，共用一次compile≤360s、一次精确17例≤180s。不是永久替代dots，不扩大产品范围。唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 唯一收件再交中央，QA不重复接C。

本增补只覆盖[原M01合同](engineering-host-save-isolation-mac-001.md)的owner、任务根、组合源码身份、单叶缓存差异、测试集合和meta白名单，其余保护、工具/资源/次数/停止政策、阶段门、证据文件名原样继承。原M01合同9578B/SHA `6adb4ce4a4ad6e82c2752568493ded25651c856008acb5c0dd1d1878b83ce755` 不改；其中所有本次新写入`M01/`改指`M02/`，994源改为下列998源、六测试改为17。唯一新根 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/M02/`；M01保持ABSENT。FIX04/M16此前拒绝、旧投影/源码/缓存证据不动，不以本包重试。

[交接记录](engineering-host-save-isolation-mac-handoff-001.json)固定旧C停止：turn `01a0fdaa-0b59-7322-8d1f-6d3e76252194` completed/idle，17:42:13Z／dd869f无Unity/UPM/copy；旧准备bd93a8仅mkdir前失败、写0。新C只读turn `01a0fdbc-c06a-7722-90f5-a6eef4693918`已completed/idle，17:56:23Z／16d496再核M01/M02缺席、进程为空、文件写0。后续准备和执行各记录真实新turn，不冒认此诊断turn为执行turn。

## 固定组合输入与机械验收

- PR3本地Git对象 `6818405b859d71bf8c0f116299e7e9605a3d769a` 的Assets/Packages/ProjectSettings原993个100644 blobs；覆盖/新增PR5两源，再追加PR6四源，成为**998叶／31,749,332B**。依原M01 canonical逐行格式所得SHA **`136fe1dff5295d9e37b8fec55cb9bbd915b9b06a2e22bb5ccfd3e2f6941b953c`**，主程e5043b exit0已核全部六后像bytes/SHA/gitBlob及17唯一源码名。`inputs.json`逐叶保留来源及身份；这是组合候选，不宣称属于任何单一PR head。
- PR5 head `a8b9758ed2db1a43dac9328f162433a4523624c9`；Host S/source-receipt.json 5655B/SHA `0f208101a2884e73ed6106e06087f1ca46cd2348870f280304036e1b646db2f7` 的sourceAfter两项逐字节采用：`Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs`、`Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs`。QA代码门回执5343B/SHA `ba187dd9d0b7b761f29cb92fac0b85ab6e5745cfbc4502cbaea3914592b55d4f`。
- PR6 head `d7775a1134556698a6489029b20402a0ce185939`；`TestArtifacts/FightMatch/RES-B-CONTRACTS-001/S/source-receipt.json` 10298B/SHA `b5437364daa542ff47737762f3860e6acfd3fc10cf4b22595aec2a6f645ca98c` 的sourceAfter四项逐字节采用：`Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef`、同目录`FightMatchAssetContracts.cs`、`Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef`、同目录`FightMatchAssetContractsTests.cs`。QA最新含云终点回执8731B/SHA `994a92ba770ccda7c4b3a1f1eff662963391166e8b5c59737c2e000b9b52c686`，同head代码门ACCEPT/no-major，不是测试通过。
- 固定manifest/lock及51依赖不变；全部17预期fullnames定义为上述两份固定source-receipt的`testFullnames`有序拼接（6＋11，无重复），在准备`inputs.json`完整展开，并在activation以逐名Regex.Escape后的全锚定alternation锁定。一次UTF结果XML须同集合、每名恰一次、17pass且0fail/skip/inconclusive；按6/11两组分别报告。无单独发现启动、无额外probe，不改任何测试断言或源码。

## 唯一缓存修订

诊断93d14a/25fa4b两次精确红，28b7d6核全17包，497c68核原tgz：冻结I/prepare.json.payloadFiles中的`com.unity.visualscripting@1.9.1/.DS_Store`（6148B／SHA `314a4380cce52874d0f82bfb9ff9109caf367ebfe67628ae6b278a5d03a41f12`）在旧投影、批准解包缓存及原tgz均缺席，其余16包全等、该包增0改0、无symlink/shared inode。删除时间/执行者未知，不推定历史篡改者。

主程限定批准新M02所需payload集合为**原固定17包清单仅减去该准确单叶**，即9928叶／394,434,959B；这是已定位的Finder元数据缺口，不是允许忽略任意隐藏文件/缺失源码。原prepare及旧缓存零写，不补造DS_Store。准备先复核除此以外缺0增0改0，再从原已批准`LAYOUT-S1-001/package-cache`只读复用；`cache-inputs.json`记录原清单身份、唯一差异、当前全量身份/来源/类型与计数。原M01 copy-on-write、无hardlink、≤1GiB、无新下载/安装、避免重复payload拷贝等限制保持。若多出任何差异、来源不明或离线缓存不足则停止报告，不能再放宽。

## 自然meta准确十路径

只允许Unity在新投影自然新增以下meta，不手写、不导出共享；998原输入零变更，GUID唯一性、默认SceneTemplateSettings及其它生成输出限制继承M01。

1. `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta`
2. `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta`
3. `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`
4. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs.meta`
5. `Assets/Scripts/FightMatch/AssetAccess.meta`
6. `Assets/Scripts/FightMatch/AssetAccess/FightMatch.AssetAccess.asmdef.meta`
7. `Assets/Scripts/FightMatch/AssetAccess/FightMatchAssetContracts.cs.meta`
8. `Assets/Tests/EditMode/FightMatchAsset.meta`
9. `Assets/Tests/EditMode/FightMatchAsset/FightMatch.AssetAccess.Tests.asmdef.meta`
10. `Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetContractsTests.cs.meta`

## 准备及返回

当前仅授权按M01相同五前检文件、projection/package-cache/tmp在新M02准备（≤120s）；共享1089、全部原S/前S保护、FIX04十叶、旧投影与旧缓存身份先后保持。完成后final给准备实际turn、五文件hash、998源digest、17完整名单、缓存准确修订/复制策略、Editor及进程/空间事实；主程机械接回后再绑定执行turn和完整argv。Unity运行前不得仅凭本增补自行越过activation。没有在执行中换owner、产品修复、Play/真实Host/G3/真实存档、Scene/Prefab、Android、Git写或SDK加载授权。出现失败保留结果并停止后续；更小修正及受影响子集另签，不自行重跑。
