# LOC M06 中断后的最小 SOURCE 后继提案

2026-10-12；仅设计，未启动恢复。交付 owner：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，actual `01a12bfe-c81c-7940-a85c-7ad023246dc3`。中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一收件。授权来源：中央 actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613`、人类“继续任务”item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`；本次只允许本文件。

## 1. 结论与证据边界

**不能安全地作为同一次 M06 剩余阶段续跑；提议 FIX05-SOURCE，之后另行签发一次新 M 执行。** 保留 FIX04 源码 ACCEPT；材料 M 尚未接收，B/L 不解锁。

中央提供：M06-resume1 已于 `2026-10-11T06:15:23.066145Z` HOST_STARTED/PID60792，原 LOC actual `01a12998-5f4d-7a03-83c3-c5670144e1cc`，工具 `exec-177da352-5c1b-4a9e-a39e-82082f70591f`。用户中断后工具 -1/102534ms、无 HOST_EXITED；不能换算为 DUT 退出码，也不能据此计算可再用的 797 秒。次日已超原 900 秒窗口，PID 缺席不证明全部子进程静止。13 GET 有 exit0/quiescent=true 收据，M4/W4 已存在；三次 restore 尚无完成证据。以上是中央输入，本轮未重复核查。

中央补充确认 LOC 唯一中断取证输出 [中断收据](loc-m17-m06-interruption-receipt.json) 已完成，actual `01a12bfe-0633-7001-9613-18780415e1a1`；本轮引用该完成通知，不重读或重做取证。13 归档共 9014649B 均 hash/固定身份一致，123 保护项未变；三次 restore 精确路径无收据/log/config/csproj/生成物，启动与完成仍 UNKNOWN，manifest/validation 缺席。中央原始检查确认 PID60792 缺席；另于 `2026-10-11T17:28:07.638624Z` 精确 M06 host/W4/M4 参数过滤为 0 匹配。这些不是 DUT 成功或完整后代闭合证明，原始检查完整工具 ID 待中央绑定，不能将 chunk ID 代用。

原执行合同见 [M06](central-loc-m17-m06-execute.md)，固定输入见 [FIX04 提案](central-loc-fix04-next-material-proposal.md)，既有改动边界见 [M05 后继设计](engineering-loc-after-m05-next.md)。M06 附录的“CreateProcess 前审批超时”恢复条件已不适用本次真实启动。

冻结输入：PR2 head `816263eaacf4f3e276c0cef7703c5824af086cc6`；F4 driver 132552B / SHA256 `8dd4d5daa8b46f2e1a2fdb076636a095e9c0b398a5f8ae9aff83ced8c3f8db6c`；source-freeze 2808B / SHA256 `9b5ea95beb2f148f6c3f99a55aff33df5bf3d192aa66403f7f7ba7d15213b7c8`；model `fefbc7a1002760a2231922069bd839864084421f888576e99a10518c5b7c6605`。身份沿用批准文件，非本轮重新哈希。

## 2. 为何不能直接重启

冻结 `m17_driver.py` 的 `MStage` 每进程新建 monotonic 起点与 GET 计数；`execute` 不提供持久恢复令牌。`execute_material` 要求 M 缺席，W 最多仅含许可归档；`MaterialWriter.put` 用 create-once，拒绝既存叶子。restore 的中间状态和阶段关联保存在内存，最终材料清单在流程末端才输出。新进程既不能证明旧阶段完成，也不是原限时执行的延续。

因此保留上述污染/创建门；不增加 checkpoint、幂等覆盖、目录清理或新执行框架，不复用旧激活文件。所有旧 partial 根及失败/中断证据作为只读输入保留。

## 3. 最小 FIX05-SOURCE 范围

根别名均为精确路径，不允许自动换名：

- F4：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix04`。
- F5：`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05`。
- W3/W4：外置 `ApplicationData/Tools/Luban/.work/m17-fix03`、`.work/m17-fix04`，均位于 `/Volumes/WD_BLACK_SN7100_2TB_Media/` 下。
- W5：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05`。
- M5：`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix05`。

以冻结 F4 模型为唯一基线，沿现有 SOURCE-only 映射方式，仅替换未来输出 E/W/M 及其派生路径；旧证据路径必须保留原值，不能全局替换破坏来源。保留原 SDK 声明清单，SOURCE 不做全量 SDK 扫描。

归档来源改为严格只读、无后备下载：31 个 nupkg 中原工具库 14 个不变，前次已复用的 5 个继续从 W3/acquire 读取，M06 新获的 12 个从 W4/acquire 读取；YamlDotNet tar 从 W4/acquire 读取。按冻结包 key 精确映射，不能将单一 REUSED_ROOT 一概改到 W4。以 LOC 回执绑定每个归档的原路径、字节数、SHA256、包 SHA512、固定 URL；Yaml 还保持原提交/paxHeaders 校验。任何归档缺失、身份不符、链接或回执不充分即阻塞，不补 GET。

将 Yaml 和新增 12 包的输入类别改为 existing，重建相关输入/命令/写集/封签。`download_contract` 及其验证断言必须一致为 **0 行、0 GET、0 下载正文 B**；执行命令表没有 GET，缺文件仍失败。保留归档身份先验、FIX04 已审双斜线规范化、路径逃逸/碰撞拒绝、31 包闭包、图/锁验证、三阶段 restore 和原进程监护。扩展现有保护输入，将 F4、M06 外层证据及 M4/W4 的精确状态绑定 LOC 回执；不把 M4/W4 声明成空目录。

未来 SOURCE 作者仅可新建 F5 下这 8 个叶子：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。差量用例嵌入 driver，不新增框架或测试文件。原源码/证据、项目文件、外置根、新 A/H 均不在作者写白名单。SOURCE 容量沿原 16MiB/driver256KiB；F5 若已占用即停止，由中央处理，不能覆盖或换根。

## 4. 待绑定字段与有限验收

中央签发 SOURCE 前，将已完成 LOC 回执绑定为准确输入：回执 bytes/SHA256、原始工具 ID；13 GET 的逐文件身份及完整收据；M4/W4/E06-resume1 精确叶子集合、链接状态和保护身份。以上逐项封签字段仍为 **待绑定**，沿 LOC 原始输出取值而不重复扫描。restore 启动/完成保持 UNKNOWN、最终清单缺席；原执行全部后代与相关写者是否静止须按回执证据界限表述，不能以单 PID 缺席、参数无匹配或空日志代填确定闭合。

未来 SOURCE 验收只覆盖本次差量：

1. 模型具有准确 31 包及 1 个源码归档，均为已绑定只读来源；与 F4 比较只允许本节列明的路径、类别、保护项及零 GET 差异，原依赖/补丁/归档安全语义不变。
2. 一次定向离线检查（提议上限 10 秒）：完整映射且零 GET；缺失/错误归档失败且无下载调用；占用 M5/W5 被拒；旧根不进入写集；原阶段顺序、总时限及闭包门保持。复用已审 66 项结果，不自动重跑历史整套或 SDK 扫描。
3. 一次 SOURCE-only 机械封签（提议上限 10 秒），仅产生上述 8 叶子；输出真实时间/退出码及各差量断言。超时/失败停止，不再跑第二轮；这些是未来提案额度，本轮未执行。
4. 按 GitHub PR Code Review 流程评审精确新 head，QA 核收差量证据后中央记录 SOURCE 判定。历史 F4 ACCEPT 不自动覆盖 F5；主程不自行批准实现。

## 5. 可单独审批的新 M 提案与后续依赖

F5 SOURCE 接收、LOC 静止/保护回执完成、全部归档有效后，提议一次新 M：新空 M5/W5，0 GET，最多原定 3 次 restore，总计不超过 900 秒（工作 870+收尾30），其余磁盘/文件数/子进程限制沿 M06 合同；SDK 新鲜校验仍计入该窗口。新执行需新的准确源码/模型封签、外层证据根及一次性 A/H，由中央另行签发，本设计不创建它们、不重新开旧 host，也不调整工具审批。

目前没有已完成 restore 证据，故提案保留三阶段原流程；这不是断言 restore 从未启动。若 LOC 回执证实已有可接受的完整阶段/最终材料，中央先评估可直接接收的证据并收窄提案，不能机械重复已接受阶段，也不能把含旧绝对路径的 partial locks/assets 直接当成新根输出。证据不足则新执行的必要重复应明确列入新授权。

M5 只是候选，需验证并接收才解锁 B；B 的 locked restore、两次 publish、build、官方测试/一致性沿既定合同，需独立签发，当前不运行。B 接收才进入 L 的 Program 身份、12 自检及两次 clean-pass；B-CATALOG 仍需真实接受的 LOC-A。保留资源 V08 有效证据及其他产品交付，不以本次恢复为由重跑 Unity。

本轮仅创建本文；未运行 prepare、源码测试、SDK 扫描、GET、restore、native、Unity 或 Git，未写源码/新 A/H，未跨聊派发。只读/方案限时 150 秒未守住：压缩后误回查历史 N07，计时检查已为 150.714 秒；当即结束新增调查并落盘已有结论，不声称限时内完成。无执行额度因此被重置。
