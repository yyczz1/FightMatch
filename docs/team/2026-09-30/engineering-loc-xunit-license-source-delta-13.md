# xunit.abstractions 2.0.3 许可来源研究与 M13 最小 delta

2026-10-01 · `PREPARED_NOT_AUTHORIZED`；研究完成，建议下述限定修订；M12 仍保持原失败，本文不是接收 verdict。
delivery owner：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。authority：中央本轮限定研究指令及 AGENTS §6 已获用户批准的团队协作；模型 `gpt-6-astra/xhigh`。
research 技能要求后台研究：`/root/xunit_license_source13` 负责8次查询，主程负责4次及收件；无其它任务派发。证据根 `R13=TestArtifacts/FightMatch/LOC-LIC-ALT-M/xunit-license-research-13/`，唯一文档为本文，旧根与生产路径只读。

## 1. 固定包与问题

主程本地核验的官方包为 `xunit.abstractions/2.0.3`，75,155 B，SHA-256 `d03d72fc2df8880448f7a81bddb00e1bcb5c18f323c7e7cc69b4cfa727469403`；包内 nuspec 1,385 B，SHA-256 `e59191df9e3047dd953b695c886597786ac2771f66897baa40877f643da9f7b1`。包内无许可正文、无 repository 元素；签名仍 `SIGNATURE_PENDING`。[本地逐字观测](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/xunit-license-research-13/local-package-observation.json)
其原始 `projectUrl=https://github.com/xunit/xunit` 与 `licenseUrl=https://raw.githubusercontent.com/xunit/xunit/master/license.txt` 是官方仓库／路径关联起点；可变 master 本身不作为不可变许可正文或构建来源。本轮不重新获取 nupkg。[本地观测](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/xunit-license-research-13/local-package-observation.json)

## 2. 官方 tag 检查：存在明确限制

官方仓库 `xunit/abstractions.xunit` 实际列出 `refs/tags/2.0.3`，关联 annotated tag `aafc368765b30c3c9ed2e953a61fe084c078302f`。[官方 refs](https://api.github.com/repos/xunit/abstractions.xunit/git/refs/tags)
该 tag 指向 full commit `489a2b3949a9ae8587201a22a8ecfc1d791fa1f3`；tagger 日期为 `2023-06-17T07:23:09Z`，GitHub verification 为 `unsigned`。不能把后补 tag 日期当作包发布日期。[不可变 tag object](https://api.github.com/repos/xunit/abstractions.xunit/git/tags/aafc368765b30c3c9ed2e953a61fe084c078302f)
对应 commit 为 `2017-02-17T17:50:51Z` 的 `Initial revision`，parents 为空；根目录未列出 license 文件。[commit](https://github.com/xunit/abstractions.xunit/commit/489a2b3949a9ae8587201a22a8ecfc1d791fa1f3)；[固定目录](https://api.github.com/repos/xunit/abstractions.xunit/contents?ref=489a2b3949a9ae8587201a22a8ecfc1d791fa1f3)
该 commit 的 `xunit.abstractions.nuspec` 实际写 `<version>2.0.1</version>`；它再次指向同一 xunit/xunit projectUrl 和旧 licenseUrl，不能被改称为 2.0.3 nuspec，也不证明已封存 2.0.3 二进制从此提交构建。[固定 nuspec](https://github.com/xunit/abstractions.xunit/blob/489a2b3949a9ae8587201a22a8ecfc1d791fa1f3/xunit.abstractions.nuspec)
该 nuspec 的 connector 文本保存为 `agent-05.xunit.abstractions.nuspec.connector-text`，1,758 B，SHA-256 `3d6a2299702b099df64f9b1b366c87c78a5d63e7f1dd27f150c951022eb1198e`；本地 `git hash-object` 得 `2fa68b7b6eac1fec67168c272c7fd253d7c50803`，与官方目录 blob SHA 相等，故这份保存正文已核为精确 Git blob 字节。[官方 blob 关联](https://api.github.com/repos/xunit/abstractions.xunit/contents?ref=489a2b3949a9ae8587201a22a8ecfc1d791fa1f3)
官方 releases 集合返回 `[]`；限定 xunit.net 的版本查询也无结果。这仅描述本轮响应，不推断所有官方网页都不存在。[releases](https://api.github.com/repos/xunit/abstractions.xunit/releases)

## 3. 找到的不可变许可与版本关联

官方 `xunit/xunit` commit **`ffee51ac171a66896c42d00486bc792470ed6da2`**（2018-08-25T20:13:24Z）题为 Update to xunit.abstractions 2.0.3；实际 diff 的 `src/xunit.core/xunit.core.csproj` 及 `src/xunit.extensibility.core.nuspec` 把该依赖从2.0.2改为2.0.3，不是仅凭标题推断。[官方 commit/diff](https://github.com/xunit/xunit/commit/ffee51ac171a66896c42d00486bc792470ed6da2)
在**同一完整 commit** 的 `license.txt` 取得官方文件：默认代码声明 Apache-2.0；分隔线后另附两个指定导入目录的 MIT 通知。保存整个文件（含范围限定），不能因其中有MIT全文就把 xunit.abstractions 归成MIT，也不能推断它包含那两个目录。[固定许可正文](https://api.github.com/repos/xunit/xunit/contents/license.txt?ref=ffee51ac171a66896c42d00486bc792470ed6da2)
保存正文 `R13/xunit-xunit-ffee51ac-license.txt`：**2,357 B，SHA-256 `e9dcf94cc4864ba47aeaaec879cfabaf3feadb813c77af480c3ad14c6de0ed79`**；逐字等于 `root-04.response.json` 的 connector content 按UTF-8编码。它是完整官方文件，但Apache部分是许可声明／链接，并非九节标准条款全文；HTTP原字节与远端Git blob SHA未暴露，不能宣称这两项已核。
`root-01` 历史表显示 license.txt 在2019年改名、2020年删除；可解释旧master链接失效，不把本轮看到的master或历史日期冒充包构建提交。[官方路径历史](https://api.github.com/repos/xunit/xunit/commits?path=license.txt&per_page=10)
**推断边界：**原包自身的 exact id/version、projectUrl、licenseUrl → 同一官方仓库／许可路径 → 同提交明确使用2.0.3的事实及许可声明，足以提出“原样binary的官方许可快照”设计修订；不是源包等价、发行tag或可重现构建证明。§2的后补tag只作限制证据，不参与肯定关联。
M12 `BLOCKED_LICENSE_EVIDENCE` 永久保留；三包签名仍pending、restore均0、material absent。本轮仅补充许可证据，不改变上述状态。[M12合同](engineering-loc-license-alt-m-materials-continuation-12.md)；[原许可设计](engineering-localization-legacy-package-license-evidence-correction.md)

## 4. agent 实际请求账本（8/8，已停止网络）

| 请求 | exact URL／query | 实际结果／保存响应字节 |
| --- | --- | --- |
| agent-01 | `https://api.github.com/repos/xunit/abstractions.xunit/tags?per_page=100` | connector `INVALID_ARGUMENT`，仍计一次；401 B |
| agent-02 | `https://api.github.com/repos/xunit/abstractions.xunit/git/refs/tags` | 成功；3,547 B |
| agent-03 | `https://api.github.com/repos/xunit/abstractions.xunit/git/tags/aafc368765b30c3c9ed2e953a61fe084c078302f` | 成功；2,253 B |
| agent-04 | `https://api.github.com/repos/xunit/abstractions.xunit/contents?ref=489a2b3949a9ae8587201a22a8ecfc1d791fa1f3` | 成功；12,999 B |
| agent-05 | `https://api.github.com/repos/xunit/abstractions.xunit/contents/xunit.abstractions.nuspec?ref=489a2b3949a9ae8587201a22a8ecfc1d791fa1f3` | 成功；4,530 B |
| agent-06 | `https://api.github.com/repos/xunit/abstractions.xunit/commits/489a2b3949a9ae8587201a22a8ecfc1d791fa1f3` | 成功；264,812 B |
| agent-07 | `site:xunit.net "xunit.abstractions" "2.0.3"`，domains=`["xunit.net"]` | web 查询无结果；71 B |
| agent-08 | `https://api.github.com/repos/xunit/abstractions.xunit/releases` | 成功，空集合；578 B |

每次返回完整保存在 `agent-NN.response.json`；这是工具对象／web 字符串的本地 JSON 序列化，**不是原 HTTP wire bytes 或 headers**。8份响应共 289,191 B；另 source nuspec 1,758 B 与 `agent-ledger.json` 2,948 B，agent 文件合计 293,897 B（小于 4 MiB）。connector 隐藏的内网传输／重定向次数未暴露，不能捏造成 0；预算按获准 URL/query 工具请求计。
主程实际4/4请求如下；完整响应为 `root-01..04.response.json`，含失败的全团队计数严格 **12/12，已停止网络**。这里只记可观测工具请求，不捏造HTTP级统计。

| 请求 | exact URL／query | 实际结果／保存响应字节 |
| --- | --- | --- |
| root-01 | `https://api.github.com/repos/xunit/xunit/commits?path=license.txt&per_page=10` | 成功；79,772 B |
| root-02 | GitHub commit query=`2.0.3`，repo=`xunit/xunit`，topn=5，sort=committer-date，order=asc | 成功；4,765 B |
| root-03 | `https://api.github.com/repos/xunit/xunit/commits/ffee51ac171a66896c42d00486bc792470ed6da2` | 成功；14,456 B |
| root-04 | `https://api.github.com/repos/xunit/xunit/contents/license.txt?ref=ffee51ac171a66896c42d00486bc792470ed6da2` | 成功；5,718 B |

12份完整响应合计393,902 B；另保存派生正文、观测、账本和哈希清单，总保存量以 `research-receipt.json` 为准且≤5 MiB。它是**独立research账**，绝不并入／改写M12 acquisition：M12新增6次/899,675 B，完整chain9次/1,764,095 B保持。nupkg／NuGet registration/catalog／SDK下载、restore、Unity、Git写操作均0。

## 5. 最小许可证据修订提案（需中央批准）

仅对§1 exact包增设 `evidenceLane=binary-package/official-version-license-snapshot`：允许包内旧licenseUrl结合§3官方不可变版本证据补足commit，而非要求该commit已写进nuspec；接受§3完整文件的精确哈希及其Apache声明作为本lane证据。旧embedded／source-rebuild规则不变；这项批准不自动授权执行。
`license-files.tsv`／SBOM投影：`component=xunit.abstractions`、`version=2.0.3`、`packageSha256=§1`、`sourceUrl=§3实际请求URL`、`sourceCommit=ffee51ac171a66896c42d00486bc792470ed6da2`、`sourcePath=license.txt`、`sourceTag=N/A_LICENSE_SNAPSHOT_COMMIT`（新lane唯一允许的字面量）；标 `sourceCommitRole=license-snapshot-not-package-build`、`licenseEvidenceKind=official-license-notice`、`expression=Apache-2.0`，正文bytes/SHA必须等§3。
`localRelativePath=licenses/official/xunit.abstractions/2.0.3/license.txt` 保存整个文件；快照中的MIT部分作为原始上下文保留，不删除、不外推其适用范围。新的sourceTag只表示无发行tag；本lane绝不使用embedded的两个sentinel，任何source-rebuild继续要求真实构建来源。
`packageRelationshipEvidence` 固定ASCII格式：`binary-official-snapshot|tests|<id>|<version>|<registrationReceipt>|<packageContent>|<catalogUrl>|<catalogReceipt>|<catalogSha512>|<packageSha256>|<nuspecPath>|<nuspecSha256>|<projectUrl>|<licenseUrl>|<licenseCommit>|<licensePath>|<licenseBodySha256>|<versionReceipt>|<signatureReceipt>`；分量禁含`|`，receipt均material-root-relative POSIX路径。所有值从已封存输入逐项核得，不能填猜测值。
registration→catalog→package SHA512/SHA256→包内nuspec hash/两个URL→root-03真实2.0.3 diff→同commit/path的root-04与正文hash必须全部join；另携带上述两份response本地bytes/SHA与保存表示。package-files/package-entries/license-files/SBOM以id/version/packageSHA exactly-once核对；signature receipt单独绑定同包，缺任一关系即fail closed。
分类器仅为此exact tuple增加显式分支，不能放宽通用正文规则或套用当前MIT-first全文匹配。离线fixture须覆盖正确snapshot、被改正文/包hash/版本/commit/path/URL/关系、误判MIT、source-rebuild冒用、embedded sentinel冒用及签名缺失；前者只到signature pending，其余拒绝。完整Apache标准条款另有归档要求时应引用已批准的标准条款输入；本轮没有获取或声称保存它。

## 6. 下一执行delta边界（准备稿，尚未派发）

owner仍LOC-A `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，唯一回主程；中央批准§5、指定activation/实际GitHub PR head后才激活。中央负责LOC PR，主测试负责三包Linux签名；主程本轮不派LOC/R、不创建PR、不执行签名。
新临时根 `T13=/private/tmp/fightmatch-loc-lic-alt-m13/loc-lic-alt-yamldotnet-16.3.0-m13`；新证据根 `E13=TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m13`；激活前须absent/create-once。只允许T13的 `preflight/{activation.json,m13_driver.py,m13-inputs.tsv,preflight.json,fixtures.json}`、`acquisition/`、`imported-runner/`，后两者按M12输入manifest逐叶复制；R13输入复制至 `acquisition/research-13/`，新增许可投影仅在新根。
E13仅 `authority/{activation,preflight,signature-handoff,first-failure}.json`、`process/{completed-commands,not-run-commands}.tsv`、`acquisition-files.tsv`、`network-ledgers.json`、`old-root-equality.json`、`result.json`、`evidence-files.tsv` 与一次性 `FAILED_EVIDENCE`；没有新material写域。旧T12/E12、研究根、旧drivers/inputs、生产代码/配置/资产/Git全部只读。
固定M12输入：result 23,708 B/SHA `b2eb0ed1f1fd59cb72557f1686a4bc7e07729609535c11af1dfeb19534370b21`；acquisition-files 6,336 B/SHA `ab57c50010e832f063d726c56cb3d9ea9b115a9de885945463bbfc7d2a818de5`；driver 42,263 B/SHA `6759dd14fc5dd77af385e0c3268a7e48dbdfd56e2b5ea4cc048bdb061de97b45`；inputs 34,598 B/SHA `703d54f52ceb3b5f8f1fa607f88d3f26e183fc5f3c8054d7a1fd7f5714b84ae1`。R13 inputs由其receipt/manifest及本文件交付hash锁定。
先逐项只读核M12全部输入与两根seal，再派生driver；delta只限根路径、已得三包/runner逐字导入、新lane投影、fixtures和停止点。**新增acquisition网络=0**（包、registration、catalog、license均不重取）；旧Mac账逐字复制并标import，research单列。冻结driver后只走离线材料预检；任一身份/fixture/范围失败首败停止，禁止同根改脚本重试。
成功标准仅为三包许可证据完整且签名pending的 `WAITING_SIGNATURE_REVIEW`；runner/tests restore **0/0/0**、material absent，封存三包exact身份与研究继承清单后停止。此delta不开启M12§5的restore；后续须主测试接收三包真实Linux签名、GitHub审查对应代码head及主程另签准确续行。
Linux仍沿M12§4固定现有SDK8.0.425/linux-x64，每包一次；优先hash-preserving传输，不能传输才条件≤3个固定nupkg GET/5 MiB。acquisition+cloud旧上限14次/10 MiB维持，其**不含独立research12次**，不得宣称全项目只有14次。无新SDK、Mac验签、Unity、第四包或B/L。
资源继承M12：temp≤768 MiB、evidence≤16 MiB、free-space起止≥2 GiB；本delta实际工作≤30min，无restore。回执需实际thread/host/turn、activation/PR head、driver/input身份、分账、三包许可/签名状态、old-root seals及未运行项；作者不能自给ACCEPT。

## 7. 本轮验证与剩余门

研究前后逐文件聚合SHA一致：T12 49files/2,016,358 B `b41704c1a95300be24696bfb1ddc0f67d46ddfc90580f6b9a2c5875276cb33b1`；E12 11files/79,526 B `b295137e7cd9236b4dc2e66e86fb0a69d5f231eeb3ebc071780f2192446b5eb8`，material仍absent。算法见 `research-receipt.json`；本轮不重跑历史validation。
剩余门是中央批准限定lane与签发实际执行，而非缺少包体或须重下载。若不接受“官方许可声明快照”作为该binary证据，保持 `BLOCKED_LICENSE_EVIDENCE`：最小替代为另授权一次带Git blob SHA/原字节的固定文件核验与标准Apache条款归档，或要求上游对exact包SHA发布明确许可声明；不再本轮搜索，不改包版本，不用tag猜构建来源。
