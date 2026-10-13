# LOC M09：FIX07 修正后一次零下载依赖还原

用户在中央给出完整FIX07提案后直接答“执行”，授权见 `central-loc-m09-approval-2026-10-12.json`；中央 actual `01a12ee5-5e38-7863-b291-95e758e24669`，人类 item `01a12ee5-62af-73d3-a5b5-c3827b86dfba`。这是新的一次M09，M08失败及其额度保留历史事实。

唯一执行者LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh，中央唯一收件。保持本次实际turn，等待中央新建 `central-loc-m17-m09-host-binding-receipt.json`，最多3分钟、每次≤30秒，不跨聊回报身份；中央将通过wait取得实际身份。中央只填既审host模板CONFIG和activation，LOC不改A/H、driver、八叶、侧录。

FIX07 PR2 head `bd92f6bcedaf89751256a4a7e0084d39cd244893`，GitHub完成结果6118132100，QA `testing-loc-m17-fix07-review.json`与中央 `loc-m17-fix07-source-verdict.json`源码限定ACCEPT。历史FIX01缓存预检P2仍保留，当前固定0GET不涉及其下载序列，不把它写成已解决。
八叶根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07`；driver261874B/SHA9aa2db85274369fa27365c3fa39b120fa68deed0effb527b7fccaa5a06894f48；freeze2807B/SHA5f747b1441f79bb43ff00f17757b36fee2e7c374d3e8dd00570d42d54b835027。后置 `loc-m17-fix07-command-receipt.json`35151B/SHA5c027e28a18b37d2e8abca762bbb84b6304d276840b824f75f04aa7faea56797绑定sourceCommandReceipt并依既审门鲜核。

绑定就绪后，经正常工具审批运行新host准确argv一次。31个NuGet包及1个源码tar只读复用，0GET/0新增下载；缺失、链接、hash或保护项不符即停止。只按prepare.executionBudget依次跑runner生成、tests生成、tests锁定验证，最多三次restore、各180秒。整个真实调用含输入/SDK鲜核共900秒，870工作+30收尾；不在计时外提前扫描SDK或重跑SOURCE、旧检查、OS探针或有效测试。

新M7 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix07`；
新W7 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix07`。
只按已审模型写入，已存在即停，不清理或换名。M6/W6及全部历史目录、归档和证据按FIX07固定清单只读保护，不重新枚举发现partial。

新增E09 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m09`只含before.json、stdout.log、stderr.log、receipt.json，总≤16MiB，各日志≤4MiB。沿M08既有外层监护方式运行≤900秒，等待调用≤55秒，记录原工具ID、实际开始/结束/OS退出、完整墙钟、保护输入、各restore真实收据、材料manifest与已知进程静止。保留父HOME/home/CODEX_HOME。真实工具返回后才补充完整原工具侧录，不截断关键输出；失败保留原日志、partial，后续依赖步骤不启动，无自动重试。

必须接回真实执行结果或实际阻塞，不能只写before就结束。上下文压缩或状态询问不取消任务；模板若无法满足现有界限报告具体缺口，不重写执行器。不执行B/L、工具构建/官方测试、Unity/APK/设备、Git写入、部署、发行、购买或下载。结果留本会话，中央wait/read接回并安排QA，作者不自签ACCEPT。
