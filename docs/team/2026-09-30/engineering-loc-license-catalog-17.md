# LOC catalog bindings 17 — 四项官方元数据补证

2026-10-01 · `REQUIRED_FOUR_BOUND_ADDITIONAL_THREE_PENDING`。四项必需 catalog 绑定完成；三项额外包身份缺口保留。不是最终 material ACCEPT、验签、代码审查或执行授权。

- 唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，实际 turn `01a0f70d-3d96-7c92-ab92-672385a76dda`。继承用户批准的团队协调和本轮中央有限授权。
- 唯一新根 `J=TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17`，仅六文件；[receipt](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17/receipt.json)、[bindings](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17/bindings.json)、[实际观察](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17/observations.json)、[请求账本](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-catalog-bindings-17/network-ledger.json)。
- H16封存不改；输入 receipt SHA `3e2cfb8b0671cf713cfd5626ec38920fd4aa8a3fa2608009e23694909079280d`。有限 M01/M09/M13/材料根检索未发现这七项既有官方 catalog/registration；找到的 Newtonsoft13.0.3 不能替代13.0.1。

| 必需包 | 官方 registration → 它实际返回的 catalog | 与既有包核对 |
| --- | --- | --- |
| `commandlineparser.2.9.1` | [registration](https://api.nuget.org/v3/registration5-semver1/commandlineparser/2.9.1.json) → [catalog](https://api.nuget.org/v3/catalog0/data/2026.04.10.00.34.31/commandlineparser.2.9.1.json) | ID/version、size 496069 B、完整包 SHA-512 全匹配 |
| `microsoft.csharp.4.5.0` | [registration](https://api.nuget.org/v3/registration5-semver1/microsoft.csharp/4.5.0.json) → [catalog](https://api.nuget.org/v3/catalog0/data/2018.12.18.00.05.37/microsoft.csharp.4.5.0.json) | ID/version、size 627579 B、完整包 SHA-512 全匹配 |
| `newtonsoft.json.13.0.1` | [registration](https://api.nuget.org/v3/registration5-semver1/newtonsoft.json/13.0.1.json) → [catalog](https://api.nuget.org/v3/catalog0/data/2022.12.08.16.43.03/newtonsoft.json.13.0.1.json) | ID/version、size 2065787 B、完整包 SHA-512 全匹配 |
| `newtonsoft.json.bson.1.0.3` | [registration](https://api.nuget.org/v3/registration5-semver1/newtonsoft.json.bson/1.0.3.json) → [catalog](https://api.nuget.org/v3/catalog0/data/2026.04.10.00.19.43/newtonsoft.json.bson.1.0.3.json) | ID/version、size 240753 B、完整包 SHA-512 全匹配 |

- 四项仍用 H16 `binary-package/embedded-license`，`sourceCommit=N/A_EMBEDDED_LICENSE_ENTRY`；J只补官方 registration/catalog→exact nupkg 身份，正文与NOTICE继续引用各自H16 provenance，未重抓正文或改为source-build证明。
- registration/catalog原响应body以base64及SHA256保存，HTTP状态、headers、时间、错误原样保留；catalog URI只从已取得registration读取，没有猜catalog时间戳或下载packageContent。
- 初始隔离执行器的7次请求均DNS解析失败，无HTTP响应；获准放宽隔离后，4个必需registration各重试一次及各catalog首次读取均200。失败不从账本剔除：共15/16次尝试、8个HTTP200、原响应总62865 B，最大29590 B；无重定向、无自动重试，同址重试最多1次。
- 额外 `messagepackanalyzer.3.1.7`、`microsoft.net.stringtools.17.11.4`、`ude.netstandard.1.2.0` 仍缺catalog绑定：首次DNS失败后，仅余1次预算，不足完成任一registration→catalog两跳；不以缓存contentHash或包页描述冒充官方hash。
- H16＋J可补齐其四个“许可证输入关系待绑定”项，31项关系映射因此有输入；上述3项独立的包身份门仍未闭合，其他签名／材料门不因本补证获得接收。H16旧状态原样保留，以J作为追加证据。
- 本地机械校验：原body base64→JSON／SHA、registration指向、catalog ID/version/packageSize/SHA512、实际nupkg SHA256/SHA512及自身nuspec全部相符；H16 receipt/inputs原hash保持。完整回执封存本次准确范围与未完成项。
- 零SDK/nupkg下载、验签、dotnet、restore、Unity、产品/Git写入；未直接派LOC或作代码审查。research用于一手元数据核查；writing-for-agents用于按key限定补证映射，延续最小收尾、由主程集中归档。
