# FM-DEMO-027 独立技术审查

VERDICT: ACCEPT

Scope: PASS。规范与规格两轴通过；在冻结包明确允许的未运行边界之外，未发现需要修正的技术问题。接收范围为已提交事实播放及中断恢复，不包含正式内容发布、PlayerSave 或完整 Demo 体验。

## 身份与完成门

- 授权：system-task-packets.md r128 §257–259，§260 登记本次实际回合；§256–259 冻结片段 SHA256 aa1219c85e2ebc655dedc7fdf6f4479d74b92f2318cf58a0d72c1a9a2ede6c21，组末复核相同。
- C 任务 01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次 turn 01a0ce47-0fb4-7c43-9717-1085de8fac27，2026-09-23T12:39:18.771Z 开始。
- 准确原生记录：physical line 11021 为 2026-09-23T14:24:49.870Z 的 phase=final_answer、delivery=null 正式 final；line 11025 于 14:24:50.427Z 记录同一 turn 的 task_complete。wait_threads 随后亦返回 completed、error=null。final 明确两包各 COMPLETED、四文件绝对路径/SHA及未运行边界；R 已核实四 SHA 与实物一致。正式裁决在此完成门之后。
- R 任务 01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次 turn 01a0ce47-ca8d-7fe1-87a5-be86ac97c3ea。C/R 原生 turn_context 均为本机工程、gpt-6-astra / max；没有用旧 B16 完成替代本次完成。
- 作者 demo-027-delivery.md：9990字节、68行，SHA256 09e91ee9c8a2cf1e9b62914d2e476fc8d2890b03213e66fe6a881f696f8dde8a；demo-027-scope.json：1446094字节，SHA256 de8a6f690d5c28e973951db9dcd9feb651550e23bc7fd696e64ac35436ac0b65。两份均在正式final前完成，组末哈希一致。
- R 自行进行规范/规格两轴审查，只写本报告及 demo-025-p1-code-review.md；未运行 Unity/项目测试/额外业务探针，未改源码、调用代理/外部模型、写 Git 或发送协调通知。§259 覆盖 code-review 技能的另开代理流程。

## 规范轴与阶段继承

027 入口继承 P1 完整 656 项，canonical=9c34b62661ae2fb8b8f5bf12ae4a51ea675c74b860f443a8957523e061463280。新增六份 C# 及其自然 meta 共 12 项；唯一旧修改为 CandidateBoardElement.cs，13 增/4 删，低于 90 行预算。构造、指针、选择、命中几何、提交和 Capture 处理原字节不变，仅增加可清除的表现覆盖入口及 Draw 读取。旧 meta 保持。

最终 668 实施项、361 唯一 GUID；其余 P1 655 项（含 Content 和全部 P1 测试）保持。全组相对 B16 637 项仅 BoardElement、Core.Tests.asmdef 两项授权覆盖，另 635 项原字节保持、新 31 项。没有 asmdef/配置/依赖/Core/Application/Input/旧测试/Scene/UXML/USS/美术修改。

新增生产 348 行（上限1800）、测试 498 行（上限2200），E17 helper 共159行。生产仅可丢弃显示值，不创建 BattleSnapshot，不调用 Evaluate/提交/奖励/随机采样，不写保存句柄。新宿主组合原 InputView/Controller；没有偷偷让旧页面自动取得新能力。

最终实施 canonical=243121465a9724a87e3e93f570273a55a6f98e08085591b90974c5091389fab4；全部 Assets 681 项 canonical=c9664adb273412ad721ea27dbd597089b735a8a556a9c22bfa0706d708cb5172。R 按实际当前字节、阶段副本及 B16/P1 基线重算，不只复述作者计数。

## 规格轴逐项核对

| 条目 | 独立代码/运行证据 |
| --- | --- |
| P27-01 事实/时间线 | Controller 只从真实输入办理结果取得 PlayOriginal，检查 Ready、已核当前头及 AttemptId/SceneRevision/OperationId，核原 Lookup 的 Before/After/OrderedFacts 引用；Frame 保留原 Fact.Index。t=0/179/180 和最后一事实完整180ms均有断言；负值/NaN/±Infinity 不推进。 |
| P27-01 普通事实 | XML 实际 L1：Direct 15→0、Enemy Strike 100→95、RouteLocked、PhaseSelected；L3：Direct20→4、A蓄力、B攻击100→95、TemporaryRouteRemoved、PhaseSelected。HP/游标/临时线/锁线及引用逐项检查；播放全过程头、Random、存储调用数与磁盘不变。 |
| P27-01 暴击 | 专门真实 SC01 见证：预先声明搜索 [0,4096)，首命中 seed=109、sequence=54、C=1/1000、f=0、原字2800022000。公开求值找分支后从未推进初态经真实019攻击，实际 Label 出现暴击；没有注入字/内部成功DTO或把搜索当频率证明。 |
| P27-02 一次性/门闩 | 重复 Query/Retry/Resolve 不重播，不重复回报；当前等待令牌但没有本地播放进度的 RebuildLatest 也正确完成一次。真实 Published 回调期间 Busy/None 不启动，回调后原 PlayOriginal 才启动。完成回报原令牌引用与三值，不靠本地“播过”集合。 |
| P27-03 过时/中断 | 已完成A1→A2、A1仍显示但新A2已挂闩、新Attempt 三类实际路径均检查旧 Advance/Close/Dispose/Query/完成不能解A2或回装A1。Skip/Close/Detach/实际焦点转移/低内存等效入口/调度异常等效入口均通过；覆盖先清、最新头先读、再回原token，存档/业务不变。 |
| P27-03 生命周期 | Host 用 epoch、controller Generation 和实例引用防旧调度回调；Close 停调度、清覆盖、注销自己的事件/低内存订阅，Dispose/重复Close幂等。重新Attach先结束旧宿主，仅明确新页面接管无条件调用019.RebuildLatest。正常完成/普通取消只回原token。 |
| P27-04 保存/恢复 | 真文件 Snapshot.Flush.after→SaveFailed、Marker.Promote.after→CommitUnknown：旧权威头、零播放、准确诊断；同原请求及 canonical bytes 经 Retry/Resolve 后一次播放/一次真实 marker promotion。Unknown时关闭不虚报完成。实际Rollback、新实例 Existing打开和新实例 Pending Resume均只重建，不重播。 |
| P27-05 UI宿主 | 真实 EditorWindow/panel，经实际 PointerDown/Move/Up 进入019；实际HP/节拍/阶段Label和Board覆盖变动，结束后可继续画第二条线并到WonPendingSettlement。另有真实UI Toolkit schedule推进，非全用手动时间。未把无图形panel等同物理输入/像素测试。 |
| P27-06 范围/回归 | 无filter全量3420/3420 Passed，失败0、跳过0；旧3402名称多重集合完整保留（原3304＋P1新增98），027新增18。旧断言由655项原字节继承核实。源码、DLL和XML绑定成立。 |

Stage映射静态完整覆盖 TemporaryRouteRemoved、RouteLocked、PendingLinkAdded/Removed、FaceChanged、PhaseSelected。RouteLocked取原After.Board的同Pair路线；跨面/原事实不足以支持中间图时保留原事实反馈并显示已知终态，不伪造新的规则状态。显示覆盖不替换 input.Controller.View 的权限判定。

同Attempt/Scene仅Commit变化的代码路径保留当前原播放，完成仍 QueryView 最新头；真实同Scene永久提交运行见证不足，单独列未运行。合法AwaitLinks/全倒补线的映射也仅静态检查，未用内部构造成功绕过公开领域边界。

## 实际运行与同版证据

E17=TestArtifacts/FMDemoB17/62aa18a2eb9443d195296216f0184526；本包为 E17/stage027。C 为唯一 Unity 执行者。

| 运行 | PID、UTC 与真实结果 |
| --- | --- |
| compile-01 | 32148，13:50:17.6775629Z→13:51:12.2288386Z，实际退出0，但日志存在两条新类型 CS0246；不计有效编译，失败输出保留。仅自然新增六meta。 |
| compile-02 | 37900，13:52:04.7954831Z→13:52:17.2211971Z，退出0，错误0；668源项前后相同。 |
| compile-03 | 21076，13:53:43.8514840Z→13:54:03.4169311Z，退出0，错误0；关闭清理顺序/重复结果条件调整后编译，源前后相同。 |
| compile-04 | 17532，13:58:26.3886318Z→13:58:44.3681600Z，退出0，错误0；新增暴击见证/面板实际标识断言后有效最终编译。 |
| tests-01 | 18668，13:59:53.7528398Z→14:09:28.3648985Z，退出0，3420/3420；XML实测时长555.1415134秒。 |

以上 UTC 均为2026-09-23。每轮实际 precheck 无已有Unity；固定 EXE SHA256 ac873fb31f0ee946ec209d943ec0b066e432377f96740874dda3ab1cb7bc2895，Hidden启动，完整argv/cwd/Process.ExitCode/stdout/stderr已核。测试没有 -quit/filter/选择用例参数；未杀用户进程、改许可证或把日志error掩成通过。

R 独立核实际 before/after 物理副本：compile-01 1014→1020，其余四轮各1020→1020；implementation/all-assets/protected/dll/extra-copies并集与物理集合相符，逐路径长度/SHA一致，无额外副本。有效测试源前后均为243121…，36 DLL/PDB在最终编译后及测试前后均为 bad0f4469095e00607da18d9408b3358865de0a0d463631d23b3a412a2f829b4。

XML FMDemoB17P27-EditMode.xml：2402156字节，SHA256 c8c3f8e962600d669187ce9efb2d39ccd64e194ffaa158d1dcb54bc88a12a18d；3420具名集合Ordinal排序+LF SHA256 99d85df383132013dc3389d6a731fb89fb6d6006e7c866f72b10bb1a93dbffe8。保留的旧3402集合仍为 d1dafe7bfcc31bf8213e01e285ba4704d74118348b1e181f738bc547d199e59d。

最终编译日志63401字节、SHA256 3947a89fb8ac8c145d0ccc858427c8c950c50d4e6edb24dcb769abdca7d00838；测试日志1506336字节、SHA256 892caabf8e4cf70b2b72c80717331d5f8d5e9c156310cc57b09a1d24f75e1fd6。XML根输出与runs/tests-01/EditMode.xml一致，固定日志、五轮验证及自然新根均实际进入归档来源。

R 于14:20:28.6320989Z实际核新归档三集合：源/payload/ZIP各14907文件、1303905578字节，canonical=0134c3639606370b1f5383444f94b1e5aadf74c8c991cdae7808d5cd101811cb；34个空目录在实际源、payload及ZIP也全等，无额外/缺失，源前后字节及时间稳定。自然B12/41ff896650e042f4a220c6499ae8ccc7、B13/92d2753b8cc2437789706350a2b5df7d完整入档。ZIP199471209字节，SHA256 1727df06eb1241c03e413ce4a8cd8687bce9c51115a1c9068a5ef228f653a17b。

late/bindings.json为5683字节、19项，SHA256 1e203d5336d9639cab34244e47a7ed08b57247c84d5f05f1a60c7663fcca1497；14:27:53Z逐项实际长度/SHA匹配，包括四报告中的027两份、原始XML/日志、归档、native和历史元数据清单。33项readInputs及P1报告继承保持。playback-evidence的18项fullname/result/duration/output按XML文本换行统一为LF后完全一致，原XML字节另以SHA锁定。

late/native.json 4128315字节、SHA256 a62e87eceb84a2d2fbad25c93b1f76463caba03dd030841fc13f1c8ed89e1e6b；R逐物理行比对131条实际CommandExecution/FileChange、3个context及1个task_started，ordinal9898–10965全匹配，没有导出推理或预造完成。此阶段取证后发生的formal final/task_complete另按本报告完成门核实。

R 于 14:23:53–59Z 复核独立保护入口493项：490项原字节不变，三协调稿仅为已登记的 SD00 r129/§260 派发记录更新；§256–259 冻结哈希仍相同。没有将协调记录更新误记为 C 越界。

R 于 14:25:33.4569141Z 完成物理元数据复核：原67个历史根308409文件与12:44:33的独立入口逐组一致；P1完整证据根及其两处自然根共19742文件，与13:37:10独立冻结一致；两helper的字节/SHA/mtime不变。合计71分组328153文件，与C入口/出口清单对应。只采旧文件路径/长度/mtime，没有重复全字节验证旧多GB历史；全部本批新归档另按实际字节独立核验。作者两阶段正式报告、P1提案与本批代码在完成门后再次核对无漂移。

## 保留边界与下一步

没有可操作的行内修正项。本结论只覆盖027；P1另有独立报告，027不依赖其候选参数或正式发布决定，不用027通过抵消P1问题。

合法AwaitLinks/全倒补线、同Scene永久提交、跨面中间显示和零事实PlayOriginal没有新增公开运行见证；相关映射/收尾仅静态核查。实际OS低内存事件和真实调度器抛异常未触发，已测的是公开入口等效收尾。物理鼠标/触控、像素检查、交互式Editor/PlayMode、Player构建、正式场景及完整Demo未验证；180ms仅技术试调，未称手感验收。

未发布正式内容、未实现PlayerSave；025-P2需另行精确派发并独立审查。SD00可接收027的独立结果，B17其余门未闭合前不放028，不扩首Demo止029的范围。
