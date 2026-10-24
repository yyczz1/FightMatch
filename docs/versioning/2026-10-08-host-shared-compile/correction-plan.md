# HOST-INTEGRATE-001/I01-FIX01 源码纠正

2026-10-08。PR18 head `54534228f88a7e8609669b00e1aae2eb4c7caaf3` 的两条 P1 由主测试 actual `01a117c1-04a7-7100-b1f2-ef387f832235` wait42 completed 接回。中央唯一 NEEDS_FIX 见 `host-integrate-001-i01-source-verdict.json`；原 27 项离线通过事实保留，native仍0。

唯一 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh、fresh actual；中央唯一完成接收者。只修以下两点，不扩张编译证明、进程准入、产品输入或执行次数：

1. `r4210875526`：直接 wb/xb 在失败时会留下被当作第三方漂移的半成品。采用同卷独占临时文件、完整写入验字节后原子提交，覆盖同步和恢复对 P 的写入；失败的半成品留在本包证据域，不让 P 莫名受损。不得把未知 P 字节一律视为己方并覆盖。先验备份再触碰 P，保留第三方漂移。
2. `r4210875534`：discover 的 details 异常不能再调用 ps 决定同一循环状态；该 supplied-snapshot 循环应明确失败并保留原因。不能用进程名放行或虚构已退出。静态/实际函数检查应覆盖 discover 自身及调用链，而非只查 monitor/helper。

新源码根 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX01-source/`，只准 `runner.py,replay-check.py,replay-results.json,correction.patch,preparation.json` 五叶。固定旧源 I01/runner.py 41638B/SHA `e39ccb11949a432020f37592d69ffa83f1eb2173ad9088aada7fb6dcb1cd4c59`，旧checker 13372B/SHA `794498fff48a3482aba3cd0b62fa76ab5652b56c35366ef81927f95ad6c95f50`；旧五准备文件及全部证据不改。沿用旧 inputs.json 806564B/SHA `d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3`，产品集合与 canonical 不变。

后续 native E 改为当前应缺席的 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-run/`，避免覆盖冻结的原 I01 五叶。经新head审查后才准将新 runner/checker/results/preparation 四叶和原 inputs.json 精确复制到此 E，再加入原 I01 编译单已列执行证据；相同层级保证 runner 的项目根推导不变。只改未来输出根数据，P/K、实际产品输入、canonical、次数和预算均不变；本轮不创建 I01-run。

为原子提交补充新 E 的精确白名单：`atomic/sync/<原20同步路径>`、`atomic/restore/<原16覆盖路径>`，只允许对应相对路径的临时普通文件，均在同一外置卷。不得生成任意兄弟路径/扩成新临时树。纳入 E 原32MiB限额和清单；后续归档/保留半成品须分列来源，未知文件不清理。本轮仅修改源码，实际 E/P 临时写入0。

离线回放调用真实新函数，增加写入已产生部分字节再失败（覆盖/新增）、提交失败、已提交前项恢复、第三方漂移保持，以及 details 异常时零第二次 ps 的用例；保留必要既有快速反例。可用受控内存文件系统，不接真实 P/ps/kill/Unity。语法/回放最多2轮累计30秒，首过即止；runner仍≤480非空行。来源检查/静态机械≤120秒，记录真实墙钟与机械口径。

直接 final 交五叶身份、精确patch、回放和未执行项，由中央 wait 接回并更新同一 PR18 复审；不新开审查会话、不自行推送、不启动 native/下载/存档/新TMP。新的源码审查通过后再固定实际激活与源封签，不沿用旧源码的审查结果。
