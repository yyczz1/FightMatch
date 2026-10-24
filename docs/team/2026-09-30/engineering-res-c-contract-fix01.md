# RES-C-CONTRACT-FIX01 — 阶段与剩余工作一致

2026-10-03。依据用户AGENTS §6与中央明确批准本finding及C08无效正例的单处例外；issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ffb9-2af3-7ea1-8f13-324e2997561c`。唯一源码owner原作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh，新actual turn在激活后绑定。先C FIX01封存并回主程，再由主程另激活同作者D-RAW；本合同不授RAW、原生或审查权限。

唯一问题是[PR11 r4171468078](https://github.com/yyczz1/FightMatch/pull/11#discussion_r4171468078)，head `f99b5a60b9ecad62aa01ddb1333a7db8a05c662f`、review `PRR_kwDOTDyjrM8AAAABQcoF1Q`，定位`ResourceProgress`构造器旧241行，不是Inspection API。QA回执 `testing-resource-contracts-pr11-review-receipt.json`2578B/SHA `958117871c64f87f5c95379d5a32336a4fe82d04122ca8b61c0531207252b58d`；原finding/10pass记录不改、不自resolve、不另开本地review或旧head红测。诊断复用该明确反例及当前源码，只新增回归，行为留后续真实验证。

前像只取 `TestArtifacts/FightMatch/RES-C-CONTRACT-001/S/`，receipt14107B/SHA `bfca5fe3550f4f25198595de9bdc3368acbdac3122b704820ff365b8b6e5e2f2`。唯一两产品路径：`Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceRuntimeContracts.cs`（324行16451B/SHA `18d5695eef979e549581fc737b0fc25753de2cd47c8ebca06be0a4e428b6663c`）；`Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceRuntimeContractsTests.cs`（394行26749B/SHA `ffd0e2df85720ca23a812a2fde3808225eb17973fb4b252f5a632ef4947b2b78`）。public面/enum/Inspection身份/Permit逻辑与所有其它生产字节保持；无依赖/friend/asmdef/meta变更。

生产只在`ResourceProgress`原有计数校验后的stage门内修两条件：`ReadyFromCache`必须初始inspection.RemainingBytes/RemainingFiles均0（其RequiresNetwork已由既有契约约束）；初始正网络工作即使本次completed全量也不准Ready，使用已有Verifying/TransportVerified。`ConsentRequired`仍要求RequiresNetwork与当前Mobile，另要求本次已计算的remainingBytes和remainingFiles均>0。用现有ArgumentException及安全常量文案，不改计数/诊断/其它stage语义，不新增Inspection stage API。生产增删≤12行。

原C01–C10十方法及所有断言保留；仅C08枚举stage正控制中的ResourceProgress第一入参允许在ReadyFromCache分支换成`Inspect(bytes:0,files:0)`，其它stage仍用原inspection。该处≤6增删行，可用一局部值或内联条件，不改completed计算/断言/循环/其它参数；其余九方法、helpers及其余C08字节完全保持。新增且仅新增普通`[Test] C11_ProgressStageRequiresConsistentRemainingWork`（≤60行）：合法零工作cache及正余量Mobile consent正控制；拒初始正工作未完成/部分完成的Ready、初始正工作虽已完成仍Ready、初始正工作completed全量后的Consent。用原Invalid/Inspect helper，核异常类型/安全诊断沿旧惯例；正控制显式核stage/剩余量/ConsentRequired。候选静态11＝原10＋1，不把源码断言称为已通过。

两文件总增删≤80行；原C分配的生产450行/64KiB、测试500行/64KiB仍保持。新根仅 `TestArtifacts/FightMatch/RES-C-CONTRACT-001/FIX01/S/`，起始ABSENT，精确8叶：source下完整两候选＋before.json、after.json、source.patch、verify.py、static-checks.json、source-receipt.json，stage≤2MiB。before/after记录实际责任/授权、本文/原scope/QA/publication，冻结C原S八叶、B FIX03/S十五叶、D FIX02/S八叶、C原before的34共享guard与已封存联合M01十七叶（receipt46585B/SHA `cc0745a765cf2a8261036251c308d4d526b3762700e0aec9680130cbe01da60c`）；不扫描/冻结package cache或活投影，不写共享或旧证据。原C两自然243B meta仅沿联合回执保持，不能重生成/导出。

apply_patch实施；最多2次新标准库`python3 -B TestArtifacts/FightMatch/RES-C-CONTRACT-001/FIX01/S/verify.py`各≤30s，保留全部失败。只验固定输入/八叶/预算/两路径patch内存重放/允许C08入参例外/其余原文与11普通Test名；不执行C#/Unity/SDK/测试/discovery/旧verifier/网络/Git/代码review。超范围即BLOCKED，不压行、削断言或借RAW写权。封存仅SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING，依AGENTS §6一次send_message回主程/local/Astra-xhigh，交actual turn/receipt/patch/两源hash/11名；停止写入，不自行接RAW。

主程接回后中央更新同PR11并唯一交QA接新head；原C10/联合23仍只属旧输入。RAW源随后独立封存，最终另签固定C11＋RAW12＋B38＋D13＝74候选的一次compile/同一次分组T，届时以实际源与XML精确多重集为准，分别绑定D/C/RAW版本和代码门。若RAW真阻塞另报最小C独立验证选择，不自动追加运行。本包不触及真实资源/下载停止/LOC/Host/Scene/Android/设备或FIX04/M16权限。
