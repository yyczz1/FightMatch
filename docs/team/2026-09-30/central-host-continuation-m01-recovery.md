# HOST-CONTINUATION-M01-RECOVERY

2026-10-07。独立收尾恢复包，不是原runner重试，也不启动Unity。唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，中央唯一收件。旧actual `01a116dd-9429-7722-acdf-e88f2eddb4cc`须确认completed/停写后开始。

只读M01=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/M01/`：receipt8231B/`b4569ac57da887f593a4d0c038272244a056a4034e06a5126ce6fbdc51a9528c`、process-after15493B/`af0bce3675f0168551c74f26f5c18c25d268318eaabe2be768e91de72569b820`、after100486B/`d9262909d3875e18dc2307b15db5cb000a4613863a6af51cb64e84bec75e9c31`。22叶4776412B冻结。前包[精确输入与恢复规则](engineering-host-continuation-mac-001.md)保持，原失败不改写。

1. 先只读核当前进程。旧owned VBCSCompiler PID27883/start `Wed Oct 7 23:14:15 2026`，原ppid27875/root27815，exe/argv/cwd/祖链见process-after/events；旧信号0。另ADB PID27858/start `Wed Oct 7 23:14:10 2026`/ppid1为未owned，不关闭。核实际PID/start/完整argv、编译pipe、FD及是否有其它Unity／compiler消费者；不得因PID相同推定同一进程。
2. 若VBCS已退出只记录。仍为同一owned进程且无其它消费者时，准许对此准确PID至多一次TERM，等最多30秒确认；不SIGKILL、不按名kill。ADB保持外部服务；若实际证据证明它无P/K/TMP或该编译pipe关联，可以仅在本次清理中排除它作为编译消费者，记录完整依据。未证明则停止有依赖的动作，其他独立收件仍做。
3. owned闭合且P输入与M01后像一致后，沿原§2执行精确归档／恢复：八覆盖叶先封存候选，再从M01/restore/source恢复前像；三新增输入及自然测试meta移入新candidate；其余发布叶复制固定只读源。保留自然meta 59B/SHA `182331a51f0fdd96f9774ae7e30dfb6e3a525e47d01c1ce34054fe1b575ec47c`、GUID `0c2429a8af0d0415f8da19df191112fc`。P恢复原1008叶；Library等生成物不回滚。M01没有默认SceneTemplateSettings、新HostRig目录，不制造这些动作。
4. 新TMP `/private/tmp/fm-hc-m01.9qXZKifu`保留，不删除／清缓存。CLR FIFO仅用lstat记录种类、owner、mode等元数据，禁止读取管道内容或冒充普通文件hash；本轮观察纠正不回写M01的Special tree entry失败。未知条目如实报告，不扩展删除范围。

唯一新证据根 `TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/M01Recovery/`，开始核缺席；准写 `before.json,cleanup.py,after.json,publication-inputs.json,receipt.json` 及精确 `candidate/<repo-path>`32叶。总≤8MiB、机械准备≤60秒、执行及等待合计≤180秒；复用固定清单，无新通用框架。原包允许的P八叶恢复／三输入与自然meta迁移是唯一投影写入；不动共享、K、S、M01原22叶、旧TMP/证据、Git、任何他人进程。重核原文件保持、P1008、32叶候选与逐叶base差量一致、owned状态及每个实际动作，失败不伪造恢复完成。

恢复是为后续新修正候选验证准备环境，不证明原编译／测试通过。与作者FIX01新S无交叉；不写作者目录，不运行修正代码。完成只回中央，不向主程重复转报。
