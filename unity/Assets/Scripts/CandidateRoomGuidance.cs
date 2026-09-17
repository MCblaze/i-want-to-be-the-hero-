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

        private void Awake()
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
                "Keep space from thorns. Strike, then recover.",
                "Look for the far edge before crossing each gap.",
                "Keep momentum across broken floor.",
                "Moving lifts introduce timing and patience.",
                "Use the safer lower route when needed.",
                "Chain ledges and watch enemy spacing.",
                "The Hero Spark unlocks stronger mobility.",
                "Swap sword and wand to match the threat.",
                "Mix movement, evasion, and lift timing.",
                "The court is close. Your next checkpoint is ahead.",
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

