# RES-SOURCE-ADOPT-G01/V01 验证草案

2026-10-08 · DRAFT_NOT_ACTIVATED。主测试local/01a0f2e3-4bd2-7130-b43a-19afa702e7bb，actual 01a11832-409f-7110-b80a-f27dc527803f。仅设计。

## 1. 输入与启动门

白名单沿[主程方案](engineering-resource-adoption-next-group.md)。[中央收件](resource-source-adopt-g01-central-receipt.json)：1009→1021叶、32464207B、13变化，native=0。

R=仓库根；E=R/TestArtifacts/FightMatch/。固定E/RES-SOURCE-ADOPT-G01/group-freeze.json：257267B，SHA256 `ed77bf69ccdfb5506c93db8240900f98ac9e30f69f0e8ea3de32d55be017daff`。

中央另签前置：FIX05两P1获对应head的GitHub接收；复用守卫，不造新监护框架；新输入/T适配若改代码另审。封签fresh C actual、脚本/activation SHA及差量。旧1000输入、I-only入口、N07/旧额度不复用；不查询/触发PR18。

## 2. 精确43项与历史证据

以下fixture内全部非参数化[Test]全名各一次，分号连接作testFilter：

| fixture | 数量 | 已核原XML（相对E） |
| --- | ---: | --- |
| FightMatch.AssetAccess.Tests.FightMatchAssetContractsTests | 11 | RES-B-ADAPTER-001/M04/T/results.xml |
| FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeContractsTests | 11 | RES-RAW-JOINT-001/M01/T/results.xml |
| FightMatch.AssetAccess.Tests.FightMatchResourceReleaseSetTests | 13 | RES-ACTIVATION-FACTORY-JOINT-001/M02/T/results.xml |
| FightMatch.Host.Tests.FightMatchResourceActivationStoreTests | 8 | 同上 |

已核固定测试声明=XML子集、全Passed。43全名ordinal排序、每名LF、UTF-8共4587B，SHA256 `4a5ae6c660d8966b7d6792dbb0a582c966e11d6daa3bef4ed2227fe9747de54c`；启动前重建冻结，不另启发现。旧证据仅原组合有效，不重跑其余用例。

## 3. dots优先条件

云聊天01a0f13c-3d87-753a-ad11-16302add9084/durable最新turn 01a0fdb0-992f-74c2-abbb-6a5d55949a38：10月2日READINESS_BLOCKED，实例/通道未就绪，检查NOT_RUN、许可UNVERIFIED。

云端仍缺当前Unity2022.3.18f1/UTF1.1.33、许可/缓存、1021传递/证据回传及AS07真实CreateSymbolicLink可用证明。不开新云任务；未齐时提议下述Mac例外，不能跳过AS或改断言。

## 4. 本机最小范围与预算

唯一C=01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh。复用E/HOST-SETTINGS-FIRST-CREATE-001/M03/下projection(P)、package-cache(K)，不复制完整P/Library。Editor/K/ADB窄例外沿[旧方案§4–5](../../versioning/2026-10-08-host-shared-compile/execution-plan.md)，不继承输入/额度。

P原1008恢复见E/HOST-INTEGRATE-001/I01-Recovery01/receipt.json。启动前核P前像、无消费者/锁；按新封签差量对齐共享1021，保留Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta（243B，身份见旧方案）。推导P=1022叶32464450B、canonical `9fc86fe8c920d8a0656375931eb3cfc782957daded1d94731315ea045b6868bc`，尚未执行。仅旧方案SceneTemplateSettings自然例外另记；其它漂移停。共享只读。

证据槽E/RES-SOURCE-ADOPT-G01/V01；新空E/RES-D-ACTIVATION-001/G01V01/state-tests作为绝对FIGHTMATCH_ACTIVATION_TEST_ROOT，逐级无链接、≤1MiB/128叶。AS自清理；旧隔离树/玩家存档不访问。自然生成仅P的Library/Temp/Logs/UserSettings/obj、根csproj/sln；新短TMPDIR另封签。

最多I/T各1，StandaloneOSX、batchmode/nographics；I含quit，T为runTests/EditMode、上述过滤、V01/T/results.xml，无quit。I未过/未闭合则T=0。准备120s，I360s，T180s；每次自然闭合60s、TERM确认30s，恢复60s，均上限，总≤900s。I/T自Popen计；探测/信号含授权调用计绝对期限，审批另报但不顺延已启期限。无重试/强编/全套/Play/构建/下载/Git。

V01≤32MiB（预留1MiB回执），单log≤8MiB、XML≤1MiB；K≤1GiB且payload冻结，P生成≤4GiB、空闲≥2GiB，TMP≤16MiB/512叶。超限停后续入口。

## 5. 判定、收尾、收件

I/T需OS exit0、全程无编译/加载/批处理中止错误。固定FightMatch.{AssetAccess,AssetAccess.Tests,Host,Host.Tests,Presentation,Host.Editor,Core.Tests}源/rsp/引用/defines/输出/DLL：本轮Csc或完整已证明缓存链。旧I01仍FAILED；[晚到证据](engineering-i01-existing-compile-evidence.json)不是编译时输入/缓存证明，日志末行不能改判。缺链INCOMPLETE，不追加启动。

XML=43/43、全名Counter=1，零额外/缺失/失败/跳过/不确定；AS真实文件/符号链接证据齐。须新鲜同快照owned/消费者核验、残留闭合、隔离根空、P恢复1008、共享/K/旧证据不变。身份重核后仅本轮owned逐PID一次TERM，无KILL/名称清理。未闭合不恢复；仅恢复仍等于本次后像的路径，外来消费者/未知漂移保留并报未恢复项。

V01留activation/input/test-name、前后像/差量、I/T日志/exit、编译链、进程/资源时间线、XML、恢复/总回执及bytes/SHA。中央唯一wait；完成后主测试独立核原始证据，给一个限定ACCEPT/NEEDS_FIX/REJECT。代码审查另门；不代表SDK/正式LOC/真实Host/Android/设备/Demo通过。
