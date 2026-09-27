# FM-DEMO-026-G1-C2 独立验收

VERDICT: ACCEPT

仅接收 026-G1 的 Codex C2 修正版。规范轴、规格轴均通过，未发现需返修的问题；旧 G1／C1 指定历史按 §184 继续 NOT VERIFIED，不追认两次旧审查。
Scope: PASS。Acceptance criteria: PASS（§184 明确限定的历史边界下）。Verification: 独立复编 83/83、原23/23、补充数值42/42；全部零失败。

- 审查任务：R-G1-C2，01a0c80b-0d55-7220-8115-8306770b0571；GPT-6 Astra／max，原项目 local。
- **准确本次原审查 turn：01a0c80b-10a2-7ed1-8586-2257200410bb。**
- C2 作者任务：01a0c719-ddfe-7733-b3a6-ad81ede45275；准确实施 turn：01a0c7e9-9612-7263-8d08-aacd5c4024b3。
- 已核该实施 turn completed、error=null、用户直接修复授权及正式 final；没有拿后续“通知”turn 代替完成门。
- 依据：AGENTS.md、.agent/PROJECT_CONTEXT／CODING_RULES／PLANS／VALIDATION／REVIEW_CHECKLIST／DEEPSEEK_WORKER_PROMPT；任务稿 §§155–157、163–164、169–170、175–176、183–185；input-presentation r3、application-flow §§2–5。
- G = D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1。
- A = G/Verify/runs/953cfd631d8f43ac98bfd5f4e8e271a3（作者证据，只读）。
- R = [本次独立证据根](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo026G1Review/2dbb4a8ec0f4438fb6faaee95f09e9bd)。
- 全部八实施文件、原 FlowPos／asmdef／meta、六份作者报告／scope、两旧审查报告及相关原证据已检查；未审改主线018A。

## 冻结、范围与规范轴

| 检查 | 独立结果 |
| --- | --- |
| 发包门 | 20项 SHA 均匹配：八文件、三直接输入、六作者交付、两旧R报告及旧23探针；见 R/frozen-input-gate.json。 |
| 契约 | §§156–157 按LF／UTF-8无BOM的SHA为542f52cf422574c9decc661ff44d45dc84a367d33d6487e8841f1d1951f98e18。发包r92全文SHA保留于R/contracts-frozen.json；后续r93身份／状态登记不作为业务变更。 |
| C2实际改动 | 仅 RouteGesture.cs 和 RouteGestureCases.cs。真实C1归档与当前字节的fresh no-index diff：Route +15/-12，Cases +49/-7，合计 +64/-19=83≤420。 |
| 生产改动 | 仅新增System.Numerics引用、替换私有ExceedsThreshold及增加FiniteUnits；Move／Up／状态机／public API均未改。 |
| 测试改动 | 只新增G15C/D/E、注册和辅助数值见证，删除无调用AssertThrows；旧80个方法体逐字保持，旧断言未弱化。见R/case-id-body-audit.json及两份fresh patch。 |
| 行数 | ReadAllLines口径：Models303＋Route395=698≤700；Cases1076；其余五文件113；八文件1887≤1900。 |
| 既有文件 | 对作者启动401文件清单逐项复核：仅上述两源变化，399份原字节保持，无缺失。八份修改前归档SHA全部正确。 |
| 作者新根 | 清单189项全部匹配实际字节，加artifact-hashes.json共190文件；未发现清单外文件。旧runs／bin／obj和两原R树保持。 |
| 依赖／设计 | 普通同步实例；生产只用System／Collections.Generic／Numerics／FlowPos。无Unity、QFramework、业务模块、文件、网络、时钟、随机、后台线程或新包。BigInteger为已有依赖。 |
| 工程 | 原csproj仅五个精确Compile、C#8／net6.0；无PackageReference／自定义Import／Task／Target。asmdef仍只引用FlowPuzzle.Core，noEngineReferences=true。 |
| 独立输出 | .NET前检查项目及祖先构建注入，未发现；三套obj/bin及CLI_HOME、NuGet缓存、TEMP/TMP均在R。没有写作者验证根或配置。 |
| 只读Git | status／diff --stat／--name-only／--check均exit0；no-index返回1表示差异，无空白错误。既有深路径和LF/CRLF警告保留，不据此宣称全仓干净。 |
| 审查保全 | G388＋旧R102＋旧C1-R101=591文件，加报告／输入13项共604项，启动至退出无增删或字节变化，无reparse项。每个.NET步骤另核606项，包含本次两探针文件。 |

本次只有新报告与R内证据写入；无分支、worktree、commit、push、子代理、新任务、Unity或依赖安装。
SD00协调三稿及018A并行工作不归为G1污染。规范轴可操作发现数：0。

## 规格轴：严格有限 double 比较

[RouteGesture.cs:248](D:/Unity/UnityProj/FightMatch/ExternalWork/FM-DEMO-026-G1/Input/RouteGesture.cs:248) 的转换和比较正确：

1. 设IEEE754指数域为e、尾数域为f、符号为s。次正规数和零为s·f·2^-1074。
2. 正规有限数为s·(2^52+f)·2^(e-1075)。除以共同单位2^-1074，即s·(2^52+f)·2^(e-1)，恰与FiniteUnits一致。
3. 指数提取屏蔽符号；尾数加隐含位仍在long正值范围内。±0均映射0；e=1覆盖最小正规数，e=2046覆盖最大有限量级。构造和Bind拒绝NaN／Infinity。
4. 先转BigInteger再相减，没有double差值上溢；整数平方与求和精确，不产生次正规舍入或下溢。正阈值下比较dx²+dy²>t²与严格欧氏距离比较等价，等值不拖动。
5. 只在原Move／Up尚未越阈值时调用；越阈值后的状态保持、回缩不得Tap、所有权与取消路径不变。没有epsilon、float或缩小输入域。

独立见证不调用／复制FiniteUnits作为oracle：期望来自相等数值、正数下一可表示值、非零正交分量和精确整数三角形。
R/Repro.cs共120行、Repro.csproj17行；生产与FlowPos只读Link，无替代生产实现。

| 数值见证 | 独立结果 |
| --- | --- |
| 旧C16～C19 | 全过：有限差溢出的Move、原格Up、相邻终点Up，以及Epsilon对角位移；原23探针源SHA仍34c20ff41fb4bf22d8d145853e423134faf3e17c0daac647bb911e3d7dd17351，未改期望。 |
| G15C/D/E的27组 | 按相同27个输入另写独立断言：逐组验证Move、原格Up及终点仅出现在Up；全过。 |
| 恰等／下一数／正交分量 | Epsilon、1e-200、6、1e200、MaxValue的等值；前四档下一可表示数；五档加Epsilon正交分量；全部正确。 |
| 精确三角形 | 3-4-5、5-12-13，分别乘Epsilon、2^-1022、1、2^1018；均恰等不拖动。 |
| 极端差与零移动 | -Epsilon→MaxValue大于MaxValue；MaxValue→自身无位移；Y轴1e308→-1e308超过MaxValue；全部正确。 |
| 另15组边界 | 正负零、负次正规等／超／对角、跨符号差、最大次正规／最小正规及相邻值、负正规边界、负MaxValue与正交分量、反向X溢出、平移三角形；全部正确。 |

补充探针共42组，各检查三个独立手势流程，并核一次Up、清捕获、完整两格路线、原绑定及越阈值后回原点的行为。
这些子场景不计入83个CaseId；无失败。规格轴可操作发现数：0。

## F1～F5／N1～N3闭环

| 项目 | 当前结论与证据 |
| --- | --- |
| F1 | PASS：Up末样本按阈值与格规则处理；G11、旧R01/R02/C20/C22及本次数字见证覆盖无Move、原格／相邻端点、恰等、重复Up和不可拖起点。 |
| F2 | PASS：Intent构造internal；View／Intent复制并只读包装集合，Context／Pair防御复制。G08/G12、R05～R07/R14覆盖源集合变更、旧观察和旧意图保持。 |
| F3 | PASS：Models:155在解引用前拒绝null Pair，参数异常家族；G09O/R03通过。 |
| F4 | PASS：Models:147只接Attack／FreeLink；非法Mode99拒绝，G09P/R04和合法G07通过。 |
| F5a/b | PASS：数学推导、原C16～C19及42组独立见证共同支持；两处旧缺陷关闭。 |
| N1 | 当前状态PASS：四标记原位均不存在，指定旧归档各32B及SHA均匹配§170。C2没有再操作它们；先归档校验再删除的旧顺序仍NOT VERIFIED。 |
| N2 | PASS：原点尾格／普通尾格／退格恢复分别存在；相邻自交及固化中段见证保留，G01正反/G02逐格断言及G09参数异常家族不变。 |
| N3 | 本次C2证据PASS：真实归档、同测试红绿、12步骤完整记录与原turn关联、原字节diff和预算已核；指定旧缺口按§184保留，不据新记录回填。 |

## G01～G10全契约检查

| 组 | 当前结论 | 预期与实际依据 |
| --- | --- | --- |
| G01 | PASS | 5×5阈值6：恰6保持Pressed、越6才拖，正反完整四格路线，最后格仅Up出现；G01/G11及旧R02、数值探针全过。 |
| G02 | PASS | S→1→2→1→S后重走(0,1)→(1,1)→(2,1)→(3,1)→(3,0)，仅最终六格，无退回Tap；完整逐格断言通过。 |
| G03 | PASS | 端点／固化中段短按一次Tap，空Cells/null角色；超阈值回缩、异格、禁Tap、不可拖起点及仅Up越阈值均按契约。 |
| G04 | PASS | 斜格、跨格、已访格、他对端点、固化占格、穿终点不追加；保留前缀，回尾或倒数第二格才恢复；G04与R10/R11通过，ProcessCell全分支已查。 |
| G05 | PASS | 活动指针离板／显式越界、Cancel匹配或全清、Bind清捕获，旧Up和回板不复活，未到终点不提交；G05、R12/R13通过。 |
| G06 | PASS | 重复Down／Up和第二指针不重置／双发，新Down可另起；负数及int.MinValue所有者ID验证通过。 |
| G07 | PASS | Attack缺角色／不可画／disabled不发路线，FreeLink无角色可画且输出角色始终null；非法枚举不进入另一模式。 |
| G08 | PASS | Attempt／Face／双修订／选人／enabled变化和同值Bind均取消；>2^53修订及带空格身份原样保留，输入／观察／意图隔离。 |
| G09 | PASS | 构造拒绝规定的null、空白身份、负修订、非法尺寸／模式、端点与固化冲突、非有限坐标及非法阈值；非法Bind先验证不改旧状态。源码检查结合G09与R03/R04/R13。 |
| G10 | 当前证据PASS | 纯模块／冻结只读输入／独立编译／83逐例零跳过通过，旧64当前行为保持；仅指定旧历史NOT VERIFIED，不作旧全工程从未改动声明。 |

注册、switch、C2 scope、作者红／绿输出与本次输出按Ordinal多重集合精确一致：83个唯一ID，各一次。
C1归档的80个ID及方法体保持，原G1的64个ID全在；新增仅G15C/D/E。未知ID返回false，Program逐例捕获失败并非零退出，零用例也失败。

## C2原执行证据与历史边界

R/author-c2-turn.json只保存准确C2实施turn的用户／执行／文件变更／final项，未拿多轮摘要作为证据。
真实顺序为用户授权→启动归档→新增测试→红阶段→生产阈值修改→同测试绿→旧23复编→交付与formal final。
原命令记录印证归档8份／启动401项；红生产SHA为51478f0439b92b2d00909e5d468505c69b39034ce2679459f28dcdd1f4d25da4。
红绿Cases均7e0b1739d7014e645d4456a1178425d6f9e2c988b4480812830fc158676c114c；绿色生产为d6a95368ba46a307bac2cf03120a1c5e7a90ed24b94fe7866d91cb3dd1459e30。

| 作者阶段 | 四步骤实际exit | 结果与UTC范围（2026-09-22） |
| --- | --- | --- |
| red | 0/0/0/2 | 80 PASS、G15C/D/E三FAIL；07:06:50.7201530～07:06:55.4645936。 |
| green | 0/0/0/0 | 83 PASS；07:07:50.4229683～07:07:54.6158046。 |
| independent（作者自验） | 0/0/0/0 | 原23全过；07:09:00.7091306～07:09:04.7287571。此名不代表新独立接收。 |

12个result的完整参数／cwd／隔离环境／UTC／stdout／stderr／真实ExitCode与准确实施turn原输出逐项一致；scope内嵌记录亦一致。
每步431条前后冻结不变，24份阶段源码归档均匹配；产物SHA正确。
仅作PE/PDB元数据读取，未执行作者DLL：红／绿／作者旧23及本次三个新DLL的CodeView GUID/stamp与PDB一致，全部源码文档SHA匹配对应原字节，零错配。
详见R/author-execution-audit.json、author-tool-result-correlation.json、compiled-source-binding-audit.json、author-preservation-budget-audit.json。

旧历史更正已落实：G1为64而非67；撤回旧G04G2“原点不能回尾／必然测试错误”的无证定责；G10B旧根因不可还原。
原C1三个实物日志为69/80、遗漏的2997de…79/80、80/80；两项无效红测试G04H/G11C有旧独立匹配DLL/PDB指令证据，不作生产失败。
本次重读原日志与旧独立证据，未改旧报告、旧退出、旧日志或原结论。
永久保留NOT VERIFIED：旧G1/C1完整源码与编辑差异、缺失旧命令／cwd／环境／UTC／stderr／阶段冻结、G1→C1累计增删≤420、四标记先校验再删除顺序。
§184只解除这些精确旧项单独阻断C2的作用；本次C2归档、来源、红绿、预算和原文件保全没有援引该例外。

## 本次独立执行与结束边界

全部.NET命令使用C:/Program Files/dotnet/dotnet.exe，原G/Verify为cwd，SDK实测6.0.412。
restore显式原NuGet.Config、R内packages；restore和build分别传同一新obj的BaseIntermediateOutputPath与MSBuildProjectExtensionsPath两个独立参数。
build --no-restore --configuration Release --output各自新bin -p:UseSharedCompilation=false；只执行本次新DLL。
作者与本次六个project.assets.json的sources/libraries全部为空，packages/output路径均在各自受控根；无网络或安装动作。

| 本次步骤 | 实际结果与UTC范围（2026-09-22） |
| --- | --- |
| 01_version | exit0，6.0.412；07:47:27.9834503～07:47:28.1417496。 |
| 02_registered restore/build/run | 全exit0；0 warnings/0 errors；83/83；07:47:28.7984459～07:47:33.2835332。 |
| 03_old23 restore/build/run | 原Repro.cs/csproj只读复编，全exit0；0 warnings/0 errors；23/23；07:47:33.6247418～07:47:37.1108063。 |
| 04_numeric restore/build/run | 全exit0；0 warnings/0 errors；42/42；07:47:37.4640040～07:47:40.8942633。 |

10步骤完整参数数组、cwd、环境、UTC、原stdout/stderr及SHA、ExitCode、前后冻结和产物SHA保存在R内同名前缀文件；汇总见review-execution-audit.json。
新注册DLL SHA：82ab63e0ed1d61b2da08f72dbcc76b766ca6c01f872540ae286ed005a16fdf1c。
新旧23探针DLL SHA：f95b3b3fa9dc51b8e736ace78f37e5bf06874df3cf13e041f239ebd105af04c8。
新42组探针DLL SHA：493e9c152d94ec2d0f3b8040f7f2e4d568a73ff2c2759c31d65c42a646b2ad41。
辅助PowerShell集合审计曾因object[]未转string[]而exit1，修正审计参数后完成；原命令／错误及未捕获UTC如实记录在reviewer-audit-retry.json。没有.NET失败、改测试预期或生产修改。
另一个摘要显示的八文件计数曾为null，原因是对OrderedDictionary直接Measure-Object；改按已保存文件记录计数为1887，真实源字节不变。

必要动作：无需代码返修；SD00仅在本次准确原审查turn completed后登记G1 C2独立接收。
Unity导入／EditMode、真实鼠标触控、019／026-G2接线、正式场景、026整包全部NOT RUN；G1接收不增加已接收功能数，也不放行G2。
最终保全与本次产物清单见R/final-preservation.json、artifact-hashes.json；报告交回后停止修改。
