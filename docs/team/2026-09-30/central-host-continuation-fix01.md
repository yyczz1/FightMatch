# HOST-CONTINUATION-FIX01：两处测试编译错误

2026-10-07。用户已授权持续开发及必要修正；中央直接签发小修正，原系统设计不变。本包唯一作者复用 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，显式Astra/xhigh；中央唯一完成收件，主程不重复转报。与C恢复包并行，写域互不交叉。

固定前像：`TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/S/source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs`，25089B/SHA256 `3d75346c8bc72b09f7e793d728db521925a5d884e7491bdf9c5c26b8587494fc`。S/source-receipt.json为37790B/`410a5ccaa8b7216dec2bf3fa7619e61690e36befc6a47e6eaba584644ed18fb6`。

新鲜失败：M01/I/editor.log 71930B/`36f690c9c24f0507dfac9510416e78ae2c1000311ce507e9a3caa88c80c1549d`，行647/648：测试365:33 CS0103 `LocalPlayerProfileState`上下文无法解析；439:67 CS1061 `IDisposable.CanWrite`不存在。I已真实exit1，T未运行；M01/receipt.json 8231B/`b4569ac57da887f593a4d0c038272244a056a4034e06a5126ce6fbdc51a9528c`。不把原静态通过当编译通过。

只修上述两处名称／静态类型绑定及必要using，最多25增删行；先定位实际枚举命名空间与真实lease类型。保留所有断言语义，尤其释放后不可写的行为检查；不得以删除／恒真断言／catch忽略来消除编译错误。八个测试名称、选择器、执行顺序、夹具及现有产品代码均不变。如需实质行为变化则报告具体冲突，不扩大本包。

唯一新写根 `TestArtifacts/FightMatch/HOST-CONTINUATION-SOURCE-001/FIX01/S/`（开始核缺席），精确七叶：

- `source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostLocaleWiringTests.cs`
- `source.patch`（相对固定前像单路径）
- `before.json`
- `after.json`
- `verify.py`
- `static-checks.json`
- `source-receipt.json`

仅机械核输入身份、单路径补丁重建、25行预算、八方法及未修改段保持；至多两轮累计30秒、总1MiB，失败保留。新回执固定候选SHA/blob、实际作者turn、诊断及未运行项。不要复跑旧checker，也不造C#通过证据。禁止写S/M01/共享工程/投影、meta/asmdef、修改生产或测试接口、启动Unity／编译／测试、Git／网络或新角色。自然测试meta由C恢复包保留，不由作者生成。完成后停写并直接回中央；后续执行采用新包，不原样重跑M01。
