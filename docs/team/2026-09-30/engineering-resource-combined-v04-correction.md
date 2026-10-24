# RES-COMBINED-V04 最小纠正方案

中央唯一收件，交原作者继续实施；仅修改 Bee 通道准入、闭合边界和新包封签，不重建监护框架。本文不授权 native。保留 V01—V03 原源、输入及 FAILED。

## 已观测证据

E3=`TestArtifacts/FightMatch/RES-COMBINED-V03/run`；actual=`01a118fa-4774-77f0-95b3-2760ea765e6c`；receipt 75933B/SHA256 `5e50c83945e07b83d687ae2116ef0b889d2d61abf947f43da8ef7963254f57de`。I exit0、runner exit1、T0；compile.stages为空、fullImportedInputs=null。不能补接受或复用为完整I证明，晚到DLL不能补历史链。

`/private/tmp/fm-rcv3.cqMuMGw3/sk63t1em.gmn/ipc_58366_cth` 中58366是Editor根PID；process-after.json/process-events.jsonl及I/editor.log的 `bee_backend --ipc` 对应Bee控制链。fresh精确lstat返回ENOENT；旧异常无mode，历史类型仍未证明。

本机固定Editor根C=`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents`。C/Managed/Bee.BeeDriver2.dll，118784B/SHA `91b7f80fa3f7cd3d396858e831653d4f22c5f5a5579dd1f821d3aafbee9a5e2e`，含ipc格式、HTC/CTH标识、pipename getter、NamedPipeServerStream和--ipc；本机bee_backend含_cth/--ipc，Mono System.Core含NamedPipeServerStream/UnixDomainSocketEndPoint/Socket。这支持BeeDriver↔bee_backend两端NamedPipe采用Unix socket的候选解释，不能代替fresh类型/归属。官方[UnityCsReference 2022.3](https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/Scripting/ScriptCompilation/EditorCompilation.cs)仅作BeeDriver.BuildAsync调用链旁证，不是精确二进制源码。没有已证明的关闭开关；不改crashhandler、不移除--ipc，保留DOTNET_EnableDiagnostics=0及CacheWrite修正。

## 最小实现

- 现有特殊条目检查只增Bee分支：每阶段最多一个fresh TMP直接子目录（已观测8.3格式），固定目录身份；仅 `ipc_<本阶段Editor根PID>_htc` 和 `_cth` 两端，可先后出现。原真实目录、无链接、UID/权限保护不变。
- 首次准入须fresh lstat为socket并记录UID/GID/mode/dev/ino/nlink；同阶段PID/start/exe/argv/cwd及固定二进制身份有效。限定该Editor根及已认可bee_backend后代取得完整路径FD绑定证据，例如 `lsof -a -p <PID> -U -Fpcftn`，准备时固定工具身份/超时，前后lstat同对象。不扫全机，不连接/读取socket。不能绑定则INCOMPLETE/失败，不能按名字猜归属。
- 将少量事实并入既有events/after，不另造账本或信号权限；I不授权T。未知socket/FIFO、后缀、链接、UID、目录/端点替换及PID复用继续失败。已准入同inode可精确ENOENT消失或退出后原样残留；首次绑定前消失仍证据不足，其它I/O错误不能当消失。端点不替代进程闭合证明。维持TMP16MiB/512项，不清旧TMP。
- V03 closure_begin=00:49:40.484335Z，三条deadline错误在00:50:40.484816/485028/485191Z，随后TERM，支持采样跨过60秒边界。总预算先判：自然窗口在下一探测前耗尽，用专用控制状态停止余下探测并转既有TERM；保留先前错误及绝对时钟。
- 不笼统吞TimeoutError。真实in-flight `subprocess.TimeoutExpired`、I/O错误、工作/总预算耗尽仍失败；TERM结束未证明闭合仍失败。仅消除正常自然窗口结束产生的伪错误。V03 remainingOwned=[]、restore.complete=true、唯一TERM58461不改变FAILED。

## 封签、回放、预算

P=`TestArtifacts/FightMatch/RES-01A/P01/projection`。fresh `Library/Bee/TundraBuildState.state` 136080B/SHA `195457cf3a5cfffd34bffe89758757ec8b916abbbae9b0de85fdcd8e95a802af`，已不同于V03封签；`Library/Bee/bee_backend.info`127B/SHA `cd510c4af398ab352e5bd1fdb74a06658814c4e8da952cb8c068909ac45e87d1`未变。新inputs的preserveInPlaceIdentities绑定准备时实值并记录新旧来源；执行前精确比较，不自动接受漂移，不写旧state、不扩大43项恢复。

复用原产品/包/89测试清单：共享1036/P前像1035/目标1037、17覆盖+4新增+2暂移、43编译前像、53节点及测试断言不变。保留177/177历史回放（V03 preparation引用V02），不能称为新head177通过。仅重跑受影响链及新增例：合法两端顺序；阶段/PID/start/二进制/FD错配；未知FIFO/链接/UID/inode替换；已准入消失与首次未绑定消失；自然边界转TERM；真实TimeoutExpired/I/O/总预算失败；TERM未闭合且原错误保留；state漂移拒绝和43项恢复。沿用原replay入口，最多两轮、每轮30秒，区分复用与新执行。

实施白名单为新 `TestArtifacts/FightMatch/RES-COMBINED-V04/run/` 下runner.py、replay-check.py、replay-results.json、inputs.json、preparation.json及原合同有限activation/执行证据槽。E4为该run；新cache=`TestArtifacts/FightMatch/RES-COMBINED-V04/bee-cache`；AS=`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V04/state-tests`；TMP=`/private/tmp/fm-rcv4.XXXXXXXX`。全部fresh，原AS独立根和真实符号链接要求不变。source/data/activation新封签并经GitHub exact head审查，由唯一C接收。

必须新I一次+T89一次：旧I缺完整来源链，T未运行。命令/过滤沿用原合同，仅替换新根及activation/actual，不弱化编译断言。准备160秒、I360秒、T180秒、每阶段自然60+TERM30秒、恢复60秒、收尾30秒，机械总量≤900秒；各上限不构成相加额外授权，剩余预算不足不启动，重试0。云HEAD125b849缺所需两包、许可UNVERIFIED，当前不替代本机；固定条件后才另安排云纯EditMode。

本设计轮未运行Unity/测试/ps/Git，未改源码或旧证据。未决：fresh端点类型/FD归属、边界修正回放、新I/T结果；由原作者落实，不再扩大研究。
