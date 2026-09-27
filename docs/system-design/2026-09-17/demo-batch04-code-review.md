# DEMO-B04 独立代码审查（B04-R1）

| 任务包 | 唯一裁决 | 适用范围 |
| --- | --- | --- |
| FM-DEMO-008 | **ACCEPT** | SC01显式材料映射、候选随机绑定及单战士PRD机会求值。 |
| FM-DEMO-020 | **ACCEPT** | 单战士候选普通经验、精确基础属性、正常结束及局外自然恢复。 |

两包各自验收满足，整批可关闭，由SD00登记。发现问题清单为空，无需修正包或用户操作。

审查依据为 [system-task-packets.md r65 §86～89](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:1326) 及其中许可的设计输入；分别核对Standards与Spec。
R于2026-09-21T05:28:12.4237181Z保存独立基线，当时任务稿SHA为 `deaad01e503027fc75a218bbe7cc1fb9fdb3dfd17d89952d868b652a8704c4d1`。
SD00随后更新三份协调稿的任务登记／WORKER_RETURNED／REVIEWING并续写后包设计。R逐行比较原§86～89，差异仅为上述登记与状态说明，接口、白名单、验收和联合验证要求保持；这些协调变更不归因A/D。
已读取AGENTS、.agent审查／验证规则、成长／平台保存／数值随机／战斗历史／内容校验／交互契约与任务包许可策划配置，完整阅读13份C#、13份meta及现有直接依赖。

| 作者 | 任务ID | 本次turn | completed（2026-09-21 UTC） |
| --- | --- | --- | --- |
| A / 008 | `01a0c1fb-248d-7321-8b69-9efd26b817f1` | `01a0c26e-2148-7a91-bf38-4acf4815edfb` | 06:04:08，wait revision 100 |
| D / 020 | `01a0c26d-9762-7ee2-bb7c-0fa8204f2f4e` | `01a0c26d-99e8-7d91-8432-136dca6d6155` | 06:07:38，wait revision 43 |

正式审查在两作者本次turn均completed、双方完整报告及联合验证证据齐全后开始。D的CODE_READY只用于允许A验证，没有提前充当完整交付。
R只做静态阅读、SHA／GUID／XML及只读Git核验，本轮唯一写入为本报告；没有启动Unity、修改实施文件或创建任务／子代理。

008六项验收：

| 项 | 实现与行为证据 | 结论 |
| --- | --- | --- |
| ①SC01逐字节 | [CandidateRandomPreparer](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs:44) 用显式LE64读取三个16字节块，只在材料映射清t最高位，复用原PCG初始化且w=0；不改核心取值域。[边界向量](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:18) 核全0、00～0f、2^63、全ff及原核心拒绝高b；[材料拒绝](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:111) 核47/49、缺能力、Mapping大小写／空格错误和准确拒绝路径。 | 满足 |
| ②域与完整初态 | [CandidateRandomPreparer](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs:27) 固定Battle/BaseReward/Bonus顺序，显式补齐Ready战士PRD f=0后交007B组装；[CandidateRandomBinding](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomBinding.cs:11) 保留原生成Player/Challenge/Attempt/Baseline与Context。[四组来源](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:58) 走006→007A→008的L1/L3、双方向，逐值比007B完整图，核a42/b54首字a15c02b7及合法相同块。 | 满足 |
| ③业务机会 | [CandidateWarriorCritEvaluator](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs:43) 复用PrdOpportunity，只替换该角色f和Battle当前态；事实保留p/C/倍率、q、前后f/流及全部原始字。[连续三机会](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:155) 核a15c02b7、7b47f409、ba1d3330与失败/失败/成功；[必出零字](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:191) 和[真实拒绝字](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:212) 核零字、拒绝消耗和奖励域不变。合成C1/4、C1明确不作为正式C审定。 | 满足 |
| ④绑定拒绝 | [CandidateWarriorCritEvaluator](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs:20) 核Ready战士、同Attempt真实目标键、非负ordinal、Battle Initial及完整PRD键集/参数/f域，成功前不产生Next/Fact。[归属拒绝](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:247)、[合法候选错配](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:277) 核跨Attempt/成员/面/敌/流/参数和Ordinal身份；[后续面](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:309) 核同Attempt任一已定义面可绑定，真实当前存活机会仍归009。 | 满足 |
| ⑤精确预算／隔离 | [CandidateWarriorCritEvaluator](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs:87) 遍历绑定、全部Prepared面/成员、初态/贡献及当前流/PRD保留数值，共用传入Math和WordsUsed，无预算重置。[小预算重验](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:349)、[共享限额](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:396) 核Limit原样传播、无半结果、新预算重试；[输入隔离](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs:425) 核材料修改、不同Attempt不加盐及只读输出。 | 满足 |
| ⑥范围／真实验证 | 仅本包5份C#及5份meta，973行≤1400；5个GUID全局唯一，旧331项不变。无Unity依赖、平台取熵、Published、H10、奖励办理或SeedId；保留能力声明不冒称实际来源。A唯一执行的同版编译/EditMode成功，008新增58条均Passed，原961多重集合保持。 | 满足 |

020七项验收：

| 项 | 实现与行为证据 | 结论 |
| --- | --- | --- |
| ①经验与防重 | [CandidateCharacterGrowth](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs:54) 先核目标/Context及原Settlement事实，再处理新请求修订，重复返回当前状态和旧收讫；新奖励按正N(L)精确扣级，Amount0也留来源。[经验算例](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:21) 核W4+157加14→W5+6、加50→W5+42、L5+92加150→L6+22及多级；[旧收讫重放](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:35) 核后续成长后重复增量0、冲突与过时新请求。 | 满足 |
| ②属性与交接 | [CandidateCharacterGrowth](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs:88) 从固定Lv1定义和L−1求HP/攻击比例成长、防御增量、目标p封顶；[CandidateComputedStats](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateComputedStats.cs:22) 保留原槽及Context，Ready才输出MaxHp作EntryHp，不含C/f。[属性向量](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:94) 核Lv4=124、118/5、29/2、21/2、p43/200，Lv20=59/200及Lv31以上7/20；[007A/007B交接](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:108) 核slot2及旧Prepared/Baseline不受后续成长影响。 | 满足 |
| ③精确与定义拒绝 | [CandidateCharacterGrowth](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs:10) 直接校验可变CandidateContext并复制定义，无入场依赖循环；[CandidateGrowthDefinition](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs:144) 重验定义及全部历史/时间/修订数值。[缺项与定义域](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:128)、[初值与预算](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:177)、[精确时间](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:199) 核非法域、超过2^53、嵌套保留值及小预算；共享步骤不足保留旧态并可新预算重试。 | 满足 |
| ④正常结束 | [CandidateCharacterEnd](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterEnd.cs:55) 同时约束Attempt及EndReceipt身份，重复返回当前态；仅正常胜利/退出中Ready参战倒下者建原180000ms期，立即重来不新建，非参战者保留旧期。[正常结束](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:15)、[重复与冲突](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:60) 核存活满HP、重复/矛盾事实、同Attempt换EndReceipt、已完成恢复ID不得再分配；[显式空值](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:121) 核未提供与null不同。 | 满足 |
| ⑤时间与旧回执 | [CandidateRecoveryClock](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs:66) 先找原恢复期，完成旧期直接返回当前状态；活动期同域用单调差，否则UTC并标DomainChanged/DeviceUntrusted；负差保留高水位与修订，非负差精确封顶。[同域及跨域](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:144)、[回拨与边界](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:174) 核10s、179999→180000、分数ms与完成后回拨；[零间隔/旧期](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:212) 核原样本Anomaly与派生诊断分存、重复不增量、旧期不触碰新期。 | 满足 |
| ⑥隔离／完整性 | [CandidateCharacterState](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterState.cs:9) 的定义、L/x、原槽、收讫/结束/恢复列表只读，Ready由活动期推导，无第二份累计经验或长期战斗HP。[定义/奖励隔离](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs:248)、[结束/时间隔离](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs:300) 核输入壳及SourceNotes修改不改旧结果；源、时长和原结束均保留。无卡证/评分/SDK/服务器时间/发布/真实提交，不以目标p充当C。 | 满足 |
| ⑦联合验证 | D先冻结8份C#并发CODE_READY，A核13份后统一生成meta、编译和测试，D随后独立核175条及同版SHA才完成。仅本包16项，1491行≤2200，8个GUID唯一，旧331项保持；作者报告未替代R裁决。 | 满足 |

Standards检查：两包均遵守新增白名单、Core纯C#与测试分离、原数学核复用、Ordinal身份、只读构造/集合和既有错误返回约定；没有旧API、依赖、工程、资源或序列化变更。
三份测试已全文核对行为断言，008错配状态来自另一个合法候选，020交接使用显式合成C；没有靠反射伪造旧不可变状态或仅以测试数量增长作验收。

被审实施文件，以下SHA均由R重新计算：

| 包 | 文件 | 行数 | SHA-256 |
| --- | --- | ---: | --- |
| 008 | [CandidateSeedMaterial.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateSeedMaterial.cs) | 10 | `eef4d6f93a8cf4dfb6bfe372702bc363d1bf5052a946231090a7e75d010557a3` |
| 008 | [CandidateSeedMaterial.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateSeedMaterial.cs.meta) | 11 | `19523465982c9c31ae14f5f45e570221cbf7b8583dc3714d1194ea454411f1df` |
| 008 | [CandidateRandomBinding.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomBinding.cs) | 113 | `94263e9be5e91d48ff3ca1efb46361f7b8cb5187a0507b13258cdfd1edfcd377` |
| 008 | [CandidateRandomBinding.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomBinding.cs.meta) | 11 | `6bf81f9966f30db0df0b41fcaa14927b2ad69618ac4271bbca68db0b497863b2` |
| 008 | [CandidateRandomPreparer.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs) | 63 | `22aa5044602fed30dd9b1a02aa8b1ffddebc19f6e762817a2be1dae228704796` |
| 008 | [CandidateRandomPreparer.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRandomPreparer.cs.meta) | 11 | `a6158aea7f0d83fa625f9f243df2b03c8f45cf84c0bb5a8d52d5f1614e87a759` |
| 008 | [CandidateWarriorCritEvaluator.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs) | 142 | `e73e9be77a30c431eab2c58d95cfa129a94e9895468774371e671dd74be41082` |
| 008 | [CandidateWarriorCritEvaluator.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateWarriorCritEvaluator.cs.meta) | 11 | `706d25c580aa387340c449256ff751d16c7f8eb9cf91f658dd0af7af0184d3fc` |
| 008 | [CandidateRandomBindingTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs) | 590 | `33af55d2bd6e8cca49dd8064582216dbd821ca892daf4d7dc0610dedcbc10f9e` |
| 008 | [CandidateRandomBindingTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRandomBindingTests.cs.meta) | 11 | `0944d96946baf71fe9eac64b09caf684dd1e6b5eebf4586729224acb13a894db` |
| 020 | [CandidateGrowthDefinition.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs) | 177 | `41d4bd2b75e209fa7be65120777caed435d4ddb60494b172581de9d9ac43b1d3` |
| 020 | [CandidateGrowthDefinition.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateGrowthDefinition.cs.meta) | 11 | `23d49a1dbe3c7a83a913c2c61df34d13b844aae68ad59ea85f01adb1dd5e3f2d` |
| 020 | [CandidateCharacterState.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterState.cs) | 94 | `333bbd2ae5f0e08508d3be493402e2cfad55de3556495ad5048bdb54230d8d83` |
| 020 | [CandidateCharacterState.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterState.cs.meta) | 11 | `c849b0039fc9522f8721e24f3fb547ce4de7b0685c38e629c1a38ba0010c993a` |
| 020 | [CandidateCharacterGrowth.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs) | 117 | `6992bcf387b0434fa86dcfec9aa689b7a3de1cfaa8784210ae2e14ca0abffe21` |
| 020 | [CandidateCharacterGrowth.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterGrowth.cs.meta) | 11 | `663b77954c614ac02986850ead6515ec353a76b016f3f33ed258f00c87738a94` |
| 020 | [CandidateCharacterEnd.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterEnd.cs) | 123 | `b3a056f0f5e1f42938e0fea1a3185c00a2c02d9434753e1671cb6272b568d884` |
| 020 | [CandidateCharacterEnd.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateCharacterEnd.cs.meta) | 11 | `e7b1be98883f84fdfdfe2b85cee171dd434ba75505e7c6fa58e3bddc0b1a3141` |
| 020 | [CandidateRecoveryClock.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs) | 135 | `86d8920bb392df984fac0d55294fa93fee736a7f81f4a79c3ef4927b724c954a` |
| 020 | [CandidateRecoveryClock.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateRecoveryClock.cs.meta) | 11 | `3a459586430427d260a5a2cd75199c08f0873bd79004de7b5302736282239c74` |
| 020 | [CandidateComputedStats.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateComputedStats.cs) | 31 | `78929d05c7f103c10730dc2e76f1c6f774b5713ac4bd46ae05d721b589358c6a` |
| 020 | [CandidateComputedStats.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateComputedStats.cs.meta) | 11 | `e6e9e5522235ea6f7320971af0fd4ece4ba519d33f4b5b00d8e36264eaea192e` |
| 020 | [CandidateCharacterGrowthTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs) | 372 | `a3ccf8647a201218f2efbc97ef43214f4508f33fec0cb00042ec3c42d0d4aaa8` |
| 020 | [CandidateCharacterGrowthTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateCharacterGrowthTests.cs.meta) | 11 | `1b38e12f7dc5c8434f422c9b1df7cc7ce03704181c367212d5528ae124c7dad7` |
| 020 | [CandidateRecoveryTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs) | 354 | `9610afacd630138c0cc677a4c1f13457ff2fae6f1ec80fce389077e25a922f56` |
| 020 | [CandidateRecoveryTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/CandidateRecoveryTests.cs.meta) | 11 | `0bfcffac376c53a8f52a06e13252942504bc0a5f9b3ceb8203a4ed9d0cce82e9` |

共26项2464行；新增C#无尾空白或冲突标记。13份meta均为Unity标准MonoImporter，空字段格式保留，编译日志226～238行逐一记录相同GUID的首次导入。

| meta对应C# | GUID | 全Assets出现次数 |
| --- | --- | ---: |
| CandidateSeedMaterial.cs | `a7b46f53318f79c43b4a4a8b07802625` | 1 |
| CandidateRandomBinding.cs | `0e847a3d817d5bc42a296e51795a7ac6` | 1 |
| CandidateRandomPreparer.cs | `615d4fa46b56b8f48bd300bb8f5956df` | 1 |
| CandidateWarriorCritEvaluator.cs | `b4f857e2c10477841ba1e23f4ae962a6` | 1 |
| CandidateRandomBindingTests.cs | `592be2951369fd649b90e540cb7858c0` | 1 |
| CandidateGrowthDefinition.cs | `1dad6da4caffa1747a7ca87ba0da93fa` | 1 |
| CandidateCharacterState.cs | `145e40f2788a8df4983e0cacd02406e9` | 1 |
| CandidateCharacterGrowth.cs | `699b798d9b049b64bb160607f02c2878` | 1 |
| CandidateCharacterEnd.cs | `fd8a3fd186e19f34ea5a6d4e3ceb09b0` | 1 |
| CandidateRecoveryClock.cs | `db7bd9a119768c34881740b1657489ca` | 1 |
| CandidateComputedStats.cs | `240e1009f6fd13f468cb0adad6cc7c15` | 1 |
| CandidateCharacterGrowthTests.cs | `6186e7cf9336c7546879181f0c917a7a` | 1 |
| CandidateRecoveryTests.cs | `480c9c816a1d5894c8fd866644146702` | 1 |

作者交付与范围证据：

| 文件 | 大小／行数限制 | 当前SHA-256 |
| --- | --- | --- |
| [demo-008-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-008-delivery.md) | 134行≤180 | `1d446b2271f93b55e8a62f667dac4b68dc7f39757362996e06f07892ad9e3c6c` |
| [demo-008-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-008-scope.json) | 194804 bytes≤300KiB | `7fb158f7ab6ef847017e007a9cc1aaeebc62539c44ca9a3d05468fbdf7b7e23a` |
| [demo-020-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-020-delivery.md) | 114行≤180 | `409dd4f7a3f300bc264d25a714439dd6e4294eee1cd2865f3826daef7834a7c7` |
| [demo-020-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-020-scope.json) | 134879 bytes≤300KiB | `9efd8a33f201e55604aaad5f762d9052d5795ee70d6c59746c6ebbd2a5c9f1d6` |

- A before捕获于05:29:17.4967456Z，D before捕获于05:30:17.8421489Z；各331项均与已接收007B after及R事前331项逐路径SHA一致。工具记录确认A的12个新路径、D的18个新路径原先不存在。
- 从A最终JSON移除追加段后还原原文件，SHA为 `c08109a94262c745477c7dd337f9d3399e6fdd4b730b23bee493c0775ec0e295`，与修改前工具记录一致；D追加freeze/after/delivery时保留前缀，原before逐值保持。
- 两份before、D的8份freeze、A的13份联合freeze、导入前344项、双方after357项，R均重新计算规范摘要并逐项与当前文件比对，差异0。规范为Ordinal相对路径+TAB+小写SHA256+LF，UTF-8无BOM，含末尾LF。

| 集合 | 时间（2026-09-21 UTC） | 项数 | 规范SHA-256 |
| --- | --- | ---: | --- |
| 旧基线／双方before | 05:29:17.4967456 / 05:30:17.8421489 | 331 | `24f0e94da6104daa0565ea00a46d541d65ab886d4d43676bd7a458f557f3496b` |
| D CODE_READY | 05:48:57.2018467 | 8 C# | `76f911afaf1091beb08a7713774efacfc38ba8d8db3a11420168ba5b488dddce` |
| A联合冻结 | 05:50:55.0969586 | 13 C# | `dec0749e599e2a5e9bebf6e8ecd282ee69bd0251cbed25c18db83ce5cf3446e2` |
| 导入前 | 05:50:55.1238741 | 344 | `9f9897e0ae251338faab582d35b702ee51147d1dc7a74eeeaf1e8b536256bc81` |
| 测试前／A after／D after／R现文件 | 05:52:31.9533564 / 05:54:56.6929101 / 05:59:53.1365126 / 06:18:34.9022146 | 357 | `ccc1f7b69dea4625d2f87c5e7689a1cc651bc9a315cf9c5bba5a2dfccafda0f0` |

R的Assets路径集合从348到374，差集恰上述26项，无删除；另一作者明列新增属于允许并行范围。除SD00许可更新的三协调稿外，基线24份规则/设计/前批报告/配置/XML保护指纹保持。
只读git diff --check及--stat均exit0，既有tracked差异仍34文件、+2809/-597；未把旧FlowPuzzle/Packages/.claude差异算入本批，也未动Git。

实际联合验证由A执行，R核对了作者本次turn的原始命令事件、输出和磁盘产物：

A先确认固定Unity路径存在、没有活动Unity进程，核旧331及冻结13份SHA；两个进程均Start-Process -WindowStyle Hidden -PassThru，WaitForExit并Refresh后读取真实ExitCode。D的本次命令记录没有启动或停止Unity。
D的CODE_READY发送事件为 `exec-0b902960-5f1f-4453-8134-abbf866942f7`；A联合冻结事件为 `exec-1354ba46-6266-4fbc-a709-d6abb5685175`，先于编译。D就绪报告历史SHA为 `06fed4ae539841426cecbb79c690a6e025e2b86271214148c7c321ad13b827ea`，最终报告按协议补充验证证据。

```powershell
$env:UNITY_EXE = 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe'
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-quit','-projectPath','D:\Unity\UnityProj\FightMatch','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Compile.log') -WindowStyle Hidden -PassThru
Start-Process -FilePath $env:UNITY_EXE -ArgumentList @('-batchmode','-nographics','-projectPath','D:\Unity\UnityProj\FightMatch','-runTests','-testPlatform','EditMode','-testResults','D:\Unity\UnityProj\FightMatch\FMDemoB04-EditMode.xml','-logFile','D:\Unity\UnityProj\FightMatch\Logs\FMDemoB04Tests.log') -WindowStyle Hidden -PassThru
```

| 运行 | 工具事件 | PID | 2026-09-21 UTC起止 | 实际退出码 |
| --- | --- | ---: | --- | ---: |
| Compile/import | `exec-906238be-1f72-4799-ac38-2e2700cd03bc` | 55792 | 05:51:26.6966193 → 05:51:39.2744102 | 0 |
| 全量EditMode | `exec-1850cafa-dbe1-4d67-978e-7adb47055547` | 43052 | 05:52:31.9533564 → 05:52:43.6411037 | 0 |

日志COMMAND LINE ARGUMENTS逐项匹配，Unity为2022.3.18f1，测试命令没有-quit。[编译日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB04Compile.log:214) 记录Core/Tests Csc编译，220行build success，530行最终return code 0；[测试日志](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB04Tests.log:335) 记录本批XML保存路径。
两日志有许可客户端Code 10/token诊断，编译日志另有Curl 42；随后授权解析和运行成功，无C# error/warning、Compilation failed或Unhandled Exception。编译202行Bee中间ExitCode 4由随后DAG重建消除，不是Unity最终退出码。

| 本次运行产物 | R重算SHA-256 |
| --- | --- |
| [FMDemoB04Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB04Compile.log) | `40c467697d85c937292e3b2e1a13e00eb3615be847a3c001166b99279a41bc82` |
| [FMDemoB04Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemoB04Tests.log) | `a3d4c3f0f8cb05ff12c5fea500db50c7c569f517cc024b04457b99f0843cfb49` |
| [FMDemoB04-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemoB04-EditMode.xml) | `37002ecd1f8f647c7f62a153d055df4055673b4019934685cbdb0dd0ebc83f95` |

R独立解析本批XML根和全部test-case：total/testcasecount/passed均1194，failed/skipped/inconclusive均0，执行时间05:52:39Z至05:52:42Z。

| 用例集合 | 条数 | Passed | 核验 |
| --- | ---: | ---: | --- |
| 原007B回归 | 961 | 961 | 958个不同fullname，Ordinal逐名出现次数及Passed次数与原XML一致。 |
| 008 CandidateRandomBindingTests | 58 | 58 | 全部为本次新增，行为断言对应六项验收。 |
| 020 CandidateCharacterGrowthTests | 111 | 111 | 全部为本次新增。 |
| 020 CandidateRecoveryTests | 64 | 64 | 全部为本次新增；020合计175。 |

新增恰233条且无其他新增fixture；本批1189个不同fullname。旧EndpointMismatch那个fullname仍出现4次，没有用去重集合替代多重集合。
旧961条Ordinal fullname+TAB+出现次数+LF规范摘要为 `a3d4d9f300cfd29699d623cf4bdca344ef0a8300ae688aa9fd86da9258892207`；旧XML SHA为 `9b0d12667df5a7bf3061377874efeb12ba1c694454446976ea50c763a8baf6e9`。
020的175条按Ordinal排序保留重复，以fullname+TAB+result+LF计算摘要为 `1a45eef7375744ff159edd88025b61bb06dad2a098d3dd960dcc2a02e6653e3c`，与D交付一致。

验证限制：R未重跑Unity，结论依据A本次真实运行及R对相同冻结输入/产物的独立核验；未运行PlayMode、Player构建或玩家端集成。
裁决不扩展至真实熵源/时间源、Published/H10、正式C参数、完整战斗机会调度、奖励/存档提交、卡证广告、其他职业或恢复反序列化。上述均非本包缺陷；008实际当前目标存活和事件唯一性仍由后续战斗调用方负责。
无需要最小失败反例或窄修的发现。报告与SHA交SD00登记，R停在B04-R1，不修改协调稿或派发后批。
