# RES-B-ADAPTER-FIX01 — 两项独立P2的原作者修正

2026-10-03。中央明确批准此普通修正与新预算；主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0fe90-2cc1-7d21-9d7c-749be6effd37`签发、唯一收件。原作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`（Astra/xhigh）从S已停写职责进入本次新FIX01/S，派发即直接源码实施；记录实际turn，不开准备回合。Unity仍仅C，源码作者不运行编译/测试/discovery/Git/网络。

固定独立事实：[PR9回执](testing-yooasset-adapter-pr9-review-receipt.json)6362B/SHA `70d45111d831cc300269c3a7149ef902b79d8627ff0951831789120655ac488b`，review5397551743/完整head `5e70957df7db1caab56e3908621a295bcb58a1ce` Completed/NEEDS_FIX。r4170434483在`YooAssetPackageLifecycle.cs:284–287`指出失败init/manifest Operation被缓存，后续Acquire重试不再调用SDK；r4170434491在`YooAssetAssetProvider.cs:150–152`指出非法set SafeDetail错误为`status=failed`。归因来自GitHub服务，不再派本地代码review或另列猜测。

输入沿[六路径合同](engineering-yooasset-res-b-adapter-source-001.md)SHA `08483aa8916a7c8ab0b459028bbd3ab4bf40a8261ca793464f2d3ba362e20baf`，原S=`TestArtifacts/FightMatch/RES-B-ADAPTER-001/S`，receipt15735B/SHA `41d012724da25235d93712445b991274e522d7d230b9e2d89dab84c4030d9fea`。原9源、六证据、实际SDK/包、共享源与在途C投影全部只读。C已报告I无编译错误，T在调整消息核查前已启动且31/31自然完成；原测试未覆盖两项发现，不把旧31通过解释为P2关闭。最终原生收据由主程另收，不妨碍本独立写域修正。

唯一新根 `TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX01/S`起始ABSENT；同原合同15叶：`source/`下完整9源候选及`before.json`、`after.json`、`source.patch`、`verify.py`、`static-checks.json`、`source-receipt.json`。真正产品diff只三路径：`Assets/Scripts/FightMatch/YooAssetAdapter/YooAssetPackageLifecycle.cs`、同目录`YooAssetAssetProvider.cs`、`Assets/Tests/EditMode/FightMatchAsset/FightMatchAssetProviderTests.cs`。另六源（两adapter/test asmdef、remote服务、三baseline）严格原字节；原S不改，不添加meta/公共接口/资源/场景/安装或业务变化。

修正1：失败阶段必须允许后续显式Acquire重新调用真实SDK；初始化失败后重启初始化，manifest失败后重启manifest且不破坏已成功初始化。成功/正在执行操作不可无故重开；旧失败观察者、pending计数、owned/borrowed、epoch/operation身份、close与释放规则保留。不得在同步失败回调中自行循环重试或把终止Task当成SDK已停止。修正2：null、格式非法及`latest` set按冻结B§6.1/6.2返回`WrongReleaseSet/ValidateRequest`、null set及精确`reason=invalid-release-set`，无SDK调用、不泄露原输入；保持asset-id优先顺序，合法但不匹配set行为不可混同。

先在现有测试seam补三条普通Test回归再修产品：`FailedInitializationCanBeRetriedByLaterAcquisition`证明首Acquire失败、下一次确实第二次初始化并可成功；`FailedManifestCanBeRetriedWithoutReinitializingPackage`证明首manifest失败、下一次manifest实际再调用而init仍一次；`InvalidReleaseSetsEmitExactCanonicalReasonBeforeSdkWork`逐项覆盖null/空白/坏格式/latest及null set/安全细节/零SDK。测试经过真实provider+lifecycle的既有内部SDK seam，不只断言fake预置返回；精确断言SDK调用次数/失败→成功及无自动重试。原31方法/fullnames/断言保持，不削弱为绿色；预期34普通Test仅静态预测，XML才确立实际用例。

依中央收缩旧候选测试及一次有效验证要求，本轮不额外运行旧代码红测或做重复诊断假设；回归源码必须针对上述确切模式，封存后由C一次新候选compile＋完整相关例实证，不声称已复现红/绿。生产总上限1100行、测试总上限1050行（含冻结251/247），同文件数、不压行、不用满额度；三文件增量目标≤270增删行，超过先给主程具体需要。无需改架构，不消耗新设计/审查轮。

机械证据沿原S协议但before保护原S15叶/三目标前像、本文/正式finding及实际所用固定输入；共享guard与旧域保持。verify.py仅标准库、≤30s、无子进程/编译/网络，FIX01/S≤2MiB，source.patch相对原S且恰三路径、内存重放等于新后像，六源字节保持、原31名称保持并声明新3、line预算/asmdef/friend/public面不变；失败保留。禁止运行旧verify.py回写旧证据。S和C原生活区不碰，即使C仍在收尾也不冲突。

封存后停写，交真实thread/host/turn、三diff/完整九源/十五叶身份、静态结果与未执行项。仍为`SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING`，两个P2在新head独立复审和新候选验证前保持OPEN。主程唯一收件→中央更新同PR9/GitHub；C有限验证另绑定，不重复旧31或全4181。FIX04 native/LOC M16原阻塞不变。
