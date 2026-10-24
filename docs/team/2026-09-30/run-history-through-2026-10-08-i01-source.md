# FightMatch 当前接续快照

2026-10-08。中央唯一维护：`01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。本页只保留当前事实；历史按任务链接读取。

## 当前任务

**Host接线十项与PR17修正均已限定接收；五路径已实际采用到共享工程，等待一次新组合编译。** 首Demo尚未完成。

- 中央已通过实际wait54接回C completed：T01 actual `01a1171a-c943-74d2-a38a-bf0dfa86549d`。固定HCP八项＋HISO05/06共10/10通过，零失败／跳过／不确定；只运行一次T，执行器43.247420秒，I0、重试0，准确复用M03编译。
- [中央限定接收](host-continuation-fix01-integration-receipt.json)为唯一 `ACCEPT`：实读XML，核66证据叶7214514B、32候选身份及P恢复原1008叶。源测试仅FIX01三行编译纠正；产品／断言未额外改动。没有本机进程遗留或关闭信号，原失败保持。
- [PR16](https://github.com/yyczz1/FightMatch/pull/16) head `030898e9a317d10b610058d867529be8b0770c7c` 与32份候选逐叶对应；同head [GitHub回执](testing-host-continuation-pr16-natural-meta-review-receipt.json)无major，主测试actual `01a1171e-dcb5-78e1-b75e-08a5d4fdf95e` completed。draft未合并，不代表所有级别问题或设备／试玩验收通过。
- M03原wrapper因自然meta补全失败仍保留；T01固定243B完整meta，同GUID，先核旧编译后测试，没有重跑I。详细证据：[T01回执](../../../TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/T01/receipt.json)，187001B/SHA `100a081a68e5f68c30a523304884753e5a5fcc04eecfd2750115b1c1b2bedd1d`。
- `HOST-NEXT-001/M01` C actual `01a1173e-2a8b-7a72-b25d-b8fb01756014` 已wait89 completed；四项真实场景观察A/B/C/D均PASS，但SDK新ADB触发旧进程门、空Scene setup恢复报错、GameView删除索引错误、收尾监护缺口导致整体FAILED。中央实核42叶，唯一[NEEDS_FIX](host-next-001-m01-integration-receipt.json)，旧结果不改；P已恢复1008。
- [M02最小纠正](central-host-next-001-m02.md) C actual `01a11762-1ac1-7040-8240-f6f19ccd6268` 已wait103 completed；一次native exit0，A/B/C/D及真实清理PASS，中央实核43叶8515709B和P恢复1008。原产品1012输入不变，新增场景/probe四输入；完整canonical `c1676dbc75b9096825d1601dbeeb182149d61479af26305697b633c6e914f964`。监护实际最大起始间隔2.270427秒、自然退出宽限60.253449秒均披露，不抹调度开销。
- [PR17](https://github.com/yyczz1/FightMatch/pull/17) head `521f1c78c70f4e88e9139ff543202c22dcf67133` 的GitHub审查已完成，主测试actual `01a11779-f5f2-70a2-a22e-9726f9d68fe3`已wait18 completed；[回执](testing-host-next-pr17-review-receipt.json)记录两条P2：runner不得将FAIL/UNOBSERVED误判stage通过；C比较样本必须确实取消后、unbound/unsubscribed/gate释放。中央唯一[NEEDS_FIX](host-next-001-m02-integration-receipt.json)。
- [M03纠正](central-host-next-001-m03.md) actual `01a11781-abc7-7203-a1de-187852f84168` 已wait114 completed：16个runner行为用例、8个C#反例、严格A/B/C/D及清理/恢复通过，native0；但monitor两次ps间新生的合法bee子进程被误报，runner1，原整体FAILED保留。中央核43叶，唯一[NEEDS_FIX](host-next-001-m03-integration-receipt.json)。
- [M04-source](central-host-next-001-m04-source.md) C actual `01a1178c-c850-7df1-a100-c1517709d229` 已wait117 completed：同快照先发现/核身份再分类，21/21离线实际函数回放通过，0.267871秒；未启动Unity/真实ps/ADB/kill。M03原整体失败保留，M04未native重跑。
- [修正发布回执](host-next-001-m04-publication-receipt.json)：PR17新head `ad0f88a177ded15a05a7c661c80b3797a19ab251`，差量11路径、全包16路径，全部远端blob实核。主测试 actual `01a1179c-ddee-73f1-917b-7cf6f4cdcba6` 已wait29 completed；[GitHub回执](testing-host-next-pr17-m04-review-receipt.json)无major/新增findings，非formal APPROVED。中央唯一[限定ACCEPT](host-next-001-m04-integration-receipt.json)仅观察源码及离线监护修正，M03失败、M04未native、旧线程未resolved及PR4 P1保留。draft未合并。
- 主程 `HOST-INTEGRATE-001` 方案 actual `01a11794-4243-7291-966d-731c049e03fb` 已wait59 completed；[五路径S01启动单](central-host-integrate-001-s01.md)已交C actual `01a1179e-15f8-7442-89fb-cded184baaee`。只接三份Host源码及两缺失meta，不改测试/场景，不整包覆盖32文件；资源候选仍未在共享Assets采用，较新AndroidBuild保持。主程 actual `01a1179e-21bb-7c11-8f81-04fb4e9311e3` 并行准备精确共享组合编译方案，中央分别唯一收件。
- C S01已wait129 completed，[中央收件](host-integrate-001-s01-central-receipt.json)实核8证据叶428021B、999当前输入及精确五路径差量：`SOURCE_ADOPTED_COMPILE_PENDING`。三源码和两meta采用成功，无Unity/测试/Git/真实存档操作；不能用PR16旧组合编译关闭当前门。
- 主程[一次编译单](engineering-host-integrate-001-compile.md)已wait75 completed：复用P/K，999共享输入+1份已有meta，精确32路径差量、I≤360秒、无Play/测试/重试。[中央I01授权](central-host-integrate-001-i01.md)已交C先准备5份源码/离线证据，再经GitHub审查后固定实际激活；尚未启动新Unity。
- I01准备 C actual `01a117ae-0ee5-7151-b160-60c882659f74` 已wait142 completed；5叶874060B，27/27首轮离线检查2.192877秒，无真实ps/信号/P同步/Unity。中央核原5叶与检查，发布[PR18](https://github.com/yyczz1/FightMatch/pull/18) head `54534228f88a7e8609669b00e1aae2eb4c7caaf3`，9远端blob实核，一次触发 `6044831804`，主测试收新head审查；[发布回执](host-integrate-001-i01-publication-receipt.json)。旧完整缓存绑定0，不伪造，必要时native结果只能INCOMPLETE。native尚未运行。
- PR18审查已wait42接回主测试 actual `01a117c1-04a7-7100-b1f2-ef387f832235`：两个P1，半写恢复及discover异常二次ps。中央[NEEDS_FIX](host-integrate-001-i01-source-verdict.json)，已交唯一C actual `01a117c8-1b7c-7f02-8b55-5cc2bea5d674` [FIX01源码小包](central-host-integrate-001-i01-fix01.md)，只原子写入/异常循环与对应离线反例；原I01证据不改，native0，修后更新同一PR复审。后续执行根另用I01-run（尚不创建），保留旧五叶，P/K与输入不变。
- FIX01已wait152 completed，5叶119909B实核；首轮夹具失败保留，第二轮37/37，两轮8.467秒，native0。PR18更新head `1c5a34ecd126135abb40a9e610a175c000e8d772`，差量9/全12路径远端实核；[发布回执](host-integrate-001-i01-fix01-publication-receipt.json)，一次触发6045165771，主测试正接新head复审。原共享5叶保持采用，未启动I01-run。
- FIX01审查已wait55接主测试 actual `01a117d2-04f8-70f2-9724-8e1f1bfbbf78`：新两P1（覆盖提交丢并发值、details失败污染owned）。中央[NEEDS_FIX](host-integrate-001-i01-fix01-source-verdict.json)，已交C [FIX02源码小包](central-host-integrate-001-i01-fix02.md)。原检查/失败及旧四条GitHub线程状态保留，native仍0。
- FIX02 C actual `01a117d8-5744-77b2-a45d-e18d86e96d93` 已wait160 completed，5叶140954B、48/48首轮9.855523秒，旧10叶及输入实核未变。PR18新head `191ee590dbb644ce129a6d6b5787555fcade8af0`，差量9/全14远端blob实核；[发布回执](host-integrate-001-i01-fix02-publication-receipt.json)，一次触发6045477065。主测试本轮机械收件指定Luna/medium，中央保留裁定；native0，I01-run仍未创建。
- FIX02审查已wait67接主测试 actual `01a117e3-96f4-7ba1-a4e6-d350073583e9`：新增根details首次失败后无法重新登记的P1。中央[NEEDS_FIX](host-integrate-001-i01-fix02-source-verdict.json)，已交唯一C [FIX03](central-host-integrate-001-i01-fix03.md)补实际Popen根的有界恢复识别及反例；原问题/已修源码/所有失败保留，native仍0。
- FIX03 C actual `01a117e9-ca3e-7cf0-9a5d-6087e8538c43` 已wait168 completed：5叶163393B、58/58首轮13.709秒，旧15叶与输入保持；PR18 head `48c51dba8cb6487f405f9291fb1a8ab1b20b6c20` 已核差量9/全16远端blob，[发布回执](host-integrate-001-i01-fix03-publication-receipt.json)。一次触发6045836915，主测试actual `01a117f7-db3e-7352-9ad4-fb32ea100aa7`（指定Luna/medium）只收新head审查。尚无新Unity或I01-run。
- FIX03审查已wait74 completed：result6045886949对应当前head，无major/新finding；[QA回执](testing-host-integrate-pr18-fix03-review-receipt.json)实核6590B，五旧P1线程outdated且未resolved、无formal APPROVED。中央[源码限定ACCEPT](host-integrate-001-i01-fix03-source-verdict.json)后派唯一C执行[一次I01-run](central-host-integrate-001-i01-run.md)，准备49秒继续累计，不重跑58项；原二十叶冻结。主程 actual `01a117f9-c3b9-7a41-9ef4-72829fd40684` 独立准备PR4/资源下一最小开发包，仅设计不运行。


- LOC被拒发送仍不重试。已准备[取消主动发送、由中央只读收件的新流程提案](central-loc-passive-receipt-proposal.md)，已用问题卡请求用户明确选择；尚无答复，不激活LOC、不下载、不重绑A/H。场景/审查独立继续。


- 组件／Session十项与真实Host/EventSystem诊断观察源码已限定接收；正式LOC、正常交互、资源构建和设备门仍缺，不用成功替身或草稿CSV伪造正式接入。

## 当前依赖与未完成项

| 工作 | 实际状态与接续 |
| --- | --- |
| uGUI与存档／语言偏好基础 | PR1 FIX22、PR3 PREF77、PR5 HISO与PR7首建settings均有各自限定接收；不冒称本包组合已通过。详见[主程索引](engineering.md)、[测试索引](testing.md)。旧有效测试保留，不重跑全量。 |
| Canvas真实场景 | N11R1保存/复开及wrapper历史UNKNOWN保留。[差量稿](engineering-layout-n11r1-serialized-delta.md)解释原场景＋79记录＋marker。M03严格观察有native证据但wrapper失败；M04监护只离线验证，PR17当前head审查已收。正常交互及设备首帧未验，不机械重试删除五记录。PR4当前head `e3d581df42f3a25cfdfc319bca5509f8b5382ada`的P1仍未关闭。 |
| N11R1临时清理 | [清理回执](../../../TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1Cleanup/receipt.json)已收：三残留清除、四临时路径缺席，原归档可恢复；原失败和输出偏差保留。不再重复清理或诊断。 |
| LOC正式材料 | M17-FIX02源码PR2已限定接收；18归档下载／三次restore／正式材料尚未执行。用户文字和精确approve已给，原样重试仍被自动审核拒绝，[实际拒绝记录](central-loc-communication-hold-2026-10-05.json)保留。不重问同句、不重试／改道、不沿用已结束M或旧A2/H2。 |
| 资源及正式Host | 资源contracts、RAW、activation、factory等限定源码／测试成果保留；正式LOC输入、真实内置资源构建和Host启动接线仍缺。不得用草稿CSV或成功替身充当正式材料。读[接线方案](engineering-runtime-localization-integration-001.md)及[内置包准入](engineering-res-built-in-admission-policy-001.md)。 |
| 美术 | [市场候选](art-market-search-2026-10-02.md)已交，正式资源未选。现阶段Prefab按用户授权使用明显占位图形，无购买／下载／导入许可扩展。 |
| 安卓与Demo | API24源码有独立候选／审查；尚无本轮完整新uGUI试玩APK、实际流程、冷启动恢复及设备验收。iQOO Neo5真机门待连接。首Demo未完成，不扩为完整安卓首版。 |

## 执行约束与负责人

- 用户已授权继续开发、真实场景修复／保存复开／限定验证。2026-10-08中央get_goal实际返回null（当前无活动Goal），旧paused记录不代表当前状态；按用户继续开发授权工作，不自行新增Goal或恢复五分钟自动任务。
- 同一产品共用玩法及存档。玩家UI用uGUI，编辑器用UI Toolkit；英／简中，预制体诊断占位符，Luban文本表／项目本地化绑定／YooAsset路线不变。
- Unity2022.3.18f1 Intel，工具／缓存优先外置盘。**只有C串行运行本机Unity**；本包准备阶段不启动Unity。每次原生执行遵循新包次数、时限、输出与进程范围，旧N07/N10/N11批准均已消耗。
- 主要负责人／设计／实施显式`gpt-6-astra/xhigh`；明确重复任务可Luna。代码审查只走GitHub PR，不派本地R；支持的自动化优先dots，本包Mac用例按限定例外准备。
- 一般完成链：作者／C → 主程 → 中央；当前成品验证包按主程明确交接改为C → 中央，只有中央收本次C。主测试按实际需要接审查／验证；闲置角色不常驻空转。不为常规技术选择重问用户。
- 本机Git基线记录为`125b849`并保留WIP；各审查候选经连接器发布独立分支，不能混同共享工程。新发布仅依准确manifest／Git权限，不自动合并或发布产品。

| 角色 | 实际thread（host均local，云例外） |
| --- | --- |
| 中央 | `01a0e401-511d-79f2-b47f-3ab0ade1681b` |
| 主程 | `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` |
| 主策 | `01a0f2e3-2c47-7273-88da-20513bcbfaf4` |
| 主美 | `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` |
| 主测试 | `01a0f2e3-4bd2-7130-b43a-19afa702e7bb` |
| 唯一C | `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c`，原C本轮停写；[交接](engineering-host-save-isolation-mac-handoff-001.json) |
| LOC | `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` |
| 云测试 | `01a0f13c-3d87-753a-ad11-16302add9084/durable` |

[协作规则](../../../.agent/TEAM_WORKFLOW.md) · [模型实际登记](model-switch-receipt.json) · [截至10月7日完整原快照](run-history-through-2026-10-07.md) · [10月1日以前历史](run-history-through-2026-10-01.md)。历史中的pending／暂停／尚未授权仅代表当时时点；恢复小包只读本页和相关证据。
