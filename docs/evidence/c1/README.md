# C1 test evidence

14 September 2026, Unity 6000.3.6f1, Windows Editor.

- `regression-footing.xml`: full 11-test run after fixing idle grounding and moving-platform carry. All four Main lifecycle tests and six movement tests passed. One lab-reset assertion inspected deferred destruction too early; this failure is deliberately retained as development evidence.
- `movement-eight-passed.xml`: all eight movement tests passed after correcting that assertion and adding real gap/coyote/buffer/checkpoint traversal.
- `camera-passed.xml`: the additional camera test passed after visual review exposed a narrow-window visibility problem. It checks both room edges at square, 16:10 and 16:9 aspects.
- `movement-measurements.csv`: the final sampled movement run. The guide reports rounded values and observed variation; do not treat Editor-sampled short-hop timing as an exact input latency measurement.

Result: four existing quest regression methods and nine movement/camera methods passed across the final relevant runs. This is not a claim that one combined 13-test run occurred. The Unity test framework's XML duration fields do not capture all Editor reload/wait time; do not use them as task-performance measurements.

Tests exercise the real controller and shared input adapter. Physical device, standalone-build and player enjoyment checks remain open. See `../../MOVEMENT-LAB.md` for the scope, measurements, fixes and limitations.
