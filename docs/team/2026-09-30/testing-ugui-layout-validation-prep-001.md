# LAYOUT-TEST-PREP-001：定向覆盖与证据复用准备

2026-10-02 · `PREPARED_NOT_EXECUTED`；仅用例／证据规划，不是代码审查、执行授权或产品ACCEPT。
主测试 `01a0f2e3-4bd2-7130-b43a-19afa702e7bb/local`，actual turn `01a0fa6e-1027-73c0-94f3-d1d391f5cb3e`；唯一回主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`。依据用户AGENTS§6／TEAM_WORKFLOW及主程转达2026-10-02继续首Demo授权；C源码turn `01a0fa6a-62bb-7c32-b577-4d268818b774`仍由主程独占收件，本任务不等C、不派下游或转中央。

## 1. 冻结输入与本轮核验

同目录前缀 `engineering-ugui-layout-source-activation-`：A=`architecture-001.md` SHA256 `83a359f352cbc0ab953ab25179f6378529ec5334fc338e9e25419d8b0b5c7f84`；S=`system-001.md` SHA256 `3220629d60fdb279e9328353d5a90156f0abc66415cf24bf1e6da42d4beba06d`；M=`inputs-001.json` SHA256 `5a2f1394e2b6fe03fdeaf7cb4d4a54a23f5b6f493d33b4dcf430e31f899a690d`。三者本轮实算相等。
布局优先级仍中央裁定＞correction＞base；本轮核五份设计／设计接收文件SHA均等于M.inputs。A/S仅替换分门执行顺序，不删除原几何、Caption、业务及生命周期断言；本轮不读取C变动中的产品／测试源码，不做重复review。uGUI技能用于冻结层级及原生证据边界，writing-for-agents用于分开前置门与完成条件。
历史依据：`ugui-fix22-integration-receipt.json` SHA `89255ba4c154a37168895d2daa7a249ac09b4249888cdb932ade2de06fc14b04`；`docs/versioning/2026-10-01-ugui-pr/fix22-result/grouped-exact222-coverage.json` SHA `be4f2799d53baa49639ee53046f8c4b96d9177678566a78d811354021fe1745b`，均实算匹配。其限定接收绑定PR1 head `3541930877837e14b58020b910976b8b67b56e42`／tree `37326f759fc065f268c23f66a674be1a61f12268`；本轮只读已接受的跨轮Counter／结果映射，不重新运行或联网验审查。

## 2. 47的计数、重复与依赖

| 集合（完整fullname唯一权威为M.tests） | 本轮机械核对 | 保留的验收对象 |
| --- | --- | --- |
| 新声明12 | 12个非参数化声明，与旧222交集0；实际编译后leaf尚未发现 | LAYOUT_01/04/06/09为层级／压缩／轴owner／OriginalSlot；其余8项是真Overlay运行时要求 |
| 历史32 | 20方法→32参数leaf：PlaybackPanel4、BattlePresentation5、DefaultReference5、NavigationRecovery8、HostRoutingLifetime10 | 精确参数／原请求、head、输入、token、保存、重复／过期回调和冷重建断言；名字含UIToolkit的历史方法不擅自改名 |
| 资源3 | SavedPrefabAssemblesOneRealHostCanvasAndProjectEventSystem、FourCornerCentersAndHalfOpenEdgesUseTheActualBoardMapper、BothResolutionsWithFourInsetsKeepHitAndRenderedCentersIdentical | 第一项为装配，后两项为映射；“3”是测试实例，不是新增资源写额度 |
| 精确并集 | 12+32+3=47、重复0；32与3互斥，35个旧fullname全在旧222.results且均Passed，新12与旧222互斥 | 这是计划集合，不是47已通过；逻辑39+8分类不再拆成两次运行 |

32参数展开必须保留：Reference B10三项、Battle B06_B09三项、CC14_CC15四项、CC17_RCV1四项、Host OriginalEndReceipt两项／ReplacingWholeView两项，其余单项；不得按20方法、旧28或测试类通配选择。完整参数文本含引号／布尔值，须字面转义并全字符串锚定。
旧222中本门只选35，其余187并非新版已绿；旧fixed28有21项在本门之外，不能把“历史fixed28通过”当作此轮输入安全全覆盖，也不另跑28来补数字。

## 3. 复用规则与PENDING缺口（不是加跑清单）

只有用例源码／程序集、fixture及传递依赖、产品输入、实际Prefab/Scene、字体／CSV和selector适用范围均有未变证据，才可接收对应旧结果；仅方法正文不变或路径没进S白名单不充分。参数集与身份可复用为发现对照，旧绿状态只属于旧输入。每个受影响实例保持原断言，转为新证据或明确NOT_RUN。

| 待主程处置的缺口 | 冻结依据与最小下一动作 |
| --- | --- |
| `LocalizedTextBindingTests`6项全部未选 | 该类在S改动范围；含精确TMP占位目标、首次激活、缺键／参数／字体诊断、locale重建head/request/token、三次Bind/Unbind监听清理与业务严重度。原119/116不能继承，3输入框及逐项绑定不能放宽成“至少”；6项均先PENDING。 |
| `UguiSceneCompositionTests`其余3项未选 | OwnerEnded重复release只提交一次、OwnerCanceled先归一化不提交、SecondFinger不抢owner；共享Prefab/Host/input及该类改变，资源3通过不能替代这3项输入断言。 |
| 其它显式相关类未选50项 | CandidateBoardInput24、BattlePresentation10、DefaultReference7、NavigationRecovery9；共享BattleUguiRoot／NavigationPanel及产品视图变动可传递，尤其B16/WIRE01、旧epoch、播放锁与原恢复动作。与上述9项合计59个待分类leaf，**不是认定59项都须执行**。 |
| 余下依赖与纯业务分界未闭合 | CandidateBattlePlaybackTests、PlayerBattleFlowTests、NavigationPresentation/PermanentFlow及Host资源／Profile等消费者须在S/P冻结后按实际依赖分类；LocalePolicy等纯逻辑也需不变证明，不能按类名批量接收。187中最终可复用／需补证数量目前未定。 |

主程接回S/P后给每个缺口标“未变且可复用／本47等价覆盖并指向全部原断言／另包最小补证／仍NOT_RUN”，列准确leaf和依赖依据。新增必要覆盖超47时单独签范围／预算；本任务不扩大15源码／26总路径、不删替断言或以跳过凑47，不执行234／222／全量或自动重试。

## 4. 下一门最小签发清单

1. **源码身份／审查：** 接收真实S回执、diff及source manifest，投影绑定PR1实际tree＋S delta＋固定CSV，而非dirty master；记录实际执行source candidate与新PR head/tree的逐路径映射。GitHub PR Code Review是唯一独立代码审查，P0/P1未闭合不进入native；纯源码审查不覆盖以后两meta／Prefab／Scene的最终head。
2. **环境隔离：** dots graphics001/002仍Xorg socket受阻，目录warning／sudo缺失不等于唯一原因；不重试。隔离候选目录、完整离线缓存闭包、图形／许可、准备空间／时限／证据范围及C独占租约仍待准确激活。PR3三源码及三meta、Config/Generated/Tools LOC WIP须排除；共享文件不移动、不改名、不复制共享Library/Temp、不直接启动共享工程Unity。
3. **I独立激活：** 一次Intel Mac Unity2022.3.18f1 compile/import≤360s，冻结工具／argv／候选绝对路径、C thread/turn和owned身份；只允许自然生成M.futureOnly的两个新C# meta。旧资源尚未迁移，编译成功不叫可玩；额外LOC meta或输入漂移即停。
4. **P独立激活：** I成功后一次 `FightMatch.Host.Editor.FightMatchAndroidBuild.MigrateLayoutResources`≤180s；按S参数／新证据根迁移已有Prefab/Scene、原生保存／卸载／同进程核验复开，保留GUID/fileID和字体/TMP；只输出已签三份native JSON，不额外启动Verify/PrepareResources或真实Host。回交先核共享目标仍等于preimage；冻结post-P源／两meta／资源candidate及最终审查head，不用pre-P资源哈希证明T。
5. **真实leaf冻结：** I/P成功且源码／资源就绪后，另签一次有界发现的入口、次数、deadline和准备证据路径；当前不能把12声明当发现结果，也不借发现执行额外测试。排序唯一真实NUnit fullnames，与M的12/32/3逐项Counter对照；重复／缺失／新增／参数漂移均先停。冻结发现程序集、47清单、literal-escaped全锚selector和source/native candidate SHA；未冻结不得T。
6. **T独立激活：** 只准上述并集一次≤360s、每实例一次，明确graphics/Release参数、runner／进度回调身份和输出白名单；不照搬写死FIX22的runner。成功需实际名字、Started/Finished Counter均精确47、全Passed、0失败/跳过/inconclusive、真实exit0／owned清空及post-P保护项不漂移。超时、首错、集合漂移立即停，不能因尚有预算追加一次。
7. **证据收尾：** 每门保存实际head/tree／执行字节映射、argv/PID/starttime、UTC与退出／耗时、日志／XML／事件hash、前后资源和源身份；owned-only自然观察≤60s、身份核准单次终止后≤30s，仍存活交人工，不伤非owned进程。单日志≤8MiB、单阶段证据≤192MiB、剩余≥2GiB；超时即使后来退出也保留超时。迁移前复开不能替代T后的身份核对，额外native最终复开另签而非暗增本门。

## 5. 原生Overlay与外部门保持独立

8项Overlay（02/03/05/07/08/10/11/12）各顶层拥有一次Enter/ExitPlayMode并在失败时清理；实际ScreenSpaceOverlay＋enabled CanvasScaler／ScaleWithScreenSize／540×960／match0.5。物理输入严格为540×960→`(16,16,508,920)`、1080×2400→`(32,120,1016,2160)`；观测真实SafeAreaRoot.rect、scaleFactor/lossyScale和首帧，不要求scale恒1，不复用被替代的长屏y=80值。
保留五区尺寸／Stage先压至96k、BottomHud再至104k／H<812k可见fail-closed、最后有效rect、活动drag取消且迟到Up/EndDrag零提交；一份共享TopBar避让Startup/Navigation/Result、resize/version一次更新无零帧、Board方形且渲染/命中同源、无第二Graphic、原槽不压缩、Reference真起笔仍保同pointer与反馈、CommandStrip真拖动不劫棋盘、三抽屉互斥不盖Board及mask零穿透／切换／Unbind／重开／Recovery优先。
Caption须实际EN/ZH八命令含Skip两状态：字号18、autosize off、字/词距0、字形scale1、LR12k/TB8k、完整单行无截短；宽`184/224/200/144/152/256/248/152k`×高48k，disabled仍占宽，Content1624k／Viewport252k／range1372k；首Retry与末Reference整格可达。Restart整格256k可宽于Viewport，但完整文案须可达；不扩成“所有整格同时可见”，也不削弱文字与热区断言。长双语正文／选择局部滚动，Header和安全/危险动作固定可见；精确TMP集合、占位符／缺字／重复ID／失联引用零异常保留。
WorldSpace／关Scaler的旧mapper用例仅证明映射；真Overlay rig仍禁用真实Host并用测试文本源／内存存档，不能证明正式LOC／真实玩家保存关闭复开。Prefab新／改装饰只许显眼灰盒TempArt_、raycast=false，不采用未选素材；静态源码、原生序列化、运行断言、主美实际截图／滚动手感及Android/L1分别记门，正式美术未选定。
本轮只新增本文；只读SHA／JSON集合核对完成，Unity／dotnet／测试执行／联网／Git／设备／安装／新环境准备均0；未改既有文件或S证据。最终回主程一次后结束，不空等C。
