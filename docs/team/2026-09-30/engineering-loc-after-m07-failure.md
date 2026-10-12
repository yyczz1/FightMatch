# M07 restore 失败：最小定位与 FIX06-SOURCE 提案

2026-10-12；主程 actual `01a12dd4-ca41-70d0-b5a7-e8528d83e67c`，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。授权沿中央 actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类 item `01a12dc8-5fb9-7e43-8659-7026352c2050`，中央唯一收件。本轮只写本文，不修改已交付的 B 就绪文件；不授权新原生尝试。

## 1. 流程调整与真实红色证据

使用 diagnosing-bugs 的证据/假设/最小回归流程。按本次明确指令，用已经完整返回的真实失败命令代替技能通常要求的再次原生复现；Phase1/2不新跑，Phase4仅静态对照，Phase5只设计，原生绿色验证与Phase6完成条件保留为未完成。未查无关 CONTEXT/ADR，不扩大限定读取范围。

E07=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m07`；[receipt.json](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m07/receipt.json) 的 nativeFailureEvidence 及其精确引用日志为本次红色证据。原工具 `exec-4def70d8-d499-409c-aefa-385c90732e0f` 已完成，host115.929155秒/exit1，非审批拒绝、非超时；M总报告为 M_FAILED，下载0，后两次restore未启动。

W5=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05`。实际首命令是固定 dotnet 的 `msbuild <W5>/phases/M-runner-generation/source/src/Luban/Luban.csproj -target:Restore ...`，完整 argv/env 见 `W5/receipts/M-runner-generation-restore.json`：2997B / SHA256 `ac2fe4b8a1c2e6b812b196b68bc5e88b516c17823e1d9e1b1f7a074a3a8cf7c1`；startCount1、8.010706375秒、exit1、timedOut=false。这里只引用现有收据，不重复QA的进程闭合/保护核收。

失败位于 **stdout**：`W5/logs/M-runner-generation-restore.stdout`，1762B / SHA256 `7041cea244a542f0b747b05eac8c589f9ffb049eab93fc64c5a55fe9daf69ab5`；stderr真实为空。NuGet.targets(174,5) 报 `Access ... is denied` / `Operation not permitted`，准确目标为 `W5/phases/M-runner-generation/source/src/Luban.Protobuf/obj/fad540fa-601a-42d8-9369-591cbb6955e3.tmp`。日志里的 workload update 建议不是授权，本方案不执行它。

## 2. 三个假设及静态判定

| 排序与可证伪预测 | 已有对照 | 当前边界 |
|---|---|---|
| H1：restore合同遗漏临时普通文件；则失败路径无匹配，而已有最终obj文件有匹配 | 冻结177条有效restore规则中，失败路径匹配0；project.assets.json匹配1；无.tmp规则 | **合同缺口已证实**，与拒绝一致；不等于已实证唯一OS原因 |
| H2：项目改写obj位置导致路径越界；则相关csproj/显式命令参数存在输出重定向 | 已读该phase的Luban.Protobuf.csproj与Luban.csproj；前者确在23节点runner图内，两文件及实际argv未见该重定向 | 当前没有支持；未扫描SDK/import链，不宣称排除所有间接配置 |
| H3：实际profile不同或叠加ACL/挂载/外层sandbox；则准确profile即使覆盖路径也仍拒绝，或profile身份不符 | 收据提供profile SHA256 `25af8b0b9b0bdbb3e7ecbfb93c54ab495c61e0380159d69dca79a202485279f5`，未提供原文；本轮未运行Seatbelt或权限探针 | 尚未排除，需后续离线profile身份对照及另行授权的原生证据 |

基线 R2=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02`；driver205640B / SHA256 `ae1d2c3cb2a8cc1ea95d92fd5c7f3c5fce0eb90fcfa2235e9650023b9bd4ec5f`，model `de9f7e47850dcd2f6cdcb3e6f2c605b3d45975c2116dd3d19722cf16623b2580`，E07绑定freeze2870B / SHA256 `d32e5af44ba61473b333f919910167c6cad2b6691d19cc12e1395fe45e7147de`。身份取现有封签/收据，不冒称本轮全量复核。

`write_plan`（453–488）只为每项目列packages.lock及五个固定obj输出；write-set.tsv 6898–6903行与prepare一致。`command_write_rules`（1003）只选 stage=M、writer为本restore或对应phase的规则，源码staging写权限不会传给restore。read-only jq按相同exact/现有prefix语义匹配上述路径得到0；这次机械对照未执行driver。

`child_sandbox`（1008）先deny file-write*，仅对精确叶子或现有bounded-prefix放行；obj祖先只获 **DIRECTORY** vnode规则，不是其任意普通文件许可。TMPDIR指向phase/env/tmp，也不覆盖日志中实际使用的项目obj。生产run_material_command现场生成该profile并记录其SHA；因此源码链条支持H1，但本轮未重建profile原字节、未解释OS审计，不能把hash记录单独当成完整Seatbelt证明。

## 3. 最小修正：有限GUID临时叶，而非开放obj

提议 FIX06-SOURCE，仅修三次 **M** restore 的写合同与同一规则的消费者。三个phase保持runner-generation/tests-generation/tests-verification；从已有runner23、tests13、tests13项目节点导出49个精确项目obj位置，生成时校验去重后的父目录集合。不给B/L追加规则，不实施B。

在每个该obj下增加专用 `bounded-guid-tmp` 行：相对路径以 `obj/{guid-tmp}` 结尾，stage=M、writer=准确phase-restore，仅匹配直接子叶 `[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\.tmp`，父路径转义且首尾锚定。不能固定本次随机GUID，也不能用obj/{safe-leaf}、obj/**、整个source或全盘*.tmp替代。仅承诺覆盖已观察名称形态；其他新形态仍拒绝并报中央。

提议最小驻留额度每obj `maxFiles=1`、合计`maxBytes=16MiB`（沿既有单项restore输出上限），同时保持W/M全局上限；这是待中央明签的新受限规则，不是已测出的NuGet临时文件峰值。离线或未来原生若证明需更大额度，先报证据，不自动扩大。现有轮询式额度检查不能被宣传为每次瞬时创建的OS硬配额，也不证明累计临时写入总量。

同一个小helper必须统一专用规则的父目录、leaf matcher与profile表达；准确接通 `write_plan`、`authorize_write`、`child_sandbox`、`inspect_command_outputs`、run_material_command的目录准备分支及模型校验。保留exact/原bounded-pattern语义；不能遗漏后两个消费者而创建字面量{guid-tmp}目录，或让quota检查回退为整个obj子树。专用检查只看该obj的匹配直接叶，验证常规文件/无链接及驻留额度，最终五类输出仍走原exact规则。

临时叶只承担中间写入，不冒充lock/assets或已接收材料；现有最终锁/图/包/许可校验和失败收尾不弱化。项目csproj、NuGet/SDK文件、包版本、命令参数、环境变量、网络/子启动限制、REVIEW02侧录及独立activation门保持，不通过chmod、sudo、关闭sandbox或改TMPDIR掩盖缺口。

## 4. 精确SOURCE白名单与历史保护

新 F6=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix06`；唯一作者未来LOC，中央另签实际turn/item。只新建F6下八叶：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`；以及封签真实返回后的 `docs/team/2026-09-30/loc-m17-fix06-command-receipt.json`（≤64KiB）。八叶≤16MiB、driver≤256KiB；不得追加其他源码/测试文件。

除上节规则消费者，源码差量仅限定向fixture、F6输入/保护/路径/实际作者及侧录元数据适配。固定本文、下一中央 `central-loc-m17-fix06-source.md` 和QA最终M07核收记录的真实身份；后两者未提供的path/bytes/SHA及新作者身份均待绑定，不自造。F6仍有后置侧录启动门，不形成自身封签循环。

未来新空W6=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix06`、M6=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix06`；只设计、不创建。现执行器拒绝partial旧根，因此不原地补跑W5/M5；新根占用就停止，不清理或换名。输出派生路径重新封签，历史来源路径不能盲目替换。

保留R2、E07、原A/H、M5/W5及所有更早partial/归档原件。复用中央/QA核收的固定闭集保护清单，不重复QA取证或全partial扫描；若该收据尚无M5/W5完整保护清单，则把这一准确缺口交中央补签一次限定只读捕获，最终SOURCE封签前绑定，不能把旧目录当空。此前有效归档仍从原TOOL14包、W3五包、W4十二包及tar只读复用；不因换输出根复制出新的下载需求。

## 5. 一次必要离线回归与尚缺的原生证据

建议中央给F6明签**一次≤10秒新定向回归**，内嵌driver并只用冻结JSON/表与内存fixture，不跑旧套件/SDK扫描或原生。它应先对旧实际规则/生产匹配与profile生成函数得到RED，再对新候选得到GREEN；不是另写一个永远通过的字符串检查器。必要断言合入这一调用：

- 固定本次失败路径作为旧规则拒绝、新规则唯一许可的样本，同时保留project.assets.json正对照；对49个确定obj和三种phase/准确writer重复结构检查。
- 用旧冻结规则与旧profile生成函数离线重建完整profile，验证其UTF-8字节hash等于真实收据的 `25af8…279f5`；若不等先停止并要求准确实际profile输入，不继续宣称重放了本次sandbox。只生成文本，不加载libsandbox。
- 新完整写集只有预期49条专用窄规则及必要目录适配，各命令profile仅含其本phase规则；真实GUID及另一合成GUID匹配，图外项目/未选phase或writer、obj子目录、非.tmp、附加后缀、大小写/分段不符、路径穿越/相邻前缀及链接负例拒绝；既有exact与bounded输出行为不变。
- 同时调用授权、profile生成、目录准备和输出检查的生产小函数，验证不会建占位符目录；内存文件集的0/1/2项及16MiB边界检测符合提案。无真实写入/进程的seam结果标注SIMULATED，不能写native PASS。
- 零GET、三restore、180秒单restore/870+30秒总M、retry0及原31包/图/patch身份不变；M05/M06/M07旧根只读，生产pin仍null，缺新侧录仍在任何M写入前拒绝。

之后一次≤30秒SOURCE机械封签及真实后置侧录，额度由中央新包明签；不挪用已耗次数。GitHub PR Code Review审准确新head，QA核收后中央决定SOURCE接收；源码层通过仅证明合同/profile表达差量，不证明OS接受该规则或NuGet全部恢复成功。

尚缺的原生绿色证据是：新准确profile被OS实际接受，首restore及后两次按合同完成、真实锁/图/包比较通过且进程闭合；ACL/挂载/外层sandbox不存在其他拦截也不能仅靠离线匹配证明。新原生尝试须单独授权一次0GET/三restore/900秒，旧M07的一次已用，不能称为“剩余两次续跑”。任何新失败保留真实输出，不额外重试。B就绪文件保持原样，B实施/执行仍不在本包。

本轮定位查阅截至134.280秒；初稿落盘并核对65行时总计341.289秒，写作/行数核对另计207.009秒。未运行修复/回归或原生复现；唯一写入本文，未改source、M/W、A/H、B就绪文件，未import driver、ps/Seatbelt/native/restore/Unity/Git、SDK扫描、整partial枚举或跨聊派发。
