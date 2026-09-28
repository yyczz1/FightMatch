# CONT-C-MAC-R1 实施交付

正式作者状态：**COMPLETED**。独立R审查待接续，本报告不签ACCEPT。

- C thread `01a0e404-d89d-7ab2-bece-3cd1df3fbc52`；host local；准确最终接续turn `01a0e6d3-38f8-7b92-9c3c-6a3df6b258fe`，startedAt1790578997。
- 原包作者turn01a0e410-3e6f-7400-b58c-34cebde8d28c及历史root/run.author保持。006～010实际执行为01a0e63c-ec16-7551-b71c-dee6bd67c93e；主机重启后实际执行映射见resumption424。
- 工作区 `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch`；master起点adaffdc4d7e8329fb4e8ee663ed8918c78ad8f0c；接收基线ce21901b7b5b42bfef7ef34eb4f46155ddbf9353。
- 继承§367/374/380/383/386/394/395；§417/418/420返回语义修正，§424/425与§427/428有限恢复验证。没有C Git写操作、新chat/agent或automation。
- 本回合自然completed/error=null须由SD00在正式返回后核实；提前出现的文件与异步消息不代完成门。

## 实现与修正

- 同一PlayerSession拥有唯一NavigationSession/QueryNavigation；地图、准备、队伍、背包、详情、确认、恢复与结果消费已核发布头。
- 三槽、相邻迁移与永久操作沿原facade/Core；Prepare后先保留原request再Submit；重复Confirm/回执丢失/后移头查原操作，其他owner、F2、S17、恢复门保持。
- 共用BuildPermanentDraft使Craft实际CharacterId=null后经Copy进入原隔离核；Equip/SetPreference保留明确角色。首包缺配方真实拒绝，没有注入正式定义或制造结果。
- UI Toolkit捕获真实目标/context/epoch，只清菜单订阅；原战斗演出/token仍存活。HostRequest仅一次明确交接，不实施H02/H06/Exit/Restart。
- Mac适配保留UTF16身份、flock、CreateNew/无覆盖提升、本实例Flush(true)、链接及非缺失I/O拒绝，FaultModel仍EditorProcessCrash。
- 004后七文件六项修正包括Team父链、精确Null图形关窗预期日志、Equip活动门及恢复/旧确认/后移头原结果/通知失败重绑见证；领域AttemptActive与UI ActiveAttemptConflict分列。
- 008后§418/420五文件修正：装备退出仍落Team并恢复外层Preparation或Map锚；Back越过Team也更新锚。确认保存真实编辑页，重复/失败/空替换、角色/头变化与Prepare拒绝都恢复匹配父链。新增16项测试实例覆盖六类迁移页、后续阵容/迁移、原结果无重复保存、非Map锚pending重绑及完整重建回Map。

## 最终验证

- 唯一工具SHA `33a99396c2ccf39ced3c1fdf25fcec27824e9bb08719849b239d47ce165222e9`；Compile013→无filter完整EditMode Tests015，Tests无-quit。
- Compile013实际exit0/错误0；Tests015实际exit0，3956/3956Passed，0失败/跳过。旧3872次/3867不同fullname逐项保留，新增84次单列；Mac物理探针9/9。
- 最终XML `TestArtifacts/FMDemoCONT/cont-c-mac-r1/runs/015/tests.xml`：2936492bytes/SHA `940e5393444d96013c25a8d8d68a1f6f1f685188c9b088ebe42441e0241d6c2a`。
- Compile后=Tests前=Tests后=交付：808实现/838Assets/441唯一GUID/36DLL-PDB。旧776中766字节不变、10个签准例外；新增32。原425个GUID/meta保持。
- 当前macOS27.0(26A428)/arm64；较重启前记录26.3变化。Unity2022.3.18f1 x86_64/Rosetta及PowerShell7.6.6 Arm64原二进制SHA未变；实际新路径为Applications。六个运行文件/资源在Compile、诊断、全量及交付时核同一SHA，实证见platform-environment。

- RUN 013 — `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/CodexTools/PowerShell/7.6.6/pwsh -NoLogo -NoProfile -NonInteractive -File Tools/Invoke-FM025P2Validation.ps1 -Mode Compile -Stage CONT-C-MAC-R1 -ExpectedScriptSha256 33a99396c2ccf39ced3c1fdf25fcec27824e9bb08719849b239d47ce165222e9`；实际包装exit0、Unityexit0。
- RUN 014 — `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/CodexTools/PowerShell/7.6.6/pwsh -NoLogo -NoProfile -NonInteractive -File Tools/Invoke-FM025P2Validation.ps1 -Mode Tests -Stage CONT-C-MAC-R1 -ExpectedScriptSha256 33a99396c2ccf39ced3c1fdf25fcec27824e9bb08719849b239d47ce165222e9`；实际包装exit0、Unityexit0。
- RUN 015 — `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/CodexTools/PowerShell/7.6.6/pwsh -NoLogo -NoProfile -NonInteractive -File Tools/Invoke-FM025P2Validation.ps1 -Mode Tests -Stage CONT-C-MAC-R1 -ExpectedScriptSha256 33a99396c2ccf39ced3c1fdf25fcec27824e9bb08719849b239d47ce165222e9`；实际包装exit0、Unityexit0。

| 槽 | 真实状态 | Unity PID / exit | 开始→结束 | 运行窗口秒 |
|---|---|---|---|---|
| 001 | PrelaunchCaptureFailure | unknown / unknown | unknown → unknown | unknown |
| 002 | Failed | 24291 / 1 | 2026-09-27T18:59:40.1911560Z → 2026-09-28T01:12:18.2461580Z | 22358.055002 |
| 003 | Passed | 29230 / 0 | 2026-09-28T01:15:31.5692040Z → 2026-09-28T01:16:01.2508270Z | 29.681623 |
| 004 | Failed | 30770 / 2 | 2026-09-28T01:55:11.4844490Z → 2026-09-28T03:07:54.6944540Z | 4363.210005 |
| 005 | ControlPlaneInterruptionNoUnityExecutionEvidence | unknown / unknown | unknown → unknown | unknown |
| 006 | HarnessFailure | unknown / unknown | 2026-09-28T04:27:08.4008560Z → 2026-09-28T04:27:14.5969660Z | unknown |
| 007 | Passed | 23700 / 0 | 2026-09-28T04:32:11.9215290Z → 2026-09-28T04:33:35.3680420Z | 83.446513 |
| 008 | Passed | 24278 / 0 | 2026-09-28T04:34:06.9251750Z → 2026-09-28T05:49:50.3428090Z | 4543.417634 |
| 009 | Passed | 29654 / 0 | 2026-09-28T05:53:21.0630450Z → 2026-09-28T05:53:43.8218580Z | 22.758813 |
| 010 | HostRestart/Interrupted/NoTestResult | 29836 / unknown | 2026-09-28T05:54:37.3172920Z → unknown | unknown |
| 011 | Passed | 11743 / 0 | 2026-09-28T07:22:49.9047740Z → 2026-09-28T07:23:32.1082530Z | 42.203479 |
| 012 | Failed | 12110 / 134 | 2026-09-28T07:24:42.3305390Z → 2026-09-28T08:49:44.9596220Z | 5102.629083 |
| 013 | Passed | 25300 / 0 | 2026-09-28T09:07:37.8424080Z → 2026-09-28T09:08:39.4826980Z | 61.64029 |
| 014 | DiagnosticOnly/Passed | 26586 / 0 | 2026-09-28T09:09:36.0013880Z → 2026-09-28T09:10:01.1234320Z | 25.122044 |
| 015 | Passed | 26925 / 0 | 2026-09-28T09:11:14.1708960Z → 2026-09-28T10:34:45.5418780Z | 5011.370982 |

- 001为启动前隐藏.gitignore取证失败，无Unity；002记录22358.055002秒（6小时12分38秒）后exit1，8个CS0103已在原SaveRecoveryTests例外内修复。日志script compilation仅14.470571秒，长停滞原因未知，CUA超时无UI证据；不计正常测试计算，也不归因用户审批。
- 004为3934/3937，旧3872与Mac9通过；3项失败是精确headless关窗日志未声明。007编译与008完整3940/3940通过，但该版本随后修正返回锚，未当最终接收。
- 005为控制面中断/空槽，原包装exec-585e02c8-d2fc-4c51-8d5b-1c929945bf99 exit-1/6422ms；无Unity执行证据。006是Bad CPU type启动失败，包装exit1，UnityPID/exit均null。
- Rosetta由环境侧以官方工具恢复后才运行007/008；C未安装或更改Unity/SDK/系统设置。
- 009编译实际通过；010于05:54:37Z开始后主机重启，boot06:53:48.577880Z，旧Unity/包装PID缺失。仅CreateNew写HostRestart/Interrupted/NoTestResult，actualExit/endedAt/NUnit均null，保全五个原片段；当前源码/资产/DLL与009after/010before一致，不冒充010.after。
- 012在旧application路径执行时实际Unityexit134/包装exit1，5102.629083秒，无XML/NUnit计数。末栈为FlowCompletionUndoAndDropTests创建ObjectField时读取编辑器缩略图，CachedReader::OutOfBoundsError→abort。源808/资产838/DLL36与011after仍完全相同。
- 用户独立授权的目录整理将application并入Applications，SD00在§427记录环境会话确认和新路径冻结；C未执行搬移、删除或重装。旧路径现缺失，新路径Unity/pwsh保持原SHA，三个内置资源与CodeResources按签准身份核对；未将日志的corrupted文案当作磁盘损坏结论。
- Compile013之后的诊断槽014只过滤准确FlowCompletionUndoAndDropTests.ApplyCompletionResult_Solved_IsOneUndoableCommand，真实1/1 Passed、Unity/包装exit0、diagnosticOnly=true；不计入全量总数。随后槽015无筛选完整回归，diagnosticOnly=false。
- 所有历史失败和中断保留，不重用槽、不把新成功倒填旧运行。
- 最终运行窗口：Compile013 61.640290秒；诊断014 25.122044秒；完整Tests015 5011.370982秒，XML测试时长4979.6116068秒。运行窗口含启动等开销，不代表持续完成用例或CPU计算时间。
- 015等待期间仅对原Unity PID做一次1秒/10ms栈采样：可见Mono数组分配/GC/Rosetta JIT，未解析出NUnit用例名。采样本身不能证明用例推进或死循环；原始采样和SHA保存在platform-environment，最终结论以终态/XML为准。

## 输入、平台与范围

- CoreCLR恢复因原NUnit CallContext依赖失败，原失败结果/stdout/stderr及工具before-text保留。
- 原四无参生成方法以Unity自带Mono/75行固定runner执行，编译PID30655 exit0、runner30660 exit0；源码/种子/断言不改。这是输入恢复，不是Unity测试通过。
- MC06固定Compile003历史DLL，实际mscorlib来自Mono/lib/mono/net_4_x-macos；后续最终DLL另核，未因导航修正或重启重跑无关恢复。
- 12源输入、4staging结果核原长度SHA后CreateNew恢复4golden；原925保护集恢复其中3项、仍缺15，第四cohort另列；126份迁移历史证据复核，初始52仅根元数据子集。
- restore-mono-result/golden-recovery均22388bytes/SHA89c3df6867220b729db547da0b90e8f3df56d32934588ad56135e7aaea8488c4。
- 真实I/O：2605个GUID case，7516目录、12247文件、15链接，≤4096case/40000文件与链接。清单lstat不跟随链接，未删历史。PythonPID/exit和锁见证在mac-probe-summary，与内存故障分列。
- 导航预算生产1180/1860、测试1254/1670、meta143/156。Mac新增181/380、69/200、178/400，meta各11/12。Runtime累计+19/-0≤24/-0。
- 九旧例外+/-：LocalSaveStoreTests19/9，SaveCommitMarkerTests3/3，SaveRecoveryTests8/7，SavePendingRecoveryTests2/2，CandidateApplicationRuntimeTestData2/2，CandidateApplicationRuntimeTests2/2，CandidateLifecycleTestData1/1，PublishedContentCatalogTests4/3，PublishedContentAuthoring3/0；原NUnit声明与旧多重集合保持。
- 工具逐段预算及最终499行见scope-audit；§417之前389行、之后413行/+27/-3，全文保全；当前有限恢复扩展独列。恢复工具226/240、runner75/80。
- Packages两项按现场签发SHA冻结，C未编辑；项目配置、旧资产与Windows实现不因本轮改变。SD00协调稿/START_HERE独立登记。

## 逐项验收

作者证据如下；原CC规范全文、真实源码行、参数化XML实例及补签边界见case-evidence与scope。

| 项 | 源码或证据入口 | 结果与边界 |
|---|---|---|
| CC01 边界/同源 | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC02 N0/F2 | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC03 N1/Query | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC04 N1→N2 | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC05 N2/交接 | PlayerNavigationPresentationTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC06 N3/Formation | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC07 N4/Bag | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC08 N5/空内容 | PlayerNavigationPermanentFlowTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC09 N6/制作 | PlayerNavigationPermanentFlowTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC10 N6/N7取消 | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC11 N7/一次确认 | PlayerNavigationPermanentFlowTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC12 版本/失效 | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC13 N9→N10 | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC14 N8/Prepare与Write失败 | PlayerNavigationRecoveryTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC15 N8/CommitUnknown | PlayerNavigationRecoveryTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC16 页面重建 | PlayerNavigationPresentationTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC17 Application重建/Observed | PlayerNavigationRecoveryTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC18 原结果优先 | PlayerNavigationRecoveryTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC19 End与Cancel | PlayerNavigationRecoveryTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC20 活动战斗门 | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC21 演出token | PlayerNavigationPresentationTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC22 S17/028 | PlayerNavigationPresentationTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC23 明确迁移 | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC24 错误/信任 | PlayerNavigationSessionTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC25 通知失败 | PlayerNavigationRecoveryTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC26 UI Adapter | PlayerNavigationPresentationTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC27 预算/纯转移 | PlayerNavigationPermanentFlowTests.cs, PlayerNavigationPresentationTests.cs | 通过所列EditMode/静态范围，保留原证据分级 |
| CC28 C1继承 | named-tests-after/old3872 | 通过所列EditMode/静态范围，保留原证据分级 |
| CC29 宿主/Android边界 | PlayerNavigationPresentationTests.cs | Host交接已核，Android/物理宿主待028/029 |
| CC30 交付/有限证据 | scope-audit/Compile/XML | 通过所列EditMode/静态范围，保留原证据分级 |

RCV1：丢弃请求后完整重建，从明确Observed commit恢复同一不可变原intent；覆盖SaveFailed/Unknown/Retry/Resolve/End，错误身份/线程/Disposed/Busy/F2拒绝，internal只读桥不授新写权限。

| 平台项 | 证据结论 |
|---|---|
| MC01 | Raw UTF16 player identity,purpose separation,canonical local paths. |
| MC02 | Real Unity Mono/Python cooperative flock contention both directions,release/reacquire,retained zero-byte lock and non-contention errors. |
| MC03 | Real 00010203 publication,idempotence/different-byte refusal,CreateNew/no-replace conflicts and owned Flush(true). |
| MC04 | Ancestor/leaf/dangling symlink and non-missing I/O refusal; original physical fault cases retained. |
| MC05 | Nine signed old source adapters; all original NUnit declarations and3872 named occurrences retained. |
| MC06 | Twelve frozen source inputs/four original generators on Compile003/Unity Mono; four original golden lengths/SHAs matched before CreateNew restoration. |
| MC07 | 3956 passed=old3872+new84;zero failure/skip. |
| MC08 | 808 implementation/838 Assets/441 GUID/36 DLL-PDB exact final chain; original main156 plus signed extensions=240,platform64; separate no-follow I/O inventory. |

## 交付身份与剩余边界

- 主根 `TestArtifacts/FMDemoCONT/cont-c-mac-r1`：原156列表不改，有限补签后允许240、实际157；manifest排自身156。root SHA `b4dc15b859685ddac2b13255c8d8254685103708727ba3298cc8378fcd9999e5`；manifest SHA `5d31e0992e975d9b8f2034636523b258eccfa3921ad0bfaa81c2f4f9dd28b5dc`。
- 平台根 `TestArtifacts/FMDemoCONT/cont-c-mac-r1-platform`：有效64、实际64，manifest排自身63；原root冻结56路径不变。root SHA `9d3093fab31cb4f919ad9327071e69bf9a09f30038cebddbd5fb410d490d19b9`；manifest SHA `9a8b8d106e42d0687f723eadef3627c4a1c3c43ea93792a7c2326de5b350b500`。
- 平台read-manifest是历史003恢复输入账本，manifest是当前实际产物，io独列；完整两组路径、实际身份、未产生叶及三段回合映射见scope。
- §406 Finder元数据曾6148bytes/SHA087940810306d36853392f1e0d0ced48d88190bc50e4aa95b7038e15301ed750，按CreateNew/flush/双边核符仅移出源叶；原/private/tmp副本随后消失，按409记TEMP_COPY_NO_LONGER_AVAILABLE，未造替身或隐藏计数。
- 正式scope `docs/system-design/2026-09-17/demo-cont-c-mac-r1-code-scope.json`；自身SHA在正式返回外报，避免循环。

- NOT VERIFIED：Android APK build/install/run and iQOO Neo5 acceptance remain028/029; Mac is development only. Environment/emulator preparation is not gameplay acceptance.
- NOT VERIFIED：Physical mouse/touch/pixels,interactive PlayMode,scene/host wiring,device Back/lifecycle and first-Demo close/reopen are not claimed.
- NOT VERIFIED：Real published recipe→PlayerSave craft chain is unavailable in the first release; production Draft.Copy→isolated legal core and real NoPublishedDefinition refusal are separate evidence.
- NOT VERIFIED：Public AwaitLinks,real OS-process crash matrix,power-loss durability and arbitrary non-cooperating-writer blocking are not claimed.
- NOT VERIFIED：Fifteen historical paths remain absent. Original Finder metadata temporary copy is TEMP_COPY_NO_LONGER_AVAILABLE under409; historical preservation retained,no replacement fabricated.
- NOT VERIFIED：Host010 restart trigger,actual process exit codes/termination time and NUnit results are unknown; boot time is not substituted for process end.
- NOT VERIFIED：Independent R verdict and SD00 acceptance are pending; C does not self-sign ACCEPT.

SD00核准确最新turn `01a0e6d3-38f8-7b92-9c3c-6a3df6b258fe` 自然completed/error=null及formal COMPLETED后交现R独立审查；当前仅CONT-C作者交付完成，首Demo安卓APK目标未完成。
