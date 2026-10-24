# UGUI-01-Q4-FIX-09：动态 TMP 字体增量与 final-reopen 续包

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的、责任与结论边界

FIX-08 已产生固定三文件 source candidate。首次 Metal graphics exact-3 的测试事实为
`3/3`、Unity exit `0`、名称精确、Metal non-Null；但 dynamic TMP font asset 新增 4 个
character/glyph，原包严格资源字节门返回 `RAW_TESTS_FAILED`。两件事实必须同时保留：
修复行为已被 exact-3 证明；资源尚未最终化。

本续包沿已接受的 uGUI correction dynamic-font/final-reopen 合同：先核这次差异只有合法
additive glyph/atlas cache；不重跑 exact-3；让剩余 targeted-9 与 exact-222 在同一有界
growth 链上执行；最后冻结实际 37-resource 身份，并用一次独立原生 Unity reopen 证明稳定。

- delivery owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求沿既有 dynamic-font/
  final-reopen 策略处理，不回滚或重复 exact-3。
- fixed source candidate：
  `c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280`。
- 成功终点只为 `READY_FOR_ENGINEERING_REVIEW`；R-TOOLS 才能最终接受。

先前未派发的 `engineering-ugui-01-q4-dynamic-font-rollback-fix-09.md` 已标
`SUPERSEDED_NOT_DISPATCHED`，不得执行、引用为 authority 或据其恢复字体。

## 1. 固定 authority 与当前实际状态

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `engineering-ugui-01-q4-fixture-lifecycle-fix-08.md` | 11,520 | `e007dbf7d1e922e199ee2128957a335c2dc020ae0d253d4f02d3a425303f98c2` |
| `engineering-ugui-01-q4-fixture-lifecycle-fix-08-addendum-02.md` | 1,397 | `12f37e8e9f210060629bb6f510a5f9f8f738fc1a9c6002520892d4faed995ea7` |
| `q4-correction-08/minimal.patch` | actual | `21574dd7ca9eaa6222b25cd7ea15eac95abf3de50f9a16c8a412a090e1e7f245` |
| `q4-correction-08/static/source-manifest.json` | actual | `b2e970c43fdd2c6c95a352bb43a0def2a1f78a927077157359d040715495944d` |
| `q4-correction-08/static/static-proof.json` | actual | `9d55b09570633c19c51141f74a32c73fea38ed4c7bd161ff186b787245487c7a` |

表内 `q4-` base：

```text
TestArtifacts/FightMatch/UGUI-01/
  c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280/
```

FIX-08 exact-3 固定证据：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `q4-correction-08/exact-3-graphics/run.json` | 55,246 | `1bcf1bd0720829073af49bc6bc9773ad96c09e2c49122ad857877afe8528f86c` |
| `q4-correction-08/exact-3-graphics/tests.xml` | actual | `b306d4bcd9c8f83018979b88029ec7f2dea3b2b2fad73a4b084406c7f555f671` |
| `q4-correction-08/exact-3-graphics/xml-statistics.json` | 445 | `49796a1a835bea3c9d4f86c47b938f7ebe9745f9c4073eed0c420ff93453b1dd` |
| `q4-correction-08/exact-3-graphics/font-before.asset` | 8,546,563 | `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |
| `q4-correction-08/exact-3-graphics/font-after.asset` | 8,548,693 | `2a186863bc12042842f77e257fc964af205a65e5de406cbdf81b22bcd007805b` |
| `q4-correction-08/exact-3-graphics/font-structure-delta.json` | 350 | `73999bee0596bff3f7de98b45d594e69176ef9acb394b6ddb3b1765f1f87f2ee` |
| `q4-correction-08/exact-3-graphics/process-quiescence.json` | 1,025 | `a40eec183379f9a688868d28c44af50ff9589d2261760396b1ee61887d24545f` |

必须机械核 raw run 的 checks：除 `resourcesByteIdentical=false` 外全 true；XML 精确 3/3；
exit0；Metal；无 compiler/unhandled/process/protected/old-root/source failure。当前 production
font 必须仍精确等于 sealed `font-after.asset` 的 bytes/SHA；不得复制、恢复或修改它。

## 2. 首次 font 差异的合法性门

执行任何后续 Unity 前，从 sealed before/after 与 structure delta 证明：

- 37 resources 中唯一 delta 是
  `Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset`；
- added characters 精确：`29616→26374`、`25552→19232`、`20132→9708`、`72→41`；
- added glyphs 精确：`26374`、`19232`、`9708`、`41`；
- changed/removed characters 与 glyphs 均为空；
- font source/config/material/external GUID/reference、既有 tables 与其它结构字段不变；
- atlas texture count `4→4`；无 missing reference 或 serialization error；
- font `.meta`、Prefab、Scene 与其它 36 resources byte-identical。

任一不成立即 `BLOCKED_UNEXPECTED_RESOURCE_DRIFT`，不得运行 targeted-9。通过只说明这是
受控 dynamic cache growth，可作为本续包 provisional resource baseline；不等于最终资源已接收。

## 3. 写白名单与禁止面

不得改任何 source/test/`.meta`/Prefab/Scene/setting/package/Git，也不得直接写、复制、回滚、
重建或手改 font asset。后续 font bytes 只允许由授权 Unity test processes 自然产生。

唯一新 evidence root，执行前必须不存在：

```text
TestArtifacts/FightMatch/UGUI-01/
  c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280/
    q4-correction-09/
```

FIX-08 root 与所有旧 evidence 只读。禁止重跑 exact-3、放宽断言/name set、`-nographics`、
SaveAssets/PrepareResources、临时 Editor script、修改 `FightMatchAndroidBuild.cs`、signal/kill、
设置 `HOME`/`home`/`CODEX_HOME`。

## 4. 后续每轮 dynamic-font 门

targeted-9 与 exact-222 各自运行前保存当前 37-resource baseline、完整 font bytes 和结构；
运行后先保存 raw after，再比较：

- 其它 36 resources、全部 source/meta/Prefab/Scene/settings/old roots 必须 byte-identical；
- font 可 byte-identical，或只新增 characters/glyphs；禁止 changed/removed entry；
- source/config/material/external GUID/reference 与非 dynamic 结构字段不变；
- atlas texture count 保持 4；无 missing reference/serialization/compiler/unhandled error；
- 本轮 exit0、Metal non-Null、actual names精确、tests 全绿、process quiescent。

任一不符，保存 raw drift 后 `BLOCKED_UNEXPECTED_RESOURCE_DRIFT`，不得继续。合法 additive
growth 直接成为下一轮 provisional baseline；不得回滚、重跑或把中间身份冒充 final。

## 5. 唯一执行序列

1. **preflight/adjudication**：核 §1/§2、当前 source candidate/font/process/free-space；创建
   q4-correction-09；写 composite receipt：FIX-08 raw status保持失败，exact-3 test sub-result
   接收为 3/3，资源转入本包 finalization；不重跑 exact-3。
2. **targeted-9 Metal**：FIX-08 固定 9 fullnames，`-batchmode`，不传 `-nographics`/`-quit`，
   timeout `<=240s`；通过 §4 后才继续。
3. **exact-222 Metal**：已接受 15-fixture 单参数 filter、expected fullnames 精确 222，
   `-batchmode`，不传 `-nographics`/`-quit`，timeout `<=600s`；通过 §4 后冻结 provisional
   final 37-resource manifest 与 font structure。
4. **native final reopen**：新独立 Unity process，一次、Metal graphics、`-batchmode -quit`
   且不传 `-nographics`，timeout `<=300s`；不运行测试、不写产品、不调用只接受旧 evidence
   path 的 `VerifyUguiResources`，只让 Unity 原生导入/加载当前项目并正常退出。保存完整
   argv/PID/log/renderer/process；复开前后 53 source、37 resources、font bytes/structure、
   Prefab/Scene/meta/settings/old roots 必须逐字节稳定，log 不得有 compiler/unhandled/
   missing-reference/serialization error。任一变化即 `BLOCKED_RESOURCE_DRIFT`，不得第二次复开。
5. **final**：组合 exact3=3/3、targeted9=9/9、exact222=222/222 与 final reopen stability；
   写 `READY_FOR_ENGINEERING_REVIEW`，明确最终 font bytes/SHA/structure 与相对原 66e796...
   的完整 additive delta。

新 Unity launch 最多 3；失败不重跑。每个 process 前确认项目未被其它 Unity 打开；保存
argv/PID/UTC/duration/exit/stdout/stderr/log/XML/hash、Metal、expected/actual names、font before/
after/structure delta、37 manifests 与进程树。若唯一 current-launch descendant 是已确认的
本 Unity bundle VBCSCompiler，可用 exact executable/DLL/unique pipe `-shutdown` 一次；禁止
signal、全局 shutdown、retry。

资源上限：log/XML each `<=8 MiB`，new evidence `<=96 MiB`，temp `<=128 MiB`，free-space
`>=2 GiB`。compile/Q1/recovery/SaveAssets/4181/Q5/Android/APK/device/Git 均 `NOT_RUN`。

## 6. 验收与回传

- [ ] FIX-08 raw failure、font before/after 与 structure delta 原样保留；exact-3未重跑；
- [ ] 首次及后续 font 变化均只有合法 additive dynamic glyph growth；非字体资源零漂移；
- [ ] targeted9/222均 Metal non-Null、名称精确、全绿；
- [ ] 最终原生 reopen 前后 53/37 与 font structure/bytes 稳定；
- [ ] 三文件 source candidate、Prefab/Scene/meta/settings/旧 roots 不变；
- [ ] 无手工 font 写入、无 signal、无 forbidden action；
- [ ] 最终状态仅 `READY_FOR_ENGINEERING_REVIEW` 或最早失败门。

C 回传：packet/activation、owner turn、preflight、exact3 adjudication、targeted/222 raw evidence、
每阶段 37/font structure、final reopen 与 process evidence、最终 53/37/old-root manifests、
final receipt/NOT_RUN。完成后停止，不自行送 R。

主程审实际 composite evidence 后，把同一 source candidate 与 FIX-08/09 全链交正在预审的
原 R-TOOLS `01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 作最终唯一 verdict。
