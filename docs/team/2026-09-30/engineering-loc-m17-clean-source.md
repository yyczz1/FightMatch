# LOC M17 — 真实输入上的干净材料后继

2026-10-04 · SOURCE_ONLY_ACTIVATED。中央已明确批准同版本/许可干净后继，依据 `central-resume-scene-loc-2026-10-04.json`；不得为旧tmp逐根复活，不伪造旧日志/hash，不删真正构建输入/验证。原M16/FIX01/FIX02原件与闭门保留。issuer/唯一收件主程 `01a0f2e3-1a80-7671-a459-38d5c8de0e6b/local/01a10641-a816-71d1-919b-97413f71ea56`；唯一作者既有LOC `01a0f40d-b0c5-7bc0-a2b2-be9d1213648a/local`，Astra/xhigh，记录新actual turn。

## 固定决定和边界

- 固定Luban5.1.0官方931源码、两已批准csproj替换、YamlDotNet16.3.0/commit `ae480660f4fb26f3eb0b41c1d1fcf21c0e9d9e73`、既有31依赖及许可证关系、现存外置dotnet-x64/sdk8.0.425；不升级工具/包/许可，不改Unity/Host/运行时模块。
- 当前真实官方源码 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0/source`，作者上一只读actual `01a10669-8d3a-7030-affe-e4c1fbe59abb` 已逐hash核931叶5675900B匹配。已批两目标csproj分别1804B/dcb87ce47a0d9789703c2e578be1aed63bdc63bbac2ac56aa4a1134bd0651911、4194B/77cf0ebbcd5a8a9e967924db71e6079c034d0a8750bda7f01c75557ebd0eddf6。
- 主程补获持久 `loc-lic-alt-yamldotnet-16.3.0-m01/acquisition/patch.diff`，1847B / `c89deeb856e1dabbde055a3fa1ec6d901443321fb883ea1ee9ba252c90545c3c`，与旧T01/identity/patch.diff完全匹配；不要再列此项无来源。Yaml源码tar固定397462B / `4f73f08a0584af4e1cac36ccb0e3cb9d9cd982209b06f7eb10dd5e31badb1bce`，旧/tmp缺席；精确原官方URL须从既有关系记录引用，后续有界获取再验hash，不猜URL/接受新hash。
- F02 driver204239B / `5df4a9acd58b3242c18945c66ab9e7dd0fb9f21b295801be78b18fa1b71d377f`、freeze6033B / `db19910e7cbfe207a604c394304c539f73f50c222a3f2edc33d881ce64921880`；源码限定ACCEPT只对应PR2 head9b8423bda3baf9545ca98e15b6ee4c0149fdf89e，不代表新M17已审/可执行。

## 唯一新写域

E17=`TestArtifacts/FightMatch/LOC-LIC-ALT-M/loc-lic-alt-yamldotnet-16.3.0-m17`，首次写前必须ABSENT。仅八叶：`source/m17_driver.py`、`source/m17-inputs.tsv`、`source/m17-write-set.tsv`、`source/source.patch`、`authority/prepare.json`、`authority/offline-checks.json`、`authority/source-freeze.json`、`authority/source-receipt.json`；总≤16MiB，driver≤256KiB。实际源码差量apply_patch，可机械复制已封底稿；无需把原回合命令正文再次嵌入driver。所有旧22叶（E16六/F01八/F02八）、其它历史证据、共享代码/索引/工具材料根全部零写。

未来仅声明、不创建：材料根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/5.1.0-fm-yamldotnet-16.3.0-m17`；构建/缓存根 `/Volumes/WD_BLACK_SN7100_2TB_Media/ApplicationData/Tools/Luban/.work/m17`。用实际精确子路径表/必要有限模式说明每个writer、阶段、叶数/字节上限、命令和用途，禁止通配整个Tools或HOME；不得复用原M16激活。源阶段本身不下载/解压/制作材料/dotnet/restore/测试/子进程/Unity/网络/Git。

## 实现计划与明确验收

1. 从真实、可验证的官方源码、精确nupkg/许可/锁依据及工具建立新确定性输入模型。旧11临时整根/旧日志作为缺失历史证据如实记录，不进入新物料输入或假作已恢复。真正必需的缺失源码/锁/资产不能直接删：能用既定固定实物重建的列明确新生成阶段与同版本独立核验；缺不可重建依据则精确报槽，不猜依赖/输出。限既定范围定点读取，关闭无效全盘搜索。
2. 自包含最小新入口，或显式固定当前存在的只读helper；不得再动态依赖已消失的T09 support.py/graphs/旧整根。不另建通用框架。复用F02已修的授权、实际输出字节/语义、完整部署闭包和许可证检查意图；不能以进程exit0/receipt声明或缩减后的三文件集合替代实际验证。31包完整closure、两源码替换、M（材料）→B（工具双构建/官方测试/oracle对照）→L（本地化编译接入）的阶段角色、独立oracle与candidate双生成、正常与负例比较及不同阶段归属沿F02已批语义，不重做选型；这不是产品LOC-IMPL-A/B模块。开工勘误：初版“M/A/B”是主程笔误，仅更正名称，不改变阶段职责/执行权限。
3. 精确列出现存输入身份、缺失但已有官方URL/固定hash的归档、需要重新生成且可独立核验的locks/assets三类，不伪称全部ready。未来每个外部动作须有具体argv/cwd/env、预算、writer和输出验证；restore预算清楚分runner生成/测试生成/测试验证。若干净锁生成必需runner restore，与M16旧runner0作显式差量；这里只提出有限清单，真实中央pin另签才授权执行。沿现存官方SDK/本地完整feed，工具和cache继续外置盘，拒绝意外网络或未列依赖。
4. `prepare`实际生成seal/input/write集合，`execute`通过同一个生产消费入口逐byte复核；分层hash无自指，实际落盘封签最终再次读取验证。独立中央可信pin保持null，生产execute在stdin/argv自授权、网络或子执行前拒绝；不得用测试pin修改生产常量。源码就绪、下载/材料就绪、独立review、实际execute四状态明确分离。
5. 针对新生成/消费链做纯进程内正反例：真实新模型重复输出一致；源/归档/包closure/锁/表篡改、缺依赖、旧tmp意外引用、写域越界、完整部署缺叶、输出缺失/不一致、缺/伪造pin均失败。可用明确标记内存适配，不声称真实dotnet或材料生成通过；已通过历史23/46/25不机械全部重跑。最多3次累计≤60秒，窗口内只修实际技术错误、逐次保留raw结果与实测计时，不吞异常或默认true。
6. 返回一份实际可审source及确定输入/执行清单：旧22叶保持、新八叶身份/精确diff、现存/待获取/待生成清单、依赖重建步骤/具体有界预算、生成→消费证据、全部正反例和actual owner/issuer。SOURCE_READY只指源码；缺口明确阻止真实执行。原作者不签ACCEPT，主程交中央同PR2新head独立审查；必要归档获取与真实执行将在源码封签后，以准确新实物和当前运行范围交中央一次可信activation绑定，不能先执行再补签。

以上方案已获工程实施授权；不为已丢失的旧执行环境做2793叶局部恢复，不重问版本/许可或用户产品决定。若实际所需超出八叶/256KiB或缺少真正构建依据，给具体最小差量/槽位再停，不进行无边界重写。
