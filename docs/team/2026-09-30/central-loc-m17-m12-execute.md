# LOC M12：FIX09 一次零下载依赖还原

直接人类授权：`central-loc-m12-approval-2026-10-12.json`，中央授权 turn `01a1306b-42a8-7992-9118-4ed11d490741`、人类 item `01a1306b-430a-7b61-8e60-3c85a081d47f`。用户批准 driver 最多280KiB，修正、静态和准确head GitHub审查通过后，在外置盘一次执行三条还原，完整调用不超过900秒，零新增下载，中央接收。本次不复用M11次数。

源码为 FIX09，PR2 head `5e2d3790d371659552c9bb24084252c70972f474`；53项静态与SOURCE封存已有主程接收，不重复运行。必须先有该head的GitHub完成结果及SOURCE接收。中央据此填写既审 `central_host.py` 模板的CONFIG，生成 `central-loc-m17-m12-activation.json`、`central-loc-m17-m12-host.py` 和 `central-loc-m17-m12-host-binding-receipt.json`；不改模板逻辑。绑定sourceCommandReceipt为最终 `loc-m17-fix09-command-receipt.json`（28904B，SHA256 `9fbf4b1d82a961ca1036df5209e6977c4756f062112dfa790aaacd51381ad87b`）。预算逐字采用FIX09 prepare的executionBudget。

LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local` 为唯一执行者，Astra/xhigh；中央为唯一结果接收者。中央通过wait读取新实际owner turn后绑定；LOC在本会话接包后最多等待绑定3分钟，每次不超过30秒，不另建身份回执。就绪后以绑定准确argv经正常工具审批执行host一次；不修改源码、A/H或权限。

新M9：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix09`；新W9：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix09`。两者执行前须不存在。新E12：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m12`，仅before.json、stdout.log、stderr.log、receipt.json，沿原16MiB总量与单日志4MiB限额。旧M8/W8及全部历史证据保留；M11保护仅指固定记录，不宣称新整树库存。

只读复用31个NuGet包及1个源码归档，0GET。三条restore按既定顺序各最多180秒，完整调用含预检及SDK核对最多900秒（870工作+30收尾），不在计时外扫描SDK。沿现有外层监护捕获完整工具命令、原输出、退出及完整墙钟。保留父HOME/home/CODEX_HOME；材料512MiB、工作4GiB、磁盘余量2GiB及其余预算不变。

任一步失败停止后续依赖步骤，不自动重启或删除partial；成功也不启动B/L、Unity、Git、部署或购买。记录三条restore各自结果、manifest、保护项与进程结束。结果留本会话，中央wait/read直接接回，不跨会话重复转发、不唤醒其他角色重复接收、不自签ACCEPT。该实际结果与SOURCE审查是两个独立验收事实。
