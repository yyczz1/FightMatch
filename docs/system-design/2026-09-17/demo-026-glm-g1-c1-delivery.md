# FM-DEMO-026-G1-C1 交付报告

- 任务: FM-DEMO-026-G1-C1 窄范围修正包
- 执行者: GLM 外部 worker
- 契约: system-task-packets.md §170；依据 R 报告 demo-026-g1-code-review.md F1～F5/N1～N3
- 原交付 SHA: delivery=1bd9fbcc…, scope=10fa2104…
- 红阶段 run: 035cc6dcdc2e476683141d048e9850dd（原生产 SHA + 仅改测试）
- 绿阶段 run: 98e0c5e66d2548afa748042fd6e72982（修正后生产 + 测试）

## STATUS
COMPLETED

## CHANGED FILES
仅修改 3 份 C#，其余 5 份配置/程序零修改，原两报告和 R 报告一字不改。

| 路径 | 原SHA(§163) | 修后SHA | 变化 |
| --- | --- | --- | --- |
| Input/GestureModels.cs | 0c021c47… | c18257cd… (12570B,304L) | F2/F3/F4 + 删未用using |
| Input/RouteGesture.cs | 8925591b… | 51478f04… (13753B,393L) | F1/F5 + 简化Read/BuildRouteIntent |
| Tests/RouteGestureCases.cs | 445e2c9e… | 21a7fa70… (50447B,1035L) | N2 补测 + F1-F5 反例 + 增强断言 |
| Input/FightMatch.Input.asmdef | b12108ce… | 不变 | — |
| Verify/Program.cs | 764b26e2… | 不变 | — |
| Verify/RouteGesture.Verify.csproj | 5b2bd1ab… | 不变 | — |
| Verify/global.json | 83c970a0… | 不变 | — |
| Verify/NuGet.Config | 5256a7e3… | 不变 | — |

FlowPos.cs 只读链接 SHA 52cc1aa7… 不变。生产 697 行(≤700)，3 C# 合计 1732 行(≤1900)。

## ACCEPTANCE CRITERIA — F1～F5 / N1～N3

| 项 | 状态 | 修正与证据 |
| --- | --- | --- |
| F1 Up 末样本位移 | PASS | Up 新增 ExceedsThreshold 调用，与 Move 同规则处理松手样本后决定结果(G11A/B PASS) |
| F2 输出构造与冻结 | PASS | GestureIntent 构造收窄为 internal；GestureView/Intent 构造复制集合并 AsReadOnly 包装(G12A/B/C PASS) |
| F3 null Pair 异常 | PASS | pairs 复制前逐元素 null 检查抛 ArgumentException(G09O PASS) |
| F4 非法 Mode | PASS | 构造时拒绝 Attack/FreeLink 之外的枚举(G09P PASS) |
| F5 极端有限 double | PASS | ExceedsThreshold 按最大维度缩放避免平方上溢/下溢(G15A/B PASS) |
| N1 四标记 | PASS | .runguid* 归档至 runs/98e0…/original-markers/ 后删除原处(见 PATCH) |
| N2 缺失见证 | PASS | 恢复 G04G3(原点回尾)+G04H(普通尾回尾)；G04I(相邻自交)+G04J(固化中段)；G01/G02 完整逐格断言；AssertThrowsExact 精确异常族 |
| N3 历史缺证 | NOT VERIFIED | 原旧证据缺口(阶段SHA/UTC/stderr)仍 NOT VERIFIED，不补造；新报告纠正 67→64 |

## G01～G10 逐项结论（绿阶段 80/80 PASS）

| 组 | 结论 | 说明 |
| --- | --- | --- |
| G01 | PASS | 正反路线完整逐格断言 + Up 末样本位移(G11A/B) + 极端阈值(G15A/B) |
| G02 | PASS | 退格重走完整逐格断言 |
| G03 | PASS | 点按分支 + Up 恰阈值不误拖(G11C) |
| G04 | PASS | 非法格 + 回尾恢复(G04G3/G04H) + 相邻自交(G04I) + 固化中段(G04J) |
| G05 | PASS | 取消系列不变 |
| G06 | PASS | 防双发不变 |
| G07 | PASS | 合法 Mode 阶段/角色不变 |
| G08 | PASS | 版本隔离 + 输出集合冻结(G12B/C) + 构造可见性(G12A) |
| G09 | PASS | 坏输入 + null Pair(G09O) + 非法 Mode(G09P) 精确异常族 |
| G10 | PASS | 独立性不变；N1 标记已归档删除 |

## VERIFICATION

环境: dotnet SDK 6.0.412, cwd=Verify, 进程级 CLI_HOME/NUGET_PACKAGES 限新根。

| 阶段 | run GUID | version | restore | build | run | 结果 |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| 红(原生产+改测试) | 035cc6… | 0 | 0 | 0 | 2 | 69 pass/11 fail(9 F1-F5 + 2 测试bug) |
| 绿(修正后) | 98e0c5… | 0 | 0 | 0 | 0 | 80 pass/0 fail |

红阶段 11 失败: G04H(测试用相邻格非斜格)、G09O/G09P(F3/F4)、G11A/B/C(F1)、G12A/B/C(F2)、G15A/B(F5)。
其中 G04H 与 G11C 为测试断言错误(非生产bug)，已修正后绿阶段全过。

## SELF-CHECK
- [x] 仅 3 份 C# 修改，其余 5 份/原报告/R 报告/runs/Assets/Git 零修改
- [x] N1 四标记 SHA 验证后归档删除，未通配删除
- [x] 红阶段在原生产 SHA 上暴露 F1-F5，绿阶段修正后全过
- [x] 历史缺证(N3)如实标 NOT VERIFIED，不回填旧 UTC/命令/stderr
- [x] 原 delivery 67→64 已纠正
- [x] 不自判 ACCEPT；Unity/触控/场景/026 整包 NOT RUN

## PATCH — 新增文件映射

| 新交付 | 路径 |
| --- | --- |
| 本报告 | docs/system-design/2026-09-17/demo-026-glm-g1-c1-delivery.md |
| 范围清单 | docs/system-design/2026-09-17/demo-026-glm-g1-c1-scope.json |

N1 标记归档映射:

| 原路径(已删除) | SHA256 | 归档至 |
| --- | --- | --- |
| Verify/.runguid | c0a05e90… | runs/98e0…/original-markers/.runguid |
| Verify/.runguid2 | 69f1286a… | runs/98e0…/original-markers/.runguid2 |
| Verify/.runguid3 | ddbe1d2d… | runs/98e0…/original-markers/.runguid3 |
| Verify/.runguid4 | 111f0089… | runs/98e0…/original-markers/.runguid4 |

新增 16 CaseId: G04G3, G04H, G04I, G04J, G09O, G09P, G11A-G11E, G12A-G12C, G15A-G15B。总计 80(64+16)。

## BLOCKER / NOT RUN
- Unity 程序集导入: NOT RUN
- 实际鼠标/触控: NOT RUN
- 正式场景接线: NOT RUN
- 026 整包: NOT RUN（G2 仍等 019 独立通过 + SD00 精确接线包）
- 历史冻结证据(N3): NOT VERIFIED（旧 stderr/UTC/阶段SHA 缺失，不补造）
- 无 BLOCKED 项: 环境满足，80/80 实跑通过

交付后停止修改，由 SD00 安排独立复审(R-G1)。
