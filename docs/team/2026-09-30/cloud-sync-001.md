# QA-CLOUD-SYNC-001：云端 EditMode 基线准备

2026-09-30 · 中央签发 · `APPROVED_FOR_BASELINE_SYNC`。

## 目标与责任

用户已授权本轮五角色团队持续推进。根据QA云环境只读回执，先把既有云仓库准备到已推送基线，供之后固定候选的Linux EditMode验证使用。执行者为既有云会话`01a0f13c-3d87-753a-ad11-16302add9084` / `durable`；中央统一协调本次Git变更，QA接收环境回执。

本包只准备基线，不是uGUI候选验证，不安装Android组件，不启动Unity、编译、测试或构建，不处理许可秘密，不修改配置发布状态。

## 精确范围与顺序

仓库为该会话已经核实的`/workspace/FightMatch`；不得搜索或修改其他仓库。

1. 只读复核：HEAD仍以`9d416e6`开头、当前分支`work`、`git status --porcelain`为空、origin仍指向FightMatch已配置远端。前提变化时停止本包并回传，不清理或覆盖。
2. 唯一网络获取：`git fetch --no-tags origin master`。
3. 核`FETCH_HEAD`精确等于`125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；另核`git merge-base --is-ancestor 9d416e6 125b849be13fe2ebf5b1185f3cdb83d19c6327ef`为exit0。若远端已推进或关系不符，停止并报告，不改取未经核定的最新提交。
4. 执行`git switch --detach 125b849be13fe2ebf5b1185f3cdb83d19c6327ef`。保留原work分支引用，不reset、merge、rebase、commit、push或删除分支。
5. 终核detached HEAD为准确目标、工作树干净、原work分支仍在原提交；读取根AGENTS.md、START_HERE.md确认基线入口。`docs/team/2026-09-30/`目前尚未推送，不要求它存在；其缺席不是本包失败。

允许变化仅为该仓库Git对象／远端跟踪信息、HEAD与目标提交的工作树内容；所有本机仓库和产品实现不在本包写入范围。每条网络／切换命令只尝试一次，发生错误保留现状回传；不得用破坏性命令修复。

## 回执与后续

回传真实thread/host/turn、前后HEAD与分支／干净度、每步退出码、实际结果及必要的非敏感输出。发中央`01a0e401-511d-79f2-b47f-3ab0ade1681b`和主测试`01a0f2e3-4bd2-7130-b43a-19afa702e7bb`，均为local；团队内回执授权见AGENTS第6节与用户本轮恢复指示。

此后实际uGUI源码若仍为本机WIP，云端仍未获得它。必须另给准确可传递候选、运行范围、许可preflight、结果目录、预算与回传方案后才能运行。旧基线成功同步不得写为编译／测试／新Demo通过。

## 实际回执

主测试已核云会话实际回合`01a0f2f7-9625-71ba-a6d6-1adf28a53a5c`返回`BASELINE_SYNCED`：前置work@9d416e6干净；fetch与祖先检查成功；一次切到detached `125b849be13fe2ebf5b1185f3cdb83d19c6327ef`；终态干净，原work引用未变，origin/master指向125b849。没有安装、Unity、测试、构建或许可操作。QA继续独立准备Linux目标测试模板；本包已完成基线同步职责。
