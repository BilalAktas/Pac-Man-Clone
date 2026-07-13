using UnityEngine;

namespace Core
{
    public class Ghost : Character
    {
        public GhostConfig Config;
        [HideInInspector] public MoveDirection CurrentDirection;
        [SerializeField] private Transform[] _eyes;

        private SpriteRenderer _renderer;
        public SpriteRenderer GetRenderer => _renderer;
        
        private GhostStateMachine _stateMachine;
        public GhostStateMachine StateMachine => _stateMachine;
        public GhostStateEnum CurrentState => _stateMachine.CurrentStateEnum;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            
            CharacterType = CharacterType.Ghost;
            ChangeMoveSpeed(GhostStateEnum.Idle);
            
            _stateMachine = new GhostStateMachine();

            _stateMachine.RegisterState(GhostStateEnum.Idle, new GhostIdleState(this));
            _stateMachine.RegisterState(GhostStateEnum.Follow, new GhostFollowState(this));
            _stateMachine.RegisterState(GhostStateEnum.Die, new GhostDieState(this));
            
            SetColor();
        }

        private void Start()
        {
            _stateMachine.Start(GhostStateEnum.Idle);
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
                GhostStateEnum.Idle => Config.IdleMoveSpeed,
                GhostStateEnum.Follow => Config.FollowMoveSpeed,
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