# CONT-C-MAC-R1 独立代码审查

VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS
Verification: 独立读取实际累计补丁、最终源码、测试断言、原始 XML/日志、运行回执及有限清单；013 编译成功，015 无筛选 EditMode 3956/3956 通过，0 failed/inconclusive/skipped。
Notes: 本结论仅接收本包导航、恢复编排和已签 Mac Editor 适配；Android APK、真机、实际宿主与物理交互仍未验证。

- R：`01a0e404-e8ee-7310-8388-9260babd53f1` / local；准确审查 turn：`01a0e79b-546f-7672-9d5b-cd3015901b56`。
- C：`01a0e404-d89d-7ab2-bece-3cd1df3fbc52`；准确最终 turn：`01a0e6d3-38f8-7b92-9c3c-6a3df6b258fe`，已直接核 completed/error=null，startedAt 1790578997、completedAt 1790591819。
- 已直接读取 C 的正式 final `msg_0d179092dafe07fa016aba4343295087d0b21cacf236efbeb7`，声明 COMPLETED；该声明不作为代码正确性的替代证据。
- 审查授权为 §430，3814 UTF-8/LF bytes，SHA256 `23a6d9dd2a87a18403d9af723659f28720b716e44031f9adae32686f052ec4c1`。规范同时包含 C1、§367/374/380/383/386/394/395/417/418/420/425/428；scope 所列 12 段冻结正文均独立重算核符。
- 实际比较基线为 `ce21901b7b5b42bfef7ef34eb4f46155ddbf9353`，未把 WIP HEAD `adaffdc4d7e8329fb4e8ee663ed8918c78ad8f0c` 当已接收源码。只读 Git 对象/差分用于累计范围核对。
- 规范与需求两条审查线由本 R 顺序完成；未创建聊天或代理，未运行 Unity、产品 DLL 或新测试，未执行 Git 变更。唯一写入为本报告。

## 正式交付身份

以下身份均针对实际文件重算；报告与 scope 在审查中未修改。

| 文件 | bytes | SHA256 |
| --- | ---: | --- |
| `demo-cont-c-mac-r1-delivery.md` | 18470 | `8c033f90615c9be394178a32c327e043091ab1b933fdbb8892d17f5e8de0eb01` |
| `demo-cont-c-mac-r1-code-scope.json` | 5303941 | `972947ab4d7d41bd385d16c60eb928b3ee6c83328101dd809b3a43e8622614d9` |
| `Tools/Invoke-FM025P2Validation.ps1` | 45816 | `33a99396c2ccf39ced3c1fdf25fcec27824e9bb08719849b239d47ce165222e9` |
| `TestArtifacts/FMDemoCONT/cont-c-mac-r1/runs/015/tests.xml` | 2936492 | `940e5393444d96013c25a8d8d68a1f6f1f685188c9b088ebe42441e0241d6c2a` |
| `TestArtifacts/FMDemoCONT/cont-c-mac-r1/runs/015/result.json` | 1572 | `53b507fbbcf7c6d00c48b1dbd0a5d90aff68016712c184a230d08752cf1d773d` |
| `TestArtifacts/FMDemoCONT/cont-c-mac-r1/runs/015/tests.log` | 1737073 | `384bb49386b4cdd403ebaedaae1f0c3ae8d3b9037c6c41e23fa6502bb22eb45a` |

## 规范、边界与累计差分

未发现需要纠正的越界文件、业务分叉、保存协议变更或无关重构。新增类型保持在原 Application/Presentation/Platform 与既有测试程序集内；导航复用同一 PlayerSession 和原 Prepare/Submit/Query/Retry/Resolve/End，视图仅管理输入、订阅和显示。

实际 808 实现中，已接收 776 项为 766 项原字节保留、10 项准确获准修改，另新增 32 项。实际 Assets/Packages/ProjectSettings 的已跟踪差异及未跟踪新文件均落在签定集合内；未发现其他产品或配置差异。

| 旧文件（省略共同目录） | 相对已接收基线 + / − | 核对结果 |
| --- | ---: | --- |
| Application/CandidateApplicationRuntime.cs | 19 / 0 | 仅新增 internal QueryResumedIntent；Guard、PlayerSave、同 owner、CreationPending、精确 pending ticket/commit/operation 门完整，无新意图或业务写。 |
| Content/PublishedContentAuthoring.cs | 3 / 0 | 仅 Mac 下提前拒绝与冻结 CLI 参数不一致的输入；未开放 Mac 内容发布。 |
| Tests/LocalSaveStoreTests.cs | 19 / 9 | 固定 Mac I/O 根与实际存储选择；原故障注入及断言保留。 |
| Tests/SaveCommitMarkerTests.cs | 3 / 3 | 替换物理后端选择，原名称、参数和断言保留。 |
| Tests/SaveRecoveryTests.cs | 8 / 7 | Mac 路径/后端及必要别名；原恢复故障路径保留。 |
| Tests/SavePendingRecoveryTests.cs | 2 / 2 | 原故障包装接既有 ILocalSaveStorage。 |
| Tests/CandidateApplicationRuntimeTestData.cs | 2 / 2 | 原夹具选择实际后端。 |
| Tests/CandidateApplicationRuntimeTests.cs | 2 / 2 | 原实际存储探针选择后端。 |
| Tests/CandidateLifecycleTestData.cs | 1 / 1 | 原夹具选择实际后端。 |
| Tests/PublishedContentCatalogTests.cs | 4 / 3 | 原精确字节探针在 Mac 隔离目录运行，租约、冲突和回读断言不变。 |

表中生产文件位于 `Assets/Scripts/FightMatch/`，Tests 位于 `Assets/Tests/EditMode/FightMatch/`。每项差分均在各自签定额度内；`FirstReleaseContentStorageTests.cs` 等其余旧实现未变。

新导航生产 1180 行、测试 1254 行、13 个 meta 合计 143 行；13 个 cs 均未超各自上限，每个 meta 11 行。新增 Mac 存储/发布/测试分别 181/69/178 行，三个 meta 各 11 行。旧 425 个 GUID/meta 全部原字节保留，新增 16 个 GUID 后共 441 个且无重复；Mac 起点已自然生成的四个导航 meta 亦与 before-text 完全一致。

验证工具相对冻结 282 行起点为 +225/−8、最终 499 行、累计 194+233=427，满足最后签定的本轮、累计和最终行数额度。已读完整最终工具及原文差分：Mac 固定路径、SHA/owner/包身份、真实进程检查、串行门、Compile 同版门、014 精确诊断与 015 全量门均保留；旧 Windows stage 的固定命令和判断路径保持。恢复工具 226 行、固定 Mono runner 75 行，分别在 240/80 行额度内。

Packages 相对基线仅包含 §380 冻结的 `com.unity.toolchain.macos-x86_64-linux-x86_64` 2.0.11 及对应 lock 项，依赖原 sysroot 2.0.10/linux-x86_64 2.0.9；两文件 SHA 核符。未将它解释成 Android 构建或发布许可。

008 after→009 before 的源码差异准确只有 §418/420 签定的 Session、Recovery、PermanentFlowTests、SessionTests、PresentationTests 五文件；原 008 全部具名测试保留，最终新增 16 次。单行 UI 返回期望修正未移除确认、旧按钮或保存断言。

## CC01～CC30 与恢复补签

以下为本 R 对实际源码和断言的判断。简称 Session/Recovery/Query/Models 指新增 Application 文件，SessionTests/PermanentFlowTests/RecoveryTests/PresentationTests 指对应新增测试文件；所有列出的测试见证均已逐项对应最终 015 Passed 记录。继承不变的业务内核与旧矩阵通过原 3872 次回归及累计差分核对，不宣称本次重新审查全部历史 Core 实现。

| 要求 | 独立核对与结论 |
| --- | --- |
| CC01 同源边界 | 通过。Query、Session 只消费原 PlayerSession/Application；SessionTests:33–47 核唯一 session、首包 L1、298 字节新档、真实三槽及零业务写；正式六件首包未变。 |
| CC02 创建/F2 | 通过。原 Guard/CreationPending 优先；记录已写、头已成未确认及 snapshot/marker 故障均不能新写或 End。SessionTests:16–31、312–326；未弱化旧 F2 矩阵。 |
| CC03 同头纯查询 | 通过。Query:18–46 使用当前头的 For 精确绑定并复核前后头；不读取 latest 或取时间/ID。SessionTests:33–57 核只读与已下载 V2 不替换 V1。 |
| CC04 精确关卡 | 通过。Session:123–129 校验 Binding/LevelId/LevelVersion；UI 列出实际目录和 progression。错误版本拒绝，选择不执行入场。 |
| CC05 宿主交接 | 通过。明确操作产生不可变 HostRequest，旧上下文/回调拒绝重复；NoReadyMember 仍只作入场检查提示。SessionTests:59–103、PresentationTests:91–109；无 H02/时钟采样。 |
| CC06 三槽阵容 | 通过。本地值冻结且可全空，重复/未知角色拒绝，成功准备后才原 Submit。SessionTests:76–114、PermanentFlowTests:315–327 核一次提交、同步重入及原结果重查。 |
| CC07 背包/显式角色 | 通过。余额/各角色 L/活动 carry 来自原查询；无角色可读余额，装配必须明确角色。空装配 null/0 与指定物品 L0 不被合并；新断言及未改旧永久矩阵共同覆盖。 |
| CC08 真实空内容 | 通过。真实首包无配方时显示 NoPublishedDefinition、无 recipe 按钮；实际 PlayerSession Preview 拒绝，未注入隔离定义。PermanentFlowTests:54–65、PresentationTests:25–49。 |
| CC09 Craft 输入 | 通过。生产 BuildPermanentDraft:163–173 保留目标/批数/份额，Craft 清 CharacterId；同一个实际 Draft.Copy() 进入合法隔离核，报价角色/ClassId 均空，3+1→6 与明确来源份额实际核定。PermanentFlowTests:67–135。 |
| CC10 Back/Cancel | 通过。Confirmation 返回首次编辑页；单次 Back 只退一层，Cancel 返回子流锚，旧 token 不再退层。Team 装配结束恢复 Team 后，其后阵容/迁移使用外层 Preparation 或 Map；六种迁移发起页、完整父链和关卡版本均有断言。 |
| CC11 一次确认 | 通过。Models 的 TryConsume→AcceptPrepared→Receive 及 Session:226–247 先保留原 request 再 Submit；重复/重入/外 owner/旧 epoch 不重办。Craft 输入验证先于隔离确认槽断言。 |
| CC12 失效/冻结 | 通过。头、角色、修订与报价替换使旧 token 失效；重复 Preview 保持原编辑页，失败或空替换退出孤立确认页。已确认后查原操作；后移头不重新报价。六入口测试及集合调用后修改断言有效。 |
| CC13 原结果/返回 | 通过。HasVerifiedResult 同时核 IsCommitted、原 Lookup 和已核 lookup head；原 commit 与当前头分列。Return 清槽一次并重查；Preparation/Bag/Team 及连续返回断言保留。 |
| CC14 Prepare/Write 失败 | 通过。原 request 在提交前留存；无 ticket 的 PendingPreparation 仍可原 Retry，明确写失败保留候选/操作/字节。RecoveryTests:13–54 及 SessionTests:252–285。 |
| CC15 未知结果 | 通过。marker 前/后故障沿原 Resolve，只有原系统确认未成后才重试；最终断言原 commit、operation 和候选字节不变。未把内存故障称真实进程崩溃。 |
| CC16 页面重建 | 通过。controller/view 只解绑自身；同 PlayerSession 保存 session、原请求和返回锚；旧 callback epoch/已脱离按钮无效果。PresentationTests:51–89 和迁移恢复重绑测试。 |
| CC17 完整对象重建 | 通过。RecoveryTests:60–106 的 helper 仅返回磁盘身份/字节，重建丢弃旧请求；显式 ResumeObserved 再失败/未知后经生产桥接取回同一个原 intent，后续 Retry/Resolve/End 不重新 Prepare；路由丢失回 Map。 |
| CC18 原操作优先 | 通过。按准确 record.Intent 查询，历史 commit 与后移 lookup head 分列；FindCommitted 按所选 commit 找唯一记录，不使用最后一条。RecoveryTests:137–149 与 PermanentFlowTests:251–265 核零重复提交。 |
| CC19 End/owner | 通过。End 需独立确认，Cancel 只关提示；F2/S17/其他 owner 不接管。Ended 后仍依据真实恢复状态留在 Recovery；普通 Query 不采纳其他拥有者 pending。RecoveryTests:152–197。 |
| CC20 活动战斗 | 通过。阵容、新入场、Equip 保留原活动门；合法永久业务仍走原核。SessionTests:167–191 核活动中 v3→v4 不改原 BattleBytes 与装配拒绝；合法制作继续按已接收隔离内核范围核验。 |
| CC21 演出 token | 通过。导航生产代码不依赖 Close/Dispose/ReportPresentationCompleted；PresentationTests:111–135 核同一 token、Starts、CompletionReports 在菜单生命周期前后不变。真实宿主叠层留后续。 |
| CC22 S17 | 通过。保留 Continuation，明确 SettlementRequired 交接一次；End 拒绝，零 H06/结算副作用。PresentationTests:138–156。 |
| CC23 明确迁移 | 通过。仅相邻 2→3/3→4 明确预览和确认，禁止跳级；原创建锚及 v4 IsMaterialized 保持。SessionTests:150–165 与六入口、后续迁移返回和恢复测试共同覆盖。 |
| CC24 错误/信任门 | 通过。导航沿原 Ready/Guard/精确发布；WrongThread/Disposed/Busy 不修改上下文或取回 intent，预算拒绝不写盘。未新增 builder、catalog 或 PlayerSave 注入入口；旧用途/错误绑定/任意 builder 拒绝矩阵完整保留。 |
| CC25 通知失败 | 通过。原 IsCommitted 真时继续呈现原结果，不把 NotificationFailure 当可重办业务；关闭后重绑仍得原 commit/通知诊断。RecoveryTests:199–216。 |
| CC26 UI Adapter | 通过。已读四份实际 Presentation 源码；精确目标/完整来源键绑定、整数输入和诊断均经生产 controller。实际 UI Toolkit panel 回调覆盖地图、三槽、空内容、确认/旧按钮；证据未冒称物理点击。 |
| CC27 有界状态 | 通过。最多五层页面、单一确认槽、三槽固定、集合和规范整数预算；无 float/clamp/无限请求队列。负数、非规范值、大数和过量来源拒绝；纯转移 seam 不创建业务结果或授保存权限。 |
| CC28 继承回归 | 通过。原 XML 的 3872 次/3867 不同 fullname 多重集合逐项保留且全部 Passed，八个旧测试文件的参数/断言未削弱；原内容、Core 和序列契约不变。 |
| CC29 宿主/Android | 通过本包静态与回调范围。HostRequest 为后续宿主的精确只读交接，无 Android 业务分支；实际场景、APK、设备输入与关闭重开不在本包完成声明内。 |
| CC30 范围/验证 | 通过。最终完整 3956 次通过；源码→DLL→验证→交付同版，有限证据并集、预算、原失败与历史身份逐项核对，见下节。 |

RCV1 另核：Runtime:133–150 仅返回原 pending.Intent 引用，输出前全部拒绝门保持 intent=null。RecoveryTests:108–135 实测 wrong owner/commit/operation、WrongThread/Busy/Disposed/F2；普通导航 Query 不调用该桥接。该桥接只在导航自身显式 ResumeObserved 的匹配结果后使用。

## Mac 平台与冻结输入

| 要求 | 独立核对与结论 |
| --- | --- |
| MC01 | 通过。MacEditorSaveStorage 按原 UTF-16 code unit 逐字节 SHA256，区分 surrogate/purpose；规范绝对路径、Ordinal 与本地卷门保持。 |
| MC02 | 通过。实际 Unity Mono flock 与 Python 子进程双向争用，只有 Darwin EWOULDBLOCK 映射 Busy；释放后重新取得且 writer.lock 留存为零字节。015 XML 含子 PID 28269～28272 的实际握手/exit0，与 probe-summary 原输出逐字一致。 |
| MC03 | 通过。实际 00 01 02 03 发布、同字节幂等/异字节拒绝、CreateNew、提升冲突保留双方字节、仅本实例真实 FileStream 可 Flush(true)；未扩大 FaultModel。 |
| MC04 | 通过。祖先、叶及 dangling symlink 在创建/读取/删除前拒绝；ENOTDIR 等非缺失 I/O 不解释成 absent。实际链接见证和旧真实文件系统故障测试均保留。 |
| MC05 | 通过。八个旧测试适配和一处 CLI 例外均对已接收基线逐 hunk 核对；所有旧具名测试保留，不使用 Ignore/Explicit 或成功替身。 |
| MC06 | 通过。12 项源输入的当前与 staging 身份、4 个原生成器源码及 Compile003 快照、7 个获准程序集的历史身份与实际 loader 输出核符。Mono 编译 PID30655、runner PID30660 均 exit0，四条唯一有序 COMPLETED 标记齐备，afterChecks 为空。 |
| MC07 | 通过。原3872＋新增84＝3956，全通过；014 的单例诊断未混入完整验收计数。 |
| MC08 | 通过。808/838/441/36 集合、保护输入、有限证据与独立 I/O 预算核符。 |

已读恢复工具完整固定 Main/反射调用路径：只加载冻结程序集和四个原生成方法，无 REPL、断言绕过或替代生成器；输出先核原长度/SHA，再 CreateNew 回填。四项当前目标及 staging 实物分别为 source-ledger.json 34768、prd-candidates.json 96954、replay-evidence.json 12528619、replay-cohort.json 10458 bytes，均与冻结原 SHA256 完全一致。该恢复证明属于 Compile003 的历史链，未要求因后续导航修正重跑生成器。

## 最终验证、清单和历史保全

013 result 为实际 Compile exit0；014 为唯一冻结 fullname 的 1/1 Passed 诊断、exit0。015 原 run.json、日志命令行与结果均证明完整 EditMode 无 filter/category/assembly 筛选且 Tests 无 `-quit`。实际环境为 Unity 2022.3.18f1、x86_64/Rosetta、macOS 27.0 (26A428)，不以 Windows DLL 字节或历史绿测代替本次运行。

015 Unity PID26925、actualExitCode=0；R 直接读取 C 回合中包装执行 `exec-c5034ee2-3dea-41d0-90df-d5d3bc6751be` 的 completed/exitCode=0，包装耗时 5025708ms。Unity 窗口 09:11:14.170896Z～10:34:45.541878Z、5011.370982 秒；XML 自身 4979.6116068 秒，两者按各自含义记录。

原 XML 3872 次/3867 不同名与最终 XML 3956 次/3951 不同名均实际解析；旧多重集合缺失 0、非通过 0，新增 84 次。008 的 3940 次全部保留，新增 16 次。最终完整 fullname 多重集合 SHA256 为 `4d92efa98eefb19e1f97ecb2435e605595a89b6a4e76bf150656c5b7dd1bddcd`，与报告逐项一致。

独立重算确认 `013 after = 014 before = 014 after = 015 before = 015 after = current`：808 实现、838 Assets、36 DLL/PDB、15 内容及 6 运行文件，路径/存在性/长度/SHA 零差异。441 GUID 清单亦逐项重算。当前平台记录/协调文档的更新与代码同版链分开，不虚称 platformInputs 的所有文档从头至尾完全不变。

主根原 root-identity 的156路径不变，三个冻结扩展的24+24+36项形成精确240并集；实际157文件，read-manifest列156项、排除自身，无多余或缺漏叶，各项长度/SHA零差异。read-manifest 为33662 bytes，SHA256 `5d31e0992e975d9b8f2034636523b258eccfa3921ad0bfaa81c2f4f9dd28b5dc`。

平台元数据精确64路径与冻结 Mono C1 plan 一致；实物64，manifest列63项、排除自身，长度/SHA零差异。manifest 为15955 bytes，SHA256 `9a8b8d106e42d0687f723eadef3627c4a1c3c43ea93792a7c2326de5b350b500`。七份冻结平台/运行扩展计划全部核符。

平台 io/ 单独以 lstat/scandir、不跟随链接重新清点并核文件 SHA：2605 个合法小写32位GUID case、7516目录、12247文件、15链接，与19778条原 inventory 完全一致；低于4096 case/40000文件及链接预算。未混入64项元数据清单，也未删除探针或故障证据。

925项保护清单当前差异仅三份已签 SD00 协调稿；额外 Mac 上下文的差异仅 START_HERE。126项迁移历史证据全部核符。4个恢复输入与新源码按独立角色登记；原15项历史缺失仍实际缺失，不将它们冒称恢复。

§425 保存的001～010历史身份59项、§428 保存的001～012历史身份74项均逐件核符。001预启动采集失败、002真实编译失败、004的3个UI关闭日志失败、005控制面中断、006 Bad CPU 未启动、010主机重启中断、012 Unity exit134 及无XML均保留；最终成功未覆写它们。002约6小时12分异常启动窗口不算作正常测试计算；010未知退出/终止时刻不以重启时间替代；012资源读取异常未推断成已证实磁盘损坏。014只作诊断，015才作完整验收。

## 残余边界与收件

未发现可行动的阻塞问题，无需纠正包或无关复测。可按本包准确白名单作为一个接收单元处理；如日后回退，保留历史失败/测试 I/O/冻结输入及独立 SD00 协调记录，不用清理证据或重置整个工作区代替范围回退。

本报告不证明正式已发布配方→PlayerSave制作整链（当前首包没有合法配方）；实际生产 Draft.Copy→合法隔离核与真实 NoPublishedDefinition 是两类不同证据。物理鼠标/触控/像素、交互式 PlayMode、场景宿主、设备 Back/生命周期、APK 构建安装、iQOO Neo5/BlueStacks 试玩和首 Demo 关闭重开仍留028/029。

真实 OS 进程崩溃矩阵、掉电持久性、非合作写者阻断、公开 AwaitLinks、原15项缺失历史路径及 §409 已消失 Finder 临时副本仍未验证；不制作替代物或追加本包范围。本 R 未执行提交/推送，也未启动028/029。准确 R 回合自然结束及正式报告齐备后，由 SD00按§430核完成门并收件。
