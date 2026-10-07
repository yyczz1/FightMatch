# I01-FIX04-source：并行源码修正交接

2026-10-08。中央批准主程 `engineering-host-integrate-i01-recovery-next.md` 第3、4节源码范围；全文4990B/SHA `8bc112222aa52f3c14fb3473cb472e3855474fbe678973d4894fb83ca0d1ea2b`，actual `01a11802-d7ba-78c1-9ca5-947296efd44d` 已wait105 completed。用户授权持续任务分发和必要技术修正。

为使恢复与源码互不等待，源码实施权从原拟唯一C转给已有实施聊天 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`（已核旧actual `01a116f2-e4ba-7f92-95ca-97088acdb2c3` completed/闲置），Astra/xhigh，绑定新actual。C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c` 仍是唯一本机Unity/恢复执行者，本轮只操作Recovery01，不能写FIX04。主程只拥有设计，无新实现/运行任务；中央唯一接收两路实际完成。

唯一写入新根 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX04-source/` 五叶 `runner.py,replay-check.py,replay-results.json,correction.patch,preparation.json`。从FIX03-source逐字节固定基线（runner47602B/SHA `b11eb06bc058362aaf833e5070876dc12db0e49491612f7570b87d8e26071317`；checker33486B/SHA `82ca9b61fe0f9562a7c3f9b657ea3e585239904be74a2e5c7d94cca4394e1531`）。原I01/FIX01/02/03二十叶、I01-run59叶、原inputs和产品输入只读；不得读写正在变化的Recovery01树或把它纳入本包冻结清单。

只实施主程已定位的三点：同快照先确认进程再判FIFO并精确支持许可进程；短命子进程details失败保留待核而非授予身份；收尾逐周期保留异常但完成60/30有界流程。必要的待核状态/证据随真实流程记录，恢复门仍拒绝活跃待核消费者。编译判据/ADB/原子账本/资源与原生时间预算不改；无新通用框架。runner上限580非空行，允许必要分支展开，不为行限压缩复杂逻辑。

按主程第4节调用真实函数验证；旧58项覆盖保留，先复现已知红点，再在修正后确认绿，分清回放轮次，不改断言凑通过。语法+离线最多2轮累计30秒、首绿停止；模拟时钟跑60/30，不真实等待。仅既有离线夹具方式，无真实Unity/ps/P写入/信号/TMP/下载/存档/Git。准备机械累计原50秒继续计入120秒上限；推理/审查墙钟分列。固定源码后交精确五叶身份与patch，中央发布同一PR18新head并接GitHub审查。此包不授权任何新native或修改旧失败。

直接本会话final返回，中央wait接回；不主动跨会话发送、不新派工、不修改角色索引。未知/out-of-scope先交具体问题，已知普通实施细节自行处理。
