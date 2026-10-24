# Luban 常规安装与最小导表验收研究

日期：2026-10-13。责任：中央派发的只读官方资料研究；接收者：中央。范围仅为工具获取方式、5.1.0 环境/依赖及验收建议。本次没有下载归档、安装、restore、运行 Luban/Unity 或审查项目代码。

## 结论

**正常使用 Luban 可以直接获取官方预编译包，以 `dotnet Luban.dll` 调用生成器；不需要先构建 Luban 源码，更不要求先跑上游全套测试。** 官方安装页把 Release、示例自带工具和自编译列为三种可选途径；README 将 xUnit 命令单列在面向开发者的测试章节。由此可判断，项目自制 restore/文件保护/进程监护驱动属于项目附加流程，不是 Luban 的安装要求。[官方安装](https://www.datable.cn/docs/guide/install)、[官方 README](https://github.com/focus-creative-games/luban)

安装页也推荐一种同时构建主工具、Agent 和 MCP 的开发方式，但该推荐不取消 Release 选项。单纯导表不需要把 Agent/MCP 一并引入。[官方安装](https://www.datable.cn/docs/guide/install)

## 5.1.0 的可核事实

| 核对项 | 结果及证据 |
| --- | --- |
| 发布版本 | 本次查阅时官方 Latest 为 v5.1.0，发布于 2026-09-10，tag 对应短提交 df34215。[Release](https://github.com/focus-creative-games/luban/releases/tag/v5.1.0) |
| 预编译资产 | 官方列出 `Luban.7z`，页面标注 1.57 MB；另有 `Luban.Agent.7z`、`Luban.Mcp.7z`。两项 Source code 压缩包是源码，不能与 `Luban.7z` 混为一谈。[资产清单](https://github.com/focus-creative-games/luban/releases/expanded_assets/v5.1.0) |
| 官方包摘要 | `Luban.7z` 的发布页 SHA256 为 `bac9a1b8d69cfeaa7ef1d8c67b34a007324d1cd158e0b14294521b3879ba5213`。本次只读元数据，未下载或核验本地字节。[资产清单](https://github.com/focus-creative-games/luban/releases/expanded_assets/v5.1.0) |
| .NET | 官方安装文档写 SDK 8.0 或更高；固定 tag 的主项目实际目标为 `net8.0`。最直接的环境基线是 .NET 8；不能仅因存在更高版本 SDK 就省略目标运行时核对。[安装](https://www.datable.cn/docs/guide/install)、[v5.1.0 项目文件](https://raw.githubusercontent.com/focus-creative-games/luban/v5.1.0/src/Luban/Luban.csproj) |
| Mac Intel / Windows | Luban 声明支持 Mac、Windows、Linux；Microsoft 提供 .NET 8 macOS x64 和 Windows x64 包。因此存在对应的官方工具链路线。此结论不是本机或 Windows 实测通过；宿主 OS 支持与实际执行仍需各自证据。[Luban README](https://github.com/focus-creative-games/luban)、[Microsoft .NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) |
| 示例 | 官方 `luban_examples` 含最小 MiniTemplate 和已编译的 `Tools/Luban`；该自带工具可能较旧，因此固定 5.1.0 时应核对实际版本。[示例 README](https://github.com/focus-creative-games/luban_examples) |

## YAML 依赖与许可证未知项

v5.1.0 的主项目明确引用 `YamlDotNet.NetCore` **1.0.0**；本次读取的 `main` 也仍保留该引用。官方最新发布仍为 5.1.0，**未找到“升级到官方新版就已更换该依赖”的证据**。[v5.1.0 源码声明](https://raw.githubusercontent.com/focus-creative-games/luban/v5.1.0/src/Luban/Luban.csproj)、[main 源码声明](https://raw.githubusercontent.com/focus-creative-games/luban/main/src/Luban/Luban.csproj)、[发布列表](https://github.com/focus-creative-games/luban/releases)

源码依赖声明不等于对发布归档内部所有文件的核验。本次未打开官方 `Luban.7z`，其中具体依赖文件、许可证附件完整性、旧 YAML 包许可证来源仍未知。**采用官方预编译包能省去本地源码构建链，但不能据此宣布已有许可证问题自动解决。** 本研究不作许可证合规结论；旧问题由中央按既有材料单独核实。

已针对官方 issues 检索 YAML 与 Mac/.NET 相关内容，没有获得能证明上述依赖已获维护者修复的具体讨论。检索未命中不证明问题不存在，不用旧版 Wiki 的 .NET 5/6 指令替代 5.1.0 的明确证据。

## 建议采用的最小验收

以下是验收建议，不是新的执行授权，也不要求另建测试框架：

1. 先复用中央已核实的现有 runner、运行环境和真实导表证据。中央本轮报告外盘已有源码构建的 5.1.0 runner 与 .NET 8.0.425，并找到 10 月 1 日 264 条 CSV 已生成的记录；本研究未独立复核这些本机事实，不把它们误称为官方预编译包缓存。
2. 仅当需要重新证明环境可用时，在独立输出目录运行一次 MiniTemplate 的 `gen.sh`（Mac）或 `gen.bat`（Windows），记录工具版本、输入、命令、退出码和实际输出。官方快速上手就是先运行最小模板并查看生成文件；输出目录会被清理，必须使用专用目录。[第一次生成](https://www.datable.cn/docs/guide/first-generate)
3. 对项目真实的一张英/简中表，人工对照几个确定的 key、中文、英文与输出内容；确认行数和字符未损坏。若现有 264 条证据已经覆盖相同输入与要求，就直接核收，不重跑。需要补错表拒绝证据时，只对副本制造一个明确字段类型错误，检查报错定位。此项为本研究针对项目的建议；官方也用新增一张 Excel/CSV 表、声明 schema、再生成作为下一步。[加一张表并导出](https://www.datable.cn/docs/guide/add-table)
4. 每个平台只声称已经观察到的结果。人工查表、终端输出和生成文件足以构成小范围工具验收记录；它们不代替游戏运行时、设备验收，也不使未知许可证项变成通过。

建议中央将“工具已经能导表”“第三方许可证材料”“项目运行时接入”分别按实际缺口处理。没有依据继续把 Luban 的正常使用绑在自制监护驱动及上游全测试通过上，也没有依据为此自动升级工具、下载新包或再启动旧 restore 流程。

## 本轮项目核对与纠偏

中央接回主程回合 `01a1335e-67b7-7482-820b-8cc0e44adb82` 的实际只读检查：工具 `exec-5d3d4fd3-471c-4cc2-aee3-683df6e801dc` 将当前 `Config/FightMatch/Localization/FightMatchText.csv` 与既有 `LOC-IMPL-A/...-fix-02/pass-1/raw/fm-text-v1.json` 逐字段比较，均为 264 条、264 个唯一 key，所有字段及顺序相同；没有运行 Luban 或新增还原。该结果支持复用这份导表输出，不代表运行时或许可证验收通过。

用户本轮要求停止继续堆叠自制验收、改用常规做法。主程已确认 LOC 回合 `01a13358-c501-72f3-8eb8-2d8b40e6ff43` 结束，无在途工具/进程，FIX11 未补跑或封存。旧 M17/FIX11 不再是默认接续路线；既有文件与失败记录保留。主程在 `engineering.md` 顶部维护简短标准执行方案，中央不再另造平行验收框架。
