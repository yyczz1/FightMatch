# FM-DEMO-017-C1 修正交付
状态：DELIVERED_PENDING_INDEPENDENT_REVIEW。F1已按真实红证据最小修正；F2新增见证在原生产版即通过，不作F2生产修改。C自验C01～C05通过，独立R尚未裁决，本报告不宣称ACCEPT。
本次原C实施turn：01a0c766-c723-7453-a0bb-3ac300182c8f；任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1。工作目录D:\Unity\UnityProj\FightMatch，原local；没有Git写入、新任务、子代理或018派发。

## 冻结契约与来源
- 按system-task-packets.md r87 §165～168及原§158～162；派发整稿SHA256：85be277fb4124673894cdf95aad6e485512c12be073ddc7a19387a3899c35a9f。
- 最终核到整稿SHA256 ed176e1725717d34be037302a9cad951c7c69d29595192ad30eb5f1e1c0bc9ef；§165～168与开始时逐字相同，整稿后续更新没有改本包契约。
- 原R报告demo-batch13-code-review.md SHA256 ede6d3a9fc7a523dc444e22ac7916ff577626429c6fdb87b2b571cbcba887653，唯一NEEDS_FIX。
- 原scope SHA256 65c72b7bd1c7b1a259168c5963de1e292de9aab002cbfcb981afc276e64b4902。继承after完整501项、capturedAtUtc=2026-09-22T03:55:41.3479783Z、规范SHA da9c585852b04bcf6a00b1664e715b77f4988501925e268e4dc4e18eaab685b9。
- 继承implementation完整505项、规范SHA 84305bcbd1117d7b83a1f54b1107ca9f2240c873eee58ae508999d2a35c90ce9；该字段没有capturedAtUtc，未补造。原finalVerifiedAtUtc=2026-09-22T03:55:42.1086241Z。
- 本次startedSnapshot另记2026-09-22T04:43:18.2537212Z；起点是R判NEEDS_FIX的作者冻结版，未标为已接收实现。
- 原两C#完整字节、501/505原清单和本次起点保存在新scope与验证根baseline.json。GLM／R-G1独立工作不作为依赖，未读取其实现、修改其目录或报告。

## 两份源码与行为
仅修改Assets/Scripts/FightMatch/Platform/LocalSaveRecovery.cs及Assets/Tests/EditMode/FightMatch/SaveRecoveryTests.cs，无新源码／meta。

| 文件 | 原SHA256 | 最终SHA256 | 实际行差 |
| --- | --- | --- | --- |
| LocalSaveRecovery.cs | 9efd226124d34c7318e81e25ee1c25adc92980dc02c7fd9ecb4b38c176840a88 | 970412b74f77d444c9ea72a912adc43126f7b406a922f762c78d0f2525ef0d53 | +4/-6 |
| SaveRecoveryTests.cs | 96fe004334851d5601fe1d470a98cf8f70d4a6524296dbe94e264e11097892f4 | d88b9e63dee0b966f2ba22f34c502d881a5bdb26a2e2313eb58d1f8038c56fc2 | +128/-1 |

合计增删139/360行，生产10/80行。原测试正文逐字保持；唯一包装器改动是在真实枚举返回后补一个观察Hook，不改原Hook顺序、断言或故障行为。
恢复候选循环只处理原Inspect判为Pending的snapshot／work。IndexedOldCopy／IndexedCompleteWorkCopy保持原分类、归属、长度和全部实际SHA，不置未决、不添Pending根、不改Current。每次仍重新读取Inspect并核全部文件证据；完整认定不会豁免后续损坏。旧选头器、Core、共享Cases、控制器、Load／Prepare／Cleanup删除逻辑均未改。

## F1与F2的实际见证
新增20例，红绿测试文件SHA完全相同：
- F1正向9例：四代真实保存，marker-work／snapshot-work／双work × G1／准确前代G3／当前G4。先核Inspect Ready及Load，再Prepare并按原规则EndUncommitted，随后核ReadRecovery Ready、准确分类／证据、无Pending根、能力回调恰一次。Cleanup只删G1/G2四个最终文件，全部work及当前／前代字节保持；重开仍Ready，四个原键均可查。之后破坏work，旧view变StaleContext，新的Pending关闭Load／Prepare／能力授权／Cleanup。
- F1负向8例：marker／snapshot分别为部分、摘要损坏、其他提交内容、无法归属提交名。实际文件保持Pending／要求不完整，冲突门拒绝且零删除；没有把所有work放行。
- F2三例使用真实Windows适配，先完整ReadRecovery取得view，再在Cleanup自身第二次枚举返回后的逐目标OpenRead入口写入等长度、异SHA的实际文件字节。首目标为G1 marker；后续目标为已删除G1 marker后的G1 snapshot；第三例无替换成功。
- 红XML实际调用序列：首目标最后枚举／Open Hook／实际替换／Close位于该例输出44／45／46／47行；后续目标为44／49／50／51，替换前恰有一次Delete.after。无替换对照为44／45／46。完整输出及独立解析保存在f2-red-witness-audit.json。
- 两次替换均返回CleanupInterrupted，Diagnostic.Code=StaleContext、FieldPath=Cleanup.Target、Stage=Cleanup.<实际目标名>.Verify；替换文件仍存在，已删除数量分别0／1，Value为空而不伪称完整清理。旧view拒绝；重开／新ReadRecovery后清除余下4／3文件。当前、准确前代和四个历史键保持。
- F2原生产版即通过，原逐目标长度／SHA核对有效，没有为F2改生产代码。

## 实际命令、红绿与失败保全
Unity固定D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe，文件版本2022.3.18.13802474。每次先确认无Unity，Hidden启动、保存PID及UTC、WaitForExit读取实际退出；测试无-quit。源码冻结记录包含每阶段14份手写源／配置及501／505项。
完整请求／cwd／工具chunk_id／session_id及原始返回存于raw-executions.json；准确原CommandExecution由本次原C turn回溯，不将chunk_id伪称CommandExecution ID。下表日期均为2026-09-22 UTC。

| 运行 | PID | 实际起止UTC | 实际结果 |
| --- | ---: | --- | --- |
| 红编译有效重试 | 448 | 04:49:17.1224538～04:49:51.7960111 | exit0；原生产SHA保持 |
| 红全量EditMode | 37368 | 04:50:38.4527179～04:51:41.0771780 | exit2；2758Passed/9Failed，零Skipped/Inconclusive |
| 绿编译 | 38476 | 04:53:11.9186042～04:53:27.5154043 | exit0 |
| 绿全量EditMode | 14264 | 04:54:20.2962713～04:55:24.1806199 | exit0；2767Passed，零Failed/Skipped/Inconclusive |
| 固定SDK／离线restore／build | 各原工具记录 | dotnet-version/restore/build.json | 各exit0；SDK6.0.412，build零警告／错误 |
| 新矩阵控制器 | 32560 | 04:56:51.7380772～04:57:48.9587763 | exit0；14场景全部Passed |

红阶段9个失败均为F1九个正向组合的“Expected Ready / But was Pending”；原2747项的Ordinal fullname重数及Passed全保留，新增八个无效work与三个F2通过。修生产前已保存14份完整红源码、XML／日志／冻结及zip。
红XML SHA256：0fc5f9dc6e660e2ff50e3e633ee78e88faf209339db6cd8b63462d780c3f718d；绿XML SHA256：3d4f6baf02ac0ba2a8cc6bb5e0b6439733ee2167e9a6007279fa0ecd16bf73ec。绿色仍逐名保留原2747并新增20全过。
红有效编译log SHA af6f15f53a0314f115bc9446386213c9edf45fc60bcce912069f7dc176adeda1；红tests log SHA 3be6fccfe3d149a443099892e0dd8e023dce66e36b2839c2f13aa820d2c56ed3。
绿compile log SHA b41260dee30ced1b6112585059e32c1c525547bceb7464fcd206ce949fec95df；绿tests log SHA 9caf718d7758266b50f2af6d881e6426c20386336c00694aa3cf737816f23a1e。
固定工具按原global.json、NuGet.Config运行dotnet --version、restore --configfile NuGet.Config、build --no-restore -c Release。CLI_HOME／packages仍限工具.cli／.nuget，离线无新增依赖；工具仅重编原Program及链接Cases，生产DLL来自本次Unity。

失败与证据限制如实保留：
1. 首次沙箱内红编译PID30168在04:46:30.7557900Z启动，Unity授权IPC超时，完整日志写明return code199，未进入有效源码编译。其后记录脚本误用不存在的FightMatch.Tests.dll，脚本实际exit1；该次Unity Process.ExitCode未成功持久化，不能把日志199冒充实际捕获值。日志、启动记录、前后501／505冻结和同版完整14源码全部保全。修正记录名为FightMatch.Core.Tests.dll，并沿原017验证的沙箱外授权路径重试，取得上表有效exit0。
2. 红XML审计首次把CDATA节点直接序列化，辅助guard失败；原XML未改。保留该次JSON和原执行，改取InnerText后确认9个准确F1失败。没有无关编译错误、测试失败或测试预期改动。
3. 只读探索中的不存在猜测文件／PowerShell rg通配路径及空进程查询曾有非0状态；未作为项目验证结果，原执行均可回溯。

## 最终同版矩阵
矩阵根：TestArtifacts/FMDemoB13/f17e61328e634b16a1afd59c8de2250a。启动／身份／屏障／kill-request／实际退出／fresh-reader结果／回执和8份Unity完整日志均保留。
13次强杀只针对原控制器本次创建且复核PID、StartTime、绝对exe、nonce／root的writer，Kill(false)，实际退出-1；P10 writer正常exit0。全部reader为不同新PID、exit0，且在writer实际退出之后启动。4组真实Unity顺序无交叠，未结束用户Unity。

| 场景 | writer→reader PID | 恢复结果 |
| --- | --- | --- |
| .NET P01 | 3052→37292 | G1 Pending，部分正文要求不完整 |
| .NET P02 | 17384→39548 | G1 Pending，Flush候选未提升 |
| .NET P03 | 36188→3940 | G1 Pending，正文Promote后未提交 |
| .NET P04 | 34652→20920 | G1 Pending，部分marker要求不完整 |
| .NET P05 | 2580→4824 | G1 Pending，marker Flush后未封记 |
| .NET P06 | 37588→18608 | G1 Pending，最终marker Promote前 |
| .NET P07 | 39644→9772 | G2 Ready／Committed |
| .NET P08 | 24356→32160 | G2 Ready，控制器实际收到Committed回执 |
| .NET P09 | 6896→24088 | G4 Ready，保留准确G3并新取证完成余下清理 |
| .NET P10 | 11452→13856 | 正常退出，G2 Ready／Committed |
| Unity P03 | 39600→19832 | G1 Pending |
| Unity P07 | 4744→19656 | G2 Ready／Committed |
| Unity P08 | 1896→33656 | G2 Ready，已回答回执仍Committed |
| Unity P09 | 5232→10720 | G4 Ready，准确G3及中断后清理保持 |

源DLL、工具CopyLocal副本、控制器实际报告及矩阵后文件逐一同SHA：
| DLL | 字节 | SHA256 |
| --- | ---: | --- |
| FightMatch.Platform.dll | 73216 | c822e3347a599b7939f9ad433f03ed88f1610e41602f09b3602e0617ffca3def |
| FightMatch.Core.dll | 520192 | a5a102832fe92f2f6fd07be8790bd02ca53737b307dff24360bfc3e9dc5d7ff8 |
| FlowPuzzle.Core.dll | 18944 | 65149856030c2ed1ab426d85b9241556b25a30e35e75bd47d931aeaeac775db0 |

完整程序集另存验证根final-unity-assemblies。实际Windows NT10.0.26200.0／NTFS、.NET6.0.20及Unity Mono6.13.0。证据仍只支持EditorProcessCrash，不扩称Android或断电持久性；没有接入M02／M10／M14、QFramework或业务补偿。

## 验收与范围
| 项 | C自验 |
| --- | --- |
| C01 | Passed：原生产SHA上9例准确红失败，测试不改后最小生产修正转绿；完整work三形态×三归属及四入口一致。 |
| C02 | Passed：8个无效work对照、完整work逐字节保留、后续损坏使旧证据过时；Current／原选头／未决冲突门保持。 |
| C03 | Passed：首目标及删除一项后的真实等长异SHA替换均拒绝，无替换成功、新view续办；F2无需生产修改。 |
| C04 | Passed：最终同版编译、2747旧名重数／Passed＋20新增、10.NET＋4Unity重跑、源码／DLL冻结均成立；首次无效环境运行的记录限制如上明示。 |
| C05 | Passed：只改2旧C#，503其他实施项及276meta SHA保持，501／505与GUID不变；原验证档、GLM范围及两交付权限保持。 |

原017八项本版重新验证：R01五片业务／奖励／历史／回退／随机与最新损坏拒绝；R02共享严格解析及原expected入口；R03根完整性并修正完整work；R04能力／旧证据／同门；R05前代保留及中断，补齐F1/F2；R06本版14真实进程场景；R07租约／I/O／预算／故障域；R08范围和同版回归。其证据分别在本版完整XML、矩阵及scope，独立是否ACCEPT由R判定。
最终14份手写源／配置规范SHA 354b66484fdfe1e3f0142585c8827ea9f36e15d2d983ece68b2e1c59b0c58978；Assets501规范SHA b57f9488fb2095956054e2d31c51a4d16070726aa59e451ca9f9df3ce687ea80；implementation505规范SHA 94d317b8cc269d79f6d218c7c5e84b0e166bf7d06df3c5fefc6f8290820922fe。规范为Ordinal路径＋TAB＋小写SHA＋LF，UTF8无BOM。

验证／归档根：TestArtifacts/FMDemoB13/1889c36196bd4de69122cf7cfb1f4ad6。
红／绿NUnit新根分别FMDemoB13/242aec761e9140158c5ac33916218c68、FMDemoB13/820182926ca24b37a1715a81ae707a09。
原回归未改代码自然新增B12红根af15d35ac74a4915a07c7c9d5a101ada、绿根c65d1a2064a141b18fa251f8ca883f69；未为换根改旧测试。
3,741份既有实验／日志产物及176项受保护输入逐项SHA保持，含原B13三根、更早B12各根、旧全量归档；旧1207项完整zip仍保持原SHA，工具工作bin／obj仅按授权自然重建。
本次all-validation-products.zip含2,472条完整文件，4,753,201字节，SHA256 1704465981b1be7bfd49c9726b1d8cc910d6e063fd1bd271671c50a84cdf06aa；每条长度／SHA已实际解压流核验。清单规范SHA 54ec3aff53703c270a25ea613b806adc0206d9f1bf88357ae74798c1a0ee1b5b，含失败保全、红绿源码／XML／日志、全新矩阵、所有本次实验根、完整原执行和工具产物。
本报告≤220行，新scope≤1536KiB。scope以brotli+base64保存较大清单，附解码长度／SHA；两份原清单及来源仍原样继承。交付后结束本次turn并停改，R须等本次准确turn completed后独立复审；未跨任务发送通知，由SD00读取本final。用户无需重复技术裁决。

