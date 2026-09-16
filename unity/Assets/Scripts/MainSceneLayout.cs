using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class MainSceneLayout : MonoBehaviour
    {
        [System.Serializable]
        public sealed class EnemySpawn
        {
            public string encounter = "Encounter";
            public Vector2 position;
            public float patrolMinX;
            public float patrolMaxX;
        }

        [System.Serializable]
        public sealed class CheckpointSpawn
        {
            public GameObject marker;
            public Vector2 respawnPosition;
            [Tooltip("Progress order; returning to an earlier checkpoint must not move the respawn backwards.")]
            public int progressOrder;
        }

        [Header("Route bounds")]
        [Tooltip("Default bounds retain the verified Main route. Candidate scenes can expand these independently.")]
        public Vector2 horizontalBounds = new(0f, 51f);
        public float cameraCenterY;
        public float killPlaneY = -8f;

        [Header("Authored encounters")]
        [Tooltip("When enabled, use enemySpawns (including an intentionally empty list) instead of Main's five legacy enemies.")]
        public bool useAuthoredEnemySpawns;
        public EnemySpawn[] enemySpawns = System.Array.Empty<EnemySpawn>();
        public Vector2 bossSpawnPosition = new(45.4f, -2.05f);
        public Vector2 bossArenaBounds = new(41.1f, 49.8f);
        public float bossWakeX = 41.1f;
        [Tooltip("Optional ordered checkpoints. Keep legacy checkpoint binding when this array is empty.")]
        public CheckpointSpawn[] checkpoints = System.Array.Empty<CheckpointSpawn>();

        [Header("Scene bindings")]
        [Tooltip("Movement and weapon settings shared with the verified movement lab.")]
        public HeroTuning tuning;
        [Tooltip("Saved scene camera used in Edit Mode and Play Mode.")]
        public Camera mainCamera;
        [Tooltip("Saved backdrop object; its follow behaviour is connected during Play Mode.")]
        public GameObject backdrop;
        [Tooltip("Editable spawn marker for Logan.")]
        public Transform heroSpawn;
        [Tooltip("Visible editor marker hidden automatically during Play Mode.")]
        public GameObject heroPreview;
        [Tooltip("Gate removed after collecting the Hero Spark.")]
        public GameObject sparkGate;
        [Tooltip("Checkpoint object that receives gameplay behaviour during Play Mode.")]
        public GameObject checkpoint;
        public Vector2 checkpointRespawnPosition = new(23.95f, -2.8f);
        [Tooltip("Hero Spark object that receives gameplay behaviour during Play Mode.")]
        public GameObject heroSpark;
        [Tooltip("Editable low/high choice targets used by the QP2 encounter.")]
        public GameObject[] encounterTargets;
        [Tooltip("Hazard objects that receive damage behaviour during Play Mode.")]
        public GameObject[] hazards;
        [Tooltip("Visible optional backflip cache marker; never required for gate or court access.")]
        public GameObject optionalBackflipCache;
    }
}
