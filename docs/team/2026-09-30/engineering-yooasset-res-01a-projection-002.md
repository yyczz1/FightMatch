# RES-01A-P02 — 临时helper名称限定纠正

2026-10-03 · 中央已基于真实CS0234批准本次有界纠正；先完成PR7固定18例与owned闭包，再由同C执行。签发主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local`／turn `01a0fde0-d533-7b00-ad78-34111f776dda`；唯一C `01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local`，Astra/xhigh，主程唯一收件。无需新设计或重复产品代码审查。本记录不立即写复用区或运行Unity。

固定失败输入：[P01 final](../../../TestArtifacts/FightMatch/RES-01A/P01/final-verdict.json)5145B／SHA `dfc823921d7a53e047f34c60d15daabe0b035edda1e50cd7b0785c0e3c5157ee`，主程04:00:26/e0cd06机械收件；32原件1984792B保持。Install1/PID42728/exit1，原日志1286/1287为helper的`Application.dataPath/unityVersion`落入`FightMatch.Application`；正式请求计数null（没有helper终端收据，不伪造实测0），fresh compile未运行，两个Packages文件未变。helper＋三个meta已归档移除，owned[]；这不是YooAsset兼容失败。

仅语义修正两处名称为`UnityEngine.Application.dataPath`与`UnityEngine.Application.unityVersion`。以前像archive `P01/transient-installer.install.cs`8368B/SHA `c35f337eaba287e69b23efbd7a22b489d9c076ea2f4c76fc316d3df5e6a20290`生成新helper；另仅允许必需的机械运行绑定：证据常量/参数/run id改P02，投影约束仍精确指向复用P01/projection，不能再把“证据根＋/projection”误当实际projectPath。保存准确diff/前后hash说明在既有run/installer身份记录中，不重构控制流、API或校验。两源码PR不变、0产品代码/断言改动。

复用区明确为`TestArtifacts/FightMatch/RES-01A/P01/projection/`及`P01/package-cache/`：保留P01已生成Library/Temp/Logs/UserSettings/obj，仅允许本次Unity必要推进；source1008/meta以P01封存后像固定，最终仅两个Packages文件可由一次UPM变更；暂态仍同一路径helper＋三自然meta，归档到P02后精确清理。无需新建/复制投影、cache或Library。P01根下32个封存证据文件（含原run/runner/log/receipt/helper归档）全字节冻结，不覆盖或追加；P01旧tmp保留。M02/M03、HSC/M01、全部源码S及共享原保护区只读。

新证据根仅`TestArtifacts/FightMatch/RES-01A/P02/`（起始ABSENT），文件名单沿[P01增补](engineering-yooasset-res-01a-projection-001.md)/旧A§6的非rollback闭集。新run绑定P01原32清单hash、复用区当次源/cache/generated身份、HSC完成/owned闭包、真实C执行turn、准确projectPath/cwd/cache/日志与新helper/runner hash；旧P01 runner只读，新P02 runner仅按上述复用路径与前像字段机械适配，≤300非空行。新短tmp仅一次`/private/tmp/fm-res-p02.XXXXXXXX`，canonical≤40、UID/0700/无链接、16MiB/512叶。新证据与现有共享/历史保护按真实范围核对，不把合法并行文档变化当产品漂移。

其余固定版本、来源及上限全部继承P01：YooAsset3.0.6/fullSHA `3b4cfb36cc2e81b9558ab74d65fee2cf2eac2804` GitHub固定URL，官方Unity registry/SBP1.21.25及其正式分发地址，一次异步AddAndRemove Install≤300s（无quit）→实际source/graph/license/assembly门及精确helper清理/owned闭包→一次fresh compile≤360s；不得加Resolve/Remove/回滚/第二候选/重试/网络补研究。每阶段最大一次，失败原样封存。资源、信号、无SIGKILL、证据/单log等上限不变；总占用门计算复用活区＋新P02证据，不能因证据根变化漏算Library/cache。准确来源未证实仍停止，不把manifest条目当解析成功。

启动前主程确认HSC验证已结束；若QA正读取将变动的P01活区则先等该读取完成，避免证据时点冲突。QA可持续读P01封存32原件和独立HSC/M01，这些不变；原P01结果只代表其结束时状态，P02后续允许的Packages/generated/cache变化不得回写历史结论。准备只需一次轻量边界核对/绑定，之后同回合按门完整执行，不逐阶段复批。任何新失败回主程，不扩版本/源/权限；禁止重跑旧17例、PR7测试或设备/真实Host/Player。作者不自给ACCEPT，共享采纳/Android/资源加载仍留后续门。
