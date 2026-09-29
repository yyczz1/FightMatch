VERDICT: ACCEPT

Scope: PASS
Acceptance criteria: PASS（§441 指定的 028 当前适用范围；验证边界见末段）
Verification: 原006全量4010/4011＋修正后该项1/1，产品未变；R独立核对源码、被测版本、XML、清单及实际文件。
Notes: 未发现需修正的实质问题。没有新的完整4011通过记录；029正常宿主、真实Player进程冷启动、Android试玩和公开默认参考Link正例未验证。

审查日期：2026-09-29（Asia/Shanghai）；实物核对在2026-09-28 UTC完成。
R chat：`01a0e404-e8ee-7310-8388-9260babd53f1` / local。
准确R turn：`01a0e913-10ba-7ec1-9cf6-f635569dfd92`，startedAt `1790616735`。本报告随本回合最终返回，不提前声称该回合已completed。
仓库根：`/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`。

**审查入口与方法**

依据AGENTS、START_HERE、PROJECT_CONTEXT、REVIEW_CHECKLIST、CODING_RULES、冻结§434/§440/§441、两份计划和028设计，顺序完成规范与规格审查。审查整个当前028补丁及必要的既有调用链，不把FIX1的一行修正当成全部工作。
仅新建本报告；未运行Unity、测试、构建或产品程序集，未作Git操作，未修改产品、测试、工具、证据或协调元数据，未创建代理/聊天或发送跨聊天消息。
以批准的基线`14f0bd76776a211ed588561691616b3523f73395`对应的冻结before-text、前序已接收CONT-C清单和当前实物作比较。七旧生产文件及工具的完整before-text逐件匹配计划长度/SHA；028的808/838/441/36前清单与已接收CONT-C scope及其after清单相同。本轮没有重新执行Git diff或旧基线测试。

直接读取C聊天回合元数据及正式final：原实施turn `01a0e7e1-94a9-76c3-89cc-3a00f910b2c7` 已completed/error=null，startedAt1790596715、completedAt1790615593；正式消息`msg_0d179092dafe07fa016abaa02280d487d08063fb1b80fb2522`返回BLOCKED。
FIX1 turn `01a0e905-d341-77f0-b763-cca2ba6d8fc4` 已completed/error=null，startedAt1790615868、completedAt1790616600；正式消息`msg_0d179092dafe07fa016abaa4112fa087d095140580d3d43fb8`返回COMPLETED。§441据此开放独立审查，不把作者完成声明视为代码验收。

**冻结输入身份（R实算）**

下表文档位于`docs/system-design/2026-09-17/`；章节按标题至独立结束标记、UTF-8/LF及一个尾LF计算。

| 输入 | bytes | SHA256 |
|---|---:|---|
| system-task-packets.md §434 | 13458 | 08988b13282b1dafa4fcc0a095b66721a0a6bd98c79532c57d9543da1e11d06c |
| system-task-packets.md §440 | 5440 | 63ad15f2628770aa0bc97a59b2be8721da8284cb8d4f871ffd59453572d6927a |
| system-task-packets.md §441 | 3647 | 2ee0b102f98ec139f0b2cba18669290acdafe90b588a6e4bf46ebf3de8be8b46 |
| demo-028-mac-design-draft.md | 25827 | 3b2eb4e8863ebc0cc8842f98c425520af78c8df5ddd765242e7f6f8a513a0b39 |
| demo-028-mac-r1-plan.json | 205569 | 037692abd5c262d7cd4efa40dcc93a2d74235fcd9f02c6c46e83ae5ea8725fe6 |
| demo-028-mac-r1-delivery.md | 20995 | 90576580eb216d04bbe5dd3539549c67ad996a14e1b1ee58e9aab03e1486a145 |
| demo-028-mac-r1-code-scope.json | 932632 | bc437b9eec5d484402ea1586c03d1b02dbde0d0384cb30d11532ba2eaf22f06a |
| demo-028-mac-r1-fix1-plan.json | 19630 | eb37ed78e7f12f871750ae148716deaf647d86516a2cf102f335c980e630d2c6 |
| demo-028-mac-r1-fix1-delivery.md | 2368 | 63d22486f88a8528013f573905454451403b60af961e848aaaf38405f1932c9a |
| demo-028-mac-r1-fix1-code-scope.json | 10788 | f6abca48442496e03da77a5f9dc70a5b1bc3c800e6161ee02f5044ced3750363 |

**范围与规范检查**

当前Assets实际文件成员恰为批准的858项，无额外或缺失成员；实现清单828项全部存在。相对前序808实现/838Assets，仅七旧生产例外改变，新增五生产、五测试及十自然meta。旧441份meta字节保持；当前451个GUID全部存在、唯一，十新meta各11行，低于13行上限。
933保护输入逐件核对：15项历史缺失仍缺失，除批准的源码/工具及五份SD00协调元数据外无漂移。Packages、ProjectSettings、asmdef、Core算法、序列化与存储实现、正式内容文件均保持。额外核对既有`Tools/Restore-FMDemoMacInputs.ps1`，仍为前序接收的16405 bytes / SHA c38bb8102703729a59cdbe022dcb7cc5198674bc94b1d901364d5784d64b0da7。
028原作者范围为28个源码/meta/工具项和两份交付报告；FIX1仅增加其批准的两份报告和有限证据。原root的28份after-text逐件等于当前源码/meta/工具；不是只检查文件出现或作者自报。

| 旧文件（均在批准路径） | 实算新增/删除行 | 增删上限 | 当前行数 |
|---|---:|---:|---:|
| Content/PublishedContentModels.cs | +6/-2 | 36 | 238 |
| Content/PublishedContentCatalog.cs | +1/-1 | 24 | 187 |
| Presentation/CandidateBoardInputView.cs | +46/-10 | 60 | 112 |
| Presentation/CandidateBattlePlaybackView.cs | +33/-7 | 90 | 114 |
| Presentation/CandidateBoardElement.cs | +17/-4 | 75 | 182 |
| Application/CandidateBattleApplicationSystem.cs | +25/-0 | 56且禁止删除 | 204 |
| Presentation/CandidateBoardInputController.cs | +23/-2 | 90 | 266 |
| Tools/Invoke-FM025P2Validation.ps1 | +114/-8 | 200 | 605≤699 |

七旧生产路径前缀为`Assets/Scripts/FightMatch/`。五新生产PlayerBattleSession/Models/Controller/View/PlayerDefaultReferenceView依次329/138/135/153/134行，总889≤1690，各自预算均满足。五新测试Fixture/Flow/Recovery/Presentation/DefaultReference依次230/195/222/167/156行，总970≤1730，各自预算均满足。
逐段diff符合最小范围：没有新增伤害、随机、回退、奖励算法或泛化层；新生产代码没有UnityEditor引用，既有Application/Presentation/Content程序集边界保持。旧文件LF风格保持，新增diff行无尾随空白，未夹带重命名或全文件格式化。
验证工具只增固定DEMO-028-MAC路径和校验分支；逐段核对原Windows及CONT-C分支在新stage=false时保持原行为。六槽限制、独立根、CreateNew、进程检查、SHA自检、Compile→Tests版本一致、Tests无filter/无quit、完整旧用例多重集合及继承I/O容量检查保留。最终工具57515 bytes / SHA 32b5cfac5c830e043af32f91be32946432b2fe5eb999e35e62a2ecf8bd86fed9；保留的AST记录exit0/0 parse errors对应同一工具，R未再执行它。

**B01～B12逐项审查**

以下测试简称均指`FightMatch.Core.Tests`下新测试类；除明确指出FIX1的唯一目标，其余引用用例在原006 XML中实际Passed。已阅读五份新测试的完整断言及必要的旧接口，不以用例名称替代逻辑核对。

| 条款 | 源码与实际证据 | 结论 |
|---|---|---|
| B01 | PlayerBattleSession:122核同一navigation owner/revision/HostRequest引用/head/binding并记录已消费revision；H02由PrepareFormationEntry→原Lifecycle.Submit办理。Flow的HostSelection、StaleHeadAndRootBack、DueRecovery三例核外来/重复请求零写、续局无新入场、RootBack，以及NoReadyMember到期恢复在真实H02中完成。 | PASS |
| B02 | 新PreviewHistoryRollback:114核Ready/已核头/expected commit/有效anchor/实际Rollback准入，调用旧ReadRange。input保留原Range，ConfirmRollback(expected)核引用及原上下文，重预览仅作一致性检查，提交字段来自原Range。Flow实际跨面选择→确认→提交恢复原成员/敌人/RNG；Presentation真实列表与旧确认按钮用例通过。旧单项松开提交用例B16B05仍Passed。 | PASS |
| B03 | Session:196～225冻结退出/重来种类及完整Context，取消不Prepare；确认重新核准入，先保留OriginalRequest再Submit。Flow的取消/旧弹窗、Exit无新Attempt、Restart保留成员成长/原Baseline/Challenge及三随机初态、无新H02和无时钟读取均实际通过。结果重打另走新H02。 | PASS |
| B04 | Session:86、227及页面:60、69使用实际Battle.QueryView的PresentationToken作结算门禁；时钟和PrepareVictory在门禁之后。Flow首次Playback.Finish Changed内重入仍PresentationPending且零时钟/零写，原token解除后沿ReservedOperationId办理H06；Presentation核待结算/已到账文案及按钮。Recovery的持久WPS对象重建、原S17续办和通用保存失败/未知恢复证据覆盖办理路径；本轮不改旧播放回报顺序。 | PASS |
| B05 | Session:165～194核当前lookup依据头、原canonical intent/operation/commit；ReceiptMatches使用OriginalLookup.Baseline与End，不再错误要求胜利TerminalRun。Models:37～73按原参与槽/CharacterId投影Reward、CharacterExperiences、CharacterEnds、InventoryGrant、End；三成员C/A/B及倒下/未倒下断言通过。H06移除ActiveHistory后页面仍显示原收据，HUD另取最新已核头。 | PASS |
| B06 | 生命周期请求由Session、攻击/补线/回退请求由原input持有。Recovery的四种生命周期故障与Presentation三种攻击故障均销毁重建整个视图树，核原引用、规范字节、候选与计数不变；完整Application重建从所选原commit调用ResumeObserved，再由PlayerSession窄桥接取同一原intent，无重Prepare/私有状态注入。已提交响应丢失从保存Record.Intent→QueryOperation取原收据；FIX1完成原S17重建用例末段全部断言。 | PASS |
| B07 | 创建未Active交宿主原创建；Continue在End前检查Continuation/SettleVictory，F2与S17不会变成新建档或新入场。owner/revision/head、kind、原intent引用、候选commit/op均沿原准入核对；原QueryResumedIntent仍检查PlayerSession owner、PlayerSave、主线程/busy/disposed及准确票据身份。具名拒绝/重入测试通过，FIX1核迁移拒绝、End/导航无写及原S17 Retry后仅一次奖励。 | PASS |
| B08 | Session:282的OpenOriginalResult从当前核定头按精确operation找原Record.Intent，再QueryOperation，核原lookup/current-head身份；不Prepare/Submit。NotificationFailure已提交查询不重发，重复Query/Retry不重播。完全重建无内存请求、回地图再开、后续再次胜利后重开旧收据、缺失/错kind/错owner/旧Context用例通过。 | PASS |
| B09 | PlayerBattleController持有同一Session/Input/Playback，页面借用Bind/Detach；普通Detach不Dispose控制器、不清原请求、不RebuildLatest，ClosePresentation显式Skip，最终宿主Dispose一次。旧Attach仍建自有控制器并保留接管语义。按钮owner/epoch、播放调度owner/generation及参考回调generation均核对；真实Panel旧按钮、整树重绑、未变化刷新保留按钮/Context、旧页/宿主Dispose、token只报告一次等测试通过。 | PASS |
| B10 | PublishedContentCatalog:133仅在原完整binding/source/payload/validation/evidence/review检查成功后传入原built.Replays；Models:216用方法返回只读列表，无新增反射序列化属性。精确内容指纹与六件发布文件保持，错binding/level/version拒绝；条件列出原槽/角色/等级/技能/携带/偏好/00..2f种子，当前差异与当前局面分析未接入分别标注。真实Resolve和不可变集合断言通过。 | PASS |
| B11 | PlayerDefaultReferenceView:62～112核精确binding/level/version、当前commit/Attempt/Scene、Face/尺寸/配对端点与颜色、无真实token/播放后才设置独立ReferenceOverride；不触及真实保存或token回报。隔离参考实际验证原路线、未杀消线、击杀固化与全程业务零写；真实Panel首个Dragging关闭参考且同一pointer成功提交攻击；错面、真实播放互斥及旧按钮/调度失效用例通过。公开参考Link正例的边界见后文。 | PASS（现有参考适用范围） |
| B12 | Session:296、310及Models:96只使用当前发布Levels与OpenFacts；返回地图/队伍/背包复用同一PlayerSession导航，重打走原入口。三个返回目标、原结果重开、合法新H02、拒绝虚构下一关、L1无下一关和无配方空态实际通过，无编号推导或Demo捷径。 | PASS |

上述关键实现可定位于[PlayerBattleSession.cs](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerBattleSession.cs)、[PlayerBattleModels.cs](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Scripts/FightMatch/Application/PlayerBattleModels.cs)、[PlayerBattleController.cs](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/PlayerBattleController.cs)、[PlayerBattleView.cs](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/PlayerBattleView.cs)、[PlayerDefaultReferenceView.cs](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Scripts/FightMatch/Presentation/PlayerDefaultReferenceView.cs)。

**验证历史与当前版本归属**

原根为`TestArtifacts/FMDemo028/mac-r1`，补充根为`TestArtifacts/FMDemo028/mac-r1-fix1`。下表按实际执行顺序排列；Unity退出码与工具判定分开。

| 根/槽 | 模式、Unity PID | Unity exit | 实际结果 |
|---|---|---:|---|
| 原001 | Compile 12635 | 1 | 新测试六处编译问题，失败保留 |
| 原003 | Compile 13369 | 0 | 工具判失败：新自然meta首次完整导入改变字节；后续仅修正批准的新meta Compile例外 |
| 原004 | Compile 13744 | 0 | 成功，供原002使用 |
| 原002 | 无filter完整EditMode 13888 | 2 | 3996/4008，12失败、0跳过；旧3956均通过 |
| 原005 | Compile 15156 | 0 | 修正后成功，供原006使用 |
| 原006 | 无filter完整EditMode 15296 | 2 | 4010/4011，唯一失败、0跳过；旧3956均通过 |
| FIX1/001 | Compile 19157 | 0 | 成功，仅测试DLL/PDB自然重编 |
| FIX1/002 | 精确单项EditMode 19284 | 0 | 1/1 Passed，0失败/跳过 |

读取真实run/result/日志/XML：原完整Tests argv无testFilter且无quit，Compile带quit；FIX1的单项filter严格等于批准的`FightMatch.Core.Tests.PlayerBattleRecoveryTests.B06_B07_RebuiltS17CandidateKeepsReservedIdAndCannotBeEndedOrNavigatedAway`，不是替换成另一个测试。各运行保留PID、UTC、真实Unity退出码及启动前进程检查；90分钟采样只是当时运行诊断，不代替终态证据。最终Compile和Tests日志未发现编译错误/Unhandled Exception标记。
R从XML重新计数：前序3956个出现/3951个不同fullname全部保留；原006共4011个出现/4006个不同fullname。新增55个出现分属Flow14、Presentation13、Recovery18、DefaultReference10；原006新增54通过、1失败。未删除旧用例、忽略重复名称或把跳过算通过。
原006名称多重集合按排序fullname、LF、尾LF重算SHA为`62bb86c64c2d204dabbf441293ae2687db952b467ca5b6ec848dfdb290df938a`；旧3956对应SHA仍为`4d92efa98eefb19e1f97ecb2435e605595a89b6a4e76bf150656c5b7dd1bddcd`。
原006 XML：2981076 bytes / SHA `a60fcb79d970b1b51357ef412e66fe89f792012703e50dc8b377137be6e962e5`。FIX1 XML：3432 bytes / SHA `a1a26f787b84e6baf0c780c902946afff76cd569e825617f5e0701b248254489`，唯一test-case实际45.603062秒。

**唯一失败、修正与复用充分性**

原006确实在[PlayerBattleRecoveryTests.cs:123](/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PlayerBattleRecoveryTests.cs:123)空引用：S17候选完整对象重建后再次marker失败，此时没有当前已核head，测试使用`head.Header.CommitId`，迁移拒绝及之后Retry/收据断言尚未执行到。不能把这些末段断言算作原006已通过。
修正仅把该参数改为`resumed.OriginalIntent.ExpectedCommitId`，取已恢复原意图的真实预期头；既不重造意图，也不减少或放宽任何断言。当前文件18640 bytes / SHA `6e75264ab8e4e9b25b03ec7e277f0ed38c7d646d66386b2836ed44b2b23512b9`。R将唯一精确替换在内存反向还原，得到18621 bytes / SHA `ad89842114c2ed87f74c0dc32946c9d12dbd54ca4cfa8d1c61441ed2fefdb7b8`，与原006被测源码完全相同。
FIX1 XML的1/1通过，加上无其他测试源码变化，证明迁移拒绝、head/files/clock零变化、原S17 Retry、原收据、原ReservedOperationId、仅一份BaseReward及Continuation清空的原断言依序执行完成。该测试没有提前返回、条件跳过或替换期望结果。

R逐件独立比较并重新hash当前实物，得到以下版本链：

1. 原005.after＝原006.before＝原006.after的828源码、858Assets、36DLL、6运行文件及工具身份；原006期间仅五份批准的SD00协调元数据变化。
2. 原006.after→FIX1 Compile.before，源码/Assets差异集合严格只有上述一个测试文件；反向字节还原证明仅一行参数变化。Compile.before全部36DLL仍等于原006；产品源码与工具未变。
3. FIX1 Compile.before→after只变化`FightMatch.Core.Tests.dll`和`.pdb`，其他34份程序集/符号保持原006身份；828源码、858Assets、933保护输入、六运行文件、工具及runner保持。
4. FIX1 Compile.after＝单项.before＝单项.after＝当前实物的828源码、858Assets、36DLL、六运行文件、工具及runner。933保护输入除独立协调例外、15冻结输入、4golden均实核无异常。

因此，原006的4010过项能在产品未变、其余测试源码未变的前提下复用；唯一失败由同一测试的精确补测补齐。该结论遵循§440/§441的组合接收规则，不改写原失败，不宣称新跑完整4011，也不要求无影响部分再跑长全量。

**证据目录与继承I/O**

原manifest103行全部按bytes/SHA核实际文件，自身另计，实际104叶；文件集合恰等于manifest＋自身，均在原128叶白名单内，无软链接或额外文件。原manifest25429 bytes / SHA `5930b9dac600893fbce7d259e5c3511372586277e6acec4ffd65e07841089417`；原root-identity13137 bytes / SHA `a1d036a804bd716dfebf769d54b10df7ed8a1b061089c527f89ec9389a7e4482`。
FIX1 manifest20行同样逐件匹配，含自身实际21叶，均在54叶白名单内，未用叶保持不存在；manifest6715 bytes / SHA `367dc9c7c377cee87d0d69fe8e455238a8ec62f7c7f2ee7d0ef7d149aa37c20c`；root-identity10249 bytes / SHA `7b3a8ecac449862ab5ce956d3e6a64b35b53716a01ccc85b8a9e0a989ed63d7f`。
FIX1一次性runner为20790 bytes / SHA `d12ade528bfb2b5207e13d5a0abd2a621a65f63152af21fea556ffc71fd02dbd`。已静读其固定计划/原103叶身份检查、Compile→单项同版、准确filter/退出码/XML判定、文件容量与新建槽保护；它属于§440明确允许的有限CLI补充，不将例外扩展到原验证工具或新运行。R未执行runner。
对继承根`TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform/io`用lstat/scandir、文件逐件SHA及链接目标核对，不跟随链接：当前3719个合法GUID case、10742目录、17425普通文件、21链接，共28188条目。原2605 case/19778条目保持，028新增1114 case/8410条目且全在新case内；FIX1 before/after与当前全部相同，新增0，未改旧case或旧CONT-C清单。
实际17446个文件＋链接≤40000，3719 case≤4096；剩余377 case不足再次按768预留条件启动完整回归，不据此清理旧证据或扩容。FIX1的8 case/128文件预算未消耗。原028 io-after7146510 bytes / SHA `404b157b2e5679369e6e7e523154e8ec98b8cced893187714dbb6a5e83502291`。

**接收边界、问题与回退单位**

实质问题：NONE。适用B01～B12的实现、范围和组合验证充分；没有需要发出的纠正包或丢弃的产品hunk。
公开默认参考Link正例：NOT VERIFIED。既有PublishedContentCompiler.Select:162～168只生成Attack；当前精确已核发布参考没有Link记录。028复用原CandidateBattlePlaybackFrame对RouteLocked/PendingLinkAdded/PendingLinkRemoved的显示逻辑，未写补线算法；新参考测试实际覆盖原路线、未杀消线、击杀固化，不能据此声称“已核公开免费补线参考播放”通过。§441要求准确保留这一内容边界；本次不篡改六件首包、放宽发布验证或伪造参考。以后发布含Link参考时仍需相应正例验证。
完整Application对象重建使用真实PlayerSave/内容协议和内存故障存储，测试同时保留了原物理I/O平台回归；这些事实不能替代029正常宿主入口、实际玩家触控/屏幕证据、真实Player进程冷启动或Android验收，后者均NOT VERIFIED。新页面编排可供029接入，029尚未实施的入口不在本报告内冒领。
回退单位仍为028这28个源码/meta/工具项及对应交付，前序已接收CONT-C和未受影响的验证证据可保留；具体Git回退/提交/推送由获授权的SD00后续办理，本轮未执行。最终接收仅对应本报告核定的当前字节与证据链，R自然结束本回合后交SD00按§441收件。
