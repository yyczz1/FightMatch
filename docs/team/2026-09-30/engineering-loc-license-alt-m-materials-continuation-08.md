# LOC-LIC-ALT-M-08 — signed preflight runner and clean material continuation

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标与责任

本包是 M07 的 clean sibling。它不改变已接受的 lock-set design、M01-only replay、四份 isolation、
四次 restore 或 material schema；它只把 preflight 的**实际可执行文件**和 authority table一起由
签发方冻结、先由签发方以 exact argv实跑，再由 owner 在任何 M08 root写入前调用同一文件一次。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已裁定使用签发方发布的 frozen helper，owner
  不再抄写/改写算法或补造 gates，且本次无需重复 architecture R。
- 基础执行合同：M07 packet，157 lines / 9,912 B / SHA-256
  `625dc3faa7b84e82f2edaa6ff86301ef6093b45fbd90f57add83ddbc6ce72e84`。
- accepted correction：384 lines / 21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；原 R verdict
  `ACCEPT`，turn `01a0f524-3498-73e2-81dd-759bd648af3b`。
- 唯一 preflight checker：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-08-preflight.py`，
  308 lines / 12,393 B / SHA-256
  `3eb16dea006dd69ff37c223e265a1955be51f9f36b3a4c83494dce8e4c2ab69c`。
- 唯一 preflight table：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-08-preflight.tsv`，
  53 lines / 12,946 B / SHA-256
  `bb64603c6dad03e1e928f96cc2da40286f21ca1bf5fba0e22ef275e69c9a97bc`。

除本文明确替换的 preflight、M08 roots/name、M07 failure preservation 与回传 identity 外，M07
§§2.3–5 全部继续适用；其中 M07 root/name机械替换为 M08。M07 §2.1–2.2 被本文 §2完整替换。
冲突时本文优先。

成功终点只能是 `READY_FOR_INDEPENDENT_M_R`；owner 不得自判接受或开始 B/L、产品、
build/test/Unity/Luban/005/Git。

## 1. M07 失败必须原样保留

M07 owner turn `01a0f549-e1c6-70c0-b370-b024b463a780`、final
`msg_0a0722abe4bcea06016abdc696ca8487d090c20cc8280809d3` 固定为
`BLOCKED_PREFLIGHT_CHECKER`。其最早失败是 owner-created checker对
`/opt/homebrew/bin/python3` 使用 `lstat` regular-file gate，而该 runtime path是合法 symlink；table只授权
followed target SHA/version。该 checker另外在创建 temp root后才检查同一 root absent，形成顺序矛盾。
这两项都是 checker设计错误，不是 predecessor drift。

M07 未运行 replay/restore/network/Unity/Git/产品/B/L，material root保持 absent。M07 temp固定为
3 files / 19,102 B，evidence固定为 9 files / 27,748 B；逐文件 exact set已全部列入 M08 table。
M04/M05/M06 的失败状态和 exact evidence同样必须保留。M08 `result.json` 必须逐项记录这四个失败，
不得把它们改写成成功、删除或补写。

## 2. 唯一允许的 preflight

### 2.1 签发方已做的同文件预签

工程主程在三个 M08 roots均 absent时，直接运行了以下 exact command；没有复制 helper、没有等价脚本、
没有输出重定向，也没有写 M08 root：

```text
/opt/homebrew/bin/python3 docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-08-preflight.py docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-08-preflight.tsv
```

实际结果：exit `0`，`TOTAL PASS rows=52 groups=13 m01Files=2021`。其中 M04/M05/M06/M07 exact
sets分别为 2/6/6/3+9 files，Python symlink被 follow 到 regular target并核 SHA/version，dotnet target也核
SHA/version，三个 M08 roots最后仍全部 absent。此预签仅证明 frozen checker/table当前可执行，不替代
owner自己的唯一一次 preflight。

### 2.2 owner 的唯一调用

owner 开始前先以只读手段核对 dispatch 指定的 packet/checker/table bytes与 SHA，以及 M08 专属进程、
start free space；不得把系统其它 Unity/MCP/Hub/CrashHandler进程当 blocker。start free space必须用
Python `os.statvfs(path).f_bavail * os.statvfs(path).f_frsize` 或 `df` 的实际可用块数，不得用 macOS
`stat -f '%a'`（它不是 available blocks）。随后在任何 M08 root、文件或目录写入前，必须从仓库根运行
§2.1 **逐字符相同的 command**。只可调用一次：

- 不得复制、生成、编辑、格式化、包装、import 后另调、截断或重写 checker；
- 不得复制、编辑、排序、补行或重写 table；
- 不得在 command前创建 temp/evidence/material root；
- 不得通过 pipe/redirection/tee把这次 command输出写入 M08 root；工具回传的 stdout/stderr、exit、
  actual turn/tool-call identity就是首次证据；
- checker只读，不启动子进程、不联网；`runtime-file` follow symlink后核 target SHA/version，
  `exact-file-set` 和 `manifest7` target仍拒绝 symlink；
- table中的固定 identity是完整 authority；禁止从 packet prose、旧 shell变量或当前 stat补造额外
  expected bytes/hash/count/composite/tree aggregate。

若 exit非 0 或 total不是 exact `PASS rows=52 groups=13 m01Files=2021`，立即
`BLOCKED_PREDECESSOR_AUTHORITY_TABLE_DRIFT`；此时因 M08 root尚未创建，禁止写 bounded evidence、
修 checker/table、重试 argv或申请 M09，只回传工具原始结果。若 PASS，才允许创建 M08 temp root，
并把捕获的原始 stdout/stderr、exit、command、actual turn/tool-call identity写入一次性 preflight receipt；
不得为了生成 receipt重跑 checker。

### 2.3 已发生调用的勘误接续

owner turn `01a0f556-0178-74f2-a475-df88c8bd94e6` 在收到最初 dispatch 后，先核了三文件身份，随后
以 §2.1 exact command完成了唯一一次只读调用：tool/command id
`exec-31dbf858-aaec-4804-8861-e263049c4cd3`，exit `0`，`TOTAL PASS rows=52 groups=13
m01Files=2021`。该次调用时 checker、table及全部被验输入与本文当前冻结版本完全相同；最初 packet
SHA `4523bd02e3c255b0f5e08ca6ab217e4202053b8c01eaedfa56dc0e03650925fa` 的唯一已知错误是 §1 把
M07实际 turn误写成 M06 turn。STOP 后 owner final
`msg_0a0722abe4bcea06016abdc8d32b3487d097c2fe62d1c42439` 确认三个 M08 roots均 absent，未写 receipt、
support或任何 M08 文件，未运行 restore。

中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b` 裁定：这是描述性 provenance勘误，不废弃同一 frozen
checker/table的实际 PASS，也不制造 M09或第二次调用。owner 接续时必须复用上述唯一调用，禁止重跑。
owner此前以 macOS `stat -f '%a' × '%k'` 得到约 7.34 TB 的观察无效；在任何 M08 root写入前须按
§2.2正确方法补做一次只读容量测量并在 receipt中同时记录“旧值无效”和新值。若新值不满足继承预算，
立即 `BLOCKED_STORAGE_BUDGET`，仍保持三个 roots absent；否则才进入 §3，且不是第二次 preflight。

## 3. M08 create-once roots 与写白名单

签发方 preflight时以下 roots均 absent：

```text
/private/tmp/fightmatch-loc-lic-alt-m08/loc-lic-alt-yamldotnet-16.3.0-m08/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m08/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m08/
```

唯一允许写入是这三个 M08 roots。material root只在全部 closure/seal gates通过后 create-once；失败前
保持 absent。M01–M07、old cache、005、LOC-A evidence、八个生产路径、仓库文档/产品、
`Assets/Packages/ProjectSettings/.git` 均只读。禁止创建 B/L root。

M08 只从 table已闭合验证的 M01 accepted root/manifest逐字节重放 approved material；不得复制或执行
M04/M05/M06/M07 partial source/support/feeds/receipts。M04 replay TSV仅作为 M01 `manifest7` authority；
candidate bytes仍必须来自 M01。external network exact `0 requests / 0 bytes`。

## 4. 执行主体与停止门

§2 owner preflight全 PASS 后，严格执行 M07 §4及其继承的 M06 §3 / M05 §§3–5 / M04 §§3–7，
全部名称和输出机械替换为 M08：

1. 第一次 frozen support source生成并 raw-byte校验 7-field `m01-to-m08-replay.tsv` 与 3-field
   `patched-source-base.tsv`；禁止 shell `awk` 拼 TSV；
2. 从 M01 patched source创建 runner-generation、runner-verification、tests-generation、
   tests-verification 四份 byte-identical source，冻结 runner 23 nodes/56 edges、tests 13/29；
3. 依序四个 single-node restore：runner generation → runner verification locked-mode → tests
   generation → tests verification locked-mode；fixed SDK dotnet starts expected/exact 4；
4. 每 project保留默认相邻 `obj` / `packages.lock.json`；生成完整 raw/canonical assets、lock sets、
   package/license/deployment/B/L budgets，最后 seal material；
5. generation `restoreLockedMode` exact absent；verification/future B exact JSON boolean true；canonical
   comparison只移除该 exact property；
6. process/storage/wall/network/equality/zero-action budgets全部继承，且 M08 network 0/0。

任何 first failure立即停止；只允许一次 bounded failure seal，不得在同一 root修 support、重写 receipt、
重做 source copy、重试 argv或继续 dependent phase。preflight PASS 后发生的失败才允许按继承 schema写
bounded evidence；失败回最早 `BLOCKED_<CAUSE>`，不得自行申请 M09。

## 5. 回传与独立 R

成功回传 actual thread/host/turn、三个 M08 roots、packet/checker/table identities、签发方预签 identity、
owner唯一调用的 command/tool-call/exit及 52-row/13-group原始结果、M04–M07 failure preservation、
M01-only replay、receipt schemas、support runtime/fixtures、graph sets、四次 restore/process、per-project
lock/raw/canonical assets、package/license/deployment/B/L manifests、storage/wall/network/equality、zero
actions、material/evidence/result identities，以及唯一状态 `READY_FOR_INDEPENDENT_M_R`。

先回工程主程，不自行送 R。主程机械核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`。只有 R `ACCEPT` 才能形成 `M-ACCEPTED`；仍不
自动授权 B/L。

本文创建只新增本 packet、frozen preflight checker及 TSV。签发阶段未创建 M08 roots、复制材料、
执行 restore、联网、seal material、build/publish/test、Unity/Luban/005、产品、B/L或 Git。
