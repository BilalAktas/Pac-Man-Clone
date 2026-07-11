using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class Pacman : Character
    {
        private MoveDirection _currentMoveDirection;

        private void GetDirectionByInput()
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
                _currentMoveDirection = MoveDirection.Up;
            if (Keyboard.current.sKey.wasPressedThisFrame)
                _currentMoveDirection = MoveDirection.Down;
            if (Keyboard.current.aKey.wasPressedThisFrame)
                _currentMoveDirection = MoveDirection.Left;
            if (Keyboard.current.dKey.wasPressedThisFrame)
                _currentMoveDirection = MoveDirection.Right;
        }
        
        private void Update()
        {
            GetDirectionByInput();

            if (_moving)
            {
                Move();
            }
            else
            {
                if (_currentMoveDirection != MoveDirection.None)
                {
                    _startPosition = transform.position;
                    _nextNode = Grid.Instance.GetNodeByDirection(transform.position, _currentMoveDirection);
                    if (_nextNode == null)
                    {
                        _currentMoveDirection = MoveDirection.None;
                        State = CharacterState.Idle;
                        return;
                    }

                    State = CharacterState.Moving;
                    _moving = true;
                }
            }
        }
    }
}