# D16 full06 必要全量正式交付

执行结果：**失败，exit 2**。4181/4181 Passed，完整墙钟492.633793秒（8分12.634秒）；33门中31真、2假。唯一磁盘空态episode缺少已签条件要求的夹持配对，不能因测试全过、末态恢复或≤600秒改判。`full_goal_verified=false`、`five_minute_goal_verified=false`；本报告为C执行交回，实际接收须由原R独立审查。
依据：[PLAN.md](PLAN.md)末节“D16唯一实际签发：full06必要全量”；原C聊天`01a0e404-d89d-7ab2-bece-3cd1df3fbc52`，准确回合`01a0f231-745b-74a1-a3a4-35fff3e511a9`（startedAt=1790769722）。原R静态ACCEPT回合`01a0f225-7860-7d31-97cf-9033248eca58`仅准许后续绑定和签发，不能代替本轮实际验收。
下列签发命令仅执行一次；本C未另跑准备preflight。准备文件`execution_authorized=false`为创建时记录，后续PLAN签发提供实际授权，原字段未改。

```text
python3 TestArtifacts/FMTestPerformance/2026-09-30/run_paired_full_validation.py --mode full --source-fingerprint d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90
```

环境：Unity 2022.3.18f1 Intel；固定实物`/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/MacOS/Unity`，86759760bytes，SHA256=`71a55038cb730aa3d01f0524e2002a599a531e5deda65a028d06a6f96207c88f`。
完整argv保存在[run.json](unity-runs/full06/run.json)：顺序Compile→无过滤EditMode全量，精确继承D14，仅输出路径换full06；两进程均含`-releaseCodeOptimization -disableManagedDebugger -buildTarget osxuniversal`。Compile含`-quit`；tests含`-runTests -testPlatform EditMode -testResults`，无`-quit`或`-testFilter`。

| 冻结签发文件 | bytes / 行数 | SHA256 |
|---|---:|---|
| [run_paired_full_validation.py](run_paired_full_validation.py) | 41922 / 588 | `9f35fa5662046618b55f46c62f9fbf639d68a3b4f177c51073450fe8660cb553` |
| [paired_preference_guard.py](paired_preference_guard.py) | 19296 / 310 | `9dd13981b40af2d07ac4651d08913fce1a688f73f218cb653e68a0f31b9ec14b` |
| [known-unity-cache-bodies.json](known-unity-cache-bodies.json) | 15112 / 397 | `6c1a3edf4727dbfa866421a400249c942a4158e323830782c8c7e753a6ac72aa` |
| [paired-mode-baseline.json](paired-mode-baseline.json) | 4447266 / 54736 | `4f1b2affefe50110e02ab97cb296d783824c6124870e0bc0423ea179fe445b36` |
| [paired-full-preparation.json](paired-full-preparation.json) | 8981 / 148 | `e6a813fe3f3a17ceaf78f916e41bf732578709c844a94cce4a52730412b5b789` |
| [expected-no-debugger-full-tests.json](expected-no-debugger-full-tests.json) | 42391 / 569 | `610c5989c90a14fda5e1a7456573e590ac9d7ecaef50c5eccfca9ce93a3f90cb` |

完整计时UTC：2026-09-30T12:03:33.405631+00:00 → 12:11:46.040847+00:00。自wrapper最早入口计入全部准备、Compile、4181、配对观察、自然等待和终核；在最终result序列化及工具服务返回之前结束。报告统计和C随后只读退出核对不计入运行成绩。

| 阶段 | 秒 |
|---|---:|
| preflight | 7.081182 |
| preference_entry | 8.085570 |
| compile | 20.159693 |
| compile_verification | 3.551133 |
| tests | 425.466448 |
| preference_closeout | 20.261728 |
| final_verification | 7.685694 |
| setup_and_reporting | 0.342344 |
总计492.633793秒；≤300：false，超192.633793秒；≤600：true，余107.366207秒。单独落在300～600区间不满足所有保护门，完整目标仍false。
tests进程425.466448秒，XML根duration=407.6121687秒，4181个case duration合计399.650337秒；XML与case差7.9618317秒，tests进程与XML差17.8542793秒，口径不混用。
具名出现以Counter计数：4181次、4176种fullname，missing/unexpected均空，所有case Passed；failed/skipped/inconclusive均0。原[029/016 XML](../../FMDemo029/mac-r1/runs/016/tests.xml)的4041次及同名重数全部保留，另140次新增；本轮两组case合计分别324.850929/74.799408秒。该旧XML为3006081bytes，SHA256=`a03827dbb1b25a5727b51833d8bc8ac076a7f2803212f64d52f72d273c45dad4`。
按既有性能基线分组：原4106次325.336760秒，D3新增20次13.334608秒，D6新增55次60.978969秒，新增75次74.313577秒。不可将仅按fullname去重的字典代替出现次数或时长求和。
重复fullname及次数：``FightMatch.Core.Tests.BattleRouteValidatorTests.Validate_EndpointMismatch_PrecedesCellErrors(System.Collections.Generic.List`1[FlowPuzzle.Core.FlowPos])``×4；`FightMatch.Core.Tests.CandidateCharacterGrowthTests.InvalidDefinition_IsRejectedAtExactField("CritBase",FightMatch.Core.ExactRational,InvalidValue)`×2；同类`InvalidDefinition_IsRejectedAtExactField("CritCap",FightMatch.Core.ExactRational,InvalidValue)`×2。XML根asserts=0不代表断言被删除；源、编译输入、四定义、NUnit和完整多重集合证据保持。

| 历史全量 | 墙钟秒 | XML秒 | case总秒 | 已记录结果 |
|---|---:|---:|---:|---|
| [D9 full02](unity-fixture-full-delivery.md) | 683.685038 | 643.1212897 | 632.991777 | 4181过；超600秒失败 |
| [D10 full03](unity-release-full-delivery.md) | 402.589015 | 332.8055378 | 326.527266 | 4181过；偏好保护失败 |
| [D14 full05](unity-debugger-endpoint-full-delivery.md) | 464.110414 | 344.428682 | 337.898581 | 4181过；偏好保护失败 |
| 本轮D16 full06 | 492.633793 | 407.6121687 | 399.650337 | 4181过；缺空态夹持配对失败 |
历史记录不重算或追认；各轮观察合同不同，上表仅列原始口径，不能归因为单一改动。D16墙钟较D14多28.523379秒。2026-09-29原4106墙钟未含必要Compile，不与本轮完整墙钟直接等价。D15只是一项诊断用例，也不作全量基准。

33个运行门：以下2项false：`native_preserved_and_disk_empty_episodes_accounted_for`、`preferences_stable_and_preserved`。异常原文：`RuntimeError('Paired preference preservation or observation contract failed; raw evidence retained')`。
true：`source_unchanged`、`compiled_dlls_unchanged`、`old_io_preserved`、`only_new_guid_io`、`io_caps`、`old_host_io_preserved`、`only_new_guid_host_io`、`host_io_caps`、`full_io_delta_bounded`、`full_host_delta_bounded`、`baseline_xml_unchanged`。
true：`expected_tests_unchanged`、`runtime_unchanged`、`wrapper_unchanged`、`all_prior_evidence_preserved`、`release_compilation_preserved`、`release_mode_and_assertions_verified`、`mode_baseline_preserved`、`old_test_occurrences`、`exact_test_occurrences`、`all_tests_passed`、`preference_observation_complete`。
true：`preference_helper_preserved`、`finite_body_spec_preserved`、`all_editors_and_children_closed`、`managed_debugger_listen_disabled_verified`、`processes_complete`、`xml_duration_covered`、`approved_source_fingerprint`、`run_leaf_scope_and_size`、`clock_consistent`。
C另据本轮冻结证据复核身份、XML/Counter、逐样本摘要/配对和编译记录，统计核对无失败；这些核对不替代上述偏好门，不是新增Unity测试，也不替代R审查。

三视图证据731条=486 disk+245次实际native；243组=pre5/compile11/owned_child_exit1/compilation_snapshot2/tests213/post11，242组完全相同，0组空态夹持。入口跨度8.016486秒；最大单组0.147186秒（≤1）、同阶段间隔2.476497秒（≤3）、跨阶段间隔7.704517秒（≤10），最长native实际读取0.038390秒。
所有245次native及485次非空disk均763键，完整语义精确为D15绑定起点`4e6bdf18cb128dc43d591ff2049862cdf9bfe7e81faf07a4a11c9b328f9012e3`；其它762键/类型/SHA全部保持。全部非空正文均成套47字段，字符串SHA=`b5e8db15e0d06ea6df9e6b7ce1570e4220a8e85da4ac0616a1ee2a76ad394929`；未见44字段切换、第三状态或字段混搭，实际时间/寿命/各视图单调合同无反证。
唯一episode：seq694，位于组231 post；之前合法非空seq693（12:11:17.203337–.203553Z），之后合法非空seq696。组长0.043680秒；`qualified=false`、`bracket_pairs=[]`。单个空样本不能测出真实空态持续时间，记录的0秒采样跨度不等于实际空态零时长。

| 顺序 | 实际读取UTC（2026-09-30） | 状态 |
|---|---|---|
| seq694 disk_before | 12:11:18.049239–12:11:18.049454Z | 42bytes，可解析且读取一致，0键；raw/semantic SHA=`9261ecceda608ef174256e5fdc774c1e6e3dcf533409c1bc393d490d01c713f1`；allowed=false |
| seq695 native | 12:11:18.049876–12:11:18.066290Z | 实际导出exit0、stderr0；763键、绑定完整语义、allowed=true |
| seq696 disk_after | 12:11:18.080273–12:11:18.080466Z | 114093bytes，763键，绑定完整语义、allowed=true，磁盘已恢复 |
实际观察为“空→原生完整→磁盘恢复”，缺少合同要求的“空→原生完整→磁盘仍空”；即使前后闭合、所有已采native合法、最终稳定和采样间隔合格，也不能替代这项条件。空态摘要因缺cache产生RuntimeError；无另行读取/解析错误。不可据此确认写入者、异步落盘机制或每一瞬间API状态。
`disk_empty_sequences=[694]`、`transient_sequences=[694]`；unexpected_content/invalid_native/sampling_gaps/process_errors/observation_errors均空。entry_verified、observation_ended、closed、sampling_contiguous均true，incomplete=false，但final_verified=false；完整记录失败后自然收尾，未恢复或写偏好。
退出后观察20.209763秒，所有owned退出后的保守共同稳定区间17.983924秒（≥10），额外子进程自然等待/确认0.044682秒已计墙钟；首次exit不重写。native_end seq727实际读取12:11:38.181708–12:11:38.195591Z，exit0/stderr0。随后最终编译快照仍采样，真正最后一次native为seq729（组242），12:11:45.797038–12:11:45.806298Z，exit0/stderr0、763键、同一绑定语义。
两次末端native均151847bytes、raw SHA=`a7d7d6c69d630e02a0723372d884317e709d6e169c36c804b40b0add86e8943a`。末disk seq730读取12:11:45.816046–12:11:45.816162Z，114093bytes、raw SHA=`0da05ecafb3a104c19d14678167e35b4b3ed924d7352b045d2c2bca6f0cd836e`；组242三视图完全一致。持久化仅名称/类型/SHA和结构化摘要，原偏好/URL/密钥/缓存值未记录或打印。

Compile PID83867自然exit0（12:03:48.711104–12:04:08.870853Z），tests PID83975自然exit0（12:04:12.459476–12:11:17.927147Z）。准确托管端口为56867/56975，分别6/210次活体已初始化lsof样本、跨度9.976928/418.628956秒，无无效查询或目标监听；两日志均Mono已初始化且无debugger-agent options，monitor_failure均null。
端口依据为冻结`VisualStudioIntegration.cs`（7617bytes，SHA=`7d50c7b932585a2bd68b6d5d228bac7281ab2bf900802e4ac8b1914914b65c60`）的`56000+(PID%1000)`。其它实际TCP端点原样留证：两进程均`*:34999/38000/38443/55000/55504`，另compile `127.0.0.1:51254`、tests `127.0.0.1:51311`；仅证明目标托管端口未监听，不分类其它服务。
10个记录子进程83879/83887/83925/83926/83927/83978/83983/84006/84007/84011均自然退出；wrapper末态及本C随后只读`/bin/ps -axo pid=,ppid=,comm=`（工具记录c0d95f、exit0）确认两Editor、上述子进程及其直接后代均无存活，亦无其它Unity；没有杀进程。
源码fingerprint=`d28ac0623c6eddb3374dcecf32fb9aed8f8b26f0d311ec5d9c9477c45294ce90`，106 DLL/PDB fingerprint=`27254b7c6b70e38bb4bf615f3e27cd9d0f11d0608b2a3eed9cd2394ce3664581`。before/compiled/after与绑定模式一致：DAG `Library/Bee/200b0aE.dag`、53rsp、4175输入、106产物、完整参数/输入/输出及NUnit原字节保持；全部Release `-optimize+`、`-debug:portable`并保留DEBUG/TRACE/UNITY_ASSERTIONS/UNITY_INCLUDE_TESTS。NUnit329728bytes、SHA=`e388e140954de12970384331d1730e95b4613ef53c3326b6f8fa0fffdbc9314f`。
自然I/O：CONT7063GUID/33002叶→7620/35594，新增557/2592（≤600/4096；总≤8192/40000）；Host185/1212→208/1362，新增23/150（≤30/512；总≤256/8192）。全部旧GUID和叶原字节保持，仅新增GUID；无清理。D1–D15全部工具/报告/失败/槽等保护清单前后完全一致，326199bytes、SHA=`ed356d1e1149c172199415753c102d59b2837034e15f470907997731b9f8d741`。SceneTemplate未出现，natural-cleanup events为空。
full06准确24个普通非链接叶，无额外文件。preferences.jsonl为78098582bytes/731条，每条≤512KiB，满足128MiB/1024；其它JSON/XML≤32MiB、日志≤128MiB。wrapper fresh已核实的完整源码/模式/历史/I/O未为报告再扫描；C只补证据统计/SHA和必要退出核对。

| full06原始证据 | bytes | SHA256 |
|---|---:|---|
| [after.json](unity-runs/full06/after.json) | 244081 | `adfc5eb1892a72dca0b39e2ba5b150fed6c9e7016fbf81b7855778a678603595` |
| [before.json](unity-runs/full06/before.json) | 207007 | `4b22748c323027e2c041025b765952153243669896881199fc2eba13fec3f0eb` |
| [compilation-after.json](unity-runs/full06/compilation-after.json) | 3882242 | `df1037ce18550b4f0358683de9f15f42c0f0695650a188594203907656ad1228` |
| [compilation-before.json](unity-runs/full06/compilation-before.json) | 3882161 | `061c2b96e75685f1a5b5124cc498f7f54790874f3a9d8c7cf527bf80c5bca39b` |
| [compilation-compiled.json](unity-runs/full06/compilation-compiled.json) | 3882239 | `1723fe73a253f06436054d9184ea207fc0a42bf49f858c2206d4157ae5d4d4c4` |
| [compile.log](unity-runs/full06/compile.log) | 59182 | `e57fb7fbe2c9cb492f30fff64a6049e4f57ff69f7eb5f0a3fb30bd8a4a6718fe` |
| [compile.stderr.txt](unity-runs/full06/compile.stderr.txt) | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| [compile.stdout.txt](unity-runs/full06/compile.stdout.txt) | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| [compiled.json](unity-runs/full06/compiled.json) | 35746 | `e35a9441c944e6359cd403943646ef188cfe4e7060a3e98cee00d92f4885a9b6` |
| [host-io-after.json](unity-runs/full06/host-io-after.json) | 501873 | `61ff65451d494e106cd4af8dd192e7efed797012487743f25581a0c4bd3b1ba4` |
| [host-io-before.json](unity-runs/full06/host-io-before.json) | 446603 | `9f2ac1975a04e6e12912b671da7f7acf8e69b271ad05df9ff1ad6721568537cd` |
| [io-after.json](unity-runs/full06/io-after.json) | 12927914 | `c215153689cea7ab09126b244bc069284e77c30763625f22b5c2a3fa5b46e23b` |
| [io-before.json](unity-runs/full06/io-before.json) | 11985513 | `bccae85a7aebfdaa841986a84e233a0b4449ffff185cb7f4d51ecd8e23e86458` |
| [named-tests.json](unity-runs/full06/named-tests.json) | 1052339 | `44dfd0fc1442622adba5864df3c89fc480463badb19f1c90c6b216906548875a` |
| [natural-cleanup.json](unity-runs/full06/natural-cleanup.json) | 19 | `1a46c0402c164e450fc729a327a7f36d64cf4e4425fc9fd7063a788623706f19` |
| [preferences.jsonl](unity-runs/full06/preferences.jsonl) | 78098582 | `33c58410ffd05ebc4d52b434887b0cb791d01d4f384d70b1dff7ee313e6bd0af` |
| [protection-after.json](unity-runs/full06/protection-after.json) | 326199 | `ed356d1e1149c172199415753c102d59b2837034e15f470907997731b9f8d741` |
| [protection-before.json](unity-runs/full06/protection-before.json) | 326199 | `ed356d1e1149c172199415753c102d59b2837034e15f470907997731b9f8d741` |
| [result.json](unity-runs/full06/result.json) | 868440 | `b051319cc115e6f5ddcc9d35a51b050a918f79c4233587cca5251d5e94df9c1a` |
| [run.json](unity-runs/full06/run.json) | 307269 | `fbb108f32d5457419706dd2121d0c5ef0fce157fb03c34f64357accb4721a0cd` |
| [tests.xml](unity-runs/full06/tests.xml) | 3102594 | `6fedc5e867bb752b92ef285c5e5f8431936a59cca6b82ab5021f182e162da9ac` |
| [unity.log](unity-runs/full06/unity.log) | 2099797 | `57e83a5f139d7e52231181b3b341cc8889571390d5b4c7f85906cdd6a94c2bb2` |
| [unity.stderr.txt](unity-runs/full06/unity.stderr.txt) | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |
| [unity.stdout.txt](unity-runs/full06/unity.stdout.txt) | 0 | `e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855` |

本C仅按签发创建full06及本报告，冻结六文件和原始24叶在报告落盘后再核身份；未改产品/测试/断言、模式/配置/引擎/包、偏好、既有证据或Git，未重跑、另建运行槽或启动新聊天/代理。D10/D14失败保持；主029仍暂停。C正式交回后停写，等待原R实际独立审查及SD00后续签发，不自行改变条件或追加全量。
