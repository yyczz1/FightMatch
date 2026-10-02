# HSC-FIX01 — 写锁生命周期测试预期纠正

2026-10-03。中央依据 M01 实际失败明确批准最小修正，不需新增人工批准。签发主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`、turn `01a0fde0-d533-7b00-ad78-34111f776dda`；唯一源码 owner 复用 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`，Astra/xhigh，绑定自身真实新 turn。主程唯一完成收件；中央负责新 PR7 head 发布及一次 GitHub Code Review。不是本地代码审查任务。

## 固定输入及问题

- 原设计 `engineering-host-settings-first-create-001.md`，SHA `90b7351a13e16aa1d04281c42a27e003815b093e33adfcca9173578aaaba027d`；原 S 与 M01 完整只读，不回写失败记录。
- 原 PR7 head `1093faffc096bffa1d36c81d29b2f629430f6f54`；S/source 下 `Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs` 为唯一修正前像，25225B/SHA `3a44b54569966c26745d875e319096f4fe0d98a7513de8a7fbaa66f11daa96dd`。生产 HostSession 后像20308B/SHA `67d62db541c47e3e1548ec32000605b6dcecf38b98a7332195a0824cc51060a9` 不变。
- M01/receipt.json 6491B/SHA `f799fa8ca9a369d82c9030d34db2334c871c623b05fc96287d57a4f5ca2b4d6e`；T/results.xml 18290B/SHA `30ff84f43bcd2d95e1375323c9402a8040bf3c09f7ee3944a3bcc47f8facf57e`。I 成功，18例16通过/2失败，XML执行6.9747519s，owned[]。两失败方法为 `H03_SettingsOnlyAllowsExplicitFirstCreate`、`H03_UnknownSettingsOrPlayerDataStillBlock`，快照实际比预期多 `/locator/writer.lock`。
- 可只读跟踪固定投影或同像源中的 HostSession.ObserveStartup/ReadLocator、LocalPlayerProfileLocator、MacContentPublicationStorage、MacEditorSaveStorage 内 MacStoragePaths、HostRig 的 Files/SameFiles/Dispose；仅定位真实 writer lease 生命周期、正常目录/锁副作用及快照时点。若存在需要产品修正的实质缺陷，报告最小证据/范围，不擅改生产。

## 精确写入范围

新根仅 `TestArtifacts/FightMatch/HOST-SETTINGS-FIRST-CREATE-001/FIX01/S/`，初始不存在。允许七个文件：

1. `source/Assets/Tests/EditMode/FightMatchHost/FightMatchHostProfileTests.cs`
2. `source.patch`（以原 S 测试后像为前像的单路径增量）
3. `before.json`（真实 owner、输入身份及完整测试前像）
4. `after.json`（后像、范围及原件保持证据）
5. `verify.py`（仅机械静态验证，无 Unity/子进程测试/网络）
6. `static-checks.json`（实际命令、退出值及逐条结果，失败同样保留）
7. `source-receipt.json`（诊断、精确更改、验收映射、证据身份、未运行项）

仅修改上述两个失败方法及其必要私有测试辅助方法，最多120增删行；其余方法字节/断言不变，不改公共接口或测试名称/数量。不改共享源码、S/M01、HostRig、Platform、生产、asmdef/meta/资产、Packages/ProjectSettings、历史证据或其他角色文件。RES-A P02 可在不同活区并行，勿将其合法变更当全局 Git 漂移。无 Git 写、Unity、编译、测试、网络、外部 review 或额外 worker。

## 验收与验证

必须用源码证据说明新增零字节普通 `locator/writer.lock` 是否是现有读取持有 writer lease 的正常持久副作用；不能为了测试通过删除正常锁、修改产品生命周期或放宽未知条目拒绝。保留真实 settings-only 的首次观察前置，不通过提前运行被测流程掩盖条件。对允许的正常 locator 目录/零字节非链接锁作精确断言，仍严格保留原全部文件键/字节及目录；若预先已有锁或其他条目，不可覆盖原预期值或忽略未知项。禁止删去 SameFiles/目录保护而无等强替代、跳过用例、减少未知条目16种覆盖或放宽 CanCreate/UnclaimedData/玩家身份/初始化次数/偏好字节保护。

一次本地静态脚本可检查固定输入、单文件 patch 内存重放、两方法/必要 helper 边界、120行预算、其余方法保持、18 NUnit fullnames不变及诊断证据。若静态脚本自身失败，保留该次事实并修正脚本，不伪造运行。它不是行为通过证据。本包仅 SOURCE_READY/UNCOMPILED/UNTESTED；后续由中央发布真实新 head 并一次 GitHub复审，再由唯一C执行固定修正输入的**一次完整18例**有界验证，不安排先2再18重复。下一验证单独精确声明 HostRig 生成的 `projection/TestArtifacts/FightMatch/UGUI-01/host-io` 目录及上限，禁止为输出门放行任意 TestArtifacts 或未知源文件。
