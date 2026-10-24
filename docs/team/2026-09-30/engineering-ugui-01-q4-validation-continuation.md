# UGUI-01-Q4-VALIDATION-CONT-01：已修复 Prefab 的验证续跑

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的、权限与完成边界

本包不再修改源码或 Prefab。它接收 FIX-02 已完成的原生保存结果，从当前修复后资源
直接执行 Q2 → Q3 → targeted 9 → exact 222，并按真实退出状态收口。

- 用户 authority：仓库 `AGENTS.md` 第 6 节已批准的五角色持续执行流程；中央 AI 已明确
  普通验证与最小技术修正按已授权包推进，不逐步等待重复批准。
- 唯一执行 owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发与接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 当前 source candidate：
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。

本包禁止重跑 recovery、修改 recovery 源码、再次保存 Prefab、重建字体/Scene、重跑 Q1
或选择新 candidate。任何验证失败、source/resource 越界、进程未在该步总预算内自然静止，
立即保存证据并停止后续步骤。

## 1. 固定接收输入

### 1.1 FIX-02 与原生保存结果

| 输入 | 固定身份 |
| --- | --- |
| FIX-02 packet | `engineering-ugui-01-q4-correction-fix-02.md` / SHA-256 `7ef805179f0d4f7d4b7585857679f7124a7041b1f7a3b50417fc8bfbbb375f12` |
| blocked receipt | `TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/q4-correction-02/blocked-recovery-quiescence.json` / 25,571 bytes / SHA-256 `50777424e0a7d8de7f0e240ea6004d2cda55037589323f389c2aaac20098f0ee` |
| blocked evidence manifest | 11,277 bytes / SHA-256 `90f903987944ce43b983c5e0992d751a25cf935813da10c0ed7cc16fdc4da389` |
| native recovery receipt | 859 bytes / SHA-256 `909ee8fba41be6348ba7e139105cd1f8b442f48549e2c2f6911a900d46b6d972` |
| Prefab delta proof | 1,494 bytes / SHA-256 `4dd6a30abe272716f3041635aceab95575b22c06fd3660dfd3f6f3d54a1a141c` |
| delayed quiescence proof | 712 bytes / SHA-256 `054cf43f637f7a03da34fc780d54c1ead610af3545629708da35793248b9af90` |
| source manifest | 10,009 bytes / SHA-256 `b8c86ac7c4e2cc71f53c24efbccf4f1f6ddbae798ec953f6ee8e21498b2cc350` |

必须同时保留两个进程事实：recovery Editor PID 75254 在 22.203164s 时 exit 0，native/save
receipt 已成功；wrapper 的独立 45s 尾进程窗口结束时仍有一个 `dotnet`，所以原 run
`passed=false`。随后只读补核确认 13 个记录 PID 与全部 Unity process 自然消失，无
signal、无第二次 Unity。不得覆盖或改写任一原 receipt。

### 1.2 当前冻结 source 与 resources

七个 correction source 当前 SHA-256：

```text
CandidateBoardElement.cs               956744b6f5f1fb571e5c069589d422e0dd8f668b604f50c35f00b22c8cdc357d
CandidateBoardInputTestData.cs         34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c
FightMatchHostTestFixture.cs           81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8
FightMatchAndroidBuild.cs              2c7a1612954ccc341f72a58918ffc1356074bf1802c866f01b7c9fdb72ad7dac
LocalizedTextBindingTests.cs           095ce3720258d860c70883201288c009efa397cddcb6bad5ec7bc713f6358cd1
PlayerNavigationRecoveryView.cs        dbfdfe0e333777d19d76e8a84860d6912fbd916e44425a99cbb7b1a9ffd128e9
PlayerNavigationPresentationTests.cs   0ee946a00433e0fdebb08b1699ebc6cbafb8816d88ea4a53ac26a1f1631279a4
```

当前关键资源：

| 资源 | bytes / SHA-256 |
| --- | --- |
| Scene | 12,900 / `0ceee6f17e95359150c43b2a754703b5770096f183cc713120287083eb400bcd` |
| repaired Prefab | 1,060,568 / `55ded35adf6fd7d4e207303801e3ada172190bea6a516443e1bcd21d3193004b` |
| TMP font | 8,546,563 / `66e7963607e4e33432af150ebd085e7cdac1324133e102eaf87da24c135febdb` |

Prefab GUID 仍为 `c36df3cfc25424c8cb3ec6cae6be1237`；BoardFrame fileID
`4805194842302782578` 只新增 component reference
`3490000220422917678`，对应唯一 class ID 222。object count 1434→1435；移除该单行后全部
旧 serialized blocks 逐字相同；其余 36 resources 与字体原字节不变。

## 2. 零产品写入与 evidence 续写范围

产品、测试、asset、meta 写入白名单为 **空集**。不得调用 `apply_patch` 修改任何现有
产品/测试/asset 文件，不得运行 recovery/PrepareResources/SaveAssets。

只允许在现有 candidate root 下创建尚不存在的验证 descendants：

```text
TestArtifacts/FightMatch/UGUI-01/0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91/
  q4-correction-02/
    continuation-packet.md
    continuation-preflight.json
    resources/reopen/
    compile/
    targeted-9/
    editmode-222/
    resources/final-reopen/       # 仅第 4.5 条条件成立时
    continuation-final-receipt.json
```

已存在的 static/recovery/blocked receipts 与 manifest 全部只读，不能覆盖。每个新 output
create-new；任一路径已存在即停止核对，不得 merge 或重写。

## 3. 进程观测合同

每步启动前核对无其它 Unity process。每个 Unity process 同时保存：

1. Editor immediate exit：absolute argv、PID、UTC、duration、exit、stdout/stderr/log hash；
2. immediate owned-process snapshot；
3. 若仍有本步记录的尾部 child，**只读等待其自然退出**，不发送 signal，并在本步从
   launch 起的剩余总预算内定期记录 snapshot；
4. final quiescence snapshot。即时与最终不同必须分别保留，不能覆盖 immediate receipt。

原 FIX-02 wrapper 的固定 45 秒只是已失败运行的收尾策略，不是本续包的新独立硬门。
本包按每步总预算判定：Editor + 尾部自然退出合计超过该步上限，或上限时仍有 owned/
Unity process，才返回 `BLOCKED_PROCESS_QUIESCENCE` 并停止。不得为等尾进程额外启动 Unity，
也不得把未静止写成通过。

## 4. 顺序验证

每步前后冻结 53-source manifest、37-resource manifest、Prefab/font 原字节与结构、旧
b9/f21/8a3 roots、当前 recovery evidence。source 必须始终是 candidate `0ff4218c…`。

1. **Continuation preflight**：核 §1 全部 identity、Prefab delta、当前无 Unity、所有上次
   PID 已消失；复制本 packet 到 create-new evidence。不得运行 Q1/recovery。
2. **Q2 resource reopen**：一次 Unity，launch→最终 quiescence 总计 `≤300s`。output 精确
   为 `q4-correction-02/resources/reopen/resource-reopen.json`，run mode
   `VerifyResources`，source/author/argv 绑定。必须通过既有未改的 119 TMP、3 linked input、
   116 placeholder、Scene、repaired Prefab/CanvasRenderer、EventSystem、font/subassets、
   shader/TMP Settings 与 H05 相关资源断言。把这次实际 37-resource identities 冻结为
   **新 Q2 baseline**。
3. **Q3 compile**：新 Unity，一次，launch→静止 `≤300s`；exit 0、无 compiler/unhandled
   error；53 source 与 Q2 37-resource baseline 不变。
4. **Targeted 9**：新 Unity，一次，launch→静止 `≤180s`；使用原 diagnosis 精确 9
   fullnames，必须 9/9 pass、0 failed/skipped/inconclusive；XML 名称集合精确，source/
   resources 不变。失败不跑 222。
5. **Q4 exact 222**：新 Unity，一次，launch→静止 `≤600s`；必须精确 222/222、0
   failed/skipped/inconclusive，名称集合与冻结清单相同。
6. **Final resource stability**：比较 Q4 后 37 resources 与 Q2 baseline：
   - 37/37 byte-identical：直接封存，不再开 Unity；
   - 只有 dynamic font 发生有结构证据的变化：最多一次 final reopen，launch→静止
     `≤300s`，使用固定 `resources/final-reopen/resource-reopen.json`；复开前后必须稳定；
   - 其它资源变化或 final reopen 后继续变化：`BLOCKED_RESOURCE_DRIFT`。

本包常规 Unity launches 最大 4（Q2/Q3/targeted/Q4）；只有第 6 步字体条件成立才最大 5。
禁止 Q1/recovery/Prepare/4181/Q5/Android/APK/device。

## 5. 验收与回传

- [ ] 现有 source、Prefab、Scene、font、meta 与所有产品/测试文件零修改；当前 repaired
  Prefab identity 始终为 `55ded35a…`。
- [ ] Q2 原资源断言通过并形成新基线；Q3 compile 通过。
- [ ] targeted 精确 9/9；Q4 精确 222/222；无 skip/inconclusive 或名称替换。
- [ ] Q2→final 资源符合 §4.6，H05/Verify/font/subasset/raycast/mesh/business assertions 未放宽。
- [ ] 每步 immediate exit 与 final quiescence 分开保存；所有 tail processes 在本步总预算
  内自然退出，无 signal、无并发 Unity。
- [ ] b9/f21/8a3、recovery blocked evidence 与原诊断 evidence 只读不变。
- [ ] Q1/recovery/Prepare/4181/Q5/Android/APK/device 均 `NOT_RUN`。

Owner 回传：continuation packet/preflight/final receipt identities；Q2/Q3/targeted/Q4 的真实
argv/PID/time/exit/log/XML/process snapshots；targeted/222 exact names；Q2/final 37-resource
manifests；font structure；source/resource/old-root before/after；未运行项。完成后停止，不得
自行请求 QA/R、提交、推送、建分支或改 Git。

主程审实际 evidence 后才送 QA；QA 回同一 R-TOOLS
`01a0f3e3-9663-7be2-84a8-ed23be32f0b9 / local` 作最终独立验收。
