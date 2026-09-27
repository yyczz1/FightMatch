# FM-DEMO-018C-2-C1 交付报告

状态：COMPLETED；仅纠正 F1，等待 R 独立复审，不自判 ACCEPT。停止源码和正式报告修改；018 整体与 019 仍按独立接收门执行。

## 身份与原起点

- 原实现任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确 C1 turn：01a0ccef-5371-7bd1-a243-d8de624637e7。
- 原生 task_started：2026-09-23T06:23:51.711Z；实际 turn_context 为 gpt-6-astra／max，cwd 为 D:/Unity/UnityProj/FightMatch。
- 按 [system-task-packets.md](system-task-packets.md) §226～228 正式派发执行；r114 签发全文 SHA256：06b2887830177d8800ba156acc2c4c11823975cef8646e120792964133f1d717。
- 冻结 §226～228（LF、TrimEnd、单末尾 LF）SHA256：77f6be9d6fc36d60e987ab5f2faeaa1913acedb0cbb0a9e864ad4471e0466bda；§209～212／§217／§224 的原冻结文本及 SHA 保持。
- C2 正式起点 569 项、canonical e5eb7e40fb8beb38230af4c758c19c826f4671155756b02e9d4eb1fc578bbc69、原 capturedAtUtc 2026-09-23T03:05:13.7509389Z 完整保留；C1 实际入口捕获为 2026-09-23T06:31:51.2059346Z，不覆盖原时间或更早 550 历史。
- C2 两报告、3068 XML、最终绑定及 R1 报告入口／出口 SHA 全符；R1 仍是唯一 NEEDS_FIX／F1，不倒填原 C2 为 ACCEPT。
- 固定 E1：TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/；未另建任务、代理或证据根。

## 两文件限定修正

- FightMatchDemoArchitecture.cs：只增 3 行。私有 deinitialized 标记只在 runtime.Close() 正常返回后设置；旧实例再次 OnDeinit 明确抛 InvalidOperationException("AlreadyDeinitialized")，阻止上游清空当前实例。
- 第一次关闭仍进入原 Runtime 的 WrongThread／Busy 检查。Close 首次抛错时标记不设置，原异常和 CloseFailed 诊断保留，owner 线程仍能再次完成上游收尾。
- CandidateApplicationRuntimeTests.cs：只增指定 1 项测试，共 48 行。真实 A 保存并关闭→B 从同 profile 打开→重复关闭 A 拒绝；B 的 Interface／Model／System／View 引用及租约保持，查询和 Enter 可用；B 关闭后同 profile 再开成功。
- A、B 和最后重开实例均由持有真实实例的 using/finally 收尾；红阶段断言失败也关闭 B，无反射或伪 Architecture。
- 合计 **51 新增＋0 删除**，低于 100 行；原 49 项测试方法／断言／名称不变。完整 [两文件差异](../../../TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/two-file.patch)。

## C1-01～05 验收证据

| 验收项 | 已执行结果 |
| --- | --- |
| C1-01 | 只加指定测试，生产仍为 a7ee449c612e73f343e6345e88389b686600fe83270ef699e704d7909a05215f。真实单项运行 total1／failed1／skip0，Unity exit2；断言为 Expected InvalidOperationException、But was null，正是旧 A 未拒绝。失败 XML／日志／红阶段源码与 DLL 全保留。 |
| C1-02 | 同一新增测试在最终全量中 Passed；明确核 A 拒绝、B 身份与引用保持、租约仍阻止第二 writer、原结果可查、新 Enter 可写、关闭后重开。 |
| C1-03 | 原 WrongThread、事件重入 Busy、CloseFailed 后再次收尾、跨实例 pending 恢复与事件测试全文保持，均 Passed；未吞异常、End pending 或修改上游 QFramework。 |
| C1-04 | 原 3068 fullname 的 Ordinal 多重集合逐个保留且 Passed，新增 1 项，合计 **3069 Passed／0 Failed／0 Skipped**；另 567 实施项、全部 meta／GUID 和保护输入保持。 |
| C1-05 | 修复后编译与无 filter 全量 EditMode 均实际 exit0；最终源码、30 DLL/PDB 在编译后／测试前后／当前一致；完整新归档与两报告绑定准确 C1 turn。 |

## 实际命令与进程

固定 exe：D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，SHA256 ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895；cwd／projectPath：D:/Unity/UnityProj/FightMatch。
三轮均先核无 Unity 冲突，再 Start-Process Hidden／PassThru；WaitForExit 后先保存真实 Process.ExitCode，再收输出。完整 argv、PID、起止 UTC、stdout／stderr、源码／DLL 快照保存在 E1/runs 对应目录。

| 阶段 | PID | 实际 Unity exit | 验证参数 |
| --- | --- | --- | --- |
| red-01 | 32288 | 2，预期红 | -batchmode -nographics -runTests -testPlatform EditMode -testFilter FightMatch.Core.Tests.CandidateApplicationRuntimeTests.A08_StaleArchitectureDeinitCannotDetachCurrentInstance -testResults E1/red/EditMode.xml -projectPath 固定项目 -logFile E1/red/Unity.log；实际记录均为完整绝对路径。 |
| compile-01 | 14512 | 0 | -batchmode -nographics -quit -projectPath 固定项目 -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2C1Compile.log。 |
| tests-01 | 14944 | 0 | -batchmode -nographics -runTests -testPlatform EditMode -testResults D:/Unity/UnityProj/FightMatch/FMDemoB14C2C1-EditMode.xml -projectPath 固定项目 -logFile D:/Unity/UnityProj/FightMatch/Logs/FMDemoB14C2C1Tests.log；无 filter、-quit 或历史专用参数。 |

只有上述三轮 Unity；红运行包装器因正确复现返回 0，不冒作 Unity exit0。最终 [逐项测试登记](../../../TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/test-verification.json) 保存所有 3068 原项及新增项。
两次静态收证失败另保留：首次路径选择过宽，把其他 Application 文件计入行数；修正字符串构造时发生 ParserError，未进入执行。后按精确八文件选择通过，未为此修改源码或重跑 Unity。见 E1/verification-attempt01.json、verification-attempt02.json 和 final-verification.json。报告构造工具另一次 JavaScript 解析失败未进入文件写入，记录于 late/report-builder-attempt01.json。

## 范围、保全及完整归档

- 569 实施／555 Scripts＋Tests／582 Assets／310 唯一 GUID 不变，零新增源码／meta／程序集。五生产 914 行、八 C# 2373 行、取证帮助 82 行。
- 224 项保护输入前后逐项 SHA 保持。旧两目标起止 SHA、完整 569 起点和最终清单在 scope 直接保存，其他 567 项逐字节保持。
- 最终源码 canonical：5da1c80cde84ca32af8be3c3b43eff7ee0c02b98a476e5953b26d9040c00f3a5。
- 30 DLL/PDB canonical：b0d57dd23175a74d3969246db04a51e9eff7d24d7a866ad9f42269ec69cc8bcb。
- 红 XML SHA256：7c746e6673a0be86d5d202646158619c06d08f2748430b614ceef1908c6f246c；最终 XML：9f2b7b054e284d82f00cccc1eb7ae17b892800c6d689005f8d945173a30f2bbd。
- 原 **161,718** 个证据文件入口／出口独立枚举路径、长度、mtime 全部一致；其中 161,717 项入口沿 R1 已审字节记录，唯一 C2 archive/registry.json.br 因旧行无 mtime 重新核 SHA，相符。没有未知旧路径或无法解释的变化。
- 更早 130,077 项完整字节审核与 C2 新证据审核明确复用 R1／原注册表；没有把本次元数据保全称为重新全树 SHA，也没有复制旧证据树进 E1。
- 红／绿普通测试自然产生三个新 B12／B13 Guid 根，共 **1,432 文件**，全部登记并收档。
- 实际源／payload／ZIP 独立枚举并逐项核路径、长度、SHA及空目录：**8,173 文件／523,249,106 字节／2 空目录，零遗漏、额外、重复、字节差异**；归档前后源稳定。
- 三方 canonical：ec0c7bfa86b4e342b45134848363589f86bb6af2c977080067898526ad8ac31d。
- ZIP：E1/archive/evidence.zip，95,644,746 字节，SHA256 bcb18f6bd9f71f7c57630bdb5ad34901ad073f73b54b4d474c1197fe0597871a。
- [完整 scope](demo-018c2-c1-scope.json)：394,898 字节，SHA256 85934bc1127ee8f6246786f7e70f45d6d49ef6908421276221769cdf581b1c78。
- [完整外置注册表索引](../../../TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/archive/registry-index.json) 明确引用所有完整 JSON 清单，不裁行。旧证据入口／出口、Assets／GUID／保护输入、测试和归档清单均可直接重核。
- archive／late 子树及两正式报告明确排除自引用；[最终绑定](../../../TestArtifacts/FMDemoB14C2C1/9152215a842c4d908bf25ed0cedb2dae/late/formal-bindings.json) 绑定 ZIP、索引、报告 SHA 和准确原生 CommandExecution／FileChange。不导出推理，不用 chunk_id 代替 Unity 退出码。
- Git 四项只读检查 exit0，未执行任何 Git 写入；三协调稿未修改，冻结条款末次核验保持。

## NOT RUN 与完成门

NOT RUN：旧 C-1 六进程／既有强杀矩阵重跑、新强杀／断电矩阵、交互式 Editor、PlayMode Domain Reload 开关、真实触控、019／024／025／026-G2／027／028／029、场景、正式 PlayerSave／发布及 Demo 体验。未新增工程、SDK／依赖、外部模型、子代理或任务间通知。
本回归只证明实际 EditMode 进程内实例和真实 M12 文件租约顺序，不称新跨进程／PlayMode 验证或 Demo 已可玩。
本 formal final 后停止源码和正式报告修改；R 须核本次准确 C1 turn 的 formal final＋task_complete＋两报告，独立裁决后由 SD00 接收；后续包不自动放行。
