using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GhostDieState : GhostState
    {
        public GhostDieState(Ghost ghost) : base(ghost)
        {
        }

        private readonly Queue<Node> _roadToBase = new();
        private Node _currentTarget;
        private MoveDirection _currentMoveDirection;
        
        public override void Enter()
        {
            Ghost.SetAlive(false);
            Ghost.SetRendererActive(false);
            Ghost.SetColor(Color.white);
            
            _roadToBase.Clear();
            _currentTarget = null;
            var startNode = Ghost.Moving ? Ghost.NextNode : Ghost.CurrentNode;
            var path = Grid.Instance.GetPath(startNode.GridPosition, Grid.Instance.CurrentLevelDesignData.EnemyBaseCell);
            foreach (var n in path)
            {
                _roadToBase.Enqueue(n);
            }
            Ghost.CurrentNode.RemoveCharacter(Ghost);
            EventBus.Raise(new OnGhostDiedEvent());
        }
        

        
        public override void Update()
        {
            if (Ghost.Moving)
            {
                Ghost.Move();
                Ghost.CurrentDirection = _currentMoveDirection;
                
                return;
            }
            
            while (_currentTarget == null || _currentTarget == Ghost.CurrentNode)
            {
                if (_roadToBase.Count == 0)
                {
                    Ghost.ChangeState(GhostStateEnum.Patrol);
                    return;
                }

                _currentTarget = _roadToBase.Dequeue();
            }
            
            var dir = Grid.Instance.GetDirectionBetweenNodes(Ghost.CurrentNode, _currentTarget);
            _currentMoveDirection = dir;
            
            Ghost.CurrentDirection = _currentMoveDirection;
            
            if (dir == MoveDirection.None)
            {
                _currentTarget = null;
                return;
            }
            if (!Ghost.TryMoveInDirection(dir))
            {
                _currentTarget = null;
                return;
            }
            
            if (Ghost.NextNode == _currentTarget)
                _currentTarget = null;
        }

        public override void Exit()
        {
            Ghost.SetAlive(true);
            Ghost.SetRendererActive(true);
            _roadToBase.Clear();
            _currentTarget = null;
        }
    }
}