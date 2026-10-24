# UGUI-01-Q4-PROCESS-06：专属 Roslyn server 温和退出与 DIAG-05 恢复

2026-10-01 · 状态：`AUTHORIZED_FOR_ASSIGNED_OWNER`

## 0. 目的与授权边界

DIAG-05 的唯一 Unity 主进程已自然退出，三项 XML/log/snapshot 已完整落盘；等待到包内
360 秒上限后，唯一剩余 owned process 是该轮 Unity 编译生成、已被收养为 PPID 1 的专属
Roslyn `VBCSCompiler`。它是编译缓存 server，不是仍在工作的 Unity/test 子任务。

本包只允许对该**确切 pipe**调用 Roslyn 自带 `-shutdown` 协议，然后在确切 PID 消失后
执行 DIAG-05 已记录的四文件反向 patch。禁止 OS signal、`kill`、`pkill`、`killall`、
`dotnet build-server shutdown`、Unity 启动、产品修复或其它 process 操作。

- owner：C `01a0e404-d89d-7ab2-bece-3cd1df3fbc52 / local`。
- 签发／接收：主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b / local`。
- 用户 authority：`AGENTS.md` 第 6 节持续五角色流程；中央 AI
  `01a0e401-511d-79f2-b47f-3ab0ade1681b / local` 已授权准确范围的普通验证包装器修正。
- product candidate 保持
  `0ff4218cdc18b10927738ab30707dc478ae8d130050da94f2013e7e4aa801b91`。

## 1. 固定进程与工具身份

签包时只读 `ps`／`lsof` 已确认：

| field | fixed value |
| --- | --- |
| server PID | `98818` |
| PPID | `1` |
| argv | `dotnet exec .../VBCSCompiler.dll -pipename:CkZH4Vcyv7lfH7i1Cg6qHwHE6JygAhEaLg7pR21Axyo` |
| exact pipe | `CkZH4Vcyv7lfH7i1Cg6qHwHE6JygAhEaLg7pR21Axyo` |
| shared memory session | `/private/tmp/.dotnet/shm/session98739/` |
| exact executable | `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/NetCoreRuntime/dotnet` |
| executable bytes / SHA-256 | `130720` / `80bc0320ab5bd1e571dab987c60087d46f02c2195ab81904bb98c5a6ae5422d3` |
| exact server DLL | `/Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/DotNetSdkRoslyn/VBCSCompiler.dll` |
| DLL bytes / SHA-256 | `221696` / `d253011c6a5b4994470772b20a8d8a048b0e91540e45cb0f6a378ec8a147be9e` |
| DIAG-05 run | `q4-diagnosis-05/probe-run/run.json`; `41945` B; `d32c5e348e60f4923f7be633614d47f929c99487983f6a626d6a48bb0b0b5d1a` |

Unity PID `98741` 已 exit 2；UnityAutoQuitter PID `98802` 已自然消失；当前无任何
`Unity.app/Contents/MacOS/Unity`、UnityShaderCompiler 或 bee_backend。Roslyn 官方 compiler
server 文档说明该 server 是编译性能缓存并在 keep-alive 后退出；官方 SDK/Roslyn 记录
`VBCSCompiler` 支持 `-shutdown`，且 pipe 可精确指定。

## 2. preflight 与唯一命令

1. 只读重算本包、DIAG-05 packet/run、exact executable/DLL 身份。
2. 只读 `ps`/`lsof`：若 PID 98818 已自然消失，记录 `ALREADY_QUIESCENT`，跳过命令并进入
   §3；若仍存在，必须同时匹配 executable、DLL、exact pipe、PPID 1、session98739。
   任一 mismatch 返回 `BLOCKED_PROCESS_IDENTITY`，不得执行命令或恢复源码。
3. 再证明 Unity/AutoQuitter/ShaderCompiler/bee_backend 均不存在。若 Unity 存在，返回
   `BLOCKED_UNITY_ACTIVE`。
4. 仅当全部匹配时，运行一次且仅一次：

   ```text
   /Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/NetCoreRuntime/dotnet \
     exec \
     /Volumes/WD_BLACK_SN7100_2TB_Media/Applications/Unity/Hub/Editor/2022.3.18f1/Unity.app/Contents/DotNetSdkRoslyn/VBCSCompiler.dll \
     -pipename:CkZH4Vcyv7lfH7i1Cg6qHwHE6JygAhEaLg7pR21Axyo \
     -shutdown
   ```

   保存 exact argv、UTC、stdout/stderr/exit、调用 PID。总 timeout `<=30s`；不得设置或
   覆盖 `HOME`、`home`、`CODEX_HOME`；不得 retry、换 pipe 或改参数。
5. 最多再自然等待 30 秒，分段 `<=10s`，确认原 PID 98818 与 shutdown client 都消失。
   若 server 仍在，返回 `BLOCKED_GRACEFUL_SHUTDOWN`；禁止升级到 signal。

## 3. 源码恢复与证据

只有 `ALREADY_QUIESCENT` 或 graceful shutdown exit 0 且原 PID 已消失，才允许按
DIAG-05 `inverse-operations.json` 执行确切反向 patch。恢复前必须匹配 DIAG-05 四个
temporary hashes；恢复后四文件必须分别为：

| path | bytes | SHA-256 |
| --- | ---: | --- |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTestData.cs` | 27,769 | `34c37acf134231045ee0752d14dc647a8567ba9b76e804b2eed546410fa6981c` |
| `Assets/Tests/EditMode/FightMatch/CandidateBoardInputTests.cs` | 42,233 | `d469f443786b22f7212c48685922faa33e7764b9277eab06ba2cf2cee7904b9d` |
| `Assets/Tests/EditMode/FightMatch/UguiSceneCompositionTests.cs` | 16,206 | `9c85a4fa10c938fa234b8ed5b55ca8753e97693adef1e049ba7129db318f7ac0` |
| `Assets/Tests/EditMode/FightMatchHost/FightMatchHostTestFixture.cs` | 21,477 | `81f3009728dc0522253a1f56e642225829e767c54e2031e2a8ee90e7cb0d79a8` |

随后重算 53-source、37-resource、旧 evidence roots equality。永久写入仅允许既有
`q4-diagnosis-05/` 内新增 `process-cleanup-06/` 与 DIAG-05 restore/final receipts；不得
覆盖 raw run/XML/log/360秒 timeout。产品、asset、meta、setting、Git 写入为 0。

## 4. 验收与回传

- [ ] exact server identity 或 already-gone 事实完整；无全局进程操作。
- [ ] 最多一次 pipe-specific `-shutdown`；无 OS signal、无 Unity launch。
- [ ] 原 360 秒 timeout 保留；shutdown 只作为后续收尾证据。
- [ ] 四测试文件精确恢复；53 source、37 resources、旧 roots 不变。
- [ ] DIAG-05 snapshot/测试结果原样；不借收尾修改诊断结论或产品。

回传本包、preflight、shutdown raw、process quiescence、restore proof、final receipt 的
path/bytes/SHA；四文件与 source/resource equality；唯一状态：
`CLEANUP_COMPLETE`、`ALREADY_QUIESCENT`、`BLOCKED_PROCESS_IDENTITY`、
`BLOCKED_UNITY_ACTIVE` 或 `BLOCKED_GRACEFUL_SHUTDOWN`。完成后停止。
