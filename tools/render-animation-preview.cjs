// Optional local rendering check; uses the game's real animation renderer.
// Requires @napi-rs/canvas. Output frames can be encoded with ffmpeg.
const fs = require('node:fs');
const path = require('node:path');
let canvas;
try { canvas = require('@napi-rs/canvas'); }
catch (_) { canvas = require(path.join(process.env.CODEX_PRIMARY_RUNTIME_NODE_MODULES, '@napi-rs/canvas')); }
const {AnimationPlayer, draw} = require('../web/animations.js');
const data = require('../web/assets/animations/manifest.json');
const root = path.resolve(__dirname,'..');
const out = path.resolve(process.argv[2] || path.join(root,'previews'));
fs.mkdirSync(out,{recursive:true});

async function main() {
  const sheets = await Promise.all(data.characters.map(d=>canvas.loadImage(path.join(root,'web/assets/animations',d.image))));
  for (const [n,d] of data.characters.entries()) {
    const contact = canvas.createCanvas(1120, d.rows*260), ctx = contact.getContext('2d');
    ctx.fillStyle='#102c28';ctx.fillRect(0,0,contact.width,contact.height);
    const a = new AnimationPlayer(d);
    for (let i=0;i<d.columns*d.rows;i++) {
      const col=i%4,row=Math.floor(i/4), x=col*280+140,y=row*260+232;
      ctx.fillStyle='#64875b';ctx.fillRect(x-128,y,256,1);
      const clip=d.clips.find(c=>c.frames.includes(i));
      a.set(clip.name).seek(clip.frames.indexOf(i)/clip.fps);
      draw(ctx,sheets[n],a,x,y,d.facing,1,212);
      ctx.fillStyle='#d8e6bf';ctx.font='15px sans-serif';ctx.fillText(`${i} / ${clip.name}`,x-125,y+21);
    }
    fs.writeFileSync(path.join(out,d.id+'-contact.png'),contact.toBuffer('image/png'));
  }
  const stage=canvas.createCanvas(1280,720),ctx=stage.getContext('2d');
  const labels=['LOGAN · AGE 9','THORNLING','MOSSBACK GUARDIAN'];
  const sequence=[['idle','idle','idle'],['run','run','telegraph'],['rise','attack','charge'],
    ['apex','hurt','charge'],['fall','run','idle'],['land','attack','telegraph'],
    ['attack','hurt','hurt'],['dash','run','charge'],['hurt','hurt','hurt'],['victory','defeat','defeat']];
  const animations=data.characters.map(d=>new AnimationPlayer(d));
  const totalFrames=process.argv.includes('--video')?300:1;
  for (let frame=0;frame<totalFrames;frame++) {
    const time=frame/30,segment=Math.floor(time)%sequence.length,phase=time%1;
    ctx.fillStyle='#0b201d';ctx.fillRect(0,0,1280,720);
    ctx.fillStyle='#dcedc4';ctx.font='bold 46px sans-serif';ctx.fillText('I WANT TO BE THE HERO',56,78);
    ctx.fillStyle='#9db6a6';ctx.font='21px sans-serif';ctx.fillText('Character animation preview  /  Original prototype artwork',58,114);
    for(let n=0;n<3;n++) {
      const x=56+n*394;
      ctx.fillStyle='#15372e';ctx.fillRect(x,156,376,430);
      ctx.fillStyle='#dcedc4';ctx.font='bold 21px sans-serif';ctx.fillText(labels[n],x+22,198);
      ctx.fillStyle='#628656';ctx.fillRect(x+20,519,336,3);
      const name=sequence[segment][n],a=animations[n].set(name);
      // Repeat short one-shot actions so their timing is easy to see.
      a.seek(a.clip.loop?time:phase%(a.duration+.3));
      draw(ctx,sheets[n],a,x+188,519,n===0?1:-1,1,n===2?280:256);
      ctx.fillStyle='#e6c570';ctx.font='20px sans-serif';ctx.fillText(name.toUpperCase(),x+22,560);
    }
    ctx.fillStyle='#9db6a6';ctx.font='19px sans-serif';ctx.fillText('56 authored poses · Shared animation timing in the browser and Unity',58,639);
    ctx.fillStyle='#6e9279';ctx.font='17px sans-serif';ctx.fillText('Open PREVIEW_ANIMATIONS.bat to inspect each clip, slow playback, or flip direction.',58,676);
    fs.writeFileSync(path.join(out,totalFrames===1?'character-animations.png':`frame-${String(frame).padStart(4,'0')}.png`),stage.toBuffer('image/png'));
  }
  console.log(`Rendered ${totalFrames} preview frame(s) and all 56 poses into ${out}`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
