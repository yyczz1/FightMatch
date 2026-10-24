# FIX01-T01：采用自然完整meta，接首次十项测试

2026-10-08。中央直接激活唯一C/Astra-xhigh，中央唯一收件；M03 actual `01a1170d-c73e-72d2-bbf7-f8137d8c454d`已wait48确认completed。沿M02/M03源码与预算规则，本包**不再运行I、不再同步源码**，只证明已有编译与当前输入一致，再运行尚未执行的T。原失败和输入保护结果不回写。

## 已产生的输入与许可

M03 receipt9090B/SHA `d6d3ee5f364ef83f3f433703971d1a3626582fdb3a834ffeb01f13cb0f50c132`；I/editor.log70441B/`237b00bf77cb5c1b900b4de12ee9524c1fbdb74b8979018f74ba96fc1355a0fe`；inputs420831B/`39a4ef7fd384053bdabe046edd92a9f4f29c1d3289736f408f2e4979e8307e4e`；compile9421B/`c5239811f5ffa3588dda41e165ee2a3b352f2e54e566d0c3561397f36ba75eba`；after3566B/`6bfaba13d231ac7ca804b17203e22057daf76b41850c3843830d99c483104715`。M03共23叶4772522B冻结。

I进程exit0，日志Csc236/237/238为Presentation/Host/Host.Tests，无CS错误且正常batchmode退出；Host.Tests新DLL116736B/SHA `86f9289d368d0c4978f4b8d41a1c3daa68b9156e0d09faf765f57672d6bf7ba0`。所有owned已闭合、VBCS30313唯一TERM已消耗，锁缺席；这些事实不改原wrapper因meta门失败的结果。先用原日志与before/DLLsAfter精确核三个程序集与当前实际DLL、C#源一致，成立才可复用该编译；证据不足则停止，不能偷偷再编译。

中央明确接收**这个测试meta的自然补全**作为新的固定输入：243B/SHA `3772247bb6d5f6883840c20f6c5423341bd0fa9e43c60e3d3e4f6dcc65778273`、blob `1478f6a376bc6d86ba8d6e1bc369cdf622506939`，GUID仍 `0c2429a8af0d0415f8da19df191112fc`。新增仅MonoImporter默认字段（externalObjects空、serializedVersion2、defaultReferences空、executionOrder0、icon instanceID0、userData/bundleName/bundleVariant空）及末换行；不得再改写。原59B保留于M01RecoveryR1。

P仍为现有M03/projection当前候选，文件集合等于M03.inputs，只更新此一个meta身份；1012叶32086333B/canonical `786fa16c0c7d1c294dd407f2b58d15c2e68011c08f0dac7f4b2c5ebc3e305c4a`。来源仍FIX01三行测试修正及原固定依赖，源码没有其它变化。新PR meta补充由中央在独立版本文档/远端完成，同一输入的新head需另收GitHub门。

## T与完成恢复

新E=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/T01/`，先核缺席。准写原M03根13文件（inputs/cache-inputs/before/preflight/activation/runner/process-events/process-after/after/compile/restore/publication-inputs/receipt），仅T五叶（editor.log/launcher.log/result.json/results.xml/discovered.json）；candidate32叶，restore/source八叶可复制M03原像，以及最多两个test-io新GUID。无I目录或伪造I本轮记录、无新meta槽、无其他写类。

复用M03 runner36579B/SHA `328c27acb735144fd2bac10f2bdd8c7f703c46571c0e3cd9a6558a5eac0455d3`的监督/预算/实际测试解析，只改T-only入口、既有编译核验、243B meta/current候选前态、恢复来源及新E；不重造框架，≤410非空行。新TMP `/private/tmp/fm-hc-t01.XXXXXXXX`，短路径/UID/0700守卫保持；所有旧TMP不动，旧ADB精确两日志FD例外保持，CLR FIFO只lstat。

准备机械120秒／静态30秒，T一次≤180秒，总执行器wall≤420秒（含60秒自然/最多一次owned TERM/30秒确认/归档），零重试/discovery，Unity只这一次T。T不带-quit，十个fullname/各1次/10Passed/零失败跳过不确定，仍分8+2。测试启动如发生Unity正常的增量重编，记录实际事件并核全部源不变；不是另启I，不删除缓存强制编译。

P输入启动前及结束均按新1012闭包保护，K冻结；E≤32MiB、单log8MiB、P生成物4GiB、K1GiB、新TMP16MiB/512叶及外置余量2GiB、HostRig最多两个新GUID等沿原合同。共享、全部旧证据和Git保持。原默认Host/真实档/Play/场景保存复开导出/设备/下载restore禁止项不变。

T结束owned闭合后，封存32实际候选（含243B meta），从M03/restore/source八前像恢复P，迁移四新增输入至新candidate，恢复原1008叶；不以新before中的候选前态冒充原基底。原51 HostRig保持，只归档本次最多两新目录。Library不回滚。每项实际结果单记；未知漂移停止相关恢复，不修改断言／旧receipt追绿。publication-inputs从原PR1 base展开同32路径、仅meta后像改243B。编译复用证明、T、恢复、同新head审查与Demo是分开的完成门。
