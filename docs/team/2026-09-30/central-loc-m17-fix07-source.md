# LOC FIX07 SOURCE：使用本机Seatbelt实证通过的等价GUID表达

中央actual `01a12e46-6af4-7b82-958d-d18c1146d85d` / 人类item `01a12e46-6b4a-7fb2-aea1-235e64f1268f`；用户要求继续，M08已经真实失败。普通源码修正和必要短回归继续，不授权新的完整restore。LOC唯一作者Astra/xhigh，中央唯一收件。

执行主程 `engineering-loc-after-m08-failure.md`（7969B/SHA04ba6aa42fb6da7414dee1c864f573bed0ddb22360e17f52754e245c75225d9c）的最小方案：生产行为仅把 FIX06_GUID 的计数重复构造成等价逐字符类，保持普通SBPL字符串、路径转义/锚点、49目录/准确writer、1文件16MiB、目录/链接和现有预算。原消费者控制逻辑不改；若必须改其他行为先报告。F6基线driver236347B/SHA715e54c8657265c2311b203e07719b0cb177f88d3140c52fbcc43a6e67e81fb1，head454eb4da250262a396b17da97d83c71de408bd48。原profile已重建匹配M08 SHA5f769139403d229b1ba22378f979019f9a7ac9425c305c73b5e5b7bca3abb916；主程6个真实OS探针约0.157秒，P2计数拒绝/P3展开成功，证据固定在 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m08-sandbox-diagnosis-01` 七叶，不重跑这6项。

唯一新SOURCE根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-fix07` 原八叶：source/m17_driver.py、source/m17-inputs.tsv、source/m17-write-set.tsv、source/source.patch、authority/prepare.json、authority/offline-checks.json、authority/source-freeze.json、authority/source-receipt.json。另后置 `docs/team/2026-09-30/loc-m17-fix07-command-receipt.json` ≤64KiB。八叶≤16MiB、driver≤256KiB。只额外适配新根、作者/输入/保护/封签及最小差量fixture；生产pin/新鲜侧录强制门保留。当前实际作者turn/item须在写源及检查前固定。

未来M7/W7分别 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17-fix07` 与 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17-fix07`，本包不创建。31包+tar沿当前固定原路径只读复用；0GET、三restore、180秒各条、870+30秒整体不变，B/L不改。

M6/W6已由中央一次只读捕获，无需重新枚举：`docs/team/2026-09-30/central-loc-m08-partial-capture.json`1332764B/SHAd6ab4afd8891eba5625852407e2ef5cd7431048d7907feae5e6618d7ddfd5777；3695文件、1328目录、129466323文件B；canonicalInventorySha256 8e6326d46c98d9b76abf08129cc588910d3b25390e944f11a7121c975ac46820。使用固定清单引用并保留原捕获时间/局限，鲜核已列文件/目录，不省略保护，不扫描SDK或重新发现partial。所有更早保护保持。固定本文、主程诊断七叶/报告、M08 E08四叶/A/H/授权和执行包、QA5480B/SHAb8e5d0f138a787342a9800c9f21638a11651bb76b6a0ec1a8235ddb66bb30acc及中央verdict2803B/SHA92f218134c75980c70cd100b1b14ebb41a169043bebf4943c11f1107bc25aca7的身份。

必要验证为一次新差量调用，总≤15秒：离线核对新生产表达与旧表达允许集合/长度/大小写/后缀/路径范围相同，49规则数量/writer/额度及其他生产行为不变，侧录/activation/保护门保持。不要重跑旧55/109/81/66/15等套件。

此调用内允许至多3个隔离Python OS回归子进程，每个≤2秒，使用新候选真实生产生成器生成的表达/序列化（不可手抄新正则后声称测了产品）。用单叶literal交集收窄：合法GUID创建/写1B/删除成功；非法长度和额外后缀必须拒绝。fixture保持.work、Luban.DataLoader.Builtin等标点，记录生产输入到fixture父目录的映射、真实profile、errno/退出/时钟/原工具；不把局部绿色当完整NuGet通过。正常工具审批若拒绝报中央，不绕过。

唯一新OS fixture根 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m17-fix07-os-checks-01`，永久仅 `probe-plan.json` 和 `probe-result.json`（合计≤64KiB）；允许暂时创建目录 `.work/phase/source/src/Luban.DataLoader.Builtin/obj` 和三条指定测试叶：`371f9ba5-1be1-4a9f-996e-d51404ed0ba5.tmp`、`371f9ba-1be1-4a9f-996e-d51404ed0ba5.tmp`、`371f9ba5-1be1-4a9f-996e-d51404ed0ba5.tmp.extra`，每叶≤16B（预期只第一叶成功且自行删除）。不新增其他helper文件，probe可内嵌driver的差量测试入口。没有网络、SDK、dotnet、restore、Unity、ps或其他材料写入；不改系统权限/ACL/挂载。

验证一次后，SOURCE机械生成/封签一次≤30秒；复用固定捕获与已完成新回归，失败即留证报中央不自动第二轮。将OS计划/结果固定身份纳入SOURCE，注意生成顺序避免自身循环。真实工具返回后补完整后置侧录，原输出应简洁且未截断，准确绑定最终freeze/八叶、directed+OS+seal真实工具、时钟和当前作者，不只填作者声明。中央负责原工具核对、既有PR分支发布/GitHub审查/QA接收。作者不自签ACCEPT。不得创建A/H，不跑完整材料/GET/restore、旧套件、Unity或Git；完成留本线程由中央wait接回。
