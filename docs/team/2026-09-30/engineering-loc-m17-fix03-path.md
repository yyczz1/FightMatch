# LOC-M17-FIX03：固定归档条目路径兼容

2026-10-08 · SOURCE 修正包，未实施／未派工。主程 fresh actual `01a11a69-64ba-7910-824d-07bc2bc10767`（read_thread 核实）；中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一收件。用户一次 M 重试已消耗；本包不授予新的材料执行。

同日中央补充已纳入，修订 actual `01a11a70-60a2-7743-9b1c-8471b78a3e00`：本候选一并固定未来新M/W根，旧根只读；仅改本设计，未新增研究／测试或执行。

## 事实、复现与根因边界

令 B=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/`，F2=`B/loc-lic-alt-yamldotnet-16.3.0-m17-fix02`。基线 driver 114855B／SHA256 `e2da00b70058835c2e268c42451f27125f1ba2f7eb6deb9a5ab4d01547699b70`；prepare 5722418B／`963384088acd9dbd134a59c17a9f2bba527bad58eca5389158176b7be003f2dd`；model `318a02fc4930c4d896aebbe8a2db910300009289e5596fe8ebee7f8980d82a38`。

M04 原 receipt/stdout/stderr 保留：HOST 3.646503833秒／exit1，stage 3.570985625秒，`GateError: unsafe relative path`，GET0／restore0。`consume_m_stage` 888–896行只保留异常类型与文字，生产具体 path **未记录，不能补写成已知**。

本轮 `python3 -I -B - <<'PY'` 两次定点内存检查（工具块 `f10ab7`、`cc90ea`）：AST仅提取原 require/safe/sha/check_bytes/verify_archive/archive_entries，读取封签模型及既存 nupkg，未导入完整 driver。首次按模型顺序检查到第6包，在79个非目录名中发现两条 RED；第二次实际调用原 `archive_entries`，相同错误稳定重现。内存耗时0.077634083＋0.037774334＝0.115408417秒；不是生产 M 重跑。

已证缺陷：`safe` 89–93行要求 `str(PurePosixPath(rel)) == rel`；`archive_entries` 958–973行把 ZIP 原名直接送入，拒绝固定官方归档内重复分隔符。`execute_material` 1131–1140行在遇到首个缺失归档前就解析现存包。`MaterialWriter.__init__` 992行仅赋三个字段，不校验路径／写盘，不是该错误的初始化来源。

问题包 `messagepackanalyzer.3.1.7`：178005B／SHA256 `2566ef3915962caddb7063cf606ae06021650284f619192fc44b9a2f03f0f4ee`，原 verify_archive 的 SHA256／SHA512均通过。仅以下两个原名需映射；该包8个普通条目映射后无大小写折叠碰撞：

| ZIP 原名 | 内存字典键 |
| --- | --- |
| `analyzers/roslyn4.3/cs//MessagePack.Analyzers.CodeFixes.dll` | `analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll` |
| `analyzers/roslyn4.3/cs//MessagePack.SourceGenerator.dll` | `analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll` |

排序假设：①固定归档别名被过严拒绝（已复现）；②别名折叠造成碰撞（本包未发生，须负例保持拒绝）；③危险路径被安全门正确拒绝（`../outside`、绝对路径、`a/../b`、反斜线、换行、未知`a//b`均仍拒绝）。①足以解释 GET0 的现象，但原生产堆栈内层丢失，结论是**相同固定输入的确定性缺陷及最可信生产原因**，不是已恢复原异常 path。按 diagnosing-bugs 使用此红反馈；实现／修后绿测留给后续源码作者，跳过材料复现与全旧测试。

## 最小实现与唯一写域

未来作者只新建 `B/loc-lic-alt-yamldotnet-16.3.0-m17-fix03/` 下八叶：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。总≤16MiB，driver≤256KiB；固定F2原件和M04全只读。作者派发身份待中央真实绑定，不沿旧 actual。

业务差量只在 `archive_entries`：先沿原 verify_archive 校验；仅当包 key、bytes、SHA256与上文精确相符时，用两项显式别名表选内存键，再调用**未修改**的 safe。普通条目走原逻辑；保持原 ZipInfo 读取，禁止按转换后的名字向 ZIP 查找。对转换后的键执行现有 casefold 重复门；原始名或规范名并存、重复、大小写碰撞均拒绝。原文件类型、数量、展开字节、读取长度、归档身份门保持。不要全局 replace/normpath，不改 safe、yaml_entries、authorize_write、MaterialWriter、consume_m_stage 或执行／网络／restore门。

SOURCE编排仅允许：新E／DRIVER／ROOTS.evidence、下列精确M/W常量与真实owner/issuer/dispatch绑定；source_artifacts的F2→F3补丁基线及相应输入/表/封签；新增定点离线用例并令本次 prepare只跑该组、不运行原offline_checks全套。旧用例函数不改，历史结果引用原件，不算新通过。语义差量及SOURCE适配合计增删≤160行，超限交中央。

未来根统一从两个精确常量生成，禁止参数化、备选根或旧根回退：

- `W=/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix03`
- `M=/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix03`

仅改变声明及由其派生的ROOTS、model、命令cwd/argv/env/收据路径、inputs/write-set和封签。31包身份／URL／分类不变，现存包仍读原TOOL；18个未来GET仅目的根随W迁移，3次restore仅隔离路径随M/W迁移。保留原prepare首尾新M/W缺席守卫、execute_material生产污染门及其余执行条件，**不再采用“让SOURCE接受已污染W”的旧提议**。作者准备时只读核新根确实缺席（含悬空链接占位检查、现存祖先无链接），不创建新外置目录；任一占位即停，不自动再换后缀。

原W=`…/Tools/Luban/.work/m17`及原M=`…/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17`仅保护，不进入新写域：原M仍缺席；原W只含已记录 `receipts/material-validation.json`（254B／`a51e3c3a362a3ed1df30401995ab9fa0fa5219ae744c322c4f12bfdfceb16a21`），准备前后逐叶／无链接／字节相同。M04/F2证据保持原身份。禁止删除、覆盖、迁移或将旧失败回执作新材料输入；漂移即停。新模型只声明未来位置，不意味着新M执行获准。

## 定向验证、封签与停止

作者一次定向离线≤5秒，失败保留红结果并停止，不自动补跑。正例：原F2 archive_entries在固定包上红；F3同包得8项、两个精确新键及未变条目字节，其余前5个既存包结果不变。负例：错误包身份、未知双斜线、穿越／绝对／反斜线／控制字符、同名重复及别名→规范名／大小写碰撞均拒绝；原非普通文件与预算门保留。合成碰撞用例走真实archive_entries分支，仅在测试命名空间以校验fixture字节的double替换verify_archive；生产函数不改，真实身份拒绝另测，不能把合成包当官方材料。定点pure seam不得调用 execute/execute_material/run_material_command，网络／子进程／M-W写入为0。

机械封签≤30秒：八叶闭集；新模型只接受上述SOURCE身份、自哈希及精确M/W前缀派生差量。31包／18 GET／3 restore身份、数量、次序、业务和预算不变；写规则的相对路径／stage／writer／配额不变，绝对目标仅按新根生成。逐字段核对无旧M/W执行或输出目标残留（历史保护引用除外），不以全文字符串替换冒充精确映射。七叶身份→seal，记录实际命令、原始输出、耗时及新driver；最终读回八叶与consume_source封签关系。旧M/W及M04/F2保护准确、新M/W仍缺席，`productionTrustPin=null`、`executionAuthorized=false`、`materialReady=false`。

完成即停在 SOURCE_READY／GitHub新head审查待办。缺任一身份、用例失败、额外归档变体、输出超界均BLOCKED，不扩大白名单。中央负责后续发布审查；源码通过也不复用M04批准、不下载／restore、不创建host/activation或材料artifact。本回合实际只新增本文，未实施上述八叶。
