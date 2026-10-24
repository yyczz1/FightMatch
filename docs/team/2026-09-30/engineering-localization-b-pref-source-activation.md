# LOC-B-PREF：source-first 激活增量

2026-10-01 · `PREPARED_NOT_AUTHORIZED`；仅新增本文。SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，turn `01a0f717-7680-7063-b476-510efbd94fd8`；用户AGENTS §6＋中央本轮委托，唯一回中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。
复用[原包](engineering-localization-luban-impl-b-pref.md) SHA256 `399c0a246a8b1ffa8d1b201d8e309433668f0e5ecc4e1b6333bed608a0237d95`（744行／40537B）的六路径、internal interface、schema／保存三态、PREF-01～21及规模预算；只以下文替换旧派发／R／验证前置，不改原包或module设计。
其§2.1的LOC设计／原B／拆分／planning／uGUI实施稿已逐项重算，均等于原冻结SHA；[RES-CD](engineering-resource-runtime-bridge-cd-001.md) SHA256 `ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378` 的I0/I1/I2拆门沿用。
中央提供FIX22 head=`3541930877837e14b58020b910976b8b67b56e42`／tree=`37326f759fc065f268c23f66a674be1a61f12268`，[GitHub审查](https://github.com/yyczz1/FightMatch/pull/1#issuecomment-5929800103)Completed/no-major；本轮未联网复核，12实跑仍待C。该事实不是整体uGUI／Host原生／设备ACCEPT。

## 1. 实读I0与目标缺席

观察时间 `2026-10-01T10:54:26Z`；下列九叶已读取全部内容并逐项统计，身份属于本地实际字节，中央激活时须机械绑定对应head blob／输入manifest，不能把本地HEAD当PR head。

| 精确只读路径 | bytes | SHA256 |
| --- | ---: | --- |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocaleId.cs` | 559 | `f5338ff0fdde20a362281380a7d84183f552ad23a7964edb51d042fca48174a5` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizationService.cs` | 2786 | `ca8b2c5f9b8afd864410b35f7ab9998c04a7b5ea9efdf3a51e8082b6dd54a408` |
| `Assets/Scripts/FightMatch/Presentation/Localization/LocalizedTextSource.cs` | 1527 | `0c830db5aa297a08e236e7a8edb7e42d62f3c779b1293b362733b450283901a6` |
| `Assets/Scripts/FightMatch/Presentation/PresentationAssemblyInfo.cs` | 258 | `047029b17c9e84e9ec8666938680901897f4dd4457516d646b5d2c64be8145ad` |
| `Assets/Scripts/FightMatch/Presentation/FightMatch.Presentation.asmdef` | 592 | `b7c59f7de8581990dacc19ab111ae4be4805958333861ac4ec15b774db242de8` |
| `Assets/Scripts/FightMatch/Host/HostAssemblyInfo.cs` | 176 | `1dc94417f744f9a09f01c16af0f5ed720347d96d883f4304e5f3eb613e968fd5` |
| `Assets/Scripts/FightMatch/Host/FightMatch.Host.asmdef` | 346 | `85acabf7d4ec5e2b9970f9299ab2687cef131a4fa8213aa4f0aea9f70b4c0c7e` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatch.Host.Tests.asmdef` | 479 | `50da0a6cb45f6ba0b7a4b843bdc8d865595df7d15bdc1976d01dcd52f2437e03` |
| `Assets/Tests/EditMode/FightMatch/LocalePolicyTests.cs` | 3575 | `eb0af193bb66b0b7dc59be45066a7b56dcb4c1524ad31139b4b44bded387d9af` |

逐项`不存在且非link`：`Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs`；`Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta`；`Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs`；`Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta`；`Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs`；`Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`。仍是原六产品／测试路径，禁止第七叶或修改既有文件。
现有friend已允许Host／Host.Tests访问Presentation及Host internal；LocalePolicy、SetLocale与事件可承载原PREF合同。I0只须这组源码／接口及适用审查证据的限定接收；完整原生复开、222重跑、LOC-A／Luban材料、R2、YooAsset不是模块前置。原包“uGUI完整ACCEPT后才能写”由此增量替换，I2仍待后续。

## 2. 单向、分阶段激活

1. **S0签源码阶段**：中央核FIX22实际结束／owned清空、C无冲突写域，复核九输入／六缺席，激活C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`（Astra/xhigh）。只绑定当前authority、I0和本稿身份；C实际turn在收到任务后写receipt，未来head／meta GUID／真实leaf不作S0前置。当前仍不派C。
2. **S1固定源码**：C只建三个原定C#，产PREF矩阵→测试声明映射与静态scope／diagnostic自检，冻结源码SHA；不执行测试、不手造meta、不从21个矩阵ID猜leaf数。参数源必须确定、无IO／副作用；原有源码／资源／证据只读。工具源码如获下节补签，也在此时冻结。
3. **S2最小导入／发现**：中央另激活一次Unity2022.3.18f1导入＋编译＋discovery，≤5分钟，测试执行数必须0；自然生成且仅生成上述3个meta，核GUID唯一与零其它asset漂移。用已固定源码实际列出全部PREF NUnit leaf／参数／run-state，冻结精确filter／Counter和PREF-01～21覆盖映射；没有发现回调／compile error／超时均停止，不以空列表通过。此必要发现阶段不等未来PR head，否则形成循环。
4. **S3审查绑定**：S2后六叶＋工具源码固定，中央按另有Git权限发布对应新head，GitHub PR Code Review接回该head结论；旧R token作废、不唤醒R。完整当前执行闭包（含Unity会导入的其它WIP及只读工具依赖）绑定真实tree／逐叶SHA；发现证据绑定产生它的三C#、自然meta、UTF／Editor和C turn。head与工具自身SHA从外置activation绑定，不让源码包含自己的未来hash。
5. **S4一次定向验证**：对应head无未解决阻塞、闭包／filter／实际leaf数已定后再激活；只跑新增PREF全部leaf，真实LocalizationService用于18/19；不跑222／图形UI／原生场景。既有LocalePolicy／uGUI证据按未变SHA复用。XML必须逐leaf Counter相等、全通过且无skip/inconclusive，exit0、owned清空、保护字节不变；一次≤10分钟，首败／超时结束，修复另签，不自动用完原包备用run额度。
6. **环境与退出**：自动化优先实际就绪的dots，固定同源码、自然meta、Unity／UTF版本和运行环境；本模块只做文件／locale事件，静态确认无TMP／Canvas渲染后可headless，不继承旧包因混入uGUI回归而设的非Null图形门。dots未就绪不冒称云可用，转C本机须中央明确例外；C唯一运行本机Unity。先S2进程退出再审查／S4，不并发导入或占用FIX22。

## 3. 最小验证工具增量（待中央补签，不在本轮创建）

没有可直接套用的精确入口：原包§10仅argv模板；`Tools/Invoke-FM025P2Validation.ps1`拒绝未列Stage；FIX22 runner／`FightMatchTestProgressCallback`固定其专用根。全部保持只读，不换路径冒用旧授权。
只拟新增 `E/validation-tools/runner.py`，其中`E=TestArtifacts/FightMatch/LOC-IMPL-B-PREF/source-activation-001`（本轮观察不存在）；限≤250行，仅两个mode `discover`／`focused`，复用既有进程安全判据，不引入通用runner框架、安装或依赖。职责限身份／精确argv／create-new输出／PID监护／退出与XML核对；旧FIX22 progress参数不传，原callback因未启用而不写旧根。
发现shim限放原白名单`LocalePreferenceStoreTests.cs`内的internal/private Editor辅助代码，独立参数`-fightMatchLocalePreferenceDiscover`仅启用一次；调用本机UTF1.1.33已有`TestRunnerApi.RetrieveTestList(TestMode.EditMode, callback)`，遍历`ITestAdaptor.Children/IsSuite/FullName/RunState`，只导出该fixture，不调用Execute／测试方法。Host.Tests沿既有TestAssemblies引用，不加asmdef／friend／public产品surface；类型不可解析即BLOCKED，不另建工具程序集。
discover argv采用batchmode／nographics、无`-runTests`／无`-quit`（等待异步发现，成功写完receipt后主动Exit）；focused采用原CLI `-runTests -testPlatform EditMode -testFilter <实际leaf字面转义集合> -testResults <新路径>`、不带`-quit`。工具版本、executable绝对路径／SHA、完整argument array须激活时冻结，不在本稿猜值。
E只拟写原包§11证据闭集及上述runner、`source-receipt.json`、`discovery/argv.json`、`discovery/exit.txt`、`discovery/unity.log`、`discovery/leaves.json`、`discovery/receipt.json`、`activation-discovery.json`、`activation-focused.json`、`process-events.jsonl`、`review-binding.json`；不用的原run槽标NOT_CREATED。source固定→自然meta/发现→head审查→一次聚焦的身份逐段链接；bounded日志／owned退出证明必须有，超时不算pass，不杀非owned进程。
中央补签前本稿仅`READY_FOR_SOURCE_AUTHORIZATION`建议；S2／S4分别需环境、精确argv／预算和进程门，不能把工具缺口扩成产品白名单。本轮只读核对和写本文，未Unity／编译／测试／联网／Git／派工；最终模块接收仍需准确head审查＋定向证据，绝非Host已接线。
