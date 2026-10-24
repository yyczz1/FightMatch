# ANDROID-API24-SOURCE-READINESS-001

2026-10-03 · READINESS_ONLY / NOT_IMPLEMENTED。用户AGENTS §6、中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local/01a0fd75-6e53-7b81-a7ce-9010190ae130授权只读收口；发单/唯一收件主程01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a100cb-8ae9-7fc3-b5e2-d1a4ca0f781e；SYS actual 01a0f2e6-29bb-7092-abf0-705b41b7bf93/local/01a100d0-b3f6-78b0-b8d3-bd3807ec85d2。
结论：可实施一个不依赖M16/Scene的两文件最小源码候选，但须主程补签“源码先行、原验证后置”及已审共享构建文件基线；按现行计划不能直接绕过UGUI串行合入前置，更不能仅改断言就称API24已实施/验收。
来源均为仓库相对path:line，SHA-256绑定本次有限输入：U=`AGENTS.md`:133、170–177 / `85c2c25b3de144f2b3444ab11652fef210efbee00d8ab01064f7866bc941b9dd`；S=`START_HERE.md`:29 / `ceebb38c3763f5c420525f25929cf4cbf46b48e82ac4cca56e9124f739d1f163`；Q=`docs/system-design/2026-09-17/system-task-packets.md`:8831 / `2128f94340f98a853b770727a8d5cfb8b934a1763ea6bec7be4f58bbe0b77dcf`。Q29已定Android7/API24，不重问，非引擎/SDK升级授权。
D=`docs/team/2026-09-30/engineering-android-api24-001.md`:13、25–36、42–95、107、134–182 / `5ed0c1d7eaab4d8e6ecaf7815afc30deacc9e58b05e0d6f6af38ed9aa91402f9`；既定计划允许准确两文件，禁止测试新增。原设计R门由U的GitHub PR Code Review新规覆盖，不恢复本地R。
B=`Assets/Scripts/FightMatch/Host/Editor/FightMatchAndroidBuild.cs` / `ebccb4d9cec5d5d9fffa025fac78c76829fc53a85370dedfee7dd30d93308ca5`；P=`ProjectSettings/ProjectSettings.asset` / `2ea02ff3b5acc6127f29d58049317eee4a52a84d9afde809eaf1efb30ed4ce1b`；与前一准入表B一致，但不是UGUI已审基线成立的证明。
前置线索=`docs/team/2026-09-30/engineering-res-built-in-admission-policy-001.md`:18 / `a7263f05de069e0ccb0c433374b8bbf66bf480eba0b09dc966cf1b9a9406b87b`，7969B；本轮以B/P实值重新核对。
测试只读来源：T=`Assets/Tests/Android/AndroidQaRun.cs`:38–55 / `f916ac4a7e63bef4f5df8e944d9563d8159903cf788892e47b7efb6275ffbd7a`；C=`Assets/Tests/Android/AndroidContentPlayerTests.cs`:14–43 / `d3f1ef5e64dd89d3f0b03d476d91e8bae481d847f098742683f2d03ec5ac20c0`；A=`Assets/Tests/Android/FightMatch.Android.Tests.asmdef`:4–10 / `513dd4001a9f093654d62ee1e6f3ffe29c3872af06a2e3ed9adda923cec39c8b`。

| 区别 | 当前事实 | 下一包准确授权申请／允许增量 |
| --- | --- | --- |
| 实际项目设置 | P:172为`AndroidMinSdkVersion: 22`，173的target为32；这是磁盘序列化现值，本轮没有读运行中Editor API。 | P恰一行`22→24`，其它字节不变；这是必要的受保护ProjectSettings授权，不能只改B。 |
| 构建检查，不是设置器 | B:1684–1685的`CheckProduction()`只Require min22/target32/code1/version0.1；QA入口B:42、normal入口B:1751共同调用。限定检索Assets/Scripts的minSdk相关项仅此断言，未发现minSdk赋值代码。 | B仅将min谓词换为`PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevel24`；同断言其它谓词逐字保留。不新增setter、自动纠正或新public入口。 |
| 报告与恢复，不是设置来源 | B:1863–1875的`WriteReport`及1890–1896的`BuildResultRecord`未记录minSdk；B:1926捕获设置原文、1853–1856恢复原文。 | B另增`int minSdk`一个字段和`minSdk = (int)PlayerSettings.Android.minSdkVersion`一次赋值，仅证据JSON增量；不改恢复流程。正确起点必须先是24，否则捕获/恢复旧设置不会替你落实24。 |
| 现有测试不是最低版本证据 | T只核Android运行平台、QA包名与阶段；C核jar内容/IL2CPP发布流；A限UNITY_ANDROID。限定检索Assets/Tests未见minSdk/API24/CheckProduction断言。 | 最小源码包不改/新增测试、asmdef或meta，也不通过反射启动带副作用的构建器来凑测试。新增测试不是本次必要范围；若以后要求运行时门测试，须另签精确测试文件与安全执行合同。 |

可观察SOURCE验收：P精确单行变化；B仅上述三个语义点（1谓词、1字段、1赋值），原QA/normal都仍经同一门；移除本门min22且target32/版本/ABI/签名/图形/BuildOptions/输出槽/保存恢复逻辑不变。二者必须同一候选交付，不能拆成仅断言补丁；结论至多SOURCE_READY、UNCOMPILED、UNTESTED、REVIEW_PENDING。
最小验证提案：下一源码授权内一次≤30秒的机械差分/哈希检查，核P替换前后唯一性、B三个定位点、非目标字节与既有两调用点保持；检查器在内存以漏改P、漏改B、target误改三种反例确认拒绝，不写工作树反例。验差分不是C#执行或SDK行为证明；检查器/证据叶由主程另绑，不运行旧029验证器。
后置而不删除D原门：唯一C获准后仍须Unity2022.3.18f1保存复开读取API24、单次compile；再按另签新输出合同各QA/normal构建一次，BuildReport和最终APK manifest同时为24。原D的Q2≤5min/Q3≤5min/Q4每包≤10min仅为既定计划限额，本次没有运行权；当前静态字节不能关闭这些门。独立GitHub审查绑定实际head，完整API24 ACCEPT仍待这些证据。
独立性/依赖：最低版本三语义点不消费M16文本、Scene、RES/AS/factory或业务状态；但B与UGUI共文件，D:25–30要求已审补丁稳定后合入，须主程冻结新SHA/责任并签SOURCE阶段；未满足时可保留候选、禁止共享覆盖。D:32–36旧构建槽不足问题只阻断后续构建，不借此泛化构建器或改第三产品文件。
明确排除：GLES3/targetSdk/Unity升级、SDK安装、Packages/依赖、其它ProjectSettings、Scene/prefab/meta、M16/AS/联合40、旧脚本/报告/APK、设备/商店/后台/部署、Git写入；本轮只新增本文，未派代码/C、未运行Unity/编译/测试/构建/网络。有限输入前后SHA终核一致；无产品完成或接收声明。
