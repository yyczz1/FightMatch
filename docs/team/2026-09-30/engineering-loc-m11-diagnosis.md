# LOC M11：NuGet contentHash 来源错误及 FIX09 最小修正

2026-10-12。主程负责诊断、SOURCE 任务分发和组内接收；中央负责原 M11 实际运行接收及后续 GitHub 发布。本文件是本次唯一详细说明，不另建交接层。

## 诊断结论

M11 首条 `M-runner-generation-restore` 已 exit 0（6.523466 秒）；随后驱动的 `validate_restore` 报 `GateError: lock NuGet contentHash mismatch`。整次调用 127.410236 秒，后两条 restore 未启动。原回执：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m11/receipt.json`；原 LOC actual `01a12fee-8c94-7a01-944a-13a3c4de81e3`。这里引用事实，不重复中央的实际执行接收。

首个失败是 `src/Luban.Bson/Luban.Bson.csproj` 的 `Newtonsoft.Json.Bson 1.0.3`。生产循环按固定项目顺序和锁文件项顺序，在此项首先失败。

| 来源 | 实际值（下方 A/B） |
| --- | --- |
| `packages.lock.json` 的 `dependencies.net8.0.Newtonsoft.Json.Bson.contentHash` | A |
| `obj/project.assets.json` 的 `libraries[Newtonsoft.Json.Bson/1.0.3].sha512` | A |
| 安装目录 `.nupkg.metadata.contentHash`，version=2，source=固定 W8/feed | A |
| 安装目录 `newtonsoft.json.bson.1.0.3.nupkg.sha512`（当前驱动的比较预期） | B |
| F8 固定包目录中该包的 `sha512`，以及归档全文 SHA512 | B |

```text
A = bCcwagnHfYnhwQfY1criEcn6Hy9PtBuVnZu0pA8hmRhuR3jI/8WxVgoVAdNw9BJ3JHkxmWJzpj/AQy+PMMLqxg==
B = urXVj8mBbUG3cPxuQb4tqXVwe53fXMYJ6HEvkIq5vg5kBzPVJbLDxpapLUuDRB94S7fPIEeRX5pFnqEiSq1UOA==
```

比较预期 B 来自安装缓存 sidecar，不是锁文件生成失败。

已完成唯一一次限定只读归档比对：固定原包、W8/feed 副本和首阶段安装缓存归档均为 240753B，SHA256 均为 `f05c52751b32c6800b3a4f14cbe1f37e220942afa1244362b5bc8b1b7786550b`，全文 SHA512 均为 B。故本项不是归档内容漂移，而是驱动把全文归档哈希当成 NuGet contentHash。未再次执行 restore、SDK、host、下载或解压。

路径基准：T=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban`；W8=`T/.work/m17-fix08`；首阶段=`W8/phases/M-runner-generation`。三份归档分别在 `T/5.1.0/nuget/newtonsoft.json.bson/1.0.3/`、`W8/feed/`、首阶段的 `env/packages/newtonsoft.json.bson/1.0.3/`。锁及 assets 在首阶段 `source/src/Luban.Bson/` 下。

另只读观察同一项目的 Newtonsoft.Json 13.0.1、NLog 5.3.4、Scriban 7.2.5，三者均 lock=assets=metadata.contentHash。只对上述 Bson 包的三份归档做了新哈希比较；不声称已新核全部 31 包，也不从这些相等关系推导 NuGet 内部具体计算算法或独立签名认证。

## 代码位置及旧测试遗漏

基线 F8=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix08`。其 `source/m17_driver.py` 278195B，SHA256 `666faf4d3ed8bc586e8edb91d2c4d07013490d045a11dc47716ef4aa5cd93dbb`；freeze SHA256 `c942f1456cdccf2c1a55f8685eb4d055aad72325d4848877e12d97a380824c0f`；model SHA256 `edf98191eb9cf105d8e4c6361aea2b69718dc75a2380b853128bd03eb5392209`。已审源码 head `a0bfe7007be65eed6d1afda317934195cb6ebbd6`。

`execute_material` 约 1253 行读取 `(archive, archive.sha512)`；`validate_restore` 约 694–697 行把第二项作为 lock contentHash，710 行又要求 assets 与它一致。`verify_archive` 669–673 行已经验证固定归档的长度/SHA256/全文 SHA512，应保留。

`restore_fixture` 约 1331–1354 行却把人工 contentHash 直接放入 production 从 `.nupkg.sha512` 读取的位置，掩盖了真实文件语义差异。1444–1455 行“归档哈希不能替换 contentHash”的负例方向正确，但 fixture 没有覆盖读取路径。

## FIX09 执行范围（中央本轮补充的普通修正授权）

按用户已授权团队普通工作自治，主程继续完成最小 SOURCE 修正、一次相关静态差量验证和组内接收；可交既有 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，模型 Astra/xhigh。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local` 是唯一普通工作接收者。中央本轮消息明确允许新 FIX09 候选、必要记录和正确命令侧录；不授权新实际还原或 Git 写入。

最小生产改动：安装记录携带归档、归档 `.sha512` 及 `.nupkg.metadata` 三份实际字节；归档继续核固定 catalog，并将归档 sidecar 与全文/catalog SHA512 对应。严格解析 metadata（version=2、source=该候选固定 W/feed、contentHash 为 64B Base64）；lock contentHash 和 assets.sha512 只与 metadata.contentHash 比较。缺失、畸形和来源漂移均失败，无 catalog 值回退。可在等价小函数中实现；所有现有包 ID/版本、图、路径、lock/assets 集合、locked-mode 和双次还原一致性约束保持。

修正 synthetic fixture，使两类哈希来源真实分离；至少覆盖生产文件读取接口、M11 A/B 形状的旧红新绿、归档/sidecar/metadata 损坏、错误 source/version/Base64、lock/assets 不一致及缺文件。真实 M11 输出只读，固定旧 feed 路径只能在明确的静态 fixture 适配中转为新 W/feed，不能重写原证据或声称新还原成功。

白名单：新根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix09` 的八叶 `source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`；另允许 `docs/team/2026-09-30/loc-m17-fix09-static-checks.py`（必要时承载定向静态检查，固定为 SOURCE 输入）和 `docs/team/2026-09-30/loc-m17-fix09-command-receipt.json`。主程独占本说明，LOC 不修改本说明或共享索引。禁止额外报告层。

新材料/工作根仅作路径文本：`T/5.1.0-fm-yamldotnet-16.3.0-m17-fix09`、`T/.work/m17-fix09`，不创建。旧 F8、M11、A/H、所有失败/侧录/partial 保持只读；不整树重枚举、重复 SDK 扫描或重哈希本次已比较的归档。新候选只适配必要根/实际身份/输入保护/检查调用；固定字节保护及启动/命令侧录强制门不得降低。旧 sidecar 纠错证据仍引用已更正 F8 侧录，不继承误写 toolId。

尺寸/执行预算保持既有：driver ≤278528B（当前仅余333B）、八叶总≤16MiB、后置侧录≤64KiB。不得通过压缩/重排无关代码或删除保护/历史有效断言凑上限；若最小必要补丁确实超额，先给实际差量和最小需要值，不自动增额。可用新定向检查文件避免向 driver 追加重复历史套件，但封存必须绑定准确测试源码及结果。

源码固定后仅一次相关静态差量检查完整≤10秒，随后必要的一次 SOURCE 机械生成/封存完整≤30秒；均无 native 子进程、网络或材料写入。复用历史 OS 证据，不跑全套旧静态套件。检查或封存失败保留真实结果并交主程，不自行重复。正确后置侧录绑定最终八叶/freeze/model/本次 author/真实工具命令、完整输出、OS exit 和完整墙钟。禁止 restore/host/SDK探针/下载/Unity/B/L、Git、工具安装和产品/API/依赖版本修改。

验收：来源修正及失败关闭静态证据成立，完整性和原合同保留，候选在原尺寸/执行预算内，旧证据未改且没有新原生执行。主程核任务/输入/原工具与回执；不做第二轮本地代码审查，不把自测签为集成 ACCEPT。精确候选和验证交中央发布 GitHub PR Code Review，新 head 独立审查仍是必须的源码接收门；材料 M 仍未通过。

## 组内接收：真实尺寸合同阻塞

LOC actual `01a13000-142a-7681-9540-9f6227d41b50` 已完成只读文本计量并停止；初始任务说明为 7688B/SHA256 `d30cf94f63176471de62be3bb5e3a61be99588719b09f147e7cd4acbeacdb299`，本节是接收后的追加，不追改测量输入。

生产读取/验证及 fixture 修正净增 272B；加入当前任务包绑定 114B、F8 八叶固定保护 266B，已测必要子集共 278847B，比 278528B 上限高 319B。尚未计入独立检查脚本绑定、完整 FIX09 根/身份/校验与封存路由、M11/更正 F8 sidecar 保护；因此该值只是下界，不是完整候选或充分的新上限。不能通过删保护来满足原上限。

唯一预算回执 `loc-m17-fix09-command-receipt.json`：18449B，SHA256 `f93f26bc85179beb7649fd702c41e588560d75dafd58befcc572b56fc098b34f`，含 4660B 准确文本差量（SHA256 `07aade9acffd3bde8c74f5e9d393c310de77945589453540156b473f8a25764c`）。主程实核原工具 `exec-8901b81c-2fb1-4518-9334-48c8cd3a5dfc`：exit0、8ms，完整命令/未截断输出与回执逐字一致；仅文本替换计数，不是静态测试。原 F8 driver SHA 未变，FIX09 driver 缺席。静态检查、封存、host、restore、下载、Unity 和 Git 写入均 0。

本轮结论 **NEEDS_FIX**：根因及最小修正方向已确定，但 SOURCE 候选受尺寸合同阻塞，未实现/验证/封存，也未送新 head 的 GitHub 独立审查；不能接收为完成。交中央的唯一所需行动是处理候选尺寸边界（先完成必要差量的有界文本计量，再确定足够且有限的新上限）。本轮没有更改任何预算，材料 M 仍未通过。

中央随后反馈扩大上限的指令已被自动审批拒绝，须另获用户授权；278528B 上限保持。按其补充要求，主程只再快速只读检查一次既有常量、`_fix05_protection`、`_fix08_prior` 和 `prepare`：既有 prior helper 固定绑定 F7/M09，不能只替换为 F8 就保持旧保护；原文档常量也是历史必核输入，不能覆盖为新任务说明。没有发现可直接复用且明确覆盖完整 FIX09、能在剩余61B内完成绑定的简单改动。仅当前新文档固定绑定114B已大于该余量，余下保护/检查入口仍必需。未做压缩、无关基线整理、生产逻辑外移或追加检查/封存/执行；停止于上述真实预算回执。此结论是限定快速可行性判断，不声称不存在任何重新设计的较小实现。
