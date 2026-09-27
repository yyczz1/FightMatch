# FM-DEMO-026-G1 独立代码审查

VERDICT: NEEDS_FIX

仅针对 G1。冻结版可独立编译、64 个注册用例全过，但存在 5 项规格问题，以及测试覆盖、历史证据和范围清单问题；不能据绿色汇总接收。

- 审查任务：R-G1／01a0c719-ddfe-7733-b3a6-ad81ede45275，local，GPT-6 Astra／max。
- 原审查 turn：01a0c719-e130-73a2-b15e-3b9fd88849d6（§164 登记）。
- 依据：AGENTS.md、.agent/PROJECT_CONTEXT.md、CODING_RULES.md、VALIDATION.md、REVIEW_CHECKLIST.md、PLANS.md；任务稿 §155～157、163～164；input-presentation.md r3。
- 已逐行检查全部八文件、FlowPos／Core asmdef、作者两报告及四个原 runs 的全部 40 文件。未审查或修改 017。
- 下文 `G/` = `ExternalWork/FM-DEMO-026-G1/`；`D/` = `docs/system-design/2026-09-17/`。
- 本次证据根 `R/` = `D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo026G1Review/1e19149fdc7f4ae581489fdd1c8b882a/`。

## 冻结与范围

启动时八源码／配置、三只读输入和作者两报告的 SHA256 全部符合 §163。§156 起至 §158 前按 LF 归一化的 SHA256 为 `542f52cf422574c9decc661ff44d45dc84a367d33d6487e8841f1d1951f98e18`，吻合冻结值。
读取时整稿 SHA 为 `42715925aae955f1fdbe3c347f6020a7fd560625160ce6e4725faa3ea8afd3fb`；派发 SHA 为 `7113b52c1b0d708ee6270c10b9601a25ffdf3ee4c1066493fc78445d9160293c`。整稿状态登记变化不替代冻结行为校验。

`R/baseline.json` 保存初始清单；每次 .NET 子命令前后另存 freeze。最终 `R/final-preservation.json` 确认：
13 个指定文件及 G/ 原有全部 81 文件字节／SHA 不变，包含原 runs 的 40 文件；G/ 无新增文件。原 bin／obj／.cli 也在这 81 文件中。
这是本次审查期间的保持证据；没有作者开工前全工程基线，不能倒推作者此前从未改动旧工程或 meta。

## 规范轴

| 检查项 | 结论与证据 |
| --- | --- |
| 手写预算 | PASS：八文件共 1562 行；两生产 C# 共 643 行，分别低于 1600／700；作者报告 74 行，scope 10222 字节。 |
| 生产／测试边界 | PASS：同步实例，无 Unity／QFramework／FightMatch.Core／Platform、业务状态写入、网络、文件、时钟、随机或后台线程调用；无额外策略／服务层。 |
| 验证工程 | PASS：csproj:4～9 设置符合契约；17～21 精确链接五个编译输入；无 PackageReference、自定义 Task／Target／Import 或祖先构建覆盖；NuGet.Config 清空源；global.json 锁定 6.0.412。 |
| asmdef | PASS：名称与命名空间 FightMatch.Input；只引用 FlowPuzzle.Core，noEngineReferences／autoReferenced=true；其余开关符合契约。Unity 导入另列 NOT RUN。 |
| 范围清单 | FAIL，N1：`G/Verify/.runguid`、`.runguid2`、`.runguid3`、`.runguid4` 各第 1 行为四次运行 GUID。它们是位于允许自然产物目录之外、未在作者变更清单列明的额外文件；不是 SDK 产物。当前审查保留，不擅自删除。 |
| 冻结 API／验证行为 | FAIL：规格轴 F2～F4；不以能够编译代替构造约束。 |
| 测试与历史证据 | FAIL：N2／N3，见以下两节。 |
| Git／受保护范围 | 本次无 Git 写入、无 Assets／配置修改。四项只读 Git 检查 exit 0，diff --check 无错误；共享工作区存在既有和并行改动，未归罪于 G1。详见 `R/git-readonly-checks.json`。 |

小项：`G/Input/GestureModels.cs:3` 的 `using System.Collections.ObjectModel;` 未使用且超出列明命名空间；可在获准的该文件窄修时删除。它没有引入外部包。

## 规格轴：可复现问题

### F1 [P1] Up 没有处理松手样本的阈值

位置：`G/Input/RouteGesture.cs:149～166`。§156 要求 Up 先按与 Move 相同的位移／格规则处理末样本。
当前 Up 只查看之前的 `_thresholdExceeded`，此前未发生越阈值 Move 就直接走 Tap 分支。
最小输入：阈值 6，Down 局部 (0,0)、cell=(0,0)，不发 Move，Up 局部 (7,0)、cell 仍 (0,0)。
预期无 Tap；实际返回 TapLocator（R01）。若 Pair 两端相邻 (0,0)/(1,0)，Up 的 cell=(1,0)，预期两格 AttackRoute，实际 null（R02）。
影响：松手首次越阈值时误发历史查询，或丢掉合法快速画线草案。最小修改只需统一 Move／Up 的末样本处理，并保留 Up 的无条件结束捕获。

### F2 [P2] 输出构造入口违背“内部创建／不可变”约束

位置：`G/Input/GestureModels.cs:240～248、265～275`。
§156 明确 GestureIntent 只由模块内部构造；当前构造器 public（R05），且 Intent.Cells／View.DraftCells 直接保存传入集合。
最小输入：用两格 List 构造 Intent 后 Clear 原 List，Cells 从 2 变 0（R06）；用一格 List 构造 View 后 Clear，DraftCells 从 1 变 0（R07）。
影响：公开 API 可制造并随后改变声称只读的草案／观察；接入后无法依靠该类型表达冻结输出。
限定：RouteGesture 正常 Read／BuildRouteIntent 已复制并只读包装，正常模块输出在 R14 中保持不变；没有据此声称这些正常输出会变。
最小修改：收窄 Intent 构造可见性；View 的公开构造也必须满足冻结约束或收窄。若仍接受外部集合，复制并只读包装；无须对已封闭的内部安全路径强加重复复制。

### F3 [P2] pairs 含 null 元素时泄漏 NullReferenceException

位置：`G/Input/GestureModels.cs:151～165`。
§156 禁止 null Pair，并限定无效构造抛 ArgumentNullException／ArgumentException／ArgumentOutOfRangeException。
最小输入：其他参数合法、pairs=new GesturePair[] { null }。预期参数异常；实际 NullReferenceException（R03，单元素在 164 行解引用；多元素可能更早在 158 行发生）。
最小修改：在任何 Pair 属性解引用前逐元素检查 null；测试必须校验异常类型，不能捕获任意异常就算通过。

### F4 [P2] 未定义 GestureMode 被默认为 FreeLink

位置：`G/Input/GestureModels.cs:130～149、219`；`G/Input/RouteGesture.cs:239～241、355～360`。
§156 只允许 Attack／FreeLink。最小输入：构造 mode=(GestureMode)99、角色 null、可画相邻端点，完成拖线。
预期构造拒绝；实际 Context 接受该值并产出 FreeLinkRoute（R04）。
影响：错误模式静默进入另一业务意图分支。最小修改：构造时显式拒绝两值之外的枚举，不新增默认模式。

### F5 [P2] 有限 double 的平方导致阈值比较上溢／下溢

位置：`G/Input/RouteGesture.cs:102～105`。
§156 接受正有限阈值及有限局部坐标，未限定数值量级。当前直接计算 sqrt(dx*dx+dy*dy)。
最小输入 A：Down 局部 0、阈值 1e200，Move 局部 1e200 且同一格；预期恰等阈值仍 Pressed，实际 Dragging（R08）。
最小输入 B：阈值 1e-200，Move 局部 2e-200；预期 Dragging，实际 Pressed（R09）。
普通阈值 6 的用例通过；本问题限极端但合法的有限输入。最小修改使用避免平方上溢／下溢的距离比较，并保持严格大于及 Unity 2022.3 可用 API。

## N2：必需见证缺口

- `G/Tests/RouteGestureCases.cs:379～392`：G04G2 改为退格后，整个注册集合没有“无效后回保留尾格恢复”的见证。R10／R11 已证明当前生产代码分别支持原点尾格／非原点尾格恢复，但这不能代替交付用例集的长期回归覆盖。
- 同文件 324～331 的 G04C 从 (2,0) 返回 (0,0)，同时是跨格；347～356 的 G04E 采样 (0,1)，同时是他对端点。二者没有独立隔离“相邻自交”和“固化中段占格”规则。R11 补验固化中段通过；相邻自交由生产代码静态检查支持。
- 同文件 113～117 的 AssertThrows 捕获任意 Exception；坏输入组没有 null Pair／非法 Mode，无法发现 F3／F4，也不能证明契约要求的异常家族。
- G01A／G01B／G02 仅断言长度和首尾，缺完整序列逐格断言（159～161、177～179、212～214）。原实现静态路径符合手写序列，但现有断言不能完整守住该要求。
- 最小补测：F1～F5 对应边界；尾格与退格分别恢复；不重叠其他非法条件的固化中段／相邻自交；完整序列与准确异常家族。不要仅改期望使当前实现转绿。

## N3：四阶段原证据与两次测试修正

以下原文件均已读取并保留字节。四列 exit 依次为 version／restore／build／run。

| 原 runs GUID | 实物结果 | 可验证范围 |
| --- | --- | --- |
| 76586f585ee94b4f81fda4b762d918c3 | 0／0／0／2；62 PASS、2 FAIL（G04G2、G10B） | result 有命令、cwd、UTC；2026-09-22 02:42:23.896829～02:42:28.934375 UTC。 |
| 6f83b2bd41c14443933e38e2f1ed49eb | 0／0／0／2；63 PASS、1 FAIL（G10B） | result 有命令、cwd、UTC；02:43:51.659900～02:43:56.153718 UTC。 |
| 1bf9ed24309b459298224599d618f854 | 四步均 -2147450751；均为 dotnet Usage，零 CaseId | result 的 CMD 为空；缺 cwd／UTC。不能算 restore／build／测试已执行。 |
| 4c26c6bab7a1494781db79cefd625de2 | 四步均 0；64 PASS、0 FAIL | result 只有 --version／restore／build／run 短标签，缺 cwd／UTC；完整命令由 scope 声明，不能独立还原执行时序。 |

`D/demo-026-glm-g1-delivery.md:41` 写“67项”，但展开其同一清单只有 64 项：3+1+7+8+8+4+6+11+14+2。
源码 BuildIds、switch、scope.caseIds、scope 内嵌运行输出、两次测试失败日志、最终原日志、本次独立输出，按 **Ordinal 多重集合及顺序** 均为同一 64 个不同 ID、各出现一次。
数字 67 是报告笔误，没有证据表明另丢了 3 个注册用例；实际缺测按 N2 独立判定。`R/case-id-audit.json` 保存完整比较；Usage 阶段单独记录为 0 个 ID。

G04G2：旧日志实际断言失败信息是“recovered by returning to tail”。scope:141～143 称尾格为原点就不能恢复，与冻结契约及当前 RouteGesture:257～260 相反。
R10 对“草稿仅原点→斜格无效→回原点”实跑通过，R11 对普通尾格恢复也通过。改成退格场景本身符合契约，但删掉回尾见证不符合覆盖要求。
旧测试字节未保存，不能确认旧样本、旧失败根因或断言究竟如何改动；不得判定旧失败一定是生产 bug，也不得接收作者“非生产 bug”的定论。

G10B：scope:149～151 所述“普通空格 (1,2) 不应捕获”符合契约；当前 795～802 使用有效端点并检查回读也合理。
R15 独立确认普通空格不捕获、有效固化端点优先归 Endpoint。然而旧 G10B 源码字节缺失，只能确认报告描述与当前行为，不能还原旧失败根因。

历史冻结 **NOT VERIFIED**：
- scope:126～132 只有最终八文件 SHA 和三个 Cases 阶段 SHA 的声明，没有每阶段两生产文件、Cases、Program、配置、FlowPos 的运行前／后清单，也未绑定到对应 runs GUID。
- “pre-fix-attempt2”被写成 63／1，第一份实际测试日志是 62／2；缺少旧字节／阶段对应关系，不能给那几个历史 SHA 擅配阶段。
- scope:5 仅引用 §155～157，未存这三节的冻结内容或哈希。SD00 本次冻结可验证当前契约，不能补造作者当时的契约记录。
- delivery:5／scope:4 的交付 UTC 01:33 早于有 UTC 的两次失败运行 02:42／02:43，历史时间描述自相矛盾。
- 1bf9ed…／4c26c6… 两目录均无独立 stderr 文件，scope 仅为最终阶段声明 stderr 为空；缺失独立原 stderr、最终 UTC 和完整 result 命令，不能升级为完整原始历史。
以上是证据不足／矛盾，不据此指控伪造。本次重跑只证明当前冻结源码，不追认旧冻结或旧工程零修改。

## G01～G10 逐项结论

“作者实跑”均指本次独立重跑注册用例 64／64；组结论同时考虑冻结规格和交付见证。

| 组 | 结论 | 输入 → 预期 → 实际证据 |
| --- | --- | --- |
| G01 | FAIL | 5×5／阈值6／正反路线作者用例通过；Up 首次越阈值应处理末样本而实际遗漏（R01／R02）；极端有限阈值误判（R08／R09）。 |
| G02 | PASS | S→1→2→1→S 后改走 (0,1)→(1,1)→(2,1)→(3,1)→目标，返回6格 FreeLink、无 Tap；G02＋ProcessCell 静态检查支持；全格断言需按 N2 加强。 |
| G03 | FAIL | 通常端点／固化中段 Tap、越阈值缩回、异格 Up、tap=false 均通过；仅 Up 越阈值时误出 TapLocator（R01）。 |
| G04 | FAIL（交付见证） | 现有非法格／退格用例通过；回尾恢复被删且部分规则测试互相遮蔽（N2）。当前原点／普通尾格恢复、固化中段及无效期间不续画由 R10／R11 实证通过。 |
| G05 | PASS | 离板、显式越界、匹配／全清 Cancel、Bind、未完成 Up 应清捕获且不复活；G05A～H、R12／R13 通过。 |
| G06 | PASS | 重复 Down／Up 和第二指针不改主手势，新 Down 可开始；G06A～D 及 R12 的负数所有者 ID／异指针越界通过。 |
| G07 | PASS（合法 Mode） | Attack 缺角色／不可画／disabled 无路线；FreeLink 无角色完成，带角色输入仍输出 null CharacterId；G07A～F＋R14 通过。非法枚举另属 G09。 |
| G08 | FAIL（公开模型） | 正常 Context、Read、Intent 的防御复制和两种大修订保持通过（G08＋R14）；相同 Bind 取消通过 R13。但公开输出构造可共享可变 List（F2）。 |
| G09 | FAIL | 原坏输入测试全过，非法 Bind 保持草稿由 R13 补证；null Pair 抛错异常、非法 Mode 被接受（R03／R04）。 |
| G10 | FAIL | 当前独立性、编译、全部注册 ID 实跑通过；范围多出4文件，历史冻结／时间／旧工程零修改缺证（N1／N3）；不能无条件 PASS。 |

## 本次独立执行

全部 cwd 均为原 `G/Verify`，绝对 dotnet 路径 `C:/Program Files/dotnet/dotnet.exe`；SDK --version=6.0.412。
仅子进程环境设置 CLI_HOME、packages、NuGet 缓存、TEMP／TMP 到 R；关闭遥测、首次体验和 workload 更新提示、共享编译及节点复用。
原 NuGet.Config 清空源；两个新 project.assets.json 的 libraries／依赖均为空、packagesPath／outputPath 均在 R。没有依赖安装或网络工具调用。

| 本次记录 | 实际结果 |
| --- | --- |
| 01_version | exit 0，6.0.412；03:19:06.8771198～03:19:07.0442788 UTC。 |
| 02_restore | exit 1，审查命令把两个 MSBuild 属性误拼成单个路径；失败完整保留。未执行其后的 build／run。 |
| 02b_restore／03_build／04_run | 各 exit 0；新目录恢复／编译，0 warnings、0 errors；total=64 pass=64 fail=0。03:19:30.4938051～03:19:33.9971036 UTC。 |
| 05_repro_restore／06_repro_build | 各 exit 0；原生产源码只读 Link，0 warnings、0 errors。03:23:08.0719495～03:23:11.0398811 UTC。 |
| 07_repro_run | exit 2；15 个契约探针，6 PASS、9 FAIL，分别映射 F1～F5。03:23:11.1164365～03:23:11.2903875 UTC。 |

每步完整绝对命令、参数数组、子进程环境、cwd、UTC、退出码、原 stdout／stderr 及其 SHA 在对应 `R/<步骤>.result.json`／stdout.txt／stderr.txt；源码前后 SHA 在同名前后 freeze.json。
restore／build 均传相同 BaseIntermediateOutputPath 与 MSBuildProjectExtensionsPath 到 R/obj；build --no-restore --configuration Release --output R/bin -p:UseSharedCompilation=false。
Repro 使用 R/repro-obj 与 R/repro-bin，其余协议相同。只运行新生成 DLL，未运行作者旧 DLL。
另一次审查 PowerShell 驱动因缺右括号在 .NET 启动前失败，shell exit 1、无输出；原命令和诚实缺失的 UTC 记在 `R/reviewer-driver-failure.json`。修正只涉及审查命令，未改测试期望或生产文件。

- `R/Repro.cs` 133行，SHA256 `d00c80efb3da87a372267f13191e8746c76c0f8cd5c44597d9cbb79ebf8bde07`；Repro.csproj 17行。
- 新 RouteGesture.Verify.dll SHA256 `fbc189ea30f090aaa1a057767e6e490cf2f7a83fc1d5386caca77cc65baca2d4`。
- 新 Repro.dll SHA256 `c8839b3b45dcf3e039207504eded3d9febfcd4a89c1bb51b00185b387222acaa`。
- 更多产物 SHA 在 `R/artifact-hashes.json`；9个失败是探针数，对应5项规格问题，不是9个独立生产缺陷。

## 最小返修包建议（未派发、未执行）

TASK_PACKET：FM-DEMO-026-G1-C1；目标仅恢复本次冻结手势／值模型契约及其证据，不重开 G1 全面实施。
允许修改建议：`G/Input/RouteGesture.cs`、`G/Input/GestureModels.cs`、`G/Tests/RouteGestureCases.cs`，仅 F1～F5／N2 对应位置。
建议新增返修交付：`D/demo-026-glm-g1-c1-delivery.md`、`D/demo-026-glm-g1-c1-scope.json`；需 SD00 正式包明确授权，当前未创建。
八文件中的其余五文件、原两报告、原 runs／bin／obj、只读输入、Assets、Packages、ProjectSettings、meta、Git、017及协调稿均不得修改。
原四个 .runguid 先由 SD00 明确隔离／删除的处理授权；本审查及建议包不授权删原证据。

验收：
1. Up 首次越阈值不 Tap，支持合法相邻端点只在 Up 到达；两类有限数值反例正确，普通阈值6和一次Up仍正确。
2. null Pair／非法 Mode 按参数异常拒绝；Intent无公开构造，所有对外输出无法被原集合或后续输入改变。
3. 注册行为用例恢复并分别隔离 N2 缺失见证；按手写完整序列与准确异常类型断言，零跳过。
4. 保留旧失败原字节；新交付改正64／67和错误根因声明，旧历史继续标 NOT VERIFIED，不能回填缺失时间／阶段SHA。
5. 沿原预算（总≤1600行／生产≤700行）；若无法容纳必要回归，由 SD00 先缩包或显式调整，worker 不自行扩大。

验证：正式返修包沿 §164 隔离输出协议，先固定 SDK --version，再分别 restore／build --no-restore／执行新 DLL；全部注册用例逐例运行。
每阶段开始与结束保存源码、配置、FlowPos SHA；记录完整命令、cwd、UTC、stdout、stderr、退出码。反例按以上契约验收；输出构造收窄后通过公开 API 证明冻结，无须照抄同程序集的 R06／R07 构造方式。
返修完成后重新独立审查。此处只给拟议窄包，不修改作者实现、不调用外部 worker。

## 交回边界

用户动作：将本报告交 SD00，据此签发上述窄修包并明确四个范围外标记文件和历史缺证的处理；没有原证据时应保留 NOT VERIFIED，不能要求作者补造。
G1 当前不接收，G2 不放行；即使后续 G1 通过，G2 仍需 019 独立通过及 SD00 精确接线包。
Unity 导入／Unity 测试、真实鼠标触控、正式场景、026 整包：全部 NOT RUN。
