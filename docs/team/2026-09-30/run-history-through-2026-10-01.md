# FightMatch 五角色团队：本轮运行登记

2026-09-30 · r2 · **用户已恢复后续开发，准备睡觉，要求明天验收。** 本轮从已推送的 `125b849be13fe2ebf5b1185f3cdb83d19c6327ef` 接续，按已批准组织持续分发、接回结果、独立审查、修正及推进下一包。

本轮目标是首Demo在同一产品代码上的可试玩与实际验收，必须落实已定运行时uGUI及相关本地化要求。保留已验收业务、存档语义、性能优化和原失败证据，不重做内容／账号／广告后台全首版。既有029的设备和独立产品R缺口保持可追踪；明早报告实际做到的范围，不以旧APK或旧绿测冒充新验收。

## 当前接续摘要（2026-10-01，代码审查切换GitHub PR）

用户最新要求：代码审查改交GitHub PR Code Review，自动化测试继续优先交dots。已通知主程、主测试停止新派本地R代码审查；旧结论保留。接收流程以[TEAM_WORKFLOW第7节](../../../.agent/TEAM_WORKFLOW.md#7-github-pr-code-review)为准。当前产品／测试写入和Unity均已静默，中央准备审查分支；ARCH仅做超时只读诊断。

| 项目 | 已成立事实 | 当前下一步 |
| --- | --- | --- |
| uGUI源码／资源 | FIX17仅修H01/H04测试生命周期，Host30为30/30；Core190在420.03s超时无XML，60s等待后核准身份单次SIGTERM，exit -15且进程树清空；产品29文件、37资源及字体398/4稳定 | 保留FIX15 exact2有效结果及历次失败；只读定位Core190停点，222并集及复开未完成。冻结代码准备GitHub PR审查，不新增本地R，无新APK。 |
| UI文本 | COPY-AMEND-03-FIX-01的263键已独立ACCEPT | 使用`snapshots/ugui-copy-amend-03-fix-01/`只读快照，避免后续制作稿更新打断本包。 |
| LOC文本 | AMEND-04新增`fm.language.save_unknown`，264键已独立ACCEPT | live CSV SHA `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`仅供后续LOC；不改UI263快照。 |
| Luban | M09 runner验证有效；M11取得首个旧NuGet包，现代license字段缺失而封存；包内MIT全文及notice实际存在，旧包证据修正规则8694172已获本轮最后一次设计R ACCEPT | 不改写失败；下一材料续作尚未执行，复用runner及已取得字节，Linux签名和剩余依赖门未完成。完整M、runner及LOC-A未接收。 |
| 布局 | 修订9c3d6d设计R ACCEPT，主美ART05限定ACCEPT；SYS已备实现草案 | 待功能基线／实现安排；实际TMP容量、滚动操作和设备画面未验收。 |
| YooAsset | RES-01设计与3.0.6固定提交探针方案已独立ACCEPT | 包安装、编译、资源加载和平台／下载验收未执行。 |
| 安卓 | API24、APK结构路线及GLES3图形方案FIX-01A设计均已独立ACCEPT | 按post-uGUI／API24真实源码另签实现；新APK未生成，旧019普通图标Vulkan崩溃仍未修复。 |
| Git／Goal | HEAD与已推送基线仍为125b849；当前开发明确获准 | 新WIP未提交；工具Goal仍paused，当前总控持续工作，防休眠进程仍在运行。 |

本轮尚不能声称首Demo已完成或新候选整体验证通过。当前没有需要用户重新决定的技术问题；iQOO实际设备门待后续连接和验证。

最近交付链：uGUI功能验证及GitHub PR代码审查 → LAYOUT实现、实际画面及PR审查 → 同源API24／GLES3／APK结构验证入口 → 新uGUI内部试玩APK → BlueStacks图标冷启与L1流程。LOC、语言偏好和YooAsset按真实依赖继续；尚未被内部候选用到的CDN／账号／发行门不阻塞首次新APK。内部试玩须列出最终Demo剩余项；iQOO未连接不阻塞模拟器证据。测试以实际fullname证明覆盖，无隔离必要时不重复跑已包含的focused集合。

11:05接续：M08随后发生的新回执错误独立于上面的历史元数据修正，中央已明确批准最小M09续作。保留M08实际预检PASS与后续失败，二者不能互相覆盖。[原预检工具记录](snapshots/loc-m08-original-preflight/tool-record.json)从read_thread结构化结果机械导出，SHA `346575eb3f91467c7898635d6a37ceb06b346f85d527b013896ba8b244f21c6e`，来源为LOC线程回合`01a0f556-0178-74f2-a475-df88c8bd94e6`／工具`exec-31dbf858-aaec-4804-8861-e263049c4cd3`，exit 0、TOTAL PASS 52行／13组／2021文件。导出不是重新执行，也不声称是PTY原始字节流。普通技术修正由主程依现有授权签发，不逐个continuation重复等待中央许可。

FIX14实际C回合`01a0f55f-32d8-7123-a65f-8fc66800cf5a`已completed，候选`f95c76420a72877ceea9465662e5532e6eaeab2438b78d3fadfdb6014098ebf9`为`BLOCKED_EXACT2_WITH_RESOURCE_DRIFT`。LAYOUT原SYS修订回合`01a0f563-13d4-75e2-9dc2-1017d078f835`已completed；修订文件682行／46746B／SHA`091b2e198b6674b9dceb7df27db9ed10bc405423ea45c1ee939d06aefb65c6f1`，保留旧设计并以附录修正，不以设计通过代替真实画面。

**M08-FINAL-01（中央已向主程、主测试、原LOC owner发送同一裁定）：** 中央完整读取并核对当前磁盘版本后，固定[执行包](engineering-loc-license-alt-m-materials-continuation-08.md)162行／10715B／SHA`aa334b591ca6768fdd68ce50a7dc9ca63e535778ae17999c86d2ddd43b64d0bb`；checker为`3eb16dea006dd69ff37c223e265a1955be51f9f36b3a4c83494dce8e4c2ab69c`，table为`bb64603c6dad03e1e928f96cc2da40286f21ca1bf5fba0e22ef275e69c9a97bc`。这三件套不再因过时消息改动。保留初始4523版的错误历史turn、ec64中间通知、实际唯一PASS、STOP、正确容量复测和合法空根创建时序；不得倒写历史为“始终采用最终版”。正确statvfs为tmp 95,846,400,000B、外置442,368,000,000B，原7.34TB计量无效。当前按唯一固定版本接续，不重跑checker／已成立容量门、不重建合法空根，不因这次元数据纠错产生M09；新实质技术失败仍按原停止门处理。

09:55接续已核实：lock-set设计修正由原R在回合`01a0f524-3498-73e2-81dd-759bd648af3b`给`ACCEPT（设计修正）`，final为`msg_0d29bf8d726d8dc0016abdbbf416a887d0b1d2a39e8b6101e5`。接受文件384行／21402字节，SHA`4d377d0f2629b67f21c249481eaa5d73d845edfc56852a985f43e337c9c0e62b`；仅关闭设计门。M04包226行／14058字节，SHA`a359488d8d93eb7ee9df50438bf8cf0d9199473b1ce44d14a4410637209d2f10`，从M01重新取得已封存离线材料，四次单节点restore生成与验证两套逐项目lock/assets；不复用M03的不完整图，也不新增网络。完整材料、修补runner及LOC-A适配仍须各自实际证据和原R接收。

FIX11先在C回合`01a0f511-5532-7bd3-8aed-832c5c8d41cf`因`BLOCKED_STATIC_READINESS_CONTRACT`停止，零源码修改、零Unity运行。主程最小补签后，同包227行／12851字节，SHA`753faa95d11827e2a0624fdea4dbddd565d2a731d64c3fc0cc7f79ea576a0124`：无参Ready允许启动页隐藏棋盘，显式弹窗目标才检查active，两者只等三帧。新C回合实际进入六文件修正与精确28验证。筛选器须完整转义参数化名称，保留真实反馈计数等断言；Scene和布局仍另包，不以本轮功能测试替代视觉验收。

## 用户授权与当前边界

- 用户本会话先批准“以后就这么执行”，又要求按该方式继续开发。授权在FightMatch团队内创建／复用角色会话、分发限定任务、互相返回任务结果并持续协调，不逐包重问。
- 本轮恢复覆盖§457／461的产品暂停指示；历史报告原状态保留。工具所读旧Goal状态仍为paused，已请用户在界面恢复目标模式；这不阻止当前获准任务继续。
- 普通技术问题由专业负责人处理；改变已定产品规则、框架或额外付费／人工外部操作时，先完成独立部分，再提交具体问题。未获批准的美术A/B/C候选不能写成用户已选定。
- 独立接收后沿已有授权保存结果并正常推送master，Git写入由中央唯一协调，避免各会话同时提交。不得强推或把候选冒充正式发布。
- C是当前唯一Unity本机执行者。主测试统筹验证；云端执行必须先核环境、源码和准确入口，不与本机重复跑旧长测试。中央、主程、主策和主美不自行启动Unity。
- 初期共用当前项目，文件写入范围互斥。下级包必须逐项列出修改／新建路径、公开API／资源／meta／设置权限。共享契约的维护权由主程明确登记后交接。

## 角色登记

| 角色 | thread / host | 当前任务与状态 |
| --- | --- | --- |
| 中央AI | `01a0e401-511d-79f2-b47f-3ab0ade1681b` / local | 由原本轮SD00转为中央；维护本登记、全局依赖和收件，不写产品代码或执行Unity。 |
| 主程 | `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / local | 首包设计已过R，C实施已启动；继续技术调度及LOC／RES后续设计。 |
| 主策 | `01a0f2e3-2c47-7273-88da-20513bcbfaf4` / local | PLAN-001及COPY-AMEND-03-FIX-01／AMEND-04已交接；UGUI固定263键，LOC制作稿264键，各有独立R接收。 |
| 主美 | `01a0f2e3-3a47-78f2-819c-cbe52e8d7e47` / local | ART-02回合`01a0f504-990f-7c11-be0c-5fc285dcb09d`已completed／NEEDS_FIX，并已接中央层级裁定；后续参与LAYOUT专业接收，不扩产画风。 |
| 主测试 | `01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / local | 统一调度三个独立R；UGUI接线／LOC设计已接收，接后续实现候选与Android修订；每个候选和每个审查者保持唯一任务归属。 |
| 实施C | `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / local | gpt-6-astra/max；FIX14回合`01a0f55f-32d8-7123-a65f-8fc66800cf5a`已completed／BLOCKED_EXACT2_WITH_RESOURCE_DRIFT，1/2；仍为唯一Mac Unity执行者。 |
| 独立R | `01a0e404-e8ee-7310-8388-9260babd53f1` / local | gpt-6-astra/max；首包设计FIX-02已正式ACCEPT；代码／证据审查仍待实际实现。 |
| 补充独立R-TOOLS | `01a0f3e3-9663-7be2-84a8-ed23be32f0b9` / local | gpt-6-astra/max；UGUI-COPY-BIND FIX-02已独立ACCEPT，后续候选由QA调度；同候选必要修订仍回原审查者。 |
| 补充独立R-ANDROID | `01a0f40f-6bc6-72d2-b3dc-eb954ad0716a` / local | QA核为gpt-6-astra/max；图形设计FIX-01A回合`01a0f421-6151-76f2-93ac-03b6419b42d8`已completed／ACCEPT；只读，不运行Unity。 |
| LOC-A实施 | `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / local | 8个Assets外路径候选冻结；M08回合`01a0f55a-992f-7e80-ba2a-b18ec9448462`已completed／BLOCKED_RECEIPT_FORMAT；M09已获中央继续许可，尚不进入B/L。 |
| 既有云环境准备 | `01a0f13c-3d87-753a-ad11-16302add9084` / durable | 专用目录8.0.425 Linux x64完成一次同包验签并获限定R接收；无Unity／产品测试，Android组件仍缺。 |
| 既有美术制作 | `01a0ec79-5b64-7b62-8365-bee9229512b5` / local | 原“FightMatch 美术与资源创作”；主美按成果接续。 |
| 像素原型制作 | `01a0f026-e238-7522-be19-a3441f0fa5fb` / local | 原“FightMatch 像素战斗原型”；A/B/C仍待采用，不重复生产。 |

四位主角色已由create_thread实际建立并开始首轮任务，使用用户当前默认模型设置；C/R保留已指定模型。准确首回合已由wait_threads核为inProgress：主程`01a0f2e3-1bd9-7740-ad2c-ddadbe801d26`；主策`01a0f2e3-2d4f-7123-b2a8-0341335ccadc`；主美`01a0f2e3-3bae-72f0-ba71-7de2340e7993`；主测试`01a0f2e3-4cb6-7a10-af28-7b412b2f327b`。首次创建、身份告知及完成状态读取均已成功，完整实施／验收回传仍需实际后续结果。

## 第一轮实际回收

- 主程已创建架构会话`01a0f2e6-1ac3-7d10-8855-54192b9489d4`和系统设计会话`01a0f2e6-29bb-7092-abf0-705b41b7bf93`，均为local。系统设计先盘点，接到固定架构后冻结；准确子任务范围见[主程登记](engineering.md)。C尚未收到实施包，R由主测试唯一排期。
- [主策交付](planning.md)为`CROSS_ROLE_INPUT_READY`：L1正常链、页面异常、双语文本合同和PQA-01～15已交接；170键[双语CSV制作稿](planning-localization-draft.csv)经最小修正后由主策专业`ACCEPT`，SHA256为`fc2429d2cd1495a0a29c805a2ce202316a61f692a64debd7c46130b1f8c68ee5`。仍需正式Luban映射、运行时绑定和设备证据，不能称为本地化已接入。
- 主策收口回合`01a0f303-937e-7781-9e5c-2adbf3d712da`已正式completed并待命；文件维护权保留，具体技术／布局／QA反馈可按既有授权唤回。该角色阶段结束不暂停中央或产品开发。
- 随后主程签发`COPY-001-AMEND-01`，主策仅新增`fm.language.save_failed`并同步制作稿登记：171唯一键／9列／38308字节，完整CSV SHA256为`c4fc583344392153e97a096b68f5d740d8ea161d1bada87bae42343b20b6fb4c`。其余170行内容和顺序保持；中英参数集合校验和diff-check通过。文案说明当前会话已切换语言但偏好未保存；这仍是制作稿，不是已接入的持久化或错误展示证据。
- [架构报告](engineering-architecture-001.md)已由主程专业接收，保留业务／Application／保存语义，语言偏好独立持久化；系统设计据固定架构冻结实施范围。该专业接收不替代后续独立R设计审查。
- [主美核实](art.md)：9月29日整批曾被用户否决；9月30日像素战斗角色A/B/C没有采用许可。两批不得导入作默认资源。本轮只以中性、可替换灰盒完成结构和玩法验证，最终美术仍待用户选择。
- ART-PROD-001制作回合`01a0f2ec-f1e8-7331-95d6-954dbb23c74e`已completed；主美对[8件灰盒布局交付](../../../ArtSource/FightMatch/2026-09-30/ugui-layout-prototype/README.md)给专业`ACCEPT`，包含三组SVG／PNG、说明和资源登记。范围仅为布局／接口，不代表Unity导入、最终美术或Android画面通过。中央已核读旧`ArtSource/FightMatch/2026-09-29/unity-import-plan.md`的撤销标注：只同步用户已否决该视觉及uGUI纠正决定，历史正文保留，不扩大导入权限。
- 主美首回合`01a0f2e3-3bae-72f0-ba71-7de2340e7993`已completed并待命；后续由主程／QA向其提交实际装配或画面反馈，中央继续推进程序线。
- 主测试首轮环境读取发现ADB可用但设备列表为空，BlueStacks已安装但未运行，iQOO未连接。暂无本轮设备玩法证据；云环境与验证安排以[主测试登记](testing.md)的实际回传为准。
- QA已接回R就绪回合`01a0f2ef-b073-7b81-9f36-bb72bd46ec39`及云核对回合`01a0f2e7-48a5-729c-9a6c-b46756472048`。云端仅具备旧Linux Unity；源码仍为9d416e6、无AndroidPlayer及其SDK/NDK/OpenJDK，当前激活配置与二进制回传未证。中央已要求主程／QA并行准备最小EditMode同步和回传通路；Android构建先由C使用现有Mac工具。该安排没有启动旧测试或批准盲目安装Android整套组件。
- 随后中央签发的[云基线同步包](cloud-sync-001.md)已完成：实际回合`01a0f2f7-9625-71ba-a6d6-1adf28a53a5c`返回`BASELINE_SYNCED`，云端detached HEAD精确为125b849且干净，原work@9d416e6引用保留。没有Unity／测试运行；新uGUI候选尚未获得或验证。
- 本机已启动本轮临时空闲防休眠，最长12小时；不更改永久系统设置，任务结束时停止本轮进程。此项不等同于Goal界面已经恢复。

2026-10-01 10:42本地时间，原时限接近结束而工作仍在继续，中央启动第二个12小时临时防休眠进程（可停止的PTY session `51831`，旧session `59037`允许自然到期）。没有更改永久电源设置。任务实际结束或用户暂停时停止仍属于本轮的防休眠进程；不能据此声称Goal界面已恢复。

## 首轮任务与依赖

详见[首轮派发](dispatch-001.md)。四条专业线可以并行准备，产品写入须以主程签发的具体实施包为准。

2026-10-01进度：SYS首个UGUI-01经两轮最小修订，QA-R-DESIGN-001-FIX-02已正式`ACCEPT`；主程据最终设计签发C实施包。审查按当前自洽包及其必要契约冻结，不等待全部后续发行／工具包；后续具体设计使用互斥新文档并行准备，禁止改动正在审的冻结输入。

该次独立R实际回合为`01a0f314-54c6-7ef0-887f-b84f38f690f1`，已completed，final为`msg_0d29bf8d726d8dc0016abd378a5b1c87d08916fc514d80f11e`；审查SYS设计SHA256为`0082a5d6eb17435047609bcf0ca8cbf079e8086052d1f19665329fee40d111d5`。五项设计缺口是TMP必要资源范围、真实TouchPhase.Canceled零提交、业务严重度与本地化故障分离、去除重复Y翻转，以及本地化跨程序集可见性。主程按`QA-R-DESIGN-001-FIX-01`最小修订；完成后由QA送同一R复核，不重开已定产品选择。

FIX-01已交回固定设计`86bf125f6311e4ecdf5655d93914c67581b93078b30b3716b43c3e9acd5aa6f1`，1200行／80442字节。复审回合`01a0f33d-ceb5-75a0-883b-6f883845577f`已completed，final为`msg_0d29bf8d726d8dc0016abd40235bc087d0bfa7a33cb878ff28`，唯一verdict仍为`NEEDS_FIX`。触摸取消、severity与绑定故障分层、左下坐标和跨程序集可见性四项已在设计层关闭；剩余TMP Mobile SDF fallback资源与6个LocalePolicy测试跨到后续持久化包的问题进入`FIX-02`。首包须将这6项改为可实际验证的内存语言选择／传播，持久化留给LOC后包；不为凑数削弱行为检查，目标仍为183+24=207。C尚未放行，尚无新Unity／测试结果。

FIX-02作者回合`01a0f33e-1c96-7452-8b72-b76ef2076c6d`已completed：设计SHA256为`6f7fb2ba94112bd2072602297e16ce77895cf94951edcb59f83a4800402395dd`，1233行／83133字节；补入Mobile fallback及双shader检查，并将6个LocalePolicy槽改为当前会话语言选择／事件传播。作者静态回执不能替代同一R复审，C实施许可仍待正式ACCEPT。

随后同一R在`01a0f354-8c09-72a3-b4b0-358822ea3c01`正式completed，final为`msg_0d29bf8d726d8dc0016abd4620edfc87d098fdfccd157bb546`，唯一verdict为`ACCEPT`，绑定上述`6f7fb2...95dd`设计。主程可据此签发准确C包；该结论只关闭首包设计门，实现、编译、207目标测试、整合全量、APK与设备门仍需本轮证据。中央已独立核对设计字节身份及171键制作稿哈希与正式回执一致。

主程已签发[UGUI-01实施包](engineering-ugui-01-implementation.md)，C实际回合`01a0f35d-e9be-7243-8ae6-9afb5d797e49`已由中央通过wait_threads核为inProgress，开始读取固定合同与共享基础实现。C保持唯一集成和本机Unity责任；主测试继续排期。正式代码／资源交付和验证尚待回传，不能将设计ACCEPT视为代码已通过。

实施包初签SHA256为`5fb9ecfdf5b4afb50d0b077a2e8d4935031b97046cb33908eba95c43d7bcf82c`，306行／16742字节，已获QA执行合同核对。C在产品文件尚未改动时指出两处概述把5个asmdef写成4个；主程仅修文字计数并补签为`b472b82423b0176a38bd0704cff29b4c27ecbe8191ab4609ac202a5516e2fef3`。路径、权限、设计身份、过滤条件与207计数不变，新身份已发C／QA。

C共享静态检查点之后实际分发两个子代理：`/root/ugui_battle`（`01a0f370-23f4-7f33-9df6-1548c16c7bbb`）和`/root/ugui_navigation`（`01a0f370-afae-7373-ae01-4d12ca252159`）。C会话随后收到“先装在 bluestack 上进行试玩”，当前回合内优先处理现有旧019 APK。主程先行安全收口了两子代理；中央要求将试玩视为兼容插入任务，保留同一C接回优先，不默认转移owner。`handoff-paused-01`记录20个WIP文件、白名单外零变化，尚无本轮Unity、meta、资源或场景写入。两叶组只是提前收口，未完成视图迁移；不得标为已接收。精确人类消息ID未由read_thread接口提供，不作推测。

旧APK在BlueStacks启动后退回首页，C正在收集日志；它属于旧包／环境诊断，不能直接判为新uGUI代码故障或当作本轮验收。主程同时派主策`COPY-AMEND-02`审计并补L1动态显示名最小键集；API24与APK结构检查设计已复用ARCH会话在独立文件准备。中央协调与独立可做工作继续，WIP检查点不等于用户再次暂停整体开发。

旧包试玩插入任务已在原C回合正式completed：设备已有base.apk与旧019身份相同，未重装／卸载／清数据；默认Vulkan启动两次崩溃，仅本次以`-force-gles30`启动后8秒存活并进入中文“创建本机资料”。证据根`TestArtifacts/FightMatch/BlueStacksPlay/20260930T180040Z`，receipt SHA256为`3283018bc25b875f43926c875745207855b7837b344d647556b94e8ee60881e9`。只接收`READY_FOR_USER_TRY_WITH_GLES_OVERRIDE`，默认图标冷启问题尚未修复，无新uGUI验收。

主程随后已实际唤回同一C，接续回合为`01a0f383-3a64-7a72-bd3b-a9ae7c4321d7`，未转移源码owner。主策正在交回217键候选（新增46个L1必要显示键，原171行不变）；正式键身份由主程接收后补签，C先推进不依赖新键的授权部分。对APK结构检查的公开API不足，中央已授权主程组织固定Unity版本、旧018/019输入的只读定义表解析样片，记录格式／边界检查与正负对照；不重建旧包，不修改旧证据，不退回原始字符串判据。R仍须审查最终可实施的验证器变更。

后续独立接收与当前依赖：

- API24设计已由R在`01a0f382-98b0-71c2-bf81-c322885a492f`给`ACCEPT`，final为`msg_0d29bf8d726d8dc0016abd51d0b0c887d0aad0c8ef373aacf4`，设计SHA256为`5ed0c1d7eaab4d8e6ecaf7815afc30deacc9e58b05e0d6f6af38ed9aa91402f9`。实际设置、构建断言和两个新输出路径仍待独立实施包，不复用旧018／019作新证据。
- C发现真实Host测试可见性缺口后，FIX-03只增HostAssemblyInfo.cs及meta、Host对两个测试程序集的friend和Core.Tests对Host的引用。R回合`01a0f390-ef07-71b3-b632-9197fc1305e8`已给`ACCEPT`，final为`msg_0d29bf8d726d8dc0016abd5521074487d0990b62413cd8b606`，绑定设计SHA256 `887dfae28158271c2dc0387ed642e14e5d2fc52aa965b3c206da6cf5abac4263`。主程已重签实施范围；不增加公共API、不改变207目标。
- COPY的217键初稿在R指出L1不可达值、英文内部术语和参考差异映射问题后最小修订为216键。R回合`01a0f395-145a-79e3-ab3f-cbe5bdc90291`已给`ACCEPT`，final为`msg_0d29bf8d726d8dc0016abd55c4739487d0a77167e28bcf1f79`；CSV为50990字节／217物理行／216唯一键，SHA256 `e546a7c3e3368b7abcde8a2353f6dc3f1157f4623a68a51e5a799cba9c8e782f`。原171行原字节不变，主程已在实施包明确本次接收取代旧170／171引用，纯输入同步不再另等整体设计审查。正式Luban运行时表仍未交付。
- 两个真实失焦测试的EditMode入口缺口已由FIX-04关闭：生产内部handler和真实Unity回调的一次转发，EditMode验证同一handler，OS事件送达仍由设备门验证。R回合`01a0f39f-2fdd-7812-b05e-eaf900164973`给`ACCEPT`，final为`msg_0d29bf8d726d8dc0016abd58aef16087d090d5273f0d6913c1`，绑定设计SHA256 `1738be494e82fc6b8a2235b70d269e61f051e91fab3bb12ce009340ea6823d0b`。其后仅同步已接收COPY身份，当前系统设计冻结为`4bba3b398516a4c0f4def18746263a5bdf52a7f20837fc04a6fbfb9d5480dd1e`，实施包冻结为`a5dbc48a210bcb9653a8b59e119aac426c1876f9e058303e596455f7ac4a558a`；日常记录和后续设计不再改正在执行的输入。
- APK结构只读样片已由R在`01a0f397-c597-7a31-abde-4383b734c25b`给`ACCEPT`，final为`msg_0d29bf8d726d8dc0016abd57a7fd4487d085fd3c80566d2129`；报告SHA256 `2e14c4473389d7a3bf0f823f03d5c13751e541aa8fcf7c55ae61bf02ff1b362b`。固定2022.3.18f1定义表解析在旧QA／正常APK得到正负对照；只接收可实施路线，C#正式实现、新构建同源绑定及新APK证据仍未完成。
- Luban Mac预检已完成固定SDK／源码恢复和构建；`--help`输出帮助但退出1触发原合同停止。该失败与后续源码语义核查分别保留，复用已构建工具的恢复样片由主程另签，不把帮助退出码泛称为安装／编译失败。工具预检与产品实现分开，SYS与ARCH按互斥范围处理。

截至2026-10-01 03:17（Asia/Shanghai），C已接回两叶组并继续Host／测试集成；正在集中审计全部界面文案，至少地图刷新导航需补键。主策暂冻216键，收到完整清单后一次补齐并交独立R；不通过隐藏控件、硬编码或复用无关键绕过缺项。当前未启动本轮Unity、207测试或新APK，源码WIP不能标接收。Luban后续预检又发现Mac脚本计时／参数记录兼容问题，主程按原失败保留、核可复用证据、最小修正再验证推进，不等待用户处理普通脚本故障。YooAsset设计已独立启动，不再等待Luban预检结束。

03:24补记：C回合383已正式交回并结束，`/private/tmp/FightMatch-UGUI-01-p5hj_ch6/copy-gap-final.json`为285726字节，SHA256 `185c37beeb867aff99ab6c43623e5c54885aa3130facb835a47df015f012ce85`；最终收据`delivery-audit-final.json`为7713字节，SHA256 `f04684151ebdc1efc7e1a673e2fafd9c1fb919b5ce6f6e8bcf1394730c398627`。共56个文案语义组、10个已有键接线组、5个数据／格式合同组及10个产品／范围分类，不能机械转成新增键或用户待答题。48份源码快照仍属WIP；183旧测试声明和24新槽静态保留，207未运行。主程获指令按既定玩法和范围直接组织主策／SYS集中处理，再恢复同一C；普通提示／格式选择不逐项升级人工决策。RES-01设计已由主程专业接收，SHA256 `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`（859行／62080字节），已送独立R；Windows待验不阻塞Mac／Android独立部分，当前没有安装YooAsset。

后续Luban预检005已获主程证据审阅`ACCEPT`：证据根`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/`，run SHA256 `ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232`，commands SHA256 `f098f03407019eae78340c8240a59ea5e8f20fe0de1a442f8101d597397a8688`，161项manifest SHA256 `ea361c90a7c6ee4747ea9548851edd4aec7c7c3b59c3b064e810aab5060e6c64`。两次正样片一致、三类负例正确拒绝，harness与清理通过；001～004原失败保留。只关闭Mac工具预检，生产CSV生成／Unity接入仍未完成。

COPY-AMEND-03随后获主程专业`ACCEPT`并送独立R：planning SHA256 `11d4c1891e0a21cabc1459695aac9473491e46e63de6008ce98c625cc5b30d9d`（345行／44197字节），CSV SHA256 `0228ca7ceadf70af140af7b03de498630c0dc5e6ebd30704457af1aa457bf765`（263唯一键／264物理行／64325字节）。原216键前缀原字节保留；81项处置为APPEND39、REUSE6、BIND4、DATA5、HIDE6、DEFER21、BLOCKED0，实际追加47键。普通UI处置已由专业负责人决定，不新增用户待答卡。QA串行安排RES后审COPY，独立接收前C依赖部分继续等待；LOC设计可并行同步005事实但保持PENDING_COPY_R。

RES-01设计现已获独立R `ACCEPT`：回合`01a0f3c5-e7d1-7b23-a264-28b37a5b1a4e`，final `msg_0d29bf8d726d8dc0016abd64c6c08087d0a679d2096244e799`，绑定上述`2e12b5…40d8`版本。只关闭设计与3.0.6固定提交探针路线；UPM安装、导入编译、Android、Windows、CDN、设备与发布信任均未运行。QA已在下一独立回合审COPY，主程同时准备五个数据／格式合同和已有键接线补充，避免文案通过后再串行等待技术约定。

COPY-AMEND-03初审回合`01a0f3d2-340b-7c03-a494-ad14288b891b`给`NEEDS_FIX`（final `msg_0d29bf8d726d8dc0016abd669b7cb487d0baa6097a5dec2979`），仅要求补清非负整数限制、候选“已观察”语义和偏好入口可达备注。FIX-01随后由同一R在`01a0f3db-8a86-7e63-b77f-2f121cedbd0b`给正式`ACCEPT`，final `msg_0d29bf8d726d8dc0016abd67d90e4887d0b247cc4f8c806593`。接收planning为347行／44944字节／SHA256 `8cdf0e39dc9364758843eefedf35146433ee5b650e4a42c24e152306eaaccff6`，CSV为263键／264物理行／64708字节／SHA256 `d2cff0784221357394fc23e3354fb7de6e48d2455630a694de665105e4bdf36c`；旧216键原字节不变。源码尚未据此补正，不能称为游戏内文案已通过。

接线附录初稿指出冷恢复原请求种类与精确手势反馈两处只读接口缺口。中央已要求主程把必要API、唯一事实来源、生命周期和验证冻结为一个完整补充后统一送R，不先审不可执行的BLOCKED合同再等待无固定定义的新包。原UGUI资源生成权限仍有效，补充不扩张资源范围；目标测试拟新增15项，正式计数须随补充合同获QA/R核实后采用。BlueStacks默认Vulkan崩溃也已交主程提前做只读平台方案准备，不重建旧APK或把临时启动参数当作修复。

云端候选传递核对结论为`TRANSFER_PATH_NOT_AVAILABLE`：当前工具未证实本机未提交文件到durable的可靠通路。遵循独立接收后推送Git的边界，新增候选必要验证先由C在本机串行执行，继续遵守预算。云端只完成准备；接收并推送后也不自动重复相同套件，只有明确的新验证问题才另签云运行。

1. 主程接续架构与系统技术设计，形成uGUI、字体、本地化及场景／验证迁移包；主策和主美同时给体验与表现输入。
2. 主测试核原证据、云端和设备，安排R独立设计审查；设计接受后主程签C实施，C先完成源码及静态检查。
3. 固定候选后按QA计划完成必要编译／回归／构建与设备场景。云端可承担的任务优先移出本机；等待时推进独立工作。
4. R核实际代码和证据，NEEDS_FIX进入最小修正；ACCEPT后接收相应范围并推进下一必要包。
5. 本轮交付必须更新试玩说明、实际证据、已知缺口和Git版本。无真实设备证据的项不得写成通过。

## 回传与写入归属

- 本README、START_HERE、PROJECT_CONTEXT、旧共享session-plan／integration-review／system-task-packets由中央唯一更新。
- `docs/demo/PLAY_FIRST_DEMO.md`的本轮状态和最终试玩索引由中央整合；实施者回传实际包、命令和证据，避免多个角色同时改指南。新候选通过前保留旧019说明并明确历史边界。
- 主程：`engineering.md`及其明确签发的技术子包；主策：`planning.md`；主美：`art.md`；主测试：`testing.md`，均位于本目录。
- 各负责人可在本目录新建自己前缀的子包／交付文件，创建前在自己的主文档列出准确路径和责任人；不得代写其他负责人的结果。
- ART-03完整布局设计专业核对由中央在此登记，唯一交付负责人仍为主美；仅允许新建`art-ugui-layout-design-receipt-001.md`，核ART-02五项与SYS新布局合同是否闭合。原`art.md`、ART-02报告及其余固定输入保持只读，不为登记改变它们的字节身份。本次不产生资源或运行Unity，也不替代R技术审查及实际画面验收。
- ART-04由同一主美限定复核LAYOUT修订对原N01–N04的关闭情况，唯一新增交付`art-ugui-layout-design-receipt-fix-001.md`。冻结ART-03、原布局设计、中央裁定及SYS修订；不扩产画风、不运行Unity、不代替技术R。
- ART-05由同一主美仅核最终9c3d6d修订对N04T文字容量的关闭，唯一新增简短回执`art-ugui-layout-design-receipt-fix-002.md`。保留ART-04的先前NEEDS_FIX，不重复原N01–N03或R的整套技术检查，实际画面留给布局实现后的专业验收。
- C可在主程明确登记的互斥子白名单下协调必要实施子会话，前提是冻结接口不变、每个文件只有一个写入者；C保持唯一集成及本机Unity责任，子执行者无Unity／Git权限。不得将并行授权解释为扩大首包范围。
- 向中央回传任务／回合、实际状态、产物路径、验收证据、受影响依赖和下一步。必要跨线输入可在已登记角色间直接发送，中央接摘要。
- 为缩短UGUI实施等待，中央已按既有团队授权允许QA必要时新建一个`R-TOOLS`（gpt-6-astra/max）只读审查LOC／工具类独立候选，原R优先UGUI／产品线。实际身份另登记，不能先称已创建。QA仍唯一调度；同一R不并发接包，同一候选不重复派两个R，NEEDS_FIX仍回其原审查者，禁止换人寻求通过。已开始的审查不打断。此安排不增加本机Unity执行者，也不放宽接收条件。

R-TOOLS现已由QA实际创建：thread `01a0f3e3-9663-7be2-84a8-ed23be32f0b9`／local；就绪回合`01a0f3e3-9764-7301-9711-4be33f3da87c`已completed，final `msg_0543862903425801016abd69b460bc87d0990bb137aa982e46`。它没有审查或接受LOC当前候选；`QA-R-LOC-TOOL-01-DESIGN-R3`在它创建前已由原R开始，仍由原R独占。仅就绪注册，没有运行Unity／Luban或写产品／Git。

随后UGUI-COPY-BIND完整合同由主程专业接收，原R仍在审LOC。中央明确该新候选可由空闲R-TOOLS只读审查两处API、绑定与验证映射，扩展其本次准确职责；原R优先UGUI的偏好不构成全局串行门。只有尚未实际开始的候选可以调整归属；已开始审查不打断，不重复派。该附录首次独立审查者确定后，其全部NEEDS_FIX修订固定回同一人，不能换人寻求ACCEPT。

本次并行审查已实际完成，各有独立`NEEDS_FIX`，主程继续最小修订：

- LOC-r3由原R回合`01a0f3e2-a494-7761-841c-43fa0da0b2cc`审查，final `msg_0d29bf8d726d8dc0016abd6bf24d8487d0a2c5926eb81df19f`，绑定`22f1466a86c1e256a465b8d91b7fd776f1a402f75ae1166854411ad3ad1543f5`（582行／36745字节）。四项为跨平台raw身份与发布manifest矛盾、偏好替换后复读失败的未知结果、完整依赖许可证证据门、跨平台CLI与005工具身份。005有效证据不重跑；同包修订仍回原R。
- UGUI-COPY-BIND由R-TOOLS回合`01a0f3e8-e601-76b3-9080-dbe7675fa9bd`审查，final `msg_0543862903425801016abd6c44ee2087d0b0196d478dcb4571`，绑定`f0c5d246c0af3570c6ad451d99313540b1f2e0068160ad83db5e8b02759e4e67`（244行／41329字节）。三项为共享LocalizationContractTests白名单／旧216键锁、真实Host失焦取消的原因传递和去重、提示在连续Changed／周期刷新／语言切换／解绑中的保留边界。两只读API方向及其余合同静态成立，222仍是待执行目标；同包修订只回R-TOOLS。

中央已要求当前UGUI采用已接收263键的不可变证据快照，后续LOC未知保存结果如需补文案另作限定增补；快照不得成为第二个人工制作权威。这样未来制作稿更新不会再次打断正在执行的固定UI包。所有C源码、Unity、资源及APK门仍待实际补正与执行。

最新接收：

- COPY-AMEND-04由原R在`01a0f3f2-c361-7311-9bad-db9b91046023`给`ACCEPT`，final `msg_0d29bf8d726d8dc0016abd6de210dc87d0a7a904c914de06f9`。planning SHA `35c51adca0eeb34155ad6e392c347e652ca24e8979f926074a229b9dbedb68f5`（372／47305），CSV SHA `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`（265物理行／65358字节／264键），仅新增语言偏好保存UNKNOWN提示和准确触发边界；UGUI263快照不变。
- UGUI-COPY-BIND FIX-01曾由R-TOOLS在`01a0f3fa-9bbe-75c3-a445-f863446af59b`给`NEEDS_FIX`（final `msg_0543862903425801016abd70b3ac8087d08fe58ae537a83642`），P1/P3关闭，仅P2保留。FIX-02在`01a0f406-3c86-71a3-bd46-291a87ead2e4`正式`ACCEPT`，final `msg_0543862903425801016abd7301b80c87d08844b314c81630bf`，绑定SHA `a26b50a40828d4598423ad49a694b4361eca33391dbcfcabe1f7ffceb82528ac`（264行／54986字节）。非借用视图Close(cause)须在Controller.Dispose默认取消前先以真实cause取消active gesture，这是已接受合同的实施／代码审查重点，不另开设计FIX-03。中央已要求主程立即签包恢复C，不等待LOC。
- LOC FIX-01曾由原R在`01a0f3fb-a0fb-76a0-9a06-4719d8da83dc`给`NEEDS_FIX`（final `msg_0d29bf8d726d8dc0016abd70ecf84887d0af99b775be765c1b`），F3关闭。FIX-02在`01a0f408-5374-7660-80d8-3fa332c632aa`正式`ACCEPT`，final `msg_0d29bf8d726d8dc0016abd73567b7c87d0824de3bb6f79ebff`，绑定SHA `43f98e353b20eb583df8154fe604dd73f32b281378aefade22382916975fe1fc`（743行／50126字节）。制作稿／正式源双身份、raw集合、偏好写入阶段与argv已闭合；实际完整许可证仍在A核验，Windows及运行时／设备门未关闭。

04:43接续已由中央实际核实：同一C的新回合`01a0f40c-69d4-7f12-90ee-50e6e9e198c3`开始读取重签包和现有WIP；主程同时准备`engineering-localization-luban-impl-a.md`，将纯Config／Tools／Generated工作与C互斥分配，复用005工具且不运行Unity。只有C实际回传的源码、资源、编译和222结果才关闭对应执行门。

05:03补记：中央核实当前UGUI实施包SHA为`ccb7ed615cf9d688f61a19620ff4619ea2c43be75c44f2820a916622552ea623`，包含263键只读快照、已接受接线FIX-02与222目标；C回合40c继续负责源码和后续串行Unity。LOC-A已实际写入获准的Config／Tools路径，首个production pass在04:56返回1，run终态`FMLOC013`，只产生未接收raw，没有发布Generated。stdout指向Luban severity集合分隔语法；中央已要求主程查固定源码并直接签最小技术纠错，保持四值语义、264条记录和输出合同，失败证据原样留存，实际修复与新正负证据交原R。普通工具语法勘误不再等待整体设计前置审查，也不重新运行005或下载／restore／publish。

ANDROID-GFX-01独立审查final为`msg_06adc457b2dc221e016abd767977d487d0a85a98fc00b984be`，绑定设计SHA `09b54b1f448ba875372262b5a0e2c699fed3eb4a70b479b77a7403a5e7bd5134`（307行／23981字节），唯一`NEEDS_FIX`是每个QA／normal成功及可控失败路径恢复保存后，还须关闭原Unity并用独立2022.3.18f1进程复读Auto、有序API、PlatformMatches及原始bytes／SHA。其余设计合同通过；主程作同文件FIX-01并回同一R-ANDROID。此额外审查会话的已开始任务继续，后续优先复用三个现有R，不自动按主题扩张；新建更多前须向中央报告实际并发冲突。此设计修订不阻塞UGUI／LOC，也不代表任何新构建或设备验收已运行。

随后ANDROID-GFX FIX-01A由同一R在`01a0f421-6151-76f2-93ac-03b6419b42d8`正式`ACCEPT`，final `msg_06adc457b2dc221e016abd799d7a1887d0bf9fe77585f1498a`；设计SHA `4e00b6404658f7925bebe72530358bf32414fa22c2186a51d9e03003f88578bd`（351行／34601字节）。三个独立证据目录、原进程finally恢复、独立复开及nonce／PID／SHA回链、private无构建失败探针合同已闭合。未来实施须读取post-uGUI／post-API24实际两文件冻结身份，不把旧观察SHA当当前源码。此接收不代表构建入口、三组实际恢复、新APK、BlueStacks或真机已验证。

LOC-A最终执行回合已completed，FIX-02在05:20:57～05:21:01运行并得到`PASS_LOC_IMPL_A`：10条命令退出0，12/12自测、三负例、两次raw／canonical／manifest／semantic一致、132文件许可证清点和diff-check通过。证据根为`TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/`，run SHA `43c4f7d9cf78194cd97b6a2b277c40a4bde9be38ad6f359e133dab17c25e3110`；正式源SHA `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030`，生成文本SHA `f48264f9ce94cd3bb2ef8d1b6a0be7b241916144b6d5764d11633bb6d6e456bc`，manifest SHA `18437127bc2e2d7371f97da0990de933f0193fe202539593ef19338d47123667`。独立R接收尚未完成，不将生成成功等同于游戏内本地化可用。

首次FMLOC013与FIX-01的pre-write身份停止均保留。后者根因为任务包将旧inputs.json的64位SHA漏抄3位，不是已证实的并发文件修改；完整原SHA为`07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2`。相关关联常量和证据字段由主程统一补签纠正。未来Git接收时须显式纳入`Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj`：它是本包手写工具项目，已在8路径交付中，但被仓库`*.csproj`规则忽略；不得因此漏交或批量加入Unity生成的csproj。当前仍无本轮Git提交。

UGUI已实际进入Unity：CLI启动尝试未创建Editor，保留启动器失败后由C按仓库规则调用Intel 2022.3.18f1。首次导入的两处错误只影响`UguiSceneCompositionTests.cs`；最小修正后源码candidate为`cb3922b36246dc2230d56f53a62a29e86a30770c237df86aacce608f2da388c5`，manifest为`8f61a93db270e5de2a46b39fbb50b2544417e55633951d5c0fdf59e99871b601`，53文件中52未变。随后资源生成／保存约25秒完成。独立复开发现3个TMP输入组件的引擎托管文本追加单个末尾零宽字符；主程限定适配这3个组件，其余116文本仍严格比较占位符。新增断言又被夹具已有`Is(...)`遮蔽，C依停止条件保留失败，主程只签两行完整限定名纠正；不能把资源生成成功写成复开／编译／222成功。

并行审查：原R在`01a0f437-52d6-7c30-a424-86b561f3c0f8`核LOC-A实际8文件和74项证据；R-TOOLS在`01a0f43a-279f-7991-8364-876fcf94273b`核UGUI固定源码，最终资源／Q2／Q3／222齐前只作预审。UGUI编译导致的源码变化只回同一R最小diff和新身份，已读未变部分不重复审。LOC-A预审已指出YamlDotNet缺本地许可声明、NeoLua仅有远程license URL却标complete，以及compiler／driver进程证据采集缺口；最终仍等原R completed裁决。主程提前只读核对应固定版本的一手许可材料，不改正在审的候选、不重跑Luban或005。

LOC-A原R随后在上述回合正式completed／`NEEDS_FIX`，final `msg_0d29bf8d726d8dc0016abd8257794887d0871f3e7f431baaa0`。P1为把nuspec存在当成许可完整，错误放行上述两个依赖；P2为compiler退出码未实际捕获而事后写0、driver stdout空但真实输出含PASS，不能证明完整进程记录。264数据、12自测、三负例、双次raw／产物一致和74证据／8生产路径均核对成立。主程只签FIX-03修许可判定及真实外层记录，补缺失／URL-only／SHA mismatch负门；使用新证据根并保留旧三根、005和未接收Generated。当前A未接收，依赖它的B运行时实施仍不放行。

UGUI后续源码candidate为`b9f2f64bf2aaeb930f83afdd6c1290a5895386cd870b1f7f4c07f7d0f4591669`。Q2独立复开与Q3编译已由C报告通过；首次Q4准确发现冻结222项，146通过／76失败／0跳过，失败集中在共享输入／渲染装配，C先按诊断流程归并根因，不削断言或改过滤器求绿。R-TOOLS上述预审回合已正式completed，final `msg_0543862903425801016abd8452edd087d080b5ed743aabc23f`，结论`PRE-REVIEW CLEAN`仅适用于53/53固定源码，不是完整候选ACCEPT。

测试期间仅字体身份发生变化：Q2历史font SHA为`d7d632b8d97db7a627d3883cae3058242f4b012c3082b36a916edce784800d2d`；05:49:05后的`Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset`为8546563 bytes／SHA `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb`，场景与Prefab仍匹配Q2。C／主程须诊断成因并冻结最终资源，不把旧Q2哈希当当前资产，也不先归为未知并发污染。Q3/Q4完整正式回执及修正后结果仍待QA接回，同一R-TOOLS才可终审。

字体后续因果核查定位到本地化绑定补字形、TMP标脏及Q4退出SaveAssets／重导入。Q2未留原字体字节，无法作历史逐字节diff；如实保留该缺口，以当前post-Q4字体作显式新起点，后续先存原字节再验稳定。C的9项既有失败定向复现得到8失败／1通过，前后53源码和37资源字节均未变；H05字体子资源检查本次通过提示此前atlas扩容状态依赖，不作为稳定通过替代。五组疑点是EditMode输入模块初始化、CanvasRenderer装配、同步销毁后的回调访问、atlas容量槽／负例字体结构、导航结果与查询按钮重复ID。主程据实际XML／日志签最小修复，不改222过滤器或削断言。

许可证研究`research-loc-a-license-sources.md`（SHA `5fa2b799a17484df59cf1ad1216edf07a007dd7ea49f40794849c35e97e00550`）确认NeoLua 1.3.14有可绑定tag／commit的Apache-2.0来源；YamlDotNet.NetCore 1.0.0的候选源仍不能充分绑定所用nupkg。因此FIX-03包`374234b8ed0f015af13eeff3771c15343a2d497e7fc54909152dad207322a812`只接收准备态，不获执行授权。中央已明确：用户选择Luban，未指定该旧间接依赖或5.1.0 pin；团队可先评估可追溯官方版本／构建，必要时仅更换旧YAML依赖，保持表格→Luban→既有发布器与输出／游戏语义。准确草案须回同一R后另签执行，不把修补构建冒充官方pin、不改旧证据、不付费／发第三方消息／更换引擎框架。LOC执行者等待主程新技术包，不索要用户继续许可；uGUI与其它独立可做工作持续进行。

后续原R对Luban替代方案的回合`01a0f467-89a8-75b1-808c-de1d25aaa3b8`已completed，final `msg_0d29bf8d726d8dc0016abd8d6b85d487d094d577ce867f83c9`为`NEEDS_FIX`。四项修订包括六个保留依赖的可追溯许可材料、材料／构建实证／适配三阶段解除身份循环、统一构建与官方测试的离线依赖和执行预算，以及区分被测失败与验证程序成功退出。主程交回[ARCH FIX-01](engineering-localization-license-alternative-architecture.md)（651行／46233字节，SHA `35afce4785af87f5cf0e562117a0ab2cc1dbe7429af06c8dbe17c4a82120d1ff`）和[SYS FIX-01](engineering-localization-license-alternative-system-design.md)（881行／59206字节，SHA `18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`），中央已核当前字节身份一致，QA送回同一R。路线只替换官方5.1.0源码中的两项YAML引用，保留原能力并标注项目修补runner；此时未获设计ACCEPT，未开始材料取得、构建或LOC-A适配。

UGUI七文件修正后的原生Prefab保存先在复合前置检查停止；失败根、源码及资源原样保留。随后唯一只读Unity诊断确认：磁盘BoardFrame没有CanvasRenderer，但`LoadPrefabContents`已自动补入恰好一个；脚本GUID、节点与挂载关系全部正确。原始退出瞬间的尾进程记录保留，单独的后续只读检查确认其自然退出，没有发信号或重跑掩盖原记录。主程签发[Q4修正FIX-02](engineering-ugui-01-q4-correction-fix-02.md)（164行／10795字节，SHA `7ef805179f0d4f7d4b7585857679f7124a7041b1f7a3b50417fc8bfbbb375f12`）：仅移除临时探针并复用原生加载所得唯一组件，禁止再次添加。C已冻结新源码并开始保存验证；后续资源复开、编译、9项和222项尚待实际结果，不能沿用旧候选的通过记录。

FIX-02实际原生保存成功：Prefab的serialized objects从1434到1435，仅新增classID 222和对应BoardFrame引用，GUID、旧对象及其余36资源不变。包装器45秒尾进程观察未静止而停止，后续独立只读补证确认全部13个记录PID自然退出。主程核实后签发从Q2开始的验证续包；原保存不重跑、源码不再改、原包装器失败不覆盖。每步使用其剩余总预算观察自然退出，保留即时与最终两种状态。

Luban方案FIX-01复审回合`01a0f484-8745-7b62-8057-3d1c49210595`由中央核为completed，QA交回final `msg_0d29bf8d726d8dc0016abd94a6ec0087d0934d80e2063745ad`，唯一`NEEDS_FIX`。剩余四项只在ARCH：M不依赖未来B产物、inactive final cache复制在B独立接收前、并发计数统一为顶层进程树与固定child DAG、失败候选全部保留且新修订另建根。SYS和研究文件冻结，主程已派回ARCH原所有者作最小修订；M/B/L均尚未放行。

随后ARCH-FIX-03由同一R在回合`01a0f493-0083-7063-88e7-e9c17d65ae55`正式completed／`ACCEPT（仅设计）`，final `msg_0d29bf8d726d8dc0016abd96c5370487d0b6d99135bc6c7c1d`。ARCH为681行／49110字节／SHA `3dba4f92c17cc50fc158963857a69f70d308a8dab36dcbdf3a9fbbf821a06cb7`，SYS保持`18be0271c405bd33444fa5a5b4aa248114eef8a8e4442f5b40fdf840dfcdeb37`；中央已实读回合与文件核对。四项约束矛盾关闭，主程可签M执行包，实际材料／runner／LOC-A实现均仍待独立证据。M准备复用原LOC-A会话，实际回合`01a0f490-97be-7831-b855-e5a11969b97c`已在执行只读盘点。

UGUI验证续包的Q2与Q3已通过，53源码和37资源稳定。9项定向回归为6通过／3失败：按钮重建、本地化负例、导航行标识及资源检查已通过，剩余B16B03、OwnerEnded真实Raycast与Host H01都涉及棋盘命中／捕获。C按停止规则没有启动222；主程按共同根因签有限文件、有限预算的诊断修复包，保留业务与真实输入断言，普通范围内修正不逐项等待中央批准。

M材料包`engineering-loc-license-alt-m-materials.md`（SHA `0ce7c44844494e8b63c5e6c149fa357932c11c9a53457275a24a16ac4ef48d13`）由主程激活，实际执行回合`01a0f4a3-c824-7e70-af0a-66c501813e7a`已completed／`BLOCKED_PACKAGE_SIGNATURE`。42次受控请求取得23259536字节，nupkg及签名entry身份匹配；固定Mac SDK的verify返回NU3003／CSSM／exit1。result SHA为`a57094354718377a4b0204793fc2e8c3d14a4316812dd0b74ee834076f8fff5e`。原缓存／证据／8生产路径／受保护区前后相同，material/B/L根未建，未构建或运行Luban／Unity。不能把此失败写成包损坏，也不能写成验签通过。

[微软官方说明](https://learn.microsoft.com/en-us/dotnet/core/tools/nuget-signed-package-verification)明确当前不支持macOS NuGet签名验证。既有durable会话只读盘点回合`01a0f4ac-72aa-7081-af3c-3b37f4f0dbc0`为NOT_READY：Debian13 x86_64只有Unity内嵌.NET6运行时，无SDK/NuGet。中央已授权主程／QA签最小云验签包，专用临时目录安装核过官方发布身份的Linux x64 SDK并验证同一官方包；不更改系统信任、全局工具或Unity，不转移未接收产品源码，实际运行结果仍待回传。

UGUI仅去掉`-nographics`的对照在Apple M4 Metal仍0/3，否定其为共同根因。随后DIAG-04的临时探针自身因uGUI IndexedSet不支持枚举而异常，未取得有效快照；原240秒尾进程未静止记录保留。进程随后自然退出，C恢复4个测试文件，主程核53源码／37资源均回到探针前身份。下一轮DIAG-05只修正探针的集合读取方式，产品根因与222门仍未关闭。

DIAG-05取得有效快照但尚未完整定位：B16 depth10、OwnerEnded -1、H01 depth3，四源恢复；原360秒尾进程超时记录保留。DIAG-07在C回合`01a0f4ca-1d98-7da1-be38-3a94e30e57f1`完成`DIAGNOSED_SPLIT_ROOTS`：B16/H01直接raycast能命中棋盘，但全局manager为空；OwnerEnded另缺有效绘制深度。final-receipt SHA `0542f347c0a0108b63a2545bfd5d9cc456630a15ff85d5b7d6951a6f9ef2d0d6`（66418字节）。原三断言仍失败，四个临时文件已恢复。专属Roslyn缓存服务通过一次精确pipe正常关闭，无信号／全局关闭；本轮Unity34.200秒，所有进程39.039秒内退出。

FIX-08随后只修三个测试文件，A02将exact3／9／222统一为Metal图形验证。新源码candidate为`c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280`。C回合`01a0f4d5-d646-71d2-9ba0-a5004a7af1b6`正式completed：exact3为3/3／exit0，但因动态字体新增“现／提／交／H”四个字符及字形（346→350）而停在`FAILED_EXACT3_RESOURCE_PROTECTION`；其余36资源不变，前后字体原文件均已保存。当前字体8548693字节／SHA `2a186863bc12042842f77e257fc964af205a65e5de406cbdf81b22bcd007805b`，中央已核当前身份；final receipt为31502字节／SHA `aaee3cb4b6ca857734c79fc36bb089859e6a3a821c56256467a942a44f105380`。主程签资源续行前不启动9/222。R-TOOLS预审实际回合`01a0f4df-6a7f-75f0-b779-2dbd7257d94d`；最终必须覆盖自b9上次预审以来全部未审增量，不能把FIX-08三文件当成整个历史差异。

云验签METADATA-002在代理CONNECT返回403；VERIFY-003因前序三个路径误加network目录而零网络停止，原证据保留。004以本机固定官方metadata为authority，在专用目录取得并验证SDK 8.0.425 Linux x64；A01/A02只约束后续命令并将实际root预算调整到832MiB，不重下载、不倒写已执行的--info。初始回合`01a0f4c9-2606-74be-b8a3-59dcf1cc81c3`与续行`01a0f4d2-cf3a-718f-931a-b92ec2c72f3b`均登记，最终为`READY_FOR_INDEPENDENT_SIGNATURE_R`：同一YamlDotNet包和签名entry哈希匹配，verify真实exit0、0errors、4条NU3018/NU3028吊销服务不可达／状态未知警告。result SHA `d03fad30914fe6e01f89cd77596fe3716022099d9e26f10fc319ac7391d1ef29`；4801项manifest SHA `43b7843df644582dff0bd467ed2c0d6d0a7365653adfac23797991980cf72bf2`。原LOC R回合`01a0f4dd-0664-7b92-a5cf-2351ee317c5e`在审；尚未关闭原M签名门或接收M/B/L。

C主动发送回执曾被自动审批以缺少直接用户授权拒绝。中央已使用安全返回路径：C在本会话输出final，主程／QA／中央通过wait_threads或read_thread主动接回并读取共用证据；没有要求用户重新批准，也没有把通信问题当成源码或验证失败。

FIX-08预审回合`01a0f4df-6a7f-75f0-b779-2dbd7257d94d`已completed，final `msg_0543862903425801016abdad0d94d087d0bf8b221872c7fbe5`为`PRE-REVIEW CLEAN（仅三文件增量）`。机器对比确认b9→0ff有7个源文件差异，FIX-08有2个重叠，故b9→c428共8个源文件待完整覆盖；Prefab及最终字体也待终审。该R的发送亦被通信审核拒绝，中央已拉回final并让QA沿同一返回路径接收，无需用户回应其重复授权卡。主程签发FIX-09（152行／9568字节／SHA `7aa0d33d6b8c067ddd558b48c1d91a01a1e52d96aa9681400a5116a6712e854c`），C核纯additive增长后继续Metal 9→222，冻结最终37资源，再做一次原生复开；不重跑已绿exact3。

原LOC R对Linux签名门的回合`01a0f4dd-0664-7b92-a5cf-2351ee317c5e`已completed／限定`ACCEPT`，final `msg_0d29bf8d726d8dc0016abdab8dd80c87d0907a376faa94426b`。4条警告只说明吊销状态未知；不声称在线吊销验证成功，不覆盖旧Mac失败，不接收完整M。主程据此签发M02（205行／11834字节／SHA `402ab00f2f1fa67e189e0a49fc42a97bfda8eec4c59d7e50777c0bd211d7b5f0`），在新sibling根只读复核既有材料，继续原M未执行的闭包、lock、映射与封存门，再交原R整体审查；不得直接进入B/L。


FIX-09正式回执为`BLOCKED_EXACT222_TEST_FAILURE`，C final `msg_0d179092dafe07fa016abdb072e1e087d0a251998fa1c48eea`；receipt 69207字节／SHA `730d018c8661ea1fb4762c9f0617643f911474bcb72f34d4cc5d89d650c59331`，28项inventory SHA `4a95f17c89ddb99c469bde0e4505fa66fe8bd84f5749039ccd45207f3fc923ec`。中央实读XML及文件，核9/9、194/222、0跳过；当前字体8558317字节／SHA `f138d8c36bde60bbd8510b600c759eef9e3d0c78082e2cacb2a4818b96046eb9`，369字符／字形、4图集，最终复开未执行。R-TOOLS补审回合`01a0f4f1-08da-7453-8559-7ce15884cb80`已completed／PRE-REVIEW CLEAN（final `msg_0543862903425801016abdaf88e66087d08e67f12e247a9258`），累计覆盖c428相对b9的全部8个源码差异及Prefab，保留45个未变文件结论。主程已签FIX-10（208行／13370字节／SHA `7b010e32b6a2216a3003f9f819a0a919f25253af2965b29c18b3fcf2960dac0c`），只修27项对应的真实帧初始化和1项已销毁文本清理，保留业务／交互断言；C静态门通过后开始定向28，仍无最终接收。

M02正式completed／`BLOCKED_PROCESS_MODEL`，final `msg_0a0722abe4bcea06016abdb1e85f7c87d081f07762daecb0d5`；result 3647字节／SHA `06bf3b7d330ed09e6836710c8797b2b6b198a00d752762e86562ec3d72367faa`，中央已实核。完整日志的阻断是90项MSBuild节点named-pipe权限错误，exit1、runner lock未生成；早期CSSM单行摘要不足以独自确定根因。收据实际存在于runner-generate路径，不是回执丢失。旧根、8个生产路径及保护状态一致，M02网络为0，未建material。M03（269行／15522字节／SHA `11abcdf14316ea7cb6755d7d6660e9b58baa2fe6c9e4c117350144d279bf88be`）改用显式单节点MSBuild Restore，复用M01固定材料、排除M02失败scratch，不更改SDK／信任／签名门。005本身没有restore/msbuild调用，不能当作相同命令已成功的证据。

中央另外下发ART-02并行静态布局预检：输入为当前Prefab（1060568字节／SHA `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b`）、其meta、固定Scene、已接收art.md及灰盒登记、固定263键快照；主美已核输入身份。唯一报告路径`art-ugui-prefab-layout-audit-001.md`，检查Canvas／安全区／棋盘／HUD／弹窗／文字容器的可证静态问题，并列后续新APK画面节点。不读C正在修改的源码，不运行Unity／改资源／生成画风，不阻塞FIX-10和M03，也不把静态结论冒充设备视觉或触摸通过。由中央wait/read接回。
