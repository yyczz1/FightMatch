# UGUI-01 Q4 FIX18 — 一次 Core190 的持久进度诊断

2026-10-01（Asia/Shanghai） · `PREPARED_NOT_AUTHORIZED`

本包仅准备。中央绑定实际 PR/head、确认冻结解除及本机 graphics 例外后，才能向 C 激活；本文不启动执行。
owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`；收件：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，测试回执交主测试。
authority：中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 的本轮 DELTA 准备指令及 AGENTS §6；主要执行会话 `gpt-6-astra/xhigh`。
当前 [AGENTS](../../../AGENTS.md) 与 [TEAM_WORKFLOW §7](../../../.agent/TEAM_WORKFLOW.md#7-github-pr-code-review) 覆盖旧包的模型和本地 R 门；代码审查只走实际新 head 的 GitHub PR。

## 1. 固定输入与这次差异

继承 [FIX17](engineering-ugui-01-q4-host-playmode-fix-17.md) 的产品、测试语义和资源保护，以及 [FIX16](engineering-ugui-01-q4-grouped-validation-fix-16.md) 的 selector、进程身份和证据合同。仅替换运行次数、600s 预算、Release 参数与进度记录。
`R17 = TestArtifacts/FightMatch/UGUI-01/872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf/q4-correction-17`。
`R15 = TestArtifacts/FightMatch/UGUI-01/1dc19bf8048aa36d905e0e72c922f9cc05646cfe4e5ed55ce7bf0205d58c4c1b/q4-correction-15`。

| 固定输入（相对 R17，另注除外） | SHA-256 |
| --- | --- |
| `static/source-manifest.json`（53源） | `c55e7b98e4c43a1a952735ce86b14b40675744ed428663b6de548acde3f73fa6` |
| `static/product-only-manifest.json`（29产品） | `07f3fa5e6c2fe155a17e69427e77dd6177fb897f33a04ec96120a9b3931317ac` |
| `static/resources-manifest-before.json`（37资源） | `7886a648bda97657a58eb0ed15cfdaa9a661fe451c089979776f4c76a36263c9` |
| `core-190-graphics/expected-fullnames.json` | `4bbbb361daddb91f8a112bf99ccbfa3d2e2a2e2ee4fccbc24cb30b6666e9d78d` |
| `core-190-graphics/test-filter.txt` | `001358c4913197180f43f9cc2466689311acccf0f763fde1b9165df85f75ead2` |
| `host-30-graphics/tests.xml`（30/30） | `79b428cf22371302534406ea1e6634d7bd3af999685ab577e7abe8f00310986c` |
| `final-receipt.json`（原超时保留） | `70bc7eb49f78d29ebc47692ac691b2f1d00772bbc1d13654bf71445c01228f07` |
| R15 `exact-2-graphics/tests.xml`（2/2） | `d0fef5721fafa5dd872e37107425d5f53765f846df33e597cfeaa6aa1b4247e8` |

FIX17 的 argv 未含 `-releaseCodeOptimization`；600s 沿用已接收本机 Release 专项的预算依据，不承诺本轮耗时。旧 Core190 420s、旧 exact222 600s 无 XML 的失败保持。
激活回执必须给实际 PR URL/head、执行端及本机例外、本文 SHA、owner turn、输出绝对路径；head 未绑定时仅为准备包，不能用工作区 HEAD `125b849` 代替审查 head。

## 2. 精确新增白名单与接线

仅新增两个 Assets 路径，现有 90 个快照路径逐字保持：

- `Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs`：测试程序集中的诊断回调及同文件 Editor 生命周期桥接。
- `Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs.meta`：普通 MonoImporter，固定唯一 GUID `8d190c536fc54e848d496974e653a731`；创建前查重，冲突即停止。

接线使用现有 `FightMatch.Core.Tests` 的 TestAssemblies 引用，文件顶层 `[assembly: TestRunCallback(typeof(FightMatch.Core.Tests.FightMatchTestProgressCallback))]`；类实现 `ITestRunCallback`。不改 asmdef、现有测试体、断言、测试名/参数、产品、包、配置或 UTF 源码。
仅存在准确 `-fightMatchTestProgress <O>/test-events.jsonl` 参数时启用；参数缺失时不注册额外 Editor 回调、不创建文件、不记录日志。路径必须规范化且严格等于本包新输出，父目录由 runner 创建并排除链接；拒绝旧文件/任意路径。

UTF 1.1.33 的本地实现决定如下接线，不能假定四个 assembly 回调都及时到达：

1. `EditModeLauncher.Run` 在 `m_EditModeRunner.Run()` 后才添加 `TestRunCallbackListener`，首次 `RunStarted` 已发出。故同文件通过 `[InitializeOnLoadMethod]` 在启用时向公开 `TestRunnerApi` 注册一个 `ICallbacks` 桥接，priority `100`，只写实际 `RunStarted`/`RunFinished`；后者先于 priority `-10` 的 CLI ExitCallbacks。attribute 的两个 run 方法留空，避免重复计数。
2. attribute 回调只写 non-suite `TestStarted`/`TestFinished`。`EditModeRunner` 序列化 `m_CallbackObjects`，重载后 `OnEnable` 重新挂监听；`TestRunCallbackListener.m_Callbacks` 不序列化，会重新按 assembly attribute 构造。Editor 桥接也须每域重新注册、同域只注册一次。
3. 每条事件独立 open/append/write/flush/close，记录全局递增 `seq`、UTC、PID、每域唯一 token、事件来源、test id/fullName、result/duration。`seq` 从已落盘末条读取，不能依赖跨域 static 计数；保留真实恢复事件，不能去重掩盖重复启动。
4. 新文件由 runner 在启动前 CreateNew；回调仅追加。JSONL 上限 4096 条/4 MiB、单条 16 KiB；写入/解析/容量失败必须留下明确错误并使证据门失败，不改测试结果或吞掉原测试异常。桥接完成事件和外层 process-exit 是两种不同事实。

静态依据：UTF `EditModeLauncher.cs:62–69`、`EditModeRunner.cs:128–145,394–410`、`TestRunCallbackListener.cs:13–32`、`TestRunnerApi.RegisterCallbacks` 和 `ExitCallbacks`；只读这些已安装 1.1.33 文件。

## 3. 单次执行和固定 argv

`P = /Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；`O = P/TestArtifacts/FightMatch/UGUI-01/q4-correction-18`，只创建一次。
O 只允许 `activation.json`、`preflight.json`、`validation-tools/runner.py`、`static/{inputs,delta-manifest,expected-fullnames,partition-proof}.json`、`test-filter.txt`、`test-events.jsonl`、`process-events.jsonl`、`progress.jsonl`、`run.json`、`tests.xml`、`unity.log`、`editor.stdout.log`、`editor.stderr.log`、`{before,after,timeout-snapshot,quiescence,grouped-exact222-coverage,final-receipt}.json`；未到阶段的叶不制造成功占位。
runner 仅从 FIX17 runner 派生本包差异，修正旧 root/阶段依赖；输出完整 diff/hash。总证据 ≤192 MiB，单 log/XML ≤8 MiB，free-space 起止 ≥2 GiB；旧 evidence 全部只读。

执行前机械核 §1 全部 identities、manifest 中每个实际文件、原 exact2/Host30 实际 Counter 与零失败、partition 三组互斥且并集 exact222；新回调不新增测试。记录既有 53源/29产品/37资源与新增二文件分别的 manifest，形成新诊断 candidate，不能冒称旧 candidate 字节完全相同。
先静态核注册顺序、无参数零写、序号恢复、失效路径/部分行/超限 fail-closed 和 selector literal escaping；沿原规则确认无其它 Unity family/先前 owned process。
唯一 Unity argv 数组如下，`F` 是 R17 冻结 `test-filter.txt` 的完整原字节字符串，不经 shell 展开：

```text
/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity
-batchmode -releaseCodeOptimization -projectPath P
-runTests -testPlatform EditMode -testFilter F -testResults O/tests.xml
-fightMatchTestProgress O/test-events.jsonl -logFile O/unity.log
```

只有一次 graphics/Metal Core190；启动/必要导入编译/测试/UTF退出全部计入从 launch 起的 600s。没有额外 Compile、Host30、exact2、singleton、exact4、186、222或 native reopen 启动；后者仍是后续独立门。为控制变量，本包不追加 `-disableManagedDebugger`、`-buildTarget`、`-automated`、`-nographics`、`-quit`。
外层每5–10s沿原进程/日志/XML采样，同时追加 launch/deadline/timeout/process-exit；600s到限即失败，最多60s自然观察，再沿 FIX16 精确 PID/出生时间/argv/project 守卫单次 SIGTERM并最多等30s。仍存活回人工收尾 blocker；不重跑。完整墙钟和测试预算分别如实报，不扣除观测成本。

## 4. 收件门与停点分类

通过要求：600s内自然 exit0、原190 fullname/重数全部 finished且Passed、真实 RunFinished、XML exact190/190且零fail/skip/inconclusive/error；事件序号连续且跨域可续，旧53源/29产品/37资源全等，字体398字符/398glyph/4atlas，无编译/序列化/生命周期错误，owned进程全部退出。
以原 exact2、FIX17 Host30及本次Core190实际 XML 作 Counter并集证明，可回 `CORE190_PASS_GROUPED222_EVIDENCE_READY`。必须注明三个不同运行的证据复用、唯一诊断增量和Release差异；这不关闭 native reopen、GitHub审查或最终产品门。

| 实际证据 | 回传解释 |
| --- | --- |
| non-suite started 无对应 finished | 指定 fullname/domain 的未完成边界；不能仅此断言业务死锁 |
| finished 前缀后停住、无 unmatched started | 下一项调度/域恢复边界 |
| 190 finished，但缺 RunFinished/XML/exit | runner finalization/退出边界；列明缺哪一项 |
| 600s后60s内自然补齐 | 本次预算不足，仍为 TIMEOUT，保留真实晚到结果 |
| 660s仍未补齐 | 停点已定位；hang 与更慢尚未区分 |
| 回调缺失、写入失败或轨迹断裂 | `BLOCKED_PROGRESS_EVIDENCE`，不能从无事件推出具体测试停点 |

首败封存后回主程：actual thread/host/turn、PR/head与诊断delta身份、完整argv、计时/事件/XML/进程/资源门、最后一个finished与未配对started、原证据复用证明及未运行项。作者不自判接受；主测试核实际结果，代码变更由中央同步到对应 GitHub PR head 后接收审查。
