# DEMO-B11-C1-R1 · FM-DEMO-015B 复审门槛检查

检查日期：2026-09-22（Asia/Shanghai）；以下时间为 UTC。

```text
Task: FM-DEMO-015B（C1复审）
RESULT: BLOCKED
Reason: 本次原C实施turn为interrupted，未满足completed门槛；正式交回工具失败
Formal code review: NOT STARTED
Required user action: NONE（由SD00处理收尾与复审恢复）
```

## 1. 冻结授权与唯一阻塞

本轮依据r79的system-task-packets.md §§138–141，派发整稿SHA256为 `7ec729d263ba06526a3943eee7b0db409e24715253e2f3c7e78024418357d171`。§141明确要求“先核C1正式交付及本次原实施turn completed”，然后才正式复审。

- C任务：`01a0c403-bfa1-7e90-b503-c0fcd61f23c1`（FightMatch 本机保存与应用接入实现）。
- 本次原C实施turn：`01a0c4e0-0c6c-7f32-ad78-2fa6593bf819`。
- R任务：`01a0c1cd-dce1-7ac3-8780-06163cb0acfc`。
- 本次原R复审turn：`01a0c4e0-aa73-76e1-961d-13900042f5f2`。
- 带cursor的wait_threads返回revision 150：C任务idle，latestTurn.status=`interrupted`，error=null，completedAt=1790011098，即2026-09-21 17:18:18。
- 随后read_thread独立确认同一原turn仍为`interrupted`。工具的wake.reason虽写turnCompleted，但不替代latestTurn.status，不能据此认作completed。
- C最后的send_message_to_thread（`exec-908cbef6-5331-499d-a17c-2a77773d34dd`）状态为failed；此前向R／SD00的两次正式交回调用也为failed。R未收到成功交回。
- C在自己的commentary说明交回被自动审批拒绝，随后核对本地任务身份并重试；R只确认了上述实际状态，不推断中断原因，也不替审批系统解释未返回的细节。

因此未开启C1正式源码／验证复审，不给C1代码正确性结论，不把作者报告的测试通过转成独立验收。原B11结论没有被本报告覆盖。

## 2. 已完成的独立准备

- 16:52:34，在C修改源码前，R逐项核对原471项；全部匹配原scope.after，规范SHA为 `fb38a53bb72479907648a9a2a713bea025ec477972705ab80768de5ccd7e9bc4`。
- 独立保留两份目标文件的完整原字节、全部260个GUID及meta字节SHA、完整Assets路径清单和209项受保护文件SHA。
- 之后将C的originalMutableFiles逐字节与R独立副本比对，两份完全相同；RestoreChecks原56380字节／523行，BattleSaveCodecTests原24379字节／315行。
- 原生产SHA：`ea42030433b3f9eedbf7b4474d855aa800c4dd736abd42a7deef49411cd923f7`；原测试SHA：`382d34639de52804ce7092e5d53c0eab31099b6fbf16766fb4f877bf23de34c1`。
- 独立读取原B11 XML：2522项全部Passed、2517个Ordinal fullname。fullname＋TAB＋出现数＋TAB＋Passed数＋LF的UTF-8规范SHA为 `f7e92a7b754c1bac7158d8732308c1d39436e0d431f643a98883a522e8573d01`。
- 当前§§138–141相对派发冻结文本仅有三处实施／等待状态和实际派发记录变化，业务、白名单与验收未变。
- 原B11报告、scope、交付、Compile.log、Tests.log及XML的SHA均仍等于原冻结值。
- 在接触C1改动前，按原M06格式独立推导：本F1场景的两Run及根列表尾部为69字节，首Run.CurrentSnapshotRef位于正文末尾前53字节；此项只是后续检查依据，不是执行证据。

上述内容仅为门槛前准备；没有完成最终两文件diff、469项／GUID终态核对、新红绿原CommandExecution审计或原七项整包复审。

## 3. 已存在的C交付文件

17:19:18只核对文件存在性、长度及SHA；以下不是R对其内容或实施结果的验收。

| 文件 | 长度／行数 | SHA256 |
| --- | --- | --- |
| demo-015b-c1-delivery.md | 10799字节／92行 | `d7df4c79934f6700036df8cf322d8053d3b27532b1c242600885ff901c280fd2` |
| demo-015b-c1-scope.json | 573057字节 | `a09e689364639e9d69c50572092dde32bdc4857f459c8b55caeb0666d036c808` |
| 原demo-batch11-code-review.md | 17207字节／144行 | `785c1dda700594e679d80f8bd690bddf71fcff1721e2046f4c324a3123679ab8` |

## 4. 恢复条件与边界

由SD00安排C补齐正式交回与可核验的完成回合，并明确恢复复审的门槛。R不会自行将interrupted当作completed或更换原回合绑定；若需新的实施收尾回合，应由SD00明确绑定后重新派复审。

恢复后仍须独立核对F1两负例的真实红阶段、生产修正时序、独立M06字节负例、最终同版编译／全量测试、原2522逐名重数和Passed、其余469项及260 meta／GUID，最终针对FM-DEMO-015B整包七项给出唯一代码审查结论。当前不能关闭B11、增加接收数或据此放行016。

本轮唯一写入为本报告；没有启动Unity或项目测试，没有改实现、测试、meta、原报告／scope／产物、三份协调稿、配置或Git，没有创建任务或子代理。本报告回传后，本原R回合结束并停改。

