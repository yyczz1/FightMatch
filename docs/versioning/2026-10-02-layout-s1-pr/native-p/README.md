# LAYOUT-S1 原生预制体迁移检查点

此前 PR4 只有响应式布局源码，已有 Prefab 仍是旧层级。本增量把同一套布局通过 Unity 原生保存写入运行时 Prefab，再关闭、复开验证；保留原资源 GUID、既有对象引用及诊断文字占位符，美术为临时图形。

四个批准的交回路径中，实际三项 Git 变化：运行时 Prefab 更新，两份新 C# 的自然生成 .meta 加入；FightMatchDemo 场景字节未变。15 份 C# 保持受审 FIX03 head `ee556f2653582534016e371f3d7dce9037de40ca` 的内容。本次不改共享 ProjectSettings、Packages、游戏规则或存档语义。

已取得的实际证据：

- Mac 导入／编译启动一次，Unity exit 0，79.103 秒。原验证器误将 UNITY_ANDROID 条件程序集列为 StandaloneOSX 必需项，故原回执 I_FAILED 保留。只读补核确认 20 个适用项目 DLL 加 QFramework 均为本次新产物，日志无编译错误或警告、两个新 meta 唯一、输入和保护文件不漂移。Android 编译未验证。
- 原生迁移／保存／卸载／同进程复开启动一次，Unity exit 0，16.142 秒。原验证器因 Unity 生成隔离工程的默认 SceneTemplateSettings.json 而记 P_FAILED，原回执保留。中央批准该精确 Editor 副产物仅在隔离目录保留，补核全部 141 个文字目标、3 个输入框、场景引用与保护文件后限定接收；该设置文件不进入本 PR，也不复制到共享工程。
- 固定四路径由唯一 C 执行者机械交回，源／目标字节一致，共享保护集只有获准 Prefab 发生内容变化。导出本身没有启动 Unity。

证据局限：I 阶段旧 runner 重复收尾覆盖了终止信号记录，历史信号状态记 UNKNOWN；不能据空列表声称从未发信号。最终无遗留进程已另行核对。P 阶段改为追加收尾记录，实际为自然闭包。网络下载字节未可靠测量，记 UNKNOWN；实际按冻结依赖、耗时和缓存／生成目录大小限制执行。

本目录 JSON 是原执行证据的逐字节副本，保留原绝对路径和原失败状态；不是重新执行入口。日志等大文件留在原 TestArtifacts，身份见这些回执及补核。publication-manifest.json 绑定本次发布文件及保留的15份源码，自身除外。

尚未完成：新资源 head 的 GitHub 审查、真实测试 leaf 发现及定向执行、画面／手感、真实 Host／本地化／保存重开、Android 构建与设备验收。本检查点不是可玩 Demo 验收。
