# Animation audit evidence

Baseline `5f59a80`, 14 September 2026, Unity 6000.3.6f1. All PNGs are actual Unity camera renders. No source artwork was generated or repainted in this repair.

- `before/`: initial 56-frame, both-facing sheets and live Logan captures before code repairs.
- `after/`: final sheets and live Logan/Thornling/Guardian captures after repairs. CSV records the actual live clip/frame and pose bounds.
- `index.html`: local screenshot gallery, with before/after comparisons and every capture. The browser tool blocked its local-file URL; individual PNGs were inspected directly. Relative file links were checked locally.
- `before-audit-results.xml`: original two capture methods passed; this did not mean the artwork was accepted.
- `first-fix-results.xml`: diagonal backflip clearance and enemy test setup failures, retained as review history.
- `gameplay-passed-cache-failed.xml`: combined 18-method run: all 13 existing movement/quest methods plus four animation methods passed; repeating the frame audit failed on a stale Sprite cache. That failure triggered the final cache fix.
- `animation-final-passed.xml`: final six animation checks passed, including destroyed-cache recovery, all frame samples, live Logan actions, actual enemy transitions, both-facing foot clearance and weapon/tuned-dash presentation.

Ten existing Node/browser animation tests also passed after metadata synchronization. These are distinct from Unity results. There was no single 19-method final run. Missing weapon/second-jump/backflip/defeat artwork, standalone/device testing and final art approval remain open.

To rerun: stop Unity, then **Tools → Hero → Run Animation Audit**. Normal outputs are in `unity/Logs/animation-audit/current/`; the recorded review used explicit `before` / `after` phases. The editor test file provides the same real-renderer sampling and live-state capture workflow for future repairs.
