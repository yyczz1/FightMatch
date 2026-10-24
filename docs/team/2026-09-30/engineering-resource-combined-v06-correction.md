# RES-COMBINED-V06 最小纠正方案

中央唯一收件；本轮仅诊断已发生失败，不构成代码审查或新native授权。保留V05 FAILED及原证据。

## 已证根因与有限修正

E5=`TestArtifacts/FightMatch/RES-COMBINED-V05/run`；C actual `01a11969-fe8b-7820-968d-2862da831a28`；receipt75895B/SHA256 `0e5f6ea7c43d8c3d54aa65556e6e9033787c70a119953e3027eee9483c8b5c1c`。

process-events.jsonl第192/203/207/211/218行的bee_fd_observation均为：目标 `/private/tmp/fm-rcv5.ShfjQbfN/7b3pcjrh.bvl/ipc_68929_htc`；Editor68929的原始lsof n字段为 `/private/tmp/fm-rcv5.ShfjQbfN//7b3pcjrh.bvl/ipc_68929_htc`，并同时出现对应cth。每次lsof exit0、parseResult=false，身份复核完成，before/after lstat相同（uid501、dev16777232、ino34708094、mode49645、nlink1）。后三类后代候选中69049无目标路径，根Editor已提供双斜线完整路径。此证据直接解释Bee绑定失败，不需要地址匹配或更宽进程筛选。

最小差量仅为两个有限完整拼写：给定已核真实TMP、已核直接子目录及当前阶段根PID端点，构造 `str(TMP)+"/"+dirname+"/"+endpoint` 和 `str(TMP)+"//"+dirname+"/"+endpoint`，逐字匹配n字段（保留已有明确STREAM后缀规则）。第二项只多TMP与直接子目录边界的一条斜线。不得使用normpath/realpath、去重所有斜线、basename/前缀/子串、..或.归一化；其它位置双斜线、三斜线、错误根/目录/PID/后缀继续拒绝。

PID/start/exe/argv/cwd、FD/type=unix、UID、socket类型、前后inode、阶段和端点数、目录/链接/替换、消失及进程恢复保护全部保留。原始n字段保留，不改写证据；记录匹配的是标准或单一额外分隔符拼写。不能用这次解释把旧V05改为PASS。

## PID69374不是仅已解决pending

第229行02:51:37.477342Z：pending_identity，ppid68929，start=Thu Oct 8 10:51:37 2026，快照exe=`(adb)`；查询 `/bin/ps -ww -p 69374 -o args=` exit1、stderr空。

第230行02:51:37.477474Z明确产生monitor_failure，phase=natural-closure、scope=consumers；第231行monitor_cycle保留同一错误。第232行02:51:39.710044Z为pending_resolved/resolution=absent。它证明后来快照中已消失，不能补出完整argv/cwd/executable身份，也不能证明实际是哪一种ADB行为。

最终pendingIdentities为空、closureClosed=true，但process-after.monitorErrors仍含这条错误；runner的最终门要求monitor_errors为空，因此这是实际新增且保留的验收失败，不只是历史pending提示。V05共有5条Bee错误+1条该进程错误。不得删除V05此错误或扩大ADB授权。按中央本次明确的竞态边界，同包实施以下有限生命周期处理；不能据此追认旧结果。

## 新发现子进程退出竞态的有限处理

仅适用于已经核对owned父链下新发现、尚未取得完整身份的PID：单PID `/bin/ps -ww -p <该PID> -o args=` 明确exit1且stdout/stderr均空时，不立即写不可撤销monitor_failure。先记录pending及结构化失败，再在同一原预算内取得一次fresh全进程快照；只有该PID确实不存在，才记录transient-child-exited生命周期事件并结束此pending。保留初次row、父链、probe argv/exit/两输出、时间和fresh快照证据，不注册完整owned身份，不授予信号权限。既有进程/文件恢复和闭合成功条件不变，后续守卫使用fresh快照，不靠旧快照宣称clear。

若fresh快照仍包含同PID（包括同start/exe、僵尸或换start/exe），或该快照查询失败，则本特例不成立，保留失败/阻塞；权限、I/O、TimeoutExpired、非1退出、非空输出、其它命令失败均走原失败路径。不按`(adb)`名称豁免，不等待循环重试，不扩大到任意瞬时身份错误。

最小结构化变更位于单PID args查询及discover的catch分类：保留真实returncode/stdout/stderr/argv，避免解析人类错误字符串；只在上述精确分支新增一次fresh确认及生命周期事件。V05旧cmd错误文本仅保存exit和stderr，未记录stdout；因此不能声称V05已满足新的“两个输出都空”条件，更不能事后删除其第6条monitorErrors。该特例的接受必须由新结构化证据证明。

## 自然窗口与下一步

第195行closure_begin=02:51:20.701607Z；第251行natural_grace_elapsed=02:52:16.106468Z，约55.405秒后正常转换；第252行term_window；第257行02:52:19.656908Z closure_end，remainingOwned=[]、pending=[]、closed=true。本轮monitorErrors没有TimeoutExpired或deadline错误：此前2.5ms自然边界超时本轮未复现，已有额度不足提前转TERM逻辑无需因本包重写。其它真实超时仍失败。

实施白名单仅新 `TestArtifacts/FightMatch/RES-COMBINED-V06/run/` 下runner.py、inputs.json、preparation.json、replay-check.py、replay-results.json及原合同有限activation/证据槽。复用V05原源/data及上述原始事件作只读输入；产品/包/89测试、43项恢复不变，生成state实施时fresh封签。新E/cache/AS/TMP独立，GitHub exact head审查后由中央另行交唯一C；本文件不启动native。

只新增相关离线用例：标准拼写仍通过；真实192等行双斜线完整路径可绑定同一已核端点；cth同规则；错误位置/三斜线/点段/错误PID或类型仍拒绝；前后身份变化仍失败；旧69374事件回放保持原失败；新增结构化exit1双空+fresh absent仅记生命周期，同PID仍在/重用/僵尸、非空输出、权限/I/O/Timeout及fresh查询失败均拒绝，无完整身份不给信号；自然窗口无足额探测时仍正常转TERM，真实TimeoutExpired仍失败。保留177及后续有效历史回放，区分复用与新执行，不重跑无关套件。机械总量≤900秒、原分项上限和重试0不扩大。

本轮未启动Unity/探针、未发信号、未改源码或旧证据。Bee路径根因已定位；新包同时落实有限退出竞态处理。PID69374的完整历史身份及stdout仍缺失，原FAILED不变。
