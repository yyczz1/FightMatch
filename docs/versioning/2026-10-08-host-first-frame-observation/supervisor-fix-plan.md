# HOST-NEXT-001/M04：监护快照竞态，源码与离线回放

2026-10-08。M03 actual `01a11781-abc7-7203-a1de-187852f84168` 已wait114 completed，43叶证据中央实核；native0、16个runner用例、8个C#反例、严格四项观察/清理/恢复均有实际证据，但runner因快照竞态整体FAILED，不改写该结果。

唯一C保持 `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，fresh actual；中央唯一收件。本次只修执行器并离线验证，不再运行Unity、真实ps/ADB、信号、源同步或场景观察。

## 原因与修正范围

M03 running循环对rows=ps()执行discover；monitor随后另取ps()分类。18:01:15.844920Z的新快照有bee36391/ppid36300，前一快照尚未登记；25ms后完整身份、cwd=P、标准编译参数已记录，18:01:15.886622Z进入owned。新进程是实际Editor36300的子进程，但两快照不一致导致误报；不是用名字放行的理由。

固定源：`TestArtifacts/FightMatch/HOST-NEXT-001/M03/runner.py` 41254B/SHA `e2c7c25f1b79902197b1c109c85dc033acd4a7d6e595b79f3c14f78920d93ed0`。在新根 `TestArtifacts/FightMatch/HOST-NEXT-001/M04-source/` 只准五叶：`runner.py,replay-check.py,replay-results.json,correction.patch,receipt.json`。

只改监护/调用点，使一次进程分类使用**同一份新鲜快照，先发现并验证owned，再分类消费者**。running、natural-closure、term-confirmation及closure-final调用点明确携带对应快照，不能在分类内部再取不匹配快照；TERM前分类如需要发现新的合法子进程，也先基于同一份快照发现/验证。保留PID/start/exe/argv/cwd/父链身份守卫，未识别或复用PID仍拒绝，不增加bee或任何名称例外，不给外来进程信号。不得修改ADB准入、预算、信号次数、资源/源码保护、M03两条观察判定及其它方法逻辑。

这是未执行的新监督器源码，保留原M03路径/身份常量，不制作新activation或伪装已经运行；未来执行仍需另签新身份。probe40558B/SHA `11a0dce7bfa6f11e0b2dda3a0397b3e17196cb26c106fce1d648bd8d6d55d920` 和全部产品/场景只读，M03真实Unity证据直接保留，不重新编译或重复观察。

## 有意义的离线核验

replay-check从实际修正源提取/调用受影响函数，使用M03保存的PID/start/parent/argv/cwd事实构造最小回放快照；派生/删减快照必须标为fixture，不能冒充完整原始ps。测试替身仅限ps/details/事件/资源检查等外部边界，任何真实Popen/kill/ps/网络/Unity调用都禁止。不得复制一套判定算法来验证自己。

至少证明：旧的“先快照无子进程、分类快照含新子进程”能再现拒绝；修正后同快照先登记再分类能接受该已核合法子进程；同名但外来父链仍拒绝；PID/start复用或身份不匹配仍拒绝。覆盖各实际monitor调用点的同快照约束，保留2秒调度/资源保护；静态AST差量证明范围外方法及观察判定不改。反例输出清晰列预期/实际。

脚本语法与回放核验最多2轮、总机械≤30秒，首过即止；五叶总≤5MiB。仅写新根，不修改P/K、旧证据/TMP/Git/任何进程。失败保留，不启动Unity追绿。receipt绑定真实实际turn、两源身份、实质patch、回放结果、准确未运行项。中央接回后将M03已实际验证的probe＋M04离线验证的监督器一起更新PR17；清楚区分M03原FAILED、实际观察通过、M04未进行native重跑，交GitHub独立审查新head。
