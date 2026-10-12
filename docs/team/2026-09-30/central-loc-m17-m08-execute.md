# LOC M08：FIX06 修正后一次零下载依赖还原

2026-10-12。当前用户直接明确批准：“允许按修正后的版本再执行一次：复用全部已下载文件，零新增下载，在外置硬盘执行三次依赖还原，最长15分钟，由中央接收结果。继续任务。”中央 actual `01a12e46-6af4-7b82-958d-d18c1146d85d`，人类 item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`；见 `central-loc-m08-approval-2026-10-12.json`。这是新的 M08 一次许可，M07 失败及其一次额度保持历史事实。

唯一执行者 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh；中央唯一接收。先读本文，保持当前 fresh turn，等中央取得实际 turn 并生成 `central-loc-m17-m08-host-binding-receipt.json`，最多3分钟、每次等待≤30秒。无需跨聊身份回执。中央仅填既审模板 CONFIG 与新 activation；LOC 不改 A/H、driver、八叶、侧录或既有证据。

输入 FIX06：PR2 head `454eb4da250262a396b17da97d83c71de408bd48`，GitHub结果6116669351，QA `testing-loc-m17-fix06-review.json` 与中央 `loc-m17-fix06-source-verdict.json` 已源码限定接收。八叶源根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix06`；driver236347B/SHA715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1；freeze2807B/SHA08d12270a97b2a87be19debc9a711b71854d9d8cacacddf256e517db23387521。后置 `loc-m17-fix06-command-receipt.json`19691B/SHA10cc6a35761da1464cf3508ccaa9ad0755a92804796ec4a6cde362da3731812b 须被新 activation 的 sourceCommandReceipt 准确绑定，材料建立前及每条 native 命令前鲜核。

绑定就绪后，经正常工具审批运行中央 host 准确 argv 一次。复用全部31个NuGet归档和1个源码tar，0 GET、0 新增下载；缺失/链接/hash/保护项不符即停止，不能下载。按已审 prepare.executionBudget，最多三次 restore：runner生成、tests生成、tests锁定验证，各180秒。整个真实调用含输入/SDK鲜核共900秒，870工作＋30收尾。所有输入/SDK预检计入同一时钟，不在 timer 外提前扫描 SDK，不重跑SOURCE/旧检查或已有有效测试。

新M6 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix06`；新W6 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix06`。仅按已审模型写入，既存即停，不清理或换名。M5/W5与全部历史归档/partial/证据只读，按FIX06固定清单保护，不重新发现旧partial。

新增外层E08 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m08` 仅 before.json、stdout.log、stderr.log、receipt.json，总≤16MiB、各日志≤4MiB。记录原工具ID、实际开始/结束/退出、完整墙钟、保护输入、三restore实际收据、材料manifest与进程静止。工具拒绝/中断不等于DUT退出。保留HOME/home/CODEX_HOME。收据完成后保留未截断原工具输出/完整时钟；失败保留partial和具体原始日志，不自动重试。

必须实际执行并接回结果或真实阻塞，不能只写before就结束。沿前次已有外层监护方式运行≤900秒，等待调用≤55秒；不要重写新执行器/额外native探针。若发现模板不能保持额度，先报告具体缺口。上下文压缩或用户状态询问不取消本次任务。完成留本线程，中央wait/read接回后安排QA；作者不自签ACCEPT。不执行B/L、工具build或官方测试、Unity/APK/设备、Git写入、部署、发行、购买或新增下载。
