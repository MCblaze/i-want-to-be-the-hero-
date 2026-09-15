using UnityEngine;

namespace IWantToBeTheHero
{
    [DisallowMultipleComponent]
    public sealed class DevelopmentOnlyLabel : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
