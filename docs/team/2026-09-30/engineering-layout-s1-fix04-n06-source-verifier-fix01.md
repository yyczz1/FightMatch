# N06 / SOURCE-VERIFIER-FIX01 — 三轮失败后的窄纠正

2026-10-05。仅纠正 [N06 §2](engineering-layout-s1-fix04-n06-continuation.md) 的静态验证器/回执制作，不再改变产品代码或四个现有交付文件。原三轮配额已耗尽，不能称原包验证通过。本补签由主程 actual `01a109dc-3fc9-7693-be65-e296bf7f6814` 签发，沿 N06 的同一直接用户权源、唯一 C 和原生预算；主程仍唯一收件。

## 前置与身份

原作者 turn `01a109e0-4c55-7aa2-b7e9-f2291b11a6f2` 必须确认停写并结束后，原聊天以 Astra/xhigh 的新实际 turn 执行本包。原准备失败 `exec-6d0f02d3-4b09-42bc-bb6a-87f3b2be89fc`、正式失败 `exec-6190308b-e92c-4ced-b78d-0ff0fe75513a`（0.081565083秒）、`exec-9c312271-0bd1-43ad-a87b-744821b36d7f`（0.039249958秒）、`exec-f68e2e96-edd8-42f9-84d1-1f5718326b32`（0.041393667秒）全部保留。三正式失败累计0.162208708秒；最初准备失败未记录精确耗时，保持未知。失败分别涉及旧前像断言、Git blob NUL 编码、旧N05路径前缀及相对/绝对路径混用，均不是扩大产品修改的理由。

主程64547f只读核到 N06/S 当前仅四叶，后续固定：

- `FightMatchAndroidBuild.before.cs`：162754B / `ca6c421b4c0713fc7cda43c493f935135fbce144e523bb81ee30a1ca08c84b1f`。
- `FightMatchAndroidBuild.cs`：162754B / `1411149019d6a9c6713e37e9d80546b619acbda67342cbf9f17cea5063b9e34a`；共享B须逐byte相同。
- `before.json`：11821B / `89a174ad23c009d016a4bb1a4aab228095ba61975d6eb7e6cc504bb7f14dbff6`。
- `source.patch`：984B / `817c38e23b36eb9c012bae35d87705f2996ab9c429f13b39588866ea83467408`。

任一身份变化或第五叶已存在，先 BLOCKED 报告，不覆盖。本包唯一允许新增文件为 `TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N06/S/source-receipt.json`，必须用 apply_patch。现有四叶、共享B及所有其它产品/历史文件均只读；没有新目录/脚本/Unity/投影/环境/TMP/网络/Git写权限。

## 一次验证与回执

读取原回合完整 command/raw，修正纯内存验证器，使用仓库绝对根与已有仓库相对路径一致拼接；不要对相对路径调用相对绝对根的 relative_to。Git blob 可用明确零字节构造，或只对已固定 SHA/字节做原需求核，不引入新的Git命令。

本补签只增加一次最终静态验证，≤5秒；检查四固定身份、共享B和完整正反向唯一替换、+1/-1单路径patch、其余14源/native4、旧N04与N05各精确八叶/hash、最终五叶≤1MiB及真实owner/issuer/权源。直接复用固定记录，不重跑旧42、历史扫描或任何原生门。失败则保留结果并停止，不自行增轮。

回执沿 sourceS15After/native4 可消费 schema，明确 `originalVerification` 三次失败及准备失败的真实命令/raw/耗时或未知字段，`correctiveVerification` 仅本次新增结果；记录 before.json 在前回合修正路径元数据的事实及现固定身份，不回写历史。只有本次逐项通过才为 SOURCE_READY，并注明“原包3/3失败，本补签1/1通过”；不是原包绿测、独立审查或产品ACCEPT。保留原作者和新纠正owner两个turn。

写完回执后只做一次元数据封签（最终五叶/精确字节/hash、receipt字段与既有raw一致），不再运行验证器。回主程五叶身份、全部失败/成功计数和准确总量后停写；主程机械收件后才向仍持直接授权的 C 派 N06 §3，Unity次数保持未消耗。
