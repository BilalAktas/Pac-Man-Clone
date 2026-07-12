using UnityEngine;

namespace Core
{
    public class Point : MonoBehaviour, ICollectable
    {
        public void Collect()
        {
            gameObject.SetActive(false);
            EventBus.Raise(new OnCollectPointEvent()
            {
                PointAmount = 10
            });
        }
    }
}