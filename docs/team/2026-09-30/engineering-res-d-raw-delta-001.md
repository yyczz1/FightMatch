# D-RAW-DELTA-001 — 额度归属与一次63项回归

2026-10-03 · `PREPARED_FOR_CENTRAL_APPROVAL`，不是源码或原生激活。依据用户AGENTS §6及中央本次范围核对委托；主程/唯一收件 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`，turn `01a0ffb9-2af3-7ea1-8f13-324e2997561c`。

原[SYS合同](engineering-res-d-raw-source-001.md)73行14259B/SHA `4c0f8116b5838eac70d52288dfcae3623d9f5cc974afe8d0778148b571a7f43d`保持原字节；本文仅补§4的64MiB额度owner，并替换§6的回归集合。中央仍须明确批准原§6的A→YooAssetAdapter程序集级friend、RawBytes精确两成员public面和三生产2300行/184KiB（净增≤902）及新测试600行/64KiB；四路径/S十叶/其它限制不变，不预批任何额外interface或依赖。

- **唯一账本owner**：`YooAssetAssetProvider`沿现有`globalPending/globalRecords/globalSlots`在同一Unity主线程维护一个private static raw预留总字节计数；不加全局字典、后台管理器或跨模块可变入口。每个raw `Entry`独占一份非零expectedLength预留记录，归其provider的既有entries集合；总数等于所有尚未归还Entry预留之和，范围0..64MiB，不按provider重置。
- **扣账点**：请求类型/受信映射/epoch/caller预算和B原槽位门通过后，新Entry在任何`lifecycle.Begin`/Ensure/正文分配前以checked运算原子完成本主线程上的上限检查和记账。拒绝不扣账、不起SDK；复用同Entry的第二lease只校验其自身预算，不重复预留。没有Ticket/SDK启动的早期异常也必须走该Entry清理路径，不能遗失已扣记录。
- **归还点**：只由主线程Drain清理同一Entry时执行一次；pending/Ensure必须真正terminal，所有待发结果已收束、存活lease为0、流已关闭且payload引用已清空、Ticket.Release成功（Ticket尚未创建则为空）。随后把Entry预留消费为0并从总账扣除，再移除Entry；重复回调/Close/Dispose见0即无二次扣减。SDK/分配/长度/hash/IO/回调错误、stale、关闭均沿该路径；清理失败保留Entry及额度待既有后续Drain，不能先还额度或宣称Close已完成。
- **线程与寿命**：后台lease.Dispose仍只置既有Released标志并Wake，使该view/stream即时拒绝后续读；不在后台改总账或释放SDK。一个lease释放不影响另一个，最后lease归还后才满足Entry清理。只要pending/有效payload仍存活就保留原预留，不用时间、GC、静态reset或finally无条件清零伪造恢复。包最终Close失败但Entry已实际清理时，raw额度可为0；包关闭结论仍沿原B门。
- **可观察证据**：在原RAW02/05/07/08/09十二方法内覆盖跨provider共用64MiB、第二lease预算拒绝不扰动已存Entry、Begin/allocate/hash/IO故障、stale晚到、最后lease和清理失败再进入、重复Close/Dispose/回调的恰一次归还；用后续另一provider的合法预留能否成功检验归还，记录SDK/分配次数。不得靠清空static状态令测试通过，不加第13个RAW测试或修改原B/D/C断言。

**后续唯一回归集合改为RAW12＋B38＋D13＝63**：raw改B共享租约链，且codec在已Validate的root上保留raw投影/构造链，二者均为本次受影响生产路径；旧D13断言/schema未变不能把旧输入结果当新输入证据。一次compile→同一次三类OR定向T，精确多重集和分组结果待原生合同绑定；不跑旧head红测或额外单独D轮。C两源/interface未变，C10复用已接联合证据，不在63内。本文无Unity启动/测试/发现权限。

主程已接联合原生23/23并收到D0377限定ACCEPT；C首次发布PR11/head `f99b5a60b9ecad62aa01ddb1333a7db8a05c662f`，原native head=null保留，代码门另接。未来源码激活把两份合同、B/D/C固定source receipt、实际owner/issuer/turn和中央明确批准项一起绑定；不重跑已有原生或冻结共享活树。未获该激活前作者继续闲置。
