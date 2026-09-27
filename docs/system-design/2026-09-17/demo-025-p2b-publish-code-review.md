# 025-P2B-PUBLISH 独立代码与交付审查

VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: P01～P05 PASS。Verification: 实际代码/diff、批准输入、有限证据、全部首包字节、原生完成门、3597 项全量 XML、八次进程与最终同版链独立核符。未发现未解决的实质问题。

本结论只覆盖 §§313～314、316、318 签定的本机首包发布、显式启用及只读冷装载，不接收后继 P2C 或 025 整体。审查依据为 AGENTS.md、.agent/REVIEW_CHECKLIST.md、.agent/VALIDATION.md 和当前准确任务包；以实际补丁和实物核验为准，不以作者 PASS 或 SD00 收件代替独立判断。

## 1. 准确回合、续接和完成门

工程根 D:/Unity/UnityProj/FightMatch。C 原任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1；R 原任务 01a0c1cd-dce1-7ac3-8780-06163cb0acfc。两者续接原生 context 均为 gpt-6-astra／effort=max／本工程根。

| 身份 | 准确 turn | 原生 context UTC |
| --- | --- | --- |
| C 原实施及 runs/001～008 | 01a0d43a-d905-7780-a1aa-f1c979d9c751 | 2026-09-24T16:23:41.692Z |
| R 原审查准备，中断 | 01a0d43b-d33f-75a3-b205-c29c3f42726b | 2026-09-24T16:24:45.712Z |
| C 本次续接完成 | 01a0d63d-1349-79b0-b029-ec83a1cae552 | 2026-09-25T01:45:24.374Z |
| R 本报告所属续接 | 01a0d63d-8ea7-7c53-91e9-e3761cb7a69d | 2026-09-25T01:45:56.463Z |

R 先核两旧中断 turn 无 task_complete，未将首包出现或最后 commentary 当作完成。旧 R 只做入口/键计划准备；本次完整代码、测试、运行与结果审查在恢复后的 C 完成门满足之后进行。未继续等待已中断旧 C 回合。

C 本次 task_started=2026-09-25T01:45:22.028Z；非异步 formal final 于 02:02:44.014Z 明确 COMPLETED，原生 task_complete 于 02:02:44.151Z，last_agent_message 与 final 一致；应用状态 completed／idle／error=null。R 于 02:03:10.3070675Z 核以下两件与 final 完全一致，才进入完整独立审查。R 不预填自身完成时间；本报告实际长度/SHA 由本次 final 外部绑定，准确 R task_complete 由原生事件提供。

| 作者正式实物（D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/） | 字节／行 | SHA256 |
| --- | --- | --- |
| demo-025-p2b-publish-delivery.md | 18474／127 | 1dcf21bfcba712edb8ff6e954abf2d73bb9c44bdd515ea7ec3d2df05057b6f5b |
| demo-025-p2b-publish-scope.json | 1736578／39728 | 692f345f1e1cc6697967ba648192bb399293d46706d94835c88aaa12f882492d |

冻结范围重新规范化核符：§313～314=39a2c272b5c55f51e71bb681e9340f5a819f5937d747b7816f914d2c4779d7b5；§316=eb79d9ef626cbedfcb38a96562424fe986e99a7fafa115a16e0568bb0803fda1；§318=4f448697d77dd73149d63d992b7cc75023c9185e8643f372811b3c552342413d。三协调稿读取最新状态；§319 准确派发、§320 作者收件不改变冻结权限。

原 root-identity.json 692 字节／a65fb7d567267636ff9fcacf9bb235105f296670e1e3d356f855ff746cf20891，publication-plan.json 12572 字节／f6413092542b757dbc0c2b9ef81d8e268741b9a0ba2d367f3775dc31149af27c 保持原实施 turn、首次时间和待登记状态。实际物理授权来自 §316 外部登记，未为续接改写原计划。

resume-001.json 12653 字节／67f8cd71dc973c32ac39cc27b0372bb6af579371c8a3aff01df9c16a0d246673，低于 16384 上限，准确连接原 C/R、恢复 C、原生 context、§318、八次旧运行及未完封存工作。独立复核其中 34 个保留引用均与现存实物一致；未倒填运行所属 turn，未冒充新运行。

2026-09-25T02:17:47.8299780Z，R 以原 runs/008/after.json 再核当前 704 实现、734 Assets、36 DLL/PDB、9 内容、308 保护项、17 发布路径和工具，存在性/长度/SHA 零差异；17 路径为 9 存在、8 不存在。续接新增 Unity 运行 0，009～012 未使用，保留同版有效验证符合 §318。

## 2. 实际范围、接口和预算

独立比较 C2 合法入口、两个 entry 副本、当前文件和 20 个 final 副本。原 700 实现只有 PublishedContentAuthoring.cs 变化；新增两份 C# 及其 meta 后为 704。另有两目录 meta、六首包及六对应 meta，共精确 18 项新增 Assets；最终 734 Assets、389 唯一 GUID，原 379 meta 无变化，无额外资产或重复 GUID。

| 改动（下列代码路径均相对工程根） | 最终字节／物理行 | SHA256 | 独立预算结果 |
| --- | --- | --- | --- |
| Assets/Scripts/FightMatch/Content/PublishedContentAuthoring.cs | 22651／244 | 7862e6913c701be05dade97becd5d550acdcadb406593ad6949cc554a40778eb | 163 增＋4 删＝167／240 |
| Tools/Invoke-FM025P2Validation.ps1 | 16933／219 | 95dc2628b5f7ddd3308b585ec3e4aba031bb275f67b38e9bab85bc2dd49cb9f3 | 39 增＋7 删＝46／80；累计 96 增＋9 删＝105／160；219／360 |
| Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs | 4510／57 | d5bce9e16a06635061ef11acfe67f097d62866fdf799e58c75d33dc94ae51724 | 57／200 |
| Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs | 14021／169 | 0be0c3a29169a62f725d829d7a15307cd5711b5cc3083910afac8d48664c4ca8 | 169／320 |

入口两件分别为 6900／3edd3570bbfa884b61d7940b74271b35f1bfd946abd8765c98b2de2733334cb2 和 13281／92de02e9e37c2629320ae0d461463bf820301e9bdd9996b5891e6e41f5a98a84，已与 C2 及 entry 副本核符。没有修改 Catalog、Codec、Compiler、Models、Core、Platform、旧测试、asmdef、依赖或序列化格式；308 保护项保持。

新增公开契约仅 FirstReleaseContentStorage.Create(byte[] source, byte[] payload, byte[] validation, byte[] review, byte[] publication, byte[] release, ContentConsumerCapabilities capabilities, ContentStoreBudget budget) → PublicationResult<FirstReleaseContentStorage> 及既有 IContentPublicationStorage 的只读实现。工厂接收字节，未承担 Unity/Android/PlayerSave 路径 I/O。原 Authoring.Prepare、Publish、Run 的公开签名保持。

完整导出独立从原 C2 的 763 项推导：加 C2 两报告、R 报告、真实 review、C2 root/manifest 共六项，再加本段 18 Assets，精确 787 项。784 不可变项实际长度/SHA 全符，只有三协调稿按最新状态处理。没有以新 scope 遗漏旧保护项或把未知差异作为入口。

## 3. P01：批准内容、真实凭证与实现

Authoring 的固定 CLI 解析拒绝未知/重复参数、缺参数、错误 mode/source/review/operation/release；根和祖先检查 reparse，精确常量/路径在写入前校验。Publish 分支使用同一真实 Job 的 Prepare 结果调用原 Publish API；与批准 source、C2 payload/validation、真实 review 原字节逐项比较，再处理实际 Completed 结果。Windows storage 构造本身不创建发布根，写入从既有排他 lease/API 开始。

真实凭证仍为 D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-content-review.json，4766 字节／16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75，未改写或格式化。该文件保留已接收 C2 的准确 C/R turn 和 19 属性结构；这是正确的内容批准来源，本段新适配代码由本报告另审，不能将旧凭证的 turn 翻成当前 turn。

已接收 C2 R 报告 17780 字节／4cd84dab8b5a3c84af7ae5df299c4e1741663138dd22ddc01222d05068a6ba9e 保全。批准映射 da286b0a01f2bd06c74bef915422f94d76e99b33d00683b33fb3389b5949c9f6、393 叶映射、31C／ε、几何/回放及 C2 顶层 null 修正按未变字节继承；未重跑原 P1 求参矩阵，也未再次额外语义读取 docs/game-design/balance/config.json。

只读工厂先拒绝空件/过限/无能力或预算，再深复制六数组，以原 Codec 解 publication/release 并构造准确八键，调用同一 Catalog.GetCurrentBinding 准入。规范载荷、source/payload 关系、review/receipt、版本及能力约束继续由原实现严格执行。Read 有界并返回复制；未知键无数据；WriteImmutable、AcquireWriter 返回明确 ReadOnly 拒绝。没有新增绕过验证、latest 回退或从文件名猜指纹的通道。

## 4. P02～P03：八键、真实发布、幂等和独立启用

R 在旧准备回合先独立推导键，核 §316 后才观察实际写入；当时 store 和 StreamingAssets 均不存在。本轮再次以 UTF-8(kind + NUL + identity) 的 SHA256 独立计算，与 plan、登记、实际路径及六件对应全部一致。binding identity 为准确五字段规范 JSON 的 e4ad9ac0c7e8d9e3cfb66bb75187e9dcaa68a5b012e8d7d80adf7f86f31b2d04；release identity 为 scope/id 规范 JSON 的 4958d7a4d6a1de0365af3f71101941bbca4a666f262d272ac0e3078ff578e9b0。未执行产品 DLL 求键。

| 角色 | 准确物理 key（publication-store 下加 .blob） |
| --- | --- |
| operation | 7b8acbc1fa35a17bfc6a0d91cda365950b6a18a84a0993e62bcd23feee447699 |
| source | 650eb34931383b21667229e89a1425263ac630f69caaa54855017c2ab3d80647 |
| payload | 8a0e0e3670bd572145ba1cd91de5cbb94845cbe17b0c956d2b65375b5d1137d6 |
| validation | 1840e0f9165293b29c4079d93332232f322f35513de2afc2acd3d793075aa733 |
| review | 5a3bd91413ade67b7b12d04150b73a50e6af00809078133443cba32d9a3838d1 |
| receipt | 52583ff605b83c6d1937084035712b5762af3a76a8932758676edb1bd268cd8e |
| binding | ad761a08f736d147a3c6b8ba2499fb5864c387912f21d9bd20197c2401b422e7 |
| release | 74353dfdcc4c823f20aaf05587adbe02291a6153f806cf4a906fc0356f1cdfb0 |

004 实际调用 Publish 得 Completed：七记录从不存在转为存在，writer.lock 为 0 字节，release 不存在；发布前后 GetCurrentBinding 均 UnsupportedBinding。005 是不同 PID 的同 operation=publish:fightmatch-demo-r1:1，仍 Completed，全部 17 发布路径存在性/长度/SHA 不变。源码、资产和二进制也无变化，证明为保留原结果的跨进程幂等。

006 单独执行 activate-export：先从真实发布库 ResolveExact，再编码固定 scope=player、releaseSetId=release-set:fightmatch-demo-r1，持 lease 写独立 release，读回后 GetCurrentBinding 成功；仅 release blob 与六首包从不存在变为存在，原七记录不变。导出使用真实存储读回，已有输出必须逐字节相同，否则拒绝；无覆盖既有 blob/首包或换 operation/root 的路径。

三份独立存在的 operation/receipt/binding 原始记录均为 682 字节且逐字节相同，才映射到首包一份完整 I；不是凭 receipt 模型重新序列化代替原字节。I 的 Binding、OperationId、DraftId、Revision="1"、SchemaVersion=1 和四个原件 SHA 均核符；release 的五属性及 PublicationReceiptSha256 指向该完整 I。

| 六件（D:/Unity/UnityProj/FightMatch/Assets/StreamingAssets/FightMatch/） | 字节 | SHA256 |
| --- | --- | --- |
| first-release.fmsource.json | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| first-release.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| first-release.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| first-release.fmreview.json | 4766 | 16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75 |
| first-release.fmpublish.json | 682 | feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910 |
| first-release.fmrelease.json | 411 | 03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516 |

六件合计 38452 字节，均小于单件 16MiB/合计 32MiB。R 对八实体到六件逐字节比较；S/P/V/R 再与批准 source、C2 prepared 两件及真实 R 凭证逐字节比较，全部一致。实际 store 只有八 blob 和零字节 lock，八个 .work 均不存在，无额外键或 reparse。

## 5. P04：新进程冷装载与只读边界

007 使用独立 PID 5604 和 verify-release argv，没有 -fmStoreRoot/-fmSource/-fmReview。实际 Authoring.cs:122～132 分支只读取六个固定首包内容，构造新的 FirstReleaseContentStorage 和 Catalog，并在创建 Windows 发布库的第 134 行之前返回。未用原 source 内容、publication-store 内容、Prepared/Job 缓存或测试 fixture 补缺。

该分支取得固定 scope/id 的 current 后调用 Confirm；Confirm:183～189 对 package:fightmatch-demo-r1／b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129／demo-r1／RC01／PC01+SC01 执行 ResolveExact，并用完整 DefinitionBinding(level:ch01-01, LevelVersion="1") 取关卡。任何一项不符即失败，不只比较一个指纹字符串。

Confirm 同时核新档为冻结 298 字节／36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646。R 将 cold-load-result 的实际数组与 C2 descriptor 数组逐字节核符，并核其六 ContentInputs 的实际路径/长度/SHA。冷结果没有单独序列化 level 字段；整关检查证据来自该真实分支的强制 Confirm 调用、最终同版代码和该过程实际 exit0，没有将不存在的 JSON 字段当证据。

OnlySixFirstReleaseFiles、SourceRead=false、StoreRead=false 已由实际调用路径支撑，不是单信输出布尔。验证工具的 before/after 哈希采集和 Unity 导入会访问工程元数据；这里证明的是产品冷装载内容来源仅六件，不声称整个 Editor/取证进程从未接触其他文件。证据结果写入有限根不属于补充内容输入。

## 6. P05：真实过程、全量回归及最终二进制

全部运行由原 C 在 Unity 2022.3.18f1 串行执行。R 逐份核 run/result/before/after、argv、PID、processStartUtc、起止、actualExitCode、工具与源码/资产/DLL 身份；过程无重叠，边界 Unity 列表均为空。退出码采集来自 WaitForExit 后进程 ExitCode，不由日志成功文字推断。

| run／模式 | PID | UTC 起止（2026-09-24） | 实际 exit／结果 |
| --- | --- | --- | --- |
| 001 Compile | 30560 | 16:45:04.5479929～16:45:13.2324555 | 1，初次失败保留 |
| 002 Compile | 33232 | 16:46:59.8742748～16:47:31.6547931 | 0 |
| 003 Tests | 5976 | 16:48:41.6777550～17:00:38.2774379 | 0，3597／3597 |
| 004 Publish | 6140 | 17:02:36.3842562～17:02:57.1170558 | 0，七记录 Completed |
| 005 Publish | 31648 | 17:03:54.0548121～17:04:14.5926302 | 0，同 operation 幂等 |
| 006 ActivateExport | 12540 | 17:09:06.3026651～17:09:23.8477437 | 0，独立 release 和六件 |
| 007 VerifyRelease | 5604 | 17:10:05.5362888～17:10:26.0962939 | 0，仅首包冷装载 |
| 008 Compile | 2448 | 17:10:58.2379330～17:11:06.4284381 | 0，最终导入/编译 |

准确 argv 保留在八个 run.json：固定 Unity executable/projectPath/logFile；Compile 为 -batchmode -nographics -quit；Tests 为 -batchmode -nographics -runTests -testPlatform EditMode，固定 results/log，无 -quit/filter/category；三生产模式通过 -executeMethod FightMatch.Content.PublishedContentAuthoring.Run 和批准的固定 -fm 参数执行。工具仅固定 Stage/模式/根，拒绝旧 Stage 的新模式，原 Prepare 权限保持，没有任意命令/输出根。

003 tests.xml 为 2523340 字节／f7fa00373ea4178bbb6c8b3ca091ccd8c866a2314f9d3e7cf303f48b45a542e7。R 独立解析每个 test-case：3597 Passed、0 failed、0 skipped；与已接收 C2 XML 的 fullname 多重集合逐项比对，原 3563 全部保留并通过，新增 34 全部位于新测试类，旧断言文件未改。

新增覆盖真实隔离发布 fixture 的六件八键/完整关卡/新档、所有输入和返回数组防修改、只读 lease/write 拒绝、六件各自缺失/空/篡改、receipt/release 的根 null/空白 null/未知 schema/错误关系、重新哈希的非规范 payload 与 source/payload 不一致、记录/数学/读取预算与能力不足、显式 scope/id/version 无回退，以及八类 CLI 拒绝并核实际 17 路径未写入。未将 isolated fixture 当成真实玩家运行。

001 failure.json 与 result.json 原字节相同；日志包含 Math() 新 helper 遮蔽 System.Math 的 CS0119，以及首轮导入时 CS0103/CS1503。001→002 的唯一源码修正为 Authoring，最终 NewMath 消除遮蔽，002 后全量成功；未删除或改绿初次失败。

两个新 C# meta 首次 59 字节，002 自然导入补全至 243 字节；当前前 59 字节分别仍匹配 001 的 40b8ce8a465f4035f1d7274fd1b610f1cd616839e7e5e4f3bbd43916c4795473、5f2679d8a63da17361d3f93e9401988ed7f900cad4282eb268e5eb896a04cc39。其余八个新 meta 在 007 导入自然产生；旧 meta 没有改变。008 确认最终资产和程序集稳定。

003→004 只有工具从 16854／977d6b4f4b8a4b3d6e85a2c5d73572573ccfc847859a7045ffdad19f069ceabe 变为 16933／95dc2628b5f7ddd3308b585ec3e4aba031bb275f67b38e9bab85bc2dd49cb9f3。R 在内存中将最终 Capture 的条件 publicationFiles 字段还原为原无条件字段，重建文件准确命中上述原长度/SHA，独立证明只涉及旧 Stage 的采集 JSON 兼容性，无 argv 或 C# 改动；PowerShell AST 无解析错误。

003 前后至 008 前后及当前，704 实现和全部 36 DLL/PDB 同版；各相邻过程的源码/资产/内容/保护/发布/DLL 也仅出现上述已解释变化。相对 C2 只有以下四个二进制变化，其余 32 个长度/SHA 保持，不能误称仍与 C2 全部同版：

| Library/ScriptAssemblies 下最终文件 | 字节 | SHA256 |
| --- | --- | --- |
| FightMatch.Content.dll | 153600 | 3578b050e03d299d94f453ea14b9744dee70f1afcb4103d918cac84b9274dabb |
| FightMatch.Content.pdb | 47056 | f9e88f10eaeff1c8ac12322147a249c6fa552dbef091c13365e2718f83ea6879 |
| FightMatch.Core.Tests.dll | 1221632 | 78e7c950026671a5d18fbec51a8131912e92d2f4a353ea4ffdb515b889725aed |
| FightMatch.Core.Tests.pdb | 358596 | dbac6a9cfac561479eaa6c5da8bc48cfa9552d11a42551640792cd672c7b8351 |

最终源码/DLL 未再变化，故沿用 003 有效全量结果符合 §314.2；恢复后不重复 Unity 验证。R 本轮未运行 Unity、产品 DLL 或修补实现。

## 7. 有限证据封存和继承

PUBLISH 根为 D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish。read-manifest.json 26390 字节／a68653f38151031488cf800a73fd529557f2c0cf8e9e889d514ebe3d5776ff26，116 项与 scope.evidence.items 一致；含 manifest 本身共 117 个实际文件。R 根据任务包的闭合路径集核无缺失/额外/重复/越界/reparse，逐件长度/SHA 一致。组成：28 根文件（含 resume/manifest）、2 entry、20 final、9 store、8 次运行共 58 叶。

| 关键实际结果（上述根下） | 字节 | SHA256 |
| --- | --- | --- |
| publication-result.json | 2166 | d6567d5bbce1466c43d39a7f27f8b13ab0c6af273453c036dfd54ee07033d068 |
| release-result.json | 930 | 73bef93caf8e178e32789a3a549a79d6b25663755257048932bbecf6291d1129 |
| export-manifest.json | 1519 | 498ec48f392b6f333b5570fbd5ada8709de6d6bb9be4b628c48971928adfba1c |
| cold-load-result.json | 3012 | 671cccea8b126abcff4067368cdda567884df3eb08e29e811398e187142f02be |
| runs/008/after.json | 497107 | a0a274d30346abb3e4bc01b02cd271c2a4700e4f43220467f0843edb554836c5 |

2026-09-25T02:18:13.4194822Z，R 完成旧 B/C1/C2 的重新保全核验：先核旧 scope、root/manifest 身份，再核描述符 items 与各原 scope/manifest 完全一致，最后按有限列表核每件实物及物理成员集合，无重解析点、长度/SHA 差异或新文件。

| 继承根 | manifest 项／含 manifest 的物理数 | manifest SHA256 |
| --- | --- | --- |
| p2b-prepare | 1442／1443 | 9ef1d23b548f11477971f0c1c37ec63f7a9108cb61f07f4a0e4e12ec0c4a8ea8 |
| p2b-prepare-c1 | 1442／1443 | c5981de520324999eeb00e87ab2e093ce467671505f11f55fe0d713f78b3776c |
| p2b-prepare-c2 | 75／76 | 4323c10a7a3a66f882f93d6724e0dd0386d0e6058f66ab95c8621bacd65ccd25 |

原失败、C1 旧独立问题报告、C2 九件 authoring、UNREVIEWED 模板、真实 content-review 和旧 B io-probe 的零字节 lock／4 字节 blob 均保留；实际有限物理集合中没有旧 probe .work。旧创建/Flush/提升证据按原 B 继承，本段未冒称重做 probe 创建或更强故障保证。

## 8. 独立结论的边界

P01 批准内容/凭证及接口、P02 真实发布/七记录/跨进程幂等/未隐式启用、P03 显式 release/六件原字节映射、P04 全新只读冷装载/完整绑定、P05 编译/全量/元数据/范围/历史保全均满足。本报告没有待修问题，不生成扩大范围的纠正包。

本轮 R 唯一写入为此新报告；未修改代码、工具、资产、meta、真实 review 或旧报告，未写 Git，未新建任务/代理/自动跟进。当前报告不自包含自身 SHA；作者两报告及本报告分别由各自准确 formal final 外部绑定。

后继 P2C 正式玩家接入仍须 SD00 另签。PlayerSave、正式玩家身份、应用页面/战斗入口、Player/Android 构建与真机、断电/崩溃矩阵、物理输入/像素、§184 NOT VERIFIED、025 整体/B17、CONT-A/B/C、028/029 与首 Demo 的既有未完成边界保持；34 功能／28 已接收／余 6／51 正向交付不因本子阶段独立通过自行变化。
