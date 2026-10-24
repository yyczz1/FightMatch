# RES-COMBINED-V08 SDK ADB后续方案

中央收件；只诊断监护缺口，不是重复代码审查。本轮不运行ps/lsof/信号/Unity，不改源或输入。保留原runner FAILED，优先复用已经完成的12程序集来源和89测试。

## 原始事实及可证根因

E8=`TestArtifacts/FightMatch/RES-COMBINED-V08/run`；C actual `01a11a65-5b5a-7ac2-a8bf-fdcbec225ff1`；receipt78531B/SHA256 `2e8e402edee1b8d6f70481ecac71e277eaca4a38fddbc4ec794a6fcf36b3efb4`。源门为PR20 head `a360ad864171a3b49f7785ab7c797028d583d5c0` / GitHub6054341623；实读runner114245B/SHA `f5801212606db11dbf9bef5452b071c8b612c4a282ec1039af425abb154f6da8`。

已绑定SDK ADB只有PID89440，start=Thu Oct 8 15:26:18 2026，uid501；九次sdk_adb_verified从07:26:19.363905至07:26:36.584910Z，固定SDK映射文件及本轮日志FD均已记录。I根89375；311行07:27:34.811605Z闭合成功。T根89983于317行07:27:45.907200Z启动。338行07:27:56.387203Z首次报More than one current SDK ADB server，随后同类14条；最终sdkAdbBinding仍是89440。

代码中的直接原因：sdk_adb_exception第489行发现旧绑定PID不在rows时没有退役绑定；第510行将每个当前候选与这个历史单例比较。isolate_stage也保留sdk_adb。该报错条件是“候选PID或start不同”，不是“同一fresh快照存在两个有效SDK服务器”。双空args后的退出确认也未将旧绑定退役。这是明确的生命周期缺口，不能靠阶段切换无条件清空绑定修补。

实际T候选PID、start及FD未在该前置拒绝点记录；events无完整freshSnapshot或SDK退出确认。故不能给出第二PID号码，也不能证明两个服务器同时存活，或证明T候选合法。已有“多PID”说法须收敛为“当前候选与历史绑定不同”；不能据最终进程为空补出当时身份。

## 有限修正

只改sdk_adb_exception的绑定退役/候选基数判断与有限诊断，不改ADB启动/关闭或信号权限。把历史绑定保留在事件中；只有完整fresh快照明确旧PID不存在，才退役当前绑定。旧PID仍在、僵尸或被复用为不同start/exe时仍阻断，不能将它当退出。发生args双空exit1时也必须沿用一次fresh absent证明，然后退役；查询错误仍失败。

随后仅允许至多一个非基线、非原豁免的当前SDK候选，且必须重新通过原全部argv/UID/固定映射文件/FD/当前日志记录/时间/无源码或编译FD/前后身份检查。旧绑定不为新候选背书；两个当前候选仍失败。不能单靠新PID、adb名称或T阶段开始放行。涉及日志时不能把I阶段旧startupHeader当成T候选证据；本次T原串缺失，不据此推测或放宽日志规则。

在判定前保存stage、root、旧绑定及其当前row或明确absent、候选PID/start/exe集合、快照时间；完整核验仍保存现有原始FD。只保留受限相关记录，不增通用进程框架。源修改必须另经GitHub对应exact head审查，本设计不自审批准。

## 保留产品证据，先做短监护验证

本轮I已COMPILE_PASS、12程序集来源完成；T实际XML89/89 Passed，I/T Unity exit0。QA独立核验这些事实及恢复，分别形成“编译证据有效”“测试证据有效”“监护仍FAILED”的收件，不覆写原receipt，不把它们冒充整轮PASS。

固定引用：compile.json11303748B/SHA `e47863ffcf925023cfba5330d45cb56df8c17afa0c7569aa73bdf8521f270c87`；T/results.xml56396B/SHA `b535cbfa2df56772657f07ff744a672848abbdd9d243b7dc48cf0fe26bba6720`；process-events502540B/SHA `11365b58467ac8867d3444a7a4cb59d2a7ef7a79685a961221284b91e834a4e9`；process-after278604B/SHA `325ce60fbad8712a7a55b44524980da62a20aeb0cd954ebb174ea412b07dadbc`。本轮实际164.364641958秒，加准备160为324.364641958秒，不能通过新验证改写该边界。

建议后续唯一小包根 `TestArtifacts/FightMatch/RES-COMBINED-V08/adb-followup`，白名单仅受限修正源、replay-check.py、replay-results.json、receipt.json及固定fixture；原E8只读，不建立V09或新E/cache/AS/TMP，不重新同步/暂移源码。

一次≤30秒离线监护回放，最多再修复重放失败项一次：已验证I绑定→fresh absent→单一T候选完整重绑；I绑定仍活+新候选拒绝；同PID复用/僵尸拒绝；双空查询后fresh absent退役；查询失败/缺FD/旧日志冒用拒绝；阶段切换本身不得清绑定；原14条历史错误不删除。固定原记录用于历史部分，补入的T候选资料必须标为合成fixture，不伪称本机观测。该预算足够验证状态转换，无需启动Unity、SDK服务或读大缓存。

回放和独立源门只能补证明修正后的监护规则，不能补齐旧T候选的历史FD身份。中央根据QA分项收件决定剩余验收边界；若另要求本机证明后续SDK换代，应单独固定一次短监护观察任务，禁止把“当前已不存在”当补证，禁止默认再跑整套I/T。没有新的产品输入变化或编译/测试证据缺陷，不因本监护修正重复12程序集/89测试。原P1035及43项恢复结果继续保留，不再触碰恢复状态。

## AS06/AS07证据解释补充

已核固定源码 `Assets/Tests/EditMode/FightMatchHost/FightMatchResourceActivationStoreTests.cs` 36336B/SHA256 `572c46a5c37734fe6cb8db4ec9b3a0c0c133ce7284ce6e0458451e6bcf9cb029`，与V08 inputs及cases合同一致。cases为24448B/SHA `d53c49167d522532a6cc87218bfa5e961ab3a096af06209fb694726fca2ed0e8`。合同asIsolation.teardown明确要求RealArea.Dispose白名单清理、最终lease为空。因此空目录是预期后像，不能据此认定AS未执行；这里不是NUnit TearDown方法，而是using退出时调用Dispose（源码189—207行）。

AS06（381—409行）使用实际SystemActivationFiles/default store，读取active/prepared/restart/backup实际字节，检查替换/重开结果、白名单及无tmp残留；下一process token是注入，不能证明真实进程重启。AS07（416—437、463—481行）在Mac分支断言原生symlink返回0；对存在目标和悬空目标分别执行Read/Enter拒绝及外部哨兵未变断言，finally仅删除精确链接；缺API或建链失败必须Fail，没有mock/skip通过分支。AS08另断言三份保护哨兵保持（488行起）。

本轮XML308/311行分别为AS06/AS07 Passed，duration=0.824151/0.058333秒；结合已核源码及本轮测试程序集来源/平台定义，可作为这些实际断言通过的证据，包括真实文件路径和两种真实链接行为。它不是额外的逐系统调用审计，也不能补造未留存的IO/链接日志。AS06源码写有Progress消息但现有T日志未见，不影响已执行断言的结果。

合同freshEvidence.execution要求AS I/O/link evidence，asIsolation把具体要求落实到上述真实测试及空lease；未要求另留独立IO或symlink系统调用日志。请QA按“固定源码/本轮程序集与UNITY_EDITOR_OSX分支绑定→对应Passed XML→Dispose预期清理及实际空lease”补充证据对应说明，复用已有12来源证明；若平台/程序集绑定确有缺口，只列该项，不凭空目录新增重跑要求。现有证据支持合同测试范围内的真实IO/链接分项验收，不支持合同外重启或文件系统审计结论。整轮监护FAILED仍保留；不改QA原始收件，由QA追加解释。
