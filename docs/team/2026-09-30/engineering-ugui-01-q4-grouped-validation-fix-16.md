# UGUI-01 Q4 correction 16 — bounded grouped coverage and native reopen

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_C`

## 0. 目标、责任与固定候选

本包只验证冻结的 FIX15 候选；不修改任何产品、测试、资源、配置或旧 evidence。它保留 FIX15
exact2 的有效 2/2 证据，以两个机械互斥的新 Unity 启动替代已超时的单进程 exact222，并在两组
均通过后执行一次 final native reopen。

- delivery owner / 唯一 Unity executor：C，thread/host
  `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：工程主程，thread/host
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` §6 standing five-role workflow；中央 thread
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` 已明确批准保留 exact2，并按
  Host30 → Core190 分组验证；三组机械并集为222，禁止原样重跑单进程222。
- FIX15 packet：183 lines / 11,022 B / SHA-256
  `b783763d60f5cffa6055044957a231dc55852caa7f914f8edf0c84707b5cd043`。
- candidate：
  `1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b`；
  sourceCandidate：
  `c45ed6d0c997867dc4bb60ac02d4f3ed832685b99149837c24c95aac7d8c2466`。
- FIX15 source manifest：10,009 B / SHA-256
  `72df96a0b26a6baf92821d21765a23b109dd24ed4d4540eedbcd3f1a1b912629`；
  minimal patch：1,897 B / SHA-256
  `cfbcdaaaa7d9e6149151fe8d459ddba22e9196cda968765374518c83c7b84e57`。
- FIX15 owner turn/final：
  `01a0f571-8f91-7dd2-a767-df451b843c33` /
  `msg_0d179092dafe07fa016abdd321219087d0bf3581ddba594fc2`；
  final receipt：100,609 B / SHA-256
  `b64f481353cca4b505aa02ce92fe98c431042cba4fa77a27f6a3268b8b408ae3`。
- FIX15 只读诊断：C turn
  `01a0f584-183d-7522-ab15-c398d2f640d2` / final
  `msg_0d179092dafe07fa016abdd538bd0487d08ea0a93545acfa64`。

成功只能回 `READY_FOR_INDEPENDENT_R`，不能自判接受、启动 LAYOUT、API24、APK、设备、
LOC/B/L或 Git。

## 1. 原超时事实永久保留

FIX15 exact2 已真实 2/2：

- P27 六种 mode完成；
- B09 `new-page` 与 `host-dispose` 均完成；
- 37资源稳定，字体 398 chars / 398 glyphs / 4 atlas。

该 XML 7,203 B / SHA-256
`d0fef5721fafa5dd872e37107425d5f53765f846df33e597cfeaa6aa1b4247e8`。
本包不得重跑 exact2。

原 exact222 的外层 waiter在 600.041 s 到限时 Unity PID `41913` 与五个 owned子进程仍存活，
exitCode `null`、无 XML；约46.9 s后的最终快照才观察不到这些进程，但未取得退出码或退出机制。
原状态永久是 `BLOCKED_EXACT222_TIMEOUT`，不得倒写为成功或“自然正常退出”。

- run：97,708 B / SHA-256
  `c4acc052ef5c38a3a6bdff47173fa2b8d49a6574fa17eec1d7433cf493631bf4`；
- Unity log：105,756 B / SHA-256
  `2a8e4f0860700314f46b3a0bc398841723506cef409895c08a34fc8ebccf9a9c`。

日志证明选择器是222个唯一 fullname且参数精确，没有 `-quit` / `-nographics`；五次 PlayMode
reload总计17.984 s，不足以解释600 s。最后明确活动到达 H05 打开/卸载
`FightMatchDemo.unity`，但不能证明 H05或此前测试通过，也不能区分累计过慢、Host/场景清理、
跨测试状态或 `RunFinished` 回调未完成。因此没有源码修补依据。

## 2. 零源码 delta 与新 evidence root

产品/测试/资源全部只读；当前53-source、37-resource必须精确匹配 FIX15 candidate/prebaseline。
不得改：

- 任意 `Assets/`、`Packages/`、`ProjectSettings/`、`Config/`、`Generated/`、`Tools/`；
- FIX15或更旧 evidence；
- 测试 fullname、断言、runner package或 project settings；
- TMP asset、prefab、scene、meta。

唯一新写域：

```text
TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-16/
```

必须 create-once。只保存静态输入、两组期望/实际 fullname、argv/filter证明、XML/log、UTC
progress/process samples、source/resource/font audits、reopen与 final receipt。evidence
`<=192 MiB`，单 log/XML `<=8 MiB`，start free space `>=2 GiB`。

## 3. 分组机械定义

只从 FIX15 冻结的
`q4-correction-15/static/expected-fullnames-222.json` 与
`expected-fullnames-2.json` 读入，不手工重抄 fullname：

1. retained exact2：原数组 exact 2，不运行；
2. G1 Host30：222中所有以 `FightMatch.Host.Tests.` 开头的 fullname，exact 30；
3. G2 Core190：222中排除 exact2 与 Host30 的其余 fullname，exact 190。

启动前必须保存三个数组、canonical hashes与比较证明，并机械断言：

- 222、2、30、190各自内部无重复；
- 三组两两交集为空；
- `Counter(exact2 + Host30 + Core190) == Counter(expected222)`；
- union exact 222，无漏项、无额外项；
- fixed28、六个参数化 cases、B16、WIRE01、P27/B09各自归属被明确列出。

任一不满足立即 `BLOCKED_PARTITION_CONTRACT`，不启动 Unity。

## 4. 有序 Unity 验证

固定 Intel Unity `2022.3.18f1`、Metal、有图形、EditMode；testFilter逐项 literal escape、
`^(?:...|...)$`首尾锚定。每次启动前确认项目没有其它 Unity/MCP/Hub/CrashHandler owner；
不得与其它 Unity任务并发。

### 4.1 launch 1 — G1 Host30

只运行 G1 exact 30，外层限额180 s。必须：

- Unity exit `0`，XML exact 30/30；
- actual fullname multiset与 G1 exact；
- zero fail/skip/inconclusive/error；
- H05 scene/resource测试完成，而非只到达 `OpenScene`；
- zero compiler/unhandled/MissingReference/serialization/hierarchy error；
- source不变，37-resource与font门通过，进程树最终 quiescent。

任一失败或超时立即停止；不得运行 G2/reopen，不得同包拆组、重跑或修补。

### 4.2 launch 2 — G2 Core190

仅在 G1全 PASS后运行 G2 exact 190，外层限额420 s。合同同上，另要求：

- fixed28、六个参数化 cases、B16/WIRE01均在 actual集合中按原断言通过；
- retained exact2的两个 fullname不在本次 actual集合；
- G1/retained exact2旧证据均未修改；
- 与 G1连续 source/resource baseline相等或只发生 FIX15 §4允许且可映射到冻结文案的合法 additive
  font growth；其它36资源、既有glyph/pixel/material/shader/GUID、atlas=4稳定。

两组与 retained exact2全部满足后，可声明
`GROUPED_EXACT222_COVERAGE_PASS`：同一冻结候选上的三组 actual fullname并集 exact 222，
222/222 zero failure。必须同时声明它不把原单进程 timeout倒写为成功，也不单独证明跨组状态污染
不存在；若任一证据显示顺序敏感，状态改为 blocker并停止。

### 4.3 launch 3 — final native reopen

仅在 grouped coverage通过后，执行 FIX15继承的一次 native Metal reopen/resource audit。
reopen前后53-source、37-resource必须 byte-identical；不允许新的字体增长。记录 renderer、process
tree、启动/退出时间、真实 exit code、scene/prefab/font/license/old evidence equality。
不得生成 APK。

## 5. UTC progress 与有界进程收尾

G1/G2 runner每5–10 s记录一次：

- UTC、elapsed、Unity PID与准确启动 argv；
- owned child PID/ppid/comm；
- log bytes/mtime及从上次 sample新增的最后一个非空日志位置；
- XML是否存在、bytes/mtime。

正常完成必须由 subprocess wait取得真实 exit code，并完成 quiescence检查。超时时：

1. 立即记录 timeout、last-progress、source/resource point-in-time snapshot与 process tree；
2. 保持 subprocess handle，再有界等待最多60 s以取得真实 exit；
3. 若仍存活，只可对本包创建且 PID/出生时间/argv/projectPath均精确匹配的 Unity根进程发送一次
   SIGTERM，等待最多30 s；禁止 global `pkill`、禁止匹配名称杀进程、禁止 SIGKILL；
4. 若仍存活，回 `BLOCKED_MANUAL_PROCESS_CLEANUP`，不再启动任何 Unity；
5. 无论之后是否退出，原分组仍是 timeout，不能视为通过。

每组都写清 started/finished/timeout UTC、waiter duration、退出码/信号、是否进入
`RunFinished` / `Saving results to`、XML产生时点。不得仅以
`Application is shutting down` 推断正常结束。

## 6. first-failure 与回传合同

新 Unity launch最多3个：Host30、Core190、native reopen。不得追加 exact2、exact4、focused28、
单进程222或其它探索运行。任何 first failure/timeout/resource drift后立即停止后续 launch；
候选保持冻结，不二次修补。

回主程必须包含：

- actual owner thread/host/turn、authority、packet/candidate/sourceCandidate；
- old FIX15 exact2与timeout evidence equality；
- 零 source delta、53-source/37-resource/font identities；
- 三组 partition arrays/hashes、pairwise-disjoint与 exact-union proof；
- G1/G2 exact argv/filter、actual fullname/XML/statistics、UTC progress与process receipts；
- fixed28/parameterized6/B16/WIRE01/P27/B09 coverage matrix；
- grouped222结论与原单进程 timeout保留声明；
- native reopen identity/equality/renderer/exit；
- old evidence protection、budget与 zero-action清单；
- 唯一状态 `READY_FOR_INDEPENDENT_R` 或最早 `BLOCKED_<CAUSE>`。

成功先回工程主程，不自动送 R。主程机械接收后由 QA交同一独立 R-TOOLS；其 limited pre-review
可复用，但最终 `ACCEPT` 必须明确审查新增 G1/G2/reopen证据。禁止 Git、LAYOUT、API24、APK、
设备、LOC/B/L或发布。
