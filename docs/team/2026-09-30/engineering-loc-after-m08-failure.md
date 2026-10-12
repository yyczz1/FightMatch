# M08：GUID规则的真实macOS差分诊断

2026-10-12；主程 actual `01a12e4f-69be-72e2-b3ef-ca33e7c4303b`，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据[中央微型诊断包](central-loc-m08-diagnosis.md)，人类授权中央turn `01a12e46-6af4-7b82-958d-d18c1146d85d` / item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`。中央唯一收件；没有新restore许可。

## 结论

**已实证表达式兼容性缺陷：本机Seatbelt安装生产所用计数表达式成功，但GUID临时文件创建被拒；仅把 `{8}/{4}/{12}` 展开为等价连续字符类后，同一1字节创建/写入/删除成功。** 这是OS反馈，不是Python字符串匹配。它直接解释FIX06“离线许可、实际EPERM”的关键差异；并不证明NuGet完整restore或全部生产profile行为已经通过。

真实M08首restore为5.9871385秒/exit1，host129.057504秒，下载0，后两条未启动。准确失败路径是W6的 `phases/M-runner-generation/source/src/Luban.DataLoader.Builtin/obj/371f9ba5-1be1-4a9f-996e-d51404ed0ba5.tmp`。本轮不重跑该命令，也不变更FIX06/M08证据。

## 执行身份与限额

新fixture D=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01`。执行前已固定三假设、六profile、准确命令及停止门到[plan.json](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01/plan.json)，SHA256 `8da77ba20231b20d7d7da259eb0bad156cfdb73006d65ce8ff6dc09d4db46b78`。

先从冻结FIX06模型重建完整生产profile并核对实际收据：`production-profile.sb` SHA256 **`5f769139403d229b1ba22378f979019f9a7ac9425c305c73b5e5b7bca3abb916`**，逐字节身份一致；driver236347B/SHA256 `715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1`。没有导入产品driver、扫描SDK或读取额外partial目录。

唯一原生微型调用：`/opt/homebrew/bin/python3 -I -B <D绝对路径>/probe.py run`，原工具 **`exec-14835b0a-9e8d-4d4c-bbc2-79ab648f7c43`**，正常工具审批后completed/exit0；准确展开命令与工具元数据在[tool-receipt.json](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01/tool-receipt.json)。工具记录108ms，控制器实测探针窗口0.157024542秒，两时钟原样分开保留；均远低于20秒，不能互相替换。

恰好6个隔离Python子进程，每个0.023950～0.027527秒（含父层启动/等待），全部reaped、无超时；每次成功数据写入仅1B，三次成功均自行删除同一测试叶，最终无残留数据。保留原始profile、命令、OS errno、退出码、stdout/stderr、时间及PID；本轮不追加任何第7次探针。

## 单变量结果

六项 `sandbox_init` 都返回0、安装errno=0，故不能只看profile“能编译”。每项继承deny写入/网络/fork，仅对fixture一个准确叶子授权；regex与该literal取交集，避免实验开放其他路径。

| 探针 | 规则变化 | 子进程exit / open errno | 写入 |
|---|---|---|---:|
| P1 | literal＋DIRECTORY正对照 | 0 / 无 | 1B |
| P2 | 生产计数表达式，普通SBPL字符串 | 1 / EPERM(1) | 0 |
| P3 | 只展开计数，普通SBPL字符串 | 0 / 无 | 1B |
| P4 | P2字符串前加 `#` | 1 / EPERM(1) | 0 |
| P5 | P3字符串前加 `#` | 1 / EPERM(1) | 0 |
| P6 | P1移除DIRECTORY授权，父目录预先存在 | 0 / 无 | 1B |

H1得到直接支持：P2→P3只变重复计数编码，OS拒绝变成功。H2的“给现有JSON转义字符串直接加#”不是修复，P3→P5反而失败；本次没探测所有合法regex-literal转义，不泛称该语法不可用。H3在本fixture的已存在父目录上不成立，P1/P6都成功；这不授权删除生产目录规则或排除其他挂载/ACL/外层环境问题。

[result.json](../../../TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01/result.json) SHA256 `705fdbfca0467b47715add521197f3cf263cfe4f5f75f8d5b46e7ba9018b6eba` 是原始子进程收据；`profiles.json` SHA256 `e483645ffeadab716e4be5caaf7ea538a5f8ec3c4de0cea841d5bd0f497caa75` 保存六个准确profile。最小fixture移除了生产其他写规则，将目标父目录映射到D/obj，预先创建目录，用Python os.open/write代替NuGet，且真实profile未被整个安装到历史路径；探针宿主审批环境与M08外层环境未证明等价，这些差异不能隐去。

## 为何55项离线检查漏掉

FIX06 `fix06_checks.profile_hit` 在2941行附近用Python `re.search` 模拟SBPL regex；其DIRECTORY判断及49个位置检查验证的是Python语义。Python接受计数重复，不代表本机Seatbelt按同样方式匹配。本次P2已经真实重现这一缺口。原55项正确标注SIMULATED/native0，其历史结果保留，但它们不足以关闭OS兼容门；profile hash相等也只能证明输入文本相同。

按diagnosing-bugs，本轮以已失败M08为红色证据，在一轮预先签定的六个小进程中完成最小重现、单变量反证和局部绿色对照；不追加通常的重复复现。技能要求的“原始完整场景修复后通过”仍未满足，不能声明M已修复或材料ACCEPT。

## 最小后继修正建议

只改GUID表达的构造，保留普通字符串序列化方式：`'-'.join('[0-9a-f]' * n for n in (8,4,4,4,12)) + r'\.tmp'`。沿用父路径re.escape、首尾锚点、49个项目obj、准确writer、1驻留文件/16MiB、无链接规则；不加#，不放宽整个obj或文件名集合。

预计生产行为差量仅 `FIX06_GUID` 常量构造；`_fix06_guid_rule`、`child_sandbox`、授权/目录准备/输出检查继续消费同一表达，控制逻辑无需改。另加最小表达等价性/边界检查及新SOURCE根/作者/固定输入/侧录封签适配。若实现发现需要更改其他运行函数或资源额度，先报中央，不以本报告默许扩大。

后续LOC建议仅新建 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07/` 下原八叶：source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch、authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json；另新建 `docs/team/2026-09-30/loc-m17-fix07-command-receipt.json`。保持八叶≤16MiB、driver≤256KiB、侧录≤64KiB。中央未来包明签具体身份/次数；主程本轮不修改这些文件。

把本报告、本次plan/profile/result/tool回执和下一中央包固定为来源。FIX06、M08、A/H及M6/W6只读保护，复用有效31＋1归档，0 GET/三restore/900秒不变；后继M/W根另由中央签发，不能原地重跑或覆盖partial。SOURCE通过与新的材料许可分开，B不在范围。

必要新回归只验证**新候选生产生成器**的差量：离线确认字符集合/长度/路径范围不变，并引用本轮P2/P3真实证据；不要重跑55项整套。签发下次完整restore前，建议中央另签一次最多3个≤2秒子进程的小型原生回归：新生成表达的合法GUID成功、非法长度拒绝、额外后缀拒绝；fixture保留 `.work/.../Luban.DataLoader.Builtin/obj` 的标点形态，并保持单叶literal交集和≤16B写入。它验证新代码接线及负边界，非重复本轮六个实验；本轮额度已经用完，不执行它。

当前仍缺：新代码准确head的GitHub审查、其限定真实回归、完整新profile与NuGet实际写入/rename及三restore成功证据。不能用本轮局部绿色建议盲跑；这些门和新原生许可须由中央分别接收/签发。

本轮保留诊断fixture为明确标记的证据：最终7文件/102782B（≤12文件/128KiB），1B测试叶均已删除，无产品调试插桩。报告初稿完成总耗时499.223秒，含阅读、准备脚本与写作；不可与0.157秒探针窗口混同。仅生成D内实验文件和本文；未修改旧driver、源码证据、材料、权限/ACL/挂载，无SDK/dotnet/restore/Unity/Git/ps/下载或跨聊消息。
