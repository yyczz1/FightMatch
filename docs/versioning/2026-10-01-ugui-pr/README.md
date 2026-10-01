# uGUI GitHub PR 审查检查点 · 2026-10-01

本检查点用于独立代码审查，尚未通过完整验证，不是首Demo完成或发布版本。

- 基线：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；审查分支：`codex/ugui-migration-review`。
- 用户新规：代码审查转 GitHub PR Code Review；停止新增本地R代码审查，自动化测试继续优先dots。
- 源码范围：53个源码／测试路径，加37个场景／meta／TMP／字体／预制体路径；全部与FIX17原始manifest逐字节一致。
- 迁移运行时页面、棋盘输入、播放和宿主到uGUI，保留现有Application／存档规则。
- 加入项目本地化绑定、英／简中文案和预制体诊断占位符；最终Luban制作链、语言偏好持久化仍有后续工作。
- FIX15修复已激活页面绑定时的结构刷新；定向2项通过，证据保留。
- FIX17仅为H01／H04测试增加PlayMode生命周期，原业务断言保留；Host30为30/30。
- Core190在420.0346秒超时，无XML；等待60秒后核准进程身份，单次SIGTERM收尾，exit -15，专属进程树清空。
- 222项并集、最终原生复开、布局、API24／GLES3、新APK、设备玩法均未完成，不以PR无发现替代。
- 未混入独立LOC的Config／Generated／Tools实现、未选美术、缓存、APK、权限文件或真实存档。
- `source-manifest.json`与`resources-manifest.json`列出精确路径、字节数及SHA-256；产品单独身份见`product-only-manifest.json`。
- 本目录JSON／XML是对应TestArtifacts原件的字节相同选集；完整日志仍保留外置盘，未重跑测试。
- PR结果须绑定实际head及GitHub返回链接；通过、失败和未执行项目分别记录。

设计依据：[运行时uGUI纠正](../../system-design/2026-09-17/runtime-ui-ugui-correction.md)、[系统设计](../../team/2026-09-30/engineering-system-001.md)、[实施合同](../../team/2026-09-30/engineering-ugui-01-implementation.md)、[FIX15](../../team/2026-09-30/engineering-ugui-01-q4-active-bind-fix-15.md)、[FIX17](../../team/2026-09-30/engineering-ugui-01-q4-host-playmode-fix-17.md)。旧包本地R路由由[新执行规则](../../../.agent/TEAM_WORKFLOW.md#7-github-pr-code-review)覆盖。

实际上传回执：通过已连接GitHub API创建blob/tree/commit并正常快进审查分支，head `656a94e4feda942b575b9429921fb4d0b3fdce52`，tree `86213610883374f437c34560a215326f3ee69cb8` 与本机独立index一致。已创建并附加[草稿PR #1](https://github.com/yyczz1/FightMatch/pull/1)；[审查请求](https://github.com/yyczz1/FightMatch/pull/1#issuecomment-5926609717)已由机器人确认Running。CLI直连网络/登录未成功，未声称执行过成功的git push命令，也未移动master或本机HEAD。

FIX18 实际编译在107行报CS0234，Unity27.715s退出1，未执行测试；外层监视因残留编译服务器继续至601.043s。此失败保留在 [原回执](fix18/final-receipt.json)，不能被此前GitHub“未发现重大问题”的审查代替。旧PID62357在FIX19开始时已不存在。

FIX19仅将Application限定到UnityEngine并绑定新诊断输出目录；原90源码／资源、meta及190 selector保持。新的 [runner](fix19/validation-tools/runner.py)在首个编译失败或非零退出时转有限收尾，保留原失败原因与独立cleanup状态；[17项离线伪进程检查](fix19/static-result.json)通过，不代表Unity编译／190项通过。新代码head须独立审查；正式运行仍是一次600s Core190，复用exact2与Host30证据。

FIX19已实际完成190项：184过、6败，261.086s退出2、268.505s进程清空。五项为geometry unavailable，另一项是mesh.vertexCount为0（不是rect宽度）。完整原[XML](fix19/tests.xml)及[回执](fix19/final-receipt.json)保留失败；当前源码head302a95b的GitHub审查未发现重大问题，仍不能代替实际测试。

FIX20仅修改三个测试文件：六项使用真实PlayMode生命周期与只读几何探针，两个类增加失败后退出保护，共享rig、产品及资源保持。17项离线检查通过；尚未运行Unity。计划只验证相关12项、复用FIX19其余178绿项及Host30/exact2，形成222项Counter；12未通过前不能声称并集完成。`fix20/review-manifest.json`明确是完整本地manifest的精简投影，其余原始选集保持字节相同。原生复开、实际场景几何、布局、APK与设备门仍未完成。

## FIX20 实测与 FIX21 绑定修正

- FIX20 后续实际12/12通过，Unity75.019s自然exit0、82.391s专属进程清空；[原回执](fix20-result/final-receipt.json)、[XML](fix20-result/tests.xml)、[222覆盖](fix20-result/grouped-exact222-coverage.json)与[26事件](fix20-result/test-events.jsonl)均按原字节保存。222是保留178＋新12＋Host30＋exact2的精确并集，非同一次222运行；上文等待文字与旧失败保留为当时事实。
- GitHub对head `36ad62c4c71f4b987efaf60f26c942a0a9de10bd`提出[P1：审查head未与实际执行源码绑定](https://github.com/yyczz1/FightMatch/pull/1#discussion_r4153777291)。旧runner只检查SHA形式，测试通过不能关闭此问题。
- [FIX21 runner](fix21/validation-tools/runner.py)读取并重算真实commit/tree/blob身份，核完整Unity导入集合及辅助输入摘要链；仅允许明确证明不参与执行的8项独立LOC WIP。延后辅助数据解析，并在启动前及运行后复核；保留原selector、360s限额、首败及专属进程收尾。
- [35项离线检查](fix21/offline-result.json)通过，覆盖错误head/tree、集合和字节漂移、辅助依赖断链、Git重定向、授权撤回及启动前变化；只使用内存fixture与假启动计数，真实Unity/dotnet均为0。初次补证发现CSV未有独立Git blob，改由实际受审测试代码中的固定path/bytes/SHA常量锚定；原失败与两次离线记录保留。
- [事后补证](fix21/posthoc-binding.json)将旧FIX20证据关联真实H20→T20，核2296输入、998受审保护项及8项明确排除；原始1006项before/after不变。它不声称原runner当时已有启动前Git门，也不是新的Unity运行。
- [真实commit原字节](fix21/commit20.raw)由GitHub API字段恢复，仅在重算Git SHA恰等于H20后导入；[来源内容](fix21/git-commit20-api.json)与[中央导入回执](fix21/commit20-object-import.json)保留。API JSON是连接器返回内容，不冒称HTTP wire字节；本机HEAD、master及index均未改。
- [FIX21交付](fix21/final-receipt.json)的源码／离线／补证已冻结；新head自绑定与GitHub独立审查由中央在发布后记录，不向本提交自身嵌入未来SHA。P1及集成门在新审查完成前保持未关闭。
- 实际Host仍缺正式本地化源接入；原生场景保存／复开、真实画面、布局、Android构建与设备流程尚未验收。FIX21不新增产品代码或资源，不以文件存在或测试rig通过代替这些门。

## FIX22 进度路径修正

- GitHub对FIX21 head `755409ecc8f513c9547911af470c0ba307b63bfc`提出[P1：新输出目录与callback路径不一致](https://github.com/yyczz1/FightMatch/pull/1#discussion_r4154220400)。旧35项未进入真实执行入口，不能证明路径可用；原结果保留。
- 本次只改一个既有C# callback，将完整进度路径固定为新的`q4-correction-22/test-events.jsonl`，拒绝归一化别名；runner、contract、activation和最终Unity参数核同一路径。新[执行增量](fix22/execution-delta.json)只替换callback身份，历史FIX20及产品／资源不变。
- [离线记录](fix22/offline-result.json)两轮均78/78通过，包含原35项；新增正例经过实际执行入口、preflight、候选校验及参数生成后到最终假spawn，反例均零启动。第二轮仅为修正contract中陈旧归档说明后复核；两轮合计约9.493秒，未运行Unity。
- [源码交付](fix22/source-receipt.json)、六份同字节归档和[限定合同](../../team/2026-09-30/engineering-ugui-progress-path-fix-22.md)固定后提交新head审查。审查关闭阻塞后仍需单独激活一次exact12（≤360s），验证真实C#注册和跨域26事件；当前不称该门通过，不重跑旧长测试。
