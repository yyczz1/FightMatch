# FM-DEMO-006 · 固定测试资料、来源与修订隔离交付

STATUS：COMPLETED — 实现与本包验证完成，等待 DEMO-B01-R1 独立代码审查；作者不作独立 ACCEPT 结论。
执行时间：2026-09-21（UTC+8）；按现行任务包§77～79，只执行§78，未开始007或其他包。

## CHANGED FILES

| 实施文件 | 改动 | 最终SHA256 |
| --- | --- | --- |
| [DemoContentFixture.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs) | 新建，测试程序集internal类型 | 07104C559950D47EE4FF6EBAA89321033E51A83AA60BA118A7FC6AC18409E213 |
| [DemoContentFixture.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs.meta) | 新建，Unity自动导入 | 31DA9C5AB02210DE3BB63F39012A32A17ACCA106C6746FAE00077DAC75E7E0E0 |
| [DemoContentFixtureTests.cs](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs) | 新建，测试程序集internal类型 | EB4CDCAF491BE99F85CA5433510A54259E6B559BCD64D7E2AD16DCAF84858CB7 |
| [DemoContentFixtureTests.cs.meta](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs.meta) | 新建，Unity自动导入 | 8EE68CE58C66013F04F4A28BC32B8FFF64C5C14C21423900FA8A74F8BF9BA43D |
| [FightMatch.Core.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef) | 只在references追加FlowPuzzle.Validation | 829ACDB13AC53136202AAF85B7E9CC5572085E720BB78F149A649AE63DFD66E5 |
| [demo-006-delivery.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-17/demo-006-delivery.md) | 新建本交付文档，≤120行 | 自身摘要随最终回复给出 |

实施预算：新建代码280+407行、meta各11行，asmdef新增2/删除1行；合计新增711/删除1，增删712≤900。交付文档另计。

## 起始与交回范围证据

本任务：01a0bebd-edcc-7c72-8483-fa11c96874be；本次turn：01a0c1cd-3ec2-75c3-904c-b9adaccc3b7d。
2026-09-21 10:33:36起始工具输出保留OriginalAsmdef全文、三份指定输入SHA及8路径不存在证据：4个新实施文件、本报告和本包3个运行产物均False。
同turn的“FM-DEMO-006 BASELINE SHA256 1/3、2/3、3/3”工具输出完整保存305份Assets/Scripts与Assets/Tests既有路径/摘要，不把历史未跟踪文件计入本包新增。
10:45:56交回工具输出对309份现有文件重新取SHA：304份原文件逐字节相同、只替换上述1份asmdef摘要、新增上表4份、删除0。该基线加所列5项增量为无损的最终范围清单；所有实施输入在最后测试后未修改。
全Assets路径由322到326，恰新增本包4份；git status新增本包XML，原tracked name-only/stat不变（既有34文件、+2809/−597）。目标目录及asmdef原已未跟踪，因此另按基线全文和SHA重建本包差异。
两个新GUID分别125fcccc8d3ddd942947493dc2e82d50、50e464d985fc4944ba2aa90f0d6d18ff，均由本次Unity导入生成，rg扫描全Assets各出现一次；旧meta全部保持。

以下10份只读输入开始与结束SHA一致：

| 输入 | SHA256 |
| --- | --- |
| [manifest.json](D:/Unity/UnityProj/FightMatch/Packages/manifest.json) | E15E302B5C4342D530AE52F31C785A9626B4FF42ECC1AA7612FCC3F0637B872A |
| [packages-lock.json](D:/Unity/UnityProj/FightMatch/Packages/packages-lock.json) | 160073F2CD54A18FC4A3995C64B53DE964356C5F36B66020FB66EB165E01C31A |
| [ProjectVersion.txt](D:/Unity/UnityProj/FightMatch/ProjectSettings/ProjectVersion.txt) | 9B7F178DD8C050E64943DB5709939F39FD3191C18C6C557143963975891B50E2 |
| [content-validation.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/content-validation.md) | B430F3384AEB20BF5742BF29C5949AF13CF25EB4EAEA31BEDBBAC5DA8BEEAFC9 |
| [configuration.md](D:/Unity/UnityProj/FightMatch/docs/system-design/2026-09-16/details/configuration.md) | 1AB07F8E5AFB1D2FDFC4A4A2CF5E436E8F876F1E4371A94A68CA9BFB25C4794B |
| [README.md](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/README.md) | 8216072DCBD0904B14E3A412AD73EB050CFC5534C5B8CF31286DACDF7D1D5548 |
| [chapter_model.py](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/chapter_model.py) | 2052792CCC3AAE007F6A23A2F799D4D724C3ECFDE97644D9731E4D706BBEC390 |
| [boards.json](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/boards.json) | 671467CAF72C52EB83A03B396AD82F5557A47B2F9CBD3F9DA6D0EC560A7D2AAD |
| [stages.csv](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/stages.csv) | 814536D3B9DE969F0F85673985E9B188857166C83967BF67AF74EAA6E3A9DB2D |
| [enemies.csv](D:/Unity/UnityProj/FightMatch/docs/game-design/balance/results/enemies.csv) | 238493BEC0BD293B12D0CFDB0395D4246FBB379E59573EC94A590E5DC25EC9D9 |

## ACCEPTANCE CRITERIA

| 项目 | 作者自检与实际证据 |
| --- | --- |
| ① 来源/映射/坐标 | [DemoContentFixtureTests.cs:41](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:41)的4个Source_L1/L3_BottomLeft/TopLeft用例逐点断言原始路线、转换后路线/端点、源路径/SHA/定位、A/B→color0/1→E01/E02及slot0/1；连续Copy不二次转换。[DemoContentFixtureTests.cs:216](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:216)缺方向明确拒绝。 |
| ② 独立构造 | [DemoContentFixtureTests.cs:115](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:115)以零基CV05输入捕获，标记Constructed、转换identity，源文件/SHA/源方向为null（不适用）；单步竖线合法，不生成玩家开放或旧经验事实。 |
| ③ 深隔离 | [DemoContentFixtureTests.cs:137](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:137)改原level全部字段、pairs及端点、solution及paths/cells、bindings列表，再改返回副本，两个捕获和旧证据均保持；[DemoContentFixtureTests.cs:170](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:170)只读集合写入拒绝，FlowPos读取为值副本。 |
| ④ 修订/取消 | [DemoContentFixtureTests.cs:184](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:184)核r8成功→r9不同几何→r10恢复原字节，旧证据不能用于新捕获；[DemoContentFixtureTests.cs:200](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:200)再核同key/同revision/同字节仍是不同捕获，取消及null当前捕获也拒收。 |
| ⑤ 两个验证层次 | 4种完整源候选均只占6/16格且通过FlowSolutionValidator；[DemoContentFixtureTests.cs:98](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:98)的4种仅A路线单步通过、完整解MissingPath；CV05同样保留单步合法。[DemoContentFixture.cs:202](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixture.cs:202)实际调用既有完整验证器并复制完整结果字段到只读证据。 |
| ⑥ 范围/来源失败 | [DemoContentFixtureTests.cs:15](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:15)真实读取boards.json核固定SHA，变化明确要求审定来源更新；[DemoContentFixtureTests.cs:227](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:227)起覆盖非法stage/enum/revision/key、null和缺绑定；[DemoContentFixtureTests.cs:306](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/DemoContentFixtureTests.cs:306)让6类几何失败交原验证器诊断，未在捕获时替代几何判断。仅测试引用增加Validation，生产Core无改动。 |
| ⑦ 实际验证 | 下列编译及全量EditMode均真实运行exit0；41项新增全部通过，原632项全部通过。范围、GUID、源码及运行产物SHA可查。 |

## VERIFICATION

先核Unity.exe存在且Get-Process Unity无活动实例；未杀进程。两次运行均Start-Process -WindowStyle Hidden -PassThru并WaitForExit，串行使用已批准的沙盒外许可证访问；无失败重跑。

| 运行 | 时间（UTC+8） | 进程与结果 |
| --- | --- | --- |
| 编译/导入 | 2026-09-21 10:42:49.022～10:43:20.350 | PID27080，exit0；编译日志518～519行正常退出，error CS/Compilation failed/Unhandled Exception均0。 |
| 全量EditMode | 2026-09-21 10:44:19.853～10:44:30.166 | PID4968，exit0；XML时间02:44:27Z～02:44:29Z，673/673 Passed，failed/skipped/inconclusive均0；测试日志337行保存本包XML，同三类错误均0。 |

实际参数（两次均由上述Start-Process传入；测试没有-quit）：
```powershell
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath 'D:\Unity\UnityProj\FightMatch' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo006Compile.log'
& 'D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Unity\UnityProj\FightMatch' -runTests -testPlatform EditMode -testResults 'D:\Unity\UnityProj\FightMatch\FMDemo006-EditMode.xml' -logFile 'D:\Unity\UnityProj\FightMatch\Logs\FMDemo006Tests.log'
```

已解析每个test-case：DemoContentFixtureTests新增41；原BattleRoute42、Pcg32Core27、ExactRational56、ExactRandomSampler35、ExactScoreCalculator32及FlowPuzzle440，原集合共632；各组NotPassed均0。本turn工具输出另列41个新增用例名称与Passed结果。

| 工具自然输出 | SHA256 |
| --- | --- |
| [FMDemo006Compile.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Compile.log) | D8183A6DADBA56C426BD168EC15A20AB1EC6CC971435311C570BEAFF994971B1 |
| [FMDemo006Tests.log](D:/Unity/UnityProj/FightMatch/Logs/FMDemo006Tests.log) | ADFD7C45D0E36E5F61EB93A0F92508297466EF1CD847152436C250A2D4FB58FC |
| [FMDemo006-EditMode.xml](D:/Unity/UnityProj/FightMatch/FMDemo006-EditMode.xml) | C48ED5AC221C8EFFDCF14A29CAA9F662137A341C653628E4305A181D379A9C49 |

## PATCH

修改前[FightMatch.Core.Tests.asmdef](D:/Unity/UnityProj/FightMatch/Assets/Tests/EditMode/FightMatch/FightMatch.Core.Tests.asmdef)原文（LF，末尾有换行），SHA256=836B9DE4FE2AAD84DED6116D2F46CDB81BFDC5406D440940694CC7A2FCFE524A：
```json
{
    "name": "FightMatch.Core.Tests",
    "rootNamespace": "FightMatch.Core.Tests",
    "references": [
        "FightMatch.Core",
        "FlowPuzzle.Core"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true,
    "optionalUnityReferences": [
        "TestAssemblies"
    ]
}
```
唯一差异是在原FlowPuzzle.Core字符串后加逗号，再追加FlowPuzzle.Validation字符串；其余字节与原文替换式比较完全一致。新建文件的完整正文即上表链接所指补丁；无额外patch文件或Git写入。

## SELF-CHECK

git status/diff --stat/--name-only/--check均已运行，tracked diff --check exit0；两个未跟踪C#分别与NUL作diff --check，exit1仅表示新增差异，空白错误输出为空。新代码无尾空白/冲突标记，原meta保持。
NOT RUN：Player/Android、正式内容发布、战斗/奖励、UI及H10保存均超出§78；夹具保留SourceCandidate坐标假设及Constructed零基语义，不宣称正式源方向已确认。无新增依赖、生产API、玩家写入句柄或权限文件修改。
收口：本报告111行≤120，32个本地链接及引用行号有效，尾空白/冲突标记均0；原asmdef全文逐字节保留。再次核309份实施输入与3份运行产物SHA均保持最后验证版本。停在006，等待独立任务审查；无BLOCKER。
