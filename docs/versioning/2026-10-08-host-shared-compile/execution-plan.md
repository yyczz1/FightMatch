# HOST-INTEGRATE-001/I01：共享组合一次编译收口

2026-10-08 · PREPARED_UNRUN。主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，actual `01a1179e-21bb-7c11-8f81-04fb4e9311e3`；本轮只新增本文。中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local` 为唯一接收者及签发者；实施仅C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh、fresh actual，由中央另行签发串行启动。

## 1. 必要性与最省路径

编译有必要：S01是已审源码采用，较新的共享AndroidBuild、布局测试与PR16验证投影不同，没有该**完整共享组合**的编译证明。不重复PR16十项或PR17观察；本包只回答Mac Editor组合能否编译，不能证明Android条件分支／APK／玩法或正式LOC。
选复用`HOST-SETTINGS-FIRST-CREATE-001/M03`的既有投影P和缓存K：Library现约565MiB，K实核9984叶731367785B，与固定缓存清单逐叶相等。只需16覆盖＋4新增＋12暂移；不建新完整工程、不复制／清除Library、不替换共享产品、不安装CLI／Pipeline。Unity技能要求区分编译与运行证明；其CLI timeout会追加SIGKILL，不适用本包既定TERM边界，沿用固定Editor直接受控入口。

R=`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；相对R定义H=`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/T01/`，O=`TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M03/`，P=O+`projection/`，K=O+`package-cache/`，Q=`TestArtifacts/FightMatch/HOST-NEXT-001/M04-source/`，新E=`TestArtifacts/FightMatch/HOST-INTEGRATE-001/I01/`（当前缺席）。

## 2. 固定实际输入

所有摘要格式为`SHA256(sorted(path NUL gitBlob NUL decimalBytes LF))`；gitBlob直接按原字节计算，不调用Git。范围为Assets／Packages／ProjectSettings普通文件全集；不跟随链接，不能悄悄排除不喜欢的WIP。
准备时仅S01五目标允许其执行单前像／后像；本轮先在内存归一为固定后像计算，不修改文件。**执行时五目标必须全为后像**，共享全集应999叶32228974B／`06edb6d3d9037a56a4e4bca3e4ca0b46cdd0a2c0aac8ce0dd04e54fa887cffdf`；任一其它漂移即停，不自动改白名单。C生成逐叶bytes/SHA/blob清单封签，不能只查五目标。
P原全集实核等于H+`before.json.restoreBaseline`：1008叶31770978B／`3cab92e65cf0f48bde8b47f9c701ee6c0e568b1bc44e991e32bc48c3878ccb8a`。共享缺失的唯一meta是`Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`；P已有自然完整243B／`97eb4963b6d27d4669a021853e7363a3bbe386269f9293ff6d4608c772be39c3`，GUID `d85e93281a75342ac8d13378ff40cdba`，共享Assets无碰撞。仅在P保留此叶，不回写R、不重新生成。
故实际编译全集=共享999叶＋上述明确meta例外，恰1000叶32229217B／`fefb2117f7e3991ab5f1a482216f3cff902368ad330295383e9705a534a18715`。除自然默认settings外没有新增meta槽；已有所有meta原字节冻结，GUID唯一。不能把P中的额外资源／旧测试留在编译集合冒充共享工程。

| 固定参考 | bytes / SHA256 |
| --- | --- |
| `docs/team/2026-09-30/engineering-host-integrate-001.md` | 11554 / `b3bfa7a591af28ac3e061911f6fcdb009a63eb947b2c851b5efcfedce6ee5f0b` |
| H+`before.json`（只取restoreBaseline及所需固定身份） | 692531 / `ae30b76f378cedc71955f27308e62787acd7d563e76bebcda2725791b3faf374` |
| H+`cache-inputs.json` | 2323524 / `c6d054990c3afc0e7a113ae21743abe8369eba98bd07c3065a7ceb743ab0dbff` |
| Q+`runner.py` | 41801 / `527c073c3cfe5b0a4e3c45a71a16828e9750f84dddc6e50b302cd77c2b4dcad5` |
| Q+`receipt.json`／`replay-results.json` | 3117 / `ded8668fd2c9ee7bd02f2949bc1b77631bded957eb129fe5b91ce02842cb5e61`；42057 / `273abf03f89859c83059d755fd72dba340181014b1e28d8771658d01b18148c2` |
| `TestArtifacts/FightMatch/HOST-NEXT-001/M03/activation.json`（ADB精确例外） | 225150 / `850cd10a91c29f487e13636ee9ca043a9cf9a19d3bc453620b4ff5890db3153e` |
| `docs/team/2026-09-30/host-integrate-001-s01-central-receipt.json` | 1132 / `5b3ce20dd4277043e527ac69c19d11db2d1079156c81cd4a6a9f9add9bd7af41` |
| `docs/team/2026-09-30/host-next-001-m04-integration-receipt.json` | 2482 / `30778b6797c8d757e7054955b3436b0c2262f7aa3fe64a4fded8c283f6063a97` |

S01 actual `01a1179e-15f8-7442-89fb-cded184baaee`已由中央wait129收completed，三Host＋两meta实际采用、999共享输入／8证据428021B已核；`TestArtifacts/FightMatch/HOST-INTEGRATE-001/S01/receipt.json`7179B／`9c37995ceb9e2318a66980bde2caca547375829a665839141756bc4dc9b924fa`，状态SOURCE_ADOPTED_COMPILE_PENDING。PR17 head `ad0f88a177ded15a05a7c661c80b3797a19ab251`已收GitHub无major／新增findings，M04同快照修正限定ACCEPT；仅21例离线回放、native0，M03原FAILED保持。原PR16产品接收不重审。

## 3. P精确差量及证据白名单

以下前缀仅缩写，括号内是穷尽文件名集合，不是递归／模糊匹配：HS=`Assets/Scripts/FightMatch/Host/`，PR=`Assets/Scripts/FightMatch/Presentation/`，FT=`Assets/Tests/EditMode/FightMatch/`，HT=`Assets/Tests/EditMode/FightMatchHost/`。

| 操作 | 精确P相对路径；来源均为执行时已封签R原字节 |
| --- | --- |
| 覆盖3 | HS+`Editor/FightMatchAndroidBuild.cs`、`FightMatchHostView.cs`、`FightMatchPlayerHost.cs` |
| 覆盖5 | PR+`CandidateBattlePlaybackView.cs`、`CandidateBoardInputView.cs`、`PlayerBattleView.cs`、`PlayerDefaultReferenceView.cs`、`PlayerNavigationRecoveryView.cs` |
| 覆盖6 | FT+`CandidateBattlePlaybackPanelTests.cs`、`CandidateBoardInputTestData.cs`、`LocalizedTextBindingTests.cs`、`PlayerBattlePresentationTests.cs`、`PlayerNavigationTestFixture.cs`、`UguiSceneCompositionTests.cs` |
| 覆盖2 | HT+`FightMatchHostProfileTests.cs`；`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab` |
| 新增4 | PR+`FightMatchResponsiveLayout.cs`及`.cs.meta`；FT+`UguiResponsiveLayoutTests.cs`及`.cs.meta` |
| 暂移5 | `Assets/Scripts/FightMatch/AssetAccess.meta`；该AssetAccess目录内`FightMatch.AssetAccess.asmdef`及`.asmdef.meta`、`FightMatchAssetContracts.cs`及`.cs.meta` |
| 暂移5 | `Assets/Tests/EditMode/FightMatchAsset.meta`；该FightMatchAsset目录内`FightMatch.AssetAccess.Tests.asmdef`及`.asmdef.meta`、`FightMatchAssetContractsTests.cs`及`.cs.meta` |
| 暂移2 | HT+`FightMatchHostSaveIsolationTests.cs`及`.cs.meta` |

保存16覆盖前像；12叶暂移至E后，对确空的两个AssetAccess／FightMatchAsset目录只用rmdir，避免Unity给空目录再生成meta。HSC的HostSession已等于S01后像，不重复同步。共享当前HostProfileTests虽小于P版本，仍必须按共享9648B同步；这是实际输入对齐，不修改R或删除其测试。AndroidBuild必须保留共享204772B／`a64fbb31f03c2ec0a5fcd88da12ac4793e4d4c75d190965c6d7f148a4b4ebc27`，布局测试81160B也必须进入编译。

E只准根文件`activation.json,inputs.json,before.json,preparation.json,runner.py,replay-check.py,replay-results.json,process-events.jsonl,process-after.json,compile.json,after.json,restore.json,receipt.json`；`I/editor.log,I/launcher.log,I/result.json`；`restore/source/<16覆盖路径>`；`park/source/<12暂移路径>`；`archive/source/<20同步路径>`；条件`archive/SceneTemplateSettings.json`。编译证据摘录放compile.json，不另复制DLL、完整Bee图或缓存。旧证据只读。
Unity自然生成范围仅P的Library／Temp／Logs／UserSettings／obj及根.csproj/.sln；P/TestArtifacts旧51个host-io树及其它旧隔离树全部冻结，新增HostRig／存档／探针／Scene为0。唯一容许自然新增源设置是`ProjectSettings/SceneTemplateSettings.json`3534B／`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`，结束归档；另记含此叶的实际完整导入摘要，不混称原1000叶。其它输入改变均失败，不就地修补。

## 4. 唯一脚本适配与一次入口

需要新E/runner.py：Q旧脚本绑定M03身份、图形P阶段及观察判定，不能直接复用启动。只保留并参数化它的身份／前后清单／资源／同快照进程监护／收尾骨架，改为一次I360、上表暂移同步恢复与编译判定；删除Play/probe/隔离激活/四观察和测试逻辑。无需任何新C#、Editor方法或产品工具；runner≤480非空行，apply_patch制作，不造通用框架。
E/replay-check.py仅对实际新runner的受影响函数做离线检查：无真实Popen/ps/kill/Unity，继承同快照合法子进程／外来同名／PID复用反例，再覆盖I仅一次、缺编译证据不能PASS、输入漂移拒绝及32叶恢复；调用真实函数，不能复制判定算法。语法和回放最多2轮累计30秒，首过停止；新脚本审查交GitHub，中央绑定准确源／head，旧PR16或M04回放不冒充新runner审查。
固定Intel Editor=`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，86759760B／`71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。封签runner/activation后通过正常命令审批运行一次`python3 -B E/runner.py <activation-sha> <fresh-C-turn>`，拒绝保存原因，不改道／重试。展开非shell argv，cwd=P：

```text
[Editor,-batchmode,-nographics,-quit,-buildTarget,StandaloneOSX,-projectPath,P,-logFile,E/I/editor.log]
```

环境只覆盖`UPM_CACHE_ROOT=K`及一次新TMPDIR=`/private/tmp/fm-hi01.XXXXXXXX`（mktemp -d，canonical≤40B、本人UID／0700／无链接，初始为空）。不带executeMethod／runTests／QA标记／Host隔离标记；AndroidBuild的InitializeOnLoad构造器在缺少`-fm029QaBuildOnly`时直接返回，不启构建／场景修改。无Play、默认Host或真实存档访问。

## 5. 全部计时、资源与进程边界

准备机械≤120秒（含上述静态≤30秒）；I自Popen起≤360秒、仅1次；自然收尾≤60秒、逐PID一次TERM后确认≤30秒；归档恢复≤90秒；总机械≤660秒。各阶段起止、审批等待和总墙钟分别如实记录，等待不藏成执行耗时。超限立即停止新工作，仍保全证据和必要安全收尾，超支如实报失败，不重置计时器或再开I。
每2秒目标间隔在running／natural-closure／term-confirmation／final执行资源及进程监护，记录实际间隔和开销。必须把**同一份新鲜ps快照先发现并核验owned，再分类消费者**，不能内部另取不匹配快照；保护PID/start/exe/完整argv/cwd/至本次根进程父链，不能按bee等名称放行。每次TERM前重核实际身份与消费者；无SIGKILL、名称kill或外来信号。
本轮只读ps被沙箱报operation not permitted，因此**没有当前无消费者证明**；P锁文件当前缺席也不等于进程安全。C执行前须在获准环境取得新鲜ps/lsof，确认R/P无Unity／编译消费者及P无锁后才同步；若查询不可用即NOT_RUN_BLOCKED，不能拿历史进程表代替。
原ADB27858精确FD1/2旧日志例外及固定SDK新ADB准入沿M03 activation的adbException/sdkAdb和Q对应守卫，不放宽。新SDK server最多一个，须证明本次启动时间、本人UID、exe SHA／inode／设备、精确argv、当前TMP日志头／FD和无源码／编译资源句柄；它不因名称成为owned，不给任何ADB信号。获准ADB若仍存活，保留其TMP日志并明确报告，不能声称所有系统进程皆退出；其它外来消费者拒绝。
E≤32MiB，单log≤8MiB并预留1MiB尾部回执；K≤1GiB且9984叶原payload精确冻结；P生成物≤4GiB，外置空闲≥2GiB；新TMP≤16MiB／512叶。CLR调试FIFO只允许本次已核owned dotnet对应的名字／UID，lstat记录，禁止读内容。旧TMP／ADB日志不清；无下载／包Resolve／restore服务／CLI更新／网络补缓存，发现缺包或下载需求即失败收尾。

## 6. 判定与恢复

启动前固定R999、P1000、K清单和DLL前像，并冻结R的Config／Tools／Generated／ArtSource、精确旧证据及旧TMP身份（已列ADB日志仅允许追加）；不得锁定正在更新的中央文档作为全目录前像。同步16+4后核共享全部999输入逐叶相同，仅保留已声明meta例外。编译后exit0且无C#／程序集加载／batch中止错误，保留导入／编译日志事件和各程序集源／响应文件标识／DLL前后SHA。Host、Presentation、Host.Editor、Host.Tests、Core.Tests必须有本轮Csc或可证明**全部相关源、引用与define一致**的缓存复用链；较新AndroidBuild／布局测试必须出现在实际输入，旧DLL存在或无error文字不足以PASS。缺充分证据记INCOMPLETE，不追加强制重编。
最后新鲜owned闭合＋消费者守卫通过才恢复：先封存20同步后像（新增4移入archive）；16覆盖用已验前像恢复；12暂移按原路径原字节放回并重建两个空目录；保留原测试meta；默认settings若出现则验固定字节后归档。P最终必须回到原1008叶摘要；R的S01采用结果、其它WIP、K及旧证据不动。不回滚／复制Library，后续任务须重新证明源码与缓存关系。
对失败同步按实际已写路径账本恢复；只覆盖仍等于本次后像的文件。未知漂移、未闭合进程、备份不符时停止对应恢复并报告，不递归删除工程／缓存，不强行抹新值。暂移／归档均可恢复；新TMP保留清单，不把清理扩大到旧临时树。编译首错、监护错、恢复错分列，原失败不覆盖。
C只交COMPILE_PASS、FAILED、INCOMPLETE或NOT_RUN_BLOCKED及准确次数／计时／恢复／未运行项；中央唯一收件并决定限定组合接收。正式LOC仍null、正常交互及Android门仍未关；GitHub审查与编译证据分开，不派本地重复审查。本轮writing-for-agents把输入、单次执行和恢复完成条件分开；方案不是执行许可或通过声明。
