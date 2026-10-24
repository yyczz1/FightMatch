# LOC-LIC-ALT-ARCH-01：Luban YAML 许可证阻塞最小替代架构

2026-10-01 · 状态：**PREPARED_NOT_AUTHORIZED**。

ARCH-FIX-03 修订责任：architecture role／当前 thread
`01a0f2e6-1ac3-7d10-8855-54192b9489d4`。修订 preimage SHA-256
`35afce4785af87f5cf0e562117a0ab2cc1dbe7429af06c8dbe17c4a82120d1ff`；固定 system
design SHA-256 `18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`；
research SHA-256 `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`。
修订针对独立 R 的 `NEEDS_FIX`：R turn
`01a0f484-8745-7b62-8057-3d1c49210595`，final message
`msg_0d29bf8d726d8dc0016abd94a6ec0087d0934d80e2063745ad`。责任范围仅为本架构文件；
system design、implementation、QA 与独立接收仍分别归其 owner。

本文只做只读架构研究。它不授权修改生产代码、外盘工具／cache、既有 evidence、
Unity 项目、Git 或任何第三方系统，也不授权运行 Luban、LOC-TOOL-01-PREFLIGHT/005、
Unity、restore、build、publish、测试或生成。本文没有联系第三方、没有使用付费服务，
只读取固定本地输入和官方一手来源；为核 package bytes，研究阶段仅把 NuGet／GitHub
官方内容下载到 `/private/tmp`，未写入 FightMatch 仓库或既有工具 cache。

结论先行：

- 优先级 A 当前 **NO-GO**。官方 Luban `v5.1.0` 的发布包、源码构建和官方项目文件都
  直接绑定 `YamlDotNet.NetCore 1.0.0`；没有找到官方 build configuration 能在保留
  Luban CLI 和现有功能的同时移除该依赖。
- 推荐的唯一后续候选是本文 **ARCH B1 = SYS Candidate A**；两者唯一且同义地指向：
  保留官方 Luban `v5.1.0`／commit
  `df34215d2e22035073d1af83d44e3ac0744dc3b3` 的全部 C# implementation，只在一个全新、
  外置、明确标为 **FightMatch patched runner** 的 source copy 中，把两个
  `PackageReference` 从 `YamlDotNet.NetCore 1.0.0` 改为官方 `YamlDotNet 16.3.0`。
  不存在第二条候选路线。
- 该候选现在只是 **CONDITIONAL_GO_FOR_A_SEPARATE_PROOF_PACKET**，不是 accepted runner，
  更不能冒充官方 `v5.1.0` pin。只有第 10 节全部 go gate 通过并获独立审查
  `ACCEPT`，才可替换 LOC-A 的制作期 runner identity。
- 未来证明必须拆成三个单向阶段：材料／闭包冻结 → 外置 patched-runner 构建与行为
  证明 → LOC-A `Program.cs` 适配与生产语义验证。后阶段只能消费前阶段已冻结并接收的
  输出；前阶段不得把未来 runner SHA、构建产物或生产验证结果列为自身签包输入。
- 优先级 C 的旧包 source→DLL 复现不建议实施：package 没有 exact source binding，
  最强候选仍使用 2016 年 `project.json` build model；在固定 SDK `8.0.425` 下建立
  可独立复核的 exact byte provenance，成本和不确定性都高于只替换依赖。

## 1. 固定 authority 与不得变化的产品合同

### 1.1 文件与工具输入

| 输入 | 固定身份 |
| --- | --- |
| localization design | `engineering-localization-luban-001.md`；SHA-256 `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`；777 lines / 52,323 bytes |
| FIX-02 packet | `engineering-localization-luban-impl-a-fix-02.md`；SHA-256 `3d437c485903ab25569f9f417f49fd1653125428a4f3913843d15b7c35a62a35` |
| FIX-03 packet | `engineering-localization-luban-impl-a-fix-03.md`；SHA-256 `374234b8ed0f015af13eeff3771c15343a2d497e7fc54909152dad207322a812` |
| license research | `research-loc-a-license-sources.md`；SHA-256 `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550` |
| Luban | official tag `v5.1.0`；commit `df34215d2e22035073d1af83d44e3ac0744dc3b3` |
| accepted runner | 132 files；manifest SHA-256 `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5` |
| SDK | `.NET SDK 8.0.425`；Mac Intel `osx-x64`，未来 Windows `win-x64` |

### 1.2 不得被替代路线改变的合同

1. 框架仍是官方 Luban；Excel／CSV 仍是唯一人工配置源，链路仍是
   `Excel/CSV → Luban → project-owned compiler → existing review/publisher`。
2. `v5.1.0` 的 CLI interface 保持：相同 executable 入口、argument token 顺序、
   `--strict --errorFormat json`、stdout／stderr／exit 语义和 staging 所有权。
3. 正式 source、schema、264 records／264 ordinal unique keys、severity
   `blocking/error/warning/info=13/5/74/172`、parameterized `78`、locale mapping、
   canonical property order、manifest 字段和 semantic digest 规则不变。
4. 两个最终生成物的已接收 bytes/SHA 仍是期望值：
   `fm-text-v1.json=f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc`；
   `fm-text-v1.manifest.json=18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667`。
5. Luban、YamlDotNet 和 patched runner 全部只属于制作期，不进入 Unity Player、
   Android APK、Core、Application、Save、serialization 或玩家可见 interface。
6. 不改变 Unity `2022.3.18f1`、.NET／Unity／Game 规则、Luban data-only JSON 决定、
   `ILocalizedTextSource` 后续 seam、YooAsset／ReleaseSet 责任或既有发布器。
7. 不用 Unity Localization、Addressables，不把 JSON 变成第二人工权威，不扩到
   LOC-B、Host composition、RES、Android、Windows acceptance、CDN 或 release。

## 2. module、interface、seam 与 adapter

本路线把制作工具视为一个深 **Module**：调用者只学习冻结的命令、输入树、输出树、
错误和确定性合同；restore graph、YAML parser 和 DLL 布局都留在其
**Implementation** 内。它的外部 **Interface** 就是第 1.2 节，不新增公开 method、
参数、业务 DTO 或运行时 package。

替换点放在 Luban 外置 source build 的 package-resolution **Seam**。官方
`YamlDotNet` 是该 seam 后面的 concrete **Adapter**；它只为 Luban 内部的 YAML
loader／target 提供相同 `YamlDotNet.*` namespace 和 representation-model types。
FightMatch 当前正式链只走 CSV input 和 JSON output，但不能据此删除 Luban 的 YAML
功能；因此必须以 official tests 和 YAML 对照证明 adapter 没有破坏 runner，而不能
把依赖简单裁掉。

这个 seam 有两个真实 adapter：历史 `YamlDotNet.NetCore 1.0.0` 和候选官方
`YamlDotNet 16.3.0`。无需在 FightMatch 生产源码再造 interface、wrapper 或 shim；
那会形成浅 Module，并把第三方差异泄漏给 caller。依赖替换、许可证证据和 runner
manifest 都集中在制作工具 implementation，给调用方保持 leverage，给维护者保持
locality。

## 3. 官方一手事实

### 3.1 Luban 官方线

- GitHub 官方 release `v5.1.0` 指向 commit
  `df34215d2e22035073d1af83d44e3ac0744dc3b3`，是本研究日可见的 latest release；
  release 资产 `Luban.7z` 已由固定设计记录为 1,647,739 bytes、SHA-256
  `bac9a1b8d69cfeaa7ef1d8c67b34a007324d1cd158e0b14294521b3879ba5213`。
  来源：[official v5.1.0 release](https://github.com/focus-creative-games/luban/releases/tag/v5.1.0)。
- 官方安装文档只给出三种获得方式：release、examples 自带和源码构建；源码构建要求
  .NET SDK 8.0+，没有列出禁用 YAML 或替换 YAML package 的官方 build flag。
  来源：[Luban 安装与获取工具](https://www.datable.cn/docs/guide/install)。
- `v5.1.0` 的 CLI project 明确引用 `YamlDotNet.NetCore 1.0.0`；
  `Luban.DataLoader.Builtin` 也直接引用同一 package。官方源码没有条件属性、feature
  flag 或 alternate package slot。
  来源：[tagged Luban.csproj](https://github.com/focus-creative-games/luban/blob/v5.1.0/src/Luban/Luban.csproj)、
  [tagged data-loader csproj](https://github.com/focus-creative-games/luban/blob/v5.1.0/src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj)。
- 官方 CI 的 interface 是 solution restore、Release build 和官方 xUnit integration
  tests；它也没有 YAML-free configuration。
  来源：[tagged CI workflow](https://github.com/focus-creative-games/luban/blob/v5.1.0/.github/workflows/ci.yml)、
  [official repository test instruction](https://github.com/focus-creative-games/luban/tree/v5.1.0#%E5%BC%80%E5%8F%91%E8%80%85%E8%BF%90%E8%A1%8C%E6%B5%8B%E8%AF%95)。
- Luban 本身是 MIT；固定 local source 的 `LICENSE` 为 1,088 bytes、SHA-256
  `a7bfa87431fde61f7408e30cd6ce57fe676638c651f5c04dc90fa3de43d28841`。
  这只覆盖 Luban implementation，不能替代第三方 package license。
  来源：[tagged Luban LICENSE](https://github.com/focus-creative-games/luban/blob/v5.1.0/LICENSE)。

### 3.2 官方 YamlDotNet `16.3.0`

本研究选择 `16.3.0` 作为唯一 B 候选，不是因为它是最新版本，而是因为它是有清晰
source/package/license binding、直接提供 `net8.0` DLL、无 package dependency，且
早于 17.1 的 large-YAML behavior change、18.0 的 breaking interface 和 18.1 的默认
recursion limit 的最小固定点。

| 项 | 精确 identity |
| --- | --- |
| official package | `YamlDotNet 16.3.0`；[NuGet page](https://www.nuget.org/packages/YamlDotNet/16.3.0)；[immutable nupkg URL](https://api.nuget.org/v3-flatcontainer/yamldotnet/16.3.0/yamldotnet.16.3.0.nupkg) |
| release/tag | [`v16.3.0`](https://github.com/aaubry/YamlDotNet/releases/tag/v16.3.0)；GitHub verified release |
| source commit | [`ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73`](https://github.com/aaubry/YamlDotNet/commit/ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73) |
| nupkg | 776,880 bytes；SHA-256 `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4`；SHA-512 `f102fb418c3afe5ec0cc5d0336b9ac6517af6f30603ab5f925a7b3622544c419bb935d1e55901147fff5a113874e55491534d5553411fb4ea198e6550c0421df` |
| nuspec | 1,340 bytes；SHA-256 `f1573d3b0c83844d8ed3ae016240cb18cd3674b8f5d82b40fcccbe49ff97774e`；`license=MIT`；repository URL=`https://github.com/aaubry/YamlDotNet`；repository commit=`ae480660...` |
| package signature | `.signature.p7s` present；SHA-256 `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308`；未来 proof 仍须用固定 SDK/NuGet verifier 实际验证，不能把“存在”写成“验证通过” |
| selected assembly | `lib/net8.0/YamlDotNet.dll`；293,376 bytes；SHA-256 `86f873f682c619a0c24e263dcdd15fe027104666641c92f503516671d61edda6` |
| source archive capture | [official tag archive](https://codeload.github.com/aaubry/YamlDotNet/tar.gz/refs/tags/v16.3.0)；本次 capture 397,462 bytes、SHA-256 `4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce`；future receipt 以 commit tree 为 authority，重新记录 archive bytes，不假设 GitHub 生成 archive 永远同字节 |
| main license | [`LICENSE.txt` at exact commit](https://github.com/aaubry/YamlDotNet/blob/ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73/LICENSE.txt)；MIT；1,110 bytes；SHA-256 `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3` |
| libyaml notice/license | [`LICENSE-libyaml` at exact commit](https://github.com/aaubry/YamlDotNet/blob/ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73/LICENSE-libyaml)；MIT-form；1,077 bytes；SHA-256 `cb65f46df0d9125ba60862117200ec6631fef3a8169cd0b4513ffcad4ef5cb9b` |

以上 nupkg、nuspec、signature、DLL 和 source archive hashes 是 2026-10-01 从官方
URLs 取得的只读研究 capture；它们不是已落地工具输入。未来 proof packet 必须把
相同 bytes 保存到全新 candidate cache/evidence，逐项复算，不能引用本文替代实际
receipt。

### 3.3 为什么不选后续 `17.1.0`／`18.1.0`

`17.1.0` 同样有完整一手 provenance：tag `v17.1.0`、commit
`ba6dd7d1ff135ef4fc1db1ba1053564b33d0b100`、nupkg 789,644 bytes／SHA-256
`65d970eb6ad5b9dd94f780502aaae9e4e150883710f6b46b29e6959cdc511236`、nuspec
SHA-256 `6100990bed3670b474eebc9449412166506f0a9c2d69a15e7ef44e4ce94d1a7a`、net8 DLL
298,496 bytes／SHA-256
`94afbd76ed0ca8de11c4188c9a052c4ae964134160407eb6af8a55ec28f9ecdc`；nuspec 绑定
official repository／exact commit／MIT，exact commit 的 `LICENSE.txt`／
`LICENSE-libyaml` 分别仍为 1,110／1,077 bytes、SHA-256 `daea8b7f...`／
`cb65f46d...`。但官方 `17.1.0` release 明示 MergingParser 的安全改进对超过默认
100,000 events 的大 YAML 可能是 breaking behavior。因此它虽比 `16.3.0` 更新，却
不是“只解除许可证阻塞”的零行为差候选。
来源：[NuGet 17.1.0](https://www.nuget.org/packages/YamlDotNet/17.1.0)、
[official 17.1.0 release](https://github.com/aaubry/YamlDotNet/releases/tag/v17.1.0)。

`18.1.0` 也有完整一手 provenance：tag `v18.1.0`、commit
`748334a8fa7c227740018b284b71ad95cc6b7fc7`、nupkg 795,624 bytes／SHA-256
`59ffe65ade67ad9d886267f877b634a450363cb81b94e19db9ca4c36461416f6`／SHA-512
`b9ec6e231b7b90819a5a5a7e2a3e554ed10012205f9712e890b69fa57f570d029bda369375d8a894b9476c571766b985c5eeaf6b474bae9bb9d47b594274abce`、
official catalog base64 SHA-512
`uexuIxt7kIGaWlp+Kj5VTtEAEiBflxLokLafpX9XDQKb2jaTddiolLlHbFcXZrmFxe6va0dLrpu51HtZQnSrzg==`、net8 DLL
300,032 bytes／SHA-256
`4fa072043bf289f0cbea622ac72c825991c924ad26537c0414699f4fec9a6cd1`；nuspec SHA-256
`362c3e6b2b0a04cae354ab97ea3d4d3945bb20390c7dcd2a696b73dc012b3036`，仍绑定 MIT 和
该 exact commit。official source tag ZIP 本次观察为 836,261 bytes／SHA-256
`50a0d7b0797741505c4ec17b2adffa21bea6b7f31edce0b180f8cc331fcbfe5f`；canonical
source identity 仍是 tag/full commit，不是 GitHub-generated archive hash。exact commit
的 `LICENSE.txt` 为 1,110 bytes／SHA-256
`daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3`，
`LICENSE-libyaml` 为 1,077 bytes／SHA-256
`cb65f46df0d9125ba60862117200ec6631fef3a8169cd0b4513ffcad4ef5cb9b`。
来源：[NuGet 18.1.0](https://www.nuget.org/packages/YamlDotNet/18.1.0)、
[official release](https://github.com/aaubry/YamlDotNet/releases/tag/v18.1.0)。

但 `18.0.0` 官方 release 明示 `ITypeInspector` interface breaking change，`18.1.0`
又引入默认 maximum recursion 130。即使 FightMatch 当前 CSV→JSON path 不调用 YAML，
这也是 Luban runner 功能语义变化，超出“只解除许可证阻塞”的最小目标。
来源：[18.0.0 breaking release note](https://github.com/aaubry/YamlDotNet/releases/tag/v18.0.0)、
[18.1.0 recursion release note](https://github.com/aaubry/YamlDotNet/releases/tag/v18.1.0)。
因此 `17.1.0`／`18.1.0` 都作为明确负对照保留，不进入本轮 version roulette。

选择 `16.3.0` 是受限于本任务“最小行为变化”的兼容性 pin，不是断言它比后续版本
更安全、更新或更适合通用 YAML 服务。candidate runner 只允许处理 FightMatch 的受信
本地 CSV→JSON 制作链；不得把它暴露给不受信 YAML 输入，也不得复用为通用 Luban
distribution。若未来需要通用 YAML、外部输入或采用后续安全行为，必须重新评估
`17.1.0`／`18.1.0`，不能继承本包的 GO。

## 4. 候选矩阵与排除理由

| 优先级／候选 | official provenance | 变更面 | 结论 | 理由 |
| --- | --- | --- | --- | --- |
| A1：官方 Luban `v5.1.0` release asset | tag `v5.1.0`、commit `df34215...`、asset SHA `bac9a1b8...`、Luban MIT SHA `a7bfa874...` | 不改 Luban source | **NO-GO** | official binary/source graph 仍含旧 `YamlDotNet.NetCore 1.0.0`；它不能解除 exact license/source binding blocker。 |
| A2：官方 `v5.1.0` source build／官方 CI configuration | 同一 tag/commit；两个 tagged csproj 明确旧 package；CI restore/build/test | 不改 source | **NO-GO** | 没有官方 YAML-free、package override 或 alternate adapter configuration；“只用 CSV”不会让 direct package reference 从 restore/deploy graph 消失。 |
| A3：等待未来 official Luban release | 尚无 tag/commit/package SHA | 未知 | **NO-GO_NOW** | 当前没有可核 exact input，不能以未来版本替代当前任务；若上游以后正式更换，必须另做新 design，不在本包猜版本。 |
| B1：official `YamlDotNet 16.3.0` 替换旧包 | tag `v16.3.0`、commit `ae480660...`、nupkg SHA `e068bcc1...`、nuspec exact commit、两份 commit-bound license bytes | 外置 Luban source copy 的两个 csproj 各一处 PackageReference；零 Luban C# source change | **RECOMMENDED / CONDITIONAL GO** | 最小可审 diff；net8.0、无 package transitives；许可证/source/package 可形成闭合 provenance。必须用行为和输出对照证明兼容。 |
| B2：official `YamlDotNet 17.1.0` | tag `v17.1.0`、commit `ba6dd7d1...`、nupkg SHA `65d970eb...`、MIT/source commit 完整 | 同 B1 | **NO-GO_THIS_ROUND** | 官方 release 明示 MergingParser 默认 100k-event 上限可能破坏大 YAML；不是零行为差替代。 |
| B3：official latest `YamlDotNet 18.1.0` | tag `v18.1.0`、commit `748334a8...`、nupkg SHA `59ffe65a...`、MIT/source commit 完整 | 同 B1 | **NO-GO_THIS_ROUND** | 官方 release 已列明 18.0 interface break 和 18.1 default recursion behavior；不是最小风险替代。B1 失败时也不得自动试 B2/B3。 |
| C1：从 `YamlDotNet.NetCore 1.0.0` 最强 source candidate 重建 exact DLL | package SHA `c88b1a0c...`；DLL SHA `c7b7e80c...`；candidate commit `3eaa273a69855d3332bbb071ea9a0d2c52972d20`，base `b758a2961bdb0b042f7f94e1fe6cddbb34756402`；candidate licenses `daea8b7f...`／`d0d8b098...`；**无 package→commit/tag binding** | 需要复原旧 build graph/toolchain并做 byte/IL proof | **NO-GO_WITHIN_REASONABLE_COST** | package 不含 repo/source/license/PDB/SourceLink；candidate 用 `project.json`，而该格式早已被 PackageReference 替代。任何 csproj migration 都会改变 build recipe，byte mismatch 也不能证明 exact package 许可。来源：[candidate source](https://github.com/adamhathcock/YamlDotNet/commit/3eaa273a69855d3332bbb071ea9a0d2c52972d20)、[Microsoft project.json history](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/10.0/dotnet-restore-project-json-unsupported)。 |

旧包的精确 NuGet record 仍是有效负证据：nupkg SHA-256
`c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97`，deployed DLL
SHA-256 `c7b7e80ce82038ed32d58a736b17bd6323571fbde0ad34fcac097cb579cfc94a`，而 metadata
没有 license、project 或 repository binding。来源：
[NuGet exact registration](https://api.nuget.org/v3/registration5-semver1/yamldotnet.netcore/1.0.0.json)、
[immutable old nupkg](https://api.nuget.org/v3-flatcontainer/yamldotnet.netcore/1.0.0/yamldotnet.netcore.1.0.0.nupkg)。

## 5. 推荐单一路线：ARCH B1 = SYS Candidate A patched runner

### 5.1 命名与身份，不得冒充官方 pin

候选 display identity 必须精确分成两层：

```text
official base:
  Luban v5.1.0
  commit df34215d2e22035073d1af83d44e3ac0744dc3b3

FightMatch derivative:
  Luban v5.1.0 df34215 FightMatch patched-runner / YamlDotNet 16.3.0 / r1
  patch-manifest SHA-256 <measured at dispatch>
  runner-manifest SHA-256 <measured after proof build>
```

不得把 derivative 写成“official Luban `v5.1.0` runner”、不得沿用旧 132-file
manifest SHA、不得覆盖现有 `5.1.0/runner`，也不得把 GitHub tag 改写成项目自己的
tag。发布 manifest 仍只记录平台中立的 official Luban tag/full commit 和 SDK version；
derivative runner/package identity 只进 tool/evidence identity，不泄漏进玩家 artifact。

### 5.2 唯一 source patch

在 exact official source copy 中只允许：

```diff
- <PackageReference Include="YamlDotNet.NetCore" Version="1.0.0" />
+ <PackageReference Include="YamlDotNet" Version="16.3.0" />
```

精确出现于：

1. `src/Luban/Luban.csproj`；当前固定 local bytes SHA-256
   `5a29bc8a18a2eae222d412ac4f2485ae366ffd7146cc2c1bf397e5554dde30c9`；
2. `src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj`；当前固定 local bytes
   SHA-256 `0f84ba758efa3ec4b8f878abf3b0429f34cc23bbf9a01f81c164878658839516`。

零 `.cs`、solution、CI、test、template 或其它 project file change。若 clean compile
需要任何 source shim、namespace edit、binding redirect、reflection、conditional
compile 或第三个 package change，立即 `NO_GO_PATCH_EXPANDED`，不得临场扩 scope。
旧 runner 的 dependent assemblies 引用 assembly name `YamlDotNet.NetCore`，新 package
的 assembly name 是 `YamlDotNet`；因此直接把新 DLL 覆盖到旧 runner 是无效且禁止的。
必须从上述两处 PackageReference patch 后的 exact Luban source clean rebuild，再让
compiler 重新绑定新 runner manifest。

### 5.3 制作期路径拓扑

既有 accepted cache 保持只读：

```text
Mac:     /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/
Windows: D:\ApplicationData\Tools\Luban\5.1.0\
```

M0（即 SYS M）未来 material packet 只能绑定下列新 material root，不能现在创建：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m01/
```

R1（即 SYS B）技术关通过后只能 create-once 下列 final runner root，不能现在创建：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-r1/
```

上述外置 roots 内部职责保持分离：M0 material root 保存 official/patched source、冻结 feed、
locks、licenses、harness 与 identity；R1 final runner root 只保存已证明的 runner、identity、
冻结 package bytes 与 licenses。共同使用下列职责名称时，不表示两个 root 可以互写：

```text
download/         exact official nupkg + source archive captures
source-official/  exact df34215... bytes, read-only after identity receipt
source-patched/   only the two csproj deltas
nuget/            isolated restore cache; never reuse/alter accepted 5.1.0 cache
work/<run-id>/    create-once obj/build/publish temporary paths
runner/           candidate final runner, never overlaid on accepted runner
licenses/         every locked deployed package/assembly's exact accepted license source bytes
identity/         material/closure, package/source/patch/runner manifests, SBOM and receipts
```

三个阶段的 proof evidence 分别使用以下精确、全新、create-once ignored roots：

```text
TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m01/
TestArtifacts/FightMatch/LOC-LIC-ALT-B/loc-lic-alt-yamldotnet-16.3.0-b01/
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-license-alt-yamldotnet-16.3.0-r1/
```

旧 005、LOC-IMPL-A original／FIX-01／FIX-02 roots 全部只读。未来通过后 LOC-A 也必须
使用新的 create-once sibling evidence root，不覆盖 FIX-03 预备包指向的历史根。
三个阶段各有独立 owner、authority receipt、输入 manifest、输出 manifest 和 verdict；
目录相邻不表示可双向读取。R1/SYS B 只能读取已接收的 M0/SYS M 输出，L1/SYS L 只能
读取前两阶段已接收输出。Windows 只能由未来独立 packet 另行固定路径和验证；本文不
提供当前 Windows candidate path，也不与以上 Mac paths 构成替代或冲突。

## 6. 对现有路径和 graph 的影响

### 6.1 当前五个 package/deployed identities

| 当前 identity | B1 处置 |
| --- | --- |
| `YamlDotNet.NetCore 1.0.0` nupkg `c88b1a0c...` | 从 candidate restore/runner graph 移除；历史 cache/evidence 保留 |
| `YamlDotNet.NetCore.dll` 172,032 bytes／`c7b7e80c...` | candidate runner 中必须不存在；历史 runner 不删 |
| `NeoLua 1.3.14` nupkg `af8282ab...` | 精确保留 |
| `Neo.Lua.dll` 321,024 bytes／`acfbc880...` | 精确保留 |
| `de/Neo.Lua.resources.dll` 10,752 bytes／`73b4e759...` | 精确保留 |

新增 candidate identities 只有：`YamlDotNet 16.3.0` nupkg
`e068bcc1...` 和 selected net8 DLL `86f873f6...`。package nuspec 对所有 target groups
声明零 dependency，因此任何新 transitive package 都是 `NO_GO_UNEXPECTED_GRAPH`。
新 runner file count 和 manifest SHA 必须实测；不得预填为旧的 132／`d9baac...`。

### 6.2 整个 locked deployed closure 的许可证门

上表五项只描述本次 YAML／NeoLua 已知身份，不是完整 license closure。M0 必须只根据
exact lock、逐项 nupkg entries、official source/build rules 与 finite expected deployment
list，冻结有限的 `expected package → expected deployed path/entry → license` mapping，并为
每项绑定 official source/tag/commit 与 license bytes。M0 不得读取、要求或反推未来
publish output、runner manifest、runner file count 或 candidate DLL hash。不能因某个预期
package/assembly 未被 FightMatch CSV→JSON 主路径调用、nupkg 有 `.nuspec`、或旧
inventory 曾写 `complete` 就跳过。

除 Luban、YamlDotNet、NeoLua 外，当前 runner 已知至少有以下六个 nupkg 内无实际
license text 的 deployed packages/assemblies；它们是材料／闭包阶段的显式阻塞清单，不是允许用 NuGet
license expression 代替原文的例外：

| 已知 package/assembly | 当前版本 | 材料阶段必须取得 |
| --- | --- | --- |
| `ExcelDataReader` | `3.7.0` | exact upstream source URL、tag/full commit、license path、原始 bytes/size/SHA-256，以及 package/deployed binding |
| `Google.Protobuf` | `3.29.0` | 同上 |
| `MessagePack` | `3.1.7` | 同上 |
| `MessagePack.Annotations` | `3.1.7` | 同上；不得仅继承 `MessagePack` 行而不证明覆盖关系 |
| `NLog` | `5.3.4` | 同上 |
| `Scriban` | `7.2.5` | 同上 |

M0 还必须从新 lock、逐项 nupkg entries、source/build rules 与 finite expected deployment
list 枚举不在此表的所有 direct/transitive/expected-deployed 项；表中“六个”不是 closure
上限。任一 expected deployed path/entry 无 exact package owner、任一 package 无 source
commit，或任一 license 无 exact bytes/hash 时，M0 返回 `BLOCKED_LICENSE_CLOSURE`，不得
进入 runner build。B 必须分别枚举两个实际 publish outputs，并将每个实际集合与 M0
冻结的有限 expected mapping 做集合等值和 exactly-once 核验；missing、unexpected 或
duplicate 任一出现即 B 失败，且不得反向补写 M0。最终 `licenses/`、SBOM、package
manifest 和 `Program.cs` 的 license gate 都必须消费这一完整冻结清单，不得继续使用
`.nuspec` 作为 license fallback。

### 6.3 八个 LOC-A 生产路径

| 路径 | 候选影响 |
| --- | --- |
| `Config/FightMatch/Luban/luban.conf` | bytes/SHA 不变 |
| `Config/FightMatch/Localization/__tables__.csv` | bytes/SHA 不变 |
| `Config/FightMatch/Localization/FightMatchText.csv` | bytes/SHA 不变 |
| `Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj` | bytes/SHA 不变；不增加 YamlDotNet reference |
| `Tools/FightMatch.Localization.Compiler/Program.cs` | 唯一预期 production source delta：把 accepted runner/package/license fixed identities 切换为经过 proof 的 derivative identities；不得改 parser、canonicalizer、process ordering、negative gates 或 publish behavior |
| `Tools/FightMatch.Localization.Compiler/global.json` | bytes/SHA 不变，继续锁 SDK `8.0.425` |
| `Generated/FightMatch/Localization/fm-text-v1.json` | L1 production pass 可重新生成，但最终 bytes/SHA 必须与当前接受值相同；否则不发布 |
| `Generated/FightMatch/Localization/fm-text-v1.manifest.json` | 同上；不得写 patched runner presentation identity |

因此 accepted repository diff 的上限是 `Program.cs` 一个 production source file；两个
Generated 文件若流程重新复制，final diff 必须为零。外置 Luban source patch、runner、
NuGet package、licenses、temporary work 和 receipts 都不进入仓库产品路径。

## 7. 证明设计：三个单向阶段

未来工作必须由三个分别签发、分别封存、单向消费的阶段完成。每阶段都要冻结 owner、
真实 thread/turn、authority SHA、create-once root、argv、进程树上限和返回合同；本文不
授权任何阶段执行。

### 7.1 M0 — 材料／闭包冻结

M0 只回答“后续可以使用哪些 exact bytes、依赖和命令”，不构建 runner，也不修改
`Program.cs`。

| 项 | M0 合同 |
| --- | --- |
| 输入 | 本文固定输入、official Luban source identity、候选 `YamlDotNet 16.3.0` 一手材料、当前只读 runner/lock 作为对照 |
| 工作 | 取得并核 official package/source/license bytes；形成唯一两行 patch；解析并冻结 runner build、official upstream tests、005/harness 和 LOC-A harness 各自需要的完整 package closure 与 process DAG |
| 输出 | `material-manifest`、official source tree manifest、patch SHA、exact nupkg/entry manifests、runner 与 test closures/locks、finite expected package→expected deployed path/entry→license mapping、所有后续命令/child-process/time/storage caps |
| 禁止 | candidate runner build/publish、生成或消费 candidate DLL、运行 Luban/005/LOC-A、修改八个生产路径；仅当 M0 packet 预先列明并计数时，可做不产出 runner 的 closure-only restore/probe |
| gate | 任一 package、test dependency、license source/commit/bytes/hash、harness 子进程或预算未知时 `BLOCKED_MATERIALS`；不得签 R1 |

M0 输出不得包含伪造的 candidate `Luban.dll` SHA、runner count/manifest 或生产验证结果；
这些只能是 R1/L1 的后续输出。反过来，M0 签包也不得把“未来构建成功后再知道的值”列为
自己的必需输入。M0 必须在无需 candidate build artifact 的条件下闭合：

1. official Luban source tree 和两个 csproj preimage；
2. 唯一两行 PackageReference patch 的 exact bytes/SHA；
3. candidate runner restore closure 与 lock；
4. `src/Luban.Tests/Luban.Tests.csproj` 的独立 package closure、test adapter、fixtures、
   restore/build/test semantics；
5. 005 与 LOC-A harness 的真实 top-level commands、它们可能启动的 `dotnet`／MSBuild／
   VSTest／Luban child processes，以及每层 timeout/kill/log cap；
6. 第 6.2 节由 exact lock/nupkg entries、source/build rules 与 finite expected deployment
   list 冻结的完整 expected package/path/entry/license mapping；不读取未来 publish output
   或 runner manifest。

所谓“一次 test invocation”只能描述 top-level command，不能冒充进程预算。M0 必须给出
每条命令预期的完整 process tree 和数值上限；工具行为无法在不执行 build 的条件下从
固定材料确定时，M0 明确 `BLOCKED_PROCESS_MODEL`，由主程另签只读 closure/process
probe，不能把未知量塞进 R1。

### 7.2 R1 — 外置 patched-runner 构建与行为证明

R1 只消费已接收 M0 的只读输出以及旧 runner oracle。它不得联网、更新 lock、补 license
或修改 FightMatch 生产路径。可复现性由**恰好两个**相互独立的空根 pass 证明：

```text
runner-proof/<run-id>-pass-1/{source,feed,nuget,obj,bin,runner,evidence}
runner-proof/<run-id>-pass-2/{source,feed,nuget,obj,bin,runner,evidence}
```

两个 pass 不共享 `source`、`nuget`、`obj`、`bin`、publish output 或环境派生文件，只
共享 M0 的只读 source/package/license/lock bytes。每个 pass 的 runner build 语义固定为：

- top-level offline `restore` 恰好 1 次；
- top-level Release `publish --no-restore` 恰好 1 次；该 publish 是本 pass 唯一产生
  runner compilation output 的操作；
- standalone `dotnet build` 恰好 0 次；不得把 publish 内部 compilation 再记成一条
  top-level build，也不得隐藏额外 build command；
- 两 pass 合计 2 restore＋2 publish-with-build，禁止第三 pass 选结果。

这与当前 SYS 的 restore→publish 形态一致。official tests 在独立 test root 中恰好执行
1 次 restore＋1 次 build＋1 次 test；该 build 属于预先冻结的 test process DAG，必须有
独立计数和 closure。它不是第三个 runner pass，也不得被“1 official test invocation”
掩盖。parity harness／DUT 的额外 exact 顶层命令数和全部子进程数由 M0 冻结。
R1 只能执行 M0 已列明的 exact DAG；任何隐式 restore/build、额外 child process 或新
package 都是 `BLOCKED_PROCESS_DRIFT`。

R1 的 positive controls：

1. 两个 source tree、patch、lock 和 environment receipts 一致；两个完整 runner
   manifest、关键 DLL/PDB（若部署）及 deterministic metadata 一致。
2. new graph 中旧 `YamlDotNet.NetCore` 为零，unexpected package 为零；分别枚举两个
   publish outputs，每个实际 deployed-file 集合都与 M0 finite expected mapping 集合等值，
   且每个 actual file exactly-once 映射到 expected package/path/entry 和 license；任何
   missing、unexpected 或 duplicate 均失败，不得反补 M0。
3. 按 M0 冻结的 official upstream test DAG 运行，真实 baseline count、failed/skipped
   policy 和全部子进程据实记录；未知或新增依赖立即阻塞。
4. old oracle 与两个 candidate runner 对相同 YAML loader、YAML target、UnityAsset
   fixture 做 success/failure、diagnostic category、semantic tree 和输出 parity。
5. 重做 005 的 2-row scratch positive/negative matrix 到新 evidence；旧 005 只读。
6. R1 只产出 derivative runner、完整 identity/SBOM/license closure 和行为 receipt；
   不生成或预填 L1 的 production verdict。

R1 技术证明全部通过后，必须先 create-once 复制 `CANDIDATE_NOT_ACTIVE` final cache 并对
final bytes 重算 manifest；独立 R 同时审查 temp proof、final bytes 与该 manifest。只有
R `ACCEPT` 才形成 `B-ACCEPTED` 并可签 L1；R 前 final cache 不得成为 active runner，
不得被 L1 消费。

### 7.3 L1 — LOC-A `Program.cs` 适配与生产语义验证

L1 只消费已接收的 M0 materials 和 R1 runner identity/final bytes。L1 不得 restore、build
或 publish Luban runner，不得改变 candidate package graph，也不得回写 M0/R1 evidence。
它的唯一预期 production source delta 仍是 `Program.cs` 的 runner/package/license
identity gate；其余七个生产路径 final bytes 不变。

1. 先把 `Program.cs` 对齐 R1 的 runner manifest、critical files、完整 license manifest
   和禁止旧 DLL 的 gate；不得保留 `.nuspec` license fallback。
2. project compiler 自身的 build/harness command、child-process tree 和 timeout 必须已由
   M0 冻结；L1 不得现场发现后扩大命令或预算。
3. 对固定 formal source 做两个独立空 staging generation pass，不允许 third pass。
4. 两次 raw relative path/file set/bytes/SHA 全相等，并与 old accepted runner frozen raw
   逐文件相等；任何 presentation drift 都先 `NO_GO_RAW_DRIFT`。
5. canonical JSON、manifest、semantic digest、264 records、locale/severity/parameter
   summaries、12 个既有 self-tests 全部保持原合同。
6. 两个 repository Generated 最终 SHA 仍须为第 1.2 节值；否则零 production publish。

### 7.4 DUT 与 harness 的负例语义

负例必须把被测进程（DUT）与观察它的验证 harness 分开记录：

- **DUT**：对非法输入执行实际 preflight/compiler/Luban contract 的进程。正确负例要求
  DUT 非零退出、出现指定诊断（若合同指定）且无 canonical/final publish。
- **harness**：外层执行器，负责确认 DUT 的 exit、diagnostic、空 output 和 process
  quiescence。harness 正确观察到预期失败时**应返回 0**；DUT 意外返回 0、诊断错误、
  留下发布物、timeout/crash 或进程未静默时，harness 必须非零。

三个既有负例 `negative-duplicate`／`negative-parameters`／`negative-severity` 因此都必须
保存两层 receipt：内层 DUT 分别非零并命中 `FMLOC002`／`FMLOC007`／`FMLOC004`、无
发布；外层 harness 在全部断言正确时返回 0。不得把 harness=0 误报为“非法输入成功”，
也不得要求正确拦截时 harness 非零。

| 负对照 | DUT 必须结果 | harness 必须结果 |
| --- | --- | --- |
| old `YamlDotNet.NetCore` package/DLL 出现在 graph/runner | pre-build/preflight 非零；不启动 production generation | 正确拦截为 0 |
| 任一 nupkg/repository commit/deployed entry/license hash mismatch | provenance DUT 非零；不得 restore/build/publish | 正确拦截为 0 |
| 第 6.2 节任一 closure license 缺失 | `LICENSE_INVENTORY_INCOMPLETE`，非零且无发布 | 正确拦截为 0 |
| 两 csproj 之外有 source diff | `NO_GO_PATCH_EXPANDED`，非零 | 正确拦截为 0 |
| restore/test/harness 出现未冻结 package 或 process | `NO_GO_UNEXPECTED_GRAPH`／`BLOCKED_PROCESS_DRIFT` | 正确拦截为 0 |
| official tests、YAML parity、005 parity 任一不同 | `NO_GO_BEHAVIOR_DRIFT` | 正确报告失败；不可伪装为通过 |
| raw/canonical/manifest/semantic 任一不同 | `NO_GO_OUTPUT_DRIFT`；零 production publish | 正确报告失败；不可伪装为通过 |
| derivative 冒充 official runner 或复用旧 manifest SHA | identity DUT 非零 | 正确拦截为 0 |
| 尝试 `17.1.0`、`18.1.0`、4.x 或第二版本 | scope DUT 在执行前非零 | 正确拦截为 0 |

### 7.5 单向消费与失败封闭

M0、R1、L1 任一失败都停止后续阶段，并保留本阶段所有已经 create-once 的外置制品和
evidence：M0 material root 标记 `REJECTED_NOT_ACCEPTED`，R1 final cache 标记
`REJECTED_NOT_ACTIVE`，各 stage evidence 标记 `FAILED_EVIDENCE`；B temp roots 同样原地
保留。不得删除、隔离、覆盖或补写这些 roots。L1 发现 runner/closure 问题只能返回
R1/M0 owner 新签 revision，并使用新的 sibling root；R1 发现材料缺口同理只能要求新的
M0 revision 和新 sibling root。任何后续产物都不能被补写为前阶段“原始输入”，从而
避免循环 authority。

## 8. 资源与时间预算

未来三个阶段仍需分别授权。当前架构固定可计数的上限，并要求 M0 在 R1/L1 签包前把
其余数值全部填实：

- candidate version：恰好 1 个，`YamlDotNet 16.3.0`；禁止多版本试跑；
- external source delta：恰好 2 个 csproj、各 1 个 PackageReference replacement；
- FightMatch production source delta：最多 1 个现有 `Program.cs`；其余 7 路径 final
  bytes 不变；
- M0 runner build/publish budget：`0`。package acquisition、closure resolution 和只读
  process probe 各自的 top-level command 数、完整 child-process 数、network request、
  timeout 和 storage 必须在 M0 packet 逐项列明；任一未知即 `BLOCKED_MATERIALS`；
- R1 runner build budget：恰好 2 个独立空根；每 pass 1 restore＋1 publish-with-build＋
  0 standalone build，总 top-level runner commands=4；无第三 pass；
- R1 official tests：恰好 1 restore＋1 build＋1 test；test package 和完整 process tree
  必须取 M0 的 frozen DAG。YAML／005 等 parity 的额外 exact 顶层命令数也必须由 M0
  冻结；top-level invocation 数不得冒充 OS process 总数；
- L1 runner restore/build/publish budget：`0`；production generation 恰好 2 个空 staging
  pass。compiler build、自检和三负例的命令／子进程预算必须取 M0 frozen DAG；
- 任何时刻最多运行 1 个 top-level process tree，不得并行启动第二个 top-level tree；
  该树内部 child processes 不受“只能 1 个”的误读约束，但每种 executable identity、
  exact 最大同时数和启动总数必须由 M0 accepted DAG 逐项冻结并全部记录；R1/L1
  network request 上限为 0；
- 与 SYS 一致的 hard ceilings：M0 wall clock `≤60` 分钟；R1 wall clock `≤120` 分钟总计，
  由 pass-1、pass-2、official tests 与 parity 共用，不另加每-pass额度；单
  restore/publish/build/test timeout `≤10` 分钟，单 Luban/control DUT timeout `≤5`
  分钟；R1 temp 总空间 `≤2 GiB`，其中 pass-1 `≤768 MiB`、pass-2 `≤768 MiB`、
  official-tests+controls `≤512 MiB`；L1 temp 总空间 `≤1 GiB`；new material root
  `≤512 MiB`；new final runner `≤128 MiB`；每个 new evidence root `≤128 MiB`；
- L1 wall clock 和 parity 额外 command counts 仍须分别在对应的 accepted material／packet
  中填入 exact 数值；任一未知即保持 `PREPARED_NOT_AUTHORIZED`；
- M0 必须在 material receipt 中给出高于零且可审的 `maxTopLevelCommandCount`、固定为
  `1` 的 `maxTopLevelProcessTreeCount`、每种 child executable identity 的 exact 最大同时数
  与启动总数、逐阶段 log cap 与 free-space floor。未填这些数值时保持
  `PREPARED_NOT_AUTHORIZED`，不得靠上述 wall/storage ceilings 推测进程预算；
- Unity、Android、Windows、LOC-B、Host、RES、device 和 release budget 均为 `0`。

如果 official test baseline 在 exact unpatched source／SDK 上失败，test closure 无法在
M0 冻结，或实际 harness 产生未声明的 restore/build/child process，保存 evidence 并
停止；不得用跳测试、更新 SDK、把 child process 藏进一次 invocation 或放大预算吸收。

## 9. 回滚与历史保全

1. accepted `ApplicationData/Tools/Luban/5.1.0/`、132-file runner、005 和三个 LOC-A
   历史 roots 从始至终只读。
2. M0 material root、B temp roots、R1 final cache 和各 stage evidence 一旦 create-once，
   无论技术失败或独立 `REJECT` 都必须原地保留：material root 标记
   `REJECTED_NOT_ACCEPTED`，final cache 标记 `REJECTED_NOT_ACTIVE`，stage evidence 标记
   `FAILED_EVIDENCE`。不得删除、隔离、覆盖或补写；新 revision 只能使用新 sibling root。
3. 若已对 `Program.cs` 形成未接收 delta，只能由获准 packet 恢复 dispatch 前 exact
   preimage；两个 Generated 文件恢复当前 exact bytes。该恢复不授权删除任何外置制品或
   evidence，且只有证明没有 LOC-B／RES 等后续依赖时才能执行。
4. 不得清 NuGet 全局 cache、PackageCache、旧 runner 或用户文件。
5. candidate 接受后也不删除历史 runner；旧 runner 保留为 provenance／对照，但不得
   再作为生产 generation authority。
6. independent `REJECT` 与技术失败使用同一保留／回滚规则，不通过改 inventory、忽略文件或
   降低输出比较标准“修复”。

## 10. 单一路线 go／no-go 门

### 10.1 GO 的必要且联合条件

后阶段的 GO 由前阶段 verdict 和本阶段 evidence 联合决定，不存在一次大包跨过三门。

**M0 → R1：**

- [ ] exact official Luban base source/commit 和 SDK `8.0.425` 匹配；
- [ ] patch 只有两个 PackageReference replacements，零 `.cs` source change；
- [ ] `YamlDotNet 16.3.0` nupkg、nuspec exact repository commit、package signature、
  net8 DLL、`LICENSE.txt`、`LICENSE-libyaml` 全部按第 3.2 节闭合；
- [ ] runner build closure、official test closure、005/LOC-A harness process DAG 和数值
  budget 全部在无 candidate build output 条件下冻结；
- [ ] 第 6.2 节 finite expected package→expected deployed path/entry→license mapping 完整；
  六个已知无包内文本包及所有其它 expected packages/assemblies 都有 exact upstream
  source/commit/license bytes/hash，且未读取未来 publish output／runner manifest；
- [ ] M0 的独立 R 对 stage `M0` 给出唯一合法 verdict `ACCEPT`，否则 R1 不得启动。

**R1 → L1：**

- [ ] 恰好两个独立空根，各 1 restore＋1 publish-with-build＋0 standalone build；
- [ ] 两个 runner manifests/critical files/deterministic receipts 相等；graph 没有旧包、
  unexpected transitive 或 process drift；
- [ ] official tests、targeted YAML、UnityAsset、005 正／负对照全过；每个 negative 都有
  DUT 非零／无发布与 harness 正确观察后 0 的双层 receipt；
- [ ] 新 runner manifest、SBOM 和 full-closure license inventory 完整，旧 132 identity
  未复用；
- [ ] R1 技术证明全过后，final cache 已 create-once 标记 `CANDIDATE_NOT_ACTIVE` 并对
  final bytes 重算 manifest；独立 R 同时审查 temp proof、final bytes 和 manifest；
- [ ] R1 的独立 R 对 stage `R1` 给出唯一合法 verdict `ACCEPT`，否则 L1 不得修改
  `Program.cs`。

**L1 final：**

- [ ] `Program.cs` 只做 accepted runner/package/full-license identity gate delta；
- [ ] FightMatch 两个空 staging pass deterministic，raw、canonical、manifest、semantic、
  diagnostics 和 Generated bytes 全部与 accepted runner 等价；
- [ ] 三个既有 negatives 的 DUT/harness 双层 exit 语义符合第 7.4 节；
- [ ] repository final diff 最多一个 `Program.cs` identity/provenance delta，零产品语义
  delta；
- [ ] 最终独立 R 只给一个 `ACCEPT`。

通过只意味着：**patched制作期 runner 可成为 LOC-A 新候选 input**。它不自动接收
FIX-03，不授权 LOC-B、Host、RES、Unity、Android、Windows 或 release。

### 10.2 任一即 NO-GO

- official Luban 发布新的可追溯 exact runner 之前，A 路线仍不可假称可用；
- patch 扩到 Luban C# source、第三个 project/package 或 FightMatch runtime；
- source/package/license/signature/graph 任一无法 exact binding，或第 6.2 节任一 deployed
  package/assembly 缺 upstream commit-bound license bytes；
- M0 依赖未来 runner SHA/build output 才能签包，R1 反向补写 M0，或 L1 反向改变
  runner/closure；
- 两个 R1 空根共享 `nuget`/`obj`/`bin`，pass 数不等于 2，或 restore/publish/build/
  official-test/harness 子进程没有按 M0 process DAG 计数；
- 并行启动第二个 top-level process tree，或树内 child 的 executable identity、最大同时数
  或启动总数超出 M0 accepted DAG；
- compile/test/YAML/005/FightMatch output 任一 drift；
- 需要换 `17.1.0`／`18.1.0`、回退 4.x、安装旧 SDK、联系 publisher 或通过法律推断补 provenance；
- 需要修改 8 路径中 `Program.cs` 之外的 source/config，或改变两个 Generated bytes；
- 超出冻结 process/time/storage budget。

上述任一情况必须返回 `NO_GO`／`BLOCKED` 和原始 evidence，不得把“MIT 很可能正确”、
namespace 可编译或 CSV 当前不用 YAML 当成 acceptance。

## 11. 后续实施包必须绑定、本文不伪造的字段

- **每阶段共同：**实际 owner thread/host/turn、主程 authorization turn、本文 accepted
  SHA/lines/bytes、上阶段 verdict、create-once root/run-id、process-safety/rollback receipt。
- **M0：**official source tree、两个 csproj preimage/patch SHA、所有 exact nupkg/entry、
  signature、runner/test closures 与 locks；由 exact lock/nupkg entries、source/build
  rules 与 finite expected deployment list 冻结的 package→expected path/entry→license
  mapping（含第 6.2 节六个已知 gap）；top-level command DAG、所有 child processes、
  build semantics、timeout/log/wall/storage/free-space 数值预算。不得要求或读取 future
  publish output、runner manifest 或 candidate runner SHA。
- **R1：**M0 material manifest SHA；两个独立空根；每 pass 的 1 restore＋1 publish-with-
  build receipts；official test/YAML/UnityAsset/005 的真实 baseline 与 DUT/harness receipts；
  两个 publish outputs 对 M0 expected mapping 的集合等值/exactly-once receipts；两份 runner
  manifests、critical DLLs、SBOM/full license inventory；`CANDIDATE_NOT_ACTIVE` final
  cache create-once 与 final-byte manifest receipt；不得要求 L1 verdict。
- **L1：**accepted R1 runner file count/manifest/critical SHA；当前八个 production paths 的
  before SHA/bytes；`Program.cs` exact diff；两 generation pass、12 self-tests、三既有
  negative 的 DUT/harness 双层 receipts、raw/canonical/manifest/semantic comparisons；
  final repo diff 与 rollback authority。

任一字段未绑定都保持 `PREPARED_NOT_AUTHORIZED`，不得让执行者猜路径、count、SHA、
命令或放宽比较。

## 12. 研究自检与最终推荐

- 唯一仓库写入：本文。
- 本轮未改 8 个 production paths、外盘 source/cache/runner、packages、Assets、
  ProjectSettings、`.meta`、旧 evidence 或 Git。
- 本轮未运行 Unity、Luban、005、restore、build、publish、test 或生成。
- 路线锁定 ARCH B1 = SYS Candidate A；ARCH A 已有一手 NO-GO，B2/B3/C1 仅保留为排除记录，不得在后续阶段切换、
  试跑或回退。
- 推荐没有引入新的 FightMatch interface 或无必要抽象；替换仅位于 tool dependency
  seam，runner 的 caller interface、项目 artifact 和 runtime architecture 不变。
- patched runner 被明确标识为 FightMatch derivative，绝不冒充 official pin。

**最终推荐：只为 ARCH B1 = SYS Candidate A 依次另签 M0/SYS M 材料／闭包冻结、R1/SYS B 外置 runner 双空根证明、L1/SYS L
`Program.cs` 适配与生产语义验证三个包；当前不实施。每一后阶段只消费前阶段已接收
输出。只有三门全部满足并获最终独立 `ACCEPT`，才允许主程重签 LOC-A；否则维持
`BLOCKED_LICENSE_INPUT`，不试第二版本。**
