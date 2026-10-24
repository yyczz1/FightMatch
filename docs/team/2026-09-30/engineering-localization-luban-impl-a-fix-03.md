# LOC-IMPL-A-FIX-03：真实许可证输入与真实进程回执

2026-10-01 · 状态：`PREPARED_NOT_AUTHORIZED`

## 0. 当前阻塞与责任

本包只纠正 LOC-IMPL-A FIX-02 独立 R 的 P1 license gate 与 P2 process evidence。
它不是实施授权；当前附加状态为 `BLOCKED_LICENSE_INPUT`。主程只有在第 2.3 节两套
一手许可证材料全部绑定并重签本文后，才可改为 `AUTHORIZED_FOR_ASSIGNED_OWNER`。

- 唯一未来 owner：原 LOC-A execution chat
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与回传：主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 原独立 R：thread `01a0e404-e8ee-7310-8388-9260babd53f1` / `local`；turn
  `01a0f437-52d6-7c30-a424-86b561f3c0f8`；final
  `msg_0d29bf8d726d8dc0016abd8257794887d0871f3e7f431baaa0`；唯一 verdict
  `NEEDS_FIX`。
- 本轮准备只允许新建本文。没有实施、Unity、Luban、005、Git、网络、QA 或 R。

P1/P2 之外的 264 records／264 unique、severity `13/5/74/172`、parameterized 78、
12/12 self-tests、三负例、双 raw/canonical/manifest/semantic 确定性和 8 路径范围已
被原 R 确认为成立；FIX-03 不重开、不改写这些合同。

## 1. 固定 authority 与旧证据

### 1.1 文件 authority

| 输入 | 固定身份 |
| --- | --- |
| `engineering-localization-luban-001.md` | SHA-256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；777 lines / 52,323 bytes |
| `engineering-localization-luban-impl-a.md` | `8e2626dad0d4b3c70854b9b7e22bdd48fce79ea91cececb7af45a9cd01a8487d` |
| `engineering-localization-luban-impl-a-fix-01.md` | `b17ff60ca4df6bbb580e96daba43792608737ca243fc5bd0573dd8663d084587` |
| `engineering-localization-luban-impl-a-fix-02.md` | `3d437c485903ab25569f9f417f49fd1653125428a4f3913843d15b7c35a62a35` |
| accepted COPY | `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf` |
| runner manifest | 132 files；SHA-256 `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` |
| toolchain | Luban `v5.1.0` / commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`；SDK `8.0.425` |

### 1.2 三个 immutable evidence roots

以下三个目录及其所有 descendants 原样只读，不覆盖、续写、删除、重命名或回填：

| Root | 当前闭集身份 |
| --- | --- |
| `TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/` | 37 regular files / 0 symlink；`run.json=e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`；`inputs.json=07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2` |
| `.../ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/` | 15 regular files / 0 symlink；`run.json=19151d7f0cd9a2bfeaeddf090d267a35941722ba1cc9a163bba229c4cabc8dc4`；`inputs.json=3852210c00e9a053691b9fd05bfc3e2642c84939e3d6088634f639873569281c`；`commands.json=3debf91a90e48c8c48828f97afeeb47a8dfe574d170f0825b7d3283eee67aa9b` |
| `.../ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/` | 74 regular files / 0 symlink；`run.json=43c4f7d9cf78194cd97b6a2b277c40a4bde9be38ad6f359e133dab17c25e3110`；`inputs.json=c191f93674f74199434c39a2c31b8a34e08e6a3925dca39dcec5d64a35af3582`；`commands.json=803a5f686d5999c8b7bd515227fa7cd39b6703f553ee6e6fe30aff008e52cb85`；license inventory `1bfeb05a91d247738b7ef36b5bdbb86b2526e6868ed5737b791f1288b23a352f` |

签发前主程须生成并冻结三个完整 path/bytes/SHA tree manifests：
`{{ORIGINAL_TREE_MANIFEST_SHA256}}`、`{{FIX01_TREE_MANIFEST_SHA256}}`、
`{{FIX02_TREE_MANIFEST_SHA256}}`。未来执行在新根分别写 before/after inventory 并逐项
相等；不向旧根写 manifest。

## 2. 精确文件范围与待绑定许可输入

### 2.1 当前 8 个生产路径

| 路径 | 当前 bytes / SHA-256 | FIX-03 权限 |
| --- | --- | --- |
| `Config/FightMatch/Luban/luban.conf` | 315 / `03ed736de1debd33a1d6c1ebbf30149c2876eb0458f0da34c4a819bee794dc41` | 只读 |
| `Config/FightMatch/Localization/__tables__.csv` | 399 / `5312d8f5367feda5a41370b5441b2e46d861e902faef5b1bb6b1b4c3795e6c35` | 只读 |
| `Config/FightMatch/Localization/FightMatchText.csv` | 65,798 / `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030` | 只读 |
| `Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj` | 294 / `37ab5325dc95ee3f9538cdcf70c56caceb250556448293e720c292ea9fafec70` | 只读 |
| `Tools/FightMatch.Localization.Compiler/Program.cs` | 80,452 / `f16e559b90dee2a423cdff1d453a21d55e241633a153a9f812e54cb9cbf05284` | 唯一默认 source 修改路径 |
| `Tools/FightMatch.Localization.Compiler/global.json` | 104 / `0a15550b7b496f0eb8ea6e405da955cf264bf519fb0fb60dd8a7e25967bbca5d` | 只读 |
| `Generated/FightMatch/Localization/fm-text-v1.json` | 121,098 / `f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc` | 全部门通过后才可由新 pass-1 原样发布；预期最终 bytes/SHA 不变 |
| `Generated/FightMatch/Localization/fm-text-v1.manifest.json` | 791 / `18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667` | 同上 |

parser、canonicalizer、schema、formal CSV、Luban config、global.json、csproj 都不得改。
默认不改另 7 个生产文件；Generated 即使被成功流程重新复制，最终也必须与新
pass-1 byte-identical，且因 P1/P2 不改变业务语义而保持上述 SHA，否则立即
`BLOCKED_UNEXPECTED_OUTPUT_DRIFT`。

### 2.2 唯一可能补签的新工具输入路径

默认 `{{VERSIONED_LICENSE_INPUT_PATHS_OR_NONE}}=NONE`。若第 2.3 节真实 license bytes
不能从既有、只读、版本可追溯的本地 source/cache 路径消费，而必须成为仓库内工具
输入，主程须在授权前把每个新路径逐项填入该 token、记录 SHA/bytes/source commit，
并重签本文。未逐项列明的路径一律禁止；执行者不得自行创建 `Licenses/`、notice、
template 或第三个 source file。

除 `Program.cs`、条件补签的精确 license inputs、新 FIX-03 evidence root 与成功后
byte-identical 的两个 Generated 外，所有文件都只读。禁止 Assets、Packages、
ProjectSettings、`.meta`、文档、外盘工具/cache、旧 evidence、Git history、Unity、
Windows、Host、LOC-B、RES、Android、device 或 release 变更。

### 2.3 `BLOCKED_LICENSE_INPUT` tokens

以下字段必须由主程基于已落地本地 bytes 与一手 source identity 填满；远程 URL、
浮动 branch/tag 名、无 license 元数据的 nuspec、SPDX 字符串或通用手写模板均不能
绑定这些 token：

| Package | 必填 token |
| --- | --- |
| YamlDotNet.NetCore `1.0.0` | `{{YAML_LICENSE_LOCAL_PATH}}`、`{{YAML_LICENSE_BYTES}}`、`{{YAML_LICENSE_SHA256}}`、`{{YAML_SOURCE_REPOSITORY}}`、`{{YAML_SOURCE_COMMIT}}`、`{{YAML_SOURCE_ENTRY_PATH}}`、`{{YAML_SOURCE_ENTRY_SHA256}}`、`{{YAML_VERSION_PROVENANCE_RECEIPT}}` |
| NeoLua `1.3.14` | `{{NEOLUA_LICENSE_LOCAL_PATH}}`、`{{NEOLUA_LICENSE_BYTES}}`、`{{NEOLUA_LICENSE_SHA256}}`、`{{NEOLUA_SOURCE_REPOSITORY}}`、`{{NEOLUA_SOURCE_COMMIT}}`、`{{NEOLUA_SOURCE_ENTRY_PATH}}`、`{{NEOLUA_SOURCE_ENTRY_SHA256}}`、`{{NEOLUA_VERSION_PROVENANCE_RECEIPT}}` |

`LOCAL_PATH` 必须在执行前已存在且只读；`SOURCE_COMMIT` 是不可变 full commit，
`SOURCE_ENTRY_SHA256` 必须与本地 license bytes 相等，version provenance 必须证明该
source commit/entry 确实对应固定 package version。任一 token 缺失或验证失败都保持
`BLOCKED_LICENSE_INPUT`，不创建 FIX-03 根、不运行 compiler/Luban，也不发布 Generated。

当前一手研究中间结论只作待正式 receipt 的约束，不解除阻塞：

- NeoLua 可绑定候选为 exact first-party tag `v1.3.14` → full commit
  `cb560afef9dd19b7afad6da594b44909c9739698`；该 commit 的 `LICENSE.md` 是
  Apache-2.0 full text，11,324 bytes，SHA-256
  `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae`。固定 nupkg
  `af8282ab27f9dafaf239558f882a42d65de2fd91bdca025ad4fc999c477d0269` 内没有
  LICENSE/NOTICE/COPYING。只有正式 research receipt 冻结
  本地 bytes、source entry 与 tag→commit 后，才可填写 NeoLua tokens。
- YamlDotNet.NetCore `1.0.0` 固定 nupkg
  `c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97` 内只有关系文件、nuspec、
  DLL、content-types 与 signature；nuspec 没有 projectUrl、repository、licenseUrl
  或 license。NuGet owner 的同日 fork 和最接近 upstream commit（已知 hash 前缀
  `b758a296`）不能证明该 modified package 的 exact source/license binding；其 MIT
  `LICENSE`（1,110 bytes，已知 SHA-256 前缀 `daea8b7f`）与 `LICENSE-libyaml`
  （1,058 bytes，已知 SHA-256 前缀 `d0d8b098`）都不得填入 Yaml tokens 或使
  inventory complete。

因此本 revision 的 Yaml tokens 明确不可填写，终态保持 `BLOCKED_LICENSE_INPUT`／
`LICENSE_INVENTORY_INCOMPLETE`。若无法取得 exact package 许可／source binding，唯一
替代路线是另行提交并获用户／架构批准的工具依赖或 runner 选择；它不属于 FIX-03，
不得在本包绕过。

### 2.4 不得变化的 package/deployed identities

| Package / deployed file | 固定 identity |
| --- | --- |
| YamlDotNet.NetCore `1.0.0` nupkg | `yamldotnet.netcore.1.0.0.nupkg` SHA `c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97` |
| `YamlDotNet.NetCore.dll` | 172,032 bytes；deployed/entry SHA `c7b7e80ce82038ed32d58a736b17bd6323571fbde0ad34fcac097cb579cfc94a`；entry `lib/netstandard1.0/YamlDotNet.NetCore.dll` |
| NeoLua `1.3.14` nupkg | `neolua.1.3.14.nupkg` SHA `af8282ab27f9dafaf239558f882a42d65de2fd91bdca025ad4fc999c477d0269` |
| `Neo.Lua.dll` | 321,024 bytes；deployed/entry SHA `acfbc880041d2022c237c32fad7d0d66ee7b2ac49fda71f5bc65c78882dbc335`；entry `lib/net5.0/Neo.Lua.dll` |
| `de/Neo.Lua.resources.dll` | 10,752 bytes；deployed/entry SHA `73b4e75980a944cbd0d96f5e6adfbd000aab781da0ac7500d0f612dae06f5a99`；entry `lib/net5.0/de/Neo.Lua.resources.dll` |

现有 YamlDotNet nuspec SHA
`871245d3394e3c4595ab1e6d89b151017a6fe75f1d82ec5d6ba657bb13e5d156` 与 NeoLua
nuspec SHA `a1018c0b7c986f7489e9a44685ea8d9955bb478be0318ba48574e4d3743e69e1`
只可作为 package provenance，
不得再写入 `licenseSource` 或使 `complete=true`。同一 NeoLua license bytes 可覆盖
两个同 package/version 的 deployed rows，但每行仍须分别记录自身 deployed/nupkg/
entry identity 与相同的真实 license SHA/source commit。

## 3. P1：严格 license gate

`Program.cs` 的 `ResolvePackageLicense`／inventory 只作以下最小修正：

1. 每个第三方 deployed DLL 只有同时验证 deployed bytes、固定 nupkg、精确 package
   entry 和真实本地 license/notice bytes 才能 `complete=true`。
2. nupkg 内真实 license/notice entry 继续按 entry bytes 取 SHA；找不到时只能使用
   第 2.3 节已绑定且通过 source commit/version provenance 的本地真实 bytes。
3. 任意 nuspec 存在、`licenseUrl`、license expression、README、Luban 主仓 MIT、
   无来源文本或通用模板都不能替代 license bytes。
4. 固定 package/nupkg/deployed/entry/source commit/license 任一 SHA、bytes、版本或路径
   mismatch，返回具体 missing reason；最终状态必须
   `LICENSE_INVENTORY_INCOMPLETE`，exit 32，repository Generated publication count=0。
5. inventory 仍覆盖 runner 132/132；`licenseSource` 精确指向 nupkg entry 或已绑定的
   local/source entry，另列 source repository/full commit/version provenance 与 license
   bytes/SHA。不得把 host 绝对路径写进发布 manifest。
6. 在 generate 内写 `license-gate-results.json`，至少逐项证明：真实两套输入通过；
   no-entry+nuspec-only、URL-only、generic template、license SHA mismatch、source commit
   mismatch、nupkg SHA mismatch、deployed entry SHA mismatch 全部失败。该矩阵不增加
   第 5 节 12 self-tests 或三项既有 negative command。

任何许可缺口都先形成真实 license inventory/gate evidence 再以
`LICENSE_INVENTORY_INCOMPLETE` 停止；不得联网补洞、改外盘 cache、跳过失败 row、
降低 132/132，或继续发布 Generated。

## 4. P2：真实 outer/child process evidence

### 4.1 单文件 outer/inner 结构

修正必须完全位于现有 `Program.cs`：

1. 同一 source 增加 internal outer launcher mode 与 inner generate mode；不新增仓库
   launcher/script/project。outer 是本次 production chain 的父进程，inner 不为自己
   写 `generate-driver` exit/stdout/stderr/timing。
2. 固定 Roslyn 先把同一 `Program.cs` 编为新根 `bootstrap/` 的 outer binary；该真实
   bootstrap child 由执行环境父层捕获到独立 `bootstrap-compiler-compile.*` 五件套。
   它不进入 `commands.json` 的 10 项 production closure，也不得省略或伪造。
3. bootstrap outer 用 `ProcessStartInfo.ArgumentList` 启动真实 Roslyn child，编译
   `compiler/` final binary并产生 `compiler-compile` receipt；只有实际 exit 0 才启动
   final binary 的 inner generate child并产生 `generate-driver` receipt。
4. inner generate 继续作为其它八个 child processes 的真实 parent；全部使用同一
   recorder。`commands.json` 由 outer 在 child 退出后从实际 receipt 构建，child 不
   能给自己补常量 exit、空流或时间。

若上述 bootstrap 无法在 `Program.cs`＋新 evidence 内完成，返回
`BLOCKED_PROCESS_EVIDENCE_DESIGN`；不得新增第二个 source、shell helper、csproj 或
产品路径。

### 4.2 每个真实 process receipt

每个已启动 child 必须由其 parent 捕获：

- `.argv.json`：实际 executable 与逐项 `argumentList`；不经 shell 拼接／重解析；
- `.stdout.txt` / `.stderr.txt`：从 redirected OS streams 并发读到 EOF 的完整原始流；
- `.exit.txt`：`WaitForExit` 后的真实 `Process.ExitCode`；
- `.timing.txt`：start 前后的 UTC timestamps、monotonic elapsed、started/completed
  标志及 parent/child PID。

start failure 也要写 truthful receipt：`started=false`、`childPid=null`、
`exitCode=null` 与稳定失败 stage，绝不写数字 0。stdout 合法为空只能来自实际 EOF；
不得预建空文件冒充捕获。成功的 `generate-driver.stdout.txt` 必须包含 child 的真实
`PASS_LOC_IMPL_A ...` 行，不能只存元数据。

任何 child exit 不符合预期时，parent 先完成 argv／两流／exit／timing 五件套，再写
partial `commands.json` 与失败 `run.json`，然后停止依赖步骤。receipt 写入本身失败则
终态为 evidence failure，不得声称该 process 已验证。

### 4.3 成功时精确 10 项 closure

成功的 `commands.json` 必须按实际执行序包含且只包含：

1. `compiler-compile`
2. `generate-driver`
3. `compiler-self-tests`
4. `pass-1-luban`
5. `compile-pass-1`
6. `pass-2-luban`
7. `compile-pass-2`
8. `negative-duplicate`
9. `negative-parameters`
10. `negative-severity`

每项由五件套读取生成，记录 receipt 文件 SHA/bytes，并反查 executable/argv、实际 OS
exit、stdout/stderr、started/ended/duration 一致。不得从常量、预期 exit、Console
输出或最后一条 `date` 的状态推导。成功 closure 精确 10；失败 closure 如实 partial，
不能补虚构项凑 10。

## 5. 新 FIX-03 evidence 与执行顺序

唯一新 create-once root：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-03/
```

授权前必须不存在。成功闭集沿 FIX-02 的 74 个相对文件，并增加以下 14 个，精确
88 regular files / 0 symlink：

```text
evidence-files.tsv
immutable-roots.before.tsv
immutable-roots.after.tsv
license-inputs.json
license-gate-results.json
licenses/YamlDotNet.NetCore-1.0.0.LICENSE.txt
licenses/NeoLua-1.3.14.LICENSE.txt
bootstrap/FightMatch.Localization.Compiler.dll
bootstrap/FightMatch.Localization.Compiler.runtimeconfig.json
bootstrap-compiler-compile.argv.json
bootstrap-compiler-compile.stdout.txt
bootstrap-compiler-compile.stderr.txt
bootstrap-compiler-compile.exit.txt
bootstrap-compiler-compile.timing.txt
```

其中两份 `licenses/*` 是第 2.3 节真实输入 bytes 的 evidence copy，不是手写替代。
FIX-02 的 74-file relative set 由 `{{FIX02_TREE_MANIFEST_SHA256}}` 冻结；FIX-03 的
`steps/` 仍为第 4.3 节 10 项各五件套。失败 run 可缺未启动步骤，但
`evidence-files.tsv` 必须把每个未创建项记为 `NOT_CREATED_AFTER_<stage>`，不得换名。

未来获准执行顺序：

1. 验证本文、Program、其余 7 路径、三个旧 tree manifests、runner/SDK/Luban/COPY
   和第 2.3/2.4 节全部身份；不符即在创建新根前 `BLOCKED_INPUT_DRIFT`。
2. 验证 FIX-03 根不存在且无 symlink；创建根与 evidence closure。
3. 实际捕获 bootstrap compile，再由 outer 捕获 final `compiler-compile` 和
   `generate-driver`；所有 failure 先落真实 receipt。
4. inner 从头运行原 12/12 self-tests、两个全新 Luban passes、两个 canonical compile、
   duplicate/parameters/severity 三负例、comparison/semantic 与严格 license gate。
   不重跑 005、不下载、不运行 `dotnet restore/publish`、不换工具版本。
5. 只有 12/12、两 pass、三负例、确定性、P1、P2、132/132 和 scope 全通过后，才把
   新 pass-1 两文件复制到 Generated；最终必须与 pass-1 bytes/SHA 一致。
6. 写 diff-check 与三个旧根 after inventory；任何旧 evidence byte/path/symlink 变化
   都失败。停止写入，回传主程，不自行请求 QA/R。

## 6. 可观察验收与回传

- [ ] 最终 production delta 只有 `Program.cs`；若有 license input 新路径，它们已在
  `{{VERSIONED_LICENSE_INPUT_PATHS_OR_NONE}}` 逐项补签。其余 7 路径最终 bytes 符合
  第 2.1 节，两个 Generated 仅在全门通过后与新 pass-1 相同。
- [ ] YamlDotNet.NetCore 和 NeoLua 三个 deployed DLL 分别绑定真实本地 license bytes、
  固定 source commit/version provenance；nuspec/URL/template 不能 `complete=true`。
- [ ] runner/package/nupkg/deployed entry 固定 SHA 不变；inventory 精确 132/132，
  missing=0，license gate matrix 全通过。
- [ ] bootstrap receipt 真实；成功 `commands.json` 恰有第 4.3 节 10 项且全部来自 parent
  捕获，argv/OS exit/stdout/stderr/timing/receipt SHA 内部一致。
- [ ] 12/12 self-tests、两 raw passes、三既有 negatives、canonical/manifest/semantic
  comparison、license 与 diff-check 全通过；首次失败立即停止，零候选发布。
- [ ] 三个旧 evidence roots 的 before/after tree manifests 逐 path/bytes/SHA 相同；005
  未重跑，外盘 cache 零修改。
- [ ] Unity、Host、LOC-B、Windows、Android、device、CDN、release 全部 `NOT_RUN`。

未来 owner 回传：精确 8 个路径的 before/after SHA/bytes、可能补签的 license input
路径、`Program.cs` 最小 diff、88-file closure、三个旧根 before/after、两套 license
identity/provenance、132/132 inventory、10 项真实 process receipts、12/12、两 pass、
三负例、comparison/semantic、Generated 与 diff-check；未运行项逐项列出。

主程审 actual patch/evidence 后才交 QA，由 QA 把 FIX-03 送回同一原 R。新 R 必须绑定
新 Program/evidence identities并只给一个 `ACCEPT`、`NEEDS_FIX` 或 `REJECT`；在新
`ACCEPT` 前 LOC-A 未接收，LOC-B 不得签发。当前 prepared 回合不发送 QA/R。

## 7. prepared 自检

- 固定生产路径：8；默认 source 修改：1（`Program.cs`）；条件新 license input token：
  `{{VERSIONED_LICENSE_INPUT_PATHS_OR_NONE}}` 当前未绑定，签发值可为精确路径列表或
  `NONE`。
- 修正项：2/2，仅 P1 license gate 与 P2 process evidence。
- license blocker：16 个一手材料 token 均未绑定；状态保持
  `BLOCKED_LICENSE_INPUT`，未猜许可证。
- process closure：10 个命名 command，各五件套；bootstrap 五件套独立且不冒充第
  11 个 production command。
- 成功验证：12 self-tests、2 Luban passes、3 negatives、comparison/semantic、
  license 132/132、diff-check；005/Unity/网络/Git/QA/R 均未运行。
- 新 evidence：一个 create-once FIX-03 root；旧 original/FIX-01/FIX-02 三根只读。

**结论：`PREPARED_NOT_AUTHORIZED / BLOCKED_LICENSE_INPUT`。**
