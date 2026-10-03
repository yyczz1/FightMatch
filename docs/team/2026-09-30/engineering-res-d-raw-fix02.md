# RES-D-RAW FIX02：分块续调度失败不能永久pending

2026-10-03 · APPROVED_FOR_SOURCE。用户AGENTS §6及中央明确批准按唯一GitHub P2最小修正；issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a1002c-2bd8-7f22-8031-e0cf6401ac97`，作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`、Astra/xhigh，实际turn后绑定。作者PREF DIAG01已completed/停写；本包优先于ActivationStore源码，PREF D由唯一C独立串行。本文不激活原生。

## 固定输入与白名单

- [PR12 QA回执](testing-resource-raw-pr12-review-receipt.json)2387B/SHA `84678b6f4b50351bebd4578035ab1fb7ae8ec87eb567983f1c2b86d56cb29f75`，head `4d4b07cc1b432add8d8102290802703ed704eb21`／P2 `r4171809834`：partial read后Post抛错被Wake吞掉，Ensure回调已脱离，后续无泵导致pending/stream/raw总账滞留。原RAW74通过不覆盖此条件；新回归尚未真实跑红，不声称已复现。按审查授权形成源码与回归，原生验证另签，不重跑旧head求红。
- 前像统一来自 `TestArtifacts/FightMatch/RES-D-RAW-001/FIX01/S/source/`：`Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetAssetProvider.cs`373行19585B/SHA `a060f985709f19d9b139014e8169ca068c96425d6807d1270d13f8dd26674538`；同目录 `YooAssetPackageLifecycle.cs`583行26942B/SHA `16c4d4a9273ee9096fb6a5dc0d71b9d19abfe414d5129a559d21906b6fcd592b`；`Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceRuntimeTests.cs`610行51958B/SHA `c0d12d460cf1e517b6617ab63b2bf5c716d65c7ddf2a233fa4b5b368868d19f4`。只准这三路径的必要内部续调度/终止接线和一个新回归；未需改的文件保持原字节，不能以白名单扩大清理。
- 新 `S=TestArtifacts/FightMatch/RES-D-RAW-001/FIX02/S` 须缺席；恰九叶：`S/source/`下上列三候选，以及`before.json`、`after.json`、`source.patch`、`verify.py`、`static.json`、`source-receipt.json`。本包不写共享Assets、P01、任何meta/asmdef/资源/Packages/ProjectSettings、旧S/FIX01/原生/PR证据、PREF或其它作者文件。读取本合同、三前像、原FIX01 source-receipt/static、QA finding；保护未变ReleaseSet/C FIX01/B与D测试只取既有固定身份，不重扫历史或SDK树。
- 预算：两生产增删各≤70、合计≤100；仍服从RAW原三生产累计≤2300行/184KiB。新测试增删≤65、最终≤675物理行/64KiB，旧D725＋新RAW≤1400；只有下列RAW13一个新普通Test，原RAW12/B38/D13/C11及断言完整保留。stage总≤512KiB，verify≤220非空行/32KiB；预算不互借，任何超额先BLOCKED回主程。

## 唯一行为与回归

1. 选择**受控终止真正仍有body剩余的分块读取**，不引入无限重试/Timer/线程池/新全局泵，也不依赖另一次Acquire、AdvanceEpoch、Close或Dispose才结束。续调度失败必须可被当前主线程raw执行链观察并按既有SdkFailure／ValidateResult安全诊断收敛；所有waiter恰一次结束，stream/hash/buffer和Entry独占64MiB预留在真实主线程清理后恰一次归还，不发布部分payload、不泄露异常正文。
2. 保留通用Wake/背景Dispose/Ensure未terminal的既有B语义、最多一个排队唤醒和每次有界读取。只有内部raw续调度可新增最小成功/失败反馈；不要改public接口/friend/owned-borrowed/epoch规则。原RAW09会在Ensure通知Post失败后用显式第三次Acquire恢复，并要求三旧请求成功，必须保留其所有语句/断言；其1字节body已读完但尚待EOF/hash的情况可在同次主线程完成剩余有界验证，不能因本修正改成失败，也不能在一个pump读取第二个64KiB body chunk。任何不能同时满足的真实技术冲突先报告，不能削弱旧测试。
3. 新精确普通名：`FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeTests.RAW13_FailedChunkSchedulingTerminatesWithoutExternalDrain`。使用原Rig/Dispatcher和真实RawReadState：大于一块的body，已确证0<已读<body长度后，让**下一chunk的Post持续抛错**；只泵此前已排队的动作，先断言原task已受控Rejected、队列有界/无自旋、读取流已关闭/operation释放且未转交payload，再做任何其它provider API或Rig.Dispose。随后验证总账容量恢复与exact-once释放；不把测试清理触发的Drain当修复。适配fake所需内部callback反馈可以改helper，原12方法体及断言逐字保持，不能只修fake而生产仍吞失败。
4. 生产与fake必须覆盖同一续调度决策，回归先写清失败条件再作最小实现；阶段仅作者源码自查/静态检查，不能声称C#、RAW13或旧74已通过。失败关闭自身不能越过真正尚在运行的Ensure；stale与close仍等真实terminal，背景回调不进行SDK/流/总账清理。

## 封存与返回

before先固定owner/host/实际turn、批准/PR/head/finding、三源前像、原FIX01与原S的receipt/patch、本合同和QA身份；其余旧叶沿封存链引用，不重扫或复制历史快照，after只核这些必要不变量。最多两次 `python3 -B S/verify.py`、各≤20s，仅文本/哈希/diff/预算/13名/原12方法体及断言保留的机械检查，结果全部写static.runs；无Unity/C#/SDK/test/discovery、网络/Git写/旧verifier运行、无local review。统一diff须精确重建三候选，未变源零diff；九叶身份、实际检查/失败、逐项作者自查和未验证项进receipt，状态SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING。
封存后停止写入，一次沿AGENTS §6回主程actual turn、变更路径/行数/完整SHA及receipt/patch身份；原RAW74和PREF失败/诊断事实不回写。中央追加同PR新head并派唯一GitHub审查；必要新原生另签精确集合，不自行重跑74或开始ActivationStore。
