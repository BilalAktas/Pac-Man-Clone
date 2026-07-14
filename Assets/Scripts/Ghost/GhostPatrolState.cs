using UnityEngine;

namespace Core
{
    public class GhostPatrolState : GhostState
    {
        public GhostPatrolState(Ghost ghost) : base(ghost)
        {
        }

        private MoveDirection _currentMoveDirection;

        public override void Enter()
        {
            _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
            Ghost.SetColor();
            
            EventBus.Subscribe<OnCollectPowerPelletEvent>(OnPowerPelletCollected);
        }
        
        private void OnPowerPelletCollected(OnCollectPowerPelletEvent e)
        {
            Ghost.ChangeState(GhostStateEnum.Frightened);
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
                foreach (var character in nodes[i].Characters)
                {
                    if (character.CharacterType == CharacterType.Pacman)
                    {
                        Ghost.ChangeState(GhostStateEnum.Chase);
                        if (Ghost.StateMachine.GetState(GhostStateEnum.Chase) is GhostChaseState followState)
                            followState.GetCurrentPath(nodes.GetRange(0, i + 1));

                        return;
                    }    
                }
            }
        }

        public override void Exit()
        {
            EventBus.Unsubscribe<OnCollectPowerPelletEvent>(OnPowerPelletCollected);
        }
    }
}