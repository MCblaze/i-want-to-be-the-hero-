# Candidate Room Guidance

## Status

- **Implementation:** unrun handoff source for Codex review/import.
- **Scope:** one standalone runtime component; no `.cs` file or scene was edited here.
- **Verification:** **Not run**. Unity commands, tests, and Play Mode were intentionally not used.

## Source

Save the following code as `Assets/Scripts/CandidateRoomGuidance.cs` during Codex integration:

```csharp
using System;
using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class CandidateRoomGuidance : MonoBehaviour
    {
        [Serializable]
        public sealed class RoomBand
        {
            public string title;
            [TextArea(1, 2)] public string upcomingMechanic;
            public float minX;
            public float maxX;
        }

        public RoomBand[] rooms = CreateDefaultRooms();

        private HeroGame game;
        private bool[] shown;
        private int currentRoom = -1;

        private void OnEnable()
        {
            currentRoom = -1;
            shown = null;
        }

        private void Start()
        {
            game = FindAnyObjectByType<HeroGame>();
            shown = new bool[rooms == null ? 0 : rooms.Length];
        }

        private void Update()
        {
            if (game == null || game.Hero == null || game.UI == null ||
                game.MainLayout == null || game.MainLayout.horizontalBounds.y <= 200f ||
                !game.Started || game.Paused || game.Won)
                return;

            int room = FindRoom(game.Hero.transform.position.x);
            if (room < 0 || room == currentRoom)
                return;

            currentRoom = room;
            if (shown[room])
                return;

            shown[room] = true;
            RoomBand band = rooms[room];
            game.UI.FlashMessage($"{band.title}\n{band.upcomingMechanic}", 2.25f);
        }

        private int FindRoom(float x)
        {
            if (rooms == null)
                return -1;

            for (int i = 0; i < rooms.Length; i++)
            {
                RoomBand band = rooms[i];
                if (band != null && x >= band.minX &&
                    (x < band.maxX || (i == rooms.Length - 1 && x <= band.maxX)))
                    return i;
            }

            return -1;
        }

        private static RoomBand[] CreateDefaultRooms()
        {
            string[] titles =
            {
                "Trailhead", "First courage", "Broken approach", "Ruin crossing",
                "Canopy lesson", "Canopy choice", "Shrine approach", "Spark shrine",
                "Weapon gallery", "Mastery crossing", "Court threshold", "Guardian court"
            };
            string[] mechanics =
            {
                "Learn movement and the first safe jumps.",
                "Short pit jumps reward a committed takeoff.",
                "Read the ledges and recover after missed jumps.",
                "Keep momentum across broken floor.",
                "Moving lifts introduce timing and patience.",
                "Use the safer lower route when needed.",
                "Chain ledges and watch enemy spacing.",
                "The Hero Spark unlocks stronger mobility.",
                "Swap sword and wand to match the threat.",
                "Mix movement, evasion, and lift timing.",
                "Prepare for the gate and final arena.",
                "The Guardian is ahead—use every lesson."
            };
            var result = new RoomBand[titles.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new RoomBand
                {
                    title = titles[i],
                    upcomingMechanic = mechanics[i],
                    minX = i * 24f,
                    maxX = (i + 1) * 24f
                };
            }
            return result;
        }
    }
}
```

## Exact Codex integration step

1. Import the code block above as `Assets/Scripts/CandidateRoomGuidance.cs` and wait for compilation.
2. Open `Assets/Scenes/Sunleaf_DeliveryCandidate.unity` in the Unity Editor.
3. In the hierarchy, select the child named `Delivery candidate - twelve authored rooms` beneath the object containing `MainSceneLayout`.
4. Add the `CandidateRoomGuidance` component to that child. Do not add it to `MainSceneLayout`, the generated `HeroGame` object, or the Main scene.
5. Leave the serialized `rooms` array at its defaults unless route review changes a title, second line, or x-range. The defaults are twelve editable bands `0..24` through `264..288`; the gate, spark, boss, pits, and lifts remain route facts rather than extra UI or searches.

## Design constraints captured

- Resolves `HeroGame` once in `Start`, after the runtime bootstrap, and never searches every frame.
- Uses the existing `HeroGame.UI.FlashMessage(string, float)` surface; creates no UI.
- Requires active gameplay and suppresses guidance while paused or won.
- Tracks shown rooms independently from the current room, so death/checkpoint repositioning does not replay messages.
- Uses no countdown or coroutine, so pause does not consume guidance timing.
- State is instance-local and resets when a new scene creates a new component instance.
- Does not display unless `MainLayout.horizontalBounds.y > 200`, leaving Main unchanged.
- **All behavior remains unrun until Codex imports and tests it.**
