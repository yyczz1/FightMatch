# UGUI-01-Q4-FIX-09：动态 TMP 字体副作用精确回滚续包

2026-10-01（Asia/Shanghai） · 状态：`SUPERSEDED_NOT_DISPATCHED`

> 本草案从未派发给 C。中央 AI 随后要求沿已接受的 dynamic-font/final-reopen 合同固定
> 合法 additive glyph growth，而不是逐轮回滚字体字节；因此本文件不得执行，后续由
> `engineering-ugui-01-q4-dynamic-font-finalize-fix-09.md` 取代。

## 0. 目的、责任与结论边界

FIX-08 的三文件修复已产生固定候选，首次 Metal graphics exact-3 实际 `3/3`、Unity
exit `0`，证明 raycaster 登记与首帧 depth 修复有效。但该运行让项目动态 TMP font asset
新增 4 个 character/glyph，违反 FIX-08 的 resource byte-identity 门，所以原 run 必须保持
`passed=false / RAW_TESTS_FAILED`，不得改写为通过，也不得继续在已漂移资源上运行。

本包只做**验证副作用回滚与有界续行**：保留 FIX-08 失败证据；从它封存的 exact preimage
恢复一个字体 asset；targeted-9 与 exact-222 每轮若仅产生同类受控动态 glyph addition，
先完整记录再恢复 exact bytes。最终 37/37 resources 必须回到原基线。

- delivery owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52` / `local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 用户 authority：`AGENTS.md` 第 6 节五角色持续流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local` 已要求保留失败证据、使用 sibling root，
  并冻结三轮 Metal graphics 条件。
- 固定 source candidate：
  `c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280`。
- 成功终点仅 `READY_FOR_ENGINEERING_REVIEW`；不自行送 R 或判 UGUI-01 接受。

## 1. 固定输入与失败事实

FIX-08 authority：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `engineering-ugui-01-q4-fixture-lifecycle-fix-08.md` | 11,520 | `e007dbf7d1e922e199ee2128957a335c2dc020ae0d253d4f02d3a425303f98c2` |
| `engineering-ugui-01-q4-fixture-lifecycle-fix-08-addendum-02.md` | 1,397 | `12f37e8e9f210060629bb6f510a5f9f8f738fc1a9c6002520892d4faed995ea7` |
| `q4-correction-08/minimal.patch` | actual file | `21574dd7ca9eaa6222b25cd7ea15eac95abf3de50f9a16c8a412a090e1e7f245` |
| `q4-correction-08/static/source-manifest.json` | actual file | `b2e970c43fdd2c6c95a352bb43a0def2a1f78a927077157359d040715495944d` |
| `q4-correction-08/static/static-proof.json` | actual file | `9d55b09570633c19c51141f74a32c73fea38ed4c7bd161ff186b787245487c7a` |

表内 `q4-` base：

```text
TestArtifacts/FightMatch/UGUI-01/
  c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280/
```

FIX-08 exact-3 固定证据：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `q4-correction-08/exact-3-graphics/run.json` | 55,246 | `1bcf1bd0720829073af49bc6bc9773ad96c09e2c49122ad857877afe8528f86c` |
| `q4-correction-08/exact-3-graphics/tests.xml` | actual file | `b306d4bcd9c8f83018979b88029ec7f2dea3b2b2fad73a4b084406c7f555f671` |
| `q4-correction-08/exact-3-graphics/xml-statistics.json` | 445 | `49796a1a835bea3c9d4f86c47b938f7ebe9745f9c4073eed0c420ff93453b1dd` |
| `q4-correction-08/exact-3-graphics/font-before.asset` | 8,546,563 | `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |
| `q4-correction-08/exact-3-graphics/font-after.asset` | 8,548,693 | `2a186863bc12042842f77e257fc964af205a65e5de406cbdf81b22bcd007805b` |
| `q4-correction-08/exact-3-graphics/font-structure-delta.json` | 350 | `73999bee0596bff3f7de98b45d594e69176ef9acb394b6ddb3b1765f1f87f2ee` |
| `q4-correction-08/exact-3-graphics/process-quiescence.json` | 1,025 | `a40eec183379f9a688868d28c44af50ff9589d2261760396b1ee61887d24545f` |

必须核 raw run：exit0、Metal non-Null、exact names、XML 3/3、source/process/protected/old
roots 全通过；唯一 failed check 是 `resourcesByteIdentical=false`。唯一 resource delta：

```text
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
66e796... / 8546563  ->  2a1868... / 8548693
```

structure delta 只允许既有事实：added characters `29616→26374`、`25552→19232`、
`20132→9708`、`72→41`；无 changed/removed character/glyph；fields 与 external GUIDs
unchanged；texture count `4→4`。任何不符即停止，不执行恢复或后续 Unity。

## 2. 写白名单与永久禁止面

本包不得改任何 source、test、`.meta`、Prefab、Scene、setting、package、Git 或 FIX-08
证据。唯一生产路径写许可是对下列既有 asset 的 exact-byte rollback：

```text
Assets/UI/FightMatch/Fonts/NotoSansCJKsc-Regular-TMP.asset
```

唯一 evidence write root 必须事前不存在：

```text
TestArtifacts/FightMatch/UGUI-01/
  c428391a6315481be7bd35e99c68d930768e036015e23b9d57d42f35c0684280/
    q4-correction-09/
```

禁止手改 YAML 字段、重建 font、SaveAssets、PrepareResources、接受漂移为产品结果、删除
glyph、复制 after asset 为新 baseline、覆盖 FIX-08、重跑 exact-3、修改断言/filter、
`-nographics`、signal/kill、设置 `HOME`/`home`/`CODEX_HOME`。

## 3. 精确 rollback 协议

每次 rollback 只可在相关 Unity 与 owned descendants 全部静止后执行：

1. 冻结 current production font；首次必须精确为 FIX-08 after identity
   `8548693 / 2a1868...`。后续每轮必须等于该轮保存的 `font-after.asset`；
2. 再核 sealed FIX-08 `font-before.asset` 精确为
   `8546563 / 66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb`；
3. 将 sealed before bytes 写到同路径；这只是 byte-for-byte rollback，不解析、不编辑字段；
4. 立即证明 production font bytes/SHA 与 sealed before 完全相等；`.meta`、其它 36
   resources、53 source、Prefab、Scene、old roots 全不变；
5. 保存 rollback receipt：source/destination identities、UTC、process gate、before/after
   37-resource manifests、exact equality 与 no-other-write。

不得从 Git、cache、别的 evidence root 或网络恢复。写入失败或最终不等即
`BLOCKED_FONT_ROLLBACK`；不启动 Unity。

## 4. 有界动态字体 drift 门

targeted-9 或 exact-222 退出后，先保存 raw 37-resource after 与 production font copy，再
做结构比较。只有同时满足以下条件，才允许按 §3 rollback：

- 37 resources 中只有该 font asset 改变；source/meta/Prefab/Scene/settings/old roots 不变；
- character/glyph delta 只能是新增；changed/removed 都为空；
- 所有非 dynamic tables/atlas payload 的既有字段不变；external GUIDs 不变；
- texture count 仍 `4→4`；无 missing reference、serialization/compiler/unhandled error；
- run 本身 exit0、Metal non-Null、name set精确、全部 tests passed；
- process 已 quiescent，且本轮没有 signal/global shutdown。

任一不满足，保留实际 drift 并 `BLOCKED_UNEXPECTED_RESOURCE_DRIFT`；不得 rollback 掩盖异常。
允许的 additive drift 也不能成为新产品 baseline，只能记录后恢复。

## 5. 唯一执行顺序

1. **preflight**：核 §1 全身份与 raw exact-3；核当前三 source 为候选 manifest；核
   current font 是 FIX-08 after；核无 Unity；创建 q4-correction-09 一次。
2. **exact-3 rollback adjudication**：执行 §3 首次 rollback；生成 composite receipt，明确
   原 run 仍 `RAW_TESTS_FAILED`，但其 test sub-result 为 3/3，资源副作用已独立回滚；不得
   重跑 exact-3。
3. **targeted-9 Metal**：用 FIX-08 固定 exact 9 fullnames，`-batchmode`，不得
   `-nographics`/`-quit`；timeout `<=240s`。运行后按 §4 分类；测试或 drift 门失败即停。
   若允许 drift，执行 §3 rollback，确认 37/37 回基线才继续。
4. **exact-222 Metal**：使用已接受 15-fixture 单参数 filter，expected fullnames 精确 222，
   `-batchmode`，不得 `-nographics`/`-quit`；timeout `<=600s`。运行后同样按 §4 与 §3；
   必须最终 37/37 回基线。
5. **final**：53 source 精确 candidate，37 resources 精确 FIX-08 before baseline，三个测试
   sub-result 3/3、9/9、222/222，所有进程静止，封存 `READY_FOR_ENGINEERING_REVIEW`。

新 Unity launch 最多 2；失败不重跑。每轮保存 argv/PID/UTC/duration/exit、stdout/stderr/
log/XML/hash、Metal device、expected/actual names、font before/raw after/structure delta、进程树与
rollback。若唯一 remaining descendant 是已确认的 current-launch VBCSCompiler，可用本 Unity
exact executable/DLL/unique pipe `-shutdown` 一次；禁止 signal、全局 shutdown、retry。

资源上限：log/XML each `<=8 MiB`，new evidence `<=64 MiB`，temp `<=128 MiB`，free-space
`>=2 GiB`。Q1/recovery/compile/SaveAssets/4181/Q5/Android/APK/device/Git 均 `NOT_RUN`。

## 6. 验收与回传

- [ ] FIX-08 raw failure与font after完整保留，没有把旧 run 改成 pass；
- [ ] exact-3不重跑，test 3/3 与独立 rollback receipt合成续行依据；
- [ ] targeted9/222均 Metal non-Null、名称精确、全绿；
- [ ] 每轮只有受控 additive dynamic-font drift，且结构证据完整；
- [ ] 每轮后 production font exact rollback 到 `66e796...`；最终 37/37相等；
- [ ] source candidate、三文件 diff、Prefab/Scene/meta/settings/旧 roots 全不变；
- [ ] no signal、no forbidden action，最终只 `READY_FOR_ENGINEERING_REVIEW` 或最早失败门。

C 回传：FIX-09 packet/activation、owner turn、preflight、原 exact3 adjudication、每轮 raw
run/XML/log/process/font copies/structure delta/rollback receipt、53/37/old-root final manifests、
final receipt 与 NOT_RUN。完成后停止，不自行送 R。

主程审实际 composite evidence 后，才把同一 source candidate 与 FIX-08/09 全链回已开始
预审的原 R-TOOLS `01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 做最终 verdict。
