# HOST-CONTINUATION-MAC-001

2026-10-07 · PREPARED_UNRUN。[中央授权准备](central-host-continuation-validation-preparation-001.md)；本轮仅文档，中央激活才转C写权，不重用旧许可。

## 1. 责任与固定输入

C=`01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`（FightMatch C-MAC-HISO 定向验证），Astra/xhigh。issuer actual=`01a116c6-f3ae-7dc3-bfc4-ff4d4439db3d`；激活核C停写及新actual，中央收件。

仓库相对路径：A=`TestArtifacts/FightMatch/`；S=A+`HOST-CONTINUATION-SOURCE-001/S/`；M=A+`HOST-SETTINGS-FIRST-CREATE-001/M03/`；P=M+`projection/`；K=M+`package-cache/`；E=A+`HOST-CONTINUATION-SOURCE-001/M01/`（实核缺席）。H=`Assets/Scripts/FightMatch/Host/`；V=`Assets/Scripts/FightMatch/Presentation/`；T=`Assets/Tests/EditMode/FightMatchHost/`。

S+`source-receipt.json` 37790B/SHA256 `410a5ccaa8b7216dec2bf3fa7619e61690e36befc6a47e6eaba584644ed18fb6`：四candidates为权威；S十叶/旧失败封存，不重跑检查器。M+`inputs.json` 8974B/`a4ba0210941efc5a54029fb491654949c578dc71f7972b5462f81d94c377d784`，按sourceManifest.files→overlays→metaManifest.preflight.frozenNaturalMeta展开。

P实核1008叶31770978B全等，已有DLL/无锁；组合版本见M.inputs。HISO/PREF/HSC/HostFixture/asmdef/字体/OFL/六业务输入全由清单固定；旧PreferenceTests30074B、旧AssetAccess四源/六meta不换新版。

## 2. 唯一同步差量与回滚

仅以下12叶；先归档前像再覆盖P，Session同字节不重写：

- S/source的H+`FightMatchPlayerHost.cs`、`FightMatchHostView.cs`、`FightMatchHostSession.cs`，T+`FightMatchHostLocaleWiringTests.cs`。
- 共享V+`CandidateBattlePlaybackView.cs`、`CandidateBoardInputView.cs`、`FightMatchResponsiveLayout.cs`及其`.meta`、`PlayerBattleView.cs`、`PlayerDefaultReferenceView.cs`、`PlayerNavigationRecoveryView.cs`；共享`Assets/UI/FightMatch/Runtime/FightMatchRuntimeRoot.prefab`。

八叶已核匹配[布局manifest](../../versioning/2026-10-02-layout-s1-pr/native-p/publication-manifest.json)（9954B/`1be36cbf7b78985aa565dfff88256a3b195447f492c1c68db6fca26ee96a3a23`）retainedSources15/productExports；不带AndroidBuild/布局测试/WIP，Scene冻结。

导入前1011叶32086051B；排序`path NUL gitBlob NUL decimalBytes LF` SHA256=`7f73b4603e4a142e02f3c04b627a0eda3c80a60a079a99aaf708bf182c64b769`。逐叶核bytes/SHA/blob，漂移停。唯一自然meta：T+`FightMatchHostLocaleWiringTests.cs.meta`，导入后核GUID唯一/封签，禁手造。仅另容许Unity默认`ProjectSettings/SceneTemplateSettings.json` 3534B/`5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`出现，不发布；其余输入/meta冻结。

E仅：`inputs.json,cache-inputs.json,before.json,preflight.json,activation.json,runner.py,process-events.jsonl,process-after.json,after.json,compile.json,restore.json,publication-inputs.json,receipt.json`；`I/{editor.log,launcher.log,result.json}`；`T/{editor.log,launcher.log,result.json,results.xml,discovered.json}`；未运行不补造。另准：

- `restore/source/<repo-path>`：八个实际被覆盖既有叶前像；Session无需备份。
- `candidate/<repo-path>`：§5固定31叶＋新自然meta，封存实际验证字节。
- `test-io/<本轮新增GUID>/`：下述最多两个HostRig目录归档。
- `restore/SceneTemplateSettings.json`：仅默认设置本轮出现时归档。

P另准Unity生成Library/Temp/Logs/UserSettings/obj/根.csproj/.sln；K冻结，下载/Resolve/清缓存/复制Library均0。前后核共享源、S十叶、M旧证据、固定清单、Scene/原meta、Git HEAD/index/master，原件只读。P生成物可更新，不冒用旧DLL证据。

结束封签后像/owned闭合后：八覆盖叶候选复制至candidate，再恢复前像；三新增输入及自然meta移入candidate，余下发布叶复制固定源。新默认设置/HostRig目录按指定归档，P恢复1008叶，Library不回滚。漂移/活进程则停恢复并报告。

## 3. 有界执行与I/O

Mac例外：OSXEditor根/lease；异平台skip不通过，不重试dots。激活后准备→I→T→归档；activation绑定actual/输入/P/K/tmp/Editor/runner SHA/argv/预算。

复用M+`runner.py`（28991B/`f5f425c535db79711157262f8b96d2629ce0200b9c9f449201f22d10f68d7db4`）监督/归属/XML结构；≤360非空行，仅改路径/当前保护清单/差量/meta/8+2/恢复，不保留过时shared1089断言。准备机械≤120s，静态检查≤30s，不运行runner探路。

Intel 2022.3.18f1：Editor=`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，86759760B/`71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。不更新/安装。P无锁、无其它Unity/compiler消费者才启动，不关闭用户Editor。

下列argv展开绝对路径，cwd=P、非shell。仅子进程UPM_CACHE_ROOT=K、TMPDIR=一次新建`/private/tmp/fm-hc-m01.XXXXXXXX`；实际canonical≤40B/本人UID/0700/无链接。

```text
I: [Editor,-batchmode,-nographics,-buildTarget,StandaloneOSX,-projectPath,P,-quit,-logFile,E/I/editor.log]
T: [Editor,-batchmode,-nographics,-buildTarget,StandaloneOSX,-projectPath,P,-runTests,-testPlatform,EditMode,-testFilter,selector,-testResults,E/T/results.xml,-logFile,E/T/editor.log]
```

I一次≤360s、exit0/无compiler-domain-package错误/import完成才T一次≤180s；T无-quit，0重试/discovery。准备至封存总≤900s（含退出/恢复）。停止新工作→60s自然→核PID/start/exe/argv/父链且无外部消费者→owned每PID最多一次TERM→30s确认；不SIGKILL/按名kill。逐段闭合。单log≤8MiB、E含归档≤32MiB、P生成物≤4GiB、K≤1GiB/51包、外置余量≥2GiB、tmp≤16MiB/512叶；每5s查预算，超限/网络依赖/漂移走退出。

I/O仅tmp下8个`FightMatch-HCP-<32hex>`、2个`FightMatch-HISO-<32hex>`所属树，包含模拟根/lease/settings/受控links/sentinel；既有TearDown可清本人树。06/07各新建P+`TestArtifacts/FightMatch/UGUI-01/host-io/<32hex>/`，最多2，已有51目录原字节保留；全树≤16MiB/2048叶/2048目录，link不逃所属GUID。结束新目录移E/test-io。默认Host Start/真实PlayerSave/Play/场景保存复开导出/设备/APK/全回归均0。

## 4. 十项与验收

selector=S回执tests.futureSelector；fullnames按以下前缀＋后缀，10名各1次：

```text
FightMatch.Host.Tests.FightMatchHostLocaleWiringTests.
HCP_01_LoadBeforeBind
HCP_02_SelectAndRetry
HCP_03_Feedback
HCP_04_Rebind
HCP_05_CancelStartup
HCP_06_DisposeOrder
HCP_07_SettingsFirst
HCP_08_NullSourceAndIsolation
FightMatch.Host.Tests.FightMatchHostSaveIsolationTests.
HISO_05_PlayerAndPreferencePathsRemainUnderOneEffectiveRoot
HISO_06_ReopenRetainsRootAndTeardownReleasesLease
```

XML根及case Counter恰10Passed/各1次，0Failed/Skipped/Inconclusive，分别统计8+2。compile.json记录源→程序集/日志编译事件/DLL前后SHA：Host、Host.Tests、Presentation须本轮编译证明及新身份；其余注明编译或复用。T输入同I，意外重编须记录并核输入一致。全部保护/预算/owned/归档通过才报VALIDATION_PASSED，C不自签ACCEPT。

HCP05不证明已yield中取消；06/07仅组件/真实Session，非Host/EventSystem/首帧。null LOC仍阻断；资源/Demo/设备/PR4 P1未闭合。

## 5. 独立审查发布闭包（本轮不发布）

唯一base=已有PR1 head `3541930877837e14b58020b910976b8b67b56e42`。本地完整git tree已比对预定1011叶：无删除、31差量＋待自然meta=32。publication-inputs列逐叶来源/before-after blob/SHA；base+差量必须与验证Assets/Packages/ProjectSettings全等，仅排除默认SceneTemplateSettings：

精确32叶=§1固定清单＋§2差量/自然meta相对base的差集；按blob机械展开，禁按共享目录通配。包括原AssetAccess十叶以对应实际编译，不采纳新版。

中央发布前核远端base/blob/新head，不猜未来hash。支撑仅本包、S/source-receipt.json/source.patch、C inputs/compile/receipt/publication-inputs及T/results.xml，发布manifest后签。GitHub独立审同head，不借旧门/不改PR4；网络核对不延误C准备。
