// A local Canvas render of the real game, with only its DOM/audio boundary mocked.
// This is a visual check, not a substitute for testing in a real browser/Unity.
const fs=require('node:fs'), path=require('node:path'), vm=require('node:vm');
let drawing;
try { drawing=require('@napi-rs/canvas'); }
catch (_) { drawing=require(path.join(process.env.CODEX_PRIMARY_RUNTIME_NODE_MODULES,'@napi-rs/canvas')); }
const root=path.resolve(__dirname,'..'), out=path.resolve(process.argv[2]||path.join(root,'previews'));
const canvas=drawing.createCanvas(1280,720), context=canvas.getContext('2d');
const elements=new Map(), hook={};
const element=()=>({classList:{add(){},remove(){}},addEventListener(){},getContext:()=>context,textContent:''});
const audioNode=()=>({frequency:{setValueAtTime(){},exponentialRampToValueAtTime(){}},gain:{setValueAtTime(){},exponentialRampToValueAtTime(){}},connect(){return this;},start(){},stop(){}});
class Audio {constructor(){this.state='running';this.currentTime=0;}createOscillator(){return audioNode();}createGain(){return audioNode();}}
const sandbox={console,performance:{now:()=>0},navigator:{getGamepads:()=>[]},requestAnimationFrame(){},
  Image:class {set src(value){}},
  document:{getElementById(id){if(!elements.has(id))elements.set(id,element());return elements.get(id);},querySelectorAll:()=>[]},
  HeroAnimationData:require('../web/assets/animations/manifest.json'),HeroAnimations:require('../web/animations.js'),
  window:{AudioContext:Audio,__HERO_TEST__:hook,addEventListener(){}}};
async function main(){
  vm.runInNewContext(fs.readFileSync(path.join(root,'web/game.js'),'utf8'),sandbox);
  const files={logan:'logan.png',thornling:'thornling.png',boss:'mossback-guardian.png',background:'sunleaf-ruins.png',
    loganSheet:'animations/logan-animations.png',thornlingSheet:'animations/thornling-animations.png',bossSheet:'animations/mossback-guardian-animations.png'};
  for (const [key,file] of Object.entries(files)) hook.images[key]=await drawing.loadImage(path.join(root,'web/assets',file));
  fs.mkdirSync(out,{recursive:true});hook.startGame();
  for(let i=0;i<60;i++)hook.update(1/60);
  hook.render();fs.writeFileSync(path.join(out,'game-start.png'),canvas.toBuffer('image/png'));
  const hero=hook.getPlayer();hero.x=4330;hero.y=558;hero.vy=0;hero.grounded=true;hero.invulnerable=5;hook.collectSpark();
  for(let i=0;i<75;i++)hook.update(1/60);
  hook.render();fs.writeFileSync(path.join(out,'game-boss.png'),canvas.toBuffer('image/png'));
  console.log('Rendered the starting area and active boss encounter.');
}
main().catch(error=>{console.error(error);process.exitCode=1;});
