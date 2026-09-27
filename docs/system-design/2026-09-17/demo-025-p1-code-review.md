# FM-DEMO-025-P1 独立技术审查

VERDICT: NEEDS_FIX

Scope: PASS。规范轴通过；规格轴存在一项 P2：默认 UTF-8 替换使不同的已接纳字符串身份共享候选指纹。下述结论在实际 C 完成门成立后签发，不将技术准备等同于内容批准。

## 身份与完成门

- 冻结依据：system-task-packets.md r128 §256–259；§260 仅登记本次派发身份。签署整稿 SHA256 c04feb2e0d35ec08f725c407b2895915a149f94020791030a181bb50bda5563c；冻结片段（CRLF→LF、TrimEnd 后加 LF）SHA256 aa1219c85e2ebc655dedc7fdf6f4479d74b92f2318cf58a0d72c1a9a2ede6c21。
- C 任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本批 turn 01a0ce47-0fb4-7c43-9717-1085de8fac27。原生 task_started：2026-09-23T12:39:18.771Z。
- 准确原生记录：physical line 11021 为 2026-09-23T14:24:49.870Z 的 phase=final_answer、delivery=null 正式 final；line 11025 于 14:24:50.427Z 记录同一 turn 的 task_complete。wait_threads 随后亦返回 completed、error=null。final 明确两包各 COMPLETED、四文件绝对路径/SHA及未运行边界；R 已核实四 SHA 与实物一致。正式裁决在此完成门之后。
- R 任务 01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次 turn 01a0ce47-ca8d-7fe1-87a5-be86ac97c3ea，2026-09-23T12:40:06.571Z 开始。C/R 的实际 turn_context 均为 local 工程、gpt-6-astra / max。
- 作者交付 demo-025-p1-delivery.md：7479 字节、59 行，SHA256 887488d14380b4473e4f8193a0fe61bf02a838c235e1e452fea7d82dfbac9fa3。
- 作者 scope：1409379 字节，SHA256 b9dec2044bdc5232fd1959676d064c8f13cdcb7c069226e0a5a12b6c721e24a6。两文件在 027 入口前完成；最终再次逐项核对。
- R 自行审查规范和规格两轴，仅写本报告与 demo-027-code-review.md；未运行 Unity、项目测试或新业务探针，未修源码、调用代理/外部模型、写 Git 或发送协调通知。code-review 技能的代理流程由 §259 明确覆盖。

## 唯一阻断项 F1（P2）

位置：[DemoContentCompiler.cs:192](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Content/DemoContentCompiler.cs:192)，Canonical.S 的 190–194 行；已审源码 SHA256 29b699aa0e264de686fe07c4135733dbe7de67e55e5f7df25600a90e568e2c15。

具体输入：使用同一个仍有效的 DemoContentJob，准备两份其余字段相同的合法 DemoContentTestData.MakeInput()，仅将 PlayerId 分别设为 C# 字符串 `"P\uD800"` 与 `"P\uD801"`（两个不同的未配对高代理项）。

预期：非法 Unicode 被明确拒绝并返回 InvalidValue，或不同的已接纳字符串被无损编码成不同规范字节。不能在候选对象仍保留不同身份时得到相同指纹。

实际：`Encoding.UTF8.GetBytes(value)` 使用默认替换回退，两字符串编码成相同的替代字符字节，长度前缀也相同，因而整个 SHA256 输入及 Fingerprint 相同。GrowthChecks.Text（CandidateGrowthDefinition.cs:85）和 BattleEntryPreparer.Text（:257）只检查非空白；CreateCandidate 及入场准备仍保留各自原 PlayerId。因此 Character.PlayerId / Entry.PlayerId 不同，而编译器生成的上下文指纹相同。此处影响开放字符串的实际语义绑定，违反 §256 的规范编码责任。

证据性质：这是沿最终源代码和既有验证路径得到的静态结论。R 未执行复现程序，也未把静态推导称为失败测试。现有 3402 个通过用例没有覆盖这个输入对；全绿不能排除此缺陷。

## 最小纠正任务包提案 P1-C1（由 SD00 另签派发）

目标：仅消除 F1 的有损字符串编码，保留所有合法候选和现有功能。

- 可修改的产品/测试仅为 Assets/Scripts/FightMatch/Content/DemoContentCompiler.cs、Assets/Tests/EditMode/FightMatch/DemoContentCompilerTests.cs；两文件现有 meta 保持。纠正证据根与新交付/审查文件由 SD00 另签，禁止覆盖本批冻结证据或作者报告。
- 在规范字符串入口采用严格 UTF-8 并将 EncoderFallbackException 转成明确 InvalidValue，或采用同等无损方案；错误不能从公共 PrepareCandidate 入口漏出。合法 Unicode、数值、语义顺序和来源排序保持原约定。
- 增加最小回归：上述两个未配对代理项输入；合法 U+FFFD 和合法代理对输入仍按公开契约处理。检查拒绝码/无候选，或对被接纳不同字符串检查不同指纹，并核原身份没有被悄悄替换。
- 禁改 Core/Application/Presentation、027、已有测试断言、asmdef/配置/依赖/资产/旧 meta、参数提案、坐标选择、PlayerSave 与正式发布能力。不在此纠正里处理其他功能。
- C 保存修正前可复现证据，再按 §258 固定 Unity 命令执行干净编译和无 filter 全量 EditMode；当前 3420 具名用例与旧断言保留，交新增回归、真实退出、源码/DLL/XML 同版、范围及新归档晚绑定。R 仍只独立审查，不代跑探针。
- 保留全部安全工作和失败历史；不要求整包回滚，也不授权 Git 或删除证据。此提案不是已派发实施包。

## 规范轴与范围

B16 起点 637 项 canonical=14e2906fb75fd6b46cc4b174214d892caac6efea2afbaad792b7e7f2cf71ab8d。R 从原 scope 独立逐路径核验，未重捕起点掩盖修改。

P1 新增 19 项，旧文件仅 Core.Tests.asmdef 追加 FightMatch.Content 引用，原字段/顺序及 meta 保持；其余 636 项原字节一致。阶段 656 实施项、355 唯一 GUID。生产 582 行、测试 578 行，均在预算内；E17 两 helper 共 159 行。Content runtime/noEngineReferences=true，只引用 Core、FlowPuzzle.Core、FlowPuzzle.Validation；没有真实玩家写句柄、Unity/QFramework/Platform/Application 依赖或发布成功接口。

最终 P1 canonical=9c34b62661ae2fb8b8f5bf12ae4a51ea675c74b860f443a8957523e061463280。027 仅获准覆盖原 BoardElement；P1 其余 655 项（含本项问题位置和全部 P1 测试）在组末保持。全组相对 B16 仅两项获准旧修改、635 项原字节保持、新增 31 项，未将 027 的合法覆盖误判为 P1 污染。

## 规格轴逐项核对

| 条目 | 独立结果 |
| --- | --- |
| P1-01 闭包与来源 | L1/L3 × BottomLeft/TopLeft 四份候选、完整两对几何及 6/16 占格成立；缺方向、映射、参数和不支持技能有拒绝。来源字节/SHA/定位及语义顺序已核。字符串指纹存在 F1，故本项未闭合。 |
| P1-02 精确参数 | C=1、1/2、1/4 的 E=1、3/2、71/32 和 rho=1、2/3、32/71 符合公式。31 行相邻 10^12 网格夹根、带符号误差及 2/3/10 次指标齐全；独立整数生存乘积与 2^m 路径枚举没有重复生产 DP。共享预算、极小 C 和零容差拒绝有证据。 |
| P1-03 真实随机/回放 | 执行前声明的 48 字节 00..2f、三域不同初态和 f=0；四候选实际 012/014 逐步求值、历史追加和 ReplayRecorded 新求值匹配，均完整 NormalVictory/WonPendingSettlement。L1/L3 最终 W HP95，实际奖励 XP26/31；L3 暴击杀 A 后合法跳过重复 WA。 |
| P1-03 来源边界 | 固定材料第一步真实字 543365255、978536786，第二步 3275454552、309975010；64 个预先固定来源全部保留，4 个暴击跳步、60 个普通补击。没有按首胜停止后伪称无条件随机，L3 为隔离候选而非 L1→L3 自然进度。 |
| P1-04 隔离/修订 | 已准备结果的嵌套来源、路线、参数、奖励深复制；开始/循环/结束 token、取消和 r8→r9→r10 同值改回均检查。IsCurrent 仅标明当前性，CommitEligible 恒 false。 |
| P1-05 审定资料 | 11 个来源实际 SHA 和四候选账本、31 行 C/ε、实际重放与 publication-proposal.md 完整；首 Demo 仅 L1、L3 隔离及新档 W Lv1/xp0/空库存/无主动/仅 L1 开放均明确为未审提案。 |
| P1-06 回归/范围 | P1 全量 3402/3402，原 3304 具名多重集合（含 83 个 G1）及旧断言保持，新 98 个；027 全量进一步保留这 3402 个。范围、真实进程及同版证据通过。 |

## 运行和冻结证据

E17=TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526；本包证据根 E17/stage025p1。

| 运行 | 实际进程与结果 |
| --- | --- |
| compile-01 | PID38052，13:09:46.1981979Z→13:10:04.8872438Z，退出1；新测试 Input 名称冲突与 Random.Battle 属性引用错误，原失败输出保留。 |
| compile-02 | PID9524，13:11:18.6793550Z→13:11:55.0818209Z，退出0、编译错误0；仅新 meta 的自然导入字段补全，GUID 不变。 |
| tests-01 | PID21612，13:13:02.9353502Z→13:20:52.8627915Z，退出0；3402/3402、失败0、跳过0。源码前后相同，未加 -quit/filter。 |

以上 UTC 均为 2026-09-23。每轮 precheck 无已有 Unity，固定 2022.3.18f1 EXE SHA256 ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895，Hidden 启动；argv/cwd/PID/start/end/实际 Process.ExitCode/stdout/stderr 均核对。前后清单的每一物理副本实际核验：982→994、994→994、994→998，无遗漏或多报。

P1 36 DLL/PDB canonical=74322e85e0a5081ac6b3ec5afa9967d9355bb1ad299f2a066d78af6ab486dfc7；有效编译后与测试前后相同。XML 2375503 字节，SHA256 8c135a35d44b282dfc11f87463660764861d693d365c3fd4793bd0739b0caffe；具名集合 Ordinal 排序+LF SHA256 d1dafe7bfcc31bf8213e01e285ba4704d74118348b1e181f738bc547d199e59d。R 独立解析 XML 和新测试方法，未仅采信总数。

R 于 13:31:22Z 实际逐文件核对新证据源/payload/ZIP：10365 文件、806953592 字节、34 空目录，三集合一致，canonical=76a6a2e61a07f75a70d5a1527653b950ffc73d1ffeb45b449ae3ec574b3f43d0。ZIP 129074477 字节，SHA256 3ddd1711fb41ebf00ab8604783543bb877c744a4523dd52c44191b3ece148f70；两自然新 B12/B13 根完整纳入。归档源前后时间/字节相同。

late/bindings.json 5714 字节，SHA256 8de65ed2d872e91586fbe26a0825cd269c3b26e2b0780f40ff9951aa30ae922e，19 项实际路径/字节/SHA 全匹配。late/native.json 的 73 条实际 CommandExecution/FileChange、2 个 context、1 个 task_started 与原生物理行 9898–10435 对应；没有把尚未发生的完成写入该阶段证据。组末完成门另按准确原生 final/task_complete 核实。

R 于 14:23:53–59Z 复核独立保护入口493项：490项原字节不变，三协调稿仅为已登记的 SD00 r129/§260 派发记录更新；§256–259 冻结哈希仍相同。没有将协调记录更新误记为 C 越界。

R 于 14:25:33.4569141Z 完成物理元数据复核：原67个历史根308409文件与12:44:33的独立入口逐组一致；P1完整证据根及其两处自然根共19742文件，与13:37:10独立冻结一致；两helper的字节/SHA/mtime不变。合计71分组328153文件，与C入口/出口清单对应。只采旧文件路径/长度/mtime，没有重复全字节验证旧多GB历史；全部本批新归档另按实际字节独立核验。作者两阶段正式报告、P1提案与本批代码在完成门后再次核对无漂移。

## 保留边界与下一步

须先完成 F1 的窄纠正再复审准备阶段。正式坐标、身份映射、31 行 C、ε=1/10^9 和新档来源仍待内容审定；本报告不代主策划作决定。publication-proposal.md SHA256 fae6eabd82df49a01138a8e881fe1d9b99cef9712f9b0797e01dc298de5822a5。

Publish、ResolveExact、PlayerSave 新档桥接及其持久恢复仍 NOT IMPLEMENTED；025 整体与 B17 不因此闭环。Player 构建、物理鼠标/触控/真机、交互式 Editor/PlayMode 和完整 Demo 未验证。027 可按自己的独立报告处理；不得因本项修正扩大其范围或提前放行 028。
