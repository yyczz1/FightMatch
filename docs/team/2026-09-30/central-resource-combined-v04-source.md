# RES-COMBINED-V04：Bee IPC 与自然闭合边界源码纠正

2026-10-08。用户已要求继续开发与限定验证；中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local唯一收件。源码唯一作者01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，Astra/xhigh，fresh actual。系统设计的调查已结束；本包接收其最小方案并直接交原作者实施，不另开长设计轮。

## 固定输入及结果

- 设计 docs/team/2026-09-30/engineering-resource-combined-v04-correction.md，5818B/SHA256 d82090d30e162c345867d32bd854e43036ef722c024773079168f1ede58e81dd。
- 基线E3=TestArtifacts/FightMatch/RES-COMBINED-V03/run；runner83629B/c73a2214edb3c7359bbaac66ed01b51503500b66c3959025dae9a480d1a27dcb，inputs1241674B/dca26616f624469bbd5639b659e3a161eb55c56cd2ec117f3ec4c864e4aedd08，preparation88412B/184db403376a8a01ebb41f7c924ba80fd30701a9b3bcc58db90c00dfe2d7a504。checker111261B/1f16c745c82b0373483ed8f029a6831345c840f12e01cbfd05b0791a1fca8f09；results451171B/ef6b9626be51273a64770d169beedc2a1eb39f5cd5acb1ac10e9b0986d00b556为原V02的177项历史，不得误报为V03新运行。
- PR20 reviewed headdbd5a203fd7b4997a1e67d7269fb1d9b87d5cbea，GitHub6049807634仅源码接收；V03 actual01a118fa-4774-77f0-95b3-2760ea765e6c native FAILED已独立核证：I1 exit0、runner exit1、T0，96.326948秒/含准备256.326948。见 testing-resource-combined-v03-native-receipt.json（8735B/097e3ea1233a3258da2e3da63ce1557a12753047c4c840186012c9576b6d63a6）及中央native verdict。

## 白名单与变更

唯一可新增产品外源文件：E4=TestArtifacts/FightMatch/RES-COMBINED-V04/run 下 runner.py、replay-check.py、replay-results.json、inputs.json、preparation.json。该目录必须fresh。本包不创建activation、BC4、AS4，不激活native；设计中执行槽许可仅留待之后单独C包。测试自有临时沙盒沿已有回放方式、按范围清理，不碰旧TMP。

只实现设计两项：已观测Bee两端socket首次fresh类型/目录身份/固定Editor与Bee后代FD来源验证，有限阶段状态及消失边界；正常natural窗口耗尽的专用控制状态，保留in-flight真正TimeoutExpired/I/O/工作与总预算错误。不要按文件名放行未知IPC、放宽现有非Bee检查、删除诊断开关、改监护总框架或增加信号权限。新增小helper/状态与现有tree_entries/monitor/closure/probe_timeout及直接所需接线可改，其余函数保持并机械报告精确AST差异；必要越界先报具体原因。

inputs仅改任务/新E4-BC4-AS4-TMP4/输入自封签、真实owner/来源、必要Bee合同及固定工具身份、设计引用、准备时重新核实的单Tundra状态；其余产品/包/测试/传输/恢复/命令语义不变。禁止读后自动接受执行时漂移。保留160准备/900机械总预算、0重试、I/T限定、外部真实OS退出验收。复制正确activationContract与completionObservationContract，sourceIdentities/实际回合/五叶封签完整，避免V03草稿漏项重现。

允许针对设计已知固定二进制/两个state做只读身份核对；不再研究开关/引入新工具。源码/数据差异、未改函数、哪些177用例复用及哪些受影响用例本次执行，写入preparation，不复制巨量历史全文；本轮回放结果必须准确绑定V04。

## 验证与交付

先完成源码/静态检查，再沿现有回放入口跑受影响链及设计列出的最小新用例；最多两轮，每轮30秒/合计60秒，第二轮只能因第一轮真实失败修正而追加。原177历史保留且逐类说明复用边界；不能简单改数字称全通过。不得运行Unity/Editor/SDK/实际Compiler、下载/安装、改产品/Packages/ProjectSettings/meta/旧证据/Git/LOC；不碰真实套接字、不发信号。

验收：合法Bee两端/顺序/消失可观测；错阶段/PID/start/工具/FD/目录/类型/UID/inode及未知IPC拒绝；自然边界可转TERM而其它错误仍失败；未闭合仍失败且保留先前错误；新state精确绑定且漂移拒绝、43恢复不扩大；原编译来源、89精确case、真实AS I/O/symlink、缓存保护和退出合同不弱化。作者只报告SOURCE_READY，不自批。中央核差异后推PR20新head并收GitHub，届时再安排C，当前native=0。

每条工具输出≤2KB，读大JSON只抽字段，不打印完整inputs/preparation/历史；结束给actual/5seal/具体变化/新检查耗时和复用范围。中央用完成事件接回，不发跨会话身份回执。
