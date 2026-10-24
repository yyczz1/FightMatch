# LOC license closure 16 — 31 包有限输入映射

2026-10-01 · `LICENSE_BODIES_COVERED_RELATIONSHIP_GAPS_REMAIN`。31 项正文集合可用；27 项输入关系可复用，4 项 binary-lane catalog 绑定仍缺。不是 M 最终 material ACCEPT、法律合规裁决或执行授权。

## 范围与取用

- 交付主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，实际 turn `01a0f6f4-ac8e-7d92-aadc-4c62b4d70052`；唯一收件中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。沿用户已批准团队协调及本轮中央有限授权，不直接派 LOC。
- 唯一新材料根 `H=TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-closure-bodies-16`；[receipt](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-closure-bodies-16/receipt.json)、[31项映射](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-closure-bodies-16/coverage-31.json)、[inputs](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-closure-bodies-16/inputs.json) 是后续逐 key join 的入口。
- M15 `authority/offline-checks.json` 仅读取一次并封存其 licenseCoverage 投影；当时 84696 B／SHA `b6099ac055c7579e7a6a22a3aa912ac3ae85a4866cf0a38a386a18f632bb26a2`。不冻结、重读或验算 LOC 正在修改的整棵 E15。
- G 七项、M13 三项保持原证据和限定结论；H 只新增其余21项的独立 provenance、必要正文／附录，以及标准 Apache 条款。原包只读，未重下源码包或 nupkg。
- 每个 H key 导入 `provenance/<key>.json` 指定的全部正文及标准引用；不可只看扩展名或把 NOTICE/附录合并成无范围许可证表达式。四个 pending key 的 provenance 可供核对，不能当作关系闭合输入。

## 新增／绑定的正文

| key 范围 | 自身关系与正文 |
| --- | --- |
| StringTools 17.11.4 | 自身 nuspec commit＋[StringTools csproj](https://github.com/dotnet/msbuild/blob/37eb419ad2c986ac5530292e6ee08e962390249e/src/StringTools/StringTools.csproj)＋根 MIT；包内 THIRDPARTYNOTICES 原字节另存。 |
| CodeCoverage / Test.Sdk / ObjectModel / TestHost 17.11.1 | 各自 nuspec 指向同一 [vstest commit](https://github.com/microsoft/vstest/tree/58dbd027217ac035a4e9114c9213b11dc0e988bd)，各自项目或打包 nuspec 路径单独绑定；根 MIT 与根第三方通知保留，CodeCoverage/TestHost 另保存自身包内 NOTICE。根通知不表示所列全部组件都在每个包内。 |
| xunit、assert、core、extensibility.core/execution 2.9.2 | 各自 nupkg/nuspec 的 ID/version/commit＋[同 commit src 下各自 nuspec](https://github.com/xunit/xunit/tree/82543a6df6f5f13b5b70f8a9f9ccb41cd676084f/src)；完整 Apache notice／MIT AssemblyResolution 附录＋标准全文。不是 M13 snapshot 扩权。 |
| xunit.analyzers 1.16.0 | [自身版本 commit/LICENSE](https://github.com/xunit/xunit.analyzers/blob/c399ff14191b5f0ae95f215e2674c8419d9c4505/LICENSE)＋自身 nuspec；MIT 附录明确仅指测试 XunitVerifier.cs，不能推断它装入 analyzer。 |
| xunit.runner.visualstudio 2.8.2 | [自身版本 commit/License.txt](https://github.com/xunit/visualstudio.xunit/blob/699d445a1aab46a014bce946edc4995b6b46accf/License.txt)＋自身 nuspec；两项 AssemblyResolution 的 MIT 附录完整保留。 |
| MessagePackAnalyzer 3.1.7 | 自身 nuspec＋[自己的 csproj](https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/1b53ae8b7681340199faf337bcbb0eb5aed702de/src/MessagePack.Analyzers/MessagePack.Analyzers.csproj) PackageId＋G 两级 props。复用 G 的3032 B全文，不用兄弟包关系代替自身证据；MIT/lz4net BSD/BufferWriter Apache 段不丢失、不外推其代码均在 Analyzer 内。 |
| YamlDotNet 16.3.0 | 原 nupkg commit与既有 tar 的 PAX comment 均为 `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73`；397462 B tar SHA `4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce`。两份本地全文与 exact tar entries 同字节，Antoine Aubry／Kirill Simonov 两份 MIT 均保存。 |
| Ude.NetStandard 1.2.0 | 包内 COPYING 仅23 B“See license directory”，不是全文，包内没有该目录。[官方tag1.2自身csproj](https://github.com/yinyue200/ude/blob/11a9ba81e2562e22129285b0c4a281b6f9f3e71f/src/NetStandard/Ude.NetStandard.csproj) 给出 PackageId、Version1.2（NuGet归一化1.2.0）及 COPYING；README＋源码头声明 MPL1.1 或 GPL2+ 或 LGPL2.1+，三份完整原文均存。旧 AssemblyVersion1.0.2 保留，不称构建证明；旧 Ude.nuspec 不作本包 authority。 |
| CommandLineParser2.9.1、CSharp4.5.0、Newtonsoft.Json13.0.1/13.0.3、Bson1.0.3、Codepages8.0.0 | 各自 exact nupkg 中完整 MIT 与存在的第三方 NOTICE；`sourceCommit=N/A_EMBEDDED_LICENSE_ENTRY`。两版 Newtonsoft 即使全文同 hash 仍分别绑定包。四项 catalog 缺口见下。 |

- Ude 的 `.txt` 是 MPL、`.secondary.txt` 是 GPL、`.NOTICE.txt` 是 LGPL；最后一个仅使用获准存储槽，不冒称上游存在独立 NOTICE。没有替项目选择分发许可证。
- [Apache 官网全文](https://www.apache.org/licenses/LICENSE-2.0.txt)唯一一次 HTTPS GET：200，11358 B／SHA `cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30`。H 的 standard receipt 保存实际 body/base64、headers、时长和方法；不是把 notice 或别的项目正文冒充官网原件。

## 剩余边界与验证

- 四项必需绑定缺口：`commandlineparser.2.9.1`、`microsoft.csharp.4.5.0`、`newtonsoft.json.13.0.1`、`newtonsoft.json.bson.1.0.3`。有限 M09/M01/old-cache 输入未见其官方 registration/catalog；完整包 SHA256/SHA512、nuspec 和正文 entry 已保存，仍不足于宣称官方 binary lane 完整闭合。未越权追加 NuGet 请求。
- StringTools、MessagePackAnalyzer、Ude 同样未在有限本地集找到 catalog，但已有各自源码／版本许可证关系；该输入关系不豁免后续 material 的包身份门。metadata 的 contentHash 与 signed ZIP SHA512 不混用；原因未在本轮诊断。
- 44次 GitHub调用（43成功，1次 generic tags URL 被 connector INVALID_ARGUMENT400 拒绝；不是上游404）；改用 git/refs/tags 后成功。28份返回正文的 Git blob SHA1／tree身份匹配，34个落盘正文／标准文件与实际源字节一致。BOM、CRLF和缺省EOF换行原样保留；每份≤128KiB，总根≤8MiB。
- NOTICE结论只覆盖保存的 exact root/subtree/ZIP；不以包内缺少文件推断源码全仓无 NOTICE。已发现此前投影漏列的 StringTools、CodeCoverage、TestHost 包内通知，并仅在 H 补充。
- M13原 `SIGNATURE_PENDING` 不改；复用[QA005限定签名ACCEPT](testing-loc-signatures-005-receipt.md)，每包8警告（NU3018×4、NU3028×4）、fallback／离线吊销以及子进程ENV未包含HOME/home/CODEX_HOME的偏差均保留。云聚合原件缺失也不伪造补齐。
- 零验签、dotnet、restore、Unity、SDK/nupkg下载、产品/Git写入；无新增代理或聊天。后续执行和 material 接收仍由中央按实际缺口决定，本文件不是新执行计划。
- research 技能用于一手来源核查，本轮明确禁止代理故由主程集中完成；writing-for-agents 将交付压缩成按key读取provenance／保留范围／阻塞项的有限映射。
