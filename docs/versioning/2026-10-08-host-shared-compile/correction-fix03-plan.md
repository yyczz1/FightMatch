# HOST-INTEGRATE-001/I01-FIX03：恢复本次启动根进程的登记

2026-10-08。PR18 head `191ee590dbb644ce129a6d6b5787555fcade8af0` 新P1 `r4211236459`；主测试 actual `01a117e3-96f4-7ba1-a4e6-d350073583e9` 已wait67 completed，中央唯一 NEEDS_FIX 见同前缀fix02-source-verdict。48项旧离线通过事实保留，native仍0。

唯一 C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，fresh actual；中央唯一wait接收。只补本次实际Popen根进程的恢复登记：首次details暂时失败后，closure在认定无进程可停止之前，必须从后续新鲜supplied snapshot重新尝试完整核验并登记该根，再发现子进程。只认本次实际Popen身份及其固定argv/cwd/Editor/生命周期；不得把同PID的新进程、同名Unity或外来进程加入owned，不在一个snapshot循环内重新ps，不留半条owned。

如果本次根已经退出，按实际Popen状态处理；如果持续不能核实而仍活着，明确保留未闭合状态，不能用owned为空宣称已全部退出或恢复P。仍遵守原自然等待、逐owned-PID一次TERM与总时限，禁止增加native次数/信号/不限量重试。原启动失败记录不改写为成功，完整身份恢复只用于安全收尾。

新 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX03-source/` 只准五叶：`runner.py,replay-check.py,replay-results.json,correction.patch,preparation.json`。固定FIX02 runner45994B/SHA `6fcc05eb0191b3394338750e69a6a9eb96e5fb6a22e62840c029ecbdf8fc9570`；checker26565B/SHA `4c105c2b3132dedb0547bc3bec929dde4a319c8c28d1c3fa5bcb909fb1f45fc7`。原I01/FIX01/FIX02十五叶及inputs原字节全保持；产品集合/P/K/canonical/atomic策略与有限证据槽不改，未来I01-run仍不创建。

真实函数离线反例需覆盖：Popen后根details第一次失败、下一fresh snapshot恢复并经实际closure完成模拟清理；根已自然退出；PID复用/外来同名拒绝；持续缺details时不假报闭合、不授权未知信号；已有child恢复与完整owned检查继续有效。尽量在现有夹具上增加，不造新监护框架。语法/回放最多2轮累计30秒，首过停止；runner仍≤480非空行。源码准备累计机械≤120秒，前序32秒继续计入，墙钟另列。

真实Unity/P/ps/信号/TMP/下载/存档/Git均0。完成直接final交精确patch及五叶身份，中央更新同一PR18复审；原生执行必须等新head审查与实际激活，不能自动启动。
