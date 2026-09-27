import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {Workbook, SpreadsheetFile, FileBlob} from '@oai/artifact-tool';

const here=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(here,'../../..');
const out=path.join(root,'outputs','01a09ee3-77ec-70a0-8c80-7ec5a8cffb79');
const d=JSON.parse(await fs.readFile(path.join(here,'results/workbook_data.json'),'utf8'));
// Preserve the delivered workbook; this branch updates only calibration notes.
if(process.argv.includes('--inspect-winding') || process.argv.includes('--sync-winding')){
 const target=path.join(out,'FightMatch-首轮配表-v0.2.1.xlsx');
 const evidence='C:/Users/YYC/.codex/visualizations/2026/09/14/01a09ee3-77ec-70a0-8c80-7ec5a8cffb79';
 const current=await SpreadsheetFile.importXlsx(await FileBlob.load(target));
 const inspectOnly=process.argv.includes('--inspect-winding');
 if(inspectOnly)await fs.copyFile(target,path.join(evidence,'n2h-workbook-before.xlsx'));
 const patches={
  '使用说明':{
   A2:'0.2.1 · 2026-09-18 上弦校准 · 首章1～16关。成长与战斗历史数值不变；火球完整回放尚缺输入。',
   B12:'421基线检查；6项上弦验证',
   C12:'54组Boss、200条广告。未验证火球全流程、全部配队及Android性能。',
   B14:'成长v4；2026-09-18上弦校准',
   C14:'普通与Boss统一精确10%恢复；首证和教学持久化仍待实现。',
   C16:'现有回放仅伤害＋嘲讽；完整评分需接入真实事件来源与去重，不把单项账本当全场验证。'
  },
  '技能状态':{
   C7:'候选：中心0.8／周边0.4；对照1.0／0.6',
   F7:'整面固定原位；中心死仍打活邻位，不跨空位。倍率未定，完整回放待材质等输入。'
  },
  '逐敌配置':Object.fromEntries(['I36','I39','I42'].map(cell=>[cell,'偶数上弦10%，保留小数、缺血封顶；2次，正恢复才耗；其余0.25E攻击']))
 };
 if(!inspectOnly){
  for(const [name,cells] of Object.entries(patches)){
   for(const [cell,value] of Object.entries(cells))current.worksheets.getItem(name).getRange(cell).values=[[value]];
  }
  current.recalculate();
  console.log((await current.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#NUM!',options:{useRegex:true,maxResults:20},summary:'Formula errors'})).ndjson);
 }
 for(const [name,range,key] of [['使用说明','A1:C16','guide'],['技能状态','A7:F7','fireball'],['逐敌配置','H35:J42','winding']]){
  console.log((await current.inspect({kind:'region',sheetId:name,range,maxChars:1000,tableMaxCellChars:80})).ndjson);
  const preview=await current.render({sheetName:name,range,scale:1,format:'png'});
  await fs.writeFile(path.join(evidence,`n2h-${key}-${inspectOnly?'before':'after'}.png`),new Uint8Array(await preview.arrayBuffer()));
 }
 if(!inspectOnly)await (await SpreadsheetFile.exportXlsx(current)).save(target);
 console.log(JSON.stringify({mode:inspectOnly?'inspect':'sync',changedCells:inspectOnly?0:11,calibrationRevision:d.summary.calibration_revision}));
 process.exit(0);
}
// Narrow update of the delivered workbook; all other cells and sheet structure stay in place.
if(process.argv.includes('--sync-entry16') || process.argv.includes('--inspect-entry16')){
 const target=path.join(out,'FightMatch-首轮配表-v0.2.1.xlsx');
 const current=await SpreadsheetFile.importXlsx(await FileBlob.load(target));
 const patches={
  '使用说明':{
   A2:'0.2.1 · 2026-09-15 首入16修订 · 首章 1～16 关；数值候选用于初版，尚未经过真机试玩。',
   B8:'8 法师；16 战前首证、首通第二证',
   C8:'15 普通复习；16 先确认 Lv.5 嘲讽、说明保护法师，再完整播放准备与 Boss 登场。两枚均通用。',
   B12:`${d.summary.checks} 项算例检查，54 组 Boss 对照，200 条广告样本`,
   B14:'内容成长 v4；战斗沿用 v3',
   C14:'14／15 旧首证与独立练习已覆盖。首次赠证、确认学习和教学进度须跨重进保留；持久化待实现。'
  },
  '首章关卡':{
   A2:'16 首次入场赠通用证、确认学嘲讽并说明后开战；整场首通再赠第二枚。教学时长与 Boss 动作预算分开。',
   K18:'',K19:'普通复习备战',K20:'首次入场：通用证 1、确认学嘲讽\n整场首通：通用证 1'
  },
  '成长验算':{A2:'无广告、无卡回放：16 入场赠证并确认学习后开战。偏法师样本在16确认用卡：W4＋157 → W5＋42。详细事件见成长 CSV。'},
  'Boss分支':{A2:'远程 26、近战 12×1.6；护壳60/25/15，核心90/10/5。无嘲讽仅为反事实对照，不是当前教学允许的首入流程。',A9:'反事实：未学嘲讽'},
  '物品配方':{
   D8:'角色页确认用卡，多余经验保留；16 教学可手动用已有 1 张补战士至5级，不自动扣卡',
   B9:'通用',C9:'16 首次入场赠1；16整场首通再赠1',
   D9:'首次16战前确认学战士嘲讽，需5级；完成学习与说明再开战',
   E9:'不倍增；跨重进不重复赠/扣，重开不退学习',
   E14:'仅返还本次战斗消耗；不退战前用卡或学习'
  }
 };
 if(process.argv.includes('--inspect-entry16')){
  console.log((await current.inspect({kind:'region',sheetId:'首章关卡',range:'A18:K20',maxChars:1500})).ndjson);
  console.log(JSON.stringify({formulas:current.worksheets.getItem('首章关卡').getRange('C18:E20').formulas}));
  const preview=await current.render({sheetName:'首章关卡',range:'F17:K20',scale:1,format:'png'});
  await fs.writeFile(path.join(out,'previews/entry16-before.png'),new Uint8Array(await preview.arrayBuffer()));
  process.exit(0);
 }
 for(const [name,cells] of Object.entries(patches)){
  const sheet=current.worksheets.getItem(name);
  for(const [cell,value] of Object.entries(cells))sheet.getRange(cell).values=[[value]];
 }
 current.worksheets.getItem('首章关卡').getRange('A20:K20').format.rowHeight=61;
 console.log((await current.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!',options:{useRegex:true,maxResults:20},summary:'Formula errors'})).ndjson);
 const growth=current.worksheets.getItem('成长验算');
 for(let i=0;i<d.growth.length;i++){
  const actual=growth.getRange(`K${i+5}:L${i+5}`).values[0];
  if(actual[0]!==d.growth[i].warrior_battle_xp || (d.growth[i].mage_battle_xp!=='' && actual[1]!==d.growth[i].mage_battle_xp))throw Error(`XP reconciliation row ${i+5}`);
 }
 await (await SpreadsheetFile.exportXlsx(current)).save(target);
 for(const [name,range] of [['使用说明','A7:C14'],['首章关卡','F17:K20'],['成长验算','A1:N5'],['Boss分支','A1:I11'],['物品配方','A8:E9']]){
  const preview=await current.render({sheetName:name,range,scale:1,format:'png'});
  await fs.writeFile(path.join(out,`previews/${name}-entry16.png`),new Uint8Array(await preview.arrayBuffer()));
 }
 console.log(JSON.stringify({output:target,changedCells:Object.values(patches).reduce((n,c)=>n+Object.keys(c).length,0),reconciledGrowthRows:d.growth.length,revision:d.config.revision}));
 process.exit(0);
}
const wb=Workbook.create();
const names=['使用说明','参数','职业','技能状态','首章关卡','逐敌配置','成长验算','Boss分支','奖励轮盘','被动频率','物品配方'];
const sheets=Object.fromEntries(names.map(n=>[n,wb.worksheets.add(n)]));
const navy='#243D50',teal='#187D80',ink='#253446',muted='#637587',input='#FFF5DB';
let tid=0;
function col(n){let s='';for(n++;n;n=Math.floor((n-1)/26))s=String.fromCharCode(65+(n-1)%26)+s;return s;}
function table(name,title,note,headers,rows,widths){
 const s=sheets[name],last=col(headers.length-1),end=4+rows.length;
 s.showGridLines=false;s.tabColor=teal;
 s.getRange(`A1:${last}${Math.max(end,20)}`).format={font:{name:'Microsoft YaHei',size:10,color:ink},verticalAlignment:'center',rowHeight:32};
 s.getRange(`A1:${last}1`).merge();s.getRange('A1').values=[[title]];s.getRange('A1').format.font={name:'Microsoft YaHei',size:18,bold:true,color:navy};s.getRange('A1').format.rowHeight=36;
 s.getRange(`A2:${last}2`).merge();s.getRange('A2').values=[[note]];s.getRange('A2').format={font:{size:10,color:muted},wrapText:true,rowHeight:43};
 s.getRange(`A4:${last}4`).values=[headers];s.getRange(`A4:${last}4`).format={fill:navy,font:{bold:true,color:'#FFFFFF'},wrapText:true,rowHeight:37};
 if(rows.length){s.getRange(`A5:${last}${end}`).values=rows;s.getRange(`A5:${last}${end}`).format.wrapText=true;for(let r=5;r<=end;r++)if(r%2===0)s.getRange(`A${r}:${last}${r}`).format.fill='#F1F6F8';}
 widths.forEach((w,i)=>s.getRange(`${col(i)}1:${col(i)}${end}`).format.columnWidth=w);
 s.freezePanes.freezeRows(4);s.tables.add(`A4:${last}${end}`,true,`Config${++tid}`);
 return s;
}
function f(s,cell,formula,format){s.getRange(cell).formulas=[[formula]];if(format)s.getRange(cell).setNumberFormat(format);}
function inputCells(s,range,format){s.getRange(range).format.fill=input;if(format)s.getRange(range).setNumberFormat(format);}

table('使用说明','FightMatch 首轮配表','0.2.1 · 2026-09-15 首入16修订 · 首章 1～16 关；数值候选用于初版，尚未经过真机试玩。',
 ['项目','当前结果','使用方式'],[
 ['下一阶段','配表完成后专注架构','架构与策划联合复核后执行初版，视觉及实际节奏在初版评审。'],
 ['参数与公式','浅黄色为可修改输入','职业属性、广告期望等公式自动更新；战斗和成长记录是模型回放快照。'],
 ['基线成长','不看广告、不用经验卡','战士第 12 关达到 5 级；首 Boss 前战士 5 级、法师 4 级。'],
 ['内容节点','8 法师；16 战前首证、首通第二证','15 普通复习；16 先确认 Lv.5 嘲讽、说明保护法师，再完整播放准备与 Boss 登场。两枚均通用。'],
 ['节奏预算','普通平均 17.16 秒；Boss 标准 53 秒','第 8 关预算 26.9 秒，重点确认连续高物抗是否影响爽感。时间尚未实测。'],
 ['角色技能','四职业初值齐全','首章回放关闭被动、火球、道具与 DOT；后续职业需要接入正式战斗规则重测。'],
 ['概率','轮盘分布与 PRD 分开','轮盘使用固定权重；PRD 连败递增，每关重置会降低短关实际触发率。'],
 ['校准范围',`${d.summary.checks} 项算例检查，54 组 Boss 对照，200 条广告样本`,'不是整款游戏的测试数量。未验证任意阵容、任意路线或 Android 性能。'],
 ['棋盘','17 面默认路径试排','默认路线相邻且互不相交，不要求铺满，不证明唯一解；趣味性仍待试玩。'],
 ['来源','内容成长 v4；战斗沿用 v3','14／15 旧首证与独立练习已覆盖。首次赠证、确认学习和教学进度须跨重进保留；持久化待实现。'],
 ['Skill 方法','game-balance-analysis + Spreadsheets','前者指导可执行算例、参数对照与留档；后者负责公式和表格核对。'],
 ['规则复核','首章评分与完整职业评分分开标注','首章回放含伤害和有效嘲讽；盾骑士等新增贡献权重需在联合复核时统一。']
 ],[20,43,91]).getRange('A5:C16').format.rowHeight=49;

const params=[
 ['比较等级',5,'级','职业页统一比较，不代表角色当前存档'],
 ['每级生命成长',.08,'比例/级','相对 1 级基础线性增加'],
 ['每级攻击成长',.06,'比例/级','相对 1 级基础线性增加'],
 ['每级防御增加',1.5,'点/级','物防与法防的候选共同增量'],
 ['升级经验常数',60,'经验','本级所需经验的常数项'],
 ['升级经验线性项',20,'经验','乘以 L−1'],
 ['升级经验平方项',5,'经验','乘以 (L−1)²'],
 ['普通关基础经验常数',18,'经验','加关卡序号项'],
 ['每关基础经验增加',2,'经验/关','首章基准'],
 ['Boss 基础经验倍率',3,'倍','只对 Boss 基础经验使用'],
 ['评分起始倍率',.5,'倍','固定首章算例；零贡献角色仍有此倍率'],
 ['评分对数系数',.5,'倍','乘 log2(1＋贡献比)'],
 ['参考贡献除数',2,'无量纲','整关敌方初始 HP 总和除以此值'],
 ['超等级宽容',1,'级','超过推荐等级＋1 后开始衰减'],
 ['每超一级经验系数',.75,'倍','按角色自己入场等级计算'],
 ['单张经验卡',50,'经验','经验卡保留多余经验'],
 ['普通关固定时间',3.2,'秒','含 0.8 秒准备；不含结算和广告'],
 ['普通关每次行动',3.95,'秒','走线＋反馈的时间预算'],
 ['Boss 每次行动',4,'秒','未实测'],
 ['Boss 固定时间',9,'秒','开场 3＋翻面 1＋读第二面 3＋补线 2'],
 ['局外恢复',180,'秒','多个倒下角色并行、离线计时'],
 ['库存每叠上限',99,'个','战前自动补充；战内不续装'],
 ['局外资源广告次数',3,'次/日','第 8 关后开启；不限制复活广告']
];
const p=table('参数','全局数值参数','浅黄色可改；Excel 局部公式即时变化，战斗回放需重新运行策划模型。',['参数','数值','单位','说明'],params,[29,16,17,80]);inputCells(p,'B5:B27','0.00');['B5','B9','B10','B11','B12','B13','B14','B17','B18','B20','B25','B26','B27'].forEach(c=>p.getRange(c).setNumberFormat('0'));['B6','B7','B19'].forEach(c=>p.getRange(c).setNumberFormat('0.0%'));

const roleRows=d.config.roles.map(r=>[r.id,r.name,r.hp,r.attack,r.pdef,r.mdef,r.range,r.unlock,r.p0,r.p_per_level,r.p_cap,null,null,null]);
const roles=table('职业','职业属性与成长','战士、法师生命/攻击/物防沿用首章模型；其余属性与后续职业为首轮候选。0 表示开局获得。',
 ['ID','职业','1级生命','1级攻击','1级物防','1级法防','普攻射程','解锁关','被动基础率','每级增加','被动上限','比较生命','比较攻击','比较概率'],roleRows,[8,21,13,13,13,13,13,13,17,15,15,16,16,17]);
inputCells(roles,'C5:K8','0');roles.getRange('I5:K8').setNumberFormat('0.0%');
for(let r=5;r<=8;r++){f(roles,`L${r}`,`=C${r}*(1+'参数'!$B$6*('参数'!$B$5-1))`,'0.00');f(roles,`M${r}`,`=D${r}*(1+'参数'!$B$7*('参数'!$B$5-1))`,'0.00');f(roles,`N${r}`,`=MIN(K${r},I${r}+J${r}*('参数'!$B$5-1))`,'0.0%');}

table('技能状态','技能与状态参数','5 级＋对应职业之证解锁专属技能。CD 从释放后的下一次完整行动开始减少。数值是试调起点。',
 ['ID','效果','首轮数值','持续/CD','范围','触发与消耗条件'],[
 ['W_TAUNT','战士嘲讽','改变单体目标','1 敌方阶段 / CD 2','技能无限；普攻 1','普攻后所连目标存活时释放；战士必须是敌人合法攻击对象。'],
 ['K_SHIELD','盾骑士群盾','每名队友护盾＝攻击×0.8，向下取整','1 敌方阶段 / CD 3','其他存活队友','击杀仍可施放；仅剩自己时省 CD。护盾吸收直接伤害和 DOT。'],
 ['M_FIRE','爆裂火球','中心攻击×0.8；左右各×0.4','CD 2','锁定中心及左右各一位','普攻后仍有有效目标才释放；锁定行动开始时位置，不因死亡补位扩大范围。'],
 ['A_BIND','丛林束缚','取消目标本阶段的一次攻击','1 敌方阶段 / CD 3','所连、存活的当前前排','敌人即将攻击且可控时施放；行为循环继续，不把重击永久顺延。'],
 ['W_CRIT','暴击','物理直接伤害×1.5','每个有效命中判一次','能暴击的普攻','不是 DOT/火球暴击；闪避后不判命中被动。'],
 ['M_PEN','奥术穿透','忽略 50% 法防','普通法术命中时','普攻目标','火球、DOT 不触发；普通法防先减半再计算伤害。'],
 ['K_BLOCK','物理格挡','物理直接伤害减少 50%','每次有效受击','自己','不中断施毒；中毒不影响格挡能力。'],
 ['A_DOUBLE','双射','追加 1 发普通箭','每次普攻最多一次','原目标','不递归；尸体第二箭仅表现，不耗毒箭、不计真实贡献。'],
 ['POISON','中毒','施加者攻击×0.15，向下取整','3 次结算','命中且非免疫目标','敌方行动前统一跳；闪避施毒可避免中毒，毒跳伤不闪避、不格挡。'],
 ['BURN','烧伤','施加者攻击×0.15×火弱点，最后向下取整','2 次结算','可燃材质','同类刷新不叠层，保留较强伤害；植物可燃，金属免烧伤。'],
 ['ELEMENT','属性伤害','弱点×1.5 / 抗性×0.5 / 普通×1','每次伤害一次','按敌人配置','直接火伤与能否烧伤独立；首章机械免毒/烧伤但不免疫全部火伤。']
 ],[17,21,43,28,31,76]).getRange('A5:F15').format.rowHeight=59;

const l=table('首章关卡','第一章关卡与投放','16 首次入场赠通用证、确认学嘲讽并说明后开战；整场首通再赠第二枚。教学时长与 Boss 动作预算分开。',
 ['关卡','推荐等级','基础经验','行动数','时间预算秒','敌人顺序','参考出手','首通卡','锡片/胜利','木料/胜利','首次投放'],
 d.stages.map(r=>[r.stage,r.recommended_level,null,r.actions,null,r.enemy_order,r.reference_plan,r.card_first_clear,r.tin_per_win,r.wood_per_win,r.stage===8?'法师、飞斧配方、资源广告':r.stage===15?'普通复习备战':r.stage===16?'首次入场：通用证 1、确认学嘲讽\n整场首通：通用证 1':'']),[10,13,16,13,19,34,42,13,16,16,44]);
for(let r=5;r<=20;r++){f(l,`C${r}`,`=('参数'!$B$12+'参数'!$B$13*A${r})*IF(A${r}=16,'参数'!$B$14,1)`,'0');f(l,`E${r}`,`=IF(A${r}=16,'参数'!$B$24+D${r}*'参数'!$B$23,'参数'!$B$21+D${r}*'参数'!$B$22)`,'0.00');}
l.getRange('A5:K20').format.rowHeight=47;
l.getRange('A20:K20').format.rowHeight=61;

const enemyNames={E01:'软糖小怪',E02:'发条木偶',E03:'锡甲卫兵',E05:'弹簧投球手',H:'上弦小工'};
const intent={E01:'每回合近战，基攻×0.6',E02:'蓄力 / 重击，基攻×1.3',E03:'每回合近战，基攻×0.65',E05:'瞄准 / 远射，基攻×0.9',H:'攻击 / 上弦；攻击×0.25；恢复两次'};
table('逐敌配置','普通关敌人实例','A/B/C 是关内实例 ID；与职业 ID 分属不同类型。机械免中毒/烧伤；软糖普通受性，无基础闪避。',
 ['关卡','实例','类型','名称','生命','物防','法防','基础攻击','行动模式','恢复绑定'],
 d.enemies.map(e=>[e.stage,e.id,e.kind,enemyNames[e.kind],e.max_hp,e.pdef,e.mdef,e.e,intent[e.kind],e.link||'无']),[10,10,12,21,13,13,13,17,58,18]);

const g=table('成长验算','无广告成长回放','无广告、无卡回放：16 入场赠证并确认学习后开战。偏法师样本在16确认用卡：W4＋157 → W5＋42。详细事件见成长 CSV。',
 ['关卡','W入场级','M入场级','敌初始HP','W真实贡献','M真实贡献','W评分倍率','M评分倍率','W等级修正','M等级修正','W经验','M经验','W结算级','M结算级'],
 d.growth.map(r=>[r.stage,r.warrior_level_before,r.mage_level_before,r.initial_enemy_hp,r.warrior_damage+r.warrior_assist_contribution_candidate,r.mage_damage===''?'':r.mage_damage+r.mage_assist_contribution_candidate,null,null,null,null,null,null,r.warrior_level_after,r.mage_level_after]),[10,14,14,17,18,18,18,18,18,18,14,14,14,14]);
for(let r=5;r<=20;r++){
 for(const [side,contrib,lev,score,cor,xp] of [['W','E','B','G','I','K'],['M','F','C','H','J','L']]){
 const absent=side==='M';
 let formula=`'参数'!$B$15+'参数'!$B$16*LOG(1+${contrib}${r}/(D${r}/'参数'!$B$17),2)`;
 f(g,`${score}${r}`,`=${absent?`IF(C${r}="","",${formula})`:formula}`,'0.000');
 formula=`'参数'!$B$19^MAX(0,${lev}${r}-'首章关卡'!B${r}-'参数'!$B$18)`;
 f(g,`${cor}${r}`,`=${absent?`IF(C${r}="","",${formula})`:formula}`,'0.000');
 formula=`ROUNDDOWN('首章关卡'!C${r}*${score}${r}*${cor}${r},0)`;
 f(g,`${xp}${r}`,`=${absent?`IF(C${r}="","",${formula})`:formula}`,'0');
 }
}

const bossLabels={clear_first_correct_taunt:'正确嘲讽、先清小工',leave_healer_one_real_heal:'留小工一次有效恢复',waste_taunt_on_both_rests:'两面休整各误耗一次',miss_one_ranged_after_first_rest:'第一面漏接一次',no_taunt_clear_first:'反事实：未学嘲讽',full_health_retained_then_two_heals:'保留次数、连续两次恢复',kill_shell_before_winding:'先杀护壳，再清小工'};
const b=table('Boss分支','发条熊王分支对照','远程 26、近战 12×1.6；护壳60/25/15，核心90/10/5。无嘲讽仅为反事实对照，不是当前教学允许的首入流程。',
 ['分支','W等级','M等级','行动数','W剩余HP','M剩余HP','实际恢复','保护次数','预算秒'],d.boss.map(r=>[bossLabels[r.scenario],r.warrior_level,r.mage_level,r.total_actions,r.warrior_hp_remaining,r.mage_hp_remaining,r.actual_shell_healing,r.phase_1_ranged_redirects+r.phase_2_ranged_redirects,null]),[43,12,12,14,19,19,18,17,18]);
for(let r=5;r<5+d.boss.length;r++)f(b,`I${r}`,`=D${r}*'参数'!$B$23+'参数'!$B$24`,'0.0');b.getRange('E5:G11').setNumberFormat('0.00');b.getRange('A5:I11').format.rowHeight=43;

const ads=table('奖励轮盘','结算倍率与取整','看完广告获得总倍率。整数物品先按同类合并，再乘并向上取整；职业/证书不翻倍。',
 ['总倍率','概率','期望贡献','概率累计','说明'],d.config.ads.multipliers.map((m,i)=>[m,d.config.ads.weights[i],null,null,'随机停针，扇区不可误导概率']),[17,17,21,20,61]);
inputCells(ads,'A5:B9');ads.getRange('A5:A9').setNumberFormat('0.0"x"');ads.getRange('B5:B9').setNumberFormat('0.0%');
for(let r=5;r<=9;r++){f(ads,`C${r}`,`=A${r}*B${r}`,'0.000');f(ads,`D${r}`,`=SUM($B$5:B${r})`,'0.0%');}
ads.getRange('A11').values=[['合计']];f(ads,'B11','=SUM(B5:B9)','0.0%');f(ads,'C11','=SUM(C5:C9)','0.000"x"');
ads.getRange('A14:E14').values=[['原数量','期望数量','实际期望倍率','最少数量','最多数量']];ads.getRange('A14:E14').format={fill:navy,font:{bold:true,color:'#FFFFFF'},rowHeight:35};
for(let r=15;r<=20;r++){ads.getRange(`A${r}`).values=[[r-14]];f(ads,`B${r}`,`=${[5,6,7,8,9].map(i=>`ROUNDUP(A${r}*$A$${i},0)*$B$${i}`).join('+')}`,'0.00');f(ads,`C${r}`,`=B${r}/A${r}`,'0.000"x"');f(ads,`D${r}`,`=ROUNDUP(A${r}*MIN($A$5:$A$9),0)`,'0');f(ads,`E${r}`,`=ROUNDUP(A${r}*MAX($A$5:$A$9),0)`,'0');}

const pr=table('被动频率','PRD 的长期与短关频率','每次失败提高下次概率；成功重置。首次概率 C 由长期目标反求；短关数据为解析计算，无抽样误差。',
 ['职业','等级','长期目标','PRD 常数 C','首次概率','头3次均率','头6次均率','最迟触发次数'],d.prd.map(r=>[r.role,r.level,r.target_probability,r.prd_c,r.first_opportunity,r.first_3_average,r.first_6_average,r.guaranteed_by_opportunity]),[22,12,20,23,20,21,21,23]);pr.getRange('C5:G20').setNumberFormat('0.00%');

table('物品配方','消耗品与资源投放','每职业一个战术槽，每叠 99；职业专属、开启后按条件自动使用。新装配默认开，记忆角色选择。',
 ['物品/入口','职业','来源与数量','使用条件','本版范围'],[
 ['飞斧','战士','锡片 3＋木料 1 → 飞斧 6','仅在目标超过近战1、位于远程2以内时替代普攻；一次发射消耗 1','第 8 关开放；普通近战不耗'],
 ['投锤','盾骑士','锡片 4＋木料 1 → 投锤 6','与飞斧相同距离补偿；物理伤害系数 1','第 21 关后续候选'],
 ['毒箭','弓箭手','木料 2＋毒囊 1 → 毒箭 6','实际对活目标发射的每箭消耗 1；命中后按中毒配置施加','第 37 关后续候选；毒囊来源随主题投放'],
 ['小型经验卡','全职业','每张 50；4/8/12 首通各1，16首通2','角色页确认用卡，多余经验保留；16 教学可手动用已有 1 张补战士至5级，不自动扣卡','按实际同类数量使用结算倍率'],
 ['职业之证','通用','16 首次入场赠1；16整场首通再赠1','首次16战前确认学战士嘲讽，需5级；完成学习与说明再开战','不倍增；跨重进不重复赠/扣，重开不退学习'],
 ['常规材料','合成','普通关材料见关卡表；16关锡片4＋木料2','先合并同种掉落，再分别乘结算倍率并向上取整','首章固定供给；暂无随机稀有材料'],
 ['资源广告','局外','8～16关进度：锡片4、木料2、经验卡1','每日3次，玩家主动领取；该广告不再套结算轮盘','后续章节按三场常规关材料再分档'],
 ['局内复活','单角色','一次广告满血复活1人，次数不限','撤销保留已获一次权益，同一未回退历史不可反复使用','不复原其他角色或重复敌方行动'],
 ['广告通关','当前关','普通固定奖励＋角色基础经验','不带评分/等级修正；再选择结算广告时才套当次倍率','已实际用掉的道具才扣；证仍按首通'],
 ['库存整理','背包/战前','同种散落数量合并，优先满叠上阵','只能从已有库存补充；战斗中不补充','仅返还本次战斗消耗；不退战前用卡或学习']
 ],[24,18,62,83,49]).getRange('A5:E14').format.rowHeight=61;

await fs.mkdir(out,{recursive:true});
const previewDir=path.join(out,'previews');await fs.mkdir(previewDir,{recursive:true});
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:100},summary:'Formula errors'})).ndjson);
// Compare spreadsheet formulas with independently computed planning results.
for(let i=0;i<d.growth.length;i++){
 const r=i+5; const actual=g.getRange(`K${r}:L${r}`).values[0];
 if(actual[0]!==d.growth[i].warrior_battle_xp || (d.growth[i].mage_battle_xp!=='' && actual[1]!==d.growth[i].mage_battle_xp))throw Error(`XP reconciliation row ${r}: ${actual}`);
}
const actualMean=ads.getRange('C11').values[0][0];if(Math.abs(actualMean-2.32)>1e-9)throw Error('Ad mean mismatch');
if(Math.abs(ads.getRange('B15').values[0][0]-2.65)>1e-9)throw Error('Item rounding mismatch');
const output=await SpreadsheetFile.exportXlsx(wb);await output.save(path.join(out,'FightMatch-首轮配表-v0.2.1.xlsx'));
for(const name of names){const preview=await wb.render({sheetName:name,range:`A1:${name==='职业'||name==='成长验算'?'N':name==='首章关卡'?'K':name==='逐敌配置'?'J':name==='Boss分支'?'I':name==='被动频率'?'H':name==='技能状态'?'F':name==='使用说明'?'C':name==='参数'?'D':'E'}${name==='奖励轮盘'?20:name==='参数'?18:Math.min(16,5+(name==='职业'?3:11))}`,scale:1,format:'png'});await fs.writeFile(path.join(previewDir,`${name}.png`),new Uint8Array(await preview.arrayBuffer()));}
console.log(JSON.stringify({output:path.join(out,'FightMatch-首轮配表-v0.2.1.xlsx'),sheets:names.length,reconciledGrowthRows:d.growth.length,rendered:names.length}));
