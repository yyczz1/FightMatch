# LOC-LIC-ALT-M-02 — 验签适配后材料闭包续行包

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目标、责任与结论边界

本包是 M01 因 macOS NuGet 验签能力停止后的 **new sibling continuation**。它不修改、
覆盖或补写 M01；先重验并复制 M01 已取得的固定材料，再引用 Linux 004 的限定验签
`ACCEPT`，继续尚未执行的 transitive/package/license closure、两个 locks、locked-mode
verification、finite deployment mapping、B/L command budgets 与 material seal。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 基础执行合同：`engineering-loc-license-alt-m-materials.md`，582 lines / 30,837 B /
  SHA-256 `0ce7c44844494e8b63c5e6c149fa357932c11c9a53457275a24a16ac4ef48d13`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已允许同级续行；不授权 B/L、Git、
  Unity、产品写入、发布或外部人工消息。

除本文明确替换的 sibling roots、签名裁决、累计预算与续行 process receipts 外，M01
合同的 source/version/host、两行 patch、schema、license、zero-action、失败保全和验收条款
全部继续适用。成功终点仍只能是 `READY_FOR_INDEPENDENT_M_R`；material 标记
`CANDIDATE_NOT_ACCEPTED`。作者不得自判 `M-ACCEPTED`，不得自动进入 B/L。

## 1. 固定 predecessor 与限定验签裁决

### 1.1 M01 失败史与可复用字节

M01 evidence root：

```text
TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m01/
```

固定 seals：

| item | actual identity |
| --- | --- |
| `result.json` | 2,367 B；SHA `a57094354718377a4b0204793fc2e8c3d14a4316812dd0b74ee834076f8fff5e`；status `BLOCKED_PACKAGE_SIGNATURE` |
| `evidence-files.tsv` | 30 lines / 2,923 B；SHA `51b4453620bde4d4e8fdbdc9093b30cef4a5349c03a2ceb3c4ed2ad5b8c66efc` |
| `comparison/temp-files.tsv` | 2,022 lines / 262,401 B；SHA `51ab7cd6b89588f04b390a8ecabd6bd957943ede18bd0cc1bf15c8edcb21cb6b` |
| `acquisition/acquired-files.tsv` | 86 lines / 11,284 B；SHA `c4d99ea68ec26b103b393ef58d6943f38b8aba4a8bbb8fa28dab391f7123e4bf` |
| `acquisition/network-requests.jsonl` | 42 lines / 29,848 B；SHA `98daea6150110657bdc84fd00e84b8d8469e6448e6d3bceeb645c585e0f53266` |
| `process/completed-commands.tsv` | SHA `6cfa16b2058fb4a3f7157a844d8008d5217f0cb0f77d9b01d867e09b0b437705` |

M01 temp root：

```text
/private/tmp/fightmatch-loc-lic-alt-m01/loc-lic-alt-yamldotnet-16.3.0-m01/
```

其 fixed receipt 为 2,021 regular files / 86,373,435 logical bytes。M01 已完成 official
source copy、exact two-line patch 与 fixed direct acquisition；42 requests / 23,259,536 body
bytes。material root 从未创建。C001/C008 是已封存的内部失败尝试；C014 是 macOS
platform verify failure；M01 C001–C019 的 child counts 未被 OS 级完整 instrument，必须原样
披露，不得倒写或伪造。M02 只可读取 M01 temp/evidence；先按以上 manifests 重验每个
复用文件，再 create-once 复制到 M02 temp。任何 mismatch 为
`BLOCKED_PREDECESSOR_DRIFT`；不得重新下载已匹配 material 来掩盖 drift。

### 1.2 Linux 004 与原 R 的精确裁决

Linux 004 packet：`testing-loc-license-alt-signature-linux-004.md`，279 lines / 15,275 B /
SHA `e9e8e9771052e0dd0fd40897487d3fc218d67401c363accdc9416fdbd23a2794`。

- durable execution turns：`01a0f4c9-2606-74be-b8a3-59dcf1cc81c3`、
  `01a0f4d2-cf3a-718f-931a-b92ec2c72f3b`；status
  `READY_FOR_INDEPENDENT_SIGNATURE_R`；
- result SHA `d03fad30914fe6e01f89cd77596fe3716022099d9e26f10fc319ac7391d1ef29`；
  4,801-file evidence manifest SHA
  `43b7843df644582dff0bd467ed2c0d6d0a7365653adfac23797991980cf72bf2`；
- fixed SDK 8.0.425/linux-x64、nupkg 776,880 B/SHA
  `e068bcc1243c46c8bfdfe2f27a026bfff03cde7c67d9f37c2cdd70bd24a9dfd4` 与
  `.signature.p7s` 12,920 B/SHA
  `ba959936ca152b03068114633994e331603c10079631cbceae2e38035c346308` 均匹配；
- 唯一 `dotnet nuget verify --all` exit 0，0 errors，4 warnings；警告仅为
  NU3018/NU3028 revocation server unreachable / revocation status unknown。

原独立 LOC R：turn `01a0f4dd-0664-7b92-a5cf-2351ee317c5e`，final
`msg_0d29bf8d726d8dc0016abdab8dd80c87d0907a376faa94426b`，唯一 verdict
`ACCEPT` **仅 package-signature gate**。该结论允许 M02 把上述 exact nupkg 的
`signatureVerdict` 记为 `VERIFIED_LINUX_EXIT0_REVOCATION_UNKNOWN`；禁止声称证书当前未
吊销、在线 revocation 已成功或整 M 已接受。M01 的 macOS blocker 仍是历史事实；M02
不重跑 Mac/Linux signature verify，不下载 SDK，不改 trust。

## 2. create-once roots 与写白名单

执行开始前三个 exact roots必须不存在：

```text
/private/tmp/fightmatch-loc-lic-alt-m02/loc-lic-alt-yamldotnet-16.3.0-m02/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m02/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m02/
```

唯一允许写入为这三个 roots。M01 temp/evidence、old cache、005、LOC-A evidence、八个
生产路径、仓库产品与文档、`Assets/Packages/ProjectSettings/.git` 均只读。禁止创建 B/L
roots、运行 Unity/Luban/005、build/publish/test、修改 `Program.cs`、Git 写入、删除旧根。

material root 仍只在全部 closure/seal gates 通过后 create-once；失败前不存在，失败后若
已创建则原地标 `REJECTED_NOT_ACCEPTED`，不得清理或复用。

## 3. M02 续行算法

1. **preflight**：重算基础合同、M01 seals、Linux 004 packet 与本包 final SHA；确认三个
   M02 roots absent、free space与只读输入。创建 M02 temp/evidence；material 暂不创建。
2. **M01 replay without re-execution**：按 M01 `temp-files.tsv` 与 acquired manifest 逐文件
   核 type/path/bytes/SHA，把 approved closed set 复制到 M02 temp；重算 copied manifest。
   保存 M01 blocker、42-request raw receipts、两行 patch 与 fixed direct identity。不得把
   复制计作新 network request，也不得运行 M01 commands。
3. **signature adjudication**：写 composite receipt，同时保留 Mac C014 exit1 历史与
   Linux 004/R 的限定 ACCEPT；只对 fixed YamlDotNet 16.3.0 使用第 1.2 节 verdict。
4. **dependency/license expansion**：从已复制 exact nuspec/project graph 继续基础合同
   §5.3/§5.4；只为尚缺的 transitive package、registration/catalog/source/license 发请求。
   不重取已匹配 direct bytes；不变更 host/version/source policy。
5. **offline closure**：fixed Mac SDK 8.0.425、local-only feeds、`<clear/>` NuGet config；
   runner/tests 各生成 lock，再在新空 isolation root 各做一次 `--locked-mode` restore。
   所有 invocation 只能设置 task-specific `DOTNET_CLI_HOME`、`NUGET_PACKAGES`、
   `NUGET_HTTP_CACHE_PATH`、`NUGET_PLUGINS_CACHE_PATH`、`NUGET_SCRATCH`、`TMPDIR`；
   **禁止设置或覆盖 `HOME`、`home`、`CODEX_HOME`**。restore 不得隐式 build/test。
6. **mapping/budgets**：按基础合同 §7/§9 Step 7 冻结完整 package/license/entry/official-test/
   expected-deployment mapping 与 future B/L numeric command DAG，不读取 future output。
7. **seal**：核 schemas、finite sets、old roots/cache/生产/protected before-after equality，
   通过后 create-once material root并复制；重算 material/evidence manifests、process/storage/
   network totals与 zero-action；返回 `READY_FOR_INDEPENDENT_M_R` 后停止。

失败使用基础合同 §11 最早 gate；新增合法 code：`BLOCKED_PREDECESSOR_DRIFT`、
`BLOCKED_SIGNATURE_ADJUDICATION`。任何 Mac/Linux receipt 身份不一致、或 signature verdict
被扩大，均用后者停止。

## 4. 累计 network、process、time 与 storage 上限

M01+M02 必须同时满足原总上限：requests `<=256`、body bytes `<=384 MiB`。M01 已用
42 requests / 23,259,536 bytes，因此 M02 新增上限为：

- requests `<=214`；顺序执行、concurrency 1、每 request redirect `<=3`；
- transferred body `<=379,393,648` bytes；单 body `<=64 MiB`；
- acquisition wall `<=25 min`；不得 retry 已封存的 M01 direct request。

M02 自身从零完整 instrument；M01 historical child-count unknown 单列为 predecessor
limitation，不计作已知 0，也不阻断 M02 正确的 independent receipts。M02 ceilings：

| item | ceiling |
| --- | ---: |
| top-level commands / trees | 48 / 1 concurrent |
| total process starts | 20,000 |
| M02 total wall | 45 min |
| fixed SDK dotnet starts | 96 total；top-level restore/probe `<=20`；concurrent `<=8` |
| zsh top-level drivers | 48；concurrent 1 |
| curl starts | 214；concurrent 1 |

其余 executable allowlist与 per-tool caps沿用 M01 §8.2，但上表更小值优先。`csc`、
VBCSCompiler、VSTest、testhost、Luban、Unity、Java、Gradle 出现即
`BLOCKED_PROCESS_DRIFT`。每个 top-level tree 保存 argv/cwd/isolated env/start/end/exit、
owned child tree 与 counts；自然等待，禁止 signal/kill/retry 绕门。

storage：M02 temp `<=1 GiB`/50,000 files；material `<=512 MiB`/25,000；evidence
`<=128 MiB`/10,000；logs aggregate `<=64 MiB`；单 stdout/stderr `<=4 MiB`；start free
`>=2 GiB`、持续 `>=1 GiB`。M01 roots不计入 M02 root大小，但必须记录 predecessor actual。

## 5. 必须产出的续行 receipts

除基础合同全部 final schemas外，M02 evidence 必须包含：

- `authority/predecessor-m01.json`：M01 status/seals/root actual与 historical instrumentation
  limitation；
- `authority/signature-adjudication.json`：Mac failure + Linux raw success + R 限定 verdict，
  含 warning scope；
- `comparison/m01-reuse.tsv`：每个复用文件的 M01 relative path、bytes/SHA、M02 path、
  bytes/SHA、copy verdict；
- `acquisition/network-cumulative.json`：M01、M02、cumulative request/body totals，明确 M01
  replay不计网络；
- `process/m02-completed-commands.tsv` 与 per-tree child receipts；不得倒写 M01 unknown；
- 两个 locks、两次 locked-mode restore raw evidence、完整 package/license/entry/deployment
  manifests、future B/L process/resource budgets；
- old roots/cache、八个生产路径、protected status before-after identity；
- material `CANDIDATE_NOT_ACCEPTED` seal与 final `result.json`。

`result.json.zeroActionReceipt` 沿用基础合同且全部为 0。另显式写：

```text
macSignatureVerifyInvocationCount: 0
linuxSignatureVerifyInvocationCount: 0
sdkDownloadCount: 0
m01MutationCount: 0
```

## 6. 回传与独立 R

delivery owner 回传：owner turn、activation、三个 root、predecessor replay、signature
adjudication、new/cumulative network、全部 process/storage/wall actual、package/license/lock/
deployment manifests、old-root equality、zero-action、material/evidence/result bytes/SHA 与
blockers。完成后停止，不自行送 R，不进入 B/L。

主程机械审 actual roots和 evidence 后，必须交**同一原 LOC 独立 R**
`01a0e404-e8ee-7310-8388-9260babd53f1 / local` 审查完整 M01+Linux004+M02 chain；R 只给
一个 `ACCEPT` / `NEEDS_FIX` / `REJECT`。只有 `ACCEPT` 才形成可供未来 B 签包引用的
`M-ACCEPTED` authority；仍不自动授权 B/L。

## 7. 签包回合未执行声明

本文件创建回合只新增本 packet。没有创建三个 M02 roots，没有网络、复制、restore、
package/license resolution、material seal、build、publish、test、Unity、Luban、005、产品、
外置工具根或 Git 写入。实际执行只在主程回传本文件 final SHA-256 后开始。
