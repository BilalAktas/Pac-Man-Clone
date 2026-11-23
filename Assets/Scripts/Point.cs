using UnityEngine;

namespace Core
{
    public class Point : MonoBehaviour, ICollectable
    {
        [SerializeField] private GameConstants _gameConstants;
        public void Collect()
        {
            gameObject.SetActive(false);
            EventBus.Raise(new OnCollectPointEvent()
            {
                PointAmount = _gameConstants.PointScore
            });
        }
    }
}