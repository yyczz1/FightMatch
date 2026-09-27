# DEMO-B13-C2-R1 独立复核

VERDICT: ACCEPT
Scope: PASS
Acceptance criteria: PASS（仅适用 SD00 §172 已批准的单次历史退出码例外）
Verification: 原目录、payload 与 zip 独立双向逐字节核验；原执行记录逐条回溯；既有同版行为证据保持。
Notes: 该历史退出码仍 NOT VERIFIED，限定例外已批准；没有将未知值改称已捕获。本报告只裁决 017 的 C2 补证版。

## 门槛、版本与范围

审查任务：01a0c1cd-dce1-7ac3-8780-06163cb0acfc；本次准确原 R turn：01a0c7a0-4df8-75e3-ba6f-73ffcfd3986e。
作者任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；准确原 C2 turn：01a0c79f-bb30-7b92-8464-4859867f5f88。
C 于 2026-09-22T06:16:06.955Z 正式 final；原 task_complete 为 06:16:07.181Z，工具同时确认该准确 turn completed、error=null。R 在两项门槛满足后才正式复核，未以 C1 completed 代替。
冻结依据为 r89 §171～174，派发整稿 SHA256：7a03020fe2d1abd074d0cf835717c6305c745a480e1e2cd5e3fc6b652bbba342。后续身份登记与 SD00 协调稿更新没有改变本包白名单、§172 裁决或验收条件。
只新增本报告（≤200行）；其余只读。C2 与本 R 均未运行 Unity、dotnet build/run、项目测试、真实存档实验、强杀、Git 写入、新任务或子代理。GLM/R-G1 不作为验收依赖，不审改其实现。

| 绑定交付 | 规模 | SHA256 |
| --- | --- | --- |
| [C2 delivery](demo-017-c2-delivery.md) | 78行／10229字节 | 16d6f5b5a39475bf47c072803a802330f8f4554592c021b961ee25e79a5818d5 |
| [C2 scope](demo-017-c2-scope.json) | 1069216字节，低于1536KiB | afd2c1598ace998d627b93aee378e113a8b1766b071b73470b208a4c5b498032 |
| [新完整归档](../../../TestArtifacts/FMDemoB13/ca9f9585cb6a4e81a3ec34e3aea275e3/complete-original-payload.zip) | 2583 entries／10317965字节 | 38d0bb855a26708bceb9d49e5309e699b17bd7505f7e8685358221426fd2ae91 |
| [原 C1 独立报告](demo-batch13-c1-code-review.md) | 163行／22675字节，保留原 NEEDS_FIX | c06fe10420daa830ac0fb223a117d658a0d4a89ba56928e6fd5aad9e3f131684 |

唯一 C2 新证据根为 TestArtifacts/FMDemoB13/ca9f9585cb6a4e81a3ec34e3aea275e3；下文称“C2根”。它在十四个旧来源根之外；实际共2608文件＝2583原字节副本＋25归档／元数据文件。
原 C1 delivery SHA仍为8cb64b9674dde7c3c18be5e1721985caa1f62bdad5a63cc21a955f3f384e15fc；scope仍为906139e1f954149c8d1d14377eeae91883f8cde297ef6fbc8bbddcab4aba71e0；原B13报告仍为ede6d3a9fc7a523dc444e22ac7916ff577626429c6fdb87b2b571cbcba887653。C2没有倒改旧结论。

## E01：全集归档闭环

R 在作者补证完成前独立冻结了六根、八根、根外来源及505实施／276meta；正式复核重新逐目录 Get-ChildItem -LiteralPath -Force，含隐藏及忽略文件，遇 reparse 拒绝递归。实际无 reparse。
预期来源由实际目录、原 C1 清单的根外项、旧清单差集及明确12历史文档共同确定；没有用 C2 清单或 zip 自身定义源全集。随后独立枚举 payload 和 ZipArchive.Entries，读取每条完整解压流计算实际长度／SHA。
2026-09-22T06:26:52.4857473Z 的 R 实算为2583来源／2583副本／2583条目，源原字节总计26329928；三边规范 SHA均39e5cc893e95647b8d25737aaaed1cedaba4d5abb4adda99dc244b98f6cea41e。
逐项缺失、额外、字节差异、重复entry、大小写冲突、路径逃逸及解压长度异常全部0。映射严格为 payload/<原项目相对路径>。

| 独立来源集合 | 数量／核定结果 |
| --- | --- |
| C1六根实际全集 | 2506；规范SHA 45c3dd9f3ab99aeccf95cc4b19725fcf01c97ed6bc57849f5d918d656ba41dc1 |
| 原2472清单与六根交集／根外 | 2440／32；原列项全部保持 |
| 原清单63漏项 | 61个.tmp＋2个.csproj，58082字节；规范SHA 49b2d1c724c0f8f366fed41fa551079ffe9ccdc4a5c72e6c7d3fce2d07124806 |
| 八旧根实际全集 | 3708；规范SHA 56b3ae40c393884eab4ef90d9507f1068c58b1ce15d21e47cfa28720a0a1402f |
| 原priorArtifacts | 3741＝八根内3675＋根外66；与实际3708属于不同范围 |
| 原prior遗漏33 work | 规范SHA 5aa6ca05a8bd935264bb4382bbf92bcd162c10e0462921bcc33bc3a227d7a5ee |
| C2原来源总集 | 六根2506＋根外32＋旧漏33＋历史报告／scope12＝2583 |

旧 all-validation-products.zip、natural-products-manifest.json、archive-verification.json 三项仅在解释“63漏项”时明确排除；C2实际把这三份旧原件本体全部收入，没有原来源payload排除项。
红／绿源码目录各实际14文件，与各自原冻结逐项同字节。两份 Tools/FMDemoSaveRecovery/FMDemoSaveRecovery.csproj 均907字节，SHA f9fad2de9e99b5d5b4d2c31542bb5015bfb2cb9c4a3fcd731e7e9b6ff66da31b，已同时进入副本和zip。
红14源规范SHA为fc15e3a2c50a7c42a594c9e4511ae6bf8ecbf024545fb15bd2e0aad4f45875f3；绿为354b66484fdfe1e3f0142585c8827ea9f36e15d2d983ece68b2e1c59b0c58978。原失败日志、启动记录、failure.json、前后冻结及有效红绿记录一并保全。
33项历史依据逐条核对：20项匹配016／016-C1原压缩清单行，5项匹配原017矩阵reader-result.tsv的原文件记录；全部33项均处于原R已冻结的3708全集，现重算该全集SHA完全相同。其余8项准确标注依赖该历史全集成员证明，没有伪造未找到的逐文件历史清单。
C2 scope 的5个压缩块均解压长度／SHA有效；来源、实际条目和重新枚举清单均与R独立结果一致。复制观察时刻全部在实际复制命令区间内，源LastWriteTime明确只称本次观察值，未冒充历史创建时刻。
25个非payload文件逐名对应scope的22个元数据引用＋3个结束记录；实际零遗漏／额外，22个已有引用的长度／SHA全部匹配。生成本次清单、zip和结果的自引用边界明确，没有再次以广泛后缀过滤掩盖漏项。

## E02：仅批准的历史退出记录例外

准确原失败进程：Unity PID30168；StartTimeUTC=2026-09-22T04:46:30.7557900Z；原 C1 turn=01a0c766-c723-7453-a0bb-3ac300182c8f。
准确原 CommandExecution：exec-7adf500f-e9bb-4489-9388-a0ebeb82832c。C2保存的完整事件与原事件内容一致；原启动记录SHA=86224b759ff74217c3a6cbbd86570dc34c2c722395248c3640c06605d5b8c674。
原失败日志700字节，SHA=ba8a55b9e0c6c8759035d7c2a30f90fc3d52497fa594a4d99da37b92080d3dee。原命令在落结果前采集不存在的DLL而失败，未留下直接实际 Process.ExitCode；wrapper1、日志宣称199与后来重试0仍是三个不同事实。
新scope与e2-historical-exception.json均保留 unityProcessExitCode=null、unityExitCodeCaptured=false、directCaptureEvidenceFound=false、status=NOT VERIFIED。启动UTC字符串省去尾零但精确ticks相同。
**该历史退出码仍 NOT VERIFIED，限定例外已批准。** R只核§172的准确身份、诚实披露和事实前提，不宣称历史值已验证；该无效尝试不参与编译／测试通过推导。
例外不扩到其他进程、任何新命令或后续验证。有效红编译、红反例、绿编译／2767与14矩阵的实际退出仍有原证据，且本次保持原字节，符合§172前提。
C2 delivery第67行及scope.priorClaimsCorrection明确纠正原“所有产物完整”和C04无条件Passed措辞，旧C1报告原貌保留。

## E03：范围与原字节保持

| R独立复核对象 | 结果 |
| --- | --- |
| 实际Assets/Scripts＋Tests与工具根文件 | 实际505，无额外／缺失；规范SHA 94d317b8cc269d79f6d218c7c5e84b0e166bf7d06df3c5fefc6f8290820922fe |
| 原501子集 | 规范SHA b57f9488fb2095956054e2d31c51a4d16070726aa59e451ca9f9df3ce687ea80 |
| 完整Assets及meta | 518文件；276meta字节／GUID保持且GUID唯一；meta规范SHA 599b751e695daf8e01bb15434a8629f435bfa9ccedd33ede3b2d463f1bf2db90 |
| R预冻结六根＋八根＋原根外32 | 6246项逐根数量／规范SHA全同；全集规范SHA 1e88153c0b7ca2b6a6499b201c53534ad74f7ea4caed6c412ee9153e2fc2c38b |
| 原prior的66根外项 | 逐项长度／SHA全同；规范SHA d0f493b9edba1ce30dd3001aa758cf2237492f699486d4d9baad00dc3e134168 |
| 旧2472及3741已列项、12历史文档 | 全部原长度／SHA保持；旧zip／清单／日志／XML未覆盖 |

C2真实startedSnapshot单列05:43:48.2433637Z的505及05:43:48.5981268Z的501；原继承after.capturedAtUtc仍05:00:14.9731359Z，implementation没有补造capturedAtUtc，原finalVerifiedAtUtc仍05:00:18.085419Z。
R额外冻结的438个文档／配置输入在C完成后的06:26:34全部原SHA保持。之后仅三协调稿变化，已回溯SD00原事件exec-91762f44-a8cb-4821-9bb2-4849f87cc11c、exec-8517d285-e454-4c30-9e5d-a3c94c796b8e、exec-4b50f8b8-56b7-4dd1-b31a-d52eadeefb1c；归SD00而非C2。相关§171～174正文条件保持，段末换行不改变契约。
检查C2全部22条命令、5次FileChange及实际目录：写入仅两个新报告和一个新Guid根；未新增手写脚本／源码，payload内源码均为已核原字节复制。C2结束后无作者追加修改。

## E04：原命令与真实结果

R将原C1的42条CommandExecution复制逐原始JSON行比对，全部一致。C2初期9条、主导出18条、最终导出21条亦逐行相同、无重复；准确C2原回合实际共22次命令。
第22次最终核验不能在自身返回前写出自己的完成事件，已由final-gate-tool-record.json保全原请求、初次输出、两次poll及actualExitCode；拼接后3247字符与原stdout／aggregated_output完全相同，实际exit0。原回合亦保留完整原生命令与毫秒起止UTC。
另外三份preparation失败／修正及scope-creation工具记录，原request.cmd、cwd、完整stdout及真实exit逐一对应原事件。所有C2命令实际stderr为空；原始stdout中的错误仍完整保留，未按展示截断重建。
490640字符的辅助查询完整stdout已与原事件一致。四次辅助命令实际exit1（路径数组、共享读取、UTC解析guard、true拼写）均保全；后续正确命令exit0，未把这些读取／归档错误当成项目测试失败或隐去。

| 必需阶段 | 原CommandExecution | 实际脚本UTC起止（2026-09-22）／exit |
| --- | --- | --- |
| 当前冻结 | exec-99a1d819-481b-4aaa-8010-414a6fbd4166 | 05:43:48.1531532～05:43:58.9759953／0 |
| 63／33实际差集 | exec-6cfe88bb-6b3c-4de6-b4b2-d29a5bfe6785 | 05:45:32.7271036～05:45:33.8852242／0 |
| 历史依据／14＋14／E2 | exec-7f21ccb1-c85c-4c1f-91e8-b0dded9ee0d3 | 05:55:48.5023026～05:55:49.8393575／0 |
| 原字节复制 | exec-f71ef7df-214a-4efa-8446-b8f94bbcf835 | 05:57:23.6522518～05:57:52.5280973／0 |
| zip生成 | exec-a094bbc7-0c77-491e-928b-7582ebd5efc4 | 05:58:35.3101557～05:58:40.5192184／0 |
| 独立双向验证 | exec-450faaef-ce80-4bd3-bc04-d08d73a1a60c | 06:00:53.5424554～06:01:04.6149231／0 |
| 作者最终核验 | exec-e3027e75-d5e5-402b-8f7d-ee5a605e6741 | 06:14:24.3588319～06:14:52.7991878／0 |

R自己的目录／zip／元数据／原事件校验均为只读内联命令，采用修正后实际exit0结果；未替作者生成证据。其原请求与结果留在本报告绑定的准确R回合。

## 既有行为结论与整包验收

以下引用原C1-R1独立结论，前提已由本次505、原验证来源及旧报告的字节保持核实；本次没有重新分析／修改生产代码，也没有重跑验证。
F1完整已索引work分类修正、未知／损坏work保守门及F2两时点等长异SHA替换已闭合。有效红在原生产上恰9个F1失败，绿2767全Passed；原2747逐Ordinal fullname重数／Passed保持。F2原生产已满足，不虚构生产修复。
最终同版DLL、10个.NET＋4个真实Unity场景、13强杀writer与P10正常退出、14个不同PID fresh-reader及原固定业务断言沿原证据成立。实际故障域仍Windows/NTFS EditorProcessCrash，不扩为Android或断电持久性。

| 原整包／修正验收 | 本次最终结论 |
| --- | --- |
| R01完整加载 | 满足；五片业务、原历史／随机／键结果与最新坏头拒绝沿已核证据。 |
| R02未提交摘要 | 满足；共享严格读取、七类损坏、预算及原expected入口控制保持。 |
| R03完整根 | 满足；原分类／完整性控制与F1已索引完整work分类保持。 |
| R04能力与过时 | 满足；能力逐项拒绝、一次action、证据过时、门／回调控制及F1一致性保持。 |
| R05保留与中断 | 满足；准确当前／Parent、四代清理、F2逐目标替换及重取证续办保持。 |
| R06独立进程 | 满足；14个真实同版场景原证据保持，遗漏work已完整补入。 |
| R07隔离与故障域 | 满足；租约／排他／预算／I/O／Flush及进程归属、顺序、实际故障域保持。 |
| R08范围与回归 | 在§172单次例外下满足；原2688→2747→2767回归链、最终源码／DLL／实际退出和完整归档闭环。 |
| C01原版复现与最小修正 | 满足；九个原版有效红反例及相同测试转绿保持。 |
| C02未知与证据保持 | 满足；真正坏／未知work仍未决，完整work不删且变化使旧证据失效。 |
| C03逐目标替换 | 满足；两个真实替换窗口、精确诊断／已删数量、正对照与续办保持。 |
| C04最终同版行为 | 在§172单次例外下满足；原行为／有效验证保持，E1完整归档已补齐，不称历史全部退出码已捕获。 |
| C05范围与归属 | 满足；C1两旧C#最小变更保持，C2零代码变更，新建范围及旧原件保全已核。 |
| E01／E03／E04 | 满足；独立全集、原字节与范围、完整当前执行追溯均闭环。 |
| E02 | 满足批准的披露边界；该历史实际退出码继续null／NOT VERIFIED。 |
| 规范／规格两轴 | 均满足上述冻结条件；没有新的生产缺陷或阻断项。 |

非阻断记录瑕疵：scope第6465行的allowedNewPaths把新根与末尾“/”拆成两项，系原scope生成命令中的数组拼接结果。该字段未参与写入授权或目标计算；R按§173精确白名单和实际22条命令核范围，没有写入根目录或扩权。本次接受不赋予“/”任何许可，也不要求为该展示瑕疵改写已冻结作者材料。

本报告正式交回后R结束停改，不派018。SD00须等本次准确原R turn completed，再与已完成的准确C2 turn共同登记017整包接收及第23个功能包，之后才按其权限准备018。
用户无需转述、补跑验证或重复裁决；无待用户动作。
