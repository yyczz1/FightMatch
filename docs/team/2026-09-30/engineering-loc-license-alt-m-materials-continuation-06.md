# LOC-LIC-ALT-M-06 — explicit-path sibling after M05 aggregate mismatch

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标与责任

本包是 M05 的 clean sibling。它只删除 M05 那个定义不完整的 absolute-path composite hash gate，
改用机器实测的 `relativePath / bytes / SHA-256` exact sets；lock-set 技术方案、M01-only replay、
四份隔离 source、四次 restore、per-project lock/assets sets 与 material freeze 均不变。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求由主程直接补签本 sibling，无需重审
  accepted lock-set design、旧工具验签或已通过测试门。
- 基础执行合同：M05 packet，149 lines / 9,612 B / SHA-256
  `a8dc95f48896345c97f5fcfda1c96bfcc21bb3d92d9ba02eb620cba90771f842`。
- accepted correction：384 lines / 21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；原 R verdict
  `ACCEPT`，turn `01a0f524-3498-73e2-81dd-759bd648af3b`。

除本文明确替换的 predecessor-preservation gate、M06 roots/name与回传 identity 外，M05 §§2–6
全部继续适用；其中 M05 root/name机械替换为 M06。冲突时本文优先。成功终点只能是
`READY_FOR_INDEPENDENT_M_R`；作者不得自判接受或开始 B/L、产品、build/test/Unity/Luban/005/Git。

## 1. M04/M05 失败保留与精确 predecessor set

M04 固定为 `BLOCKED_STOP_GATE_VIOLATION`，underlying first failure 为
`BLOCKED_RECEIPT_FORMAT`；收到 stop 后的 TSV rewrite、four source copies、support source creation
均须继续记载。M04 fixtures/restore/material均未执行/创建。

M05 owner turn `01a0f535-caed-7d33-b7a8-40144271b7ad` 固定为
`BLOCKED_M04_FAILURE_PRESERVATION`：M04 temp 用 owner exact pipeline匹配，但 M04 evidence 的
absolute-path composite expected `d61c...` / observed `251243...`；两个 individual files实际完全
匹配。M05 没有创建 temp/material，support Python/dotnet/network/product/Git均为 0，只创建 bounded
failure evidence。

M06 不再计算或比较任何 absolute-path stream/composite/tree aggregate。签发前主程已用实际
`find/stat/shasum` 结果冻结以下 **relative path exact sets**；owner只按 relative path ordinal set、
bytes与文件内容 SHA-256逐项比较，不能把 absolute root spelling、`shasum` 行格式或排序文本放入
identity。

### 1.1 M04 evidence exact set（恰 2 files）

| relative path | bytes | SHA-256 |
| --- | ---: | --- |
| `comparison/m01-to-m04-replay.tsv` | 536,958 | `ce3343854892890f1075182e7cc3325865c24111438a2cb0dfdf1bcfeb07e814` |
| `comparison/patched-source-base.tsv` | 107,129 | `a1ab64b5ee977ea9d295fb141367891244a8ddc5b492597478332bb2ad647a79` |

### 1.2 M05 evidence exact set（恰 6 files）

| relative path | bytes | SHA-256 |
| --- | ---: | --- |
| `FAILED_EVIDENCE` | 156 | `88b4d2a828db6c3276b77ef81a1ccbbb89f3c7703a242c8b6667858bbc700c95` |
| `authority/m04-preservation-failure.json` | 1,475 | `44da954817508613a0630b870dfc049ef1f2c1f09b42a11394a419e7806a87a2` |
| `evidence-files.tsv` | 657 | `94aebc345f4d208257be36974c343cc1e85c35d6e82c83091c502864285dff3e` |
| `process/m05-completed-commands.tsv` | 842 | `9d5f42a5eb7c3b3df5c8495ccd0bdd800d9af41eefb3008727339dce2c03adcf` |
| `process/not-run-commands.tsv` | 580 | `8c4ca78f28f8849e2e6f4df8281a92e716a3c12080e0e2f5045886b4f85433a0` |
| `result.json` | 2,661 | `52fe1227405f216c35493373da6fe2db86ed8d247b475874edcd5da75d3ab4e3` |

M04/M05 temp/evidence/material roots全部禁止写、删除或作为 M06 candidate 输入。M06 只确认上述
两个 evidence sets exact；不扫描、不执行、不复制 M04 temp内容。M04 temp的最后一次受控检查已由
M05 result固定为 5,746 files / 109,101,799 B / matching owner pipeline；它不是 M06 的 source或
验收 gate。任一 listed set缺、多或 bytes/SHA不符才停止
`BLOCKED_PREDECESSOR_EXPLICIT_SET_DRIFT`。

## 2. M06 create-once roots 与写白名单

签发时以下三个 exact roots 已读验证为 absent；owner首个写动作前再次确认：

```text
/private/tmp/fightmatch-loc-lic-alt-m06/loc-lic-alt-yamldotnet-16.3.0-m06/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m06/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m06/
```

唯一允许写入是这三个 M06 roots。material root只在全部 closure/seal gates通过后 create-once；
失败前保持 absent。M01/M02/M03/M04/M05、old cache、005、LOC-A evidence、八个生产路径、仓库
文档与产品、`Assets/Packages/ProjectSettings/.git` 均只读。禁止创建 B/L root。

M06 必须从 M01 accepted acquired/source manifests逐字节重放 approved material；不得复制或执行
M04/M05 partial source、support、feeds、receipts、closure或其它产物。external network仍为 exact
`0 requests / 0 bytes`。

## 3. 执行与停止门

严格执行 M05 §§3–5，但所有输出为 M06：

1. 首次 frozen Python support source直接生成并 raw-byte校验 7-field
   `m01-to-m06-replay.tsv` 与 3-field `patched-source-base.tsv`；禁止 shell `awk` 拼 TSV；
2. 从 M01 patched source创建四份 byte-identical isolation，冻结 runner 23 nodes/56 edges、tests
   13 nodes/29 edges；
3. 依序只做 runner generation → runner verification locked-mode → tests generation → tests
   verification locked-mode，fixed SDK dotnet starts expected/exact 4；
4. 每 project默认相邻 `obj` / `packages.lock.json`，生成完整 raw/canonical assets、lock sets、package/
   license/deployment/B/L budgets，再 seal material；
5. generation `restoreLockedMode` exact absent；verification/future B exact JSON boolean true；
   canonical comparison只移除该 exact property；
6. process/storage/wall/network/equality/zero-action budgets全部沿用 M04/M05。

任何 first failure立即停止；只允许一次 bounded failure seal，不得在同一 M06 root改写失败 receipt、
修 support source、重做复制或重试 argv。失败回最早 `BLOCKED_<CAUSE>`；不得自行申请 M07。

## 4. 回传与独立 R

成功回传 actual thread/host/turn、三个 M06 roots、§1 两个 explicit set comparisons、M01-only
replay、receipt schemas、support runtime/fixtures、graph sets、四次 restore/process、per-project
lock/raw/canonical assets、package/license/deployment/B/L manifests、storage/wall/network/equality、
zero actions、material/evidence/result identities，以及唯一状态 `READY_FOR_INDEPENDENT_M_R`。

先回工程主程，不自行送 R。主程核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`。只有 R `ACCEPT` 才能形成 `M-ACCEPTED`；仍不
自动授权 B/L。

本文创建只新增本 packet。没有创建 M06 roots、复制材料、执行 Python/restore、联网、seal
material、build/publish/test、Unity/Luban/005、产品、B/L或 Git。
