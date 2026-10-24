# RES-COMBINED-V04：限定I→T89（需新head源码门）

2026-10-08。C 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local唯一执行，gpt-6-astra/xhigh，登记fresh actual；中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local唯一收件。用户继续开发与限定验证授权不变。只读本页、相关JSON必要字段及短接续，输出每条≤2KB；不重读历史/大JSON。

启动门：中央resource-combined-v04-fix02-source-verdict.json必须已存在、结论ACCEPT、准确绑定PR20 headca0204b1aa67096d72f81e4e71c3596bfa1da9ff及实际GitHub结果URL；否则不激活。该门只覆盖源码，不是native通过。V03实际失败和恢复证据、V04原63及当前69回放完整保留。此包不重跑离线回放。旧V01/V02/V03额度不可重用。

## 封签和激活

E=TestArtifacts/FightMatch/RES-COMBINED-V04/run。五叶：

- runner.py：92571B / 2327cb60e5fff6406d9d0a56415eef005d9b11452755930f96d0c071314b2caf
- replay-check.py：36478B / 08519cdfda274179bc472e34f0986f9dce9c1c166df50eb53baef5f2ca9c8f41
- replay-results.json：257494B / 2e61957f14f67199d64b1343014f2824b59ea1879de01df1a1095931de18f944
- inputs.json：1244365B / 9f3379932108880c1898786de3bf978640c566bd647d0a36274632e702e8f524
- preparation.json：136069B / 07e4bc5b437d8276040b0bfa26430a5eb9ee94cf654dd8d83864e9f29367f211

P/K仍为RES-01A/P01的projection/package-cache。inputs.paths指定新BC4和AS4，启动前必须缺席。新TMP=/private/tmp/fm-rcv4.XXXXXXXX（8位字母数字），canonical无链接，本人0700空目录、≤40B。C只创建本次数据activation.json，按preparation.activationContract准确绑定本次actual、五叶seals、paths/editor、stages/stopping、当前ADB元数据和sourceReview。双方stage显式环境DOTNET_EnableDiagnostics="0"，UPM_CACHE_ROOT、TMPDIR、BEE_CACHE_DIRECTORY及T专用FIGHTMATCH_ACTIVATION_TEST_ROOT均按已审合同。不要修改runner/input/断言或再跑大缓存准备扫描；完整preflight在已审runner时限内做。

V04已审Bee准入只接收当前stage rootPID对应的htc/cth socket：首次必须核真实类型、目录/UID/inode、固定Editor/Bee进程与全路径FD关系；无权按文件名放行未知IPC。已绑定同inode消失/残留处理按runner，旧V03特殊项不追认。正常natural60秒边界转TERM；FIX01透传控制异常，FIX02在既有TERM窗口内持续纳入新鲜核实owned成员、每PID最多一次TERM，真正探测超时/I/O/身份错误仍失败。保留的单Tundra输入固定136080B/195457cf3a5cfffd34bffe89758757ec8b916abbbae9b0de85fdcd8e95a802af，缓存实物不清除、不刷新封签接受漂移。

## ADB历史身份核实

仅元数据核对、不获信号权限。候选：原27858/start Wed Oct 7 23:14:10 2026，log /private/tmp/fm-hc-m01.9qXZKifu/adb.501.log；V01 SDK54234/start Thu Oct 8 07:41:58 2026，log /private/tmp/fm-rcv1.uAvmwtPk/adb.501.log；新增V03 SDK58410/start Thu Oct 8 08:49:34 2026，log /private/tmp/fm-rcv3.cqMuMGw3/adb.501.log。新增身份来自V03/run/process-after.json sdkAdbBinding/sdk observations：UID501、SDK txt D0x100001c/inode8166469；log D0x1000010/inode34696470，仅1/2 FD，cwd固定SDK目录。历史记录不证明目前仍存活。

共同SDK路径为固定2022.3.18f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb，14527792B/953e88dc19d1a8737761302d7d13ebabd6d2935b2f239f65533464290c034169；argv [adb,-L,tcp:5037,fork-server,server,--reply-fd,4]。现在若都不存在，如实绑定仍存在且一致的一份历史日志/原PID；若仅一个候选存在，须精确start/argv/UID/SDK txt/cwd与仅旧log1/2、无当前P/K/TMP/编译关联全部成立，才填现有adbException。未知PID、多server或漂移停止；不能名称豁免、杀ADB或更换执行路线。旧日志只容许ADB自然追加，旧TMP/BC/AS不写。

## 一次执行与验收

按argv执行 python3 -B E/runner.py <activation_sha> <fresh actual>，cwd=P。最多I一次360秒；I和owned闭合/来源通过后T一次180秒，T不加-quit。自然闭合60、TERM确认30、恢复60、末证据30均受剩余900总机械秒（含准备160）约束。每已核owned PID最多TERM一次、无KILL、0自动重试。不得临时延长、扩大清理/恢复范围。

保留短外部观察：inline Python的monotonic start在subprocess.run之前，end在实际退出后，输出externalStartMonotonic、externalEndMonotonic、runnerSeconds、preparationSeconds160、totalSeconds、osExitCode，外层以实际code退出；不新增监护脚本。原工具输出保留，长命令通过yield/wait接回。内核I/O阻塞不能由软时限保证硬实时，按真实退出时间判定，不假称保证。

成功要求receipt/stdout暂态AWAITING_PROCESS_EXIT + validationStatus TEST_PASS + failure缺席 + receipt hash一致 + 实际OS exit0 + 外部elapsed+160≤900；12程序集完整本轮来源、89个准确case全Passed/0多余缺失跳过失败、AS实际Move/Replace/Flush/reopen和existing/dangling symlink、源和缓存保护、owned全部退出、P1035/43compiler恢复同时成立。缺任何项按实际FAILED/INCOMPLETE/NOT_RUN_BLOCKED保留；作者不自批。

只写既有有限evidenceSlots/transferPlan、activation及新TMP/BC/AS；禁止额外产品写、下载/安装、LOC、Play/Player、Android/设备、Git或付费。实际审批拒绝就停止该动作并报告，不绕过。结束给actual/activation hash、完整外部时间、I/T次数与exit、89结果、异常/恢复及证据路径。中央收结果后安排独立证据核收。
