// Create offline reading images from the delivered SVG, without a browser.
// Typography is adapted for static reading; these are not HTML screenshots.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createRequire } from 'node:module';
import { createHash } from 'node:crypto';

const root = path.dirname(fileURLToPath(import.meta.url));
const args = process.argv.slice(2);
const sharpIndex = args.indexOf('--sharp');
if (sharpIndex < 0 || !args[sharpIndex + 1]) throw new Error('Supply the existing sharp module with --sharp.');
const sharp = createRequire(import.meta.url)(path.resolve(args[sharpIndex + 1]));
const hash = input => createHash('sha256').update(input).digest('hex');
const xml = value => value.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;');
const receipt = { evidenceKind: 'offline-svg-rasterization', browserUsed: false, typography: 'static reading sizes; graph geometry and relationships unchanged', outputs: [] };

for (const name of ['c2-runtime', 'c3-client-services']) {
  const html = fs.readFileSync(path.join(root, name + '.html'), 'utf8');
  const spec = JSON.parse(fs.readFileSync(path.join(root, name + '.architecture.json'), 'utf8'));
  const css = html.match(/<style>([\s\S]*?)<\/style>/)?.[1];
  const graph = html.match(/<svg\b[^>]*>([\s\S]*?)<\/svg>/)?.[1];
  if (!css || !graph || /<image\b[^>]*(?:href|src)=["']https?:/i.test(graph)) throw new Error('Missing or externally linked graph: ' + name);
  const [width, graphHeight] = spec.meta.viewBox;
  const height = graphHeight + 205;
  for (const theme of ['light', 'dark']) {
    const baseBlock = css.match(/:root,\s*\[data-theme="dark"\]\s*\{([^}]+)\}/)?.[1] ?? '';
    const themeBlock = theme === 'light' ? css.match(/\[data-theme="light"\]\s*\{([^}]+)\}/)?.[1] ?? '' : '';
    const variables = Object.fromEntries([...`${baseBlock}\n${themeBlock}`.matchAll(/(--[\w-]+)\s*:\s*([^;]+);/g)].map(m => [m[1], m[2].trim()]));
    const resolvedCss = css.replace(/var\((--[\w-]+)(?:,\s*([^()]+))?\)/g, (_, key, fallback) => variables[key] ?? fallback ?? 'initial');
    const graphBody = graph.replace(/font-size="([\d.]+)"/g, (_, size) => `font-size="${Math.max(10, Number(size))}"`);
    const cards = spec.cards.map((card, i) => {
      const x = 32 + i * (width - 64) / 3;
      return `<text x="${x}" y="${graphHeight + 111}" fill="${variables['--text']}" font-size="17" font-weight="600">${xml(card.title)}</text>` +
        card.items.map((item, j) => `<text x="${x}" y="${graphHeight + 140 + j * 24}" fill="${variables['--text-muted']}" font-size="14">${xml(item)}</text>`).join('');
    }).join('');
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}" data-theme="${theme}" data-preset="classic" role="img" aria-label="${xml(spec.meta.title)}"><style><![CDATA[${resolvedCss}
      text { font-family: 'Microsoft YaHei', 'Segoe UI', sans-serif; }
      text[data-node-label] { font-size: 17px; }
      text[data-detail="context"] { font-size: 12px; }
      text[data-detail="fine"] { font-size: 11px; }
    ]]></style><rect width="${width}" height="${height}" fill="${variables['--bg']}"/><text x="32" y="40" fill="${variables['--text']}" font-size="27" font-weight="700">${xml(spec.meta.title)}</text><text x="32" y="65" fill="${variables['--text-muted']}" font-size="13">目标架构扩展稿 · 2026-09-16 · 静态阅读图</text><g transform="translate(0 82)">${graphBody}</g>${cards}</svg>`;
    const svgName = `${name}.${theme}.svg`;
    const pngName = `${name}.${theme}.png`;
    fs.writeFileSync(path.join(root, svgName), svg);
    const png = await sharp(Buffer.from(svg), { density: 144 }).png().toBuffer();
    fs.writeFileSync(path.join(root, pngName), png);
    receipt.outputs.push({ diagram: name, theme, sourceHtmlSha256: hash(html), svg: svgName, svgSha256: hash(svg), png: pngName, pngSha256: hash(png) });
  }
}
fs.writeFileSync(path.join(root, 'static-receipt.json'), JSON.stringify(receipt, null, 2) + '\n');
console.log(JSON.stringify({ images: receipt.outputs.length, browserUsed: false, receipt: 'static-receipt.json' }));
