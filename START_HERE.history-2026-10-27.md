# FightMatch 当前开发接续入口

更新：2026-10-27。**首 Demo已ACCEPT，CH1-SLICE-001详细设计已接收，用户现已明确授权继续产品实现。** 当前CH1-IMPL-001按法师8／9→16教学→双面Boss推进；只补受影响验证，GitHub PR独审、不合并，不恢复旧运行合同或重复验收；没有活动Goal对象。

## 1. 当前工作与下一步

**2026-10-26：r3本地发布及六件冷载已限定ACCEPT，版本已推送。** 用户直接许可后，原C沿被拒的同一入口执行publish→activate-export→verify-release，三次均exit0；仅凭六件文件ColdLoaded，未读制作源／publication store，修订3、16关、8能力、Trial绑定正确。QA已接本地功能结果；136／600／44、364旧文件及原prepare／批准保持。最终含准备和收尾333.248494／300秒，超限33.248494秒；首次259.887863不代替最终总时长，旧拒绝／344.191330秒与临时Profiler脚本引发后恢复的CS2001均保留。没有新业务测试、资源构建、Scene／Play或设备运行。

冻结candidate08的14文件／313110字节（六件数据、七meta、实施README）已从准确副本提交推送[PR28](https://github.com/yyczz1/FightMatch/pull/28)，head `bf76a06c04ae5ea92423252ff73feb546376cd9a`、base为PR27 `2e74c989724acceaf47135725a02a4dd16265b5c`，分支`codex/ch1-r3-publication`，draft未合并。master125b849与普通索引保持，源码／测试无增量；主测试已接[准确head独审](https://github.com/yyczz1/FightMatch/pull/28#issuecomment-6415577797)，未发现重大问题，本14路径数据入库限定ACCEPT；超时记录及整章验收缺口保持。证据见[验证记录](docs/implementation/2026-10-25-ch1/validation.md)、[执行总回执](TestArtifacts/FightMatch/CH1-IMPL-001/host/publication-r3-01/authorized-01/result.json)及[版本回执](TestArtifacts/FightMatch/Versioning/20261026-ch1-r3-publication/publication.json)。2026-10-27 用户追问为何再次要求回复并停工，中央纠正将内部拆包误作新授权门：沿既有CH1持续实施授权，由主程交原C执行已备三平台资源／实际inventory与copy／Scene保存复开。新段内部上限900秒、最多4次Unity，含准备等待与收尾；不是用户逐字新增数字许可，原300秒段及333.248494超限记录独立保留。主程唯一接C和QA检查点，中央接里程碑；尚未取得本段运行结果，不重跑旧测试或诊断，不扩GUI／Play、设备或线上部署。

PR26原缺审门已由[同head恢复性审查](https://github.com/yyczz1/FightMatch/pull/26#issuecomment-6407265479)及QA限定ACCEPT解除。旧raw已用公开API在两次Unity中完成加载和有限调用树读取，API段实际494.581243秒；仍因遍历截断无法确认目标及热点，原采集421.206353秒／两启动及各失败完整保留，已停止该诊断，不再要求用户人工Load，也不作为全部开发前置。原13项恢复修正ACCEPT保持，真实r3新档至8／9、16教学和双Boss整链、页面、设备与最终平衡仍未完成。下方旧“缺审／等待许可／人工读取”只代表历史时点。

**当前恢复修正已限定接收：** [PR27](https://github.com/yyczz1/FightMatch/pull/27) head `2e74c989724acceaf47135725a02a4dd16265b5c`，draft未合并、总137路径；[准确head独审](https://github.com/yyczz1/FightMatch/pull/27#issuecomment-6397173825)未发现重大问题。用户2026-10-26批准的新增300秒／最多两次启动已完成：首轮9项通过，因筛选清单命名空间错误漏4项，第二次仅纠正filter补4项；两份XML覆盖原5方法13个不同实例，全部通过、无跳过，源码／测试未变、旧9项未重跑。两次Unity正常退出，136候选／600输入／44保护和字体保持；含准备、必要筛选修正等待及收尾231.704897／300秒，两次启动已用完，无第三次，旧3579.452900／3600及余20.547100独立保留。主测试已给本head的同次Restore复用与守卫回归限定ACCEPT，普通工程收件闭环，无新源码delta或新独审请求。本轮不是性能基准，不证明真实r3整链超时已解决；长链、发布／资源、Scene／Play、设备及PR26缺失审查仍独立未完成，不扩用本次额度。准确证据与限定结论见[验证记录](docs/implementation/2026-10-25-ch1/validation.md)及[执行总回执](TestArtifacts/FightMatch/CH1-IMPL-001/b03-b04/corrections/recovery-current-reuse-01/verification-01/combined-result.json)。

**CH1第四批P2复审完成，运行接续：** [PR27](https://github.com/yyczz1/FightMatch/pull/27) `936f2b173fde52327c2d17f1ab6dd3391f69da69` 基于PR26，draft未合并；[准确head独审](https://github.com/yyczz1/FightMatch/pull/27#issuecomment-6394590011)未发现重大问题，服务completed／findings与unresolved均false，未手动resolve旧P2。经验卡入口保留玩家所选培养角色，嘲讽学习仍针对战士；新增两例及原两例UI共4项通过。教学Restart测试helper修正的单例补验已实际通过、Unity正常退出且输入／保护保持；此前96项组合XML和Boss一项标准UTP限定通过证据按原版本复用，不合并冒称整批XML。用户本轮再次要求继续后，原C沿同一审批入口获准执行，真实r3新档整链已在371.826088秒按上限停止，最后完成第6关、无完整XML；Unity已退出、输入／保护保持。旧拒绝与累计预算保留，余量不足新一轮运行；主程继续源码耗时定位与最小修正准备，不直接重跑同一长链。源码范围仍为136冻结路径中的16教学、双阶段Boss、V5、r3的16关17面／310文本及接线源码，P2仅追加原五文件差量；QA现已给本head的P2修正、教学helper补验及r3准备内容门限定ACCEPT；独立内容批准5131字节／SHA49e08cee…fe5817已交主程，原八件与UNREVIEWED模板／BattleReplayPerformed=false保持。该结论不含未完成的实际整链、r3发布／资源／Scene／Play或设备，不追认PR26缺失的独审，也不声称整树已测或整轮完成。主程唯一接运行和后续实施，主测试接源码／内容与验收门，中央只接发布差量及整体里程碑；详细证据与本地fixture／发布差异见[实施记录](docs/implementation/2026-10-25-ch1/README.md)和[验证记录](docs/implementation/2026-10-25-ch1/validation.md)，不重跑有效旧检查。

**CH1第三批原独审请求缺回执：** [PR26](https://github.com/yyczz1/FightMatch/pull/26) `7a226936751ab265609b0fdc751207648bdffffb` 基于PR25，draft未合并；113路径为112冻结产物／源码及实施记录。实际r2六件发布与仅凭六件冷加载、三平台资源构建、45件runtime／boot复制导入均已有成功证据；唯一源码修正为chapter包采用SDK标准IncludePathInHash选项，避免同字节source／payload的身份冲突。GUI连接两次超时，原进程由既定timeout结束，Scene接线／保存、Play及游戏观察均NOT_RUN，不能当作试玩通过。QA已核原审查running标记自12:37:44（上海）长期未更新，无最终回执或可见检查／作业记录；官方代码审查管理页要求登录，内部任务状态不可得，故不判通过或失败，不再密集轮询或重发请求。该服务门与已完成PR27复审分开；主程继续独立工作，停止重复CUA连接。

**CH1第二批接入已限定接收：** [PR25](https://github.com/yyczz1/FightMatch/pull/25) `1e87c7960fc429b7229862c720520ed9ea42e207` 基于已接收PR24，draft未合并；37路径为36冻结候选加实施记录。Host双入口、uGUI真实职业／结算显示、279文本及发布准备／第二资源包接线已实现；原36通过加9项补测覆盖45项。标准prepare已产出8件未审内容，尚未正式发布／激活，不代表Scene、实际资源产物或页面试玩完成。[准确head独审](https://github.com/yyczz1/FightMatch/pull/25#issuecomment-6393017173)未发现重大问题，QA结合36＋9和真实prepare／字段映射给入口、显示、内容准备及r2 Trial内容门限定ACCEPT，已交唯一内容批准记录；原模板与BattleReplayPerformed=false保持。主程继续实际内容发布／六件冷载／资源／Scene与作者流程验证，随后16教学与Boss；无需用户例行确认。

**CH1首批基础已接收，任务继续：** [PR24](https://github.com/yyczz1/FightMatch/pull/24) `e642d337ca2d8f8d5cf0307ff263eed4771f7134` 为8／9关内容、法师战斗和V5保存基础，draft未合并；69项原通过证据加1项定向补测覆盖本批70项，未重跑69项。[准确head的GitHub独审](https://github.com/yyczz1/FightMatch/pull/24#issuecomment-6392713312)未发现重大问题，QA已给该基础限定ACCEPT。主程继续实际Host入口，随后16教学与双面Boss；此批基础不等于真实作者内容试玩、页面或整轮验收完成。一次主程及短执行者回合中断后已接管原Unity并收回结果，无重复启动，历史失败与unknown退出码保留。主程持续负责组内实现／验证和接收，新的Git发布候选或整体阻塞直接唤醒中央，无需用户逐步推进。准确候选、后续状态与证据仍由下述实施／验证记录维护。

**当前实施 CH1-IMPL-001：** 用户最新明确要求按已接收设计继续实现，普通技术工作连续推进，真实决策集中汇总。主程负责分包、准确文件白名单、短上下文实施、必要修正和下一包；主测试沿既有17组场景选择增量验证并唯一接GitHub独审。主程持有[当前实施记录](docs/implementation/2026-10-25-ch1/README.md)，主测试持有[验证与接收](docs/implementation/2026-10-25-ch1/validation.md)；中央接里程碑并负责发布差量草稿PR。Core／Application／Content／Platform在f94240fa与已审dece3d8c间无差异，可作为当前源码起点；旧UI／测试WIP保留但不混入新发布。新授权覆盖本轮相关实现、必要验证及PR审查工作，下方“仅设计／不启动实现”是已结束阶段的范围，不得误作当前停工指令。先完成法师8／9，再衔接16教学及Boss，不扩后台／广告／CDN或正式发行。

**CH1-SLICE-001详细设计已接收：ACCEPT（仅设计）。** 用户要求的法师8／9关、16关教学和双阶段Boss规则／状态／表结构已形成；主策与主程对齐，主测试按三份冻结稿完成17组场景映射，最终回合`01a16f01-1425-7201-8fdc-eebefdfcfc64/local`。当前无新增用户待选事项，候选数值不冒称最终平衡；尚未实施或运行本切片。准确产物、冻结依据、下一开发拆分及剩余边界统一见[设计包当前记录](docs/plans/2026-10-25-next-stage-design-index.md#当前设计包-ch1-slice-001)。首Demo结案保持，旧验证未重跑。

**2026-10-25 新阶段：版本已保存，开始后续设计规划。** 当前工作区检查点 `f94240fa` 已推送至 `codex/demo-accepted-checkpoint-20261025` 并核对远端；它保存已审产品及既有本地WIP，不代表整棵快照已通过验收。PR22仍draft未合并，本机master及普通索引保持。先读[下一阶段设计计划](docs/plans/2026-10-25-next-stage-design-index.md)、[实际架构与代码阅读指南](docs/architecture/2026-10-25-code-reading-guide.md)和[版本记录](docs/versioning/2026-10-25-demo-checkpoint/README.md)。主策负责首章内容计划，主程负责工程依赖和接口计划，中央只接里程碑；本阶段没有新增Unity／设备／测试运行，也不自动开通云服务。下方Demo结案与历史记录不覆盖本条新授权。

**2026-10-25 首 Demo 验收结案：ACCEPT。** 主测试 actual `01a16eb2-f323-7472-9419-b6043df3be30/local`已接收最后一次有效Skip证据，主程正式收件；既定L1、占位美术、共用正式业务／存档范围内无剩余必验项。沿原输入复用已通过的通关、26XP／2锡片到账、三状态恢复、导航／回退、双语及真人触控／系统操作，不声称APK14重跑全部项目。用户在C执行聊天直接授权后，原iQOO上的APK14完成一次首红L及一次有效Skip；连续录屏证明第3反馈直接跳到当前战局、未播放第4反馈。原9份可读文件保持，仅新增同ID snapshot／commit一对；最终95HP、敌0＋15、单红L，未领奖或重开。实际操作及取证216.481秒，含回传276.186／300秒；先前失败、超限和审批拒绝保留为历史。

交付包：[APK14](Builds/DEMO-NEXT-001/android-14/fightmatch-demo.apk)，SHA256 `c3880130093c928516ca559cc9c36bbf612004a58d958cc4055936e2f191bd8d`。代码已推[PR22](https://github.com/yyczz1/FightMatch/pull/22) `dece3d8c589742b5ee3d75b2e255c97f6bf2495b`，准确head独审无重大问题，按用户要求仍draft未合并。详见[最终QA判定](docs/team/2026-09-30/testing.md)、[实际执行](TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/remaining-01/authorized-20261025-result.md)及[启动与试玩说明](docs/demo/PLAY_FIRST_DEMO.md)。负责人均结束本轮执行；后续功能、正式Google Play发行、服务部署和最终美术不自动启动。以下条目为此前时点记录，不能据旧“剩余／等待”恢复测试。

**2026-10-24 APK14已保档安装，源码独审通过，真人触控与系统操作已确认，剩余验收接续：** 用户批准的五文件已推PR22 `dece3d8c589742b5ee3d75b2e255c97f6bf2495b`，相对ec4be08d仅1提交／5路径／+250−1；[准确head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6380505631)未发现重大问题，QA已接收本轮选中反馈／恢复按钮修正限定ACCEPT，PR仍draft未合并。有效检查为旧选中边框1例＋FIX03恢复2/2；APK14一次构建成功。用户解锁并允许安装后，已装包SHA／原签名匹配，真实页面观察到持续战士白框、普通恢复按钮隐藏及英简中，最终恢复简中；8业务文件与安装前一致，资源／语言技术记录正常更新。原锁屏安装失败和三轮测试失败保留。用户已直接确认白框持续、真人单指拖动有白线且松手消失、第二指不抢操作、拖线Home返回清除且战斗未变、真实系统Back确认与取消正常、必要按钮未被系统区域遮挡；这些实际观察集中交QA，不重测，也不外推新的业务文件哈希。模拟器两处Reference文字、重来预览取消后导航返回及受控真实英文系统无偏好冷启动已完成；原中文系统顺序／偏好完整恢复，18可读业务文件保持，实际版本边界见主程。手机最后一次行动＋本机短录屏因自动审批不接受跨会话权源而被同入口拒绝，已停止重试；行动和录屏均0。QA已复核原PQA要求并复用有效导航／回退／owner证据，撤销额外三页竞速组合；唯一剩余为有效播放时实际点击“跳到当前战局”及立即结束的观察。当前首次行动名义播放约0.9秒（源码推导），不强迫真人竞速。原C待人类在执行聊天直接许可一次行动＋仅游戏本机短录屏；未派第三次重试，原手机100／两敌15现场保持。整体Demo尚未通过。详见[主程](docs/team/2026-09-30/engineering.md)及[发布回执](TestArtifacts/FightMatch/DEMO-NEXT-001/iqoo-01/publish/publication-2026-10-24.json)。

**2026-10-23新增设备阶段已完成、限定ACCEPT：** 含`ec4be08d`修复的android-13已一次构建、同签名保档更新原BlueStacks user0，并完成正常启动→游戏内“退出应用”→冷复开。旧PID3638确实退出、新PID3819；两次BusinessReady，简中、战士95/100及胜利待结算未领奖保持，五时点18份可读业务文件＋语言偏好字节相同（2锁仍不可读）。无新战斗/领奖/花费，旧验证与未变源码独审复用。主测试已收件限定ACCEPT，首Demo整体仍NEEDS_FIX。下一步真机需要USB连接iQOO Neo5并允许调试，已通知用户；不重复旧模拟器项。详细只见[主程](docs/team/2026-09-30/engineering.md)／[主测试](docs/team/2026-09-30/testing.md)。

**当前：模拟器正常主链及四项修正已有实际证据，首 Demo 整体仍 NEEDS_FIX。** 新档建档取消/确认、正常 L1、26XP＋2锡片到账、攻略交互、历史回退、原开局重来、退出本局和四个结果去向已有分段实证；26符合评分公式，基础E20不是最终到账值。已修 Android 新档误判、活动战斗只读导航入口、双语确认正文及 Reference 正文布局。android-11 的中英条件/范围/差异正文与两个步骤按钮已观察，英文差异小标题和中文选步骤说明未取得完整截图，保留为人工观察缺口，不据此声称永久不可读。拖线中 Home 返回清除未提交手势已有实证；已过语言/奖励/三状态恢复等不重跑。原 user0 简中待结算未领奖，18份可读业务文件与此前一致。

当前已安装并限定验收 APK：`Builds/DEMO-NEXT-001/android-13/fightmatch-demo.apk`，46153060字节，SHA256 `b070e9c6d26de0632e6b56eb1658b1d3e9f3d685fa73ee9beeafe56864c9795c`。一次构建0错0警告，原签名/API24/ARM64/IL2CPP/GLES3保持；18固定输入和1221保护文件保持、设置恢复。构建313.022秒、设备625.395秒、全段1262.522/1800秒。已验新包启动、正常退出及冷复开；原299条W/E告警保留，本轮未观察启动失败/异常/ANR/崩溃，不追认旧退出原因。见[构建回执](TestArtifacts/FightMatch/RES-BOOT-HOST-001/apk13/result.json)及[设备回执](TestArtifacts/FightMatch/RES-BOOT-HOST-001/emulator13/result.md)。

历史未安装 APK：`Builds/DEMO-NEXT-001/android-12/fightmatch-demo.apk`，46148818字节，SHA256 `0fff1937d3ba308c419ff1da9fd592e207af0e795feeb76022844765082a0079`。含新Host/CORE，一次构建成功；最终报告0错0警告，同一调用早期3条编译诊断经内部重编解决。原签名保持、1209保护不变、设置恢复，558.892/900秒完成；尚未安装或验证新启动。详情见[构建回执](TestArtifacts/FightMatch/RES-BOOT-HOST-001/apk12/result.md)。

此前完整流程设备验收 APK：`Builds/DEMO-NEXT-001/android-11/fightmatch-demo.apk`，46143120字节，SHA256 `73c8da9bd3d166848e1eb56d75052f68078eef16fcfa0d9aa8e041b5d0fe011f`。本轮一次定点应用、一次构建（0错0警告）及保档安装；无新增测试或下载。七路径发布材料见[候选](TestArtifacts/FightMatch/DEMO-NEXT-001/navigation-fix01/publish/inputs.json)，同范围内刷新最新 Build/Prefab；旧本地 Responsive 测试/WIP 排除，历史试验失败和未匹配用例不改绿。2026-10-22用户已批准七文件推送及独审、不合并；PR22已发布 `4e78fdecc4bb3cbe579d7d13a166b507131d06fc`，远端核对1提交/7路径、draft未合并。QA已接回[同head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6339540133)：未发现重大问题，源码审查门已补，首Demo整体仍NEEDS_FIX；[发布回执](TestArtifacts/FightMatch/DEMO-NEXT-001/navigation-fix01/publish/publication-2026-10-22.json)。RES-BOOT-CORE-001下载控制器纯逻辑已完成，16/16新针对用例通过。用户另行批准四个新增文件后已发PR22 `d06faab14a64b8a49b574e645768df5f2b170e2b`，其独审P2已修于`f79a0ed2`，3项针对回归通过且准确head复审无重大问题，CORE纯逻辑限定ACCEPT。随后Host接线原8项新检查与b141发布保留。2026-10-23按用户授权，连续Editor Play和失败清理重试两项P1已修正，复审发现的同一路径异步最终释放P2也已完成；最终提交`ec4be08d895b7c0d6ea01404b658ea892915764f`已推送PR22。有效必要证据为P1清理2例（p1-run02）＋Editor4例（p1-run04）＋本轮新增异步关闭3例（p2-run01），既有通过用例未重跑，失败历史完整保留。[准确head独审](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6347414049)于10月23日01:48:58上海完成、未发现重大问题，QA本轮限定`ACCEPT`；PR22及旧恢复PR23保持draft未合并，未自动resolve旧审查线程。本轮未重建APK或操作设备，APK12不含这些修正且尚未安装；首Demo整体仍`NEEDS_FIX`。详细回执只见[主程](docs/team/2026-09-30/engineering.md)／[主测试](docs/team/2026-09-30/testing.md)。真实场景smoke01因原生窗口工具约336秒延迟未进入Play，正常退出且原保护文件未变，结果未验证；总段640.920/600秒的超限保留。当时新接线构建为android-12但未安装；现android-13已补模拟器启动/正常退出/冷开，既有android-11玩法证据保持旧输入，真机和HTTP/CDN能力仍未验证。主策说明同步已完成，主程继续收敛首Demo可自主补完项；不因真机未连暂停全部工作，旧验证不重跑。已请用户 USB 连接 iQOO Neo5；还需真机触控/显示/系统返回、Skip实际时序、无偏好系统首开及上述两处文字观察。旧两次退出因触发前日志缺失仍未归因，不盲重跑。唯一明细见[主程](docs/team/2026-09-30/engineering.md)、[主测试PQA表](docs/team/2026-09-30/testing.md)及[试玩指南](docs/demo/PLAY_FIRST_DEMO.md)。

### 已完成里程碑与历史（按需读取）

**2026-10-18当前里程碑：英文战斗排版修复已推送并获独审，本轮集成ACCEPT；首Demo整体仍NEEDS_FIX。** 用户要求“那你就继续修复”后，主程接收English-layout01：正常播放与待结算的中英文本在920/812高度下完整放入原文字区域，棋盘/触区/最低高度保持；修后单项完整流程检查1/1通过，早期夹具失败及真实溢出失败保留。android-07一次构建成功并保档安装，实际中英活动/待结算画面、格中心拖线均已观察；正常动作推进至待结算但未领奖，原16份可读业务文件保持，仅新增一对正常snapshot/commit。运行含证据收尾1021.429/1200秒（961.172为进程/哈希后检时点），44保护文件不变，Unity已退出；旧语言/结算/三状态证据复用。

APK为`Builds/DEMO-NEXT-001/android-07/fightmatch-demo.apk`，46134553字节，SHA256 `dbd5a5cc0646b15009f72a5a26bb6ab7c006ce06a659761eff81217bf7c59204`。用户随后明确允许这4个文件上传`yyczz1/FightMatch`的PR22并发起独审、不合并。中央已推产品`1e4cfeaea7de5d9b7eeeddddf74a8b45a2c06a25`，远端对745a5f5实核仅1提交/4路径；旧拒绝保留为历史，当前发布阻塞已解除。QA已接收匹配head的[GitHub独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6250967784)：22:17上海完成、未发现重大问题，结合已接本地实际证据给出本轮排版修正ACCEPT。PR仍draft未合并，历史未归因退出、其他完整流程/页面及真机缺口保留。唯一明细见[主程](docs/team/2026-09-30/engineering.md)、[实际结果](TestArtifacts/FightMatch/DEMO-NEXT-001/english-layout01/result.md)和[试玩指南](docs/demo/PLAY_FIRST_DEMO.md)。

**当前：两项UI修正已获主测试限定ACCEPT；首Demo整体仍NEEDS_FIX。** 已审产品`af35b6006113d8b924150f67a9cdd57b7421deca`接入，3项针对检查全部通过、零跳过，android-06一次构建成功并保档安装。用户新增900秒许可实际使用873.734秒、构建1/2；旧A511.292/B726.724独立保留，无下载/清档/全量。APK SHA256 `66bafa41b30f773461bbd67642083f160ec3f4d1e2a4f5d1ccc663863f97290f`，46139736B，见[试玩指南](docs/demo/PLAY_FIRST_DEMO.md)。

**原问题后验：** 中英均按“刚切语言收菜单→不清焦点→不选战士首次按住/松开棋盘”验证，语言控件整块像素不变；键盘打开后触屏选项/遮罩关闭，触屏打开后键盘选择/根与选项Esc均实际通过，Esc只收菜单。短拖线预览释放后不提交，拖线期间的语言门禁由指定针对用例证明；保档安装前后及结束时可读业务字节一致。结算入口、显式26经验/2锡片到账及三状态冷恢复沿用已接收的android-05证据，本轮未重跑、未再结算。最终简中、原95HP及两敌0/15和15/15现场保留。

产品独立审查[6249275509](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6249275509)无重大问题，本轮无新源码差量。[主测试](docs/team/2026-09-30/testing.md)已接收本轮语言/结算修正，限定结论ACCEPT，唯一执行详录在[主程](docs/team/2026-09-30/engineering.md)。英文战斗文字重叠、历史未归因退出及其他完整Demo验收缺口仍保留；旧两项场景自动检查的布局前置失败不改写为通过。以下为历史时点，不覆盖本条。

**2026-10-18实际结果及接续：** 用户“许可”后，四份已审启动修复源码接入；8项定向全部通过、无跳过，`android-02`普通APK构建成功，检查与构建总段404.808553秒（≤15分钟）。新包SHA256 `e50b711ecbebc7d380fa62d1abd5d7bbedfda0760b292eba2cf6eb17ea8b2480`，API24／ARM64／仅GLES3／原签名。保留测试档安装及普通图标启动后，越过原进程身份故障，但报`RES_STORAGE`；L1／三状态复开／奖励均未运行。设备段354.195276秒（≤20分钟），六份可读业务文件前后相同，两份锁文件不可读，不外推全部存档字节证明。主程继续同任务最小存储诊断与修正准备，保留现场、复用证据，不清档或放宽守卫；必要相关后验仍受本轮时限约束。源码审查沿用`1d75386b`，本轮未新增源码差量。详情：[主程](docs/team/2026-09-30/engineering.md)、[定向与构建结果](TestArtifacts/FightMatch/DEMO-NEXT-001/startup-run01/result.json)、[设备结果](TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-02/result.json)。首Demo仍NEEDS_FIX。

**最新接续（2026-10-17）：** 用户直接确认“可以，现有模拟器存档就是测试档”，现有档用途阻塞解除，主程已收到继续原模拟器安装/试玩任务。保留现有档，允许正式游戏自然推进；零下载/重建/卸载/清档。昨日约6分钟已执行前检复用，本次自限最多24分钟连续设备操作及收尾，不另开30分钟。昨日17:41绝对截止系内部增加的约定，并非用户要求答复在当日到达；本次按最新答复恢复剩余验收，等待人类答复不计测试耗时，原时间证据不改。实际已覆盖安装并从普通图标启动，但初始化报`RES_STATE`，画面停在诊断占位符；L1/三状态恢复/奖励未运行。此次设备段196.870357秒，加昨日累计555.741022秒；未清档、重启或重建。已确认`Process.StartTime`通向IL2CPP不支持的接口；主程已收完整四源修正候选：Android改用boot/PID/elapsed身份及资源技术记录v2，保留玩家档；pre-bind诊断显示安全错误码。四源已发布PR22产品提交`1d75386b2ba30e6746cb0123b12e86dfa70f4907`，[同head独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6228731368)未发现重大问题，源码审查门ACCEPT；本机未接入/编译/运行，等待必要后验；修后效果尚未闭环，首Demo仍NEEDS_FIX。详情沿[主程同任务记录](docs/team/2026-09-30/engineering.md)及[实际回执](TestArtifacts/FightMatch/DEMO-NEXT-001/emulator-01/resume-result.json)。

**当前模拟器验收已授权（2026-10-16）：** 用户对操作卡`call_95ad0e34cf5649b3b6f2dd45d745bfb2`直接回答“允许进行这轮模拟器验收”。主程已派原C并接收环境前检：现有BlueStacks/API33/ARM64、旧普通包及同签名已核；发现已有玩家档，历史无法证明其测试用途，故安装0、游戏启动0、试玩未运行。补查只有一个Android用户，当前包没有现成隔离资料入口；中央已一次询问现有档能否作为测试资料，等待答案，原资料不动。整轮最多30分钟，原连续截止2026-10-16 17:41:19上海不重置；目标仍为L1及战斗中/待结算/已结算三状态复开，零下载/重建/卸载/清档。实际结果和剩余时间由[主程](docs/team/2026-09-30/engineering.md)唯一记录，首Demo仍NEEDS_FIX。此为构建结案后的独立新许可，覆盖下方旧时点的“不追加安装”，不复用历史运行预算。

**最新里程碑：新版普通APK已构建成功，首Demo仍待验收。** `Builds/DEMO-NEXT-001/android-01/fightmatch-demo.apk`为36583902字节，SHA256 `4d72f265bd437d9d0e5131128ae46f71fb4fd968310f3a2d93344a791c017ab6`。用户明确许可后的唯一构建自然exit0，前检至结果落盘252.466秒；minSDK24、仅GLES3及Auto=false已由Unity实际保存，原包名/签名保持。三项针对检查为原2项通过＋修正锁文件基线后单项通过，原失败不覆盖。四源码及真实Settings共5产品路径已推送PR22产品提交`c78f8a52ba589534171b5990328e9cac2d42a752`；主测试核对[同head独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6211066540)未发现重大问题，仅源码审查门ACCEPT，首Demo整体NEEDS_FIX。本机保留字体/旧WIP不同于clean PR输入，不能声称整个PR树已全测。依据[实际构建回执](TestArtifacts/FightMatch/DEMO-NEXT-001/execution-android01-result.json)和[试玩指南](docs/demo/PLAY_FIRST_DEMO.md)。尚未安装、普通图标启动、完整L1或设备验收；本轮一次构建许可已用，不追加运行。以下为准备及旧UI过程记录，时点状态不覆盖本条。

**当前接续：DEMO-NEXT-001（用户2026-10-16直接要求）。** 本轮UI已结案，用户要求列清首Demo未完项并继续能独立完成的工作。主程核L1正常流程与新版Android普通APK准备，继续已定API24/GLES3及必要构建入口的最小候选；主测试合并当前有效证据与剩余实际验收；主策接管`docs/demo/PLAY_FIRST_DEMO.md`更新真实uGUI试玩说明。两项源码已暂存冻结，用户已正常退出Editor；运行按新阶段短合同，不复用已结束且超时的UI启动/时间额度。各组直接完成普通任务，中央收里程碑及必要人工动作；不因旧LOC/1036冻结历史恢复长验证链。首Demo仍限已批准L1、占位美术和同产品正式业务/存档；广告、后台、CDN、账号及完整Google Play首版不扩入此阶段。

**本轮候选已齐，用户已正常退出Unity。** 用户已对本次退出操作卡直接回答“已正常退出Unity”，中央已转交主程继续。Android构建器/API24/GLES3预期设置，以及建档前可取消确认的三源候选均已冻结；截至派发尚未接入、编译、测试或生成新APK。中央已核建档三源前像与PR22 `9982575`一致；Android发布使用独立clean适配，保留本机旧WIP。主程派唯一C先核无Editor占用，再按[当前短合同](docs/team/2026-09-30/engineering.md)运行3项定向检查及一次普通APK构建，实际源码/设置形成后一次发布并交GitHub独立审查。不需要再次询问同一退出或运行许可；实际结果以主程收件为准。

**后续实测与审批历史：** 两项原定向通过；建档确认用例先因空锁文件基线错误失败，FIX01修正后实际1项通过，未重跑前两项。四源码已接入/编译，最终测试源为`d30177d5…`。TMP动态字体变化保留为本机冻结输入`9d477445…`并排除源码发布，不声称已逐字段证明全部为缓存。首次APK尚未运行时，派发被自动审批以“仅GUI、禁止构建”拒绝；沿同入口补充本轮人类请求/时间证据后第二次仍被拒，已停止该动作，未换入口。详见[主程当前记录](docs/team/2026-09-30/engineering.md)。

**最新真人明确批准（两次拒绝之后）：** 用户对操作卡`call_cd5bd0cebc2243f7a297bb3ae5914c8c`直接回答“允许本次派发和一次APK构建”。范围为主程向原C派发首次普通Android APK，仅一次、最长15分钟、外置盘/现有缓存/零新下载，Unity API保存已定Android7和仅GLES3设置，保留当前字体与玩家档，不增加测试/GUI/安装/额外重试。中央已交主程沿原入口执行，并自限本次前检/构建/后检总段≤900秒，保留原FIX01累计时间约束。实际执行结果尚待回传，许可与派发不能代替APK成功证据。

**最新追加验证授权（2026-10-15）：** 用户在turn `01a14023-7ebb-7a90-b1e8-8b3acd5feb3e` 明确追加最多2次Unity启动、主动检查合计900秒，复用现有隔离档，完成下拉/中英文按钮/返回键/语言保存复开，不重跑全量测试。此为独立的新额度，旧2/2保留历史，不混算。派发首次因混算旧额度被自动审批拒绝，补充实际人类授权时间证据后，同一入口已获准送达C；当前先验证固定产品候选，计数和结果由主程唯一接收。

用户2026-10-15直接批准：使用隔离测试档启动Unity最多两次，完成下拉框、中英文按钮和语言保存复开；推送本轮修改到PR22并发起GitHub审查，普通步骤不再确认。主程接收本轮UI收尾，C唯一运行Unity，中央负责发布。此为上轮实际拒绝后的新授权，不复用旧N07额度；实际启动和结果以主程当前记录为准。

- **已完成：** 当前264条文本与既有导出逐字段一致；标准Luban重复键检查2.541秒，按预期exit1并报告重复key，正式输入未修改。见[实际结果](TestArtifacts/FightMatch/LOC-STANDARD-001/result.md)。没有新下载、依赖还原或上游重建。
- **当前完成与阻塞：** 正式文本/内置资源/Host已接线；原Mac正常Session和简中曾实际观察，按钮早期自动点击无响应；随后人工操作反馈正常（见本轮修正）。三平台资源选择、Windows既有存储/路径入口接线已修，平台定向检查13通过/1 Windows原生跳过/0失败，Android和Windows真实raw构建成功，Mac产物复用，Scene已绑定三平台。原103pass、Bridge5+HCP8及最后Fake3/3均保留。源码/资源已推送`ef4c33f`至PR22，第三轮独立审查已完成，详见下方收件。详见[运行记录](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)、[平台证据](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/platform-verification.json)，只读[主程索引顶部](docs/team/2026-09-30/engineering.md)取具体范围，不从历史恢复旧任务。
- **前一版UI实现与证据（fe1ced8）：** 原46个Button（含3模板）已统一调整中英排版；语言窗口改为TMP下拉框，保留偏好保存与输入/解绑保护。14产品路径固定；Prefab SHA`77c17069…`，最终测试SHA`f0052bb5…`。Apply27.57秒exit0；15项定向为12通过/3失败，其中重绑定一项只修测试旧控件定位后1/1通过38.09秒；两个无图形窗口用例仍未通过。不得写成15/15或整个clean PR树全测。详见[主程索引顶部](docs/team/2026-09-30/engineering.md)和[实际记录](TestArtifacts/FightMatch/RUNTIME-INTEGRATION-001/result.md)。
- **GUI实际验收（2026-10-16）：** 用户先在c07d807实际Play确认等宽左对齐、Esc只收列表、中英可见按钮不超框；C随后接入已审c3f0812两源候选并完成第二次GUI冷启动和编译。用户在新实例明确确认：未重新选语言即为English、原玩家档/进度正常，切简中再切English时每次成功提示约2秒后消失，最后English并保持Unity打开。C已核接入前语言及原档主要文件保持；实际预算和日志仅见[主程顶部](docs/team/2026-09-30/engineering.md)。追加启动已2/2，不追加启动或全量测试。
- **本轮发布/审查：** 产品提交`c3f0812404a1223a4b5024cca7617aa054e4cc40`已推送PR22并接入本机，源码编译成功；[独立审查](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6207548412)未发现重大问题，主测试仅接收该源码门，不是formal APPROVED。修正复用现有HUD条，不是独立通用Toast系统。新增UnityTest未执行；本轮受影响正常行为采用实际人工验收，原自动化证据及两个旧headless失败保留其输入边界。PR保持draft未合并。
- **验收：** 按[主测试8项清单](docs/team/2026-09-30/testing.md)检查真实启动、文本、语言切换、保存复开、地图/战斗及存档连续性。源码、自动化、实际观察、真机分开记录；新增代码交GitHub PR Code Review。
- **上一轮独立审查已收件：** [PR22第三轮结果](https://github.com/yyczz1/FightMatch/pull/22#issuecomment-6143365353)于2026-10-13 18:17:56完成，准确审查版本`23f00824db710f0c35299e7006f824384a267f02`，服务未发现重大问题，主测试接收此版本的源码审查门。两个旧P1已过期但GitHub线程未关闭，不能称formal APPROVED。当时后续只补文档；本轮fe1ced8另行审查已发现返回键P2，修正见当前条目。
- **本轮UI集成判定：ACCEPT。** 依据c3f0812独立源码审查、实际编译、既有下拉/返回键/可见双语按钮观察，以及本次冷开语言/原档连续性和提示自动消失的用户验收。此结论仅覆盖本轮UI修正；首Demo完整玩法、安卓包/设备、Windows原生验收仍未完成，不把未到达页面或新增未运行测试标通过。
- **执行偏差：** 本轮C收尾触发主动时间`<900秒`守卫断言失败，已停止新增验证，不移除守卫重试、不重置预算。启动2/2未超次数；超时如实保留，不写成全部执行约束合规。该过程问题与以上窄UI功能验收分开记录，精确可证耗时由主程收件。
- **已停止：** Luban M17/FIX11自制还原/验收链。旧失败与候选保留；Yaml旧依赖许可证资料单列未解决，不再以重建整套工具链作为游戏接入前置。

角色与跨组状态见[团队快照](docs/team/2026-09-30/README.md)。详细历史只在定位特定证据时读取：[本次入口归档](docs/team/2026-09-30/central-entry-history-through-2026-10-13.md)、[旧执行次数与原因](docs/team/2026-09-30/central-loc-execution-history-2026-10-13.md)。历史中的“当前”“待激活”和一次授权均描述原时点。

## 2. 执行规则

用户于2026-10-14批准流程精简：新派工先按[任务分级](.agent/TEAM_WORKFLOW.md#0-按任务选择流程)选择短流程或详细设计；小修正由负责人直接交执行者，同一记录内修正和接收。本次UI与PR22权限来自上节2026-10-15人类新授权，流程变更本身不构成工具批准。

[AGENTS.md](AGENTS.md)是规则入口；另读[项目事实](.agent/PROJECT_CONTEXT.md)和[团队执行约定](.agent/TEAM_WORKFLOW.md)。中央分发/接收，五位负责人分工；仅 C 串行运行本机 Unity。代码审查只用 GitHub PR Code Review，自动化测试优先 dots；当前云环境缺就绪证据；本轮本机验证由C按已授权接入范围执行必要短命令，遵循团队约定第5节的简化规则。

普通已授权技术修正继续完成，不逐包重问用户。仅在确需用户决定或工具审批实际拒绝时说明具体缺口；不要把历史授权当新工具批准，也不要把一个独立阻塞扩成全项目暂停。每包一个完成接收者，使用完成事件/等待工具接回结果后继续下一就绪包。

默认主要角色 `gpt-6-astra/xhigh`，明确机械任务可用 Luna。保留有效证据，源码固定后只做必要验证；新增失败/修复/源码变化才追加。旧 N07/N10/N11/I01 的 native 额度不可复用。

## 3. 产品与环境边界

Demo 与正式安卓产品共用代码、规则、存档和正式流程，只减少内容。运行时 UI 用 uGUI，Editor 用 UI Toolkit。英/简中、预制体诊断占位符、Luban 表格和自有本地化绑定、YooAsset 路线不变。美术继续使用占位资源，未授权购买新素材。

当前项目在外置盘，Unity2022.3.18f1 Intel。2022.3.62f3 仅是发行准备候选，未升级。Android7/API24、AdMob 单来源、R2 等批准设计不等于已实施/验证；首版网络范围及后置账号/云档见 AGENTS 和角色背景。

本机 master 基线125b849及既有 WIP 保留。审查分支已有独立推送，PR 均仍 draft 未合并；不要在脏工作区 pull/reset，不自动合并、发布、部署或购买服务。具体 Git 写权限沿用户已授权范围和任务包。

## 4. 角色背景与历史

需要背景时按职责读取：[系统设计](docs/migration/2026-09-28/SYSTEM_DESIGN_CONTEXT.md)、[架构](docs/migration/2026-09-28/ARCHITECTURE_CONTEXT.md)、[策划](docs/migration/2026-09-28/PLANNING_CONTEXT.md)。日常接包不重读全部历史；实际 thread/host/turn 在团队快照及任务回执登记。

原迁移启动模板、旧第4节授权文本、历史暂停和验收记录均逐字节保存在 [2026-10-08 接续入口历史](START_HERE.history-2026-10-08.md)。它们是历史证据，不是新的派发或执行指令。本页整理入口，没有改变用户决定、扩大实施范围或重新启动旧任务。
