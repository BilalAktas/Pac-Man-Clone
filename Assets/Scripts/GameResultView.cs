using TMPro;
using UnityEngine;

namespace Core
{
    public class GameResultView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _text;
        private const string WIN_MESSAGE = "YOU WIN!";
        private const string LOSE_MESSAGE = "GAME OVER";

        private void Start()
        {
            if (_root != null) _root.SetActive(false);

            EventBus.Subscribe<OnGameWonEvent>(OnGameWon);
            EventBus.Subscribe<OnGameLostEvent>(OnGameLost);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameWonEvent>(OnGameWon);
            EventBus.Unsubscribe<OnGameLostEvent>(OnGameLost);
        }

        private void OnGameWon(OnGameWonEvent e) => Show(WIN_MESSAGE);
        private void OnGameLost(OnGameLostEvent e) => Show(LOSE_MESSAGE);

        private void Show(string message)
        {
            if (_text != null) _text.text = message;
            if (_root != null) _root.SetActive(true);
        }
    }
}
