# LOC M07：零下载的一次材料执行

2026-10-12。当前用户明确批准：“允许再执行一次：复用全部已下载文件，零新增下载，在外置硬盘执行三次依赖还原，最长15分钟，由中央接收结果。继续任务。”中央actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类item `01a12dc8-5fb9-7e43-8659-7026352c2050`；准确授权记录见 `central-loc-m07-approval-2026-10-12.json`。这是一次新M07，不复用M06的次数或计时。

唯一执行者原LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一收件。先读本包，保持同一fresh回合，等中央取得真实turn后生成 `central-loc-m17-m07-host-binding-receipt.json`，最多3分钟、每次等≤30秒；不用发送跨聊身份回执。中央负责A/H数据绑定，执行者不创建或修改A/H、driver及封签。

输入为REVIEW02八叶，PR2 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`，GitHub6112531539完成且无major，中央 `loc-m17-fix05-review02-source-verdict.json` 为源码限定ACCEPT。源根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02`；driver205640B/SHA ae1d2c3cb2a8cc1ea95d92fd5c7f3c5fce0eb90fcfa2235e9650023b9bd4ec5f；freeze2870B/SHA d32e5af44ba61473b333f919910167c6cad2b6691d19cc12e1395fe45e7147de。后置回执 `loc-m17-fix05-review02-command-receipt.json` 20990B/SHA 55cb8c419ab9fe4da8f3a4b0a4fb69e38d925bed45a27bab8d510d177cf25cc5 必须由新activation的sourceCommandReceipt准确绑定。此门在材料状态前及每次native命令前fresh核验。

绑定就绪后，走正常工具审批执行中央host准确argv一次。归档复用31个NuGet包和1个Yaml源码tar，零GET/零新增下载正文；缺失、链接、hash或保护项不符即失败，不能重下载。原定三次restore（runner生成、tests生成、tests验证）和同一900秒计时（870工作+30收尾）沿已审prepare.executionBudget。所有源/SDK鲜核计入该timer，不提前做SDK扫描、prepare、SOURCE检查或重复历史测试。

新M5：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix05`。新W5：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05`。只按已审模型写入；既存即停，不清理或改名。原M4/W4等全部只读。

新增外层证据E07：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m07`，仅before.json、stdout.log、stderr.log、receipt.json，合计≤16MiB、各日志≤4MiB。记录原工具ID、真实开始/结束/退出、完整墙钟、保护输入、三restore收据及manifest/进程静止；工具拒绝/中断不是DUT退出。保留HOME/home/CODEX_HOME。失败保留partial和原始输出，返回具体原因，不自动重跑。

执行须实际完成或返回真实阻塞，不能只生成before后结束。等待绑定及工具运行时使用≤55秒有界等待并留简短进展；上下文压缩或状态询问不取消本次任务。结束由中央read_thread/wait接回，再安排QA独立证据接收；作者不自签ACCEPT。此次仅M，不启动B/L、工具build/官方测试、Unity、APK、设备、Git写入、部署、发行或购买。

