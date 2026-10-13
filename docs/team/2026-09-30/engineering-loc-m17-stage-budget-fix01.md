# LOC M17-FIX01 — M 阶段总预算

2026-10-04 · SOURCE_ONLY。中央本轮明确要求M先独立收件、整个阶段≤15分钟，B/L后续另签；沿人类授权 `central-resume-scene-loc-2026-10-04.json`、AGENTS §6及[M17合同](engineering-loc-m17-clean-source.md)，不改变版本、材料、许可或原验收语义。issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一作者LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh，登记实际新turn。

## 固定输入与范围

旧E17=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17`八叶已封存；driver104059B/`203be2d994db81da1722f5334cd1ac49b383b5aafaa64695f7325a6b3addc6f0`，freeze2765B/`d572872d038a19418c75c776cef3ae7299616a69118b6a63619c774f0b2fb244`，receipt14135B/`33d530b4fde0eecee7971d1832d48c8763409dd99338ed4bd95324218cdd0731`。旧42→50两轮原件及其余旧22叶保持零写。

仅新E17F=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix01`，首次写前ABSENT；与M17相同八个相对叶名，driver仍`source/m17_driver.py`，总≤16MiB/driver≤256KiB。实际源码增删≤300行，仅预算、授权绑定、对应纯检查和必要新封签路径/身份适配；缺范围先报具体差量。未来材料M/.work/m17路径不变且不创建；共享、工具、历史、中央/主程索引零写。源阶段0真实外部执行、0下载/解包/restore/子进程/Unity/Git。

## 唯一纠正与验收

1. 现`execute`已仅允许M；保留此边界。`maxCumulativeCommandSeconds=23340`只是M/B/L逐命令预算的总数，不可再作为M总计时器。新机器可消费的M预算固定总墙钟≤900秒，包含生产入口源消费/输入完整核验、下载、内存解包、物料写入、三个restore、最终核验及收尾；为有界终止/失败回执预留时间，工作截止不得吃掉收尾余量。B/L保持独立激活/独立验收，不由本入口启动，也不从它们借时间。
2. 使用同一个monotonic阶段截止，贯穿所有工作/阻塞外进程。启动前检查剩余额度，每命令的等待上限取原命令上限与剩余工作额度的较小值；纯处理/核验也须受整体限时约束，不能只在下一次写文件时才发现超时。单命令125/180秒、retry0、三个既定restore及现存写域/资源/子树规则保持。时间耗尽/输入错误/外进程错误立即停止后续动作；仅对本次所有进程按既有有界方式收尾，保留真实失败和已发生输出。失败回执不能因工作deadline已过而完全丢失，也不能在无限宽限中补写成功。
3. 独立可信中央pin绑定stage=M、准确预算/执行源/模型/审查head及固定下载集合。18个official-get上限共25172764字节（17 nupkg＋Yaml tar）；实际缓存可减少GET，不能多取/重试/替换URL/hash或从B/L引入动作。按实际归档响应体字节记录总量，不把TLS/协议总流量伪报为已量测；固定逐档长度/hash与总量一起验证。预算缺失、扩大、stage混用、caller替换应在外部动作前拒绝；生产pin仍null。
4. 对真实生产消费函数接入纯进程内可控clock/runner seam：正常M、已有耗时扣减、正好截止/超时、输入核验或解包阶段超时、当前子命令在剩余预算到期时终止且不启动下一命令、失败回执保留、18档/总量越界、缺/伪造/扩大pin、B/L拒绝。可记录模拟行为，严禁声称真实下载/子进程/restore已验证。复用未改检查，新增检查必须确实调用预算消费逻辑，不能只核字段存在。最多3次完整源检查累计≤60秒，保留每次真实原始结果/完整耗时；不得机械重跑更早历史23/46/25。
5. 新八叶生成→落盘封签→同消费入口再核，hash分层无自指；保留旧30叶身份。交精确M17→FIX01差量、全部新增检查、真实准备耗时、实际owner与未来M预算。结果仅SOURCE_READY_NOT_MATERIAL_READY，pin关闭/execute未授权/未review；回主程交中央同PR2最终新head。此包不激活真实M/B/L。
