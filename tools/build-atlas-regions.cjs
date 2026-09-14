// Detect each authored pose without changing any source PNG pixels.
// Re-run when replacing these sheets; inspect the preview before committing.
const fs = require('node:fs');
const path = require('node:path');
const inspect = require('./inspect-animation-art.cjs');
const root = path.resolve(__dirname, '..');
const file = path.join(root, 'web/assets/animations/manifest.json');
const manifest = JSON.parse(fs.readFileSync(file, 'utf8'));
const heights = {logan:[96,1.62], thornling:[82,1.15], 'mossback-guardian':[225,3.15]};

(async function () {
  for (const def of manifest.characters) {
    const art = await inspect(path.join(path.dirname(file), def.image));
    const slots = Array.from({length:def.columns*def.rows},()=>[]);
    for (const c of art.components) {
      const col = Math.min(def.columns-1, Math.floor((c.x+c.w/2)*def.columns/art.width));
      const row = Math.min(def.rows-1, Math.floor((c.y+c.h/2)*def.rows/art.height));
      slots[row*def.columns+col].push(c);
    }
    def.regions = []; def.pivots = [];
    for (const [index, components] of slots.entries()) {
      const body = components.reduce((a,b)=>!a||b.pixels>a.pixels?b:a, null);
      if (!body || body.pixels < 10000) throw new Error(`${def.id} frame ${index} needs manual inspection`);
      const left = Math.max(0, Math.min(...components.map(c=>c.x))-2);
      let top = Math.max(0, Math.min(...components.map(c=>c.y))-2);
      const right = Math.min(art.width, Math.max(...components.map(c=>c.x+c.w))+2);
      let bottom = Math.min(art.height, Math.max(...components.map(c=>c.y+c.h))+2);
      // These adjacent authored poses approach within four source pixels.
      // Trim their outer tips to keep a neighbour's pixels out of each slice.
      if (def.id === 'logan' && index === 18) bottom = 1250;
      if (def.id === 'logan' && index === 22) top = 1254;
      const region = {x:left,y:top,w:right-left,h:bottom-top};
      // Logan's sword and scarf change the silhouette, but his body stays in
      // the same column. Creatures use their body centre, excluding particles.
      const anchorX = def.id === 'logan' ? [145,394,643,892][index%4] : body.x+body.w/2;
      const anchorY = Math.min(bottom, body.y+body.h);
      def.regions.push(region);
      def.pivots.push({x:+((anchorX-left)/region.w).toFixed(6),y:+((anchorY-top)/region.h).toFixed(6)});
    }
    [def.displayHeight, def.unityHeight] = heights[def.id];
    console.log(`${def.id}: ${slots.length} poses, ${art.width} x ${art.height}`);
  }
  fs.writeFileSync(file, JSON.stringify(manifest,null,2)+'\n');
})();
