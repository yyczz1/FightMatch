# LOC-IMPL-A 实施包：正式源、project compiler 与确定性产物

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`。本包只授权非 Unity 的
本地化制作链候选；不授权 LOC-IMPL-B、Host 接线、RES、Windows、Android、设备或发行。

## 1. owner、固定输入与返回

- 唯一交付 owner：独立 LOC-A execution chat
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`。设计作者、C、QA、R 不并发
  写本包路径。
- 返回主程：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 代码起点：`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；保留当前脏工作树，不清理、
  覆盖或提交其他角色文件，不执行 Git 写操作。
- 接受设计：`engineering-localization-luban-001.md` SHA-256
  `43f98e353b20eb583df8154fe604dd73f32b281378aefade22382916975fe1fc`，
  743 lines / 50,126 bytes；原独立 R thread
  `01a0e404-e8ee-7310-8388-9260babd53f1`，turn
  `01a0f408-5374-7660-80d8-3fa332c632aa`，final
  `msg_0d29bf8d726d8dc0016abd73567b7c87d0824de3bb6f79ebff`，verdict `ACCEPT`。
- COPY 输入只读：`docs/team/2026-09-30/planning-localization-draft.csv` SHA-256
  `ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf`，
  65,358 bytes、265 physical lines、264 records/unique，severity
  info/warning/error/blocking=`172/74/5/13`，parameter mismatch=`0`。
- 005 输入只读：`TestArtifacts/FightMatch/LOC-TOOL-01-PREFLIGHT/mac-intel/005/`；
  `run.json` SHA `ba0840a861a4b557bd779d8fcbe516818a98fcfc70551c947f643f79be795232`，
  `tool-identity.json`、`input-hashes.json`、`identity/runner-files.tsv` 按接受设计取值。
  外盘固定 cache/SDK/runner 只读复用；不得重跑 preflight 或改 001～005。

## 2. 唯一生产写白名单

```text
Config/FightMatch/Luban/luban.conf
Config/FightMatch/Localization/__tables__.csv
Config/FightMatch/Localization/FightMatchText.csv
Tools/FightMatch.Localization.Compiler/FightMatch.Localization.Compiler.csproj
Tools/FightMatch.Localization.Compiler/Program.cs
Tools/FightMatch.Localization.Compiler/global.json
Generated/FightMatch/Localization/fm-text-v1.json
Generated/FightMatch/Localization/fm-text-v1.manifest.json
```

这些路径均在 `Assets` 外，不创建 `.meta`。禁止修改 `Assets/**`、`Packages/**`、
`ProjectSettings/**`、任何文档、旧 evidence、Git、外盘 cache/runner/SDK 或玩家资料。

## 3. 唯一新 evidence 根

只允许新建并写入：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/
```

该根内允许的闭集类别为：`inputs.json`、`commands.json`、`run.json`、
`comparison.json`、`third-party-license-inventory.json`、`self-tests/results.json`、
`negative/{duplicate,parameters,severity}/result.json`，以及 `pass-1/`、`pass-2/`
各自的 `raw/`、`raw-manifest.tsv`、`canonical/fm-text-v1.json`、
`canonical/fm-text-v1.manifest.json`、`semantic-digest.txt`、`formal-source.sha256`、
`steps/` 中每步的 `.argv.json/.exit.txt/.stdout.txt/.stderr.txt/.timing.txt`，
`compiler/` 中本次用固定 SDK Roslyn 直接编译的临时 CLI 文件，和最终
`diff-check.txt`。不得在该根外创建 bin/obj、pycache、下载或临时产品输出。

## 4. 实施合同

1. 先核所有固定 SHA/bytes/count/tool identity；不符即 `BLOCKED_INPUT_DRIFT`，
   不继续。正式 `FightMatchText.csv` 必须由接受 COPY 的 264 records 逐字迁移并
   添加设计冻结的 3 control rows；分别记录 `acceptedCopySha256` 与实测
   `formalSourceSha256`，不得把 `ea05…` 冒充正式源 SHA。
2. project CLI 只用固定 SDK Roslyn `csc.dll` 编到 evidence `compiler/`；禁止
   restore/publish/global install。三类进程调用均令完整 process argv[0]=dotnet/
   `ProcessStartInfo.FileName`，完整 argv[1..] 逐项进入 `ArgumentList`，不经 shell。
3. 连续两个全新空 raw staging 运行固定 Luban。两次 raw 相对路径/file set、逐文件
   bytes/SHA 必须相同；任一差异立即 `NON_DETERMINISTIC`，不得第三次挑结果。
4. compiler 严格核 raw 与正式源一一对应，产生 canonical JSON/manifest；两次
   canonical bytes、manifest bytes 与 semantic digest 必须相同。publish manifest
   分列 accepted COPY/formal source/canonical identities，禁止 host/time/path/raw/
   runner presentation identity。
5. 运行设计冻结的 12 项 self-test，以及 duplicate、parameters、severity 三个负例。
   parameters/severity 必须由 project gate `FMLOC007`/`FMLOC004` 拦截，三个负例
   published canonical count 都为 0。
6. 生成 `third-party-license-inventory.json`，绑定 132-file runner manifest；逐 DLL/
   package/version/file SHA/license-or-notice source SHA 完整覆盖，companion 明确归属。
   任一缺失或不匹配即 `LICENSE_INVENTORY_INCOMPLETE`，阻断候选；禁止联网补洞。
7. 只有全部门通过后，才把 pass 的 canonical 两文件复制为 §2 的两个 Generated
   候选。禁止手改 Generated；失败时保留新 evidence，不发布半成品。

## 5. 验收、停止与回传

- 264 records/unique、9 logical columns、双语/参数/severity/placeholder 和 AMEND-04
  单行 delta 全部复证；formal source 与 COPY 双身份明确。
- 两次 raw 闭集和 bytes、canonical/manifest/semantic 完全确定；manifest 无 host/raw
  身份；负例与许可证门均通过。
- `git diff --check` 只检查本包生产白名单；工作区实际 changed paths 不超白名单，
  `Assets/Packages/ProjectSettings` 零新增差异。本包不运行 Unity。
- 任一 out-of-scope 需求、identity 漂移、工具非零、确定性、license 或负例失败立即
  停止并返回稳定终态；不得换版本、下载、扩大白名单或自行签 B。
- 回传：实际 changed/new files；每项 acceptance；完整 evidence 路径与关键 SHA；
  每步 argv/exit/timing；acceptedCopy/formalSource/canonical/manifest/semantic identities；
  132-file license 完整性；diff-check；未运行项。主程审 patch 后交 QA 安排独立 R。

LOC-IMPL-A `ACCEPT` 只接收制作链候选；LOC-IMPL-B 必须另签，Windows 与发行门仍关闭。
