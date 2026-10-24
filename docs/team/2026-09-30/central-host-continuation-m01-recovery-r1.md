# M01RecoveryR1：区分ADB日志与编译消费者

2026-10-07，中央直接激活唯一C，沿[原精确恢复包](central-host-continuation-m01-recovery.md)，只替换本页列明的证据根和ADB判据，其他边界不变。无需新设计轮／新角色／人类决策；中央唯一接收。

实际新证据：M01Recovery/before.json 4180B/SHA `c05d00f6506d2c82dfadd6ad5464a0d5ee8101f0fe241120ee289f07ccb390d0`；receipt1225B/`28ac0958f6b3d0b4c0b17dd03f6b768d6dbb7541c6f89a5a3228f45047b43fe1`。此次检查3.206秒失败、信号/迁移/恢复0。完整FD实测仅ADB stdout1/stderr2指向 `/private/tmp/fm-hc-m01.9qXZKifu/adb.501.log`；没有P/K或VBCS pipe关联。临时目录／该日志本就保留，因此它不妨碍只终止owned编译服务并恢复P源码。原失败保持。

新根仅 `TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/M01RecoveryR1/`（先核缺席），写清单仍为before.json、cleanup.py、after.json、publication-inputs.json、receipt.json及32个candidate叶。复用原cleanup.py14318B/SHA `1af8b73911c77bc90d9dade87f7f31127354d78d1a6a5fb6f95365c1641d409f`，只改根、旧证据保护列表和以下精确条件；允许为最终只读记录保留失败观测，不再丢失抛错前FD事实。原M01及M01Recovery四叶205494B冻结。

重新核同一ADB PID27858/start/argv及完整FD。仅当与P/K/TMP/pipe相关的FD恰为1和2、均为上述同一路径且它仍是同一普通非链接日志，P/K及VBCS pipe关联为0，才把这两条日志FD从**编译消费者**判断中排除；其它关联一律停。ADB保持运行，日志允许作为该外部服务持续写入，不承诺其字节冻结；记录前后身份、长度／时间及差异，不删除／截断／移动它或TMP。此精确例外不适用于任何未知ADB、新PID或其他文件。

随后沿原包重核VBCS PID27883/start/argv/pipe/FD及无其他消费者；若已退则记录，否则最多一次TERM＋30秒确认。闭合后执行原32候选封存、八叶前像恢复及四新叶迁移，P恢复1008；保留自然meta GUID。新执行至多一次，静态准备60秒／执行180秒／8MiB，Unity／测试／Git／其他进程信号0。

可在当前C回合完成原四叶正式封存回执后接本独立阶段，记录同actual下两个任务映射；若原回合已结束则绑定新actual。不要重复旧runner或改旧receipt。与FIX01作者的新源码无写域交叉。
