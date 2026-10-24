# LOC-LIC-ALT-M-05 — clean sibling after M04 stop-gate violation

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标与责任

本包是 M04 的 clean sibling retry。它不改变已接受的 lock-set design，也不重做 architecture R、
Linux signature、M01/M02/M03 已完成验证；它只在全新 M05 roots 按 M04 的有效技术合同完成四份
隔离 source、四次 restore、per-project lock/assets sets、material freeze 与回执。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已明确允许本 clean sibling，且无需重复
  design R / old tool验签 / 已通过测试门。
- 技术合同：`engineering-loc-license-alt-m-materials-continuation-04.md`，226 lines /
  14,058 B / SHA-256
  `a359488d8d93eb7ee9df50438bf8cf0d9199473b1ce44d14a4410637209d2f10`。
- accepted correction：`engineering-localization-lock-set-system-design-correction.md`，384 lines /
  21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；R verdict
  `ACCEPT`，turn `01a0f524-3498-73e2-81dd-759bd648af3b`。

除本文明确替换的 M05 roots、M04 failure preservation、receipt writer与回传 identity 外，M04
§§1、3–8 全部继续适用。M04 中所有 `<M04_TEMP>` / M04 output 名称在本次机械替换为对应 M05
root/name；M04 §2 的三个旧 write roots全部改为只读。冲突时本文优先。

成功终点只能是 `READY_FOR_INDEPENDENT_M_R`；作者不得写 `M-ACCEPTED`，不得开始 B/L、build、
publish、test、Unity、Luban、005、产品写入、Git或外部人工消息。

## 1. M04 失败事实必须保留

M04 owner turn `01a0f52a-63a0-7961-bd0c-fdc900ff78b9` 最终状态固定为
`BLOCKED_STOP_GATE_VIOLATION`，底层最早失败为 `BLOCKED_RECEIPT_FORMAT`：首次
`m01-to-m04-replay.tsv` 不是 tab-delimited；QA/主程发出 stop 后，owner 又在同一 M04 root 修正
TSV、创建四份 source copies 与 support source。后来 byte comparison 全 MATCH 不能消除 stop-gate
违规。

M04 没有运行 support Python fixtures、任何 restore/network/build/publish/test/Unity/Luban/005/Git/
产品写入，也没有创建 material root。M04 partial roots不得补写、删除、复用为 candidate 或改成
成功。主程在 M05 签发前只读观察到：

| partial root/item | fixed observation |
| --- | --- |
| M04 temp root | 5,746 files / 109,101,799 B；ordered absolute-path `shasum` stream SHA-256 `631a38c130f9d22e678c7626fa12b369e292a9e87575a8a1ecfc8ec5105a9aee` |
| M04 evidence root | 2 files / 644,087 B；ordered absolute-path `shasum` stream SHA-256 `d61c7fd8f8f2bca7632aa455901f2d51e52b2d0f13e122a3e0d07c83c9c518fd` |
| corrected replay TSV | 2,022 lines / 536,958 B / SHA-256 `ce3343854892890f1075182e7cc3325865c24111438a2cb0dfdf1bcfeb07e814`；header/data均 7 tab fields，2,021 data rows，0 non-MATCH |
| patched-source-base TSV | 932 lines / 107,129 B / SHA-256 `a1ab64b5ee977ea9d295fb141367891244a8ddc5b492597478332bb2ad647a79`；3 tab fields |
| post-stop support source | 649 lines / 24,820 B / SHA-256 `fd3301e94052d58ea3f4cec5b9aa9e9e6be3de0cbb66494361cb6c763339dcb8`；未执行，禁止复用 |

M05 preflight与final equality都必须证明两个 M04 partial roots保持上述 file/byte/hash identity，且
M04 material root继续 absent。任何漂移立即 `BLOCKED_M04_FAILURE_PRESERVATION`。

## 2. M05 create-once roots 与写白名单

签发时以下三个 exact roots 已读验证为 absent；owner在首个写动作前必须再确认：

```text
/private/tmp/fightmatch-loc-lic-alt-m05/loc-lic-alt-yamldotnet-16.3.0-m05/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m05/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m05/
```

唯一允许写入是这三个 M05 roots。material root仍只在全部 closure/seal gates通过后 create-once；
失败前保持 absent，若 seal后失败则原地标 `REJECTED_NOT_ACCEPTED`。M01/M02/M03/M04 roots、old
cache、005、LOC-A evidence、八个生产路径、仓库文档与产品、`Assets/Packages/ProjectSettings/.git`
均只读。禁止创建 B/L root。

M05 必须从 M01 accepted acquired/source manifests逐字节重放 approved material；不得复制或执行
M04 partial source、support source、feeds、comparison receipts、closure或其它产物。M01 source/
package material的逐 path/bytes/SHA要求、runner/tests local-only feed覆盖与 external network exact
`0 requests / 0 bytes` 均沿用 M04 §2。

## 3. receipt writer 的唯一修正

为避免再次把普通格式错误扩散成同 root 续写，M05 frozen standard-library-only support source必须
从第一次 invocation 就同时拥有：

1. M04 §4 的 graph inventory、canonical assets与全部 fixtures能力；
2. 一个 source/material replay comparison 子命令，直接写 UTF-8/no-BOM/LF TSV，不通过 shell
   `awk` 拼接。

`m01-to-m05-replay.tsv` header固定为以下 **7 个 tab-separated fields**：

```text
m01RelativePath	m01Bytes	m01Sha256	m05RelativePath	m05Bytes	m05Sha256	copyVerdict
```

每个 data row也必须恰好 7 fields，path用 source-relative POSIX `/`，bytes为十进制，SHA为64位
lowercase hex，`copyVerdict` 只能为 `MATCH`。writer关闭文件后、任何 closure source复制或 fixture/
restore前，必须重新以 raw bytes验证：无 BOM、仅 LF、header exact、每行 tab count exact 6、row count
与 M01 manifest exact、relative path ordinal set exact、0 duplicate、0 missing/unexpected/non-MATCH，
并保存 schema receipt。

`patched-source-base.tsv` 固定为 `relativePath / bytes / sha256` 三个 tab fields，执行同类 raw schema
与 set validation。不能把 spaces 当 delimiter，也不能在 schema失败后原地改写继续；失败立即
`BLOCKED_RECEIPT_FORMAT` 并按 §5 seal/stop。

support source必须在第一次执行前保存 path/bytes/SHA，之后只读；M04 post-stop source禁止作为
输入或模板。固定 Python path/version/hash、invocation budget、children/network规则沿用 M04 §4/§7。

## 4. 执行主体与不可重复门

完成 §1–§3 后，严格执行 M04 §§3–7：

- 从 M01 patched source byte-identical 创建 runner-generation、runner-verification、
  tests-generation、tests-verification 四份隔离 source；
- 冻结 runner 23 nodes / 56 directed edges、tests 13 nodes / 29 directed edges；
- 每 project保留自身默认相邻 `obj` 与 `packages.lock.json`，禁止任何共享 output property；
- 依序只做 runner generation → runner verification locked-mode → tests generation → tests
  verification locked-mode 四个 single-node restore；
- 生成完整 per-project lock sets、raw/canonical assets sets、package/license/deployment/B/L budget
  manifests，再 create-once material root与 final receipts；
- generation 的 exact `restoreLockedMode` property absent；verification/future B 必须 JSON boolean
  true；canonical comparison只删除该 exact property；
- fixed SDK dotnet starts expected/exact 4、external network 0/0、所有 process/storage/wall/equality/
  zero-action budgets与 M04 §7 相同。

M01/M02/M03、accepted correction与 M04 failure roots只重算 identity/equality，不复跑其 network、
signature、restore、fixtures、tests或 R。本文没有授权把 M04 已失败步骤算入 M05 成功证据。

## 5. first-failure stop 与失败 seal

任何 first failure立即停止所有 dependent phases，不得在同一 M05 root修脚本、重写失败 receipt、
重做 source copy、重试同一 argv或继续到下一阶段。失败 root原地保留；只允许一次 bounded failure
seal 写入 M05 evidence root，记录：first failure code/message/time、已执行动作、未执行动作、三个
root状态、当时 file/byte/hash manifests、process/network/zero-action receipts与
`BLOCKED_<EARLIEST_CAUSE>`。随后结束 turn并回主程；不得自行申请另一个 sibling。

`result.json` 成功或失败都必须如实保留 M04 history：M04 status
`BLOCKED_STOP_GATE_VIOLATION`、underlying `BLOCKED_RECEIPT_FORMAT`、收到 stop后的三类越界动作
（TSV rewrite、four source copies、support source creation）及未执行 fixtures/restore/material。

## 6. 回传与独立 R

成功时回传：actual thread/host/turn、三个 M05 roots、M04 preservation equality、predecessor/
correction seals、M01-only replay、receipt schemas、support source/runtime/fixtures、graph node/edge
sets、四次 exact restore/process receipts、per-project lock/raw/canonical assets sets、package/license/
deployment/B/L budget manifests、storage/wall/network、before-after equality、zero actions、material/
evidence/result bytes/SHA，以及唯一状态 `READY_FOR_INDEPENDENT_M_R`。

完成后先回工程主程，不自行送 R。主程机械核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`，只给 `ACCEPT` / `NEEDS_FIX` / `REJECT`。只有 R
`ACCEPT` 才能形成 `M-ACCEPTED` authority；仍不自动授权 B/L。

本文创建只新增本 packet。没有创建 M05 roots、复制材料、执行 Python/restore、联网、seal material、
build/publish/test、Unity/Luban/005、产品、B/L或 Git。
