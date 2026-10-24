# UGUI FIX15 limited source / exact2 pre-review

2026-10-01（Asia/Shanghai） · 同一 R-TOOLS · 有限预审

**VERDICT: PRE_REVIEW_PASS**

本轮未发现 FIX15 源码纠正项；两文件补丁、冻结范围和 exact2 原始证据通过有限预审。剩余为 222 超时的运行／诊断门，以及其后仍未运行的 native reopen 与完整独立接收。本结论不是 FIX15 整体接收，不解除 LAYOUT 前置，不授权追加 Unity 或原样重跑 222。

**身份、授权与输出范围**

- Reviewer thread：`01a0f3e3-9663-7be2-84a8-ed23be32f0b9`；host：`local`；turn：`01a0f584-db60-7883-979c-df1d96e26d7d`。
- 派发／收件：主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb`；用户 authority：`AGENTS.md` §6 standing workflow，以及本轮 source/exact2 有限预审委派。
- C owner：`01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local；turn：`01a0f571-8f91-7dd2-a767-df451b843c33`；final：`msg_0d179092dafe07fa016abdd321219087d0bf3581ddba594fc2`。
- 唯一新增：`docs/team/2026-09-30/testing-ugui-fix15-source-pre-review-001.md`。没有修改产品、测试、配置、资源、meta、旧 evidence、旧回执或 testing.md；本轮未调用 Unity、test、build 或 Git。
- Reviewer final message ID 写文件时尚未产生，由 QA 从上述 thread/turn 完成事件取得；不预填，也不使用上一回合 ID。

**固定输入门**

候选：`1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b`。
sourceCandidate：`c45ed6d0c997867dc4bb60ac02d4f3ed832685b99149837c24c95aac7d8c2466`。
下表除 packet 外均相对于 `TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-15/`。

| 固定输入 | bytes | SHA-256 |
| --- | ---: | --- |
| `docs/team/2026-09-30/engineering-ugui-01-q4-active-bind-fix-15.md`（183 行） | 11022 | `b783763d60f5cffa6055044957a231dc55852caa7f914f8edf0c84707b5cd043` |
| `minimal.patch` | 1897 | `cfbcdaaaa7d9e6149151fe8d459ddba22e9196cda968765374518c83c7b84e57` |
| `observed-source-manifest.json` | 10009 | `72df96a0b26a6baf92821d21765a23b109dd24ed4d4540eedbcd3f1a1b912629` |
| `final-receipt.json` | 100609 | `b64f481353cca4b505aa02ce92fe98c431042cba4fa77a27f6a3268b8b408ae3` |
| `exact-2-graphics/run.json` | 59760 | `f2acdbbd9801c29b7b63a268ba1d4e67a50cb8cba0777c1ce40f4f2f0baaa8d3` |
| `exact-2-graphics/tests.xml` | 7203 | `d0fef5721fafa5dd872e37107425d5f53765f846df33e597cfeaa6aa1b4247e8` |
| `exact-2-graphics/unity.log` | 110413 | `d51c56ceb51127aa96878b10a1d2249e842a81528986cafaf7dfcc05f3022d79` |
| `exact-222-graphics/run.json` | 97708 | `c4acc052ef5c38a3a6bdff47173fa2b8d49a6574fa17eec1d7433cf493631bf4` |
| `exact-222-graphics/unity.log` | 105756 | `2a8e4f0860700314f46b3a0bc398841723506cef409895c08a34fc8ebccf9a9c` |
| `exact-222-graphics/process-exit-observation.json` | 31853 | `147083e2b2893eb048c135bbfd4c5d9844751cdd2e489b00bd1fb54a5669c371` |

全部入口身份匹配。对 `static/candidate-identity-input.json` 和 53 项 source manifest 重新执行 sorted-key、compact canonical JSON 的 SHA-256，分别得到上述 candidate 与 sourceCandidate；不是仅复述作者字段。

**源码与范围：PASS**

| 文件（均在 `Assets/Scripts/FightMatch/Presentation/`） | pre bytes / SHA-256 | post bytes / SHA-256 |
| --- | --- | --- |
| `CandidateBattlePlaybackView.cs` | 13828 / `3e79a24e00614bcdd7691cbc05ed671b649d31c693f93208514c2a218f316083` | 13944 / `38c7cfbb999897db56e90c37b6c7cacdef95d0aaabae87b8c671e1f262ea6216` |
| `CandidateBoardInputView.cs` | 34488 / `3b2355f306cbc3b5062fce8833415a07470b09e00b69f7702a147e7ede6dc9a5` | 34540 / `659d879b2154e3e7b9347638cfeaf5f3f33b4daea8908b14711712170b5ed3ed` |

静态 preimage 与 packet §2 精确匹配；postimage 与当前源码字节相同。由 pre/post 独立生成的 unified diff 与 1897 B minimal.patch 完全一致；移除三条新赋值并恢复唯一拆开的既有语句行后，逐字恢复原 preimage。

- Input L39–43：参数校验、既有 Unbind、新 Controller/localization 赋值后，在 Subscribe 前以 `isActiveAndEnabled` 同步自身 `structuralRefresh`。active 路径当次到达 BindNative L59 的 `board.Bind(controller)`；inactive 保留托管订阅，Render guard 拦截，后续 OnEnable L191–195 执行原 native attach。
- Playback Attach L29–35 与 Bind L39–44：完成既有检查、新 owner/localization 与 nested input 入口后，在 BindCallbacks 前各同步自身状态；active 当次 BindNative，inactive 由既有 Render guard 与 OnEnable L230–234 延后 native 工作。parent 不替代 nested input 的独立状态判断。
- Input Render L143、Playback Render L134 的全部 guards 原样保留；nativeBound 幂等、closed/callbacksActive、100ms refresh、schedule/low-memory、listener owner/epoch、pointer cancellation 与 Close/Unbind 顺序均无改动。
- Input L93–130、Playback L75–112 的 managed/native teardown seam、OnDisable/OnDestroy 均与 FIX14 preimage 相同；没有 catch/fallback、fixture hook、反射、断言弱化、新 public API、serialized 字段或依赖。

53-source pre-manifest 等于 FIX14 observed manifest；FIX15 只有上述两条 identity 改变，其余 51 条和所有阶段 source manifests 一致。FIX14 另外两文件当前仍为 `PlayerBattleView.cs` 25884 B / `3baa9fc9f2f4a565a4933130e7f5c46e6c16edf4e7a3a0d13c8dac9761201e7c`、`PlayerDefaultReferenceView.cs` 18882 B / `b3f28fd28df9687e8a92b95a81b2bd8d65fe974e43b962436b2a75370bfb8cac`。

1004-path closed prebaseline 等于 FIX14 final closed set；FIX15 fixed set 只替换上述两条 source identity，没有增删路径。实际磁盘全部 1004 条与 fixed set 匹配；exact2、222、final closed snapshots 一致，tests／fixtures／controllers／Host／localization 保持冻结。继承 expected-fullnames-4、28、222 与 FIX14 各自文件 byte-identical；4、28 都是 expected222 子集，此处不代表 actual222 已执行通过。

**exact2 原始执行证据：PASS**

独立解析原始 XML：准确两个唯一 fullname，Counter/set 与冻结 exact2 完全一致；root total=2、passed=2、failed/skipped/inconclusive=0、result=Passed，两条 test-case 均 Passed，无失败条目。run 的 argv 使用逐 fullname literal escape 与首尾锚定，包含 -runTests/EditMode，没有 -quit 或 -nographics；日志 L7 为 x86_64、L149 为 Unity 2022.3.18f1、L156–157 为 Apple M4 Metal。

| actual fullname | 原始结果 | 覆盖核对 |
| --- | --- | --- |
| `FightMatch.Core.Tests.CandidateBattlePlaybackPanelTests.P27_03_SkipCloseDetachBlurLowMemoryAndSchedulingFailureReleaseOnlyOwnResources` | Passed | 冻结测试 L95–176 的六 mode 循环、前置取消顺序 helper、logical-update-before-token、A2 隔离和中断文案断言均保持 |
| `FightMatch.Core.Tests.PlayerBattlePresentationTests.B09_AnotherPageOrHostDisposalDetachesEveryOldBorrowedCallback` | Passed | 冻结测试 L154–166 的 new-page 与 host-dispose 两分支、旧 Controller 清空、stale click、head/files/clocks 与 LastRequest 不变断言均保持 |

P27 XML output 含 skip、close、detach、low-memory-equivalent、schedule-error-equivalent 五条 A2 完成标记及 blur 的真实 product handler 标记；结合完整 Passed 与未删改循环确认六 mode。blur 证据只证明直接调用产品 focus handler，OS 焦点投递仍属 Q7。B09 没有逐 mode 输出；其两分支覆盖依据是无早退的冻结 foreach 和原始 Passed，不伪称存在额外运行日志。

exact2 run：2026-10-01T03:14:59.731027Z 至 03:15:54.275903Z，Editor exit=0，54.543169s；含 quiescence 共 61.353994s。进程清理记录为一次精确 owned compiler pipe graceful client、0.468731s、exit=0、server/client gone、signalsSent=false；在 222 启动前完成。项目相关 compiler/unhandled、missing-reference/serialization、null/assertion 与 hierarchy error 均未发现；日志 L15–28 的 licensing signature/token/SSL diagnostics 原样存在，不声称整个日志没有任何 Error 字样。

**37 resources 与 398 / 398 / 4 语义 prebaseline：PASS**

实际 37 项资源逐一 bytes/SHA 匹配 static prebaseline；exact2 与 222 的 before/after、最终 observed resource manifests 全部相等。font 前后四份保存字节及当前磁盘 asset 全部等于 8573334 B / `290a57ac32c5ce7d965bc771d0e65e5c9f543f98df28b6e2e360f8dd6b2196c0`；直接数 YAML 表项为 398 characters、398 glyphs、4 个 Texture2D atlas。其余 36 项 byte-identical，包括受保护 prefab/scene/meta/license；没有新 glyph、旧 entry/pixel 改变或资源漂移。

packet §4 明确提升的 FIX14 font-chain audit SHA `531a5e016a72febf586290cd457e4a174e7dc9a6415dbab91fe9f9eaba1511c2` 实测匹配；396→398 只增加 U+663E「显」和 U+793A「示」，atlas 仍4。冻结 CSV 的 `fm.battle.playback.interrupted` 简中文案确含两字；本次实际通过的 P27 schedule-error 分支 L162–168 保留该键的完整本地化断言。FIX15 没有新增字符，故不存在待补的新增字符映射；不能据此宣称有逐 glyph 渲染追踪或未来阶段字体门已过。

FIX14 原 final receipt 与 failure inventory 也匹配 packet 固定 SHA，仍是 `BLOCKED_EXACT2_WITH_RESOURCE_DRIFT`；本次 prebaseline 提升不改写 FIX14 历史失败。

**222 timeout 与 reopen 边界**

- 原 222 run 于 03:16:16.481220Z 启动；600.041069s 超时，exitCode/endedUtc 均 null，passed=false，tests.xml 不存在。原 run 的 RAW_TESTS_FAILED 是阶段失败状态，不足以断言具体测试失败或确定卡住的测试。
- 原 process-exit snapshot 于 03:26:16.550208Z 仍有 Editor PID 41913，quiescent=false；final-process snapshot 于 03:27:03.422381Z 才记 remaining=0、quiescent=true，final receipt 记 allOwnedProcessesExited=true、signalsSent=false。后续退出不能补出超时阶段的 exit=0 或 XML。
- timeout 时 snapshot 所列 log 为 105657 B，冻结最终 log 为 105756 B；该时点差异保留，不把较晚的停止／退出观察倒写为预算内完成。
- native-final-reopen 明确 NOT_RUN；两次 Unity launch 后停止，没有第三次 reopen，也没有额外 exact4/focused28。B16/WIRE01、fixed28、六参数实例在本候选的完整执行覆盖未闭合；旧绿测与 expected 集合关系不能替代 actual222。
- final-receipt 的 `CURRENT_222_EXECUTED`／部分 `status: RUN` 字段仅能理解为该阶段曾启动；没有 XML、实际项为 NOT_RUN、passed=false 和顶层 `BLOCKED_EXACT222_TIMEOUT` 是本轮接收边界。空 failure-inventory 也不构成 222/222 通过证据。

本轮只给源码／exact2 预审结论。原始日志和 runner 的只读诊断继续由现有负责人处理，先确定超时原因及可验证的最小后续方案，再由主程签发所需执行范围；本回执不建议原样重跑 222，不启动任何补跑，不据现有资料认定根因属于源码或环境。

**验证回传与证据保留**

本轮实际执行的是只读 Python bytes/SHA、canonical identity、pre/post diff、XML/fullname、CSV/font 表项与保护集合核对；没有运行任何项目测试。旧 evidence 的 15-root / 927-file manifest 在 exact2／222 四个阶段快照中相等，FIX14 closed manifest 的固定身份另行核实；未重哈希全部约 396 MB 历史 payload，不能把 manifest 对比描述成全量历史内容复验。

写入前目标文件不存在。落盘后仅对本新增文件执行非 Git 静态 diff-check，并复核固定输入及当前冻结源码／资源；最终 path、lines、bytes、SHA-256 与检查结果由本回合 final 返回。文件不内嵌自身哈希。回传后等待 QA 收件。
