# RES-COMBINED-V05 — 补真实FD观测和修正自然窗口探测准入

2026-10-08。用户继续任务/普通技术修正授权内。原源码作者01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，gpt-6-astra/xhigh；中央唯一收件。使用design文档最后“探针收件后的最小决策”，不重做设计调查、不做本地review。

固定输入：engineering-resource-combined-v05-correction.md SHAfe90ceef001eb80827890afed6ca8bd83a057e3be82259d973079fb3abe5e20c；V04源码PR20 ca0204b1aa67096d72f81e4e71c3596bfa1da9ff，GitHub6050592958仅源码门。V04 runner92571B/2327cb60e5fff6406d9d0a56415eef005d9b11452755930f96d0c071314b2caf、inputs1244365B/9f3379932108880c1898786de3bf978640c566bd647d0a36274632e702e8f524，其余三叶固定于V04现存preparation/sourceIdentities和中央执行包。真实native FAILED、T0、94.520780秒/含准备254.520780，已独立核证恢复1035/43/owned0，见resource-combined-v04-native-verdict.json。probe.json48709B/3e48b59b1ba56b3decf6ad62f579452abf0e2835ab95e6affe7049de3eb0c3ff已真实观察服务端n全路径、客户端n地址指针；不能据此宣称已知Bee根因。

唯一新增五叶：TestArtifacts/FightMatch/RES-COMBINED-V05/run/{runner.py,inputs.json,preparation.json,replay-check.py,replay-results.json}，目录必须fresh。旧V04及所有probe/source-before/失败记录只读；本包不创建activation、BC5、AS5或运行Unity。现有回放临时沙盒照既有范围，禁止真实socket/ps/信号/Compiler、下载/产品改写/Git。

实施两项，禁止第三项推测性修复：

1. 保持bee_fd_bound精确路径准入和Editor/Bee候选不变，只增加既有process-events内的有限原始观测。逐次记录候选选择与真实身份、stage/monotonic/目标完整路径、before-lstat、lsof精确argv/timeout/exit/原始stdout和stderr、解析结果及身份复核、finally after-lstat或ENOENT。绑定失败也保存；finally不得覆盖首个异常。原输出容量/机械时限不扩；超限或落盘失败仍INCOMPLETE。没有证据不加解析别名、basename/地址指针放行、宽松PID匹配或新进程扫描。
2. 自然60秒是最多等待：阻塞探测前必须仍有完整单次固定探测额度（现有ps/FD为5秒），不足则不启动该探测，记normal-natural-boundary并转既有TERM。每次阻塞调用前重核，原绝对deadline和总/工作/TERM优先级保留。真正已启动探测的TimeoutExpired/I/O仍失败，不吞异常；保留既有错误和FIX01/FIX02对新owned成员的闭合行为，无新信号或重试。

仅直接所需helper/cmd/bee观测接线/边界准入修改，准确报告AST差异，其余函数机械比对；不大重构。inputs允许新task/owner/E5-BC5-AS5-TMP5路径、前述观测/自然边界合同、源封签、设计引用、旧证据保护清单，以及重新只读核实V04之后Tundra/既有state准确身份。不能改实物state或执行时接受漂移。产品shared/projection、53包图、89测试、转移43compiler、信号权限、900总/160准备、I360/T180/自然60/TERM30/恢复60/收尾30均不改。带全activationContract/真实OS退出观测合同和self/sourceIdentities。

完成源码静态后，一轮受影响离线回放≤30秒；只有真实失败修正才可第二轮≤30秒。用已保存三份真实probe原串喂实际绑定逻辑：服务端精确路径可匹配、只含客户端箭头不匹配；错PID/路径/类型仍拒绝；成功/失败/异常路径均记录原串和后像且不覆盖原错误。补2.561ms余量不调用ps便转TERM、足额调用真实超时仍失败、总预算优先、边界不抹原Bee错误等。当前69及历史177只按有效类别复用，必要受影响部分单独本轮跑；不为了数字重跑全部历史，不把旧结果改为本轮通过。

SOURCE_READY仅表示观测和确定的边界修正完成，Bee根因继续UNKNOWN。中央核范围并推PR20新head/GitHub审查后才可能另签一次fresh I→T89；若精确绑定仍失败，T停并用新增原始证据定位，不追认旧I或原样重试。本次无native授权。目标8分钟内交付，超时报告具体原因、不追加研究。输出actual/5seal/差异/新回放耗时/复用边界，中央完成事件接回。
