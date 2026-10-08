# V03 绑定文件漏项修正

2026-10-08。交原源码会话Astra/xhigh处理明确数据漏项，中央唯一收件。当前V03未审、未激活、native0。Luna交付actual01a118eb-210a-7cb3-a203-b24e0740194b的五叶已逐字节保存在V03/draft-source；不要修改该历史、V01/V02或P/K。仅仍写V03/run同五叶。原central-resource-combined-v03-binding-refresh.md范围不变，以下具体缺口必须修齐。

中央核到三个实际问题：

1. preparation缺runner运行必需的mechanicalPreparationSeconds（仍须160），也遗漏原要求的activationContract、completionObservationContract。复制V02对应合同再只变当前V03路径/任务/TMP前缀，使用当前五封签。completion契约语义完全不变。preparation的177通过明确是V02证据复用，不新跑。
2. owner错误用了更早失败回合01a118e7，且effort仍low。当前修正应通过read_thread识别自己的实际新turn，以本次实际请求Astra/xhigh记录preparation/input owner，另列Luna历史author01a118eb…，不伪造当前身份。修改owner后重算input整体bytes/SHA并更新runner的该硬绑定。
3. 当前data.newRootPreconditions.activationTests.path仍指V02，潜在其它AS/TMP字段须机械穷尽核对。仅在当前顶层paths、commands、newRootPreconditions、evidenceSlots、transferPlan里递归把字符串路径段 /RES-COMBINED-V02/→/RES-COMBINED-V03/，以及当前TMP前缀fm-rcv2→fm-rcv3。task直接V03。固定历史references和source信息完全不替换。commands.environmentTOnly的实际键是FIGHTMATCH_ACTIVATION_TEST_ROOT，不能写虚构ASSET_CACHE_DIRECTORY字段名。compilePlan只变那一个Tundra SHA；其余完整保持V02。

runner相对V02依旧只允许任务/当前路径标签、TMP前缀和新input整体bytes/SHA常量，归一后全文相等，不改逻辑。checker/results继续原字节复用，不执行。按真实递归diff产生准确data差异，不手写模糊/不存在字段列表；记录完整差异位置与before/after或可精确重建映射。全部sourceIdentities和对应基线身份需齐全；不必复制V02的庞大案例详情，引用固定V02原文件即可。

由于以上新发现的漏项，明确只追加一次≤10秒静态核验：语法/JSON、当前五叶/基线封签、input差异重建、受保护集合不变、所有当前E/BC/AS/TMP一致、mechanicalPreparationSeconds及activation/exit合同存在且语义等价、runner归一全文等价、历史checker/results相等。不是177项回放重跑；不native/ps/信号/网络/Git/缓存写。保留Luna原0.160秒检查及其漏项事实。不要新建监护框架、工具脚本或测试；只返回纠正后的五seals与静态证据位置，输出每条≤2KB。
