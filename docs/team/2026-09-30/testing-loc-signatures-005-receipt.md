# LOC 三包 Linux 签名门接收 005

2026-10-01（Asia/Shanghai） · 主测试证据接收 · 唯一 verdict：**ACCEPT**。

范围仅为固定 Linux SDK 对 M13 三份同字节官方包的实际签名验证；允许后续材料包引用此门。
不接收完整 005 执行合同“全无偏差”的声明，不批准 M 最终 material、restore、构建、产品或许可证适用性。
本回合没有验签重跑、云端读取、联网、Unity、代码审查或下级派发；唯一新增文件为本回执。

## 输入与可追溯身份

- 合同：[005](testing-loc-package-signatures-005.md)，4,024 B，SHA-256 `ce3ffd0b2bcbdeaf255853a54ccf92a0e386df32f946b35da0aae47376aac680`；继承 [004](testing-loc-license-alt-signature-linux-004.md) 与 [A01](testing-loc-license-alt-signature-linux-004-addendum-01.md) 的限定信任／隔离要求。
- 执行者：`01a0f13c-3d87-753a-ad11-16302add9084` / durable；实际 turn `01a0f6a4-ac65-7368-b532-67a4afaf4d57`。
- [原命令与输出](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/cloud-signature-005-received/retrieved-outputs.json)：97,891 B，SHA-256 `0d44d64993a5db23ae6cc6b2a7a120c36b0f722ad628859a7026582e387b0db5`；18 条已取回 commandExecution 均为 completed、exit 0、output 未截断。它不是云聚合 result 的原字节。
- [M13 handoff](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m13/authority/signature-handoff.json)：31,253 B，SHA-256 `f0ac0e45c5058aa7e11dd0e00c67521c4ae7500fc72574f6d80ecc2e1cd18ed0`；其历史 `SIGNATURE_PENDING` 保持。

## 已成立的实际验签链

复用 SDK 8.0.425 / linux-x64；实际 launcher 为原 004 `sdk/dotnet`，68,424 B／SHA-256
`7dda7a639bc35fba05c40d308b0296450aa4e628d109245a6312c385da9b0931`。
原输出证明 launcher、archive 与 archive-manifest 前后匹配 004 封存身份，archive SHA-512 与 004 合同匹配；没有重下 SDK。
这不是对整个解压 SDK 逐文件重新核验的声明。三次命令均为该绝对路径的 `nuget verify <各自包> --all --verbosity detailed --configfile <005>/process/NuGet.Config`。

| 包 | nupkg / signature 字节数 | exit / 秒 | 原 stdout 字节数 / SHA-256 |
| --- | --- | --- | --- |
| System.Reflection.Metadata 1.6.0 | 852,113 / 22,354 | 0 / 0.566 | 8,739 / `cc429ca95a6d41d4402ac1c8f6ae973bcfb35e776187ee977e859f6aec8656e7` |
| System.Collections.Immutable 1.5.0 | 804,405 / 22,354 | 0 / 0.469 | 8,745 / `c557f9a649986b94097fee91b284a3aba7aa1070bbd4a740e41010d73fe6eb66` |
| xunit.abstractions 2.0.3 | 75,155 / 18,486 | 0 / 0.417 | 8,393 / `b3e0820f8385be573c9d0abb451c303fa7c5f7832c85a5927e8edfb81988a380` |

三次原工具 item 依次为 `exec-661d9557-e8a8-4079-8d02-a3ede2c3a745`、
`exec-9a07ffe2-7121-4662-8603-c6968529d72a`、`exec-b93b4890-ddc4-41b9-8254-e11fb8cf410d`。
主测试只读机械核对：本地 observed receipt 与各原输出 JSON 完全相等；六份 stdout/stderr 与原 bytes/SHA 及文本一致，stderr 均为空。
三包原 GET URL、bytes、SHA-256、catalog SHA-512、唯一 signature entry 身份均与 M13 handoff 对应；另对 M13 指向的实际 nupkg/signature 文件重算后全部一致。
每包 raw 均明确 `Finished with 0 errors and 8 warnings.` 及对应完整包名的成功消息，没有把 entry hash 充作验签成功。

每包 8 条警告为 NU3018 ×4、NU3028 ×4：作者主签名、其时间戳、仓库 countersignature、其时间戳四处各两条，均为吊销服务不可达／`RevocationStatusUnknown`／`OfflineRevocation`。
工具使用原 SDK `codesignctl.pem`／`timestampctl.pem` fallback bundle，未以修改系统 trust 或压制警告获得成功。
沿已接收 004 的限定门，此结果不证明证书当前未吊销、在线吊销成功、零警告或包可复现构建。

## 异常原样保留及接收边界

1. 首次 SDK 比较把 SHA-512 预期值与 SHA-256 实测值相比较，产生 false；`exec-84bdd4fd-ccef-44f7-8fff-e1726f3057da` 随即分别输出两算法实值，创建 005 前已正确匹配。该 false 是算法字段误配，未发现 SDK 字节改变。
2. Immutable observed `errors=1` 来自 `text.count('errors and')`，统计短语次数而非错误数量；raw 仍为 0 errors。旧字段不改写。
3. 初始 `ENVIRONMENT_BLOCKED`（`exec-dae8f92f-4e74-4521-a6cf-cc7a2a6d90e5`）对每包都读取共享 `.signature.p7s`，该导出文件已被后包覆盖，造成聚合身份误配。每包执行前的 ZIP 身份与各自 nupkg 验签均已独立通过。
4. `exec-83aaa50f-234d-403a-8140-c9568060485c` 从三份原包重新读取唯一 entry，分别保存 `.signature.01/02/03.p7s`；随后 `exec-c1ecc5dd-bdb6-4287-83b6-fb87326d6314` 重建报告。此处没有再次 GET 或 verify；原初失败输出永久保留。
5. 实际 `Popen(env=...)` 使用显式环境字典，含全部任务隔离目录和 A01 两项禁用变量，PATH取父进程值；未包含 HOME/home/CODEX_HOME。因此记录中的“inherited unchanged”不准确，不能认定子进程与父进程这些变量相同。原命令与此差异保持可追溯；已观察到的真实用户九路径 before/after 相等、用户 dotnet/NuGet 目录仍 absent。此限定签名接收不豁免该环境记录偏差，也不宣称完整环境继承合同通过。

网络原输出为固定 URL 串行 GET 各一次、HTTP 200、exit 0、0 redirect，三包合计 1,731,673 B；各 GET <1 秒，无重试证据。证书链内部网络尝试未被计为零。
最终原命令输出证明 owned verify 进程为零、用户路径相等、root 1,877,849 B <16 MiB、最大日志 8,745 B <2 MiB、free 29,796,995,072 B >2 GiB。
这些与 raw 验签链足够支撑本次限定签名门；不因汇总格式重复验签。

## 尚未取得的聚合原件

云 `result.json` 仅有报告身份：13,743 B／SHA-256 `f44b06a6b1367986ee63ea27c5732e34f108d4563800169504490df61193cd36`。
云 `evidence-files.tsv` 仅有报告身份：6,406 B／SHA-256 `51c385b336a3f2a7557ee52a9f7ec6e033e1cb3cbf0a2cdba535c766cd78d474`；原命令报告 68 条及自校验 true，本机未独立重放完整 manifest。
上述原字节因 app-server unavailable 未回收，不能把本地 projection 命名或封存为它们。归档补齐只需以后一次只读取回这两份原件及核 hash；不是重新验签的前置。
后续报告应引用本回执保留环境记录差异；整个 005 合同符合性不能从本 verdict 外推。
中央提供的 [PR #2 评审](https://github.com/yyczz1/FightMatch/pull/2#issuecomment-5928085467) 对应 head `1ac2081958d8cd91dd821d85ed7d071ec1310ee6`；它不替代本签名证据，本回合未重复审 driver。

本机核对命令 exit 0；三份实际 M13 包与签名、本地六份 raw、原输出引用、警告计数全部匹配。M 最终材料及下一阶段仍需另行接收与授权。
