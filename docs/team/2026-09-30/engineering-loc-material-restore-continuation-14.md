# LOC-M14 — material / tests restore 最小续行方案

2026-10-01（Asia/Shanghai） · `PLAN_ONLY_PENDING_CENTRAL_AUTHORIZATION`。本轮仅新增本文；未派 LOC、创建执行 roots、运行 driver/restore/dotnet/Unity、联网、验签或修改 Git。

## 1. 责任、继承与接收边界

- authority：用户 `AGENTS.md` §6 standing workflow及中央本轮“准备最薄 M14”指令；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`；签发/收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；待授权 delivery owner 为原 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，后续派发显式 Astra/xhigh并记录实际 turn/tool identity。本文不代替中央执行授权。
- 保留 [M09](engineering-loc-license-alt-m-materials-continuation-09.md)、[M10](engineering-loc-license-alt-m-materials-continuation-10.md) 的 material/old-root/budget 合同及 [lock-set correction](engineering-localization-lock-set-system-design-correction.md) 的 graph、canonicalizer、B/L schema；本文只替换 roots、preflight、已完成 acquisition/signature、tests-only 执行与接收顺序。旧 R 路由替换为 GitHub PR Code Review；不新增 R/agent。
- 中央已允许以 [QA 限定签名 ACCEPT](testing-loc-signatures-005-receipt.md) 和 [PR #2](https://github.com/yyczz1/FightMatch/pull/2#issuecomment-5928085467) 的 Completed/no-major-issues（head `1ac2081958d8cd91dd821d85ed7d071ec1310ee6`）继续设计；它们不是 M14 driver review、最终 material ACCEPT、许可证适用性或 restore 通过。

## 2. 固定输入与 actual-state 差异

`Tn=/private/tmp/fightmatch-loc-lic-alt-mNN/loc-lic-alt-yamldotnet-16.3.0-mNN`；`En=<repo>/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-mNN`；NN 使用两位数。表内均为实际 bytes / SHA-256。

| 固定对象 | bytes / SHA-256 |
| --- | --- |
| M10 `-preflight.tsv`（同目录完整文件名 `engineering-loc-license-alt-m-materials-continuation-10-preflight.tsv`） | 23812 / `b2e58332a0868ded905e3aae516c5e97ad3faea17c225e1de3a8a1f984634b16` |
| E13 `result.json` | 30391 / `53117ca8e44f113f3cf256b25d6da77c6d2cd241650d582e3b6f652f2ae9c481` |
| E13 `evidence-files.tsv` | 879 / `e424ab5f55763072c5e983a07cc839c26333e0b9db5382f62080ebb4f5d6559d` |
| E13 `acquisition-files.tsv` | 10380 / `4ae40c1194acd37e3c9db48f180ee58b1b6f9e9ab0410f7aa4557ff42f49e3ed` |
| E13 `authority/signature-handoff.json` | 31253 / `f0ac0e45c5058aa7e11dd0e00c67521c4ae7500fc72574f6d80ecc2e1cd18ed0` |
| T13 `preflight/m13-inputs.tsv` | 19832 / `93064be8f56eaaed29d00d10860fc10d5d70cddf99d847aaa48f3e2586d97f88` |
| QA `testing-loc-signatures-005-receipt.md` | 6856 / `c96c6579e9d2297dce6afa12a7b53273045b07b3dc8a219fcec5e06d4f8b6d70` |
| `cloud-signature-005-received/retrieved-outputs.json`（位于 E13 的父目录下） | 97891 / `0d44d64993a5db23ae6cc6b2a7a120c36b0f722ad628859a7026582e387b0db5` |

- M13 owner turn `01a0f68b-620a-7892-86d8-345252abe23a`；T13 只有 `acquisition/`、`preflight/`，`importedRunnerLeaves=0`。runner 必须直接取 M10 表冻结的 T09 集，不得从不存在的 M13 runner 推导。M13 `WAITING_SIGNATURE_REVIEW`/`SIGNATURE_PENDING` 原样保存，由新 receipt 追加限定门。
- M09 runner exact 23 nodes/56 edges/23 locks，两阶段 canonical 各 23 files/361970 B、aggregate `2a0d91800d00ad74d579b001611ff5ba306ed8691a6cecda6789f6bf068fdcd5`；其 raw、locks、markers、comparison、process provenance 逐项按 M10 表复用。M09 失败 tests lock/assets 不可导入。
- T09 `comparison/patched-source-base.tsv`：107129 B / `a1ab64b5ee977ea9d295fb141367891244a8ddc5b492597478332bb2ad647a79`（931 files）；T09 tests feed：28 nupkg/34904100 B、aggregate `91a1fd8a8b43f26ac11a7e3e96eb83d3033c82be9978a38a8da479d304fa3d74`。加上 E13 handoff 精确绑定的 SRM 1.6.0、Immutable 1.5.0、xunit.abstractions 2.0.3，形成唯一 31 包 feed；不下载第四包、不重新 GET 三包。

## 3. 三个新 root 与有限叶白名单

| root | 精确绝对路径 |
| --- | --- |
| T14 | `/private/tmp/fightmatch-loc-lic-alt-m14/loc-lic-alt-yamldotnet-16.3.0-m14` |
| E14 | `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m14` |
| M14 | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m14` |

首次准备前检查三者 absent；准备授权只创建 E14，执行授权才创建 T14；M14 在全部 material seal 门通过前保持 absent。以下 `{a,b}` 是有限枚举，`<project-dir>`/`<entry>` 只能来自已冻结 manifest，不授予任意目录写权限。

- E14 准备叶：`source/{m14_driver.py,m14-inputs.tsv,m14-write-set.tsv}`、`authority/{prepare,offline-checks,source-freeze}.json`。执行叶：`authority/{activation,preflight,signature-projections}.json`、`process/{tests-generation,tests-verification}.{json,stdout.txt,stderr.txt}`、`process/{completed-commands,not-run-commands}.tsv`、`{acquisition-files,feed-files,material-files,temp-files,evidence-files}.tsv`、`{network-ledgers,old-root-equality,result}.json`；清单自身不自哈希。
- T14 支撑叶：`preflight/{m14_driver.py,m14-inputs.tsv,m14-write-set.tsv}` 为冻结源的同字节副本；`acquisition/` 仅 E13 acquisition 清单逐叶复制及 §4 一份标准文本/receipt；`local-feed/<31 个确切 nupkg 文件名>`；`imported-runner/` 仅 M10 表所列 runner graph、locks、raw/canonical、markers、comparison及原 process 证据投影；全部目标 leaf 在 `m14-write-set.tsv` 展开。
- T14 两个 phase 为 `tests-generation/`、`tests-verification/`：各 `source/` 仅 931 源叶 + 13 个 project-dir 的 `packages.lock.json` 和 SDK Restore 约定的 `obj/{project.assets.json,project.nuget.cache,<csproj-name>.nuget.dgspec.json,<csproj-name>.nuget.g.props,<csproj-name>.nuget.g.targets}`；另有 `NuGet.Config`、`packages/`、`dotnet-home/`、`http-cache/`、`plugins-cache/`、`scratch/`、`tmp/`。包展开/SDK 隔离产物按固定 31 包 ZIP entries 和固定 SDK 的允许模式预列；无法列清的写入必须在 source freeze 前报主程，不能运行后追认。
- T14 `stage-material/` 与 M14 叶形一致：继承原 M §6 source/patch/feed/licenses/harness/identity schema；增加 M09/correction 的 runner/tests locks、raw/canonical、verification comparisons/markers，以及 `identity/sbom.json`、`verification/package-signatures/<三包 id.lower.version>.json` 和其本地 raw/provenance 引用。M14 是已校验 stage 的 create-once 字节副本；其余新叶须先修订白名单并重新冻结。
- M01–M13、005、旧 cache、原/FIX01/FIX02、八个生产路径、用户 dotnet/NuGet 路径只读，沿已冻结 preservation 清单比对；Config/Generated/Tools、产品、Assets/Packages/ProjectSettings、`.git` 不属于本执行写域。Git 上传由中央另行授权；不安装到 005，不创建 B/L roots。

## 4. license/signature 只补材料，不重新证明已接收门

- xunit 完整 snapshot 为 2357 B / `e9dcf94cc4864ba47aeaaec879cfabaf3feadb813c77af480c3ad14c6de0ed79`，commit `ffee51ac171a66896c42d00486bc792470ed6da2`、path `license.txt`；保留 `sourceCommitRole=license-snapshot-not-package-build`、`sourceTag=N/A_LICENSE_SNAPSHOT_COMMIT` 和 M13 lane。它含 Apache notice/link 与限定 MIT appendix，不是 Apache 九节标准全文；`rawHttpBytesVerified`、`xunitRemoteGitBlobVerified`、`xunitReproducibleBuildProven` 仍 false。
- M13 尚无可绑定的 Apache 标准全文。本方案申请执行阶段最多一次 GET `https://www.apache.org/licenses/LICENSE-2.0.txt`，host 仅 `www.apache.org`、HTTP 200、redirect/retry=0、≤60s、body≤128KiB，保存 `T14/acquisition/standard-licenses/{Apache-2.0.txt,Apache-2.0.receipt.json}`；核版本、九节及 appendix 完整性，记录真实 URL/headers/body bytes/SHA/时间。该项是 standard-text companion，适用性仍由 xunit snapshot 关联，不冒充包构建源或 commit-bound license；失败即停，不改 URL。
- 沿 M13 package/license/SBOM 三者统一键传递 snapshot 与新增 standard-text relationship；保留 Microsoft MIT/NOTICE 和原 M 的 Luban/YamlDotNet/NeoLua及六项 license gap 门。准备阶段用现有封存字节证明旧门可闭合；缺证据即 `BLOCKED_LICENSE_CLOSURE`，本包不开放额外 license/source 下载。
- 三个新 signature receipt 必须自标 `local-projection`，引用 QA verdict、observed receipt、原 tool item 与 stdout/stderr bytes/hash；核其 nupkg/signature/catalog SHA512 与 M13 同字节。每包 0 errors/8 warnings（NU3018×4、NU3028×4）及 fallback/离线吊销限制完整保留；不声明在线吊销通过或零警告。
- 云 005 的显式 env 遗漏 HOME/home/CODEX_HOME、错误计数字段、共享 signature 导出误配与后续修正均引用 QA 原回执，不改旧文件。未回收的云 `result.json`/`evidence-files.tsv` 只有 reported identity，不能被本地 projection 替代；以后只读取回不是 M14 restore 的前置。
- 新 ledger 分开列 imported Mac 9 requests/1764095 B、research 12 calls/serialized 393902 B（非 HTTP wire bytes）、云 GET 3/1731673 B、M13 新增 0、M14 Apache≤1/128KiB。证书链内部网络不能记为零；M14 restore 禁联网，验签/SDK下载调用数均 0。

## 5. 两阶段执行门：先准备源，再审查/授权执行

1. **准备授权后，仅离线准备**：读取 §2 与 M10 表冻结 inputs，派生 M14 inputs/write-set；M10 历史 root-absent 行改为 predecessor preservation，不运行已过期 M10 checker，也不把现场 hash 当历史 authority。核 M13/QA 原叶、runner/feed/source 明确集合、旧许可证、931 文件仅两处 YamlDotNet PackageReference patch，完成 strict JSON/manifest/闭包、正反 fixtures 与环境继承单测；不启动 dotnet。E14 driver 提供显式 `prepare`/`execute` 模式，prepare 不隐式执行后者。
2. **固定后交中央**：将实际写出的 driver/inputs/write-set bytes/SHA、只读来源映射、离线结果封入 `source-freeze.json`；中央把这些真实源文件上传新 LOC head 并发起 GitHub PR Code Review，然后决定授权必要 restore。不能要求“尚未写 driver 先有其 PR”；PR #2 不替代新 head 审查，源有变更即更新 head/freeze，未获对应执行授权不运行。
3. **执行 activation**：绑定中央授权、实际 thread/host/turn、freeze/head和精确 argv，核 T14/M14 absent、E14 仅许可准备叶、旧 roots与输入一致、两存储位置 free≥2GiB；创建 T14，完成唯一 Apache 归档及全部 license/signature/31包依赖门后再准入 restore。
4. **tests generation 一次**：两个独立 phase 均从 931 源叶新复制，packages/obj/cache/CLI/tmp 起始干净；共享只读 `T14/local-feed`。固定 SDK 对 `tests-generation/source/src/Luban.Tests/Luban.Tests.csproj` 做 correction §2.3 的 `msbuild -target:Restore -maxCpuCount:1 -nodeReuse:false -noAutoResponse -verbosity:minimal`；逐参数使用本 phase `RestoreConfigFile`、`RestorePackagesPath`、唯一 local feed `RestoreSources`，空 `RestoreFallbackFolders`，`RestoreDisableParallel=true`、`RestoreNoHttpCache=true`、`RestoreIgnoreFailedSources=false`、`RestorePackagesWithLockFile=true`、`RestoreForceEvaluate=true`、`NuGetAudit=false`。exact 13 nodes/29 edges/13 locks，退出 0、无 NU1101/NU1605，新增包闭包符合 M10 §4.3。
5. **tests locked verification 一次**：只复制 generation 的 13 lock 原字节到独立 verification source；argv 同上改 phase，去掉 ForceEvaluate、加 `RestoreLockedMode=true`。两次各≤180s；runner restore=0，tests=1+1，无共享 obj/单一全局 lock。每个项目 lock 字节相等；各 13 raw assets 留存，canonical 字节逐项目相等。
6. **封材料**：保留 T09 canonicalizer 核心算法，M14 仅做明确 root adapter，运行 inherited 正反 fixtures；generation exact marker absent、verification `/project/restore/restoreLockProperties/restoreLockedMode` 为 JSON boolean true 后仅去掉此 marker，其他 normalization 严格依 correction，raw 绝不改写。合并 runner 与 tests，逐项验证 §6，before/after old-root equality 后封 stage、create-once M14、封 E14。每一步 fail 即止，无同 root 修补/重跑/补包。

固定 local launcher：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/dotnet-x64/dotnet`，SDK 8.0.425/mac x64，SHA-256 `e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686`；Python `/opt/homebrew/bin/python3` 3.13.7，target SHA `a708f6e9f4803b806b29146c4e0feecfd9bf2d9eb60f3e15b850cd7cb56f200b`。不探测/切换/下载 SDK。

子进程环境必须先 `child_env = os.environ.copy()`，只 `update` 已列隔离键 `DOTNET_CLI_HOME,NUGET_PACKAGES,NUGET_HTTP_CACHE_PATH,NUGET_PLUGINS_CACHE_PATH,NUGET_SCRATCH,TMPDIR`（本 phase 目录）及 A01 `DOTNET_GENERATE_ASPNET_CERTIFICATE=false,DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false`。更新集合与 `{HOME,home,CODEX_HOME}` 必须不相交；逐键私下核父/子“是否存在及原值”完全相同，公开 receipt 只写相等布尔值，绝不输出完整 env，也不逐键赋值受保护变量。NuGet.Config 仅本地 source、clear fallback/disabled sources；网络禁止必须在执行合同中可验证，单纯 argv 不是网络为零证据。

## 6. 可观察验收与回传

- runner 23/56 与原 23 locks/raw/canonical/provenance 字节复用、tests 13/29 与 13 组新 lock/raw/canonical/marker 完整；31包 identities/selected entry/sha512/依赖组逐项目联结。`package-closure.tsv` 包含 graph+projectRelativePath；无旧 YamlDotNet.NetCore 或无依据丢失的边。
- 原 M §6 的 `upstream-source.tsv,source-files.tsv,patch.tsv,package-files.tsv,package-entries.tsv,package-closure.tsv,license-files.tsv,expected-deployment.tsv,official-test-project.tsv,commands.json,process-budget.json,temp-budget.json,old-roots.tsv,material-files.tsv` 全字段真实闭合；完整 SBOM 与 package/license 相互一致，expected-deployment 有限且覆盖 selected entries，不预填未来 build 字节。B/L commands 需 executable/hash/完整 argv/env/read/create/timeout/log/数字预算，不能直接使用 M09 finalize 的占位投影充作完成；不执行 B/L。
- 预算：执行 activation 起 wall≤30min（不含等待中央/PR）、temp≤768MiB、evidence≤16MiB、material≤512MiB/25000files；开始/结束 free≥2GiB；并发 restore=1、仅两棵 restore tree，顶层命令≤64、总 process starts≤20000、Python≤8；纯离线核验 helpers children=0，driver 仅可启动两次授权 restore。stdout/stderr 各≤4MiB、总日志≤64MiB并受 evidence 总额约束；超时只终止本任务 owned tree，保留首个失败与未运行项。
- 回主程一个 bounded receipt：actual owner/turn/authority、固定 head与源身份、0/1/1 restore argv/exit/时长、13/29/13比较、31包及 license/SBOM closure、签名限定门与环境偏差、网络/资源/old-root/三root seals、未运行项；状态仅 `READY_FOR_INDEPENDENT_M_ACCEPTANCE` 或最早 `BLOCKED_<CAUSE>`，作者不得自判 ACCEPT。主程/中央按新 head GitHub review 与 QA/材料证据作唯一 integration verdict；review未完成或有未闭合 finding 均不能最终 ACCEPT。
- 本文件完成标准：只形成上述 M14 delta并一次回中央（≤200字+路径），随即结束本轮；不触发后续 owner、不等待执行、不重验签名。编写采用 writing-for-agents 的阶段门与有限完成条件，防止准备阶段滑入执行。
