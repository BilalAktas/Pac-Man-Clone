using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class BattleManager : MonoBehaviour
    {
        [Tooltip("Hit distance in node sizes. 0.75 = sprites visibly overlap, and fast frames can't tunnel through.")]
        [SerializeField, Range(.3f, 1f)] private float _hitRadiusInNodes = .75f;

        private readonly List<Ghost> _touching = new();
        private Pacman _pacman;
        private IReadOnlyList<Ghost> _ghosts;
        private float _hitRadiusSqr;

        private bool _pelletActive;
        private bool _warningRaised;
        private float _pelletTimer;

        [SerializeField] private GameConstants _gameConstants;

        private void Start()
        {
            _pacman = Grid.Instance.Pacman;
            _ghosts = Grid.Instance.Ghosts;

            var radius = Grid.Instance.CurrentLevelDesignData.NodeSize.x * _hitRadiusInNodes;
            _hitRadiusSqr = radius * radius;

            EventBus.Subscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
            EventBus.Subscribe<OnRoundResetEvent>(OnRoundReset);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCollectPowerPelletEvent>(OnCollectPowerPellet);
            EventBus.Unsubscribe<OnRoundResetEvent>(OnRoundReset);
        }

        private void OnCollectPowerPellet(OnCollectPowerPelletEvent e)
        {
            _pelletTimer = 0;
            _pelletActive = true;
            _warningRaised = false;
        }

        private void OnRoundReset(OnRoundResetEvent e)
        {
            _pelletTimer = 0;
            _pelletActive = false;
            _warningRaised = false;
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Play) return;

            TickPowerPellet(Time.deltaTime);
            DetectBattles();
        }

        private void TickPowerPellet(float deltaTime)
        {
            if (!_pelletActive) return;

            _pelletTimer += deltaTime;

            var warningAt = _gameConstants.PowerPelletDuration - _gameConstants.PowerPelletWarningTime;
            if (!_warningRaised && _pelletTimer >= warningAt)
            {
                _warningRaised = true;
                EventBus.Raise(new OnPowerPelletWarningEvent());
            }

            if (_pelletTimer >= _gameConstants.PowerPelletDuration)
            {
                _pelletActive = false;
                EventBus.Raise(new OnPowerPelletEffectFinishedEvent());
            }
        }

        private void DetectBattles()
        {
            if (_pacman == null || _ghosts == null) return;

            _touching.Clear();
            foreach (var ghost in _ghosts)
            {
                if (ghost.IsAlive && IsTouching(ghost))
                    _touching.Add(ghost);
            }

            if (_touching.Count == 0) return;
            
            foreach (var ghost in _touching)
            {
                if (ghost.IsVulnerable) continue;

                FinishBattle(ghost, pacmanWon: false);
                return;
            }

            foreach (var ghost in _touching)
                FinishBattle(ghost, pacmanWon: true);
        }

        private bool IsTouching(Ghost ghost)
        {
            var delta = (Vector2)(_pacman.transform.position - ghost.transform.position);
            return delta.sqrMagnitude <= _hitRadiusSqr;
        }

        private void FinishBattle(Ghost ghost, bool pacmanWon)
        {
            if (pacmanWon)
                EventBus.Raise(new OnCollectPointEvent { PointAmount = _gameConstants.GhostKillScore });

            EventBus.Raise(new OnBattleFinishedEvent
            {
                Pacman = _pacman,
                Ghost = ghost,
                PacmanWon = pacmanWon
            });
        }
    }
}
