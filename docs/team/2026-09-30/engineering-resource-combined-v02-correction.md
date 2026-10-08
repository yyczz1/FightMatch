# RES-COMBINED-V02：诊断通道环境小纠正

2026-10-08，`DESIGN_ONLY_NOT_ACTIVATED`。中央唯一收件；只读核证并修改本文，无Unity/可执行文件/ps/测试/Git。本版替换先前IPC身份账本方案：优先在本轮headless I/T子进程环境禁用CoreCLR诊断，保持未知IPC失败规则，仅另修CacheWrite分类。native仍须另签。

## 1. 原失败及固定运行时

V01/run的FIX02 runner为81937B／SHA256 `1a30b4c6e1a5807664d047635c4fc20b131fe9d15191b128656dc366612aa7ba`，PR20已审head `5f43b7a8cdff0795ad91b342deb5d566be38da0f`。C actual `01a118bd-3ba0-7f31-843e-bcb2a6d00175`：I一次OS exit0，监护失败、runner exit1，T=0；原receipt75924B／`4acab85e99917971de6b768cae12bd3601a39e3fbce5db19a40b30c4e89748d0`。外部87.088998＋准备160＝247.088998秒。原FAILED及全部证据保持。

首错为TMP下`dotnet-diagnostic-54204-1791416511-socket`；owned54204是本轮Unity.Licensing.Client。另有dotnet/csc socket、ILPP的FIFO可执行文件遗漏，以及短命/闭合阶段FIFO的owner存活与原ppid检查失败。54290为VBCSCompiler；不能仅凭文件名追认归属。[QA回执](testing-resource-combined-v01-native-receipt.json)保留全部错误；最终owned0，TERM仅54290一次，43compiler、21同步源及2暂移源完整恢复无冲突。

只读核固定Intel Editor的Contents目录（Unity2022.3.18f1不变）：

| 组件／libcoreclr.dylib相对位置 | 包内版本依据 | 字节／SHA256 |
| --- | --- | --- |
| `NetCoreRuntime/shared/Microsoft.NETCore.App/6.0.21/` | 已安装共享框架目录6.0.21 | 7180656／`98b6d08ed4c6e5f19ab798879f72832a7dd08b2543e0089bd005ffeeebc2f4fb` |
| `Tools/ilpp/Unity.ILPP.Runner/` | runtimeconfig：net6.0，includedFramework6.0.11 | 7148944／`bfa550f36240079dbf9fab8498b4332175ff7653170651edf83603c6c7648271` |
| `Frameworks/UnityLicensingClient.app/Contents/MacOS/` | runtimeconfig：net7.0，includedFramework7.0.10 | 6992080／`ec3e5864627b34a8fbd7022ad1197c9e6d7fc4526939a1dba6c73dda8b72cb4c` |

三个实际运行库均含`EnableDiagnostics`、`DOTNET_`及`COMPlus_`字符串。版本来自包内目录/配置，未运行--info；字符串核验不是开关已生效的证明。

## 2. 优先最小修复

微软[诊断配置](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/debugging-profiling)定义`DOTNET_EnableDiagnostics=0`关闭debugger、profiler和EventPipe；[环境变量文档](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables#dotnet_enablediagnostics)说明它禁用Diagnostic Port且不能被其它诊断设置覆盖。固定组件均为.NET6/7，使用总开关，不选.NET8才新增的`DOTNET_EnableDiagnostics_IPC`等分项。本次未发现需要旧COMPlus兼容项的运行时。

**唯一新环境项：I/T均显式`DOTNET_EnableDiagnostics=0`。** 在启动Editor的Popen子进程env中设置，与现有UPM_CACHE_ROOT、TMPDIR、BEE_CACHE_DIRECTORY及T专用AS根共同封签；检查实际传入Popen的精确映射，不能只写activation或依赖父会话环境。不改全局shell、用户环境、runtimeconfig、二进制、产品或Unity版本。接受这一轮无法附加外部CoreCLR调试/分析工具；保留普通stdout、Editor.log及UTF XML，它们仍是既定验收产物。

这是根据官方开关语义及本机组件匹配选择的优先方案，预期避免诊断socket/debug FIFO；尚未证明Unity所有子进程均继承并采用它，许可、ILPP、Roslyn和T实际成功也未验证。若原IPC仍出现，原守卫继续失败并保留证据；不自动重试、改用COMPlus或切换IPC放行方案。

**不改tree_entries/owned生命周期、未知特殊条目和链接判定，不新增IPC账本或任何类型豁免。** 一并修本次日志直接暴露的Csc分类：四SDK各有一条实际动作，紧接同名`[CacheWrite ]`遥测（I/editor.log254–272行）。当前matcher把两条都计入matches，`len==1`会拒绝四SDK。仅识别精确CacheWrite为关联遥测；有效Csc动作仍须匹配节点/输出及原完整来源链。CacheRead、未知格式和真实双动作不冒充actualCsc，不按程序集名简单去重。

## 3. 旧I不能关闭编译门

保留可复用的输入/包身份、保护/恢复事实、12程序集Csc日志事实、43个compiler后像及374个引用前像。原日志未见C#编译错误，OS exit0保留。

但监护门在compile_evidence前抛出：compile.json的stages为空、fullImportedInputs=null，before.compileBindings为空。43归档图/输出未封存该阶段完整rsp/rsp2/附加输入与最终ScriptAssemblies绑定；P已恢复，晚取DLL/响应文件不能补成编译时证明。本次不追认PASS或verifiedCacheChain，不做长时间穷举补证。仍需新一次I＋一次89项T，原T从未执行。

## 4. 实施白名单、精确回放与新验证

新E2=`TestArtifacts/FightMatch/RES-COMBINED-V02/run`，BC2=同包`bee-cache`，AS2=`TestArtifacts/FightMatch/RES-D-ACTIVATION-001/RES-COMBINED-V02/state-tests`，TMP2=`/private/tmp/fm-rcv2.XXXXXXXX`。均须新空且逐级无链接；所有V01路径只读保留，不改名复用。

实施仅准E2的`runner.py,replay-check.py,replay-results.json,inputs.json,preparation.json`；另签激活后准activation及原有限证据/transfer派生槽。复用FIX02源码骨架、共享1036/P前像1035/导入1037、17覆盖＋4新增＋2暂移、43compiler前像、53包图及89清单；启动前按原保护合同核仍一致。数据只更新packet/owner/actual、新根与argv/transfer目的地、I/T环境精确映射及新source/data/replay/review seals；记录上述运行库身份，原fixedReferences保留V01来源。撤回此前“三runtime身份/IPC规则”扩展，不改产品、P/K/SDK/meta或43路径设计。

离线回放调用实际修改函数、模拟Popen，不启Unity：①原环境缺总开关的红例；②I与T实际Popen均收到字符串0，覆盖父env中的1；③其它环境和T专用AS精确不变；④诊断socket、遗漏ILPP FIFO等原非法输入仍拒绝，证明未新增豁免；⑤用原四组Csc＋CacheWrite片段每组认一动作，真实双Csc/CacheRead/未知格式不给通过；⑥保留既有source/data、43恢复、89与绝对闭合回放。先红后绿，最多2轮累计30秒，不新增IPC生命周期测试矩阵或重复审查代理。

新差量须GitHub准确head审查后中央fresh激活，C仍唯一执行。沿FIX02上限：准备160、I360、T180、各自然/TERM闭合60＋30、恢复60、末证据30秒，总机械**仍≤900秒**；保留剩余预算门，probe/审批/信号共享绝对期限，0重试。I通过闭合才T，原89过滤、真实AS两种symlink断言及空间限制不变。日志/XML/来源链缺失或任何监护失败均不能通过；不追加全量、Host/Android、包下载或缓存清理。中央唯一收件，主测试独立验收。
