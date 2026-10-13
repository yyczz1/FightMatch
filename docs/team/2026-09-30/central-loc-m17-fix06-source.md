# LOC FIX06 SOURCE：M restore 的 GUID 临时文件合同

2026-10-12。中央actual `01a12dc8-58f5-7150-aa6d-c763dd9d9e02` / 当前人类继续item `01a12dc8-5fb9-7e43-8659-7026352c2050`。M07一次已真实失败并经QA核收，中央NEEDS_FIX；本包继续普通技术源码修正，不是新的材料运行许可。LOC/local唯一作者，Astra/xhigh，中央唯一收件。

执行主程 `engineering-loc-after-m07-failure.md` 第3–5节设计（11368B / SHA256 `5a2943ca3037632dca72b3bb03eda3f6a49cb67daa34fd6ebdb2cab52f46fbb3`），含完整消费者、负例及证据边界。唯一行为修正为三次M restore图内49个精确obj目录的直接GUID.tmp叶：新增bounded-guid-tmp规则、每obj驻留maxFiles=1、合计maxBytes=16MiB/规则，保留原M/W总上限。GUID形式为小写8-4-4-4-12十六进制加.tmp。新类型必须统一授权、profile、输出检查和目录准备；保持旧exact/bounded语义与最终输出校验，不开整个obj/source。该额度不是实测峰值，超额报告，不自动增大。

基线REVIEW02 head `01a5ab7afa5f3d2d55df4b5d6b72c8fa6e353736`；R2 driver205640B/SHA ae1d2c3cb2a8cc1ea95d92fd5c7f3c5fce0eb90fcfa2235e9650023b9bd4ec5f，freeze2870B/SHA d32e5af44ba61473b333f919910167c6cad2b6691d19cc12e1395fe45e7147de。保留R2的独立activation及后置侧录强制门，适配新F6作者/固定路径/authority/真实成功封签记录；旧sidecar不替代新记录，不形成自身封签循环。实际当前作者turn/item在写源和检查前固定；主机/中央身份不凭历史猜测。

唯一新SOURCE根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix06` 的八叶：source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch、authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json。唯一额外后置叶 `docs/team/2026-09-30/loc-m17-fix06-command-receipt.json` ≤64KiB。八叶≤16MiB、driver≤256KiB；占用即停。除主程列明规则消费者，仅调整必要fixture、来源/保护/模型/新输出根和侧录元数据；归档/依赖图/命令/环境/网络/子进程和B/L行为不变。

未来M6/W6准确路径沿主程第4节，仅出现在SOURCE计划中，本包不创建。旧R2、E07、M5/W5、A/H、人类授权、QA与所有更早根/31包+源码tar保持只读，不清理旧失败或复用为新输出。0GET、三restore、180秒单命令及870+30秒总M保持。

中央已补完一次限定只读M5/W5清单，无需你再次枚举：`central-loc-m07-partial-capture.json` 1340246B/SHA `4af68e331d1bfb8c78b66283dd4a4703446f1c9742b7860bbf5253f8a5131430`；3713文件、1338目录、136062723文件B、链接0；canonicalInventorySha256 `54eff583f444642b1663a24bc1741374bd107982ef158a9d6b4983940562ea82`。中央actual工具 `exec-b8a512f8-9833-4a97-8127-9ef96db4bd89`，完整调用0.632150秒/exit0。清单是闭集保护输入，按准确身份与固定条目鲜核，保留原捕获时间；不再发现/枚举partial或扫描SDK。可把清单作为固定身份引用来避免在多份SOURCE文档重复展开；不得因此省略真实M启动前的保护验证。若现有表示无法在额度内安全接通，报具体缺口，不扩大根/文件/上限。

另须固定本文及主程设计、QA `testing-loc-m17-m07-native-receipt.json` 5385B/SHA f1a7d7bc2cdb73ff843d59b13815b8b99af354c94ab4b1651cec56c507c6e23c、中央 `loc-m17-m07-native-verdict.json` 3479B/SHA ef1152b0aae3004031c72a1b48cf9a72b8678b97a63a9baf2bfdbaa24a4c5104，以及E07准确原始记录和只读A/H所需身份，不引用未产出的临时记录。M07原工具exec-4def70d8-d499-409c-aefa-385c90732e0f；实际runner收据/profileHash及失败stdout由设计列明。

验证合为一次≤10秒新定向调用：在同一调用里旧规则对本次路径RED、新规则GREEN；用真实旧生产profile生成函数重建字节并先匹配真实profile SHA25af8…279f5，不匹配即停。覆盖49目录/三phase/准确writer、GUID合法与各越界/链接负例、真实消费者目录准备无占位符目录、0/1/2叶及16MiB边界、未变exact/bounded规则、零GET/三restore/预算/包/图/patch/旧根保护/关闭pin和侧录门。用内存fixture/哨兵，无真实材料写入或native。旧109/81/66/15不重跑；本次profile模拟不等于OS已接受。

然后一次≤30秒SOURCE机械生成/封签，复用清单，真实工具返回后捕获完整侧录（tool/turn/command/未截断原输出/退出/完整时钟/最终freeze/八叶/检查身份）。失败或超时保留并报中央，不自动第二轮；小技术修正由中央收窄派发，不找用户重复确认源码范围。中央负责后续既有PR分支推送/GitHub审查/QA接收。作者不自签独立ACCEPT。

本包不执行GET、restore、材料、SDK扫描、Seatbelt加载、ps、Unity、B/L或Git，不创建新A/H。完成返同一会话，由中央wait接回。若必须改上述范围，先报告具体变更和原因。

