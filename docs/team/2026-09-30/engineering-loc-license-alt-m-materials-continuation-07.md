# LOC-LIC-ALT-M-07 — table-driven preflight and clean material continuation

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 唯一目标与责任

本包是 M06 的 clean sibling。它不改变已接受的 lock-set design、M01-only replay、四份 isolation、
四次 restore 或 material schema；它只把此前手写 preflight 改为一份签发方冻结的机器可读清单和
一个通用检查器，移除 owner 自行补造的 identity/gate 后，在全新 M07 roots 完成原 M 流程。

- delivery owner：LOC-A implementation owner，thread/host
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已明确要求本 clean sibling，不重复设计 R、
  旧工具验签或已通过测试门。
- 基础执行合同：M06 packet，118 lines / 7,304 B / SHA-256
  `fa0dfe59913b7edc57ddd0ea000eef1cddbecd7ca678feca2bd35a66c3bf62ec`。
- accepted correction：384 lines / 21,402 B / SHA-256
  `4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；原 R verdict
  `ACCEPT`，turn `01a0f524-3498-73e2-81dd-759bd648af3b`。
- 唯一 preflight authority table：
  `docs/team/2026-09-30/engineering-loc-license-alt-m-materials-continuation-07-preflight.tsv`，
  39 lines / 9,396 B / SHA-256
  `5e4cb6215d438add05491cc7bdf046d7a2c8d1ff8525a3f90c3888bf15a93ce8`。

除本文明确替换的 preflight、M07 roots/name、M06 failure preservation 与回传 identity 外，M06
§§2–4 全部继续适用；M06 root/name机械替换为 M07。M06 继承 M05 §§3–5 的执行主体、receipt
writer、预算和 stop gate继续有效。冲突时本文优先。

成功终点只能是 `READY_FOR_INDEPENDENT_M_R`；owner 不得自判接受或开始 B/L、产品、
build/test/Unity/Luban/005/Git。

## 1. M06 失败必须保留

M06 owner turn `01a0f53c-e4ec-7af1-9106-69dbb4e70ce4` 固定为
`BLOCKED_PREFLIGHT`。最早失败是 owner 在 packet 外自行给 M02
`comparison/temp-files.tsv` 添加 `287,627 B` 断言；actual 为 `284,073 B`，但 packet 权威
SHA-256 `bc0b7752...d6c71` 完全匹配，所以 `predecessorDrift=false`。M06 未创建 temp/material，
未运行 support/restore/network/Unity/Git/产品/B/L，只写 bounded failure evidence。

M06 evidence 固定为 6 files / 6,904 B；逐文件 exact set 已全部列入 authority table。M04 的 2-file
set、M05 的 6-file set也在同一 table。M04 仍是 `BLOCKED_STOP_GATE_VIOLATION`（underlying
`BLOCKED_RECEIPT_FORMAT`，stop 后 TSV rewrite/four source copies/support source creation）；M05
仍是 `BLOCKED_M04_FAILURE_PRESERVATION`。所有失败 roots只读，不删除、不补写、不复用为 candidate。

M07 `result.json` 必须逐项保留上述 M04/M05/M06 history；不得把任何一个失败改写成成功。

## 2. 唯一允许的 preflight

### 2.1 table 是完整 authority，禁止 owner 扩展

owner 必须先 raw 校验 authority table：UTF-8/no BOM/LF、header恰 8 tab fields、38 data rows、每行
恰 8 fields、`setId/mode/root/relativePath/authorityPath/expectedBytes/expectedSha256/comparison`。
之后所有固定 identity 只能来自该 table；禁止根据人类文本、旧 shell 变量、经验或当前 `stat`
补造 expected bytes/hash/count/composite/tree aggregate。

特别是：

- M02 `comparison/temp-files.tsv` 的 comparison 明确为 `sha256-only`；owner 不得再检查其 bytes、
  lines 或 aggregate；
- M01 closed set 只能按 `manifest7` row：先核 authority manifest本身 bytes/SHA，再以其 first three
  columns 对 M01 temp root做恰 2,021 regular files 的 relativePath/bytes/SHA exact comparison；
- `exact-file-set` 必须同时检查 relative path set无缺无多及每项 bytes/SHA；
- `file` 只执行 `comparison` 列明确维度；空白 expected field绝不能推导新 expected；
- `absent` 只检查 exact root不存在；不得把父目录内容或其它 sibling 状态混入 identity。

签发前主程已用一个通用只读 checker实跑完整 table，结果
`PASS rows=38 groups=11 m01Files=2021`。这只是 packet 可执行性检查，不替代 owner 首次 preflight。

### 2.2 一个通用检查器，一次 preflight

owner 首个写动作前只可先做：

1. raw 校验本 packet dispatch identity和 authority table identity；
2. 只读确认 table 中三个 M07 roots均 absent；
3. 只读确认没有 command line含 exact M07 roots或 fixed SDK dotnet absolute path的遗留
   M-owned dotnet/MSBuild/NuGet process；只匹配这些任务专属标识，不把系统其它进程当 blocker；
4. start free space满足继承预算。

随后首个写动作只可创建 M07 temp root及一个 Python standard-library-only **通用 table checker**。
checker创建后立即冻结 path/bytes/SHA，之后只读；它必须遍历 table rows/groups完成 `file`、
`exact-file-set`、`manifest7`、`absent` 四种模式，输出每 row/group verdict和单一总 verdict。禁止再写
逐项 `check_file`、手拼 if/expected value、第二个身份检查器或补充 gate。checker只能读取，不得修改
predecessor；不得启动子进程或联网。

完整 table只运行一次。任一权威 comparison失败立即
`BLOCKED_PREDECESSOR_AUTHORITY_TABLE_DRIFT`；不得改 checker/table/expected 后重试。全部通过后才
进入 M01 replay/receipt writer/support fixtures/graph/restore主体。

### 2.3 明确移除虚假全局门

M 是 Assets 外纯离线材料线：不启动 Unity，不读写 `Library/Temp`，唯一写入是三个 M07 roots。
`AGENTS.md` 禁止的是在项目已由另一 Unity 打开时再启动 batch Unity，不是禁止 M 与唯一 C 并行。
因此：

- “系统/项目无任何 Unity/MCP/Hub/CrashHandler 进程”不得成为 M07 preflight、stop gate 或 verdict；
- 可记录只读 process observation，但 Unity presence/absence不能阻断 M07；
- 仍严格保持 M07 `unityInvocationCount=0`，不得向现有 Unity 发命令；
- before/after equality只按 M 合同的 old cache、005、LOC-A roots、M01–M06、八个生产路径及明确
  manifests检查；并行 FIX13 的 8-file whitelist、团队新 packet/receipt属于外部授权写，不得用
  whole-repo `git status` 把它们误判为 M 污染。M07 不得读取它们作为 candidate，也不得写它们。

## 3. M07 create-once roots 与写白名单

签发时以下 roots 已由 authority table确认 absent：

```text
/private/tmp/fightmatch-loc-lic-alt-m07/loc-lic-alt-yamldotnet-16.3.0-m07/
/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m07/
/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m07/
```

唯一允许写入是这三个 M07 roots。material root只在全部 closure/seal gates通过后 create-once；失败
前保持 absent。M01–M06、old cache、005、LOC-A evidence、八个生产路径、仓库文档/产品、
`Assets/Packages/ProjectSettings/.git` 均只读。禁止创建 B/L root。

M07 只从 table 已闭合验证的 M01 accepted root/manifest逐字节重放 approved material；不得复制或
执行 M04/M05/M06 partial source/support/feeds/receipts。M04 replay TSV在 preflight仅作为 M01
`manifest7` authority；candidate bytes仍必须来自 M01。external network exact `0 requests / 0 bytes`。

## 4. 执行主体与停止门

table preflight全 PASS 后，严格执行 M06 §3 / M05 §§3–5 / M04 §§3–7 的有效技术合同，全部名称
和输出机械替换为 M07：

1. 第一次 frozen support source生成并 raw-byte校验 7-field `m01-to-m07-replay.tsv` 与 3-field
   `patched-source-base.tsv`；禁止 shell `awk` 拼 TSV；
2. 从 M01 patched source创建 runner-generation、runner-verification、tests-generation、
   tests-verification 四份 byte-identical source，冻结 runner 23 nodes/56 edges、tests 13/29；
3. 依序四个 single-node restore：runner generation → runner verification locked-mode → tests
   generation → tests verification locked-mode；fixed SDK dotnet starts expected/exact 4；
4. 每 project保留默认相邻 `obj` / `packages.lock.json`；生成完整 raw/canonical assets、lock sets、
   package/license/deployment/B/L budgets，最后 seal material；
5. generation `restoreLockedMode` exact absent；verification/future B exact JSON boolean true；canonical
   comparison只移除该 exact property；
6. process/storage/wall/network/equality/zero-action budgets全部继承，且 M07 network 0/0。

任何 first failure立即停止；只允许一次 bounded failure seal，不得在同一 root修 support/checker、
重写 receipt、重做 source copy、重试 argv或继续 dependent phase。失败回最早 `BLOCKED_<CAUSE>`；
不得自行申请 M08。

## 5. 回传与独立 R

成功回传 actual thread/host/turn、三个 M07 roots、packet/table/checker identities、table 38-row/11-group
逐项结果、M04/M05/M06 failure preservation、M01-only replay、receipt schemas、support runtime/fixtures、
graph sets、四次 restore/process、per-project lock/raw/canonical assets、package/license/deployment/B/L
manifests、storage/wall/network/equality、zero actions、material/evidence/result identities，以及唯一状态
`READY_FOR_INDEPENDENT_M_R`。

先回工程主程，不自行送 R。主程机械核 actual evidence后交原 LOC 独立 R
`01a0e404-e8ee-7310-8388-9260babd53f1 / local`。只有 R `ACCEPT` 才能形成 `M-ACCEPTED`；仍不
自动授权 B/L。

本文创建只新增本 packet及其 preflight TSV。签发阶段未创建 M07 roots、复制材料、执行 restore、
联网、seal material、build/publish/test、Unity/Luban/005、产品、B/L或 Git。
