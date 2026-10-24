# LOC M05：修正版一次材料执行

2026-10-11。用户在中央 actual `01a12926-663f-78a0-bee9-38392d8c0f5e` / item `01a12926-6966-77b3-9e90-961521142cee` 回复“继续任务”，接续上条已明确范围的一次修正版M提案。真实原话和上下文见 `central-loc-m05-approval-2026-10-11.json`，不是复用M04已消耗许可。

执行者仍为LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh；唯一接收者中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。当前fresh执行turn由中央实际读取后绑定，不复用旧source/M04 turn。无需执行者向其他会话发送消息。

固定输入/范围：`central-loc-fix03-next-material-proposal.md`、`loc-m17-fix03-source-verdict.json`及FIX03八叶封签。PR2当前head仍 `c3e017f2fe071f71aa0ed4ad73a927fab7a8e09d`，GitHub6055811693已完成；中央已核七源文件哈希一致，新M/W和M05证据根均缺席。历史源/失败/产品工程不修改。

先读包，保持本回合等中央创建且完整封签的 `central-loc-m17-m05-host-binding-receipt.json`（最多3分钟，每次等待≤30秒），不要自行生成或修补host/activation。中央按已审模板仅填字面量，将新turn、当前人类授权、FIX03 driver/model/seal及新M/W绑定到 `central-loc-m17-m05-activation.json` 和 `central-loc-m17-m05-host.py`。

绑定齐全后，沿已审外层运行方式，使用正常工具审批执行准确host argv一次。限18 GET / 25172764正文B、3次restore、900秒含870工作+30收尾；所有前提核验在该材料timer内，不先重复扫描SDK或跑离线套件。仅写既定新外置M/W以及M05证据四叶 `before.json`、`stdout.log`、`stderr.log`、`receipt.json`（总≤16MiB、单日志≤4MiB）。保持HOME/CODEX_HOME，子工具隔离路径沿模型。

禁止额外重试、B/L、build、Unity、清理旧失败或Git写入。失败保留实际原因、输出、partial roots并停止；正常工具审批拒绝也如实报告，不换通道规避。完成要给原工具ID/外层时钟/实际退出码/GET字节与还原数量/产物身份和保护结果；中央会直接读取该会话及文件。M候选不自称独立验收通过。

## 同次未启动执行恢复（resume1）

首次 owner 回合在仅生成 before.json 后结束，没有 host 执行记录，M/W 仍缺席；不视为失败重试或额外执行额度。旧 A/H 和 before 保留。继续相同 M05 一次授权，fresh owner actual `01a12931-9e6f-79b1-ac3d-ab9ce3ab4ac0`；新 A/H/绑定凭证使用 `central-loc-m17-m05-resume1` 前缀，外层四叶证据改为 `/Volumes/WD_BLACK_SN7100_2TB_Media/UnityProj/FightMatch/TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17-m05-resume1`。仅身份及证据路径变化，其他输入、M/W、预算和禁止项不变。中央仍唯一接收者。
