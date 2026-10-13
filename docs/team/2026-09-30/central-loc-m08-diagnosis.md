# M08 后的定向诊断：真实 macOS 文件规则

中央 actual 01a12e46-6af4-7b82-958d-d18c1146d85d / 当前人类 item 01a12e46-6b4a-7fb2-aea1-235e64f1268f。用户要求继续处理；M08 一次完整还原已真实失败，不授权再跑 restore。主程负责普通技术诊断，中央收件；保持 FIX06/M08 既有源码和材料只读。本包的微型诊断仅用于定位普通技术故障，不算新的完整还原，不扩展材料运行许可。

读取已存在的 M08 E08 receipt/stdout/stderr、W6首restore收据/日志，FIX06八叶、后置侧录、执行包及A/H；按需旧M07和FIX06设计/55项检查。首条5.987138秒exit1，host129.057504秒，0GET，244保护项未变，后两条未启动。失败准确路径 W6/phases/M-runner-generation/source/src/Luban.DataLoader.Builtin/obj/371f9ba5-1be1-4a9f-996e-d51404ed0ba5.tmp；真实profile SHA 5f769139403d229b1ba22378f979019f9a7ac9425c305c73b5e5b7bca3abb916。不能再次只靠 Python 正则匹配断言 OS 行为。

采用 diagnosing-bugs：先核对实际profile重建身份，给出3个可证伪假设，定位真实OS层反馈。允许一次合并的微型诊断，总调用≤20秒，最多6个隔离Python子进程，每个≤2秒（含收尾）；不执行SDK/dotnet/restore/Unity、无网络/下载。仅在新 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01` 下生成≤12文件、总≤128KiB，每次实际数据写入≤16B。按正常工具权限运行；若工具拒绝则保留证据，不绕过。

用固定小fixture对比实际生产profile的相同规则/语义与必要控制组：例如精确literal正对照、生产GUID表达、足以区分正则方言/转义与目录vnode权限的单变量对照。必须保留profile、原始errno/返回码/耗时及原工具ID，记录fixture路径改写及真实环境差异，不能把实验fixture当NuGet成功。先准备明确命令和检查再一次执行，失败后不自动增加运行。允许调用系统libsandbox做限定检查，但不修改系统或用户权限、挂载/ACL，不sudo、不取消真实执行器隔离。实验子进程只能写其新fixture内指定叶；所有SDK、M6/W6、E08和历史根只读，不重枚举partial，不扫描SDK，不ps。

唯一报告写 `docs/team/2026-09-30/engineering-loc-after-m08-failure.md`，尽量简洁：已证实原因、仍不确定部分、为何55项离线检查漏掉、最小修正建议及必要真实回归。若新源码修正必需，列准确改动函数和允许文件，由中央随后派LOC，主程不修改产品driver。报告此轮实际身份。只在本线程完成，由中央wait接收，无跨聊消息/Git写入或审查代理。
