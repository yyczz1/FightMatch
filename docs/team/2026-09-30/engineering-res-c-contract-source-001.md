# RES-C-CONTRACT-001：资源运行与下载许可值契约

2026-10-03 · `READY_AFTER_D_SOURCE_SEAL`。中央本轮明确批准两文件最小实施；遵循AGENTS §6用户站立授权。本合同不启动作者或Unity。主程签发／唯一收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff0c-f3dd-7193-9357-e443efed3808`；D源封存后原作者 `01a0fe0e-fc5e-7a53-b05c-ad014ab72f1c/local`、Astra/xhigh串行接续，激活消息绑定实际turn、D receipt／清单。无新ARCH→SYS设计轮。

## 固定输入与两文件范围

依据[CD §2](engineering-resource-runtime-bridge-cd-001.md) SHA `ea3fc6efecd6ea118675cfcf96499cb5c3192bb6af2371d7ca9637cb7bdf9378`及[RES §10/15/18](engineering-yooasset-001.md) SHA `2e12b5f24ecedef03df5795c9632318c55fe6330562731892d4c9704c54740d8`。B基线为FIX03/S receipt21906B/SHA `b422bc2f2777e103ec9015ef67ac2daace91a3f70a6b007a51d58abbbaab97f4`与其中完整9源；D合同 SHA `8b8e2c92d4b323da416507bbaf03e378a11b11a65ffe67039b2f9d20f3ed4d40`及激活时已封存D/S。B/D源、测试、程序集均只读，本包不调用D codec。
只新增 `Assets/Scripts/FightMatch/AssetAccess/FightMatchResourceRuntimeContracts.cs` 和 `Assets/Tests/EditMode/FightMatchAsset/FightMatchResourceRuntimeContractsTests.cs`；后者是中央已批CD旧表的明确测试路径增补。归既有AssetAccess／AssetAccess.Tests，无asmdef、friend、依赖、SDK、UnityEngine、Host、LOC、UI、资源或meta变化。共享两source/meta激活前应ABSENT；写入只在下述新S，不采用到共享工程。

## 精确公开面

namespace `FightMatch.AssetAccess`。仅下表interface、五个sealed不可变类（四值型＋诊断）和四enum；无public字段、setter、额外构造器／方法。下表工厂和构造均public，类内其余实现private；可复用同程序集既有internal `AssetContractValues`，不得修改它。命名参数用对应属性的camelCase。

| 类型 | 精确成员／顺序 |
| --- | --- |
| `IFightMatchResourceRuntime : IDisposable` | `Task<ResourceInspection> InspectAsync(long epoch, CancellationToken cancellationToken)`；`Task<ResourceTransportResult> PrepareAsync(ResourceDownloadPermit permit, IProgress<ResourceProgress> progress, CancellationToken cancellationToken)`；`IFightMatchAssetProvider Assets { get; }` |
| `ResourceNetworkKind` | `Unknown=0, Offline=1, Wifi=2, Mobile=3` |
| `ResourceTransportStage` | `Inspecting=0, ReadyFromCache, ConsentRequired, Queued, Downloading, Verifying, StopRequested, Stopped, FailedRetryable, FailedTerminal, TransportVerified`（后续依序递增） |
| `ResourceTransportStatus` | `TransportVerified=0, Cancelled=1, Rejected=2` |
| `ResourceRuntimeDiagnosticCode` | `Trust=0, Schema, Hash, Receipt, Content, Text, Budget, Storage, Stop, Network, Consent, Stale, Asset`（依序递增） |
| `ResourceRuntimeDiagnostic` | ctor `(ResourceRuntimeDiagnosticCode code, bool retryable, FightMatchAssetDiagnostic assetDiagnostic)`；get-only `Code, Retryable, AssetDiagnostic, string SafeCode`；除最后项外类型同参数 |
| `ResourceInspection` | get-only `bool IsAccepted`、string `RuntimeInstanceId/InspectionId/ReleaseSetId`、long `Epoch/RemainingBytes`、int `RemainingFiles`、bool `RequiresNetwork`、`ResourceNetworkKind NetworkKind`、`ResourceRuntimeDiagnostic Diagnostic`；static `Accepted(string runtimeInstanceId,string inspectionId,string releaseSetId,long epoch,long remainingBytes,int remainingFiles,bool requiresNetwork,ResourceNetworkKind networkKind)`；static `Rejected(string runtimeInstanceId,string inspectionId,long epoch,ResourceRuntimeDiagnostic diagnostic)`，两者返回本类型，无public ctor |
| `ResourceDownloadPermit` | get-only `ResourceInspection Inspection`、`bool MobileConfirmed`；static `bool TryCreate(ResourceInspection inspection,bool mobileConfirmed,out ResourceDownloadPermit permit,out ResourceRuntimeDiagnostic diagnostic)`；`bool Matches(ResourceInspection current)`，无public ctor |
| `ResourceProgress` | ctor `(ResourceInspection inspection,string operationId,long completedBytes,int completedFiles,ResourceNetworkKind networkKind,ResourceTransportStage stage,ResourceRuntimeDiagnostic diagnostic)`；get-only `Inspection, OperationId, CompletedBytes, CompletedFiles, NetworkKind, Stage, Diagnostic`类型同参数；另get-only long `TotalBytes/RemainingBytes`、int `TotalFiles/RemainingFiles`、bool `ConsentRequired/Retryable`、string `SafeCode` |
| `ResourceTransportResult` | get-only `ResourceTransportStatus Status`、string `RuntimeInstanceId/OperationId`、long `RequestEpoch`、`ResourceDownloadPermit Permit`、`ResourceRuntimeDiagnostic Diagnostic`；static `Verified(ResourceDownloadPermit permit,string operationId)`；static `Cancelled(ResourceDownloadPermit permit,string operationId)`；static `Rejected(string runtimeInstanceId,string operationId,long requestEpoch,ResourceRuntimeDiagnostic diagnostic)`，均返回本类型，无public ctor |

## 值规则与不可越过的运行边界

- runtime/inspection/operation ID恰32位小写hex；set沿B `IsReleaseSet`，ordinal比较。Accepted epoch>0。Inspection的remaining指本次尚需网络传输的计划量，bytes为0..1073741824、files为0..512；二者同为零或同为正、files≤bytes；RequiresNetwork精确等于remaining>0。正量可inspect于任一合法network，以便展示禁止原因；BuiltIn／全cache必须零量，不要求联网。所有enum拒未定义值。
- Accepted无Diagnostic；Rejected要求诊断，保留有效runtime/inspection ID及原请求epoch（可≤0，以报告非法请求），ReleaseSetId=null、量0、RequiresNetwork=false、NetworkKind=Unknown、IsAccepted=false。这是失败值，不可当离线accepted；不得保留非法set／原始输入详情。除TryCreate外，无效程序参数抛标准ArgumentNullException/ArgumentOutOfRangeException/ArgumentException，固定安全消息，不回显输入。
- Permit只可由accepted inspection产生；null→Schema，failed inspection→其原诊断；需要网络且Unknown/Offline→Network；Mobile未本次确认→Consent，返回false/null permit/非null诊断。Wifi无需mobile许可；本地零网络允许任意network；成功true/permit/null诊断，MobileConfirmed只在需要网络且Mobile时为true，其他成功规范为false，不保存跨attempt同意。Inspection对象不可变，可安全持引用，无可变集合。
- Matches只作纯值完整匹配：current非null且accepted，runtime/inspection/set/epoch/remaining bytes/files/requiresNetwork/network全部ordinal或值相等；mobile绑定必须已确认。新inspection ID、epoch或任一量/网络变化均false。它不是不可伪造授权、不是一次性消费账本；真实runtime必须核当前自有inspection、拒旧epoch及复用permit，同一inspection只能消费一次。不能靠可构造值证明用户真的看过对话框。
- Progress须accepted Inspection和合法OperationId；Total取inspection最初remaining。0≤Completed≤Total，completedFiles≤completedBytes，剩余files≤剩余bytes，剩余bytes/files同为0或同为正，checked相减后派生Remaining；零网络进度量全0。NetworkKind是当前观察，允许不同于inspection，**不同不等于重新获准**。ConsentRequired仅Stage=ConsentRequired且RequiresNetwork且当前Mobile，此stage不满足条件则拒绝；FailedRetryable/FailedTerminal必须有诊断且Retryable分别true/false，其余stage诊断必须null。SafeCode/Retryable派生诊断（无诊断为""/false）。TransportVerified stage要求剩余0；stage本身不证明网络已停止／业务准备完成。
- SafeCode按enum固定为`RES_TRUST/SCHEMA/HASH/RECEIPT/CONTENT/TEXT/BUDGET/STORAGE/STOP/NETWORK/CONSENT/STALE/ASSET`；不接收任意string。Asset代码恰要求非null原B诊断，其余必须null；保留B原code/stage，不伪造assetId、不改B规则。Retryable只允许Network/Storage/Stop或Asset；Asset必须等于B.Retryable；它指用户重试资格，不授权自动重试或跳过条件重检。
- Verified/Cancelled必须有合法permit，结果从其Inspection复制runtime/epoch且持该permit、Diagnostic=null；Verified只名TransportVerified，不得出现Prepared/BusinessReady成员。Cancelled仅供真实实现观察请求已终止后调用，构造值不能证明abort。Rejected只持有效runtime/operation、原requestEpoch及必需诊断，Permit=null，不能含可用资产／半成功。失败没有可信目标set时不伪造它，调用方仍须以runtime/operation/epoch绑定原await上下文。
- Inspect实际实现只能读本地受信计划/cache，不联网；Prepare必须重新验证permit、以受控结果报告失败／取消、等待实际停止后才Cancelled；Assets在TransportVerified前拒绝acquire。此包仅声明这些语义，不实现runtime、不创建factory/下载器、不订阅网络或触碰存档。Dispose幂等、回调身份和主线程消费由后续真实实现验证；不得通过测试fake宣称已做到。取消／重试先停旧请求、Inspect新epoch，本次确认不持久化。

## 固定验收、封存与验证

测试类 `FightMatch.AssetAccess.Tests.FightMatchResourceRuntimeContractsTests`；恰10个普通非参数化[Test]：`C01_PublicSurfaceIsPureAndFixed`、`C02_InspectionIdentitiesAndEpochsAreValidated`、`C03_InspectionCountsAndNetworksAreConsistent`、`C04_RejectedInspectionCannotBecomePermit`、`C05_MobileConsentIsBoundToEachInspection`、`C06_UnknownOfflineAndLocalZeroNetworkPermits`、`C07_PermitMatchesEveryBoundField`、`C08_ProgressCountsStagesAndDiagnosticsAreConsistent`、`C09_DiagnosticsAreClosedAndPreserveAssetCause`、`C10_TransportResultsRemainImmutableAndTransportOnly`。涵盖每个非法身份/枚举/相邻数值边界和溢出、每个身份槽单独变化、同值异对象匹配、每次mobile重新许可、失败无半成功、无setter/可变public集合；闭集和public反射精确核，不弱化或重跑原B/D测试。Task/interface形状为契约检查，不造会“证实”网络/停止的fake runtime。
新根仅 `TestArtifacts/FightMatch/RES-C-CONTRACT-001/S/` 八叶：`source/`下两源码，以及 `before.json,after.json,source.patch,verify.py,static-checks.json,source-receipt.json`。before绑定实际激活thread/host/turn、本文、CD/RES、FIX03完整封存清单、D完整封存清单、两目标/meta缺席及共享只读源/meta；after核同组。**不读取/冻结活投影或进行中的M04/D原生证据**。无需复制B/D基线、SDK或工程。
生产≤450物理行/64KiB，测试≤500行/64KiB，S≤2MiB；计入原C总预算700/900，后续bootstrap尚余250生产/400测试行，不能借此自动扩大总预算。若不足先报实际delta，不压行/藏生成代码。apply_patch编辑；机械生成证据只写八叶；最多3次 `python3 -B TestArtifacts/FightMatch/RES-C-CONTRACT-001/S/verify.py`，各≤30s，留全部失败记录。仅标准库检查范围/哈希/预算/精确public面/Test名称/两新文件patch内存重放，不运行C#/Unity/测试/discovery/Git/网络。
源码状态只可SOURCE_READY/UNCOMPILED/UNTESTED/REVIEW_PENDING或BLOCKED；主程接真实final身份/hash后交中央另PR。实际原生待M04及D之后由唯一C另签一次compile＋本类filter（10为计划，非通过），不重跑B38/D12/全量。独立GitHub审查和定向验证各自接受；真实runtime/LOC/bootstrap资源/Host/停止/网络/设备NOT RUN。FIX04/M16旧拒绝不变，无共享采纳权限。
