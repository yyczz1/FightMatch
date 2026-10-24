# RES-COMBINED-V05 纠正前置方案

中央唯一收件。本文为 V04 实际失败诊断，不是独立代码审查；下述自有socket探针已完成，最新决策见补充；不能猜改解析器后再跑Unity。保留V04 FAILED及所有原证据。

## 可证明的事实

证据根E4=`TestArtifacts/FightMatch/RES-COMBINED-V04/run`。actual=`01a1193b-f712-7691-a05d-a24a7de3dea8`，receipt75893B/SHA256 `d1ff9979a0ec259c2aa352b9e4b2f5c682e8e18377e0b0a36584b918dccd265c`。I一次exit0、runnerFAILED、T0；compile.stages为空、fullImportedInputs=null，不能补认完整编译来源或复用为合格I。

1. runner.py:59—71只接受lsof的n字段为精确全路径，或加 ` type=STREAM` / ` type=STREAM (LISTEN)`。:106—111取得stdout/stderr后只解析，没有落原始探测记录；process-events没有bee探测或准入事件，也无独立lsof输出文件。因此不能从现有证据证明是macOS字段格式、截断、连接后地址表示，还是采样竞争。
2. 四次绑定失败时刻为02:01:26.887809、27.897704、29.925091、32.023060Z。Editor根62180；Bee后代62271于27.029749Z已发现，其ppid、argv含--ipc、cwd=P。首次采样可能未包含该后代，但后三次仍失败，不能把全部失败解释为首次发现延迟。失败位置说明之前类型/UID/mode等检查已通过，不等于已记录完整socket前像或FD归属。
3. closure_begin=02:01:26.986057Z；ps错误=02:02:26.994550Z，phase=natural-closure、scope=process-snapshot。命令 `/bin/ps -ww -axo pid=,ppid=,lstart=,stat=,comm=`，TimeoutExpired.timeout=0.0025610830052755773秒。probe_timeout先算自然截止余量，再min(5秒,余量)，cmd将其传给subprocess.run。这是被自然窗口压到2.561ms的真实subprocess超时，不能描述为ps耗尽固定5秒或整轮900秒。
4. term_window=02:02:26.995492Z；唯一TERM62301；closure_end=02:02:28.184827Z，remainingOwned=[]、closed=true。恢复事实不消除前述失败。

## 先做一次格式探针，暂不启动Unity

由中央单独限定授权，一次标准库Python自有AF_UNIX/SOCK_STREAM实验，最多15秒、无重试；只在新的 `/private/tmp/fm-bee-fd.<8>/xxxxxxxx.xxx/` 创建两项 `ipc_<本探针PID>_htc/cth`。目录0700；记录本探针PID、Python/lsof路径身份和本机版本；lsof复用V04已固定 `/usr/sbin/lsof`（470608B/SHA `deff85c7b4750a948a69fcd48eae3b394a37575e5048e5eb4a35b7a0482c3dc8`）。不启动Unity，不查询他人PID，不接触既有IPC、缓存、玩家文件或网络。

单一进程保有全部FD，顺序保存三次快照：两监听端；各自connect/accept且保留监听端；关闭监听FD但保留已连接双方。每次仅 `/usr/sbin/lsof -a -p <自身PID> -U -Fpcftn`，单次最多2秒，保存argv、开始/结束、timeout、exit、原始stdout/stderr字节（可base64）、每个socket的lstat与本进程已知FD号/角色/fstat；证据输出上限64KiB，超限失败。finally仅关闭自己的FD、unlink自己创建且身份未变的两条socket，记录清理结果；不得清理旧根或他人条目。输出白名单仅 `TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.py`、`TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.json`；后者包含三次原始输出和清理结果。TMP仅上述随机根、直接子目录和两条socket；完成后只移除自身身份未变的空目录。本设计轮不创建探针或结果，不编码V05。

n字段判据：按p/f/t/n记录边界关联本探针PID及已知FD，t必须为unix；逐字比较n与对应完整socket路径及V04已有两种STREAM后缀。保存不匹配原串，区分无n、地址箭头、截断或未知装饰，禁止basename/前缀/子串匹配。把真实输出作为固定离线fixture交回中央，随后再喂既有bee_fd_bound；本探针不改V05解析器，不伪造成功。若存在确定的macOS n字段装饰，只调整被实证的语法，仍要求精确完整路径和unix/FD/PID绑定。若连接后全路径不可观测，明确现有lsof合同在该生命周期不可满足；不要按PID/名称/地址前缀放行。可进一步评估本机libproc对限定已拥有FD返回的Unix本地地址作为替代，但先用同一自有socket证明它能返回完整地址并固定字段映射，不能把未经验证的API要求直接压给下一次Unity。Python探针只能证明本机表示能力，不证明Mono/Bee实现相同。

V05必须保存每个实际Bee绑定尝试的有限原始FD输出、候选PID身份、前后lstat、解析结果/失败原因，即使绑定失败也保留；仍限定本轮Editor及认可Bee后代，不扩大进程或特殊文件白名单。若探针不足以确定可实现归属观测，IPC项保持BLOCKED，中央收件后决定下一步，不以新Unity尝试补猜测。

## 已可实施的闭合边界修正

不要把本次TimeoutExpired事后吞掉。改成在发起阻塞探测前做准入：自然窗口必须有足够的完整探测额度（ps沿用5秒，FD探测沿用固定5秒），否则记 normal-natural-boundary，不启动该探测，沿原控制流转TERM；每次后续阻塞探测都重新检查。自然截止、TERM和总截止仍为原绝对时钟，保留先前错误、恢复预留与剩余预算门。

合同需明确自然60秒是最多等待，最后不足一个完整探测额度时允许提前转TERM，不再发起2.5ms截断探测。只有自然窗口正常耗尽适用；总/工作/TERM预算不足、真正已启动探测的TimeoutExpired及I/O错误仍失败。禁止通用except TimeoutError转正常；不得延长900秒或伪造闭合成功。

待探针收件后才实施的最小离线增量：余量2.561ms不调用ps且转TERM；余量足额调用并保留真实TimeoutExpired失败；总预算先耗尽失败；自然转TERM不清既有Bee错误；TERM仍需fresh闭合证明；三种真实socket输出fixture及错误PID/路径/类型/字段负例。保留177历史有效回放与V04已有回放证据，仅执行相关受影响链和新增例，不能冒称旧结果是新head通过。

## 合同字段及后续边界

仅改FD观测表示/有限原始证据槽、阻塞探测准入及自然窗口提前转换规则；产品1036输入、53包节点、89测试、编译来源断言、43项恢复、链接/UID/替换及进程归属保护不变。新源/数据/生成state在实施准备时重新实核封签，不把V04运行前state当当前值；不写旧state或旧证据。新包须独立E/cache/AS/TMP与activation，GitHub exact head审查后才交唯一C。

是否以及何时执行新I+T，必须在探针解决可观测性后决定；旧I不能复用为验收。原机械总量≤900秒、准备160/I360/T180、自然60/TERM30、恢复60/收尾30及重试0不扩大。当前仅诊断和此文档写入，未运行Unity、信号、测试、下载或Git。

## 探针收件后的最小决策（替代前文待探针条件）

已核 `TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.json`：48709B/SHA256 `3e48b59b1ba56b3decf6ad62f579452abf0e2835ab95e6affe7049de3eb0c3ff`，C actual `01a11945-ec1a-7281-8e01-a2f08ffc2c90`，ownPID63322，3次lsof exit0，清理完成。文件内elapsedThroughCleanup=0.075648417秒（外部收件约0.077秒）。监听2/2、连接保留监听4/6、关闭监听后2/4记录有精确全路径；无路径的恰为客户端FD5/7，其n为地址箭头。服务端已接受FD6/8在关闭监听后仍有全路径。

因此不修改bee_fd_bound的精确路径语法，不接受地址前缀。probe.observationStatus=NOT_OBSERVABLE不能解释为“服务端路径不可观测”：该标签包含客户端不能提供远端全路径的结果。探针不证明Bee的FD布局、生命周期或实际路径，更未证明V04格式错误。

定向核对V04实现：monitor先snapshot_consumers/discover，再检查resources；candidates包含Editor根，以及已认可、alive、exe等于固定bee_backend、同stage/rootPid的后代。真实62271的记录满足这些筛选，且后三次失败在其发现之后；若bee_process的start/exe/argv/cwd检查失败，错误也不会是最终holders为空。因此现有证据没有直接证明候选筛选遗漏，也不能声称它从未漏过短命进程。可直接证明的缺项是原始FD输出和失败时端点后像未保存；绑定失败发生在最终after-lstat之前。

下一步只允许“补观测+已确定的闭合边界修正”，不以本次收件授权新Unity。固定输入：上列probe、V04原receipt及PR20 head `ca0204b1aa67096d72f81e4e71c3596bfa1da9ff` 的原runner/data。runner身份为 92571B/SHA `2327cb60e5fff6406d9d0a56415eef005d9b11452755930f96d0c071314b2caf`；inputs身份为 1244365B/SHA `9f3379932108880c1898786de3bf978640c566bd647d0a36274632e702e8f524`。产品/包/测试仍引用原封签；生成state需实施时fresh重绑定。

未来实现白名单限定 `TestArtifacts/FightMatch/RES-COMBINED-V05/run/` 内runner.py、inputs.json、preparation.json、replay-check.py、replay-results.json及既有activation/执行证据槽；所有V04和probe文件只读。FD差量仅在既有process-events中记录每次本轮候选PID选择、完整目标路径、阶段/monotonic时刻、before-lstat、lsof argv/timeout/exit及原始stdout/stderr、解析结果、身份复核和finally的after-lstat/ENOENT。失败也必须落盘；不得由finally覆盖原异常。限定原Editor/Bee候选，原输出/机械预算继续适用，超限或落盘失败为INCOMPLETE，不扩扫描、不连接socket、不新增放行分支。

先离线用三份真实输出验证：服务端两路径均匹配，单独客户端箭头均不匹配；并保留错PID/路径/类型拒绝和闭合边界回放。无需重跑短探针。GitHub对新exact head审查后，中央若批准一次既有I/T合同的fresh运行，以上观测随I执行；任一实际绑定失败仍停T、保存原始输出并闭合恢复，不重试。真实Bee绑定失败的原因仍UNKNOWN；现在需要的最小实际观测就是同一失败瞬间的候选身份、lsof原串与前后端点类型/身份，旧进程结束后无法补取。此方案不假称IPC已解决。
