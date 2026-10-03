# RES-D-RAW-001：同一 lease 链的有界 raw 读取

2026-10-03 · `PREPARED / SOURCE_ACTIVATION_BLOCKED`；SDK入口可用性已作源码核对，§6仍待中央批准；本轮只增本文。

## 1. 身份、固定输入与范围

依据用户AGENTS §6、中央批准SYS准备及[CD §3](engineering-resource-runtime-bridge-cd-001.md)／SHA256 `ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`。主程唯一收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，派发turn `01a0ff8c-1b41-7262-a8be-362ff4870414`；SYS `01a0f2e6-29bb-7092-abf0-705b41b7bf93/local`，本轮 `01a0ffac-ae2d-7a72-afb6-8e20157f7556`。源码owner候选 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`、`gpt-6-astra/xhigh`，激活turn待主程绑定。
固定根：B=`TestArtifacts/FightMatch/RES-B-ADAPTER-001/FIX03/S/`，D=`TestArtifacts/FightMatch/RES-D-CODEC-001/FIX02/S/`，C=`TestArtifacts/FightMatch/RES-C-CONTRACT-001/S/`。各`source-receipt.json` SHA依次为`b422bc2f2777e103ec9015ef67ac2daace91a3f70a6b007a51d58abbbaab97f4`、`145c30289c4a8dedb6bdb8381eb4645853dec3307bad8dc4e09b2a372c8339db`、`bfca5fe3550f4f25198595de9bdc3368acbdac3122b704820ff365b8b6e5e2f2`。B沿已收FIX03/38证据；D13+C10联合执行仍在C手中，本稿不读活投影、不推断通过、不干预。
仅补`AcquireAsync<FightMatchRawBytes>`，无factory/downloader/Host/LOC或通用loader；保留B Unity对象、借用包、scope/epoch/关闭语义及C interface。原provider 165–168行明确拒raw。

## 2. SDK源码裁定：先文件长度，再payload分配

SDK只读根K=`TestArtifacts/FightMatch/RES-01A/P01/projection/Library/PackageCache/com.tuyoogame.yooasset@3b4cfb36cc/`；3.0.6／commit `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804`，以下SDK路径相对K。

- 采用`Runtime/ResourcePackage/ResourcePackage.cs:868`的`EnsureBundleFileAsync(new EnsureBundleFileOptions(location))`；同目录`Operations/EnsureBundleFileOperation.cs`返回Detail文件信息。保留operation等terminal，Detail/path仅在RealYooSdk内使用。
- 禁`LoadBundleFileAsync`：`Runtime/ResourceManager/Providers/BundleFileProvider.cs`仍加载bundle；`Runtime/BundleCache/Operations/Common/LoadLocalRawBundleOperation.cs`→`Runtime/BundleHandle/Services/RawBundleHandle/RawBundleHelper.cs`先`File.ReadAllBytes`，RawFileObject.GetBytes又复制。`Runtime/Compatibility/CompatibleHandles.cs`旧raw/path入口抛NotSupported；ReadOnlySpan也不能消除先前分配。
- `Runtime/FileSystem/Services/BuiltinFileSystem/Operations/BFSEnsurePackageBundleOperation.cs`直取内置文件，或调用解包下载；其`Internal/CopyBuiltinFileOperation.cs`本地分支`File.Copy`，默认下载分支经`Runtime/DownloadSystem/Services/UnityWebBackend/UnityWebRequestBackend.cs:79`→`Services/UnityWebRequest/UnityWebRequestFile.cs`的`DownloadHandlerFile`落盘，不先生成整份managed raw数组。`Runtime/FileSystem/Policies/DefaultBundleUnpackPolicy.cs`在Android对raw解包。
- **前提**：模块自建且完成默认Builtin初始化，默认backend、null BuiltinFileAccessor。自定义accessor分支会ReadAllBytes，故外部借用／未知配置在Ensure前拒raw，原Unity对象行为不变。虚拟／加密／非RawBundle拒绝；无解密、Editor模拟或Remote。Ready/version相同不证明配置所有权。
- Ensure成功后只接受`EBundleType.RawBundle`且未加密；在RealYooSdk内打开同一个只读`FileStream(FileShare.Read)`，先读实际Length，要求与受信expectedLength精确相等且≤§4授权限额，再分配唯一等长payload。每次推进最多64KiB到该数组并增量SHA256；拒提前EOF、额外尾字节、结束Length变化和hash错；成功前不发布，失败关闭流并丢弃数组。禁ReadAllBytes/GetBytes/ToArray、按SDK自报长度分配或“复制后限长”。FileStream小固定buffer不算整份payload。
- 默认解包根为`BuiltinFileSystem.cs:331`的`YooAssetConfiguration.GetDefaultCacheRoot`。本包不改root/建cache/运行Ensure；真实执行须另签Builtin及解包/cache根隔离，否则BLOCKED；不获得玩家目录写权。

## 3. 最小interface、信任与所有权

- A文件内新增`public sealed FightMatchRawBytes`，唯一自定义public面为`long Length { get; }`和`System.IO.Stream OpenRead()`；无public构造器、Dispose、buffer/path/SDK成员。只读Length是稳定元数据；OpenRead及stream操作受lease存活控制。A维持纯.NET/noEngineReferences。
- 同文件internal `RawBuffer`独占已验证数组；internal构造`FightMatchRawBytes(RawBuffer buffer,Func<bool> isReleased)`。operation→Entry单向转交，不按lease复制；buffer无数组出口，发布后只读、最终清空数组引用，旧view/stream不能续命正文。
- codec仅在`TryDecodePinned`已Validate的同一root上保留投影：internal `TryGetRawEntry(string assetId,out RawEntry entry)`；immutable RawEntry含assetId/set/platform/descriptorSHA/package/manifestVersion/location/physicalName/expectedLength/SHA。沿raw单文件及六业务/两text关系，不重parse、不暴露树；schema/canonical/pin/预算/public五属性/方法及D13断言不变。
- Adapter增internal `AssetMapping.TryFromPinnedRaw(FightMatchResourceReleaseSet source,FightMatchAssetId id,string expectedSet,string expectedPlatform,string builtinRoot,out AssetMapping mapping)`；错set/platform/非raw返回false/null，成功绑定投影及typeof(FightMatchRawBytes)。旧raw参数不是受信映射，保留B预算错误顺序。真实Player pin/build receipt授权后置，synthetic pin只证一致性。
- A internal投影、buffer/构造器的跨程序集权限见§6；禁重解析EncodeCanonical、reflection/public任意数组工厂、Host friend。friend为程序集级权限，不是成员级。
- 保留原`IYooSdk/IYooOperation`，Y新增internal seam：`IRawYooSdk.BeginRaw(object package,AssetMapping map,long limit,Action wake) -> IRawYooOperation`；后者继承IYooOperation，加`object Payload`、`Code? Error`、`void Pump(bool stopRequested)`。Payload仅准已验证RawBuffer，Ticket检查后接收；object使旧Tests无需新增A friend。RealYooSdk/新fake实现，raw旧Asset=null，Unity路径不改。
- Y中internal `RawReadState(Stream,long expectedLength,string expectedSha256,long limit,Func<int,byte[]> allocate)`：先核Length才allocate，每Pump≤64KiB，成功只转交一次`object Payload`（实际RawBuffer），Dispose幂等。生产等长分配／测试计数分配器共用读取逻辑；无路径入口/泛型loader，不只测fake。
- 沿B Entry/Ticket/Reservation计数及五元身份；Entry共用Ensure/buffer，每Reservation独立view，Lease.Asset仍仅主线程。每lease限一个活stream（重复OpenRead抛安全InvalidOperationException），关闭可重开、cursor独立；CanWrite=false，拒Write/SetLength，read/seek限[0,Length]，无MemoryStream/数组出口。
- stream.Dispose不释放lease；lease.Dispose任意线程幂等，立即使该view的OpenRead及stream后续Read/Seek/Position抛安全ObjectDisposedException，已进入的Read可完成；buffer锁同步最终释放，不承诺同stream并发读。A释放不影响B；最后lease主线程Drain释放buffer/Ticket一次，漏关stream不保留正文。
- 没有需“pin”的BundleFileHandle：Ensure是operation，Package.Uses中的Ticket与自有快照承担租约生命周期，不能称持有实际SDK bundle handle。失效pending不发布，等Ensure真实terminal后关闭／释放；读取阶段stop不继续分配/读取，清理后terminal。已发布lease沿B在epoch切换后仍由owner释放；CloseAsync等pending及所有lease收齐，不能提前宣称SDK已停止。Pump由原Wake/Drain合并推进，不加Task.Run、MonoBehaviour或另一路更新器。

## 4. 预算与安全结果

- 受信长度>0且≤`min(16MiB,budget.MaxRawBytes)`，占Entry额度后才Begin/Ensure；实际Length先核上限再核expected相等。跨provider pending预留＋存活payload≤64MiB，checked计数，每Entry扣/还一次；第二lease也核自身预算。失败/最终释放归还，B pending32/callback1024/lease4096及caller更小限制不变。
- 六件总≤32MiB沿codec；未来Bridge传`min(16MiB,MaxRecordBytes)`、text传LOC预算再admit，不把16MiB当LOC验收。禁buffer池/全量复制/未知长度流/无界集合。
- 继续B诊断enum，不加public错误类型：预算→BudgetExceeded；请求类型／无受信raw→WrongAssetType；不安全包配置→PackageUnavailable；实际kind/encrypted→WrongAssetType；length/hash/IO→SdkFailure，阶段ValidateResult。raw校验错误固定retryable=false、安全detail=`status=failed`；SDK初始化/manifest/acquire失败沿B原stage/retry规则。保留WrongReleaseSet/StaleEpoch及重复释放诊断；不得回显exception/path/location/URL/hash正文，不自动重试坏hash。

## 5. 精确源码与回归验收

前缀A=`Assets/Scripts/FightMatch/AssetAccess/`，Y=`Assets/Scripts/FightMatch/YooAssetAdapter/`，T=`Assets/Tests/EditMode/FightMatchAsset/`；逐项展开而非glob。仅下列四个逻辑路径，产品源码先在新S中候选封存，不写共享Assets：

| 动作／文件 | 固定before：行/B/SHA256 | 候选完整文件上限 |
| --- | --- | --- |
| 改 A`FightMatchResourceReleaseSet.cs` | 682/31863/`1dcd1288125b2c1f2ed26d6be9a4cfdd28ae59ae024e584b1b7a42137cc6061a` | 1000行/80KiB |
| 改 Y`YooAssetAssetProvider.cs` | 327/16395/`d265d9e266e79d915bd74aaa84f483210c5554d5a4f640c497f657580681a2be` | 600行/48KiB |
| 改 Y`YooAssetPackageLifecycle.cs` | 389/18380/`5285775cc312958d20c75a32f60088bcd21bc5ac698f9cefd551b143a74d7b80` | 700行/56KiB |
| 新 T`FightMatchResourceRuntimeTests.cs` | 激活前核absence，已存在即BLOCKED | 600行/64KiB |

新class=`FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeTests`，12个普通`[Test]`，非参数化；fixture复制D FIX02 V0字面量，不改旧helper；以下非discovered/passed：

- `RAW01_PinnedRawMappingOnly`：错误set/platform/object、任意raw标记拒绝；映射用成功codec的同一投影，改原boot不改变length/hash；合法raw Acquire返回RawBytes。
- `RAW02_LengthAndBudgetBeforeAllocation`：未知/超限/actual≠expected在allocate前拒绝；limit边界成功，跨provider共享64MiB、第二lease更小预算拒绝；失败/最后释放额度恢复。
- `RAW03_ExactBodyAndHashBeforePublication`：RawReadState分段短读可完成；提前EOF、尾字节、Length变化、错hash/IO不发布；每Pump≤64KiB，成功正文和SHA一致、仅一次payload分配。
- `RAW04_ReadOnlyBoundedStream`：读/seek/EOF/越界/写入拒绝、无array导出；第二stream拒绝、关闭重开、独立cursor。
- `RAW05_TwoLeasesShareOneVerifiedPayload`：一次SDK操作和一次buffer，两个view不同；A Dispose使缓存view/stream失效而B仍可读，B最终释放一次且旧stream不留正文。
- `RAW06_StreamAndLeaseHaveSeparateOwners`：stream关闭不释放lease；后台Dispose立即失效、主线程释放一次、Length稳定。
- `RAW07_StalePendingCannotPublish`：旧epoch成功/重复/异源回调不发布、不污染新Entry；已发布旧lease语义沿B，实际terminal才释放。
- `RAW08_CloseWaitsForEnsureAndLease`：Ensure未停时Close不完成；读到一半关闭不再读／发布；晚完成detach/release一次，存活lease释放后才Closed。
- `RAW09_CallbackBudgetAndThreadRulesRemain`：raw/Unity共用槽、合并Wake、Post失败恢复、无后台SDK释放。
- `RAW10_UnsafePackageAndBundleKindAreRejected`：借用/未知filesystem在Ensure前拒绝，encrypted/virtual/object bundle不进RawReadState；旧Unity对象借用行为保留。
- `RAW11_FailureDiagnosticsAreSafe`：精确code/stage/retryable；敏感路径/异常不泄露，坏hash不重试。
- `RAW12_RawInterfaceAndRegressionSurfaceStayClosed`：public两成员/无构造，A无Unity/SDK；codec canonical/五属性保留，旧B/C测试/asmdef逐byte保持。

## 6. 激活增量、封存与后置门

**须补签D-RAW-DELTA**：①A原文件加`[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FightMatch.YooAssetAdapter")]`，变更D-codec“无friend／其余private”，仅raw投影/构造变internal；无Host/Tests friend或asmdef变更。②按CD方向批准RawBytes精确两成员public面。③原D总8生产/1600行：三个before已1398、只余202；请求本子包3生产≤2300行（净增≤902）/184KiB，新测试≤600行/64KiB。D旧测试725＋600＝1325≤1400；未来factory/Host/build预算另签，不含在2300中。未批则SOURCE_ACTIVATION_BLOCKED，禁压行/拆helper规避。
中央批准后新S=`TestArtifacts/FightMatch/RES-D-RAW-001/S/`，仅10叶：`source/`下四个完整候选＋`before.json/after.json/source.patch/verify.py/static-checks.json/source-receipt.json`。S≤2MiB；before/after绑定本文、§1三receipt、四目标/旧meta身份与共享只读字节、B其余7源、D旧测试、C两源；不复制基线/SDK、不读冻在途D13+C10投影。所有新旧.meta、Packages、ProjectSettings、Host/UI/LOC/scene/assets、旧测试helper与原证据均禁止改。
作者apply_patch；仅未来`python3 -B TestArtifacts/FightMatch/RES-D-RAW-001/S/verify.py`≤30秒/最多3次留失败，检查hash/闭包/行数/权限面/四路径patch重放、固定SDK选用与禁用入口、既有断言/源码不减。源检查不等于编译或SDK运行。回执含owner/实际turn、精确叶SHA/bytes、各RAW准则及`SOURCE_READY / UNCOMPILED / UNTESTED / REVIEW_PENDING`，只回主程；不自行ACCEPT。
后续主程另绑一次compile＋RAW12/B38联合定向回归：共享lease链已改，B38一次必要；C10/D13断言/schema未改则复用届时已收证据，不主动重跑或降低断言。codec行为若变先重签回归。dots优先，C/环境/filter/次数/限时/日志/退出/证据根另签，本稿无运行权。
GitHub PR Code Review须绑实际head；真实包/Android解包、内存峰值/停流、Player锚、LOC/业务admission、factory/Host/scene/UI/设备门均后置NOT RUN。回滚仅四路径增量，不碰旧证据/存档/共享资源。本轮无S、测试/Unity/SDK、联网/安装/Git/派工；SYS只回主程收文补签。
