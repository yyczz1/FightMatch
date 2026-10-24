# RES-COMBINED-V08 一次受限 I→T89

2026-10-08。唯一 C 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh，fresh actual；中央唯一完成收件。用户继续任务及限定验证授权内。仅中央明确派发且 resource-combined-v08-fix01-source-verdict.json 为 ACCEPT 后执行；必须匹配实际已审 head、真实完成 URL 和以下FIX01五叶（已审head a360ad864171a3b49f7785ab7c797028d583d5c0，结果comment6054341623已由QA接收）。未满足时不激活。V07 FAILED/T0 与已接收恢复保持原记录。

继承 central-resource-combined-v07-execute.md 的未变操作、历史 ADB、恢复及真实 OS 观测约束，本页覆盖任务、源码、新根和当前 state。可一次批量读必要字段≤6KB，不重复整套预检、离线回放或已核目录扫描。

唯一新 E=TestArtifacts/FightMatch/RES-COMBINED-V08/run：
- runner.py 114245B / f5801212606db11dbf9bef5452b071c8b612c4a282ec1039af425abb154f6da8
- inputs.json 1254426B / c20441f395bc7479484aec9436732280a8c68b6303797eb39f3a302b08249d19
- preparation.json 15856B / 025e9b4bd925a4cdf6a179da3a5d6825a56aa152d0f49b0b5b2ae848bbda7b5a
- replay-check.py 92286B / abcc293dfd479486d5c3f6a1aa7816bb8c40d90adc769299ad2c124f40942f6a
- replay-results.json 130081B / ba0fafb9f81e59d98aa9b1555004e699bc0b32a5bf42f18b0f531c3fd4f04a2f

P/K 不变。BC8/AS8 精确按 inputs.paths，必须不存在；新 /private/tmp/fm-rcv8.XXXXXXXX 为空、0700、canonical 无链接。按已审 activationContract 写本次 activation，绑定 fresh actual、五叶、sourceReview、stage/environment 和真实 TMP 身份。DOTNET_EnableDiagnostics="0"。Tundra 原位136080B/a2a3f660349eb118ed1071c7a2d1909d91075878dff4490bded2c2edd3e3ab4f；bee_backend.info127B/cd510c4af398ab352e5bd1fdb74a06658814c4e8da952cb8c068909ac45e87d1。漂移停止，不能改实物或临时重封。

补充历史 SDK ADB：V07/process-after.json 记录 PID82386/start Thu Oct 8 13:34:20 2026，UID501，argv [adb,-L,tcp:5037,fork-server,server,--reply-fd,4]，SDK txt D0x100001c/inode8166469；log device16777232/inode34744928，原 TMP /private/tmp/fm-rcv7.ewpPfmSh。旧记录不证明仍存活。沿已审规则 fresh 核对实际身份/FD或确认缺席；不因历史记录给未知进程例外，不杀 ADB。

V08 仅执行已审的结构化 argv/cwd 采样，以及已核 owned 父链下新未注册 PID 的一次即时全快照缺席确认。严格只限源码允许的缺字段结果；仍活、zombie、重用、矛盾、探针错误/Timeout 都失败。不完整身份不注册、不发信号；已有 owned、SDK 和消费者闭合规则保持。旧82401具体缺项未知，不能追认V07通过或现场改判据。

一次 argv 调用 python3 -B E/runner.py <activation_sha> <fresh actual>，cwd=P。I 一次≤360秒；所有门通过才 T89 一次≤180秒且不加-quit。总900秒含160准备，natural60/TERM30/restore60/final30服从剩余。每 owned PID 最多 TERM1、无KILL、无重试。外层记录 monotonic 开始→真实 subprocess.run 退出→结束、runnerSeconds、+160 totalSeconds、实际OSexit及工具原始输出；内部计时不能代替。

通过要求 AWAITING_PROCESS_EXIT、validationStatus TEST_PASS、无failure、receipt准确封签、真实OS0及外部总≤900，同时12程序集本轮来源、89准确Passed、AS真实I/O/symlink、P1035/43compiler恢复、shared/cache/旧证据保护、fresh消费者闭合全满足。原56/56及FIX01相关15/15仅是各自输入的离线证据。失败保留原始原因，按既定安全恢复收尾，不能原样重跑。

仅 inputs evidenceSlots/transferPlan、新activation/TMP/BC/AS写入，无产品/依赖/安装/下载/LOC/Play/Android/Git。工具实际拒绝只停止受影响动作，不换路线。回执给actual、activation/receipt封签、真实外部时间/退出、I/T/12/89、首异常事件和恢复状态；不重复runner已做的全目录验证。final留当前会话，中央wait收件，再交QA一次有限证据接收。
