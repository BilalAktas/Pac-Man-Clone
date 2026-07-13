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