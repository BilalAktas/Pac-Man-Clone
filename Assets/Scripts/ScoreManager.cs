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
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCollectPointEvent>(OnCollectPoint);
        }

        private void OnCollectPoint(OnCollectPointEvent e)
        {
            _currentScore += e.PointAmount;
            _scoreText.text = _currentScore.ToString();
        }
    }
}