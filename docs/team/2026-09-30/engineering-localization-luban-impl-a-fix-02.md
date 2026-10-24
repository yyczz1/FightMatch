# LOC-IMPL-A-FIX-02：纠正证据身份转录并完成生产链

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`。本包只纠正 FIX-01 中一个
不完整 SHA-256 转录及其已消费 evidence root；不改变已完成的 Luban set 语法修复、
264 条业务记录、四值集合、生成合同或白名单。

## 1. owner 与固定输入

- 唯一 owner：原 LOC-A execution chat
  `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a` / `local`；返回主程
  `01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`。
- 原实施包：`engineering-localization-luban-impl-a.md` SHA-256
  `8e2626dad0d4b3c70854b9b7e22bdd48fce79ea91cececb7af45a9cd01a8487d`。
- 首次纠正包：`engineering-localization-luban-impl-a-fix-01.md` SHA-256
  `b17ff60ca4df6bbb580e96daba43792608737ca243fc5bd0573dd8663d084587`；
  它驱动的 FIX-01 run 已真实停止，不能再执行。
- 接受设计保持 SHA-256
  `022c060801df22520653856c24bd78dda394deb26f946dbff1b70ec9c338380a`，
  777 lines / 52,323 bytes。设计内原失败 `inputs.json` 已正确记录完整 64 位 SHA；
  61 字符串只来自主程 FIX-01 包／派发转录。
- COPY、005、Luban v5.1.0/commit、SDK、runner 与 license 输入全部沿 FIX-01，不重跑、
  下载、restore、publish 或换版本。

## 2. 两个 immutable 失败根

原 production failure root 保持只读：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf/
```

- `run.json` SHA-256
  `e31cbc302d357dcc0135482e284afd8816e6bbb700a6bb05c8306adfd1c93420`；
- `inputs.json` 正确 SHA-256
  `07134c3d422c534be1d0211c7cbfe3aa11a5b2ae40af0af09d3fee789fff96d2`；
- 失败 formal source SHA-256
  `9695b4501b9d6ffda53914d0a42f61948147e2bf655385fa263e7bf916348e39`。

FIX-01 consumed root 也保持只读：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-01/
```

- `run.json` SHA-256
  `19151d7f0cd9a2bfeaeddf090d267a35941722ba1cc9a163bba229c4cabc8dc4`，终态
  `BLOCKED_INPUT_DRIFT`，compiler exit 0、generate exit 21；
- `inputs.json` SHA-256
  `3852210c00e9a053691b9fd05bfc3e2642c84939e3d6088634f639873569281c`；
- `commands.json` SHA-256
  `3debf91a90e48c8c48828f97afeeb47a8dfe574d170f0825b7d3283eee67aa9b`；
- 共 15 个文件；`selfTestsStarted=false`、`lubanStarted=false`、
  `rawPassesStarted=0`、`generatedPublished=false`。

不得覆盖、续写、删除、重命名或把任一失败根当成功证据。

## 3. 唯一新根与当前候选起点

本轮只允许 create-once 新根：

```text
TestArtifacts/FightMatch/LOC-IMPL-A/ea05eae763b6facb54c3db540f52c5d63867c7de48ca08ab6455c009c871eedf-fix-02/
```

开始前它必须不存在。证据闭集沿原包／FIX-01；若已有任一文件即
`BLOCKED_EVIDENCE_EXISTS`。不得清理 FIX-01 后复用其目录。

当前停止候选已合法完成 set 修复：

- `FightMatchText.csv` SHA-256
  `1a96886aa29ed3e78211b78ffcba9cd79667f57ed0bf88daa7c8c7d18076a030`，
  type cell 是 RFC 4180 引号化的逗号集合；本轮不得再改该文件。
- `Program.cs` SHA-256
  `9c038017fbd1f04e0e85bc9fabf9614a28bdd7746b1106ae08bd18391e322bdd`，
  只含 FIX-01 已签 provenance/root/shape 改动及错误的 61 字符 inputs SHA。

其余四个既有 Config/Tools 候选保持 FIX-01 前字节；Generated 仍不存在。

## 4. 唯一生产 delta

从 §3 起点只允许修改
`Tools/FightMatch.Localization.Compiler/Program.cs`，闭集如下：

1. `ImmutableFailureInputsSha256` 精确改为 §2 的完整 64 位值；启动任何新 evidence
   或子进程前仍 `RequireHash` 原 failure run/inputs 并核其 formal-source field。
2. 保留原包与 FIX-01 包 SHA 校验；新增本 FIX-02 packet path 与
   `CurrentCorrectivePacketSha256`，其值精确取主程最后派发本文件时给出的 SHA-256，
   并在 `ValidateFixedInputs` 中 `RequireHash`。
3. 新增 FIX-01 failure root/run/inputs/commands fixed constants；创建 FIX-02 根前对
   三文件做 `RequireHash`，并核 run 中终态、exit 与全部 `false/0` 停止字段。
4. `EvidenceRelativeRoot` 精确改为 §3 的 `-fix-02` 根；保留严格 equality、file、
   symlink 门，不指向／续写旧根。
5. 新 `inputs.json.packet` 与 `run.json.packet` 精确为 `LOC-IMPL-A-FIX-02`。
   `inputs.json` 保留 base task/design/COPY/005/tool/owner，同时分列 FIX-01 packet、
   当前 FIX-02 packet、原 production failure 四身份和 FIX-01 failure 四身份。

除这些 provenance/root/status 字段外，不得改 parser、canonicalizer、negative、license、
publish、process invocation、strict gate 或业务验证行为。完整生产白名单仍是原包 8 路径；
本轮前置 delta 只有 `Program.cs`。禁止修改 `Assets/**`、`Packages/**`、
`ProjectSettings/**`、文档、`.meta`、外盘工具、Git 或两个旧根。

## 5. 完整重跑与回传

用固定 Roslyn 在 FIX-02 新根编译，从头执行 12/12 self-tests、两个 raw pass、
raw/canonical/manifest/semantic 确定性、duplicate/parameters/severity 三负例、132-file
license inventory 和 diff-check。不得复用 FIX-01 compiler，不得第三次挑结果。只有全部
通过后才生成两个 Generated 候选；失败则保留 FIX-02 根并立即停止。本包不运行 Unity。

回传 Program 精确 delta、三个根身份、新 formal/canonical/manifest/semantic SHA、
每步 argv/exit/timing、self-tests、raw 比较、负例、license、Generated、scope/diff-check
与未运行项；停止修改，不自判 `ACCEPT`。主程检查实际 patch/证据后只交原独立 R
thread `01a0e404-e8ee-7310-8388-9260babd53f1` 给出唯一 verdict。
