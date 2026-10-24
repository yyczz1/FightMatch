# LOC license gap bodies 15 — 七项固定许可证输入

2026-10-01 · `LICENSE_INPUTS_READY`（仅本文七项输入；不是 M14 SOURCE_READY、最终 material ACCEPT 或法律合规裁决）。

## 范围与可复用输入

- 中央授权/唯一收件 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`；主程交付 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，实际 turn `01a0f6d4-605a-78a1-9bec-718b88da3a61`。继承 [原 M §5.4](engineering-loc-license-alt-m-materials.md#54-license-closure) 的正文与 package relationship 要求。
- 唯一材料根 `G=TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15`；[receipt](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15/receipt.json)、[inputs](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15/inputs.json)、[源观察](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15/source-observations.json)、[请求账本](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/license-gap-bodies-15/network-ledger.json) 为机器可查证据。
- 七个既有 M09 nupkg 只读复用；逐包保存 archive SHA256/SHA512、nuspec 原 bytes 的 base64/SHA256及自身声明。六包自行声明 exact repository commit；NeoLua 用官方版本 tag→commit→自身项目→版本/许可证属性补齐，未把模板或相邻包作正文 authority。
- 原冻结 M01 licenses 为空，M09 replay/stage 无正文。并行本地查找另找到 `/private/tmp/neolua-v1.3.14/LICENSE.md` 与 common.targets，分别与新 Git blob 完全同字节；发现时初始 NeoLua 读取已开始，28 次调用均如实保留。旧研究见 [NeoLua 来源](research-loc-a-license-sources.md#3-neolua-1314-verifiable-first-party-license-input)。

## 实际保存的完整官方文件

| G 的 license basename（均 `.txt`） | exact official commit/path | 声明/保留范围 | bytes / SHA-256 |
| --- | --- | --- | --- |
| `neolua.1.3.14` | [`cb560afef9dd19b7afad6da594b44909c9739698` / `LICENSE.md`](https://github.com/neolithos/neolua/blob/cb560afef9dd19b7afad6da594b44909c9739698/LICENSE.md) | Apache-2.0 | 11324 / `a8ad31b1c3f40dca5a84119351b8fa8ddc868edd77fad8a8ebf6d8f2d16fa4ae` |
| `exceldatareader.3.7.0` | [`840007736467bb66be083764021a3a6b9afad5c2` / `LICENSE`](https://github.com/ExcelDataReader/ExcelDataReader/blob/840007736467bb66be083764021a3a6b9afad5c2/LICENSE) | MIT | 1081 / `52ba99951f976c3c89ac69d9b3bed7ca264f7aac0253c61c163dc3b658fd0f04` |
| `google.protobuf.3.29.0` | [`2d4414f384dc499af113b5991ce3eaa9df6dd931` / `LICENSE`](https://github.com/protocolbuffers/protobuf/blob/2d4414f384dc499af113b5991ce3eaa9df6dd931/LICENSE) | BSD-3-Clause | 1732 / `6e5e117324afd944dcf67f36cf329843bc1a92229a8cd9bb573d7a83130fea7d` |
| `messagepack.3.1.7` | [`1b53ae8b7681340199faf337bcbb0eb5aed702de` / `LICENSE`](https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/1b53ae8b7681340199faf337bcbb0eb5aed702de/LICENSE) | MIT；完整第三方附录保留 | 3032 / `580852521f6e48cb1d210fcb228e7179a21433a32ebe3fbb46ec8c10255e7451` |
| `messagepack.annotations.3.1.7` | [`1b53ae8b7681340199faf337bcbb0eb5aed702de` / `LICENSE`](https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/1b53ae8b7681340199faf337bcbb0eb5aed702de/LICENSE) | MIT；完整第三方附录保留 | 3032 / `580852521f6e48cb1d210fcb228e7179a21433a32ebe3fbb46ec8c10255e7451` |
| `nlog.5.3.4` | [`73d83d3161d31288ca5c554cc50d27b6bed5f28b` / `LICENSE.txt`](https://github.com/NLog/NLog/blob/73d83d3161d31288ca5c554cc50d27b6bed5f28b/LICENSE.txt) | BSD-3-Clause | 1553 / `0bf6ba2a9fc4e8b4b2fccbd712a9bf118989a60b8a1767551c23150f4154b08c` |
| `scriban.7.2.5` | [`8872919995b8baae3008c2d3be6f0fad04e812ac` / `license.txt`](https://github.com/scriban/scriban/blob/8872919995b8baae3008c2d3be6f0fad04e812ac/license.txt) | BSD-2-Clause | 1318 / `7423242b4ae72bccdf19a06cd3c20790df8519a164a08f37b31cb4a40034d827` |

- 七个 `licenses/<basename>.txt` 各自配 `provenance/<basename>.json`。28 次官方 GitHub 只读调用均 completed；保存的是真实工具返回，不宣称 HTTP wire 原件。工具返回的 UTF-8 正文重新计算 Git blob SHA-1，全部匹配固定 commit tree entry 和 blob URL；落盘字节又与返回正文、SHA-256逐项相等。
- 原始 BOM、换行和无 EOF newline 保持；ExcelDataReader/Scriban 原文没有末尾换行，未擅自补入。NeoLua 是 11324 B / 预定 SHA，包含 Apache 九节及 appendix。
- NeoLua 的 `v1.3.14` ref、`NeoLua.csproj`→`common.nupkg.targets`→`common.targets` 导入链确证 PackageId、FileVersion `1.3.14.0` 与 PackageVersion `1.3.14`；nupkg 本身没有 source commit，此限制保留，不声称可复现构建。
- Annotations 使用自己的 nupkg/nuspec 的 ID/version/MIT/commit，加自己的 csproj 及两级 Directory.Build.props；不是由 MessagePack 的 license 推断。两份完整 LICENSE 允许同 hash，但 provenance 各自独立。
- MessagePack LICENSE 不止 MIT：同时保留 lz4net 的 BSD-2-Clause 正文和 BufferWriter.cs 的 Apache notice。Annotations 保留同一完整上游文件，不据此声称这两段实现存在于 Annotations，也不把其 nuspec 的 MIT 声明改写为无范围组合表达式。

## NOTICE 检查与下一阶段有限映射

- NeoLua、ExcelDataReader、MessagePack、NLog、Scriban 的 exact commit 完整 recursive tree 均未截断，未见独立 NOTICE/COPYING；Google.Protobuf 检查 exact repo root 与完整 csharp subtree，均未截断且未见，结论不外推其他语言/third_party 子树。该 C# csproj 明确 3.29.0/BSD-3-Clause 与源码范围；不是用 nupkg 缺失来推断 NOTICE 不存在。
- Scriban 另有 `src/Scriban.Tests/LiquidTests/dotliquid-license.txt`，位于测试项目外部于已核的 Scriban 包项目；其存在与排除范围均保留，不当作 Scriban 运行包正文替代物。
- 后续 M 源码准备只可按表中七个 basename 导入 `G/licenses/<basename>.txt` 和其同名 provenance，以 `G/files.tsv`、inputs 和 source-observations 固定来源；package/license/SBOM 应逐项 join 自身 package key，MessagePack 的附录与 Annotations 独立关系不能丢。
- 七项正文/关系当前无剩余输入缺项；M14 整套 source/driver、完整依赖闭包与 material 门仍未接收。M14 拟议的 Apache 官方网站标准文本归档不在本轮新 GET 范围，不把 NeoLua 文件冒充 Apache 网站原件；中央可在后续合同中明确其标准条款复用关系。
- E14 原四叶仍保持 `BLOCKED_LICENSE_CLOSURE` 历史，不改成成功；T14/M14 absent。零 SDK/nupkg/整库下载、零验签/dotnet/restore/Unity、零产品/Git写入，未直接派 LOC。
- 本次 research 技能用于一个只读本地来源子任务；writing-for-agents 用于收敛成有限输入映射。原始证据由主程统一归档，后续执行仍须中央授权。
