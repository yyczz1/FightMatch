# FightMatch 当前接续快照

2026-10-01 · 唯一维护者：中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。只在状态改变时更新；任务只读相关包，历史按需定位。

## 当前约束

- 继续同一产品的首Demo，保留玩法和存档；不扩到完整安卓首版。
- 运行时uGUI、编辑器UI Toolkit；英／简中及错误占位符；Luban表格与YooAsset路线不变。
- Unity2022.3.18f1，工具／缓存优先外置盘；本机Unity仍只由C串行执行。
- 代码审查只走GitHub PR；自动化优先dots，实际云环境不等于已就绪或免费。
- 主要角色默认`gpt-6-astra/xhigh`；明确重复小任务可`gpt-6-luna/low`。无工作就结束回合，每任务只设一个完成接收者。

## 固定候选与任务

| 任务／owner | 事实与下一动作 |
| --- | --- |
| uGUI PR #1／GitHub reviewer；中央收件 | [PR](https://github.com/yyczz1/FightMatch/pull/1)，已审基线`656a94e4feda942b575b9429921fb4d0b3fdce52`；最新head以PR为准，draft，基线GitHub审查已Completed，未发现重大问题；[原结论](https://github.com/yyczz1/FightMatch/pull/1#issuecomment-5926702254)。109文件Git tree与本机快照一致；未合并，测试门未关闭。 |
| FIX17／C已结束 | exact2=2/2、Host30=30/30；Core190超时无XML，未带Release优化；资源37和产品29稳定，222并集及复开未完成。[原始证据选集](../../versioning/2026-10-01-ugui-pr/README.md)。 |
| FIX18／C已交SOURCE_READY；中央收件 | [准备包](engineering-ugui-01-q4-progress-delta-fix-18.md)：回合`01a0f657-799c-72c1-9b0f-1b3804054c6b`已交两条test-only路径及静态证据，原90路径相等；编译与运行未验证。后续才激活一次exact190、Release、逐项跨域进度、600s。不机械拆组重跑。 |
| 云图形preflight受阻／durable已结束 | 单次Xorg :97 exit1：非root无法创建/tmp/.X11-unix及socket；进程已回收，85.7s，未运行Unity。分辨率／GL和当前许可未验证；后续仅修此环境缺口或签本机例外，不重装工具。 |
| LOC M12／LOC待激活 | [准备包](engineering-loc-license-alt-m-materials-continuation-12.md)：补两包、复用旧runner与已下载字节；Linux三包验签后才开放tests restore。工具链尚未接收。 |
| 后续／按需唤醒 | LAYOUT设计与主美限定接收已完成；实际布局、语言偏好、YooAsset、API24／GLES3、APK与L1设备验收仍缺。 |

## 调度和接收

- 中央直接收当前任务的唯一回执；主程／主测试已完成准备回合，按需唤醒，不重复空等。
- 主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b`；主策 `01a0f2e3-2c47-7273-88da-20513bcbfaf4`；主美 `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47`；主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb`，均local。
- C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`；LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`；云 `01a0f13c-3d87-753a-ad11-16302add9084/durable`。
- 本机HEAD仍为`125b849`且保留WIP；审查分支通过GitHub连接器上传，未改本机分支／暂存区。后续以源码manifest与实际PR/head绑定，不能混同两者。
- Goal工具仍paused；用户已授权的当前开发继续。首Demo尚未验收，无新APK；iQOO真机门尚待连接。

[协作规则](../../../.agent/TEAM_WORKFLOW.md) · [实际模型回合](model-switch-receipt.json) · [完整历史登记](run-history-through-2026-10-01.md) · [主程索引](engineering.md) · [测试证据索引](testing.md)。不要为恢复一个小任务整篇读取这些历史。
