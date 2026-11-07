namespace Core
{
    public enum GameState
    {
        Idle,
        Play
    }
    
    public class GameManager : Singleton<GameManager>
    {
        public GameState GameState { get; private set; }

        private void Start()
        {
            GameState = GameState.Play;
            EventBus.Subscribe<OnGhostDiedEvent>(OnGhostDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnGhostDiedEvent>(OnGhostDied);
        }

        private void OnGhostDied(OnGhostDiedEvent e)
        {
            GameState = GameState.Idle;
            Invoke(nameof(SetGameStateToPlay), 1f);
        }

        private void SetGameStateToPlay() => GameState = GameState.Play;
    }
}