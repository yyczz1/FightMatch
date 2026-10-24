# RES-COMBINED-V01：FIX02 首次限定执行

2026-10-08。中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local签发；唯一执行者C01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh，必须是本次新actual。用户已要求继续开发并授权场景、保存复开和限定验证；本包是该范围内已有组合验证方案的首次执行。中央唯一接收完成事件；无需C向其它会话发送身份消息。旧N07/N10/N11/I01及历史问题均不可恢复。

## 固定前置和范围

先读START_HERE、PROJECT_CONTEXT，再读本包及engineering-resource-combined-v01.md、central-resource-combined-v01-fix02.md。FIX02源码门已在resource-combined-v01-fix02-source-verdict.json限定ACCEPT；独立GitHub结果为https://github.com/yyczz1/FightMatch/pull/20#issuecomment-6048937458，对应head5f43b7a8cdff0795ad91b342deb5d566be38da0f。机器人未给正式APPROVED，历史3条线程未手工解决。没有本轮Unity通过证据。

只用TestArtifacts/FightMatch/RES-COMBINED-V01/run中已封签的runner及四份输入；五个bytes/SHA以源码接收JSON为准。runner81937B/1a30b4c6e1a5807664d047635c4fc20b131fe9d15191b128656dc366612aa7ba。共享1036、P前像1035、拟导入1037、53包、43暂移编译叶、精确89用例及全部源/缓存/输出路径按封签inputs，不重做设计，不重跑158项离线检查，不改源码/断言/脚本。

## 新激活与实际执行

允许C在本次actual内填写**数据文件**E/activation.json：沿preparation.activationContract逐项填本次真实身份、封签、sourceReview的ACCEPT/head/结果URL、paths/editor/stages/stopping、环境和fresh TMP身份。中央授权的是这里的具体一次运行，不是C自行批准源代码。五份输入先验封签；所有owner字段及CLI actual必须匹配当前真实回合，不采用旧绑定。新TMP仅/private/tmp/fm-rcv1.XXXXXXXX，canonical、本人0700、空、无链接；BC和AS仍须不存在。

ADB旧窄例外数据可参照HOST-INTEGRATE-001/I01-run/inputs.json，但须当前重新核实SDK文件字节/身份、旧log身份和PID现状；已不存在的原PID如实记为不存在，不发信号、不编造替代PID、不豁免其它ADB。未知消费者或身份漂移交回中央。填写和只读绑定机械时间最多20秒，纳入已有160秒准备额度的剩余量；不重新读取大缓存作重复准备。真正完整preflight由已审查runner在执行时限内做。

激活写完后记录其bytes/SHA，运行且只运行一次：`python3 -B E/runner.py <activation_sha256> <本次actual>`，cwd=P。用argv数组传入，不能让shell解释测试filter。runner按固定序列最多I一次、通过后T一次；失败不重试。I360秒/T180秒、自然闭合60/TERM30、恢复60、收件30，包含160准备总机械900秒。只对精确核实owned进程按已有合同每PID最多TERM一次，禁止KILL；未知外部进程不处理。保留所有失败和恢复冲突。写入限原inputs evidenceSlots/transferPlan及本包activation/TMP/BC/AS，禁止新增监护脚本或框架。

必须从外部记录runner完整退出。可在工具命令中使用最小内联Python：`t=time.monotonic(); r=subprocess.run([sys.executable,'-B',runner,activation_sha,actual],cwd=P); end=time.monotonic(); print(json.dumps({'externalStartMonotonic':t,'externalEndMonotonic':end,'runnerSeconds':end-t,'preparationSeconds':160,'totalSeconds':end-t+160,'osExitCode':r.returncode}),flush=True); sys.exit(r.returncode)`。它只观察退出和计时，不提供第二个重试/信号/超时逻辑，不写新文件。工具原始输出保留stdout和真实exit，不能只抄作者摘要。长命令使用yield并正常等待，中央会接回完成事件。

## 收件

按FIX02 completionObservation：receipt/stdout的AWAITING_PROCESS_EXIT仅为临时状态；必须同时具备validationStatus=TEST_PASS、无failure、准确receipt hash、真实OS exit0、外部monotonic完整elapsed+160≤900，才能报告整个运行成功。检查精确89全Passed/0额外缺失失败跳过、12程序集本轮来源、真实AS文件操作、过程闭合和恢复；缺任何项保留FAILED/INCOMPLETE/NOT_RUN_BLOCKED，不补造通过。

最终回执用聊天结果引用原E证据：本次actual、激活SHA、真实I/T次数及exit、89结果、完整外部时间、剩余进程和恢复状态；中央/主测试继续独立接收。禁止Git、下载、联网材料恢复、LOC、Player/Play、Android或设备运行；这些仍是独立门。C结束本次执行回合后等待有用的新任务，不空转。
