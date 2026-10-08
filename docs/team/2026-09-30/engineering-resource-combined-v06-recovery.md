# RES-COMBINED-V06 一次限定恢复

中央最终派发，唯一C执行。当前只设计恢复，不启动Unity/测试/探针/信号，不修改runner，不推进V07。旧FAILED、closureClosed=false和所有原日志保持原样；“恢复成功”不等于验证通过。

## 固定事实与输入

E6=`TestArtifacts/FightMatch/RES-COMBINED-V06/run`；P=`TestArtifacts/FightMatch/RES-01A/P01/projection`；C原actual=`01a1198b-3beb-7052-b04c-5cd441a5eee7`。receipt30369B/SHA256 `c0da0f99a0675ddce7510cb257b2a7da971d500d1c81cfd5cf0c6b8f4e8d5755`。process-after载明remainingOwned=[]、pending={}、popenStillLive=false、closureClosed=false、transferConflicts=[]；restore=null。中央已核原外部exit1、执行117.2632786秒；历史记录不是当前无消费者证明。

只读封签：inputs.json1248829B/SHA `4fdcfff78f03da819b028e596db29c4ea584c7fde79d9830b929da2d4321405b`；before.json514041B/SHA `d25480c094098af4048ad72903c30b8e1728eb2ea57fee929c3fdce82d3df351`；process-events343305B/SHA `4831dc9696809e9eb82b10f5e3787ef2512bda21be61154de5de9fba4e78bc04`；runner101320B/SHA `415a8a5bd312875e972363d4ad9cb504ac4b8579390eb2ad2081a0323fb36119`。

events有21次source_synchronized/atomic_committed，62次transfer（17备份+2暂移+43编译暂移）、45次transfer_committed，无transfer_return_blocked/atomic_conflict。实际前像位置和内容仍须C逐项验证。htc绑定后cth ENOENT及目录变化导致资源检查持续失败，阻断综合closureClosed；不能把这些历史TMP异常当作当前进程仍活，也不能把remainingOwned空直接当恢复授权。

## 不可直接调用原恢复入口

原restore_all和archive_and_restore先拒绝closure_closed=false；后者tick还调用resources，继续遇到旧Bee目录异常。原E还承载event/restore.json写入。不能把旧闭合位改true、清monitor_errors、调用main、替换resources为空函数，或让一次导入覆写旧证据。

需要一个独立、一次性、恢复专用调用脚本/命令稿；可复用已审rename_exclusive、rename_swap及验证前后像的原子转移算法，不能直接重入整套restore_all。它只实施下面有限清单，独立记录recoveryConsumersClear，不写原closureClosed。原源只读；不得新增通用监护框架。若C无法在该范围使用既有原语，停止并报告具体缺项，不临场扩大恢复。

## 写前必须一次完成的检查

新证据根Q=`TestArtifacts/FightMatch/RES-COMBINED-V06/recovery-01`，必须不存在；同外置卷、真实目录、无链接。恢复所有路径仅来自已封签inputs的17 overwritten、4 newPaths、2 parked、43 proposedParkExactLeaves及allowedNewSettings。

1. fresh全进程快照沿用原ps完整字段；核对旧owned PID/start/exe是否仍在、是否复用，并按原consumer_guard规则排除Unity/编译器等消费者。原已允许ADB例外须重新核对身份/用途，不按名称新增豁免，不发信号。已识别有消费者、无法证明身份或探测失败均停止。
2. fresh FD快照可用 `/usr/sbin/lsof -nP -Fpcftn`，只在内存筛选P、E6前像/转移槽及Q相关完整路径/目录边界；不遍历Library或包缓存。保留相关记录和完整输出身份，不保存无关路径明文；命令失败、输出截断、权限/不可读导致不完整均停止。除本恢复器已固定PID/FD外，任何进程持有待改文件、备份或相关目录的FD/cwd均阻止写入；关闭自身预检文件句柄后再进入变更。进程和FD证据联合成立，不以“没搜到名字”代替证明。
3. 按transfer事件确认62份前像完整，逐项无链接、常规文件、大小/SHA等于inputs/before。盘点所有既有atomic capture/retired/conflict槽，确认没有未解决转移；不得删除它们。
4. 固定P当前66项（17+4+2+43）的存在性及身份。源17+4应匹配本轮同步值，2park目标应缺失；若已等于原前像可记录已恢复而不重写，任何第三种内容/类型停止。43项当前编译产物可有或无，但现有者在此刻固定完整身份，之后逐项匹配；其替换档案总量≤原32MiB。允许的新SceneTemplateSettings仅按原精确路径/内容身份处理。预检全通过才开始修改，不重新解析包、不读整缓存。

## 有限恢复操作

所有E6文件保持只读；前像复制到Q的限定staging并再次校验，保留原备份。Q中所有目标用exclusive create/rename，不覆盖已有证据；无链接、同卷与逐项现值匹配贯穿操作。

- 17覆盖：复制并验证本轮P值到Q/archive/source/<path>；将原前像写到限定staging，使用已审原子swap算法恢复P，检查被换出者等于预检现值；被换出字节保留在Q。若竞争，按既有算法最多一次有证据的交换归还，不能覆盖第三方值。
- 4新增：仅当P仍等于同步值，按原验证捕获/无覆盖转移算法移至Q/archive/source/<path>，不删除。若已缺失只记录，第三种内容停止。
- 2暂移：从E6/park/source/<path>复制已核前像到Q/staging后，exclusive安装回原P路径；目标必须缺失或已为同一前像。原park证据保留。
- 43编译叶：原前像仅来自E6/park/compiler/<path>。若P有新产物且仍等于预检身份，先无覆盖转移至Q/archive/compiler/<path>，再exclusive安装已核前像；P缺失则直接exclusive安装。不替换Library/Bee整目录，不恢复或清理TundraBuildState.state、bee_backend.info等清单外产物。
- allowedNewSettings若存在且精确符合原合同，限定转移至Q/archive/SceneTemplateSettings.json；不触其它生成设置。空目录保留，不执行目录删除、广泛清理或缓存失效。

每次破坏性转移前复核相应现值，至少每2秒重新检查消费者；任何新消费者/竞争/探测失败停止后续改动并保留部分恢复报告、原备份和档案。历史TMP/Bee IPC无需修复或清理，不将其异常混入本次“无消费者”判定；新证据或恢复目标的实际异常仍阻止恢复。

## 输出、预算、成功判据

Q白名单：recovery.py（或封签命令稿）、plan.json、process-before.json、process-after.json、fd-before.json、fd-after.json、events.jsonl、receipt.json；以及仅从上述有限路径导出的staging/atomic/archive叶。不得写E6/restore.json或改E6日志。Q≤原证据100MiB，其中compiler archive≤32MiB；外置盘free≥4GiB，不靠删除旧文件凑空间。

独立恢复一次总机械≤120秒：前检≤30、变更≤60、收尾≤30；无重试、无Unity/测试/信号。没有足够变更及收尾时间不开始；超限保留已做项并报INCOMPLETE，不能以新900秒重置预算。记录原运行与本次恢复各自实际耗时，不伪造原运行闭合成功。

成功必须同时满足：P的Assets/Packages/ProjectSettings精确回到projectionBefore的1035文件及canonical；43叶逐项等于封签前像；无未解决转移/交换冲突；前像仍保存在E6，所有移出字节在Q可核；fresh进程/FD后检无消费者，旧receipt/inputs/before/events/runner身份未变。共享源码、UPM/两SDK、AS、旧TMP及Bee新缓存不改。receipt只报告RECOVERED或INCOMPLETE，注明原V06仍FAILED，不能报告native/test PASS。
