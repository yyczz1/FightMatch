# LOC-LIC-ALT legacy package license evidence correction

2026-10-01（Asia/Shanghai） · 状态：`READY_FOR_INDEPENDENT_R_FIX_001`

## 0. 范围与固定输入

本文只修正 LOC-LIC-ALT 的一个证据假设：官方旧版 NuGet binary package 未使用后来出现的
`<license>`／repository 元数据时，如何证明许可证。它不改变包版本、依赖闭包、签名验证、
Luban／YamlDotNet 源码提交、runner、restore、material、B/L 或产品范围。

- owner／回传：工程主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 原系统设计：`engineering-localization-license-alternative-system-design.md`，881 lines /
  59,206 B / SHA-256 `18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`。
- 原架构修订：`engineering-localization-license-alternative-architecture.md`，681 lines /
  49,110 B / SHA-256 `3dba4f92c17cc50fc158963857a69f70d308a8dab36dcbdf3a9fbbf821a06cb7`。
- M10：`engineering-loc-license-alt-m-materials-continuation-10.md`，236 lines /
  13,786 B / SHA-256 `cc8c023f5d18dae95f8df90d785876a3405ee1798f5f6915095931ff5c777b26`。
- M11：`engineering-loc-license-alt-m-materials-continuation-11.md`，130 lines /
  6,407 B / SHA-256 `88cfffa8cf0c8f29f6480501e4583eb4bab5027459f390fb506bd5431f7e2496`。
- M11 失败回执：`result.json`，3,471 B / SHA-256
  `bf9c0d4b856cb8e29c2109e71296c8521f9d6d99d27603867c569ddac7d1dfc0`；失败 authority
  3,642 B / SHA-256 `8fbdb262f60b397acf2119bddda23fdf2b5fc6728d813eec2d34a2e9d7b52624`。
- 官方 NuGet `.nuspec` reference：
  `https://learn.microsoft.com/en-us/nuget/reference/nuspec#licenseurl`。该文档把
  `licenseUrl` 标为 deprecated，并说明现代 `license` 元素从 NuGet 4.9 开始支持；
  repository 的 type/url/branch/commit 均为 optional。deprecated 不等于旧包无许可，optional
  字段缺失也不能单独成为拒绝理由。

本文若经原独立 R `ACCEPT`，只对 §1.1 定义的原样官方 binary-package lane，精确取代
M10 §4.2／M11 §4、原 ARCH §6.2、原 SYS §8 以及 M 的 material/license manifest 中以下冲突：
“缺 repository commit 必然失败”和“`sourceCommit` 只能填写源码提交”。§1.2 的 source-rebuild
lane，以及包内无许可正文而转向 repository raw license 的 lane，仍要求真实 exact full
commit/path；其它条款继续有效。当前不授权 M12、下载、验签、restore、material 或 B/L。

## 1. 两条证据通道必须分开

### 1.1 原样使用的官方 binary package

进入 local feed 的 `.nupkg` 若逐字节保持官方包，不要求它补交发布年代不存在或非必填的现代字段。
每包仍必须同时满足：

1. registration leaf 的 exact id/version/packageContent及 catalogEntry；catalog 的
   id/version/packageHashAlgorithm=`SHA512`/packageHash 与下载包一致；
2. archive bytes/SHA-256、nuspec id/version/dependency groups及 package files 均记录；
3. `.signature.p7s` 存在，固定 SDK `dotnet nuget verify --all` 成功；
4. 许可证据满足下列至少一种、并保存实际正文而不是只记标签：
   - nuspec `<license type="expression">`；
   - nuspec `<license type="file">` 与包内同名文件；
   - legacy `<licenseUrl>` 加包内 license 文件，文件正文可机械归类为批准的许可证；
   - 若包内无许可正文，则 metadata 给出 official repository exact full commit与路径，并取得该
     不可变提交的 raw license正文。
5. 归类结果须记录 evidence lane、正文相对路径或不可变 URL、bytes、SHA-256、规范许可证名，以及
   package identity；包内 `THIRD-PARTY-NOTICES*`／`NOTICE*` 若存在，必须连同完整正文、路径、
   bytes与SHA-256一起保留并进入最终license manifest。只看到可变 `master` URL、网页标签或包名
   仍不足；但 package内实际许可正文与已验证签名包绑定时，不再额外强迫 repository commit。

`licenseExpression == null`、缺 `<license>` 或缺 repository字段本身不再是 blocker；只有上述四种
证据通道均不能成立时才 `BLOCKED_LICENSE_EVIDENCE`。

#### 1.1.1 material／SBOM 的固定投影

embedded-license binary lane 不留空字段，也不伪造 source commit。对
`identity/license-files.tsv` 及 M/B/L 消费者，逐 package/license entry 使用以下固定投影：

- `component`／`version` 必须与 `package-files.tsv` 的 exact `packageId`／`version` 相等；
- `sourceUrl` 必须等于已封存 registration leaf 的 exact `packageContent`，且与 M10 固定的
  official nupkg URL相等；catalog URL只能取自同一 leaf 的 exact `catalogEntry`；
- `sourceTag` 固定为字面量 `N/A_OFFICIAL_BINARY_PACKAGE`，`sourceCommit` 固定为字面量
  `N/A_EMBEDDED_LICENSE_ENTRY`；这两个 sentinel 只允许用于本节 lane，不表示空值、版本、tag
  或 commit，包内 `version.txt` 也不得替代它们成为 source commit；
- `sourcePath` 是区分大小写的 nupkg ZIP entry path，`localRelativePath` 是保存的正文副本；两者
  bytes／SHA-256 必须相等。license row 的 `expression` 写机械归类后的规范许可证名；每个
  `THIRD-PARTY-NOTICES*`／`NOTICE*` entry 也必须形成 `expression=NOTICE` 的独立行；
- `packageRelationshipEvidence` 使用固定、可解析的 ASCII 值
  `binary-embedded|<graph>|<packageId>|<version>|<registrationReceiptRelativePath>|<packageContent>|<catalogUrl>|<catalogReceiptRelativePath>|<catalogSha512>|<packageSha256>|<entryPath>|<entrySha256>|<signatureReceiptRelativePath>`；
  各分量不得包含 `|`。registration receipt 必须证明 exact id/version/packageContent/catalogEntry，
  其中 `catalogEntry == catalogUrl`、`packageContent == sourceUrl`；catalog receipt 必须证明同一
  id/version、`packageHashAlgorithm=SHA512`及原始 base64 `packageHash == catalogSha512`，再以该
  SHA512和 `packageSha256`绑定实际 nupkg。由此逐项连接同一 `package-files.tsv` 行、ZIP entry
  manifest和已接受的签名回执；不要求或补造 catalog 中不存在的 `packageContent`。
  `packageSha256`／`entrySha256` 为64位小写hex；所有 receipt 路径均为 material root-relative
  POSIX path。

SBOM 中同一 component row 必须保留 `evidenceLane=binary-package/embedded-license`、上述 exact
registration packageContent/catalogEntry、catalog id/version/SHA512、package SHA-256、signature
verdict/receipt、license/notices entry path/bytes/SHA-256 与归档正文路径。M 的
`package-files.tsv`、`package-entries.tsv`、
`license-files.tsv`、SBOM 和未来 B/L inventories 以 packageId/version/package SHA-256 为 join key
做 exactly-once 集合核对；两个 sentinel 只允许用于 `System.Reflection.Metadata 1.6.0`、
`System.Collections.Immutable 1.5.0`、`xunit.abstractions 2.0.3` 中实际满足 embedded lane 的包；
任一 sentinel 出现在 §1.2、raw-repository lane 或其它包，任一 join、正文、notice、签名回执缺失，
均 fail closed。由此 `System.Reflection.Metadata 1.6.0` 在
未来签名门通过后可完成 material/SBOM；同样缺 commit 的 source rebuild，或无包内正文且无
不可变 repository source 的包，仍必须阻断。

### 1.2 自行修改／重建的源码

Luban本体、YamlDotNet replacement及任何未来会被项目修改或重建的源码仍沿用原严格合同：
official repository URL、full commit、精确 source/license path、source hash、patch diff与构建身份
缺一不可。本修正不得把 binary-package通道外推到源码重建，也不得伪造不存在的 commit。

## 2. M11 具体事实与允许的 M12 延续

M11 已验证并封存 `System.Reflection.Metadata 1.6.0`：

- req-01 registration：791 B / SHA-256
  `9ae28ac42b0927fdc3930ff72abaf60fc4f00086ab60d408949c9ec9d5146d0b`；
- req-02 catalog：11,516 B / SHA-256
  `ee0ebaa566d832502f2e388eeddff89619286c78b5e6cd3719ed98978290f918`；
- req-03 nupkg：852,113 B / SHA-256
  `2497e068f6afed47c4878c9101074684b645d5baebd2d5163e5eaa99f356abf1`；
- catalog SHA512 与包体匹配；nuspec legacy `licenseUrl` 指向 dotnet/corefx `LICENSE.TXT`；
- 包内 `LICENSE.TXT` 为 1,139 B，SHA-256
  `d7a68596ab69b06f51ca278a6545148e4269a9381c26d597c13df5d88e08cf5b`，正文明确为 MIT；
- 包内 `THIRD-PARTY-NOTICES.TXT` 为 15,835 B，SHA-256
  `7864a01e2fdef7e8fdf81b906efb1466f083206affea7ba7e6dadea429754765`，须完整保留；
- 包内 `version.txt` 是 full SHA `30ab651fcb4354552bd4891619a0bdd81e0ebdbf`；这是附加来源线索，
  不是让旧 binary lane成立所必需的伪造 repository 元数据。

因此 M12 可在先重验上述缓存身份与包内正文后，把该包暂记为
`binary-package/embedded-license/MIT`；它在 §2.1 的签名回执被独立接收前仍为
`SIGNATURE_PENDING`，不得重请求 req-01..03，也不得进入 restore/material。

M12 对剩余两个固定包继续只允许既定 registration/catalog/nupkg。每包先检查现代 license，随后
检查包内许可正文；只有包内正文缺失且 metadata 有 exact official commit/path时，才允许一个
对应 raw license请求。由此：

- M12新网络必须为 6–8 requests；combined chain为 9–11 requests；
- host仍限 `api.nuget.org`、`raw.githubusercontent.com`，redirect exact 0，总响应仍 `<=5 MiB`；
- 少于11不是缺证据，只能表示一个或两个 package由签名包内正文完成许可闭环；
- 超过11、探索搜索、GitHub API、重复 req-01..03、无 exact commit却拼 raw URL均禁止。

其它 M10/M11 gates保持：三包固定版本、无第4包、每包签名、tests feed 31包、runner restore 0、
tests generation/locked verification各1次、13 nodes/29 edges/13 locks、失败即停、material最后
create-once、旧roots只读。

这里的 9–11 requests／5 MiB 是 **Mac acquisition ledger**，不得因后续 cloud verify 重写。
M12 应在同一白名单与该预算内先完成其余两包的 registration/catalog/nupkg、完整性及各自实际
license/notices 形态收集；某一包的 license mapping 尚未闭合时，只标记该包 pending，仍可完成
其它固定包的只读收集。HTTP、identity/hash、host/redirect 或预算硬失败仍按 first-failure stop；
三包收集结束后只要任一 license 未闭合，整体即阻断 restore/material，不能带病进入执行阶段。

### 2.1 三包 Linux 签名门与等待边界

M11 已明确把 signature verification 列为 not-run；`.signature.p7s` 存在只证明 archive entry
存在，不是验证成功。Mac .NET 8 的既有 `NU3003`／`CSSM_ModuleLoad` 失败保留为历史事实，M12
禁止在 Mac 重试 `dotnet nuget verify`。三个 exact packages 都必须分别进入既有 durable Linux
云路线，使用固定 SDK `8.0.425/linux-x64` 与既有 trust／隔离／revocation 解释执行唯一
`dotnet nuget verify --all <exact-nupkg>`；不得安装或下载另一个 SDK、改 trusted roots、运行
Unity，或把 YamlDotNet 16.3.0 的 Linux004 verdict 复用为这三包的 verdict。

顺序固定为：M12 先封存三包 registration leaf 的 id/version/packageContent/catalogEntry、对应
catalog 的 id/version/packageHashAlgorithm/packageHash、nupkg bytes/SHA-256及 license/notices
entries；随后状态置为 `WAITING_SIGNATURE_REVIEW` 并停止 restore/material；
再由独立测试链签发并执行 Linux 验签包。优先用 M12 后续短包明示的 hash-preserving handoff
传入已封存 bytes；若 durable 环境不能机械接收本机 bytes，允许 cloud 仅从三条已冻结的 official
nupkg URL各 GET 至多一次。cloud ledger 独立封存为 `<=3 requests`／总响应 `<=5 MiB`，零
registration、catalog、raw-license、SDK 或其它网络；每个 cloud nupkg必须与 Mac 已冻结的 exact
id/version/bytes/SHA-256及 catalog SHA512相等后才可验签。Mac ledger仍为 9–11 requests／5 MiB，
不得重写或把 cloud请求强塞进去；跨端 combined 上限为 14 requests／10 MiB。超过任一分账或
combined上限、第四个包、非固定 URL、redirect 或 identity不等，立即 fail closed。cloud 的三次
GET是条件上限，不得在 handoff已成功时仍重复下载；固定 Linux SDK必须复用，下载次数为零。

Mac 接回时逐包核对 exact id/version/package bytes/SHA-256，并保存 Linux SDK identity、exact
argv、exit、raw stdout/stderr、errors/warnings、`.signature.p7s` identity、evidence manifest与
独立 R verdict。只有三包均由对应独立 R 接受后，才把各自 `signatureVerdict` 从
`SIGNATURE_PENDING` 改为限定结论；`NU3018`／`NU3028` 等 warning只能按实际回执限定表述，
不得声称在线吊销检查成功或证书当前未吊销。任一云任务、回执、identity或独立接收缺失即保持
等待／阻断；禁止继续 restore、material 或后续接收。

## 3. 独立 R 验收

R 只读确认并给唯一 `ACCEPT`／`NEEDS_FIX`／`REJECT`：

1. 没有把 metadata字段缺失误写为许可缺失，也没有降低 immutable package/hash/signature门；
2. binary package与source rebuild两通道不混用；
3. 包内许可正文必须真实保存、哈希并机械归类，不能只信 `licenseUrl`；
4. embedded lane 已机械接入 license-files/SBOM/M/B/L，sentinel 不会外溢到其它 lane；
5. 三包独立 Linux 验签及 `WAITING_SIGNATURE_REVIEW` 门不会复用旧 verdict或重试 Mac；
6. Mac 9–11/5 MiB、条件 cloud 0–3/5 MiB 及跨端最多14/10 MiB 分账明确且不漏算；
7. 本修正不授权执行，`ACCEPT` 后仍须主程另签短 M12 delta packet。
