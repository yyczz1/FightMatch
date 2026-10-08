# QA-CLOUD-NUGET-SIG-VERIFY-004-A01：首次启动写入隔离补充

2026-10-01（Asia/Shanghai） · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 1. Authority 与适用边界

本补充只适用于已授权的 `QA-CLOUD-NUGET-SIG-VERIFY-004`，其固定基线为：

- packet：`docs/team/2026-09-30/testing-loc-license-alt-signature-linux-004.md`
- bytes：`15275`
- lines：`279`
- SHA-256：`e9e8e9771052e0dd0fd40897487d3fc218d67401c363accdc9416fdbd23a2794`
- assigned owner：durable QA cloud thread
  `01a0f13c-3d87-753a-ad11-16302add9084`
- receiving QA lead：`01a0f2e3-4bd2-7130-b43a-19afa702e7bb` / `local`
- engineering sign-off：`01a0f2e3-1a80-7671-a459-38d5c8de0e6b` / `local`
- central authority：`01a0e401-511d-79f2-b47f-3ab0ade1681b` / `local`

中央 AI 在 004 已完成固定 SDK 下载、但尚未执行任何 SDK 命令时，指出 .NET 首次启动
仍可能尝试生成 ASP.NET 开发证书或修改全局工具搜索路径。本补充只消除这两个与验签
无关的副作用；不改变 004 的 URL、版本、RID、bytes/hash、网络预算、验签命令、结果
分类、写白名单、HOME 禁令或独立 R 合同。

## 2. 唯一允许的执行修正

在 004 第 4 步 SDK archive 身份及安全检查通过后、第一次调用
`<root>/sdk/dotnet` 之前，把下列两项加入 004 第 5 节定义的**同一组仅对子进程生效的
环境变量**，并用于 `--info`、`nuget verify --help` 与正式 `nuget verify --all` 三次
调用：

```text
DOTNET_GENERATE_ASPNET_CERTIFICATE=0
DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0
```

004 已列出的 `DOTNET_CLI_HOME`、五个 `NUGET_*`/`TMPDIR` 隔离目录、telemetry、
multilevel lookup 与 no-logo 设置保持不变。`DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1` 可原样
保留以维持 argv/env 记录稳定，但不得把它当作当前 .NET 8 的有效隔离控制；本补充的两项
显式禁用才是新增门。

仍然禁止设置、覆盖或导出 `HOME`、`home`、`CODEX_HOME`，禁止写 shell/profile、真实
用户工具目录、trust store 或 root 外任何路径。三次调用的 exact environment 必须保存
为证据，并在每次调用后继续执行 004 规定的真实用户路径 before/after 比较。

## 3. 准确续行与停止条件

- 已按 004 fixed URL 完成且通过固定 bytes/SHA-512 门的 SDK 下载可复用；不得为了本补充
  重发 HEAD/GET、增加 request 数或改写既有网络记录。
- 若任何 SDK invocation 已在收到本补充前开始，则不得倒写或伪造环境；保留真实结果，
  按 004 的最早失败门停止并回传。
- 若尚未调用 SDK，则必须先把本补充、其最终 hash 与激活回执写入 004 的
  `authority/`，再按本节新增环境继续原 004 第 5 步。
- 若无法证明新增两项只对子进程生效，返回 `BLOCKED_UNISOLATED_TOOL_WRITE`；不得通过
  修改 HOME、系统配置、证书库或全局安装补救。

除上述两项外，004 全文继续生效。执行完成仍只回原独立 R；不得自行把 package、M、
LOC-A、LOC-B 或 LOC-L 判定为通过。
