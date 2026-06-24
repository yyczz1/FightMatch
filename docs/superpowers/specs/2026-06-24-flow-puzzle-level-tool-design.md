# Flow Puzzle 自动化关卡工具设计

日期：2026-06-24  
目标 Unity 版本：2022.3.18f1

## 1. 目标与范围

在 Unity Editor 中提供一套可直接使用的 Flow Free / Numberlink 类关卡生产工具，支持两种主要工作流：

1. 根据预设或自定义参数全自动生成关卡。
2. 开发者自由放置端点、绘制可选的局部路径约束，再由求解器补全并验证关卡。

最终版包含生成、精确求解、合法性验证、难度评估、单关与批量保存、可选 JSON 导出、棋盘预览、人工创作、后台任务、失败诊断和 Undo/Redo。最终版不实现玩家运行时玩法，不引入 QFramework，也不绑定具体大模型 API。

QFramework 可在后续 Runtime 中包装并加载本设计生成的 `FlowLevelAsset`，无须修改生成、求解或验证核心。

本项目按阶段实施，但阶段划分不代表制作临时版或降低最终目标。每个阶段写出的代码都必须属于最终架构，禁止先做简单随机游走、临时 `MonoBehaviour` 或把业务逻辑堆入 `EditorWindow` 后再推倒重来。

## 2. 不可变游戏规则

- 棋盘为 `width × height` 二维网格。
- 每种颜色恰好有两个端点。
- 同色端点通过上下左右连续路径连接，禁止斜向移动。
- 单条路径不能重复经过同一格。
- 不同颜色路径不能重叠。
- 路径不能穿过其他颜色端点。
- 允许空格，不要求填满棋盘。
- 不要求唯一解，不搜索或统计全部解。
- 不因存在多个合法解而丢弃关卡。
- 推荐路径只是提示和开发数据，不是玩家唯一答案。
- 玩家完成任意合法连接全部同色端点的方案即可通关。
- 覆盖率、推荐路径长度、转弯数和难度筛选只用于关卡生产，不限制运行时走法。

## 3. 总体架构

采用适度分层：稳定的数据和计算使用具体类，真正可能变化的算法边界使用接口。避免为每个类创建单实现接口。

### 3.1 Core

负责框架无关的数据、棋盘和路径计算：

- `FlowPos`
- `FlowPairData`
- `FlowPathData`
- `FlowLevelData`
- `FlowSolutionData`
- `FlowGenerationConfig`
- `FlowDifficultyReport`
- `FlowGeneratedLevel`
- `FlowGenerationResult`
- `FlowBoard`
- `FlowPathUtility`

JSON 主字段不使用 `Dictionary`。坐标序列化统一使用 `FlowPos`，不直接使用 `Vector2Int`。

Core、Generation、Validation、Difficulty 和 Solving 保持纯 C#，不依赖 `UnityEngine`。`FlowPos` 与 `Vector2Int` 的转换放在 Editor 或 Unity 适配扩展中。Unity 类型只允许出现在 ScriptableObject 持久化和 Editor 绘制层。

### 3.2 Generation

- `IFlowRandom`：可复现随机源。
- `SystemFlowRandom`：基于指定 seed 的实现。
- `IFlowPathGenerationStrategy`：单路径生成策略。
- `RandomizedDfsPathGenerationStrategy`：随机化 DFS 回溯实现。
- `FlowPathLengthAllocator`：目标覆盖率和路径长度分配。
- `FlowSolutionGenerator`：编排完整关卡生成。
- `FlowBatchGenerator`：批量生成和报告。

自动生成器禁止调用 `IFlowPuzzleSolver` 作为主要生成方式。自动生成必须使用 solution-first 路径生成；Solver 仅用于人工布局补全、外部布局求解和未来工具调用。

### 3.3 Validation

- `FlowSolutionValidator`
- `FlowValidationResult`

只判断给定推荐解是否合法，不检查唯一解或其他可能解。

### 3.4 Difficulty

- `FlowDifficultyEvaluator`

根据给定布局计算难度，不搜索唯一解。

### 3.5 Solving / Completion

- `IFlowPuzzleSolver`：本地精确求解接口。
- `BacktrackingFlowPuzzleSolver`：约束传播和回溯求解实现。
- `IFlowLevelCompletionProvider`：Editor 使用的统一异步补全接口。
- `LocalExactCompletionProvider`：使用本地精确求解器。
- `FlowSolverToolAdapter`：未来向大模型暴露本地工具的数据契约。

未来可增加：

- `LlmCompletionProvider`
- `HybridCompletionProvider`

大模型返回的任何候选解必须通过本地 Validator。只有本地精确求解器穷尽搜索后，才允许返回严格的 `NoSolution`。

这里的“精确”只描述结果语义，不承诺任意规模棋盘都能快速求解：

- `Solved` 必须提供通过 Validator 的合法解。
- 找到第一组合法解即返回，不继续搜索第二组解，也不统计解数量。
- `NoSolution` 必须来自当前约束下搜索空间的完整排除。
- 超时、取消或节点预算耗尽必须返回 `Timeout` 或 `Cancelled`，不能误报无解。

### 3.6 Persistence / Export

- `FlowLevelAsset`：每关唯一的 ScriptableObject 主资产。
- `FlowLevelAssetRepository`：Editor 主线程中的资产创建、覆盖和保存。
- `FlowLevelExporter`：可选 JSON 导出。

### 3.7 Application

- `FlowLevelGenerationService`：生成、验证和评分门面。
- `FlowLevelCompletionService`：人工布局补全、验证和评分门面。

EditorWindow 只调用应用服务，不直接拼装底层算法。

### 3.8 Editor Draft

- `FlowLevelDraft`：Editor 内部可变编辑态。
- `FlowDraftConstraintData`：每种颜色的固定局部路径约束。
- `FlowDraftMapper`：Asset、生成结果和 Draft 之间的复制转换。

`FlowLevelDraft` 至少包含：

- `levelId`
- `width` / `height`
- `pairs`
- `fixedConstraints`
- `currentSolution`
- `currentDifficulty`
- `isSolutionDirty`
- `isValidated`
- `seed`

EditorWindow 不直接修改 `FlowLevelAsset`。工作流统一为：

```text
Asset / Generated Result
→ FlowLevelDraft
→ Edit / Complete / Validate
→ Save / Save As
→ FlowLevelAsset
```

### 3.9 Editor

- `FlowLevelGeneratorWindow`
- `FlowBoardView : VisualElement`
- UI Toolkit 参数、状态、诊断和批量结果视图
- UXML / USS 窗口布局与样式
- 参数预设与失败诊断映射

Editor 主界面使用 UI Toolkit。`FlowLevelGeneratorWindow` 通过 `CreateGUI()` 创建界面，不使用 `OnGUI()` 构建主窗口。参数、按钮、折叠区、评分、诊断、进度和批量结果使用 UI Toolkit 控件。

棋盘区域使用单个自定义 `FlowBoardView`，通过 `generateVisualContent` 和 `Painter2D` 绘制网格、端点、路径、约束和选中状态；通过 `PointerDownEvent`、`PointerMoveEvent`、`PointerUpEvent` 处理 Editor 内点击与拖拽。不得为每个棋盘格创建一个独立 `VisualElement`。

`FlowBoardView` 只负责显示、坐标换算和将输入转换为编辑意图，不直接执行生成、求解、验证、保存或修改资产。允许极少数兼容场景使用 `IMGUIContainer`，但不得把主工具退化成纯 IMGUI。

这里的棋盘交互仅用于关卡生产、预览和人工编辑，不是 Runtime 玩家拖线、断线、重连、动画或通关交互。

### 3.10 Editor Undo

人工编辑 Draft 使用 Command Pattern：

- `IFlowEditorCommand`
- `FlowEditorCommandHistory`
- 精确字段命令：放置、移动、删除端点。
- 快照命令：改变棋盘尺寸、增删颜色、应用生成结果、应用求解结果和清空 Draft。
- `DrawConstraintStrokeCommand`：将一次鼠标拖拽合并为一个可撤销操作。

复杂命令保存 Draft 的 before/after 深拷贝，减少反向操作漏字段的风险。保存到已有 ScriptableObject 时额外使用 Unity `Undo.RecordObject`，随后 `SetDirty` 和 `SaveAssets`。

### 3.11 Tests

使用独立 EditMode 测试程序集，测试 Core、Generation、Validation、Difficulty、Solving 和存储契约。

### 3.12 Assembly Definition 边界

使用独立 asmdef 强制依赖边界：

- `FlowPuzzle.Core`：不依赖其他 FlowPuzzle 程序集，不依赖 `UnityEngine`。
- `FlowPuzzle.Validation`：依赖 Core。
- `FlowPuzzle.Difficulty`：依赖 Core。
- `FlowPuzzle.Generation`：依赖 Core、Validation、Difficulty。
- `FlowPuzzle.Solving`：依赖 Core、Validation。
- `FlowPuzzle.Application`：依赖 Core、Generation、Validation、Difficulty、Solving。
- `FlowPuzzle.Persistence`：依赖 Core、Difficulty，可依赖 `UnityEngine`。
- `FlowPuzzle.Editor`：依赖 Application、Persistence 及需要绘制或编辑的业务程序集，可依赖 `UnityEditor`。
- `FlowPuzzle.Tests`：依赖被测试程序集和 Unity Test Framework。

建议文件：

```text
Assets/Scripts/FlowPuzzle/Core/FlowPuzzle.Core.asmdef
Assets/Scripts/FlowPuzzle/Validation/FlowPuzzle.Validation.asmdef
Assets/Scripts/FlowPuzzle/Difficulty/FlowPuzzle.Difficulty.asmdef
Assets/Scripts/FlowPuzzle/Generation/FlowPuzzle.Generation.asmdef
Assets/Scripts/FlowPuzzle/Solving/FlowPuzzle.Solving.asmdef
Assets/Scripts/FlowPuzzle/Application/FlowPuzzle.Application.asmdef
Assets/Scripts/FlowPuzzle/Persistence/FlowPuzzle.Persistence.asmdef
Assets/Scripts/FlowPuzzle/Editor/FlowPuzzle.Editor.asmdef
Assets/Tests/EditMode/FlowPuzzle.Tests.asmdef
```

## 4. 数据模型与持久化

### 4.1 DTO

`FlowLevelData` 只包含玩家关卡基础数据：

- `levelId`
- `width`
- `height`
- `difficulty`
- `difficultyScore`
- `List<FlowPairData> pairs`

`FlowSolutionData` 包含完整推荐路径：

- `levelId`
- `List<FlowPathData> paths`

`FlowDifficultyReport` 包含总分、难度和全部评分明细。

### 4.2 单资产设计

每关生成一个 `FlowLevelAsset`：

```text
FlowLevelAsset
├── FlowLevelData levelData
├── FlowSolutionData solutionData
├── FlowDifficultyReport difficultyReport
├── int generationSeed
└── float coverageRatio
```

默认保存位置：

```text
Assets/FlowPuzzleGenerated/Levels/Level_1001.asset
```

提示系统是必需能力，因此 Level 与 Solution 在逻辑上分离、物理上合并，避免配对资产和双重加载。

### 4.3 JSON

JSON 为可选导出：

- `level_1001.json`：只包含基础关卡和端点。
- `solution_1001.json`：包含完整推荐路径。

使用 `JsonUtility.ToJson(data, true)`。JSON 不替代 SO 主资产。

## 5. 自动生成算法

### 5.1 覆盖率

```text
coverageRatio = 推荐答案占用的不同格子数 / 棋盘总格子数
```

生成时从配置范围内选取目标覆盖率：

```text
targetCoverageRatio = random(minCoverageRatio, maxCoverageRatio)
targetUsedCellCount = round(width * height * targetCoverageRatio)
```

目标格数仅用于规划。最终只要求实际覆盖率处于配置范围，不要求精确命中目标，也不要求填满棋盘。

### 5.2 路径长度分配

`FlowPathLengthAllocator`：

1. 检查 `colorCount × minPathLength` 和 `colorCount × maxPathLength` 是否与棋盘及覆盖率范围相容。
2. 为每种颜色分配 `[minPathLength, maxPathLength]` 内的目标长度。
3. 使长度总和尽量接近目标占用格数。
4. 保留原始 `colorId`，按目标长度降序生成。
5. 无法分配时立即返回可操作诊断，不进入布局尝试。

### 5.3 随机化 DFS 回溯

每条路径：

1. 在空格中选择起点。
2. 使用 DFS 搜索目标长度。
3. 只访问棋盘内空格。
4. 候选方向由 `IFlowRandom` 洗牌。
5. 根据转弯倾向对直行和转弯候选加权排序。
6. 死路时回退并尝试其他候选。
7. 单路径超过预算才放弃本轮关卡。

先生成长路径，降低后期碎片化导致长路径无法放置的概率。

### 5.4 完整生成流程

每次关卡尝试：

1. 清空棋盘。
2. 规划目标覆盖率。
3. 分配路径长度。
4. 按长度降序生成路径。
5. 构建 `FlowLevelData` 和 `FlowSolutionData`。
6. 检查实际覆盖率。
7. 使用 Validator 验证推荐解。
8. 计算难度。
9. 若启用目标难度筛选且分数不符，则重试。
10. 返回成功结果。

达到最大关卡尝试次数后返回失败结果，不返回 `null`。

### 5.5 Seed

- 固定配置和固定 seed 必须产生相同结果。
- 随机源不能使用不受控的 `UnityEngine.Random` 全局状态。
- 所有影响生成结果的候选集合必须先转换为 `List`，按确定性规则排序，再通过 `IFlowRandom` 洗牌或加权选择。
- 禁止依赖 `Dictionary`、`HashSet` 或其他无顺序保证集合的遍历顺序决定生成结果。
- 批量生成派生公式：

```text
levelSeed = baseSeed + levelId * 9973
```

- 失败和资产中均记录实际 seed。

## 6. 精确求解与人工创作

### 6.1 人工创作能力

开发者可以：

- 自由设置棋盘尺寸和颜色数量。
- 自由放置、移动和删除端点。
- 可选绘制某种颜色的部分路径。
- 将部分路径标记为必须保留的约束。
- 加载已有 `FlowLevelAsset` 后重新编辑。
- 修改后请求系统重新补全推荐答案。
- 撤销、重做和另存为。

棋盘尺寸或颜色数量改变后，已有解标记为失效，但允许继续编辑并重新求解。

### 6.2 固定局部路径约束

最终版初始实现对人工路径约束采用以下明确规则：

- 每种颜色最多一条固定路径。
- 固定路径必须是上下左右连续、无自交、无分叉的简单链。
- 固定路径必须从一个本色端点开始，并且只能包含这一个本色端点。
- 固定路径至少包含端点和一个相邻路径格。
- 固定路径远离起始端点的一端是唯一开放端。
- 不同颜色固定路径不能重叠。
- 固定路径不能经过其他颜色端点。
- 求解器必须保留全部固定格，并从唯一开放端继续扩展到另一个本色端点。
- 开发者不绘制固定路径时，求解器直接根据两个端点完成连接，因此此方案同时包含纯端点补全。
- 悬空固定路径、同色多段固定路径、包含两个端点的完整固定路径和分叉约束不在本次最终版范围内。

### 6.3 精确求解器

求解器输入只含普通 C# 数据，不访问 Unity API。使用：

- 端点连通性预检。
- 障碍和固定路径约束检查。
- 约束传播。
- 最少候选优先。
- 路径顺序启发式。
- 回溯和死区剪枝。
- 取消令牌、超时和搜索节点预算。

求解器的基础实现保持克制：

1. 输入和固定约束验证。
2. 对每种颜色执行 BFS 连通性预检。
3. 按端点距离、可达空间和固定约束排序颜色。
4. 对当前颜色使用 DFS 枚举候选简单路径。
5. 放置一条路径后，对剩余颜色重新执行可达性剪枝。
6. 找到第一组合法解即返回，不继续搜索唯一性。

初始实现不引入 SAT/ILP、候选路径全集预生成或复杂瓶颈定理。只有实际性能数据证明必要时才增加高级剪枝。

结果状态：

- `Solved`
- `NoSolution`
- `Timeout`
- `Cancelled`
- `InvalidInput`
- `Error`

`NoSolution` 仅在搜索空间被完整排除时返回。预算耗尽返回 `Timeout`，不能误报无解。

### 6.4 后台执行

- 精确求解通过 `Task` 在后台线程执行。
- 默认超时 10 秒，可配置。
- 支持取消。
- 进度可报告搜索节点、耗时和阶段。
- Unity 对象、GUI 和 `AssetDatabase` 仅在主线程访问。
- 求解完成后回到主线程更新预览和资产。

### 6.5 Completion Provider

```csharp
public interface IFlowLevelCompletionProvider
{
    string DisplayName { get; }

    Task<FlowCompletionResult> CompleteAsync(
        FlowCompletionRequest request,
        IProgress<FlowCompletionProgress> progress,
        CancellationToken cancellationToken);
}
```

Editor 通过 Provider 下拉框选择实现，不依赖具体求解方式。

### 6.6 大模型工具契约

最终版实现接口和本地 `FlowSolverToolAdapter` 数据契约，不接入网络模型。Editor 中只显示实际可用的 `Local Exact Solver`，不会展示不可用的 LLM 选项。

未来工具能力：

- `solve_board`
- `solve_with_constraints`
- `validate_solution`
- `evaluate_difficulty`
- `analyze_failure`

推荐链路：

```text
开发者意图
→ 大模型提出或调整布局
→ 调用本地求解器工具
→ Validator 最终验证
→ 返回关卡、推荐解和解释
```

大模型负责创意和调度；本地算法负责正确性与无解证明。

## 7. Validator

必须检查：

- Level 和 Solution 非空。
- 棋盘尺寸合法。
- pair 的 colorId 唯一。
- 每个 pair 有且仅有一条对应 path。
- 不存在无对应 pair 的 path。
- 每条 path 至少两个格子。
- 首尾与对应端点匹配。
- 所有格子在棋盘内。
- 相邻格曼哈顿距离为 1。
- 单路径无重复格。
- 不同路径不重叠。
- 路径中间格不穿过其他颜色端点。

不检查：

- 是否填满棋盘。
- 是否唯一解。
- 是否存在多个解。
- 是否与玩家路线一致。

## 8. 难度评估

评分项：

- 棋盘大小。
- 颜色数量。
- 推荐路径覆盖率。
- 总转弯次数。
- 总绕路程度。
- 不同颜色路径相邻/纠缠程度。
- 同色端点曼哈顿距离。
- 局部瓶颈程度。

最终版初始权重：

```text
boardSizeScore = width * height * 0.25
colorCountScore = colorCount * 6
coverageScore = coverageRatio * 45
turnScore = totalTurnCount * 2.5
detourScore = totalDetour * 2
interactionScore = differentColorAdjacentCount * 1.5
endpointDistanceScore = totalEndpointManhattanDistance * 1.2
bottleneckScore = bottleneckCount * 1.5

totalScore =
    boardSizeScore
  + colorCountScore
  + coverageScore
  + turnScore
  + detourScore
  + interactionScore
  + endpointDistanceScore
  + bottleneckScore
```

分桶：

- Easy：`[0, 60)`
- Normal：`[60, 120)`
- Hard：`[120, 200)`
- Expert：`[200, +∞)`

预设仅填充参数。实际难度以生成或补全后的评分为准。目标难度筛选只影响生产阶段。

达到最大关卡尝试次数仍无法满足目标难度时，返回结构化失败，不继续重试。错误必须包含实际分数范围、目标范围和放宽颜色数、覆盖率、路径长度或转弯倾向的建议。

## 9. EditorWindow

菜单：

```text
Tools/Flow Puzzle/Level Generator
```

窗口主体使用 UI Toolkit：

- `CreateGUI()` 加载或构建 UXML 视觉树。
- USS 管理分栏、间距、状态颜色和响应式布局。
- `TwoPaneSplitView` 或等价 UI Toolkit 布局划分参数区与棋盘/结果区。
- `ListView` 展示批量报告。
- `ProgressBar` 展示求解进度。
- `HelpBox` 展示验证错误、失败诊断和建议。
- 自定义 `FlowBoardView` 负责棋盘绘制和 Editor 输入。

窗口必须在脚本重载后重建视觉树，并从序列化的窗口状态或 Draft 恢复必要选择状态。

### 9.1 参数

基础参数：

- Level Id
- Width / Height
- Color Count
- Min / Max Coverage Ratio
- Min / Max Path Length
- Seed / Use Random Seed
- Max Path Attempt
- Max Level Attempt
- Batch Count
- Output Folder
- Difficulty Preset
- Target Difficulty Filter 或自定义分数范围

高级参数：

- 转弯倾向
- 路径邻接/纠缠倾向
- 端点距离范围
- 绕路范围
- 瓶颈倾向
- 求解超时和节点预算

高级参数默认折叠。

路径邻接/纠缠倾向、端点距离、绕路程度和瓶颈倾向属于候选排序、评分和生成后筛选使用的软目标，不保证每次精确命中。硬约束包括棋盘尺寸、颜色数、路径长度范围、覆盖率范围、目标难度筛选以及不可变合法性规则。软目标未命中本身不构成非法关卡。

### 9.2 自动生成操作

- Apply Preset
- Generate One
- Generate Batch
- Save Current Asset
- Export Current JSON
- Validate Current
- Clear Preview

`Generate One` 只更新内存预览；保存由开发者显式触发。批量生成成功后直接保存资产。

### 9.3 人工创作操作

- New Draft
- Load Asset
- Add / Remove Color
- Place / Move / Remove Endpoint
- Draw / Erase Partial Constraint
- Complete
- Cancel Completion
- Undo / Redo
- Save
- Save As

人工修改后旧解立即标记为失效。未重新求解和验证前不能覆盖正式资产。

所有人工操作修改 `FlowLevelDraft`，不直接修改已保存资产。Undo/Redo 通过命令历史执行；鼠标拖拽绘制或擦除约束时，一次按下到松开只产生一个 Stroke Command。

### 9.4 预览与状态

显示：

- 棋盘、端点、推荐路径、固定约束和空格。
- levelId、seed、覆盖率、难度和评分明细。
- 当前 Provider。
- 求解状态、耗时、节点数和进度。
- 失败原因和参数建议。

## 10. 失败诊断

错误结果使用结构化数据：

- 错误代码。
- 人类可读消息。
- 涉及参数。
- 当前值。
- 建议修改方向。
- usedSeed。
- attemptCount。

示例诊断：

- 理论最小占用格超过棋盘容量：建议降低颜色数、最小路径长度或扩大棋盘。
- 后续长路径持续失败：建议降低覆盖率、最大路径长度或增加棋盘尺寸。
- 难度持续过高：建议减少颜色数、覆盖率、转弯或纠缠倾向。
- 精确求解超时：建议增加预算、减少颜色或移除部分固定约束。

Editor 操作：

- `Apply Suggestion`：仅用于明确、安全的单项参数调整。
- `Retry Same Seed`
- `Retry New Seed`

概率性失败只提供候选建议，不声称已确定唯一原因。

## 11. 资产写入

- 默认拒绝覆盖已有同名资产。
- 提供显式覆盖选项。
- 人工编辑支持 `Save As`。
- 正式 `FlowLevelAsset` 必须同时包含有效的 LevelData、SolutionData 和 DifficultyReport。
- 保存正式资产前必须存在未失效的推荐解，并通过 Validator。
- 未求解、求解失败或解已因编辑失效的 Draft 不能创建或覆盖正式资产。
- 本最终版不保存未完成 Draft；未来若需要保存草稿，应另建 `FlowLevelDraftAsset`，不得与正式资产混用。
- 保存时重新计算难度和覆盖率。
- `AssetDatabase`、`SaveAssets` 和 `Refresh` 仅在主线程调用。
- 批量处理中单关失败不终止后续关卡。

## 12. 分阶段实施

阶段划分用于降低集成风险，不改变最终版目标，也不允许阶段性代码成为后续必须推倒的临时实现。

### Phase 1：Final Core Foundation

- Core DTO、`FlowBoard`、`FlowPathUtility`
- 纯 C# 程序集边界
- Core EditMode 测试

### Phase 2：Final Validation + Difficulty

- Validator 与结构化结果
- DifficultyEvaluator
- 对应 EditMode 测试

### Phase 3：Final Solution-first Generator

- 可复现随机源
- 覆盖率规划和路径长度分配
- 长路径优先的随机化 DFS 回溯
- 失败结果与参数建议
- Seed 和生成器测试

### Phase 4：Final Persistence + Batch Export

- 单个 `FlowLevelAsset`
- SO Repository
- 可选双 JSON
- Batch Generator 和批量报告
- 持久化测试

### Phase 5：Final Editor Auto-generation Workflow

- 自动生成参数、预设、预览
- 单关生成、保存、批量生成
- 验证、评分和失败展示

### Phase 6：Final Draft Editing Workflow

- `FlowLevelDraft`
- 端点、棋盘和颜色编辑
- 从本色端点开始的固定简单链约束绘制
- Asset/Draft 映射
- 此阶段不实现任何临时或占位 Solver。
- `Complete` 操作在此阶段保持禁用，并明确显示将在 Phase 7 启用。

### Phase 7：Final Local Solver + Completion Provider

- `IFlowPuzzleSolver`
- 本地回溯求解器
- `IFlowLevelCompletionProvider`
- 人工布局补全
- 小棋盘求解测试

### Phase 8：Final Async + Diagnostics

- 后台 Task
- 取消、超时、节点预算和进度
- 结构化诊断与建议

### Phase 9：Final Undo / Redo

- Command History
- 精确命令、快照命令和 Stroke Command
- 资产保存时的 Unity Undo

### Phase 10：Final Verification + Tool Contract

- 补全跨模块和较大棋盘测试
- `FlowSolverToolAdapter` 数据契约
- Unity 编译、EditMode 测试和 Editor 手动验收

## 13. 测试

### 13.1 Core

- 上下左右邻接和斜向拒绝。
- 直线和 L 型转弯数。
- 移动数、绕路和重复格检测。
- `FlowBoard` 边界、占用和邻居。

### 13.2 Validator

- 合法解。
- 断路、越界、自交、跨色重叠。
- 穿过其他颜色端点。
- 缺失、重复或多余 colorId。
- 允许空格。

### 13.3 Generation

- 固定 seed 可复现。
- 5×5、6×6、7×7 基础生成。
- 实际覆盖率、路径长度和目标难度满足配置。
- 不可能配置快速失败并给出建议。
- 批量 seed 派生稳定。

### 13.4 Difficulty

- 各评分项计算。
- 阈值边界。
- 增加颜色、覆盖率、转弯等因素时相应分项上升。

### 13.5 Solver

- 已知可解布局返回 `Solved`。
- 已知无解布局返回 `NoSolution`。
- 部分约束补全。
- 非法固定约束返回 `InvalidInput`。
- 超时、取消和进度报告。
- 返回解必须通过 Validator。
- 优先使用 3×3、4×4、5×5 的确定性案例，再增加 6×6、7×7 性能案例。

### 13.6 Persistence

- 单个 SO 保存 Level、Solution、Difficulty、Seed 和覆盖率。
- Level JSON 不含完整答案。
- Solution JSON 含完整路径。

### 13.7 Editor Undo

- 端点操作可撤销和重做。
- 一次拖拽作为一个 Stroke Command。
- Resize、Remove Color 和 Apply Solution 的快照恢复完整。
- 保存资产时 Unity Undo 可恢复原资产内容。

## 14. 验收标准

- Unity 菜单可打开工具。
- 支持 Easy、Normal、Hard、Expert 和 Custom。
- 支持全部基础参数和高级约束。
- 单关可生成、预览、验证并保存单个 SO。
- 批量生成可保存资产并报告每关结果。
- 固定 seed 可复现。
- 推荐路径始终通过 Validator。
- 棋盘允许空格。
- 不做唯一解检测。
- 人工可自由放置端点和局部约束。
- 本地精确求解器可后台补全、取消、超时并严格区分无解。
- Provider 接口允许未来接入大模型。
- 本地求解器具备大模型工具适配契约。
- 自动生成器不依赖 Solver，并始终采用 solution-first。
- Editor 通过 Draft 编辑，不直接修改资产。
- Undo/Redo 使用命令历史，复杂操作使用快照。
- Editor 主界面使用 UI Toolkit，棋盘由单个自定义 `FlowBoardView` 绘制和处理指针事件。
- Editor 棋盘交互不包含任何 Runtime 玩家玩法逻辑。
- 失败信息包含可操作建议。
- 可选导出双 JSON。
- Unity 无编译错误，EditMode 测试通过。

## 15. 明确不做

- 玩家 Runtime 输入、绘线和通关 UI。
- QFramework Runtime 集成。
- 具体大模型 API、密钥管理和网络请求。
- 唯一解检测或解数量统计。
- 强制填满棋盘。
- 把推荐路径作为玩家唯一合法路线。
- 同色多段断开的固定约束或分叉约束。
- 为了阶段演示而实现后续需要推倒的临时随机游走或临时 Editor 架构。
