# FIX05 REVIEW02：后置侧录的启动校验设计

2026-10-12；仅设计。依据中央提供的 PR2 head `5c12ddab`（短标识）及 P1 `4242690950`。用户继续授权：中央 actual `01a12bf9-e665-7d20-ab65-31ca1dfc8613` / item `01a12bf9-ee2d-7140-a43f-a33e3bafec6c`。主程负责设计，后续唯一源码作者 LOC，中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 唯一收件；本文件不是源码接收或材料执行授权。

## 1. 决策与固定输入

采用“中央 activation 固定后置侧录身份，driver 在启动门内 fresh 校验”的最小方案。**中央 host 模板逻辑不用改，只扩展 activation 文件正文；侧录不进入自己的 SOURCE inputs。** 不增加签名服务、认证框架、恢复或执行框架。

基线 R1=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review01`。driver 162125B / SHA256 `ec629ed6be21319add7d0a5b75bcd0747ca07930aef91f88a114ade1347b6b81`；freeze 2869B / SHA256 `d9a387c7918b9d0dfb27c302e31d073ce3a0aa11915d7d6f93b32135258782b3`；model `af686a7f3a35987f194d955dfef1b12eccd9ad8f5464da8d22227dc9af797bc0`。身份来自冻结字段及中央输入，本轮未重复源码审查或测试。

当前 R1 `execute` 在第908行完成 `verify_activation` 后立即设置 `state['m']`，第910行进入材料；该验证函数未读侧录。现有 `EXTERNAL_SOURCE_RECEIPT` 只是声明，不能阻止侧录缺失/改动后的启动。`run_material_command` 第1036行已再次调用同一验证函数，可复用新门，不新增另一条旁路。

## 2. 无循环的绑定与准确 schema

后继 R2=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02`。固定侧录 S2=`docs/team/2026-09-30/loc-m17-fix05-review02-command-receipt.json`；代码与记录中均使用仓库 `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch` 下的准确绝对路径。

链条为：R2 driver/固定输入 → 七叶与 freeze → 成功工具返回 → S2 → 中央 activation A → 原 host CONFIG 固定 A 的 bytes/SHA256。R2 内只封签 S2 的结构化路径/上限/必需门；不封签尚未产生的 S2 内容、tool ID 或耗时。

在现有 A 正文中增加一个必需字段，不另加 CONFIG 字段：

```json
"sourceCommandReceipt": {
  "path": "<固定 S2 绝对路径>",
  "bytes": "<实际正整数，非字符串，最多65536>",
  "sha256": "<实际64位小写十六进制>"
}
```

上例仅为字段说明，不能生成含占位符的激活。该对象键集合严格为 path/bytes/sha256；path 必须逐字等于 driver 固定 S2 和封签契约路径，禁止调用者选择其他记录或以 R1 侧录替代。bytes 排除 bool，范围1..65536；SHA格式准确。A 的全部正文仍先由既存独立 trusted pin 校验，无 pin 不读取侧录。

S2 延续当前侧录 schemaVersion=1 及原成功捕获结构，不引入平行证明格式。必需字段包括 sourceAuthor、sealExecutor、capture、authority、tool、fullInvocationClock、sourceFreeze、sourceFreezeUtf8、sourceModelSha256、eightLeaves、totalSourceBytes、directedVerification、externalRequirement 和现有范围/状态字段。重复 JSON 键、非有限数、错误类型、缺必需字段均拒绝；摘要布尔值不是证明。

author 固定为 R2 `OWNER` 并等于 freeze.owner；实际封签者沿 `sealExecutor={threadId,hostId,turnId,receivedItem}` 记录，thread/host 为批准 LOC/local，真实 turn/item 可与源码作者不同，但必须与原输出时钟及 capture 的 thread/host/turn 相符。不得因封签回合变化重写源码作者。authority 绑定新中央包的精确 path/bytes/SHA256 与 ISSUER，不拿材料执行 ownerTurn 冒充源码作者/封签者。

## 3. 唯一启动门与失败边界

仅在 `verify_activation` 原有 pin、身份、源码/model、审查 head、预算、M-only 等条件通过后、返回 True 前增加必需侧录验证。可增加仅供定向 fixture 注入的 reader seam；生产默认必须调用固定 S2 的 fresh bounded reader，不读取调用方摘要/缓存，不提供“跳过侧录”开关。

reader 拒绝文件及祖先 symlink，要求普通文件，以最多65537B读取并按65536B硬上限拒绝超额；校验 A 记录的 bytes/SHA256 后才 strict 解析，同一份读入字节用于全部校验。原始命令/输出仅作数据，绝不 eval/exec/subprocess 执行。原通用 `read` 不必为此改造。

`execute` 的调用顺序保持：consume_source → 验证 A 与 S2 → `state['m']=m` → stage.allowed → execute_material。现有位置已经合适，无需搬动材料代码。S2 失败必须在 state 赋值之前抛错；此时 finish 只能打印失败报告，不构造 MaterialWriter、不创建 M/W、也不写 `material-validation.json`。中央 host 自身获准的 stdout/stderr 外层记录不属于 M/W 写入。

`run_material_command` 仍调用同一个 verify_activation，默认 fresh 再读 S2；中途删除/改动侧录时不能启动下一 native 命令。此处保留原失败收尾规则，不承诺撤销此前已获准的材料写入。SOURCE prepare/consume_source 本身不要求自己的后置 S2 已存在，否则重新造成循环；它们不是 M 激活门。

## 4. 侧录内容的强制对应

1. 从已经消费的 freeze 七叶清单加当前 freeze 的 bytes/SHA256，构造准确八叶身份集；路径集合必须等于 R2/PREP，条数8、无重复，每行仅 path/bytes/sha256。S2.eightLeaves 与原工具输出 leaves 都须逐项一致；不只比较数量，也不重新枚举目录。S2.sourceFreeze 及 sourceFreezeUtf8 的 UTF-8 原字节须等于实际 freeze；model 同时等于 freeze、已消费 m 及原工具输出。
2. tool 必须为原 `commandExecution`、真实 `exec-UUID`、status=completed、整数 exitCode=0、非空完整 command、准确 cwd，output.truncated=false 且保留完整 output.text。capture 标示 read_thread、afterToolReturned=true、toolOutputUntruncated=true；这些来源声明由中央核收，运行时不得将其单独当作真实性证明。
3. strict 解析 output.text 的非空 JSON 行：本契约恰好两条，先唯一 `SOURCE_SEALED_EXTERNAL_RECEIPT_REQUIRED` 结果，再唯一 `FINAL_COMMAND_CLOCK`；额外/重复/缺失/不完整行均拒绝。S2.fullInvocationClock 必须等于解析得到的完整时钟对象，不能仅信任复制的 exit/耗时。
4. 原时钟要求 attempts=1、maxSeconds=30、整数 exitCode=0、rawError=null、有限数 fullInvocationSeconds 在0..30，UTC起止有效且有序；tool.durationMs 同为有限非负整数且≤30000。工具时长与单调时钟不要求数值相等（R1已有4650ms与4.73761475秒），不能用函数局部耗时替代完整调用时钟。原结果 mechanicalFunctionSeconds 必须有限非负且≤完整调用时间；超时即拒绝，即使其他摘要写成功。
5. 原结果 sourceAuthor、freeze、modelSha256、leaves、totalBytes、externalCommandReceiptRequirement 分别与 OWNER、实际 freeze/model、准确八叶及封签契约一致；S2 对应摘要也必须一致。totalBytes 按八叶重算并≤16MiB，driver≤256KiB；侧录不计入八叶和自身总量。
6. directedVerification.evidence 必须等于八叶内 offline-checks 身份，caseCount=passed>0 并与封签检查结果及原输出 checks 一致，exit0、真实工具ID、完整调用≤10秒。原输出 directedSeconds 对应封签检查 elapsedSeconds，而不是强行等于工具完整调用耗时；sourceIdentity 必须指向本次 R2 driver。离线检查内容沿已消费的封签文件校验，不执行任何历史用例。
7. scope/原输出/时钟维持 SOURCE-only：无材料执行、GET、restore、SDK扫描、partial新枚举和历史套件重跑，mechanical attempts=1；status 仍为成功封签但非独立接收，materialExecutionAuthorized=false。externalRequirement 与 R2 封签契约相等；历史回执只作历史输入，不能替代本次成功记录。

中央生成 A 前仍需用真实原工具结果独立核对 S2 的命令、tool/turn、完整输出及退出状态，再固定其哈希。启动校验能证明“读取的记录就是中央绑定的记录且内部与冻结源码一致”，不能离线证明任意人编造的一整套日志确实由工具执行；不为此扩大成认证系统。摘要与原输出不一致必须由程序拒绝，所有摘要与原输出一起伪造的来源问题由既存中央信任边界负责。

## 5. host 结论与精确实现白名单

已读 `loc-m17-central-host-template-01/source/central_host.py`：CONFIG 仅 driver/activation/argv；activation 值仅 path/bytes/SHA256及七身份，verified_file 固定整个 A 文件，不限制其 JSON 正文字段。因此新字段只放 A 正文；未来 H 仅按已有权限填新 A/driver 身份及 argv，**无模板逻辑修改、无新 H 顶层键、无本轮 A/H 文件创建**。

后续 LOC 只允许新建 R2 下八叶：

- `source/m17_driver.py`
- `source/m17-inputs.tsv`
- `source/m17-write-set.tsv`
- `source/source.patch`
- `authority/prepare.json`
- `authority/offline-checks.json`
- `authority/source-freeze.json`
- `authority/source-receipt.json`

唯一额外新叶为 S2，≤64KiB，只在封签命令真实返回后捕获生成。driver 以本页 R1 身份为补丁基线；差量限 verify_activation、来源/保护/模型绑定、source_artifacts/prepare 的新版本元数据，以及 `_review02_*` 小 helper 与 `review02_checks`。execute、run_material_command、consume_source、MaterialWriter、归档/restore/监护算法可保持不变；若实现认为必须变更这些函数，先报中央而不扩展。

把本设计与下一中央包 `docs/team/2026-09-30/central-loc-m17-fix05-review02.md` 的准确 bytes/SHA256 加入新固定输入；中央包尚未产生，身份待绑定。保留并固定 R1 八叶和已完成 R1 command-receipt 作为历史输入；它们不是 R2 自引用。文档、真实源码 author/dispatch 均在新检查前固定，所有旧根和回执只读。

未来 M5=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix05`、W5=`/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix05` 不改。31+1归档身份、0 GET、三 restore/870+30秒保持；复用原 partial manifest/捕获身份，不新增 partial 枚举或 SDK 扫描。R2/S2 如被占用停止，不覆盖或自动换根。

## 6. 一次定向回归与接收

新回归合为**一次完整调用≤10秒**，不跑旧81/66/15；用小内存 fixture 和哨兵替代 source消费、材料写入/native，不能借测试启动材料。至少覆盖：

- 无 trusted pin/无 A 新字段/错字段类型/非固定侧录路径：拒绝，错误 pin 时尚未读取 S2。
- S2 缺失、超64KiB、链接、bytes/hash不符：拒绝；篡改后重读被发现，不能命中缓存。
- 将变异 S2 重新哈希并放入合成 A，使外层 hash 正确，再验证内部错 freeze/model/八叶缺失、重复、换路径、错身份均拒绝，防止负例只测到外层 hash。
- sourceAuthor/封签者/authority不符；失败tool、未完成、截断、非0退出、缺/双结果或时钟：拒绝。
- 时钟>30秒、失败/超时被摘要伪装、非法数值/bool、摘要时钟/叶集/model与原输出不一致：拒绝；定向检查>10秒或绑定另一 driver 也拒绝。
- 有效记录通过，容许工具时长与完整单调时钟合理不相等；原始命令字符串只作为数据，fixture 中的可执行文本绝不执行。
- execute 集成哨兵：每个启动拒绝分支 execute_material/native/MaterialWriter 调用为0、`state['m']` 尚未赋值；有效分支仅到达一次材料哨兵。第二次 verify 前删除/改侧录则拒绝，不启动后续命令。
- AST/字段差量证明 host 模板、未来 M/W、零GET及既存预算/保护不被改变；新侧录仅为后置门，不进入自己的 SOURCE inputs。

之后**一次完整调用≤30秒** SOURCE机械生成/封签，沿原16MiB/driver256KiB限额；真实返回后创建 S2。失败/超时保留证据并报中央，不自动第二轮，不将历史通过算作本次启动门通过。新head与S2随候选交付，Git由中央现有权限处理；必须由 GitHub PR Code Review 核对新head/P1关闭，QA核收后中央判定源码。源码接收不解锁材料，新 A/H 和 M 执行仍须独立授权。

本轮只读调查约31.314秒（120秒限额内）即停止；其后初稿思考/撰写另计约206.579秒，已超过120秒，不能把全轮声称为31秒。其后仅作本文措辞与行数核对，不追加历史查询。本轮仅新建本文，未实现、prepare、测试、SDK扫描、partial目录重枚举、Git、GET/restore、Unity/native或新增激活；未跨发消息。
