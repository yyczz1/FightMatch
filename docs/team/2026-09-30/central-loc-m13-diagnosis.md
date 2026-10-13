# M13 失败：主程组内诊断与最小修正

用户当前要求继续FIX10一次限定执行并查明反复停滞；中央turn 01a13323-9503-7942-8536-bb479256d06b / 人类item 01a13323-95c4-77a2-910c-5a03d6e5bb02。原一次native已实际启动完毕，不授权第二次还原或新增下载。普通源码诊断／修正继续，由主程组内收件，中央只收一次影响整体进度的结论。

固定输入：FIX10源head7be2852330b7a0138be7b4fe3a8bed06924f0da6和完整八叶；M13外层证据 TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m13/；对应工作根 /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix10。当前LOC实际turn01a13327-e085-7760-846e-d6b6014f86b4正在补录原工具，只读诊断可先开始，任何后续派发等此turn结束。旧源、工作目录及所有历史证据保留，不覆盖。

实际结果：完整调用216.202724秒；第一restore0/6.029369秒；第二restore143/3.554103秒，failure为FileNotFoundError，路径 phases/M-tests-generation/env/packages/microsoft.testplatform.objectmodel/17.11.1/qci5yr0f.zv2；第三未启动。无下载、378保护项未变、knownProcessQuiescent=true。中央接原工具后另记唯一native verdict，主程不重复接收整轮。

先用失败receipt、两条子命令stdout/stderr、实际生产监护函数定位抛错层，区分：①目录计量扫描与NuGet临时文件消失发生竞争（若成立，确定性模拟同一生产函数中枚举后删除应重现）；②沙箱写入范围遗漏（若成立，NuGet日志或OS拒绝应先出现）；③NuGet真实依赖／包内容错误（若成立，原NuGet输出应有独立失败）。先建立能复现原症状的小回放，再改代码；不得仅把异常吞掉，不得放宽保护固定输入、链接/越界拒绝、容量限制或最终静止态检查。

新诊断/修正计划唯一文档 engineering-loc-m13-diagnosis.md，约定事实、选择、精确源码白名单和验收。最多一次有界只读诊断，10分钟内出结论；只读本次与直接相关输入，不遍历全部旧历史/SDK，不运行dotnet、Unity、网络或真实restore。确定性回放只在新 TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m13-diagnosis-01/，最多2MiB/30秒，无native子进程；不重复旧静态测试。若原因成立，主程按既有职责签最小FIX11源码包并交原LOC执行，保留driver≤280KiB及其它原限额。若无法在限额内安全修正，先给具体减法方案，不累加新框架或单为旧历史再制造一层绑定。

验收：明确真实抛错路径；同一生产函数的确定性失败与修复后通过；覆盖活跃临时文件消失及受保护/非法路径仍拒绝；固定源码后必要静态检查，代码变更走GitHub PR Code Review，不派R。主程独立收组内源码结果后向中央只回一次修正与下一具体执行方案。没有第二次native许可，不启动B/L、Unity、删除旧根、改系统权限或付费。
