# HOST-CONTINUATION-FIX01-M02

2026-10-07。中央准备的修正版执行单，**仅在M01RecoveryR1实际恢复通过并由中央发出激活消息后运行**。唯一C、中央唯一完成收件，Astra/xhigh；其他角色不重复准备/等待。本包接[原Mac执行合同](engineering-host-continuation-mac-001.md)，只替换以下已知差量；源码修改和原生失败均不重写。

## 固定修正与前置

- FIX01/S/source-receipt.json 13334B/SHA `399ab40223701fb7cf05a778c0b7f7ddb711832bf8d074dfb2ad83e3f5866157`，作者actual `01a116f2-e4ba-7f92-95ca-97088acdb2c3`已completed。七叶61748B；仅测试2加1删，15/15静态通过，未编译/执行。
- 唯一替换原S输入的源码为 `FIX01/S/source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs`，25128B/SHA `c34c86a9686e85c145143a0294adbe0964b226a492dda76997732e0621d9ab90`，blob `ee8a1dbf23a218d2ee8784c81a98dc4a9d451633`。其余原12来源叶固定不变。
- 新meta不再生成：复用R1/candidate中M01自然产生的同名测试meta，59B/SHA `182331a51f0fdd96f9774ae7e30dfb6e3a525e47d01c1ce34054fe1b575ec47c`，blob `45a16a3dfaec7b2f4945d0f8cb59cb5024595601`，GUID `0c2429a8af0d0415f8da19df191112fc`。保留原来源；不手造或重置GUID。
- 输入严格取M01/inputs.json（419668B/SHA `fe6ecbdafec0f86187e865f8dc7e79df56907809f4d58ea882ddf4a78fb7f0c9`）的files/blobs/sources/baseline，替换上述测试、加入该meta；同步13来源（8覆盖/4新增/Session不写），导入前1012叶32086149B。原排序算法的canonical SHA为 `ff8fbe99041b93af2f0caba8259513d9403b913c14fb2580d677794f160e287b`；逐叶复核，不把整个共享WIP同步进去。
- 前置必须：R1已封存原32叶并恢复P1008；新C激活绑定实际R1 receipt身份，原P/K固定基底一致，无其他Unity/编译消费者。原M01 I exit1/T0、Recovery失败及R1结果全部保留。

## 写域、执行与收尾差量

新E=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/M02/`（开始核缺席）。精确文件仍为原合同E十三根文件、I三叶、T五叶及restore/source八叶、candidate32叶、test-io最多两GUID、条件SceneTemplateSettings归档；不增加其他类别。P/K复用原合同路径；P只写八覆盖/四新增/原允许生成物，K冻结。旧S/FIX01S/M01/M01Recovery/M01RecoveryR1、共享和Git不写。

复用M01 runner31198B/SHA `fe1ff5633f3d1da9d1f6f20e528e66428806bc872cf265d19b9940c9a8054d1f`，仅调整本包输入/路径/meta已存在、以下消费者及FIFO观察、实际编译证明；≤390非空行，不重造框架。新的TMP仅 `/private/tmp/fm-hc-m02.XXXXXXXX`，实际短路径≤40B，UID/0700/无链接；原TMP及其ADB日志保持。

继承R1的精确ADB例外：每次相关进程门都重核已记录PID27858/start/argv、FD1/2仅原TMP/adb.501.log、无P/K/新TMP/VBCS pipe或其他源码关联，才排除这两个**旧日志**FD。该日志可继续由外部ADB追加，记录观察，不把日志长度变化视为源码漂移；不关闭ADB。未知PID/新关联仍停。owned子进程监督、身份确认后每PID至多一次TERM、60秒自然/30秒确认及无SIGKILL保持，不重复已消耗信号。

TMP最终快照对本轮owned CLR IPC的FIFO只记录lstat的种类/owner/mode等元数据，不读取FIFO/伪造字节hash或删除TMP。其它不在已观测CLR IPC类别的特殊条目停止并记录。此项修复观察器不能回写原M01失败。

准备机械≤120秒、静态≤30秒；I一次≤360秒→成功才T一次≤180秒，总执行器wall≤900秒，零重试/额外discovery；8MiB单日志、32MiB E、4GiB P生成物、1GiB K、16MiB/512叶TMP、外置余量2GiB、HostRig两目录上限均继承。I/T argv模板和十项selector原样，不跑全回归。默认Host、真实存档、Play、场景保存/复开/导出、设备、下载restore、Git均0。

本次只改测试：Host.Tests须本轮编译成功及新DLL证明。Host/Presentation若I实际复用，可引用M01编译日志Csc与after所记录DLL身份、固定源一致性，准确标为复用；若重编则记录新事件/身份。不删除缓存强制三程序集重编。测试XML十名各1次、10Passed/0失败跳过不确定，8+2分别统计；执行时输入同I。

成功或失败都按原约定封存candidate与前像、owned闭合后恢复P1008；仅经实际安全条件完成才报告恢复。publication-inputs对PR1固定base `3541930877837e14b58020b910976b8b67b56e42`展开相同32路径，其中测试后像改FIX01；与实际验证输入逐叶一致。中央可并行发布这组固定源码的草稿审查，测试尚未完成时如实注明，不能据代码门关闭本轮测试或Demo门。
