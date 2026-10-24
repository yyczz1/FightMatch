# RES-B-ADAPTER-FIX02 — manifest 后验失败与观察者稳定性

2026-10-03。中央依用户AGENTS §6站立授权明确批准此修正及预算；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0feee-dc74-7930-b61a-1642a94d27a6`签发／唯一收件。原作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`以Astra/xhigh直接源码实施，记录实际turn；Unity仍仅C。本包不运行编译、测试、discovery、网络或Git。

固定依据：[正式FIX01审查](testing-yooasset-adapter-fix01-pr9-review-receipt.json)6664B/SHA `903cfd94778487b6de7eed9052ccf0b334315a3dbd761e5fb0ff4134dfaabbe7`，review5397790966/head `69d8738976095bbcf0d014b737c73abd5b153f7a`，P2 r4170623081，`YooAssetPackageLifecycle.cs`右255。服务发现：manifest SDK operation成功后Version不符／抛异常，Operation已清、Phase=2，后来显式Acquire不重调Manifest；旧observer可能随版本恢复变成功。归因只引用独立服务，不再派本地review或另列诊断猜测。

沿[原六路径合同](engineering-yooasset-res-b-adapter-source-001.md)SHA `08483aa8916a7c8ab0b459028bbd3ab4bf40a8261ca793464f2d3ba362e20baf`与[FIX01](engineering-res-b-adapter-fix01.md)的格式／保护约定，以`TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX01/S`为本次前像，receipt17637B/SHA `4384b0b472314c0ff62cbe0f6f6b02d258091b6e4e2cd30d0dd903301e5657d2`。M02原生receipt14099B/SHA `3c60c0e7a5ee16c2301b1cf63d82ac61938e636b2ae834e20a61052ef134e490`的34/34仅属FIX01，不能关闭新P2。实际SDK及包图沿原固定身份，只读。

唯一新根`TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX02/S`起始ABSENT，仍恰15叶：`source/`完整原9源候选，加`before.json`、`after.json`、`source.patch`、`verify.py`、`static-checks.json`、`source-receipt.json`。真正diff仅`Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetPackageLifecycle.cs`（前像17542B/SHA `8e45ebe9a6296eb0bfefa9b115b46e19f83886ce2c390db1adfdb4b106ddbae7`）及`Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetProviderTests.cs`（39580B/SHA `e70168ee71b25c2f332d89f6f269c2a0ab40b0f2ae1e0fb74b37ad31c2eba36c`）；另七源严格FIX01原字节，包括provider的invalid-set修正。S／FIX01／M01／M02、活投影、12自然meta、共享Assets/Packages/ProjectSettings和历史保护只读。无新公共接口、asmdef/friend、资源、包或业务变化。

验收：owned包manifest操作成功但Version不符／异常后，当前attempt必须固定ManifestUnavailable/SelectManifest失败；后来显式Acquire实际重新调用Manifest并可成功，已成功Initialize不再调用。每个旧observer无论在新attempt之前或之后被消费，都保持原失败，不能改成成功或消耗新attempt结果。运行中／成功操作不无故重启，无同步失败回调自旋、自动重试或版本恢复后仅重读Version冒充Manifest重试。borrowed包不得重新初始化／切manifest；原epoch、operation身份、共享lease、pending计数、close与释放语义完整保留。

先补针对真实provider/lifecycle内部SDK seam的回归源码再修产品：候选三个普通Test可分别命名`PostLoadVersionMismatchRetriesManifestOnLaterAcquisition`、`PostLoadVersionExceptionRetriesManifestOnLaterAcquisition`、`PostLoadValidationFailureRemainsStableAcrossLaterAttempts`。覆盖两种后验失败后Manifest实调次数／Initialize仍一次／后来成功，及同一旧attempt多个observer在重试成功前后不变；稳定性对不符和异常两种入口均有效。保留原34名称／方法／断言，不只测试fake预置结果或削弱断言。若正确seam需要不同用例组织，在收据精确说明覆盖和新fullname；数量由固定源码和后续XML确定，不把37预测当discovery。

中央禁止重跑旧head，故本轮明确跳过重复旧候选红跑／假设轮；不声称已复现红绿。源码封存后同C一次受影响编译＋完整相关suite，另签新证据根，不跑独立三例或旧34替代。生产总≤1100、测试总≤1200（均含冻结基线），两路径增删目标≤230；可读优先、不压行，超出先报具体需求。

标准库机械verify≤30s、无子进程／网络／编译，新根≤2MiB；before绑定本文／正式finding／前像与实际输入，并冻结原S15叶、FIX01/S15叶、M01/M02各17叶及原共享guard。patch相对FIX01且恰两路径、内存重放等于新后像；七源、原34方法／断言、公共面和预算均核；原verify.py不运行。失败留原件，不改活投影。封存后停写，返回真实thread/host/turn、完整9源／15叶、diff／静态证据、准确新增fullname和未运行项，状态只到SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING。主程接回→中央更新同PR9／GitHub；最终接收仍需新候选原生及新head独立结果。D不启动，FIX04／M16拒绝保持。
