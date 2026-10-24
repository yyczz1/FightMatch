# FIX10 后续材料执行提案（尚未授权）

M12实际114.755321秒，首restore成功，之后目录键末尾斜杠校验误判，后两条未启动。原始结果与中央NEEDS_FIX见 `loc-m17-m12-native-verdict.json`；一次native许可已使用，不可复用。

修正候选为FIX10，源码head `7be2852330b7a0138be7b4fe3a8bed06924f0da6`，PR2 trigger6136772763；准确head GitHub审查已完成，结果6136822761未报重大问题；`testing-loc-m17-fix10-review.json` 已作SOURCE限定ACCEPT，非正式APPROVED。生产门只接受唯一指定目录的无斜杠或一个末尾斜杠形式，其他路径仍拒绝。51项定向检查及真实M12首阶段完整生产读取/校验重放已通过（23项目、15包、91文件）；driver285972B≤286720B，SOURCE封存13.710168334秒。主程已接收原工具与八叶，静态/封存不再重复运行。

提案：在准确head审查通过、用户另行允许后，由LOC执行一次新的M13，中央直接接收。复用31个包和1个源码归档，0新增下载；最多三条顺序restore，每条180秒，完整调用含预检/SDK核对/收尾≤900秒（870+30）。父HOME/home/CODEX_HOME及全部历史文件保留；driver≤280KiB、材料512MiB、工作4GiB、磁盘余量2GiB，其他预算直接采用FIX10 prepare。

新材料根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix10`；新工作根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix10`。旧M9/W9不覆盖。新证据根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m13` 仅before.json、stdout.log、stderr.log、receipt.json，沿原限额。中央取得新的真实owner后，仅填已审host模板CONFIG，生成M13 activation/host/binding三文件；不改已审SOURCE。

成功标准：三条实际restore均成功、校验及manifest成立、保护项未变、进程收束，中央实核原始工具并记录单一native结论。任一步失败停止依赖步骤，保留partial与原证据，不自动重复。成功也不自动运行B/L、Unity、部署、合并或发布；这些仍是独立后续工作。

本文件只是具体提案，不是授权。当前不创建M13外置根，不启动host/restore。
