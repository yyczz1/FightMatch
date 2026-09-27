# FM-DEMO-018C-2-C1 独立复审

VERDICT: ACCEPT

Scope: PASS。C1-01～05: PASS。原 F1 已关闭；本次没有新增待修项。
本结论仅针对 FM-DEMO-018C-2-C1，不改写原 C2 的 NEEDS_FIX 历史，也不代 SD00 接收018整体或派发019。

## 1. 身份、冻结包与完成门

项目根：D:/Unity/UnityProj/FightMatch。以下时间均为2026-09-23 UTC。
R任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确本次turn：01a0ccf0-4b79-7460-9974-51da42cabb3f，06:24:55.199Z task_started。本次原生turn_context核见gpt-6-astra／max、local，未用旧回合设置替代。
按 [system-task-packets.md](system-task-packets.md) §226～228执行，沿仓库审查清单及code-review规范／规格两轴；依精确包自行审查，不启动技能的并行代理流程。
r114签发全文SHA256=06b2887830177d8800ba156acc2c4c11823975cef8646e120792964133f1d717；§226～228规范化SHA256=77f6be9d6fc36d60e987ab5f2faeaa1913acedb0cbb0a9e864ad4471e0466bda。
06:59:40Z重新核当前冻结文本：§209～212=14157867553e4c18ca2933554e2bd4966b01fd1cdcdd2de7d5a02e7f22e71d8e；§217=584b272d8b7e270cdf1b141426d0aa33360f2837612b557597f939b4b34f27ac；§224=8aac59482fe5e72393c5dcb8acceaffb0c60ec83b66a87bfb1b7d0004c0b4a8a；本次§226～228同上。均与C1 scope所存全文逐字相同（LF／TrimEnd／单末尾LF）。
C任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确C1 turn：01a0ccef-5371-7bd1-a243-d8de624637e7，06:23:51.711Z task_started，06:24:06.861Z turn_context为gpt-6-astra／max、指定项目。
R先用wait_threads核该turn的inProgress；完成时工具明确返回completed、error=null，再用read_thread复核。工具未返回消息正文，R直接核原生formal final与完成事件，而非以报告出现代替完成。
准确原生formal final为06:54:20.625Z、ordinal7626、AgentMessage id=msg_01289151d0393480016ab3778629fc87d0b5d99be03cec646d，非async问答；task_complete为06:54:20.889Z、ordinal7630，均属于上述C1 turn。
原生路径：C:/Users/YYC/.codex/sessions/2026/09/21/rollout-2026-09-21T20-49-35-01a0c403-bfa1-7e90-b503-c0fcd61f23c1.jsonl。未误用旧C2已完成回合；最终裁决在本C1完成门之后。
固定证据根E1：TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/；原C2根E0：TestArtifacts/FMDemoB14C2/bf4b05e72b854bd9a5711ca42d609794/。

| C1正式交付，与准确final及实物一致 | 长度 | SHA256 |
| --- | --- | --- |
| [demo-018c2-c1-delivery.md](demo-018c2-c1-delivery.md) | 9012 bytes／69行 | a2684b7b4749171cd0885774e09cc7c59b9a82372c252cbc82aeb6765f8fc038 |
| [demo-018c2-c1-scope.json](demo-018c2-c1-scope.json) | 394898 bytes | 85934bc1127ee8f6246786f7e70f45d6d49ef6908421276221769cdf581b1c78 |
| E1/late/formal-bindings.json | 18001 bytes | 494f2cf98e4e40aed03e66ae75ee0f5ac4d1ca4212fd7003297eefb6cd2c4164 |

## 2. F1关闭与两轴结论

生产改动仅为 [FightMatchDemoArchitecture.cs:23](/D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs:23) 的实例重复关闭保护及其私有标志。
正常首次关闭时，deinitialized为false，仍先进入原runtime.Close的owner-thread／Busy检查；只有Close正常返回才把标志置true。已正常关闭A的后续Deinit在上游清理前抛InvalidOperationException("AlreadyDeinitialized")，不会再到达QFramework的mArchitecture=null，因此不能注销后来工作的B。
首次租约Close抛异常时，标志赋值未执行，原异常及CloseFailed诊断照常保留；后续owner线程再次收尾仍可进入原Close的Disposed分支并完成上游清理。没有把Runtime的Disposed误当成已完成Architecture清理。
[新增测试:661](/D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateApplicationRuntimeTests.cs:661) 通过真实A初始化／关闭、B从同profile恢复、旧A再次关闭被拒绝，核B的Interface／Model／System／View引用与租约保持。它还查原结果、执行真实Enter写入，关闭B后再开同profile；没有仅断言一个标志或使用伪Architecture。
测试的using实际编译为finally，对保留的A／B／重开实例收尾；红阶段在重复关闭断言处失败时，仍通过持有的B关闭其租约。未反射修改全局入口。

| 审查轴 | 独立结论 |
| --- | --- |
| 规范（Standards） | PASS。只改精确两文件，新增51行、删除0行；无公共接口／上游／配置／meta改动或无关重构。1个实例私有标志足以解决问题，没有新抽象、store或静态业务状态。 |
| 规格（Spec） | PASS。F1的准确输入有真实红绿见证；正常关闭、WrongThread／Busy、CloseFailed后再次收尾及pending保留均符合§227。C1-01～05证据闭合。 |

| 验收项 | R独立核实 |
| --- | --- |
| C1-01 | 仅增加指定测试、生产仍为原SHA时，Unity实际运行1项并因未抛InvalidOperationException而失败；不是编译失败、0项或缺XML。 |
| C1-02 | 同一新增测试在最终无filter全量中Passed；旧A明确拒绝，B引用／租约／查询和真实写入可用，B关闭后同profile重开。 |
| C1-03 | 原WrongThread、事件重入Busy、CloseFailed再次收尾、跨实例pending恢复及事件测试未改且全部Passed；没有吞异常或封死失败收尾。 |
| C1-04 | 原3068 fullname的Ordinal多重集合完整保留，新增恰好1项；3069全Passed、零跳过。另567实施项、580个非目标Assets及全部310meta／GUID保持。 |
| C1-05 | 三轮真实Unity命令与PID／UTC／实际退出一致；修复后编译及全量exit0，源码／30 DLL-PDB／XML同版；新归档与正式报告、原生记录绑定准确C1 turn。 |

## 3. 精确起点与范围

原 [C2 scope](demo-018c2-scope.json) SHA=8b54e95a80d8d2f3008205048ed8fcfdd50735e5664caf413788a570751d5758；原 [R1报告](demo-batch14c2-code-review.md) SHA=8fece5d7c0c82a8c1279913274b5a2a13006c2608dbf186319735b3b1beba433，原判定与F1保持历史原文。
C1原569起点capturedAtUtc仍为03:05:13.7509389Z，canonical=e5eb7e40fb8beb38230af4c758c19c826f4671155756b02e9d4eb1fc578bbc69。R于06:29:34.856～06:29:41.156Z独立读取569项，当时全部长度／SHA吻合，未以中途源码重建起点。
作者另记实际C1入口06:31:51.2059346Z；正式scope保留完整569原列表和569最终列表。原550历史及§224已授权单行例外未被重置、回写或扩展。

| 唯一源码差异 | 原 → 最终 | 原SHA256 → 最终SHA256 |
| --- | --- | --- |
| Assets/Scripts/FightMatch/Application/FightMatchDemoArchitecture.cs | 3219→3388 bytes，71→74行；+3／-0 | a7ee449c612e73f343e6345e88389b686600fe83270ef699e704d7909a05215f → 22f074564f8d779363afae7a38dffe7395b6f98827844ec9ddc778a46a83f1db |
| Assets/Tests/EditMode/FightMatch/CandidateApplicationRuntimeTests.cs | 37960→40120 bytes，701→749行；+48／-0 | cd2fe460417f862f089ffd76e7a36506b5f9555ce8c0dc6610f10eeccff32546 → fb4c1b120f22836cabd4477661df201ceabbc3984a0661c0f0c676117650df59 |

R直接验证：删除生产新增三行即可逐字还原原文件；移除新增48行测试块即可逐字还原原测试文件，原49项测试没有任何方法／名称／断言修改。
总差异51行≤100；五生产914行≤2000、八C#2373行≤3800、取证帮助82行≤280。报告69行≤240，scope394898 bytes≤2048KiB。
最终569实施／555 Scripts＋Tests／582 Assets／310唯一GUID，没有新增源码、meta或程序集；其他567实施canonical=b09f389d28edc8b850df4ab6adb327b0f40755e57c471e5783011fb2a2c4cbfe。
最终实施canonical=5da1c80cde84ca32af8be3c3b43eff7ee0c02b98a476e5953b26d9040c00f3a5；Assets canonical=d541f0a6d7ddb70e930914940119227944c88be1341de9e28ebd991754faa124。
06:58:05Z交付后再次实读：569项全部与最终scope吻合，580个非目标Assets逐项保持，224项包内保护输入全部匹配。
R另覆盖469项文档／.agent／Packages／ProjectSettings／根规则／权限输入；除三协调稿外466项入口／出口canonical均为8da155a8c12cacff648847e88ce96ec36d043fc1253f2f2a55c473345f0f11f0。
入口后两个协调稿的变化属于SD00：06:30:45.943Z、ordinal18016、exec-eaf73fd8-deae-44cc-bbbd-22fe8c6afcd3、turn=01a0cceb-9756-7961-bdbf-c1c989e0b28b，路径为integration-review.md与system-task-packets.md；冻结条款未改，非C／R越权。

## 4. 真实红绿、命令与同版性

固定exe：D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；cwd／projectPath均为项目根。
R读取82行取证帮助及三轮实际原生命令、process-check、start、process-result、end、verification；每轮先查无Unity冲突，Start-Process Hidden／PassThru，WaitForExit后先保存真实Process.ExitCode，再收证。没有把包装器exit或chunk_id当Unity退出。

| 轮次 | PID | 起止UTC／实际exit捕获UTC | 实际Unity exit／结果 |
| --- | --- | --- | --- |
| red-01 | 32288 | 06:34:08.2435477～06:34:47.1194717；06:34:47.1722104 | 2；total1、failed1、skip0，准确F1断言失败。 |
| compile-01 | 14512 | 06:36:13.0558618～06:36:21.9281000；06:36:21.9916620 | 0；修复后编译，无编译错误。 |
| tests-01 | 14944 | 06:37:07.9720787～06:39:03.1107982；06:39:03.1634475 | 0；3069／3069 Passed，0失败／跳过。 |

准确原生CommandExecution分别为：ordinal7468／exec-91288adb-ce4a-4c72-87e5-c35ac1afaa18；ordinal7487／exec-4bf18a08-38a7-4446-bf7e-6308ac0c2595；ordinal7509／exec-3144fa11-3073-4bd8-953d-d90e2c1c487c。包装器均exit0，其中红轮按预期失败成功收证，不冒称Unity exit0。
红轮仅用指定testFilter：FightMatch.Core.Tests.CandidateApplicationRuntimeTests.A08_StaleArchitectureDeinitCannotDetachCurrentInstance；XML在E1/red/EditMode.xml，失败原文为Expected: <System.InvalidOperationException>／But was: null，栈指向测试684行。
红轮before／after生产均为原SHA a7ee449c…，测试均为最终SHA fb4c1b12…；红源码canonical=dd608e5fa67f7aa8bce664492ec5b56c55682e76043fec8dabdfb6c5b909b9d1。生产于06:36:00才修改，晚于实际红轮完成。
红XML4848 bytes，SHA=7c746e6673a0be86d5d202646158619c06d08f2748430b614ceef1908c6f246c；完整失败日志、源码、DLL及单个完整witness对象均保留。
编译参数沿-batchmode -nographics -quit -projectPath指定项目 -logFile本包Compile；绿色测试沿-batchmode -nographics -runTests -testPlatform EditMode -testResults本包XML -projectPath指定项目 -logFile本包Tests，无filter、-quit或历史专用参数。实际完整argv在各轮start.json及上述原生命令内。
R直接解析原／新XML：3068 cases／3063 distinct fullname → 3069 cases／3064 distinct；原Ordinal多重集合无丢失或次数减少，新增恰为指定测试，全部Passed。原WrongThread、Busy、CloseFailed再次收尾及跨实例pending见证均Passed。
最终XML FMDemoB14C2C1-EditMode.xml为2120866 bytes，SHA=9f2b7b054e284d82f00cccc1eb7ae17b892800c6d689005f8d945173a30f2bbd，与runs/tests-01/EditMode.xml原字节相同。
最终Compile.log为62710 bytes，SHA=3fcca9d5af30afea2a79309e67751f1aae38c83839aab88bca30a816ac976e8f；Tests.log为90848 bytes，SHA=8a2a10f84fceca22fd50c2ec3f10ab5e43da328b7f86c779fdf1e802864223bc。
R独立枚举三轮before／after共六份实际source副本：每份836项，与Assets＋保护输入＋当轮DLL的完整集合逐项长度／SHA相符，无漏项／额外项。四份绿色569源码均为最终canonical。
编译后／测试前后及06:58出口实读的30 DLL/PDB均为canonical b0d57dd23175a74d3969246db04a51e9eff7d24d7a866ad9f42269ec69cc8bcb；红轮前后旧DLL摘要分别保留，没有混充绿色程序集。
两次静态取证失败是行数选择过宽及PowerShell字符串构造解析错误；另一次报告构造JavaScript解析失败未进入写入。原记录均保留，未引起额外Unity轮次或源修改；不把收证错误当成生产测试失败或抹去红轮。

## 5. 旧证据保全、新归档与完整绑定

按§227引用原R1已完成的字节审核及完整注册表，不重做旧全树SHA或旧Unity矩阵。原130077项字节审核canonical=ff88c8acaef983ae748183530b49b1b2f79eb6e73f8832bc59cc8eb4717175be；C2完整新证据审核仍由原R1报告及E0绑定支持。
R本次入口06:30:16.080～06:30:59.786Z独立枚举44旧根、161718文件、8720138352 bytes；路径／长度／mtime摘要=483ea761b0a410f2ead0525b8e21843ad251525a5d269aaa92ea8133ae664925，与C入口完整161718行列表独立计算相同。
R另逐项对更早130077行已审记录检查当前长度／mtime，变化0。旧记录唯一缺mtime的E0/archive/registry.json.br已重新实读SHA=e724aa0824813cbcadcf6ff7d86df737096481251052fc194e52baa7417c7308，与原审核相符。
C完成之后06:54:54.386～06:55:35.647Z，R重新完整枚举出口；44旧根各自的路径／长度／mtime摘要、文件数与字节全部等于入口，无无法解释的旧变化或散落文件。不把此元数据复核表述为本次重新读取8.7GB全部旧字节。
新增自然根精确为TestArtifacts/FMDemoB12/63c5745f9595482e8e280f288c2f095e（1087文件）、FMDemoB12/6f554110b7334ea9b12ecfe947d03b9c（3文件）、FMDemoB13/a5cba0dd714e46e492eb0e37b1b82c37（342文件）；合计1432全部纳入新归档，未覆盖旧根。
原C2两报告、R1报告、旧XML及E0最终binding出口SHA保持；更早550与C2的569捕获未覆盖。当前四个Tools源／配置仍在567不变项中，其历史字节沿E0已审快照引用，没有复制旧证据树到E1。

R于06:48:08～06:48:26Z独立枚举新选源、payload及全部ZIP条目流：8173文件、523249106 bytes、2空目录，三方路径／长度／SHA／空目录集合一致，零遗漏、额外、重复或字节差异。
三方canonical=ec0c7bfa86b4e342b45134848363589f86bb6af2c977080067898526ad8ac31d；源选择额外257项恰为224保护输入＋30 DLL/PDB＋本包3日志/XML，五个根为Assets、E1及上述三自然根。
E1/archive/evidence.zip为95644746 bytes，SHA=bcb18f6bd9f71f7c57630bdb5ad34901ad073f73b54b4d474c1197fe0597871a。archive／late和两正式报告明确自引用排除，不把旧证据树递归复制进来。
注册表索引5675 bytes，SHA=d877e724350924f0e68611cace8ffa7e40a0759388076d016b6bbb4d9975db35；16份完整JSON原件的长度／SHA及完整行数均核符，含入口／出口各161718、原／终实施各569、3069测试及三方8173归档行。单项redWitness为一个完整对象，并与独立解析的1项失败XML对应。scope的注册表内容与索引相同，未裁剪清单。
根原生导出23条（22 CommandExecution＋1 FileChange），截止ordinal7553；late最终导出29条（27＋2），截止ordinal7609。R逐条原始JSON行与准确C1原生会话匹配，两个导出均零遗漏／不同，未导出推理。
最终原生导出814216 bytes，SHA=053125d375c6c886951e6025552a335323e15b4b709fb56ee0a9cdf83244fe6c；导出自身、最终binding和将来formal final的因果排除明确，后续完成事件已直接核实。
06:57:17Z核最终binding／注册表引用的35个不同实物，长度／SHA全符；另核binding本体SHA与formal final一致，7个late文件没有未绑定项。历史生产SHA只作为红轮／原起点记录，当前源码另由569项实读验证。

## 6. NOT RUN与交接

R本次未运行Unity、项目测试或额外探针；以上运行结论来自独立检查C的真实红绿证据与源码，不是R自执行。R仅新增本报告，未改作者文件／旧报告／协调稿，未创建任务／代理／外部模型调用或Git写入。
本回归证明同一EditMode进程内真实Architecture实例及M12目录／租约顺序，不称新跨进程、PlayMode或真实触控验证。
NOT RUN：旧C-1六进程／既有强杀矩阵重跑、新强杀／断电、交互式Editor、PlayMode Domain Reload开关、真实触控、019／024／025／026-G2／027／028／029、场景、正式PlayerSave／发布和Demo体验。
原C2已通过且本次未改变的事项按正式R1审核引用；本次没有更宽的功能接收或下一包授权。SD00核本R准确formal final与task_complete后处理F1关闭及后续接收，R不发送任务间通知，交付后停止修改。
