using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public enum GameState
    {
        Idle,
        Play,
        Respawning,
        Win,
        Lose
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField, Min(1)] private int _maxLives = 3;
        [SerializeField, Min(0f)] private float _respawnDelay = 1.5f;
        [SerializeField, Min(0f)] private float _restartDelay = 3f;
        [SerializeField, Min(0f)] private float _ghostDeathFreezeDuration = 1f;

        private int _remainingCollectables;

        public GameState GameState { get; private set; }
        public int Lives { get; private set; }
        public int MaxLives => _maxLives;
        public float GhostDeathFreezeDuration => _ghostDeathFreezeDuration;
        public bool IsGameEnded => GameState == GameState.Win || GameState == GameState.Lose;

        private void Awake()
        {
            Lives = _maxLives;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            GameState = GameState.Play;
            _remainingCollectables = Grid.Instance.TotalCollectables;

            EventBus.Subscribe<OnGhostDiedEvent>(OnGhostDied);
            EventBus.Subscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Subscribe<OnCollectableCollectedEvent>(OnCollectableCollected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnGhostDiedEvent>(OnGhostDied);
            EventBus.Unsubscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Unsubscribe<OnCollectableCollectedEvent>(OnCollectableCollected);
        }

        private void OnGhostDied(OnGhostDiedEvent e)
        {
            if (GameState != GameState.Play && GameState != GameState.Idle) return;

            GameState = GameState.Idle;
            CancelInvoke(nameof(SetGameStateToPlay));
            Invoke(nameof(SetGameStateToPlay), _ghostDeathFreezeDuration);
        }

        private void OnBattleFinished(OnBattleFinishedEvent e)
        {
            if (!e.PacmanWon)
                OnPacmanKilled();
        }

        private void OnPacmanKilled()
        {
            if (GameState != GameState.Play) return;

            Lives = Mathf.Max(0, Lives - 1);
            EventBus.Raise(new OnLivesChangedEvent { Lives = Lives, MaxLives = _maxLives });

            if (Lives <= 0)
            {
                EndGame(GameState.Lose);
                return;
            }

            GameState = GameState.Respawning;
            Invoke(nameof(RespawnRound), _respawnDelay);
        }

        private void RespawnRound()
        {
            EventBus.Raise(new OnRoundResetEvent());
            GameState = GameState.Play;
        }

        private void OnCollectableCollected(OnCollectableCollectedEvent e)
        {
            _remainingCollectables--;
            if (_remainingCollectables <= 0)
                EndGame(GameState.Win);
        }

        private void EndGame(GameState result)
        {
            if (IsGameEnded) return;
            
            CancelInvoke(nameof(SetGameStateToPlay));
            GameState = result;

            if (result == GameState.Win)
                EventBus.Raise(new OnGameWonEvent());
            else
                EventBus.Raise(new OnGameLostEvent());

            Invoke(nameof(RestartGame), _restartDelay);
        }

        private void SetGameStateToPlay()
        {
            if (GameState == GameState.Idle)
                GameState = GameState.Play;
        }

        private void RestartGame()
        {
            EventBus.Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
