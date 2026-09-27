# FM-DEMO-017-C2-EVIDENCE 补证交付
STATUS: COMPLETED
本次原C2实施turn：01a0c79f-bb30-7b92-8464-4859867f5f88；任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；原项目local。
仅完成E1归档补证及§172限定的E2历史披露，等待独立R复核，不自判ACCEPT。F1/F2、2767测试及14场景沿DEMO-B13-C1-R1已核结论引用，本次未运行Unity、项目测试、dotnet构建／执行或任何存档实验。

## CHANGED FILES
仅新增：
- docs/system-design/2026-09-17/demo-017-c2-delivery.md（本报告，≤160行）。
- docs/system-design/2026-09-17/demo-017-c2-scope.json（≤1536KiB）。
- TestArtifacts/FMDemoB13/ca9f9585cb6a4e81a3ec34e3aea275e3/（本次唯一新证据根，以下简称C2根）：原字节payload、副本来源／阶段清单、zip、逐项核验、原执行记录及本次命令证据。没有新手写脚本或新实施源码；payload中的.cs／.csproj均为原字节副本。

## ACCEPTANCE CRITERIA
| 项 | C补证自检 |
| --- | --- |
| E01 | 满足：63漏项／58082字节及33旧work准确核定；实际源、实际payload、实际zip三集合均2583项，零缺项／额外项／差异／重复／路径逃逸，红绿各14份完整源／配置齐全。 |
| E02 | 按§172满足披露边界：准确原PID／启动UTC／CommandExecution已绑定，无原直接Unity退出捕获，永久保留null／NOT VERIFIED；没有把日志199或wrapper1移填实际值。 |
| E03 | 满足：原505实施项、276meta、六C1根2506项、八旧根3708项、全部旧原件及报告保持；只写本表三处新建范围。 |
| E04 | 满足：保留真实原命令、cwd、起止UTC、stdout／stderr／实际退出；独立来源全集与zip条目清单、完整流SHA及前后校验均可回溯。本次补证与原已核行为明确区分。 |

## VERIFICATION
冻结契约为system-task-packets.md r89 §171～174，派发整稿SHA256：7a03020fe2d1abd074d0cf835717c6305c745a480e1e2cd5e3fc6b652bbba342。06:03:43 UTC观察中，第171～174节仍与开始时逐字相同；允许的后续状态登记不扩写入范围。
绑定原C1 delivery SHA256：8cb64b9674dde7c3c18be5e1721985caa1f62bdad5a63cc21a955f3f384e15fc；原C1 scope：906139e1f954149c8d1d14377eeae91883f8cde297ef6fbc8bbddcab4aba71e0；原C1-R1报告：c06fe10420daa830ac0fb223a117d658a0d4a89ba56928e6fd5aad9e3f131684。均保持原字节。
继承原after完整501项及capturedAtUtc=2026-09-22T05:00:14.9731359Z；原implementation完整505项仍无独立capturedAtUtc，原finalVerifiedAtUtc=2026-09-22T05:00:18.085419Z。C2 startedSnapshot单列实际当前时间，没有倒填历史。起点仍为技术行为已核、整包NEEDS_FIX的C1版。

使用Get-ChildItem -LiteralPath -Force逐目录枚举，遇reparse即拒绝；验证另用Get-ChildItem -LiteralPath -Recurse -Force重新枚举原目录，不使用Git忽略规则定义全集。

| 原范围 | 实际数量／比较结果 |
| --- | --- |
| C1六根完整实际文件 | 2506；规范SHA 45c3dd9f3ab99aeccf95cc4b19725fcf01c97ed6bc57849f5d918d656ba41dc1 |
| 原2472清单中位于六根者 | 2440；逐项字节保持 |
| 原2472清单的根外来源 | 32；原日志／XML／工具自然产物，逐项补入新payload |
| 旧归档比较的明确自引用项 | 3；all-validation-products.zip、natural-products-manifest.json、archive-verification.json |
| 六根中旧清单实际漏项 | 63＝61个.tmp＋2个.csproj，共58082字节；规范SHA 49b2d1c724c0f8f366fed41fa551079ffe9ccdc4a5c72e6c7d3fce2d07124806 |
| 八旧根实际全集 | 3708；规范SHA 56b3ae40c393884eab4ef90d9507f1068c58b1ce15d21e47cfa28720a0a1402f，与原R冻结相同 |
| 原priorArtifacts与八旧根交集 | 3675；漏33个work，另有66项根外条目，因此旧3741不是八根3708的同义范围 |

上述三项自引用排除只用于解释旧归档比较。C2新payload已把这三份旧文件本体全部收入；本次选定原来源不存在payload排除项。
63项分布：B12红／绿各5，B13红／绿各23，旧C1矩阵5，原C1证据根的冻结csproj各1。两份精确路径均在原C1根1889c36196bd4de69122cf7cfb1f4ad6下：
- red-source-bytes/Tools/FMDemoSaveRecovery/FMDemoSaveRecovery.csproj
- green-source-bytes/Tools/FMDemoSaveRecovery/FMDemoSaveRecovery.csproj
各907字节、SHA256 f9fad2de9e99b5d5b4d2c31542bb5015bfb2cb9c4a3fcd731e7e9b6ff66da31b。红／绿各14份源／配置逐项与各自原阶段冻结SHA相同。
33旧漏项规范SHA：5aa6ca05a8bd935264bb4382bbf92bcd162c10e0462921bcc33bc3a227d7a5ee。20项另匹配016／016-C1原压缩清单的逐文件行，5项另匹配原017矩阵reader-result实际文件记录；全部33项都属于已冻结3708全集，本次完整行表重算为相同历史规范SHA。其余8项明确以该历史全集成员证明为依据，不伪称存在另一个未找到的逐文件历史记录。每项依据在prior-33-original-basis.json。

本次选定来源为：六根2506＋原根外32＋旧遗漏33＋历史报告／scope12＝2583项，原字节合计26329928。所有原项目路径按payload/<原项目相对路径>映射，文件时间字段标为“本次观察到的源LastWriteTime”；复制时刻另记，没有把复制时间称为原创建／捕获时刻。
归档生成直接枚举实际payload；独立校验则重新枚举原六根、八旧根，重新求根外与旧漏项，再单独读取ZipArchive.Entries。没有以旧漏项清单或新payload清单自比来定义源全集。
源／payload／zip规范SHA完全相同：39e5cc893e95647b8d25737aaaed1cedaba4d5abb4adda99dc244b98f6cea41e。全部2583条实际解压流完整计算SHA并核长度；缺项、额外项、字节差异、重复条目、大小写冲突及路径逃逸均0。
新归档：C2根/complete-original-payload.zip，10317965字节，SHA256 38d0bb855a26708bceb9d49e5309e699b17bd7505f7e8685358221426fd2ae91。
来源、归档实际条目及独立结果分别在source-payload-manifest.json、independent-source-reenumeration.json、actual-archive-entry-manifest.json、independent-archive-verification.json；所有大清单在scope有带解码长度／SHA的压缩记录或文件SHA引用。

以下均为2026-09-22 UTC、本次真实内联命令；完整请求和原CommandExecution保存在同一原C2 turn及C2根记录中：
| 阶段 | 脚本内实际起止UTC | 真实退出 |
| --- | --- | --- |
| 有效开始冻结／无忽略全集 | 05:43:48.1531532～05:43:58.9759953 | 0 |
| 实际集合与63／33差集 | 05:45:32.7271036～05:45:33.8852242 | 0 |
| 逐项历史依据／红绿14／E2绑定 | 05:55:48.5023026～05:55:49.8393575 | 0 |
| 原字节payload复制 | 05:57:23.6522518～05:57:52.5280973 | 0 |
| zip生成 | 05:58:35.3101557～05:58:40.5192184 | 0 |
| 独立双向与旧源不变核验 | 06:00:53.5424554～06:01:04.6149231 | 0 |

保全原C1全部42条CommandExecution，包括E2准确原记录；只选命令事件，没有复制其他任务内容。C2的command-executions-through-verification.jsonl保留18条已完成原生命令事件；之后的导出、交付整理和最终门核另有完整工具请求／返回记录，并仍属于同一准确原C2 turn。原生记录含分别的stdout／stderr和毫秒起止时刻，未把显示用chunk_id当原CommandExecution ID。辅助查询曾在工具显示中截断，已从原CommandExecution取得490640字符完整stdout，未手工重建。
本次四个辅助命令实际exit1已原样保留：数组嵌套导致路径拼接、读取活跃任务记录的共享模式、UTC解析丢失时区的guard、PowerShell布尔值拼写。分别修正为扁平文件序列、只读共享打开、准确UTC ticks及$true。另一次清单形状显示把包装对象计为1，后续依据直接解码.files并核1090／1426历史规范SHA；其原输出同样保留。这些都不是Unity／项目测试运行，未写任何旧来源；有效归档与双向校验真实exit0。

## SELF-CHECK
E2已直接读取原C1的exec-7adf500f-e9bb-4489-9388-a0ebeb82832c：原Unity PID30168，启动UTC 2026-09-22T04:46:30.7557900Z；原启动记录及700字节失败日志SHA ba8a55b9e0c6c8759035d7c2a30f90fc3d52497fa594a4d99da37b92080d3dee相符。
原命令stdout只有等待及DLL采集错误，wrapper实际exit1；原日志声称return199。没有找到准确绑定该进程的原实际Process.ExitCode直接捕获，所以unityProcessExitCode=null、unityExitCodeCaptured=false、status=NOT VERIFIED。
只援用SD00 §172已经批准的单次历史记录例外；该失败不作为有效编译依据。原有效红编译／红反例、绿编译／2767 Passed及14真实矩阵的实际退出要求不变，不扩到其他进程或后续验证。原R是否整包ACCEPT仍由独立复核决定。
原C1报告的“所有产物完整”应更正为“已列2472项均匹配，但当时漏63项；priorArtifacts另漏33项”。C04无条件Passed也须限定：行为及有效最终验证已核，归档经本C2补齐，该一次历史退出码仅按§172已披露例外处理。旧C1报告、scope、清单及zip全部保留，未倒改历史。
本次开始与归档后505实施规范SHA均94d317b8cc269d79f6d218c7c5e84b0e166bf7d06df3c5fefc6f8290820922fe；501规范SHA b57f9488fb2095956054e2d31c51a4d16070726aa59e451ca9f9df3ce687ea80；全部276meta／GUID保持。六根2506、八根3708、原3741条已列旧产物及172个去重受保护输入分别核字节不变。
C2自身manifest、zip、核验结果和命令记录不属于原来源集合；scope逐名列出这些新元数据及自引用原因，未用后缀过滤隐藏漏项。C2根位于原十四根之外，未递归收入自己。
未作源码／测试／meta／工具配置／旧证据修改，未作网络、依赖安装、Git写入、进程强杀、新任务或子代理；GLM范围未作为依赖或修改对象。

## PATCH
实施代码差异为0。交付补丁仅为上述两份新报告与一个新证据根，原文件不覆盖、不移动、不清理。

## BLOCKER
无执行阻塞。PID30168历史实际退出码仍NOT VERIFIED，是§172批准的限定例外，不是已验证值。
正式交付后结束本次原turn并停改，由SD00读取final；R须等本次准确C2 turn completed后复核。本次不派018，不自行作ACCEPT裁决。

