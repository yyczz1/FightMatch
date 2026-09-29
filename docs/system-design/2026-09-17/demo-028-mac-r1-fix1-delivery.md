# DEMO-028-MAC-R1-FIX1 C 补充验证交付

COMPLETED

作者 C 01a0e404-d89d-7ab2-bece-3cd1df3fbc52；新 turn 01a0e905-d341-77f0-b763-cca2ba6d8fc4；startedAt 1790615868；host local。
本包仅执行 §440 的编译和精确单项测试；产品、测试、原工具、原报告及原证据均冻结。

## 实际验证

| 槽 | 模式 | Unity PID | 退出码 | 结果 |
|---|---|---:|---:|---|
| 001 | compile | 19157 | 0 | PASS |
| 002 | tests | 19284 | 0 | PASS |

完整 argv、UTC、实际 PID、stdout/stderr、日志与前后身份见各 run/result；Unity 自然退出，未杀进程或重启。
精确测试：`FightMatch.Core.Tests.PlayerBattleRecoveryTests.B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway`。
该项实际 1/1 Passed、零失败/跳过；源码第123～125行的迁移拒绝、零写、原S17 Retry/收据、预留ID、一次奖励及Continuation清空断言保持原字节并顺序执行完毕。
正式结论是：原006全量4010/4011＋修正后该项1/1，产品未变。这是组合证据，没有新跑全量4011。

## 冻结与范围核对

§440：5440 bytes / SHA256 63ad15f2628770aa0bc97a59b2be8721da8284cb8d4f871ffd59453572d6927a。
计划：19630 bytes / SHA256 eb37ed78e7f12f871750ae148716deaf647d86516a2cf102f335c980e630d2c6。
原103份manifest行及manifest自身逐件身份保持；原§434、828源码、858Assets、451meta、933保护输入（15历史缺失）、原工具、六运行文件和四golden按计划核对，五份SD00元数据单列。
Compile前36DLL与006一致；Compile只许可测试dll/pdb更新，其他34保持原身份；Focused前后全部36与成功Compile后一致。
运行器：20790 bytes / SHA256 d12ade528bfb2b5207e13d5a0abd2a621a65f63152af21fea556ffc71fd02dbd。
证据：21份实际叶、20行manifest，均在54叶白名单内；未用叶保持缺失。
I/O：原3719cases/28188entries保持；新增0cases、0files+links；未清旧目录、未扩大4096/40000上限。
Git HEAD 14f0bd76776a211ed588561691616b3523f73395；未提交、推送、建分支/工作树。

## 接收边界

仅为本补充验证包的C交付；不作R的ACCEPT判定，不宣称Demo/Android或真实Player进程冷启动已验收。
SD00核对本准确turn正式完成及两份报告后，再派R独立审查整个028补丁、B01～B12和组合证据的充分性。

COMPLETED
