using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class Pacman : Character
    {
        private MoveDirection _targetMoveDirection;
        private MoveDirection _currentMoveDirection;
        private Transform _dirArrow;

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
            _dirArrow = transform.GetChild(0);
        }

        private void Update()
        {
            if(CurrentNode.Collectable != null)
                CurrentNode.Collect();
            
            GetDirectionByInput();

            switch (_targetMoveDirection)
            {
                case MoveDirection.Up:
                    _dirArrow.rotation = Quaternion.Euler(0,0,90);
                    break;
                case MoveDirection.Down:
                    _dirArrow.rotation = Quaternion.Euler(0,0,270);
                    break;
                case MoveDirection.Left:
                    _dirArrow.rotation = Quaternion.Euler(0,0,180);
                    break;
                case MoveDirection.Right:
                    _dirArrow.rotation = Quaternion.Euler(0,0,0);
                    break;
            }
            
            if (Moving)
            {
                Move();
            }
            else
            {
                if (_targetMoveDirection != MoveDirection.None)
                {
                    StartPosition = CurrentNode.WorldPosition;
                    var node = Grid.Instance.GetNodeByDirection(CurrentNode.WorldPosition, _targetMoveDirection);
                    if (node!=null && node.IsWalkable)
                    {
                        NextNode = Grid.Instance.GetNodeByDirection(CurrentNode.WorldPosition, _targetMoveDirection);
                        _currentMoveDirection = _targetMoveDirection;
                    }
                    else
                        NextNode = Grid.Instance.GetNodeByDirection(CurrentNode.WorldPosition, _currentMoveDirection);
                    
                    if (NextNode == null)
                    {
                        _targetMoveDirection = MoveDirection.None;
                        State = CharacterState.Idle;
                        return;
                    }

                    if (!NextNode.IsWalkable)
                    {
                        _targetMoveDirection = MoveDirection.None;
                        State = CharacterState.Idle;
                        return;
                    }

                    State = CharacterState.Moving;
                    Moving = true;
                }
            }
        }
    }
}