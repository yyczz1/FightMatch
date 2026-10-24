# LOC-LIC-ALT-M-10 — bounded three-package acquisition and tests-only closure

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标、责任与固定输入

本包是 M09 `BLOCKED_MATERIAL_GAP` 的 clean sibling。唯一目标是在不重跑已封存 runner
restore 的前提下，获取经 NuGet 官方元数据证明的三个固定缺失包，补齐 tests local feed，
重新执行 tests generation / locked verification 两次 restore，并在全部 identity、license、
signature、graph 与 equality 门通过后形成 M material 候选。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与收件：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：根目录 `AGENTS.md` §6 standing workflow；中央 thread
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` 已明确授权普通技术 next sibling，并允许
  小型官方公开包下载，但要求 packet 固定 URL、版本、请求数、字节预算、license/identity，
  且一次解析完整闭包。
- M09 packet：9,955 B / SHA-256
  `7dd9857082f8295b6646507736288b90c8868c60f98bbd7454d16d3eb1d2d8ee`。
- M09 owner turn/final：
  `01a0f56d-30b4-7820-820d-a4541db00161` /
  `msg_0a0722abe4bcea06016abdd0b6bfd487d0965c50fa0359ebd4`；状态
  `BLOCKED_MATERIAL_GAP`。
- M09 result：2,735 B / SHA-256
  `f9a41ef6753109bb45a0323731853dd57a88dc6b777971520232a8b3f06c11cb`；
  failure authority：2,029 B / SHA-256
  `52bfc6cc429a9fee8c3ba87763ff6631c07d22bfb7761331a7994876f1708372`。
- accepted correction：384 lines / 21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；
  原 R verdict `ACCEPT`。
- 本包唯一 preflight checker：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-10-preflight.py`，
  391 lines / 16,587 B / SHA-256
  `10ba490720ed8ddac03e9e36b4e97e68b610b44bc943b5bd036b7e7488b91149`。
- 本包唯一 preflight table：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-10-preflight.tsv`，
  98 lines / 23,812 B / SHA-256
  `b2e58332a0868ded905e3aae516c5e97ad3faea17c225e1de3a8a1f984634b16`。

除本文明确替换的 dependency acquisition、M09 runner 复用、tests-only restore、roots、
preflight 与网络预算外，M09 继承的 accepted M material schema、canonicalization、deployment/
B/L budget、old-root preservation 和回传合同继续适用。冲突时本文优先。

成功终点只能是 `READY_FOR_INDEPENDENT_M_R`。owner 不得自判接受、安装到 005、开始
Luban generation、B/L、产品、Unity、build/test、APK、设备或 Git。

## 1. M09 失败与可复用边界

M09 在 runner generation / verification 均通过后，tests generation restore 因本地 feed 缺少
`System.Reflection.Metadata 1.6.0` 与 `xunit.abstractions 2.0.3` 返回两个 NU1101；
未产生有效 tests lock 候选、未运行 tests verification、未 finalize material。M09 material root
必须继续 absent，旧失败不能改写为成功。

只读诊断证明完整 package-level closure 是以下三个固定包：

1. `System.Reflection.Metadata 1.6.0`；
2. `System.Collections.Immutable 1.5.0`；
3. `xunit.abstractions 2.0.3`。

第三个包不可省略：`System.Reflection.Metadata 1.6.0` 的兼容 NuGet dependency group
要求 `System.Collections.Immutable >=1.5.0`。固定 SDK 的 `testhost.deps.json` 是发布投影，
不是 NuGet restore authority，不得据其无依赖显示删边。

M09 的失败 `tests-generation/packages.lock.json`、`obj/project.assets.json`、dgspec、stdout/
stderr 仅为诊断证据，绝不能成为 candidate lock/assets 或复制进 material。

### 1.1 runner exact reuse

checker/table 冻结并验证以下 M09 runner 集：

- runner graph exact 23 nodes / 56 edges；
- generation 与 verification 各 23 locks/raw/canonical identities；
- 两阶段 canonical aggregate完全相等；
- runner comparison、markers、process receipt与 canonicalizer fixture；
- 931-file patched source manifest；
- M09 tests graph 13 nodes / 29 edges；
- M09 tests feed exact 28 nupkg。

M10 必须把这些对象按字节复制到 `imported-runner/` / `stage-material/` 并再比较；
runner restore 调用数必须 exact `0`。任一标量、目录 aggregate、source manifest 或 M09
old evidence 漂移，立即 `BLOCKED_PREDECESSOR_EXPLICIT_SET_DRIFT`，不得退化为重跑 runner。

## 2. 唯一 preflight 与 referenced receipt

### 2.1 签发方冻结检查

三个 M10 roots 均 absent时，工程主程从仓库根运行：

```text
/opt/homebrew/bin/python3 docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-10-preflight.py docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-10-preflight.tsv
```

最终冻结 checker/table 的实际结果为 exit `0`，
`TOTAL PASS rows=97 groups=27 m01Files=2021`。pre-freeze draft table 曾因尾部空行被 checker
拒绝；该草稿在签发前已修正，既不是 owner 调用也不是执行证据，不得复用其失败输出。

### 2.2 owner 唯一调用

owner 开始前只读核 packet/checker/table identity、M10 专属进程与
`os.statvfs(path).f_bavail * os.statvfs(path).f_frsize` 的 start free space。任何 M10 root写入前，
从仓库根逐字符运行 §2.1 command，只可一次，不得复制/包装 checker/table，不得 pipe、redirect或
保存 row stdout。

若 exit非 0 或 TOTAL不精确匹配，立即
`BLOCKED_PREDECESSOR_AUTHORITY_TABLE_DRIFT`，保持三个 M10 roots absent，不得重试。

PASS 后才创建 temp/evidence；`preflight/owner-preflight.json` 沿用 M09 小型 referenced schema：
actual thread/host/turn/tool-call、argv/cwd/exit/TOTAL/rawOutputReference、packet/checker/table
identity、两处 statvfs、预算结论、`rowOutputCopied=false`、`checkerRerun=false`、roots创建时序。
必须由结构化工具结果程序化生成；不得创建 stdout TSV 或人工转录 hashes。写后只做一次
schema/self-check，不重跑 checker。

## 3. create-once roots 与唯一写域

```text
/private/tmp/fightmatch-loc-lic-alt-m10/loc-lic-alt-yamldotnet-16.3.0-m10/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m10/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m10/
```

唯一允许写入是这三个 roots。material 只在全部 seal gate通过后 create-once；失败前保持 absent。
temp 内只允许 `preflight/`、`acquisition/`、`local-feed/`、`imported-runner/`、
`tests-generation/`、`tests-verification/`、`stage-material/`。evidence 只保存 bounded
authority/process/result/seal receipts。

M01–M09、005、旧 cache、八个生产路径、仓库文档/产品、`Assets/Packages/ProjectSettings/.git`
全部只读。禁止创建 B/L root。

## 4. 官方 acquisition：固定三包、11 请求、5 MiB

不得使用 latest、floating version、搜索 API、GitHub API、package manager restore联网或探索请求。
host allowlist只有 `api.nuget.org` 与 `raw.githubusercontent.com`；拒绝任何其它 host和任何
redirect。所有响应都记录 request id、exact URL、status、response headers SHA、bytes、SHA-256、
duration与 redirect count。

### 4.1 固定 registration 与 nupkg URL

| id/version | registration | nupkg | nupkg cap |
| --- | --- | --- | ---: |
| `System.Reflection.Metadata/1.6.0` | `https://api.nuget.org/v3/registration5-semver1/system.reflection.metadata/1.6.0.json` | `https://api.nuget.org/v3-flatcontainer/system.reflection.metadata/1.6.0/system.reflection.metadata.1.6.0.nupkg` | 2 MiB |
| `System.Collections.Immutable/1.5.0` | `https://api.nuget.org/v3/registration5-semver1/system.collections.immutable/1.5.0.json` | `https://api.nuget.org/v3-flatcontainer/system.collections.immutable/1.5.0/system.collections.immutable.1.5.0.nupkg` | 1 MiB |
| `xunit.abstractions/2.0.3` | `https://api.nuget.org/v3/registration5-semver1/xunit.abstractions/2.0.3.json` | `https://api.nuget.org/v3-flatcontainer/xunit.abstractions/2.0.3/xunit.abstractions.2.0.3.nupkg` | 256 KiB |

每个 registration/catalog响应 cap 256 KiB。三个 catalog URL只能从相应 exact registration leaf的
`catalogEntry`取得并冻结，不得预造。每个 registration leaf必须同时证明 lower/upper id、version、
packageContent与 catalog entry精确一致；catalog中的 packageHashAlgorithm必须 SHA512，base64 hash
必须与下载 nupkg计算值一致。

### 4.2 license/source/signature gate

期望但尚未冻结的语义：

- 两个 Microsoft 包：license expression `MIT`，官方 source/repository identity可机械绑定到
  exact full commit与仓内 `LICENSE.TXT`；
- `xunit.abstractions 2.0.3`：license `Apache-2.0`；其 exact repository full commit与 license
  source path必须由下载后的 registration/catalog/nuspec元数据导出，禁止猜 `LICENSE` 路径。

每个 nupkg必须核 archive SHA-256/bytes、nuspec id/version、dependency groups、license expression/
URL、repository type/url/full 40-hex commit、package files和 `.signature.p7s`，并用固定
SDK `dotnet nuget verify --all` 成功。source license只能从已验证 official repository full commit
构造 `raw.githubusercontent.com` URL。

若两个 Microsoft package由元数据证明共享同一 exact repository commit/path，可只取一次 license；
xunit取一次。正常请求总数 exact `11`：3 registration + 3 catalog + 3 nupkg + 2 raw license。
若合法元数据表明 Microsoft license不能去重，先停止并回
`BLOCKED_LICENSE_CLOSURE`；不得自行扩到第12请求。总 response bytes不得超过 5 MiB。

任何 metadata不足以绑定 exact source、license/path不符、signature失败、dependency组不符、
第4个缺失 nupkg、host/redirect/request/byte cap越界，立即停止；不得执行 restore。

### 4.3 closure必须一次闭合

以真实 nuspec 和 net8.0 NuGet兼容选择证明：

- ObjectModel `17.11.1` → SRM `1.6.0`；
- SRM `1.6.0` → Immutable `1.5.0`；
- xunit.extensibility.core `2.9.2` → xunit.abstractions `2.0.3`；
- 三个新增包不再引入第4个 external nupkg；framework-suppressed
  `NETStandard.Library` edge不进入 local feed。

closure不精确即停止。禁止“先 restore看还缺什么”。

## 5. tests-only 双 restore

仅在 §2–§4 全 PASS 后：

1. byte-copy M09 exact 28-package feed并加入三个已验证 nupkg；M10 tests local feed必须 exact
   31 nupkg，禁止其它文件/包；
2. 从 M09 931-file manifest验证过的 patched source复制到新鲜
   `tests-generation/source` 与 `tests-verification/source`；
3. 把 M09 runner sealed set按字节导入新 `stage-material`；禁止 runner restore；
4. tests generation：固定 SDK、完全 offline、只调用一次普通 restore；期望 graph exact
   13 nodes / 29 edges、13 locks、无 NU1101/NU1605、selected target/dependency groups符合 §4；
5. tests verification：从 generation逐字节复制 13 locks到独立 source/package root，只调用一次
   `--locked-mode` restore；
6. 比较全部 13 generation/verification locks、raw assets及 canonical assets；canonicalization仅移除
   exact `restoreLockedMode` 属性，verification/future-B marker必须 JSON boolean `true`；
7. 合并已复用 runner和新 tests产物，生成 package/license/deployment/B/L manifests；
8. old roots before/after全等；先 seal temp，再 create-once material，最后写 bounded evidence/result。

dotnet restore invocation count必须 exact `2`，runner `0`、tests generation `1`、tests verification
`1`。external requests只能是 §4 acquisition；所有 restore必须 offline且 network `0`。

generation任何失败、graph非13/29、lock非13、NU1101/NU1605、verification mismatch、marker错误、
old-root漂移或预算超限，立即 first-failure stop；不得同 root修 support、重试 restore、补包或继续
dependent phase。

## 6. 资源预算与交付

- start free space两处均 `>=2 GiB`；
- acquisition response总量 `<=5 MiB`；
- temp `<=768 MiB`，evidence `<=16 MiB`，final material沿 accepted schema且须报告实际大小；
- wall time `<=30 min`；单个 network request `<=60 s`；单次 restore `<=180 s`；
- failed attempt只允许一次 bounded seal；material仍 absent。

回主程必须包含：

- actual owner thread/host/turn与用户 authority；
- packet/checker/table identities和唯一 preflight tool-call/argv/exit/TOTAL/raw reference；
- M09失败保留、M09 evidence/material状态与 runner explicit-set equality；
- 11-request ledger、三份 registration/catalog/nupkg identities、redirect/host/bytes预算；
- 每包 nuspec/dependency/license/repository/full commit/source path/signature验证；
- 31-package feed manifest；
- exact 0/1/1 restore计数、13/29 graph、13 generation/verification locks/raw/canonical；
- runner复用与 tests新产物的 package/license/deployment/B/L manifests；
- temp/material/evidence seals、old-root equality、storage/wall/network/zero-action receipts；
- 唯一状态 `READY_FOR_INDEPENDENT_M_R` 或最早 `BLOCKED_<CAUSE>`；未运行项明确列出。

成功先回工程主程，不自行送 R。主程机械核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`。只有 R `ACCEPT` 才形成
`M-ACCEPTED`；不自动授权 005、B/L、产品、Unity或发布。

本 packet签发仅新增本文、checker/table；未创建 M10 roots、未联网、未运行 dotnet/restore/test/
Unity/Luban、未修改产品/资源/Packages/ProjectSettings，未执行 Git写操作。
