const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");
const { AnimationPlayer, sourceRect, draw } = require("../web/animations.js");
const data = require("../web/assets/animations/manifest.json");
const def = id => data.characters.find(character => character.id === id);

test("every clip references a valid frame, and the browser and Unity use the same data", () => {
  assert.deepEqual(data, JSON.parse(fs.readFileSync(path.join(__dirname, "../unity/Assets/Resources/Art/Animations/manifest.json"))));
  const context = {}; vm.runInNewContext(fs.readFileSync(path.join(__dirname, "../web/animation-data.js"), "utf8"), context);
  assert.deepEqual(JSON.parse(JSON.stringify(context.HeroAnimationData)), data);
  for (const d of data.characters) for (const c of d.clips) {
    assert.ok(c.fps > 0 && c.frames.length > 0);
    assert.ok(c.frames.every(i => Number.isInteger(i) && i >= 0 && i < d.columns * d.rows));
  }
});

test("run timing is the same at 30, 60, and 120 updates per second", () => {
  const frames = [30, 60, 120].map(rate => {
    const a = new AnimationPlayer(def("logan")).set("run");
    for (let i = 0; i < rate; i++) a.update(1 / rate);
    return a.frame;
  });
  assert.equal(new Set(frames).size, 1);
});

test("every atlas slice stays within its image and preserves the pose's pixel scale", () => {
  for (const d of data.characters) {
    const png = fs.readFileSync(path.join(__dirname, '../web/assets/animations', d.image));
    const image = {width:png.readUInt32BE(16), height:png.readUInt32BE(20)};
    assert.equal(d.regions.length, d.columns*d.rows);
    assert.equal(d.pivots.length, d.regions.length);
    const ctx = {globalAlpha:1,save(){},restore(){},translate(){},scale(){},drawImage(...args){this.args=args;}};
    for (let i=0;i<d.regions.length;i++) {
      const r=sourceRect(image,d,i);
      assert.ok(r.x>=0 && r.y>=0 && r.w>0 && r.h>0 && r.x+r.w<=image.width && r.y+r.h<=image.height);
      assert.ok(d.pivots[i].x>=0 && d.pivots[i].x<=1 && d.pivots[i].y>=0 && d.pivots[i].y<=1);
      const a=new AnimationPlayer(d),clip=d.clips.find(c=>c.frames.includes(i));
      a.set(clip.name).seek(clip.frames.indexOf(i)/clip.fps);
      draw(ctx,image,a,0,0);
      assert.equal(ctx.args[8],Math.round(r.h*d.displayHeight/(image.height/d.rows)));
    }
  }
});

test("one-shot attacks start in wind-up and hold the recovery frame", () => {
  const a = new AnimationPlayer(def("logan")).set("attack");
  assert.equal(a.frame, 12);
  a.update(.08); assert.equal(a.frame, 13);
  a.update(5); assert.equal(a.frame, 15); assert.ok(a.finished);
  a.set("run"); assert.equal(a.frame, 4);
  a.set("attack"); assert.equal(a.frame, 12); assert.equal(a.time, 0);
});

test("rendering and mirroring cannot advance animation time", () => {
  const ctx = {globalAlpha:1, save(){}, restore(){}, translate(){}, scale(x){this.flip=x;}, drawImage(){}};
  const a = new AnimationPlayer(def("logan")).set("run").update(.2);
  const frame = a.frame;
  draw(ctx, {width:1024,height:1536}, a, 0, 0, -1);
  assert.equal(ctx.flip, -1);
  assert.equal(a.frame, frame); assert.equal(a.time, .2);
  const enemy = new AnimationPlayer(def("thornling"));
  draw(ctx, {width:1254,height:1254}, enemy, 0, 0, -1);
  assert.equal(ctx.flip, 1); // Enemy artwork natively faces left.
});

// Exercise the actual game update loop with a small DOM/canvas boundary mock.
// No duplicate implementation of combat, transitions, or physics lives here.
function game() {
  const gradient = {addColorStop(){}};
  const context = new Proxy({globalAlpha:1, createLinearGradient:()=>gradient, createRadialGradient:()=>gradient}, {
    get(target,key){return key in target ? target[key] : ()=>{};}
  });
  const elements = new Map();
  const element = () => ({classList:{add(){},remove(){}}, addEventListener(){}, getContext:()=>context, textContent:""});
  const node = () => ({frequency:{setValueAtTime(){},exponentialRampToValueAtTime(){}},
    gain:{setValueAtTime(){},exponentialRampToValueAtTime(){}},connect(){return this;},start(){},stop(){}});
  class Audio {constructor(){this.state="running";this.currentTime=0;} createOscillator(){return node();} createGain(){return node();}}
  const hook = {};
  const sandbox = {console, performance:{now:()=>0}, navigator:{getGamepads:()=>[]},
    requestAnimationFrame(){}, setTimeout(){throw new Error("Gameplay must use simulation time, not wall-clock timers");},
    Image:class {set src(value){queueMicrotask(()=>this.onerror && this.onerror());}},
    document:{getElementById(id){if(!elements.has(id))elements.set(id,element());return elements.get(id);},querySelectorAll:()=>[]},
    HeroAnimationData:data, HeroAnimations:{AnimationPlayer,sourceRect,draw},
    window:{AudioContext:Audio,__HERO_TEST__:hook,addEventListener(){}}};
  vm.runInNewContext(fs.readFileSync(path.join(__dirname, "../web/game.js"),"utf8"),sandbox);
  hook.startGame();
  const p = hook.getPlayer(); p.y=558; p.vy=0; p.grounded=true;
  hook.step = (frames=1) => {for(let i=0;i<frames;i++)hook.update(1/60);};
  return hook;
}

test("sword hits start on the swing frame and hit each enemy once per attack", () => {
  const g = game(), e = g.getEnemies()[0];
  e.x=180; e.vx=0; e.minX=0; e.maxX=1000;
  g.input.attackPressed=true; g.step();
  assert.equal(e.health,2); assert.equal(g.getPlayer().animation.frame,12);
  g.step(5);
  assert.equal(g.getPlayer().animation.frame,13); assert.equal(e.health,1);
  g.step(8); assert.equal(e.health,1);
});

test("jump transitions through rise, apex, fall, and landing", () => {
  const g = game(); g.input.jump=g.input.jumpPressed=true;
  const seen = new Set();
  for(let i=0;i<60;i++){g.step();seen.add(g.getPlayer().animation.name);}
  for(const clip of ["rise","apex","fall","land","idle"]) assert.ok(seen.has(clip),clip);
});

test("dash requires the spark, creates afterimages, and recovers", () => {
  const g=game(); g.input.dashPressed=true; g.step();
  assert.equal(g.getPlayer().dashTimer,0);
  g.collectSpark(); g.input.dashPressed=true; g.step();
  assert.equal(g.getPlayer().animation.name,"dash"); assert.ok(g.getGhosts().length>0);
  g.step(30); assert.notEqual(g.getPlayer().animation.name,"dash"); assert.equal(g.getGhosts().length,0);
});

test("pause freezes poses; hurt interrupts attacks; respawn clears transient poses", () => {
  const g=game(); g.input.attackPressed=true; g.step();
  const before=g.getPlayer().animation.time;
  g.togglePause(); g.step(30); assert.equal(g.getPlayer().animation.time,before);
  g.togglePause(); g.hurtPlayer(200); g.step();
  assert.equal(g.getPlayer().animation.name,"hurt"); assert.equal(g.getPlayer().attackTimer,0);
  g.respawnPlayer(); assert.equal(g.getPlayer().animation.name,"idle");
  assert.equal(g.getPlayer().hurtTimer,0); assert.equal(g.getPlayer().dashTimer,0);
});

test("enemy defeat remains visible and victory keeps animating before the result", () => {
  const g=game(), e=g.getEnemies()[0];
  e.x=180;e.vx=0;e.minX=0;e.maxX=1000;e.health=1;
  g.input.attackPressed=true;g.step(7);
  assert.equal(e.alive,false);assert.equal(e.animation.name,"defeat");assert.ok(e.deathTime<.75);
  g.defeatBoss();g.step();assert.equal(g.getState(),"won");
  assert.equal(g.getBoss().animation.name,"defeat");
  const frame=g.getPlayer().animation.frame;g.step(16);
  assert.equal(g.getPlayer().animation.name,"victory");assert.notEqual(g.getPlayer().animation.frame,frame);
  g.startGame();g.step(120);assert.equal(g.getState(),"playing");
});
