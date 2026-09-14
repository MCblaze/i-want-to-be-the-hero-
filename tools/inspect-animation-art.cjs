// Development-only atlas inspection. Reads pixels; never alters the artwork.
// Install sharp, or use the runtime's CODEX_PRIMARY_RUNTIME_NODE_MODULES.
const path = require('node:path');
let sharp;
try { sharp = require('sharp'); }
catch (_) { sharp = require(path.join(process.env.CODEX_PRIMARY_RUNTIME_NODE_MODULES, 'sharp')); }

async function inspect(file) {
  const {data, info} = await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const w = info.width, h = info.height;
  const seen = new Uint8Array(w * h), queue = new Int32Array(w * h), components = [];
  for (let start = 0; start < seen.length; start++) {
    if (seen[start] || data[start * 4 + 3] < 64) continue;
    let head = 0, tail = 1, minX = w, minY = h, maxX = 0, maxY = 0;
    queue[0] = start; seen[start] = 1;
    while (head < tail) {
      const p = queue[head++], x = p % w, y = Math.floor(p / w);
      minX = Math.min(minX, x); maxX = Math.max(maxX, x);
      minY = Math.min(minY, y); maxY = Math.max(maxY, y);
      for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) {
        const nx = x + dx, ny = y + dy, n = ny * w + nx;
        if (nx < 0 || nx >= w || ny < 0 || ny >= h || seen[n] || data[n * 4 + 3] < 64) continue;
        seen[n] = 1; queue[tail++] = n;
      }
    }
    if (tail > 400) components.push({pixels:tail, x:minX, y:minY, w:maxX-minX+1, h:maxY-minY+1});
  }
  components.sort((a,b)=>a.y-b.y || a.x-b.x);
  return {file, width:w, height:h, components};
}
if (require.main === module) Promise.all(process.argv.slice(2).map(inspect)).then(results=>console.log(JSON.stringify(results,null,2)));
module.exports = inspect;
