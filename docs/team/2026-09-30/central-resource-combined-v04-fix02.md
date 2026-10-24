# RES-COMBINED-V04-FIX02 — 边界发现的子进程闭合

2026-10-08，原作者01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，Astra/xhigh；中央唯一收件。用户继续任务授权不变。只落实已独立审查提出的P1，不再新设计/审查轮。

输入：PR20 a2949cfc655e95693c44b5bfd17f1252e239fe2d，当前V04五叶封签见central-resource-combined-v04-execute.md（该执行包现在尚未生效，FIX01未通过）。GitHub新P1 4213782936：自然边界中details尚未登记新子进程便抛NaturalGraceExpired，closure固定remaining遗漏该子进程；后续authorization snapshot登记也不能让固定TERM循环包含它。现有回放的共享live掩盖父退出但子仍活的场景。GitHub原文已由中央直接核实。

允许写：TestArtifacts/FightMatch/RES-COMBINED-V04/run现有五叶；先逐字节保存至TestArtifacts/FightMatch/RES-COMBINED-V04/source-before-fix02/同名五叶，必须fresh。source-before-fix01保留。原runner92317B/c14d3c110e6e696ebbc6aabbc66ccb96449d53b43d7288d5dbd695801a9cd5b2；其他封签同上述执行包。

修正目标：TERM候选在完成新鲜发现/身份核验后包含新登记owned子进程，在既有30秒TERM窗口内处理这些新发现成员；不能仅在进入窗口时冻结旧owned。必须保留已核链/身份/新鲜快照检查及每PID最多TERM一次、无KILL、未核身份不得发信号、不延长总900/160准备/各阶段上限；真实身份/探测错误仍失败。实现采用最小closure及直接所需接线变更，报告精确AST差异；FIX01透传保留。不得重构其他框架或扩大消费者豁免。

回放必须走实际discover/register/closure链，按每PID独立存活/退出状态，不能给父发信号就同时让子消失。覆盖natural边界晚发现子、父先退子独立存活/重挂、TERM期间新发现且已核成员、同PID不重复TERM、未知/身份失败不获信号。源码静态完成后一次受影响套件≤30秒，真实失败修复才允许第二轮≤30秒。既有65/V02 177保留，准确标明当前与历史。不要跑Unity/实际进程/真实信号或socket。

inputs产品/包/测试/命令/恢复语义和激活/真实退出合同保持；五叶来源、当前actual、封签更新。无activation/native/产品/依赖/下载/Git写。目标5分钟内完成；若确有设计歧义先报具体点，不扩范围。结束SOURCE_READY、实际回合、5seal、AST差异和回放结果，中央等待接回后推新head审查；作者不自批。
