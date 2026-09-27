# FightMatch：Mac mini 开发迁移交接

本次保存同一 FightMatch 项目的最新开发进度。Windows 开发已停止，迁移后等待用户明确恢复。Demo 与 Android 共用项目、代码和产品流程；Demo 尚未完成。

**没有旧会话时，先读根目录 [START_HERE.md](../../../START_HERE.md)。** 其中提供系统设计、架构、策划三个接续入口，以及可直接发给 Mac 新会话的完整启动消息。代码迁移快照 `4be170fd65051eeac5a523817f534e725526c4c6` 已推到既有 GitHub；本次追加上下文不要求重新克隆，已有仓库正常 pull 即可获取。

本次另补回内容策划工作树中的21份原文/CSV，[来源索引](content-source-index.json)列出路径与SHA；[会话回执摘录](conversation-receipts.json)保留三条选定正式最终答复及其历史解释。这是可移植的项目上下文，不是导入 Codex 原生聊天记录。历史候选、待答和旧设备路径以根入口说明及后续实际登记为准。

## 获取项目

- 远程：`origin`，GitHub 仓库 `https://github.com/yyczz1/FightMatch.git`
- 分支：`master`
- Windows 使用 Git `2.53.0.windows.2`；Mac 使用本机 Git 即可。

在 Mac 的目标父目录执行：

```sh
git clone --branch master https://github.com/yyczz1/FightMatch.git FightMatch
cd FightMatch
git log -3 --oneline
git status --short
```

若已克隆该仓库，先保留自己的本地改动，再在 `master` 执行 `git pull --ff-only origin master`。私有仓库需要使用有访问权限的 GitHub 账号认证。

## 当前版本与恢复入口

- 已接收代码基线：`ce21901b7b5b42bfef7ef34eb4f46155ddbf9353`。
- 首次草稿保存：`ade498c9aac96c4e00c50d90d1159f380a185571`。
- 最新分支还保存 CONT-C-RCV1 停改时的 5 项源码变化。这部分是 WIP，未编译、未测试，不能当作可运行 Demo。
- 正式停改交接：[demo-cont-c-rcv1-delivery.md](../../system-design/2026-09-17/demo-cont-c-rcv1-delivery.md)。准确 C turn 为 `01a0e3b9-0fa6-7183-8406-8b1a116e5c56`，于 `2026-09-27T16:48:59.077Z` 完成停改交接。
- 剩余工作为 CONT-C、028、029。恢复时先读 [session-plan.md](../../system-design/2026-09-16/session-plan.md) 和任务包 §376～377，不能因旧 §374 写着可实施就自行恢复。

只查看原已接收基线时，可执行 `git switch --detach ce21901b7b5b42bfef7ef34eb4f46155ddbf9353`；返回最新开发进度执行 `git switch master`。切换前先处理自己尚未保存的工作。

## 交接证据

本目录 `continuation-evidence.zip` 保存 126 个原字节文件：CONT-B-CODE-C1 76 个、CONT-C 22 个、CONT-C-RCV1 28 个。SHA256 为 `3f984a8cffacbe7a3f7f5040dc0225af36363634c3165f4fc684c7be34062808`；逐文件身份见 [evidence-index.json](evidence-index.json)。所有压缩条目的长度和 SHA256 已与原文件核对。

在新克隆的仓库根目录恢复证据：

```sh
shasum -a 256 docs/migration/2026-09-28/continuation-evidence.zip
unzip -n docs/migration/2026-09-28/continuation-evidence.zip -d .
```

证据会恢复到原 `TestArtifacts/FMDemoCONT/` 子目录，`-n` 保留已有文件。该包包含当前续接及最近已验收阶段的证据，不是全部历史原始验证材料；其余历史材料仍在 Windows 原目录，引用旧证据前须确认实物可用。Git 中的正式设计、交付、范围和审查报告已一并保留。

## Unity 与验证边界

使用 `ProjectSettings/ProjectVersion.txt` 指定的 Unity **2022.3.18f1**。Windows `Library/`、`Temp/`、`UserSettings/`、本机权限改动和真实玩家存档未纳入迁移提交。Unity 在 Mac 上自行生成平台缓存。

当前 `Tools/Invoke-FM025P2Validation.ps1` 含固定 Windows 工程和 Editor 路径，不能直接作为 Mac 验证命令；恢复开发时先安排所需路径适配。Mac 导入、编译、Android 构建与设备验收尚未验证。本次只做停改、版本和迁移检查，没有重跑原 3872 项测试，也没有把 Git 校验当作产品验收。
