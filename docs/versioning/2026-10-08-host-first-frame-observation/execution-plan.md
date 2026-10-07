# HOST-NEXT-001：真实场景启动／首帧布局最小观察

2026-10-08 · PREPARED_UNRUN。中央本轮明确续接首Demo；本文只准备，不激活Unity。唯一交付负责人主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，本次actual `01a1172b-b406-7160-8c0e-e1b8b35cb0b6`；唯一完成接收者中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。中央收包后交唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，核停写并绑定fresh actual；本包不派其它角色。

## 1. 本次只补四项

1. **真实场景启动**：打开固定N11R1后像资产，进入一次Editor Play，由真实Host自然Start，不用InstantiatePrefab夹具替代。
2. **帧推进**：证明Play中Time.frameCount至少推进2，捕获自然Canvas回调；“进入Play”本身不是首帧证据。
3. **Canvas几何**：区分首次Host.Bind内同步Apply/ValidityChanged、随后自然布局帧、取消后解绑三个时点；记录原Canvas有效尺寸/scale、SafeArea版本及布局结果。
4. **null LOC正确阻断**：观察LocalizationNotReady、Session仍null、诊断显示、回调撤销、隔离lease释放及无玩家档写入。EventSystem只核实际组件/当前module/真实RaycastAll命中，不声称完成玩家点击。

正式LOC缺失时可尝试并分别记录这四项，不保证首次有效布局能在启动取消前被观察到；正常按钮业务、棋盘手势提交/取消恢复、非空会话、文本翻译、Android设备/像素验收仍待办。本包不得注入成功文本源、草稿CSV、真实Session替身，亦不手动Bind/Apply或使IsReady为真。结果最多为这四项限定观察；不自动关闭PR4 P1。

## 2. 固定版本及实际闭包

R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；下列路径相对R。
H=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/T01/`；N=`TestArtifacts/FightMatch/LAYOUT-S1-001/FIX04/N11R1/`。
M=`TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/`；复用P=M+`projection/`、K=M+`package-cache/`。
新E=`TestArtifacts/FightMatch/HOST-NEXT-001/M01/`，本轮实核父根缺席。

| 固定文件 | bytes / SHA256 |
| --- | --- |
| `docs/team/2026-09-30/host-continuation-fix01-integration-receipt.json` | 3420 / `ceae4f194dcb3aaf68a8a18739d68a13ca947dda237a0bca7e7b4989a5fe69f3` |
| H+`inputs.json` | 420831 / `7c960aab0db21db94c2af3d30e46b51fcf51e452768e06828655057f01d97614` |
| H+`publication-inputs.json` | 17867 / `18c8cfd1f9e571e7922691bb7b935497d0f8a800e3ffe4a75211a9817f30d050` |
| H+`runner.py` | 35053 / `c6be8a53b519b956223c5b34cd21e572fb814d20651828bfc7a152e937ecadfc` |
| N+`P/scene-final.unity` | 27798 / `69a927c8f43eb1612f0b3069accb726833c4849a76fe8a5bef6041157cab7e4b` |
| N+`P/scene.meta` | 155 / `a9c4675d97e3764f91fa62e47d0498a79a101e1456019e6e298d8c8407d5241c` |
| N+`I/activation.json` | 130129 / `ed46f7fd93aa97b30a0e0de2bc9beca6a2c0274b4a08fe4c48c0f005745efd17` |
| N+`S/FightMatchCanvasPersistenceProbe.cs` | 18436 / `18307351e47024cce5fb80943818764968877d1a7096283f5461fc813041d9bd` |
| `docs/team/2026-09-30/engineering-layout-n11r1-serialized-delta.md` | 8186 / `5b0db3a217d1e46b89f1d84e4954ffd98aff931060e68257b6ec0cd6c5467f05` |

Host固定PR16 head `030898e9a317d10b610058d867529be8b0770c7c`；十项10/10和同head GitHub门已限定ACCEPT，直接引用，不重测/重审。主程本轮只做下一输入身份检查，不重做该集成判定。
P实核已恢复1008叶31770978B。从H/candidate按publication-inputs.files.after取32个固定候选，逐叶相等即不重写，仅同步H.inputs.overwritten八叶/newPaths四叶。得到原已接受1012叶32086333B；全部其它源、Packages/ProjectSettings、51包及旧证据冻结，不混共享WIP。
再把N两固定文件原字节复制至P+`Assets/Scenes/FightMatchCanvasPersistenceProbe.unity`及其`.meta`，GUID=`36446959839f049a29eb2a66fd39cbc5`；两路径当前缺席。原`Assets/Scenes/FightMatchDemo.unity`及其meta不改。保留N11 marker/106条序列化包络；不删五条驱动记录、不剥marker、不重排YAML。
加入探针前应恰1014叶32114286B；`sorted(path NUL gitBlob NUL decimalBytes LF)` SHA256=`e4caecf42b39e97d6acc517ae20c0dc1a6d97ffab0bd72b0a675b1cfbddb9050`，已静态重建。C在此上加入下节唯一临时cs及自然meta后封存最终清单/候选SHA，不预造未来hash。

## 3. C的准备与写入白名单

本轮主程只新增本文。以下写权只在中央激活C后生效，所有旧文件作只读来源：

- E根文件：`activation.json,inputs.json,before.json,preparation.json,runner.py,process-events.jsonl,process-after.json,compile.json,after.json,restore.json,receipt.json`。
- E+`S/FightMatchHostFirstFrameProbe.cs`（唯一新探针源）；E+`P/{editor.log,launcher.log,result.json,observations.json}`。
- E+`restore/source/<repo-path>`：H.inputs.overwritten精确八前像；E+`archive/source/<repo-path>`：实际八覆盖后像、四Host新增输入、两场景文件、探针cs及meta，共16叶。已封S及N不复制大清单/旧日志。
- P输入仅前节十二同步叶、两场景叶及`Assets/Scripts/FightMatch/Host/Editor/FightMatchHostFirstFrameProbe.cs`和自然`.meta`（后两当前缺席）。不改asmdef/API/已有工具源码，Host.Editor已引用Host/UI/TMP；Presentation内部状态通过反射读取/事件订阅，不增加程序集引用。
- 新隔离D=P+`TestArtifacts/FightMatch/UGUI-01/native-scene-reopen-001/<新32小写hex>/`：只准`activation.json`、`host-save/owner.lock`（CreateNew空文件）和空目录`host-save/persistent/`；退出归档到E+`archive/isolation/`。fresh ID及每个canonical绝对路径在activation固化。
- Unity自然生成范围仍为P的Library/Temp/Logs/UserSettings/obj/根.csproj/.sln；K已有payload冻结。仅容许已定默认SceneTemplateSettings.json（3534B/SHA `5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`）自然出现，结束移E+`archive/SceneTemplateSettings.json`。

唯一新meta槽是probe.cs.meta：只接受自然GUID及默认MonoImporter，允许首导入短头→完整默认字段，逐次记录并保持GUID，结束冻结实际完整字节；不要复发59B→243B精确前像门误报。Scene直接复用N既有meta；全投影GUID唯一，任何其它meta漂移失败。
在P无进程/锁时保存八前像、按清单同步，K不复制/下载/Resolve、不清Library。probe及runner由apply_patch创建；机器清单/归档机械生成。准备机械≤120s，静态语法/范围检查最多2轮累计30s，首过即止；不运行旧runner/源码检查器探路。P原51个host-io目录全部冻结，本包不创建HostRig。

## 4. 最小探针流程

复用N探针的身份、真实Scene/Prefab对象映射、geometry与CreateNew报告代码，删除其Save/marker创建/复开流程；复用已存在`Assets/Tests/EditMode/FightMatch/UguiResponsiveLayoutTests.cs`中OverlayResolution的GameView选择/恢复方式，仅摘取该机制，不复制其Prefab夹具/成功LOC/Force调用。该参考81160B/SHA `bbaa34f34a5c894125ec2dcc63c3d2d507403282a7bf6d4505d62ed050647b89`，不是P编译输入。
新probe≤400非空行、runner≤450非空行；不是通用测试框架。runner复用H的精确进程/资源/保护/归档骨架，仅改一次图形probe阶段、场景/临时源/隔离路径及新报告判定，不重复旧T十项。probe全程只观测产品状态；事件增减订阅、拥有的Editor窗口设置与Play控制须计数。

1. `Execute`校验activation/source/完整候选/cwd/命令行及原N11包络；OpenScene前证场景和meta仍固定字节。仅此隔离P且无用户场景脏状态；记录原Scene setup、GameView选择和自有窗口状态。已有`runInBackground=1`、EnterPlayModeOptionsEnabled=0保持，不改配置/禁域重载。
2. GameView暂时内存选540×960竖屏：借原OverlayResolution添加唯一本轮标识项、记录index，结束恢复选择并删该项；禁SaveToHDD/EditorPrefs写。只允许窗口Repaint及有界QueuePlayerLoopUpdate调度，不调用Canvas.ForceUpdateCanvases/ForceRebuildLayoutImmediate、SafeArea.Apply或RectTransform写入。尺寸就绪≤20个Editor update/5秒；实际Screen及safeArea始终记录，未达不伪造。
3. Open固定候选Scene一次，核唯一Host/Canvas/EventSystem、原Prefab GUID和绑定、N marker纯Transform、稳定ID及完整序列化记录。只用真实Scene；不InstantiatePrefab、不加测试Canvas/module。注册观察后EnterPlaymode一次。
4. 域重载用本任务nonce的SessionState保存armed/phase、原Scene setup/GameView选择和有界报告，`InitializeOnLoad`恢复观察，`InitializeOnEnterPlayMode`幂等注册SceneManager.sceneLoaded/Canvas回调；不改生产对象。必须证实观察器在候选sceneLoaded（OnEnable之后、Start之前）就位；否则FIRST_BIND_UNOBSERVED，不能从晚期样本推定首次Apply。ExitPlay前保存报告，退出域重载后恢复收尾、最终仅CreateNew一次observations.json。
5. sceneLoaded时从真实View取得ResponsiveLayout，订阅现有ValidityChanged（反射add/remove访问器只挂观察者），并记录SafeArea.Version、Host/Canvas状态；不调用Bind/Start/Apply。Host自然Start会使用命令行隔离根。同步ValidityChanged时记录门值与几何，随后观察自然preWillRender/willRender及frameCount，保留时序/次数，最多20份样本。
6. **时序事实**：此Host在null LOC下同步Bind后即CancelStartup→Unbind→ShowFailure，Responsive.Unbind又ReleaseGate。只有同步ValidityChanged能证明初次Apply；取消后的`CanvasGroup.interactable=true`不能当作布局或游戏输入已解锁。若首次门false/缺失，保留反证；不手动重绑等绿。
7. 获首个自然Canvas样本和至少两帧推进后，记录EventSystem.current、当前真实FightMatchStandaloneInputModule、GraphicRaycaster及diagnostic可见性。在diagnostic和languageButton实际Rect中心各调用一次RaycastAll，保留有序命中与所属根，判断遮挡是否相符。只测试真实命中链，**不直接调用Button.onClick/ExecuteEvents/业务方法或注入输入替身**；未产生物理输入不是“玩家点击通过”。
8. 从sceneLoaded起最多10帧或10秒（先到），停止采样并ExitPlaymode一次。正常取消已释放lease时，可用相同owner.lock尝试一次独占打开并立即释放，证明没有遗留锁；不触及真实用户目录。退回EditMode≤15秒后卸订阅、清本任务SessionState，恢复GameView/scene setup（仅本隔离实例），不保存场景；关闭自有窗口。异常也走相同收尾，不再次进入Play。

反射只读属性/字段与事件挂接；不得写Host私有状态/篡改生命周期顺序。完整GetPropertyModifications仅用于观测，不把自然内存记录增减当磁盘漂移；磁盘Scene/Prefab必须始终字节不变。

待实测前提：新probe尚未编译；此固定Editor的batch图形模式能否在预算内产生目标GameView尺寸/自然Canvas回调，以及跨域重载观察器能否赶在Host.Start前订阅，均不是已有通过事实。按本次唯一运行记录失败或UNOBSERVED；不得追加试跑、改生产时序或改用成功LOC来补齐。

## 5. 隔离、入口与预算

D.activation.hostSaveIsolation严格沿已接受Host schemaVersion1：activationId、canonicalProjectRoot=P、persistentRoot=D/host-save/persistent、candidateSha256、ownerThread/ownerTurn；两命令行标记与记录一致。执行候选摘要精确定义为SHA256(UTF8(上述1014叶canonicalSHA＋LF＋probe源SHA＋LF))；命令行/两activation始终绑定此摘要，不含自引用或尚未产生的自然meta。导入后另外封存完整清单SHA/自然meta于compile/observations，不修改activation或混称同一摘要。
真实Host的默认系统路径只允许既有隔离守卫执行祖先/链接/相离元数据检查，禁止读写/枚举真实存档内容；不通过创建默认目录解决失败。实际EffectivePersistentDataPath必须是D/host-save/persistent，CanonicalProductRoot为其FightMatch子路径；null LOC后persistent仍为空，locator/profiles/偏好文件均未创建。

固定Intel Unity2022.3.18f1路径=`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，86759760B/SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。C封签后以正常命令审批运行一次`python3 -B E/runner.py <activation-sha> <fresh-C-turn>`；旧许可/旧prefix不替代本命令审批，拒绝保留原因、不改道。runner展开以下完整非shell argv，cwd=P：

```text
[Editor,-batchmode,-force-metal,-buildTarget,StandaloneOSX,-projectPath,P,
 -executeMethod,FightMatch.Host.Editor.FightMatchHostFirstFrameProbe.Execute,
 -fmProbeInput,E/activation.json,-fmProbeNonce,<本次nonce>,
 -fmHostSaveIsolationId,<D的ID>,-fmHostSaveIsolationCandidate,<固定执行候选摘要>,
 -logFile,E/P/editor.log]
```

不带-nographics/-quit/-runTests。只一次Unity≤240秒（含必要新probe编译/导入），Play一次且受上节帧/秒限制；不另开I/T/discovery/重试。新Host.Editor必须有本轮Csc/源/DLL绑定，其余程序集真实记录编译或复用，不为十项旧绿测重编/重跑。
准备至归档总机械≤600秒，Unity结束自然等待≤60秒→fresh PID/start/exe/argv/父链核对→owned每PID最多一次TERM→≤30秒确认；安全收尾不省略。无SIGKILL/按名称kill/外来信号。沿H runner的旧ADB精确FD1/2日志例外和CLR FIFO仅lstat规则，旧ADB/旧TMP不动；其它消费者或无法证明归属即停。
新TMP一次`/private/tmp/fm-hn01.XXXXXXXX`，canonical≤40B、0700/本人UID/无链接，≤16MiB/512叶；K≤1GiB/51包、P生成物≤4GiB、外置空闲≥2GiB、E≤32MiB、单log≤8MiB、observations≤1MiB；每2秒监护，超时/预算/网络/未知写入走收尾。新CLI/Pipeline/下载/restore/付费/merge/设备均0，现有固定Editor合同优先于Unity技能的通用安装流程。

## 6. 分项判定、恢复与审查条件

| 项 | 可观察完成条件 | 不得据此声称 |
| --- | --- | --- |
| A 场景启动 | 正确Scene/GUID/Prefab/Host实例，真实Start的localeLoaded/root/失败状态可证，磁盘输入未改 | 默认真实档启动、正式内容就绪 |
| B 帧推进 | Play未pause、frameCount差≥2、自然Canvas回调有对应帧/时间 | GPU像素正确、设备首帧；本最小包不截屏 |
| C 几何 | 首次Apply事件和首个自然帧分别记录；有限正scale/rect/pixelRect，540×960支持尺寸，SafeArea自然更新；原零序列化不等于有效零值 | 将晚期释放Gate当首次Apply通过；布局恢复/取消手势已验证 |
| D 正确阻断 | LocalizationNotReady可见、Session=null、绑定/选择回调已撤销、lease释放、隔离persistent空；EventSystem/module/命中结果如实报告 | 正常按钮/棋盘输入闭环、翻译/正式LOC接入 |

四项各列PASS/FAIL/UNOBSERVED；C仅在当时已满足Responsive.Apply的有效尺寸/scale条件却Gate(false)时判失败；有效Apply缺失或null LOC早退导致未见有效样本则UNOBSERVED，不能用初始无效尺寸的正常Gate(false)宣称P1复现。两者均不追绿，保留A/B/D独立证据。正常交互明确LOC_BLOCKED；异常首错/清理错分列，C只报OBSERVATION_COMPLETE或PARTIAL/FAILED，不自签产品ACCEPT。
owned闭合后先封存16叶实际后像及新meta，八前像恢复；四Host新增与四探针/场景新增输入移入指定archive，归档D仅本次树；P恢复原1008叶，旧51 host-io不动。漂移或归档缺失就停止对应恢复并报差异；不递归清工程/旧缓存，不回滚Library。本次移走文件可从archive恢复；显式Scene Save/只读复开/产品导出均0，Play引擎内部reload单记，不隐瞒为“未加载”。
本轮不发布、不复审PR16。将来若用观察证据申请关闭PR4 P1，中央须固定“PR16产品＋原字节N11场景/meta＋实际probe/runner及完整结果”对应的候选head与发布清单，交GitHub独立判断精确序列化包络及首帧风险，保留原P1/UNKNOWN；不能拿新组合替旧head自动resolve。任何产品修正另签最小包，本包不直接修复。

技术依据：[Unity 2022.3 sceneLoaded示例](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html)给出OnEnable→sceneLoaded→Start顺序；[EnterPlaymode](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/EditorApplication.EnterPlaymode.html)用于受控生命周期。本包不用[batch中不会执行的WaitForEndOfFrame](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/WaitForEndOfFrame.html)。writing-for-agents明确分项完成条件，uGUI/Unity技能要求实际帧推进、几何/输入分层，均不扩大产品写权。
