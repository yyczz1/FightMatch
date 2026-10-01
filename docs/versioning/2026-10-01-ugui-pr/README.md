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
