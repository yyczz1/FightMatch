# LOC-LIC-ALT-SYS-01 — Luban runner 许可证替代系统设计

状态：`PREPARED_NOT_AUTHORIZED`

日期：2026-09-30（研究与落盘日期：2026-10-01，Asia/Shanghai）

所有者：主程／系统设计；未来实施必须另行签发新的单一实现包。

## 0. 结论与授权边界

当前 Luban runner 不能通过发布许可证门禁：固定的官方 Luban v5.1.0 源提交仍直接引用 `YamlDotNet.NetCore 1.0.0`，而该旧 NuGet 包自身没有可闭环的许可证、项目或仓库元数据。它还带入一组老旧 `System.*` 传递依赖。问题来自 Luban 的 YAML／Unity Asset 可选能力，并非 FightMatch 的 CSV 输入或 JSON 输出主路径。

当前唯一允许继续设计与未来证明的路线是 **候选 A**：在 exact Luban v5.1.0
source copy 中仅把两处 `YamlDotNet.NetCore 1.0.0` 替换为官方
`YamlDotNet 16.3.0`。两个包保持相同的 `YamlDotNet` 命名空间；上游 16.3.0
明确支持 `net8.0`，NuGet 记录该目标无依赖并采用 MIT。此方案保留 YAML 能力，
补丁面最小。

候选 B（裁掉 YAML／Unity Asset 能力）与候选 C（复现旧 DLL）只保留为已审查的
排除记录，不是本轮 fallback、预算项或可签发实现路线。候选 A 失败时必须停单，
不得自动或人工在同一 authority 下切换 B/C。

任何候选 A 产物都必须叫 **patched-runner**，不得叫 “official pin” 或暗示 Luban
官方发布包含该修改。官方固定项仅是 Luban `v5.1.0` 源码提交；依赖替换是
FightMatch 自有补丁。

本文只设计，不授权以下动作：下载包或二进制、修改现有 Luban 缓存、构建／运行 Luban、运行 Unity、运行 `005`、运行 Git、修改现有证据、写入最终工具目录、修改八个生产路径、联系 QA 或 R。

## 1. 固定输入与研究基线

本设计以以下本地文件的当前字节为固定输入；未来签包前任一值变化都应使本文失效并重新审查：

| 输入 | SHA-256 | 行／字节 |
|---|---|---:|
| `docs/team/2026-09-30/engineering-localization-luban-001.md` | `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a` | 777 行／52,323 B |
| `docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-02.md` | `3d437c485903ab25569f9f417f49fd1653125428a4f3913843d15b7c35a62a35` | 112 行／5,703 B |
| `docs/team/2026-09-30/engineering-localization-luban-impl-a-fix-03.md` | `374234b8ed0f015af13eeff3771c15343a2d497e7fc54909152dad207322a812` | 319 行／20,411 B |
| `docs/team/2026-09-30/research-loc-a-license-sources.md` | `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550` | 170 行／11,681 B |
| `Tools/FightMatch.Localization.Compiler/Program.cs` | `f16e559b90dee2a423cdff1d453a21d55e241633a153a9f812e54cb9cbf05284` | 1,583 行／80,452 B |
| `Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj` | `37ab5325dc95ee3f9538cdcf70c56caceb250556448293e720c292ea9fafec70` | 294 B |
| `Tools/FightMatch.Localization.Compiler/global.json` | `0a15550b7b496f0eb8ea6e405da955cf264bf519fb0fb60dd8a7e25967bbca5d` | 104 B |

本次 `SYS-FIX-01` 还固定以下设计／评审 authority：

- 架构：`engineering-localization-license-alternative-architecture.md`，SHA-256
  `01c47276258569f3617b613f0ed72fe9c71043c633dab084f097e999dbd7e936`。
- 本文修订前字节：SHA-256
  `b43c17075c8012323b16673810124b9bdd0ca6ea52a6e0d1e38047d7f48d9257`。
- 独立设计 R：turn `01a0f467-89a8-75b1-808c-de1d25aaa3b8`，final message
  `msg_0d29bf8d726d8dc0016abd8d6b85d487d094d577ce867f83c9`，verdict
  `NEEDS_FIX`；本修订只处理其四项最小修正，完成后仍需主程复核和同一 LOC-A
  独立设计 R 的新 verdict。

工具基线：

- Luban 官方源：tag `v5.1.0`，full commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`。
- .NET SDK：`8.0.425`。
- 当前 runner manifest：132 files，SHA-256 `d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5`。
- 当前 `Luban.dll`：SHA-256 `6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e`。
- 当前 packages lock：SHA-256 `3231b50eccd462dd529fb3d0e67c44d62ca8c3363af8367e086ed6b5f5081194`，14,103 B。
- 当前只读缓存根：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/`。

当前八个生产路径继续由原设计固定：

| # | 路径 | 当前 SHA-256 |
|---:|---|---|
| 1 | `Config/FightMatch/Luban/luban.conf` | `03ed736de1debd33a1d6c1ebbf30149c2876eb0458f0da34c4a819bee794dc41` |
| 2 | `Config/FightMatch/Localization/__tables__.csv` | `5312d8f5367feda5a41370b5441b2e46d861e902faef5b1bb6b1b4c3795e6c35` |
| 3 | `Config/FightMatch/Localization/FightMatchText.csv` | `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030` |
| 4 | compiler `.csproj` | `37ab5325dc95ee3f9538cdcf70c56caceb250556448293e720c292ea9fafec70` |
| 5 | compiler `Program.cs` | `f16e559b90dee2a423cdff1d453a21d55e241633a153a9f812e54cb9cbf05284` |
| 6 | compiler `global.json` | `0a15550b7b496f0eb8ea6e405da955cf264bf519fb0fb60dd8a7e25967bbca5d` |
| 7 | `Generated/FightMatch/Localization/fm-text-v1.json` | `f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc` |
| 8 | `Generated/FightMatch/Localization/fm-text-v1.manifest.json` | `18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667` |

候选 A 的未来 L 实现只应允许修改第 5 项以接受新的 runner
identity／manifest／license closure；第 7、8 项只有在新 runner 输出与现有字节完全
相同时才允许以相同字节重拷贝。其余五项不得变化。若输出字节不相同，停止并回报，
不得把差异解释成“等价”后发布。

## 2. 一手来源

- Luban 官方 [v5.1.0 release](https://github.com/focus-creative-games/luban/releases/tag/v5.1.0) 固定本设计使用的上游版本；官方当前 [`src/Luban/Luban.csproj`](https://github.com/focus-creative-games/luban/blob/main/src/Luban/Luban.csproj) 仍可见 `YamlDotNet.NetCore 1.0.0`。因此不存在可直接替换本地 runner 的“未修改官方 v5.1.0 clean build”。
- YamlDotNet 官方 [v16.3.0 release](https://github.com/aaubry/YamlDotNet/releases/tag/v16.3.0) 是 GitHub verified release，tag 指向 full commit `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73`。
- 官方 [NuGet YamlDotNet 16.3.0](https://www.nuget.org/packages/YamlDotNet/16.3.0) 记录 owner `aaubry`、MIT、`net8.0` compatible，并显示 `net8.0` 直接包无依赖。该页面也是未来签包核对官方 package identity 的入口。
- exact tag 的 [`YamlDotNet.csproj`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/YamlDotNet/YamlDotNet.csproj) 明确目标包含 `net8.0`。
- exact tag 的 [`LICENSE.txt`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/LICENSE.txt) 与 [`LICENSE-libyaml`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/LICENSE-libyaml) 提供需要随 runner 归档的上游许可证文本。
- exact tag 的 [`YamlStream`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/YamlDotNet/RepresentationModel/YamlStream.cs) 仍位于 `YamlDotNet.RepresentationModel` 并暴露现有 Luban 所调用的 `Load(TextReader)`／documents／save 能力。这支撑“先做仅包替换”的兼容性假设，但不能替代实际构建和专门 YAML 正负测试。
- exact tag 的 [`YamlNode`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/YamlDotNet/RepresentationModel/YamlNode.cs) 保留 string→`YamlNode` implicit、`YamlNode`→`string?` explicit 和 indexer；[`ScalarStyle`](https://github.com/aaubry/YamlDotNet/blob/v16.3.0/YamlDotNet/Core/ScalarStyle.cs) 路径也仍存在。静态接口形状因此支持“高概率源兼容”，但本文没有编译；只有固定构建和专项 fixture 能确认。

网络页面不是最终制品证据。未来获准实施时必须把 exact nupkg、NuGet 官方 catalog identity、两份许可证原始字节及各自 SHA-256 固定到新的 identity/证据根；当前研究没有下载它们，也不伪造 nupkg SHA-256、大小或 license 文件字节值。

## 3. 当前包图与 `YamlDotNet.NetCore` 出现原因

当前 exact source 的直接边为：

```text
Luban.csproj
├── CommandLineParser 2.9.1
├── YamlDotNet.NetCore 1.0.0        ← direct, legacy
├── Luban.DataLoader.Builtin
└── Luban.DataTarget.Builtin

Luban.DataLoader.Builtin.csproj
├── ExcelDataReader
├── NeoLua
├── Ude.NetStandard
└── YamlDotNet.NetCore 1.0.0        ← direct, legacy

Luban.DataTarget.Builtin.csproj
└── Luban.DataLoader.Builtin
```

源代码用途与注册点：

- `YamlDataLoader` 注册 `DataLoader("yml")`。
- `UnityAssetDataSource`／`UnityAssetDataCreator` 解析 Unity YAML asset。
- `YamlDataTarget`／`YamlDataVisitor` 注册并实现 `DataTarget("yaml")`。
- `PipelineScope` 加载 `Luban*.dll` 并通过 `RegisterBehaviour` 特性扫描注册实现。
- `CsvDataLoader` 与 `JsonDataTarget` 是独立实现；它们没有调用以上 YAML 文件。

FightMatch 当前 `luban.conf` 只把 `../Localization/__tables__.csv` 作为 schema，`__tables__.csv` 只指向 `FightMatchText.csv`；已批准命令使用 `-d json`。因此旧 YAML 包是在完整 builtin assembly 中随可选能力被部署，不是产品 CSV→JSON 数据流的必要依赖。

当前部署还包含：

| 文件 | 字节 | SHA-256 |
|---|---:|---|
| `YamlDotNet.NetCore.dll` | 172,032 | `c7b7e80ce82038ed32d58a736b17bd6323571fbde0ad34fcac097cb579cfc94a` |
| `Luban.DataLoader.Builtin.dll` | 82,944 | `be51108b5437c924149f59b165ec8c8f1b483811d5e505782de2a0d232553279` |
| `Luban.DataTarget.Builtin.dll` | 25,600 | `e30784329e44bfc7d753d48acd51535755c1dcb79829b79a7b2f38cc0164e481` |
| `Luban.deps.json` | 33,976 | `00e7ac6abc99de3b75d1ead583b7291719169f193448f6db991c895e5d457f2d` |
| `Luban.dll` | 20,992 | `6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e` |
| `Neo.Lua.dll` | 321,024 | `acfbc880041d2022c237c32fad7d0d66ee7b2ac49fda71f5bc65c78882dbc335` |
| `de/Neo.Lua.resources.dll` | 10,752 | `73b4e75980a944cbd0d96f5e6adfbd000aab781da0ac7500d0f612dae06f5a99` |

旧 `YamlDotNet.NetCore 1.0.0` nupkg 本地 SHA-256 是 `c88b1a0c04e38dc813c373f9775a12370f6921abdd639ee8a1e9da56b0a3ae97`，nuspec SHA-256 是 `871245d3394e3c4595ab1e6d89b151017a6fe75f1d82ec5d6ba657bb13e5d156`；nuspec 没有 license、project URL 或 repository metadata。包名相似或代码历史推断不能替代许可证证据，所以 FIX-03 的 Yaml 项无法签名闭环。

## 4. 模块边界与实现缝

唯一需要保留的产品侧接口是现有 runner 调用缝：

```text
Excel/CSV authority
  → absolute --luban-dll path + frozen runner identity
  → dotnet <runner>/Luban.dll ... -d json
  → staged JSON
  → existing compiler validation/publication
  → byte-identical generated JSON + manifest
```

变化必须留在 runner implementation 之后：

- **接口不变**：输入表、Luban CLI 参数、`-d json`、staging、compiler validation、publication 和运行时 JSON schema 不变。
- **实现可换**：当前 accepted source-built unpatched runner（不是 Luban 官方 release binary）仅作 oracle；新 patched runner 由一个冻结 identity 描述。
- **不要新增公共抽象**：`Program.cs` 内部把现有硬编码 runner file count/hash/license map 改成一个私有、不可变的候选 identity record 即可；不要为单一实现增加公共接口、配置文件或运行时选项。
- **失败封闭**：identity、许可证、输出或负向测试任一不匹配时，在 publication 前退出非零。

这是一条深模块边界：调用方只知道“受信 runner identity + CLI”，内部依赖替换、source patch、package graph 与许可证归档都封装在 patched-runner 制品中。

### 4.1 M／B／L 三阶段接收物

为避免“先要 runner hash 才能签材料、又先要签材料才能构建 runner”的循环，后续只能
按三个独立阶段推进。每阶段有自己的 owner、create-once 根、输入 SHA、输出 receipt 和
独立接收；后阶段只能消费前阶段已接收的只读字节。

| 阶段 | 唯一职责 | 必须接收的输出 | 本阶段明确不需要／不得做 |
|---|---|---|---|
| **M — material／closure freeze** | 冻结 official Luban source、两处 csproj patch、official `Luban.Tests.csproj` preimage、五个 direct test package seeds、runner/test locks、全部 nupkg/source/license bytes、测试 harness 命令及 exact process/temp budget | `M-ACCEPTED` receipt；逐项 source/package/license/test manifest；两个 lock；exact patch；B 命令与预算 | 不需要 runner file count、runner manifest 或 `Luban.dll` hash；不构建 runner；不改 `Program.cs` |
| **B — 外置 build proof** | 只读消费 M；在两个独立空根各做一次 restore+publish；完成 official tests、YAML／UnityAsset／005／FightMatch parity、SBOM 与 license inventory | `B-ACCEPTED` receipt；runner file count、runner manifest SHA、`Luban.dll`／`YamlDotNet.dll` hashes、两 pass 比较、测试与 parity 证据 | 不补材料、不改 lock／patch／预算；不改仓库生产文件或运行 LOC-A publication |
| **L — LOC-A adapter** | 只读消费已接收 B 身份，只适配 `Program.cs` 的 private runner/package/license identity，并执行 LOC-A 两 clean pass、12 self-tests 与负向门 | `L` 新 evidence root、精确 repo diff、最终验收回执 | 不重新 restore／build runner，不改变 B identity，不修改另七个生产输入语义或字节 |

接收依赖严格为 `M-ACCEPTED → B-ACCEPTED → L`。M receipt 不得含伪造或预估的 B
hash；B 发现 M 缺项时返回 `BLOCKED_MATERIAL_FREEZE`，不得现场下载或补清单；L 在
没有 B 独立接收时返回 `BLOCKED_RUNNER_IDENTITY`。M/B/L 不能共用 evidence root，
也不能用后一阶段结果回写前一阶段 receipt。

## 5. 唯一候选与排除记录

### 5.1 候选 A（推荐）— `YamlDotNet 16.3.0` 两点替换

正式身份建议：

- 人类名：`Luban v5.1.0 df34215 FightMatch patched-runner / YamlDotNet 16.3.0 / r1`
- 目录名：`5.1.0-fm-yamldotnet-16.3.0-r1`
- 禁止名：`official Luban 5.1.0 pin`、`official runner`、`unmodified Luban`。

在 exact source 临时副本中只做两行替换：

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

应用前必须逐文件核对：

| source project | bytes | SHA-256 |
|---|---:|---|
| `src/Luban/Luban.csproj` | 4,201 | `5a29bc8a18a2eae222d412ac4f2485ae366ffd7146cc2c1bf397e5554dde30c9` |
| `src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj` | 1,811 | `0f84ba758efa3ec4b8f878abf3b0429f34cc23bbf9a01f81c164878658839516` |
| `src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj` | 1,669 | `935771a2b429ec8c0490cf88f022d8c31d5d2af4a73ad76fe627fe5f814a2dc8` |

候选 A 只能改变前两项，每项恰好一处 PackageReference；第三项和全部 `.cs` 必须保持上述／source manifest 固定值。

不得改任何 `.cs`、target framework、语言版本、Luban 参数或其他 package version。特别不得用浮动版本、自动升级到 latest、兼容 shim 或 binding redirect 扩大补丁。

预期价值：

- 包名仍提供 Luban 使用的 `YamlDotNet.*` 命名空间；无需 source API patch。
- exact 16.3.0 原生包含 `net8.0` 目标，当前 SDK 与 runner 目标吻合。
- 直接包在 `net8.0` 下没有传递依赖，从 runner 图中同时移除旧包带来的历史 `System.*` 依赖链。
- YAML／Unity Asset loader 与 YAML target 仍被编译、扫描和注册。

风险与门禁：

- 命名空间相同不保证所有运行时行为相同，必须通过 build、CSV→JSON byte oracle、YAML 专门正负对照。
- 新 package 的 exact bytes 尚未在本任务下载／固定；在 package material 被另行批准、核验和签入新制品证据前，候选 A 仍是设计，不是可执行选择。
- 若编译失败或 YAML 对照失败，不得追加任意 source compatibility edits；先停单回报，再决定是否批准一个新的、精确增量设计或降级到 B。

### 5.2 候选 B（已排除，不得签发）— 移除 YAML／Unity Asset 可选能力

本节仅保留早期设计取舍的审计记录。主程已限定本轮唯一候选为 A；以下补丁、身份和
测试描述都不构成 fallback authority，A 失败也不得在本单启用 B。

正式身份建议：`5.1.0-fm-no-yaml-r1`。

精确项目补丁：

1. 从 `src/Luban/Luban.csproj` 删除 `YamlDotNet.NetCore 1.0.0` PackageReference。
2. 从 `src/Luban.DataLoader.Builtin/Luban.DataLoader.Builtin.csproj` 删除同一 PackageReference，并添加明确的 `Compile Remove`，排除：
   - `DataVisitors/YamlDataCreator.cs`
   - `DataVisitors/UnityAssetDataCreator.cs`
   - `UnityAsset/UnityAssetDataSource.cs`
   - `Yaml/YamlDataLoader.cs`
3. 在 `src/Luban.DataTarget.Builtin/Luban.DataTarget.Builtin.csproj` 添加明确的 `Compile Remove`，排除：
   - `Yaml/YamlDataTarget.cs`
   - `Yaml/YamlDataVisitor.cs`

建议 XML 形态：

```xml
<!-- Luban.DataLoader.Builtin.csproj -->
<ItemGroup>
  <Compile Remove="DataVisitors/YamlDataCreator.cs" />
  <Compile Remove="DataVisitors/UnityAssetDataCreator.cs" />
  <Compile Remove="UnityAsset/UnityAssetDataSource.cs" />
  <Compile Remove="Yaml/YamlDataLoader.cs" />
</ItemGroup>

<!-- Luban.DataTarget.Builtin.csproj -->
<ItemGroup>
  <Compile Remove="Yaml/YamlDataTarget.cs" />
  <Compile Remove="Yaml/YamlDataVisitor.cs" />
</ItemGroup>
```

六个被排除源文件的 fixed preimage 是：

| source file | bytes | SHA-256 |
|---|---:|---|
| `src/Luban.DataLoader.Builtin/DataVisitors/YamlDataCreator.cs` | 7,218 | `96e8216d21cefc9192eb05ce0a41dcbfa0d3dc6bd95f5175aa46ded35f81fb9c` |
| `src/Luban.DataLoader.Builtin/DataVisitors/UnityAssetDataCreator.cs` | 1,267 | `c945421cc1d54775cf51fd2a0bd725bed1c45348fb98bcfb8935d8bde70eeaf3` |
| `src/Luban.DataLoader.Builtin/UnityAsset/UnityAssetDataSource.cs` | 3,599 | `a24d0eccc54793ba167db8bbfd5cedf775d8d12fe70a2ea358289f7e0fce4809` |
| `src/Luban.DataLoader.Builtin/Yaml/YamlDataLoader.cs` | 3,282 | `7762b789962801f993aced292bee92cd17f58250f96ef0a8be9363a4be9f32d1` |
| `src/Luban.DataTarget.Builtin/Yaml/YamlDataTarget.cs` | 2,100 | `20e0e600f1252d64c401f90e52c6e3bd39d2404e524dc5eb1ec2a3df28676308` |
| `src/Luban.DataTarget.Builtin/Yaml/YamlDataVisitor.cs` | 4,367 | `ed289e0abb2877ce3ab44f53d8fabb36c8dbe397d2a9197126a2b39351847bbb` |

不要删除源文件；`Compile Remove` 让补丁和上游 source tree manifest 都可审计。候选 B 只适用于 FightMatch 固定 CSV→JSON 用例，不能被宣传为通用 Luban runner。

专门门禁必须证明：

- 正向：现有 FightMatch CSV→JSON 输出与 official-source oracle 逐字节相同。
- 负向：`-d yaml` 或 `-d yml`（以实际 CLI 注册名为准）必须非零退出且不留下输出；Unity `.asset` 输入也必须非零退出。若仍因残余注册成功，则裁剪不完整，失败。

### 5.3 候选 C（已排除，不得签发）— 旧 DLL 的可复现 source-to-binary 证明

本节同样只作历史取证说明，不进入 M、B、L 的 package、process、time 或 storage
budget，也不得作为 A 失败后的同单路线。

研究候选为上游历史提交 `3eaa273a69855d3332bbb071ea9a0d2c52972d20` 及其 `project.json`。该路线只有同时满足下列条件才成立：

1. exact source commit、submodules（若有）、编译器／SDK、所有 restore packages、环境变量和构建命令全部本地冻结；
2. 构建出的目标 DLL 与当前 `YamlDotNet.NetCore.dll` 在长度和 SHA-256 上完全相同；
3. source tree 中许可证原字节及其覆盖关系可明确绑定该 DLL；
4. 两次从清空的独立 temp root 构建产生相同 DLL、PDB（若归档）和 manifest。

候选 source 的许可证研究值为：`LICENSE` 1,110 B／SHA-256 `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3`，`LICENSE-libyaml` 1,058 B／SHA-256 `d0d8b09800a45cd982e9568fc7669d9c1a4c330e275a821bbe24d54366d16fe9`。这只能说明候选源码有许可证，不能证明当前 NuGet DLL 来自该源码。IL 相似、API 相同、反编译相似或运行测试通过都不能替代 byte identity。

由于旧 nupkg 没有 source/PDB/build manifest，这条路线成本高且成功概率低。它不得阻塞候选 A；若不能 byte-match，记录 `NOT_PROVEN` 并结束，不进行无限工具链考古。

## 6. 未来实现的精确白名单

### 6.1 仓库写白名单

M 与 B 不得修改任何现有仓库文件；仓库内只可新建各自的 ignored evidence root，
外置 material/final candidate 写入另受第 6.3 节约束。L 阶段才有以下生产仓库写白名单：

1. `Tools/FightMatch.Localization.Compiler/Program.cs`
2. `Generated/FightMatch/Localization/fm-text-v1.json` — 仅允许以现有 SHA-256 `f48264…56bc` 的相同字节原子替换。
3. `Generated/FightMatch/Localization/fm-text-v1.manifest.json` — 仅允许以现有 SHA-256 `184371…3667` 的相同字节原子替换。
4. 新建且仅新建一次：`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-license-alt-yamldotnet-16.3.0-r1/`

除第 1 项外不允许对现有生产文件产生 diff。不存在候选 B 的替代根或自动改名规则。

### 6.2 临时写白名单

三阶段只能写各自新建、空的 run root；实际 `<RUN_ID>` 必须由对应 packet 在启动前
固定为一个不可复用的字面值，receipt 中不得保留通配符：

```text
/private/tmp/fightmatch-loc-lic-alt-m01/<M_RUN_ID>/
├── source-official/
├── source-patched/
├── feed-runner/
├── feed-tests/
├── licenses/
├── harness/
└── identity/

/private/tmp/fightmatch-loc-lic-alt-b01/<B_RUN_ID>/
├── pass-1/       # 独立 source/nuget/obj/bin/runner
├── pass-2/       # 第二个独立 source/nuget/obj/bin/runner
├── official-tests/
├── oracle/
├── candidate/
├── controls/
└── identity/

/private/tmp/fightmatch-loc-lic-alt-l01/<L_RUN_ID>/
├── pass-1/
├── pass-2/
├── negatives/
├── staging/
└── identity/
```

B 的 `pass-1/` 与 `pass-2/` 必须在开始时分别不存在；每个 pass 从 M 的只读 material
重新复制 source、feed 和 lock，并使用自己的 `nuget/obj/bin/runner`。一次 restore/publish
后复制目录、清输出再重用、或从 pass-1 派生 pass-2 都不算两个 pass。

### 6.3 最终外部工具白名单

M 在全部材料门通过后只允许 create-once 新建以下 `CANDIDATE_NOT_ACCEPTED` material
root；独立 R `ACCEPT` 后改为只读 accepted input，B 才能消费：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/
└── 5.1.0-fm-yamldotnet-16.3.0-m01/
    ├── source-official/
    ├── patch/
    ├── feed-runner/
    ├── feed-tests/
    ├── licenses/
    ├── harness/
    └── identity/
```

它的 `identity/material-files.tsv`、`package-files.tsv`、`license-files.tsv`、
`official-test-project.tsv`、两个 lock、`commands.json` 与 `process-budget.json` 必须逐项
写出 exact relative path、bytes、SHA；
接受的文件中不允许 `<package-id>`、`*`、`**` 或“其余依赖”等开放表达。若 official
tests 从五个 direct seeds 解析出尚未列出的 transitive、analyzer、build、native、
contentfiles package 或 license，M 必须在接收前把每一项加入 exact
manifest；无法冻结即 `BLOCKED_MATERIAL_FREEZE`。

B 技术关全部通过后只允许 create-once 新建以下 `CANDIDATE_NOT_ACTIVE` final runner
cache；独立 R 必须连同 temp proof 审查，形成 `B-ACCEPTED` 后 L 才能消费：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/
└── 5.1.0-fm-yamldotnet-16.3.0-r1/
    ├── runner/
    ├── identity/
    │   ├── upstream-source.tsv
    │   ├── source-files.tsv
    │   ├── patch.diff
    │   ├── patched-project-files.tsv
    │   ├── packages.lock.json
    │   ├── package-files.tsv
    │   ├── license-files.tsv
    │   ├── runner-files.tsv
    │   └── build-receipt.json
    ├── nuget/        # 文件路径只能来自 M 的逐项 package-files.tsv
    └── licenses/
        ├── Luban-v5.1.0-LICENSE
        ├── YamlDotNet-16.3.0-LICENSE.txt
        ├── YamlDotNet-16.3.0-LICENSE-libyaml.txt
        ├── NeoLua-1.3.14-LICENSE.md
        ├── CommandLineParser-2.9.1-License.md
        ├── ExcelDataReader-3.7.0-LICENSE
        ├── Google.Protobuf-3.29.0-LICENSE
        ├── MessagePack-3.1.7-LICENSE
        ├── MessagePack.Annotations-3.1.7-LICENSE
        ├── Microsoft.CSharp-4.5.0-LICENSE.TXT
        ├── Microsoft.NET.StringTools-17.11.4-THIRDPARTYNOTICES.txt
        ├── Newtonsoft.Json-13.0.1-LICENSE.md
        ├── Newtonsoft.Json.Bson-1.0.3-LICENSE.md
        ├── NLog-5.3.4-LICENSE
        ├── Scriban-7.2.5-LICENSE
        └── Ude.NetStandard-1.2.0-COPYING
```

上表是当前已知 runner locked/deployed closure 的最小命名集合，不是允许遗漏其它
M-resolved package 的上限。最终 B packet 必须把 M 接收的有限 `package-files.tsv` 与
`license-files.tsv` 原样展开到 packet 白名单；多一项、少一项或路径不同都失败，不能
依靠目录通配。test-only packages／licenses 必须完整保存在 M/B evidence 和 material
root，但只有同时属于 accepted runner locked/deployed closure 的条目才复制到 final
runner cache。

尤其是 `ExcelDataReader 3.7.0`、`Google.Protobuf 3.29.0`、`MessagePack 3.1.7`、
`MessagePack.Annotations 3.1.7`、`NLog 5.3.4`、`Scriban 7.2.5` 六项，当前旧证据只把
`.nuspec` 当 license fallback，不能沿用。M 必须为每一项取得并冻结 **official upstream
license source URL、tag 或 full commit、原始 bytes、SHA-256、license expression 和
本地 exact filename**。本文不猜这些尚未冻结的值；任一缺失即
`BLOCKED_MATERIAL_FREEZE`。

`nuget/` 要让 `cacheRoot = parent(runnerRoot)` 在离线状态下完成每个 deployed
DLL→exact nupkg entry 的复核；旧 `YamlDotNet.NetCore` nupkg 及仅由它引入的历史
`System.*` 包不得复制。`.nuspec` 可作 metadata，但不能出现在任何 component 的
`licenseSource` 字段。

当前 `/ApplicationData/Tools/Luban/5.1.0/` 只能读；不得重命名、覆盖、补文件或就地“修复”。先在 temp 完成全部检查，再以 create-once 方式复制到新的最终目录。若目标已存在，无条件停止，人工核对身份；不得 merge 或覆盖。

不允许写：其他 `Assets/`、`Packages/`、`ProjectSettings/`、`.meta`、`.git/`、旧工具缓存、旧证据根、`Library/`、`Temp/`、`Logs/`、`UserSettings/`、生成 `.csproj/.sln`。

### 6.4 `Program.cs` 精确适配要求

当前代码不仅固定 CLI：它还硬编码 old runner 的 132 files、manifest SHA `d9baacda…2bd5`、`Luban.dll` SHA `6e60beee…df7`；从旧 `005` evidence 取 `tool-identity.json`／`identity/runner-files.tsv`；`IdentifyPackage` 只认识 `YamlDotNet.NetCore.dll`；`ResolvePackageLicense` 在 nupkg 没有 license/notice entry 时把 `.nuspec` 当作许可证。后者正是当前 YAML 项不能真正闭环的原因，不能只换 DLL 后继续沿用。

未来唯一允许的 repo code patch 必须同时做到：

1. 保持现有 CLI 参数名、参数意义和 Luban invocation 不变；`--luban-dll` 仍是绝对路径，`--expected-tool-sha256` 仍由签包调用方显式传入。
2. 将 accepted runner count、manifest SHA、`Luban.dll` SHA 和 patched-runner identity 更新为候选构建后冻结的 exact 值；不得预填“仍为 132”。
3. `cacheRoot = parent(runnerRoot)` 的 seam 可保留，但身份来源改为新 cache 的 `identity/runner-files.tsv`、`identity/build-receipt.json`、`identity/package-files.tsv` 和 source/patch records；不得把旧 `005/tool-identity.json` 当作新 runner 的身份证明。旧 `005` 只能继续作为历史不可变输入审计。
4. `ValidateRunnerManifest`、`WriteLicenseInventory`、`WriteInputs`、`WriteRun` 和 self-test 中所有 old `132`／old manifest／old DLL literals 统一由同一个私有、编译期冻结的 patched-runner identity 提供，避免部分门禁仍接受旧 runner。不要增加外部配置可绕过身份。
5. `IdentifyPackage` 删除 `YamlDotNet.NetCore.dll → YamlDotNet.NetCore/1.0.0`，增加精确映射 `YamlDotNet.dll → YamlDotNet/yamldotnet/16.3.0`；old DLL 若出现必须成为 unmapped/forbidden 并失败。
6. `ResolvePackageLicense` 保留 “deployed bytes 必须命中 exact nupkg entry” 的强绑定，但完全删除 `.nuspec` 许可证 fallback。nupkg 内 license/notice entry 可用时仍需 hash；无内嵌文本时，只允许走签名的 `cacheRoot/licenses/` 显式清单，清单同时绑定 component/version/source commit/license path/hash。
7. 对 Luban、YamlDotNet 两份 license 与 NeoLua 保留已知 exact 绑定；同时逐项消费 M 的
   `license-files.tsv`，覆盖 runner locked/deployed closure 的每个 package。六个旧
   `.nuspec` fallback package 必须命中第 6.3 节冻结的 official upstream license bytes；
   `.nuspec` 只能作为 package metadata 归档，永远不得再次赋值给 `licenseSource`。
8. inventory 输出增加 M receipt SHA、B receipt SHA、upstream commit、package official
   hash、license kind/source commit 与 patched-runner identity；complete 仍要求 runner
   中每个文件 exactly-once coverage、每个 locked/deployed package exactly-one license
   record，且 license 文件集合与 M 的 exact manifest 相等。
9. 所有新证据写到新 evidence root；旧 root、旧 `005` 和旧 cache 只读。八条生产路径集合不增不减。

这组变化属于同一个门禁一致性 patch：如果只更新顶层常量而不更新 package/license resolution，或只更新 `IdentifyPackage` 而保留 nuspec fallback，都应判定为失败。

## 7. 构建与身份固定要求

### 7.1 候选 A 输入

M 必须冻结下表的材料字段；架构已经给出的 YamlDotNet 值在此直接采用，不再留空：

| 字段 | 必须绑定的值 |
|---|---|
| Luban upstream commit | `df34215d2e22035073d1af83d44e3ac0744dc3b3` |
| YamlDotNet tag commit | `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73` |
| nupkg identity | `YamlDotNet` / exact `16.3.0` / owner `aaubry` |
| nupkg SHA-256／bytes | `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4`／776,880 B |
| nupkg SHA-512 | `f102fb418c3afe5ec0cc5d0336b9ac6517af6f30603ab5f925a7b3622544c419bb935d1e55901147fff5a113874e55491534d5553411fb4ea198e6550c0421df` |
| package signature entry | `.signature.p7s`；SHA-256 `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308`；M 仍须冻结真实 verifier receipt |
| selected net8 DLL | 293,376 B；SHA-256 `86f873f682c619a0c24e263dcdd15fe027104666641c92f503516671d61edda6` |
| `LICENSE.txt` | 1,110 B；SHA-256 `daea8b7f1b2bab48ea12ad50b0ce91634669d99d7dd0cb64aed8dffeeef69ed3` |
| `LICENSE-libyaml` | 1,077 B；SHA-256 `cb65f46df0d9125ba60862117200ec6631fef3a8169cd0b4513ffcad4ef5cb9b` |
| exact unified patch | M receipt 的 exact SHA-256；只能是两处 PackageReference replacement |
| patched two csproj | M receipt 分别固定 after bytes／SHA-256 |
| runner build lock | M receipt 固定 exact path／bytes／SHA-256 |
| official test project | `src/Luban.Tests/Luban.Tests.csproj`；SHA-256 `d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1`；M 固定原始 bytes/path |
| five direct test seeds | `Microsoft.NET.Test.Sdk 17.11.1`、`Newtonsoft.Json 13.0.3`、`System.Text.Encoding.CodePages 8.0.0`、`xunit 2.9.2`、`xunit.runner.visualstudio 2.8.2` |
| official-test lock | M receipt 固定 exact path／bytes／SHA-256；从上述 csproj、五个 direct seeds 与其 project references 解析完整 test closure |
| complete package/license closure | M receipt 的逐项 `package-files.tsv`／`license-files.tsv`；零 wildcard、零 unknown |
| official-test harness and budget | M receipt 的 exact argv、fixture hashes、DUT matrix、子进程与 temp budget |

runner file count／manifest、candidate `Luban.dll` 与最终 `YamlDotNet.dll` hash **不是 M
前置字段**。它们只能由 B 在消费已接收 M 后实测并写入 `B-ACCEPTED` receipt；L 只
消费这些 B 字段。这样 M 不依赖尚未构建的 runner identity。

六个 material-freeze license 字段必须在 M 中逐项出现：

| component | 固定 package version | M 必须填满、不得猜测的字段 |
|---|---:|---|
| ExcelDataReader | 3.7.0 | official URL；tag/full commit；license source path；bytes；SHA-256；expression |
| Google.Protobuf | 3.29.0 | 同上 |
| MessagePack | 3.1.7 | 同上 |
| MessagePack.Annotations | 3.1.7 | 同上；不得默认为与 MessagePack 共用而不留关系证据 |
| NLog | 5.3.4 | 同上 |
| Scriban | 7.2.5 | 同上 |

official-test graph 的 direct seeds 必须逐名冻结如下；这些版本来自上述 exact csproj，
不是对完整 test closure 的推断：

| direct PackageReference | exact version | source semantics |
|---|---:|---|
| Microsoft.NET.Test.Sdk | 17.11.1 | direct test SDK seed |
| Newtonsoft.Json | 13.0.3 | direct test seed；不得误用 runner closure 的 13.0.1 代替 |
| System.Text.Encoding.CodePages | 8.0.0 | direct test seed |
| xunit | 2.9.2 | direct test framework seed |
| xunit.runner.visualstudio | 2.8.2 | 保持 source 的 `IncludeAssets=runtime; build; native; contentfiles; analyzers; buildtransitive` 与 `PrivateAssets=all` |

五项只是 direct seed。M 必须从 exact csproj／project references 解析并逐项冻结它们的
所有 transitives、analyzers、build/buildTransitive、native、contentfiles、testhost 与
其它运行时资产；每个 package 的 official source、nupkg bytes/hash、license bytes/hash
及 lock edge 都必须进入 M manifest。本文不虚构这些尚未实测的 nupkg/hash/license 值；
任一未知即 `BLOCKED_MATERIAL_FREEZE`。

这些是显式签名字段，不是可留到 B/L 执行者自由填写的选择。材料取得必须由独立、
另行授权的 M packet 完成；本文不授权联网下载。

### 7.2 建议的离线命令形态

M receipt 必须把下列模板展开成 pass-1、pass-2 的两组绝对 argv；B 只能逐字消费。
这里是设计模板，不是运行授权：

```text
dotnet restore <B_PASS_ROOT>/source/src/Luban/Luban.csproj
  --source <M_ROOT>/feed-runner
  --packages <B_PASS_ROOT>/nuget
  --locked-mode
  -p:NuGetLockFilePath=<M_ROOT>/identity/runner.packages.lock.json

dotnet publish <B_PASS_ROOT>/source/src/Luban/Luban.csproj
  -c Release
  --no-restore
  --disable-build-servers
  -p:ContinuousIntegrationBuild=true
  -p:Deterministic=true
  -p:PathMap=<B_PASS_ROOT>/source=/_/luban-v5.1.0
  -o <B_PASS_ROOT>/runner
```

`<B_PASS_ROOT>` 必须分别展开为 B 临时根下的 `pass-1` 与 `pass-2`；两组各有自己的
source、nuget、obj、bin、runner。每组恰好 **1 次 restore + 1 次 publish**，总计
2 restore + 2 publish；不能让一次 restore/publish 的结果服务两个 pass。

official tests 使用第三个独立 `official-tests/` 根。运行前必须验证复制后的
`src/Luban.Tests/Luban.Tests.csproj` 仍为 SHA-256
`d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1`，并由 M 固定三条
顶层命令：

```text
dotnet restore <TEST_ROOT>/source/src/Luban.Tests/Luban.Tests.csproj
  --source <M_ROOT>/feed-tests
  --packages <TEST_ROOT>/nuget
  --locked-mode
  -p:NuGetLockFilePath=<M_ROOT>/identity/tests.packages.lock.json

dotnet build <TEST_ROOT>/source/src/Luban.Tests/Luban.Tests.csproj
  -c Release --no-restore --disable-build-servers
  -p:ContinuousIntegrationBuild=true -p:Deterministic=true
  -p:PathMap=<TEST_ROOT>/source=/_/luban-v5.1.0

dotnet test <TEST_ROOT>/source/src/Luban.Tests/Luban.Tests.csproj
  -c Release --no-restore --no-build --disable-build-servers
```

因此 B 在 parity 之前的顶层 .NET process budget 恰好为 7：两个 pass 的 4 个，
official tests 的 restore/build/test 各 1 个。publish 与 official-test build 产生的
MSBuild、compiler、testhost 等真实 child processes 不能被“包含在 dotnet 内”而忽略；
M 的 `process-budget.json` 必须按命令逐项冻结 executable identity、最大启动次数、
最大同时数、timeout 与 log cap。parity harness 的顶层 invocation 数、它触发的 old
oracle／candidate DUT 次数、任何 implicit build 和子进程也必须在 M 中给出 exact
整数。无法在 M 冻结这些数，或实际 B 多启动一个进程，立即
`BLOCKED_PROCESS_BUDGET`。

要求：

- `dotnet --version` 必须精确是 `8.0.425`。
- build 环境阻断网络；runner/test restore 只读 M receipt 分别固定的
  `feed-runner`／`feed-tests`，不能回退到用户 NuGet 缓存或远程源。
- runner 与 official-test 两个 lock 必须在 M 中先生成、审核、签 SHA；两个 build pass
  使用同一份 runner lock，official tests 使用自己的 test lock。B 不得现场解析、更新
  或添加／更新五个 direct seeds 及其任何 transitive、analyzer、build、native、
  contentfiles、testhost package。
- test lock 的五个 direct entries 必须精确为 `Microsoft.NET.Test.Sdk 17.11.1`、
  `Newtonsoft.Json 13.0.3`、`System.Text.Encoding.CodePages 8.0.0`、`xunit 2.9.2`、
  `xunit.runner.visualstudio 2.8.2`；后者的 IncludeAssets/PrivateAssets 语义必须与 exact
  csproj 相同。五项之外的完整 closure 只能来自 M accepted test lock/feed。
- lock 必须只包含获准的 exact versions；`YamlDotNet.NetCore` 及其只由它带入的旧 `System.*` 边必须为零。
- `YamlDotNet/16.3.0` 必须是两个项目的直接依赖；`net8.0` 选中的 asset 必须来自 nupkg 记录的精确 entry。
- publish 后扫描全部文件，任何未列入预期 manifest 的文件失败。
- 两次 pass 分别把各自绝对 source root 映射到同一个虚拟路径 `/_/luban-v5.1.0`；
  比较 source manifest、patch、lock、完整 runner manifest 与关键 DLL。若 PDB 仍含 host
  路径或造成差异，先停单；不得静默排除或删除来伪造 deterministic build。

## 8. 许可证与 SBOM 闭环

patched-runner 的 identity 必须同时表达“上游来源”和“本地改动”：

```text
component: Luban
version: v5.1.0
upstream_commit: df34215...
distribution_identity: FightMatch patched-runner r1
patch_sha256: <exact>

component: YamlDotNet
version: 16.3.0
tag_commit: ae480660...
package_sha256: <exact>
license: MIT
license_evidence: exact local LICENSE.txt + LICENSE-libyaml bytes
deployed_files: runner/YamlDotNet.dll <sha256>
```

最小 SBOM／license inventory 每行至少含：component、version、supplier/owner、source URL、source commit/tag、package filename、package SHA-256、official catalog SHA-512、license expression、license file path/SHA-256、deployed file path/SHA-256、dependency kind（direct/transitive）。

完整性条件：

- runner 中每个非项目自有二进制恰有一条归属；零 unmatched、零 duplicate owner、零 unknown license。
- package-to-deployed-file 必须由 nupkg exact entry bytes 或等价可重算 extraction manifest 绑定，不能仅按 assembly name 猜测。
- `YamlDotNet.NetCore` 名称、DLL 和 lock edge 都必须为零。
- NeoLua 仍按已研究的 exact `v1.3.14` commit `cb560afef9dd19b7afad6da594b44909c9739698`、Apache license 11,324 B／SHA-256 `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae` 闭环；替换 YAML 不得回退或遗漏 NeoLua 的 FIX-03 证据要求。
- 两份 YamlDotNet license 都归档；即便当前二进制未触发 libyaml 衍生代码，也采用保守包含，避免未来能力路径变化造成证据缺口。
- 六个旧 `.nuspec` fallback component 必须逐项指向 M 冻结的 official upstream license
  bytes；共享一份 upstream license 只有在 M 保存 exact project relationship 证据后才可，
  不能按相同组织名推断。
- build-only、analyzer 与 official-test-only packages 也必须在 M/B SBOM 中逐项出现并有
  license evidence；它们可以不复制到 final runner cache，但不能从 closure、预算或
  evidence 中消失。
- official-test SBOM/lock 必须明确标出五个 direct seeds 与全部 resolved children；
  `xunit.runner.visualstudio` 的 runtime/build/native/contentfiles/analyzers/buildtransitive
  assets 和实际 testhost files 都要逐项归属，不能只记录顶层 package 行。
- `package-files.tsv`、`license-files.tsv`、SBOM 与 runner deployed-file inventory 必须
  做集合等值检查；不能用目录存在、包名猜测或通配匹配代替逐项相等。

## 9. 完整验证设计

### 9.1 patched-runner 构建／对照关

M 先完成材料 closure 并获独立接收；B 才按以下顺序证明唯一候选 A：

1. 校验 `M-ACCEPTED` receipt、material root、固定输入、old cache 和三棵旧证据根未变。
2. 校验 B temp、B evidence 与 final runner root 都不存在；创建 B temp/evidence，不创建 L root。
3. 在 pass-1 空根验证并复制 M exact source、patch、runner lock、feed 和 licenses；执行该 pass 自己的 restore+publish。
4. 在 pass-2 的另一个空根从相同 M bytes 重新开始，再执行一次独立 restore+publish。
5. 比较两个 pass 的 source、patch、lock、完整 runner manifest、DLL/PDB/JSON 与所有决定性制品；不能选择较好一份。
6. 在独立 official-tests 根使用 M 的 test lock/feed，执行固定 restore→build→test；zero failed/skipped 与真实 baseline count 都必须满足 M receipt。
7. 对每个 pass 分别生成完整 SBOM/license inventory，且两者与 M closure 集合一致。
8. 用当前 accepted source-built unpatched runner（oracle）生成结果，再用 pass-1 与 pass-2 runner 对相同 FightMatch 输入生成 candidate；raw、JSON、publication manifest 与 semantic digest 均逐字节／逐字段满足架构合同。
9. current oracle 与两个 candidate runner 分别运行同一个有效 YAML fixture，至少覆盖 `yml` loader→`yaml` target；输出解析为 YAML tree 后做结构／标量逐项相等比较。再用一份最小 Unity `.asset` fixture 对比 `asset` loader→`json`。两边原始输出必须保存。
10. current oracle 与两个 candidate runner 分别运行同一个畸形 YAML fixture；直接 runner DUT 都必须非零退出、捕获真实 stderr 且 output root 为空。错误文案允许版本差异，失败阶段、无发布和非零结果必须相同。
11. 运行 M 已冻结的 005 parity harness 与官方 fixture matrix；不得写旧 005 evidence。
12. 全部通过后冻结 B runner count/manifest 与关键 DLL hashes；create-once 复制 final
    cache，标记 `CANDIDATE_NOT_ACTIVE`，并对 final bytes 重新 manifest。独立 R 必须把
    temp proof、final copy 与 manifest 一并审查；只有其 `ACCEPT` 后才形成
    `B-ACCEPTED`，L 才能消费。R 前不得把 final cache 当作 active runner。

所有子进程都必须由外层记录器捕获：absolute executable、逐参数 argv、cwd、开始／结束 UTC、PID、exit code、stdout/stderr file hashes、timeout、kill escalation、process-tree quiescence。shell 文本和自述 JSON 不能代替真实进程证据。

### 9.2 LOC-A 原有验证关

patched-runner 固定后，新的实现包必须保留 FIX-03 的语义，不得缩减：

- 12 个 self-tests 全部通过。
- 两次独立 clean pass，输出及证据决定性字段一致。
- 三个既有负向测试必须原名保留且全部通过其负向断言：`negative-duplicate` 观察 `FMLOC002`、`negative-parameters` 观察 `FMLOC007`、`negative-severity` 观察 `FMLOC004`；每项都不得发布 canonical artifact。
- 新增一个 package negative：把 candidate runner 的 `YamlDotNet.dll` 替换／改一字节后，preflight 必须在启动 Luban 前失败。
- 新增一个 license negative：移走／改变 M exact `license-files.tsv` 中任一 final-cache
  license 后，license gate 必须在启动 Luban 前失败；至少显式覆盖 YamlDotNet 和六个
  原 `.nuspec` fallback component 中一项。
- 新增一个 YAML behavior positive 与一个 malformed-YAML negative；它们只使用 temp fixture，不进入八个生产路径。
- existing semantic comparison、publication manifest comparison 和 exact byte comparison 全部保留。
- success pass 后 repo diff 只能包含获准的 `Program.cs` 变化和新 evidence root；生成 JSON／manifest 若被触碰，最终 bytes 必须仍与固定 SHA 相同。

现有 12 项必须按原名逐项保留：`schema-column-count`、`duplicate-key`、
`empty-translation`、`invalid-severity`、`missing-parameter`、
`multiple-parameters`、`unclosed-template`、`unknown-raw-column`、`sorting`、
`newlines`、`round-trip`、`duplicate-generation`。runner identity、package graph、
license completeness、staging isolation、atomic publication、negative no-output 和真实
process receipt/quiescence 作为外层 gate／附加测试，不得通过改名或替换把现有总数
仍伪装成 12/12；新增后报告实际总数与逐项结果。

### 9.3 DUT 与 harness exit code 合同

负例必须分别记录被测进程（DUT）与判定它的 harness exit code；不能再使用“任何
negative 返回 0 即失败”的笼统规则：

| 场景 | DUT 期望 | harness 期望 |
|---|---|---|
| direct malformed-YAML old/candidate runner | 非零；stderr 有真实诊断；output root 为空 | 观察到两边所需失败阶段、非零与无输出后返回 `0`；否则非零 |
| tampered `YamlDotNet.dll` package preflight | 非零；在启动 Luban 前失败；无 staging/publication | 正确观察 package gate 后返回 `0`；否则非零 |
| missing／changed license preflight | 非零；在启动 Luban 前失败；无 staging/publication | 正确观察 license gate 后返回 `0`；否则非零 |
| `negative-duplicate` direct compiler DUT | 非零；含 `FMLOC002`；无 canonical publication | 正确观察后 harness 返回 `0` |
| `negative-parameters` direct compiler DUT | 非零；含 `FMLOC007`；无 canonical publication | 正确观察后 harness 返回 `0` |
| `negative-severity` direct compiler DUT | 非零；含 `FMLOC004`；无 canonical publication | 正确观察后 harness 返回 `0` |

process receipt 必须用 `role=DUT|harness` 标注每个 exit code。harness 返回 0 只表示它
正确验证了预期负例，不得改写为 DUT 成功；DUT 返回 0、产生输出或 publication 才是
负例失败。

### 9.4 Mac／Windows 边界

本研究只确认 Mac 本地材料与 primary sources；没有运行 Windows 构建。框架依赖式 net8 runner 的设计目标是继续支持 Mac 与 Windows，但不得据此声称跨平台已验证。发布前必须由另行授权的 Windows packet：

- 使用同一 SDK `8.0.425`、source commit、patch、lock、package bytes；
- 执行同一 CSV→JSON oracle 和 YAML 正负对照；
- 解释 runner manifest 的平台差异，或证明共用同一 managed runner bytes；
- 不改变 Unity/package/project settings。

## 10. 证据根、旧根保护与接收

以下三棵历史证据根永久只读：

| 根 | 文件数 | 关键固定值 |
|---|---:|---|
| original `…/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/` | 37 | run `e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`；inputs `07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2` |
| `…-fix-01/` | 15 | run `19151d7f0cd9a2bfeaeddf090d267a35941722ba1cc9a163bba229c4cabc8dc4`；inputs `3852210c00e9a053691b9fd05bfc3e2642c84939e3d6088634f639873569281c`；commands `3debf91a90e48c8c48828f97afeeb47a8dfe574d170f0825b7d3283eee67aa9b` |
| `…-fix-02/` | 74 | run `43c4f7d9cf78194cd97b6a2b277c40a4bde9be38ad6f359e133dab17c25e3110`；inputs `c191f93674f74199434c39a2c31b8a34e08e6a3925dca39dcec5d64a35af3582`；commands `803a5f686d5999c8b7bd515227fa7cd39b6703f553ee6e6fe30aff008e52cb85`；license inventory `1bfeb05a91d247738b7ef36b5bdbb86b2526e6868ed5737b791f1288b23a352f` |

未来开始前和结束后都要保存旧根的递归 path/size/SHA manifest，并断言 before/after 完全相同。不得向旧根追加“更正”文件。FIX-03 提议的 `…-fix-03/` 尚未执行；它不能被本设计默认为已批准，也不能与新 runner evidence 混用。

三个新 create-once evidence root 固定为：

```text
TestArtifacts/FightMatch/LOC-LIC-ALT-M/
  loc-lic-alt-yamldotnet-16.3.0-m01/

TestArtifacts/FightMatch/LOC-LIC-ALT-B/
  loc-lic-alt-yamldotnet-16.3.0-b01/

TestArtifacts/FightMatch/LOC-IMPL-A/
  ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-license-alt-yamldotnet-16.3.0-r1/
```

M 根至少包含：authority、fixed inputs、official source、two-csproj patch、exact
`Luban.Tests.csproj` preimage、五个 direct test seeds、runner/test locks、完整 build/test
package closure、逐项 licenses、official-test/harness commands、process/temp budgets、
exact temp/material/final path manifests、`M-ACCEPTED` 或 failure receipt。

B 根至少包含：M receipt/hash、old-root before/after manifests、两个独立空根的
restore/publish/process receipts、official tests、两个 runner manifests、SBOM、license
inventory、oracle/candidate raw/semantic/byte comparisons、YAML／UnityAsset／005 parity、
final-cache create-once receipt、`B-ACCEPTED` 或 failure receipt。

L 根至少包含：M/B receipts、Program preimage/diff、两个 LOC-A clean pass、12 self-tests、
三项既有 negatives、package/license negatives、DUT/harness exit table、semantic／publication
manifest／exact byte comparison、repo whitelist audit 和 failure/success receipt。B 的 source、
build 或 official-test 证据不得复制成 L 自称的新执行。

每个证据根自身只创建一次：开始时必须不存在；创建后只允许该阶段执行器写入；结束
封存 manifest。中途失败也保留失败证据，下一次重试必须经新 packet 固定新后缀，
不擦除、不复用。任何阶段都不得向另一阶段或三棵历史根追加文件。

## 11. 资源预算与停止规则

M 必须把最终 exact 数字写入 `process-budget.json`／`temp-budget.json`；下表是不得扩大的
统一硬上限，不是允许 B/L 自行填数：

| 阶段／项 | 硬上限与精确规则 |
|---|---|
| M wall clock | ≤60 分钟；仅未来独立授权的材料取得／核验；本文为 0 |
| M network | 只能是 M exact artifact manifest 逐项列出的 official URL；请求数、bytes、redirect 上限在 M packet 先固定；无法固定即 BLOCKED |
| B wall clock | ≤120 分钟总计；pass-1、pass-2、official tests 与 parity 共用，不另加“每 pass”额度 |
| B runner passes | 正好 2 个独立空根；每 pass 恰好 1 restore + 1 publish；不自动第三次重试 |
| B official tests | 恰好 1 restore + 1 build + 1 test；五个 direct seeds、完整 resolved test closure 与所有 child process caps 由 M 固定 |
| B 顶层 .NET processes | parity 前恰好 7；parity harness/DUT 的额外 exact 数由 M 填满，不能留变量 |
| L runner build/restore | 0；L 只消费 B accepted cache |
| L LOC-A clean passes | 正好 2；12 self-tests、3 existing negatives、2 package/license negatives 与其 DUT/harness 次数在 L packet 逐项固定 |
| 单 restore/publish/build/test timeout | ≤10 分钟；M 可按命令降低，不得提高 |
| 单 Luban/control DUT timeout | ≤5 分钟；harness 总 timeout 在 M/L 对应 packet 精确固定 |
| 并发外部进程 | 1 个 process tree；树内实际 children 仍计数并记录 |
| B temp 总空间 | ≤2 GiB：pass-1 ≤768 MiB、pass-2 ≤768 MiB、official-tests+controls ≤512 MiB |
| L temp 总空间 | ≤1 GiB |
| 新 material root | ≤512 MiB；M receipt 记录 exact bytes |
| 新 final runner cache | ≤128 MiB |
| 每个新 evidence root | ≤128 MiB；超出先停单 |
| B/L network | 0 |

M 接收的 process budget 必须列出每条顶层命令和每种可执行 child 的 exact 最大启动次数；
official-test harness 若隐式 build、testhost 或另起 runner，也必须计入。不能以“由 dotnet
内部启动”排除计数。若材料阶段拿不到五个 direct test seeds 中任一项、不能解析并冻结
其完整 closure、test lock／harness argv 或 exact process/temp budget，M 必须返回
`BLOCKED_MATERIAL_FREEZE`，不得派 B。

任一条件立即停止且不发布：

- 固定输入、上游 source manifest、old cache 或旧证据根不匹配；
- M receipt 未接收、nupkg／catalog／完整 license identity 未绑定或不匹配；
- restore 尝试访问网络，或从非白名单缓存取包；
- patch 触及白名单外 source；
- build graph 仍含 `YamlDotNet.NetCore` 或 unknown/transitive license；
- 两个独立空根任一未各自完成 restore+publish，或两 pass 不确定；
- oracle/candidate JSON 或 publication manifest 非逐字节相同；
- 专门 YAML 对照不符合候选定义；
- 任一 negative DUT 返回 0、留下输出或发生 publication；或负例 harness 没有按第 9.3
  节返回预期值；
- 实际 process 数、temp bytes 或 test package closure 超出 M exact budget；
- process tree 未静默、timeout、最终目录已存在；
- repo diff 超白名单或预算超限。

不要在同一包内“顺手修复”失败。保存证据，回报最小失败事实，由主程／系统设计决定新包。

## 12. 失败、回滚与升级路径

M 失败时保留 M failure evidence，不创建或更新 B/L 根；material root 若已 create-once，
标记 `REJECTED_NOT_ACCEPTED`，不得补写。B 失败时保留两个 pass 与 failure evidence，
不创建 L 根，不修改 `Program.cs`。当前只读 `5.1.0/` cache 和生产输出始终不变。
若 final cache 已 create-once 但 B/L preflight 失败，不删除也不覆盖；标记
`REJECTED_NOT_ACTIVE`，新修订只能经新 packet 使用 `r2` 新目录。

`Program.cs` 的未来修改必须先对 patched runner preflight 成功，才可进入 production run。若仓库验证失败，恢复只能由获准 packet 把该文件回到签包前固定字节；禁止 `git reset --hard`、广域 checkout 或删除用户变更。生成 JSON／manifest 因要求 byte-identical，回滚时应从签包前备份原子恢复并重算固定 SHA。

唯一候选 A 的状态规则：

```text
M closure/license/test budget 全部 exact
  → M 独立接收后才可派 B

B 两空根 build + official tests + byte oracle + YAML/UnityAsset/005 controls pass
  → B 独立接收后才可派 L

L 两 clean pass + self-tests + negatives + exact output pass
  → 交独立实现 R；不自动扩大到其它 localization 工作

M 材料缺失／预算无法冻结
  → BLOCKED_MATERIAL_FREEZE

A API／behavior、build、test、parity 或 output 任一失败
  → NO_GO；不得现场加 shim、换 YamlDotNet 版本或切换 B/C
```

## 13. 接收标准

### 13.1 M 接收 checklist

- 唯一路线 A；source commit、两处 patch 和 SDK 固定。
- runner/test 两个 lock、所有 nupkg/source bytes、完整 locked/build/test/deployed closure
  逐项 exact；零 wildcard、零 unknown。
- `Luban.Tests.csproj` SHA 为 `d3fc132b9ae2a3881c142d9d88d0c1ce6ab01e148dcc087dfddb634261ebb2f1`；
  五个 direct entries 精确为 `Microsoft.NET.Test.Sdk 17.11.1`、`Newtonsoft.Json 13.0.3`、
  `System.Text.Encoding.CodePages 8.0.0`、`xunit 2.9.2`、`xunit.runner.visualstudio 2.8.2`，
  且 runner.visualstudio 的 IncludeAssets/PrivateAssets 语义未变。
- 五个 direct seeds 的所有 transitives、analyzers/build/native/contentfiles、source/package/
  license bytes、testhost 与 child-process caps 已逐项冻结；任一未知即
  `BLOCKED_MATERIAL_FREEZE`。
- 第 6.3／7.1 节六项 official upstream license 的 URL、tag/full commit、bytes、SHA、
  expression 全部填满；其它 closure licenses 同样逐项闭环。
- official-test/harness argv、fixture hashes、DUT matrix、7 个固定顶层 .NET processes、
  所有额外 harness/child exact counts、timeout/log/temp budgets 已签名。
- M evidence 与 material root create-once；不要求、不伪造 runner count/hash。
- 独立 R 给 M 唯一 `ACCEPT` 后，主程才能签 B。

### 13.2 B 接收 checklist

- 只读消费 accepted M；两个独立空根分别完成 1 restore + 1 publish。
- official tests 使用 frozen test lock，真实 baseline 全过；所有实际 processes/temp bytes
  未超过 M exact budget。
- 两 runner manifests／critical DLLs 确定；完整 SBOM/license inventory 与 M closure
  集合相等。
- YAML、UnityAsset、005、FightMatch raw/canonical/manifest/semantic parity 全过；负例按
  DUT/harness 双 exit 合同记录。
- final cache 已 create-once 为 `CANDIDATE_NOT_ACTIVE` 并重算 manifest；B 独立 R 同时审查
  temp proof 与 final bytes，唯一 `ACCEPT` 后形成 `B-ACCEPTED`，主程才可签 L。

### 13.3 L 接收 checklist

- 只读消费 accepted B runner count/manifest/DLL hashes；不 restore/build runner。
- repo/temp/final/evidence 白名单逐项 exact；`Program.cs` 是唯一允许的 production
  source delta，另七个生产路径最终 bytes 不变。
- 两 LOC-A clean pass、12 个原名 self-tests、三既有 negative harness、package/license
  negatives、semantic/publication manifest/exact byte comparisons 全部满足合同。
- 新 L evidence create-once；旧三根、M/B 根与旧 cache before/after 不变。
- QA/R 由测试主程在 L 证据完成后另行派发，作者不得自验收。

责任身份：本设计 owner 为主程下的系统设计角色；M、B、L 各自必须在 packet 中记录
delivery owner、实际 thread/host/turn、上游 receipt SHA 和 return path。当前修订 authority
来自主程 thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` 的 `SYS-FIX-01` 下发；它只
授权修改本文，不授权 M/B/L 执行。

本文的当前 verdict 是：**PREPARED_NOT_AUTHORIZED**。当前唯一候选是 A。YamlDotNet
16.3.0 的架构固定 package/license bytes 已写入，但完整 locked/test/deployed license
closure、M process/temp budget、B runner identity 和 L 适配仍必须按三阶段分别接收。
没有下载、构建、测试、缓存写入、生产修改或后续派发已由本修订完成。
