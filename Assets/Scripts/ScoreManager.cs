using TMPro;
using UnityEngine;

namespace Core
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        private int _currentScore;
        private void Start()
        {
            EventBus.Subscribe<OnCollectPointEvent>(OnCollectPoint);
            Refresh();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCollectPointEvent>(OnCollectPoint);
        }

        private void OnCollectPoint(OnCollectPointEvent e)
        {
            _currentScore += e.PointAmount;
            Refresh();
        }

        private void Refresh()
        {
            if (_scoreText != null)
                _scoreText.text = _currentScore.ToString();
        }
    }
}