# FM-DEMO-026-G2 交付

状态：BLOCKED。024 已完成并冻结；G2 未开始实现。C 不自判 ACCEPT，也不将计划中的637项／345GUID写成实际完成。

准确实施 turn：01a0cd85-45f2-7841-903c-20d443a6314a；任务01a0c403-bfa1-7e90-b503-c0fcd61f23c1；gpt-6-astra／max，local。
依据 §240–243；r121全文SHA256=01d0ce5e023a3d9bd961587a292bc8bc7a7dacd1db8b1f70b29323ba604d0e12。
冻结§240–243规范化SHA256=a5d9afc9841b504685372cb3672c60f562c4cf04fb72f72c9003a33b3eed618c。

## 具体范围冲突与所需决定

[§241 文件范围与 B16B-04/07](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/system-task-packets.md:4690) 要求在原 FightMatch.Core.Tests 程序集内放置真实 VisualElement／EditorWindow 测试宿主、发送真实 UI Toolkit 事件，并保持旧用例全 Passed。唯一获准旧文件是该测试程序集的 asmdef。
[现存编译引用断言](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs:175) 同时遍历 Core、Platform、Core.Tests，182–184行对 Core.Tests 仍禁止所有 UnityEngine／UnityEditor 引用。直接引用实际 UI Toolkit／EditorWindow 的测试宿主会触发该断言。

这是一项读源码可定位的范围冲突；没有运行 G2 编译或测试，因此不声称已观察到 G2 的运行失败。G1 手势源码没有此问题。
已通过异步问题请求精确例外：只为两项 Unity 引用谓词增加 name != "FightMatch.Core.Tests" 条件，Core／Platform 的纯引用约束保持，测试不删除／跳过。
[拟议窄补丁](D:/Unity/UnityProj/FightMatch/TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/late/g2-proposed-exception.patch) 仅修改断言两行（+2/-2），尚未应用。
缺少的是本项范围授权或修改后的冻结包；此前许可恢复、QFramework 单行例外和 Application 对 FlowPuzzle.Core 的引用例外均已生效，但不包含本次 Unity 引用断言变化。没有把未答复视为许可。

若获准，G2 相对611阶段为 Core.Tests.asmdef＋该断言两个旧修改，另609项保持；全组相对589入口为三个旧修改／586项保持，新增总48项后仍637。此处仅记提案，不记作已获准。
如果保持冻结范围，则 G2 保持 BLOCKED，024 可独立送 R 审查。无新任务、代理、Git写入或任务间通知。

## 已完成且保留的024阶段

| 项目 | 实际 |
| --- | --- |
| 范围 | 589→611，Architecture +3/-1，另外588旧项原字节保持；新22项。 |
| 文件/GUID | Scripts＋Tests597，Assets624，GUID331。 |
| 编译与测试 | compile-03实际exit0；tests-02实际exit0，3183/3183 Passed，0 Failed／Skipped。原3136＋新47保留。 |
| 实现canonical | e57a457441b5d0e3353ca0651f07bb569fa06d28bf7e7d68c77ebd034e11405d |
| XML SHA256 | 0219aec144aaa9ac560ff39a38cc2d02111f6499f6afcb3317be02c88cbc994f |
| 旧证据 | 207705项入口/出口元数据不变；沿旧字节审计，没有重拷／重哈希全部历史。 |
| 新归档 | 14939文件、862683489字节、66空目录；源/payload/ZIP路径、长度、SHA及空目录集合全部一致。 |
| ZIP SHA256 | d8844ce81792943d97d5e48f7091e48b97fef60c48ed8fd7cdad9e431b95adf2 |

024生产、测试和两报告已经冻结。G2 后续不得借本包白名单返修024，已通过的024验证无需单独重跑。获准续接时只补 G2 工作和其阶段规定的全量验证。

## G2 实际范围与验证

- before／after均611完整项，相同canonical；本阶段旧修改0、新建实现0、删除0。当前没有应用任何G2阶段覆盖。
- 26个计划新目标均不存在；Core.Tests.asmdef和旧引用断言保持024阶段原字节。
- 4份G1原来源长度／SHA已复核，匹配§241；尚未复制到Assets。原C2独立83/23/42结果继承，未重跑。
- G2源码0行，新测试0行；83 CaseIds尚未作为Unity独立用例运行。
- G2 compile、全量EditMode、真实panel事件、鼠标/触控阈值接线、rollback确认及PlayOriginal交接均 NOT RUN。
- B16B-01／02／03／04／05／06／07全部NOT RUN；04与07是明确冲突来源，其余未实施不作通过声明。
- 正式内容/PlayerSave、027事实播放、028完整结束/回退面板、029正式场景及完整Demo均NOT RUN；交互式Editor／PlayMode／物理输入未验证。
- 合法AwaitLinks公开成功链缺口和§184旧历史NOT VERIFIED继续保留。

scope完整列实际611阶段before／after、当前保护输入、GUID、DLL、预算、namedCases、validation、archive和NOT RUN；没有只写计划差异。
阻塞原证据：TestArtifacts/FMDemoB16/fb696314689c4ebeaba77001a08a9a9c/late/stage026g2-blocked.json；晚绑定：同根late/stage026g2-bindings.json。
024归档负责其冻结时的全部新证据；本阻塞报告、scope及late记录独立绑定，无额外G2测试产物或伪造G2 ZIP。

## 停止与交接

本批状态为024 COMPLETED／026-G2 BLOCKED。四份正式报告已提供供SD00/R收件；C不作独立裁决，不宣布整批放行。
结束本回合后停止源码与报告修改。准确 task_complete由SD00/R在formal final后主动核验；收到精确授权或新冻结包后，在原任务从024冻结状态续接G2。
