# FM-DEMO-024-C1 独立复审

VERDICT: ACCEPT

Scope: PASS
Acceptance criteria: PASS（C1-01～05）
Verification: 原生产下 9 项预期断言失败；修正后编译 exit 0、3198/3198 Passed、零失败／跳过；阶段实物、同版产物、归档和正式完成门独立核对通过。
Notes: 本结论关闭原 024 的 F01；原 024 其他已审通过且未变化的业务链沿原结论。保留本文末尾的 NOT RUN／NOT VERIFIED。

## 准确回合与正式接收

- C 任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1（FightMatch 本机保存与应用接入实现）；准确 turn：01a0cddf-4836-7ec0-8d7c-720950abc88c。
- C 原生 task_started：2026-09-23T10:45:57.458Z；turn_context：10:45:57.509Z，gpt-6-astra／max，cwd=D:/Unity/UnityProj/FightMatch。11:34:45.168Z 为同 turn 续接 context，不是另一个实施回合。
- C 非异步 formal final：2026-09-23T12:06:40.533Z，AgentMessage／final_answer、delivery=null；准确 task_complete：12:06:40.814Z。原生物理行分别为 9881、9885；开始和首 context 为 9003、9004。
- wait_threads 另核同一 turn completed、error=null、任务 idle；cursor=5c8a73c1-d962-408b-880b-35f970baaab2:12。工具省略 final 正文，因此按 §252 只读准确原生正式消息补核。未用旧 completed 或报告提前出现代替完成。
- 正式 final 对 024-C1、G2-RESUME2 分别声明作者 COMPLETED、停止修改，并列四份正式文件及 SHA；R 已对实物逐项核对。
- R 任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；准确 turn：01a0cde0-108c-71f1-b0e0-b501c8f4b87b。task_started=10:46:48.771Z，context=10:46:48.814Z，gpt-6-astra／max、同 cwd。R 自身完成事件须在本轮 final 后由 SD00 核，不提前声称完成。
- R 仅新建本报告及同批 G2-RESUME2 独立报告；未运行 Unity、项目测试或业务代码探针，未修实现、写 Git、启动代理／外部模型或发送协调通知。规范与规格两轴均由本任务独立审阅。

依据：[任务包 §249～253](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md)。签发 r125 全文 SHA256=9a6b3f58fdb3cb53bddedfa6cfe04f7902f7c8215c36d5a4b331848f219d650d；R 曾于 10:47:53.6922597Z 实读该签发版本。
最终只读核当前 r126 为 942250 字节、SHA256=74bc710d73f56df8bce36ddd61ed8fe89b40c0d104cfac4089294aa75bb7b14f；不冒称当前全文仍为 r125。
§249～252 规范化 SHA256=9d211baa68b0d09ca81f65fb073a136475b6711e606b33fdd3c5a81b4cc2f773，12:11:09Z 再核一致；CRLF→LF、TrimEnd＋LF、UTF8。旧 §240～243、§246 冻结也未变。
原 [024 独立报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-code-review.md) 的 F01／P2 及原 NEEDS_FIX 历史保留；原报告 SHA256=a3b77ef5dfce2a59ada4e47eb867dcf927192f9bc52c06657c78743517a55057，本轮未覆盖。

| 正式作者文件 | 实物长度 | SHA256 |
| --- | --- | --- |
| [C1 delivery](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-c1-delivery.md) | 67 行／7975 字节 | 117e396a71881f01ef24f51d0566b789057872343620b9b6388568b9e0ffe7c1 |
| [C1 scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-024-c1-scope.json) | 1144251 字节 | 078c0d12e662d7a12acbf84a7e30444c173c6ccad642788e211a342c8579a688 |
| [C1 late/bindings](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/late/bindings.json) | 6210 字节／21 项绑定 | 35f2dcdcb4cbead767a1754b3fe044b7a693de610ca6aee6f3227932c1acae19 |

以上在 G2 前于 11:08:10～16Z 冻结，最终 12:09:59Z 重核仍匹配。两份作者文件分别低于 180 行／3MiB 限额。

## Standards：范围、预算与继承

独立比较原阶段副本、C1 运行副本和当前实物，C1 仅五项既有文件变动，无新增／删除实现或 meta；其他 606 项不变。Core 改动用预期两处文本替换后作整文件字节比较，App 只在 Time 复制前增加两个检查，三测试原方法／断言／注册保持。

| 文件 | 实际增／删 | 独立范围结论 |
| --- | --- | --- |
| [CandidateRecoveryClock.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs:23) | +2/-2 | 仅 HasMonotonic、HasScope 从 internal 改 public；private setter、两个单调 setter、PrepareTime／Advance 其余字节保持；4 行达到且未超授权。 |
| [CandidateLifecyclePreparation.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/CandidateLifecyclePreparation.cs:120) | +2/-0 | Time 复制前检查提供标志；未增加第二套时间规则，2 行≤24。 |
| [PreparationTests](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateLifecyclePreparationTests.cs:104) | +28/-0 | 三项恢复缺字段回归及其输入构造。 |
| [RecoveryTests](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateLifecycleRecoveryTests.cs:98) | +45/-0 | 四种显式样本／调用者修改，两个 Retry／Resolve 回归。 |
| [EndTests](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateLifecycleEndTests.cs:126) | +20/-0 | 退出与胜利的六项缺字段回归。 |

三测试新增 93 行≤220，未删除旧测试。没有修改存储格式、依赖、领域枚举、018／019 门面或 Architecture；本包唯一 API 变化是两个既有只读属性的可见性。
C1 仍为 611 实现项、597 Scripts＋Tests、624 Assets、331 个唯一 GUID。完整清单 canonical 规则为 Ordinal path＋TAB＋小写 SHA＋LF：

| 阶段 | canonical SHA256 | 与前阶段的关系 |
| --- | --- | --- |
| 原 024／C1 入口 611 | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d | 原正式交付入口 |
| C1 冻结 611 | 41344696e520c8c0f379498d779831c48597fea696d059d3325298f6c9ec92ca | 五项修正，606 项保持 |
| G2 当前 637 | 14e2906fb75fd6b46cc4b174214d892caac6efea2afbaad792b7e7f2cf71ab8d | C1 中仅两项授权旧测试配置／谓词覆盖，另 609 项保持；新增 26 |

G2 覆盖仅 FightMatch.Core.Tests.asmdef 与 QFrameworkIntegrationTests.cs；五个 C1 修正文件在 G2 中均冻结。C1 的证明取自当时 611 项及对应 DLL／XML，不错误要求最终 G2 的 DLL 或两项旧测试 SHA 仍等于 C1。
全组对原 019 的 589 项只有 Architecture、CandidateRecoveryClock、Core.Tests.asmdef、QFrameworkIntegrationTests 四项变化，585 项保持、新增 48；C1 的 App 与三测试原本属于 024 新增，未误计为 019 旧项污染。
两份新增取证 helper 共 157 行≤320，原 184 行 helper 只读复用；G2 没有返修 C1 helper。R 最终重核 run.ps1／finalize.ps1 的 SHA 与阶段冻结一致。

## Spec：F01 闭合与逐项验收

| ID | 独立结论与证据 |
| --- | --- |
| C1-01 | PASS。恢复 Prepare、退出 Prepare、PrepareVictory 三入口×双省略／仅省略 elapsed／仅省略 scope 共九例，分别断言 MissingField 和具体 FieldPath、无 Request、头不发布、存储调用及磁盘不变。省略不再经复制被改成显式 null；原缺项校验顺序保持。 |
| C1-02 | PASS。通过公开链得到真实未完成恢复期；双 null、零、同单调域、换域与原 CandidateRecoveryClock.Advance 对照。显式 null 仍是合法无单调样本；没有构造内部成功快照。 |
| C1-03 | PASS。四种合法 Prepare 后修改调用者 time 均不改变冻结样本。两个显式 null 样本在实际 SaveFailed→Retry、CommitUnknown→Resolve 中保持 operation／commit 身份和 canonical bytes，恢复累计 10ms、marker 一次及 Restore 结果正确。未将两个例子扩大为完整故障矩阵。 |
| C1-04 | PASS。原 3183 fullname 的 Ordinal 多重集合及旧方法／断言保留；新增 15 项均 Passed。五文件、行预算、611 项和 GUID 均由 R 核实际内容。 |
| C1-05 | PASS。红阶段使用原生产 SHA，九例实际失败均为预期 MissingField／实际 Prepared，并非编译失败。修改后编译 exit 0、无 filter 全量 3198 Passed；真实 Process.ExitCode、日志、XML、源码与 DLL/PDB 同版。 |

未发现 F01 修正引入的未解决实质问题。原 024 中与 F01 无关且未变化的六类业务链沿原独立审查，不重复审出一个新版本或重跑旧矩阵。

## 真实验证与同版绑定

以下均为 C 实际运行、R 独立读取证据；R 没有再开 Unity。固定 Unity 2022.3.18f1，exe SHA256=ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895。
每次预检同项目无其他 Unity，Start-Process 使用 Hidden、PassThru；记录实际 argv／cwd／PID／UTC／Process.ExitCode。编译带 -batchmode -nographics -quit；测试带 -batchmode -nographics -runTests -testPlatform EditMode、不带 -quit，projectPath 为本项目。

| 运行／证据 | PID | 开始→结束 UTC（2026-09-23） | 真实结果 |
| --- | --- | --- | --- |
| [red-01](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/runs/red-01/start.json) | 37844 | 10:52:59.2221813→10:53:44.0271770 | exit 2；9/9 预期断言失败、0 跳过；仅该轮 testFilter=F01_ |
| [compile-01](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/runs/compile-01/start.json) | 35244 | 10:55:21.3766754→10:55:32.6491746 | exit 0；无编译错误 |
| [tests-01](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/runs/tests-01/start.json) | 37160 | 10:59:14.3389312→11:03:13.2834400 | exit 0；3198/3198 Passed，0 Failed／Skipped，227.5369695 秒 |

红阶段 Core SHA=86d8920bb392df984fac0d55294fa93fee736a7f81f4a79c3ef4927b724c954a，App SHA=0d2eae39698b077886e7ee40e8e4f36e473079a05c7e0ffbe6a0cc7ac75e1685；与红运行前后实物相符。
红 XML SHA256=a53f1b349a22a8dbb5aaaa3f1bdb1e3d5f4d02d7a7700390e7326308fba99033。wrapper 曾错误按精确 Failed 匹配 NUnit 聚合 Failed(Child)，在真实证据保存后报错；原异常及 runner-used 保留，后续修取证匹配，没有重复红轮或掩盖 Unity exit 2。
[绿色 XML](D:/Unity/UnityProj/FightMatch/FMDemoB16C1-EditMode.xml) 为 2243370 字节，SHA256=048b07ab23921877ccd07c045ad719b10294a598cca7b81485aa1a51c2cc324b。
原 3183 具名集合 SHA=ab16984d1358fac676a6b770d1de757bccadba72eb3bcee5a20a25387eb56caa；3198 集合 SHA=12d3219ce57d7b46fbfca87bc417d9dc735acaefd19da76851aa56e03b50ddcd；逐名比较无旧项缺失。
编译后、测试前、测试后的 30 份 DLL/PDB canonical 均为 5853aa03a8a663ffcc86858c7d0a58ee63a9612c347657e522fe21eb21b9a478。
R 逐一哈希入口 910 份实际副本及三运行六个 before／after 的各 910 份副本；实现、Assets、保护输入、DLL/PDB 清单、路径／长度／SHA 和副本集合均一致，含 Tools；没有用清单声称不存在的副本。

## 归档、历史保全与剩余边界

C1 新归档独立实物核验：9580 文件、661141059 字节、33 个空目录；source-before／payload／ZIP／source-after 路径、长度、SHA 及空目录集合一致，打包前后源元数据保持。
canonical SHA256=1e22bbe553b5a2902642cb48c1d2d936747818d08d827539970c69c1331309f2。
[ZIP](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/stage024-c1/archive/FMDemoB16-024-c1.zip) 为 114764958 字节，SHA256=0c2ea77d5dca8be9023fdb65a76202aaed1a3488b04acd26b7065007bc0a0549。
archive／late／两正式报告的自引用排除由 21 项 late/bindings 逐项实物绑定；binding 自身 SHA 与 C formal final 一致。原生导出 30 条 CommandExecution／FileChange 的物理 ordinal、时间及内容与准确 C 回合逐项相符，未将 Reasoning 当证据。
C1 自然新根为 B12/f43c890bce55486c998d6ad1009e1950、B12/f72b8bb884d84ad1a7cb721f9b5b801c、B13/1593322f176540b4b17c272db4901c56，均完整纳入归档。
R 独立入口 10:49:28.6191218Z 至最终 12:09:06.0564942Z 的 58 个历史根／239440 文件／14103273130 字节，其路径＋长度＋LastWriteTimeUtc 逐根摘要全等；明确排除两个新阶段子树。按 §251 没有重新全字节审计历史 13.6GB。
R 另逐字节复核 487 项保护输入，均保持；原两份 R 报告、作者历史报告、旧 G1 与旧证据未覆盖。git --no-optional-locks diff --check 退出 0；仓库既有 LF/CRLF 提示未改写文件。

NOT RUN／NOT VERIFIED：原 019 与六进程／强杀矩阵未另行重跑；正式内容与 PlayerSave 来源；合法 AwaitLinks／全倒补线公开成功见证；未覆盖的保存故障交叉组合；交互式 Editor／PlayMode、绘制像素、物理鼠标／触控；025／027／028／029 和完整 Demo；§184 历史时间／stderr 缺证。
以上是任务包保留边界，不被本包的时间修正或全量通过消除。本报告自身 SHA 由本轮 R 正式 final 给出，避免自引用。
