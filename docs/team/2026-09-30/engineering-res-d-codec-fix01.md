# RES-D-CODEC-FIX01 — 单调用null重载修正

2026-10-03。中央本轮明确批准：原作者C-CONTRACT封存后串行本修正，无新诊断轮。用户授权沿AGENTS §6；owner `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`、Astra/xhigh；issuer／唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff5a-04a6-7612-9618-b557d6f2ecb9`。本合同未激活作者；激活时补实际turn和已封存C-CONTRACT receipt/八叶身份。

固定D/S receipt10065B/SHA `6bc665c989731ec7c464e0d968015a57262d7eba76470a9d74761ebb89f03c12`与原八叶；M01 receipt4703B/SHA `16910802c4e59ad1251138bae22d3b679c129bf37f22e45d1340206e22769063`及原11失败叶不变。I仅一次/exit1，原log SHA `767a760eaebfb913753e420856b0feda4445175b7a653451bdf04b688c29b246`在631/636/638记录CS0121；T未运行，无XML。scope/源码原检查复用主程bb9f0e，失败11叶身份复用7b8a02；不重跑旧候选来再次证明红。

产品diff唯一允许 `Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceReleaseSetTests.cs` 第298行的 `Reject(null, "RES_SCHEMA");` 改为 `Reject((byte[])null, "RES_SCHEMA");`。原53919B/SHA `ee621dc9d2d853a66b6db6aee795fbb4eff745a7d20d707c2317e27bc653f731`，预期53927B、仍684行，严格+1/-1；不得改变12方法名称、任何断言、其它字符、helper或fixture。生产 `Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceReleaseSet.cs` 原31716B/SHA `2fff363f68195ba7e948e6b998b1ebfe140b12adbc7ab02ba2bebe6d3f69dc46`逐字节保持。问题是测试编译调用歧义，不声称已修正生产行为。

新根仅 `TestArtifacts/FightMatch/RES-D-CODEC-001/FIX01/S`（起始ABSENT），八叶为source/下上述完整两源码＋before.json、after.json、source.patch、verify.py、static-checks.json、source-receipt.json。before绑定实际turn/激活/本文、D原八叶、M01原11叶、B FIX03十五叶、已封存C-CONTRACT八叶及原共享guard；after保持同组。不得读/冻结活投影或进行中的CLOSE01，不改原S/M01/C-CONTRACT/共享源/meta/程序集/依赖；补丁只记录上述单行delta并在内存重放原D测试源。

apply_patch实施；机械证据只写新八叶。生产/测试/总量预算沿原D合同（1200/1000行、80/100KiB、S≤2MiB）。最多2次标准库 `python3 -B TestArtifacts/FightMatch/RES-D-CODEC-001/FIX01/S/verify.py`、每次≤30s，保留失败；只核精确单字符段替换、生产和其余字节保持、固定输入、八叶、12静态fullnames及patch重放，不复算未变fixture/重跑旧verifier。无C#/Unity/测试/discovery/网络/Git/代码审查。

源码完成只报SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING，封存后依AGENTS §6用send_message_to_thread仅回主程/local/Astra-xhigh一次实际turn／receipt／patch／范围。不自行启动后继：唯一C须先真实关闭旧owned门，并由中央按证据批准ADB保留例外，再签M02一次I→精确12T。原两59B自然meta仅下一Unity可同GUID自然补全，旧12meta保持，禁止手补MonoImporter。原失败、无XML、未闭合历史记录保持；真实资源/LOC/Host/设备未验证，FIX04/M16旧拒绝不变。
