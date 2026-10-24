# LOC-LIC-ALT-M-11 — registration shape repair and request reuse

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. Delta目标与责任

本包是 M10 `BLOCKED_ACQUISITION_DRIVER` 的 clean sibling，只修 registration leaf中
`catalogEntry` 可为 string或object的解析器假设，并复用 M10 已成功下载的 req-01 响应。
依赖闭包、license/signature门、M09 runner复用、tests-only双restore、material schema与接收标准
全部沿用 M10；不重做产品设计或已成立元数据。

- delivery owner：LOC-A，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发/收件：工程主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- authority：`AGENTS.md` §6 standing workflow；中央已授权普通技术 sibling与有界官方包下载，
  并明确要求复用 M10 req-01、先用已封存响应做离线 string/object fixture。
- 基础合同：
  [M10](engineering-loc-license-alt-m-materials-continuation-10.md)，236 lines / 13,786 B /
  SHA-256 `cc8c023f5d18dae95f8df90d785876a3405ee1798f5f6915095931ff5c777b26`。
- M10 owner turn/final：
  `01a0f58a-1f6f-7ee1-b15b-20223b8a9ffc` /
  `msg_0a0722abe4bcea06016abdd77216d087d0b371d515126dff62`。
- 本包 checker：
  `engineering-loc-license-alt-m-materials-continuation-11-preflight.py`，
  390 lines / 16,586 B / SHA-256
  `3ba975931686729800f5704cd3658b5bf5c20a20791f83a7a854a175f4c8ceaf`。
- 本包 table：
  `engineering-loc-license-alt-m-materials-continuation-11-preflight.tsv`，
  112 lines / 27,149 B / SHA-256
  `3e51974a8237b7cc8168123fd7f5e4bd5a1d05383e0d4fbd715afcfe4819651b`。

除本文明确替换处，M10 §§3–6继续适用；所有 M10 root/evidence只读。成功仍只能是
`READY_FOR_INDEPENDENT_M_R`。

## 1. M10 首败永久保留

M10唯一请求 req-01：

- URL
  `https://api.nuget.org/v3/registration5-semver1/system.reflection.metadata/1.6.0.json`；
- HTTP 200、791 B、SHA-256
  `9ae28ac42b0927fdc3930ff72abaf60fc4f00086ab60d408949c9ec9d5146d0b`；
- headers SHA-256
  `efbc88ef1e6d9852a1a632bb87054cbbe5ddf2c2eccf94cd93e3bce26ae1ca99`；
- 503 ms、0 redirect、host `api.nuget.org`；
- `catalogEntry` 实际为 string：
  `https://api.nuget.org/v3/catalog0/data/2018.12.18.00.36.35/system.reflection.metadata.1.6.0.json`。

旧 driver对该 string调用 `.get`，抛 `AttributeError`；retry=0，restore=0，M10 material absent。
M10 result 3,201 B / SHA
`b98158f362d8ae502cdb6903ee09f1a5b595920058b2787f83e4106bae91db01`；
failure authority 2,150 B / SHA
`249c6c6be3cf7a28c5246674398160332ead1b34d1e34f7844e72579e1c68ff9`。
不得覆盖、补写或重试 M10 root。

## 2. M11 preflight与新 roots

三个 create-once roots：

```text
/private/tmp/fightmatch-loc-lic-alt-m11/loc-lic-alt-yamldotnet-16.3.0-m11/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m11/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m11/
```

owner在任何 M11 root写入前，从仓库根唯一运行：

```text
/opt/homebrew/bin/python3 docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-11-preflight.py docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-11-preflight.tsv
```

签发方冻结结果 exit `0`，`TOTAL PASS rows=111 groups=30 m01Files=2021`。owner exact
TOTAL不符即保持 roots absent并停止。PASS后按 M10 §2.2程序化生成一次 referenced receipt；
不得复制 row stdout或重跑 checker。

## 3. 离线 parser fixture先于网络

在 M11 temp创建新 driver；不得编辑或执行 M10 driver作为候选。任何新 HTTP前，必须从 M10
791 B registration原响应逐字节复制到 M11 fixture并核 SHA，然后让同一个 production parser
离线解析两种形态：

1. string fixture：原响应原字节；
2. object fixture：只把 `catalogEntry` 的 string值替为 `{"@id": <同一URL>}`，其它 leaf语义不变。

parser规则只能是：

- string → 该 string；
- object/dict → 非空 string `@id`；
- 其它类型、缺字段、空值、额外猜测路径 → fail closed。

两 fixture必须都导出同一固定 catalog URL、相同 packageContent/id/version，并写 bounded
input/output SHA与 PASS receipt。fixture任何失败立即
`BLOCKED_ACQUISITION_DRIVER_FIXTURE`，network actual 0。

## 4. 请求复用与剩余 acquisition

M11不得再次请求 req-01 URL。把 M10原响应、原 ledger行及其 tool-call provenance只读导入，
标记 `reusedFromM10=true`、`newNetworkRequest=false`；不得伪造成 M11 HTTP。

M11只允许剩余 exact 10个新请求：

- SRM catalog + nupkg：2；
- Immutable registration + catalog + nupkg：3；
- xunit registration + catalog + nupkg：3；
- metadata导出的两个 raw source-license：2。

请求ID继续 `req-02` 至 `req-11`。combined chain必须 exact 11 requests；M11 actual network
exact 10。host仍只允许 `api.nuget.org`、`raw.githubusercontent.com`，redirect exact 0；
combined response bytes `<=5 MiB`，M11剩余预算为 `5 MiB - 791 B`。

M10 §4的三固定包/version/URL、registration-derived catalog、catalog SHA512、nuspec dependency、
repository full commit、license/source path、`.signature.p7s` 与 fixed-dotnet
`nuget verify --all` 全部不变。xunit source/license path仍必须由真实 metadata导出，不能猜。
出现第4包、字段/host/redirect/请求数/字节/signature/license漂移立即停止。

## 5. tests-only closure与回传

§3–§4全 PASS后，严格继续 M10 §5–§6，但所有输出使用 M11 roots：

- M09 runner sealed set按字节复用，runner restore exact 0；
- M09 28包 + 新3包 = tests feed exact 31；
- tests generation普通 offline restore exact 1；
- tests verification `--locked-mode` offline restore exact 1；
- graph 13 nodes / 29 edges、13 locks，generation/verification lock/raw/canonical全等；
- 先 seal temp，再 create-once material；old M01–M10 before/after全等。

first failure立即 bounded seal，不在同一 root修 driver、重试请求/restore或继续 dependent phase。
回主程时除 M10 §6外，必须给 offline two-shape fixture、req-01复用证明、M11 req-02..11 ledger与
combined 11-request预算。禁止 Unity、产品、005、B/L、build/publish/test、Git或自行送 R。
