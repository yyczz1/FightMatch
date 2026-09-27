# FM-DEMO-026-G1-C2 · Codex 直接修复交付

STATUS: COMPLETED

用户明确要求“由你来修复，不再交接给glm了”，本轮由 Codex 直接实施上一轮报告的窄范围修复，没有交接或调用 GLM。
F5 两处数值缺陷已关闭：本轮83个注册用例全过；使用原独立探针源码重新编译，23个探针全过，原C16～C19四个失败全部转绿。
这是实现与本地验证交付；没有把自行实施后的检查描述为另一位审查者的独立接收。

## CHANGED FILES

| 文件 | 本轮修改 |
| --- | --- |
| [RouteGesture.cs](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Input/RouteGesture.cs:247) | 只替换私有阈值计算，新增私有有限double转整数辅助方法及已有框架的System.Numerics引用。公开API、Move／Up状态机不变。 |
| [RouteGestureCases.cs](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Tests/RouteGestureCases.cs:103) | 新增G15C／D／E并注册；移除C1已造成无调用的AssertThrows。原80个用例的方法体及断言均保留。 |

新建本报告、[结构化scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-026-codex-g1-c2-scope.json)，以及 [本轮证据目录](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Verify/runs/953cfd631d8f43ac98bfd5f4e8e271a3)。
证据目录以下简称R；工作目录始终为 `D:/Unity/UnityProj/FightMatch`，实现仍在 `ExternalWork/FM-DEMO-026-G1`。
其余六个实施文件、FlowPos、原报告／scope、两轮独立审查证据、旧runs／bin／obj、Assets／meta／Packages／ProjectSettings均未修改。
没有Git写入、分支、worktree、Unity、网络工具、依赖安装、子代理或跨任务派发。

| 文件 | 修改前SHA256 | 修改后SHA256 |
| --- | --- | --- |
| RouteGesture.cs | 51478f0439b92b2d00909e5d468505c69b39034ce2679459f28dcdd1f4d25da4 | d6a95368ba46a307bac2cf03120a1c5e7a90ed24b94fe7866d91cb3dd1459e30 |
| RouteGestureCases.cs | 21a7fa709f7148126c538954d17a1b79ec0e22798bd36c9426b2f38a89b8cd67 | 7e0b1739d7014e645d4456a1178425d6f9e2c988b4480812830fc158676c114c |

## 修复原理

原实现先以double相减，再缩放、开方、乘回量级。有限坐标相减可溢出为Infinity，后续Infinity/Infinity形成NaN；极小距离乘回量级又可能舍入到阈值，使严格大于关系丢失。
现在把有限double按其IEEE 754位模式转换为以2^-1074为单位的精确BigInteger，再进行坐标差和平方比较。
次正规数使用原尾数；正规数补上隐含最高位并左移指数减一；最后保留符号。所有合法输入因此拥有相同精确整数尺度。
比较 `dx² + dy² > threshold²` 时共同尺度抵消，无浮点相减、开方或乘回舍入，也不加容差、不改float、不缩小合法输入域。
BigInteger原已用于本模块Context修订，没有增加程序集配置或外部依赖。阈值计算仍只位于原有未越阈值判断路径。

## ACCEPTANCE CRITERIA

| 项目 | 本轮结果 |
| --- | --- |
| F5a：有限坐标差溢出 | PASS：G15C及原C16／C17／C18；Move进入Dragging，原格Up不Tap，相邻终点仅在Up出现仍得完整两格AttackRoute。 |
| F5b：次正规对角位移 | PASS：G15D及原C19；最小正double在两轴的对角位移严格超过同值阈值。 |
| 严格阈值及全量级边界 | PASS：G15E覆盖最小次正规／1e-200／6／1e200／MaxValue的等值与下一可表示值、微小正交分量、3-4-5及5-12-13精确三角形的四档量级、极小负起点到MaxValue、极大坐标零位移、反向Y轴差溢出。 |
| Move与Up一致 | PASS：新增27组数值见证逐组检查Move、原格Up、相邻终点Up；没有把这些子场景另计为CaseId。 |
| F1～F4及原行为 | PASS：原80个用例保持并通过；原23个独立探针全部通过，冻结模型／参数异常／状态机行为保持。 |
| N2注册与长期回归 | PASS：83个唯一Ordinal CaseId，旧80全部保留，新3个G15C／D／E；注册、switch、红／绿实跑多重集合一致，零跳过。 |
| N1当前文件状态 | PASS：旧标记归档仍在、原位置仍不存在；本轮未操作它们。旧先归档再删除时序仍NOT VERIFIED。 |
| N3本轮证据 | PASS：编辑前原字节归档、红绿原字节、每步完整命令／cwd／环境／UTC／stdout／stderr／实际退出／前后SHA／产物SHA均保留。旧历史限制见下节。 |
| G01～G03 | PASS：完整正反路线、完整退格重走、Tap条件及极端数值末样本路线均通过。 |
| G04～G07 | PASS：非法格、恢复、取消、绑定、指针所有权、防双发、角色及FreeLink等原回归全部通过。 |
| G08～G09 | PASS：版本／集合隔离、合法枚举、null Pair和其余规定坏输入回归通过。 |
| G10当前实现与本轮验证 | PASS：纯模块、FlowPos只读Link、同版离线构建及全部注册实跑；不能据此补证旧G1／C1执行历史。 |

## VERIFICATION

修复前先核当前C1冻结值，并把八实施文件完整字节复制到 `R/original/` 后逐项校验SHA；包括两生产文件和Cases。
新增测试先在原生产SHA `51478f…` 上运行红阶段，之后只改RouteGesture；红／绿Cases SHA均为最终 `7e0b17…`。
各阶段八文件又单独按原字节保存于 `R/<stage>/sources/`，24份阶段归档全部与各自阶段冻结值一致。

| 阶段 | SDK／restore／build／run退出码 | 结果与UTC范围（2026-09-22） |
| --- | --- | --- |
| red | 0／0／0／2 | 编译0 warning／0 error；83例80过、G15C/D/E三项失败；07:06:50.7201530～07:06:55.4645936。 |
| green | 0／0／0／0 | 编译0 warning／0 error；83例全过；07:07:50.4229683～07:07:54.6158046。 |
| independent | 0／0／0／0 | 编译0 warning／0 error；原23探针全过；07:09:00.7091306～07:09:04.7287571。 |

固定绝对dotnet `C:/Program Files/dotnet/dotnet.exe`，SDK6.0.412，原Verify为cwd并读取原global.json。
每阶段CLI_HOME、NuGet包／缓存、obj／bin、TEMP／TMP均指向本阶段新目录；原NuGet.Config清空源，三个assets的sources／libraries均为空。
restore显式原csproj和原NuGet.Config；restore与build分别传同一新obj的BaseIntermediateOutputPath及MSBuildProjectExtensionsPath。
build使用--no-restore --configuration Release --output新bin -p:UseSharedCompilation=false；仅执行各阶段实际新DLL。
independent仅新增离线Repro.csproj，直接Link原复审的Repro.cs和当前两生产／FlowPos；没有改写探针源码或测试期望。
12个子命令均完成前后源码冻结；stdout／stderr和产物SHA复核一致。stdout原文保留SDK首次体验提示，没有用摘要覆盖原日志。

可直接核验：
- [红阶段原输出](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Verify/runs/953cfd631d8f43ac98bfd5f4e8e271a3/red/04_run.stdout.txt)、[绿阶段原输出](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Verify/runs/953cfd631d8f43ac98bfd5f4e8e271a3/green/04_run.stdout.txt)、[原23探针复验输出](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Verify/runs/953cfd631d8f43ac98bfd5f4e8e271a3/independent/04_run.stdout.txt)。
- 每步 `R/<stage>/<step>.result.json` 含完整实参、cwd、进程环境、起止UTC、实际ExitCode、stdout／stderr及产物SHA；对应before／after.freeze.json保存前后冻结。
- `R/case-id-audit.json`、`execution-audit.json`、`stage-archive-audit.json`及 `offline-assets-audit.json` 保存交叉校验。
- `R/RouteGesture.patch`、`RouteGestureCases.patch` 是与编辑前原字节比较的实际diff；原代码此前为未提交交付，不能用空Git工作区diff冒充本轮差异。

## SELF-CHECK／预算

只读Git命令及标准错误保存在 `R/git-readonly-checks.json`。no-index diff返回1表示两文件有差异；--check没有空白错误。常规diff --check exit0；已有LF/CRLF警告保留，没有改行尾。
本轮实质diff为新增64／删除19，共83行；Models不变。该可核预算基于本轮归档C1版本，不冒充缺失的原G1→C1累计diff。
按ReadAllLines：Models303＋Route395＝两生产698≤700；Cases1076，八实施文件总1887≤1900；本轮三源增删83≤420。
启动枚举既有两轮审查目录与G共401文件；结束只有两份允许源码改变，其余399文件保持原字节。原交付和两轮审查报告也分别冻结保全。
详见 `R/scope-budget-audit.json`、`preservation-audit.json`、`completed-checks.json`；完整新产物清单与SHA见 `R/artifact-hashes.json`。

## 历史证据更正与限制

- 原G1实际64个CaseId，C1为80，本轮为83；不沿用原“67个”说法。
- 旧G04G2“原点为尾格就不能恢复／必然测试错误／非生产bug”的无证定责在本交付中明确撤回，不作为接收依据。冻结契约要求回保留尾格恢复；当时完整源码缺失，不能重建失败根因。
- 旧G10B的“普通空格不捕获”符合契约，当前行为也通过；其旧源码字节缺失，旧失败原因仍NOT VERIFIED。
- C1实际有035cc6…红阶段69/80、2997de…中间阶段79/80、98e0c5…绿阶段80/80。中间阶段未在作者C1报告列出，本交付补正说明，原日志未改。
- C1两项测试错误G04H／G11C已由旧独立复审的匹配DLL/PDB和编译指令支持：合法相邻格被误当斜格、恰阈值Up被误断言为null。它们不计作有效生产红灯。
- C1缺少原三源码字节、完整命令／UTC／stderr和阶段前后冻结；原G1→C1新增＋删除≤420无法核实。旧标记操作顺序也不能补证。这些仍NOT VERIFIED，当前修复和新记录不改写过去。
- 本轮以用户直接修复指令和真实C1当前字节为新执行起点，完整保存此后证据；没有补造历史UTC、源码或作者声明。

## PATCH／BLOCKER

补丁已直接应用到上述两个文件；可用R/original的原字节与两份patch定位或回退本轮改动，本轮没有执行回退或Git写入。
代码修复无阻塞，注册回归和原独立探针均通过。旧证据不可恢复的限制按上节保留。
Unity导入／EditMode、真实鼠标触控、场景接线及026整包仍NOT RUN；本轮修复不包含019／026-G2接线。
