# Host 组合编译之后：PR4 与最小资源采用包

2026-10-08 · PROPOSAL_ONLY。主程 actual `01a117f9-c3b9-7a41-9ef4-72829fd40684`；中央唯一收件。只新增本文；不派工、Git、Unity或审查，不接I01-run结果。

## 1. 固定依据与剩余差量

本目录输入SHA256：

- `engineering-layout-n11r1-serialized-delta.md`：`5b0db3a217d1e46b89f1d84e4954ffd98aff931060e68257b6ec0cd6c5467f05`。
- `host-next-001-m04-integration-receipt.json`：`30778b6797c8d757e7054955b3436b0c2262f7aa3fe64a4fded8c283f6063a97`；PR17 head `ad0f88a177ded15a05a7c661c80b3797a19ab251`。
- `resource-codec-fix02-integration-receipt.json`：`7f9b8c89cec9ea1f2a3155481271cce58aae88cce77d5e0d7e2883bf5c562d36`；PR10 head `0377c31fef0a0bc08478175c6a22667bb21c8b73`，限定ACCEPT。

PR4 P1仍对应head `e3d581df42f3a25cfdfc319bca5509f8b5382ada`，finding `r4166096874`；不是本轮重新审查。

| 对象 | 已证事实与下一动作 |
| --- | --- |
| Scene五条零覆盖 | N11R1无Set冷载也重现，有效scale为正。不再猜测删五条，Scene改动0。原27＋79条＋marker解释后像，不证明79条必须采用。 |
| ResponsiveLayout | 已有尺寸、safeArea版本、factor/lossyScale自然重采样。无证据要求改Apply、强刷或解除门；源码改动0，P1保持。 |
| PR17观察 | M03严格A/B/C/D及清理PASS，wrapper仍FAILED；M04仅离线修正通过。取消后Unbind/ReleaseGate不等于正常输入通过，不为补绿灯重跑。 |
| 原产品Scene | 共享仍是12900B原27条，SHA `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd`。PR17用了27798B、带marker且不同GUID的N11R1场景，不能直接替原Scene签首帧通过。继续保留原Scene及meta，不导入诊断后像。 |
| 真正待实现 | `FightMatchPlayerHost.InitializeHost`仍构造null文本源。后续需正式资源准入→受验文本parser/adapter→有源LocalizationService→现有偏好/Bind/Session顺序，以及lease/取消释放接线；不是Canvas修复。当前Host SHA `d3774d5c9cf20cdf9649540f2479ebb198241188ecfdf88bdae09d1122c22456`。 |

待验证：原Scene自然首Bind／首有效Canvas后下一自然willRender布局；非空会话输入、无效布局取消手势、恢复不提交旧手势；设备画面。随正式Host接线复用PR17方法补齐；Prefab夹具、Force／手动Bind不能替代。新候选及原P1证据交同head GitHub判断。

LOC阻塞精确在正式artifact/manifest/source receipt与B-CATALOG输入，不在几何采样或纯资源代码。null源会使Host退出并解绑，故正常交互门不能靠继续等帧完成。真实内置包还依赖已批准策略、八raw和UI对象闭包及独立Player pin；沿`engineering-res-built-in-admission-policy-001.md`补齐，不把fixture当产品值。

## 2. 最小下一包：RES-CODEC-ADOPT-001

目的：采用已接收的发布描述编解码器。两个目标目录缺席；纯C#子集不依赖SDK、Host或正式LOC。RAW／activation／factory候选保持，后续按已接受增量接入，不整树覆盖。

根路径均相对仓库。A=`Assets/Scripts/FightMatch/AssetAccess/`，T=`Assets/Tests/EditMode/FightMatchAsset/`。
B=`TestArtifacts/FightMatch/RES-B-CONTRACTS-001/S/source/`；D=`TestArtifacts/FightMatch/RES-D-CODEC-001/FIX02/S/source/`；P=`TestArtifacts/FightMatch/RES-01A/P01/projection/`。

唯一产品白名单：下表四路径、各自`.meta`，以及`Assets/Scripts/FightMatch/AssetAccess.meta`、`Assets/Tests/EditMode/FightMatchAsset.meta`，合10叶。四源码从B/D加目标相对路径逐字节采用；六meta从P同路径采用，保留GUID。

| 目标 | 来源 | bytes / SHA256 |
| --- | --- | --- |
| A+`FightMatch.AssetAccess.asmdef` | B | 377 / `09347e70d10f8cb3e26751c004ba005cadc32d1f3b6ecadf78bb6c9cf1fb2e45` |
| A+`FightMatchResourceReleaseSet.cs` | D | 31863 / `1dcd1288125b2c1f2ed26d6be9a4cfdd28ae59ae024e584b1b7a42137cc6061a` |
| T+`FightMatch.AssetAccess.Tests.asmdef` | B | 515 / `1d605c9ea6e3727c7fe4f595078a12469def24ec7055489f1a467951dde244a0` |
| T+`FightMatchResourceReleaseSetTests.cs` | D | 56352 / `e2fc16d89145bf7f502426545f1a6d0bf0b822adcc45db89842d0dc89ded5842` |

六meta SHA按上表四项、A目录、T目录顺序：

```text
71e1d59633f8843474b4b49e08037d59ab5fce7cb98b6c87fbae1a3befbf0b14
900e5f8ccba2464369b17e4e73258e5431ef329a24e31658a597c61d0fdbb62c
72aca75813de7109895ab4a65f46a5dd60ba1f235850ea566d53df2ad03b5ee3
7cc4ffb364174abe938cc709959afed33daddd259554f5d8614b29f8d16dde85
ddfbcd983fe098bfcd8e030ead0151a21546e79ed7870588e1bda12717477a32
449fc8beb147b177d6c8d018a543b446392c8aae9b2c447982f30255e38bde28
```

采用步骤／验收：

1. 中央接完I01-run并确认C停写后激活；封当前共享基线。目标仍须缺席，十叶身份和全工程GUID无冲突才采用；漂移停报，不覆盖新工作。
2. 仅新增十叶；代码、断言、程序集引用不改。生产asmdef无外部引用且noEngineReferences=true，测试仅引用AssetAccess，不采用带YooAssetAdapter引用的后期测试asmdef。
3. 一次机械核对十叶字节／GUID、总新增90269B、其它产品输入不变；仅新增证据`TestArtifacts/FightMatch/RES-CODEC-ADOPT-001/S01/receipt.json`，记录前后身份、实际owner/turn与未验证项。成功状态只为SOURCE_ADOPTED_COMPILE_PENDING。

必要验证：新子集/程序集须纳入下一次获批资源组合编译，旧D13/13不证明新组合通过；仅新增行为风险才跑原D01–D13，不跑全量。当前包Unity为0；运行根、预算、收尾另签，不追加I01-run。沿用PR10代码接收，新集成提交按GitHub门处理，不派本地复审。

负责人主程；执行为唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`、Astra/xhigh，fresh turn待派发；中央唯一收件。仅十叶及单回执可写；Host/Scene/Prefab/Packages/ProjectSettings/存档/旧证据/Git禁改。writing-for-agents固定完成条件；uGUI核现状原则避免猜测修复。
