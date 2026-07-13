using UnityEngine;

namespace Core
{
    public class Character : MonoBehaviour
    {
        private static readonly int _stateId = Animator.StringToHash("State");
        private Animator _anim;
        private CharacterState _state;
        public CharacterState State
        {
            get { return _state; }
            set
            {
                _state = value;
                _anim?.SetInteger(_stateId, (int)value);
            }
        }

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        [HideInInspector] public bool Moving;
        [HideInInspector] public Vector3 StartPosition;
        [HideInInspector] public Node NextNode;
        [HideInInspector] public CharacterType CharacterType;
        private float _moveTimer;
        private float _moveSpeed;
        public void SetMoveSpeed(float speed) => _moveSpeed = speed;

        public Node CurrentNode { get; private set; }


        public bool TryMoveInDirection(MoveDirection dir)
        {
            if (dir == MoveDirection.None)
                return false;

            var node = Grid.Instance.GetNodeByDirection(CurrentNode, dir);
            if (node == null || !node.IsWalkable)
                return false;

            StartPosition = CurrentNode.WorldPosition;
            NextNode = node;
            Moving = true;
            State = CharacterState.Moving;
            return true;
        }

        public void Move()
        {
            _moveTimer += Time.deltaTime;
            var progress = Mathf.Clamp01(_moveTimer / _moveSpeed);
            transform.position = Vector3.Lerp(StartPosition, NextNode.WorldPosition, progress);
            if(CharacterType == CharacterType.Pacman) transform.right = NextNode.WorldPosition - StartPosition;

            if (progress >= 1f)
            {
                OnCompleteMove();
            }
        }

        private void OnCompleteMove()
        {
            Moving = false;
            _moveTimer = 0;
            SetNode(NextNode);
        }

        public void SetNode(Node node)
        {
            CurrentNode?.SetCharacter(null);
            CurrentNode = node;
            CurrentNode.SetCharacter(this);
        }
    }
}