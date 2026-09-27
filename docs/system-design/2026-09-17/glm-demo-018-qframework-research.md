# FM-DEMO-018-GLM-R1 · QFramework 最小接入范围与版本证据核对

2026-09-22 · GLM 外部研究交付 · 收件对象：SD00。研究性质：不构成第018包实施、导入或验收授权；Unity、导入、编译、测试一律 NOT RUN。

## 1. STATUS：COMPLETED

版本、最小文件集合、许可证、程序集边界均已取得固定 commit 证据；遗留决策项见 §8 BLOCKER（均不阻塞研究结论本身）。

## 2. 结论摘要

- **推荐候选**：tag `v1.0.246-Unity2018Compatible`，commit `64cb5397c3a4497d99f1c82a5730e7444ba3d992`（2026-05-27，同日发布 GitHub Release）。这是当前最新的**带 tag 且发布 Release** 的版本。master 在其后另有 25 个提交（含 v1.0.247～257 的变更，均未打 tag），属移动引用，不作最终证据。
- **备选（确有必要时）**：master HEAD `ab6611a66d9b6161b9103c1ecaa6a65c4f0291e9`（2026-09-20）。相对 tag 的核心文件 QFramework.cs 差异仅为：`RegisterSystem/Model/Utility` 改为返回注册实例（2026-08-12 变更）＋一处错别字修复；v1.0.247 的 WebGL 构建修复只改 Toolkits（BuildKit/FluentAPI），不影响最小集合。是否需要由 SD00 决定。
- **最小接入集合**：上游自身就是完整且独立的最小模块——`QFramework.Unity2018+/Assets/QFramework/Framework/Scripts/` 下的 `QFramework.cs`（960 行单文件核心）＋ `QFramework.asmdef`（`{"name":"QFramework"}`）＋ `link.xml`（IL2CPP 裁剪保留）及其 .meta。**无需自行删改上游实现来凑最小**。
- **许可证**：MIT（仓库根 LICENSE，Copyright (c) 2023 凉鞋；QFramework.cs 头部署 2015~2025 liangxiegame MIT License）。最小集合不含第三方代码；Toolkits 含外部贡献代码但不引入。
- **官方兼容声明**：固定 commit 的 README 写明运行环境 **Unity 2018.4.x ~ Unity 6.x**，覆盖本项目 2022.3.18f1；源码静态核对未发现 2022.3 不可用 API。
- **能力边界（关键）**：核心文件的 Command/Query/Event 全部为**调用线程同步执行**，无持久队列、无事务、无幂等、无主线程切换（960 行全文核对）。第018包的应用办理队列、OperationId 防重、写门、保存后发布全部由本项目实现；QFramework 只提供客户端组织结构。
- **剩余缺口**：导入编译验证未做（NOT RUN）；Assets 内具体引入路径与 app 层 asmdef 引用关系归第018包精确包决定。

## 3. 来源与输入记录

### 3.1 本地实际读取（2026-09-22）

| 路径 | 核验结果 |
| --- | --- |
| AGENTS.md | 会话注入读取 |
| Packages/manifest.json | 无 QFramework 依赖声明 |
| Packages/packages-lock.json | 50 项全部为 com.unity.*，无 QFramework／第三方包 |
| ProjectSettings/ProjectVersion.txt | 2022.3.18f1 (d29bea25151d) |
| FightMatch.Core.asmdef | 引用 FlowPuzzle.Core；noEngineReferences=true |
| FightMatch.Platform.asmdef | 引用 FightMatch.Core；noEngineReferences=true |
| FightMatch.Core.Tests.asmdef | 引用 Core/Platform/FlowPuzzle.Core/Validation；Editor only；noEngineReferences=true |
| docs/system-design/2026-09-17/system-task-packets.md §76.2/76.4/§77 | 018 包定义（前置017）；发包前交接项"客户端依赖与接线" |
| docs/system-design/2026-09-16/details/application-flow.md | 全文（M02 单队列、三种等待、恢复流程） |
| docs/system-design/2026-09-16/details/input-presentation.md | §1～§5（M01 语义入口、令牌与门闩） |
| docs/architecture/2026-09-16/c4/r2/README.md | 客户端职责、热更分层（QFramework 业务 System/Model/Command 优先放热更层） |
| Assets/、Packages/ 目录＋Glob/Grep `QFramework` | 无任何 QFramework 文件命中（Library 外全部源码范围） |

白名单内未读：`.agent/` 各规则文件、system-task-packets.md 其余历史包。六月 PROJECT_CONTEXT.md 按任务指示不作为现状依据。

### 3.2 官方来源（访问日期 2026-09-22）

| 来源 | 关键内容 |
| --- | --- |
| github.com/liangxiegame/QFramework（作者 liangxiegame，主页 qframework.cn） | MIT；默认分支 master；README：运行环境 Unity 2018.4.x ~ Unity 6.x；最小安装方式为"直接复制 QFramework.cs 到自己项目中的任意脚本中" |
| tag v1.0.246-Unity2018Compatible / Release | commit `64cb5397...`，2026-05-27；Release 资产 QFramework.cs（28069B）与仓库根目录副本同码，和 Framework/Scripts 副本仅头部注释不同、代码一致 |
| GitHub API git/trees（recursive，truncated=false，2749 项） | 固定 commit 的完整文件清单与目录结构 |
| api.github.com compare 64cb5397...master | master 领先 25 提交（v1.0.247～257、QFramework.cs 错别字修复） |
| qframework.cn | 官方文档站；当前首页分发 v1.0.257 unitypackage（移动版本，未采用） |

哈希取得方式：对每个文件用 raw.githubusercontent.com 与 api.github.com git/blobs 两条独立通道各取原始字节，SHA-256 完全一致后方写入 §4。

## 4. 最小文件与依赖清单

固定 commit `64cb5397c3a4497d99f1c82a5730e7444ba3d992`，基路径 `QFramework.Unity2018+/Assets/QFramework/`。固定链接规则：`https://github.com/liangxiegame/QFramework/blob/64cb5397c3a4497d99f1c82a5730e7444ba3d992/<路径>`（raw 为 `/raw/` 前缀）。

| 文件 | 字节 | SHA-256 | 用途与依赖 |
| --- | ---: | --- | --- |
| Framework/Scripts/QFramework.cs | 28212 | 8ad2e7385a334b7be7da17a5f1c8da1bfd6ae2ef141ec658fb506c1e71421797 | 单文件框架核心（类型清单见 §5.1）；仅依赖 BCL＋UnityEngine＋UnityEngine.SceneManagement，Unity 部分全部在 UNITY_5_6_OR_NEWER／UNITY_EDITOR 条件编译内；不依赖任何其他 QFramework 文件 |
| Framework/Scripts/QFramework.asmdef | 29 | f9c9d43d25712f15175b43570b487781d972256c740401ecc638e7bcac0a6604 | 程序集定义 `{"name":"QFramework"}`：无 references、无平台限制，只引引擎程序集 |
| Framework/Scripts/link.xml | 69 | e8c043d01d995c9cb54d728b10280d0a20223d2455060946357f0ebc10b2ed0d | IL2CPP 托管裁剪时保留 QFramework 程序集（Android 后续相关） |
| Framework/Scripts/QFramework.cs.meta | 243 | 1d43095bdab8127bf9889b586f2f45be6d5eea6e9f772b46639f7884bcfeeb69 | 保留上游 GUID `7a1fba0c3bb80422c82f18ba7b548eda` |
| Framework/Scripts/QFramework.asmdef.meta | 166 | c8e274d8c9e078cb906added52179ae36b2ffa23a6267e588088c78b02a5988d | 保留上游 GUID `9e97e42c28a41430880bee376c274c86` |
| Framework/Scripts/link.xml.meta | 158 | 51f2a2b64bde61a0ff8aa96bb41fa4f7d7ff5fda7ad715d604f90c69641c0983 | 保留上游 GUID `32878c59179e84e84ab2d2132693f8f4` |
| Framework.meta（如按上游目录结构引入） | 172 | 427ace19f49f0d4e7592d1851b368fdc320d447149f3556728148a3e7540b76b | 目录 meta（guid `2c64798740b034274bbe7de863517ca1`） |
| Framework/Scripts.meta（同上） | 172 | 7e512b03a8a7f50335733401dc0426f1e472cec39924dfd05d48f2ac8a5cfe90 | 目录 meta（guid `2b3ecb0221ffc4967908529774091b89`） |
| LICENSE（随引入保留） | 1084 | 6a4cd471d0116635d6f11075d0688e331046665035f7afef99f5f5a3a98f10ec | MIT License，Copyright (c) 2023 凉鞋 |
| Framework/PackageVersion.json（可选版本标记，非编译必需） | 598 | 0dc79bf3a3262992f5aff186b28be10f712770763b00396bbe24da06ee453589 | 上游 PackageKit 的版本元数据（v1.0.246）；不引入 PackageKit 则无功能作用 |

**依赖闭合性**：QFramework.cs 即上游官方的最小安装单位（README 安装节原文）。文件内无对 Toolkits 或其他文件的引用；asmdef 无 references；link.xml 独立生效。**非必须（不引入）**：Toolkits 全部（UIKit、ResKit、AudioKit、SupportOldQF、_CoreKit 的 ActionKit/BindableKit/BuildKit/CodeGenKit/ConsoleKit/DeclareKit/EventKit/FSMKit/FluentAPI/GraphKit/GridKit/IOCKit/JsonKit/LiveCodingKit/LocaleKit/LogKit/PackageKit/PoolKit/ScriptKit/SingletonKit/TableKit/ZipKit 及 Internal）、Examples unitypackage、Doc.md、根目录 QFramework.cs（Godot／旧头注释变体）、qframework.cn 的 v1.0.257 unitypackage。**无 DLL、无包依赖、无生成步骤、无 Editor 初始化要求**（编辑器侧仅自动出现一个 "QFramework/Install QFrameworkWithToolKits" 菜单项，作用是打开 URL，无其他副作用）。UPM git URL 安装在该 commit **不可用**：Assets/QFramework 全树无 package.json（API 核对不存在）。

## 5. 能力与责任映射

### 5.1 QFramework.cs 实际提供的能力（固定 commit 源码，960 行全文已读）

| 能力 | 上游类型（行号区间） | 实际行为 |
| --- | --- | --- |
| 客户端组织 | `IArchitecture`／`Architecture<T>`（41～198） | 每个子类一个静态单例；`Interface` 首次访问惰性初始化，或显式 `InitArchitecture()` |
| 命令 | `ICommand`／`ICommand<TResult>`／`AbstractCommand`（289～328） | `SendCommand` 在**调用线程同步执行**；无队列、无撤销 |
| 查询 | `IQuery<TResult>`／`AbstractQuery`（332～352） | 同步执行并返回结果 |
| 模型/系统/工具注册 | `IModel`/`AbstractModel`（254～277）、`ISystem`/`AbstractSystem`（225～250）、`IUtility`＋`IOCContainer`（639～673） | 架构已 init 后注册会立即 Init；**同类型重复注册静默覆盖旧实例** |
| 事件通知 | `TypeEventSystem`（616～632，含静态 `Global`）、`EasyEvent` 族（792～873）、`OrEvent` | `Trigger` 直接 `Action.Invoke`，**发送线程同步回调**；无锁、无跨线程派发、无持久化 |
| 可绑定属性 | `BindableProperty<T>`（697～781） | 值变化触发同步事件；`Comparer` 为**静态**字段；`ComparerAutoRegister` 以 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 每次进入 Play 自动注册常用类型比较器（754～781） |
| 自动注销 | `UnRegisterWhenGameObjectDestroyed/Disabled/CurrentSceneUnloaded`（498～566） | 依附 GameObject 生命周期；场景卸载触发器是**隐藏的 DontDestroyOnLoad 常驻 GameObject**（静态 `mDefault`） |

### 5.2 生命周期与交接要点（基于真实源码）

- **初始化顺序**：`InitArchitecture()`（89～113）＝ new T() → 用户重写的 `Init()` 注册 → `OnRegisterPatch` → 全部未初始化 IModel.Init() → 全部 ISystem.Init() → `mInited=true`。
- **释放**：`Deinit()`（117～124）＝ OnDeinit → System.Deinit → Model.Deinit → 容器清空 → 静态实例置 null，之后可再次 Init。但架构实例内的 TypeEventSystem 订阅**不被 Deinit 显式清理**（随实例丢弃）；静态 `TypeEventSystem.Global` **永不清理**；`OnRegisterPatch` 静态委托也不复位。
- **重复注册／重复初始化**：`InitArchitecture` 有 null 守卫；IOC 同类型注册直接覆盖；`RegisterEvent` 为 `Action +=`，同一处理器注册两次会回调两次，`UnRegister` 只移除一次。
- **场景切换／关闭 Domain Reload 风险（需后续验证）**：`Architecture<T>.mArchitecture`、`TypeEventSystem.Global`、场景卸载触发器 `mDefault`、`BindableProperty<T>.Comparer` 均为静态；编辑器关闭 Domain Reload 时跨 Play 会话残留，存在旧状态与重复订阅风险。
- **线程**：全程无锁；事件与命令均在调用者线程执行。M01→M02 若涉及非主线程，主线程切换必须由本项目实现。

### 5.3 业务要求 → 框架能力／本项目职责

| 已定业务要求（不修改） | QFramework 提供 | 本项目承担 |
| --- | --- | --- |
| 1 单应用队列组织本机写入 | Architecture 把 M02 组织为 System／Command 入口 | 队列本体、串行临界区、Busy／拒绝语义 |
| 2 UI 不直接扣血发奖、读已提交状态 | Command/Query 分离、IController 约定 | 唯一写入入口约束、演出门闩 |
| 3 候选→保存确认→发布 | 无（SendCommand 同步即返，非事务） | M02 收候选、M12 Commit、H10 后一次发布 |
| 4 SaveFailed 保旧态／Unknown 封写 | 无 | M12 结果分类＋M02 写门 |
| 5 同 OperationId 幂等、异意图冲突 | 无 | 原操作记录、同 ID 异意图拒绝 |
| 6 页面/动画不构成成功、旧回调不解锁 | 仅订阅生命周期清理触发器 | 令牌（AttemptId＋SceneRevision＋OperationId）匹配 |
| 7 Core/Platform 不增 Unity/QF 依赖 | QFramework 为独立 asmdef，不被引用即零影响 | app 层 asmdef 显式加 "QFramework" 引用；Core/Platform/Tests 不动 |
| 8 框架机制≠队列/事务/幂等/主线程切换 | **已查证：均不存在**（§5.1 行号级核对） | 全部由本项目应用层实现 |

## 6. 静态兼容结论与后续验证

| 项 | 证据等级 | 内容 |
| --- | --- | --- |
| 官方明确说明 | 固定 commit README | 运行环境 Unity 2018.4.x ~ Unity 6.x |
| 源码静态判断 | 960 行全文 | C# 特性不超过 7.1（局部函数、表达式体成员、default 参数字面量）；所用 Unity API（GameObject/MonoBehaviour/AddComponent/DontDestroyOnLoad/HideFlags/SceneManager.sceneUnloaded/RuntimeInitializeOnLoadMethod/编辑器 MenuItem/Vector 族与 Color 族比较器）在 2022.3 均存在；`UNITY_5_6_OR_NEWER` 在 2022.3 已定义 |
| 程序集 | asmdef 实读 | 独立程序集 QFramework，仅引擎依赖；Core/Platform（noEngineReferences=true）不引用它则结构上互不影响 |
| 仍需实际验证 | NOT RUN | 导入、编译、运行、测试均未执行；"源码看起来兼容"不等于已编译通过 |
| Android/IL2CPP/热更 | 仅记录待验证项 | link.xml 已随最小集保留（裁剪保护）；QFramework 程序集放 AOT 层还是热更层、与 HybridCLR 的组合留 Android 阶段设计，本次不扩展为接入或测试任务 |

## 7. ACCEPTANCE CRITERIA（A～F 逐项）

| 项 | 状态 | 证据位置 |
| --- | --- | --- |
| A 固定来源与版本 | 已完成 | §2／§3.2：官方仓库＋tag＋准确 commit；推荐一个＋备选一个；无移动引用作终证 |
| B 最小接入集合 | 已完成 | §4：3＋3 文件（目录 meta 可选）依赖闭合，双通道 SHA-256；排除清单；采用上游完整最小模块、未自行删改 |
| C 许可证与依赖 | 已完成 | §4：MIT 及保留文件；核心／附加／第三方区分（最小集无第三方代码）；无额外包/DLL/生成步骤/Editor 初始化/特定程序集配置 |
| D 静态兼容 | 已完成（静态） | §6：官方声明／静态推断／NOT RUN 三级分列；未使用 Unity 6 文档替代本项目版本依据 |
| E 应用层接入交接 | 已完成 | §5：真实源码行号级生命周期／线程／重复注册风险＋8 条要求映射；未设计新业务协议、未冻结公共 API、未编写队列代码 |
| F 后续验证清单 | 已完成 | 见下 |

### 第018包后续验证清单（框架侧与业务侧分开，不互相替代）

框架侧（只测 QFramework 本身）：
1. 按固定 commit＋§4 哈希导入文件后 `-batchmode -quit` 编译零错误；app 层 asmdef 增加引用 "QFramework" 后再编译一次。
2. `InitArchitecture`→`Deinit`→再 `Init` 的状态复位，含 `TypeEventSystem.Global` 与场景卸载触发器常驻对象的残留检查。
3. `RegisterEvent`/`UnRegister`、重复注册叠加、场景重入后的订阅清理；编辑器关闭 Domain Reload 连续两次 Play 的静态残留。
4. 确认编辑器只出现 "QFramework/Install QFrameworkWithToolKits" 菜单项，无其他编辑器副作用。

业务侧（第018包真实验收，不得以框架示例通过冒充业务通过）：
5. 事件处理器内直接 SendCommand 仍必须经 M02 唯一入口与写门，不存在绕过队列的写入路径（通知重入不绕过应用写入入口）。
6. Core/Platform/Tests 的 asmdef 与程序集引用不变（无 QFramework、无新引擎引用）。
7. 保存成功后才发布状态、SaveFailed 保旧态、CommitUnknown 封写、同 OperationId 异意图冲突——按 application-flow.md 的 AF01～24 与 input-presentation.md 的 IP01～16 见证逐项。

## 8. CHANGED FILES / VERIFICATION / SELF-CHECK / BLOCKER

**CHANGED FILES**：仅新建本文 `docs/system-design/2026-09-17/glm-demo-018-qframework-research.md`（写入前 Test-Path=False 确认不存在）。未修改任何现有文件、源码、.meta、Packages 或 ProjectSettings。

**VERIFICATION（实际执行的只读核验）**：
- GitHub API：repo 元数据、tags、releases、commits、compare、git/trees（recursive，truncated=false），均成功返回。
- 原始字节：raw.githubusercontent.com 与 api.github.com git/blobs 两条独立通道各取一次，§4 全部哈希两通道一致。
- 本地：Glob/Grep/JSON 解析确认 Assets 与 Packages 无 QFramework；manifest、packages-lock、ProjectVersion、三个 asmdef 实读。
- **NOT RUN**：Unity、导入、编译、测试、构建、安装任何依赖、Git 写操作、账号或云端操作。

**SELF-CHECK**：
- 未把"源码静态兼容"写成"已编译通过"；未把官方 README 声明当成实测结果。
- 根目录 QFramework.cs 与 Framework/Scripts 副本经逐行 diff 确认仅头部注释差异（代码一致）；Release 资产 QFramework.cs（28069B）对应根目录副本，未用其冒充 Framework 副本哈希。
- 网络抖动期间曾出现 HTTP 000 空响应，相关结果全部改由 blobs API 复核后才采信；无对网页展示文本计算哈希的情况。
- 本文为外部研究自查，不是独立代码审查 ACCEPT；不修改现有 Codex 任务模型设置。

**BLOCKER（需 SD00 决定或补充）**：
1. 候选版本终裁：推荐 v1.0.246 tag；是否改用 master HEAD（获得 Register 返回实例 API 与错别字修复）由 SD00 确认。
2. 引入路径（Assets 内目录归属）与 app 层程序集命名／引用关系，属第018包精确包内容。
3. QFramework 程序集在 HybridCLR 架构中的层归属（AOT／热更）留 Android 阶段，需与启动层设计联合决定。
4. 本机网络（代理 TUN 出口）对 raw.githubusercontent.com 不稳定；第018包下载固定文件时建议走 api.github.com git/blobs 或本地 git clone 后校验 tag commit 再取文件。
