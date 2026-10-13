# LOC-LIC-ALT-M12 — 旧包许可材料续作与三包 Linux 签名门

2026-10-01（Asia/Shanghai） · `PREPARED_NOT_AUTHORIZED`

本包只准备 DELTA；中央解除冻结、绑定本包 activation 与实际审查 head/执行路线后才向 LOC 激活。此次没有下载、验签或 restore。
owner：LOC-A `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`；回主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。
Linux 验签由主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local` 另行签给既有 durable owner `01a0f13c-3d87-753a-ad11-16302add9084`。
authority：中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 本轮准备指令与 AGENTS §6；主要执行使用 `gpt-6-astra/xhigh`。当前 [TEAM_WORKFLOW §7](../../../.agent/TEAM_WORKFLOW.md#7-github-pr-code-review) 覆盖旧包本地 R/模型条款；新代码审查由 GitHub PR 完成，主测试独立核验实际签名证据。

## 1. 继承与固定输入

只替换 [M11](engineering-loc-license-alt-m-materials-continuation-11.md) 的许可证据假设、根路径和签名等待点；闭包、runner复用、restore及material合同继承 [M10](engineering-loc-license-alt-m-materials-continuation-10.md) §§1、3、5、6。
必须应用已获设计 ACCEPT 的 [旧包许可证据修正](engineering-localization-legacy-package-license-evidence-correction.md) §1–2.1：13,648 B，SHA `86941725f20168e5c51583ca17af4578ef25b89850d351d0d6c4ff0bfc85c870`；原R turn `01a0f5af-650d-7033-955e-467029947d07` 的结论只证明设计，不证明材料。
M10 SHA `cc8c023f5d18dae95f8df90d785876a3405ee1798f5f6915095931ff5c777b26`；M11 SHA `88cfffa8cf0c8f29f6480501e4583eb4bab5027459f390fb506bd5431f7e2496`。
M11 `result.json` SHA `bf9c0d4b856cb8e29c2109e71296c8521f9d6d99d27603867c569ddac7d1dfc0`，`evidence-files.tsv` SHA `d1c8e8e83bd5c6e4061d6a96a4c604fe00c6e93cf02cdf5e9cf073093e2957b0`；状态永久保留 `BLOCKED_LICENSE_EXPRESSION`。

`T11 = /private/tmp/fightmatch-loc-lic-alt-m11/loc-lic-alt-yamldotnet-16.3.0-m11`。以下只读导入，不能重新请求：

| T11下输入 | bytes | SHA-256 |
| --- | ---: | --- |
| `acquisition/packages/system.reflection.metadata/1.6.0/registration.json` | 791 | `9ae28ac42b0927fdc3930ff72abaf60fc4f00086ab60d408949c9ec9d5146d0b` |
| 同目录 `catalog.json` | 11516 | `ee0ebaa566d832502f2e388eeddff89619286c78b5e6cd3719ed98978290f918` |
| 同目录 `system.reflection.metadata.1.6.0.nupkg` | 852113 | `2497e068f6afed47c4878c9101074684b645d5baebd2d5163e5eaa99f356abf1` |
| `acquisition/combined-network-requests.jsonl` | 以实际封存行核 | `36470f78fd6593e6cdc5c70e65c2aca8bf07f0b34069c14b37c3dbd4e6abebc4` |
| `preflight/m11_acquire.py`（只作派生输入） | 20663 | `f71c3adc0475f068206e350648865f5a1b8b9627503d1821103c0547f1f7b49d` |

旧 req-01..03 为3次/864420 B/0 redirect。SRM包内 `LICENSE.TXT` 1139 B/SHA `d7a68596ab69b06f51ca278a6545148e4269a9381c26d597c13df5d88e08cf5b`，`THIRD-PARTY-NOTICES.TXT` 15835 B/SHA `7864a01e2fdef7e8fdf81b906efb1466f083206affea7ba7e6dadea429754765`，正文全部保留；验签前只能 `SIGNATURE_PENDING`。
M09 runner沿 M11 frozen table逐项验证复用：23 nodes/56 edges、23 locks及raw/canonical对等、931-file patched source；tests graph13/29、旧feed28包。M11 checker/table SHA分别 `3ba975931686729800f5704cd3658b5bf5c20a20791f83a7a854a175f4c8ceaf` / `3e51974a8237b7cc8168123fd7f5e4bd5a1d05383e0d4fbd715afcfe4819651b`。不重跑runner。

## 2. 新写域、预检与停止点

仅以下三个新根；旧M01–M11、005、生产八路径、Config/Generated/Tools、Assets、包/设置与Git均只读：

```text
T = /private/tmp/fightmatch-loc-lic-alt-m12/loc-lic-alt-yamldotnet-16.3.0-m12
E = /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m12
M = /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m12
```

激活后先核T/E/M全 absent及新根所需free-space≥2 GiB，再只创建T/E一次。T允许 `preflight/{activation.json,m12_driver.py,m12-inputs.tsv,preflight.json,fixtures.json}`、`acquisition/`、`imported-runner/`；后续签名门通过并另收续行回执后才开放 M10既定 `local-feed/`、`tests-generation/`、`tests-verification/`、`stage-material/`。M必须保持absent直至最终seal。
E只允许 `authority/{activation,preflight,signature-handoff,signature-receipt,first-failure}.json`、`process/{completed-commands,not-run-commands}.tsv`、`acquisition-files.tsv`、`network-ledgers.json`、`old-root-equality.json`、`result.json`、`evidence-files.tsv`及一次性 `FAILED_EVIDENCE`；等待阶段回执写 `authority/signature-handoff.json`，最终result在真正结束时一次落盘。
唯一新支持文件 `T/preflight/m12_driver.py` 由已封存M11 driver派生；delta仅为新根、req01..03复用、批准的binary-license投影、签名等待和M10续行门。记录源码/差异SHA；旧driver不改不重跑。
driver的只读preflight重放M11 table所有内容身份门；仅将已过时的 `m11-roots absent` 行替为此前记录的T/E/M入口absent证明，新增M11十个temp叶及其evidence封存身份、本文和许可修正身份。旧M11 checker不能直接运行后把roots-exist失败豁免为PASS。新inputs逐项记录原行来源、差异理由与实际bytes/SHA；任一内容门失败停止，不复制旧TOTAL冒充新验证。
fixture先离线复用M11 string/object成功证据并核新parser结果；覆盖legacy embedded MIT+NOTICE投影、sentinel误用/缺正文/缺关系/缺签名拒绝。fixture通过后冻结driver，才允许网络；首败后不在同根修driver或重试。

## 3. 剩余 acquisition 与双账预算

只补 `System.Collections.Immutable 1.5.0` 和 `xunit.abstractions 2.0.3`，三包固定集合不变。
每包的 registration与nupkg URL严格使用 M10 §4.1 表；catalog URL仅取对应真实registration的string或object `@id`，先绑定exact id/version/packageContent/catalogEntry，再核catalog SHA512和nupkg原字节。SRM已得三响应及ledger逐字复用并标 `newNetworkRequest=false`。
req-04..09为两包各registration/catalog/nupkg；只有包内无许可正文、metadata已给official repository exact full commit/path时，才可追加对应raw license请求req-10..11。现代字段缺失本身不阻断；license pending仍可收完另一个固定包。HTTP、身份/hash、redirect、预算失败立即停止。

| 账本 | 请求/响应总量上限 |
| --- | --- |
| M12 Mac新增 | 6–8次，剩余响应≤`5242880 - 864420 = 4378460` B |
| Mac完整req01起的chain | 9–11次，≤5 MiB，0 redirect |
| 条件cloud nupkg GET | 0–3次，≤5 MiB，0 redirect；独立账本 |
| 跨端combined | ≤14次/10 MiB；复用传输不能重复计为HTTP或漏记实际GET |

Mac host仅 `api.nuget.org`/`raw.githubusercontent.com`；registration/catalog各≤256 KiB、Immutable nupkg≤1 MiB、xunit≤256 KiB、raw license各≤128 KiB；请求≤60s，保留真实URL/HTTP/headers hash/body bytes/hash/时序。redirect/探索/第4包不在范围。
逐包保存nuspec、dependency groups、完整ZIP entry manifest、`.signature.p7s`、license和全部NOTICE正文；safe entry提取保持原字节。binary embedded lane的两个sentinel、registration→catalog→nupkg→entry→signature关联字段，精确按许可修正 §1.1.1投影到license-files和SBOM；source rebuild/raw repository lane仍要求真实full commit。
全部收集后，只要license或三包闭包不能闭合，回对应blocker；否则封存 `SIGNATURE_PENDING` 三包清单并回 `WAITING_SIGNATURE_REVIEW`，此时runner/tests restore均0，M absent，停止本地执行。

## 4. 独立 Linux 三包签名门

handoff写明三包exact URL/id/version/bytes/SHA256/catalog SHA512、signature entry/许可manifest身份、Mac累计ledger及owner turn。优先通过中央/主测试核准的传输逐字交付；传输不可用才允许durable对三条已冻结nupkg URL各GET至多一次。cloud接收后先逐项比Mac身份，成功handoff时GET=0。
使用现有 `/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-VERIFY-004/sdk/dotnet`；固定SDK8.0.425/linux-x64及原004文件manifest、实际SDK身份须复核，下载/重装SDK次数0。原YamlDotNet验签结论仅为环境先例，不代替三包结论。
主测试另签新create-once cloud root、实际owner turn和有限文件清单后，每包一次如下argv：

```text
<fixed-004-sdk>/dotnet nuget verify <new-cloud-root>/packages/<exact-id.version>.nupkg --all --verbosity detailed --configfile <new-cloud-root>/process/NuGet.Config
```

继承 [Linux004](testing-loc-license-alt-signature-linux-004.md) §4–5及[A01](testing-loc-license-alt-signature-linux-004-addendum-01.md)的独立子进程缓存/临时目录、最小config、证书/PATH副作用禁用、真实用户目录保全与trust边界。每包≤180s，一次顶层进程树，自然收尾；无Mac验签重试。不得更改HOME/CODEX_HOME、trusted roots或全局配置。
回执须含SDK实物identity、每包exact argv/exit/raw stdout/stderr、errors/warnings、signature hash、manifest、cloud ledger及主测试独立证据结论。0 error且实际成功才能关闭对应签名门；NU3018/NU3028如实际出现，按004限定解释，不宣称在线吊销检查成功。
三包中任一缺实际成功、身份不等或主测试未接收，保持等待/阻断。验签实现脚本的代码审查绑定对应GitHub PR/head；不启动本地R，不把uGUI PR审查当作LOC审查。

## 5. 签名后的条件续行和交付

只有主测试交回三包有效签名证据、主程发准确resume receipt后，才在同一已封存M12材料上继续M10 §5：旧feed28+新3=31；runner restore=0；tests offline generation一次、独立 `--locked-mode` verification一次，13 nodes/29 edges/13 locks及raw/canonical/marker集合门保持。
两条restore的fixed SDK、单节点/隔离/离线argv由已接受M09/M10合同导出，仅替换本包root和31包feed；启动前保存exact argv及输入SHA，不复用M09失败tests lock/assets。任何阶段首败停止，禁止补包/重试/build/test/publish/Luban/005/B/L/Unity。
license-files/package-files/package-entries/SBOM以exact id/version/packageSHA exactly-once join；三包signature receipt进入material，完整NOTICE有独立行；seal temp及old-root equality通过后才create-once M。
沿M10预算：temp≤768 MiB、evidence≤16 MiB、每次restore≤180s、free-space起止≥2 GiB；Mac实际工作墙钟合计≤30min（签名等待日历时间另报），原材料预算保持。超限如实封存，不能删证据或重签同根。
最终回主程实际thread/host/turn、activation/PR head、driver/input身份、两账本、三包完整关系/许可/签名、runner复用、restore0/1/1、13/29/13、material/old-root seals及未运行项。作者最多 `READY_FOR_QA_AND_GITHUB_REVIEW`；主测试接收实际证据、GitHub完成对应代码审查后才能形成M接收，不能自动进入B/L或产品阶段。
