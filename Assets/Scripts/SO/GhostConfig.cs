using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GhostConfig", menuName = "ScriptableObjects/GhostConfig", order = 0)]
    public class GhostConfig : ScriptableObject
    {
        [SerializeField, Min(.1f)] private float _idleMoveSpeed;
        [SerializeField, Min(.1f)] private float _chaseMoveSpeed;
        [SerializeField, Min(.1f)] private float _frightenedMoveSpeed;
        [SerializeField, Min(.1f)] private float _deathMoveSpeed;
        [SerializeField] private Color _ghostColor;
        [SerializeField, Range(.05f, 1)] private float _followColorLoopDuration;
        [SerializeField, Min(1)] private float _followDuration;
        
        public float IdleMoveSpeed => _idleMoveSpeed;
        public float ChaseMoveSpeed => _chaseMoveSpeed;
        public float DeathMoveSpeed => _deathMoveSpeed;
        public Color GhostColor => _ghostColor;
        public  float FollowColorLoopDuration => _followColorLoopDuration;
        public float FollowDuration => _followDuration;
        public float FrightenedMoveSpeed => _frightenedMoveSpeed;
    }
}