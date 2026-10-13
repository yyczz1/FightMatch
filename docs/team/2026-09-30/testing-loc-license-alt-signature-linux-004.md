# QA-CLOUD-NUGET-SIG-VERIFY-004：Linux 固定 SDK 包签名验证包

2026-10-01（Asia/Shanghai） · 状态：`PREPARED_FOR_ENGINEERING_REVIEW`

本文件只准备执行合同，不授权任何下载、解压、安装或命令执行。只有工程主程在后续
回合机械复核本文件最终 SHA-256，并把该 SHA 回传给 assigned owner 后，状态才可变为
`AUTHORIZED_FOR_ASSIGNED_OWNER`。不得把本文出现的 URL 视为提前执行许可。

## 0. 目标、责任与结论边界

本包只回答一个问题：在支持 NuGet signed-package verification 的 durable Linux
环境中，以固定官方 .NET SDK 8.0.425 对**重新从同一官方 public URL 取得**的
YamlDotNet 16.3.0 nupkg 运行真实 `dotnet nuget verify --all`，结果是什么。

- delivery owner：既有 durable QA cloud thread
  `01a0f13c-3d87-753a-ad11-16302add9084` / `durable`；Debian 13 x86_64。
- 准备与接收：主测试 thread
  `01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / `local`。
- 签发与工程审查：工程主程 thread
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI thread
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已授权最小跨平台验签适配。
- 原 M owner、材料与证据保持原位；本包不修改或替代 M 的
  `BLOCKED_PACKAGE_SIGNATURE` 结果。

成功终点只能是 `READY_FOR_INDEPENDENT_SIGNATURE_R`，不是作者自判 package 或 M
通过。网络、内容身份、SDK 能力或验签失败均须原样保存并送同一原独立 R；R 才给唯一
`ACCEPT` / `NEEDS_FIX` / `REJECT`。即使 SDK 能运行、nupkg hash 相同或包内签名 entry
相同，也不能冒充密码学验签成功。

## 1. 固定 authority 与前序环境事实

### 1.1 主程固定的官方 release metadata 条目

工程主程于 2026-10-01 从官方
`https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/8.0/releases.json`
取得并保存 metadata body：1,584,285 bytes，SHA-256
`21cc0b1f333822c9c86df690b9dad0191ee3bd4abebd05ad86a0bb353e38fc0b`。
从该 JSON 唯一锁定：

| field | fixed value |
| --- | --- |
| release version / date | `8.0.31` / `2026-09-08` |
| SDK version | `8.0.425` |
| RID | `linux-x64` |
| SDK URL | `https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.425/dotnet-sdk-8.0.425-linux-x64.tar.gz` |
| SDK body bytes | `216833108` |
| SDK SHA-512 | `934b8060a7190e5909ad1fd0785db542f487b3bbf6cdd14826b02095fdd0d0394298b1634085eff302928fccc33f7c1a7253e9b87df555fc36fce819bcd2e798` |

这些固定值是本包的签包 authority。执行端不得搜索 latest、改版本、改 RID、解析网页
替代条目，或自行选择其它 SDK host／镜像。

### 1.2 云端 metadata 端点已被一次性证伪

同一 durable owner 已在 turn `01a0f4b2-6d03-7148-b9be-d8b5b5ad58ce` 执行
`QA-CLOUD-NUGET-SIG-METADATA-002`。唯一 metadata GET 在代理 CONNECT 阶段收到
HTTP 403；curl exit 56、HTTP code 000、0 body bytes、0 redirects。证据根为：

```text
/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-METADATA-002/
```

该 root 共 15 个证据文件／6,399 bytes，`SHA256SUMS` 自校验通过；未生成空或伪造的
`releases.json`，未执行 SDK HEAD，也未下载 SDK／nupkg。本文明确**禁止再次请求**
该 metadata endpoint；重复已知 403 没有信息增益，也不得换 metadata mirror。

同一 owner 于只读补充 turn `01a0f4ba-c885-7040-b1d6-e3985d3fbfcb` 复核：
`SHA256SUMS` verify exit 0，`result.json` 不存在，`run.json.result` 为
`BLOCKED_NETWORK_POLICY`。签包固定的 predecessor seals 为：

| relative path | bytes | SHA-256 |
| --- | ---: | --- |
| `run.json` | 3,271 | `b2bfa9ff1e8c695f3450437754c2250a3b250c87ac1d9adfed13903136b14c2c` |
| `metadata.headers` | 143 | `a24b68175b9817693292c22356afa00cfb41f490e204eccd2885d085f11354f9` |
| `metadata.stderr.txt` | 47 | `115ac061d5c11b9f88bc706341277e549679986ec53ba237026722b790803312` |
| `metadata.stdout.txt` | 194 | `a40dd82cc569e0c44c2777f35006db95ab6783df967e862fdd39ccc12014f6f9` |
| `SHA256SUMS` | 2,116 | `40d5712dd5970cc48362da5992e33ec1b7fb243ad5dd34d2f46e2d685ab6a3e3` |

执行端必须先机械核这五项并重跑 `sha256sum -c SHA256SUMS`；任一不一致返回
`BLOCKED_AUTHORITY_MISMATCH`，不得联网。

### 1.2a 003 失败封存与 004 sibling 边界

前一 sibling `QA-CLOUD-NUGET-SIG-VERIFY-003` 已在 turn
`01a0f4be-6200-7730-a905-e8636b9b05ac` 按最早 authority 门停止：
`BLOCKED_AUTHORITY_MISMATCH`。其 create-once root 保留为：

```text
/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-VERIFY-003/
```

该 root 为 10 files／23,060 bytes；`result.json` SHA-256
`b199ae83586cbecc3880cb670fbc1bfed7254061c512c1a4521bc8958d321f6c`，
`evidence-files.tsv` SHA-256
`a6207b732255561332ad584b0c842fee8a5f7312ee9e688c29f74c5beada5996`。
实际 network requests 为 0；SDK／package 的 HEAD、GET、identity、extract 和所有
dotnet／verify 均 `NOT_RUN`。真实 user home/profile、trust、project、Git、Unity 与
environment configuration 前后相等，无遗留进程。004 不得删除、覆盖、复用或补写
003 root；003 不提供任何 SDK／package／signature verdict，只证明旧 packet 的三条
predecessor path 写错。

### 1.3 固定 NuGet package 身份

| field | fixed value |
| --- | --- |
| package URL | `https://api.nuget.org/v3-flatcontainer/yamldotnet/16.3.0/yamldotnet.16.3.0.nupkg` |
| nupkg body bytes | `776880` |
| nupkg SHA-256 | `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4` |
| exact ZIP entry | `.signature.p7s` |
| signature bytes | `12920` |
| signature SHA-256 | `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308` |

这些值与原 M actual 一致，但云端必须从上表同一 public URL 重新取得 package；不得传入
Mac 文件、M temp/material bytes、本地 cache 或另一个 feed。

## 2. 唯一写白名单与永久禁止范围

执行开始前下列 root 必须不存在；存在即 `BLOCKED_ROOT_EXISTS`，不得清理、覆盖或复用：

```text
/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-VERIFY-004/
```

唯一允许写入为该 create-once root 及其子目录：

```text
authority/
before/
network/
downloads/
sdk/
package/
verify/
process/
  dotnet-home/
  nuget-packages/
  nuget-http-cache/
  nuget-plugins-cache/
  nuget-scratch/
  tmp/
  NuGet.Config
after/
result.json
evidence-files.tsv
```

SDK 只可解压到 `<root>/sdk/`，不是系统安装。禁止写 `/usr`、`/usr/local`、`/opt`、
用户全局 dotnet/NuGet 目录、system/user trust store、环境 profile、`/workspace/FightMatch`
及其它 repository、原 METADATA-002 root、原 M root、Git objects/refs/index、Unity、Luban、
Android、package cache 或生产路径。禁止 `apt`、`sudo`、installer script、`curl | bash`、
全局安装、证书导入、trust 变更、容器／VM 创建、branch／commit／push／fetch／reset。

## 3. 网络与资源合同

允许联网目标只有第 1.1 节 fixed SDK URL 和第 1.3 节 fixed nupkg URL。每个 URL 最多
一次 HEAD 和一次 GET，顺序执行、network concurrency `1`；不得请求 metadata、索引、
registration、搜索、备用 CDN、mirror 或其它版本。

- request 总数 `<=4`；每个 method/URL 最多一次；不得 retry。
- redirect hops 每 request `<=3`；仅 HTTPS。每一 hop 的 URL、status、headers、bytes、
  duration 和 exit code 必须保存；出现未经本包固定的目标 host 即
  `BLOCKED_NETWORK_POLICY`，不得跟随。
- HEAD 若被策略拒绝但 GET 可达，可按固定 GET body bytes/hash 门继续；HEAD 失败不能
  放宽 GET 的身份检查。GET 403/代理拒绝则按 URL 返回 `BLOCKED_NETWORK_POLICY`。
- SDK GET：connect timeout `<=30s`、total timeout `<=600s`、response body hard cap
  `216833108` bytes；下载完成后 body bytes 必须**精确**相等。
- nupkg GET：connect timeout `<=30s`、total timeout `<=120s`、response body hard cap
  `776880` bytes；下载完成后 body bytes 必须**精确**相等。
- 两个成功 GET 的 expected aggregate body 为 `217609988` bytes；任何额外成功 body、
  partial body 或响应越界均停止，不提高预算。
- 整包墙钟 `<=20 min`；证据 root `<=768 MiB`；执行前和最终 free-space 均
  `>=2 GiB`。超限即停止，不修改合同。

每次 request 保存实际 argv（敏感代理值脱敏）、开始／结束 UTC、requested URL、
redirect chain、status codes、response headers raw＋SHA-256、body bytes/hash、curl exit、
stderr/stdout。不得把 HEAD `Content-Length` 当成 GET body 证据。

## 4. 唯一执行序列

1. 记录 thread/host/turn、`uname -a`、Debian release、CPU、UTC、free-space、proxy 变量名
   与是否设置（值脱敏）、项目/Git/Unity 零动作声明；对真实用户的 `~/.dotnet/`、
   `~/.nuget/` 及现有 shell profile 文件保存 read-only before manifest（不存在也显式
   记录）；核 root absent 后只创建一次。
2. 把本包最终文件、主程 activation receipt 以及第 1 节固定 authority 写入
   `authority/`；读取但不改 METADATA-002 的 `run.json`、headers、stderr 与
   `SHA256SUMS`，保存路径／bytes／SHA 和自校验结果。禁止重发 metadata GET。
3. 按第 3 节最多 HEAD+GET fixed SDK URL。GET 完成后先核 body `216833108` bytes 与
   SHA-512
   `934b8060a7190e5909ad1fd0785db542f487b3bbf6cdd14826b02095fdd0d0394298b1634085eff302928fccc33f7c1a7253e9b87df555fc36fce819bcd2e798`；
   任一 mismatch 为 `BLOCKED_SDK_IDENTITY`，不得解压。
4. 对 SDK tar.gz 先生成完整 entry manifest。拒绝绝对路径、`..` traversal、device、
   FIFO 和 socket；symlink/hardlink 只允许其解析后目标仍在 `<root>/sdk/`。通过后才解压
   到空的 `<root>/sdk/`，并证明没有写出 root。记录 extracted file/link count、总 bytes、
   manifest SHA-256 和 `<root>/sdk/dotnet` bytes/SHA-256。
5. 在 `<root>/process/` 内创建隔离的 `dotnet-home/`、`nuget-packages/`、
   `nuget-http-cache/`、`nuget-plugins-cache/`、`nuget-scratch/`、`tmp/` 与固定最小
   `NuGet.Config`（只含空
   `<configuration />`）。三个 dotnet invocation 都必须只在该进程设置：

   ```text
   DOTNET_CLI_HOME=<root>/process/dotnet-home
   NUGET_PACKAGES=<root>/process/nuget-packages
   NUGET_HTTP_CACHE_PATH=<root>/process/nuget-http-cache
   NUGET_PLUGINS_CACHE_PATH=<root>/process/nuget-plugins-cache
   NUGET_SCRATCH=<root>/process/nuget-scratch
   TMPDIR=<root>/process/tmp
   DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
   DOTNET_CLI_TELEMETRY_OPTOUT=1
   DOTNET_MULTILEVEL_LOOKUP=0
   DOTNET_NOLOGO=1
   ```

   禁止设置或覆盖 `HOME`、`home`、`CODEX_HOME`。以上是本包子进程的工具专属环境，
   不得写入 shell/profile 或真实用户环境。先只以绝对路径
   `<root>/sdk/dotnet` 运行一次 `--info`；保存 raw stdout/stderr/exit，
   必须确认 SDK `8.0.425`、RID `linux-x64`。再运行一次
   `nuget verify --help`，保存 raw 输出并确认 exit 0 且包含 `--all`；失败为
   `BLOCKED_SDK_CAPABILITY`，不得安装或改 trust 修复。每个命令后立即重算第 1 步的
   真实用户路径；任一变化返回 `BLOCKED_UNISOLATED_TOOL_WRITE`，不得继续下一命令。
6. 按第 3 节最多 HEAD+GET fixed nupkg URL。GET 完成后先核 body `776880` bytes 与
   SHA-256
   `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4`；
   mismatch 为 `BLOCKED_PACKAGE_IDENTITY`。
7. 用 host 既有只读 ZIP 能力列出 nupkg entries；只提取 exact `.signature.p7s` 到
   `<root>/package/`。必须恰好一项、12920 bytes、SHA-256
   `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308`；任何差异为
   `BLOCKED_SIGNATURE_ENTRY_IDENTITY`。禁止把该 entry hash 当成验签结论。
8. 不改 `PATH`、真实 HOME、真实 NuGet config/cache 或 trust；沿用第 5 步隔离的
   per-process 环境，并只运行一次：

   ```text
   <root>/sdk/dotnet nuget verify <root>/downloads/yamldotnet.16.3.0.nupkg --all --verbosity detailed --configfile <root>/process/NuGet.Config
   ```

   保存 exact argv、raw stdout、raw stderr、exit code、开始／结束 UTC、墙钟和 owned
   process tree。一次 top-level process tree；自然等待，单次轮询不超过 60s，总命令
   timeout `<=180s`；不得 signal 后重试或换参数。
9. 等待全部本包 owned descendants 自然退出，证明无遗留进程；记录隔离
   dotnet-home/cache/scratch/tmp
   的完整 manifest，并对第 1 步冻结的真实用户 `~/.dotnet/`、`~/.nuget/` 与现有 shell
   profile 文件做 before/after 只读身份比较，证明未被本包写入；生成
   `evidence-files.tsv`（relative path、bytes、SHA-256）和 `result.json`，再核 evidence
   root 大小、free-space、项目／Git／Unity／trust／环境配置零写入。

## 5. 结果分类与独立 R 合同

按最早失败门只写一个状态：

| status | 条件 |
| --- | --- |
| `BLOCKED_ROOT_EXISTS` | create-once root 前置失败 |
| `BLOCKED_AUTHORITY_MISMATCH` | 固定 METADATA-002 predecessor seals 或自校验不一致 |
| `BLOCKED_NETWORK_POLICY` | GET/redirect 被策略拒绝或越出固定 host/path |
| `BLOCKED_SDK_IDENTITY` | SDK bytes/SHA-512 不匹配，或 archive 不安全 |
| `BLOCKED_SDK_CAPABILITY` | 固定 SDK 版本/RID/help 能力不成立 |
| `BLOCKED_UNISOLATED_TOOL_WRITE` | fixed SDK 改写了任务 root 外的用户工具路径/profile |
| `BLOCKED_PACKAGE_IDENTITY` | nupkg bytes/SHA-256 不匹配 |
| `BLOCKED_SIGNATURE_ENTRY_IDENTITY` | exact entry count/bytes/SHA-256 不匹配 |
| `BLOCKED_PLATFORM_SIGNATURE_CAPABILITY` | verify raw output 明确表示本平台无法验证 |
| `SIGNATURE_VERIFY_FAILED` | verify 实际运行且 exit 非零，未命中平台能力分类 |
| `READY_FOR_INDEPENDENT_SIGNATURE_R` | 所有身份门通过，verify exit 0 且 raw output 明确报告签名有效 |

执行者不得根据期望文字改写 raw 分类；不确定时保留 stdout/stderr/exit 并写
`SIGNATURE_VERIFY_FAILED_NEEDS_R_CLASSIFICATION`。无论 verify exit 0/非零，都只回原独立
R thread `01a0e404-e8ee-7310-8388-9260babd53f1` 做实际证据审查；R 要把同包 hash、签名
entry identity、固定 Linux SDK、完整命令与 raw 输出连成一条链，再决定原 M 的签名门
是否关闭。不得覆盖或重写 Mac M result。

## 6. 回传与 NOT_RUN

回传必须包含：packet SHA/lines/bytes；activation receipt；thread/host/turn；authority 与
METADATA-002 identity；root absent/create count；platform/free-space；每个网络请求；SDK
archive/hash/extract/dotnet identity；package/signature identity；`--info`、help、verify 的
raw stdout/stderr/exit/hash；process tree/quiescence；result/evidence manifest；全部零写入
证明和唯一状态。

下列均为 `NOT_RUN`：restore、build、publish、official tests、harness/self-tests、Luban、
005、LOC-A/B/L、`Program.cs`、Unity compile/test、uGUI、Android、APK、device、CDN、Git
写入与任何产品/配置/资源修改。完成后停止；不得自行进入 B、L 或 LOC-A 实施。
