# I01-FIX05-source：两条GitHub P1最小纠正

2026-10-08。PR18 head `e30b9f25facdc35d9ec3ef1d08b2a5fae26bc198` 的review5448246182包含P1 `4211887116`、`4211887126`；主测试 actual `01a11823-5862-7543-9d6e-c423a15079c1` 已wait83 completed。中央唯一NEEDS_FIX见同目录fix04-source-verdict。用户持续技术修正授权继续有效。

实施仍为 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh，新actual；中央唯一wait收件。C/主程不写本包。基线FIX04 runner50243B/SHA `90b94e0ea8c8e1ed6465a8ee72e075a52bdad87c310860c4804212f788c8e6ab`；checker55492B/SHA `c07a84ad0571ec18744417ce97ae9c9ef6558ab855d423fbe057808c02c89141`。

仅新 `TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01-FIX05-source/` 五叶：runner.py、replay-check.py、replay-results.json、correction.patch、preparation.json。原25源码证据及I01-run59叶冻结，Recovery01不读写，原inputs不改，共享Assets仍只读且允许已批准codec十叶独立新增事实。

只修：

1. `transfer`及直接调用点：park/归档同样保护并发源值。检查与rename之间换值不能让未知值丢失或不可恢复地移走；采用最小原子交换/验证与有界归还/保留，目标并发值也不得覆盖。不能因为移至证据目录就宣称原路径恢复成功。继承已审覆盖写保护，不重做其它账本。
2. `closure`的TERM授权全过程纳入绝对结束时间，从自然期结束就开始计TERM/确认的30秒；每个新ps/details/consumer探测及信号前核剩余时间，阻塞子命令timeout不得大于剩余预算。耗尽后不再授权新操作或信号，留下未闭合状态；不能在全部授权之后才开始30秒计时。严格保留原60秒自然、30秒TERM/确认、360秒I/660总预算及原错误语义。

必要反例：实际transfer在park与新增文件归档的原子边界被替换、目标竞争、归还时再竞争，证明所有未知字节保留且失败不伪报恢复；多个owned且ps/details/consumer探测实际推进模拟时钟，确认授权计入同一绝对期限、到期不再发新信号、未核实者零信号/未闭合不恢复。原80项覆盖/断言保持，受批准语义影响的夹具变更逐项说明；调用真实函数，不复制判定算法。

先红后绿，语法/离线最多2轮累计30秒，首绿停止。源码准备累计前70秒继续计入120秒上限；墙钟另列。runner只增必要分支，上限650非空行，不压复杂语句、不另建框架。若原子保护需要未来新有限证据槽，在preparation逐一列出对应已定12park/20归档/条件settings路径；不能用未枚举通配目录扩大写入面。

真实Unity/ps/信号/P/TMP/下载/存档/Git均0；编译判据、ADB、产品及公开接口不改。完成交五叶身份/精确patch/原失败与绿结果，直接final不跨发；中央更新同一PR18并接GitHub审查。未审新源码不授权native，原FAILED与恢复结果分别保持。

## 中央追加：新语法失败后的必要验证（2026-10-08）

实施 actual `01a11829-2ce1-77f0-b77f-1b994c3261c5` wait58反馈：第二轮在语法阶段因第274行字符串错误终止，功能回放未开始。按用户“只因新失败、修复或源码变化追加测试”的既定授权，中央允许修复此错误后追加最多两轮短时语法/离线检查，新增合计≤15秒、整包总计仍≤30秒，首绿停止；不是无变化重复测试。原第二轮语法失败原样写入最终replay-results/preparation，不能删改失败结果或把它计作通过。准备累计仍≤120秒，超过则如实停；本机Unity/真实进程/下载仍0，其余白名单不变。此追加由中央技术调度决定，不需用户重新批准。
