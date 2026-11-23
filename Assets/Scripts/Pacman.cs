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
        
        public MoveDirection CurrentMoveDirection => _currentMoveDirection;

        private void GetDirectionByInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Up;
            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Down;
            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Left;
            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                _targetMoveDirection = MoveDirection.Right;
        }

        private void OnEnable()
        {
            CharacterType = CharacterType.Pacman;
            SetMoveSpeed(.15f);
            _dirArrow = transform.GetChild(0);

            EventBus.Subscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Subscribe<OnRoundResetEvent>(OnRoundReset);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnBattleFinishedEvent>(OnBattleFinished);
            EventBus.Unsubscribe<OnRoundResetEvent>(OnRoundReset);
        }

        private void OnBattleFinished(OnBattleFinishedEvent e)
        {
            SetRendererActive(false);

            if (e.PacmanWon)
                Invoke(nameof(ShowAfterKill), GameManager.Instance.GhostDeathFreezeDuration);
        }

        private void ShowAfterKill() => SetRendererActive(true);

        private void OnRoundReset(OnRoundResetEvent e) => ResetToSpawn();

        public override void ResetToSpawn()
        {
            CancelInvoke(nameof(ShowAfterKill));
            _targetMoveDirection = MoveDirection.None;
            _currentMoveDirection = MoveDirection.None;
            transform.right = Vector3.right;

            base.ResetToSpawn();
            SetRendererActive(true);
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Play) return;
            if (CurrentNode == null) return;

            if (CurrentNode != _lastTrackedNode)
            {
                _lastTrackedNode = CurrentNode;
                EventBus.Raise(new OnPacmanNodeChangedEvent { Node = CurrentNode });
            }

            if (CurrentNode.Collectable != null)
                CurrentNode.Collect();
            
            if (GameManager.Instance.GameState != GameState.Play) return;

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
