using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GhostConfig", menuName = "ScriptableObjects/GhostConfig", order = 0)]
    public class GhostConfig : ScriptableObject
    {
        [SerializeField, Min(.1f)] private float _idleMoveSpeed;
        [SerializeField, Min(.1f)] private float _followMoveSpeed;
        [SerializeField, Min(.1f)] private float _deathMoveSpeed;
        [SerializeField] private Color _ghostColor;
        [SerializeField, Range(.05f, 1)] private float _followColorLoopDuration;
        [SerializeField, Min(1)] private float _followDuration;
        
        public float IdleMoveSpeed => _idleMoveSpeed;
        public float FollowMoveSpeed => _followMoveSpeed;
        public float DeathMoveSpeed => _deathMoveSpeed;
        public Color GhostColor => _ghostColor;
        public  float FollowColorLoopDuration => _followColorLoopDuration;
        public float FollowDuration => _followDuration;
    }
}