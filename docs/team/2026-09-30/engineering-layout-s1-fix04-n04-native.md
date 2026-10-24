# LAYOUT FIX04 / N04 — 正式刷新修正的一次保存复开

2026-10-04 · PREPARED，主程接回N04/S五叶、机械核固定receipt/B后派发即激活；不得据本文件提前启动。沿人类 `central-resume-scene-loc-2026-10-04.json`、[N04源码包](engineering-layout-s1-fix04-n04-source.md)和[N03原生合同](engineering-layout-s1-fix04-n03-native.md)；N03完整保护/预算/成功导出规则继承，下列窄差量优先。不是D04诊断，不分两次跑同源；一次同时验证刷新假设与正式save→close→read-only reopen。

issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，新actual turn。继承同turn按主程派发精确身份生成I的窄委托，源作者已停写才开始。

## 固定输入与适配

- N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N04`；S五叶只读。B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs`仅此次sync一次，前像162700B/`97d062af40ae59ae8fba67ad05e17860616b243d0949b337a0e336ca07d62fb2`，后像由主程实际消息和S receipt同时固定。T=`Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs`两端36769B/`1d53ef04c21b6914a9aa9c9042cf98aba78c58e62b77f321edd11888e02acbdc`不sync/不写；其余14源不变。
- 最新投影前像用D03/PExecution/after.json的`projectedAfter`（已正式还原N03 B），共享用D03/PExecution/before.json的`sharedInputs`，只把共享B和投影sync后B更新为N04固定后像。D03主回执41052B/`fc3a1419f26d67ec6a79b881be590d16b9b9ff1367570ada2203c28c3906093d`、after376078B/`475c4044f5b835e32c71a8bb9f9576a902d47f735ecc2f1e934de936fd81685b`；冻结其12叶、before内116旧证据、N04/S五叶/源码合同及本合同，勿沿诊断sync后前像或还原支路。
- Scene/Prefab/meta仍N03合同固定身份，1089共享/996投影/513GUID/51依赖/37资源/2自然meta/7保护保持。除成功后的唯一Scene导出外，共享零写；投影只B一次sync及原生Scene允许差量，禁止手工YAML/额外序列化变化。
- 五旧TMP全部保持：N02 `fm-ln02.9rDNmQQL`、D01 `fm-ld01.voVEfQ7Y`、D02 `fm-ld02.3pV7HUAz`、N03 `fm-ln03.808Kd2tY`、D03 `fm-ld03.jtjZZsWG`，均在`/private/tmp/`，按最新各回执清单核，不复用/删除。仅一次新`mktemp -d /private/tmp/fm-ln04.XXXXXXXX`，≤40UTF8/0700/当前用户/无link/初始空/≤16MiB和512文件，保留。

## 执行及成功门

- 新白名单为N03合同相同I一叶/P三叶/PExecution七叶/PExport两叶，加既有S五叶最多18叶、总≤192MiB；未发生的产物不补造。固定Intel2022.3.18f1/Editor hash、projection/cwd、UPM_CACHE_ROOT、RepairLayoutSceneOverrides argv不变，仅I/P/log前缀N03→N04与新TMPDIR。I绑定实际C/issuer/人类权源/本合同/S版本/完整源资源集合。
- 优先最小适配N03/PExecution/runner.py（27418B/`5df948425f0f04e274c2ec9538545291225dd9f35557a21d1da5f610f71e38da`，原308非空行），≤350非空行。single B sync替代原two-source，正确使用D03正式还原前像；不带诊断stop/日志或自动source restore。机械生成身份+AST最多3次累计30秒，各尝试原始结果/完整计时保留。
- 编译证据只要求**本次变化的Host.Editor**真实Csc且DLL不同于D03诊断189952B/`39f74a17dee8f6312afb6914f7723ef6fb7cdf7eb527c3466bf91347c00bdbe2`；记录前后。Core.Tests未改源，不再要求它为了新验证必须重编；可复用N03现有1819136B/`c8bbeecb44ee65c7620f13b31598086aa0a8083a2fdcdf998b0caddd666088be`，若Unity自然重编则记录真实Csc/新身份，均不等于99测试通过。不能沿N03成功分支“两个DLL必须都变”的旧制作条件误拒合法单源变化。
- preflight≤120秒，真实输入/Editor/新鲜进程与锁检查；无其它Unity/UPM才sync B。唯一Editor≤180秒含必要compile/import，retry0，监护≤2秒、cache≤1GiB/派生≤4GiB/单log≤8MiB/free≥2GiB。无需独立编译进程、D04、测试/发现、Play/Host/设备、安装/网络补料/Git。
- 单次正常刷新由已封B的公共验证入口负责；保留0.5角点、有限正值、活动/本Canvas驱动/源身份、Prefab严格门、五覆盖及其它覆盖/顺序保护。原位SaveScene一次→关闭→同进程只读复开，三native JSON、141 targets/3 inputs/6 IDs及byte/dirty门全部保持。外层机械确认已保存Scene仅减少五陈旧覆盖，其余文本/语义不变。没有临时赋值、组件禁启、容差放宽、刷新循环或第二次验证进程。
- 首错或预算失败按N03封存；唯一自然60秒→fresh owned核→各仍活owned最多一次SIGTERM→30秒观察，无SIGKILL/外来信号。失败不重试/导出，不回滚正式N04 B或真实已保存Scene，准确保留实际后像及未完成门。
- 只有exit0/真实编译/三native证据/精确保存差量/只读复开/完整保护/owned清空全通过，才按原白名单逐byte导出唯一共享Scene，前像须等原12900B/0ceee6f…，Prefab/meta不复制。回主程18叶内实际集合与所有身份/计时/闭包/结果后停写；source＋actual Scene交中央PR4新head，独立GitHub/99测试/设备仍后置，作者不签产品ACCEPT。
