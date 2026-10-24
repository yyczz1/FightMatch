# FIX01-M03：清理无主空锁后执行既定验证

2026-10-07。中央直接激活唯一C/中央唯一收件；沿[FIX01-M02固定合同](central-host-continuation-fix01-m02.md)，产品／测试源码、自然meta、13来源/1012叶/十项selector/所有原生及资源预算保持。无需再次设计或人类决策。

M02新鲜失败：receipt3602B/SHA `4bcbabeaf8e97b0ce434c2c58882b6c44c32c4932dcb6f4284846668c1ca2dad`，`Projection lock present`；stages空，Unity/同步/测试0、owned空，P仍原1008叶。原13叶3564393B冻结；不重跑其runner或覆盖回执。

锁唯一目标：P=`TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/projection/`下 `Temp/UnityLockfile`。已实际观察为普通非链接0B/empty SHA、UID501/GID20/mode0644/device16777244/inode10924639、birth1791386048.184、mtimeNs1791386048183999947；lsof该路径exit1且空stdout/stderr，ps完整候选只剩已知ADB27858，无Unity/编译进程。此为可恢复的原生临时残留，不是源码，也不是清缓存许可。

新E仅 `TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/M03/`，原M02准写清单原样映射E；额外只准 `restore/UnityLockfile`这一0B归档叶。在本轮preflight中重核上述lstat身份、当前无Unity/编译消费者、该路径无FD占用；再次紧邻核身份后，将原文件重命名移入E/restore/UnityLockfile，记录原/归档身份。若身份或持有者变化立即停止，不操作新锁。不删Temp目录、其他锁、cache或TMP。

复用M02已固定runner，只改本轮E/保护清单、上述精确归档和对应回执字段；≤410非空行，静态≤30秒，准备机械≤120秒。归档后核原路径缺席，再执行I一次≤360秒→成功才T一次≤180秒，执行器总≤900秒，零重试。新TMP模板 `/private/tmp/fm-hc-m03.XXXXXXXX`，原两TMP保持；已有ADB精确旧日志例外／FIFO仅lstat／owned监督与收尾条件不变。仍禁止默认Host/真实档/Play/场景保存导出/设备/下载/Git。

修正版源是同一PR16 head `1e2f7ad5382e6c36ec402e14fa224e1de286d1fa`的32产品/测试/meta叶，中央已核全部远端blob。以publication-inputs机械核同一组叶，不改任何源码以追求通过。M02失败保留；新I/T结果、归档32叶、P1008恢复和临时空锁迁移各自如实记录。

可以在当前C回合封存M02原13叶并回执后接独立M03阶段；若回合已结束则新actual，记录映射。前包许可不能被误读为需要再次停在激活前，本页及中央消息即本轮激活。
