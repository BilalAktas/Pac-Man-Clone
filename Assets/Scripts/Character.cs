using System;
using UnityEngine;

namespace Core
{
    public class Character : MonoBehaviour
    {
        private static readonly int _stateId = Animator.StringToHash("State");
        private Animator _anim;
        private CharacterState _state;

        protected CharacterState State
        {
            get { return _state; }
            set
            {
                _state = value;
                _anim.SetInteger(_stateId, (int)value);
            }
        }
        
        private void Start()
        {
            _anim = GetComponent<Animator>();
        }

        protected bool _moving;
        protected Vector3 _startPosition;
        protected Node _nextNode;
        private float _moveTimer;
        [SerializeField] private float _moveSpeed;

        public Node CurrentNode {get; private set;}
        
        
        protected void Move()
        {
            _moveTimer += Time.deltaTime;
            var progress = Mathf.Clamp01(_moveTimer / _moveSpeed);
            transform.position = Vector3.Lerp(_startPosition, _nextNode.WorldPosition, progress);
            transform.right = _nextNode.WorldPosition - _startPosition;
            
            //Debug.Log($"st {_startPosition}  --  tp {_nextPosition}");
            
            if (progress >= 1f)
            {
                OnCompleteMove();
            }
        }

        private void OnCompleteMove()
        {
            _moving = false;
            _moveTimer = 0;
            SetNode(_nextNode);
        }

        private void SetNode(Node node)
        {
            CurrentNode?.SetCharacter(null);
            CurrentNode = node;
            CurrentNode.SetCharacter(this);
        }
    }
}