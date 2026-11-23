using UnityEngine;

namespace Core
{
    public class LivesView : MonoBehaviour
    {
        [Tooltip("Left to right. Each lost life hides the right-most visible icon.")]
        [SerializeField] private GameObject[] _lifeIcons;

        private void Start()
        {
            Refresh(GameManager.Instance.Lives);
            EventBus.Subscribe<OnLivesChangedEvent>(OnLivesChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnLivesChangedEvent>(OnLivesChanged);
        }

        private void OnLivesChanged(OnLivesChangedEvent e) => Refresh(e.Lives);

        private void Refresh(int lives)
        {
            if (_lifeIcons == null) return;

            for (var i = 0; i < _lifeIcons.Length; i++)
            {
                if (_lifeIcons[i] != null)
                    _lifeIcons[i].SetActive(i < lives);
            }
        }
    }
}
