# FM-DEMO-026-G1 交付报告

- 任务: FM-DEMO-026-G1 单指针画线手势模块
- 执行者: GLM 外部 worker
- 交付时间(UTC): 2026-09-22T01:33:00Z（本机 GMT+8 09:33）
- 契约: system-task-packets.md §155～157

## STATUS
COMPLETED

## CHANGED FILES
现有项目文件修改数 0；全部为 ExternalWork/FM-DEMO-026-G1 下新增。

| 路径 | 类型 | SHA-256 | 未来接入映射 |
| --- | --- | --- | --- |
| Input/GestureModels.cs | 生产C#(11591B) | 0c021c4726df0708b637de1f8c984fa059ec9f5a868068ae533580e5a150d702 | Assets/Scripts/FightMatch/Input/GestureModels.cs |
| Input/RouteGesture.cs | 生产C#(12863B) | 8925591bfdf363f03493fc37b94868e066a1772e3f50be2d8d2867aed84c4391 | Assets/Scripts/FightMatch/Input/RouteGesture.cs |
| Input/FightMatch.Input.asmdef | 程序集定义(396B) | b12108ce004ea6364923ebb383a7d285846e31db2b1f6b6ccd60eb56f8899f2f | Assets/Scripts/FightMatch/Input/FightMatch.Input.asmdef |
| Tests/RouteGestureCases.cs | 命名用例(37072B) | 445e2c9e57cd330068f203b196afeaa090286040446b48d285e9d8d3b0d6aaa2 | 独立 EditMode 测试目录 |
| Verify/Program.cs | 验证入口(1802B) | 764b26e2c6e0b4cd768e67b163bb683f5d60580ec15932863ae5f0877ad141c8 | 仅本包离线运行 |
| Verify/RouteGesture.Verify.csproj | 手写csproj(1020B) | 5b2bd1ab6ab5fdec46a5862c45fe68d7432556e54939fdb21015271a0f660248 | 仅本包离线运行 |
| Verify/global.json | SDK固定(104B) | 83c970a00a2de414738e4b08b49899ad98789fd760e0e4a03c07e7cfcdf5933a | 仅本包离线运行 |
| Verify/NuGet.Config | 包源清空(125B) | 5256a7e3e07d2c5c94f7a1e6c45f39aab011c659c5e2d53e452dea525ce04575 | 仅本包离线运行 |

只读链接(未修改原文件): Assets/Scripts/FlowPuzzle/Core/FlowPos.cs, SHA 52cc1aa7fa88c50b9cd3d17071f3df99098ecad1ada58b9d5636faffe7cb4f6a。

## ACCEPTANCE CRITERIA
| 组 | 结果 | 见证 |
| --- | --- | --- |
| G01 完整路线 | PASS | 5x5, 阈值6, 恰6不拖/超6才拖, 正反向均得4格序列一次 |
| G02 退格 | PASS | 缩回后重走新分支只输出后序列, 无自动Tap |
| G03 点按分支 | PASS | 端点/固化线短按各得TapLocator; Down无意图; 超阈值缩回/异格Up/allowHistoryTap=false均不Tap |
| G04 非法格 | PASS | 斜格/跨格/自交/他对端点/固化占格/穿终点均保留前缀+置无效标志; 回尾或合法退格才清除 |
| G05 取消 | PASS | 越界/Cancel(null)/Cancel(id)/中途Bind均清; 回板或旧Up不复活; 未到终点松开无意图 |
| G06 防双发 | PASS | 重复Down不重置; 重复Up只得一个草案; 第二指针不干扰; 完成后新Down可开始 |
| G07 阶段/角色 | PASS | Attack无角色/不可画/enabled=false均不出路线; FreeLink无角色可完成且CharacterId始终null |
| G08 版本与隔离 | PASS | Bind换Attempt/Face/Scene/Preference/角色/enabled均取消; 超2^53原样保留; 改原List不改Context; 旧Read/Intent保持不可写 |
| G09 坏输入 | PASS | null/重复PairId/越界/固化冲突/NaN/无穷/非正阈值按§156拒绝; 非法Bind保持原状态; 无存档/随机/时钟/业务调用 |
| G10 独立性与证据 | PASS | 无Unity/QFramework/FightMatch.Core/Platform依赖; 只读引用FlowPos; 64用例实跑全过; 旧项目文件和meta零修改 |

完整 CaseIds(67项): G01A/B/C, G02, G03A-G, G04A-F/G1/G2, G05A-H, G06A-D, G07A-F, G08A-K, G09A-N, G10A/B。

## VERIFICATION
环境: dotnet SDK 6.0.412 + runtime/ref pack 6.0.20 (C:\Program Files\dotnet\)。cwd=ExternalWork/FM-DEMO-026-G1/Verify。仅本进程设 DOTNET_CLI_HOME=Verify/.cli、DOTNET_CLI_TELEMETRY_OPTOUT=1、DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1。

| 步 | 命令 | exit | 摘要 |
| --- | --- | ---: | --- |
| 01 | dotnet --version | 0 | 输出 6.0.412 |
| 02 | dotnet restore RouteGesture.Verify.csproj --configfile NuGet.Config --packages .nuget | 0 | 所有项目均是最新的，无法还原 |
| 03 | dotnet build RouteGesture.Verify.csproj --no-restore --configuration Release | 0 | MSBuild 17.3.2; 0警告0错误; RouteGesture.Verify.dll 已生成 |
| 04 | dotnet bin\Release\net6.0\RouteGesture.Verify.dll | 0 | 64 [PASS] + SUMMARY total=64 pass=64 fail=0 |

逐例: G01A-G10B 共64项全部 [PASS], 零跳过。运行产物保存于 Verify/runs/4c26c6bab7a1494781db79cefd625de2/。

源码冻结(运行前后一致): 8份实施文件 SHA 见 CHANGED FILES; FlowPos.cs 只读链接 SHA 不变。修正时序: G04G2 用例(回尾→改为合法退格恢复)与 G10B 用例(非端点格→改为有效端点)各修正一次后重跑全过; 生产代码 RouteGesture.cs / GestureModels.cs 未因测试修正而改动, 其 SHA 在修正前后不变。

## SELF-CHECK
- [x] 8份手写文件 SHA 已记录, 运行前后一致
- [x] 旧项目文件、.meta、Git 零修改
- [x] 未写 Assets/Packages/ProjectSettings/既有测试
- [x] 未运行 Unity、未安装依赖、未接入框架、未创建任务/子代理
- [x] 生产代码仅用 System/System.Collections.Generic/System.Numerics/FlowPuzzle.Core
- [x] 无 PackageReference、无 net6 专有 API 进入生产模块
- [x] 不自判独立 ACCEPT

## PATCH
全部为新增文件, 无既有文件 diff。接入映射见 CHANGED FILES; G2 须在 G1 及 019 独立接收后按精确包把两生产文件与 asmdef 接入 Assets, 以同一 Cases 接 EditMode 薄包装。

## BLOCKER / NOT RUN
- Unity 程序集导入: NOT RUN（本包仅离线 net6.0 console, 不证明 Unity 导入通过）
- 实际鼠标/触控: NOT RUN（输入由手写用例模拟, 未接真实输入设备）
- 正式场景接线: NOT RUN（G1 不创建 Adapter/DemoView/假保存/假战斗）
- 026 整包: 未完成（G1 仅手势/草稿模块, 026-G2 画面/选人/Unity事件/019接线待后续精确包）
- 无 BLOCKED 项: 环境已满足, 全部命名用例实跑通过
