using UnityEngine;

namespace Core
{
    public class PowerPellet : MonoBehaviour, ICollectable
    {
        public void Collect()
        {
            gameObject.SetActive(false);
            EventBus.Raise(new OnCollectPowerPelletEvent());
        }
    }
}