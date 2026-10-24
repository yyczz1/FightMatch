# LOC-LIC-ALT-M-09 — referenced preflight receipt and clean material continuation

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标、责任与优先级

本包是 M08 的 clean sibling。它不改变已接受的 lock-set design、M01-only replay、四份 isolation、
四次 restore 或 material schema；唯一流程修正是：owner 不再把 checker 的逐行 stdout 手工复制到文件，
而只把真实工具调用身份及可机械核对的摘要写入小型 JSON receipt。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程。中央 thread
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` 明确裁决 `AUTHORIZE_M09`：M08 保留终局
  `BLOCKED_RECEIPT_FORMAT`，批准最小 clean sibling；文件系统回执只记录真实 tool-call id、精确 argv、
  exit、TOTAL 与 raw output reference，禁止人工复制 row stdout 或长哈希。
- 基础执行合同：M08 packet，162 lines / 10,715 B / SHA-256
  `aa334b591ca6768fdd68ce50a7dc9ca63e535778ae17999c86d2ddd43b64d0bb`。
- accepted correction：384 lines / 21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；原 R verdict
  `ACCEPT`，turn `01a0f524-3498-73e2-81dd-759bd648af3b`。
- 唯一 preflight checker：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-09-preflight.py`，
  308 lines / 12,393 B / SHA-256
  `b7839648158e37cdee9550b8a04e2e8c224930724bf0250f7d737a7158e5fa3f`。
- 唯一 preflight table：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-09-preflight.tsv`，
  65 lines / 15,824 B / SHA-256
  `953ae17ba10e00d9a9f6bcf9ff1df87cf33a2bf7a37c7fdab11b289033c20b94`。

除本文明确替换的 preflight、receipt、M09 roots/name、M08 failure preservation 与回传 identity 外，
M08 §§3–5 全部继续适用；其中 M08 root/name机械替换为 M09。M08 §§1–2 被本文 §§1–3 完整替换。
冲突时本文优先。成功终点只能是 `READY_FOR_INDEPENDENT_M_R`；owner 不得自判接受或开始 B/L、
产品、build/test/Unity/Luban/005/Git。

## 1. M08 失败必须原样保留

M08 owner turn `01a0f55a-992f-7e80-ba2a-b18ec9448462`、final
`msg_0a0722abe4bcea06016abdcad387cc87d08a0fa6b7dbe271c6` 固定为
`BLOCKED_RECEIPT_FORMAT`。其唯一 checker 调用实际 exit `0` 且 TOTAL PASS，但 owner 手工复制逐行
stdout 时转录错两个 hash 字符；这是新失败，不得改写为成功、删除或补写。

M08 temp exact set固定 2 files / 12,574 B；M08 evidence exact set固定 6 files / 7,669 B；material
root absent。M08 未运行 replay/restore/network/dotnet/Unity/Git/产品/B/L。M04–M07 的失败状态和 exact
evidence同样保留。M09 `result.json` 必须逐项记录 M04–M08 的失败与 M08 material absent。

中央从原 M08 thread 的结构化 `commandExecution` 记录机械导出了：
`docs/team/2026-09-30/snapshots/loc-m08-original-preflight/tool-record.json`。该文件不是新执行，也不声明
PTY 原始字节身份；只作为 M08 原调用 provenance。M09 table 已以 bytes/SHA 将 M08 temp/evidence exact
sets与 material absent纳入一次性检查；该导出不属于 table authority。禁止据该 JSON 重建、覆盖或
“修正”M08 receipt。

## 2. 唯一允许的 M09 preflight

### 2.1 签发方预签

工程主程在三个 M09 roots均 absent时，从仓库根直接运行以下 exact command；未复制 helper、未用等价
脚本、未输出重定向，也未写 M09 root：

```text
/opt/homebrew/bin/python3 docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-09-preflight.py docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-09-preflight.tsv
```

实际结果：exit `0`，`TOTAL PASS rows=64 groups=16 m01Files=2021`。其中 M04–M08 exact sets、
M08 material absent、Python/dotnet固定 runtime、M01 2,021-file closed set及三个 M09 roots均通过。
此预签只证明冻结 checker/table 可执行，不替代 owner 的唯一一次调用。

### 2.2 owner 的唯一调用

owner 开始前仅用只读手段核 dispatch 指定的 packet/checker/table bytes与 SHA、M09 专属进程及 start
free space。容量必须用 Python `os.statvfs(path).f_bavail * os.statvfs(path).f_frsize` 或 `df` 的实际
available blocks；不得用 macOS `stat -f '%a'`。随后在任何 M09 root、文件或目录写入前，从仓库根运行
§2.1 逐字符相同的 command，且只可调用一次。

- 不得复制、生成、编辑、格式化、包装、import 后另调、截断或重写 checker/table；
- 不得在 command前创建 temp/evidence/material root；
- 不得用 pipe/redirection/tee保存 stdout；工具返回的 stdout/stderr、exit及 actual
  thread/turn/tool-call identity是唯一原始调用证据；
- table中的固定 identity是完整 authority；禁止从 prose、旧变量或当前 stat补造 expected 值；
- checker只读、不启动子进程、不联网；`runtime-file` 仅 follow runtime symlink，exact sets与
  `manifest7`仍拒绝 symlink。

若 exit非 `0` 或 TOTAL不是 exact `PASS rows=64 groups=16 m01Files=2021`，立即
`BLOCKED_PREDECESSOR_AUTHORITY_TABLE_DRIFT`。保持三个 M09 roots absent；禁止写 bounded evidence、
修 checker/table、重试或继续 dependent phase，只回传工具原始结果。

若 PASS，才允许创建 M09 temp/evidence roots并进入 §3。不得为了 receipt、审查或格式化重跑 checker。

## 3. referenced receipt 的唯一 schema

preflight PASS 后，M09 temp root中只创建一个小型
`preflight/owner-preflight.json`；不得创建 `owner-preflight.stdout.tsv`、stderr副本、row列表、长哈希
转录或 checker/table副本。该 JSON 至少且只能按以下语义记录：

- `schemaVersion`；
- actual owner `threadId`、`hostId`、`turnId`；
- actual command/tool-call id；
- exact argv（参数数组或不经人工改写的 exact command字符串）；
- working directory；
- integer exit code `0`；
- exact TOTAL `PASS rows=64 groups=16 m01Files=2021`；
- `rawOutputReference`：明确指向同一 thread/turn/tool-call 的工具原始输出，而不是复制其内容；
- packet/checker/table path、bytes、SHA；
- 两个正确 `statvfs` 可用字节值、测量路径/方法及预算 verdict；
- 明确 `rowOutputCopied=false`、`checkerRerun=false`、三个 roots 的创建时序。

JSON 必须由实际工具结果字段程序化填入，不得逐字符抄 row output；写后只做 JSON 解析、字段等值、
bytes/SHA 与 exact-file-set核验，不调用 checker。若 receipt 写入或自检失败，立即
`BLOCKED_RECEIPT_FORMAT` 并按继承规则 bounded seal；禁止同 root重写或创建 sibling。

## 4. M09 create-once roots 与写白名单

签发方预签时以下 roots均 absent：

```text
/private/tmp/fightmatch-loc-lic-alt-m09/loc-lic-alt-yamldotnet-16.3.0-m09/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m09/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m09/
```

唯一允许写入是这三个 M09 roots。material root只在全部 closure/seal gates通过后 create-once；失败前
保持 absent。M01–M08、old cache、005、LOC-A evidence、八个生产路径、仓库文档/产品、
`Assets/Packages/ProjectSettings/.git` 均只读。禁止创建 B/L root。

M09 只从 table已闭合验证的 M01 accepted root/manifest逐字节重放 approved material；不得复制或执行
M04–M08 partial source/support/feeds/receipts。M04 replay TSV仅作为 M01 `manifest7` authority；candidate
bytes仍必须来自 M01。external network exact `0 requests / 0 bytes`。

## 5. 执行主体、停止门与回传

§2–§3 全 PASS 后，严格执行 M08 §4及其继承合同，全部名称和输出机械替换为 M09：

1. 生成并 raw-byte校验 7-field `m01-to-m09-replay.tsv` 与 3-field `patched-source-base.tsv`；
2. 建四份 byte-identical source，冻结 runner 23 nodes/56 edges、tests 13/29；
3. 依序四个 single-node restore；fixed SDK dotnet starts expected/exact 4；
4. 保留相邻 `obj` / `packages.lock.json`，生成全部 raw/canonical assets、lock sets、package/license/
   deployment/B/L budgets，最后 seal material；
5. generation `restoreLockedMode` exact absent；verification/future B exact JSON boolean true；canonical
   comparison只移除该 exact property；
6. process/storage/wall/network/equality/zero-action budgets全部继承，M09 network 0/0。

任何 first failure立即停止；只允许一次 bounded failure seal，不得在同一 root修 support、重写 receipt、
重做 source copy、重试 argv或继续 dependent phase。失败回最早 `BLOCKED_<CAUSE>`。

成功回传 actual thread/host/turn、三个 M09 roots、packet/checker/table identities、owner唯一调用的
tool-call/argv/exit/TOTAL/raw reference、M04–M08 preservation、M01-only replay、receipt schema、support、
graphs、四次 restore/process、每项目 lock/raw/canonical assets、package/license/deployment/B/L manifests、
storage/wall/network/equality、zero actions、material/evidence/result identities，以及唯一状态
`READY_FOR_INDEPENDENT_M_R`。

先回工程主程，不自行送 R。主程机械核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`。只有 R `ACCEPT` 才能形成 `M-ACCEPTED`；仍不
自动授权 B/L。

本文创建只新增本 packet；冻结 checker/table早已存在。签发阶段未创建 M09 roots、复制材料、执行
restore、联网、seal material、build/publish/test、Unity/Luban/005、产品、B/L或 Git。
