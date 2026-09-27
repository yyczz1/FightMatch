# CONT-C 实施阻塞交接

状态：`BLOCKED`。实施未完成，不是代码接收或 `COMPLETED`。
准确任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`；准确实施 turn：`01a0e382-ea66-76f3-a0a0-7be71598b4cd`；gpt-6-astra/max。
此前 IPC 恢复答复保留。本次阻塞来自接口缺口；未启动 Unity，没有新的许可证或 IPC 结论。

## 起点与已保留成果

- 冻结 §367：11217 UTF-8/LF bytes；SHA256 `9b98b275f4066726466df278c683aebecf2cfb073420499533b96cae5533713b`。C1 三份实物及 CONT-B-CODE-C1 起点均核符，根绑定本次准确实施 turn。
- 已保存起点并新增 3 份 Application 草稿，共 466 物理行。其余 10 个 cs、13 个自然 meta、工具 CONT-C 扩展、新测试未完成。
- 当前草稿依赖尚未写入的 Recovery partial，包括 GateReason、ReturnToAnchor、OriginalToken、Receive 等，Act 也未实现；不是可编译交付，未通过运行编译制造已知的不完整失败。
- 旧 776 实现、806 Assets、425 GUID、36 DLL/PDB 身份逐项不变。旧工具 282 行、增删 0、累计仍 194；旧测试修改数 0。
- 当前实际 779 实现／809 Assets／425 GUID／36 DLL/PDB；921 导出路径中 23 个获准新路径仍不存在。目标 802／832／438 尚未达到。
- 原源码、首包、旧证据、六份 CONT-C 设计交接保全。未写 Git、未创建任务或代理。

## CONT-C-RCV-01：Observed 续办再次失败后缺原 intent 读取接口

完整重建 Application/PlayerSession 丢失内存 request 后，显式选择 observed commit 调 ResumeObserved；若续写或提交后读回再次失败，原 runtime 保留候选和 intent，但导航无法取回原 Retry／Resolve／End 所需的 immutable intent。涉及 CC17 及该分支的 CC14／CC15／CC19。

1. CandidateApplicationRecovery.cs:254–256 将解码的 last.Intent 保存到私有 pending，然后 WriteOriginal；失败返回不提供 intent。
2. 同文件 :42 在 pending 存在时 Restore 只返回 PendingResult；:217 使重复 ResumeObserved／EndObserved 返回 Busy。
3. CandidateApplicationRuntime.cs:264/:276/:303 的 Resolve／Retry／End 均要求原 PreparedCandidateApplicationIntent；:101 的 PendingResult 没有 OriginalLookup。
4. 未完成操作不在旧 PublishedSnapshot.Records 中；重建后的该视图也可能为空。操作 ID 不足以重建 canonical intent、来源及份额。
5. CandidateApplicationRecoveryTests.cs:257–280 的 SaveFailed／CommitUnknown 用例断言 resumed 结果 OriginalLookup 为 null，再用测试局部保存的重建前 intent 调 Resolve／Retry。这两个具名实例在已接收 3872 基线中为 Passed，本轮未重跑。

这不表示 M12 丢失候选或原恢复核错误；缺的是重建后连接原恢复核的读取接口。反射、在新文件暗加 runtime partial 访问私有字段、页面解析候选、重造 intent、第二份 UI 日志或反复关闭／重开对象均不是获准路径。

## 最小补签提案（未应用）

按 §367.4 保全当前成果并交 SD00 处理。§367.2 规定“原生产与原测试修改数为0”，并规定“唯一获准修改的旧文件是 Tools/Invoke-FM025P2Validation.ps1”；不能自行修改旧 runtime。
仅建议新增旧文件范围 Assets/Scripts/FightMatch/Application/CandidateApplicationRuntime.cs：新增≤24物理行、删除0行，增加下列 internal 只读方法。原导航新文件、预算、13个 cs/meta 白名单及唯一验证工具保持。以下仅为报告内草案，未写入生产代码。

```csharp
internal CandidateApplicationCallResult QueryResumedIntent(
    PlayerSessionSystem owner, string commitId, string operationId,
    out PreparedCandidateApplicationIntent intent)
{
    intent = null;
    var refusal = Guard(true);
    if (refusal != null) return refusal;
    if (owner == null || !ReferenceEquals(owner, playerSession) ||
        View.Purpose != SavePurpose.PlayerSave)
        return Reject("InconsistentBinding", "Navigation.Owner");
    if (playerSession.CreationPending)
        return Reject("CreationPending", "CreateRecord");
    if (pending?.Ticket == null || commitId == null || operationId == null ||
        pending.Ticket.Metadata.CommitId != commitId || pending.Intent.OperationId != operationId)
        return Reject("StaleContext", "Navigation.ResumedCandidate");
    intent = pending.Intent;
    return PendingResult();
}
```

导航须记录本 session 显式发起的 ResumeObserved，只在该次返回的 pending operation／commit 均匹配时取回引用。普通 Query 不得接管其他 owner 的 pending。随后将同一个原 intent 交现有 Application.QueryOperation／Retry／Resolve／End；不重新 Prepare／build，不改旧公开 DTO、序列字段、持久协议或正式信任门。
补签后须用真实 PlayerSave 重建并丢弃旧 request 引用，制造再次 SaveFailed／CommitUnknown，再沿原 intent Resolve／Retry／End，核 operation／commit／canonical bytes／来源份额不变；另核错误 owner／commit／operation、F2、WrongThread／Disposed／Busy 及其他 owner 拒绝。全部30项实施验收仍须完成。

## 验证与有限证据

- RUN：当前静态身份、有限路径、物理行数、旧 GUID、旧工具及基线 XML／具名多重集合核对；除 3 个获准新 cs 外，保护输入与起点一致。细项在 scope-audit.json。
- REUSED：CONT-B-CODE-C1 002 Compile exit 0；003 全量 EditMode 3872/3872，3867 不同 fullname，0 fail/0 skip。这是旧基线，不代表当前草稿通过。
- NOT RUN：本轮 CONT-C Compile、EditMode。源码／测试未完成且接口待补签；001–008 槽全未用，无新日志、XML 或失败槽。
- 30 项验收全部 BLOCKED_NOT_VERIFIED。CC28 只记录旧源码与已有绿测保全；CC30 仅完成当前范围／身份检查，不代表最终验证完成。
- 证据根实际22文件，manifest排除自身列21项，均在141条精确白名单内；未预造代码、meta或测试产物。
- 物理鼠标／触控／像素、交互式PlayMode、Player／Android、028／029宿主、正式配方→PlayerSave制作整链、公开AwaitLinks、未覆盖强保存／真实崩溃及§184仍NOT VERIFIED。

协调稿例外：docs/system-design/2026-09-16/session-plan.md、docs/system-design/2026-09-17/integration-review.md、docs/system-design/2026-09-17/system-task-packets.md 与起点不同；已在 inputs-before/after 和 scope-audit 单列，属于独立协调更新，不计本轮源码修改。最终静态核对按该例外排除协调稿的并发变化。

## 续接

交接文件为 demo-cont-c-delivery.md 与 demo-cont-c-code-scope.json；formal BLOCKED final 绑定外部身份，不预填原生 completed 或 R verdict。
保留起点、3个草稿及阻塞记录。补签后从当前源码与根继续，补完实现与静态核对，再固定工具 Compile→无筛选全量 EditMode；不重复旧基线。

- root-identity：`TestArtifacts/FMDemoCONT/cont-c/root-identity.json`；1525 bytes；SHA256 `4de12a9a70ddf6234564c17b0c1277943e189ae74cce8d0964bba7763db6751c`。
- read-manifest：`TestArtifacts/FMDemoCONT/cont-c/read-manifest.json`；5289 bytes；SHA256 `934d390f7e8e5997a200f306a8221963ed278f878e0ee59f06b956cc1f1de165`。
