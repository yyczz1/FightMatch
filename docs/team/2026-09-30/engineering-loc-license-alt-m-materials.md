# LOC-LIC-ALT-M-01 — patched Luban M 材料取得与闭包冻结执行包

日期：2026-10-01（Asia/Shanghai）

状态：**AUTHORIZED_FOR_ASSIGNED_OWNER**

本文件是候选 M 执行包。当前回合只创建并封存本包，不执行其中任何命令。执行只可在
工程主程于新回合向 assigned owner 回传本文件最终 SHA-256 后开始；该回传是 activation
receipt，不扩大本文 scope。

## 0. 责任、authority 与交付边界

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与回传：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- architecture：`engineering-localization-license-alternative-architecture.md`；SHA-256
  `3dba4f92c17cc50fc158963857a69f70d308a8dab36dcbdf3a9fbbf821a06cb7`；
  681 lines / 49,110 B。
- system design：`engineering-localization-license-alternative-system-design.md`；SHA-256
  `18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`；
  881 lines / 59,206 B。
- research：`research-loc-a-license-sources.md`；SHA-256
  `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`；
  170 lines / 11,681 B。
- preparation：`engineering-loc-license-alt-m-materials-preparation.md`；SHA-256
  `0ef62293c53061f42bef202e031f0caa334a3b40a8892ff06ed20bb9f8095599`；
  354 lines / 32,486 B。
- architecture independent R：thread
  `01a0e404-e8ee-7310-8388-9260babd53f1`，turn
  `01a0f493-0083-7063-88e7-e9c17d65ae55`，final message
  `msg_0d29bf8d726d8dc0016abd96c5370487d0b6d99135bc6c7c1d`，verdict
  `ACCEPT`。该 verdict 只接受设计，不接受 M 材料或 runner。

M 的唯一职责是取得并冻结未来 B 所需的 exact official source/package/license bytes、
两行 patch、runner/test closure 与 locks、finite expected deployment mapping，以及 B/L
后续命令的 process/resource caps。M 不构建 runner，不运行测试，不运行 Luban／`005`，
不修改 `Program.cs` 或任何产品文件。

成功终点是 `READY_FOR_INDEPENDENT_M_R`，不是作者自判 `M-ACCEPTED`。独立 R 必须另给
唯一 `ACCEPT` / `NEEDS_FIX` / `REJECT`；只有主程固定该 receipt 后才能签 B。

## 1. 输入循环纠正

签包输入只包括：

1. 第 0 节四个固定文档及 architecture R receipt；
2. official Luban `v5.1.0` / commit
   `df34215d2e22035073d1af83d44e3ac0744dc3b3` 的现有只读 source tree；
3. 当前只读 `5.1.0` cache、005、LOC-A original/FIX-01/FIX-02 和八个生产路径；
4. 第 5 节的 exact direct versions、固定 YamlDotNet identity、官方 host policy、取得／
   验证算法和有限预算；
5. 三个 create-once roots 在执行瞬间均不存在。

以下项目当前不存在，且正是 M 要取得、解析、冻结的**输出或失败 gate**，不是执行前必须
已有的输入：

- `YamlDotNet 16.3.0` local nupkg/source/license/signature receipt；
- 五个 official-test direct seeds 的 local nupkg；
- 六个 explicit license gaps 的 commit-bound license bytes；
- runner/test locks 与完整 transitive/analyzer/build/native/contentfiles/testhost closure；
- finite expected package/path/entry/license mapping；
- 本次网络实际 redirect/bytes/hash、命令/process/temp actual receipts；
- 新 temp/material/evidence roots。

M 必须在本文预算内形成这些输出。不能闭合时返回 `BLOCKED`；不得要求主程先提供 M 本应
产生的 bytes，不得扩大 host/source/version，不得把未来 B publish output 反写成 M 输入。

## 2. exact roots 与写白名单

### 2.1 本次允许写入

执行开始前以下三个 exact roots 都必须不存在：

```text
/private/tmp/fightmatch-loc-lic-alt-m01/loc-lic-alt-yamldotnet-16.3.0-m01/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m01/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m01/
```

允许写入的完整集合仅为：

1. 上述 M temp root；
2. 上述 M material root；
3. 上述 M evidence root。

material root 只在所有 material gates 通过后由 temp create-once 复制；目标已存在即
`BLOCKED_ROOT_EXISTS`。失败发生在 material root 创建前，只保留 temp/evidence；创建后
失败则原地标记 `REJECTED_NOT_ACCEPTED`。任何 retry 使用新 packet 和新 sibling suffix。

### 2.2 三个 root 的固定结构

```text
<M_TEMP>/
  source-official/
  source-patched/
  feed-runner/
  feed-tests/
  acquisition/
  licenses/
  closure/runner/
  closure/tests/
  harness/
  identity/
  logs/

<M_MATERIAL>/
  source-official/
  source-patched/
  patch/
  feed-runner/
  feed-tests/
  licenses/
  harness/
  identity/

<M_EVIDENCE>/
  authority/
  before/
  acquisition/
  closure/
  process/
  comparison/
  after/
  result.json
  evidence-files.tsv
```

### 2.3 永久只读与零写入

以下范围只读：

- `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/`；
- `TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/`；
- LOC-A original、`-fix-01`、`-fix-02` 三棵 evidence roots；
- 第 0 节四个文档、`AGENTS.md`、`.agent/**`；
- 八个生产路径、`Assets/**`、`Packages/**`、`ProjectSettings/**`、`.git/**`。

M 对仓库产品／文档写入为 `0`。禁止创建 B/L roots、final runner root、branch、commit、
worktree、`.meta`、generated `.csproj/.sln`、Unity `Library/Temp/Logs/UserSettings` 内容。

## 3. 固定工具、source 与 old-oracle 输入

| 输入 | exact identity / path |
| --- | --- |
| SDK | `.NET SDK 8.0.425`；dotnet `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/dotnet-x64/dotnet`；SHA-256 `e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686` |
| Luban source root | `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source/`；clean tag `v5.1.0` / commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`；Git tree `a8f8243ef84b2d11156f869af76061863ffb6d6b` |
| Luban.csproj | `src/Luban/Luban.csproj`；4,201 B；SHA-256 `5a29bc8a18a2eae222d412ac4f2485ae366ffd7146cc2c1bf397e5554dde30c9` |
| DataLoader csproj | `src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj`；1,811 B；SHA-256 `0f84ba758efa3ec4b8f878abf3b0429f34cc23bbf9a01f81c164878658839516` |
| DataTarget csproj negative control | `src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj`；1,669 B；SHA-256 `935771a2b429ec8c0490cf88f022d8c31d5d2af4a73ad76fe627fe5f814a2dc8` |
| official test project | `src/Luban.Tests/Luban.Tests.csproj`；2,542 B；SHA-256 `d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1` |
| Luban license | `source/LICENSE`；1,088 B；SHA-256 `a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841` |
| old runner oracle | `.../5.1.0/runner/`；132 files / 5,969,118 B；manifest `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` |
| old lock | `.../5.1.0/identity/packages.lock.json`；14,103 B；SHA-256 `3231b50eccd462dd529fb3d0e67c44d62ca8c3363af8367e086ed6b5f5081194` |
| 005 runner manifest | `TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/identity/runner-files.tsv`；12,674 B；SHA-256 `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` |
| 005 commands | `TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/commands.json`；51,338 B；SHA-256 `f098f03407019eae78340c8240a59ea5e8f20fe0de1a442f8101d597397a8688` |
| FIX-02 inputs | `...-fix-02/inputs.json`；2,961 B；SHA-256 `c191f93674f74199434c39a2c31b8a34e08e6a3925dca39dcec5d64a35af3582` |
| FIX-02 inventory | `...-fix-02/third-party-license-inventory.json`；78,477 B；SHA-256 `1bfeb05a91d247738b7ef36b5bdbb86b2526e6868ed5737b791f1288b23a352f` |
| FIX-02 commands | `...-fix-02/commands.json`；38,972 B；SHA-256 `803a5f686d5999c8b7bd515227fa7cd39b6703f553ee6e6fe30aff008e52cb85` |

执行前逐项重算；任一 mismatch 都在写 material root 前返回
`BLOCKED_AUTHORITY_DRIFT`。旧 inventory 的 `.nuspec` license fallback 是 gap evidence，
不是可沿用的 acceptance。

## 4. official network policy

### 4.1 允许的 host 与 path class

所有网络读取必须由 acquisition driver 逐请求记录。允许的 host 只有：

1. `api.nuget.org`
   - `/v3/index.json`；
   - `/v3-flatcontainer/<lower-package-id>/<exact-version>/...`；
   - `/v3/registration5-semver1/<lower-package-id>/<exact-version>.json`；
   - exact registration response 指向的 `/v3/catalog0/...` entry。
2. `github.com`
   - fixed repositories `focus-creative-games/luban`、`aaubry/YamlDotNet`、
     `neolithos/neolua` 的 exact tag/full-commit pages；
   - 对其它 resolved component，仅允许 exact nupkg/nuspec/catalog 的 repository metadata
     明确绑定的 repository 与 full commit/tag。
3. `raw.githubusercontent.com`
   - 仅允许上一项 exact repository + full commit + exact file path。
4. `codeload.github.com`
   - 仅允许上一项 exact repository + full commit/tag 的 archive。

redirect 的每一 hop 仍须命中以上 host policy；任何搜索引擎、镜像、mutable branch、
unbound fork、package 作者主页、通用 license template 或其它 host 均
`BLOCKED_NETWORK_SOURCE`。不联系 publisher/maintainer，不使用付费服务。

### 4.2 request receipt 与有限预算

每个实际 request 写一条 `acquisition/network-requests.jsonl`：

```text
requestId, phase, method, requestedUrl, redirectChain[], statusCodes[],
responseHeadersSha256, contentBytes, contentSha256, destinationRelativePath,
startedAtUtc, durationMs, exitCode
```

预算上限：

- request 总数 `≤256`；
- 单 request redirect hops `≤3`；
- 单 response body `≤64 MiB`；
- aggregate transferred body bytes `≤384 MiB`；
- 单 request timeout `≤120 s`；
- acquisition phase wall clock `≤25 min`；
- requests 顺序执行；network concurrency `1`。

已知固定 bytes/hash 仍须严格匹配；未知 response bytes/hash 由 M actual receipt 冻结，
不是本包签发前置。超预算或 redirect/source drift 立即停止，不加 host、不提高预算。

## 5. package、source 与 license resolution contract

### 5.1 fixed roots 与 direct versions

| component | exact version / authority |
| --- | --- |
| Luban | official `v5.1.0` / `df34215d2e22035073d1af83d44e3ac0744dc3b3` |
| YamlDotNet | exact `16.3.0`；tag commit `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73` |
| NeoLua | exact `1.3.14`；tag commit `cb560afef9dd19b7afad6da594b44909c9739698` |
| ExcelDataReader | exact `3.7.0` |
| Google.Protobuf | exact `3.29.0` |
| MessagePack | exact `3.1.7` |
| MessagePack.Annotations | exact `3.1.7` |
| NLog | exact `5.3.4` |
| Scriban | exact `7.2.5` |
| Microsoft.NET.Test.Sdk | exact `17.11.1` |
| Newtonsoft.Json test seed | exact `13.0.3`；不得替换成 runner 的 13.0.1 |
| System.Text.Encoding.CodePages | exact `8.0.0` |
| xunit | exact `2.9.2` |
| xunit.runner.visualstudio | exact `2.8.2`；保留 source 的 IncludeAssets/PrivateAssets semantics |

禁止 `latest`、floating version、自动升级、第二个 YamlDotNet version、17.x/18.x 回退、
source shim 或 package fallback。

### 5.2 YamlDotNet fixed identity

M 必须取得并验证：

- nupkg URL
  `https://api.nuget.org/v3-flatcontainer/yamldotnet/16.3.0/yamldotnet.16.3.0.nupkg`；
  776,880 B；SHA-256
  `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4`；
  SHA-512
  `f102fb418c3afe5ec0cc5d0336b9ac6517af6f30603ab5f925a7b3622544c419bb935d1e55901147fff5a113874e55491534d5553411fb4ea198e6550c0421df`；
- `.signature.p7s` SHA-256
  `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308`，并由 fixed SDK
  `dotnet nuget verify --all` 离线验证；entry present 不等于 verified；
- `lib/net8.0/YamlDotNet.dll`：293,376 B；SHA-256
  `86f873f682c619a0c24e263dcdd15fe027104666641c92f503516671d61edda6`；
- exact source commit/tag 与实际 archive receipt；研究 capture hash只作对照，commit/tree
  才是 source authority；
- `LICENSE.txt`：1,110 B；SHA-256
  `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3`；
- `LICENSE-libyaml`：1,077 B；SHA-256
  `cb65f46df0d9125ba60862117200ec6631fef3a8169cd0b4513ffcad4ef5cb9b`。

任一固定值不匹配即 `BLOCKED_YAMLDOTNET_IDENTITY`；不能重选版本。

### 5.3 transitive resolution algorithm

1. 从 exact csproj/project references 与已取得 exact nupkg nuspec dependency groups 开始；
   target framework 固定 `net8.0`。
2. direct versions 使用第 5.1 节 exact values；transitive version ranges 使用 fixed SDK
   NuGet SemVer2 resolution semantics。记录 declared range、selected exact version、选择规则
   与 parent edge。
3. acquisition driver只请求满足已声明 dependency range 的 immutable registration/catalog/
   flatcontainer records；不得枚举或试跑无关版本。遇到 floating/latest、unbounded选择无法
   按 NuGet规则唯一化、prerelease漂移或 source冲突时 `BLOCKED_VERSION_RESOLUTION`。
4. 所有 nupkg 先进入 temp acquisition；逐项核 catalog SHA-512、archive SHA-256、nuspec、
   signature（若有）、entries 后才复制到 local feed。
5. runner/test 两个 closure 使用独立 local feeds/locks。`dotnet restore` 只可离线读取这些
   feeds和隔离 package roots；NuGet config必须 `<clear/>` 后只列 exact local feed，禁用
   fallback folder、HTTP cache复用和远程 source。
6. runner/test lock 首次生成后，在第二个空 isolation root 各做一次 `--locked-mode`
   closure verification。M 全阶段仍是 candidate build/publish/test `0`。
7. restore 产生的 lock、`project.assets.json` 与 nupkg entry manifests共同冻结 finite
   expected deployment mapping；不得读取 future B publish output或 runner manifest。

closure 无法在预算内闭合即 `BLOCKED_MATERIAL_FREEZE`；不得扩大 source/version。

### 5.4 license closure

每个 runner/test direct/transitive/build/analyzer/native/contentfiles/testhost component 都必须
有 exact official source URL、tag/full commit、license expression、source path、raw bytes、
SHA-256 和 package relationship evidence。`.nuspec` 只作 metadata，永远不能成为
`licenseSource`。

六个显式 gaps 是：`ExcelDataReader 3.7.0`、`Google.Protobuf 3.29.0`、
`MessagePack 3.1.7`、`MessagePack.Annotations 3.1.7`、`NLog 5.3.4`、
`Scriban 7.2.5`。`MessagePack.Annotations` 必须有独立 relationship evidence。

NeoLua license 必须绑定 exact tag/commit 的 Apache-2.0 `LICENSE.md`：11,324 B，SHA-256
`a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae`。

发现新的 gap 是 M 正常解析输出；能按 official policy 在预算内闭合就加入 manifests，
否则 `BLOCKED_LICENSE_CLOSURE`。不能用熟悉的 license 模板、组织名或相邻 package 推断。

## 6. 两行 patch material

在 temp 的 exact official source copy 中只生成以下两处 replacement：

```diff
--- a/src/Luban/Luban.csproj
+++ b/src/Luban/Luban.csproj
-    <PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />
+    <PackageReference Include="YamlDotNet" Version="16.3.0" />

--- a/src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj
+++ b/src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj
-    <PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />
+    <PackageReference Include="YamlDotNet" Version="16.3.0" />
```

完成标准：两个 target preimages 匹配第 3 节；每个文件恰好一处 replacement；整个 tracked
source diff 只含这两行；零 `.cs`、solution、test、target framework、其它 package 或
template change。保存 `patch/patch.diff`、patch SHA、两个 postimage bytes/SHA 和完整
source before/after manifests。任何额外 diff 为 `BLOCKED_PATCH_SCOPE`。

## 7. exact manifest 与 receipt schema

所有 TSV 使用 UTF-8/LF、header、ordinal path sort；路径为 material/evidence root relative
POSIX path。所有 SHA-256 为 64 位小写十六进制。零 glob、零“其余依赖”、零 unknown。

| file | required columns / fields |
| --- | --- |
| `identity/material-files.tsv` | `relativePath,bytes,sha256,kind,authorityId,sourceReceiptId` |
| `identity/upstream-source.tsv` | `component,versionOrTag,repositoryUrl,fullCommit,treeId,transportUrl,transportBytes,transportSha256,localRoot,fileManifestSha256` |
| `identity/source-files.tsv` | `component,relativePath,bytes,sha256,gitObjectId,role` |
| `identity/patch.tsv` | `targetRelativePath,preBytes,preSha256,postBytes,postSha256,oldPackageId,oldVersion,newPackageId,newVersion,patchSha256` |
| `identity/package-files.tsv` | `graph,packageId,version,relativePath,bytes,sha256,catalogSha512,catalogUrl,immutablePackageUrl,owner,signaturePath,signatureSha256,signatureVerdict` |
| `identity/package-entries.tsv` | `graph,packageId,version,entryPath,assetKind,targetFramework,bytes,sha256,selected,expectedDeploymentPath` |
| `identity/package-closure.tsv` | `graph,parentId,parentVersion,packageId,version,declaredRange,resolutionRule,dependencyKind,targetFramework,includeAssets,privateAssets,lockEdge` |
| `identity/license-files.tsv` | `component,version,expression,sourceUrl,sourceTag,sourceCommit,sourcePath,localRelativePath,bytes,sha256,packageRelationshipEvidence` |
| `identity/expected-deployment.tsv` | `ownerKind,ownerId,ownerVersion,sourceEntryOrProject,expectedRunnerRelativePath,assetKind,licenseRelativePath,licenseSha256`；candidate bytes/hash留给 B，不预填 |
| `identity/official-test-project.tsv` | `projectRelativePath,bytes,sha256,targetFramework,referenceKind,referenceId,versionOrPath,includeAssets,privateAssets` |
| `identity/runner.packages.lock.json` | candidate runner exact lock |
| `identity/tests.packages.lock.json` | official-test exact lock |
| `identity/commands.json` | `schemaVersion,commands[]`；每项 `id,stage,purpose,executableAbsolutePath,executableSha256,argv,cwd,env,networkPolicy,expectedExit,timeoutMs,killGraceMs,stdoutMaxBytes,stderrMaxBytes,reads,creates` |
| `identity/process-budget.json` | `schemaVersion,maxTopLevelCommandCount,maxTopLevelProcessTreeCount,maxConcurrentTopLevelTrees,maxTotalProcessStarts,commands[],executables[]` |
| `identity/temp-budget.json` | `schemaVersion,wallClockMs,roots[]`；每 root `absolutePath,maxBytes,maxFiles,freeSpaceFloorBytes,logCapBytes,ownerStage` |
| `acquisition/network-requests.jsonl` | 第 4.2 节逐 request fields |
| `identity/old-roots.tsv` | `root,relativePath,bytes,sha256,phase`；phase=`before|after` |
| `result.json` | `schemaVersion,status,owner,authority,roots,manifestSha256,completedCommands,notRunCommands,blockers,oldRootEquality,zeroActionReceipt,returnTo` |

`material-files.tsv` 必须精确覆盖 material root 除自身 seal files 外的每个 regular file；
`evidence-files.tsv` 同理覆盖 evidence root。symlink、socket、device、FIFO 全部禁止。

## 8. M process、time、storage 与 network budget

这些是可执行上限；actual 值由 receipts 封存。未知 actual 不是签包前置，但 actual 超上限
立即 `BLOCKED_BUDGET_EXCEEDED`。

### 8.1 stage ceilings

| stage | max top-level commands | max wall | max log bytes |
| --- | ---: | ---: | ---: |
| preflight / before manifests | 8 | 5 min | 8 MiB |
| acquisition / provenance | 8 driver invocations | 25 min | 16 MiB |
| closure-only restore/probe | 24 | 20 min | 24 MiB |
| verify / copy / seal / after manifests | 16 | 10 min | 16 MiB |
| **total** | **56** | **60 min** | **64 MiB** |

全阶段：`maxTopLevelProcessTreeCount=1`，`maxConcurrentTopLevelTrees=1`，
`maxTotalProcessStarts=20000`。任何时刻只运行一个 top-level tree；该树内 child 仍计数。

### 8.2 executable caps

| executable identity | max starts | max concurrent | purpose |
| --- | ---: | ---: | --- |
| `/bin/zsh` | 56 | 1 | top-level drivers only |
| `/usr/bin/curl` | 256 | 1 | recorded official acquisition only |
| fixed SDK `dotnet` from第 3 节 | 128 total，其中 top-level restore/verify `≤24` | 8 | closure-only restore、NuGet signature verify及其 MSBuild children |
| `/usr/bin/git` | 32 | 1 | local source/tag/tree/status/object verification only；network disabled |
| `/usr/bin/shasum` | 6000 | 16 | file manifests |
| `/usr/bin/stat` | 6000 | 16 | bytes/type receipts |
| `/usr/bin/find` | 128 | 1 | closed-set enumeration |
| `/usr/bin/sort` | 128 | 1 | deterministic manifests |
| `/usr/bin/awk` | 256 | 4 | manifest checks |
| `/usr/bin/sed` | 256 | 4 | exact metadata extraction；不用于 broad source rewrite |
| `/usr/bin/grep` | 256 | 4 | gates |
| `/usr/bin/jq` | 512 | 4 | JSON validation/receipts |
| `/usr/bin/unzip` | 512 | 4 | nupkg entry extraction |
| `/usr/bin/tar` | 64 | 2 | exact source archive extraction |
| `/bin/cp` | 256 | 4 | create-once staging/copy |
| `/bin/mkdir` | 128 | 4 | whitelisted roots only |
| `/usr/bin/cmp` | 256 | 4 | equality gates |
| `/usr/bin/diff` | 128 | 2 | patch/source diff receipts |
| `/usr/bin/wc` | 256 | 4 | counts |
| `/usr/bin/tr` | 256 | 4 | receipt normalization |

任何未列 executable，或 `csc`、VBCSCompiler、VSTest、testhost、Luban、Unity、Java、Gradle
进程出现，立即 `BLOCKED_PROCESS_DRIFT`。restore不得隐式 build/test。

### 8.3 storage ceilings

- M temp root：`≤1 GiB`，`≤50,000` regular files；
- M material root：`≤512 MiB`，`≤25,000` regular files；
- M evidence root：`≤128 MiB`，`≤10,000` regular files；
- aggregate logs：`≤64 MiB`；单 stdout/stderr file `≤4 MiB`；
- start free-space floor：`2 GiB`；执行期间持续 floor：`1 GiB`；
- 超任一上限先写 blocker receipt并停止；不删除已创建的新 roots来“恢复预算”。

## 9. ordered execution

每一步都以 completion criterion 收口；失败时跳到第 11 节，不继续依赖步骤。

### Step 1 — authority、root 与 process preflight

1. 重算第 0、3 节所有固定 inputs；记录 assigned owner、activation receipt和 tool hashes。
2. 断言三个新 roots 不存在、free space满足第 8.3 节、没有 Unity进程占用本项目。
3. 创建 temp/evidence roots；material root暂不创建。
4. 写 `authority.json`、budget manifests与 initial `result.json`。

完成标准：固定输入全匹配、新 roots precondition成立、实际 process allowlist可执行；否则
`BLOCKED_PREFLIGHT`，且 material root不存在。

### Step 2 — old roots/cache before manifests

对以下 read-only roots形成递归 path/type/bytes/SHA closed-set manifests：old `5.1.0` cache、
005、LOC-A original/FIX-01/FIX-02。另记录八个生产路径 bytes/SHA 与 protected
`Assets/Packages/ProjectSettings` status hash。禁止跟随 root 外 symlink。

完成标准：old fixed counts/hashes与第 3 节及 accepted designs一致；所有 before manifests
写入 evidence。否则 `BLOCKED_OLD_ROOT_DRIFT`。

### Step 3 — official source copy 与 two-line patch

从 clean commit-bound source只读复制到 temp `source-official`，生成 source manifest；再复制
到 `source-patched` 并执行第 6 节 exact replacement。保留完整 before/after/diff receipts。

完成标准：official copy 与 Git tree/files相符；patched copy只有两个 PackageReference
line replacements。否则 `BLOCKED_PATCH_SCOPE`。

### Step 4 — fixed direct acquisition

按第 4 节逐请求取得并验证 YamlDotNet 16.3.0、五 test direct seeds以及 runner exact direct
packages。现有 old cache可作为 bytes对照，但所有 material copy必须进入新 temp并独立重算。
取得 Luban/YamlDotNet/NeoLua commit-bound source/license inputs和 package metadata。

完成标准：所有 fixed direct versions有 nupkg/catalog/nuspec/signature/entry receipts；Yaml
固定值全匹配；没有 latest/fallback/old Yaml package进入 candidate feeds。

### Step 5 — dependency expansion 与 license acquisition

从 exact nuspec/project graph递归展开 runner/test dependencies，按第 5.3 节选择并取得每个
exact package。为六 explicit gaps和所有新发现 components取得第 5.4 节 license closure。
每个 request和版本选择都写 receipt。

完成标准：待验证 local feeds包含有限、逐项 exact package set；每个 package/source/license
关系闭合。任何范围、来源或许可证不能闭合即 `BLOCKED_MATERIAL_FREEZE` 或
`BLOCKED_LICENSE_CLOSURE`。

### Step 6 — offline closure-only restore/probe

使用 isolated temp roots、local-only NuGet configs和 fixed SDK：

1. runner lock generation restore；
2. tests lock generation restore；
3. new empty runner isolation root的 `--locked-mode` verification restore；
4. new empty tests isolation root的 `--locked-mode` verification restore。

若 dependency expansion需要有限迭代，可在 24-command stage cap内重复失败 probe后取得其
明确缺失 exact dependency；每次失败都保留 receipt。任何 probe访问网络、compile、build、
test或启动未列 child立即阻断。

完成标准：两个 locks与两次 locked verification成功；完整 closure、assets、testhost/
analyzer/build/native/contentfiles分类稳定；candidate build/publish/test均为零。

### Step 7 — finite mapping 与 future command budgets

从 locks、`project.assets.json`、nupkg entries、source project graph和 fixed harness evidence
生成：

- runner expected package/project → expected deployed relative path/entry → license mapping；
- official tests完整 assets/fixtures/adapter/testhost manifest；
- B restore/publish/official-test/YAML/UnityAsset/005 parity command DAG与numeric caps；
- L compiler/self-tests/negatives command DAG与numeric caps。

不得填 candidate runner file bytes/hash/count、未来 actual publish output或B/L verdict。

完成标准：所有 mapping有限、exact、无 wildcard/unknown；B只需对两个 actual outputs做集合
等值/exactly-once核验。如果无法在不构建/测试情况下冻结，`BLOCKED_PROCESS_MODEL`。

### Step 8 — material create-once、after equality 与 seal

1. 对 temp material closed set运行全部 schemas与集合等值检查；
2. 仅在通过后创建 material root并复制 approved files；重算 material manifests；
3. 重算 old roots/cache、八生产路径和 protected status；与 before逐项相等；
4. 写 evidence manifests和 `result.json`，确认 processes静默；
5. 返回主程，等待独立 M R；不启动 B/L。

完成标准：material status `CANDIDATE_NOT_ACCEPTED`；result status
`READY_FOR_INDEPENDENT_M_R`；oldRootEquality=true；zero-action receipt全部为零。

## 10. zero-action receipt

`result.json.zeroActionReceipt` 必须逐项写整数：

```text
candidateBuildCount: 0
candidatePublishCount: 0
candidateTestCount: 0
lubanInvocationCount: 0
preflight005InvocationCount: 0
unityInvocationCount: 0
programCsWriteCount: 0
productionPathWriteCount: 0
bRootCreateCount: 0
lRootCreateCount: 0
gitWriteCount: 0
```

closure-only `dotnet restore` 不计为 build，但其 actual top-level/child counts必须进入 process
receipt。任何 zero-action field非零都使 M 失败。

## 11. BLOCKED、保全与 return contract

任一 earliest gate 失败后：停止全部依赖步骤；结束当前 process tree；保存已经写入的新
temp/evidence/material bytes；不删除、不覆盖、不改 old roots；不创建 B/L roots。

`result.json` 必须包含：

```text
status: BLOCKED_<CODE>
failedGate: <single earliest gate>
expected: <exact expected or allowed policy>
observed: <exact observed or MISSING>
completedCommands: <ordered command IDs + exit/timing/process counts>
notRunCommands: <ordered dependent command IDs>
networkActual: <request/redirect/bytes/hash receipt summary>
writtenRoots: <exact roots + sealed/partial state>
oldRootEquality: <true|false|not-run>
zeroActionReceipt: <all fields>
returnTo: 01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local
```

合法 code：`BLOCKED_AUTHORITY_DRIFT`、`BLOCKED_ROOT_EXISTS`、`BLOCKED_PREFLIGHT`、
`BLOCKED_OLD_ROOT_DRIFT`、`BLOCKED_NETWORK_SOURCE`、`BLOCKED_NETWORK_DRIFT`、
`BLOCKED_NETWORK_BUDGET`、`BLOCKED_VERSION_RESOLUTION`、
`BLOCKED_YAMLDOTNET_IDENTITY`、`BLOCKED_PACKAGE_SIGNATURE`、
`BLOCKED_PATCH_SCOPE`、`BLOCKED_MATERIAL_FREEZE`、`BLOCKED_LICENSE_CLOSURE`、
`BLOCKED_PROCESS_MODEL`、`BLOCKED_PROCESS_DRIFT`、`BLOCKED_BUDGET_EXCEEDED`、
`BLOCKED_SEAL_MISMATCH`。

失败 root 原地保留并标明 `FAILED_EVIDENCE`／`REJECTED_NOT_ACCEPTED`。不能通过改版本、
新增 host、放宽 hash、借用 `.nuspec` license、运行 build/publish/test或删除失败材料继续。

## 12. M acceptance checklist 与输出合同

交独立 M R 前，delivery owner逐项报告：

- [ ] authority、owner、activation receipt、SDK/source/cache/old evidence全部匹配；
- [ ] 三个 roots create-once，写入 closed set未越界；
- [ ] official source manifest与two-line patch exact，零额外 source diff；
- [ ] YamlDotNet 16.3固定 package/source/signature/DLL/two licenses全部匹配；
- [ ] NeoLua commit-bound license和六 explicit gaps全部闭合；
- [ ] runner/test完整 package/license/assets closures与两个 locks冻结；
- [ ] 五 direct test seeds及所有 transitives/analyzers/build/native/contentfiles/testhost逐项列明；
- [ ] finite expected-deployment mapping完整，旧 Yaml package/old-only System edges为零；
- [ ] B/L future commands和process/resource caps冻结，未读取future publish output；
- [ ] actual requests/process/time/temp/log均在第 4、8 节上限内；
- [ ] old roots/cache/八生产路径/protected status before-after相等；
- [ ] material/evidence manifests closed-set相等，零 symlink/special file；
- [ ] zero-action receipt全部为0；
- [ ] `result.json.status=READY_FOR_INDEPENDENT_M_R`，作者未写 `M-ACCEPTED`。

回传必须包含：changed/new roots、每个 manifest path/bytes/SHA、fixed materials摘要、完整
blocker list（成功时空数组）、actual network/process/storage/wall totals、old-root equality、
zero-action receipt、delivery owner thread/host/turn和主程 return path。

独立 R 审查 material bytes和M evidence后给唯一 verdict。`ACCEPT` 只形成可供 B 签包引用
的 `M-ACCEPTED` authority；不会自动启动 B，也不会授权 L、LOC-B、Unity、Windows、
Android、Host、RES或release。

## 13. 本回合未执行声明

本包创建回合只新建本文件。没有创建三个 M roots，没有网络请求、download、restore、
package resolution、patch material、build、publish、test、Luban、`005`、Unity、产品写入、
外盘写入或 Git 写入。实际执行等待主程以本文件最终 SHA 向 assigned owner发出 activation
receipt。
