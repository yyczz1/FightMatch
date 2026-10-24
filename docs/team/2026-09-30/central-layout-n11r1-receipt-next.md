# N11R1 收件及限定后继

2026-10-06，中央。唯一产品集成判定继续为 **NEEDS_FIX**；本轮诊断分类 **UNKNOWN**，不改写旧回执、不撤销GitHub PR4原P1。

## 已接收事实

- C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local` actual `01a10f18-13cd-7933-b4a7-8106add4caef` completed/停写；主程收件actual `01a10f1c-8b22-7b70-b74b-14bf40047bd3`。实际命令 `exec-52bb1cee-7765-4f57-98e8-aaf297590f46` 获准，wrapper84.328秒/exit1；Unity一次22.315279625秒/exit0，retry0。
- 根目录 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1/`，18叶2230004B冻结。`PExecution/receipt.json` 390157B/SHA256 `2b14702e26abaca73b22644635abea6fe75aa1ad1de1df5364e29697ea87405d`；`P/native-probe.json` 369276B/`25fbc18732fe9674b84640acc00ee403ec0c523658ae04fe68e422be6b564875`。中央已实际读取并核二者身份、计数及分类。
- 五观察完整，native无异常；Open1、Save1返回True、Reopen1、Close2、marker1、自然update4。内存27→106→106→106→125，磁盘22→22→106→106→106；五项在冷载内存及保存后磁盘重现。
- marker真实落盘且复开保留。两Scene后像27798B/`69a927c8f43eb1612f0b3069accb726833c4849a76fe8a5bef6041157cab7e4b`；只在内存剥离marker三片段875B后26923B/`71bf02138c95154f743b27d302d156fb25ba3776cd841f86676e15d677f811ad`，既非原27也非冷22。已数出79条额外修改，不代表其它字节全部解释。
- Host.Editor实际Csc；新诊断DLL229376B/`21559b1722333367ef3ad3c1efd0ab136de805afb27e5cf504f8b955aa6b1983`。13 owned闭合，VBCS21186 TERM1；没有产品导出、测试、Player功能或设备验收。
- 前任与本轮临时Scene已归档后删除。两个meta导入后增加Importer字段，触发“首次59B必须等于最终身份”清理守卫，留下源码和两个meta。旧三保护布尔仍null；主程另核原输入未改，不能称整个投影干净。

## 后继A：仅清理三个已归档临时文件

交付者复用唯一C，新实际回合；主程唯一接收。可与下述只读分析并行，两者不共写。模型均Astra/xhigh。

允许C只创建 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1Cleanup/receipt.json`（≤64KiB）；该目录必须原本缺席。删除范围精确为投影 `TestArtifacts/FightMatch/LAYOUT-S1-001/projection/` 内：

| 相对路径 | 最终归档字节/SHA256 | inode / birthtime | 恢复归档（N11R1内） |
| --- | --- | --- | --- |
| `Assets/Scenes/FightMatchCanvasPersistenceProbe.unity.meta` | 155 / `a9c4675d97e3764f91fa62e47d0498a79a101e1456019e6e298d8c8407d5241c` | 10919268 / 1791254909 | `P/scene.meta` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchCanvasPersistenceProbe.cs` | 18436 / `18307351e47024cce5fb80943818764968877d1a7096283f5461fc813041d9bd` | 10919255 / 1791254901.979538 | `S/FightMatchCanvasPersistenceProbe.cs` |
| `Assets/Scripts/FightMatch/Host/Editor/FightMatchCanvasPersistenceProbe.cs.meta` | 243 / `ebedc86375ba888c8ef1ce74dade692901094e39e1af6cab8b79716dc0923672` | 10919269 / 1791254909 | `P/probe-source.meta` |

三叶均device16777244/uid501/0644/无link。删除前fresh确认：无Unity/UPM占用，旧owned闭合，三叶全身份与以上最终归档及原creation稳定字段匹配，两个GUID与原创建记录一致，已删除Scene仍缺席；恢复归档可读且hash相同。所有前置先核完，逐叶删除前再fresh核，漂移保留不强删。仅从首次观测身份改为本包明确列出的最终归档身份；不修改或重跑旧runner，不泛化放宽守卫。

receipt记录新owner/issuer、原回执绑定、三叶删除前/后的事实与归档、实际命令、四临时路径最终缺席或保留原因；再机械核原共享/投影清单与N11R1冻结证据不变。只读检查不是测试复跑。保留所有TMP、Library/DLL及失败证据，不删目录、不清缓存、不启动Unity。输出≤4KiB。完成后停写，向主程交回可恢复清理结果；失败则交实际残留与原因。

## 后继B：从现有数据解释保存差量并提出下一步

主程复用SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，新实际回合，Astra/xhigh；只写 `docs/team/2026-09-30/engineering-layout-n11r1-serialized-delta.md`（≤8KiB），不更新其他索引。不是代码审查，不派额外审查代理。

固定输入为上述receipt/native、N11R1的S原/冷Scene和P两后像、原N10持久化诊断稿，以及与观察直接有关的已有Canvas/ResponsiveLayout/Host源码。所有输入只读，无新Unity、测试、fixture、依赖安装或产品修改。

1. 对精确marker片段之外的完整字节差量分类：79条新增Prefab修改的对象/属性/来源观察、原27的内容/顺序、其它对象/版本/数值/排列是否变化。用机械证据说明覆盖了哪些字节；不只列属性名便称全部解释。不得导入runner触发顶层副作用；若复用纯解析逻辑，隔离提取后仅处理冻结字节。
2. 对照五观察说明首次出现阶段、驱动者/有效几何/磁盘值各自证明什么。区分“观测到重新产生”与“已定位具体writer”；未知明确保留。解释本次Unity成功而wrapper UNKNOWN的具体含义，不把诊断分类失败冒称Unity保存失败。
3. 回答下一步如何实际消除原P1风险：是否仍有依据要求五条磁盘记录必须消失，还是应先验证真实Host首次布局与交互。提出一个最小、可证伪、限定运行的后继建议；如建议调整技术验收断言，明确原断言、证据、替代可观察条件及仍需GitHub独立审查，不自行修改门或宣告P1误报。
4. 不以继续尝试Set/Save删除同样记录作为默认下一轮；不混入LOC的新授权尝试，也不凭Editor几何证明Player功能。只读分析结束即可，不为填满8KiB写长报告。

## 收件与持续推进

主程只更新 `docs/team/2026-09-30/engineering.md`，记录新issuer/C/SYS实际映射；不复制本单。主程唯一收A/B，尽量合并一次实质回执，中央不双重等待下游。完成事件驱动，无变化不轮询；中央收到实际结果后决定最小实施/验证后继。无需用户重复授权普通技术工作；具体工具拒绝按事实交回，禁止绕过。
