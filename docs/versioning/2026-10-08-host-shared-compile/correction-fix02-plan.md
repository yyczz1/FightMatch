# HOST-INTEGRATE-001/I01-FIX02 源码纠正

2026-10-08。PR18 head `1c5a34ecd126135abb40a9e610a175c000e8d772` 复审新增两条P1，主测试 actual `01a117d2-04f8-70f2-9724-8e1f1bfbbf78` 已wait55 completed；中央唯一 NEEDS_FIX 见 `host-integrate-001-i01-fix01-source-verdict.json`。既有37项离线通过不覆盖这两种新反例，native仍0。

唯一 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh、新actual；中央唯一wait接收。仅修新P1：

- `r4211058182`：普通replace不是CAS。已有目标的提交/恢复须原子保留被换出的实际值，检验它是否是prior；不匹配时恢复该并发值，原值与冲突证据不丢。若恢复期间又有新漂移，保全各值并明确失败，不不断重试或覆写未知字节。新目标的exclusive commit继续保留；不要把一次额外preimage检查冒充原子条件提交。
- `r4211058192`：register先收集并完整核对details，再一次发布owned项；异常时不留下半条owned，后续新的supplied snapshot可以重新识别。当前失败仍如实记录；不能通过补空argv/cwd骗过守卫。保留已有效owned项，失败子项不能毒化合法根进程的清理身份。

只在新 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX02-source/` 写 `runner.py,replay-check.py,replay-results.json,correction.patch,preparation.json` 五叶。旧 I01 与 FIX01-source 全部原字节保留。固定源 FIX01 runner 44197B/SHA `2f5f11ec8011ef8de13774fd5813af1ef697f56634087c7c40f25f197bddf812`，checker 19747B/SHA `efd02cef050f208438bb07af07402bedd24b7ee3b72f4ac5ab20b9f9f2f3fc98`。原inputs.json 806564B/SHA `d1e4d34147305cfc073800d7c6c911d18593a5f91c91735409a28b7d677726e3` 不变。

未来 native E 仍为缺席的 I01-run，本轮不创建。沿FIX01精确atomic路径，可在同一 E 下补充 `atomic/displaced/sync/<原16覆盖路径>`、`atomic/displaced/restore/<原16覆盖路径>` 与相同32路径的 `atomic/conflict/{sync,restore}/...` 用于被换出/冲突值；这些是有限保全槽，不是无限重试许可，最多各一路径一叶，计入原E≤32MiB。若用现有stage槽即可安全保全，无需制造多余副本；在preparation冻结实际采用的精确叶集合。P/K、产品集合、原子partial证据、次数及监护边界不扩大。

实际函数离线故障注入至少覆盖：恰在最后preimage检查和提交之间写入第三方值（同步和恢复）；冲突值被保全/还原；归还冲突值时再次漂移不丢新值；details首次失败而下一份snapshot成功时owned状态完整恢复；合法根项仍可经原守卫清理；PID复用/外来父链仍拒绝。旧夹具须适配新提交primitive，不能让注入悄悄失效。保留必要快速反例，语法/回放最多2轮累计30秒，首过停止，runner≤480非空行。

本轮真实P/Unity/ps/信号/TMP/下载/存档/Git均0。源码机械≤120秒并累计前序成本，墙钟另列；先前失败记录不改。以精确patch和五文件回执final交回，中央更新同一PR18复审；不得直接执行未来native或自签接收。
