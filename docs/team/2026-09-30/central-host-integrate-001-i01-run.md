# HOST-INTEGRATE-001 / I01-run：一次共享组合编译

2026-10-08。中央已实收主测试 actual `01a117f7-db3e-7352-9ad4-fb32ea100aa7`，wait74 completed；PR18 head `48c51dba8cb6487f405f9291fb1a8ab1b20b6c20` 当前审查结果 `6045886949` 无major/新增finding。唯一源码限定ACCEPT见 `host-integrate-001-i01-fix03-source-verdict.json`，不是formal APPROVED或原生通过。

用户继续开发、真实场景/保存复开/限定验证授权覆盖本包；中央批准原 `engineering-host-integrate-001-compile.md`（13877B，SHA256 `38fa7b99d457933fbdae993aa89f0922fc94cc1ab205e05bfae4c48f5b45c1db`）加FIX01/02/03的精确修正。唯一执行 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，必须绑定当前新实际回合；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一完成接收。

执行根改为 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-run`，须先确认缺席。只复制FIX03-source的 runner.py、replay-check.py、replay-results.json、preparation.json 四原字节，inputs.json从原I01逐字复制。runner47602B/SHA `b11eb06bc058362aaf833e5070876dc12db0e49491612f7570b87d8e26071317`；preparation31200B/SHA `d68ff9db8668910a9e46c4fc9bd12906a07f390e9dfad2a3627e4cea33bb23e7`；inputs806564B/SHA `d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3`。旧四个根二十叶保持冻结，不再跑已通过的离线回放。

activation只绑定数据：新actual owner、准确来源与审查、固定Editor、P/K/R和输入/证据白名单、一次新TMP、原预算与封签。允许FIX03 preparation列明的有限atomic槽，不能动态扩目录/修改runner。取得新鲜ps/lsof且R/P无消费者/锁后，才按账本同步16覆盖/4新增/12暂移。共享999原输入不变；P编译1000（含唯一已存meta），结束恢复原1008。现有ADB只按原精确例外，不给它信号，不操作真实存档。

准备累计已保守计49秒（预算120）；本轮继续累计。Unity一次I≤360秒，自然收尾≤60秒、核实owned后逐PID最多一次TERM并确认≤30秒，归档恢复≤90秒，总机械≤660秒，审批等待/墙钟另记。走正常执行审批；如被拒保留原文，不重试或换路径/执行者绕过。无Play/测试/build/下载/forced recompile/native重试。

按审过runner原判定接收编译日志和真实输入证据；缺完整cache复用证明须INCOMPLETE，不能用旧DLL/exit0冒充。共享S01五路径保持；只允许既定投影同步与安全恢复。失败保护现状并如实交回，不自动修源再跑。完成本会话final，中央wait接回，禁新增主动跨会话回执步骤。
