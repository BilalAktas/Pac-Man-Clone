using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class GhostChaseState : GhostState
    {
        public GhostChaseState(Ghost ghost) : base(ghost)
        {
        }

        private readonly Queue<Node> _trail = new();
        private Node _currentTarget;
        private float _followTimer;
        
        private float _colorLoopTimer;
        private Color _baseColor;
        private bool _dark;

        public override void Enter()
        {
            _trail.Clear();
            _currentTarget = null;
            _followTimer = 0;
            _baseColor = Ghost.GetRenderer().color;
            
            EventBus.Subscribe<OnPacmanNodeChangedEvent>(OnPacmanNodeChanged);
            EventBus.Subscribe<OnCollectPowerPelletEvent>(OnPowerPelletCollected);

        }

        public void GetCurrentPath(List<Node> nodes)
        {
            foreach (var node in nodes) _trail.Enqueue(node);
        }

        private void OnPacmanNodeChanged(OnPacmanNodeChangedEvent e)
        {
            _trail.Enqueue(e.Node);
        }

        private void OnPowerPelletCollected(OnCollectPowerPelletEvent e)
        {
            Ghost.ChangeState(GhostStateEnum.Frightened);
        }
        
        public override void Update()
        {
            _followTimer += Time.deltaTime;
            _colorLoopTimer += Time.deltaTime;
            if (_colorLoopTimer >= Ghost.Config.FollowColorLoopDuration)
            {
                _dark = !_dark;
                _colorLoopTimer = 0;
                var color = _dark ? _baseColor * 0.5f : _baseColor;
                color.a = 1;
                Ghost.GetRenderer().color = color;
            }
            
            if (_followTimer >= Ghost.Config.FollowDuration)
            {
                Ghost.ChangeState(GhostStateEnum.Patrol);
                return;
            }
            
            if (Ghost.Moving)
            {
                Ghost.Move();
                return;
            }
            
            if (_currentTarget == null && _trail.Count > 0)
                _currentTarget = _trail.Dequeue();
            
            if (_currentTarget == null)
                return; 
            
            if (_currentTarget == Ghost.CurrentNode)
            {
                _currentTarget = null;
                return;
            }
            
            var dir = Grid.Instance.GetDirectionBetweenNodes(Ghost.CurrentNode, _currentTarget);
            Ghost.CurrentDirection = dir;
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
            EventBus.Unsubscribe<OnPacmanNodeChangedEvent>(OnPacmanNodeChanged);
            EventBus.Unsubscribe<OnCollectPowerPelletEvent>(OnPowerPelletCollected);
            _trail.Clear();
        }
    }
}