# LOC M13 材料与签名门审查

本PR只归档本地化工具链材料准备脚本、限定许可证据方案及真实离线结果；不改变游戏代码、配置、依赖版本或生产工具。原执行根为外置盘／临时目录；这里的脚本是字节相同的审查副本，不是仓库安装器，不应直接在PR环境执行。

旧NuGet包缺少现代许可字段曾导致阻断。M13复用已取得的三包，保留包内MIT/NOTICE；对固定xunit.abstractions2.0.3采用包自身URL、官方同提交版本diff和完整许可声明的限定关联。该提交只作许可快照，不证明二进制构建来源；声明中范围限定的MIT附录不改变该包的Apache-2.0分类。完整Apache标准条款仍须于后续分发告知归档。

中央已批准上述exact binary lane；原M12失败不改写。M13实际状态WAITING_SIGNATURE_REVIEW，新增网络／SDK／restore均0，旧根一致、material absent。代码审查未完成，三包真实Linux验签由既有SDK另执行；不得把这些静态材料或signature entry哈希当作密码学验签通过。

审查重点：m13/preflight/m13_driver.py 的身份与关系核对、fixture反例、sentinel隔离、签名等待门、旧根与写域保护；m12/m12_driver.py仅为派生前像。身份／许可清单与evidence保留原字节，实际路径仍指向原材料根，不能用PR镜像路径冒充原执行位置。研究responses是连接器返回的本地JSON表示，不是HTTP原字节。

后续restore必须同时有对应实际代码head审查、三包验签回执和明确续行指令；本PR不授权restore/build/test/Luban/Unity或合并发布。uGUI变更在独立PR #1，本PR不引用其审查作为LOC通过依据。
