# LOC-LIC-ALT lock-set system-design correction

2026-10-01（Asia/Shanghai） · 状态：`PROPOSED_FOR_INDEPENDENT_DESIGN_R`

## 0. 目的与裁决边界

本文只修正已接受的 Localization license-alternative SYS/M 中“每个 graph 只有一个
`packages.lock.json`，并向整个 project-reference graph 传入同一个
`MSBuildProjectExtensionsPath` / `NuGetLockFilePath`”这一错误模型。它不改变：

- Luban `v5.1.0`、.NET SDK `8.0.425`、YamlDotNet `16.3.0` 或任何 package/source/license
  identity；
- ARCH B1 / SYS Candidate A、两行 `PackageReference` replacement、local-only feeds、
  signature 限定裁决或 M/B/L 分段；
- M 阶段 build/publish/test 全为零、single-node restore、网络与存储预算、独立 M R；
- `Config/` / `Generated/` / `Tools/FightMatch.Localization.Compiler/` 或 Unity 产品范围。

本文是设计修正，不是 M04 执行授权。原独立 R 对本文给出 `ACCEPT` 前，不得签发 M04、
创建 M04 roots、运行 restore 或改写任何 predecessor/material/product root。

责任：工程主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`；返回同一线程。
用户 authority 为 `AGENTS.md` 第 6 节已批准的五角色持续流程。独立设计 R 必须与本文作者、
未来 M04 执行者分离。

## 1. 实证 blocker

M03 依照当时已签 exact argv，仅执行一次 runner generation：

```text
dotnet msbuild src/Luban/Luban.csproj
  -target:Restore -maxCpuCount:1 -nodeReuse:false -noAutoResponse
  ...
  -property:MSBuildProjectExtensionsPath=<one-empty-obj-root>
  -property:NuGetLockFilePath=<one-absolute-lock-path>
```

执行 exit `0`，out-of-process MSBuild node / named-pipe failure 均为 `0`，证明 M02 的
process-model blocker 已被 single-node 形式消除。但输出只保留最后写入的
`Luban.Rust` 子图：

| evidence | identity / observation |
| --- | --- |
| M03 result | 3,523 B；SHA-256 `97acafbe4ed1bb88b7788af9e2ab3a5304a8be00a37bbc9168bec985fb42cf6f`；`BLOCKED_MATERIAL_FREEZE` |
| process receipt | `process/runner-generation.json`，4,104 B；SHA-256 `5ccfbd6c53641a36b2b47c49ece20999cec66f7a0a081fc381241c9c17ec3c4a` |
| generated lock | 621 B；SHA-256 `2781e0424a555858f2af4649c1d3de3a36be9d9ada3c8c1d248d93e2378cb299` |
| dgspec projects | only `Luban.Core`, `Luban.Rust` |
| lock packages | only `NLog 5.3.4`, `Scriban 7.2.5` |
| missing top-level runner packages | `CommandLineParser 2.9.1`, `YamlDotNet 16.3.0` |

该 lock 不能代表 `src/Luban/Luban.csproj` 的完整 project/package graph。M03 正确停止，
没有生成 tests lock、locked verification、material、B 或 L。

根因是 project-reference graph 的 restore 由多个 project 各自产生 `obj/project.assets.json`
和 project lock。把 graph 中每个 project 的输出重定向到同一个 `obj` 与同一个绝对 lock
路径，会发生覆盖；“runner 一个 lock、tests 一个 lock”不是完整 graph identity。

官方依据：

- NuGet 的 PackageReference lock 文档将 `packages.lock.json` 定义为项目根目录中的 lock，
  并用 `RestorePackagesWithLockFile` / `RestoreLockedMode` 控制生成与验证：
  <https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files>
- NuGet MSBuild targets 文档说明 `NuGetLockFilePath` 是指定 lock 路径的 project property；
  本设计不再把一个 project property 的单一路径强行共享给整个 graph：
  <https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets>
- single-node / restore-only 约束继续采用 MSBuild 与 `dotnet msbuild` 官方命令语义：
  <https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference>，
  <https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-msbuild>。

## 2. 决策：两个 graph-scoped project lock sets

### 2.1 graph 与 lock 的单位

M 不再生成两个 singular lock files。M 必须生成两个 **graph-scoped project lock sets**：

1. runner set：以 `src/Luban/Luban.csproj` 为 root，覆盖其完整、有限、闭包内的每个
   project；
2. tests set：以 `src/Luban.Tests/Luban.Tests.csproj` 为 root，覆盖其完整、有限、闭包内的
   每个 project。

每个 project 恰好拥有该 graph 内自己的默认相邻 `packages.lock.json`，以及 restore 产生的
自己的默认 `obj/project.assets.json`。两个 graph 即使包含同一个 project，也必须保存为两套
独立产物，不以“预计相同”去重或互借。

M04 前的只读 closure 解析预期如下；M04 必须从固定 patched source 重新计算、逐路径 sealing，
任何增减或环路均 `BLOCKED_PROJECT_GRAPH_DRIFT`：

**runner root（23 projects）**

```text
src/Luban/Luban.csproj
src/Luban.Bson/Luban.Bson.csproj
src/Luban.CSharp/Luban.CSharp.csproj
src/Luban.Core/Luban.Core.csproj
src/Luban.Cpp/Luban.Cpp.csproj
src/Luban.Dart/Luban.Dart.csproj
src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj
src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj
src/Luban.DataValidator.Builtin/Luban.DataValidator.Builtin.csproj
src/Luban.FlatBuffers/Luban.FlatBuffers.csproj
src/Luban.Gdscript/Luban.Gdscript.csproj
src/Luban.Golang/Luban.Golang.csproj
src/Luban.Java/Luban.Java.csproj
src/Luban.Javascript/Luban.Javascript.csproj
src/Luban.L10N/Luban.L10N.csproj
src/Luban.Lua/Luban.Lua.csproj
src/Luban.MsgPack/Luban.MsgPack.csproj
src/Luban.PHP/Luban.PHP.csproj
src/Luban.Protobuf/Luban.Protobuf.csproj
src/Luban.Python/Luban.Python.csproj
src/Luban.Rust/Luban.Rust.csproj
src/Luban.Schema.Builtin/Luban.Schema.Builtin.csproj
src/Luban.Typescript/Luban.Typescript.csproj
```

**tests root（13 projects）**

```text
src/Luban.Tests/Luban.Tests.csproj
src/Luban.Bson/Luban.Bson.csproj
src/Luban.CSharp/Luban.CSharp.csproj
src/Luban.Core/Luban.Core.csproj
src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj
src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj
src/Luban.DataValidator.Builtin/Luban.DataValidator.Builtin.csproj
src/Luban.FlatBuffers/Luban.FlatBuffers.csproj
src/Luban.L10N/Luban.L10N.csproj
src/Luban.Lua/Luban.Lua.csproj
src/Luban.MsgPack/Luban.MsgPack.csproj
src/Luban.Protobuf/Luban.Protobuf.csproj
src/Luban.Schema.Builtin/Luban.Schema.Builtin.csproj
```

以上计数都**已经包含 root project**：runner nodes 恰好 23，tests nodes 恰好 13，不再另加
一条重复 root node。project graph 不能用“每 project 一个 parent”表示，因为同一 project 可被
多个 parent 引用。M04 必须分别冻结：

- node set：runner 23 / tests 13 个唯一 project-relative paths；
- complete directed edge set：runner 56 / tests 29 个唯一
  `(parentProjectRelativePath, childProjectRelativePath)` ProjectReference edges；
- root identity：在 graph receipt 顶层单独写 root path，不作为额外 node/edge row。

四个 generation / verification 与 future B 都必须比较完整 node set + complete edge set；只核
reachable nodes、只留 first parent 或用 spanning tree 代替完整多父 graph 均不合格。

### 2.2 isolated source layout

每次 generation / verification 都使用全新的 patched-source copy：

```text
<M04_TEMP>/closure/runner-generation/source/
<M04_TEMP>/closure/runner-verification/source/
<M04_TEMP>/closure/tests-generation/source/
<M04_TEMP>/closure/tests-verification/source/
```

因此默认相邻 lock 与默认 per-project `obj` 全部位于允许写入的 M04 temp root 内，不污染
official source、M01/M02/M03 或仓库。四份 source copy 必须在 restore 前与 M01 accepted
patched-source manifest 等值；generation 后只允许各 graph 闭包项目下出现 lock/obj outputs，
不得接受 `.cs`、`.csproj` 或其它 source diff。

### 2.3 generation command correction

M04 的 runner/tests generation 继续各只有一个 top-level command tree，并继续使用：

```text
<fixed-dotnet> msbuild <graph-root-project>
  -target:Restore
  -maxCpuCount:1
  -nodeReuse:false
  -noAutoResponse
  -verbosity:minimal
  -property:RestoreConfigFile=<graph-local-only-NuGet.Config>
  -property:RestorePackagesPath=<graph-empty-packages-root>
  -property:RestoreSources=<exact-graph-local-feed>
  -property:RestoreFallbackFolders=
  -property:RestoreDisableParallel=true
  -property:RestoreNoHttpCache=true
  -property:RestoreIgnoreFailedSources=false
  -property:RestorePackagesWithLockFile=true
  -property:RestoreForceEvaluate=true
  -property:NuGetAudit=false
```

必须删除、不得替换为另一共享路径：

```text
-property:MSBuildProjectExtensionsPath=...
-property:NuGetLockFilePath=...
```

不得设置全局 `BaseIntermediateOutputPath`、`IntermediateOutputPath`、`ProjectAssetsFile` 或
其它把多个 project 汇聚到单一路径的等价 property。不得逐 project 启动 23/13 次 restore；
top-level root restore 必须自然遍历 project references，仍维持四次 restore 总量与 single-node
约束。

### 2.4 locked verification

每个 generation 完成后：

1. 按 project-relative path 收集该 graph 每个 `packages.lock.json`；
2. 把 lock set 原字节复制到对应 verification source 的同一 project 目录；
3. verification 使用同一命令模板，但删除 `RestoreForceEvaluate=true`，增加
   `RestoreLockedMode=true`；仍不设置共享 obj/lock properties；
4. verification 前 packages roots、每个 project 的 `obj`、CLI/cache/temp 均为空；
5. verification exit `0` 后逐 lock bytes/SHA 不变，并逐 project按 §3.1 比较
   `project.assets.json` 的 canonical dependency/asset identity。

任一 graph project 缺 lock、多 lock、verification 改写 lock、产生未列 project 输出或只恢复
子图，均 `BLOCKED_LOCK_SET_INCOMPLETE`；不得用 root lock 成功掩盖 child project 缺失。

## 3. material identity 与 schema correction

以下新 manifest 取代 singular lock schema：

| file | required fields |
| --- | --- |
| `identity/runner-projects.tsv` | `projectRelativePath,csprojBytes,csprojSha256,targetFramework,isRoot`；恰好 23 rows，恰好一行 `isRoot=true` |
| `identity/tests-projects.tsv` | 同 schema；恰好 13 rows，恰好一行 `isRoot=true` |
| `identity/runner-project-edges.tsv` | `parentProjectRelativePath,childProjectRelativePath,includeLiteral,referenceOutputAssembly`；恰好 56 个完整 directed ProjectReference rows |
| `identity/tests-project-edges.tsv` | 同 schema；恰好 29 rows |
| `identity/runner-lock-set.tsv` | `projectRelativePath,lockRelativePath,lockBytes,lockSha256,rawAssetsRelativePath,rawAssetsBytes,rawAssetsSha256,canonicalAssetsRelativePath,canonicalAssetsBytes,canonicalAssetsSha256,directPackageCount,transitivePackageCount,projectReferenceCount` |
| `identity/tests-lock-set.tsv` | 同 schema |
| `identity/runner-locks/<project-dir>/packages.lock.json` | 对应 runner generation project lock 原字节 |
| `identity/tests-locks/<project-dir>/packages.lock.json` | 对应 tests generation project lock 原字节 |
| `identity/runner-assets/<project-dir>/project.assets.json` | 对应 generation assets 原字节 |
| `identity/tests-assets/<project-dir>/project.assets.json` | 对应 generation assets 原字节 |
| `identity/runner-assets-canonical/<project-dir>/project.assets.canonical.json` | 按 §3.1 从 runner generation raw assets 得到的 canonical bytes |
| `identity/tests-assets-canonical/<project-dir>/project.assets.canonical.json` | 按 §3.1 从 tests generation raw assets 得到的 canonical bytes |

`<project-dir>` 必须保留 source-relative project directory，例如：

```text
identity/runner-locks/src/Luban/packages.lock.json
identity/runner-locks/src/Luban.DataLoader.Builtin/packages.lock.json
identity/tests-locks/src/Luban.Tests/packages.lock.json
```

删除且 material 中禁止存在：

```text
identity/runner.packages.lock.json
identity/tests.packages.lock.json
```

原 `package-closure.tsv`、`package-files.tsv`、`package-entries.tsv` 等仍保留，但每条 lock edge
必须增加或以现有 `graph` + 新 `projectRelativePath` 字段绑定来源 project/lock；跨 lock
去重只能发生在 package material 存储层，不能丢失项目边。

### 3.1 `project.assets.json` canonical identity

raw `project.assets.json` 必须保留并逐文件记录 bytes/SHA，供审计实际 NuGet 输出；但 raw JSON
包含 generation / verification / future B 各自 isolation source、packages 与 obj absolute
paths，**不得**用 raw hash 做跨 root 等值结论。

M04 必须在任何 restore 前创建并冻结唯一 canonicalizer source、bytes/SHA 与 executable
identity；generation、verification、material sealing 和 future B 都逐字节复用该实现。对每个
raw assets file 执行以下封闭算法：

1. 以 UTF-8 严格解析 JSON；拒绝 BOM、duplicate object property、invalid JSON、NaN/Infinity。
2. 在内存副本中递归处理**每个 object key 与每个 value**。对每个 string先构造仅用于
   path-match 的 candidate（把 `\\` 统一为 `/`）；不匹配下列路径规则的 string 保留原字节
   语义，不对普通文本擅自改斜线。目录规则必须同时接受 `candidate == root` 与
   `candidate` 以 `root + "/"` 开头；前者输出 exact token，后者输出 token + 原相对 suffix：
   - 当前 isolation source root → `/_/source`；
   - 当前 isolation packages root → `/_/packages`；
   - 当前 graph local feed root → `/_/feed`；
   - 当前 `RestoreConfigFile` 只允许 candidate 与 exact file path 完全相等，输出
     `/_/nuget-config/NuGet.Config`，不得把相邻文件作 prefix match；
   - 当前 isolation `DOTNET_CLI_HOME` → `/_/dotnet-home`；
   - 当前 isolation NuGet HTTP/plugin/scratch/TMP roots分别 →
     `/_/nuget-http`、`/_/nuget-plugins`、`/_/nuget-scratch`、`/_/tmp`。
   object key 必须先按同一规则 token 化；若同一 object 的两个原 keys token 化为同一 key，
   立即 `BLOCKED_ASSETS_CANONICALIZATION`，不得覆盖、合并或保留 first/last。fixed SDK
   absolute root、package identities与非 isolation strings不得重写。key/value tokenization
   后，任一 key 或 value 的 path-match candidate 仍包含当前 isolation source/packages/feed/
   config/CLI/cache/temp absolute identity，亦立即阻断。
3. 在任何 path tokenization 前，读取 exact JSON pointer
   `/project/restore/restoreLockProperties/restoreLockedMode`：
   - generation 必须不存在该 property（固定 argv未启用 locked mode）；
   - verification 与 future B 必须存在且 JSON boolean 恰好为 `true`；
   - 每阶段把 pointer存在性、type、value、raw assets SHA写入独立
     `restore-locked-mode.tsv` receipt；错阶段、false、string `"true"` 或其它 type均
     `BLOCKED_LOCKED_MODE_DRIFT`。
   阶段校验通过后，canonical 内存副本只删除这一 exact `restoreLockedMode` property，使
   generation / verification / B 比较 dependency/asset identity，而不是比较执行模式。
   `restoreLockProperties` object本身、`restorePackagesWithLockFile`、`nuGetLockFilePath` 与
   其它 restore metadata仍保留并走同一 path normalization；不得删除整个 restore subtree。
4. 每个 JSON object 的 properties按 tokenization 后的 ordinal UTF-8 key bytes升序输出；
   arrays保持 NuGet 原始
   顺序，不擅自集合排序；numbers 使用解析后的 invariant shortest round-trip 表示；布尔/null
   使用小写 JSON literals；strings 使用标准 JSON escaping，不转义普通非 ASCII 字符。
5. 输出 UTF-8 without BOM、LF、末尾恰好一个 LF、无非语义 whitespace；文件名固定
   `project.assets.canonical.json`。

canonical artifact 必须完整保留并可由 R 读取以下语义字段；不得为了稳定 hash 删除：

- `version`、`targets` 的全部 target-framework/library nodes、dependency versions；
- 每个 target node 的 compile/runtime/native/contentFiles/build/buildMultiTargeting/
  buildTransitive/analyzers/resource/frameworkReferences及其它 selected asset maps；
- `libraries` 的 type/path/sha512/files 与 package content hash identity；
- `projectFileDependencyGroups` 的 direct package constraints；
- `packageFolders`（只允许 packages root token 化）；
- `project.frameworks` dependencies、frameworkReferences、imports/fallback；
- `project.restore.frameworks[].projectReferences`，source path token 化后仍保留完整项目引用；
- NuGet `logs`；任何 error 或未获准 warning仍按原 M blocker 处理。

generation 与对应 verification 必须按 project-relative path得到相同 canonical bytes/SHA；两套
graph 的 raw/canonical files都进入 material。future B restore 后使用 accepted canonicalizer与
该 graph accepted canonical set逐项目比较。不能只比较 package names/versions、不能忽略
contentHash / ProjectReference / selected assets，也不能让 M、verification、B 各自实现不同
投影。

canonicalizer 在 M04 restore 前还必须对固定 synthetic JSON fixtures执行静态正负自检并封存
argv/output/hash：

- positive A：只把 source/packages/feed/config/CLI/cache/temp absolute roots全部换成另一组
  roots，含 `packageFolders` key、`sources` key、`projectReferences` key以及恰好等于 directory
  root / config file 的 values；canonical bytes必须相同；
- positive B：generation fixture无 locked marker，verification fixture只有该 exact marker为
  boolean true；分别通过阶段校验后 canonical bytes必须相同；
- negative：generation带 marker、verification/B缺 marker或 type/value非 boolean true必须
  拒绝；
- negative：两个 keys token 化后碰撞必须拒绝；
- negative：分别改变 dependency version、library sha512/contentHash、ProjectReference path、
  任一 selected asset path，canonical SHA必须改变。

这些 fixture只验证 canonicalizer，不是 restore、build或test，也不得读取/改写 M03 raw assets
作为新 accepted output。M03 raw assets继续只是 blocker依据。

验收至少包括：

- runner 的 `src/Luban/packages.lock.json` 直接包含 `CommandLineParser 2.9.1` 与
  `YamlDotNet 16.3.0`；
- runner/tests 各自的 `src/Luban.DataLoader.Builtin/packages.lock.json` 直接包含
  `YamlDotNet 16.3.0`；
- runner set 覆盖 23 nodes / 56 edges，tests set 覆盖 13 nodes / 29 edges；
- old `YamlDotNet.NetCore` / old-only `System.*` edges 为零；
- 五个 official-test direct seeds 出现在 `src/Luban.Tests/packages.lock.json`，其所有
  transitives/assets 仍进入 tests closure；
- generation 与 verification 的 graph nodes/complete edges、逐项目 lock bytes、逐项目
  canonical assets bytes分别集合等值；raw assets只保留审计 identity，不要求跨 root同 hash。

`material-files.tsv`、receipts、storage totals 与独立 M R 都必须覆盖新增 lock/assets 文件，
不能只 hash `*-lock-set.tsv` 摘要。

## 4. future B command correction

accepted M material 被 B 消费时：

- pass-1 / pass-2 各得到新的 patched-source copy；
- 只把 accepted runner lock set 逐项目放回对应 source project 目录；
- official-tests source 只放回 accepted tests lock set；
- restore 使用 graph root project、local feed/packages root 与 `RestoreLockedMode=true`；
- 不传 `NuGetLockFilePath`、`MSBuildProjectExtensionsPath` 或其它共享 project-output property；
- restore 后逐 lock 不变，并用 accepted M canonicalizer把 actual per-project
  `project.assets.json` 转成 canonical artifacts，与 accepted canonical assets identity 逐字节
  比较后，才允许既有 `--no-restore` publish/build/test 阶段。

原 SYS §7.2 与 M 的 future commands 中 singular lock argv 由本文替换；其它 deterministic
publish/build/test、process budget、parity、deployment mapping 和 B/L verdict 规则不变。

## 5. M04 前置与 acceptance

只有独立设计 R 对“原 SYS/M + 本 addendum”给出 `ACCEPT` 后，工程主程才可签 M04。
M04 必须是新的 sibling continuation，使用新 `m04` temp/material/evidence roots，并：

1. 重验 M01、M02、M03、Linux004/R seals；
2. 只重放 M01 approved material，不复制 M02/M03 partial restore scratch；
3. 采用本文四份 isolated source 与两个 project lock sets；
4. 保持 M03 已验证的 single-node process model、网络 `0` 起步和所有 zero-action；
5. 完成两个 generation、两个 locked verification 后，才继续 license/package closure、finite
   deployment mapping、future B/L budgets 与 material seal；
6. 仍只返回 `READY_FOR_INDEPENDENT_M_R`，作者不得自判 `M-ACCEPTED` 或自动进入 B/L。

独立设计 R 必须核查：

- 本文是否准确解释 M03 的覆盖证据，而非把成功 exit `0` 误作完整 graph；
- graph-scoped per-project lock set 是否与 NuGet project lock 语义一致；
- generation/verification 是否没有共享输出路径、没有逐项目扩大 top-level process budget；
- material schema与 future B 都消费同一 lock-set identity；
- 本文是否保持版本、source、license/signature、two-line patch、M/B/L 与 zero-build/test 边界。

R 只给一个 verdict：`ACCEPT`、`NEEDS_FIX` 或 `REJECT`。`NEEDS_FIX` 必须列出逐条可执行修正；
本文作者修订后仍回同一 R，不得另找新 R 绕过意见。
