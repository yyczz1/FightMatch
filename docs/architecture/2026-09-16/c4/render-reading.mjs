// Build a local reading page from the C4 guide using an existing marked module.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { createHash } from 'node:crypto';

const root = path.dirname(fileURLToPath(import.meta.url));
const args = process.argv.slice(2);
const moduleIndex = args.indexOf('--marked');
if (moduleIndex < 0 || !args[moduleIndex + 1]) throw new Error('Supply the existing marked module with --marked.');
const { marked } = await import(pathToFileURL(path.resolve(args[moduleIndex + 1])).href);
const source = fs.readFileSync(path.join(root, 'README.md'), 'utf8');
let section = 0;
const body = marked.parse(source)
  .replace(/<h2>/g, () => `<h2 id="section-${++section}">`)
  .replace(/<img src="([^"]+)\.light\.png" alt="([^"]*)"\s*\/?\s*>/g, (_, base, alt) => `<picture><source media="(prefers-color-scheme: dark)" srcset="${base}.dark.png"><img src="${base}.light.png" alt="${alt}" loading="lazy"></picture>`)
  .replace(/<table>/g, '<div class="table-wrap"><table>').replace(/<\/table>/g, '</table></div>');
const html = `<!doctype html><html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>FightMatch · C4 架构阅读</title><style>
:root{color-scheme:light dark;--bg:#f4f7f9;--paper:#fff;--ink:#173247;--muted:#546c7e;--line:#d8e3eb;--accent:#08766d}*{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--ink);font:16px/1.85 'Microsoft YaHei UI','Microsoft YaHei',sans-serif}main{max-width:1500px;margin:30px auto;padding:32px 40px;background:var(--paper);border:1px solid var(--line);border-radius:14px}h1{font-size:32px;line-height:1.4}h2{font-size:24px;margin-top:44px;padding-top:24px;border-top:1px solid var(--line);scroll-margin-top:22px}p{max-width:1100px}a{color:var(--accent);text-underline-offset:3px}nav{display:flex;flex-wrap:wrap;gap:12px 24px;margin:24px 0}picture,img{display:block;width:100%;height:auto}picture{margin:24px 0;border:1px solid var(--line);border-radius:8px;overflow:hidden}.table-wrap{overflow-x:auto}table{width:100%;border-collapse:collapse;font-size:14px}th,td{padding:12px;border:1px solid var(--line);text-align:left;vertical-align:top}th{background:var(--bg)}code{font-family:Consolas,monospace}footer{margin-top:36px;color:var(--muted);font-size:13px}@media(prefers-color-scheme:dark){:root{--bg:#0c1721;--paper:#15232e;--ink:#e4edf3;--muted:#aec0cd;--line:#354b5b;--accent:#77d4c7}}@media(max-width:650px){body{font-size:15px}main{margin:0;padding:22px 16px;border:0;border-radius:0}h1{font-size:26px}h2{font-size:21px}th,td{padding:8px}nav{gap:8px 16px}}
</style></head><body><main><nav aria-label="层级导航"><a href="#section-1">C1 整个产品</a><a href="#section-2">C2 应用与存储</a><a href="#section-3">C3 客户端内部</a><a href="../index.html">系统协作基线</a></nav>${body}<footer>图源为本目录 *.architecture.json。静态阅读图与交互 HTML 分别维护证据；本页不加载外部脚本或字体。</footer></main></body></html>`;
const targets = [...html.matchAll(/(?:href|src|srcset)="([^"]+)"/g)].map(m => m[1]).filter(value => !/^(https?:|#)/.test(value));
const missing = targets.filter(target => !fs.existsSync(path.resolve(root, decodeURIComponent(target.split('#')[0]))));
if (missing.length) throw new Error('Missing local targets: ' + missing.join(', '));
fs.writeFileSync(path.join(root, 'index.html'), html);
const hash = value => createHash('sha256').update(value).digest('hex');
const receipt = {source:'README.md',output:'index.html',sourceSha256:hash(source),outputSha256:hash(html),sections:section,localTargetsChecked:targets.length,missingTargets:missing};
fs.writeFileSync(path.join(root, 'reading-receipt.json'), JSON.stringify(receipt,null,2)+'\n');
console.log(JSON.stringify(receipt));
