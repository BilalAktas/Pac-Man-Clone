using UnityEngine;

namespace Core
{
    public class GhostIdleState : GhostState
    {
        public GhostIdleState(Ghost ghost) : base(ghost)
        {
        }

        private MoveDirection _currentMoveDirection;

        public override void Enter()
        {
            _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
            Ghost.SetColor();
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

            var nodes = Grid.Instance.GetAllNodesByDirection(_currentMoveDirection, 5, Ghost.CurrentNode);
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Character && nodes[i].Character.CharacterType == CharacterType.Pacman)
                {
                    Ghost.ChangeState(GhostStateEnum.Follow);
                    if (Ghost.StateMachine.GetState(GhostStateEnum.Follow) is GhostFollowState followState)
                        followState.GetCurrentPath(nodes.GetRange(0, i + 1));

                    return;
                }
            }
        }

        public override void Exit()
        {
        }
    }
}