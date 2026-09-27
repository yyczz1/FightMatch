# FM-DEMO-026-G1-C1 独立复审

VERDICT: NEEDS_FIX

当前 80 个交付用例全部通过，原 R01～R15 探针全部转绿；F5 仍有两个可复现的数值缺陷。独立扩展探针 23 个中 19 PASS、4 FAIL，四个失败对应两处数值问题。C1 原源码归档及运行记录亦未满足 §170，不能用当前复验替代作者历史证明。

- 审查范围：仅 026-G1 的 C1 冻结交付，规范／规格两轴；没有修实现、测试或作者报告，没有审改 017。
- 原审查任务：`01a0c719-ddfe-7733-b3a6-ad81ede45275`；**本次原 R turn：`01a0c7d0-d6d9-7fb1-83b6-303ecf1c54af`**。
- 模型／环境：gpt-6-astra／max，原项目 local；2026-09-22。
- G = `D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1`。
- D = `D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17`。
- R = [本次独立证据目录](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo026G1Review/c56a5d4bff0340abb085439cd3e91c27)。
- 依据：AGENTS、.agent 相关规则、任务稿 §155～157／163～164／169～170／175～176、input-presentation r3、三只读输入、作者四报告、原独立报告及两轮原证据。全部八实施文件和当前用例已检查。

## 冻结与保全

启动核对 §175 的八文件、三输入、四作者报告及原 R 报告，共 16 项，逐项 SHA／字节吻合。
两棵完整原树含隐藏与 ignored 文件：当前 G 共 198 文件，原 R 根共 102 文件，共 300；本轮开始与结束逐项复核，无增删、哈希或长度变化，无 reparse 项。
每次独立 .NET 子命令另有前后冻结；证据见 `R/baseline.json`、各步 `*.freeze.json`、`R/final-preservation.json`。
§156～157 的 LF 规范化冻结 SHA 始终为 `542f52cf422574c9decc661ff44d45dc84a367d33d6487e8841f1d1951f98e18`。
派发整稿 SHA `c4e66aab5d62796cd75515cc9d2da219e151880292c9b06c244e63b4ab213807` 在启动吻合；后续状态登记不替代行为冻结。

| 本次三源 | SHA256 |
| --- | --- |
| Input/GestureModels.cs | c18257cdecb0f993b27130380b071e2f092ad5c230311e30bbd559c5218e9464 |
| Input/RouteGesture.cs | 51478f0439b92b2d00909e5d468505c69b39034ce2679459f28dcdd1f4d25da4 |
| Tests/RouteGestureCases.cs | 21a7fa709f7148126c538954d17a1b79ec0e22798bd36c9426b2f38a89b8cd67 |

C1 delivery SHA `1ae00f6def4efaad74ad567e5f640907019431e78cf28960b10e4e6b4b523953`；C1 scope SHA `c1edef9db522b877fb28468924e6bc6eb5a3999092de0295f4fa7b871f5a9131`。
其余五文件、三输入、原两交付及原 R 报告保持原冻结值，完整清单在 baseline。

## 规范轴

| 项目 | 独立结论 |
| --- | --- |
| 模块独立性 | PASS：普通同步实例；生产依赖为基础集合／BigInteger／FlowPos；无 Unity、QFramework、FightMatch.Core、Platform、时钟、随机、存档或业务裁定调用。 |
| 工程边界 | PASS：原 csproj 精确五个 Compile，C# 8／net6.0，无 PackageReference、越界 target／task；FlowPos 只读 Link。未改原工程或配置。 |
| 当前行数 | PASS：ReadAllLines 口径，Models 303、Route 392、Cases 1034；两生产 695≤700，三 C# 1729，八文件总计 1842≤1900。另五文件依次 16／60／24／7／6 行。 |
| 计数说明 | 作者 697／1732 按末尾换行多计，并仅列三源；不据此判功能失败。完整八文件预算已独立确认。 |
| 三源增删≤420 | NOT VERIFIED：没有原三源码完整字节。原八文件 1562 减未变五文件 113，得原三源 1449；净增 280 不能代替新增＋删除。 |
| 三源改动归属 | 当前可识别改动对应 F1～F5／N2；Read:204～211 与 BuildRouteIntent:380～389 去掉外层重复复制，由冻结构造统一复制，属于 F2。无法给出原字节级完整 diff 保证。 |
| 原 81 文件 | 原 R 基线对照仅三源改变、四标记原位删除，剩余 74 文件完全保持。旧四 runs 及原 bin／obj／.cli 等未被覆盖。 |
| 新作者文件 | G 内新增 121 文件均在三个新 runs 根：035cc6… 41、2997de… 38、98e0c5… 42；未发现新根外额外产物。原源码归档缺失另见 N3。 |
| 四标记 | 当前状态 PASS；归档四份各 32 字节且 SHA 与原基线一致，原位置均不存在。先校验再精确删除的历史顺序 NOT VERIFIED，不能从当前状态推定。 |
| C1 证据契约 | 不满足：必需原源码归档、完整命令／UTC／stderr／前后阶段冻结缺失，且旧根因声明未准确撤回；详见 N3。 |
| 自身造成的孤立代码 | 低优先级：Cases:133～138 的旧 AssertThrows 已被所有调用改用 AssertThrowsExact，现无调用；应仅删除这个由本次修改造成的孤立 helper。 |
| 本次写入／Git | 只新建本报告及 R。无 Unity、网络、依赖安装、Git 写入、分支、worktree、任务或子代理。只读 Git 四命令 exit 0；diff --check 无错误。 |

Git status 对其他历史深路径有 Filename too long 警告，diff 有 LF／CRLF 警告，原输出完整保留于 `R/git-readonly-checks.json`。
不据此宣称全仓干净或旧全仓零改动；G 与原 R 的完整保全以直接路径／SHA 枚举为准。共享工作区的 SD00／017 变化不归罪于 GLM。

## 规格轴：F1～F5／N1～N3

| 项目 | 独立结论与证据 |
| --- | --- |
| F1 | PASS（原末样本遗漏已修）：Route:146～174 在 Up 判断位移并处理末格；阈值6、原格 Up7 不 Tap、相邻终点只在 Up 出现可完整提交。G11A～E、R01／R02、C20／C22通过。下述 F5 同时影响 Move／Up 的极端数值判断。 |
| F2 | PASS：Intent 构造 internal（Models:280），View／Intent 在 Models:252～262／290～300 复制并只读包装列表；原列表变更及后续输入不改旧输出。G12A～C、R05～R07／R14通过；不倒推正常旧 Read 路径原来可变。 |
| F3 | PASS：Models:155～156 在 Pair 解引用前检查 null，抛 ArgumentException；G09O／R03通过。 |
| F4 | PASS：Models:147～148 明确只接受 Attack／FreeLink；非法 Mode99 被参数异常拒绝，合法两种模式仍正常。G09P／R04及 G07通过。 |
| F5 | FAIL：原 1e200 恰阈值、1e-200／2e-200 反例通过，但有限减法溢出和最小次正规数对角位移仍失败，见 F5a／F5b。 |
| N1 | 当前范围／标记字节 PASS；操作顺序 NOT VERIFIED。精确映射、四 SHA 在 `R/scope-budget-audit.json`，未删除或重做作者历史。 |
| N2 | PASS（当前必需见证）：G04G3 原点尾格、G04H 普通尾格、G04G2 退格恢复均保留；G04I 相邻自交不同时跨格，G04J 固化中段不同时命中端点；G01正反及G02逐格序列完整。G09 使用规定异常家族，不吞任意异常。 |
| N3 | FAIL（证据契约）：当前二进制可支持部分红绿事实，但缺必需原字节／运行记录，遗漏中间阶段，未明确撤回旧 G04G2／限定旧 G10B 根因；不能无条件宣称本次所有验收通过。 |

### F5a［P2］有限坐标相减溢出后，NaN 使阈值判断为 false

位置：[RouteGesture.cs:249](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Input/RouteGesture.cs:249)，同时涉及 250～259、Move:102 和 Up:146。
最小条件：合法 Attack 绑定、可画端点、已选角色、allowHistoryTap=true、阈值6；Down 局部(-1e308,0)，同一指针最后样本为(1e308,0)，两个坐标均有限。

| 独立探针 | 预期 | 实际 |
| --- | --- | --- |
| C16：Move 仍映射原格 | 超阈值，Dragging | Pressed |
| C17：无 Move，Up 仍映射原格 | 不得 Tap | TapLocator |
| C18：两端相邻，另一端只在 Up 出现 | 一次两格 AttackRoute | null |

原因：dx 相减得到 +Infinity；maxDim=Infinity，rx=Infinity/Infinity=NaN，scaledDist=NaN，比较 `NaN > threshold` 为 false。
影响是有效输入被误认为未移动，导致错误历史查询意图或丢失路线。§170 明确要求的有限减法溢出场景尚未修复，现有 G15A/B 没有覆盖。
最小修正范围：ExceedsThreshold 的比较及对应回归，须在缩放前正确处理差值溢出，保持原有限 double 输入域、严格大于和 Move／Up 一致规则。

### F5b［P2］缩放后乘回极小数，舍入抹掉严格大于关系

位置：[RouteGesture.cs:258](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Input/RouteGesture.cs:258)。
最小条件：阈值 `double.Epsilon`（最小正次正规 double，此处是合法测试值，不是新增容差），Down(0,0)，Move(Epsilon,Epsilon) 仍在原格。
预期距离 sqrt(2)×Epsilon 严格超过 Epsilon，应 Dragging；C19 实际 Pressed。
dx／dy 和归一化均非零，但最后乘回 maxDim 时，距离舍入成 Epsilon，与阈值相等，返回 false。
必须比较而不丢掉这类大小关系；不能增加 epsilon、改 float 或拒收这些有限值。与 F5a 共用同一小范围修正。
对照 C20：阈值6／1e200／1e-200，恰阈值保持 Pressed／允许同格 Tap，下一可表示 double 进入 Dragging；C21：MaxValue 对角超过有限阈值、3-4-5 恰阈值均通过。

## N3：作者红绿与历史证据分层

| C1 原 run GUID | 原四步退出码／实跑 | 编译源指纹与限制 |
| --- | --- | --- |
| 035cc6dcdc2e476683141d048e9850dd | 0／0／0／2；80=69 PASS+11 FAIL | PDB 为原两生产 SHA，Cases 为 076ba604…；有效红阶段含9个 F1～F5 失败，另2个是测试错误。 |
| 2997de21abe748a4a5b4ad3cf4251308 | 0／0／0／2；80=79 PASS+1 FAIL（G11C） | 原报告未列；PDB 为当前两生产 SHA，Cases 为 42bd9c93…；G04H 已更正、G11C 尚未更正。 |
| 98e0c5e66d2548afa748042fd6e72982 | 0／0／0／0；80全部 PASS | PDB 三源 SHA 与当前冻结三源一致，支持当前编译源身份。 |

对三份作者 DLL／PDB 只做 PE／元数据读取，没有执行作者 DLL；CodeView GUID／stamp 与对应 PDB 身份均匹配。
`R/author-pdb-document-hashes.json` 保存源码文档 SHA；`R/author-compiled-witness-audit.json` 保存二进制 SHA、匹配结果及 G04H／G11C 的指令。
红生产 SHA 分别为 `0c021c4726df0708b637de1f8c984fa059ec9f5a868068ae533580e5a150d702`／`8925591bfdf363f03493fc37b94868e066a1772e3f50be2d8d2867aed84c4391`。
红 Cases SHA 为 `076ba6048df52b4bca8c536b3b125abd6388b570d96ec3e6448607276bf97b5a`；中间为 `42bd9c932edc42c6de74b3c02966d2658a310ef0b5ab63f55ebf397f09f31ae9`。

- 红阶段9个有效失败：G09O／G09P、G11A／G11B、G12A／G12B／G12C、G15A／G15B，与原 F1～F5 对应。
- G04H 红编译指令确实从尾格(1,0)采样(1,1)，这是相邻有效新格；后来改为(0,1)，才是非法斜格。C23亦独立确认原样本应有效，不能计作生产缺陷。
- G11C 红／中间指令都对 Up 返回值作 null 断言；阈值6、距离恰6、原格且允许历史 Tap 应返回 Tap。绿改为 TapLocator 断言正确，C20独立通过。
- 因而两次测试更正有可核支持；这不能证明每次编辑的完整源码或执行时刻，也不能把11个失败全部算作有效生产红灯。

以下是本次 C1 自身缺口，位置为 C1 scope:70～77／95～110／130～132、delivery:27／红绿说明及三个 runs 的 result 文件：
1. §170 要求修改前保存原三源码完整字节；G及原R完整300文件中，无任一原三源、红 Cases 或中间 Cases SHA 对应的完整文件；三个 PDB 无嵌入源码。PDB 校验和不能还原原字节，也不能证明原64断言的逐字差异或新增＋删除≤420。
2. 三个 run 的 result 仅 CMD 短标签／EXIT，无 cwd／开始结束UTC；没有 stderr 文件或等价原样内嵌。scope 的省略号／相对简写不能还原精确命令、输出路径参数及阶段环境。
3. 没有红绿各阶段全套源码／配置／FlowPos 运行前后 SHA 与产物绑定记录；当前源 SHA、PDB 和现在计算的产物 SHA只证明当前证据，不补齐历史前后冻结。
4. scope 未列 2997de… 中间失败阶段；`deliveredAtUtc=2026-09-22T04:17:45Z` 是声明，缺阶段时刻不能验证先后。文件时间未被用作执行 UTC。
5. 新报告纠正67为64，但仅泛称旧历史 NOT VERIFIED，未按 §170 明确撤回“旧G04G2必然测试错误／非生产bug”，亦未说明旧G10B源码无法还原。原结论限制仍按原 R 报告保持。

实际 restore assets 的输出与 packages 指向各自新 run，build stdout 指向对应新 bin；旧81文件对照也支持没有覆盖旧输出。不能仅凭 scope 命令写法就指称发生了越界覆盖。
证据缺失不等于已证明代码错误；其独立接收影响是 C1 必需证据／diff预算无法核准。旧 G1 的 UTC／stderr／阶段SHA缺口继续 NOT VERIFIED，不倒填、不用旧豁免覆盖 C1 新义务。

## G01～G10 逐项结论

| 组 | 结论 | 依据与限制 |
| --- | --- | --- |
| G01 | FAIL（完整合法输入域） | 规定正反完整序列和恰6／越6见证均过；C18仍可丢失合法相邻终点 Up 路线，原因 F5a。 |
| G02 | PASS | G02完整六格重走序列无已退格；超阈值后回原点不能退成 Tap。 |
| G03 | FAIL | 普通端点／固化中段 Tap、返回原点／不同格／禁Tap对照均过；C17对已超过阈值的合法序列误发 Tap。 |
| G04 | PASS | 无效格保留前缀、不插点，封闭终点、尾格及倒数第二格恢复；新增 G04G3/H/I/J 和 R10／R11通过。 |
| G05 | PASS | 离板／越界／Cancel匹配与全清／Bind清除／未完成Up；R12／R13含非法Bind与相同Bind对照通过。 |
| G06 | PASS | 重复Down／Up、第二指针和完成后新Down；G11D/E、R12所有权对照通过。 |
| G07 | PASS | Attack缺角色／不可画／disabled不产路线，FreeLink正确且CharacterId=null；C22不可拖起点Up越阈值不Tap。 |
| G08 | PASS | 版本／身份保留、重绑取消、输入与全部可获得输出冻结；G08、G12和R05～R07／R13／R14通过。 |
| G09 | PASS | 规定坏输入准确参数异常、非法Bind不改状态、null Pair／未定义Mode已拒绝；G09及R03／R04／R13通过。 |
| G10 | FAIL（证据契约） | 当前纯模块／只读Link／同版编译／80逐例零跳过通过；原字节、C1完整执行历史和增删预算缺证，不能无条件通过。 |

`R/case-id-audit.json`：BuildIds、switch、scope、三次作者日志及本次实跑的 **Ordinal多重集合** 均相同，80个唯一ID、各一次，无跳过，旧64个全部保留。
新增16为 G04G3/H/I/J、G09O/P、G11A/B/C/D/E、G12A/B/C、G15A/B；与声明一致。旧64当前行为已逐项按契约检查，原完整源 diff 限制仍保留。
未知ID返回false且Program将未执行／失败作为非零；不靠名称推定通过。

## 本次独立验证

所有 .NET 步骤 cwd 均为原 `G/Verify`，固定 `C:/Program Files/dotnet/dotnet.exe`、SDK6.0.412。
先查原工程无自定义越界动作／PackageReference／祖先 Directory.Build 注入，再使用清空源的原 NuGet.Config；新两个 assets 的 libraries／sources均为空。
子进程 CLI_HOME、NuGet packages／缓存、TEMP／TMP 均限 R；禁遥测、首次体验、workload提示、共享编译与节点复用，未安装依赖。
restore／build 各自传两个独立 MSBuild 属性，指向同一新 obj；build 使用 --no-restore --configuration Release --output新bin。
Repro只读Link原两生产和FlowPos；产物在独立repro-obj／repro-bin，不复制或修改生产源码；只执行本次新DLL。

| 步骤 | 实际结果／2026-09-22 UTC |
| --- | --- |
| 01_version | exit0，6.0.412；06:37:36.9961973～06:37:37.1834289。 |
| 02_restore | exit0；06:37:37.4160533～06:37:38.5252143。 |
| 03_build | exit0，0 warnings／0 errors；06:37:38.7594846～06:37:41.1355066。 |
| 04_run | exit0，80 PASS／0 FAIL；06:37:41.5085538～06:37:41.6280970。 |
| 05_repro_restore | exit0；06:39:20.8444856～06:39:21.7463948。 |
| 06_repro_build | exit0，0 warnings／0 errors；06:39:22.0596861～06:39:24.1501231。 |
| 07_repro_run | exit2，23例19 PASS／4 FAIL；06:39:24.4862225～06:39:24.6030200。 |

每步完整参数／绝对命令、cwd、子进程环境、UTC、实际退出码、stdout／stderr及SHA、源码前后冻结与产物SHA，见 `R/<步骤>.result.json` 及同名前后freeze和原日志。
R01～R15全过；C16～C19失败；C20～C23通过，详见 [独立探针原输出](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo026G1Review/c56a5d4bff0340abb085439cd3e91c27/07_repro_run.stdout.txt)。
辅助只读元数据查询曾因审查命令的扩展方法调用／操作码解码问题退出1，修正后完成上述读取；未执行作者程序集，未更改任何测试期望或用作生产红灯。

- Repro.cs 181行≤220，SHA `34c20ff41fb4bf22d8d145853e423134faf3e17c0daac647bb911e3d7dd17351`；Repro.csproj 17行≤60。
- 新 RouteGesture.Verify.dll SHA `a6ecb74c65032ec9e55d5bdc8c709df94637d977093195682a0202ad8e46dbe7`。
- 新 Repro.dll SHA `5a8d01f5e4c45c6a53c078529cf120cb419948b10dd63f9946e014088a308438`。
- 完整本次产物清单／SHA见 `R/artifact-hashes.json`；最终交回核对见 `R/completed-review-checks.json`。

## 最小修正包建议（待SD00正式签发，未执行）

TASK_PACKET：FM-DEMO-026-G1-C2；目标仅关闭 F5a/F5b 与本次证据交付问题，不重开架构或扩展G2。
建议允许：`G/Input/RouteGesture.cs` 的阈值比较、`G/Tests/RouteGestureCases.cs` 的对应独立回归及移除本次孤立AssertThrows。
建议新交付：`D/demo-026-glm-g1-c2-delivery.md`、`D/demo-026-glm-g1-c2-scope.json` 和 `G/Verify/runs/<全新GuidN>/`；这只是范围建议，须由SD00授权。
Models、其余五文件、所有原报告／scope／runs／R证据、三输入、Assets／meta／Packages／ProjectSettings／Git／017／协调稿保持只读。

验收与验证：
1. 先归档当前完整三源字节及全部既有文件清单；先增加C16～C19等效长期回归，在本次冻结生产上实跑红灯，核失败确实来自F5。
2. 修正全有限double域的严格阈值比较；Move、Up同样正确。保留原80个ID及断言，极大／极小、减法溢出、最小次正规对角、恰阈值／下一可表示数和普通3-4-5对照全过。
3. 沿已验证离线协议取得同版全绿；每一步保留完整命令、cwd、环境、起止UTC、stdout、stderr、实际退出码、阶段前后源码／配置／FlowPos及产物SHA；所有失败保留原件。
4. 新交付准确列出三个C1阶段和二进制可支持的测试更正，撤回旧G04G2必然根因、明确旧G10B不可还原。可找到的真实历史原件原样归档；不存在的继续列明，禁止重跑冒充历史。
5. 当前八文件≤1900／两生产≤700仍是约束。C1原三源新增＋删除≤420不能由本次净增或反编译证明；由SD00明确未恢复历史的接收处置与C2基线／增删预算，worker不得自行豁免或扩额。

本次不接收G1，不放行G2。所需动作是SD00签发上述窄修包并明确历史缺证处置，再交独立复审。
Unity导入／EditMode／真实鼠标触控／正式场景／026整包均NOT RUN；G2仍需019独立接收和SD00精确接线包。
本报告交回后本审查停止修改；不向GPT C／主线R派发任务。
