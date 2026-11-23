using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameConstants", menuName = "ScriptableObjects/GameConstants")]
    public class GameConstants : ScriptableObject
    {
        [SerializeField] private float _powerPelletDuration; // 15
        [SerializeField] private float _powerPelletWarningTime; // 3
        [SerializeField] private int _pointScore; // 10
        [SerializeField] private int _ghostKillScore; // 200
        
        public float PowerPelletDuration => _powerPelletDuration;
        public float PowerPelletWarningTime => _powerPelletWarningTime;
        public int PointScore => _pointScore;
        public int GhostKillScore => _ghostKillScore;
    }
}
