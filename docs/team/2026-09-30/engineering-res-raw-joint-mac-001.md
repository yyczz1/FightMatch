# RES-RAW-JOINT-MAC-001 — 固定RAW/B/D/C，一次I→74T

2026-10-03。用户AGENTS §6与中央明确批准：RAW/C源固定且PREF安全收尾后，唯一C可先执行本74集合；PREF发现器纠正另签，不空等、不重跑其D。issuer/唯一完成收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1000b-6980-78f3-bbc5-a8a31fd972ae`；唯一执行owner `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，新actual turn由激活绑定。本稿本身无执行结果。

## 固定输入与实际前像

- RAW取 `TestArtifacts/FightMatch/RES-D-RAW-001/FIX01/S` 十叶369452B，receipt15911B/SHA `6b13e82a1673af5b43abf82d1ffe995d34c493b6af56dd8e96330ef4545a4d2a`，patch87097B/SHA `58bc34bd73ea14c0b6a03d672ea93ab12e29490807bb1781733b6bc68800e163`。C取 `RES-C-CONTRACT-001/FIX01/S` 八叶，receipt12616B/SHA `7abff33326b5dc6f0716627ed74b57fde73e8b03f1d90a47b293d7bf7f0c9c9e`。B FIX03/S receipt21906B/SHA `b422bc2f2777e103ec9015ef67ac2daace91a3f70a6b007a51d58abbbaab97f4` 的38名和未变两测试源、D FIX02/S receipt10990B/SHA `145c30289c4a8dedb6bdb8381eb4645853dec3307bad8dc4e09b2a372c8339db` 的13名和未变测试源一起固定。上述简写根均为 `TestArtifacts/FightMatch/`，不运行其verifier。
- 新前像是[PREF M01](../../../TestArtifacts/FightMatch/LOC-IMPL-B-PREF/M01/receipt.json)的真实收尾，不回填旧联合23。PREF receipt5389B/SHA `d0af4c5f46eeb559bb3399f677e9b8b095c2f9e9b701936401d01669124d9105`、run242970B/SHA `2515e490a1e28ffa362baaaf72473ab8d68378978a4461938bd1cc3076747f4f`、after6870B/SHA `0156c8631b1e426e6c156120231c7d9c041ea3dd3f62c5f715f2c13d3e79b9d9`、assemblies8617B/SHA `0c24f60ca89359a62a0882e991cc602a0ca88c0cfa1f32c19299eb2ca72c4668`；1027源/meta/config、531GUID不变，D1/exit3/T0、O空、owned[]/consumer CLEAR/原ADB缺席。主程cadc78核12原件和当前PR3三源/三meta/五DLL；原PREF失败及空发现目录保持。没有PREF新源码/程序集变化，不构成本74的输入冲突；发现器开关本次不传。
- 复用P=`R/TestArtifacts/FightMatch/RES-01A/P01/projection`、cache=`R/TestArtifacts/FightMatch/RES-01A/P01/package-cache`，R为当前仓库绝对根。以PREF run.currentInputs的1027项作为进入清单；53固定包和68必要package/SDK入口、七资源asmdef/DLL沿PREF与旧联合真实记录绑定，source/meta/包源码无未授权漂移才启动。只对当前P/必要入口检查，复用封存历史，不复制工程/Library、不重扫旧历史或全cache。

## 唯一投影增量、meta与编译

仅apply_patch将以下六候选投影，其他1022原项保持：

| 逻辑路径 | 固定后像 bytes / SHA256 |
| --- | --- |
| `Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs` | 38720 / `25129b814768e39fc359a8d374cf52b735b4aac37fb269521669f3a097d9fe7a` |
| `Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetAssetProvider.cs` | 19585 / `a060f985709f19d9b139014e8169ca068c96425d6807d1270d13f8dd26674538` |
| `Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetPackageLifecycle.cs` | 26942 / `16c4d4a9273ee9096fb6a5dc0d71b9d19abfe414d5129a559d21906b6fcd592b` |
| 新 `Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceRuntimeTests.cs` | 51958 / `c0d12d460cf1e517b6617ab63b2bf5c716d65c7ddf2a233fa4b5b368868d19f4` |
| `Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceRuntimeContracts.cs` | 16774 / `4ef8e56e70a19f7df81105fa46b3b1d334f39ce956610285845f1c594ec0520b` |
| `Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceRuntimeContractsTests.cs` | 28399 / `4fe6bb1ed6e2c5bc0d62685b807de42db9a38e2069d07547abbd27502503e13b` |

96ba7e已机械核五个当前前像等PREF记录、新RAW测试及其meta缺席、六后像固定、74普通Test精确名；无Unity执行。投影六源后1028项；只允许Unity为新RAW测试自然生成一份.cs.meta，既见GUID不可重生，I成功须完整MonoImporter/唯一GUID；最终1029项/532GUID。既有16份资源meta与三份PREF meta及其它meta全部保持。新meta的原text/path/bytes/SHA/GUID/importer存入I结果/after，T严格等I后像；不手造、不改目录meta/asmdef/资源。I需真实Csc并更新FightMatch.AssetAccess、FightMatch.YooAssetAdapter、FightMatch.AssetAccess.Tests三DLL；其余四SDK DLL记录实际，不强求变化。另记录PREF五DLL，保持五者字节不变。编译错误即停，不现场修源、补I或重生meta。

## 一次联合测试与版本归属

run精确名称集只从四份固定receipt取：RAW.test_fullnames=12、B.candidate_test_fullnames=38、D.test_fullnames=13、C.test_fullnames=11；全部普通Test，已核74唯一名，无TestCaseSource推算或额外discovery。T的单argv filter严格为以下五类分号OR（固定UTF1.1.33语义已由[旧联合合同](engineering-res-cd-joint-mac-001.md)绑定）：

```text
^FightMatch\.AssetAccess\.Tests\.FightMatchAssetContractsTests(\.|$);^FightMatch\.AssetAccess\.Tests\.FightMatchAssetProviderTests(\.|$);^FightMatch\.AssetAccess\.Tests\.FightMatchResourceReleaseSetTests(\.|$);^FightMatch\.AssetAccess\.Tests\.FightMatchResourceRuntimeContractsTests(\.|$);^FightMatch\.AssetAccess\.Tests\.FightMatchResourceRuntimeTests(\.|$)
```

I成功及owned闭合后才T一次。XML全局及test-case fullName Counter必须等74精确名且各一次；分别记RAW12/B38/D13/C11的case/Passed/Failed/Skipped/Inconclusive和整个74结果。不能只认exit0或用旧B38/D13/C10冒充新输入；一个分组失败保留其他真实结果，但公共编译/保护/消费者异常不能称任一完整门通过。仅运行既有契约、fake SDK/内存流与新RawReadState测试，不运行真实YooAsset包初始化/Ensure/下载/解包、Host/LOC/PREF/场景/Android/设备或旧head红测。
版本各自绑定：C FIX01代码head `19179f3a68725d4f806bbc99989bf514f626acbf`；B原 `49f177ca5594cf987d12bd892b726145b8e41fc8`、D原 `0377c31fef0a0bc08478175c6a22667bb21c8b73`仅为历史基础/未变测试证据，受RAW改变的生产以本次source SHA为准。RAW head尚未发布可null；中央收到新测试自然meta后固定首次head并唯一交GitHub review，另关联本次source/meta证据，不为发布追加测试或回写封存run。未获独立代码门与验证不作integration ACCEPT。

## 资源、进程、证据与停止

新E=`R/TestArtifacts/FightMatch/RES-RAW-JOINT-001/M01`（96ba7e核ABSENT）；仅17叶：run.json、runner.py、preflight.json、I/editor.log、I/stdout.log、I/stderr.log、I/result.json、T/editor.log、T/stdout.log、T/stderr.log、T/results.xml、T/result.json、process-events.jsonl、process-after.json、after.json、assemblies.json、receipt.json，失败仅实际子集。最小适配旧联合runner，≤400非空行；绑定新actual turn/issuer/receipt六源前后像/准确argv/既有和新meta规则。最多2次新纯机械语法/guard/selector检查各≤10s、OS/Unity0；不执行任何旧runner main或产品verifier。
固定x86_64 Editor=`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，86759760B/SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。共用argv前缀 `[Editor,-batchmode,-nographics,-buildTarget,StandaloneOSX,-projectPath,P]`；I追加 `[-quit,-logFile,E/I/editor.log]`；T追加 `[-runTests,-testPlatform,EditMode,-testFilter,上述单argv,-testResults,E/T/results.xml,-logFile,E/T/editor.log]`，T不带-quit或发现/旧progress开关。新TMP一次 `/private/tmp/fm-raw.XXXXXXXX`、canon≤40B，TMPDIR子进程隔离；UPM_CACHE_ROOT固定cache。不更新/安装CLI、Editor、Pipeline或包。
同回合准备≤120s→I仅一次≤360s→成功才T仅一次≤180s；含finally总≤900s，最多2Unity，无重试或第三次编译。执行脚本预算不是整个会话耗时。cache≤2GiB、P生成≤4GiB、总≤7GiB、证据≤32MiB、单log≤8MiB、TMP≤16MiB/512叶、free≥4GiB；cache大小只元数据计量，不宣称重hash全cache。P生成目录仅本次Unity自然变更；共享树/Packages配置/SDK源码/旧S与历史证据/Git零写。PREF旧O空目录、M01十二叶与其TMP保留。固定源/包/预算漂移或失败即封存停，不借同一回合修源/削断言。
安全沿旧联合与PREF同一exact-owned流程：每次启动fresh consumer/lock；自有非ADB PID/start/exe/完整argv/cwd/本次历史父链一致，先自然等60s，仍存才各至多一次SIGTERM后≤30s，无SIGKILL/按名kill/外来信号。原ADB63318缺席可用；仍在必须原UID/start/argv/exe/cwd且P FD0，新未知ADB仅必要元数据后BLOCKED，不操作ADB；自有finally与外来异常门解耦。工具拒绝原样记录并停，不绕过FIX04/M16拒绝。
正式完成后一次send_message仅回主程/local/Astra-xhigh，交actual turn、I/T次数PID/exit/阶段耗时、实际17叶、XML74与四分组、新RAW自然meta完整原文、旧meta/1029闭包、七资源DLL及PREF五DLL、owned/消费者与原失败保持。作者不自ACCEPT、不自行启动PREF纠正/后继；主程接回交中央首次RAW发布及唯一QA门。
