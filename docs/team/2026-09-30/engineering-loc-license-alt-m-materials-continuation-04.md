# LOC-LIC-ALT-M-04 — per-project lock-set 材料闭包续行包

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目标、责任与结论边界

本包是 M03 在 single-node restore 已成功、但共享 `obj`／单 lock 折叠完整 project graph 后的
new sibling continuation。它只把 accepted lock-set design落实为一次新的 M continuation：从 M01
approved material重放四份隔离 source，使用每 project默认 `obj` 与相邻 lock，生成 runner/tests
完整 lock sets，执行 locked-mode verification，冻结 raw/canonical assets、package/license/
deployment mapping与 future B/L budgets，最后形成 `CANDIDATE_NOT_ACCEPTED` material。

- delivery owner：原 LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已允许普通同级续行。
- 基础合同：`engineering-loc-license-alt-m-materials.md`，582 lines / 30,837 B /
  SHA-256 `0ce7c44844494e8b63c5e6c149fa357932c11c9a53457275a24a16ac4ef48d13`。
- M03 packet：`engineering-loc-license-alt-m-materials-continuation-03.md`，269 lines /
  15,522 B / SHA-256
  `11abcdf14316ea7cb6755d7d6660e9b58baa2fe6c9e4c117350144d279bf88be`。
- accepted correction：`engineering-localization-lock-set-system-design-correction.md`，384 lines /
  21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；原独立 R
  turn `01a0f524-3498-73e2-81dd-759bd648af3b` verdict `ACCEPT`。

除本文与 accepted correction明确替换的 M04 roots、per-project graph/lock/assets schema、
canonicalizer及 process budget外，基础合同和 M02/M03 的 source/version/patch、Linux signature
限定裁决、license、zero-action、失败保全、network与验收边界继续有效。成功终点只能是
`READY_FOR_INDEPENDENT_M_R`；作者不得写 `M-ACCEPTED`，不得开始 B/L、build、publish、test、
Unity、Luban、005、产品写入、Git或外部人工消息。

## 1. fixed predecessor 与 M03 blocker

执行前必须重算 M01、M02、M03、Linux004/R 与 accepted correction的固定 identity；任一不符
立即 `BLOCKED_PREDECESSOR_DRIFT`。M03 必须至少匹配：

| item | fixed identity |
| --- | --- |
| `result.json` | 75 lines / 3,523 B / SHA `97acafbe4ed1bb88b7788af9e2ab3a5304a8be00a37bbc9168bec985fb42cf6f`；`BLOCKED_MATERIAL_FREEZE` |
| `evidence-files.tsv` | 44 lines / 4,367 B / SHA `33247b6e94c4695c00b63a3e3dad83f9b732d158e17e0e9a0aed8cf6f68281e6` |
| `process/runner-generation.json` | 71 lines / 4,104 B / SHA `5ccfbd6c53641a36b2b47c49ece20999cec66f7a0a081fc381241c9c17ec3c4a` |
| `process/m03-completed-commands.tsv` | 26 lines / 3,195 B / SHA `cf095e1907fdcc7d8d78d1b315f635eb83f1c39d8d854259eecb1d6ce0206dcd` |
| M03 temp manifest | 2,294 lines / 305,437 B / SHA `ac6358c79056ed0840c603979a1278553c8ba1515814fd84765193ef025a120d` |

M03 唯一 restore exit `0`、fixed SDK dotnet starts `1`、out-of-process nodes `0`、network `0`，
证明 single-node process adaptation有效；但共享 `MSBuildProjectExtensionsPath` 与
`NuGetLockFilePath` 最终只冻结 `Luban.Core` / `Luban.Rust`、NLog / Scriban，不能代表 runner
23-node / 56-edge graph。M04 不得复制或消费 M03 `m03/closure/**`、不完整 lock、`obj`、packages
或 assets作为 candidate；它们只作 blocker history。

## 2. create-once roots 与写白名单

签发时以下三个 exact roots 已读验证为 absent；owner必须在动作前再次验证：

```text
/private/tmp/fightmatch-loc-lic-alt-m04/loc-lic-alt-yamldotnet-16.3.0-m04/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m04/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m04/
```

唯一允许写入是这三个 roots。material root只在全部 closure/seal gates通过后 create-once；
失败前保持 absent，若 seal后失败则原地标 `REJECTED_NOT_ACCEPTED`。M01/M02/M03 roots、old cache、
005、LOC-A evidence、八个生产路径、仓库文档与产品、`Assets/Packages/ProjectSettings/.git` 均只读。
禁止清空、覆盖、合并或补写 predecessor；不得创建 B/L root。

M04 只可按 M01 accepted acquired/source manifests逐字节重放 approved material。允许从 M01
material组装新的 runner/tests local-only feeds；禁止从 M02/M03 scratch取捷径。重放后 runner
feed必须恰好覆盖其闭包包，tests feed必须覆盖 tests闭包所需的 28 个 unique nupkg；每项与 M01
receipt逐 path/bytes/SHA相等。M04 external network必须为 `0 requests / 0 bytes`；缺件即
`BLOCKED_MATERIAL_GAP`，不得下载或读取未列缓存。

## 3. 冻结 graph 与四份 isolation

在任何 restore 前，从 M01 patched source相同字节分别创建：

```text
<M04_TEMP>/closure/runner-generation/source/
<M04_TEMP>/closure/runner-verification/source/
<M04_TEMP>/closure/tests-generation/source/
<M04_TEMP>/closure/tests-verification/source/
```

四份 source copy开始时必须与 M01 accepted patched-source manifest集合等值。root projects固定为
`src/Luban/Luban.csproj` 与 `src/Luban.Tests/Luban.Tests.csproj`。按 accepted correction §2.1
冻结唯一 node set、完整有向 ProjectReference edge set与 root identity：

- runner：恰好 23 unique project nodes、56 unique directed edges；
- tests：恰好 13 unique project nodes、29 unique directed edges；
- root已经包含在 node count，不能重复另加；edge保存 parent、child、Include literal与
  `ReferenceOutputAssembly`；同一 child可有多个 parents，禁止用 spanning tree替代。

generation、verification、material seal与 future B必须比较完整 node/edge set。任一缺、多、
source-relative path漂移、root错误或 ProjectReference metadata漂移立即
`BLOCKED_PROJECT_GRAPH_DRIFT`。每个 graph的 generation/verification使用不同 empty packages、
CLI/cache/temp roots；每个 project保留自身默认相邻 `obj` 与 `packages.lock.json`。

## 4. frozen support tool 与 canonical identity

M04 在任何 restore 前，只可在 M04 temp创建一个 Python standard-library-only support source，
负责 graph inventory、canonical assets与 fixtures；不得安装模块、启动子进程或联网。固定解释器：

```text
/opt/homebrew/bin/python3
Python 3.13.7
SHA-256 a708f6e9f4803b806b29146c4e0feecfd9bf2d9eb60f3e15b850cd7cb56f200b
```

若 path/version/hash任一漂移，停止 `BLOCKED_CANONICALIZER_RUNTIME_DRIFT`，不得换解释器。source
创建后立即冻结 path/bytes/SHA；之后只读，所有 invocation逐字节复用。support tool必须完整实现
accepted correction §3.1：strict UTF-8/no BOM、duplicate-key/invalid/NaN拒绝；对每个 object key
与 value执行 exact-root/root+slash目录规则与 config exact-file规则；key token collision拒绝；
ordinal UTF-8 key排序、arrays原序、invariant shortest round-trip numbers；任何剩余 isolation
identity拒绝。

每个 raw assets在 tokenization前核 exact pointer
`/project/restore/restoreLockProperties/restoreLockedMode`：generation必须 absent，verification与
future B必须 JSON boolean true；写 `restore-locked-mode.tsv` 后 canonical copy只删除该 exact
property，其余 restore metadata保留。support tool在 restore前必须运行 accepted correction
§3.1 的全部 positive/negative fixtures；wrong phase/type、key collision、residual path必须 nonzero，
version/contentHash/ProjectReference/selected asset漂移必须产生不同 identity。fixture任一不符立即
`BLOCKED_ASSETS_CANONICALIZATION`，不得改算法后继续同一 root。

## 5. exact restore model

四次 restore依序执行：runner generation → runner verification → tests generation → tests
verification。每次一个 top-level tree，前一次 complete seal后才开始下一次。固定命令模板：

```text
<fixed-dotnet> msbuild <graph-root-project>
  -target:Restore
  -maxCpuCount:1
  -nodeReuse:false
  -noAutoResponse
  -verbosity:minimal
  -property:RestoreConfigFile=<graph-local-only-NuGet.Config>
  -property:RestorePackagesPath=<graph-new-empty-packages-root>
  -property:RestoreSources=<exact-graph-local-feed>
  -property:RestoreFallbackFolders=
  -property:RestoreDisableParallel=true
  -property:RestoreNoHttpCache=true
  -property:RestoreIgnoreFailedSources=false
  -property:RestorePackagesWithLockFile=true
  -property:RestoreForceEvaluate=true
  -property:NuGetAudit=false
```

generation后按 project-relative path收集完整 lock set，将原 bytes复制到 verification source相同
project目录。verification命令删除 `RestoreForceEvaluate=true`，增加
`RestoreLockedMode=true`；其它参数相同，packages/obj/CLI/cache/temp重新为空。

显式禁止设置：`MSBuildProjectExtensionsPath`、`NuGetLockFilePath`、`BaseIntermediateOutputPath`、
`IntermediateOutputPath`、`ProjectAssetsFile` 或任何把多 project汇聚到共享路径的等价 property。
禁止逐 project restore；每个 graph只从其 root project启动一次。禁止 `-restore`、默认/Build/
Compile/Test/Publish targets。NuGet.Config必须 `<clear/>` 后只列 exact local feed。

每次保存 argv/cwd/env/start/end/exit/stdout/stderr/process tree。必须是 fixed-SDK dotnet starts `1`、
out-of-process MSBuild nodes `0`、`csc`/VBCSCompiler/VSTest/testhost/Luban/Unity/Java/Gradle starts `0`、
network `0`。任一 process/network drift立即停止，不重试同一 argv。

## 6. lock/assets/material acceptance

每个 graph generation与 verification都必须满足：

1. 每个 node恰好一个相邻 `packages.lock.json` 与一个 `obj/project.assets.json`；不得缺、多或出现
   graph外 project output；
2. verification exit `0`，对应 lock bytes/SHA与 generation逐文件相同；
3. raw assets全部保存 bytes/SHA；同一 frozen support tool生成 canonical assets；同 project的
   generation/verification canonical bytes/SHA相同；
4. canonical `projectFileDependencyGroups`、`targets`、`libraries`、`packageFolders`、`project`
   restore/frameworks与 ProjectReference identity完整保留；不得只比较 package名字；
5. lock/package edges与 graph/project来源相联，跨 lock去重只能发生在 material package bytes层。

material schema以 accepted correction §3取代 singular lock。必须包含且 seal：

- `identity/runner-projects.tsv`、`tests-projects.tsv`、两个 `*-project-edges.tsv`；
- `identity/runner-lock-set.tsv`、`tests-lock-set.tsv`；
- `identity/runner-locks/<project-dir>/packages.lock.json` 与 tests对称集合；
- generation raw assets 与 canonical assets逐 project集合；verification raw/canonical receipts；
- `restore-locked-mode.tsv`、canonicalizer source/runtime/fixture receipts；
- 基础合同的 package-files、package-entries、package-closure、license-files、expected-deployment、
  official-test-project、commands/process/temp/network/receipts manifests；
- material-files、storage totals与 final result。

material中禁止 singular `identity/runner.packages.lock.json` 与
`identity/tests.packages.lock.json`。任一 lock/assets/project集合不完整使用
`BLOCKED_LOCK_SET_INCOMPLETE`；canonical不同使用 `BLOCKED_ASSETS_IDENTITY_DRIFT`；locked-mode
错阶段使用 `BLOCKED_LOCKED_MODE_DRIFT`。

## 7. budgets、equality 与 stop gates

- top-level commands/trees `<=64` / concurrent `1`；wall `<=45 min`；total process starts
  `<=20,000`；fixed SDK dotnet restore starts expected/exact `4`、cap `4`；support Python starts
  `<=8`、concurrent `1`、children `0`；zsh top-level `<=64`；
- M04 network exact `0/0`；M01+M02+M03+M04 cumulative仍为 `42 requests / 23,259,536 B`；
- temp `<=1 GiB/50,000 files`，material `<=512 MiB/25,000`，evidence
  `<=128 MiB/10,000`，logs aggregate `<=64 MiB`，单 stdout/stderr `<=4 MiB`；start free
  `>=2 GiB`、sustained `>=1 GiB`；
- 禁止设置/覆盖 `HOME`、`home`、`CODEX_HOME`、系统 trust或 package manager状态；只可设置
  task-specific `DOTNET_CLI_HOME`、NuGet cache/scratch与 `TMPDIR`；
- before/after equality覆盖 old cache、005、LOC-A original/FIX-01/FIX-02、M01/M02/M03
  temp/evidence、八个生产路径、repo protected status；M04 source/material/evidence不得污染它们。

任何 first failure立即 seal 当前 evidence并停止后续 dependent phases。不得删除失败 root、提高
预算、申请沙箱外运行、切换 SDK/runtime、编辑 source以迎合 restore、补跑 build/test或自行进入
B/L。M04 `result.json.zeroActionReceipt` 至少保持 candidate build/publish/test、Luban、005、
Unity、Program.cs/product/Git写入、signature reverify、SDK download、predecessor mutation、
sandbox escalation全部为 `0`。

## 8. 回传与独立 R

owner回传：actual thread/host/turn、三个 roots、predecessor/correction seals、M01-only replay、
support source/runtime/fixtures、graph node/edge sets、四次 exact restore/process receipts、每 project
lock/raw/canonical assets sets、package/license/deployment/B/L budget manifests、storage/wall/network、
before-after equality、zero actions、material/evidence/result bytes/SHA及唯一状态
`READY_FOR_INDEPENDENT_M_R` 或最早 blocker。

成功后先回主程，不自行送 R。主程机械核 actual evidence后交同一原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local` 审 M01→Linux004/R→M02→M03→accepted
correction→M04完整 chain，只给 `ACCEPT` / `NEEDS_FIX` / `REJECT`。只有 R `ACCEPT` 才能形成
`M-ACCEPTED` authority；仍不自动授权 B/L。

本文创建只新增本 packet。没有创建 M04 roots、复制材料、运行 support tool/restore、联网、
seal material、build/publish/test、Unity/Luban/005、产品、B/L或 Git。
