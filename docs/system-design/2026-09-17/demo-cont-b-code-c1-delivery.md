# CONT-B-CODE-C1 实现纠正与验证交付

已完成 §354～358 授权的 R1～R4 纠正与规定验证。本报告是原 C 的实现证据；独立 R verdict 待原 R 在本次自然 completed 和 formal COMPLETED 后给出，不代填 ACCEPT。

- 原 C task：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次完成 turn：01a0e1bd-5b28-7bc0-bd42-f86b25c95fd9；gpt-6-astra / max；工程 D:/Unity/UnityProj/FightMatch。
- C1 根起点 turn：01a0e11c-7272-7e22-a9a1-4b3fbb24235b。根身份保持原样；001、004 环境失败及本轮 005→002→003 按各自真实 turn、PID、时刻、参数和退出码留存。
- 合同及差分起点：§354 的已审原 CONT-B 实现、§355 工具/9文件预算与证据补签、§356/358 的独立新槽；原 design、delivery、scope、root、manifest、R NEEDS_FIX 与全部旧失败证据只读。

## 修正与可复核回归

| 项目 | 最终行为 | 本轮证据 |
| --- | --- | --- |
| R1 | 显式用卡与完整 Held 的规范选择核对，广告→持久取得顺序→原行/单位优先不能绕过；显式 W 学习仍优先未完成教学用途的首证。保留合法相邻拆并、材料选择、Mage 独立普通证与说明后的用途释放。 | 005 的4项实际失败；003 的广告/新旧来源/首证负例与合法显式正例。 |
| R2 | 仅 Equip 写/读物品存在标记；明确空槽的历史状态为 ItemId=null、L=0、Enabled=null，保存/重建后重复为 Unchanged。其他6种 kind 的 Definition 仍必填。 | 005 的1项实际失败；003 空装配完整保存/重建和6项非Equip缺字段拒绝；原同物品L0/主动关用例保留。 |
| R3 | 仅 M03 按 CharacterId、OperationId 的 Ordinal 顺序严格校验读/写；不重排 M04/M05 的先后历史。 | 005 的2项实际失败；003 跨角色/同角色倒序拒绝、规范字节往返、M04/M05 非字典序先后正例。 |
| R4 | M03 的 Applied LearnSkill 恢复增加角色/技能业务唯一键，并禁止该新应用事实带 OriginalLearningOperation。普通 AlreadyLearned 必须引用原结果且无新输入/成本/产出或owner效果。 | 005 的1项真实完整M02/M03/M04一致重复学习反例；003 完整恢复与业务Prepare均拒绝、普通AlreadyLearned正例。 |

R4 保留原设计 §5 的合法教学补记：已在赠物前学习的角色可引用原学习完成尚缺的 M05 教学步骤，M03/M04 不新增成本或效果；说明后仍可明确选用其他普通证。新增 C1_R1_R4_OriginalLearningCanCompleteTeachingAndReleaseCertificatePurpose 保护这条既有行为。其正例与普通 AlreadyLearned 的零新增 owner 正例分别列证，不混为同一状态。

逐个实际 fullname、结果、失败信息/栈、用例方法和断言行号见 TestArtifacts/FMDemoCONT/cont-b-code-c1/case-evidence.json；源码原文和逐行diff见 scope。

## 必要运行与旧用例保留

| 槽 | 源码阶段 / 模式 | 实际退出码 | 结果 | PID / 耗时秒 |
| --- | --- | ---: | --- | --- |
| 001 | 未修复源码 / Tests | 199 | IPC连接失败；无XML，未取得代码失败证据 | 5540 / 60.365 |
| 004 | 未修复源码 / Tests | 199 | IPC连接失败；无XML，未取得代码失败证据 | 33968 / 60.313 |
| 005 | 未修复生产＋20新例 / Tests | 2 | 3863通过 / 8失败；旧3851全部通过 | 18228 / 4248.223 |
| 002 | 最终固定源码 / Compile | 0 | 通过；编译错误0 | 13632 / 19.311 |
| 003 | 同一最终源码 / Tests | 0 | 3872/3872通过；失败0、跳过0 | 42612 / 4002.247 |

- 005：无 filter 完整 EditMode。3871 个实际用例，R1/R2/R3/R4 的实际失败分别为4/1/2/1；其他新例12通过。失败断言仅按XML实际到达位置登记，不冒称在首个失败后的断言也已执行。
- 003：无 filter 完整 EditMode，Tests 未加 -quit。原3851条具名多重集合（3846不同fullname）全部保留并通过；最终新增21条另列，合计3872通过、0失败、0跳过。
- 原具名集合 SHA256：ce073c337744f59b40e3b28c70b9feca6ab9d37b31a46c69a5624daa25cf08f2。未删、跳过或改写旧测试/断言；三测试文件只有追加。
- 本轮没有重复旧CONT-B已通过的012/013；005是补足真实反例，002/003是修复后必须执行的最终同源验证。
- Hub 保持运行。005前记录了精确Editor/Hub/LicenseClient身份；对占用FightMatch的已确认Editor仅调用正常CloseMainWindow，没有强杀或关闭Hub。005在获准宿主执行下越过IPC；不据此断言001/004的唯一根因。
- 001/004环境失败完整保留。005的真实失败及failure.json同样保留；没有复用、清理或覆盖任一槽。
- 最终002 after＝003 before＝003 after＝当前交付的776实现、806Assets、工具及36项DLL/PDB身份；DLL/PDB仅校验文件元数据，未在Unity外执行产品DLL。

## 范围与预算

| 获准文件 | 增 / 删 | 合计 / 上限 | 最终物理行 / 上限 |
| --- | ---: | ---: | ---: |
| Assets/Scripts/FightMatch/Core/CandidatePermanentInventory.cs | 14 / 0 | 14 / 60 | 293 / 330 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentProgression.cs | 6 / 3 | 9 / 12 | 155 / 160 |
| Assets/Scripts/FightMatch/Core/CandidatePermanentCodec.cs | 12 / 2 | 14 / 32 | 259 / 275 |
| Assets/Scripts/FightMatch/Core/CandidateBusinessRestoreChecks.cs | 3 / 0 | 3 / 12 | 721 / 728 |
| Assets/Scripts/FightMatch/Core/CandidateApplicationReferences.cs | 4 / 1 | 5 / 24 | 699 / 712 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentInventoryTests.cs | 86 / 0 | 86 / 180 | 481 / 575 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProgressionTests.cs | 99 / 0 | 99 / 120 | 243 / 264 |
| Assets/Tests/EditMode/FightMatch/CandidatePermanentProtocolTests.cs | 197 / 0 | 197 / 240 | 534 / 577 |
| Tools/Invoke-FM025P2Validation.ps1 | 16 / 5 | 21 / 24 | 282 / 300 |

- 工具累计：原173＋本轮21＝194，在197补签上限与200绝对上限内；原入口、SHA自检、进程安全及异常证据协议保留。
- 776实现中768项不变；806 Assets中798项不变；425 meta/GUID逐字节及唯一性保持。885保护输入中，9个明确变动例外和3份协调稿单列，其余873项保持。
- 原CONT-B 219项实物以及7个继承根的4897项实物按原有限manifest逐项核验不变；首包六实物与298字节建档配方继续保持。没有新增源码、meta、配置、依赖、资产、任意脚本、公开API、Git动作或内容发布。
- 初始72路径storage-plan保持原字节；§356与§358分别只补入004与005十二叶族，最终允许96路径。只写真实产生项：本根76个文件，manifest列75项并排除自身；正式报告仅本交付和scope两项。
- 协调稿仅允许其原有独立写者推进，按实际before/after差异单列；C不改写三协调稿。

## 固定入口与身份

- C1 root：TestArtifacts/FMDemoCONT/cont-b-code-c1/root-identity.json；751 bytes；SHA256 b744740e3646f8d27701240ca1524673c2b71fbaa2d502ba098637c86ffaf822。
- C1 read-manifest：TestArtifacts/FMDemoCONT/cont-b-code-c1/read-manifest.json；17267 bytes；SHA256 fcd9105703acd260594d09ffb80d1309af192e2088c72de17463f15966295f02。
- 新正式scope：docs/system-design/2026-09-17/demo-cont-b-code-c1-scope.json；含9文件准确diff和原文副本引用、预算、起点/续接身份、全部5槽、具名集合、保护及同源链。两份新正式报告的最终身份在本次formal外部给出，scope不自写自身哈希。

## 验证边界与下一步

真实首包仍无卡、技能/证书、配方及16关教学定义；继续按原契约返回NoPublishedDefinition。正例使用既有合法隔离定义、原M07普通发物、共用永久操作/M02规范路径及M12内存保存/对象重建；广告仅验证已有合法持有证明的共用消费核，不形成真实广告发放或新M10发布。

公开AwaitLinks、物理鼠标/触控/像素/交互式PlayMode、Player/Android、未覆盖强保存故障和§184原NOT VERIFIED保持原边界。内存测试不称物理崩溃或真机验收；本C1不推进CONT-C、028、029，不改变34/30/4/51计数，不恢复自动跟进。

提交本次非异步formal COMPLETED后停止修改，由原R核准确自然completed及本两份正式实物，再独立审本C1增量和必要回归，给唯一ACCEPT/NEEDS_FIX/REJECT。
