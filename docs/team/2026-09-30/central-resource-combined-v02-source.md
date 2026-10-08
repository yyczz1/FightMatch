# RES-COMBINED-V02：只修诊断环境与CacheWrite分类

2026-10-08，中央批准实施本文范围，尚不允许native。用户已授权持续开发及限定验证，普通技术修正由团队处理。唯一源码负责人01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local，Astra/xhigh，fresh actual；中央01a0e401-511d-79f2-b47f-3ab0ade1681b/local唯一完成接收者，不发其它会话消息。

## 输入与目的

读engineering-resource-combined-v02-correction.md（6895B/SHA8d92cf419c9e68e69b0bc2f80acad3a25240749545d20c18e18edd18da6bd007），本包正式采用其**禁用诊断环境项方案**，不采用已撤回IPC放行/账本方案。基线V01/run runner81937B/SHA1a30b4c6e1a5807664d047635c4fc20b131fe9d15191b128656dc366612aa7ba及四个相邻输入，PR20 head5f43b7a8已有源审查。原native失败、QA及中央NEEDS_FIX见resource-combined-v01-native-verdict.json。原I进程exit0但完整编译绑定未形成，T89从未运行；不追认原PASS。

## 唯一允许变更

只新建TestArtifacts/FightMatch/RES-COMBINED-V02/run下runner.py、replay-check.py、replay-results.json、inputs.json、preparation.json。所有V01、旧TMP/BC/AS、源/缓存/包、元数据和既存回执只读，不覆盖旧文件。新候选代码仅三类差量：

1. 绑定V02任务与新E/BC/AS/TMP前缀，数据按方案只改当前路径、执行环境、身份与新seals。source/reference历史路径不能全局替换。保留1036/1035/1037输入、43compiler叶、53包与89case逐项身份，列出全部input字段差异供中央机械核对；原fixedReferences的来源不重写。
2. I/T实际Popen的环境均设置DOTNET_EnableDiagnostics为字符串0，override父环境，activation/env门同步精确验证。仅改变本轮子进程环境，不改全局用户环境/二进制/runtimeconfig。使用方案的.NET6/7已知身份，必要读取也只限这三个固定lib/config，不运行dotnet或Unity。
3. 修compile_evidence实际动作与CacheWrite遥测分类，保持真实双Csc/缓存读取/未知格式拒绝。依据V01/I/editor.log真实片段，不能靠简单去重掩盖动作。其余已审守卫、PID信号权限、tree_entries、恢复及绝对期限保持。

不得新增IPC类型豁免/账本、换监护框架、改游戏源码或放宽测试断言。若必要变更超出三类，先返回具体缺口。

## 验证与交付

只做方案列出的实际函数离线回放，原158项期待保留（必须变动的V02路径/任务标签单列映射，非断言放宽），新增最小环境传递及CacheWrite案例。最多两轮累计30秒：先必要针对性红，再完整绿；首过即止，失败不擅自加轮。新包准备机械总计≤160秒，记录实际计时边界和保守上限；沿900总native设计不增。禁止Unity、真实ps/信号、P/K/BC/AS写、下载、Git、activation及任何native。

preparation记录五文件seals所能非递归封存的部分、完整基线差量、函数/输入变更清单、已有断言保留、两轮实际结果、source-owner actual及本包依据。新源码未审，status仅SOURCE_REPLAY_PASS，不能自填集成ACCEPT。输出每条≤3KB；不要打印大inputs/preparation以免上下文再次被历史问题带偏。中央发布对应head并接GitHub审查后，另签C首次V02限定执行。
