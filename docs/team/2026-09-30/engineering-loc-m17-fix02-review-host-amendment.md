# M17-FIX02：独立P2与最小H模板补签

2026-10-04 · 只补同一活动作者turn `01a106e6-15b0-75a3-a1dd-b5b3108744d8`；中央已明确批准合入GitHub唯一新P2，并要求H逻辑也经同PR代码门。本补签优先于[timer FIX02短包](engineering-loc-m17-timer-shadow-fix02.md)的单点/100行范围；其余禁止项、旧38冻结、owner/issuer/唯一返回保持。不是第二实施包/第二review，不运行材料。主程issuer仍 `01a106dc-0140-7783-9ec7-aab4235c4800`。

## 1. 同一driver的第二处纠正

QA `testing-loc-m17-fix01-pr2-final-review-receipt.json` SHA`4706acde56ac0df3cb25b29ccaa3e24aff74f6ea629d1369c52e5463534fa981`已由主程全文接回：head837dd4e39683548ee23dce5f5228074ec87231e5，review5406086277/COMMENTED/Completed，P2 r4177549312，driver1131。

`execute_material`须在**第一个GET和材料准备写入前**验证全部当前存在的official-get归档：所有现存17nupkg/1Yaml tar用原长度/hash/目录安全门；缺失归档保留为后续固定GET，不能使输入检查放行。增加一个明确preflight遍历，不删除下载后再验、不扩大归档/URL/网络/重试范围。旧existing完整输入核验仍先于外部动作，不给预检另开计时器；工作根既有官方归档规则保持。

定向harness走真实`execute_material`路径：前包缺失＋后缓存损坏→原源码red（不允许发生的GET只能由替身记录）/新源码0GET且0材料写；全部现存归档正确→验证事件全部先于第一个GET；已存在Yaml源码与nupkg均覆盖，mtime/文件存在不是hash替代。与stage覆盖共用本轮纯验证，不另跑旧73。driver相对E1增删上限调整为180行，仍只两处生产语义差量＋必要路径/身份/封签和定向harness；超限报实际最小需求，不自行重构。

## 2. 同PR审查的中央H模板

额外唯一新根H0=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m17-central-host-template-01`，首次ABSENT。只两叶：`source/central_host.py`和`source-receipt.json`，总≤256KiB；模板≤100非空行/16KiB。**H0独立于E2的八叶seal**，不要为了把H塞进E2改consume_source的八叶结构/自指关系。模板+回执与FIX02一起交中央同一PR/head一次独立review；源作者不写中央A或生产H。

模板只包含一个带唯一固定起止标记的字面量`CONFIG = None`数据块，其他逻辑固定。未填配置直接BLOCKED，不从CLI/env/stdin取pin/配置。不内置真实pin、当前/猜造权源或可执行默认值。

配置未来由中央一次填入：准确driver绝对路径/bytes/SHA、A绝对路径/bytes/SHA和七真实身份、准确允许argv；数据种类仅字典/列表/字符串/整数/布尔/null的Python字面量。固定逻辑只做配置键/类型与绝对规范路径校验、拒绝文件/祖先symlink、实际argv==CONFIG.argv、A和driver原字节长度/hash相等，然后从已核driver字节加载正常独立模块（原__file__、非main），核其生产pin原为None，仅初始化该模块`TRUSTED_CENTRAL_ACTIVATION`为已核中央pin，并调用其原`execute(list(sys.orig_argv))`。代码不得替换reader/计时/模型/验证函数、删源seal门、注入额外模块或执行任何材料动作；真实中央身份与reviewed head等仍交现有verify_activation严格验证。异常/退出码保留、不能默认成功；入口返回dict不直接作为sys.exit错误对象。

无环关系保持：最终已审driver/seal → 中央A → H仅数据区填A的bytes/SHA → 中央可信派发固定A/H两hash。A的hostExecutorSha256按现接口等driver，不等H；A不反向包含H最终摘要。中央填数据后须机械核：唯一CONFIG字面量可由ast.literal_eval读取，标记外字节完全等已review模板，移除该Assign.value后AST完全一致，实际DATA仅来自A/已审源/真实派发，不做表达式替换。审查逻辑与运行逻辑因此逐字相同；不能称当前CONFIG=None模板已获得真实执行权。

只做隔离内存回归：None拒绝且无文件读/执行、argv/driver/A摘要不符拒绝、有效synthetic配置仅初始化相应pin并调一次原execute接口、数据区机械替换不改逻辑。用明确synthetic模块或替身记录调用，不执行真实driver的M动作，不更改正式全局/文件pin，不自造生产中央消息。测试事实与未来真实授权严格分开。

## 3. 合并预算与返回

旧两个具体故障的red合计≤5秒；本轮全部定向（stage/P2/H）最多3轮累计≤30秒，准备/封签/实际consumer另≤30秒。已在当前turn发生的尝试计入，不重置；普通制作错误窗口内合并处理，所有原始结果保留。E2八叶≤16MiB、H0两叶≤256KiB、共享/工具/历史/中央文件0写；不重复旧73全套，除非明确证明最终封签无法隔离后先报必要原因。

一次返回E2八叶及H0两叶最终身份、两处生产差量、真实生产路径red/green与H门、数据填充机械规则、实际回合/命令/各口径耗时。结果仅SOURCE_READY/REVIEW_PENDING，生产pin null、M/W缺席，下载/材料/restore/Unity/Git0。主程收回后中央发布最终单head；所有代码包含H模板经GitHub后才可能签M。N04仍hold，与此无关。
