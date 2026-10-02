# LOC三包Linux签名验证005

2026-10-01 · 中央授权；owner为既有durable `01a0f13c-3d87-753a-ad11-16302add9084`，Luna/low，仅机械执行；唯一收件者中央 `01a0e401-511d-79f2-b47f-3ab0ade1681b/local`。

复用004已经安装的 `/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-VERIFY-004/sdk/dotnet`（SDK8.0.425/linux-x64），核原004已封存SDK文件manifest及实际launcher身份。旧004、002、003只读；不重下载SDK，不重做metadata探测，不继承旧包身份。旧本地R路由由GitHub代码审查取代；实际签名证据另接收。

新建一次 `/workspace/TestArtifacts/FightMatch/QA-CLOUD-NUGET-SIG-VERIFY-005`；仅允许authority、network、packages、verify、process、before、after、result.json、evidence-files.tsv。所需支持脚本只放process。旧根、仓库、Git、Unity、用户工具/配置/profile、系统trust只读。free-space起止≥2GiB，证据≤16MiB，每日志≤2MiB。

本机M13已机械核三包bytes/SHA256/catalogSHA512/signature entry身份。无直接跨主机二进制传输工具，授权各fixed URL一次GET（不先HEAD，不retry，0redirect，network并发1），总获包≤3请求/5MiB；每GET≤60s，保存URL/status/header/body身份/时序，预算与M12旧chain9请求1764095B分账。含本次获包不超过12次，原cap14次/10MiB保持；独立research12次另记。

| package | bytes | SHA256 | signature bytes / SHA256 |
| --- | ---: | --- | --- |
| System.Reflection.Metadata 1.6.0 | 852113 | `2497e068f6afed47c4878c9101074684b645d5baebd2d5163e5eaa99f356abf1` | 22354 / `48b089f802897bf823e41916e8d44b8bfbfabc89b2f3ce7c8775ff2c27667435` |
| System.Collections.Immutable 1.5.0 | 804405 | `0658aa6252fd9ed6cc5e8e5decd52b297c1837e3ea772a83212e5ba78a95afb4` | 22354 / `310496ac19ab76b103bc5756648df226406b988c49d103ca4d7ccc706c2bd188` |
| xunit.abstractions 2.0.3 | 75155 | `d03d72fc2df8880448f7a81bddb00e1bcb5c18f323c7e7cc69b4cfa727469403` | 18486 / `6690990d24e2b4e1edad729d1779d37d22f148add95b90858fa8b71263a056a5` |

固定URL与catalog SHA512完整值见随激活消息传入的JSON；执行端保存为authority/packages.json，不手抄重造。先核全部已获取包身份，按每包顺序验签；首败封存停止。

隔离及副作用控制原样继承 [004 §4步骤5/8/9](testing-loc-license-alt-signature-linux-004.md) 和 [A01](testing-loc-license-alt-signature-linux-004-addendum-01.md)：process下独立DOTNET_CLI_HOME、NUGET_PACKAGES/HTTP_CACHE_PATH/PLUGINS_CACHE_PATH/SCRATCH、TMPDIR；TELEMETRY_OPTOUT=1、MULTILEVEL_LOOKUP=0、NOLOGO=1、SKIP_FIRST_TIME_EXPERIENCE=1、GENERATE_ASPNET_CERTIFICATE=0、ADD_GLOBAL_TOOLS_TO_PATH=0。只对子进程生效，不改HOME/home/CODEX_HOME/PATH、trust或全局设置。最小NuGet.Config仅`<configuration />`。

每包且仅一次：`<fixed-sdk>/dotnet nuget verify <root>/packages/<id.version>.nupkg --all --verbosity detailed --configfile <root>/process/NuGet.Config`。每包≤180s，单次等待≤60s；保存exact argv/env、raw stdout/stderr/exit/开始结束/owned进程。SDK文件一致可直接复用004能力证据，不重复--info/help。包HTTP账与验签工具内部证书链尝试分开，不伪称隐藏请求为0。

超时停止后只对本次创建且即时身份匹配的owned进程单次SIGTERM、最多30s等待，仍在即返回清理blocker；不SIGKILL、不重跑。成功须真实exit0、实际签名成功，SHA/entry相同本身不等于验签成功。NU3018/NU3028等警告逐条保留，吊销状态不确定不得写成已验证。

原真实用户工具目录/profile和SDK/旧证据前后比较；结果含实际owner/turn、每包固定身份、SDK身份、命令/退出/警告/错误、网络账、进程收尾、raw证据manifest。至多回`SIGNATURE_EVIDENCE_READY`，不自判M材料接收；restore/build/test/Luban/Unity/Git全部0。成功或首败后只回中央简短结果及云端路径，并在最终输出提供可回收的result和raw验签文本，不唤醒其它角色。
