# LOC-LIC-ALT-M-03 — 单节点 MSBuild 材料闭包续行包

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目标、责任与结论边界

本包是 M02 在 runner lock generation 因受限环境无法创建 MSBuild out-of-process
named-pipe nodes 后的 **new sibling continuation**。它不修改、覆盖或补写 M01/M02；先重验
M01、Linux004/R 与 M02 blocked seals，再以显式 single-node `dotnet msbuild -t:Restore`
替换 M02 的 `dotnet restore` process model，继续尚未完成的 package/license closure、两个
locks、两次 locked-mode verification、finite deployment mapping、future B/L budgets 与
material seal。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 基础合同：`engineering-loc-license-alt-m-materials.md`，582 lines / 30,837 B /
  SHA-256 `0ce7c44844494e8b63c5e6c149fa357932c11c9a53457275a24a16ac4ef48d13`。
- M02 continuation：`engineering-loc-license-alt-m-materials-continuation-02.md`，205 lines /
  11,834 B / SHA-256
  `402ab00f2f1fa67e189e0a49fc42a97bfda8eec4c59d7e50777c0bd211d7b5f0`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已允许同级续行；不授权 B/L、Git、
  Unity、产品写入、build/publish/test、外部人工消息或受限环境外执行。

除本文明确替换的 sibling roots、M02 process adaptation、process receipts 与累计 actual 外，
M02 及基础合同的 source/version/host、两行 patch、schema、license、signature adjudication、
zero-action、失败保全和验收条款全部继续适用。成功终点仍只能是
`READY_FOR_INDEPENDENT_M_R`；material 标记 `CANDIDATE_NOT_ACCEPTED`。作者不得自判
`M-ACCEPTED`，不得自动进入 B/L。

## 1. 固定 predecessor 与问题定界

### 1.1 M01 与 Linux004/R

M01 temp/evidence、fixed direct material、42 requests / 23,259,536 body bytes、Mac signature
failure history，以及 Linux004/R 对 exact YamlDotNet 16.3.0 package-signature gate 的限定
`ACCEPT`，全部按 M02 §§1、3 原样复核和重放。signature verdict 仍只能是
`VERIFIED_LINUX_EXIT0_REVOCATION_UNKNOWN`；不得声称当前未吊销、在线 revocation 成功或
整 M 已接受。禁止重跑 Mac/Linux signature verify、下载 SDK、修改 trust 或重取已匹配
direct bytes。

### 1.2 M02 blocked evidence

M02 roots：

```text
/private/tmp/fightmatch-loc-lic-alt-m02/loc-lic-alt-yamldotnet-16.3.0-m02/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m02/
```

执行前必须逐项重算以下 fixed seals：

| item | actual identity |
| --- | --- |
| M02 `result.json` | 3,647 B；SHA `06bf3b7d330ed09e6836710c8797b2b6b198a00d752762e86562ec3d72367faa`；status `BLOCKED_PROCESS_MODEL` |
| M02 `evidence-files.tsv` | 127 lines / 18,185 B；SHA `9065599e45aa5e29a00c1ae8d6153c9b28cfe0538f0d186220a3b9925bbc9a57` |
| M02 `process/runner-lock-generation.json` | 3,561 B；SHA `73e243580aeb505e8b2c0ffeb86807ee02997470a9f13a84454498950b1a05ae` |
| M02 `process/m02-completed-commands.tsv` | 5,599 B；SHA `93e319f7c159a4a6d48c63ed97bbf92265503073790aab0a0e100603cbbeddb7` |
| M02 temp manifest | 2,159 files / 93,114,157 B；SHA `bc0b7752acbffa0a1de3714b2f8e5bc1cde770124927af3abc4aa317782d6c71` |
| M02 node failure manifest | 90 files / 232,380 B；SHA `89fe2eb01ed5ea32548c369be6c73dc75ba83f29512a72ad647cbce6bac3aa4a` |

M02 的唯一 restore：1 top-level fixed-SDK dotnet + 90 unique out-of-process node PIDs，exit 1；
90 个 failure receipts 均为 `SocketException (13): Permission denied` at
`NamedPipeServerStream` / `NodeEndpointOutOfProcBase.InternalConstruct`。runner lock 缺失，M02
network 为 0，material root absent，所有 dependent stages not run。M02 old/cache/005/LOC/M01/
production/protected before-after 相等；M02 zero-action 15 项全 0。

该证据只证明 M02 的 multi-node process model 与当前受限执行环境不兼容。它不证明 fixed
Mac SDK、offline restore、package graph、license closure 或 macOS 平台本身无效；M02 的
79-byte `CSSM_ModuleLoad` stderr 也不得单独扩大为根因结论。

005 `commands.json` 的 closed-set strings 中没有 `dotnet restore`、`msbuild` 或 restore
invocation；005 只继续作为旧 runner/command oracle，不能被引用为“同一 SDK 曾成功
restore”的证据。M03 的 single-node command 是对 M02 实际 failure chain 的新适配。

### 1.3 process adaptation authority

以下官方文档固定本包的最小适配：

- Microsoft Learn `dotnet msbuild`：该命令提供完整 MSBuild，参数与 MSBuild command-line
  client 相同；
  <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-msbuild>
- MSBuild command-line reference：`-maxCpuCount:1` 将 concurrent MSBuild processes 限为
  1，`-nodeReuse:false` 禁止完成后保留 node；
  <https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference>
- NuGet MSBuild restore target：`-t:Restore` 只运行 restore target；
  `RestoreConfigFile`、`RestorePackagesPath`、`RestoreDisableParallel`、
  `RestoreNoHttpCache`、`RestoreIgnoreFailedSources`、`RestorePackagesWithLockFile`、
  `NuGetLockFilePath`、`RestoreForceEvaluate` 与 `RestoreLockedMode` 是正式 properties；
  <https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets>
- PackageReference lock docs：lock generation 使用 `RestorePackagesWithLockFile` /
  `NuGetLockFilePath`，locked verification 使用 `RestoreLockedMode=true`；force-evaluate 与
  locked mode 不得同时使用；
  <https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files>

不得把 `-restore` 当成 `-t:Restore`：前者会 restore 后运行默认 build target，违反 M 的
zero-build 边界。不得继续使用 `dotnet restore --disable-parallel`；该选项只限制 restore
项目并行，M02 已证明它没有限制 MSBuild node count。

## 2. create-once roots 与写白名单

执行开始前三个 exact roots 必须不存在：

```text
/private/tmp/fightmatch-loc-lic-alt-m03/loc-lic-alt-yamldotnet-16.3.0-m03/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m03/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m03/
```

唯一允许写入为这三个 roots。M01/M02 temp/evidence、old cache、005、LOC-A evidence、八个
生产路径、仓库产品与文档、`Assets/Packages/ProjectSettings/.git` 均只读。M03 只按 M01
fixed manifests 重放 approved material closed set；不得把 M02 `m02/**` scratch、failure
receipts 或 partial restore outputs复制进 candidate feed/material。M02 只作为 authority、
failure history 与 equality baseline。

material root 仍只在全部 closure/seal gates 通过后 create-once；失败前不存在，失败后若已
创建则原地标 `REJECTED_NOT_ACCEPTED`。禁止创建 B/L roots、运行 Unity/Luban/005、
build/publish/test、修改 `Program.cs`、Git 写入、删除或补写 predecessor roots。

## 3. M03 ordered execution

1. **preflight**：重算基础合同、M02 packet、本包、M01/Linux004/R 与 §1.2 seals；确认三个
   M03 roots absent、free space 与只读 inputs。创建 M03 temp/evidence；material 暂不创建。
2. **approved replay**：按 M01 `temp-files.tsv` / acquired manifest逐文件核 type/path/bytes/
   SHA，复制 M01 approved closed set到 M03 temp；生成 M01→M03 comparison。保留 M01 Mac
   blocker、Linux限定 verdict与 M02 blocker；不执行 predecessor commands，不计新 network。
3. **dependency/license expansion**：按基础合同 §§5.3–5.4，仅取得尚缺 transitive package、
   registration/catalog/source/license。不得重取 fixed direct bytes；不得改 host/version/source。
4. **single-node offline closure**：按 §4 exact process model为 runner/tests各生成 lock，再在
   新空 isolation roots各执行一次 locked verification。禁止任何 build/test target。
5. **mapping/budgets**：按基础合同 §7、§9 Step 7冻结 package/license/entry/official-test/
   expected-deployment mapping与 future B/L numeric command DAG；不读取 future output。
6. **seal**：核 schemas、finite sets、predecessors/cache/生产/protected before-after equality；
   通过后 create-once material root并复制；重算 material/evidence/process/storage/network/
   zero-action；返回 `READY_FOR_INDEPENDENT_M_R` 后停止。

失败使用基础合同 §11 最早 gate。新增合法 code：`BLOCKED_PREDECESSOR_DRIFT`、
`BLOCKED_SIGNATURE_ADJUDICATION`。若 §4 first runner generation仍出现 out-of-process node、
named-pipe permission failure、unexpected compiler/test process或 process count不符，使用
`BLOCKED_PROCESS_MODEL` / `BLOCKED_PROCESS_DRIFT` 原样停止；不得重试、切回 `dotnet
restore`、申请沙箱外权限或改变系统 trust。

## 4. exact single-node restore model

四次 restore 均使用第 3 节 fixed SDK dotnet，cwd、project、config、packages、obj、lock、
stdout/stderr 必须位于 M03 temp。仅允许设置 task-specific `DOTNET_CLI_HOME`、
`NUGET_PACKAGES`、`NUGET_HTTP_CACHE_PATH`、`NUGET_PLUGINS_CACHE_PATH`、`NUGET_SCRATCH`、
`TMPDIR`；禁止设置/覆盖 `HOME`、`home`、`CODEX_HOME`、`MSBUILDUSESERVER` 或系统 trust。

### 4.1 lock generation argv template

每个 runner/tests generation invocation 的 argv 必须等价于下列 closed set；路径替换为该
graph 的 exact M03 absolute paths：

```text
<fixed-dotnet> msbuild <project.csproj>
  -target:Restore
  -maxCpuCount:1
  -nodeReuse:false
  -noAutoResponse
  -verbosity:minimal
  -property:RestoreConfigFile=<local-only-NuGet.Config>
  -property:RestorePackagesPath=<new-empty-packages-root>
  -property:MSBuildProjectExtensionsPath=<new-empty-obj-root>
  -property:RestoreSources=<exact-local-feed>
  -property:RestoreFallbackFolders=
  -property:RestoreDisableParallel=true
  -property:RestoreNoHttpCache=true
  -property:RestoreIgnoreFailedSources=false
  -property:RestorePackagesWithLockFile=true
  -property:NuGetLockFilePath=<graph-lock-path>
  -property:RestoreForceEvaluate=true
  -property:NuGetAudit=false
```

NuGet config 必须 `<clear/>` 后只列 exact local feed；network request count必须保持 0。每个
invocation必须先确认 packages/obj/DOTNET_CLI_HOME/caches/tmp为空且互不复用。

### 4.2 locked verification argv template

verification在另一个 new-empty isolation root使用同一 template，但：

```text
REMOVE -property:RestoreForceEvaluate=true
ADD    -property:RestoreLockedMode=true
```

lock bytes/SHA 必须与 generation output 相同且不可改写。禁止同时传
`RestoreForceEvaluate=true` 与 `RestoreLockedMode=true`。

### 4.3 process acceptance

每个 invocation保存 exact argv/cwd/isolated env/start/end/exit、stdout/stderr、owned process
tree 与 executable/start/concurrency counts。必须证明：

- MSBuild effective `maxCpuCount=1`，不存在 bare `-maxcpucount`；
- target closed set仅 `Restore`，没有 default/Build/Compile/Test/Publish；
- top-level fixed-SDK dotnet starts = 1 per invocation；out-of-process MSBuild node starts = 0；
- `csc`、VBCSCompiler、VSTest、testhost、Luban、Unity、Java、Gradle starts = 0；
- generation产生 exact lock；verification exit 0且 lock不变；network = 0；
- sequential execution，任何时刻只有一个 top-level tree。

若 runtime仍启动 out-of-process node但成功且 process evidence可精确归属，不得自行接受；
返回 `BLOCKED_PROCESS_DRIFT` 由主程判断。不得通过扩大 node cap掩盖 M02 根因。

## 5. cumulative budgets 与 equality

M01+M02+M03 的 external network总上限仍为 requests `<=256`、body `<=384 MiB`。M01 已用
42 / 23,259,536 B，M02 为 0 / 0，因此 M03 上限仍为 214 requests / 379,393,648 B；
concurrency 1、每 request redirects `<=3`、单 body `<=64 MiB`、acquisition wall `<=25 min`。

M03 自身 ceilings沿用 M02：top-level commands/trees `48/1 concurrent`、total process starts
`<=20,000`、wall `<=45 min`、fixed SDK dotnet starts `<=96`（top-level restore/probe
`<=20`，concurrent `<=1`）、zsh top-level `<=48`、curl `<=214`。§4 四次成功 restore的
expected fixed-SDK actual为 4 top-level / 0 node children；失败 probe也计入 caps且不得重复同一
argv。storage沿用 M02：temp `<=1 GiB/50,000 files`、material
`<=512 MiB/25,000`、evidence `<=128 MiB/10,000`、logs aggregate `<=64 MiB`、单 stdout/
stderr `<=4 MiB`、free floor start `>=2 GiB` / sustained `>=1 GiB`。

before/after equality必须覆盖 old cache、005、LOC-A original/FIX-01/FIX-02、M01 temp/evidence、
M02 temp/evidence、八个生产路径与 protected status。M02 failed roots不得变化；M03 source/
material/evidence写入不得污染 predecessors或生产路径。

## 6. 必须产出的 M03 receipts

除基础合同与 M02要求的 final schemas外，M03 evidence必须包含：

- `authority/predecessor-m01.json`、`authority/predecessor-m02.json` 与
  `authority/signature-adjudication.json`；
- M01→M03 replay manifest，明确排除 M02 partial scratch；
- `acquisition/network-cumulative.json` 分列 M01/M02/M03/cumulative；
- `process/m03-completed-commands.tsv` 与每个 tree 的 exact effective MSBuild/process receipts；
- runner/tests generation与locked verification的四套 raw stdout/stderr/exit/process evidence；
- 两个 locks、完整 package/license/entry/deployment manifests、future B/L budgets；
- predecessor/old/cache/生产/protected before-after identity；
- material `CANDIDATE_NOT_ACCEPTED` seal与 final `result.json`。

`result.json.zeroActionReceipt`沿用 M02全部15项且均为0。另显式写：

```text
dotnetRestoreInvocationCount: 0
dotnetMsbuildRestoreTargetInvocationCount: <actual>
msbuildOutOfProcessNodeStartCount: 0
macSignatureVerifyInvocationCount: 0
linuxSignatureVerifyInvocationCount: 0
sdkDownloadCount: 0
m01MutationCount: 0
m02MutationCount: 0
sandboxEscalationCount: 0
```

## 7. 回传、独立 R 与签包未执行声明

delivery owner回传：owner turn/activation、三个 roots、M01/M02 predecessor replay、signature
adjudication、new/cumulative network、四次 restore exact argv与全部 process/storage/wall actual、
package/license/lock/deployment manifests、equality、zero-action、material/evidence/result identities
与 blocker。完成后停止，不自行送 R，不进入 B/L。

主程机械审 actual roots/evidence后，成功时交同一原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local` 审完整 M01+Linux004+M02+M03 chain；R只给
一个 `ACCEPT` / `NEEDS_FIX` / `REJECT`。只有 `ACCEPT` 才形成可供未来 B 引用的
`M-ACCEPTED` authority；仍不自动授权 B/L。

本文件创建回合只新增本 packet。没有创建三个 M03 roots，没有网络、复制、restore、
package/license resolution、material seal、build、publish、test、Unity、Luban、005、产品、
外置工具根、沙箱升级或 Git 写入。实际执行只在主程机械复核本文件 final SHA-256、把状态
提升为 `AUTHORIZED_FOR_ASSIGNED_OWNER` 并向 owner回传身份后开始。
