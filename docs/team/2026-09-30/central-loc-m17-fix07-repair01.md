# FIX07 源码记录修正：复用已通过OS结果

中央当前turn 01a12e46-6af4-7b82-958d-d18c1146d85d / human 01a12e46-6b4a-7fb2-aea1-235e64f1268f。这是普通源码/证据格式修正的继续，不准新增restore或native。上一LOC turn 01a12e5d-f4c9-7443-87c9-3eb4d3f47ce4 的实际指令仍是FIX07，不是历史权限咨询；本轮回到FIX07交付，不扩展OpenAI文档调研。

已证实：三OS子进程全部达到预期（合法写1B成功、非法长度/后缀EPERM，均reaped），记录在原固定OS两叶，禁止重跑。aggregate检查工具exec-4edf9488-4e3e-4c64-8627-a36ae7e16de9在2.401538秒/exit20止于“M08 inventory SHA”，根因是中央清单用缩进JSON＋换行计算，消费方按紧凑JSON计算；不是材料被修改。未封存SOURCE。

中央已保留旧清单，新增规范化副本 `docs/team/2026-09-30/central-loc-m08-partial-capture-normalized.json`1333581B/SHAf84843c252c67ba0c34b6297814b29868f3dc3ef958ed841efc3afc0cefddd70，canonicalSHA44bd29bd122c4adacfaf58921f68259d4365f38f785dad960475b2903a5b7ff1；3695文件/1328目录，inventory内容/捕获时间逐项不变，新文件系统捕获0。使用该准确副本作为新的绑定，不去更改历史原件或其SHA。旧未封存driver260786B/SHAdc1c9293582e77ca6e46a34e9ad60c39afc82c16ca5a21de020a80246e00a7d4、失败checks273B/SHAce3972823d56b69ee7db3f101e05b1b8f679c5affc1fbf95a69eb45a22a84d43，以及原失败工具，已由中央完整只读备份到 `TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-m17-fix07-repair01-inputs/` 的 original-driver.py / failed-checks.json / failed-tool.json。保留这三叶及原OS两叶只读。

允许继续修改未封存的FIX07同一根八叶，及尚未创建的后置 `loc-m17-fix07-command-receipt.json`。生产行为唯一变更仍是原包展开GUID；本轮只修清单准确身份和必要作者/修正链/证据复用/封存元数据，所有生产规则/预算保持。原作者、此次修正作者、封存执行者分别记录真实turn，不能把旧OS检查说成本轮新跑。

本轮新增一次≤10秒的只读差量检查：验证规范化清单与原清单inventory同一、紧凑digest匹配、保护项不变，并完成先前因metadata异常未达的源码差量检查。保留旧失败aggregate，新aggregate按本次actual记录。复用原3OS的计划/结果/原工具，通过比较旧实际被测driver与最终driver的生产GUID及相关函数/序列化不变，证明OS证据适用；不复写旧probe-plan/result，不新增OS子进程。新运行应有启动哨兵确保native0。不要用新的driver hash冒充旧探针输入hash。

随后使用尚未用过的一次≤30秒SOURCE机械封存，输出完整后置工具回执。八叶≤16MiB，driver≤256KiB，sidecar≤64KiB保持；可精简仅本轮FIX07新引入、已经不再执行的探针运行脚手架以接入只读证据复用，但不得改其他生产消费者或重写历史测试。固定本包、规范化清单和旧失败备份，避免自身循环。若必须扩大行为/预算或无法证明旧OS证据适用，报具体差量，不自行重跑。

只读差量/封存若再失败则留证返回，普通修正不要求用户重复授权。完成返回实际结果、八叶/侧录和证据复用说明，中央唯一wait接回，作者不自签ACCEPT。不调查历史审批、不Git、不SDK扫描/partial重枚举、不创建A/H，不native/restore/Unity/B/L/下载。
