# FM-DEMO-007A 实现交付

状态：WORKER_RETURNED，作者验证完成，等待 R 按 §82 独立代码审查；本报告不作独立 ACCEPT。
批次：DEMO-B02；实现任务 A：01a0c1fb-248d-7321-8b69-9efd26b817f1。
本次 turn：01a0c1fb-26b8-7fb1-abbb-809699906678；交回日期：2026-09-21。
范围依据：现行 system-task-packets.md §77、80～82，技术范围冻结为 §81。

实现唯一办理入口 `BattleEntryPreparer.PrepareCandidate(BattleEntryInput, ExactMathBudget)`。
输入保留普通可变数据；成功才深复制为只读 `PreparedBattleEntry`。失败返回规定的六类原因及完整 FieldPath，Entry 为 null。
按根元数据、Context、Level/Face/Pair/Enemy、Members、Ready 门槛顺序验证；输入 null/budget null 抛 ArgumentNullException。
共享本次 ExactMathBudget，调用已接收整数位数检查和精确比较；预算异常原样传播。不会重新成长、缩放敌人或抽样。
零值可合法的原槽、行动序、颜色、端点、IsReady 及 Charge 的可空字段记录是否显式赋值，避免缺项被默认值接收。
输出仅含候选资料，无 SceneRevision、Phase、Random、EntryBaseline、BattleSnapshot 或真实提交能力。

## 七项验收

下列 PASS 仅指作者本次实际验证，独立结论仍归 R。测试均在 BattleEntryPreparerTests 中。

| §81 项 | 作者验证 | 实际见证 |
| --- | --- | --- |
| ①固定资料接入 | PASS | SourceCandidates 四例通过：006 CaptureSource 的 L1/L3 × C↑/C↓；直接使用已转换端点，保留 A/B、E01/E02、原槽与行动序。W1=100/20/10/6、range1；E01=15/10/0/0、Strike 3/5；E02=20/10/20/0、Charge→Strike 13/10。p=1/5、C=1/4、倍率3/2；SourceNotes 明确合成 C 和未审坐标条件。 |
| ②结构和映射拒绝 | PASS | 重复面/Pair/color/实例/原槽/行动序、重叠/同端/越界、空面/非法尺寸及必填对象/列表/数值均逐例检查原因和字段。独立双面可各含局部 A；2×2 交叉端点构造仍按结构接收，不求完整解。 |
| ③精确入场与职责 | PASS | ComputedW4 保留 124、118/5、29/2、21/2；推荐等级变化不改敌人。2^80+1 及相邻等级/修订精确保留；四处 BigInteger、21 处有理字段分别重验分子/分母位数。合法域边界、非法域和较小预算均有见证。 |
| ④队伍和支持域 | PASS | 唯一 Ready 战士 slot2 不补槽；空队/不可出战为 NoReadyMember；重复角色/职业/槽拒绝。主动、未知功能/枚举、非空携带、非零闪避及多战士明确拒绝。 |
| ⑤候选与依据 | PASS | 根/属性 Context 的每个字段、修订、SourceNotes 数量/内容/顺序及大小写差异均拒绝；字符串不 trim。完整候选可 Prepared，不核验或伪造 L3 已开放，不产生 Published/持久提交依据。 |
| ⑥深隔离与原子性 | PASS | 修改成员、Stats、Context 列表、Faces/Pairs/意图后两份旧输出及下一独立输出不变；递归验证只读属性、非公共构造器及集合写入拒绝。所有拒绝见证比较输入前后；共享步骤预算不足无结果，更大新预算重试完全相同。 |
| ⑦验证与范围 | PASS | 编译 exit0；全量 EditMode exit0，851/851；新增178，原673按完整名称和出现次数全过。现有309文件SHA保持，10份新实施文件1516行，5份Unity生成meta的GUID全Assets唯一。 |

## 实际变更与摘要

仅新建下表10份实施文件、本文和 demo-007a-scope.json。没有修改旧 C#/测试/asmdef/meta、共同/details/协调稿、场景、配表、依赖、设置、权限或 Git。
工作区已有改动保留；不把原有34份 tracked diff 归于本包。所有新实施文件已另行全文检查。

| 新建路径 | 行数 | SHA256 |
| --- | ---: | --- |
| Assets/Scripts/FightMatch/Core/BattleEntryInput.cs | 139 | 458d6266a155018c69e9a52ed40145c23335953f43e68dd9bf089eee35ee67c4 |
| Assets/Scripts/FightMatch/Core/BattleEntryInput.cs.meta | 11 | 4313bde049fd47db566ee84d49e1485ed1e94cb44c192c4de1e78fe6f625be76 |
| Assets/Scripts/FightMatch/Core/PreparedBattleEntry.cs | 221 | 5b7f482f98b8815bb0bbf23eacc45dd6154567dbeebb014d08548b4674c815b2 |
| Assets/Scripts/FightMatch/Core/PreparedBattleEntry.cs.meta | 11 | f4c0c235d2b06b54eb46766375c582cf99bff004f3f300f8d778b0d60cbb8014 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs | 267 | d641bb55773f3e4e93e0c15d98e24de464dd33a6e73e47683c3d57c7dbec21b2 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs.meta | 11 | ca86d300d580456446dc5c7446bf32b48d69d41b881cf28f212d5f1ae5ace57c |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparationResult.cs | 27 | 7d1f497cbf7827cbcbc42a6afbd30ecffb63b80122fe691893370c0115aa29b1 |
| Assets/Scripts/FightMatch/Core/BattleEntryPreparationResult.cs.meta | 11 | 2e7c7dae2c1d7bbe1f384c11a4712a89565ea4c243f90655732231c081982064 |
| Assets/Tests/EditMode/FightMatch/BattleEntryPreparerTests.cs | 807 | 343d65177d362f1f94d25fbaf210133cbe3cf2f364b48c7b63773f13ecc43574 |
| Assets/Tests/EditMode/FightMatch/BattleEntryPreparerTests.cs.meta | 11 | bea156c01d50243cddf07175b88f77a701886cc30c8e616c1c477420191a17ef |

合计1516行≤1600；新meta GUID依次为0844180a3a26e9749b891967cad240a4、e0a7c085df6f99b4c8035dfd105b43c2、80534e4c30161b14f8e55147bb177980、139d32a461eff784b9565aa5c09ef1dc、5646c1dc66bb5024790a10b7b4373a21，各在全部Assets中出现一次。

## 实际命令与运行证据

每次启动前核对 Unity.exe 存在及 Unity 进程数为0；未杀进程。版本为2022.3.18f1 (d29bea25151d)。
实际通过 Start-Process -WindowStyle Hidden -PassThru 启动以下等价参数，再 WaitForExit、Refresh、读取 ExitCode；编译/测试严格串行。

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
& $env:UNITY_EXE -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo007ACompile.log'
& $env:UNITY_EXE -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemo007A-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo007ATests.log'
```

RUN 编译：UTC 03:46:32.3596039～03:46:44.6440909，PID40336，exit0；日志含“Exiting batchmode successfully now!”。
RUN EditMode：UTC 03:47:20.9961614～03:47:31.8646768，PID42408，exit0；XML执行区间03:47:28Z～03:47:30Z，851通过/0失败/0跳过，新增178实际执行且通过。
两份最终日志的 error CS / Compilation failed / Unhandled Exception 匹配数均0；已实读日志和XML，不以文件存在作为通过。
许可证恢复前两次编译启动为exit199（IPC超时60.01秒）和exit1（无有效Editor许可证），均在导入前退出。用户明确恢复后继续，第三次成功；没有代码修改或测试重跑。
失败启动的时刻/退出码/当时日志SHA保留在scope.after.validation.compileAttempts；专属编译日志最终由Unity写为成功运行内容，未手写运行产物。

| 运行产物 | SHA256 |
| --- | --- |
| Logs/FMDemo007ACompile.log | 009b1b1bdfe910e398e7e29b61120b033fe0886928a5e3da6edda7d8719e3c30 |
| Logs/FMDemo007ATests.log | bb5c113d891f7694bdafe69a53f490e8c140289685223ec301a9542f2b2175f2 |
| FMDemo007A-EditMode.xml | 631a94ee9347188c8e7bb1e645653121a08d4b54725dbaa2aee2285146591492 |
| 原回归基线 FMDemo006-EditMode.xml | c48ed5ac221c8effdcf14a29caa9f662137a341c653628e4305a181d379a9c49 |

原673条包含670个不同fullname，其中 Validate_EndpointMismatch_PrecedesCellErrors 的同一完整名称出现4次。按Ordinal完整名称分别统计before/after/Passed出现次数，670组逐项完全相等，缺失/次数变化/非通过均0；新名称全部属于BattleEntryPreparerTests。
原名称+TAB+次数+LF（Ordinal排序、UTF-8无BOM）的两侧SHA均为21bfa132e4ea736cf2fbc291b5d3c02686eda3be99b7de3efc538e2c932829e1；逐名称见证已存scope.after.validation.regressionByFullname。

## 范围清单与交回边界

before于UTC03:23:55.9479505采集309份文件，且当时12个获准新路径均不存在，11份原asmdef摘要已在工具输出记录。
before规范序列SHA：ea20d11dd44a2486d3902e1225387c2acc623e9bf8b86404da36e1b10c341e4a；该段原文本保持，只更新after。
after于UTC03:51:06.0641566采集319份文件；旧309份完全相同，新增恰10份，规范序列SHA：ba26d102e53096c71e95d0665d964371f7a5952829be58c1485601054a8b543c。
全部最终源码/测试/asmdef/meta输入SHA及获准文档/依赖输入SHA均在scope；五份C#在编译前、测试后及交回时摘要相同。
demo-007a-scope.json为267457字节≤300KiB，最终SHA256：da9770fd250339b02676eb881219923a3d91611abf04a02d942add378669c8e6。
RUN git只读status、diff --stat、diff --name-only、diff --check；check exit0。五份新C#无行尾空白/冲突标记；Unity生成meta未手改。
未运行Player构建、PlayMode、真机、真实玩家入场、正式随机回放或发布；这些不在007A范围。未开始007B/008，未创建任务/子代理或进行任何Git写入。
本次交回后停止代码修改，等待R独立审查；需要修正时由后续同批窄包安排。
