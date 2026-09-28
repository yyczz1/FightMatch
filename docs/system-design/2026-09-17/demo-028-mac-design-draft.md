# 028 同源战斗流程、实际结算与默认参考设计草案

2026-09-28 · SD00 · PREPARED_NOT_DISPATCHED。依§271/272和已接收019/024/025/026/027及待接收CONT-C；C实施门仍未放行。R只读接口咨询turn 01a0e42c-930e-7883-af5a-f559d246d71b于1790534990 completed/error=null，不是代码ACCEPT。最终包应在CONT-C独立接收后冻结当次基线、源码/工具范围及验证根。

目标是补齐正常入场、战斗出口、原请求恢复和真实结算页面。复用同一个PlayerSession及026输入/027播放；不写新伤害、成长、随机、库存或奖励算法。029只承担真实内容/本机根/建档/场景/应用寿命与物理试玩，不再重复028的办理逻辑。

## 拟定文件与职责

新增生产五文件及自然meta：Application/PlayerBattleSession.cs（同文件窄PlayerSession partial桥接、流程/确认/原请求，≤520行）、PlayerBattleModels.cs（不可变上下文/token/只读结果/参考投影，≤280行）；Presentation/PlayerBattleController.cs（组合026/027与session、页面epoch，≤300行）、PlayerBattleView.cs（战场/确认/恢复/结算UI，≤350行）、PlayerDefaultReferenceView.cs（只读参考小窗和独立棋盘覆盖层，≤240行）。总1690行上限，不要求写满；若实现需要拆文件必须先列最小精确差异。

测试拟新增PlayerBattleTestFixture.cs≤320、PlayerBattleFlowTests.cs≤350、PlayerBattleRecoveryTests.cs≤420、PlayerBattlePresentationTests.cs≤360、PlayerDefaultReferenceTests.cs≤280及自然meta；复用已验收PlayerSave真实定义及内存故障夹具，不篡改旧测试。正式六件首包不可改，隔离多成员/多历史等用例与正式内容分开。

七个旧生产例外：PublishedContentModels/Catalog保存并暴露ResolveExact已经核过的参考；CandidateBoardInputView/PlaybackView增加绑定既有控制器的窄入口及明确所有权；CandidateBoardElement增加独立只读参考覆盖层，不争用真实PlaybackOverride。上述五份拟增删预算依次≤36/24/60/90/75。另明确Application/CandidateBattleApplicationSystem.cs增≤56、删0，以及Presentation/CandidateBoardInputController.cs累计增删≤90，仅补下述历史锚点回退窄入口；不修改Core历史/回退算法。任何超出先具体说明。原Attach、当前面手势、单项即时回退语义及旧测试完整保留，不重构整个026/027。

Presentation尽量只消费Application的只读投影，避免新增Content程序集依赖。既有存档协议、DTO/序列字节、Core算法、Windows/Mac存储、Packages/ProjectSettings/正式资源不在028范围。唯一验证工具另增028固定stage，边界沿已验收Mac工具；实际白名单与预算在正式包确定。

## 准入、确认与一个原操作

PlayerSession拥有且缓存单个PlayerBattleSession，其依赖原application/lifecycle与只读PlayerSession能力。PlayerBattleController组合既有input/playback，由宿主持有，寿命独立于VisualElement；页面销毁和业务流程结束是不同事件。Query/预览/返回不创建ID、时间、熵或保存。

导航HostRequest只有Kind/Context/Application，028保留有界的一次接收记录。所有请求先核同一navigation owner/revision、当前head/binding，再按Kind分流。BattleSelectionRequested由PrepareFormationEntry完成含到期恢复的真实准入，不以静态Lifecycle.Enter.NoReadyMember作最终拒绝；draft由已核头填current commit、LevelId/version、formation/各角色/库存/进度修订，再交原Lifecycle.Submit。ResumeBattleRequested核原活动Attempt，不Prepare/Submit新入场；SettlementRequired核WPS和原S17 continuation，不要求Enter可用；RootBackRequested交029宿主。重复Query/事件/双击不能再次H02，UI不注入PlayerId、内容或熵。正常重打走新入场，不是Restart。

回退沿原input.RollbackConfirmationRequired/原Range。单项仍按原松开即时提交；多项展示真实Entries、跨面范围与BeforeSnapshot，按钮闭包绑定owner/epoch/range。确认前比对当前range相同，才调用原ConfirmRollback；旧弹窗不能误确认后来的新range。取消零业务写。

退出与重来分别明确确认，冻结Attempt/Challenge/EntryBaseline/scene/head和操作种类。确认再核当前准入，经PlayerSession.PrepareLifecycle后保留同一prepared request，只调用一次Submit。重来沿原H06保留原入场成员、成长快照、三域初态，不走新入场/重取熵；退出没有新Attempt。未确认取消不分配新operation、不写盘。

胜利处于WPS时只显示“胜利已保存，基础奖励待结算”。027最后演出完成或玩家明确跳至终态后，显示续办结算入口；冷恢复的WPS可以直接继续。使用PlayerSession.PrepareVictory(LocalPlayerClock.Read(),budget)与Lifecycle.Submit，保留S17 ReservedOperationId，不通过通用Lifecycle绕过玩家内容准入。WPS期间不攻击/回退/退出/重来/开新局，也不预报到账。

## 恢复与结果

流程拥有者保留唯一prepared request/original intent和本次操作上下文。页面重建只重新绑定，不重新Prepare。SaveFailed显示原已核头，Retry仍原请求；Unknown显示待确认并封本机冲突写，先原Query/Resolve。不把NotificationFailure当提交失败，也不换ID重复办理。

应用对象重建且有磁盘候选时，玩家明确选commit→Application.ResumeObserved；再次失败或未知时，用新增PlayerSession partial窄桥接调用已接收runtime.QueryResumedIntent取回同一个原pending intent，再走Application.Query/Retry/Resolve/End。不得公开任意pending getter或反射私有字段。已提交但响应丢失时，从当前已核snapshot的对应Record.Intent→QueryOperation取得OriginalLookup；ObservedAlreadyCommitted不能单独充当到账证据。恢复battle请求后允许沿既有明确接管/重建最新终态，不凭页面计时器假回报令牌。

未完成F2建档交回029原创建/Active确认；S17结算优先，不能通过End、导航、迁移或新入场取消保留续办语义。所有kind/owner/commit/operation绑定错误、WrongThread/Disposed/Busy拒绝零副作用。

H06成功会移除ActiveHistory，不能等待活动视图出现Closed才展示结果。结果页绑定原operation/commit/attempt/settlement并用OriginalLookup.Reward、CharacterExperiences、CharacterEnds、InventoryGrant、End按CharacterId和原参与槽展示；当前权威HUD和返回页另取最新已核头。禁用单人Character/Ends/Recovery及单人lookup快捷属性，也不以当前库存或经验差额代原到账凭据。重复结果不重复发奖、不重播已经完成的演出。

结果提供地图/队伍/背包与合法重打；下一关只根据已核发布集合和真实OpenFact。首包只有L1时显示本内容段结束，不按编号伪造下一关。原CandidateDemoView.RewardDestination=NotImplemented不是可用去向接口。

## 控制器寿命和已核默认参考

旧InputView.Attach会建新input，旧PlaybackView.Attach还会RebuildLatest。PlayerBattleController及同一input/playback controller由宿主持有，寿命独立于VisualElement；借用模式Bind/Detach只绑定/解绑视图事件、调度和覆盖层，不Dispose controller、不清原请求、不自动RebuildLatest。普通菜单隐显不结束演出，明确关闭战斗表现时才沿027处理当前原token。生命周期请求由PlayerBattleSession持有，Attack/Link/Rollback请求继续由原input持有。旧Attach语义保持。Generation/epoch/owner逐一核旧按钮/定时回调；宿主最终关闭统一释放一次。验收必须在SaveFailed/Unknown后销毁重建整个视图树，核原request引用、operation、canonical bytes与候选完全保持。

当前已发布package:fightmatch-demo-r1、fingerprint b2247d3f951626edfdf25753520f3421dc20e8d8cab7731c9ab3ba6ece1a5129，L1/version1。ResolveExact已重新跑隔离回放并核EvidenceSha256=86e9f3aa1708fcdd9b9f6eb080ab74cc81acba4709f8c0f12e203bb4366cbfc4，但ResolvedPublication没有保留Replays。只在完整核验成功后把已有不可变列表传入ResolvedPublication，用GetDefaultReferences()方法读取，避免新增公开属性进入反射序列化；不新增发布文件/改hash或另造算法。

PlayerSession只返回已准入精确绑定/关卡的只读参考投影。明确“已验证的开局默认参考”，显示发布版本、W/等级/技能/空携带/关闭道具和固定隔离00..2f种子条件；不称当前玩家随机必胜。当前局面分析未接入须如实标不可用；历史参考有条件不符原因，不能冒充当前已验证。

参考来源身份与当前适用性分开记录；阵容/等级/技能/携带/偏好/随机条件不同仍可阅读，但须展示具体差异，不能升级为当前局面已验证。棋盘叠加只在精确binding、Level/version、Face及几何相符时启用；上下文变化使旧回调失效，不匹配只保留说明。真实战斗演出期间不启动参考覆盖；参考关闭不结束真实播放或回报其token。参考只播放原已核步骤的路线、未杀消线、击杀固化和实际免费补线，独立覆盖层不改变真实HP/敌人意图/scene/偏好/保存，也不代用户提交。首次Gesture.Stage=Dragging时同时关闭窗口和覆盖层，不CancelGesture、不吞首拖、不回报战斗令牌。显示参考前后做源码/业务状态零写入见证。

## 验收和验证顺序

正式包细化B01～B12：一次入场与重复HostRequest；回退单项/多项与旧弹窗；退出/原基线重来；WPS→H06两次提交；多角色原结果；页面/整个对象/已提交三种恢复；S17/F2/拒绝；NotificationFailure与重复结果；播放token和生命周期；精确已核参考/不符分类；零写预览与首拖；真实空内容去向。

全部源码和静态范围固定后，复用CONT-C本轮已通过基线及老3872证据，串行必要Compile→一次无filter全量。只因新失败/修复/源码变化追加验证，不重新启动未变旧基线。正式证据root、各运行slot、允许路径、旧源码身份、工具范围及准确C/R回合在实施包落定；只读草案不是完成证据。

独立接收028后，029再做正常宿主/实际玩家输入和冷启动。R本轮事实咨询不预签ACCEPT，实际C完成之后必须独立审补丁和验证。

## 当前旧文件只读身份（正式派发前再核）

- Assets/Scripts/FightMatch/Content/PublishedContentModels.cs：12798 bytes / ec1a4f4fe40658959bbc9f97c7a1376d14f748f3a3c3a6aacf2de4997ac4080e / 234 lines
- Assets/Scripts/FightMatch/Content/PublishedContentCatalog.cs：16029 bytes / 95bbbe04c2c05c4538b15b5ae97a67aace11b4e10e5d8e3ea858bad273e20b8a / 187 lines
- Assets/Scripts/FightMatch/Presentation/CandidateBoardInputView.cs：4355 bytes / 02d9e49150d0dfd0ef52c9b7db4f416cbfcb94d0177f97a30bf75e55b5a2d2ba / 76 lines
- Assets/Scripts/FightMatch/Presentation/CandidateBattlePlaybackView.cs：5932 bytes / 818b1b97ba9ce6e97cbb5a656af8107c6ef108c3d175ecef8bf4d66cf66638a1 / 88 lines
- Assets/Scripts/FightMatch/Presentation/CandidateBoardElement.cs：9038 bytes / 6d5403009964f1a83487392524dc831b35f223f8d0232890142906bee2dabb7d / 169 lines


## B01～B12的可观察验收矩阵（待正式包冻结）

每项在正式交付中绑定实际具名用例、最终源码/程序集和本轮XML；本表不是已通过声明。不按表行数硬凑测试数，允许一个有意义用例覆盖同一场景的多个观察点。故障/多成员/多面内容使用已获准隔离夹具，不能把它们写成029正常首包试玩证据。

| 编号 | 必须实际证明的行为 | 主要测试归属 |
| --- | --- | --- |
| B01 | 正常HostRequest经当前owner/revision/head/binding只准备并办理一次H02；双击/重复事件零新增操作。到期待恢复且静态NoReadyMember的选择仍交真实H02决定；活动局续局不Prepare新入场，WPS路由不以Enter失败拒绝，RootBack不变成退出战斗。过期/错owner请求零写入。 | Flow / Recovery |
| B02 | 单项回退保持原松开提交；多项确认显示原Entries、跨面范围和BeforeSnapshot。取消无写入，旧弹窗在range/head/epoch变化后不得确认新范围。真正确认调用原input的同一个range，不能重算另一份后默默提交。 | Flow / Presentation |
| B03 | 退出/重来分别确认，取消不分配operation/时间/熵；确认后保存原prepared request。Restart保持原入场参与者、成长快照及三域初态，不成为新H02；Exit无新Attempt；结果页合法重打才创建新入场。 | Flow / Recovery |
| B04 | 合法胜利先成WPS，最后真实播放尚未结束时不显示已到账；播放完成/明确Skip后沿原ReservedOperationId办理H06。恢复WPS沿原续办；WPS中攻击/回退/退出/重来/新入场拒绝。H06失败、未知及成功分别有准确状态。 | Flow / Recovery / Presentation |
| B05 | H06删除ActiveHistory后仍由原OriginalLookup显示结果，逐CharacterId/原槽核Reward、Experiences、Ends、InventoryGrant、End；多成员不能落单人快捷属性。页面展示原收据，当前HUD/返回页读最新已核头；后续库存/经验变化不得改变原到账解释。 | Flow / Presentation |
| B06 | SaveFailed/Unknown后销毁并重建完整VisualElement树，宿主持有的session/input/playback及原request引用保持，operation/规范字节/候选不变。真实应用对象重建后用原观察、ResumeObserved及窄桥接恢复同一意图；已提交响应丢失必须取得原Record.Intent/QueryOperation结果，不能把ObservedAlreadyCommitted本身当收据。 | Recovery / Presentation |
| B07 | F2尚未Active交回宿主原创建；S17继续优先办理原结算。kind/owner/commit/op不符及WrongThread/Disposed/Busy拒绝零副作用；End、导航或迁移不取消S17保留语义。不增任意pending getter或反射窥探。 | Recovery |
| B08 | NotificationFailure仍按真实已提交操作展示和查询，不重新Prepare/Submit。重复结果回调、重复到账查询和返回后重进结果不再次发奖励，也不重播已完成的战斗演出。 | Recovery / Presentation |
| B09 | 旧Attach行为与旧用例保留；新借用绑定只收发页面事件、调度和覆盖层。Detach/普通隐显不Dispose宿主持有控制器、不清原请求、不自动RebuildLatest；明确关闭才走原027令牌处理，最终宿主释放一次。迟到按钮/定时回调不作用于新owner/epoch。 | Presentation |
| B10 | 六件首包与原发布/验证指纹保持。ResolveExact完整成功后可读原核验参考，错误binding/level/version不能取其他参考；新读取方法不进入反射序列属性，不暴露可变集合。参考固定条件和当前阵容/成长/技能/携带/偏好/随机差异分别展示，不能声称当前局面已验证或随机必胜。 | DefaultReference |
| B11 | 参考覆盖只在精确binding/level/version/face/几何相符且无真实播放时出现；不符保留说明。打开/播放/关闭前后真实scene/HP/意图/偏好/保存头/历史均无变化，不回报真实播放token。第一次Dragging关闭参考同时保留该次合法拖线，不吞事件/CancelGesture；旧覆盖回调失效。 | DefaultReference / Presentation |
| B12 | 真实结果可返回地图/队伍/背包并核同一资料，合法重打走原入口；下一关只来自实际已发布及OpenFact，不能按编号合成。L1无下一关、无发布配方均准确显示真实空态，不把NotImplemented当路由能力或以Demo分支绕过已有合法流程。 | Flow / Presentation |

正式范围初算：沿CONT-C当前808实现/838Assets/441GUID新增10cs和10自然meta，若前序终态不变则为828/858/451；派发时必须按独立接收后的实际清单重新核定，不把此预计数字当范围证明。所有原测试和当次CONT-C新增用例保留具名多重集合；后续新增用例单列。每次源码包完成静态/范围后只执行获准必要编译与一次完整回归，失败/源码变更才追加；029的屏幕、物理输入与两次真实进程冷启动（通常三个Player进程）另交实际证据。


## R可执行性咨询后的四项收敛

R只读咨询turn 01a0e5de-a14e-7bc3-adee-e93189a36cde于1790563600 completed/error=null。以下落定准备稿所缺入口，不是CONT-C验收、028实施派发或预先ACCEPT；正式包按前序接收基线冻结。

**B02历史锚点入口。** Core/CandidateHistoryOperations.Locate只定位当前面配对，但现有ReadRange已经支持原有效HistoryAnchorId的跨面范围。028的历史列表只列当前已核history.EffectiveAnchors对应的原记录，由玩家选择，按钮绑定028 owner/epoch、expected commit及原anchor。CandidateBattleApplicationSystem增加窄的历史范围预览入口，核WrongThread/Disposed/Busy、已核Ready头、expected commit、真实Rollback准入和当前history，再调用既有ReadRange；不得在Core新增或复制回退算法。CandidateBoardInputController增加历史锚点选择入口及对应确认上下文，选择和确认均核同一头、Attempt、SceneRevision、anchor和原范围；原手势仍用原Locator，二者不得混淆。新历史入口也复用原Prepare/Submit/Retry/Resolve/End和原request持有方式。028窗口显示真实Entries/跨面范围/BeforeSnapshot，确认仍交同一input controller。至少一个隔离多面场景要从新列表真实选择走到确认/提交；只直接构造Range并展示不算B02。旧锚、已回退失效锚、头变化、播放门禁和重复确认均须零新增操作。

**B08/M06原收据重开。** PlayerBattleSession新增只读的指定原operation结果入口，在当前已核头按准确operation找到原Record.Intent，再调用Application.QueryOperation，重核OriginalLookup、原commit/attempt/settlement及lookup所依据的当前头后投影完整收据。错误kind、外来player/owner、缺失记录或过期页面上下文拒绝；不Prepare/Submit、不重新结算、不复播。029宿主订阅导航Changed，在导航原操作查询实际到达CommittedResult且已核结果属于战斗结果时，按精确原operation交给028结果页；宿主提供明确“查看原战斗结果”的正常入口，不把纯技术编号标签当完整结算页。不修改CONT-C导航源文件来隐式增HostRequest种类。具名用例必须包括完全重建Application/PlayerSession且无内存请求后读取已保存原收据，以及离开结果返回地图后再打开同一收据。最新HUD另读当前头，旧收据不随以后成长/库存改变。

**B04真实播放门禁。** 结算按钮与PlayerBattleSession的执行入口都必须在读取时间、PrepareVictory及任何operation准备前，核CandidateBattleApplicationSystem.QueryView().PresentationToken已经解除，不能只看PlaybackController.IsPlaying或隐藏按钮。原Playback.Finish会先清original/发Changed再回报token，因此首次Changed内重入点击仍须零读时间、零准备、零提交；第二次真实token解除后的明确玩家操作才允许原S17续办。保留旧Lifecycle系统对SettleVictory的合法规则，不改旧播放回报顺序来绕过。冷恢复WPS无播放token时可继续原ReservedOperationId。B04/B06另具名验证持久化WPS/S17后整个应用对象重建、准确恢复原continuation、只结算一次；这项对象重建测试不标为真实Player进程冷启动。

**M07活动保存点。** 029按“合法重打→原基线重来→明确退出战斗→再次合法入场→至少一次非终局已提交行动→正常关闭”建立P2活动保存点；P3第一次输入前核同一Attempt/Challenge/EntryBaseline/三域RNG/history，随后真实下一行动证明接续。该三进程方案不宣称WPS真实进程冷启动已验证，未实际执行时保留NOT VERIFIED；持久化WPS/S17对象重建由B04/B06覆盖。


## 验证工具与证据准备边界（未授权执行）

028继续唯一Tools/Invoke-FM025P2Validation.ps1，不新造另一个Unity验证命令。拟新增固定Stage=DEMO-028-MAC，仅Compile/Tests，工程和2022.3.18f1路径沿已核Mac值；原各Windows阶段及CONT-C阶段的固定根/身份/命令行为保持。新阶段独立读取前序正式接收清单加本包10cs/10meta与七旧文件例外，不能让旧CONT-C根校验误绑本包，也不能删掉Mac进程检查、SHA自检、Compile到Tests同版、无filter/无quit全量或CreateNew运行槽保护。工具最终差异预算及原文本/SHA等C正式终态后冻结，不把当前工具字节当已接收。

拟定新运行根TestArtifacts/FMDemo028/mac-r1。正式签发精确列出root-identity、source/资产/GUID/DLL前后清单、范围审计、原文件before/after-text、日志/结果/XML及manifest有限叶；失败保留，未执行槽不造文件，不用通配目录授无限证据写入。每槽仍run/before/after/result/failure、compile/tests三类输出及tests.xml等12叶，先固定001～006六槽作为失败恢复上限，实际只需一次编译和一次全量；没有失败/修复/源码变化不能为凑槽追加运行。

源码前序基线从CONT-C本轮R ACCEPT的最终scope取得，不重复运行未变基线，原3872及全部CONT-C用例按最终多重集合保留。历史四冻结输入已由003支持的恢复产物严格比对原SHA，应读取现有四件并保持，不因无关028页面变化重复运行恢复器。028新增B01～B12测试和七处窄改随最终源码一次Compile/完整EditMode验证，交付明确原范围/新范围/Windows与Mac证据的各自版本。

C正式报告拟为docs/system-design/2026-09-17/demo-028-mac-r1-delivery.md与demo-028-mac-r1-code-scope.json，R报告拟为demo-028-mac-r1-code-review.md。准确作者turn/root与各正式范围哈希通过派发记录绑定；准确completed/error=null、正式COMPLETED、实物身份齐备后才由R作独立代码审查，咨询回执不代替该门。


另外两处历史锚点旧文件的当前只读身份（正式派发仍核前序接收版本）：

- Assets/Scripts/FightMatch/Application/CandidateBattleApplicationSystem.cs：10725 bytes / 38db8c7d63a6e6702cd8854c7e7589e8582712648a0f23bfb58e11e7dad1cdc2 / 179 lines。
- Assets/Scripts/FightMatch/Presentation/CandidateBoardInputController.cs：14527 bytes / fd6a1d16d6a612cb68e725e66586ae3fc65c4f5297baa33ba5cbe1fc3902e8bd / 245 lines。

有限128条证据候选（正式签发仍展开为逐项JSON，不凭本段写文件）：根内20个元数据叶固定为root-identity.json、storage-plan.json、platform-environment.json、scope-audit.json、case-evidence.json、implementation-before.json、implementation-after.json、assets-before.json、assets-after.json、guid-before.json、guid-after.json、dll-before.json、dll-after.json、inputs-before.json、inputs-after.json、named-tests-before.json、named-tests-after.json、io-before.json、io-after.json、read-manifest.json。before-text仅七旧生产例外及唯一验证工具共8件完整原文；after-text为这8件与10个新cs/10个自然meta共28件完整最终文本，按原相对路径保留目录层次并追加.txt。001～006每槽12叶沿已验收工具名称，总72项；因此候选总20＋8＋28＋72＝128，manifest排自身最多127项，未执行槽不造产物。正式文本快照必须含全部28个作者最终可写源/工具项，并记录每个旧meta字节保持。元数据含失败/修正的真实before与具名测试变化，不另开未列根。新计划/交付/范围/独立审查报告在根外按正式包逐项签出和单列身份，避免自引用。


## 继承的Mac物理测试区（实施前冻结，不修改旧测试）

源码只读确认LocalSaveTestFiles.MacIoRoot固定为TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform/io，旧完整测试在每个新GUID case中执行真实I/O。028不为换阶段修改这个已验收测试辅助，不将新运行写成旧CONT-C证据；必须在正式storage-plan中明确授权此继承I/O区的有限追加，并在本包io-before.json/io-after.json中记录前后差异。

截至015终态，原io-inventory.json为4984796bytes/SHAce9e3daa1cca65e30f74b68bbf9572ee68e2cd21d47debeb029c4318f8a22d82，实际2605个case目录；按本机birthtime只读计，本次015运行窗口新增556个case。此计数仅用于容量预检，不代替测试数或写入契约。派发前按CONT-C最终独立接收状态重新核定。保留既有≤4096 case/≤40000文件和链接总预算，不静默扩大；容量不足时应在启动Unity前报告具体缺口，不能跑到一半才清旧目录。

只允许原测试在全新GUID case内按既有合法存储语义创建/改变其自有文件与链接，不碰真实玩家根，不修改/删除历史case或原CONT-C元数据、manifest、日志/结果/输入。使用lstat且不跟随链接核前后清单，原已有条目逐件保持；新增条目、实际总数与版本单列于028自己的两份io清单。该继承区不计入128个元数据/文本/运行叶，但受上述精确根、既有布局和数量边界约束。正式新包必须同时列出此区与128条证据叶，不用一个通配目录替代授权。

新DEMO-028-MAC阶段的验证工具拟以当次已接收工具为before，允许仅为该固定阶段/根/输入清单/继承I/O预检添加必要分支，增删合计≤200行、最终≤before行数+200（按当前499预计≤699），其他阶段行为保持；不要求用满预算，不压行规避。C须先完成实现、静态范围和工具AST检查，固定源码后再做获准验证。当前只是待签范围准备，不授权实施或Unity运行。
