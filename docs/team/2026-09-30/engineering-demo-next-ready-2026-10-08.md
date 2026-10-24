# 首 Demo 独立小包就绪核对

2026-10-08 · **NONE（本次约束内无已就绪源码／场景小包）**。
主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`；fresh actual `01a11a30-f8c1-7a50-9a56-f8d655d1f939`（read_thread 核实）。中央唯一收件；本文不是执行授权或代码审查。

## 已核事实与硬依赖

| 候选 | 当前事实；阻止直接派发的硬依赖 |
| --- | --- |
| 正式文本 parser／adapter | [B 拆分合同](engineering-localization-luban-impl-b-dependency-split.md)明确：真实 LOC-A artifact／manifest／evidence 独立接收前，B-CATALOG 的 parser、adapter、测试 builder 不得实施或冻结。现有 `LocalizedTextSource.cs` 是接口／结果类型，不能用草稿 CSV 或测试源充当正式 catalog。 |
| 正常 Host 启动接线 | 实读 `Assets/Scripts/FightMatch/Host/FightMatchPlayerHost.cs:88` 仍传 null；104–108 在 `LocalizationNotReady` 时退出，Session 创建在138行之后。`LocalizationService.cs:12` 的就绪条件为非空 source。硬依赖为正式文本 adapter、真实资源准入输入及加载／释放接线；现偏好初始化已存在，不再拆出一个重复偏好包。 |
| 真实 BuiltIn 包／生产 factory 装配 | G01/G02 的源码采用已完成，不重复。缺正式 LOC 两 raw、六业务 raw 的固定来源、获准 UI 对象闭包、真实 build receipt 与独立 Player pin；[内置准入策略](engineering-res-built-in-admission-policy-001.md)仍是设计候选，不能把 fixture 值或空装配壳当产品交付。资源组合 I/T89 尚未完成，PR20 V08 FIX01 新 head 审查由中央接收。 |
| Scene／布局修正 | [后续依据第1节](engineering-post-host-compile-next.md)与[N11R1差量结论](engineering-layout-n11r1-serialized-delta.md)没有支持再删五条覆盖或导入诊断后像的事实。PR4 P1尚待原Scene自然首次布局及正常交互证据；后者依赖非空真实会话。本轮不运行 Unity／探针，不能凭静态推断制造一个场景修复包。 |
| Android API24／GLES3 | [API24 源码接收](android-api24-source-integration-receipt.json)已完成，明确不含共享采用／保存复开；现共享设置172行和构建器2262行仍为22。采用将改变冻结1036输入，违反本次约束。GLES3 [既定包](engineering-android-graphics-001.md)另要求 post-uGUI／post-API24 固定起点及真实保存复开，不能提前改共享两文件或重做已收API24源码。 |

## 给中央的下一动作

维持当前 V08 FIX01 审查收件与既定后继，不另造“可并行”产品包。要开启独立产品实施，至少须满足上表对应依赖，或由中央另签解除1036冻结的准确采用窗口；本结论不把所有工作笼统归因于 LOC，也不要求重跑旧验证。正式 LOC 的审批阻塞保持，不换执行器／来源绕行。

本次仅阅读既存源码、合同和接收记录，新增本文；未复审代码、重跑测试、联网、探针、Unity、派工或修改资源固定输入。writing-for-agents 用于把结论、硬依赖及解除条件压为单页；未重新设计。
