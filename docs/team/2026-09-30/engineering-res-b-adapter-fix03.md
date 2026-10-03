# RES-B-ADAPTER-FIX03 — 全局初始化异常后的同provider重试

2026-10-03。中央已依AGENTS §6明确批准优先修正；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff0c-f3dd-7193-9357-e443efed3808`签发／唯一收件。原作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`（Astra/xhigh）直接源码实施，记录实际turn；D源码等本候选封存后才串行派，不改变B冻结输入。C仍唯一Unity执行者。

正式依据：[FIX02代码回执](testing-yooasset-adapter-fix02-pr9-review-receipt.json)6603B/SHA `8dc67678e3af9cda5e54618eeb4a6e4352ba8295d7d9acfa722809bdaaf84606`，review5397948629/fullhead `d8cc01056bfbc28391d5f9ac34a58e17f38ab111`、P2 r4170761716，lifecycle右233–235：scope先赋值，sdk.Initialize抛异常后注册／client尚未完成，同provider下次Acquire因非null跳过初始化。仅按服务已确定模式修正，不做重复本地review或猜测轮。[37原生门](testing-yooasset-adapter-fix02-m03-receipt.json)6056B/SHA `e6aa041b50741eac123acbafe2e1457d05ee2b2e52ec8f264839959c39ac1f26`限定ACCEPT保持，不能覆盖此P2。

沿[FIX02](engineering-res-b-adapter-fix02.md)范围与证据格式，唯一新根`TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX03/S`起始ABSENT：同9源＋before.json/after.json/source.patch/verify.py/static-checks.json/source-receipt.json，恰15叶。前像是FIX02/S receipt21078B/SHA `d1be360c76307a7ebe98eb4bb7afc552bb95a30efb98554ea848c4248a822057`；真正diff只`Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetPackageLifecycle.cs`（18285B/SHA `191a559143f1c35209853aaa76dd0a9f0c347c58fe76d08f7bf49fabcdcaf860`）和`Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetProviderTests.cs`（44921B/SHA `e1575a343da4c79add6fffc578d8fa1e21c7b7e051c17f172292ee2199b1aad3`）。其余七源、旧37方法／断言、public/friend/asmdef/依赖及12meta原样；无重构、业务、UI、资源或权限扩张。

验收：新owned全局Initialize抛异常时不得把半成品scope留给当前provider的后来请求；仅在初始化／注册成功后发布scope，或失败时清除半成品，保留已有已注册scope／其他client所有权。首次返回既有受控失败，无自动重试／package工作冒进；下一次显式Acquire在同一个provider上确实再调全局Initialize，成功后正常package/manifest/load并给有效lease。成功全局不重复初始化；borrowed全局／包、旧phase失败重试／observer稳定性、lease/epoch/close语义不变。

先在现有真实provider/lifecycle的内部SDK seam新增普通Test `GlobalInitializationFailureCanBeRetriedBySameProvider`再修产品。使首次全局Initialize抛异常且SDK仍未初始化，断言首次失败／没有后续package/load或自动重试、同provider下次实际第二次Initialize并最终成功，以及释放／关闭完整；不能只测fake的返回值或靠重建provider恢复。保持原37正文断言；预期38仅静态候选，若正确seam确需不同组织，先报告具体增量与fullname，不降弱覆盖。

生产总≤1100、测试总≤1200沿中央原预算，两文件增删目标≤110；可读优先，不压行，确需超出先报具体需要。中央明确只对新候选一次验证，故诊断技能的重复旧head红跑／假设循环在此跳过；不声称红绿已复现。不运行Unity/编译/测试/discovery/网络/Git，也不运行旧verify.py回写原证据。

标准库verify≤30s、新根≤2MiB，before绑定本文、正式finding／原生回执、实际输入／共享guard和原S/FIX01/S/FIX02/S/M01/M02/M03共96封存叶；活投影及meta不碰。patch相对FIX02恰两路径、内存重放相等、七源／旧37／公共面／预算保持，失败原run必须保留。封存停写后返回真实thread/host/turn、九源／十五叶身份、两diff、静态结果和准确新fullnames，状态只到SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING。主程接回后交中央更新同PR9，C另绑新M04一次编译＋完整相关候选，复用M03预算与活区，不复制Library或跑旧大套件。原生与新head复审并行；新旧P2均不自关，所有原receipt/失败和FIX04/M16拒绝保留。
