# FM-DEMO-B01-R1 · DEMO-B01 独立代码审查

日期：2026-09-21（UTC+8）；审查者R；唯一被审实现包：FM-DEMO-006，依据现行任务包§77～79及仓库审查／验证规则。

**FM-DEMO-006 — VERDICT: ACCEPT**
Scope: PASS；Acceptance criteria: PASS；Verification: 作者实际编译exit0、EditMode 673/673通过，R独立核对源码、工具记录、日志、逐用例XML和文件摘要。
未发现必须修正的代码或证据问题；无需同批纠错包。接收只覆盖§78的测试夹具、来源、几何调用及捕获隔离，批次状态由SD00登记。

**交回门槛与独立性**

实现任务“执行 FM-DEMO-001 单步路线验证”：01a0bebd-edcc-7c72-8483-fa11c96874be；本次006 turn：01a0c1cd-3ec2-75c3-904c-b9adaccc3b7d。
先准备上下文，再以每次≤60000ms的wait_threads等待；明确收到该turn completed（10:50:41）且完整交付文档存在后才正式审代码，没有把旧数值复核或中间文件当交回。
read_thread对该turn返回items=[]；改为只读同任务本次turn的原始工具记录，取得实际基线、命令、退出码及交回输出；没有用作者自检代替独立结论。
审计记录：[本次执行记录](C:/Users/YYC/.codex/sessions/2026/09/20/rollout-2026-09-20T20-15-13-01a0bebd-edcc-7c72-8483-fa11c96874be.jsonl:519)，仅使用006的519～710行。
其中566行是起始不存在清单／原asmdef；573、577、582行是三段305文件基线；638、653行是进程结束输出；663、677、694行是交回及收口范围；710行明确COMPLETED。
旧数值独立报告仅作为002～005纯计算范围已接收的前置；本轮未重审其算法，未把该报告外推为完整随机业务、发布服务或可试玩Demo。

**范围与规范核对**

独立重算当前309份Assets/Scripts及Assets/Tests文件SHA，与实际起始305项逐一比较：304项完全不变，1项获准asmdef变更，4项获准新增，删除0。
新增仅DemoContentFixture.cs、DemoContentFixtureTests.cs及各自.meta；唯一旧文件变化是FightMatch.Core.Tests.asmdef的references追加FlowPuzzle.Validation。
实读起始及交付所存asmdef全文，按UTF-8/LF重算原SHA并与当前文本做精确替换比较：除原字符串后的逗号和新增引用行，其余字节相同；原.meta摘要保持。
原asmdef SHA256：836B9DE4FE2AAD84DED6116D2F46CDB81BFDC5406D440940694CC7A2FCFE524A；当前SHA见下表。
两份新meta为Unity导入的MonoImporter，GUID=125fcccc8d3ddd942947493dc2e82d50、50e464d985fc4944ba2aa90f0d6d18ff；独立扫描全Assets，各出现1次。
全Assets路径数322→326；作者实际路径差集仅上述4份。当前源码范围、交回增量及10份受保护输入摘要一致；既有34份tracked差异（+2809/−597）不归属006。
实施预算独立计数：280+407行C#、22行meta、asmdef新增2／删除1，共712≤900；交付文档111≤120行。
夹具、绑定、路线、证据均为internal；只使用System及现有Core／Validation能力，没有新增生产API、接口框架、玩家Model／存档／SDK句柄或发布能力。
测试程序集仍限Editor、noEngineReferences=true、TestAssemblies；生产Core及既有依赖不变。没有满盘、唯一解或未来可解性检查，没有不相关重构。

**七项验收与最小行为见证**

| §78验收 | R独立结论及依据 |
| --- | --- |
| ① 来源、映射与坐标 — PASS | [DemoContentFixture.cs:124](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:124)固定L1/L3原点序及A/B→color0/1、E01/E02、slot0/1；与boards及chapter/stages/enemies来源核对一致。[DemoContentFixtureTests.cs:28](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:28)四组硬编码预期逐点核C↑／C↓，[DemoContentFixtureTests.cs:41](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:41)核源SHA／定位、端点及两次副本，[DemoContentFixtureTests.cs:216](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:216)核缺方向诊断。转换只发生在CaptureSource；两方向均为SourceCandidate。 |
| ② 独立构造 — PASS | [DemoContentFixture.cs:178](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:178)不要求源方向，Constructed的源路径／SHA／定位／约定为null、转换为零基identity。[DemoContentFixtureTests.cs:115](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:115)直接构造4×4的CV05，A沿x=1从(1,0)连至(1,3)仍单步合法。没有玩家开放L3或W1+53字段／事实。 |
| ③ 深隔离 — PASS | [DemoContentFixture.cs:238](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:238)复制Level全部字段、每个pair及值坐标；[DemoContentFixture.cs:259](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:259)复制Solution／paths／cells；[DemoContentFixture.cs:214](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:214)复制绑定并封只读集合。[DemoContentFixtureTests.cs:137](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:137)破坏原嵌套输入和返回副本后，两捕获、下一副本、旧证据保持；[DemoContentFixtureTests.cs:170](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:170)试写只读集合失败，FlowPos读取为值副本。原始路线另存独立只读数组。 |
| ④ 修订与取消 — PASS | [DemoContentFixture.cs:70](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:70)以ReferenceEquals(Capture,current)并核cancelled；[DemoContentFixtureTests.cs:184](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:184)实际r8成功→r9不同方向→r10恢复原字节，旧证据对r10为false；[DemoContentFixtureTests.cs:200](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:200)同key／同revision／同字节新捕获、取消及null也为false，重新验证才有新捕获自己的证据。 |
| ⑤ 两验证层次 — PASS | [DemoContentFixture.cs:202](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:202)实际调用既有FlowSolutionValidator，对独立Level／Solution验证；三结果字段复制为只读证据。[DemoContentFixtureTests.cs:41](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:41)四种完整A/B候选只占6/16格且通过；[DemoContentFixtureTests.cs:98](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:98)删B后BattleRouteValidator通过而完整解MissingPath；CV05不因隔断B被拒。[DemoContentFixtureTests.cs:306](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:306)六类几何失败保留原错误码／消息，没有升级成捕获错误。 |
| ⑥ 范围与来源失败 — PASS | [DemoContentFixtureTests.cs:15](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:15)真实只读boards.json核固定SHA，变化明示SourceChanged并要求审定更新。[DemoContentFixtureTests.cs:227](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:227)至[DemoContentFixtureTests.cs:339](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:339)覆盖非法stage／enum／revision／key、必要null、缺绑定及缺嵌套对象；缺绑定按color核验。程序集及生产隔离见上，未添加玩家写入口或恒false发布属性。 |
| ⑦ 实际验证 — PASS | 独立实读本次两日志、进程结束输出及673个test-case；新增41项全Passed，原632项逐名并保留重复次数匹配且全Passed。最终文件／产物SHA与实际交回一致；测试后实施输入没有变化。 |

IsCurrentFor仅判捕获身份与取消，几何成败另读IsValid；当前的失败诊断仍可被记录，与§78的两个判断相符。
测试有效性：四组坐标期望来自独立常量，不复用被审转换函数算期望；破坏嵌套对象、取消及同值新捕获都是可失败的反例，未用总测试数代替行为验收。

**实际运行证据与本轮未运行边界**

以下命令由实现任务RUN，R只读核对，未重新执行Unity；实际Start-Process使用-WindowStyle Hidden、-PassThru及WaitForExit，两次运行串行。
启动记录先检查Unity.exe存在及无活动Unity；两次均正常取得PID并结束，无杀进程。参数与实际日志COMMAND LINE ARGUMENTS一致。
```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo006Compile.log'
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemo006-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo006Tests.log'
```

编译：10:42:49.022～10:43:20.350，PID27080，进程exit0；[FMDemo006Compile.log:518](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Compile.log:518)正常退出。中途Bee的ExitCode4后紧接DAG重建及两次ExitCode0，是同次编译的内部步骤。
EditMode：10:44:19.853～10:44:30.166，PID4968，进程exit0；[FMDemo006Tests.log:337](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Tests.log:337)保存本包XML。XML时间02:44:27Z～02:44:29Z，673 Passed，failed／skipped／inconclusive均0；测试参数无-quit。
两日志error CS／Compilation failed／Unhandled Exception均无匹配；已核全部新增41个用例的名称及Passed结果。
原632=BattleRoute42+Pcg32Core27+ExactRational56+ExactRandomSampler35+ExactScoreCalculator32+FlowPuzzle440。
以旧005 XML作回归名称基线：632条含629个不同fullname；按fullname及出现次数逐一比较，无缺失／替换／计数差异，新增恰为本包41条，全部结果Passed。
作者10:41:38的两份C#摘要已等于交回／本轮版本；309份实施输入最晚写入02:43:17.593Z，早于测试开始；本轮再次核对交回摘要均相同。
R实际RUN：Get-Content全文审阅、Get-FileHash、XML解析与多重集合比较、rg的路径／GUID扫描、原asmdef逐字节比较，以及git status／diff --stat／--name-only／--check；只读检查成功，tracked diff --check exit0。
NOT RUN（R）：Unity导入／编译／EditMode、算法执行、额外测试或自动修复，原因是§79明确限定只读审查既有运行证据；本轮没有发现必须追加运行才可判定的问题。
NOT VERIFIED：正式坐标方向／内容发布、M06战斗与奖励、H10保存、Player／Android／IL2CPP、UI试玩；均不在006接收范围。测试内对象身份令牌不能当生产或持久化身份。

**被审文件及证据SHA256**

下表为本轮实际核对的文件摘要；正文读取包括全部新实现／测试、直接调用接口与数据类型、原测试／程序集。旧数值源码只作范围摘要比对，没有重审其算法。

| 文件／产物 | SHA256 |
| --- | --- |
| [DemoContentFixture.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs) | 07104C559950D47EE4FF6EBAA89321033E51A83AA60BA118A7FC6AC18409E213 |
| [DemoContentFixture.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs.meta) | 31DA9C5AB02210DE3BB63F39012A32A17ACCA106C6746FAE00077DAC75E7E0E0 |
| [DemoContentFixtureTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs) | EB4CDCAF491BE99F85CA5433510A54259E6B559BCD64D7E2AD16DCAF84858CB7 |
| [DemoContentFixtureTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs.meta) | 8EE68CE58C66013F04F4A28BC32B8FFF64C5C14C21423900FA8A74F8BF9BA43D |
| [FightMatch.Core.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef) | 829ACDB13AC53136202AAF85B7E9CC5572085E720BB78F149A649AE63DFD66E5 |
| [FightMatch.Core.Tests.asmdef.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef.meta) | 1171DB5F6FB922469BDF6470DCD19AC2746BE9D223BDF119D264C928C285745F |
| [BattleRouteValidator.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleRouteValidator.cs) | E535F092BAF9F6E29F60CEAEC69D2385E17E481E26294BACB2C9CE2CD132B3A2 |
| [RouteValidationResult.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RouteValidationResult.cs) | 948F43F12D23732FA06FFD5D26FCBCB018496862312F18EA2ED3389D5123ADFA |
| [FightMatch.Core.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/FightMatch.Core.asmdef) | 6D6137F686A78F223C6C93810D124DAD6E1F4D91B5CCFA40068A9C44FF2D867C |
| [FlowSolutionValidator.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Validation/FlowSolutionValidator.cs) | FB732BE5E11FCDF176A92D7219CE6B9894BFB9B8D976D6AE83EEB1C4E252B579 |
| [FlowValidationResult.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Validation/FlowValidationResult.cs) | 6198F6816D3A0A861A00A2532CCF511B9491CE0F489C4E6DB20A9D61C22B3646 |
| [FlowPuzzle.Validation.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Validation/FlowPuzzle.Validation.asmdef) | 9E7B08FB80099CC6AC4F6820AC83C073959FE999EEAC94208EC04557EA1621DB |
| [FlowPuzzle.Core.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef) | 9F684E62A102825715D9D6DB929FDF9EF0D23A636C6AD38C59BFBC4168C9E3A8 |
| [FlowPos.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPos.cs) | 52CC1AA7FA88C50B9CD3D17071F3DF99098ECAD1ADA58B9D5636FAFFE7CB4F6A |
| [FlowLevelData.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowLevelData.cs) | 13F9D629009B95A58DAB452EE9429ABE6ADC492CA3037A8DA2BE27728C000CC6 |
| [FlowPairData.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPairData.cs) | 700458B31665FE69FE7E723A564ED7B0846AFCDE9202244813B180D2609B10F3 |
| [FlowPathData.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPathData.cs) | 71E7B5D15AF03B9C4037D9F91136FEEEEB8EFF3CF738D43D5EB33644250B6E42 |
| [FlowSolutionData.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowSolutionData.cs) | 6FB6D5C60BAF9A3B4E724DCBBCC6EE8AFD7DCE0DFE4104A0661CFAC9CA0CEC7C |
| [FlowPathUtility.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPathUtility.cs) | AF6914555F3374965337C36816FA839888A412C2A58202A019BD743043661949 |
| [BattleRouteValidatorTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleRouteValidatorTests.cs) | 4292DA12BEC706736E88F57013955561C8157B6C071EBC894C8E0E5C8EC4590A |
| [FlowSolutionValidatorTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/Validation/FlowSolutionValidatorTests.cs) | F0DE8ADA930FEF1074E82308AD43FC0E4EF310C4D130E3F28DA424D5208EF669 |
| [FlowPuzzle.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef) | CA8A4C0F4CFC1E395EEF8AF9DB42FF4240226B39BE37AA26163BA33AB4DF4A1E |
| [demo-006-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-006-delivery.md) | 6A4EB6991673D83FF38DFBD97701146D90F73CBF7E026676472100080EB6D467 |
| [demo-numeric-independent-review.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-numeric-independent-review.md) | 539D7805A143BE717320EC61364CA4D107B29B70C7D4EB59B949D54E31219925 |
| [content-validation.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/content-validation.md) | B430F3384AEB20BF5742BF29C5949AF13CF25EB4EAEA31BEDBBAC5DA8BEEAFC9 |
| [configuration.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/configuration.md) | 1AB07F8E5AFB1D2FDFC4A4A2CF5E436E8F876F1E4371A94A68CA9BFB25C4794B |
| [README.md](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/README.md) | 8216072DCBD0904B14E3A412AD73EB050CFC5534C5B8CF31286DACDF7D1D5548 |
| [chapter_model.py](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/chapter_model.py) | 2052792CCC3AAE007F6A23A2F799D4D724C3ECFDE97644D9731E4D706BBEC390 |
| [boards.json](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/boards.json) | 671467CAF72C52EB83A03B396AD82F5557A47B2F9CBD3F9DA6D0EC560A7D2AAD |
| [stages.csv](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/stages.csv) | 814536D3B9DE969F0F85673985E9B188857166C83967BF67AF74EAA6E3A9DB2D |
| [enemies.csv](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/enemies.csv) | 238493BEC0BD293B12D0CFDB0395D4246FBB379E59573EC94A590E5DC25EC9D9 |
| [manifest.json](D:/Unity/UnityProj/FightMatch/Packages/manifest.json) | E15E302B5C4342D530AE52F31C785A9626B4FF42ECC1AA7612FCC3F0637B872A |
| [packages-lock.json](D:/Unity/UnityProj/FightMatch/Packages/packages-lock.json) | 160073F2CD54A18FC4A3995C64B53DE964356C5F36B66020FB66EB165E01C31A |
| [ProjectVersion.txt](D:/Unity/UnityProj/FightMatch/ProjectSettings/ProjectVersion.txt) | 9B7F178DD8C050E64943DB5709939F39FD3191C18C6C557143963975891B50E2 |
| [FMDemo006Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Compile.log) | D8183A6DADBA56C426BD168EC15A20AB1EC6CC971435311C570BEAFF994971B1 |
| [FMDemo006Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Tests.log) | ADFD7C45D0E36E5F61EB93A0F92508297466EF1CD847152436C250A2D4FB58FC |
| [FMDemo006-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo006-EditMode.xml) | C48ED5AC221C8EFFDCF14A29CAA9F662137A341C653628E4305A181D379A9C49 |
| [FMDemo005-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo005-EditMode.xml) | 4AE312EA8459B6D3C708EA7C37CB7F250F10C9A91EB4BFAA44CF4EB6D522A9F4 |

源码／测试范围清单采用正斜杠相对路径、Ordinal路径排序；每行“SHA256＋两个空格＋路径＋LF”，UTF-8无BOM。原305项完整内容在上述三段工具输出中，最终清单为该基线加唯一asmdef替换及4项新增。
起始305项清单SHA256：B9006A712929450C21310BBCF9DB98A34A1020E3565CAF783622472658A19198；当前309项清单SHA256：2AD9F8BA7AEC356C8DE746F58918FBF13FECC53A3035D36A1D6CD924AB55E50A。

**审查收口**

唯一写入：本报告；未改实现／测试／meta、协调三稿、依赖、运行证据或Git，未创建其他任务／子代理。
收口静态验证：115行≤180，62个链接及行号有效，尾空白／冲突标记0，唯一VERDICT；309份实施文件及74份只读输入摘要保持，Git status／name-only／stat与写报告前一致。tracked diff --check exit0；本新报告的NUL差异检查exit1但无空白错误。结论只对上表版本有效，SD00据此登记DEMO-B01并决定后批派发。
