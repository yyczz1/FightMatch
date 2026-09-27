VERDICT: ACCEPT

Scope: PASS。Acceptance criteria: PASS（A01～A06）。Verification: 原实现回合正式完成后，独立检查实际差异、有限证据、最终编译010及全量EditMode011；3464/3464通过，原3424用例多重集保留。没有发现需阻断025-P2A接收的问题。

本结论仅接收025-P2A共享规则与正式内容绑定。依据为冻结§280～282及§285、§287补充，承接§271～273批准输入和同产品约束；不把作者COMPLETED、协调收件或旧C1结论当成本次独立审查。

审查身份和完成门如下。R未运行Unity、修改实现、执行Git写操作、新建任务或代理；本次唯一写入是此报告。R的正式完成仍以随后本准确回合的原生final/task_complete为准，不在文件中提前宣称已完成。

| 对象 | 准确身份与原生记录 |
| --- | --- |
| 原C任务 | 01a0c403-bfa1-7e90-b503-c0fcd61f23c1 |
| 本次C回合 | 01a0d207-3f3f-7801-a775-692b0b60d52d；task_started 2026-09-24T06:08:05.463Z |
| C正式交付 | 非异步formal final 2026-09-24T07:35:45.434Z；task_complete 07:35:45.765Z；COMPLETED；工具状态idle/completed、error=null |
| 原R任务 | 01a0c1cd-dce1-7ac3-8780-06163cb0acfc |
| 本次R回合 | 01a0d208-0f04-7613-a90b-bf804c600909；task_started 2026-09-24T06:08:58.652Z |
| 模型／工作目录 | C/R原生turn_context均为gpt-6-astra / max，D:/Unity/UnityProj/FightMatch；R续接上下文07:45:38.411Z仍相同 |
| 最后写报告前核对 | 2026-09-24T07:53:45.7470032Z：680实现、308受保护输入无差异；C两正式报告及三段冻结摘要保持；此review路径尚不存在 |

已排除06:34:58的许可异步问题、报告提前出现和旧回合完成。准确C的task_complete.last_agent_message含本次两正式报告SHA；实物长度／SHA与其formal final逐项相符。

| 绑定对象 | 字节／SHA256 |
| --- | --- |
| [C delivery](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2a-delivery.md) | 11690 / 84f298bb28eb5fe2804f8f5ada1a4bc0f9bf76c50a60f089448ca0c18461a966 |
| [C scope](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-025-p2a-scope.json) | 1701832 / 22b6ff2ac2d1261428461f837d8765490e4c21a9a48fde065a5bd00e379d5e70 |
| §280～282规范化范围 | eea36b604d5646c3ef5104cc2d37ed9eda64bb6126978ad846708cac4909cac8 |
| §285规范化范围 | 0608638f1d84e41443bc7588dc6e795872e1a25c768e361ef8ccbd7bdb8936bd |
| §287规范化范围 | ee3c2320b6b926355bada2d698236ab49c1a1389469a3c863a55c005b14c260a |
| 旧668基线scope | 833816 / f8ba3e2e25fa3d7d87fe1fd2c75fbddbc92c253d131b198fccb82ae34edbb677 |
| C1设计scope | 633186 / da3eabc8fc96834bdcf3f61805afc2e606e3aa741b82cf5c97982d4b582ec058 |
| 已接收C1独立review | 14734 / 240907f34a2ea5e7f2ef66e73b038d2cc135920f1406311488a7c1bf324e2444 |

冻结段摘要独立按CRLF→LF、准确起止标题、TrimEnd后单LF及UTF-8重算。三协调稿的后续登记沿授权处理，冻结段不变；没有要求协调稿整个文件继续等于实施入口SHA。

A01通过。[ContentBindings.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/ContentBindings.cs:19)的ContentBinding五字段不可变且全部参与精确相等；DefinitionBinding追加LevelId、正整数LevelVersion和规范十进制版本。[RuleContextChecks.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/RuleContextChecks.cs:9)按候选／正式实际类型分派，未知类型、缺失正式绑定不进入业务核；候选SourceNotes复制为独立列表／只读列表，正式分支不合成草稿字段。正式身份拒绝空白、孤立代理和字符串／整数超预算，合法FFFD及代理对保留。

[BattleEntryPreparer.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/BattleEntryPreparer.cs:20)先核正式上下文、全部包身份及解析关卡的ID／规范版本，再复用原私有准备核；最终对准备关卡与resolvedLevel作完整显式值编码比较，不能只用ID／版本掩盖敌人属性变化。PreparedBattleEntry保留准确DefinitionBinding。新增[PublishedRuleContextTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/PublishedRuleContextTests.cs:174)实际覆盖错域、缺绑定、缺关卡、成员上下文不符、整关载荷改变、版本和LevelId不符等拒绝路径。

[PublishedSaveContext.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/PublishedSaveContext.cs:23)将已准备定义作为不可变闭包，复制外部列表；核成长／库存／进度／奖励的正式上下文、关卡集合及版本一致，拒绝重复包、重复关卡／奖励和候选混入。FindExact只返回全包绑定及准确关卡版本匹配项；它不证明实际发布、启用或玩家会话准入。

A02通过。41个旧生产文件的实际差异局限于共享上下文类型、复制／比较／预算、Requirements及身份编码；没有第二套战斗、成长、库存、进度、随机或收益算法。[SharedRuleContinuityTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/SharedRuleContinuityTests.cs:19)用相同内存关卡／角色事实及冻结48字节种子，分别走候选和正式准备入口，执行两次真实攻击、敌方行动、历史记录、结算和收益应用。

该测试比较完整operation record编码，包括HP、伤害、随机状态／耗字、阶段和贡献；另核最终HP=95、XP=26、材料2、成长经验、库存carry清空、进度结束及首通。来源上下文和报告指纹差异单独断言，没有删除业务字段使两路相等。正式隔离重放仍沿既有Core得到Matched，CommitEligible保持false。以上数字为合成规则测试结果，不是新批准内容、已发布关卡或实际玩家进度。

A03通过。独立比对新测试内LegacyCandidateReportWriter与入口副本的整个原Fmbr01Writer：仅类名／构造器名替换，正文完全相同；原文件SHA256=bb35f4fb3c6943ec4f6e8f17a688d95fcd4f596592fd0839ed596915c798654e。完整候选报告的expected由这个冻结旧规范生成，未调用本次修改后的编码器生成expected后自比。

两组字面golden直接回查入口旧测试：CandidateApplicationIntentTests.cs第18行的FMINT001完整100字节／SHA256 dc9d0a7e42a08310d68fd546199dd0f484f818da5deee3f4d5568e799a95383d，以及CandidateBattleOperationsTests.cs第383行的FMBR01独立51字节／SHA256 c3918a41722ceee31527635fd94a3b52faff87f80bd40235f136b0b8bd13465c。新测试中的固定值与旧值相同，最终XML通过。

[CandidateApplicationIntentCodec.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateApplicationIntentCodec.cs)保留候选FMINT001/schema1，正式使用FMINT002/schema2并编码全包绑定；正式进关版本必须为正整数规范文本。混贴tag/schema、正式意图中的坏代理被拒绝，合法FFFD／代理对往返保持。候选原来允许的UTF-16单位和不透明关卡版本不被新规则追溯改写。[CandidateBattleReportFingerprint.cs](D:/Unity/UnityProj/FightMatch/Assets/Scripts/FightMatch/Core/CandidateBattleReportFingerprint.cs:120)为正式entry/context增加独立版本tag及完整DefinitionBinding，候选分支保持原顺序和字节。P1-C1的F1-FFFD、F1-PAIR、F1-D800、F1-D801四个旧编译器用例均保留并通过。

A04通过。PrepareCandidate拒绝正式上下文；原业务／Application存档入口保留CandidateValidation限定，候选字段访问前作分支拒绝。新增测试实际检查正式状态不能通过旧Prepare／Encode，以及将旧信封purpose改成PlayerSave仍被旧Decode拒绝。正式Requirements是纯元数据投影：完整包字段、规范Definition版本，DraftId／DraftRevision／SourceNotes均为null；去重及信封往返通过，不构成正式业务存档准入。

Application生命周期仍明确限定候选上下文，视图Purpose来自实际会话描述或CandidateValidation默认值，CommitEligible仍false。请求／控制器仅承接同一上下文，原runtime、队列、store、操作优先查询、阈值／播放／旧token规则未另建或改写。Input/GestureModels.cs保持只读；没有新Core依赖，没有正式发布／启用、首次持久建档、EncodePublished／DecodePublished或假正式session成功。

A05通过。独立解析旧XML与最终011 XML，以fullname计数构成多重集：旧3424全部保留，缺失0，新增40；最终3464项全部Passed，Failed=0，Skipped／其他非Passed=0。新增集合与named-cases-diff.json准确fullname逐项一致，其中PublishedRuleContextTests为34项、SharedRuleContinuityTests为6项。不是只比较总数。

独立以入口字节副本对每个旧测试作全文比较：最终文本严格等于获准签名类型替换后的原文本。十个CandidateContext→RuleContext和一个PreparedCandidateContext→PreparedRuleContext，共11处、22行增删；其余返回类型、访问性、方法体、用例参数、断言和预期未变。CandidateBaseRewardsTests有两处，其余九文件各一处；不把新增测试修正算作放宽旧断言。

| 最终实际运行 | 进程与UTC时间 | 独立核对结果 |
| --- | --- | --- |
| 010 compile | PID35100；07:16:23.3734975～07:16:34.6596336；11.2861361秒 | 实际Process.ExitCode=0，无C#编译错误；命令含-batchmode -nographics -quit及准确工程／compile.log路径 |
| 011 tests | PID15256；07:18:00.6843449～07:24:34.5522420；393.8678971秒 | 实际Process.ExitCode=0；完整EditMode，无-quit及用例过滤；3464/3464通过 |

运行脚本实际使用Start-Process -WindowStyle Hidden、持有进程句柄、WaitForExit／Refresh后获取ExitCode；先记录真实退出再采集后快照。010和011的before/after均有准确680源码、脚本、36 DLL/PDB及308受保护输入身份；启动前Unity进程集合为空，没有同工程并行批处理证据。R没有复跑Unity。

独立比较010-before、010-after、011-before、011-after与正式交付：680源码及脚本全部相同。010-after→011-before→011-after的36 DLL/PDB完全一致，并在07:47:39.4746649Z与当前实际文件逐项长度／SHA复核一致。308受保护输入在四个运行快照及交付均相同。XML运行时段07:18:08Z～07:24:31Z位于011实际进程时段内。

| 最终验证实物 | 字节／SHA256 |
| --- | --- |
| [011 tests.xml](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/runs/011/tests.xml) | 2434965 / 7dcd867c4b4f1ac53f3252dcece9130084dea95a708bda0aeb72758b8176f528 |
| [010 compile.log](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/runs/010/compile.log) | 64999 / 21c7d64a22fca8fe035eddcf3cba197fb5c33caaf0ec914e6cdfb14c8f84900f |
| [011 tests.log](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/runs/011/tests.log) | 1505824 / d53e654bd6f4c897406f84579ba6192ff9fd2b61992148033a384cfce433ca0b |
| [最终验证脚本](D:/Unity/UnityProj/FightMatch/Tools/Invoke-FM025P2Validation.ps1) | 8300 / 7231aaffb7e3d04634d71c13a15057fbf2a26475f98f5887ca9e0202b7005656 |
| 旧3424基准XML | 2407521 / b3331b5bcbc76794f487aa1401fe1c177be74fd0accf8df6c0c685e14134b385 |

本次失败和中间成功均保留。001发生许可IPC超时及输出采集失败，actualExitCode缺失，日志声明199不能冒充真实进程退出；002实际exit1，未获得可用许可。003／004／007实际exit1，分别保留生产／测试编译失败；005／008实际编译exit0。006实际测试exit2，3446/3450，四项失败均在新增用例，原3424仍全通过；009实际3458/3458、exit0，但其后Requirements修正及六个新增用例使它不能替代最终010／011。

§287环境补充已独立核对。[environment-diagnosis.json](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/environment-diagnosis.json)为18933字节，SHA256=5413720a3dcf8925aaf3c35a5c07771ac1c1c7db95b07196f811b099dc00192a，低于49152上限。实际Editor为D:/Unity/UnityClient/2022.3.18f1/Editor/Unity.exe，版本2022.3.18.13802474，当前文件SHA256 ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895，与已接受P1-C1运行身份相同。

当前获准执行通道已有真实编译／测试成功，没有用户激活待办。002同样记录经require_escalated仍失败，故不能把后续恢复唯一归因于权限变化；Hub／客户端启动时间变化只支持待证假设，历史Owner／令牌／完整性级别未取得。最终日志仍有客户端签名和Access token诊断，随后成功取得entitlements并完成执行；未将诊断抹去或据其推断账号未激活。Hub截图不是本报告的编译／测试通过依据，R未读取账号缓存或要求用户重新激活。

A06通过。范围以旧668精确基线及§285补充白名单核对，未用Git当前差异混入此前已接收实现。entry-files的668副本与基线逐项相符，final-files的680实现及脚本与当前文件逐项相符。

| 范围／预算项 | 独立实核 | 上限／预期 |
| --- | --- | --- |
| 旧实现修改／保持 | 51修改（41生产、10测试），617保持；无额外旧文件变化 | 51／617 |
| 旧文件增删合计 | 567；逐文件git diff --no-index --numstat重算，与报告一致 | ≤2200 |
| 旧helper增删 | 11签名，共22行 | ≤22 |
| 新生产／新测试 | 4文件409行；2文件853行 | ≤1200／≤1600 |
| 新自然meta | 6；原361不变，最终367 GUID全部唯一 | 6 |
| 实现／Assets集合 | 680／693，完整路径集合及逐项长度／SHA匹配 | 680／693 |
| 脚本／作者报告 | 脚本132行；delivery91行；scope1701832字节 | ≤360／≤280／≤4194304 |
| 受保护输入 | 308全部保持；旧六项P1-C1绑定日志／XML亦逐项保持 | 不变 |

没有发现未许可Packages、ProjectSettings、asmdef、旧meta/GUID、配置、依赖或旧报告变更。三协调稿的当前身份与C导出观察存在后续差异，已结合§290正式收件及SD00本回合通知识别为获准协调更新；不归到C实现差异，也不要求后继使用其旧观察SHA。后继应按冻结段及最新协调身份分别重绑定。

有限证据准入独立通过：准确根TestArtifacts/FMDemo025P2/p2a；manifest共1454条，实际文件1455个（含不自哈希的manifest）。逐项检查允许展开集合、relativePath、role、bytes、SHA、重复／缺失／额外文件、真实路径越根及各级reparse/junction，未发现异常。1454条与scope.stageExports.evidence.items完全一致；其中668入口副本、681最终源码／工具副本、19固定阶段元数据、86实际运行证据。

| 外部绑定实物 | 字节／SHA256 |
| --- | --- |
| [root-identity.json](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/root-identity.json) | 465 / 985c8c9159f4778ce36b656448fa2ce050b70d7f4dc8d8aa4acbeedef1b74f4e |
| [read-manifest.json](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemo025P2/p2a/read-manifest.json) | 385398 / 2cb78d0a45526e60a4435e7becf5f1c03a9ad8aafe04fec5cf392b3ae8ea197a |

root-identity绑定本准确C任务／回合、gpt-6-astra/max及原§280～282摘要；§285／§287另在正式scope绑定，不伪造早期运行已经获得补充授权。stageExports含725项实现／脚本／必要旧入口，完整继承集合符合要求（包括明确增准的旧3424 XML）。delivery→delivery-bindings→manifest→scope→外部formal final的生成顺序明确，无scope自哈希或报告互相自哈希循环；C未提前宣称原生完成时间。后续可据该导出和本次正式报告身份精确接续。

Notes: 许可失败的唯一根因未证实；本段技术验收已由最终真实运行闭合。当前验证脚本仅实现025-P2A固定阶段／证据根，C1未来阶段设计不等于现有脚本能力；后继适配必须另包授权并保留本次最终脚本副本和运行身份。

未执行、未接收的边界保持：实际鼠标／触控、像素／交互PlayMode、AwaitLinks／全队倒下等既有NOT VERIFIED，Player／Android构建、CI／lint／性能、剩余保存故障矩阵及§184边界。本次全绿不清除这些项目，不改变027／P1既有接收证据；没有重新运行旧求参或旧故障矩阵。

仅此P2A独立结论可供SD00按准确R完成记录收件，再另签P2B-PREPARE；不自动执行P2B／P2C，不发布／启用内容，不开放真实PlayerSave，不接收025整体、B17或完整Demo，也不增计当前28项已接收功能。已采用的34功能／51交付路线口径保持。本报告的字节数和SHA由本回合formal final外部绑定，写后停改。
