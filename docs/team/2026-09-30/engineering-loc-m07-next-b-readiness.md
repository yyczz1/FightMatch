# M07 之后 B 工具构建验证：静态就绪核对

2026-10-12；主程 actual `01a12dce-9e05-7c20-bd1f-bbbfc8a3edc7`，thread `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据中央 turn `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 人类 item `01a12dc8-5fb9-7e43-8659-7026352c2050` 的静态准备派发；中央唯一收件。**未查看 M07 运行目录或结果；M 是否通过/接收仍未知。本文不扩大 M07 许可，也不授权 B 执行。**

## 1. 当前事实：有合同，没有 B 执行器

固定只读基线 R2=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix05-review02`。prepare.json 记录 driver 205640B / SHA256 `ae1d2c3cb2a8cc1ea95d92fd5c7f3c5fce0eb90fcfa2235e9650023b9bd4ec5f`，model `de9f7e47850dcd2f6cdcb3e6f2c605b3d45975c2116dd3d19722cf16623b2580`；固定 SDK8.0.425 / Luban source commit `df34215d2e22035073d1af83d44e3ac0744dc3b3`。身份来自静态封签声明，本轮不另跑验证。

driver `command_plan`（340行）及模型已经声明 B 命令、53项命令预算与3888条 B 写规则；`validate_official_trx`（773）、`check_b_receipt`（789）、`assert_controls`（797）提供结果校验。它们不是构建调度器。

`verify_activation`（900）明确只允许 stage=M；`execute`（908）只走 execute_material；后者（1106）注明 B/L 仅为冻结合同，并要求新 M 根不存在。`run_material_command`（1045）只允许 M/maxChildren=0，其收据固定 childStarts=0。CLI（2750）没有 B build/run 入口，`check-b`/`assert-controls --stage B` 只是读证据。因此不能通过改 activation.stage、运行 check-b 或重启 M 来完成 B。

## 2. 下一包：B-SOURCE01，先补独立 B 启动与调度

唯一交付 owner 建议仍为 LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh，中央签发实际新 turn/item。这是建议包，不在本轮派发；作者不得与其尚在运行的 M07 回合并发接新执行任务。

仅新建源码根 E_B=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-b-source01` 下八叶：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`。唯一后置叶：`docs/team/2026-09-30/loc-m17-b-source01-command-receipt.json`，≤64KiB。总源码仍≤16MiB、driver≤256KiB；新根占用或实现不能落入此界限则报中央，不自动增文件。

R2、旧证据及所有外置 M/W 只读，产品八路径、Program.cs、Config、Generated、Assets/Packages/ProjectSettings 不在白名单。新包应固定本文与中央 B-SOURCE01 包、R2 源码/模型/契约身份；保留 R2 原快照，不就地放开 M 启动门。

最小源码工作：

1. 新候选内提供 B-only 启动/activation 门与调度入口，消费 R2 固定 B 合同；必须同时绑定新源码/成功封签侧录/准确审查 head 和**独立中央 M 接收记录＋实际 M manifest**。没有 M 接收时生产门关闭；SOURCE生成与纯fixture不得读取尚在形成的 M07 文件，也不能要求整个既存 M/W 根为空。
2. 从已接收 M 的 source-patched、runner/tests锁、31包本地feed、部署/许可证清单与harness进行三个新的 B phase staging。保留 W5 既有 M 区域，只有拟写 B 叶子必须新建/无污染；不重新下载、解包旧归档、生成 M 锁或修补第三方源码。B源码 prepare 不能直接复用 M 的“未来根全部缺席”前置条件。
3. 按冻结顺序执行53项 B 命令，保留其 argv/cwd/env、预期退出码、依赖与 writer。005 每侧既有11步按冻结列表串行；补齐 staging 与先后消费约束，不因原 dependsOn 只列publish便任意调序。结果先写真实进程回执，再判断后续是否可启动。
4. 为4条允许128个子启动的 publish/build/test 实现真实父子树监护、结束闭合和限额；其余命令保留0子启动。不能直接删掉 M 的 maxChildren==0 断言或把 B 收据仍写常量0。有限原流、真实OS退出、超时/超输出停止及只终止本次所属进程，沿既定语义实现，不另造通用任务框架。
5. 接通既有 restore/图与包闭包、完整部署/许可证、官方TRX及三侧输出比较校验；全部通过只输出 `B_CANDIDATE_NOT_ACCEPTED`。独立中央/QA接收另行产生 B acceptance，作者不能自批。入口与中央host调用/返回协议须有纯适配检查；如必须改host模板，另报精确额外叶子，不暗含在本八叶授权中。

## 3. 实际 B 范围与已定限额

沿 R2 prepare.json 的 `model.commands[stage==B]` 原顺序，共52条DUT＋1条harness；每条 maxStarts=1、retry0、killGraceSeconds=5，两路日志各≤4MiB，OS禁网/本地feed，maxConcurrentTrees=1。

| 固定动作 | 次数 | 每条 timeout | maxChildren |
|---|---:|---:|---:|
| 两个runner locked restore＋官方tests locked restore | 3 | 180秒 | 0 |
| 两次publish＋一次官方build＋一次无过滤官方test | 4 | 600秒 | 128 |
| yaml/unity-asset/malformed-yaml/fightmatch × oracle/candidate-1/candidate-2 | 12 | 300秒 | 0 |
| 005冻结11步 × 三侧 | 33 | 300秒 | 0 |
| B-assert-controls | 1 | 60秒 | 0 |

逐命令 timeout 合计16500秒，按声明另加53×5秒为16765秒；**这只是命令限额求和，不是已授权的 B 整体墙钟**。现模型 B 没有 wallSeconds；全 M/B/L `maxCumulativeCommandSeconds=21000` 也不是 B 总计时器。[stage-budget-fix01](engineering-loc-m17-stage-budget-fix01.md)只确定 M900秒并要求 B/L另签。中央实际B包须明确整体墙钟及收尾边界；当前不填新数字，不借M07的900秒或失败后重试额度。

未来材料根 M5 与工作根 W5 沿 R2 `model.roots`（外置 Luban 的 `5.1.0-fm-yamldotnet-16.3.0-m17-fix05` / `.work/m17-fix05`）不迁移。实际写域精确引用 R2 `model.writes` 中 stage=B 的3888条规则及各 writer：三个B phase、B controls、53项logs/receipts及其独立env；bounded-pattern保留原文件数/字节上限，不是任意目录写许可。W总≤4GiB/200000文件、M≤512MiB/25000文件、空闲≥2GiB不扩。

六条 stage=M/B/L 共享收据规则不是自动全部可写；中央实际包逐叶补签需要的新证据，旧 activation/material-validation/root-equality 不得覆盖。`b-acceptance.json` 由独立接收流程签发，不从作者成功stdout推导。M material、原 M 收据、oracle、SDK、原005证据及正式产品均零修改。

## 4. 验收与必要新检查

SOURCE验收：无M接收/pin、错manifest或source/sidecar/head、stage混用、扩大预算/写域均在任何B写入/进程前拒绝；53项合同及生产白名单保持，M/L入口不被顺带解锁。采用一次新的纯内存fixture差量检查，覆盖有效入口、拒绝先于写/启动、锁种子/新phase污染、命令顺序/次数、错stage/writer、错误OS退出、malformed非零且零输出、子启动>128/残留子树、超时/日志越界停止后续与真实失败记录。模拟不声称已验证真实native子树。

新增结果校验fixture只测接线及失败传播：缺/跳过/失败TRX、错部署/许可证/锁/包、三侧输出不等必须拒绝；有效记录到候选而非独立ACCEPT。复用未变的历史校验，旧81/66/15及REVIEW02整套不机械重跑。建议中央沿现有SOURCE模板明签一次≤10秒新回归、一次≤30秒机械封签及真实返回后侧录；本轮不消耗或授予这些次数。新head经GitHub PR Code Review和QA核收，Git由中央协调。

真实B验收：23节点/56边runner图、13节点/29边tests图及31包闭包保持；3次locked restore有效，完整两份runner部署及许可证符合M清单，不能只验三个DLL，也不能把旧runner的132项数量套用新候选；官方tests实际total>0且executed=passed=total、零失败/跳过/超时，保留CsharpCompileHarness；yaml、unity-asset、fightmatch、005两pass在三侧比较通过，三侧malformed均非零＋stderr诊断＋零发布。52条DUT收据完整、harness成功且所属子树静止，缺一项不是通过。

## 5. 可并行工作及真正执行前置

M未接收前可独立做：中央签B-SOURCE01、闭锁的B执行器源码、上述小fixture、源码封签及GitHub审查准备，全部只在新SOURCE域；不读取运行中的M目录、不制作B phase，不抢占唯一LOC执行回合。M实际manifest/接收身份留到中央确认后绑定，不能填预期hash冒充结果。

真正B依赖：中央接收M07且绑定真实源码/model/material/收据，相关进程闭合与材料只读；B执行源码及host协议就绪、准确新head独立审查通过；中央另外批准B动作/整体时限/新叶子并签独立activation。若M失败则保留源码准备，B不启动；若材料身份与R2合同不同，先报差异，不自动移植或重跑M。

B仅为许可证替代工具链的构建与行为验证，不是 LOC-IMPL-B，也不是正式 LOC-A 接收。[LOC-IMPL-A §4–5](engineering-localization-luban-impl-a.md)与[FIX03 §4、§6](engineering-localization-luban-impl-a-fix-03.md)的12自检、两clean pass、三负例、真实父进程回执与Generated发布仍属后续L/正式制作链；B通过不能跳过它们，更不解锁Host、Unity、Android或设备验收。

用时：静态查阅与分析截至174.325秒；初稿落盘并核对63行时总计295.567秒，写作/行数核对另计121.242秒。仅新建本文，未源码import/prepare、测试、SDK扫描、native、Unity、Git；未读取M07运行目录、轮询LOC、写运行目录或跨聊派发。
