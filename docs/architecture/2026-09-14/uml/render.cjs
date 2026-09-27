// Documentation renderer. Uses supplied local runtimes; installs no project dependency.
const fs = require('node:fs');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
const { createHash } = require('node:crypto');
const args = process.argv.slice(2);
const arg = key => args[args.indexOf(key) + 1];
for (const key of ['--mermaid', '--playwright', '--chrome', '--marked']) {
  if (!args.includes(key)) throw new Error(`Missing ${key} local path`);
}
const { chromium } = require(path.resolve(arg('--playwright')));
const root = __dirname;
const views = JSON.parse(fs.readFileSync(path.join(root, 'views.json'), 'utf8'));
const esc = value => String(value).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const hash = value => createHash('sha256').update(value).digest('hex');
const write = (name, content) => fs.writeFileSync(path.join(root, name), content, 'utf8');
function scopeSvgIds(svg, prefix) {
  const ids = [...svg.matchAll(/\bid="([^"]+)"/g)].map(match => match[1]);
  svg = svg.replace(/\bid="([^"]+)"/g, (_, id) => `id="${prefix}${id}"`);
  for (const id of ids) {
    const escaped = id.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    svg = svg.replace(new RegExp('#' + escaped + '(?=[\\s.>{:),"\\]])', 'g'), '#' + prefix + id);
  }
  return svg;
}
const css = `
:root{color-scheme:light;--bg:#f3f6f8;--paper:#fff;--text:#172c3d;--muted:#50697c;--line:#cddbe5;--accent:#047e8a;--note:#fff6de}
:root[data-theme=dark]{color-scheme:dark;--bg:#0d1821;--paper:#142532;--text:#edf4f8;--muted:#b0c5d4;--line:#345065;--accent:#67ded9;--note:#3a301b}
*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font-family:'Microsoft YaHei UI','Microsoft YaHei',sans-serif;line-height:1.75}
main{max-width:1440px;margin:auto;padding:28px 36px 40px}a{color:var(--accent);text-underline-offset:4px}button{font:inherit;cursor:pointer;color:var(--text);background:var(--paper);border:1px solid var(--line);border-radius:8px;padding:6px 12px}button:hover{border-color:var(--accent)}
nav{display:flex;justify-content:space-between;align-items:center;gap:16px;flex-wrap:wrap;font-size:14px}.eyebrow{color:var(--accent);letter-spacing:.12em;font-size:12px;font-weight:700;margin:24px 0 6px}h1{font-size:30px;line-height:1.4;margin:0 0 14px}h2{font-size:20px;margin:28px 0 12px}p{margin:10px 0}.summary{max-width:1060px;color:var(--muted);font-size:16px}.note{border-left:3px solid #d79832;background:var(--note);padding:12px 16px;margin:20px 0;font-size:14px}
.toolbar{display:flex;align-items:center;gap:12px;flex-wrap:wrap;margin:18px 0 10px;font-size:14px}.diagram{background:var(--paper);border:1px solid var(--line);border-radius:12px;overflow:auto;padding:24px}.diagram svg{display:block;width:100%;height:auto;margin:auto}.diagram.actual svg{width:var(--native-width);max-width:none!important}.dark-svg{display:none}:root[data-theme=dark] .dark-svg{display:block}:root[data-theme=dark] .light-svg{display:none}
details{margin-top:20px;padding:12px 16px;border:1px solid var(--line);border-radius:8px}summary{cursor:pointer;font-size:14px}pre{white-space:pre-wrap;overflow-wrap:anywhere;font-family:Consolas,monospace;font-size:13px;line-height:1.6}.grid{display:grid;grid-template-columns:repeat(3,1fr);gap:14px}.card{display:block;background:var(--paper);border:1px solid var(--line);border-radius:12px;padding:18px 20px;text-decoration:none;color:var(--text)}.card:hover{border-color:var(--accent)}.card strong{display:block;font-size:16px}.card span{display:block;color:var(--muted);font-size:14px;margin-top:6px}.chip{display:inline-block;color:var(--accent);font-size:12px;font-weight:700;margin-bottom:8px}.footer{margin-top:24px;font-size:13px;color:var(--muted)}
.reading{max-width:1140px;margin:auto;overflow-wrap:anywhere}.reading h2{padding-top:14px;border-top:1px solid var(--line)}.reading h3{font-size:20px;color:var(--accent);margin-top:32px;scroll-margin-top:24px}.reading li{margin:8px 0}.table-wrap{overflow-x:auto;margin:20px 0}.reading table{width:100%;border-collapse:collapse;font-size:14px;background:var(--paper)}.reading th,.reading td{border:1px solid var(--line);padding:12px 14px;text-align:left;vertical-align:top;min-width:130px}.reading th{background:var(--note)}.reading code{font-family:Consolas,monospace}.reading .contents{display:flex;gap:8px 18px;flex-wrap:wrap;margin:20px 0;font-size:14px}
@media(max-width:900px){main{padding:20px}.grid{grid-template-columns:1fr 1fr}h1{font-size:25px}}@media(max-width:580px){.grid{grid-template-columns:1fr}.diagram{padding:12px}}`;
const themeScript = `<script>document.getElementById('theme').onclick=()=>{const d=document.documentElement;d.dataset.theme=d.dataset.theme==='dark'?'light':'dark'};const z=document.getElementById('zoom');if(z)z.onclick=()=>{document.querySelector('.diagram').classList.toggle('actual');z.textContent=document.querySelector('.diagram').classList.contains('actual')?'适合宽度':'原始尺寸'};</script>`;
const shell = (title, content) => `<!doctype html><html lang="zh-CN" data-theme="light"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>${esc(title)} · FightMatch</title><style>${css}</style></head><body><main>${content}</main>${themeScript}</body></html>`;
const flowViews = [
  ['overview','总架构','System、共享规则与两种状态'],
  ['reuse','代码复用','保留基础工具，复用战斗规则'],
  ['action','一次行动','合法连线、玩家结果与敌方阶段'],
  ['rollback','完整回退','恢复目标行动前的战斗历史'],
  ['progression','养成与结算','成长、装备、开战与收益入账'],
  ['guide','完整攻略','获取、验证与提示权益']
];
(async () => {
  const browser = await chromium.launch({executablePath:path.resolve(arg('--chrome')),headless:true});
  const page = await browser.newPage({viewport:{width:1600,height:1000}});
  const receipt = {renderer:'local Mermaid 10.9.5',views:[],browserChecks:[]};
  try {
    const { marked } = await import(pathToFileURL(path.resolve(arg('--marked'))).href);
    const alignmentFile = path.resolve(root,'..','..','2026-09-15-planning-alignment.html');
    const alignmentSource = fs.readFileSync(alignmentFile.replace(/\.html$/,'.md'),'utf8');
    const contents = [];
    let sectionIndex = 0;
    const alignmentBody = marked.parse(alignmentSource)
      .replace(/<h([23])>(.*?)<\/h\1>/g, (_,level,title) => {
        const id = /^Q\d{2}/.exec(title)?.[0] || 'section-' + (++sectionIndex);
        contents.push(`<a href="#${id}">${title}</a>`);
        return `<h${level} id="${id}">${title}</h${level}>`;
      }).replace(/<table>/g,'<div class="table-wrap"><table>').replace(/<\/table>/g,'</table></div>');
    const alignmentHtml = shell('技能、配置表与策划对齐',`<article class="reading"><nav><a href="2026-09-14/index.html">← 图稿总览</a><a href="2026-09-15-planning-alignment.md">Markdown 源文档</a><button id="theme" type="button">切换明暗</button></nav><p class="eyebrow">FIGHTMATCH / PLANNING ALIGNMENT / 2026.09.15</p><p class="note">玩法问题由策划会话讨论，本页是交给策划的架构依赖清单。已确认：连线自动施放，战士击杀跳过嘲讽，盾骑仍按可用条件放盾。</p><details><summary>跳转到技能、配置目录或具体问题</summary><div class="contents">${contents.join('')}</div></details>${alignmentBody}</article>`);
    fs.writeFileSync(alignmentFile,alignmentHtml,'utf8');
    receipt.alignment = {sourceSha256:hash(alignmentSource),htmlSha256:hash(alignmentHtml)};
    await page.setContent('<!doctype html><html><head><meta charset="utf-8"></head><body></body></html>');
    await page.addScriptTag({path:path.resolve(arg('--mermaid'))});
    for (const item of views) {
      const source = fs.readFileSync(path.join(root, item.id + '.mmd'), 'utf8');
      const svgs = {};
      for (const theme of ['light', 'dark']) {
        svgs[theme] = await page.evaluate(async ({source,id,theme}) => {
          const dark = theme === 'dark';
          mermaid.initialize({startOnLoad:false,securityLevel:'strict',theme:'base',fontFamily:'Microsoft YaHei UI, Microsoft YaHei, sans-serif',
            themeVariables:{sequenceNumberColor:dark?'#172c3d':'#ffffff',darkMode:dark,fontSize:'17px',primaryColor:dark?'#213e51':'#e8f3f6',primaryTextColor:dark?'#ecf4fa':'#193448',primaryBorderColor:dark?'#79a5bd':'#58899e',lineColor:dark?'#acc5d5':'#587386',secondaryColor:dark?'#314f42':'#eaf6ef',tertiaryColor:dark?'#3c3220':'#fff4da',mainBkg:dark?'#213e51':'#e8f3f6',textColor:dark?'#ecf4fa':'#193448',nodeBorder:dark?'#79a5bd':'#58899e',edgeLabelBackground:dark?'#142532':'#ffffff',clusterBkg:dark?'#142532':'#ffffff',actorBkg:dark?'#213e51':'#e8f3f6',actorBorder:dark?'#79a5bd':'#58899e',actorTextColor:dark?'#ecf4fa':'#193448',signalColor:dark?'#acc5d5':'#587386',signalTextColor:dark?'#ecf4fa':'#193448',noteBkgColor:dark?'#3c3220':'#fff4da',noteTextColor:dark?'#ecf4fa':'#193448'},
            flowchart:{htmlLabels:false,nodeSpacing:35,rankSpacing:50},class:{useMaxWidth:false},sequence:{useMaxWidth:false,actorMargin:35,messageMargin:35}});
          const result = await mermaid.render(id + '_' + theme, source);
          return result.svg;
        }, {source,id:'fm_' + item.id.replace(/-/g,'_'),theme});
        svgs[theme] = scopeSvgIds(svgs[theme], item.id + '_' + theme + '__');
        write(item.id + '.' + theme + '.svg', svgs[theme]);
      }
      const nativeWidth = svgs.light.match(/viewBox="[^"]*?\s([\d.]+)\s[\d.]+"/)?.[1] || '1200';
      const html = shell(item.title, `<nav><a href="../index.html">← 图稿总览</a><a href="../../2026-09-15-planning-alignment.html">技能、配置与具体问题</a><button id="theme" type="button">切换明暗</button></nav><p class="eyebrow">FIGHTMATCH / UML / DISCUSSION DRAFT</p><h1>${esc(item.title)}</h1><p class="summary">${esc(item.summary)}</p><p class="note">${esc(item.note)}</p><div class="toolbar"><button id="zoom" type="button">原始尺寸</button><a href="${esc(item.id)}.light.svg" download>导出浅色 SVG</a><a href="${esc(item.id)}.dark.svg" download>导出深色 SVG</a><a href="${esc(item.related)}">关联流程与说明 ↗</a></div><div class="diagram" style="--native-width:${nativeWidth}px"><div class="light-svg">${svgs.light}</div><div class="dark-svg">${svgs.dark}</div></div><details><summary>查看可编辑 Mermaid 源码</summary><p><a href="${esc(item.id)}.mmd" download>保存 .mmd 文件</a></p><pre>${esc(source)}</pre></details><p class="footer">候选类与代表性方法，用于讨论职责；除 Existing 类型外，均未实施。可用原始尺寸检查细节。</p>`);
      write(item.id + '.html', html);
      receipt.views.push({id:item.id,sourceSha256:hash(source),htmlSha256:hash(html),lightSvgSha256:hash(svgs.light),darkSvgSha256:hash(svgs.dark),render:'passed'});
    }
    const index = shell('架构讨论稿', `<nav><a href="../2026-09-14-fightmatch-architecture-draft.md">架构说明</a><button id="theme" type="button">切换明暗</button></nav><p class="eyebrow">FIGHTMATCH / ARCHITECTURE / UPDATED 2026.09.15</p><h1>根据已确认玩法，梳理模块与数据。</h1><p class="summary">QFramework 组织 Unity 客户端；普通 C# 规则由 System 调用，也供编辑器与攻略验证复用。主动技能、被动概率、CD 和配置表现在分别展开。</p><p class="note">候选架构，尚未批准实施。连线自动施放已定，具体技能各自判断；CD 递减等玩法细则交由策划会话确认。</p><div class="grid"><a class="card" href="../2026-09-15-planning-alignment.html"><small class="chip">本轮先看</small><strong>策划对齐与具体问题</strong><span>由架构整理背景与影响，交策划会话和用户讨论；确认后同步图稿。</span></a><a class="card" href="uml/skills.html"><small class="chip">新增展开图</small><strong>被动概率与主动 CD</strong><span>配置、每个角色的技能状态、PRD 与完整回退。</span></a><a class="card" href="uml/configuration.html"><small class="chip">新增展开图</small><strong>等级属性与敌人配置</strong><span>职业等级行、敌人数值档、关卡引用与只读配置目录。</span></a></div><h2>架构与流程 · Archify</h2><div class="grid">${flowViews.map((v,i)=>`<a class="card" href="${v[0]}.html"><small class="chip">${String(i+1).padStart(2,'0')} / ${i<2?'架构':'流程'}</small><strong>${esc(v[1])}</strong><span>${esc(v[2])}</span></a>`).join('')}</div><h2>模块类图与调用时序 · Mermaid</h2><div class="grid">${views.map(v=>`<a class="card" href="uml/${v.id}.html"><strong>${esc(v.title)}</strong><span>${esc(v.summary)}</span></a>`).join('')}</div><h2>阅读顺序</h2><p class="summary">本会话讨论模块职责、状态归属、接口与技术取舍。玩法依赖交给策划会话；收到已确认规则后更新类图与时序，避免两边重复询问。</p><p class="footer"><a href="validation.md">图稿验证记录</a> · 图源：Archify JSON / Mermaid .mmd · SVG 与 HTML 均可离线查看 · 尚无 Unity 编译或游戏运行验证。</p>`);
    fs.writeFileSync(path.join(root,'..','index.html'),index,'utf8');
    receipt.indexSha256 = hash(index);
    if (args.includes('--verify')) {
      const output = path.resolve(arg('--verify'));
      fs.mkdirSync(output,{recursive:true});
      for (const id of ['index','alignment',...views.map(v=>v.id)]) {
        const file = id === 'index' ? path.join(root,'..','index.html') : id === 'alignment' ? alignmentFile : path.join(root,id+'.html');
        const errors = [];
        const errorHandler = e => errors.push(String(e));
        page.on('pageerror',errorHandler);
        await page.setViewportSize({width:1440,height:1000});
        await page.goto(pathToFileURL(file).href);
        await page.evaluate(() => document.fonts.ready);
        const metrics = await page.evaluate(() => {
          const ids = [...document.querySelectorAll('[id]')].map(node=>node.id);
          return {width:innerWidth,scrollWidth:document.documentElement.scrollWidth,svgCount:document.querySelectorAll('svg').length,duplicateIds:ids.length-new Set(ids).size};
        });
        await page.screenshot({path:path.join(output,id+'.light.png'),fullPage:true});
        await page.locator('#theme').click();
        await page.screenshot({path:path.join(output,id+'.dark.png'),fullPage:true});
        if(id === 'alignment') {
          await page.screenshot({path:path.join(output,'alignment.top.dark.png')});
          await page.locator('#theme').click();
          for(const question of ['Q01','Q03']) {
            await page.locator('#'+question).scrollIntoViewIfNeeded();
            await page.screenshot({path:path.join(output,'alignment.'+question+'.light.png')});
          }
          await page.setViewportSize({width:390,height:844});
          await page.evaluate(()=>scrollTo(0,0));
          const mobile = await page.evaluate(()=>({width:innerWidth,scrollWidth:document.documentElement.scrollWidth}));
          await page.screenshot({path:path.join(output,'alignment.mobile.light.png')});
          if(mobile.scrollWidth>mobile.width)errors.push('Alignment page overflows at 390px');
          receipt.alignment.mobile = mobile;
          await page.setViewportSize({width:1440,height:1000});
        }
        const localLinks = await page.locator('a[href]').evaluateAll(links=>links.map(a=>a.getAttribute('href')).filter(h=>h&&!/^(https?:|#)/.test(h)));
        const missingLinks = localLinks.filter(h=>!fs.existsSync(path.resolve(path.dirname(file),h)));
        receipt.browserChecks.push({id,...metrics,pageErrors:errors,missingLinks,passed:metrics.scrollWidth<=metrics.width&&metrics.duplicateIds===0&&errors.length===0&&missingLinks.length===0});
        page.off('pageerror',errorHandler);
      }
    }
    write('render-receipt.json',JSON.stringify(receipt,null,2)+'\n');
    console.log(JSON.stringify({rendered:receipt.views.length,svgCount:receipt.views.length*2,browserChecks:receipt.browserChecks.length,failures:receipt.browserChecks.filter(x=>!x.passed)},null,2));
    if(receipt.browserChecks.some(x=>!x.passed))process.exitCode=1;
  } finally {await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
