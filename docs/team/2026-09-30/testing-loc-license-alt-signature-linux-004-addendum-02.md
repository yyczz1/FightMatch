# QA-CLOUD-NUGET-SIG-VERIFY-004-A02：准确续行与证据根预算补充

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 1. 固定输入与 supersession

本补充绑定：

- 004 基包：`testing-loc-license-alt-signature-linux-004.md`，279 lines／15275 bytes／
  SHA-256 `e9e8e9771052e0dd0fd40897487d3fc218d67401c363accdc9416fdbd23a2794`；
- A01：`testing-loc-license-alt-signature-linux-004-addendum-01.md`，57 lines／3093 bytes／
  SHA-256 `8f42f5aa484cec687df5a4aa285ea73f75c4567f04a59a0c48a09f2982eae48d`；
- durable owner turn：`01a0f4c9-2606-74be-b8a3-59dcf1cc81c3`；
- engineering owner：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`；
- central authority：`01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local`。

A02 supersedes A01 第 3 节“任何 SDK invocation 已开始即停止”的过宽规则。A01 其余边界
继续有效。不得改写、删除或伪造 A01 抵达前已经发生的真实命令与环境证据。

## 2. 已发生事实与准确续行

A01 抵达前，004 已完成且必须原样保留：

- fixed SDK HEAD/GET；body `216833108` bytes，固定 SHA-512 相等；
- archive safety manifest：`5559` entries／`4704` files，无危险成员；
- 固定 `<root>/sdk/dotnet --info` 一次，exit `0`，SDK `8.0.425`、RID `linux-x64`；
- 该命令后真实 `~/.dotnet/`、`~/.nuget/` 仍 absent，既有 profiles 身份未变。

这些事实满足原 004 对首个 SDK 命令的身份与隔离门。不得重发 SDK 网络、不得重解压、
不得重跑 `--info`，也不得倒写它当时未包含的环境变量。

尚未执行的 `nuget verify --help` 与正式 `nuget verify --all` 必须在原 004 的 per-process
隔离环境上再加入：

```text
DOTNET_GENERATE_ASPNET_CERTIFICATE=0
DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0
```

每个剩余命令保存真实 exact environment，并继续执行原 004 的真实用户路径 before/after
比较。仍禁止设置或覆盖 `HOME`、`home`、`CODEX_HOME`；仍禁止写 profile、真实用户工具
目录、trust store 或任务 root 外路径。若新增变量无法只对子进程生效，按
`BLOCKED_UNISOLATED_TOOL_WRITE` 停止。

## 3. 专用证据根预算修正

固定 SDK 下载并安全展开后，004 root 的实测大小为 `808936369` bytes；这比原
`768 MiB`（`805306368` bytes）上限多 `3630001` bytes。该超量来自已通过身份门的官方
SDK archive 与其 task-local 展开副本，不是额外网络、cache 泄漏或 scope 扩张。

因此仅把本次 004 create-once root 的最终上限修订为：

```text
832 MiB = 872415232 bytes
```

原执行前及最终 free-space `>=2 GiB` 门继续有效。不得借此增加 URL、request、SDK 副本、
重下载、安装、缓存位置或其它产物；不得删除失败证据、raw stdout/stderr、网络身份材料、
archive manifest、package/signature bytes 或 verify 证据来制造合规。若最终 root 超过
`872415232` bytes，停止并回传实际大小，不再上调或清理。

## 4. 不变边界

原 004 的 package URL/hash、signature entry identity、最多剩余的一次 package HEAD/GET、
唯一 help/verify 次数、verify argv、timeout、结果分类、保护面、Git/Unity/产品零写入与原
独立 R 合同全部不变。完成后只回原独立 R；执行者不得自行判定 package、M 或 LOC-A 通过。
