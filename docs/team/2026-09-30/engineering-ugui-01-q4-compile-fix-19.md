# UGUI-01 FIX19：编译限定名与首败收尾

2026-10-01 · 中央授权源码修正／runner准备；Unity执行须另绑定新PR head。

- owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52/local`；唯一收件者中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`；Astra/xhigh。
- 继承[FIX18](engineering-ugui-01-q4-progress-delta-fix-18.md)的源码、资源、selector、断言、进程身份和容量约束，只替换下述差异。
- FIX18实际 `BLOCKED_COMPILE`：107行CS0234，Unity 27.715s退出1；0测试事件／无XML，监视至601.043s仍有已归属的VBCSCompiler。保留旧根，不改写旧失败。
- 已审源码head `bfa8943aacbee2eaec11a8ee5be9935c538e2898`；[GitHub结论](https://github.com/yyczz1/FightMatch/pull/1#issuecomment-5927017964)未发现重大问题。实际编译失败要求修正，旧审查不能代替新head或编译验证。

## 本轮白名单和动作

1. 先只读确认旧PID62357现状。若仍为FIX18记录的同一owned进程（出生时间、完整argv、工具身份和已记录祖先链均匹配），允许对它单次SIGTERM、最多30s等待并保存回执；不存在就记录已退出。禁止按进程名称批量结束、SIGKILL或结束身份不符的PID。普通ps受sandbox限制时可申请此精确核验／收尾的工具权限。旧证据只读。
2. 唯一可改Assets文件 `Assets/Tests/EditMode/FightMatch/FightMatchTestProgressCallback.cs`；前像SHA `5d2a020ee659e297259334335cabbbda751e393b73dc23b86db79176abf326f6`。只将错误引用限定为 `UnityEngine.Application.dataPath`，将本包固定输出叶从 `q4-correction-18`改为`q4-correction-19`。不改meta、原90路径、回调语义、测试名或断言。
3. 新建一次 `TestArtifacts/FightMatch/UGUI-01/q4-correction-19-source/` 保存activation、patch、delta-manifest、static-result、旧进程收尾证据及`validation-tools/runner.py`。不创建正式运行根或启动Unity。
4. runner从FIX18派生，仅更改新root、源码身份及首败收尾：首次发现本轮编译错误或root非零退出立即锁定原始失败；停止等待测试结果，不再耗尽600s。随后有限收尾，保留第一原因和独立cleanup状态。仅对已归属且即时身份匹配的残留子进程先沿既有温和收尾，最多60s；仍存活可单次SIGTERM并最多30s等待。身份不明则返回blocker，禁止继续测试或扩大结束范围。
5. 离线检查覆盖root提前退出／编译错误／成功／超时分支和不匹配PID不发送信号；使用伪进程状态，不启动Unity或真的测试进程。静态核验2处源码差异、原90路径、meta、190 selector与旧exact2/Host30证据保持。原诊断契约继续，不能把0事件算通过。
6. 本回合只交 `SOURCE_READY` 或具体blocker，≤200字回执及证据路径，然后结束。无Git、下载、独立Compile、Unity、测试重跑、native reopen、APK或产品改动。

## 后续执行合同（本轮尚不启动）

中央上传这次delta并绑定实际新head后，允许代码审查与一次必要验证并行；最终接收两门均须满足。正式新根 `TestArtifacts/FightMatch/UGUI-01/q4-correction-19`，仅FIX18既定叶；SOURCE_READY中的runner冻结后运行。

唯一Core190使用FIX18的graphics/Metal、`-releaseCodeOptimization`和冻结selector；从启动至测试退出600s。超时仍按60s自然观察、身份守卫单次SIGTERM及30s终止观察；编译首败走本包提前收尾。首败停止，不同回合修正后才可有新验证。复用exact2和Host30，机械核Counter并集222；不增加独立Compile、分组、单例或整套旧测试。新的运行只为本次编译／监视修正验证，不能抹去FIX18失败。
