# 025-P2B-PUBLISH 正式交付

作者状态：COMPLETED。独立审查：PENDING；本报告不构成独立 ACCEPT、025 整体接收或完整 Demo 验收。
原 C task=01a0c403-bfa1-7e90-b503-c0fcd61f23c1；原实施及运行 turn=01a0d43a-d905-7780-a1aa-f1c979d9c751；本次续接完成 turn=01a0d63d-1349-79b0-b029-ec83a1cae552。
本次原生 turn_context=2026-09-25T01:45:24.374Z，gpt-6-astra / max，工程根=D:/Unity/UnityProj/FightMatch；§319 已外部登记准确 C/R 续接回合。
原回合实际 interrupted，未把旧进度视为正式完成。原 root、plan、001～008 运行及生产结果仍归原 turn；本次仅完成恢复核验、封存及首次正式两报告，新增 Unity 运行 0。
主包 §313～314 冻结 SHA256=39a2c272b5c55f51e71bb681e9340f5a819f5937d747b7816f914d2c4779d7b5；§316 精确键登记 SHA256=eb79d9ef626cbedfcb38a96562424fe986e99a7fafa115a16e0568bb0803fda1；§318 续接许可 SHA256=4f448697d77dd73149d63d992b7cc75023c9185e8643f372811b3c552342413d。

入口与恢复身份：

- [已接收 C2 交付](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-delivery.md)：15036 字节；SHA256=2e89e1cb138d8adce04785a6e02eeb9399d1088e46690743fe1c1ddb992280e1。
- [已接收 C2 完整范围](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-scope.json)：1564823 字节；SHA256=da2a19b47161f0cc7d9f2a5cff5ce96b93d8dcc67ce6f1c102f6fb1493993269。
- [§312 原 R 唯一 ACCEPT 报告](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-prepare-c2-code-review.md)：17780 字节；SHA256=4cd84dab8b5a3c84af7ae5df299c4e1741663138dd22ddc01222d05068a6ba9e。
- [唯一真实 content-review](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-content-review.json)：4766 字节；SHA256=16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75。
- [原根身份](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/root-identity.json)：692 字节；SHA256=a65fb7d567267636ff9fcacf9bb235105f296670e1e3d356f855ff746cf20891。
- [原首次八键计划](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/publication-plan.json)：12572 字节；SHA256=f6413092542b757dbc0c2b9ef81d8e268741b9a0ba2d367f3775dc31149af27c。
- [本次恢复链](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/resume-001.json)：12653 字节；SHA256=67f8cd71dc973c32ac39cc27b0372bb6af579371c8a3aff01df9c16a0d246673。
- [最终有限读取清单](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/read-manifest.json)：26390 字节；SHA256=a68653f38151031488cf800a73fd529557f2c0cf8e9e889d514ebe3d5776ff26。

原 C2 入口的 763 项导出中 760 项不可变内容曾逐一核准；本段最终仅两个准许既有文件更新，758 项仍与 C2 同字节，三协调稿取最新并核冻结段。续接再按 008 after 核 704 实现、734 Assets、36 DLL/PDB、9 内容、308 保护项和 17 发布路径（9 存在／8 不存在），零差异。
原 root 692 字节及 plan 12572 字节保持；plan 的首次 AWAITING_SD00_EXACT_PATH_REGISTRATION 状态保留，实际写入授权来自外部 §316 登记，不改写旧计划状态。

本段验收逐项结果（均为作者验证，独立裁决待原 R）：

| 条目 | 作者结果与证据 |
| --- | --- |
| P01 | PASS：真实 review 原 4766 字节不变。004／005 各为真实 Publish 构造同一 Job/Prepared，先对 C2 source/payload/validation 原字节及完整绑定，再调用既有 Publish/Catalog；规则、Codec、Catalog、Compiler 未变。 |
| P02 | PASS：004 实际 IsAccepted/Completed，七种持久记录；发布前后均 UnsupportedBinding/ReleaseSet。005 新进程同 operationId 成功，17 路径前后同状态/长度/SHA，无新记录或改写；原 publication-result 同字节。 |
| P03 | PASS：006 独立明确 activate-export，仅新增第八条 release 记录；release SchemaVersion=1 绑定原五字段与原收据 SHA。六件导出逐字节等于实际 store，原 operation/receipt/binding 三实体先核相等才共享 I。 |
| P04 | PASS：007 新进程仅读六件实物构造新只读 storage/Catalog，经 GetCurrentBinding 与完整 ResolveExact、level:ch01-01/1、298 字节新档核验。无原 source/store/Job 缓存补缺；只读、防修改及拒绝用例全绿。 |
| P05 | PASS：002 与 008 编译 exit0；003 最终 C# 全量 3597/3597、0 失败/0 跳过。003 至 008 的最终源码及 36 二进制一致；旧 3563 全名多重集合、测试体/断言不变。范围、自然 meta、旧证据及行预算全部核符。 |

精确内容身份与接口：

- operationId=publish:fightmatch-demo-r1:1；scope=player；releaseSetId=release-set:fightmatch-demo-r1。它们是发布身份，未创建正式 PlayerId、玩家 OperationId 或随机种子。
- draft=draft:fightmatch-demo-r1，revision=1；Binding=package:fightmatch-demo-r1 / b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 / demo-r1 / RC01 / PC01+SC01；DefinitionBinding=level:ch01-01 / 1。
- capabilities=ContentConsumerCapabilities.Current；每次 Prepare/Store 按批准边界提供独立 ExactMathBudget(maxPrimitiveSteps:16000000)，每件≤16MiB；六件合计≤32MiB。原 393 字段映射及 P1 参数矩阵继承已审证据，不重复求参或回放旧矩阵。
- 新唯一工厂：FirstReleaseContentStorage.Create(byte[] source, byte[] payload, byte[] validation, byte[] review, byte[] publication, byte[] release, ContentConsumerCapabilities capabilities, ContentStoreBudget budget) → PublicationResult<FirstReleaseContentStorage>。实现既有 IContentPublicationStorage，内部持有深复制字节，Read 返回深复制，AcquireWriter/WriteImmutable 明确 ReadOnly。
- 只读适配不承担 Unity、Android 或 PlayerSave 路径 I/O；从原 I/release 建准确八键，所有内容关系、规范编码、review/validation/收据、能力及回放准入继续由同一个 PublishedContentCatalog 完成。
- PublishedContentAuthoring 原 Prepare/Publish/Run() 公共签名保持；RunArguments(string[]) 是 Run 的私有参数解析抽取，新增测试仅通过反射触达该实际入口，不增第二公开启动器。未知/重复/缺参、错误 source/review/operation/release 身份在写入前拒绝。
- Run 保留 prepare，新增固定 publish、activate-export、verify-release 分支；工具新模式仅准新 Stage，固定根、实物、参数与调用，无任意输出根或命令开放。

实际运行（均为 2026-09-24 UTC、原实施 turn；精确 argv、PID、起止、ExitCode 及源码/资产/工具/36 DLL 前后身份见各 run.json、before/after/result.json）：

| Run | 模式 | PID | 开始 UTC | 结束 UTC | 实际退出码 | 结果 |
| --- | --- | --- | --- | --- | --- | --- |
| 001 | compile | 30560 | 09/24/2026 16:45:04 | 09/24/2026 16:45:13 | 1 | 保留失败 |
| 002 | compile | 33232 | 09/24/2026 16:46:59 | 09/24/2026 16:47:31 | 0 | 通过 |
| 003 | tests | 5976 | 09/24/2026 16:48:41 | 09/24/2026 17:00:38 | 0 | 3597/3597；0 失败/跳过 |
| 004 | publish | 6140 | 09/24/2026 17:02:36 | 09/24/2026 17:02:57 | 0 | 通过 |
| 005 | publish | 31648 | 09/24/2026 17:03:54 | 09/24/2026 17:04:14 | 0 | 同 op 幂等 |
| 006 | activate-export | 12540 | 09/24/2026 17:09:06 | 09/24/2026 17:09:23 | 0 | 通过 |
| 007 | verify-release | 5604 | 09/24/2026 17:10:05 | 09/24/2026 17:10:26 | 0 | 通过 |
| 008 | compile | 2448 | 09/24/2026 17:10:58 | 09/24/2026 17:11:06 | 0 | 通过 |

执行命令为 Tools/Invoke-FM025P2Validation.ps1 -Stage 025-P2B-PUBLISH -Mode <对应模式> -ExpectedScriptSha256 <该运行真实工具 SHA>。Compile/Publish/ActivateExport/VerifyRelease 使用 Unity -batchmode -nographics -quit；Tests 使用 -runTests -testPlatform EditMode，无 -quit、无筛选。实际绝对 argv 已逐运行封存，不用此展示形式重构命令。
001 的新私有 Math() 遮蔽旧 System.Math.Min，最终编译 pass 报 CS0119；初次导入还记录新文件未纳入的 CS0103/CS1503。仅把新 helper 改名 NewMath，002 起无编译错误。001 全部原日志、失败、身份保留。
001～003 工具为 16854 字节 / 977d6b4f4b8a4b3d6e85a2c5d73572573ccfc847859a7045ffdad19f069ceabe；全绿后仅修正 Capture 为 PUBLISH 才输出 publicationFiles 字段，以保留旧 Stage JSON 形状，未改 C#、argv 或测试参数。004～008 使用最终工具 16933 字节 / 95dc2628b5f7ddd3308b585ec3e4aba031bb275f67b38e9bab85bc2dd49cb9f3；各运行工具自身均未中途改变，最终 PowerShell 语法错误 0。
两份新 C# meta 在 001 首次自然生成 59 字节，002 完整导入后为 243 字节；最终文件前 59 字节与原 SHA 完全一致，GUID 保持。其余八份新首包/目录 meta 在 007 自然导入；未手写或重写 meta。
新 34 项覆盖准确八键与关卡/新档、输入/返回防修改、拒绝写/lease、六件逐一缺失/空值/篡改、I/release 顶层 null 和未知 schema、错收据/绑定、重新算哈希后的非规范 payload 或 source/payload 不符、预算/能力、无 fallback 和八类 CLI 拒绝。原 3563 项均通过，旧 Windows probe 仅保留既有精确 blob/lease 读回及幂等，不新建 probe。
最终 36 二进制与有效全量测试相同，因此 008 后及本次续接不重复全量测试。相对 C2 只有 FightMatch.Content 与 FightMatch.Core.Tests 各自 dll/pdb 共四项变化，其余 32 项不变；本次发布适配由本段 R 独立审，不冒称原 C2 R 已审新实现。

- [3597 项有效全量 XML](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/runs/003/tests.xml)：2523340 字节；SHA256=f7fa00373ea4178bbb6c8b3ca091ccd8c866a2314f9d3e7cf303f48b45a542e7。
- [完整验证与过程链](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/validation.json)：66064 字节；SHA256=c371d0a9279a119f0117a6953c50b76f97d23de49971277ef1b0a10856d0e4ee。
- [真实发布结果](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/publication-result.json)：2166 字节；SHA256=d6567d5bbce1466c43d39a7f27f8b13ab0c6af273453c036dfd54ee07033d068。
- [独立启用结果](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/release-result.json)：930 字节；SHA256=73bef93caf8e178e32789a3a549a79d6b25663755257048932bbecf6291d1129。
- [六件导出结果](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/export-manifest.json)：1519 字节；SHA256=498ec48f392b6f333b5570fbd5ada8709de6d6bb9be4b628c48971928adfba1c。
- [新进程冷装载结果](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2b-publish/cold-load-result.json)：3012 字节；SHA256=671cccea8b126abcff4067368cdda567884df3eb08e29e811398e187142f02be。

实际 store 根为 TestArtifacts/FMDemo025P2/p2b-publish/publication-store；最终仅八条 .blob 与零字节 writer.lock，八个 .work 均不存在，无未知文件或 reparse。004 新增七 blob+lock，005 新增 0，006 仅新增 release blob，007 新增 0，已有字节均保留。

| 角色 | 准确 key（.blob） | 字节 | 原记录 SHA256 |
| --- | --- | --- | --- |
| operation | 7b8acbc1fa35a17bfc6a0d91cda365950b6a18a84a0993e62bcd23feee447699 | 682 | feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910 |
| source | 650eb34931383b21667229e89a1425263ac630f69caaa54855017c2ab3d80647 | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| payload | 8a0e0e3670bd572145ba1cd91de5cbb94845cbe17b0c956d2b65375b5d1137d6 | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| validation | 1840e0f9165293b29c4079d93332232f322f35513de2afc2acd3d793075aa733 | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| review | 5a3bd91413ade67b7b12d04150b73a50e6af00809078133443cba32d9a3838d1 | 4766 | 16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75 |
| receipt | 52583ff605b83c6d1937084035712b5762af3a76a8932758676edb1bd268cd8e | 682 | feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910 |
| binding | ad761a08f736d147a3c6b8ba2499fb5864c387912f21d9bd20197c2401b422e7 | 682 | feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910 |
| release | 74353dfdcc4c823f20aaf05587adbe02291a6153f806cf4a906fc0356f1cdfb0 | 411 | 03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516 |

六件位于 Assets/StreamingAssets/FightMatch/，总计 38452 字节：S→source，P→payload，V→validation，R→review，原完整 I→operation/receipt/binding，独立 release-set→release。I 的三条实际记录各 682 字节，实体都保留；release 411 字节，收据哈希为 feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910。
007 的 verify-release 分支在构建实际磁盘 store 前返回；输入仅上述六件。Confirm(cold) 在输出结果前已实际 ResolveExact 完整绑定及 level:ch01-01/1；该定义核验由生产调用与 exit0 绑定，cold JSON 本身未另外序列化 DefinitionBinding。验证 wrapper 的元数据哈希不向 cold loader 提供内容。
冷装载冻结新档为 298 字节 / SHA256=36f2d8935f626c4bdafb53c56ac288f5199e7a6dec29680c0fac02f354685646；得到的是已审新档 DTO，未写 PlayerSave 或形成真实玩家会话。

范围与预算：

| 文件 | 本段增/删 | 合计或新物理行 | 上限 |
| --- | --- | --- | --- |
| Assets/Scripts/FightMatch/Content/PublishedContentAuthoring.cs | +163 / -4 | 167 | 240 |
| Tools/Invoke-FM025P2Validation.ps1 | +39 / -7 | 46 | 80 |
| Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs | 新增 | 57 | 200 |
| Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs | 新增 | 169 | 320 |
工具最终 219 物理行≤360，对最初工具累计增删 105≤160。旧 700 实现仅 Authoring 一项变化，另外 699 项保持；全部 308 保护项、旧 379 meta、旧 asmdef/规则/测试体保持。最终为 704 实现、734 Assets、389 唯一 GUID。

本段精确新增 Assets 18 项（2 C#、6 首包、10 自然 meta），完整身份如下：

| 路径 | 字节 | SHA256 |
| --- | --- | --- |
| Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs | 4510 | d5bce9e16a06635061ef11acfe67f097d62866fdf799e58c75d33dc94ae51724 |
| Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs | 14021 | 0be0c3a29169a62f725d829d7a15307cd5711b5cc3083910afac8d48664c4ca8 |
| Assets/Scripts/FightMatch/Content/FirstReleaseContentStorage.cs.meta | 243 | a333d4c5ac00a80409224a09e05193cfd6817efdce89b5f3761f06a49c76ae8d |
| Assets/Tests/EditMode/FightMatch/FirstReleaseContentStorageTests.cs.meta | 243 | 524215c5eac7d69cb58105e56b67459d62e0624ba20ef18b31bd48c3dfa7a291 |
| Assets/StreamingAssets/FightMatch/first-release.fmsource.json | 20443 | fc2a6e8ec7cdba10992b9e0de7433020b75b1a4ef2b18b5570b199b9983a261a |
| Assets/StreamingAssets/FightMatch/first-release.fmpackage.bytes | 11486 | b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129 |
| Assets/StreamingAssets/FightMatch/first-release.fmvalidation.bytes | 664 | 512b2f9b5027674dee1eed127e811a7b7c31322562f2dd81604d1faf1c79b38d |
| Assets/StreamingAssets/FightMatch/first-release.fmreview.json | 4766 | 16b282218b0652910535a2c6d70c736da1023d74c327e0492a399b5b8fd9fa75 |
| Assets/StreamingAssets/FightMatch/first-release.fmpublish.json | 682 | feb77ac4f21746130bdcdb3dc4a19702f92181e4a29182f86856b4781f2de910 |
| Assets/StreamingAssets/FightMatch/first-release.fmrelease.json | 411 | 03dd942a8ea3d4cbfa58ee7aba60768196f8811576550f204cf742f4e5c35516 |
| Assets/StreamingAssets.meta | 172 | 37940fda545b9ed965a05e680368c944f5ae4cf9ba1b7369290a3541369b6175 |
| Assets/StreamingAssets/FightMatch.meta | 172 | 1ccb2e12471fdbdc07bdbe29aa5e6829e5215803977780e14ab7a2b27dbc4a30 |
| Assets/StreamingAssets/FightMatch/first-release.fmsource.json.meta | 155 | 141930ad22c912aea637e2865f3d054f0e250f4e35704244a95be01ab532a19e |
| Assets/StreamingAssets/FightMatch/first-release.fmpackage.bytes.meta | 155 | cb57daf41a182baa41cfdfc92c90ed863e8d87a393e1c7e0d5e48c79093d30be |
| Assets/StreamingAssets/FightMatch/first-release.fmvalidation.bytes.meta | 155 | c2cb292729ef6910c6884fc4998353bb8b9119e15e3977101f75c357e1bdb8a3 |
| Assets/StreamingAssets/FightMatch/first-release.fmreview.json.meta | 155 | 4e7c85cacab1d524067261602ef9a64ad4a669ac5af8728a6f0a8375ccabb4a4 |
| Assets/StreamingAssets/FightMatch/first-release.fmpublish.json.meta | 155 | f3b5eeda61ecd87d919be3b90b916703a826c91982cf0aa7b88eb6e3b411d7bc |
| Assets/StreamingAssets/FightMatch/first-release.fmrelease.json.meta | 155 | db67ed0b3c015b1d936aee08a2d945281512c058527ace11a5b65b850ecb9e85 |

当前有限证据清单 116 项，连同不自包含哈希的 read-manifest 共 117 文件：28 根文件（含 resume 与 manifest）、2 entry 副本、20 final 副本、9 store 文件、8 次运行共 58 叶。仅两个旧文件有本段 entry 副本；final 副本仅它们与新增 18 项。
旧 B 1442、C1 1442、C2 75 个有限 manifest 条目及各自 manifest/root、旧报告/失败/恢复链与 C2 九件 authoring 输出均逐项核长度/SHA并原样保留；未把旧失败改成通过。最终 scope 导出 787 项：完整 C2 763 项加 C2 两报告、R 报告/真实 review、C2 根/manifest 共 6 项，再加本段 18 Assets；当前两报告由正式 final 外部绑定，避免自哈希。

交接范围：原 R 按 §§313～314、316、318 审本次完整 P01～P05 与恢复链，等待准确续接 turn 自然 completed、非异步正式 final 及两报告实物相符后给唯一 verdict。本段只交付首包发布与只读冷装载；P2C 玩家接入须在独立 ACCEPT 后由 SD00 另签精确包。
未执行 Player/Android 构建或真机、PlayerSave/正式玩家身份、页面/战斗入口、断电/崩溃矩阵、Git、额外任务/代理或自动跟进。025 整体、CONT-A/B/C、028/029、首 Demo 及 §184 NOT VERIFIED/物理输入/像素等既有未验证边界继续保留。
本机 Unity Editor 的八次进程证据不能提升为 Android 文件系统、断电可靠性或完整 Demo 的证明。

关联范围报告：[demo-025-p2b-publish-scope.json](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2b-publish-scope.json)
