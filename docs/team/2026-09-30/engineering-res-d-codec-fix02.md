# RES-D-CODEC-FIX02 — raw物理叶独占

2026-10-03。中央已收完整GitHub单P2并批准最小纠正，用户授权AGENTS §6。owner原作者01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local、Astra/xhigh；issuer/唯一收件主程01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff8c-1b41-7262-a8be-362ff4870414。前FIX01 turn01a0ff6a-9d37-71d3-b76a-07ed863e265b已completed并封存；本合同只定义源码职责，实际激活绑定新turn，不授Unity或审查。

唯一finding：[r4171291210](https://github.com/yyczz1/FightMatch/pull/10#discussion_r4171291210)，review5398527691/full head bf3098717d3c2b0bd02eeac9c6c0562711c87b00，summary5964439974 Completed02:13:32.281080Z。QA receipt testing-resource-codec-pr10-review-receipt.json3209B/SHA f399095763d15c77a7d38275b49663a9b8eaa180d91a4559341946d11e1704fd为NEEDS_FIX；只修该P2，不新增本地review/旧候选诊断运行。原合同§4闭包只允许bundle多引用；raw跨逻辑槽共享physical name即使同length/hash且所有scope/父hash重算也必须拒绝。

前像取FIX01/S receipt10495B/SHA3aa9bc0e28746c8ef21a496b075381fa5f7fdd5930a8bb762741b184803c36e6及完整8叶。唯一两产品路径仍 Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs（31716B/680行/SHA2fff363f68195ba7e948e6b998b1ebfe140b12adbc7ab02ba2bebe6d3f69dc46）和 Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceReleaseSetTests.cs（53927B/684行/SHAfb5c5d866478d28a203e84908a5a7b8a66a7e478f0976e877dad1c5e979ef47d）。全部public API/schema/外部pin/上限/闭集/fixture字面量/其它校验保持。

生产仅在现有Validate闭包内增加以Ordinal物理name为键的私有局部raw所有权去重，第二个raw逻辑槽引用同一叶即Need失败RES_SCHEMA。现有used集合仍负责覆盖全部Physical；object引用bundle的多对多语义不变。不得把所有used.Add的重复统一拒绝，也不得按SHA去重或引入新API/helper/依赖。生产增删≤12行，范围仅局部集合与raw引用检查。

测试保留原12方法名称及各自完整正文/断言，FIX01 null cast、所有helper和V0/V1字面量逐字节保持。原D09开头已有两个不同object引用同一bundles/ui并Accept的正例，继续运行它，不复制重复bundle测试。仅新增普通[Test] D13_RawPhysicalLeavesHaveExclusiveOwners：用Build(true)构造11个Physical，使两个text raw槽有相同length/hash但不同physical name，连同Text.manifestSha256/entry/physical一致更新，再Rebind并Accept作为正控制；接着把manifest槽files改为artifact同一raw/text，删除不再引用的raw/text-manifest，保持两assetId/location不同、physical仍10且无孤儿，Rebind完整scope/mapping/descriptor/receipt，再以有效外pin拒绝RES_SCHEMA。显式核相同payload/两个独立槽与最终physical10，以排除hash错误、下限或孤儿导致的假阳性。复用既有独立测试序列化/哈希helper，不让生产encoder构造预期；不运行旧解码器证明红。测试新增≤80行，总增删≤100；原1200/1000物理行、80/100KiB及stage2MiB预算不变。候选精确13＝原12＋此1，行为仍待实跑。

新根仅 TestArtifacts/FightMatch/RES-D-CODEC-001/FIX02/S，起始ABSENT；8叶为source/下完整两源＋before.json、after.json、source.patch、verify.py、static-checks.json、source-receipt.json。before记录实际owner/issuer/本合同/审查与publication固定身份，并冻结D原S8、FIX01/S8、B FIX03/S15、C-CONTRACT/S8及FIX01既有34共享guard；after核同组。C receipt14107B/SHA bfca5fe3550f4f25198595de9bdc3368acbdac3122b704820ff365b8b6e5e2f2，原publication1538B/SHA cfaba1865e341206a2ccbfe4072f42589b73c17f3e4c648db6f0a061e61a21e6。不读或冻结活投影、正在封存的T-continuation、C原生；原S/FIX01/B/C及共享不写，meta/asmdef/packages/生成物不改。

apply_patch实施；补丁仅两路径delta并在内存重放FIX01前像。最多2次新标准库python3 -B FIX02/S/verify.py机械检查各≤30s，保留失败和原脚本身份；只查范围/预算/固定输入/patch/原12正文与helper保持/新增13名称及所需raw检查，不重跑旧verifier或复算未变fixture。C#/Unity/discovery/测试/Git/网络/代码review均0；超范围或预算不足即BLOCKED交主程，不压行或修改旧断言。

封存仅报SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING。依AGENTS §6一次send_message仅回主程/local/Astra-xhigh，交实际turn/receipt/patch/两源hash/13静态名称/旧12与bundle正例保持；不自批ACCEPT或接下一包。中央发布同PR10修正head并接GitHub；原finding不自行resolve。后继由唯一C另签D修正版13＋已封存C10的固定组合一次I/双类T，旧候选12若未开始就NOT_RUN，若已运行只属旧候选；不重复旧T，不把组合称单一PR-head/真实资源验证。原M01/M02/CLOSE证据和FIX04/M16拒绝保持。
