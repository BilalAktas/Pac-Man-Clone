namespace Core
{
    public abstract class GhostState
    {
        protected Ghost Ghost { get; private set; }
        protected GhostState(Ghost ghost)
        {
            Ghost = ghost;
        }

        public abstract void Enter();
        public abstract void Update();
        public abstract void Exit();
    }
}