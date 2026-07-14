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
        public GhostStateEnum CurrentState => _stateMachine.CurrentStateEnum;

        private void Start()
        {
            CharacterType = CharacterType.Ghost;
            ChangeMoveSpeed(GhostStateEnum.Patrol);

            _stateMachine = new GhostStateMachine();

            _stateMachine.RegisterState(GhostStateEnum.Patrol, new GhostPatrolState(this));
            _stateMachine.RegisterState(GhostStateEnum.Chase, new GhostChaseState(this));
            _stateMachine.RegisterState(GhostStateEnum.Frightened, new GhostFrightenedState(this));
            _stateMachine.RegisterState(GhostStateEnum.Die, new GhostDieState(this));

            SetColor();
            
            _stateMachine.Start(GhostStateEnum.Patrol);
            EventBus.Subscribe<OnBattleFinishedEvent>(OnBattleFinished);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnBattleFinishedEvent>(OnBattleFinished);
        }

        private void OnBattleFinished(OnBattleFinishedEvent e)
        {
            if (e.Node.GridPosition != CurrentNode.GridPosition) return;
            
            if (e.Winner != this) {
                SetNode(null);
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
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
                GhostStateEnum.Frightened=> Config.FrightenedMoveSpeed,
                GhostStateEnum.Die => Config.DeathMoveSpeed
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

        public void SetColor() => GetRenderer.color = Config.GhostColor;
    }
}