using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class GhostFrightenedState : GhostState
    {
        private const float BLINK_INTERVAL = .2f;

        public GhostFrightenedState(Ghost ghost) : base(ghost)
        {
        }

        private MoveDirection _currentMoveDirection;
        private Node _currentPacmanNode;
        private Node _targetNode;
        private MoveDirection _pacmanDirection;

        private bool _warning;
        private bool _blinkWhite;
        private float _blinkTimer;

        public override void Enter()
        {
            _currentMoveDirection = (MoveDirection)Random.Range(1, 5);
            _targetNode = null;
            ResetWarning();
            
            var pacman = Grid.Instance.Pacman;
            if (pacman != null)
            {
                _currentPacmanNode = pacman.CurrentNode;
                _pacmanDirection = pacman.CurrentMoveDirection;
            }

            EventBus.Subscribe<OnPowerPelletEffectFinishedEvent>(OnPowerPelletEffectFinished);
            EventBus.Subscribe<OnPowerPelletWarningEvent>(OnPowerPelletWarning);
            EventBus.Subscribe<OnPacmanNodeChangedEvent>(OnPacmanNodeChanged);
            EventBus.Subscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
        }

        private void ResetWarning()
        {
            _warning = false;
            _blinkWhite = false;
            _blinkTimer = 0;
            Ghost.SetColor(Color.blue);
        }

        private void OnPowerPelletWarning(OnPowerPelletWarningEvent e)
        {
            _warning = true;
            _blinkTimer = 0;
        }

        private void OnCollectPowerPellet(OnCollectPowerPelletEvent e)
        {
            _targetNode = null;
            ResetWarning();
        }

        private void OnPacmanNodeChanged(OnPacmanNodeChangedEvent e)
        {
            if (_currentPacmanNode != null)
            {
                var dir = Grid.Instance.GetDirectionBetweenNodes(_currentPacmanNode, e.Node);
                if (dir != MoveDirection.None)
                    _pacmanDirection = dir;
            }
            _currentPacmanNode = e.Node;
        }

        private void OnPowerPelletEffectFinished(OnPowerPelletEffectFinishedEvent e)
        {
            Ghost.ChangeState(GhostStateEnum.Patrol);
        }

        private void UpdateWarningBlink()
        {
            if (!_warning) return;

            _blinkTimer += Time.deltaTime;
            if (_blinkTimer < BLINK_INTERVAL) return;

            _blinkTimer = 0;
            _blinkWhite = !_blinkWhite;
            Ghost.SetColor(_blinkWhite ? Color.white : Color.blue);
        }

        public override void Update()
        {
            UpdateWarningBlink();

            if (Ghost.Moving)
            {
                Ghost.Move();
                Ghost.CurrentDirection = _currentMoveDirection;
                return;
            }

            if (_currentPacmanNode == null) return;

            if (_targetNode == null || Ghost.CurrentNode == _targetNode)
            {
                var predictedPacmanPos = _currentPacmanNode.WorldPosition;
                if (_pacmanDirection != MoveDirection.None)
                {
                    var lookaheadNodes = Grid.Instance.GetAllNodesByDirection(_pacmanDirection, 4, _currentPacmanNode);
                    if (lookaheadNodes.Count > 0)
                        predictedPacmanPos = lookaheadNodes.Last().WorldPosition;
                }

                var reverseDirection = Grid.Instance.GetOppositeDirection(_currentMoveDirection);
                var directions = new List<MoveDirection>
                {
                    MoveDirection.Up, MoveDirection.Down,
                    MoveDirection.Right, MoveDirection.Left
                };

                var candidates = new Dictionary<MoveDirection, (Node node, float cost)>();

                foreach (var dir in directions)
                {
                    if (dir == reverseDirection) continue;

                    var nodeList = Grid.Instance.GetAllNodesByDirection(dir, 3, Ghost.CurrentNode);
                    if (nodeList.Count <= 0) continue;

                    var nextNode = nodeList.Last();
                    var costNow = Vector2.Distance(nextNode.WorldPosition, _currentPacmanNode.WorldPosition);
                    var costFuture = Vector2.Distance(nextNode.WorldPosition, predictedPacmanPos);
                    candidates[dir] = (nextNode, costNow * 0.4f + costFuture * 0.6f);
                }

                if (candidates.Count == 0 && reverseDirection != MoveDirection.None)
                {
                    var nodeList = Grid.Instance.GetAllNodesByDirection(reverseDirection, 1, Ghost.CurrentNode);
                    if (nodeList.Count > 0)
                        candidates[reverseDirection] = (nodeList[0], 0f);
                }
                
                if (candidates.Count == 0) return;

                var maxCost = candidates.Values.Max(c => c.cost);
                var topChoices = candidates.Where(c => c.Value.cost >= maxCost - 0.15f).ToList();
                var chosen = topChoices[Random.Range(0, topChoices.Count)];

                _currentMoveDirection = chosen.Key;
                _targetNode = chosen.Value.node;

                Ghost.TryMoveInDirection(_currentMoveDirection);
            }
            else
            {
                Ghost.TryMoveInDirection(_currentMoveDirection);
            }
        }

        public override void Exit()
        {
            EventBus.Unsubscribe<OnPowerPelletEffectFinishedEvent>(OnPowerPelletEffectFinished);
            EventBus.Unsubscribe<OnPowerPelletWarningEvent>(OnPowerPelletWarning);
            EventBus.Unsubscribe<OnPacmanNodeChangedEvent>(OnPacmanNodeChanged);
            EventBus.Unsubscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
        }
    }
}
