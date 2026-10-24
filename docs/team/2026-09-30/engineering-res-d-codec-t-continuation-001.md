# RES-D-CODEC-T-CONT-001 — 复用M02编译，仅首次12T

2026-10-03。中央已明确批准CLOSE02后仅补D首次精确12T、复用成功编译，不重复I。唯一C 01a0fdbc-bf1e-7780-8f7f-dec13d6d590c/local，Astra/xhigh；issuer／唯一收件主程01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a0ff7d-36ca-7010-b402-b018c2bd43d8，用户授权AGENTS §6。继承[MAC-D002](engineering-res-d-codec-mac-002.md) SHA 518b379d610f4046b17d55278d1fef61beeaaf93c4cc154b717499953551a0b1的Editor/53包/资源/源与meta保护，仅以下明确更正。激活须绑定实际CLOSE02单回执及ownedClosureComplete=true；本文件准备不启动Unity。

原M02 turn01a0ff72-fa0d-76b3-8c65-02494d7e91bf已completed：receipt4673B/SHA72b3bc89497facbd48ebdd587016ae41fc24fefe038ef6cab6faeb736996fffb、run97051B/SHA537eadc4f808f0dd767def85d1f5f0ef4480348e2b4cae5b0711d0a538f7039c、runner29335B/SHAe6c6bec5b1502271108ea10d97134faf85126b0bdd187cf24d2bca141bc2ee61（266非空行）。I/result.json1088B/SHA8c56e1c5145671219a778c49f463ac1116e1e73ee453f8afab5ce4c05ac165a1实录I1/PID65952/exit0/79.649070s，阶段仍FAILED_OR_BLOCKED；I/editor.log67536B/SHA8cf3d4c340691b728e6c9b77b1340a6bf5d5be2666ca50793e4d0d87d6786fd5的239/240真实Csc/CopyFiles测试DLL。原T0/无XML/guard失败/remainingOwned=[66013]/0信号永不回写；不伪造原assemblies.json。主程3651d1/02:01:13.076413Z核原11叶245003B及下列DLL/meta。

编译复用门：原FIX01/S receipt10495B/SHA3aa9bc0e28746c8ef21a496b075381fa5f7fdd5930a8bb762741b184803c36e6与8叶、生产31716B/SHA2fff363f68195ba7e948e6b998b1ebfe140b12adbc7ab02ba2bebe6d3f69dc46、测试53927B/SHAfb5c5d866478d28a203e84908a5a7b8a66a7e478f0976e877dad1c5e979ef47d完全不变。M02七asmdef仍原hash；七DLL只有FightMatch.AssetAccess.Tests区别原前像，现154624B/SHA593f014c2c9031669e2b13ec25c8751db975ecb722a4813819f692c9a2ff9ab9，其余六逐项等于M02.run对应beforeDll。新assemblies.json保存七个path/bytes/SHA/原编译来源和前后验证，明确复用I而非本次新编译。任一必要输入/DLL漂移即停止，不自行启动I。

meta复用门：M02/after.json2937B/SHA6d5e32d7e5e2fa3b9329657d3f7ccc129651bd083e275da5ed253c27f4084032的checks.projection.naturalMeta是精确原字节。生产ReleaseSet.cs.meta243B/GUID90dec5bc95b7749d7b91b1e8105ab079/SHA900e5f8ccba2464369b17e4e73258e5431ef329a24e31658a597c61d0fdbb62c；测试ReleaseSetTests.cs.meta243B/GUID6a34a07056c2d4b349a533751994da8c/SHA7cc4ffb364174abe938cc709959afed33daddd259554f5d8614b29f8d16dde85。两者均自然MonoImporter，同原M01 GUID；全1023源/meta/529唯一GUID、旧12meta及两新meta从准备至最终严格不变，不再允许stub或任何meta演进。after保存对应原身份；不写产品、断言、源/meta、依赖或共享工程。

消费者与finally更正（同样适用于后续已批准C-CONTRACT原生）：先获得成功且完整的OS进程快照。原共享adb63318缺席是允许状态，记录originalAdb=ABSENT，不猜退出原因；仍在才核MAC-D002既定UID501/PPID1/start/exe/cwd/argv及P01投影FD0。PID复用、身份不符、FD命中、查询失败均阻断。任何新adb仅只读采集其PID/start/UID/exe/cwd/argv和投影FD命中，保留必要信息且上报，未经确认不得自动归属/放行；不读文件内容/设备/监听输出，不发ADB命令或信号，不把任何adb纳入可发信号owned集合。其他Unity/工程消费者门保持。启动前及最终核消费者；外来异常禁止新Unity及本次成功结论，但不阻断已明确归属、重核一致的自有Unity/编译子进程finally。

自有finally独立执行：原实际启动记录＋PID/start/exe＋完整argv/cwd＋捕获parent chain全相符，才适用自然60s→各自最多一次TERM→≤30s确认；不得调用可能抛出的外来consumer门作为TERM前提。所有外来/ADB查询异常单独保存，finally仍只处理精确自有非ADB集合，不发外来信号，无SIGKILL/按名kill。身份变更或无法确认不得发信号；闭合未达仍封存失败，不启动后继。门实现仅限这两项经批准修正，不扩大归属启发式。

唯一新根 TestArtifacts/FightMatch/RES-D-CODEC-001/M02/T-continuation 起始ABSENT，精确13叶：run.json、runner.py、preflight.json、T/editor.log、T/stdout.log、T/stderr.log、T/results.xml、T/result.json、process-events.jsonl、process-after.json、after.json、assemblies.json、receipt.json；失败只保留实际子集。原M02旧11叶用receipt.evidence＋receipt.json逐文件冻结；原根仅允许这一明确新子树，不能让递归清单把获准续测误判为旧文件漂移，也不能忽略任何其它新叶。其他所有旧保护、C/S八叶及CLOSE01/02冻结。

runner从固定M02最小机械适配，≤360非空行：深一层root定位、T-only、13叶、编译复用、两完成meta锁定、允许原ADB缺席及finally解耦。不得运行旧runner/main或补写其旧输出。准备中允许最多2次纯机械语法/门分支校验（每次≤10s、OS/Unity调用0，结果仅记新run/preflight），至少证明允许缺席、精确存在、未知/不确定拒绝且finally不依赖外来门；不运行C#、产品verifier或额外测试。apply_patch编辑新控制脚本与证据定义；完整历史/包cache只准备和最终各一次，阶段门保持小范围，不重复重算无变更的大树。

同P01/projection与package-cache，固定2022.3.18f1 Editor及x86_64 hash沿D002。新tmp一次/private/tmp/fm-res-dt.XXXXXXXX；同回合准备≤120s→T仅一次≤180s、含收尾总≤420s，预留finally时间。Unity argv取M02.run中尚未执行的T规范，仅换本根log/XML，-testFilter FightMatch.AssetAccess.Tests.FightMatchResourceReleaseSetTests，无-quit。精确FIX01与原D相同12 fullnames各一次、12Passed/0fail/skip/inconclusive、进程exit0、错误日志门及owned闭合才成功；无B38/C10/旧37/全量/额外discovery/自动重试或先单例再12。

资源沿D002：cache≤2GiB、生成≤4GiB、总≤7GiB、证据≤32MiB、单log≤8MiB、tmp≤16MiB/512叶、free≥4GiB；0安装/Client/Resolve/下载/Git/共享采纳，不复制工程/Library、不删缓存/锁。正常工具权限拒绝保存上报，不能换路径/工具绕过；所有失败保留并停，不修产品/断言。待正式PR可只关联固定head，不为发布改源或追加运行。

封存即依据AGENTS §6一次send_message仅回主程/local/Astra-xhigh，报告实际turn、复用I与本次T的独立身份/次数PID/exit/耗时、XML12、13实际叶、1023/529与七DLL/meta、owned/消费者门。中央可在固定四blob上并行单次PR/GitHub审查，T结果只补正文不改head；独立审查/QA门不由C自批。后继C须本次真实T和消费者闭合，另激活；不把值codec验证称为真实资源/admission/LOC/Host/设备/Android/共享集成，FIX04/M16旧拒绝不变。
