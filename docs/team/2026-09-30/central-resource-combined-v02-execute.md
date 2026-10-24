# RES-COMBINED-V02：首次限定I→T89

2026-10-08。唯一执行者C01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh，fresh actual；中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local唯一收件。用户已授权继续开发和限定验证。只执行本包，不恢复N07/V01或历史问题。只读本页、相关JSON必要字段和既有短接续；不打印大inputs/preparation，不重读整个历史，每条输出≤2KB。

**启动门**：resource-combined-v02-source-verdict.json必须已有中央ACCEPT，绑定PR20 head281bb731ed08a21a25df67932068279853b3fd09及实际GitHub结果URL；没有该文件/匹配结论不激活。这里的ACCEPT只覆盖源码，不能当成native通过。V01原FAILED完整保留，缺完整编译绑定且T从未运行，本次有明确补证必要性。

## 封签与范围

E=TestArtifacts/FightMatch/RES-COMBINED-V02/run。五叶：runner.py83629B/3d6215f29eacf35bad556a5ada01548cf4655132001c80292c87963b881171b1；replay-check.py111261B/1f16c745c82b0373483ed8f029a6831345c840f12e01cbfd05b0791a1fca8f09；replay-results.json451171B/ef6b9626be51273a64770d169beedc2a1eb39f5cd5acb1ac10e9b0986d00b556；inputs.json1241674B/5042438e26cd9e7fea1c5decee0f689f30af05ee667c25dbec57c472d363e636；preparation.json350556B/40c7f86f884240b6db62bc2b303711a4e97d110ee966aa3bba82842e9db62460。

P/K仍是RES-01A/P01；新BC2和AS2按inputs.paths，启动前必须缺席。新TMP=/private/tmp/fm-rcv2.XXXXXXXX，canonical、无链接、本人0700且空。允许C只填本次数据activation.json，按preparation.activationContract逐项绑定真实actual、seals、paths/editor、stages/stopping、当前ADB元数据与sourceReview。I/T共同环境精确加入DOTNET_EnableDiagnostics字符串0，不能只依赖父shell。真实子进程映射由已审runner执行。准备核验不重跑177离线或整套大缓存；当前完整preflight在runner时限内完成。

ADB须新鲜核身份，不获信号权。历史可比对候选只有原27858（旧fm-hc-m01日志）或V01已核SDK54234/start Thu Oct 8 07:41:58 2026（见V01/run/process-after.json sdkAdbBinding/observations，对应/private/tmp/fm-rcv1.uAvmwtPk/adb.501.log）。当前若都不存在，如实绑定仍存在且身份一致的一份历史日志/原PID；若一个历史候选仍存在，须精确argv/start/UID、固定SDK txt身份与仅旧log1/2 FD、无当前P/K/TMP/编译关联全部成立才绑定现有adbException结构。不可用名称豁免新PID、同时放行多个server或编造身份。未知消费者/漂移即停，交中央处理。旧日志只允许ADB自然追加，不由C写；旧TMP/BC/AS和所有证据保留。

## 一次执行与验收

用已审runner的现有数据合同，不修改代码/输入/断言：`python3 -B E/runner.py <activation_sha> <fresh actual>`，cwd=P，以argv数组执行。最多I一次360秒，I和owned闭合通过后T一次180秒；自然闭合60/TERM确认30、恢复60、末证据30，总机械900含准备160，各阶段受剩余总额度约束。只对已核owned按原合同每PID最多TERM一次，无KILL、0自动重试。未知IPC仍失败；不改用其它诊断开关或新增豁免。

保留原短外部观察：monotonic start在subprocess.run前，end在退出后；stdout打印runnerSeconds、preparationSeconds160、totalSeconds及真实osExitCode，外层以该exit退出。工具原始输出保留，不只抄摘要，不新建监护脚本。长命令正常yield/wait接回；无跨会话身份消息。

成功必须是receipt/stdout临时AWAITING_PROCESS_EXIT＋validationStatus TEST_PASS＋无failure＋准确receipt hash＋真实OS exit0＋外部完整elapsed+160≤900，并有12程序集完整本轮来源、精确89全Passed/0额外缺失失败跳过、真实AS I/O和两种symlink、输入/缓存保护、owned全部退出及P/43compiler恢复。缺项不报告成功；按实际FAILED/INCOMPLETE/NOT_RUN_BLOCKED保留。允许写原有限evidenceSlots/transferPlan、activation和新TMP/BC/AS；禁止产品额外写入、下载、LOC、Player/Play、Android/设备、Git或付费服务。

回报当前actual、激活hash、完整外部时间、真实I/T次数/exit、89结果、异常与恢复状态及原证据路径。中央接回再交主测试，不由作者自批。若工具实际拒绝，停止受影响动作并如实报原因，不更换执行路线。
