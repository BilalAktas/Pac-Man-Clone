using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class Pacman : Character
    {
        private MoveDirection _targetMoveDirection;
        private MoveDirection _currentMoveDirection;
        private Transform _dirArrow;
        private Node _lastTrackedNode;
        private bool _inBattle;


        private void GetDirectionByInput()
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Up;
            if (Keyboard.current.sKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Down;
            if (Keyboard.current.aKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Left;
            if (Keyboard.current.dKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Right;
        }

        private void OnEnable()
        {
            CharacterType = CharacterType.Pacman;
            SetMoveSpeed(.15f);
            _dirArrow = transform.GetChild(0);

            EventBus.Subscribe<OnBattleFinishedEvent>(OnBattleFinished);
            
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnBattleFinishedEvent>(OnBattleFinished);
        }

        private void OnBattleFinished(OnBattleFinishedEvent e)
        {
            //if (e.Node.GridPosition != CurrentNode.GridPosition) return;

            _inBattle = false;
            //
          
            if (e.Winner != this)
            {
                SetNode(null);
                Time.timeScale = 0f;
            }
            else
            {
                Invoke(nameof(OnKilledGhost), 1);
                EventBus.Raise(new OnCollectPointEvent()
                {
                    PointAmount = 200
                });
            }
        }

        private void OnKilledGhost()
        {
            SetRendererActive(true);
        }

        private void Update()
        {
            if (CurrentNode != _lastTrackedNode)
            {
                _lastTrackedNode = CurrentNode;
                EventBus.Raise(new OnPacmanNodeChangedEvent { Node = CurrentNode });
            }

            if (CurrentNode.Collectable != null)
                CurrentNode.Collect();

            CheckGhostCollision();

            GetDirectionByInput();
            UpdateArrowRotation();

            if (Moving)
            {
                Move();
                return;
            }

            if (_targetMoveDirection == MoveDirection.None)
                return;

            if (TryMoveInDirection(_targetMoveDirection))
            {
                _currentMoveDirection = _targetMoveDirection;
            }
            else if (!TryMoveInDirection(_currentMoveDirection))
            {
                _targetMoveDirection = MoveDirection.None;
                State = CharacterState.Idle;
            }
        }

        private void CheckGhostCollision()
        {
            if (_inBattle) return;

            if (CurrentNode != null)
            {
                foreach (var character in CurrentNode.Characters)
                {
                    if (character.CharacterType == CharacterType.Ghost && character.IsAlive)
                    {
                        _inBattle = true;
                        SetRendererActive(false);
                        EventBus.Raise(new OnBattleStartedEvent
                        {
                            Pacman = this,
                            Ghost = character,
                            Node = CurrentNode
                        });

                        break;
                    }
                }
            }    
            
        }

        private void UpdateArrowRotation()
        {
            switch (_targetMoveDirection)
            {
                case MoveDirection.Up:
                    _dirArrow.rotation = Quaternion.Euler(0, 0, 90);
                    break;
                case MoveDirection.Down:
                    _dirArrow.rotation = Quaternion.Euler(0, 0, 270);
                    break;
                case MoveDirection.Left:
                    _dirArrow.rotation = Quaternion.Euler(0, 0, 180);
                    break;
                case MoveDirection.Right:
                    _dirArrow.rotation = Quaternion.Euler(0, 0, 0);
                    break;
            }
        }
    }
}