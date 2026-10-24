# RES-COMBINED-V07 验证器生命周期纠正

中央确定的普通技术修正，不改变产品决定。仅在V06限定恢复成功、前像核对完成后实施；本文不授权native。旧V06仍FAILED，旧日志/闭合位不重写。

基线：V06/run/runner.py 101320B/SHA256 `415a8a5bd312875e972363d4ad9cb504ac4b8579390eb2ad2081a0323fb36119`，同目录inputs/events及recovery-01实际收件。12程序集、89测试、源输入、原预算不变，生成state重新封签。

## 分开三个判定

- 编译/测试及完整性：12程序集本轮来源链、89个实际通过结果及输入保护仍必需。容量、越界路径、链接、类型/UID、真实I/O/权限/Timeout错误仍失败，不借生命周期分类清除。
- 当前消费者闭合：只依据fresh进程/身份/FD、pending和root退出证明。历史IPC或其它资源错误不代表进程仍活；探测失败也不是空集合。
- 临时IPC观察：记录出现、消失、差异、重新绑定及未决原因，不要求socket存活整个阶段；诊断不授予进程/信号权限。

最终验收仍要求编译/测试证据完整、无未解决硬完整性错误、消费者已清、恢复成功。安全恢复独立要求当前无消费者、封签前像和逐项原子条件；本轮验收失败仍应进行已授权的安全恢复，原结果不改PASS。

## Bee生命周期

185/186行htc inode34720956绑定稳定，原FD同时含cth；187行cth inode34720957约57ms内消失。I/editor.log:234—244有backend1→frontend→backend2，一次I并非一个永久IPC实例。后续“replaced”未保存变化字段，不能断言全部正常。

1. 保留fresh TMP、直接子目录格式、当前Editor根PID、htc/cth精确名称、两种已证完整路径拼写、socket/UID/权限、无链接和完整FD归属。未知类型、越界、错UID/拥有者仍失败；不连接/读取socket，不泛化normpath。
2. 原受限目录/候选端点在lstat或FD采样期间精确ENOENT，记录路径、最近lstat、已有原始FD和时间，记“本次观察消失”，不伪造绑定。其它OSError/权限/I/O/Timeout不当消失。枚举中目录消失须保留事件并在后续原定采样确认，不把部分枚举当完整容量证明。
3. 目录身份用dev/ino和真实目录/UID/权限保护；nlink等可变字段另记差异，不用完整bee_stamp相等断言替换。socket原类型/UID/nlink限制保留。
4. inode等身份变化不是天然正常：失效旧绑定，保存old/new字段；新对象存在时重走现有完整归属检查，成功才新绑定。过程中消失仅记生命周期，不继承旧FD证明；对象仍在却不能绑定仍INCOMPLETE/失败。沿用已有路径记录/events，不增通用状态机或账本。
5. 去掉每阶段“一份永久目录/两份永久inode”假设。每个直接子目录仍仅当前根PID的htc/cth两种逻辑端点；新目录/对象逐一fresh绑定，旧目录不背书。数量仍受TMP512项/16MiB约束，不扩大容量。实际未证明第二目录已发生；此规则依据独立归属准入，不假定重复对象安全。
6. 比较失败保存原始字段差异，不再只写“replaced”。缺少诊断字段不能成为合法性证明。

## 已绑定SDK ADB退出

73931先前已有SDK路径/映射文件/argv/UID/FD绑定。异常来自consumer_guard→sdk_adb_exception→details→child_args，不经过discover，因此新owned子进程特例没有接住。

仅对已存在精确sdk_adb绑定、当前快照PID/start/exe相符者：单PIDargs查询exit1且stdout/stderr双空，保留结构化cause，取得一次fresh全进程快照。只有PID完全不存在，才记录“已绑定SDK进程退出”并结束该核验；不注册owned、不授予信号。fresh rows交回后续消费者判断，不继续用旧快照。

PID仍在（包括僵尸）、换start/exe、fresh查询失败及任何非双空/非exit1情况仍失败；旧FD证明不替代fresh absent。新owned子进程保留原规则，两调用点各处理明确身份语义，不吞所有ChildArgsProbeError/ps失败，不循环重试。旧73931缺紧随失败的fresh absent，不能事后删错。

## 源范围及验证

只改bee_stamp/bee_ipc_entry/bee_fd_binding/bee_ipc_snapshot及受限目录枚举、sdk_adb_exception与fresh快照回传、monitor/closure结果分类、restore_all/archive_and_restore恢复门和tick。编译解析、测试断言、Unity命令及原子转移算法不改，不重写全部runner或加通用框架。

closure不再要求历史monitor错误或IPC诊断为空，仍需本次消费者核验成功、无活owned/未决身份、root已退出。原错误保留供最终验收。恢复tick继续核消费者、目标/前像、空间及恢复预算，不因旧TMP瞬时差异推翻无消费者证明；原swap/exclusive转移、冲突保留、未知现值不覆盖不变。真实恢复目标或证据写入错误仍停止。

一次相关离线回放，复用177及后续有效历史证据：185—187消失；两种精确拼写；目录仅nlink变化；inode变化须重绑/未知拒绝；两次Bee调用及同/不同目录独立绑定且容量不变；链接/错UID/未知类型/权限/I/O/Timeout拒绝；SDK双空+fresh absent与仍在/重用负例；新owned规则不扩；历史IPC差异不阻止fresh安全恢复，当前消费者/前像冲突必须阻止；12程序集/89结果缺失拒绝。只用固定事件fixture，不跑新socket探针；仅回放受影响链及新增例，修复后最多再跑失败相关项，不重复旧完整套件。

白名单为新RES-COMBINED-V07/run下runner.py、inputs.json、preparation.json、replay-check.py、replay-results.json及原有限activation/证据槽；E/cache/AS/TMP独立，旧源/数据/证据只读。恢复收件、相关回放及GitHub新exact head独立源门完成后，中央才另行派发唯一C。机械总量≤900秒、原分项上限、重试0、无KILL/无新增信号授权不变。本设计轮不native。
