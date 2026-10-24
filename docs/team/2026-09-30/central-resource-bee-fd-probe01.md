# RES-BEE-FD-PROBE-001 — 单次本机表示能力核对

2026-10-08。用户继续任务和普通技术诊断授权内；C 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local唯一执行，Astra/xhigh，中央唯一接收。不是Unity重跑或新产品代码交付。

依据：engineering-resource-combined-v05-correction.md，6823B/ad27e6183508b064447374ae34043bcadb00c0073076c4d440915106d32123d2。V04真实失败没有原始lsof输出，暂不猜修FD解析，也不启动V05 native。

只做该文的自有AF_UNIX/SOCK_STREAM三阶段元数据观察：同一Python进程创建两监听端；分别connect/accept、保留监听端；关闭监听FD、保留已连接双方。新/private/tmp/fm-bee-fd.<8>本人0700、无链接，直接子xxxxxxxx.xxx中仅ipc_<本探针PID>_htc/cth。lsof只查自己os.getpid()，严格argv [/usr/sbin/lsof,-a,-p,<PID>,-U,-Fpcftn]，固定lsof470608B/deff85c7b4750a948a69fcd48eae3b394a37575e5048e5eb4a35b7a0482c3dc8。不得扫描他人、启动子服务或监听TCP/UDP、接触旧TMP/P/K/BC/AS/Unity/玩家数据。

中央批准的唯一调整：通过一次inline标准库Python命令完成，不落可执行probe.py；将该命令源码及SHA、actual/PID/工具身份/平台、三个snapshot的精确argv/时刻/exit/原始stdout/stderr、socket路径lstat和FD角色/fstat、完整运行时钟/结果/清理写入唯一 TestArtifacts/FightMatch/RES-BEE-FD-PROBE-001/probe.json。目录必须fresh，结果≤64KiB；这是本机观测记录，不改产品或已审runner。严禁解析后丢弃原字符串。

一轮最多15秒、每次lsof≤2秒、0重试，预留自有FD关闭/清理时间。所有socket阻塞操作设置短超时。finally先正常关闭自身socket，精确核前像后unlink自己创建的两条socket，再删自身空目录；有冲突保留不覆盖。无信号/外网/下载/依赖/Git/旧证据改写。真实工具拒绝必须停止并回报，不换路线。

分别给出每个已知fd的t/n与预期全路径逐字比较，不用basename/前缀/子串放行。观察不到就报告NOT_OBSERVABLE，不自行引入libproc或放宽要求；Python结果不能声称证明实际Bee实现。中央接回后交设计者基于真实输出定最小差量。目标5分钟内交付；本次不跑离线套件/Unity/测试89，无需跨会话身份消息。
