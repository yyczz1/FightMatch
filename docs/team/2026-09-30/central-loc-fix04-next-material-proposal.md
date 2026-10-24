# FIX04 后续材料执行提案

2026-10-11。状态：M06已获后续用户明确授权，具体执行与身份按 `central-loc-m17-m06-execute.md`；准确源码的 GitHub 审查已完成（结果6105581700，reviewed head816263eaac）；中央 `loc-m17-fix04-source-verdict.json` 已限定源码ACCEPT。M05 已消耗其一次执行许可并失败，不能自动复用；本提案不是执行指令。

候选为 PR2 head `816263eaacf4f3e276c0cef7703c5824af086cc6`，FIX04 driver 132552B / SHA256 `8dd4d5daa8b46f2e1a2fdb076636a095e9c0b398a5f8ae9aff83ced8c3f8db6c`，source-freeze 2808B / `9b5ea95beb2f148f6c3f99a55aff33df5bf3d192aa66403f7f7ba7d15213b7c8`，model `fefbc7a1002760a2231922069bd839864084421f888576e99a10518c5b7c6605`。新实现只在完整校验身份后处理 nupkg 内部重复分隔符，危险路径、类型和碰撞仍拒绝；66项定向验证一次通过1.481335秒，机械生成封签一次不到4秒。

拟执行动作：

- 原 LOC 会话作为唯一执行者，中央唯一读回结果；不依赖作者向别的会话发信。
- 再次核对并只读复用 M05 已取得的五包，共16158115B，原文件及失败证据不修改。复用输入缺失或身份错误则停止，不自动改成重新下载。
- 获取剩余13个固定官方归档，正文总量9014649B（约9MB），精确URL/字节/摘要取 FIX04 `authority/source-receipt.json` 的 `MOnlyBudget.officialGets`，无镜像、重定向或重试。
- 完成原定三次依赖还原：runner generation、tests generation、tests verification；还原使用固定本地feed，保持原网络限制。
- 整次材料阶段仍最多900秒，包含870秒工作及30秒收尾。完整输入/SDK核验进入同一计时，不在计时前另跑测试或全SDK扫描。
- 仅新外置工作根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix04` 和材料根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix04`；必须缺席，旧根全只读。
- 外层证据拟保存在 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m06/` 四叶：before.json、stdout.log、stderr.log、receipt.json，沿总16MiB/单日志4MiB上限。现在没有创建。
- 不进入工具构建B、文本生成L或Unity。失败保留实际错误与部分结果，停止，不自动再次尝试。

签发前须完成：准确head的GitHub审查及源码接收；用户对上述新一次M06明确授权；读取fresh实际LOC回合；中央仅按已审host模板填新A/H身份和预算。使用正常工具审批，不能因本提案存在而跳过审批。

材料成功仍只是候选；须核读真实下载/还原/manifest及收尾证据并独立接收。后续工具构建和正式文本生成另按既定阶段接续，不能把源码验证当成可试玩Demo。
