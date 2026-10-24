# HOST-SETTINGS-FIRST-CREATE-MAC-001 — 18例定向验证

2026-10-03 · `READY_AFTER_RES_A_CLOSURE`。中央按用户继续实际开发及AGENTS§6，明确延续本次Mac有界例外；云刚READINESS_BLOCKED且环境未变，不重复云等待。匹配PR7 head的GitHub代码门已限定ACCEPT、无待修源码项；RES-A既定探针及owned闭包接收后可直接准备并执行，无需再问中央。若实际输入有修正则重新固定，不提前测试。唯一Unity C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`唯一收件，签发turn `01a0fde0-d533-7b00-ad78-34111f776dda`。本文不启动任何进程。

## 输入、写入和预算

固定[源码包](engineering-host-settings-first-create-001.md) SHA `90b7351a13e16aa1d04281c42a27e003815b093e33adfcca9173578aaaba027d`；源码S回执3641B/SHA `991f403940dfdf536abfa318f36f9a58d99278c094823cf5fb75e7971ce94278`。PR7 `1093faffc096bffa1d36c81d29b2f629430f6f54`／base PR5 `a8b9758ed2db1a43dac9328f162433a4523624c9`，发布manifest3044B/SHA `e14031cb9cc30dfa4911c472a943b6238711fc17ddbdce6020d6984bb78744fd`；[QA代码门回执](testing-host-settings-first-create-pr7-review-receipt.json)5591B/SHA `a79f62efcaa2f8c09bbd54d935f714ca9843f3e518a9ff999a161cbabb1e6808`，summary5960027758 completed、result5960086010 no-major/无inline findings、无formal APPROVED；只是代码门ACCEPT，非本18例已通过。

新根仅 `TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/M01/`，必须起始ABSENT。投影从已收M02/projection复制Assets/Packages/ProjectSettings（不复制Library/Temp等生成物），只以本包S两源覆盖新副本：HostSession20308B/SHA `67d62db541c47e3e1548ec32000605b6dcecf38b98a7332195a0824cc51060a9`、ProfileTests25225B/SHA `3a44b54569966c26745d875e319096f4fe0d98a7513de8a7fbaa66f11daa96dd`。组合998原源31766638B，沿M02的`path NUL gitBlob NUL bytes LF`排序规范，canonical SHA `693d73afb84f7e9555e323637bbed21baca340b19302e6d41dc8737a783132fc`；这是PR3/PR5/PR6/PR7复合输入，不冒称单一PR head。另十个自然meta逐叶沿M03/T-continuation activation完整import后像冻结，零新增meta；仅允许Unity产生既定3534B默认SceneTemplateSettings.json（SHA `5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`）。

新`package-cache/`仅COW复制已核M02的51包缓存，不hardlink/下载/Resolve；M02/M03/P01/全部源码S及旧LAYOUT、共享产品/固定证据只读。新增短tmp仅一次`mktemp -d /private/tmp/fm-hsc-m01.XXXXXXXX`，canonical≤40字节、当前UID/0700、根及父组件无链接，≤16MiB/512叶，保留残留不泛清；大型投影/cache/log外置。新投影generated≤4GiB、cache≤1GiB、证据≤32MiB、单log≤8MiB、可用空间≥2GiB，准备机械操作≤120s。新源码实施chat不能运行本包Unity。

证据闭集恰为`inputs.json`、`cache-inputs.json`、`preflight.json`、`activation.json`、`runner.py`、`process-events.jsonl`、`process-after.json`、`after.json`、`receipt.json`，以及`I/editor.log`、`I/launcher.log`、`I/result.json`、`T/editor.log`、`T/launcher.log`、`T/results.xml`、`T/result.json`、`T/discovered.json`。inputs引用原M02清单/hash＋两后像和十meta，不复制多份大清单；条件未运行的产物不补造。作者文件apply_patch，机器证据可脚本生成。初检/末检核本候选全部源/meta、共享原清单、源码S八叶、M03/M02既有证据及RES-A封存身份，不修旧记录。

## 一次编译、一次18例

固定Intel Unity2022.3.18f1及M03已证exe SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`；不安装/更新CLI/Editor/许可，M03直启合同优先于CLI默认行为。沿T-continuation已证228非空行监督器结构最小适配，≤300行，精确PID/start/exe/argv/父链归属，无其它消费者才单次TERM，不要求每个已证后代argv含projectPath。超时停止新工作→60s自然→单次精确TERM→30s确认；无SIGKILL/按名称kill/设备ADB动作。

首启动前同一activation绑定实际C执行turn、匹配head审查、RES-A完成回执、全部输入/副本/源meta、实际canonical root/tmp/cache、Editor/helper-free状态、runner hash及两个完整非shell argv；不得用准备turn冒认执行turn。准备完成后主程只做一次机械绑定；全部已绑定前检通过后同回合I→T，不逐阶段复批。

两argv同用`-batchmode -nographics -buildTarget StandaloneOSX -projectPath <新projection>`、cwd同projection，UPM_CACHE_ROOT与TMPDIR只覆盖该子进程。I≤360s、含`-quit`，exit0/无compiler-domain或package错误/import完成，实际Host与Host.Tests DLL存在且新身份不同于M03旧DLL；仅通过后T≤180s、不含`-quit`，`-runTests -testPlatform EditMode -testFilter <18全名逐一re.escape后锚定alternation> -testResults <T/results.xml> -logFile <T/editor.log>`。各最多一次，0自动重试，无额外discovery/全量/17例复跑/Play/真实Host/G3/设备/Player/Android。失败保留原件，回主程缩小修复。

预期XML fullname前缀统一`FightMatch.Host.Tests.FightMatchHostProfileTests.`，后缀精确如下（字符串引号/布尔大小写来自既有真实XML，仅复用名称格式，不复用旧pass）：
```text
H03_AbsentLocatorDoesNotAuthorizeCreationOverExistingPhysicalData("orphan-profile")
H03_AbsentLocatorDoesNotAuthorizeCreationOverExistingPhysicalData("unknown-file")
H03_AbsentLocatorDoesNotAuthorizeCreationOverExistingPhysicalData("unknown-lock")
H03_ActiveProfileWithSettingsReopensUnchanged
H03_ActiveReopenAndRepeatedObserveDoNotInitializeAgain
H03_F2ColdReconstructionContinuesOriginalCreationAtBothDurableBoundaries(False)
H03_F2ColdReconstructionContinuesOriginalCreationAtBothDurableBoundaries(True)
H03_FreshCreateRechecksTheRootAfterTheEmptyScreenWasShown
H03_OwnedPreferenceResiduesPreserveBytes
H03_PreparedIdentityIsRetainedBeforeTheStorageFactoryCanFail
H03_ProductSubtreeLinksIncludingDanglingAreBlocked(False)
H03_ProductSubtreeLinksIncludingDanglingAreBlocked(True)
H03_ReadFailureNeverFallsBackToAbsentOrANewPlayer
H03_SettingsAreRecheckedBeforeCreation
H03_SettingsLinksNeverAuthorizeCreation
H03_SettingsOnlyAllowsExplicitFirstCreate
H03_SettingsSurviveInterruptedCreation
H03_UnknownSettingsOrPlayerDataStillBlock
```
十一旧叶名称来源`TestArtifacts/FightMatch/UGUI-01/872b313d139da2b594f4233e38bf6794955f319d6a0febef5b61f0dd03673aaf/q4-correction-17/host-30-graphics/tests.xml`（26559B/SHA `79b428cf22371302534406ea1e6634d7bd3af999685ab577e7abe8f00310986c`）；七新名字来自已封S。主程a08be6仅静态提取/核组合，未运行测试。

合格须Counter恰18各1次、18Passed、failed/skipped/inconclusive均0，分新7/回归11；源码/证据/资源保护及owned闭包同时通过。失败/超时/未运行不算pass；作者不自批。回执交主程→中央/QA，独立审查与实测共同决定限定集成结论，不能接收共享产品、resource-state完整H或真实Demo。
