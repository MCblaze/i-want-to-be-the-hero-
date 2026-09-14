(() => {
  "use strict";
  const names = {idle:"Idle", run:"Run / walk", rise:"Jump — rise", apex:"Jump — apex", fall:"Jump — fall",
    land:"Landing", attack:"Attack", dash:"Hero dash", hurt:"Hit reaction", victory:"Victory", telegraph:"Charge warning", charge:"Charge", defeat:"Defeat"};
  const notes = {attack:"Wind-up → impact → follow-through → recovery", telegraph:"A clear warning before the charge.",
    defeat:"A gentle defeat, with time to read the pose.", victory:"Logan celebrates his first quest.",
    rise:"Selected while moving upward.", apex:"Selected at the top of the jump.", fall:"Selected while falling.",
    land:"Brief crouch when Logan touches down.", dash:"Launch → burst → braking."};
  let paused = false, direction = 1, speed = 1, cards = [], last = performance.now();
  const loaded = new Map();
  let request = 0;

  async function select(id) {
    const token = ++request;
    const def = HeroAnimationData.characters.find(character => character.id === id);
    document.querySelectorAll("[data-character]").forEach(button => button.setAttribute("aria-pressed", String(button.dataset.character === id)));
    if (!loaded.has(id)) {
      try {
        const img = await new Promise((resolve, reject) => {
          const image = new Image(); image.onload = () => resolve(image); image.onerror = reject;
          image.src = "assets/animations/" + def.image;
        });
        loaded.set(id, img);
      } catch {
        if (token !== request) return;
        document.getElementById("error").hidden = false;
        document.getElementById("error").textContent = "The animation image could not load. Extract the complete ZIP and reopen this page.";
        return;
      }
    }
    if (token !== request) return;
    document.getElementById("error").hidden = true;
    document.getElementById("summary").textContent = `${def.columns * def.rows} frames · ${def.clips.length} clips`;
    const section = document.getElementById("clips");
    section.replaceChildren();
    cards = def.clips.map(clip => {
      const article = document.createElement("article");
      const header = document.createElement("header");
      const title = document.createElement("h2"); title.textContent = names[clip.name];
      const badge = document.createElement("span"); badge.className = "badge"; badge.textContent = clip.loop ? "Loop" : "Action";
      header.append(title, badge);
      const canvas = document.createElement("canvas"); canvas.width = 480; canvas.height = 320;
      canvas.setAttribute("aria-label", `${id} ${names[clip.name]} animation`);
      const note = document.createElement("p"); note.textContent = notes[clip.name] || `${clip.frames.length} frames · ${clip.fps} fps`;
      article.append(header, canvas, note); section.append(article);
      return {ctx:canvas.getContext("2d"), image:loaded.get(id), animation:new HeroAnimations.AnimationPlayer(def).set(clip.name)};
    });
  }

  function render(now) {
    const dt = Math.min(.05, (now - last) / 1000) * speed; last = now;
    cards.forEach(card => {
      const {ctx, image, animation:a} = card;
      if (!paused) { a.update(dt); if (!a.clip.loop && a.time >= a.duration) {
        card.rest = (card.rest || 0) + dt;
        if (card.rest > .55) { a.seek(0); card.rest = 0; }
      }}
      ctx.fillStyle = "#102c28"; ctx.fillRect(0, 0, 480, 320);
      ctx.fillStyle = "#18392f";
      for (let x = 0; x < 480; x += 32) for (let y = 0; y < 320; y += 32) ctx.fillRect(x, y, 1, 1);
      ctx.fillStyle = "#4f7050"; ctx.fillRect(40, 282, 400, 2);
      HeroAnimations.draw(ctx, image, a, 240, 282, direction, 1, 260);
    });
    requestAnimationFrame(render);
  }
  document.querySelectorAll("[data-character]").forEach(button => button.addEventListener("click", () => select(button.dataset.character)));
  document.getElementById("pause").addEventListener("click", event => {
    paused = !paused; event.currentTarget.textContent = paused ? "Play" : "Pause";
    event.currentTarget.setAttribute("aria-pressed", String(paused));
  });
  document.getElementById("flip").addEventListener("click", event => {
    direction *= -1; event.currentTarget.setAttribute("aria-pressed", String(direction < 0));
  });
  document.getElementById("speed").addEventListener("change", event => { speed = Number(event.target.value); });
  select("logan"); requestAnimationFrame(render);
})();
