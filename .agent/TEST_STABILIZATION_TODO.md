# Flow Puzzle 测试稳定化 TODO

目标：Unity EditMode 测试必须完整结束，不通过同步资产刷新/保存或主线程阻塞制造假通过。

## 已处理

- [x] 移除生产代码中的 `AssetDatabase.Refresh()`、`AssetDatabase.SaveAssets()` 和 `EditorUtility.SetDirty()`。
- [x] 移除测试代码中的同步资产刷新调用。
- [x] 停用会反复创建、删除 `.asset` 的旧 Persistence batchmode 集成夹具。
- [x] 用纯内存测试覆盖 JSON、canonical 数据、深拷贝、覆盖率、难度和路径/名称校验。
- [x] 移除 Editor 测试夹具每例执行的 `AssetDatabase.DeleteAsset()` SetUp/TearDown。
- [x] 删除不适合 batchmode 的 Save、Save As 和成功批量落盘测试；保留无副作用的窗口行为测试。
- [x] 修复 `LocalExactCompletionProvider` 捕获 Unity 同步上下文造成的同步等待死锁。
- [x] 修正 Tool Adapter 测试中的非法“无解”棋盘和实际无解的“可解”棋盘。
- [x] Diagnostics 重试操作栏在显示诊断时恢复布局，在普通信息/错误时隐藏。
- [x] Completion 使用编辑器输入的求解超时和节点预算，不再写死默认值。
- [x] Completion provider 异常和空结果转换为稳定的 `Error` 结果。
- [x] Tool Adapter 对输入、前缀、求解结果和生成结果执行深拷贝。
- [x] 负数棋盘尺寸的诊断建议被限制为最小有效值。

## 完成门槛

- [x] 完整 Unity EditMode 测试在独立 batchmode 进程中自然退出。
- [x] 测试结果为 0 failed、0 skipped。
- [x] `Assets/Temp` 和 `Assets/Temp.meta` 均不存在。
- [x] 活跃测试不调用 `AssetDatabase.Refresh()`、`SaveAssets()`、`CreateAsset()` 或 `DeleteAsset()`。
- [x] `git diff --check` 无错误。

## 手动冒烟（不放入 batchmode 自动测试）

- [ ] 在 Unity Editor 中执行一次 Save New，关闭并重开项目后确认资产可加载。
- [ ] 对已有资产执行 Overwrite，保存项目并重开后确认修改持久化。
- [ ] 执行 Save As 和成功批量生成，确认文件名、数量及内容正确。
