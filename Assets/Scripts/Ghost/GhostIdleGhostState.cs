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
        }

        public override void Update()
        {
            if (Ghost.Moving)
            {
                Ghost.Move();
            }
            else
            {
                if (_currentMoveDirection != MoveDirection.None)
                {
                    Debug.Log($"g {Ghost.transform.name}");
                    Ghost.StartPosition = Ghost.CurrentNode.WorldPosition;
                    var node = Grid.Instance.GetNodeByDirection(Ghost.CurrentNode.WorldPosition, _currentMoveDirection);
                    if (node!=null && node.IsWalkable)
                    {
                        Ghost.NextNode = Grid.Instance.GetNodeByDirection(Ghost.CurrentNode.WorldPosition, _currentMoveDirection);
                    }
                    else
                    {
                        _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
                    }
                    
                    if (Ghost.NextNode == null)
                    {
                        //Ghost.State = CharacterState.Idle;
                        return;
                    }

                    if (!Ghost.NextNode.IsWalkable)
                    {
                      //  Ghost.State = CharacterState.Idle;
                        return;
                    }

                    //Ghost.State = CharacterState.Moving;
                    Ghost.Moving = true;
                }
            }
        }

        public override void Exit()
        {
        }
    }
}