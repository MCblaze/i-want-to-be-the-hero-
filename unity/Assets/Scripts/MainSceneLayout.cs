using UnityEngine;

namespace IWantToBeTheHero
{
    public sealed class MainSceneLayout : MonoBehaviour
    {
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
