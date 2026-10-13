# LOC-TOOL-01-DESIGN：Luban 本地化制作链冻结

2026-10-01 · r3 · 状态：**READY_FOR_INDEPENDENT_REVIEW**。Luban `v5.1.0`、固定
外盘 runner／SDK 身份已验证；Mac Intel 005=`PASS_MAC_PREFLIGHT`，并获主程
`ACCEPT`。这只证明 2-row scratch 制作链可行。正式生产输入、Config、project
CLI、Unity 接入仍为 `NOT_RUN`。COPY FIX-01 已获独立 R `ACCEPT`，但 263-key
版本只作为 UGUI 文案快照 provenance；主策已冻结该快照，COPY AMEND-04 已
**只新增** `fm.language.save_unknown(errorCode)` 并获原独立 R `ACCEPT`，成为
LOC-IMPL-A 最终 COPY 输入。本设计尚待同一 R 独立复审，A/B 都是
`NOT_AUTHORIZED`。Windows 为
`NOT_VERIFIED / WAITING_FOR_WINDOWS_HOST`：不阻断 Mac 侧 A/B 模块候选工作，但
禁止 Windows／跨平台接收或发行就绪声明。

交付负责人：系统技术设计
`01a0f2e6-29bb-7092-abf0-705b41b7bf93` / `local`；正式回传主程
`01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。授权来自根
`AGENTS.md` 第 6 节、`.agent/TEAM_WORKFLOW.md`、本轮 README 和主程委托。

## 1. 固定输入与边界

- r3 修订起点：本文 r2 SHA-256
  `99fdf7d56d90598aefc4f7257725eaa3dff2af5299f2fbe15efa7a3c8bfe4dbb`，
  553 lines / 34,814 bytes。
- LOC-R3-FIX-01 候选 SHA-256
  `f4455bb214627a08478cdf690e751601a45a388a84966f6d7d0c211f8aa4b6ed`，
  689 lines / 46,711 bytes；独立 R verdict=`NEEDS_FIX`：thread
  `01a0e404-e8ee-7310-8388-9260babd53f1` / turn
  `01a0f3fb-a0fb-76a0-9a06-4719d8da83dc` / final
  `msg_0d29bf8d726d8dc0016abd70ecf84887d0af99b775be765c1b`。F3 已关闭；本次
  LOC-R3-FIX-02 只修 F1／F2／F4，不重开其它边界。
- 源码起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`。
- 上游架构：`engineering-architecture-001.md`，SHA-256
  `309762cb3c4062aab9beadbccb5e34fbc9225d53fb9339f85021e46668513325`。
- 系统设计：`engineering-system-001.md`，当前冻结 SHA-256
  `4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e`。
- UGUI implementation：`engineering-ugui-01-implementation.md`，当前唯一冻结 SHA-256
  `ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`，435 lines /
  26,488 bytes；旧
  `a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a` 只属主程重签前
  历史，不得作为当前输入。
- 主策 r3 初始输入：`planning.md` SHA-256
  `11d4c1891e0a21cabc1459695aac9473491e46e63de6008ce98c625cc5b30d9d`；
  COPY FIX-01 接受的 263-key UGUI 快照为 `planning.md` SHA-256
  `8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`；
  COPY AMEND-04 接受的当前 `planning.md` SHA-256
  `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`，
  372 lines / 47,305 bytes。
- COPY FIX-01 接受的 UGUI 快照 `planning-localization-draft.csv` SHA-256
  `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`，
  64,708 bytes、264 个物理行、9 列。RFC 4180 只读复算为 263 个数据记录／
  263 个唯一 key。COPY 独立 R `ACCEPT` 身份：thread
  `01a0e404-e8ee-7310-8388-9260babd53f1` / turn
  `01a0f3db-8a86-7e63-b77f-2f121cedbd0b` / final
  `msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`。F1～F3 已关闭，
  81 dispositions／47 new／263 unique 接受；它只冻结 AMEND-04 之前的 UGUI 文案
  快照，不再是 A 的最终输入，也不证明 runtime、Luban 或设备。
- COPY AMEND-04 最终 A 输入是当前 `planning-localization-draft.csv`：SHA-256
  `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，
  65,358 bytes、265 个物理行、9 列；RFC 4180 为 264 个数据记录／264 个唯一 key，
  severity=`info/warning/error/blocking=172/74/5/13`，parameter mismatch=`0`。
  它只在上述 263-key 快照后新增 `fm.language.save_unknown(errorCode)`，旧 snapshot/
  receipt 保持不变。独立 R `ACCEPT`：thread
  `01a0e404-e8ee-7310-8388-9260babd53f1` / turn
  `01a0f3f2-c361-7311-9bad-db9b91046023` / final
  `msg_0d29bf8d726d8dc0016abd6de210dc87d0a7a904c914de06f9`。
  前一
  `0228ca7ceadf70af140af7b03de498630c0dc5e6ebd30704457af1aa457bf765`
  已获 `NEEDS_FIX`，只作修订前 provenance。
- 当前候选的前 217 个物理行 SHA-256 为
  `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`；
  它证明旧 216-key 前缀未改写。170／171／216-key 版本及其旧 SHA 只作历史
  provenance，均不是本次迁移 authority、第二输入或可选回退源。
- 005 Mac evidence root：
  `TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/`；`run.json`
  SHA-256 `ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232`，
  主程 verdict=`ACCEPT`。该 verdict 不接受 264-key 生产生成或 Unity 接入。
- 首发 locale 只有 `en`、`zh-Hans`；手选记忆独立于 PlayerSave；所有玩家
  可见 Prefab TMP 默认值为 `【if you see this, it is a bug.】`。
- 本包只设计 Excel/CSV→Luban→项目 artifact→既有审查／发布入口和 Locale
  preference adapter。不安装工具、不运行 Unity、不修改 Packages、
  ProjectSettings、产品代码、资源、`.meta` 或 Git。
- 不引入 Unity Localization、Addressables；不把 JSON 变成第二人工权威；
  不扩到 YooAsset 运输、R2/CDN、HybridCLR、后台部署或设备验收。

## 2. 官方一手证据（检索日 2026-10-01）

| 事实 | 官方证据 | 设计结论 |
| --- | --- | --- |
| GitHub 最新 release 为 `v5.1.0`，发布于 `2026-09-10T12:06:39Z`；tag 解引用为 commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`，该 commit 未签名 | [官方 Release](https://github.com/focus-creative-games/luban/releases/tag/v5.1.0)、[release API](https://api.github.com/repos/focus-creative-games/luban/releases/tags/v5.1.0)、[tag ref API](https://api.github.com/repos/focus-creative-games/luban/git/ref/tags/v5.1.0)、[固定 commit](https://github.com/focus-creative-games/luban/commit/df34215d2e22035073d1af83d44e3ac0744dc3b3) | 锁 tag＋完整 commit；未签名是供应链记录，不得用短 SHA 或浮动 `latest` 代替。 |
| 官方 `Luban.7z` 为 1,647,739 bytes，GitHub digest 为 `sha256:bac9a1b8d69cfeaa7ef1d8c67b34a007324d1cd158e0b14294521b3879ba5213`；release `immutable=false` | [官方资产 URL](https://github.com/focus-creative-games/luban/releases/download/v5.1.0/Luban.7z)、[release API](https://api.github.com/repos/focus-creative-games/luban/releases/tags/v5.1.0) | 只接收精确 bytes＋SHA；因 release 可变，每次下载都重新核 hash，接收后镜像到外盘版本目录。 |
| 5.x 官方安装要求 .NET SDK 8.0+，支持 release 或源码构建，常规入口是 `dotnet Luban.dll` | [安装与获取工具](https://www.datable.cn/docs/guide/install) | 工具仅在制作机运行；Mac/Windows 必须使用同一 SDK patch。 |
| 官方声明 Luban 基于 .NET，支持 Windows/Linux/macOS，支持 CSV/Excel 与 Unity/C# | [官方仓库 README](https://github.com/focus-creative-games/luban)、[能力清单](https://www.datable.cn/docs/reference/features) | Mac Intel 有平台声明，但仍需本项目样片；声明不能替代字节一致性。 |
| `v5.1.0` 的 `Luban.csproj` 明确 `net8.0`／版本 `5.1.0`；.NET SDK `8.0.425` 同时提供 macOS x64 与 Windows x64，含 runtime `8.0.31`；官方 release metadata 给出逐包 SHA-512 | [tagged csproj](https://raw.githubusercontent.com/focus-creative-games/luban/v5.1.0/src/Luban/Luban.csproj)、[Microsoft .NET 8.0.31 release](https://github.com/dotnet/core/blob/main/release-notes/8.0/8.0.31/8.0.31.md)、[official release.json](https://github.com/dotnet/core/blob/main/release-notes/8.0/8.0.31/release.json) | 本包锁 SDK `8.0.425`；Mac Intel 用 x64，Windows 用 x64。两端保存官方 checksum 与 `dotnet --info`。 |
| 5.x 用 `--strict` 取代 `--validationFailAsError`；失败 exit 1，并支持 `--errorFormat json` | [5.x 更新说明](https://www.datable.cn/docs/other/whats-new)、[AI 速查](https://www.datable.cn/docs/ai/cheat-sheet) | 发布命令固定 `--strict --errorFormat json`，不沿用 4.x 开关。 |
| `luban.conf` 定义 groups/schemaFiles/dataDir/targets；CSV 可从表头推断 schema | [luban.conf](https://www.datable.cn/docs/concepts/luban-conf)、[Excel Schema](https://www.datable.cn/docs/schema/excel-schema)、[加表说明](https://www.datable.cn/docs/guide/add-table) | 正式源用 CSV，schema 声明也用 CSV，避免二进制 Excel 成为必需。 |
| JSON data target 可单独生成；默认会清空 outputDataDir | [第一次生成](https://www.datable.cn/docs/guide/first-generate)、[运行时加载](https://www.datable.cn/docs/guide/load-runtime) | 输出只能指向包专属 staging，禁止指向手写源码目录。 |
| Luban 的 `text`/l10n 能校验 key；动态长文案适合文本表 | [L10N](https://www.datable.cn/docs/quality/l10n)、[validators](https://www.datable.cn/docs/quality/validators) | 本项目仍由自有 binding 做运行时语言切换；Luban 负责制作期结构和 key 校验。 |
| 主仓是 MIT License；入口项目还直接引用 CommandLineParser `2.9.1` 与 YamlDotNet.NetCore `1.0.0` | [tagged LICENSE](https://raw.githubusercontent.com/focus-creative-games/luban/v5.1.0/LICENSE)、[tagged csproj](https://raw.githubusercontent.com/focus-creative-games/luban/v5.1.0/src/Luban/Luban.csproj) | Luban 许可证和完整 archive DLL／第三方许可证清单都须归档；MIT 不能替代传递依赖审查。 |

官方事实只证明版本身份、公开能力和命令合同。005 已另外证明固定 Mac Intel
runner 对 2-row scratch 合同的最小功能与确定性；它没有证明 264-key FightMatch
生产 schema、正式生成、Unity `2022.3.18f1`、Android IL2CPP 或 Mac/Windows
生产产物一致。后者继续是 `NOT_RUN`／`NOT_VERIFIED`。

## 3. 版本与获取冻结

### 3.1 推荐版本

- Luban：`v5.1.0`。
- commit：`df34215d2e22035073d1af83d44e3ac0744dc3b3`。
- 上游：`https://github.com/focus-creative-games/luban.git`。
- 许可证：MIT，固定读取该 tag 的 `LICENSE`。
- 官方 release 资产：`Luban.7z`，1,647,739 bytes，SHA-256
  `bac9a1b8d69cfeaa7ef1d8c67b34a007324d1cd158e0b14294521b3879ba5213`。
- .NET SDK：`8.0.425`；Mac Intel=`osx-x64`，Windows=`win-x64`。

选择 5.1.0 的理由是：当前官方 5.x 文档、`--strict`、机器可读错误和 release
身份一致；不需要 AI/MCP 功能，也不把新特性引入运行时。若本地样片在 Unity
2022.3.18f1 或现有发布流程发现不兼容，只能形成版本变更提案，不能悄悄退回
4.x 或跟随新 release。

### 3.2 获取与 SHA 验证

本项目使用 005 已验证的**固定 commit 源码构建产物**，不把本机全局工具当输入。
官方 `Luban.7z` 的 size／SHA 只作上游 release 身份交叉证据；不为后续产品包
解压、安装或替换 runner。外盘缓存固定为：

```text
Mac:     /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/
Windows: D:\ApplicationData\Tools\Luban\5.1.0\
```

目录内现有 `download/`、`dotnet-x64/`、`source/`、`runner/`、`nuget/`、身份、
许可证和 inventory。005 冻结源码 HEAD
`df34215d2e22035073d1af83d44e3ac0744dc3b3`、SDK `8.0.425` `osx-x64`、
`dotnet` SHA-256 `e307c181562634fd59416032f56d506f85de5b405604dae4a20e2117e8709686`
以及 132-file runner manifest SHA-256
`d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5`。
后续正式实现直接复用该缓存与 runner，不重新下载、restore、publish 或再跑
preflight；只有固定工具身份或其相关命令合同输入发生变化时，才回主程另签新
验证包。缓存、二进制、NuGet 缓存或源码副本不得提交到 FightMatch。

## 4. 正式人工源与一次迁移

### 4.1 唯一人工编辑路径

```text
Config/FightMatch/Localization/FightMatchText.csv
```

该文件在迁移接收后成为唯一人工权威。`planning-localization-draft.csv` 留作
来源证据，不再双向编辑；生成 JSON、manifest、运行时缓存和 Prefab 都不是
人工文案权威。

正式 CSV 使用 UTF-8 无 BOM、LF、RFC 4180 引号规则，逻辑字段仍是：
`key,screen,element,state_or_trigger,zh_cn,en,parameters,severity,qa_notes`。
为让 Luban 从文件表头推断 schema，物理第一列是控制列：

```text
##var,key,screen,element,state_or_trigger,zh_cn,en,parameters,severity,qa_notes
##type,string,string,string,string,string,string,string,"string#set=blocking,error,warning,info",string
##comment,stable key,page,role,state/trigger,zh-Hans,en,named args,severity,QA note
,fm.common.action.back,...
```

`parameters` 是用 `;` 分隔、ordinal 排序的唯一名称；空集合为空字符串。
运行时 locale 名规范化为 `zh-Hans`，但沿用已接收列名 `zh_cn`，生成 manifest
必须显式记录 `zh_cn -> zh-Hans`，避免悄悄重命名源列。

### 4.2 schema 文件

```text
Config/FightMatch/Luban/luban.conf
Config/FightMatch/Localization/__tables__.csv
```

`__tables__.csv` 只声明一个 client map 表：full name `fm.TbLocalizedText`、
value type `LocalizedTextRow`、`read_schema_from_file=true`、input
`FightMatchText.csv`、index `key`、mode `map`、group `client`、output
`fm-text-v1`。`luban.conf` 只收集该 table schema，`dataDir` 指向
`Config/FightMatch/Localization`，target `client` 只含 client group；不生成
C#，不含 server/AI/MCP target。

上述 CSV 语法符合官方 5.x 模型；具体 `output` 文件名和相对路径解析仍由样片
确认。`string#set` 的值分隔已由固定 v5.1.0 source 和首次 production failure
确认必须是逗号；RFC 4180 用双引号把整个 type cell 保持为一格，因此物理 CSV
仍是含控制列的 10 列。四值集合仍精确为 `blocking,error,warning,info`。

### 4.3 COPY-001 一次迁移候选

COPY FIX-01 接受的 263-key CSV 只作为一次迁移的 UGUI 快照 provenance。其身份是
SHA-256
`d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`、
64,708 bytes、264 个物理行、9 列；RFC 4180 解析为 263 个数据记录／263 个
ordinal 唯一 key。物理行数只作字节证据，不能替代 CSV parser 的记录数。

旧 217 个物理行的 prefix SHA-256 必须仍为
`e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`。
170／171／216-key 文件只作历史 provenance，不参与合并、回退或权威选择。
FIX-01 已改变被拒绝的相关文案字节并获独立 R `ACCEPT`。主策随后明确冻结该
263-key 快照；COPY AMEND-04 只新增
`fm.language.save_unknown(errorCode)`（severity=`error`），产生记录 `+1`、unique
key `+1`、parameterized `+1`、error `+1`，其余既有记录 bytes／顺序／摘要维度
不变，并获原独立 R `ACCEPT`。最终 A 输入冻结为 SHA-256
`ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`、65,358
bytes、265 个物理行、264 个数据记录／264 个 ordinal unique key；LOC-IMPL-A
不得把上述 263-key 快照当成最终输入。输入接受仍不等于实施授权。

263-key UGUI 快照只读复算为：`info=172`、`warning=74`、`blocking=13`、
`error=4`、参数化记录 `77`；简中／英文空值、重复 key、参数合同问题均为 `0`。
AMEND-04 已复证这些 snapshot 指标并只产生上一段规定的 `+1` delta；接受后的
实测为 `info=172`、`warning=74`、`blocking=13`、`error=5`、264 unique key、
parameter mismatch=`0`。正式源接收后，后续文案变更只走
`Config/FightMatch/Localization/FightMatchText.csv` 的主策审查；不得由 draft
覆盖或编辑生成 JSON 反向写回。

### 4.4 LOC-IMPL-A `string#set` 技术勘误

本勘误起点是当前接受设计 SHA-256
`43f98e353b20eb583df8154fe604dd73f32b281378aefade22382916975fe1fc`，743 lines /
50,126 bytes。中央已授权普通制作工具语法纠错不再重复走整体设计前置 R；由主程
签发给同一执行者纠正，最终实际 patch、正例／负例证据仍交原 R 独立审查。

首次 production run 的 immutable failure root 是
`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/`：
`run.json` SHA-256
`e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`，终态／诊断均为
`FMLOC013`；`inputs.json` SHA-256
`07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2`，其中 accepted
COPY SHA 是 `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，
失败 run 的 formal source SHA 是
`9695b4501b9d6ffda53914d0a42f61948147e2bf655385fa263e7bf916348e39`。该 root 必须
保持 immutable，不得覆盖、续写或当成成功证据。

固定本机官方 source
`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source` 的
HEAD=`df34215d2e22035073d1af83d44e3ac0744dc3b3`、exact tag=`v5.1.0`；
`src/Luban.DataValidator.Builtin/Set/StringDataSet.cs:26-29` 以
`args.Split(',').ToHashSet()` 解析 set 参数。这与 `FMLOC013` 共同确认 §4.1 的逗号
分隔勘误；不改变四值集合、264 records、输出合同或 project-owned 强校验。

corrective run 必须使用全新 create-once root
`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/`。
不得重跑 005、download、restore 或 publish；必须重新计算 formal source SHA，并完整
执行 pass-1／pass-2、canonical、manifest、negative、license 与 diff-check 门。

## 5. 生成命令、工作目录与输出

### 5.1 仓库内统一入口

当前 Mac 没有已验证的 PowerShell 7，因此不新增该工具依赖。Mac Intel 与
Windows 都从仓库根使用同一个 project-owned .NET CLI。下列内容是平台中立的
结构化 invocation，不是 shell 命令，也没有 PowerShell continuation：

```json
{
  "fileName": "<absolute-dotnet-8.0.425-x64>",
  "argumentList": ["<OutputRoot>/compiler/FightMatch.Localization.Compiler.dll", "generate", "--luban-dll", "<external-cache>/5.1.0/runner/Luban.dll", "--expected-tool-sha256", "6e60beee2b1e8e2870278e6ec363b4dc516311a6f2711bf3518b5edae056df7e", "--expected-commit", "df34215d2e22035073d1af83d44e3ac0744dc3b3", "--expected-sdk", "8.0.425", "--output-root", "TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01"]
}
```

上述身份分别取自仓库内已接收 005 证据：SDK version `8.0.425` 的唯一证据源是
`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/input-hashes.json`
的 `sdkArchive.version`；source commit、dotnet 可执行文件、runner 闭集和 Luban DLL
则取
`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/tool-identity.json`
的 `sourceHead`、`dotnetExecutableSha256`、
`runner.fileCount`／`runner.manifestPath`／`runner.manifestSha256` 和
`runner.criticalFiles["Luban.dll"]`。不得声称 `tool-identity.json` 含 SDK version。
A 必须先逐字段匹配；不得接受抽象 `identity.json`、当前机器重新推断值或仅文件名
相同。

LOC-IMPL-A 用 005 已证明的固定 SDK Roslyn `csc.dll` 路径直接编译 project
compiler 到本次 evidence root；不得 restore、publish 或写仓库 `bin/obj`。
`.csproj`／`global.json` 只冻结工程身份与 SDK，不授权联网解析。CLI 必须用
`ProcessStartInfo` 启动 Luban，不经 shell 字符串拼接。其唯一 Luban 生成调用冻结为
同一个平台中立 invocation；Mac／Windows 只替换绝对 dotnet 路径和
`<external-cache>` 根，除此之外 token 的值和顺序完全相同：

```json
{
  "fileName": "<absolute-dotnet-8.0.425-x64>",
  "argumentList": ["<external-cache>/5.1.0/runner/Luban.dll", "--conf", "Config/FightMatch/Luban/luban.conf", "-t", "client", "-d", "json", "--strict", "--errorFormat", "json", "-l", "<external-cache>/5.1.0/runner/nlog.xml", "-x", "outputDataDir=<OutputRoot>/raw"]
}
```

本节三处调用统一按完整 process argv 解释：`fileName`／dotnet executable 就是
argv[0]，赋给 `ProcessStartInfo.FileName`；完整 argv[1..] 才逐项加入
`ProcessStartInfo.ArgumentList`。在结构化 JSON 的 `argumentList` 数组内部，index 0
分别是统一 CLI 的 compiler DLL、CLI 启 Luban 的 `Luban.dll`、compile 调用的
compiler DLL。

输出目录是一次性空 staging；脚本拒绝仓库根、`Assets/Scripts`、人工 CSV
目录、非空目录、symlink 或越界路径。Luban 默认清目录的行为不能接触手写
文件。

### 5.2 project-owned 编译产物

随后调用固定 .NET 工具；同样把 executable 与 arguments 分开：

```json
{
  "fileName": "<absolute-dotnet-8.0.425-x64>",
  "argumentList": ["<OutputRoot>/compiler/FightMatch.Localization.Compiler.dll", "compile", "--source", "Config/FightMatch/Localization/FightMatchText.csv", "--luban-json", "<OutputRoot>/raw/<verified-luban-output>.json", "--artifact", "<OutputRoot>/publish/fm-text-v1.json", "--manifest", "<OutputRoot>/publish/fm-text-v1.manifest.json"]
}
```

此调用遵循同一映射：完整 process argv[0] 是赋给 `ProcessStartInfo.FileName` 的
dotnet executable；结构化 JSON `argumentList` 的 index 0 是 compiler DLL，对应
完整 process argv[1]，其余 tokens 依次后移并逐项加入
`ProcessStartInfo.ArgumentList`。

raw Luban JSON 输入参数在该调用中精确出现一次；不得用重复参数依赖解析器的
first/last value 行为。

`ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`
只标识 R-accepted COPY draft/input，**不是**正式
`Config/FightMatch/Localization/FightMatchText.csv` 的 SHA。A 必须把 accepted COPY
的 264 个数据记录逐字迁移，并添加 §4.1 已冻结的 3 条 Luban control rows，形成正式
source；随后对该正式文件完整 bytes 独立求 `formalSourceSha256`。该值只能由 A
evidence 产生，本设计不得预填，也不得把 accepted COPY SHA 复用为 formal source
SHA。

compiler 不改文案，只核 Luban 输出与正式源一一对应并写 canonical artifact：
UTF-8 无 BOM、LF、固定 property 顺序、row 按 key ordinal、locale 固定
`en,zh-Hans`、参数集合排序、severity 枚举化。发布 manifest 至少含
`schemaVersion`、平台中立的 Luban tag/full commit 与 SDK version、
`acceptedCopySha256=ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`、
A 实测的 `formalSourceSha256`、canonical artifact SHA、RFC 4180
record/unique-key count，以及 locale 映射、parameter/severity 摘要和 canonical
semantic digest。`acceptedCopySha256` 与 `formalSourceSha256` 是两个确定、独立、
不可混名或复用的字段；两者都进入 canonical publish manifest。
发布 manifest 的每个字段都必须能在 Mac／Windows 产生 byte-identical JSON；因此
**不得**包含 host/OS、时间、绝对路径、随机 ID、raw 文件名／文件集合／bytes／SHA、
dotnet 可执行文件 SHA、runner binary/file-manifest SHA 或其它平台 presentation
identity。上述运行身份和 raw 事实只进每次运行 evidence：`run.json`、
`pass-1/raw-manifest.tsv`、`pass-2/raw-manifest.tsv` 与 `comparison.json`。

正式候选经 review 接收后，只有以下两个生成文件进入资源发布输入：

```text
Generated/FightMatch/Localization/fm-text-v1.json
Generated/FightMatch/Localization/fm-text-v1.manifest.json
```

它们是机器生成、禁止手改的候选，不直接放进 `Assets/StreamingAssets`；后续
`RES-01` 决定 built-in／远端打包并把 artifact 身份绑定 ReleaseSet。

## 6. 校验合同

### 6.1 schema/内容门

任一条件失败均 exit 非零且不替换已接收生成物：

1. COPY FIX-01 R-accepted 263-key 身份只作 UGUI 快照 provenance。LOC-IMPL-A
   必须使用 COPY AMEND-04 独立 R `ACCEPT` 的唯一 SHA
   `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`、65,358
   bytes、265 个物理行、264 records／unique keys 与接受摘要，并由 RFC 4180
   parser 复证；AMEND-04 只允许在快照后追加
   `fm.language.save_unknown(errorCode)`。旧 `0228ca7…` 和 FIX-01 263-key 快照都不得
   作为最终 A 输入。
   A 正式 source 另由这 264 条 accepted COPY data records 加 3 条冻结 control rows
   构成；必须对完整正式 bytes 计算新的 `formalSourceSha256`，不得等同或回填为
   `acceptedCopySha256`。
2. `screen/element/state_or_trigger/zh_cn/en/severity/qa_notes` 非空；severity
   仅 `blocking/error/warning/info`。
3. 两语言 `{name}` 占位符集合逐字相同，且与 `parameters` 双向相等；拒绝
   重复、空名、位置参数、未闭合括号和未声明变量。
4. `fm.language.save_failed` 与 `fm.language.save_unknown` 必须同时存在，参数都仅
   `errorCode`、severity 都为 `error`；两者不可互换，也不得映射到业务保存错误。
5. `fm.diagnostic.placeholder` 两语言必须逐字为
   `【if you see this, it is a bug.】` 且 severity=`blocking`。
6. `zh_cn -> zh-Hans` 和 `en -> en` 映射唯一；不允许隐式 fallback。
7. 数字／物品／角色／关卡显示只接受具名参数或稳定 display-name key；拒绝
   枚举 `ToString()` 契约和拼接模板。
8. raw Luban JSON 每条与正式 CSV 字段相同；canonical artifact round-trip
   后 key/text/args/severity 无丢失。
9. `parameters` 必须是去重、ordinal 排序的具名参数；两语言模板参数集合与声明
   三方完全相等。AMEND-03 的 `operationName`、`phaseName`、`enemyName`、
   `errorCode` 等只能来自显式项目映射或稳定玩家码，不能传异常／枚举原值。
10. 玩家文本字段禁止出现 `OperationId`、`CommitId`、`FaceId`、`PairId`、
    `TargetKind`、内部版本号或 enum 原值；QA 备注可以记录这些禁止规则，但不得
    被编译为玩家文本。synthetic-only 名称和未发布内容保持不可见／不可启用。

### 6.2 确定性门

- 当前 Mac 门：同一 Mac Intel、同一空目录策略连续两次生成，raw 文件集合、
  相对路径、bytes 和 SHA 必须分别相同；这些事实分别写入
  `pass-1/raw-manifest.tsv`、`pass-2/raw-manifest.tsv` 和 `comparison.json`，失败即
  `NON_DETERMINISTIC`，不接收 LOC-IMPL-A。
- Mac 产品实现后，project-owned compiler 对 raw 做严格解析并生成 canonical
  artifact；同一输入连续两次的 canonical JSON／manifest 必须逐字节相同。
- 后续 Windows 门：使用同一 Luban full commit／runner build recipe、SDK
  `8.0.425`、schema 和同一 formal source bytes。Mac／Windows 的 raw 相对路径与
  file set 必须完全一致；任何文件名、相对路径、文件数量或拆分／合并差异都立即
  阻断。跨主机只允许同一相对路径文件的 presentation bytes／SHA 不同，各平台必须
  在自己的 `raw-manifest.tsv` 与 `comparison.json` 中如实记录，不能写进发布
  manifest。两端严格解析出的完整 record 语义树和 canonical semantic digest 必须
  相同；canonical JSON 与发布 manifest 必须逐字节相同。
- 同一相对路径的 raw 若仅换行、JSON property 顺序等非语义 presentation 差异，
  必须在 evidence 中列出，并由已审 normalizer 得到同一 canonical bytes；file set／
  相对路径、key、值、参数、severity、record 数或完整语义树任一差异都阻断跨平台
  接收，不能用 normalizer 掩盖。
- 发布 manifest 不含 host、时间、绝对路径、raw identity 或 runner binary identity；
  每次运行的 `run.json` 承载 host/tool identity，`raw-manifest.tsv` 承载 raw
  presentation identity，`comparison.json` 承载两次运行与跨平台比较结论。
- third run 不允许作为“多跑几次挑相同”；第一次不一致即保留两份 evidence，
  返回 `NON_DETERMINISTIC`。

### 6.3 第三方许可证门

A 不从 README、Luban 主仓 MIT 声明或人工记忆推断传递依赖许可证。唯一清点源是
现有固定 inventory：已接收
`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/tool-identity.json`、
其 `runner.manifestPath` 指向的
`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/identity/runner-files.tsv`、
外盘 `runner/Luban.deps.json`、`identity/packages.lock.json`、`source/LICENSE`，以及
外盘 `nuget/<package>/<version>/` 中已存在的 `.nupkg` 和对应 license／notice 文件。
禁止下载或联网补齐缺项；`.nupkg` 内的许可证入口必须用
`<nupkg-relative-path>!/<entry-path>` 精确标识并对 entry bytes 单独求 SHA-256。

A evidence 必须生成精确文件 `third-party-license-inventory.json`，并记录绑定的
`runner.fileCount=132` 与
`runner.manifestSha256=d9baacda3afc7fee9db4d92cd4199587fa5b9f1b7e4601be85028644170f2bd5`。
清单对每个实际 runner DLL／NuGet dependency 至少输出 dependency name、version、
deployed file relative path＋SHA-256、NuGet/package identity、license/notice source
relative path（或 nupkg entry）＋SHA-256，以及 `complete` 布尔值和缺失理由。每个
132-file runner manifest 中的 executable dependency 都必须映射到 Luban 自有许可
或一个完整第三方条目；非 DLL 的 pdb/config/resource 也须被归属或明确标成
non-license-bearing companion，不能静默遗漏。

任一实际 DLL 无来源、package/version 不明、文件 SHA 不匹配、license/notice 缺失
或清单未覆盖固定 runner 时，A 终态必须为 `LICENSE_INVENTORY_INCOMPLETE`；这同时
阻断 A 接收、B 和 RES。不得下载许可证补洞，也不得用 Luban MIT 覆盖传递依赖。
该 JSON 是 A 的 per-run evidence，不是生产仓库或发布 manifest 文件。

## 7. 运行时合同、依赖与发布

### 7.1 无 Luban 运行时依赖

本包选择 data-only JSON，不生成 C#，Unity `Packages/manifest.json` 与
`packages-lock.json` 都不变，也不安装 `com.code-philosophy.luban`。运行时
由 Host adapter 用项目 DTO 解析 canonical artifact，再实现 UGUI-01 的
`ILocalizedTextSource`；Luban 类型、Tables、SimpleJSON、dll 不进入玩家包，
也不进入 Core/Application/Save/Presentation public API。

### 7.2 Locale preference

按系统设计实现独立文件
`Application.persistentDataPath/FightMatch/settings/locale-preference-v1.json`：
missing→系统语言；合法值→手选；损坏／未知版本／读失败保留原文件并仅本次
回退系统语言；写临时文件、Flush、pre-verify、原子替换、替换后重新打开／读取／
解析／逐字段比较全部成功才显示 saved。temp write、Flush、pre-verify 或
commit-before-replace 失败时 replace 尚未发生，这些步骤不修改 target，因此一律
显示 `fm.language.save_failed(errorCode)`；可以记录 cleanup／旧 target 读取诊断，
但随后无法读取旧 target 也不得升级为 UNKNOWN。原子 replace 一旦发生，持久状态
只有后验复读验证成功才可判为新值；无法打开、读取、解析或比较不一致均为
`UNKNOWN`，显示 `fm.language.save_unknown(errorCode)`，不得宣称旧文件保留或
“未保存”。只有 backup rollback 成功且再次复读逐 byte 证明旧值已恢复，才可把
结果从 `UNKNOWN` 降为 `save_failed`；否则继续 UNKNOWN。

`save_unknown` 的合法触发严格限于 replace 已发生后的后验验证失败／mismatch，
以及 replace 后 rollback 无法同时满足“rollback 成功”与“复读逐 byte 证明旧
bytes／absence 已恢复”的情形；pre-replace 阶段永不使用该 key。

无论 `save_failed` 或 `UNKNOWN`，`LocalizationService.CurrentLocale` 都保持用户
本次新选择，当前 session 继续使用新语言。Retry 必须对同一个 desired locale 做
幂等 preference 写入，不重复 locale 改变／重绑事件；不得转入 PlayerSave
recovery，不创建／复用 OperationId，不使用业务保存的 unknown 结果，也不得改
PlayerSave、SaveEnvelope 或云档。

### 7.3 进入审查／不可变发布

LOC 只产 candidate artifact＋manifest。主策核文案，主程核 schema/adapter，
QA 核 AMEND-04 R-accepted key 集／参数／双语／字形集合，R 给独立 verdict；
全部接受后才将二者交
`RES-01`。ReleaseSet manifest 必须记录 canonical SHA 和 schemaVersion；文本
修订不改战斗 ContentBinding 或玩家保存。当前六件业务发布物仍走既有
`PublishedContentCatalog`，本包不把文本冒充第七件业务 definition。

## 8. 已接收 Mac 前置证据：`LOC-TOOL-01-PREFLIGHT/005`

### 8.1 身份与历史不可改写

005 evidence root 是
`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/`，终态
`PASS_MAC_PREFLIGHT`，并获主程 `ACCEPT`。固定收件身份：

- `run.json` SHA-256
  `ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232`；
- `commands.json` SHA-256
  `f098f03407019eae78340c8240a59ea5e8f20fe0de1a442f8101d597397a8688`；
- `harness-preflight/result.json` SHA-256
  `ba95e41f9339c9e994ae9a6de4fd9ebbe4ba2a1d122bee1a33eaa1aa82f9ff6d`；
- `comparison.json` SHA-256
  `23fc553155ac9719767d6ca77acd4e517c222e8fe486d1f8dd372ce48e56c5cd`；
- `cleanup-receipt.json` SHA-256
  `79461c4424dda1cfb3a1769f1f215a2537e02cee23c5aa1cfab33dc7b7edc60f`；
- `evidence-files.tsv` 为 161 项，SHA-256
  `ea361c90a7c6ee4747ea9548851edd4aec7c7c3b59c3b064e810aab5060e6c64`，
  逐文件 mismatch=`0`。

SDK archive SHA-512 完整值保持为
`3b27552358905f1bde600dd0315a23051ef1cbd055a50eb64c675ee25a04e658c2ed735177125436c5bec38dfcd958fee338299611348299cd34a9bea8b8011f`；
Luban 源码身份保持 `v5.1.0` / full commit
`df34215d2e22035073d1af83d44e3ac0744dc3b3`。

005 不重写旧失败：001 `run.json`
`86392759f7d8ed448c8dfd832b734acff69374be878a1555143c835621b10091`
仍为 `RESTORE_OR_BUILD`；002／003／004 `run.json` 分别为
`e46941883b9c1ba2d44e2f00565a661703ebe2500368d8bd14309239b12031f6`、
`91f1dc182c366104e747f8504233d8934aecf440fa338f8e3bbb4424231cc393`、
`438fdb2c65b711c035fe7ccf5aca32593d3dfdbf724bac1e5fe99e14a87c0c2c`，
终态都保持 `SCRATCH_CONFIG_FAILURE`。它们分别记录旧 help 合同误分类、wrapper/
receipt 缺陷、argv evidence harness 缺陷和额外 Python bytecode cache 检查越界；
005 是新证据链，不是对 001～004 的追认或改写。

精确 v5.1.0 源码 `src/Luban/Program.cs:299-304` 将解析结果 `NotParsed` 写 help
后调用 `Environment.Exit(1)`；`--help` 因此 exit 1。help exit 0 不是 readiness
gate，未来不得再运行 help。001 的记录必须继续保持当时的 `RESTORE_OR_BUILD`
分类，不能用后来认识回写历史。

### 8.2 005 实际证明的最小功能

- 统一 `record_step` 同时通过复杂 argv 的成功路径和保留 stdout/stderr 的预期
  exit 7 路径；参数不经 `eval` 或 shell 重解析。
- zsh 闭集语法 `12/12`，Python 内存 AST `5/5`；未创建越界 pycache。
- 固定 2-row scratch 正样片用全新目录运行两次；canonicalizer 自检 `12/12`。
- duplicate 由 Luban `--strict` 以 exit 1 拦截；parameters 与 severity 负例被
  Luban 放行，随后 project canonicalizer 分别以 `FMLOC007`／`FMLOC004`、exit 20
  拦截。Luban strict **不能独立覆盖项目规则**，canonicalizer 是必需发布门。
- 三个负例的 published canonical count=`0`；正样片 raw 文件集／bytes／SHA、
  canonical bytes 与 semantic digest 两次一致。
- `commands.json` 保存 15 个命令的 JSON argv 与真实 exit code；005 evidence
  manifest 逐文件校验通过，外盘 cache 外新增残留=`0`，结束时匹配的 Luban／
  dotnet 子进程=`0`。

这些证据只覆盖 2-row scratch、最小 schema、runner、记录器和 canonicalizer
原型，不是 263-key UGUI 快照或当前 264-key 最终 COPY 输入的生产生成、Config、
project CLI、Unity compile、Android 或发布证据。

### 8.3 复用与后续门

LOC-IMPL-A 直接复用同一外盘 cache、runner、SDK 及 005 证据；使用同样的参数
安全 recorder 语义，并在 project .NET 实现中以 `ProcessStartInfo.ArgumentList`
传参。禁止重新安装、下载、restore、publish 或重跑 preflight，除非固定工具
身份或相关命令合同输入发生变化并由主程另签精确包。

Windows 仍为 `NOT_VERIFIED / WAITING_FOR_WINDOWS_HOST`。COPY AMEND-04 输入门已
满足，不再阻断 Mac 侧 A/B 模块候选工作；但 Windows 使用同 commit、SDK patch、
AMEND-04 R-accepted COPY bytes、3 条冻结 control rows、由此得到的同一 formal
source bytes 和 project canonicalizer 的生成／语义对比通过前，不得宣称跨平台或
发行就绪。

## 9. 下一阶段拆包（当前不授权实施）

COPY FIX-01 已由独立 R `ACCEPT`，只冻结 263-key UGUI 快照。COPY AMEND-04 已只
追加 `fm.language.save_unknown(errorCode)` 并获原独立 R `ACCEPT`；最终 A 输入
冻结为 SHA-256
`ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`、65,358
bytes、265 个物理行、264 records／unique keys。本设计现在是
`READY_FOR_INDEPENDENT_REVIEW`，但 LOC-IMPL-A/B 仍为 `NOT_AUTHORIZED`。只有本
设计获同一 R `ACCEPT`，且主程把上述 AMEND-04 身份、摘要和本设计接受 SHA 写入
精确 A 任务包后，才可启动 A；A 独立 `ACCEPT` 后才可签 B。Windows 不作为 A/B
的 Mac 模块候选前置，但继续阻断跨平台与最终发行声明。

### 9.1 LOC-IMPL-A — 正式源、Config、project compiler、非 Unity 生成

唯一生产写白名单：

```text
Config/FightMatch/Luban/luban.conf
Config/FightMatch/Localization/__tables__.csv
Config/FightMatch/Localization/FightMatchText.csv
Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj
Tools/FightMatch.Localization.Compiler/Program.cs
Tools/FightMatch.Localization.Compiler/global.json
Generated/FightMatch/Localization/fm-text-v1.json
Generated/FightMatch/Localization/fm-text-v1.manifest.json
```

证据只写新根
`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/`；
执行包必须在运行
前逐项列出 `inputs.json`、`commands.json`、`run.json`、
`pass-1/raw-manifest.tsv`、`pass-2/raw-manifest.tsv`、两次 canonical/publish
manifest、`comparison.json`、`third-party-license-inventory.json`、self-test、
negative-case 和 diff-check 文件，不能用未展开通配符增加其它输出。
`Config/**`、`Tools/**`、`Generated/**` 位于 `Assets` 外，不创建 `.meta`。

A 的只读输入是 COPY AMEND-04 R 接受字节（SHA-256
`ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，65,358
bytes、265 个物理行、264 records／unique keys）、005 的外盘 cache/runner/SDK
和证据、冻结架构/系统/UGUI 设计。它必须：

1. 原样迁移 AMEND-04 R-accepted 264 条 data records 并添加 3 条冻结 Luban control
   rows，证明其 `acceptedCopySha256`、263-key UGUI 快照不变、AMEND-04 只新增
   `fm.language.save_unknown(errorCode)`、9 逻辑列、接受的 severity／参数化摘要和
   双语参数合同；对形成的正式 source 完整 bytes 独立计算 `formalSourceSha256`，
   不得与 accepted COPY SHA 混名或复用；
2. 以与 005 相同的参数安全 recorder 语义记录每步，在 project CLI 中只用
   `ProcessStartInfo.ArgumentList` 启动固定 runner；
3. 连续两个空 staging 做正式生成，要求 raw 相对路径/file set 完全一致且逐文件
   bytes/SHA 相同，并写入各自 per-run evidence；再由 project canonicalizer 生成并
   比较 canonical JSON／发布 manifest／semantic digest；确认发布 manifest 分别记录
   `acceptedCopySha256` 与实测 `formalSourceSha256`，且不含任何 raw 或 host identity；
4. 运行 12 项 compiler/canonicalizer 合同和 duplicate/parameters/severity
   负例；duplicate 可由 Luban strict 拦截，但 parameters/severity 必须分别由
   project gate `FMLOC007`／`FMLOC004` 拦截且 published canonical=`0`；
5. 按 §6.3 生成 `third-party-license-inventory.json` 并完整覆盖固定 132-file runner；
   任一缺项以 `LICENSE_INVENTORY_INCOMPLETE` 停止；
6. 只在所有门通过后把确定的两个 Generated 文件作为候选交独立 R。

A 禁止修改 `Assets/**`、`.meta`、`Packages/**`、`ProjectSettings/**`、其他文档、
旧 evidence 或 Git；禁止运行 Unity、下载、安装、restore、publish、重跑 preflight、
接 Host 或 RES。任一输入／生成／canonicalizer／确定性门失败立即停止，保留新
evidence，不运行依赖步骤。回滚只恢复 A 白名单内原有文件的 preflight bytes，
并移除本包新建但未接收的候选；不得手改 Generated、清已验证外盘 cache 或重置
其它工作。

### 9.2 LOC-IMPL-B — bounded parser/adapter、locale preference 与聚焦验证

B 只在 A 获独立 `ACCEPT` 后，由现有串行 Unity owner C 执行。B 不修改任何
既有生产文件；尤其不修改 `FightMatchPlayerHost.cs`，不虚构 repository-relative
文件路径作为 Player build 字节来源，也不做 Host composition。只允许新建：

```text
Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs
Assets/Scripts/FightMatch/Host/FightMatchLocalizationCatalogParser.cs.meta
Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs
Assets/Scripts/FightMatch/Host/FightMatchLocalizedTextSourceAdapter.cs.meta
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs
Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta
Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs
Assets/Tests/EditMode/FightMatchHost/FightMatchLocalizationCatalogTests.cs.meta
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs
Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta
```

上述每个精确路径只出现一次；locale preference interface 的 `.meta` 没有重复项。

B 的 `Config/**`、`Tools/**` 和两个接受的 Generated 文件均只读。证据只写新根
`TestArtifacts/FightMatch/LOC-IMPL-B/<accepted-a-sha>/`，其精确文件清单由 B 包
在执行前冻结。禁止修改 Core/Application/Save/Platform/Content/Input、public API、
业务序列化、asmdef、场景、Prefab、字体、StreamingAssets、YooAsset、Packages、
ProjectSettings、其他文档、旧 evidence 或 Git；也不创建 Unity Localization／
Addressables。若冻结 Host interface 或路径不匹配，返回
`BLOCKED_INTERFACE_MISMATCH`，不得复制接口、反射或扩大 public surface。

B 只交付 synchronous bounded canonical catalog parser、满足现有
`ILocalizedTextSource` 的 adapter，以及 locale preference store。parser 输入由
测试／未来调用方注入已受验的 bytes 或 stream；B 不定位、不打开、不下载
production artifact，也不拥有 transport。Luban 类型、runner、第三方 DTO 和生成
C# 不进玩家包。新增 Host 类型保持 `internal` 并使用既有 friend。locale
preference schema v1 与 artifact schema v1 都不改变 PlayerSave、SaveEnvelope
或业务恢复语义。

production artifact 装载、outer ReleaseSet SHA/admission 及 Host composition 均为
`NOT_RUN`。它们必须等已接受 RES-01D 的 `fm.text.full` raw-byte bridge 提供精确
ReleaseSet 身份和受验 bytes，再由后续明确白名单接线；B 不提前完成或模拟该门。

### 9.3 B parser 输入所有权、预算与失败语义

- 调用方拥有输入。对于 memory，调用方保证在 synchronous parse 返回前不改写；
  parser 返回后不保留其引用。对于 stream，调用方负责创建和 Dispose，parser
  不关闭它，只通过 bounded reader 消费并拒绝超限或 trailing bytes。
- B 任务包在执行前必须冻结非零有限的 `maxArtifactBytes`、`maxRecords`、
  `maxKeyChars`、`maxTextCharsPerLocale`、`maxParametersPerRecord`、
  `maxParameterNameChars`、`maxTotalDecodedChars` 和 JSON `maxDepth`。数值依据 A
  接受 manifest 的实测最大值加明确余量制定，并写入生产常量与测试；实现者不得
  临场放宽。`maxRecords` 不得低于 AMEND-04 接受的最终记录数，也不得以无界
  `int.MaxValue` 代替。
- 在分配完整对象图前先检查 byte limit；每读取一个 token/record/text/parameter
  都递增对应预算。长度未知的 stream 最多读取 `maxArtifactBytes + 1` 以判超限，
  不能 `ReadToEnd` 到无界缓冲。
- 任一 SHA/schema/version/count/duplicate/key/text/parameter/JSON/UTF-8/预算错误均
  fail closed：返回无 catalog、无部分 `ILocalizedTextSource`、无当前 source 替换。
  对外只返回稳定 project diagnostic code、stage 和 retryability；异常类型、堆栈、
  原始 message、路径和任意输入片段只进受限日志，绝不进入玩家 UI。
- 测试至少覆盖每个上限的 `limit-1/limit/limit+1`、截断 JSON、额外 trailing
  token、坏 UTF-8、重复 key、参数爆炸、深度超限、stream 中途失败，以及失败后
  零 retained input／零 partial publication。

### 9.4 A/B 验证、失败停止与 Windows 门

- A 门：COPY AMEND-04 R-accepted identity、whitelist、正式 compiler 合同、两次
  raw/canonical/manifest 确定性、第三方许可证完整性、负例和独立 R；任何失败
  停止 B。
- B 门：静态 whitelist/input/artifact hash → Unity compile → 聚焦 EditMode →
  独立 R。catalog fixture 必须覆盖 AMEND-04 接受的 key/locale 集、severity 分布、
  参数化记录、AMEND-03 内部 ID 禁止、COPY/formal identity、placeholder、缺 key/参数/
  glyph、canonical round-trip、binding failure、§9.3 ownership/budget/fail-closed。
  preference fixture 覆盖 §10 的全部故障语义。
- 旧的精确聚焦／全量总数是历史输入时期的未执行预测，作废。B 包必须在实际
  接受的 UGUI fullname 基线上先枚举新增 test case，再冻结聚焦 filter 和准确
  总数；不得从文件数或 parameterized method 数推算。全量只在代码稳定且主测试
  授权后运行，并对最新接受 fullname Counter 做差异解释。
- B 失败只在白名单内修复，先定向验证失败 fixture，最终再跑一次冻结聚焦集；
  rollback 只移除本包新文件／meta，不动 PlayerHost、A 产物、旧 COPY、玩家资料、
  ReleaseSet 或其它 WIP。
- Windows 主机就绪后，以同 commit、SDK patch、accepted COPY bytes、3 条冻结
  control rows、由此得到的同一 formal source bytes／`formalSourceSha256` 和 project
  canonicalizer 各生成两次。每个主机内部两次都要求 raw 相对路径/file set 及每个
  文件 bytes/SHA 相同；Mac／Windows 之间也要求 raw 相对路径/file set 完全一致，
  任何文件名、路径、数量或拆分／合并差异都失败。跨平台只允许同一相对路径文件
  的 property 顺序、LF/CRLF 等 presentation bytes/SHA 不同，且必须写入 per-run
  raw manifest 和 comparison，并证明 AMEND-04 accepted 完整语义树、canonical
  JSON、发布 manifest、semantic digest 一致。任何 key/text/参数/severity/record
  语义差异都失败。真实证据前保持 `NOT_VERIFIED`。

## 10. Locale preference 的精确故障语义

用户选择新语言时，先更新 session `CurrentLocale` 并触发一次重绑，再尝试写
独立 preference。保存状态机按原子 replace 是否已成功分界：

1. 写 temp、Flush、temp pre-verify 或 commit-before-replace 任一阶段失败时，atomic
   replace 尚未发生，target 未被这些步骤修改，因此一律返回 `save_failed` 并显示
   `fm.language.save_failed(errorCode)`。实现仍应尽力清理 temp 并记录旧 target
   read-back 诊断，但随后无法读取、解析或证明旧 target bytes／absence 不得把该
   pre-replace 结果升级为 `UNKNOWN`。
2. 原子 replace 成功后必须重新打开目标文件、读取、解析并逐字段比较 schema/
   desired locale。全部成功才返回 saved 并显示 `fm.language.saved`；任一 open/read/
   parse/compare 失败或 mismatch 都返回 `UNKNOWN`，显示
   `fm.language.save_unknown(errorCode)`。此时不得声称旧文件保留或“未保存”。
3. replace 后可以尝试从 pre-write backup rollback，但只有 rollback 成功且再次
   reopen/read 逐 byte 证明旧 bytes（或旧 absence）恢复，才可从 `UNKNOWN` 降为
   `save_failed`；rollback 失败、不可验证或 bytes 不同都保持 UNKNOWN。
4. 当前 session **始终继续使用新语言**，绝不因持久化结果回滚
   `CurrentLocale`。Retry 对同一 desired locale 幂等重做完整 preference 流程；
   成功后才显示 saved，且不重复 locale 改变、重绑或业务事件。
5. `UNKNOWN` 是 locale preference 专属持久化状态，合法触发只限第 2 项和第 3 项的
   replace 后情形，只能使用 `fm.language.save_unknown(errorCode)`；不得复用
   PlayerSave／业务保存的 unknown、SaveFailed Retry、OperationId、恢复或云档逻辑。
   `save_failed` 与 UNKNOWN 都不改 PlayerSave、SaveEnvelope 或业务状态。

读失败／坏文件／未知版本仍保留原文件，仅本次从系统语言初始化；也不得写回
“修复”。此语义由 LOC-IMPL-B 在执行前枚举并冻结的 preference fixture 覆盖；
至少分别覆盖每个 pre-replace 失败点恒为 `save_failed`（包括随后 read-back 失败）、
replace 后 reopen/read/parse/mismatch 的 UNKNOWN、rollback 已证明／未证明两支、
同 desired locale 幂等 retry 和 session locale 保持；本设计不沿用旧 8-case 数量
预测。

## 11. 回滚、接收与结论

- 工具/tag/SDK/依赖身份不符：停止并保留 evidence；不换浮动版本。
- COPY FIX-01 已由独立 R `ACCEPT`，只作 263-key UGUI 快照 provenance；COPY
  AMEND-04 也已获原独立 R `ACCEPT`，此前的 `BLOCKED_ON_COPY_AMEND_04` 条件已
  满足。若实施前 AMEND-04 identity 漂移，立即返回 `BLOCKED_INPUT_DRIFT` 并重新走
  COPY 评审。schema/CSV/template 失败时只在对应责任方修订并重新评审；禁止手改
  Generated JSON。
- 005 已接收且复用，不再清理、重跑或重建。A/B 回滚分别只处理各自精确白名单
  与未接收 candidate，不改历史 COPY、玩家资料、旧 ReleaseSet、Git 历史或已
  核验外盘 cache。
- adapter/Unity 失败时 Generated 仅作 evidence，不交 RES；任何 out-of-scope
  文件需求都返回主程，不扩大白名单。
- 本设计 r3 获同一独立 R `ACCEPT`，且主程把许可证 inventory gate 与冻结身份写入
  精确 A 包后，才可另签 A；A 独立 `ACCEPT` 后才可签 B。A+B `ACCEPT`
  只证明本地化 artifact、bounded parser、
  `ILocalizedTextSource` adapter 与 locale preference 模块候选，不证明 PlayerHost
  已接线、Android Demo 已呈现或 runtime 已闭合。后者仍需 RES-01D
  `fm.text.full` transport／outer admission／Host composition、Unity 与设备证据。
  Windows、RES、APK、设备和最终发行均未关闭。

本次 r3 只读复核既有 005 和当前 COPY 候选，并只修改本文。没有重新下载、安装、
restore、publish、运行 preflight／Luban／Unity、生成正式 Config/Tools/artifact，
也没有修改产品、Packages、ProjectSettings、Assets、`.meta`、Git 或云服务。
005 保持 `PASS_MAC_PREFLIGHT`，COPY FIX-01 的 263-key UGUI 快照保持独立 R
`ACCEPT`；COPY AMEND-04 已获原独立 R `ACCEPT`，最终 A 输入身份已冻结。A、B、
正式生成、Unity、RES 接线和 Android 均为 `NOT_RUN`，Windows 为
`NOT_VERIFIED`。

**LOC-TOOL-01-DESIGN r3 结论：READY_FOR_INDEPENDENT_REVIEW。263-key COPY
FIX-01 只作 UGUI 快照 provenance；COPY AMEND-04 已获原独立 R `ACCEPT` 并成为
最终 A 输入。本设计仍须同一 R `ACCEPT`，之后主程另签精确 LOC-IMPL-A 包；当前
A/B 仍为 `NOT_AUTHORIZED`，本设计不是实施授权。**
