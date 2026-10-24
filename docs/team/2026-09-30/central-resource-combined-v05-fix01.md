# RES-COMBINED-V05-FIX01 — 两项独立审查异常优先级修正

2026-10-08。原作者01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，Astra/xhigh，中央唯一接收；沿用户继续任务授权。PR20 head6cb1f6f10aad344d14876cd26c86650fd99b8401的新GitHub问题已直接核原文，主测试actual01a1195b-1c80-7000-a1c3-5518b45518ea已接回。无需新设计或本地review。

P1 4214034014：NaturalGraceExpired发生后，观测写入失败仅add_note保持原异常类型，monitor误当正常边界丢掉写失败。P2 4214034022：lsof TimeoutExpired携带超容量部分stdout/stderr时，bee_capture_output的False未处理，容量失败未记INCOMPLETE。保持原失败上下文，但这两项不得被正常自然边界掩盖。

唯一可写V05/run现有五叶，封签见central-resource-combined-v05-execute.md；先把五叶逐字节存fresh TestArtifacts/FightMatch/RES-COMBINED-V05/source-before-fix01/同名五叶。旧V04/probe/历史不改。最小修正只在bee_fd_binding及直接必须的异常接线：自然控制异常+观测落盘失败必须抛不同的INCOMPLETE失败并保留grace为cause/context；超容量的超时部分输出必须明确INCOMPLETE并保留TimeoutExpired和容量事实。不可吞原错误、泛化放行、改解析/候选/信号/预算/产品输入。inputs无需改变，actual和当前源码封签在preparation更新。

补回放实际调用链：正常自然边界且记录成功仍转TERM；natural+写容量/I/O失败进入monitor_errors且本轮失败；已超时的stdout或stderr超容量时同时保留超时和容量失败；原普通失败上下文不被覆盖。静态完成后跑一次受影响V05套件≤30秒，真实失败修复才第二轮≤30秒；保留原37/两轮首夹具失败及69/177历史，不重复native/短probe。不得增加新异常框架、研究、下载、真实ps/socket/信号、产品/Git变更。

目标5分钟交付SOURCE_READY、实际turn、五叶封签、精确AST差异、新回放耗时和旧证据边界。中央核对后推新head并由GitHub复审；V05 native仍未授权，旧执行包门不满足。
