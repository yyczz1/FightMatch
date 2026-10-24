# RES-COMBINED-V01：一次编译＋89项组合合同

2026-10-08，`DESIGN_COMPLETE_NOT_ACTIVATED`。责任见[中央交接](central-resource-combined-v01-design-handoff.md)：`/root/resource_combined_design`设计，中央`01a0e401-511d-79f2-b47f-3ab0ade1681b/local`唯一收件。本机执行仅C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh、另签fresh actual。当前native许可0，旧租约不恢复。

## 1. 固定输入，先完成实施与审查

R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；P=`R/TestArtifacts/FightMatch/RES-01A/P01/projection`；K=P同级`package-cache`；E=`R/TestArtifacts/FightMatch/RES-COMBINED-V01/run`；BC=同级`bee-cache`。精确路径、逐叶bytes/SHA/blob、算法、源图与恢复账本统一在[inputs.json](../../../TestArtifacts/FightMatch/RES-COMBINED-V01/source-plan/inputs.json)。所有检查均为只读，native=0。

| 集合 | 叶数／字节 | canonical SHA256 |
| --- | --- | --- |
| 当前共享 | 1036／32657794 | `61f1364be566b5266468ea38a19caba1d172428dc7e5fda06b97398c89c83ba8` |
| P原前像 | 1035／32149498 | `3871bc36f096be53d6570c4d277bdcf8ea4d232825353861cf86a04ca201b207` |
| 拟导入P | 1037／32658037 | `a27ee9a2192ea7a4fecf1ca85e3dbbe6cb011dc08a0c9cb6983da70192a6578a` |

共享逐叶等于G02 group-freeze。P按`projectionProposed`穷尽清单做17覆盖、4新增、2暂移；Packages已一致。唯一额外叶为P已有`Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`，243B／`97eb4963b6d27d4669a021853e7363a3bbe386269f9293ff6d4608c772be39c3`；M03自然来源已实核同字节。此包仅保留P的meta；共享采用须独立收件并重封输入，不暗加第1037共享叶。任何漂移先停。

实施只从FIX05 reviewed runner作差量：基线54760B／`148cbc0b63a3bb6371343ed20d6205ae9b44a02cdfde8c1cd6ad9766e4e2266d`，PR18 head `ac5378d566e1b0cbb43c400c3f302b5f0c3f82b8`、102离线通过、native0。保留并发保护、no-clobber transfer、同快照owned分类与绝对收尾；参数化R/P/K/E、当前输入和限额，接入下面I/T、有限缓存暂移及编译来源判定。不得另造监护框架。

新文件限E下`runner.py,replay-check.py,replay-results.json,inputs.json,preparation.json`及签发时`activation.json`；动态证据限inputs的`evidenceSlots/transferPlan`。离线回放只调用实际差量函数，模拟Popen/ps/信号；覆盖I闭合后才T、T不带quit、89计数、SDK来源映射、43缓存移交、竞争漂移和期限。准备最多120秒，回放最多2轮累计30秒，首过即止。新runner准确head须GitHub PR Code Review接收；FIX05历史接收不覆盖差量。中央随后封签脚本、数据、case、actual及执行许可。

## 2. 缓存与本轮编译来源

K实核10315文件／732722938B，完整树SHA `b18af973da7bdb0716f7e2e367aeb53be4c7ebbdf3997e4b201b8e898c86f8e2`。53个安装包完整树均等于P03/run.json；其中YooAsset@3b4cfb36cc为2070文件／30567744B，SBP@1.21.25为327／1024192B。两SDK逐叶另封，53节点版本/source/depth/edges、P02路径及本机ProjectCache均固定。启动前后核图与payload；不能用旧P03编译替代本轮证据。共享根和M03没有这两包，不选它们补下载。

普通refresh可能复用未变程序集。采用**一次启动前的精确可恢复失效**：先保护源/目标并暂移`compilePlan.proposedParkExactLeaves`的43叶到`E/park/compiler/<原P相对路径>`：12程序集各dll/pdb/ref.dll共36叶，加现存单图dag、payloads、json、fsmtime、derived、outputdata、inputdata共7叶，总22357429B。清单是穷尽路径，不是glob；P的`TundraBuildState.state`及`bee_backend.info`留原位。旧图暂移避免延迟核图先使用已移出测试源；不改rsp、不改源来制造Csc、不清整个Bee/Library或全局缓存。

I/T继承一个新空外置BC，通过`BEE_CACHE_DIRECTORY=BC`隔离旧全局Bee结果。Unity官方[2022.3 UnityBeeDriver](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2022.3/Editor/Mono/Scripting/ScriptCompilation/BeeDriver/UnityBeeDriver.cs)定义此变量并传给backend；已在本机18f1 CoreModule中核到同名符号。官方[EditorCompilation](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2022.3/Editor/Mono/Scripting/ScriptCompilation/EditorCompilation.cs)保留上述两个状态文件；本43叶方案是依据现存图的局部推导，尚无native成功证明。精确18f1源码URL未取到，不冒称字节对应。BC不能从旧缓存填充；本轮自然产生后供T复用。

I须对inputs列明的8项目＋4SDK程序集取得本轮真实Csc；`priorCompileBindings={}`。固定当前源/asmdef、编译器工具、完整rsp及rsp2/附加文件/options/defines、全部引用、图依赖与compiler输出、最终ScriptAssemblies DLL，SDK源支持`Library/PackageCache`及Packages逻辑路径的唯一映射，排除Samples~。源从启动到取证受保护；引用通过本轮图的产出→消费顺序及无后续改写绑定；I结束闭合后、恢复前保存完整字段，不能只存摘要承诺。无后处理时产物与导入DLL应相等；有IL后处理须完整链，否则INCOMPLETE。记录全导入输入，较新AndroidBuild/布局测试不得丢失。T编译来源仅可复用刚封好的I完整链，按原六字段精确比对；若新Csc则重新证明。缺链不启动T，不追加编译入口。

## 3. 精确I/T及真实AS

Editor绝对路径与86759760B／`71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`已固定；2022.3.18f1 Intel、StandaloneOSX。以下是非shell argv，变量按inputs展开，cwd=P：

```text
I=[Editor,-batchmode,-nographics,-quit,-buildTarget,StandaloneOSX,-projectPath,P,-logFile,E/I/editor.log]
T=[Editor,-batchmode,-nographics,-buildTarget,StandaloneOSX,-projectPath,P,-runTests,-testPlatform,EditMode,-testFilter,F,-testResults,E/T/results.xml,-logFile,E/T/editor.log]
```

F取[主测试89清单](testing-resource-combined-v01-cases.json)的`selection.testFilterArgument`，24448B／`d53c49167d522532a6cc87218bfa5e961ab3a096af06209fb694726fca2ed0e8`；完整argv见inputs。不另发现测试。I通过且owned闭合才T，各最多1次、0重试。每阶段记录与重置已闭合的volatile owned状态，保留I历史；T重核消费者，旧PID不成为新豁免。

两阶段环境仅覆盖K、BC、一个新`TMPDIR=/private/tmp/fm-rcv1.XXXXXXXX`（canonical≤40B、本人0700、无链接、初始空）。仅T再设置`FIGHTMATCH_ACTIVATION_TEST_ROOT=R/TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V01/state-tests`。所有新根在本次检查均不存在，激活时重核；AS根逐级真实目录。沿固定AS实现执行真实Move/Replace/Flush/reopen，以及AS07的存在目标和悬空目标两种真实symlink；链接仅为`AS07/product/resource-state/v1/active.json`，目标分别为`AS07/product/external.bin`与`absent-target`。不跟随链接做监护，不弱化断言或跳过。AS07固定实现＋新Passed结果绑定实际调用证明；不要求监护恰好捕获短命链接。AS结束根空、sentinel不变，未知残留保留上报；不访问旧隔离树或玩家档。

## 4. 资源、闭合与恢复

准备120s、I360s、T180s；各阶段自然闭合60s＋TERM确认30s；恢复60s，总机械≤900s。Popen起计阶段；探测、审批调用与信号共享绝对期限，墙钟/思考/等待另记但不顺延已启期限。复用FIX05同一新鲜快照先发现owned再分类消费者，逐PID核PID/start/exe/argv/cwd/父链；每PID最多一次TERM，无KILL。ps/lsof不可用或消费者/锁存在即不启动。原ADB窄例外只在新激活逐项重新封签；ADB不获信号权。BeeLocalCacheTool若自然派生，仅按本轮完整父链、固定Editor内exe和BC证据纳入owned，不能按名称豁免。

E≤96MiB（含compiler前像22.36MB及新compiler归档≤32MiB，预留末回执1MiB）；单log≤8MiB、XML≤1MiB。K≤1GiB且字节冻结；BC≤256MiB/8192叶；P生成≤4GiB；外置空闲≥4GiB；TMP≤16MiB/512叶；AS≤1MiB/128叶。超限停止后续入口并闭合，原失败保留。自然生成限P的Library/Temp/Logs/UserSettings/obj、根csproj/sln；已安装PackageCache冻结。唯一额外源允许既定3534B SceneTemplateSettings自然字节，另计导入摘要并归档。其它meta/源漂移失败。

新鲜owned全部闭合且消费者核验通过后，先固定最终compiler后像并按43路径移至`archive/compiler`，再把已暂移的原43前像逐叶归还；未生成路径只归还原件。源21后像归档、17覆盖恢复、4新增移出、2暂移归还；P三目录必须恢复原1035摘要。逐条使用FIX05身份核验/排他交换；只归还仍为空或仍等于本轮封存后像的目标，发现外来值/备份漂移即保留冲突，不覆盖、不递归清理。部分准备失败按已完成账本恢复。其余Library自然结果不回滚，也不宣称恢复了整体缓存链。BC/TMP保留限定清单，不清旧缓存；P旧TestArtifacts、共享Config/Tools/Generated/ArtSource及旧证据冻结。

## 5. 收件条件与未决项

I/T真实OS exit0、全程无编译/UPM/加载/domain错误；12程序集来源完整；53图与保护不变；新XML精确89全名Counter各1且全Passed、0额外/缺失/失败/跳过/不确定；AS、资源/时间、退出及恢复齐全才可交主测试限定接收。仅日志末行、DLL存在或旧XML均不够。原I01仍FAILED/exit=null，晚取DLL不是缓存链；原RAW/PREF及FIX05失败原样保留。未知退出/缺XML/缺链为INCOMPLETE，超时、测试或保护/恢复失败为FAILED，前置缺口为NOT_RUN_BLOCKED；不自动重试。

剩余门：差量runner实施/封签、对应head审查、中央fresh激活及C一次I/T，均未发生。dots缺当前环境/许可/传递/symlink就绪，走限定本机例外。PR19 head `dc1bba0537779983647f5e261f8e1c33a1210a49`源码门已由中央限定ACCEPT，见`resource-shared-integration-source-verdict.json`；整PR树不等于共享1036。正式LOC工具阻塞不纳入本包；Host、真实下载、Windows、Android/设备和Demo仍为独立门。
