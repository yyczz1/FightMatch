# RES-CD-JOINT-MAC-001 — 固定D FIX02＋C契约，一次I→23T

2026-10-03。中央已明确批准本地UTF可行时合并两包验证，保留各自源版本/自然meta/结果与代码审查门。唯一C 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh；issuer/唯一收件主程01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff8c-1b41-7262-a8be-362ff4870414，沿用户AGENTS §6。此合同替代尚未开始的旧D单独12T和C单独10T安排，不重写原合同或证据；激活须先接真实旧T NOT_RUN/owned闭合回执及D FIX02已封存八叶。若旧T已真实启动，则先原合同安全收尾、旧结果只归旧候选，仍不再补旧12。

两个固定源码输入：D取[FIX02](engineering-res-d-codec-fix02.md)17行4908B/SHA73ff39f7f1505298af47ef1ba9dc86e272b6a697c702b2ad2a554aafa6d5ffaf及激活时封存receipt/两源hash/13 fullnames；C取C-CONTRACT/S receipt14107B/SHA bfca5fe3550f4f25198595de9bdc3368acbdac3122b704820ff365b8b6e5e2f2及原八叶180087B/10 fullnames，生产16451B/SHA18d5695eef979e549581fc737b0fc25753de2cd47c8ebca06be0a4e428b6663c、测试26749B/SHAffd0e2df85720ca23a812a2fde3808225eb17973fb4b252f5a632ef4947b2b78。D13仅一新增回归，原12及其中D09 bundle共享正例保持；C源码逐字节保持。四产品路径只为现有D ReleaseSet.cs/ReleaseSetTests.cs两后像及新C RuntimeContracts.cs/RuntimeContractsTests.cs两源，完整路径见各receipt.source_inventory；无其它源/断言/asmdef写权。

固定本地UTF依据：P01/projection/Library/PackageCache/com.unity.test-framework@1.1.33，package.json1587B/SHA63cb29f4cc8dfc03ce8b3e9a68196f134310e8412cd80110307035e82100cfc4。UnityEditor.TestRunner/CommandLineParser/CommandLineOption.cs1159B/SHA3d6242d986b07997ee4f469c132be1edab31668832d44a4aab0646ac5f540280的35–42行分号Split；CommandLineTest/SettingsBuilder.cs9051B/SHA1b73ea92d67da0652b58c1924929fa72d5e5aba3dd3a69bb6e832a4cbaefd6e9的44/67行送groupNames；Api/Filter.cs2979B/SHA85a133adc6ef138da0205ec3e9677d46ad13f765e5910fddc8764652f393d55e的46–55转换；UnityEngine.TestRunner/TestRunner/RuntimeTestRunnerFilter.cs3483B/SHA7f5fe39c85fe5e5bdd2bda4122aaf3b91c3da2237d1330c470140d9fe9f5f2e6的27/63–78构造OrFilter；UnityEditor.TestRunner/TestRunner/EditModeRunner.cs15948B/SHA0f881d1ab13ec2a942dc80d80f50a001110daad97bcd29a16716b4afd4e7f9fc的447使用它；UnityEngine.TestRunner/NUnitExtensions/Filters/FullNameFilter.cs549B/SHA003ce32628f1650f8df941636337c9dab4e49b060e6d1490d8a0d2b93ccc500c匹配去DLL路径的full name。主程b483d3机械筛选23候选且排除B38，未执行C#/Unity/discovery；实际精确性必须XML证明。

T的-testFilter单个argv值固定为下面一行，两个正则以分号OR，反斜线为实字符；使用argv数组传入，不交shell展开，不把分号拆为命令：

```text
^FightMatch\.AssetAccess\.Tests\.FightMatchResourceReleaseSetTests(\.|$);^FightMatch\.AssetAccess\.Tests\.FightMatchResourceRuntimeContractsTests(\.|$)
```

环境/安全继承[MAC-D002](engineering-res-d-codec-mac-002.md)SHA518b379d610f4046b17d55278d1fef61beeaaf93c4cc154b717499953551a0b1及[续测合同](engineering-res-d-codec-t-continuation-001.md)SHA2a016cb2439da16db3062024e0549208b2b1e10f815f24b9779c07ac4a32add8：固定2022.3.18f1/x86_64 Editor、53包和同P01投影/cache，不换工具或重装。原adb63318可缺席；仍在必须原UID/start/exe/cwd/argv与投影FD0，任何新未知adb只读必要元数据并停止上报，不自行归属/放行或发送ADB命令/信号。自有非ADB finally与外来consumer门解耦，精确PID/start/exe/argv/cwd/历史chain一致才自然60→各最多一次TERM→≤30s，无SIGKILL/按名kill/外来信号；外来异常禁止新Unity及成功结论，不阻断已证自有清理。正常工具拒绝保存上报，不绕过。

前像为真实M02终态1023源/meta/529GUID，D仍FIX01两源、全部14相关meta已稳定；按旧T实际封存回执确认无额外变化。仅apply_patch应用D FIX02两源并新增C两源，之后1025叶；仅Unity可为C两路径自然生成.cs.meta，I中允许stub到MonoImporter但已见GUID不可改，I成功要求完整两新meta，最终1027叶/531唯一GUID。D原两243B meta及B旧12从开始到结束完全不变，C新两meta在T前/期间/结束严格等于I后像；全部其它1019叶保持，无新目录meta/资源/asmdef。after保存C两meta原text/path/bytes/SHA/GUID/importer，D两meta只记录保持原字节。

新证据根仅TestArtifacts/FightMatch/RES-CD-JOINT-001/M01，起始ABSENT，同17叶：run.json、runner.py、preflight.json、I/editor.log、I/stdout.log、I/stderr.log、I/result.json、T/editor.log、T/stdout.log、T/stderr.log、T/results.xml、T/result.json、process-events.jsonl、process-after.json、after.json、assemblies.json、receipt.json。失败只留实际子集。run分列D/C owner/receipt/source/hash/fullnames/审查head映射；已发布D旧bf309871不冒充FIX02新head，未发布新head可null且后续另映射，不回写旧证据。旧M02原十一叶和T-continuation实际封存子树分别精确冻结，CLOSE01/02、各源S/旧B/D及共享保护保持。

runner以旧续测已封存控制脚本或M02固定runner最小适配，≤400非空行（双包分项收件的明确额度）；不得执行旧runner主入口。新tmp一次/private/tmp/fm-res-cd.XXXXXXXX，同回合直接准备≤120→I仅一次≤360→成功才T仅一次≤180，含finally总≤900s。完整历史/包cache只准备与最终各一次；阶段前小范围源/meta/asmdef/run/runner/Editor/consumer/lock/资源门。最多2次纯机械语法/guard/selector校验各≤10s，OS/Unity0并记新run/preflight；不重跑产品verifier、诊断或旧候选测试。

I argv沿M02规范仅换本根日志；T沿其EditMode规范仅换本根log/XML和上述单argv双类filter，无-quit。I需真实Csc AssetAccess及AssetAccess.Tests且两DLL区别前像；其余五DLL记录实际，不强求变化。七asmdef/七DLL/源固定关联、日志编译/package/domain错误门保持。Unity可能自然重编依赖不等于获准追加I；总Unity启动恰最多2。XML必须等于D13与C10精确fullnames多重集并各一次；按类各列total/passed/failed/skipped/inconclusive、case列表及各自verdict，另列全局total23/23Passed。若一类失败，另一类真实结果仍原样保存，但源/消费者/公共编译保护异常不允许宣称任一完整原生门通过。无B38/单类追加/全量/额外discovery/自动重试。

资源cache≤2GiB、生成≤4GiB、总≤7GiB、证据≤32MiB、单log≤8MiB、tmp≤16MiB/512叶、free≥4GiB；0安装/Client/Resolve/下载/Git/共享采纳，不复制工程/Library、不删缓存/锁。任何输入漂移、时间/空间超限或新失败即封存停，不改产品/断言、不追加第二轮；获准自有finally始终执行。D/C审查均走中央/QA的GitHub，不开本地review。

完成依据AGENTS §6一次send_message仅回主程/local/Astra-xhigh，交实际turn/I-T次数PID/exit/耗时/17叶、同一XML中D13与C10分别结果、两C新meta和旧14/1027/531保持、七DLL/owned/ADB门。中央分别发布/更新D与C PR并接独立代码门，不把组合当单一PR-head构建；若测试结果到后只补正文/另关联，不能因发布追加跑。真实runtime/permit消费/网络停止/资源admission/LOC/Host/Android/设备/共享集成NOT RUN，FIX04/M16旧拒绝不变。
