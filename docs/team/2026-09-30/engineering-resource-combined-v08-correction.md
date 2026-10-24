# RES-COMBINED-V08 身份采样纠正

仅技术纠正设计，不是代码审查；不运行测试/探针/native，不修改旧源或证据。V07仍FAILED，不能凭最终不存在追认通过。

## 实际证据与缺口

E7=`TestArtifacts/FightMatch/RES-COMBINED-V07/run`。固定runner107583B/SHA256 `41ba871264309f905ffc0ed0747ac9fde15779e6a5ebbffd6370054b0144304c`；inputs1251531B/SHA `5d7409d1e9f1ee5984d3e1f7cb2b948fcbfdca62e8a6790b806b34d104d6ff0c`；receipt75940B/SHA `2e72eec4fcc8c6695aead816cac7d692153cdb47af15e46a90351b364d0c37d2`。

185行05:34:23.007491Z的pending_identity：PID82401，ppid82343，start=Thu Oct 8 13:34:21 2026，stat=S，exe为固定Editor下 `Contents/Tools/ilpp/Unity.ILPP.Trigger/Unity.ILPP.Trigger`。186行05:34:23.007721Z为running/consumers失败，192行进入closure。193行05:34:23.932469Z的后续快照明确pending_resolved/absent；约0.925秒后已消失，但没有失败分支当次的freshConfirmation。

准确调用链为monitor→snapshot_consumers→discover→register→details；register第327行的合取检查失败：argv为空、cwd为None、cwdProbeExit非0至少一项。不是后面检查已owned记录的同名断言，因错误有discover的Supplied-snapshot前缀，且82401从未加入owned。child_args没有抛错，所以该次单PIDps返回0；但这不能证明argv非空。cwd lsof已返回，没有Timeout异常；其stdout/stderr/exit及details返回值未保存。故**现有证据无法定位三个字段中的具体失败项**，不能猜“必定cwd消失”。

V06/V07特例只识别ChildArgsProbeError的exit1双空；此处是成功返回后的身份字段不完整，被折叠成RuntimeError，因此未进入即时fresh absent分支。最终闭合和恢复完成由中央/QA独立收件，本设计不重复核验。

## 最小实现范围和边界

只改child_args/details的结构化观测、register的不完整原因、discover的有限退出确认及相关回放。不得重写进程框架，不得按Unity.ILPP.Trigger名称豁免。

1. 保留两次采样各自argv、开始/结束、timeout、exit、stdout/stderr及解析字段；details返回presence/缺失原因，register在拒绝前将其附到结构化IdentityProbeIncomplete。沿用既有事件上限和敏感参数处理，超限明确INCOMPLETE，不丢字段后猜测。普通I/O异常不能改成这种“缺字段”结果。
2. 仅对已核owned父链下新发现、尚未注册的PID，完成的args/cwd探测出现**缺失**而非矛盾身份时，可立即做一次fresh全进程确认。候选限：return0、stderr空、输出可按该固定命令完整解析但缺目标字段；或固定单PID命令明确exit1且两个输出都空。记录具体缺失字段和原串，不能只匹配错误文字。
3. 非法/截断格式、错误PID/字段矛盾、非空stderr、其它退出码、权限/I/O/Timeout错误均拒绝；不得借后续消失吞掉真实错误。return0缺字段也只是“需确认退出”，不是身份合格。
4. 只有fresh快照中该PID完全不存在，才记录transient-child-exited并结束此pending；保留初始row、父链、两次探测及fresh快照。不给owned身份、不授信号权限。fresh仍在（含僵尸）、PID重用/换start/exe、查询失败或无剩余预算均保持失败/阻塞，不轮询重试。fresh rows必须传回后续消费者守卫。
5. 已owned条目的完整性检查不适用此新分支；SDK已绑定进程仍遵守V07独立语义。所有其它register/父链/可执行文件绑定错误继续原失败路径。旧82401没有完整采样原串，不能把它改记为新条件已满足。

这修复的是身份采样结果的表示和有限退出确认，不保证旧V07未知的具体原因必落入允许分支。若新证据是权限/格式或活进程缺身份，应准确失败，不能为追求运行通过继续扩大范围。

## 回放、封签和派发

一次相关离线回放：args return0为空、cwd return0缺字段、cwd exit1双空分别结合fresh absent；仍活/僵尸/复用均拒绝；非空stderr/异常退出/解析矛盾/Timeout及fresh失败拒绝；完整身份照常注册；缺身份不给信号；已owned检查和SDK规则不扩。保留原82401事件作为“旧字段未知、后续absent而原失败不变”的fixture。复用177及后续有效证据，只跑受影响链和新增例，不重跑无关套件。

实施白名单为新 `TestArtifacts/FightMatch/RES-COMBINED-V08/run/` 下runner.py、inputs.json、preparation.json、replay-check.py、replay-results.json和原有限activation/执行证据槽。新cache=`TestArtifacts/FightMatch/RES-COMBINED-V08/bee-cache`；AS=`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V08/state-tests`；TMP=`/private/tmp/fm-rcv8.XXXXXXXX`。E/cache/AS/TMP均fresh、真实根，原链接约束不变。

准备时重新实核P的1035前像及43编译叶；TundraBuildState.state、bee_backend.info等preserveInPlaceIdentities按当前实值封签并记录与V07旧值的关系，不套用旧hash，不写回旧state、不清缓存。产品输入、12程序集来源、89实际测试、原子恢复及≤900秒/原分项/重试0均不降低。新源/data封签、相关回放、GitHub exact head独立源门完成后，中央另派唯一C；不能原样重跑V07。
