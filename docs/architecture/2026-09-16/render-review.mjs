// Generate the local architecture reading page; no project dependency installation.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createHash } from 'node:crypto';

const root = path.dirname(fileURLToPath(import.meta.url));
const args = process.argv.slice(2);
const markedIndex = args.indexOf('--marked');
if (markedIndex < 0 || !args[markedIndex + 1]) throw new Error('Supply the existing marked module with --marked.');
const { marked } = await import(pathToFileURL(path.resolve(args[markedIndex + 1])).href);
const source = fs.readFileSync(path.join(root, 'review.md'), 'utf8');
const headings = [];
let body = marked.parse(source).replace(/<h2>(.*?)<\/h2>/g, (_, label) => {
  const id = 'section-' + (headings.length + 1);
  headings.push({ id, label });
  return `<h2 id="${id}">${label}</h2>`;
}).replace(/<table>/g, '<div class="table-wrap"><table>').replace(/<\/table>/g, '</table></div>');
const nav = headings.map(h => `<a href="#${h.id}">${h.label}</a>`).join('');
const css = `:root{color-scheme:light dark;--bg:#f4f7f9;--paper:#fff;--ink:#173247;--muted:#52677a;--line:#d6e1e8;--accent:#08756e;--tint:#e8f4f1}*{box-sizing:border-box}html{scroll-behavior:smooth}body{margin:0;background:var(--bg);color:var(--ink);font:16px/1.8 'Microsoft YaHei UI','Microsoft YaHei',sans-serif}a{color:var(--accent);text-underline-offset:3px}a:focus-visible{outline:3px solid var(--accent);outline-offset:4px}.top{max-width:1420px;margin:auto;padding:25px 32px 17px;display:flex;justify-content:space-between;gap:20px;flex-wrap:wrap}.brand{font-weight:800;letter-spacing:.08em}.status{color:var(--muted);font-size:14px}.layout{max-width:1420px;margin:auto;display:grid;grid-template-columns:205px minmax(0,1fr);gap:30px;padding:0 32px 55px}.side{position:sticky;top:24px;align-self:start;padding-top:24px}.side p{font-size:12px;color:var(--muted);letter-spacing:.08em}.side a{display:block;text-decoration:none;padding:9px 0;line-height:1.6;font-size:14px}.side .diagram-link{margin-top:18px;padding:12px;background:var(--tint);border-radius:8px;font-weight:700}article{min-width:0;background:var(--paper);border:1px solid var(--line);border-radius:16px;padding:30px 36px 36px}h1{font-size:30px;line-height:1.4;margin:0 0 18px}h2{font-size:22px;line-height:1.5;border-top:1px solid var(--line);margin:38px 0 17px;padding-top:24px;scroll-margin-top:20px}p{margin:12px 0 18px}li{margin:10px 0}strong{font-weight:700}.table-wrap{margin:20px 0}table{width:100%;border-collapse:collapse;table-layout:fixed;font-size:14px;line-height:1.75}th,td{border:1px solid var(--line);padding:12px 13px;text-align:left;vertical-align:top;overflow-wrap:anywhere}th{background:var(--tint);font-weight:700}th:first-child,td:first-child{width:21%}code{font-family:Consolas,monospace;font-size:.95em}.foot{border-top:1px solid var(--line);margin-top:30px;padding-top:16px;color:var(--muted);font-size:13px}.foot a{margin-right:18px}@media(prefers-color-scheme:dark){:root{--bg:#0d1821;--paper:#15232f;--ink:#e4edf4;--muted:#b2c3cf;--line:#354b5b;--accent:#73d2c6;--tint:#203c3e}}@media(max-width:980px){.layout{grid-template-columns:1fr}.side{position:static;display:flex;flex-wrap:wrap;gap:4px 20px;padding-top:0}.side p{width:100%;margin:0}.side a{padding:4px 0}.side .diagram-link{margin:0;padding:5px 10px}article{padding:24px}}@media(max-width:600px){.top,.layout{padding-left:14px;padding-right:14px}.layout{gap:16px}article{padding:19px 15px}h1{font-size:25px}h2{font-size:20px}table{font-size:12px;line-height:1.65}th,td{padding:8px 6px}body{font-size:15px}}`;
const html = `<!doctype html><html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>FightMatch · 系统协作基线 r1</title><style>${css}</style></head><body><header class="top"><span class="brand">FIGHTMATCH / ARCHITECTURE</span><span class="status">2026.09.16 · 推荐基线 r1 · 系统内部设计另行展开</span></header><div class="layout"><nav class="side" aria-label="阅读目录"><p>架构与交接</p>${nav}<a class="diagram-link" href="systems.html">打开交互系统关系图 ↗</a></nav><article>${body}<footer class="foot"><a href="review.md">Markdown 源文档</a><a href="systems.architecture.json">可维护图源</a><a href="validation.md">验证记录</a><p>图上的分组不规定实现类数量。先明确共同约束，再统一系统设计和接口，最后按系统分发执行。</p></footer></article></div></body></html>`;
fs.writeFileSync(path.join(root, 'index.html'), html, 'utf8');
const hash = value => createHash('sha256').update(value).digest('hex');
const links = [...body.matchAll(/href="([^"]+)"/g)].map(m => m[1]);
const missing = links.filter(href => !/^(?:https?:|#)/.test(href)).filter(href => !fs.existsSync(path.resolve(root, decodeURIComponent(href.split('#')[0]))));
if (missing.length) throw new Error('Missing local links: ' + missing.join(', '));
const receipt = {source:'review.md',output:'index.html',sourceSha256:hash(source),outputSha256:hash(html),sections:headings.length,localLinksChecked:links.filter(href=>!/^(?:https?:|#)/.test(href)).length,missingLinks:missing};
fs.writeFileSync(path.join(root, 'review-render-receipt.json'), JSON.stringify(receipt,null,2)+'\n');
console.log(JSON.stringify(receipt,null,2));
