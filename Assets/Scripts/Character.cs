using System;
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
                _anim.SetInteger(_stateId, (int)value);
            }
        }
        
        private void Start()
        {
            _anim = GetComponent<Animator>();
        }

        [HideInInspector] public bool Moving;
        [HideInInspector] public Vector3 StartPosition;
        [HideInInspector] public Node NextNode;
        private float _moveTimer;
        [SerializeField] private float _moveSpeed;

        public Node CurrentNode {get; private set;}
        
        public void Move()
        {
            _moveTimer += Time.deltaTime;
            var progress = Mathf.Clamp01(_moveTimer / _moveSpeed);
            transform.position = Vector3.Lerp(StartPosition, NextNode.WorldPosition, progress);
            transform.right = NextNode.WorldPosition - StartPosition;
            
            //Debug.Log($"st {_startPosition}  --  tp {_nextPosition}");
            
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