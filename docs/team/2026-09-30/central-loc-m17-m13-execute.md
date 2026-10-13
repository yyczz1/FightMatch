# LOC M13：FIX10 一次零下载依赖还原

本轮直接人类授权见 central-loc-m13-approval-2026-10-13.json，中央 turn 01a13323-9503-7942-8536-bb479256d06b / item 01a13323-95c4-77a2-910c-5a03d6e5bb02。既有提案 central-loc-fix10-next-material-proposal.md 现已获准；不用旧次数，不再询问相同权限。

唯一执行者 LOC 01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local，gpt-6-astra/xhigh；中央唯一接收。SOURCE head 7be2852330b7a0138be7b4fe3a8bed06924f0da6，testing-loc-m17-fix10-review.json 已SOURCE限定ACCEPT。51项检查、真实M12输入重放及封存均复用，不重跑，不改源码。sourceCommandReceipt 为 loc-m17-fix10-command-receipt.json，29677B / b2721c1a3c7c4d1806af41e78412fb0709d339358e89e815af51bb1cd9ba9a1b。

中央通过wait获取实际owner turn，只填既审central_host.py模板CONFIG，写 central-loc-m17-m13-activation.json、central-loc-m17-m13-host.py、central-loc-m17-m13-host-binding-receipt.json。LOC最多等待绑定3分钟，每次不超过30秒，无身份回执或跨会话转报。绑定就绪后，准确argv经正常工具审批执行一次；不改A/H、权限或源码。

允许新根：材料 /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix10；工作 /Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix10。执行前两者必须不存在。新证据 TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m13 仅before.json、stdout.log、stderr.log、receipt.json（总16MiB，各日志4MiB）。旧根和历史证据只读保留。

复用31个NuGet包及1个源码归档，0GET；最多三条顺序restore，各180秒；完整调用含预检、SDK核对、运行和收尾≤900秒（870+30），不在计时外扫描SDK。其他预算逐字取FIX10 prepare.executionBudget：driver≤280KiB、材料512MiB、工作4GiB、磁盘余量2GiB；父HOME/home/CODEX_HOME不变。

成功须三条restore均通过、完整校验及manifest成立、保护项未变、进程结束。失败即停止依赖步骤，保留partial与原输出，无第二次host启动，不删除旧文件。成功也不自动执行B/L、Unity、Git、部署或购买。完成后留原始工具ID、完整命令/输出/退出/时长及证据在本会话，中央wait/read直接接收并记录单一native结论；不唤醒其他角色重复接收。
