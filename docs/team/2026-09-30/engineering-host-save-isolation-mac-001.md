# HOST-SAVE-ISOLATION-MAC-001

2026-10-03 · PREPARATION_AUTHORIZED，实际Unity执行待主程绑定本包C新turn、前检和argv后激活；不需要新增用户产品选择。

## 1. 已获授权与唯一执行者

中央在用户继续实际开发及AGENTS §6范围，已明确授权本包最小Mac验证：一次compile/import≤360s、一次精确六HISO≤180s，失败即停止，不重试旧套或FIX04/M16。C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`，Astra/xhigh，唯一串行执行者；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，签发turn `01a0fd7b-5ea7-72f0-ad67-3ba4d6441453`，唯一收件，再回中央。C先完成并封存RES-B契约S（已completed），本包不回写任何S。
[QA PR5代码门回执](testing-host-save-isolation-pr5-review-receipt.json)：5343B／SHA `ba187dd9d0b7b761f29cb92fac0b85ab6e5745cfbc4502cbaea3914592b55d4f`，head `a8b9758ed2db1a43dac9328f162433a4523624c9`／tree `ee14d352b9f89e5c76204bba6433c1c1e3ad7378`，completed/no-major，无finding／无formal APPROVED；仅代码门ACCEPT。
[Host源码回执](../../../TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/S/source-receipt.json)：5655B／SHA `0f208101a2884e73ed6106e06087f1ca46cd2348870f280304036e1b646db2f7`。采用其两源、六名，不改源码，不把静态检查当行为证明。共享Host前像和FIX04继续冻结。

## 2. 精确新投影输入，不改旧投影

新唯一根 `TestArtifacts/FightMatch/HOST-SAVE-ISOLATION-001/M01/` 起始须ABSENT：
- `projection/`：只读导出本地Git对象PR3 `6818405b859d71bf8c0f116299e7e9605a3d769a` 的Assets、Packages、ProjectSettings全部993个100644 blobs；用Host S回执两源逐字节覆盖／新增后成为994叶、31,719,742B。不可复制dirty共享Assets、旧LAYOUT projection/Library/Temp、Git目录或任意真实存档。
- 排序按path逐行UTF8 `path NUL gitBlob NUL decimalBytes LF` 的输入身份SHA必须为 `c2e858b4557bd673cf74b3b65da7245b995d64ec9e374417665c15b04c049ae9`。准备时先把994逐叶path/bytes/gitBlob/SHA列入 `inputs.json`，核此digest再写投影；所有新产品输入由此精确闭合。PR5在本地无commit对象不是fetch权限；中央publication manifest映射与两后像已证明候选身份，不执行fetch/checkout/branch/commit。
- 额外材料/CSV、RES-B契约、LOC M16、布局FIX04源、probe、临时Editor工具均不混入；不调用AndroidBuild或任何迁移方法。不进Play，不启动真实Host/G3，不传fmHostSaveIsolation argv到Unity进程（六测试在owner测试根传给纯接缝）。
- 原包图两文件不改：manifest blob `47f1c2c01f1e0b4a6457f943d2b88934b840f294` 1775B，lock blob `3bd49b66f9816d97bf01c519c02ef46ebb628bbc` 11101B；沿既有51固定依赖和registry，不新增依赖/Client.Add/Resolve循环。
- `package-cache/`：允许从已验 `TestArtifacts/FightMatch/LAYOUT-S1-001/package-cache` 只读复用缓存，优先同卷copy-on-write，不硬链接，不让UPM写旧缓存；preparation记录来源集合/类型/hash和当前总量，开始复制前与固定manifest/lock比依赖身份，新cache≤1GiB。已有I002记录731,367,785B仅历史，当前重新核，拒绝symlink或不明来源。仅需的缓存可复制；不下载新版本或安装工具。固定包payload可据原I/prepare.json的17包9929叶/394441107B清单从旧projection/Library/PackageCache只读校验，若需要仅复制这些叶到新projection/Library/PackageCache；旧cache和旧Library全程零写。不要重复复制相同payload，不复制任何ScriptAssemblies/Artifacts。
- 原I prepare固定SHA `07da268ba6abf254db99017589f77b57b5a41a58edf0ecdfe55f02005ec2493e`、1,988,053B；这是可复用缓存身份，不是本候选编译证据。正常UPM若仍需联网解析，先停并报告准确缺项；当前不自动扩下载权限。

## 3. 自然输出白名单与保护

投影Assets仅允许Unity自然新增四meta：
1. `Assets/Scripts/FightMatch/Host/FileLocalePreferenceStore.cs.meta`（PR3依赖）
2. `Assets/Scripts/FightMatch/Host/ILocalePreferenceStore.cs.meta`（PR3依赖）
3. `Assets/Tests/EditMode/FightMatchHost/LocalePreferenceStoreTests.cs.meta`（PR3测试依赖）
4. `Assets/Tests/EditMode/FightMatchHost/FightMatchHostSaveIsolationTests.cs.meta`（本包）
不手写meta、不导出共享；记录GUID/SHA，扫描投影已有GUID唯一性。994原输入均保持字节及集合；Scene/Prefab/原meta绝不保存或改动。只允许投影 `ProjectSettings/SceneTemplateSettings.json` 自然创建为既有2022.3默认22项（3534B／SHA `5baf593374ad67246277a435d8d0ded7167696eef6e05d2d245670e490625d8c`）；其它Settings变化停止，不能自动接收。
仅新投影的Library/Temp/Logs/UserSettings/obj及生成csproj/sln允许Unity自身产生，合计≤4GiB，不复制回仓库；保留异常原件。证据≤32MiB，各日志≤8MiB，测试临时文件≤16MiB/512叶，准备≤120s，启动前后可用空间≥2GiB。准备超限停，不开Unity。
C先固定共享1089、Host S八叶、RES-B S十叶、FIX04十叶及前S保护集合；旧LAYOUT投影的Assets/Packages/ProjectSettings输入快照和旧缓存/17payload身份只读前后相等。旧投影Library不作为本包写入点。共享Logs/Temp/Library/UserSettings、真实Application.persistentDataPath和用户存档完全不写、不读取正文、不备份、不建占位。
测试固定源码的自有目录为Path.GetTempPath()/FightMatch-HISO-<GUID>，本包将进程TMPDIR绑定新M01/tmp/；只允许该新tmp中的owner临时子树及Unity本次临时文件。原测试TearDown仅清其自建子树；失败残留原地留证，不执行宽泛清理。

## 4. 前检与执行参数

固定Editor：`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，Intel x86_64／2022.3.18f1，86759760B／SHA `71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。不安装更新Editor/CLI/Pipeline，不变更license或读凭据。主程查CLI为beta.8，self-update --check因DNS失败exit6；不把CLI默认2s后SIGKILL用于本包，沿固定Editor和已定owned收尾政策。
C前检实际Editor/架构、输入/缓存、空间、精确root及symlink、包图、当前进程/项目锁；不得启动已被其它Unity占用的本投影，也不得关闭其它进程。记录本次实际thread/turn及read-only命令/exit；进程读被拒时保留原文，不自称无进程。完成以下新证据后先final给主程绑定执行，不提前运行：
`before.json`、`inputs.json`、`cache-inputs.json`、`prepare.json`、`process-before.json`。
主程在同根保留槽 `activation.json` 绑定实际C turn/前检摘要与完整argv；后续执行工具只可新建 `runner.py`（≤300非空行、有实际源码hash、使用已有小进程监督结构，不复制大型旧runner）及 `process-events.jsonl`、`process-after.json`、`after.json`、`receipt.json`、`I/editor.log`、`I/launcher.log`、`I/result.json`、`T/editor.log`、`T/launcher.log`、`T/results.xml`、`T/result.json`、`T/discovered.json`。不另加runner/probe或额外Unity启动。

编译argv固定Editor + `-batchmode -nographics -buildTarget StandaloneOSX -projectPath <M01/projection> -quit -logFile <M01/I/editor.log>`。cwd为新projection，UPM_CACHE_ROOT为新M01/package-cache，TMPDIR为新M01/tmp；环境只对此进程覆盖，不改用户配置。一次≤360s，exit0且无CS/compiler/domain失败、实际DLL和导入完成且自然meta/保护均合格才可开始T。
测试argv固定Editor + `-batchmode -nographics -buildTarget StandaloneOSX -projectPath <M01/projection> -runTests -testPlatform EditMode -testFilter <selector> -testResults <M01/T/results.xml> -logFile <M01/T/editor.log>`，**无-quit**；selector由Host source-receipt六fullnames逐个Regex.Escape后用全锚定alternation组成，写入activation；空/不符禁止启动。只一次≤180s，不重试，不跑历史222/234/full/布局或真实Host。
无需另起发现进程或编写发现probe：本次UTF实际XML的六个test-case/fullname作为真实发现/运行集合，discovered.json记录XML身份及actual/expected集合；总6、passed6、failed/skipped/inconclusive0、每项fixture身份精确才通过。缺项/超集/Ignore/无XML不是通过。

## 5. 进程、安全停止与返程

每次只启动一owned Editor，记录PID、PPID、starttime、exe/argv/cwd、子进程关系/身份、exit code及UTC。达到阶段限时先停止新工作，给owned进程自然退出60s；仍活仅对重新匹配本包PID/starttime/exe/root的进程发单次SIGTERM，再等30s，仍活报告BLOCKED，不SIGKILL、不全局kill、不按名字关进程；关闭后实际核完整owned闭包。不因工具yield/消息中断重复启动；保留每次信号记录而非覆盖。
任何编译/测试/保护/预算/权限错误即停止后续阶段，保留日志和不完整结果，实际工具拒绝不换执行者/目录/命令规避，缺权限具体报事实。没有发生的run/PID/exit使用null/NOT_RUN，不补成功。产品缺陷回主程更小源码修正包、重新新head审查；本验证阶段没有源码修复权。
C final给准确执行turn、输入/runner/hash、run次数/UTC/exit、实际6例或阻塞、四自然meta、原输入及共享保护、缓存/空间/日志量、进程闭包及证据receipt；主程唯一接回后交中央，作者不自给integration ACCEPT。不创建Git/PR、不做本地Code Review、不提交/导出meta、不进入G3/真实存档/Scene/Prefab/Android。

