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
                if(CharacterType == CharacterType.Pacman) _anim?.SetInteger(_stateId, (int)value);
            }
        }

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            SetAlive(true);
        }
        private SpriteRenderer _renderer;

        public SpriteRenderer GetRenderer()
        {
            if (_renderer)
                return _renderer;
            
            _renderer = GetComponent<SpriteRenderer>();
            return _renderer;
        }

        public void SetRendererActive(bool state)
        {
            GetRenderer().enabled = state;   
        }
        
        [HideInInspector] public bool Moving;
        [HideInInspector] public Vector3 StartPosition;
        [HideInInspector] public Node NextNode;
        [HideInInspector] public CharacterType CharacterType;
        public bool IsAlive { get; private set; }
        public void SetAlive(bool state) => IsAlive = state;
        
        private float _moveTimer;
        private float _moveSpeed;
        protected void SetMoveSpeed(float speed) => _moveSpeed = speed;

        public Node CurrentNode { get; private set; }
        
        public Node SpawnNode { get; private set; }
        public void SetSpawnNode(Node node) => SpawnNode = node;
        
        public virtual void ResetToSpawn()
        {
            Moving = false;
            _moveTimer = 0;
            NextNode = null;
            SetAlive(true);
            State = CharacterState.Idle;

            if (SpawnNode == null) return;

            transform.position = SpawnNode.WorldPosition;
            SetNode(SpawnNode);
        }


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
            if (GameManager.Instance.GameState != GameState.Play) return;
            
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
            CurrentNode?.RemoveCharacter(this);
            CurrentNode = node;
            CurrentNode?.AddCharacter(this);
        }
        
    }
}