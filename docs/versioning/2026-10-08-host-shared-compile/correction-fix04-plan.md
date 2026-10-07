# I01-run恢复与最小后继

2026-10-08 · DESIGN_ONLY；主程actual `01a11802-d7ba-78c1-9ca5-947296efd44d`，中央唯一收件。diagnosing-bugs复用失败；本轮不复现/修复。

E=`TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-run/`。固定SHA256：

| E内文件 | SHA256 |
| --- | --- |
| runner.py | `b11eb06bc058362aaf833e5070876dc12db0e49491612f7570b87d8e26071317` |
| replay-check.py | `82ca9b61fe0f9562a7c3f9b657ea3e585239904be74a2e5c7d94cca4394e1531` |

## 1. 精确恢复包，独立于源码修正

原runner10.855秒FAILED，exit/restore=null。中央已wait185收C completed并签`host-integrate-001-i01-run-integration-receipt.json` NEEDS_FIX；转述P1000及20后像/16备份/12park一致，59叶仅日志追加。本稿不重复收件。

42225：PPID1/start `Thu Oct 8 04:14:56 2026`，固定Editor NetCoreRuntime/dotnet执行DotNetSdkRoslyn/VBCSCompiler.dll，pipename `CkZH4Vcyv7lfH7i1Cg6qHwHE6JygAhEaLg7pR21Axyo`。FD5–9持P/Bee图/日志；43/44/67/68关联`/private/tmp/.dotnet/{shm,lockfiles}/session42172`；FD16及分析器关联`/private/tmp/fm-hi01.n7tUDjLx`。

中央最新单PID查询exit1/空输出：默认零信号恢复，C仍须fresh完整消费者检查。中央激活、C绑回合并走正常审批。fresh核PID/start、exe映射、完整argv/cwd、session/TMP/FD目标与C原件一致，才仅对42225一次TERM、确认≤30秒。FD按目标/dev/inode核，内容不读。已退出则零信号；PID复用、归属缺失或外来消费者即停，无SIGKILL/批量dotnet信号。

fresh无消费者/锁后，沿原账本与原子冲突门恢复16覆盖/4新增/12park至P1008；旧E备份/park复制返还而非移删，共享S01五路径保持。新根`TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-Recovery01/`仅七叶activation.json、recover.py、before.json、process.jsonl、after.json、restore.json、receipt.json，加archive/source内N.overwritten+N.newPaths精确20后像、原获准SceneTemplateSettings归档及原子恢复槽。证据≤32MiB、恢复≤90秒；Library/旧TMP不动，Unity0；中央收恢复后释放P。

## 2. 编译事实与缺证

实读E/I/editor.log：100793B/SHA `48d397ee7646bd29f89edf150e5a78fa2ce1a94f3e3ed3fee1fe7da92162b131`；前22222B匹配原receipt，追加78571B。

日志238、540/541/544/545有五目标Csc；550后段成功、555重载、855退出文本。但527有CS2001，指向暂移的AssetAccess/FightMatchAssetContracts.cs；236先ExitCode4，随后重建图成功，不能称全程无错或OS exit0。

compile.json仍stages={}、fullImportedInputs=null，五DLL早封签值同before；run_stage:402因problem拒绝，未到compile_evidence。缺退出后rsp/源码/引用/输出与固定1000叶关联、实际退出码和安全恢复。中央只依据C已有封存补证，缺项INCOMPLETE；不修改原FAILED、不放宽log_guard、不清缓存或加旧源求绿，不自动重编。

## 3. 最小源码差量

仅以下三个点；不改编译判据、ADB、预算或原子账本。

- **tree_entries:187–198、monitor:132–142：** FIFO仅准owned dotnet，漏固定Editor的Unity.Licensing.Client；resources失败50.002Z还早于owned登记50.100Z。先同快照发现身份再检查FIFO；仅增精确许可exe/本次root父链，保留TMP/UID/命名/lstat/零内容读取。
- **details/discover:34–56、snapshot_consumers:125–131：** 42200 ps args返回1即抛错。错误仅留待核，不发布owned；后续正常快照证明消失/僵尸或同身份补齐details才解除。exit1不等于退出，PID复用/持续失败无信号权，不追加同周期ps。
- **closure:143–167：** 147/164裸discover在非strict monitor之外抛出，提前终止收尾。逐周期捕获错误并保留首错，继续原60秒自然期/TERM/30秒确认；每快照发现一次，失败/跳过采样不算clear。TERM仍需fresh身份/消费者门；未核实者零信号，未闭合不恢复。

## 4. 源码包与验收

中央另激活`I01-FIX04-source`；新根`TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX04-source/`仅runner.py、replay-check.py、replay-results.json、correction.patch、preparation.json。复用两脚本，旧根冻结；主程负责，C用Astra/xhigh，中央收件。

实际函数反例，后继先红后绿：

1. 同快照许可进程＋FIFO走真实三函数；dotnet保留，错UID/根、外来同名、PID复用、非FIFO拒绝，内容读取0。
2. details失败在**closure首周期内部**发生，再测消失/恢复/持续错误/PID复用。旧checker:87–104先在closure外恢复身份，漏此次断点。
3. ps/资源/投影异常均走60/30；未知者零信号、owned至多一次TERM、未闭合不恢复、原失败保持；旧58项覆盖保留。

离线≤2轮累计30秒、首过停止；真实Unity/ps/信号/P写入0。runner470→≤580行（+110），同步guard/checker，不压复杂语句；原预算保持。后续走GitHub门，本轮不触发。writing-for-agents分开恢复与源码验收。
