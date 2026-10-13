# LOC M05 后继：FIX04 定点源码修正，不进入 B

2026-10-11 · PROPOSAL_ONLY。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，fresh actual `01a12934-1cbc-7991-a403-3b90d61be5b7`（read_thread核实）；中央唯一完成收件。中央后续消息已将原“M接收后规划”改为新失败定位；本文件不宣布M接收、不派工、不授权执行。

## 1. 失败与可证范围

令 A=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/`，F3=`A/loc-lic-alt-yamldotnet-16.3.0-m17-fix03`，E05=`A/loc-lic-alt-yamldotnet-16.3.0-m17-m05-resume1`。已审F3：PR2 head `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`／GitHub6055811693／[源码ACCEPT](loc-m17-fix03-source-verdict.json)，不是材料ACCEPT。当前driver 121284B／SHA256 `6f3511bcb06ba7f85a640a58f9d6a6aaaa6d9d87247eb82fd9ac7fa2774d6b20`。

中央报告实际LOC turn `01a12931-9e6f-79b1-ac3d-ab9ce3ab4ac0`、工具 `exec-4f9e3510-3f50-4e1d-b33f-f2c983b48133`：HOST 42.369107958秒／OS1。本轮亲读stdout：stage 42.297938833秒，`M_FAILED / GateError: unsafe relative path`，5 GET共16158115B，restore0由中央提供；未等／读取在途最终receipt。stdout 3061B／SHA `647c47d6382904715b183a82faf5abd948364440325af529c5eb285e800cee67`；stderr 1317B／`89cccd444a7fe7b70e819ed6172fa7a90fef0e0be57b38b6ae09c8cd986f3f6c`。

stderr仍只有execute→consume_m_stage的包装异常，未保留内层包名／条目。不能将下述离线定位伪写成原生产异常路径。

只读命令 `python3 -I -B - <<'PY'`（工具块 `81ea74`）对F3模型31个固定nupkg路径逐项判断存在；仅读ZIP中央目录并调用AST提取的原safe及原别名分支。19包可读（14既存＋5本次下载），695普通条目、10目录；归档实际尺寸均等于模型，未重算归档内容hash、未读取／解压条目正文。0.135906208秒；没有driver导入、SDK扫描、原生或网络。

| 当前全部目录元数据结果 | 结论 |
| --- | --- |
| MessagePackAnalyzer 3.1.7 两条`cs//` | F3已有两个精确别名生效，本轮不是该修正失效。 |
| **Scriban 7.2.5** 的 `build//Scriban.props`、`build//Scriban.targets` | 原safe均报同一错误；可分别唯一映射为`build/Scriban.props`、`build/Scriban.targets`，普通文件73B／481B。 |
| 其余当前条目 | 无其他unsafe条目；按PurePosixPath／casefold核19包内部均无碰撞、非普通文件为0。仅是目录级路径事实，不是解包／许可证／构建通过。 |

Scriban来自原TOOL `5.1.0/nuget/scriban/7.2.5/scriban.7.2.5.nupkg`，固定827831B／SHA `13500f59e5fb2128692229f58ed2a129cd82de799ca093bf4fd13ced23b520ac`；本轮只核尺寸／目录，内容身份沿原封签，未来消费仍须实核。F3 `execute_material:1137–1146` 顺序读取现存包并在缺失包处GET；Scriban排在第五次GET之后，吻合观测。F3 `fix03_checks:1531–1532` 只复核MessagePack之前5包，遗漏后面的既存Scriban。

诊断假设排序：①固定归档的重复分隔符未列别名（路径门已RED，顺序吻合）；②规范化碰撞／非普通条目（当前中央目录未见）；③归档损坏或其他内层路径异常（生产内层堆栈缺失，不能完全排除，本轮未做内容校验）。按diagnosing-bugs复用生产红证据并建立元数据反馈，跳过材料重跑／解包；不称唯一生产路径已被证明。剩余12 nupkg＋Yaml源码tar均缺席，不能保证未取得归档无新变体。

## 2. 唯一下一源码包与准确写域

建议中央派 **LOC-M17-FIX04-SOURCE**，复用F3同源执行器／模型，作者actual在派发时绑定。仅新建 `A/loc-lic-alt-yamldotnet-16.3.0-m17-fix04/` 下八叶：

- `source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`；
- `authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。

driver≤256KiB、八叶合计≤16MiB；旧F3／M04／M05及全部外置现存输入只读。无生产仓库、LOC-A、B-CATALOG、Unity、Git或工具目录写入。未来根只声明，不创建：W4=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix04`，M4=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix04`；占位即停，不自动另选根。

中央补充后的工程决定：**选（b）完整身份验证后的nupkg内部重复分隔符处理**，不再逐包加别名。比较：（a）Scriban再加两条精确别名改动最少，但未取得包仍可能同类失败，且把ZIP路径语法绑成不断增长的包表；（b）只折叠内部空段，配合原身份／类型／碰撞门，能关闭同一语法缺陷而不接受越界。它不推定未下载内容合法，也不是通用path normalize。此决定只改变已验证nupkg内部文件名处理；原`safe('a//b')`仍拒绝，F3特例及其“未知双斜线一概拒绝”的nupkg用例被该明确规则替代，历史结果不改写。

准确最小代码范围：

1. `archive_entries`在原verify_archive成功后，以一个私有纯函数（仅本函数调用）替代包专属aliases表：非目录原名须为非空相对字符串；先拒绝开头／结尾`/`、盘符绝对路径、反斜杠、ASCII控制字符和任何精确`.`／`..`段，再只移除`raw.split('/')`中内部空段，以`/`拼回并调用原safe。禁止normpath／resolve／URL解码／Unicode改写。仍用原ZipInfo读取正文；规范键的casefold重复门、文件类型／数量／展开预算／长度门完整保留，别名与规范名同存、大小写碰撞、重复原名都拒绝。全局safe、yaml_entries、输出路径检查不变；目录仍按旧语义跳过且记录，未扩成落盘解压器。
2. 为`archive_entries`路径／类型／碰撞失败补同一个GateError的包key、原ZIP名、规范名（如已产生）及理由；使用转义表示保留完整原名，不截成256字符或只报第一段，不增加文件／日志框架；受既有日志上限约束，超限明确失败。新的`offline-checks.json`保留全部19包705条目录项（含10目录）的原名、规范名／拒绝理由、类型、原始尺寸及包身份，另列四个发生折叠的文件名；不得只保留总数或两条新名。未来新取得包同类处理，遇危险路径仍在原材料门停止并留上述定位，不自动扩大容忍规则。
3. `package_model`只加下节五个精确只读实物来源：复验原bytes／SHA256／目录服务SHA512后记existing并纳入输入表。原14包不动，不搜全盘，不按“文件存在”信任，不对坏／缺复用项自动重新GET。包ID、版本、URL、许可证关系、两csproj补丁、31包闭包全部保持。
4. `download_contract`、`execution_budget`及MStage两处累计体积门固定为剩余13项／9014649B；列表及摘要由固定新路径模型重生，不手抄新hash。command_plan/write_plan按existing分类自然取消五个GET及其写规则；3 restore／900秒（870＋30）不增加，不改网络、进程或失败门。
5. E／M／W、SOURCE身份与补丁基线改为F3→F4；只改相应模型／表／封签。保护函数准确保留F3封签、其failed-prepare-01、M04及M05最终证据和旧W3实物，不把旧保护路径随E误迁到新根。新M4/W4缺席、原生产污染门原样保留；中央收妥最终M05清单后才冻结保护输入，不从在途文件补造身份。

## 3. 五个已下载实物与新执行边界

五个只读来源均为W3=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix03` 的 `acquire/<key>.nupkg`，精确bytes／SHA256／SHA512从F3模型对应条目继承，下载证据取E05日志与中央后续完成回执：

| key | bytes |
| --- | ---: |
| microsoft.codecoverage.17.11.1 | 9139653 |
| microsoft.net.test.sdk.17.11.1 | 34301 |
| microsoft.testplatform.objectmodel.17.11.1 | 1660096 |
| microsoft.testplatform.testhost.17.11.1 | 2882099 |
| newtonsoft.json.13.0.3 | 2441966 |

这是**可复用候选实物**，不是整体M接收：SOURCE准备及未来消费均须复验且保留原件，不把旧失败收据当成功。新write-set不包含旧W3写权；不复制／清理旧根来假装空根。仍缺12个固定nupkg（System三项、xUnit八项、YamlDotNet一项）及Yaml源码tar，总9014649B；锁／assets与真正材料身份仍须后续3次restore生成并核验。

## 4. 验收、有限验证与停止

未来作者只做一次定向内存验证≤5秒：F3两条Scriban RED；F4该包184项保原ZipInfo字节，MessagePack及其余17包结果保持，全部705目录项准确回执。合成第三包的`a//b`、`a///b`在身份门后应映射为`a/b`；不通过完整身份门则不得进入转换。规范名／别名重复、大小写碰撞、错误身份、开头／结尾空段、`.`／`..`段、盘符／绝对路径、反斜线／控制字符、非普通文件与原配额均拒绝，断言错误含完整包／原名。真实ZIP正例仅内存读取；合成例明确标记且只在测试命名空间替换归档校验，生产门不改。覆盖全部19包，不再限前5；未取得13归档记UNKNOWN，语法修正不是其内容已通过。

同组纯模型检查：五来源错hash／缺席拒绝；19 existing＋12待GET nupkg＋1待GET tar；13项精确9014649B、GET0重试／3restore／900秒；旧W3零写权、未知路径和污染根拒绝。只生成／读回新八叶的机械封签一次≤30秒，与定向命令分别记录完整耗时；不跑prepare默认全suite或SDK全扫描，旧完整SDK身份沿既存sealed inventory，真正执行时的fresh门仍保留。测试失败或差量超上述函数／数据范围即停交中央，不新造审计框架。

SOURCE_READY后停在准确新head GitHub PR Code Review及独立源码接收前，不自行执行。**新材料运行须用户另行明确**：消费五个逐字节确认的既有归档，在新M4/W4一次完成剩余13 GET／9014649B和3 restore，最多900秒、零自动重试；由中央重新固定实际owner／head／模型／预算／host pin，不复用M05批准。若用户不授新执行，源码及审查可独立完成，材料保持未接收。

既定阶段不变：M-ACCEPTED→外置B双构建／官方测试／对照→B-ACCEPTED→L编译接入。F3 `verify_activation:931`、`run_material_command`仍限M，B/L只有命令及校验构件，不能把模型声明当已有B执行器。当前唯一后继是FIX04源码，不签B执行，也不提前实现／冻结依赖真实LOC-A的B-CATALOG。若后续M再失败，保留新原始包名／条目与partial receipt，只修新证据指向的最小差量，完整归档经复验复用；不重跑成功GET或自动提升运行额度。

本轮仅新增本文；writing-for-agents将职责／缺口／完成门集中，diagnosing-bugs仅做只读路径元数据定位；未实施F4、未解包、未等待或转发LOC结果。
