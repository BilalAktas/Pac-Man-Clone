using UnityEngine;

namespace Core
{
    public class GhostFrightenedState : GhostState
    {
        public GhostFrightenedState(Ghost ghost) : base(ghost)
        {
        }

        private MoveDirection _currentMoveDirection;
        
        public override void Enter()
        {
            _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
            Ghost.GetRenderer.color = Color.blue;
        }

        public override void Update()
        {
            if (Ghost.Moving)
            {
                Ghost.Move();
                Ghost.CurrentDirection = _currentMoveDirection;
                return;
            }

            if (!Ghost.TryMoveInDirection(_currentMoveDirection))
            {
                _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
                return;
            }
        }

        public override void Exit()
        {
            
        }
    }
}