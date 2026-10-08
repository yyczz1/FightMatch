# V06-RECOVERY FIX03 — 保留首个失败并发布真实合同路径

GitHub head6002d2c10509ecd17e6258f49d2c32edf86094ba新P1 4214565396/4214565407。真实恢复仍0。前者为stdout摘要漏原result.failure；后者为审查分支没有本机真实存在的canonical addendum路径。中央将把实际docs/team/2026-09-30/central-resource-combined-v06-recovery-fix02.md原字节发布到同一Git路径（与已发表recovery-v06/fix02-packet.md等字节）；无需把本机有效参考改成仅远端存在的路径。

原作者Luna/medium，先保存recovery-source三叶到fresh recovery-source-before-fix03。仅在recovery.py main的summary字典增加failure:result.get('failure')，保留receiptFailure独立字段、暂态合同、退出码及预算等所有其它代码。plan只更新scriptIdentity，completionAddendum不改；source-receipt准确记本机原文件存在、中央负责等字节发布及本轮差量。不能声称已经远端发布，中央随后核实。

一次≤10秒内存定向检查实际main末段：原恢复失败+收据写入失败时stdout同时保留两个原因；created=false的前检失败同样输出首因；正常暂态不会假报failure。可截取实际AST末段注入假时钟/writer与捕获stdout，不能复制一套摘要实现来测。其余函数AST不变，原66恢复清单/前像不变；不重跑旧7/4/2测试、不实际恢复/ps/lsof/Unity。计划2分钟交件，中央推Git补canonical文件并复审，未过门不执行。
