# FM-DEMO-018A · 固定 QFramework 最小模块接入
STATUS: COMPLETED
任务：01a0c403-bfa1-7e90-b503-c0fcd61f23c1；本次准确原实施turn：01a0c7e4-4bee-7363-afad-4475020babce。
仅交付018A阶段，等待独立R审查，不自判ACCEPT，不派018B。018应用办理／持久协议／写门尚未实现。
冻结包：system-task-packets.md r91 §177～182；派发SHA256：6ac773ab5118c45ef02dcce7739f4f22e97b63ac99a7673f7b2faec567fc5b4a。交付整理前逐字核对该六节无差异。

## CHANGED FILES
新增Assets恰15文件，旧文件修改为0：
- Assets/ThirdParty/QFramework/：QFramework.cs、QFramework.asmdef、link.xml及各自上游.meta，以及LICENSE.txt，共7份固定原字节。
- Unity生成6份meta：Assets/ThirdParty.meta、Assets/ThirdParty/QFramework.meta、Assets/ThirdParty/QFramework/LICENSE.txt.meta、Assets/Tests/EditMode/FightMatchQFramework.meta，以及下述两份手写文件各自.meta。
- Assets/Tests/EditMode/FightMatchQFramework/QFrameworkIntegrationTests.cs（258行）。
- Assets/Tests/EditMode/FightMatchQFramework/FightMatch.QFramework.Tests.asmdef（21行）；手写总279行，低于340行。
另新增本报告（≤180行）、demo-018a-scope.json（≤1536KiB）及唯一作者证据根TestArtifacts/FMDemoB14A/fec577db4655430491f17a2ea2352b81/，下称“证据根”。
Unity自然输出：Logs/FMDemoB14ACompile.log、Logs/FMDemoB14ATests.log、FMDemoB14A-EditMode.xml。
原回归测试自然创建且无损保留：
- TestArtifacts/FMDemoB12/72727a12bb4743fc9a75d624218b4e3d/
- TestArtifacts/FMDemoB13/4e9194f675c44c09864576da69552491/
未手写meta、移动旧文件、改Packages／锁／ProjectSettings／旧程序集／场景／资源／GLM或协调稿；未创建Git分支、worktree、提交、新任务或子代理。

## SOURCE
固定tag：v1.0.246-Unity2018Compatible；commit：64cb5397c3a4497d99f1c82a5730e7444ba3d992。
原件仅来自[固定Framework/Scripts](https://github.com/liangxiegame/QFramework/tree/64cb5397c3a4497d99f1c82a5730e7444ba3d992/QFramework.Unity2018%2B/Assets/QFramework/Framework/Scripts)及[根LICENSE](https://github.com/liangxiegame/QFramework/blob/64cb5397c3a4497d99f1c82a5730e7444ba3d992/LICENSE)。
七次固定raw请求均HTTP200；先在证据根downloads保存原响应，再核长度／SHA与GUID冲突，最后File.Copy入Assets。QFramework.asmdef的UTF8 BOM原样保留，LICENSE仅改本地文件名为LICENSE.txt。
| 文件 | 字节 | SHA256 |
| --- | ---: | --- |
| QFramework.cs | 28212 | 8ad2e7385a334b7be7da17a5f1c8da1bfd6ae2ef141ec658fb506c1e71421797 |
| QFramework.asmdef | 29 | f9c9d43d25712f15175b43570b487781d972256c740401ecc638e7bcac0a6604 |
| link.xml | 69 | e8c043d01d995c9cb54d728b10280d0a20223d2455060946357f0ebc10b2ed0d |
| QFramework.cs.meta | 243 | 1d43095bdab8127bf9889b586f2f45be6d5eea6e9f772b46639f7884bcfeeb69 |
| QFramework.asmdef.meta | 166 | c8e274d8c9e078cb906added52179ae36b2ffa23a6267e588088c78b02a5988d |
| link.xml.meta | 158 | 51f2a2b64bde61a0ff8aa96bb41fa4f7d7ff5fda7ad715d604f90c69641c0983 |
| LICENSE.txt | 1084 | 6a4cd471d0116635d6f11075d0688e331046665035f7afef99f5f5a3a98f10ec |
导入后以及最终编译／测试后七项仍完全相同。三个上游GUID保持，六份本地meta由Unity生成。未引入Toolkits、Examples、安装菜单动作、package.json、外部DLL、其他包或移动版本。

## ACCEPTANCE CRITERIA
| 项目 | 作者实证 |
| --- | --- |
| A01 | 七原件和MIT许可逐字节相同；原276meta保持，最终285GUID唯一。真实QFramework及测试程序集编译成功；旧Core／Platform／Core.Tests的asmdef保持，测试内反射及PEReader元数据均证实无QFramework／UnityEngine／UnityEditor引用。 |
| A02 | 通过真实Architecture.Interface／GetModel／GetSystem，重复获取同一实例且初始化各一次；故意先注册System仍观察Model先初始化；Deinit计数各一次，再获取新Architecture／Model／System，旧Value=99未沿用。 |
| A03 | 真实Command改测试Model、Event回调、Query读值，通知和结果在返回前可见。普通用例caller=1，三handler=1,1,1；顺序独占的托管线程caller=23，三handler=23,23,23，runner仍1；没有自动切回runner线程。 |
| A04 | 同handler重复注册逐次回调2→1→0，显式IUnRegister逐一注销；释放再建后旧订阅0次、新订阅1次。全部自有句柄与Architecture在finally清理，未触碰Global／OnRegisterPatch／Comparer。 |
| A05 | 最终同版编译exit0、全量EditMode exit0：2772／2772 Passed，原2767 Ordinal fullname多重集合和逐名Passed保持，新增5／5 Passed，零跳过。15新增／520实施／533Assets／285meta吻合，旧源与证据保持。 |
测试程序集固定Editor平台、references仅QFramework、noEngineReferences=true，其余配置严格沿§180。实际新测试DLL引用BCL、nunit.framework、QFramework，无引擎引用。
夹具用自身私有FixtureGate在SetUp／TearDown串行，工作线程只在一个用例内顺序独占自有Architecture；不与其他用例并发共享。测试类型全部位于FightMatch.Framework.Tests。

## VERIFICATION
所有下列时刻为2026-09-22 UTC；真实Process.ExitCode均在WaitForExit后先落盘，再采DLL／日志／源码。每次启动前检查无Unity，Hidden启动，无强杀或第二Unity并发。
| 运行 | 实际PID／StartTimeUTC | 捕获实际退出 |
| --- | --- | --- |
| 首次编译compile-01 | 35400／07:02:21.7876518 | 1，07:02:31.0147521落盘 |
| 最终编译compile-03 | 30980／07:06:17.3088861 | 0，07:06:45.7383落盘 |
| 全量测试tests-01 | 37144／07:07:59.3023538 | 0，07:09:01.2095落盘 |
编译实参：-batchmode -nographics -quit -projectPath D:\Unity\UnityProj\FightMatch -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14ACompile.log。
测试实参：-batchmode -nographics -projectPath D:\Unity\UnityProj\FightMatch -runTests -testPlatform EditMode -testResults D:\Unity\UnityProj\FightMatch\FMDemoB14A-EditMode.xml -logFile D:\Unity\UnityProj\FightMatch\Logs\FMDemoB14ATests.log；没有-quit。
exe均为D:\Unity\UnityClient\2022.3.18f1\Editor\Unity.exe。完整命令／cwd／原生stdout／stderr／真实UTC／wrapper退出均留在准确原turn和证据根命令记录。
最终编译和测试前后均为同一520实施规范SHA：fc9c5b4e911327e625841070d26c70d49b740398bb818badcd0a20cdeab368e6；28份DLL/PDB逐文件SHA亦完全相同。
QFramework.dll SHA：bf1866fec406768e1e5396392d51f0646092b711fa28f6e6a50a5439ce65a8ef；FightMatch.QFramework.Tests.dll：eecdfd8ef2f62ecfaa075f6fba1851a4dbabe2dfe9b3400a50ae318fee3ae11d。
原基线FMDemoB13C1-EditMode.xml SHA仍为3d4f6baf02ac0ba2a8cc6bb5e0b6439733ee2167e9a6007279fa0ecd16bf73ec。tests-01/regression.json保存逐项比较和新例XML原output；compiled-assembly-references.json保存实际DLL引用。
继承017原C1 after完整501项、capturedAtUtc=2026-09-22T05:00:14.9731359Z，implementation完整505项且不补造capturedAtUtc，原finalVerifiedAtUtc=2026-09-22T05:00:18.085419Z；本次startedSnapshot另记实际时刻，沿C2独立ACCEPT作接收依据。
静态范围：旧505、旧完整Assets518、原276meta、185个受保护输入和15个旧根8822文件均核SHA保持；当前Scripts＋Tests子集506、实施520、完整Assets533分别统计。只读git status/diff/diff --check亦运行；新文件按实际全集核，不以Git忽略规则定义范围。

## FAILURE PRESERVATION
首次编译因当前Unity附带NUnit无NonParallelizable特性产生CS0246；只改本包测试，以私有夹具锁替代。失败前514项（六份本地meta尚未生成）、失败后520项、原日志、stdout／stderr、PID及实际exit1完整保全。
该日志在进程退出后追加68字节缓存统计；compile-02的保全guard因此退出，尚未启动Unity。原前缀及完整后续字节分别保留，post-exit-log-tail.json核前缀完全相同；随后先保全完整旧日志再运行compile-03。最终运行另等待日志稳定并保存Unity-final.log。
另外两个实际exit1为CIM进程查询被拒（改用Get-Process）及只读诊断末尾Get-Process无匹配导致shell状态1；均非隐藏Unity运行。截至验证整理共四条非零命令，完整错误和真实退出都保留。
交付整理首次scope为1606659字节，超限guard实际exit1；随后将本次startedSnapshot改为带解码长度／SHA的无损压缩记录，原501／505继承字段不动。超限原字节和两次真实命令结果保存在scope-creation-tool-record.json。
辅助摘要曾因OrderedDictionary的Select-Object投影显示null，落盘下载／归档JSON实际字段完整；后续直接读JSON复核。XML output最初被转成类型名，原投影另存regression-initial-output-projection.json，后从未变XML的InnerText准确补正，未改变测试结果或历史时间。

## ARCHIVE / SELF-CHECK
实际无忽略枚举三根（本证据根＋两回归自然新根），另加当前520实施文件、三份最终日志/XML及原基线XML，规范映射payload/<原项目相对路径>。
实际来源与zip均5360项，共48785978原字节；独立校验重新枚举原目录和自然根，另读实际ZipArchive entries，对每条完整解压流核长度／SHA。
来源／zip规范SHA同为7ba99e8dd5ec728e5fd4340411869e87585ef396ec67c56e98346b47be381b80；缺项／额外项／字节差异／重复／大小写冲突／路径逃逸均0，07:15:58.2454063完成独立核验。
证据根/all-validation-products.zip为12462684字节，SHA=73c6b8b65efbe2bbeeca523cf3fd01cfe58c862ce58fd40cf0c4d2469d7f9ba0。
archive-source-policy.json逐名列9个自引用／后生成元数据排除；这些文件仍交付且可追溯，不以.tmp／.csproj等后缀过滤。两报告是外部交付元数据。源码、原下载、全部失败、最终日志/XML/DLL、1055项新回归自然产物均归档。
command-events-through-validation.jsonl保留20条截至验证整理的完整原生命令；后续归档、整理、最终门核在final-command-events.jsonl和scope-creation／final-gate工具原请求与返回中补齐，不用显示chunk_id冒充原CommandExecution。
本次未直接运行.NET存档工具或强杀矩阵；未接入玩家流程、修改领域格式或增加业务写入。

## NOT RUN
应用办理持久队列／事务／OperationId幂等／主线程切换、018B/C、实际Domain Reload开关、GameObject自动注销、场景卸载、正式应用写门／保存／UI／Player构建／Android／IL2CPP／HybridCLR均NOT RUN。link.xml随包不等于Player裁剪验证。QFramework同步调用测试不证明M02业务保证。

## PATCH / BLOCKER
补丁仅上述15个新Assets文件、两报告及授权自然验证证据；没有旧源码差异。无执行阻塞。
正式final后结束本次准确原018A turn并停改；独立R须待该turn completed后复核。通知由SD00读取final，不自行扩大阶段或作ACCEPT裁决。
