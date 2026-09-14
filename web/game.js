(() => {
  "use strict";

  const canvas = document.getElementById("game");
  const ctx = canvas.getContext("2d", { alpha: false });
  ctx.imageSmoothingEnabled = false;

  const titleScreen = document.getElementById("title-screen");
  const resultScreen = document.getElementById("result-screen");
  const startButton = document.getElementById("start-button");
  const restartButton = document.getElementById("restart-button");
  const pauseButton = document.getElementById("pause-button");
  const touchControls = document.getElementById("touch-controls");
  const resultKicker = document.getElementById("result-kicker");
  const resultTitle = document.getElementById("result-title");
  const resultCopy = document.getElementById("result-copy");

  const WIDTH = 1280;
  const HEIGHT = 720;
  const WORLD_WIDTH = 5100;
  const STEP = 1 / 60;
  const GRAVITY = 2300;
  const ATTACK_DURATION = .32;
  const ATTACK_ACTIVE_START = .08;
  const ATTACK_ACTIVE_END = .20;
  const animationDefinitions = new Map(HeroAnimationData.characters.map(def => [def.id, def]));
  const animator = id => new HeroAnimations.AnimationPlayer(animationDefinitions.get(id));

  const images = {};
  const imageSources = {
    logan: "assets/logan.png",
    background: "assets/sunleaf-ruins.png",
    boss: "assets/mossback-guardian.png",
    thornling: "assets/thornling.png",
    loganSheet: "assets/animations/logan-animations.png",
    thornlingSheet: "assets/animations/thornling-animations.png",
    bossSheet: "assets/animations/mossback-guardian-animations.png"
  };

  let state = "title";
  let paused = false;
  let accumulator = 0;
  let lastTime = performance.now();
  let elapsed = 0;
  let cameraX = 0;
  let shake = 0;
  let flash = 0;
  let bossAwake = false;
  let audioContext = null;
  let victoryDelay = 0;
  let dashGhosts = [];
  let ghostTimer = 0;

  const input = {
    left: false,
    right: false,
    jump: false,
    attack: false,
    dash: false,
    padLeft: false,
    padRight: false,
    padJump: false,
    padAttack: false,
    padDash: false,
    jumpPressed: false,
    attackPressed: false,
    dashPressed: false
  };

  const platforms = [
    { x: 0, y: 620, w: 760, h: 120 },
    { x: 850, y: 620, w: 620, h: 120 },
    { x: 1550, y: 620, w: 690, h: 120 },
    { x: 2340, y: 620, w: 850, h: 120 },
    { x: 3280, y: 620, w: 700, h: 120 },
    { x: 4060, y: 620, w: 1040, h: 120 },
    { x: 470, y: 500, w: 170, h: 28 },
    { x: 930, y: 500, w: 190, h: 28 },
    { x: 1190, y: 410, w: 160, h: 28 },
    { x: 1650, y: 500, w: 170, h: 28 },
    { x: 1900, y: 430, w: 180, h: 28 },
    { x: 2470, y: 490, w: 230, h: 28 },
    { x: 2780, y: 400, w: 180, h: 28 },
    { x: 3020, y: 505, w: 160, h: 28 },
    { x: 3380, y: 490, w: 200, h: 28 },
    { x: 3660, y: 405, w: 170, h: 28 }
  ];

  const hazards = [
    { x: 700, y: 594, w: 60, h: 26 },
    { x: 1390, y: 594, w: 80, h: 26 },
    { x: 2170, y: 594, w: 70, h: 26 },
    { x: 3125, y: 594, w: 65, h: 26 },
    { x: 3910, y: 594, w: 70, h: 26 }
  ];

  const spark = { x: 3090, y: 448, r: 20, collected: false, phase: 0 };
  const checkpoint = { x: 2460, y: 540, lit: false };
  const motes = Array.from({ length: 70 }, (_, i) => ({
    x: (i * 283) % WORLD_WIDTH,
    y: 100 + ((i * 97) % 450),
    phase: i * 0.71,
    size: 1 + (i % 3)
  }));
  let particles = [];
  let enemies = [];
  let boss = null;
  let player = null;

  function loadImages() {
    return Promise.all(Object.entries(imageSources).map(([key, src]) => new Promise(resolve => {
      const image = new Image();
      image.onload = () => { images[key] = image; resolve(); };
      image.onerror = resolve;
      image.src = src;
    })));
  }

  function createPlayer() {
    return {
      x: 120, y: 540, w: 38, h: 62,
      vx: 0, vy: 0, dir: 1,
      grounded: false, coyote: 0, jumpBuffer: 0,
      health: 5, maxHealth: 5, invulnerable: 0,
      attackTimer: 0, attackCooldown: 0, attackHits: new Set(),
      dashTimer: 0, dashCooldown: 0, hasSpark: false,
      checkpointX: 120, checkpointY: 540,
      defeated: 0, hurtTimer: 0, landTimer: 0,
      animation: animator("logan")
    };
  }

  function makeEnemy(x, minX, maxX) {
    return { x, y: 568, w: 46, h: 52, vx: 55, minX, maxX, health: 2,
      alive: true, hurt: 0, contactTimer: 0, deathTime: 0,
      animation: animator("thornling"), id: Symbol("thornling") };
  }

  function resetGame() {
    player = createPlayer();
    enemies = [
      makeEnemy(570, 420, 700),
      makeEnemy(1050, 880, 1380),
      makeEnemy(1730, 1570, 2180),
      makeEnemy(2600, 2370, 3100),
      makeEnemy(3470, 3300, 3890)
    ];
    boss = {
      x: 4520, y: 462, w: 150, h: 158,
      health: 10, maxHealth: 10, alive: true, hurt: 0,
      state: "sleep", timer: 0, dir: -1, vx: 0, deathTime: 0,
      animation: animator("mossback-guardian"), id: Symbol("boss")
    };
    spark.collected = false;
    checkpoint.lit = false;
    bossAwake = false;
    cameraX = 0;
    elapsed = 0;
    particles = [];
    flash = 0;
    shake = 0;
    victoryDelay = 0;
    dashGhosts = [];
    ghostTimer = 0;
  }

  function startGame() {
    ensureAudio();
    resetGame();
    state = "playing";
    paused = false;
    pauseButton.textContent = "Ⅱ";
    clearTransientInput();
    titleScreen.classList.remove("visible");
    resultScreen.classList.remove("visible");
    touchControls.classList.add("playing");
    pauseButton.classList.add("playing");
    lastTime = performance.now();
  }

  function ensureAudio() {
    if (!audioContext) audioContext = new (window.AudioContext || window.webkitAudioContext)();
    if (audioContext.state === "suspended") audioContext.resume();
  }

  function sound(frequency, duration = 0.08, type = "square", volume = 0.035, endFrequency = frequency) {
    if (!audioContext) return;
    const now = audioContext.currentTime;
    const oscillator = audioContext.createOscillator();
    const gain = audioContext.createGain();
    oscillator.type = type;
    oscillator.frequency.setValueAtTime(frequency, now);
    oscillator.frequency.exponentialRampToValueAtTime(Math.max(40, endFrequency), now + duration);
    gain.gain.setValueAtTime(volume, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + duration);
    oscillator.connect(gain).connect(audioContext.destination);
    oscillator.start(now);
    oscillator.stop(now + duration);
  }

  function keyControl(code) {
    if (["ArrowLeft", "KeyA"].includes(code)) return "left";
    if (["ArrowRight", "KeyD"].includes(code)) return "right";
    if (["ArrowUp", "KeyW", "Space"].includes(code)) return "jump";
    if (["KeyJ", "KeyX"].includes(code)) return "attack";
    if (["ShiftLeft", "ShiftRight", "KeyK"].includes(code)) return "dash";
    return null;
  }

  window.addEventListener("keydown", event => {
    const control = keyControl(event.code);
    if (control) {
      event.preventDefault();
      if (!input[control]) input[`${control}Pressed`] = true;
      input[control] = true;
    }
    if ((event.code === "Enter" || event.code === "Space") && state !== "playing") startGame();
    if (event.code === "Escape" && state === "playing") togglePause();
    if (event.code === "KeyR" && state === "playing") startGame();
  });

  window.addEventListener("keyup", event => {
    const control = keyControl(event.code);
    if (control) input[control] = false;
  });

  document.querySelectorAll("[data-control]").forEach(button => {
    const control = button.dataset.control;
    const press = event => {
      event.preventDefault();
      ensureAudio();
      if (!input[control]) input[`${control}Pressed`] = true;
      input[control] = true;
      button.classList.add("pressed");
      button.setPointerCapture?.(event.pointerId);
    };
    const release = event => {
      event.preventDefault();
      input[control] = false;
      button.classList.remove("pressed");
    };
    button.addEventListener("pointerdown", press);
    button.addEventListener("pointerup", release);
    button.addEventListener("pointercancel", release);
    button.addEventListener("lostpointercapture", release);
  });

  function pollGamepad() {
    const pad = navigator.getGamepads?.()[0];
    if (!pad) {
      input.padLeft = input.padRight = input.padJump = input.padAttack = input.padDash = false;
      return;
    }
    const axis = pad.axes[0] || 0;
    input.padLeft = axis < -0.25 || !!pad.buttons[14]?.pressed;
    input.padRight = axis > 0.25 || !!pad.buttons[15]?.pressed;
    const mappings = [[0, "Jump", "jumpPressed"], [2, "Attack", "attackPressed"], [1, "Dash", "dashPressed"]];
    mappings.forEach(([buttonIndex, control, pressedFlag]) => {
      const pressed = !!pad.buttons[buttonIndex]?.pressed;
      const heldFlag = `pad${control}`;
      const previousFlag = `${heldFlag}Previous`;
      if (pressed && !input[previousFlag]) input[pressedFlag] = true;
      input[heldFlag] = pressed;
      input[previousFlag] = pressed;
    });
  }

  function clearTransientInput() {
    input.jumpPressed = false;
    input.attackPressed = false;
    input.dashPressed = false;
  }

  function togglePause() {
    paused = !paused;
    pauseButton.textContent = paused ? "▶" : "Ⅱ";
    sound(paused ? 380 : 540, .06);
  }

  function overlap(a, b) {
    return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
  }

  function moveAndCollide(body, dt) {
    const oldX = body.x;
    body.x += body.vx * dt;
    for (const platform of platforms) {
      if (!overlap(body, platform)) continue;
      if (oldX + body.w <= platform.x + 2) body.x = platform.x - body.w;
      else if (oldX >= platform.x + platform.w - 2) body.x = platform.x + platform.w;
      body.vx = 0;
    }

    const oldY = body.y;
    body.y += body.vy * dt;
    body.grounded = false;
    for (const platform of platforms) {
      if (!overlap(body, platform)) continue;
      if (body.vy >= 0 && oldY + body.h <= platform.y + 8) {
        body.y = platform.y - body.h;
        body.vy = 0;
        body.grounded = true;
      } else if (body.vy < 0 && oldY >= platform.y + platform.h - 8) {
        body.y = platform.y + platform.h;
        body.vy = 0;
      }
    }
  }

  function updatePlayer(dt) {
    pollGamepad();
    elapsed += dt;
    player.invulnerable = Math.max(0, player.invulnerable - dt);
    player.hurtTimer = Math.max(0, player.hurtTimer - dt);
    player.landTimer = Math.max(0, player.landTimer - dt);
    player.attackTimer = Math.max(0, player.attackTimer - dt);
    player.attackCooldown = Math.max(0, player.attackCooldown - dt);
    player.dashCooldown = Math.max(0, player.dashCooldown - dt);
    player.jumpBuffer = Math.max(0, player.jumpBuffer - dt);
    player.coyote = player.grounded ? 0.12 : Math.max(0, player.coyote - dt);

    if (input.jumpPressed) player.jumpBuffer = 0.14;

    if (input.attackPressed && player.attackCooldown <= 0 && player.hurtTimer <= 0 && player.dashTimer <= 0) {
      player.attackTimer = ATTACK_DURATION;
      player.attackCooldown = ATTACK_DURATION;
      player.attackHits.clear();
      sound(240, .08, "square", .035, 520);
    }

    if (input.dashPressed && player.hasSpark && player.dashCooldown <= 0 && player.hurtTimer <= 0) {
      player.dashTimer = 0.17;
      player.attackTimer = 0;
      ghostTimer = 0;
      player.dashCooldown = 0.65;
      player.vx = player.dir * 720;
      player.vy = 0;
      burst(player.x + player.w / 2, player.y + player.h / 2, "#ffd15c", 12);
      sound(210, .14, "sawtooth", .04, 720);
    }

    if (player.dashTimer > 0) {
      player.dashTimer -= dt;
      player.vx = player.dir * 720;
      player.vy = 0;
    } else {
      const direction = (input.right || input.padRight ? 1 : 0) - (input.left || input.padLeft ? 1 : 0);
      if (direction && player.attackTimer <= 0 && player.hurtTimer <= 0) player.dir = direction;
      const target = direction * 285;
      player.vx += Math.max(-1700 * dt, Math.min(1700 * dt, target - player.vx));
      player.vy += GRAVITY * dt;

      if (player.jumpBuffer > 0 && player.coyote > 0) {
        player.jumpBuffer = 0;
        player.coyote = 0;
        player.vy = -775;
        player.grounded = false;
        sound(310, .1, "square", .035, 620);
      }
      if (!input.jump && !input.padJump && player.vy < -260) player.vy += 2400 * dt;
    }

    const wasGrounded = player.grounded;
    moveAndCollide(player, dt);
    if (!wasGrounded && player.grounded) {
      player.landTimer = .10;
      burst(player.x + player.w / 2, player.y + player.h, "#c8d6b3", 5);
    }
    player.x = Math.max(0, Math.min(WORLD_WIDTH - player.w, player.x));

    const attackAge = ATTACK_DURATION - player.attackTimer;
    if (player.attackTimer > 0 && attackAge >= ATTACK_ACTIVE_START && attackAge < ATTACK_ACTIVE_END)
      resolvePlayerAttack();
    hazards.forEach(hazard => { if (overlap(player, hazard)) hurtPlayer(hazard.x + hazard.w / 2); });

    if (player.y > HEIGHT + 140) respawnPlayer();

    if (!checkpoint.lit && player.x > checkpoint.x - 35) {
      checkpoint.lit = true;
      player.checkpointX = checkpoint.x - 50;
      player.checkpointY = 530;
      sound(440, .12, "sine", .04, 880);
    }

    const dx = player.x + player.w / 2 - spark.x;
    const dy = player.y + player.h / 2 - spark.y;
    if (!spark.collected && dx * dx + dy * dy < 65 * 65) collectSpark();
  }

  function resolvePlayerAttack() {
    const hitbox = {
      x: player.dir > 0 ? player.x + player.w - 4 : player.x - 66,
      y: player.y + 8,
      w: 70,
      h: 48
    };
    for (const enemy of enemies) {
      if (!enemy.alive || player.attackHits.has(enemy.id) || !overlap(hitbox, enemy)) continue;
      player.attackHits.add(enemy.id);
      enemy.health -= player.hasSpark ? 2 : 1;
      enemy.hurt = .18;
      enemy.x += player.dir * 24;
      burst(enemy.x + enemy.w / 2, enemy.y + enemy.h / 2, "#e8bd55", 7);
      sound(155, .07, "square", .035, 95);
      if (enemy.health <= 0) {
        enemy.alive = false;
        player.defeated++;
        sound(180, .13, "triangle", .04, 70);
      }
    }
    if (bossAwake && boss.alive && !player.attackHits.has(boss.id) && overlap(hitbox, boss)) {
      player.attackHits.add(boss.id);
      boss.health--;
      boss.hurt = .2;
      shake = .15;
      burst(hitbox.x + hitbox.w / 2, hitbox.y + 20, "#ffd15c", 9);
      sound(115, .1, "square", .05, 80);
      if (boss.health <= 0) defeatBoss();
    }
  }

  function updateEnemies(dt) {
    enemies.forEach(enemy => {
      if (!enemy.alive) { enemy.deathTime += dt; return; }
      enemy.hurt = Math.max(0, enemy.hurt - dt);
      enemy.contactTimer = Math.max(0, enemy.contactTimer - dt);
      enemy.x += enemy.vx * dt;
      if (enemy.x < enemy.minX || enemy.x + enemy.w > enemy.maxX) {
        enemy.vx *= -1;
        enemy.x = Math.max(enemy.minX, Math.min(enemy.maxX - enemy.w, enemy.x));
      }
      if (overlap(player, enemy)) {
        if (player.invulnerable <= 0) enemy.contactTimer = 1 / 3;
        hurtPlayer(enemy.x + enemy.w / 2);
      }
    });
  }

  function updateBoss(dt) {
    if (!boss.alive) { boss.deathTime += dt; return; }
    boss.hurt = Math.max(0, boss.hurt - dt);
    if (!bossAwake && player.x > 4160) {
      bossAwake = true;
      boss.state = "telegraph";
      boss.timer = 1.1;
      sound(85, .5, "sawtooth", .055, 55);
    }
    if (!bossAwake) return;

    if (boss.state !== "charge") boss.dir = player.x < boss.x ? -1 : 1;
    boss.timer -= dt;
    if (boss.state === "telegraph") {
      if (boss.timer <= 0) {
        boss.state = "charge";
        boss.timer = .7;
        boss.vx = boss.dir * 430;
        shake = .25;
      }
    } else if (boss.state === "charge") {
      boss.x += boss.vx * dt;
      if (boss.x < 4110 || boss.x + boss.w > 5010 || boss.timer <= 0) {
        boss.x = Math.max(4110, Math.min(5010 - boss.w, boss.x));
        boss.state = "rest";
        boss.timer = .8;
        boss.vx = 0;
        shake = .2;
        burst(boss.x + boss.w / 2, 610, "#c5e36b", 14);
      }
    } else if (boss.state === "rest" && boss.timer <= 0) {
      boss.state = "telegraph";
      boss.timer = boss.health <= 5 ? .65 : .95;
    }
    if (overlap(player, boss)) hurtPlayer(boss.x + boss.w / 2);
  }

  function collectSpark() {
    spark.collected = true;
    player.hasSpark = true;
    player.health = player.maxHealth;
    flash = .55;
    burst(spark.x, spark.y, "#ffd15c", 32);
    sound(440, .45, "triangle", .055, 1100);
  }

  function hurtPlayer(sourceX) {
    if (player.invulnerable > 0 || player.dashTimer > 0 || state !== "playing") return;
    player.health--;
    player.hurtTimer = .2;
    player.attackTimer = 0;
    player.invulnerable = 1.05;
    player.vx = player.x < sourceX ? -360 : 360;
    player.vy = -420;
    shake = .25;
    flash = .12;
    burst(player.x + player.w / 2, player.y + player.h / 2, "#e85d3f", 10);
    sound(170, .18, "sawtooth", .05, 70);
    if (player.health <= 0) respawnPlayer();
  }

  function respawnPlayer() {
    player.x = player.checkpointX;
    player.y = player.checkpointY;
    player.vx = 0;
    player.vy = 0;
    player.health = player.maxHealth;
    player.invulnerable = 1.2;
    player.hurtTimer = player.landTimer = player.attackTimer = player.dashTimer = 0;
    player.animation.set("idle", true);
    player.grounded = false;
    dashGhosts = [];
    flash = .35;
    cameraX = Math.max(0, player.x - 300);
    sound(115, .25, "triangle", .04, 260);
  }

  function defeatBoss() {
    boss.alive = false;
    boss.deathTime = 0;
    bossAwake = false;
    state = "won";
    flash = 1;
    shake = .4;
    burst(boss.x + boss.w / 2, boss.y + boss.h / 2, "#ffd15c", 55);
    sound(330, .5, "triangle", .06, 1320);
    victoryDelay = 1.8;
    player.attackTimer = player.dashTimer = player.hurtTimer = 0;
    player.animation.set("victory", true);
  }

  function showResult(won) {
    state = won ? "won" : "lost";
    resultKicker.textContent = won ? "Quest complete" : "The ruins endure";
    resultTitle.textContent = won ? "Hero of Sunleaf!" : "Try again, Logan!";
    const minutes = Math.floor(elapsed / 60);
    const seconds = Math.floor(elapsed % 60).toString().padStart(2, "0");
    resultCopy.textContent = won
      ? `Logan found the Hero Spark and befriended the Mossback Guardian in ${minutes}:${seconds}. Being a hero was never about being the strongest — it was about trying again.`
      : "Every hero gets another chance. Return to the last checkpoint and keep going.";
    resultScreen.classList.add("visible");
    touchControls.classList.remove("playing");
    pauseButton.classList.remove("playing");
  }

  function burst(x, y, color, count) {
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = 60 + Math.random() * 230;
      particles.push({ x, y, vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed, life: .35 + Math.random() * .55, max: .9, color, size: 2 + Math.random() * 5 });
    }
  }

  function updateParticles(dt) {
    for (const particle of particles) {
      particle.life -= dt;
      particle.x += particle.vx * dt;
      particle.y += particle.vy * dt;
      particle.vy += 700 * dt;
    }
    particles = particles.filter(particle => particle.life > 0);
  }

  function update(dt) {
    if (paused) return;
    if (state !== "playing") {
      updateAnimations(dt);
      if (state === "won") {
        boss.deathTime += dt;
        updateParticles(dt);
        shake = Math.max(0, shake - dt);
        flash = Math.max(0, flash - dt);
        if (victoryDelay > 0) {
          victoryDelay -= dt;
          if (victoryDelay <= 0) showResult(true);
        }
      }
      clearTransientInput();
      return;
    }
    spark.phase += dt * 3;
    updatePlayer(dt);
    updateEnemies(dt);
    updateBoss(dt);
    updateAnimations(dt);
    updateParticles(dt);
    cameraX += ((player.x - WIDTH * .38) - cameraX) * Math.min(1, dt * 6);
    cameraX = Math.max(0, Math.min(WORLD_WIDTH - WIDTH, cameraX));
    shake = Math.max(0, shake - dt);
    flash = Math.max(0, flash - dt);
    clearTransientInput();
  }

  function updateAnimations(dt) {
    const a = player.animation;
    if (state === "won") a.set("victory").update(dt);
    else if (state === "title") a.set("idle").update(dt);
    else if (player.hurtTimer > 0) a.set("hurt").seek(.2 - player.hurtTimer);
    else if (player.dashTimer > 0) a.set("dash").seek(.17 - player.dashTimer);
    else if (player.attackTimer > 0) a.set("attack").seek(ATTACK_DURATION - player.attackTimer);
    else if (!player.grounded) a.set(Math.abs(player.vy) < 90 ? "apex" : player.vy < 0 ? "rise" : "fall").update(dt);
    else if (player.landTimer > 0) a.set("land").update(dt);
    else a.set(Math.abs(player.vx) > 25 ? "run" : "idle").update(dt,
      Math.abs(player.vx) > 25 ? Math.max(.5, Math.abs(player.vx) / 285) : 1);

    for (const enemy of enemies) {
      if (!enemy.alive) enemy.animation.set("defeat").seek(enemy.deathTime);
      else if (enemy.hurt > 0) enemy.animation.set("hurt").seek(.18 - enemy.hurt);
      else if (enemy.contactTimer > 0) enemy.animation.set("attack").seek(1 / 3 - enemy.contactTimer);
      else enemy.animation.set(state === "title" ? "idle" : "run").update(dt);
    }
    const bossState = !boss.alive ? "defeat" : boss.hurt > 0 ? "hurt" :
      boss.state === "charge" ? "charge" : boss.state === "telegraph" ? "telegraph" : "idle";
    boss.animation.set(bossState).update(dt, bossState === "charge" && boss.health <= 5 ? 1.25 : 1);
    if (!boss.alive) boss.animation.seek(boss.deathTime);
    if (boss.hurt > 0 && boss.alive) boss.animation.seek(.2 - boss.hurt);

    dashGhosts.forEach(ghost => { ghost.life -= dt; });
    dashGhosts = dashGhosts.filter(ghost => ghost.life > 0);
    ghostTimer -= dt;
    if (player.dashTimer > 0 && ghostTimer <= 0) {
      const pose = animator("logan").set("dash").seek(a.time);
      dashGhosts.push({ x: player.x + player.w / 2, y: player.y + player.h,
        direction: player.dir, life: .16, animation: pose });
      ghostTimer = .04;
    }
  }

  function drawBackground() {
    const bg = images.background;
    if (bg) {
      const sourceWidth = bg.width * .78;
      const sx = (cameraX / (WORLD_WIDTH - WIDTH || 1)) * (bg.width - sourceWidth);
      ctx.drawImage(bg, sx, 0, sourceWidth, bg.height, 0, 0, WIDTH, HEIGHT);
    } else {
      ctx.fillStyle = "#75b7c2";
      ctx.fillRect(0, 0, WIDTH, HEIGHT);
    }
    const shade = ctx.createLinearGradient(0, 0, 0, HEIGHT);
    shade.addColorStop(0, "rgba(15,49,49,.08)");
    shade.addColorStop(1, "rgba(4,22,20,.5)");
    ctx.fillStyle = shade;
    ctx.fillRect(0, 0, WIDTH, HEIGHT);
  }

  function drawPlatforms() {
    for (const p of platforms) {
      const x = Math.round(p.x - cameraX);
      if (x > WIDTH || x + p.w < 0) continue;
      ctx.fillStyle = "#263c32";
      ctx.fillRect(x, p.y, p.w, p.h);
      ctx.fillStyle = "#657d45";
      ctx.fillRect(x, p.y, p.w, Math.min(10, p.h));
      ctx.fillStyle = "#9ebc55";
      for (let gx = x + 5; gx < x + p.w; gx += 18) ctx.fillRect(gx, p.y - 3 - ((gx / 18) % 2) * 2, 10, 5);
      if (p.h > 30) {
        ctx.fillStyle = "rgba(5,19,17,.28)";
        for (let bx = x + 24; bx < x + p.w; bx += 58) ctx.fillRect(bx, p.y + 30, 4, 4);
      }
    }
  }

  function drawHazards() {
    hazards.forEach(h => {
      const x = h.x - cameraX;
      ctx.fillStyle = "#372f2c";
      const count = Math.max(1, Math.floor(h.w / 18));
      for (let i = 0; i < count; i++) {
        const bx = x + i * (h.w / count);
        ctx.beginPath();
        ctx.moveTo(bx, h.y + h.h);
        ctx.lineTo(bx + h.w / count / 2, h.y);
        ctx.lineTo(bx + h.w / count, h.y + h.h);
        ctx.fill();
      }
    });
  }

  function drawCheckpoint() {
    const x = checkpoint.x - cameraX;
    ctx.fillStyle = "#4b3327";
    ctx.fillRect(x, checkpoint.y - 45, 7, 80);
    ctx.fillStyle = checkpoint.lit ? "#ffd15c" : "#c8d6b3";
    ctx.beginPath();
    ctx.moveTo(x + 7, checkpoint.y - 44);
    ctx.lineTo(x + 48, checkpoint.y - 28);
    ctx.lineTo(x + 7, checkpoint.y - 13);
    ctx.fill();
    if (checkpoint.lit) drawGlow(x + 21, checkpoint.y - 27, 35, "255,209,92");
  }

  function drawSpark() {
    if (spark.collected) return;
    const x = spark.x - cameraX;
    const y = spark.y + Math.sin(spark.phase) * 8;
    drawGlow(x, y, 52, "255,209,92");
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(spark.phase * .18);
    ctx.fillStyle = "#fff4a8";
    ctx.beginPath();
    for (let i = 0; i < 10; i++) {
      const radius = i % 2 ? 9 : 22;
      const angle = -Math.PI / 2 + i * Math.PI / 5;
      const px = Math.cos(angle) * radius;
      const py = Math.sin(angle) * radius;
      if (!i) ctx.moveTo(px, py); else ctx.lineTo(px, py);
    }
    ctx.closePath();
    ctx.fill();
    ctx.restore();
  }

  function drawGlow(x, y, radius, rgb) {
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
    gradient.addColorStop(0, `rgba(${rgb},.65)`);
    gradient.addColorStop(1, `rgba(${rgb},0)`);
    ctx.fillStyle = gradient;
    ctx.fillRect(x - radius, y - radius, radius * 2, radius * 2);
  }

  function drawImageFacing(image, x, y, w, h, direction, alpha = 1) {
    if (!image) return;
    ctx.save();
    ctx.globalAlpha = alpha;
    if (direction < 0) {
      ctx.translate(x + w, y);
      ctx.scale(-1, 1);
      ctx.drawImage(image, 0, 0, w, h);
    } else {
      ctx.drawImage(image, x, y, w, h);
    }
    ctx.restore();
  }

  function drawEntities() {
    for (const ghost of dashGhosts)
      HeroAnimations.draw(ctx, images.loganSheet, ghost.animation, ghost.x - cameraX, ghost.y,
        ghost.direction, ghost.life / .16 * .26);

    for (const enemy of enemies) {
      if (!enemy.alive && enemy.deathTime >= .75) continue;
      const alpha = !enemy.alive ? Math.min(1, (.75 - enemy.deathTime) / .25) : 1;
      if (!HeroAnimations.draw(ctx, images.thornlingSheet, enemy.animation,
        enemy.x - cameraX + enemy.w / 2, enemy.y + enemy.h, Math.sign(enemy.vx), alpha))
        drawImageFacing(images.thornling, enemy.x - cameraX - 20, enemy.y - 18, 86, 82, enemy.vx < 0 ? 1 : -1, alpha);
    }

    if (boss.alive || boss.deathTime < 1.8) {
      const telegraph = boss.state === "telegraph" && Math.floor(boss.timer * 12) % 2;
      if (telegraph) drawGlow(boss.x - cameraX + boss.w / 2, boss.y + boss.h / 2, 120, "255,163,62");
      const alpha = boss.alive ? 1 : Math.min(1, (1.8 - boss.deathTime) / .6);
      if (!HeroAnimations.draw(ctx, images.bossSheet, boss.animation,
        boss.x - cameraX + boss.w / 2, boss.y + boss.h, boss.dir, alpha))
        drawImageFacing(images.boss, boss.x - cameraX - 32, boss.y - 26, 215, 205, -boss.dir, alpha);
    }

    const alpha = player.invulnerable > 0 && Math.floor(player.invulnerable * 18) % 2 ? .4 : 1;
    if (player.hasSpark) drawGlow(player.x - cameraX + player.w / 2, player.y + player.h / 2, 54, "255,209,92");
    if (!HeroAnimations.draw(ctx, images.loganSheet, player.animation,
      player.x - cameraX + player.w / 2, player.y + player.h, player.dir, alpha))
      drawImageFacing(images.logan, player.x - cameraX - 20, player.y - 17, 82, 92, player.dir, alpha);

    const attackAge = ATTACK_DURATION - player.attackTimer;
    if (player.attackTimer > 0 && attackAge >= ATTACK_ACTIVE_START && attackAge < ATTACK_ACTIVE_END) {
      const cx = player.x - cameraX + player.w / 2 + player.dir * 38;
      const cy = player.y + 31;
      ctx.strokeStyle = player.hasSpark ? "#fff4a8" : "#ead49c";
      ctx.lineWidth = 4;
      ctx.beginPath();
      ctx.arc(cx, cy, 32, player.dir > 0 ? -1.1 : Math.PI - 1.1, player.dir > 0 ? 1.1 : Math.PI + 1.1);
      ctx.stroke();
    }
  }

  function drawParticles() {
    particles.forEach(particle => {
      ctx.globalAlpha = Math.max(0, particle.life / particle.max);
      ctx.fillStyle = particle.color;
      ctx.fillRect(Math.round(particle.x - cameraX), Math.round(particle.y), particle.size, particle.size);
    });
    ctx.globalAlpha = 1;
  }

  function drawMotes() {
    motes.forEach(mote => {
      const x = mote.x - cameraX * .7;
      if (x < 0 || x > WIDTH) return;
      const y = mote.y + Math.sin(elapsed * 1.7 + mote.phase) * 7;
      ctx.fillStyle = `rgba(255,222,101,${.25 + .2 * Math.sin(elapsed + mote.phase)})`;
      ctx.fillRect(Math.round(x), Math.round(y), mote.size, mote.size);
    });
  }

  function objectiveText() {
    if (!player.hasSpark) return "Find the Hero Spark";
    if (!bossAwake) return "Use your dash · Reach the ancient gate";
    return "Defeat the Mossback Guardian";
  }

  function drawHud() {
    ctx.fillStyle = "rgba(5,27,25,.78)";
    ctx.fillRect(24, 22, 590, 78);
    ctx.strokeStyle = "rgba(255,230,155,.7)";
    ctx.lineWidth = 3;
    ctx.strokeRect(24, 22, 590, 78);
    ctx.fillStyle = "#fff0c2";
    ctx.font = "900 22px Trebuchet MS";
    ctx.fillText("LOGAN", 43, 51);
    ctx.fillStyle = "#e85d3f";
    for (let i = 0; i < player.maxHealth; i++) {
      ctx.fillStyle = i < player.health ? "#e85d3f" : "#493c39";
      ctx.fillRect(43 + i * 30, 64, 22, 18);
      ctx.fillRect(47 + i * 30, 60, 6, 6);
      ctx.fillRect(57 + i * 30, 60, 6, 6);
    }
    ctx.fillStyle = "#ffd15c";
    ctx.font = "800 17px Trebuchet MS";
    ctx.fillText(objectiveText(), 205, 72, 385);
    ctx.fillStyle = player.hasSpark ? "#fff0a5" : "#78928a";
    ctx.font = "700 13px Trebuchet MS";
    ctx.fillText(player.hasSpark ? "HERO DASH READY" : "POWER NOT YET FOUND", 205, 91);

    if (bossAwake && boss.alive) {
      const w = 500;
      const x = WIDTH - w - 120;
      ctx.fillStyle = "rgba(5,27,25,.86)";
      ctx.fillRect(x, 28, w, 48);
      ctx.fillStyle = "#fff0c2";
      ctx.font = "900 15px Trebuchet MS";
      ctx.textAlign = "center";
      ctx.fillText("MOSSBACK GUARDIAN", x + w / 2, 48);
      ctx.fillStyle = "#3e4940";
      ctx.fillRect(x + 18, 57, w - 36, 10);
      ctx.fillStyle = "#c9df67";
      ctx.fillRect(x + 18, 57, (w - 36) * (boss.health / boss.maxHealth), 10);
      ctx.textAlign = "left";
    }

    if (paused) {
      ctx.fillStyle = "rgba(4,20,18,.72)";
      ctx.fillRect(0, 0, WIDTH, HEIGHT);
      ctx.textAlign = "center";
      ctx.fillStyle = "#ffd15c";
      ctx.font = "900 58px Impact";
      ctx.fillText("QUEST PAUSED", WIDTH / 2, HEIGHT / 2);
      ctx.fillStyle = "#fff0c2";
      ctx.font = "700 20px Trebuchet MS";
      ctx.fillText("Press Escape or tap play to continue", WIDTH / 2, HEIGHT / 2 + 42);
      ctx.textAlign = "left";
    }
  }

  function render() {
    ctx.save();
    if (shake > 0) ctx.translate((Math.random() - .5) * 14, (Math.random() - .5) * 10);
    drawBackground();
    drawMotes();
    drawPlatforms();
    drawHazards();
    drawCheckpoint();
    drawSpark();
    drawEntities();
    drawParticles();
    ctx.restore();

    if (state === "playing") drawHud();
    if (flash > 0) {
      ctx.fillStyle = `rgba(255,241,178,${Math.min(.65, flash)})`;
      ctx.fillRect(0, 0, WIDTH, HEIGHT);
    }
  }

  function frame(now) {
    const delta = Math.min(.05, (now - lastTime) / 1000);
    lastTime = now;
    accumulator += delta;
    while (accumulator >= STEP) {
      update(STEP);
      accumulator -= STEP;
    }
    render();
    requestAnimationFrame(frame);
  }

  startButton.addEventListener("click", startGame);
  restartButton.addEventListener("click", startGame);
  pauseButton.addEventListener("click", togglePause);
  window.addEventListener("blur", () => {
    ["left", "right", "jump", "attack", "dash"].forEach(key => { input[key] = false; });
    clearTransientInput();
    if (state === "playing") { paused = true; pauseButton.textContent = "▶"; }
  });

  // A narrow opt-in hook for repeatable animation/gameplay checks; absent in normal play.
  if (window.__HERO_TEST__) Object.assign(window.__HERO_TEST__, {
    update, render, startGame, hurtPlayer, respawnPlayer, collectSpark, defeatBoss, togglePause,
    input, images, getPlayer: () => player, getEnemies: () => enemies,
    getBoss: () => boss, getState: () => state, getGhosts: () => dashGhosts
  });

  resetGame();
  loadImages().finally(() => requestAnimationFrame(frame));
})();
