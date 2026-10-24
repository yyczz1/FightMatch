# RES-B-ADAPTER-001：剩余真实 adapter 源码增量

2026-10-03 · `READY_FOR_IMPLEMENTATION_DISPATCH`；仅源码合同，不是最终 ACCEPT。

## 1. 授权、输入和最小覆盖

授权：用户继续开发/AGENTS §6/中央本次指示。唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0fe90-2cc1-7d21-9d7c-749be6effd37`；ARCH turn `01a0fe93-27c1-7203-8163-c7103b37d69f`裁定补B、不重设计CD。SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a0fe9c-4026-7253-a221-5bc8de8c3124`（thread/host/turn）。
owner `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，`gpt-6-astra/xhigh`；主程首次派发即交S实施至封存，before记实际turn/hash。不等Host/LOC/Scene，不占Unity槽。
复用[RES](engineering-yooasset-001.md) §10/16/18–19／SHA `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`；[B](engineering-yooasset-res-01b-implementation.md) §6–8／`c5a5714c5bf1f12e5d4387e7c0fa8232b898eb2578092120e51f3a86b2f4aec5`；[CD](engineering-resource-runtime-bridge-cd-001.md)／`ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`。旧B“先A才source/旧C/新8文件/全部meta/全量回归”仅改为本6delta/独立作者/无meta/定向验证；合同、生命周期、预算、诊断、租约不弱化。
P03 `TestArtifacts/FightMatch/RES-01A/P03/final-verdict.json`＝2316B／`36b5028f50f61b6f5e1a670dcc510d0c8512a36b54045b40a62df12a6d9742ff`。[A集成回执](yooasset-res-01a-integration-receipt.json)／`d022dd583d94cef6d3e8669b600529d994716c9783031a2f216e3c61a158938a`已限定ACCEPT；[PR8审查回执](testing-yooasset-package-authority-fix-pr8-review-receipt.json)＝5765B／`fda68a0c134379e06a5ad17d616bc9cc34566f53a3594c27e241916df076ed98`，head `4d81378556d27dfe2f8e445d3c2ce59d7a4e61c8` completed/no-major＋P03 native ACCEPT。不是资源运行证据，不重装/共享采纳或重跑全量。
基线为PR6 `d7775a1134556698a6489029b20402a0ce185939` 的 `TestArtifacts/FightMatch/RES-B-CONTRACTS-001/S/source/`。路径前缀精确展开：A=`Assets/Scripts/FightMatch/AssetAccess/`，Y=`Assets/Scripts/FightMatch/YooAssetAdapter/`，T=`Assets/Tests/EditMode/FightMatchAsset/`。

| 冻结基线叶 | SHA256 |
| --- | --- |
| A`FightMatchAssetContracts.cs`（251行） | `4ababded06a870408ebb74e157eeb4ea3233509fb7e6b48d6b904f1d6ac04330` |
| A`FightMatch.AssetAccess.asmdef` | `09347e70d10f8cb3e26751c004ba005cadc32d1f3b6ecadf78bb6c9cf1fb2e45` |
| T`FightMatchAssetContractsTests.cs`（247行、原11例） | `6c65da27c7847d50e5a3c0beb1da30d58db025646be21b2eec493c816c3ba5fc` |
| T`FightMatch.AssetAccess.Tests.asmdef`（唯一修改基线） | `1d605c9ea6e3727c7fe4f595078a12469def24ec7055489f1a467951dde244a0` |

SDK＝YooAsset 3.0.6／`3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`；只读根 `TestArtifacts/FightMatch/RES-01A/P01/projection/Library/PackageCache/com.tuyoogame.yooasset@3b4cfb36cc/`；`Runtime/YooAsset.asmdef` 名`YooAsset`／SHA `8a526656e25acb868a4b1b4539e528d9fa30190aca1b29e2ff0163f930e9ad5b`。实际入口已读，不下载/改/复制SDK。

## 2. 唯一写域与checkpoint

S=`TestArtifacts/FightMatch/RES-B-ADAPTER-001/S/`，已核ABSENT、本轮不创建。仅15叶：`S/source/`下6delta＋上表前3叶原字节输入副本（非diff）；另`S/{before.json,after.json,source.patch,verify.py,static-checks.json,source-receipt.json}`六证据。无其他文件/缓存/投影/meta。

| 产品delta（相对source，前缀按§1展开） | 唯一允许变化 |
| --- | --- |
| 新 Y`FightMatch.YooAssetAdapter.asmdef` | runtime、engine enabled，refs恰`FightMatch.AssetAccess`＋`YooAsset` |
| 新 Y`YooAssetAssetProvider.cs` | internal provider/映射/租约/有界dispatcher及测试seam |
| 新 Y`YooAssetPackageLifecycle.cs` | internal真实SDK操作与明确owned/borrowed生命周期 |
| 新 Y`YooAssetRemoteServices.cs` | internal `IRemoteService`实现，无真实域名/凭据 |
| 新 T`FightMatchAssetProviderTests.cs` | focused测试、nested fake；不直接依赖SDK类型 |
| 改 T`FightMatch.AssetAccess.Tests.asmdef` | 仅references追加`FightMatch.YooAssetAdapter`，其余保持 |

生产类/构造器internal；唯一friend置于provider文件：`[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.AssetAccess.Tests")]`。八public contracts/3基线原字节不变，无新增外部public surface。SDK与fake只跨internal seam，断言走原provider/lease/result，不拿fake充生产实现。
生产≤1000物理行（251＋3新文件≤749），测试≤900（247＋新文件≤653）；不压行。共享Assets、旧暂存/证据、Packages/ProjectSettings、Host/UI/LOC/存档/配置/资源/场景、索引/旧文档禁写；不增public factory/raw DTO/download permit、实际映射/build/网络策略。
before一次绑定授权/身份、本文/输入/SDK叶SHA、共享9路径及meta类型/absence/hash；after同组核对。检查15叶/bytes/hash/行数、asmdef/friend/quarantine、6delta patch重放、合同/原11不变及无外写。漂移/已有S回主程，不全盘扫描/覆盖。
作者用apply_patch，机械复制/patch/JSON仅写15叶。离线命令`python3 -B TestArtifacts/FightMatch/RES-B-ADAPTER-001/S/verify.py`：≤30秒、S≤2MiB、仅标准库，无子进程/网络/编译；保留失败，只为脚本修复有限重核。静态检查非行为验证。

## 3. 实施必须闭合的细节

- B §6顺序/null/result/诊断/四budget不变；按请求预算＋模块总数预留pending/waiter/lease，失败归还。`T:class`先验Unity.Object可赋值及映射type，再非泛型`LoadAssetAsync(location, typeof(T))`；null/type错误拒绝lease。
- Immutable mapping: id→exact set/package/version/location/type. BuiltIn: `new OfflinePlayModeOptions { BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(root) }`→`InitializePackageAsync(options)`→`LoadPackageManifestAsync(new LoadPackageManifestOptions(exactVersion, timeout))`→核`GetPackageVersion()`→acquire；逐步成功才推进，timeout=60秒。root显式所有。禁旧入口/latest/RequestPackageVersion/Task.Run/同步Wait/直读loader/downloader/远端传输/Prepared宣称。
- `IRemoteService.GetRemoteUrls(string)`→`IReadOnlyList<string>`；沿B §7.2/8核HTTPS allowlist/固定身份/安全相对filename，主备有序不重复，拒绝注入不转义坏值；诊断不含SDK异常/URL/路径。
- 真实构造限Unity主线程，捕获Unity context/thread id；缺context拒绝。Acquire/Asset限该线程；后台Acquire先做B纯验证再返`SdkFailure/ValidateRequest`，零SDK。仅测试可注入dispatcher，禁线程池context/后台SDK。
- Any-thread Dispose: Interlocked marks released once; set the reserved lease record's flag and coalesce one main-context drain, never touch Unity/SDK off-thread. Drain owns refcount/Release. Reserve callback/wakeup capacity under MaxQueuedCallbacks before admission; pending/lease records remain bounded. Full acquire queues cannot drop cleanup or cause unbounded Posts. Failed Post retains flags for retry at the next main-thread entry/CloseAsync: no off-thread release or false closed. Released Asset returns null; safe sink records ReleasedLease once and cannot throw outward.
- 先登记operation/TCS/额度/五元，再取并登记handle，最后订阅Completed（已完成会同步调用）。回调/异常exact-once、解绑后释放；TCS用RunContinuationsAsynchronously。publish核provider实例/epoch/set/op/asset；重复不重复交付，stale仅释放自己旧handle，不动新记录。
- 同一provider/epoch/set/package/location/type/asset共享pending/已载handle，各waiter占额度、不同leaseId；首释放不伤其它，末lease且无waiter才一次Release。epoch单调，旧请求拒绝/旧pending不发布，live lease仍有效；换manifest等包pending/live归零。
- Track global and package ownership separately. Existing global is borrowed: never Destroy. Existing package may only be borrowed after successful initialization/exact-version match; never reinitialize/switch manifest/unload/destroy/remove it. Only self-created packages are owned. Private ownership records count all module runtimes/providers/borrowers, pending and leases; creator shutdown cannot bypass these counts.
- Idempotent internal CloseAsync closes admission/invalidates pending publication, waits for actual init/manifest/load terminals, reclaims handles but preserves live leases. Destroy an owned package only at zero pending/waiters/leases/queued releases: successful `DestroyPackageAsync`＋same object `InitializeStatus==None`→`YooAssets.RemovePackage`. Failures stay cleanup-pending; no forced UnloadAll. Destroy global only if initialized by this module, module refs zero and SDK package list empty; otherwise retain it and report not destroyed. Rejected Task≠SDK stopped.
- MaxRawBytes按可信长度先验；不可核raw拒绝（本包无raw读取/DTO）。禁无界读后检查/伪造0/宣称Unity对象内存封顶；实际raw/完整资源链后置。

## 4. 测试、后置门与回执

原11个`FightMatchAssetContractsTests`及证据复用（B-01、B-17/18值合同），不改。新增provider测试覆盖B-00、B-02–16及adapter B-17/18全部矩阵行为；原11不代替adapter验证。
新增同步/重复/迟到Completed、后台Dispose主线程释放、满队列清理、Post失败重排、pending关闭/重试、owned/borrowed及live lease阻止销毁。fake控制终态/异常/计数，不冒充真实内置加载。作者列fullname，后续discovery冻参数叶数；矩阵19行/原11不是新增discovered count。
checkpoint仅`SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING`或`BLOCKED`。QA/C另绑同候选/SDK图的一次真实compile＋一次focused（可含原11，不另跑17/4181/全量）；dots优先，本地例外/命令/filter/预算/证据根另签。Unity C=`01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`；源码作者不运行Unity/discovery/测试或生成meta。
A门已限定通过；B须新head GitHub PR Code Review＋实际SDK compile/focused后接收，作者不自批、不派本地review。真实BuiltIn包/manifest/对象依赖/双lease/断网cache/raw、Host/设备烟测为`NOT RUN`，后续CD/H另收。FIX04 native/LOC M16 auto-review原阻塞保持，禁绕过。
主程wait收final：实际身份、15叶hash/bytes/lines、准则/失败/NOT RUN。扩文件/public API/用户选择则具体BLOCKED。本轮仅新增本文，无S/派工/产品/Unity/测试/Git/网络操作。
