using UnityEngine;

namespace Core
{
    public class Ghost : Character
    {
        public GhostConfig Config;
        [HideInInspector] public MoveDirection CurrentDirection;
        [SerializeField] private Transform[] _eyes;

        private GhostStateMachine _stateMachine;
        public GhostStateMachine StateMachine => _stateMachine;
        public GhostStateEnum CurrentState =>
            _stateMachine != null ? _stateMachine.CurrentStateEnum : GhostStateEnum.Patrol;

   
        public bool IsVulnerable => CurrentState == GhostStateEnum.Frightened;

        private void Start()
        {
            CharacterType = CharacterType.Ghost;
            ChangeMoveSpeed(GhostStateEnum.Patrol);

            _stateMachine = new GhostStateMachine();

            _stateMachine.RegisterState(GhostStateEnum.Patrol, new GhostPatrolState(this));
            _stateMachine.RegisterState(GhostStateEnum.Chase, new GhostChaseState(this));
            _stateMachine.RegisterState(GhostStateEnum.Frightened, new GhostFrightenedState(this));
            _stateMachine.RegisterState(GhostStateEnum.Die, new GhostDieState(this));

            SetBaseColor();

            _stateMachine.Start(GhostStateEnum.Patrol);
            EventBus.Subscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Subscribe<OnRoundResetEvent>(OnRoundReset);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Unsubscribe<OnRoundResetEvent>(OnRoundReset);
            
            _stateMachine?.Stop();
        }

        private void OnBattleFinished(OnBattleFinishedEvent e)
        {
            if (e.Ghost != this || !e.PacmanWon) return;

            ChangeState(GhostStateEnum.Die);
        }

        private void OnRoundReset(OnRoundResetEvent e) => ResetToSpawn();

        public override void ResetToSpawn()
        {
            ChangeState(GhostStateEnum.Patrol);
            CurrentDirection = MoveDirection.None;

            base.ResetToSpawn();
            SetRendererActive(true);
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Play) return;

            _stateMachine.Update();
            SetEyePosition();
        }

        public void ChangeState(GhostStateEnum newState)
        {
            _stateMachine.ChangeState(newState);
            ChangeMoveSpeed(newState);
        }

        private void ChangeMoveSpeed(GhostStateEnum newState)
        {
            var speed = newState switch
            {
                GhostStateEnum.Patrol => Config.IdleMoveSpeed,
                GhostStateEnum.Chase => Config.ChaseMoveSpeed,
                GhostStateEnum.Frightened => Config.FrightenedMoveSpeed,
                GhostStateEnum.Die => Config.DeathMoveSpeed,
                _ => Config.IdleMoveSpeed
            };

            SetMoveSpeed(speed);
        }

        private void SetEyePosition()
        {
            foreach (var eye in _eyes)
            {
                eye.localPosition = CurrentDirection switch
                {
                    MoveDirection.Up => new Vector3(0, .3f, 0),
                    MoveDirection.Down => new Vector3(0, -.3f, 0),
                    MoveDirection.Left => new Vector3(-.3f, 0, 0),
                    MoveDirection.Right => new Vector3(.3f, 0, 0),
                    _ => Vector3.zero
                };
            }
        }

        public void SetBaseColor() => GetRenderer().color = Config.GhostColor;
        public void SetColor(Color c) => GetRenderer().color = c;
    }
}
