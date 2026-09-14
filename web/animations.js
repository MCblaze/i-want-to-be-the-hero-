(function (root) {
  "use strict";

  // Shared by the game and the animation viewer. All time is simulation time:
  // rendering, pausing, or changing display refresh rate cannot advance a clip.
  class AnimationPlayer {
    constructor(definition) {
      this.definition = definition;
      this.clips = new Map(definition.clips.map(clip => [clip.name, clip]));
      this.name = "";
      this.time = 0;
      this.set("idle");
    }

    set(name, restart = false) {
      const clip = this.clips.get(name);
      if (!clip) throw new Error(`Unknown ${this.definition.id} animation: ${name}`);
      if (this.name !== name || restart) {
        this.name = name;
        this.clip = clip;
        this.time = 0;
      }
      return this;
    }

    update(dt, rate = 1) {
      this.time += Math.max(0, dt) * Math.max(0, rate);
      if (!this.clip.loop) this.time = Math.min(this.time, this.duration);
      return this;
    }

    seek(seconds) {
      this.time = Math.max(0, seconds);
      return this;
    }

    get duration() { return this.clip.frames.length / this.clip.fps; }
    get finished() { return !this.clip.loop && this.time >= this.duration; }
    get frame() {
      const position = Math.floor((this.time + 1e-9) * this.clip.fps);
      const index = this.clip.loop ? position % this.clip.frames.length : Math.min(position, this.clip.frames.length - 1);
      return this.clip.frames[index];
    }
  }

  function sourceRect(image, definition, frame) {
    if (definition.regions && definition.regions[frame]) return definition.regions[frame];
    const col = frame % definition.columns;
    const row = Math.floor(frame / definition.columns);
    const left = Math.round(col * image.width / definition.columns);
    const top = Math.round(row * image.height / definition.rows);
    return { x: left, y: top,
      w: Math.round((col + 1) * image.width / definition.columns) - left,
      h: Math.round((row + 1) * image.height / definition.rows) - top };
  }

  function draw(ctx, image, player, footX, footY, direction = 1, alpha = 1, height) {
    if (!image) return false;
    const def = player.definition;
    const rect = sourceRect(image, def, player.frame);
    // Keep every pose at the same pixel scale, even when a sword or crouch
    // changes its bounding rectangle. The pivot pins the feet to the ground.
    const scale = (height || def.displayHeight) / (image.height / def.rows);
    const h = rect.h * scale;
    const w = rect.w * scale;
    const pivot = def.pivots && def.pivots[player.frame];
    const px = pivot ? pivot.x : def.pivotX;
    const py = pivot ? pivot.y : def.pivotY;
    ctx.save();
    ctx.imageSmoothingEnabled = false;
    ctx.globalAlpha *= Math.max(0, Math.min(1, alpha));
    ctx.translate(Math.round(footX), Math.round(footY));
    ctx.scale(direction === def.facing ? 1 : -1, 1);
    ctx.drawImage(image, rect.x, rect.y, rect.w, rect.h,
      Math.round(-w * px), Math.round(-h * py), Math.round(w), Math.round(h));
    ctx.restore();
    return true;
  }

  const api = { AnimationPlayer, sourceRect, draw };
  root.HeroAnimations = api;
  if (typeof module !== "undefined" && module.exports) module.exports = api;
})(typeof globalThis !== "undefined" ? globalThis : this);
