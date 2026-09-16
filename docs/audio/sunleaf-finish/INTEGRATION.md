# Original procedural audio completion batch — 17 September 2026

Seven original synthesized WAVs. No external samples, downloads or licensed source recordings. Generator uses Python standard library, seeded noise, decaying sine partials and a D-major musical cadence. Reproduction: run `generate_finish_audio.py`, then `validate_finish_audio.py`. Preserve generator, manifest and WAVs in Git and asset backup. These are original draft game cues, not a claim of professional mixing or listening qualification.

## Cue mapping

| Cue | WAVs | Bus | Suggested volume | Pitch | Cooldown |
|---|---|---|---|---|---|
| LoganFootstepStone | sunleaf_footstep_stone_1/2/3 | SoundEffects | .40 | .96–1.04 | .12s |
| LoganFootstepWood | sunleaf_footstep_wood_1/2/3 | SoundEffects | .40 | .96–1.04 | .12s |
| LoganFootstepDefault | stone variants as fallback | SoundEffects | .32 | .96–1.04 | .12s |
| MusicVictory | sunleaf_victory_resolve | Music | .65 | 1 | 0 |

Existing footstep enum values must stay unchanged. Append MusicVictory after LoganWandCast. Director currently sets music loop true for every cue: make victory non-looping explicitly (or introduce configurable loop flag with old music default true). Call PlayMusic(MusicVictory, .35f) on actual CompleteQuest only, replacing canopy music. Do not route victory through SFX, because that bypasses user music volume. Existing music mixer preferences should apply.

## Safe controller hooks (parent-owned file)

Implement within HeroController, which already owns true support-relative movement and gameplay state; a standalone observer would duplicate collision tracking unnecessarily.

1. Keep a footstep distance accumulator. While Started && !Won && !Paused && IsGrounded && !Dashing && !Backflipping, obtain abs(body.linearVelocity.x - SupportVelocity.x). Below .2 speed reset the accumulator, otherwise add speed * Time.deltaTime. Trigger each ~1.25 world units of travel, at most once per frame. Reset accumulation while airborn/paused/respawning/title/victory to avoid delayed footfalls.
2. In accepted floor contact (normal.y > .55), remember collision.collider.GetComponentInParent<SurfaceAudioTag>(); static colliders have null Rigidbody2D so don't rely on support rigidbody for surface lookup. Call tag.FootstepCue or LoganFootstepDefault. Clear stale tag on respawn; replace on each valid floor contact.
3. Idle on moving platforms must be silent because support-relative speed is zero. Never derive footsteps from world displacement or platform world velocity.
4. Test stone/wood variation, mute + effects slider, jumping, moving-platform idle, pause/resume, checkpoint reset. Hearing/subjective mix remains to be reviewed in the running game.

## Asset import

Use Assets/Audio/Sunleaf (existing pack location); preserve Unity-generated GUID metas after import. PCM WAV is 44.1kHz 16-bit; footsteps .23 seconds mono, victory 8 seconds stereo. Disable normalization, make footstep spatial blend 0 for existing consistent 2D mix. Victory is a short resolve with a silent tail, not a looping level track.
