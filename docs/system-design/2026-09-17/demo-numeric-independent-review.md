# FM-DEMO-002005-R1 · 数值代码独立复核

日期：2026-09-21（UTC+8）；对象：当前工作区已合并的 FM-DEMO-002～005，按 002→003→004→005 复核。
执行者此前只实现 FM-DEMO-001，未参与本次四包编码。本轮实读全部现有核心源码、测试、许可证、asmdef、meta、指定契约及实际日志/XML，不采用 SD00 历史自检 ACCEPT 作为结论。
授权与范围：[任务包 §75](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md)；唯一写入为本报告，源码、测试、meta、协调稿、依赖、日志/XML及 Git 均只读。

**当前数值链总接收意见：ACCEPT。** 在四个原包限定的纯计算范围内，未发现必须修正的问题；无待修文件或必要的新增复现包。正式协调状态由 SD00 核对本报告后更新。

| 任务包 | 独立结论 | 本包测试 / 当阶段全量测试 | 接收范围 |
| --- | --- | ---: | --- |
| FM-DEMO-002 | ACCEPT | 27 / 509 | PCG32 固定转换、不可变核心状态、16 字节编码 |
| FM-DEMO-003 | ACCEPT | 56 / 565 | 精确有理四则、比较、取整及整数位/步骤限制 |
| FM-DEMO-004 | ACCEPT | 35 / 600 | 无偏映射适配、完整耗字轨迹、PRD 候选与失败原子性 |
| FM-DEMO-005 | ACCEPT | 32 / 632 | 精确评分整数出口、上下界证据及共享工作区 |

## 逐项验收依据

以下编号逐项对应原任务包的验收编号；每项均为 PASS。测试依据为所链接测试源码及下方实际 XML，而非本轮执行。

| 包/条目 | 独立核对结果 | 源码与行为测试定位 |
| --- | --- | --- |
| 002① | 42/54 初始化丢弃两字；后续六字逐一断言 a15c02b7、7b47f409、ba1d3330、83d2f293、bfa4784b、cbed606e。 | [Pcg32Core.cs:27](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:27)；[Pcg32CoreTests.cs:18](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:18) |
| 002② | 第三字后编码/解码直接恢复，接续后三字；Restore 不调用 Initialize。 | [Pcg32Core.cs:39](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:39)；[Pcg32CoreTests.cs:151](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:151) |
| 002③ | Restore(0,1) 输出 0、后态(1,1)，零状态合法；编码显式按 State、Increment 各 8 字节小端。 | [Pcg32Core.cs:47](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:47)；[Pcg32CoreTests.cs:85](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:85)、[Pcg32CoreTests.cs:165](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:165) |
| 002④ | 最大状态得到 fff00001 / a7ae0bd2b36a80d4；oldstate、uint 截位、旋转31和 unchecked 回绕符合固定公式。 | [Pcg32Core.cs:52](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:52)；[Pcg32CoreTests.cs:100](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:100) |
| 002⑤ | rot=0 原样返回；同前态重复、不同实例交错均不改输入。 | [Pcg32Core.cs:58](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:58)；[Pcg32CoreTests.cs:111](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:111)、[Pcg32CoreTests.cs:119](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:119)、[Pcg32CoreTests.cs:133](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:133) |
| 002⑥ | sequence≥2^63、null、错误长度、偶数 inc 按规定拒绝；编解码不保留可变数组。 | [Pcg32Core.cs:80](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs:80)；[Pcg32CoreTests.cs:53](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:53)、[Pcg32CoreTests.cs:182](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:182)、[Pcg32CoreTests.cs:197](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:197)、[Pcg32CoreTests.cs:222](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs:222) |
| 002⑦ | 核心仅纯值运算，无 Unity/Editor/系统随机；001 的42项和 FlowPuzzle 的440项均在后续实际 XML 中通过。 | 依赖核对及验证表；当前001文件指纹见后表。 |
| 003① | Create 先核位长，再处理零分母、正分母、0/1及 gcd；构造私有、值只读。 | [ExactRational.cs:17](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:17)；[ExactRationalTests.cs:25](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:25) |
| 003② | 四则和有符号精确比较正确；2^53以上的单位差未丢失。 | [ExactRational.cs:38](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:38)、[ExactRational.cs:85](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:85)；[ExactRationalTests.cs:34](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:34)、[ExactRationalTests.cs:89](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:89)、[ExactRationalTests.cs:95](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:95) |
| 003③ | 加减先分母 gcd，乘除先交叉约分；比较/分母等未约分中间值也受界限约束。 | [ExactRational.cs:48](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:48)、[ExactRational.cs:110](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:110)；[ExactRationalTests.cs:69](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:69)、[ExactRationalTests.cs:221](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:221) |
| 003④ | DivRem 按余数符号修正数学 floor/ceil，覆盖13.9999999995、2.0000000005及−1/3。 | [ExactRational.cs:96](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs:96)；[ExactRationalTests.cs:112](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:112) |
| 003⑤ | 4720/203取23；盾吸收6/5、9/5及剩余14/5、21/5精确守恒；贡献2、3及奖励30、5均直接断言。 | [ExactRationalTests.cs:120](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:120)、[ExactRationalTests.cs:133](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:133) |
| 003⑥ | 各入口重验旧值；同号加法剩余额度、乘法除界预检在超界结果产生前失败；gcd余数、DivRem双步及预检计步，同一预算不重置。 | [ExactMathBudget.cs:36](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathBudget.cs:36)、[ExactMathBudget.cs:62](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathBudget.cs:62)、[ExactMathBudget.cs:115](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathBudget.cs:115)、[ExactMathBudget.cs:152](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathBudget.cs:152)；[ExactRationalTests.cs:165](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:165)、[ExactRationalTests.cs:189](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:189)、[ExactRationalTests.cs:229](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs:229) |
| 003⑦ | 复用 System.Numerics.BigInteger，无新增程序集/包引用；既有509项均通过。 | 依赖核对及验证表。 |
| 004① | 最小32位字组、t=M%n、R≥t接受；整组拒绝，首字置高位。注入0/1和2^32+1四字向量及非对称64/96位拼接均覆盖。 | [ExactRandomSampler.cs:95](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:95)；[ExactRandomSamplerTests.cs:20](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:20)、[ExactRandomSamplerTests.cs:39](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:39)、[ExactRandomSamplerTests.cs:56](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:56) |
| 004② | n=1及q=0/1不读字；n=2^32以一字直接输出，未溢出成0。 | [ExactRandomSampler.cs:85](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:85)、[ExactRandomSampler.cs:101](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:101)；[ExactRandomSamplerTests.cs:48](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:48)、[ExactRandomSamplerTests.cs:130](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:130) |
| 004③ | 保留 Initial/Current/BigInteger w；恢复不播种，w=0核核心一致；零状态w7→8及超过64位计数正确，轨迹复制为只读数组。 | [Pcg32StreamState.cs:26](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32StreamState.cs:26)、[RandomSample.cs:12](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RandomSample.cs:12)；[ExactRandomSamplerTests.cs:70](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:70)、[ExactRandomSamplerTests.cs:92](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:92) |
| 004④ | 先核0<C≤1与0≤f<ceil(1/C)，按约分q=min(1,(f+1)C)抽样；seed10/54、C1/4前三次确实失败，第四次零字成功归零。 | [ExactRandomSampler.cs:24](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:24)；[ExactRandomSamplerTests.cs:153](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:153)、[ExactRandomSamplerTests.cs:183](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:183)、[ExactRandomSamplerTests.cs:206](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:206)、[ExactRandomSamplerTests.cs:392](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:392) |
| 004⑤ | 字、步骤、位数分别设限；每个实际读字记录，包括失败前缀；后态只经 Complete 返回，异常不发布值/后态，输入不改，补预算沿原前态重演。 | [ExactRandomSampler.cs:142](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:142)、[ExactRandomSampler.cs:164](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:164)；[ExactRandomSamplerTests.cs:233](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:233)、[ExactRandomSamplerTests.cs:266](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:266)、[ExactRandomSamplerTests.cs:282](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:282)、[ExactRandomSamplerTests.cs:336](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:336) |
| 004⑥ | 记录字列与真实PCG共用 UniformCore；实际PCG拒绝及严格U<a有独立断言，交错流不相互消费。 | [ExactRandomSampler.cs:50](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs:50)；[ExactRandomSamplerTests.cs:109](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:109)、[ExactRandomSamplerTests.cs:218](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs:218) |
| 004⑦ | 当前公开入口符合原包，无新增服务/身份/存档接口；既有565项均通过。 | 白名单、依赖与验证表。 |
| 005① | 两个G01贡献分支分别得14，LowerBound/UpperBound各自 floor=14。 | [ExactScoreCalculator.cs:26](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:26)；[ExactScoreCalculatorTests.cs:13](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:13) |
| 005② | x=3.99/4/4.01得0/1/1；log2(1)、2^200、2^2000及零斜率走精确分支，TermsUsed=0。 | [ExactScoreCalculator.cs:30](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:30)、[ExactScoreCalculator.cs:63](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:63)；[ExactScoreCalculatorTests.cs:32](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:32)、[ExactScoreCalculatorTests.cs:54](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:54)、[ExactScoreCalculatorTests.cs:70](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:70) |
| 005③ | 非幂先规约到1≤y<2，正尾界传播；只在上下界同floor时返回。近整数低项预算失败、更大预算成功；另以(5/3)^100的整数不等式核规约和缩放。 | [ExactScoreCalculator.cs:71](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:71)、[ExactScoreCalculator.cs:118](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:118)；[ExactScoreCalculatorTests.cs:97](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:97)、[ExactScoreCalculatorTests.cs:119](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:119) |
| 005④ | E/G/a/b/C非负、J>0及所有null/释放状态先检查，E=0亦不跳过J。 | [ExactScoreCalculator.cs:12](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:12)、[ExactScoreCalculator.cs:50](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:50)；[ExactScoreCalculatorTests.cs:315](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:315)、[ExactScoreCalculatorTests.cs:331](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:331)、[ExactScoreCalculatorTests.cs:345](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:345) |
| 005⑤ | 全调用共享Math/LogTerms；每轮预占2项，已用项/步骤不退还；成功保留证据，失败回到本次标记，旧结果不动；多scope共用总预留，Dispose仅释放本scope。 | [ExactEvaluationBudget.cs:28](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationBudget.cs:28)、[ExactEvaluationScope.cs:21](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationScope.cs:21)、[ExactScoreCalculator.cs:39](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs:39)；[ExactScoreCalculatorTests.cs:139](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:139)、[ExactScoreCalculatorTests.cs:176](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:176)、[ExactScoreCalculatorTests.cs:198](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:198)、[ExactScoreCalculatorTests.cs:217](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:217)、[ExactScoreCalculatorTests.cs:239](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:239)、[ExactScoreCalculatorTests.cs:261](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:261) |
| 005⑥ | 同输入的Amount、上下界、TermsUsed及资源证据可重复；原600项回归通过；无浮点/decimal/Math.Log、epsilon或近似兜底。 | [ExactScoreCalculatorTests.cs:382](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs:382)；依赖扫描与验证表。 |

工作区单独复核：[ExactEvaluationScope.cs:58](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationScope.cs:58)按实际绝对值位长保留整数（0计1），[ExactEvaluationScope.cs:145](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationScope.cs:145)在执行有理运算前预留24×MaxIntegerBits，涵盖本版非递归四则/gcd调用链的局部整数及返回值，完成后转为保留登记。这里的24槽是原包明确采用的保守预留，不等于CLR数组或物理RAM峰值。
评分证明：N项下界为2Σ(t^(2j+1)/(2j+1))，尾界为2t^(2N+1)/((2N+1)(1−t²))；分母ln2区间严格正，正B允许单调传播。每轮先计2项，int型MaxLogTerms保证可执行N≤floor(int.MaxValue/2)，2N+1不会先于项预算检查溢出。

## 范围、来源与历史证据边界

当前51份实施文件=001的13份前置+四包38份新增（含meta）；现有公开API与原包限定相符。当前归属核算：002为601/700行；003现存685/900行（含后两包32行内部扩展）；004新建821+内部扩展24=845/1200；005新建914+内部扩展8=922/1500。合并总量3552行，001前置531行。
ExactMathBudget当前165行；004只补Remainder/ShiftLeft/ChargePcgWord，005只补IsPowerOfTwo，均为内部入口。历史003的133行、004的157行与当前版本不可直接比成污染；本报告以当前合并成果逐项归属，不从当前文件伪造历史逐包补丁。
历史Math哈希（仅标明阶段）：003为8C11901A7664FB050ABB56D7D13B031187E80EB5C69B5A34DD19FF63015A9CBD；004为35FEE46250C8F82EC9FC3A1DDBF135E5CA7D61D6C87DAFB8DCC0212419D7A360；当前005哈希见后表。当前其余有记录的核心/测试哈希与原阶段记录一致。
依赖实读：Unity2022.3.18f1、Test Framework1.1.33；FightMatch.Core只引用FlowPuzzle.Core且noEngineReferences=true；测试仅引用两Core并限制Editor。核心using仅System/System.Collections.Generic/System.Numerics及001的FlowPuzzle.Core，无新增数值库、Unity/Editor/系统随机引用。
27份允许读取的meta均有合法32位GUID，范围内无重复；全Assets唯一性沿原导入记录，本轮未越过白名单重扫全仓。当前002版权、移植说明与完整Apache-2.0许可仍在。
PCG来源独立核对固定修订bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3的[pcg_basic.c](https://raw.githubusercontent.com/imneme/pcg-c-basic/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/pcg_basic.c)、[pcg_basic.h](https://raw.githubusercontent.com/imneme/pcg-c-basic/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/pcg_basic.h)、[demo](https://raw.githubusercontent.com/imneme/pcg-c-basic/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/pcg32-demo.c)及[许可原文](https://raw.githubusercontent.com/imneme/pcg-c-basic/bc39cd76ac3d541e618606bcc6e1e5ba5e5e6aa3/LICENSE.txt)。转换/初始化与移植一致；作者六字网页本轮两次超时，六字数值依据已批准契约和实际测试XML，未冒称重取网页成功。

## 实际只读验证

| 阶段 | XML中的测试时间（UTC） | 实读结果 | 日志证据 |
| --- | --- | --- | --- |
| FM-DEMO-002 | 2026-09-20 13:54:01Z～13:54:03Z | 509/509 Passed；failed/skipped/inconclusive均0 | [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo002Compile.log)含return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo002Tests.log)记录保存[对应XML](D:/Unity/UnityProj/FightMatch/FMDemo002-EditMode.xml) |
| FM-DEMO-003 | 2026-09-20 15:23:41Z～15:23:43Z | 565/565 Passed；failed/skipped/inconclusive均0 | [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo003Compile.log)含return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo003Tests.log)记录保存[对应XML](D:/Unity/UnityProj/FightMatch/FMDemo003-EditMode.xml) |
| FM-DEMO-004 | 2026-09-20 15:43:33Z～15:43:35Z | 600/600 Passed；failed/skipped/inconclusive均0 | [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo004Compile.log)含return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo004Tests.log)记录保存[对应XML](D:/Unity/UnityProj/FightMatch/FMDemo004-EditMode.xml) |
| FM-DEMO-005 | 2026-09-21 01:25:38Z～01:25:39Z | 632/632 Passed；failed/skipped/inconclusive均0 | [编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo005Compile.log)含return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemo005Tests.log)记录保存[对应XML](D:/Unity/UnityProj/FightMatch/FMDemo005-EditMode.xml) |

实际解析四份XML的每个test-case：本包夹具分别27、56、35、32项；最新632=FightMatch192（含001的42）+FlowPuzzle440。八份日志的error CS / Compilation failed / Unhandled Exception均无匹配；命令均为指定项目，测试命令无-quit。
测试日志自身没有OS进程退出码行；测试exit0属于历史运行记录，本轮可独立确认的是XML Passed及日志的结果保存记录。四份XML哈希与历史记录相同；003～005日志哈希亦同记录，002两日志以本轮实取哈希补足标识。
当前源码/测试/meta的写入时间均早于005 XML所记测试开始；有记录的当前文件哈希匹配，审阅期间48份目录内文件与初始指纹一致。该证据支持当前合并成果接收，但旧日志未嵌入每个输入文件的完整摘要，不能据此重建各历史补丁或证明当时所有工作区输入。
NOT RUN：本轮未启动Unity、重跑编译/EditMode、执行算法复现或修改任何验证产物，原因是§75明令只读复核既有证据。未发现必须追加运行才能判定的疑点；Android/IL2CPP/AOT、正式概率/种子绑定、真实业务保存/结算及可试玩场景均不在本次接收内。

| 运行产物（SHA256） | 当前摘要 |
| --- | --- |
| [FMDemo002Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo002Compile.log) | 259D4FA2CA06D179406322DD0C00E9DE102B5AB83A3BDD0E518BF4E701483E3F |
| [FMDemo002Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo002Tests.log) | ABB03C6699BA66A2FDB7281409F8663D5B631C443A59A314382F4FF1BF475F61 |
| [FMDemo002-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo002-EditMode.xml) | 53FAD5FC051ED8AA22144973F24E294DFE4E290B561D01A5A199CB5CB9F158D7 |
| [FMDemo003Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo003Compile.log) | FF5D27BC05E1E056FD896AA2ED440FB37990AD82BD973C7B919C4C2230C5D854 |
| [FMDemo003Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo003Tests.log) | 63A7BEF14198C4184F4D3B3FA84F40651AB0726DFC125D263D7E2AF8C4FAD0EA |
| [FMDemo003-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo003-EditMode.xml) | 28F39F250A90FBDDE50D0ABE5066939C4FC729B51C072A17ECAC8CD9E521C8F3 |
| [FMDemo004Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo004Compile.log) | 0411F8D4FDB105574E8D90FA6DBE9541B062AB11815A9A4C77A6ACFCB17CE9CF |
| [FMDemo004Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo004Tests.log) | D26D69D53638F5E3C1999F5E48E324BE4C0D9CA7EC1E9AD0DD7AB5CAD26001A8 |
| [FMDemo004-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo004-EditMode.xml) | 1A8CD576258AA94EDE8B86DBCC1A00A6E48B7B11AA251C5465D5E4D4315615D0 |
| [FMDemo005Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo005Compile.log) | F9CE46341DC1CBD338DECDC4DBC0261D53908D358688D017AD100649FF91B5BB |
| [FMDemo005Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo005Tests.log) | 9F518EF5DB6D82B5D22E95F703CCBE932B7995F60B62ABC01BE374A95BDD88BA |
| [FMDemo005-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo005-EditMode.xml) | 4AE312EA8459B6D3C708EA7C37CB7F250F10C9A91EB4BFAA44CF4EB6D522A9F4 |

## 当前被审文件指纹

S=[Assets/Scripts/FightMatch/Core/](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/)，T=[Assets/Tests/EditMode/FightMatch/](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/)。下表每行同时记录正文及其同名.meta的SHA256；共48份文件。

| 文件 | 正文SHA256 | 同名.meta SHA256 |
| --- | --- | --- |
| [S/BattleRouteValidator.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleRouteValidator.cs) | E535F092BAF9F6E29F60CEAEC69D2385E17E481E26294BACB2C9CE2CD132B3A2 | CEFFE4FA6572412D2E4AE5BB3D4BDE48FEF5D47852471820C304F1DD71E82C56 |
| [S/ExactEvaluationBudget.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationBudget.cs) | 752B0520D68559A48F138CA0F77C429E699AD458A1394AFEFCBEF3DB6102FBAD | AF59A9114EFEB08E3C882FFD72BA6BE3A8BA66C7444A89369DEAB283933BB4EE |
| [S/ExactEvaluationScope.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactEvaluationScope.cs) | 1C510A91E813F6C32C1BF8FC8706923110ADDFDB18515720BE1A16262F488E89 | F019C7B699283B8D0EAA749076AEB3A67E2EE10119068EC15980483BE69EC8DF |
| [S/ExactMathBudget.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathBudget.cs) | C509642D11D3AB49836051452A1A175F196BAED6CF33E8C007FD0DBD9B7D5F14 | E17C6AB8D3DB08E0B7E7C1B8820B7E6EC33A8D84A0F10319A478A7AB4DF09A4F |
| [S/ExactMathLimitException.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactMathLimitException.cs) | 9F70F299A74918397FB45E5FF6E3E800E4E5970A27AB7D78077DB10687F7EA78 | F21EAE053551019050E76F27F44F132007ABE75BE58C6809D2B4B9343BE5ED09 |
| [S/ExactRandomSampler.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRandomSampler.cs) | 58A1099CC4067A88B25743B52287C76DE83DE9DD2C6188BAF4C8DD2A5ADE563C | 5EC8CBC0DC9695A770F95ED5536D251DCAB073A6248BEB70155945737431B8F4 |
| [S/ExactRational.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactRational.cs) | 474B6335ADB12F7E7B8B47C0C0C464D41AE4DE432C19004626165343F0AF3CB8 | AD08482EC134F41945BA1E79D4C6AB22E60B0D04B7584A7F4A57667A2C963451 |
| [S/ExactScoreCalculator.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreCalculator.cs) | 32A539979402C292BBCB514BE8ABAA5B5D8097E443A56C3FCE3241314E34F770 | 1C31605EF5445707B467EA2FEC4C0C56A04E2E7C631B213A8DE2C23B0197A5C1 |
| [S/ExactScoreResult.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ExactScoreResult.cs) | 555FEF276508606D2D7BEC5F821CEC9DB20ABE0A52BBE77B319A1595B8C8E054 | 7DBEC22F1E24A78D69A72DA3F486109290BE7E3B6095854CB295B9D0C187A6D8 |
| [S/FightMatch.Core.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/FightMatch.Core.asmdef) | 6D6137F686A78F223C6C93810D124DAD6E1F4D91B5CCFA40068A9C44FF2D867C | 5CBE329643CF73FA4740DEF9C151284AD9820FA7B00B162148E8C8D685922227 |
| [S/Pcg32.LICENSE.txt](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32.LICENSE.txt) | B40930BBCF80744C86C46A12BC9DA056641D722716C378F5659B9E555EF833E1 | 9A341AA5E2FE498BBC00EFA9DF4B42C1438FACA9441442D899CE0D90B4256BF0 |
| [S/Pcg32Core.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32Core.cs) | A4A882E1C8A92F0C88D49ABA90FE995E039D72237DCB1138BEFBC93DF0D787A1 | B9A1480168EF58422055B1F92B42524EB78126E43CA96B04F2122E63B98B22D8 |
| [S/Pcg32CoreState.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32CoreState.cs) | 516CB27B76F8813C76FB456F395D775DBA4C369091C3C78BBE5A84EED82C917F | CADD5042D378537B4A9BA24F6648A478D3E6AAF3313E3F87C6DA0F7EBF1C32EA |
| [S/Pcg32StreamState.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/Pcg32StreamState.cs) | 42AD7D12D97B4924C5DE2C10F9395066C888BA7EF664364B1354C4D80261B5AB | 3D4D4B6246C5853C2277C8F6AA788F22ED7808CCF48DE5146AFF6C8E58536094 |
| [S/PrdOutcome.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/PrdOutcome.cs) | 27CB46328A519A4A76BFE8DFA415CDFFC96D36D3BFA64989A13B0D5871AE10E0 | 9D75181937A64F76074859A35E21196A12DC4DC98F964289CFF8C79B7C6FB90D |
| [S/RandomSample.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RandomSample.cs) | DE3FDD94A0877E99385D5A5CF159E255BEA246B6E5C5EAAC8327FDC3896F2323 | 81A14B1E8111B5A8D74F4457001D470A84EFEEFEB6268EB7285E0CA5B40368CD |
| [S/RandomSamplingBudget.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RandomSamplingBudget.cs) | 027C84587FFC0D03B8A6F61B97218D465E20608085D1155C60E2E3D93EEE21D6 | B0C77112BF948C1DBE0DAD584252476E04242C1080861E366FB9951BF7799FFF |
| [S/RouteValidationResult.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RouteValidationResult.cs) | 948F43F12D23732FA06FFD5D26FCBCB018496862312F18EA2ED3389D5123ADFA | B03E651B5F6E5EBBCD843C16577AAC166D69018F8645A12E102B13D743C2F1BB |
| [T/BattleRouteValidatorTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/BattleRouteValidatorTests.cs) | 4292DA12BEC706736E88F57013955561C8157B6C071EBC894C8E0E5C8EC4590A | 385928CCA803D5429D72ED6C0C1E8D9B9F143402611DB6CD9D4F2383B320420C |
| [T/ExactRandomSamplerTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRandomSamplerTests.cs) | EA11585652F17575DBBFC01824AD481C6BA7E0B07C014D0D08FBB3FBA5E8F01B | 6F4E77586F791B478B4D4851DAE3BF91F38EFF92F0FBBF321C80BB62590DEF3D |
| [T/ExactRationalTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactRationalTests.cs) | BE217F9D8DD08C1B3EB78854BA55A5F6B520175968836885513BAB34A0E5CD60 | 131EF3B741FC0CBDF5EE5B1382DE777A05381BACA518A3915B5F72BEB1F79360 |
| [T/ExactScoreCalculatorTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/ExactScoreCalculatorTests.cs) | 1B07216A7E0A4E316DD042B745C56D2C708524405304D390FA3D2A7775BBF760 | 53C810A3EE5063A7C03578BEF7B61E0FF232BE1FC3A954DF41F682F079235AEA |
| [T/FightMatch.Core.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef) | 836B9DE4FE2AAD84DED6116D2F46CDB81BFDC5406D440940694CC7A2FCFE524A | 1171DB5F6FB922469BDF6470DCD19AC2746BE9D223BDF119D264C928C285745F |
| [T/Pcg32CoreTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/Pcg32CoreTests.cs) | 2E244AD06009C02E135340EB4A95136830AF5B752A005B885FF5E613C41D00FD | 538EA72278A8FDDBEDD1C80F7C6B47B80D61E661BE9317B695A2A42DF984BCA7 |

| 目录meta及只读依赖/规范 | SHA256 |
| --- | --- |
| [CODING_RULES.md](D:/Unity/UnityProj/FightMatch/.agent/CODING_RULES.md) | 35DCF6B76EC660509414580BA160DFF080DC76D2450293B59A79A7CC91F91F9F |
| [PLANS.md](D:/Unity/UnityProj/FightMatch/.agent/PLANS.md) | 5039F9BC8A51EAFAC968B652B09906B90C6F335395DC3AFEC3899FB83A7324EB |
| [PROJECT_CONTEXT.md](D:/Unity/UnityProj/FightMatch/.agent/PROJECT_CONTEXT.md) | 4CF1E141E39F12165103E52E44357B1C179175BD18D2C874F386F0360ABFC9A9 |
| [REVIEW_CHECKLIST.md](D:/Unity/UnityProj/FightMatch/.agent/REVIEW_CHECKLIST.md) | B5C39A7C85D2FD41182AC4AD3DB30CF7ED28E0D9A696354F4CFE5A507E57C8A7 |
| [VALIDATION.md](D:/Unity/UnityProj/FightMatch/.agent/VALIDATION.md) | B65114C2444DEE680741E52307FEC57CC0ABC4B1196767962BC8E4DF322690A0 |
| [AGENTS.md](D:/Unity/UnityProj/FightMatch/AGENTS.md) | C1A836FF14CC0834E16C33CBC49D53BEC82CADFF656A329C24C72F569D460528 |
| [FightMatch.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch.meta) | 0C10366EDEFB5C9B0456407EFDB3015C44762067E0993152651102010B7ACEEE |
| [Core.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core.meta) | EBD9BEF06FA901C5A3A525F4F8C222D34082E2D7EEE13974A251C042D2709058 |
| [FlowPuzzle.Core.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef) | 9F684E62A102825715D9D6DB929FDF9EF0D23A636C6AD38C59BFBC4168C9E3A8 |
| [FightMatch.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch.meta) | D98B054B40701598DF59C281B66D0F275A62B0A66C2AE8A0C37234EAF8F3684C |
| [FlowPuzzle.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef) | CA8A4C0F4CFC1E395EEF8AF9DB42FF4240226B39BE37AA26163BA33AB4DF4A1E |
| [contribution-scoring.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/contribution-scoring.md) | 5194DC775EEB2BF359604EB1966A20A51062C7C48BFEDFCE5C15601EC1931912 |
| [numeric-random.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/numeric-random.md) | 45ADBD3FF517C2DB91F1FC8A7C967241C681771FE0A61E2D4E860404387B47BC |
| [interaction-contracts.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/interaction-contracts.md) | EAB83027C412619731DF1F7F3FD1650161915D96E31D68EA30F4AF4AF9F96F06 |
| [session-plan.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/session-plan.md) | A0B9BECC14B49AB76A9286B1F017D412291B9CB64F7367BEF50E4851BE86F635 |
| [integration-review.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/integration-review.md) | CBA39712D3B81CDBC047B36348E9D8B187758B0271C58C1B77BEA566DBFC9051 |
| [system-task-packets.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md) | EF75D8A4D77B54CCC9A1E8F86A4239C625E8DD30BAC41DA7AD4425E6976E4EF6 |
| [manifest.json](D:/Unity/UnityProj/FightMatch/Packages/manifest.json) | E15E302B5C4342D530AE52F31C785A9626B4FF42ECC1AA7612FCC3F0637B872A |
| [packages-lock.json](D:/Unity/UnityProj/FightMatch/Packages/packages-lock.json) | 160073F2CD54A18FC4A3995C64B53DE964356C5F36B66020FB66EB165E01C31A |
| [ProjectVersion.txt](D:/Unity/UnityProj/FightMatch/ProjectSettings/ProjectVersion.txt) | 9B7F178DD8C050E64943DB5709939F39FD3191C18C6C557143963975891B50E2 |

## 交回与自检

唯一变更：新建本报告（≤180行）；没有源码补丁、待修文件或必要复现用例。SD00可据四包ACCEPT及上述摘要更新原三份协调稿；本任务不代写协调状态或启动下一包。
已执行git diff --check，exit0（仅有既有文件行尾转换提示）；未将空的tracked diff当作未跟踪源码的证据，全部现有源码/测试另行全文审阅并按字节取SHA256。
收口验证（2026-09-21 10:04，UTC+8）：149行≤180；165个本地链接及引用行号有效，尾空白/冲突标记均0；80份只读输入SHA256全部保持。Git status/name-only/stat与写入前一致（既有tracked差异仍为34文件、+2809/−597），本报告为唯一写入；单独NUL→报告的diff --check退出1，仅有新增文件差异和行尾转换提示，无空白错误。
