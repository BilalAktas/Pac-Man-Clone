namespace Core
{
    public class Ghost : Character
    {
        private GhostStateMachine _stateMachine;

        public GhostStateEnum CurrentState => _stateMachine.CurrentStateEnum;

        private void Awake()
        {
            _stateMachine = new GhostStateMachine();

            _stateMachine.RegisterState(GhostStateEnum.Idle, new GhostIdleState(this));
            _stateMachine.RegisterState(GhostStateEnum.Follow, new GhostFollowState(this));
            _stateMachine.RegisterState(GhostStateEnum.Die, new GhostDieState(this));
        }

        private void Start()
        {
            _stateMachine.Start(GhostStateEnum.Idle);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void ChangeState(GhostStateEnum newState)
        {
            _stateMachine.ChangeState(newState);
        }
    }
}