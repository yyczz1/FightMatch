# YooAsset 下载续传事实核对

2026-09-30 · 仅官方文档／源码静态研究，未运行 Unity、下载测试或安装依赖。主 Goal 继续暂停。

## 结论与所查版本

**FightMatch 应保留已下载的有效文件，并利用 YooAsset 现有半文件续传能力，避免中断后无条件重下全部内容；不需要先自研一套 HTTP 下载器。** 但框架具备能力不等于当前工程已接入，也不等于默认开启或已经通过手机／CDN验证。

本次固定核对官方 **3.0.6** 标签（官方发布页列于2026-09-18，提交短号 `3b4cfb3`）及3.0.x文档。它是研究样本，**不是项目已选定的安装版本**；结论不直接套到2.x或自定义下载后端。[发布记录](https://github.com/tuyoogame/YooAsset/releases/tag/3.0.6)、[包版本](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/package.json)

本次实读工程 [manifest](../../../Packages/manifest.json)，并检索 [packages-lock](../../../Packages/packages-lock.json)：未声明 YooAsset。根代理另核 `Assets`／`Packages` 文件清单未找到 YooAsset 文件。本项目目前没有可据以宣称已启用续传的接入证据；此研究没有修改包或设置。

## 六个容易混淆的行为

| 行为 | 3.0.6官方代码事实与对项目的意义 |
| --- | --- |
| 完整文件缓存复用 | `IsDownloadRequired` 根据 `BundleCache.IsCached(BundleGuid)` 判断；下载操作也先检查缓存。已有有效缓存的同一资源包无需再下载。这与半文件续传是两回事，也不意味着新版本不同字节的包能复用旧半包。[SandboxFileSystem.cs L383](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/SandboxFileSystem.cs#L383)、[SFSDownloadBundleOperation.cs L44](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/SFSDownloadBundleOperation.cs#L44) |
| 半文件续传及默认值 | `ResumeDownloadMinimumSize` 默认 **`long.MaxValue`**；仅当单个 `Bundle.FileSize >=` 此阈值才走续传。普通资源文件默认实际上不启用。参数名为 `EFileSystemParameter.DownloadResumeMinimumSize`；官方文档的 `1024 * 1024`（1 MiB）是配置示例，**不是默认值，也不是FightMatch已选阈值**。[默认与设置 L109／275](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/SandboxFileSystem.cs#L109)、[分支 L48](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/Internal/DownloadAndCacheFileOperation.cs#L48)、[官方参数说明](https://www.yooasset.com/docs/guide-runtime/FileSystem) |
| HTTP Range机制 | 续传路径读取临时文件长度作为偏移，设置追加写入且中止时不删除文件；默认后端在偏移大于0时发送 `Range: bytes=<offset>-`。普通下载路径则先删除原临时文件后重新请求。文件大小达到／超过期望值的残片会被删除；续传收到416也会删残片。因此不能承诺任何失败都保留每一字节。[下载操作 L159／190](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/Internal/DownloadAndCacheFileOperation.cs#L159)、[416处理 L101](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/Internal/DownloadAndCacheFileOperation.cs#L101)、[默认HTTP实现 L38](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Services/UnityWebRequest/UnityWebRequestFile.cs#L38) |
| 关闭进程再打开 | **有条件的源码推论：可以从保留下来的磁盘半文件继续，不只支持同进程重试。** 临时路径由缓存根和BundleGuid确定，新下载操作重新读取其磁盘长度，不依赖原内存下载器。前提是同一资源身份／缓存根、续传配置仍生效、文件确实落盘且未被清理；应用重建并启动下载流程仍由项目负责。覆盖安装若选 `ClearAllCacheFiles` 会删除临时文件，不能与普通重开混同。未实测强杀、掉电、换包或真机存储行为。[临时路径 L436](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/SandboxFileSystem.cs#L436)、[重读长度 L159](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/Internal/DownloadAndCacheFileOperation.cs#L159)、[覆盖安装清理 L60](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/FileSystem/Services/SandboxFileSystem/Operations/SFSInitializeOperation.cs#L60) |
| 暂停、继续、取消 | `PauseDownload()`仅设标志，停止安排后续文件，**已经开始的文件仍会继续**；`ResumeDownload()`解除该暂停。底层调度器的暂停也明确保留进行中的请求。`CancelDownload()`把本批操作结束为失败并中止它的子下载操作；之后应重新创建下载操作，不能用Resume复活已取消批次。底层下载有共享引用，仅取消一个入口未必停止其他入口仍需要的同一请求。[DownloaderOperation.cs L199／369／385](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/ResourcePackage/Operations/DownloaderOperation.cs#L199)、[调度器 L66／121](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L66) |
| Wi-Fi切移动数据 | 所查下载与调度链不代办网络类型、玩家流量授权或确认弹窗。**不能只调用PauseDownload就承诺立即停止移动流量。** 项目需统一约束首装、后台更新及按需下载入口，在不允许移动数据时停止新请求并处理中止中的请求；Unity底层中止也不是瞬时零字节边界。[暂停源码](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/ResourcePackage/Operations/DownloaderOperation.cs#L369)、[调度器暂停](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Operations/DownloadSchedulerOperation.cs#L180)、[底层Abort说明 L160](https://github.com/tuyoogame/YooAsset/blob/3.0.6/Assets/YooAsset/Runtime/DownloadSystem/Services/UnityWebRequest/UnityWebRequestBase.cs#L160) |

## FightMatch仍需完成的接入

确定实际安装版本后，复核对应API和默认值；按实际资源包大小设置续传阈值，保存稳定的下载目标及缓存位置；用现有可信清单／完整文件校验和版本启用规则接收结果。下载量提示、重试入口、移动数据确认、取消后重建及错误提示仍由项目组织。官方[资源更新流程](https://www.yooasset.com/docs/guide-runtime/ResourceUpdate)提供下载总数、字节数、进度／错误回调和启动入口，但这些不能替代项目的流量策略和“内容全部有效后才能进入游戏”规则。

源代码表明可发送Range，不证明待选CDN已经正确支持它；CDN忽略Range、对象发生变化或残片损坏时，必须以文件校验结果处理，不把HTTP成功当资源可信。FightMatch现有[更新设计§3～7](../2026-09-16/details/update-lifecycle.md)的首次缺包重试、完整缓存离线、空间不足保留旧集合及兼容后启用继续有效。

## 后续验收条件（本次全部NOT RUN）

1. **完整文件复用与阈值：** 构造低于、等于、高于选定阈值的文件；中断后核请求与实际字节量，已缓存有效文件不重下，阈值行为与所选版本一致。
2. **半文件及进程重开：** 下载中断／取消／应用重开后，在相同资源身份和缓存根下从已有有效长度发Range；若临时文件丢失或不合法则准确重下，进度提示不虚报。普通重开与覆盖安装分别验证。
3. **CDN及校验：** 对206、忽略Range返回200、416、长度不符、损坏文件和远端版本变化逐项核对；错误数据不得成为可执行内容，重试或重下不得破坏原可用集合。
4. **流量授权与暂停：** Wi-Fi切移动数据时，首装／更新／按需下载均服从用户确定的策略；记录暂停后在途请求及实际字节，验证单纯Pause不会被误当已断网，并覆盖共享下载引用与重复继续。
5. **空间不足和中断收尾：** 暂存、写缓存和校验各阶段失败时，保留玩家资料／旧可用集合，提示所需空间及可重试状态；半包不得触发业务初始化或新版启用。

以上是接入方向与验收条件，不是SDK版本批准、产品实施派发、CDN选择或运行验收。
